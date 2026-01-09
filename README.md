# Cloud Windows Notifier

Cloud-based notification platform composed of three parts:

- **Core / Tray app (WinForms, .NET 6)** – scans modules from local disk, shows Windows toast notifications (standard, conditional, dynamic, hero), applies Core settings, and sends telemetry with resilient retry.
- **API (ASP.NET Core)** – authentication, modules CRUD/export, telemetry ingestion & reporting, PowerShell template gallery.
- **Frontend (React + Vite)** – admin UI to build/export modules, manage templates, view telemetry, and Core settings.

> Repository: https://github.com/seth1985/CloudWindowsNotifier.git  
> Root: `C:\Users\crven\OneDrive\Documents\AI_Builds\Cloud_Based_Notifier`

---
## Prerequisites
- .NET SDK 8.0 (API/Infrastructure) and .NET 6.0 (Tray target)
- Node.js 18+ for the frontend
- SQLite (bundled via Microsoft.Data.Sqlite)

---
## Quick Start
1) **Restore & migrate DB (first run)**
```powershell
cd C:\Users\crven\OneDrive\Documents\AI_Builds\Cloud_Based_Notifier
dotnet ef database update --project src/WindowsNotifierCloud.Infrastructure --startup-project src/WindowsNotifierCloud.Api
```

2) **Run API (default http://localhost:5210)**
```powershell
dotnet run --project src/WindowsNotifierCloud.Api
```

3) **Run Frontend (default http://localhost:5173)**
```powershell
cd frontend
npm install
npm run dev
```

4) **Build & run Tray/Core**
```powershell
cd C:\Users\crven\OneDrive\Documents\AI_Builds\Cloud_Based_Notifier
dotnet build core/WindowsNotifierTray/WindowsNotifierTray.csproj -c Release
core\WindowsNotifierTray\bin\Release\net6.0-windows10.0.19041.0\WindowsNotifierTray.exe
```

5) **Export modules from Frontend** to Dev Core – they will be written under `%LOCALAPPDATA%\CloudNotifier\Modules` and picked up by the tray.

---
## Components & Paths
### Core / Tray
- Scans modules in `%LOCALAPPDATA%\CloudNotifier\Modules\module-<id>`.
- State/registry: `HKCU\Software\CloudNotifier\Core` (settings, telemetry URL/key).
- Telemetry retry queue: `%LOCALAPPDATA%\WindowsNotifier\Telemetry\pending.jsonl` (append-only JSONL with retries/backoff).
- Default icons/hero validation happens when showing a toast (icon ≤512 KB; hero PNG 2:1, ≤1 MB).

### API
- Project: `src/WindowsNotifierCloud.Api`
- DB: `src/WindowsNotifierCloud.Api/bin/Debug/net8.0/App_Data/wncloud.db` (or Release equivalent)
- Key endpoints:
  - `POST /api/modules` (create/upsert), `POST /api/export/{id}/devcore`
  - `GET /api/reporting/modules`, `GET /api/reporting/devices/{moduleId}`
  - `POST /api/telemetry/events`
  - Templates: `GET /api/templates?type=conditional|dynamic|both`, `POST /api/templates`, `PUT /api/templates/{id}`, `DELETE /api/templates/{id}`

### Frontend
- Vite/React app at `frontend` (modular pages under `frontend/src/features`).
- Runs on 5173 by default; API base is configurable in the UI.
- Offers module builder (Standard, Conditional, Dynamic, Hero, Core Settings), telemetry view, and PowerShell Templates gallery.

---
## Module Types
- **Standard** – fixed title/body/link/icon; reminder hours optional.
- **Conditional** – PowerShell exit 0 => show; interval minutes for re-check; script templates available.
- **Dynamic** – PowerShell produces body (title/message fields are optional in UI); max length enforced (160 cap); trim/fail-if-empty/fallback options.
- **Hero** – 2:1 PNG banner (up to ~1 MB); optional message; uses hero image instead of app logo.
- **Core Settings** – applies polling/heartbeat/auto-clear/sound/Exit+StartStop menu visibility/telemetry URL+key. Admin-only intent.

---
## Telemetry (with retry queue)
- Immediate send: HTTP POST to `telemetryUrl` with header `x-wn-api-key`.
- If a send fails, the event is appended to `pending.jsonl` and retried on a background timer (5 min, exponential backoff capped at 60 min).
- Drop rules: max 10 attempts, max age 7 days, queue trimmed to 5 MB (oldest removed).
- **Config sources (priority):** env `WN_TELEMETRY_URL`/`WN_TELEMETRY_KEY` → registry `HKCU\Software\CloudNotifier\Core` TelemetryUrl/TelemetryKey → `%LOCALAPPDATA%\CloudNotifier\tray.config.json` → dev defaults.

---
## Building Blocks & Project Structure
- `core/WindowsNotifierTray` – WinForms tray, toast rendering (Windows.UI.Notifications), module scan loop, telemetry queue.
- `src/WindowsNotifierCloud.Api` – ASP.NET Core API, SQLite, EF Core migrations.
- `src/WindowsNotifierCloud.Domain` – domain models (ModuleDefinition, TelemetryEvent, PowerShellTemplate, etc.).
- `src/WindowsNotifierCloud.Infrastructure` – EF Core DbContext, migrations, repositories.
- `frontend` – React/Vite UI, modular pages in `src/wnc_modular`, assets in `public` (icons, scripts).

---
## Common Commands
```powershell
# API
dotnet run --project src/WindowsNotifierCloud.Api
dotnet ef database update --project src/WindowsNotifierCloud.Infrastructure --startup-project src/WindowsNotifierCloud.Api

# Frontend
cd frontend
npm install
npm run dev    # or npm run build

# Tray / Core
dotnet build core/WindowsNotifierTray/WindowsNotifierTray.csproj -c Release
core\WindowsNotifierTray\bin\Release\net6.0-windows10.0.19041.0\WindowsNotifierTray.exe
```

---
## PowerShell Templates
- Managed via UI gallery (Conditional/Dynamic) with Modify/Remove.
- API supports create/update/delete; templates stored in DB.
- Selecting a template injects its script into the active module editor.

---
## Export & Module Flow
1) Build module in frontend, Save.
2) Export to Dev Core → writes `manifest.json` + assets into `%LOCALAPPDATA%\CloudNotifier\Modules\module-<id>`.
3) Tray scan loop reads manifest, evaluates behavior (conditional/dynamic), validates media, shows toast.
4) On activation (OK/More info), tray updates module state/registry, completes module, sends telemetry.

---
## Troubleshooting
- **API not reachable**: ensure `dotnet run --project src/WindowsNotifierCloud.Api` is running (default port 5210). Update API base in frontend.
- **DB schema errors (e.g., HeroFileName missing)**: rerun migrations (`dotnet ef database update ...`) or delete/recreate local `App_Data/wncloud.db` then migrate.
- **Telemetry missing**: verify telemetry URL/key (env/registry/config). Check `%LOCALAPPDATA%\WindowsNotifier\Telemetry\pending.jsonl` for queued events; network failures will retry automatically.
- **Hero/icon rejected**: ensure PNG 2:1 up to ~1 MB for hero; icon <= 512 KB, png/jpg/ico.
- **Core settings not applied**: export a Core Settings module; tray applies and auto-clears it. Registry at `HKCU\Software\CloudNotifier\Core` will reflect applied values.

---
## Security & Roles
- Admin-only intent for Core settings and template management (UI currently trusts authenticated “Advanced” role).
- Telemetry protected by `x-wn-api-key`; rotate by updating registry/config/env and restarting tray.

---
## Notes
- Target frameworks: API/Infrastructure use .NET 8; tray targets `net6.0-windows10.0.19041.0` (build warnings expected).
- Frontend uses Vite/React; styling lives in `frontend/src/wnc_modular` and `frontend/public` assets.
- Queue/log files live under `%LOCALAPPDATA%\CloudNotifier` (modules, config) and `%LOCALAPPDATA%\WindowsNotifier\Telemetry` (retry queue). 
