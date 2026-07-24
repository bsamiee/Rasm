# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_DOCUMENT_LOAD]

`@opentelemetry/instrumentation-document-load` opens the navigation span tree once per page load — a `documentLoad` root over one `documentFetch` and per-resource fetch child spans, each carrying the Performance-Timeline navigation and paint entries as span events. It is the RUM navigation-trace producer, registered at the browser composition root inside the `web` SDK row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-document-load`
- package: `@opentelemetry/instrumentation-document-load` (Apache-2.0)
- base: extends `@opentelemetry/instrumentation` `InstrumentationBase`
- runtime: browser only — reads the document Performance Timeline at load
- rail: observability/rum

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation + config

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                  |
| :-----: | :------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `DocumentLoadInstrumentation`          | instrumentation | one row in the root's registered instrumentation set |
|  [02]   | `DocumentLoadInstrumentationConfig`    | config          | the hook + emission policy surface at construction   |
|  [03]   | `DocumentLoadCustomAttributeFunction`  | hook shape      | stamp the load and fetch document spans              |
|  [04]   | `ResourceFetchCustomAttributeFunction` | hook shape      | stamp each per-resource child span                   |
|  [05]   | `AttributeNames`                       | enum            | the emitted span-name vocabulary                     |
|  [06]   | `EventNames`                           | enum            | the paint span-event vocabulary                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction policy

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                           |
| :-----: | :----------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `new DocumentLoadInstrumentation(config?)` | ctor           | one construction at the browser root          |
|  [02]   | `applyCustomAttributesOnSpan`              | config field   | bounded stamps on load, fetch, resource spans |
|  [03]   | `ignoreNetworkEvents`                      | config field   | drop the network-timing span events           |
|  [04]   | `ignorePerformancePaintEvents`             | config field   | drop the paint span events                    |
|  [05]   | `semconvStabilityOptIn`                    | config field   | select the emitted HTTP semconv version       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — the row hooks the window load event; a library registration double-instruments the host.
- one page, one tree — the row fires once on the load event; SPA route changes are the router's own span concern, never a re-fire.

[STACKING]:
- `otel/emit` `web` row: the navigation tree exports under the same batch discipline and pagehide flush, so a bounce before idle still lands the load trace.
- `otel/vital`: two projections of one timeline — this row's spans carry the navigation events, the vital rows carry the graded scalars; neither re-derives the other.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-document-load`
- Owns: the per-navigation span tree over the document Performance Timeline
- Accept: one construction at the root; bounded attribute hooks; the emission toggles
- Reject: library-altitude registration, SPA-route re-firing, hook-stamped identifier-grade values
