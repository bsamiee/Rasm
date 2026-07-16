# [TS_RUNTIME]

`runtime` is the branch's execution substrate — one body across its altitudes: the process plane where a runtime is a table row, the distributed plane where egress, transport, fanout, coordination, durable work, and the intelligence spine compose one budget ledger and one fault law, and the browser condition, the same package under one build flag, never a sibling. Its bar is structural singularity: one circuit ledger every dial inherits, one ambient redaction scrub at every capture seam, one `WorkClass` economy pricing every durable surface, one assembled front door no lib can hold itself — every capability, whether runtime, lane, provider, channel, cadence, or vital, is a row, and only the boot module reads a concrete one. Faults leave only as self-rendering values, byte-stable renders and embedding fingerprints hold across processes and languages, and degradation is a readable `Layer` choice, never hidden behavior.

It imports core, security, and data and is composed by the interface and deploy planes: security satisfies its guard and intake seams into the front door, data owns the outbox, mailbox, and journal its durable work composes, the deploy plane's outputs arrive only as typed env facts, and the C# host pushes telemetry through its OTLP ingress. It mints no content identity and holds no record of truth of its own.

## [01]-[ROUTER]

- [01]-[PROC](.planning/proc/): Runtime binding, resolve-once config, flags-as-data, lifecycle receipts, and one worker protocol over platform Tags.
- [02]-[NET](.planning/net/): One HTTP lane table off the core budget, framed byte channels, and engine-blind fanout, replay, and coordination.
- [03]-[OTEL](.planning/otel/): OTLP wire half under one redaction scrub — egress, ingress continuation, the crash fold, and RUM vitals.
- [04]-[SERVE](.planning/serve/): Libs export route, verb, and group data; the app assembles one `HttpApi` and CLI root; faults leave as `Problem`s.
- [05]-[WORK](.planning/work/): One `WorkClass` economy — sharded actors, replay workflows, durable queues with the sole DLQ owner, cluster cron.
- [06]-[AI](.planning/ai/): Provider families on one capability-asymmetry table with ranked fallback, chunk-and-embed, and Schema-typed MCP tools.
- [07]-[BROWSER](.planning/browser/): One boot graph per document, the PWA shell, IndexedDB vocabulary, and the router carrying `Vault`.

## [02]-[DOMAIN_PACKAGES]

Runtime-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[DISTRIBUTION]:
- `@effect/cluster`
- `@effect/workflow`
- `@effect/rpc`
- `@nats-io/nats-core`
- `@nats-io/jetstream`
- `@nats-io/kv`
- `@nats-io/obj`

[INTELLIGENCE]:
- `@effect/ai`
- `@effect/ai-anthropic`
- `@effect/ai-openai`
- `@effect/ai-google`
- `@effect/ai-amazon-bedrock`
- `@effect/ai-openrouter`
- `@modelcontextprotocol/sdk`

[TELEMETRY]:
- `@opentelemetry/api-logs`
- `@opentelemetry/core`
- `@opentelemetry/resources`
- `@opentelemetry/sdk-logs`
- `@opentelemetry/sdk-metrics`
- `@opentelemetry/sdk-trace-base`
- `@opentelemetry/sdk-trace-node`
- `@opentelemetry/sdk-trace-web`
- `@opentelemetry/exporter-trace-otlp-http`
- `@opentelemetry/exporter-metrics-otlp-http`
- `@opentelemetry/exporter-logs-otlp-http`

[TERMINAL]:
- `@effect/cli`
- `@effect/printer`
- `@effect/printer-ansi`

[FLAGS]:
- `@openfeature/server-sdk`

[DOCUMENTS]:
- `nodemailer`
- `@types/nodemailer`
- `exceljs`
- `jspdf`
- `jszip`
- `papaparse`
- `@types/papaparse`

[BROWSER_SHELL]:
- `workbox-build`
- `workbox-window`
- `idb-keyval`
- `nuqs`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/platform-browser`
- `@effect/experimental`

[OTLP_BRIDGE]:
- `@effect/opentelemetry`
