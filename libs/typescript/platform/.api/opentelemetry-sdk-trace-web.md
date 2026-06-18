# [API_CATALOGUE] @opentelemetry/sdk-trace-web

`@opentelemetry/sdk-trace-web` supplies `WebTracerProvider`, `StackContextManager`, `PerformanceTimingNames`, browser-specific span utility functions, and re-exports of the full `@opentelemetry/sdk-trace-base` tracer surface for use in browser-based tracing pipelines.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/sdk-trace-web`
- package: `@opentelemetry/sdk-trace-web`
- module: `@opentelemetry/sdk-trace-web`
- asset: runtime library
- rail: tracing

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: provider and context manager family
- rail: tracing

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------- | :------------ | :------------------------------------- |
|   [1]   | `WebTracerProvider`   | class         | browser `BasicTracerProvider` subclass |
|   [2]   | `WebTracerConfig`     | type alias    | alias for `TracerConfig`               |
|   [3]   | `StackContextManager` | class         | stack-based `ContextManager` for DOM   |

[PUBLIC_TYPE_SCOPE]: performance timing types
- rail: tracing

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------ | :------------ | :------------------------------- |
|   [1]   | `PerformanceTimingNames`        | enum          | timing entry name constants      |
|   [2]   | `PerformanceEntries`            | type          | keyed timing value record        |
|   [3]   | `PerformanceLegacy`             | interface     | Safari fallback timing shape     |
|   [4]   | `PerformanceResourceTimingInfo` | interface     | main + preflight resource pair   |
|   [5]   | `PropagateTraceHeaderCorsUrls`  | type          | URL pattern for CORS header prop |
|   [6]   | `URLLike`                       | interface     | URL + anchor-element compatible  |

[PUBLIC_TYPE_SCOPE]: sdk-trace-base re-exports (classes)
- rail: tracing

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                           |
| :-----: | :------------------------- | :------------ | :------------------------------- |
|   [1]   | `AlwaysOffSampler`         | class         | sampling always reject           |
|   [2]   | `AlwaysOnSampler`          | class         | sampling always accept           |
|   [3]   | `BasicTracerProvider`      | class         | base provider implementation     |
|   [4]   | `BatchSpanProcessor`       | class         | buffered span export processor   |
|   [5]   | `ConsoleSpanExporter`      | class         | console span sink                |
|   [6]   | `InMemorySpanExporter`     | class         | in-process span sink             |
|   [7]   | `NoopSpanProcessor`        | class         | no-op processor                  |
|   [8]   | `ParentBasedSampler`       | class         | parent-context sampling delegate |
|   [9]   | `RandomIdGenerator`        | class         | random trace/span id generator   |
|  [10]   | `SamplingDecision`         | enum          | RECORD, RECORD_AND_SAMPLED, DROP |
|  [11]   | `SimpleSpanProcessor`      | class         | synchronous span export          |
|  [12]   | `TraceIdRatioBasedSampler` | class         | probabilistic sampler            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: WebTracerProvider construction and registration
- rail: tracing

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY]   | [RAIL]                                |
| :-----: | :------------------------------- | :--------------- | :------------------------------------ |
|   [1]   | `new WebTracerProvider(config?)` | provider factory | browser tracer provider construction  |
|   [2]   | `provider.register(config?)`     | SDK registration | global OTel API provider registration |

[ENTRYPOINT_SCOPE]: StackContextManager operations
- rail: tracing

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [RAIL]                         |
| :-----: | :-------------------------------------------- | :-------------- | :----------------------------- |
|   [1]   | `new StackContextManager()`                   | manager factory | stack context manager creation |
|   [2]   | `manager.enable()`                            | lifecycle       | enable context tracking        |
|   [3]   | `manager.disable()`                           | lifecycle       | disable and clear context      |
|   [4]   | `manager.active()`                            | context read    | returns current active context |
|   [5]   | `manager.bind(context, target)`               | context bind    | attach context to target       |
|   [6]   | `manager.with(context, fn, thisArg, ...args)` | context exec    | run fn under given context     |

[ENTRYPOINT_SCOPE]: span network utility functions
- rail: tracing

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [RAIL]                              |
| :-----: | :--------------------------------------------------------------- | :-------------- | :---------------------------------- |
|   [1]   | `addSpanNetworkEvent(span, perfName, entries, ignoreZeros?)`     | span mutation   | add single performance timing event |
|   [2]   | `addSpanNetworkEvents(span, resource, ignoreNet?, ignoreZeros?)` | span mutation   | add all performance timing events   |
|   [3]   | `getResource(url, startHR, endHR, resources, ignored?, type?)`   | resource lookup | match performance resource entry    |
|   [4]   | `sortResources(filteredResources)`                               | sort            | sort by startTime ascending         |
|   [5]   | `parseUrl(url)`                                                  | URL parse       | URL constructor or anchor fallback  |
|   [6]   | `normalizeUrl(url)`                                              | URL normalize   | parse and serialize to string       |
|   [7]   | `getElementXPath(target, optimised?)`                            | DOM query       | XPath string for element            |
|   [8]   | `shouldPropagateTraceHeaders(spanUrl, corsUrls?)`                | CORS check      | true when trace headers should send |
|   [9]   | `hasKey(obj, key)`                                               | type guard      | enum-keyed property existence       |

## [4]-[IMPLEMENTATION_LAW]

[WEB_TRACER_TOPOLOGY]:
- `WebTracerProvider` extends `BasicTracerProvider`; it wires `StackContextManager` by default and overrides `register` to bind the provider globally via the OTel API
- `StackContextManager` is a synchronous call-stack context manager; it does not support async context propagation across `Promise`/`setTimeout` boundaries — use `ZoneContextManager` or `AsyncLocalStorageContextManager` (from `sdk-trace-base`) for async work
- `WebTracerConfig` is a direct type alias for `TracerConfig`; no additional web-specific fields
- All `sdk-trace-base` classes (`BatchSpanProcessor`, `SimpleSpanProcessor`, `AlwaysOnSampler`, etc.) are re-exported verbatim from this package for single-import ergonomics

[LOCAL_ADMISSION]:
- Provider construction takes an optional `WebTracerConfig`; registration calls `provider.register({ contextManager, propagator, idGenerator })` to plug into global OTel API handles
- Network span enrichment via `addSpanNetworkEvents` reads `PerformanceResourceTiming` entries from the browser Performance API; pair with `getResource` to correlate fetch spans to timing entries
- `PropagateTraceHeaderCorsUrls` controls which cross-origin URLs receive `traceparent`/`tracestate` headers; configure on the propagator, not on the provider

[RAIL_LAW]:
- Package: `@opentelemetry/sdk-trace-web`
- Owns: browser `TracerProvider` implementation, stack-based context management, performance timing integration
- Accept: `WebTracerProvider` as the browser tracing root; `StackContextManager` for synchronous context
- Reject: hand-rolled browser context managers or direct `BasicTracerProvider` use without web context integration
