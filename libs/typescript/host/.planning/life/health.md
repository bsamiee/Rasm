# [HOST_HEALTH]

Health is a probe vocabulary and a total report fold, never an endpoint. `Health` closes the kind rows — `started`, `ready`, `live`, each carrying its canonical route so `edge` serves and `iac` targets one anchor — and holds the probe registry: services register ranked probe rows at their own Layer construction, and one memoized fold sweeps a kind's probes concurrently under the per-probe budget, folding every outcome into a graded report. A probe never fails the fold: a crash, a lapse, and a refused dependency are grade evidence (`fail` with detail), so the report is total and the serving edge maps grades to status codes with zero recovery arms. Readiness composes the lifecycle: a process outside `running` is unready by fold — the drain flip stops traffic instantly — while liveness ignores the phase so an orderly drain is never mistaken for a hang.

## [1]-[INDEX]

- [01]-[PROBE_ROWS]: the kind/route anchor, the grade lattice, the probe registry.
- [02]-[REPORT_FOLD]: the concurrent sweep, the phase gate, the memo window, the report receipt.

## [2]-[PROBE_ROWS]

- Owner: the vocabulary anchors. `_kinds` closes the probe taxonomy with its serving routes (`/startupz`, `/readyz`, `/livez` — the k8s probe trio); `_grades` closes the verdict lattice (`pass < warn < fail`) with rank columns so the worst-of merge is an `Order` projection, never an if-ladder. A probe row is `label`, `kind`, and `run: Effect<Health.Grade>` — authored total where possible; whatever it does anyway becomes evidence at the fold.
- Law: kind semantics are the row's contract — `started` answers once per boot (slow warm-up allowed, the startup gate), `ready` answers "route traffic to me now" (dependency reachability, queue depth), `live` answers "is this process worth keeping" (event-loop responsiveness, deadlock sentinels); a dependency check inside a liveness probe is the classic self-inflicted restart loop and is the named defect.
- Law: routes are data — `Health.route(kind)` projects the row; `edge` mounts the three routes from this anchor and `iac` writes the same three paths into workload manifests, so the path never exists twice.
- Entry: `Health.register(probe)` from any Layer build (`accessors: true`).
- Packages: `effect` (`Order`, `Chunk`, `Ref`).

## [3]-[REPORT_FOLD]

- Owner: `Health.report(kind)` — the one read. The fold filters the registry to the kind, sweeps every probe with unbounded concurrency (probes are independent — accumulation, never abort), bounds each by `Setting.life.probe` (`Effect.timeoutOption` — a lapse grades `fail` with `"lapsed"` detail), converts a crash to a `fail` grade through `Effect.exit`, and merges grades worst-of through the rank `Order` — zero probes fold to `pass`, vacuously healthy. The `ready` fold gates on the `Cycle` phase first: outside `running` the report is `fail` with the phase as detail before any probe runs.
- Law: the sweep is memoized per kind — `Effect.cachedWithTTL` under `Setting.life.report` collapses a probe storm into one execution per window, so an aggressive load balancer cannot amplify an expensive dependency check; the memo window is policy, not a cache surface.
- Law: the report is a receipt — kind, overall grade, per-row grade with elapsed and detail, instant; the serving edge encodes it (`pass/warn → 200`, `fail → 503`) and telemetry consumes the same rows; no second health shape exists.
- Boundary: serving is `edge`'s (the routes anchor here, the handler there); restart policy is the orchestrator's; this fold only answers.
- Receipt: `Health.Report`.
- Packages: `effect` (`Array`, `Chunk`, `Clock`, `DateTime`, `Duration`, `Effect`, `Exit`, `Option`, `Order`, `Ref`, `SubscriptionRef`), `../config/schema.ts` (`Setting`), `./cycle.ts` (`Cycle`).

```typescript
import { Array, Chunk, Clock, DateTime, Duration, Effect, Exit, Option, Order, Ref, SubscriptionRef, pipe } from "effect"
import { Setting } from "../config/schema.ts"
import { Cycle } from "./cycle.ts"

const _kinds = {
  started: { route: "/startupz" },
  ready: { route: "/readyz" },
  live: { route: "/livez" },
} as const

const _grades = {
  pass: { rank: 0 },
  warn: { rank: 1 },
  fail: { rank: 2 },
} as const

declare namespace Health {
  type Kind = keyof typeof _kinds
  type Grade = keyof typeof _grades
  type Probe = { readonly label: string; readonly kind: Kind; readonly run: Effect.Effect<Grade> }
  type Row = {
    readonly label: string
    readonly grade: Grade
    readonly elapsed: Duration.Duration
    readonly detail: Option.Option<string>
  }
  type Report = { readonly kind: Kind; readonly overall: Grade; readonly rows: ReadonlyArray<Row>; readonly at: DateTime.Utc }
  type _Kinds<T extends Record<Kind, { readonly route: string }> = typeof _kinds> = T
  type _Grades<T extends Record<Grade, { readonly rank: number }> = typeof _grades> = T
}

const _byRank: Order.Order<Health.Grade> = Order.mapInput(Order.number, (grade: Health.Grade) => _grades[grade].rank)

const _proven = (probe: Health.Probe, budget: Duration.Duration): Effect.Effect<Health.Row> =>
  Effect.gen(function* () {
    const opened = yield* Clock.currentTimeMillis
    const outcome = yield* Effect.exit(Effect.timeoutOption(probe.run, budget))
    const closed = yield* Clock.currentTimeMillis
    const [grade, detail] = Exit.match(outcome, {
      onFailure: () => ["fail", Option.some("crashed")] as const,
      onSuccess: Option.match({
        onNone: () => ["fail", Option.some("lapsed")] as const,
        onSome: (graded: Health.Grade) => [graded, Option.none<string>()] as const,
      }),
    })
    return { label: probe.label, grade, elapsed: Duration.millis(closed - opened), detail }
  })

const _merged = (rows: ReadonlyArray<Health.Row>): Health.Grade =>
  pipe(
    Array.map(rows, (row) => row.grade),
    (graded) => (Array.isNonEmptyReadonlyArray(graded) ? Array.max(graded, _byRank) : "pass"),
  )

class Health extends Effect.Service<Health>()("host/Health", {
  scoped: Effect.gen(function* () {
    const setting = yield* Setting
    const cycle = yield* Cycle
    const probes = yield* Ref.make(Chunk.empty<Health.Probe>())

    const swept = (kind: Health.Kind): Effect.Effect<Health.Report> =>
      Effect.gen(function* () {
        const at = yield* DateTime.now
        const phase = yield* SubscriptionRef.get(cycle.phase)
        const gated = kind === "ready" && phase !== "running"
        const registered = Array.filter(Chunk.toReadonlyArray(yield* Ref.get(probes)), (probe) => probe.kind === kind)
        const rows = gated
          ? [{ label: "cycle", grade: "fail" as const, elapsed: Duration.millis(0), detail: Option.some(phase) }]
          : yield* Effect.forEach(registered, (probe) => _proven(probe, setting.life.probe), { concurrency: "unbounded" })
        return { kind, overall: _merged(rows), rows, at }
      })

    const memo = {
      started: yield* Effect.cachedWithTTL(swept("started"), setting.life.report),
      ready: yield* Effect.cachedWithTTL(swept("ready"), setting.life.report),
      live: yield* Effect.cachedWithTTL(swept("live"), setting.life.report),
    }

    return {
      register: (probe: Health.Probe): Effect.Effect<void> => Ref.update(probes, Chunk.append(probe)),
      report: (kind: Health.Kind): Effect.Effect<Health.Report> => memo[kind],
      route: (kind: Health.Kind): string => _kinds[kind].route,
    }
  }),
  dependencies: [Setting.Default, Cycle.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Health }
```
