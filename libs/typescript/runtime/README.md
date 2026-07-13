# [TS_RUNTIME]

`libs/typescript/runtime` owns the branch's execution substrate across both process planes and its own browser condition: every runtime binding, outbound and framed transport lane, telemetry export, served surface, durable work unit, and intelligence call folds through one arity whose engines, providers, and channels are table rows. Owners meet the core, security, and data peers only through seam contracts, minting no content identity and holding no record of truth of their own.

## [01]-[ROUTER]

[PROC]:
- [01]-[EXEC](.planning/proc/exec.md): Keyed `node|bun` runtime-row binding table minting child processes as declarative values.
- [02]-[CONFIG](.planning/proc/config.md): Ordered provider chain resolving the boot-validated `Setting` contract once.
- [03]-[FLAG](.planning/proc/flag.md): OpenFeature server `Provider` — a recursive rule family over content-key bucketing.
- [04]-[LIFE](.planning/proc/life.md): Ranked lifecycle and health rows folded off severed fibers into one graded receipt.
- [05]-[WORKER](.planning/proc/worker.md): Off-thread worker protocol carrying zero-copy crossings over one pool.

[NET]:
- [06]-[CLIENT](.planning/net/client.md): Outbound HTTP lane table — status admission and retry pulses off the core budget.
- [07]-[CHANNEL](.planning/net/channel.md): Framed long-lived byte channels — socket duplex and SSE feeds over one frame vocabulary.
- [08]-[PUBSUB](.planning/net/pubsub.md): Engine-blind broadcast, replay, and blob fanout over one `Broker` port.
- [09]-[COORDINATE](.planning/net/coordinate.md): Engine-blind lease, elect, and CAS coordination port.

[OTEL]:
- [10]-[EMIT](.planning/otel/emit.md): One OTLP egress `Layer` and collector ingress under the redaction scrub.
- [11]-[CRASH](.planning/otel/crash.md): Total `Cause`-to-fatal-emission fold through the core forensic fault band.
- [12]-[VITAL](.planning/otel/vital.md): Six RUM vital rows over one scoped `PerformanceObserver` bridge.

[SERVE]:
- [13]-[API](.planning/serve/api.md): Assembly law — sub-domains export group data, the app assembles one `HttpApi`.
- [14]-[ROUTE](.planning/serve/route.md): `HttpLayerRouter` serving fold — api mount, upload dispatch, and intake verify.
- [15]-[LIVE](.planning/serve/live.md): Realtime SSE/WS serving over branch feeds under the resume-token and admission laws.
- [16]-[PROBLEM](.planning/serve/problem.md): RFC 9457 `Problem` owner rendering itself as a self-describing response.
- [17]-[CLI](.planning/serve/cli.md): Command-value verb families folded into one root.

[WORK]:
- [18]-[ENTITY](.planning/work/entity.md): Durable-actor plane — the `WorkClass` service-class table over tiered mailboxes.
- [19]-[FLOW](.planning/work/flow.md): Workflow suspend-and-replay — minted steps, two-tier deadlines, one durable pause timer.
- [20]-[QUEUE](.planning/work/queue.md): `DurableQueue` families and rate-limiter throttles over the pg lane policy and DLQ fold.
- [21]-[SCHEDULE](.planning/work/schedule.md): Cadence rows minted into cluster cron with misfire and catch-up postures.
- [22]-[DELIVER](.planning/work/deliver.md): One channel table for mail and webhook egress — one receipt, one fault, one suppression.
- [23]-[REPORT](.planning/work/report.md): Report specs folded through three engine arms over the same decoded rows.

[AI]:
- [24]-[MODEL](.planning/ai/model.md): Five provider families on one capability-asymmetry table with ranked fallback.
- [25]-[EMBED](.planning/ai/embed.md): Deterministic chunking and embedding rows satisfying the data retrieval ports.
- [26]-[TOOL](.planning/ai/tool.md): Schema-typed tools and toolkit assembly across both MCP lanes under one safety owner.
- [27]-[AGENT](.planning/ai/agent.md): Agent altitude — transition-machine sessions with persisted-chat durability.

[BROWSER]:
- [28]-[BOOT](.planning/browser/boot.md): Single-boot law — the app-spec budget, connect cells, and the capability roster.
- [29]-[SHELL](.planning/browser/shell.md): PWA shell — the manifest as a typed value under a scoped resource and update handshake.
- [30]-[PERSIST](.planning/browser/persist.md): IndexedDB domain vocabulary with batch read and write modalities.
- [31]-[ROUTE_BROWSER](.planning/browser/route.md): Navigation-API typed router carrying the `Vault` session plane and admission fold.
- [32]-[FETCH](.planning/browser/fetch.md): Browser byte transport — XHR, WebSocket, and worker binding rows for verified arrivals.

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
