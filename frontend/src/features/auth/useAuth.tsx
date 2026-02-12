import { useEffect, useState } from 'react';
import { getAuthConfig, login, type AuthConfigResponse } from './api';
import type { UserRole } from '../../types';
import { loginWithEntra } from './entraAuth';

type AuthProvider = 'Local' | 'Entra';

export function useAuth(initialApiBase: string) {
  const [apiBase, setApiBase] = useState(initialApiBase);
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('P@ssw0rd!');
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<UserRole>('Admin'); // Default to Admin for now to unblock UI
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState('');
  const [authProvider, setAuthProvider] = useState<AuthProvider>('Local');
  const [entraConfig, setEntraConfig] = useState<AuthConfigResponse['entra'] | null>(null);

  const decodeRole = (jwt: string): UserRole => {
    try {
      const payload = JSON.parse(atob(jwt.split('.')[1] || ''));
      const rolesRaw = payload.roles;
      const roles = Array.isArray(rolesRaw)
        ? rolesRaw.map((r) => String(r).toLowerCase())
        : [];
      const singleClaim = String(
        payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || ''
      ).toLowerCase();

      if (roles.includes('admin') || singleClaim === 'admin') return 'Admin';
      if (roles.includes('advanced') || singleClaim === 'advanced') return 'Advanced';
      return 'Standard';
    } catch {
      return 'Standard';
    }
  };

  const handleLogin = async () => {
    try {
      setLoading(true);
      setStatus('Logging in...');
      if (authProvider === 'Entra') {
        if (!entraConfig?.spaClientId || !(entraConfig?.authority || entraConfig?.tenantId) || !entraConfig?.scope) {
          throw new Error('Entra configuration is incomplete. Set Entra settings in API appsettings.');
        }

        const authority = entraConfig.authority || `https://login.microsoftonline.com/${entraConfig.tenantId}/v2.0`;
        const token = await loginWithEntra({
          clientId: entraConfig.spaClientId,
          authority,
          scope: entraConfig.scope
        });
        setToken(token);
        setRole(decodeRole(token));
        setStatus('Authenticated with Microsoft Entra.');
        return;
      }

      const res = await login(apiBase, username, password);
      setToken(res.token);
      setRole(decodeRole(res.token));
      setStatus(`Logged in as ${username}`);
    } catch (err: any) {
      setToken(null);
      setRole('Standard');
      setStatus(err?.message ?? 'Login failed.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let cancelled = false;

    const loadAuthConfig = async () => {
      try {
        const cfg = await getAuthConfig(apiBase);
        if (cancelled) return;
        const provider: AuthProvider = cfg.provider === 'Entra' ? 'Entra' : 'Local';
        setAuthProvider(provider);
        setEntraConfig(cfg.entra ?? null);
      } catch {
        if (!cancelled) {
          setAuthProvider('Local');
          setEntraConfig(null);
        }
      }
    };

    loadAuthConfig();
    return () => {
      cancelled = true;
    };
  }, [apiBase]);

  return {
    apiBase,
    setApiBase,
    username,
    setUsername,
    password,
    setPassword,
    token,
    role,
    setRole,
    setToken,
    loading,
    status,
    setStatus,
    authProvider,
    handleLogin
  };
}
