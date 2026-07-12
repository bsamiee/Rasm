# [RUNTIME_SCHEDULE]

Calendar recurrence as a vocabulary: a scheduled job is one `Cadence` row — cron expression with intrinsic timezone, anchor policy, misfire window, catch-up posture, service class, shard group — and one registration fold mints the whole table into `ClusterCron` singletons that fire exactly once per cluster tick. The page owns the three decisions a cron engine leaves open and every hand-rolled scheduler re-invents: what a tick's successor is anchored to (wall clock versus previous completion — `calculateNextRunFromPrevious` as a row column), what happens to a tick that fired while no runner lived (the misfire window — `skipIfOlderThan` bounds how stale a tick still executes), and what happens to the ticks beyond that window (the catch-up posture — skip them, run one representative, or replay all, computed from the `Cron.sequence` between the last recorded run and now with each replayed tick an idempotent step keyed by its instant). Durable pauses arrive settled from `flow#SIGNAL_GATE` — this page composes `Signal.pause` and mints no timer. The same row table drives the host fallback: a single-node process or a scope whose in-database cron grant is refused runs identical rows on the in-process `Schedule.cron`, so degradation is an engine swap, never a second table. The module ships on the `./server` exports subpath as `runtime/src/work/schedule.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                    | [PUBLIC]  |
| :-----: | :------------- | :------------------------------------------------------------------------ | :-------- |
|  [01]   | `CADENCE_ROWS` | the recurrence vocabulary — cron, anchor, misfire, catch-up, class, group | `Cadence` |
|  [02]   | `CLUSTER_MINT` | the registration fold onto `ClusterCron` and the catch-up computation     | `Cadence` |
|  [03]   | `HOST_ROW`     | the in-process fallback engine over the same rows                         | `Cadence` |

## [02]-[CADENCE_ROWS]

[CADENCE_ROWS]:
- Owner: the `Cadence.Row` — `name` (the singleton identity and the idempotency prefix), `cron` (an `effect/Cron` parsed once with its timezone intrinsic, so a DST transition is the value's own arithmetic), `anchor` (`"clock"` fires on calendar instants regardless of run length; `"previous"` measures the next tick from the last completion — the long-running-job posture that compiles to `calculateNextRunFromPrevious`), `misfire` (the staleness bound compiled to `skipIfOlderThan` — a tick older than the window never executes as if current), `catchUp` (`"skip" | "once" | "all"` — the posture over ticks beyond the misfire window), `clazz` (the `entity#WORK_CLASS` row pricing the execution budget), and `group` (the shard-group partition the singleton registers under).
- Law: an app declares its closed cadence table in this shape and hands it to one mint — schedule policy is root data, a `ClusterCron.make` call outside the fold is unspellable, and a new scheduled job is one row.
- Law: anchor and catch-up are orthogonal columns — a `"previous"`-anchored row has no missed-tick set by construction (its successor derives from completion), so its `catchUp` is structurally `"skip"`; the row type encodes this with a discriminated pair rather than documenting it.
- Growth: a new recurrence concern (a jitter band, a blackout calendar) is one row column both engines read; a one-shot delayed job is not a cadence — it is a `flow#SIGNAL_GATE` pause or a `DeliverAt` payload on the actor plane.
- Packages: `effect` (`Cron`, `Duration`); `./entity.ts` (`WorkClass`).

```typescript
import { ClusterCron } from "@effect/cluster"
import { Array, Cron, DateTime, Duration, Effect, Iterable, Layer, Option, Schedule, Schema } from "effect"
import { Fact } from "@rasm/ts/data"
import { WorkClass } from "./entity.ts"
import { Step } from "./flow.ts"

declare namespace Cadence {
  type CatchUp = "skip" | "once" | "all"
  type Anchor =
    | { readonly anchor: "clock"; readonly catchUp: CatchUp }
    | { readonly anchor: "previous"; readonly catchUp: "skip" }
  type Row = Anchor & {
    readonly name: string
    readonly cron: Cron.Cron
    readonly misfire: Duration.Duration
    readonly clazz: WorkClass.Kind
    readonly group: string
  }
  type Table = ReadonlyArray<Row>
}
```

## [03]-[CLUSTER_MINT]

[CLUSTER_MINT]:
- Owner: `Cadence.cluster(table, run)` — the registration fold: each row becomes one `ClusterCron.make({ name, cron, execute, shardGroup, calculateNextRunFromPrevious, skipIfOlderThan })` Layer, merged into the one schedule Layer the composition root provides beside `entity#GRID`. The `execute` is the row's body wrapped in `Step.run(row.name, row.clazz, …)` so every tick carries the class's deadline geometry and durable exit, and each execution's step key is the tick instant — a re-fired tick replays its recorded exit instead of double-running.
- Law: exactly-once-per-tick is the cluster fact — the cron rides the singleton substrate, migrating on rebalance, firing once per cluster regardless of runner count; a per-runner guard, a lock row, or a "am I the leader" read beside it is unspellable.
- Law: catch-up is a computed prefix, not a loop — on execution the fold reads the last-run mark through the caller-supplied `marks` projection (an audit read over the `cadence.backfilled`/step-exit evidence — read-side material, so the mint carries no run-history table), takes the missed instants from `Cron.sequence(cron, lastRun)` strictly before now and beyond the misfire window, and applies the posture: `"skip"` drops them, `"once"` runs the latest missed instant as one representative tick, `"all"` folds every missed instant through the same instant-keyed step so a replay after a weekend outage is a bounded, idempotent, evidence-bearing sequence. Each caught-up execution records a meter fact so backfill volume is observable.
- Receipt: every tick's step exit is the durable run evidence — instant, verdict, duration — the operator census reads; no separate run-history table exists.
- Growth: a new posture is one `CatchUp` arm in the computed prefix; per-row notification is a tap on the row's body at its owner.
- Packages: `@effect/cluster` (`ClusterCron`); `effect` (`Cron`, `Array`, `Layer`); `./flow.ts` (`Step`); `@rasm/ts/data` (`Fact` — the backfill meter).

```typescript
const _missed = (row: Cadence.Row, lastRun: DateTime.Utc, now: DateTime.Utc): ReadonlyArray<Date> => {
  const horizon = DateTime.toDate(DateTime.subtractDuration(now, row.misfire))
  const ticks = Array.filter(
    Array.fromIterable(
      Iterable.takeWhile(Cron.sequence(row.cron, DateTime.toDate(lastRun)), (tick) => tick < DateTime.toDate(now)),
    ),
    (tick) => tick < horizon,
  )
  return row.catchUp === "skip" ? [] : row.catchUp === "once" ? Array.takeRight(ticks, 1) : ticks
}

const _cluster = <R, R2>(
  table: Cadence.Table,
  run: (row: Cadence.Row, tick: Date) => Effect.Effect<void, never, R>,
  marks: (name: string) => Effect.Effect<Option.Option<DateTime.Utc>, never, R2>,
) =>
  Layer.mergeAll(
    ...Array.map(table, (row) =>
      ClusterCron.make({
        name: row.name,
        cron: row.cron,
        shardGroup: row.group,
        calculateNextRunFromPrevious: row.anchor === "previous",
        skipIfOlderThan: row.misfire,
        execute: Effect.gen(function* () {
          const now = yield* DateTime.now
          const last = yield* marks(row.name)
          const backlog = _missed(row, Option.getOrElse(last, () => now), now)
          yield* Effect.forEach(
            backlog,
            (tick) =>
              Step.run(`${row.name}@${tick.toISOString()}`, row.clazz, {
                success: Schema.Void,
                error: Schema.Never,
                execute: run(row, tick),
              }),
            { discard: true },
          )
          yield* Effect.when(
            Fact.record({
              action: "cadence.backfilled",
              actor: { key: row.name, kind: "system" },
              change: [{ _tag: "Assigned", path: "/ticks", next: String(backlog.length) }],
              retention: "operational",
              target: { key: row.name, kind: "cadence" },
            }),
            () => backlog.length > 0,
          )
          yield* Step.run(`${row.name}@${DateTime.formatIso(now)}`, row.clazz, {
            success: Schema.Void,
            error: Schema.Never,
            execute: run(row, DateTime.toDate(now)),
          })
        }),
      })),
  )
```

## [04]-[HOST_ROW]

[HOST_ROW]:
- Owner: `Cadence.host(table, run)` — the in-process engine over identical rows: each row repeats its body on `Schedule.cron` under a forked daemon fiber scoped to the Layer, with the same `Step.run` wrapping when a workflow engine is present and a bare budgeted body when not. Two consumers select this row: the single-node process whose grid runs the local entry, and the data wave's maintenance jobs whose in-database cron grant is refused — the grooming and rebuild schedules degrade to host execution through this fold without touching their row shapes.
- Law: the fallback is honest about its guarantees — host cadence is at-most-once-per-process (a dead process misses ticks until restart, and the catch-up prefix runs on boot from the last durable mark where one exists); a consumer whose ticks may never be lost belongs on the cluster row, and the table's engine selection is the root's deployment fact.
- Law: one engine per row per process — the root selects `cluster` or `host` for the whole table; a row running on both engines double-fires and the selection type makes that unspellable.
- Growth: a third engine (an edge scheduler, a test-clock driver) is one more fold over the same table; specs drive `host` under `TestClock` with no cluster.
- Packages: `effect` (`Schedule`, `Stream`, `Layer`).

```typescript
const _host = <R>(table: Cadence.Table, run: (row: Cadence.Row, tick: Date) => Effect.Effect<void, never, R>) =>
  Layer.scopedDiscard(
    Effect.forEach(table, (row) =>
      Effect.forkScoped(
        Effect.repeat(
          DateTime.now.pipe(Effect.flatMap((now) => run(row, DateTime.toDate(now)))),
          Schedule.cron(row.cron),
        ),
      ), { discard: true }),
  )

const Cadence = {
  cluster: _cluster,
  host: _host,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cadence }
```
