# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_USER_INTERACTION]

`@opentelemetry/instrumentation-user-interaction` opens one span per admitted DOM interaction (click by default) and parents the async work the handler triggers — the fetch a click causes lands under the click span, closing the user-action→request causality the RUM plane reads. It patches `Zone` when the zone manager is present and `HTMLElement.addEventListener` otherwise, so it is app-composition-root material registered inside the `web` SDK row, with the event roster and span-admission predicate as construction policy.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@opentelemetry/instrumentation-user-interaction`
- package: `@opentelemetry/instrumentation-user-interaction`
- license: `Apache-2.0`
- base: extends `@opentelemetry/instrumentation` `InstrumentationBase`
- consumed-by: the browser composition root beside the `web` export row and the zone manager
- runtime: browser only — patches `Zone` or `addEventListener`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: instrumentation + config
- rail: observability/rum

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]   | [CONSUMER_BOUNDARY]                                  |
| :-----: | :--------------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `UserInteractionInstrumentation`          | instrumentation | one row in the root's registered instrumentation set   |
|  [02]   | `UserInteractionInstrumentationConfig`    | config          | event roster + admission predicate at construction     |
|  [03]   | `EventName`                               | event union     | the admissible DOM event-name vocabulary               |
|  [04]   | `ShouldPreventSpanCreation`               | predicate shape | per-event span admission — the cardinality gate        |
|  [05]   | `AttributeNames`                          | attribute rows  | the row's own span-attribute vocabulary                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction policy
- rail: observability/rum

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                 |
| :-----: | :--------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `new UserInteractionInstrumentation(config?)`   | ctor           | one construction at the browser root                  |
|  [02]   | `eventNames` config field                       | config field   | the admitted event roster (click-only default)        |
|  [03]   | `shouldPreventSpanCreation` config field        | config field   | refuse spans for noise targets before they exist      |

## [04]-[IMPLEMENTATION_LAW]

[RUM_TOPOLOGY]:
- composition-root only — the row patches globals; a library registration double-instruments the host.
- admission before emission — high-frequency events (scroll, mousemove) enter only through a deliberate `eventNames` row with a `shouldPreventSpanCreation` gate, because every admitted event is a span.

[INTEGRATION_LAW]:
- Stack with `opentelemetry-context-zone.md`: the zone manager is what parents the triggered fetch under the interaction span; without it the row still opens interaction spans but async causality thins to same-tick work.
- Stack with `opentelemetry-instrumentation-fetch.md`: the click→fetch trace is these two rows composing — interaction parents, fetch childs, `Vital.enrich` projects timing onto the child.
- Stack with `otel/emit` `web` row: interaction spans ride the same redaction scrub and pagehide flush.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.

[RAIL_LAW]:
- Package: `@opentelemetry/instrumentation-user-interaction`
- Owns: DOM interaction spans and the async parenting of the work they trigger
- Accept: one construction at the root; a deliberate event roster; a span-admission predicate on every high-frequency row
- Reject: library-altitude registration, an unbounded event roster, interaction spans as a metrics substitute (the vital rows own graded scalars)
