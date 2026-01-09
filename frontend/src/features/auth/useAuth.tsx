import { useState } from 'react';
import { login } from './api';
import type { UserRole } from '../../types';

export function useAuth(initialApiBase: string) {
  const [apiBase, setApiBase] = useState(initialApiBase);
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('P@ssw0rd!');
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<UserRole>('Admin'); // Default to Admin for now to unblock UI
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState('');

  const decodeRole = (jwt: string): UserRole => {
    try {
      const payload = JSON.parse(atob(jwt.split('.')[1] || ''));
      const claim = String(payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '').toLowerCase();
      if (claim === 'admin') return 'Admin';
      if (claim === 'advanced') return 'Advanced';
      return 'Standard';
    } catch {
      return 'Standard';
    }
  };

  const handleLogin = async () => {
    try {
      setLoading(true);
      setStatus('Logging in...');
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
    handleLogin
  };
}
