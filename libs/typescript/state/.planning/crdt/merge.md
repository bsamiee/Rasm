# [STATE_MERGE]

`crdt/merge.ts` is the one lawful merge owner of the branch: every convergent combination — CRDT register, counter, flag, keyed map, record product — is a `Merge.Instance<A>` value composing `@effect/typeclass` `Semigroup` atoms with a declared law posture, a shared `Equivalence`, and an optional identity, so the merge law, the d2ts/d2mini `reduce` reducer law, and the `crdt/converge` proof obligations are one declaration read three ways. The algebra is generic over the op vocabulary: the C#-minted wire op family (decoded by `wire/codec/crdt`) and app-authored journal event families are instance rows on this surface, never sibling merge functions. LWW is not a constructor — it is `Merge.max` applied to a total stamp order (`Hlc.Order` composed by the caller), so one extremum owner serves lattice and last-write semantics without a name twin.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                       | [SURFACE]                                                |
| :-----: | :------------------ | :---------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | [INSTANCE_CONTRACT] | the `Merge.Instance<A>` shape, posture vocabulary, order-derived equivalence  | `Merge.Posture`, `Merge.Instance`, `Merge.instance`       |
|  [02]   | [INSTANCE_ROSTER]   | scalar, keyed, optional, and product instance constructors                    | `Merge.max/min/first/counter/flag/union/optional/struct`  |
|  [03]   | [FOLD_ENTRY]        | the absence-honest fold and the convergence-legality read                     | `Merge.fold`, `Merge.convergent`                          |

## [2]-[INSTANCE_CONTRACT]

- Owner: `Merge.Instance<A>` — `combine` (the `Semigroup`; associativity is its contract), `posture` (the declared commutativity/idempotence obligations `crdt/converge` asserts), `alike` (the equivalence every law check and table comparison shares), `empty` (the lawful identity as `Option`, never a forged sentinel).
- Packages: `@effect/typeclass` (`Semigroup`, `data/*` atoms); `effect` (`Array`, `Equivalence`, `Option`, `Order`, `Record`).
- Law: associativity rides the `Semigroup` contract itself; commutativity and idempotence are claims the posture declares and `crdt/converge` proves — an instance claiming a posture it cannot witness fails at the law gate, never diverges silently at a replica.
- Law: `alike` derives from the instance's own material — `_fromOrder` for extremum instances, `Schema.equivalence` for decoded owners, composed `Equivalence` rows for products — so law checks and replay-table comparison never fall back to reference identity.
- Growth: a new merge semantic is a new constructor row or a `data/*` atom lift; the `Instance` shape never widens per semantic.

```typescript
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as BooleanInstances from "@effect/typeclass/data/Boolean"
import * as NumberInstances from "@effect/typeclass/data/Number"
import * as OptionInstances from "@effect/typeclass/data/Option"
import * as RecordInstances from "@effect/typeclass/data/Record"
import { Array, Equivalence, Option, type Order, Record, type Types } from "effect"

declare namespace Merge {
  type Posture = { readonly commutative: boolean; readonly idempotent: boolean }
  type Instance<A> = {
    readonly combine: Semigroup.Semigroup<A>
    readonly posture: Posture
    readonly alike: Equivalence.Equivalence<A>
    readonly empty: Option.Option<A>
  }
  type Fields<S> = { readonly [K in keyof S]: Instance<S[K]> }
  type Shape = {
    readonly instance: <A>(spec: Instance<A>) => Instance<A>
    readonly max: <A>(order: Order.Order<A>) => Instance<A>
    readonly min: <A>(order: Order.Order<A>) => Instance<A>
    readonly first: <A>(alike: Equivalence.Equivalence<A>) => Instance<A>
    readonly counter: Instance<number>
    readonly flag: Instance<boolean>
    readonly union: <V>(row: Instance<V>) => Instance<Record.ReadonlyRecord<string, V>>
    readonly optional: <A>(row: Instance<A>) => Instance<Option.Option<A>>
    readonly struct: <S extends object>(fields: Fields<S>) => Instance<Types.Simplify<S>>
    readonly fold: <A>(instance: Instance<A>, rows: ReadonlyArray<A>) => Option.Option<A>
    readonly convergent: <A>(instance: Instance<A>) => boolean
  }
}

const _LATTICE: Merge.Posture = { commutative: true, idempotent: true }
const _COMMUTES: Merge.Posture = { commutative: true, idempotent: false }
const _ORDERED: Merge.Posture = { commutative: false, idempotent: true }

const _fromOrder = <A>(order: Order.Order<A>): Equivalence.Equivalence<A> =>
  Equivalence.make((self, that) => order(self, that) === 0)
```

## [3]-[INSTANCE_ROSTER]

- Owner: the constructor family — every row is a `data/*` atom or a one-line `Semigroup` derivation carrying its true posture; the roster is seed data on the `Instance` shape, and a bespoke lawful merge enters through `Merge.instance` with its obligations proven at `crdt/converge`.
- Law: `Merge.max`/`Merge.min` over an `Order` are the convergence-legal semilattices — join and meet; LWW is `Merge.max(stampOrder)` where the caller supplies a total stamp order (`Hlc.Order`, or `Order.combine` of stamp and replica tiebreak), never a second constructor.
- Law: `Merge.counter` (`SemigroupSum`) is commutative, not idempotent — its convergence argument is op-multiset uniqueness at the fold, the posture row that makes `crdt/converge` demand a dedup witness instead of an idempotence proof.
- Law: `Merge.union` merges keyed partial records grow-only — keys present in one side keep, collisions combine through the row instance — and `Merge.optional` lifts an identity-free row to lawful with `Option.none()` as the empty, so absent fields pad folds without forged sentinels.
- Law: `Merge.struct` is the record CRDT — one instance per field, posture the conjunction of field postures, equivalence and empty composed from the same rows — so a record merge is exactly as lawful as its weakest field and the whole matrix reads from one declaration.
- Exemption: `_mapped` and `_struct` are the reverse-mapped projection kernel — the checker cannot correlate per-key `Instance<S[K]>` rows through `Record.map`, so the scoped assertions live in these two interior functions and nowhere else in the folder.
- Growth: a new CRDT type is one constructor row here plus its law row in `crdt/converge.md`; the wire op family binds instances per op case at the `wire/codec/crdt` decode seam, never by forking this algebra.

```typescript
const _mapped = <S extends object, R>(
  fields: Merge.Fields<S>,
  project: (row: Merge.Instance<never>) => R,
): { readonly [K in keyof S]: R } =>
  Record.map(
    fields as Record.ReadonlyRecord<string, Merge.Instance<never>>,
    project,
  ) as { readonly [K in keyof S]: R }

const _postures = (rows: ReadonlyArray<Merge.Posture>): Merge.Posture => ({
  commutative: Array.every(rows, (row) => row.commutative),
  idempotent: Array.every(rows, (row) => row.idempotent),
})

const _max = <A>(order: Order.Order<A>): Merge.Instance<A> => ({
  combine: Semigroup.max(order),
  posture: _LATTICE,
  alike: _fromOrder(order),
  empty: Option.none(),
})

const _min = <A>(order: Order.Order<A>): Merge.Instance<A> => ({
  combine: Semigroup.min(order),
  posture: _LATTICE,
  alike: _fromOrder(order),
  empty: Option.none(),
})

const _struct = <S extends object>(fields: Merge.Fields<S>): Merge.Instance<Types.Simplify<S>> => ({
  combine: Semigroup.struct(_mapped(fields, (row) => row.combine)) as Semigroup.Semigroup<Types.Simplify<S>>,
  posture: _postures(Record.values(_mapped(fields, (row) => row.posture))),
  alike: Equivalence.struct(_mapped(fields, (row) => row.alike)) as Equivalence.Equivalence<Types.Simplify<S>>,
  empty: Option.all(_mapped(fields, (row) => row.empty)) as Option.Option<Types.Simplify<S>>,
})
```

## [4]-[FOLD_ENTRY]

- Owner: `Merge.fold` — the absence-honest fold: a non-empty collection folds through `combineMany` on its head, an empty collection falls to the instance's lawful `empty`, and the return is `Option` because an identity-free instance has no lawful answer for zero rows. A caller holding a witnessed `NonEmptyReadonlyArray` composes `instance.combine.combineMany(Array.headNonEmpty(rows), Array.tailNonEmpty(rows))` directly — the package surface is the proven-arity spelling, never a second entrypoint.
- Law: the reducer the d2ts/d2mini `reduce` operator applies in `fold/replay` is the elementwise projection of this same `combineMany` — the merge law and the incremental reducer law are one law at two speeds, declared once here.
- Law: `Merge.convergent` is the gate read — `commutative && idempotent` — the `fold/replay` retraction guard and the `crdt/converge` obligation selector consume the same predicate, never a re-derived posture check.
- Boundary: law assertion runs in the tests estate — `crdt/converge.md` owns the obligation rows and witnesses; `tests/typescript/_testkit` lifts them over Schema-derived arbitraries against the `tests/contracts` corpus.

```typescript
const _fold = <A>(instance: Merge.Instance<A>, rows: ReadonlyArray<A>): Option.Option<A> =>
  Array.isNonEmptyReadonlyArray(rows)
    ? Option.some(instance.combine.combineMany(Array.headNonEmpty(rows), Array.tailNonEmpty(rows)))
    : instance.empty

const Merge: Merge.Shape = {
  instance: (spec) => spec,
  max: _max,
  min: _min,
  first: (alike) => ({ combine: Semigroup.first(), posture: _ORDERED, alike, empty: Option.none() }),
  counter: {
    combine: NumberInstances.SemigroupSum,
    posture: _COMMUTES,
    alike: Equivalence.number,
    empty: Option.some(0),
  },
  flag: {
    combine: BooleanInstances.SemigroupSome,
    posture: _LATTICE,
    alike: Equivalence.boolean,
    empty: Option.some(false),
  },
  union: (row) => ({
    combine: RecordInstances.getSemigroupUnion(row.combine),
    posture: row.posture,
    alike: Record.getEquivalence(row.alike),
    empty: Option.some(Record.empty()),
  }),
  optional: (row) => ({
    combine: OptionInstances.getOptionalMonoid(row.combine),
    posture: row.posture,
    alike: Option.getEquivalence(row.alike),
    empty: Option.some(Option.none()),
  }),
  struct: _struct,
  fold: _fold,
  convergent: (instance) => instance.posture.commutative && instance.posture.idempotent,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Merge }
```
