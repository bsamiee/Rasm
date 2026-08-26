# [TS_RUNTIME_API_OPENTELEMETRY_INSTRUMENTATION_USER_INTERACTION]

`@opentelemetry/instrumentation-user-interaction` opens one span per admitted DOM interaction and parents the async work its handler triggers, so a click's fetch lands under the click span and closes the user-action→request causality the RUM plane reads. It patches `Zone` when the zone manager is present and `HTMLElement.addEventListener` otherwise; the event roster and span-admission predicate are its construction policy.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the interaction instrumentation and its construction-policy shapes

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `UserInteractionInstrumentation`       | class         | one row in the root's registered instrumentation set    |
|  [02]   | `UserInteractionInstrumentationConfig` | interface     | event roster and admission predicate at construction    |
|  [03]   | `EventName`                            | union         | admissible DOM event names, `keyof HTMLElementEventMap` |
|  [04]   | `ShouldPreventSpanCreation`            | delegate      | per-event span admission — the cardinality gate         |
|  [05]   | `AttributeNames`                       | enum          | emitted span-attribute keys                             |

- `AttributeNames`: `event_type` `target_element` `target_xpath` `url.full`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and its policy fields

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `new UserInteractionInstrumentation(config?)` | ctor     | one construction at the browser root             |
|  [02]   | `eventNames`                                  | property | the admitted event roster, click-only by default |
|  [03]   | `shouldPreventSpanCreation`                   | property | refuse spans for noise targets before they open  |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- composition-root only — the row patches globals; a library registration double-instruments the host.
- every admitted event is a span, so a high-frequency event (scroll, mousemove) enters only through a deliberate `eventNames` row gated by `shouldPreventSpanCreation`.
- `AttributeNames.URL_FULL` spells the page URL on its semantic-convention key, so a repo rule keyed on the convention — a scrub seal, a deny-list view, a gateway strip — reaches interaction spans with no instrumentation-specific alias; the three interaction-local keys carry no convention and stay this row's own vocabulary.

[STACKING]:
- `opentelemetry-context-zone.md` `ZoneContextManager`: the row detects the patched `Zone` and parents the triggered fetch under the interaction span; absent the manager it degrades to `addEventListener` patching and async causality thins to same-tick work.
- `opentelemetry-instrumentation-fetch.md` `FetchInstrumentation`: the click→fetch trace is these two rows composing — the interaction span parents, the fetch span childs, and `Vital.enrich` projects timing onto the child.
- `otel/emit` `web` row (within-lib): interaction spans register in the same web SDK configuration, and `AttributeNames.URL_FULL` lands exactly on the `url.full` key `Redaction.defaults` seals — so the interaction span's page URL scrubs at the export boundary through the row that already covers every other producer, with no interaction-local rule.

[LOCAL_ADMISSION]:
- `scope:runtime`, browser lane; registration lives only in the browser boot graph.
