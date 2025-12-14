import { apiGet, apiPostJson } from '../../core/apiClient';
import type { CoreSettingsBlock, ModuleRow, NotificationType } from '../../types';
import { apiPostForm } from '../../core/apiClient';

export type UpsertModuleRequest = {
  displayName: string;
  moduleId: string;
  type: NotificationType;
  category: string;
  title: string;
  message?: string;
  description?: string;
  linkUrl?: string;
  scheduleUtc?: string | null;
  expiresUtc?: string | null;
  reminderHours?: string;
  iconFileName?: string;
  soundEnabled?: boolean;
  conditionalScriptBody?: string | null;
  conditionalIntervalMinutes?: number | null;
  dynamicScriptBody?: string | null;
  dynamicMaxLength?: number | null;
  dynamicTrimWhitespace?: boolean | null;
  dynamicFailIfEmpty?: boolean | null;
  dynamicFallbackMessage?: string | null;
  heroFileName?: string | null;
  heroOriginalName?: string | null;
  coreSettings?: CoreSettingsBlock | null;
};

export async function getModules(apiBase: string, token: string): Promise<ModuleRow[]> {
  return apiGet<ModuleRow[]>(`${apiBase}/api/modules`, token);
}

export async function createModule(apiBase: string, token: string, payload: UpsertModuleRequest): Promise<ModuleRow> {
  return apiPostJson<UpsertModuleRequest, ModuleRow>(`${apiBase}/api/modules`, token, payload);
}

export async function uploadIcon(apiBase: string, token: string, moduleId: string, file: File) {
  const form = new FormData();
  form.append('file', file);
  return apiPostForm(`${apiBase}/api/modules/${moduleId}/icon`, token, form);
}

export function getIconUrl(apiBase: string, moduleId: string) {
  return `${apiBase}/api/modules/${moduleId}/icon`;
}

export async function uploadHero(apiBase: string, token: string, moduleId: string, file: File) {
  const form = new FormData();
  form.append('file', file);
  return apiPostForm(`${apiBase}/api/modules/${moduleId}/hero`, token, form);
}

export function getHeroUrl(apiBase: string, moduleId: string) {
  return `${apiBase}/api/modules/${moduleId}/hero`;
}

export async function exportDevCore(apiBase: string, token: string, moduleId: string) {
  return apiPostJson<null, any>(`${apiBase}/api/export/${moduleId}/devcore`, token, null as any);
}

export async function exportZip(apiBase: string, token: string, moduleId: string) {
  return apiPostJson<null, any>(`${apiBase}/api/export/${moduleId}/package`, token, null as any);
}
