# [TS_RUNTIME_API_OPENTELEMETRY_SDK_TRACE_BASE]

`@opentelemetry/sdk-trace-base` owns the trace-export pipeline the SDK-bridge lane wires into: the `SpanProcessor` → `SpanExporter` contract, the `Sampler` algebra, the `IdGenerator`, and the `TracerConfig` construction bag `BasicTracerProvider` reads. `otel/emit` never touches it directly — `@effect/opentelemetry` `NodeSdk`/`WebSdk` construct the provider from a `Configuration` whose `spanProcessor`/`tracerConfig` slots ARE this package's `SpanProcessor` and `TracerConfig` (verified: `NodeSdk.d.ts` imports `SpanProcessor, TracerConfig` from here). `sdk-trace-node` re-exports this entire surface and adds only `NodeTracerProvider`, so this leg is the one place the trace roster is defined. It is a `[OTEL_PIN_BLOCK]` collapse target: when the native `@effect/opentelemetry` `Otlp` lane reaches parity the whole `@opentelemetry/sdk-*` block retires and only `semantic-conventions` survives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-base`
- package: `@opentelemetry/sdk-trace-base` (Apache-2.0)
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM mirror (`build/esm/index.js`, `module`); flat barrel, no `exports` subpath map, one `.d.ts` per concern under `build/src/{export,sampler,platform}`.
- asset: TSDECL `build/src/index.d.ts` (restored).
- peer: `@opentelemetry/api >=catalog <catalog` — the version-pinned API contract; deps `@opentelemetry/core` (`ExportResult`/`ExportResultCode`/`InstrumentationScope`), `@opentelemetry/resources` (`Resource`), `@opentelemetry/semantic-conventions`.
- runtime: runtime-neutral — the `./platform` conditional export swaps the node crypto `RandomIdGenerator` + `BatchSpanProcessor<BufferConfig>` for the browser `Math.random` generator + `BatchSpanProcessor<BatchSpanProcessorBrowserConfig>`; no fork in consumer code.
- plane: `plane:runtime`, edge-ledger-fenced to `scope:runtime` — no folder outside `telemetry` imports it.
- rail: observability/sdk-bridge; `[OTEL_PIN_BLOCK]` collapse target — only `semantic-conventions` survives the pin-block retirement.
- role: the `SpanProcessor`/`SpanExporter`/`Sampler`/`IdGenerator` roster behind `@effect/opentelemetry` `NodeSdk`/`WebSdk`; re-exported wholesale by `sdk-trace-node`.

## [02]-[PROVIDER]

`BasicTracerProvider` is the platform-extensible provider (`NodeTracerProvider`/`WebTracerProvider` subclass it); `TracerConfig` is its one construction bag, and every axis on it — sampler, limits, id generator, processor list — is a policy value, never a subclass. Under the effect facade `tracerConfig` is `Omit<TracerConfig, "resource">` — identity enters through the facade `Configuration`'s own resource options (or the `Resource` layer), so a consumer never sets `TracerConfig.resource`.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                                         |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `BasicTracerProvider`          | class         | `implements TracerProvider`; base for `NodeTracerProvider` (sdk-trace-node) |
|  [02]   | `TracerConfig`                 | interface     | provider bag — `sampler`/`spanLimits`/`idGenerator`/`spanProcessors`        |
|  [03]   | `SpanLimits` / `GeneralLimits` | interface     | per-span vs global attribute/link/event count + value-length caps           |
|  [04]   | `SDKRegistrationConfig`        | interface     | `propagator?`/`contextManager?` for the global `register()` path            |
|  [05]   | `ReadableSpan` / `Span`        | type          | the recorded-span read shape; `Span = APISpan & ReadableSpan`               |
|  [06]   | `TimedEvent`                   | type          | a span event with `HrTime` — the `events[]` element                         |

[TRACER_CONFIG]: `TracerConfig.sampler: Sampler` `TracerConfig.generalLimits: GeneralLimits` `TracerConfig.spanLimits: SpanLimits` `TracerConfig.resource: Resource` `TracerConfig.idGenerator: IdGenerator` `TracerConfig.forceFlushTimeoutMillis: number` `TracerConfig.spanProcessors: SpanProcessor[]` `TracerConfig.meterProvider: MeterProvider`
[BASIC_TRACER_PROVIDER]: `BasicTracerProvider(TracerConfig?)` `BasicTracerProvider.getTracer(string,string?,{schemaUrl?:string}?) -> Tracer` `BasicTracerProvider.forceFlush() -> Promise<void>` `BasicTracerProvider.shutdown() -> Promise<void>`
[READABLE_SPAN]: `ReadableSpan.name: string` `ReadableSpan.kind: SpanKind` `ReadableSpan.spanContext: ()=>SpanContext` `ReadableSpan.parentSpanContext: SpanContext` `ReadableSpan.startTime: HrTime` `ReadableSpan.endTime: HrTime` `ReadableSpan.duration: HrTime` `ReadableSpan.status: SpanStatus` `ReadableSpan.attributes: Attributes` `ReadableSpan.links: Link[]` `ReadableSpan.events: TimedEvent[]` `ReadableSpan.ended: boolean` `ReadableSpan.resource: Resource` `ReadableSpan.instrumentationScope: InstrumentationScope` `ReadableSpan.droppedAttributesCount: number` `ReadableSpan.droppedEventsCount: number` `ReadableSpan.droppedLinksCount: number`

## [03]-[PROCESSOR_AND_EXPORTER]

The pipeline is TWO parameterized interfaces, not a fixed set of pairs. `SpanProcessor` owns the start/end/flush lifecycle; `SpanExporter` owns the format/transport. The built-in classes are ROWS on those interfaces — `Simple` (per-span, synchronous, diagnostics-only), `Batch` (queued, the production row, parameterized on `BufferConfig`), `Noop` (drop) — and a custom transport is a new `SpanExporter`, a custom lifecycle a new `SpanProcessor`, never a fork. `BatchSpanProcessor` is the `./platform` specialization of the internal `BatchSpanProcessorBase<T extends BufferConfig>` — node binds `T = BufferConfig`, browser `T = BatchSpanProcessorBrowserConfig`.

| [INDEX] | [SYMBOL]                          | [KIND]               | [CAPABILITY_BOUNDARY]                                             |
| :-----: | :-------------------------------- | :------------------- | :---------------------------------------------------------------- |
|  [01]   | `SpanProcessor`                   | interface            | `onStart`/`onEnding?`/`onEnd`/`forceFlush`/`shutdown` lifecycle   |
|  [02]   | `SimpleSpanProcessor`             | class                | one export per ended span; sync; diagnostics/test only            |
|  [03]   | `BatchSpanProcessor`              | class (`./platform`) | queued batch export; the production row; `BufferConfig`-tuned     |
|  [04]   | `NoopSpanProcessor`               | class                | drop-all — the disabled-signal row                                |
|  [05]   | `BufferConfig`                    | interface            | node batch tuning: queue/batch size, delay                        |
|  [06]   | `BatchSpanProcessorBrowserConfig` | interface            | browser batch tuning; adds `disableAutoFlushOnDocumentHide`       |
|  [07]   | `SpanExporter`                    | interface            | `export(spans, cb)`/`shutdown`/`forceFlush?` — format + transport |
|  [08]   | `ConsoleSpanExporter`             | class                | stdout diagnostics                                                |
|  [09]   | `InMemorySpanExporter`            | class                | `getFinishedSpans()`/`reset()` — the kit-driven spec-assert lane  |

[SPAN_PROCESSOR]: `SpanProcessor.onStart(Span,Context) -> void` `SpanProcessor.onEnding(Span) -> void` `SpanProcessor.onEnd(ReadableSpan) -> void` `SpanProcessor.forceFlush() -> Promise<void>` `SpanProcessor.shutdown() -> Promise<void>`
[SPAN_EXPORTER]: `SpanExporter.export(ReadableSpan[],(result:ExportResult)=>void) -> void` `SpanExporter.shutdown() -> Promise<void>` `SpanExporter.forceFlush() -> Promise<void>`
[SIMPLE_SPAN_PROCESSOR]: `SimpleSpanProcessor(SpanExporter)`
[BATCH_SPAN_PROCESSOR_BASE]: `BatchSpanProcessorBase(SpanExporter,T?)`
[BUFFER_CONFIG]: `BufferConfig.maxExportBatchSize: number` `BufferConfig.scheduledDelayMillis: number` `BufferConfig.exportTimeoutMillis: number` `BufferConfig.maxQueueSize: number`
[IN_MEMORY_SPAN_EXPORTER]: `InMemorySpanExporter.getFinishedSpans() -> ReadableSpan[]` `InMemorySpanExporter.reset() -> void`

## [04]-[SAMPLER_AND_IDS]

Sampling is ONE interface (`Sampler.shouldSample` → `SamplingResult`) with four built-in rows; `ParentBasedSampler` is a COMBINATOR that delegates by parent trace-flags/remoteness, so head-based policy composes from the roster rather than forking a class. `IdGenerator` is the trace/span-id source, `RandomIdGenerator` its `./platform` row (node: `crypto`; browser: `Math.random`).

| [INDEX] | [SYMBOL]                               | [KIND]             | [CONFIG_AXIS_DECISION]                                          |
| :-----: | :------------------------------------- | :----------------- | :-------------------------------------------------------------- |
|  [01]   | `Sampler` / `SamplingResult`           | interface          | `shouldSample(ctx, traceId, name, kind, attrs, links)`          |
|  [02]   | `SamplingDecision`                     | enum               | `NOT_RECORD` / `RECORD` / `RECORD_AND_SAMPLED`                  |
|  [03]   | `AlwaysOnSampler` / `AlwaysOffSampler` | class              | unconditional record / drop                                     |
|  [04]   | `TraceIdRatioBasedSampler`             | class              | `constructor(ratio?)` — deterministic head sampling by trace-id |
|  [05]   | `ParentBasedSampler`                   | class (combinator) | delegates to `{ root, remote/localParent{Sampled,NotSampled} }` |
|  [06]   | `IdGenerator` / `RandomIdGenerator`    | interface/class    | 32-hex trace id + 16-hex span id; platform-random               |

[SAMPLER]: `Sampler.shouldSample(Context,string,string,SpanKind,Attributes,Link[]) -> SamplingResult` `Sampler.toString() -> string`
[SAMPLING_RESULT]: `SamplingResult.decision: SamplingDecision` `SamplingResult.attributes: Readonly<Attributes>` `SamplingResult.traceState: TraceState`
[PARENT_BASED_SAMPLER]: `ParentBasedSampler({…})`
[TRACE_ID_RATIO_BASED_SAMPLER]: `TraceIdRatioBasedSampler(number?)`
[ID_GENERATOR]: `IdGenerator.generateTraceId() -> string` `IdGenerator.generateSpanId() -> string`

## [05]-[STACKING]

- Stack with `@effect/opentelemetry` `NodeSdk`/`WebSdk`: the primary consumer. `NodeSdk.Configuration.spanProcessor: SpanProcessor | ReadonlyArray<SpanProcessor>` and `tracerConfig: Omit<TracerConfig, "resource">` are exactly this package's types (verified import). Effect wires them via `layerTracerProvider(spanProcessor, tracerConfig)` and constructs the provider; `otel/emit` passes `new BatchSpanProcessor(otlpExporter)` and a `{ sampler: new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio) }) }` tracerConfig — never a `BasicTracerProvider` directly, and never `TracerConfig.resource` (the facade's resource options own identity).
- Stack with sibling `exporter-trace-otlp-http`: `OTLPTraceExporter implements SpanExporter`; a `BatchSpanProcessor(new OTLPTraceExporter(opts))` is the production trace pipeline. The `SpanExporter` interface is defined HERE; the OTLP-HTTP sibling and any vendor exporter are rows on it. `ExportResult`/`ExportResultCode` (the `resultCallback` payload) come from `@opentelemetry/core`.
- Stack with `@opentelemetry/resources` + `semantic-conventions`: `TracerConfig.resource: Resource` carries the `AppIdentity`-derived resource; `ReadableSpan.attributes` keys are `semantic-conventions` vocabulary rows (`telemetry/core/observe/convention`), never string literals. `semantic-conventions` is the sole survivor of the `[OTEL_PIN_BLOCK]` collapse.
- Stack with effect-native instrumentation: application code emits through `Effect.withSpan`; the facade's `Tracer` bridge feeds those spans into this pipeline. No `plane:runtime` folder imports `sdk-trace-base` (edge-ledger `scope:runtime` ban); the pipeline is constructed once at the composition root.

## [06]-[RAIL_LAW]

- Owns: the runtime-neutral trace-export pipeline — `SpanProcessor`/`SpanExporter` contracts + the `Simple`/`Batch`/`Noop` and `Console`/`InMemory` rows, the `Sampler` algebra + `ParentBased` combinator, the `IdGenerator`, `TracerConfig`/`SpanLimits`, and the `ReadableSpan`/`Span` recorded shape. `BasicTracerProvider` is the base `sdk-trace-node` extends.
- Accept: `BatchSpanProcessor` wrapping an `OTLPTraceExporter` for production; `ParentBasedSampler({ root: TraceIdRatioBasedSampler(ratio) })` for head sampling; `InMemorySpanExporter` for kit-driven specs; the whole surface reached through `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration`, never constructed inline in instrumentation.
- Reject: `SimpleSpanProcessor` in production (per-span sync export — diagnostics only); setting `TracerConfig.resource` under the effect facade (the `Resource` layer owns it); importing this package outside `scope:runtime`; a hand-rolled span exporter where the `SpanExporter` interface + an OTLP/vendor row suffices; treating this leg as permanent — it collapses at `[OTEL_PIN_BLOCK]`.
- Boundary: `BasicTracerProvider` is directly instantiated only in a pure-SDK path; under effect the node lane's `NodeTracerProvider` (sdk-trace-node) is the wrapped provider. `BatchSpanProcessorBase<T>`, `ForceFlushState`, and the `[inspectCustom]` node-console hook are internal (not barrel exports) — consume the concrete `BatchSpanProcessor`.
