# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_DOCUMENT_LOAD]

`@opentelemetry/instrumentation-document-load` opens the navigation span tree once per page load — a `documentLoad` root over one `documentFetch` and per-resource fetch child spans, each carrying the Performance-Timeline navigation and paint entries as span events. It is the RUM navigation-trace producer, registered at the browser composition root inside the `web` SDK row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-document-load`
- package: `@opentelemetry/instrumentation-document-load` (Apache-2.0)
- base: extends `@opentelemetry/instrumentation` `InstrumentationBase`
- runtime: browser only — reads the document Performance Timeline at load
- rail: observability/rum

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the instrumentation, its config, and the span-name vocabulary — the whole barrel export

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `DocumentLoadInstrumentation`          | class         | one row in the root's registered instrumentation set |
|  [02]   | `DocumentLoadInstrumentationConfig`    | interface     | the hook and emission policy surface at construction |
|  [03]   | `DocumentLoadCustomAttributeFunction`  | delegate      | stamp the load and fetch document spans              |
|  [04]   | `ResourceFetchCustomAttributeFunction` | delegate      | stamp each per-resource child span                   |
|  [05]   | `AttributeNames`                       | enum          | the emitted span-name vocabulary                     |

- `AttributeNames`: `documentLoad` `documentFetch` `resourceFetch` — span names, never attribute keys; the paint and network event names stay interior and reach a consumer only as emitted span events.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction policy

| [INDEX] | [SURFACE]                                  | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :----------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `new DocumentLoadInstrumentation(config?)` | ctor     | one construction at the browser root          |
|  [02]   | `applyCustomAttributesOnSpan`              | property | bounded stamps on load, fetch, resource spans |
|  [03]   | `ignoreNetworkEvents`                      | property | drop the network-timing span events           |
|  [04]   | `ignorePerformancePaintEvents`             | property | drop the paint span events                    |
|  [05]   | `enabled`                                  | property | inherited `InstrumentationConfig` gate        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — the row hooks the window load event; a library registration double-instruments the host.
- one page, one tree — the row fires once on the load event; SPA route changes are the router's own span concern, never a re-fire.
- `DocumentLoadInstrumentation` emits stable HTTP and URL semantic conventions unconditionally — no version selector exists on the config, so a collector pipeline and a dashboard read one attribute generation off this producer and a deployment states nothing to get it.

[STACKING]:
- `@opentelemetry/instrumentation`(`.api/opentelemetry-instrumentation.md`): `DocumentLoadInstrumentation` extends `InstrumentationBase` and enters `registerInstrumentations({ instrumentations })` as one `Instrumentation` value bound to the browser lane's `tracerProvider`; `InstrumentationConfig.enabled` is the zeroth column a kiosk build refuses the row on.
- `@opentelemetry/sdk-trace-web`(`.api/opentelemetry-sdk-trace-web.md`): the fetch child spans read the same `PerformanceResourceTiming` entries `addSpanNetworkEvents` projects, so the resource-timing buffer serves both producers and neither row may clear it.
- `otel/emit` `web` row (within-lib): the navigation tree exports under `policy.caps.batch`'s `disableAutoFlushOnDocumentHide` posture and `Redaction.processor`'s `onEnding` scrub, so a bounce before idle still lands the load trace with `url.full` sealed.
- `otel/vital`: two projections of one timeline — this row's spans carry the navigation events, the vital rows carry the graded scalars; neither re-derives the other.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-document-load`
- Owns: the per-navigation span tree over the document Performance Timeline under stable HTTP and URL semantic conventions
- Accept: one construction at the root; bounded attribute hooks; the emission toggles; the `enabled` refusal column
- Reject: library-altitude registration, SPA-route re-firing, hook-stamped identifier-grade values, a deep import reaching an interior enum the barrel withholds
