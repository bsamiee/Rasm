# [CORE_PRESENCE]

The actor-presence CRDT: the wire-carried op family — `Join` with actor metadata, `Beat` heartbeats, `Leave` departures — merged per actor into one `Merge.struct` product whose every row is a proven instance, so presence converges across feeds and replicas like any lattice, with status a read-time verdict over a caller-supplied horizon so liveness policy is a value and the fold never reads an ambient clock. A serving edge decodes client frames INTO this family and forwards rosters; the fold below is the only presence authority, one more `fold#PLAN_CONTRACT` plan row every altitude runs unchanged — the browser roster is the fold's in-memory handle, the ordered roster board is the fractional-index lane, and no second presence table exists anywhere. The module is `core/src/state/presence.ts`; a new presence op is one tagged case plus one lift arm, a new state axis is one field plus one product row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                      | [PUBLIC]                                            |
| :-----: | :--------------- | :------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `OP_FAMILY`      | the actor brand and the wire-carried op union                   | `Presence.Op`                                            |
|  [02]   | `STATE_PRODUCT`  | the per-actor state product instance and the op lift            | `Presence.state`, `Presence.plan`                        |
|  [03]   | `ROSTER_READS`   | lease policy, status verdicts, the roster projection            | `Presence.status`, `Presence.roster`, `Presence.Lease`   |

## [2]-[OP_FAMILY]

[OP_FAMILY]:
- Owner: `Presence.Op` — the wire-carried op family (`Join` with actor metadata, `Beat` heartbeats, `Leave` departures), each a tagged struct stamped with `Hlc` and tenant; the interchange codec decodes client frames INTO this family at its own seam, and interior construction composes already-branded parts.
- Law: the actor brand is interior — `ActorId` reaches consumers only as `Presence.Actor`, one spelling for roster keys and op fields; a session or replica identity is `causal`'s `Vector.Replica`, a distinct concept that never unifies with the actor axis.
- Growth: a new presence op is one tagged case plus one lift arm; a new op axis is one field on its case.
- Packages: `effect` (`Schema`, `Duration`, `HashMap`, `Match`, `Option`, `Order`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`TenantContext`); `./merge.ts` (`Merge`); `./fold.ts` (`Fold`).

```typescript
import { type Duration, HashMap, Match, Option, Order, pipe, Schema } from "effect"
import { Hlc } from "../value/clock.ts"
import { TenantContext } from "../value/identity.ts"
import { Fold } from "./fold.ts"
import { Merge } from "./merge.ts"

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
```

## [3]-[STATE_PRODUCT]

[STATE_PRODUCT]:
- Owner: `Presence.state` — the per-actor `Merge.struct` product: `joined` min-stamp, `face` (`Option`-lifted metadata paired with its stamp, merged by stamped LWW under the optional lift), `last` max-stamp, `gone` optional max-stamp — every row a proven instance, so the product's posture derives as the conjunction and the whole instance is convergence-legal by construction; `Presence.plan` keys by actor, so the presence table is one more replay-maintained fold.
- Law: `Leave` is evidence, not deletion — the `gone` stamp coexists with later `Beat`s (a rejoin outruns a stale leave by stamp comparison at read time), so out-of-order delivery cannot resurrect or bury an actor incorrectly.
- Law: a `Beat` can never bury richer metadata — `face` is `Option`-lifted, only `Join` contributes `some` at its stamp while `Beat`/`Leave` contribute the lawful `none` identity, so the latest `Join` metadata survives every heartbeat until a newer `Join` replaces it.
- Law: batch-atomic roster application — a multi-actor op batch that must land all-or-nothing rides `merge#MERGE_CELLS`'s `Merge.cell(Presence.state)`, the same instance in a transactional table; the fold handles stay the live-view altitude and neither re-declares the other.
- Growth: a new state axis (device kind, focus coordinate) is one field plus one product row.

```typescript
declare namespace Presence {
  type Actor = typeof _Actor.Type
  type Op = typeof _Op.Type
  type Face = { readonly meta: HashMap.HashMap<string, string>; readonly at: Hlc }
  type State = {
    readonly joined: Hlc
    readonly face: Option.Option<Face>
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
  face: Merge.optional(Merge.max(_byFaceStamp)),
  last: Merge.max(Hlc.Order),
  gone: Merge.optional(Merge.max(Hlc.Order)),
})

const _lifted: (op: Presence.Op) => Presence.State = pipe(
  Match.type<Presence.Op>(),
  Match.tagsExhaustive({
    Join: (op) => ({
      joined: op.at,
      face: Option.some({ meta: op.meta, at: op.at }),
      last: op.at,
      gone: Option.none(),
    }),
    Beat: (op) => ({
      joined: op.at,
      face: Option.none(),
      last: op.at,
      gone: Option.none(),
    }),
    Leave: (op) => ({
      joined: op.at,
      face: Option.none(),
      last: op.at,
      gone: Option.some(op.at),
    }),
  }),
)
```

## [4]-[ROSTER_READS]

[ROSTER_READS]:
- Owner: the read family — `status` folds one actor's state against a horizon and a `Lease` policy row (`idle` and `gone` windows as `Duration` values); `roster` maps the verdict across the folded table.
- Law: status is three-valued and read-time — `gone` when a leave stamp is at or after the last activity AND the horizon has outrun it by the lease's gone window (the departure grace that absorbs in-flight beats), `idle` when the horizon outruns the last stamp by the idle window, `live` otherwise — and expiry never mutates state: a sweep is the consumer re-reading with a fresh horizon, so no timer fiber lives in this module; distances measure through `Hlc.delta`, never a millisecond re-derivation.
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
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Presence }
```
