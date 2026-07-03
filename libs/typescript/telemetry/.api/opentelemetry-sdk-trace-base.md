# [@opentelemetry/sdk-trace-base] — the runtime-neutral trace SDK leg of the bridging pin block

`@opentelemetry/sdk-trace-base` owns the trace-export pipeline the SDK-bridge lane wires into: the `SpanProcessor` → `SpanExporter` contract, the `Sampler` algebra, the `IdGenerator`, and the `TracerConfig` construction bag `BasicTracerProvider` reads. `telemetry/otlp/export` never touches it directly — `@effect/opentelemetry` `NodeSdk`/`WebSdk` construct the provider from a `Configuration` whose `spanProcessor`/`tracerConfig` slots ARE this package's `SpanProcessor` and `TracerConfig` (verified: `NodeSdk.d.ts` imports `SpanProcessor, TracerConfig` from here). `sdk-trace-node` re-exports this entire surface and adds only `NodeTracerProvider`, so this leg is the one place the trace roster is defined. It is a `[R3]` collapse target: when the native `@effect/opentelemetry` `Otlp` lane reaches parity the whole `@opentelemetry/sdk-*` block retires and only `semantic-conventions` survives.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-base`
- package: `@opentelemetry/sdk-trace-base` · version `2.8.0` · license `Apache-2.0`
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM mirror (`build/esm/index.js`, `module`); flat barrel, no `exports` subpath map, one `.d.ts` per concern under `build/src/{export,sampler,platform}`.
- asset: TSDECL `build/src/index.d.ts` (`assay api resolve @opentelemetry/sdk-trace-base` → `2.8.0`, restored).
- peer: `@opentelemetry/api >=1.3.0 <1.10.0` — the version-pinned API contract; deps `@opentelemetry/core` (`ExportResult`/`ExportResultCode`/`InstrumentationScope`), `@opentelemetry/resources` (`Resource`), `@opentelemetry/semantic-conventions`.
- runtime: runtime-neutral — the `./platform` conditional export swaps the node crypto `RandomIdGenerator` + `BatchSpanProcessor<BufferConfig>` for the browser `Math.random` generator + `BatchSpanProcessor<BatchSpanProcessorBrowserConfig>`; no fork in consumer code.
- plane: `plane:runtime`, edge-ledger-fenced to `scope:telemetry` — no folder outside `telemetry` imports it.
- rail: observability/sdk-bridge; `[R3]` collapse target — only `semantic-conventions` survives the pin-block retirement.
- role: the `SpanProcessor`/`SpanExporter`/`Sampler`/`IdGenerator` roster behind `@effect/opentelemetry` `NodeSdk`/`WebSdk`; re-exported wholesale by `sdk-trace-node`.

## [02]-[PROVIDER]

`BasicTracerProvider` is the platform-extensible provider (`NodeTracerProvider`/`WebTracerProvider` subclass it); `TracerConfig` is its one construction bag, and every axis on it — sampler, limits, id generator, processor list — is a policy value, never a subclass. Under the effect facade the `resource` field is supplied by `Resource.layerFromEnv` (`AppIdentity`) and `Omit`'d from the `Configuration`, so a consumer never sets it here.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                                  |
| :-----: | :-------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `BasicTracerProvider`             | class         | `implements TracerProvider`; base for `NodeTracerProvider` (sdk-trace-node) |
|  [02]   | `TracerConfig`                    | interface     | provider bag — `sampler`/`spanLimits`/`idGenerator`/`spanProcessors`   |
|  [03]   | `SpanLimits` / `GeneralLimits`    | interface     | per-span vs global attribute/link/event count + value-length caps      |
|  [04]   | `SDKRegistrationConfig`           | interface     | `propagator?`/`contextManager?` for the global `register()` path       |
|  [05]   | `ReadableSpan` / `Span`           | type          | the recorded-span read shape; `Span = APISpan & ReadableSpan`          |
|  [06]   | `TimedEvent`                      | type          | a span event with `HrTime` — the `events[]` element                    |

```ts contract
// The construction bag. Under @effect/opentelemetry `resource` is Omit'd — the Resource layer owns identity.
interface TracerConfig {
  sampler?: Sampler                         // default AlwaysOnSampler; wrap in ParentBasedSampler for parent respect
  generalLimits?: GeneralLimits             // attributeCountLimit / attributeValueLengthLimit (trace-wide)
  spanLimits?: SpanLimits                    // + linkCountLimit / eventCountLimit / attributePer{Event,Link}CountLimit
  resource?: Resource                        // AppIdentity-derived; supplied by the effect Resource layer
  idGenerator?: IdGenerator                  // default RandomIdGenerator (crypto on node)
  forceFlushTimeoutMillis?: number           // default 30000
  spanProcessors?: SpanProcessor[]           // the export pipeline — one or many, fanned by MultiSpanProcessor
  meterProvider?: MeterProvider              // @experimental — self-observability metrics
}
declare class BasicTracerProvider implements TracerProvider {
  constructor(config?: TracerConfig)
  getTracer(name: string, version?: string, options?: { schemaUrl?: string }): Tracer
  forceFlush(): Promise<void>                // drains every processor
  shutdown(): Promise<void>
}
// ReadableSpan is the immutable projection every SpanExporter/SpanProcessor.onEnd receives.
interface ReadableSpan {
  readonly name: string; readonly kind: SpanKind; readonly spanContext: () => SpanContext
  readonly parentSpanContext?: SpanContext; readonly startTime: HrTime; readonly endTime: HrTime; readonly duration: HrTime
  readonly status: SpanStatus; readonly attributes: Attributes; readonly links: Link[]; readonly events: TimedEvent[]
  readonly ended: boolean; readonly resource: Resource; readonly instrumentationScope: InstrumentationScope
  readonly droppedAttributesCount: number; readonly droppedEventsCount: number; readonly droppedLinksCount: number
}
```

## [03]-[PROCESSOR_AND_EXPORTER]

The pipeline is TWO parameterized interfaces, not a fixed set of pairs. `SpanProcessor` owns the start/end/flush lifecycle; `SpanExporter` owns the format/transport. The built-in classes are ROWS on those interfaces — `Simple` (per-span, synchronous, diagnostics-only), `Batch` (queued, the production row, parameterized on `BufferConfig`), `Noop` (drop) — and a custom transport is a new `SpanExporter`, a custom lifecycle a new `SpanProcessor`, never a fork. `BatchSpanProcessor` is the `./platform` specialization of the internal `BatchSpanProcessorBase<T extends BufferConfig>` — node binds `T = BufferConfig`, browser `T = BatchSpanProcessorBrowserConfig`.

| [INDEX] | [SYMBOL]                              | [KIND]                    | [CAPABILITY / BOUNDARY]                                          |
| :-----: | :------------------------------------ | :------------------------ | :--------------------------------------------------------------- |
|  [01]   | `SpanProcessor`                       | interface                 | `onStart`/`onEnding?`/`onEnd`/`forceFlush`/`shutdown` lifecycle   |
|  [02]   | `SimpleSpanProcessor`                 | class                     | one export per ended span; sync; diagnostics/test only           |
|  [03]   | `BatchSpanProcessor`                  | class (`./platform`)      | queued batch export; the production row; `BufferConfig`-tuned    |
|  [04]   | `NoopSpanProcessor`                   | class                     | drop-all — the disabled-signal row                               |
|  [05]   | `BufferConfig` / `BatchSpanProcessorBrowserConfig` | interface    | batch tuning; browser adds `disableAutoFlushOnDocumentHide`      |
|  [06]   | `SpanExporter`                        | interface                 | `export(spans, cb)`/`shutdown`/`forceFlush?` — format + transport |
|  [07]   | `ConsoleSpanExporter`                 | class                     | stdout diagnostics                                               |
|  [08]   | `InMemorySpanExporter`                | class                     | `getFinishedSpans()`/`reset()` — the kit-driven spec-assert lane     |

```ts contract
interface SpanProcessor {
  onStart(span: Span, parentContext: Context): void
  onEnding?(span: Span): void                                    // @experimental — mutate before read-only freeze
  onEnd(span: ReadableSpan): void
  forceFlush(): Promise<void>; shutdown(): Promise<void>
}
interface SpanExporter {
  export(spans: ReadableSpan[], resultCallback: (result: ExportResult) => void): void   // ExportResult from @opentelemetry/core
  shutdown(): Promise<void>; forceFlush?(): Promise<void>
}
declare class SimpleSpanProcessor implements SpanProcessor { constructor(exporter: SpanExporter) }
declare abstract class BatchSpanProcessorBase<T extends BufferConfig> implements SpanProcessor { constructor(exporter: SpanExporter, config?: T) }   // internal base — not barrel-exported
// ./platform node → `class BatchSpanProcessor extends BatchSpanProcessorBase<BufferConfig>`  (the barrel export)
interface BufferConfig {
  maxExportBatchSize?: number     // 512  — must be ≤ maxQueueSize
  scheduledDelayMillis?: number   // 5000
  exportTimeoutMillis?: number    // 30000
  maxQueueSize?: number           // 2048 — overflow drops spans
}
declare class InMemorySpanExporter implements SpanExporter { getFinishedSpans(): ReadableSpan[]; reset(): void }
```

## [04]-[SAMPLER_AND_IDS]

Sampling is ONE interface (`Sampler.shouldSample` → `SamplingResult`) with four built-in rows; `ParentBasedSampler` is a COMBINATOR that delegates by parent trace-flags/remoteness, so head-based policy composes from the roster rather than forking a class. `IdGenerator` is the trace/span-id source, `RandomIdGenerator` its `./platform` row (node: `crypto`; browser: `Math.random`).

| [INDEX] | [SYMBOL]                          | [KIND]              | [CONFIG AXIS / DECISION]                                        |
| :-----: | :-------------------------------- | :------------------ | :-------------------------------------------------------------- |
|  [01]   | `Sampler` / `SamplingResult`      | interface           | `shouldSample(ctx, traceId, name, kind, attrs, links)`          |
|  [02]   | `SamplingDecision`                | enum                | `NOT_RECORD` / `RECORD` / `RECORD_AND_SAMPLED`                   |
|  [03]   | `AlwaysOnSampler` / `AlwaysOffSampler` | class          | unconditional record / drop                                     |
|  [04]   | `TraceIdRatioBasedSampler`        | class               | `constructor(ratio?)` — deterministic head sampling by trace-id |
|  [05]   | `ParentBasedSampler`              | class (combinator)  | delegates to `{ root, remote/localParent{Sampled,NotSampled} }` |
|  [06]   | `IdGenerator` / `RandomIdGenerator` | interface/class   | 32-hex trace id + 16-hex span id; platform-random               |

```ts contract
interface Sampler {
  shouldSample(context: Context, traceId: string, spanName: string, spanKind: SpanKind, attributes: Attributes, links: Link[]): SamplingResult
  toString(): string
}
interface SamplingResult { decision: SamplingDecision; attributes?: Readonly<Attributes>; traceState?: TraceState }
// The combinator — respect an inbound sampled decision, sample fresh roots at a ratio:
declare class ParentBasedSampler implements Sampler {
  constructor(config: { root: Sampler; remoteParentSampled?: Sampler; remoteParentNotSampled?: Sampler; localParentSampled?: Sampler; localParentNotSampled?: Sampler })
}
declare class TraceIdRatioBasedSampler implements Sampler { constructor(ratio?: number) }
// new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(0.1) })  — the canonical production sampler
interface IdGenerator { generateTraceId(): string; generateSpanId(): string }
```

## [05]-[STACKING]

- [STACK: `@effect/opentelemetry` `NodeSdk`/`WebSdk`] — the primary consumer. `NodeSdk.Configuration.spanProcessor: SpanProcessor | ReadonlyArray<SpanProcessor>` and `tracerConfig: Omit<TracerConfig, "resource">` are exactly this package's types (verified import). Effect wires them via `layerTracerProvider(spanProcessor, tracerConfig)` and constructs the provider; `telemetry/otlp/export` passes `new BatchSpanProcessor(otlpExporter)` and a `{ sampler: new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio) }) }` tracerConfig — never a `BasicTracerProvider` directly, and never the `resource` field (the effect `Resource` layer owns identity).
- [STACK: sibling `exporter-trace-otlp-http`] — `OTLPTraceExporter implements SpanExporter`; a `BatchSpanProcessor(new OTLPTraceExporter(opts))` is the production trace pipeline. The `SpanExporter` interface is defined HERE; the OTLP-HTTP sibling and any vendor exporter are rows on it. `ExportResult`/`ExportResultCode` (the `resultCallback` payload) come from `@opentelemetry/core`.
- [STACK: `@opentelemetry/resources` + `semantic-conventions`] — `TracerConfig.resource: Resource` carries the `AppIdentity`-derived resource; `ReadableSpan.attributes` keys are `semantic-conventions` vocabulary rows (`telemetry/signal/convention`), never string literals. `semantic-conventions` is the sole survivor of the `[R3]` collapse.
- [STACK: effect-native instrumentation] — application code emits through `Effect.withSpan`; the facade's `Tracer` bridge feeds those spans into this pipeline. No `plane:runtime` folder imports `sdk-trace-base` (edge-ledger `scope:telemetry` ban); the pipeline is constructed once at the composition root.

## [06]-[RAIL_LAW]

- Owns: the runtime-neutral trace-export pipeline — `SpanProcessor`/`SpanExporter` contracts + the `Simple`/`Batch`/`Noop` and `Console`/`InMemory` rows, the `Sampler` algebra + `ParentBased` combinator, the `IdGenerator`, `TracerConfig`/`SpanLimits`, and the `ReadableSpan`/`Span` recorded shape. `BasicTracerProvider` is the base `sdk-trace-node` extends.
- Accept: `BatchSpanProcessor` wrapping an `OTLPTraceExporter` for production; `ParentBasedSampler({ root: TraceIdRatioBasedSampler(ratio) })` for head sampling; `InMemorySpanExporter` for kit-driven specs; the whole surface reached through `@effect/opentelemetry` `NodeSdk`/`WebSdk` `Configuration`, never constructed inline in instrumentation.
- Reject: `SimpleSpanProcessor` in production (per-span sync export — diagnostics only); setting `TracerConfig.resource` under the effect facade (the `Resource` layer owns it); importing this package outside `scope:telemetry`; a hand-rolled span exporter where the `SpanExporter` interface + an OTLP/vendor row suffices; treating this leg as permanent — it collapses at `[R3]`.
- Boundary: `BasicTracerProvider` is directly instantiated only in a pure-SDK path; under effect the node lane's `NodeTracerProvider` (sdk-trace-node) is the wrapped provider. `BatchSpanProcessorBase<T>`, `ForceFlushState`, and the `[inspectCustom]` node-console hook are internal (not barrel exports) — consume the concrete `BatchSpanProcessor`.
