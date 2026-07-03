# [STATE_REPLAY]

`fold/replay.ts` is the incremental lane and the replay law. REPLAY_LAW: a fold rebuilt from any event prefix and advanced by the remaining deltas is equivalent — under the plan's own `alike` — to the live fold of the whole history; the algebraic half is `Converge.commutes`, and this module supplies the operational half by maintaining every fold incrementally through the differential engines so no consumer ever re-folds a prefix or re-sorts a collection on change (the array re-sort fallback is the named discard). One `Fold.Plan` drives three handles on one operator algebra: `Replay.memory` (d2mini, browser-safe, time-free), `Replay.ordered` (the d2mini `sorted-btree` fractional-index lane for live ordered views), and `Replay.versioned` (d2ts core, partial-order time, the retained slice log serving `query/window` AsOf reads and compacting on the `causal/order` retention frontier). The durable altitude is the same algebra: `store/project` binds these plans over the d2ts durable trace — never a second fold implementation.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                    | [SURFACE]                                        |
| :-----: | :--------------- | :-------------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | [LANE_CONTRACT]  | the engine time coordinate, delta lift, reducer projection, handle shapes   | `Replay.Time`, `Replay.time/covers/delta`, handle types |
|  [02]   | [MEMORY_LANE]    | the time-free in-memory handle and the ordered BTree handle                 | `Replay.memory`, `Replay.ordered`                   |
|  [03]   | [VERSIONED_LANE] | the d2ts handle — push/seal, retained slices, AsOf read, diff, compaction   | `Replay.versioned`                                  |

## [2]-[LANE_CONTRACT]

- Owner: `Replay.Time` — the three-coordinate engine time `[physical, logical, ordinal]`: the two `Hlc` halves plus the journal ordinal; `Replay.time` composes it from `Hlc` and a `Refined.OrdinalKey`, and `Replay.covers` answers containment through the engine's own product order (`Diff.v(...).lessEqual`), so `query/window` never re-derives version comparison.
- Packages: `@electric-sql/d2mini` (namespace `Mini`), `@electric-sql/d2ts` (namespace `Diff`), `effect`, `@rasm/ts/kernel` (`Hlc`, `Refined`), `../crdt/merge.ts`, `./algebra.ts`.
- Law: the engine coordinate is the number plane — `Replay.time` narrows the bigint stamp halves through `Number` at this one seam, total in practice because the physical half is kernel-clamped epoch milliseconds and the logical half a small counter, both far inside the 2^53 window; the narrowed coordinate drives engines only and never re-enters `Hlc`.
- Law: handle writes serialize — every constructor mints a one-permit semaphore and `push`/`seal` run under `withPermits(1)`, so the graph and its pending buffer are single-writer by construction while `state`/`frontier` reads stay lock-free `Ref` snapshots.
- Law: the reducer the engines run is the elementwise projection of `Merge.combineMany` — `_reduced` expands positive multiplicities, folds the survivors, and emits one `[state, 1]` row; the engine recomputes retraction by re-folding the surviving contribution set, so retraction is lawful for every instance without group inverses, and negative net rows are upstream corruption the reducer treats as absent.
- Law: `Replay.delta` is the only op-to-delta lift — `[op, 1]` rows — so push surfaces take exactly one input shape (`Fold.Delta<Op>`) and no arity or sign twin exists beside them.
- Law: handles expose read-only projections — `Subscribable` state, `Subscribable` change wave — and one push verb; a consumer can neither reach the graph nor mutate the table, so the engine seam is closed by construction.
- Growth: a new dataflow verb (join lane, semijoin filter, fixpoint closure over `Diff.iterate`) is a new pipeline row inside the owning handle constructor — the handle shapes and the plan contract never widen per verb.

```typescript
import * as Mini from "@electric-sql/d2mini"
import * as Diff from "@electric-sql/d2ts"
import { Array, Chunk, Effect, Equal, HashMap, Option, Order, Ref, SortedMap, Subscribable, SubscriptionRef } from "effect"
import type { Hlc, Refined } from "@rasm/ts/kernel"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "./algebra.ts"

declare namespace Replay {
  type Time = readonly [physical: number, logical: number, ordinal: number]
  type Slice<K, S> = { readonly at: Time; readonly rows: Fold.Delta<readonly [K, S]> }
  type Memory<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<K, S>>
    readonly wave: Subscribable.Subscribable<Chunk.Chunk<Fold.Change<K, S>>>
  }
  type Ordered<Op, K, S> = {
    readonly push: (delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly ranks: Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
  }
  type Versioned<Op, K, S> = {
    readonly push: (at: Time, delta: Fold.Delta<Op>) => Effect.Effect<void>
    readonly seal: (frontier: Time) => Effect.Effect<void>
    readonly state: Subscribable.Subscribable<Fold.Table<K, S>>
    readonly frontier: Subscribable.Subscribable<Option.Option<Time>>
    readonly read: (upTo: Time) => Effect.Effect<Fold.Table<K, S>>
    readonly diff: (after: Time, upTo: Time) => Effect.Effect<Fold.Delta<readonly [K, S]>>
    readonly compact: (upTo: Time) => Effect.Effect<void>
  }
  type Shape = {
    readonly time: (stamp: Hlc, ordinal: Refined.OrdinalKey) => Time
    readonly covers: (upper: Time, lower: Time) => boolean
    readonly delta: <Op>(ops: ReadonlyArray<Op>) => Fold.Delta<Op>
    readonly memory: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Effect.Effect<Memory<Op, K, S>>
    readonly ordered: <Op, K, S>(
      plan: Fold.Plan<Op, K, S>,
      rank: Order.Order<S>,
      lens: { readonly limit?: number; readonly offset?: number },
    ) => Effect.Effect<Ordered<Op, K, S>>
    readonly versioned: <Op, K, S>(plan: Fold.Plan<Op, K, S>, origin: Time) => Effect.Effect<Versioned<Op, K, S>>
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
```

## [3]-[MEMORY_LANE]

- Owner: `Replay.memory` — the browser in-memory altitude: one d2mini graph per plan (`keyBy`-shaped map, `reduce` under `_reduced`, `consolidate`, `output`), a `SubscriptionRef` table advanced by the drained change wave, and a synchronous `push` that sends the signed delta and drains the scheduler to quiescence inside one `Effect.sync`.
- Law: `consolidate` and `distinct` are structural over the decoded op values — equal ops collapse before the fold, so idempotent delivery costs nothing beyond the value equality the op family already carries.
- Law: `Replay.ordered` is the array-re-sort deletion at its sharpest — `orderByWithFractionalIndexBTree` maintains rank as a fractional string index in O(log n) per change, the handle folds `[key, [state, index]]` rows into a `SortedMap` keyed by index, and `ranks` projects the ordered chunk; `loadBTree` resolves once at handle construction, escalated to a defect because a missing dynamic import is a platform fault, not a domain outcome.
- Exemption: the `output` callback and the `pending` buffer it fills are the platform-forced statement seam — the engine's sink is a `void` callback; the buffer drains inside the same synchronous `run` and no mutable reference escapes the constructor closure.
- Boundary: which folds run in memory versus durably is the app root's altitude selection; `query/live` wraps these handles into live views and never touches the graph.

```typescript
const _memory = <Op, K, S>(plan: Fold.Plan<Op, K, S>): Effect.Effect<Replay.Memory<Op, K, S>> =>
  Effect.gen(function* () {
    const gate = yield* Effect.makeSemaphore(1)
    const graph = new Mini.D2()
    const input = graph.newInput<Op>()
    const pending: Array<Fold.Change<K, S>> = []
    input.pipe(
      Mini.map((op: Op): Mini.KeyValue<K, S> => [plan.key(op), plan.lift(op)]),
      Mini.reduce(_reduced(plan.merge)),
      Mini.consolidate(),
      Mini.output((delta: Mini.MultiSet<Mini.KeyValue<K, S>>) => {
        for (const change of _changes(delta.getInner())) pending.push(change)
      }),
    )
    graph.finalize()
    const state = yield* SubscriptionRef.make(HashMap.empty<K, S>())
    const wave = yield* SubscriptionRef.make(Chunk.empty<Fold.Change<K, S>>())
    return {
      push: (delta) =>
        gate.withPermits(1)(Effect.gen(function* () {
          const drained = yield* Effect.sync(() => {
            input.sendData(new Mini.MultiSet(delta.map(([op, count]) => [op, count] as [Op, number])))
            graph.run()
            return Chunk.fromIterable(pending.splice(0, pending.length))
          })
          yield* Ref.update(state, (table) => Chunk.reduce(drained, table, _patch))
          yield* Ref.set(wave, drained)
        })),
      state,
      wave,
    }
  })

const _ordered = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  rank: Order.Order<S>,
  lens: { readonly limit?: number; readonly offset?: number },
): Effect.Effect<Replay.Ordered<Op, K, S>> =>
  Effect.gen(function* () {
    yield* Effect.orDie(Effect.tryPromise(() => Mini.loadBTree()))
    const gate = yield* Effect.makeSemaphore(1)
    const graph = new Mini.D2()
    const input = graph.newInput<Op>()
    const pending: Array<readonly [Mini.KeyValue<K, readonly [S, string]>, number]> = []
    input.pipe(
      Mini.map((op: Op): Mini.KeyValue<K, S> => [plan.key(op), plan.lift(op)]),
      Mini.reduce(_reduced(plan.merge)),
      Mini.orderByWithFractionalIndexBTree((state: S) => state, { comparator: rank, ...lens }),
      Mini.output((delta: Mini.MultiSet<Mini.KeyValue<K, readonly [S, string]>>) => {
        for (const row of delta.getInner()) pending.push(row)
      }),
    )
    graph.finalize()
    const board = yield* SubscriptionRef.make(SortedMap.empty<string, readonly [K, S]>(Order.string))
    return {
      push: (delta) =>
        gate.withPermits(1)(Effect.gen(function* () {
          const rows = yield* Effect.sync(() => {
            input.sendData(new Mini.MultiSet(delta.map(([op, count]) => [op, count] as [Op, number])))
            graph.run()
            return pending.splice(0, pending.length)
          })
          yield* Ref.update(board, (held) =>
            Array.reduce(rows, held, (acc, [[key, [state, index]], count]) =>
              count > 0 ? SortedMap.set(acc, index, [key, state] as const) : SortedMap.remove(acc, index)))
        })),
      ranks: Subscribable.map(board, (held) => Chunk.fromIterable(SortedMap.values(held))),
    }
  })
```

## [4]-[VERSIONED_LANE]

- Owner: `Replay.versioned` — the d2ts core altitude: the graph opens at an origin frontier, `push` sends a signed delta at a `Replay.Time`, `seal` advances the input frontier and settles every version at or below it, the sink appends emitted state deltas as retained `Slice` rows, and the live table plus the sealed frontier publish as `Subscribable`s — the spine `query/window` reads.
- Law: `read(upTo)` folds retained slices covered by `upTo` in retention order — the AsOf materialization; `diff(after, upTo)` concatenates the slice rows strictly inside the window and consolidates them through the engine's own `MultiSet.consolidate`, so `asOfDiff` returns net signed rows with zero-sum churn removed.
- Law: a `read` or `diff` coordinate below the compaction floor is a retention breach, not a lane behavior — the floor handed to `compact` is the same `causal/order` frontier the journal guarantees no reader crosses, so the base slice a compaction mints is never mistaken for a window delta.
- Law: `compact(upTo)` collapses every covered slice into one base slice at `upTo` whose rows are the folded table — the owned mirror of the engine's `Index.compact`; the compaction coordinate arrives from `causal/order`'s retention frontier via `Replay.time`, so the trace never retains below what the journal retains.
- Law: `seal` is the only frontier writer and it writes what it sent — the frontier `Subscribable` advances from the seal argument, never parsed back out of engine messages, so watermark reads are exact.
- Exemption: the versioned `output` sink is the same platform-forced callback seam as the memory lane; the retained-slice append happens inside the draining `Effect.sync`.
- Boundary: the durable trace (`@electric-sql/d2ts` `./sqlite`, node-only) and its journal binding are `store/project`'s altitude; `Diff.MessageType` payload member spellings and the `Index` write surface are pinned against the shipped declarations before this lane goes load-bearing.
- Growth: a transitive fold (causal reachability, Merkle closure) is a `Diff.iterate` pipeline row inside this constructor; a second handle shape is never the answer.

```typescript
const _versioned = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
  origin: Replay.Time,
): Effect.Effect<Replay.Versioned<Op, K, S>> =>
  Effect.gen(function* () {
    const gate = yield* Effect.makeSemaphore(1)
    const graph = new Diff.D2({ initialFrontier: [...origin] })
    const input = graph.newInput<Op>()
    const pending: Array<readonly [Diff.KeyValue<K, S>, number]> = []
    input.pipe(
      Diff.map((op: Op): Diff.KeyValue<K, S> => [plan.key(op), plan.lift(op)]),
      Diff.reduce(_reduced(plan.merge)),
      Diff.consolidate(),
      Diff.output((message: Diff.Message<Diff.KeyValue<K, S>>) => {
        if (message.type === Diff.MessageType.DATA) {
          for (const row of message.data.collection.getInner()) pending.push(row)
        }
      }),
    )
    graph.finalize()
    const state = yield* SubscriptionRef.make(HashMap.empty<K, S>())
    const sealed = yield* SubscriptionRef.make(Option.none<Replay.Time>())
    const retained = yield* Ref.make(Chunk.empty<Replay.Slice<K, S>>())
    const applied = (table: Fold.Table<K, S>, rows: Fold.Delta<readonly [K, S]>): Fold.Table<K, S> =>
      Array.reduce(rows, table, (acc, [[key, held], count]) =>
        count > 0
          ? HashMap.set(acc, key, held)
          : Option.match(HashMap.get(acc, key), {
              onNone: () => acc,
              onSome: (current) => (plan.merge.alike(current, held) ? HashMap.remove(acc, key) : acc),
            }))
    const rebuilt = (slices: Chunk.Chunk<Replay.Slice<K, S>>, upTo: Replay.Time): Fold.Table<K, S> =>
      Chunk.reduce(
        Chunk.filter(slices, (slice) => Replay.covers(upTo, slice.at)),
        HashMap.empty<K, S>(),
        (table, slice) => applied(table, slice.rows),
      )
    const settle = (at: Replay.Time): Effect.Effect<void> =>
      Effect.gen(function* () {
        const rows = yield* Effect.sync(() => pending.splice(0, pending.length))
        yield* Effect.when(
          Effect.gen(function* () {
            yield* Ref.update(retained, Chunk.append({ at, rows }))
            yield* Ref.update(state, (table) => applied(table, rows))
          }),
          () => rows.length > 0,
        )
      })
    return {
      push: (at, delta) =>
        gate.withPermits(1)(Effect.gen(function* () {
          yield* Effect.sync(() => {
            input.sendData([...at], new Diff.MultiSet(delta.map(([op, count]) => [op, count] as [Op, number])))
            graph.run()
          })
          yield* settle(at)
        })),
      seal: (frontier) =>
        gate.withPermits(1)(Effect.gen(function* () {
          yield* Effect.sync(() => {
            input.sendFrontier([...frontier])
            graph.run()
          })
          yield* settle(frontier)
          yield* Ref.set(sealed, Option.some(frontier))
        })),
      state,
      frontier: sealed,
      read: (upTo) => Effect.map(Ref.get(retained), (slices) => rebuilt(slices, upTo)),
      diff: (after, upTo) =>
        Ref.get(retained).pipe(
          Effect.map((slices) =>
            new Diff.MultiSet(
              Chunk.toReadonlyArray(
                Chunk.filter(slices, (slice) => Replay.covers(upTo, slice.at) && !Replay.covers(after, slice.at)),
              ).flatMap((slice) => slice.rows.map(([row, count]) => [row, count] as [readonly [K, S], number])),
            )
              .consolidate()
              .getInner(),
          ),
        ),
      compact: (upTo) =>
        Ref.update(retained, (slices) =>
          Chunk.prepend(Chunk.filter(slices, (slice) => !Replay.covers(upTo, slice.at)), {
            at: upTo,
            rows: Array.map(
              Array.fromIterable(rebuilt(slices, upTo)),
              ([key, held]) => [[key, held] as const, 1] as const,
            ),
          })),
    }
  })

const Replay: Replay.Shape = {
  time: (stamp, ordinal) => [Number(stamp.physical), Number(stamp.logical), ordinal] as const,
  covers: (upper, lower) => Diff.v([...lower]).lessEqual(Diff.v([...upper])),
  delta: (ops) => Array.map(ops, (op) => [op, 1] as const),
  memory: _memory,
  ordered: _ordered,
  versioned: _versioned,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Replay }
```
