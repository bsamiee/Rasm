# [TYPESCRIPT_VALUES]

This page is the value law: which runtime primitive owns an invariant, how identity is declared, and how comparison, combination, and scalar arithmetic compose as instance values. The doctrine names the Effect family as the domain default; this layer legislates the selection — the owner an invariant rides, the construction that implants identity, the algebra instance that carries policy — so every value decision is recoverable from a declaration, and JS stdlib survives only at the FFI seam and inside marked kernels. Everything around the value plane is shed by kind: vocabulary tables and the derivation algebra are `derivation.md`'s, Schema owners and admitted shapes are `shapes.md`'s, the carrier and in-flow `Option`/`Either` folds are `rails-and-effects.md`'s, dispatch over values is `surfaces-and-dispatch.md`'s, keyed accumulators threaded through incremental dataflow are `streams.md`'s, the decode seam that admits these values is `boundaries.md`'s, and the kernel exemption is `language.md`'s.

## [01]-[VALUE_ALGEBRA]

[VALUE_ALGEBRA]:
- Owner law: the invariant selects the owner — keyed state rides `HashMap`, membership `HashSet`, traversal order `SortedMap`, amortized growth and batch windows `Chunk`, literal-keyed rest shapes the plain record under `Record`/`Struct`/`Tuple` folds, linear transforms the `Array` module whose reads are total (`Option`-returning) and whose comparison-bearing operations take instances as parameters
- Identity law: identity is a construction fact — `Data.struct`, `Data.tuple`, and `Data.array` implant `Equal`/`Hash` to full depth, `Equal.equals` is the one domain equality, and keyed containers compare through it, so a keyable value is `Data`-constructed or primitive and one concept constructs through one channel
- Instance law: comparison, equivalence, and refinement are composed instance values — `Order`/`Equivalence`/`Predicate` build from shipped atoms through `mapInput`, `combine`, `struct`, and `tuple`, travel as parameters into `Array.sortBy`, `Array.dedupeWith`, and `SortedMap.empty`, and carry their derived operator family (`min`, `max`, `clamp`, `between`) with them
- Merge law: a domain combine is a `Semigroup`/`Monoid` instance — `Semigroup.struct` names each field's algebra as a row, any `Order` derives its extremum semigroup, and the identity decision is structural: a lawful `empty` lifts the fold to `Monoid.combineAll` over any collection, an identity-free algebra folds `combineMany` over a witnessed head
- Scalar law: instants are `DateTime`, spans are `Duration`, exact decimals are `BigDecimal`, and the arithmetic lives on the owner — calendar moves, distances, rounding, comparisons — so epoch numbers, `Date`, and binary-float money never carry domain meaning; the ambient clock read is rail material
- Partiality law: a partial operation returns `Option` — `Number.divide`, `Number.parse`, `BigDecimal.fromString`, `BigDecimal.divide`, `Duration.divide`, `DateTime.make` — and the caller folds absence as a value; `NaN`, `Infinity`, silent `undefined`, and the throwing parse are the deleted spellings

Treat the value plane as an algebra of owners and instances, never a bag of ad-hoc helpers. Replace a JS `Map`, an inline comparator, a hand merge function, an epoch subtraction, a NaN guard, or a raw secret string the moment the owning primitive or a composed instance carries the invariant.

## [02]-[CANONICAL_CHOOSER]

Each table routes a value invariant to the primitive that owns it, and every `[USE]` names the spelling it deletes. Each group routes to its `[03]` contract for the rule a row cannot state.

[COLLECTION_OWNER_FORMS]: which container owns a state invariant.

| [INDEX] | [CONCERN]                        | [USE]                                          | [REPLACE]                                |
| :-----: | :------------------------------- | :--------------------------------------------- | :---------------------------------------- |
|  [01]   | keyed domain state               | `HashMap` keyed by `Data` value or primitive   | JS `Map`, object-as-map mutation           |
|  [02]   | keyed read-merge-write           | `HashMap.modifyAt` `Option` fold               | `get`-then-`set` pair, spread rebuild      |
|  [03]   | membership                       | `HashSet`                                      | JS `Set`, `Array.includes` scan            |
|  [04]   | order-bearing keyed traversal    | `SortedMap` carrying its `Order`               | sort-on-read of map entries                |
|  [05]   | amortized growth, batch windows  | `Chunk`                                        | `push` accumulation, spread rebuild        |
|  [06]   | literal-keyed rest shape         | plain record under `Record`/`Struct` folds     | `HashMap` over a closed key set            |
|  [07]   | linear transform, total read     | `Array` module folds; `Option`-returning reads | method chains with inline comparators      |
|  [08]   | measured batch mutation          | `HashMap.mutate` / `MutableHashMap` kernel     | mutable map escaping as live state         |

[IDENTITY_FORMS]: how a value carries identity.

| [INDEX] | [CONCERN]                  | [USE]                                        | [REPLACE]                              |
| :-----: | :------------------------- | :------------------------------------------- | :-------------------------------------- |
|  [01]   | domain equality            | `Equal.equals` over `Data`-constructed values | `===`, `JSON.stringify` comparison      |
|  [02]   | record, tuple, array value | `Data.struct` / `Data.tuple` / `Data.array`  | plain literal expected to compare        |
|  [03]   | container key              | `Data`-constructed or primitive key           | plain object key missing on fresh build  |
|  [04]   | hash identity              | `Hash.hash` implied by the constructors       | hand hashCode, string-concat keys        |
|  [05]   | projection equality        | `Equivalence` instance                        | widening `Equal` to answer a projection  |

[INSTANCE_FORMS]: how comparison policy is declared.

| [INDEX] | [CONCERN]                | [USE]                                                | [REPLACE]                          |
| :-----: | :----------------------- | :--------------------------------------------------- | :---------------------------------- |
|  [01]   | order on a projection    | `Order.mapInput` onto a shipped atom                 | `(a, b) => a.x - b.x`               |
|  [02]   | lexicographic tie-break  | `Order.combine` / `Order.combineAll`                 | nested-ternary comparator           |
|  [03]   | record, positional order | `Order.struct` / `Order.tuple`                       | hand multi-field comparator         |
|  [04]   | inversion                | `Order.reverse`                                      | negated subtraction                 |
|  [05]   | comparison family        | `Order.min` / `max` / `clamp` / `between`            | ad-hoc ternaries per site           |
|  [06]   | projection equivalence   | `Equivalence.mapInput` / `struct` / `tuple` / `array` | delimiter-joined key strings        |
|  [07]   | composed refinement      | `Predicate.and` / `or` / `not` / `struct`            | boolean-soup conditions             |

[MERGE_FORMS]: how two values of one shape combine.

| [INDEX] | [CONCERN]                | [USE]                                                     | [REPLACE]                        |
| :-----: | :----------------------- | :--------------------------------------------------------- | :-------------------------------- |
|  [01]   | per-field record merge   | `Semigroup.struct` rows                                    | hand merge function                |
|  [02]   | positional merge         | `Semigroup.tuple`                                          | index juggling                     |
|  [03]   | numeric, duration atoms  | `@effect/typeclass/data/*` shipped instances               | re-authored `(a, b) => a + b`      |
|  [04]   | extremum by any order    | `Semigroup.min` / `Semigroup.max` over an `Order`          | `reduce` with a comparison ternary |
|  [05]   | keep-first, keep-last    | `Semigroup.first` / `Semigroup.last`                       | ordering hacks                     |
|  [06]   | separator fold           | `Semigroup.intercalate`                                    | join-with-map scatter              |
|  [07]   | fold with identity       | `Monoid.fromSemigroup` / `Monoid.struct`; `combineAll`     | `reduce` with a hand seed          |

[SCALAR_FORMS]: which owner carries a scalar invariant.

| [INDEX] | [CONCERN]           | [USE]                                                  | [REPLACE]                          |
| :-----: | :------------------ | :------------------------------------------------------ | :---------------------------------- |
|  [01]   | instant             | `DateTime`; `DateTime.now` on the rail                  | `new Date()`, `Date.now()`          |
|  [02]   | span                | `Duration` constructors / `Duration.decode`             | raw millisecond literals            |
|  [03]   | calendar arithmetic | `DateTime.add` / `addDuration` / `startOf` / `endOf`    | epoch-millisecond arithmetic        |
|  [04]   | elapsed             | `DateTime.distanceDuration`                             | `getTime()` subtraction             |
|  [05]   | instant comparison  | `DateTime.Order` / `min` / `max` / `between`            | `>=` on epoch numbers               |
|  [06]   | exact decimal       | `BigDecimal`                                            | binary-float money math             |
|  [07]   | fallible numeric    | `Number.divide` / `Number.parse` returning `Option`     | NaN guards, `parseFloat` checks     |

[BYTE_AND_SECRET_FORMS]: how bytes cross text channels and secrets stay sealed.

| [INDEX] | [CONCERN]          | [USE]                                                    | [REPLACE]                            |
| :-----: | :----------------- | :-------------------------------------------------------- | :------------------------------------ |
|  [01]   | binary-to-text     | `Encoding.encodeBase64` / `encodeBase64Url` / `encodeHex` | hand `btoa`, platform buffer calls    |
|  [02]   | text-to-binary     | `Encoding.decodeBase64` / `decodeHex` returning `Either`  | throwing `atob`, unchecked parse      |
|  [03]   | secret carrier     | `Redacted.make` at admission                              | raw string secret inside a shape      |
|  [04]   | secret comparison  | `Redacted.getEquivalence`                                 | unwrap-and-compare                    |
|  [05]   | secret retirement  | `Redacted.unsafeWipe` at the owning seam                  | secret left live for the process life |

## [03]-[VALUE_CONTRACTS]

Each contract fixes the selection and composition rule its chooser rows cannot state. Snippets compose finalized doctrine as supporting material; the spotlight is the value form itself, and each contract closes on the boundary that hands the value to its owning page.

[COLLECTION_OWNER_SITE]:
- Use when: domain state is keyed, member-tested, ordered, accumulated, or batch-transformed — the invariant selects the owner before any code is shaped.
- Accept: `HashMap` for keyed state, keyed by `Data`-constructed values or primitives; `HashMap.modifyAt` as the single keyed read-merge-write whose `Option -> Option` fold decides insert, update, and delete in one arm; `HashSet` membership; `SortedMap` when traversal order is the invariant — the `Order` arrives at construction, never at read; `Chunk` for amortized append and batch windows; `Array`/`Record`/`Struct`/`Tuple` module folds over plain shapes; `HashMap.mutate` or `MutableHashMap` batching writes inside one marked kernel with the draft never escaping.
- Reject: JS `Map`/`Set` in domain flow — reference-keyed, so structurally equal keys miss and composite keys degrade into hand-joined strings; object-as-map mutation; a `get`-then-`set` pair restating `modifyAt`; `Array.includes` scans where `HashSet` owns membership; sort-on-read where `SortedMap` owns order; `HashMap` over a closed literal key set — that shape is a plain record whose key space is derivation material.
- Law: the fold is the write surface — a keyed accumulator builds through `Array.reduce` over `modifyAt`, persistent structural sharing keeps the fold allocation-honest, and no intermediate map escapes mid-fold.
- Boundary: a keyed accumulator threaded through incremental dataflow is `streams.md`'s; decoded collection admission (`Schema.HashMap`, `Schema.Chunk`) is `shapes.md`'s; the kernel mark is `language.md`'s.

```ts conceptual
import { Array, Data, HashMap, Number, Option } from "effect"

export type Pulse = { readonly realm: string; readonly lane: string; readonly load: number }
export type Meter = { readonly count: number; readonly peak: number }

export const meter = (pulses: ReadonlyArray<Pulse>): HashMap.HashMap<readonly [string, string], Meter> =>
  Array.reduce(pulses, HashMap.empty<readonly [string, string], Meter>(), (acc, pulse) =>
    HashMap.modifyAt(acc, Data.tuple(pulse.realm, pulse.lane), (slot) => // one keyed read-merge-write: the Option fold owns insert and update; Option.none() would delete the key
      Option.some(Option.match(slot, {
        onNone: () => ({ count: 1, peak: pulse.load }),
        onSome: (row) => ({ count: row.count + 1, peak: Number.max(row.peak, pulse.load) }),
      }))))

export const read = (meters: HashMap.HashMap<readonly [string, string], Meter>, realm: string, lane: string): Option.Option<Meter> =>
  HashMap.get(meters, Data.tuple(realm, lane))               // a fresh key retrieves: lookup rides Equal.equals, so a plain literal key would miss every time
```

[STRUCTURAL_IDENTITY_SITE]:
- Use when: a value participates in equality, dedup, keying, or memo identity — identity is declared where the value is built, never computed where it is compared.
- Accept: `Data.struct`, `Data.tuple`, and `Data.array` implanting `Equal` and `Hash` at construction; nesting — members are `Data`-constructed so depth compares structurally; `Equal.equals` as the one equality with `Hash.hash` its container-facing shadow; `HashSet` and `HashMap` consuming both so dedup and retrieval are structure-decided.
- Reject: `===` between domain values — reference identity asked of structural data; `JSON.stringify` comparison — key-order-dependent and allocating per probe; a hand deep-equality function; a plain literal as a container key; mixing `Data`-constructed and plain-constructed values of one concept — identity splits down the middle and half the concept misses.
- Law: `Equal.equals` implies hash agreement — the container contract; equality that arrives anywhere but the constructors desynchronizes from `Hash` and breaks retrieval silently.
- Law: projection equality is not identity — "same key" is an `Equivalence` instance question, and widening `Equal` to answer it poisons every container keyed on the concept.
- Boundary: tagged families and class owners (`Data.taggedEnum`, `Schema.Class`, `Data.Class` heritage) implant the same identity at their declaration — the owner forms are `shapes.md`'s and `language.md`'s; the `Equivalence` algebra is this page's instance contract.

```ts conceptual
import { Array, Data, Equal, Hash, HashSet } from "effect"

export type Facet = { readonly axis: string; readonly grade: number }
export type Mark = { readonly key: string; readonly facets: ReadonlyArray<Facet> }

export const mark = (key: string, facets: ReadonlyArray<Facet>): Mark =>
  Data.struct({ key, facets: Data.array(Array.map(facets, Data.struct)) }) // identity is implanted at construction, to full depth: nested members are Data-constructed too

const _one = mark("<value-a>", [{ axis: "<axis-a>", grade: 3 }])
const _two = mark("<value-a>", [{ axis: "<axis-a>", grade: 3 }])

export const same: boolean = Equal.equals(_one, _two)        // true — structure decides; _one === _two is false and never asked
export const aligned: boolean = Hash.hash(_one) === Hash.hash(_two) // the contract Equal implies: equal values hash equal — what every keyed container consumes
export const distinct: HashSet.HashSet<Mark> = HashSet.fromIterable([_one, _two]) // size 1: the set collapses structural duplicates
```

[ALGEBRA_INSTANCE_SITE]:
- Use when: ordering, equivalence, or refinement carries domain policy — the policy is one composed instance value every consumer shares.
- Accept: `Order` composed from atoms — `Order.mapInput` projects onto `Order.number`/`Order.string`/`Order.bigint`, `Order.reverse` inverts, `Order.combine`/`Order.combineAll` chain lexicographic tie-breaks, `Order.struct`/`Order.tuple` assemble record and positional orders; the derived family riding the instance — `Order.min`, `Order.max`, `Order.clamp`, `Order.between`, `Order.lessThan`; `Equivalence.struct`/`tuple`/`array`/`mapInput` for projection equality; `Predicate.and`/`or`/`not`/`struct` composing refinements, where `Predicate.struct` over refinements narrows.
- Reject: `(a, b) => a.x - b.x` — subtraction comparators are NaN-blind and restate policy per site; nested-ternary comparators `Order.combine` owns; delimiter-joined key strings as equivalence; boolean-soup conditions the predicate algebra composes; a per-site re-derivation of min, max, or clamp the instance already carries.
- Law: instances are contravariant material — `mapInput` builds a projection instance in one line, and a record instance accepts every richer shape by parameter contravariance, so no mirror instance per consumer type exists.
- Law: the instance anchors its operator family — `Order.max(rank)` is the domain's binary max and `Order.clamp(rank)` its band; one instance edit repoints every derived operator at once.
- Boundary: an `Order` consumed by a merge is the merge contract's; `Equivalence` as a memo key and the `Schema.equivalence` derivation ride `rails-and-effects.md` and `shapes.md`.

```ts conceptual
import { Array, Equivalence, Order, Predicate, pipe } from "effect"

export type Probe = { readonly realm: string; readonly grade: number; readonly load: number }

const _byGrade = Order.mapInput(Order.reverse(Order.number), (probe: Probe) => probe.grade)
const _byRealm = Order.mapInput(Order.string, (probe: Probe) => probe.realm)

export const rank: Order.Order<Probe> = Order.combine(_byGrade, _byRealm) // one lexicographic policy value: grade descending, realm tie-break — every consumer shares it

export const alike: Equivalence.Equivalence<Probe> = Equivalence.struct({ realm: Equivalence.string, grade: Equivalence.number }) // the two-field equivalence accepts Probe by contravariance: projection equality, distinct from identity

const _live: Predicate.Predicate<Probe> = Predicate.struct({ grade: (grade: number) => grade > 0, load: (load: number) => load > 8 })

export const admit: Predicate.Predicate<Probe> = Predicate.and(_live, Predicate.not((probe: Probe) => probe.realm === "<realm-a>")) // refinement composes as values, never boolean soup

export const lead = (probes: ReadonlyArray<Probe>): ReadonlyArray<Probe> =>
  pipe(probes, Array.filter(admit), Array.dedupeWith(alike), Array.sortBy(rank), Array.take(4)) // instance-parameterized consumption: the policy travels, the operation stays polymorphic

export const cap = (probe: Probe, floor: Probe, ceiling: Probe): Probe =>
  Order.clamp(rank)(probe, { minimum: floor, maximum: ceiling }) // derived operators arrive with the instance: clamp/min/max/between are projections of rank
```

[MERGE_ALGEBRA_SITE]:
- Use when: two values of one shape combine — meters, tallies, settings overlays, receipt roll-ups — and the combine semantics must be recoverable from one declaration.
- Accept: `Semigroup.struct` and `Semigroup.tuple` composing per-field algebras; shipped atoms from `@effect/typeclass/data/Number`, `data/Duration`, `data/String`, and `data/Boolean`; `Semigroup.min`/`Semigroup.max` deriving the extremum semigroup from any `Order`; `Semigroup.first`/`Semigroup.last` keep policies; `Semigroup.intercalate` separator folds; `Monoid.fromSemigroup` where a lawful `empty` exists, `Monoid.struct`/`Monoid.tuple`, and the `Bounded` lifts `Monoid.min`/`Monoid.max`; `combineAll` folding any collection, `combineMany` folding a witnessed head plus rest; `Record.union(left, right, instance.combine)` as the keyed overlay of two partial records — present-in-one keeps, present-in-both combines.
- Reject: a hand merge function restating field policy inline; `reduce` with a hand seed where a `Monoid` owns the identity; a sentinel `empty` forged for an identity-free algebra — `first`, `last`, and `intercalate` admit no lawful identity, so their folds stay non-empty; a parallel merge helper per call site.
- Law: the struct instance is the merge table — one row per field algebra, every merge semantic recoverable from the declaration, and growth is a row, never a second function.
- Law: the fold signature encodes the identity decision — `(rows)` through `combineAll` where every row is lawful, `(head, rest)` through `combineMany` where any row is identity-free; promoting the second to the first forges data.
- Boundary: applicative error accumulation on the rail is `rails-and-effects.md`'s; a merge folded across a stream window is `streams.md`'s.

```ts conceptual
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as DurationInstances from "@effect/typeclass/data/Duration"
import * as NumberInstances from "@effect/typeclass/data/Number"
import * as StringInstances from "@effect/typeclass/data/String"
import { Duration } from "effect"

export type Tally = {
  readonly runs: number
  readonly peak: number
  readonly spent: Duration.Duration
  readonly slowest: Duration.Duration
  readonly origin: string
  readonly trace: string
}

const _merge = Semigroup.struct({                            // the merge table: one row per field algebra — six merge semantics, one declaration; growth is a row
  runs: NumberInstances.SemigroupSum,
  peak: NumberInstances.SemigroupMax,
  spent: DurationInstances.SemigroupSum,
  slowest: Semigroup.max(Duration.Order),                    // extremum over any Order: the Order is the only requirement, no shipped instance needed
  origin: Semigroup.first<string>(),
  trace: Semigroup.intercalate(StringInstances.Semigroup, "/"),
})

export const fold = (head: Tally, rest: Iterable<Tally>): Tally => _merge.combineMany(head, rest) // first and intercalate admit no lawful empty, so the fold demands a witnessed head — a sentinel empty forges data

export type Gauge = Pick<Tally, "runs" | "peak" | "spent">

const _gauge = Monoid.struct({
  runs: NumberInstances.MonoidSum,
  peak: Monoid.max(NumberInstances.Bounded),                 // the Bounded lift: empty is minBound, so max over zero rows stays lawful
  spent: DurationInstances.MonoidSum,
})

export const total = (rows: Iterable<Gauge>): Gauge => _gauge.combineAll(rows) // every row carries a lawful identity: the empty collection folds to it, zero special-casing
```

[SCALAR_OWNER_SITE]:
- Use when: a value is an instant, a span, or an exact decimal, or a numeric operation can fail — the scalar owner carries the arithmetic, the calendar, the precision, and the comparisons.
- Accept: `DateTime.now` on the rail and `DateTime.make` returning `Option`; calendar arithmetic on the owner — `DateTime.add`, `DateTime.addDuration`, `DateTime.subtractDuration`, `DateTime.startOf`, `DateTime.endOf`, `DateTime.nearest`; `DateTime.distanceDuration` for elapsed; `DateTime.Order`, `DateTime.min`/`max`/`between` comparisons; `Duration` policy values through the constructors and `Duration.decode`, combined by `Duration.sum`/`times`/`subtract` and compared by `Duration.lessThan` and `Duration.Order`; `BigDecimal.make`/`fromString` construction, `sum`/`multiply`/`subtract`/`divide` arithmetic, `round` with an explicit `mode`, `normalize`, and `BigDecimal.Order` for exact decimal domains; `Number.divide`/`parse`/`max`/`min`/`clamp`/`sumAll` module operations.
- Reject: `new Date()`, `Date.now()`, and epoch-millisecond arithmetic in domain flow — `Date` survives only at the FFI seam under `Order.Date`; raw millisecond literals that re-derive units at every reader; binary-float money math; `parseFloat`-then-`isNaN` ladders; hand unit scaling.
- Law: the `unsafe*` constructors are kernel vocabulary — `unsafeNow`, `unsafeFromNumber`, `unsafeDivide` state a proof obligation at the call site, so outside a marked kernel the total or `Option`-returning form is the only legal spelling.
- Law: partiality composes — `Option.flatMap` chains partial constructions and partial arithmetic into one absent-or-present pipeline, so a failed parse and a zero divisor collapse into the same fold at the consumer.
- Boundary: wire admission of scalars (`Schema.DateTimeUtc`, decimal-string decode) is `shapes.md`'s and `boundaries.md`'s; `Schedule` policy consuming `Duration` and the `Clock` capability behind `DateTime.now` are `rails-and-effects.md`'s.

```ts conceptual
import { BigDecimal, DateTime, Duration, Effect, Number, Option, pipe } from "effect"

export const Grace: { readonly retry: Duration.Duration; readonly lease: Duration.Duration } = {
  retry: Duration.decode("250 millis"),                      // policy spans are Duration values; a raw millisecond literal re-derives units at every reader
  lease: Duration.minutes(8),
}

export const unitPrice = (raw: string, batch: bigint): Option.Option<BigDecimal.BigDecimal> =>
  pipe(
    BigDecimal.fromString(raw),                              // exact-decimal partiality: fromString and divide return Option — a zero divisor is an absent quotient, never Infinity or a throw
    Option.flatMap((price) => BigDecimal.divide(price, BigDecimal.make(batch, 0))),
    Option.map(BigDecimal.round({ scale: 2, mode: "half-even" })),
  )

export const rate = (hits: number, total: number): number =>
  Option.getOrElse(Number.divide(hits, total), () => 0)      // the caller folds partiality explicitly; NaN never exists to propagate

export const window: Effect.Effect<{ readonly until: DateTime.Utc; readonly slack: Duration.Duration }> =
  Effect.gen(function* () {
    const now = yield* DateTime.now                          // the wall-clock read rides the rail; DateTime.unsafeNow is kernel material
    const until = DateTime.addDuration(now, Grace.lease)
    return { until, slack: DateTime.distanceDuration(now, until) } // calendar arithmetic and elapsed stay in the scalar owners — epoch math never appears
  })
```

[BYTES_AND_SECRETS_SITE]:
- Use when: binary crosses a text channel or a value must never print.
- Accept: `Encoding.encodeBase64`, `Encoding.encodeBase64Url`, and `Encoding.encodeHex` as total encodes over `Uint8Array | string`; `Encoding.decodeBase64`, `decodeBase64Url`, `decodeHex`, `decodeBase64String`, and `decodeHexString` returning `Either<_, DecodeException>`; `Redacted.make` sealing at admission; `Redacted.getEquivalence` comparing sealed; `Redacted.value` exactly once at the consuming boundary; `Redacted.unsafeWipe` terminal retirement — a wiped secret throws on `value`, so retirement is structural, not advisory.
- Reject: `btoa`/`atob` and platform buffer conversions in domain flow; a raw string secret inside a shape, log, error, or policy record — the field is `Redacted` from admission; unwrap-and-compare; base64 text carried as the domain representation of bytes — the interior stores `Uint8Array` and text exists only at the channel.
- Law: encode is total, decode is fallible — the asymmetry is the law; the malformed-text fault is a value at the seam and the interior never sees undecoded text.
- Law: the secret lifecycle is sealed, carried and compared sealed, unwrapped once at the consuming seam, wiped at retirement — `Redacted` implements `Equal` and prints `<redacted>` on every string, JSON, and inspect channel, so safety is construction, never discipline.
- Boundary: `Config.redacted` and `Schema.Redacted` admission are `boundaries.md`'s and `shapes.md`'s; lifting the decode `Either` onto the rail is `rails-and-effects.md`'s.

```ts conceptual
import { Either, Encoding, Equivalence, Option, Redacted } from "effect"

export const frame = (bytes: Uint8Array): string => Encoding.encodeBase64Url(bytes) // encode is total; the interior carries Uint8Array, never a base64 string pretending to be bytes

export const digest = (hex: string): Option.Option<Uint8Array> =>
  Either.getRight(Encoding.decodeHex(hex))                   // decode is fallible: the malformed-text fault is a value at the seam, never a thrown atob

export const admitKey = (wire: string): Either.Either<Redacted.Redacted<string>, Encoding.DecodeException> =>
  Either.map(Encoding.decodeBase64String(wire), Redacted.make) // sealed at admission: past this expression the secret never exists raw

const _sameKey = Redacted.getEquivalence(Equivalence.string)

export const rotated = (live: Redacted.Redacted<string>, next: Redacted.Redacted<string>): boolean =>
  !_sameKey(live, next)                                      // compared sealed: policy reads never unwrap, and Redacted prints as <redacted> on every accidental channel
```

## [04]-[ABSTRACTION_COLLAPSE_TESTS]

Use these tests before keeping a form the value layer already owns.

[STDLIB_RESIDUE]:
- Smell: a `Map`, `Set`, `Date`, epoch number, `btoa`, or float money value carries domain meaning, or a domain shape stores base64 text as its byte representation.
- Collapse: route to the owning primitive — `HashMap`/`HashSet`, `DateTime`/`Duration`, `BigDecimal`, `Encoding` — and pin the stdlib value at its FFI seam or marked kernel.
- Done when: stdlib values appear only at FFI seams and inside marked kernels, and every domain invariant names its Effect owner.

[IDENTITY_BLUR]:
- Smell: `===` or `JSON.stringify` compares domain values, a plain literal keys a container, or one concept is built through both `Data` and plain construction.
- Collapse: construct through `Data.struct`/`Data.tuple`/`Data.array`, compare through `Equal.equals`, and route projection equality to an `Equivalence` instance.
- Done when: every compared or keyed value is `Data`-constructed or primitive and each concept constructs through one channel.

[COMPARATOR_SPRAWL]:
- Smell: inline comparator lambdas, repeated min/max/clamp ternaries, or delimiter-joined key strings restate one policy across sites.
- Collapse: one composed `Order`/`Equivalence`/`Predicate` instance, consumed by instance-parameterized operations and projected into its derived operator family.
- Done when: each comparison policy exists once as a named instance and no call site re-derives what the instance carries.

[MERGE_DRIFT]:
- Smell: a hand merge function restates per-field policy, a `reduce` carries a hand seed, or a forged empty pads an identity-free algebra.
- Collapse: `Semigroup.struct` rows over shipped atoms and `Order`-derived extrema; lift to `Monoid.combineAll` only where every row's identity is lawful.
- Done when: every combine names its algebra, the fold signature encodes the identity decision, and no sentinel value forges data.

[PARTIALITY_LEAK]:
- Smell: a NaN guard, an `isNaN` ladder, a throwing parse, or an `Infinity` escaping a division sits downstream of a numeric operation.
- Collapse: the `Option`-returning owners — `Number.divide`, `Number.parse`, `BigDecimal.fromString`, `BigDecimal.divide` — folded once at the consumer.
- Done when: no numeric path can produce `NaN`, `Infinity`, or silent `undefined` observable past its owning expression.
