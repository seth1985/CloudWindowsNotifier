param(
    [string]$SourceFolder = ".\artifacts\offline-authoring\publish-Release-win-x64",
    [string]$InstallRoot = "$env:LOCALAPPDATA\Windows Notifier\OfflineAuthoring",
    [switch]$NoShortcut
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$resolvedSource = if ([System.IO.Path]::IsPathRooted($SourceFolder)) { $SourceFolder } else { Join-Path $repoRoot $SourceFolder }

if (-not (Test-Path $resolvedSource)) {
    throw "Source folder not found: $resolvedSource"
}

$targetFolder = Join-Path $InstallRoot "app"
if (Test-Path $targetFolder) {
    Remove-Item -Recurse -Force $targetFolder
}
New-Item -ItemType Directory -Path $targetFolder -Force | Out-Null

Write-Host "Copying app files to $targetFolder ..."
Copy-Item -Path (Join-Path $resolvedSource "*") -Destination $targetFolder -Recurse -Force

$exe = Join-Path $targetFolder "WindowsNotifier.OfflineAuthoring.App.exe"
if (-not (Test-Path $exe)) {
    throw "Installed executable not found: $exe"
}

if (-not $NoShortcut.IsPresent) {
    $desktop = [Environment]::GetFolderPath("Desktop")
    $shortcutPath = Join-Path $desktop "Windows Notifier Offline Authoring.lnk"

    Write-Host "Creating desktop shortcut: $shortcutPath"
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $exe
    $shortcut.WorkingDirectory = $targetFolder
    $shortcut.IconLocation = $exe
    $shortcut.Save()
}

Write-Host ""
Write-Host "Installed executable: $exe"
