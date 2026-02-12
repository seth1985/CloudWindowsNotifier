import { apiPostJson } from '../../core/apiClient';

export type AuthResponse = {
  token: string;
};

export type AuthConfigResponse = {
  provider: 'Local' | 'Entra';
  entra?: {
    tenantId?: string;
    authority?: string;
    spaClientId?: string;
    scope?: string;
  };
};

export async function login(apiBase: string, username: string, password: string): Promise<AuthResponse> {
  return apiPostJson<{ username: string; password: string }, AuthResponse>(
    `${apiBase}/api/auth/login`,
    null,
    { username, password }
  );
}

export async function getAuthConfig(apiBase: string): Promise<AuthConfigResponse> {
  const res = await fetch(`${apiBase}/api/config/auth`);
  if (!res.ok) {
    throw new Error(`GET ${apiBase}/api/config/auth failed (${res.status})`);
  }
  return res.json();
}
