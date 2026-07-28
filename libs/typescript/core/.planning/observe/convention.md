# [CORE_CONVENTION]

Vocabulary spine of the four-signal plane: every name a telemetry node stamps — attribute key, metric, event, bounded value, instrument row — is a typed row on the one `Convention` owner: semconv constants as literal-typed data, incubating names behind a churn-absorbing alias table, Rasm-owned families minted beside them, and the one `AppIdentity -> Identity` projection parameterizing the plane per app. New name families land as one row in the owning table; a string literal or call-site unit conversion at a signal site is the named defect — the row is the instruction.

Conformance rides as rows: dotted `rasm.<domain>.<measure>` names under UCUM codes closing against `_domain`, `Convention.scope` the scope spelling, `Convention.wire` the estate pins. `Convention.translated` projects a store's series name, so suffixing is a target property; `Convention.mount` materializes a row into its handle, so a site names an instrument. Closed rosters — `keys`, `kinds`, `units`, `durations` — publish as data, `dimensions` generates the metric-plane roster off each row's own fan, and C# parity stays name-level. Its module is `core/src/observe/convention.ts`.

## [01]-[INDEX]

- [02]-[SEMCONV_ROWS] — the tier law with its promotion move, the plane consumers, the bounded-value families and alias seam.
- [03]-[RASM_ROWS] — the branch module and domain rosters and the project-owned name families resolving against them.
- [04]-[IDENTITY_PROJECTION] — the assembled owner: key census, metric-plane roster, record contract, identity, scope, wire pins, published rosters, translation, mount.

## [02]-[SEMCONV_ROWS]

- Owner: the interior `_attr`/`_incubating`/`_value` anchors — flat `as const` tables whose values are `@opentelemetry/semantic-conventions` constants, so every row keeps the package's literal type and a semconv rename fails at this declaration, never at a call site.
- Packages: `@opentelemetry/semantic-conventions` (stable `.` entry) and `@opentelemetry/semantic-conventions/incubating` — pure data, zero peers, unused rows tree-shaken to zero bytes; the one `@opentelemetry/*` admission in `core`, while the SDK and exporter machinery stays fenced to the runtime export owner.
- Law: stable-tier names are the default import — API-frozen, safe inside durable dashboards, SLO policy rows, and cross-language wire parity; an incubating name enters only through the `_incubating` alias table, whose Rasm-stable keys absorb a minor-release rename at one seam while the constant-valued rows keep the break compile-visible.
- Law: promotion moves a name between the entry modules and never between spellings, so a promoted constant leaves `_incubating` for `_attr` in the same pass its import path moves; the `/incubating` entry re-exports the stable modules whole, so a promoted name left behind still compiles and silently forfeits the tier signal every durable consumer reads — the tier a row sits in IS the claim that its spelling is frozen, and the installed pin decides it, never the row's age.
- Law: `_attr` spans the frozen planes its consumers own — the resource identity spine (`service.*`, `deployment.environment.name`), the egress-redaction seam (`client.address`, `url.full`, `user_agent.original`), the fatal-capture dimension (`error.type`), the gateway shaping keys (`http.route` and `http.request.method` on the span-metrics connector, `url.path` on the probe filter), and the placement-correlation plane (`container.image.*`, `k8s.*`) carrying the join from a workload series to the pod that emitted it.
- Law: the placement plane's row set IS that processor's own roster, proved both directions — every key it extracts or associates on carries a row here and `iac/operate/observe#CHART_ROWS` names every row back, so the enrichment config spells no attribute key free-string and neither side gains a key the other cannot resolve; `k8s.pod.ip` earns its row as an association SOURCE the processor reads rather than a dimension it stamps, and the image key is the plural array spelling the installed pin declares.
- Law: `_incubating` spans the two planes still overlay at the pin — the RUM plane (`browser.*`, `device.model.identifier`, `session.*`, `network.connection.type`, stamped on the vital and crash signals), the placement plane's unfrozen half (`cloud.region`, `cloud.availability_zone`, and `host.name`, all three folded by the identity projection) — beside the flag-evaluation plane (`feature_flag.key`, `feature_flag.provider.name`, `feature_flag.result.reason`), so an enrichment consumer composes rows and never opens a second import of the incubating entry.
- Law: bounded attribute values import as `*_VALUE_*` rows and dispatch as discriminated values — `_value` carries the flag-reason and network-connection families in one table whose key prefix IS the family discriminant, each family's `ValueOf` clause extracting its union by template over that prefix so a third bounded family lands as rows under an unchanged projection while the `_Value` guard refuses a row no family claims, and a `Match` arm or vocabulary lookup keys on the row, never on a free string.
- Law: a bounded row carries the TELEMETRY value vocabulary and the emitting owner maps its own dialect onto that row at the stamp site, never here — the flag SDK resolves `TARGETING_MATCH` where the spec spells `targeting_match` and the codec `FlagVerdict.reason` axis carries the contract wire spelling beside it, the Network Information API answers `cellular`/`ethernet`/`none` where the spec spells `cell`/`wired`/`unavailable` — so a dialect value reaching a bounded key unmapped is exactly the free string the row exists to delete, and the mapping is total at its one site or the residue folds to the family's own unknown row.
- Law: every row imports a flat `ATTR_*`/`METRIC_*`/`*_VALUE_*` constant from the current spec surface — one vocabulary, one constant form, so each row keeps its literal type and the census stays greppable.
- Law: admission is consumer-earned — a namespace with no consuming signal concept stays unrowed (`otel.*` scope and status, the `network.*` transport keys beyond the rowed connection type, `db.*` because `@effect/sql` stamps its own spans, `exception.*` because `value/fault` owns the forensic anchors, `host.arch`, `process.*`, and `container.id` because the SDK detector roster stamps them onto the resource without this table), and an unimported constant costs zero bytes under tree-shaking. Consumption means a signal concept STAMPS or QUERIES the key, never that the census carries it: every row here answers with the emitting or querying owner its plane law names, and a row whose named consumer is absent from disk deletes rather than standing as a promise.
- Growth: a new convention is one row in the owning family table — attribute, metric, event, or value — never a new export, file, or parallel constant.

```typescript signature
import {
  ATTR_CLIENT_ADDRESS,
  ATTR_CONTAINER_IMAGE_NAME,
  ATTR_CONTAINER_IMAGE_TAGS,
  ATTR_DEPLOYMENT_ENVIRONMENT_NAME,
  ATTR_ERROR_TYPE,
  ATTR_HTTP_REQUEST_METHOD,
  ATTR_HTTP_ROUTE,
  ATTR_K8S_CLUSTER_NAME,
  ATTR_K8S_CONTAINER_NAME,
  ATTR_K8S_CRONJOB_NAME,
  ATTR_K8S_DAEMONSET_NAME,
  ATTR_K8S_DEPLOYMENT_NAME,
  ATTR_K8S_JOB_NAME,
  ATTR_K8S_NAMESPACE_NAME,
  ATTR_K8S_NODE_NAME,
  ATTR_K8S_POD_IP,
  ATTR_K8S_POD_NAME,
  ATTR_K8S_POD_UID,
  ATTR_K8S_STATEFULSET_NAME,
  ATTR_SERVICE_INSTANCE_ID,
  ATTR_SERVICE_NAME,
  ATTR_SERVICE_NAMESPACE,
  ATTR_SERVICE_VERSION,
  ATTR_URL_FULL,
  ATTR_URL_PATH,
  ATTR_USER_AGENT_ORIGINAL,
  EVENT_EXCEPTION,
  METRIC_HTTP_SERVER_REQUEST_DURATION,
} from "@opentelemetry/semantic-conventions" // container.* and k8s.* are stable at the manifest pin: the frozen tier is where the collector's enrichment keys belong
import {
  ATTR_BROWSER_BRANDS,
  ATTR_BROWSER_LANGUAGE,
  ATTR_BROWSER_MOBILE,
  ATTR_BROWSER_PLATFORM,
  ATTR_CLOUD_AVAILABILITY_ZONE,
  ATTR_CLOUD_REGION,
  ATTR_DEVICE_MODEL_IDENTIFIER,
  ATTR_FEATURE_FLAG_KEY,
  ATTR_FEATURE_FLAG_PROVIDER_NAME,
  ATTR_FEATURE_FLAG_RESULT_REASON,
  ATTR_HOST_NAME,
  ATTR_NETWORK_CONNECTION_TYPE,
  ATTR_SESSION_ID,
  ATTR_SESSION_PREVIOUS_ID,
  FEATURE_FLAG_RESULT_REASON_VALUE_CACHED,
  FEATURE_FLAG_RESULT_REASON_VALUE_DEFAULT,
  FEATURE_FLAG_RESULT_REASON_VALUE_DISABLED,
  FEATURE_FLAG_RESULT_REASON_VALUE_ERROR,
  FEATURE_FLAG_RESULT_REASON_VALUE_SPLIT,
  FEATURE_FLAG_RESULT_REASON_VALUE_STALE,
  FEATURE_FLAG_RESULT_REASON_VALUE_STATIC,
  FEATURE_FLAG_RESULT_REASON_VALUE_TARGETING_MATCH,
  FEATURE_FLAG_RESULT_REASON_VALUE_UNKNOWN,
  NETWORK_CONNECTION_TYPE_VALUE_CELL,
  NETWORK_CONNECTION_TYPE_VALUE_UNAVAILABLE,
  NETWORK_CONNECTION_TYPE_VALUE_UNKNOWN,
  NETWORK_CONNECTION_TYPE_VALUE_WIFI,
  NETWORK_CONNECTION_TYPE_VALUE_WIRED,
} from "@opentelemetry/semantic-conventions/incubating" // feature_flag.result.* supersedes the deprecated feature_flag.evaluation.* family: the alias row absorbs the next move
import { Array, Duration, Metric, MetricBoundaries, Option, Record, type Types } from "effect"
import type { AppIdentity } from "../value/identity.ts"

const _attr = {
  clientAddress: ATTR_CLIENT_ADDRESS,
  containerImage: ATTR_CONTAINER_IMAGE_NAME,
  containerImageTags: ATTR_CONTAINER_IMAGE_TAGS,
  deploymentEnvironment: ATTR_DEPLOYMENT_ENVIRONMENT_NAME,
  errorType: ATTR_ERROR_TYPE,
  httpMethod: ATTR_HTTP_REQUEST_METHOD,
  httpRoute: ATTR_HTTP_ROUTE,
  k8sCluster: ATTR_K8S_CLUSTER_NAME,
  k8sContainer: ATTR_K8S_CONTAINER_NAME,
  k8sCronJob: ATTR_K8S_CRONJOB_NAME,
  k8sDaemonSet: ATTR_K8S_DAEMONSET_NAME,
  k8sDeployment: ATTR_K8S_DEPLOYMENT_NAME,
  k8sJob: ATTR_K8S_JOB_NAME,
  k8sNamespace: ATTR_K8S_NAMESPACE_NAME,
  k8sNode: ATTR_K8S_NODE_NAME,
  k8sPodIp: ATTR_K8S_POD_IP, // the association source, never an extracted dimension: the processor reads it to key a pod and stamps the rest
  k8sPodName: ATTR_K8S_POD_NAME,
  k8sPodUid: ATTR_K8S_POD_UID,
  k8sStatefulSet: ATTR_K8S_STATEFULSET_NAME,
  serviceInstance: ATTR_SERVICE_INSTANCE_ID,
  serviceName: ATTR_SERVICE_NAME,
  serviceNamespace: ATTR_SERVICE_NAMESPACE,
  serviceVersion: ATTR_SERVICE_VERSION,
  urlFull: ATTR_URL_FULL,
  urlPath: ATTR_URL_PATH,
  userAgent: ATTR_USER_AGENT_ORIGINAL,
} as const

const _incubating = {
  browserBrands: ATTR_BROWSER_BRANDS,
  browserLanguage: ATTR_BROWSER_LANGUAGE,
  browserMobile: ATTR_BROWSER_MOBILE,
  browserPlatform: ATTR_BROWSER_PLATFORM,
  cloudRegion: ATTR_CLOUD_REGION,
  cloudZone: ATTR_CLOUD_AVAILABILITY_ZONE,
  connectionType: ATTR_NETWORK_CONNECTION_TYPE,
  deviceModel: ATTR_DEVICE_MODEL_IDENTIFIER, // the UA client hints expose a model alone: device.manufacturer has no browser source to stamp
  flagKey: ATTR_FEATURE_FLAG_KEY,
  flagProvider: ATTR_FEATURE_FLAG_PROVIDER_NAME,
  flagReason: ATTR_FEATURE_FLAG_RESULT_REASON,
  hostName: ATTR_HOST_NAME,
  sessionId: ATTR_SESSION_ID,
  sessionPrevious: ATTR_SESSION_PREVIOUS_ID,
} as const

// Bounded-value rows: the key prefix names the family, so a `ValueOf` clause extracts its union by template and the
// `_Value` guard refuses a row belonging to no family — a bounded key whose union nothing binds is the free string.
const _value = {
  connectionCell: NETWORK_CONNECTION_TYPE_VALUE_CELL,
  connectionUnavailable: NETWORK_CONNECTION_TYPE_VALUE_UNAVAILABLE,
  connectionUnknown: NETWORK_CONNECTION_TYPE_VALUE_UNKNOWN,
  connectionWifi: NETWORK_CONNECTION_TYPE_VALUE_WIFI,
  connectionWired: NETWORK_CONNECTION_TYPE_VALUE_WIRED,
  flagCached: FEATURE_FLAG_RESULT_REASON_VALUE_CACHED,
  flagDefault: FEATURE_FLAG_RESULT_REASON_VALUE_DEFAULT,
  flagDisabled: FEATURE_FLAG_RESULT_REASON_VALUE_DISABLED,
  flagError: FEATURE_FLAG_RESULT_REASON_VALUE_ERROR,
  flagSplit: FEATURE_FLAG_RESULT_REASON_VALUE_SPLIT,
  flagStale: FEATURE_FLAG_RESULT_REASON_VALUE_STALE,
  flagStatic: FEATURE_FLAG_RESULT_REASON_VALUE_STATIC,
  flagTargeting: FEATURE_FLAG_RESULT_REASON_VALUE_TARGETING_MATCH,
  flagUnknown: FEATURE_FLAG_RESULT_REASON_VALUE_UNKNOWN,
} as const
```

## [03]-[RASM_ROWS]

- Owner: the `_module`, `_domain`, `_rasm`, `_ESTATE`, `_metric`, `_unit`, `_units`, `_scale`, `_promUnit`, `_grafanaUnit`, `_tail`, `_instrument`, `_durations`, `_event`, and `_profile` anchors — the project's own name space beside the semconv imports; `_module` is the branch's exports-map subpath roster and `_domain` its declared domain roster under Tier-0 `[08]-[OBSERVABILITY_CONFORMANCE]`, and the `_Module`/`_Domain`/`_Rasm`/`_RasmMetric`/`_Event` guards close every project-owned name against those keys, so an unrostered segment or an unrostered emitter fails at the declaration rather than at a backend join; `_instrument` correlates every metric name with its wire form, UCUM unit, description, bucket ladder, value width, and fan, including the derivative queue/active pressure gauges, so SLI admission, dashboard units, the metric-plane roster, and the one instrument mount derive from the declaration instead of accepting any metric in any role.
- Law: the UCUM code set is a row family exactly like the names — `_unit` closes the `unit` column so a mistyped or invented code refuses at the row, and `_scale` carries one millisecond expressed in each temporal code, which is what makes a code temporal at all; a unit spelled free-string at a row is the same defect as a free-string metric name, and the boundary between a measured code and an annotation code (`{deliverable}`, `{lease}`) is the row's own spelling.
- Law: `_units` carries that same code set as the ordered tuple, so order, iteration, and non-emptiness are tuple facts a consumer's schema spreads rather than key facts it re-lists.
- Law: `_promUnit` carries the Prometheus base-unit word each code translates to, total over the unit vocabulary, so a new code answers its store-side spelling at admission rather than renaming its own series on the first suffixing receiver.
- Law: `_grafanaUnit` carries the display id each code renders under, total over the same vocabulary on the `_promUnit` precedent, so a panel row names a UCUM code and the deploy plane spells the renderer's word — a code shipped raw to a display field resolves to no registry entry and renders the series unitless, and a display word spelled beside a panel forks the correspondence per author.
- Law: the display column splits `level` from `rate` because the fold decides the id — an instant, a distribution rung, and a threshold read the quantity while a windowed rate reads it per second, so a byte counter's rate is a throughput id and a temporal counter's rate is an occupancy fraction; a panel builder picks the column its own fold reads and no row carries a second unit field.
- Law: `_tail` keys the type tail on the kind column, so that column IS the wire form a receiver reads — `counter` and `frequency` export a monotonic sum, `updown` the non-monotonic one, `gauge` a level, `histogram` a distribution — and a level maintained by increment and release takes the `updown` row rather than a gauge row no mount can honour.
- Law: a level counting things takes the annotation code naming them, because the `1` code turns a gauge into a RATIO at the suffixing receiver — a depth, an occurrence tally, or a drain position minted dimensionless renders `<name>_ratio` through `Convention.translated` and reaches every board panel and alert rule under a noun it never measures.
- Law: `bounds` rides the distribution rows alone and names a GENERATED ladder — `exponential` and `linear` derive their edges from three scalars, `explicit` carries the edges an external contract already fixes — so one row spells the bucket vector the whole branch mints from.
- Law: `count` counts the FINITE edges a ladder mints, because `MetricBoundaries` appends its own `+Inf` bucket and spends one generator slot on it — the row generates its rungs and hands them over, so the projected edge set is exactly the set a `le` can name.
- Law: edges freeze at the mint because a collected point cannot be re-bucketed, so a ladder reads no tunable policy row — a cost, budget, or ceiling bump moves the objective naming the series and leaves the ladder standing.
- Law: `bigint` marks a row whose value outgrows the double-safe integer range, so the mount takes Effect's 64-bit posture and the bridge exports an integer point; the column rides the counting kinds, a distribution and a word census carrying no such value.
- Law: `dimensions` names the keys a row's series FANS on, so the metric-plane roster folds from the instrument table rather than from the attribute vocabulary — a key the census carries for span or log use widens no allow-list, and an axis a governor must pass is one entry on the row minting it.
- Law: a row declares only what its own producer stamps, so a board grouping on a key no row names reads one flat series and the divergence surfaces at the census rather than at the panel; the gateway-verb axis rides spans, which is why its counter names no fan.
- Law: the drain counter fans on the stream tag beside the audit verb and actor class — the three axes every drained row carries or lawfully omits, all bounded — while the actor, target, and retention keys stay log annotations because an actor key is identifier-grade and a retention class answers policy rather than a board; a fan present on one stream alone splits the counter into two lawful series rather than forcing a second instrument.
- Law: the fatal capture fans on `error.type` alone — the bounded fault class is the one dimension the counter stamps, while fingerprint and hop evidence rides the fatal log band, so a Rasm row restating the semconv class duplicates a name the vocabulary already owns and a fingerprint on a metric axis carries identifier-grade cardinality onto a series.
- Law: `ring` and `tenant` stay unspellable as a row's fan — the identity projection stamps the estate pair onto every series, so a row restating one forks the correspondence and re-declares a dimension it never owns.
- Law: a word census names no fan, because its one axis is `wire.occurrence` and both metric bridges append that key themselves; a frequency row spelling an axis declares a dimension its own kind forecloses.
- Law: only genuinely-Rasm vocabulary earns a row — an axis is admitted by the signal, work-plane, or security concept that stamps it, and the `_rasm` fence owns the roster; re-declaring an OTel standard name as a Rasm row is the rejected duplication, and when semconv promotes an equivalent stable name the row's value migrates to the imported constant with every consumer following through the literal type.
- Law: the `ui:system/hook` rail is the package-keyed hook-point namespace Tier-0's grammar carve exempts, so its segments earn no `_domain` row while every ui SERIES resolves here.
- Law: a `_domain` row names the capability SUBJECT a query joins on, never the package minting it — its `emitters` column carries every module emitting under the segment, closed by the `_Domain` guard against the `_module` roster's own emitting half so a misspelled module or a non-mounting one fails at the table rather than orphaning a series, and two emitters serving one subject share the row while `service.name` separates their series, a module spanning two subjects emitting under both. Estate identity dimensions (`ring`, `tenant`) carry no segment and ride the identity projection instead; the corpus `TELEMETRY_CONVENTION` mint projects this roster, so one segment two branches admit spells one subject byte-identical or fails parity there.
- Law: metric names are rows exactly like attribute keys — the runtime fact-stream, vital, and meter-bridge owners declare their instruments against `_metric` rows, the data lane and batch owners against the lane/batch rows, the security owners against the security instrument family, `slo#OBJECTIVE` objectives reference the same rows, `interchange/invoke#CAPABILITY_BIND` and `interchange/invoke#COMMAND_GATEWAY` declare the capability-plane instruments against the invoke/gateway rows, and `board#QUERY` queries build from them, so one rename propagates the whole plane at compile time.
- Law: the outbox/queue/relay rows are the work plane's lossy projection vocabulary — journal fact rows stay the billing and evidence truth, and the runtime meter bridge mints these instruments FROM those facts and census probes, so a dashboard of durable-work health reads OTel series while every dispute settles against the journal.
- Law: outcome and reason tag VALUES stay bounded — the invoke outcome vocabulary is the `Exit`-fold union the emitting page anchors, the fault-reason vocabulary is the codec `Hops` row set, the security kind/dialect/surface/reason vocabularies are the reject stream's closed tables — so a Rasm tag key here never carries an identifier-grade value; identifier context rides span attributes and log annotations.
- Law: core's log ownership is the `_event` vocabulary alone — log records mint through `Effect.log*` and reach the wire on the runtime export lane, so an event name is a row here, the record shape stays Effect's, and the decoded-evidence plane (`Receipt`, `FaultCapture`) owns every structured-evidence need; `breadcrumb` names the replay annotations riding the fatal capture, never a standalone record stream, so its consumer is the crash emission, not a board panel.
- Law: the object rows are the content-addressed plane's receipt projections — the data object owners tap `objectWritten`, `objectSize`, and `objectReclaimed` off the write-receipt and sweep-mark folds, and `streamSize` projects the resumable-upload finalize receipt once per completed upload so retried offsets never double-count — instruments the runtime meter bridge exports while the receipts stay the evidence truth, and `board#PACKS`'s `object` pack is their standing consumer. Both byte measures name the quantity and carry `By` on the row: a `.bytes` tail restates the unit column inside the series name, which the Tier-0 grammar refuses because the descriptor already carries it and a UCUM rescale then renames the series.
- Law: the lake rows are the storage-harvest projection family — the olap trio meters the lake-engine admission lanes, `profileDuration` lands the harvested DuckDB/SQLite/pg engine profiles, and `cacheHits`/`cacheMisses`/`poolHeld` project the data-lane cache and pool census — every row a data-lane tap with its `board#PACKS` `lake` consumer, so lake-engine profile parity reads as OTel series beside every other signal.
- Law: the bench rows are the claim bridge's lossy projection — the runtime meter bridge mints the timing, GC, heap, and hardware-counter bands from landed benchmark claims and `benchVerdicts` from the `board#PACKS` regression fold's grades — so boards trend claims while every dispute settles against the decoded claim landing, exactly the journal-versus-series split the work plane holds.
- Law: `_profile.id` is the one profile-link attribute the runtime profiling bridge stamps on the long-lived scoped spans.
- Law: `_profile.spanId` and `_profile.traceId` carry those same identifiers back as the store's own sample labels.
- Law: that identifier pair closes the join both directions, so a profile query resolves its span rather than a time window.
- Law: `_profile.service` is the store series label, its value always the identity projection's app key.
- Law: `_profile.span` bands samples by workload region; a free-string profile label repeats the free-string metric-name defect.
- Law: the profile signal carries no `_metric` rows, because profiles ride their own store.
- Law: the board `profile` pack lands with the iac compile arm under `iac/operate/observe`'s `[PYROSCOPE_PANEL]` gate, never one side first.
- Law: `vitalInstance`, `vitalDelta`, `vitalSession`, and `vitalNavigation` carry one report's own accounting identity, so they ride the evidence span alone like every vital row outside the two instrument fans.
- Law: the `vitalPhase*` family decomposes a vital value into the subparts that spent it.
- Law: the vital subject rows name what a value fell on — element, resource, interaction class, load state, longest script.
- Law: the phase family measures, so its `ValueOf` clause extracts by key prefix and stamps numbers where every other row prints.
- Law: one OTLP metric name carries one descriptor unit, so the vital level family splits per UCUM code on the `bench*` precedent.
- Growth: a new metered resource, vital kind, audit axis, work-plane instrument, security facet, object receipt axis, lake-harvest lane, bench band, or capability-plane dimension lands as one row here with its consuming row on the owning signal page; a measure whose subject no `_domain` row holds lands that row first, and a new emitter under a standing subject is one `emitters` entry.
- Growth: a new fan axis is one `_rasm` row beside one `dimensions` entry on the instrument slicing it, so every metric-plane governor admits it with zero edits of its own.
- Growth: a new distribution is one row answering `bounds`; a wire form Effect gains is one `InstrumentKind` case answering `_tail` beside its own mount arm.
- Growth: a temporal row is one `_durations` entry beside its own instrument row, because the multiplier already derives from the unit column.

```typescript signature
// Branch module roster: the package exports-map subpath set verbatim, so every scope names a specifier that resolves
// — a published subpath this roster omits has no spellable scope, a row the manifest never publishes names nothing —
// while `emits` carves the instrument-mounting half as a column: iac compiles a deploy plane off convention rows and
// mints nothing, so it alone stays outside the emitting half and no domain row can name it.
const _module = {
  core: { emits: true },
  data: { emits: true },
  iac: { emits: false },
  runtime: { emits: true },
  security: { emits: true },
  ui: { emits: true },
  "ui/viewer": { emits: true },
} as const

// Branch domain roster: the second segment every rasm.<domain>.<measure> name and every domain-scoped rasm attribute
// key resolves against. Each row fixes a capability subject, so `emitters` grows while the subject holds; a package
// name promoted to a row strands every consumer joining on capability the moment a second package serves it.
const _domain = {
  audit: { emitters: ["data"], subject: "actor-attributed access decisions and the retention class each carries" },
  batch: { emitters: ["data"], subject: "resolver batching windows" },
  bench: { emitters: ["runtime"], subject: "benchmark claims and the verdicts grading them" },
  cache: { emitters: ["data"], subject: "lane cache population and hit economics" },
  chart: { emitters: ["ui"], subject: "pivot delta delivery into a live chart view" },
  crash: { emitters: ["runtime"], subject: "fatal captures and the breadcrumb replay riding them" },
  derivative: { emitters: ["data"], subject: "derivative render pressure over stored objects" },
  fact: { emitters: ["data"], subject: "journal fact drain into the queryable fact table" },
  form: { emitters: ["ui"], subject: "form submit round-trip settlement by outcome" },
  gateway: { emitters: ["core"], subject: "command-gateway dispatch by verb and outcome" },
  invoke: { emitters: ["core"], subject: "capability-plane calls crossing the interchange" },
  lane: { emitters: ["data"], subject: "serialized work-lane drain progress and occupancy" },
  meter: { emitters: ["data"], subject: "billable usage by metered resource" },
  object: { emitters: ["data"], subject: "content-addressed object writes, bytes, and reclamation" },
  olap: { emitters: ["data"], subject: "lake-engine admission and retry economics" },
  outbox: { emitters: ["data", "runtime"], subject: "undelivered durable-work rows and their age" },
  pool: { emitters: ["data"], subject: "connection-lease occupancy by scheme" },
  profile: { emitters: ["data"], subject: "harvested engine profiles and the store selectors keying them" },
  queue: { emitters: ["runtime"], subject: "durable-queue settlement and the dead set" },
  relay: { emitters: ["runtime"], subject: "relay claim settlement by channel" },
  scene: { emitters: ["ui/viewer"], subject: "scene graft admission and the refusals gating it" },
  security: { emitters: ["security"], subject: "authenticity, authorization, and key-custody decisions" },
  slo: { emitters: ["core"], subject: "objective burn and severity axes" },
  stream: { emitters: ["data"], subject: "resumable-upload finalization" },
  vital: { emitters: ["runtime"], subject: "graded web-vital observations with the phase and subject decomposition attributing each" },
  work: { emitters: ["runtime"], subject: "work-plane channel routing" },
} as const

const _rasm = {
  auditAction: "rasm.audit.action",
  auditActorKey: "rasm.audit.actor.key",
  auditActorKind: "rasm.audit.actor.kind",
  auditRetention: "rasm.audit.retention",
  auditTargetKey: "rasm.audit.target.key",
  auditTargetKind: "rasm.audit.target.kind",
  benchBand: "rasm.bench.band",
  benchLabel: "rasm.bench.label",
  benchSuite: "rasm.bench.suite",
  benchVerdict: "rasm.bench.verdict",
  cacheName: "rasm.cache.name",
  crashHop: "rasm.crash.hop",
  factStream: "rasm.fact.stream",
  formOutcome: "rasm.form.outcome",
  gatewayFrame: "rasm.gateway.frame",
  gatewayOutcome: "rasm.gateway.outcome",
  gatewayVerb: "rasm.gateway.verb",
  invokeLane: "rasm.invoke.lane",
  invokeMethod: "rasm.invoke.method",
  invokeOutcome: "rasm.invoke.outcome",
  invokeService: "rasm.invoke.service",
  laneName: "rasm.lane.name",
  meterResource: "rasm.meter.resource",
  objectOutcome: "rasm.object.outcome",
  olapEngine: "rasm.olap.engine",
  poolScheme: "rasm.pool.scheme",
  profileEngine: "rasm.profile.engine",
  ring: "rasm.ring",
  securityDialect: "rasm.security.dialect",
  securityKind: "rasm.security.kind",
  securityReason: "rasm.security.reason",
  securitySurface: "rasm.security.surface",
  sloBurn: "rasm.slo.burn",
  sloObjective: "rasm.slo.objective",
  sloSeverity: "rasm.slo.severity",
  tenant: "rasm.tenant",
  vitalDelta: "rasm.vital.delta",
  vitalElement: "rasm.vital.element",
  vitalGrade: "rasm.vital.grade",
  vitalInstance: "rasm.vital.instance",
  vitalInteraction: "rasm.vital.interaction",
  vitalKind: "rasm.vital.kind",
  vitalNavigation: "rasm.vital.navigation",
  vitalPhaseCache: "rasm.vital.phase.cache",
  vitalPhaseConnection: "rasm.vital.phase.connection",
  vitalPhaseDns: "rasm.vital.phase.dns",
  vitalPhaseElementRender: "rasm.vital.phase.element.render",
  vitalPhaseFirstByte: "rasm.vital.phase.first.byte",
  vitalPhaseInput: "rasm.vital.phase.input",
  vitalPhaseLargestShift: "rasm.vital.phase.largest.shift",
  vitalPhaseLongestScript: "rasm.vital.phase.longest.script",
  vitalPhasePaint: "rasm.vital.phase.paint",
  vitalPhasePresentation: "rasm.vital.phase.presentation",
  vitalPhaseProcessing: "rasm.vital.phase.processing",
  vitalPhaseRender: "rasm.vital.phase.render",
  vitalPhaseRequest: "rasm.vital.phase.request",
  vitalPhaseResourceLoad: "rasm.vital.phase.resource.load",
  vitalPhaseResourceStart: "rasm.vital.phase.resource.start",
  vitalPhaseScript: "rasm.vital.phase.script",
  vitalPhaseStyleAndLayout: "rasm.vital.phase.style.layout",
  vitalPhaseUnattributed: "rasm.vital.phase.unattributed",
  vitalPhaseWaiting: "rasm.vital.phase.waiting",
  vitalResource: "rasm.vital.resource",
  vitalScript: "rasm.vital.script",
  vitalScriptFunction: "rasm.vital.script.function",
  vitalScriptInvoker: "rasm.vital.script.invoker",
  vitalScriptPart: "rasm.vital.script.part",
  vitalSession: "rasm.vital.session",
  vitalState: "rasm.vital.state",
  workChannel: "rasm.work.channel",
} as const

// Identity dimensions no instrument row declares: the projection stamps them onto every series this branch mints, so
// they carve out of the domain grammar and enter the metric-plane roster once rather than on fifty-odd rows.
const _ESTATE = ["ring", "tenant"] as const

const _metric = {
  batchDuration: "rasm.batch.duration",
  benchCounter: "rasm.bench.counter",
  benchGc: "rasm.bench.gc",
  benchHeap: "rasm.bench.heap",
  benchTime: "rasm.bench.time",
  benchVerdicts: "rasm.bench.verdicts",
  cacheHits: "rasm.cache.hits",
  cacheMisses: "rasm.cache.misses",
  chartFrames: "rasm.chart.frames",
  crashCaptured: "rasm.crash.captured",
  derivativeActive: "rasm.derivative.active",
  derivativeQueued: "rasm.derivative.queued",
  factDeduped: "rasm.fact.deduped",
  factDeferred: "rasm.fact.deferred",
  factDrained: "rasm.fact.drained",
  formSubmit: "rasm.form.submit",
  gatewayCommands: "rasm.gateway.commands",
  gatewayDuration: "rasm.gateway.duration",
  httpServerDuration: METRIC_HTTP_SERVER_REQUEST_DURATION,
  invokeCalls: "rasm.invoke.calls",
  invokeDuration: "rasm.invoke.duration",
  invokeFault: "rasm.invoke.fault",
  laneCheckpoint: "rasm.lane.checkpoint",
  meterUsage: "rasm.meter.usage",
  objectReclaimed: "rasm.object.reclaimed",
  objectSize: "rasm.object.size",
  objectWritten: "rasm.object.written",
  olapDeferred: "rasm.olap.deferred",
  olapRetried: "rasm.olap.retried",
  olapWait: "rasm.olap.wait",
  outboxAge: "rasm.outbox.age",
  outboxDepth: "rasm.outbox.depth",
  outboxRedelivered: "rasm.outbox.redelivered",
  poolHeld: "rasm.pool.held",
  profileDuration: "rasm.profile.duration",
  queueDepth: "rasm.queue.depth",
  queueParked: "rasm.queue.parked",
  relayDrained: "rasm.relay.drained",
  sceneGrafts: "rasm.scene.grafts",
  sceneRefusals: "rasm.scene.refusals",
  securityJwksMiss: "rasm.security.jwks.miss",
  securityJwksQuarantined: "rasm.security.jwks.quarantined",
  securityJwksResolve: "rasm.security.jwks.resolve",
  securityKdf: "rasm.security.crypto.kdf",
  securityPolicyDeny: "rasm.security.policy.deny",
  securityRejects: "rasm.security.rejects",
  securitySecretRotation: "rasm.security.secret.rotation",
  securityShredReject: "rasm.security.shred.reject",
  streamSize: "rasm.stream.size",
  vitalDuration: "rasm.vital.duration",
  vitalObserved: "rasm.vital.observed",
  // the measure segment, never the type word: this row is the `1`-coded gauge whose suffixing tail IS `ratio`, so a
  // `rasm.vital.ratio` mint renders `rasm_vital_ratio_ratio` and the sibling rows name their own measure the same way
  vitalScore: "rasm.vital.score",
  vitalSize: "rasm.vital.size",
} as const

// UCUM codes this branch mints under — the closed unit vocabulary every instrument row names, so a mistyped code
// refuses at the row instead of landing on a backend axis, and an annotation code carries the counted thing.
const _unit = {
  byte: "By",
  decision: "{decision}",
  deliverable: "{deliverable}",
  event: "{event}",
  frame: "{frame}",
  graft: "{graft}",
  hit: "{hit}",
  key: "{key}",
  lease: "{lease}",
  milli: "ms",
  miss: "{miss}",
  nano: "ns",
  object: "{object}",
  position: "{position}",
  reject: "{reject}",
  render: "{render}",
  rotation: "{rotation}",
  second: "s",
  trip: "{trip}",
  unity: "1",
} as const

// Ordered code roster: `_unit` owns lookup by semantic name and this tuple owns order, iteration, and non-emptiness, so
// every entry reads its own `_unit` row and a consumer's schema spreads the tuple instead of re-listing the code set.
const _units = [
  _unit.byte, _unit.decision, _unit.deliverable, _unit.event, _unit.frame, _unit.graft, _unit.hit, _unit.key,
  _unit.lease, _unit.milli, _unit.miss, _unit.nano, _unit.object, _unit.position, _unit.reject, _unit.render,
  _unit.rotation, _unit.second, _unit.trip, _unit.unity,
] as const

// One millisecond expressed in each temporal code: presence here is what makes a code temporal, so every duration
// multiplier the branch spends is this table read through a row's own unit and no row can carry a factor of its own.
const _scale = { [_unit.milli]: 1, [_unit.nano]: 1_000_000, [_unit.second]: 0.001 } as const

// Prometheus base-unit words per UCUM code: a suffixing receiver appends the word as the unit tail, and an annotation
// code contributes none because the counted thing is no unit — total over `_unit`, so a new code answers its store-side
// spelling here or mints a series no board resolves.
const _promUnit = {
  [_unit.byte]: "bytes",
  [_unit.decision]: "",
  [_unit.deliverable]: "",
  [_unit.event]: "",
  [_unit.frame]: "",
  [_unit.graft]: "",
  [_unit.hit]: "",
  [_unit.key]: "",
  [_unit.lease]: "",
  [_unit.milli]: "milliseconds",
  [_unit.miss]: "",
  [_unit.nano]: "nanoseconds",
  [_unit.object]: "",
  [_unit.position]: "",
  [_unit.reject]: "",
  [_unit.render]: "",
  [_unit.rotation]: "",
  [_unit.second]: "seconds",
  [_unit.trip]: "",
  [_unit.unity]: "",
} as const

// Grafana display-unit ids per UCUM code, two columns because a panel renders a code either as the quantity it
// measures or as that quantity per second, and the two spell different ids: `level` answers an instant or a
// distribution rung, `rate` answers a windowed rate fold. Ids are the Grafana unit registry's own, so a code lands a
// word the renderer resolves rather than the UCUM code itself, which Grafana reads as an unknown id and prints bare.
const _grafanaUnit = {
  [_unit.byte]: { level: "bytes", rate: "binBps" },        // IEC on both halves: a level in 1024-scaled bytes and a rate in the matching per-second id
  [_unit.decision]: { level: "short", rate: "cps" },
  [_unit.deliverable]: { level: "short", rate: "cps" },
  [_unit.event]: { level: "short", rate: "cps" },
  [_unit.frame]: { level: "short", rate: "cps" },
  [_unit.graft]: { level: "short", rate: "cps" },
  [_unit.hit]: { level: "short", rate: "cps" },
  [_unit.key]: { level: "short", rate: "cps" },
  [_unit.lease]: { level: "short", rate: "cps" },
  // a temporal quantity per unit time is a dimensionless occupancy fraction, so the rate half leaves the time family
  [_unit.milli]: { level: "ms", rate: "percentunit" },
  [_unit.miss]: { level: "short", rate: "cps" },
  [_unit.nano]: { level: "ns", rate: "percentunit" },
  [_unit.object]: { level: "short", rate: "cps" },
  [_unit.position]: { level: "short", rate: "cps" },
  [_unit.reject]: { level: "short", rate: "cps" },
  [_unit.render]: { level: "short", rate: "cps" },
  [_unit.rotation]: { level: "short", rate: "cps" },
  [_unit.second]: { level: "s", rate: "percentunit" },
  [_unit.trip]: { level: "short", rate: "cps" },
  [_unit.unity]: { level: "short", rate: "cps" },
} as const

// Wire forms close as a tuple, because the roster is read at runtime as often as it is read as a type: the kind union
// derives from it, so one entry widens the tail table, the mount table, and every consumer's own literal schema.
const _kinds = ["counter", "frequency", "gauge", "histogram", "updown"] as const

// Type tails per kind, the dimensionless column selected by the `1` code alone: a monotonic sum takes `total` whatever
// it measures, a non-monotonic one takes none because the receiver reads the sum's own monotonicity flag, a gauge takes
// `ratio` only under that code, and every other pairing takes none — an annotation-coded gauge therefore keeps its bare
// name while a unity-coded one gains a tail, which is why the code decides the column.
const _tail = {
  counter: { dimensionless: "total", measured: "total" },
  frequency: { dimensionless: "total", measured: "total" },
  gauge: { dimensionless: "ratio", measured: "" },
  histogram: { dimensionless: "", measured: "" },
  updown: { dimensionless: "", measured: "" },
} as const

// Kind IS the wire form: a counter and a frequency sum monotonically, an updown sums both ways, a gauge levels, a
// histogram spans — so the mount reads the column instead of a site picking a constructor whose monotonicity the row
// cannot see, and the unit column alone decides duration-ness. `bounds` rides the distributions, `bigint` the counting
// rows, and `dimensions` the rows whose series fans, each column earned by the row consuming it rather than blank.
const _instrument = {
  batchDuration: { bounds: { count: 11, factor: 2, ladder: "exponential", start: 1 }, description: "resolver window wall span", kind: "histogram", name: _metric.batchDuration, unit: _unit.milli },
  benchCounter: { description: "hardware-counter band by axis", dimensions: [_rasm.benchBand, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchCounter, unit: _unit.event },
  benchGc: { description: "benchmark GC-timing band", dimensions: [_rasm.benchBand, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchGc, unit: _unit.nano },
  benchHeap: { description: "benchmark heap-delta band", dimensions: [_rasm.benchBand, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchHeap, unit: _unit.byte },
  benchTime: { description: "benchmark timing ladder by band", dimensions: [_rasm.benchBand, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchTime, unit: _unit.nano },
  benchVerdicts: { description: "regression verdicts by grade", dimensions: [_rasm.benchLabel, _rasm.benchSuite, _rasm.benchVerdict], kind: "counter", name: _metric.benchVerdicts, unit: _unit.unity },
  cacheHits: { description: "cache hits by cache name", dimensions: [_rasm.cacheName], kind: "gauge", name: _metric.cacheHits, unit: _unit.hit },
  cacheMisses: { description: "cache misses by cache name", dimensions: [_rasm.cacheName], kind: "gauge", name: _metric.cacheMisses, unit: _unit.miss },
  chartFrames: { description: "pivot delta frames delivered", kind: "counter", name: _metric.chartFrames, unit: _unit.frame },
  crashCaptured: { description: "fatal captures by fault class", dimensions: [_attr.errorType], kind: "counter", name: _metric.crashCaptured, unit: _unit.unity },
  derivativeActive: { description: "derivative renders in flight", kind: "gauge", name: _metric.derivativeActive, unit: _unit.render },
  derivativeQueued: { description: "derivative renders awaiting a worker", kind: "gauge", name: _metric.derivativeQueued, unit: _unit.render },
  // Redeliveries the content key matched, the one series proving at-least-once delivery is happening at all: an
  // implementation folding them into `factDrained` claims zero redelivery, and zero redelivery is exactly what a
  // wedged retry re-offering one window forever looks like from the drain's own series
  factDeduped: { description: "journal facts a content key matched as already landed", dimensions: [_rasm.factStream], kind: "counter", name: _metric.factDeduped, unit: _unit.unity },
  // Append attempts the durable plane refused, the only series a never-exhausting retry publishes: without it a drain
  // sitting on a dead database reads identically to a quiet one, since both drain nothing and neither fails
  factDeferred: { description: "journal append attempts the durable plane refused", kind: "counter", name: _metric.factDeferred, unit: _unit.unity },
  // Audit rows alone carry the audit half of the fan, so a meter increment lands on the stream-only series and OTel's
  // own attribute semantics make the two shapes one counter rather than a second instrument per stream
  factDrained: { description: "journal facts drained to the fact table", dimensions: [_rasm.auditAction, _rasm.auditActorKind, _rasm.factStream], kind: "counter", name: _metric.factDrained, unit: _unit.unity },
  formSubmit: { description: "settled submit trips by outcome", dimensions: [_rasm.formOutcome], kind: "counter", name: _metric.formSubmit, unit: _unit.trip },
  gatewayCommands: { description: "gateway dispatches by outcome", dimensions: [_rasm.gatewayOutcome], kind: "counter", name: _metric.gatewayCommands, unit: _unit.unity },
  gatewayDuration: { bounds: { count: 5, factor: 4, ladder: "exponential", start: 25 }, description: "gateway dispatch wall span", kind: "histogram", name: _metric.gatewayDuration, unit: _unit.milli },
  // semconv fixes this advisory ladder for http.server.request.duration in seconds, so the row transcribes edges an external contract owns rather than generating them, and the gateway connector derives the series off span keys this table already carries
  httpServerDuration: { bounds: { edges: [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10], ladder: "explicit" }, description: "server request wall span", dimensions: [_attr.httpMethod, _attr.httpRoute], kind: "histogram", name: _metric.httpServerDuration, unit: _unit.second },
  invokeCalls: { description: "capability calls by outcome", dimensions: [_rasm.invokeOutcome], kind: "counter", name: _metric.invokeCalls, unit: _unit.unity },
  invokeDuration: { bounds: { count: 5, factor: 4, ladder: "exponential", start: 25 }, description: "capability call wall span", kind: "histogram", name: _metric.invokeDuration, unit: _unit.milli },
  invokeFault: { description: "capability fault reasons", kind: "frequency", name: _metric.invokeFault, unit: _unit.unity },
  laneCheckpoint: { bigint: true, description: "last committed projection drain position", dimensions: [_rasm.laneName], kind: "gauge", name: _metric.laneCheckpoint, unit: _unit.position },
  meterUsage: { description: "billable usage by metered resource", dimensions: [_rasm.meterResource], kind: "counter", name: _metric.meterUsage, unit: _unit.unity },
  objectReclaimed: { description: "bytes reclaimed by the reference sweep", kind: "counter", name: _metric.objectReclaimed, unit: _unit.byte },
  objectSize: { description: "object bytes landed by write legs", kind: "counter", name: _metric.objectSize, unit: _unit.byte },
  objectWritten: { description: "object writes by dedup outcome", dimensions: [_rasm.objectOutcome], kind: "counter", name: _metric.objectWritten, unit: _unit.object },
  olapDeferred: { bounds: { count: 11, factor: 2, ladder: "exponential", start: 1 }, description: "deferred-query wait until execution", dimensions: [_rasm.olapEngine], kind: "histogram", name: _metric.olapDeferred, unit: _unit.milli },
  olapRetried: { description: "lake queries retried by engine", dimensions: [_rasm.olapEngine], kind: "counter", name: _metric.olapRetried, unit: _unit.unity },
  olapWait: { bounds: { count: 11, factor: 2, ladder: "exponential", start: 1 }, description: "lake-engine admission wait span", dimensions: [_rasm.olapEngine], kind: "histogram", name: _metric.olapWait, unit: _unit.milli },
  outboxAge: { description: "oldest undelivered outbox row age", kind: "gauge", name: _metric.outboxAge, unit: _unit.second },
  outboxDepth: { description: "undelivered outbox rows", kind: "gauge", name: _metric.outboxDepth, unit: _unit.deliverable },
  outboxRedelivered: { description: "undelivered rows claimed more than once", kind: "gauge", name: _metric.outboxRedelivered, unit: _unit.deliverable },
  poolHeld: { description: "pool leases held by scheme", dimensions: [_rasm.poolScheme], kind: "updown", name: _metric.poolHeld, unit: _unit.lease },
  profileDuration: { bounds: { count: 13, factor: 2, ladder: "exponential", start: 1 }, description: "harvested engine profile wall span", dimensions: [_rasm.profileEngine], kind: "histogram", name: _metric.profileDuration, unit: _unit.milli },
  queueDepth: { description: "durable-queue rows awaiting settlement", kind: "gauge", name: _metric.queueDepth, unit: _unit.deliverable },
  queueParked: { description: "deliverables parked to the dead set", dimensions: [_rasm.workChannel], kind: "counter", name: _metric.queueParked, unit: _unit.deliverable },
  relayDrained: { description: "relay claims settled by channel", dimensions: [_rasm.workChannel], kind: "counter", name: _metric.relayDrained, unit: _unit.deliverable },
  sceneGrafts: { description: "committed graft arrivals", kind: "counter", name: _metric.sceneGrafts, unit: _unit.graft },
  sceneRefusals: { description: "graft refusals by fault reason", kind: "frequency", name: _metric.sceneRefusals, unit: _unit.unity },
  securityJwksMiss: { description: "cold JWKS resolutions missing the cache", kind: "counter", name: _metric.securityJwksMiss, unit: _unit.miss },
  securityJwksQuarantined: { description: "JWKS keys quarantined by the breaker", kind: "counter", name: _metric.securityJwksQuarantined, unit: _unit.key },
  securityJwksResolve: { bounds: { edges: [5, 25, 100, 250, 1000, 5000], ladder: "explicit" }, description: "JWKS resolve wall span", kind: "histogram", name: _metric.securityJwksResolve, unit: _unit.milli },
  // argon2id cost classes cluster the interesting mass, so edges bracket each class rather than spacing evenly; a cost bump moves the objective, never these
  securityKdf: { bounds: { edges: [10, 25, 50, 100, 250, 500, 1000, 2500], ladder: "explicit" }, description: "key-derivation wall span", kind: "histogram", name: _metric.securityKdf, unit: _unit.milli },
  securityPolicyDeny: { description: "authorization denials by reason", dimensions: [_rasm.securityReason], kind: "counter", name: _metric.securityPolicyDeny, unit: _unit.decision },
  securityRejects: { description: "authenticity rejects by kind and facet", dimensions: [_rasm.securityDialect, _rasm.securityKind, _rasm.securityReason, _rasm.securitySurface], kind: "counter", name: _metric.securityRejects, unit: _unit.reject },
  securitySecretRotation: { description: "secret rotations by custody path", kind: "counter", name: _metric.securitySecretRotation, unit: _unit.rotation },
  securityShredReject: { description: "opens rejected on a shredded key", kind: "counter", name: _metric.securityShredReject, unit: _unit.reject },
  streamSize: { description: "resumable-upload bytes finalized", kind: "counter", name: _metric.streamSize, unit: _unit.byte },
  vitalDuration: { description: "current accounted time-measured vital level", dimensions: [_rasm.vitalKind], kind: "gauge", name: _metric.vitalDuration, unit: _unit.milli },
  vitalObserved: { description: "graded web-vital observations", dimensions: [_rasm.vitalGrade, _rasm.vitalKind], kind: "counter", name: _metric.vitalObserved, unit: _unit.unity },
  vitalScore: { description: "current accounted dimensionless vital level", dimensions: [_rasm.vitalKind], kind: "gauge", name: _metric.vitalScore, unit: _unit.unity },
  vitalSize: { description: "current accounted byte-measured vital level", dimensions: [_rasm.vitalKind], kind: "gauge", name: _metric.vitalSize, unit: _unit.byte },
} as const

// Ordered temporal roster: the instrument names whose UCUM code is temporal. Membership is the tuple's fact and the
// multiplier stays the unit column's, so a duration-admitting schema spreads this and no row restates a scale.
const _durations = [
  _metric.batchDuration, _metric.benchGc, _metric.benchTime, _metric.gatewayDuration, _metric.httpServerDuration,
  _metric.invokeDuration, _metric.olapDeferred, _metric.olapWait, _metric.outboxAge, _metric.profileDuration,
  _metric.securityJwksResolve, _metric.securityKdf, _metric.vitalDuration,
] as const

const _event = {
  breadcrumb: "rasm.crash.breadcrumb",
  exception: EVENT_EXCEPTION,
} as const

const _profile = {
  id: "pyroscope.profile.id",  // the span-profile correlation attribute the runtime profiling bridge stamps on long-lived scoped spans
  service: "service_name",     // the profile-store series label; its value is always the identity projection's app key
  span: "span_name",           // the profile-store label banding samples by workload region
  spanId: "span_id",           // the store's own join key back to the stamping span; the identifier half the region label cannot carry
  traceId: "trace_id",         // the store-side trace filter: every banded sample of one trace answers one query
} as const
```

## [04]-[IDENTITY_PROJECTION]

- Owner: the assembled `Convention` export — row families as properties, the ordered `keys` census beside the metric-plane roster the instrument table generates, the `identity` projection, the duration-to-instrument-unit projection, the ladder projection, the one instrument mount, the `scope` instrumentation-scope mint, and the `wire` constant set — with companion types on the merged namespace and contract guards riding the hub so a malformed row fails at this declaration with zero widening of the interior anchors.
- Law: `Convention.scope(module, version)` mints the WHOLE instrumentation-scope coordinate — the `@rasm/ts/<module>` specifier off the branch's own exports-map roster, the emitting build version, and the estate `schemaUrl` — so one value crosses `getMeter`, `getTracer`, and `getLogger` alike and the three signals cannot carry three coordinates. Both raw-OTel seams consume it whole: `runtime/otel/emit` `Hooks.meter` mounts a package's third-party instruments, and the same lane's metric-producer projection rewrites Effect's version-free `@effect/opentelemetry/Metrics` constant, while Effect's own spans, metrics, and logs ride the app `Resource` under one facade scope.
- Law: the version names the EMITTING artifact, never the consuming one — `AppIdentity.build.version` is the composing application's, so stamping it on a library scope attributes every third-party series to whoever imported the library; the composition root supplies the emitting package's own build version, which is why the coordinate takes it as data exactly as the identity projection takes an `AppIdentity`. Name-only mints forfeit the version-stamped single-coordinate row outright, and a free-string module forfeits the specifier resolving at all.
- Law: `Convention.wire` is the estate wire constant set — `namespace` pins the `service.namespace` half of the resource triple, `schemaUrl` pins one semconv schema on every `Resource` the runtime export lanes emit (`runtime/otel/emit` folds it into `resourceFromAttributes`; the path segment tracks the manifest's semconv pin and bumps with it), and `translation` selects the Prometheus OTLP-receiver strategy — `iac/operate/observe` reads `translation` into each store row's own dialect and `namespace` onto the collector's own `service.telemetry.resource`, so no wire member is spelled free-string at any store, gateway, or export seam and the three runtimes' resources and series stay byte-identical.
- Law: `_translation` closes that strategy against its four-row roster and carries what each one does — `escape` folds a dotted name onto the legacy `[a-zA-Z0-9:_]` scheme, `suffix` appends the unit word and the type tail — so one selected value fixes both behaviours and the `_Wire` guard refuses a strategy the receiver does not implement.
- Law: `Convention.translated(metric, strategy)` is the one mint-name-to-series-name projection, defaulting to the estate pin — a query plane renders through it and a store row answering a different strategy passes its own, so the suffix a receiver appends is a target property that no `_metric` row bakes and no consumer re-derives.
- Law: the Tier-0 grammar forbids a baked unit or type tail on a minted name, so the projection appends unconditionally and a name carrying its own tail renders that word twice under a suffixing receiver; `_RasmMetric` closes the dotted `rasm.<domain>.<measure>` shape against the domain roster, so the tail-free obligation rides the measure segment each row spells — a `1`-coded gauge names its measure and never `ratio`, a counter names its measure and never `total` — and is never a case this fold absorbs.
- Law: `Convention.named` keys every instrument row by its own wire name, so a consumer holding a `MetricName` reaches kind, unit, and description in one lookup and no fold scans the table for the row it already names.
- Law: `Convention.mount(metric)` is the one instrument materialization — the kind column selects the constructor and its monotonicity, `bounds` arms the distribution, `bigint` selects the value width, name and description ride the row, and the UCUM code lands as the `wire.unit` tag — so a signal site names an instrument and receives its handle.
- Law: a mount is a module-scope value, because Effect keys an instrument by name and tag set, so a mount inside a fold re-derives one registry entry per call and buys nothing.
- Law: a temporal distribution mounts over `Duration`, its input mapped through the row's own `_scale` factor, so an elapsed span records in the unit the row declares and the duration aspect composes at the site untouched.
- Law: the mapping rides the distributions alone because Effect constrains a level's and a counter's carrier to `number | bigint`, so a mapped temporal level forfeits its `Gauge` interface and the `set` aspect narrowed to it — a temporal level therefore takes `Convention.duration` at its own site, and widening the mapping to those kinds trades one call for a broken update surface.
- Law: Effect's timer constructors hardcode milliseconds and stamp their own unit tag, so a `s`-coded or `ns`-coded row minted through one exports the wrong magnitude under the wrong unit — the mapped input is why no timer constructor has a seat here.
- Law: a frequency row mounts against its publisher's closed word roster, so every admitted word reports zero before its first occurrence and a panel never reads an unseen reason as a missing series; the roster stays the publisher's because the vocabulary is its own, and `Convention.Word` closes it non-empty because an empty roster pre-registers nothing and forfeits exactly the guarantee the law claims — a publisher projecting its vocabulary through a plain key walk lands the non-empty proof at that projection, never at this mint.
- Law: `Convention.bounds(metric)` projects a distribution row's finite edge set, so an objective, a bucket-bound query, and the mounted ladder read one generated array and a `le` never names an edge no bucket carries.
- Law: an `explicit` vector arrives ASCENDING, because Effect appends its own `+Inf` and dedupes without sorting — a descending or shuffled transcription mints a distribution whose every bucket bound lies and whose projected edge set a query re-reads unchallenged; both generators rise by construction, so the obligation rides the transcribed case alone.
- Law: `Convention.kinds` publishes the wire-form roster as data, so a consumer building a literal schema over instrument families derives it here rather than re-listing the forms beside its own panel or residence table.
- Law: `Convention.units` publishes the UCUM roster the same way, so a panel feed, claim schema, or residence column decodes its unit closed rather than admitting a free string beside a kind column already decoding against `kinds`.
- Law: `Convention.grafanaUnit` publishes the display correspondence as data, so a board pane carries the UCUM code its own series declares and the deploy plane reads the renderer's word off this one table; a pane spelling a display word directly and a pane shipping the raw code both bypass the correspondence, the first forking it per author and the second rendering unitless.
- Law: `Convention.durations` publishes the temporal name roster on that same footing, so an admission proving the MEASUREMENT DOMAIN beside the instrument role decodes against it — a latency objective naming a size or ratio distribution refuses at its own schema rather than compiling a bucket bound the duration projection cannot scale.
- Law: `wire.unit` names the synthetic tag key carrying a UCUM code onto the wire, because Effect's metric constructors take no unit option; `Convention.mount` stamps it off the row's own `_instrument` column, so an owner spelling it beside a mount mints one wire fact twice.
- Law: `wire.occurrence` names the word axis both metric bridges append to a frequency point from a hardcoded spelling, so a query plane's label vocabulary and the metric-plane roster read one owned constant rather than each carrying its own free string.
- Law: `wire` homes both synthetic keys rather than `_rasm`, so neither enters a domain roster nor the `Convention.keys` census — yet their metric-plane dispositions invert: the UCUM carrier is a descriptor input a governor drops once read, while the occurrence axis IS the word census's whole fan and a roster omitting it collapses every frequency series into one undifferentiated sum no panel can split.
- Law: the namespace pin is reachable without an identity — a telemetry node holding no `AppIdentity` (the ingest gateway stamping its own self-telemetry resource) reads `Convention.wire.namespace` directly, which is why the estate value homes on the wire triple rather than inside the identity projection alone; a literal spelled at such a node forks the one value Tier-0 declares.
- Law: the record contract is two-sided by trust direction — `Convention.Attributes` is the closed stamping record whose `ValueOf` binds the two bounded-value unions, the browser-mobile boolean, the string-array rows the spec declares plural (`browser.brands`, `container.image.tags`), and the remaining string rows at their own keys — one clause per value SHAPE, so a fourth plural row extends a union rather than adding a branch; `Convention.Bag` is the open read-side record whose value type mirrors the OTel API's own `AttributeValue` shape, so foreign material admits exactly as the wire carries it. Writing through `Bag` at a signal site, or demanding `Attributes` at a scrub seam, is the inverted-trust defect; a row whose spec value is bounded gains its `ValueOf` clause in the same edit that admits the `_value` family, never later.
- Law: the read side is the foreign contract, not a convenience widening — an attribute array is homogeneous per element type and may carry empty slots, so a mirror admitting mixed arrays passes material no exporter can encode while one refusing empty slots rejects material every collector forwards; both errors surface as an admission that silently disagrees with the wire, which is why the shape is transcribed from the frozen API line rather than approximated. Empty-slot arms belong to that mirror alone and carry no domain licence: nothing this branch decodes travels as `Value`, and interior absence stays `Option` at its owning schema.
- Law: `Convention.keys` is the one iteration anchor — the ordered census over every attribute row (`_attr`, `_incubating`, `_rasm` in table order), so a render fold probing a closed record walks the vocabulary and emits pairs in canonical order; an `Object.keys` walk over a stamped record re-imports per-record insertion order and is the deleted spelling. `_Census` closes the three tier axes and the profile row pairwise disjoint, because the promotion move this page legislates lands a stable row and retires an incubating one in two edits: half-landed, the census carries one key twice and every ordered render emits its pair twice from a record holding it once.
- Law: `Convention.dimensions` is the metric-plane roster the instrument table generates — each row's own `dimensions`, the estate pair the identity projection stamps, and the one axis the bridge appends — so a governor allow-lists exactly what some instrument mints and a deployment policy row widens it for foreign keys alone. Folding the ATTRIBUTE vocabulary instead admits every identifier-grade key the census carries for span and log use, which turns the ceiling below into the only cardinality guard and prices a burst as silently dropped series; folding a governor's own mounted rows instead strips every plane it does not mount.
- Law: `Convention.duration(metric, span)` is the one duration conversion, reading `_scale` through the named row's own unit code so no multiplier is ever declared twice, and `_durations` is the roster whose guard closes membership both directions against the unit column's own answer: a temporal row the tuple omits strands the objective naming it, and an excess entry widens `DurationMetric` and hands a byte histogram a millisecond scale. Size and ratio distributions therefore reach no multiplier at all, so a consumer assuming every distribution measures time refuses at its own call site rather than rendering a bucket bound in the wrong unit; SLO and rule compilers read this projection and never re-derive a scale.
- Law: `Convention.identity` is the one `AppIdentity -> Identity` correspondence — `service.name` carries the app key, `service.namespace` the estate pin an identity's own service group overrides, `service.version` the build version, `service.instance.id` the boot-minted instance guid, `deployment.environment.name` the environment tier, `host.name` the host print through the incubating alias row, and `rasm.ring` the exposure rung, while `cloud.region`, `cloud.availability_zone`, `k8s.cluster.name`, and `rasm.tenant` stamp only when the identity pins the dimension: a multi-tenant process emits no resource-level tenant and per-fact tenancy rides the audit/meter streams — and every identity-bearing surface (the runtime OTLP `Resource`, the fact stamps on the runtime signal streams, the dashboard identity on `board#MODEL`) derives from this one projection, so a per-app telemetry fork is structurally impossible.
- Law: a dimension is projected only once its value settles — the projection line lands in the same edit that adds the identity field, never ahead of it — so the projection reads settled `instance`/`environment`/`ring`/`region`/`zone`/`cluster` values and never re-mints a dimension the floor owns; an absent `Option` dimension is omission, never a sentinel, so a backend filter never matches an empty string. `service.namespace` is the one always-present fold: the estate value settles on the wire triple, so the identity's `Option` reads as an override rather than as presence.
- Law: `Convention.profiled(identity)` is the one profile-store selector projection — the `service_name` label bound to the identity's app key — so the board `profile` pack and the runtime pyroscope seat derive one series identity and a hand-spelled profile label never exists; `ProfileLabel` spans the store-label keys alone (`service_name`, `span_name`, `span_id`, `trace_id`) — `_profile.id` is the span-correlation attribute a label-only consumer never accepts — while the `id` row rides `Convention.keys`, so a stamped record's census walk emits it in canonical order.
- Entry: `Convention.mount(metric, ...words)`, `Convention.bounds(metric)`, `Convention.identity(identity)`, `Convention.profiled(identity)`, `Convention.duration(metric, span)`, `Convention.translated(metric, strategy)`, and `Convention.scope(module, version)` — the operation set; row families read as properties (`Convention.attr.httpRoute`, `Convention.incubating.flagKey`, `Convention.domain.olap`, `Convention.metric.meterUsage`, `Convention.instrument.meterUsage`, `Convention.unit.milli`, `Convention.grafanaUnit[code].level`, `Convention.named[name]`, `Convention.event.exception`, `Convention.value.flagTargeting`, `Convention.profile.id`, `Convention.wire.schemaUrl`, `Convention.translation.NoTranslation`); the `Convention.keys`, `Convention.dimensions`, `Convention.kinds`, `Convention.units`, and `Convention.durations` rosters at every iteration, governor, and schema seam. `_module` and `_ESTATE` stay interior — it names scopes and closes emitter columns, and a consumer walking the branch's own subpaths reads the exports map.
- Growth: a new identity dimension is one projection line with its `_rasm` row; a new bounded-value binding is one `ValueOf` clause beside its prefixed `_value` rows; a new sub-domain is one `_module` row carrying whether it mounts instruments, and every scope name, emitter column, and `_domain` row it earns follows from that one entry.
- Growth: a new UCUM code is one `_unit` row answering `_promUnit` and `_grafanaUnit` and one `_units` entry; a receiver strategy the ecosystem adds is one `_translation` row every store dialect then spells.
- Growth: a bounded semconv key a `rasm.*` instrument slices on is one `dimensions` entry on that instrument, so every metric-plane governor admits it with zero edits of its own.
- Growth: a ladder family the bucket algebra gains is one `Bounds` case answering `_LADDER`; a wire form Effect gains is one `InstrumentKind` case answering `_MOUNT` beside its `_tail` row.
- Boundary: `AppIdentity` is `value/identity`'s value — this page projects it into attribute space and owns nothing about its construction; `value/fault` owns the forensic anchors whole, importing `ATTR_EXCEPTION_*` and the `code.*` frame quartet straight from the spec surface, so the `exception.*` keys carry no row here while `error.type` does — the fatal capture's bounded class dimension is a signal this page's consumers query, the free-form exception fields are forensics only the fault floor writes. Two owners share one spec vocabulary across that import boundary, never a re-export hop.

```typescript signature
// Prometheus OTLP-receiver strategies, each row carrying what it does rather than naming it: `escape` folds a dotted
// name onto the legacy identifier scheme and `suffix` appends the unit word and the type tail, so a store row selecting
// a strategy selects both behaviours and a hand-spelled fifth posture has no seat.
const _translation = {
  NoTranslation: { escape: false, suffix: false },
  NoUTF8EscapingWithSuffixes: { escape: false, suffix: true },
  UnderscoreEscapingWithoutSuffixes: { escape: true, suffix: false },
  UnderscoreEscapingWithSuffixes: { escape: true, suffix: true },
} as const

const _wire = {
  namespace: "rasm",                                   // the estate resource-triple pin; a node with no AppIdentity reads it here rather than spelling a second literal
  occurrence: "key", // the word axis both metric bridges append to a frequency point, hardcoded there: the one exported dimension no vocabulary row mints
  schemaUrl: "https://opentelemetry.io/schemas/1.43.0", // path segment tracks the manifest semconv pin; bump together
  translation: "NoUTF8EscapingWithSuffixes",
  unit: "unit", // the synthetic UCUM carrier: Effect's constructors take no unit option, so the OTLP bridge reads the descriptor unit off this tag key
} as const

// KEY REMAP: `_instrument` re-keyed by its own `name` column, so the mapped type proves the index total over
// `MetricName` and one construction cast is the only place the two keyings meet.
const _named = Record.fromEntries(Array.map(Record.values(_instrument), (row) => [row.name, row] as const)) as Convention.Named

const _translated = (metric: Convention.MetricName, strategy: Convention.Translation = _wire.translation): string => {
  const row = _named[metric]
  const policy = _translation[strategy]
  const name = policy.escape ? metric.replaceAll(/[^a-zA-Z0-9:_]/g, "_") : metric
  return policy.suffix
    ? Array.join(
      Array.filter([name, _promUnit[row.unit], _tail[row.kind][row.unit === _unit.unity ? "dimensionless" : "measured"]], (part) => part.length > 0),
      "_",
    )
    : name
}

// `_scale`'s own key set IS the temporal vocabulary, so the refinement reads the table the unit law already names
// instead of re-listing the codes a second time and drifting on the next temporal code admitted.
const _timed = (unit: Convention.Unit): unit is Convention.TimeUnit => unit in _scale

const _duration = (metric: Convention.DurationMetric, span: Duration.Duration): number =>
  Duration.toMillis(span) * _scale[_named[metric].unit]

// One arm per ladder family, generating rather than delegating: `MetricBoundaries.linear`/`.exponential` spend one of
// their own `count` slots on the `+Inf` bucket `fromIterable` appends, so a row's `count` counts FINITE edges alone and
// a generated rung set lands in the same shape a transcribed one does.
const _LADDER: { readonly [L in Convention.Ladder]: (bounds: Extract<Convention.Bounds, { readonly ladder: L }>) => ReadonlyArray<number> } = {
  explicit: (bounds) => bounds.edges,
  exponential: (bounds) => Array.makeBy(bounds.count, (rung) => bounds.start * bounds.factor ** rung),
  linear: (bounds) => Array.makeBy(bounds.count, (rung) => bounds.start + rung * bounds.width),
}

const _edges = (metric: Convention.MetricName<"histogram">): ReadonlyArray<number> =>
  // BOUNDARY ADAPTER: a keyed dispatch erases the case-to-arm correlation the mapped table declares, so one cast rejoins each
  // arm to the case its own discriminant selected
  (_LADDER[_named[metric].bounds.ladder] as (bounds: Convention.Bounds) => ReadonlyArray<number>)(_named[metric].bounds)

// Effect correlates the value width through a LITERAL `bigint` option, so the row's own column resolves the overload at each
// call while one arm carries whichever monotonicity the kind selected.
const _counted = (row: Extract<Convention.Row, { readonly kind: "counter" | "updown" }>, incremental: boolean): Convention.Instrument =>
  row.bigint === true
    ? Metric.counter(row.name, { bigint: true, description: row.description, incremental })
    : Metric.counter(row.name, { description: row.description, incremental })

// Temporal codes take the elapsed span and scale it into the row's OWN unit, so `Metric.trackDuration` composes at each
// site untouched and no converter aspect enters; every other code takes the number the site already holds.
const _spanned = (row: Extract<Convention.Row, { readonly kind: "histogram" }>): Convention.Instrument => {
  const base = Metric.histogram(row.name, MetricBoundaries.fromIterable(_edges(row.name)), row.description)
  return Option.match(Option.liftPredicate(row.unit, _timed), {
    onNone: () => base,
    onSome: (unit) => Metric.mapInput(base, (span: Duration.Duration) => Duration.toMillis(span) * _scale[unit]),
  })
}

const _MOUNT: { readonly [K in Convention.InstrumentKind]: (row: Extract<Convention.Row, { readonly kind: K }>, words: ReadonlyArray<string>) => Convention.Instrument } = {
  counter: (row) => _counted(row, true),
  frequency: (row, words) => Metric.frequency(row.name, { description: row.description, preregisteredWords: words }),
  gauge: (row) =>
    row.bigint === true
      ? Metric.gauge(row.name, { bigint: true, description: row.description })
      : Metric.gauge(row.name, { description: row.description }),
  histogram: _spanned,
  updown: (row) => _counted(row, false),
}

const _mount = <N extends Convention.MetricName>(metric: N, ...words: Convention.Words<N>): Convention.Mounted<N> =>
  // BOUNDARY ADAPTER: the keyed dispatch erases the row-to-arm correlation the mapped table declares and `Mounted`
  // re-derives the carrier from the same columns the arm read; UCUM tagging lands here because the OTLP bridge computes
  // that exported descriptor unit before any view runs
  Metric.tagged(
    (_MOUNT[_named[metric].kind] as (row: Convention.Row, words: ReadonlyArray<string>) => Convention.Instrument)(
      _named[metric],
      words[0] ?? [],
    ),
    _wire.unit,
    _named[metric].unit,
  ) as Convention.Mounted<N>

const _keys: ReadonlyArray<Convention.Key> = [
  ...Record.values(_attr),
  ...Record.values(_incubating),
  ...Record.values(_rasm),
  _profile.id,
]

// Instrument rows own which keys reach a metric point, so the governor's roster folds each row's own `dimensions`
// beside the estate pair the identity projection stamps and the one axis the bridge appends — identifier-grade keys the
// census carries for span or log use widen no allow-list, and a fan no row declares never survives one.
const _dimensions: ReadonlyArray<Convention.Dimension> = Array.dedupe([
  ...Array.flatMap(Record.values(_instrument), (row) => row.dimensions ?? []),
  ...Array.map(_ESTATE, (dimension) => _rasm[dimension]),
  _wire.occurrence,
])

const _identity = (identity: AppIdentity): Convention.Identity => ({
  [_attr.deploymentEnvironment]: identity.environment,
  [_attr.serviceInstance]: identity.instance,
  [_attr.serviceName]: identity.app,
  [_attr.serviceNamespace]: Option.getOrElse(identity.namespace, () => _wire.namespace), // the estate pin is the floor; a fleet's own service group overrides it
  [_attr.serviceVersion]: identity.build.version,
  [_incubating.hostName]: identity.host,
  [_rasm.ring]: identity.ring,
  ...Option.match(identity.region, {
    onNone: () => ({}),
    onSome: (region) => ({ [_incubating.cloudRegion]: region }),
  }),
  ...Option.match(identity.zone, {
    onNone: () => ({}),
    onSome: (zone) => ({ [_incubating.cloudZone]: zone }),
  }),
  ...Option.match(identity.cluster, {
    onNone: () => ({}),
    onSome: (cluster) => ({ [_attr.k8sCluster]: cluster }),
  }),
  ...Option.match(identity.tenant, {
    onNone: () => ({}),
    onSome: (tenant) => ({ [_rasm.tenant]: tenant }),
  }),
})

declare namespace Convention {
  type Attr = keyof typeof _attr
  type Domain = keyof typeof _domain
  type Module = keyof typeof _module
  type Emitter = { readonly [M in Module]: (typeof _module)[M]["emits"] extends true ? M : never }[Module] // the roster's own column carves the emitting half; a second hand-listed union drifts on the first module that starts or stops mounting
  type EstateDimension = (typeof _ESTATE)[number] // the identity dimensions the resource projection stamps: the carve on the domain grammar
  type EstateKey = (typeof _rasm)[EstateDimension] // their wire spellings, which no instrument row may claim as its own fan
  type AttrKey = (typeof _attr)[Attr]
  type IncubatingKey = (typeof _incubating)[keyof typeof _incubating]
  type RasmKey = (typeof _rasm)[keyof typeof _rasm]
  type Key = AttrKey | IncubatingKey | RasmKey | (typeof _profile)["id"]
  type Dimension = Key | Wire["occurrence"] // the metric-plane roster: vocabulary rows beside the one exported axis the bridge appends and no row mints
  type ProfileLabel = (typeof _profile)[Exclude<keyof typeof _profile, "id">] // store labels only: _profile.id is the span-correlation attribute, never a series label
  type ConnectionType = (typeof _value)[Extract<keyof typeof _value, `connection${string}`>]
  type FlagReason = (typeof _value)[Extract<keyof typeof _value, `flag${string}`>] // template extraction over the family prefix, never the whole table: each bounded family projects under one unchanged clause
  type InstrumentKind = (typeof _kinds)[number] // the closed wire forms, derived from their own roster so the type and the runtime tuple can never disagree
  type Unit = (typeof _unit)[keyof typeof _unit]
  type Units = typeof _units // the ordered tuple, so a spread into `Schema.Literal` holds its non-empty overload and keeps the exact literal set
  type TimeUnit = keyof typeof _scale
  type Ladder = Bounds["ladder"]
  // Bucket ladders: two generators over three scalars each, and one transcription for edges an external contract fixes
  type Bounds =
    | { readonly count: number; readonly factor: number; readonly ladder: "exponential"; readonly start: number }
    | { readonly count: number; readonly ladder: "linear"; readonly start: number; readonly width: number }
    | { readonly edges: readonly [number, ...ReadonlyArray<number>]; readonly ladder: "explicit" }
  type MetricName<K extends InstrumentKind = InstrumentKind> = {
    readonly [N in keyof typeof _instrument]: (typeof _instrument)[N]["kind"] extends K ? (typeof _instrument)[N]["name"] : never
  }[keyof typeof _instrument]
  type Named = { readonly [N in keyof typeof _instrument as (typeof _instrument)[N]["name"]]: (typeof _instrument)[N] } // the row table re-keyed by its own wire name
  type Row<N extends MetricName = MetricName> = Named[N]
  // Rows materialize into one carrier: kind picks the family, unit picks a distribution's input, and width picks the
  // counting families' value — so the declaration alone types every mounted handle.
  type Mounted<N extends MetricName, R = Named[N]> = R extends { readonly kind: "frequency" } ? Metric.Metric.Frequency<string>
    : R extends { readonly kind: "histogram" } ? Metric.Metric.Histogram<R extends { readonly unit: TimeUnit } ? Duration.Duration : number>
    : R extends { readonly kind: "gauge" } ? Metric.Metric.Gauge<R extends { readonly bigint: true } ? bigint : number>
    : Metric.Metric.Counter<R extends { readonly bigint: true } ? bigint : number>
  type Instrument = Mounted<MetricName> // every carrier the census admits, which is exactly what one mount arm returns
  type Word = readonly [string, ...ReadonlyArray<string>] // an empty roster forfeits the zero-before-first-occurrence guarantee the frequency law claims, so the tuple refuses it at the mint
  type Words<N extends MetricName> = Named[N] extends { readonly kind: "frequency" } ? [words: Word] : [] // the roster is required where the family counts words and unspellable everywhere else
  type GrafanaUnit = typeof _grafanaUnit
  type Display = keyof GrafanaUnit[Unit] // the fold's own read: a quantity or that quantity per second
  type Durations = typeof _durations // the ordered tuple, so a spread into `Schema.Literal` holds its non-empty overload and keeps the exact temporal name set
  type DurationMetric = Durations[number]
  type EventName = (typeof _event)[keyof typeof _event]
  type Scalar = string | number | boolean
  // Read-side values mirror the frozen `@opentelemetry/api` 1.x `AttributeValue` shape exactly — arrays stay
  // homogeneous per element type and lawfully carry empty slots — so foreign material never fails admission and a
  // mixed array, which no exporter can encode, never passes it.
  type Value =
    | Scalar
    | ReadonlyArray<string | null | undefined>
    | ReadonlyArray<number | null | undefined>
    | ReadonlyArray<boolean | null | undefined>
  type ValueOf<K extends Key> = K extends (typeof _incubating)["connectionType"] ? ConnectionType
    : K extends (typeof _incubating)["flagReason"] ? FlagReason
    : K extends (typeof _incubating)["browserMobile"] ? boolean
    : K extends (typeof _attr)["containerImageTags"] | (typeof _incubating)["browserBrands"] ? ReadonlyArray<string>
    // Phase rows measure, so this clause extracts them by key prefix exactly as a bounded value family extracts its own,
    // and the two whole-report quantities name themselves, so a causal decomposition stamps as numbers rather than printed text
    : K extends `rasm.vital.phase.${string}` | (typeof _rasm)["vitalDelta" | "vitalSession"] ? number
    : string
  type Attributes = { readonly [K in Key]?: ValueOf<K> }
  type Bag = { readonly [key: string]: Value }
  type Identity = Types.Simplify<
    & { readonly [K in (typeof _attr)["deploymentEnvironment" | "serviceInstance" | "serviceName" | "serviceNamespace" | "serviceVersion"] | (typeof _incubating)["hostName"] | (typeof _rasm)["ring"]]: string }
    & { readonly [K in (typeof _attr)["k8sCluster"] | (typeof _incubating)["cloudRegion" | "cloudZone"] | (typeof _rasm)["tenant"]]?: string }
  >
  type Scope = { readonly name: `@rasm/ts/${Module}`; readonly schemaUrl: Wire["schemaUrl"]; readonly version: AppIdentity.Version } // the whole InstrumentationScope triple: name, version, and one schema coordinate travel as one value
  type Translation = keyof typeof _translation
  type Wire = typeof _wire
  type Shape = Types.Simplify<{
    readonly attr: typeof _attr
    readonly bounds: (metric: MetricName<"histogram">) => ReadonlyArray<number>
    readonly dimensions: ReadonlyArray<Dimension>
    readonly domain: typeof _domain
    readonly duration: (metric: DurationMetric, span: Duration.Duration) => number
    readonly durations: Durations
    readonly event: typeof _event
    readonly grafanaUnit: GrafanaUnit
    readonly identity: (identity: AppIdentity) => Identity
    readonly incubating: typeof _incubating
    readonly instrument: typeof _instrument
    readonly keys: ReadonlyArray<Key>
    readonly kinds: typeof _kinds
    readonly metric: typeof _metric
    readonly mount: <N extends MetricName>(metric: N, ...words: Words<N>) => Mounted<N>
    readonly named: Named
    readonly profile: typeof _profile
    readonly profiled: (identity: AppIdentity) => { readonly [K in (typeof _profile)["service"]]: string }
    readonly rasm: typeof _rasm
    readonly scope: (module: Module, version: AppIdentity.Version) => Scope
    readonly translated: (metric: MetricName, strategy?: Translation) => string
    readonly translation: typeof _translation
    readonly unit: typeof _unit
    readonly units: Units
    readonly value: typeof _value
    readonly wire: Wire
  }>
  type _Attr<T extends Record<Attr, string> = typeof _attr> = T
  type _Census<
    Overlap extends never =
      | Extract<AttrKey, IncubatingKey | RasmKey | (typeof _profile)["id"]>
      | Extract<IncubatingKey, RasmKey | (typeof _profile)["id"]>
      | Extract<RasmKey, (typeof _profile)["id"]>,
  > = Overlap // pairwise key disjointness: a promotion landing the stable row without retiring its incubating twin doubles a `keys` entry and forks every census-ordered render
  type _Domain<T extends Record<Domain, { readonly emitters: readonly [Emitter, ...ReadonlyArray<Emitter>]; readonly subject: string }> = typeof _domain> = T // a subject with no emitter is a segment nothing mints
  type _Event<T extends Record<Exclude<keyof typeof _event, "exception">, `rasm.${Domain}.${string}`> = typeof _event> = T // the semconv event keeps its spec spelling; every Rasm event name closes against the domain roster
  // Each column is obligated by the row's OWN kind, read back off the table: a distribution without its ladder, a
  // distribution or word census claiming a value width, and a counting row carrying edges each refuse right here. A
  // word census names no fan either, because its one axis is the kind's own and the bridge appends it unasked.
  type _InstrumentRows<
    T extends {
      readonly [K in keyof typeof _metric]:
        & { readonly description: string; readonly kind: InstrumentKind; readonly name: (typeof _metric)[K]; readonly unit: Unit }
        & ((typeof _instrument)[K]["kind"] extends "histogram" ? { readonly bounds: Bounds } : { readonly bounds?: never })
        & ((typeof _instrument)[K]["kind"] extends "frequency" | "histogram" ? { readonly bigint?: never } : { readonly bigint?: true })
        & ((typeof _instrument)[K]["kind"] extends "frequency" ? { readonly dimensions?: never }
          : { readonly dimensions?: readonly [Exclude<Key, EstateKey>, ...ReadonlyArray<Exclude<Key, EstateKey>>] })
    } = typeof _instrument,
  > = T
  type _Named<T extends Record<MetricName, unknown> = Named> = T // the key remap covers every name: an unreachable row would leave the translation projection partial
  type _PromUnits<T extends Record<Unit, string> = typeof _promUnit> = T // totality over the unit vocabulary: an unanswered code renames its own series at the receiver
  type _GrafanaUnits<T extends Record<Unit, { readonly level: string; readonly rate: string }> = typeof _grafanaUnit> = T // totality over the same vocabulary: an unanswered code renders its panel unitless
  type _Units<Missing extends never = Exclude<Unit, Units[number]>> = Missing // roster totality: a code the tuple omits reaches every consuming schema as an inadmissible value
  type _Tails<T extends Record<InstrumentKind, { readonly dimensionless: string; readonly measured: string }> = typeof _tail> = T
  type _Temporal = { readonly [K in keyof typeof _instrument]: (typeof _instrument)[K]["unit"] extends TimeUnit ? (typeof _instrument)[K]["name"] : never }[keyof typeof _instrument]
  // Roster totality both directions against the unit column's own answer: a temporal row the tuple omits strands the
  // objective naming it, and an excess entry widens `DurationMetric` and hands a byte histogram a millisecond scale.
  type _Durations<Missing extends never = Exclude<_Temporal, DurationMetric>, Excess extends never = Exclude<DurationMetric, _Temporal>> = [Missing, Excess]
  type _Module<T extends Record<Module, { readonly emits: boolean }> = typeof _module> = T
  type _Rasm<
    T extends
      & Record<Exclude<keyof typeof _rasm, EstateDimension>, `rasm.${Domain}.${string}`>
      & Record<EstateDimension, `rasm.${string}`> = typeof _rasm,
  > = T
  type _RasmMetric<T extends Record<Exclude<keyof typeof _metric, "httpServerDuration">, `rasm.${Domain}.${string}`> = typeof _metric> = T // the dotted rasm.<domain>.<measure> law, closed against the domain roster
  type _Value<K extends `connection${string}` | `flag${string}` = keyof typeof _value> = K // family closure: a bounded row outside every prefix binds no ValueOf clause and reaches its signal site as a free string
  type _Wire<
    T extends { readonly namespace: string; readonly schemaUrl: `https://opentelemetry.io/schemas/${string}`; readonly translation: Translation; readonly unit: string } = typeof _wire,
  > = T // the strategy closes against its roster: a receiver posture no `_translation` row implements refuses here, never at the store's own values file
}

const Convention: Convention.Shape = {
  attr: _attr,
  bounds: _edges,
  dimensions: _dimensions,
  domain: _domain,
  duration: _duration,
  durations: _durations,
  event: _event,
  grafanaUnit: _grafanaUnit,
  identity: _identity,
  incubating: _incubating,
  instrument: _instrument,
  keys: _keys,
  kinds: _kinds,
  metric: _metric,
  mount: _mount,
  named: _named,
  profile: _profile,
  profiled: (identity) => ({ [_profile.service]: identity.app }),
  rasm: _rasm,
  scope: (module, version) => ({ name: `@rasm/ts/${module}`, schemaUrl: _wire.schemaUrl, version }),
  translated: _translated,
  translation: _translation,
  unit: _unit,
  units: _units,
  value: _value,
  wire: _wire,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Convention }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
