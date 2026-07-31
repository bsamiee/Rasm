# [TS_RUNTIME]

`runtime` is the branch's execution substrate — one body across the process plane, the distributed plane, and the browser condition, one package under one build flag. Every capability is a row, and only the boot module reads a concrete one.

## [01]-[ROUTER]

- [01]-[PROC](.planning/proc/): Process substrate every plane boots on — runtime rows, resolve-once config, data-driven flags, one worker protocol.
- [02]-[NET](.planning/net/): Outbound egress — HTTP lanes off the core budget, framed byte channels, engine-blind fanout and coordination ports.
- [03]-[OTEL](.planning/otel/): OTLP wire half of observability — sole egress under the ambient redaction scrub; vocabulary stays core's.
- [04]-[SERVE](.planning/serve/): Libs export route, verb, and group data; the app assembles one `HttpApi` and CLI root; faults leave as `Problem`s.
- [05]-[WORK](.planning/work/): `WorkClass` economy pricing every durable surface — actors, workflows, queues under the sole DLQ owner, cron.
- [06]-[AI](.planning/ai/): Provider families on one capability-asymmetry table with ranked fallback, chunk-and-embed, and Schema-typed MCP tools.
- [07]-[BROWSER](.planning/browser/): Browser condition — one boot graph per document, the PWA shell, the typed router carrying `Vault`.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[DISTRIBUTION]:
- `@effect/cluster`
- `@effect/workflow`
- `@effect/rpc`
- `@nats-io/nats-core`
- `@nats-io/transport-node` — native TCP/TLS `connect` for the node/bun lane; the browser lane rides `wsconnect`.
- `@nats-io/jetstream`
- `@nats-io/kv`
- `@nats-io/obj`
- `@confluentinc/kafka-javascript` — librdkafka client backing the Kafka broker engine row on the shared C# broker plane.
- `@confluentinc/schemaregistry` — Kafka schema identity, compatibility admission, subject resolution, and codec framing.
- `@connectrpc/connect-node` — Node Connect/gRPC transport factories; `net/client.md` owns transport dispatch.
- `mqtt` — `net/channel.md` owns the MQTT v5 channel seam.

[INTELLIGENCE]:
- `@effect/ai`
- `@effect/ai-anthropic`
- `@effect/ai-openai`
- `@effect/ai-google`
- `@effect/ai-amazon-bedrock`
- `@effect/ai-openrouter`
- `@modelcontextprotocol/sdk`

[TELEMETRY]:
- `@effect/opentelemetry` — Effect-signal bridge both export lanes compose; imported nowhere outside this folder.
- `@opentelemetry/api`
- `@opentelemetry/api-logs` — peer contract the sdk-logs processors type against; log records mint through `Effect.log*`, never this API.
- `@opentelemetry/core` — W3C propagator pair and composite the export lane registers globally for foreign libraries.
- `@opentelemetry/context-async-hooks` — `AsyncLocalStorageContextManager`, the server condition's ambient context seat.
- `@opentelemetry/otlp-exporter-base` — shared OTLP exporter config base carrying compression, timeout, and concurrency.
- `@opentelemetry/resources`
- `@opentelemetry/resource-detector-aws`
- `@opentelemetry/resource-detector-container`
- `@opentelemetry/resource-detector-gcp`
- `@opentelemetry/opentelemetry-browser-detector`
- `@opentelemetry/sdk-logs`
- `@opentelemetry/sdk-metrics`
- `@opentelemetry/host-metrics` — host and process series on the node lane's exposed meter provider.
- `@opentelemetry/sdk-trace-base`
- `@opentelemetry/sdk-trace-node` — `NodeSdk` facade substrate on the node lane; no direct import.
- `@opentelemetry/sdk-trace-web`
- `@opentelemetry/baggage-span-processor` — promotes admitted `rasm.*` baggage onto span attributes under the one promotion predicate.
- `@opentelemetry/exporter-trace-otlp-http`
- `@opentelemetry/exporter-trace-otlp-proto` — protobuf span leg of the SDK bridge.
- `@opentelemetry/exporter-metrics-otlp-http`
- `@opentelemetry/exporter-metrics-otlp-proto` — protobuf metric leg of the SDK bridge.
- `@opentelemetry/exporter-logs-otlp-http`
- `@opentelemetry/exporter-logs-otlp-proto` — protobuf log leg of the SDK bridge.
- `@opentelemetry/context-zone`
- `@opentelemetry/instrumentation` — `registerInstrumentations` activation and the `InstrumentationBase` contract under both condition nodes.
- `@opentelemetry/instrumentation-http` — inbound and outbound node HTTP spans covering foreign libraries the Effect seams never reach.
- `@opentelemetry/instrumentation-undici` — `fetch` and undici client spans on the node condition under the parent-presence gate.
- `@opentelemetry/instrumentation-pg` — PostgreSQL client spans under the parent-presence gate and the statement-capture posture row.
- `@opentelemetry/instrumentation-runtime-node` — event-loop, GC, and V8 heap series on the node lane's meter provider.
- `@opentelemetry/instrumentation-fetch`
- `@opentelemetry/instrumentation-document-load`
- `@opentelemetry/instrumentation-user-interaction`
- `@opentelemetry/instrumentation-xml-http-request`
- `@pyroscope/nodejs` — continuous-profiling push; `otel/profile.md` owns the lifecycle, composed only at the node root.
- `web-vitals` — estate-wide Core Web Vitals source; `otel/vital.md` registers the enriched-build capture functions and owns the cutoff pairs.

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

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/platform-browser`
- `@effect/experimental`

[WIRE_ENVELOPE]:
- `cloudevents` — `work/deliver.md` and `serve/route.md` own the HTTP binding; `net/pubsub.md` carries the opaque envelope.

[BENCH]:
- `mitata` — `proc/exec.md` owns the trial-engine route.
