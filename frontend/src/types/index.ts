export type UserRole = 'Standard' | 'Advanced' | 'Admin';

export type User = {
  id: string;
  displayName: string;
  email: string;
  role: UserRole;
  avatarUrl?: string;
  lastLogin?: string;
};

export type NotificationType = 'Standard' | 'Conditional' | 'Dynamic' | 'Hero' | 'CoreSettings';

export type ModuleRow = {
  id: string;
  displayName: string; // stored name
  moduleId: string;
  type: NotificationType;
  category: string;
  description?: string;
  version: number;
  isPublished: boolean;
  iconFileName?: string | null;
  iconOriginalName?: string | null;
  heroFileName?: string | null;
  heroOriginalName?: string | null;
};

export type CoreSettingsBlock = {
  enabled?: number;
  pollingIntervalSeconds?: number;
  autoClearModules?: number;
  soundEnabled?: number;
  exitMenuVisible?: number;
  startStopMenuVisible?: number;
  heartbeatSeconds?: number;
};

export type TemplateType = 'Conditional' | 'Dynamic' | 'Both';

export type PowerShellTemplate = {
  id: string;
  title: string;
  description?: string;
  category: string;
  scriptBody: string;
  type: TemplateType;
  createdUtc?: string;
  createdBy?: string | null;
};

export type TelemetrySummary = {
  totalEvents?: number;
  toastShown?: number;
  buttonOk?: number;
  buttonMoreInfo?: number;
  dismissed?: number;
  timedOut?: number;
  scriptError?: number;
  conditionCheck?: number;
  completed?: number;
  rangeStartUtc?: string;
  rangeEndUtc?: string;
};

export type TelemetryPerModule = {
  moduleId: string;
  displayName?: string;
  type?: string;
  category?: string;
  toastShown?: number;
  buttonOk?: number;
  buttonMoreInfo?: number;
  dismissed?: number;
  timedOut?: number;
  scriptError?: number;
  conditionCheck?: number;
  completed?: number;
  firstEventUtc?: string;
  lastEventUtc?: string;
};
