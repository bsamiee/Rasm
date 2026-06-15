# Uniform Arm Foldability Monoid Boundary

[THE_CARRIER_ALGEBRA_IS_TWO_TIERED]:
- Foldability of a uniform-return dispatch is not one predicate but two graded by the carrier's algebra: an arm-return with a `Monoid` folds over ANY key collection including the empty one, an arm-return with only a `Semigroup` folds over a NON-EMPTY collection only — the dividing line is whether the carrier has an identity element, and the empty key list is exactly the input that demands one.
- `Foldable.combineMap(Foldable)(Monoid)` is the dispatch fold's real signature — `<A>(f: (a: A) => M) => (self) => M` — and it requires a full `Monoid<M>` because folding the empty collection must return `Monoid.empty`; the `Semigroup`-only carrier has no `combineMap` analogue over an arbitrary collection because there is no value to return when zero arms fire.
- The carrier whose `empty` cannot exist is the marker: `Semigroup.intercalate(S, separator)` builds a separator-joining combine that has NO `Monoid` counterpart because an identity that joins nothing with the separator is undefined — so a vocabulary whose aggregate interleaves a delimiter folds only over the non-empty key set, and the empty case is a structurally distinct branch the carrier forbids collapsing into the fold.
- The fold over a possibly-empty key collection therefore selects its combinator by the carrier tier, never by a guard: a `Monoid` carrier feeds `combineMap` and the empty list degrades to `empty`, a `Semigroup` carrier feeds a non-empty fold and the call site's input type is `Array.NonEmptyReadonlyArray<K>` — the emptiness contract rides the carrier and the key-collection type together, the absent identity surfaced as a type obligation, not a runtime length check.

```typescript
import { Array } from 'effect'
import { Foldable } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'
import type { Semigroup } from '@effect/typeclass/Semigroup'

const COST = {
    a: { weight: 3 },
    b: { weight: 5 },
    c: { weight: 0 }, // a new row extends both the empty-tolerant fold and the non-empty fold with zero combinator edits
} as const satisfies Record<string, { readonly weight: number }>

const total = Foldable.combineMap(Array.Foldable)(N.MonoidSum) // Monoid carrier: folds the empty key list to 0
const totalOf = (ks: ReadonlyArray<keyof typeof COST>): number => total(ks, (k) => COST[k].weight)
const reduceNonEmpty = (S: Semigroup<number>) => (ks: Array.NonEmptyReadonlyArray<keyof typeof COST>): number =>
    S.combineMany(COST[ks[0]].weight, Array.map(Array.tailNonEmpty(ks), (k) => COST[k].weight)) // Semigroup carrier: NonEmpty is the type-level price of the absent identity
```

[PRODUCT_ARM_RETURNS_DERIVE_THEIR_MONOID]:
- A struct- or tuple-shaped arm return folds without a hand-written combine because the carrier's `Monoid` is DERIVED from its components: `Monoid.struct({ col: M_col })` builds a `Monoid` over the whole row by combining each named column under its own `Monoid` and seeding `empty` per-column, so a dispatch whose arms each return `{ count: number; log: string }` folds through `Monoid.struct({ count: Number.MonoidSum, log: String.Monoid })` — the row aggregate is the product of the column aggregates, never an accumulator threaded by hand.
- `Monoid.tuple(...Ms)` is the positional dual — `[Monoid<A>, Monoid<B>] -> Monoid<[A, B]>` combining element-wise — so an arm returning a positional record (a sum-and-max pair, a count-and-concat tuple) folds through the tuple monoid, the `empty` being the tuple of component identities; the product structure of the return type IS the source of its foldability, and adding a column or slot extends the derived monoid with one more component entry.
- The derivation is total over product shapes and bounded by them: a carrier built from `Number.MonoidSum`, `String.Monoid`, `Array.getMonoid`, `Boolean.MonoidEvery`/`MonoidSome`, and nested `Monoid.struct` covers any record of monoidal columns, so the foldability of a uniform-arm dispatch over a rich row return is decided once at the row's column types — every column being monoidal makes the row monoidal, one non-monoidal column blocks the whole derivation.
- `Monoid.min`/`Monoid.max` lift a `Bounded` into a selection monoid whose `empty` is the bound — `min` seeds `maxBound`, `max` seeds `minBound` — so an arm-return aggregated by extremum folds through `Number.MonoidMax`/`MonoidMin` with the empty key list returning the neutral bound, and a column-as-discriminant aggregate (the worst severity, the highest rank across fired arms) is the bounded monoid over the column, never a `reduce` with a sentinel seed.

```typescript
import { Array } from 'effect'
import { Foldable, Monoid } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'
import * as B from '@effect/typeclass/data/Boolean'
import * as S from '@effect/typeclass/data/String'

const STEP = {
    open: { count: 1, halts: false, trace: '<glyph-a>' },
    work: { count: 2, halts: false, trace: '<glyph-b>' },
    close: { count: 1, halts: true, trace: '<glyph-c>' },
} as const satisfies Record<string, { readonly count: number; readonly halts: boolean; readonly trace: string }>

const rowMonoid = Monoid.struct({ count: N.MonoidSum, halts: B.MonoidSome, trace: S.Monoid }) // empty = { count: 0, halts: false, trace: '' } — the product of column identities
const summarize = Foldable.combineMap(Array.Foldable)(rowMonoid)
const over = (ks: ReadonlyArray<keyof typeof STEP>): { readonly count: number; readonly halts: boolean; readonly trace: string } => summarize(ks, (k) => STEP[k])
```

[ONE_TRAVERSAL_THREE_APPLICATIVES]:
- The single dispatch read `Table[k].run(arg)` scales from one key to a collection to a concurrent batch not through three entrypoints but through ONE traversal over the key collection's `Traversable`, the algebra swapped by the applicative supplied: `Array.Traversable.traverse(F)(keys, (k) => Table[k].run(arg))` is the universal form, and `F` selects pure aggregation, sequential effect, or concurrent effect from the identical arm read.
- The pure collection fold and the effectful batch are the SAME `traverse` under different `Applicative` instances: under the identity applicative `traverse` collapses to `map`-then-fold, under `Effect`'s applicative it threads the effect — so the bedrock's per-combinator split (`Array.map` for pure, `Effect.forEach` for effectful) is one `Traversable.traverse` whose `F` parameter is the only thing that changes, the dispatch surface untouched.
- `Foldable.combineMap` and `Traversable.traverse` are the value-aggregating and effect-threading faces of the one fold: a uniform-return arm with a `Monoid` aggregates through `combineMap`, the same arm returning an `Effect` threads through `traverse(Effect.Applicative)`, and the carrier's algebra (monoid for the aggregate, applicative for the effect) is the only parameter that distinguishes them — adding a vocabulary row extends both faces with zero edits because each reads the same key set and the same arm column.
- The trap is reaching for a plural-named dispatch entrypoint when the singular already composes: a `dispatchAll(keys)` beside `dispatch(key)` is the modal duplication the traversal deletes — the collection modality is `Array.Foldable`/`Array.Traversable` wrapping the singular `Table[k].run`, the batch modality is the same wrapped in an effect applicative, and the singular read is the only owner, the modalities its applicative arguments.

[THE_EFFECTFUL_LIFT_IS_TWO_MONOIDS]:
- An arm returning `Effect<A, E, R>` folds by lifting the SUCCESS carrier's algebra into the effect: `Applicative.getMonoid(Effect.getApplicative())(M): Monoid<Effect<A, E, R>>` lifts a `Monoid<A>` to a monoid over the effectful arm that combines successes under `M` and seeds `empty` with `Effect.succeed(M.empty)` — so an effectful uniform-return dispatch folds through the lifted monoid, the aggregate computed inside the effect with no manual `Effect.zipWith` chain.
- The lift is graded exactly as the pure carrier is: `Applicative.getMonoid` needs the full `Applicative` (it has the `Of`/`zero` to build `Effect.succeed(empty)` for the empty collection), while `SemiApplicative.getSemigroup(Effect.getSemiApplicative())(S): Semigroup<Effect<A, E, R>>` lifts only a `Semigroup<A>` and folds the effectful arms over a non-empty key collection — the same Monoid-vs-Semigroup empty boundary, now one level up inside the effect.
- The error channel is the SECOND carrier the lift threads, and its algebra is selected by `mode`: default short-circuits on the first failing arm (the success monoid never sees the rest), `mode: "validate"` accumulates every arm's failure as `Array<E>` on the error channel, so a foldable effectful dispatch carries a success monoid on `A` AND an error semigroup on `E` simultaneously — the aggregate of successes and the accumulation of failures are two folds over one traversal, the carrier's success algebra and the channel's `validate` mode independently chosen.
- The success-channel lift and the error-channel accumulation are duals selected by independence: a dependent fold where a later arm needs an earlier arm's success threads sequentially and short-circuits, an independent fold where arms are mutually disjoint accumulates errors under `validate` and aggregates successes under the lifted monoid — the carrier picks the algebra, the dependence picks short-circuit versus accumulate, neither a boolean flag.

```typescript
import { Array, Effect } from 'effect'
import { Applicative, Foldable } from '@effect/typeclass'
import { getApplicative } from '@effect/typeclass/data/Effect'
import * as N from '@effect/typeclass/data/Number'

declare const TABLE: Record<'a' | 'b' | 'c', { readonly run: (x: number) => Effect.Effect<number, string> }>

const effectSum = Applicative.getMonoid(getApplicative())(N.MonoidSum) // Monoid<Effect<number, string>>: combines successes by sum, empty = Effect.succeed(0)
const foldArms = Foldable.combineMap(Array.Foldable)(effectSum) // folds the effectful dispatch; empty key list returns Effect.succeed(0)
const aggregate = (ks: ReadonlyArray<'a' | 'b' | 'c'>, x: number): Effect.Effect<number, string> => foldArms(ks, (k) => TABLE[k].run(x))
const accumulated = (ks: ReadonlyArray<'a' | 'b' | 'c'>, x: number): Effect.Effect<ReadonlyArray<number>, ReadonlyArray<string>> =>
    Effect.validateAll(ks, (k) => TABLE[k].run(x)) // mode-validate dual: the SAME arm read, errors accumulate as Array<string> instead of short-circuiting
```

[CONCURRENCY_RIDES_THE_APPLICATIVE_SELECTION]:
- The concurrency degree of a batched dispatch is not a parameter of the fold but a parameter of the APPLICATIVE the traversal threads: `Effect.getApplicative(options?: ConcurrencyOptions)` returns a different applicative per `{ concurrency, batching }` bag, and `traverse(F)` consumes that applicative — so sequential, bounded, and unbounded batch dispatch over one vocabulary are three calls to `getApplicative` with the same `Table[k].run` arm, the policy a value selecting the instance, never a branched pipeline.
- `Concurrency` is the closed policy alphabet `number | "unbounded" | "inherit"`, so the dispatch's batch degree is itself a vocabulary value the applicative selection consumes — a numeric cap, the unbounded member, or the inheriting member — and the foldable dispatch carries its degree as this policy value on `getApplicative`, the cap riding the fold the same way a behavior table's columns ride the row; widening the dispatch from sequential to capped to unbounded is one literal swap, never a branched second pipeline.
- `reduceEffect(zero, combine, options)` is the effectful `combineMap` with the concurrency policy on the fold itself — `zero: Effect<Z, E, R>` is the monoidal identity lifted into the effect and `combine` is the carrier's combine — so an effectful aggregate over a key collection folds through `reduceEffect` seeded with `Effect.succeed(empty)` and the `concurrency` option threaded, the lifted monoid and the concurrency degree composed in one call.
- The applicative-selection lift composes the two carriers under one policy: `getApplicative({ concurrency })` produces the applicative whose `getMonoid` lift aggregates successes concurrently while the `mode` on the surrounding `all`/`validateAll` governs error accumulation — so a concurrent foldable dispatch is the success monoid lifted through a concurrency-parameterized applicative, the degree, the success algebra, and the error algebra three orthogonal policy choices over one arm read.

[DIVERGENT_RETURN_IS_THE_PRODUCT_COPRODUCT_BOUNDARY]:
- The reason a divergent-return dispatch is not foldable is structural, not stylistic: the uniform fold combines arm CONTENTS through an applicative-derived `Semigroup<F<A>>` from `SemiApplicative.getSemigroup`, which requires the contents to be one type `A` with a combine, while divergent arms returning `X` and `Y` form a coproduct `X | Y` whose only `Semigroup<F<X | Y>>` is `SemiCoproduct.getSemigroup` — and that combine SELECTS a branch (`coproduct: (F<X>, F<Y>) => F<X | Y>`, left-biased), it never aggregates, so a coproduct "fold" is a fallback or first-present selection, not an aggregate.
- The two `getSemigroup` derivations are the exact fork: `SemiApplicative.getSemigroup(F)(S)` combines contents under the supplied `S` and keeps the type `F<A>` (the aggregate), `SemiCoproduct.getSemigroup(F)()` takes NO content semigroup and widens to `F<A | B>` by choosing one side (the alternative) — a uniform-return dispatch rides the applicative semigroup because its arms share `A`, a divergent-return dispatch can only ride the coproduct semigroup, which proves the divergent fold is structurally a selection, never a sum.
- The empty boundary mirrors the same fork: the applicative path's identity is the lifted `M.empty` wrapped in the carrier (the neutral aggregate), the coproduct path's identity is `Coproduct.zero<A>()` — `Option.none` / the always-losing alternative seeded at `never` — so the divergent dispatch's only monoid over the empty collection returns the empty alternative, confirming it carries no aggregate to seed and the "fold" is a first-success selection, not a summary.
- This is the identical boundary that forces the surface to `Match`: a coproduct must be ELIMINATED before it can be aggregated — `Match.exhaustive` over the union collapses `X | Y` to a single carrier per branch, after which the now-uniform results fold through the applicative semigroup — so the dispatch direction is fixed by the carrier's product-versus-coproduct structure, uniform product returns fold through `combineMap`/`traverse`, divergent coproduct returns are eliminated by `Match` first and folded second, never aggregated directly.

```typescript
import { Option } from 'effect'
import { getSemigroup as applicativeSemigroup } from '@effect/typeclass/SemiApplicative'
import { getSemigroup as coproductSemigroup } from '@effect/typeclass/SemiCoproduct'
import { SemiApplicative, SemiCoproduct } from '@effect/typeclass/data/Option'
import { SemigroupSum } from '@effect/typeclass/data/Number'

const aggregateArms = applicativeSemigroup(SemiApplicative)(SemigroupSum) // Semigroup<Option<number>>: combines CONTENTS by sum, type stays Option<number> — the uniform fold
const selectArm = coproductSemigroup(SemiCoproduct)<unknown, unknown, unknown, number | string>() // Semigroup<Option<number | string>>: takes NO content semigroup, SELECTS a present branch, widens to the union — never an aggregate
const summed: Option.Option<number> = aggregateArms.combine(Option.some(3), Option.some(5)) // product path: one carrier, true aggregate (Option.some(8))
const chosen: Option.Option<number | string> = selectArm.combine(Option.some(3), Option.some('<value-a>')) // coproduct path: the divergent "fold" is branch-selection; Match must eliminate the union before any real aggregate
```
