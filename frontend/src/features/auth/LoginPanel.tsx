import React from 'react';
import { Shield, Globe, Lock, User, Terminal } from 'lucide-react';

type Props = {
  apiBase: string;
  setApiBase: (v: string) => void;
  username: string;
  setUsername: (v: string) => void;
  password: string;
  setPassword: (v: string) => void;
  loading: boolean;
  status: string;
  onLogin: () => void;
};

export const LoginPanel: React.FC<Props> = ({
  apiBase,
  setApiBase,
  username,
  setUsername,
  password,
  setPassword,
  loading,
  status,
  onLogin
}) => {
  return (
    <div className="min-h-screen flex items-center justify-center relative overflow-hidden bg-[#0e1a2d]">
      {/* Animated Background Blobs */}
      <div className="absolute top-0 -left-4 w-72 h-72 bg-primary-main mix-blend-multiply filter blur-3xl opacity-20 animate-blob"></div>
      <div className="absolute top-0 -right-4 w-72 h-72 bg-purple-500 mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-2000"></div>
      <div className="absolute -bottom-8 left-20 w-72 h-72 bg-pink-500 mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-4000"></div>

      <div className="w-full max-w-md p-8 relative z-10">
        <div className="glass rounded-2xl p-8 space-y-8">
          <div className="text-center space-y-2">
            <div className="inline-flex items-center justify-center p-3 rounded-xl bg-primary-soft text-primary-main mb-2">
              <Shield className="w-8 h-8" />
            </div>
            <h2 className="text-3xl font-bold tracking-tight text-white">Notifier Cloud</h2>
            <p className="text-text-tertiary">Enterprise notification management</p>
          </div>

          <div className="space-y-6">
            <div className="space-y-2">
              <label className="text-sm font-medium text-text-tertiary ml-1 flex items-center gap-2">
                <Globe className="w-3.5 h-3.5" /> API Endpoint
              </label>
              <div className="relative group">
                <input
                  value={apiBase}
                  onChange={(e) => setApiBase(e.target.value)}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder:text-white/20 focus:ring-2 focus:ring-primary-main focus:border-transparent outline-none transition-all"
                  placeholder="http://localhost:5210"
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-text-tertiary ml-1 flex items-center gap-2">
                <User className="w-3.5 h-3.5" /> Username
              </label>
              <div className="relative group">
                <input
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder:text-white/20 focus:ring-2 focus:ring-primary-main focus:border-transparent outline-none transition-all"
                  placeholder="admin"
                />
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium text-text-tertiary ml-1 flex items-center gap-2">
                <Lock className="w-3.5 h-3.5" /> Password
              </label>
              <div className="relative group">
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white placeholder:text-white/20 focus:ring-2 focus:ring-primary-main focus:border-transparent outline-none transition-all"
                  placeholder="••••••••"
                />
              </div>
            </div>

            <button
              onClick={onLogin}
              disabled={loading}
              className="w-full bg-primary-main hover:bg-primary-strong disabled:bg-primary-main/50 text-white font-semibold py-4 rounded-xl shadow-lg shadow-primary-main/20 transition-all active:scale-[0.98] flex items-center justify-center gap-2"
            >
              {loading ? (
                <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
              ) : (
                <>
                  <Terminal className="w-5 h-5" />
                  <span>Authenticate Session</span>
                </>
              )}
            </button>
          </div>

          {status && (
            <div className="p-4 rounded-xl bg-red-500/10 border border-red-500/20 text-red-400 text-sm text-center animate-in fade-in slide-in-from-top-2">
              {status}
            </div>
          )}

          <div className="pt-4 border-t border-white/5">
            <p className="text-[11px] text-center text-text-tertiary leading-relaxed">
              API base auto-fills from the last value.<br />
              Ensure the backend is active at the specified endpoint.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
