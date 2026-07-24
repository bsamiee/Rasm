# [TS_CORE_API_EFFECT_TYPECLASS]

`@effect/typeclass` owns the standalone higher-kinded typeclass hierarchy `effect` core underexposes: the `Semigroup`/`Monoid`/`Bounded` merge algebra and the `Covariant`→`Applicative`→`Monad`→`Foldable`→`Traversable` lattice over `TypeLambda`. `state/merge` declares every CRDT/journal merge as one of these instances and asserts its laws over `tests/contracts`, so a new merge is a row on the algebra, never a hand-written combine.

## [01]-[PACKAGE_SURFACE]

- package: `@effect/typeclass` (MIT)
- module: ESM + CJS (`dist/esm`/`dist/cjs`, `dist/dts` types), `sideEffects: []`; per-typeclass subpaths (`./Semigroup`), per-`data/` instance subpaths (`./data/Number`), and the `.` barrel re-exporting each as a namespace.
- asset: `dist/dts/index.d.ts` — barrel namespacing every typeclass and `data/` instance.
- runtime: pure type-level over the `TypeLambda`/`Kind<F, R, O, E, A>` HKT encoding, isomorphic across node/bun/browser/worker; no addon.
- peer: `effect` — the `Order`/`Equivalence`/`Data`/`Duration` values `min`/`max`/instances parameterize on; no runtime dependency of its own.
- plane: `plane:runtime` (W1), folder-local to `state`; `state/merge` owns the merge law and `@rasm/ts-testkit` (`tests/typescript/_testkit`) the law combinators.
- rail: algebraic-structure / convergence-law.

## [02]-[MERGE_LAW]

Three nested contracts are the merge substrate; each instance in this catalog is one of them.

| [INDEX] | [SYMBOL]                                      | [CONTRACT]                              | [MERGE_ROLE_BOUNDARY]                          |
| :-----: | :-------------------------------------------- | :-------------------------------------- | :--------------------------------------------- |
|  [01]   | `Semigroup<A>`                                | `combine` + `combineMany`               | pairwise merge; ⇔ idempotent + commutative     |
|  [02]   | `Monoid<A>`                                   | `Semigroup<A>` + `empty` + `combineAll` | bounded merge; `empty` + whole-collection fold |
|  [03]   | `Bounded<A>`                                  | `Order<A>` + `minBound` + `maxBound`    | semilattice bounds; derives `Monoid` `empty`   |
|  [04]   | `SemigroupTypeLambda` / `Kind<F, R, O, E, A>` | HKT encoding                            | machinery the law combinators quantify over    |

## [03]-[INSTANCE_VOCABULARY]

Merge algebra is one parameterized pattern of static factories on the `Semigroup`/`Monoid` namespace: policy constructors, structural combinators, and `data/*` named instances — a new merge is a new row, never a new shape.

| [INDEX] | [SURFACE]                                               | [PRODUCES]     | [MERGE_SEMANTICS]                                         |
| :-----: | :------------------------------------------------------ | :------------- | :-------------------------------------------------------- |
|  [01]   | `Semigroup.make(combine, combineMany?)`                 | constructor    | bespoke lawful merge; obligations proven by hand          |
|  [02]   | `Monoid.fromSemigroup(S, empty)`                        | constructor    | lift a `Semigroup` to a `Monoid` with an identity         |
|  [03]   | `Semigroup.min(O)` / `.max(O)`                          | semilattice    | meet/join over an `Order` — the register/lattice CRDT     |
|  [04]   | `Monoid.min(B)` / `.max(B)`                             | semilattice    | bounded meet/join over a `Bounded`; `empty` from a bound  |
|  [05]   | `Semigroup.first()` / `.last()`                         | LWW register   | last-write-wins (commutative under a timestamp order)     |
|  [06]   | `Semigroup.constant(a)`                                 | const register | ignores both inputs, always yields `a`                    |
|  [07]   | `Semigroup.struct(fields)` / `.tuple(...)` / `.array()` | product        | per-field/positional merge — record/tuple CRDT from parts |
|  [08]   | `Semigroup.reverse(S)`                                  | derived        | the dual — combine with arguments flipped                 |
|  [09]   | `Semigroup.intercalate(sep)`                            | derived        | concat with a separator between elements                  |
|  [10]   | `Semigroup.imap(to, from)`                              | derived        | profunctor-mapped merge over a wrapper (encode/decode)    |

`data/*` named instances — each a lawful `Semigroup`/`Monoid` leaf on the shape above:
- [01]-[NUMBER]: `data/Number` `SemigroupSum`/`Multiply`/`Min`/`Max` (+ `MonoidSum`, `Bounded`) — counter (`Sum`, commutative not idempotent) and `Min`/`Max` lattice merges.
- [02]-[BOOLEAN]: `data/Boolean` `SemigroupEvery`/`Some`/`Xor`/`Eqv` (+ `Monoid*`) — AND/OR idempotent flag lattices and XOR/EQV toggle monoids.
- [03]-[RECORD]: `data/Record` `getSemigroupUnion`/`getSemigroupIntersection` (+ `getMonoidUnion`) — grow-only/observed map CRDT; per-value `Semigroup` merges collisions.
- [04]-[OPTION_ARRAY]: `data/Option` `getOptionalMonoid(S)`, `data/Array` `getSemigroup`/`getMonoid` — `None` is `empty`, `Some`s merge via `S`; array concat monoid.
- [05]-[SCALAR]: `data/String`, `data/BigInt`, `data/Duration`, `data/Ordering`, `data/Predicate` — sum/max/min lattices, `Ordering` tie-break monoid, predicate boolean lattices.
- [06]-[EITHER]: `data/Either` `Covariant`/`Monad`/`SemiApplicative`/`Applicative`/`SemiCoproduct`/`SemiAlternative`/`Bicovariant`/`Foldable`/`Traversable` — the `F` the law combinators lift through; `Semigroup<Either<E, Op>>` via `getSemigroup(Either.SemiApplicative)`.
- [07]-[IDENTITY]: `data/Identity` `Monad`/`Applicative`/`Foldable`/`Traversable` (+ `getSemiCoproduct(S)`/`getSemiAlternative(S)`) — the bare-value functor; lift/fold laws proven without a container.
- [08]-[TUPLE]: `data/Tuple` `Bicovariant` — map both pair positions under the law combinators.
- [09]-[EFFECT_MICRO]: `data/Effect`, `data/Micro` `Covariant`/`Monad` + `getSemiProduct`/`getProduct`/`getSemiApplicative`/`getApplicative` — the effectful lift, concurrency as the instance parameter; `Semigroup<Effect<State>>`/`Monoid<Micro<State>>`.

## [04]-[LAW_COMBINATORS]

`@rasm/ts-testkit` (`tests/typescript/_testkit`) lifts a `Semigroup`/`Monoid` through any functor `F` and folds it over any `Foldable`, so one instance proves its laws inside `Option`, `Either`, `Array`, or a decoded op container without a bespoke harness.

| [INDEX] | [SURFACE]                            | [LIFTS_FOLDS]                               | [PROOF_ROLE]                                        |
| :-----: | :----------------------------------- | :------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `SemiApplicative.getSemigroup(F)(S)` | `Semigroup<A>` → `Semigroup<Kind<F, …, A>>` | `Semigroup<Option<Op>>` from a base `Semigroup<Op>` |
|  [02]   | `Applicative.getMonoid(F)(M)`        | `Monoid<A>` → `Monoid<Kind<F, …, A>>`       | a `Monoid<Either<E, State>>` from the base          |
|  [03]   | `Foldable.combineMap(F)(M)`          | fold a container through `M`                | decoded-op container → one merged state             |
|  [04]   | `Foldable.toArray(F)`                | extract to `Array`                          | drain a functor container to an array               |
|  [05]   | `Foldable.reduceComposition`         | nested fold                                 | fold a container-of-containers                      |
|  [06]   | `Foldable.reduceKind`                | effectful fold                              | fold under a `Monad` effect                         |
|  [07]   | `SemiProduct.nonEmptyStruct(F)`      | product by field                            | zip decoded op records field-wise                   |
|  [08]   | `SemiProduct.productComposition`     | nested product                              | product across nested functors `F`                  |
|  [09]   | `Covariant.mapComposition`           | nested map                                  | map through nested functors                         |
|  [10]   | `Covariant.imap`                     | invariant map                               | invariant bidirectional map; functor laws           |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op folds through the associativity `Semigroup.combine` guarantees; commutativity and idempotence are each instance's own obligation `state/merge` asserts. `first`/`last` are commutative only under a total timestamp order, `struct`/`tuple` as lawful as their weakest field; the `Kind`/`TypeLambda` encoding is compile-time, never a runtime service.

[STACKING]:
- `effect`(`.api/effect.md`): `Semigroup.min`/`max` take an `effect/Order<A>` (Order is core `effect`, never a `data/Order`); `Bounded` wraps `Order` + `minBound`/`maxBound` so `Monoid.min`/`max` derive `empty` — the lattice-CRDT bottom/top. A CRDT op is an `effect/Data.TaggedEnum` and `Equal.equals` is the idempotence witness `state/merge` checks (`combine(a, a) ≡ a`); the C#-minted wire op family and app journal families are `Schema.TaggedClass` unions decoded by `wire`/`store`, and `state/merge` binds one `Semigroup` per op `_tag` via `Semigroup.struct` or a `Match` dispatch — a new op is a new `struct` row with its `converge` law.
- `@electric-sql/d2ts`(`.api/electric-sql-d2ts.md`), `@electric-sql/d2mini`(`.api/electric-sql-d2mini.md`): `state/fold` applies the merge as the `reduce` reducer — `reduce((vals: [Op, number][]) => …)` is `Semigroup.combineMany` projected onto signed multiplicities, and `groupByOperators.{sum,min,max}` are `Number.Monoid{Sum,Min,Max}` specialized to the dataflow. One instance declared in `state/merge`, applied at both fold altitudes; the reducer law is the merge law.
- `@rasm/ts-testkit`(`tests/typescript/_testkit`): `state/merge` states the semilattice laws as property tests — `@effect/vitest` `it.prop` (`.api/effect-vitest.md`) over `Schema`-derived `fast-check` arbitraries (`tests/typescript/.api/`) checks `combine` associativity/commutativity/idempotence and `empty` identity per instance, fixtures pinned in `tests/contracts`; `Foldable.combineMap` folds a generated op-sequence to the expected merged state inside the law body.
- within-lib: `state/merge` composes per-op `Semigroup.struct`/`tuple` into a record/tuple CRDT and folds the state vector through `Monoid.combineAll`; `state/fold` threads the same instance through the incremental `reduce`.

[RAIL_LAW]:
- Owns: the lawful merge algebra — the `Semigroup`/`Monoid`/`Bounded` contracts, their constructors and structural combinators, the `data/*` named instances, and the law-combinators lifting and folding an instance through any functor.
- Accept: a CRDT/journal merge as a `Semigroup`/`Monoid` instance; `Semigroup.min`/`max` or `Record.getSemigroupUnion` for the convergence-legal semilattice subset; `Semigroup.struct`/`tuple` to compose a record/tuple CRDT; `Monoid.combineAll` for the state-vector fold; the `data/*` instances as scalar leaves; the law combinators as the shared `@rasm/ts-testkit` harness.
- Reject: an ad hoc merge function where a `Semigroup` instance fits; a non-idempotent/non-commutative instance on a convergence path without a proof obligation (`Sum` is a commutative counter-CRDT monoid, not a semilattice); a phantom `data/Order` import (Order is `effect/Order`); a re-derived per-container merge where `getSemigroup`/`getMonoid` lifts the base instance.
