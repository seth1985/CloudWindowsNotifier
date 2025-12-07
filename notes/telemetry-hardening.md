# Telemetry Ingest Hardening (Plan)

Current state (for context):
- Core posts to `/api/telemetry/events` over HTTP with a shared header key.
- No per-device identity, no rate limiting, no idempotency, direct DB writes.
- Suitable only for local/dev.

Target for production (20k devices):

## Transport / Network
- HTTPS only (enforce TLS 1.2+). If devices are on VPN, prefer a private DNS/endpoint restricted to VPN networks.
- If public-facing, use WAF/rate limits at the edge; consider private endpoints for App Service/VM if feasible.
- HSTS + TLS termination with modern ciphers.

## Auth / Identity
- Per-device or per-install credentials:
  - Option A: device key → exchange for short-lived JWT (deviceId, exp, audience).
  - Option B: Entra ID device auth if practical.
- Reject anonymous; rotate shared secrets if used during bootstrap.

## Payload Validation
- Require `moduleId` and validate it exists (or allow known pending).
- Add `eventId` (UUID) for idempotency; drop duplicates.
- Check `eventType` is one of: ToastShown, ButtonOk, ButtonMoreInfo, Dismissed, TimedOut, ScriptError, ConditionCheck, Completed.
- Enforce size limits and batch length; return 400 on malformed payloads.

## Rate Limiting / Abuse Controls
- Per-device/IP throttling on the ingest endpoint.
- Cap batch size and total payload size.
- Log and alert on spikes or repeated auth failures.

## Ingest Architecture
- Keep POST shape, but enqueue inside API:
  - API receives events → validates → writes to queue (Event Hub/Service Bus/Storage Queue).
  - Background worker/Function consumes queue → writes to TelemetryEvents table and aggregates.
- Benefits: handles spikes, decouples DB, improves resilience.

## Core Client Expectations
- Post over HTTPS to the configured endpoint.
- Include auth (JWT or per-device key) in headers.
- Batch events where possible; backoff/retry on failures (with idempotent eventId).
- Do not block UI on telemetry failures.

## Observability
- Metrics: ingestion rate, queue depth, error rates, auth failures.
- Alerts on sustained failures or drops.
- Audit logs on ingest and auth decisions.

## Next Steps (when implementing)
- Add per-device auth (JWT or key exchange) and require HTTPS.
- Add eventId/idempotency, validation, and rate limits.
- Wire API ingest to a queue; add worker to persist.
- Update Core to use the new auth and HTTPS endpoint; keep batching/backoff.
