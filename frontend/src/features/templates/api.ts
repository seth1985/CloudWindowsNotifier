import { apiGet, apiPostJson } from '../../core/apiClient';
import type { PowerShellTemplate, TemplateType } from '../../types';

export async function getTemplates(apiBase: string, token: string, mode: 'conditional' | 'dynamic'): Promise<PowerShellTemplate[]> {
  const url = `${apiBase}/api/templates?type=${mode}`;
  return apiGet<PowerShellTemplate[]>(url, token);
}

export async function createTemplate(
  apiBase: string,
  token: string,
  payload: {
    title: string;
    description: string | null;
    category: string;
    type: TemplateType;
    scriptBody: string;
  }
): Promise<PowerShellTemplate> {
  return apiPostJson<typeof payload, PowerShellTemplate>(`${apiBase}/api/templates`, token, payload);
}
