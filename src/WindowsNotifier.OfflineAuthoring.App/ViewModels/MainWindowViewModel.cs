using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WindowsNotifier.OfflineAuthoring.Core.Abstractions;
using WindowsNotifier.OfflineAuthoring.Core.Models;
using WindowsNotifier.OfflineAuthoring.Core.Services;
using WindowsNotifier.OfflineAuthoring.Infrastructure.Persistence;

namespace WindowsNotifier.OfflineAuthoring.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private const string RecoverySnapshotFileName = "offline-recovery.json";
    public const int MessageMaxLength = 160;
    private const int ReminderHoursMinimum = 1;
    private readonly IOfflineProjectStore _projectStore;
    private readonly IOfflineTemplateStore _templateStore;
    private readonly ModuleValidationService _validationService;
    private readonly ModuleExportService _exportService;

    private string? _currentProjectPath;
    private string? _selectedProjectFile;
    private string _moduleId = string.Empty;
    private OfflineModuleType _selectedType = OfflineModuleType.Standard;
    private string _category = "General";
    private string? _title;
    private string? _message;
    private string? _linkUrl;
    private int? _reminderHours;
    private int? _conditionalIntervalMinutes;
    private string? _conditionalScriptBody;
    private string? _dynamicScriptBody;
    private int _dynamicMaxLength = 240;
    private bool _dynamicTrimWhitespace = true;
    private bool _dynamicFailIfEmpty = true;
    private string? _dynamicFallbackMessage;
    private string? _iconSourcePath;
    private string? _iconFileName;
    private string? _heroSourcePath;
    private string? _heroFileName;
    private string _statusMessage = "Ready.";
    private string _lastValidationSummary = "No validation executed.";
    private string? _lastOutputFolder;
    private bool _isMessageVisible = true;
    private bool _isLinkVisible = true;
    private bool _isReminderVisible = true;
    private bool _isLinkLeftColumn;
    private bool _isLinkRightColumn = true;
    private bool _isReminderLeftColumn;
    private bool _isReminderRightColumn = true;
    private bool _isScheduleLeftColumn;
    private bool _isScheduleRightColumn = true;
    private bool _isIconLeftColumn;
    private bool _isIconRightColumn = true;
    private bool _isIconVisible = true;
    private bool _isHeroVisible;
    private bool _isConditionalVisible;
    private bool _isDynamicVisible;
    private bool _isCoreSettingsVisible;
    private bool _coreEnabled = true;
    private int _corePollingIntervalSeconds = 300;
    private int _coreAutoClearModules = 1;
    private int _coreSoundEnabled = 1;
    private int _coreExitMenuVisible;
    private int _coreStartStopMenuVisible;
    private int _coreHeartbeatSeconds = 15;
    private bool _scheduleEnabled;
    private DateTime _scheduleLocalDate = DateTime.Today;
    private int _scheduleHour;
    private int _scheduleMinute;
    private bool _expiresEnabled;
    private DateTime _expiresLocalDate = DateTime.Today;
    private int _expiresHour;
    private int _expiresMinute;
    private string _scheduleUtcError = string.Empty;
    private string _expiresUtcError = string.Empty;
    private OfflineScriptTemplate? _selectedTemplate;
    private string _newTemplateName = string.Empty;
    private string _templateSearchText = string.Empty;
    private string _templateTypeFilter = "All";
    private readonly List<OfflineScriptTemplate> _allScriptTemplates = new();
    private DateTime _createdUtc = DateTime.UtcNow;
    private string _savedDraftSnapshot = string.Empty;
    private bool _isDirty;
    private string _moduleIdError = string.Empty;
    private string _titleError = string.Empty;
    private string _messageError = string.Empty;
    private string _linkUrlError = string.Empty;
    private string _reminderHoursError = string.Empty;
    private string _conditionalIntervalMinutesError = string.Empty;
    private string _conditionalScriptBodyError = string.Empty;
    private string _dynamicScriptBodyError = string.Empty;
    private string _dynamicMaxLengthError = string.Empty;
    private string _iconSourcePathError = string.Empty;
    private string _heroSourcePathError = string.Empty;
    private string _corePollingIntervalSecondsError = string.Empty;
    private string _coreHeartbeatSecondsError = string.Empty;
    private string _coreEnabledError = string.Empty;
    private string _coreAutoClearModulesError = string.Empty;
    private string _coreSoundEnabledError = string.Empty;
    private string _coreExitMenuVisibleError = string.Empty;
    private string _coreStartStopMenuVisibleError = string.Empty;

    public MainWindowViewModel()
        : this(
            new JsonOfflineProjectStore(),
            new JsonOfflineTemplateStore(),
            new ModuleValidationService(),
            new ModuleExportService(new ManifestGenerationService()))
    {
    }

    public MainWindowViewModel(
        IOfflineProjectStore projectStore,
        IOfflineTemplateStore templateStore,
        ModuleValidationService validationService,
        ModuleExportService exportService)
    {
        _projectStore = projectStore;
        _templateStore = templateStore;
        _validationService = validationService;
        _exportService = exportService;
        PropertyChanged += (_, args) => OnAnyPropertyChanged(args.PropertyName);
        ModuleTypes = Enum.GetValues<OfflineModuleType>();
        RefreshProjectFiles();
        NewProject();
        _ = RefreshTemplatesAsync();
    }

    public string ProjectsRootFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Windows Notifier",
        "OfflineProjects");

    public string ExportRootFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Windows Notifier",
        "OfflineExports");

    public string DeployRootFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Windows Notifier",
        "Modules");

    public string IntunePackageRootFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Windows Notifier",
        "IntunePackages");

    public string TemplatesRootFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Windows Notifier",
        "OfflineTemplates");

    public string RecoveryRootFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "Windows Notifier",
        "OfflineRecovery");

    public ObservableCollection<string> ProjectFiles { get; } = new();

    public OfflineModuleType[] ModuleTypes { get; }
    public int[] TimeHours { get; } = Enumerable.Range(0, 24).ToArray();
    public int[] TimeMinutes { get; } = [0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55];
    public ObservableCollection<OfflineScriptTemplate> ScriptTemplates { get; } = new();
    public string[] TemplateTypeFilters { get; } = ["All", "Conditional", "Dynamic"];
    public bool IsDirty
    {
        get => _isDirty;
        private set => SetProperty(ref _isDirty, value);
    }

    public string ModuleIdError
    {
        get => _moduleIdError;
        private set => SetProperty(ref _moduleIdError, value);
    }

    public string TitleError
    {
        get => _titleError;
        private set => SetProperty(ref _titleError, value);
    }

    public string MessageError
    {
        get => _messageError;
        private set => SetProperty(ref _messageError, value);
    }

    public string LinkUrlError
    {
        get => _linkUrlError;
        private set => SetProperty(ref _linkUrlError, value);
    }

    public string ReminderHoursError
    {
        get => _reminderHoursError;
        private set => SetProperty(ref _reminderHoursError, value);
    }

    public string ConditionalIntervalMinutesError
    {
        get => _conditionalIntervalMinutesError;
        private set => SetProperty(ref _conditionalIntervalMinutesError, value);
    }

    public string ConditionalScriptBodyError
    {
        get => _conditionalScriptBodyError;
        private set => SetProperty(ref _conditionalScriptBodyError, value);
    }

    public string DynamicScriptBodyError
    {
        get => _dynamicScriptBodyError;
        private set => SetProperty(ref _dynamicScriptBodyError, value);
    }

    public string DynamicMaxLengthError
    {
        get => _dynamicMaxLengthError;
        private set => SetProperty(ref _dynamicMaxLengthError, value);
    }

    public string IconSourcePathError
    {
        get => _iconSourcePathError;
        private set => SetProperty(ref _iconSourcePathError, value);
    }

    public string HeroSourcePathError
    {
        get => _heroSourcePathError;
        private set => SetProperty(ref _heroSourcePathError, value);
    }

    public string CorePollingIntervalSecondsError
    {
        get => _corePollingIntervalSecondsError;
        private set => SetProperty(ref _corePollingIntervalSecondsError, value);
    }

    public string CoreHeartbeatSecondsError
    {
        get => _coreHeartbeatSecondsError;
        private set => SetProperty(ref _coreHeartbeatSecondsError, value);
    }

    public string CoreEnabledError
    {
        get => _coreEnabledError;
        private set => SetProperty(ref _coreEnabledError, value);
    }

    public string CoreAutoClearModulesError
    {
        get => _coreAutoClearModulesError;
        private set => SetProperty(ref _coreAutoClearModulesError, value);
    }

    public string CoreSoundEnabledError
    {
        get => _coreSoundEnabledError;
        private set => SetProperty(ref _coreSoundEnabledError, value);
    }

    public string CoreExitMenuVisibleError
    {
        get => _coreExitMenuVisibleError;
        private set => SetProperty(ref _coreExitMenuVisibleError, value);
    }

    public string CoreStartStopMenuVisibleError
    {
        get => _coreStartStopMenuVisibleError;
        private set => SetProperty(ref _coreStartStopMenuVisibleError, value);
    }

    public string ScheduleUtcError
    {
        get => _scheduleUtcError;
        private set => SetProperty(ref _scheduleUtcError, value);
    }

    public string ExpiresUtcError
    {
        get => _expiresUtcError;
        private set => SetProperty(ref _expiresUtcError, value);
    }

    public string? CurrentProjectPath
    {
        get => _currentProjectPath;
        private set => SetProperty(ref _currentProjectPath, value);
    }

    public string? SelectedProjectFile
    {
        get => _selectedProjectFile;
        set => SetProperty(ref _selectedProjectFile, value);
    }

    public string ModuleId
    {
        get => _moduleId;
        set => SetProperty(ref _moduleId, value);
    }

    public OfflineModuleType SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value))
            {
                UpdateTypeVisibility();
            }
        }
    }

    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string? Message
    {
        get => _message;
        set
        {
            var normalized = value ?? string.Empty;
            if (normalized.Length > MessageMaxLength)
            {
                normalized = normalized[..MessageMaxLength];
            }

            if (SetProperty(ref _message, normalized))
            {
                RaisePropertyChanged(nameof(MessageCharacterCount));
            }
        }
    }

    public string MessageCharacterCount => $"{Message?.Length ?? 0}/{MessageMaxLength}";

    public string? LinkUrl
    {
        get => _linkUrl;
        set => SetProperty(ref _linkUrl, value);
    }

    public int? ReminderHours
    {
        get => _reminderHours;
        set => SetProperty(ref _reminderHours, value);
    }

    public bool ScheduleEnabled
    {
        get => _scheduleEnabled;
        set => SetProperty(ref _scheduleEnabled, value);
    }

    public DateTime ScheduleLocalDate
    {
        get => _scheduleLocalDate;
        set => SetProperty(ref _scheduleLocalDate, value);
    }

    public int ScheduleHour
    {
        get => _scheduleHour;
        set => SetProperty(ref _scheduleHour, value);
    }

    public int ScheduleMinute
    {
        get => _scheduleMinute;
        set => SetProperty(ref _scheduleMinute, value);
    }

    public bool ExpiresEnabled
    {
        get => _expiresEnabled;
        set => SetProperty(ref _expiresEnabled, value);
    }

    public DateTime ExpiresLocalDate
    {
        get => _expiresLocalDate;
        set => SetProperty(ref _expiresLocalDate, value);
    }

    public int ExpiresHour
    {
        get => _expiresHour;
        set => SetProperty(ref _expiresHour, value);
    }

    public int ExpiresMinute
    {
        get => _expiresMinute;
        set => SetProperty(ref _expiresMinute, value);
    }

    public int? ConditionalIntervalMinutes
    {
        get => _conditionalIntervalMinutes;
        set => SetProperty(ref _conditionalIntervalMinutes, value);
    }

    public string? ConditionalScriptBody
    {
        get => _conditionalScriptBody;
        set => SetProperty(ref _conditionalScriptBody, value);
    }

    public string? DynamicScriptBody
    {
        get => _dynamicScriptBody;
        set => SetProperty(ref _dynamicScriptBody, value);
    }

    public int DynamicMaxLength
    {
        get => _dynamicMaxLength;
        set => SetProperty(ref _dynamicMaxLength, value);
    }

    public bool DynamicTrimWhitespace
    {
        get => _dynamicTrimWhitespace;
        set => SetProperty(ref _dynamicTrimWhitespace, value);
    }

    public bool DynamicFailIfEmpty
    {
        get => _dynamicFailIfEmpty;
        set => SetProperty(ref _dynamicFailIfEmpty, value);
    }

    public string? DynamicFallbackMessage
    {
        get => _dynamicFallbackMessage;
        set => SetProperty(ref _dynamicFallbackMessage, value);
    }

    public string? IconSourcePath
    {
        get => _iconSourcePath;
        set => SetProperty(ref _iconSourcePath, value);
    }

    public string? IconFileName
    {
        get => _iconFileName;
        set => SetProperty(ref _iconFileName, value);
    }

    public string? HeroSourcePath
    {
        get => _heroSourcePath;
        set => SetProperty(ref _heroSourcePath, value);
    }

    public string? HeroFileName
    {
        get => _heroFileName;
        set => SetProperty(ref _heroFileName, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string LastValidationSummary
    {
        get => _lastValidationSummary;
        private set => SetProperty(ref _lastValidationSummary, value);
    }

    public string? LastOutputFolder
    {
        get => _lastOutputFolder;
        private set => SetProperty(ref _lastOutputFolder, value);
    }

    public bool IsMessageVisible
    {
        get => _isMessageVisible;
        private set => SetProperty(ref _isMessageVisible, value);
    }

    public bool IsLinkVisible
    {
        get => _isLinkVisible;
        private set => SetProperty(ref _isLinkVisible, value);
    }

    public bool IsReminderVisible
    {
        get => _isReminderVisible;
        private set => SetProperty(ref _isReminderVisible, value);
    }

    public bool IsLinkLeftColumn
    {
        get => _isLinkLeftColumn;
        private set => SetProperty(ref _isLinkLeftColumn, value);
    }

    public bool IsLinkRightColumn
    {
        get => _isLinkRightColumn;
        private set => SetProperty(ref _isLinkRightColumn, value);
    }

    public bool IsReminderLeftColumn
    {
        get => _isReminderLeftColumn;
        private set => SetProperty(ref _isReminderLeftColumn, value);
    }

    public bool IsReminderRightColumn
    {
        get => _isReminderRightColumn;
        private set => SetProperty(ref _isReminderRightColumn, value);
    }

    public bool IsScheduleLeftColumn
    {
        get => _isScheduleLeftColumn;
        private set => SetProperty(ref _isScheduleLeftColumn, value);
    }

    public bool IsScheduleRightColumn
    {
        get => _isScheduleRightColumn;
        private set => SetProperty(ref _isScheduleRightColumn, value);
    }

    public bool IsIconLeftColumn
    {
        get => _isIconLeftColumn;
        private set => SetProperty(ref _isIconLeftColumn, value);
    }

    public bool IsIconRightColumn
    {
        get => _isIconRightColumn;
        private set => SetProperty(ref _isIconRightColumn, value);
    }

    public bool IsIconVisible
    {
        get => _isIconVisible;
        private set => SetProperty(ref _isIconVisible, value);
    }

    public bool IsHeroVisible
    {
        get => _isHeroVisible;
        private set => SetProperty(ref _isHeroVisible, value);
    }

    public bool IsConditionalVisible
    {
        get => _isConditionalVisible;
        private set => SetProperty(ref _isConditionalVisible, value);
    }

    public bool IsDynamicVisible
    {
        get => _isDynamicVisible;
        private set => SetProperty(ref _isDynamicVisible, value);
    }

    public bool IsCoreSettingsVisible
    {
        get => _isCoreSettingsVisible;
        private set => SetProperty(ref _isCoreSettingsVisible, value);
    }

    public bool CoreEnabled
    {
        get => _coreEnabled;
        set => SetProperty(ref _coreEnabled, value);
    }

    public int CorePollingIntervalSeconds
    {
        get => _corePollingIntervalSeconds;
        set => SetProperty(ref _corePollingIntervalSeconds, value);
    }

    public int CoreAutoClearModules
    {
        get => _coreAutoClearModules;
        set => SetProperty(ref _coreAutoClearModules, value);
    }

    public int CoreSoundEnabled
    {
        get => _coreSoundEnabled;
        set => SetProperty(ref _coreSoundEnabled, value);
    }

    public int CoreExitMenuVisible
    {
        get => _coreExitMenuVisible;
        set => SetProperty(ref _coreExitMenuVisible, value);
    }

    public int CoreStartStopMenuVisible
    {
        get => _coreStartStopMenuVisible;
        set => SetProperty(ref _coreStartStopMenuVisible, value);
    }

    public int CoreHeartbeatSeconds
    {
        get => _coreHeartbeatSeconds;
        set => SetProperty(ref _coreHeartbeatSeconds, value);
    }

    public OfflineScriptTemplate? SelectedTemplate
    {
        get => _selectedTemplate;
        set => SetProperty(ref _selectedTemplate, value);
    }

    public string NewTemplateName
    {
        get => _newTemplateName;
        set => SetProperty(ref _newTemplateName, value);
    }

    public string TemplateSearchText
    {
        get => _templateSearchText;
        set
        {
            if (SetProperty(ref _templateSearchText, value))
            {
                RefreshTemplateView();
            }
        }
    }

    public string TemplateTypeFilter
    {
        get => _templateTypeFilter;
        set
        {
            if (SetProperty(ref _templateTypeFilter, value))
            {
                RefreshTemplateView();
            }
        }
    }

    public void NewProject()
    {
        CurrentProjectPath = null;
        _createdUtc = DateTime.UtcNow;
        ModuleId = $"module-{DateTime.UtcNow:HHmmss}-offline";
        SelectedType = OfflineModuleType.Standard;
        Category = "General";
        Title = string.Empty;
        Message = string.Empty;
        LinkUrl = string.Empty;
        ReminderHours = ReminderHoursMinimum;
        ScheduleEnabled = false;
        var scheduleLocal = DateTime.Now.AddMinutes(15);
        ScheduleLocalDate = scheduleLocal.Date;
        ScheduleHour = scheduleLocal.Hour;
        ScheduleMinute = RoundToMinuteBucket(scheduleLocal.Minute);
        ExpiresEnabled = false;
        var expiresLocal = scheduleLocal.AddDays(7);
        ExpiresLocalDate = expiresLocal.Date;
        ExpiresHour = expiresLocal.Hour;
        ExpiresMinute = RoundToMinuteBucket(expiresLocal.Minute);
        ConditionalIntervalMinutes = null;
        ConditionalScriptBody = string.Empty;
        DynamicScriptBody = string.Empty;
        DynamicMaxLength = 240;
        DynamicTrimWhitespace = true;
        DynamicFailIfEmpty = true;
        DynamicFallbackMessage = string.Empty;
        IconSourcePath = string.Empty;
        IconFileName = string.Empty;
        HeroSourcePath = string.Empty;
        HeroFileName = "hero.png";
        CoreEnabled = true;
        CorePollingIntervalSeconds = 300;
        CoreAutoClearModules = 1;
        CoreSoundEnabled = 1;
        CoreExitMenuVisible = 0;
        CoreStartStopMenuVisible = 0;
        CoreHeartbeatSeconds = 15;
        UpdateTypeVisibility();
        ClearFieldErrors();
        MarkCleanFromCurrentDraft();
        LastValidationSummary = "No validation executed.";
        StatusMessage = "New project initialized.";
    }

    public async Task RefreshTemplatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = await _templateStore.LoadAsync(cancellationToken);
            var filtered = templates
                .Where(t => !string.IsNullOrWhiteSpace(t.Name) && !string.IsNullOrWhiteSpace(t.ScriptBody))
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _allScriptTemplates.Clear();
            _allScriptTemplates.AddRange(filtered);
            RefreshTemplateView();

            if (SelectedTemplate != null)
            {
                SelectedTemplate = ScriptTemplates.FirstOrDefault(
                    x => string.Equals(x.Name, SelectedTemplate.Name, StringComparison.OrdinalIgnoreCase) &&
                         x.Type == SelectedTemplate.Type);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Template refresh failed: {ex.Message}";
        }
    }

    public async Task LoadProjectAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var draft = await _projectStore.LoadAsync(filePath, cancellationToken);
            ApplyDraft(draft);
            CurrentProjectPath = filePath;
            ClearFieldErrors();
            MarkCleanFromCurrentDraft();
            StatusMessage = $"Loaded project: {Path.GetFileName(filePath)}";
            RefreshProjectFiles();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Load failed: {ex.Message}";
        }
    }

    public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(CurrentProjectPath))
        {
            StatusMessage = "No file selected. Use Save As.";
            return false;
        }

        return await SaveAsAsync(CurrentProjectPath, cancellationToken);
    }

    public async Task<bool> SaveAsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var draft = ToDraft();
            await _projectStore.SaveAsync(filePath, draft, cancellationToken);
            CurrentProjectPath = filePath;
            MarkCleanFromCurrentDraft();
            StatusMessage = $"Saved project: {Path.GetFileName(filePath)}";
            RefreshProjectFiles();
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
            return false;
        }
    }

    public async Task SaveRecoverySnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (!IsDirty)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(RecoveryRootFolder);
            var payload = new RecoverySnapshot
            {
                SavedAtUtc = DateTime.UtcNow,
                ProjectPath = CurrentProjectPath,
                Draft = ToDraft()
            };
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            var recoveryPath = GetRecoverySnapshotPath();
            await File.WriteAllTextAsync(recoveryPath, json, cancellationToken);
        }
        catch
        {
            // Best effort only; recovery failures should not break the editor flow.
        }
    }

    public async Task<RecoverySnapshotInfo?> GetRecoverySnapshotInfoAsync(CancellationToken cancellationToken = default)
    {
        var path = GetRecoverySnapshotPath();
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var payload = JsonSerializer.Deserialize<RecoverySnapshot>(json);
            if (payload?.Draft == null)
            {
                return null;
            }

            return new RecoverySnapshotInfo(payload.SavedAtUtc, payload.ProjectPath);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RestoreRecoverySnapshotAsync(CancellationToken cancellationToken = default)
    {
        var path = GetRecoverySnapshotPath();
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var payload = JsonSerializer.Deserialize<RecoverySnapshot>(json);
            if (payload?.Draft == null)
            {
                return false;
            }

            ApplyDraft(payload.Draft);
            CurrentProjectPath = payload.ProjectPath;
            _savedDraftSnapshot = string.Empty;
            UpdateDirtyFlag();
            StatusMessage = "Recovered unsaved draft from local snapshot.";
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task ClearRecoverySnapshotAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var path = GetRecoverySnapshotPath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // no-op, best effort cleanup
        }

        return Task.CompletedTask;
    }

    public Task CleanupStaleRecoverySnapshotsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        try
        {
            var path = GetRecoverySnapshotPath();
            if (!File.Exists(path))
            {
                return Task.CompletedTask;
            }

            var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(path);
            if (age > maxAge)
            {
                File.Delete(path);
            }
        }
        catch
        {
            // no-op cleanup
        }

        return Task.CompletedTask;
    }

    public bool ValidateCurrent()
    {
        var draft = ToDraft();
        var result = _validationService.Validate(draft);
        ApplyValidationIssues(result);
        if (result.IsValid)
        {
            LastValidationSummary = "Validation passed.";
            StatusMessage = "Validation passed.";
            return true;
        }

        LastValidationSummary = string.Join(Environment.NewLine, result.Errors);
        StatusMessage = $"Validation failed ({result.Errors.Count} issue(s)).";
        return false;
    }

    public async Task<bool> ExportAsync(CancellationToken cancellationToken = default)
    {
        if (!ValidateCurrent())
        {
            return false;
        }

        try
        {
            Directory.CreateDirectory(ExportRootFolder);
            var exportedPath = await _exportService.ExportModuleAsync(ToDraft(), ExportRootFolder, cancellationToken);
            StatusMessage = $"Exported to: {exportedPath}";
            LastOutputFolder = exportedPath;
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            return false;
        }
    }

    public async Task<bool> DeployAsync(CancellationToken cancellationToken = default)
    {
        if (!ValidateCurrent())
        {
            return false;
        }

        try
        {
            Directory.CreateDirectory(DeployRootFolder);
            var deployedPath = await _exportService.ExportModuleAsync(ToDraft(), DeployRootFolder, cancellationToken);
            StatusMessage = $"Deployed to: {deployedPath}";
            LastOutputFolder = deployedPath;
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Deploy failed: {ex.Message}";
            return false;
        }
    }

    public async Task<bool> ExportIntunePackageAsync(CancellationToken cancellationToken = default)
    {
        if (!ValidateCurrent())
        {
            return false;
        }

        var deploymentToolsPath = ResolveDeploymentToolsPath();
        if (deploymentToolsPath == null)
        {
            StatusMessage = "Intune export failed: deployment_tools folder not found.";
            return false;
        }

        var intuneUtil = Path.Combine(deploymentToolsPath, "IntuneWinAppUtil.exe");
        var installScript = Path.Combine(deploymentToolsPath, "install_module_intune.ps1");
        if (!File.Exists(intuneUtil) || !File.Exists(installScript))
        {
            StatusMessage = "Intune export failed: IntuneWinAppUtil.exe or install_module_intune.ps1 missing.";
            return false;
        }

        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var packageName = SanitizeName(ModuleId);
        var workRoot = Path.Combine(IntunePackageRootFolder, $"{packageName}-{stamp}");
        var sourceRoot = Path.Combine(workRoot, "source");
        var outputRoot = Path.Combine(workRoot, "output");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(outputRoot);

        try
        {
            var moduleOutput = await _exportService.ExportModuleAsync(ToDraft(), sourceRoot, cancellationToken);
            _ = moduleOutput;
            File.Copy(installScript, Path.Combine(sourceRoot, "install_module_intune.ps1"), overwrite: true);

            var psi = new ProcessStartInfo
            {
                FileName = intuneUtil,
                Arguments = $"-c \"{sourceRoot}\" -s \"install_module_intune.ps1\" -o \"{outputRoot}\" -q",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                StatusMessage = "Intune export failed: could not start IntuneWinAppUtil.";
                return false;
            }

            var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                StatusMessage = $"Intune export failed (exit code {process.ExitCode}). {stderr}";
                return false;
            }

            var instructionsPath = Path.Combine(outputRoot, "INTUNE_INSTALL_INSTRUCTIONS.txt");
            var instructions = BuildIntuneInstructions(packageName);
            await File.WriteAllTextAsync(instructionsPath, instructions, cancellationToken);

            StatusMessage = $"Intune package exported to: {outputRoot}";
            LastOutputFolder = outputRoot;
            LastValidationSummary = string.IsNullOrWhiteSpace(stdout)
                ? LastValidationSummary
                : $"Packaging output:{Environment.NewLine}{stdout.Trim()}";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Intune export failed: {ex.Message}";
            return false;
        }
    }

    public async Task<bool> ImportModuleFolderAsync(string moduleFolderPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var manifestPath = Path.Combine(moduleFolderPath, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                StatusMessage = "Import failed: manifest.json not found in selected folder.";
                return false;
            }

            var json = await File.ReadAllTextAsync(manifestPath, cancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            ModuleId = root.TryGetProperty("id", out var idEl) ? (idEl.GetString() ?? ModuleId) : ModuleId;
            SelectedType = root.TryGetProperty("type", out var tEl) ? ParseType(tEl.GetString()) : OfflineModuleType.Standard;
            Category = root.TryGetProperty("category", out var cEl) ? (cEl.GetString() ?? "General") : "General";
            Title = root.TryGetProperty("title", out var titleEl) ? (titleEl.GetString() ?? string.Empty) : string.Empty;
            Message = root.TryGetProperty("message", out var msgEl) ? (msgEl.GetString() ?? string.Empty) : string.Empty;
            ScheduleEnabled = false;
            ExpiresEnabled = false;
            if (root.TryGetProperty("schedule_utc", out var scheduleEl) &&
                scheduleEl.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(scheduleEl.GetString(), out var scheduleUtc))
            {
                SetScheduleLocalFromUtc(scheduleUtc);
            }
            if (root.TryGetProperty("expires_utc", out var expiresEl) &&
                expiresEl.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(expiresEl.GetString(), out var expiresUtc))
            {
                SetExpiresLocalFromUtc(expiresUtc);
            }

            LinkUrl = string.Empty;
            IconFileName = string.Empty;
            HeroFileName = "hero.png";
            if (root.TryGetProperty("media", out var mediaEl))
            {
                if (mediaEl.TryGetProperty("link", out var linkEl))
                {
                    LinkUrl = linkEl.GetString() ?? string.Empty;
                }

                if (mediaEl.TryGetProperty("icon", out var iconEl))
                {
                    IconFileName = iconEl.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(IconFileName))
                    {
                        var iconPath = Path.Combine(moduleFolderPath, IconFileName);
                        IconSourcePath = File.Exists(iconPath) ? iconPath : string.Empty;
                    }
                }

                if (mediaEl.TryGetProperty("hero", out var heroEl))
                {
                    HeroFileName = heroEl.GetString() ?? "hero.png";
                    if (!string.IsNullOrWhiteSpace(HeroFileName))
                    {
                        var heroPath = Path.Combine(moduleFolderPath, HeroFileName);
                        HeroSourcePath = File.Exists(heroPath) ? heroPath : string.Empty;
                    }
                }
            }

            ReminderHours = null;
            ConditionalIntervalMinutes = null;
            if (root.TryGetProperty("behavior", out var behaviorEl))
            {
                if (behaviorEl.TryGetProperty("reminder_hours", out var remEl) && remEl.TryGetInt32(out var rem))
                {
                    ReminderHours = rem;
                }
                if (behaviorEl.TryGetProperty("conditional_interval_minutes", out var ciEl) && ciEl.TryGetInt32(out var ci))
                {
                    ConditionalIntervalMinutes = ci;
                }
            }

            var conditionalPath = Path.Combine(moduleFolderPath, "conditional.ps1");
            ConditionalScriptBody = File.Exists(conditionalPath)
                ? await File.ReadAllTextAsync(conditionalPath, cancellationToken)
                : string.Empty;
            if (root.TryGetProperty("behavior", out var behaviorWithScript) &&
                behaviorWithScript.TryGetProperty("conditional_script", out var condScriptEl))
            {
                var conditionalScriptName = condScriptEl.GetString();
                if (!string.IsNullOrWhiteSpace(conditionalScriptName))
                {
                    var conditionalScriptPath = Path.Combine(moduleFolderPath, conditionalScriptName);
                    if (File.Exists(conditionalScriptPath))
                    {
                        ConditionalScriptBody = await File.ReadAllTextAsync(conditionalScriptPath, cancellationToken);
                    }
                }
            }

            DynamicScriptBody = string.Empty;
            DynamicMaxLength = 240;
            DynamicTrimWhitespace = true;
            DynamicFailIfEmpty = true;
            DynamicFallbackMessage = string.Empty;
            var dynamicPath = Path.Combine(moduleFolderPath, "dynamic.ps1");
            if (File.Exists(dynamicPath))
            {
                DynamicScriptBody = await File.ReadAllTextAsync(dynamicPath, cancellationToken);
            }
            if (root.TryGetProperty("dynamic", out var dynamicEl))
            {
                if (dynamicEl.TryGetProperty("script", out var dynScriptEl))
                {
                    var dynamicScriptName = dynScriptEl.GetString();
                    if (!string.IsNullOrWhiteSpace(dynamicScriptName))
                    {
                        var dynamicScriptPath = Path.Combine(moduleFolderPath, dynamicScriptName);
                        if (File.Exists(dynamicScriptPath))
                        {
                            DynamicScriptBody = await File.ReadAllTextAsync(dynamicScriptPath, cancellationToken);
                        }
                    }
                }
                if (dynamicEl.TryGetProperty("max_length", out var maxEl) && maxEl.TryGetInt32(out var max))
                {
                    DynamicMaxLength = max;
                }
                if (dynamicEl.TryGetProperty("options", out var optsEl))
                {
                    if (optsEl.TryGetProperty("trim_whitespace", out var twEl) &&
                        (twEl.ValueKind is JsonValueKind.True or JsonValueKind.False))
                    {
                        DynamicTrimWhitespace = twEl.GetBoolean();
                    }
                    if (optsEl.TryGetProperty("fail_if_empty", out var fieEl) &&
                        (fieEl.ValueKind is JsonValueKind.True or JsonValueKind.False))
                    {
                        DynamicFailIfEmpty = fieEl.GetBoolean();
                    }
                    if (optsEl.TryGetProperty("fallback_message", out var fbEl))
                    {
                        DynamicFallbackMessage = fbEl.GetString() ?? string.Empty;
                    }
                }
            }

            if (root.TryGetProperty("core_settings", out var coreEl))
            {
                CoreEnabled = GetInt(coreEl, "enabled", 1) != 0;
                CorePollingIntervalSeconds = GetInt(coreEl, "polling_interval_seconds", 300);
                CoreAutoClearModules = GetInt(coreEl, "auto_clear_modules", 1);
                CoreSoundEnabled = GetInt(coreEl, "sound_enabled", 1);
                CoreExitMenuVisible = GetInt(coreEl, "exit_menu_visible", 0);
                CoreStartStopMenuVisible = GetInt(coreEl, "start_stop_menu_visible", 0);
                CoreHeartbeatSeconds = GetInt(coreEl, "heartbeat_seconds", 15);
            }

            CurrentProjectPath = null;
            _createdUtc = DateTime.UtcNow;
            UpdateTypeVisibility();
            ClearFieldErrors();
            MarkCleanFromCurrentDraft();
            LastValidationSummary = "Imported from module folder.";
            StatusMessage = $"Imported module folder: {moduleFolderPath}";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import failed: {ex.Message}";
            return false;
        }
    }

    public void RefreshProjectFiles()
    {
        Directory.CreateDirectory(ProjectsRootFolder);

        var files = Directory.GetFiles(ProjectsRootFolder, "*.wnproj", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToList();

        ProjectFiles.Clear();
        foreach (var file in files)
        {
            ProjectFiles.Add(file);
        }

        if (!string.IsNullOrWhiteSpace(SelectedProjectFile) && !ProjectFiles.Contains(SelectedProjectFile))
        {
            SelectedProjectFile = null;
        }
    }

    private OfflineModuleDraft ToDraft()
    {
        DateTime? scheduleUtc = ScheduleEnabled
            ? BuildUtcFromLocal(ScheduleLocalDate, ScheduleHour, ScheduleMinute)
            : null;
        DateTime? expiresUtc = ExpiresEnabled
            ? BuildUtcFromLocal(ExpiresLocalDate, ExpiresHour, ExpiresMinute)
            : null;

        return new OfflineModuleDraft
        {
            ModuleId = ModuleId.Trim(),
            Type = SelectedType,
            Category = string.IsNullOrWhiteSpace(Category) ? "General" : Category.Trim(),
            Title = string.IsNullOrWhiteSpace(Title) ? null : Title.Trim(),
            Message = string.IsNullOrWhiteSpace(Message) ? null : Message.Trim(),
            LinkUrl = string.IsNullOrWhiteSpace(LinkUrl) ? null : LinkUrl.Trim(),
            ScheduleUtc = scheduleUtc,
            ExpiresUtc = expiresUtc,
            ReminderHours = ReminderHours,
            ConditionalIntervalMinutes = ConditionalIntervalMinutes,
            ConditionalScriptBody = string.IsNullOrWhiteSpace(ConditionalScriptBody) ? null : ConditionalScriptBody,
            DynamicScriptBody = string.IsNullOrWhiteSpace(DynamicScriptBody) ? null : DynamicScriptBody,
            DynamicMaxLength = DynamicMaxLength,
            DynamicTrimWhitespace = DynamicTrimWhitespace,
            DynamicFailIfEmpty = DynamicFailIfEmpty,
            DynamicFallbackMessage = string.IsNullOrWhiteSpace(DynamicFallbackMessage) ? null : DynamicFallbackMessage,
            IconSourcePath = string.IsNullOrWhiteSpace(IconSourcePath) ? null : IconSourcePath.Trim(),
            IconFileName = string.IsNullOrWhiteSpace(IconFileName) ? null : IconFileName.Trim(),
            HeroSourcePath = string.IsNullOrWhiteSpace(HeroSourcePath) ? null : HeroSourcePath.Trim(),
            HeroFileName = string.IsNullOrWhiteSpace(HeroFileName) ? "hero.png" : HeroFileName.Trim(),
            CoreSettings = SelectedType == OfflineModuleType.CoreSettings
                ? new CoreSettingsDraft
                {
                    Enabled = CoreEnabled ? 1 : 0,
                    PollingIntervalSeconds = CorePollingIntervalSeconds,
                    AutoClearModules = CoreAutoClearModules,
                    SoundEnabled = CoreSoundEnabled,
                    ExitMenuVisible = CoreExitMenuVisible,
                    StartStopMenuVisible = CoreStartStopMenuVisible,
                    HeartbeatSeconds = CoreHeartbeatSeconds
                }
                : null,
            CreatedUtc = _createdUtc
        };
    }

    private void ApplyDraft(OfflineModuleDraft draft)
    {
        _createdUtc = draft.CreatedUtc == default ? DateTime.UtcNow : draft.CreatedUtc;
        ModuleId = draft.ModuleId;
        SelectedType = draft.Type;
        Category = string.IsNullOrWhiteSpace(draft.Category) ? "General" : draft.Category;
        Title = draft.Title ?? string.Empty;
        Message = draft.Message ?? string.Empty;
        LinkUrl = draft.LinkUrl ?? string.Empty;
        ReminderHours = draft.ReminderHours ?? ReminderHoursMinimum;
        if (draft.ScheduleUtc.HasValue)
        {
            SetScheduleLocalFromUtc(draft.ScheduleUtc.Value);
        }
        else
        {
            ScheduleEnabled = false;
        }
        if (draft.ExpiresUtc.HasValue)
        {
            SetExpiresLocalFromUtc(draft.ExpiresUtc.Value);
        }
        else
        {
            ExpiresEnabled = false;
        }
        ConditionalIntervalMinutes = draft.ConditionalIntervalMinutes;
        ConditionalScriptBody = draft.ConditionalScriptBody ?? string.Empty;
        DynamicScriptBody = draft.DynamicScriptBody ?? string.Empty;
        DynamicMaxLength = draft.DynamicMaxLength <= 0 ? 240 : draft.DynamicMaxLength;
        DynamicTrimWhitespace = draft.DynamicTrimWhitespace;
        DynamicFailIfEmpty = draft.DynamicFailIfEmpty;
        DynamicFallbackMessage = draft.DynamicFallbackMessage ?? string.Empty;
        IconSourcePath = draft.IconSourcePath ?? string.Empty;
        IconFileName = draft.IconFileName ?? string.Empty;
        HeroSourcePath = draft.HeroSourcePath ?? string.Empty;
        HeroFileName = draft.HeroFileName ?? "hero.png";
        CoreEnabled = (draft.CoreSettings?.Enabled ?? 1) != 0;
        CorePollingIntervalSeconds = draft.CoreSettings?.PollingIntervalSeconds ?? 300;
        CoreAutoClearModules = draft.CoreSettings?.AutoClearModules ?? 1;
        CoreSoundEnabled = draft.CoreSettings?.SoundEnabled ?? 1;
        CoreExitMenuVisible = draft.CoreSettings?.ExitMenuVisible ?? 0;
        CoreStartStopMenuVisible = draft.CoreSettings?.StartStopMenuVisible ?? 0;
        CoreHeartbeatSeconds = draft.CoreSettings?.HeartbeatSeconds ?? 15;
        UpdateTypeVisibility();
    }

    private void UpdateTypeVisibility()
    {
        IsConditionalVisible = SelectedType == OfflineModuleType.Conditional;
        IsDynamicVisible = SelectedType == OfflineModuleType.Dynamic;
        IsHeroVisible = SelectedType == OfflineModuleType.Hero;
        IsIconVisible = SelectedType is OfflineModuleType.Standard or OfflineModuleType.Conditional or OfflineModuleType.Dynamic;
        IsMessageVisible = SelectedType is OfflineModuleType.Standard or OfflineModuleType.Conditional or OfflineModuleType.Hero;
        IsLinkVisible = SelectedType != OfflineModuleType.CoreSettings;
        IsReminderVisible = SelectedType != OfflineModuleType.CoreSettings;
        var leftLinkReminder = SelectedType is OfflineModuleType.Dynamic or OfflineModuleType.Conditional;
        IsLinkLeftColumn = IsLinkVisible && leftLinkReminder;
        IsLinkRightColumn = IsLinkVisible && !leftLinkReminder;
        IsReminderLeftColumn = IsReminderVisible && leftLinkReminder;
        IsReminderRightColumn = IsReminderVisible && !leftLinkReminder;
        IsScheduleLeftColumn = IsReminderVisible && SelectedType == OfflineModuleType.Dynamic;
        IsScheduleRightColumn = IsReminderVisible && SelectedType != OfflineModuleType.Dynamic;
        IsIconLeftColumn = IsIconVisible && SelectedType == OfflineModuleType.Dynamic;
        IsIconRightColumn = IsIconVisible && SelectedType != OfflineModuleType.Dynamic;
        IsCoreSettingsVisible = SelectedType == OfflineModuleType.CoreSettings;
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return $"module-{Guid.NewGuid():N}";
        }

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

    private static string BuildIntuneInstructions(string moduleName)
    {
        return
$@"Intune Win32 App Install Instructions
=================================

Package purpose:
- Deploy module '{moduleName}' into %LOCALAPPDATA%\Windows Notifier\Modules and set registry state to Pending.

Install command:
powershell.exe -ExecutionPolicy Bypass -File .\install_module_intune.ps1

Uninstall command:
powershell.exe -ExecutionPolicy Bypass -Command ""Write-Host 'No uninstall action configured for module package.'; exit 0""

Recommended install behavior:
- Install behavior: User
- Device restart behavior: No specific action

Minimum detection recommendation:
- Script-based detection (PowerShell):

if (Test-Path ""$env:LOCALAPPDATA\Windows Notifier\Modules\{moduleName}\manifest.json"") {{
    exit 0
}} else {{
    exit 1
}}
";
    }

    private static OfflineModuleType ParseType(string? manifestType) =>
        manifestType?.ToLowerInvariant() switch
        {
            "standard" => OfflineModuleType.Standard,
            "conditional" => OfflineModuleType.Conditional,
            "dynamic" => OfflineModuleType.Dynamic,
            "hero" => OfflineModuleType.Hero,
            "core_update" => OfflineModuleType.CoreSettings,
            _ => OfflineModuleType.Standard
        };

    private static int GetInt(JsonElement parent, string property, int fallback)
    {
        if (parent.TryGetProperty(property, out var el) && el.TryGetInt32(out var value))
        {
            return value;
        }
        return fallback;
    }

    public void IncrementReminderHours()
    {
        ReminderHours = Math.Max(ReminderHours ?? ReminderHoursMinimum, ReminderHoursMinimum) + 1;
    }

    public void DecrementReminderHours()
    {
        var current = Math.Max(ReminderHours ?? ReminderHoursMinimum, ReminderHoursMinimum);
        ReminderHours = Math.Max(ReminderHoursMinimum, current - 1);
    }

    public bool OpenLastOutputFolder()
    {
        if (string.IsNullOrWhiteSpace(LastOutputFolder) || !Directory.Exists(LastOutputFolder))
        {
            StatusMessage = "No output folder available to open yet.";
            return false;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = LastOutputFolder,
            UseShellExecute = true
        });
        return true;
    }

    public async Task<bool> SaveCurrentScriptAsTemplateAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedType is not (OfflineModuleType.Conditional or OfflineModuleType.Dynamic))
        {
            StatusMessage = "Templates can only be saved from Conditional or Dynamic modules.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewTemplateName))
        {
            StatusMessage = "Template name is required.";
            return false;
        }

        var scriptBody = SelectedType == OfflineModuleType.Conditional ? ConditionalScriptBody : DynamicScriptBody;
        if (string.IsNullOrWhiteSpace(scriptBody))
        {
            StatusMessage = "Cannot save empty script as template.";
            return false;
        }

        try
        {
            var templates = (await _templateStore.LoadAsync(cancellationToken)).ToList();
            var templateType = SelectedType == OfflineModuleType.Conditional
                ? OfflineScriptTemplateType.Conditional
                : OfflineScriptTemplateType.Dynamic;

            templates.RemoveAll(t =>
                string.Equals(t.Name, NewTemplateName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                t.Type == templateType);

            templates.Add(new OfflineScriptTemplate
            {
                Name = NewTemplateName.Trim(),
                Type = templateType,
                ScriptBody = scriptBody!
            });

            await _templateStore.SaveAsync(templates, cancellationToken);
            await RefreshTemplatesAsync(cancellationToken);
            NewTemplateName = string.Empty;
            StatusMessage = "Template saved.";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Template save failed: {ex.Message}";
            return false;
        }
    }

    public bool InsertSelectedTemplate()
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "Select a template first.";
            return false;
        }

        if (SelectedTemplate.Type == OfflineScriptTemplateType.Conditional)
        {
            SelectedType = OfflineModuleType.Conditional;
            ConditionalScriptBody = SelectedTemplate.ScriptBody;
            StatusMessage = $"Inserted template '{SelectedTemplate.Name}' into conditional script.";
            return true;
        }

        SelectedType = OfflineModuleType.Dynamic;
        DynamicScriptBody = SelectedTemplate.ScriptBody;
        StatusMessage = $"Inserted template '{SelectedTemplate.Name}' into dynamic script.";
        return true;
    }

    public async Task<bool> RenameSelectedTemplateAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "Select a template first.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewTemplateName))
        {
            StatusMessage = "New template name is required.";
            return false;
        }

        try
        {
            var newName = NewTemplateName.Trim();
            var templates = (await _templateStore.LoadAsync(cancellationToken)).ToList();
            var existing = templates.FirstOrDefault(t =>
                string.Equals(t.Name, SelectedTemplate.Name, StringComparison.OrdinalIgnoreCase) &&
                t.Type == SelectedTemplate.Type);

            if (existing == null)
            {
                StatusMessage = "Template no longer exists on disk. Refresh and try again.";
                return false;
            }

            templates.RemoveAll(t =>
                string.Equals(t.Name, newName, StringComparison.OrdinalIgnoreCase) &&
                t.Type == SelectedTemplate.Type);

            existing.Name = newName;
            await _templateStore.SaveAsync(templates, cancellationToken);
            await RefreshTemplatesAsync(cancellationToken);
            SelectedTemplate = ScriptTemplates.FirstOrDefault(t =>
                string.Equals(t.Name, newName, StringComparison.OrdinalIgnoreCase) &&
                t.Type == existing.Type);
            NewTemplateName = string.Empty;
            StatusMessage = "Template renamed.";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Template rename failed: {ex.Message}";
            return false;
        }
    }

    public async Task<bool> DeleteSelectedTemplateAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "Select a template first.";
            return false;
        }

        try
        {
            var templates = (await _templateStore.LoadAsync(cancellationToken)).ToList();
            var removed = templates.RemoveAll(t =>
                string.Equals(t.Name, SelectedTemplate.Name, StringComparison.OrdinalIgnoreCase) &&
                t.Type == SelectedTemplate.Type);

            if (removed == 0)
            {
                StatusMessage = "Template no longer exists on disk. Refresh and try again.";
                return false;
            }

            await _templateStore.SaveAsync(templates, cancellationToken);
            SelectedTemplate = null;
            await RefreshTemplatesAsync(cancellationToken);
            StatusMessage = "Template deleted.";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Template delete failed: {ex.Message}";
            return false;
        }
    }

    public async Task<bool> DuplicateSelectedTemplateAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedTemplate == null)
        {
            StatusMessage = "Select a template first.";
            return false;
        }

        try
        {
            var templates = (await _templateStore.LoadAsync(cancellationToken)).ToList();
            var source = templates.FirstOrDefault(t =>
                string.Equals(t.Name, SelectedTemplate.Name, StringComparison.OrdinalIgnoreCase) &&
                t.Type == SelectedTemplate.Type);

            if (source == null)
            {
                StatusMessage = "Template no longer exists on disk. Refresh and try again.";
                return false;
            }

            var desiredName = string.IsNullOrWhiteSpace(NewTemplateName)
                ? $"{source.Name} Copy"
                : NewTemplateName.Trim();
            var duplicateName = BuildUniqueTemplateName(templates, source.Type, desiredName);

            templates.Add(new OfflineScriptTemplate
            {
                Name = duplicateName,
                Type = source.Type,
                ScriptBody = source.ScriptBody
            });

            await _templateStore.SaveAsync(templates, cancellationToken);
            await RefreshTemplatesAsync(cancellationToken);
            SelectedTemplate = ScriptTemplates.FirstOrDefault(t =>
                string.Equals(t.Name, duplicateName, StringComparison.OrdinalIgnoreCase) &&
                t.Type == source.Type);
            NewTemplateName = string.Empty;
            StatusMessage = $"Template duplicated as '{duplicateName}'.";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Template duplicate failed: {ex.Message}";
            return false;
        }
    }

    public async Task<bool> ExportTemplatesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            StatusMessage = "Template export failed: destination file path is required.";
            return false;
        }

        try
        {
            var templates = (await _templateStore.LoadAsync(cancellationToken))
                .Where(IsValidTemplateEntry)
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var parent = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                Directory.CreateDirectory(parent);
            }

            var json = JsonSerializer.Serialize(templates, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
            StatusMessage = $"Templates exported: {filePath}";
            LastOutputFolder = parent;
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Template export failed: {ex.Message}";
            return false;
        }
    }

    public async Task<bool> ImportTemplatesAsync(string filePath, bool merge, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            StatusMessage = "Template import failed: selected file was not found.";
            return false;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var imported = JsonSerializer.Deserialize<List<OfflineScriptTemplate>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<OfflineScriptTemplate>();

            var validImported = imported.Where(IsValidTemplateEntry).ToList();
            if (validImported.Count == 0)
            {
                StatusMessage = "Template import failed: no valid template entries found.";
                return false;
            }

            var existing = merge
                ? (await _templateStore.LoadAsync(cancellationToken)).Where(IsValidTemplateEntry).ToList()
                : new List<OfflineScriptTemplate>();

            foreach (var template in validImported)
            {
                existing.RemoveAll(t =>
                    t.Type == template.Type &&
                    string.Equals(t.Name, template.Name, StringComparison.OrdinalIgnoreCase));
                existing.Add(new OfflineScriptTemplate
                {
                    Name = template.Name.Trim(),
                    Type = template.Type,
                    ScriptBody = template.ScriptBody
                });
            }

            await _templateStore.SaveAsync(existing, cancellationToken);
            await RefreshTemplatesAsync(cancellationToken);
            StatusMessage = merge
                ? $"Imported and merged {validImported.Count} template(s)."
                : $"Imported {validImported.Count} template(s) (replaced existing library).";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Template import failed: {ex.Message}";
            return false;
        }
    }

    public void SetIconSourcePath(string filePath)
    {
        IconSourcePath = filePath;
        if (!string.IsNullOrWhiteSpace(filePath) && string.IsNullOrWhiteSpace(IconFileName))
        {
            IconFileName = Path.GetFileName(filePath);
        }
    }

    public void SetHeroSourcePath(string filePath)
    {
        HeroSourcePath = filePath;
        if (!string.IsNullOrWhiteSpace(filePath) &&
            (string.IsNullOrWhiteSpace(HeroFileName) || HeroFileName == "hero.png"))
        {
            HeroFileName = Path.GetFileName(filePath);
        }
    }

    public bool LoadConditionalScriptFromFile(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                StatusMessage = "Conditional script load failed: file not found.";
                return false;
            }

            ConditionalScriptBody = File.ReadAllText(filePath);
            SelectedType = OfflineModuleType.Conditional;
            StatusMessage = $"Loaded conditional script: {Path.GetFileName(filePath)}";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Conditional script load failed: {ex.Message}";
            return false;
        }
    }

    public bool LoadDynamicScriptFromFile(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                StatusMessage = "Dynamic script load failed: file not found.";
                return false;
            }

            DynamicScriptBody = File.ReadAllText(filePath);
            SelectedType = OfflineModuleType.Dynamic;
            StatusMessage = $"Loaded dynamic script: {Path.GetFileName(filePath)}";
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Dynamic script load failed: {ex.Message}";
            return false;
        }
    }

    private void RefreshTemplateView()
    {
        IEnumerable<OfflineScriptTemplate> query = _allScriptTemplates;

        if (string.Equals(TemplateTypeFilter, "Conditional", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.Type == OfflineScriptTemplateType.Conditional);
        }
        else if (string.Equals(TemplateTypeFilter, "Dynamic", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.Type == OfflineScriptTemplateType.Dynamic);
        }

        if (!string.IsNullOrWhiteSpace(TemplateSearchText))
        {
            var search = TemplateSearchText.Trim();
            query = query.Where(t =>
                t.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.ScriptBody.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var selectedName = SelectedTemplate?.Name;
        var selectedType = SelectedTemplate?.Type;

        ScriptTemplates.Clear();
        foreach (var template in query.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
        {
            ScriptTemplates.Add(template);
        }

        if (!string.IsNullOrWhiteSpace(selectedName) && selectedType.HasValue)
        {
            SelectedTemplate = ScriptTemplates.FirstOrDefault(t =>
                string.Equals(t.Name, selectedName, StringComparison.OrdinalIgnoreCase) &&
                t.Type == selectedType.Value);
        }
        else if (ScriptTemplates.Count == 0)
        {
            SelectedTemplate = null;
        }
    }

    private static string BuildUniqueTemplateName(
        IReadOnlyList<OfflineScriptTemplate> templates,
        OfflineScriptTemplateType type,
        string baseName)
    {
        if (!templates.Any(t => t.Type == type && string.Equals(t.Name, baseName, StringComparison.OrdinalIgnoreCase)))
        {
            return baseName;
        }

        var suffix = 2;
        while (true)
        {
            var candidate = $"{baseName} ({suffix})";
            if (!templates.Any(t => t.Type == type && string.Equals(t.Name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                return candidate;
            }
            suffix++;
        }
    }

    private static bool IsValidTemplateEntry(OfflineScriptTemplate template)
    {
        if (template == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(template.Name) || string.IsNullOrWhiteSpace(template.ScriptBody))
        {
            return false;
        }

        return template.Type is OfflineScriptTemplateType.Conditional or OfflineScriptTemplateType.Dynamic;
    }

    private void OnAnyPropertyChanged(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName) ||
            propertyName is nameof(StatusMessage)
            or nameof(LastValidationSummary)
            or nameof(CurrentProjectPath)
            or nameof(IsDirty)
            or nameof(NewTemplateName)
            or nameof(TemplateSearchText)
            or nameof(TemplateTypeFilter)
            or nameof(SelectedTemplate)
            or nameof(SelectedProjectFile)
            or nameof(ProjectFiles)
            or nameof(ScriptTemplates)
            or nameof(LastOutputFolder))
        {
            return;
        }

        if (propertyName.StartsWith("Is", StringComparison.Ordinal))
        {
            return;
        }

        if (propertyName.EndsWith("Error", StringComparison.Ordinal))
        {
            return;
        }

        UpdateDirtyFlag();
    }

    private void MarkCleanFromCurrentDraft()
    {
        _savedDraftSnapshot = SerializeDraft(ToDraft());
        IsDirty = false;
        _ = ClearRecoverySnapshotAsync();
    }

    private void UpdateDirtyFlag()
    {
        var current = SerializeDraft(ToDraft());
        IsDirty = !string.Equals(_savedDraftSnapshot, current, StringComparison.Ordinal);
    }

    private static string SerializeDraft(OfflineModuleDraft draft)
    {
        return JsonSerializer.Serialize(draft, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    private string GetRecoverySnapshotPath() => Path.Combine(RecoveryRootFolder, RecoverySnapshotFileName);

    private void ApplyValidationIssues(ModuleValidationResult result)
    {
        ClearFieldErrors();
        foreach (var issue in result.Issues)
        {
            switch (issue.Field)
            {
                case "ModuleId":
                    ModuleIdError = issue.Message;
                    break;
                case "Title":
                    TitleError = issue.Message;
                    break;
                case "Message":
                    MessageError = issue.Message;
                    break;
                case "LinkUrl":
                    LinkUrlError = issue.Message;
                    break;
                case "ReminderHours":
                    ReminderHoursError = issue.Message;
                    break;
                case "ConditionalIntervalMinutes":
                    ConditionalIntervalMinutesError = issue.Message;
                    break;
                case "ConditionalScriptBody":
                    ConditionalScriptBodyError = issue.Message;
                    break;
                case "DynamicScriptBody":
                    DynamicScriptBodyError = issue.Message;
                    break;
                case "DynamicMaxLength":
                    DynamicMaxLengthError = issue.Message;
                    break;
                case "IconSourcePath":
                    IconSourcePathError = issue.Message;
                    break;
                case "HeroSourcePath":
                    HeroSourcePathError = issue.Message;
                    break;
                case "CorePollingIntervalSeconds":
                    CorePollingIntervalSecondsError = issue.Message;
                    break;
                case "CoreHeartbeatSeconds":
                    CoreHeartbeatSecondsError = issue.Message;
                    break;
                case "CoreSettings":
                    CoreEnabledError = issue.Message;
                    break;
                case "CoreEnabled":
                    CoreEnabledError = issue.Message;
                    break;
                case "CoreAutoClearModules":
                    CoreAutoClearModulesError = issue.Message;
                    break;
                case "CoreSoundEnabled":
                    CoreSoundEnabledError = issue.Message;
                    break;
                case "CoreExitMenuVisible":
                    CoreExitMenuVisibleError = issue.Message;
                    break;
                case "CoreStartStopMenuVisible":
                    CoreStartStopMenuVisibleError = issue.Message;
                    break;
                case "ScheduleUtc":
                    ScheduleUtcError = issue.Message;
                    break;
                case "ExpiresUtc":
                    ExpiresUtcError = issue.Message;
                    break;
            }
        }
    }

    private void ClearFieldErrors()
    {
        ModuleIdError = string.Empty;
        TitleError = string.Empty;
        MessageError = string.Empty;
        LinkUrlError = string.Empty;
        ReminderHoursError = string.Empty;
        ConditionalIntervalMinutesError = string.Empty;
        ConditionalScriptBodyError = string.Empty;
        DynamicScriptBodyError = string.Empty;
        DynamicMaxLengthError = string.Empty;
        IconSourcePathError = string.Empty;
        HeroSourcePathError = string.Empty;
        CorePollingIntervalSecondsError = string.Empty;
        CoreHeartbeatSecondsError = string.Empty;
        CoreEnabledError = string.Empty;
        CoreAutoClearModulesError = string.Empty;
        CoreSoundEnabledError = string.Empty;
        CoreExitMenuVisibleError = string.Empty;
        CoreStartStopMenuVisibleError = string.Empty;
        ScheduleUtcError = string.Empty;
        ExpiresUtcError = string.Empty;
    }

    private void SetScheduleLocalFromUtc(DateTime scheduleUtc)
    {
        var local = scheduleUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(scheduleUtc, DateTimeKind.Utc).ToLocalTime()
            : scheduleUtc.ToLocalTime();
        ScheduleEnabled = true;
        ScheduleLocalDate = local.Date;
        ScheduleHour = local.Hour;
        ScheduleMinute = RoundToMinuteBucket(local.Minute);
    }

    private void SetExpiresLocalFromUtc(DateTime expiresUtc)
    {
        var local = expiresUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(expiresUtc, DateTimeKind.Utc).ToLocalTime()
            : expiresUtc.ToLocalTime();
        ExpiresEnabled = true;
        ExpiresLocalDate = local.Date;
        ExpiresHour = local.Hour;
        ExpiresMinute = RoundToMinuteBucket(local.Minute);
    }

    private static DateTime BuildUtcFromLocal(DateTime localDate, int hour, int minute)
    {
        var normalizedHour = Math.Clamp(hour, 0, 23);
        var normalizedMinute = Math.Clamp(minute, 0, 59);
        var local = new DateTime(
            localDate.Year,
            localDate.Month,
            localDate.Day,
            normalizedHour,
            normalizedMinute,
            0,
            DateTimeKind.Local);
        return local.ToUniversalTime();
    }

    private static int RoundToMinuteBucket(int minute)
    {
        var rounded = (int)Math.Round(minute / 5.0, MidpointRounding.AwayFromZero) * 5;
        return rounded >= 60 ? 55 : rounded;
    }

    private sealed class RecoverySnapshot
    {
        public DateTime SavedAtUtc { get; set; }
        public string? ProjectPath { get; set; }
        public OfflineModuleDraft? Draft { get; set; }
    }
}

public sealed record RecoverySnapshotInfo(DateTime SavedAtUtc, string? ProjectPath);
