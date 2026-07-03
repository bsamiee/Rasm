# [STATE_ORDER]

`causal/order.ts` owns delivery order and finality: the happened-before fold that answers causality honestly under the kernel clock-uncertainty window, the causal delivery buffer that holds an op until its vector predecessors delivered, the stability frontier — the GLB meet of per-replica acknowledged vectors — the finalize partition that seals events the frontier covers, and the retention-frontier value handed to `store/journal/retain` and to `fold/replay` compaction. Every answer is four-way (`Vector.Ordering`): overlapping uncertainty windows yield `"concurrent"` rather than a fabricated order, so no consumer ever acts on clock precision the hardware never had — honesty is the vocabulary, not a caveat.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                  | [SURFACE]                                     |
| :-----: | :------------------ | :----------------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | [HAPPENED_BEFORE]   | the stamped-event comparison under honest uncertainty                    | `Causal.compare`, `Causal.Stamped`               |
|  [02]   | [DELIVERY_BUFFER]   | the causal hold-and-drain Mealy step                                     | `Causal.admit`, `Causal.Buffer`, `Causal.Envelope` |
|  [03]   | [FRONTIER_FINALIZE] | stability frontier (GLB meet), finalize partition, retention handoff     | `Causal.frontier`, `Causal.finalize`, `Causal.retention` |

## [2]-[HAPPENED_BEFORE]

- Owner: `Causal.compare` over `Causal.Stamped` — an event's `Hlc` stamp paired with its kernel `Uncertainty` window; wholly-disjoint windows decide `"before"`/`"after"`, identical stamps decide `"equal"`, and every overlap answers `"concurrent"` — the honest verdict `query/window` lateness and `crdt/merge` tiebreaks consume.
- Packages: `effect` (`Array`, `Chunk`, `Equal`, `HashMap`, `Option`); `@rasm/ts/kernel` (`Hlc`, `Uncertainty`); `./vector.ts`; `../crdt/merge.ts`.
- Law: vector evidence outranks clock evidence — when both operands carry vectors, `Vector.compare` decides and the windows are not consulted; the stamped comparison exists for the vectorless altitude (single-lane feeds, cross-tenant timelines), and the two spellings share one `Vector.Ordering` vocabulary.
- Law: `Uncertainty.precedes` is the only window read this module consumes — its `"before"`/`"after"` verdicts pass through, and `"indeterminate"` refines by stamp equality into `"equal"` or `"concurrent"`; the window's internal bound shape stays `kernel/clock`'s, and a physical-bound projection re-derived here would fork the uncertainty vocabulary.
- Growth: a new causality read (interval overlap census, k-way frontier compare) is a static on `Causal` composing the same two comparisons.

```typescript
import { Array, Chunk, Equal, HashMap, type Option, pipe } from "effect"
import { Hlc, Uncertainty } from "@rasm/ts/kernel"
import { Merge } from "../crdt/merge.ts"
import { Vector } from "./vector.ts"

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
  }
}

const _FINALITY = ["pending", "final"] as const

const _compare = (self: Causal.Stamped, that: Causal.Stamped): Vector.Ordering =>
  pipe(Uncertainty.precedes(self.window, that.window), (verdict) =>
    verdict === "indeterminate"
      ? Equal.equals(self.stamp, that.stamp) ? "equal" : "concurrent"
      : verdict)
```

## [3]-[DELIVERY_BUFFER]

- Owner: `Causal.admit` — the hold-and-drain Mealy step: an arriving `Envelope` joins the held set, then the drain extracts every envelope whose vector is exactly the next expected observation from its origin, advancing `seen` by `Vector.join` per delivery until no held envelope is deliverable — one step, `Stream.mapAccum`-ready, so a live feed gains causal delivery by lifting this declaration unchanged.
- Law: deliverability is the classic vector condition — the envelope's vector equals `Vector.observe(seen, origin)` on the origin axis and is covered by `seen` elsewhere, spelled as `Vector.covers(Vector.observe(buffer.seen, envelope.origin), envelope.vector)` — duplicate and already-covered envelopes drain immediately as no-ops because `seen` already covers them, giving idempotent delivery without a dedup set.
- Law: the drain is a fixpoint to data depth — each pass partitions the held set once, delivers every currently-deliverable envelope, and re-checks under the advanced `seen`, so recursion is bounded by the held census; within one pass at most one envelope per origin can qualify and no qualifier depends on an in-pass sibling, so batch emission is itself causal.
- Boundary: what delivery FEEDS is the consumer's fold — `fold/replay` pushes drained envelopes into the engine lane; this module never folds payloads.

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

## [4]-[FRONTIER_FINALIZE]

- Owner: the stability frontier and its consequences — `Causal.frontier` folds per-replica acknowledged vectors through `Vector.meet` (the GLB), `Causal.finality`/`Causal.finalize` seal what the frontier covers, and `Causal.retention` mints the handoff value.
- Law: the frontier is `Option` — meet over zero replicas has no lawful identity (the meet instance declares `empty: none`), so an unacked topology yields `Option.none` and no consumer ever compacts against a fabricated floor.
- Law: an envelope is `"final"` exactly when the frontier covers its vector — every replica has observed it, so no concurrent sibling can still arrive; finalize is the partition of a batch by that predicate, and finality is monotone because the frontier only ascends the lattice.
- Law: `Causal.Retention` is the one compaction coordinate — `floor` (the stable vector) plus the `Hlc` stamp at which it was computed; `store/journal/retain` compacts the journal below it and `fold/replay` consolidates its versioned trace below the same value, so retention decisions have exactly one source.
- Boundary: the durable retain lane is `store/journal/retain`; trace compaction is `fold/replay.md`'s `compact`; both consume `Causal.Retention` and neither recomputes a frontier.

```typescript
const Causal: Causal.Shape = {
  compare: _compare,
  buffer: () => ({ seen: Vector.zero, held: Chunk.empty() }),
  admit: _admit,
  frontier: (acks) => Merge.fold(Vector.meet, Array.fromIterable(HashMap.values(acks))),
  finality: (frontier, envelope) => (Vector.covers(frontier, envelope.vector) ? "final" : "pending"),
  finalize: (frontier, batch) =>
    pipe(
      Array.partition(batch, (envelope) => Causal.finality(frontier, envelope) === "final"),
      ([pending, final]) => [final, pending] as const,
    ),
  retention: (floor, stamp) => ({ floor, stamp }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Causal }
```
