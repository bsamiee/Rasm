# [STATE_LIVE]

`query/live.ts` owns the live read surface: `Live` — the lens family that projects a replay handle into `Subscribable` views (whole table, keyed row, ordered board) — and `Presence` — the actor-presence semantics `edge/live` serves over its sockets. A live query is a projection of a maintained fold, never a re-run: the handle's `SubscriptionRef` state is the one source, `Subscribable.map` derives every lens, and ordered boards ride `Replay.ordered`'s fractional-index lane so a churning list never re-sorts. Presence is one more fold plan — join/beat/leave ops merged per actor under a `Merge.struct` product — with status a read-time verdict over a caller-supplied horizon, so liveness policy is a value and the fold never reads an ambient clock.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                       | [SURFACE]                                       |
| :-----: | :--------------- | :-------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | [VIEW_LENSES]    | table, row, and board projections over replay handles           | `Live.view`, `Live.feed`                              |
|  [02]   | [PRESENCE_OPS]   | the presence op family and per-actor state product              | `Presence.Op` (`Join`/`Beat`/`Leave`), `Presence.state` |
|  [03]   | [PRESENCE_READS] | the fold plan, status verdicts, roster read, lease policy       | `Presence.plan/.status/.roster`, `Presence.Lease`      |

## [2]-[VIEW_LENSES]

- Owner: `Live.view` — one entrypoint over the view modalities, discriminated on the input shape: a memory handle alone yields the table view; a handle with a key yields the row view (`Option`-carried absence); an ordered handle yields the board view — three overload signatures, one declaration, no `viewAll`/`viewByKey` siblings.
- Packages: `effect` (`Subscribable`, `Stream`, `HashMap`, `Option`, `Chunk`); `../fold/algebra.ts`; `../fold/replay.ts`.
- Law: every lens is `Subscribable.map` over the handle's state — get and changes stay coherent because they derive from one projection; a lens that re-runs the fold, caches its own copy, or subscribes twice is the re-derivation defect.
- Law: `Live.feed` flattens the handle's change wave into a per-row `Stream` — the delivery feed `edge/live` frames onto its socket rows — and the wave replays its latest batch to a late subscriber, the stated warm-up semantic of the `SubscriptionRef` spine.
- Boundary: `edge/live/socket` serves these `Subscribable`s and streams; `ui` binds them through its atom bridge; neither reaches the graph or the plan.
- Growth: a new lens modality is one overload line plus one projection arm.

```typescript
import { Chunk, Duration, HashMap, Match, Option, Order, Predicate, Schema, Stream, Subscribable, pipe } from "effect"
import { Hlc, TenantContext } from "@rasm/ts/kernel"
import { Merge } from "../crdt/merge.ts"
import { Fold } from "../fold/algebra.ts"
import { Replay } from "../fold/replay.ts"

function view<Op, K, S>(handle: Replay.Memory<Op, K, S>): Subscribable.Subscribable<Fold.Table<K, S>>
function view<Op, K, S>(handle: Replay.Memory<Op, K, S>, key: K): Subscribable.Subscribable<Option.Option<S>>
function view<Op, K, S>(handle: Replay.Ordered<Op, K, S>): Subscribable.Subscribable<Chunk.Chunk<readonly [K, S]>>
function view<Op, K, S>(
  handle: Replay.Memory<Op, K, S> | Replay.Ordered<Op, K, S>,
  key?: K,
): Subscribable.Subscribable<Fold.Table<K, S> | Option.Option<S> | Chunk.Chunk<readonly [K, S]>> {
  return Predicate.hasProperty(handle, "ranks")
    ? handle.ranks
    : key === undefined
      ? handle.state
      : Subscribable.map(handle.state, (table) => HashMap.get(table, key))
}

const _feed = <Op, K, S>(handle: Replay.Memory<Op, K, S>): Stream.Stream<Fold.Change<K, S>> =>
  Stream.flattenIterables(handle.wave.changes)

const Live: {
  readonly view: typeof view
  readonly feed: <Op, K, S>(handle: Replay.Memory<Op, K, S>) => Stream.Stream<Fold.Change<K, S>>
} = { view, feed: _feed }
```

## [3]-[PRESENCE_OPS]

- Owner: `Presence.Op` — the wire-carried op family (`Join` with actor metadata, `Beat` heartbeats, `Leave` departures), each a tagged struct stamped with `Hlc` and tenant; `edge/live` decodes client frames INTO this family, and the fold below is the only presence authority.
- Law: per-actor state is a `Merge.struct` product — `joined` min-stamp, `face` (metadata paired with its stamp, merged by stamped LWW), `last` max-stamp, `gone` optional max-stamp — every row a proven instance, so presence converges across feeds and replicas like any lattice.
- Law: `Leave` is evidence, not deletion — the `gone` stamp coexists with later `Beat`s (a rejoin outruns a stale leave by stamp comparison at read time), so out-of-order delivery cannot resurrect or bury an actor incorrectly.
- Growth: a new presence op is one tagged case plus one lift arm; a new state axis is one field plus one product row.

```typescript
const _Actor = Schema.NonEmptyString.pipe(Schema.brand("ActorId"))

const _Join = Schema.TaggedStruct("Join", {
  actor: _Actor,
  at: Hlc,
  tenant: TenantContext,
  meta: Schema.HashMap({ key: Schema.String, value: Schema.String }),
})

const _Beat = Schema.TaggedStruct("Beat", {
  actor: _Actor,
  at: Hlc,
  tenant: TenantContext,
})

const _Leave = Schema.TaggedStruct("Leave", {
  actor: _Actor,
  at: Hlc,
  tenant: TenantContext,
})

const _Op: Schema.Union<[typeof _Join, typeof _Beat, typeof _Leave]> = Schema.Union(_Join, _Beat, _Leave)

declare namespace Presence {
  type Actor = typeof _Actor.Type
  type Op = typeof _Op.Type
  type Face = { readonly meta: HashMap.HashMap<string, string>; readonly at: Hlc }
  type State = {
    readonly joined: Hlc
    readonly face: Face
    readonly last: Hlc
    readonly gone: Option.Option<Hlc>
  }
  type Status = (typeof _STATUS)[number]
  type Lease = { readonly idle: Duration.Duration; readonly gone: Duration.Duration }
  type Shape = {
    readonly Op: typeof _Op
    readonly state: Merge.Instance<State>
    readonly plan: Fold.Plan<Op, Actor, State>
    readonly status: (state: State, horizon: Hlc, lease: Lease) => Status
    readonly roster: (
      table: Fold.Table<Actor, State>,
      horizon: Hlc,
      lease: Lease,
    ) => HashMap.HashMap<Actor, Status>
  }
}

const _STATUS = ["live", "idle", "gone"] as const

const _byFaceStamp: Order.Order<Presence.Face> = Order.mapInput(Hlc.Order, (face: Presence.Face) => face.at)

const _state: Merge.Instance<Presence.State> = Merge.struct({
  joined: Merge.min(Hlc.Order),
  face: Merge.max(_byFaceStamp),
  last: Merge.max(Hlc.Order),
  gone: Merge.optional(Merge.max(Hlc.Order)),
})

const _lifted: (op: Presence.Op) => Presence.State = pipe(
  Match.type<Presence.Op>(),
  Match.tagsExhaustive({
    Join: (op) => ({
      joined: op.at,
      face: { meta: op.meta, at: op.at },
      last: op.at,
      gone: Option.none(),
    }),
    Beat: (op) => ({
      joined: op.at,
      face: { meta: HashMap.empty<string, string>(), at: op.at },
      last: op.at,
      gone: Option.none(),
    }),
    Leave: (op) => ({
      joined: op.at,
      face: { meta: HashMap.empty<string, string>(), at: op.at },
      last: op.at,
      gone: Option.some(op.at),
    }),
  }),
)
```

## [4]-[PRESENCE_READS]

- Owner: the read family — `status` folds one actor's state against a horizon and a `Lease` policy row (`idle` and `gone` windows as `Duration` values); `roster` maps the verdict across the folded table; the plan keys by actor so the presence table is one more replay-maintained fold.
- Law: status is three-valued and read-time — `gone` when a leave stamp is at or after the last activity, `idle` when the horizon outruns the last stamp by the idle window, `live` otherwise — and expiry never mutates state: a sweep is the consumer re-reading with a fresh horizon, so no timer fiber lives in this module.
- Law: the `Beat` lift's empty face never overwrites richer metadata — `face` merges by stamped LWW and a beat carries its stamp with empty metadata only as the lift's neutral contribution; the stamped pair keeps the latest `Join` metadata until a newer `Join` replaces it.
- Boundary: `edge/live/presence` serves rosters and forwards decoded ops; heartbeat cadence and socket lifecycle are edge policy — this module owns only the fold and its verdicts.

```typescript
const _status = (state: Presence.State, horizon: Hlc, lease: Presence.Lease): Presence.Status =>
  Option.match(state.gone, {
    onSome: (gone) =>
      Hlc.physical(gone) >= Hlc.physical(state.last)
        && Hlc.physical(horizon) - Hlc.physical(gone) > Duration.toMillis(lease.gone)
        ? ("gone" as const)
        : Hlc.physical(horizon) - Hlc.physical(state.last) > Duration.toMillis(lease.idle)
          ? ("idle" as const)
          : ("live" as const),
    onNone: () =>
      Hlc.physical(horizon) - Hlc.physical(state.last) > Duration.toMillis(lease.idle)
        ? ("idle" as const)
        : ("live" as const),
  })

const Presence: Presence.Shape = {
  Op: _Op,
  state: _state,
  plan: Fold.plan({
    name: "query/presence",
    key: (op) => op.actor,
    lift: _lifted,
    merge: _state,
  }),
  status: _status,
  roster: (table, horizon, lease) => HashMap.map(table, (state) => _status(state, horizon, lease)),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Live, Presence }
```
