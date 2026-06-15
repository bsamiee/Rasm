# Strength Promotion Pipeline

[ONE_LAW_REALIZED_BY_FOUR_IDENTITY_SOURCES]:
- Every promotion from a non-empty accumulation to a unit-bearing one is the SAME act — supply the one missing datum, a two-sided identity — differing only in WHERE the identity originates, and the four sources form a strict capability ladder: a literal passed at the declaration, a retarget of an existing unit through a projection, a bound read off the order witness, and a unit lifted into a carrier that owns one structurally.
- The single endpoint that every source funnels through is `Monoid.fromSemigroup(S, empty)`, whose body is `{ combine: S.combine, combineMany: S.combineMany, empty, combineAll: (c) => S.combineMany(empty, c) }` — it ADDS only `empty` and a `combineAll` that seeds the SAME `S.combineMany` with that identity, so the promoted monoid reuses the operand's exact fold and the only new fact is the seed. A second `Monoid.make` re-spelling `combine` beside an existing `Semigroup` is the duplication this deletes; the promotion keeps one fold and acquires one element.
- The four sources are not four functions but four ways of computing the second argument to one function: `fromSemigroup(S, literal)`, `fromSemigroup(Semigroup.imap(S, to, from), to(M.empty))`, `fromSemigroup(Semigroup.min(B.compare), B.maxBound)`, and `Applicative.getMonoid(F)(M) = fromSemigroup(getSemigroup(F)(M), F.of(M.empty))` — read the second argument and the identity source is named.
- A promotion is provably weaker→stronger because the operand cannot exhibit the new datum and the output declares it: `getOptionalMonoid(S: Semigroup<A>): Monoid<Option<A>>` takes a unit-less algebra and returns a unit-bearing one over a DIFFERENT carrier, the input-output strength gap disclosing that the carrier, not the parameter, contributes the identity.

[SOURCE_ONE_LITERAL_IDENTITY_IS_THE_DECLARATION_TIME_SEED]:
- The weakest source supplies the identity as a raw value the author asserts is two-sided: `fromSemigroup(SemigroupSum, 0)`, `fromSemigroup(SemigroupEvery, true)`, `fromSemigroup(SemigroupXor, false)` — the concrete numeric and lattice monoids every consumer imports are literally this call, so the shipped `MonoidSum`/`MonoidEvery` are not primitive instances but promotions frozen as consts.
- The literal is the UNCHECKED premise the type system cannot guard: `combineAll([])` returns the literal and `combineAll([a])` returns `S.combineMany(literal, [a])`, both correct iff the literal is genuinely two-sided, so a near-identity that is left-neutral only (`0` for a `combine` that is `(a, _) => a + b` would pass) corrupts only multi-element folds — the trap a singleton unit test never surfaces because `combineAll([a])` exercises the seed on exactly one side.
- The literal source is the only one where authorship, not derivation, places the datum, so it is the only source where the identity can be wrong while every other layer is lawful — the higher sources foreclose this by computing the identity from a structure that cannot supply a wrong one.

```typescript
import { Monoid, Semigroup } from '@effect/typeclass'

type Span = { readonly lo: number; readonly hi: number }
const SpanS: Semigroup.Semigroup<Span> = Semigroup.make((a, b) => ({ lo: Math.min(a.lo, b.lo), hi: Math.max(a.hi, b.hi) }))
const SpanM: Monoid.Monoid<Span> = Monoid.fromSemigroup(SpanS, { lo: Infinity, hi: -Infinity })
const covered: Span = SpanM.combineAll([{ lo: 3, hi: 7 }, { lo: -1, hi: 4 }])
const vacuous: Span = SpanM.combineAll([])
```

[SOURCE_TWO_RETARGET_DEMANDS_AN_EXPLICIT_IDENTITY_RELIFT]:
- `Semigroup.imap(S, to, from)` retargets the fold onto a wrapped type — `make((self, that) => to(S.combine(from(self), from(that))), …)` round-tripping every operand through `from`/`to` — but `Monoid` exposes NO `imap`, so promoting a retargeted accumulation to a unit-bearing one cannot reuse the retarget: it goes through `fromSemigroup(Semigroup.imap(M, to, from), to(M.empty))`, with the identity retargeted EXPLICITLY through `to`.
- The missing `Monoid.imap` is the API refusing an unproven identity: the bare `(to, from)` pair cannot guarantee `to(M.empty)` is the new two-sided identity unless `to`/`from` are mutual inverses on the relevant domain, so the author must re-state the identity through `to` and own the obligation that the wrap preserves neutrality — a clamping `to` that loses information silently breaks both associativity and the identity law on the retargeted carrier.
- This source is strictly stronger than the literal because the identity is COMPUTED (`to(M.empty)`) from the operand's own identity rather than asserted blind, yet strictly weaker than the carrier sources because the round-trip law is still an unchecked premise — the retarget propagates the parent's laws iff the projection is a true isomorphism, so a value object inherits its parent's algebra through one declared `(to, from)` only when the wrap is lossless.

[SOURCE_THREE_THE_BOUND_IS_THE_ONLY_LAWFUL_EXTREMUM_IDENTITY]:
- `Bounded.min(B) = fromSemigroup(Semigroup.min(B.compare), B.maxBound)` and `Bounded.max(B) = fromSemigroup(Semigroup.max(B.compare), B.minBound)`: the extremum monoid's identity is the LOSING bound — `maxBound` loses every minimum (`min(maxBound, x) = x`), `minBound` loses every maximum — so the identity law holds BY the bound's definition, read off the witness, never passed as a literal.
- This is a TWO-STEP promotion stacking sources: `Order<A>` first promotes to `Semigroup.min(O)`/`Semigroup.max(O)` (a unit-less last-extremum fold over the comparator), then that semigroup promotes to a `Monoid` whose identity comes from the bound — the comparator climbs to a semigroup, the bound supplies the unit the comparator alone cannot, so an unbounded order reaches the semigroup rung and stops.
- The strength boundary is materialized as a missing export: a `number` owner ships `Bounded = { compare, maxBound: Infinity, minBound: -Infinity }` so `MonoidMin = Bounded.min(B)` and `MonoidMax = Bounded.max(B)` exist, while a `bigint` owner ships `SemigroupMin`/`SemigroupMax` and `MonoidSum`/`MonoidMultiply` but NO `MonoidMin`/`MonoidMax` and NO `Bounded` — `bigint` is unbounded, has no `maxBound` to seed the extremum identity, and the absent monoid is the missing identity source made a compile-time fact rather than a runtime sentinel.
- The bound source is wrong-identity-PROOF in a way the literal is not: `fromSemigroup(Semigroup.min(O), 0)` over an order where `0` is not the maximum produces a fold that wrongly clamps at `0`, the exact corruption `Bounded.min` forecloses by demanding the witness that already carries the lawful bound — and `Bounded.reverse(B)` flips `compare` and swaps both bounds atomically, so `Monoid.max(Bounded.reverse(B))` equals `Monoid.min(B)`, an ascending and a descending extremum fold one declaration and its involution, identity included.

```typescript
import { Bounded, Monoid } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'

const Floor: Monoid.Monoid<number> = Bounded.min(N.Bounded)
const Ceiling: Monoid.Monoid<number> = Bounded.max(N.Bounded)
const Tallest: Monoid.Monoid<number> = Bounded.max(Bounded.reverse(N.Bounded))
const lowest: number = Floor.combineAll([4, 1, 9])
const highest: number = Ceiling.combineAll([4, 1, 9])
```

[SOURCE_FOUR_THE_CARRIER_SUPPLIES_THE_UNIT_THE_INNER_TYPE_LACKS]:
- The strongest source needs no identity argument at all because a carrier owns one structurally, and it splits by which carrier datum is the unit: `Option.getOptionalMonoid(S) = fromSemigroup(make((self, that) => isNone(self) ? that : isNone(that) ? self : some(S.combine(self.value, that.value))), none())` — `None` is a genuine two-sided identity by construction, so the optional carrier PROMOTES a bare `Semigroup<A>` to a `Monoid<Option<A>>` with the unit nowhere supplied by the caller.
- `Applicative.getMonoid(F)(M) = fromSemigroup(SemiApplicative.getSemigroup(F)(M), F.of(M.empty))` lifts BOTH layers: `getSemigroup(F)(S)` (requiring only `SemiApplicative`, no identity) builds the elementwise `combine` as `F.map(F.product(self, that), ([a, b]) => S.combine(a, b))`, then the FULL `Applicative` is needed precisely because the lifted identity is `F.of(M.empty)` — the `Of` half of `Product` is the seam where promotion to a lifted monoid lives, so a lifted `Semigroup` needs only `SemiApplicative` and a lifted `Monoid` needs `Applicative`, the strength gap encoded in which interface each lifter constrains.
- `Coproduct.getMonoid(F)() = { ...SemiCoproduct.getSemigroup(F)(), empty: F.zero(), combineAll: F.coproductAll }` is the value-FREE promotion: the recovery semigroup's `combine` IS `F.coproduct` and the unit IS the carrier's `zero`, so the factory takes no inner algebra — the contrast with `Applicative.getMonoid(F)(M)` is the sharpest cut of the source ladder, same result shape `Monoid<Kind<F,…,A>>`, but the applicative form takes an inner `Monoid` (its identity is `F.of(M.empty)`, a LIFTED inner unit) while the coproduct form takes nothing (its identity is `F.zero()`, a NATIVE carrier unit).
- The carrier source is the only one that adds a law the operand provably lacks: `getOptionalMonoid`'s input is a bare `Semigroup<A>` with no unit anywhere in `A`, and the output's unit lives entirely in the `Option` layer — the promotion does not find a hidden identity in `A`, it MOVES the algebra to a carrier that has one, which is why the result type changes carriers while the literal/bound sources keep the carrier and only add a value.

```typescript
import { Applicative, Coproduct } from '@effect/typeclass'
import * as EffectInstances from '@effect/typeclass/data/Effect'
import * as OptionInstances from '@effect/typeclass/data/Option'
import * as N from '@effect/typeclass/data/Number'
import { Effect, Option } from 'effect'

const Lifted = Applicative.getMonoid(EffectInstances.getApplicative({ concurrency: 'unbounded' }))<number, never, never, never>(N.MonoidMax)
const First = Coproduct.getMonoid(OptionInstances.Coproduct)<unknown, never, never, number>()
const peak: Effect.Effect<number> = Lifted.combineAll([Effect.succeed(2), Effect.succeed(9), Lifted.empty])
const winner: Option.Option<number> = First.combineAll([Option.none(), Option.some(7)])
const fallback: Option.Option<number> = First.combineAll([])
```

[THE_FACTORY_PARAMETER_LIST_IS_THE_SOURCE_SIGNATURE]:
- Read the second-stage parameter of a promotion factory and its identity source is named with no implementation read: a value parameter (`getOptionalMonoid(S)`, `Applicative.getMonoid(F)(M)`) means the unit is COMPUTED from a sub-algebra the caller supplies; a value-free `()` (`Coproduct.getMonoid(F)()`) means the unit is the carrier's NATIVE `zero`; a `Bounded` parameter (`Monoid.min(B)`) means the unit is READ off the order witness; a raw literal slot (`fromSemigroup(S, empty)`) means the author ASSERTS it.
- The two-stage shape of `Applicative.getMonoid(F)(M)` separates the two free data by source: stage one fixes the carrier (the layer that owns `of`), stage two fixes the inner value algebra (the layer that owns the content's `combine`/`empty`), so a swap of either stage reshapes the lifted monoid with no body edit — the carrier and the value algebra are orthogonal axes the factory absorbs as two parameters rather than re-deriving inside the fold.
- The ABSENCE of a promotion factory is the load-bearing fact: `data/Either` exports `SemiCoproduct`/`SemiAlternative` but NO `Coproduct`/`Alternative`/`getMonoid`, because a `Left` requires a witness `E` and there is no neutral failure value to serve as `zero`, so an empty Either alternation is uninhabitable and the missing factory makes that a type-level fact — `Either.coproduct(self, that) = isRight(self) ? self : that` and `coproductMany` exist (a non-empty recovery fold), but `coproductAll` cannot, so the promotion to a unit-bearing recovery monoid is foreclosed at exactly the rung where `Option` (whose `coproductAll = firstSomeOf`, empty case `None`) completes it.

[A_PROMOTION_NEVER_REPAIRS_AN_UNLAWFUL_OPERAND]:
- The lift composes laws, it does not synthesize them: `SemiApplicative.getSemigroup(F)(S)` is associative IFF `F.product` is associative AND `S.combine` is associative, so a non-associative inner `S` produces a defined-but-incoherent lifted semigroup even when `F` is a lawful applicative — the type checks, the fold runs, the result is wrong, and no signature records the violation.
- `Applicative.getMonoid(F)(M)` adds `empty = F.of(M.empty)`, lawful IFF `M.empty` is a two-sided identity AND `F.of(M.empty)` is the applicative unit for `product`: both layers must independently carry their unit for the lift to carry one, so a near-identity inner `M.empty` propagates upward as a near-identity lifted `empty` that corrupts only multi-element lifted folds — the same singleton-blind trap the literal source carries, now one layer up.
- Identity acquisition is ORTHOGONAL to the associativity the operand already had to satisfy, and the promotion touches only the former: `getOptionalMonoid` over a non-associative `S` yields a `Monoid<Option<A>>` whose `None`-identity is genuinely two-sided yet whose `Some ⊕ Some` step is exactly as non-associative as `S` — the carrier supplies a real unit and inherits a broken combine, so the strongest source cannot rescue the weakest axis, and a witness must clear associativity BEFORE any promotion is reached for, never as a side-effect of acquiring an identity.
- The promotion is therefore a one-way trust gate: the rung below must be lawful for the rung above to be, so the property test asserting associativity belongs at the leaf `Semigroup` admission, not at any `fromSemigroup` site — a promotion that re-checked the operand's law would be re-deriving a fact the source rung already owed, and the missing check is not an omission but the recognition that the lift consumes a proof it does not produce.

[STRUCT_PROMOTION_RECURSES_THE_FOUR_SOURCES_INTO_ONE_IDENTITY]:
- `Monoid.struct(fields)` is `fromSemigroup(Semigroup.struct(fields), empty)` where `empty` is assembled per-field as `{ [k]: fields[k].empty }`, and `Monoid.tuple(...elements)` is `fromSemigroup(Semigroup.tuple(...), elements.map(m => m.empty))` — the struct/tuple promotion does not introduce a fifth identity source, it COMPOSES the per-field promotions, reading each field's already-acquired `empty` and assembling the record identity field-wise.
- This makes the record identity correct by construction exactly when every field's identity was correct: a field promoted by `Bounded.max` contributes `minBound`, a field promoted by `fromSemigroup(S, 0)` contributes `0`, a field frozen by `Semigroup.first<K>()`/`last<K>()` cannot appear because those are unit-LESS (a side-projection has no neutral), so a struct monoid is buildable only when every column carries a unit — a discriminant field that must survive intact forces the struct to a `Semigroup`, the absent column-identity surfacing as the inability to promote the whole record.
- Adding a field to the owner adds one row to the `fields` argument and the assembled identity extends with zero hand-edits, where a hand-written `Monoid.make` would re-spell the entire `empty` record per field — the struct promotion is the chain-collapse at the identity layer: one owner declaration yields the field-wise `combine` (from `Semigroup.struct`) and the field-wise `empty` (from each field's promotion), and the next field is one provably-unit-bearing column, never a re-opened identity record.

```typescript
import { Monoid } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'
import * as B from '@effect/typeclass/data/Boolean'

const Tally = Monoid.struct({ hits: N.MonoidSum, peak: N.MonoidMax, clean: B.MonoidEvery })
const seed: (typeof Tally)['empty'] = Tally.empty
const summary = Tally.combineAll([{ hits: 1, peak: 7, clean: true }, { hits: 1, peak: 3, clean: false }])
```
