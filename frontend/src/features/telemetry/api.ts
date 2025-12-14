import { apiGet } from '../../core/apiClient';
import type { TelemetrySummary, TelemetryPerModule } from '../../types';

export type TelemetryResponse = {
  summary: TelemetrySummary;
  perModule: TelemetryPerModule[];
};

export async function getTelemetry(apiBase: string, token: string): Promise<TelemetryResponse> {
  const summary = await apiGet<TelemetrySummary>(`${apiBase}/api/reporting/summary`, token);
  const perModule = await apiGet<TelemetryPerModule[]>(`${apiBase}/api/reporting/modules`, token);
  return { summary, perModule };
}
