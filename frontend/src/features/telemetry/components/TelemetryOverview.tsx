import React from 'react';
import type { TelemetrySummary, TelemetryPerModule } from '../../types';

function formatDate(val: any) {
  if (!val) return '—';
  const d = new Date(val);
  if (isNaN(d.getTime())) return '—';
  return d.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
}

const MetricTile: React.FC<{ label: string; value: any; small?: boolean }> = ({ label, value, small }) => (
  <div className={`metric ${small ? 'small' : ''}`}>
    <div className="metric-label">{label}</div>
    <div className="metric-value">{value ?? '—'}</div>
  </div>
);

type Props = {
  summary: TelemetrySummary | null;
  perModule: TelemetryPerModule[];
  loading: boolean;
  onRefresh: () => void;
};

export const TelemetryOverview: React.FC<Props> = ({ summary, perModule, loading, onRefresh }) => {
  return (
    <section className="card stack">
      <div className="card-head">
        <div>
          <h3>Telemetry Overview</h3>
          <p>Quick glance at recent interactions.</p>
        </div>
        <div className="actions">
          <button onClick={onRefresh} disabled={loading}>
            Refresh telemetry
          </button>
        </div>
      </div>
      {summary ? (
        <>
          <div className="telemetry-grid">
            <MetricTile label="Toasts Shown" value={summary.toastShown ?? '—'} />
            <MetricTile label="Button OK" value={summary.buttonOk ?? '—'} />
            <MetricTile label="Learn More" value={summary.buttonMoreInfo ?? '—'} />
            <MetricTile label="Completed" value={summary.completed ?? '—'} />
            <MetricTile label="First Seen" value={formatDate(summary.rangeStartUtc)} small />
            <MetricTile label="Last Seen" value={formatDate(summary.rangeEndUtc)} small />
          </div>
          <div className="chart-placeholder">Trend chart placeholder</div>
          <div className="table-actions spaced-top">
            <h4>Per-module telemetry</h4>
            <div className="module-telemetry-table">
              <table>
                <thead>
                  <tr>
                    <th>Module</th>
                    <th>Name</th>
                    <th>Type</th>
                    <th>Shown</th>
                    <th>OK</th>
                    <th>Learn</th>
                    <th>Completed</th>
                    <th>First Seen</th>
                    <th>Last Seen</th>
                  </tr>
                </thead>
                <tbody>
                  {perModule.length === 0 && (
                    <tr>
                      <td colSpan={9} className="empty">No telemetry yet.</td>
                    </tr>
                  )}
                  {perModule.map((m) => (
                    <tr key={m.moduleId}>
                      <td>{m.moduleId}</td>
                      <td>{m.displayName ?? '—'}</td>
                      <td>{m.type ?? '—'}</td>
                      <td>{m.toastShown ?? 0}</td>
                      <td>{m.buttonOk ?? 0}</td>
                      <td>{m.buttonMoreInfo ?? 0}</td>
                      <td>{m.completed ?? 0}</td>
                      <td>{formatDate(m.firstSeen)}</td>
                      <td>{formatDate(m.lastSeen)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      ) : (
        <div className="empty">Telemetry not loaded. Click Refresh telemetry to fetch.</div>
      )}
    </section>
  );
};
