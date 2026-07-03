# [STATE_ALGEBRA]

`fold/algebra.ts` declares the keyed fold every consumer binds: a `Fold.Plan<Op, K, S>` names the key projection, the state lift, and the lawful `Merge.Instance` that combines contributions — one value that is simultaneously the pure snapshot fold, the Mealy step a stream lifts unchanged, and the graph specification both incremental engines execute. ONE algebra, two altitudes: browser apps fold wire-decoded events in memory through `fold/replay`'s d2mini binding; node apps fold journal events durably through `store/project` binding the SAME plan — a second fold implementation per runtime is the named defect this owner exists to make unspellable. The algebra is generic over the op vocabulary: evidence folds, presence folds, CRDT projections, and journal projections are plan rows, never sibling fold machines.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                          | [SURFACE]                            |
| :-----: | :------------- | :--------------------------------------------------------------- | :------------------------------------ |
|  [01]   | [PLAN_CONTRACT]| the plan shape, the delta currency, the change row               | `Fold.Plan`, `Fold.Delta`, `Fold.Change`, `Fold.Table` |
|  [02]   | [FOLD_ENTRY]   | the modality-polymorphic fold, the Mealy step, the running trace | `Fold.run`, `Fold.step`, `Fold.trace` |

## [2]-[PLAN_CONTRACT]

- Owner: `Fold.Plan<Op, K, S>` — `name` (the plan identity registries and telemetry dimensions key on), `key` (op to fold key), `lift` (op to state contribution), `merge` (the `crdt/merge` instance combining contributions) — the single declaration every altitude executes; a fold is a value, and capability growth is a plan row.
- Packages: `effect` (`HashMap`, `Array`, `Option`, `Stream`, `Effect`); `../crdt/merge.ts`.
- Law: the fold is `combineMap` shaped — an op projects into a contribution and contributions merge under the lawful instance — so insert (`none -> lift`) and update (`some -> combine`) are two arms of one `HashMap.modifyAt` fold and no `get`-then-`set` pair exists.
- Law: `Fold.Delta<A>` — signed `[value, multiplicity]` rows — is the folder-wide delta currency: `fold/replay` pushes it into the engines, `query/window` returns it from `asOfDiff`, and negative rows are retraction vocabulary only the engine lane may honor; the pure altitude folds insertions and treats nonpositive rows as absent, because un-merging demands the versioned trace only `fold/replay` retains.
- Law: `Fold.Change<K, S>` reifies a fold advance — the touched key and its post-state, `Option.none` when the key retracted — the one row shape live views, timeline feeds, and store projections consume, so every altitude emits identical change vocabulary.
- Growth: a new fold is one `Fold.plan` row binding an existing or new merge instance; a new consumer altitude binds the plan, never re-declares the fold.

```typescript
import { Array, Effect, HashMap, Option, Stream } from "effect"
import { Merge } from "../crdt/merge.ts"

declare namespace Fold {
  type Plan<Op, K, S> = {
    readonly name: string
    readonly key: (op: Op) => K
    readonly lift: (op: Op) => S
    readonly merge: Merge.Instance<S>
  }
  type Delta<A> = ReadonlyArray<readonly [A, number]>
  type Change<K, S> = { readonly key: K; readonly state: Option.Option<S> }
  type Table<K, S> = HashMap.HashMap<K, S>
  type Step<Op, K, S> = (table: Table<K, S>, op: Op) => readonly [Table<K, S>, Change<K, S>]
  type Shape = {
    readonly plan: <Op, K, S>(spec: Plan<Op, K, S>) => Plan<Op, K, S>
    readonly step: <Op, K, S>(plan: Plan<Op, K, S>) => Step<Op, K, S>
    readonly trace: <Op, K, S>(
      plan: Plan<Op, K, S>,
    ) => <E, R>(ops: Stream.Stream<Op, E, R>) => Stream.Stream<Change<K, S>, E, R>
    readonly run: {
      <Op, K, S>(plan: Plan<Op, K, S>, ops: ReadonlyArray<Op>): Table<K, S>
      <Op, K, S, E, R>(plan: Plan<Op, K, S>, ops: Stream.Stream<Op, E, R>): Effect.Effect<Table<K, S>, E, R>
    }
  }
}

const _absorb = <Op, K, S>(plan: Fold.Plan<Op, K, S>) => (table: Fold.Table<K, S>, op: Op): Fold.Table<K, S> =>
  HashMap.modifyAt(table, plan.key(op), (slot) =>
    Option.some(Option.match(slot, {
      onNone: () => plan.lift(op),
      onSome: (held) => plan.merge.combine.combine(held, plan.lift(op)),
    })))
```

## [3]-[FOLD_ENTRY]

- Owner: `Fold.run` — one entrypoint over both bounded modalities, discriminated on the input value: an admitted `ReadonlyArray` folds pure to the table; a `Stream` folds on the rail with the same `_absorb` — seed and step are shared declarations, so the modalities cannot drift. Unbounded live maintenance is not a `run` modality: it is `fold/replay`'s handle, bound to the same plan.
- Law: `Fold.step` is the Mealy shape — `(table, op) => [table, change]` — written once and lifted unchanged: `Fold.trace` is its `Stream.mapAccum` lift emitting one `Change` per op, and `store/project` threads the identical step through its durable lane.
- Law: the emitted change reads the post-write table exactly once — the change is derived from the same `HashMap.modifyAt` result the table advance produced, so the trace and the table cannot disagree.
- Boundary: engine execution (d2mini graphs, d2ts versioned graphs, ordered lanes) is `fold/replay.md`'s; the durable altitude is `store/project` binding these plans; live view projection over the folded table is `query/live.md`'s.

```typescript
function run<Op, K, S>(plan: Fold.Plan<Op, K, S>, ops: ReadonlyArray<Op>): Fold.Table<K, S>
function run<Op, K, S, E, R>(
  plan: Fold.Plan<Op, K, S>,
  ops: Stream.Stream<Op, E, R>,
): Effect.Effect<Fold.Table<K, S>, E, R>
function run<Op, K, S, E, R>(
  plan: Fold.Plan<Op, K, S>,
  ops: ReadonlyArray<Op> | Stream.Stream<Op, E, R>,
): Fold.Table<K, S> | Effect.Effect<Fold.Table<K, S>, E, R> {
  return Array.isArray(ops)
    ? Array.reduce(ops, HashMap.empty<K, S>(), _absorb(plan))
    : Stream.runFold(ops, HashMap.empty<K, S>(), _absorb(plan))
}

const Fold: Fold.Shape = {
  plan: (spec) => spec,
  step: (plan) => (table, op) => {
    const key = plan.key(op)
    const next = _absorb(plan)(table, op)
    return [next, { key, state: HashMap.get(next, key) }] as const
  },
  trace: (plan) => (ops) =>
    Stream.map(Stream.mapAccum(ops, HashMap.empty(), Fold.step(plan)), (change) => change),
  run,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Fold }
```
