# Kind Channel Arithmetic

[FOUR_SLOTS_ARE_A_POSITIONAL_RECORD_NOT_A_SEMANTIC_ONE]:
- `Kind<F, R, O, E, A>` is a four-field positional record `{ In; Out2; Out1; Target }` whose names in every typeclass signature are `R, O, E, A` — but the binding from slot to MEANING is the carrier's `TypeLambda`, not the Kind: a typeclass combinator manipulates slots by POSITION while the carrier decides which domain channel each position carries.
- `EffectTypeLambda.type = Effect<this["Target"], this["Out1"], this["Out2"]>` binds the success value to `Target`, the failure to `Out1`, the requirement to `Out2` — so the slot a typeclass author calls `R` (`In`) is UNUSED by Effect and the requirement actually rides the slot the typeclass calls `O` (`Out2`); the algebra is slot-faithful and channel-blind, and the carrier alone reconciles the two vocabularies.
- The reconciliation is total because the arithmetic each slot performs is FORCED by that slot's variance, not by what the carrier stores there: any covariant payload the carrier routes through `Out1`/`Out2` is unioned, any contravariant payload routed through `In` is intersected, regardless of whether it models a failure, a requirement, or an emitted element — so a carrier is free to assign channels to slots as long as it routes accumulate-by-union content to a covariant slot and accumulate-by-intersect content to the contravariant one.
- A combinator written generically over `Kind<F, R, O, E, A>` therefore composes Effect's requirements, Either's `Left`, and a stream's emitted type through ONE arithmetic, because all three are covariant payloads in covariant slots — the four-slot record is the single owner from which every carrier's composition law is derived, never a per-carrier hand-written merge.

[VARIANCE_HELPERS_ARE_THE_ARITHMETIC_MECHANISM]:
- The `&`/`|` outcome is not declared per combinator; it falls out of three function-position encodings the slot defaults reduce to: `Contravariant<A> = (_: A) => void`, `Covariant<A> = (_: never) => A`, `Invariant<A> = (_: A) => A` — the unconstrained `Kind` branch wraps `In` in `Contravariant`, `Out2`/`Out1` in `Covariant`, `Target` in `Invariant`.
- Two function types in contravariant position intersect their parameters under assignability (`((_: R1) => void) & ((_: R2) => void)` unifies at `R1 & R2`); two in covariant position union their results (`((_: never) => O1) | ...` unifies at `O1 | O2`) — so `R1 & R2`, `O1 | O2`, `E1 | E2` in `flatMap`/`product`/`coproduct` is the compiler discharging variance, never a bespoke type operator the library spells.
- `Variance<in out F, in R, out O, out E>` states the same fact as explicit modifiers: `R` is `in` (contravariant, intersected), `O` and `E` are `out` (covariant, unioned), `F` is `in out` (invariant) — the generator-recovery surface reads each channel back through `[K] extends [Variance<F, infer R, …>]`, so the SAME variance that drives merge arithmetic also drives extraction, one encoding serving accumulation and projection.
- `Target` is `Invariant` precisely because a map retargets it through a `to`/`from` pair, never accumulates it: `map`, `imap`, `contramap`, `flatMap`'s continuation all REPLACE `Target` (`A → B`) while holding `R, O, E` fixed — the value slot is the one slot the arithmetic never touches because it is the slot every transform rewrites.

[CHANNEL_ARITHMETIC_IS_THE_SAME_MONOID_ONE_LEVEL_UP]:
- `product`/`flatMap`/`coproduct` merge `R1 & R2` over `(types, &, unknown)` and `O1 | O2`, `E1 | E2` over `(types, |, never)` — `&` with unit `unknown` and `|` with unit `never` are the two free monoids on the lattice of types, so composing two carrier values runs the SAME monoid combine the value owner runs on its content, lifted one level onto the type parameters.
- `Of.of: <A>(a: A) => Kind<F, unknown, never, never, A>` is not a permissive default; it is the TWO-SIDED IDENTITY of that type-level monoid: `R & unknown = R`, `O | never = O`, `E | never = E`, so seeding any fold with `of(a)` adds zero requirement and zero failure — the neutral element of the channel arithmetic, materialized as the lift's signature.
- `Coproduct.zero: <A>() => Kind<F, unknown, never, never, A>` carries the identical `unknown, never, never` triple, so the recovery fold and the product fold share one type-level unit — the empty product and the empty alternation seed from the same neutral, the Semigroup/Monoid identity boundary recurring at the channel layer exactly as it recurs at the value layer.
- The collapse is total: a carrier owner declares one `flatMap` and one `of`, and the entire channel algebra — sequential merge, the unit, the bulk fold seed — is the free monoid the slot variances already encode; an owner that hand-merged channels per combinator would re-derive what the four-slot record states once.

[BULK_SAME_CHANNEL_FOLDS_REFUSE_HETEROGENEOUS_ACCUMULATION]:
- `product` accumulates `R1 & R2`, `O1 | O2`, `E1 | E2` across two DISTINCT channel sets, but `productMany: <R, O, E, A>(self, collection: Iterable<Kind<F, R, O, E, A>>)` pins ONE `R, O, E, A` across the whole iterable — a bulk product demands channel-UNIFORM elements, never accumulating a heterogeneous collection.
- `coproductMany` and `coproductAll` carry the same single-`R, O, E, A` constraint, and `combineMany`/`combineAll` on a derived carrier semigroup inherit it: the moment a fold runs over an `Iterable` rather than a fixed-arity pair, the type system stops unioning failures and starts REQUIRING they already match.
- The asymmetry is the executable specification of accumulation: pairwise composition through `product`/`flatMap` is where channels widen, so a do-builder accreting one `bind`/`andThenBind` per step accumulates each step's channels; a `combineAll` over a runtime-length list cannot, because the element type is fixed before the iterable is seen — heterogeneous accumulation lives in the static struct shape (`Product.struct(F)({ a, b })` where each field's channels union into the result), never in a dynamic fold.
- Reaching for `productAll`/`combineMany` over carrier values whose failure sets differ is the silent narrowing this forecloses: the collection element type unifies to the COMMON channel, so a value that should have widened the error set instead constrains the whole fold to the intersection — the static struct lift is the only shape where distinct channels survive.

[THE_VALUE_SLOT_IS_THE_ONLY_SLOT_TRANSFORMS_REWRITE]:
- Every retarget witness moves `Target` and holds `R, O, E`: `map` (`A → B`), `imap` (`to`/`from`), `contramap` (`B → A`), `flatMap`'s continuation — the variance on `Target` is `Invariant` because a covariant-only or contravariant-only annotation would forbid one of the two directions `imap` needs, and the carrier may map either way.
- `Bicovariant.bimap: <E1, E2, A, B>(f: (e: E1) => E2, g: (a: A) => B)` is the ONE witness that retargets a SECOND slot: its signature `<R, O>(self: Kind<F, R, O, E1, A>) => Kind<F, R, O, E2, B>` holds `R` and `O` fixed and rewrites `Out1` (`E1 → E2`) and `Target` (`A → B`) — the only retarget that touches a channel slot, because `Out1` is the slot the carrier stores a transformable second content in, never an accumulated one.
- `EitherTypeLambda.type = Either<this["Target"], this["Out1"]>` makes the `Left` a COVARIANT `Out1` payload, so `product`/`coproduct` over `Either` union `E1 | E2` at the `Out1` slot exactly as the same combinators union Effect's failure at `Out1` — the arithmetic is slot-driven and channel-agnostic, so a `Left` and a typed failure widen through one union because they occupy the same covariant slot, never two carrier-specific merge rules.
- A carrier that needs to ACCUMULATE a channel routes it to `Out1`/`Out2`; a carrier that needs to CONSUME a channel routes it to `In` (`FunctionTypeLambda.type = (a: this["In"]) => this["Target"]`, the sole owner using `In`); a carrier with one content type leaves the unused slots at their defaults — the slot assignment in the `TypeLambda` declaration IS the channel-behavior specification, read once and never restated at a combine site.

```typescript
import * as EitherInstances from '@effect/typeclass/data/Either'
import { Either } from 'effect'

type FaultA = { readonly _tag: 'FaultA' }
type FaultB = { readonly _tag: 'FaultB' }

const left: Either.Either<number, FaultA> = Either.right(7)
const right: Either.Either<string, FaultB> = Either.right('<value-a>')
const paired: Either.Either<[number, string], FaultA | FaultB> = EitherInstances.SemiProduct.product(left, right)
const retargeted: Either.Either<number, string> = EitherInstances.Bicovariant.bimap(
    left,
    (e: FaultA) => e._tag,
    (a: number) => a + 1,
)
```

[SINGLE_CONTENT_CARRIERS_SLOT_IN_WITH_ZERO_ARITHMETIC]:
- `OrderTypeLambda.type = Order<this["Target"]>`, `EquivalenceTypeLambda.type = Equivalence<this["Target"]>`, `OptionTypeLambda.type = Option<this["Target"]>`, `ReadonlyArrayTypeLambda.type = ReadonlyArray<this["Target"]>` populate ONLY `Target`, leaving `In`/`Out2`/`Out1` at their unconstrained defaults — a one-content carrier participates in the four-slot machinery without ever exercising the channel arithmetic.
- This is why a comparison instance composes through `product` as cleanly as an effectful one: `Order.product`/`Equivalence.product` build tuple comparisons through the SAME `SemiProduct` whose signature unions `O1 | O2` and `E1 | E2`, but those slots stay at the carrier default for a single-content owner, so the result's only moving part is `Target` widening to `[A, B]` — one applicative law spans channel-rich and channel-free carriers.
- `ReadonlyRecordTypeLambda<K extends string = string>` parameterizes the `TypeLambda` ITSELF (`ReadonlyRecord<K, this["Target"]>`), so the carrier's own shape carries a type argument independent of the four slots — the key type rides the lambda, the value rides `Target`, and `getTraversable<K>()` pins the lambda's `K` while the walk threads `Target`, two orthogonal type axes on one carrier with neither leaking into the channel arithmetic.
- A standalone tuple-order utility hand-spelling lexicographic comparison pins the one cell a `SemiProduct<OrderTypeLambda>` spans generically: the single-content carrier's product is the same monoid the channel-rich carrier runs, so the comparison family and the effect family share one composition surface, and a per-arity order helper is the re-derivation the carrier-polymorphic product deletes.

[TYPELAMBDAFIX_PINS_CONTENT_TO_SATISFY_A_SLOT_SHAPED_TYPECLASS]:
- A typeclass demands an `F extends TypeLambda` whose `Target` is threaded through `this["Target"]`, but some instances are determined only AFTER the content type is fixed — `IdentityTypeLambdaFix<A>` sets `type` to a constant `Identity<A>` (ignoring `this["Target"]`) precisely so a content-fixed carrier can still present the slot-shaped interface the typeclass requires.
- The split is load-bearing on `data/Identity`: `IdentityTypeLambda.type = Identity<this["Target"]>` is the polymorphic form carrying `Covariant`, `Monad`, `Applicative`, `Traversable` as bare consts (content-generic, no free datum), while `getSemiCoproduct: <A>(S: Semigroup<A>) => SemiCoproduct<IdentityTypeLambdaFix<A>>` returns an instance over the FIXED lambda because the coproduct of two bare values has no structural choice point and must be decided by `S.combine` — the algebra cannot be defined until `A` is pinned, so the fix-lambda is what lets the determined-only-after-`A` instance exist at all.
- The two lambdas coexist in one module: the polymorphic one serves every content-generic consumer, the fixed one serves the consumer whose instance needs the content nailed down — the same carrier presents both faces without a second carrier type, the fix-lambda absorbing the per-content instance family rather than spawning a parallel owner per fixed `A`.
- The pattern generalizes any carrier whose instance is parameterized by an inner algebra: `getSemiAlternative: <A>(S) => SemiAlternative<IdentityTypeLambdaFix<A>>` reuses the identical fix-lambda, so the bare-value carrier's full recovery suite is one lambda and a family of `<A>(S)`-shaped factories — the fix-lambda is the encoding that converts "this instance needs the content first" from an impossibility into one extra type parameter on the carrier shape.

```typescript
import * as Identity from '@effect/typeclass/data/Identity'
import * as N from '@effect/typeclass/data/Number'

const Peak = Identity.getSemiCoproduct(N.SemigroupMax)
const generic: number = Identity.Covariant.map(9, (n) => n + 1)
const winner: number = Peak.coproduct(3, 9)
const folded: number = Peak.coproductMany(2, [9, 4])
```

[THE_TYPECLASS_INTERFACE_IS_THE_DEEP_OWNER_OVER_THE_CARRIER_FAMILY]:
- The channel arithmetic is authored ONCE, in `flatMap`'s lone signature, and every free projection over `FlatMap<F>` inherits it by composition, not by restatement: `flatten`'s `R1 & R2, O1 | O2, E1 | E2` and `zipRight`'s identical merge are the SAME slot variances discharged again, so a carrier supplying one `flatMap` gets a whole sequencing surface whose every member's channel behavior is fixed by the four-slot record, never re-decided per combinator.
- `composeK(F)(afb, bfc)` performs the union at the ARROW level before any value flows: it fuses `(a) => Kind<F, R1, O1, E1, B>` and `(b) => Kind<F, R2, O2, E2, C>` into `(a) => Kind<F, R1 & R2, O1 | O2, E1 | E2, C>`, so a multi-stage pipeline's total requirement and failure set are computed once at composition time as one pre-fused arrow, never re-accumulated by a nested `flatMap` ladder re-opening the arithmetic per stage.
- Adding a carrier is adding one `TypeLambda` plus the minimum methods, and the entire derived family — bulk products, Kleisli fusion, do-builders, lifted algebras — materializes with the channel arithmetic already correct, because the arithmetic lives in the slot variances the carrier inherits from `TypeLambda`, not in any per-carrier code. A wrapper that re-spelled `flatten` or `zipRight` for one carrier would re-derive the free function the interface already owns.
- `productComposition(F, G)` derives the channel arithmetic for a STACKED carrier `Kind<F, …, Kind<G, …, A>>` by merging BOTH layers' slots independently in one signature — `Kind<F, FR1 & FR2, FO1 | FO2, FE1 | FE2, Kind<G, GR1 & GR2, GO1 | GO2, GE1 | GE2, [A, B]>>` — so a two-layer stack gets its product from composing the layer instances, the outer and inner channel arithmetic each discharged once and never re-spelled per stack depth; `mapComposition(F, G)` retargets only the inner `Target` through both layers, leaving every channel slot fixed.
