# Module Distribution via Intune/Win32App (Plan)

Goal: deliver packaged modules to Core across Entra ID-managed devices using Microsoft Intune Win32App deployments (intunewin). Capture the approach now; implement later.

## Overview
1) Package the module export (folder + manifest + assets) into an `.intunewin`.
2) Create/Update a Win32App in Intune via Microsoft Graph.
3) Assign the app to an Entra ID group (includes targeted devices/users).
4) Device receives the payload; install/unpack into the Core Modules root (e.g., `%LOCALAPPDATA%\CloudNotifier\Modules`), then Core ingests on next scan.

## Packaging
- Input: module export folder from the API (contains `manifest.json`, icon, scripts/assets).
- Wrap into an `.intunewin` using the Microsoft Win32 Content Prep Tool.
- Installer command (suggested): copy/unzip contents into `%LOCALAPPDATA%\CloudNotifier\Modules\<moduleId>` (or shared modules root if using ProgramData).
- Detection rule: presence of `manifest.json` at the target path, or registry marker written by a tiny post-install script.

## Graph Steps (to automate later)
- Auth: App registration with Graph scopes for Intune app management.
- Create Win32 app (`/deviceAppManagement/mobileApps`) with:
  - Install command: deploy module files to the Modules root.
  - Uninstall command: remove the module folder (optional).
  - Detection rules: file/registry as above.
  - Requirement rules: OS version, disk space, etc.
- Upload the `.intunewin` content to the app’s content version.
- Assign the app to the target Entra ID group (`/assign`).

## Device Flow
- Intune agent downloads the Win32 app content.
- Runs install command to place module files in Modules root.
- Core scan loop picks up the new module and shows the toast according to manifest.

## Considerations
- Versioning: include module version in folder name or manifest; detection should account for version to allow upgrades.
- Cleanup: uninstall should remove the module folder; Core’s auto-clear can also clean completed/expired modules, but Intune uninstall should be authoritative.
- Payload size: keep assets small; icons already copied per module.
- Security: modules are still validated by Core using manifest fields; ensure installer path is locked down.
- Scheduling: Intune assignment + delivery timing may not be immediate; design modules with reasonable schedule/expiry windows.

## Next Steps (when we implement)
- Script the `.intunewin` creation for a built module.
- Script Graph calls to create/update the Win32 app and assign to a group.
- Add a small installer script for the module payload (copy + optional registry marker).
- Optionally, surface this pipeline in the portal as “Deploy via Intune” once stable.
