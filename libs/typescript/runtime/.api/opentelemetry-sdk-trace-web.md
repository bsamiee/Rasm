# [@opentelemetry/sdk-trace-web] — the browser trace SDK leg backing the WebSdk export row

`@opentelemetry/sdk-trace-web` is `sdk-trace-base` plus the browser context spine and the RUM resource-timing toolkit: `WebTracerProvider` (a `BasicTracerProvider` subclass whose `register()` installs the browser globals), `StackContextManager` (the synchronous window-scoped context manager — the browser counterpart to node's `AsyncLocalStorageContextManager`), and a `utils` module of `PerformanceResourceTiming` → span-network-event helpers keyed by the `PerformanceTimingNames` enum. That RUM toolkit is the whole reason this leg is distinct from base and from node: it turns browser `PerformanceObserver`/Resource-Timing entries into span network events. The barrel re-exports the ENTIRE `sdk-trace-base` roster (samplers, processors, exporters, id generator, every type — catalogued in `opentelemetry-sdk-trace-base.md`), so a browser consumer has one import site. `@effect/opentelemetry` `WebSdk` constructs `new WebTracerProvider({ ...tracerConfig, resource, spanProcessors })` and drives it inside the Effect runtime WITHOUT calling `.register()` — effect owns context wiring through its fiber-backed `Tracer.layer`, so `StackContextManager` is active only on the pure-SDK global path. It collapses with the pin block at `[R3]`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-web`
- package: `@opentelemetry/sdk-trace-web` · version `2.8.0` · license `Apache-2.0`
- module: dual — CJS default (`build/src/index.js`, no `"type"` field) + ESM mirror (`build/esm/index.js`, `module`); flat barrel, no `exports` subpath map; `sideEffects: false`.
- asset: TSDECL `build/src/index.d.ts` (`assay api resolve @opentelemetry/sdk-trace-web` → `2.8.0`, restored; per-concern `.d.ts` under `build/src/{WebTracerProvider,StackContextManager,utils,types,enums}`).
- peer: `@opentelemetry/api >=1.0.0 <1.10.0` — the version-pinned API contract; deps `@opentelemetry/core` (the W3C `CompositePropagator` `register()` installs), `@opentelemetry/sdk-trace-base` (the re-exported roster + `BasicTracerProvider` base).
- runtime: browser only — `StackContextManager` binds `window`/DOM and the `utils` module reads `PerformanceResourceTiming`; the node counterpart is `sdk-trace-node` (`NodeTracerProvider`, `AsyncLocalStorageContextManager`).
- plane: `plane:runtime` / `runtime:browser`, edge-ledger-fenced to `scope:telemetry` — no folder outside `telemetry` imports it.
- rail: observability/sdk-bridge; `[R3]` collapse target — only `semantic-conventions` survives the pin-block retirement.
- role: backs the `@effect/opentelemetry` `WebSdk` layer (the `telemetry` WebSdk / browser-RUM export row); supplies the browser `WebTracerProvider` effect wraps, the RUM resource-timing utils `signal/vital` composes, and re-exports the whole base trace roster.

## [02]-[WEB_PROVIDER]

The two symbols this leg adds over base for context/provider. `WebTracerConfig` is a pure alias of `TracerConfig` — no browser-specific config axis; the browser-ness is entirely in `register()`, whose `SDKRegistrationConfig` selects the global context manager and propagator. `StackContextManager` is synchronous and self-documents that it "doesn't fully support the async calls" — an `await`/microtask boundary loses the active context, so cross-`await` span parenting is best-effort in the browser (the concrete reason effect's fiber context, not this manager, is the parenting spine under the facade). Passing `null` for either `register()` field skips that global install; `undefined` takes the browser default.

| [INDEX] | [SYMBOL]                     | [KIND]                | [CAPABILITY / BOUNDARY]                                                       |
| :-----: | :--------------------------- | :-------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `WebTracerProvider`          | class                 | `extends BasicTracerProvider`; `register()` installs the browser globals      |
|  [02]   | `WebTracerConfig`            | type alias            | `= TracerConfig` — no browser-specific field                                 |
|  [03]   | `StackContextManager`        | class (`ContextManager`) | synchronous window-scoped context; `active`/`with`/`bind`/`enable`/`disable` |
|  [04]   | `SDKRegistrationConfig`      | interface (re-export) | `register()` arg — `{ propagator?, contextManager? }`                        |

```ts contract
declare class WebTracerProvider extends BasicTracerProvider {
  constructor(config?: WebTracerConfig)                          // WebTracerConfig = TracerConfig
  register(config?: SDKRegistrationConfig): void                 // pure-SDK global path (effect WebSdk does NOT call this)
}
// register() defaults, from WebTracerProvider: trace.setGlobalTracerProvider(this)
//   contextManager === undefined → new StackContextManager().enable() → global   (null ⇒ skip)
//   propagator     === undefined → W3C CompositePropagator (trace-context + baggage, from @opentelemetry/core)  (null ⇒ skip)
declare class StackContextManager implements ContextManager {
  active(): Context
  with<A extends unknown[], F extends (...a: A) => ReturnType<F>>(context: Context | null, fn: F, thisArg?: ThisParameterType<F>, ...args: A): ReturnType<F>
  bind<T>(context: Context, target: T): T                        // NB: no async continuation — lost across await/microtask
  enable(): this; disable(): this
}
interface SDKRegistrationConfig { propagator?: TextMapPropagator | null; contextManager?: ContextManager | null }
```

## [03]-[RUM_TOOLKIT]

The net-new browser capability over both base and node: one `PerformanceTimingNames` enum keys the Resource-Timing phase set, and the `utils` functions fold a `PerformanceResourceTiming` entry into span network events + content-length attributes. These are the raw material a fetch/XHR RUM span consumes; `signal/vital` reads native `PerformanceObserver` directly (zero web-vitals), so this toolkit is the resource-timing enrichment lane for any browser span that names a URL, never a second RUM source. `getResource` correlates a span URL to its Resource-Timing entry (with the CORS pre-flight companion), `shouldPropagateTraceHeaders` gates `traceparent` injection to same-origin + an allow-list, and `normalizeUrl`/`parseUrl` are the one admitted URL codec (root policy bans stringy URL parsing).

| [INDEX] | [SYMBOL]                                                                | [KIND]        | [CAPABILITY / BOUNDARY]                                              |
| :-----: | :---------------------------------------------------------------------- | :------------ | :------------------------------------------------------------------- |
|  [01]   | `PerformanceTimingNames`                                                | enum          | Resource-Timing phase keys (`FETCH_START`…`RESPONSE_END`, sizes)     |
|  [02]   | `addSpanNetworkEvent(span, name, entries, ignoreZeros?)`                | fold          | one Resource-Timing phase → a span event at its `HrTime`            |
|  [03]   | `addSpanNetworkEvents(span, resource, ignoreNetworkEvents?, …)`         | fold          | all phases + `*_body_size` content-length attrs onto a span         |
|  [04]   | `getResource(spanUrl, startHR, endHR, resources, ignored?, initiator?)` | correlator    | span URL → `PerformanceResourceTimingInfo` (main + CORS pre-flight) |
|  [05]   | `sortResources(entries)` / `getElementXPath(target, optimised?)`        | util          | Resource-Timing ordering; DOM-event span target XPath               |
|  [06]   | `parseUrl(url): URLLike` / `normalizeUrl(url): string`                  | URL codec     | WHATWG URL parse/serialize — the one admitted URL decoder           |
|  [07]   | `shouldPropagateTraceHeaders(spanUrl, corsUrls?)` / `hasKey(obj, key)` | guard         | same-origin + allow-list `traceparent` injection gate               |

```ts contract
declare enum PerformanceTimingNames { FETCH_START, DOMAIN_LOOKUP_START, CONNECT_START, REQUEST_START, RESPONSE_START, RESPONSE_END, /* + DOM_* + REDIRECT_* + *_body_size */ }
type PropagateTraceHeaderCorsUrls = (string | RegExp) | (string | RegExp)[]
declare function addSpanNetworkEvents(span: api.Span, resource: PerformanceEntries, ignoreNetworkEvents?: boolean, ignoreZeros?: boolean, skipOldSemconvContentLengthAttrs?: boolean): void
declare function getResource(spanUrl: string, startTimeHR: api.HrTime, endTimeHR: api.HrTime, resources: PerformanceResourceTiming[], ignoredResources?: WeakSet<PerformanceResourceTiming>, initiatorType?: string): PerformanceResourceTimingInfo
declare function shouldPropagateTraceHeaders(spanUrl: string, propagateTraceHeaderCorsUrls?: PropagateTraceHeaderCorsUrls): boolean
interface URLLike { hash: string; host: string; hostname: string; href: string; readonly origin: string; pathname: string; port: string; protocol: string; search: string /* + username/password */ }
// types: PerformanceEntries (PerformanceTimingNames-keyed), PerformanceLegacy (Safari timing fallback), PerformanceResourceTimingInfo { mainRequest?, corsPreFlightRequest? }
```

## [04]-[SUPERSET_BARREL]

The barrel re-exports the full `sdk-trace-base` public surface — a browser consumer imports the roster and the web provider from one entry. These are the SAME symbols documented in `opentelemetry-sdk-trace-base.md`; not re-catalogued here. The browser-relevant divergence: `BatchSpanProcessor` accepts `BatchSpanProcessorBrowserConfig extends BufferConfig { disableAutoFlushOnDocumentHide? }` — auto-flush on `pagehide`/`visibilitychange` is ON by default, so RUM spans drain before navigation/tab-close (the concrete reason browser batch tuning differs from node's `BufferConfig`).

- classes: `BasicTracerProvider`, `BatchSpanProcessor`, `SimpleSpanProcessor`, `NoopSpanProcessor`, `ConsoleSpanExporter`, `InMemorySpanExporter`, `AlwaysOnSampler`, `AlwaysOffSampler`, `ParentBasedSampler`, `TraceIdRatioBasedSampler`, `RandomIdGenerator` (browser `Math.random`); enum `SamplingDecision`.
- types: `Sampler`, `SamplingResult`, `Span`, `SpanProcessor`, `SpanExporter`, `ReadableSpan`, `TimedEvent`, `IdGenerator`, `TracerConfig`, `SpanLimits`, `GeneralLimits`, `BufferConfig`, `BatchSpanProcessorBrowserConfig`, `SDKRegistrationConfig`.

## [05]-[STACKING]

- [STACK: `.api/effect-opentelemetry.md` `WebSdk`] — the primary consumer, and the reason this leg (not base) backs the browser RUM lane. `WebSdk.layer` does `new WebTracerProvider({ ...config.tracerConfig, resource, spanProcessors: [...config.spanProcessor] })` and drives it through the Effect runtime — it does NOT call `.register()`, because effect owns global tracer/context wiring via its fiber-backed `Tracer.layer`/`OtelTracerProvider` Tag. So under the telemetry rail the consumed surface is the `WebTracerProvider` CONSTRUCTOR; `StackContextManager` is bypassed (its sync-only limitation is exactly why effect's fiber context is the parenting spine).
- [STACK: `.api/effect-platform-browser.md` `BrowserHttpClient`] — the browser export transport. The native `Otlp.layer` requires `BrowserHttpClient.layerXMLHttpRequest` (XHR — the only browser client exposing upload/download progress + arraybuffer for protobuf frames); `WebSdk` + the browser `OTLPTraceExporter` is the SDK-bridge alternative when SDK processor semantics are required. Browser RUM egress rides the XHR client either way.
- [STACK: `sdk-trace-base` roster] — every processor/exporter/sampler passed to the `WebTracerProvider` config is a base symbol reached through this barrel; `new BatchSpanProcessor(new OTLPTraceExporter(cfg), { disableAutoFlushOnDocumentHide: false })` + `ParentBasedSampler({ root: TraceIdRatioBasedSampler(ratio) })` is the production browser trace pipeline.
- [STACK: `signal/vital` + the RUM toolkit] — `signal/vital` reads native `PerformanceObserver` for the vital budgets (zero web-vitals); when a browser span names a URL (fetch/XHR/navigation), `addSpanNetworkEvents` + `getResource` fold its Resource-Timing entry into span network events, and `shouldPropagateTraceHeaders` gates `traceparent` injection at the CORS boundary. The convention attribute keys stamped on those spans are `semantic-conventions` rows (`browser.*` incubating + `http.*`/`url.*` stable), never string literals.
- [STACK: runtime split] — `sdk-trace-web` (`WebTracerProvider`) vs `sdk-trace-node` (`NodeTracerProvider`) is a browser/node lane selection at the composition root, never a fork in instrumentation; the native `Otlp` lane is runtime-neutral and rides whichever `HttpClient` the runtime provides. `browser/boot` composes the `WebSdk`/native lane once at startup.

## [06]-[RAIL_LAW]

- Owns: the browser trace provider — `WebTracerProvider` + the `register()` global-install semantics (`StackContextManager` + W3C composite propagator) — the synchronous `StackContextManager`, the `PerformanceTimingNames` enum + RUM resource-timing toolkit, and the superset barrel re-exporting the full `sdk-trace-base` roster.
- Accept: `new WebTracerProvider(tracerConfig)` reached through `@effect/opentelemetry` `WebSdk` for the browser RUM lane; `register()` only in a pure-SDK non-Effect path; base processors/exporters/samplers imported from this barrel; the RUM `utils` for span network-event enrichment of URL-bearing browser spans; `BatchSpanProcessorBrowserConfig` pagehide-flush for RUM drain-before-navigation.
- Reject: calling `.register()` under the effect facade (effect owns global context wiring — a double registration); relying on `StackContextManager` for cross-`await` span parenting (it is sync-only; effect's fiber context is the spine); using this leg in node (`sdk-trace-node` owns that); re-documenting or re-implementing the base roster (it is re-exported, catalogued in `opentelemetry-sdk-trace-base.md`); a hand-rolled URL parser where `parseUrl`/`normalizeUrl` exist; importing outside `scope:telemetry`; treating the browser lane as permanent — it collapses at `[R3]`.
- Boundary: `WebTracerConfig` carries no browser-specific axis (`= TracerConfig`); the browser-specific behavior lives in `register()` + `StackContextManager` + the RUM toolkit. Under effect the `resource` is the `AppIdentity`-derived `Resource` layer, not a field set here; `signal/vital`'s primary RUM source is native `PerformanceObserver`, this toolkit the resource-timing enrichment lane.
