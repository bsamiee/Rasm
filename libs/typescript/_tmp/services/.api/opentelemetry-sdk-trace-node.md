# [API_CATALOGUE] @opentelemetry/sdk-trace-node

`@opentelemetry/sdk-trace-node` supplies the Node.js `NodeTracerProvider` and re-exports the full `@opentelemetry/sdk-trace-base` 2.x surface (span processors, exporters, samplers, id generators, config/contract types) symbol-for-symbol as the single trace-SDK import point. In `services` it is NOT a general tracing bootstrap — it is the SDK-only trace READER two owners compose: `execution/slo#SLO_BUDGET` materializes a concrete OTel `TracerProvider` to DECODE the C#-minted `ONE_DISTRIBUTED_TRACE` W3C trace context off the `interchange` wire and REATTACH it as the parent of a `slo.burn` span (so a node burn-rate alert carries the originating C# trace id — decode-only, never re-minting), and `provisioning/contract#PROVISIONING ObservabilityStack` provisions it as the collector's node trace source. It is the SDK-bridge `TracerProvider` that `@effect/opentelemetry` `NodeSdk.layer`/`Tracer.OtelTracerProvider` wires to the OTLP exporter edge (universal `.api/effect-opentelemetry.md`), the exact mirror of the sibling `@opentelemetry/sdk-metrics` `MeterProvider` reader (`.api/opentelemetry-sdk-metrics.md`) — both readers end at the one OTLP exporter, never a second emitter. The 2.x break is load-bearing: span processors attach via the `TracerConfig.spanProcessors` constructor array; the 1.x `provider.addSpanProcessor(proc)` method was REMOVED and there is no post-construction add, and no instrumentation-loader. It is admitted ONLY inside `scope:telemetry` and is the `[R3]`-collapse candidate the native `@effect/opentelemetry` `OtlpTracer` lane retires once parity closes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-node`
- package: `@opentelemetry/sdk-trace-node` (2.8.0, Apache-2.0, © OpenTelemetry Authors)
- module format: CJS-only barrel — `main` `build/src/index.js`, types `build/src/index.d.ts`; no `module`/`exports`/`esnext` entry and `type` unset (`build/` carries `src` alone), pure-JS with zero native ABI
- runtime target: node/bun — `NodeTracerProvider.register()` installs the node-default `AsyncLocalStorageContextManager` + W3C propagator; the browser reader is the separate `@opentelemetry/sdk-trace-web`
- otel-peer: `@opentelemetry/api` (peer `>=1.0.0 <1.10.0`; the `Tracer`/`Context`/`SpanKind`/`TextMapPropagator`/`ContextManager` contract this SDK implements), `@opentelemetry/sdk-trace-base` 2.8.0 (the re-export source), `@opentelemetry/context-async-hooks` 2.8.0 (the `AsyncLocalStorageContextManager` `register()` installs), `@opentelemetry/core` 2.8.0 (the W3C `TextMapPropagator`); `@opentelemetry/resources` (`Resource`) + `@opentelemetry/exporter-trace-otlp-http` (`OTLPTraceExporter`) join at the composition boundary
- catalog-verdict: KEEP but FENCED — `@opentelemetry/*` admitted only in `scope:telemetry`; `[R3]`-collapse target once `@effect/opentelemetry` native `OtlpTracer` reaches parity
- asset: 2 own symbols (`NodeTracerProvider`, `NodeTracerConfig`) + the full `@opentelemetry/sdk-trace-base` 2.x re-export (12 value + 14 type symbols, identical to the base barrel) — providers, span processors, exporters, samplers, id generators, and the config/contract type block
- consumer: `execution/slo#SLO_BUDGET` (trace-context decode + span reattach) and `provisioning/contract#PROVISIONING ObservabilityStack`; bridged to OTLP by `@effect/opentelemetry` `NodeSdk`/`Tracer` (`.api/effect-opentelemetry.md`); paired with `@opentelemetry/sdk-metrics` (`.api/opentelemetry-sdk-metrics.md`)
- rail: telemetry / tracing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: own symbols + base provider
`NodeTracerProvider` is the sole own class and the one composition root; it `extends BasicTracerProvider`, is immutable after construction (span processors are constructor data — there is no add-after-init API), and `register()` is its only node specialization. `NodeTracerConfig` is a verbatim `TracerConfig` alias.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                                     |
| :-----: | :------------------- | :------------ | :----------------------------------------------------------------------------------------------- |
|  [01]   | `NodeTracerProvider` | class         | `extends BasicTracerProvider`; `constructor(config?: NodeTracerConfig)`, `register(config?: SDKRegistrationConfig): void` |
|  [02]   | `NodeTracerConfig`   | type alias    | `= TracerConfig` verbatim — no Node-specific fields; the node behavior is in `register()`, not the config |
|  [03]   | `BasicTracerProvider`| class         | implements the `@opentelemetry/api` `TracerProvider`; inherited `getTracer`, `forceFlush`, `shutdown` (2.x base carries NO `register`/`addSpanProcessor`) |

[PUBLIC_TYPE_SCOPE]: span processors + exporters (re-export)
`BatchSpanProcessor` is the production processor wrapping the OTLP `SpanExporter`; `SimpleSpanProcessor`/`NoopSpanProcessor` are the synchronous/disabled arms, and `ConsoleSpanExporter`/`InMemorySpanExporter` are diagnostic/test sinks — never a production edge.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `BatchSpanProcessor`   | class         | queue-and-flush processor; `BufferConfig`-tuned (`maxExportBatchSize`/`scheduledDelayMillis`/`exportTimeoutMillis`/`maxQueueSize`) — the production processor |
|  [02]   | `SimpleSpanProcessor`  | class         | synchronous per-span export; no batching                                            |
|  [03]   | `NoopSpanProcessor`    | class         | drop processor; the disabled-tracing arm                                             |
|  [04]   | `ConsoleSpanExporter`  | class         | `SpanExporter` writing finished spans to the console — diagnostic only              |
|  [05]   | `InMemorySpanExporter` | class         | `SpanExporter` retaining spans in memory — test-only (`getFinishedSpans`/`reset`)   |
|  [06]   | `SpanProcessor`        | interface     | `onStart(span, ctx)` / `onEnding?(span)` / `onEnd(ReadableSpan)` / `forceFlush()` / `shutdown()` |
|  [07]   | `SpanExporter`         | interface     | `export(spans, resultCallback)` / `shutdown()` — the exporter contract              |

[PUBLIC_TYPE_SCOPE]: samplers (re-export)
`ParentBasedSampler` is the reader default — it honors the inbound sampled flag off the C#-minted context so a continued trace is not re-diced; `TraceIdRatioBasedSampler` head-samples new roots, and `SamplingDecision` is the numeric decision enum the `Sampler` contract returns.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `ParentBasedSampler`       | class         | `{ root, remoteParentSampled?, remoteParentNotSampled?, … }` — honor the inbound sampled flag, the reader default |
|  [02]   | `TraceIdRatioBasedSampler` | class         | `new (ratio)` — deterministic probabilistic head sampling                       |
|  [03]   | `AlwaysOnSampler`          | class         | record + sample every span                                                      |
|  [04]   | `AlwaysOffSampler`         | class         | drop every span                                                                 |
|  [05]   | `SamplingDecision`         | enum          | `NOT_RECORD = 0`, `RECORD = 1`, `RECORD_AND_SAMPLED = 2` (numeric)              |
|  [06]   | `SamplingResult`           | interface     | `{ decision: SamplingDecision; attributes?; traceState? }`                       |
|  [07]   | `Sampler`                  | interface     | `shouldSample(context, traceId, spanName, spanKind, attributes, links): SamplingResult` |

[PUBLIC_TYPE_SCOPE]: config + contract types (re-export)
`TracerConfig` is the 2.x constructor config carrying the `spanProcessors: SpanProcessor[]` array; `SDKRegistrationConfig` is what `register()` installs globally, and the remaining shapes are the buffer/limit/id-generator/read-model contracts the processors consume.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `TracerConfig`                    | interface     | `sampler?`, `generalLimits?`, `spanLimits?`, `resource?`, `idGenerator?`, `forceFlushTimeoutMillis?`, `spanProcessors?: SpanProcessor[]`, `meterProvider?` (@experimental) — the 2.x processor-array config |
|  [02]   | `SDKRegistrationConfig`           | interface     | `{ propagator?: TextMapPropagator \| null; contextManager?: ContextManager \| null }` — what `register()` installs globally |
|  [03]   | `BufferConfig`                    | interface     | `BatchSpanProcessor` tuning (`maxExportBatchSize` 512, `scheduledDelayMillis` 5000, `exportTimeoutMillis` 30000, `maxQueueSize` 2048) |
|  [04]   | `BatchSpanProcessorBrowserConfig` | interface     | `extends BufferConfig` + `disableAutoFlushOnDocumentHide?` (browser reader)      |
|  [05]   | `GeneralLimits`                   | interface     | `attributeValueLengthLimit?`, `attributeCountLimit?`                             |
|  [06]   | `SpanLimits`                      | interface     | per-span attribute/link/event count + length caps                               |
|  [07]   | `IdGenerator` / `RandomIdGenerator` | interface / class | trace/span id contract + the default random implementation                  |
|  [08]   | `ReadableSpan` / `Span`           | interface      | the finished-span read model (`onEnd`) / the live recording span (`onStart`)   |
|  [09]   | `TimedEvent`                      | interface      | `{ time, name, attributes? }` — a span event                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider construction, registration, lifecycle
The provider is constructed once with its `spanProcessors`/`sampler`/`resource` as constructor data, `register()`ed once at startup to install the global context manager + propagator, and drained on CLI exit via `forceFlush()` then `shutdown()`.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `new NodeTracerProvider(config?: NodeTracerConfig)`         | constructor    | span processors + sampler + resource passed in `config.spanProcessors`/`config.sampler` — there is NO `addSpanProcessor` |
|  [02]   | `provider.register(config?: SDKRegistrationConfig)`         | registration   | install as the global API `TracerProvider` + set propagator/context manager; call once at startup |
|  [03]   | `provider.getTracer(name, version?, { schemaUrl? })`        | factory        | obtain the `@opentelemetry/api` `Tracer`; `tracer.startSpan(name, opts?, ctx?)` mints a span under a parent `Context` |
|  [04]   | `provider.forceFlush()` / `provider.shutdown()`             | lifecycle      | drain the processors on CLI exit — force-flush then shutdown after envelope dispatch |

[ENTRYPOINT_SCOPE]: processor + exporter + sampler wiring (2.x)
The 2.x wiring is all constructor data: a `BatchSpanProcessor` around the OTLP exporter goes into `TracerConfig.spanProcessors`, and a `ParentBasedSampler` continues the inbound decision — there is no runtime attach.

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `new BatchSpanProcessor(exporter, config?: BufferConfig)`   | processor      | wrap an OTLP exporter for batched export; place in `TracerConfig.spanProcessors` |
|  [02]   | `new SimpleSpanProcessor(exporter)`                         | processor      | synchronous export — tests / low-volume                        |
|  [03]   | `new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(r) })` | sampler | continue the inbound sampled flag, ratio-sample new roots       |

```ts contract
// @opentelemetry/sdk-trace-node 2.8.0 — the 2.x wiring the SLO reader composes
import { NodeTracerProvider, BatchSpanProcessor, ParentBasedSampler, TraceIdRatioBasedSampler } from "@opentelemetry/sdk-trace-node"
import type { NodeTracerConfig, SDKRegistrationConfig, TracerConfig, ReadableSpan } from "@opentelemetry/sdk-trace-node"

// NodeTracerConfig === TracerConfig (verbatim alias)
interface TracerConfig {
  sampler?: Sampler
  generalLimits?: GeneralLimits
  spanLimits?: SpanLimits
  resource?: import("@opentelemetry/resources").Resource
  idGenerator?: IdGenerator
  forceFlushTimeoutMillis?: number
  spanProcessors?: SpanProcessor[]          // 2.x: processors are constructor data
  meterProvider?: import("@opentelemetry/api").MeterProvider // @experimental
}
interface SDKRegistrationConfig {
  propagator?: import("@opentelemetry/api").TextMapPropagator | null
  contextManager?: import("@opentelemetry/api").ContextManager | null
}

const provider = new NodeTracerProvider({
  sampler: new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(0.1) }),
  spanProcessors: [new BatchSpanProcessor(otlpExporter)], // no addSpanProcessor in 2.x
})
provider.register()                          // installs global AsyncLocalStorage + W3C propagator
// reattach: prefer @effect/opentelemetry Tracer.makeExternalSpan(wire) over a raw Context assembly
await provider.forceFlush(); await provider.shutdown() // CLI-exit drain
```

## [04]-[IMPLEMENTATION_LAW]

[TRACE_NODE_TOPOLOGY]:
- 2 own symbols; every other export is a direct `@opentelemetry/sdk-trace-base` re-export — symbol-for-symbol (12 value + 14 type), identical to the base barrel — so this package is the single trace-SDK import point on node; never mix a `sdk-trace-base` import for the same symbol.
- 2.x processor model: attach `SpanProcessor`s through the `TracerConfig.spanProcessors: SpanProcessor[]` constructor field. The 1.x `BasicTracerProvider.addSpanProcessor(proc)`/`getActiveSpanProcessor()` were REMOVED in 2.0 — there is no runtime add-processor path, and the provider is immutable after construction.
- `NodeTracerConfig` is a verbatim alias of `TracerConfig`; the node specialization lives entirely in `register()`, which installs the default `AsyncLocalStorageContextManager` (`@opentelemetry/context-async-hooks`) and W3C `TextMapPropagator` (`@opentelemetry/core`) unless overridden — it loads NO instrumentations (the 1.x auto-loader is gone).
- provider lifecycle is `forceFlush()` then `shutdown()`; a CLI process drains spans this way after the last write, matching the OTel drain the assay operator itself performs at exit.

[LOCAL_ADMISSION]:
- Wire one `NodeTracerProvider` per telemetry composition root with `spanProcessors` at construction and `register()` once at startup; rebuild the provider to reconfigure — there is no add-processor API.
- `InMemorySpanExporter` is test-only (`getFinishedSpans`/`reset`); `ConsoleSpanExporter` is diagnostic-only — the production sink is a `BatchSpanProcessor` around `@opentelemetry/exporter-trace-otlp-http` `OTLPTraceExporter`, drained `forceFlush()` then `shutdown()` on exit.
- Every `@opentelemetry/*` import stays inside `scope:telemetry`; no `services` folder imports this package directly — instrumentation emits through Effect's native `Effect.withSpan`, and `@effect/opentelemetry` owns the export boundary.

[STACKING]:
- decode-and-reattach (`execution/slo#SLO_BUDGET`): the `NodeTracerProvider` materializes the concrete OTel `TracerProvider` behind `reattachTrace` — the owner decodes the C#-minted `ONE_DISTRIBUTED_TRACE` `TraceContextWire` (`traceId`/`parentSpanId`/`traceFlags`/`traceState`) off the `interchange` descriptor path through one `Schema` carrier and reattaches it as the parent of the `slo.burn` span so the node burn-rate alert carries the originating C# trace id, never re-minting. Express the continuation through `@effect/opentelemetry` `Tracer.makeExternalSpan({ traceId, spanId, traceFlags?, traceState? })` + `Tracer.withSpanContext` (`.api/effect-opentelemetry.md`) rather than a hand-assembled `@opentelemetry/api` `Context` — the bridge owns the W3C extract-and-continue and keeps the reattachment on the Effect tracer spine.
- exporter edge (`provisioning/contract#PROVISIONING` + `@effect/opentelemetry`): this package is the SDK-bridge `TracerProvider`; `@effect/opentelemetry` `NodeSdk.layer({ resource, spanProcessor, tracerConfig })` binds it to OTLP export — the `spanProcessor` field takes this package's `BatchSpanProcessor` around the manifest `@opentelemetry/exporter-trace-otlp-http` `OTLPTraceExporter`, and `Tracer.OtelTracerProvider` bridges Effect `Effect.withSpan` spans onto it. `@opentelemetry/*` is admitted only inside `scope:telemetry` and the native `Otlp` lane is preferred; reach for this SDK bridge only when an SDK-only trace exporter/processor is required, and record it as an `[R3]` dependency.
- two-reader pair (`execution/slo#SLO_BUDGET`): paired with the sibling `@opentelemetry/sdk-metrics` `MeterProvider`/`MetricProducer`/`PeriodicExportingMetricReader` (`.api/opentelemetry-sdk-metrics.md`) — the `NodeTracerProvider` reads/reattaches trace context, the `MeterProvider` collects the `latency`/`availability` request metrics; both end at the one `@effect/opentelemetry` OTLP exporter edge, never a second emitter beside the `ObservabilityStack` collector.
- `[R3]` collapse: the inbound span-context reattach rides the native `Tracer.makeExternalSpan` bridge, NOT an SDK-only capability, so this reader carries the SAME collapse posture as `@opentelemetry/sdk-metrics` — the fallback `TracerProvider`/`SpanProcessor`/exporter block the native `@effect/opentelemetry` `Otlp`/`OtlpTracer` lane retires once it reaches parity; `semantic-conventions` survives as the signal-name vocabulary, the rest of the `@opentelemetry/*` peer set collapses.
- `@opentelemetry/api` boundary: `Tracer`, `Span`, `Context`, `SpanKind`, `TextMapPropagator`, `ContextManager`, `TraceState`, `Attributes`, `Link` are `@opentelemetry/api` types this SDK implements; `Resource` is `@opentelemetry/resources`; the W3C propagator implementation is `@opentelemetry/core`; the OTLP exporter is `@opentelemetry/exporter-trace-otlp-http` — instrumentation code emits through Effect's native `Effect.withSpan`, never a direct import of this package outside the composition root.

[RAIL_LAW]:
- package: `@opentelemetry/sdk-trace-node` (2.8.0)
- owns: the node `NodeTracerProvider`, the `@opentelemetry/sdk-trace-base` 2.x symbol-for-symbol re-export hub, the `SpanProcessor`/`SpanExporter`/sampler machinery below the `@opentelemetry/api` `Tracer`, and the SDK-bridge `TracerProvider` `@effect/opentelemetry` `NodeSdk` binds
- accept: `NodeTracerConfig.spanProcessors` at construction; `SDKRegistrationConfig` at `register()`; `Tracer.makeExternalSpan`/`withSpanContext` for the W3C reattach; construction only inside `scope:telemetry`
- reject: `provider.addSpanProcessor(...)` (removed in 2.x — a phantom); a mixed `@opentelemetry/sdk-trace-base` import for a symbol this package re-exports; this reader imported outside `scope:telemetry`; a hand-rolled `traceparent` parse where `Tracer.makeExternalSpan` continues the context; re-minting the C# trace context instead of decode-only reattach; a second emitter beside the `@effect/opentelemetry` OTLP edge
