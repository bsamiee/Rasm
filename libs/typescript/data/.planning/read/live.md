# [DATA_LIVE]

Reactivity-keyed reactive reads: read-your-writes is one coordinate vocabulary written at the mutation and consumed at the query, never a poll and never a cache to bust by hand. Publish transactions stamp invalidation keys through their slots; this page owns the read half — `Live.of(spec)` binds a keyed query into a one-shot `read`, a `changes` stream re-running on every overlapping mutation under a declared re-arm policy, and a `mailbox` twin derived from that stream — with the foreign-write edge waking the coordinates a bypassing write names.

Every modality reaches the invalidation bus directly, so a binding over a non-relational read carries no relational dependency. `Live.Keys` is the identity-rich coordinate owner: its `coordinates` field carries the pattern-proven band and member refinements, `Live.scope` is the one band mint, and `Live.band`/`Live.cells`/`Live.merged` are its only admissions. Coordinate records scope `{ band: cells }`, an empty cell list names the whole band, and a member mutation wakes member readers and whole-band readers both.

## [01]-[INDEX]

- [02]-[KEY_COORDINATES]: `Live.Keys` — field refinements, admissions, merge, the stamp/consume law.
- [03]-[LIVE_READS]: `Live.of` — the decoded read, the re-armed reactive stream, the mailbox twin, the emission-coordinate projection.
- [04]-[FOREIGN_EDGE]: mutation wrapping and bare invalidation for writes outside the publish transaction.

## [02]-[KEY_COORDINATES]

- Owner: `Live.Keys` — the one admitted coordinate shape both sides of read-your-writes speak — with band, name, and cell evidence embedded as field refinements, `Live.scope` as the one band mint, and `Live.band`/`Live.cells`/`Live.merged` as its closed admission family.
- Packages: `effect` (`Schema`, `Record`, `Array`); `@rasm/core` (`Shape.Record`).
- Entry: a composition binds `Live.scope(discriminant)` once from the scope key it already holds and every lane declaration mints its band through that binding; a `Journal.Slot`'s `keys` member returns the resulting shape (the publish transaction stamps it once per commit — `journal/append.md`'s slot law); every reader below subscribes the same shape; the foreign edge in `[4]` invalidates it directly.
- Law: coordinates are scope-qualified at mint, because the bus is not — one `Reactivity` root serves every scope in the process and keys handlers on the coordinate value alone, so two scopes minting one band spelling share its wake; `Live.scope` is that qualification's one producer, folding the composition's discriminant with the lane's declared name into the band the slot carries, so cross-scope wake is unrepresentable rather than merely unlikely. Over-invalidation WITHIN a scope stays the honest degradation and costs re-runs; across scopes it leaks one tenant's write cadence to another's readers as timing evidence, which no re-run repairs.
- Law: the bus stays ONE root and never becomes per-scope — `lane/tenant.md`'s shared spine adopts a single pool constructed ABOVE the per-scope lookup, so a bus composed inside that lookup still serves every row- and schema-isolated scope from one instance and separates the dedicated-database case alone, granting isolation exactly where the pool already grants it and withholding it everywhere the pool does not; the discriminant therefore rides the coordinate VALUE, which no composition topology undoes.
- Law: this owner takes the discriminant as an argument and imports no data sibling — it mints at the folder floor, beneath the journal that stamps it, so the scope key that decides which subgraph serves a scope stays unreachable from here and arrives already projected into one lowercase token; reaching upward for that key inverts the strata the branch closes.
- Law: a key family decides its own fits, admit, and lifetime, and decides no retention — a whole-band coordinate fits a reader that cannot name cells and a member coordinate fits one that can; both admit through the publish transaction's slot stamp or `[4]`'s foreign edge, and both live exactly until the next overlapping stamp, so a coordinate holds no state to age and retention has nothing here to own. What the vocabulary forfeits is delta precision: a band wake re-reads every member reader.
- Growth: a new read surface is one `Live.scope` call under the slot's own scope binding — the vocabulary never widens, only the band namespace grows; a cross-band view subscribes several bands through `Live.merged`.
- Law: band, name, and cell evidence are field schemas owned by `Live.Keys`, never standalone branded exports — declaration sites name their lane through `Live.Keys.Name`, coordinate positions admit through `Live.Keys.Band`, lane key projections admit through `Live.Keys.Cell`, and a call-site string literal has no type-level road to `Live.Keys`.
- Law: `Live.Keys.Band` admits the QUALIFIED spelling alone — its pattern is two name segments across one separator, so a bare lane name refuses at decode and the only assembly that produces a passing value is `Live.scope`; an unqualified band is therefore unspellable rather than merely discouraged.
- Law: `Live.cell` is the relation's own key spelling admission — the lane's `cell` projection returns the embedded member type, so the coordinate a mutation stamps and the coordinate a reader subscribes are the same admitted value by construction, and stale-wake or missed-wake is a compile error, not runtime drift.
- Law: the record form is the coordinate algebra — `{ board: [cell] }` names a member, `{ board: [] }` names the whole band, and a member mutation wakes both the member's readers and the band's readers; scoping is data the bus already folds, so no reader re-derives overlap, and `Live.merged` unions and deduplicates coordinate sets cell-list-wise while treating either empty member set as whole-band dominance.
- Boundary: the `Reactivity` service ships from `@effect/experimental` and its provisioning is ONE root row — `Reactivity.layer` composed once beneath every driver Layer, because every binding on this page reads the service directly AND `SqlClient` construction requires it; a scope whose root omits the row fails at the composition proof, never at first subscription. Which rows stamp which bands is each lane's declaration in `read/fold.md`.

```typescript
import { Array, Record, Schema } from "effect"
import { Shape } from "@rasm/core"

const _Name = Schema.NonEmptyString.pipe(Schema.pattern(/^[a-z][a-z0-9_]*$/), Schema.brand("LiveName"))

const _Band = Schema.NonEmptyString.pipe(
  Schema.pattern(/^[a-z][a-z0-9_]*:[a-z][a-z0-9_]*$/),
  Schema.brand("LiveBand"),
)

const _Cell = Schema.NonEmptyString.pipe(Schema.brand("LiveCell"))

class _Keys extends Schema.Class<_Keys>("Live.Keys")({
  coordinates: Shape.Record(_Band, Schema.Array(_Cell)),
}) {
  static readonly Band = _Band
  static readonly Cell = _Cell
  static readonly Name = _Name
}

declare namespace Live {
  type Band = typeof _Band.Type
  type Cell = typeof _Cell.Type
  type Name = typeof _Name.Type
  type Keys = _Keys
}

const _cell = Schema.decodeSync(_Cell)

const _scope = (discriminant: Live.Name) => (name: Live.Name): Live.Band =>
  Schema.decodeSync(_Band)(`${discriminant}:${name}`)

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

- Owner: `Live.of(spec)` — one binding over `{ keys, query, rearm, backoff, backlog, coordinate }` yielding the three read modalities — `read` (the decoded one-shot), `changes` (the reactive stream re-running on every overlapping mutation under the bound's re-arm policy), `mailbox` (the pull twin derived from that same stream) — and `coordinate`, the emission-identity projection the serving plane's SSE fold reads as its dedupe token.
- Packages: `@effect/experimental` (`Reactivity.stream` — the keyed re-run over any effect); `effect` (`Effect`, `Stream`, `Mailbox.fromStream`, `Schedule`, `ParseResult`, `Predicate`, `Duration`, `Option`); `@rasm/core` (`Fault.Class.retryable`).
- Entry: a projection lane publishes `Live.of` bindings beside its table (the lane's `read` composed with the lane's band); the runtime branch serves `changes` over sockets and server-sent events, and the browser's persistence lane pulls `mailbox` — both consume the bound value, never the bus.
- Output: every `changes` emission is a fresh run of the same query — the stream carries decoded values only, so a subscriber holds domain shapes and re-render diffing is the consumer's fold over equal-by-construction values.
- Growth: a new live view is one `Live.of` with its own query and coordinates; a parameterized view (per-cell, per-window) is a constructor argument closing over the query, never a second modality.
- Law: the bus is the one reactive road — `Reactivity.stream` re-runs ANY effect on overlapping invalidation, so a bound over an object-plane fold, a cache read, or an in-memory projection binds exactly as a relational read binds and no client type enters the binding; the SQL client's own reactive members delegate to this same service on every driver this branch carries, so routing the read half through a client while the write half stamps the bus spells one capability twice and drags a relational dependency into bindings that touch no relation.
- Law: re-arm is a policy row, never a knob — the bus ENDS a feed on the first failed re-run, so `rearm` partitions the query's fault channel into `resume` (re-register the coordinates and re-read after the bound's backoff) and `close` (surface once and end); without the row one connection reset retires a subscription permanently and silently, which is the stall this policy deletes.
- Law: the default reads the core class table wherever a fault carries one, and probes the SHAPE first — `Fault.Class.of` grades a classless fault `defect` and `defect` is not retryable, so a blind class read closes every feed on the first driver fault; the probe routes graded faults through all ten kinds and leaves the ungraded road its own partition, where a decode refusal closes and every other fault resumes.
- Law: the backlog holds the latest answer alone — each emission carries the COMPLETE answer, so an older one is worthless the moment a newer lands and the pull twin rides a sliding window of one; an unbounded default banks full answers a slow consumer must drain and discard, paying memory for values it can never act on.
- Law: `changes` is push-exact and `mailbox` is pull-exact — both derive from ONE re-armed stream, so a policy change reaches both twins and the modalities cannot drift on failure behavior; choosing by consumer geometry is the whole decision, and a cadence poll beside either restates delivery the keys already own.
- Law: the query inside a binding is a decoded read — a `SqlSchema` accessor from `read/query.md` or a lane's own decoded load — so the reactive stream can never emit an untyped row; a stale-schema row's decode refusal ends the feed under the default policy and the repair is the rebuild lane, never an in-place patch.
- Law: TTL is not freshness — a cached read that must follow writes composes these coordinates, never a shorter cache window; the tier table in `lane/cache.md` already bans that smuggle and this page is the lawful alternative.
- Law: resume identity over a re-running feed is DEDUPE, not replay — `changes` re-runs the whole decoded query on every overlapping mutation, so each emission already carries the complete answer and a reconnecting client proves what it has already rendered rather than naming a backlog to ship; `coordinate` is therefore an `Option` projection off the bound's own decoded value (a lane carries its `AsOf` sequence, a coordinate-free bound answers none), and a bound with no coordinate serves frames a client cannot dedupe but never misses, which is the honest degradation — never a forged ordinal a client trusts as a resume point.
- Law: this plane forfeits row-level deltas by construction — the bus wakes on coordinate overlap and the bound re-reads its whole query, so a hot band pays one full read per overlapping mutation batch and a consumer wanting per-row change evidence folds it from successive answers; an incremental dataflow maintaining operator state in process is a different placement with its own owner, and a driver shipping a native change feed lands as one capability row at this binding rather than a second modality beside it.
- Boundary: wire rendering is the serving plane's — `serve/live#SSE_ROW`'s one endpoint fold frames every feed family through the branch's single `Sse` encoder, reading `coordinate` as the event `id`, so this folder ships decoded values and their identity projection while no frame string, route, content type, or connection lifetime enters it.

```typescript
import { Duration, Effect, Mailbox, Option, ParseResult, Predicate, Schedule, type Scope, Stream } from "effect"
import { Reactivity } from "@effect/experimental"
import { Fault } from "@rasm/core"

const _REARM = ["resume", "close"] as const

const _BACKOFF = Schedule.union(Schedule.exponential(Duration.millis(100)), Schedule.spaced(Duration.seconds(5)))

const _rearm = (fault: unknown): Live.Rearm =>
  Predicate.hasProperty(fault, "class")
    ? Fault.Class.retryable(fault) ? "resume" : "close"
    : ParseResult.isParseError(fault) ? "close" : "resume"

declare namespace Live {
  type Rearm = (typeof _REARM)[number]
  type Spec<A, E, R> = {
    readonly keys: Keys
    readonly query: Effect.Effect<A, E, R>
    readonly rearm?: (fault: E) => Rearm
    readonly backoff?: Schedule.Schedule<unknown, E>
    readonly backlog?: number
    readonly coordinate?: (value: A) => Option.Option<string>
  }
  type Bound<A, E, R> = {
    readonly read: Effect.Effect<A, E, R>
    readonly changes: Stream.Stream<A, E, Exclude<R, Scope.Scope> | Reactivity.Reactivity>
    readonly mailbox: Effect.Effect<
      Mailbox.ReadonlyMailbox<A, E>,
      never,
      Exclude<R, Scope.Scope> | Reactivity.Reactivity | Scope.Scope
    >
    readonly coordinate: (value: A) => Option.Option<string>
  }
}

const _of = <A, E, R>(spec: Live.Spec<A, E, R>): Live.Bound<A, E, R> => {
  const verdict = spec.rearm ?? _rearm
  const changes = Reactivity.stream(spec.query, spec.keys.coordinates).pipe(
    Stream.retry(Schedule.whileInput<E>((fault) => verdict(fault) === "resume")(spec.backoff ?? _BACKOFF)),
  )
  return {
    read: spec.query,
    changes,
    mailbox: Mailbox.fromStream(changes, { capacity: spec.backlog ?? 1, strategy: "sliding" }),
    coordinate: spec.coordinate ?? (() => Option.none()),
  }
}
```

## [04]-[FOREIGN_EDGE]

- Owner: `Live.mutation` and `Live.invalidate` — the two spellings by which a write outside the publish transaction still delivers read-your-writes: wrap the write so completion stamps its coordinates, or stamp bare coordinates when the write already happened somewhere this process only observes.
- Packages: `@effect/experimental` (`Reactivity.mutation`, `Reactivity.invalidate` — the module accessors over the service Tag; `Reactivity.layer` is the one root provisioning row).
- Entry: the relay completion statement wraps in `Live.mutation` so a drained outbox row wakes the delivery boards; the rebuild swap invalidates its lane's whole band after the rename commits; a fact-journal drain stamps the meter bands its rollup readers subscribe; a write observed off the wire (a peer runtime's committed effect arriving as an event) invalidates the coordinates its decoded payload names.
- Growth: a new foreign writer is one wrap or one stamp at its completion boundary — readers never change, because the coordinates are the contract.
- Law: stamp on durable completion only — `mutation` runs the effect and stamps when it succeeds, and a bare `invalidate` follows the commit it reports, never precedes it; a pre-commit stamp wakes readers into the old state and is the torn spelling. Refusal on any channel — typed fault, defect, or interrupt — leaves the coordinates unstamped, so readers hold the state a failed write never moved.
- Law: exactly one stamp per completed unit of work — the publish transaction already stamps its slots' coordinates once per commit, so a foreign wrap never doubles a slotted write; foreign means outside that transaction, and double-stamping the same commit is the named defect.
- Law: over-invalidation is the honest degradation — a foreign writer that cannot name member cells stamps the whole band, costing re-runs, never correctness; silent under-invalidation is the defect class the minted vocabulary exists to prevent, and it is unspellable because a foreign writer holds either the lane's published band value or nothing.

```typescript
import { Reactivity } from "@effect/experimental"

const _mutation = <A, E, R>(keys: Live.Keys, write: Effect.Effect<A, E, R>) =>
  Reactivity.mutation(write, keys.coordinates)

const _invalidate = (keys: Live.Keys) => Reactivity.invalidate(keys.coordinates)

const Live = {
  Keys: _Keys,
  scope: _scope,
  band: _band,
  cell: _cell,
  cells: _cells,
  merged: _merged,
  of: _of,
  mutation: _mutation,
  invalidate: _invalidate,
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Live }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
