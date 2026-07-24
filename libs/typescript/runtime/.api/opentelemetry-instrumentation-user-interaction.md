# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_USER_INTERACTION]

`@opentelemetry/instrumentation-user-interaction` opens one span per admitted DOM interaction and parents the async work its handler triggers, so a click's fetch lands under the click span and closes the user-action→request causality the RUM plane reads. It patches `Zone` when the zone manager is present and `HTMLElement.addEventListener` otherwise; the event roster and span-admission predicate are its construction policy.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-user-interaction`
- package: `@opentelemetry/instrumentation-user-interaction` (Apache-2.0)
- module: dual CJS + ESM flat barrel, no subpath exports; extends `@opentelemetry/instrumentation` `InstrumentationBase`
- runtime: browser only — patches `Zone` or `HTMLElement.addEventListener`
- rail: observability/rum

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the interaction instrumentation and its construction-policy shapes

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `UserInteractionInstrumentation`       | class         | one row in the root's registered instrumentation set    |
|  [02]   | `UserInteractionInstrumentationConfig` | interface     | event roster and admission predicate at construction    |
|  [03]   | `EventName`                            | union         | admissible DOM event names, `keyof HTMLElementEventMap` |
|  [04]   | `ShouldPreventSpanCreation`            | delegate      | per-event span admission — the cardinality gate         |
|  [05]   | `AttributeNames`                       | enum          | emitted span-attribute keys                             |

- `AttributeNames`: `event_type` `target_element` `target_xpath` `http.url`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and its policy fields

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `new UserInteractionInstrumentation(config?)` | ctor     | one construction at the browser root             |
|  [02]   | `eventNames`                                  | property | the admitted event roster, click-only by default |
|  [03]   | `shouldPreventSpanCreation`                   | property | refuse spans for noise targets before they open  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — the row patches globals; a library registration double-instruments the host.
- every admitted event is a span, so a high-frequency event (scroll, mousemove) enters only through a deliberate `eventNames` row gated by `shouldPreventSpanCreation`.

[STACKING]:
- `opentelemetry-context-zone.md` `ZoneContextManager`: the row detects the patched `Zone` and parents the triggered fetch under the interaction span; absent the manager it degrades to `addEventListener` patching and async causality thins to same-tick work.
- `opentelemetry-instrumentation-fetch.md` `FetchInstrumentation`: the click→fetch trace is these two rows composing — the interaction span parents, the fetch span childs, and `Vital.enrich` projects timing onto the child.
- `otel/emit` `web` row: interaction spans register in the same web SDK configuration and ride its redaction scrub and pagehide flush.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-user-interaction`
- Owns: DOM interaction spans and the async parenting of the work they trigger
- Accept: one construction at the root; a deliberate event roster; a span-admission predicate on every high-frequency row
- Reject: library-altitude registration, an unbounded event roster, interaction spans as a metrics substitute (the vital rows own graded scalars)
