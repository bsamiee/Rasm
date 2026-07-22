# [RUNTIME_SCHEDULE]

Calendar recurrence as a vocabulary: a scheduled job is one `Cadence` row — cron expression with intrinsic timezone, anchor policy, misfire window, catch-up posture, service class, shard group — and one registration fold mints the whole table into `ClusterCron` singletons that fire exactly once per cluster tick. The page owns the three decisions a cron engine leaves open and every hand-rolled scheduler re-invents: what a tick's successor is anchored to (wall clock versus previous completion — `calculateNextRunFromPrevious` as a row column), what happens to a tick that fired while no runner lived (the misfire window — `skipIfOlderThan` bounds how stale a tick still executes), and what happens to ticks beyond that window (the bounded catch-up posture computed newest-first through `Cron.sequenceReverse`, then executed in chronological order with each replayed tick an idempotent step keyed by its instant). Durable pauses arrive settled from `flow#SIGNAL_GATE` — this page composes `Signal.pause` and mints no timer. The same row table drives the host fallback: a single-node process or a scope whose in-database cron grant is refused runs identical rows through a schedule driver, so degradation is an engine swap, never a second table. The module ships on the `./server` exports subpath as `runtime/src/work/schedule.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                    | [PUBLIC]  |
| :-----: | :------------- | :------------------------------------------------------------------------ | :-------- |
|  [01]   | `CADENCE_ROWS` | the recurrence vocabulary — cron, anchor, misfire, catch-up, class, group | `Cadence` |
|  [02]   | `CLUSTER_MINT` | the registration fold onto `ClusterCron` and the catch-up computation     | `Cadence` |
|  [03]   | `HOST_ROW`     | the in-process fallback engine over the same rows                         | `Cadence` |

## [02]-[CADENCE_ROWS]

[CADENCE_ROWS]:
- Owner: `Cadence.Table` — a record whose key is the singleton identity and idempotency prefix, so duplicate schedule identities are structurally impossible. Each policy value carries `cron`, the discriminated `anchor`/`catchUp` pair, `misfire`, `clazz`, and `group`; `Cadence.rows(table)` derives named rows for both engines, so identity is never repeated inside a value.
- Law: an app declares its closed cadence table in this shape and hands it to one mint — schedule policy is root data, a `ClusterCron.make` call outside the fold is unspellable, and a new scheduled job is one row.
- Law: anchor and catch-up are orthogonal columns — a `"previous"`-anchored row has no missed-tick set by construction (its successor derives from completion), so its `catchUp` is structurally `"skip"`; the row type encodes this with a discriminated pair rather than documenting it.
- Growth: a new recurrence concern (a jitter band, a blackout calendar) is one row column both engines read; a one-shot delayed job is not a cadence — it is a `flow#SIGNAL_GATE` pause or a `DeliverAt` payload on the actor plane.
- Packages: `effect` (`Cron`, `Duration`); `./entity.ts` (`WorkClass`).

```typescript
import { ClusterCron } from "@effect/cluster"
import { Array, Cron, DateTime, Duration, Effect, Iterable, Layer, Option, Record, Schedule, Schema, Stream } from "effect"
import { Fact } from "@rasm/ts/data"
import { WorkClass } from "./entity.ts"
import { Step } from "./flow.ts"

declare namespace Cadence {
  type Backfill = typeof Backfill.Type
  type CatchUp = "skip" | "once" | "all"
  type Anchor =
    | { readonly anchor: "clock"; readonly catchUp: "skip" | "once" }
    | { readonly anchor: "clock"; readonly catchUp: "all"; readonly backfill: Backfill }
    | { readonly anchor: "previous"; readonly catchUp: "skip" }
  type Policy = Anchor & {
    readonly cron: Cron.Cron
    readonly misfire: Duration.Duration
    readonly clazz: WorkClass.Kind
    readonly group: string
  }
  type Row = Policy & { readonly name: string }
  type Table = { readonly [name: string]: Policy }
}

const Backfill = Schema.Int.pipe(Schema.positive(), Schema.brand("CadenceBackfill"))

const _rows = (table: Cadence.Table): ReadonlyArray<Cadence.Row> =>
  Array.map(Record.toEntries(table), ([name, policy]) => ({ name, ...policy }))
```

## [03]-[CLUSTER_MINT]

[CLUSTER_MINT]:
- Owner: `Cadence.cluster(table, run, marks)` — the registration fold: each row becomes one `ClusterCron.make({ name, cron, execute, shardGroup, calculateNextRunFromPrevious, skipIfOlderThan })` Layer, merged into the one schedule Layer the composition root supplies beside `entity#GRID`. The `execute` is the shared `_fired` pass — catch-up prefix, backfill meter, current tick — with every tick wrapped in `Step.run(row.name, row.clazz, …)` so it carries the class's deadline geometry and durable exit, and each execution's step key is the tick's `DateTime.formatIso` instant — a re-fired tick replays its recorded exit instead of double-running.
- Law: exactly-once-per-tick is the cluster fact — the cron rides the singleton substrate, migrating on rebalance, firing once per cluster regardless of runner count; a per-runner guard, a lock row, or a "am I the leader" read beside it is unspellable.
- Law: catch-up is a bounded computed prefix, not an open loop — on execution the pass reads the last-run mark through the caller-supplied `marks` projection (an audit read over the `cadence.backfilled`/step-exit evidence — read-side material, so the mint carries no run-history table), walks `Cron.sequenceReverse(cron, now)` only until the last run, filters ticks older than the misfire horizon, and applies the posture row: `skip` takes none, `once` takes the latest missed instant, and `all` takes at most the row's `backfill` ceiling before restoring chronological execution order. Every replay uses the same instant-keyed step, and each non-empty pass records a meter fact, so a long outage cannot allocate an unbounded tick roster or conceal truncated recovery policy.
- Law: `Date` lives only at the `Cron.sequenceReverse` seam — the iterator's platform `Date` converts through `DateTime.unsafeFromDate` inside the seam's own `Iterable.map`, so the cadence-domain callback, the tick identity, and every comparison ride `DateTime.Utc`; a `Date`-typed callback or a `toISOString` identity is the host-scalar leak `values.md` names.
- Receipt: every tick's step exit is the durable run evidence — instant, verdict, duration — the operator census reads; no separate run-history table exists.
- Growth: a new posture is one `Anchor` discriminant and one `_missed` arm; per-row notification is a tap on the row's body at its owner.
- Packages: `@effect/cluster` (`ClusterCron`); `effect` (`Cron`, `Array`, `Iterable`, `Layer`, `DateTime`); `./flow.ts` (`Step`); `@rasm/ts/data` (`Fact` — the backfill meter).

```typescript
const _missed = (row: Cadence.Row, lastRun: DateTime.Utc, now: DateTime.Utc): ReadonlyArray<DateTime.Utc> => {
  if (row.catchUp === "skip") return []
  const horizon = DateTime.subtractDuration(now, row.misfire)
  const ceiling = row.catchUp === "once" ? 1 : row.backfill
  return Array.reverse(Array.fromIterable(
    Iterable.take(
      Iterable.filter(
        Iterable.takeWhile(
          Iterable.map(Cron.sequenceReverse(row.cron, now), DateTime.unsafeFromDate),
          (tick) => DateTime.lessThan(lastRun, tick),
        ),
        (tick) => DateTime.greaterThanOrEqualTo(tick, horizon),
      ),
      ceiling,
    ),
  ))
}

const _step = <R>(row: Cadence.Row, run: (row: Cadence.Row, tick: DateTime.Utc) => Effect.Effect<void, never, R>, tick: DateTime.Utc) =>
  Step.run(`${row.name}@${DateTime.formatIso(tick)}`, row.clazz, {
    success: Schema.Void,
    error: Schema.Never,
    execute: run(row, tick),
  })

const _caughtUp = <R, R2>(
  row: Cadence.Row,
  run: (row: Cadence.Row, tick: DateTime.Utc) => Effect.Effect<void, never, R>,
  marks: (name: string) => Effect.Effect<Option.Option<DateTime.Utc>, never, R2>,
  now: DateTime.Utc,
) =>
  Effect.gen(function* () {
    const last = yield* marks(row.name)
    const backlog = _missed(row, Option.getOrElse(last, () => now), now)
    yield* Effect.forEach(backlog, (tick) => _step(row, run, tick), { discard: true })
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
  })

const _fired = <R, R2>(
  row: Cadence.Row,
  run: (row: Cadence.Row, tick: DateTime.Utc) => Effect.Effect<void, never, R>,
  marks: (name: string) => Effect.Effect<Option.Option<DateTime.Utc>, never, R2>,
) =>
  Effect.gen(function* () {
    const now = yield* DateTime.now
    yield* _caughtUp(row, run, marks, now)
    yield* _step(row, run, now)
  })

const _cluster = <R, R2>(
  table: Cadence.Table,
  run: (row: Cadence.Row, tick: DateTime.Utc) => Effect.Effect<void, never, R>,
  marks: (name: string) => Effect.Effect<Option.Option<DateTime.Utc>, never, R2>,
) =>
  Layer.mergeAll(
    ...Array.map(_rows(table), (row) =>
      ClusterCron.make({
        name: row.name,
        cron: row.cron,
        shardGroup: row.group,
        calculateNextRunFromPrevious: row.anchor === "previous",
        skipIfOlderThan: row.misfire,
        execute: _fired(row, run, marks),
      })),
  )
```

## [04]-[HOST_ROW]

[HOST_ROW]:
- Owner: `Cadence.host(table, run, marks)` — the in-process engine over identical rows and the SAME `_fired` pass: each row performs one boot recovery through `_caughtUp`, then advances a `Schedule.driver(Schedule.cron(row.cron))` before every `_fired` pass under a scoped daemon fiber. Advancing the driver first is load-bearing because `Effect.repeat(effect, schedule)` executes `effect` once before consulting the schedule, so the unadvanced driver mints an off-calendar current tick at boot. Every scheduled tick carries the same `Step.run` geometry against the `WorkflowEngine` Tag — an engineless process satisfies it with `WorkflowEngine.layerMemory`, so no bare-body second execution shape exists. Two consumers select this row: the single-node process whose grid runs the local entry, and the data wave's maintenance jobs whose in-database cron grant is refused — the grooming and rebuild schedules degrade to host execution through this fold without touching their row shapes.
- Law: the fallback is honest about its guarantees — host cadence is at-most-once-per-process (a dead process misses ticks until restart, and the boot pass replays them through the catch-up prefix where a durable mark exists); a consumer whose ticks may never be lost belongs on the cluster row, and the table's engine selection is the root's deployment fact.
- Law: one engine per row per process — the root selects `cluster` or `host` for the whole table; a row running on both engines double-fires and the selection type makes that unspellable.
- Growth: a third engine (an edge scheduler, a test-clock driver) is one more fold over the same table and the same `_fired` pass; specs drive `host` under `TestClock` with no cluster.
- Packages: `effect` (`Schedule`, `Layer`); `@effect/workflow` (`WorkflowEngine.layerMemory` — the engineless host's step substrate, selected at the root).

```typescript
const _host = <R, R2>(
  table: Cadence.Table,
  run: (row: Cadence.Row, tick: DateTime.Utc) => Effect.Effect<void, never, R>,
  marks: (name: string) => Effect.Effect<Option.Option<DateTime.Utc>, never, R2>,
) =>
  Layer.scopedDiscard(
    Effect.forEach(_rows(table), (row) =>
      Effect.forkScoped(Effect.gen(function* () {
        const booted = yield* DateTime.now
        yield* _caughtUp(row, run, marks, booted)
        const driver = yield* Schedule.driver(Schedule.cron(row.cron))
        yield* Stream.runDrain(Stream.repeatEffectOption(
          Effect.zipRight(driver.next(undefined), _fired(row, run, marks)),
        ))
      })), { discard: true }),
  )

const Cadence = {
  Backfill,
  rows: _rows,
  cluster: _cluster,
  host: _host,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cadence }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
