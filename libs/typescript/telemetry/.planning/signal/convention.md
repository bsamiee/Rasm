# [TELEMETRY_CONVENTION]

The vocabulary spine of the four-signal plane: every attribute key, metric name, event name, and bounded value any telemetry node stamps is a typed row on the one `Convention` owner — semconv constants imported as literal-typed data, incubating names fenced behind a churn-absorbing alias table, Rasm-owned name families for the audit/meter/vital/slo/crash domains minted beside them, and the one `AppIdentity -> Identity` attribute projection that makes the whole plane app-parameterized. A string literal at a signal site is the named defect; the row is the instruction, and its literal type flows into every attribute record, `Match` fold, and dashboard query downstream. Cross-language parity with the C# OTLP egress is name-level against the OpenTelemetry spec — the same `service.name`/`http.route`/`exception.type` spellings — so this page is the JS-side name source, never a shared artifact.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]             | [OWNS]                                                                                  |
| :-----: | :-------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | [SEMCONV_ROWS]        | the stable/incubating import law, the alias seam, the deprecated-spelling ban            |
|  [02]   | [RASM_ROWS]           | the project-owned name families: identity, audit, meter, vital, slo, crash               |
|  [03]   | [IDENTITY_PROJECTION] | `AppIdentity -> Identity` attributes — the one identity spine every surface derives from |

## [2]-[SEMCONV_ROWS]

[SEMCONV_ROWS]:
- Owner: the interior `_attr`/`_incubating`/`_value` anchors — flat `as const` tables whose values are `@opentelemetry/semantic-conventions` constants, so every row keeps the package's literal type and a semconv rename fails at this declaration, never at a call site.
- Packages: `@opentelemetry/semantic-conventions` (stable `.` entry) and `@opentelemetry/semantic-conventions/incubating` — the sole `[R3]` survivor; pure data, zero peers, unused rows tree-shaken to zero bytes.
- Law: stable-tier names are the default import — API-frozen, safe inside durable dashboards, SLO policy rows, and cross-language wire parity; an incubating name enters only through the `_incubating` alias table, whose Rasm-stable keys absorb a minor-release rename at one seam while the constant-valued rows keep the break compile-visible.
- Law: bounded attribute values import as `*_VALUE_*` rows and dispatch as discriminated values — a `Match` arm or vocabulary lookup keys on the row, never on a free string.
- Law: the deprecated `SEMATTRS_*`/`SEMRESATTRS_*` enum-object spelling is banned — one vocabulary, one flat-constant form; a row referencing the legacy objects is the drift defect.
- Law: consumers are fenced — `@opentelemetry/*` resolves only inside `scope:telemetry`, so every folder whose ledger row admits `telemetry` reaches names through this owner; `store` cannot import `telemetry`, and its `db.*` spans are stamped by `@effect/sql`'s own tracing — the `db` rows here exist for `board` query construction, never for `store` emission.
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
  METRIC_HTTP_SERVER_REQUEST_DURATION,
} from "@opentelemetry/semantic-conventions"
import {
  ATTR_BROWSER_BRANDS,
  ATTR_BROWSER_LANGUAGE,
  ATTR_BROWSER_MOBILE,
  ATTR_BROWSER_PLATFORM,
  ATTR_HOST_NAME,
  ATTR_K8S_POD_NAME,
  ATTR_PROCESS_PID,
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
  hostName: ATTR_HOST_NAME,
  k8sPodName: ATTR_K8S_POD_NAME,
  processPid: ATTR_PROCESS_PID,
} as const

const _value = {
  dbPostgres: DB_SYSTEM_NAME_VALUE_POSTGRESQL,
} as const
```

## [3]-[RASM_ROWS]

[RASM_ROWS]:
- Owner: the `_rasm`, `_metric`, and `_event` anchors — the project's own name space beside the semconv imports, spelled `rasm.<domain>.<axis>` so a Rasm attribute never collides with a spec namespace and a backend filters the project plane in one prefix.
- Law: only genuinely-Rasm vocabulary earns a row — the audit actor/action/target/retention axes, the meter resource/surface axes, the vital kind/grade axes, the slo objective/severity/burn axes, the crash hop and breadcrumb names — and re-declaring an OTel standard name as a Rasm row is the rejected duplication; when semconv later promotes an equivalent stable name, the row's value migrates to the imported constant and every consumer follows through the literal type.
- Law: metric names are rows exactly like attribute keys — `signal/meter`, `signal/vital`, `signal/audit`, and `signal/crash` declare their instruments against `_metric` rows, `slo/burnrate` objectives reference the same rows, and `board` queries build from them, so one rename propagates the whole plane at compile time.
- Growth: a new metered resource, vital kind, or audit axis lands as one row here plus its consuming row on the owning signal page.

```typescript
const _rasm = {
  auditAction: "rasm.audit.action",
  auditActorKey: "rasm.audit.actor.key",
  auditActorKind: "rasm.audit.actor.kind",
  auditRetention: "rasm.audit.retention",
  auditTargetKey: "rasm.audit.target.key",
  auditTargetKind: "rasm.audit.target.kind",
  crashHop: "rasm.crash.hop",
  meterResource: "rasm.meter.resource",
  meterSurface: "rasm.meter.surface",
  sloBurn: "rasm.slo.burn",
  sloObjective: "rasm.slo.objective",
  sloSeverity: "rasm.slo.severity",
  tenant: "rasm.tenant",
  vitalGrade: "rasm.vital.grade",
  vitalKind: "rasm.vital.kind",
} as const

const _metric = {
  auditDrained: "rasm.audit.drained",
  crashCaptured: "rasm.crash.captured",
  httpServerDuration: METRIC_HTTP_SERVER_REQUEST_DURATION,
  meterDrained: "rasm.meter.drained",
  meterUsage: "rasm.meter.usage",
  vitalLevel: "rasm.vital.level",
  vitalObserved: "rasm.vital.observed",
} as const

const _event = {
  breadcrumb: "rasm.crash.breadcrumb",
  exception: EVENT_EXCEPTION,
} as const
```

## [4]-[IDENTITY_PROJECTION]

[IDENTITY_PROJECTION]:
- Owner: the assembled `Convention` export — row families as properties, the `identity` projection as the one operation, companion types on the merged namespace, contract guards riding the hub so a malformed row fails at this declaration with zero widening of the interior anchors.
- Law: `Convention.identity` is the single `AppIdentity -> Identity` correspondence — `service.name` carries the app key, `service.version` the build version, `service.instance.id` the host print, and `rasm.tenant` stamps only when the process tenancy is pinned: a multi-tenant process emits no resource-level tenant and per-fact tenancy rides the audit/meter streams — and every identity-bearing surface (the OTLP `Resource` on `otlp/export`, the fact stamps on `signal/audit`/`signal/meter`, the dashboard identity on `board/model`) derives from this one projection, so a per-app telemetry fork is structurally impossible.
- Boundary: `AppIdentity` is `kernel/identity/appidentity`'s value — this page projects it into attribute space and owns nothing about its construction; `deployment.environment.name` is a config fact stamped by `otlp/export`'s policy, deliberately absent from the identity projection.
- Entry: `Convention.identity(identity)` — the one operation; row families read as properties (`Convention.attr.httpRoute`, `Convention.metric.meterUsage`, `Convention.event.exception`).
- Growth: a new identity dimension is one projection line plus its `_rasm` row.

```typescript
import { Option, type Types } from "effect"
import type { AppIdentity } from "@rasm/ts/kernel"

const _identity = (identity: AppIdentity): Convention.Identity => ({
  [_attr.serviceInstance]: identity.host,
  [_attr.serviceName]: identity.app,
  [_attr.serviceVersion]: identity.build.version,
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
  type MetricName = (typeof _metric)[keyof typeof _metric]
  type EventName = (typeof _event)[keyof typeof _event]
  type Value = string | number | boolean
  type Attributes = { readonly [key: string]: Value }
  type Identity = Types.Simplify<
    & { readonly [K in (typeof _attr)["serviceInstance" | "serviceName" | "serviceVersion"]]: string }
    & { readonly [K in (typeof _rasm)["tenant"]]?: string }
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
