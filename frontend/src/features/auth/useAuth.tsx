import { useState } from 'react';
import { login } from './api';

export function useAuth(initialApiBase: string) {
  const [apiBase, setApiBase] = useState(initialApiBase);
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('P@ssw0rd!');
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<'Basic' | 'Advanced'>('Basic');
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState('');

  const decodeRole = (jwt: string): 'Basic' | 'Advanced' => {
    try {
      const payload = JSON.parse(atob(jwt.split('.')[1] || ''));
      const claim = payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      return claim === 'Advanced' ? 'Advanced' : 'Basic';
    } catch {
      return 'Basic';
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
      setRole('Basic');
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
