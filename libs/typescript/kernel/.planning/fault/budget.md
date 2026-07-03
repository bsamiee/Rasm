# [KERNEL_BUDGET]

The resilience budget vocabulary every folder policy types against: `Budget` carries the retry/timeout rows — base curve, growth factor, attempt and elapsed bounds, reset window, per-attempt and whole-call deadlines — and compiles each row once into a jittered, class-gated `Schedule` value; `Degrade` carries the connection-degradation ladder long-lived feeds fold silence through. A hand-rolled retry loop, an inline schedule rebuilt per declaration, or a reconnect cadence invented at a call site is the named defect: `wire/client` retry/backoff, `work/activity` retry/timeout, EventLog sync and SSE reconnection, and `host/net` client policy all reference these rows, so one budget edit retunes every surface that names it. The module is `kernel/src/fault/budget.ts`; a new budget is one row, a new degradation posture is one ladder rung.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                            | [PUBLIC]  |
| :-----: | :-------------- | :------------------------------------------------------------------ | :-------- |
|  [01]   | `RETRY_BUDGET`  | the budget rows and their compiled class-gated `Schedule` values    | `Budget`  |
|  [02]   | `DEGRADE_LADDER` | the silence-threshold ladder and its level fold                    | `Degrade` |

## [2]-[RETRY_BUDGET]

[RETRY_BUDGET]:
- Owner: `Budget`, the assembled budget vocabulary — the interior key tuple anchors the roster, the row table carries every axis as `Duration` policy values, the merged hub carries derived types plus the guard pair, and the exported owner assembles rows, `kinds`, and the `schedule` lookup under a `typeof`-derived stated annotation.
- Law: four rows ride the floor — `pulse` (interactive point ops: 40ms base, 4 attempts, 2s window), `lease` (infrastructure ops: 250ms base, 6 attempts, 20s window), `bulk` (batch work: 1s base, 8 attempts, 5m window), `feed` (long-lived reconnection: 500ms base, 64 attempts, 2m window, 90s reset) — floors a folder policy references by kind; a genuinely novel envelope is a new row, never a per-site literal.
- Law: every row carries the two deadline budgets the rails layering law consumes — `attempt` composes below the retry transformer (per-try), `total` above it (whole-call) — so `work/activity` and `wire/client` read `Budget[kind].attempt`/`.total` and the budget's whole geometry lives in one row.
- Law: compilation is fixed-form and total — `exponential(base, factor)` → `jittered` → `resetAfter(reset)` → `intersect(recurs(attempts))` → `upTo(window)` — jitter is unconditional (a bare curve synchronizes a fleet into waves), `resetAfter` re-arms base delay after quiet so the next outage never inherits the last one's escalated tail, and the attempt/elapsed bounds stack because a budget names both.
- Law: every compiled schedule is class-gated through `Schedule.whileInput` over `FaultClass.retryable` — only the transient family re-drives, the gate travels with the policy value, and a call-site predicate re-deriving retryability is policy leakage; the compile happens once at module init into a governed record whose stated annotation demands a schedule per row.
- Law: the schedule input is `unknown` — one policy value serves every fault channel in the branch, and classification, not typing, decides re-drive.
- Growth: a new budget is one tuple entry plus one row — the governed compile record breaks at compile time until its schedule lands; a new axis (a fleet-cost weight, a hedge delay) is one `Row` field consumed by the surfaces that name it.
- Boundary: which budget a surface selects is that folder's policy row; deadline transformers (`Effect.timeoutFail`) compose at the owning `Effect.fn` seam with the row's durations — the kernel ships values, never wrappers.
- Packages: `effect` (`Duration`, `Schedule`); `kernel/fault/classify` (`FaultClass.retryable`).

```typescript
import { Array, Duration, Option, Schedule, type Types } from "effect"
import { FaultClass } from "./classify.ts"

const _kinds = ["pulse", "lease", "bulk", "feed"] as const
const _rows = {
  pulse: {
    base: Duration.millis(40),
    factor: 2,
    attempts: 4,
    window: Duration.seconds(2),
    reset: Duration.seconds(30),
    attempt: Duration.seconds(1),
    total: Duration.seconds(8),
  },
  lease: {
    base: Duration.millis(250),
    factor: 2,
    attempts: 6,
    window: Duration.seconds(20),
    reset: Duration.seconds(90),
    attempt: Duration.seconds(5),
    total: Duration.seconds(45),
  },
  bulk: {
    base: Duration.seconds(1),
    factor: 2,
    attempts: 8,
    window: Duration.minutes(5),
    reset: Duration.minutes(10),
    attempt: Duration.minutes(2),
    total: Duration.minutes(15),
  },
  feed: {
    base: Duration.millis(500),
    factor: 2,
    attempts: 64,
    window: Duration.minutes(2),
    reset: Duration.seconds(90),
    attempt: Duration.seconds(10),
    total: Duration.minutes(30),
  },
} as const

declare namespace Budget {
  type Kinds = typeof _kinds
  type Kind = keyof typeof _rows
  type Row = {
    readonly base: Duration.Duration
    readonly factor: number
    readonly attempts: number
    readonly window: Duration.Duration
    readonly reset: Duration.Duration
    readonly attempt: Duration.Duration
    readonly total: Duration.Duration
  }
  type Contract = { readonly [K in Kinds[number]]: Row }
  type Gated = Schedule.Schedule<[Duration.Duration, number], unknown>
  type Shape = Types.Simplify<
    typeof _rows & {
      readonly kinds: Kinds
      readonly schedule: (kind: Kind) => Gated
    }
  >
  type _Rows<T extends Contract = typeof _rows> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const _gated = (row: Budget.Row): Budget.Gated =>
  Schedule.exponential(row.base, row.factor).pipe(
    Schedule.jittered,
    Schedule.resetAfter(row.reset),
    Schedule.intersect(Schedule.recurs(row.attempts)),
    Schedule.upTo(row.window),
    Schedule.whileInput(FaultClass.retryable),
  )

const _compiled: { readonly [K in Budget.Kind]: Budget.Gated } = {
  pulse: _gated(_rows.pulse),
  lease: _gated(_rows.lease),
  bulk: _gated(_rows.bulk),
  feed: _gated(_rows.feed),
}

const Budget: Budget.Shape = {
  ..._rows,
  kinds: _kinds,
  schedule: (kind) => _compiled[kind],
}
```

## [3]-[DEGRADE_LADDER]

[DEGRADE_LADDER]:
- Owner: `Degrade`, the connection-degradation ladder — an interior level tuple in escalation order (a load-bearing sequence: each rung's silence threshold exceeds its predecessor's), a row table carrying per-level entry threshold and probe cadence, and the exported owner assembling rows, `levels`, and the `level` fold under a stated annotation.
- Law: three rungs ride the ladder — `live` (healthy: zero threshold, 30s heartbeat cadence), `lagging` (10s of silence: 5s probe cadence), `severed` (2m of silence: 30s probe cadence) — and `level(silence)` folds an observed silence span to its rung by walking the tuple from the top, so the ladder's shape is data and the fold never spells a level name.
- Law: the ladder is a reconnection BUDGET, not evidence — EventLog sync, the `host/flag` SSE stream, and `edge/live` presence fold their silence through it to pick probe cadence; the wire-decoded `DegradationLevel`/`CommandAvailability` evidence vocabulary is `state/evidence`'s sibling concern and the two never merge.
- Law: cadence at a rung is a `Duration` policy value a consumer hands to `Schedule.spaced` or `Stream.repeatEffectWithSchedule` at its own seam — the ladder prices the probe, the surface owns the loop.
- Growth: a new rung is one tuple entry plus one row in threshold order; a per-surface ladder override is a caller-composed row set folded through the same `level` shape.
- Boundary: what counts as silence — missed heartbeats, an idle socket, a stalled pull — is the consuming surface's measurement; the ladder folds the span it is handed.
- Packages: `effect` (`Duration`, `Option`, `Array`); the ladder is class-free by design and composes nothing from `classify`.

```typescript
const _levels = ["live", "lagging", "severed"] as const
const _ladder = {
  live: { after: Duration.zero, cadence: Duration.seconds(30) },
  lagging: { after: Duration.seconds(10), cadence: Duration.seconds(5) },
  severed: { after: Duration.minutes(2), cadence: Duration.seconds(30) },
} as const

declare namespace Degrade {
  type Levels = typeof _levels
  type Kind = keyof typeof _ladder
  type Row = { readonly after: Duration.Duration; readonly cadence: Duration.Duration }
  type Contract = { readonly [K in Levels[number]]: Row }
  type Shape = Types.Simplify<
    typeof _ladder & {
      readonly levels: Levels
      readonly level: (silence: Duration.DurationInput) => Kind
    }
  >
  type _Rows<T extends Contract = typeof _ladder> = T
  type _Keys<K extends keyof Contract = Kind> = K
}

const _leveled = (silence: Duration.DurationInput): Degrade.Kind =>
  Option.getOrElse(
    Array.findLast(_levels, (kind) => Duration.greaterThanOrEqualTo(silence, _ladder[kind].after)),
    () => "live",
  )

const Degrade: Degrade.Shape = {
  ..._ladder,
  levels: _levels,
  level: _leveled,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Budget, Degrade }
```
