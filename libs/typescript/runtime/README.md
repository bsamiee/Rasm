# [TS_RUNTIME]

`runtime` is the branch's execution substrate — one body across the process plane, the distributed plane, and the browser condition, one package under one build flag. Its bar is structural singularity: one circuit ledger every dial inherits, one ambient redaction scrub at every capture seam, one `WorkClass` economy pricing every durable surface, one assembled front door no lib can hold itself — every capability is a row, and only the boot module reads a concrete one. Faults leave only as self-rendering values, and degradation is a readable `Layer` choice, never hidden behavior.

It imports core, security, and data and is composed by the interface and deploy planes: security satisfies its guard and intake seams into the front door, data owns the outbox, mailbox, and journal its durable work composes, the deploy plane's outputs arrive only as typed env facts, and the C# host meets its export lanes on the one OTLP collector wire. It mints no content identity and holds no record of truth of its own.

## [01]-[ROUTER]

- [01]-[PROC](.planning/proc/): Runtime binding, resolve-once config, flags-as-data, lifecycle receipts, and one worker protocol over platform Tags.
- [02]-[NET](.planning/net/): One HTTP lane table off the core budget, framed byte channels, and engine-blind fanout, replay, and coordination.
- [03]-[OTEL](.planning/otel/): OTLP wire half under one redaction scrub — egress, continuation, crash fold, RUM vitals, meter bridge.
- [04]-[SERVE](.planning/serve/): Libs export route, verb, and group data; the app assembles one `HttpApi` and CLI root; faults leave as `Problem`s.
- [05]-[WORK](.planning/work/): One `WorkClass` economy — sharded actors, replay workflows, durable queues with the sole DLQ owner, cluster cron.
- [06]-[AI](.planning/ai/): Provider families on one capability-asymmetry table with ranked fallback, chunk-and-embed, and Schema-typed MCP tools.
- [07]-[BROWSER](.planning/browser/): One boot graph per document, the PWA shell, IndexedDB vocabulary, and the router carrying `Vault`.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[DISTRIBUTION]:
- `@effect/cluster`
- `@effect/workflow`
- `@effect/rpc`
- `@nats-io/nats-core`
- `@nats-io/transport-node` — native TCP/TLS `connect` for the node/bun lane; re-exports the core surface, sibling to the browser lane's `wsconnect`
- `@nats-io/jetstream`
- `@nats-io/kv`
- `@nats-io/obj`
- `@confluentinc/kafka-javascript` — librdkafka client backing the Kafka broker engine row, TypeScript counterpart of the C# host's `Confluent.Kafka` on the shared broker plane
- `@connectrpc/connect-node` — catalog-confirmed Node gRPC/Connect client transport factories; `net/client.md` owns transport dispatch, while its interceptor pair and `serve/live.md` mount remain blocked research
- `mqtt` — admitted manifest package with no applicable catalog; `net/channel.md` owns the armed MQTT v5 research route
- `cloudevents` — admitted manifest package with no applicable catalog; `work/deliver.md` and `serve/route.md` own the armed HTTP-binding routes, while `net/pubsub.md` transports the data owner's opaque envelope

[INTELLIGENCE]:
- `@effect/ai`
- `@effect/ai-anthropic`
- `@effect/ai-openai`
- `@effect/ai-google`
- `@effect/ai-amazon-bedrock`
- `@effect/ai-openrouter`
- `@modelcontextprotocol/sdk`

[TELEMETRY]:
- `@effect/opentelemetry` — facade bridging Effect `Tracer`/`Metric`/`Logger` signals to OTLP export; both export lanes compose it, only this folder imports it
- `@opentelemetry/api`
- `@opentelemetry/api-logs` — peer contract the sdk-logs processors type against; log records mint through `Effect.log*`, never this API
- `@opentelemetry/core`
- `@opentelemetry/resources`
- `@opentelemetry/resource-detector-aws` — aws-arm compute-identity detector rows for the node lane roster
- `@opentelemetry/resource-detector-container` — cgroup container-identity detector row for the node lane roster
- `@opentelemetry/resource-detector-gcp` — metadata-server detector row for the gcp arm
- `@opentelemetry/opentelemetry-browser-detector` — browser identity detector on the web lane
- `@opentelemetry/sdk-logs`
- `@opentelemetry/sdk-metrics`
- `@opentelemetry/host-metrics` — `HostMetrics` sweeps `system.*` and `process.*` series onto the node lane's exposed meter provider
- `@opentelemetry/sdk-trace-base`
- `@opentelemetry/sdk-trace-node` — `NodeSdk` facade substrate on the node lane; no direct import
- `@opentelemetry/sdk-trace-web`
- `@opentelemetry/baggage-span-processor` — `BaggageSpanProcessor` promotes `rasm.*` baggage entries onto span attributes through a key predicate, the contributed `Hooks` row replacing the tenant `onStart` bridge
- `@opentelemetry/exporter-trace-otlp-http`
- `@opentelemetry/exporter-trace-otlp-proto` — protobuf-wire sibling of the http trace exporter; the SDK-bridge span leg when the collector demands OTLP/HTTP protobuf, one `OTLPTraceExporter` binding the `ProtobufTraceSerializer`
- `@opentelemetry/exporter-metrics-otlp-http`
- `@opentelemetry/exporter-metrics-otlp-proto` — protobuf-wire sibling of the http metric exporter; the SDK-bridge metric leg satisfying the OTLP/HTTP+protobuf sole-egress mandate, one `OTLPMetricExporter` binding the `ProtobufMetricsSerializer` and inheriting the http row's temporality algebra
- `@opentelemetry/exporter-logs-otlp-http`
- `@opentelemetry/exporter-logs-otlp-proto` — protobuf-wire sibling of the http log exporter; the SDK-bridge log leg completing the three-signal protobuf set, one `OTLPLogExporter` binding the `ProtobufLogsSerializer` and wrapped by `BatchLogRecordProcessor`
- `@opentelemetry/context-zone`
- `@opentelemetry/instrumentation` — `registerInstrumentations` activation and the `InstrumentationBase` contract under the browser rows
- `@opentelemetry/instrumentation-runtime-node` — `RuntimeNodeInstrumentation` registers event-loop delay/utilization, GC-duration, and V8 heap series on the node lane's meter provider beside `HostMetrics`
- `@opentelemetry/instrumentation-fetch`
- `@opentelemetry/instrumentation-document-load`
- `@opentelemetry/instrumentation-user-interaction`
- `@opentelemetry/instrumentation-xml-http-request`
- `@pyroscope/nodejs` — continuous-profiling push over the native pprof sampler; `otel/profile.md` owns the lifecycle, composed only at the node root and drained with the process

[BENCH]:
- `mitata` — admitted manifest package with no applicable catalog; `proc/exec.md` owns the armed engine-composition route while its package-independent receipt producer remains settled

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

[RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/platform-browser`
- `@effect/experimental`
