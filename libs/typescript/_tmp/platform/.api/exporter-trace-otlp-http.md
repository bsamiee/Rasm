# [API_CATALOGUE] @opentelemetry/exporter-trace-otlp-http

`@opentelemetry/exporter-trace-otlp-http` supplies `OTLPTraceExporter`, the concrete browser `SpanExporter` that serializes `ReadableSpan[]` to OTLP/JSON and POSTs them to a collector over the Fetch/Beacon transport. It is a THIN concrete class over the shared `@opentelemetry/otlp-exporter-base` owner, which carries the entire lifecycle (`export`/`forceFlush`/`shutdown`), the retry/backoff loop (`ExportResponseRetryable`), the transport delegate (`createOtlpNetworkExportDelegate`), the typed `OTLPExporterError`, and the base/node/shared configuration split — so this catalogue documents the exporter AND the base surface it inherits. In the `platform` telemetry stack the exporter is the `SpanExporter` bound under a `@opentelemetry/sdk-trace-web` `BatchSpanProcessor` inside the `@effect/opentelemetry` `WebSdk.Configuration.spanProcessor`; it sits on the SDK-BRIDGE lane the `@effect/opentelemetry` catalogue fences to `scope:telemetry` and marks for `[R3]` collapse once the native `OtlpTracer` lane reaches parity.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/exporter-trace-otlp-http`
- package: `@opentelemetry/exporter-trace-otlp-http`
- version: `0.219.0` (central pin `pnpm-workspace.yaml`; experimental line, tracks `@opentelemetry/otlp-exporter-base@0.219.0`)
- license: `Apache-2.0`
- api-peer: `@opentelemetry/api ^1.3.0` (declared peer; resolves `1.9.1`) — consumes `@opentelemetry/core` `ExportResult`/`ExportResultCode` (the `export(...)` callback shape, `opentelemetry-core.md`) + `@opentelemetry/sdk-trace-base` `SpanExporter`/`ReadableSpan` (contract); deps `@opentelemetry/otlp-exporter-base@0.219.0` (lifecycle + transport + config), `@opentelemetry/otlp-transformer@0.219.0` (span -> OTLP frame), `@opentelemetry/sdk-trace-base@2.8.0`, `@opentelemetry/core@2.8.0`, `@opentelemetry/resources@2.8.0`
- module: `@opentelemetry/exporter-trace-otlp-http` (single barrel; no subpaths)
- runtime: `browser` — the browser build is selected by the legacy package.json `browser` field remap (`build/src/platform/index.js` -> `platform/browser/index.js`), NOT a conditional `exports` map (`exports` is `null`); Vite/Rolldown honor the `browser` field, so browser code never binds the Node `http`/`https` agent path
- asset: runtime library — side-effects-free (`sideEffects: false`), tree-shakeable
- rail: tracing
- collapse-fence: SDK-bridge exporter lane, fenced to `scope:telemetry`, retired at `[R3]` when native `@effect/opentelemetry` `Otlp`/`OtlpTracer.layer` reaches parity (`libs/typescript/.api/effect-opentelemetry.md`)

## [02]-[PUBLIC_TYPES]

The package's ONLY own export is `OTLPTraceExporter`. Everything load-bearing lives on the shared `@opentelemetry/otlp-exporter-base` owner (lifecycle, retry, transport, config, error) and the `@opentelemetry/sdk-trace-base` contract (`SpanExporter`/`ReadableSpan`) — re-listed here with `[SOURCE_PACKAGE]` because the consumed contract is the base, not this thin subclass.

[PUBLIC_TYPE_SCOPE]: exporter and consumed contract
- rail: tracing

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [SOURCE_PACKAGE]                          | [CONSUMER / BOUNDARY]                                 |
| :-----: | :----------------------- | :------------ | :---------------------------------------- | :---------------------------------------------------- |
|  [01]   | `OTLPTraceExporter`      | class         | `@opentelemetry/exporter-trace-otlp-http` | browser `SpanExporter` over OTLP/HTTP                 |
|  [02]   | `SpanExporter`           | interface     | `@opentelemetry/sdk-trace-base`           | the contract `OTLPTraceExporter implements`           |
|  [03]   | `ReadableSpan`           | interface     | `@opentelemetry/sdk-trace-base`           | the `Internal` batch type (`OTLPExporterBase<ReadableSpan[]>`) |

[PUBLIC_TYPE_SCOPE]: shared `otlp-exporter-base` owner (lifecycle / config / retry / error)
- rail: tracing
- ownership split: `ExportResponse` (`success`\|`failure`\|`retryable`) is the base-owned TRANSPORT response the retry loop reads; the `export(...)` CALLBACK result — `ExportResult`/`ExportResultCode` — is `@opentelemetry/core`-owned (`opentelemetry-core.md`, imported by `sdk-trace-base` `SpanExporter.export`), cited not redeclared here.
- `@opentelemetry/otlp-exporter-base@0.219.0`, Apache-2.0. This is the SAME owner `@opentelemetry/exporter-metrics-otlp-http` wraps — one base, two signal subclasses. The base owns `export`/`forceFlush`/`shutdown`, the retryable-response backoff, and the transport delegate; the concrete exporter adds only the signal type and the OTLP path suffix.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :------------------------------ | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `OTLPExporterBase<Internal>`    | class         | shared `export`/`forceFlush`/`shutdown` base; ctor takes an `IOtlpExportDelegate` |
|  [02]   | `OTLPExporterConfigBase`        | interface     | browser config: `url`, `headers`, `concurrencyLimit`, `timeoutMillis`       |
|  [03]   | `OTLPExporterNodeConfigBase`    | interface     | REJECTED in browser: adds `keepAlive`, `compression`, `httpAgentOptions`, `userAgent` |
|  [04]   | `OtlpSharedConfiguration`       | interface     | normalized `{ timeoutMillis, concurrencyLimit, compression }` after merge   |
|  [05]   | `CompressionAlgorithm`          | enum          | `NONE='none'`, `GZIP='gzip'` — node-only; browser Fetch owns encoding       |
|  [06]   | `OTLPExporterError`             | class         | `extends Error` with `code?`/`data?`; the typed export failure              |
|  [07]   | `ExportResponse`                | union         | `success` \| `failure` \| `retryable` — the transport response the retry loop reads |
|  [08]   | `IOtlpExportDelegate<Internal>` | interface     | the transport delegate the subclass injects into the base                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: OTLPTraceExporter construction and lifecycle
- rail: tracing
- The only user-constructed surface is `new OTLPTraceExporter(config?)`. `export`/`forceFlush`/`shutdown` are inherited from `OTLPExporterBase` verbatim and are driven by the SDK span processor, never called directly.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY]   | [CONSUMER / BOUNDARY]                                       |
| :-----: | :--------------------------------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `new OTLPTraceExporter(config?: OTLPExporterConfigBase)` | exporter factory | browser trace exporter; the `SpanExporter` `BatchSpanProcessor` wraps |
|  [02]   | `exporter.export(spans: ReadableSpan[], (result: ExportResult) => void): void` | span export | callback-style (NOT promise); `ExportResult` is `@opentelemetry/core`-owned (`opentelemetry-core.md`); serialize + POST; the processor supplies the callback |
|  [03]   | `exporter.forceFlush(): Promise<void>`   | lifecycle        | resolve when pending batch exports drain; the SDK `forceFlush` fan-out reaches it |
|  [04]   | `exporter.shutdown(): Promise<void>`     | lifecycle        | stop exporter, release the delegate; SDK shutdown reaches it |

[ENTRYPOINT_SCOPE]: `OTLPExporterConfigBase` fields (browser)
- rail: tracing

| [INDEX] | [FIELD]            | [TYPE]                                     | [DEFAULT]                          |
| :-----: | :----------------- | :----------------------------------------- | :--------------------------------- |
|  [01]   | `url`              | `string`                                   | `—` (delegate appends `v1/traces`) |
|  [02]   | `headers`          | `Record<string, string> \| HeadersFactory` | `—` (`HeadersFactory` = async `() => Record<string,string>` for bearer rotation) |
|  [03]   | `concurrencyLimit` | `number`                                   | `—`                                |
|  [04]   | `timeoutMillis`    | `number`                                   | `10000`                            |

## [04]-[IMPLEMENTATION_LAW]

[TRACE_EXPORT_TOPOLOGY]:
- `OTLPTraceExporter extends OTLPExporterBase<ReadableSpan[]> implements SpanExporter`; the class body is only a constructor that builds a browser network delegate (`createOtlpNetworkExportDelegate` over the Fetch/Beacon transport) and hands it to the base — all export/flush/shutdown behavior is the base's.
- browser vs node is a BUILD-TIME `browser`-field remap, not a runtime branch and not an `exports` condition: TypeScript type resolution follows `platform/index.d.ts -> ./node`, so the compile-time `OTLPTraceExporter` TYPE is the node class, but the two platform classes share one identical public shape (`constructor(config?: OTLPExporterConfigBase)`), so the browser bundle binding the browser class is transparent to the type.
- retry/backoff is OWNED BY THE BASE: a transport `ExportResponseRetryable` re-enqueues with `retryInMillis` backoff (the base `retrying-transport`/`is-export-retryable`); the exporter never re-implements retry, and the outer `BatchSpanProcessor` owns batching/queue-drop, so a hand-rolled retry wrapper around `export` is the deleted form.
- `export` is callback-style (`(result: ExportResult) => void`), never promise-returning; `ExportResult`/`ExportResultCode` are `@opentelemetry/core`-owned (`opentelemetry-core.md`), distinct from the base's `ExportResponse` transport union — the exporter reports the core `ExportResult`, never a redeclared shape; the browser build uses the Fetch/Beacon transport, never a Node `http`/`https` agent — the `keepAlive`/`httpAgentOptions`/`compression`/`userAgent` node fields (`OTLPExporterNodeConfigBase`) are unreachable in browser config.

[INTEGRATION_LAW]:
- stack under `@opentelemetry/sdk-trace-web` `BatchSpanProcessor`: `new BatchSpanProcessor(new OTLPTraceExporter({ url: \`${endpoint}/v1/traces\` }))` — the processor owns batching, queue limits, and export scheduling; the exporter owns only serialize+POST. `SimpleSpanProcessor` is the reject (synchronous per-span export).
- stack under `@effect/opentelemetry` `WebSdk`: the processor is the `WebSdk.Configuration.spanProcessor` row (`libs/typescript/.api/effect-opentelemetry.md` `[R3]` SDK-bridge lane); `WebSdk.layer` yields a `Resource.Resource` layer and never exposes the exporter, so the exporter is constructed inline in the `Configuration` thunk (`Observability/telemetry#METRIC_REGISTRY`).
- one sampler, one resource, one endpoint: the `WebSdk.Configuration.tracerConfig.sampler` is `new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio) })` (`libs/typescript/_tmp/platform/.api/opentelemetry-sdk-trace-web.md`) so a child inherits the parent's sampling decision; the endpoint reads from `RuntimeConfig.collectorOtlpEndpoint`; the resource is the `AppIdentity`-derived `@effect/opentelemetry` `Resource` shared with the metric exporter — never a per-exporter endpoint or resource fork.
- W3C continuation is a SEPARATE owner: this exporter emits spans; the inbound `traceparent` extract-and-continue is `@opentelemetry/core` `CompositePropagator` registered via a self-constructed `WebTracerProvider.register({ propagator })` (`Observability/telemetry#TRACE_PROPAGATION`), never a field on this exporter.

[LOCAL_ADMISSION]:
- construct `OTLPTraceExporter` ONLY at the composition root inside the `WebSdk.Configuration.spanProcessor`; instrumentation ships spans through Effect's native `Effect.withSpan` / the `MetricRegistry.span` projector, never by importing this package.
- set `url` to the collector OTLP/HTTP traces endpoint from `RuntimeConfig`; set `headers` to a `HeadersFactory` for rotating bearer tokens; leave batching/retry to `BatchSpanProcessor` + the base retry loop.
- record this dependency as an `[R3]` non-collapsed SDK-bridge exporter: it exists only until the native `@effect/opentelemetry` `OtlpTracer.layer` (over `HttpClient`) reaches browser parity, at which point the whole `@opentelemetry/exporter-*` + `sdk-trace-web` block collapses while the propagation (`@opentelemetry/core`) / resource-identity (`@opentelemetry/resources`) / convention (`@opentelemetry/semantic-conventions`) vocabulary survives.

[RAIL_LAW]:
- Package: `@opentelemetry/exporter-trace-otlp-http`
- Owns: the concrete browser OTLP/HTTP `SpanExporter` (`OTLPTraceExporter`) over the shared `@opentelemetry/otlp-exporter-base` lifecycle/retry/transport/config/error owner
- Accept: `OTLPTraceExporter` as the `SpanExporter` under `BatchSpanProcessor` inside `WebSdk.Configuration.spanProcessor`, endpoint from `RuntimeConfig`, resource shared with the metric exporter, `[R3]`-tagged
- Reject: hand-rolled span POST clients; the Node exporter variant or `OTLPExporterNodeConfigBase` fields in browser code; a hand-rolled retry wrapper around `export` (the base owns retry); direct `export` invocation outside the span processor; an exporter-owned propagator field (propagation is `@opentelemetry/core`'s owner)
