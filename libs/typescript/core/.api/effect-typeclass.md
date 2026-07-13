# [TS_CORE_API_EFFECT_TYPECLASS]

[PACKAGE_SURFACE]:
- package: `@effect/typeclass` · version `` · license `MIT` · © Effectful Technologies
- module: ESM + CJS (`dist/esm` / `dist/cjs`, `dist/dts` types); `sideEffects: []`. Per-typeclass subpaths (`./Semigroup`, `./Monoid`, `./Bounded`, …) plus per-datatype instance subpaths (`./data/Number`, `./data/Boolean`, `./data/Record`, …) and the flat `.` barrel that re-exports each as a namespace.
- asset: `dist/dts/index.d.ts` (barrel: 24 typeclass namespaces + 15 `data/` instance namespaces).
- runtime: pure type-level + tiny runtime; the surface is the higher-kinded `TypeLambda`/`Kind<F, R, O, E, A>` encoding — no addon, isomorphic (node/bun/browser/worker).
- peer: `effect` (the `Order`/`Equivalence`/`Data`/`Duration` values `min`/`max`/instances are parameterized by); no runtime dependencies of its own.
- plane: `plane:runtime` (W1); folder-local to `state`, catalogued here — the merge-law owner `state/merge` and the `@rasm/ts-testkit` law combinators (`tests/typescript/_testkit`) share.
- rail: algebraic-structure / convergence-law.

`@effect/typeclass` is the standalone, complete higher-kinded typeclass hierarchy `effect` core does not fully expose: the full `Semigroup`/`Monoid`/`Bounded` algebra plus the `Covariant`→`Applicative`→`Monad`→`Foldable`→`Traversable` lattice over `TypeLambda`. In `state` it is the ONE thing that makes `state/merge` lawful: a CRDT merge is a `Semigroup<Op>` (an associative `combine`), a bounded merge is a `Monoid<Op>` (associative + identity), and the convergence-legal subset is the idempotent-commutative semilattice — `Semigroup.min(Order)` / `Semigroup.max(Order)` for join/meet, `Record.getSemigroupUnion` for grow-only maps. `state/merge` declares merges as these instances; `state/merge` asserts associativity/commutativity/idempotence as the typeclass laws over the `tests/contracts` corpus; `state/fold` applies the same instance through the d2ts `reduce` — the merge law and the reducer law are one law. The instance is composed structurally (`struct`/`tuple`/`array`), lifted through any functor (`SemiApplicative.getSemigroup`), and folded over any container (`Foldable.combineMap`), so growth is a row on the algebra, never a new hand-written merge.

## [01]-[MERGE_LAW]

Three nested contracts are the whole merge substrate — `Semigroup` (associative combine), `Monoid` (adds identity + whole-collection fold), `Bounded` (adds an `Order` with bounds so a `Monoid` empty derives from `minBound`/`maxBound`). Every instance in [02] is one of these.

| [INDEX] | [SYMBOL]                                      | [CONTRACT]                              | [MERGE_ROLE_BOUNDARY]                          |
| :-----: | :-------------------------------------------- | :-------------------------------------- | :--------------------------------------------- |
|  [01]   | `Semigroup<A>`                                | `combine` + `combineMany`               | pairwise merge; ⇔ idempotent + commutative     |
|  [02]   | `Monoid<A>`                                   | `Semigroup<A>` + `empty` + `combineAll` | bounded merge; `empty` + whole-collection fold |
|  [03]   | `Bounded<A>`                                  | `Order<A>` + `minBound` + `maxBound`    | semilattice bounds; derives `Monoid` `empty`   |
|  [04]   | `SemigroupTypeLambda` / `Kind<F, R, O, E, A>` | HKT encoding                            | machinery the law combinators quantify over    |

```ts signature
// The one merge shape. combineMany is the incremental fold — the d2ts reduce reducer is its elementwise projection.
interface Semigroup<A> {
  readonly combine: (self: A, that: A) => A
  readonly combineMany: (self: A, collection: Iterable<A>) => A
}
interface Monoid<A> extends Semigroup<A> {
  readonly empty: A
  readonly combineAll: (collection: Iterable<A>) => A   // fold from empty — the state-vector merge
}
interface Bounded<A> { readonly compare: Order<A>; readonly minBound: A; readonly maxBound: A }
// The convergence-legal semilattice is Order-parameterized — join = max, meet = min. Order comes from effect/Order (NOT a data/Order module).
declare const Semigroup.min: <A>(O: Order<A>) => Semigroup<A>   // idempotent + commutative ⇒ a lawful meet-semilattice
declare const Semigroup.max: <A>(O: Order<A>) => Semigroup<A>   // ⇒ a lawful join-semilattice
declare const Monoid.min: <A>(B: Bounded<A>) => Monoid<A>       // empty = maxBound
declare const Monoid.max: <A>(B: Bounded<A>) => Monoid<A>       // empty = minBound
```

## [02]-[INSTANCE_VOCABULARY]

The merge algebra is ONE parameterized pattern: constructors that build a `Semigroup`/`Monoid` from a policy, structural combinators that compose per-field instances, and the `data/*` named instances. The roster below is SEED DATA on the `Semigroup<A>`/`Monoid<A>` shape — a new merge is a new instance row (a new `Order`, a new `struct` field, a new `data/*` constant), never a new merge shape.

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

The `data/*` named instances — each a lawful `Semigroup`/`Monoid` leaf on the shape above; a new merge is a new entry, never a new shape:
- [01]-[NUMBER]: `data/Number` `SemigroupSum`/`SemigroupMultiply`/`SemigroupMin`/`SemigroupMax` (+ `Monoid*`, `Bounded`) — counter (`Sum`) and lattice (`Min`/`Max`) merges; `Sum` is a commutative monoid, not idempotent.
- [02]-[BOOLEAN]: `data/Boolean` `SemigroupEvery`/`SemigroupSome`/`SemigroupXor`/`SemigroupEqv` (+ `Monoid*`) — AND/OR idempotent flag lattices and XOR/EQV toggle monoids.
- [03]-[RECORD]: `data/Record` `getSemigroupUnion`/`getSemigroupIntersection` (+ `getMonoidUnion`) — grow-only / observed map CRDT; per-value `Semigroup` merges collisions.
- [04]-[OPTION_ARRAY]: `data/Option` `getOptionalMonoid(S)` · `data/Array` `getSemigroup`/`getMonoid` — `None` is `empty`, `Some`s merge via `S`; array concat monoid.
- [05]-[SCALAR]: `data/String` · `data/BigInt` · `data/Duration` · `data/Ordering` · `data/Predicate` — string/bigint/duration sum-max-min lattices, `Ordering` tie-break monoid, predicate boolean lattices.
- [06]-[EITHER]: `data/Either` `Covariant`/`Monad`/`SemiApplicative`/`Applicative`/`SemiCoproduct`/`SemiAlternative`/`Bicovariant`/`Foldable`/`Traversable` — the `Either` `F` the [03] combinators lift through; `Semigroup<Either<E, Op>>` via `getSemigroup(Either.SemiApplicative)`.
- [07]-[IDENTITY]: `data/Identity` `Monad`/`Applicative`/`Foldable`/`Traversable` (+ `getSemiCoproduct(S)`/`getSemiAlternative(S)`) — the bare-value functor; lift/fold laws proven without a container.
- [08]-[TUPLE]: `data/Tuple` `Bicovariant` — map both pair positions; the tuple projection under the [03] combinators.
- [09]-[EFFECT_MICRO]: `data/Effect` · `data/Micro` `Covariant`/`Monad` + `getSemiProduct`/`getProduct`/`getSemiApplicative`/`getApplicative` (concurrency-parameterized) — the effectful lift; `Semigroup<Effect<State>>`/`Monoid<Micro<State>>` with concurrency as the instance parameter.

```ts signature
// A record CRDT is composed, never written: each field is a lawful component instance, struct lifts them to the record.
const PresenceMerge = Semigroup.struct({
  cursor:   Semigroup.last<Cursor>(),                     // LWW register (timestamp-ordered upstream)
  reactions: Record.getSemigroupUnion(Boolean.SemigroupSome),  // grow-only reaction set
  score:    Number.SemigroupMax,                          // max-wins lattice
})   // ⇒ Semigroup<{ cursor: Cursor; reactions: Record<string, boolean>; score: number }>
// combineAll (a Monoid instance member) folds a whole state vector; combineMany folds a delta batch onto a base.
const StateMonoid = Monoid.struct({ /* … */ })            // Monoid<State>
const merged = StateMonoid.combineAll(replicaStates)      // fold from empty across replicas
```

## [03]-[LAW_COMBINATORS]

The higher hierarchy is the `@rasm/ts-testkit` law-combinator surface (`tests/typescript/_testkit`): it LIFTS a `Semigroup`/`Monoid` through any functor `F` and FOLDS it over any `Foldable`, so the same merge instance proves its laws inside `Option`, `Either`, `Array`, or a decoded op container without a bespoke harness. This is the "shared with the testkit law combinators" the merge design names.

| [INDEX] | [SURFACE]                            | [LIFTS_FOLDS]                               | [PROOF_ROLE]                                        |
| :-----: | :----------------------------------- | :------------------------------------------ | :-------------------------------------------------- |
|  [01]   | `SemiApplicative.getSemigroup(F)(S)` | `Semigroup<A>` → `Semigroup<Kind<F, …, A>>` | `Semigroup<Option<Op>>` from a base `Semigroup<Op>` |
|  [02]   | `Applicative.getMonoid(F)(M)`        | `Monoid<A>` → `Monoid<Kind<F, …, A>>`       | a `Monoid<Either<E, State>>` from the base          |
|  [03]   | `Foldable.combineMap(F)(M)`          | fold a container through `M`                | decoded-op container → one merged state             |
|  [04]   | `Foldable.toArray(F)`                | extract to `Array`                          | drain a functor container to an array               |
|  [05]   | `Foldable.reduceComposition`         | nested fold                                 | fold a container-of-containers                      |
|  [06]   | `reduceKind`                         | effectful fold                              | fold under a `Monad` effect                         |
|  [07]   | `SemiProduct.nonEmptyStruct(F)`      | product by field                            | zip decoded op records field-wise                   |
|  [08]   | `productComposition`                 | nested product                              | product across nested functors `F`                  |
|  [09]   | `Covariant.mapComposition`           | nested map                                  | map through nested functors                         |
|  [10]   | `Covariant.imap`                     | invariant map                               | invariant bidirectional map; functor laws           |

```ts signature
// Lift once, prove once: a merge instance and its laws transport through any functor F — no per-container merge, no per-container proof.
declare const SemiApplicative.getSemigroup: <F>(F: SemiApplicative<F>) => <A>(S: Semigroup<A>) => Semigroup<Kind<F, any, any, any, A>>
declare const Applicative.getMonoid: <F>(F: Applicative<F>) => <A>(M: Monoid<A>) => Monoid<Kind<F, any, any, any, A>>
declare const Foldable.combineMap: <F>(F: Foldable<F>) => <M>(M: Monoid<M>) => <A>(f: (a: A) => M) => (self: Kind<F, any, any, any, A>) => M
```

## [04]-[INTEGRATION]

[STACK: `Semigroup` + `effect/Order` + `effect/Data`/`Equal` (`.api/effect.md`)] — the semilattice is Order-parameterized: `Semigroup.min`/`max` take an `effect/Order<A>` (there is NO `@effect/typeclass/data/Order` — Order is core `effect`). A CRDT op is an `effect/Data.TaggedEnum`, and `Equal.equals` gives the idempotence witness `state/merge` checks (`combine(a, a) ≡ a`). `Bounded` wraps `Order` + `minBound`/`maxBound` so `Monoid.min`/`max` derive `empty` — the lattice-CRDT bottom/top.

[STACK: merge instance + d2ts/d2mini `reduce` (`.api/electric-sql-d2ts.md`, `.api/electric-sql-d2mini.md`)] — `state/fold` applies the merge as the `reduce` reducer: the d2ts/d2mini `reduce((vals: [Op, number][]) => …)` is `Semigroup.combineMany` projected onto signed multiplicities, and `groupByOperators.{sum,min,max}` are `Number.Monoid{Sum,Min,Max}` specialized to the dataflow. One instance declared in `state/merge`, applied incrementally at both fold altitudes — the reducer law IS the merge law.

[STACK: convergence laws + `@rasm/ts-testkit` (`.api/effect-vitest.md`, `fast-check` catalogued at `tests/typescript/.api/`)] — `state/merge` states the semilattice laws as property tests: `@effect/vitest` `it.prop` over `Schema`-derived `fast-check` arbitraries checks `combine` associativity/commutativity/idempotence and `empty` identity for each declared instance, with fixtures pinned in the `tests/contracts` corpus. The `Foldable.combineMap` combinator is what folds a generated op-sequence to the expected merged state inside the law body — the merge instance and its proof share this package.

[STACK: `Schema` op vocabulary (`.api/effect.md`)] — the merge is generic over the op vocabulary: the C#-minted wire op family and app-authored journal families are `Schema.TaggedClass` unions decoded by `wire`/`store`, and `state/merge` binds one `Semigroup` per op `_tag` via `Semigroup.struct`/a `Match` dispatch. A new op is a new `struct` row plus its `converge` law — never a fork of the merge algebra.

## [05]-[RAIL_LAW]

- Owns: the lawful merge algebra — `Semigroup`/`Monoid`/`Bounded` contracts, the `make`/`min`/`max`/`first`/`last`/`constant`/`struct`/`tuple`/`array`/`reverse`/`imap` constructors, the `data/*` named instances (Number/Boolean/String/BigInt/Duration/Ordering/Record/Array/Option/Predicate/Either/Identity/Tuple/Effect/Micro), and the higher law-combinators (`getSemigroup`/`getMonoid`/`combineMap`/`nonEmptyStruct`/`mapComposition`) that lift and fold an instance through any functor.
- Accept: a CRDT/journal merge declared as a `Semigroup`/`Monoid` instance; `Semigroup.min`/`max` (Order-parameterized) or `Record.getSemigroupUnion` for the convergence-legal semilattice subset; `Semigroup.struct`/`tuple` to compose a record/tuple CRDT from component instances; a `Monoid` instance's `combineAll` for the state-vector fold; the `data/*` instances as the scalar leaves; the law combinators as the shared `@rasm/ts-testkit` harness.
- Reject: an ad hoc merge function where a `Semigroup` instance fits; a non-idempotent/non-commutative instance on a convergence path without a proof obligation (`Sum` is a commutative monoid, not a semilattice — its convergence argument is counter-CRDT, not last-writer); a phantom `data/Order` import (Order is `effect/Order`); re-deriving a per-container merge where `getSemigroup`/`getMonoid` lifts the base instance.
- Boundary: `Semigroup.combine` guarantees associativity only — commutativity and idempotence are the instance's own law obligations `state/merge` must assert. `first`/`last` are commutative only under a total timestamp order; `struct`/`tuple` are as lawful as their weakest field. The HKT `Kind`/`TypeLambda` encoding is compile-time; nothing here is a runtime service.
