param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\WindowsNotifier.OfflineAuthoring.App\WindowsNotifier.OfflineAuthoring.App.csproj"
$artifactsRoot = Join-Path $repoRoot "artifacts\offline-authoring"
$publishFolder = Join-Path $artifactsRoot "publish-$Configuration-$Runtime"
$zipPath = Join-Path $artifactsRoot "WindowsNotifier.OfflineAuthoring.$Configuration.$Runtime.zip"

if (Test-Path $publishFolder) {
    Remove-Item -Recurse -Force $publishFolder
}

New-Item -ItemType Directory -Path $artifactsRoot -Force | Out-Null

$scValue = if ($SelfContained.IsPresent) { "true" } else { "false" }

Write-Host "Publishing offline authoring app..."
dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained $scValue `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishFolder

if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}

Write-Host "Creating zip package..."
Compress-Archive -Path (Join-Path $publishFolder "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "Publish folder: $publishFolder"
Write-Host "Zip package:    $zipPath"
