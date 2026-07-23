# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_FETCH]

`@opentelemetry/instrumentation-fetch` patches the global `fetch` in a browser document and opens one client span per request with W3C trace headers injected on same-origin (and CORS-allow-listed) calls. It is the RUM request-span producer: `Vital.enrich` projects Performance-Timeline resource timings onto the spans this row opens, so the fetch span and its network breakdown come from two owners composing at the span. Global patching makes it app-composition-root material — it registers inside the `web` SDK row's configuration, never in a library.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-fetch`
- package: `@opentelemetry/instrumentation-fetch` (Apache-2.0)
- base: extends `@opentelemetry/instrumentation` `InstrumentationBase`; span shapes from `@opentelemetry/sdk-trace-web`
- consumed-by: the browser composition root beside the `web` export row; `Vital.enrich` composes onto its spans
- runtime: browser only — patches `globalThis.fetch`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation + config
- rail: observability/rum

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                  |
| :-----: | :----------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `FetchInstrumentation`         | instrumentation | one row in the root's registered instrumentation set |
|  [02]   | `FetchInstrumentationConfig`   | config          | the whole policy surface, one object at construction |
|  [03]   | `FetchCustomAttributeFunction` | hook shape      | `(span, request, result) => void` post-settle stamp  |
|  [04]   | `FetchRequestHookFunction`     | hook shape      | `(span, request) => void` pre-dispatch stamp         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction policy
- rail: observability/rum
- One config object is the entire knob surface; every row below is a verified `FetchInstrumentationConfig` field.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                     |
| :-----: | :-------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `new FetchInstrumentation(config?)`           | ctor           | one construction at the browser root                    |
|  [02]   | `propagateTraceHeaderCorsUrls`                | config field   | the CORS allow-list `traceparent` injection rides       |
|  [03]   | `ignoreUrls`                                  | config field   | telemetry-egress self-exclusion (the collector origin)  |
|  [04]   | `applyCustomAttributesOnSpan` / `requestHook` | config field   | bounded attribute stamps; identifier-grade context only |
|  [05]   | `ignoreNetworkEvents` / `measureRequestSize`  | config field   | timing-event and request-size toggles                   |
|  [06]   | `clearTimingResources`                        | config field   | resource-timing buffer hygiene after projection         |

## [04]-[IMPLEMENTATION_LAW]

[RUM_TOPOLOGY]:
- composition-root only — the row patches `globalThis.fetch`; a library registration double-instruments the host.
- self-exclusion is mandatory policy — the collector origin rides `ignoreUrls`, otherwise every export batch mints its own span and the trace feed feeds itself.

[INTEGRATION_LAW]:
- Stack with `otel/emit` `web` row: registers inside the same SDK configuration whose redaction processor scrubs span attributes at the export boundary, so fetch-span URLs pass the shared scrub.
- Stack with `otel/vital`: `Vital.enrich(span, request)` selects the matching `PerformanceResourceTiming` and projects network events onto this row's span — the row owns the span, vital owns the timing projection.
- Stack with `opentelemetry-context-zone.md`: the zone manager keeps the fetch span parented across the promise chain.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-fetch`
- Owns: browser fetch client spans and their `traceparent` injection
- Accept: one construction at the root with `ignoreUrls` covering telemetry egress and `propagateTraceHeaderCorsUrls` naming the API origins
- Reject: library-altitude registration, unbounded custom attributes, propagation to origins outside the allow-list
