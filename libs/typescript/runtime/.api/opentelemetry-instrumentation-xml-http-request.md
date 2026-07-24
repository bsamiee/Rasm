# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_XML_HTTP_REQUEST]

`@opentelemetry/instrumentation-xml-http-request` patches `globalThis.XMLHttpRequest` and opens one client span per request with W3C `traceparent` injected on same-origin and CORS-allow-listed calls. `XMLHttpRequestInstrumentation` folds Performance-Timeline resource timings onto each span and opens a CORS pre-flight child span. Global patching seats it in the browser `web` SDK set, never a library — the XHR request-span producer beside the fetch row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-xml-http-request`
- package: `@opentelemetry/instrumentation-xml-http-request` (Apache-2.0)
- base: extends `@opentelemetry/instrumentation` `InstrumentationBase`; span shapes from `@opentelemetry/sdk-trace-web`
- consumed-by: the browser composition root beside the `web` export row
- runtime: browser only — patches `globalThis.XMLHttpRequest`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation, config, and the post-settle hook shape

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]   | [CAPABILITY]                                      |
| :-----: | :------------------------------------ | :-------------- | :------------------------------------------------ |
|  [01]   | `XMLHttpRequestInstrumentation`       | instrumentation | one row in the root's instrumentation set         |
|  [02]   | `XMLHttpRequestInstrumentationConfig` | config          | the whole policy surface at construction          |
|  [03]   | `XHRCustomAttributeFunction`          | hook shape      | `(span, xhr) => void` post-settle attribute stamp |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction policy — one `XMLHttpRequestInstrumentationConfig` object is the whole knob surface

| [INDEX] | [SURFACE]                                    | [SHAPE]      | [CAPABILITY]                                        |
| :-----: | :------------------------------------------- | :----------- | :-------------------------------------------------- |
|  [01]   | `new XMLHttpRequestInstrumentation(config?)` | ctor         | one construction at the browser root                |
|  [02]   | `propagateTraceHeaderCorsUrls`               | config field | CORS allow-list carrying `traceparent` injection    |
|  [03]   | `ignoreUrls`                                 | config field | telemetry-egress self-exclusion (collector origin)  |
|  [04]   | `applyCustomAttributesOnSpan`                | config field | bounded span attribute stamp, identifier-grade only |
|  [05]   | `ignoreNetworkEvents` / `measureRequestSize` | config field | network-event and request-size toggles              |
|  [06]   | `clearTimingResources`                       | config field | resource-timing buffer hygiene after projection     |
|  [07]   | `enable()` / `disable()`                     | lifecycle    | patch/unpatch the global `XMLHttpRequest`           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — patching `globalThis.XMLHttpRequest` at library altitude double-instruments the host.
- self-exclusion is mandatory — the collector origin rides `ignoreUrls`, else every export batch mints its own span and the feed traces itself.

[STACKING]:
- `otel/emit` `web` row: registers inside the `web` SDK configuration, so XHR-span attributes pass its export-boundary redaction processor.
- `opentelemetry-context-zone.md` `ZoneContextManager`: parents the XHR span across the async callback chain the stack context manager drops at the first timer.
- `opentelemetry-instrumentation-fetch.md` `FetchInstrumentation`: peer request-surface split — this row owns `XMLHttpRequest`, the fetch row owns `fetch`; `ignoreUrls` on both excludes the collector origin.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-xml-http-request`
- Owns: browser XHR client spans, their `traceparent` injection, and the CORS pre-flight child span
- Accept: one root construction with `ignoreUrls` covering telemetry egress and `propagateTraceHeaderCorsUrls` naming the API origins
- Reject: library-altitude registration, unbounded custom attributes, propagation to origins outside the allow-list
