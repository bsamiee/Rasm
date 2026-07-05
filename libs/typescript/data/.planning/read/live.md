# [DATA_LIVE]

Reactivity-keyed reactive reads: read-your-writes is one coordinate vocabulary written at the mutation and consumed at the query, never a poll and never a cache to bust by hand. The journal's publish transaction stamps invalidation keys through its slots; this page owns the read half — `Live.of(spec)` binds a keyed, schema-decoded query into a one-shot `read`, a push `changes` stream that re-runs on every overlapping mutation, and a pull `mailbox` twin — plus the foreign-write edge, the one road by which a mutation that bypassed the publish transaction (a relay completion, a wire-arrived write, a rebuild swap, a fact drain) still wakes exactly the readers its coordinates name. Keys are the whole contract: the record form scopes `{ band: cells }`, an empty cell list names the whole band, a member mutation wakes member readers and whole-band readers both, and delivery is exact because the coordinates are shared data, not convention.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                        |
| :-----: | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `KEY_COORDINATES` | the invalidation-key vocabulary — band/member scoping, the stamp/consume symmetry  |
|  [02]   | `LIVE_READS`      | `Live.of` — the decoded read, the reactive stream, the mailbox twin                |
|  [03]   | `FOREIGN_EDGE`    | mutation wrapping and bare invalidation for writes outside the publish transaction |

## [2]-[KEY_COORDINATES]

- Owner: `Live.Keys` — the one coordinate shape both sides of read-your-writes speak — and `Live.band`/`Live.cells`, the two mints that keep every spelling of a coordinate on one constructor pair.
- Packages: `effect` (`Record`, `Array`).
- Entry: a `Journal.Slot`'s `keys` member returns this shape (the publish transaction stamps it once per commit — `journal/append.md`'s slot law); every reader below subscribes the same shape; the foreign edge in `[4]` invalidates it directly.
- Growth: a new read surface is a new band name minted where its slot lives — the vocabulary never widens, only the band namespace grows; a cross-band view subscribes several bands in one record.
- Law: the record form is the coordinate algebra — `{ board: ["cell-a"] }` names a member, `{ board: [] }` names the whole band, and a member mutation wakes both the member's readers and the band's readers; scoping is data the bus already folds, so no reader re-derives overlap.
- Law: band names are the slot's `name` — the projection lane that writes a table owns its band string, and a reader names the band through the lane's published constant, never a string literal; a stringly key minted at a call site is the drift this vocabulary exists to kill.
- Law: cells are the relation's own key spelling — the same `cell` value the keyed upsert writes — so the coordinate a mutation stamps and the coordinate a reader subscribes are equal by construction, and stale-wake or missed-wake is unspellable.
- Boundary: the `Reactivity` service itself ships from `@effect/experimental` and is provisioned by the driver layers (`lane/postgres.md` mints clients whose `reactive` members require it); which rows stamp which bands is each lane's declaration in `read/fold.md`.

```typescript
import { Array, Record } from "effect"

declare namespace Live {
  type Keys = Record.ReadonlyRecord<string, ReadonlyArray<string>>
}

const _band = (name: string): Live.Keys => ({ [name]: [] })

const _cells = (name: string, cells: ReadonlyArray<string>): Live.Keys => ({ [name]: cells })

const _merged = (coordinates: ReadonlyArray<Live.Keys>): Live.Keys =>
  Array.reduce(coordinates, Record.empty<string, ReadonlyArray<string>>(), (held, keys) =>
    Record.union(held, keys, (left, right) => [...left, ...right]))
```

## [3]-[LIVE_READS]

- Owner: `Live.of(spec)` — one binding over `{ keys, query }` yielding the three read modalities: `read` (the decoded one-shot), `changes` (the reactive stream re-running on every overlapping mutation), `mailbox` (the pull-model twin whose consumer drains on its own cadence).
- Packages: `@effect/sql` (`SqlClient` — `sql.reactive`, `sql.reactiveMailbox`); `effect` (`Effect`, `Stream`, `Mailbox`).
- Entry: a projection lane publishes `Live.of` bindings beside its table (the lane's `read` composed with the lane's band); the runtime branch serves `changes` over sockets and server-sent events, and the browser's persistence lane pulls `mailbox` — both consume the bound value, never the bus.
- Receipt: every `changes` emission is a fresh run of the same decoded query — the stream carries decoded values only, so a subscriber holds domain shapes and re-render diffing is the consumer's fold over equal-by-construction values.
- Growth: a new live view is one `Live.of` with its own query and coordinates; a parameterized view (per-cell, per-window) is a constructor argument closing over the query, never a second modality.
- Law: the query inside a binding is a decoded read — a `SqlSchema` accessor from `read/query.md` or a lane's own decoded load — so the reactive stream can never emit an untyped row; the decode failure of a stale-schema row surfaces as `ParseError` on the stream and the repair is the rebuild lane, never an in-place patch.
- Law: `changes` is push-exact and `mailbox` is pull-exact — the stream re-emits once per overlapping mutation batch, the mailbox holds the re-run for the consumer's next take; choosing by consumer geometry is the whole decision, and a cadence poll beside either restates delivery the keys already own.
- Law: TTL is not freshness — a cached read that must follow writes composes these coordinates, never a shorter cache window; the tier table in `lane/cache.md` already bans that smuggle and this page is the lawful alternative.

```typescript
import { Effect, type Mailbox, Stream, type Scope } from "effect"
import { SqlClient, type SqlError } from "@effect/sql"
import type { ParseResult } from "effect"

declare namespace Live {
  type Spec<A, R> = {
    readonly keys: Keys
    readonly query: Effect.Effect<A, SqlError.SqlError | ParseResult.ParseError, R>
  }
  type Bound<A, R> = {
    readonly read: Effect.Effect<A, SqlError.SqlError | ParseResult.ParseError, R>
    readonly changes: Stream.Stream<A, SqlError.SqlError | ParseResult.ParseError, R | SqlClient.SqlClient>
    readonly mailbox: Effect.Effect<
      Mailbox.ReadonlyMailbox<A, SqlError.SqlError | ParseResult.ParseError>,
      never,
      R | SqlClient.SqlClient | Scope.Scope
    >
  }
}

const _of = <A, R>(spec: Live.Spec<A, R>): Live.Bound<A, R> => ({
  read: spec.query,
  changes: Stream.unwrap(
    Effect.map(SqlClient.SqlClient, (sql) => sql.reactive(spec.keys, spec.query)),
  ),
  mailbox: Effect.flatMap(SqlClient.SqlClient, (sql) => sql.reactiveMailbox(spec.keys, spec.query)),
})
```

## [4]-[FOREIGN_EDGE]

- Owner: `Live.mutation` and `Live.invalidate` — the two spellings by which a write outside the publish transaction still delivers read-your-writes: wrap the write so completion stamps its coordinates, or stamp bare coordinates when the write already happened somewhere this process only observes.
- Packages: `@effect/experimental` (`Reactivity` — the service Tag, `mutation`, `invalidate`; `Reactivity.layer` is the provisioning row the driver layers compose).
- Entry: the relay completion statement wraps in `Live.mutation` so a drained outbox row wakes the delivery boards; the rebuild swap invalidates its lane's whole band after the rename commits; a fact-journal drain stamps the meter bands its rollup readers subscribe; a write observed off the wire (a peer runtime's committed effect arriving as an event) invalidates the coordinates its decoded payload names.
- Growth: a new foreign writer is one wrap or one stamp at its completion seam — readers never change, because the coordinates are the contract.
- Law: stamp on durable completion only — `mutation` runs the effect and stamps when it succeeds, and a bare `invalidate` follows the commit it reports, never precedes it; a pre-commit stamp wakes readers into the old state and is the torn spelling.
- Law: exactly one stamp per completed unit of work — the publish transaction already stamps its slots' coordinates once per commit, so a foreign wrap never doubles a slotted write; foreign means outside that transaction, and double-stamping the same commit is the named defect.
- Law: over-invalidation is the honest degradation — a foreign writer that cannot name member cells stamps the whole band, costing re-runs, never correctness; silent under-invalidation is the defect class this edge exists to prevent.

```typescript
import { Reactivity } from "@effect/experimental"

const _mutation = <A, E, R>(keys: Live.Keys, write: Effect.Effect<A, E, R>) =>
  Effect.flatMap(Reactivity.Reactivity, (bus) => bus.mutation(keys, write))

const _invalidate = (keys: Live.Keys) =>
  Effect.flatMap(Reactivity.Reactivity, (bus) => bus.invalidate(keys))

const Live = {
  band: _band,
  cells: _cells,
  merged: _merged,
  of: _of,
  mutation: _mutation,
  invalidate: _invalidate,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Live }
```
