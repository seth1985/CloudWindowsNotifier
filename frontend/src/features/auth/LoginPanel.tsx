import React from 'react';

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
    <section className="grid two">
      <div className="card stack">
        <div className="card-head">
          <div>
            <h3>Login Panel</h3>
            <p>Connect to your API and authenticate.</p>
          </div>
        </div>
        <div className="row">
          <label>API base</label>
          <input value={apiBase} onChange={(e) => setApiBase(e.target.value)} />
        </div>
        <div className="row">
          <label>Username</label>
          <input value={username} onChange={(e) => setUsername(e.target.value)} />
        </div>
        <div className="row">
          <label>Password</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </div>
        <div className="actions">
          <button className="btn primary" onClick={onLogin} disabled={loading}>
            Login (DevLocal)
          </button>
        </div>
        <div className={`status ${status ? 'visible' : ''}`}>{status}</div>
        <small className="hint">
          API base auto-fills from the last value. Ensure the API is running on that host/port.
        </small>
      </div>
    </section>
  );
};
