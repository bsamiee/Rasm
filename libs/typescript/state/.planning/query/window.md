# [STATE_WINDOW]

`query/window.ts` owns the windowed read surface and carries the REPLAY_LAW spine to consumers: `AsOf` — the three-coordinate read time (the two `Hlc` halves plus the journal ordinal) that projects into `Replay.Time` — with `read` and `asOfDiff` delegating to the versioned handle's retained lane, so a time-travel read IS the fold of the prefix at that coordinate and a diff IS the consolidated signed difference between two reads, never a second bookkeeping structure. Event-time completeness is the `Watermark`: the meet of per-replica last-seen stamps paired with the kernel uncertainty window, whose lateness verdict is `Causal.compare`'s honest four-way answer collapsed to `punctual`/`late`/`uncertain`. Window folds are policy values — tumbling, sliding, session — spread ops into pane coordinates that compose any `Fold.Plan` into a paned plan, and panes close by watermark passage, never by wall-clock timers.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                            | [SURFACE]                                          |
| :-----: | :---------------- | :------------------------------------------------------------------ | :----------------------------------------------------- |
|  [01]   | [ASOF_COORDINATE] | the three-coordinate read time, its order, the versioned-lane reads | `AsOf`, `AsOf.Order/.time`, `Window.read`, `Window.asOfDiff` |
|  [02]   | [WATERMARK]       | event-time completeness bound and the lateness verdict              | `Window.mark`, `Window.verdict`, `Window.Verdict`       |
|  [03]   | [PANE_FOLDS]      | window policy values, pane spread, paned plans, close partition     | `Window.Policy`, `Window.spread/.panes/.close`          |

## [2]-[ASOF_COORDINATE]

- Owner: `AsOf` — `{ stamp: Hlc, ordinal: OrdinalKey }`: event time plus journal position, totally ordered lexicographically on one lane (`AsOf.Order` composes the `Hlc` order then the ordinal), projected into the engine's product-order coordinate by `AsOf.time` — one composition of kernel vocabulary, zero re-mints.
- Packages: `effect` (`Data`, `Order`, `Option`, `HashMap`, `Chunk`, `Duration`, `Effect`); `@rasm/ts/kernel` (`Hlc`, `OrdinalKey`, `Uncertainty`); `../fold/replay.ts`; `../causal/order.ts`; `../fold/algebra.ts`; `../crdt/merge.ts`.
- Law: REPLAY_LAW at read altitude — `Window.read(handle, asOf)` equals the pure fold of every op at or below the coordinate, because the versioned handle's retained slices are exactly that prefix's emissions; `Window.asOfDiff(handle, from, to)` equals the consolidated signed difference of the two reads — both delegate to `fold/replay`'s lane, so the law is inherited, not re-implemented.
- Law: cross-replica completeness is never an `AsOf` question — the coordinate orders one journal lane; multi-replica frontiers are `Vector` facts folded in `causal/order`, and the two vocabularies meet only at the watermark.
- Growth: a branch axis is a key-space partition (a plan key row), never a fourth coordinate.
- Boundary: `store/project` mints `AsOf` values from journal positions; `ui` history scrubbing consumes `read`/`asOfDiff` through served views.

```typescript
import { Array, Chunk, Data, Duration, Effect, HashMap, Option, Order } from "effect"
import { Hlc, OrdinalKey, Uncertainty } from "@rasm/ts/kernel"
import { Causal } from "../causal/order.ts"
import { Vector } from "../causal/vector.ts"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "../fold/algebra.ts"
import { Replay } from "../fold/replay.ts"

class AsOf extends Data.Class<{ readonly stamp: Hlc; readonly ordinal: OrdinalKey }> {
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
  type Pane = { readonly open: number; readonly until: number }
  type Shape = {
    readonly Policy: Data.TaggedEnum.Constructor<Policy>
    readonly read: typeof _read
    readonly asOfDiff: typeof _asOfDiff
    readonly mark: (acks: HashMap.HashMap<Vector.Replica, Hlc>, window: Uncertainty) => Option.Option<Mark>
    readonly verdict: (op: Causal.Stamped, mark: Mark) => Verdict
    readonly spread: (policy: Policy) => (stamp: Hlc) => Chunk.Chunk<Pane>
    readonly panes: <Op, K, S>(
      policy: Policy,
      plan: Fold.Plan<Op, K, S>,
      stamp: (op: Op) => Hlc,
    ) => Fold.Plan<readonly [Pane, Op], Data.Data<readonly [number, K]>, S>
    readonly close: <K, S>(
      table: Fold.Table<Data.Data<readonly [number, K]>, S>,
      policy: Policy,
      mark: Mark,
    ) => readonly [closed: Fold.Table<Data.Data<readonly [number, K]>, S>, open: Fold.Table<Data.Data<readonly [number, K]>, S>]
  }
}

const _VERDICTS = ["punctual", "late", "uncertain"] as const

const _mark = (
  acks: HashMap.HashMap<Vector.Replica, Hlc>,
  window: Uncertainty,
): Option.Option<Window.Mark> =>
  Option.map(
    Merge.fold(Merge.min(Hlc.Order), Array.fromIterable(HashMap.values(acks))),
    (stamp) => ({ stamp, window }),
  )

const _verdict = (op: Causal.Stamped, mark: Window.Mark): Window.Verdict =>
  Causal.compare(op, mark) === "before"
    ? "late"
    : Causal.compare(op, mark) === "concurrent"
      ? "uncertain"
      : "punctual"
```

## [4]-[PANE_FOLDS]

- Owner: the pane algebra — `Window.Policy` is a closed policy family carried as values; `spread` assigns a stamp to its pane coordinates (one pane tumbling, `width/step` panes sliding, one provisional `gap` pane per op for sessions); `panes` composes ANY `Fold.Plan` into a paned plan whose key is the `Data`-tupled `[paneOpen, key]` — window folds are the same algebra under a composite key, not a second fold machine.
- Law: the sliding fan-out happens at spread, before the fold — the caller flat-maps ops across their panes and the plan stays single-key, so the fold algebra's one-key law holds and pane multiplicity is data.
- Law: `close` partitions panes by watermark passage — a pane whose `until` (session: plus gap) lies wholly before the mark's stamp closes; closed panes are final by the same argument as `Causal.finalize` (nothing punctual can still land), and late arrivals route by the verdict, never into a closed pane.
- Law: session coalescing is the merge instance's job — provisional session panes for the same key whose spans overlap fold together when the composite key collapses them at spread grain; a policy needing exact session stitching tightens `spread`, never forks the fold.
- Growth: a new window shape is one `Policy` case plus one `spread` arm — `panes` and `close` absorb it untouched.

```typescript
const _Policy = Data.taggedEnum<Window.Policy>()

const _spread = (policy: Window.Policy) => (stamp: Hlc): Chunk.Chunk<Window.Pane> =>
  _Policy.$match(policy, {
    Tumbling: ({ width }) => {
      const span = Duration.toMillis(width)
      const open = Math.floor(Hlc.physical(stamp) / span) * span
      return Chunk.of({ open, until: open + span })
    },
    Sliding: ({ width, step }) => {
      const span = Duration.toMillis(width)
      const stride = Duration.toMillis(step)
      const last = Math.floor(Hlc.physical(stamp) / stride) * stride
      const count = Math.max(1, Math.ceil(span / stride))
      return Chunk.map(Chunk.range(0, count - 1), (back) => ({
        open: last - back * stride,
        until: last - back * stride + span,
      }))
    },
    Session: ({ gap }) => {
      const at = Hlc.physical(stamp)
      return Chunk.of({ open: at, until: at + Duration.toMillis(gap) })
    },
  })

const _panes = <Op, K, S>(
  policy: Window.Policy,
  plan: Fold.Plan<Op, K, S>,
  stamp: (op: Op) => Hlc,
): Fold.Plan<readonly [Window.Pane, Op], Data.Data<readonly [number, K]>, S> =>
  Fold.plan({
    name: `${plan.name}/paned`,
    key: ([pane, op]) => Data.tuple(pane.open, plan.key(op)),
    lift: ([, op]) => plan.lift(op),
    merge: plan.merge,
  })

const _close = <K, S>(
  table: Fold.Table<Data.Data<readonly [number, K]>, S>,
  policy: Window.Policy,
  mark: Window.Mark,
): readonly [Fold.Table<Data.Data<readonly [number, K]>, S>, Fold.Table<Data.Data<readonly [number, K]>, S>] => {
  const slack = _Policy.$match(policy, {
    Tumbling: ({ width }) => Duration.toMillis(width),
    Sliding: ({ width }) => Duration.toMillis(width),
    Session: ({ gap }) => Duration.toMillis(gap),
  })
  const sealed = HashMap.filter(table, (_state, key) => key[0] + slack < Hlc.physical(mark.stamp))
  return [sealed, HashMap.filter(table, (_state, key) => !(key[0] + slack < Hlc.physical(mark.stamp)))] as const
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
