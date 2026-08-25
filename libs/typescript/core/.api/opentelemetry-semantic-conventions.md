# [TS_CORE_API_OPENTELEMETRY_SEMANTIC_CONVENTIONS]

`@opentelemetry/semantic-conventions` owns the OpenTelemetry name vocabulary as code-generated literal-typed string constants — one `const` per attribute key, bounded value, metric name, and event name — with zero runtime dependency and zero peer. `observe/convention` re-exports the Rasm subset as typed rows, and a raw string literal at any signal site is the stringy-key defect.

## [01]-[VOCABULARY_PATTERN]

[VOCABULARY_TYPE_SCOPE]: the four generated constant families every name in the package belongs to

Every name is a literal-typed `const` binding `NAME` to its `"dotted.string"`, TYPE narrowed to the literal, so a mistyped key fails compile and the value discriminates dispatch. Each new convention generates one row in its family; `observe/convention` references the constant, never the literal, and the literal type flows to the OTLP attribute record.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `ATTR_<NAME>`          | attribute key | span/metric/log field-name vocabulary; value is the dotted key |
|  [02]   | `<GROUP>_VALUE_<ENUM>` | bounded value | an enum attribute's closed value set — a union fold value      |
|  [03]   | `METRIC_<NAME>`        | metric name   | `Metric` names on `data` meter rows + native OTLP metrics      |
|  [04]   | `EVENT_<NAME>`         | event name    | span/log event names; `EVENT_EXCEPTION` = `"exception"` crash  |

Generation owns the roster, so enumerating it here re-anchors the catalog to whatever names exist at one pin. Resolution runs mechanically instead: `build/src/index.d.ts` re-exports `trace`, `resource`, `stable_attributes`, `stable_metrics`, and `stable_events`, while `build/src/index-incubating.d.ts` re-exports those three `stable_*` modules beside `experimental_attributes`, `experimental_metrics`, and `experimental_events`.

Names resolve from `./incubating` if and only if an `experimental_*` module declares them, and that barrel is a strict superset of the stable one — so a stable name imported from `./incubating` compiles and merely forfeits the tier signal.

## [02]-[TIER_SPLIT]

[TIER_SPLIT_TYPE_SCOPE]: which of the two module entrypoints resolves each namespace, and what a consumer forfeits by taking the other

Which entrypoint a namespace resolves from is the load-bearing decision. Stable (`.`) names are API-frozen — safe in durable dashboards, SLO rows, and cross-language parity. Incubating (`./incubating`) names are overlay, renamed or dropped between minor releases, so `observe/convention` imports them behind a Rasm alias row absorbing the churn at one seam.

Promotion moves a name between the two module families, never between spellings, so an incubating-to-stable promotion breaks the IMPORT PATH alone and a rename breaks the identifier; the alias row absorbs the first mechanically and surfaces the second at compile.

Stable (`.`) namespaces, imported by default:

| [INDEX] | [NAMESPACE]                                              | [CONSUMER]                                                       |
| :-----: | :------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `service.*` (name/version/instance.id/namespace)         | `Resource` identity spine — `AppIdentity → resource`             |
|  [02]   | `container.image.*`, `k8s.*`                             | collector `k8s_attributes` enrichment; the cluster identity fold |
|  [03]   | `http.request.method`, `http.route`                      | collector `span_metrics` RED dimensions                          |
|  [04]   | `url.full`, `client.address`, `user_agent.original`      | egress-redaction seal set at `runtime/otel/emit`                 |
|  [05]   | `url.path`                                               | collector probe-traffic filter at `iac/operate/observe`          |
|  [06]   | `error.type`, `EVENT_EXCEPTION`                          | fatal-capture dimension and event at `runtime/otel/crash`        |
|  [07]   | `deployment.environment.name`                            | environment tier on the identity projection                      |
|  [08]   | `exception.*`, `code.*`                                  | `value/fault` `FaultCapture.Forensic` — direct import, unrowed   |
|  [09]   | `telemetry.*`, `otel.*`, `network.*`, `db.*`, `server.*` | unrowed — consumer-earned admission law                          |

Incubating (`./incubating`) namespaces, imported behind the alias row:

| [INDEX] | [NAMESPACE]                                                              | [CONSUMER]                                                   |
| :-----: | :----------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `browser.*` (brands/language/mobile/platform), `device.model.identifier` | vital RUM enrichment through the `Convention` aliases        |
|  [02]   | `session.*`, `network.connection.type` with its `TYPE_VALUE` family      | RUM session continuity and transport-class enrichment        |
|  [03]   | `cloud.region`/`cloud.availability_zone`, `host.name`                    | region, zone, and host folds on the identity projection      |
|  [04]   | `feature_flag.*` with its `RESULT_REASON` value family                   | flag telemetry hook and tracking seat at `runtime/proc/flag` |
|  [05]   | `cloudevents.event_*` (id/source/spec_version/subject/type)              | announcement identity on every event-fabric span             |
|  [06]   | `messaging.*` with its `OPERATION_TYPE` and `SYSTEM` value families      | transport coordinates on `runtime/net` publish and consume   |
|  [07]   | `host.arch`, `process.*`, `cloud.provider`                               | unrowed — SDK detectors name them without a row              |

- `feature_flag.*` churns hardest of the incubating set: it carries `feature_flag.provider.name` and `feature_flag.result.reason` at this pin, superseding the `@deprecated` `feature_flag.evaluation.reason` predecessor and its parallel value family, so the alias row makes the next move one seam edit rather than a sweep of every flag site.
- Two `*_VALUE_*` families reach `observe/convention` rows: `FEATURE_FLAG_RESULT_REASON_VALUE_*` and `NETWORK_CONNECTION_TYPE_VALUE_*` (`cell`/`unavailable`/`unknown`/`wifi`/`wired`). Each spells the TELEMETRY vocabulary its producer answers in another dialect — the OpenFeature SDK resolves uppercase reasons, the Network Information API answers `cellular`/`ethernet`/`none` — so the emitting owner maps dialect onto row at its stamp site.
- `NETWORK_CONNECTION_SUBTYPE_VALUE_*` stays unrowed beside its type family: no browser surface reports the cellular generation, so no signal concept stamps it.
- `cloudevents.event_id` and `cloudevents.event_source` are the pair the specification's own uniqueness composite is made of, so a span, a subscription, and a dedup key read ONE coordinate — a private correlation attribute beside them names the same fact twice.
- `messaging.system` is an OPEN enum: `MESSAGING_SYSTEM_VALUE_*` generates `kafka`, `rabbitmq`, `pulsar`, and the cloud brokers at this pin and names neither NATS nor MQTT, both of which the branch binds, so those two emitting owners spell their own system value while every rostered one reads the constant. `MESSAGING_OPERATION_TYPE_VALUE_*` is bounded by contrast — `create`/`publish`/`receive`/`process`/`settle` span the whole fabric, so a lane naming a sixth step is describing one of these five.
- `messaging.client.*` and `messaging.process.*` metric names stay unrowed: the branch's throughput and latency instruments are `rasm.*` rows on its own meter, so adopting the spec names publishes a second series beside them under one meaning.
- `container.*` and `k8s.*` sit in `stable_attributes` at this pin and resolve from BOTH entries, so an import left on `./incubating` compiles while forfeiting the frozen-spelling claim every durable dashboard reads — tier placement is re-proved against the installed build at each pin bump, never inherited from the previous roster.
- `ATTR_CONTAINER_IMAGE_TAGS` is the whole image-tag surface at this pin — a plural array key with no singular twin — so a `container.image.tag` spelling downstream is a collector dialect resolving to no semconv name. `container.id` stays outside the `observe/convention` rows for the detector reason, not the tier reason: `containerDetector` stamps it onto the resource at `runtime/otel/emit`, the same carve `host.arch` and `process.*` take.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every name crosses as its generated constant, never its literal: the constant's type IS the literal, so a mistyped key fails compile, a bounded value discriminates a fold, and the same identifier carries the spelling to every signal site. Generation owns the roster, so this catalog anchors the RESOLUTION rule and the tier split rather than a name census that goes stale at the next pin.

[STACKING]:
- `observe/convention` (primary consumer): the plane's vocabulary spine. It imports the Rasm-relevant `ATTR_*`/`METRIC_*`/`EVENT_*`/`*_VALUE_*` constants — stable by default, incubating behind a churn-absorbing alias row — and re-exports them as typed rows every telemetry node names fields through; `*_VALUE_*` families become `Match`-discriminated union values.
- runtime `otel/emit`: the OTLP export lane stamps `Convention` rows on span/metric/log attributes at egress and keys the identity `Resource` from the one `Convention.identity` projection; egress-redaction rows scrub PII by attribute key against the same vocabulary.
- runtime `otel/vital`: the estate's one CWV owner stamps the RUM alias rows — `browser.*` off the UA client hints, `device.model.identifier`, `session.id`/`session.previous_id`, `network.connection.type` mapped from the Network Information API's own vocabulary onto the bounded spec row — beside its own `rasm.vital*` dimensions, so a graded vital fact carries client, device, session, and transport context under one vocabulary.
- runtime `net/pubsub` + `net/channel` + `serve/route` + `work/deliver`: the event fabric's own span and metric dimensions — the `cloudevents.event_*` five off the announced message envelope, the `messaging.*` coordinates off the binding row already deciding destination, partition, consumer group, and batch arity — so a published fact, its transport hop, and its handler share one attribute vocabulary across three transports.
- runtime `proc/flag`: the OpenFeature SDK telemetry hook stamps `feature_flag.key`/`.provider.name`/`.result.reason`/`.context.id`/`.result.variant` on the active evaluation span, mapping the SDK's uppercase resolution reason onto the spec's lowercase bounded value at that one site; the `Provider.track` seat stamps `feature_flag.context.id` beside the `rasm.flag.*` tracking rows, so outcomes join evaluations on the targeting identity.
- `iac/operate/observe`: every attribute key the gateway names resolves to a convention row — `k8s_attributes` extracts the whole `k8s.*`/`container.image.*` placement roster and associates pods on `k8s.pod.ip`/`k8s.pod.uid`, `span_metrics` takes `http.route` and `http.request.method` as RED dimensions, the OTTL migration targets `deployment.environment.name`, the probe filter reads `url.path`, and the gateway's own self-telemetry resource stamps `service.namespace` with no `AppIdentity` present.
- `value/fault` + runtime `otel/crash`: `FaultCapture.Forensic` anchors `exception.*` and the `code.*` frame quartet by direct import — those keys carry no convention row — while `error.type` and `EVENT_EXCEPTION` are rowed for the crash dimension and event; a shared-import boundary beside `observe/convention`, two owners over one spec vocabulary, never a re-export hop.
- cross-language parity, C# `Rasm.AppHost/Observability/Telemetry`: the wire is OTel, so parity is name-level against the spec — a Rasm span from either language carries `service.name`/`http.route`/`exception.type` identically, and this package is the JS-side name source, not a shared artifact.

[LOCAL_ADMISSION]:
- Pure data with zero deps, so this package is the standing name source the `[OTEL_PIN_BLOCK]` SDK-block retirement leaves behind and never a member of that block.
- Rasm-owned fact vocabularies — audit actor/action/target, meter counters — are project convention rows beside these imports, never re-declarations of a spec name.
- Cross-language parity is name-level against the OTel spec, so no shared artifact crosses the branch boundary.
