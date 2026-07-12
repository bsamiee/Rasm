# [CORE_CONVENTION]

The vocabulary spine of the four-signal plane: every attribute key, metric name, event name, and bounded value any telemetry node stamps is a typed row on the one `Convention` owner — semconv constants imported as literal-typed data, incubating names fenced behind a churn-absorbing alias table spanning the RUM device/session plane and the infra-correlation plane, Rasm-owned name families for the audit/meter/vital/slo/crash/invoke/gateway domains minted beside them, and the one `AppIdentity -> Identity` attribute projection that makes the whole plane app-parameterized. A string literal at a signal site is the named defect; the row is the instruction, and its literal type flows into every attribute record, `Match` fold, and dashboard query downstream. Cross-language parity with the C# OTLP egress is name-level against the OpenTelemetry spec — the same `service.name`/`http.route`/`exception.type` spellings — so this page is the JS-side name source, never a shared artifact. The module is `core/src/observe/convention.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]             | [OWNS]                                                                                      |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `SEMCONV_ROWS`        | the stable/incubating import law, the value families, the alias seam, the deprecated ban    |
|  [02]   | `RASM_ROWS`           | the project-owned name families: identity, audit, meter, vital, slo, crash, invoke, gateway |
|  [03]   | `IDENTITY_PROJECTION` | `AppIdentity -> Identity` attributes — the one identity spine every surface derives from    |

## [02]-[SEMCONV_ROWS]

[SEMCONV_ROWS]:
- Owner: the interior `_attr`/`_incubating`/`_value` anchors — flat `as const` tables whose values are `@opentelemetry/semantic-conventions` constants, so every row keeps the package's literal type and a semconv rename fails at this declaration, never at a call site.
- Packages: `@opentelemetry/semantic-conventions` (stable `.` entry) and `@opentelemetry/semantic-conventions/incubating` — pure data, zero peers, unused rows tree-shaken to zero bytes; the one `@opentelemetry/*` admission in `core`, while the SDK and exporter machinery stays fenced to the runtime export owner.
- Law: stable-tier names are the default import — API-frozen, safe inside durable dashboards, SLO policy rows, and cross-language wire parity; an incubating name enters only through the `_incubating` alias table, whose Rasm-stable keys absorb a minor-release rename at one seam while the constant-valued rows keep the break compile-visible.
- Law: the incubating table spans the two planes its consumers own — the RUM plane (`browser.*`, `device.*`, `session.*`, `network.connection.type` for the vital and crash signals) and the infra-correlation plane (`cloud.*`, `container.*`, `host.*`, `process.runtime.*`, `k8s.*` for the runtime resource and the iac correlation queries) — so an enrichment consumer composes rows, never a second import of the incubating entry.
- Law: bounded attribute values import as `*_VALUE_*` rows and dispatch as discriminated values — the `_value` table carries the closed nine-method HTTP family (the spec's `_OTHER` residue row included, so a foreign verb folds to a bounded value instead of an open string) beside the database row, `Convention.Method` derives the method union by template extraction over the table's own keys, and a `Match` arm or vocabulary lookup keys on the row, never on a free string.
- Law: the deprecated `SEMATTRS_*`/`SEMRESATTRS_*` enum-object spelling is banned — one vocabulary, one flat-constant form; a row referencing the legacy objects is the drift defect.
- Law: the data stores never reach these rows for emission — `@effect/sql` stamps its own `db.*` spans — so the `db` rows here exist for `board#QUERY` construction only, and an emission-side re-stamp is the duplication defect.
- Growth: a new convention is one row in the owning family table — attribute, metric, event, or value — never a new export, file, or parallel constant.

```typescript
import {
  ATTR_CLIENT_ADDRESS,
  ATTR_DB_SYSTEM_NAME,
  ATTR_DEPLOYMENT_ENVIRONMENT_NAME,
  ATTR_ERROR_TYPE,
  ATTR_EXCEPTION_ESCAPED,
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
  ATTR_HOST_ARCH,
  ATTR_HOST_NAME,
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
} from "@opentelemetry/semantic-conventions/incubating"

const _attr = {
  clientAddress: ATTR_CLIENT_ADDRESS,
  dbSystem: ATTR_DB_SYSTEM_NAME,
  deploymentEnvironment: ATTR_DEPLOYMENT_ENVIRONMENT_NAME,
  errorType: ATTR_ERROR_TYPE,
  exceptionEscaped: ATTR_EXCEPTION_ESCAPED,
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
  hostArch: ATTR_HOST_ARCH,
  hostName: ATTR_HOST_NAME,
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

[RASM_ROWS]:
- Owner: the `_rasm`, `_metric`, and `_event` anchors — the project's own name space beside the semconv imports, spelled `rasm.<domain>.<axis>` so a Rasm attribute never collides with a spec namespace and a backend filters the project plane in one prefix.
- Law: only genuinely-Rasm vocabulary earns a row — the audit actor/action/target/retention axes, the meter resource/surface axes, the vital kind/grade axes, the slo objective/severity/burn axes, the crash hop/fingerprint/kind and breadcrumb names, the deployment `ring` exposure axis the identity projection stamps, and the invoke/gateway plane axes (`lane`, `method`, `service`, `verb`, `outcome`) the capability instruments tag on — and re-declaring an OTel standard name as a Rasm row is the rejected duplication; when semconv later promotes an equivalent stable name, the row's value migrates to the imported constant and every consumer follows through the literal type.
- Law: metric names are rows exactly like attribute keys — the runtime fact-stream and vital owners declare their instruments against `_metric` rows, `slo#OBJECTIVE` objectives reference the same rows, `invoke#CAPABILITY_BIND` and `invoke#COMMAND_GATEWAY` declare the capability-plane instruments against the invoke/gateway rows, and `board#QUERY` queries build from them, so one rename propagates the whole plane at compile time.
- Law: outcome and reason tag VALUES stay bounded — the invoke outcome vocabulary is the `Exit`-fold union the emitting page anchors, the fault-reason vocabulary is the codec `Hops` row set — so a Rasm tag key here never carries an identifier-grade value; identifier context rides span attributes and log annotations.
- Growth: a new metered resource, vital kind, audit axis, or capability-plane dimension lands as one row here plus its consuming row on the owning signal page.

```typescript
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

const _event = {
  breadcrumb: "rasm.crash.breadcrumb",
  exception: EVENT_EXCEPTION,
} as const
```

## [04]-[IDENTITY_PROJECTION]

[IDENTITY_PROJECTION]:
- Owner: the assembled `Convention` export — row families as properties, the `identity` projection as the one operation, companion types on the merged namespace, contract guards riding the hub so a malformed row fails at this declaration with zero widening of the interior anchors.
- Law: `Convention.identity` is the single nine-dimension `AppIdentity -> Identity` correspondence — `service.name` carries the app key, `service.version` the build version, `service.instance.id` the boot-minted instance guid, `deployment.environment.name` the environment tier, `host.name` the host print through the incubating alias row, and `rasm.ring` the exposure rung, while `service.namespace`, `cloud.region`, and `rasm.tenant` stamp only when the identity pins the dimension: a multi-tenant process emits no resource-level tenant and per-fact tenancy rides the audit/meter streams — and every identity-bearing surface (the runtime OTLP `Resource`, the fact stamps on the runtime signal streams, the dashboard identity on `board#MODEL`) derives from this one projection, so a per-app telemetry fork is structurally impossible.
- Law: a dimension is projected only once the identity value settles it — the projection line lands in the same edit that adds the identity field, never ahead of it — so the projection reads settled `instance`/`namespace`/`environment`/`ring`/`region` values and never re-mints a dimension the floor owns; an absent `Option` dimension is omission, never a sentinel, so a backend filter never matches an empty string.
- Boundary: `AppIdentity` is `value/identity`'s value — this page projects it into attribute space and owns nothing about its construction.
- Entry: `Convention.identity(identity)` — the one operation; row families read as properties (`Convention.attr.httpRoute`, `Convention.metric.meterUsage`, `Convention.event.exception`, `Convention.value.methodGet`).
- Growth: a new identity dimension is one projection line plus its `_rasm` row.

```typescript
import { Option, type Types } from "effect"
import type { AppIdentity } from "../value/identity.ts"

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
  type MetricName = (typeof _metric)[keyof typeof _metric]
  type EventName = (typeof _event)[keyof typeof _event]
  type Value = string | number | boolean
  type Attributes = { readonly [key: string]: Value }
  type Identity = Types.Simplify<
    & { readonly [K in (typeof _attr)["deploymentEnvironment" | "serviceInstance" | "serviceName" | "serviceVersion"] | (typeof _incubating)["hostName"] | (typeof _rasm)["ring"]]: string }
    & { readonly [K in (typeof _attr)["serviceNamespace"] | (typeof _incubating)["cloudRegion"] | (typeof _rasm)["tenant"]]?: string }
  >
  type Shape = Types.Simplify<{
    readonly attr: typeof _attr
    readonly event: typeof _event
    readonly identity: (identity: AppIdentity) => Identity
    readonly incubating: typeof _incubating
    readonly metric: typeof _metric
    readonly rasm: typeof _rasm
    readonly value: typeof _value
  }>
  type _Attr<T extends Record<Attr, string> = typeof _attr> = T
  type _Rasm<T extends Record<keyof typeof _rasm, `rasm.${string}`> = typeof _rasm> = T
}

const Convention: Convention.Shape = {
  attr: _attr,
  event: _event,
  identity: _identity,
  incubating: _incubating,
  metric: _metric,
  rasm: _rasm,
  value: _value,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Convention }
```
