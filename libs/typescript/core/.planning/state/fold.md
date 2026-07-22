# [CORE_FOLD]

The one keyed-fold and time-coordinate owner of the branch. `Fold.Plan<Op, K, S>` names the key projection, the state lift, and the lawful `merge` instance — one value that is simultaneously the pure snapshot fold, the Mealy step a stream lifts unchanged, and the graph specification the incremental engines execute. `AsOf` is the ONE read time — the two `Hlc` halves plus the journal ordinal — every push, seal, time-travel read, watermark, and compaction coordinate speaks; no second engine-time notion exists. `Replay` maintains every fold incrementally through the differential engines under REPLAY_LAW — a fold rebuilt from any event prefix and advanced by the remaining deltas is equivalent, under the plan instance's `alike`, to the live fold of the whole history; the algebraic half is `merge#LAW_SURFACE`'s `Converge.commutes`, this module supplies the operational half — and the versioned lane retains history in the engine's own `Index` trace: `reconstructAt` IS the AsOf materialization, `compact` IS the retention handoff, so no owned slice log shadows the engine. The dataflow verbs — the keyed join in every engine kind, the `filterBy` semijoin, grouped aggregation over the engine's whole seven-combinator roster, the bounded per-group board, `iterate` fixpoint closure — are handle rows on the same algebra, and `Window` closes event time: the watermark meet, the honest lateness verdict, and pane folds that compose any plan under a composite pane key. The module is `core/src/state/fold.ts`; a new fold is a plan row, a new dataflow verb is a handle row, a new window shape is a policy case.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                  | [PUBLIC]                             |
| :-----: | :---------------- | :---------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `PLAN_CONTRACT`   | the plan shape, delta currency, change row, and the pure/stream folds   | `Fold`                               |
|  [02]   | `TIME_COORDINATE` | the one three-coordinate read time and its engine projection            | `AsOf`                               |
|  [03]   | `MEMORY_LANE`     | the in-memory and ordered handles plus the lens reads over every handle | `Replay.memory/.ordered/.view/.feed` |
|  [04]   | `DATAFLOW_VERBS`  | joins, semijoins, rollups, bounded boards, and fixpoints                | `Replay`                             |
|  [05]   | `VERSIONED_LANE`  | the Index-traced versioned handle: seal, AsOf read, diff, compaction    | `Replay.versioned`                   |
|  [06]   | `WATERMARK_PANES` | event-time completeness, the lateness verdict, and window policy folds  | `Window`                             |

## [02]-[PLAN_CONTRACT]

[PLAN_CONTRACT]:
- Owner: `Fold.Plan<Op, K, S>` — `name` (the plan identity registries and telemetry dimensions key on), `key`, `lift`, `merge` — the single declaration every altitude executes; a fold is a value and capability growth is a plan row. Evidence folds, presence folds, CRDT projections, and journal projections are plan rows, never sibling fold machines.
- Law: the fold is `combineMap`-shaped — an op projects into a contribution and contributions merge under the lawful instance — so insert (`none -> lift`) and update (`some -> combine`) are two arms of one `HashMap.modifyAt` fold and no `get`-then-`set` pair exists.
- Law: `Fold.Delta<A>` — signed `[value, multiplicity]` rows — is the module-wide delta currency: handles push it, `diff` returns it, and negative rows are retraction vocabulary only the engine lanes honor, because un-merging demands the versioned trace only the engines retain; the pure altitude consumes bare ops and never a delta.
- Law: `Fold.Change<K, S>` reifies a fold advance — the touched key and its post-state, `Option.none` when the key retracted — the one row shape live views, the feed, and durable projections consume, so every altitude emits identical change vocabulary; the emitted change reads the post-write table exactly once, so the trace and the table cannot disagree.
- Law: `Fold.run` is one entrypoint over both bounded modalities, discriminated on the input value — an admitted `ReadonlyArray` folds pure to the table, a `Stream` folds on the rail with the same `_absorb` — and `(ops) => Fold.run(plan, ops)` is the run argument `merge#LAW_SURFACE`'s `Converge.commutes` consumes, so the replay proof exercises exactly this fold. Unbounded live maintenance is never a `run` modality: it is a `Replay` handle bound to the same plan.
- Growth: a new fold is one `Fold.plan` row binding an existing or new merge instance; a new consumer altitude binds the plan, never re-declares the fold.
- Boundary: the durable altitude is the data branch binding these plans over the engine's persistent trace; serving handles over sockets and binding them into view atoms are runtime- and ui-branch concerns.
- Packages: `@electric-sql/d2mini`, `@electric-sql/d2ts`; `effect` (`Array`, `Chunk`, `Data`, `Duration`, `Effect`, `Either`, `Equal`, `HashMap`, `HashSet`, `Match`, `Option`, `Order`, `pipe`, `Predicate`, `Record`, `Ref`, `Schema`, `SortedMap`, `Stream`, `Subscribable`, `SubscriptionRef`); `../value/clock.ts` (`Hlc`, `Uncertainty`); `./causal.ts` (`Causal`, `Vector`); `./merge.ts` (`Merge`).

```typescript
import * as Mini from "@electric-sql/d2mini"
import * as Diff from "@electric-sql/d2ts"
import {
  Array, Chunk, Data, Duration, Effect, Either, Equal, HashMap, HashSet, Match, Option, Order, pipe, Predicate, Record,
  Ref, Schema, SortedMap, Stream, Subscribable, SubscriptionRef,
} from "effect"
import { Hlc, type Uncertainty } from "../value/clock.ts"
import { Causal, type Vector } from "./causal.ts"
import { Merge } from "./merge.ts"

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

const _step = <Op, K, S>(plan: Fold.Plan<Op, K, S>): Fold.Step<Op, K, S> => (table, op) =>
  pipe(_absorb(plan)(table, op), (next) =>
    [next, { key: plan.key(op), state: HashMap.get(next, plan.key(op)) }] as const)

const _trace = <Op, K, S>(plan: Fold.Plan<Op, K, S>) =>
<E, R>(ops: Stream.Stream<Op, E, R>): Stream.Stream<Fold.Change<K, S>, E, R> =>
  Stream.mapAccum(ops, HashMap.empty<K, S>(), _step(plan))

const _isBatch = <Op, E, R>(ops: ReadonlyArray<Op> | Stream.Stream<Op, E, R>): ops is ReadonlyArray<Op> =>
  globalThis.Array.isArray(ops)

function run<Op, K, S>(plan: Fold.Plan<Op, K, S>, ops: ReadonlyArray<Op>): Fold.Table<K, S>
function run<Op, K, S, E, R>(
  plan: Fold.Plan<Op, K, S>,
  ops: Stream.Stream<Op, E, R>,
): Effect.Effect<Fold.Table<K, S>, E, R>
function run<Op, K, S, E, R>(
  plan: Fold.Plan<Op, K, S>,
  ops: ReadonlyArray<Op> | Stream.Stream<Op, E, R>,
): Fold.Table<K, S> | Effect.Effect<Fold.Table<K, S>, E, R> {
  return _isBatch(ops)
    ? Array.reduce(ops, HashMap.empty<K, S>(), _absorb(plan))
    : Stream.runFold(ops, HashMap.empty<K, S>(), _absorb(plan))
}

const Fold: Fold.Shape = { plan: (spec) => spec, step: _step, trace: _trace, run }
```

## [03]-[TIME_COORDINATE]

[TIME_COORDINATE]:
- Owner: `AsOf` — the `Schema.Class` owner of `{ stamp: Hlc, ordinal }` where the ordinal is a non-negative bigint journal sequence branded in-field and the class carries the engine-window filter as its own invariant: event time plus journal position, totally ordered lexicographically on one lane (`AsOf.Order` composes the `Hlc` order then `Order.bigint` on the ordinal), the one decode anchor for any serialized coordinate — a resume token, a scrub bookmark — and the one value every handle's push, seal, read, diff, and compaction takes. `AsOf.at(stamp, sequence)` is the one mint from a durable position — the data branch's lanes construct through it and a bare position tuple never travels. The old engine-time/read-time split is dead: `AsOf.time` projects into the engine's number plane interior to this module, and no consumer meets a bare coordinate triple.
- Law: the engine projection is total BY ADMISSION — the class filter bounds all three coordinates inside the safe-integer window (`<= 2^53 - 1`), so the `Number` narrowing at this one seam is proven, distinct admitted coordinates keep distinct engine versions, and `AsOf.Order` agrees with the engine tuple on every admitted value; an out-of-window coordinate — an adversarial wire stamp with a u64-scale logical half — fails decode as a typed `ParseError` and fails the trusted constructor at the same filter, so an aliased engine version is unspellable rather than guarded. The narrowed triple drives engines only, never re-enters `Hlc` or the identity-bearing coordinate.
- Law: `AsOf.covers` answers containment through the engine's own product order (`Diff.v(...).lessEqual`), so watermark and retention reads share exactly the order the trace retains under; `AsOf.Order` is the total single-lane order sorts and resume tokens use — the two orders answer different questions and neither substitutes.
- Law: cross-replica completeness is never an `AsOf` question — the coordinate orders one journal lane; multi-replica frontiers are `Vector` facts folded in `causal`, and the two vocabularies meet only at the watermark.
- Growth: a branch axis is a key-space partition (a plan key row), never a fourth coordinate.
- Boundary: the data branch mints `AsOf` values from journal positions; history scrubbing consumes `read`/`diff` through served views.

```typescript
const _Sequence = Schema.BigIntFromSelf.pipe(Schema.nonNegativeBigInt(), Schema.brand("Sequence"))
const _sequence = Schema.decodeSync(_Sequence)
const _WINDOW = 9007199254740991n // Number.MAX_SAFE_INTEGER: the engine's number plane; admission bounds every coordinate inside it

class AsOf extends Schema.Class<AsOf>("AsOf")(
  Schema.Struct({
    stamp: Hlc,
    ordinal: _Sequence,
  }).pipe(Schema.filter((asOf) => asOf.stamp.physical <= _WINDOW && asOf.stamp.logical <= _WINDOW && asOf.ordinal <= _WINDOW)),
) {
  static readonly Order: Order.Order<AsOf> = Order.combine(
    Order.mapInput(Hlc.Order, (asOf: AsOf) => asOf.stamp),
    Order.mapInput(Order.bigint, (asOf: AsOf) => asOf.ordinal),
  )
  static readonly genesis: AsOf = new AsOf({ stamp: Hlc.genesis, ordinal: _sequence(0n) })
  static at(stamp: Hlc, sequence: bigint): AsOf {
    return new AsOf({ stamp, ordinal: _sequence(sequence) })
  }
  static time(asOf: AsOf): AsOf.Time {
    return [Number(asOf.stamp.physical), Number(asOf.stamp.logical), Number(asOf.ordinal)] as const
  }
  static covers(upper: AsOf, lower: AsOf): boolean {
    return Diff.v([...AsOf.time(lower)]).lessEqual(Diff.v([...AsOf.time(upper)]))
  }
}

declare namespace AsOf {
  type Time = readonly [physical: number, logical: number, ordinal: number]
}
```

## [04]-[MEMORY_LANE]

[MEMORY_LANE]:
- Owner: `Replay.memory` — the browser in-memory altitude: one d2mini graph per plan (`map` to the keyed lift, `reduce` under the instance reducer, `consolidate`, `output`), a `SubscriptionRef` table advanced by the drained change wave, and a synchronous `push` that sends the signed delta and drains the scheduler to quiescence inside one `Effect.sync`; `Replay.ordered` re-keys every folded entity under one internal board key before `orderByWithFractionalIndex`, so the operator ranks entities globally instead of ranking the single value inside each entity key, a change moves one fractional key without re-sorting the collection, and `ranks` projects the ordered chunk. The engine's `sorted-btree` ordered twins are sealed out of the installed barrel — the package `exports` map is `.`-only and the operators barrel omits them — so the fractional-index operator is the whole reachable ordered surface and the lane stays synchronous end to end.
- Owner: `Replay.view` — one lens entry over the handle modalities, discriminated on the input shape: a table handle alone yields the table view, a handle with a key yields the row view (`Option`-carried absence), an ordered handle yields the board view — overload signatures on one declaration, no `viewAll`/`viewByKey` siblings; `Replay.feed` flattens a handle's change wave into a per-row `Stream`, the delivery feed a serving edge frames onto its sockets.
- Law: the reducer the engines run is the elementwise projection of `Merge.combineMany` — `_reduced` expands positive multiplicities, folds the survivors, and emits one `[state, 1]` row; the engine recomputes retraction by re-folding the surviving contribution set, so retraction is lawful for every instance without group inverses, and negative net rows are upstream corruption the reducer treats as absent.
- Law: `_engine` is the one graph scaffold — one interior owner holds the pending sink, `finalize`, the one-permit drain, and the publish continuation for every lane on both engines, so a lane declares only its inputs, its pipeline stages, and its publish fold, and a new dataflow verb is one `_engine` wire, never a re-rolled scaffold; the permit brackets `sendData`/`run`/publish because the engine drain is a non-STM statement seam no transaction owns, the published cells stay `SubscriptionRef`s because views need the change stream, and transactional state lives where the cells are pure: `causal#FRONTIER_TRACKER` and `merge#MERGE_CELLS`.
- Law: every lens is `Subscribable.map` over the handle's state — get and changes stay coherent because they derive from one projection; a lens that re-runs the fold, caches its own copy, or subscribes twice is the re-derivation defect.
- Law: redelivery is absorbed at the value plane, never by the engine — an idempotent instance re-combines a redelivered contribution to its fixed point inside `_reduced`, and a non-idempotent instance (the counter) relies on `causal`'s admission shedding duplicate envelopes as `Drained` receipt evidence before any op reaches a fold; the engine's `consolidate` is multiplicity compaction whose keyed lane compares object values by reference and never consults `Equal`, so it is not a structural dedup and no lane claims it as one.
- Exemption: the `output` callback and the `pending` buffer it fills are the platform-forced statement seam — the engine's sink is a `void` callback; the buffer drains inside the same synchronous `run` and no mutable reference escapes the constructor closure.
- Boundary: which folds run in memory versus durably is the composition root's altitude selection; a serving edge consumes `view`/`feed` projections and never reaches the graph or the plan.
- Growth: a new lens modality is one overload line plus one projection arm.

```typescript
declare namespace Replay {
  type Memory<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<K, S>>
    readonly wave: Subscribable.Subscribable<Chunk.Chunk<Fold.Change<K, S>>>
  }
  type Ordered<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly ranks: Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
  }
  type JoinKind = "inner" | "left" | "right" | "full" | "anti"
  type Joined<OpL, OpR, K, P> = {
    readonly push: (delta: Fold.Delta<Either.Either<OpR, OpL>>) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<K, P>>
  }
  type Matched<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Either.Either<K, Op>>) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<K, S>>
  }
  type Agg<Op> =
    | { readonly kind: "count" }
    | { readonly kind: "avg" | "max" | "median" | "min" | "mode" | "sum"; readonly of: (op: Op) => number }
  type Rollup<Aggs> = { readonly [Column in keyof Aggs]: number }
  type AggregateOperator<Op> =
    | ReturnType<typeof Mini.groupByOperators.count<Op>>
    | ReturnType<typeof Mini.groupByOperators.avg<Op>>
    | ReturnType<typeof Mini.groupByOperators.max<Op>>
    | ReturnType<typeof Mini.groupByOperators.median<Op>>
    | ReturnType<typeof Mini.groupByOperators.min<Op>>
    | ReturnType<typeof Mini.groupByOperators.mode<Op>>
    | ReturnType<typeof Mini.groupByOperators.sum<Op>>
  type Grouped<Op, By, Aggs> = {
    readonly name: string
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<string, By & Rollup<Aggs>>>
  }
  type Topped<Op, K, S, G> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly boards: Subscribable.Subscribable<Fold.Table<G, Chunk.Chunk<readonly [K, S]>>>
  }
  type Closure<K> = {
    readonly push: (at: AsOf, edges: Fold.Delta<readonly [K, K]>) => Effect.Effect<void>
    readonly seal: (frontier: AsOf) => Effect.Effect<void>
    readonly reach: Subscribable.Subscribable<Fold.Table<K, HashSet.HashSet<K>>>
  }
  type Versioned<Op, K, S> = {
    readonly push: (at: AsOf, delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly seal: (frontier: AsOf) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<K, S>>
    readonly wave: Subscribable.Subscribable<Chunk.Chunk<Fold.Change<K, S>>>
    readonly frontier: Subscribable.Subscribable<Option.Option<AsOf>>
    readonly read: (upTo: AsOf) => Effect.Effect<Fold.Table<K, S>>
    readonly diff: (after: AsOf, upTo: AsOf) => Effect.Effect<Fold.Delta<readonly [K, S]>>
    readonly compact: (upTo: AsOf) => Effect.Effect<void>
  }
  type Shape = {
    readonly delta: <Op>(ops: ReadonlyArray<Op>) => Fold.Delta<Op>
    readonly memory: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Effect.Effect<Memory<Op, K, S>>
    readonly ordered: <Op, K, S>(
      plan: Fold.Plan<Op, K, S>,
      rank: Order.Order<S>,
      lens: { readonly limit?: number; readonly offset?: number },
    ) => Effect.Effect<Ordered<Op, K, S>>
    readonly joined: {
      <OpL, OpR, K, SL, SR>(
        left: Fold.Plan<OpL, K, SL>,
        right: Fold.Plan<OpR, K, SR>,
      ): Effect.Effect<Joined<OpL, OpR, K, readonly [SL, SR]>>
      <OpL, OpR, K, SL, SR>(
        left: Fold.Plan<OpL, K, SL>,
        right: Fold.Plan<OpR, K, SR>,
        kind: JoinKind,
      ): Effect.Effect<Joined<OpL, OpR, K, readonly [Option.Option<SL>, Option.Option<SR>]>>
    }
    readonly matched: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Effect.Effect<Matched<Op, K, S>>
    readonly grouped: <Op, By extends Readonly<Record<string, boolean | number | string>>, Aggs extends Readonly<Record<string, Agg<Op>>>>(spec: {
      readonly name: string
      readonly by: (op: Op) => By
      readonly aggs: Aggs
    }) => Effect.Effect<Grouped<Op, By, Aggs>>
    readonly topped: <Op, K, S, G>(
      plan: Fold.Plan<Op, K, S>,
      spec: { readonly by: (key: K, state: S) => G; readonly rank: Order.Order<S>; readonly take: number },
    ) => Effect.Effect<Topped<Op, K, S, G>>
    readonly closure: <K>(origin: AsOf) => Effect.Effect<Closure<K>>
    readonly versioned: <Op, K, S>(plan: Fold.Plan<Op, K, S>, origin: AsOf) => Effect.Effect<Versioned<Op, K, S>>
    readonly view: {
      <Op, K, S>(handle: Memory<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
      <Op, K, S>(handle: Versioned<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
      <Op, K, S>(handle: Memory<Op, K, S> | Versioned<Op, K, S>, key: K): Subscribable.Subscribable<Option.Option<S>>
      <Op, K, S>(handle: Ordered<Op, K, S>): Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
    }
    readonly feed: <Op, K, S>(handle: Memory<Op, K, S> | Versioned<Op, K, S>) => Stream.Stream<Fold.Change<K, S>>
  }
}

const _reduced = <S>(instance: Merge.Instance<S>) => (values: Array<[S, number]>): Array<[S, number]> => {
  const survivors = Array.flatMap(values, ([state, count]) => (count > 0 ? Array.makeBy(count, () => state) : []))
  return Array.isNonEmptyReadonlyArray(survivors)
    ? [[instance.combine.combineMany(Array.headNonEmpty(survivors), Array.tailNonEmpty(survivors)), 1]]
    : []
}

const _patch = <K, S>(table: Fold.Table<K, S>, change: Fold.Change<K, S>): Fold.Table<K, S> =>
  Option.match(change.state, {
    onNone: () => HashMap.remove(table, change.key),
    onSome: (state) => HashMap.set(table, change.key, state),
  })

const _changes = <K, S>(rows: ReadonlyArray<readonly [readonly [K, S], number]>): ReadonlyArray<Fold.Change<K, S>> => {
  const kept = Array.filterMap(rows, ([row, count]) => (count > 0 ? Option.some(row) : Option.none()))
  const dropped = Array.filterMap(rows, ([row, count]) => (count < 0 ? Option.some(row[0]) : Option.none()))
  return [
    ...Array.filterMap(dropped, (key) =>
      Array.some(kept, ([survivor]) => Equal.equals(survivor, key))
        ? Option.none()
        : Option.some<Fold.Change<K, S>>({ key, state: Option.none() })),
    ...Array.map(kept, ([key, state]): Fold.Change<K, S> => ({ key, state: Option.some(state) })),
  ]
}

const _rows = <A>(delta: Fold.Delta<A>): Array<[A, number]> => delta.map(([value, count]) => [value, count])

const _engine = <Row>(
  graph: { readonly finalize: () => void; readonly run: () => void },
  wire: (emit: (row: Row) => void) => void,
): Effect.Effect<{
  readonly drive: <A>(send: () => void, publish: (rows: ReadonlyArray<Row>) => Effect.Effect<A>) => Effect.Effect<A>
}> =>
  Effect.map(Effect.makeSemaphore(1), (gate) => {
    // BOUNDARY ADAPTER: the engine sink is a void callback and the drain is a statement seam; the pending buffer never escapes the closure
    const pending: Array<Row> = []
    wire((row) => pending.push(row))
    graph.finalize()
    return {
      drive: (send, publish) =>
        gate.withPermits(1)(Effect.flatMap(
          Effect.sync(() => {
            send()
            graph.run()
            return pending.splice(0, pending.length)
          }),
          publish,
        )),
    }
  })

const _keyed = <Op, K, S>(graph: Mini.D2, plan: Fold.Plan<Op, K, S>) => {
  const input = graph.newInput<Op>()
  return {
    input,
    staged: input.pipe(
      Mini.map((op: Op): Mini.KeyValue<K, S> => [plan.key(op), plan.lift(op)]),
      Mini.reduce(_reduced(plan.merge)),
    ),
  }
}

const _memory = <Op, K, S>(plan: Fold.Plan<Op, K, S>): Effect.Effect<Replay.Memory<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const engine = yield* _engine<Fold.Change<K, S>>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.consolidate(),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<K, S>>) => _changes(delta.getInner()).forEach(emit)),
      ))
    const state = yield* SubscriptionRef.make(HashMap.empty<K, S>())
    const wave = yield* SubscriptionRef.make(Chunk.empty<Fold.Change<K, S>>())
    return {
      push: (delta) =>
        engine.drive(
          () => keyed.input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) =>
            Effect.zipRight(
              Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
              Effect.when(Ref.set(wave, Chunk.fromIterable(drained)), () => Array.isNonEmptyReadonlyArray(drained)),
            ),
        ),
      state,
      wave,
    }
  })

const _ORDERED = "ordered" as const

const _ordered = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  rank: Order.Order<S>,
  lens: { readonly limit?: number; readonly offset?: number },
): Effect.Effect<Replay.Ordered<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const engine = yield* _engine<readonly [Mini.KeyValue<typeof _ORDERED, readonly [readonly [K, S], string]>, number]>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.map(([key, state]: Mini.KeyValue<K, S>): Mini.KeyValue<typeof _ORDERED, readonly [K, S]> =>
          [_ORDERED, [key, state] as const]),
        Mini.orderByWithFractionalIndex<Mini.KeyValue<typeof _ORDERED, readonly [K, S]>, S>((row) => row[1], { comparator: rank, ...lens }),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<typeof _ORDERED, readonly [readonly [K, S], string]>>) => {
          delta.getInner().forEach(emit)
        }),
      ))
    const board = yield* SubscriptionRef.make(SortedMap.empty<string, readonly [K, S]>(Order.string))
    return {
      push: (delta) =>
        engine.drive(
          () => keyed.input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) =>
            Ref.update(board, (held) =>
              Array.reduce(drained, held, (acc, [[, [[key, state], index]], count]) =>
                count > 0 ? SortedMap.set(acc, index, [key, state] as const) : SortedMap.remove(acc, index))),
        ),
      ranks: Subscribable.map(board, (held) => Chunk.fromIterable(SortedMap.values(held))),
    }
  })

function _view<Op, K, S>(handle: Replay.Memory<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
function _view<Op, K, S>(handle: Replay.Versioned<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
function _view<Op, K, S>(
  handle: Replay.Memory<Op, K, S> | Replay.Versioned<Op, K, S>,
  key: K,
): Subscribable.Subscribable<Option.Option<S>>
function _view<Op, K, S>(handle: Replay.Ordered<Op, K, S>): Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
function _view<Op, K, S>(
  handle: Replay.Memory<Op, K, S> | Replay.Ordered<Op, K, S> | Replay.Versioned<Op, K, S>,
  key?: K,
): Subscribable.Subscribable<Fold.Table<K, S> | Option.Option<S> | Chunk.Chunk<readonly [K, S]>> {
  return Predicate.hasProperty(handle, "ranks")
    ? handle.ranks
    : key === undefined
      ? handle.state
      : Subscribable.map(handle.state, (table) => HashMap.get(table, key))
}

const _feed = <Op, K, S>(
  handle: Replay.Memory<Op, K, S> | Replay.Versioned<Op, K, S>,
): Stream.Stream<Fold.Change<K, S>> => Stream.flattenIterables(handle.wave.changes)
```

## [05]-[DATAFLOW_VERBS]

[DATAFLOW_VERBS]:
- Owner: the verb handles — `Replay.joined` correlates two plans by key through the engine's ONE `join` operator parameterized by `JoinKind` (`inner`/`left`/`right`/`full`/`anti` are rows on it, never five handles), so evidence correlated by operation or command key is one maintained table, never a hand walk over two folded tables; `Replay.matched` is the `filterBy` semijoin — a plan's folded table restricted to keys a signed probe feed currently holds, the live-key gate a hand table intersection restates; `Replay.grouped` aggregates through the engine's whole `groupByOperators` roster (`sum`/`count`/`avg`/`min`/`max`/`median`/`mode`), the incremental rollup a hand recursion over a folded table restates; `Replay.topped` maintains the bounded top-`take` board per group through `topKWithFractionalIndex` — the group coordinate derives from the folded row through `by`, a change moves one fractional key inside one group's pane, and the full-order-then-slice a feed timeline or instant panel restates is deleted; `Replay.closure` runs the `iterate` fixpoint — transitive reachability over a keyed edge feed, the lane commit-graph ancestor closure and causal happened-before closure ride.
- Law: `joined` takes one push whose rows discriminate by `Either` — `Either.left` routes the left plan's op, `Either.right` the right's (`Either.Either<OpR, OpL>` spells the ecosystem's success-first parameter order, so the `Left` member IS the left plan's op) — so the correlated handle keeps one write surface and no `pushLeft`/`pushRight` twin exists; both sides fold under their own plan instance before the join, so the correlation is between merged states, never raw ops. `matched` rides the identical discriminated push — `Either.left` the plan's op, `Either.right` a probe key whose signed multiplicity enrolls or retires it — so semijoin membership is retractable data, never a rebuilt filter.
- Law: the join kind follows the input shape — the two-argument call is the inner join and its pair is total, the kind-bearing call returns `Option`-carried absence per side because the engine's outer arms speak `null`, and that `null` respells into `Option` inside the sink boundary so the interior never meets it; the anti kind is the left-only census whose right side is `Option.none` by construction.
- Law: `grouped` aggregates are a closed vocabulary row — `kind` plus the numeric projection — mapped onto the engine's own aggregate combinators at construction; the engine's aggregate identifier never reaches a consumer, and a new aggregate kind is one union arm plus one dispatch row; `median` and `mode` are roster rows like any other, so distributional rollups never re-materialize a group.
- Law: the rollup row is typed by derivation — `by` fields and the aggregate record parameter drive `By & Rollup<Aggs>` (`{ [Column in keyof Aggs]: number }`), so a consumer reads its named group dimensions and numeric columns while the table key stays the engine's serialized coordinate; `AggregateOperator<Op>` binds the full package roster to its published return types, and an erased `Record<string, unknown>` rollup or a sink assertion is absent.
- Law: group dimensions are JSON scalars by admission — `By` is bounded to `boolean | number | string` columns because the engine serializes its group key with bare `JSON.stringify`: a `bigint` coordinate throws inside the operator and an `undefined` member silently aliases two groups onto one serialized key, so a stamp-grade coordinate projects to `number` or `string` inside `by` before it becomes a dimension and the crash is unspellable at the type plane.
- Law: the grouped handle republishes `spec.name` beside its state and push surfaces, so registries and signal aspects consume the plan identity the constructor requires; the name is carried capability data, never a dead option field.
- Law: `closure` grows by self-join per pass — reach extended with reach-composed-with-reach, `concat` of the base, `distinct` closing the pass — converging to the transitive closure inside the engine's fixpoint scope; `distinct` is legal because reachability is grow-only within a version, and edge retraction arrives as a signed push the next fixpoint absorbs.
- Exemption: the verb sinks share the memory lane's platform-forced callback seam and its permit discipline; `_operators` is the marked `Record.map` key-correlation kernel — the package mapper homogenizes values, so one exact mapped-key rebinding restores the aggregate record's literal columns before `groupBy` consumes it.
- Law: `topped` re-keys the folded row onto its group coordinate before the bounded operator — one state per entity key under the plan instance, then `[G, [K, S]]` rows ranked per group — so the board ranks merged states, never raw ops, and eviction from a full pane arrives as the operator's own signed retraction, never a re-slice.
- Growth: a new dataflow verb (the versioned engine's `buffer` staging) is a new handle row on this family — the handle shapes and the plan contract never widen per verb.
- Boundary: `joined`/`matched`/`grouped`/`topped` ride the time-free engine; `closure` is versioned by nature (the fixpoint needs the frontier) and takes `AsOf` pushes like the versioned lane.

```typescript
const _agg = <Op>(row: Replay.Agg<Op>): Replay.AggregateOperator<Op> =>
  row.kind === "count" ? Mini.groupByOperators.count() : Mini.groupByOperators[row.kind]((op: Op) => row.of(op))

const _operators = <Op, Aggs extends Readonly<Record<string, Replay.Agg<Op>>>>(
  aggs: Aggs,
): { readonly [Column in keyof Aggs]: Replay.AggregateOperator<Op> } => {
  // BOUNDARY ADAPTER: Record.map preserves the key census at runtime but homogenizes it in types; the mapped-key result restores that census
  return Record.map(aggs, (row): Replay.AggregateOperator<Op> => _agg(row)) as unknown as {
    readonly [Column in keyof Aggs]: Replay.AggregateOperator<Op>
  }
}

function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
): Effect.Effect<Replay.Joined<OpL, OpR, K, readonly [SL, SR]>>
function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
  kind: Replay.JoinKind,
): Effect.Effect<Replay.Joined<OpL, OpR, K, readonly [Option.Option<SL>, Option.Option<SR>]>>
function _joined<OpL, OpR, K, SL, SR>(
  left: Fold.Plan<OpL, K, SL>,
  right: Fold.Plan<OpR, K, SR>,
  kind?: Replay.JoinKind,
): Effect.Effect<Replay.Joined<OpL, OpR, K, readonly [SL, SR] | readonly [Option.Option<SL>, Option.Option<SR>]>> {
  return Effect.gen(function* () {
    const graph = new Mini.D2()
    const lhs = _keyed(graph, left)
    const rhs = _keyed(graph, right)
    const paired = kind === undefined
      ? lhs.staged.pipe(Mini.innerJoin(rhs.staged))
      : lhs.staged.pipe(
          Mini.join(rhs.staged, kind),
          Mini.map(([key, [sl, sr]]: Mini.KeyValue<K, [SL | null, SR | null]>): Mini.KeyValue<K, readonly [Option.Option<SL>, Option.Option<SR>]> =>
            [key, [Option.fromNullable(sl), Option.fromNullable(sr)] as const]), // the engine's outer arms speak null; the respell into Option happens here and never past the sink
        )
    const engine = yield* _engine<Fold.Change<K, readonly [SL, SR] | readonly [Option.Option<SL>, Option.Option<SR>]>>(graph, (emit) =>
      paired.pipe(
        Mini.consolidate(),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<K, readonly [SL, SR] | readonly [Option.Option<SL>, Option.Option<SR>]>>) =>
          _changes(delta.getInner()).forEach(emit)),
      ))
    const state = yield* SubscriptionRef.make(
      HashMap.empty<K, readonly [SL, SR] | readonly [Option.Option<SL>, Option.Option<SR>]>(),
    )
    return {
      push: (delta: Fold.Delta<Either.Either<OpR, OpL>>) =>
        pipe(
          Array.partitionMap(delta, ([op, count]) =>
            Either.match(op, {
              onLeft: (held): Either.Either<readonly [OpR, number], readonly [OpL, number]> =>
                Either.left([held, count] as const),
              onRight: (held) => Either.right([held, count] as const),
            })),
          ([lows, rows]) =>
            engine.drive(
              () => {
                lhs.input.sendData(new Mini.MultiSet(_rows(lows)))
                rhs.input.sendData(new Mini.MultiSet(_rows(rows)))
              },
              (drained) => Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
            ),
        ),
      state,
    }
  })
}

const _matched = <Op, K, S>(plan: Fold.Plan<Op, K, S>): Effect.Effect<Replay.Matched<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const probe = graph.newInput<Mini.KeyValue<K, boolean>>()
    const engine = yield* _engine<Fold.Change<K, S>>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.filterBy(probe),
        Mini.consolidate(),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<K, S>>) => _changes(delta.getInner()).forEach(emit)),
      ))
    const state = yield* SubscriptionRef.make(HashMap.empty<K, S>())
    return {
      push: (delta) =>
        pipe(
          Array.partitionMap(delta, ([row, count]) =>
            Either.match(row, {
              onLeft: (op): Either.Either<readonly [Mini.KeyValue<K, boolean>, number], readonly [Op, number]> =>
                Either.left([op, count] as const),
              onRight: (key): Either.Either<readonly [Mini.KeyValue<K, boolean>, number], readonly [Op, number]> => {
                const membership: Mini.KeyValue<K, boolean> = [key, true]
                const change: readonly [Mini.KeyValue<K, boolean>, number] = [membership, count]
                return Either.right(change)
              },
            })),
          ([ops, keys]) =>
            engine.drive(
              () => {
                keyed.input.sendData(new Mini.MultiSet(_rows(ops)))
                probe.sendData(new Mini.MultiSet(_rows(keys)))
              },
              (drained) => Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
            ),
        ),
      state,
    }
  })

const _grouped = <Op, By extends Readonly<Record<string, boolean | number | string>>, Aggs extends Readonly<Record<string, Replay.Agg<Op>>>>(spec: {
  readonly name: string
  readonly by: (op: Op) => By
  readonly aggs: Aggs
}): Effect.Effect<Replay.Grouped<Op, By, Aggs>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const input = graph.newInput<Op>()
    const engine = yield* _engine<Fold.Change<string, By & Replay.Rollup<Aggs>>>(graph, (emit) =>
      input.pipe(
        Mini.groupBy(spec.by, _operators<Op, Aggs>(spec.aggs)),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<string, By & Replay.Rollup<Aggs>>>) =>
          _changes(delta.getInner()).forEach(emit)),
      ))
    const state = yield* SubscriptionRef.make(HashMap.empty<string, By & Replay.Rollup<Aggs>>())
    return {
      name: spec.name,
      push: (delta) =>
        engine.drive(
          () => input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) => Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
        ),
      state,
    }
  })

const _topped = <Op, K, S, G>(
  plan: Fold.Plan<Op, K, S>,
  spec: { readonly by: (key: K, state: S) => G; readonly rank: Order.Order<S>; readonly take: number },
): Effect.Effect<Replay.Topped<Op, K, S, G>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const keyed = _keyed(graph, plan)
    const engine = yield* _engine<readonly [Mini.KeyValue<G, readonly [readonly [K, S], string]>, number]>(graph, (emit) =>
      keyed.staged.pipe(
        Mini.map(([key, state]: Mini.KeyValue<K, S>): Mini.KeyValue<G, readonly [K, S]> => [spec.by(key, state), [key, state] as const]),
        Mini.topKWithFractionalIndex((left: readonly [K, S], right: readonly [K, S]) => spec.rank(left[1], right[1]), { limit: spec.take }),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<G, readonly [readonly [K, S], string]>>) => {
          delta.getInner().forEach(emit)
        }),
      ))
    const boards = yield* SubscriptionRef.make(HashMap.empty<G, SortedMap.SortedMap<string, readonly [K, S]>>())
    return {
      push: (delta) =>
        engine.drive(
          () => keyed.input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) =>
            Ref.update(boards, (held) =>
              Array.reduce(drained, held, (acc, [[group, [row, index]], count]) =>
                HashMap.modifyAt(acc, group, (slot) =>
                  pipe(
                    Option.getOrElse(slot, () => SortedMap.empty<string, readonly [K, S]>(Order.string)),
                    (pane) => (count > 0 ? SortedMap.set(pane, index, row) : SortedMap.remove(pane, index)), // eviction is the operator's signed retraction: one fractional key leaves, the pane never re-slices
                    (pane) => (SortedMap.size(pane) === 0 ? Option.none() : Option.some(pane)),
                  )))),
        ),
      boards: Subscribable.map(boards, HashMap.map((pane) => Chunk.fromIterable(SortedMap.values(pane)))),
    }
  })

const _closure = <K>(origin: AsOf): Effect.Effect<Replay.Closure<K>> =>
  Effect.gen(function* () {
    const graph = new Diff.D2({ initialFrontier: [...AsOf.time(origin)] })
    const input = graph.newInput<Diff.KeyValue<K, K>>()
    const engine = yield* _engine<readonly [Diff.KeyValue<K, K>, number]>(graph, (emit) =>
      input.pipe(
        Diff.iterate((paths) =>
          paths.pipe(
            Diff.map(([from, to]): Diff.KeyValue<K, K> => [to, from]),
            Diff.innerJoin(paths),
            Diff.map(([, [tail, next]]): Diff.KeyValue<K, K> => [tail, next]),
            Diff.concat(paths),
            Diff.distinct(),
          )),
        Diff.output((message: Diff.Message<Diff.KeyValue<K, K>>) => {
          if (message.type === Diff.MessageType.DATA) message.data.collection.getInner().forEach(emit)
        }),
      ))
    const reach = yield* SubscriptionRef.make(HashMap.empty<K, HashSet.HashSet<K>>())
    const drained = (rows: ReadonlyArray<readonly [Diff.KeyValue<K, K>, number]>) =>
      (table: Fold.Table<K, HashSet.HashSet<K>>): Fold.Table<K, HashSet.HashSet<K>> =>
        Array.reduce(rows, table, (acc, [[from, to], count]) =>
          HashMap.modifyAt(acc, from, (slot) =>
            pipe(
              Option.getOrElse(slot, () => HashSet.empty<K>()),
              (held) => (count > 0 ? HashSet.add(held, to) : HashSet.remove(held, to)),
              (next) => (HashSet.size(next) === 0 ? Option.none() : Option.some(next)),
            )))
    return {
      push: (at, edges) =>
        engine.drive(
          () =>
            input.sendData(
              Diff.v([...AsOf.time(at)]),
              new Diff.MultiSet(edges.map(([[from, to], count]) => [[from, to], count] as [Diff.KeyValue<K, K>, number])),
            ),
          (rows) => Ref.update(reach, drained(rows)),
        ),
      seal: (frontier) =>
        engine.drive(
          () => input.sendFrontier(Diff.v([...AsOf.time(frontier)])),
          (rows) => Ref.update(reach, drained(rows)),
        ),
      reach,
    }
  })
```

## [06]-[VERSIONED_LANE]

[VERSIONED_LANE]:
- Owner: `Replay.versioned` — the d2ts altitude: the graph opens at the origin frontier, `push` sends a signed delta at an `AsOf`, `seal` advances the input frontier and settles every version at or below it, and the sink appends every emitted state row into an owned engine `Index` trace — `trace.addValue(key, version, [state, multiplicity])` — so the retained history IS the engine's versioned trace and no hand-rolled slice log exists beside it.
- Law: `read(upTo)` is `Index.reconstructAt` — the trace's own time-travel read per key over `trace.keys()`, consolidated through the engine's `MultiSet` so the surviving state per key is the AsOf materialization; `diff(after, upTo)` is the negated earlier reconstruction concatenated with the later and consolidated — net signed rows with zero-sum churn removed, straight from the engine algebra.
- Law: `compact(upTo)` is `Index.compact` under an `Antichain` at the coordinate — the engine's own retention collapse; the coordinate arrives as an `AsOf` minted from `causal#FRONTIER_TRACKER`'s `Retention` plus the journal ordinal, so the trace never retains below what the journal retains, and a `read` or `diff` below the compaction floor is a retention breach the journal contract excludes, never a lane behavior.
- Law: `seal` is the only frontier writer and it writes what it sent — the frontier `Subscribable` advances from the seal argument, never parsed back out of engine messages, so watermark reads are exact.
- Law: trace reads are synchronous walks on the JS thread — the single-threaded engine seam keeps them coherent with the permit-gated writes, and no read observes a half-drained batch because the sink drains inside the same `run`.
- Exemption: the versioned `output` sink is the same platform-forced callback seam as the memory lane; the trace append happens inside the draining `Effect.sync`.
- Growth: a durable trace surviving restart is the engine's `./sqlite` subpath bound at the data branch's node altitude over the same plan — never a second fold implementation; a replication-fed lane binds the engine's `./electric` LSN-to-version bridge at the same altitude.
- Boundary: the data branch owns the durable and replication bindings and mints the compaction `AsOf`; this lane owns the in-process versioned fold.

```typescript
const _versioned = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  origin: AsOf,
): Effect.Effect<Replay.Versioned<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Diff.D2({ initialFrontier: [...AsOf.time(origin)] })
    const input = graph.newInput<Op>()
    const trace = new Diff.Index<K, S>()
    const engine = yield* _engine<Fold.Change<K, S>>(graph, (emit) =>
      input.pipe(
        Diff.map((op: Op): Diff.KeyValue<K, S> => [plan.key(op), plan.lift(op)]),
        Diff.reduce<K, S, S, Diff.KeyValue<K, S>>(_reduced(plan.merge)),
        Diff.consolidate(),
        Diff.output((message: Diff.Message<Diff.KeyValue<K, S>>) => {
          if (message.type === Diff.MessageType.DATA) {
            const rows = message.data.collection.getInner()
            for (const [[key, held], count] of rows) trace.addValue(key, message.data.version, [held, count])
            _changes(rows).forEach(emit)
          }
        }),
      ))
    const state = yield* SubscriptionRef.make(HashMap.empty<K, S>())
    const wave = yield* SubscriptionRef.make(Chunk.empty<Fold.Change<K, S>>())
    const sealed = yield* SubscriptionRef.make(Option.none<AsOf>())
    const reconstructed = (key: K, at: AsOf): Option.Option<S> =>
      pipe(
        new Diff.MultiSet(trace.reconstructAt(key, Diff.v([...AsOf.time(at)]))).consolidate().getInner(),
        Array.filterMap(([held, count]) => (count > 0 ? Option.some(held) : Option.none())),
        Array.head,
      )
    const published = (drained: ReadonlyArray<Fold.Change<K, S>>): Effect.Effect<void> =>
      Effect.zipRight(
        Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
        Effect.when(Ref.set(wave, Chunk.fromIterable(drained)), () => Array.isNonEmptyReadonlyArray(drained)),
      )
    return {
      push: (at, delta) =>
        engine.drive(
          () =>
            input.sendData(
              Diff.v([...AsOf.time(at)]),
              new Diff.MultiSet(_rows(delta)),
            ),
          published,
        ),
      seal: (frontier) =>
        engine.drive(
          () => input.sendFrontier(Diff.v([...AsOf.time(frontier)])),
          (drained) => Effect.zipRight(published(drained), Ref.set(sealed, Option.some(frontier))),
        ),
      state,
      wave,
      frontier: sealed,
      read: (upTo) =>
        Effect.sync(() =>
          Array.reduce(trace.keys(), HashMap.empty<K, S>(), (table, key) =>
            Option.match(reconstructed(key, upTo), {
              onNone: () => table,
              onSome: (held) => HashMap.set(table, key, held),
            }))),
      diff: (after, upTo) =>
        Effect.sync(() =>
          Array.flatMap(trace.keys(), (key) =>
            pipe(
              new Diff.MultiSet(trace.reconstructAt(key, Diff.v([...AsOf.time(after)])))
                .negate()
                .concat(new Diff.MultiSet(trace.reconstructAt(key, Diff.v([...AsOf.time(upTo)]))))
                .consolidate()
                .getInner(),
              Array.map(([held, count]) => [[key, held] as const, count] as const),
            ))),
      compact: (upTo) =>
        engine.drive(
          () => trace.compact(new Diff.Antichain([Diff.v([...AsOf.time(upTo)])])),
          () => Effect.void,
        ),
    }
  })

const Replay: Replay.Shape = {
  delta: (ops) => Array.map(ops, (op) => [op, 1] as const),
  memory: _memory,
  ordered: _ordered,
  joined: _joined,
  matched: _matched,
  grouped: _grouped,
  topped: _topped,
  closure: _closure,
  versioned: _versioned,
  view: _view,
  feed: _feed,
}
```

## [07]-[WATERMARK_PANES]

[WATERMARK_PANES]:
- Owner: `Window` — event-time completeness and the pane algebra: `mark` folds per-replica last-seen stamps through the meet (`Option` because zero acked replicas bound nothing) paired with the `Uncertainty` window that prices its honesty; `verdict` collapses `Causal.compare` to `punctual`/`late`/`uncertain`; the schema-carried `Policy` is the admitted closed family, `spread` assigns fixed-window stamps, `session` incrementally derives gap-connected panes from a keyed event history, `panes` composes any plan under a `Data`-tupled `[pane, key]` coordinate, and `close` partitions every modality by the pane's carried boundary.
- Law: the lateness verdict reads the honest four-way answer — an op wholly before the mark is `late`, wholly after or equal is `punctual`, an overlap is `uncertain` — so lateness policy consumes a three-value vocabulary instead of fabricating precision at the boundary the hardware cannot support.
- Law: the watermark advances monotonically because each replica's last-seen stamp advances and the meet of ascending inputs ascends; a regressed ack is stale evidence the max-merge on the ack table absorbs before the meet ever sees it.
- Law: policy admission rejects non-finite or sub-millisecond spans, and `Sliding.maxPanes` bounds the exact `ceil(width / step)` fan-out inside `1..4096`; zero divisors, unbounded array allocation, and an infinite `Hlc.delta` saturation never reach a pane body. The fixed-window fan-out happens at `spread`, before the fold — the caller flat-maps ops across their panes and the plan stays single-key — and `spread` answers only panes containing the stamp, so a stride that does not divide the width never assigns an op outside its interval.
- Law: a session boundary depends on neighboring event time and cannot be a unary `spread` arm — `session` groups by the plan key, sorts the surviving signed multiset under `Hlc.Order`, joins events whose stamp is at or before the prior pane's `until`, extends that boundary by `gap`, and merges states through the plan instance; retractions rebuild the affected key from the surviving multiset, so sessions can split as well as merge without a shadow log or switch ladder.
- Law: pane coordinates live on the stamp's own bigint physical plane — spans convert once through `Hlc.delta`, so pane math, close comparison, and the composite fold key never re-derive milliseconds or narrow the stamp.
- Law: every `Pane` carries both `open` and the authoritative exclusive `until`; `close` partitions by that coordinate against `mark.window.earliest` — a pane closes only when its `until` is at or before the earliest credible watermark instant — so fixed and session windows share one uncertainty-honest terminal operation and no policy-dependent boundary is reconstructed after the fold.
- Growth: a fixed-membership window is one `Fixed` policy case plus one `spread` arm; a data-dependent window is one `Policy` case plus one handle modality selected by payload timing, while `Pane`, `panes`, and `close` absorb both unchanged.
- Boundary: ack collection is the feed owner's concern (serving sockets, journal positions); this cluster folds whatever ack table arrives; time-travel reads are the versioned handle's own `read`/`diff`, consumed directly with zero delegation hop.

```typescript
declare namespace Window {
  type Mark = Causal.Stamped
  type Verdict = (typeof _VERDICTS)[number]
  type Policy = typeof _Policy.Type
  type Fixed = typeof _Fixed.Type
  type SessionPolicy = typeof _Session.Type
  type Pane = Readonly<{ readonly open: bigint; readonly until: bigint }>
  type Key<K> = readonly [Pane, K]
  type Sessioned<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<Key<K>, S>>
  }
  type Shape = {
    readonly Policy: typeof _Policy
    readonly mark: (acks: HashMap.HashMap<Vector.Replica, Hlc>, window: Uncertainty) => Option.Option<Mark>
    readonly verdict: (op: Causal.Stamped, mark: Mark) => Verdict
    readonly spread: (policy: Fixed) => (stamp: Hlc) => Chunk.Chunk<Pane>
    readonly panes: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Fold.Plan<readonly [Pane, Op], Key<K>, S>
    readonly session: <Op, K, S>(
      plan: Fold.Plan<Op, K, S>,
      stamp: (op: Op) => Hlc,
      policy: SessionPolicy,
    ) => Effect.Effect<Sessioned<Op, K, S>>
    readonly close: <K, S>(
      table: Fold.Table<Key<K>, S>,
      mark: Mark,
    ) => readonly [closed: Fold.Table<Key<K>, S>, open: Fold.Table<Key<K>, S>]
  }
}

const _VERDICTS = ["punctual", "late", "uncertain"] as const

const _VERDICT_BY_ORDER: Record<Vector.Ordering, Window.Verdict> = {
  before: "late",
  after: "punctual",
  equal: "punctual",
  concurrent: "uncertain",
}

const _earliest: Merge.Instance<Hlc> = Merge.min(Hlc.Order)

const _Span = Schema.DurationFromSelf.pipe(
  Schema.filter((span) => Duration.isFinite(span) && Duration.greaterThanOrEqualTo(span, Duration.millis(1))),
)
const _Fanout = Schema.Int.pipe(Schema.between(1, 4096))
const _Tumbling = Schema.TaggedStruct("Tumbling", { width: _Span })
const _Sliding = Schema.TaggedStruct("Sliding", {
  width: _Span,
  step: _Span,
  maxPanes: _Fanout,
}).pipe(
  Schema.filter((policy) =>
    (Hlc.delta(policy.width) + Hlc.delta(policy.step) - 1n) / Hlc.delta(policy.step) <= BigInt(policy.maxPanes)),
)
const _Session = Schema.TaggedStruct("Session", { gap: _Span })
const _Fixed = Schema.Union(_Tumbling, _Sliding)
const _Policy = Schema.Union(_Fixed, _Session)

const _pane = (open: bigint, until: bigint): Window.Pane => Data.struct({ open, until })

const _spread = (policy: Window.Fixed) => (stamp: Hlc): Chunk.Chunk<Window.Pane> =>
  Match.valueTags(policy, {
    Tumbling: ({ width }) => {
      const span = Hlc.delta(width)
      const open = (stamp.physical / span) * span
      return Chunk.of(_pane(open, open + span))
    },
    Sliding: ({ width, step, maxPanes }) => {
      const span = Hlc.delta(width)
      const stride = Hlc.delta(step)
      const at = stamp.physical
      const last = (at / stride) * stride
      const count = Math.min(Number((span + stride - 1n) / stride), maxPanes)
      return Chunk.filter(
        Chunk.map(Chunk.range(0, count - 1), (back) => {
          const open = last - BigInt(back) * stride
          return _pane(open, open + span)
        }),
        (pane) => pane.open <= at && at < pane.until,
      )
    },
  })

const _panes = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
): Fold.Plan<readonly [Window.Pane, Op], Window.Key<K>, S> =>
  Fold.plan({
    name: `${plan.name}/paned`,
    key: ([pane, op]) => Data.tuple(pane, plan.key(op)),
    lift: ([, op]) => plan.lift(op),
    merge: plan.merge,
  })

const _sessionRows = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  stamp: (op: Op) => Hlc,
  gap: bigint,
) => (values: Array<[Op, number]>): Array<[readonly [Window.Pane, S], number]> =>
  pipe(
    Array.flatMap(values, ([op, count]) => (count > 0 ? Array.makeBy(count, () => op) : [])),
    Array.sort(Order.mapInput(Hlc.Order, stamp)),
    Array.reduce([] as Array<readonly [Window.Pane, S]>, (sessions, op) => {
      const at = stamp(op).physical
      const state = plan.lift(op)
      return Array.match(sessions, {
        onEmpty: () => [[_pane(at, at + gap), state] as const],
        onNonEmpty: (held) => {
          const prior = Array.lastNonEmpty(held)
          return at <= prior[0].until
            ? [
                ...Array.initNonEmpty(held),
                [_pane(prior[0].open, at + gap), plan.merge.combine.combine(prior[1], state)] as const,
              ]
            : [...held, [_pane(at, at + gap), state] as const]
        },
      })
    }),
    Array.map((session): [readonly [Window.Pane, S], number] => [session, 1]),
  )

const _session = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  stamp: (op: Op) => Hlc,
  policy: Window.SessionPolicy,
): Effect.Effect<Window.Sessioned<Op, K, S>> =>
  Effect.gen(function* () {
    const graph = new Mini.D2()
    const input = graph.newInput<Op>()
    const engine = yield* _engine<Fold.Change<Window.Key<K>, S>>(graph, (emit) =>
      input.pipe(
        Mini.map((op: Op): Mini.KeyValue<K, Op> => [plan.key(op), op]),
        Mini.reduce(_sessionRows(plan, stamp, Hlc.delta(policy.gap))),
        Mini.map(([key, [pane, state]]: Mini.KeyValue<K, readonly [Window.Pane, S]>): Mini.KeyValue<Window.Key<K>, S> =>
          [Data.tuple(pane, key), state]),
        Mini.consolidate(),
        Mini.output((delta: Mini.MultiSet<Mini.KeyValue<Window.Key<K>, S>>) =>
          _changes(delta.getInner()).forEach(emit)),
      ))
    const state = yield* SubscriptionRef.make(HashMap.empty<Window.Key<K>, S>())
    return {
      push: (delta) =>
        engine.drive(
          () => input.sendData(new Mini.MultiSet(_rows(delta))),
          (drained) => Ref.update(state, (table) => Array.reduce(drained, table, _patch)),
        ),
      state,
    }
  })

const _close = <K, S>(
  table: Fold.Table<Window.Key<K>, S>,
  mark: Window.Mark,
): readonly [Fold.Table<Window.Key<K>, S>, Fold.Table<Window.Key<K>, S>] => {
  const sealed = (key: Window.Key<K>): boolean => key[0].until <= mark.window.earliest
  return [
    HashMap.filter(table, (_state, key) => sealed(key)),
    HashMap.filter(table, (_state, key) => !sealed(key)),
  ] as const
}

const Window: Window.Shape = {
  Policy: _Policy,
  mark: (acks, window) =>
    Option.map(
      Merge.fold(_earliest, Array.fromIterable(HashMap.values(acks))),
      (stamp) => ({ stamp, window }),
    ),
  verdict: (op, mark) => _VERDICT_BY_ORDER[Causal.compare(op, mark)],
  spread: _spread,
  panes: _panes,
  session: _session,
  close: _close,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AsOf, Fold, Replay, Window }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
