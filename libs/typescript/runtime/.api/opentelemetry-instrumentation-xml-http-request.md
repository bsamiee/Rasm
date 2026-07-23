# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_XML_HTTP_REQUEST]

`@opentelemetry/instrumentation-xml-http-request` patches the global `XMLHttpRequest`, opening one client span per request with W3C trace headers injected on same-origin and CORS-allow-listed calls. It is the legacy-XHR request-span producer beside the fetch row — `XMLHttpRequestInstrumentation` extends `InstrumentationBase`, folds Performance-Timeline resource timings onto its spans, and opens a CORS pre-flight child span. Global patching makes it composition-root material — it registers in the `web` SDK set beside the fetch, document-load, and user-interaction rows, never a library.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-xml-http-request`
- package: `@opentelemetry/instrumentation-xml-http-request` (Apache-2.0)
- base: `XMLHttpRequestInstrumentation` extends `@opentelemetry/instrumentation` `InstrumentationBase`; span shapes from `@opentelemetry/sdk-trace-web`
- consumed-by: the browser composition root's `web` SDK instrumentation set beside the fetch, document-load, and user-interaction rows
- runtime: browser only — patches `globalThis.XMLHttpRequest`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation + config + hook shape
- rail: observability/rum
- One `XMLHttpRequestInstrumentationConfig` object is the whole policy surface; `XHRCustomAttributeFunction` is the post-settle attribute hook, distinct from the fetch row's request-time hook.

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                  |
| :-----: | :------------------------------------ | :-------------- | :--------------------------------------------------- |
|  [01]   | `XMLHttpRequestInstrumentation`       | instrumentation | one row in the root's registered instrumentation set |
|  [02]   | `XMLHttpRequestInstrumentationConfig` | config          | the whole policy surface, one object at construction |
|  [03]   | `XHRCustomAttributeFunction`          | hook shape      | `(span, xhr) => void` post-settle attribute stamp    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction policy
- rail: observability/rum
- One config object is the entire knob surface; every row below is a verified `XMLHttpRequestInstrumentationConfig` field, and `enable()`/`disable()` bracket the active patch.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                    |
| :-----: | :------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `new XMLHttpRequestInstrumentation(config?)` | ctor           | one construction at the browser root                   |
|  [02]   | `propagateTraceHeaderCorsUrls`               | config field   | the CORS allow-list `traceparent` injection rides      |
|  [03]   | `ignoreUrls`                                 | config field   | telemetry-egress self-exclusion (the collector origin) |
|  [04]   | `applyCustomAttributesOnSpan`                | config field   | bounded span attribute stamp; identifier-grade only    |
|  [05]   | `ignoreNetworkEvents` / `measureRequestSize` | config field   | network-event and request-size toggles                 |
|  [06]   | `clearTimingResources`                       | config field   | resource-timing buffer hygiene after projection        |
|  [07]   | `enable()` / `disable()`                     | lifecycle      | patch/unpatch the global `XMLHttpRequest`              |

## [04]-[IMPLEMENTATION_LAW]

[RUM_TOPOLOGY]:
- composition-root only — the row patches `globalThis.XMLHttpRequest`; a library registration double-instruments the host.
- self-exclusion is mandatory policy — the collector origin rides `ignoreUrls`, the same law as the fetch row, otherwise every export batch mints its own span and the trace feed feeds itself.

[INTEGRATION_LAW]:
- Stack with `otel/emit` `web` row: registers inside the same `WebSdk` instrumentation set beside the fetch, document-load, and user-interaction rows, so the shared redaction processor scrubs span attributes at the export boundary.
- Stack with `opentelemetry-context-zone.md`: the zone manager parents the XHR span across the async callback chain; absent it the stack context manager drops the parent at the first timer.
- Stack with `opentelemetry-instrumentation-fetch.md`: the two rows split the request surface — this row owns legacy `XMLHttpRequest`, the fetch row owns `fetch`; `ignoreUrls` on both excludes the collector origin.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-xml-http-request`
- Owns: browser XHR client spans, their `traceparent` injection, and the CORS pre-flight child span
- Accept: one construction at the root with `ignoreUrls` covering telemetry egress and `propagateTraceHeaderCorsUrls` naming the API origins
- Reject: library-altitude registration, unbounded custom attributes, propagation to origins outside the allow-list
