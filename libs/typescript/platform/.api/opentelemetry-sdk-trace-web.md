# [API_CATALOGUE] @opentelemetry/sdk-trace-web

`@opentelemetry/sdk-trace-web` supplies `WebTracerProvider`, `StackContextManager`, `PerformanceTimingNames`, browser-specific span utility functions, and re-exports of the full `@opentelemetry/sdk-trace-base` tracer surface for use in browser-based tracing pipelines.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-web`
- package: `@opentelemetry/sdk-trace-web`
- module: `@opentelemetry/sdk-trace-web`
- asset: runtime library
- rail: tracing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and context manager family
- rail: tracing

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------- | :------------ | :------------------------------------- |
|  [01]   | `WebTracerProvider`   | class         | browser `BasicTracerProvider` subclass |
|  [02]   | `WebTracerConfig`     | type alias    | alias for `TracerConfig`               |
|  [03]   | `StackContextManager` | class         | stack-based `ContextManager` for DOM   |

[PUBLIC_TYPE_SCOPE]: performance timing types
- rail: tracing

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------ | :------------ | :------------------------------- |
|  [01]   | `PerformanceTimingNames`        | enum          | timing entry name constants      |
|  [02]   | `PerformanceEntries`            | type          | keyed timing value record        |
|  [03]   | `PerformanceLegacy`             | interface     | Safari fallback timing shape     |
|  [04]   | `PerformanceResourceTimingInfo` | interface     | main + preflight resource pair   |
|  [05]   | `PropagateTraceHeaderCorsUrls`  | type          | URL pattern for CORS header prop |
|  [06]   | `URLLike`                       | interface     | URL + anchor-element compatible  |

[PUBLIC_TYPE_SCOPE]: sdk-trace-base re-exports (classes)
- rail: tracing

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------- | :------------ | :------------------------------- |
|  [01]   | `AlwaysOffSampler`         | class         | sampling always reject           |
|  [02]   | `AlwaysOnSampler`          | class         | sampling always accept           |
|  [03]   | `BasicTracerProvider`      | class         | base provider implementation     |
|  [04]   | `BatchSpanProcessor`       | class         | buffered span export processor   |
|  [05]   | `ConsoleSpanExporter`      | class         | console span sink                |
|  [06]   | `InMemorySpanExporter`     | class         | in-process span sink             |
|  [07]   | `NoopSpanProcessor`        | class         | no-op processor                  |
|  [08]   | `ParentBasedSampler`       | class         | parent-context sampling delegate |
|  [09]   | `RandomIdGenerator`        | class         | random trace/span id generator   |
|  [10]   | `SamplingDecision`         | enum          | RECORD, RECORD_AND_SAMPLED, DROP |
|  [11]   | `SimpleSpanProcessor`      | class         | synchronous span export          |
|  [12]   | `TraceIdRatioBasedSampler` | class         | probabilistic sampler            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: WebTracerProvider construction and registration
- rail: tracing

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]   | [RAIL]                                |
| :-----: | :------------------------------- | :--------------- | :------------------------------------ |
|  [01]   | `new WebTracerProvider(config?)` | provider factory | browser tracer provider construction  |
|  [02]   | `provider.register(config?)`     | SDK registration | global OTel API provider registration |

[ENTRYPOINT_SCOPE]: StackContextManager operations
- rail: tracing

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [RAIL]                         |
| :-----: | :-------------------------------------------- | :-------------- | :----------------------------- |
|  [01]   | `new StackContextManager()`                   | manager factory | stack context manager creation |
|  [02]   | `manager.enable()`                            | lifecycle       | enable context tracking        |
|  [03]   | `manager.disable()`                           | lifecycle       | disable and clear context      |
|  [04]   | `manager.active()`                            | context read    | returns current active context |
|  [05]   | `manager.bind(context, target)`               | context bind    | attach context to target       |
|  [06]   | `manager.with(context, fn, thisArg, ...args)` | context exec    | run fn under given context     |

[ENTRYPOINT_SCOPE]: span network utility functions
- rail: tracing

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :--------------------------------------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `addSpanNetworkEvent(span, perfName, entries, ignoreZeros?)`     | span mutation   | add single performance timing event |
|  [02]   | `addSpanNetworkEvents(span, resource, ignoreNet?, ignoreZeros?)` | span mutation   | add all performance timing events   |
|  [03]   | `getResource(url, startHR, endHR, resources, ignored?, type?)`   | resource lookup | match performance resource entry    |
|  [04]   | `sortResources(filteredResources)`                               | sort            | sort by startTime ascending         |
|  [05]   | `parseUrl(url)`                                                  | URL parse       | URL constructor or anchor fallback  |
|  [06]   | `normalizeUrl(url)`                                              | URL normalize   | parse and serialize to string       |
|  [07]   | `getElementXPath(target, optimised?)`                            | DOM query       | XPath string for element            |
|  [08]   | `shouldPropagateTraceHeaders(spanUrl, corsUrls?)`                | CORS check      | true when trace headers should send |
|  [09]   | `hasKey(obj, key)`                                               | type guard      | enum-keyed property existence       |

## [04]-[IMPLEMENTATION_LAW]

[WEB_TRACER_TOPOLOGY]:
- `WebTracerProvider` extends `BasicTracerProvider`; it wires `StackContextManager` by default and overrides `register` to bind the provider globally via the OTel API
- `StackContextManager` is a synchronous call-stack context manager; it does not support async context propagation across `Promise`/`setTimeout` boundaries — use `ZoneContextManager` or `AsyncLocalStorageContextManager` (from `sdk-trace-base`) for async work
- `WebTracerConfig` is a direct type alias for `TracerConfig`; no additional web-specific fields
- All `sdk-trace-base` classes (`BatchSpanProcessor`, `SimpleSpanProcessor`, `AlwaysOnSampler`, etc.) are re-exported verbatim from this package for single-import ergonomics

[LOCAL_ADMISSION]:
- Provider construction takes an optional `WebTracerConfig`; registration calls `provider.register({ contextManager, propagator, idGenerator })` to plug into global OTel API handles
- `register({ propagator })` is reachable ONLY on a SELF-CONSTRUCTED `new WebTracerProvider(...)` — the `@effect/opentelemetry` `WebSdk.layer` builds its `WebTracerProvider` internally and yields a `Resource.Resource` layer with no exposed `.register` handle (its `Configuration` carries no `propagator` field), so the W3C-propagator registration is a standalone `new WebTracerProvider().register({ propagator })` composed alongside the WebSdk export layer (`Observability/telemetry#TRACE_PROPAGATION`), never a call on the WebSdk-built provider
- Network span enrichment via `addSpanNetworkEvents` reads `PerformanceResourceTiming` entries from the browser Performance API; pair with `getResource` to correlate fetch spans to timing entries
- `PropagateTraceHeaderCorsUrls` controls which cross-origin URLs receive `traceparent`/`tracestate` headers; configure on the propagator, not on the provider

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-trace-web`
- Owns: browser `TracerProvider` implementation, stack-based context management, performance timing integration
- Accept: `WebTracerProvider` as the browser tracing root; `StackContextManager` for synchronous context
- Reject: hand-rolled browser context managers or direct `BasicTracerProvider` use without web context integration
