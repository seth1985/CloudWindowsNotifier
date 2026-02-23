using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using System.Diagnostics;
using WindowsNotifierCloud.Api.Models.Intune;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Services;

public sealed class IntuneDeploymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IntuneDeploymentOptions _options;
    private readonly StorageOptions _storage;
    private readonly ExportService _exportService;
    private readonly IModuleRepository _modules;
    private readonly IHttpClientFactory _httpClientFactory;

    public IntuneDeploymentService(
        IntuneDeploymentOptions options,
        StorageOptions storage,
        ExportService exportService,
        IModuleRepository modules,
        IHttpClientFactory httpClientFactory)
    {
        _options = options;
        _storage = storage;
        _exportService = exportService;
        _modules = modules;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<IntuneGroupDto>> GetEligibleGroupsAsync(CancellationToken ct = default)
    {
        EnsureGraphConfigured();
        var token = await AcquireGraphTokenAsync(ct);
        using var http = CreateGraphClient(token);
        var prefix = string.IsNullOrWhiteSpace(_options.GroupPrefix) ? "WN-" : _options.GroupPrefix.Trim();
        var escapedPrefix = prefix.Replace("'", "''", StringComparison.Ordinal);
        var url = $"https://graph.microsoft.com/v1.0/groups?$filter=startsWith(displayName,'{escapedPrefix}')&$select=id,displayName&$top=999";
        using var response = await http.GetAsync(url, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Graph group query failed ({(int)response.StatusCode}): {payload}");
        }

        using var doc = JsonDocument.Parse(payload);
        var groups = new List<IntuneGroupDto>();
        if (doc.RootElement.TryGetProperty("value", out var valueEl) && valueEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in valueEl.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                var displayName = item.TryGetProperty("displayName", out var dnEl) ? dnEl.GetString() : null;
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(displayName))
                {
                    continue;
                }

                groups.Add(new IntuneGroupDto
                {
                    Id = id,
                    DisplayName = displayName
                });
            }
        }

        return groups
            .OrderBy(g => g.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IntuneDeployResult> DeployToGroupAsync(Guid moduleDbId, string groupId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(groupId))
        {
            throw new InvalidOperationException("GroupId is required.");
        }

        EnsureGraphConfigured();

        var module = await _modules.GetAsync(moduleDbId, ct);
        if (module == null)
        {
            throw new InvalidOperationException("Module not found.");
        }

        var intuneWinPath = await BuildIntunePackageAsync(moduleDbId, module.ModuleId, ct);
        var packageInfo = ReadIntunePackageMetadata(intuneWinPath);

        var token = await AcquireGraphTokenAsync(ct);
        using var http = CreateGraphClient(token);

        var appId = await CreateWin32AppAsync(http, module, ct);
        var contentVersionId = await CreateContentVersionAsync(http, appId, ct);
        var contentFile = await CreateContentFileAsync(http, appId, contentVersionId, Path.GetFileName(intuneWinPath), packageInfo.UnencryptedSizeBytes, new FileInfo(intuneWinPath).Length, ct);

        await UploadContentAsync(intuneWinPath, contentFile.AzureStorageUri, ct);
        await CommitContentFileAsync(http, appId, contentVersionId, contentFile.FileId, packageInfo, ct);
        await WaitForUploadStateAsync(http, appId, contentVersionId, contentFile.FileId, ct);
        await CommitContentVersionAsync(http, appId, contentVersionId, ct);
        await AssignAppToGroupAsync(http, appId, groupId, ct);

        return new IntuneDeployResult
        {
            IntuneAppId = appId,
            IntuneWinPath = intuneWinPath,
            ContentVersionId = contentVersionId
        };
    }

    private async Task<string> BuildIntunePackageAsync(Guid moduleDbId, string moduleId, CancellationToken ct)
    {
        var export = await _exportService.ExportAsync(moduleDbId, ct);
        if (export == null)
        {
            throw new InvalidOperationException("Module export failed.");
        }

        var deploymentTools = ResolveDeploymentToolsPath();
        if (deploymentTools == null)
        {
            throw new InvalidOperationException("deployment_tools folder not found.");
        }

        var intuneUtil = Path.Combine(deploymentTools, "IntuneWinAppUtil.exe");
        var installScript = Path.Combine(deploymentTools, "install_module_intune.ps1");
        if (!File.Exists(intuneUtil) || !File.Exists(installScript))
        {
            throw new InvalidOperationException("IntuneWinAppUtil.exe or install_module_intune.ps1 not found in deployment_tools.");
        }

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var safeModule = SanitizeName(moduleId);
        var workRoot = Path.Combine(_storage.Root, "modules", "intune", $"{safeModule}-{stamp}");
        var sourceRoot = Path.Combine(workRoot, "source");
        var moduleRoot = Path.Combine(sourceRoot, safeModule);
        var outputRoot = Path.Combine(workRoot, "output");
        Directory.CreateDirectory(moduleRoot);
        Directory.CreateDirectory(outputRoot);

        CopyDirectory(export.Path, moduleRoot, overwrite: true);
        File.Copy(installScript, Path.Combine(sourceRoot, "install_module_intune.ps1"), overwrite: true);

        var psi = new ProcessStartInfo
        {
            FileName = intuneUtil,
            Arguments = $"-c \"{sourceRoot}\" -s \"install_module_intune.ps1\" -o \"{outputRoot}\" -q",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            throw new InvalidOperationException("Could not start IntuneWinAppUtil.");
        }

        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"IntuneWinAppUtil failed ({process.ExitCode}): {stderr}{Environment.NewLine}{stdout}");
        }

        var packagePath = Directory.GetFiles(outputRoot, "*.intunewin", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            throw new InvalidOperationException("Intune package output was not generated.");
        }

        return packagePath;
    }

    private static IntunePackageMetadata ReadIntunePackageMetadata(string intuneWinPath)
    {
        using var archive = ZipFile.OpenRead(intuneWinPath);
        var detectionEntry = archive.Entries.FirstOrDefault(e =>
            string.Equals(e.Name, "Detection.xml", StringComparison.OrdinalIgnoreCase));
        if (detectionEntry == null)
        {
            throw new InvalidOperationException("Detection.xml not found in .intunewin package.");
        }

        using var stream = detectionEntry.Open();
        var doc = XDocument.Load(stream);

        string GetRequired(string localName)
        {
            var value = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == localName)?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Detection.xml missing required value: {localName}");
            }
            return value;
        }

        var unencryptedSizeRaw = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "UnencryptedContentSize")?.Value;
        var unencryptedSize = long.TryParse(unencryptedSizeRaw, out var parsedSize)
            ? parsedSize
            : new FileInfo(intuneWinPath).Length;

        var digestAlgorithm = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "FileDigestAlgorithm")?.Value;
        if (string.IsNullOrWhiteSpace(digestAlgorithm))
        {
            digestAlgorithm = "SHA256";
        }

        var profileIdentifier = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "ProfileIdentifier")?.Value;
        if (string.IsNullOrWhiteSpace(profileIdentifier))
        {
            profileIdentifier = "ProfileVersion1";
        }

        return new IntunePackageMetadata
        {
            EncryptionKey = GetRequired("EncryptionKey"),
            InitializationVector = GetRequired("InitializationVector"),
            Mac = GetRequired("Mac"),
            MacKey = GetRequired("MacKey"),
            FileDigest = GetRequired("FileDigest"),
            FileDigestAlgorithm = digestAlgorithm,
            ProfileIdentifier = profileIdentifier,
            UnencryptedSizeBytes = unencryptedSize
        };
    }

    private async Task<string> CreateWin32AppAsync(HttpClient http, ModuleDefinition module, CancellationToken ct)
    {
        var installCommand = "powershell.exe -ExecutionPolicy Bypass -File .\\install_module_intune.ps1";
        var uninstallCommand = "powershell.exe -ExecutionPolicy Bypass -Command \"exit 0\"";
        var body = new
        {
            @odata_type = "#microsoft.graph.win32LobApp",
            displayName = $"WN - {module.DisplayName}",
            description = module.Description,
            publisher = string.IsNullOrWhiteSpace(_options.Publisher) ? "Windows Notifier" : _options.Publisher,
            isFeatured = false,
            privacyInformationUrl = (string?)null,
            informationUrl = (string?)null,
            owner = "Windows Notifier",
            developer = "Windows Notifier",
            notes = $"Windows Notifier module: {module.ModuleId}",
            installCommandLine = installCommand,
            uninstallCommandLine = uninstallCommand,
            installExperience = new
            {
                @odata_type = "#microsoft.graph.win32LobAppInstallExperience",
                runAsAccount = "user"
            },
            minimumSupportedOperatingSystem = new
            {
                @odata_type = "#microsoft.graph.windowsMinimumOperatingSystem",
                v10_1607 = true
            },
            detectionRules = new object[]
            {
                new
                {
                    @odata_type = "#microsoft.graph.win32LobAppFileSystemDetection",
                    path = $"%LOCALAPPDATA%\\Windows Notifier\\Modules\\{module.ModuleId}",
                    fileOrFolderName = "manifest.json",
                    detectionType = "exists",
                    operatorValue = "notConfigured",
                    check32BitOn64System = false
                }
            }
        };

        using var response = await http.PostAsync(
            "https://graph.microsoft.com/v1.0/deviceAppManagement/mobileApps",
            ToJsonContent(body, rewriteOData: true),
            ct);

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Create win32 app failed ({(int)response.StatusCode}): {payload}");
        }

        using var doc = JsonDocument.Parse(payload);
        var appId = doc.RootElement.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new InvalidOperationException("Graph did not return a mobile app id.");
        }

        return appId;
    }

    private static async Task<string> CreateContentVersionAsync(HttpClient http, string appId, CancellationToken ct)
    {
        using var response = await http.PostAsync(
            $"https://graph.microsoft.com/v1.0/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions",
            new StringContent("{}", Encoding.UTF8, "application/json"),
            ct);

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Create content version failed ({(int)response.StatusCode}): {payload}");
        }

        using var doc = JsonDocument.Parse(payload);
        var contentVersionId = doc.RootElement.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(contentVersionId))
        {
            throw new InvalidOperationException("Graph did not return a content version id.");
        }

        return contentVersionId;
    }

    private static async Task<CreatedContentFile> CreateContentFileAsync(
        HttpClient http,
        string appId,
        string contentVersionId,
        string fileName,
        long unencryptedSize,
        long encryptedSize,
        CancellationToken ct)
    {
        var body = new
        {
            @odata_type = "#microsoft.graph.mobileAppContentFile",
            name = fileName,
            size = unencryptedSize,
            sizeEncrypted = encryptedSize,
            isDependency = false
        };

        using var response = await http.PostAsync(
            $"https://graph.microsoft.com/v1.0/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersionId}/files",
            ToJsonContent(body, rewriteOData: true),
            ct);

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Create content file failed ({(int)response.StatusCode}): {payload}");
        }

        using var doc = JsonDocument.Parse(payload);
        var fileId = doc.RootElement.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var azureStorageUri = doc.RootElement.TryGetProperty("azureStorageUri", out var uriEl) ? uriEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(fileId) || string.IsNullOrWhiteSpace(azureStorageUri))
        {
            throw new InvalidOperationException("Graph did not return upload target metadata.");
        }

        return new CreatedContentFile(fileId, azureStorageUri);
    }

    private static async Task UploadContentAsync(string packagePath, string azureStorageUri, CancellationToken ct)
    {
        using var uploadClient = new HttpClient();
        await using var fs = File.OpenRead(packagePath);
        using var content = new StreamContent(fs);
        content.Headers.Add("x-ms-blob-type", "BlockBlob");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using var response = await uploadClient.PutAsync(azureStorageUri, content, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Azure blob upload failed ({(int)response.StatusCode}): {payload}");
        }
    }

    private static async Task CommitContentFileAsync(
        HttpClient http,
        string appId,
        string contentVersionId,
        string fileId,
        IntunePackageMetadata packageInfo,
        CancellationToken ct)
    {
        var body = new
        {
            fileEncryptionInfo = new
            {
                encryptionKey = packageInfo.EncryptionKey,
                initializationVector = packageInfo.InitializationVector,
                mac = packageInfo.Mac,
                macKey = packageInfo.MacKey,
                profileIdentifier = packageInfo.ProfileIdentifier,
                fileDigest = packageInfo.FileDigest,
                fileDigestAlgorithm = packageInfo.FileDigestAlgorithm
            }
        };

        using var response = await http.PostAsync(
            $"https://graph.microsoft.com/v1.0/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersionId}/files/{fileId}/commit",
            ToJsonContent(body),
            ct);

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Commit content file failed ({(int)response.StatusCode}): {payload}");
        }
    }

    private static async Task WaitForUploadStateAsync(
        HttpClient http,
        string appId,
        string contentVersionId,
        string fileId,
        CancellationToken ct)
    {
        var maxAttempts = 60;
        for (var i = 0; i < maxAttempts; i++)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
            using var response = await http.GetAsync(
                $"https://graph.microsoft.com/v1.0/deviceAppManagement/mobileApps/{appId}/microsoft.graph.win32LobApp/contentVersions/{contentVersionId}/files/{fileId}",
                ct);
            var payload = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            using var doc = JsonDocument.Parse(payload);
            var uploadState = doc.RootElement.TryGetProperty("uploadState", out var usEl)
                ? usEl.GetString()
                : null;
            if (string.Equals(uploadState, "commitFileSuccess", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(uploadState) && uploadState.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Intune content upload failed with state: {uploadState}");
            }
        }

        throw new InvalidOperationException("Timed out waiting for Intune content upload commit.");
    }

    private static async Task CommitContentVersionAsync(HttpClient http, string appId, string contentVersionId, CancellationToken ct)
    {
        var body = new
        {
            committedContentVersion = contentVersionId
        };
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://graph.microsoft.com/v1.0/deviceAppManagement/mobileApps/{appId}")
        {
            Content = ToJsonContent(body)
        };
        using var response = await http.SendAsync(request, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Commit content version failed ({(int)response.StatusCode}): {payload}");
        }
    }

    private static async Task AssignAppToGroupAsync(HttpClient http, string appId, string groupId, CancellationToken ct)
    {
        var body = new
        {
            mobileAppAssignments = new object[]
            {
                new
                {
                    @odata_type = "#microsoft.graph.mobileAppAssignment",
                    intent = "required",
                    target = new
                    {
                        @odata_type = "#microsoft.graph.groupAssignmentTarget",
                        groupId
                    }
                }
            }
        };

        using var response = await http.PostAsync(
            $"https://graph.microsoft.com/v1.0/deviceAppManagement/mobileApps/{appId}/assign",
            ToJsonContent(body, rewriteOData: true),
            ct);

        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Assign app failed ({(int)response.StatusCode}): {payload}");
        }
    }

    private async Task<string> AcquireGraphTokenAsync(CancellationToken ct)
    {
        using var http = _httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["scope"] = "https://graph.microsoft.com/.default",
            ["grant_type"] = "client_credentials"
        });

        var tokenUrl = $"https://login.microsoftonline.com/{_options.TenantId}/oauth2/v2.0/token";
        using var response = await http.PostAsync(tokenUrl, content, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Graph token request failed ({(int)response.StatusCode}): {payload}");
        }

        using var doc = JsonDocument.Parse(payload);
        var accessToken = doc.RootElement.TryGetProperty("access_token", out var atEl) ? atEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("Graph token response missing access_token.");
        }

        return accessToken;
    }

    private HttpClient CreateGraphClient(string accessToken)
    {
        var http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return http;
    }

    private void EnsureGraphConfigured()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("Intune deployment is not enabled.");
        }

        if (string.IsNullOrWhiteSpace(_options.TenantId) ||
            string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException("Intune deployment credentials are not configured.");
        }
    }

    private static StringContent ToJsonContent(object body, bool rewriteOData = false)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        if (rewriteOData)
        {
            json = json.Replace("\"odata_type\":", "\"@odata.type\":", StringComparison.Ordinal);
            json = json.Replace("\"operatorValue\":", "\"operator\":", StringComparison.Ordinal);
        }
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static string SanitizeName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }

    private static string? ResolveDeploymentToolsPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "deployment_tools");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
            dir = dir.Parent;
        }

        return null;
    }

    private static void CopyDirectory(string sourceDir, string destDir, bool overwrite)
    {
        if (Directory.Exists(destDir) && overwrite)
        {
            Directory.Delete(destDir, recursive: true);
        }
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, dest, overwrite);
        }
    }

    private sealed record CreatedContentFile(string FileId, string AzureStorageUri);

    private sealed class IntunePackageMetadata
    {
        public string EncryptionKey { get; set; } = string.Empty;
        public string InitializationVector { get; set; } = string.Empty;
        public string Mac { get; set; } = string.Empty;
        public string MacKey { get; set; } = string.Empty;
        public string ProfileIdentifier { get; set; } = "ProfileVersion1";
        public string FileDigest { get; set; } = string.Empty;
        public string FileDigestAlgorithm { get; set; } = "SHA256";
        public long UnencryptedSizeBytes { get; set; }
    }
}

public sealed class IntuneDeployResult
{
    public string IntuneAppId { get; set; } = string.Empty;
    public string IntuneWinPath { get; set; } = string.Empty;
    public string ContentVersionId { get; set; } = string.Empty;
}
