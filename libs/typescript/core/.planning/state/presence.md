# [CORE_PRESENCE]

The actor-presence CRDT: the wire-carried op family — `Join` carrying the typed profile and device, `Beat` heartbeats sampling connection quality, `Move` carrying the ephemeral-axes patch (cursor, selection, view, focus, input) as data so every collaborative axis rides ONE op, `Leave` departures — merged per actor into one `Merge.struct` product whose every row is a proven instance: the durable stamps (`joined`/`last`/`gone`) plus one stamped-LWW `Worn` row per axis, so presence converges across feeds and replicas like any lattice and the axis roster grows as product rows, never as op cases. Status is a read-time verdict over a caller-supplied horizon so liveness policy is a value and the fold never reads an ambient clock. A serving edge decodes client frames INTO this family and forwards rosters; the fold below is the only presence authority, one more `fold#PLAN_CONTRACT` plan row every altitude runs unchanged — the browser roster is the fold's in-memory handle, the ordered roster board is the fractional-index lane, and no second presence table exists anywhere. The module is `core/src/state/presence.ts`; a new ephemeral axis is one `Move` patch field plus one product row plus one lift line, a new roster read is one projection member.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                      | [PUBLIC]                                                            |
| :-----: | :--------------- | :------------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `OP_FAMILY`      | the brands, the axis vocabularies, and the wire-carried op union | `Presence.Op`                                                            |
|  [02]   | `STATE_PRODUCT`  | the per-actor axis product instance and the op lift              | `Presence.state`, `Presence.plan`                                        |
|  [03]   | `ROSTER_READS`   | lease policy, status verdicts, the roster and surface projections | `Presence.status`, `Presence.roster`, `Presence.crowd`, `Presence.Lease` |

## [2]-[OP_FAMILY]

[OP_FAMILY]:
- Owner: `Presence.Op` — the wire-carried op family (`Join` with the typed profile and device row, `Beat` heartbeats carrying an optional connection-quality sample, `Move` carrying the ephemeral-axes patch, `Leave` departures), each a tagged struct stamped with `Hlc` and tenant; the interchange codec decodes client frames INTO this family at its own seam, and interior construction composes already-branded parts.
- Law: the actor brand is interior — `ActorId` reaches consumers only as `Presence.Actor`, one spelling for roster keys and op fields; a session or replica identity is `causal`'s `Vector.Replica`, a distinct concept that never unifies with the actor axis; `SurfaceId` is the one view-surface spelling cursor, view, and focus share.
- Law: the profile is typed evidence, never an erased bag — `name`, an optional `hue` render hint, an optional content-addressed `avatar` artifact key — so a roster renders from fields policy reads and no `HashMap<string, string>` escape hatch exists on the wire.
- Law: `Move` is the one ephemeral op — every axis is an `Option`-carried patch field (`cursor`, `selection`, `view`, `focus`, `input`) and any subset travels in one frame, so a client coalesces pointer, selection, and camera churn into single ops and the family never grows an op case per axis; `selection` is the actor's whole selected `HashSet` of `ContentKey`s — the latest statement replaces, never accretes, because a selection is a register of a set, not a set CRDT.
- Growth: a new ephemeral axis is one `Move` patch field; a new join-time fact is one `Join` field; the op roster itself is closed at these four lifecycle cases.
- Packages: `effect` (`Schema`, `Duration`, `HashMap`, `HashSet`, `Match`, `Option`, `Order`); `../value/clock.ts` (`Hlc`); `../value/contentKey.ts` (`ContentKey`); `../value/identity.ts` (`TenantContext`); `./merge.ts` (`Merge`); `./fold.ts` (`Fold`).

```typescript
import { type Duration, HashMap, HashSet, Match, Option, Order, pipe, Schema } from "effect"
import { Hlc } from "../value/clock.ts"
import { ContentKey } from "../value/contentKey.ts"
import { TenantContext } from "../value/identity.ts"
import { Fold } from "./fold.ts"
import { Merge } from "./merge.ts"

const _Actor = Schema.NonEmptyString.pipe(Schema.brand("ActorId"))
const _Surface = Schema.NonEmptyString.pipe(Schema.brand("SurfaceId"))

const _DEVICES = ["desktop", "tablet", "phone", "headset"] as const
const _GRADES = ["solid", "degraded", "flaky"] as const
const _INPUTS = ["idle", "pointing", "typing"] as const

const _Profile = Schema.Struct({
  name: Schema.NonEmptyString,
  hue: Schema.optionalWith(Schema.Int.pipe(Schema.between(0, 360)), { as: "Option" }),
  avatar: Schema.optionalWith(ContentKey, { as: "Option" }),
})

const _Point = Schema.Struct({
  surface: _Surface,
  x: Schema.Number,
  y: Schema.Number,
  z: Schema.optionalWith(Schema.Number, { as: "Option" }), // absent on a sheet, present in a scene: one point shape serves both
})

const _View = Schema.Struct({
  surface: _Surface,
  x: Schema.Number,
  y: Schema.Number,
  zoom: Schema.Number.pipe(Schema.positive()),
})

const _Join = Schema.TaggedStruct("Join", {
  actor: _Actor,
  at: Hlc,
  tenant: TenantContext,
  profile: _Profile,
  device: Schema.Literal(..._DEVICES),
})

const _Beat = Schema.TaggedStruct("Beat", {
  actor: _Actor,
  at: Hlc,
  tenant: TenantContext,
  quality: Schema.optionalWith(Schema.Literal(..._GRADES), { as: "Option" }),
})

const _Move = Schema.TaggedStruct("Move", {
  actor: _Actor,
  at: Hlc,
  tenant: TenantContext,
  cursor: Schema.optionalWith(_Point, { as: "Option" }),
  selection: Schema.optionalWith(Schema.HashSet(ContentKey), { as: "Option" }),
  view: Schema.optionalWith(_View, { as: "Option" }),
  focus: Schema.optionalWith(_Surface, { as: "Option" }),
  input: Schema.optionalWith(Schema.Literal(..._INPUTS), { as: "Option" }),
})

const _Leave = Schema.TaggedStruct("Leave", {
  actor: _Actor,
  at: Hlc,
  tenant: TenantContext,
})

const _Op: Schema.Union<[typeof _Join, typeof _Beat, typeof _Move, typeof _Leave]> = Schema.Union(_Join, _Beat, _Move, _Leave)
```

## [3]-[STATE_PRODUCT]

[STATE_PRODUCT]:
- Owner: `Presence.state` — the per-actor `Merge.struct` product: the durable stamps (`joined` min, `last` max, `gone` optional max) plus one `Worn` axis row apiece for `face`, `device`, `quality`, `cursor`, `selection`, `view`, `focus`, and `input` — every axis one `_worn` generator application (`Merge.optional` over `Merge.max` on the worn stamp), so the eleven-row product's posture derives as the conjunction, the whole instance is convergence-legal by construction, and the axis roster is seed data on one generated instance shape; `Presence.plan` keys by actor, so the presence table is one more replay-maintained fold.
- Law: `Worn<A>` is the one stamped-LWW carrier — a value paired with the `Hlc` that justified it, merged by stamp so an axis cannot drift from its clock; every op contributes `Option.none` on axes it does not speak (the lawful identity under the optional lift), so a `Beat` can never bury a profile, a cursor patch can never bury a selection, and the latest statement per axis survives any delivery order.
- Law: `Leave` is evidence, not deletion — the `gone` stamp coexists with later `Beat`s (a rejoin outruns a stale leave by stamp comparison at read time), so out-of-order delivery cannot resurrect or bury an actor incorrectly.
- Law: batch-atomic roster application — a multi-actor op batch that must land all-or-nothing rides `merge#MERGE_CELLS`'s `Merge.cell(Presence.state)`, the same instance in a transactional table; the fold handles stay the live-view altitude and neither re-declares the other.
- Growth: a new axis is one `State` field, one `_worn` product row, and one lift line — the instance, plan, and every read absorb it untouched.

```typescript
declare namespace Presence {
  type Actor = typeof _Actor.Type
  type Surface = typeof _Surface.Type
  type Op = typeof _Op.Type
  type Device = (typeof _DEVICES)[number]
  type Grade = (typeof _GRADES)[number]
  type Input = (typeof _INPUTS)[number]
  type Profile = typeof _Profile.Type
  type Point = typeof _Point.Type
  type View = typeof _View.Type
  type Worn<A> = { readonly value: A; readonly at: Hlc }
  type State = {
    readonly joined: Hlc
    readonly face: Option.Option<Worn<Profile>>
    readonly device: Option.Option<Worn<Device>>
    readonly quality: Option.Option<Worn<Grade>>
    readonly cursor: Option.Option<Worn<Point>>
    readonly selection: Option.Option<Worn<HashSet.HashSet<ContentKey>>>
    readonly view: Option.Option<Worn<View>>
    readonly focus: Option.Option<Worn<Surface>>
    readonly input: Option.Option<Worn<Input>>
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
    readonly crowd: (
      table: Fold.Table<Actor, State>,
      horizon: Hlc,
      lease: Lease,
    ) => HashMap.HashMap<Surface, HashSet.HashSet<Actor>>
  }
}

const _STATUS = ["live", "idle", "gone"] as const

const _byWorn = <A>(): Order.Order<Presence.Worn<A>> =>
  Order.mapInput(Hlc.Order, (worn: Presence.Worn<A>) => worn.at)

const _worn = <A>(): Merge.Instance<Option.Option<Presence.Worn<A>>> => Merge.optional(Merge.max(_byWorn<A>()))

const _state: Merge.Instance<Presence.State> = Merge.struct({
  joined: Merge.min(Hlc.Order),
  face: _worn<Presence.Profile>(),
  device: _worn<Presence.Device>(),
  quality: _worn<Presence.Grade>(),
  cursor: _worn<Presence.Point>(),
  selection: _worn<HashSet.HashSet<ContentKey>>(),
  view: _worn<Presence.View>(),
  focus: _worn<Presence.Surface>(),
  input: _worn<Presence.Input>(),
  last: Merge.max(Hlc.Order),
  gone: Merge.optional(Merge.max(Hlc.Order)),
})

const _dress = <A>(at: Hlc) => (value: A): Presence.Worn<A> => ({ value, at })

const _silent = (at: Hlc): Presence.State => ({
  joined: at,
  face: Option.none(),
  device: Option.none(),
  quality: Option.none(),
  cursor: Option.none(),
  selection: Option.none(),
  view: Option.none(),
  focus: Option.none(),
  input: Option.none(),
  last: at,
  gone: Option.none(),
})

const _lifted: (op: Presence.Op) => Presence.State = pipe(
  Match.type<Presence.Op>(),
  Match.tagsExhaustive({
    Join: (op) => ({
      ..._silent(op.at),
      face: Option.some(_dress<Presence.Profile>(op.at)(op.profile)),
      device: Option.some(_dress<Presence.Device>(op.at)(op.device)),
    }),
    Beat: (op) => ({
      ..._silent(op.at),
      quality: Option.map(op.quality, _dress<Presence.Grade>(op.at)),
    }),
    Move: (op) => ({
      ..._silent(op.at),
      cursor: Option.map(op.cursor, _dress<Presence.Point>(op.at)),
      selection: Option.map(op.selection, _dress<HashSet.HashSet<ContentKey>>(op.at)),
      view: Option.map(op.view, _dress<Presence.View>(op.at)),
      focus: Option.map(op.focus, _dress<Presence.Surface>(op.at)),
      input: Option.map(op.input, _dress<Presence.Input>(op.at)),
    }),
    Leave: (op) => ({
      ..._silent(op.at),
      gone: Option.some(op.at),
    }),
  }),
)
```

## [4]-[ROSTER_READS]

[ROSTER_READS]:
- Owner: the read family — `status` folds one actor's state against a horizon and a `Lease` policy row (`idle` and `gone` windows as `Duration` values); `roster` maps the verdict across the folded table; `crowd` groups the non-`gone` actors by their sighting surface, the per-surface census a view header binds without walking raw states.
- Law: status is three-valued and read-time — `gone` when a leave stamp is at or after the last activity AND the horizon has outrun it by the lease's gone window (the departure grace that absorbs in-flight beats), `idle` when the horizon outruns the last stamp by the idle window, `live` otherwise — the ephemeral axes never enter the verdict, so liveness stays a stamp question; expiry never mutates state: a sweep is the consumer re-reading with a fresh horizon, so no timer fiber lives in this module; distances measure through `Hlc.delta`, never a millisecond re-derivation.
- Law: the sighting surface reads cursor-first, focus-fallback — an actor pointing on a sheet counts there even while a panel holds keyboard focus, and an actor with neither axis worn is unsighted and absent from every census row.
- Law: the ordered roster board — actors ranked by recency or name for a live list — is `fold#MEMORY_LANE`'s `Replay.ordered` over `Presence.plan` with an `Order` on the state, never a re-sort of the roster projection.
- Boundary: a serving edge forwards decoded ops and serves rosters; heartbeat cadence and socket lifecycle are its policy — this module owns only the fold and its verdicts.

```typescript
const _noEarlier = Order.greaterThanOrEqualTo(Hlc.Order)

const _idled = (state: Presence.State, horizon: Hlc, lease: Presence.Lease): Presence.Status =>
  horizon.physical - state.last.physical > Hlc.delta(lease.idle) ? "idle" : "live"

const _status = (state: Presence.State, horizon: Hlc, lease: Presence.Lease): Presence.Status =>
  Option.match(state.gone, {
    onSome: (gone) =>
      _noEarlier(gone, state.last) && horizon.physical - gone.physical > Hlc.delta(lease.gone)
        ? "gone"
        : _idled(state, horizon, lease),
    onNone: () => _idled(state, horizon, lease),
  })

const _sighted = (state: Presence.State): Option.Option<Presence.Surface> =>
  Option.orElse(
    Option.map(state.cursor, (worn) => worn.value.surface),
    () => Option.map(state.focus, (worn) => worn.value),
  )

const Presence: Presence.Shape = {
  Op: _Op,
  state: _state,
  plan: Fold.plan({
    name: "state/presence",
    key: (op) => op.actor,
    lift: _lifted,
    merge: _state,
  }),
  status: _status,
  roster: (table, horizon, lease) => HashMap.map(table, (state) => _status(state, horizon, lease)),
  crowd: (table, horizon, lease) =>
    HashMap.reduce(table, HashMap.empty<Presence.Surface, HashSet.HashSet<Presence.Actor>>(), (acc, state, actor) =>
      _status(state, horizon, lease) === "gone"
        ? acc
        : Option.match(_sighted(state), {
            onNone: () => acc,
            onSome: (surface) =>
              HashMap.modifyAt(acc, surface, (slot) =>
                Option.some(HashSet.add(Option.getOrElse(slot, () => HashSet.empty<Presence.Actor>()), actor))),
          })),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Presence }
```
