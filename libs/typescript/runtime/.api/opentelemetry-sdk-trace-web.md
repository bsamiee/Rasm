# [TS_RUNTIME_API_OPENTELEMETRY_SDK_TRACE_WEB]

`@opentelemetry/sdk-trace-web` mints the browser trace provider: `WebTracerProvider.register()` installs the `StackContextManager` and a W3C composite propagator as OpenTelemetry globals over the re-exported `sdk-trace-base` pipeline.

Its `PerformanceTimingNames` enum keys Resource-Timing phases and `utils` folds a `PerformanceResourceTiming` entry into span network events, the lane every URL-bearing browser span rides; `@effect/opentelemetry` `WebSdk` drives the constructor over `register()`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-web`
- package: `@opentelemetry/sdk-trace-web` (Apache-2.0)
- module: CJS default (`main`) + ESM mirror (`module`); flat barrel, no `exports` subpath map; `sideEffects: false`
- runtime: browser only — `StackContextManager` binds `window`/DOM and `utils` reads `PerformanceResourceTiming`; `sdk-trace-node` owns the node counterpart
- depends: `@opentelemetry/api` (`Context`/`ContextManager`/`Span`/`HrTime`), `@opentelemetry/core` (composite W3C propagator `register()` installs), `@opentelemetry/sdk-trace-base` (`BasicTracerProvider` base + re-exported roster)
- rail: observability/sdk-bridge — the browser trace provider `@effect/opentelemetry` `WebSdk` drives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the browser provider, its window-scoped context manager, and the RUM resource-timing vocabulary

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :------------------------------ | :------------ | :------------------------------------------------------------------- |
|  [01]   | `WebTracerProvider`             | class         | `extends BasicTracerProvider`; `register()` installs browser globals |
|  [02]   | `WebTracerConfig`               | alias         | `= TracerConfig`; browser-ness rides `register()`, not a config axis |
|  [03]   | `StackContextManager`           | class         | synchronous `window`-scoped api `ContextManager`                     |
|  [04]   | `PerformanceTimingNames`        | enum          | Resource-Timing phase keys plus `*BodySize` byte keys                |
|  [05]   | `PerformanceEntries`            | type          | phase-keyed timing map `addSpanNetworkEvents` folds                  |
|  [06]   | `PerformanceResourceTimingInfo` | interface     | `getResource` return — `mainRequest` + CORS pre-flight companion     |
|  [07]   | `URLLike`                       | interface     | WHATWG-URL/anchor field bag `parseUrl` returns                       |
|  [08]   | `PropagateTraceHeaderCorsUrls`  | union         | `(string\|RegExp)[]` allow-list gating `traceparent` injection       |

- `WebTracerConfig`: `register()` alone carries the browser behavior, so the base `TracerConfig` axes (`sampler`/`spanProcessors`/`idGenerator`) reach the browser provider unchanged.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: provider registration and the window-scoped context manager

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `WebTracerProvider(WebTracerConfig?)`                | ctor     | the browser provider `WebSdk` wraps               |
|  [02]   | `WebTracerProvider.register(SDKRegistrationConfig?)` | instance | install the api tracer/propagator/context globals |
|  [03]   | `StackContextManager.active() -> Context`            | instance | the current window-scoped active context          |
|  [04]   | `StackContextManager.with(Context\|null, Fn) -> Ret` | instance | run `fn` under a context; `null` binds `window`   |
|  [05]   | `StackContextManager.bind(Context, T) -> T`          | instance | bind a context to a function or emitter target    |
|  [06]   | `StackContextManager.enable/disable() -> this`       | instance | create / clear the root context                   |

- `WebTracerProvider.register`: a `null` field skips that global install; `undefined` installs the browser default.
- `StackContextManager.with`: forwards `thisArg` and trailing `...args` to `fn`; synchronous — an `await`/microtask boundary drops the active context.

[ENTRYPOINT_SCOPE]: RUM resource-timing toolkit — a `PerformanceResourceTiming` entry folded onto a URL-bearing span

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `addSpanNetworkEvent(Span, string, PerformanceEntries, boolean?)`    | fold    | one Resource-Timing phase → a span event           |
|  [02]   | `addSpanNetworkEvents(Span, PerformanceEntries, boolean?, ...)`      | fold    | every phase + `*BodySize` content-length attrs     |
|  [03]   | `getResource(string, HrTime, HrTime, PerformanceResourceTiming[])`   | static  | span URL → `PerformanceResourceTimingInfo`         |
|  [04]   | `sortResources(PerformanceResourceTiming[]) -> ...[]`                | static  | Resource-Timing ordering by `startTime`            |
|  [05]   | `parseUrl(string) -> URLLike` / `normalizeUrl(string) -> string`     | static  | the one admitted WHATWG URL parse/serialize codec  |
|  [06]   | `shouldPropagateTraceHeaders(string, PropagateTraceHeaderCorsUrls?)` | static  | same-origin + allow-list `traceparent` gate        |
|  [07]   | `getElementXPath(target, boolean?) -> string`                        | static  | DOM-event span target XPath; `optimised` uses id   |
|  [08]   | `hasKey(object, PropertyKey) -> boolean`                             | static  | typed-key guard for `PerformanceEntries` iteration |

- `addSpanNetworkEvent`: returns the `Span` or `undefined` when the named phase is absent or zeroed under `ignoreZeros`.
- `getResource`: optional `ignoredResources` `WeakSet` skips reused entries and `initiatorType` filters by kind; the return pairs `mainRequest` with its CORS pre-flight.
- `addSpanNetworkEvents`: trailing `ignoreNetworkEvents`, `ignoreZeros`, `skipOldSemconvContentLengthAttrs` flags gate emission and legacy content-length keys.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `WebTracerProvider` extends `BasicTracerProvider`, and `register()` installs the api globals — the tracer provider, a `CompositePropagator` of W3C trace-context and baggage, and `StackContextManager` — each field opting out with `null` or defaulting with `undefined`.
- `StackContextManager` parents spans within one call stack and drops the active context across an `await`; effect's fiber context, not this manager, is the async-safe parenting spine under the facade.
- `utils` folds a `PerformanceResourceTiming` entry into span network events keyed by `PerformanceTimingNames`, so a URL-bearing browser span carries its resource-timing phases as timed events from this one enrichment lane.

[STACKING]:
- `opentelemetry-sdk-trace-base.md` `BasicTracerProvider`/`SpanProcessor`/`Sampler`: the barrel re-exports the whole trace roster, so `new WebTracerProvider({ spanProcessors: [new BatchSpanProcessor(exporter)], sampler: new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio) }) })` reads every processor and sampler from this import, and base-owned `BatchSpanProcessorBrowserConfig.disableAutoFlushOnDocumentHide` drains RUM spans before navigation.
- `effect-opentelemetry.md` `WebSdk.layerTracerProvider`: the primary consumer wraps `WebTracerProvider` and exposes the `Tracer.OtelTracerProvider` Tag, driving the provider through the Effect runtime rather than `register()` — the consumed surface is the constructor and `StackContextManager` stays bypassed.
- `opentelemetry-context-zone.md` `ZoneContextManager`: satisfies `SDKRegistrationConfig.contextManager`, replacing the sync-only `StackContextManager` on a pure-SDK `register()` path so async hops keep span parenting.
- `opentelemetry-instrumentation.md` `registerInstrumentations`: `otel/instrument.ts` binds the browser rows to the web lane's `OtelTracerProvider` Tag through the `tracerProvider` option, each fetch/XHR row mirroring this surface's CORS gate with its own `propagateTraceHeaderCorsUrls`, and the URL-bearing spans consume `addSpanNetworkEvents`/`getResource` for resource-timing enrichment.
- `otel/instrument.ts` + `browser/boot` (within-lib): boot composes the `WebSdk` lane once at startup and hands the exposed provider to `registerInstrumentations`; the RUM toolkit enriches any span naming a URL, and the stamped attribute keys resolve to `semantic-conventions` rows, never string literals.

[LOCAL_ADMISSION]:
- `@opentelemetry/*` admits only inside `scope:runtime` (edge-ledger ban); no folder outside `telemetry` imports `sdk-trace-web`, and instrumentation emits through Effect's native signals against the facade-driven provider.
- design code reaches the `WebSdk` layer for the browser provider and the RUM `utils` for URL-bearing span enrichment; `register()` serves a pure-SDK non-Effect path alone.
- native `Otlp` export stays the standing rail; `.api/effect-opentelemetry.md` owns the `[OTEL_PIN_BLOCK]` collapse roster this browser SDK leg joins.

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-trace-web`
- Owns: the browser trace provider `WebTracerProvider` with its `register()` global-install semantics (`StackContextManager` and the W3C composite propagator), the synchronous `StackContextManager`, and the `PerformanceTimingNames` enum and `utils` resource-timing enrichment toolkit; the barrel re-exports the `sdk-trace-base` roster catalogued in `opentelemetry-sdk-trace-base.md`.
- Accept: `new WebTracerProvider(tracerConfig)` reached through `@effect/opentelemetry` `WebSdk`; base processors and samplers imported from this barrel; `BatchSpanProcessorBrowserConfig` pagehide-flush for RUM drain-before-navigation; the `utils` fold enriching any URL-bearing browser span; `register()` on a pure-SDK path.
- Reject: `.register()` under the effect facade (it owns global context wiring — a double registration); `StackContextManager` for cross-`await` parenting (sync-only — effect's fiber context is the spine); this leg in node (`sdk-trace-node` owns it); a hand-rolled URL parser where `parseUrl`/`normalizeUrl` exist; re-cataloguing the re-exported base roster; importing outside `scope:runtime`.
