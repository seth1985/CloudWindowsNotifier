import React from 'react';
import { TelemetryChart } from './TelemetryChart';
import { formatDate } from '../../../lib/utils';
import { MetricTile } from './MetricTile';
import {
  Activity,
  BarChart3,
  RefreshCw,
  Table,
  MousePointer2,
  Info,
  CheckCircle2,
  Calendar,
  AlertCircle
} from 'lucide-react';
import type { TelemetrySummary, TelemetryPerModule } from '../../../types';
import { cn } from '../../../lib/utils';

type Props = {
  summary: TelemetrySummary | null;
  perModule: TelemetryPerModule[];
  loading: boolean;
  onRefresh: () => void;
};

export const TelemetryOverview: React.FC<Props> = ({ summary, perModule, loading, onRefresh }) => {
  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
      {/* Dashboard Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6 pb-6 border-b border-border/50">
        <div className="flex items-center gap-4">
          <div className="p-3 bg-primary-main/10 text-primary-main rounded-2xl border border-primary-main/20 shadow-sm">
            <Activity className="w-6 h-6" />
          </div>
          <div className="flex flex-col">
            <h3 className="text-2xl font-black text-text-primary tracking-tighter uppercase">
              Telemetry <span className="text-primary-main">Insights</span>
            </h3>
            <p className="text-xs font-bold text-text-tertiary uppercase tracking-widest mt-0.5">
              Real-time monitoring & engagement metrics
            </p>
          </div>
        </div>
        <button
          onClick={onRefresh}
          disabled={loading}
          className="btn btn-primary px-6 h-12 gap-3 shadow-lg shadow-primary-main/20"
        >
          <RefreshCw className={cn("w-4 h-4", loading && "animate-spin")} />
          <span>Refresh Data</span>
        </button>
      </div>

      {!summary ? (
        <div className="flex flex-col items-center justify-center py-24 bg-surface-chip/10 rounded-3xl border border-dashed border-border gap-6">
          <div className="p-5 bg-card rounded-full shadow-xl border border-border">
            <BarChart3 className="w-10 h-10 text-text-tertiary opacity-40" />
          </div>
          <div className="text-center space-y-2">
            <p className="text-xl font-black text-text-primary uppercase tracking-tight">No Telemetry Signal</p>
            <p className="text-sm text-text-tertiary max-w-[280px] mx-auto font-medium">Click the refresh button to synchronize latest interactions from the logic layer.</p>
          </div>
          <button onClick={onRefresh} className="btn btn-secondary px-8 h-11">Initialize Fetch</button>
        </div>
      ) : (
        <div className="space-y-8">
          {/* Main Metrics Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <MetricTile
              label="Toasts Shown"
              value={summary.toastShown ?? 0}
              icon={BarChart3}
              color="primary"
            />
            <MetricTile
              label="Primary Actions"
              value={summary.buttonOk ?? 0}
              icon={CheckCircle2}
              color="success"
            />
            <MetricTile
              label="Learn More Click"
              value={summary.buttonMoreInfo ?? 0}
              icon={Info}
              color="info"
            />
            <MetricTile
              label="Logic Completed"
              value={summary.completed ?? 0}
              icon={MousePointer2}
              color="warning"
            />
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Chart Container */}
            <div className="lg:col-span-2 card p-6 space-y-6">
              <div className="flex items-center justify-between">
                <h4 className="flex items-center gap-2 text-[10px] font-black text-text-secondary uppercase tracking-[0.2em]">
                  <Activity className="w-3.5 h-3.5 text-primary-main" />
                  Interaction Trend
                </h4>
                <div className="flex items-center gap-4">
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-primary-main" />
                    <span className="text-[10px] font-bold text-text-tertiary uppercase italic">Shown</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-green-500" />
                    <span className="text-[10px] font-bold text-text-tertiary uppercase italic">Clicked</span>
                  </div>
                </div>
              </div>
              <TelemetryChart />
            </div>

            {/* Range Info Case */}
            <div className="space-y-4">
              <div className="card p-6 border-l-4 border-l-primary-main">
                <div className="flex items-center gap-3 mb-4">
                  <Calendar className="w-4 h-4 text-primary-main" />
                  <span className="text-[10px] font-black text-text-secondary uppercase tracking-[0.2em]">Date Range</span>
                </div>
                <div className="space-y-4">
                  <div>
                    <p className="text-[10px] font-bold text-text-tertiary uppercase tracking-widest leading-none mb-1">Observation Started</p>
                    <p className="text-sm font-black text-text-primary tracking-tight">{formatDate(summary.rangeStartUtc)}</p>
                  </div>
                  <div className="w-full h-px bg-border/50" />
                  <div>
                    <p className="text-[10px] font-bold text-text-tertiary uppercase tracking-widest leading-none mb-1">Latest Pulse Recorded</p>
                    <p className="text-sm font-black text-text-primary tracking-tight">{formatDate(summary.rangeEndUtc)}</p>
                  </div>
                </div>
              </div>

              <div className="card p-6 bg-surface-chip/30 border-dashed">
                <div className="flex items-center gap-3 mb-3 text-text-tertiary">
                  <AlertCircle className="w-4 h-4" />
                  <span className="text-[10px] font-black uppercase tracking-[0.2em]">System Note</span>
                </div>
                <p className="text-[11px] font-medium text-text-tertiary leading-relaxed italic">
                  Telemetry is captured asynchronously from distributed endpoints.
                  Data may take up to 300 seconds to synchronize with the cloud controller.
                </p>
              </div>
            </div>
          </div>

          {/* Module Detailed Table */}
          <div className="space-y-4 pt-4">
            <div className="flex items-center gap-3">
              <Table className="w-5 h-5 text-text-tertiary" />
              <h4 className="text-lg font-black text-text-primary tracking-tight uppercase">Per-Module Breakdown</h4>
            </div>

            <div className="card overflow-hidden border-border/60 shadow-xl">
              <div className="overflow-x-auto no-scrollbar">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="bg-background/50 border-b border-border">
                      <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Display Name</th>
                      <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Type</th>
                      <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Shown</th>
                      <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">OK</th>
                      <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Learn</th>
                      <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Complete</th>
                      <th className="px-6 py-5 font-black text-text-secondary uppercase tracking-[0.15em] text-[10px]">Last Seen</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border/50">
                    {perModule.length === 0 ? (
                      <tr>
                        <td colSpan={7} className="px-6 py-12 text-center">
                          <p className="text-xs font-bold text-text-tertiary uppercase tracking-widest italic">No discrete module data available</p>
                        </td>
                      </tr>
                    ) : (
                      perModule.map((m) => (
                        <tr key={m.moduleId} className="group hover:bg-primary-main/[0.02] transition-colors">
                          <td className="px-6 py-4">
                            <div className="flex flex-col">
                              <span className="text-sm font-bold text-text-primary group-hover:text-primary-main transition-colors">{m.displayName || 'Unknown Module'}</span>
                              <span className="text-[10px] font-mono text-text-tertiary uppercase tracking-wider">{m.moduleId}</span>
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <span className="px-2 py-1 rounded-md bg-surface-chip text-[10px] font-black uppercase text-text-secondary border border-border">
                              {m.type || '—'}
                            </span>
                          </td>
                          <td className="px-6 py-4 font-black text-text-secondary">{m.toastShown ?? 0}</td>
                          <td className="px-6 py-4 font-black text-green-500/80">{m.buttonOk ?? 0}</td>
                          <td className="px-6 py-4 font-black text-blue-500/80">{m.buttonMoreInfo ?? 0}</td>
                          <td className="px-6 py-4 font-black text-orange-500/80">{m.completed ?? 0}</td>
                          <td className="px-6 py-4 text-[11px] font-bold text-text-tertiary tabular-nums">
                            {formatDate(m.lastEventUtc)}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
