import { apiPostJson } from '../../core/apiClient';

export type AuthResponse = {
  token: string;
};

export async function login(apiBase: string, username: string, password: string): Promise<AuthResponse> {
  return apiPostJson<{ username: string; password: string }, AuthResponse>(
    `${apiBase}/api/auth/login`,
    null,
    { username, password }
  );
}
