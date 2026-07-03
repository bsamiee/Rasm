# [TELEMETRY_BURNRATE]

SLO is algebra, not config: an `Objective` is a typed policy value — an SLI over `Convention` metric rows, a target ratio, a compliance window — and the multi-window multi-burn-rate discipline is one closed `_BURN` table whose four rows carry severity, long/short window pair, burn factor, and budget share. Every downstream artifact is a total derivation over these two values: `Slo.evaluate` folds window readings into a fired/quiet verdict, `Slo.budget` computes the error-budget arithmetic, `slo/alert` derives one alert spec per burn row, `board` renders the same rows as panels, and `iac/observe` compiles them into provider rules — so a threshold change is one row edit that moves the runtime verdict, the alerts, and the dashboards in a single diff. Evaluation is pure and source-agnostic: readings arrive as sampled error rates, and who sampled them (a board query, a rule engine, a runtime probe over metric snapshots) is the caller's seam.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                  |
| :-----: | :---------- | :------------------------------------------------------------------------ |
|  [01]   | [OBJECTIVE] | the `Objective` policy value and the closed `Sli` family                   |
|  [02]   | [BURN_ROWS] | the multi-window multi-burn-rate table and its derivations                 |
|  [03]   | [ALGEBRA]   | burn/budget arithmetic and the windowed verdict fold                       |

## [2]-[OBJECTIVE]

[OBJECTIVE]:
- Owner: the `Sli` closed family and the `Objective` row — `Sli` is a process-local `Data.taggedEnum` with two cases: `Ratio` (good-events metric over total-events metric) and `Latency` (a duration metric against a ceiling at a quantile); `Objective` binds one `Sli` to a `target` ratio and a compliance `window`.
- Law: an SLI names its series through `Convention.MetricName` rows only — `Convention.metric.httpServerDuration` for the standing latency objective, `Convention.metric.meterUsage`-derived ratios for usage objectives — so an objective cannot reference a series the plane does not emit, and a metric rename breaks every objective at compile time.
- Law: `target` is the good-ratio (`0 < target < 1`) and the error budget is its complement — `1 - target` — fixed at the objective, never recomputed differently per consumer; `window` is the compliance horizon the budget amortizes over (the 28-day standing default).
- Law: the family is process-local by design (`Data.taggedEnum`, not a Schema union) — objectives are lib-authored policy values composed at build time; a wire-carried objective store is an app concern that would promote the family to Schema case owners in one declaration edit.
- Entry: objectives are plain values — `{ name, sli: Sli.Ratio({ good, total }), target: 0.999, window }` — composed where the app declares its reliability policy.
- Growth: a new SLI shape (saturation, freshness) is one `Sli` case plus its arm in the two folds below.

```typescript
import { Data, type Duration } from "effect"
import type { Convention } from "@rasm/ts/telemetry"

type Sli = Data.TaggedEnum<{
  Latency: { readonly ceiling: Duration.Duration; readonly metric: Convention.MetricName; readonly quantile: number }
  Ratio: { readonly good: Convention.MetricName; readonly total: Convention.MetricName }
}>
const Sli: Data.TaggedEnum.Constructor<Sli> = Data.taggedEnum<Sli>()

declare namespace Slo {
  type Objective = {
    readonly name: string
    readonly sli: Sli
    readonly target: number
    readonly window: Duration.Duration
  }
}
```

## [3]-[BURN_ROWS]

[BURN_ROWS]:
- Owner: the `_BURN` table — the standing multi-window multi-burn-rate discipline as four rows: two paging pairs (2% of budget in 1h at 14.4x burn over 5m/1h windows; 5% in 6h at 6x over 30m/6h) and two ticketing pairs (10% in 1d at 3x over 2h/1d; 10% in 3d at 1x over 6h/3d); each row carries `severity`, `long`, `short`, `factor`, and `spend`.
- Law: the two-window trip is the false-positive/reset discipline — the long window proves sustained burn, the short window proves it is still burning now, and a verdict fires only when BOTH exceed the row's factor; the short window is what lets a resolved incident reset quickly instead of paging for the tail of its own long window.
- Law: the row set derives — `keyof typeof _BURN` is the burn-kind union, the severity axis projects from rows, and the guard pair closes the table — so `slo/alert` derives one spec per row and `board` one threshold pair per row with zero re-listing.
- Law: `factor` and `spend` are redundant by construction (`spend = factor * long / window` at the standing 28-day window) and both are carried anyway — `factor` drives evaluation, `spend` states the human budget meaning an alert annotation prints — with the consistency provable from the row itself.
- Growth: a tuned discipline (a fifth row, a different factor) is a table edit; consumers re-derive.

```typescript
const _BURN = {
  pageFast: { factor: 14.4, long: "1 hour", severity: "page", short: "5 minutes", spend: 0.02 },
  pageSlow: { factor: 6, long: "6 hours", severity: "page", short: "30 minutes", spend: 0.05 },
  ticketFast: { factor: 3, long: "1 day", severity: "ticket", short: "2 hours", spend: 0.1 },
  ticketSlow: { factor: 1, long: "3 days", severity: "ticket", short: "6 hours", spend: 0.1 },
} as const

declare namespace Slo {
  type Burn = keyof typeof _BURN
  type BurnRow = {
    readonly factor: number
    readonly long: Duration.DurationInput
    readonly severity: "page" | "ticket"
    readonly short: Duration.DurationInput
    readonly spend: number
  }
  type _Rows<T extends Record<Burn, BurnRow> = typeof _BURN> = T
  type _Keys<K extends Burn = keyof typeof _BURN> = K
}
```

## [4]-[ALGEBRA]

[ALGEBRA]:
- Owner: the assembled `Slo` export — the burn table spread in, the arithmetic members, and the verdict fold under one name with companion types on the merged hub.
- Law: burn rate is `errorRate / (1 - target)` — the multiple of budget-consumption speed — and `Slo.burn` is that one division, `Option`-returning because a degenerate `target = 1` objective has no budget to divide by and the absence folds at the caller, never as `Infinity` downstream.
- Law: `Slo.evaluate(objective, readings)` is total over its readings — a `Record<Burn, { long, short }>` of sampled error rates — and returns the verdict per row (`fired` exactly when both windows' burn meets the row factor) plus the fired severity ceiling; sampling the readings is the caller's seam, so the same fold serves a runtime probe, a spec fixture, and a rule compiler.
- Law: budget arithmetic is closed at the objective — `Slo.budget(objective)` yields the error budget ratio, and `Slo.spent(objective, errorRate, elapsed)` the budget fraction consumed by a measured rate over an elapsed span (`errorRate * elapsed / (budget * window)`), the number an incident review reads.
- Receipt: `Verdict` — per-row fired flags with their burn readings plus the dominant severity as `Option` — data a caller routes on, never a side effect; emission belongs to `slo/alert` specs and runtime consumers.
- Entry: `Slo.evaluate(objective, readings)`; `Slo.burn(objective, errorRate)`; `Slo.budget(objective)`; `Slo.spent(objective, errorRate, elapsed)`; `Slo.rows` for derivers.
- Growth: a new verdict axis is one field on the fold's construction — the table and arithmetic are closed.

```typescript
import { Array, Duration, Option, Record } from "effect"

declare namespace Slo {
  type Reading = { readonly long: number; readonly short: number }
  type RowVerdict = { readonly burn: Reading; readonly fired: boolean; readonly row: BurnRow }
  type Verdict = {
    readonly rows: { readonly [K in Burn]: RowVerdict }
    readonly severity: Option.Option<"page" | "ticket">
  }
}

const _budget = (objective: Slo.Objective): number => 1 - objective.target

const _burnOf = (objective: Slo.Objective, errorRate: number): Option.Option<number> => {
  const budget = _budget(objective)
  return budget > 0 ? Option.some(errorRate / budget) : Option.none()
}

const _evaluate = (objective: Slo.Objective, readings: Record<Slo.Burn, Slo.Reading>): Slo.Verdict => {
  const rows = Record.map(_BURN, (row, kind): Slo.RowVerdict => {
    const reading = readings[kind]
    const fired = Option.match(
      Option.zipWith(_burnOf(objective, reading.long), _burnOf(objective, reading.short), (long, short) =>
        long >= row.factor && short >= row.factor),
      { onNone: () => false, onSome: (both) => both },
    )
    return { burn: reading, fired, row }
  })
  const fired = Array.filter(Record.values(rows), (verdict) => verdict.fired)
  return {
    rows,
    severity: Array.some(fired, (verdict) => verdict.row.severity === "page")
      ? Option.some("page")
      : Array.isNonEmptyReadonlyArray(fired)
        ? Option.some("ticket")
        : Option.none(),
  }
}

const _spent = (objective: Slo.Objective, errorRate: number, elapsed: Duration.Duration): number =>
  Option.match(_burnOf(objective, errorRate), {
    onNone: () => 0,
    onSome: (burn) => (burn * Duration.toMillis(elapsed)) / Duration.toMillis(objective.window),
  })

const Slo: {
  readonly budget: (objective: Slo.Objective) => number
  readonly burn: (objective: Slo.Objective, errorRate: number) => Option.Option<number>
  readonly evaluate: (objective: Slo.Objective, readings: Record<Slo.Burn, Slo.Reading>) => Slo.Verdict
  readonly rows: typeof _BURN
  readonly spent: (objective: Slo.Objective, errorRate: number, elapsed: Duration.Duration) => number
} = {
  budget: _budget,
  burn: _burnOf,
  evaluate: _evaluate,
  rows: _BURN,
  spent: _spent,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Sli, Slo }
```
