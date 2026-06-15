# Algebraic Owner Instances

[INSTANCE_AS_WITNESS]:
- An instance is a record of operations, not a class: `Semigroup<A>` is `{ combine: (self, that) => A; combineMany: (self, collection: Iterable<A>) => A }`, so the algebra is a value passed to a call site, never a method re-resolved per value or a `class extends` heritage.
- `Semigroup.make(combine, combineMany?)` derives `combineMany` from `combine` when omitted, defaulting to a STRICT LEFT fold `reduce(self, combine)(collection)` (`(((self ⊕ a₀) ⊕ a₁) ⊕ a₂)`); the second argument is supplied only when an `Iterable` fold has a faster spelling than left-folding `combine`, so the witness ships one operation and the bulk operation is free.
- One instance value threads through a whole computation: a function takes `S: Semigroup<A>` once and every accumulation inside reads `S.combine`, never re-deriving the algebra at each site — the witness passed, not the rule restated.
- `combineAll` lives on `Monoid<A>`, not `Semigroup<A>`, because the empty case requires `empty`; `combineMany` is the semigroup-level fold that needs a non-empty seed (`self`), and the two are the same fold split by whether a zero element exists.

[STRENGTH_HIERARCHY_GATES_THE_WITNESS]:
- The interface lattice — `Of` → `Pointed extends Covariant + Of` → `Monad extends FlatMap + Pointed`, and `SemiProduct + Covariant` → `SemiApplicative` → `Applicative extends SemiApplicative + Product`, with `SemiCoproduct` → `Coproduct extends SemiCoproduct` and the recovery duals `SemiAlternative = SemiCoproduct & Covariant` → `Alternative = Coproduct & SemiAlternative` — means the witness a combinator demands is exactly its weakest sufficient algebra: passing too weak an instance is a compile error, never a runtime gap.
- `Monoid<A> extends Semigroup<A>` adding `empty: A` and `combineAll: (collection: Iterable<A>) => A`; a `Monoid` is a `Semigroup` everywhere one is accepted, so a function constrained on `Semigroup<A>` takes any `Monoid<A>` with zero adaptation.
- `Monoid.fromSemigroup(S, empty)` is the one derivation that turns the weaker witness into the stronger one — the only new datum is the identity element, and `combineAll` is generated as a left fold seeded by `empty`. Hand-writing a parallel `Monoid` record beside an existing `Semigroup` is the duplication this deletes.
- The identity boundary recurs at every layer of the lattice: `Applicative.getMonoid(F)(M)` requires the FULL `Applicative` because `empty` is `F.of(M.empty)` — the `Of` half of `Product`; `SemiApplicative.getSemigroup(F)(S)` requires only `SemiApplicative` because a semigroup has no identity to lift; `Coproduct.getMonoid(F)()` needs `zero` while `SemiCoproduct.getSemigroup(F)()` does not — the identity-needs-`Of`/`zero` boundary is encoded in which interface each lifter constrains.
- The strength a fold demands is the identity it admits, and the identity it admits is the law it requires: `combineMany`/`getSemigroup` need only associativity (Semigroup), `combineAll`/`getMonoid`/`combineMap` need associativity AND a two-sided identity (Monoid) — passing a Semigroup where a Monoid is required is a compile error, but supplying a unit that is left-neutral-only is a SILENT corruption the type system cannot catch.

[OF_AND_POINTED_ARE_THE_LIFT_SEAM]:
- `Of<F>` carries one member, `of: <A>(a: A) => Kind<F, unknown, never, never, A>`, the single-value lift whose channels are MAXIMALLY permissive (`R = unknown`, `O = never`, `E = never`) so a lifted pure value imposes no requirement and announces no failure — the seed that unifies with any accumulating context.
- The `unknown, never, never` channels on `of` are load-bearing for accumulation: a lifted seed unifies under `R1 & unknown = R1` and widens under `E | never = E`, so seeding a `bind` chain or a `coproductMany` fold with `of(a)` or `Do()` adds zero requirement and zero failure to the accumulated channels — the neutral element of the channel arithmetic, not merely of the value.
- `Of` derives three seeds for free: `Do(F)()` lifts the empty struct `{}` that a `bind`/`let` chain accretes onto, `void(F)()` lifts a `void` value for a discard channel, and `ofComposition(F, G)` produces a nested `(a) => F<G<A>>` lift so a stacked value owner seeds both layers in one call — none re-spelled per owner.

[VARIANCE_RETARGETS_INSTANCES]:
- `Invariant<F>`, `Covariant<F>`, `Contravariant<F>`, and `Bicovariant<F>` are not value algebras; they are the witnesses that RETARGET an existing instance onto a projection, so a new instance for `B` is `imap`/`contramap`/`map`/`bimap` of the instance for `A`, never a second `make`.
- `Covariant<F> extends Invariant<F>` and `Contravariant<F> extends Invariant<F>`: both refine `imap`, so any covariant or contravariant owner is accepted wherever an invariant retarget is required, and `bindTo`/`tupled` (which need only `Invariant`) run over either.
- The retarget direction is the variance: `Covariant.imap(map)` builds `imap` by discarding `from` (`(to, _from) => map(to)`); `Contravariant.imap(contramap)` builds `imap` by discarding `to` (`(_to, from) => contramap(from)`) — one `imap` interface, two derivations, the discarded arrow naming the variance.
- `imapComposition(F, G)` retargets a nested `Kind<F, …, Kind<G, …, A>>` through both layers with one `(to, from)` pair, and `contramapComposition(F, G)` over two contravariant layers yields a COVARIANT `map` — composing two input-flips restores forward variance, so a doubly-nested consumer owner maps its content covariantly through one `f` and a stacked owner derives its invariant instance from the two layer instances without spelling the nesting by hand.

[CONTRAVARIANT_RETARGET]:
- `Contravariant<F>.contramap: <B, A>(f: (b: B) => A) => (self: Kind<F,…,A>) => Kind<F,…,B>` pulls a consumer instance back along an input projection: a predicate, comparator, or equivalence over `A` becomes one over `B` by `f: B => A`, the standard way to compare records by a field without a new instance.
- `effect/Order` and `effect/Equivalence` expose contravariance as the standalone `mapInput: <B, A>(f: (b: B) => A) => (self) => Order<B>`, NOT a `Contravariant` instance: neither exports a typeclass instance over its `TypeLambda`, only the `TypeLambda` itself, so the comparison family retargets through the free function and slots into generic machinery only as the `F` carrier its `product`/`combine` already drive.

[CARRIER_OWNER_AS_CONTRAVARIANT_INSTANCE]:
- `data/Predicate` carries the predicate-as-value-owner full contravariant suite: `Contravariant`, `Invariant`, `Of`, `SemiProduct`, `Product` over `PredicateTypeLambda`, so a predicate is mapped, paired, and retargeted by witness, never re-closed by hand.
- The same owner ships four lattice algebras as Semigroup/Monoid pairs — `getMonoidEvery`/`getSemigroupEvery` (∧), `getMonoidSome`/`getSemigroupSome` (∨), `getMonoidXor`/`getSemigroupXor`, `getMonoidEqv`/`getSemigroupEqv` — each a fresh-typed `<A>()` factory because the identity predicate differs per operation; an all-pass guard fold and an any-pass guard fold are two instance values over the one predicate owner.
- `Predicate.Contravariant.contramap(f)` plus `Product` is the canonical move: build one predicate over a leaf, retarget it onto a record field with `contramap`, then `combine` field predicates through `getMonoidEvery` — a record validator is the every-monoid fold of field-retargeted leaf predicates, no enumerated `&&`.

```typescript
import * as Pred from '@effect/typeclass/data/Predicate'
import { Predicate } from 'effect'

type Owner = { readonly count: number; readonly name: string }
const positive: Predicate.Predicate<number> = (n) => n > 0
const nonEmpty: Predicate.Predicate<string> = (s) => s.length > 0

const Every = Pred.getMonoidEvery<Owner>()
const guard: Predicate.Predicate<Owner> = Every.combineMany(
    Pred.Contravariant.contramap(positive, (o: Owner) => o.count),
    [Pred.Contravariant.contramap(nonEmpty, (o: Owner) => o.name)],
)
const admits = guard({ count: 3, name: '<value-a>' })
```

[BICOVARIANT_TWO_CHANNEL_OWNER]:
- `Bicovariant<F>.bimap: <E1, E2, A, B>(f: (e: E1) => E2, g: (a: A) => B)` is the single witness for a two-channel owner: it retargets the error/left channel and the value/right channel independently in one pass.
- `data/Tuple` exposes ONLY `Bicovariant` — a positional pair has no monad or product structure, so its sole derivable instance is the dual map over both slots, and `mapLeft(F)`/`map(F)` are both DERIVED from that one `bimap` (`mapLeft` fixes `g = identity`, `map` fixes `f = identity`).
- `data/Either` carries `Bicovariant` plus the full success-channel hierarchy (`Covariant`, `Monad`, `SemiProduct`, `Product`, `Applicative`): the value channel is monadic while the error channel is only functorial, so `mapLeft` transforms the failure without a `flatMap` over it — the asymmetry the two-channel shape encodes.
- `bimapComposition(CovariantF, BicovariantG)` lifts a `bimap` through an outer covariant layer, so a covariant collection of two-channel owners retargets both inner channels with one nested call.

[COVARIANT_MAPPING_PRIMITIVES]:
- `as(F)(b)` replaces every success with a constant `B` (`map` to a fixed value); `asVoid(F)` is the `B = void` specialization — a result discarded to a unit channel without `map(() => undefined)`.
- `flap(F)(a)` applies a held function inside the carrier to a plain argument: a `Kind<F,…,(a: A) => B>` becomes `(a: A) => Kind<F,…,B>`, the dual of `ap` where the argument is bare and the function is wrapped.
- These are derived from the `Covariant` witness alone, so they exist for every covariant owner with zero per-owner code; reaching for `map((_) => b)` where `as(F)(b)` exists re-derives the combinator the witness already carries.

[SEMIGROUP_AS_VARIANCE_INSTANCE]:
- `Semigroup<A>` is itself an invariant owner: `Semigroup.Invariant` over `SemigroupTypeLambda` and `Semigroup.imap(to, from)` retarget an accumulation algebra onto a wrapped type — a `Semigroup<number>` becomes a `Semigroup<Branded>` by `imap(wrap, unwrap)`, the lawful path to give a value object its parent's algebra without re-implementing `combine`, provided `wrap`/`unwrap` are mutual inverses on the relevant domain.
- `Semigroup.first<A>()` (`(a, _) => a`) keeps the left, `last<A>()` (`(_, b) => b`) keeps the right, `constant(a)` (`() => a`) ignores both and always returns `a` — three trivial-but-lawful semigroups selected as instance values where a placeholder accumulation is needed, e.g. a `struct` field that must survive intact rather than merge; passing `MonoidSum` where a discriminant key must not sum is the bug, `first`/`last` the lawful no-merge column.
- `Semigroup.intercalate(sep)` inserts a separator between every `combine` (joined concatenation); `reverse(S)` flips `combine`'s argument order — a separated or right-biased accumulation is a wrapped existing semigroup, never a rewritten fold.
- `Monoid` exports NEITHER `imap` NOR `intercalate`: a separated combine has no identity (an empty fold cannot decide a leading separator), and an invariant retarget of a monoid would have to retarget `empty` too, which the bare `(to, from)` pair cannot guarantee lawful — so monoid retargeting goes through `Monoid.fromSemigroup(Semigroup.imap(M, to, from), to(M.empty))`, lifting the retargeted semigroup with the explicitly-retargeted identity.

[CONCRETE_VALUE_WIRING]:
- One numeric owner carries four distinct monoids selected by operation, not by flag: `data/Number` `MonoidSum`, `MonoidMultiply`, `MonoidMin`, `MonoidMax`, each a complete `Monoid<number>` with its own `empty` (`0`, `1`, `Infinity`, `-Infinity`). Selecting summation versus extremum is choosing which instance value to pass, never branching a parameterized accumulator.
- `data/Number` also exposes the semigroup-only forms `SemigroupSum`/`SemigroupMultiply`/`SemigroupMin`/`SemigroupMax` for accumulation over guaranteed-non-empty input, plus `Bounded: Bounded<number>` whose `.compare` is the `Order<number>` underlying min/max; `data/BigInt` mirrors it with `SemigroupSum`/`SemigroupMultiply`/`SemigroupMin`/`SemigroupMax` plus `MonoidSum`/`MonoidMultiply` over `bigint`.
- `data/Boolean` carries the full lattice as four monoids — `MonoidEvery` (∧, `empty` `true`), `MonoidSome` (∨, `empty` `false`), `MonoidXor` (`empty` `false`), `MonoidEqv` (`empty` `true`) — so an all-pass predicate fold and an any-pass predicate fold are two instance values over one owner, not two written loops.
- `data/String.Monoid` is concatenation with `empty` `""`; `data/Ordering` exposes both `Monoid<Ordering>` and `Semigroup<Ordering>` over the `-1 | 0 | 1` verdict, the value-typed comparator-result algebra distinct from the `Order<A>` comparator algebra it folds.

[ORDER_BOUNDED_FUSION]:
- `Order<A>` is callable directly — `(self: A, that: A) => -1 | 0 | 1` — so an order instance is invoked, not dispatched through a `.compare` member; `Order.make(compare)` admits a raw comparator once at the boundary, and the `-1 | 0 | 1` return is a TOTALITY contract: every pair is comparable and `0` means equal, so a comparator returning `0` for distinguishable values silently merges them under `min`/`max` and collapses them under sort.
- `Bounded<A>` fuses four facts in one witness: `{ compare: Order<A>; maxBound: A; minBound: A }` plus both extremum monoids — `Bounded.min(B)`/`Bounded.max(B)` derive clamping monoids whose `empty` is the LOSING bound (`maxBound` for `min`, `minBound` for `max`), so the identity element is read off the bound the owner already declares rather than enumerated as a fifth field.
- `Bounded.between(B)` and `Bounded.clamp(B)` derive a membership predicate and a saturating projection from the same witness; `Bounded.reverse(B)` flips `compare` and swaps the two bounds atomically, so an ascending and a descending bounded order are one declaration and its dual, never two records.
- `Semigroup.min(O)`/`Semigroup.max(O)` lift any `Order<A>` to a last-extremum `Semigroup<A>` without bounds; `Monoid.min(B)`/`Monoid.max(B)` require `Bounded<A>` because the identity is a bound — the same extremum logic at two strengths, gated by whether a zero exists. A concrete owner ships the bound as a const (`Number.Bounded`, `Duration.Bounded`) so the factory is fed a fixed witness.

[ORDER_IS_ITS_OWN_MONOID]:
- `effect/Order` exposes a complete monoid OVER COMPARATORS — `combine`, `combineMany`, `combineAll`, and `empty()` all consume and produce `Order<A>` values, so a comparator is both the witness applied to a pair and the value accumulated into a richer comparator.
- `Order.combine(self, that)` returns `self`'s verdict when non-zero and falls through to `that` only on a tie (`out !== 0 ? out : that(a1, a2)`) — leftmost-decisive — and this IS associative with two-sided identity `empty() = make(() => 0)`, the comparator calling every pair equal, so `combineAll([])` is the total "always equal" order and a no-keys sort means "preserve input order" by construction, never an undefined seed.
- `Order.combineMany(self, collection)` seeds with `self` (non-empty fold); `combineAll(collection)` seeds with `empty()` (empty-admitting fold) — the Semigroup/Monoid split applied to the comparator itself, so a multi-key comparator is `combineAll([primary, secondary, tertiary])`, never a nested `if (cmp === 0)` ladder.
- The leftmost-decisive monoid is associative but NOT commutative: `combine(byPriority, byName)` and `combine(byName, byPriority)` are different total orders, so key ORDER in `combineAll` is a CORRECTNESS decision, the one place where the fold's argument order is semantic rather than a performance lever.
- `Order.product`/`all`/`tuple`/`array` are the SAME leftmost-decisive law applied positionally — lexicographic comparison is the order monoid folded over slots, so a tuple order, a multi-key record order, and an array order are one algebra at three arities; `Order.array` adds the length tie-break only after element-wise exhaustion.
- `Order.mapInput(O, f)` contravariantly retargets an order onto a projection, so ordering a record by one field is `Order.mapInput(Order.number, (r) => r.field)`, and `Order.reverse` flips direction — a multi-key sort is a combined chain of mapped single-field orders, reversing one key is wrapping its leaf in `reverse`, and the comparator body never reopens.

[EQUIVALENCE_IS_THE_CONJUNCTION_MONOID]:
- `Equivalence<A>` is a callable `(self, that) => boolean` carrying its own monoid where `combine` is logical AND (`make((x, y) => self(x, y) && that(x, y))`), so a multi-field equality is `combineAll([byId, byVersion, byTag])` — every field must agree, never a hand-spelled `&&` chain re-reading the pair per clause.
- `Equivalence.make(isEquivalent)` wraps every comparator as `self === that || isEquivalent(self, that)` — reference identity short-circuits BEFORE the user predicate, so any constructed equivalence is reflexive-by-reference even when the supplied predicate is not, the reflexivity law enforced for identical references and assumed for distinct ones.
- `combineMany` short-circuits on the first disagreeing witness, so field ORDER in the fold is a performance lever (cheapest-discriminating field first prunes earliest) and NEVER a correctness one — conjunction's commutativity guarantees the verdict is order-independent even though the work is not, the exact inverse of `Order.combine` where order IS correctness.
- `combineAll([])` seeds with the always-true `isAlwaysEquivalent` as `empty`: a zero-field equality means "indistinguishable", the lawful unit of conjunction and the dual of `Order.empty()`'s "equal on no keys".
- `Equivalence.strict<A>()` is the `===` equivalence and `mapInput(self, f)` retargets via `make((x, y) => self(f(x), f(y)))` — contravariant retarget onto a projection, so equality-by-field is `mapInput(Equivalence.string, (o) => o.name)` and the conjunction monoid folds the retargeted leaves; `Equivalence.mapInput` and `Order.mapInput` share the same field accessor, so a field added to the owner adds one row to BOTH the equivalence conjunction and the order chain, never two hand-maintained comparators drifting apart.

[VALUE_OWNER_IDENTITY_IS_THE_EQUIVALENCE]:
- `Equal.equivalence<A>()` is `() => Equal.equals`: the structural `Equal` protocol returned AS an `Equivalence<A>`, so a value owner carrying the `Equal`/`Hash` protocol exposes its own equality to the comparison-algebra surface with zero per-type code — the owner's identity IS the witness, never a second `Equivalence.make` restating field comparison.
- `Equal extends Hash`: a structural-equality owner is obligated to carry `[Hash.symbol](): number`, so any value admitted into a `HashSet`/`HashMap` keyed by structural identity gets its bucket from the same protocol that decides equality — equality and hashing are one fused obligation on the owner, never a pair the consumer maintains in lockstep.
- A `Data.struct`/`Data.array`/`Data.tuple` value or a `Schema.Class`/`Data.TaggedClass` instance carries structural `Equal`/`Hash` by construction, so `Equal.equivalence()` over such an owner compares by deep value — the by-reference equality of a plain object is the defect this deletes, and the value owner that anchors a domain concept already ships the equivalence its algebra needs.
- `Hash.structureKeys(o, keys)` and `Hash.structure(o)` derive the structural hash from a key subset or the full record, and `Hash.cached(self, hash)` memoizes it on the object — a custom value owner implementing `Equal` computes `[Hash.symbol]` once through `structureKeys` over its identity fields, so equality and hashing agree on exactly the same field set by construction.

[SCHEMA_OWNER_DERIVES_THE_EQUIVALENCE]:
- `Schema.equivalence(schema)` produces `Equivalence<A>` for the decoded type from the codec owner alone, so the schema that already owns the type, the decoder, and the encoder also yields its equality — the comparison witness is extracted from the one owner, never a parallel `Equivalence.struct` restating the field shape.
- The schema carries an `equivalence` annotation `(..._: any) => Equivalence.Equivalence<A>` resolved per type-parameter, so a field whose default deep-equality is wrong (a normalized string, a tolerant numeric) overrides equality AT THE DECLARATION as a composition value, and `Schema.equivalence` of the whole owner folds the per-field overrides through the conjunction monoid — the override lands inside the owner, every consumer shifts at compile time.
- Schema-derived equivalence and `Equal.equivalence()` converge for a `Schema.Class` instance: the class carries structural `Equal`, and `Schema.equivalence` of its schema produces the field-wise conjunction — two paths to the same equality, one from the runtime protocol, one from the static codec, and a divergence between them is a declared annotation, never an accident.

[STRUCT_PRODUCT_BUILD]:
- `Semigroup.struct(fields)`/`Monoid.struct(fields)` build the algebra of a record from per-field algebras: `{ readonly [K in keyof R]: ... infer A }` lifts each field's instance into a struct instance whose `combine` merges field-wise, and `Monoid.struct` derives the struct `empty` as the record of each field's `empty` — adding a field to the owner adds one row to the `fields` argument and the identity extends with zero hand-edits.
- `Semigroup.tuple(...)`/`Monoid.tuple(...)` are the positional analogues with the same lift; `Semigroup.array<A>()`/`Monoid.array<A>()` give concatenation algebras whose elements need no instance because the operation is structural.
- `effect/Struct.getOrder(fields)` and `Struct.getEquivalence(fields)` derive `Order`/`Equivalence` for a record from per-field instances, the same field-lift shape as `Semigroup.struct` but for comparison and equality — one record owner yields its ordering, its equality, and its accumulation algebra from the same per-field instance maps, never three parallel hand-written comparators.
- `Order` and `Equivalence` are both interfaces with their own `TypeLambda` (`OrderTypeLambda`, `EquivalenceTypeLambda`), so they slot as `F` into the generic typeclass machinery and `Order.product`/`Equivalence.product` build tuple comparisons applicatively — comparison instances are themselves carrier values composed by the same product algebra that builds value structs.

[SEMIPRODUCT_APPLICATIVE_BUILD]:
- `SemiProduct<F>.product(self, that)` pairs two carrier values into a carrier of `[A, B]`, accumulating `R1 & R2`, `O1 | O2`, `E1 | E2` across the channels — applicative struct-building is the product of independent carrier values, with requirements intersected and outputs/errors unioned by the carrier, never a flag.
- `SemiProduct.nonEmptyStruct(F)(fields)`/`nonEmptyTuple(F)(...)` and `Product.struct(F)(fields)`/`Product.tuple(F)(...)` lift a record or tuple of carrier values into a carrier of the record, the applicative dual of `Semigroup.struct`; `EnforceNonEmptyRecord<R> = keyof R extends never ? never : R` makes an empty `nonEmptyStruct` argument resolve to `never`, so a zero-field applicative struct is a compile error at the semi level.
- `Semigroup` itself carries `Semigroup.SemiProduct`/`Semigroup.Product` instances over `SemigroupTypeLambda`, so semigroups compose applicatively into struct-shaped semigroups through `Product.struct` and `SemiProduct.nonEmptyStruct` — the same applicative machinery builds value structs and algebra structs.
- `SemiProduct.andThenBind(F)` and `SemiProduct.appendElement(F)` accrete one named field or one tuple slot at a time onto an in-progress carrier struct, each widening the result type with `K in keyof A | N`, so a do-style build grows by one bound name per step with the full channel accumulation threaded through.
- `Product<F> extends SemiProduct<F>, Of<F>` adds `productAll` (an `Iterable` of carriers to a carrier of `Array<A>`); `Of<F>.of` supplies the single-value lift that seeds an empty product, so `Of` plus `SemiProduct` is exactly what `Product` fuses.

```typescript
import { Monoid } from '@effect/typeclass'
import * as N from '@effect/typeclass/data/Number'
import * as B from '@effect/typeclass/data/Boolean'

const Tally = Monoid.struct({ hits: N.MonoidSum, peak: N.MonoidMax, clean: B.MonoidEvery })

const fold = <A>(M: Monoid.Monoid<A>) => (rows: Iterable<A>): A => M.combineAll(rows)
const summary: (typeof Tally)['empty'] = fold(Tally)([
    { hits: 1, peak: 7, clean: true },
    { hits: 1, peak: 3, clean: false },
])
```

[DO_BUILDER_DEGRADES_TO_THE_WEAKEST_WITNESS]:
- The do-notation builder is assembled per combinator from the minimum witness it needs, never from the carrier: `bindTo(Invariant)` and `tupled(Invariant)` need only invariance; `let(Covariant)`, `as(Covariant)`, `flap(Covariant)` need covariance; `bind(Chainable)` and `tap(Chainable)` need `Chainable<F> = FlatMap<F> & Covariant<F>`.
- A pure-covariant owner (no `flatMap`) still builds a struct via `let` (pure field from prior fields) and `andThenBind(SemiProduct)` (independent carrier field) but CANNOT `bind`, because `bind` sequences a dependent carrier — the builder degrades exactly to the algebra the owner provides, surfacing missing structure as an absent combinator rather than a runtime gap.
- `bind`/`tap`/`zipLeft` (Chainable) thread the full `R1 & R2`, `O1 | O2`, `E1 | E2` channel accumulation through each step, so a do-chain over a carrier owner accumulates requirements by intersection and errors by union at compile time, the channel arithmetic the witness performs.
- `let`'s name parameter is `Exclude<N, keyof A>`: rebinding an existing field is a compile error, so a builder grows by one provably-fresh key per step and the result type is `{ [K in keyof A | N]: K extends keyof A ? A[K] : B }`.

[DERIVING_ALGEBRA_THROUGH_THE_CARRIER]:
- `SemiApplicative.getSemigroup(F)(S)` lifts a `Semigroup<A>` into `Semigroup<Kind<F, R, O, E, A>>`: the carrier's `product` combined with `S.combine` on the paired values gives a semigroup over carrier values whose inner content accumulates — `make((self, that) => F.map(F.product(self, that), ([a, b]) => S.combine(a, b)))`, one elementwise semigroup derived from the elementwise algebra plus the carrier's applicative.
- `Applicative.getMonoid(F)(M)` is the monoid-strength lift, where `combine` comes from `SemiApplicative.getSemigroup` and `empty` is `F.of(M.empty)` — the identity carrier value is the lifted inner identity, derived not declared; `getSemigroup` requires only `SemiApplicative` (no identity), so promoting to a lifted `Monoid` is exactly where `Applicative` is needed for `F.of(M.empty)`.
- `SemiApplicative.lift2(F)(f)` lifts any binary `(a, b) => c` into the carrier (`zipWith` curried on the function first), so a value-level merge becomes a carrier-level merge with full channel accumulation — the bridge from a bare combine to its lifted form without touching `product` directly.
- `Option.getOptionalMonoid(S)` derives `Monoid<Option<A>>` from a bare `Semigroup<A>` with `None` as `empty`: two `Some`s combine their contents via `S`, a `Some` and a `None` keep the `Some` — the optional carrier supplies the identity a non-empty semigroup lacks, the canonical promotion of a semigroup-only inner type to a monoid.
- `Foldable.combineMap(F)(M)(f)` folds any foldable carrier by mapping each element to `M` and combining — the witness-driven `foldMap`: changing the accumulation is swapping the `Monoid` argument, never rewriting the traversal. `Foldable.toArray(F)`/`toArrayMap(F)(f)` are the same fold specialized to the array monoid.

```typescript
import { Foldable } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as N from '@effect/typeclass/data/Number'
import * as O from '@effect/typeclass/data/Option'
import { pipe } from 'effect'

const Span = O.getOptionalMonoid(N.SemigroupMax)
const widest = Foldable.combineMap(ArrayInstances.Foldable)(Span)
const result = pipe([{ k: 'a' }, { k: 'b' }], widest((r) => (r.k === 'a' ? O.Of.of(5) : O.Of.of(2))))
```

[RECORD_KEYED_ALGEBRA]:
- `data/Record.getMonoidUnion(M)` derives `Monoid<ReadonlyRecord<string, A>>` where shared keys combine via `M` and disjoint keys pass through, `empty` is `{}` — a keyed accumulator (per-key counters, per-key maxima) is the union monoid over the value monoid, never a hand-merged reducer; `getMonoidUnion(N.MonoidMax)` versus `getMonoidUnion(N.MonoidSum)` are per-key-maximum and per-key-sum over one owner, the merge chosen by the value witness, never a reducer rewritten per policy.
- `getSemigroupUnion(S)` is the non-empty union; `getSemigroupIntersection(S)` keeps only keys present in both records and combines their values — union and intersection are two derivations over the same value `Semigroup`, selecting set behavior by which combinator wraps the witness.
- `data/Record` exposes the carrier instances per key type through `getCovariant<K>()`/`getFilterable<K>()`/`getTraversable<K>()`/`getTraversableFilterable<K>()` factories alongside the `string`-keyed defaults, so a literal-key record participates in carrier-polymorphic composition with its key type preserved.

[IDENTITY_DEGENERATE_CARRIER]:
- `data/Identity` is `type Identity<A> = A` with the full instance stack (`Monad`, `Applicative`, `Foldable`, `Traversable`, etc.) over `IdentityTypeLambda` — the carrier that is its own content, so a `Foldable`/`Traversable`-polymorphic algorithm runs over a bare value by passing the `Identity` witness, with no wrapper allocated.
- `Identity.getSemiCoproduct(S)` and `getSemiAlternative(S)` need a `Semigroup<A>` because the bare-value carrier has no structural choice point — the coproduct of two bare values is decided by combining them, so the alternative algebra is supplied by the value's own semigroup, the reason the factory is parameterized rather than a constant instance.
- `IdentityTypeLambdaFix<A>` sets `type` to a fixed `Identity<A>` rather than threading `this["Target"]`, the encoding that lets a fixed-content carrier satisfy a type-lambda-shaped typeclass; the unfixed `IdentityTypeLambda` is `Identity<this["Target"]>`, the polymorphic form, and the two coexist so the same module serves both content-generic and content-fixed instance consumers.

[CHAINING_IS_THE_FLATMAP_BULK_FAMILY]:
- `FlatMap<F>` carries one member, `flatMap`, and every sequencing combinator is a FREE function over it: `flatten(F)` is `flatMap(identity)` collapsing `Kind<F,…,Kind<F,…,A>>` one layer with the channels merging (`R1 & R2`, `O1 | O2`, `E1 | E2`), `composeK(F)(afb, bfc)` is Kleisli composition fusing `(a) => F<B>` and `(b) => F<C>` into `(a) => F<C>`, and `zipRight(F)(that)` sequences then discards the left value — bulk operations the owner never re-spells.
- `composeK` is the arrow-level dual of `flatMap`: where `flatMap` threads a value through one continuation, `composeK` pre-fuses two continuations into one, so a multi-stage decoding/normalizing pipeline over a value owner is built as a single composed Kleisli arrow applied once, not a nested `flatMap` ladder re-opened per stage; the channels merge across the fusion so the composed arrow's failure set is the union of every stage's.
- A new sequencing semantics is a new combinator selecting `FlatMap<F>` as its witness, never a new method on the owner's instance — the owner ships `flatMap` and the family (`flatten`, `composeK`, `zipRight`) is generated identically for every value owner that supplies it.

[ZIPRIGHT_VS_ZIPLEFT_PLACEMENT_LAW]:
- `zipRight` is the ONLY combining function `FlatMap<F>` exports beyond derivation, because keeping the SECOND value requires only running both effects in order and returning the latter — no re-read of the discarded first value, so `flatMap` alone suffices.
- `zipLeft`, `tap`, and `bind` live on `Chainable<F> = FlatMap<F> & Covariant<F>`: keeping the FIRST value after sequencing the second (`zipLeft`), re-yielding the original after a side-effect (`tap`), or widening an accumulating struct (`bind`) all re-surface a held value through a `map`, so the witness must add `Covariant` — passing a `FlatMap`-only owner to any of them is a compile error, never a silent identity.
- The placement is the executable proof of what each operation costs: an operation that only sequences is `FlatMap`-gated; an operation that sequences AND re-reads a prior value is `Chainable`-gated.

[ONE_NAME_TWO_WITNESSES_DEPENDENT_VS_INDEPENDENT]:
- `zipRight` and `zipLeft` BOTH exist on `SemiApplicative<F>` as well, with the same signatures but a different mechanism: the applicative forms run the two effects INDEPENDENTLY via `product`, the chainable/flatmap forms run them DEPENDENTLY via `flatMap` — same result type, opposite evaluation contract.
- The selection is the import, not a flag: a value owner whose `that` carrier must not be constructed unless `self` succeeds takes the `FlatMap`/`Chainable` form; a value owner whose channels accumulate regardless takes the `SemiApplicative` form — the dependence axis of the doctrine surfaces as which module the combinator is read from.
- `zipWith(SemiApplicative)(that, f)` is the general applicative pairing both `zipLeft`/`zipRight` and `lift2` specialize — `f = (a, _) => a`, `f = (_, b) => b`, and the function-first curry respectively — the one binary-merge primitive the three combinators select different argument bindings of.

[COPRODUCT_RECOVERY_STOPS_AT_THE_IDENTITY_BOUNDARY]:
- `SemiCoproduct<F>.coproduct(self, that)` selects between two carrier values yielding `A | B`, the first-success / choice algebra dual to `product`'s pairing; `coproductMany` folds a NON-EMPTY collection seeded by `self`. `Coproduct<F> extends SemiCoproduct<F>` adds `zero<A>()` (the always-failing carrier with channels `unknown, never, never`) and `coproductAll` (an EMPTY-admitting fold) — the split is whether a no-match identity exists, the same boundary that separates `Semigroup` from `Monoid`.
- `data/Option` carries the FULL `Coproduct` and `Alternative` because `None` IS the `zero` identity — an empty fallback chain resolves to `None`; `data/Either` carries only `SemiCoproduct` and `SemiAlternative`, never `Coproduct`/`Alternative`, because `Either` has no neutral failure value to serve as `zero` (a `Left` requires a witness `E`), so an empty Either alternation is uninhabitable and the missing instance makes that a compile-time fact.
- The asymmetry is the absorption signal: a recovery fold over `Option` admits the empty-collection case (`coproductAll` / `getMonoid(Coproduct)()`), a recovery fold over `Either` requires at least one alternative (`coproductMany` / `getSemigroup(SemiCoproduct)()`), and choosing the wrong strength surfaces as an absent export, never a runtime empty-list crash.

[ALTERNATIVE_LIFTS_THE_CHOICE_INTO_A_SEMIGROUP]:
- `SemiCoproduct.getSemigroup(F)()` lifts the choice into `Semigroup<Kind<F,…,A>>` whose `combine` is `coproduct` — a fallback chain becomes `combineMany` over that semigroup, and `Coproduct.getMonoid(F)()` adds `empty = zero` so an alternation list with an explicit no-match identity is `combineAll` over the coproduct monoid, never a `reduce` seeded by a hand-written sentinel.
- `SemiAlternative<F> = SemiCoproduct<F> & Covariant<F>` and `Alternative<F> = Coproduct<F> & SemiAlternative<F>`: the `Covariant` addition is what lets a choice algebra also retarget its content, so an alternative owner both selects a winner and maps it — the recovery hierarchy mirrors the chaining hierarchy's `FlatMap → Chainable` step, `Covariant` joining at the same rung for the same reason.

```typescript
import * as OptionInstances from '@effect/typeclass/data/Option'
import { FlatMap, Coproduct } from '@effect/typeclass'
import { Option, pipe } from 'effect'

const decode = (raw: string): Option.Option<number> => (raw.length > 0 ? Option.some(raw.length) : Option.none())
const bound = (n: number): Option.Option<number> => (n < 64 ? Option.some(n) : Option.none())
const stage: (raw: string) => Option.Option<number> = FlatMap.composeK(OptionInstances.FlatMap)(decode, bound)

const FirstSome = Coproduct.getMonoid(OptionInstances.Coproduct)<unknown, never, never, number>()
const checked: Option.Option<number> = pipe('<value-a>', stage)
const winner: Option.Option<number> = FirstSome.combineAll([Option.none(), Option.some(7), Option.some(3)])
const absent: Option.Option<number> = FirstSome.combineAll([])
```

[OUTER_STRUCTURE_INNER_ALGEBRA]:
- `Traversable<T>.traverse` takes TWO witnesses, never one: `traverse(F: Applicative<F>)` curries the inner algebra first, then `(self: Kind<T,…,A>, f: (a: A) => Kind<F,…,B>)` walks the structure `T` with the effect `F` — the outer witness owns the walk shape, the inner witness owns what accumulates, and swapping `F` reshapes the result with zero change to `T` or `f`.
- `Foldable<T>` is the minimum walk witness: its sole member is `reduce`, and `combineMap(F)(M)`, `reduceKind(F)(G)`, `coproductMapKind(F)(G)`, `toArray(F)`, `toArrayMap(F)(f)` are ALL free functions deriving the bulk operation from that one `reduce` plus a SECOND witness — the `Monoid`, the `Monad<G>`, or the `Coproduct<G>` that decides the fold's algebra.
- The structure witness and the algebra witness are independent axes: `combineMap(ArrayFoldable)(MonoidSum)` and `combineMap(OptionFoldable)(MonoidSum)` share the monoid and differ only in walk; `combineMap(ArrayFoldable)(MonoidMax)` and `combineMap(ArrayFoldable)(MonoidSum)` share the walk and differ only in algebra — a 2-D dispatch over (owner, algebra) where naive code writes one loop per cell, and a standalone reducer hardcoding both pins one cell of the grid the witness pair spans.
- `data/Array`, `data/Option`, `data/Either` each carry the complete stack (`Foldable`, `Traversable`, `Filterable`, `Applicative`, `Monad`), so any of them can serve as EITHER the outer structure witness OR the inner threaded applicative — the roles assigned by argument position, not by type; `data/Record` ships `getTraversable<K>()`/`getFilterable<K>()`/`getTraversableFilterable<K>()` as key-typed FACTORIES so a literal-key record traverses with its key type preserved through the walk.

[APPLICATIVE_SELECTS_TRAVERSAL_SEMANTICS]:
- The inner `Applicative<F>` passed to `traverse` IS the policy: `Option.Applicative` makes `traverse` short-circuit to `None` on the first failing element collapsing `T<Option<B>>` to `Option<T<B>>`; `Either.Applicative` short-circuits to the first `Left` (`product` is `isRight(self) ? (isRight(that) ? right([…]) : left(that.left)) : left(self.left)` — leftmost failure kept, rest dropped); the lifted `Applicative.getMonoid(F)(M)` over an applicative accumulates — one `traverse` body, three semantics, chosen by which witness arrives, so reaching for `Either.Applicative` where every fault must survive keeps only the first.
- `sequence(T)(F)` is `traverse` with `f = identity`: `Kind<T,…,Kind<F,…,A>>` to `Kind<F,…,Kind<T,…,A>>` flips the two layers, the canonical "turn a structure of effects into an effect of a structure".
- `traverseTap(T)(F)(f)` runs the inner effect for each element and discards `B`, returning `Kind<F,…,Kind<T,…,A>>` with the ORIGINAL `A` content — the validating walk where each element triggers an effect whose value is irrelevant and only the channel accumulation survives.

```typescript
import { Traversable } from '@effect/typeclass'
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as O from '@effect/typeclass/data/Option'
import { Option } from 'effect'

const validateAll = Traversable.sequence(ArrayInstances.Traversable)(O.Applicative)
const parsePositive = (n: number): Option.Option<number> => (n > 0 ? Option.some(n) : Option.none())
const traverse = ArrayInstances.Traversable.traverse(O.Applicative)

const kept: Option.Option<ReadonlyArray<number>> = traverse([3, 7, 1], parsePositive)
const flipped: Option.Option<ReadonlyArray<number>> = validateAll([Option.some(2), Option.some(9)])
```

[FILTER_IS_EITHER_OPTION_VALUED_MAP]:
- `Filterable<F>` reduces filtering to TWO members that each return a discriminating carrier: `partitionMap(f: (a: A) => Either<C, B>)` splits one structure into `[F<B>, F<C>]` by the `Either` tag, and `filterMap(f: (a: A) => Option<B>)` keeps the `Some` payloads — a refinement is an `Either`/`Option`-valued function, never a boolean knob threading a second pass.
- `filter`, `partition`, `compact`, `separate` are ALL derived from those two: `filter(F)(predicate)` is `filterMap` of a predicate-gated `Option`, `compact(F)` is `filterMap(identity)` over `F<Option<A>>`, `separate(F)` is `partitionMap(identity)` over `F<Either<B,A>>` — four named entrypoints, one `Either`-or-`Option` valued core each.
- `filter`/`partition` carry refinement overloads `(a: A) => a is B` that NARROW the element type in the output carrier (`Kind<F,…,B>`), so a type-guard filter tightens `T<A>` to `T<B>` at compile time, a guarantee a boolean predicate cannot make.
- `partitionMapComposition(F, G)` and `filterMapComposition(F, G)` derive the filter for a `Covariant<F>`-of-`Filterable<G>` nesting from the two layer witnesses, so filtering a structure of filterable structures is one composed witness.

[TRAVERSAL_FILTER_FUSION]:
- `TraversableFilterable<T>` fuses the structure-walk, the inner applicative effect, and the partition into ONE pass: `traverseFilterMap(F)(f: (a: A) => Kind<F,…,Option<B>>)` walks `T`, runs each element's `F` effect, and keeps only the inner `Some`s — a filter whose KEEP decision is itself effectful, expressed without a separate `traverse` then `filterMap`.
- `traversePartitionMap(F)(f: (a: A) => Kind<F,…,Either<C, B>>)` returns `Kind<F,…,[T<B>, T<C>]>`: an effectful split into rejected-`C` and accepted-`B` halves in one walk, the two-channel dual of `traverseFilterMap` where both sides survive.
- `traverseFilter(F)(predicate)` and `traversePartition(F)(predicate)` are the boolean specializations carrying the same refinement-overload narrowing — the keep test runs inside `F`, so an effectful refinement narrows the element type while accumulating the inner channel.
- The default free functions DERIVE the instance from the constituent witnesses: `traverseFilterMap(T)` needs `Traversable<T> & Filterable<T>`, `traversePartitionMap(T)` additionally needs `Covariant<T>` — an owner that already carries the walk and the filter gets effectful partitioning for free, never a fourth hand-written instance member.

```typescript
import * as ArrayInstances from '@effect/typeclass/data/Array'
import * as O from '@effect/typeclass/data/Option'
import { Either, Option } from 'effect'

const split = ArrayInstances.TraversableFilterable.traversePartitionMap(O.Applicative)
const classify = (n: number): Option.Option<Either.Either<number, number>> =>
    n === 0 ? Option.none() : Option.some(n > 0 ? Either.right(n) : Either.left(n))

const halves: Option.Option<[ReadonlyArray<number>, ReadonlyArray<number>]> = split([5, -2, 8], classify)
```

[FOLD_WITH_AN_EFFECTFUL_ACCUMULATOR]:
- `reduceKind(F)(G: Monad<G>)(b, f: (b: B, a: A) => Kind<G,…,B>)` is the monadic left fold: the accumulator step returns a carrier `G`, so each element's contribution is sequenced through `G`'s `flatMap`, threading `G`'s channel accumulation across the whole walk — a fold whose every step can fail, branch, or require, with the structure witness `F` supplying only the iteration order.
- `coproductMapKind(F)(G: Coproduct<G>)(f: (a: A) => Kind<G,…,B>)` folds with the COPRODUCT instead of the monad: each element maps into `G` and the results are alternated via `G.coproduct`, so a "first element that succeeds in `G`" search over any foldable `F` is `coproductMapKind`, the choice-fold dual to `reduceKind`'s sequencing-fold.
- `combineMap(F)(M)` is the same fold specialized to a `Monoid` accumulator with no carrier — three folds over one `Foldable`, gated by whether the accumulator is a plain monoid, a sequencing carrier, or a choosing carrier; `Foldable` exposes ONLY `reduce`, so a new fold semantics is a new combinator selecting a new second witness, never a new member on the owner's instance.

[SCALE_AXES_ARE_INSTANCE_SWAPS]:
- A pipeline reading "walk this owner, run an effectful check per element, keep the survivors, summarize" is FOUR independent instance choices — the `Traversable`/`Filterable` owner, the inner `Applicative` deciding short-circuit vs accumulate, the `Option`/`Either` discriminant deciding filter vs partition, the `Monoid`/`Coproduct` deciding the summary — and each axis changes by passing a different witness, never by editing the pipeline body.
- Adding a new structure owner is one new `Traversable`/`Foldable` instance and zero changes to the algorithm; adding a new accumulation is one new `Monoid`/`Monad`/`Coproduct` and zero changes to the walk — the witness-pair pattern makes both growth axes additive at the call site, the multiplicative collapse the two-witness shape buys.
- `traverseComposition(T, G)(F)` and `reduceComposition(F, G)` derive the walk/fold for a `T`-of-`G` nesting from the two layer witnesses, so a stacked structure (records of arrays, arrays of options) gets its traversal and fold from composing the layer instances — the nesting is never spelled by hand, and a third layer is a third composition, not a rewritten recursion.

[EXPORT_SHAPE_NAMES_THE_FREE_DATUM]:
- An instance is exported as a bare `const` when its behavior is fully fixed by the carrier, and as a `get*`/`<A>()` factory when one datum is undetermined — the export form is the witness's signature of what it cannot decide alone, read off before any call site.
- The factory's parameter list IS the free datum and nothing else: a value parameter means a sub-algebra is needed (`getOptionalMonoid(S: Semigroup<A>)`), a phantom `<A>()` means only the element type floats (`Array.getMonoid<A>()`), a `(options)` parameter means a runtime policy is unfixed (`getApplicative(options?)`), and a `<K extends string>()` parameter means the carrier's own shape is parameterized (`getCovariant<K>()`).
- Choosing the instance is choosing the export and feeding its one parameter once; threading the same free datum as a flag at every call site is the re-derivation this collapses — the witness absorbs the policy so the consumer body never re-decides it. Every factory parameter is a domain value carrying its own behavior — a `Semigroup`, a `Monoid`, a `Bounded`, a `ConcurrencyOptions` policy, a key-type witness — never a boolean knob whose combinations the instance re-derives.
- The absence of an expected factory is a load-bearing compile fact: `data/Either` exports no `getMonoid`/`Coproduct`/`Alternative`, so the empty-alternation case is foreclosed at the type level rather than guarded at runtime — the missing export is the strength boundary made checkable.

[CONSTANT_INSTANCES_ARE_FULLY_DETERMINED]:
- The sequencing stack is constant across every value owner — `Covariant`, `Of`, `Pointed`, `FlatMap`, `Chainable`, `Monad` ship as bare consts on `data/Array`, `data/Option`, `data/Either`, `data/Effect`, `data/Micro` — because `map`, `of`, and `flatMap` have exactly one lawful behavior per carrier with no free choice to surface.
- `Monad<F> extends FlatMap<F>, Pointed<F>` and `Pointed<F> extends Covariant<F>, Of<F>`: a monad const is mechanically the fusion of two consts, so an owner exporting `FlatMap` and `Pointed` separately also exports their join `Monad` with zero additional datum — the constant nature propagates up the interface lattice.
- A constant instance is passed by import alone, a factory instance by one application; the distinction is invisible in the consuming pipeline body because both arrive as a fully-formed witness — the policy is resolved at the witness boundary, never inside the fold.

[CONCURRENCY_IS_THE_POLICY_PARAMETER_OF_THE_APPLICATIVE]:
- `data/Effect` and `data/Micro` split their stack precisely at the dependence boundary: `Monad`/`FlatMap`/`Chainable` are CONSTS (sequential evaluation is forced by data dependence), while `getSemiProduct`, `getProduct`, `getSemiApplicative`, `getApplicative` are FACTORIES over `ConcurrencyOptions` because independent composition admits a parallelism choice the monad cannot.
- `ConcurrencyOptions` is `{ readonly concurrency?: Concurrency; readonly batching?: boolean | "inherit" }` on `data/Effect` (`Micro` drops `batching`), and `Concurrency = number | "unbounded" | "inherit"` — the degree of parallel evaluation rides the applicative witness as a policy value, so a bounded-parallel struct-build and an unbounded one are two witnesses over one owner, never a branched pipeline.
- The applicative-factory parameterization is the executable proof of the doctrine's dependence/independence split: a `flatMap` chain's order is forced and its instance is constant, an independent product's order is a policy and its instance is a factory — the channel that selects the algebra also selects whether the instance can be a const.
- `getApplicative(options)` returns a witness whose `product` runs both effects at the declared concurrency, so `SemiApplicative.getSemigroup(getApplicative({ concurrency: "unbounded" }))(S)` is an elementwise accumulation over carrier values that fuses content via `S` while evaluating the carriers in parallel — the concurrency policy and the value algebra arrive as two independent witness parameters at one combine site.

```typescript
import { SemiApplicative } from '@effect/typeclass'
import * as EffectInstances from '@effect/typeclass/data/Effect'
import * as N from '@effect/typeclass/data/Number'
import type { Semigroup } from '@effect/typeclass/Semigroup'
import { Effect } from 'effect'

const merge = <A>(S: Semigroup<A>, opts: EffectInstances.ConcurrencyOptions): Semigroup<Effect.Effect<A>> =>
    SemiApplicative.getSemigroup(EffectInstances.getSemiApplicative(opts))(S)

const Peak: Semigroup<Effect.Effect<number>> = merge(N.SemigroupMax, { concurrency: 'unbounded' })
const widest: Effect.Effect<number> = Peak.combineMany(Effect.succeed(1), [Effect.succeed(7), Effect.succeed(3)])
```

[VALUE_ALGEBRA_FACTORIES_CARRY_THE_INNER_WITNESS]:
- A carrier whose accumulation needs an inner algebra exports that need as the factory parameter: `Option.getOptionalMonoid(S: Semigroup<A>)`, `Record.getMonoidUnion(M: Monoid<A>)`, `Record.getSemigroupUnion(S)`, `Record.getSemigroupIntersection(S)` — the carrier supplies the structural merge and the parameter supplies how matched contents combine.
- `Applicative.getMonoid(F)(M: Monoid<A>)` and `SemiApplicative.getSemigroup(F)(S: Semigroup<A>)` are TWO-STAGE factories: stage one fixes the carrier witness, stage two fixes the inner value algebra, so a lifted instance is built by feeding the two free data in order and a swap of either stage reshapes the result with no body edit.
- `getOptionalMonoid`'s factory parameter is provably WEAKER than its result — a bare `Semigroup<A>` in, a `Monoid<Option<A>>` out — because the carrier contributes the missing identity, the signature where the input-output strength gap discloses that the structure is supplied by the carrier and not the parameter.

```typescript
import { getMonoidUnion } from '@effect/typeclass/data/Record'
import * as N from '@effect/typeclass/data/Number'
import type { Monoid } from '@effect/typeclass/Monoid'
import type { ReadonlyRecord } from 'effect/Record'

const Tally: Monoid<ReadonlyRecord<string, number>> = getMonoidUnion(N.MonoidSum)
const folded = Tally.combineAll([{ a: 1, b: 2 }, { b: 3, c: 5 }, Tally.empty])
const peaks = getMonoidUnion(N.MonoidMax).combine({ a: 1 }, { a: 9, b: 4 })
```

[NULLARY_AND_KEYED_FACTORIES_PIN_TYPES_WITH_NO_VALUE]:
- `Array.getMonoid<A>()`, `Array.getSemigroup<A>()`, `Monoid.array<A>()` take a type parameter but no value: array concatenation needs no element algebra, so the only free datum is the phantom element type the factory pins for inference — a value-free factory whose call exists solely to fix `A`.
- `Coproduct.getMonoid(F)<R, O, E, A>()` and `SemiCoproduct.getSemigroup(F)<R, O, E, A>()` are value-free at the second stage because `zero`/`coproduct` supply the identity and choice from the carrier itself, so the trailing `()` pins the four channel/content type parameters and contributes nothing else; the contrast `Applicative.getMonoid(F)(M)` versus `Coproduct.getMonoid(F)()` is the sharpest cut of the facet — same name, same result shape `Monoid<Kind<F,…,A>>`, but the applicative form takes the inner `Monoid` because the identity is the lifted inner identity, while the coproduct form takes nothing because the identity is the carrier's `zero`.
- `data/Record` exports each carrier instance twice: a `const` defaulted at `K = string` and a `get*<K extends string>()` factory whose type parameter pins the literal key type into `ReadonlyRecordTypeLambda<K>` so the walk preserves the key type instead of flattening it — the free datum is neither a value nor a sub-algebra but the CARRIER'S OWN type lambda shape, the carrier-shape analogue of the concurrency policy. Reaching for the defaulted const where a literal-key map must survive the walk is the silent widening the factory form forecloses, the key type recovered through `getTraversable<"<key-a>" | "<key-b>">()`.

[BOUNDED_FACTORY_INPUT_CARRIES_THE_IDENTITY]:
- `Bounded.min(B: Bounded<A>)` and `Bounded.max(B)` are factories whose single argument carries the extremum monoid's identity inside it: the bound feeds its own `compare` as the last-extremum semigroup and its OPPOSITE bound as `empty`, so `min` seeds with `maxBound` and `max` with `minBound` — the identity is read off the witness, never passed as a literal.
- `Monoid.fromSemigroup(S, empty)` is the explicit-identity factory and `Bounded.min/max` are its bound-derived specializations: passing a `Bounded` instead of a `(Semigroup, literal)` pair makes a wrong identity unrepresentable, because the bound the order already declares IS the only lawful extremum identity. A concrete owner ships the bound as a const (`Number.Bounded`, `Duration.Bounded`), so an ascending and descending extremum fold are one bound and its `reverse`, two factory calls over one declaration.

[FACTORY_SELECTION_IS_THE_MULTIPLICATIVE_COLLAPSE]:
- `getMonoidUnion` and `getSemigroupIntersection` over the same `data/Record` owner select union versus intersection set behavior by WHICH FACTORY is read, and the per-key merge by which value witness is fed — two orthogonal axes, both resolved as instance selection plus one parameter, never a mode argument inside one merge function.
- A pipeline reading "build this struct in parallel, accumulate matched keys by max, fall back through alternatives, fold extremes from a bound" is four factory applications — `getApplicative(opts)`, `getMonoidUnion(MonoidMax)`, `Coproduct.getMonoid(F)()`, `Bounded.max(B)` — each absorbing one free datum, and changing any axis is re-feeding one factory while the pipeline body stays fixed.

[LAW_IS_THE_UNCHECKED_PREMISE]:
- `Semigroup<A>` is `{ combine, combineMany }` with NO law field and NO runtime check: associativity is the silent premise every derived combinator assumes, so `combineMany`, `Monoid.combineAll`, `Foldable.combineMap`, and `SemiApplicative.getSemigroup` all produce a defined-but-wrong value when `combine` is non-associative — the fold runs, the type checks, the result is incoherent.
- Supplying a custom `combineMany` that folds in any association other than the default strict left fold is a lawful optimization ONLY under associativity; a non-associative `combine` makes the default and a custom fold diverge, and the divergence is the proof the law was violated.
- `Monoid.combineAll` over an empty `Iterable` returns `empty` and over a singleton returns that element unchanged — both depend on `empty` being a genuine TWO-SIDED identity (`empty ⊕ a = a = a ⊕ empty`); a near-identity that is left-neutral but not right-neutral passes every singleton test and corrupts only multi-element folds, the trap a one-element unit test never surfaces.
- `Foldable.combineMap(F)(M)(f)` is `F.reduce(self, M.empty, (m, a) => M.combine(m, f(a)))`: the seed is `M.empty` and every step left-combines, so the summary is correct iff `M.empty` is a LEFT identity and `combine` is associative — the witness ships the obligation, the consumer inherits it, and no signature records it.

[REVERSE_AND_DUAL_ARE_LAWFUL_BY_CONSTRUCTION]:
- `Semigroup.reverse(S)` builds `combine(self, that) => S.combine(that, self)`: argument-flipping preserves associativity mechanically, so the dual of a lawful semigroup is lawful with zero re-proof, and `reverse(reverse(S))` is `S` — an involution, not a fresh instance.
- `Monoid.reverse(M)` swaps `combine` but KEEPS `empty`: a two-sided identity stays a two-sided identity under argument flip, which is exactly why `Monoid` CAN expose `reverse` while a separated combine cannot — the identity is invariant under the dual, the separator is not.
- `reverse` on a COMMUTATIVE semigroup (`MonoidSum`, `MonoidMultiply`, `MonoidEvery`) is the identity transform — the dual collapses to the original — so reaching for `reverse` over a commutative algebra is a no-op the type system permits but the algebra forbids meaning; `reverse` carries information only over a non-commutative owner (`String.Monoid`, `Semigroup.last`, struct accumulation with order-sensitive fields).
- `Semigroup.reverse`'s custom `combineMany` reverses the collection then folds appending `self` LAST, the mirror of the forward seed-first fold, so the dual's bulk operation agrees with `combine` only because associativity lets the reassociated reverse-fold equal the flipped forward fold.

[INTERCALATE_IS_THE_LAW_THAT_FORBIDS_AN_IDENTITY]:
- `Semigroup.intercalate(S, sep)` is `make((self, that) => S.combineMany(self, [sep, that]))` — it injects `sep` between every pair, and this is the ONE combinator with no `Monoid` counterpart: a separating combine has no two-sided identity because any candidate `e` would force `e ⊕ a` to drop the leading separator, contradicting `combine`'s own shape.
- The absent `Monoid.intercalate` is a load-bearing API fact, not an omission: the missing export encodes "this algebra is a Semigroup and cannot be promoted", so an empty intercalated fold is unrepresentable at the type level — the strength boundary between Semigroup and Monoid is materialized as which module carries the combinator.
- `intercalate` is associative (the separators distribute consistently across nesting) but NOT idempotent and NOT identity-bearing — a witness that occupies the exact algebraic slot "associative, no unit", proving the Semigroup/Monoid split is a real lattice rung the library refuses to bridge, never a convenience tier.

[FIRST_LAST_CONSTANT_ARE_THE_DEGENERATE_LAWFUL_INSTANCES]:
- `Semigroup.first()` (`(a, _) => a`) and `Semigroup.last()` (`(_, b) => b`) are associative but NON-COMMUTATIVE and idempotent-on-equal-inputs: `first` always projects left, `last` always right, so they are the lawful "do not merge, pick a side" instances — the placeholder a `struct` field takes when its column must survive intact.
- `last`'s custom `combineMany` loops `for (a of collection) {}` keeping the final element — it cannot delegate to `combine` left-folded because that would keep the first; the bespoke fold is the proof that `last`'s bulk semantics ("the rightmost") is associativity-coherent only through a dedicated traversal, not the default reducer.
- `Semigroup.constant(a)` (`() => a`, ignoring both operands) is associative and idempotent but is a unit ONLY for itself: `combine(a, a) = a`, yet `combine(a, x) = a` discards `x`, so it is NEVER a `Monoid` for a non-trivial type — a constant semigroup whose only lawful identity is its own constant, the absorbing element wearing a semigroup's shape.

[MIN_MAX_TIE_BREAK_DIVERGES_BETWEEN_THE_TWO_SURFACES]:
- `Semigroup.min(O)` is `make((self, that) => O(self, that) === -1 ? self : that)` — STRICT less-than, so on a tie (`O` returns `0`) it keeps `that`, the RIGHTMOST equal element; `Semigroup.max(O)` keeps `that` on `=== 1` failing, also rightmost-on-tie. The "last minimum/maximum" naming is literal: among order-equal elements, the last wins.
- `effect/Order.min(O)` is `self === that || O(self, that) < 1 ? self : that` — less-than-OR-EQUAL plus a reference fast-path, so on a tie it keeps `self`, the LEFTMOST; `Order.max(O)` uses `> -1` keeping `self` on tie. The value-level and witness-level extremum disagree on which order-equal element survives, a divergence invisible until two elements compare equal-by-order but differ by identity.
- The divergence is harmless for a total order over a primitive (equal-by-order means equal-by-value) and load-bearing for an order BY PROJECTION: `Order.mapInput(Order.number, (r) => r.priority)` ties every record sharing a priority, and `Semigroup.max` over that keeps the last-seen tied record while `Order.max` keeps the first — selecting the extremum of a struct by one field forces a choice of which surface owns the tie-break.
- Both extremum algebras are associative, commutative-up-to-tie (the result value is order-equal regardless of order, but the surviving IDENTITY is order-dependent), and idempotent (`min(a, a) = a`) — the lattice laws hold on the order, the tie-break is an identity-selection policy the law does not constrain, so it is the witness's documented behavior, never derivable.

[BOUNDED_DERIVES_THE_ONLY_LAWFUL_EXTREMUM_IDENTITY]:
- `Monoid.min(B)` is `fromSemigroup(Semigroup.min(B.compare), B.maxBound)` and `Monoid.max(B)` seeds with `B.minBound`: the extremum monoid's identity is the LOSING bound — `maxBound` loses every minimum (`min(maxBound, x) = x`), `minBound` loses every maximum — so the identity law `min(empty, x) = x` holds BY the bound's definition, never by a supplied literal.
- A wrong extremum identity is unrepresentable through `Bounded.min/max` because the bound is read off the witness, not passed: `fromSemigroup(Semigroup.min(O), 0)` over an order where `0` is not the maximum produces a fold that wrongly clamps at `0`, the exact corruption the `Bounded`-typed factory forecloses by demanding the witness that already carries the lawful bound.
- `Bounded.reverse(B)` flips `compare` AND swaps the two bounds atomically, so `Monoid.max(Bounded.reverse(B))` equals `Monoid.min(B)` — the reversed bounded order's maximum-monoid is the original's minimum-monoid, identity included, because the swap moves the losing bound to the new winning position; an ascending and descending extremum fold are one declaration and its involution, both identity-correct by construction.

[ORDERING_IS_THE_TIE_BREAK_MONOID]:
- `data/Ordering.Monoid` is the concrete `Monoid<Ordering>` whose `combine` returns the LEFTMOST non-zero verdict and whose `empty` is `0` — the value owner proving the comparator monoid dispatches over a real domain: `combine(0, -1)` is `-1`, `combine(1, -1)` is `1`, and `0` yields to the first decisive comparison.
- `data/Ordering.Semigroup` is the same leftmost-non-zero algebra WITHOUT the `0` identity: a tie-break fold guaranteed to start from a decided comparison takes the Semigroup and `combineMany`, an empty-admitting fold takes the Monoid and `combineAll` — the identity boundary that separates "at least one key compared" from "zero keys means equal", surfaced as which instance is imported.
- `effect/Ordering` exposes the standalone `combine`/`combineMany`/`combineAll`/`reverse`/`match` directly, so a comparator chain composes without importing the typeclass instance, while the `data/Ordering` `Monoid` is the form that slots into `combineAll`-shaped generic folds — the same left-most-non-zero algebra at two surfaces, value-direct and witness-driven.
- `Order.combineAll([...])` over per-field comparators and `data/Ordering.Monoid.combineAll([...])` over per-field VERDICTS are the same algebra at two altitudes: the former composes comparators then applies once building a reusable `Order<A>` value, the latter applies each comparator then folds the results deciding one specific pair.

[EQUIVALENCE_CONJUNCTION_LAW]:
- `Equivalence.combine` is a conjunction that is associative, commutative, AND idempotent: idempotence makes a field repeated in the fold harmless, and commutativity is precisely why field order is a performance lever rather than the correctness lever it is for `Order.combine` — the lattice properties, not the short-circuit, are what license reordering the equality fold.
- An equivalence built from a transitive predicate is transitive, but the library cannot enforce transitivity: a tolerance equivalence (`|x - y| < ε`) passes `make` and `combine` yet violates transitivity, corrupting `HashSet` bucketing where membership assumes a true equivalence relation — the law the conjunction monoid preserves (it ANDs transitive relations into a transitive one) is exactly the law the leaf predicate must already satisfy.

[LATTICE_MONOID_IDENTITIES_ARE_THE_OPERATION_NEUTRAL]:
- The four Boolean/Predicate lattice monoids each pin the identity that is the operation's algebraic neutral, never an arbitrary blank: `MonoidEvery` (∧) → `true`, `MonoidSome` (∨) → `false`, `MonoidXor` → `false`, `MonoidEqv` (↔) → `true`, each built `fromSemigroup(S, neutral)` so the identity law is structural — an empty ∧-fold is vacuously `true`, an empty ∨-fold vacuously `false`.
- ∧ and ∨ are associative, commutative, AND idempotent (a bounded distributive lattice), so `MonoidEvery.combineAll` over duplicated predicates is duplicate-insensitive — idempotence makes a predicate appearing twice in a guard fold harmless, the law that lets a validator collection carry redundant rows without double-counting.
- XOR is associative and commutative but NOT idempotent (`x ⊻ x = false`), so `MonoidXor.combineAll` over a multiset counts PARITY — a value appearing twice cancels — the one lattice monoid where collection multiplicity changes the result, the trap when a parity fold is reached for where an any-pass (∨) fold was meant.
- `Predicate.getMonoidEvery<A>()` over contramapped leaves is a record validator whose identity (`constTrue`) makes the empty field set admit everything; adding a field is one `Contravariant.contramap` row into the `combineAll` collection, and idempotence guarantees the new row composes commutatively with the rest — growth is one lawful row, never a reopened `&&` chain.

[LIFTING_PRESERVES_LAWS_ONLY_WHEN_BOTH_LAYERS_HOLD]:
- The `SemiApplicative.getSemigroup(F)(S)` lift composes two associativities: its `combine` is associative IFF `F.product` is associative AND `S.combine` is associative, so a non-associative inner `S` makes the lifted carrier semigroup non-associative even when `F` is a lawful applicative — the lift never repairs an unlawful operand, it propagates the weaker layer's defect.
- `Applicative.getMonoid(F)(M)` adds `empty = F.of(M.empty)`: the lifted identity is the inner identity seeded into the carrier, lawful IFF `M.empty` is a two-sided identity AND `F.of(M.empty)` is the applicative unit for `product` — both layers must carry their unit for the lift to carry one, the identity-needs-`Of` boundary reappearing as a law-composition requirement.
- `Option.getOptionalMonoid(S)` is the strength PROMOTION that adds a law the inner type lacks: `make((self, that) => isNone(self) ? that : isNone(that) ? self : some(S.combine(...)))` with `empty = None` — `None` is a genuine two-sided identity by construction, so the optional carrier SUPPLIES the identity law a bare `Semigroup<A>` cannot, the canonical lawful lift from "associative" to "associative with unit".
- `Coproduct.getMonoid(F)()` lifts `coproduct` with `empty = F.zero()`: `Option.coproduct(self, that) = isSome(self) ? self : that` is associative because first-`Some` wins regardless of grouping, idempotent because `coproduct(a, a) = a`, and `None` (`zero`) is its two-sided identity — the three monoid laws hold structurally, which is why the first-success fold over `Option` is a genuine lawful monoid and not a sentinel-seeded reduce wearing one's shape.

[LAW_VIOLATION_BREAKS_THE_DERIVED_FAMILY_SILENTLY]:
- A custom value owner exposing `Semigroup` via `make` enters the entire derived combinator family (`combineMany`, `combineMap`, `getSemigroup`, struct/tuple lifts) on the unverified premise of associativity — there is no signature, brand, or check that records the law, so the only enforcement is a property test asserting `combine(combine(a, b), c) === combine(a, combine(b, c))` before the witness is admitted.
- A value object that takes its parent's algebra via `Semigroup.imap(S, wrap, unwrap)` inherits the parent's laws iff `wrap`/`unwrap` are mutual inverses on the relevant domain — `imap(N.SemigroupSum, brand, unbrand)` is lawful because the round-trip is identity, but an `imap` whose `to`/`from` lose information (a clamping wrap) silently breaks associativity of the retargeted combine, the trap when a lossy projection is mistaken for a lawful isomorphism.
- The lawful path to promote a retargeted Semigroup to a Monoid is `Monoid.fromSemigroup(Semigroup.imap(M, to, from), to(M.empty))` — the identity must be explicitly retargeted through `to`, because the bare `(to, from)` pair cannot guarantee `to(M.empty)` is the new two-sided identity unless `to`/`from` are inverses; `Monoid` exposing no `imap` forces this explicit identity-retarget, the API refusing the unlawful shortcut.

```typescript
import * as OrderingInstances from '@effect/typeclass/data/Ordering'
import { Bounded, Semigroup } from '@effect/typeclass'
import * as Num from '@effect/typeclass/data/Number'
import { Order, Equal, Equivalence, Hash } from 'effect'
import type { Ordering } from 'effect/Ordering'

type Owner = { readonly rank: number; readonly label: string; readonly span: number }
const byField = <K extends keyof Owner>(k: K, O: Order.Order<Owner[K]>): Order.Order<Owner> =>
    Order.mapInput(O, (o) => o[k])

const ranked: Order.Order<Owner> = Order.combineAll([
    byField('rank', Order.number),
    byField('label', Order.string),
    byField('span', Order.reverse(Order.number)),
])
const same: Equivalence.Equivalence<Owner> = Equivalence.combineAll([
    Equivalence.mapInput(Equivalence.number, (o: Owner) => o.rank),
    Equivalence.mapInput(Equal.equivalence<string>(), (o: Owner) => o.label),
])

const NoMerge = Semigroup.struct({ rank: Num.SemigroupMax, label: Semigroup.last<string>() })
const Peak = Bounded.max(Num.Bounded)

const verdict = ranked({ rank: 1, label: '<value-a>', span: 9 }, { rank: 1, label: '<value-a>', span: 4 })
const tied: Ordering = OrderingInstances.Monoid.combineAll([verdict, Order.number(9, 4)])
const folded = NoMerge.combineMany({ rank: 3, label: '<value-a>' }, [{ rank: 5, label: '<value-b>' }])
const key = Hash.structureKeys({ rank: 1, label: '<value-a>', span: 9 }, ['rank', 'label'])
const ceiling: number = Peak.combineAll([2, 9, 4])
```
