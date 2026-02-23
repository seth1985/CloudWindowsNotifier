# Cloud Windows Notifier

## Overview

Cloud Windows Notifier is a module-based Windows toast notification platform with three runtime parts:

- `core/WindowsNotifierTray`: endpoint tray app that scans module folders, evaluates behavior, shows toasts, and emits telemetry.
- `src/WindowsNotifierCloud.Api`: ASP.NET Core API for auth, module/campaign/template CRUD, export, telemetry ingest, and reporting.
- `frontend`: React + Vite admin portal for building modules and viewing telemetry.

## Workspace Layout

- `core/WindowsNotifierTray` - WinForms tray app (.NET 6, Windows 10 target)
- `core/WindowsNotifierHost` - activation-forwarding host process
- `src/WindowsNotifierCloud.Api` - Web API (.NET 8)
- `src/WindowsNotifierCloud.Domain` - entities and interfaces
- `src/WindowsNotifierCloud.Infrastructure` - EF Core DbContext, repositories, migrations
- `frontend` - React/TypeScript SPA
- `tests/WindowsNotifierCloud.Tests` - xUnit tests
- `notes` - architecture and hardening notes

## Prerequisites

- .NET SDK 8.0 (API/Domain/Infrastructure/tests)
- .NET SDK 6.0 with Windows targeting support (tray/host)
- Node.js 18+
- npm
- PostgreSQL 17+ running locally (or Docker Desktop)
- Optional: `dotnet-ef` CLI

## Quick Start

### 1. Run API

Start PostgreSQL first.

Option A: local PostgreSQL service (recommended on this repo's current setup)

- Ensure PostgreSQL service is running on `localhost:5432`
- Create clean dev DB (if needed):

```powershell
$env:PGPASSWORD='postgres'
psql -h localhost -p 5432 -U postgres -d postgres -c "CREATE DATABASE windows_notifier_cloud_dev"
```

Option B: Docker

```powershell
docker compose -f docker-compose.postgres.yml up -d
```

Then run the API:

```powershell
dotnet run --project src/WindowsNotifierCloud.Api
```

Default API URL: `http://localhost:5210`  
Swagger (Development): `http://localhost:5210/swagger`

### 2. Run Frontend

```powershell
cd frontend
npm install
npm run dev
```

Default frontend URL: `http://localhost:5173`

### 3. Build and Run Tray

```powershell
dotnet build core/WindowsNotifierTray/WindowsNotifierTray.csproj -c Release
core\WindowsNotifierTray\bin\Release\net6.0-windows10.0.19041.0\WindowsNotifierTray.exe
```

### 4. Build and Run Offline Authoring Tool

```powershell
dotnet run --project src/WindowsNotifier.OfflineAuthoring.App
```

Offline authoring output roots:

- Projects: `%USERPROFILE%\Documents\Windows Notifier\OfflineProjects`
- Exports: `%USERPROFILE%\Documents\Windows Notifier\OfflineExports`
- Intune packages: `%USERPROFILE%\Documents\Windows Notifier\IntunePackages`

Intune packaging dependencies:

- `deployment_tools\IntuneWinAppUtil.exe`
- `deployment_tools\install_module_intune.ps1`

Intune cloud publish status:

- `.intunewin` packaging is implemented.
- Graph-based publish/assignment to Intune groups is implemented in API/frontend but currently **TODO for activation** in this environment.
- Activation prerequisite: create an Azure app registration with Graph application permissions (`Group.Read.All`, `DeviceManagementApps.ReadWrite.All`) and set `IntuneDeployment:*` config values.

### 5. Publish Offline Authoring Tool (Distributable)

Use the packaging script:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\publish_offline_authoring.ps1
```

Optional self-contained publish:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\publish_offline_authoring.ps1 -SelfContained
```

Output artifacts:

- Publish folder: `artifacts\offline-authoring\publish-<Configuration>-<Runtime>`
- Zip package: `artifacts\offline-authoring\WindowsNotifier.OfflineAuthoring.<Configuration>.<Runtime>.zip`

Install published app locally and create desktop shortcut:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\install_offline_authoring.ps1
```

## Configuration (DevelopmentLocal)

Defaults from `src/WindowsNotifierCloud.Api/appsettings.Development.json`:

- `Environment:Mode`: `DevelopmentLocal`
- `Authentication:Provider`: `Local`
- `Storage:Root`: `%LOCALAPPDATA%\\Windows Notifier\\ApiStorage`
- `Storage:DevCoreModulesRoot`: `%LOCALAPPDATA%\\Windows Notifier\\Modules`
- `ConnectionStrings:Default`: `Host=localhost;Port=5432;Database=windows_notifier_cloud_dev;Username=postgres;Password=postgres`
- `Telemetry:ApiKey`: `dev-telemetry-key-change-me`
- `IntuneDeployment:Enabled`: `false` (set `true` only after Azure app registration is ready)

Default seeded portal user (if none exists):

- Username: `admin`
- Password: `P@ssw0rd!`
- Role: `Admin`

Flip to Entra later:

1. Set `Authentication:Provider` to `Entra` in active appsettings or environment variables.
2. Populate `Entra:TenantId`, `Entra:ApiAudience`, `Entra:SpaClientId`, and `Entra:Scope`.
3. Restart API and frontend.

## Runtime Paths

API:

- PostgreSQL DB: configured via `ConnectionStrings:Default`
- Export output: `%LOCALAPPDATA%\\Windows Notifier\\ApiStorage\\modules\\exports`
- Uploaded assets: `%LOCALAPPDATA%\\Windows Notifier\\ApiStorage\\module-assets`

Tray/Core:

- Module scan root: `%LOCALAPPDATA%\\Windows Notifier\\Modules`
- Core settings registry key: `HKCU\\Software\\WindowsNotifier\\Core`
- Module state registry key: `HKCU\\Software\\WindowsNotifier\\Modules`
- Telemetry retry queue: `%LOCALAPPDATA%\\Windows Notifier\\Telemetry\\pending.jsonl`

## Implemented Capabilities

- Module types: `Standard`, `Conditional`, `Dynamic`, `Hero`, `CoreSettings`
- Manifest generation and preview
- Export to storage, Dev Core, and ZIP package
- Template gallery CRUD
- Telemetry ingest and reporting summaries
- Role-based API authorization
- Offline authoring desktop app with local project save/load, import module folder, deploy to local modules root, and Intune package export
- Intune group-targeted publish workflow in frontend/API (feature remains disabled until Azure app registration and Graph permissions are configured)
- Unsaved-changes guard in offline app for new/open/load/import/close with save-discard-cancel prompt
- Local recovery snapshots with startup restore prompt, periodic autosave (2 minutes), and stale snapshot cleanup
- Inline field-level validation messages in offline editor (in addition to summary panel)
- Script file workflow in offline editor: load `.ps1` directly into Conditional/Dynamic editor and edit inline
- Keyboard shortcuts in offline editor: `Ctrl+S` save, `Ctrl+Shift+S` save as, `F5` validate

## API Surface (Current)

Auth and config:

- `POST /api/auth/login`
- `GET /api/config/environment`

Modules and assets:

- `GET /api/modules`
- `GET /api/modules/{id}`
- `POST /api/modules`
- `PUT /api/modules/{id}`
- `DELETE /api/modules/{id}`
- `POST /api/modules/{id}/icon`
- `GET /api/modules/{id}/icon`
- `POST /api/modules/{id}/hero`
- `GET /api/modules/{id}/hero`
- `POST /api/modules/{id}/assets/icon`

Manifest and export:

- `GET /api/manifest/{id}`
- `GET /api/manifest/{moduleId}/preview`
- `POST /api/export/{id}`
- `POST /api/export/{id}/devcore`
- `POST /api/export/{id}/package`
- `GET /api/intune/groups` (AdvancedOnly, filtered by configured prefix, default `WN-`)
- `POST /api/intune/deploy/{id}` (AdvancedOnly, publish/assign to selected group)

Campaigns, templates, users:

- `GET/POST/PUT/DELETE /api/campaigns`
- `GET/POST/PUT/DELETE /api/templates`
- `GET/POST/PUT/DELETE /api/users`

Telemetry and reporting:

- `POST /api/telemetry/events` (header `x-wn-api-key`)
- `GET /api/reporting/summary`
- `GET /api/reporting/modules`
- `GET /api/reporting/modules/{moduleId}`
- `GET /api/reporting/campaigns/{campaignId}`

Health and maintenance:

- `GET /api/health/ping`
- `GET /api/health/ready`
- `POST /api/maintenance/storage/cleanup`

## Authorization

JWT bearer auth is enabled for portal APIs.

Policies:

- `BasicOrAdvanced`
- `AdvancedOnly`
- `AdminOnly`

## Common Commands

Run API with PostgreSQL:

```powershell
dotnet run --project src/WindowsNotifierCloud.Api
```

Apply/update DB schema:

```powershell
dotnet ef database update --project src/WindowsNotifierCloud.Infrastructure --startup-project src/WindowsNotifierCloud.Api
```

Run tests:

```powershell
dotnet test WindowsNotifierCloud.sln
```

Build frontend:

```powershell
cd frontend
npm run build
```

## Notes

- API and infrastructure target .NET 8; tray/host target .NET 6 Windows.
- `ModuleHeroController` uses `System.Drawing`, which raises cross-platform analyzer warnings in API builds.
- SQLite support has been removed; this repo is now PostgreSQL-only.
- Test coverage is currently minimal.
- Intune Graph deployment endpoints are present but should stay disabled until Azure app registration + Graph app permissions are provisioned.
