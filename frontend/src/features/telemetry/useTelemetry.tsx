import { useState } from 'react';
import type { TelemetrySummary, TelemetryPerModule } from '../../types';
import { getTelemetry } from './api';

export function useTelemetry(apiBase: string, token: string | null, setGlobalStatus: (s: string) => void) {
  const [summary, setSummary] = useState<TelemetrySummary | null>(null);
  const [perModule, setPerModule] = useState<TelemetryPerModule[]>([]);
  const [loading, setLoading] = useState(false);

  const loadTelemetry = async () => {
    if (!token) {
      setGlobalStatus('Login first.');
      return;
    }
    try {
      setLoading(true);
      setGlobalStatus('Loading telemetry...');
      const data = await getTelemetry(apiBase, token);
      setSummary(data.summary);
      setPerModule(data.perModule);
      setGlobalStatus('Telemetry loaded.');
    } catch (err: any) {
      setGlobalStatus(err?.message ?? 'Error loading telemetry.');
    } finally {
      setLoading(false);
    }
  };

  return { summary, perModule, loading, loadTelemetry };
}
