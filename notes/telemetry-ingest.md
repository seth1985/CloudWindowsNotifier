# Telemetry Ingest Plan (Draft)

Goal: enable 20k deployed Cores to send interaction events back to the cloud API with reliability, without overloading the API. Implement later, but capture the plan now.

## Flow Overview
1) Core (device)
   - After showing/acting on a toast, POST a small JSON event to the API over HTTPS (batched where possible).
   - Event fields: deviceId, moduleId, eventType (shown, okClick, moreInfoClick, expired, dismissed), timestamps (UTC), appVersion, optional context (linkClicked).
   - Auth: per-device token (installer-provisioned key exchanged for short-lived JWT) or device code flow. Do not ship anonymous.
   - Retry with backoff if offline; batch multiple events per POST; don’t block UI.

2) API (Azure VM/App Service)
   - Expose `POST /api/telemetry/events` to accept an array of events.
   - Immediately enqueue to durable queue (Azure Event Hub/Service Bus/Storage Queue) for spike protection; return 202/200.
   - Minimal validation: schema + auth + size limits + rate-limit per device.

3) Worker/Function
   - Consume queue, write to telemetry store (SQL or NoSQL) and update aggregates.
   - Deduplicate via idempotency key (eventId) to handle retries.

4) Read Path
   - Keep `GET /api/reporting/summary` but back it with the telemetry store/aggregates.
   - Future: add per-module timelines, exports, filters.

## Transport & Payload
- HTTPS, gzip, small JSON; allow batching (array of events).
- Suggested envelope:
  ```json
  {
    "deviceId": "guid-or-hardware-hash",
    "appVersion": "1.0.x",
    "events": [
      {
        "eventId": "uuid",
        "moduleId": "module-12345-foo",
        "type": "okClick",
        "occurredUtc": "2025-12-01T12:34:56Z",
        "details": { "linkUrl": "https://..." }
      }
    ]
  }
  ```

## Scaling Notes
- Queue decouples 20k devices from API spikes.
- Rate-limit per device/IP; drop oversized batches.
- Backoff + retry in Core; cache locally if offline.
- Keep payloads small; avoid chatty per-click calls when batching is possible.

## Security
- TLS everywhere.
- Auth: per-device secret → short-lived JWT; rotate tokens.
- Validate moduleId belongs to issuer (optional later).

## Action Items (when we implement)
- Add ingest endpoint in API, queue producer, and a background consumer to write telemetry.
- Add client-side batching/retry in Core.
- Point summary endpoint at stored aggregates instead of stub data.
