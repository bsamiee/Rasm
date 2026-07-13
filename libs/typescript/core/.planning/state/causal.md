# [CORE_CAUSAL]

The causality owner: `Vector` — the per-replica version vector whose comparison is the four-way causal ordering and whose join/meet are `Merge` lattice instances — plus delivery order and finality over it: the happened-before fold that answers causality honestly under the `value/clock` uncertainty window, the causal hold-and-drain buffer, the stability frontier (the GLB meet of per-replica acknowledged vectors), the finalize partition, the retention-frontier value handed to the durable journal and to `fold` compaction, and the live `Tracker` whose buffer advance is one `TRef` transaction and whose ack table is a `Merge.cell` over the `Vector.join` lattice — batch-atomic ack absorb, committed-snapshot frontier reads, whole-table `settled` stability waits. Every ordering answer is four-way: overlapping uncertainty windows yield `"concurrent"` rather than a fabricated order, so no consumer acts on clock precision the hardware never had. The version-vector wire shape C# mints decodes through the interchange codec INTO `Vector`, and no TS re-mint of a wire shape exists. The module is `core/src/state/causal.ts`; a new causality read is a static composing the same comparisons, a new tracker read is one transactional member.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                         | [PUBLIC]                                           |
| :-----: | :----------------- | :------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `VECTOR_LATTICE`   | version-vector class, four-way comparison, join/meet instances | `Vector`                                           |
|  [02]   | `HAPPENED_BEFORE`  | the stamped-event comparison under honest uncertainty          | `Causal.compare`, `Causal.Stamped`                 |
|  [03]   | `DELIVERY_BUFFER`  | the causal hold-and-drain Mealy step                           | `Causal.admit`, `Causal.Buffer`, `Causal.Envelope` |
|  [04]   | `FRONTIER_TRACKER` | frontier fold, finalize partition, retention mint, STM tracker | `Causal.frontier/.finalize/.retention/.tracker`    |

## [02]-[VECTOR_LATTICE]

[VECTOR_LATTICE]:
- Owner: `Vector` — one `Schema.Class` whose `clocks` field is the decoded `HashMap` of replica counters; comparison, dominance, increment, and the two lattice instances ride the class as statics, so one import carries the shape, the decode target, and the whole causal-order algebra.
- Law: the wire clocks are a keyed object — the proto and msgpack map shape — and the interior `_Clocks` transform respells it into the `HashMap` at the field, replica keys re-proving their brand on admission; the encoded twin stays the keyed object the C# mint emits, and msgpack's `sortKeys` egress keeps it byte-canonical.
- Law: `Vector.compare` answers the happened-before question structurally — `"before"` when strictly dominated, `"after"` when strictly dominating, `"equal"` on identical clocks, `"concurrent"` when each side carries a count the other lacks — and the ordering vocabulary anchors on the interior tuple so every verdict consumer derives the same literal union.
- Law: `join` is the pointwise-max lattice (semilattice posture, empty `Vector.zero`) — the merge every delivery advances `seen` by; `meet` is the pointwise-min GLB over the key union with absent-as-zero, so a replica that never acked pins the frontier at zero rather than being skipped — the stability-frontier semantics the tracker requires.
- Law: replica identity is `Vector.Replica` — the schema rides the class as a static and the branded type rides the merged namespace, one spelling for presence actors, delivery origins, commit authors, and ack keys; a free-floating replica-id export is the named defect.
- Growth: a new causal comparison read is a static on `Vector`; a new ordering verdict is a `_ORDERINGS` row — consumers dispatching on `Vector.Ordering` break loudly at the missing arm.
- Boundary: the interchange codec decodes the C# version-vector wire into `Vector`; the commit graph riding vectors is `commit#COMMIT_OWNER`'s; both consume this owner and this owner imports neither.
- Packages: `@effect/typeclass` (`Semigroup.make`); `effect` (`Schema`, `Array`, `Chunk`, `Effect`, `Equal`, `HashMap`, `Number`, `Option`, `STM`, `TRef`); `../value/clock.ts` (`Hlc`, `Uncertainty`); `./merge.ts` (`Merge`).

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Array, Chunk, Effect, Equal, HashMap, Number, Option, pipe, Record, Schema, STM, TRef } from "effect"
import { Hlc, Uncertainty } from "../value/clock.ts"
import { Merge } from "./merge.ts"

const _ORDERINGS = ["before", "after", "equal", "concurrent"] as const

const _Replica = Schema.NonEmptyString.pipe(Schema.brand("ReplicaId"))
const _Counter = Schema.Int.pipe(Schema.nonNegative())

const _Clocks = Schema.transform(
  Schema.Record({ key: _Replica, value: _Counter }),
  Schema.HashMapFromSelf({ key: Schema.typeSchema(_Replica), value: Schema.typeSchema(_Counter) }),
  {
    strict: true,
    decode: (record) => HashMap.fromIterable(Record.toEntries(record)),
    encode: (map) => Record.fromEntries(HashMap.toEntries(map)),
  },
)

declare namespace Vector {
  type Ordering = (typeof _ORDERINGS)[number]
  type Replica = typeof _Replica.Type
}

const _at = (clocks: HashMap.HashMap<Vector.Replica, number>, replica: Vector.Replica): number =>
  Option.getOrElse(HashMap.get(clocks, replica), () => 0)

const _dominates = (
  self: HashMap.HashMap<Vector.Replica, number>,
  that: HashMap.HashMap<Vector.Replica, number>,
): boolean => HashMap.reduce(that, true, (holds, count, replica) => holds && count <= _at(self, replica))

const _pointwise = (pick: (left: number, right: number) => number) => (self: Vector, that: Vector): Vector =>
  new Vector({
    clocks: HashMap.reduce(
      HashMap.reduce(that.clocks, self.clocks, (acc, _count, replica) => HashMap.set(acc, replica, _at(acc, replica))),
      HashMap.empty<Vector.Replica, number>(),
      (acc, _count, replica) =>
        HashMap.set(acc, replica, pick(_at(self.clocks, replica), _at(that.clocks, replica))),
    ),
  })

const _lattice = (pick: (left: number, right: number) => number, empty: Option.Option<Vector>): Merge.Instance<Vector> =>
  Merge.instance({
    combine: Semigroup.make(_pointwise(pick)),
    posture: { commutative: true, idempotent: true },
    alike: (self, that) => Vector.compare(self, that) === "equal",
    empty,
  })

class Vector extends Schema.Class<Vector>("Vector")({
  clocks: _Clocks,
}) {
  static readonly Replica: typeof _Replica = _Replica
  static readonly zero: Vector = new Vector({ clocks: HashMap.empty() })
  static compare(self: Vector, that: Vector): Vector.Ordering {
    const forward = _dominates(self.clocks, that.clocks)
    const backward = _dominates(that.clocks, self.clocks)
    return forward && backward ? "equal" : forward ? "after" : backward ? "before" : "concurrent"
  }
  static covers(self: Vector, that: Vector): boolean {
    return _dominates(self.clocks, that.clocks)
  }
  static observe(self: Vector, replica: Vector.Replica): Vector {
    return new Vector({ clocks: HashMap.set(self.clocks, replica, _at(self.clocks, replica) + 1) })
  }
  static readonly join: Merge.Instance<Vector> = _lattice(Number.max, Option.some(Vector.zero))
  static readonly meet: Merge.Instance<Vector> = _lattice(Number.min, Option.none())
}
```

## [03]-[HAPPENED_BEFORE]

[HAPPENED_BEFORE]:
- Owner: `Causal.compare` over `Causal.Stamped` — an event's `Hlc` stamp paired with its `Uncertainty` window; wholly-disjoint windows decide `"before"`/`"after"`, identical stamps decide `"equal"`, and every overlap answers `"concurrent"` — the honest verdict `fold#WATERMARK_PANES` lateness and merge tiebreaks consume.
- Law: vector evidence outranks clock evidence — when both operands carry vectors, `Vector.compare` decides and the windows are not consulted; the stamped comparison exists for the vectorless altitude (single-lane feeds, cross-tenant timelines), and the two spellings share one `Vector.Ordering` vocabulary.
- Law: `Uncertainty.precedes` is the only window read this module consumes — its `"before"`/`"after"` verdicts pass through, and `"indeterminate"` refines by stamp equality into `"equal"` or `"concurrent"`; the window's internal bound shape stays `value/clock`'s, and a physical-bound projection re-derived here forks the uncertainty vocabulary.
- Growth: a new causality read (interval overlap census, k-way frontier compare) is a static on `Causal` composing the same two comparisons.

```typescript
declare namespace Causal {
  type Stamped = { readonly stamp: Hlc; readonly window: Uncertainty }
  type Envelope<A> = {
    readonly origin: Vector.Replica
    readonly vector: Vector
    readonly stamp: Hlc
    readonly payload: A
  }
  type Buffer<A> = { readonly seen: Vector; readonly held: Chunk.Chunk<Envelope<A>> }
  type Finality = (typeof _FINALITY)[number]
  type Retention = { readonly floor: Vector; readonly stamp: Hlc }
  type Tracker<A> = {
    readonly admit: (envelope: Envelope<A>) => Effect.Effect<Chunk.Chunk<Envelope<A>>>
    readonly ack: (replica: Vector.Replica, vector: Vector) => Effect.Effect<void>
    readonly seen: Effect.Effect<Vector>
    readonly frontier: Effect.Effect<Option.Option<Vector>>
    readonly stable: (target: Vector) => Effect.Effect<void>
    readonly retention: (stamp: Hlc) => Effect.Effect<Option.Option<Retention>>
  }
  type Shape = {
    readonly compare: (self: Stamped, that: Stamped) => Vector.Ordering
    readonly buffer: <A>() => Buffer<A>
    readonly admit: <A>(buffer: Buffer<A>, envelope: Envelope<A>) => readonly [Buffer<A>, Chunk.Chunk<Envelope<A>>]
    readonly frontier: (acks: HashMap.HashMap<Vector.Replica, Vector>) => Option.Option<Vector>
    readonly finality: (frontier: Vector, envelope: Envelope<unknown>) => Finality
    readonly finalize: <A>(
      frontier: Vector,
      batch: ReadonlyArray<Envelope<A>>,
    ) => readonly [final: ReadonlyArray<Envelope<A>>, pending: ReadonlyArray<Envelope<A>>]
    readonly retention: (frontier: Vector, stamp: Hlc) => Retention
    readonly tracker: <A>() => Effect.Effect<Tracker<A>>
  }
}

const _FINALITY = ["pending", "final"] as const

const _compare = (self: Causal.Stamped, that: Causal.Stamped): Vector.Ordering =>
  pipe(Uncertainty.precedes(self.window, that.window), (verdict) =>
    verdict === "indeterminate"
      ? Equal.equals(self.stamp, that.stamp) ? "equal" : "concurrent"
      : verdict)
```

## [04]-[DELIVERY_BUFFER]

[DELIVERY_BUFFER]:
- Owner: `Causal.admit` — the hold-and-drain Mealy step: an arriving `Envelope` joins the held set, then the drain extracts every envelope whose vector is exactly the next expected observation from its origin, advancing `seen` by `Vector.join` per delivery until no held envelope is deliverable — one step, `Stream.mapAccum`-ready, so a live feed gains causal delivery by lifting this declaration unchanged.
- Law: deliverability is the classic vector condition — the envelope's vector equals `Vector.observe(seen, origin)` on the origin axis and is covered by `seen` elsewhere, spelled as `Vector.covers(Vector.observe(buffer.seen, envelope.origin), envelope.vector)` — duplicate and already-covered envelopes drain immediately as no-ops because `seen` already covers them, giving idempotent delivery without a dedup set.
- Law: the drain is a fixpoint to data depth — each pass partitions the held set once, delivers every currently-deliverable envelope, and re-checks under the advanced `seen`, so recursion is bounded by the held census; within one pass at most one envelope per origin qualifies and no qualifier depends on an in-pass sibling, so batch emission is itself causal.
- Boundary: what delivery FEEDS is the consumer's fold — `fold` handles receive drained envelopes as engine deltas; this module never folds payloads.

```typescript
const _deliverable = <A>(seen: Vector, envelope: Causal.Envelope<A>): boolean =>
  Vector.covers(Vector.observe(seen, envelope.origin), envelope.vector)

const _drain = <A>(
  seen: Vector,
  held: Chunk.Chunk<Causal.Envelope<A>>,
  out: Chunk.Chunk<Causal.Envelope<A>>,
): readonly [Vector, Chunk.Chunk<Causal.Envelope<A>>, Chunk.Chunk<Causal.Envelope<A>>] =>
  pipe(Chunk.partition(held, (envelope) => _deliverable(seen, envelope)), ([waiting, ready]) =>
    Chunk.isEmpty(ready)
      ? [seen, held, out] as const
      : _drain(
          Chunk.reduce(ready, seen, (acc, envelope) => Vector.join.combine.combine(acc, envelope.vector)),
          waiting,
          Chunk.appendAll(out, ready),
        ))

const _admit = <A>(
  buffer: Causal.Buffer<A>,
  envelope: Causal.Envelope<A>,
): readonly [Causal.Buffer<A>, Chunk.Chunk<Causal.Envelope<A>>] => {
  const [seen, held, out] = _drain(buffer.seen, Chunk.append(buffer.held, envelope), Chunk.empty())
  return [{ seen, held }, out] as const
}
```

## [05]-[FRONTIER_TRACKER]

[FRONTIER_TRACKER]:
- Owner: the stability frontier and its consequences — `Causal.frontier` folds per-replica acknowledged vectors through `Vector.meet` (the GLB), `Causal.finality`/`Causal.finalize` seal what the frontier covers, `Causal.retention` mints the handoff value, and `Causal.tracker` holds the live cells: the delivery buffer as a `TRef` Mealy cell and the ack table as `merge#MERGE_CELLS`'s `Merge.cell(Vector.join)` — every advance one STM transaction.
- Law: the frontier is `Option` — meet over zero replicas has no lawful identity (the meet instance declares `empty: none`), so an unacked topology yields `Option.none` and no consumer compacts against a fabricated floor.
- Law: an envelope is `"final"` exactly when the frontier covers its vector — every replica has observed it, so no concurrent sibling can still arrive; finalize is the partition of a batch by that predicate, and finality is monotone because the frontier only ascends the lattice.
- Law: tracker advances are transactions, never a permit around a cell — `admit` reads the buffer, drains, and writes back in one commit so two concurrent admits re-run instead of tearing the held set; `ack` is one `Merge.cell` batch absorb — the keyed insert-or-combine through `Vector.join` is the cell's own fold, so a regressed ack is absorbed by the lattice before any frontier read sees it and no hand `HashMap.modifyAt` merge exists beside the roster.
- Law: `stable(target)` composes the cell's whole-table `settled` wait — suspends until the meet of the committed ack table covers the target, with zero polling: the transaction re-runs when any ack cell changes and the predicate closes over the same-transaction table snapshot, so the wake condition and the evidence are one atomic read; `frontier` and `retention` read the same committed snapshot through `acks.table`, never a raw cell walk.
- Law: `Causal.Retention` is the one compaction coordinate — `floor` (the stable vector) plus the `Hlc` stamp at which it was computed; the durable journal compacts below it and `fold#VERSIONED_LANE` compacts its trace below the same value, so retention decisions have exactly one source, and the tracker's `retention(stamp)` mints it from the live frontier in one transaction.
- Boundary: the durable retain lane and journal positions are the data branch's; trace compaction is `fold#VERSIONED_LANE`'s `compact`; both consume `Causal.Retention` and neither recomputes a frontier.

```typescript
const _frontier = (acks: HashMap.HashMap<Vector.Replica, Vector>): Option.Option<Vector> =>
  Merge.fold(Vector.meet, Array.fromIterable(HashMap.values(acks)))

const _tracker = <A>(): Effect.Effect<Causal.Tracker<A>> =>
  Effect.gen(function* () {
    const cellBuffer = yield* STM.commit(TRef.make<Causal.Buffer<A>>({ seen: Vector.zero, held: Chunk.empty() }))
    const acks = yield* Merge.cell<Vector.Replica, Vector>(Vector.join)
    return {
      admit: (envelope) =>
        STM.commit(
          STM.gen(function* () {
            const held = yield* TRef.get(cellBuffer)
            const [next, drained] = _admit(held, envelope)
            yield* TRef.set(cellBuffer, next)
            return drained
          }),
        ),
      ack: (replica, vector) => acks.absorb([[replica, vector] as const]),
      seen: Effect.map(STM.commit(TRef.get(cellBuffer)), (buffer) => buffer.seen),
      frontier: Effect.map(acks.table, _frontier),
      stable: (target) =>
        acks.settled((table) =>
          Option.match(_frontier(table), {
            onNone: () => false,
            onSome: (floor) => Vector.covers(floor, target),
          })),
      retention: (stamp) =>
        Effect.map(acks.table, (table) => Option.map(_frontier(table), (floor) => ({ floor, stamp }))),
    }
  })

const Causal: Causal.Shape = {
  compare: _compare,
  buffer: () => ({ seen: Vector.zero, held: Chunk.empty() }),
  admit: _admit,
  frontier: _frontier,
  finality: (frontier, envelope) => (Vector.covers(frontier, envelope.vector) ? "final" : "pending"),
  finalize: (frontier, batch) =>
    pipe(
      Array.partition(batch, (envelope) => Causal.finality(frontier, envelope) === "final"),
      ([pending, final]) => [final, pending] as const,
    ),
  retention: (floor, stamp) => ({ floor, stamp }),
  tracker: _tracker,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Causal, Vector }
```
