# [CORE_CAUSAL]

The causality owner: `Vector` — the per-replica version vector whose comparison is the four-way causal ordering and whose join/meet are `Merge` lattice instances — plus delivery order and finality over it: the happened-before fold that answers causality honestly under the `value/clock` uncertainty window, the causal hold-and-drain buffer whose drained receipt separates deliveries from duplicate no-ops, the stability frontier (the GLB meet of the declared replica topology's acknowledged vectors), the finalize partition, the retention-frontier value handed to the durable journal and to `fold` compaction, and the live `Tracker` whose buffer advance is one `TRef` transaction and whose topology-seeded ack table is a `Merge.cell` over the `Vector.join` lattice — batch-atomic ack absorb, committed-snapshot frontier reads, whole-table `settled` stability waits. Every ordering answer is four-way: overlapping uncertainty windows yield `"concurrent"` rather than a fabricated order, so no consumer acts on clock precision the hardware never had. The version-vector wire shape C# mints decodes through the interchange codec INTO `Vector`, and no TS re-mint of a wire shape exists. The module is `core/src/state/causal.ts`; a new causality read is a static composing the same comparisons, a new tracker read is one transactional member.

## [01]-[INDEX]

- [02]-[VECTOR_LATTICE]: version-vector class, four-way comparison, join/meet instances; `Vector`.
- [03]-[HAPPENED_BEFORE]: the stamped-event comparison under honest uncertainty; `Causal.compare`, `Causal.Stamped`.
- [04]-[DELIVERY_BUFFER]: the causal hold-and-drain Mealy step and its drained receipt; `Causal.admit/.Buffer/.Envelope/.Drained`.
- [05]-[FRONTIER_TRACKER]: frontier fold, finalize partition, retention mint, STM tracker; `Causal.frontier/.finalize/.retention/.tracker`.

## [02]-[VECTOR_LATTICE]

[VECTOR_LATTICE]:
- Law: `Vector` encodes with its slots SORTED by replica id, the canonical order the peer runtimes hold, so one causal position yields one byte string wherever it is hashed; bucket or insertion order forks every digest taken over an encoded vector and is what leaves a cross-runtime fixture unfreezable.
- Boundary: `Envelope.identity` is a content digest and the `Dot` is the operation identity, and the two never merge — a dot repeating under a DIFFERENT content digest is an equivocation the buffer reports, where a log keying entries on content reads two peers writing identical bytes as one operation and discards the second.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import { Array, Chunk, Data, Effect, Equal, HashMap, Number, Option, Order, pipe, Record, Schema, STM, TRef } from "effect"
import { Clock } from "../value/clock.ts"
import { Digest } from "../value/contentKey.ts"
import { Shape } from "../value/schema.ts"
import { Merge } from "./merge.ts"

const _ORDERINGS = ["before", "after", "equal", "concurrent"] as const

const _Replica = Schema.NonEmptyString.pipe(Schema.brand("ReplicaId"))
const _Counter = Schema.Int.pipe(Schema.nonNegative())

const _Clocks = Schema.transform(
  Shape.Record(_Replica, _Counter),
  Schema.HashMapFromSelf({ key: Schema.typeSchema(_Replica), value: Schema.typeSchema(_Counter) }),
  {
    strict: true,
    decode: (record) => HashMap.fromIterable(Record.toEntries(record)),
    encode: (map) => Record.fromEntries(Array.sort(HashMap.toEntries(map), _bySlot)),
  },
)

const _bySlot: Order.Order<readonly [Vector.Replica, number]> = Order.mapInput(Order.string, ([replica]) => replica)

declare namespace Vector {
  type Ordering = (typeof _ORDERINGS)[number]
  type Replica = Schema.Schema.Type<typeof _Replica>
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
  ({
    combine: Semigroup.make(_pointwise(pick)),
    law: "semilattice",
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
- Growth: a new causality read (interval overlap census, k-way frontier compare) is a static on `Causal` composing the same two comparisons.

```typescript
declare namespace Causal {
  namespace Vector {
    type Ordering = (typeof _ORDERINGS)[number]
    type Replica = Schema.Schema.Type<typeof _Replica>
  }
  type Stamped = { readonly stamp: Clock.Hlc; readonly window: Clock.Uncertainty }
  type Envelope<A> = {
    readonly identity: Digest.Key<"content">
    readonly origin: Vector.Replica
    readonly vector: Vector
    readonly stamp: Clock.Hlc
    readonly payload: A
  }
  type Dot = readonly [origin: Vector.Replica, counter: number]
  type Buffer<A> = {
    readonly seen: Vector
    readonly identities: HashMap.HashMap<Dot, Digest.Key<"content">>
    readonly held: Chunk.Chunk<Envelope<A>>
  }
  type Equivocation<A> = { readonly prior: Digest.Key<"content">; readonly conflicting: Envelope<A> }
  type Drained<A> = {
    readonly delivered: Chunk.Chunk<Envelope<A>>
    readonly duplicates: Chunk.Chunk<Envelope<A>>
    readonly equivocations: Chunk.Chunk<Equivocation<A>>
  }
  type Finality = (typeof _FINALITY)[number]
  type Retention = _Retention
  type Tracker<A> = {
    readonly admit: (envelope: Envelope<A>) => Effect.Effect<Drained<A>, Merge.CellFault>
    readonly ack: (replica: Vector.Replica, vector: Vector) => Effect.Effect<void, Merge.CellFault>
    readonly seen: Effect.Effect<Vector>
    readonly frontier: Effect.Effect<Option.Option<Vector>>
    readonly stable: (target: Vector) => Effect.Effect<void>
    readonly retention: (stamp: Clock.Hlc) => Effect.Effect<Option.Option<Retention>>
  }
  type Shape = {
    readonly compare: (self: Stamped, that: Stamped) => Vector.Ordering
    readonly buffer: <A>() => Buffer<A>
    readonly admit: <A>(buffer: Buffer<A>, envelope: Envelope<A>) => readonly [Buffer<A>, Drained<A>]
    readonly frontier: (acks: HashMap.HashMap<Vector.Replica, Vector>) => Option.Option<Vector>
    readonly finality: (frontier: Vector, envelope: Envelope<unknown>) => Finality
    readonly finalize: <A>(
      frontier: Vector,
      batch: ReadonlyArray<Envelope<A>>,
    ) => readonly [final: ReadonlyArray<Envelope<A>>, pending: ReadonlyArray<Envelope<A>>]
    readonly retention: (frontier: Vector, stamp: Clock.Hlc) => Retention
    readonly tracker: <A>(replicas: Array.NonEmptyReadonlyArray<Vector.Replica>) => Effect.Effect<Tracker<A>>
    readonly Vector: typeof Vector
    readonly Retention: typeof _Retention
  }
}

const _FINALITY = ["pending", "final"] as const

class _Retention extends Schema.Class<_Retention>("Causal.Retention")({
  floor: Vector,
  stamp: Clock.Hlc,
}) {}

const _compare = (self: Causal.Stamped, that: Causal.Stamped): Vector.Ordering =>
  pipe(Clock.Uncertainty.precedes(self.window, that.window), (verdict) =>
    verdict === "indeterminate"
      ? Equal.equals(self.stamp, that.stamp) ? "equal" : "concurrent"
      : verdict)
```

## [04]-[DELIVERY_BUFFER]

[DELIVERY_BUFFER]:

```typescript
const _deliverable = <A>(seen: Vector, envelope: Causal.Envelope<A>): boolean =>
  Vector.covers(Vector.observe(seen, envelope.origin), envelope.vector)

const _dot = <A>(envelope: Causal.Envelope<A>): Causal.Dot =>
  Data.tuple(envelope.origin, _at(envelope.vector.clocks, envelope.origin))

const _dry = <A>(): Causal.Drained<A> => ({
  delivered: Chunk.empty(),
  duplicates: Chunk.empty(),
  equivocations: Chunk.empty(),
})

const _drain = <A>(
  seen: Vector,
  identities: HashMap.HashMap<Causal.Dot, Digest.Key<"content">>,
  held: Chunk.Chunk<Causal.Envelope<A>>,
  out: Causal.Drained<A>,
): readonly [Vector, HashMap.HashMap<Causal.Dot, Digest.Key<"content">>, Chunk.Chunk<Causal.Envelope<A>>, Causal.Drained<A>] =>
  pipe(Chunk.partition(held, (envelope) => Vector.covers(seen, envelope.vector)), ([fresh, covered]) =>
    pipe(
      Chunk.reduce(covered, out, (drained, envelope) =>
        Option.match(HashMap.get(identities, _dot(envelope)), {
          onNone: () => ({ ...drained, duplicates: Chunk.append(drained.duplicates, envelope) }),
          onSome: (prior) => prior === envelope.identity
            ? { ...drained, duplicates: Chunk.append(drained.duplicates, envelope) }
            : { ...drained, equivocations: Chunk.append(drained.equivocations, { prior, conflicting: envelope }) },
        })),
      (classified) => pipe(Chunk.partition(fresh, (envelope) => _deliverable(seen, envelope)), ([waiting, ready]) =>
      Option.match(Chunk.head(ready), {
        onNone: () => [seen, identities, waiting, classified] as const,
        onSome: (next) => _drain(
          Vector.join.combine.combine(seen, next.vector),
          HashMap.set(identities, _dot(next), next.identity),
          Chunk.appendAll(waiting, Chunk.drop(ready, 1)),
          {
            ...classified,
            delivered: Chunk.append(classified.delivered, next),
          },
        ),
      })),
    ))

const _admit = <A>(
  buffer: Causal.Buffer<A>,
  envelope: Causal.Envelope<A>,
): readonly [Causal.Buffer<A>, Causal.Drained<A>] => {
  const [seen, identities, held, drained] = _drain(
    buffer.seen,
    buffer.identities,
    Chunk.append(buffer.held, envelope),
    _dry<A>(),
  )
  return [{ seen, identities, held }, drained] as const
}
```

## [05]-[FRONTIER_TRACKER]

[FRONTIER_TRACKER]:

```typescript
const _frontier = (acks: HashMap.HashMap<Vector.Replica, Vector>): Option.Option<Vector> =>
  Merge.fold(Vector.meet, Array.fromIterable(HashMap.values(acks)))

const _tracker = <A>(replicas: Array.NonEmptyReadonlyArray<Vector.Replica>): Effect.Effect<Causal.Tracker<A>> =>
  Effect.gen(function* () {
    const topology: Array.NonEmptyReadonlyArray<Vector.Replica> = Array.dedupe(replicas)
    const cellBuffer = yield* STM.commit(TRef.make<Causal.Buffer<A>>({
      seen: Vector.zero,
      identities: HashMap.empty(),
      held: Chunk.empty(),
    }))
    const acks = yield* Merge.cell<Vector.Replica, Vector>(Vector.join, { keys: topology })
    yield* acks.absorb(Array.map(topology, (replica) => [replica, Vector.zero] as const))
    return {
      admit: (envelope) =>
        Array.contains(topology, envelope.origin)
          ? STM.commit(
          STM.gen(function* () {
            const held = yield* TRef.get(cellBuffer)
            const [next, drained] = _admit(held, envelope)
            yield* TRef.set(cellBuffer, next)
            return drained
          }),
          )
          : Effect.fail(new Merge.CellFault({ case: { reason: "unseated", key: envelope.origin } })),
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
        Effect.map(acks.table, (table) => Option.map(_frontier(table), (floor) => new _Retention({ floor, stamp }))),
    }
  })

const Causal: Causal.Shape = {
  Vector,
  Retention: _Retention,
  compare: _compare,
  buffer: () => ({ seen: Vector.zero, identities: HashMap.empty(), held: Chunk.empty() }),
  admit: _admit,
  frontier: _frontier,
  finality: (frontier, envelope) => (Vector.covers(frontier, envelope.vector) ? "final" : "pending"),
  finalize: (frontier, batch) =>
    pipe(
      Array.partition(batch, (envelope) => Causal.finality(frontier, envelope) === "final"),
      ([pending, final]) => [final, pending] as const,
    ),
  retention: (floor, stamp) => new _Retention({ floor, stamp }),
  tracker: _tracker,
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Causal }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
