# [CORE_SLO]

SLO is algebra, not config, and alerting is its total derivation: an `Objective` is a typed policy value — an SLI over `Convention` metric rows, a target ratio, a compliance window — the multi-window multi-burn-rate discipline is one closed `_BURN` table whose four rows carry severity, long/short window pair, burn factor, and budget share, and `Alert.of` derives one compilation-ready spec per burn row with zero re-decided thresholds. The `Sli` family covers the four-kind SRE taxonomy — event ratio, latency threshold, saturation share, freshness age — and every case states its own breach predicate as data, so the one sampled shape (`breaching` over `total`) feeds one error-rate fold regardless of kind. Every downstream artifact is a projection over these values: `Slo.evaluate` folds window readings into a fired/quiet verdict, `Slo.budget` computes the error-budget arithmetic, `board#PACKS` renders the same rows as breach-rate panels and firing annotations, and `iac` compiles the specs into provider alert rules — so a threshold change is one row edit that moves the runtime verdict, the alerts, and the dashboards in a single diff. Evaluation is pure and source-agnostic: readings arrive as sampled error rates, and who sampled them (a board query, a rule engine, a runtime probe over metric snapshots) is the caller's seam. Delivery is out of scope by law — a spec says WHAT fires and HOW urgent, and the notification transport is the deploy plane's routing concern. The module is `core/src/observe/slo.ts`; a hand-authored alert rule beside this derivation is the drift defect the total function exists to kill.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                    |
| :-----: | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `OBJECTIVE`   | the `Objective` policy value, the four-kind `Sli` family, the sample fold    |
|  [02]   | `BURN_ROWS`   | the multi-window multi-burn-rate table and its derivations                   |
|  [03]   | `ALGEBRA`     | burn/budget arithmetic and the windowed verdict fold                         |
|  [04]   | `ALERT_SPECS` | the severity routing rows and the `Objective -> specs` total derivation      |

## [2]-[OBJECTIVE]

[OBJECTIVE]:
- Owner: the `Sli` closed family and the `Objective` row — `Sli` is a process-local `Data.taggedEnum` with four cases: `Ratio` (good-events metric over total-events metric), `Latency` (a duration metric against a ceiling, with the display quantile the panels headline), `Saturation` (a utilization metric against a share ceiling), and `Freshness` (an age metric against a staleness horizon); `Objective` binds one `Sli` to a `target` ratio and a compliance `window`.
- Law: an SLI names its series through `Convention.MetricName` rows only — `Convention.metric.httpServerDuration` for the standing latency objective, `Convention.metric.meterUsage`-derived ratios for usage objectives — so an objective cannot reference a series the plane does not emit, and a metric rename breaks every objective at compile time.
- Law: every case defines its breach predicate as its own fields, and the sampled shape is uniform — `Slo.Sample` is `{ breaching, total }` where `breaching` counts the case's own breach events: requests over `ceiling` for `Latency`, bad events (`total - good`) for `Ratio`, samples above the utilization `ceiling` for `Saturation`, samples older than `horizon` for `Freshness` — and `Sli.rate(sample)` is the one error-rate fold, `Option`-returning because an empty window has no rate and the absence folds at the caller, never as `NaN` downstream. The `Latency` `ceiling` is therefore load-bearing on both sides: it defines which requests count as breaching at the sampler, and `board#PACKS` compiles it into the le-share breach expression — the `quantile` is display vocabulary only and never enters the burn arithmetic.
- Law: `target` is the good-ratio (`0 < target < 1`) and the error budget is its complement — `1 - target` — fixed at the objective, never recomputed differently per consumer; `window` is the compliance horizon the budget amortizes over (the 28-day standing default). The degenerate `target = 1` objective has no budget, and every arithmetic member below carries that absence as `Option` rather than a division guard per consumer.
- Law: the family is process-local by design (`Data.taggedEnum`, not a Schema union) — objectives are lib-authored policy values composed at build time, and `Objective` stays the bare policy `type` for the same reason; a wire-carried objective store is an app concern that promotes the family to Schema case owners in one declaration edit, and that promotion is where the `target` bound becomes a `Schema.between` refinement.
- Entry: objectives are plain values — `{ name, sli: Sli.Ratio({ good, total }), target: 0.999, window }` — composed where the app declares its reliability policy; `Sli.rate(sample)` at every sampling seam.
- Growth: a fifth SLI shape is one `Sli` case plus its breach-expression arm in `board#PACKS` `_breach` — the sample fold and the burn algebra are already kind-agnostic.

```typescript
import { Array, Data, Duration, Number, Option, Order, Record, Struct } from "effect"
import { Convention } from "./convention.ts"

type Sli = Data.TaggedEnum<{
  Freshness: { readonly horizon: Duration.Duration; readonly metric: Convention.MetricName }
  Latency: { readonly ceiling: Duration.Duration; readonly metric: Convention.MetricName; readonly quantile: number }
  Ratio: { readonly good: Convention.MetricName; readonly total: Convention.MetricName }
  Saturation: { readonly ceiling: number; readonly metric: Convention.MetricName }
}>
const _Sli = Data.taggedEnum<Sli>()

const Sli: Data.TaggedEnum.Constructor<Sli> & {
  readonly rate: (sample: Slo.Sample) => Option.Option<number>
} = {
  ..._Sli,
  rate: ({ breaching, total }) => Number.divide(breaching, total),
}

declare namespace Slo {
  type Sample = { readonly breaching: number; readonly total: number }
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
- Law: the two-window trip is the false-positive/reset discipline — the long window proves sustained burn, the short window proves it is still burning now, and a verdict fires only when BOTH exceed the row's factor; the short window is what lets a resolved incident reset quickly instead of paging for the tail of its own long window. Every consumer honors both halves — the runtime probe samples both windows, and `board#PACKS` renders both burn expressions per row, never the long half alone.
- Law: the row set derives — `keyof typeof _BURN` is the burn-kind union, the severity axis projects from rows, and the guard pair closes the table — so `[04]` derives one spec per row and `board#PACKS` one threshold pair per row with zero re-listing.
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
  type _Rows<T extends { readonly [K in Burn]: BurnRow } = typeof _BURN> = T
  type _Keys<K extends Burn = keyof typeof _BURN> = K
}
```

## [4]-[ALGEBRA]

[ALGEBRA]:
- Owner: the assembled `Slo` export — the burn table spread in, the arithmetic members, and the verdict fold under one name with companion types on the merged hub.
- Law: burn rate is `errorRate / (1 - target)` — the multiple of budget-consumption speed — and `Slo.burn` is that one division, `Option`-returning because a degenerate `target = 1` objective has no budget to divide by and the absence folds at the caller, never as `Infinity` downstream.
- Law: `Slo.evaluate(objective, readings)` is total over its readings — `Slo.Readings`, one sampled long/short error-rate pair per burn row, each rate the output of `Sli.rate` over the case's own breach sample — and returns the verdict per row (`fired` exactly when both windows' burn meets the row factor) plus the fired severity ceiling, the dominant fired severity folded through the one `_bySeverity` `Order`; sampling the readings is the caller's seam, so the same fold serves a runtime probe, a spec fixture, and a rule compiler.
- Law: budget arithmetic is closed at the objective — `Slo.budget(objective)` yields the error budget ratio, and `Slo.spent(objective, errorRate, elapsed)` the budget fraction consumed by a measured rate over an elapsed span, the number an incident review reads.
- Receipt: `Verdict` — per-row fired flags with their burn readings plus the dominant severity as `Option` — data a caller routes on, never a side effect; emission belongs to `[05]` specs and runtime consumers.
- Entry: `Slo.evaluate(objective, readings)`; `Slo.burn(objective, errorRate)`; `Slo.budget(objective)`; `Slo.spent(objective, errorRate, elapsed)`; `Slo.rows` for derivers.
- Growth: a new verdict axis is one field on the fold's construction — the table and arithmetic are closed.

```typescript
declare namespace Slo {
  type Reading = { readonly long: number; readonly short: number }
  type Readings = { readonly [K in Burn]: Reading }
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

const _bySeverity: Order.Order<Slo.BurnRow["severity"]> = Order.mapInput(Order.boolean, (severity) => severity === "page")

const _evaluate = (objective: Slo.Objective, readings: Slo.Readings): Slo.Verdict => {
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
    severity: Array.match(fired, {                             // the ceiling is one Order policy value: the dominant fired severity, never a branch ladder
      onEmpty: Option.none,
      onNonEmpty: (verdicts) => Option.some(Array.max(Array.map(verdicts, (verdict) => verdict.row.severity), _bySeverity)),
    }),
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
  readonly evaluate: (objective: Slo.Objective, readings: Slo.Readings) => Slo.Verdict
  readonly rows: typeof _BURN
  readonly spent: (objective: Slo.Objective, errorRate: number, elapsed: Duration.Duration) => number
} = {
  budget: _budget,
  burn: _burnOf,
  evaluate: _evaluate,
  rows: _BURN,
  spent: _spent,
}
```

## [5]-[ALERT_SPECS]

[ALERT_SPECS]:
- Owner: the `_severity` routing table and the assembled `Alert` export — one severity row per posture (`urgency`: page interrupts a human now, ticket enters the queue; `hold`: how long the condition holds before the spec counts as firing — page rows fire immediately because the short window already debounces, ticket rows hold to suppress flappy toil; `tone`: the one severity-to-tone correspondence, riding annotations and threshold steps alike so no dashboard re-declares it) and `Alert.of` as the one derivation: one spec per burn row, total by construction because the burn table is closed, so every objective yields exactly the four-row discipline.
- Law: the severity axis is exactly `[03]`'s row projection — the union derives from the burn rows' `severity` column, so a severity this table carries but no burn row produces is dead vocabulary the guard rejects, and the two clusters cannot drift.
- Law: the spec is compilation-ready data — `slug` (the deterministic `${objective.name}:${burn}` key both consumers use as the provider-side identity, so a re-apply updates in place), the `sli` carried whole (the consumer compiles the case's breach predicate — ceiling, horizon, good/total — into its own query dialect), `target`, the row's `windows`/`factor`, the severity row inline, and the annotation record under `Convention.rasm.sloObjective`/`sloSeverity`/`sloBurn` keys — everything a rule compiler or a panel builder needs, nothing it must look up elsewhere.
- Law: consumers compile, never re-derive — `board#PACKS` folds specs into two-window burn panels and firing annotations, `iac` folds the same specs into provider rule resources; a consumer computing its own burn thresholds from the objective has forked the discipline and is the named defect.
- Law: delivery routing is not spec data — receivers, schedules, and escalation chains are deploy-plane configuration keyed by the spec's severity row; the spec's `urgency` is the routing INPUT, the route itself lives where the notifier lives.
- Receipt: `Alert.Spec` — plain policy data; no effect, no fault channel, no emission.
- Entry: `Alert.of(objective)`; `Alert.severity` for posture lookups.
- Growth: a new spec field is one construction line inherited by both consumers; a new severity is first a burn-row change, then its `_severity` row; a routing posture axis (a business-hours gate, an escalation tier) is one column every spec inherits.

```typescript
const _severity = {
  page: { hold: "0 seconds", tone: "critical", urgency: "interrupt" },
  ticket: { hold: "30 minutes", tone: "warning", urgency: "queue" },
} as const

declare namespace Alert {
  type Severity = Slo.BurnRow["severity"]
  type SeverityRow = { readonly hold: Duration.DurationInput; readonly tone: string; readonly urgency: "interrupt" | "queue" }
  type Spec = {
    readonly annotations: Convention.Attributes
    readonly burn: Slo.Burn
    readonly factor: number
    readonly severity: SeverityRow & { readonly kind: Severity }
    readonly sli: Sli
    readonly slug: string
    readonly target: number
    readonly windows: { readonly long: Duration.DurationInput; readonly short: Duration.DurationInput }
  }
  type _Rows<T extends { readonly [K in Severity]: SeverityRow } = typeof _severity> = T
  type _Keys<K extends Severity = keyof typeof _severity> = K
}

const _of = (objective: Slo.Objective): ReadonlyArray<Alert.Spec> =>
  Array.map(Struct.keys(Slo.rows), (burn): Alert.Spec => {
    const row = Slo.rows[burn]
    return {
      annotations: {
        [Convention.rasm.sloBurn]: burn,
        [Convention.rasm.sloObjective]: objective.name,
        [Convention.rasm.sloSeverity]: row.severity,
      },
      burn,
      factor: row.factor,
      severity: { ..._severity[row.severity], kind: row.severity },
      sli: objective.sli,
      slug: `${objective.name}:${burn}`,
      target: objective.target,
      windows: { long: row.long, short: row.short },
    }
  })

const Alert: {
  readonly of: (objective: Slo.Objective) => ReadonlyArray<Alert.Spec>
  readonly severity: typeof _severity
} = {
  of: _of,
  severity: _severity,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Alert, Sli, Slo }
```
