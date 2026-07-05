# [RUNTIME_LIFE]

Lifecycle and health are one owner because they are one skeleton: register ranked rows at Layer construction, run each row under its own budget on a severed fiber, convert every outcome to evidence, fold the evidence into a graded receipt. `Life` holds the closed phase spine — `booting → running → draining → halted` — in one `SubscriptionRef` every lifecycle question projects from, the ranked drain registry whose fold runs on interrupt before the graph's finalizers release resources, and the probe registry whose kind rows — `started`, `ready`, `live`, each carrying its canonical k8s route — feed a memoized concurrent report fold. The budgeted row executor is spelled exactly once: drain rows and probe rows are two registries over one `_bounded` fold, their verdicts two graders of one `Exit`-of-`Option` evidence shape. Readiness composes the phase — outside `running` the ready report fails by fold, so the drain flip stops traffic instantly — while liveness ignores it so an orderly drain is never mistaken for a hang. The drain total budget is the number `iac` mirrors into `terminationGracePeriod`; a `process.on("SIGTERM")` listener, an exit-hook library, and teardown-as-ordinary-step are unspellable because the runtime row's `runMain` already owns the signal edge. The module is `runtime/src/proc/life.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                     | [PUBLIC] |
| :-----: | :------------- | :---------------------------------------------------------------------------- | :------- |
|  [01]   | `PHASE_SPINE`  | the phase vocabulary, the cell, the parked boot entry                        | `Life`   |
|  [02]   | `RANKED_FOLD`  | the one budgeted row executor both registries share                          | `Life`   |
|  [03]   | `DRAIN_BANDS`  | ranked drain rows, the two-tier budget, the drain receipt                    | `Life`   |
|  [04]   | `PROBE_ROUTES` | the kind/route anchor, the grade lattice, the memoized phase-gated report    | `Life`   |

## [2]-[PHASE_SPINE]

[PHASE_SPINE]:
- Owner: the phase tuple and the cell — the tuple is a semantic order (lifecycle rank is load-bearing; the anchor derives `Phase` and no consumer re-lists states); the cell starts at `booting`, `life.online` stamps `running` (the boot module's last act before serving), and only the drain fold advances further; the published `phase` is the read-only `Subscribable` projection of the interior cell, so a consumer reads and subscribes and a write is unspellable.
- Law: the park is the drain trigger — `life.parked` is `online` then `Effect.never` with the drain fold attached through `Effect.onInterrupt`, so the boot module runs `row.main(Effect.scoped(Effect.provide(life.parked, root)))` and a delivered `SIGINT`/`SIGTERM` (which `runMain` converts to root-fiber interruption) executes the ordered drain BEFORE scope close releases resources — choreography first, finalizers second, exactly once, on every exit class.
- Law: `Layer.launch` remains the boot for graphs with no ordered drain; the moment one drain step exists, `parked` is the entry — two boot shapes selected by whether choreography is registered, never mixed.
- Boundary: phase consumption is one-directional — the ready fold gates on the cell, the serving edge stops intake by it, and `iac` aligns `terminationGracePeriod` to the drain budget row rather than a second constant; browser lifecycle is the ui wave's own boot fact.
- Entry: `Life.register(step)` and `Life.probe(row)` from any Layer build (`accessors: true`); `life.parked` in the boot module; `life.phase` for observers.
- Packages: `effect` (`SubscriptionRef`, `Subscribable`, `Deferred`, `Ref`, `Chunk`), `./config.ts` (`Setting`).

## [3]-[RANKED_FOLD]

[RANKED_FOLD]:
- Owner: `_bounded` — the one budgeted row executor: measure the open instant, run the row's effect under `Effect.exit` with the budget applied as `Effect.timeoutOption` over `Effect.disconnect`, measure the close, and return the `Exit`-of-`Option` evidence beside the elapsed span; both registries fold through it, so the lapse-is-a-verdict law, the crash-is-evidence law, and the severed-deadline law are stated once.
- Law: every deadline rides a severed fiber — the drain fold runs inside the interrupt's masked finalizer, where a bare timeout waits instead of interrupting, so `Effect.disconnect` severs the row's work onto its own fiber and the deadline settles on time while the shielded work finishes in background; a lapse is a verdict, never an abort.
- Law: a row never fails its fold — a crash converts through `Effect.exit`, a lapse folds from `Option.none`, so the surrounding report is total and the serving edge carries zero recovery arms; the graders are the only per-surface difference — the drain grader folds to `drained | lapsed | crashed`, the probe grader to the grade lattice with lapse and crash landing at `fail` with their detail.
- Growth: a third ranked surface (a warm-up band, a maintenance sweep) is one registry plus one grader over the same executor.
- Packages: `effect` (`Clock`, `Duration`, `Effect`, `Exit`, `Option`).

```typescript
import { Array, Chunk, Clock, DateTime, Deferred, Duration, Effect, Exit, Option, Order, Record, Ref, type Subscribable, SubscriptionRef, pipe } from "effect"
import { Setting } from "./config.ts"

const _PHASES = ["booting", "running", "draining", "halted"] as const
const _BANDS = { intake: 0, domain: 10, report: 90 } as const
const _GRADES = { pass: { rank: 0 }, warn: { rank: 1 }, fail: { rank: 2 } } as const
const _KINDS = { started: { route: "/startupz" }, ready: { route: "/readyz" }, live: { route: "/livez" } } as const

declare namespace Life {
  type Phase = (typeof _PHASES)[number]
  type Verdict = "drained" | "lapsed" | "crashed"
  type Grade = keyof typeof _GRADES
  type Kind = keyof typeof _KINDS
  type Step = {
    readonly label: string
    readonly rank: number
    readonly budget: Option.Option<Duration.Duration>
    readonly run: Effect.Effect<void>
  }
  type Probe = { readonly label: string; readonly kind: Kind; readonly run: Effect.Effect<Grade> }
  type Row = { readonly label: string; readonly rank: number; readonly verdict: Verdict; readonly elapsed: Duration.Duration }
  type Graded = {
    readonly label: string
    readonly grade: Grade
    readonly elapsed: Duration.Duration
    readonly detail: Option.Option<string>
  }
  type Receipt = { readonly at: DateTime.Utc; readonly rows: ReadonlyArray<Row>; readonly landed: Phase }
  type Report = { readonly kind: Kind; readonly overall: Grade; readonly rows: ReadonlyArray<Graded>; readonly at: DateTime.Utc }
  type _Kinds<T extends Record<Kind, { readonly route: string }> = typeof _KINDS> = T
  type _Grades<T extends Record<Grade, { readonly rank: number }> = typeof _GRADES> = T
}

const _byRank: Order.Order<Life.Step> = Order.mapInput(Order.number, (step: Life.Step) => step.rank)
const _byGrade: Order.Order<Life.Grade> = Order.mapInput(Order.number, (grade: Life.Grade) => _GRADES[grade].rank)

const _bounded = <A>(
  run: Effect.Effect<A>,
  budget: Option.Option<Duration.Duration>,
): Effect.Effect<{ readonly outcome: Exit.Exit<Option.Option<A>>; readonly elapsed: Duration.Duration }> =>
  Effect.gen(function* () {
    const opened = yield* Clock.currentTimeMillis
    const outcome = yield* Effect.exit(
      Option.match(budget, {
        onNone: () => Effect.map(run, Option.some),
        onSome: (limit) => Effect.timeoutOption(Effect.disconnect(run), limit),
      }),
    )
    const closed = yield* Clock.currentTimeMillis
    return { outcome, elapsed: Duration.millis(closed - opened) }
  })

const _drainGrade: (outcome: Exit.Exit<Option.Option<void>>) => Life.Verdict = Exit.match({
  onFailure: () => "crashed" as const,
  onSuccess: Option.match({ onNone: () => "lapsed" as const, onSome: () => "drained" as const }),
})

const _probeGrade: (outcome: Exit.Exit<Option.Option<Life.Grade>>) => readonly [Life.Grade, Option.Option<string>] =
  Exit.match({
    onFailure: () => ["fail", Option.some("crashed")] as const,
    onSuccess: Option.match({
      onNone: () => ["fail", Option.some("lapsed")] as const,
      onSome: (grade: Life.Grade) => [grade, Option.none<string>()] as const,
    }),
  })
```

## [4]-[DRAIN_BANDS]

[DRAIN_BANDS]:
- Owner: the drain fold — a step is a row (`label`, `rank`, `budget: Option<Duration>`, `run`) appended to the registry `Chunk` at registration; the fold flips the cell to `draining` first so load balancers stop routing, sorts once by the rank `Order`, splits at the report band, runs sequentially (rank order licenses sequence — a later step depends on the earlier one having stopped its traffic), grades each row through the shared executor, and stamps `halted` under `Effect.ensuring` so the terminal phase is unconditional even when the total budget expires mid-fold.
- Law: budgets are two-tier by construction — the per-row `Option<Duration>` bounds its row and `Setting.life.drain` bounds the whole fold, both over the severed-fiber executor; a step that ignores its budget cannot stall the process, and the total is the number `iac` mirrors into the pod's grace period.
- Law: the receipt is the drain's value — one `Life.Receipt` (open instant, per-row verdict with elapsed, landed phase) settled into the `Deferred` after the drain bands complete and BEFORE the report band runs; a terminal reporter — the telemetry flush — registers at report rank and reads the settled receipt, so shutdown forensics ride the fold without the reporter awaiting a receipt its own completion gates.
- Law: growth is a row — a new graceful concern (stop intake, pause queues, flush spans, checkpoint state) is one `register` call at its owner's Layer build with a rank inside the `_BANDS` anchor (0–9 intake, 10–89 domain, 90+ reporters); no new surface, hook API, or event bus.
- Receipt: `Life.Receipt` via `life.settled`.

## [5]-[PROBE_ROUTES]

[PROBE_ROUTES]:
- Owner: the probe vocabulary and the report fold — `_KINDS` closes the taxonomy with its serving routes (`/startupz`, `/readyz`, `/livez` — the k8s trio), `_GRADES` closes the verdict lattice with rank columns so worst-of merge is an `Order` projection; `Life.report(kind)` filters the registry to the kind, sweeps every probe with unbounded concurrency (probes are independent — accumulation, never abort), bounds each by `Setting.life.probe` through the shared executor, and merges grades worst-of; zero probes fold to `pass`, vacuously healthy.
- Law: kind semantics are the row's contract — `started` answers once per boot (slow warm-up allowed), `ready` answers "route traffic to me now" (dependency reachability, queue depth), `live` answers "is this process worth keeping" (event-loop responsiveness, deadlock sentinels); a dependency check inside a liveness probe is the classic self-inflicted restart loop and is the named defect.
- Law: the ready fold gates on the phase first — outside `running` the report is `fail` with the phase as detail before any probe runs; liveness never reads the phase.
- Law: the sweep is memoized per kind and the memo record derives from the anchor — `Record.map` over `_KINDS` under `Effect.all` mints one `Effect.cachedWithTTL(swept(kind), Setting.life.report)` per row, so a probe storm collapses into one execution per window and a new kind is one anchor row with zero memo edits.
- Law: routes are data — `Life.route(kind)` projects the row; the serving edge mounts the three routes from this anchor and encodes the report (`pass/warn → 200`, `fail → 503`), `iac` writes the same three paths into workload manifests, so the path never exists twice.
- Receipt: `Life.Report` — kind, overall grade, per-row grade with elapsed and detail, instant; telemetry consumes the same rows and no second health shape exists.

```typescript
class Life extends Effect.Service<Life>()("runtime/Life", {
  scoped: Effect.gen(function* () {
    const setting = yield* Setting
    const cell = yield* SubscriptionRef.make<Life.Phase>("booting")
    const phase: Subscribable.Subscribable<Life.Phase> = cell
    const steps = yield* Ref.make(Chunk.empty<Life.Step>())
    const probes = yield* Ref.make(Chunk.empty<Life.Probe>())
    const settled = yield* Deferred.make<Life.Receipt>()

    const ran = (step: Life.Step): Effect.Effect<Life.Row> =>
      Effect.map(_bounded(step.run, step.budget), ({ elapsed, outcome }) => ({
        label: step.label,
        rank: step.rank,
        verdict: _drainGrade(outcome),
        elapsed,
      }))

    const drained = Effect.gen(function* () {
      yield* SubscriptionRef.set(cell, "draining")
      const queue = Array.sort(Chunk.toReadonlyArray(yield* Ref.get(steps)), _byRank)
      const [drain, report] = Array.splitWhere(queue, (step) => step.rank >= _BANDS.report)
      const at = yield* DateTime.now
      const rows = yield* Effect.forEach(drain, ran)
      yield* Deferred.succeed(settled, { at, rows, landed: "halted" })
      yield* Effect.forEach(report, ran, { discard: true })
    }).pipe(
      Effect.disconnect,
      Effect.timeoutOption(setting.life.drain),
      Effect.asVoid,
      Effect.ensuring(SubscriptionRef.set(cell, "halted")),
    )

    const proven = (probe: Life.Probe): Effect.Effect<Life.Graded> =>
      Effect.map(_bounded(probe.run, Option.some(setting.life.probe)), ({ elapsed, outcome }) =>
        pipe(_probeGrade(outcome), ([grade, detail]) => ({ label: probe.label, grade, elapsed, detail })))

    const swept = (kind: Life.Kind): Effect.Effect<Life.Report> =>
      Effect.gen(function* () {
        const at = yield* DateTime.now
        const held = yield* cell.get
        const gated = kind === "ready" && held !== "running"
        const registered = Array.filter(Chunk.toReadonlyArray(yield* Ref.get(probes)), (probe) => probe.kind === kind)
        const rows = gated
          ? [{ label: "life", grade: "fail" as const, elapsed: Duration.zero, detail: Option.some(held) }]
          : yield* Effect.forEach(registered, proven, { concurrency: "unbounded" })
        const overall = pipe(
          Array.map(rows, (row) => row.grade),
          (graded) => (Array.isNonEmptyReadonlyArray(graded) ? Array.max(graded, _byGrade) : "pass"),
        )
        return { kind, overall, rows, at }
      })

    const memo = yield* Effect.all(Record.map(_KINDS, (_, kind) => Effect.cachedWithTTL(swept(kind), setting.life.report)))

    return {
      phase,
      online: SubscriptionRef.set(cell, "running"),
      register: (step: Life.Step): Effect.Effect<void> => Ref.update(steps, Chunk.append(step)),
      probe: (row: Life.Probe): Effect.Effect<void> => Ref.update(probes, Chunk.append(row)),
      report: (kind: Life.Kind): Effect.Effect<Life.Report> => memo[kind],
      route: (kind: Life.Kind): string => _KINDS[kind].route,
      parked: Effect.zipRight(SubscriptionRef.set(cell, "running"), Effect.never).pipe(
        Effect.onInterrupt(() => drained),
      ),
      settled: Deferred.await(settled),
    }
  }),
  dependencies: [Setting.Default],
  accessors: true,
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Life }
```
