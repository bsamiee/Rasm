# [DATA_LIVE]

Reactivity-keyed reactive reads: read-your-writes is one coordinate vocabulary written at the mutation and consumed at the query, never a poll and never a cache to bust by hand. The journal's publish transaction stamps invalidation keys through its slots; this page owns the read half — `Live.of(spec)` binds a keyed, schema-decoded query into a one-shot `read`, a push `changes` stream that re-runs on every overlapping mutation, and a pull `mailbox` twin — plus the foreign-write edge, the one road by which a mutation that bypassed the publish transaction still wakes exactly the readers its coordinates name. `Live.Keys` is the identity-rich coordinate owner: its `coordinates` field carries the pattern-proven band and member refinements, and `Live.band`/`Live.cells`/`Live.merged` are its only admissions. The embedded record scopes `{ band: cells }`, an empty cell list names the whole band, and a member mutation wakes member readers and whole-band readers both.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                             |
| :-----: | :---------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `KEY_COORDINATES` | `Live.Keys` — field refinements, admissions, merge, the stamp/consume law          |
|  [02]   | `LIVE_READS`      | `Live.of` — the decoded read, the reactive stream, the mailbox twin                |
|  [03]   | `FOREIGN_EDGE`    | mutation wrapping and bare invalidation for writes outside the publish transaction |

## [02]-[KEY_COORDINATES]

- Owner: `Live.Keys` — the one admitted coordinate shape both sides of read-your-writes speak — with band and cell evidence embedded as field refinements and `Live.band`/`Live.cells`/`Live.merged` as its closed admission family.
- Packages: `effect` (`Schema`, `Record`, `Array`).
- Entry: a `Journal.Slot`'s `keys` member returns this shape (the publish transaction stamps it once per commit — `journal/append.md`'s slot law); every reader below subscribes the same shape; the foreign edge in `[4]` invalidates it directly.
- Growth: a new read surface is a new `Band` minted where its slot lives — the vocabulary never widens, only the band namespace grows; a cross-band view subscribes several bands through `Live.merged`.
- Law: band and cell evidence are field schemas owned by `Live.Keys`, never standalone branded exports — declaration sites admit through `Live.Keys.Band`, lane key projections admit through `Live.Keys.Cell`, and a call-site string literal has no type-level road to `Live.Keys`.
- Law: `Live.cell` is the relation's own key spelling admission — the lane's `cell` projection returns the embedded member type, so the coordinate a mutation stamps and the coordinate a reader subscribes are the same admitted value by construction, and stale-wake or missed-wake is a compile error, not runtime drift.
- Law: the record form is the coordinate algebra — `{ board: [cell] }` names a member, `{ board: [] }` names the whole band, and a member mutation wakes both the member's readers and the band's readers; scoping is data the bus already folds, so no reader re-derives overlap, and `Live.merged` unions and deduplicates coordinate sets cell-list-wise while treating either empty member set as whole-band dominance.
- Boundary: the `Reactivity` service ships from `@effect/experimental` and its provisioning is ONE root row — `Reactivity.layer` composed once beneath every driver Layer, because `SqlClient.make` itself requires the service and `sql.reactive`/`sql.reactiveMailbox` read it; a scope whose root omits the row fails at the composition proof, never at first subscription. Which rows stamp which bands is each lane's declaration in `read/fold.md`.

```typescript signature
import { Array, Record, Schema } from "effect"

const _Band = Schema.NonEmptyString.pipe(Schema.pattern(/^[a-z][a-z0-9_]*$/), Schema.brand("LiveBand"))

const _Cell = Schema.NonEmptyString.pipe(Schema.brand("LiveCell"))

class _Keys extends Schema.Class<_Keys>("Live.Keys")({
  coordinates: Schema.Record({ key: _Band, value: Schema.Array(_Cell) }),
}) {
  static readonly Band = _Band
  static readonly Cell = _Cell
}

declare namespace Live {
  type Band = typeof _Band.Type
  type Cell = typeof _Cell.Type
  type Keys = _Keys
}

const _cell = Schema.decodeSync(_Cell) // the one Cell mint: lanes call it inside their key projections, proven non-empty at the seam

const _band = (name: Live.Band): Live.Keys => new _Keys({ coordinates: { [name]: [] } })

const _cells = (name: Live.Band, cells: ReadonlyArray<Live.Cell>): Live.Keys => new _Keys({ coordinates: { [name]: cells } })

const _merged = (coordinates: ReadonlyArray<Live.Keys>): Live.Keys =>
  new _Keys({
    coordinates: Array.reduce(coordinates, Record.empty<Live.Band, ReadonlyArray<Live.Cell>>(), (held, keys) =>
      Record.union(held, keys.coordinates, (left, right) =>
        left.length === 0 || right.length === 0 ? [] : Array.dedupe(Array.appendAll(left, right)))),
  })
```

## [03]-[LIVE_READS]

- Owner: `Live.of(spec)` — one binding over `{ keys, query }` yielding the three read modalities: `read` (the decoded one-shot), `changes` (the reactive stream re-running on every overlapping mutation), `mailbox` (the pull-model twin whose consumer drains on its own cadence).
- Packages: `@effect/sql` (`SqlClient` — `sql.reactive`, `sql.reactiveMailbox`); `effect` (`Effect`, `Stream`, `Mailbox`).
- Entry: a projection lane publishes `Live.of` bindings beside its table (the lane's `read` composed with the lane's band); the runtime branch serves `changes` over sockets and server-sent events, and the browser's persistence lane pulls `mailbox` — both consume the bound value, never the bus.
- Receipt: every `changes` emission is a fresh run of the same decoded query — the stream carries decoded values only, so a subscriber holds domain shapes and re-render diffing is the consumer's fold over equal-by-construction values.
- Growth: a new live view is one `Live.of` with its own query and coordinates; a parameterized view (per-cell, per-window) is a constructor argument closing over the query, never a second modality.
- Law: the query inside a binding is a decoded read — a `SqlSchema` accessor from `read/query.md` or a lane's own decoded load — so the reactive stream can never emit an untyped row; the decode failure of a stale-schema row surfaces as `ParseError` on the stream and the repair is the rebuild lane, never an in-place patch.
- Law: `changes` is push-exact and `mailbox` is pull-exact — the stream re-emits once per overlapping mutation batch, the mailbox holds the re-run for the consumer's next take; choosing by consumer geometry is the whole decision, and a cadence poll beside either restates delivery the keys already own.
- Law: TTL is not freshness — a cached read that must follow writes composes these coordinates, never a shorter cache window; the tier table in `lane/cache.md` already bans that smuggle and this page is the lawful alternative.

```typescript signature
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
    Effect.map(SqlClient.SqlClient, (sql) => sql.reactive(spec.keys.coordinates, spec.query)),
  ),
  mailbox: Effect.flatMap(SqlClient.SqlClient, (sql) => sql.reactiveMailbox(spec.keys.coordinates, spec.query)),
})
```

## [04]-[FOREIGN_EDGE]

- Owner: `Live.mutation` and `Live.invalidate` — the two spellings by which a write outside the publish transaction still delivers read-your-writes: wrap the write so completion stamps its coordinates, or stamp bare coordinates when the write already happened somewhere this process only observes.
- Packages: `@effect/experimental` (`Reactivity` — the service Tag; the instance members `mutation` and `invalidate`; `Reactivity.layer` is the one root provisioning row).
- Entry: the relay completion statement wraps in `Live.mutation` so a drained outbox row wakes the delivery boards; the rebuild swap invalidates its lane's whole band after the rename commits; a fact-journal drain stamps the meter bands its rollup readers subscribe; a write observed off the wire (a peer runtime's committed effect arriving as an event) invalidates the coordinates its decoded payload names.
- Growth: a new foreign writer is one wrap or one stamp at its completion seam — readers never change, because the coordinates are the contract.
- Law: stamp on durable completion only — `mutation` runs the effect and stamps when it succeeds, and a bare `invalidate` follows the commit it reports, never precedes it; a pre-commit stamp wakes readers into the old state and is the torn spelling.
- Law: exactly one stamp per completed unit of work — the publish transaction already stamps its slots' coordinates once per commit, so a foreign wrap never doubles a slotted write; foreign means outside that transaction, and double-stamping the same commit is the named defect.
- Law: over-invalidation is the honest degradation — a foreign writer that cannot name member cells stamps the whole band, costing re-runs, never correctness; silent under-invalidation is the defect class the minted vocabulary exists to prevent, and it is unspellable because a foreign writer holds either the lane's published band value or nothing.

```typescript signature
import { Reactivity } from "@effect/experimental"

const _mutation = <A, E, R>(keys: Live.Keys, write: Effect.Effect<A, E, R>) =>
  Effect.flatMap(Reactivity.Reactivity, (bus) => bus.mutation(keys.coordinates, write))

const _invalidate = (keys: Live.Keys) =>
  Effect.flatMap(Reactivity.Reactivity, (bus) => bus.invalidate(keys.coordinates))

const Live = {
  Keys: _Keys,
  band: _band,
  cell: _cell,
  cells: _cells,
  merged: _merged,
  of: _of,
  mutation: _mutation,
  invalidate: _invalidate,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Live }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
