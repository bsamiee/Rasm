# [CORE_CONVENTION]

The vocabulary spine of the four-signal plane: every attribute key, metric name, event name, bounded value, and instrument measurement any telemetry node stamps is a typed row on the one `Convention` owner — semconv constants imported as literal-typed data, incubating names fenced behind a churn-absorbing alias table spanning the RUM device/session plane, the infra-correlation plane, and the feature-flag evaluation plane, Rasm-owned name families for the audit/meter/vital/slo/crash/invoke/gateway domains minted beside them, and the one `AppIdentity -> Identity` attribute projection that makes the whole plane app-parameterized. The record contract is two-sided: `Convention.Attributes` is the CLOSED stamping record — a partial record over the derived `Convention.Key` union whose per-key `ValueOf` binding holds bounded families to their own value vocabulary, so a typo, an unowned key, or an off-vocabulary bounded value fails at the stamp site — and `Convention.Bag` is the open read-side record the scrub and foreign-span inspection seams consume, because material arriving FROM a telemetry backend or a platform tracer lawfully carries keys this vocabulary never minted. `Convention.keys` is the ordered key census, and `Convention.duration` is the duration-to-instrument-unit projection guarded by the complete duration-metric row table. A string literal or call-site unit conversion at a signal site is the named defect; the row is the instruction, and its literal type flows into every attribute record, `Match` fold, and dashboard query downstream. Cross-language parity with the C# OTLP egress is name-level against the OpenTelemetry spec — the same `service.name`/`http.route`/`exception.type` spellings — so this page is the JS-side name source, never a shared artifact. The module is `core/src/observe/convention.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]             | [OWNS]                                                                                      |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `SEMCONV_ROWS`        | the stable/incubating import law, the value families, the alias seam, the deprecated ban    |
|  [02]   | `RASM_ROWS`           | the project-owned name families: identity, audit, meter, vital, slo, crash, invoke, gateway |
|  [03]   | `IDENTITY_PROJECTION` | the assembled owner: key census, closed/open record contract, `AppIdentity -> Identity`     |

## [02]-[SEMCONV_ROWS]

- Owner: the interior `_attr`/`_incubating`/`_value` anchors — flat `as const` tables whose values are `@opentelemetry/semantic-conventions` constants, so every row keeps the package's literal type and a semconv rename fails at this declaration, never at a call site.
- Packages: `@opentelemetry/semantic-conventions` (stable `.` entry) and `@opentelemetry/semantic-conventions/incubating` — pure data, zero peers, unused rows tree-shaken to zero bytes; the one `@opentelemetry/*` admission in `core`, while the SDK and exporter machinery stays fenced to the runtime export owner.
- Law: stable-tier names are the default import — API-frozen, safe inside durable dashboards, SLO policy rows, and cross-language wire parity; an incubating name enters only through the `_incubating` alias table, whose Rasm-stable keys absorb a minor-release rename at one seam while the constant-valued rows keep the break compile-visible.
- Law: the incubating table spans the three planes its consumers own — the RUM plane (`browser.*`, `device.*`, `session.*`, `network.connection.type` for the vital and crash signals), the infra-correlation plane (`cloud.*`, `container.*`, `host.*`, `process.runtime.*`, `k8s.*` for the runtime resource and the iac correlation queries), and the flag-evaluation plane (`feature_flag.key`, `feature_flag.provider.name`, `feature_flag.result.reason` the runtime flag service stamps per evaluation) — so an enrichment consumer composes rows, never a second import of the incubating entry.
- Law: bounded attribute values import as `*_VALUE_*` rows and dispatch as discriminated values — the `_value` table carries the closed nine-method HTTP family (the spec's `_OTHER` residue row included, so a foreign verb folds to a bounded value instead of an open string), the database row, and the nine-value flag-reason family beside them; `Convention.Method` and `Convention.FlagReason` derive their unions by template extraction over the table's own keys, and a `Match` arm or vocabulary lookup keys on the row, never on a free string. The flag-reason family is the TELEMETRY value vocabulary — the codec `FlagVerdict.reason` axis is the C#-minted wire vocabulary, name-level adjacent (`targeting` on the wire, `targeting_match` on the spec), and the runtime flag owner maps wire row to spec row at its stamp site, never here.
- Law: the deprecated `SEMATTRS_*`/`SEMRESATTRS_*` enum-object spelling is banned — one vocabulary, one flat-constant form; a row referencing the legacy objects is the drift defect. A member-level `@deprecated` constant (`ATTR_EXCEPTION_ESCAPED`) earns no row on the same ground.
- Law: the data stores never reach these rows for emission — `@effect/sql` stamps its own `db.*` spans — so the `db` rows here exist for `board#QUERY` construction only, and an emission-side re-stamp is the duplication defect.
- Growth: a new convention is one row in the owning family table — attribute, metric, event, or value — never a new export, file, or parallel constant.

```typescript signature
import {
  ATTR_CLIENT_ADDRESS,
  ATTR_DB_SYSTEM_NAME,
  ATTR_DEPLOYMENT_ENVIRONMENT_NAME,
  ATTR_ERROR_TYPE,
  ATTR_EXCEPTION_MESSAGE,
  ATTR_EXCEPTION_STACKTRACE,
  ATTR_EXCEPTION_TYPE,
  ATTR_HTTP_REQUEST_METHOD,
  ATTR_HTTP_RESPONSE_STATUS_CODE,
  ATTR_HTTP_ROUTE,
  ATTR_SERVER_ADDRESS,
  ATTR_SERVICE_INSTANCE_ID,
  ATTR_SERVICE_NAME,
  ATTR_SERVICE_NAMESPACE,
  ATTR_SERVICE_VERSION,
  ATTR_URL_FULL,
  ATTR_USER_AGENT_ORIGINAL,
  DB_SYSTEM_NAME_VALUE_POSTGRESQL,
  EVENT_EXCEPTION,
  HTTP_REQUEST_METHOD_VALUE_CONNECT,
  HTTP_REQUEST_METHOD_VALUE_DELETE,
  HTTP_REQUEST_METHOD_VALUE_GET,
  HTTP_REQUEST_METHOD_VALUE_HEAD,
  HTTP_REQUEST_METHOD_VALUE_OPTIONS,
  HTTP_REQUEST_METHOD_VALUE_OTHER,
  HTTP_REQUEST_METHOD_VALUE_PATCH,
  HTTP_REQUEST_METHOD_VALUE_POST,
  HTTP_REQUEST_METHOD_VALUE_PUT,
  HTTP_REQUEST_METHOD_VALUE_TRACE,
  METRIC_HTTP_SERVER_REQUEST_DURATION,
} from "@opentelemetry/semantic-conventions"
import {
  ATTR_BROWSER_BRANDS,
  ATTR_BROWSER_LANGUAGE,
  ATTR_BROWSER_MOBILE,
  ATTR_BROWSER_PLATFORM,
  ATTR_CLOUD_AVAILABILITY_ZONE,
  ATTR_CLOUD_PROVIDER,
  ATTR_CLOUD_REGION,
  ATTR_CONTAINER_ID,
  ATTR_CONTAINER_IMAGE_NAME,
  ATTR_DEVICE_MANUFACTURER,
  ATTR_DEVICE_MODEL_IDENTIFIER,
  ATTR_FEATURE_FLAG_KEY,
  ATTR_FEATURE_FLAG_PROVIDER_NAME,
  ATTR_FEATURE_FLAG_RESULT_REASON,
  ATTR_HOST_ARCH,
  ATTR_HOST_NAME,
  ATTR_K8S_CLUSTER_NAME,
  ATTR_K8S_DEPLOYMENT_NAME,
  ATTR_K8S_NAMESPACE_NAME,
  ATTR_K8S_NODE_NAME,
  ATTR_K8S_POD_NAME,
  ATTR_NETWORK_CONNECTION_TYPE,
  ATTR_PROCESS_PID,
  ATTR_PROCESS_RUNTIME_NAME,
  ATTR_PROCESS_RUNTIME_VERSION,
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
} from "@opentelemetry/semantic-conventions/incubating"
import { Array, Duration, Option, Record, type Types } from "effect"
import type { AppIdentity } from "../value/identity.ts"

const _attr = {
  clientAddress: ATTR_CLIENT_ADDRESS,
  dbSystem: ATTR_DB_SYSTEM_NAME,
  deploymentEnvironment: ATTR_DEPLOYMENT_ENVIRONMENT_NAME,
  errorType: ATTR_ERROR_TYPE,
  exceptionMessage: ATTR_EXCEPTION_MESSAGE,
  exceptionStacktrace: ATTR_EXCEPTION_STACKTRACE,
  exceptionType: ATTR_EXCEPTION_TYPE,
  httpMethod: ATTR_HTTP_REQUEST_METHOD,
  httpRoute: ATTR_HTTP_ROUTE,
  httpStatus: ATTR_HTTP_RESPONSE_STATUS_CODE,
  serverAddress: ATTR_SERVER_ADDRESS,
  serviceInstance: ATTR_SERVICE_INSTANCE_ID,
  serviceName: ATTR_SERVICE_NAME,
  serviceNamespace: ATTR_SERVICE_NAMESPACE,
  serviceVersion: ATTR_SERVICE_VERSION,
  urlFull: ATTR_URL_FULL,
  userAgent: ATTR_USER_AGENT_ORIGINAL,
} as const

const _incubating = {
  browserBrands: ATTR_BROWSER_BRANDS,
  browserLanguage: ATTR_BROWSER_LANGUAGE,
  browserMobile: ATTR_BROWSER_MOBILE,
  browserPlatform: ATTR_BROWSER_PLATFORM,
  cloudProvider: ATTR_CLOUD_PROVIDER,
  cloudRegion: ATTR_CLOUD_REGION,
  cloudZone: ATTR_CLOUD_AVAILABILITY_ZONE,
  connectionType: ATTR_NETWORK_CONNECTION_TYPE,
  containerId: ATTR_CONTAINER_ID,
  containerImage: ATTR_CONTAINER_IMAGE_NAME,
  deviceMaker: ATTR_DEVICE_MANUFACTURER,
  deviceModel: ATTR_DEVICE_MODEL_IDENTIFIER,
  flagKey: ATTR_FEATURE_FLAG_KEY,
  flagProvider: ATTR_FEATURE_FLAG_PROVIDER_NAME,
  flagReason: ATTR_FEATURE_FLAG_RESULT_REASON,
  hostArch: ATTR_HOST_ARCH,
  hostName: ATTR_HOST_NAME,
  k8sCluster: ATTR_K8S_CLUSTER_NAME,
  k8sDeployment: ATTR_K8S_DEPLOYMENT_NAME,
  k8sNamespace: ATTR_K8S_NAMESPACE_NAME,
  k8sNode: ATTR_K8S_NODE_NAME,
  k8sPodName: ATTR_K8S_POD_NAME,
  processPid: ATTR_PROCESS_PID,
  runtimeName: ATTR_PROCESS_RUNTIME_NAME,
  runtimeVersion: ATTR_PROCESS_RUNTIME_VERSION,
  sessionId: ATTR_SESSION_ID,
  sessionPrevious: ATTR_SESSION_PREVIOUS_ID,
} as const

const _value = {
  dbPostgres: DB_SYSTEM_NAME_VALUE_POSTGRESQL,
  flagCached: FEATURE_FLAG_RESULT_REASON_VALUE_CACHED,
  flagDefault: FEATURE_FLAG_RESULT_REASON_VALUE_DEFAULT,
  flagDisabled: FEATURE_FLAG_RESULT_REASON_VALUE_DISABLED,
  flagError: FEATURE_FLAG_RESULT_REASON_VALUE_ERROR,
  flagSplit: FEATURE_FLAG_RESULT_REASON_VALUE_SPLIT,
  flagStale: FEATURE_FLAG_RESULT_REASON_VALUE_STALE,
  flagStatic: FEATURE_FLAG_RESULT_REASON_VALUE_STATIC,
  flagTargeting: FEATURE_FLAG_RESULT_REASON_VALUE_TARGETING_MATCH,
  flagUnknown: FEATURE_FLAG_RESULT_REASON_VALUE_UNKNOWN,
  methodConnect: HTTP_REQUEST_METHOD_VALUE_CONNECT,
  methodDelete: HTTP_REQUEST_METHOD_VALUE_DELETE,
  methodGet: HTTP_REQUEST_METHOD_VALUE_GET,
  methodHead: HTTP_REQUEST_METHOD_VALUE_HEAD,
  methodOptions: HTTP_REQUEST_METHOD_VALUE_OPTIONS,
  methodOther: HTTP_REQUEST_METHOD_VALUE_OTHER,
  methodPatch: HTTP_REQUEST_METHOD_VALUE_PATCH,
  methodPost: HTTP_REQUEST_METHOD_VALUE_POST,
  methodPut: HTTP_REQUEST_METHOD_VALUE_PUT,
  methodTrace: HTTP_REQUEST_METHOD_VALUE_TRACE,
} as const
```

## [03]-[RASM_ROWS]

- Owner: the `_rasm`, `_metric`, `_instrument`, and `_event` anchors — the project's own name space beside the semconv imports; `_instrument` correlates every metric name with its instrument kind and unit, including the derivative queue/active pressure gauges, so SLI admission and dashboard units derive from the declaration instead of accepting any metric in any role.
- Law: only genuinely-Rasm vocabulary earns a row — the audit actor/action/target/retention axes, the meter resource/surface axes, the vital kind/grade axes, the slo objective/severity/burn axes, the crash hop/fingerprint/kind and breadcrumb names, the deployment `ring` exposure axis the identity projection stamps, and the invoke/gateway plane axes (`lane`, `method`, `service`, `verb`, `outcome`) the capability instruments tag on — and re-declaring an OTel standard name as a Rasm row is the rejected duplication; when semconv later promotes an equivalent stable name, the row's value migrates to the imported constant and every consumer follows through the literal type.
- Law: metric names are rows exactly like attribute keys — the runtime fact-stream and vital owners declare their instruments against `_metric` rows, `slo#OBJECTIVE` objectives reference the same rows, `invoke#CAPABILITY_BIND` and `invoke#COMMAND_GATEWAY` declare the capability-plane instruments against the invoke/gateway rows, and `board#QUERY` queries build from them, so one rename propagates the whole plane at compile time.
- Law: outcome and reason tag VALUES stay bounded — the invoke outcome vocabulary is the `Exit`-fold union the emitting page anchors, the fault-reason vocabulary is the codec `Hops` row set — so a Rasm tag key here never carries an identifier-grade value; identifier context rides span attributes and log annotations.
- Growth: a new metered resource, vital kind, audit axis, or capability-plane dimension lands as one row here plus its consuming row on the owning signal page.

```typescript signature
const _rasm = {
  auditAction: "rasm.audit.action",
  auditActorKey: "rasm.audit.actor.key",
  auditActorKind: "rasm.audit.actor.kind",
  auditRetention: "rasm.audit.retention",
  auditTargetKey: "rasm.audit.target.key",
  auditTargetKind: "rasm.audit.target.kind",
  crashFingerprint: "rasm.crash.fingerprint",
  crashHop: "rasm.crash.hop",
  crashKind: "rasm.crash.kind",
  gatewayOutcome: "rasm.gateway.outcome",
  gatewayVerb: "rasm.gateway.verb",
  invokeLane: "rasm.invoke.lane",
  invokeMethod: "rasm.invoke.method",
  invokeOutcome: "rasm.invoke.outcome",
  invokeService: "rasm.invoke.service",
  meterResource: "rasm.meter.resource",
  meterSurface: "rasm.meter.surface",
  ring: "rasm.ring",
  sloBurn: "rasm.slo.burn",
  sloObjective: "rasm.slo.objective",
  sloSeverity: "rasm.slo.severity",
  tenant: "rasm.tenant",
  vitalGrade: "rasm.vital.grade",
  vitalKind: "rasm.vital.kind",
} as const

const _metric = {
  crashCaptured: "rasm.crash.captured",
  derivativeActive: "rasm.derivative.active",
  derivativeQueued: "rasm.derivative.queued",
  factDrained: "rasm.fact.drained",
  gatewayCommands: "rasm.gateway.commands",
  gatewayDuration: "rasm.gateway.duration",
  httpServerDuration: METRIC_HTTP_SERVER_REQUEST_DURATION,
  invokeCalls: "rasm.invoke.calls",
  invokeDuration: "rasm.invoke.duration",
  invokeFault: "rasm.invoke.fault",
  meterUsage: "rasm.meter.usage",
  vitalLevel: "rasm.vital.level",
  vitalObserved: "rasm.vital.observed",
} as const

const _instrument = {
  crashCaptured: { kind: "counter", measure: "count", name: _metric.crashCaptured, unit: "1" },
  derivativeActive: { kind: "gauge", measure: "count", name: _metric.derivativeActive, unit: "1" },
  derivativeQueued: { kind: "gauge", measure: "count", name: _metric.derivativeQueued, unit: "1" },
  factDrained: { kind: "counter", measure: "count", name: _metric.factDrained, unit: "1" },
  gatewayCommands: { kind: "counter", measure: "count", name: _metric.gatewayCommands, unit: "1" },
  gatewayDuration: { kind: "histogram", measure: "duration", name: _metric.gatewayDuration, unit: "ms" },
  httpServerDuration: { kind: "histogram", measure: "duration", name: _metric.httpServerDuration, unit: "s" },
  invokeCalls: { kind: "counter", measure: "count", name: _metric.invokeCalls, unit: "1" },
  invokeDuration: { kind: "histogram", measure: "duration", name: _metric.invokeDuration, unit: "ms" },
  invokeFault: { kind: "frequency", measure: "count", name: _metric.invokeFault, unit: "1" },
  meterUsage: { kind: "counter", measure: "count", name: _metric.meterUsage, unit: "1" },
  vitalLevel: { kind: "gauge", measure: "level", name: _metric.vitalLevel, unit: "1" },
  vitalObserved: { kind: "counter", measure: "count", name: _metric.vitalObserved, unit: "1" },
} as const

const _durationScale = {
  [_metric.gatewayDuration]: 1,
  [_metric.httpServerDuration]: 1 / 1000,
  [_metric.invokeDuration]: 1,
} as const

const _event = {
  breadcrumb: "rasm.crash.breadcrumb",
  exception: EVENT_EXCEPTION,
} as const
```

## [04]-[IDENTITY_PROJECTION]

- Owner: the assembled `Convention` export — row families as properties, the ordered `keys` census, the `identity` projection, and the duration-to-instrument-unit projection, with companion types on the merged namespace and contract guards riding the hub so a malformed row fails at this declaration with zero widening of the interior anchors.
- Law: the record contract is two-sided by trust direction — `Convention.Attributes` is the closed stamping record whose `ValueOf` binds method, flag reason, database system, status/pid number, browser-mobile boolean, and the remaining string rows at their own keys; `Convention.Bag` is the open read-side record and admits scalar arrays because foreign OpenTelemetry attributes lawfully carry them. A signal site writing through `Bag`, or a scrub seam demanding `Attributes`, is the inverted-trust defect.
- Law: `Convention.keys` is the one iteration anchor — the ordered census over every attribute row (`_attr`, `_incubating`, `_rasm` in table order), so a render fold probing a closed record walks the vocabulary and emits pairs in canonical order; an `Object.keys` walk over a stamped record re-imports per-record insertion order and is the deleted spelling.
- Law: `Convention.duration(metric, span)` is the one duration conversion — `_durationScale` is a mapped record over every duration histogram name, so adding a duration instrument without its canonical millisecond multiplier breaks at the declaration; SLO and rule compilers consume this projection and never assume every histogram is measured in seconds.
- Law: `Convention.identity` is the single eleven-dimension `AppIdentity -> Identity` correspondence — `service.name` carries the app key, `service.version` the build version, `service.instance.id` the boot-minted instance guid, `deployment.environment.name` the environment tier, `host.name` the host print through the incubating alias row, and `rasm.ring` the exposure rung, while `service.namespace`, `cloud.region`, `cloud.availability_zone`, `k8s.cluster.name`, and `rasm.tenant` stamp only when the identity pins the dimension: a multi-tenant process emits no resource-level tenant and per-fact tenancy rides the audit/meter streams — and every identity-bearing surface (the runtime OTLP `Resource`, the fact stamps on the runtime signal streams, the dashboard identity on `board#MODEL`) derives from this one projection, so a per-app telemetry fork is structurally impossible.
- Law: a dimension is projected only once the identity value settles it — the projection line lands in the same edit that adds the identity field, never ahead of it — so the projection reads settled `instance`/`namespace`/`environment`/`ring`/`region`/`zone`/`cluster` values and never re-mints a dimension the floor owns; an absent `Option` dimension is omission, never a sentinel, so a backend filter never matches an empty string.
- Boundary: `AppIdentity` is `value/identity`'s value — this page projects it into attribute space and owns nothing about its construction; `value/fault` imports `ATTR_EXCEPTION_*`/`ATTR_ERROR_TYPE` directly for its forensic anchors — a shared-import boundary, two owners over one spec vocabulary, never a re-export hop.
- Entry: `Convention.identity(identity)` — the one operation; row families read as properties (`Convention.attr.httpRoute`, `Convention.metric.meterUsage`, `Convention.instrument.meterUsage`, `Convention.event.exception`, `Convention.value.methodGet`); `Convention.keys` at every iteration seam.
- Growth: a new identity dimension is one projection line plus its `_rasm` row; a new bounded-value binding is one `ValueOf` clause beside its `_value` rows.

```typescript signature
const _keys: ReadonlyArray<Convention.Key> = [
  ...Record.values(_attr),
  ...Record.values(_incubating),
  ...Record.values(_rasm),
]

const _identity = (identity: AppIdentity): Convention.Identity => ({
  [_attr.deploymentEnvironment]: identity.environment,
  [_attr.serviceInstance]: identity.instance,
  [_attr.serviceName]: identity.app,
  [_attr.serviceVersion]: identity.build.version,
  [_incubating.hostName]: identity.host,
  [_rasm.ring]: identity.ring,
  ...Option.match(identity.namespace, {
    onNone: () => ({}),
    onSome: (namespace) => ({ [_attr.serviceNamespace]: namespace }),
  }),
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
    onSome: (cluster) => ({ [_incubating.k8sCluster]: cluster }),
  }),
  ...Option.match(identity.tenant, {
    onNone: () => ({}),
    onSome: (tenant) => ({ [_rasm.tenant]: tenant }),
  }),
})

declare namespace Convention {
  type Attr = keyof typeof _attr
  type Key =
    | (typeof _attr)[Attr]
    | (typeof _incubating)[keyof typeof _incubating]
    | (typeof _rasm)[keyof typeof _rasm]
  type Method = (typeof _value)[Extract<keyof typeof _value, `method${string}`>]
  type FlagReason = (typeof _value)[Extract<keyof typeof _value, `flag${string}`>]
  type InstrumentKind = (typeof _instrument)[keyof typeof _instrument]["kind"]
  type Measure = (typeof _instrument)[keyof typeof _instrument]["measure"]
  type MetricName<K extends InstrumentKind = InstrumentKind> = {
    readonly [N in keyof typeof _instrument]: (typeof _instrument)[N]["kind"] extends K ? (typeof _instrument)[N]["name"] : never
  }[keyof typeof _instrument]
  type DurationMetric = keyof typeof _durationScale
  type EventName = (typeof _event)[keyof typeof _event]
  type Scalar = string | number | boolean
  type Value = Scalar | ReadonlyArray<Scalar>
  type ValueOf<K extends Key> = K extends (typeof _attr)["httpMethod"] ? Method
    : K extends (typeof _incubating)["flagReason"] ? FlagReason
    : K extends (typeof _attr)["dbSystem"] ? (typeof _value)["dbPostgres"]
    : K extends (typeof _attr)["httpStatus"] | (typeof _incubating)["processPid"] ? number
    : K extends (typeof _incubating)["browserMobile"] ? boolean
    : K extends (typeof _incubating)["browserBrands"] ? ReadonlyArray<string>
    : string
  type Attributes = { readonly [K in Key]?: ValueOf<K> }
  type Bag = { readonly [key: string]: Value }
  type Identity = Types.Simplify<
    & { readonly [K in (typeof _attr)["deploymentEnvironment" | "serviceInstance" | "serviceName" | "serviceVersion"] | (typeof _incubating)["hostName"] | (typeof _rasm)["ring"]]: string }
    & { readonly [K in (typeof _attr)["serviceNamespace"] | (typeof _incubating)["cloudRegion" | "cloudZone" | "k8sCluster"] | (typeof _rasm)["tenant"]]?: string }
  >
  type Shape = Types.Simplify<{
    readonly attr: typeof _attr
    readonly duration: (metric: DurationMetric, span: Duration.Duration) => number
    readonly event: typeof _event
    readonly identity: (identity: AppIdentity) => Identity
    readonly incubating: typeof _incubating
    readonly instrument: typeof _instrument
    readonly keys: ReadonlyArray<Key>
    readonly metric: typeof _metric
    readonly rasm: typeof _rasm
    readonly value: typeof _value
  }>
  type _Attr<T extends Record<Attr, string> = typeof _attr> = T
  type _InstrumentRows<
    T extends { readonly [K in keyof typeof _metric]: { readonly kind: "counter" | "frequency" | "gauge" | "histogram"; readonly measure: "count" | "duration" | "level"; readonly name: (typeof _metric)[K]; readonly unit: string } } = typeof _instrument,
  > = T
  type _DurationRows<T extends Record<MetricName<"histogram">, number> = typeof _durationScale> = T
  type _Rasm<T extends Record<keyof typeof _rasm, `rasm.${string}`> = typeof _rasm> = T
}

const Convention: Convention.Shape = {
  attr: _attr,
  duration: (metric, span) => Duration.toMillis(span) * _durationScale[metric],
  event: _event,
  identity: _identity,
  incubating: _incubating,
  instrument: _instrument,
  keys: _keys,
  metric: _metric,
  rasm: _rasm,
  value: _value,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Convention }
```
