# [TYPESCRIPT_VALUES]

Every runtime invariant selects exactly one owning primitive, and the selection is this page's law: keyed state, membership, order, growth, identity, comparison, combination, time, precision, partiality, bytes, and secrecy each ride the owner that carries the invariant structurally. Identity is a construction fact, never a comparison-site computation; comparison, equivalence, refinement, and combination are composed instance values that travel as parameters and carry their derived operator families with them; scalars ride owners whose arithmetic is the domain's arithmetic. JS stdlib `Map`, `Set`, `Date`, epoch numbers, `NaN` sentinels, `btoa`, and raw secret strings survive only at the FFI seam and inside marked kernels — the moment one carries domain meaning, the owning primitive or a composed instance replaces it, and the value decision becomes recoverable from a declaration.

Everything around the value plane is shed by kind: vocabulary tables and the type-derivation algebra are `derivation.md`'s, Schema owners and decoded collection admission are `shapes.md`'s, the carrier with its in-flow `Option`/`Either` folds is `rails-and-effects.md`'s, dispatch over values is `surfaces-and-dispatch.md`'s, keyed accumulators threaded through incremental dataflow are `streams.md`'s, the decode seam that admits these values is `boundaries.md`'s, and the kernel mark with the statement seam is `language.md`'s.

## [01]-[INDEX]

This table maps a value invariant to the form that owns it; the most specific invariant wins.

| [INDEX] | [INVARIANT]                       | [OWNING_FORM]                                       | [REJECTED_FORM]                       |
| :-----: | :-------------------------------- | :-------------------------------------------------- | :------------------------------------ |
|  [01]   | keyed domain state                | `HashMap` keyed by `Data` value or primitive        | JS `Map`, object-as-map mutation      |
|  [02]   | keyed insert, update, and delete  | `HashMap.modifyAt` `Option` fold                    | `get`-then-`set` pair, spread rebuild |
|  [03]   | membership                        | `HashSet`                                           | JS `Set`, `Array.includes` scan       |
|  [04]   | order-bearing keyed traversal     | `SortedMap` carrying its `Order` at construction    | sort-on-read of map entries           |
|  [05]   | amortized growth, batch windows   | `Chunk`                                             | `push` accumulation, spread rebuild   |
|  [06]   | closed literal key set            | plain record under `Record`/`Struct` folds          | `HashMap` over a closed key set       |
|  [07]   | domain equality and container key | `Data` construction; `Equal.equals`                 | `===`, `JSON.stringify` comparison    |
|  [08]   | comparison and refinement policy  | composed `Order`/`Equivalence`/`Predicate` instance | inline comparators, boolean soup      |
|  [09]   | combine two values of one shape   | `Semigroup.struct`/`Monoid.struct` row table        | hand merge function, seeded `reduce`  |
|  [10]   | keyed partial-record merge        | `getMonoidUnion`/`getSemigroupUnion` instance       | spread overlay last-wins              |
|  [11]   | instant and span                  | `DateTime`/`Duration` owner arithmetic              | `Date`, epoch-millisecond math        |
|  [12]   | exact decimal                     | `BigDecimal` with explicit rounding                 | binary-float money math               |
|  [13]   | fallible numeric operation        | `Option`-returning owner; one fold at the consumer  | `NaN` guards, `parseFloat` ladders    |
|  [14]   | binary crossing a text channel    | `Encoding` total encode, `Either` decode            | `btoa`/`atob`, platform buffer calls  |
|  [15]   | secret value                      | `Redacted` sealed lifecycle                         | raw string secret in a shape or log   |

## [02]-[COLLECTION_OWNERS]

The invariant selects the container before any code is shaped, and the write surface is a fold. A keyed accumulator is one `Array.reduce` over `HashMap.modifyAt` — persistent structural sharing keeps the fold allocation-honest, no intermediate map escapes mid-fold, and the single `Option -> Option` update function owns every write modality one keyed concern will ever need.

[OWNER_SELECTION]:
- Law: keyed state rides `HashMap` keyed by `Data`-constructed values or primitives — JS `Map` is reference-keyed, so structurally equal keys miss on every fresh build and composite keys degrade into hand-joined strings; membership rides `HashSet`, never an `Array.includes` scan; traversal order rides `SortedMap` whose `Order` arrives at construction — `SortedMap.fromIterable(rows, order)`, `SortedMap.empty(order)` — never at read.
- Law: the key-space test decides record versus map — a closed literal key set is a plain record under `Record`/`Struct`/`Tuple` folds because the type level already carries its vocabulary, and an open, runtime-keyed space is `HashMap`; a `HashMap` over closed keys erases compile-time exhaustiveness, a record over open keys forges it.
- Use: `Chunk` for amortized append and batch windows — `Chunk.append`, `Chunk.appendAll`, `Chunk.splitAt` — where array spread rebuilds per step; `Array` module folds for linear transforms whose reads are total: `Array.head`, `Array.last`, `Array.findFirst`, and `HashMap.get` return `Option`, so no read produces `undefined` to guard.
- Accept: `HashMap.mutate` or `MutableHashMap` batching writes inside one marked kernel with the draft never escaping as live state.
- Boundary: a keyed accumulator threaded through incremental dataflow is `streams.md`'s; `Schema.HashMap`/`Schema.Chunk` admission is `shapes.md`'s; the kernel mark is `language.md`'s.

[KEYED_FOLD]:
- Law: `HashMap.modifyAt` is the single keyed write — its update function receives `Option<V>` and returns `Option<V>`, so insert (`none -> some`), update (`some -> some`), and delete (`-> none`) are three arms of one fold; a `get`-then-`set` pair, a `has` ladder, or a spread rebuild restates modalities the fold already discriminates.
- Law: the owner constructs its own keys — a lookup or write takes the raw discriminants and builds the `Data` key inside, because a key accepted from a caller may arrive plain-constructed and miss silently.
- Reject: object-as-map mutation; an escaping mutable map; a second map holding a projection of the first that one `HashMap.filterMap` derives on demand.

```typescript
import { Array, Data, HashMap, Number, Option } from "effect"

type Pulse = { readonly realm: string; readonly lane: string; readonly load: number }
type Meter = { readonly count: number; readonly peak: number; readonly sum: number }
type Key = readonly [realm: string, lane: string]

const meter = (pulses: ReadonlyArray<Pulse>): HashMap.HashMap<Key, Meter> =>
  Array.reduce(pulses, HashMap.empty<Key, Meter>(), (acc, pulse) =>
    HashMap.modifyAt(acc, Data.tuple(pulse.realm, pulse.lane), (slot) => // one keyed read-merge-write: the Option fold discriminates insert and update in place
      Option.some(Option.match(slot, {
        onNone: (): Meter => ({ count: 1, peak: pulse.load, sum: pulse.load }),
        onSome: (row): Meter => ({ count: row.count + 1, peak: Number.max(row.peak, pulse.load), sum: row.sum + pulse.load }),
      }))))

const prune = (meters: HashMap.HashMap<Key, Meter>, realm: string, lane: string, floor: number): HashMap.HashMap<Key, Meter> =>
  HashMap.modifyAt(meters, Data.tuple(realm, lane), (slot) =>
    Option.filter(slot, (row) => row.count >= floor))          // the delete modality of the same fold: a below-floor row filters to none and the key leaves the map

const read = (meters: HashMap.HashMap<Key, Meter>, realm: string, lane: string): Option.Option<Meter> =>
  HashMap.get(meters, Data.tuple(realm, lane))                 // the owner builds its key: retrieval rides structural Equal/Hash, a plain tuple literal would miss

// --- [EXPORTS] --------------------------------------------------------------------------

export { meter, prune, read }
export type { Key, Meter, Pulse }
```

## [03]-[STRUCTURAL_IDENTITY]

Identity is declared where a value is built, never computed where it is compared. The `Data` constructors implant `Equal` and `Hash` at construction, every keyed and deduplicating container consumes both, and one concept constructs through one channel — so equality, dedup, keying, and memo identity are structure-decided facts of the value itself.

[CONSTRUCTED_IDENTITY]:
- Law: `Data.struct`, `Data.tuple`, and `Data.array` implant `Equal` and `Hash` to full depth when members are themselves `Data`-constructed or primitive; `Equal.equals` is the one domain equality and `Hash.hash` its container-facing shadow, and `Equal.equals` implies hash agreement — the container contract — so equality arriving anywhere but the constructors desynchronizes from `Hash` and breaks retrieval silently.
- Law: one concept constructs through one channel — mixing `Data`-constructed and plain-built values of one concept splits identity down the middle, and half the concept misses every set, map, and dedup it enters.
- Law: projection equality is not identity — "same key fields" is an `Equivalence` question answered by a composed instance, and widening `Equal` to answer it poisons every container keyed on the concept; `Equal.equivalence()` bridges the other direction, lifting structural equality into an instance parameter where an operation wants one.
- Reject: `===` between domain values — reference identity asked of structural data; `JSON.stringify` comparison — key-order-dependent and allocating per probe; a hand deep-equality function; a plain literal as a container key.
- Boundary: tagged families and class owners implant this same identity at their declaration — the owner forms are `shapes.md`'s; the `Equivalence` algebra is `[04]`'s.

```typescript
import { Array, Data, Equal, Hash, HashSet } from "effect"

type Facet = { readonly axis: string; readonly grade: number }
type Mark = { readonly key: string; readonly facets: ReadonlyArray<Facet> }

const mark = (key: string, facets: ReadonlyArray<Facet>): Mark =>
  Data.struct({ key, facets: Data.array(Array.map(facets, Data.struct)) }) // identity implants at construction to full depth: nested members are Data-constructed too

const distinct = (marks: ReadonlyArray<Mark>): HashSet.HashSet<Mark> =>
  HashSet.fromIterable(marks)                                  // the set consumes the contract Equal implies — equal values hash equal — so duplicates collapse with no comparator

const sift = (seen: HashSet.HashSet<Mark>, incoming: ReadonlyArray<Mark>): ReadonlyArray<Mark> =>
  Array.filter(incoming, (row) => !HashSet.has(seen, row))     // membership is structure-decided: a rebuilt equal mark is recognized and dropped

const compact = (rows: ReadonlyArray<Mark>): ReadonlyArray<Mark> =>
  Array.dedupeWith(rows, Equal.equivalence())                  // Equal lifted to an instance parameter: order-preserving dedup without a set

const _replay: boolean = Equal.equals(mark("<value-a>", [{ axis: "<axis-a>", grade: 3 }]), mark("<value-a>", [{ axis: "<axis-a>", grade: 3 }]))
const _agreed: boolean = Hash.hash(mark("<value-a>", [])) === Hash.hash(mark("<value-a>", [])) // both true: fresh builds compare and hash structurally; === on the pair is false and never asked

// --- [EXPORTS] --------------------------------------------------------------------------

export { compact, distinct, mark, sift }
export type { Facet, Mark }
```

## [04]-[ALGEBRA_INSTANCES]

Ordering, equivalence, and refinement carry domain policy, so each is one composed instance value every consumer shares — built from shipped atoms at the owner declaration, passed as a parameter into instance-taking operations, and projecting its derived operator family so no call site re-derives what the instance carries.

[ORDER_COMPOSITION]:
- Law: an `Order` composes inline at its owner declaration — `Order.mapInput` projects onto `Order.number`/`Order.string`/`Order.bigint`/`Order.boolean`/`Order.Date`, `Order.reverse` inverts, `Order.combine`/`Order.combineAll` chain lexicographic tie-breaks, `Order.struct`/`Order.tuple`/`Order.array` assemble composite orders — and the loose intermediate consts a consumer would reassemble do not exist; the `(a, b) => a.x - b.x` comparator is deleted as NaN-blind policy restated per site.
- Law: the instance anchors its derived family — `Order.min`, `Order.max`, `Order.clamp`, `Order.between`, `Order.lessThan` are projections of one instance, so one policy edit repoints every operator at once; `Array.min`/`Array.max` demand a `NonEmptyReadonlyArray`, so the empty case is decided at the type, never by a sentinel element.
- Law: instances travel as parameters — `Array.sortBy`, `Array.dedupeWith`, `Array.containsWith`, and `SortedMap` construction take the instance; `Array.sortBy`'s variadic orders accept the composed policy, never an inline re-derivation of it.

[EQUIVALENCE_REFINEMENT]:
- Law: projection equality is a composed `Equivalence` — `Equivalence.struct`/`tuple`/`array`/`mapInput` over `Equivalence.string`/`number`/`bigint` atoms — and instances are contravariant material: a record instance accepts every richer shape by parameter contravariance, so no mirror instance per consumer type exists and the delimiter-joined key string dies.
- Law: refinement composes as values — `Predicate.and`/`or`/`not`/`xor` over `Predicate.struct`/`tuple` field rows — and composition order pre-solves inference: the widest-typed predicate leads a data-first composition so the type parameter lands on the rich shape and narrower field instances follow by contravariance.
- Reject: boolean-soup conditions; per-site min/max/clamp ternaries; a comparison policy that exists twice under two names.
- Boundary: an `Order` consumed by a merge is `[05]`'s; `Schema.equivalence` derivation is `shapes.md`'s; predicate dispatch over non-keyed shapes is `surfaces-and-dispatch.md`'s.

```typescript
import { Array, Equivalence, Order, Predicate, SortedMap, pipe } from "effect"

type Probe = { readonly realm: string; readonly grade: number; readonly load: number }

const rank: Order.Order<Probe> = Order.combine(               // one lexicographic policy composed inline at the owner: grade descending, realm tie-break
  Order.mapInput(Order.reverse(Order.number), (probe: Probe) => probe.grade),
  Order.mapInput(Order.string, (probe: Probe) => probe.realm),
)

const alike: Equivalence.Equivalence<Probe> = Equivalence.struct({ realm: Equivalence.string, grade: Equivalence.number }) // the two-field instance accepts the richer Probe by contravariance: projection equality, distinct from identity

const admit: Predicate.Predicate<Probe> = Predicate.and(      // refinement as values: the Probe-typed arm leads so inference lands wide, field rows follow by contravariance
  Predicate.not((probe: Probe) => probe.realm === "<realm-a>"),
  Predicate.struct({ grade: (grade: number) => grade > 0, load: (load: number) => load > 8 }),
)

const lead = (probes: ReadonlyArray<Probe>): ReadonlyArray<Probe> =>
  pipe(probes, Array.filter(admit), Array.dedupeWith(alike), Array.sortBy(rank), Array.take(4)) // instance-parameterized consumption: the policy travels, the operations stay polymorphic

const cap = (probe: Probe, floor: Probe, ceiling: Probe): Probe =>
  Order.clamp(rank)(probe, { minimum: floor, maximum: ceiling }) // the derived family rides the instance: clamp/min/max/between are projections of rank

const board = (rows: Iterable<readonly [Probe, string]>): SortedMap.SortedMap<Probe, string> =>
  SortedMap.fromIterable(rows, rank)                           // the Order arrives at construction, never at read

// --- [EXPORTS] --------------------------------------------------------------------------

export { admit, alike, board, cap, lead, rank }
export type { Probe }
```

## [05]-[MERGE_ALGEBRA]

A domain combine is a `Semigroup`/`Monoid` instance, never an ad-hoc function. `Semigroup.struct` is the merge table — one row per field algebra, every merge semantic recoverable from the declaration, growth a row and never a second function — and the instance shares its type's name, so one exported name serves annotation and algebra together.

[MERGE_TABLE]:
- Law: rows are shipped atoms or one-line derivations — `SemigroupSum`/`SemigroupMax`/`MonoidSum` and `Bounded` from `@effect/typeclass/data/Number`, the `data/Duration`, `data/String`, and `data/Boolean` siblings, `Semigroup.min`/`Semigroup.max` deriving the extremum semigroup from any `Order`, `Semigroup.first`/`Semigroup.last` keep policies, `Semigroup.intercalate` separator folds — and a re-authored `(a, b) => a + b` restates an atom that ships; `Semigroup.tuple`/`Monoid.tuple` assemble the positional merge the same way `struct` assembles the keyed one.
- Law: the fold signature encodes the identity decision — `combineAll(rows)` where every row carries a lawful `empty` (a `Monoid` table), `combineMany(head, rest)` over a witnessed head where any row is identity-free — `first`, `last`, and `intercalate` admit no lawful empty, so promoting their fold to `combineAll` with a forged sentinel forges data.
- Law: `Bounded` lifts the extremum to lawful — `Monoid.max(bounded)`/`Monoid.min(bounded)` fold zero rows to `minBound`/`maxBound`, and `Monoid.fromSemigroup(semigroup, empty)` names the identity explicitly where one lawfully exists.

[KEYED_AND_ABSENT_MERGE]:
- Law: a keyed partial-record merge is an instance — `getMonoidUnion`/`getSemigroupUnion` from `@effect/typeclass/data/Record` keep keys present in one side and combine keys present in both by the row algebra, `getSemigroupIntersection` keeps only shared keys — and the spread overlay `{ ...left, ...right }` is the deleted spelling because it silently last-wins every collision.
- Law: absence lifts an identity-free algebra to lawful — `getOptionalMonoid(semigroup)` from `data/Option` makes `Option.none()` the empty, so an absent field is the identity and no sentinel value is forged to pad the fold.
- Use: `Record.union(self, that, combine)` as the one-off spelling when the combine is genuinely site-local; a keyed merge that recurs is the instance.
- Boundary: applicative error accumulation on the rail is `rails-and-effects.md`'s; a merge folded across a stream window is `streams.md`'s.

```typescript
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as DurationInstances from "@effect/typeclass/data/Duration"
import * as NumberInstances from "@effect/typeclass/data/Number"
import * as OptionInstances from "@effect/typeclass/data/Option"
import * as RecordInstances from "@effect/typeclass/data/Record"
import * as StringInstances from "@effect/typeclass/data/String"
import { Duration, Option } from "effect"

type Tally = {
  readonly runs: number
  readonly peak: number
  readonly spent: Duration.Duration
  readonly slowest: Duration.Duration
  readonly origin: string
  readonly note: Option.Option<string>
}

const Tally: Semigroup.Semigroup<Tally> = Semigroup.struct({  // the merge table: six merge semantics, one declaration, growth is a row — and the instance shares the type's name, one export serving both
  runs: NumberInstances.SemigroupSum,
  peak: NumberInstances.SemigroupMax,
  spent: DurationInstances.SemigroupSum,
  slowest: Semigroup.max(Duration.Order),                     // extremum over any Order: the Order is the only requirement, no shipped instance needed
  origin: Semigroup.first<string>(),
  note: OptionInstances.getOptionalMonoid(Semigroup.intercalate(StringInstances.Semigroup, "/")), // absence is the lawful identity: none() pads the fold, no sentinel string forged
})

const fold = (head: Tally, rest: Iterable<Tally>): Tally =>
  Tally.combineMany(head, rest)                               // first and intercalate admit no lawful empty: the witnessed head is the signature

type Gauge = { readonly runs: number; readonly peak: number; readonly spent: Duration.Duration }

const Gauge: Monoid.Monoid<Gauge> = Monoid.struct({
  runs: NumberInstances.MonoidSum,
  peak: Monoid.max(NumberInstances.Bounded),                  // the Bounded lift: zero rows fold to minBound, the extremum stays lawful over the empty collection
  spent: DurationInstances.MonoidSum,
})

const total = (rows: Iterable<Gauge>): Gauge => Gauge.combineAll(rows)

const ledger: Monoid.Monoid<{ readonly [realm: string]: number }> =
  RecordInstances.getMonoidUnion(NumberInstances.MonoidSum)   // keyed union merge: present-in-one keeps, present-in-both sums, the empty record is the identity

// --- [EXPORTS] --------------------------------------------------------------------------

export { Gauge, Tally, fold, ledger, total }
```

## [06]-[SCALAR_OWNERS]

A scalar invariant rides the owner whose operations are the domain's operations: instants are `DateTime`, spans are `Duration`, exact decimals are `BigDecimal`, and every partial numeric operation returns `Option` so absence folds once at the consumer instead of leaking as `NaN`, `Infinity`, or a throw.

[TIME_OWNERS]:
- Law: calendar arithmetic lives on `DateTime` — `DateTime.add`, `DateTime.addDuration`, `DateTime.subtractDuration`, `DateTime.startOf`, `DateTime.endOf`, `DateTime.nearest` move, `DateTime.distanceDuration` measures elapsed, `DateTime.Order` with `min`/`max`/`between`/`clamp` compares — so epoch-millisecond arithmetic never carries domain meaning and `Date` survives only at the FFI seam under `Order.Date`.
- Law: span policy is one value table — named `Duration` rows validated by `satisfies` without widening, constructed by `Duration.millis`/`seconds`/`minutes`/`hours` or the `Duration.decode` template forms, combined by `Duration.sum`/`times`/`subtract`, compared by `Duration.Order`/`lessThan`/`between` — and a raw millisecond literal re-derives units at every reader.
- Law: `DateTime.now` rides the rail as `Effect<Utc>`; `DateTime.unsafeNow`, `BigDecimal.unsafeFromNumber`, `Number.unsafeDivide`, and every `unsafe*` constructor is kernel vocabulary stating a proof obligation at the call site — outside a marked kernel the total or `Option`-returning form is the only legal spelling.

[EXACT_AND_PARTIAL_NUMBERS]:
- Law: exact decimals are `BigDecimal` — `make`/`fromString` construction, `sum`/`multiply`/`subtract`/`divide` arithmetic, `round` with an explicit `RoundingMode`, `normalize` for canonical form, `BigDecimal.Order` comparisons — and binary-float money math is the deleted spelling.
- Law: a partial operation returns `Option` — `Number.divide`, `Number.parse`, `BigDecimal.fromString`, `BigDecimal.divide`, `Duration.divide`, `DateTime.make` — and partiality composes: `Option.flatMap` chains partial construction and partial arithmetic into one absent-or-present pipeline, so a failed parse and a zero divisor collapse into the same fold at the consumer.
- Reject: `new Date()` and `Date.now()` in domain flow; `parseFloat`-then-`isNaN` ladders; an `Infinity` escaping a division; hand unit scaling between milliseconds and minutes.
- Boundary: wire admission of scalars — `Schema.DateTimeUtc`, decimal-string decode — is `shapes.md`'s and `boundaries.md`'s; `Schedule` policy consuming `Duration` is `rails-and-effects.md`'s; substituting the clock behind `DateTime.now` is `services-and-layers.md`'s Layer provision.

```typescript
import { BigDecimal, DateTime, Duration, Effect, Number, Option, pipe } from "effect"

const _grace = {                                              // span policy is one value table: satisfies validates rows without widening, no reader re-derives units
  retry: Duration.millis(250),
  lease: Duration.minutes(8),
  audit: Duration.hours(12),
} as const satisfies Record<string, Duration.Duration>

const window: Effect.Effect<{ readonly until: DateTime.Utc; readonly slack: Duration.Duration }> =
  Effect.gen(function* () {
    const now = yield* DateTime.now                           // the wall-clock read rides the rail; DateTime.unsafeNow is kernel vocabulary
    const until = DateTime.endOf(DateTime.addDuration(now, _grace.lease), "hour")
    return { until, slack: DateTime.distanceDuration(now, until) } // calendar moves and elapsed stay on the owners — epoch math never appears
  })

const unitPrice = (raw: string, batch: bigint): Option.Option<BigDecimal.BigDecimal> =>
  pipe(
    BigDecimal.fromString(raw),                               // partiality composes: a failed parse and a zero divisor collapse into one absent-or-present pipeline
    Option.flatMap((price) => BigDecimal.divide(price, BigDecimal.make(batch, 0))),
    Option.map(BigDecimal.round({ scale: 2, mode: "half-even" })),
  )

const share = (raw: string, total: number): number =>
  pipe(Number.parse(raw), Option.flatMap((count) => Number.divide(count, total)), Option.getOrElse(() => 0)) // the caller folds partiality once; no NaN exists to guard downstream

// --- [EXPORTS] --------------------------------------------------------------------------

export { share, unitPrice, window }
```

## [07]-[BYTES_AND_SECRETS]

Bytes and secrets are sealed at the text seam. The interior carries `Uint8Array` and sealed values; base64 and hex text exist only at the channel, and a secret never exists raw past its admission expression.

[SEALED_SEAMS]:
- Law: encode is total, decode is fallible — `Encoding.encodeBase64`, `encodeBase64Url`, and `encodeHex` accept `Uint8Array | string` and always succeed; `Encoding.decodeBase64`, `decodeBase64Url`, `decodeHex`, `decodeBase64String`, and `decodeHexString` return `Either<_, DecodeException>` so malformed text is a value at the seam — the asymmetry is the law, and the interior never sees undecoded text.
- Law: the secret lifecycle is structural — `Redacted.make` seals at admission, values travel and compare sealed through `Redacted.getEquivalence`, `Redacted.value` unwraps exactly once at the consuming boundary, and `Redacted.unsafeWipe` retires terminally: a wiped secret throws on `value`, so retirement is enforced by the carrier, and the wipe's boolean is its receipt. `Redacted` implements `Equal` and prints `<redacted>` on every string, JSON, and inspect channel — safety is construction, never discipline.
- Reject: `btoa`/`atob` and platform buffer conversions in domain flow; a raw string secret inside a shape, log, error, or policy record — the field is `Redacted` from admission; unwrap-and-compare; base64 text carried as the domain representation of bytes.
- Boundary: `Config.redacted` and `Schema.Redacted` admission are `boundaries.md`'s and `shapes.md`'s; lifting the decode `Either` onto the rail is `rails-and-effects.md`'s.

```typescript
import { Either, Encoding, Equivalence, Option, Redacted } from "effect"

const frame = (bytes: Uint8Array): string =>
  Encoding.encodeBase64Url(bytes)                              // encode is total; the interior carries Uint8Array, never base64 text pretending to be bytes

const digest = (hex: string): Option.Option<Uint8Array> =>
  Either.getRight(Encoding.decodeHex(hex))                     // decode is fallible: malformed text is a value at the seam, never a thrown atob

const admitKey = (wire: string): Either.Either<Redacted.Redacted<string>, Encoding.DecodeException> =>
  Either.map(Encoding.decodeBase64String(wire), Redacted.make) // sealed at admission: past this expression the secret never exists raw

const _sameKey: Equivalence.Equivalence<Redacted.Redacted<string>> = Redacted.getEquivalence(Equivalence.string)

const rotated = (live: Redacted.Redacted<string>, next: Redacted.Redacted<string>): boolean =>
  !_sameKey(live, next)                                        // compared sealed: policy reads never unwrap, and Redacted prints <redacted> on every accidental channel

// --- [EXPORTS] --------------------------------------------------------------------------

export { admitKey, digest, frame, rotated }
```
