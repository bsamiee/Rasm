# [CORE_PRESENCE]

`Presence` owns the actor-presence CRDT: the wire-carried op family — `Join` carrying the typed profile and device, `Beat` heartbeats sampling connection quality, `Move` carrying the ephemeral-axes patch (cursor, caret, selection, view, focus, input) as data so every collaborative axis rides ONE op, `Leave` departures — merged per actor into one `Merge.struct` product whose every row is a proven instance: the durable stamps (`joined`/`last`/`gone`) and one stamped-LWW `Worn` row per axis, and the ephemeral-axis roster is ONE `_AXES` schema-row anchor from which the `Move` patch fields, the product rows, and the op lift all derive — presence converges across feeds and replicas like any lattice and a new axis is one anchor row, never three synchronized edits and never an op case. Status is a read-time verdict over a caller-supplied horizon so liveness policy is a value and the fold never reads an ambient clock. Caret carries an editor's EPHEMERAL text selection in the surface's own document coordinates — durable anchors are consumer data, so core stays app-blind to every document model behind the two integers. Serving edges decode client frames INTO this family and forward rosters; the fold below is the only presence authority, one more `fold#PLAN_CONTRACT` plan row every altitude runs unchanged — the browser roster is the fold's in-memory handle, the ordered roster board is the fractional-index lane, and no second presence table exists. `core/src/state/presence.ts` holds the module, and a new roster read is one projection member.

## [01]-[INDEX]

- [02]-[OP_FAMILY]: brands, axis vocabularies, and the wire-carried op union; `Presence.Op`.
- [03]-[STATE_PRODUCT]: per-actor axis product instance and the op lift; `Presence.state`, `Presence.plan`.
- [04]-[ROSTER_READS]: lease policy, status verdicts, roster and surface projections; `Presence.status/roster/crowd`, `Presence.Lease`.

## [02]-[OP_FAMILY]

[OP_FAMILY]:

```typescript signature
import { Array, Data, Duration, Equal, HashMap, HashSet, Option, Order, pipe, Record, Schema, type Types } from "effect"
import { Clock } from "../value/clock.ts"
import { Digest } from "../value/contentKey.ts"
import { Identity } from "../value/identity.ts"
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
  avatar: Schema.optionalWith(Digest.Key.content, { as: "Option" }),
})

const _Point2 = Schema.TaggedStruct("Sheet", {
  surface: _Surface,
  x: Schema.Number.pipe(Schema.finite()),
  y: Schema.Number.pipe(Schema.finite()),
})

const _Point3 = Schema.TaggedStruct("Scene", {
  surface: _Surface,
  x: Schema.Number.pipe(Schema.finite()),
  y: Schema.Number.pipe(Schema.finite()),
  z: Schema.Number.pipe(Schema.finite()),
})
const _Point = Schema.Union(_Point2, _Point3)

// document positions in the surface's own coordinate space; anchor and head are independent, so a backward
// selection is two integers, never a normalized pair a consumer re-derives direction from
const _Caret = Schema.Struct({
  surface: _Surface,
  anchor: Schema.Int.pipe(Schema.nonNegative()),
  head: Schema.Int.pipe(Schema.nonNegative()),
})

const _View2 = Schema.TaggedStruct("Sheet", {
  surface: _Surface,
  x: Schema.Number.pipe(Schema.finite()),
  y: Schema.Number.pipe(Schema.finite()),
  zoom: Schema.Number.pipe(Schema.finite(), Schema.positive()),
})

const _Vector3 = Schema.Struct({
  x: Schema.Number.pipe(Schema.finite()),
  y: Schema.Number.pipe(Schema.finite()),
  z: Schema.Number.pipe(Schema.finite()),
})
const _PosePolicy = Schema.Struct({
  normalizationTolerance: Schema.Number.pipe(Schema.positive(), Schema.finite()),
})
const _POSE_POLICY = Schema.decodeSync(_PosePolicy)({ normalizationTolerance: 1e-6 })
const _Orientation = Schema.Struct({
  x: Schema.Number.pipe(Schema.finite()),
  y: Schema.Number.pipe(Schema.finite()),
  z: Schema.Number.pipe(Schema.finite()),
  w: Schema.Number.pipe(Schema.finite()),
}).pipe(Schema.filter((orientation) => {
  const norm = Math.hypot(orientation.x, orientation.y, orientation.z, orientation.w)
  return Math.abs(norm - 1) <= _POSE_POLICY.normalizationTolerance
}))
const _Perspective = Schema.TaggedStruct("Perspective", {
  verticalFov: Schema.Number.pipe(Schema.finite(), Schema.between(Number.EPSILON, Math.PI - Number.EPSILON)),
  near: Schema.Number.pipe(Schema.finite(), Schema.positive()),
  far: Schema.Number.pipe(Schema.finite(), Schema.positive()),
}).pipe(Schema.filter((projection) => projection.near < projection.far))
const _Orthographic = Schema.TaggedStruct("Orthographic", {
  height: Schema.Number.pipe(Schema.finite(), Schema.positive()),
  near: Schema.Number.pipe(Schema.finite()),
  far: Schema.Number.pipe(Schema.finite()),
}).pipe(Schema.filter((projection) => projection.near < projection.far))
const _Projection = Schema.Union(_Perspective, _Orthographic)
const _Frame = Schema.NonEmptyString.pipe(Schema.brand("PresenceFrame"))
const _PoseBase = { position: _Vector3, orientation: _Orientation, projection: _Projection, frame: _Frame } as const
const _Pose = Schema.Union(
  Schema.TaggedStruct("Model", _PoseBase),
  Schema.TaggedStruct("Headset", _PoseBase),
)
const _View3 = Schema.TaggedStruct("Scene", {
  surface: _Surface,
  pose: _Pose,
})
const _View = Schema.Union(_View2, _View3)

const _Join = Schema.TaggedStruct("Join", {
  actor: _Actor,
  at: Clock.Hlc,
  tenant: Identity.Tenant,
  profile: _Profile,
  device: Schema.Literal(..._DEVICES),
})

const _Beat = Schema.TaggedStruct("Beat", {
  actor: _Actor,
  at: Clock.Hlc,
  tenant: Identity.Tenant,
  quality: Schema.optionalWith(Schema.Literal(..._GRADES), { as: "Option" }),
})

const _AXES = {
  cursor: _Point,
  caret: _Caret,
  selection: Schema.HashSet(Digest.Key.content),
  view: _View,
  focus: _Surface,
  input: Schema.Literal(..._INPUTS),
} as const

type _Axes = typeof _AXES
type _Axis = keyof _Axes

const _patch: { readonly [K in _Axis]: Schema.optionalWith<_Axes[K], { as: "Option" }> } = Record.map(_AXES, (axis) =>
  Schema.optionalWith(axis, { as: "Option" })) as { readonly [K in _Axis]: Schema.optionalWith<_Axes[K], { as: "Option" }> }

const _Move = Schema.TaggedStruct("Move", {
  actor: _Actor,
  at: Clock.Hlc,
  tenant: Identity.Tenant,
  ..._patch,
}).pipe(Schema.filter((move) => Record.some(_AXES, (_axis, key) => Option.isSome(move[key]))))

const _Leave = Schema.TaggedStruct("Leave", {
  actor: _Actor,
  at: Clock.Hlc,
  tenant: Identity.Tenant,
})

const _Op: Schema.Union<[typeof _Join, typeof _Beat, typeof _Move, typeof _Leave]> = Schema.Union(_Join, _Beat, _Move, _Leave)

const _OpCase = Data.taggedEnum<Presence.Op>()

const _none = Fold.cell(["none"])
const _optional = <A>(value: Option.Option<A>, cell: (held: A) => Fold.Cell): Fold.Cell =>
  Option.match(value, { onNone: () => _none, onSome: (held) => Fold.cell(["some", cell(held)]) })
const _pointCell = (point: Presence.Point): Fold.Cell =>
  point._tag === "Sheet"
    ? Fold.cell([point._tag, point.surface, point.x, point.y])
    : Fold.cell([point._tag, point.surface, point.x, point.y, point.z])
const _caretCell = (caret: Presence.Caret): Fold.Cell => Fold.cell([caret.surface, caret.anchor, caret.head])
const _vectorCell = (vector: Schema.Schema.Type<typeof _Vector3>): Fold.Cell => Fold.cell([vector.x, vector.y, vector.z])
const _orientationCell = (orientation: Schema.Schema.Type<typeof _Orientation>): Fold.Cell =>
  Fold.cell([orientation.x, orientation.y, orientation.z, orientation.w])
const _projectionCell = (projection: Schema.Schema.Type<typeof _Projection>): Fold.Cell =>
  projection._tag === "Perspective"
    ? Fold.cell([projection._tag, projection.verticalFov, projection.near, projection.far])
    : Fold.cell([projection._tag, projection.height, projection.near, projection.far])
const _poseCell = (pose: Presence.Pose): Fold.Cell => Fold.cell([
  pose._tag,
  pose.frame,
  _vectorCell(pose.position),
  _orientationCell(pose.orientation),
  _projectionCell(pose.projection),
])
const _viewCell = (view: Presence.View): Fold.Cell =>
  view._tag === "Sheet"
    ? Fold.cell([view._tag, view.surface, view.x, view.y, view.zoom])
    : Fold.cell([view._tag, view.surface, _poseCell(view.pose)])
const _profileCell = (profile: Presence.Profile): Fold.Cell => Fold.cell([
  profile.name,
  _optional(profile.hue, (hue) => Fold.cell([hue])),
  _optional(profile.avatar, (avatar) => Fold.cell([avatar])),
])
const _opCell: (op: Presence.Op) => Fold.Cell = _OpCase.$match({
  Join: (op) => Fold.cell([
    op._tag, op.tenant.scope, op.actor, op.at.physical, op.at.logical, _profileCell(op.profile), op.device,
  ]),
  Beat: (op) => Fold.cell([
    op._tag, op.tenant.scope, op.actor, op.at.physical, op.at.logical,
    _optional(op.quality, (quality) => Fold.cell([quality])),
  ]),
  Move: (op) => Fold.cell([
    op._tag,
    op.tenant.scope,
    op.actor,
    op.at.physical,
    op.at.logical,
    _optional(op.cursor, _pointCell),
    _optional(op.caret, _caretCell),
    _optional(op.selection, (selection) =>
      Fold.cell(["selection", ...Array.sort(Array.fromIterable(selection), Order.string)])),
    _optional(op.view, _viewCell),
    _optional(op.focus, (focus) => Fold.cell([focus])),
    _optional(op.input, (input) => Fold.cell([input])),
  ]),
  Leave: (op) => Fold.cell([op._tag, op.tenant.scope, op.actor, op.at.physical, op.at.logical]),
})
```

## [03]-[STATE_PRODUCT]

[STATE_PRODUCT]:

```typescript signature
declare namespace Presence {
  type Actor = Schema.Schema.Type<typeof _Actor>
  type Surface = Schema.Schema.Type<typeof _Surface>
  type Key = readonly [Identity.Tenant.Scope, Actor]
  type Op = Schema.Schema.Type<typeof _Op>
  type Device = (typeof _DEVICES)[number]
  type Grade = (typeof _GRADES)[number]
  type Input = (typeof _INPUTS)[number]
  type Profile = Schema.Schema.Type<typeof _Profile>
  type Point = Schema.Schema.Type<typeof _Point>
  type Caret = Schema.Schema.Type<typeof _Caret>
  type Pose = Schema.Schema.Type<typeof _Pose>
  type View = Schema.Schema.Type<typeof _View>
  type Worn<A> = { readonly value: A; readonly at: Clock.Hlc; readonly tie: string }
  type State = Types.Simplify<
    & {
      readonly joined: Option.Option<Clock.Hlc>
      readonly face: Option.Option<Worn<Profile>>
      readonly device: Option.Option<Worn<Device>>
      readonly quality: Option.Option<Worn<Grade>>
      readonly last: Clock.Hlc
      readonly gone: Option.Option<Clock.Hlc>
    }
    & { readonly [K in _Axis]: Option.Option<Worn<Schema.Schema.Type<_Axes[K]>>> }
  >
  type Status = (typeof _STATUS)[number]
  type Lease = _Lease
  type Shape = {
    readonly Op: typeof _Op
    readonly state: Merge.Instance<State>
    readonly plan: Fold.Plan<Op, Key, State>
    readonly Lease: typeof _Lease
    readonly status: (state: State, horizon: Clock.Hlc, lease: Lease) => Status
    readonly roster: (
      table: Fold.Table<Key, State>,
      horizon: Clock.Hlc,
      lease: Lease,
    ) => HashMap.HashMap<Key, Status>
    readonly crowd: (
      table: Fold.Table<Key, State>,
      horizon: Clock.Hlc,
      lease: Lease,
    ) => HashMap.HashMap<Surface, HashSet.HashSet<Key>>
  }
}

const _STATUS = ["live", "idle", "gone"] as const

const _LeaseDuration = Schema.DurationFromSelf.pipe(
  Schema.filter((duration) => Duration.isFinite(duration) && Duration.greaterThan(duration, Duration.zero)),
)
class _Lease extends Schema.Class<_Lease>("Presence.Lease")(
  Schema.Struct({ idle: _LeaseDuration, gone: _LeaseDuration }).pipe(
    Schema.filter((lease) => Duration.lessThan(lease.idle, lease.gone)),
  ),
) {}

const _byWorn = <A>(): Order.Order<Presence.Worn<A>> =>
  Order.combine(
    Order.mapInput(Clock.Hlc.Order, (worn: Presence.Worn<A>) => worn.at),
    Order.mapInput(Order.string, (worn: Presence.Worn<A>) => worn.tie),
  )

const _worn = <A>(): Merge.Instance<Option.Option<Presence.Worn<A>>> => Merge.optional(Merge.max(_byWorn<A>()))

const _ephemeral: { readonly [K in _Axis]: Merge.Instance<Option.Option<Presence.Worn<Schema.Schema.Type<_Axes[K]>>>> } = Record.map(
  _AXES,
  () => _worn(),
) as { readonly [K in _Axis]: Merge.Instance<Option.Option<Presence.Worn<Schema.Schema.Type<_Axes[K]>>>> }

const _blank: { readonly [K in _Axis]: Option.Option<never> } = Record.map(_AXES, () => Option.none()) as {
  readonly [K in _Axis]: Option.Option<never>
}

const _state: Merge.Instance<Presence.State> = Merge.struct({
  joined: Merge.optional(Merge.min(Clock.Hlc.Order)),
  face: _worn<Presence.Profile>(),
  device: _worn<Presence.Device>(),
  quality: _worn<Presence.Grade>(),
  ..._ephemeral,
  last: Merge.max(Clock.Hlc.Order),
  gone: Merge.optional(Merge.max(Clock.Hlc.Order)),
})

const _dress = <A>(at: Clock.Hlc, tie: string) => (value: A): Presence.Worn<A> => ({
  value,
  at,
  tie,
})

const _silent = (at: Clock.Hlc): Presence.State => ({
  joined: Option.none(),
  face: Option.none(),
  device: Option.none(),
  quality: Option.none(),
  ..._blank,
  last: at,
  gone: Option.none(),
})

const _dressed = (op: Schema.Schema.Type<typeof _Move>): Pick<Presence.State, _Axis> =>
  Record.map(_AXES, (_axis, key) => Option.map(op[key], _dress(op.at, _opCell(op)))) as Pick<Presence.State, _Axis>

const _lifted: (op: Presence.Op) => Presence.State = _OpCase.$match({
  Join: (op) => ({
    ..._silent(op.at),
    joined: Option.some(op.at),
    face: Option.some(_dress(op.at, _opCell(op))(op.profile)),
    device: Option.some(_dress(op.at, _opCell(op))(op.device)),
  }),
  Beat: (op) => ({
    ..._silent(op.at),
    quality: Option.map(op.quality, _dress(op.at, _opCell(op))),
  }),
  Move: (op) => ({
    ..._silent(op.at),
    ..._dressed(op),
  }),
  Leave: (op) => ({
    ..._silent(op.at),
    gone: Option.some(op.at),
  }),
})
```

## [04]-[ROSTER_READS]

[ROSTER_READS]:

```typescript signature
const _noEarlier = Order.greaterThanOrEqualTo(Clock.Hlc.Order)

const _idled = (state: Presence.State, horizon: Clock.Hlc, lease: Presence.Lease): Presence.Status =>
  horizon.physical - state.last.physical > Clock.Hlc.delta(lease.idle) ? "idle" : "live"

const _status = (state: Presence.State, horizon: Clock.Hlc, lease: Presence.Lease): Presence.Status =>
  Option.match(state.gone, {
    onSome: (gone) =>
      _noEarlier(gone, state.last) && horizon.physical - gone.physical > Clock.Hlc.delta(lease.gone)
        ? "gone"
        : _idled(state, horizon, lease),
    onNone: () => _idled(state, horizon, lease),
  })

// sighting precedence is engagement strength: a pointer position, then a text caret, then bare focus — every
// surface-carrying axis sights, so an actor typing without a pointer still counts toward the surface's crowd
const _sighted = (state: Presence.State): Option.Option<Presence.Surface> =>
  Option.orElse(
    Option.map(state.cursor, (worn) => worn.value.surface),
    () =>
      Option.orElse(
        Option.map(state.caret, (worn) => worn.value.surface),
        () => Option.map(state.focus, (worn) => worn.value),
      ),
  )

const Presence: Presence.Shape = {
  Op: _Op,
  Lease: _Lease,
  state: _state,
  plan: {
    name: "state/presence",
    key: (op) => Data.tuple(op.tenant.scope, op.actor),
    cell: ([tenant, actor]) => Fold.cell([tenant, actor]),
    keyAlike: Equal.equivalence(),
    lift: _lifted,
    merge: _state,
    identity: Option.some(_opCell),
  },
  status: _status,
  roster: (table, horizon, lease) => HashMap.map(table, (state) => _status(state, horizon, lease)),
  crowd: (table, horizon, lease) =>
    HashMap.reduce(table, HashMap.empty<Presence.Surface, HashSet.HashSet<Presence.Key>>(), (acc, state, key) =>
      _status(state, horizon, lease) === "gone"
        ? acc
        : Option.match(_sighted(state), {
            onNone: () => acc,
            onSome: (surface) =>
              HashMap.modifyAt(acc, surface, (slot) =>
                Option.some(HashSet.add(Option.getOrElse(slot, () => HashSet.empty<Presence.Key>()), key))),
          })),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Presence }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
