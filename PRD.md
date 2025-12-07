0. Context
Windows Notifier is a module-driven toast notification system for Windows endpoints:
•	Core (tray app on endpoints)
o	Runs in user context.
o	Scans %LOCALAPPDATA%\Windows Notifier\Modules for module folders.
o	Each module has a manifest.json plus optional assets/scripts.
o	Shows toast notifications and updates per-module registry state.
•	Builder (currently a PowerShell/WPF app)
o	Creates/edits module folders locally.
We are replacing the Builder with a web-based Admin Portal that is:
•	Cloud-ready (Entra ID, Azure VM, Graph & Intune integration).
•	Also usable locally on a single developer machine (no Entra ID, no Graph required to run).
In production, the portal will live on an Azure VM in the same tenant as Intune.
During development, it must run locally on the developer’s workstation (optional Hyper-V VM), saving modules to disk so they can be copied into the Core’s Modules folder.
________________________________________
1. Goals & Non-Goals
1.1 Goals
1.	Admin Portal (Builder replacement)
o	Web UI for authenticated admins to:
	Create, edit, categorize, and delete modules (Standard, Conditional, Dynamic, Core Settings).
	Organize modules into campaigns.
	Upload icons and scripts to be bundled in module folders.
	Preview generated manifest.json.
	Export modules:
	To a generic export folder (for production deployment).
	Directly into a local Core Modules folder for development/testing.
2.	Dual-Mode Environment (Cloud-ready + Local dev)
o	Production mode:
	Runs on Azure VM.
	Uses Entra ID (Azure AD) authentication.
	Later: Intune and Graph integration.
o	Development mode (on physical dev device or local VM):
	Runs with simple local auth (no Entra / Graph required).
	Uses local SQL and local file storage.
	Allows one-click export to a configurable local Core Modules path.
3.	Enterprise-grade Reporting & Telemetry
o	Core will POST telemetry events back to the portal.
o	Portal shows:
	Overview of modules and campaigns.
	Counts of:
	Toasts shown.
	“OK / I understand” presses.
	“More info / Learn more” presses.
	Non-interaction (toast shown but no button press).
	Time range, start and expiry dates, status.
	Module details (author, type, campaign, deployment targets).
4.	Roles & Permissions
o	Basic role:
	Can create/edit/deploy Standard modules only.
o	Advanced role:
	Can create/edit/deploy all types (Standard, Conditional, Dynamic, Core Settings).
o	Role enforcement must be implemented in both UI and backend.
5.	Deployment Groups (for later Intune integration)
o	Concept of “Deployment Groups” as a list of AAD groups whose names start with WN-.
o	In production later, these are pulled from Microsoft Graph and used to help assign modules/apps in Intune.
o	For earlier phases, just model them in the DB and stub data if Graph isn’t configured.
6.	Intune Integration is LAST
o	Intune Win32 app packaging + Graph-based creation/updates are a final phase.
o	The portal must be fully functional for local file-based module export before Intune is wired in.
1.2 Non-Goals for this spec
•	No multi-tenant SaaS; single-tenant only.
•	No need to re-implement Core; only define telemetry contract and file layout.
•	No need to automate Intune assignments in v1 (admin can use Intune portal).
________________________________________
2. Environments & Configuration
2.1 Modes
Add a configuration setting: EnvironmentMode with values:
•	"DevelopmentLocal"
o	Runs on a physical dev machine (or local Hyper-V VM).
o	Auth uses a simple local mechanism (e.g. seeded admin users in DB, username/password login).
o	No requirement to reach Entra ID / Graph.
o	All module exports are to local filesystem paths.
•	"ProductionCloud"
o	Runs on Azure VM.
o	Uses Entra ID (OpenID Connect) for admin sign-in.
o	Intended to support Graph & Intune integration in later phases.
Codex: implement a simple environment switch using appsettings.Development.json / appsettings.Production.json, or equivalent.
2.2 Local Dev Stack
For DevelopmentLocal:
•	Backend runs with Kestrel on https://localhost:<port>.
•	Frontend runs either:
o	As a separate dev server (npm run dev) OR
o	Built and served by backend in a single process.
•	Database:
o	SQL Server Express / LocalDB OR SQLite.
•	File storage:
o	Local directory (e.g. C:\WN-Portal-Storage):
	/modules – persistent module assets & exports.
	/temp – working area for packaging.
2.3 Local Core Modules Export
A configurable setting:
•	DevCoreModulesRoot:
o	Example: %LOCALAPPDATA%\Windows Notifier\Modules on the dev machine.
Portal must provide a button: “Export to Dev Core” which:
1.	Builds the module folder (manifest + assets + scripts).
2.	Copies it into <DevCoreModulesRoot>\<ModuleId>.
This lets the developer immediately test with their local Core.
________________________________________
3. Tech Stack
Codex: use this stack unless instructed otherwise.
•	Backend: C# / .NET 8, ASP.NET Core Web API.
•	Frontend: React + TypeScript SPA.
•	Database: EF Core with SQL Server (or SQLite in dev).
•	Auth:
o	DevelopmentLocal: simple local login (username/password stored in DB, hashed).
o	ProductionCloud: Entra ID (Azure AD) OpenID Connect.
•	Hosting in production: ASP.NET Core on Azure VM (service or IIS).
•	Future integration: Microsoft Graph + Intune, last phase.
________________________________________
4. High-Level Architecture
1.	Admin Portal Frontend
o	Module & campaign management.
o	Export to file system / Dev Core.
o	Reporting dashboards.
o	Role-based UI (Basic vs Advanced).
2.	Backend API
o	Modules & campaigns CRUD.
o	Manifest generation.
o	Export endpoints (filesystem, Dev Core).
o	Telemetry ingestion.
o	Reporting APIs.
o	Intune integration endpoints (Phase last).
3.	Database
o	Users & Roles.
o	Module definitions & versions.
o	Campaigns.
o	Deployment groups.
o	Telemetry events.
4.	File Storage
o	Module assets (icons, scripts).
o	Exported module folders ready for Core/Intune.
________________________________________
5. Data Models
5.1 Users & Roles
Simplified entities:
public enum PortalRole
{
    Basic,
    Advanced
}

public class PortalUser
{
    public Guid Id { get; set; }
    public string UserPrincipalName { get; set; } // For Entra-based users
    public string DisplayName { get; set; }

    // For DevelopmentLocal only:
    public string LocalUsername { get; set; }
    public string PasswordHash { get; set; }

    public PortalRole Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
Logic:
•	In DevelopmentLocal, authenticate via LocalUsername + PasswordHash.
•	In ProductionCloud, map Entra ID user → PortalUser row by UPN; missing users default to Basic or are denied based on a config.
5.2 Campaign
A campaign groups multiple modules (e.g., “VPN Upgrade Q1”).
public class Campaign
{
    public Guid Id { get; set; }
    public string Name { get; set; }          // e.g. "VPN Upgrade Q1"
    public string Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public PortalUser CreatedBy { get; set; }

    public ICollection<ModuleDefinition> Modules { get; set; }
}
5.3 ModuleDefinition
public enum ModuleType
{
    Standard,
    Conditional,
    Dynamic,
    CoreSettings
}

public enum ModuleCategory
{
    GeneralInfo,
    Security,
    Compliance,
    Maintenance,
    Application,
    Other
}

public class ModuleDefinition
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }       // Friendly name for admins
    public string ModuleId { get; set; }          // Stable ID used in manifest/registry
    public ModuleType Type { get; set; }
    public ModuleCategory Category { get; set; }
    public string Description { get; set; }

    public Guid? CampaignId { get; set; }
    public Campaign Campaign { get; set; }

    // Content
    public string Title { get; set; }             // Toast title
    public string Message { get; set; }           // Toast body (Standard / Conditional)
    public string LinkUrl { get; set; }

    // Scripts
    public string ConditionalScriptBody { get; set; } // For Conditional
    public string DynamicScriptBody { get; set; }     // For Dynamic

    // Scheduling
    public DateTime CreatedUtc { get; set; }
    public DateTime? ScheduleUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }
    public string ReminderHours { get; set; }

    // Media
    public string IconFileName { get; set; }      // Stored in portal storage
    public string IconOriginalName { get; set; }

    // Dynamic options
    public int? DynamicMaxLength { get; set; }
    public bool? DynamicTrimWhitespace { get; set; }
    public bool? DynamicFailIfEmpty { get; set; }
    public string DynamicFallbackMessage { get; set; }

    // Core settings (for CoreSettings type)
    public CoreSettingsBlock CoreSettings { get; set; }

    // Lifecycle
    public int Version { get; set; }
    public bool IsPublished { get; set; }

    public Guid CreatedByUserId { get; set; }
    public PortalUser CreatedBy { get; set; }
    public Guid? LastModifiedByUserId { get; set; }
    public PortalUser LastModifiedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }

    // Future: Intune bindings (Phase Intune)
    public ICollection<IntuneAppBinding> IntuneBindings { get; set; }
}
CoreSettingsBlock holds fields equivalent to your existing core_settings schema.
5.4 DeploymentGroup
Represents an AAD group that starts with WN- (for Intune later):
public class DeploymentGroup
{
    public Guid Id { get; set; }
    public string AadGroupId { get; set; }     // Graph objectId
    public string DisplayName { get; set; }    // Must start with "WN-"
    public bool IsActive { get; set; }
    public DateTime DiscoveredAtUtc { get; set; }
}
For pre-Intune phases, this can be populated manually or left empty.
5.5 TelemetryEvent
public enum TelemetryEventType
{
    ToastShown,
    ButtonOk,
    ButtonMoreInfo,
    Dismissed,
    TimedOut,
    ScriptError,
    ConditionCheck,
    Completed
}

public class TelemetryEvent
{
    public Guid Id { get; set; }
    public string ModuleId { get; set; }            // From manifest.id
    public string DeviceId { get; set; }            // e.g., device GUID or hostname
    public string UserPrincipalName { get; set; }

    public TelemetryEventType EventType { get; set; }
    public DateTime OccurredAtUtc { get; set; }

    public string AdditionalDataJson { get; set; }  // error message, core version, etc.
}
________________________________________
6. Manifest Generation
Codex must implement a generator that converts ModuleDefinition → manifest.json matching your current Core schema.
Shape (illustrative):
{
  "id": "module-id-string",
  "type": "standard | conditional | dynamic | core_settings",
  "title": "…",
  "message": "…",
  "created_utc": "2025-01-01T12:34:56Z",
  "schedule_utc": "…",
  "expires_utc": "…",
  "media": {
    "icon": "icon.png",
    "link": "https://example.com"
  },
  "behavior": {
    "reminder_hours": "24",
    "conditional_script": "check.ps1",
    "conditional_interval_minutes": 60
  },
  "dynamic": {
    "script": "dynamic.ps1",
    "maxLength": 240,
    "options": {
      "trimWhitespace": true,
      "failIfEmpty": true,
      "fallbackMessage": "No data available"
    }
  },
  "core_settings": {
    "enabled": 1,
    "polling_interval_seconds": 300,
    "auto_clear_modules": 1,
    "sound_enabled": 1,
    "exit_menu_visible": 0,
    "start_stop_menu_visible": 0,
    "heartbeat_seconds": 15
  }
}
Backend endpoints:
•	GET /api/modules/{id}/manifest
•	GET /api/modules/{id}/manifest-preview
________________________________________
7. Module Storage & Export
7.1 File Layout in Portal Storage
Under StorageRoot (configurable):
•	modules/assets/<moduleGuid>/ – icons, scripts uploaded for that module.
•	modules/exports/<moduleGuid>/version-<n>/ – exported module folders ready for Core/Intune.
7.2 Export API
Implement:
•	POST /api/modules/{id}/export
o	Builds a fresh module folder:
	manifest.json
	icon.* (if set)
	*.ps1 scripts (conditional/dynamic)
o	Writes to modules/exports/<moduleGuid>/version-<n>/module/
Return:
•	Path (server-side) + a downloadable archive (zip).
7.3 Export to Dev Core (DevelopmentLocal only)
•	POST /api/modules/{id}/export-dev-core
o	Requires EnvironmentMode == DevelopmentLocal.
o	Uses configured DevCoreModulesRoot.
o	Copies built module folder to <DevCoreModulesRoot>\<ModuleId>.
Frontend: button “Export to Dev Core” on module details page, visible only in DevelopmentLocal.
________________________________________
8. Telemetry & Reporting
8.1 Telemetry Ingestion API
Core will POST to:
POST /api/telemetry/events
Body:
{
  "moduleId": "string",
  "deviceId": "string",
  "userPrincipalName": "user@domain.com",
  "eventType": "ToastShown | ButtonOk | ButtonMoreInfo | Dismissed | TimedOut | ScriptError | ConditionCheck | Completed",
  "occurredAtUtc": "2025-01-01T12:34:56Z",
  "additionalData": {
    "status": "Pending",
    "lastError": "Script timeout",
    "coreVersion": "1.0.0",
    "sessionId": "optional-session-guid"
  }
}
Backend must:
•	Validate fields.
•	Map eventType → TelemetryEventType.
•	Serialize additionalData → JSON string field.
•	Insert TelemetryEvent row.
Auth:
•	For v1, use an API key header: x-wn-api-key.
•	Config: Telemetry:ApiKey.
8.2 Reporting Queries & APIs
Required insights:
•	For each module:
o	Total ToastShown.
o	Total ButtonOk.
o	Total ButtonMoreInfo.
o	Calculated “Shown with no interaction” = ToastShown - (ButtonOk + ButtonMoreInfo + Dismissed + TimedOut) (approximation).
o	Unique users.
o	First/last event timestamps.
•	For each campaign:
o	Aggregated metrics over all modules in that campaign.
•	Time-filtering:
o	Range inputs: fromUtc, toUtc.
Endpoints (suggested):
•	GET /api/reporting/summary?from=...&to=...
o	Overall counts + top modules by completion/etc.
•	GET /api/reporting/modules
o	Returns per-module aggregates with basic details (name, type, category, campaign, author, start/expiry).
•	GET /api/reporting/modules/{moduleId}
o	Detail metrics + small event timeline.
•	GET /api/reporting/campaigns/{campaignId}
o	Aggregated metrics by campaign.
These will back the “Enterprise-grade overview” UI.
________________________________________
9. Auth & Roles
9.1 DevelopmentLocal
•	Implement simple local login:
o	POST /api/auth/login with { username, password }.
o	Validate against PortalUser.LocalUsername + PasswordHash.
o	Issue JWT with Role claim (Basic/Advanced).
•	Provide a minimal seeding mechanism:
o	On first run, create an initial Advanced user with known credentials (documented).
9.2 ProductionCloud (future-ready)
•	Use Entra ID OpenID Connect:
o	Map UPN to PortalUser.UserPrincipalName.
o	If user does not exist:
	Either deny access or create an entry with default role (Basic), configurable.
•	Use role-based authorization:
o	Advanced role required for:
	Creating/editing Conditional, Dynamic, CoreSettings modules.
	Changing CoreSettings block.
	Managing campaigns.
o	Basic role can:
	View all modules & campaigns.
	Create/edit Standard modules.
	Export their own modules.
Enforce roles both in backend (policies/attributes) and frontend (hide controls).
________________________________________
10. Frontend UX Requirements
10.1 Module List
Columns:
•	Name
•	ModuleId
•	Type (Standard / Conditional / Dynamic / Core Settings)
•	Category
•	Campaign
•	Author
•	Version
•	Status (Draft / Published)
•	Actions (Edit, Clone, Export, Export to Dev Core [dev only], View Metrics)
Filters:
•	By Type
•	By Category
•	By Campaign
•	By Status
10.2 Module Editor
Sections:
•	General:
o	Name, ModuleId (read-only after creation), Type, Category, Campaign selector.
o	Description.
•	Content:
o	Title, Message (for Standard, Conditional).
o	Link URL.
o	For Dynamic:
	Dynamic script editor area (text).
	Options: Max length, Trim whitespace, Fail if empty, Fallback message.
•	Scheduling:
o	Created (read-only).
o	Schedule UTC (date/time picker).
o	Expires UTC.
o	Reminder hours.
•	Media:
o	Upload icon file (shows preview).
o	Option to clear icon.
•	Behavior (only for Conditional & Dynamic):
o	Conditional script editor (for Conditional):
	Code editor with basic syntax highlighting.
	Interval (minutes).
•	Core Settings (only for CoreSettings type):
o	Map to CoreSettingsBlock (enabled, polling, auto_clear, sound, exit menu, etc.).
Buttons:
•	Save (draft).
•	Preview manifest.
•	Export.
•	Export to Dev Core (DevelopmentLocal only).
•	Publish (later used when Intune integration exists).
Role enforcement:
•	For Basic:
o	Type dropdown must not allow Conditional/Dynamic/CoreSettings creation.
o	If viewing those types, fields read-only.
10.3 Campaigns UI
•	List campaigns, with counts of modules and summary stats.
•	Campaign detail page:
o	Campaign info.
o	Table of modules in campaign (with metrics).
10.4 Reporting UI
•	Global dashboard:
o	Cards summarizing total toasts shown, total OK, total MoreInfo, etc.
o	Charts (bar/time series) per module and campaign.
•	Module detail metrics:
o	Start/expiry dates.
o	Author, type, category, campaign.
o	Aggregated counts (Shown, OK, MoreInfo, etc.).
o	Small event timeline.
________________________________________
11. Intune & Deployment Groups (Final Phase Only)
Important: Intune integration and Graph-based features must be implemented after all local export & reporting features are stable.
When implementing this phase:
11.1 Graph & Deployment Groups
•	Use Microsoft Graph to read Azure AD groups where displayName starts with WN-.
•	Populate/update DeploymentGroup table periodically or on demand.
•	In the UI, show Deployment Groups in a dedicated section and allow linking them as “intended targets” for modules/campaigns (metadata only until Intune wiring is done).
11.2 Intune Win32 Packaging & Publishing
Same design as original spec, but:
•	Use the exported module folder as source content.
•	Wrap module folder + install/uninstall scripts into .intunewin.
•	Create/update Win32 app via Graph.
•	Store IntuneAppBinding linking module to Intune app.
Assignments (group targeting) can use DeploymentGroup metadata to suggest which WN- groups should be assigned, but actual assignments may remain manual in Intune for v1.
________________________________________
12. Project Structure & Implementation Phases
Codex should implement in this order:
1.	Phase 1 – Backend Skeleton & Local Auth
o	Projects: API, Domain, Infrastructure, Frontend.
o	Database & migrations.
o	Local Development mode (DevelopmentLocal) with simple login & roles.
2.	Phase 2 – Modules & Campaigns
o	ModuleDefinition + Campaign entities & CRUD.
o	Manifest generator.
o	Basic React UI for listing and editing modules & campaigns.
3.	Phase 3 – File Storage & Export
o	Implement storage layout.
o	Implement /export and /export-dev-core endpoints.
o	Wire “Export” and “Export to Dev Core” buttons in UI.
4.	Phase 4 – Telemetry & Reporting
o	Telemetry ingestion endpoint with API key.
o	TelemetryEvent entity & storage.
o	Aggregated reporting APIs.
o	Dashboards in UI.
5.	Phase 5 – ProductionCloud Auth (Entra ID)
o	Add Entra ID login flow.
o	Map UPN to PortalUser.
o	Keep DevelopmentLocal mode fully functional.
6.	Phase 6 – Intune & Deployment Groups
o	Graph-based discovery of WN- groups → DeploymentGroup.
o	IntuneWin packaging & Win32 app creation/update.
o	UI for publish, showing linked Intune apps & Deployment Groups.

