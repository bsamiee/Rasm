# [RUNTIME_METER]

`Pulse` is the work-plane meter bridge — one lossy projection from durable-work evidence onto Convention-keyed Effect instruments, so queue depth, drain lag, and relay throughput read as OTel series while every dispute settles against the journal. `mark` folds a settlement fact into its counter row at the emitting call site, and `live` runs the sampled census sweep setting the outbox and queue gauges from one `Probe` port the app root satisfies with the data journal's census statement — fact rows stay the billing truth, instruments stay bounded, and neither plane re-derives the other.

Two policy seams close at the same altitude: `verbosity` wires the config tier table into `Logger.minimumLogLevel` so the declared `verbose` column governs the process log floor, and `tenants` contributes the tenant metric-stream view row through the `Hooks` registry the export lanes drain — an allow-list attributes processor under the cardinality ceiling, so per-tenant series ride the same governor every reader inherits. Its module is `runtime/src/otel/meter.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                    | [PUBLIC] |
| :-----: | :----------- | :------------------------------------------------------------------------ | :------- |
|  [01]   | `PROJECTION` | the work-fact→counter row table and the one `mark` fold                  | `Pulse`  |
|  [02]   | `CENSUS`     | the `Probe` port and the sampled gauge sweep Layer                        | `Pulse`  |
|  [03]   | `VERBOSITY`  | the tier-table→`Logger.minimumLogLevel` wiring                            | `Pulse`  |
|  [04]   | `TENANT`     | the tenant metric-stream view row under the cardinality governor         | `Pulse`  |

## [02]-[PROJECTION]

[PROJECTION]:
- Owner: the interior `_WORK` row table — one row per work-plane settlement kind (`drained`: relay claims settled, the throughput counter; `parked`: deliverables diverted to the dead set, the DLQ counter) binding the kind to its `Convention.instrument` row — and `Pulse.mark(kind, channel, count?)`, the one projection fold: it increments the row's counter tagged `Convention.rasm.workChannel`, so the emitting owner adds one composed line beside its `Fact.record` call and the instrument mints nowhere else.
- Law: the projection is lossy by design — the journal fact is the truth a billing or forensic read settles against, the counter is the dashboard series, and the two emit from ONE call site so they cannot disagree on what happened, only on retention; a missing metric point is a dashboard gap, never an evidence defect.
- Law: instrument metadata derives from the row — name and description read off `Convention.instrument`, the UCUM unit stays a Convention column the board and SLO planes consume — so a signal-site string literal has no spelling here.
- Law: `channel` values are the work plane's own closed channel vocabulary (the deliver channel rows, the queue lane names) — bounded tags by construction, and an identifier-grade value rides span attributes, never this tag.
- Entry: `Pulse.mark("drained", channel, settled)` beside the relay's drain fact; `Pulse.mark("parked", channel)` beside the park fact.
- Growth: a new work-plane settlement kind is one `_WORK` row with its Convention instrument row.
- Boundary: the facts themselves are the work plane's (`work/deliver`, `work/queue`) and the journal is the data plane's; this page owns only the projection.
- Packages: `effect` (`Metric`); `@rasm/ts/core` (`Convention`).

```typescript
import { createAllowListAttributesProcessor } from "@opentelemetry/sdk-metrics"
import { Convention } from "@rasm/ts/core"
import { Context, Duration, Effect, Layer, Logger, LogLevel, Metric, Schedule } from "effect"
import { Setting } from "../proc/config.ts"
import { Hooks } from "./emit.ts"

const _counter = (row: { readonly name: string; readonly description: string }) =>
  Metric.counter(row.name, { description: row.description, incremental: true })

const _gauge = (row: { readonly name: string; readonly description: string }) =>
  Metric.gauge(row.name, { description: row.description })

const _WORK = {
  drained: { counter: _counter(Convention.instrument.relayDrained) },
  parked: { counter: _counter(Convention.instrument.queueParked) },
} as const

declare namespace Pulse {
  type Work = keyof typeof _WORK
  type Census = {
    readonly outbox: { readonly age: Duration.Duration; readonly depth: number; readonly redelivered: number }
    readonly queue: { readonly depth: number }
  }
  type Policy = {
    readonly cadence: Duration.Duration
    readonly tenant: { readonly keys: ReadonlyArray<string>; readonly limit: number }
  }
  type _Rows<T extends Record<Work, { readonly counter: ReturnType<typeof _counter> }> = typeof _WORK> = T
}

const _marked = (kind: Pulse.Work, channel: string, count: number): Effect.Effect<void> =>
  Metric.incrementBy(Metric.tagged(_WORK[kind].counter, Convention.rasm.workChannel, channel), count)
```

## [03]-[CENSUS]

[CENSUS]:
- Owner: the `Probe` port and the sweep — `Probe` is one `Context.Tag` whose `census` member answers the current outbox and queue truth, and `Pulse.live(policy)` is a `Layer.scopedDiscard` forking one `Schedule.spaced(policy.cadence)` repeat that sets the four gauges (`outboxDepth`, `outboxAge`, `outboxRedelivered`, `queueDepth`) from each sample; the fork dies with the graph scope, so a leaked sweep fiber is structurally impossible.
- Law: the port keeps the strata clean — the data journal's `Journal.census` statement satisfies `Probe` at the app root, so the outbox truth crosses the seam as a value and this module imports no SQL surface; the queue depth arrives from the durable-queue owner's own read through the same binding.
- Law: the probe is total by contract — the satisfying binding internalizes its store faults (the prior sample or a zero census stands in), because a broken gauge sweep must degrade a dashboard, never fail a process.
- Law: gauges are sampled, never accumulated — depth, age, and redelivery are census facts of one instant, so the sweep sets absolute levels and rate questions (`DLQ rate`, redelivery rate) derive in the query plane from the counter and gauge series.
- Entry: `Pulse.live(policy)` merged at the composition root beside `Export.live`, after the root binds `Probe`.
- Growth: a new census dimension is one `Census` field, one Convention gauge row, and one `Metric.set` line in the sweep.
- Packages: `effect` (`Context`, `Layer`, `Schedule`, `Metric`, `Duration`).

```typescript
class Probe extends Context.Tag("runtime/Pulse/Probe")<Probe, {
  readonly census: Effect.Effect<Pulse.Census>
}>() {}

const _GAUGES = {
  outboxAge: _gauge(Convention.instrument.outboxAge),
  outboxDepth: _gauge(Convention.instrument.outboxDepth),
  outboxRedelivered: _gauge(Convention.instrument.outboxRedelivered),
  queueDepth: _gauge(Convention.instrument.queueDepth),
} as const

const _swept: Effect.Effect<void, never, Probe> = Effect.flatMap(Probe, (probe) =>
  Effect.flatMap(probe.census, (census) =>
    Effect.all([
      Metric.set(_GAUGES.outboxAge, Duration.toSeconds(census.outbox.age)),
      Metric.set(_GAUGES.outboxDepth, census.outbox.depth),
      Metric.set(_GAUGES.outboxRedelivered, census.outbox.redelivered),
      Metric.set(_GAUGES.queueDepth, census.queue.depth),
    ], { discard: true })))
```

## [04]-[VERBOSITY]

[VERBOSITY]:
- Owner: `Pulse.verbosity` — one Layer wiring the config tier table into the process log floor: it reads `Setting.serve.tier`, projects the tier's `verbose` column through the `Setting.tiers` anchor, and installs `Logger.minimumLogLevel` — `Debug` where the tier is verbose, `Info` otherwise — so the declared column governs every `Effect.log*` call in the process and no page carries a level literal.
- Law: the floor is one root decision — the Layer merges once at the composition root beneath the export lane, so the OTLP log leg and any file logger both inherit it; a per-module level override is a `Logger.withMinimumLogLevel` region on the owning rail, never a second root install.
- Entry: `Pulse.verbosity` at the composition root.
- Packages: `effect` (`Logger`, `LogLevel`, `Layer`); `../proc/config.ts` (`Setting`).

```typescript
const _verbosity: Layer.Layer<never, never, Setting> = Layer.unwrapEffect(
  Effect.map(Setting, (setting) =>
    Logger.minimumLogLevel(Setting.tiers[setting.serve.tier].verbose ? LogLevel.Debug : LogLevel.Info)),
)
```

## [05]-[TENANT]

[TENANT]:
- Owner: `Pulse.tenants(policy)` — the tenant metric-stream row: one `Hooks.contribute` tap adding a `ViewOptions` row over the `rasm.*` instrument space whose allow-list attributes processor admits exactly the policy's tag keys and `Convention.rasm.tenant`, with `aggregationCardinalityLimit` as the per-view ceiling — so per-tenant series exist as governed streams the export lanes drain, and an ungoverned tenant tag cannot mint unbounded series.
- Law: the governor stacks — this view's allow-list is the primary bound, its cardinality limit the circuit breaker above it, and the reader-level `cardinality.tenant` ceiling from `Export.Policy` sits above every view; the three tiers are declared at three owners and compose without restatement.
- Entry: `Pulse.tenants(policy)` merged among the `Hooks.contribute` nodes, before `Export.live` drains.
- Growth: a second governed stream (a per-app view, a per-ring view) is one more contributed view row.
- Packages: `@opentelemetry/sdk-metrics` (`createAllowListAttributesProcessor`); `./emit.ts` (`Hooks`).

```typescript
const _tenants = (policy: Pulse.Policy): Layer.Layer<never, never, Hooks> =>
  Hooks.contribute((hooks) =>
    hooks.add("views", {
      instrumentName: "rasm.*",
      attributesProcessors: [createAllowListAttributesProcessor([...policy.tenant.keys, Convention.rasm.tenant])],
      aggregationCardinalityLimit: policy.tenant.limit,
    }))

const Pulse: {
  readonly Probe: typeof Probe
  readonly live: (policy: Pulse.Policy) => Layer.Layer<never, never, Probe>
  readonly mark: (kind: Pulse.Work, channel: string, count?: number) => Effect.Effect<void>
  readonly tenants: (policy: Pulse.Policy) => Layer.Layer<never, never, Hooks>
  readonly verbosity: Layer.Layer<never, never, Setting>
  readonly work: typeof _WORK
} = {
  Probe,
  live: (policy) => Layer.scopedDiscard(Effect.forkScoped(Effect.repeat(_swept, Schedule.spaced(policy.cadence)))),
  mark: (kind, channel, count = 1) => _marked(kind, channel, count),
  tenants: _tenants,
  verbosity: _verbosity,
  work: _WORK,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Pulse }
```

## [06]-[RESEARCH]

(none)
