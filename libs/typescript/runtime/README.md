# [TS_RUNTIME]

`runtime` is the branch's execution substrate: one body across the process plane, the distributed plane, and the browser condition, one package under one build flag. Every capability is a row, and only the boot module reads a concrete one.

## [01]-[ROUTER]

[PROC]:
- [01]-[EXEC](.planning/proc/exec.md): Runtimes as rows — a bun swap is one Layer selection and a child process a declarative value.
- [02]-[CONFIG](.planning/proc/config.md): One ordered provider chain answering every read; the boot-validated contract resolves once.
- [03]-[FLAG](.planning/proc/flag.md): Feature evaluation over the real OpenFeature SDK — targeting rules decoded and folded as data.
- [04]-[LIFE](.planning/proc/life.md): Startup and health as one skeleton — each row runs under its own budget and grades one receipt.
- [05]-[WORKER](.planning/proc/worker.md): Off-thread compute at full platform depth — a closed schema union speaks for every crossing.

[NET]:
- [06]-[CLIENT](.planning/net/client.md): One lane table for every branch egress — status admission, transient retry pulses, budgets inherited whole.
- [07]-[CHANNEL](.planning/net/channel.md): Long-lived byte conversations framed once, whatever the transport carries them.
- [08]-[PUBSUB](.planning/net/pubsub.md): Broadcast, replay, and blob handoff behind one engine-blind broker port.
- [09]-[COORDINATE](.planning/net/coordinate.md): Distributed agreement beside the fanout plane — lease, elect, and guarded state as one port.

[OTEL]:
- [10]-[EMIT](.planning/otel/emit.md): OTLP egress as one policy value and one Layer beside the W3C continuation ingress.
- [11]-[SERVER](.planning/otel/server.md): Node auto-instrumentation rows over the async-local manager — self-egress excluded, engine series bound.
- [12]-[INSTRUMENT](.planning/otel/instrument.md): Document instrumentation rows over the zone manager; interaction admission gates span cardinality.
- [13]-[CRASH](.planning/otel/crash.md): One structured fatal emission for every settled `Cause` — a total fold no failure class escapes.
- [14]-[METER](.planning/otel/meter.md): Durable-work evidence projected lossily onto Convention-keyed instruments.
- [15]-[PROFILE](.planning/otel/profile.md): Continuous wall and heap profiling pushed from the node lane under one identity projection.
- [16]-[VITAL](.planning/otel/vital.md): Browser RUM — the Core Web Vitals family measured whole, graded, and emitted once per document.

[SERVE]:
- [17]-[API](.planning/serve/api.md): Front-door assembly law — domain groups as data, one app-assembled `HttpApi`, derived secondary surfaces.
- [18]-[ROUTE](.planning/serve/route.md): Routes as Layers — api mount, upload dispatch, and intake verify in one serving fold.
- [19]-[LIVE](.planning/serve/live.md): Realtime SSE and WebSocket serving over branch feeds under resume-token and admission laws.
- [20]-[PROBLEM](.planning/serve/problem.md): RFC 9457 outbound-fault law — every leaving fault renders itself.
- [21]-[CLI](.planning/serve/cli.md): Verb families contributed as `Command` values, folded by the app into one root.

[WORK]:
- [22]-[ENTITY](.planning/work/entity.md): Cluster entities — sharded, per-id, single-writer identity over tiered mailboxes.
- [23]-[FLOW](.planning/work/flow.md): Suspend-and-replay workflows — recorded activities never re-run their side effects.
- [24]-[QUEUE](.planning/work/queue.md): Restart-surviving job families with keyed quotas spent through one store-backed counter.
- [25]-[SCHEDULE](.planning/work/schedule.md): Calendar recurrence as cadence rows — timezone-intrinsic cron, misfire windows, catch-up.
- [26]-[DELIVER](.planning/work/deliver.md): Mail and webhook egress as channel rows sharing one settlement receipt and one suppression.
- [27]-[FILTER](.planning/work/filter.md): Every subscription dialect compiled into a single predicate shape, CESQL parsed in-house.
- [28]-[REPORT](.planning/work/report.md): Document egress folded per format discriminant — each column owns its value projection.

[AI]:
- [29]-[MODEL](.planning/ai/model.md): Intelligence providers folded onto one asymmetry table, fallback ranked by the plan engine.
- [30]-[EMBED](.planning/ai/embed.md): Retrieval-port satisfaction — one normalization anchor, cut lanes as policy rows.
- [31]-[TOOL](.planning/ai/tool.md): Tools as typed data merged into toolkits, both MCP lanes gated by one safety owner.
- [32]-[AGENT](.planning/ai/agent.md): Sealed agent altitude — a session's phase spine is a machine, its chat persists durably.

[BROWSER]:
- [33]-[BOOT](.planning/browser/boot.md): One boot per document minting the one managed runtime handle under the app-spec budget.
- [34]-[SHELL](.planning/browser/shell.md): PWA manifest as a typed value the build encodes, with one update handshake.
- [35]-[PERSIST](.planning/browser/persist.md): Local persistence — each concern maps to its own named IndexedDB store, residency graded.
- [36]-[ROUTE](.planning/browser/route.md): Zero-package typed routing over the Navigation API, carrying the `Vault` session plane.
- [37]-[FETCH](.planning/browser/fetch.md): Byte-flow policy over `Web`, `Fetch`, `Pool`, and `Depot` — worker decoding, generation-scoped residency.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[DISTRIBUTION]:
- `@effect/cluster`
- `@effect/workflow`
- `@effect/rpc`
- `@nats-io/nats-core`
- `@nats-io/transport-node` — Native TCP/TLS `connect` for the node/bun lane.
- `@nats-io/jetstream`
- `@nats-io/kv`
- `@nats-io/obj`
- `@confluentinc/kafka-javascript` — librdkafka client backing the Kafka broker engine row on the shared C# broker plane.
- `@confluentinc/schemaregistry` — Kafka schema identity, compatibility admission, subject resolution, and codec framing.
- `@connectrpc/connect-node` — Node Connect/gRPC transport factories; `net/client` owns transport dispatch.
- `mqtt` — `net/channel` owns the MQTT v5 channel seam and the branch-owned CloudEvents binding riding it.
- `avsc` — `net/channel` mints the one `AvroCloudEvent` codec filling the empty arm `core/interchange/format`'s Avro media row leaves.
- `chevrotain` — `work/filter` owns the CESQL lexer and LL(k) grammar behind the `sql` filter dialect.

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
- `@opentelemetry/api-logs` — Peer contract the sdk-logs processors type against; log records mint through `Effect.log*`, never this API.
- `@opentelemetry/core` — W3C propagator pair and composite the export lane registers globally for foreign libraries.
- `@opentelemetry/context-async-hooks` — `AsyncLocalStorageContextManager`, the server condition's ambient context seat.
- `@opentelemetry/otlp-exporter-base` — Shared OTLP exporter config base carrying compression, timeout, and concurrency.
- `@opentelemetry/resources`
- `@opentelemetry/resource-detector-aws`
- `@opentelemetry/resource-detector-container`
- `@opentelemetry/resource-detector-gcp`
- `@opentelemetry/opentelemetry-browser-detector`
- `@opentelemetry/sdk-logs`
- `@opentelemetry/sdk-metrics`
- `@opentelemetry/host-metrics` — Host and process series on the node lane's exposed meter provider.
- `@opentelemetry/sdk-trace-base`
- `@opentelemetry/sdk-trace-node` — `NodeSdk` facade substrate on the node lane; no direct import.
- `@opentelemetry/sdk-trace-web`
- `@opentelemetry/baggage-span-processor` — Admitted `rasm.*` baggage promoted onto span attributes under the one promotion predicate.
- `@opentelemetry/exporter-trace-otlp-http`
- `@opentelemetry/exporter-trace-otlp-proto` — Protobuf span leg of the SDK bridge.
- `@opentelemetry/exporter-metrics-otlp-http`
- `@opentelemetry/exporter-metrics-otlp-proto` — Protobuf metric leg of the SDK bridge.
- `@opentelemetry/exporter-logs-otlp-http`
- `@opentelemetry/exporter-logs-otlp-proto` — Protobuf log leg of the SDK bridge.
- `@opentelemetry/context-zone`
- `@opentelemetry/instrumentation` — `registerInstrumentations` activation and the `InstrumentationBase` contract under both condition nodes.
- `@opentelemetry/instrumentation-http` — Inbound and outbound node HTTP spans covering foreign libraries the Effect seams never reach.
- `@opentelemetry/instrumentation-undici` — `fetch` and undici client spans on the node condition under the parent-presence gate.
- `@opentelemetry/instrumentation-pg` — PostgreSQL client spans under the parent-presence gate and the statement-capture policy row.
- `@opentelemetry/instrumentation-runtime-node` — Event-loop, GC, and V8 heap series on the node lane's meter provider.
- `@opentelemetry/instrumentation-fetch`
- `@opentelemetry/instrumentation-document-load`
- `@opentelemetry/instrumentation-user-interaction`
- `@opentelemetry/instrumentation-xml-http-request`
- `@pyroscope/nodejs` — Continuous-profiling push; `otel/profile` owns the lifecycle, composed only at the node root.
- `web-vitals` — Estate-wide Core Web Vitals source; `otel/vital` registers the enriched-build capture functions and owns the cutoff pairs.

[TERMINAL]:
- `@effect/cli`
- `@effect/printer`
- `@effect/printer-ansi`

[FLAGS]:
- `@openfeature/server-sdk` — Server evaluation SDK the `proc/flag` Provider implements; targeting rules stay decoded data.

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

Shared substrate consumed from the TypeScript registry, whose charters own the full contracts; `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-node`
- `@effect/platform-bun`
- `@effect/platform-browser`
- `@effect/experimental`

[EVENT_FABRIC]:
- `cloudevents` — HTTP, MQTT, NATS, and Kafka bindings riding the owning channel, fanout, delivery, and intake rows.

[BENCH]:
- `mitata` — `proc/exec` owns the trial-engine route.
