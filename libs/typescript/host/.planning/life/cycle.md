# [HOST_CYCLE]

Process lifecycle is one phase cell and one ordered drain fold. `Cycle` holds the closed phase vocabulary — `booting → running → draining → halted` — in one `SubscriptionRef` every lifecycle question projects from (`health.md` gates readiness on it, `edge` stops intake by it), and owns the shutdown choreography: consumers register drain steps as ranked rows at their own Layer construction, the boot module parks the process on `cycle.parked`, and the interrupt a signal delivers runs the fold — flip to `draining` first so load balancers stop routing, then every step in rank order under its own budget, the whole fold under the configured total budget, every outcome a receipt row — before the graph's finalizers release resources. Graceful concerns are steps; hard release stays in Layer finalizers; a `process.on("SIGTERM")` listener, an exit-hook library, and teardown-as-ordinary-step are unspellable here because the runtime row's `runMain` already owns the signal edge.

## [1]-[INDEX]

- [01]-[PHASE_SPINE]: the phase vocabulary, the cell, and the parked boot entry.
- [02]-[DRAIN_FOLD]: ranked step rows, per-step and total budgets, the drain receipt.

## [2]-[PHASE_SPINE]

- Owner: `Cycle` — the lifecycle service. The phase tuple is a semantic order (lifecycle rank is load-bearing; the tuple anchor derives the `Phase` type and no consumer re-lists states); the cell starts at `booting`, `cycle.online` stamps `running` (the boot module's last act before serving), and only the drain fold advances further — the published `phase` is the read-only `Subscribable` projection of the interior cell, so a consumer can read and subscribe and a write is unspellable, never merely discouraged.
- Law: the park is the drain trigger — `cycle.parked` is `online` then `Effect.never` with the drain fold attached through `Effect.onInterrupt`, so the boot module runs `row.main(Effect.scoped(Effect.provide(cycle.parked, root)))` and a delivered `SIGINT`/`SIGTERM` (which `runMain` converts to root-fiber interruption) executes the ordered drain BEFORE scope close releases resources — choreography first, finalizers second, exactly once, on every exit class.
- Law: `Layer.launch` remains the boot for graphs with no ordered drain; the moment one drain step exists, `parked` is the entry — two boot shapes, selected by whether choreography is registered, never mixed.
- Boundary: phase consumption is one-directional — `health.md` folds readiness from the cell, `browser` lifecycle is its own page (`browser/boot`), and `iac` aligns its `terminationGracePeriod` to the drain budget row rather than a second constant.
- Entry: `Cycle.register(step)` from any Layer build (`accessors: true`); `cycle.parked` in the boot module; `cycle.phase` for observers.
- Packages: `effect` (`SubscriptionRef`, `Subscribable`, `Deferred`, `Ref`, `Chunk`, `Effect`), `../config/schema.ts` (`Setting`).

## [3]-[DRAIN_FOLD]

- Owner: the fold itself. A step is a row — `label`, `rank`, `budget: Option<Duration>`, `run` — appended to the registry `Chunk` at registration; the fold sorts once by the rank `Order`, splits at the report band, runs sequentially (rank order licenses sequence — a later step depends on the earlier one having stopped its traffic), converts a crash to evidence through `Effect.exit`, and stamps `halted` under `Effect.ensuring` so the terminal phase is unconditional even when the total budget expires mid-fold.
- Law: budgets are two-tier by construction and every deadline rides a severed fiber — the fold runs inside the interrupt's masked finalizer, where a bare timeout waits instead of interrupting, so the per-step `Option<Duration>` bounds its row over `Effect.disconnect` under `Effect.timeoutOption` (a lapse is a verdict, never an abort) and the `Setting.life.drain` total bounds the whole fold the same way; a step that ignores its budget cannot stall the process, and the total is the number `iac` mirrors into the pod's grace period.
- Law: the receipt is the drain's value — one `Cycle.Receipt` (open instant, per-row verdict `drained | lapsed | crashed` with elapsed, landed phase) settled into the `Deferred` after the drain bands complete and BEFORE the report band runs; a terminal reporter — the telemetry flush — registers at report rank and reads the settled receipt, so shutdown forensics ride the fold without the reporter awaiting a receipt its own completion gates, and reporter outcomes flush signals rather than joining the rows they read.
- Law: growth is a row — a new graceful concern (stop intake, pause queues, flush spans, checkpoint state) is one `register` call at its owner's Layer build with a rank inside the `_BANDS` anchor (0–9 intake, 10–89 domain, 90+ reporters); no new surface, hook API, or event bus.
- Receipt: `Cycle.Receipt` via `cycle.settled`.
- Packages: `effect` (`Array`, `Chunk`, `Clock`, `DateTime`, `Duration`, `Effect`, `Exit`, `Function`, `Option`, `Order`, `Subscribable`).

```typescript
import { Array, Chunk, Clock, DateTime, Deferred, Duration, Effect, Exit, Function, Option, Order, Ref, type Subscribable, SubscriptionRef } from "effect"
import { Setting } from "../config/schema.ts"

const _PHASES = ["booting", "running", "draining", "halted"] as const
const _BANDS = { intake: 0, domain: 10, report: 90 } as const

declare namespace Cycle {
  type Phase = (typeof _PHASES)[number]
  type Verdict = "drained" | "lapsed" | "crashed"
  type Step = {
    readonly label: string
    readonly rank: number
    readonly budget: Option.Option<Duration.Duration>
    readonly run: Effect.Effect<void>
  }
  type Row = { readonly label: string; readonly rank: number; readonly verdict: Verdict; readonly elapsed: Duration.Duration }
  type Receipt = { readonly at: DateTime.Utc; readonly rows: ReadonlyArray<Row>; readonly landed: Phase }
}

const _byRank: Order.Order<Cycle.Step> = Order.mapInput(Order.number, (step: Cycle.Step) => step.rank)

const _ran = (step: Cycle.Step): Effect.Effect<Cycle.Row> =>
  Effect.gen(function* () {
    const opened = yield* Clock.currentTimeMillis
    const outcome = yield* Effect.exit(
      Option.match(step.budget, {
        onNone: () => Effect.as(step.run, "drained" as const),
        onSome: (budget) =>
          Effect.map(
            Effect.timeoutOption(Effect.disconnect(step.run), budget),
            Option.match({ onNone: () => "lapsed" as const, onSome: () => "drained" as const }),
          ),
      }),
    )
    const closed = yield* Clock.currentTimeMillis
    const verdict = Exit.match(outcome, { onFailure: () => "crashed" as const, onSuccess: Function.identity })
    return { label: step.label, rank: step.rank, verdict, elapsed: Duration.millis(closed - opened) }
  })

class Cycle extends Effect.Service<Cycle>()("host/Cycle", {
  scoped: Effect.gen(function* () {
    const setting = yield* Setting
    const cell = yield* SubscriptionRef.make<Cycle.Phase>("booting")
    const phase: Subscribable.Subscribable<Cycle.Phase> = cell
    const steps = yield* Ref.make(Chunk.empty<Cycle.Step>())
    const settled = yield* Deferred.make<Cycle.Receipt>()

    const drained = Effect.gen(function* () {
      yield* SubscriptionRef.set(cell, "draining")
      const queue = Array.sort(Chunk.toReadonlyArray(yield* Ref.get(steps)), _byRank)
      const [drain, report] = Array.splitWhere(queue, (step) => step.rank >= _BANDS.report)
      const at = yield* DateTime.now
      const rows = yield* Effect.forEach(drain, _ran)
      yield* Deferred.succeed(settled, { at, rows, landed: "halted" })
      yield* Effect.forEach(report, _ran, { discard: true })
    }).pipe(
      Effect.disconnect,
      Effect.timeoutOption(setting.life.drain),
      Effect.asVoid,
      Effect.ensuring(SubscriptionRef.set(cell, "halted")),
    )

    return {
      phase,
      online: SubscriptionRef.set(cell, "running"),
      register: (step: Cycle.Step): Effect.Effect<void> => Ref.update(steps, Chunk.append(step)),
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

export { Cycle }
```
