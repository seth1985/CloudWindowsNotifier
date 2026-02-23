# Offline Authoring Tool Development Plan

## Goal
Build a Windows desktop backup authoring application that can create, validate, import, export, and deploy notification modules without relying on the hosted frontend or API runtime availability.

## Recommended Stack
- Language: C#
- UI: WPF (.NET 8, Windows)
- Architecture: MVVM
- Storage: Local project files (`.wnproj` JSON) + local template catalog

Rationale:
- Aligns with existing C# codebase.
- Native Windows integration for file paths and deployment workflows.
- Fastest path to delivery with current team/tooling.

## Scope

### MVP In Scope
- Author module types:
  - `Standard`
  - `Conditional`
  - `Dynamic`
  - `Hero`
  - `CoreSettings`
- Validate module inputs/assets/scripts.
- Import existing module folders.
- Export module package to folder.
- Export Intune `.intunewin` package using `deployment_tools/IntuneWinAppUtil.exe` and `deployment_tools/install_module_intune.ps1`.
- TODO (future): Graph-based Intune publish + assignment to AAD groups (prefix filter `WN-*`) after Azure app registration and permissions are available.
- Deploy directly to `%LOCALAPPDATA%\Windows Notifier\Modules`.
- Local template library for script snippets.

### Out of Scope (MVP)
- Cloud authentication.
- Telemetry analytics dashboards.
- Multi-user collaboration/sync.

## Product Requirements

### Functional
1. Create/edit/delete local module projects.
2. Support module-type-specific authoring fields and conditional visibility in the editor.
3. Validate required fields per type and block invalid deploy.
4. Support hero/icon file selection and validation.
5. Generate output identical to tray runtime expectations:
   - `manifest.json`
   - `conditional.ps1` or `dynamic.ps1` (when applicable)
   - referenced assets in module folder
6. Deploy module directly into tray module root.
7. Import previously exported module folder for edits.
8. Generate Intune package output and include install instructions text file for Win32 app setup.

### Non-Functional
1. Must run offline end-to-end.
2. No dependency on API availability for core authoring/export/deploy.
3. Fast startup and low memory footprint.
4. Clear error reporting with actionable remediation.

## Architecture

### Project Structure (target)
1. `src/WindowsNotifier.OfflineAuthoring.App`
   - WPF UI, MVVM view models, workflows.
2. `src/WindowsNotifier.OfflineAuthoring.Core`
   - Domain models, validation, manifest/export orchestration.
3. `src/WindowsNotifier.OfflineAuthoring.Infrastructure`
   - File system, local project persistence, template repository.
4. `tests/WindowsNotifier.OfflineAuthoring.Tests`
   - Unit tests for validation and export output contracts.

### Reuse Strategy
1. Mirror `ManifestBuilder` logic used by API to avoid format drift.
2. Mirror export structure used by `ExportService`.
3. Keep manifest and export logic isolated in core library for future reuse by API.

## Delivery Phases

### Phase 0: Discovery and Contract Freeze (2-3 days)
1. Freeze tray manifest contract and file layout.
2. Define MVP screens and workflows.
3. Capture validation rules by module type.

### Phase 1: Shared Core Logic (1 week)
1. Create core models for module authoring.
2. Implement manifest generation compatible with tray parser.
3. Implement export/deploy services and validation pipeline.
4. Add unit tests for manifest and export structure.

### Phase 2: Desktop Foundation (1 week)
1. Scaffold WPF shell and navigation.
2. Create local persistence for projects and templates.
3. Add basic editor surface and project lifecycle.

### Phase 3: Authoring UX (1-2 weeks)
1. Build forms per module type.
2. Add asset pickers and script editors.
3. Add template insertion workflows.
4. Add validation UI and inline errors.

### Phase 4: Import/Export/Deploy (1 week)
1. Import existing module folders.
2. Export folder and Intune package.
3. Deploy to local tray modules root.
4. Add preflight validation report before deploy.

### Phase 5: Hardening and Release (1 week)
1. End-to-end testing with tray app.
2. Recovery features (autosave/version snapshots).
3. Packaging and installer publishing.
4. Documentation updates and fallback operations guide.

## Milestones
1. Milestone A: Core manifest/export library complete.
2. Milestone B: Offline app can create and save modules.
3. Milestone C: Offline export/deploy works with tray.
4. Milestone D: MVP release candidate.

## Risks and Mitigations
1. Risk: Manifest drift from API/tray expectations.
   - Mitigation: Shared contract tests and shared core library.
2. Risk: Asset validation differences causing runtime failures.
   - Mitigation: Reuse same validation constraints as tray/API.
3. Risk: Path/permission issues on endpoint machines.
   - Mitigation: Configurable deploy path with diagnostics.

## MVP Acceptance Criteria
1. Author all 5 module types offline.
2. Import, edit, and redeploy existing module folders.
3. Export/deploy output is consumed by tray without manual changes.
4. Validation blocks invalid manifests/assets/scripts before deploy.
5. Workflow runs fully with API/frontend offline.

## Immediate Next Build Steps
1. Scaffold WPF app and supporting class libraries.
2. Implement core module model + manifest generation service.
3. Implement initial local project save/load workflow.
4. Add first end-to-end flow: create standard module -> export -> deploy.
