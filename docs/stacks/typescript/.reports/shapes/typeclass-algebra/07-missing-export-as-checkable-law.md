# Absence as the Checkable Premise

[THE_DO_BUILDER_IS_ASSEMBLED_PER_MEMBER_FROM_THE_WEAKEST_SUFFICIENT_WITNESS]:
- The struct-accreting notation is not one combinator over a carrier; it is five free functions each closed over the MINIMUM interface its step consumes, so the builder a value owner gets is the EXACT subset of the five its instance set satisfies — `Do(Of)` seeds the empty struct, `bindTo(Invariant)`/`tupled(Invariant)` wrap a scalar into a one-field record or one-slot tuple, `let(Covariant)` accretes a field computed purely from prior fields, `andThenBind(SemiProduct)` accretes an INDEPENDENT carrier field, `bind(Chainable)` accretes a DEPENDENT carrier field — and a carrier missing a witness const is missing exactly the builder step that witness gates, surfaced as the combinator failing to type-resolve, never a runtime gap.
- The gating is a strict capability climb the interface heritage encodes: `Pointed extends Covariant, Of`, `Chainable extends FlatMap, Covariant`, `SemiProduct extends Invariant` (NOT Covariant), `Product extends SemiProduct, Of` — so the four accretion steps demand four strictly-ordered strengths, and the builder a carrier exposes is the down-closure of its strongest instance under that heritage. Reaching for `bind` over an owner that ships only `Covariant` is the compile fact that the owner cannot SEQUENCE a dependent field, the absent `FlatMap` making "the next field may depend on the previous one" unrepresentable rather than silently mis-evaluated.
- The naive shape welds the four accretions into one mutable record threaded through a procedure, so a step that should have been pure-from-prior, independent, or dependent is indistinguishable in the body and the carrier's actual strength is never checked; the per-witness builder factors each step into the combinator whose witness the owner must prove it carries, so the dependence/independence/purity of every field is a type obligation discharged at the accretion site.

[THE_OBJECT_AND_TUPLE_CONSTRAINTS_FORCE_THE_SEED_BEFORE_ANY_FIELD_ACCRETES]:
- `let`, `bind`, and `andThenBind` each constrain the accumulated carrier as `A extends object` and the new name as `Exclude<N, keyof A>`, so the three field-accreting steps run ONLY over a record-shaped carrier and ONLY with a provably-fresh key — a scalar carrier cannot be accreted onto and a rebind of an existing field is a compile error, so the builder grows strictly monotonically by one new key per step with the result type `{ [K in keyof A | N]: K extends keyof A ? A[K] : B }` widening one member at a time.
- The seed that satisfies `A extends object` comes from exactly two sources, and which is available is itself a witness fact: `Do(Of)()` lifts the empty struct `{}` (the carrier must own `Of`), or `bindTo(Invariant)(name)` wraps an existing scalar carrier into `{ [name]: A }` (the carrier need only be `Invariant`) — so an owner with `Invariant` but no `Of` cannot START a do-block from nothing yet CAN convert a held value into the record the field steps then grow, the two seed routes drawn from two different rungs.
- `appendElement(SemiProduct)` is the positional dual carrying `A extends ReadonlyArray<any>` and producing `[...A, B]`, so the tuple-builder accretes slots only over a carrier already shaped as a readonly array, seeded by `tupled(Invariant)` lifting a scalar to `[A]` — the record-builder and the tuple-builder are the same independent-accretion step (`andThenBind`/`appendElement`, both `SemiProduct`) over two carrier shapes, each refusing the other's seed at compile time.

```typescript
import { Invariant, Covariant, Chainable } from '@effect/typeclass'
import * as OptionInstances from '@effect/typeclass/data/Option'
import { Option, pipe } from 'effect'

const seed: (s: Option.Option<number>) => Option.Option<{ readonly raw: number }> = Invariant.bindTo(OptionInstances.Invariant)('raw')
const label = (a: { readonly scaled: number }): Option.Option<string> => (a.scaled > 0 ? Option.some('<value-a>') : Option.none())
const built: Option.Option<{ readonly raw: number; readonly scaled: number; readonly tag: string }> = pipe(
    Option.some(7),
    seed,
    Covariant.let(OptionInstances.Covariant)('scaled', (a) => a.raw * 2),
    Chainable.bind(OptionInstances.Chainable)('tag', label),
)
```

[THE_CONTRAVARIANT_OWNER_ACCRETES_INDEPENDENT_FIELDS_BUT_CANNOT_COMPUTE_OR_SEQUENCE_ONE]:
- A predicate-as-value owner carries `Of`, `Invariant`, `SemiProduct`, `Product`, and `Contravariant` over its type lambda but ships NO `Covariant` and NO `FlatMap`/`Chainable`/`Monad`, so its do-builder degrades to a SHARP intermediate: `Do(Of)` seeds it, `andThenBind(SemiProduct)` accretes an independent predicate field, `bindTo(Invariant)` wraps a held one — yet `let(Covariant)` is foreclosed because a contravariant carrier has no forward `map` to compute a field FROM prior fields, and `bind(Chainable)` is foreclosed because a predicate cannot SEQUENCE a dependent continuation. The builder a contravariant owner exposes is the independent-accretion subset and nothing forward, the missing `Covariant`/`FlatMap` consts making "a field derived from earlier fields" and "a field depending on earlier fields" two distinct compile gaps.
- This is the exact inversion of a monadic carrier's builder: an owner whose only forward capability is `SemiProduct` accretes by PAIRING (each `andThenBind` runs an independent carrier and joins it via `product`), where a `Chainable` owner accretes by SEQUENCING — so the same `andThenBind` step is the strongest field-accretion a contravariant owner has and merely the WEAKEST a monadic owner has, the builder's ceiling read off which forward consts the carrier ships.
- The contravariant carrier's actual growth axis lives on a different combinator entirely: a record validator grows by `Contravariant.contramap(leaf, accessor)` retargeting a leaf predicate onto a field, then folding the retargeted leaves through the conjunction monoid — so the do-builder's `andThenBind` (independent pairing into a struct of predicates) and the contramap-then-fold (one predicate over a struct) are two different compositions the one owner exposes, the builder for stacking predicate-valued fields and the fold for a single field-spanning guard.

```typescript
import { SemiProduct, Of } from '@effect/typeclass'
import * as Pred from '@effect/typeclass/data/Predicate'
import { Predicate, pipe } from 'effect'

type Span = { readonly lo: number; readonly hi: number }
const positive: Predicate.Predicate<number> = (n) => n > 0
const bounded: Predicate.Predicate<number> = (n) => n < 64
const paired = pipe(
    Of.Do(Pred.Of)(),
    SemiProduct.andThenBind(Pred.SemiProduct)('lo', positive),
    SemiProduct.andThenBind(Pred.SemiProduct)('hi', bounded),
)
const guard: Predicate.Predicate<Span> = Pred.getMonoidEvery<Span>().combineMany(
    Pred.Contravariant.contramap(positive, (o: Span) => o.lo),
    [Pred.Contravariant.contramap(bounded, (o: Span) => o.hi)],
)
```

[BIND_SUPPRESSES_INFERENCE_BACK_INTO_THE_ACCUMULATED_STRUCT_WHERE_LET_AND_ANDTHENBIND_DO_NOT]:
- `bind(Chainable)`'s continuation is typed `(a: NoInfer<A>) => Kind<F, …, B>` while `let(Covariant)`'s is `(a: A) => B` and `andThenBind(SemiProduct)` takes a fixed carrier `that` with no continuation at all — the `NoInfer` on `bind` alone is load-bearing: a dependent step reads the accumulated struct as ALREADY-FIXED so the body cannot widen `A` by inference, forcing the prior fields' types to be settled by the chain before the dependent continuation runs, where a pure `let` field computed synchronously can let its accessor participate in inference because it allocates no new carrier.
- The asymmetry is the executable proof of the dependence axis at the type level: `andThenBind`'s field is a fully-constructed independent carrier (no read of `A`, so no inference concern), `let`'s field is a pure function of a settled `A` (read-only, inference-safe), and `bind`'s field is a continuation whose carrier is built FROM `A` and must therefore freeze `A` first — the three accretion steps differ precisely in how each relates to the prior struct, and `NoInfer` marks the one step that consumes the struct to PRODUCE the next carrier.
- `tap(Chainable)` is the dependent step that accretes NOTHING: it runs `(a: A) => Kind<F, …, _>` for its channel accumulation and re-yields the original `A` unchanged (`Kind<F, R1 & R2, O1 | O2, E1 | E2, A>`), so the value slot is preserved while the requirement/failure channels widen — the validating step in a builder where the field's value is irrelevant and only its effect threads through, gated on `Chainable` because re-surfacing the held `A` after the side-effect needs the `Covariant` half `FlatMap` alone lacks.

[THE_EXPORT_FORM_IS_A_FIVE_WAY_WITNESS_SIGNATURE_READ_BEFORE_ANY_CALL]:
- An instance's export FORM declares what the carrier cannot decide alone, and the five forms partition by the kind of free datum, read off the surface with zero implementation inspection: a BARE CONST means the behavior is fully fixed by the carrier (`Option.Monad`, `Array.Covariant`); a `get*(subAlgebra)` factory means an inner value algebra is undetermined (`getMonoidUnion(M)`, `getOptionalMonoid(S)`); a `get*(options)` factory means a runtime POLICY is unfixed (`getApplicative(options)`); a `get*<K extends string>()` factory means the carrier's OWN type-lambda shape is parameterized (`getCovariant<K>()`); a `<A>()` phantom factory means only an element type floats with no value datum (`Array.getMonoid<A>()`, `getMonoidEvery<A>()`); and an ABSENT export means the operation is uninhabitable for that carrier — five signatures of free-datum quantity, the consumer selecting an instance by choosing the form and feeding its one parameter once.
- The phantom `<A>()` form is the sharpest cut against the sub-algebra form: `getMonoidEvery<A>()` over a predicate owner takes a type parameter but NO value because the all-pass identity (`constTrue`) is fully determined by the operation, while `getMonoidUnion(M)` over a record owner takes a real `Monoid<A>` because the per-key merge is undetermined — the empty parens versus the value parameter is the exact disclosure of whether the carrier owns the policy or delegates it, so a predicate's four lattice algebras ship as four `<A>()` phantom factories (one per operation, each with its own fixed identity) while a record's keyed algebra ships as one `get*(M)` family parameterized by the value witness.
- The factory parameter is ALWAYS a domain value carrying its own behavior, never a boolean knob whose combinations the instance re-derives — a `Semigroup`, a `Monoid`, a `ConcurrencyOptions` policy, a phantom key witness — so choosing the instance and feeding its one datum resolves the policy at the WITNESS boundary, and the consuming fold body never re-decides it; threading the same free datum as a flag at every call site is the re-derivation the export form collapses into one application.

```typescript
import * as N from '@effect/typeclass/data/Number'
import * as ArrayInstances from '@effect/typeclass/data/Array'
import { getMonoidUnion, getTraversable } from '@effect/typeclass/data/Record'
import { getApplicative } from '@effect/typeclass/data/Effect'

const phantom = ArrayInstances.getMonoid<number>()
const subAlgebra = getMonoidUnion(N.MonoidSum)
const policy = getApplicative({ concurrency: 'unbounded' })
const carrierShape = getTraversable<'<key-a>' | '<key-b>'>()
const folded = phantom.combineAll([[1, 2], [3], phantom.empty])
const overlaid = subAlgebra.combine({ '<key-a>': 1 }, { '<key-a>': 9, '<key-b>': 4 })
```

[ONE_CONCEPT_TAKES_DIFFERENT_FORMS_ACROSS_CARRIERS_AND_THE_FORM_NAMES_THE_CARRIER_GAP]:
- The product/applicative concept appears in every form across carriers and the form at each is the carrier's signature of what IT cannot fix: `SemiProduct`/`Product` are BARE CONSTS on a predicate owner and a bare-value identity owner (independent pairing is fully determined there), they are `get*(options)` FACTORIES on an effect owner (independent composition admits a parallelism degree the carrier cannot fix), and they are ABSENT on a positional-pair owner that ships only `Bicovariant` (a tuple has no product seed) — so the same combinator the do-builder's `andThenBind` consumes is a const for one owner, a policy factory for another, and unspellable for a third, the build step's availability read off whether the carrier exposes the witness at all.
- The factory-versus-const split tracks the dependence boundary exactly: an effect owner ships its `Monad`/`FlatMap`/`Chainable` as CONSTS (sequential evaluation is FORCED by data dependence, no free datum) while shipping `getSemiProduct`/`getApplicative` as FACTORIES (independent composition admits the concurrency choice the monad cannot), so the channel that selects the algebra also selects whether the instance can be a const — a `bind`-chain over an effect owner accretes through a constant `Chainable`, but the SAME owner's `andThenBind`-style independent accretion must first build a witness via `getSemiProduct(opts)` because the independent step carries a policy the dependent step forecloses.
- The consequence for the builder is structural: an effect owner's do-block accretes pure (`let`) and dependent (`bind`) fields through bare consts, but every INDEPENDENT field accretion is gated behind a policy choice — there is no `andThenBind(Effect.SemiProduct)` to reach for because no `SemiProduct` const exists, only `andThenBind(getSemiProduct(opts))`, so the parallelism degree of an independent field is a witness parameter the builder demands at exactly the step where independence licenses a parallelism the sequential steps cannot have.

[THE_BARE_CONST_PROPAGATES_UP_THE_HERITAGE_WHILE_THE_FACTORY_STOPS_AT_THE_FREE_DATUM]:
- A constant instance composes up the interface lattice with zero new datum: `Monad extends FlatMap, Pointed` and `Pointed extends Covariant, Of`, so an owner exporting `FlatMap` and `Pointed` consts also exports their join `Monad` as a const, because `map`, `of`, and `flatMap` each have exactly one lawful behavior per carrier with no choice to surface — the constant nature is closed under heritage, and a carrier's bare-const stack (`Covariant`, `Of`, `Pointed`, `FlatMap`, `Chainable`, `Monad`) is one mechanical fusion the owner declares once and the lattice propagates.
- A factory instance does NOT propagate up the heritage for free: an effect owner's `getApplicative(opts)` cannot be a const-fused `Monad` because the applicative half carries a policy the monad half does not, so the carrier splits its stack precisely at the dependence seam — the constant rungs below `Applicative` and the factory rungs at `Applicative` and above, the split being the executable map of which rungs carry a free datum and which are determined.
- The consequence for a consuming pipeline is that the const-versus-factory distinction is INVISIBLE in the body: a const arrives by import alone, a factory by one application, and both reach the fold as a fully-formed witness, so the policy is resolved at the witness boundary and the fold body is identical whether the witness was a const or a factory — the form decides where the datum is fed, never how the fold reads it.

[ABSENCE_AT_THE_LIFTED_LAYER_FORECLOSES_A_DERIVED_FAMILY_NOT_ONE_OPERATION]:
- A missing const at a structure-walk rung removes a WHOLE derived family, not a single method, because the free functions over that rung all consume it: a keyed-record owner ships `Covariant`/`Filterable`/`Traversable` consts but NO `Foldable` const, so `combineMap`, `reduceKind`, `coproductMapKind`, `toArray`, and every fold-shaped derivation over that owner is foreclosed at once — the absent `Foldable.reduce` makes the entire summary-fold surface unreachable, the missing single member subtracting the family the interface would have generated, where the same owner's present `Traversable` keeps the whole walk surface open.
- The blast radius of an absence is read off WHICH surface form is missing, and the two forms subtract at different scales: a missing FACTORY removes exactly one operation (its single constructed instance), but a missing single-member CONST removes the entire free-function family that interface generates, because every derived combinator over that rung names the absent member — so foreclosing `Foldable` (one absent `reduce`) subtracts `combineMap`, `reduceKind`, `coproductMapKind`, `toArray`, and `toArrayMap` in one stroke, where foreclosing a `getMonoid` factory subtracts only the empty-admitting fold. The absent CONST is the higher-leverage foreclosure precisely because the interface's economy works in reverse: one member generates a family, so one missing member ungenerates it.
- The foreclosure is positional inside a composed deriver, not global to the carrier: a deriver names a distinct capability per layer (`filterMapComposition` demands `Covariant` of the outer and `Filterable` of the inner), so the same carrier may satisfy one role and fail another in the same composition — an owner that is `Covariant` but not `Filterable` composes as the OUTER of a filter-composition (it only re-maps) yet is rejected as the INNER (it cannot drop), the absence being a per-position compile rejection rather than a carrier-wide verdict, so the legality of a stacked operation is the conjunction of per-position const-presence checks the deriver discharges.

[ABSENCE_IS_A_PROOF_THE_CHECKER_DISCHARGES_WHILE_PRESENCE_IS_A_PREMISE_IT_TRUSTS]:
- The two ways a fact reaches the export surface are inverse in enforcement strength: a fact encoded as ABSENCE is discharged by the checker with no test (the missing combinator cannot be named, so the foreclosed case is a compile error at the call), while a fact encoded as PRESENCE is a premise the checker reads structurally and TRUSTS — the witness shape is a record of operations, never of the laws those operations must obey, so admitting any present witness enters its entire derived family on premises the signature does not record, and a leaf that violates one produces a defined, type-checking, silently-wrong value across every derivation.
- This makes the export surface a one-sided gate: what the library CANNOT do for a carrier is a hard compile fact (a foreclosed factory, a foreclosed builder step, a foreclosed nesting role), but what it CAN do is admitted on trust, so the genuine verification work concentrates entirely at the LEAF witness admission — the property test asserting a leaf's associativity, an equivalence leaf's transitivity, or an `imap` retarget's round-trip inverse is the only enforcement of those facts, and the form is mute on every one of them. A tolerance leaf annotated on one owner field folds cleanly through the entire derived conjunction the form admits and corrupts hashed membership where the relation is assumed true.
- The consequence is a trust gate at exactly one point per derivation fan: a promotion, a struct lift, a carrier-driven monoid, or a do-builder accretion CONSUMES a proof it does not produce, contributing only the datum its parameter names and re-establishing none of the premises the present witness claims — so the leaf admission test guards the whole multiplicative fan, and a re-check at any downstream lift would re-derive a fact the source rung's admission already owed, the missing downstream check being the recognition that absence enforces and presence delegates.

```typescript
import { Semigroup, Monoid, Foldable } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'

type Span = { readonly lo: number; readonly hi: number }
const Merge: Semigroup.Semigroup<Span> = Semigroup.make((a, b) => ({ lo: Math.min(a.lo, b.lo), hi: Math.max(a.hi, b.hi) }))
const Cover: Monoid.Monoid<Span> = Monoid.fromSemigroup(Merge, { lo: Infinity, hi: -Infinity })
const widen = Foldable.combineMap(ArrayInstances.Foldable)(Cover)
const total: Span = widen((s: Span) => s)([{ lo: 3, hi: 7 }, { lo: -1, hi: 4 }])
const vacuous: Span = Cover.combineAll([])
```
