# [CORE_CONVENTION]

Conformance rides as rows: dotted `rasm.<domain>.<measure>` names under UCUM codes closing against `_domain`, `Convention.scope` the scope spelling, `Convention.wire` the estate pins. `Convention.translated` projects a store's series name, so suffixing is a target property; `Convention.mount` materializes a row into its handle, so a site names an instrument. Closed rosters — `keys`, `kinds`, `units`, `durations` — publish as data, `dimensions` generates the metric-plane roster off each row's own fan, and C# parity stays name-level. Its module is `core/src/observe/convention.ts`.

## [01]-[INDEX]

- [02]-[SEMCONV_ROWS]: frozen and incubating attribute rosters beside their bounded value families; `Convention`.
- [03]-[RASM_ROWS]: module, domain, dimension, metric, unit, instrument, event, and profile tables; `Convention`.
- [04]-[IDENTITY_PROJECTION]: store translation, wire pins, mount memo, outcome fold, and resource stamping; `Convention`.

## [02]-[SEMCONV_ROWS]

- Owner: `_attr`, `_incubating`, and `_value` name every OpenTelemetry key and bounded value the branch spells, so a producer reads one roster and a collector's enrichment keys stay recoverable from it.
- Law: stability decides the table, never the concern — a key the pinned release ships stable rides `_attr` and an incubating key rides `_incubating`, so a promotion is a row moving between two tables and the census guard refuses the moment a promoted key keeps its incubating twin.
- Law: announcement identity crosses on the specification's own five `cloudevents.*` keys, so a span joins the fact producing it through the same `(source, id)` composite every branch dedups on rather than a private correlation key.
- Law: `messaging.system` is an OPEN enum and the pinned release generates a value for Kafka alone, so the branch's NATS and MQTT bindings spell their own system name at the emitting owner and no row here fabricates a generated constant that does not exist.
- Law: messaging span names derive from the bounded five-word operation vocabulary, so a lane never coins a sixth word for a step one of those already names.
- Law: bounded value families project by template extraction over their own prefix, so each family closes under one unchanged clause and a row outside every prefix binds no projection and reaches its signal site as a free string.
- Growth: a key is one row on the table its stability names; a bounded value family is one prefix with its extraction clause.
- Boundary: which producer stamps which key is the emitting folder's; this cluster owns the spellings alone.
- Packages: `@opentelemetry/semantic-conventions` (stable and `incubating` entrypoints).

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
  ATTR_CLOUDEVENTS_EVENT_ID,
  ATTR_CLOUDEVENTS_EVENT_SOURCE,
  ATTR_CLOUDEVENTS_EVENT_SPEC_VERSION,
  ATTR_CLOUDEVENTS_EVENT_SUBJECT,
  ATTR_CLOUDEVENTS_EVENT_TYPE,
  ATTR_DEVICE_MODEL_IDENTIFIER,
  ATTR_FEATURE_FLAG_CONTEXT_ID,
  ATTR_FEATURE_FLAG_KEY,
  ATTR_FEATURE_FLAG_PROVIDER_NAME,
  ATTR_FEATURE_FLAG_RESULT_REASON,
  ATTR_FEATURE_FLAG_RESULT_VARIANT,
  ATTR_HOST_NAME,
  ATTR_MESSAGING_BATCH_MESSAGE_COUNT,
  ATTR_MESSAGING_CONSUMER_GROUP_NAME,
  ATTR_MESSAGING_DESTINATION_NAME,
  ATTR_MESSAGING_DESTINATION_PARTITION_ID,
  ATTR_MESSAGING_MESSAGE_BODY_SIZE,
  ATTR_MESSAGING_MESSAGE_ID,
  ATTR_MESSAGING_OPERATION_NAME,
  ATTR_MESSAGING_OPERATION_TYPE,
  ATTR_MESSAGING_SYSTEM,
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
  MESSAGING_OPERATION_TYPE_VALUE_CREATE,
  MESSAGING_OPERATION_TYPE_VALUE_PROCESS,
  MESSAGING_OPERATION_TYPE_VALUE_PUBLISH,
  MESSAGING_OPERATION_TYPE_VALUE_RECEIVE,
  MESSAGING_OPERATION_TYPE_VALUE_SETTLE,
  MESSAGING_SYSTEM_VALUE_KAFKA,
  NETWORK_CONNECTION_TYPE_VALUE_CELL,
  NETWORK_CONNECTION_TYPE_VALUE_UNAVAILABLE,
  NETWORK_CONNECTION_TYPE_VALUE_UNKNOWN,
  NETWORK_CONNECTION_TYPE_VALUE_WIFI,
  NETWORK_CONNECTION_TYPE_VALUE_WIRED,
} from "@opentelemetry/semantic-conventions/incubating" // feature_flag.result.* supersedes the deprecated feature_flag.evaluation.* family: the alias row absorbs the next move
import { Array, Cause, Duration, Effect, Exit, Metric, MetricBoundaries, MutableHashMap, Option, Record, type Types } from "effect"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"

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
  // Announcement identity rides the specification's own five: a board joins a span to the fact producing it by
  // `cloudevents.event_id` beside `cloudevents.event_source`, which IS the uniqueness composite every branch dedups
  // on, so a subscription and a trace read one coordinate rather than each stamping a private correlation key.
  cloudeventsId: ATTR_CLOUDEVENTS_EVENT_ID,
  cloudeventsSource: ATTR_CLOUDEVENTS_EVENT_SOURCE,
  cloudeventsSpecVersion: ATTR_CLOUDEVENTS_EVENT_SPEC_VERSION,
  cloudeventsSubject: ATTR_CLOUDEVENTS_EVENT_SUBJECT,
  cloudeventsType: ATTR_CLOUDEVENTS_EVENT_TYPE,
  cloudRegion: ATTR_CLOUD_REGION,
  cloudZone: ATTR_CLOUD_AVAILABILITY_ZONE,
  connectionType: ATTR_NETWORK_CONNECTION_TYPE,
  deviceModel: ATTR_DEVICE_MODEL_IDENTIFIER, // the UA client hints expose a model alone: device.manufacturer has no browser source to stamp
  // Tracked outcomes name a business event and carry no flag key, so `feature_flag.context.id` — the targeting
  // identity both planes stamp — IS the join back to the evaluations that produced it, and `feature_flag.result.variant`
  // is the arm the outcome attributes to. Without the pair a tracking event lands as an unattributable count.
  flagContext: ATTR_FEATURE_FLAG_CONTEXT_ID,
  flagKey: ATTR_FEATURE_FLAG_KEY,
  flagProvider: ATTR_FEATURE_FLAG_PROVIDER_NAME,
  flagReason: ATTR_FEATURE_FLAG_RESULT_REASON,
  flagVariant: ATTR_FEATURE_FLAG_RESULT_VARIANT,
  hostName: ATTR_HOST_NAME,
  // Transport coordinates a binding row already decides, so a producer and a consumer stamp one vocabulary: the
  // destination a row routes on, the partition an ordering key selected, the group a durable consumer holds, and
  // whatever batch arity a frame settled per member. `messaging.system` is an OPEN enum whose generated value names
  // Kafka and not the branch's NATS or MQTT bindings, so those two spell their system name at the emitting owner.
  messagingBatchCount: ATTR_MESSAGING_BATCH_MESSAGE_COUNT,
  messagingBodySize: ATTR_MESSAGING_MESSAGE_BODY_SIZE,
  messagingConsumerGroup: ATTR_MESSAGING_CONSUMER_GROUP_NAME,
  messagingDestination: ATTR_MESSAGING_DESTINATION_NAME,
  messagingMessageId: ATTR_MESSAGING_MESSAGE_ID,
  messagingOperation: ATTR_MESSAGING_OPERATION_NAME,
  messagingOperationType: ATTR_MESSAGING_OPERATION_TYPE,
  messagingPartition: ATTR_MESSAGING_DESTINATION_PARTITION_ID,
  messagingSystem: ATTR_MESSAGING_SYSTEM,
  sessionId: ATTR_SESSION_ID,
  sessionPrevious: ATTR_SESSION_PREVIOUS_ID,
} as const

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
  // Messaging span names derive from this bounded operation vocabulary: `create` mints the announcement, `publish`
  // hands it to a transport, `receive` pulls it, `process` runs the handler, and `settle` acknowledges — five words
  // spanning the whole fabric, so a lane never invents a sixth to describe a step one of these already names.
  messagingCreate: MESSAGING_OPERATION_TYPE_VALUE_CREATE,
  messagingKafka: MESSAGING_SYSTEM_VALUE_KAFKA,
  messagingProcess: MESSAGING_OPERATION_TYPE_VALUE_PROCESS,
  messagingPublish: MESSAGING_OPERATION_TYPE_VALUE_PUBLISH,
  messagingReceive: MESSAGING_OPERATION_TYPE_VALUE_RECEIVE,
  messagingSettle: MESSAGING_OPERATION_TYPE_VALUE_SETTLE,
} as const
```

## [03]-[RASM_ROWS]

- Owner: `_module`, `_domain`, `_rasm`, `_metric`, `_unit`, `_kinds`, `_censuses`, `_instrument`, `_event`, and `_profile` publish every estate-minted name, so a dotted spelling closes against a roster rather than a producer's coinage.
- Law: `rasm.<domain>.<measure>` closes against `_domain` at the declaration — a measure naming an unrostered segment fails the guard rather than exporting a series no board can find, and the same `<domain>` segment is the capability subject a message-envelope `type` reads.
- Law: `_domain` rows name their emitting modules, so a subject no module mints refuses at the guard and a segment carries legislated capability rather than a coined word.
- Law: `_module` carries the emitting column and the emitter union derives from it, so a module that stops mounting narrows one roster instead of stranding a hand-listed union.
- Law: estate dimensions carve out of every instrument's own fan — a row claiming `ring` or `tenant` as its dimension double-stamps a coordinate the resource projection already carries.
- Law: unit codes are UCUM and every unit answers both egress tables, so an unanswered code renames its own series at one receiver and renders unitless at the other.
- Law: instrument rows carry their bucket layout, quantile window, and carrier width as columns, so a mount reads one row and a histogram never takes a bucket vector a caller assembled.
- Law: a frequency row admits no dimension column, because its word axis IS the exported dimension the bridge appends and a second fan multiplies one census into many.
- Law: every word-bearing row declares its CENSUS SOURCE — `fault` binds the axis to a raiser's own reason roster and `vocabulary` to a lane's declared roster — so which census a site takes is the row's answer, never the site's.
- Law: a census column rides a frequency row or a row declaring a reason fan, because a census over neither counts words no tag can carry.
- Law: `rasm.fault.*` log annotations are rostered dimensions like every other estate coordinate, so an annotation key no row spells refuses at its raise rather than reaching a log sink no query resolves.
- Law: a tracked outcome joins its evaluations on `feature_flag.context.id`, because the tracking call names a business event and carries no flag key — the targeting identity both planes stamp is the only shared coordinate, and an outcome spelled under `feature_flag.key` asserts a flag the call never named.
- Law: the tracked magnitude rides the wide event and the occurrence rides the metric plane, because a caller-defined value carries no dimension — one summed series folds currency, duration, and arity into a code no UCUM row spells — while a sampled trace plane cannot count an outcome an experiment reads as a rate.
- Law: `rasm.flag.detail` carries the tracking remainder as ONE rendered payload, because those members are caller-keyed and admit nested objects, arrays, and instants no attribute value type accepts, so the boundary is declared at the row rather than flattened into keys no roster closes.
- Growth: a measure is one `_metric` name with one `_instrument` row; a dimension is one `_rasm` key; a domain is one `_domain` row naming its emitters.
- Boundary: which site mounts which instrument is the emitting folder's; this cluster owns names, units, shapes, and their closure.
- Packages: `effect` (`Duration`); `../value/schema.ts` (`Shape`).

```typescript signature
const _module = {
  core: { emits: true },
  data: { emits: true },
  iac: { emits: false },
  runtime: { emits: true },
  security: { emits: true },
  ui: { emits: true },
  "ui/viewer": { emits: true },
} as const

const _domain = {
  admit: { emitters: ["runtime"], subject: "front-door admission decisions and the pressure windows refusing them" },
  asset: { emitters: ["data"], subject: "served-asset transform fanout and transcode economics" },
  audit: { emitters: ["data"], subject: "actor-attributed access decisions and the retention class each carries" },
  batch: { emitters: ["data"], subject: "resolver batching windows" },
  bench: { emitters: ["runtime"], subject: "benchmark claims and the verdicts grading them" },
  cache: { emitters: ["data"], subject: "lane cache population and hit economics" },
  canvas: { emitters: ["ui"], subject: "canvas layout solve admission and the supersession accounting guarding it" },
  chart: { emitters: ["ui"], subject: "pivot delta delivery into a live chart view" },
  content: { emitters: ["ui"], subject: "document commit settlement and the quarantined foreign blocks roster skew leaves" },
  crash: { emitters: ["runtime"], subject: "fatal captures and the breadcrumb replay riding them" },
  deliver: { emitters: ["runtime"], subject: "outbound channel egress and the settlement each transmission returns" },
  derivative: { emitters: ["data"], subject: "derivative render pressure over stored objects" },
  export: { emitters: ["ui"], subject: "surface serialization into content-minted parcels and their egress routes" },
  fact: { emitters: ["data"], subject: "journal fact drain into the queryable fact table" },
  fanout: { emitters: ["runtime"], subject: "broker fanout publication and the consumer lanes draining it" },
  fault: { emitters: ["core"], subject: "raised-fault identity and the recovery verdict each raise publishes to its readers" },
  flag: { emitters: ["runtime"], subject: "feature-flag decisions and the business outcomes attributed back to them" },
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
  remote: { emitters: ["data"], subject: "remote-origin transfer, reconciliation, and the refusal economics of each scheme" },
  scene: { emitters: ["ui/viewer"], subject: "scene graft admission and the refusals gating it" },
  security: { emitters: ["security"], subject: "authenticity, authorization, and key-custody decisions" },
  slo: { emitters: ["core"], subject: "objective burn and severity axes" },
  stream: { emitters: ["data"], subject: "resumable-upload finalization" },
  tap: { emitters: ["core"], subject: "hook-plane seating, delivery admission, and the isolated subscriber breaches each rail accounts" },
  vital: { emitters: ["runtime"], subject: "graded web-vital observations with the phase and subject decomposition attributing each" },
  work: { emitters: ["runtime"], subject: "work-plane channel routing and durable-actor identity" },
} as const

const _rasm = {
  admitDisposition: "rasm.admit.disposition",
  admitReason: "rasm.admit.reason",
  admitScheme: "rasm.admit.scheme",
  assetEngine: "rasm.asset.engine",
  assetOutcome: "rasm.asset.outcome",
  auditAction: "rasm.audit.action",
  auditActorKey: "rasm.audit.actor.key",
  auditActorKind: "rasm.audit.actor.kind",
  auditRetention: "rasm.audit.retention",
  auditTargetKey: "rasm.audit.target.key",
  auditTargetKind: "rasm.audit.target.kind",
  benchBand: "rasm.bench.band",
  benchCounterKind: "rasm.bench.counter.kind", // the leaf axis: the addon's platform-forked counters share one band value, so the band alone cannot split them; `Board.Bench.counterLeaves` closes the vocabulary
  benchLabel: "rasm.bench.label",
  benchSuite: "rasm.bench.suite",
  benchVerdict: "rasm.bench.verdict",
  cacheName: "rasm.cache.name",
  canvasSolveStage: "rasm.canvas.solve.stage",
  contentKind: "rasm.content.kind", // the quarantined block's foreign kind: bounded by the sending deployment's own roster, so skew reads as a short vocabulary and never a free coinage
  crashHop: "rasm.crash.hop",
  exportFormat: "rasm.export.format",
  exportSource: "rasm.export.source",
  factStream: "rasm.fact.stream",
  // the log-annotation trio a raise stamps: `code` is the routing key a reader dispatches on, `owner` the blamed leg,
  // and `posture` the producer's own re-drive verdict — one spelling the C# kernel mints under the same three words,
  // so a cross-language reader joins on the key rather than on a per-branch synonym. Governed rows, so
  // `Convention.Attributes` admits them and an unrostered `rasm.fault.*` spelling refuses at its annotation site
  // rather than reaching a log sink unresolvable
  faultCode: "rasm.fault.code",
  faultOwner: "rasm.fault.owner",
  faultPosture: "rasm.fault.posture",
  flagDetail: "rasm.flag.detail", // the rendered remainder: the member family admits nested objects, arrays, and instants no attribute value type accepts
  flagEvent: "rasm.flag.event",
  flagValue: "rasm.flag.value",
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
  remoteAction: "rasm.remote.action",
  remoteEngine: "rasm.remote.engine",
  remoteOp: "rasm.remote.op",
  remoteScheme: "rasm.remote.scheme",
  remoteWatch: "rasm.remote.watch",
  ring: "rasm.ring",
  securityDialect: "rasm.security.dialect",
  securityKind: "rasm.security.kind",
  securityReason: "rasm.security.reason",
  securitySurface: "rasm.security.surface",
  sloBurn: "rasm.slo.burn",
  sloObjective: "rasm.slo.objective",
  sloSeverity: "rasm.slo.severity",
  tapLoss: "rasm.tap.loss",       // shed beside lost: the two halves `Tap.Census` keeps apart, since one merged tally cannot tell a refused fact from a displaced one
  tapOutcome: "rasm.tap.outcome", // `Tap.Verdict`'s own arms on the wide event: a per-publish arbitration is not a summable series
  tapPoint: "rasm.tap.point",     // the `rasm.<pkg>.<domain>.<point>` row the fact fired at
  tapSeating: "rasm.tap.seating", // `Tap.Seating`'s own columns
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
  workFamily: "rasm.work.family", // the actor family, static on every message span beside the package's own entity.type
  workShard: "rasm.work.shard", // the instance's shard placement, stamped on the lifetime span alone: the message-span seat is a static record
} as const

const _ESTATE = ["ring", "tenant"] as const

const _metric = {
  admitPassed: "rasm.admit.passed",
  admitRefused: "rasm.admit.refused",
  assetTranscodeDuration: "rasm.asset.transcode.duration",
  assetTransformed: "rasm.asset.transformed",
  batchDuration: "rasm.batch.duration",
  benchCounter: "rasm.bench.counter",
  benchGc: "rasm.bench.gc",
  benchHeap: "rasm.bench.heap",
  benchTime: "rasm.bench.time",
  benchVerdicts: "rasm.bench.verdicts",
  cacheEntries: "rasm.cache.entries",
  cacheHits: "rasm.cache.hits",
  cacheMisses: "rasm.cache.misses",
  canvasSolve: "rasm.canvas.solves",
  chartFrames: "rasm.chart.frames",
  contentQuarantine: "rasm.content.quarantined",
  crashCaptured: "rasm.crash.captured",
  derivativeActive: "rasm.derivative.active",
  derivativeQueued: "rasm.derivative.queued",
  exportParcels: "rasm.export.parcels",
  exportSize: "rasm.export.size",
  factDeduped: "rasm.fact.deduped",
  factDeferred: "rasm.fact.deferred",
  factDrained: "rasm.fact.drained",
  factRefused: "rasm.fact.refused",
  flagTracked: "rasm.flag.tracked",
  formSubmit: "rasm.form.submit",
  gatewayCommands: "rasm.gateway.commands",
  gatewayDuration: "rasm.gateway.duration",
  httpServerDuration: METRIC_HTTP_SERVER_REQUEST_DURATION,
  idempotencyOutcome: "rasm.admit.idempotency",
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
  remoteBytes: "rasm.remote.bytes",
  remoteDuration: "rasm.remote.duration",
  remoteExecExits: "rasm.remote.exec.exits",
  remoteOps: "rasm.remote.ops",
  remoteResumed: "rasm.remote.resumed",
  remoteSyncActions: "rasm.remote.sync.actions",
  remoteWatchChanges: "rasm.remote.watch.changes",
  sceneBackend: "rasm.scene.backend",
  sceneGrafts: "rasm.scene.grafts",
  sceneRefusals: "rasm.scene.refusals",
  securityAdmitted: "rasm.security.admitted",
  securityCeremony: "rasm.security.ceremony",
  securityJwksMiss: "rasm.security.jwks.miss",
  securityJwksQuarantined: "rasm.security.jwks.quarantined",
  securityJwksResolve: "rasm.security.jwks.resolve",
  securityKdf: "rasm.security.crypto.kdf",
  securityPolicyDeny: "rasm.security.policy.deny",
  securityRejects: "rasm.security.rejects",
  securitySecretRotation: "rasm.security.secret.rotation",
  securityShredReject: "rasm.security.shred.reject",
  streamSize: "rasm.stream.size",
  tapAdmitted: "rasm.tap.admitted",
  tapBreaches: "rasm.tap.breaches",
  tapDropped: "rasm.tap.dropped",
  tapSeats: "rasm.tap.seats",
  tapVetoed: "rasm.tap.vetoed",
  vitalDuration: "rasm.vital.duration",
  vitalObserved: "rasm.vital.observed",
  vitalScore: "rasm.vital.score",
  vitalSize: "rasm.vital.size",
} as const

const _metricKinds = [
  _metric.admitPassed, _metric.admitRefused, _metric.assetTranscodeDuration, _metric.assetTransformed,
  _metric.batchDuration, _metric.benchCounter, _metric.benchGc, _metric.benchHeap, _metric.benchTime,
  _metric.benchVerdicts, _metric.cacheEntries, _metric.cacheHits, _metric.cacheMisses, _metric.canvasSolve,
  _metric.chartFrames, _metric.contentQuarantine,
  _metric.crashCaptured, _metric.derivativeActive, _metric.derivativeQueued, _metric.exportParcels, _metric.exportSize,
  _metric.factDeduped, _metric.factDeferred, _metric.factDrained, _metric.factRefused, _metric.flagTracked,
  _metric.formSubmit,
  _metric.gatewayCommands, _metric.gatewayDuration,
  _metric.httpServerDuration, _metric.idempotencyOutcome, _metric.invokeCalls, _metric.invokeDuration,
  _metric.invokeFault, _metric.laneCheckpoint, _metric.meterUsage, _metric.objectReclaimed, _metric.objectSize,
  _metric.objectWritten, _metric.olapDeferred, _metric.olapRetried, _metric.olapWait, _metric.outboxAge,
  _metric.outboxDepth, _metric.outboxRedelivered, _metric.poolHeld, _metric.profileDuration, _metric.queueDepth,
  _metric.queueParked, _metric.relayDrained, _metric.remoteBytes, _metric.remoteDuration, _metric.remoteExecExits,
  _metric.remoteOps, _metric.remoteResumed, _metric.remoteSyncActions, _metric.remoteWatchChanges,
  _metric.sceneBackend, _metric.sceneGrafts, _metric.sceneRefusals,
  _metric.securityAdmitted, _metric.securityCeremony, _metric.securityJwksMiss, _metric.securityJwksQuarantined,
  _metric.securityJwksResolve, _metric.securityKdf, _metric.securityPolicyDeny, _metric.securityRejects,
  _metric.securitySecretRotation, _metric.securityShredReject, _metric.streamSize,
  _metric.tapAdmitted, _metric.tapBreaches, _metric.tapDropped, _metric.tapSeats, _metric.tapVetoed,
  _metric.vitalDuration,
  _metric.vitalObserved, _metric.vitalScore, _metric.vitalSize,
] as const

const _unit = {
  action: "{action}",
  admission: "{admission}",
  byte: "By",
  call: "{call}",
  change: "{change}",
  decision: "{decision}",
  deliverable: "{deliverable}",
  entry: "{entry}",
  event: "{event}",
  exit: "{exit}",
  fault: "{fault}",
  frame: "{frame}",
  graft: "{graft}",
  hit: "{hit}",
  item: "{item}",
  key: "{key}",
  lease: "{lease}",
  milli: "ms",
  miss: "{miss}",
  nano: "ns",
  object: "{object}",
  op: "{op}",
  parcel: "{parcel}",
  position: "{position}",
  refusal: "{refusal}",
  reject: "{reject}",
  render: "{render}",
  rotation: "{rotation}",
  second: "s",
  transfer: "{transfer}",
  trip: "{trip}",
  unity: "1",
  verdict: "{verdict}",
} as const

const _unitKinds = [
  _unit.action, _unit.admission, _unit.byte, _unit.call, _unit.change, _unit.decision, _unit.deliverable, _unit.entry,
  _unit.event, _unit.exit, _unit.fault, _unit.frame, _unit.graft, _unit.hit, _unit.item, _unit.key, _unit.lease,
  _unit.milli,
  _unit.miss, _unit.nano, _unit.object,
  _unit.op, _unit.parcel, _unit.position, _unit.refusal, _unit.reject, _unit.render, _unit.rotation, _unit.second,
  _unit.transfer, _unit.trip, _unit.unity, _unit.verdict,
] as const

const _scale = { [_unit.milli]: 1, [_unit.nano]: 1_000_000, [_unit.second]: 0.001 } as const

const _promUnit = {
  [_unit.action]: "",
  [_unit.admission]: "",
  [_unit.byte]: "bytes",
  [_unit.call]: "",
  [_unit.change]: "",
  [_unit.decision]: "",
  [_unit.deliverable]: "",
  [_unit.entry]: "",
  [_unit.event]: "",
  [_unit.exit]: "",
  [_unit.fault]: "",
  [_unit.frame]: "",
  [_unit.graft]: "",
  [_unit.hit]: "",
  [_unit.item]: "",
  [_unit.key]: "",
  [_unit.lease]: "",
  [_unit.milli]: "milliseconds",
  [_unit.miss]: "",
  [_unit.nano]: "nanoseconds",
  [_unit.object]: "",
  [_unit.op]: "",
  [_unit.position]: "",
  [_unit.refusal]: "",
  [_unit.reject]: "",
  [_unit.render]: "",
  [_unit.rotation]: "",
  [_unit.second]: "seconds",
  [_unit.transfer]: "",
  [_unit.trip]: "",
  [_unit.unity]: "",
  [_unit.verdict]: "",
} as const

const _Unit = Shape.vocabulary(_unitKinds, _promUnit)
const _grafanaUnit = {
  [_unit.action]: { level: "short", rate: "cps" },
  [_unit.admission]: { level: "short", rate: "cps" },
  [_unit.byte]: { level: "bytes", rate: "binBps" },        // IEC on both halves: a level in 1024-scaled bytes and a rate in the matching per-second id
  [_unit.call]: { level: "short", rate: "cps" },
  [_unit.change]: { level: "short", rate: "cps" },
  [_unit.decision]: { level: "short", rate: "cps" },
  [_unit.deliverable]: { level: "short", rate: "cps" },
  [_unit.entry]: { level: "short", rate: "cps" },
  [_unit.event]: { level: "short", rate: "cps" },
  [_unit.exit]: { level: "short", rate: "cps" },
  [_unit.fault]: { level: "short", rate: "cps" },
  [_unit.frame]: { level: "short", rate: "cps" },
  [_unit.graft]: { level: "short", rate: "cps" },
  [_unit.hit]: { level: "short", rate: "cps" },
  [_unit.item]: { level: "short", rate: "cps" },
  [_unit.key]: { level: "short", rate: "cps" },
  [_unit.lease]: { level: "short", rate: "cps" },
  [_unit.milli]: { level: "ms", rate: "percentunit" },
  [_unit.miss]: { level: "short", rate: "cps" },
  [_unit.nano]: { level: "ns", rate: "percentunit" },
  [_unit.object]: { level: "short", rate: "cps" },
  [_unit.op]: { level: "short", rate: "cps" },
  [_unit.position]: { level: "short", rate: "cps" },
  [_unit.refusal]: { level: "short", rate: "cps" },
  [_unit.reject]: { level: "short", rate: "cps" },
  [_unit.render]: { level: "short", rate: "cps" },
  [_unit.rotation]: { level: "short", rate: "cps" },
  [_unit.second]: { level: "s", rate: "percentunit" },
  [_unit.transfer]: { level: "short", rate: "cps" },
  [_unit.trip]: { level: "short", rate: "cps" },
  [_unit.unity]: { level: "short", rate: "cps" },
  [_unit.verdict]: { level: "short", rate: "cps" },
} as const

// Where a row's word axis comes FROM, declared on the row rather than chosen at the site: `fault` names a raiser's own
// reason roster, so the census is complete exactly when every reason that family can raise preregisters; `vocabulary`
// names a lane's declared state or outcome roster, whose words a fold produces. The column is what makes the word axis
// a published census value instead of a tuple a caller assembles.
const _censuses = ["fault", "vocabulary"] as const

const _kinds = ["counter", "frequency", "gauge", "histogram", "summary", "updown"] as const

const _tail = {
  counter: { dimensionless: "total", measured: "total" },
  frequency: { dimensionless: "total", measured: "total" },
  gauge: { dimensionless: "ratio", measured: "" },
  histogram: { dimensionless: "", measured: "" },
  summary: { dimensionless: "", measured: "" }, // the quantile series carries the bare name; the receiver mints the `_count`/`_sum` siblings itself
  updown: { dimensionless: "", measured: "" },
} as const
const _Kind = Shape.vocabulary(_kinds, _tail)

const _instrument = {
  admitPassed: { description: "front-door admissions by authentication scheme", dimensions: [_rasm.admitScheme], kind: "counter", name: _metric.admitPassed, unit: _unit.admission },
  admitRefused: { description: "front-door refusals by reason", dimensions: [_rasm.admitReason], kind: "counter", name: _metric.admitRefused, unit: _unit.refusal },
  assetTranscodeDuration: { bounds: { count: 11, factor: 2, ladder: "exponential", start: 16 }, description: "asset transcode wall span", kind: "histogram", name: _metric.assetTranscodeDuration, unit: _unit.milli },
  assetTransformed: { description: "asset transforms by engine plane and outcome", dimensions: [_rasm.assetEngine, _rasm.assetOutcome], kind: "counter", name: _metric.assetTransformed, unit: _unit.object },
  batchDuration: { bounds: { count: 11, factor: 2, ladder: "exponential", start: 1 }, description: "resolver window wall span", kind: "histogram", name: _metric.batchDuration, unit: _unit.milli },
  benchCounter: { description: "hardware-counter band by counter kind and axis", dimensions: [_rasm.benchBand, _rasm.benchCounterKind, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchCounter, unit: _unit.event },
  benchGc: { description: "benchmark GC-timing band", dimensions: [_rasm.benchBand, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchGc, unit: _unit.nano },
  benchHeap: { description: "benchmark heap-delta band", dimensions: [_rasm.benchBand, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchHeap, unit: _unit.byte },
  benchTime: { description: "benchmark timing ladder by band", dimensions: [_rasm.benchBand, _rasm.benchLabel, _rasm.benchSuite], kind: "gauge", name: _metric.benchTime, unit: _unit.nano },
  benchVerdicts: { description: "regression verdicts by grade", dimensions: [_rasm.benchLabel, _rasm.benchSuite, _rasm.benchVerdict], kind: "counter", name: _metric.benchVerdicts, unit: _unit.verdict },
  cacheEntries: { description: "cache entries resident by cache name", dimensions: [_rasm.cacheName], kind: "gauge", name: _metric.cacheEntries, unit: _unit.entry },
  cacheHits: { description: "cache hits by cache name", dimensions: [_rasm.cacheName], kind: "counter", name: _metric.cacheHits, unit: _unit.hit },
  cacheMisses: { description: "cache misses by cache name", dimensions: [_rasm.cacheName], kind: "counter", name: _metric.cacheMisses, unit: _unit.miss },
  canvasSolve: { description: "canvas layout solves by admission stage", dimensions: [_rasm.canvasSolveStage], kind: "counter", name: _metric.canvasSolve, unit: _unit.op },
  chartFrames: { description: "pivot delta frames delivered", kind: "counter", name: _metric.chartFrames, unit: _unit.frame },
  contentQuarantine: { description: "document blocks quarantined by foreign kind", dimensions: [_rasm.contentKind], kind: "counter", name: _metric.contentQuarantine, unit: _unit.item },
  crashCaptured: { description: "fatal captures by fault class", dimensions: [_attr.errorType], kind: "counter", name: _metric.crashCaptured, unit: _unit.fault },
  derivativeActive: { description: "derivative renders in flight", kind: "gauge", name: _metric.derivativeActive, unit: _unit.render },
  derivativeQueued: { description: "derivative renders awaiting a worker", kind: "gauge", name: _metric.derivativeQueued, unit: _unit.render },
  exportParcels: { description: "export parcels minted by format and source", dimensions: [_rasm.exportFormat, _rasm.exportSource], kind: "counter", name: _metric.exportParcels, unit: _unit.parcel },
  exportSize: { bounds: { count: 11, factor: 4, ladder: "exponential", start: 1024 }, description: "parcel octets by format", dimensions: [_rasm.exportFormat], kind: "histogram", name: _metric.exportSize, unit: _unit.byte },
  factDeduped: { description: "journal facts a content key matched as already landed", dimensions: [_rasm.factStream], kind: "counter", name: _metric.factDeduped, unit: _unit.item },
  factDeferred: { description: "journal append attempts the durable plane refused", kind: "counter", name: _metric.factDeferred, unit: _unit.item },
  factDrained: { description: "journal facts drained to the fact table", dimensions: [_rasm.auditAction, _rasm.auditActorKind, _rasm.factStream], kind: "counter", name: _metric.factDrained, unit: _unit.item },
  factRefused: { description: "journal facts parked on the refused roster", dimensions: [_rasm.factStream], kind: "counter", name: _metric.factRefused, unit: _unit.item },
  flagTracked: { census: "vocabulary", description: "tracked business outcomes by event name", kind: "frequency", name: _metric.flagTracked, unit: _unit.event },
  formSubmit: { description: "settled submit trips by outcome", dimensions: [_rasm.formOutcome], kind: "counter", name: _metric.formSubmit, unit: _unit.trip },
  gatewayCommands: { description: "gateway dispatches by outcome", dimensions: [_rasm.gatewayOutcome], kind: "counter", name: _metric.gatewayCommands, unit: _unit.call },
  gatewayDuration: { bounds: { count: 5, factor: 4, ladder: "exponential", start: 25 }, description: "gateway dispatch wall span", kind: "histogram", name: _metric.gatewayDuration, unit: _unit.milli },
  httpServerDuration: { bounds: { edges: [0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10], ladder: "explicit" }, description: "server request wall span", dimensions: [_attr.httpMethod, _attr.httpRoute], kind: "histogram", name: _metric.httpServerDuration, unit: _unit.second },
  idempotencyOutcome: { description: "idempotency-key dispositions", dimensions: [_rasm.admitDisposition], kind: "counter", name: _metric.idempotencyOutcome, unit: _unit.decision },
  invokeCalls: { census: "fault", description: "capability calls by outcome", dimensions: [_rasm.invokeOutcome], kind: "counter", name: _metric.invokeCalls, unit: _unit.call },
  invokeDuration: { bounds: { count: 5, factor: 4, ladder: "exponential", start: 25 }, description: "capability call wall span", kind: "histogram", name: _metric.invokeDuration, unit: _unit.milli },
  invokeFault: { census: "fault", description: "capability fault reasons", kind: "frequency", name: _metric.invokeFault, unit: _unit.fault },
  laneCheckpoint: { bigint: true, description: "last committed projection drain position", dimensions: [_rasm.laneName], kind: "gauge", name: _metric.laneCheckpoint, unit: _unit.position },
  meterUsage: { description: "billable usage by metered resource", dimensions: [_rasm.meterResource], kind: "counter", name: _metric.meterUsage, unit: _unit.item },
  objectReclaimed: { description: "bytes reclaimed by the reference sweep", kind: "counter", name: _metric.objectReclaimed, unit: _unit.byte },
  objectSize: { description: "object bytes landed by write legs", kind: "counter", name: _metric.objectSize, unit: _unit.byte },
  objectWritten: { description: "object writes by dedup outcome", dimensions: [_rasm.objectOutcome], kind: "counter", name: _metric.objectWritten, unit: _unit.object },
  olapDeferred: { bounds: { count: 11, factor: 2, ladder: "exponential", start: 1 }, description: "deferred-query wait until execution", dimensions: [_rasm.olapEngine], kind: "histogram", name: _metric.olapDeferred, unit: _unit.milli },
  olapRetried: { description: "lake queries retried by engine", dimensions: [_rasm.olapEngine], kind: "counter", name: _metric.olapRetried, unit: _unit.call },
  olapWait: { bounds: { count: 11, factor: 2, ladder: "exponential", start: 1 }, description: "lake-engine admission wait span", dimensions: [_rasm.olapEngine], kind: "histogram", name: _metric.olapWait, unit: _unit.milli },
  outboxAge: { description: "oldest undelivered outbox row age", kind: "gauge", name: _metric.outboxAge, unit: _unit.second },
  outboxDepth: { description: "undelivered outbox rows", kind: "gauge", name: _metric.outboxDepth, unit: _unit.deliverable },
  outboxRedelivered: { description: "undelivered rows claimed more than once", kind: "gauge", name: _metric.outboxRedelivered, unit: _unit.deliverable },
  poolHeld: { description: "pool leases held by scheme", dimensions: [_rasm.poolScheme], kind: "updown", name: _metric.poolHeld, unit: _unit.lease },
  profileDuration: { bounds: { count: 13, factor: 2, ladder: "exponential", start: 1 }, description: "harvested engine profile wall span", dimensions: [_rasm.profileEngine], kind: "histogram", name: _metric.profileDuration, unit: _unit.milli },
  queueDepth: { description: "durable-queue rows awaiting settlement", kind: "gauge", name: _metric.queueDepth, unit: _unit.deliverable },
  queueParked: { description: "deliverables parked to the dead set", dimensions: [_rasm.workChannel], kind: "counter", name: _metric.queueParked, unit: _unit.deliverable },
  relayDrained: { description: "relay claims settled by channel", dimensions: [_rasm.workChannel], kind: "counter", name: _metric.relayDrained, unit: _unit.deliverable },
  remoteBytes: { description: "remote-origin octets moved by scheme and verb", dimensions: [_rasm.remoteOp, _rasm.remoteScheme], kind: "counter", name: _metric.remoteBytes, unit: _unit.byte },
  remoteDuration: { description: "remote-origin operation wall span", dimensions: [_rasm.remoteOp, _rasm.remoteScheme], kind: "summary", name: _metric.remoteDuration, unit: _unit.milli, window: { error: 0.01, maxAge: Duration.minutes(10), maxSize: 4096, quantiles: [0.5, 0.9, 0.99] } },
  remoteExecExits: { description: "remote command exits by scheme and fault class", dimensions: [_attr.errorType, _rasm.remoteScheme], kind: "counter", name: _metric.remoteExecExits, unit: _unit.exit },
  remoteOps: { census: "fault", description: "remote-origin operations by scheme, verb, and fault class", dimensions: [_attr.errorType, _rasm.remoteOp, _rasm.remoteScheme], kind: "counter", name: _metric.remoteOps, unit: _unit.op },
  remoteResumed: { description: "transfers resumed by engine", dimensions: [_rasm.remoteEngine], kind: "counter", name: _metric.remoteResumed, unit: _unit.transfer },
  remoteSyncActions: { description: "reconciliation actions by kind", dimensions: [_rasm.remoteAction], kind: "counter", name: _metric.remoteSyncActions, unit: _unit.action },
  remoteWatchChanges: { description: "watch changes by scheme and strategy", dimensions: [_rasm.remoteScheme, _rasm.remoteWatch], kind: "counter", name: _metric.remoteWatchChanges, unit: _unit.change },
  sceneBackend: { census: "vocabulary", description: "backend lifecycle turns by landed phase", kind: "frequency", name: _metric.sceneBackend, unit: _unit.event },
  sceneGrafts: { description: "committed graft arrivals", kind: "counter", name: _metric.sceneGrafts, unit: _unit.graft },
  sceneRefusals: { census: "fault", description: "graft refusals by fault reason", kind: "frequency", name: _metric.sceneRefusals, unit: _unit.refusal },
  securityAdmitted: { description: "authenticity admissions by kind", dimensions: [_rasm.securityKind], kind: "counter", name: _metric.securityAdmitted, unit: _unit.admission },
  securityCeremony: { bounds: { count: 6, factor: 2, ladder: "exponential", start: 125 }, description: "credential-ceremony wall span", dimensions: [_rasm.securityKind], kind: "histogram", name: _metric.securityCeremony, unit: _unit.milli }, // rungs 125..4000 land 1000 as a declared edge — the audit ceremony objective pins its p99 ceiling there, and a ceiling must land on a bound
  securityJwksMiss: { description: "cold JWKS resolutions missing the cache", kind: "counter", name: _metric.securityJwksMiss, unit: _unit.miss },
  securityJwksQuarantined: { description: "JWKS keys quarantined by the breaker", kind: "counter", name: _metric.securityJwksQuarantined, unit: _unit.key },
  securityJwksResolve: { bounds: { edges: [5, 25, 100, 250, 1000, 5000], ladder: "explicit" }, description: "JWKS resolve wall span", kind: "histogram", name: _metric.securityJwksResolve, unit: _unit.milli },
  securityKdf: { bounds: { edges: [10, 25, 50, 100, 250, 500, 1000, 2500], ladder: "explicit" }, description: "key-derivation wall span", kind: "histogram", name: _metric.securityKdf, unit: _unit.milli },
  securityPolicyDeny: { description: "authorization denials by reason", dimensions: [_rasm.securityReason], kind: "counter", name: _metric.securityPolicyDeny, unit: _unit.decision },
  securityRejects: { description: "authenticity rejects by kind and facet", dimensions: [_rasm.securityDialect, _rasm.securityKind, _rasm.securityReason, _rasm.securitySurface], kind: "counter", name: _metric.securityRejects, unit: _unit.reject },
  securitySecretRotation: { description: "secret rotations by custody path", kind: "counter", name: _metric.securitySecretRotation, unit: _unit.rotation },
  securityShredReject: { description: "opens rejected on a shredded key", kind: "counter", name: _metric.securityShredReject, unit: _unit.reject },
  streamSize: { description: "resumable-upload bytes finalized", kind: "counter", name: _metric.streamSize, unit: _unit.byte },
  tapAdmitted: { description: "hook facts a point channel admitted", dimensions: [_rasm.tapPoint], kind: "counter", name: _metric.tapAdmitted, unit: _unit.event },
  tapBreaches: { description: "isolated subscriber breaches by point and fault class", dimensions: [_attr.errorType, _rasm.tapPoint], kind: "counter", name: _metric.tapBreaches, unit: _unit.fault },
  tapDropped: { description: "hook facts lost by half — refused at admission or displaced from the retained window", dimensions: [_rasm.tapLoss, _rasm.tapPoint], kind: "counter", name: _metric.tapDropped, unit: _unit.event },
  tapSeats: { description: "hook seating outcomes by mount, refusal, and release", dimensions: [_rasm.tapSeating], kind: "counter", name: _metric.tapSeats, unit: _unit.admission },
  tapVetoed: { description: "hook facts an arbiter refused before delivery", dimensions: [_rasm.tapPoint], kind: "counter", name: _metric.tapVetoed, unit: _unit.refusal },
  vitalDuration: { description: "current accounted time-measured vital level", dimensions: [_rasm.vitalKind], kind: "gauge", name: _metric.vitalDuration, unit: _unit.milli },
  vitalObserved: { description: "graded web-vital observations", dimensions: [_rasm.vitalGrade, _rasm.vitalKind], kind: "counter", name: _metric.vitalObserved, unit: _unit.event },
  vitalScore: { description: "current accounted dimensionless vital level", dimensions: [_rasm.vitalKind], kind: "gauge", name: _metric.vitalScore, unit: _unit.unity },
  vitalSize: { description: "current accounted byte-measured vital level", dimensions: [_rasm.vitalKind], kind: "gauge", name: _metric.vitalSize, unit: _unit.byte },
} as const

const _durationKinds = [
  _metric.assetTranscodeDuration, _metric.batchDuration, _metric.benchGc, _metric.benchTime,
  _metric.gatewayDuration, _metric.httpServerDuration, _metric.invokeDuration, _metric.olapDeferred,
  _metric.olapWait, _metric.outboxAge, _metric.profileDuration, _metric.remoteDuration,
  _metric.securityCeremony, _metric.securityJwksResolve, _metric.securityKdf, _metric.vitalDuration,
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

- Owner: `_translation`, `_wire`, `_translated`, `_mount`, `_outcome`, `_tracked`, `_dimensions`, and `_identity` own store-side naming, the estate pins, instrument materialization, the two census aspects, and resource stamping.
- Law: suffixing is a TARGET property — `translated` projects a store's own series name off the receiver's declared strategy, so a producer mints one dotted name and a store that escapes or suffixes reads its own row rather than a second spelling on the mint.
- Law: `mount` memoizes each row-and-vocabulary pair, so one site names an instrument and a second site naming the same row shares its carrier instead of minting a twin the exporter reports separately.
- Law: UCUM tagging lands at the mount, because the OTLP bridge computes an exported descriptor's unit before any view runs and the constructors take no unit option; the export lane drops that key by name.
- Law: outcome derives from ONE `Exit` fold discriminating interrupt-first, so an interrupted run carries no outcome and a defect never reads as a typed fault; the vocabulary anchors on the family's own reason axis and widens there, never inside an arm.
- Law: `outcome` takes the row's OWN declared fan, so an axis the instrument never mints is unspellable at the call and a tagged increment cannot invent a dimension the census refuses.
- Law: every word axis arrives as its owner's PUBLISHED census, so a duplicate word already refused at the vocabulary mint and a word no raiser produces has no spelling at the call.
- Law: a census word shadowing an exit row refuses at the aspect's own parameter, because a reason spelled `resolved` fuses a settled success with the fault it hid.
- Law: the outcome fold holds one tagged carrier per admitted word from construction, so the exit path mints nothing and a word outside the census cannot be reached.
- Law: `tracked` is the fault-census frequency aspect — the row's mount and the error-rail fold ride one declaration, so no page spells the tracking operator beside its own mount.
- Law: the resource projection stamps identity ONCE and every optional coordinate rides a fold, so an absent region, zone, cluster, or tenant OMITS its key rather than exporting an empty string a query reads as a value.
- Law: the estate service group is the floor a fleet's own namespace overrides, so one pin serves every unconfigured app without freezing a deployment that names its own.
- Growth: a receiver posture is one `_translation` row; a promoted identity coordinate is one `_ESTATE` entry with its resource fold.
- Boundary: exporter wiring and the reader seat are `runtime/otel`'s; this cluster owns names, shapes, and the projections a store reads.
- Packages: `effect` (`Array`, `Cause`, `Duration`, `Effect`, `Exit`, `Metric`, `MetricBoundaries`, `MutableHashMap`, `Option`, `Record`, `Types`); `../value/identity.ts` (`Identity`); `../value/schema.ts` (`Shape`).

```typescript signature
const _translation = {
  NoTranslation: { escape: false, suffix: false },
  NoUTF8EscapingWithSuffixes: { escape: false, suffix: true },
  UnderscoreEscapingWithoutSuffixes: { escape: true, suffix: false },
  UnderscoreEscapingWithSuffixes: { escape: true, suffix: true },
} as const

const _wire = {
  namespace: "rasm",
  occurrence: "key", // the word axis both metric bridges append to a frequency point, hardcoded there: the one exported dimension no vocabulary row mints
  schemaUrl: "https://opentelemetry.io/schemas/1.43.0", // path segment tracks the manifest semconv pin; bump together
  translation: "NoUTF8EscapingWithSuffixes",
  unit: "unit", // the synthetic UCUM carrier: Effect's constructors take no unit option, so the OTLP bridge reads the descriptor unit off this tag key
} as const

const _named = Record.fromEntries(Array.map(Record.values(_instrument), (row) => [row.name, row] as const)) as Convention.Named
const _Metric = Shape.vocabulary(_metricKinds, _named)
const _Duration = Shape.vocabulary(_durationKinds, {
  [_metric.assetTranscodeDuration]: _Metric.at(_metric.assetTranscodeDuration),
  [_metric.batchDuration]: _Metric.at(_metric.batchDuration),
  [_metric.benchGc]: _Metric.at(_metric.benchGc),
  [_metric.benchTime]: _Metric.at(_metric.benchTime),
  [_metric.gatewayDuration]: _Metric.at(_metric.gatewayDuration),
  [_metric.httpServerDuration]: _Metric.at(_metric.httpServerDuration),
  [_metric.invokeDuration]: _Metric.at(_metric.invokeDuration),
  [_metric.olapDeferred]: _Metric.at(_metric.olapDeferred),
  [_metric.olapWait]: _Metric.at(_metric.olapWait),
  [_metric.outboxAge]: _Metric.at(_metric.outboxAge),
  [_metric.profileDuration]: _Metric.at(_metric.profileDuration),
  [_metric.remoteDuration]: _Metric.at(_metric.remoteDuration),
  [_metric.securityCeremony]: _Metric.at(_metric.securityCeremony),
  [_metric.securityJwksResolve]: _Metric.at(_metric.securityJwksResolve),
  [_metric.securityKdf]: _Metric.at(_metric.securityKdf),
  [_metric.vitalDuration]: _Metric.at(_metric.vitalDuration),
})

const _translated = (metric: Convention.MetricName, strategy: Convention.Translation = _wire.translation): string => {
  const row = _Metric.at(metric)
  const policy = _translation[strategy]
  const name = policy.escape ? metric.replaceAll(/[^a-zA-Z0-9:_]/g, "_") : metric
  return policy.suffix
    ? Array.join(
      Array.filter([name, _Unit.at(row.unit), _Kind.at(row.kind)[row.unit === _unit.unity ? "dimensionless" : "measured"]], (part) => part.length > 0),
      "_",
    )
    : name
}

const _temporal = (unit: Convention.Unit): unit is Convention.TimeUnit => unit in _scale

const _duration = (metric: Convention.DurationMetric, span: Duration.Duration): number =>
  Duration.toMillis(span) * _scale[_Metric.at(metric).unit]

const _LADDER: { readonly [L in Convention.Ladder]: (bounds: Extract<Convention.Bounds, { readonly ladder: L }>) => ReadonlyArray<number> } = {
  explicit: (bounds) => bounds.edges,
  exponential: (bounds) => Array.makeBy(bounds.count, (rung) => bounds.start * bounds.factor ** rung),
  linear: (bounds) => Array.makeBy(bounds.count, (rung) => bounds.start + rung * bounds.width),
}

const _quantiles = (metric: Convention.MetricName<"summary">): ReadonlyArray<number> => _Metric.at(metric).window.quantiles

const _edges = (metric: Convention.MetricName<"histogram">): ReadonlyArray<number> =>
  // BOUNDARY ADAPTER: a keyed dispatch erases the case-to-arm correlation the mapped table declares, so one cast rejoins each
  // arm to the case its own discriminant selected
  (_LADDER[_Metric.at(metric).bounds.ladder] as (bounds: Convention.Bounds) => ReadonlyArray<number>)(_Metric.at(metric).bounds)

const _counted = (row: Extract<Convention.Row, { readonly kind: "counter" | "updown" }>, incremental: boolean): Convention.Instrument =>
  row.bigint === true
    ? Metric.counter(row.name, { bigint: true, description: row.description, incremental })
    : Metric.counter(row.name, { description: row.description, incremental })

const _spanned = (row: Extract<Convention.Row, { readonly kind: "histogram" }>): Convention.Instrument => {
  const base = Metric.histogram(row.name, MetricBoundaries.fromIterable(_edges(row.name)), row.description)
  return Option.match(Option.liftPredicate(row.unit, _temporal), {
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
  summary: (row) =>
    Metric.summary({
      description: row.description,
      error: row.window.error,
      maxAge: row.window.maxAge,
      maxSize: row.window.maxSize,
      name: row.name,
      quantiles: row.window.quantiles,
    }),
  updown: (row) => _counted(row, false),
}

const _mounted = MutableHashMap.empty<string, Convention.Instrument>()

const _mount = <N extends Convention.MetricName, const W extends Convention.Roster>(
  metric: N,
  ...words: Convention.Words<N, W>
): Convention.Mounted<N> => {
  const vocabulary: ReadonlyArray<string> = words[0]?.kinds ?? []
  const key = JSON.stringify([metric, vocabulary])
  return Option.getOrElse(MutableHashMap.get(_mounted, key), () => {
    // BOUNDARY ADAPTER: the keyed dispatch erases the row-to-arm correlation the mapped table declares and `Mounted`
    // re-derives the carrier from the same columns the arm read; UCUM tagging lands here because the OTLP bridge computes
    // that exported descriptor unit before any view runs. This memo boundary materializes each row/vocabulary pair once.
    const mounted = Metric.tagged(
      (_MOUNT[_Metric.at(metric).kind] as (row: Convention.Row, words: ReadonlyArray<string>) => Convention.Instrument)(
        _Metric.at(metric),
        vocabulary,
      ),
      _wire.unit,
      _Metric.at(metric).unit,
    )
    MutableHashMap.set(_mounted, key, mounted)
    return mounted
  }) as Convention.Mounted<N>
}

const _TERMINAL = ["crashed", "halted", "resolved"] as const // the fold's own three exit rows, which every census word must stay disjoint from

const _outcome = <N extends Convention.CensusMetric<"fault", "counter">, E, const W extends Convention.Roster>(
  metric: N,
  key: Convention.Fan<N>, // the row's OWN declared fan: an axis the instrument never mints is unspellable here
  census: Convention.Census<W> & Convention.Fused<W>, // the raiser's published roster, and `Fused` refuses one whose word shadows an exit row
  reason: (fault: E) => W[number],
): (<A, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>) => {
  const mounted = _mount(metric)
  // BOUNDARY ADAPTER: the entry pairs erase to a string-keyed record and the census closes the word set the fold can
  // reach, so every admitted word holds its tagged carrier from construction rather than minting one per exit
  const tagged = Record.fromEntries(
    Array.map([..._TERMINAL, ...census.kinds], (word) => [word, Metric.tagged(mounted, key, word)] as const),
  ) as { readonly [O in Convention.Outcome<W[number]>]: Convention.Mounted<N> }
  const worded: (exit: Exit.Exit<unknown, E>) => Convention.Outcome<W[number]> = Exit.match({
    onFailure: (cause) =>
      Cause.isInterruptedOnly(cause)
        ? ("halted" as const)
        : Option.match(Cause.failureOption(cause), { onNone: () => "crashed" as const, onSome: reason }),
    onSuccess: () => "resolved" as const,
  })
  return (self) => Effect.onExit(self, (exit) => Metric.increment(tagged[worded(exit)]))
}

const _tracked = <N extends Convention.CensusMetric<"fault", "frequency">, E, const W extends Convention.Roster>(
  metric: N,
  census: Convention.Census<W>,
  reason: (fault: E) => W[number],
): (<A, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>) => {
  const mounted = _mount(metric, census)
  return (self) => Metric.trackErrorWith(self, mounted, reason)
}

const _keys: ReadonlyArray<Convention.Key> = [
  ...Record.values(_attr),
  ...Record.values(_incubating),
  ...Record.values(_rasm),
  _profile.id,
]

const _dimensions: ReadonlyArray<Convention.Dimension> = Array.dedupe([
  ...Array.flatMap(Record.values(_instrument), (row) => row.dimensions ?? []),
  ...Array.map(_ESTATE, (dimension) => _rasm[dimension]),
  _wire.occurrence,
])

const _identity = (identity: Identity.App): Convention.Resource => ({
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
  type Units = typeof _Unit.kinds
  type TimeUnit = keyof typeof _scale
  type Ladder = Bounds["ladder"]
  type Bounds =
    | { readonly count: number; readonly factor: number; readonly ladder: "exponential"; readonly start: number }
    | { readonly count: number; readonly ladder: "linear"; readonly start: number; readonly width: number }
    | { readonly edges: readonly [number, ...ReadonlyArray<number>]; readonly ladder: "explicit" }
  type Window = {
    readonly error: number
    readonly maxAge: Duration.Duration
    readonly maxSize: number
    readonly quantiles: readonly [number, ...ReadonlyArray<number>]
  }
  type Terminal = (typeof _TERMINAL)[number]
  type Outcome<W extends string = never> = Terminal | W // the fold's three exit rows beside the census's own word axis
  type CensusSource = (typeof _censuses)[number]
  type Roster = readonly [string, ...ReadonlyArray<string>] // the census's word tuple as a CONSTRAINT only: a mount takes the published census, never a tuple a caller assembled
  type Census<W extends Roster = Roster> = Shape.Vocabulary<W, { readonly [K in W[number]]: unknown }> // the ordered roster its own owner minted, so a duplicate word already refused at the vocabulary
  type CensusMetric<S extends CensusSource = CensusSource, K extends InstrumentKind = InstrumentKind> = {
    readonly [N in keyof typeof _instrument]: (typeof _instrument)[N] extends { readonly census: S; readonly kind: K } ? (typeof _instrument)[N]["name"] : never
  }[keyof typeof _instrument] // the rows a census aspect may aim at: a counter whose fan is a name, scheme, or resource axis is unspellable at `outcome`
  type Fused<W extends Roster> = [Extract<W[number], Terminal>] extends [never] ? unknown : never // a census word shadowing an exit row fuses a success with a fault, so the intersection admits no argument
  type Fan<N extends MetricName> = Named[N] extends { readonly dimensions: infer D extends ReadonlyArray<Dimension> } ? D[number] : never
  type MetricName<K extends InstrumentKind = InstrumentKind> = {
    readonly [N in keyof typeof _instrument]: (typeof _instrument)[N]["kind"] extends K ? (typeof _instrument)[N]["name"] : never
  }[keyof typeof _instrument]
  type Named = { readonly [N in keyof typeof _instrument as (typeof _instrument)[N]["name"]]: (typeof _instrument)[N] } // the row table re-keyed by its own wire name
  type Row<N extends MetricName = MetricName> = Named[N]
  type Mounted<N extends MetricName, R = Named[N]> = R extends { readonly kind: "frequency" } ? Metric.Metric.Frequency<string>
    : R extends { readonly kind: "histogram" } ? Metric.Metric.Histogram<R extends { readonly unit: TimeUnit } ? Duration.Duration : number>
    : R extends { readonly kind: "summary" } ? Metric.Metric.Summary<number>
    : R extends { readonly kind: "gauge" } ? Metric.Metric.Gauge<R extends { readonly bigint: true } ? bigint : number>
    : Metric.Metric.Counter<R extends { readonly bigint: true } ? bigint : number>
  type Input<N extends MetricName> = Mounted<N> extends Metric.Metric<infer _Type, infer In, infer _Out> ? In : never // the admitted update a named row's own carrier takes, so a subscriber's projection types off the name alone
  type Instrument = Mounted<MetricName> // every carrier the census admits, which is exactly what one mount arm returns
  type Words<N extends MetricName, W extends Roster> = Named[N] extends { readonly kind: "frequency" } ? [census: Census<W>] : [] // the census is required where the family counts words and unspellable everywhere else
  type GrafanaUnit = typeof _grafanaUnit
  type Display = keyof GrafanaUnit[Unit] // the fold's own read: a quantity or that quantity per second
  type DurationMetric = (typeof _Duration.kinds)[number]
  type EventName = (typeof _event)[keyof typeof _event]
  type Scalar = string | number | boolean
  type Value =
    | Scalar
    | ReadonlyArray<string | null | undefined>
    | ReadonlyArray<number | null | undefined>
    | ReadonlyArray<boolean | null | undefined>
  type ValueOf<K extends Key> = K extends (typeof _incubating)["connectionType"] ? ConnectionType
    : K extends (typeof _incubating)["flagReason"] ? FlagReason
    : K extends (typeof _incubating)["browserMobile"] ? boolean
    : K extends (typeof _attr)["containerImageTags"] | (typeof _incubating)["browserBrands"] ? ReadonlyArray<string>
    : K extends `rasm.vital.phase.${string}` | (typeof _rasm)["flagValue" | "vitalDelta" | "vitalSession"] ? number
    : string
  type Attributes = { readonly [K in Key]?: ValueOf<K> }
  type Bag = { readonly [key: string]: Value }
  type Resource = Types.Simplify<
    & { readonly [K in (typeof _attr)["deploymentEnvironment" | "serviceInstance" | "serviceName" | "serviceNamespace" | "serviceVersion"] | (typeof _incubating)["hostName"] | (typeof _rasm)["ring"]]: string }
    & { readonly [K in (typeof _attr)["k8sCluster"] | (typeof _incubating)["cloudRegion" | "cloudZone"] | (typeof _rasm)["tenant"]]?: string }
  >
  type Scope = { readonly name: `@rasm/ts/${Module}`; readonly schemaUrl: Wire["schemaUrl"]; readonly version: Identity.App.Version }
  type Translation = keyof typeof _translation
  type Wire = typeof _wire
  type Shape = Types.Simplify<{
    readonly Kind: typeof _Kind
    readonly Metric: typeof _Metric
    readonly Duration: typeof _Duration
    readonly Unit: typeof _Unit
    readonly attr: typeof _attr
    readonly bounds: (metric: MetricName<"histogram">) => ReadonlyArray<number>
    readonly dimensions: ReadonlyArray<Dimension>
    readonly domain: typeof _domain
    readonly duration: (metric: DurationMetric, span: Duration.Duration) => number
    readonly event: typeof _event
    readonly grafanaUnit: GrafanaUnit
    readonly identity: (identity: Identity.App) => Resource
    readonly incubating: typeof _incubating
    readonly instrument: typeof _instrument
    readonly keys: ReadonlyArray<Key>
    readonly metric: typeof _metric
    readonly mount: <N extends MetricName, const W extends Roster>(metric: N, ...words: Words<N, W>) => Mounted<N>
    readonly outcome: <N extends CensusMetric<"fault", "counter">, E, const W extends Roster>(
      metric: N,
      key: Fan<N>,
      census: Census<W> & Fused<W>,
      reason: (fault: E) => W[number],
    ) => <A, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
    readonly profile: typeof _profile
    readonly profiled: (identity: Identity.App) => { readonly [K in (typeof _profile)["service"]]: string }
    readonly quantiles: (metric: MetricName<"summary">) => ReadonlyArray<number>
    readonly rasm: typeof _rasm
    readonly scope: (module: Module, version: Identity.App.Version) => Scope
    readonly temporal: (unit: Unit) => unit is TimeUnit
    readonly tracked: <N extends CensusMetric<"fault", "frequency">, E, const W extends Roster>(
      metric: N,
      census: Census<W>,
      reason: (fault: E) => W[number],
    ) => <A, R>(self: Effect.Effect<A, E, R>) => Effect.Effect<A, E, R>
    readonly translated: (metric: MetricName, strategy?: Translation) => string
    readonly translation: typeof _translation
    readonly unit: typeof _unit
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
  type _InstrumentRows<
    T extends {
      readonly [K in keyof typeof _metric]:
        & { readonly description: string; readonly kind: InstrumentKind; readonly name: (typeof _metric)[K]; readonly unit: Unit }
        & ((typeof _instrument)[K]["kind"] extends "histogram" ? { readonly bounds: Bounds } : { readonly bounds?: never })
        & ((typeof _instrument)[K]["kind"] extends "summary" ? { readonly window: Window } : { readonly window?: never })
        & ((typeof _instrument)[K]["kind"] extends "frequency" | "histogram" | "summary" ? { readonly bigint?: never } : { readonly bigint?: true })
        & ((typeof _instrument)[K]["kind"] extends "frequency" ? { readonly census: CensusSource; readonly dimensions?: never }
          : { readonly census?: CensusSource; readonly dimensions?: readonly [Exclude<Key, EstateKey>, ...ReadonlyArray<Exclude<Key, EstateKey>>] })
    } = typeof _instrument,
  > = T
  type _Censuses<
    Loose extends never = {
      readonly [K in keyof typeof _instrument]: (typeof _instrument)[K] extends { readonly census: CensusSource }
        ? (typeof _instrument)[K] extends { readonly kind: "frequency" } | { readonly dimensions: ReadonlyArray<Dimension> } ? never
        : (typeof _instrument)[K]["name"]
        : never
    }[keyof typeof _instrument],
  > = Loose // a census declared on a row with neither a word axis nor a reason fan names words no exported tag can carry
  type _Named<T extends Record<MetricName, unknown> = Named> = T // the key remap covers every name: an unreachable row would leave the translation projection partial
  type _PromUnits<T extends Record<Unit, string> = typeof _promUnit> = T // totality over the unit vocabulary: an unanswered code renames its own series at the receiver
  type _GrafanaUnits<T extends Record<Unit, { readonly level: string; readonly rate: string }> = typeof _grafanaUnit> = T // totality over the same vocabulary: an unanswered code renders its panel unitless
  type _Units<Missing extends never = Exclude<Unit, Units[number]>> = Missing // roster totality follows the semantic table's own values
  type _Tails<T extends Record<InstrumentKind, { readonly dimensionless: string; readonly measured: string }> = typeof _tail> = T
  type _Temporal = { readonly [K in keyof typeof _instrument]: (typeof _instrument)[K]["unit"] extends TimeUnit ? (typeof _instrument)[K]["name"] : never }[keyof typeof _instrument]
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
  Kind: _Kind,
  Metric: _Metric,
  Duration: _Duration,
  Unit: _Unit,
  attr: _attr,
  bounds: _edges,
  dimensions: _dimensions,
  domain: _domain,
  duration: _duration,
  event: _event,
  grafanaUnit: _grafanaUnit,
  identity: _identity,
  incubating: _incubating,
  instrument: _instrument,
  keys: _keys,
  metric: _metric,
  mount: _mount,
  outcome: _outcome,
  profile: _profile,
  profiled: (identity) => ({ [_profile.service]: identity.app }),
  quantiles: _quantiles,
  rasm: _rasm,
  scope: (module, version) => ({ name: `@rasm/ts/${module}`, schemaUrl: _wire.schemaUrl, version }),
  temporal: _temporal,
  tracked: _tracked,
  translated: _translated,
  translation: _translation,
  unit: _unit,
  value: _value,
  wire: _wire,
}

export { Convention }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
