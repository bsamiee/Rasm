# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_DOCUMENT_LOAD]

`@opentelemetry/instrumentation-document-load` opens the navigation span tree once per page load: a `documentLoad` root over `documentFetch` and per-resource fetch child spans, each carrying the Performance-Timeline navigation events as span events. It is the RUM navigation-trace producer beside the vital rows — `Vital` folds the same timeline into gauges and counters while this row renders it as one trace a Tempo click-through opens. One-shot by nature (it fires on the load event), registered at the browser composition root inside the `web` SDK row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-document-load`
- package: `@opentelemetry/instrumentation-document-load`
- license: `Apache-2.0`
- base: extends `@opentelemetry/instrumentation` `InstrumentationBase`
- consumed-by: the browser composition root beside the `web` export row
- runtime: browser only — reads the document Performance Timeline at load

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation + config
- rail: observability/rum

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                |
| :-----: | :--------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `DocumentLoadInstrumentation`             | instrumentation | one row in the root's registered instrumentation set |
|  [02]   | `DocumentLoadInstrumentationConfig`       | config          | the hook policy surface at construction              |
|  [03]   | `DocumentLoadCustomAttributeFunction`     | hook shape      | stamp the `documentLoad` root span                   |
|  [04]   | `ResourceFetchCustomAttributeFunction`    | hook shape      | stamp each per-resource child span                   |
|  [05]   | `AttributeNames`                          | attribute rows  | the row's own span-attribute vocabulary              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction
- rail: observability/rum

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                            |
| :-----: | :-------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `new DocumentLoadInstrumentation(config?)` | ctor        | one construction at the browser root             |
|  [02]   | `applyCustomAttributesOnSpan` config hooks | config field | bounded stamps on root and resource spans        |

## [04]-[IMPLEMENTATION_LAW]

[RUM_TOPOLOGY]:
- composition-root only — the row hooks the window load event; a library registration double-instruments the host.
- one page, one tree — the row fires once per navigation; SPA route changes are the router's own span concern, never a re-fire of this row.

[INTEGRATION_LAW]:
- Stack with `otel/emit` `web` row: the navigation tree exports under the same batch discipline and pagehide flush, so a bounce before idle still lands the load trace.
- Stack with `otel/vital`: two projections of one timeline — this row's spans carry the navigation events, the vital rows carry the graded scalars; neither re-derives the other.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-document-load`
- Owns: the per-navigation span tree over the document Performance Timeline
- Accept: one construction at the root; bounded attribute hooks
- Reject: library-altitude registration, SPA-route re-firing, hook-stamped identifier-grade values
