# [RUNTIME_SERVER]

`Instrument` under the `server` condition owns node auto-instrumentation: one `_rows` roster covering the inbound and outbound HTTP surfaces, the PostgreSQL client, and engine health, one `AsyncLocalStorageContextManager` install published as `Instrument.Ambient`, and exactly one `registerInstrumentations` bound to the lane's own providers. Effect spans stay primary and these rows cover foreign libraries alone, so a client hop enriches the live span tree rather than rooting a rival trace.

Composed owners: `otel/emit#POLICY` supplies the `server` policy group every row reads and the `egress` roster the self-exclusion folds, `otel/emit#HOOKS` supplies the contributed instrumentation rows and the raw meter provider this node drains into its one activation, and `otel/emit#LANES` exposes the tracer and logger providers the requirement channel names. `crash#CAPTURE` holds the process-fatal seat this node refuses. Its module is `runtime/src/otel/server.ts`, resolved by the `server` exports condition alone.

## [01]-[INDEX]

- [02]-[REGISTRATION] — the async-local manager install, the node row roster, self-egress exclusion, and the one activation; `Instrument`.

## [02]-[REGISTRATION]

- Owner: `Instrument` carries two names because the halves carry different requirements — `Instrument.ambient` publishes the installed `ContextManager` and requires nothing, so a native-lane root composes it alone, while `Instrument.live(policy)` names `OtelTracerProvider` in its requirement channel and only an SDK lane satisfies it. Reading `_Ambient` from `R` orders the pair at the type level, so registration provably follows the manager install rather than trusting root composition order.
- Cases: `_rows` seats four node instruments — the HTTP row covering both request directions, the undici row covering `fetch`, the `pg` row, and the runtime row carrying engine series — each keyed to its own `policy.server.rows` cell.
- Law: the condition split is structural rather than disciplinary — these rows pull node-only native surfaces (`perf_hooks`, `pg` module patching), so a condition-bound side effect physically cannot load on the browser plane and the exports map is what keeps it out.
- Law: `enabled` is the zeroth column on every row, and a refused row never patches its module at all where every other per-row field tunes a row that is always on; a deployment with no Postgres stops `PgInstrumentation` from reaching the `pg` module rather than configuring a patch it does not want.
- Law: self-exclusion carries the egress classes the SDK's own suppression cannot reach. OTLP legs already suppress inside their own processors and readers, so what needs a roster is every push leaving outside one — the Pyroscope flush on its own transport, a vendor exporter, a second collector. `_egress` folds `policy.collector.baseUrl` with every `policy.egress` origin into ONE roster each hook matches against the coordinate its own family receives.
- Law: `_authority` is what makes the node client compare exact — a `RequestOptions` record splits its authority across optional `host` (which may carry a port), `hostname` (which never does), and `port`, and a URL-built request leaves `host` undefined entirely, so the pair normalizes under the package's own default-port fill and the compare runs on both halves. Undici's hook receives a scheme-qualified `origin` compared through `URL`, while the inbound hook receives a request PATH, so `policy.server.ignore` carries paths alone.
- Law: `policy.server.orphan` set false gates the client rows behind a live parent (`requireParentforSpans` on undici, `requireParentSpan` on pg), so one outbound hop yields one span from one layer. C# spells that posture as downstream suppression; these packages carry parent-presence gates instead, and the row is the gate.
- Law: database posture is four independent rows because four mechanisms answer four questions at four costs. `statement` drives `enhancedDatabaseReporting`, attaching bound parameters — identifier-grade material, so the row defaults off and a deployment enabling it inherits the export-boundary scrub as its only guard. `comment` drives `addSqlCommenterCommentToQueries`, so `pg_stat_statements` carries the trace coordinate inside the normalized statement text. `session` drives `enableTraceContextPropagation`, which issues a `SET application_name` round-trip before every user query — roughly doubling the connection's round-trips to buy `pg_stat_activity.application_name` carrying the live `traceparent` — so it stands apart from the comment and defaults off. `connect` admits the pool-acquisition span through `ignoreConnectSpans`, which a pooled workload turns off so a checkout is not a span per query.
- Law: query-parameter redaction is the HTTP row's own mechanism — the boundary scrub seals `url.full` whole, and `policy.server.redact` is the finer form a deployment keeping the URL uses, replacing the package's own default roster.
- Law: engine health rides this condition alone — `HostMetrics` under the `policy.server.engine.groups` allow-list and `RuntimeNodeInstrumentation` at the stated precision bind the raw provider `Hooks.Meter` exposes, so event-loop delay and utilization, GC duration, and V8 heap series carry the same resource identity as every span and log while the `v8js.*` attribute fan stays governed by the deny-list view row (`meter#VIEWS`). `captureUncaughtException` stays explicitly false because `crash#CAPTURE` is the branch's one process-fatal owner and a second listener mints a rival fatal record under a foreign shape.
- Exemption: the acquire body is the platform-forced registration seam — the global manager install, the `HostMetrics` start call, and the composed unload closure are the SDK's own imperative contract.
- Entry: `Instrument.ambient` merges at every root, native and SDK alike, because `emit#LANES`'s `_tracerContext` and every foreign `context.active()` read depend on the install; `Instrument.live(policy)` merges beside it under an SDK lane's `Export.live(policy)`, where a native-lane root dies at the requirement channel.
- Packages: `@opentelemetry/instrumentation` (`registerInstrumentations`, `Instrumentation`, `InstrumentationConfig.enabled`), `@opentelemetry/context-async-hooks` (`AsyncLocalStorageContextManager`), `@opentelemetry/host-metrics` + `@opentelemetry/instrumentation-runtime-node` (engine health), `@opentelemetry/instrumentation-http`, `-undici`, `-pg`, `@opentelemetry/api` (`context`, `ContextManager`), `@effect/opentelemetry` (`Tracer.OtelTracerProvider`, `Logger.OtelLoggerProvider`), `effect` (`Array`, `Context`, `pipe`), `node:http` (`RequestOptions`, type-only).
- Growth: a new node instrumentation is one `_rows` entry with its `policy.server.rows` cell and its policy fields, or one `Hooks.contribute` row from the feature plane that needs it; a new self-egress backend is one `policy.egress` origin.
- Boundary: registration composes only at a composition root beside `Export.live` — a library composing either node double-instruments its host; the browser condition's own roster and manager live at `instrument#REGISTRATION`.

```typescript signature

import { context as ambient, type ContextManager } from "@opentelemetry/api"
import { AsyncLocalStorageContextManager } from "@opentelemetry/context-async-hooks"
import { HostMetrics } from "@opentelemetry/host-metrics"
import { registerInstrumentations, type Instrumentation } from "@opentelemetry/instrumentation"
import { HttpInstrumentation } from "@opentelemetry/instrumentation-http"
import { PgInstrumentation } from "@opentelemetry/instrumentation-pg"
import { RuntimeNodeInstrumentation } from "@opentelemetry/instrumentation-runtime-node"
import { UndiciInstrumentation } from "@opentelemetry/instrumentation-undici"
import { Logger as OtelLogger, Tracer as OtelBridge } from "@effect/opentelemetry"
import { Array, Context, Effect, Layer, pipe } from "effect"
import type { RequestOptions } from "node:http"
import { type Export, Hooks } from "./emit.ts"

class _Ambient extends Context.Tag("runtime/Instrument/Ambient")<_Ambient, ContextManager>() {}

const _ambient: Layer.Layer<_Ambient> = Layer.scoped(
  _Ambient,
  Effect.acquireRelease(
    Effect.sync(() => {
      const manager = new AsyncLocalStorageContextManager().enable()
      ambient.setGlobalContextManager(manager)
      return manager
    }),
    () => Effect.sync(() => ambient.disable()),
  ),
)

const _egress = (policy: Export.Policy): ReadonlyArray<{ readonly host: string; readonly origin: string; readonly port: string }> =>
  Array.map([policy.collector.baseUrl, ...policy.egress], (raw) =>
    pipe(new URL(raw), (url) => ({
      host: url.hostname,
      origin: url.origin,
      port: url.port === "" ? (url.protocol === "https:" ? "443" : "80") : url.port,
    })))

type _Egress = ReturnType<typeof _egress>

const _authority = (request: RequestOptions): { readonly host: string; readonly port: string } =>
  pipe(request.host?.match(/^([^:/ ]+)(:\d{1,5})?/), (stated) => ({
    host: request.hostname ?? stated?.[1] ?? "localhost",
    port: String(request.port ?? stated?.[2]?.slice(1) ?? (request.protocol === "https:" ? "443" : "80")),
  }))

const _rows = (policy: Export.Policy, egress: _Egress): ReadonlyArray<Instrumentation> => [
  new HttpInstrumentation({
    enabled: policy.server.rows.http,
    ignoreIncomingRequestHook: (request) => Array.some(policy.server.ignore, (row) => (request.url ?? "").includes(row)),
    ignoreOutgoingRequestHook: (request) =>
      pipe(_authority(request), (authority) => Array.some(egress, (row) => row.host === authority.host && row.port === authority.port)),
    redactedQueryParams: [...policy.server.redact],
    requireParentforOutgoingSpans: !policy.server.orphan,
  }),
  new UndiciInstrumentation({
    enabled: policy.server.rows.undici,
    ignoreRequestHook: (request) => pipe(new URL(request.origin).origin, (origin) => Array.some(egress, (row) => row.origin === origin)),
    requireParentforSpans: !policy.server.orphan,
  }),
  new PgInstrumentation({
    addSqlCommenterCommentToQueries: policy.server.comment,
    enableTraceContextPropagation: policy.server.session,
    enabled: policy.server.rows.pg,
    enhancedDatabaseReporting: policy.server.statement,
    ignoreConnectSpans: !policy.server.connect,
    requireParentSpan: !policy.server.orphan,
  }),
  new RuntimeNodeInstrumentation({
    captureUncaughtException: false,
    enabled: policy.server.rows.runtime,
    monitoringPrecision: policy.server.engine.precision,
  }),
]

const Instrument: {
  readonly Ambient: typeof _Ambient
  readonly ambient: Layer.Layer<_Ambient>
  readonly live: (
    policy: Export.Policy,
  ) => Layer.Layer<never, never, _Ambient | Hooks | Hooks.Meter | OtelBridge.OtelTracerProvider | OtelLogger.OtelLoggerProvider>
} = {
  Ambient: _Ambient,
  ambient: _ambient,
  live: (policy) =>
    Layer.scopedDiscard(
      Effect.flatMap(
        Effect.all([
          Effect.flatMap(Hooks, (hooks) => hooks.drained),
          Hooks.Meter,
          OtelBridge.OtelTracerProvider,
          OtelLogger.OtelLoggerProvider,
          _Ambient,
        ]),
        ([adds, raw, tracerProvider, loggerProvider]) =>
          Effect.acquireRelease(
            Effect.sync(() => {
              const egress = _egress(policy)
              new HostMetrics({
                meterProvider: raw.provider,
                metricGroups: [...policy.server.engine.groups],
                name: policy.identity.app,
              }).start()
              return registerInstrumentations({
                instrumentations: [..._rows(policy, egress), ...adds.instruments],
                loggerProvider,
                meterProvider: raw.provider,
                tracerProvider,
              })
            }),
            (unload) => Effect.sync(unload),
          ),
      ),
    ),
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Instrument }
```

## [03]-[RESEARCH]

(none)
