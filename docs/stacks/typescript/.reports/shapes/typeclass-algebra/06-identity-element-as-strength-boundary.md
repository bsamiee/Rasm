# Identity as the Strength-Boundary Discriminant

[ONE_BIT_PER_RUNG_AND_IDENTITY_IS_THE_ONLY_REIFIED_LAW]:
- The strength lattice is one bit of information per rung — does a two-sided identity exist for this operation over this carrier — and that bit is never a flag: it is materialized as which interface a witness inhabits. Four canonical gates ask the SAME question at four layers: `combineMany`-vs-`combineAll` of a value fold, `getSemigroup`-vs-`getMonoid` of a lifted fold, `SemiCoproduct`-vs-`Coproduct` of a choice fold, `SemiApplicative`-vs-`Applicative` of a product fold. One discriminant, four surfaces, never four independent capability tiers.
- Identity is structurally privileged among the laws a witness can carry: associativity, commutativity, and idempotence live entirely in the unchecked premise — the witness shape `{ combine, combineMany }` records none of them — while identity is the ONE law reified as a MEMBER, since `Monoid<A> extends Semigroup<A>` adds exactly `{ empty: A; combineAll }`. The boundary is checkable precisely because identity alone is a field the checker reads, where the other three are facts only a property test establishes.
- The collapse this buys is the deletion of every `Monoid` declaration sitting beside a `Semigroup`: a function constrained on `Semigroup<A>` accepts any `Monoid<A>` by heritage with zero adaptation, and a witness becomes a `Monoid` by acquiring one field, so the `Semi*`→full hierarchy is never two parallel owners — it is one owner plus an `extends` clause plus a single datum, the lattice rung reduced to a member.

[COMBINEMANY_AND_COMBINEALL_ARE_ONE_FOLD_AND_THE_EMPTY_CASE_NEEDS_A_VALUE_NOT_A_FEATURE]:
- `combineMany(self, collection)` and `combineAll(collection)` are the IDENTICAL left-fold differing only in seed: the non-empty form seeds with the runtime value `self`, the empty-admitting form with the static value `empty`. `Semigroup.make(combine)` defaults `combineMany` to `reduce(self, combine)(collection)` — seeded by `self` — so a semigroup folds any non-empty iterable but cannot decide the empty case, because the empty case must return a value and `self` is the only value a non-empty fold ever has. The empty fold is not a missing feature; it is a missing VALUE, and `empty` is exactly the value that lets the seed exist with no element to take it from.
- `Monoid.fromSemigroup(S, empty)` makes the split exact: its `combineAll` is `(c) => S.combineMany(empty, c)` — it reuses the operand's bulk fold verbatim and feeds it the one new datum, so the monoid `combineAll` IS the semigroup `combineMany` pre-seeded with the identity and nothing else. The promotion keeps one fold and acquires one element; a second `Monoid.make` re-spelling `combine` is the duplication this deletes.
- Choosing the rung reconciles the cardinality of the input against the algebra's strength at compile time: a fold over a guaranteed-non-empty stream takes `combineMany`, a fold over a possibly-empty collection takes `combineAll`, and a `Semigroup` having no `combineAll` makes "what does an empty fold return" a type error rather than a runtime question.

```typescript
import { Monoid, Semigroup } from '@effect/typeclass'

type Span = { readonly lo: number; readonly hi: number }
const Merge: Semigroup.Semigroup<Span> = Semigroup.make((a, b) => ({ lo: Math.min(a.lo, b.lo), hi: Math.max(a.hi, b.hi) }))
const Bounds: Monoid.Monoid<Span> = Monoid.fromSemigroup(Merge, { lo: Infinity, hi: -Infinity })

const nonEmpty: Span = Merge.combineMany({ lo: 3, hi: 7 }, [{ lo: -1, hi: 4 }])
const admitsEmpty: Span = Bounds.combineAll([])
```

[THE_REIFIED_LAW_IS_THE_ONE_ITS_PRESENCE_DOES_NOT_VERIFY]:
- `combineAll([])` returns `empty` UNCOMBINED and `combineAll([a])` returns `combineMany(empty, [a])` — the seed touches exactly one side of one combine — so a unit that is left-neutral but not right-neutral passes every singleton fold and every non-empty fold and corrupts ONLY a multi-element `combineAll` where `empty` lands at the head and a later element re-reads the right side. The defect is invisible to any test whose input is empty or a singleton, because in both cases the identity never sits between two real elements.
- This is the paradox the discriminant carries: identity is the one law reified as a field, yet the field's PRESENCE does not verify its own law. `fromSemigroup(S, leftNeutralOnly)` type-checks, every singleton test passes, and the corruption surfaces only when `combineAll` folds three or more elements — the checker confirms an `empty` of the right TYPE exists and asserts nothing about its two-sidedness, so the lattice bit being SET is necessary but not sufficient for the law it claims.
- The empty-fold property test is therefore irreplaceable AND distinct from the associativity test: associativity fails on a re-parenthesized triple of real elements, two-sidedness fails on `combine(a, empty)` and `combine(empty, a)` diverging — a left-neutral-only unit is associative AND type-correct, so only a test asserting `combine(a, empty) === a === combine(empty, a)` over arbitrary `a` admits the identity, and that test is the gate `fromSemigroup` consumes without re-running.

[PRODUCT_STRUCT_VERSUS_NONEMPTYSTRUCT_IS_THE_IDENTITY_GATE_ONE_FUNCTOR_LEVEL_UP]:
- The product hierarchy carries the identical discriminant, surfaced as the empty-record gate: `SemiProduct.nonEmptyStruct(F)` constrains its argument with `EnforceNonEmptyRecord<R> = keyof R extends never ? never : R`, so a zero-field applicative struct resolves to `never` and is a COMPILE error at the semi level — a `SemiProduct` cannot build the empty product because it has no seed for one. `Product.struct(F)` carries no such guard because `Product<F> extends SemiProduct<F>, Of<F>`: the `Of` half supplies the lift of the empty struct, exactly as `empty` supplies the seed `combineAll([])` needs.
- `Of.of: <A>(a: A) => Kind<F, unknown, never, never, A>` is the applicative-level identity element, so `nonEmptyTuple`/`nonEmptyStruct` are the `combineMany`-equivalents (a real first element seeds them) and `tuple`/`struct`/`productAll` are the `combineAll`-equivalents (`of` seeds them) — the same non-empty-vs-empty split the value fold makes, lifted to where the structure is the product rather than the accumulation. The empty product is `of` applied to the empty tuple, the value-fold's `empty` and the product-fold's `of` playing one role at two levels.
- `Applicative.getMonoid(F)(M) = fromSemigroup(getSemigroup(F)(M), F.of(M.empty))` makes both identity layers visible in one expression: the lifted carrier monoid's `empty` is the inner value identity `M.empty` lifted through the applicative identity `of`, the product of the two units. `getSemigroup` requires only `SemiApplicative` because the lifted COMBINE needs no identity; `getMonoid` requires the full `Applicative` because the lifted EMPTY needs `of` — the precise rung where the lift demands the stronger carrier is exactly the rung where it acquires an identity.

```typescript
import { Product } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'

const build = Product.struct(ArrayInstances.Product)
const grid: ReadonlyArray<{ readonly tag: string; readonly weight: number }> = build({
    tag: ['<value-a>', '<value-b>'],
    weight: [1, 2],
})
const seededEmpty: ReadonlyArray<{}> = ArrayInstances.Product.productAll([])
```

[THE_CHOICE_GATE_AND_PRODUCT_GATE_SHARE_ONE_CHANNEL_NEUTRAL]:
- The choice hierarchy splits on the same bit: `Coproduct<F> extends SemiCoproduct<F>` adds `zero: <A>() => Kind<F, unknown, never, never, A>` and `coproductAll`, where `coproductMany` (seeded by `self`) is the choice-fold's `combineMany` and `coproductAll` (seeded by `zero`) is its `combineAll`. So the empty alternation needs `zero` exactly as the empty accumulation needs `empty` and the empty product needs `of` — three names for the seed the empty-admitting fold requires that the non-empty fold does not.
- `Coproduct.zero` and `Of.of` carry the IDENTICAL `unknown, never, never` channel triple, so the empty alternation and the empty product seed from one type-level neutral — the same identity object playing the value role announces zero requirement and zero failure, the channel arithmetic's unit and the value algebra's unit being one element. The discriminant bit at the value layer and the discriminant bit at the channel layer are the same bit read off the same `unknown, never, never` seed.
- The seed's origin is read off the export form with no implementation inspection: `Coproduct.getMonoid(F)()` takes a value-free `()` because the unit is the carrier's native `zero`, while `Applicative.getMonoid(F)(M)` takes the inner `Monoid` because the unit is the LIFTED inner identity `F.of(M.empty)` — same result shape `Monoid<Kind<F, R, O, E, A>>`, and the second-stage parameter list is the signature of where the identity lives.

```typescript
import { Coproduct } from '@effect/typeclass'
import * as OptionInstances from '@effect/typeclass/data/Option'
import { Option } from 'effect'

const FirstSome = Coproduct.getMonoid(OptionInstances.Coproduct)<unknown, never, never, number>()
const winner: Option.Option<number> = FirstSome.combineAll([Option.none(), Option.some(7)])
const fallback: Option.Option<number> = FirstSome.combineAll([])
```

[INTERCALATE_IS_THE_RUNG_THE_BIT_IS_UNSETTABLE_IN_PRINCIPLE]:
- `Semigroup.intercalate(S, sep) = make((self, that) => S.combineMany(self, [sep, that]))` injects `sep` between every pair, and `Monoid` exports no `intercalate` because a separating combine has NO two-sided identity: any candidate `e` forces `combine(e, a)` to emit `sep` before `a`, contradicting `combine(e, a) = a`. The discriminant bit here is not merely unset for a particular carrier — it is UNSETTABLE in principle, so the absent `Monoid.intercalate` encodes the slot associative-without-unit permanently, distinct from a carrier that merely happens to lack a bound.
- `Monoid.reverse(M)` is the contrast that CROSSES the boundary because the dual preserves the bit: swapping `combine`'s arguments keeps a two-sided identity two-sided, so `Monoid` exposes `reverse` while it cannot expose `intercalate`. Which combinators a `Monoid` exports beyond `Semigroup` is the executable map of which operations preserve the unit — argument-flip preserves it, separator-injection destroys it, and the export surface records exactly that.
- Intercalation is the witness whose mere existence proves the `Semigroup`/`Monoid` split is a genuine lattice level: `first`/`last`/`constant` are degenerate but conceivably unit-bearing under some other algebra, whereas intercalation's shape forbids a unit for any non-empty separator over any type — the library refusing a default `empty` here is the boundary asserted as structural, not as a convenience tier it could have collapsed.

```typescript
import { Semigroup } from '@effect/typeclass'
import * as S from '@effect/typeclass/data/String'

const Joined = Semigroup.intercalate(S.Semigroup, ', ')
const list: string = Joined.combineMany('<value-a>', ['<value-b>', '<value-c>'])
const Reversed = Semigroup.reverse(S.Semigroup)
const suffixed: string = Reversed.combine('<value-a>', '<value-b>')
```

[FIRST_IS_UNIT_LESS_BY_PROJECTION_AND_CONSTANT_IS_AN_ABSORBER_WEARING_THE_SHAPE]:
- `Semigroup.first() = make(a => a)` and `Semigroup.last() = make((_, b) => b)` are the lawful no-merge instances — associative, non-commutative, idempotent-on-equal-inputs — and unit-LESS by construction: a projection always keeping one operand has no `e` with `first(e, a) = a` for every `a`, since that would force `e` to vanish. They are the column a discriminant field takes when its value must survive a fold intact, and their unit-lessness is why a struct column frozen by `first`/`last` denies the whole record an identity rather than supplying a wrong one.
- `Semigroup.constant(a) = make(() => a)` ignores BOTH operands and is the INVERSE degeneracy of a monoid: `combine(a, a) = a` but `combine(a, x) = a` discards `x`, so `a` is an ABSORBING element — it annihilates rather than preserves, the algebraic opposite of a neutral. The discriminant bit reads zero for `constant` not because the identity is missing but because the only candidate is an absorber, the role inverted.
- `last`'s bespoke `combineMany` loops the collection keeping the final element rather than left-folding `combine`, so `last` ships its own bulk operation while remaining unit-less — proof that the `combineMany` rung (non-empty) is reachable by an instance the `combineAll` rung (empty) provably is not. The no-merge instances live at exactly the rung the boundary stops them at, the strongest demonstration that the split is a level an instance can sit ON rather than a tier it must clear.

[THE_LOSING_BOUND_IS_THE_ONLY_EXTREMUM_IDENTITY_AND_THE_INVOLUTION_RELOCATES_IT]:
- The extremum monoid reads its identity off the LOSING bound, so the only correct unit is structurally determined by the order: `Bounded.min(B)` seeds with `B.maxBound` because `min(maxBound, x) = x` — the maximum loses every minimum — and `Bounded.max(B)` seeds with `B.minBound`. The identity law holds BY the bound's definition, so where a raw literal source can assert a wrong unit, the `Bounded` witness makes a wrong extremum identity unrepresentable — the bit is set BY the witness, not by the author.
- `Bounded.reverse(B)` flips the order and swaps both bounds atomically, so the losing bound MOVES to the new winning position: `Monoid.max(Bounded.reverse(B))` equals `Monoid.min(B)`, identity included. An ascending and a descending extremum fold are one bounded declaration and its involution, both identity-correct by construction — the discriminant bit travels with the bound it is read from, never re-asserted at the dual.
- `Bounded.between(B)` and `Bounded.clamp(B)` derive a membership predicate and a saturating projection from the same `{ compare, maxBound, minBound }`, so the bound that supplies the extremum identity also supplies the range and the clamp — one witness, and the extremum's lawful seed is the same datum the range test and the saturation read, never a literal any of the three could disagree on.

```typescript
import { Bounded, Monoid } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'

const Floor: Monoid.Monoid<number> = Bounded.min(N.Bounded)
const Ceiling: Monoid.Monoid<number> = Bounded.max(N.Bounded)
const ascendingFloor: Monoid.Monoid<number> = Bounded.min(Bounded.reverse(N.Bounded))
const lowest: number = Floor.combineAll([4, 1, 9])
const vacuousFloor: number = Floor.combineAll([])
```

[ORDER_IS_ITS_OWN_MONOID_AND_THE_ALWAYS_EQUAL_COMPARATOR_IS_PROVABLY_TWO_SIDED]:
- `effect/Order` is a monoid over COMPARATORS whose identity is itself a comparator: `empty() = make(() => 0)` — the order calling every pair equal — and `combineAll(c) = combineMany(empty(), c)`, the identical seed split where the empty-admitting fold is seeded by the always-equal comparator and the non-empty fold by `self`. The identity here is a value of the SAME type the fold produces, so `combineAll([])` of zero comparators is a defined `Order<A>`, the "preserve input order" total order, never an undefined seed.
- The always-equal `empty()` is PROVABLY two-sided for the leftmost-decisive combine — the corruption trap the value-fold identity can hide cannot occur here: a comparator that never decides cannot pre-empt a later key (`combine(empty(), O) = O`) and cannot be pre-empted (`combine(O, empty()) = O`), so two-sidedness holds by the operation's own short-circuit shape rather than by the author's assertion. The one rung where the reified-but-unverified identity becomes verified-by-construction, because the seed value's behavior is structurally forced.
- The empty comparator is the dual of the value-fold `empty`: it contributes nothing to a lexicographic chain exactly as `0` contributes nothing to a sum, so the comparator monoid and the value monoid share one identity discipline — the seed the empty-admitting fold needs and the non-empty fold does not, the discriminant bit set by an operation that owns a genuine neutral.

```typescript
import { Order } from 'effect'

type Owner = { readonly rank: number; readonly label: string }
const ranked: Order.Order<Owner> = Order.combineAll([
    Order.mapInput(Order.number, (o: Owner) => o.rank),
    Order.mapInput(Order.string, (o: Owner) => o.label),
])
const preserveInput: Order.Order<Owner> = Order.combineAll([])
const verdict = ranked({ rank: 1, label: '<value-a>' }, { rank: 1, label: '<value-b>' })
```

[THE_BIT_AND_THE_PREMISES_ARE_ORTHOGONAL_SO_THE_PROMOTION_CONSUMES_A_PROOF_IT_DOES_NOT_PRODUCE]:
- A carrier supplies the unit the content provably lacks: `Option.getOptionalMonoid(S) = fromSemigroup(make((self, that) => isNone(self) ? that : isNone(that) ? self : some(S.combine(self.value, that.value))), none())` takes a bare `Semigroup<A>` with NO unit anywhere in `A` and returns a `Monoid<Option<A>>` whose `empty` is `None`. The input-output strength gap discloses that the carrier, not the parameter, sets the bit — the promotion does not find a hidden identity in `A`, it moves the algebra onto a carrier that owns one, which is why the result type changes carrier where the bound and literal sources keep it.
- Setting the identity bit is ORTHOGONAL to the associativity premise the operand already had to satisfy: `getOptionalMonoid` over a non-associative `S` yields a `Monoid<Option<A>>` whose `None`-identity is genuinely two-sided yet whose `Some ⊕ Some` step is exactly as non-associative as `S`. The strongest identity source cannot rescue the weakest law axis, so a `combineAll`-shaped fold over a lifted monoid is correct iff the inner combine was associative AND the lifted `empty` is two-sided — two independent facts, only one of them the bit.
- The empty-fold and associativity property tests therefore belong at the leaf `Semigroup` admission, never at a `fromSemigroup`/`getOptionalMonoid`/`Bounded.min` site: the promotion consumes a proof it does not produce, contributing only the identity datum and establishing none of the laws the datum's presence claims. A promotion that re-checked the operand's associativity would re-derive a fact the source rung already owed; the missing check is not an omission but the recognition that the lift trusts the rung below.

```typescript
import { Foldable } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as N from '@effect/typeclass/data/Number'
import * as O from '@effect/typeclass/data/Option'
import { Option } from 'effect'

const Span = O.getOptionalMonoid(N.SemigroupMax)
const widest = Foldable.combineMap(ArrayInstances.Foldable)(Span)
const result: Option.Option<number> = widest((n: number) => (n > 0 ? O.Of.of(n) : Option.none()))([3, 7, 1])
const fromEmpty: Option.Option<number> = Span.combineAll([])
```

[THE_RECORD_IDENTITY_IS_THE_FIELD_WISE_AND_OF_THE_COLUMN_BITS]:
- `Monoid.struct(fields)` assembles the record identity field-wise as `{ [k]: fields[k].empty }`, so the composite discriminant bit is the field-wise AND of its columns' bits — the struct monoid is buildable exactly when EVERY column already carries a unit, and one unit-less column denies the whole record an identity. A column frozen by `Semigroup.first`/`last` is unit-less, so its presence forces the entire struct down to a `Semigroup`: the missing column-identity surfaces as the inability to call `Monoid.struct` at all, the rung propagating from one field to the record.
- `Foldable.combineMap(F)(M) = F.reduce(self, M.empty, (m, a) => M.combine(m, f(a)))` seeds the whole walk with `M.empty`, so a foldMap over an empty structure returns the assembled identity record unmerged and the left-neutral-only trap reappears one level up — a struct `empty` that is left-neutral only corrupts only a multi-element fold, the field-wise composition of the per-column two-sidedness tests. The struct identity is correct iff every field's `empty` is two-sided, the bit being AND-ed across columns and the law being AND-ed alongside it.
- Adding a field adds one row to `fields` and the assembled identity extends with zero hand-edits, where a hand-written `Monoid.make` re-spells the entire `empty` record per field — and the moment a column needs `first`/`last`, the checker forces the demotion to `Semigroup` rather than admitting a wrong record identity. The composite owner's strongest fold is the field-wise minimum of its columns' rungs, the discriminant bit composing as an AND while the capability composes as a single owner declaration.

```typescript
import { Monoid } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'
import * as B from '@effect/typeclass/data/Boolean'

const Tally = Monoid.struct({ hits: N.MonoidSum, peak: N.MonoidMax, clean: B.MonoidEvery })
const seed: (typeof Tally)['empty'] = Tally.empty
const summary = Tally.combineAll([{ hits: 1, peak: 7, clean: true }, { hits: 1, peak: 3, clean: false }])
```
