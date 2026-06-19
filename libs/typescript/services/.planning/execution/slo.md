# [SERVICES_SLO]

The SLO and error-budget read model for the node tier — `SloBudget`, the single `Effect.Service` that records the four-objective signal series, folds the Google-SRE multi-window multi-burn-rate alert algebra over it, and routes a burn-rate breach through the existing outbox delivery sinks, never a parallel notification path. Four owners compose one surface: the closed `Objective` vocabulary (`latency`/`availability`/`saturation`/`freshness`), each objective carrying its own SLO target, fast/slow window pair, and burn-rate multiplier as a behavior row read by indexed access rather than four sibling threshold configs; the `BurnRate` fold that evaluates each objective against its two windows in one set-algebraic SQL round-trip and fires only when BOTH windows confirm the burn; the `AlertRoute` `Data.TaggedEnum` over the `outbox#TRANSACTIONAL_OUTBOX` `DeliverySink` Internal/External axis, dispatched by `Match.tagsExhaustive` so a new sink breaks every alert site at compile time; and `SignalSeries`, one `Model.Class` on the `persistence/store#STORE_BOUNDARY` registry holding the recorded objective observations the windows aggregate. The `saturation` objective's cluster signal is the five `@effect/cluster` `ClusterMetrics` gauges read off the runtime, never a re-derived shard scrape; the burn evaluation runs as a `backplane#RUNNER_AND_SCHEDULING` shard-pinned durable cron, not a per-service threshold check. The reader is the `@opentelemetry/sdk-metrics` `MeterProvider` plus the `@opentelemetry/sdk-trace-node` `NodeTracerProvider` decode of the C#-minted `ONE_HEALTH_DEGRADATION_WIRE`/`ONE_DISTRIBUTED_TRACE` level and trace context off the `interchange` wire — decode-only, never re-minting — NOT `@effect/opentelemetry`, which stays the README `[3]-[CROSS_CUTTING]` exporter edge with no reader catalogue. This is the node analog of `platform:ui/Observability/vitals#PERFORMANCE_BUDGET`, distinct from the deploy-time `provisioning/contract#PROVISIONING` `ObservabilityStack.emit` collector — both end at the one OTel emitter boundary, but this owner is the runtime read model and that is the deploy-time provisioning of the stack.

## [01]-[INDEX]

- [01]-[SLO_BUDGET]: owns the closed `Objective` delegate-row vocabulary, the `SignalSeries` `Model.Class`, the multi-window multi-burn-rate `BurnRate` fold over the `@effect/sql-pg` signal series, the `ClusterMetricsReader` over the five `ClusterMetrics` gauges, the `AlertRoute` `Data.TaggedEnum` over the outbox `DeliverySink`, the `@opentelemetry/sdk-metrics`/`@opentelemetry/sdk-trace-node` reader, and the C#-wire health/trace decode.

## [02]-[SLO_BUDGET]

- Owner: `SloBudget`, the single `Effect.Service` over four composed owners — `Objective`, the closed delegate-row vocabulary (`latency`/`availability`/`saturation`/`freshness`) where each row IS the SLO target, the fast/slow window pair, the two burn-rate multipliers, and the unit; `SignalSeries`, the one `Model.Class` row holding a recorded objective observation; `BurnRate`, the multi-window multi-burn-rate fold; `AlertRoute`, the `Data.TaggedEnum` over the outbox `DeliverySink`; and `ClusterMetricsReader`, the projection of the five runtime gauges into the `saturation` signal. Never four parallel objective services, never a per-objective threshold config, never a parallel alert path beside the outbox.
- Cases: each `Objective` is one row in the `OBJECTIVES` `as const satisfies Record` table — `latency` (the p99 request-latency good-event ratio, fast 5 m / slow 1 h, page-multiplier 14.4 / ticket-multiplier 6), `availability` (the success-ratio, fast 1 h / slow 6 h, 13.44 / 1), `saturation` (the cluster runner-health ratio sourced from `ClusterMetrics`, fast 5 m / slow 30 m), `freshness` (the signal-recency staleness ratio, fast 30 m / slow 6 h) — so a new objective is one row, never a sibling config block, and `windowsOf`/`targetOf` read the row by indexed access keyed to `keyof typeof OBJECTIVES`, never a re-derived literal. `SignalSeries` is one `Model.Class` row — objective key, the good-event count, the total-event count, and the insert timestamp — recorded each evaluation tick through the one `PgClient`, so the windows aggregate a real persisted series rather than an in-memory ring. `BurnRate` is the Google-SRE multi-window multi-burn-rate fold: one `windowAggregates` SQL round-trip computes, per objective, the error budget consumed over BOTH the fast and slow window as `1 - SUM(good) / NULLIF(SUM(total), 0)` divided by `(1 - target)`, and the fold fires the page alert ONLY when the fast-window burn AND the slow-window burn both exceed the page multiplier (the slow window suppresses a transient spike, the fast window suppresses a slow leak), the ticket alert when both exceed the ticket multiplier, expression-shaped over `Effect.gen` Bind/Map and `Match.value` on the `BurnVerdict` literal, never an if/else ladder and never a statement switch over window arms. `AlertRoute` is the closed `Data.TaggedEnum` over the delivery target reusing the outbox `DeliverySink` axis — `Internal` routes to the `messaging/rpc#INTERNAL_RPC` `RpcGroup` procedure, `External` routes to an outbound sink with headers — handed opaquely to the outbox `enqueue.alert`, which folds it by `Match.tagsExhaustive` (the `outbox#TRANSACTIONAL_OUTBOX` `deliver` fold owns the dispatch, never a second fold on this site), so the burn-rate alert is one same-txn outbox row published through the SAME `DeliverySink` family the outbox already owns, never a second notification rail. `ClusterMetricsReader` reads the five `Metric.Metric.Gauge<bigint>` (`entities`/`singletons`/`runners`/`runnersHealthy`/`shards`) off the runtime through `Metric.value`, projecting `runnersHealthy / runners` as the `saturation` good-ratio, never a re-derived shard scrape.
- Auto: the burn evaluation is a `backplane#RUNNER_AND_SCHEDULING` `ScheduledWork.cron` shard-pinned singleton — exactly one runner records the tick's `SignalSeries`, runs the `windowAggregates`, folds the `BurnVerdict`, and enqueues the `AlertRoute` outbox row — so the SLO evaluation is a durable cluster sweep surviving runner restart, never a per-service in-process timer the cron-pinning bounds the poll cost of; the `saturation` objective's per-tick observation is the `ClusterMetricsReader` projection, the `latency`/`availability` objectives' observations are the OTel `MeterProvider`-collected request metrics, and the `freshness` objective's observation is the recency of the last recorded series row.
- Entry: the owner rides the one `persistence/store#STORE_BOUNDARY` `PgClient` — `SignalSeries` is one entity on the `EntityRegistry`, the recording write rides `SqlClient` through the same client, the `windowAggregates` is one set-algebraic SQL round-trip with the window bounds and the per-objective target bound as parameters, and the alert enqueue rides the `outbox#TRANSACTIONAL_OUTBOX` `enqueue` in the same transaction the evaluation tick writes the series in, so the alert publishes atomically with the observation that triggered it; the `saturation` signal reads the `backplane#RUNNER_AND_SCHEDULING` `ClusterMetrics` gauges, and the recorded series is tenant-scoped through the same `app.current_tenant` GUC every query reads (`persistence/tenancy#TENANCY`).
- Wire: the owner is the node-side consumer of the cross-`libs/` C#-minted `ONE_HEALTH_DEGRADATION_WIRE`/`ONE_DISTRIBUTED_TRACE` frames — the `HealthWire` carries a degradation level and the `TraceContextWire` carries the W3C trace-parent/trace-state, both decoded off the `interchange` descriptor path through one `Schema` carrier (the `ONE_MODEL_THREE_FACES` descriptor decode, decode-only) and never re-minted; the decoded level seeds the corresponding objective's good/total ratio and the decoded trace context reattaches to the `@opentelemetry/sdk-trace-node` `NodeTracerProvider` span so the node burn-rate alert carries the originating C# trace id; the OTel `MeterProvider`/`NodeTracerProvider` are the read-and-export boundary, not a second emitter beside the `provisioning/contract#PROVISIONING` `ObservabilityStack` collector — both end at the one OTel exporter edge (`@effect/opentelemetry`, README `[3]-[CROSS_CUTTING]`).
- Packages: `@effect/cluster` for the five `ClusterMetrics` `Metric.Metric.Gauge<bigint>` read off the runtime (the `saturation` signal source) and the `ScheduledWork.cron` the evaluation pins on; `@opentelemetry/sdk-metrics` for the `MeterProvider`/`PeriodicExportingMetricReader`/`MetricProducer` reader that collects the `latency`/`availability` request metrics; `@opentelemetry/sdk-trace-node` for the `NodeTracerProvider`/`ParentBasedSampler` that decodes and reattaches the C#-minted trace context; `@effect/sql` and `@effect/sql-pg` for the `SignalSeries` `Model.Class` write and the multi-window `windowAggregates` over the one `PgClient`; `effect` for the multi-window burn-rate fold, the `Objective` `as const satisfies Record` delegate-row table, and the `AlertRoute` `Match.tagsExhaustive`.
- Growth: a new objective lands as one `OBJECTIVES` row with its target, window pair, and multipliers, breaking nothing and adding no config block; a new alert target lands as one `AlertRoute` `Data.TaggedEnum` variant breaking the `Match.tagsExhaustive` fold at compile time, never a parallel relay (the outbox `DeliverySink` already owns the publish); a new burn window lands as one column on the window-pair row; a new cluster signal lands as one `ClusterMetricsReader` projection over an added gauge; a new wire frame lands as one decode-only `Schema` carrier, never a second mint.
- Boundary: the named defects — four sibling threshold configs instead of the one `Objective` delegate-row table; a single-window burn alert that fires on a transient spike instead of the multi-window fast-AND-slow confirmation; a parallel notification path instead of the outbox `DeliverySink` enqueue; a re-derived shard scrape instead of the `ClusterMetrics` gauge read; a per-service in-process timer instead of the shard-pinned durable cron; a re-minted health/trace frame instead of the decode-only `Schema` carrier off the wire; a second SQL surface beside the one `PgClient`; promoting `@effect/opentelemetry` from the exporter edge to a named reader without its branch `.api` catalogue; an `if`/`else` window ladder or a statement `switch` over window arms instead of the expression-shaped fold; a series recorded outside the alert-enqueue transaction breaking the atomic-publish contract. This is a node-only surface, never browser-reachable.

```ts contract
import type { PgClient } from "@effect/sql-pg"
import { ClusterMetrics } from "@effect/cluster"
import { Model, SqlClient, SqlError } from "@effect/sql"
import { Data, DateTime, Duration, Effect, Layer, Match, Metric, Schema, Stream } from "effect"
import type { MeterProvider, MetricProducer } from "@opentelemetry/sdk-metrics"
import type { NodeTracerProvider, ReadableSpan } from "@opentelemetry/sdk-trace-node"

// --- [TYPES] -------------------------------------------------------------------------------

type ObjectiveKind = keyof typeof OBJECTIVES
type AlertSeverity = "page" | "ticket" | "ok"
type BurnVerdict = "fast" | "slow" | "both" | "clear"

interface BurnWindows {
  readonly fast: Duration.Duration
  readonly slow: Duration.Duration
  readonly page: number
  readonly ticket: number
}

interface ObjectiveRow {
  readonly target: number
  readonly unit: "ratio" | "ms"
  readonly windows: BurnWindows
}

interface WindowAggregate {
  readonly objective: ObjectiveKind
  readonly fastBurn: number
  readonly slowBurn: number
}

interface BurnAlert {
  readonly objective: ObjectiveKind
  readonly severity: Exclude<AlertSeverity, "ok">
  readonly fastBurn: number
  readonly slowBurn: number
  readonly budgetRemaining: number
}

type AlertRoute = Data.TaggedEnum<{
  readonly Internal: { readonly procedure: string }
  readonly External: { readonly endpoint: string; readonly headers: Record<string, string> }
}>
const AlertRoute = Data.taggedEnum<AlertRoute>()

// --- [CONSTANTS] ---------------------------------------------------------------------------

const TENANT_GUC = "app.current_tenant" as const

const OBJECTIVES = {
  latency: {
    target: 0.99,
    unit: "ratio",
    windows: { fast: Duration.minutes(5), slow: Duration.hours(1), page: 14.4, ticket: 6 },
  },
  availability: {
    target: 0.999,
    unit: "ratio",
    windows: { fast: Duration.hours(1), slow: Duration.hours(6), page: 13.44, ticket: 1 },
  },
  saturation: {
    target: 0.9,
    unit: "ratio",
    windows: { fast: Duration.minutes(5), slow: Duration.minutes(30), page: 6, ticket: 3 },
  },
  freshness: {
    target: 0.95,
    unit: "ratio",
    windows: { fast: Duration.minutes(30), slow: Duration.hours(6), page: 8, ticket: 2 },
  },
} as const satisfies Record<string, ObjectiveRow>

const OBJECTIVE_KEYS = ["latency", "availability", "saturation", "freshness"] as const satisfies ReadonlyArray<ObjectiveKind>

const windowsOf = (k: ObjectiveKind): BurnWindows => OBJECTIVES[k].windows
const targetOf = (k: ObjectiveKind): number => OBJECTIVES[k].target

// --- [MODELS] ------------------------------------------------------------------------------

class SignalSeries extends Model.Class<SignalSeries>("SignalSeries")({
  id: Model.Generated(Schema.Number),
  objective: Schema.Literal(...OBJECTIVE_KEYS),
  good: Schema.Number,
  total: Schema.Number,
  recordedAt: Model.DateTimeInsert,
}) {}

class HealthWire extends Schema.Class<HealthWire>("HealthWire")({
  service: Schema.String,
  level: Schema.Literal("healthy", "degraded", "critical"),
  good: Schema.Number,
  total: Schema.Number,
}) {}

class TraceContextWire extends Schema.Class<TraceContextWire>("TraceContextWire")({
  traceId: Schema.String,
  parentSpanId: Schema.String,
  traceFlags: Schema.Number,
  traceState: Schema.optional(Schema.String),
}) {}

// --- [ERRORS] ------------------------------------------------------------------------------

class SloFault extends Schema.TaggedError<SloFault>()("SloFault", {
  objective: Schema.Literal(...OBJECTIVE_KEYS),
  stage: Schema.Literal("record", "aggregate", "route"),
  cause: Schema.Unknown,
}) {}

// --- [SERVICES] ----------------------------------------------------------------------------

interface OutboxEnqueue {
  readonly alert: (route: AlertRoute, alert: BurnAlert) => Effect.Effect<void, SqlError.SqlError>
}
const OutboxEnqueue = Effect.Tag("services/OutboxEnqueue")<OutboxEnqueue, OutboxEnqueue>()

class ClusterMetricsReader extends Effect.Service<ClusterMetricsReader>()("services/ClusterMetricsReader", {
  accessors: true,
  succeed: {
    get gauges(): Effect.Effect<Record<"entities" | "singletons" | "runners" | "runnersHealthy" | "shards", bigint>> {
      return Effect.all({
        entities: Metric.value(ClusterMetrics.entities).pipe(Effect.map((g) => g.value)),
        singletons: Metric.value(ClusterMetrics.singletons).pipe(Effect.map((g) => g.value)),
        runners: Metric.value(ClusterMetrics.runners).pipe(Effect.map((g) => g.value)),
        runnersHealthy: Metric.value(ClusterMetrics.runnersHealthy).pipe(Effect.map((g) => g.value)),
        shards: Metric.value(ClusterMetrics.shards).pipe(Effect.map((g) => g.value)),
      })
    },
    get saturation(): Effect.Effect<{ readonly good: number; readonly total: number }> {
      return this.gauges.pipe(
        Effect.map((g) => ({ good: Number(g.runnersHealthy), total: Math.max(Number(g.runners), 1) })),
      )
    },
  },
}) {}

class SloBudget extends Effect.Service<SloBudget>()("services/SloBudget", {
  accessors: true,
  effect: Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const enqueue = yield* OutboxEnqueue
    const reader = yield* ClusterMetricsReader

    const record = (objective: ObjectiveKind, good: number, total: number): Effect.Effect<void, SqlError.SqlError> =>
      sql`INSERT INTO signal_series (objective, good, total) VALUES (${objective}, ${good}, ${total})`

    const evaluate = (tenant: string): Effect.Effect<ReadonlyArray<BurnAlert>, SqlError.SqlError | SloFault> =>
      sql.withTransaction(
        Effect.gen(function* () {
          yield* sql`SELECT set_config(${TENANT_GUC}, ${tenant}, true)`
          const saturation = yield* reader.saturation
          yield* record("saturation", saturation.good, saturation.total)
          const aggregates = yield* windowAggregates(sql)
          const alerts = aggregates.flatMap(toAlert)
          yield* Effect.forEach(alerts, (alert) => enqueue.alert(routeFor(alert), alert), { discard: true })
          return alerts
        }),
      )

    const run = (tenant: string): Stream.Stream<ReadonlyArray<BurnAlert>, SqlError.SqlError | SloFault> =>
      Stream.repeatEffect(evaluate(tenant))

    return { record, evaluate, run } as const
  }),
  dependencies: [ClusterMetricsReader.Default],
}) {}

// --- [OPERATIONS] --------------------------------------------------------------------------

const burnVerdict = (windows: BurnWindows, fastBurn: number, slowBurn: number): BurnVerdict =>
  fastBurn >= windows.page && slowBurn >= windows.page ? "both"
    : fastBurn >= windows.page ? "fast"
    : slowBurn >= windows.page ? "slow"
    : "clear"

const severityOf = (windows: BurnWindows, fastBurn: number, slowBurn: number): AlertSeverity =>
  Match.value(burnVerdict(windows, fastBurn, slowBurn)).pipe(
    Match.when("both", () => "page" as const),
    Match.orElse(() =>
      fastBurn >= windows.ticket && slowBurn >= windows.ticket ? ("ticket" as const) : ("ok" as const),
    ),
  )

const toAlert = (a: WindowAggregate): ReadonlyArray<BurnAlert> => {
  const windows = windowsOf(a.objective)
  const severity = severityOf(windows, a.fastBurn, a.slowBurn)
  return severity === "ok"
    ? []
    : [{
        objective: a.objective,
        severity,
        fastBurn: a.fastBurn,
        slowBurn: a.slowBurn,
        budgetRemaining: Math.max(0, 1 - a.slowBurn * (1 - targetOf(a.objective))),
      }]
}

const routeFor = (alert: BurnAlert): AlertRoute =>
  alert.severity === "page"
    ? AlertRoute.External({ endpoint: "alerts.page", headers: { "x-objective": alert.objective } })
    : AlertRoute.Internal({ procedure: "notifications.ticket" })

const windowAggregates = (sql: SqlClient.SqlClient): Effect.Effect<ReadonlyArray<WindowAggregate>, SqlError.SqlError> =>
  Effect.forEach(
    OBJECTIVE_KEYS,
    (objective) => {
      const { fast, slow } = windowsOf(objective)
      const target = targetOf(objective)
      const fastSec = Duration.toSeconds(fast)
      const slowSec = Duration.toSeconds(slow)
      return sql<WindowAggregate>`
        SELECT ${objective}::text AS objective,
               (1 - COALESCE(SUM(good) FILTER (WHERE recorded_at > now() - make_interval(secs => ${fastSec})), 0)
                  / NULLIF(SUM(total) FILTER (WHERE recorded_at > now() - make_interval(secs => ${fastSec})), 0))
                  / (1 - ${target}::float8) AS "fastBurn",
               (1 - COALESCE(SUM(good) FILTER (WHERE recorded_at > now() - make_interval(secs => ${slowSec})), 0)
                  / NULLIF(SUM(total) FILTER (WHERE recorded_at > now() - make_interval(secs => ${slowSec})), 0))
                  / (1 - ${target}::float8) AS "slowBurn"
        FROM signal_series
        WHERE objective = ${objective}
          AND recorded_at > now() - make_interval(secs => ${slowSec})
      `.pipe(Effect.map((rows) => rows[0] ?? { objective, fastBurn: 0, slowBurn: 0 }))
    },
  )

const decodeHealth = Schema.decodeUnknown(HealthWire)
const decodeTrace = Schema.decodeUnknown(TraceContextWire)

const observeHealth = (frame: unknown): Effect.Effect<{ readonly objective: ObjectiveKind; readonly good: number; readonly total: number }, SloFault> =>
  decodeHealth(frame).pipe(
    Effect.map((h) => ({ objective: "availability" as const, good: h.good, total: h.total })),
    Effect.mapError((cause) => new SloFault({ objective: "availability", stage: "record", cause })),
  )

const reattachTrace = (tracer: NodeTracerProvider, frame: unknown): Effect.Effect<TraceContextWire, SloFault> =>
  decodeTrace(frame).pipe(
    Effect.tap((ctx) =>
      Effect.sync(() =>
        tracer
          .getTracer("services/observability")
          .startSpan("slo.burn", { startTime: Date.now() }, propagatedContext(ctx)),
      ),
    ),
    Effect.mapError((cause) => new SloFault({ objective: "latency", stage: "record", cause })),
  )

declare const propagatedContext: (ctx: TraceContextWire) => import("@opentelemetry/api").Context
declare const collectMetrics: (provider: MeterProvider, producer: MetricProducer) => Effect.Effect<ReadonlyArray<ReadableSpan>>

// --- [COMPOSITION] -------------------------------------------------------------------------

const SloBudgetLayer: Layer.Layer<SloBudget, never, SqlClient.SqlClient | PgClient.PgClient | OutboxEnqueue> =
  SloBudget.Default

// --- [EXPORTS] -----------------------------------------------------------------------------

export {
  AlertRoute,
  ClusterMetricsReader,
  decodeHealth,
  decodeTrace,
  OBJECTIVE_KEYS,
  OBJECTIVES,
  observeHealth,
  reattachTrace,
  severityOf,
  SignalSeries,
  SloBudget,
  SloBudgetLayer,
  SloFault,
  toAlert,
  windowAggregates,
}
export type { AlertSeverity, BurnAlert, BurnVerdict, BurnWindows, ObjectiveKind, ObjectiveRow, WindowAggregate }
```
