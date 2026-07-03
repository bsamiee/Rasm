# [STATE_WINDOW]

`query/window.ts` owns the windowed read surface and carries the REPLAY_LAW spine to consumers: `AsOf` — the three-coordinate read time (the two `Hlc` halves plus the journal ordinal) that projects into `Replay.Time` — with `read` and `asOfDiff` delegating to the versioned handle's retained lane, so a time-travel read IS the fold of the prefix at that coordinate and a diff IS the consolidated signed difference between two reads, never a second bookkeeping structure. Event-time completeness is the `Watermark`: the meet of per-replica last-seen stamps paired with the kernel uncertainty window, whose lateness verdict is `Causal.compare`'s honest four-way answer collapsed to `punctual`/`late`/`uncertain`. Window folds are policy values — tumbling, sliding, session — spread ops into pane coordinates that compose any `Fold.Plan` into a paned plan, and panes close by watermark passage, never by wall-clock timers.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                            | [SURFACE]                                          |
| :-----: | :---------------- | :------------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | [ASOF_COORDINATE] | the three-coordinate read time, its order, the versioned-lane reads | `AsOf`, `AsOf.Order/.time`, `Window.read`, `Window.asOfDiff` |
|  [02]   | [WATERMARK]       | event-time completeness bound and the lateness verdict              | `Window.mark`, `Window.verdict`, `Window.Verdict`       |
|  [03]   | [PANE_FOLDS]      | window policy values, pane spread, paned plans, close partition     | `Window.Policy`, `Window.spread/.panes/.close`          |

## [2]-[ASOF_COORDINATE]

- Owner: `AsOf` — the `Schema.Class` owner of `{ stamp: Hlc, ordinal: Refined.OrdinalKey }`: event time plus journal position, totally ordered lexicographically on one lane (`AsOf.Order` composes the `Hlc` order then the ordinal), projected into the engine's product-order coordinate by `AsOf.time` — one composition of kernel vocabulary, zero re-mints, and one decode anchor for any serialized coordinate (a resume token, a scrub bookmark).
- Packages: `effect` (`Schema`, `Data`, `Order`, `Option`, `HashMap`, `Chunk`, `Effect`); `@rasm/ts/kernel` (`Hlc`, `Refined`, `Uncertainty`); `../fold/replay.ts`; `../causal/order.ts`; `../fold/algebra.ts`; `../crdt/merge.ts`.
- Law: REPLAY_LAW at read altitude — `Window.read(handle, asOf)` equals the pure fold of every op at or below the coordinate, because the versioned handle's retained slices are exactly that prefix's emissions; `Window.asOfDiff(handle, from, to)` equals the consolidated signed difference of the two reads — both delegate to `fold/replay`'s lane, so the law is inherited, not re-implemented.
- Law: cross-replica completeness is never an `AsOf` question — the coordinate orders one journal lane; multi-replica frontiers are `Vector` facts folded in `causal/order`, and the two vocabularies meet only at the watermark.
- Growth: a branch axis is a key-space partition (a plan key row), never a fourth coordinate.
- Boundary: `store/project` mints `AsOf` values from journal positions; `ui` history scrubbing consumes `read`/`asOfDiff` through served views.

```typescript
import { Array, Chunk, Data, type Duration, Effect, HashMap, Option, Order, Schema } from "effect"
import { Hlc, Refined, Uncertainty } from "@rasm/ts/kernel"
import { Causal } from "../causal/order.ts"
import { Vector } from "../causal/vector.ts"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "../fold/algebra.ts"
import { Replay } from "../fold/replay.ts"

class AsOf extends Schema.Class<AsOf>("AsOf")({
  stamp: Hlc,
  ordinal: Refined.OrdinalKey,
}) {
  static readonly Order: Order.Order<AsOf> = Order.combine(
    Order.mapInput(Hlc.Order, (asOf: AsOf) => asOf.stamp),
    Order.mapInput(Order.number, (asOf: AsOf) => asOf.ordinal),
  )
  static time(asOf: AsOf): Replay.Time {
    return Replay.time(asOf.stamp, asOf.ordinal)
  }
}

const _read = <Op, K, S>(
  handle: Replay.Versioned<Op, K, S>,
  asOf: AsOf,
): Effect.Effect<Fold.Table<K, S>> => handle.read(AsOf.time(asOf))

const _asOfDiff = <Op, K, S>(
  handle: Replay.Versioned<Op, K, S>,
  from: AsOf,
  to: AsOf,
): Effect.Effect<Fold.Delta<readonly [K, S]>> => handle.diff(AsOf.time(from), AsOf.time(to))
```

## [3]-[WATERMARK]

- Owner: `Window.mark` — the HLC event-time watermark: the meet (minimum) of per-replica last-seen stamps, `Option` because zero acked replicas bound nothing, paired with its `Uncertainty` window so the bound carries the honesty of the clocks that produced it.
- Law: the lateness verdict is `Causal.compare` collapsed — an op wholly before the mark is `late`, wholly after or equal is `punctual`, and an overlap is `uncertain` — so lateness policy (`admit late into a grace pane, count uncertain as open`) reads a three-value vocabulary instead of fabricating precision at the boundary the hardware cannot support.
- Law: the watermark advances monotonically because each replica's last-seen stamp advances and the meet of ascending inputs ascends; a regressed ack is stale evidence the max-merge on the ack table absorbs before the meet ever sees it.
- Boundary: ack collection is the feed owner's concern (`edge/live` sockets, `store/project` journal positions); this module folds whatever ack table arrives.

```typescript
declare namespace Window {
  type Mark = Causal.Stamped
  type Verdict = (typeof _VERDICTS)[number]
  type Policy = Data.TaggedEnum<{
    Tumbling: { readonly width: Duration.Duration }
    Sliding: { readonly width: Duration.Duration; readonly step: Duration.Duration }
    Session: { readonly gap: Duration.Duration }
  }>
  type Pane = { readonly open: bigint; readonly until: bigint }
  type Shape = {
    readonly Policy: Data.TaggedEnum.Constructor<Policy>
    readonly read: <Op, K, S>(handle: Replay.Versioned<Op, K, S>, asOf: AsOf) => Effect.Effect<Fold.Table<K, S>>
    readonly asOfDiff: <Op, K, S>(
      handle: Replay.Versioned<Op, K, S>,
      from: AsOf,
      to: AsOf,
    ) => Effect.Effect<Fold.Delta<readonly [K, S]>>
    readonly mark: (acks: HashMap.HashMap<Vector.Replica, Hlc>, window: Uncertainty) => Option.Option<Mark>
    readonly verdict: (op: Causal.Stamped, mark: Mark) => Verdict
    readonly spread: (policy: Policy) => (stamp: Hlc) => Chunk.Chunk<Pane>
    readonly panes: <Op, K, S>(plan: Fold.Plan<Op, K, S>) => Fold.Plan<readonly [Pane, Op], Data.Data<readonly [bigint, K]>, S>
    readonly close: <K, S>(
      table: Fold.Table<Data.Data<readonly [bigint, K]>, S>,
      policy: Policy,
      mark: Mark,
    ) => readonly [closed: Fold.Table<Data.Data<readonly [bigint, K]>, S>, open: Fold.Table<Data.Data<readonly [bigint, K]>, S>]
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

const _mark = (
  acks: HashMap.HashMap<Vector.Replica, Hlc>,
  window: Uncertainty,
): Option.Option<Window.Mark> =>
  Option.map(
    Merge.fold(_earliest, Array.fromIterable(HashMap.values(acks))),
    (stamp) => ({ stamp, window }),
  )

const _verdict = (op: Causal.Stamped, mark: Window.Mark): Window.Verdict => _VERDICT_BY_ORDER[Causal.compare(op, mark)]
```

## [4]-[PANE_FOLDS]

- Owner: the pane algebra — `Window.Policy` is a closed policy family carried as values; `spread` assigns a stamp to its pane coordinates (one pane tumbling, `width/step` panes sliding, one provisional `gap` pane per op for sessions); `panes` takes only the plan and composes it into a paned plan whose key is the `Data`-tupled `[paneOpen, key]` — window folds are the same algebra under a composite key, not a second fold machine, and pane assignment is the caller's `spread` flat-map, never a second knob on the composer.
- Law: the sliding fan-out happens at spread, before the fold — the caller flat-maps ops across their panes and the plan stays single-key, so the fold algebra's one-key law holds and pane multiplicity is data.
- Law: spread answers only panes that contain the stamp — the sliding candidate ladder filters by containment, so a stride that does not divide the width can never assign an op to a pane it lies outside.
- Law: pane coordinates live on the stamp's own bigint physical plane — spans convert once through the kernel's `Hlc.delta` unit site, so pane math, close comparison, and the composite fold key never re-derive milliseconds or narrow the stamp.
- Law: `close` partitions panes by watermark passage — a pane whose `until` (session: plus gap) lies wholly before the mark's stamp closes; closed panes are final by the same argument as `Causal.finalize` (nothing punctual can still land), and late arrivals route by the verdict, never into a closed pane.
- Law: session coalescing is the merge instance's job — provisional session panes for the same key whose spans overlap fold together when the composite key collapses them at spread grain; a policy needing exact session stitching tightens `spread`, never forks the fold.
- Growth: a new window shape is one `Policy` case plus one `spread` arm — `panes` and `close` absorb it untouched.

```typescript
const _Policy = Data.taggedEnum<Window.Policy>()

const _spread = (policy: Window.Policy) => (stamp: Hlc): Chunk.Chunk<Window.Pane> =>
  _Policy.$match(policy, {
    Tumbling: ({ width }) => {
      const span = Hlc.delta(width)
      const open = (stamp.physical / span) * span
      return Chunk.of({ open, until: open + span })
    },
    Sliding: ({ width, step }) => {
      const span = Hlc.delta(width)
      const stride = Hlc.delta(step)
      const at = stamp.physical
      const last = (at / stride) * stride
      const count = Number((span + stride - 1n) / stride)
      return Chunk.filter(
        Chunk.map(Chunk.range(0, count - 1), (back) => {
          const open = last - BigInt(back) * stride
          return { open, until: open + span }
        }),
        (pane) => pane.open <= at && at < pane.until,
      )
    },
    Session: ({ gap }) => Chunk.of({ open: stamp.physical, until: stamp.physical + Hlc.delta(gap) }),
  })

const _panes = <Op, K, S>(
  plan: Fold.Plan<Op, K, S>,
): Fold.Plan<readonly [Window.Pane, Op], Data.Data<readonly [bigint, K]>, S> =>
  Fold.plan({
    name: `${plan.name}/paned`,
    key: ([pane, op]) => Data.tuple(pane.open, plan.key(op)),
    lift: ([, op]) => plan.lift(op),
    merge: plan.merge,
  })

const _close = <K, S>(
  table: Fold.Table<Data.Data<readonly [bigint, K]>, S>,
  policy: Window.Policy,
  mark: Window.Mark,
): readonly [Fold.Table<Data.Data<readonly [bigint, K]>, S>, Fold.Table<Data.Data<readonly [bigint, K]>, S>] => {
  const slack = _Policy.$match(policy, {
    Tumbling: ({ width }) => Hlc.delta(width),
    Sliding: ({ width }) => Hlc.delta(width),
    Session: ({ gap }) => Hlc.delta(gap),
  })
  const sealed = (key: Data.Data<readonly [bigint, K]>): boolean => key[0] + slack < mark.stamp.physical
  return [
    HashMap.filter(table, (_state, key) => sealed(key)),
    HashMap.filter(table, (_state, key) => !sealed(key)),
  ] as const
}

const Window: Window.Shape = {
  Policy: _Policy,
  read: _read,
  asOfDiff: _asOfDiff,
  mark: _mark,
  verdict: _verdict,
  spread: _spread,
  panes: _panes,
  close: _close,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { AsOf, Window }
```
