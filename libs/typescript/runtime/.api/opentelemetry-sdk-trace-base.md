# [TS_RUNTIME_API_OPENTELEMETRY_SDK_TRACE_BASE]

`@opentelemetry/sdk-trace-base` owns the trace-export pipeline: the `SpanProcessor`→`SpanExporter` lifecycle contract, the `Sampler` algebra with its `ParentBasedSampler` combinator, the `IdGenerator`, and the `TracerConfig` bag `BasicTracerProvider` reads.

`@effect/opentelemetry` `NodeSdk`/`WebSdk` build the provider from these `SpanProcessor`/`TracerConfig` types, `sdk-trace-node`/`-web` re-export the whole surface, and the leg collapses at `[OTEL_PIN_BLOCK]` on native `Otlp` parity.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-base`
- package: `@opentelemetry/sdk-trace-base` (Apache-2.0)
- module: CJS default + ESM mirror, flat barrel, no `exports` subpath map; an env-fallback shim re-exporting the `@opentelemetry/sdk-trace` trace roster under `OTEL_*` variable defaults
- runtime: runtime-neutral — the `@opentelemetry/sdk-trace` `./platform` split binds node crypto ids + `BufferConfig` or browser `Math.random` + `BatchSpanProcessorBrowserConfig` with no consumer fork; the `@opentelemetry/api` peer floor rides the workspace catalog
- depends: `@opentelemetry/sdk-trace` (the re-exported roster), `@opentelemetry/core` (`ExportResult`/`InstrumentationScope`), `@opentelemetry/resources` (`Resource`), `@opentelemetry/semantic-conventions`
- rail: observability/sdk-bridge; `[OTEL_PIN_BLOCK]` collapse target — only `semantic-conventions` survives the retirement

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the pipeline contracts, the sampler and id algebra, and the provider construction bags

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `SpanProcessor`                                    | interface      | start/end/flush lifecycle each recorded span folds through |
|  [02]   | `SpanExporter`                                     | interface      | `export(spans, cb)`/`shutdown` — format and transport      |
|  [03]   | `Sampler` / `SamplingResult`                       | interface      | `shouldSample` decision with `attributes`/`traceState`     |
|  [04]   | `SamplingDecision`                                 | enum           | `NOT_RECORD`/`RECORD`/`RECORD_AND_SAMPLED` decision        |
|  [05]   | `IdGenerator`                                      | interface      | 32-hex trace id and 16-hex span id source                  |
|  [06]   | `ReadableSpan` / `Span`                            | interface/type | recorded read shape; `Span = APISpan & ReadableSpan`       |
|  [07]   | `TimedEvent`                                       | interface      | `HrTime`-stamped span event, the `events[]` element        |
|  [08]   | `TracerConfig`                                     | interface      | provider bag — sampler/limits/idGenerator/processors axes  |
|  [09]   | `SpanLimits` / `GeneralLimits`                     | interface      | per-span and global count plus value-length caps           |
|  [10]   | `BufferConfig` / `BatchSpanProcessorBrowserConfig` | interface      | node batch tuning; browser adds document-hide toggle       |
|  [11]   | `SDKRegistrationConfig`                            | interface      | `propagator?`/`contextManager?` for `register()`           |
|  [12]   | `BasicTracerProvider`                              | class          | env-fallback provider `sdk-trace-node`/`-web` subclass     |
|  [13]   | `BatchSpanProcessor`                               | class          | queued batch export, the production row                    |
|  [14]   | `SimpleSpanProcessor` / `NoopSpanProcessor`        | class          | synchronous per-span (diagnostics) / drop-all              |
|  [15]   | `ConsoleSpanExporter` / `InMemorySpanExporter`     | class          | stdout diagnostics / spec-assert capture                   |
|  [16]   | `RandomIdGenerator`                                | class          | platform-random ids — node `crypto`, browser `Math.random` |
|  [17]   | `AlwaysOnSampler` / `AlwaysOffSampler`             | class          | unconditional record / drop                                |
|  [18]   | `TraceIdRatioBasedSampler`                         | class          | deterministic head sampling by trace-id ratio              |
|  [19]   | `ParentBasedSampler`                               | class          | combinator delegating by parent sampled/remote flags       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider, processor, exporter, and sampler construction with the SDK-driven lifecycle hooks

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new BasicTracerProvider(TracerConfig?)`                         | ctor     | env-fallback provider, pure-SDK path only   |
|  [02]   | `provider.getTracer(string, string?, {schemaUrl?})` -> `Tracer`  | instance | tracer handle keyed by name and scope       |
|  [03]   | `provider.forceFlush()` / `provider.shutdown() -> Promise<void>` | instance | drain processors / tear the provider down   |
|  [04]   | `new BatchSpanProcessor(SpanExporter, BufferConfig?)`            | ctor     | production processor wrapping one exporter  |
|  [05]   | `new SimpleSpanProcessor(SpanExporter)`                          | ctor     | synchronous per-span processor, diagnostics |
|  [06]   | `processor.onStart(Span, Context)` / `onEnd(ReadableSpan)`       | instance | SDK-driven start and end hooks              |
|  [07]   | `processor.forceFlush()` / `shutdown() -> Promise<void>`         | instance | flush queued spans / stop the processor     |
|  [08]   | `exporter.export(ReadableSpan[], (ExportResult) => void)`        | instance | custom-exporter transport hook              |
|  [09]   | `new InMemorySpanExporter()`                                     | ctor     | spec-capture exporter                       |
|  [10]   | `exporter.getFinishedSpans() -> ReadableSpan[]` / `.reset()`     | instance | read and clear captured spans in specs      |
|  [11]   | `new TraceIdRatioBasedSampler(number?)`                          | ctor     | head sampler keeping a trace-id fraction    |
|  [12]   | `new ParentBasedSampler({ root, ... })`                          | ctor     | parent-decision combinator over `root`      |
|  [13]   | `sampler.shouldSample(Context, ...) -> SamplingResult`           | instance | the custom-sampler decision hook            |
|  [14]   | `idGenerator.generateTraceId()` / `.generateSpanId() -> string`  | instance | 32/16-hex id source                         |

- `BasicTracerProvider`: carries no `register()` — the global propagator/context install is the `NodeTracerProvider`/`WebTracerProvider` subclass op that `SDKRegistrationConfig` types.
- `Sampler.shouldSample`: takes `(Context, traceId, spanName, SpanKind, Attributes, Link[])`; `TraceIdRatioBasedSampler` reads only `(context, traceId)` despite the six-argument interface.
- `ParentBasedSampler`: config carries `root` required with `remoteParentSampled?`/`remoteParentNotSampled?`/`localParentSampled?`/`localParentNotSampled?` overrides.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Sampling flows head-first: `ParentBasedSampler` reads the parent span-context's sampled and remote flags and delegates to `root` only for a parentless span, where `TraceIdRatioBasedSampler` maps the trace-id to keep-below-ratio; one `shouldSample` fires per span start and the decision holds for the whole trace.
- `SpanProcessor` receives `onStart` at span creation and `onEnd` at span close; `BatchSpanProcessor` enqueues each ended `ReadableSpan` and drains on `scheduledDelayMillis`, `maxExportBatchSize`, or `forceFlush`, so export runs asynchronous and batched, while `SimpleSpanProcessor` exports synchronously per span and belongs to diagnostics.
- `SpanExporter.export` reports terminal disposition through the `@opentelemetry/core` `ExportResult` callback, never a throw; a custom transport lands as a `SpanExporter` row and a custom lifecycle as a `SpanProcessor` row, never a provider subclass.
- Shim constructors layer `OTEL_*` environment fallbacks over the `@opentelemetry/sdk-trace` constructors and restore `SimpleSpanProcessor`'s single-exporter signature; `BasicTracerProvider` instantiates directly only on a pure-SDK path.

[STACKING]:
- `@effect/opentelemetry`(`.api/effect-opentelemetry.md`): `NodeSdk.Configuration.spanProcessor: SpanProcessor | ReadonlyArray<SpanProcessor>` and `tracerConfig: Omit<TracerConfig, "resource">` import these two types directly, and `layerTracerProvider(spanProcessor, config?)` builds the provider — so `otel/emit` passes `new BatchSpanProcessor(otlpExporter)` and `{ sampler: new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio) }) }`, never `BasicTracerProvider` and never `TracerConfig.resource`.
- `opentelemetry-sdk-trace-node`(`.api/opentelemetry-sdk-trace-node.md`) `NodeTracerProvider` and `opentelemetry-sdk-trace-web`(`.api/opentelemetry-sdk-trace-web.md`) `WebTracerProvider` subclass `BasicTracerProvider` and re-export this whole roster for one node or browser import site; their `register(SDKRegistrationConfig?)` installs the context spine this base omits.
- `opentelemetry-exporter-trace-otlp-http`(`.api/opentelemetry-exporter-trace-otlp-http.md`) and `opentelemetry-exporter-trace-otlp-proto`(`.api/opentelemetry-exporter-trace-otlp-proto.md`) `OTLPTraceExporter implements SpanExporter` — the interface declares here, the JSON and protobuf exporters are rows on it, each wrapped by `new BatchSpanProcessor(new OTLPTraceExporter(cfg))` for the production trace leg.
- `opentelemetry-baggage-span-processor`(`.api/opentelemetry-baggage-span-processor.md`) `BaggageSpanProcessor implements SpanProcessor` folds into `TracerConfig.spanProcessors`, its `onStart` stamping admitted `rasm.*` baggage onto each child span before the exporting processor runs.
- `otel/emit` (within-lib): the export-boundary owner constructs the processor and sampler once at the composition root and feeds them through the facade `Configuration`; instrumentation emits through `Effect.withSpan`, so `ReadableSpan.attributes` keys stay `semantic-conventions` vocabulary rows scrubbed of PII before `SpanExporter.export`.

[LOCAL_ADMISSION]:
- `@opentelemetry/sdk-trace-base` admits only inside `scope:runtime` (edge-ledger ban); no folder outside `otel` constructs a processor, sampler, or provider, and `.api/effect-opentelemetry.md` owns the `[OTEL_PIN_BLOCK]` survive-and-collapse roster.

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-trace-base`
- Owns: the runtime-neutral trace-export pipeline — the `SpanProcessor`/`SpanExporter` contracts with the `Batch`/`Simple`/`Noop` and `Console`/`InMemory` rows, the `Sampler` algebra with the `ParentBased` combinator over `TraceIdRatioBased`/`AlwaysOn`/`AlwaysOff` rows, the `IdGenerator`, `TracerConfig`/`SpanLimits`, and the `ReadableSpan`/`Span` recorded shape; `BasicTracerProvider` is the base `sdk-trace-node`/`-web` extend
- Accept: `new BatchSpanProcessor` wrapping an `OTLPTraceExporter` for production; `ParentBasedSampler({ root: TraceIdRatioBasedSampler(ratio) })` for head sampling; `InMemorySpanExporter` for kit-driven specs; the surface reached through `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration`
- Reject: `SimpleSpanProcessor` in production, `TracerConfig.resource` under the facade, imports outside `scope:runtime`, a hand-rolled exporter where a `SpanExporter` row suffices, treating this leg as permanent past `[OTEL_PIN_BLOCK]`
