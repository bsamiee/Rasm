# [WORK_SCHEDULE]

Calendar recurrence is a vocabulary, and every durable timer is named: `Cadence` owns the schedule rows — cron expression, misfire window, and anchor semantics as data — and mints each row into a `ClusterCron` registration whose singleton-per-name execution, missed-window skip, and next-run derivation are engine capability. Misfire policy is the `window` column lowered to `skipIfOlderThan`: a runner waking after an outage runs a missed job only while it is younger than its window, so a nightly sweep never storms at noon. Anchor semantics are the `anchor` column lowered to `calculateNextRunFromPrevious`: `clock` rows fire on the wall-clock grid regardless of run duration, `previous` rows measure from the prior run's completion so long jobs never overlap themselves. The durable pause completes the page: `Cadence.pause` names a `DurableClock` sleep that survives restarts, with the folder threshold row deciding when a short sleep stays in memory. A raw cron string at a call site, a `setInterval`, an unnamed sleep, and a second scheduler beside the engine are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                  |
| :-----: | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | [CADENCE_ROWS]  | the schedule vocabulary — expression, misfire window, anchor — and the mint  |
|  [02]   | [DURABLE_PAUSE] | the named durable sleep, the in-memory threshold row, the owner assembly    |

## [2]-[CADENCE_ROWS]

[CADENCE_ROWS]:
- Owner: the cadence rows and their mint. Rows are the folder's recurrence floor: `hourly` (`0 * * * *`, a one-hour window, `clock` anchor), `nightly` (`0 4 * * *`, a six-hour window, `clock` anchor), `weekly` (`0 5 * * 1`, a one-day window, `previous` anchor). `Cadence.job(kind, name, execute)` lifts a row into the durable registration: the expression parses on the rail — `Cron.parse` is a pure carrier composing straight into `Layer.unwrapEffect`, so a malformed expression is a typed layer-construction fault at the composition root, never a runtime surprise — and `ClusterCron.make` receives the parsed cron, the row's misfire window, and the row's anchor as fields.
- Law: misfire is a window, not a boolean — `skipIfOlderThan: row.window` means a missed occurrence still runs if the cluster recovers inside the window and is skipped once staler, so the policy question "how late is too late" is answered per row and inherited by every job on that row.
- Law: anchor is overlap semantics — `calculateNextRunFromPrevious: true` (`previous`) makes the next occurrence derive from the last completion, the non-overlapping form for jobs whose duration can exceed their period; `false` (`clock`) holds the wall-clock grid for jobs whose cadence is a contract. The column is the whole decision; no job body ever checks whether it is already running.
- Law: execution is cluster-fenced — `ClusterCron` registers through `Sharding`, one occurrence fires across the whole fleet, and a tenant-partitioned schedule rides the mint's optional shard group through `Fence.group`; a per-process timer beside it would fire once per runner.
- Law: the body is durable work — `execute` composes `Step.run` bodies and `Job.family` enqueues, because a cron body that does long work inline holds its occurrence open; the standing pattern is cron-enqueues, family-drains.
- Boundary: the engine assembly the registration rides is `engine/entity.md`'s; the drain cadence of the outbox relay is `deliver/relay.md`'s own policy row, not a cadence row here.
- Entry: `Cadence.job("nightly", "<name>", execute)` merged at the composition root.
- Growth: a new recurrence is one row; a new policy axis (a jitter spread, a blackout calendar) is one `Row` field lowered inside `job`.
- Packages: `@effect/cluster` (`ClusterCron`), `effect` (`Cron`, `Duration`, `Effect`, `Layer`, `Types`).

```typescript
import { ClusterCron } from "@effect/cluster"
import { Cron, Duration, Effect, Layer, type Types } from "effect"

const _kinds = ["hourly", "nightly", "weekly"] as const
const _rows = {
  hourly: { expr: "0 * * * *", window: Duration.hours(1), anchor: "clock" },
  nightly: { expr: "0 4 * * *", window: Duration.hours(6), anchor: "clock" },
  weekly: { expr: "0 5 * * 1", window: Duration.days(1), anchor: "previous" },
} as const

declare namespace Cadence {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _rows
  type Anchor = "clock" | "previous"
  type Row = { readonly expr: string; readonly window: Duration.Duration; readonly anchor: Anchor }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const _job = <E, R>(
  kind: Cadence.Kind,
  name: string,
  execute: Effect.Effect<void, E, R>,
): Layer.Layer<never, Cron.ParseError, R> =>
  Layer.unwrapEffect(
    Effect.map(Cron.parse(_rows[kind].expr), (cron) =>
      ClusterCron.make({
        name,
        cron,
        execute,
        calculateNextRunFromPrevious: _rows[kind].anchor === "previous",
        skipIfOlderThan: _rows[kind].window,
      }),
    ),
  )
```

## [3]-[DURABLE_PAUSE]

[DURABLE_PAUSE]:
- Owner: `Cadence.pause` — the named durable sleep, assembled beside the rows into the one exported owner. `pause(name, duration)` lowers to `DurableClock.sleep` with the folder threshold row: a sleep at or under the threshold runs in memory (a restart re-waits the short span, which is cheaper than persisting it), a longer sleep persists through the engine so a three-day cooling-off period survives any number of restarts and resumes with the remainder.
- Law: the threshold is one policy value — thirty seconds, declared once beside the rows — so the memory-versus-durable split is a folder fact, never a per-call judgment; a caller that needs a different split is describing a new policy row, not an argument.
- Law: every pause is named — the name is the durable timer's identity across restarts, so two workflows sleeping under one name collide by design intent, and an anonymous sleep inside a durable body is the replay hazard this member exists to delete.
- Boundary: in-memory `Effect.sleep` remains legal outside durable bodies; inside a workflow, this member is the only sleep spelling.
- Entry: `yield* Cadence.pause("<name>", "3 days")` inside a workflow body.
- Growth: a second threshold class is one more `_PAUSE` field consumed by the same member.
- Packages: `@effect/workflow` (`DurableClock`, `WorkflowEngine`), `effect` (`Duration`, `Effect`).

```typescript
import { DurableClock, type WorkflowEngine } from "@effect/workflow"

const _PAUSE = { threshold: Duration.seconds(30) } as const

const _pause = (
  name: string,
  duration: Duration.DurationInput,
): Effect.Effect<void, never, WorkflowEngine.WorkflowEngine | WorkflowEngine.WorkflowInstance> =>
  DurableClock.sleep({ name, duration, threshold: _PAUSE.threshold })

declare namespace Cadence {
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: Kinds
      readonly job: typeof _job
      readonly pause: typeof _pause
    }
  >
}

const Cadence: Cadence.Shape = { ..._rows, kinds: _kinds, job: _job, pause: _pause }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cadence }
```
