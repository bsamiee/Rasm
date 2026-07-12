# [TS_RUNTIME_API_OPENTELEMETRY_SDK_TRACE_NODE]

`@opentelemetry/sdk-trace-node` is `sdk-trace-base` plus ONE net-new capability: `NodeTracerProvider`, a `BasicTracerProvider` subclass whose `register()` installs the Node async-context spine — the `AsyncLocalStorageContextManager` (from `@opentelemetry/context-async-hooks`) and the W3C `CompositePropagator` (`W3CTraceContextPropagator` + `W3CBaggagePropagator`, from `@opentelemetry/core`). That async-local-storage context manager is the whole reason this leg is distinct from base: it makes span parenting survive `await`/callback boundaries in Node with no manual context threading. The barrel re-exports the ENTIRE `sdk-trace-base` roster (samplers, processors, exporters, id generator, every type — catalogued in `opentelemetry-sdk-trace-base.md`), so a node consumer has one import site. `@effect/opentelemetry` `NodeSdk` constructs `new NodeTracerProvider({ ...tracerConfig, resource, spanProcessors })` (verified in `NodeSdk.js`) and manages it inside the Effect runtime WITHOUT calling `.register()`; the global-registration path is the pure-SDK (non-Effect) usage. It collapses with the pin block at `[R3]`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-node`
- package: `@opentelemetry/sdk-trace-node` · version `` · license `Apache-2.0`
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM mirror (`build/esm/index.js`); flat barrel, no `exports` subpath map.
- asset: TSDECL `build/src/index.d.ts` (restored).
- peer: `@opentelemetry/api >=catalog <catalog`; deps `@opentelemetry/context-async-hooks` (`AsyncLocalStorageContextManager`), `@opentelemetry/core` (the W catalogC propagators), `@opentelemetry/sdk-trace-base` (the re-exported roster).
- runtime: node/bun only — the async-local-storage context manager is a node runtime dependency; the browser counterpart is `sdk-trace-web` (`WebTracerProvider`, `StackContextManager`).
- plane: `plane:runtime` / `plane:server`, edge-ledger-fenced to `scope:runtime`.
- rail: observability/sdk-bridge; `[R3]` collapse target.
- role: backs the `@effect/opentelemetry` `NodeSdk` layer (the `telemetry` NodeSdk export row); supplies the node `NodeTracerProvider` effect wraps, and re-exports the whole base trace roster.

## [02]-[NODE_PROVIDER]

The only symbol this leg adds over base. `NodeTracerConfig` is a pure alias of `TracerConfig` — no node-specific config axis; the node-ness is entirely in `register()`, whose `SDKRegistrationConfig` selects the global context manager and propagator. Passing `null` for either skips that global install; `undefined` takes the node default.

| [INDEX] | [SYMBOL]                | [KIND]                | [CAPABILITY_BOUNDARY]                                                 |
| :-----: | :---------------------- | :-------------------- | :-------------------------------------------------------------------- |
|  [01]   | `NodeTracerProvider`    | class                 | `extends BasicTracerProvider`; `register()` installs the node globals |
|  [02]   | `NodeTracerConfig`      | type alias            | `= TracerConfig` — no node-specific field                             |
|  [03]   | `SDKRegistrationConfig` | interface (re-export) | `register()` arg — `{ propagator?, contextManager? }`                 |

```ts contract
declare class NodeTracerProvider extends BasicTracerProvider {
  constructor(config?: NodeTracerConfig)                        // NodeTracerConfig = TracerConfig
  register(config?: SDKRegistrationConfig): void                // pure-SDK global path (effect NodeSdk does NOT call this)
}
// register() defaults, verified from NodeTracerProvider.js:
//   trace.setGlobalTracerProvider(this)
//   contextManager === undefined → new AsyncLocalStorageContextManager().enable() → global   (null ⇒ skip)
//   propagator     === undefined → new CompositePropagator({ propagators: [W3CTraceContextPropagator, W3CBaggagePropagator] })  (null ⇒ skip)
interface SDKRegistrationConfig { propagator?: TextMapPropagator | null; contextManager?: ContextManager | null }
```

## [03]-[SUPERSET_BARREL]

The barrel re-exports the full `sdk-trace-base` public surface — a node consumer imports the roster and the node provider from one entry. These are the SAME symbols documented in `opentelemetry-sdk-trace-base.md`; not re-catalogued here.

- classes: `BasicTracerProvider`, `BatchSpanProcessor`, `SimpleSpanProcessor`, `NoopSpanProcessor`, `ConsoleSpanExporter`, `InMemorySpanExporter`, `AlwaysOnSampler`, `AlwaysOffSampler`, `ParentBasedSampler`, `TraceIdRatioBasedSampler`, `RandomIdGenerator`; enum `SamplingDecision`.
- types: `Sampler`, `SamplingResult`, `Span`, `SpanProcessor`, `SpanExporter`, `ReadableSpan`, `TimedEvent`, `IdGenerator`, `TracerConfig`, `SpanLimits`, `GeneralLimits`, `BufferConfig`, `BatchSpanProcessorBrowserConfig`, `SDKRegistrationConfig`.

## [04]-[STACKING]

- Stack with `@effect/opentelemetry` `NodeSdk`: the primary consumer, and the reason this leg (not base) backs the node lane. `NodeSdk.layer` does `new NodeTracerProvider({ ...config.tracerConfig, resource, spanProcessors: [...config.spanProcessor] })` and drives it through the Effect runtime — it does NOT call `.register()`, because effect owns global tracer/context wiring via its `Tracer.layer`/`OtelTracerProvider` Tag. So under the telemetry rail the consumed surface is the `NodeTracerProvider` CONSTRUCTOR; the async-local-storage context manager is effect's concern, not a `register()` call.
- Stack with pure-SDK global path: a non-Effect consumer (e.g. an operator that runs OTel directly) does `const p = new NodeTracerProvider(config); p.register()` to install the global provider + `AsyncLocalStorageContextManager` + W3C composite propagator in one call. This is the path that makes `traceparent` extract-and-continue and cross-`await` parenting work without effect.
- Stack with `sdk-trace-base` roster: every processor/exporter/sampler passed to the `NodeTracerProvider` config is a base symbol reached through this barrel; `BatchSpanProcessor(new OTLPTraceExporter(opts))` + `ParentBasedSampler({ root: TraceIdRatioBasedSampler(ratio) })` is the production node trace pipeline.
- Stack with runtime split: `sdk-trace-node` (`NodeTracerProvider`) vs `sdk-trace-web` (`WebTracerProvider`) is a node/browser lane selection at the composition root, never a fork in instrumentation; the native `Otlp` lane is runtime-neutral and rides whichever `HttpClient` the runtime provides.

## [05]-[RAIL_LAW]

- Owns: the node trace provider — `NodeTracerProvider` + the `register()` global-install semantics (async-local-storage context manager + W3C composite propagator) — and the superset barrel re-exporting the full `sdk-trace-base` roster.
- Accept: `new NodeTracerProvider(tracerConfig)` reached through `@effect/opentelemetry` `NodeSdk` for the node/bun telemetry lane; `register()` only in a pure-SDK non-Effect path; base processors/exporters/samplers imported from this barrel.
- Reject: calling `.register()` under the effect facade (effect owns global context wiring — a double registration); using this leg in the browser (`sdk-trace-web` owns that); re-documenting or re-implementing the base roster (it is re-exported, catalogued in `opentelemetry-sdk-trace-base.md`); importing outside `scope:runtime`; treating the node lane as permanent — it collapses at `[R3]`.
- Boundary: `NodeTracerConfig` carries no node-specific axis (`= TracerConfig`); the node-specific behavior lives entirely in `register()`. Under effect the `resource` is the `AppIdentity`-derived `Resource` layer, not a field set here.
