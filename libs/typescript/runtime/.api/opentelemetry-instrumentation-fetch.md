# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_FETCH]

`@opentelemetry/instrumentation-fetch` patches the browser-global `fetch`, opening one client span per request with W3C trace headers injected on same-origin and CORS-allow-listed calls. `Vital.enrich` projects Performance-Timeline resource timings onto those spans, so the span and its network breakdown compose from two owners. Global patching makes it composition-root material — it registers in the `web` SDK configuration, never a library.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-fetch`
- package: `@opentelemetry/instrumentation-fetch` (Apache-2.0)
- base: extends `@opentelemetry/instrumentation` `InstrumentationBase`; span shapes from `@opentelemetry/sdk-trace-web`
- runtime: browser only — patches `globalThis.fetch`
- rail: observability/rum

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation, config, and the two hook shapes

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `FetchInstrumentation`         | instrumentation | one row in the root's instrumentation set           |
|  [02]   | `FetchInstrumentationConfig`   | config          | the whole policy surface at construction            |
|  [03]   | `FetchCustomAttributeFunction` | hook shape      | `(span, request, result) => void` post-settle stamp |
|  [04]   | `FetchRequestHookFunction`     | hook shape      | `(span, request) => void` pre-dispatch stamp        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction policy — one `FetchInstrumentationConfig` object is the whole knob surface

| [INDEX] | [SURFACE]                                     | [SHAPE]      | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------- | :----------- | :------------------------------------------------- |
|  [01]   | `new FetchInstrumentation(config?)`           | ctor         | one construction at the browser root               |
|  [02]   | `propagateTraceHeaderCorsUrls`                | config field | CORS allow-list carrying `traceparent` injection   |
|  [03]   | `ignoreUrls`                                  | config field | telemetry-egress self-exclusion (collector origin) |
|  [04]   | `applyCustomAttributesOnSpan` / `requestHook` | config field | bounded attribute stamps, identifier-grade only    |
|  [05]   | `ignoreNetworkEvents` / `measureRequestSize`  | config field | timing-event and request-size toggles              |
|  [06]   | `clearTimingResources`                        | config field | resource-timing buffer hygiene after projection    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — patching `globalThis.fetch` at library altitude double-instruments the host.
- self-exclusion is mandatory — the collector origin rides `ignoreUrls`, else every export batch mints its own span.

[STACKING]:
- `otel/emit` `web` row: registers inside the same `web` SDK configuration, so fetch-span URLs pass its export-boundary redaction processor.
- `otel/vital` `Vital.enrich(span, request)`: selects the matching `PerformanceResourceTiming` and projects its network events onto this row's span — the row owns the span, vital owns the timing.
- `opentelemetry-context-zone.md` `ZoneContextManager`: parents the fetch span across the promise chain the stack context manager drops.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-fetch`
- Owns: browser fetch client spans and their `traceparent` injection
- Accept: one root construction with `ignoreUrls` covering telemetry egress and `propagateTraceHeaderCorsUrls` naming the API origins
- Reject: library-altitude registration, unbounded custom attributes, propagation outside the allow-list
