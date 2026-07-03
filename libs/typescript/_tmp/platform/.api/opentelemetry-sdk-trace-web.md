# [API_CATALOGUE] @opentelemetry/sdk-trace-web

`@opentelemetry/sdk-trace-web` supplies `WebTracerProvider` (the browser `BasicTracerProvider` subclass that re-adds the `register` method for global OTel-API binding), `StackContextManager` (the synchronous DOM call-stack context manager), the `PerformanceTimingNames` + browser span network-enrichment utilities, and a verbatim re-export of the full `@opentelemetry/sdk-trace-base` surface — the sampler/processor/exporter CLASSES and, critically, the contract TYPES (`SpanProcessor`, `SpanExporter`, `TracerConfig`, `SDKRegistrationConfig`, `ReadableSpan`, `Sampler`, `BatchSpanProcessorBrowserConfig`) that `@effect/opentelemetry` `WebSdk.Configuration` composes. In the pinned WebSdk lane the design page constructs a `BatchSpanProcessor(new OTLPTraceExporter(...))` for `WebSdk.Configuration.spanProcessor` and a `ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio) })` for `tracerConfig.sampler`; it separately constructs a standalone `new WebTracerProvider().register({ propagator })` to install the `@opentelemetry/core` `CompositePropagator` globally, because `WebSdk.Configuration` carries no `propagator` field and its built provider exposes no `.register` handle.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-web`
- package: `@opentelemetry/sdk-trace-web`
- version: `2.8.0` (central pin `pnpm-workspace.yaml`; matches `@effect/opentelemetry@0.63.0` peer resolution)
- license: `Apache-2.0`
- api-peer: `@opentelemetry/api ^1.9.x` — `Context`/`ContextManager`/`Span`/`TextMapPropagator`; re-exports `@opentelemetry/sdk-trace-base@2.8.0` and consumes `@opentelemetry/resources` `Resource` (via `TracerConfig.resource`)
- module: `@opentelemetry/sdk-trace-web` (barrel `build/src/index.d.ts`; the base re-export is a `from '@opentelemetry/sdk-trace-base'` pass-through)
- runtime: BROWSER-ONLY — `StackContextManager`/`WebTracerProvider`/the Performance-API utilities are DOM-bound; the node lane is `@opentelemetry/sdk-trace-node`, selected at the app root, never a fork
- asset: runtime library — side-effects-free (`sideEffects: false`), tree-shakeable
- rail: tracing
- collapse-fence: SDK-bridge peer block, fenced to `scope:telemetry`, COLLAPSES at `[R3]` when the native `@effect/opentelemetry` `OtlpTracer.layer` reaches parity (`libs/typescript/.api/effect-opentelemetry.md`) — the browser span pipeline (`BatchSpanProcessor` + `WebTracerProvider`) retires while the propagation (`@opentelemetry/core`) / resource-identity (`@opentelemetry/resources`) / convention (`@opentelemetry/semantic-conventions`) vocabulary survives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and context-manager family
- rail: tracing

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                      |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `WebTracerProvider`   | class         | `BasicTracerProvider` subclass; adds web `register(config?)` |
|  [02]   | `WebTracerConfig`     | type alias    | `= TracerConfig` (no extra web fields)                    |
|  [03]   | `StackContextManager` | class         | synchronous DOM stack `ContextManager` (`enable`/`with`/`bind`) |

[PUBLIC_TYPE_SCOPE]: performance-timing family (browser span enrichment)
- rail: tracing

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                  |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------------- |
|  [01]   | `PerformanceTimingNames`        | enum          | `PerformanceResourceTiming` entry-name constants      |
|  [02]   | `PerformanceEntries`            | type          | keyed timing-value record                             |
|  [03]   | `PerformanceLegacy`             | interface     | Safari fallback timing shape                          |
|  [04]   | `PerformanceResourceTimingInfo` | interface     | main + preflight resource-timing pair                 |
|  [05]   | `PropagateTraceHeaderCorsUrls`  | type          | URL pattern set for cross-origin header propagation   |
|  [06]   | `URLLike`                       | interface     | `URL` + anchor-element compatible shape               |

[PUBLIC_TYPE_SCOPE]: sdk-trace-base re-exported classes
- rail: tracing

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                  |
| :-----: | :------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `BatchSpanProcessor`       | class         | buffered browser span export (wraps a `SpanExporter`) |
|  [02]   | `SimpleSpanProcessor`      | class         | synchronous per-span export                           |
|  [03]   | `NoopSpanProcessor`        | class         | no-op processor                                       |
|  [04]   | `ParentBasedSampler`       | class         | parent-context sampling delegate                      |
|  [05]   | `TraceIdRatioBasedSampler` | class         | deterministic ratio sampler (`constructor(ratio?)`)   |
|  [06]   | `AlwaysOnSampler` / `AlwaysOffSampler` | class | unconditional accept / reject samplers            |
|  [07]   | `ConsoleSpanExporter` / `InMemorySpanExporter` | class | diagnostic / in-process `SpanExporter` sinks  |
|  [08]   | `BasicTracerProvider`      | class         | base provider (`getTracer`/`forceFlush`/`shutdown`; NO `register` in 2.x) |
|  [09]   | `RandomIdGenerator`        | class         | default random trace/span id generator                |
|  [10]   | `SamplingDecision`         | enum          | `NOT_RECORD = 0`, `RECORD = 1`, `RECORD_AND_SAMPLED = 2` |

[PUBLIC_TYPE_SCOPE]: sdk-trace-base re-exported contract types (`WebSdk.Configuration` composes these)
- rail: tracing
- These TYPE re-exports are the seam `@effect/opentelemetry` `WebSdk.Configuration` binds against (`spanProcessor: SpanProcessor | SpanProcessor[]`, `tracerConfig: Omit<TracerConfig, "resource">`), so they are cited here rather than reached through `sdk-trace-base` directly.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CONSUMER / BOUNDARY]                                     |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `SpanProcessor`                   | interface     | `onStart`/`onEnd`/`forceFlush`/`shutdown` — the processor contract |
|  [02]   | `SpanExporter`                    | interface     | `export(spans, cb)` — the exporter contract (sibling `OTLPTraceExporter`) |
|  [03]   | `TracerConfig`                    | interface     | `sampler?`/`spanProcessors?`/`idGenerator?`/`resource?`/`spanLimits?`/`generalLimits?`/`forceFlushTimeoutMillis?` |
|  [04]   | `SDKRegistrationConfig`           | interface     | `{ propagator?; contextManager? }` — the `register()` arg (NO `idGenerator`) |
|  [05]   | `Sampler` / `SamplingResult`      | interface     | `shouldSample(...) => SamplingResult { decision; attributes?; traceState? }` |
|  [06]   | `ReadableSpan` / `Span` / `TimedEvent` | interface | the exported span shape / mutable span / span event    |
|  [07]   | `BatchSpanProcessorBrowserConfig` | interface     | `extends BufferConfig` + `disableAutoFlushOnDocumentHide?` |
|  [08]   | `BufferConfig`                    | interface     | `maxExportBatchSize?`(512)/`scheduledDelayMillis?`(5000)/`exportTimeoutMillis?`(30000)/`maxQueueSize?`(2048) |
|  [09]   | `SpanLimits` / `GeneralLimits`    | interface     | span/global attribute-count/value-length/link/event caps |
|  [10]   | `IdGenerator`                     | interface     | `generateTraceId`/`generateSpanId` — swappable id source |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: WebTracerProvider construction and global registration
- rail: tracing

```ts
// build/src/WebTracerProvider.d.ts + sdk-trace-base types.d.ts — register adds a WEB-specific method (base has none in 2.x)
export type WebTracerConfig = TracerConfig;
export declare class WebTracerProvider extends BasicTracerProvider {
  constructor(config?: WebTracerConfig);
  register(config?: SDKRegistrationConfig): void;   // propagator + contextManager ONLY
}
export interface SDKRegistrationConfig {
  propagator?: TextMapPropagator | null;            // null skips; undefined -> default
  contextManager?: ContextManager | null;           // defaults to a new StackContextManager when omitted
}
export interface TracerConfig {
  sampler?: Sampler;
  spanProcessors?: SpanProcessor[];                  // 2.x wires processors HERE (no addSpanProcessor)
  idGenerator?: IdGenerator;
  resource?: Resource;                               // @opentelemetry/resources
  spanLimits?: SpanLimits;
  generalLimits?: GeneralLimits;
  forceFlushTimeoutMillis?: number;                  // default 30000
}
```

[ENTRYPOINT_SCOPE]: batch span processor + sampler construction
- rail: tracing

```ts
// sdk-trace-base — the WebSdk-lane span pipeline; BatchSpanProcessor(exporter, config?) with browser tuning
export declare class BatchSpanProcessor implements SpanProcessor {
  constructor(exporter: SpanExporter, config?: BatchSpanProcessorBrowserConfig);
}
export declare class TraceIdRatioBasedSampler implements Sampler { constructor(ratio?: number); }
export declare class ParentBasedSampler implements Sampler {
  constructor(config: {
    root: Sampler;                                   // sampler for spans with NO parent
    remoteParentSampled?: Sampler; remoteParentNotSampled?: Sampler;   // default AlwaysOn / AlwaysOff
    localParentSampled?: Sampler;  localParentNotSampled?: Sampler;
  });
}
export declare enum SamplingDecision { NOT_RECORD = 0, RECORD = 1, RECORD_AND_SAMPLED = 2 }
```

[ENTRYPOINT_SCOPE]: StackContextManager operations
- rail: tracing

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [CONSUMER / BOUNDARY]                       |
| :-----: | :-------------------------------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `new StackContextManager()`                   | manager factory | synchronous DOM stack context manager       |
|  [02]   | `manager.enable(): this` / `manager.disable(): this` | lifecycle | enable (root context) / disable (clear)     |
|  [03]   | `manager.active(): Context`                   | context read    | current active context                      |
|  [04]   | `manager.bind<T>(context, target): T`         | context bind    | attach context to a function/event target   |
|  [05]   | `manager.with(context, fn, thisArg?, ...args)` | context exec   | run `fn` under `context` as active          |

[ENTRYPOINT_SCOPE]: span network-enrichment utilities
- rail: tracing

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [CONSUMER / BOUNDARY]                              |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `addSpanNetworkEvent(span, perfName, entries, ignoreZeros?)`     | span mutation   | add one `PerformanceResourceTiming` event         |
|  [02]   | `addSpanNetworkEvents(span, resource, ignoreNet?, ignoreZeros?)` | span mutation   | add all performance-timing events                 |
|  [03]   | `getResource(url, startHR, endHR, resources, ignored?, type?)`   | resource lookup | match the `PerformanceResourceTiming` entry       |
|  [04]   | `sortResources(filtered)`                                        | sort            | sort resource entries by `startTime` ascending    |
|  [05]   | `parseUrl(url)` / `normalizeUrl(url)`                            | URL parse       | `URL`/anchor fallback parse; parse-then-serialize |
|  [06]   | `getElementXPath(target, optimised?)`                            | DOM query       | XPath string for a DOM element                    |
|  [07]   | `shouldPropagateTraceHeaders(spanUrl, corsUrls?)`               | CORS check      | true when trace headers may cross to `spanUrl`    |
|  [08]   | `hasKey(obj, key)`                                              | type guard      | enum-keyed property existence                     |

## [04]-[IMPLEMENTATION_LAW]

[WEB_TRACER_TOPOLOGY]:
- `WebTracerProvider extends BasicTracerProvider` and ADDS `register(config?: SDKRegistrationConfig): void` — in OTel 2.x `BasicTracerProvider` has NO `register` (only `getTracer`/`forceFlush`/`shutdown`), so `register` is a web-specific re-addition, not an override; `register` binds the provider into the global OTel API and defaults the `contextManager` to a new `StackContextManager` when omitted
- `SDKRegistrationConfig` carries `propagator?` and `contextManager?` ONLY — `idGenerator` is a `TracerConfig` CONSTRUCTOR field, never a `register` field
- span processors are wired at construction via `TracerConfig.spanProcessors` (2.x removed the runtime `addSpanProcessor`); in the WebSdk lane `@effect/opentelemetry` owns this wiring through `WebSdk.Configuration.spanProcessor`
- `StackContextManager` is a SYNCHRONOUS call-stack context manager; it does NOT propagate context across `Promise`/`setTimeout` — async work needs `ZoneContextManager`/`AsyncLocalStorageContextManager`; `enable`/`disable` return `this` for chaining
- `SamplingDecision` is `NOT_RECORD = 0` / `RECORD = 1` / `RECORD_AND_SAMPLED = 2` — there is NO `DROP` member; `Sampler.shouldSample` returns a `SamplingResult { decision; attributes?; traceState? }`
- `WebTracerConfig` is a direct `TracerConfig` alias with no web-specific fields; all `sdk-trace-base` classes are re-exported verbatim for single-import ergonomics

[INTEGRATION_LAW]:
- Stack with `@effect/opentelemetry` `WebSdk`: `WebSdk.Configuration.spanProcessor` takes `SpanProcessor | ReadonlyArray<SpanProcessor>` — pass `new BatchSpanProcessor(new OTLPTraceExporter({ url }))`; `WebSdk.Configuration.tracerConfig` is `Omit<TracerConfig, "resource">` — pass `{ sampler: new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio) }) }`; the WebSdk builds the `WebTracerProvider` internally over the shared `Resource` and yields a `Resource.Resource` layer with no exposed provider handle
- Stack with `@opentelemetry/core` propagators: because the WebSdk provider is unreachable, `Observability/telemetry#TRACE_PROPAGATION` constructs a SEPARATE `new WebTracerProvider().register({ propagator: new CompositePropagator({ propagators: [new W3CTraceContextPropagator(), new W3CBaggagePropagator()] }) })` to install the global propagator the `@effect/opentelemetry` `Tracer` global reads — the standalone-provider path in the current package set (the alternative admits `@opentelemetry/api` `propagation.setGlobalPropagator`)
- Stack with `exporter-trace-otlp-http`: the sibling `OTLPTraceExporter` IS the `SpanExporter` the `BatchSpanProcessor` wraps; its `export(spans, cb)` reports the `@opentelemetry/core` `ExportResult`, and the processor owns batching/retry from `BufferConfig` (`maxExportBatchSize`/`scheduledDelayMillis`/`maxQueueSize`) plus the browser `disableAutoFlushOnDocumentHide` flush-on-hide
- Stack with the Performance API: `addSpanNetworkEvents(span, perfEntry)` reads `PerformanceResourceTiming` from the browser Performance API to enrich a fetch span; pair with `getResource` to correlate the span to its timing entry, and `shouldPropagateTraceHeaders`/`PropagateTraceHeaderCorsUrls` to gate which cross-origin URLs receive `traceparent`/`tracestate`

[LOCAL_ADMISSION]:
- construct `BatchSpanProcessor`/`ParentBasedSampler`/`TraceIdRatioBasedSampler` at the telemetry composition root; the WebSdk lane consumes them via `WebSdk.Configuration`, and the standalone `WebTracerProvider().register({ propagator })` is the only direct provider construction (for global propagator install)
- `register({ propagator })` is reachable ONLY on a self-constructed `WebTracerProvider` — never on the WebSdk-built provider (its `Configuration` has no `propagator` field and yields no `.register` handle)
- `@opentelemetry/sdk-trace-web` is admitted ONLY inside `scope:telemetry`; instrumentation code uses Effect's native `Effect.withSpan`, never this package
- the node lane is `@opentelemetry/sdk-trace-node`, selected at the app root — never import both in one bundle

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-trace-web`
- Owns: the browser `TracerProvider` with its web-specific `register`, the synchronous `StackContextManager`, the Performance-API span enrichment, and the verbatim `sdk-trace-base` class + contract-type re-export
- Accept: `BatchSpanProcessor(OTLPTraceExporter)` as `WebSdk.Configuration.spanProcessor`; `ParentBasedSampler({ root: TraceIdRatioBasedSampler })` as `tracerConfig.sampler`; a standalone `new WebTracerProvider().register({ propagator })` for global `CompositePropagator` install; `StackContextManager` for synchronous context
- Reject: a `register({ ..., idGenerator })` call (`idGenerator` is a `TracerConfig` field); a `SamplingDecision.DROP` reference (the member is `NOT_RECORD`); `addSpanProcessor` (removed in 2.x — use `TracerConfig.spanProcessors`); hand-rolled browser context managers or direct `BasicTracerProvider` use; the node exporter/provider variant in browser code
