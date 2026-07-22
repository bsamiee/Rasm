# [CORE_MERGE]

The one lawful merge owner of the branch and its law surface in one module: every convergent combination — CRDT register, counter, flag, grow-only set, keyed map, record product, bounded lattice, wrapper re-landing — is a `Merge.Instance<A>` value composing `@effect/typeclass` `Semigroup` atoms with a declared law posture, a shared `Equivalence`, and an optional identity, and every instance's proof obligations — associativity always, commutativity and idempotence exactly when the posture claims them, identity exactly when an `empty` exists — live beside it as `Converge` witness values. An instance with a lawful `empty` projects the lawful `Monoid` whose `combineAll` is the state-vector fold, a `Bounded` scale derives its lattice pair with `empty` from the bounds, and a keyed live table whose multi-key batches commit all-or-nothing is one STM cell family on the same instance vocabulary. The merge law, the incremental reducer law the fold engines apply, and the convergence proofs are one declaration read three ways; ordered-sequence convergence is `fold`'s fractional-index lane by construction, so no sequence instance exists here. The module is `core/src/state/merge.ts`; a new merge semantic is a constructor row, a new law is a witness row, and a bespoke lawful merge enters through `Merge.instance` with its obligations proven at the law surface.

## [01]-[CLUSTERS]

The `[PUBLIC]` column drops the shared `Merge.` prefix; `Converge` and `Breach` spell whole.

| [INDEX] | [CLUSTER]           | [OWNS]                          | [PUBLIC]                                                                        |
| :-----: | :------------------ | :------------------------------ | :------------------------------------------------------------------------------ |
|  [01]   | `INSTANCE_CONTRACT` | combine, posture, alike, empty  | `Posture`, `Instance`, `instance`                                               |
|  [02]   | `INSTANCE_ROSTER`   | scalar, keyed, set, product     | `max/min/lattice/first/counter/flag/union/hashSet/hashMap/imap/optional/struct` |
|  [03]   | `FOLD_ENTRY`        | fold, `Monoid` projection, gate | `fold`, `monoid`, `convergent`                                                  |
|  [04]   | `LAW_SURFACE`       | obligations, witnesses, replay  | `Converge`, `Breach`                                                            |
|  [05]   | `MERGE_CELLS`       | keyed transactional cell table  | `cell`                                                                          |

## [02]-[INSTANCE_CONTRACT]

[INSTANCE_CONTRACT]:
- Owner: `Merge.Instance<A>` — `combine` (the `Semigroup`; associativity is its contract), `posture` (the declared commutativity/idempotence obligations the law surface asserts), `alike` (the equivalence every law check and table comparison shares), `empty` (the lawful identity as `Option`, never a forged sentinel).
- Law: associativity rides the `Semigroup` contract itself; commutativity and idempotence are claims the posture declares and `Converge` proves — an instance claiming a posture it cannot witness fails at the law gate, never diverges silently at a replica.
- Law: `alike` derives from the instance's own material — `_fromOrder` for extremum instances, `Schema.equivalence` for decoded owners, composed `Equivalence` rows for products — so law checks and table comparison never fall back to reference identity.
- Law: the algebra is generic over the op vocabulary — the C#-minted wire op family the interchange codec decodes and app-authored journal families are instance rows on this surface, never sibling merge functions; LWW is `Merge.max` applied to a total stamp order (`Hlc.Order` composed by the caller), never a second constructor.
- Growth: a new merge semantic is a new constructor row or a `data/*` atom lift; the `Instance` shape never widens per semantic.
- Packages: `@effect/typeclass` (`Semigroup`, `Monoid`, `Bounded`, `data/*` atoms); `effect` (`Array`, `Data`, `Effect`, `Either`, `Equal`, `Equivalence`, `HashMap`, `HashSet`, `Option`, `Order`, `Predicate`, `Record`, `STM`, `TMap`).

```typescript
import type * as Bounded from "@effect/typeclass/Bounded"
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as BooleanInstances from "@effect/typeclass/data/Boolean"
import * as NumberInstances from "@effect/typeclass/data/Number"
import * as OptionInstances from "@effect/typeclass/data/Option"
import * as RecordInstances from "@effect/typeclass/data/Record"
import { Array, Data, Effect, Either, Equal, Equivalence, HashMap, HashSet, Option, type Order, Predicate, Record, STM, TMap, TRef, type Types } from "effect"

declare namespace Merge {
  type Posture = { readonly commutative: boolean; readonly idempotent: boolean }
  type Instance<A> = {
    readonly combine: Semigroup.Semigroup<A>
    readonly posture: Posture
    readonly alike: Equivalence.Equivalence<A>
    readonly empty: Option.Option<A>
  }
  type Fields<S> = { readonly [K in keyof S]: Instance<S[K]> }
  type Lattice<A> = { readonly join: Instance<A>; readonly meet: Instance<A> }
  type Cell<K, S> = {
    readonly absorb: (rows: ReadonlyArray<readonly [K, S]>) => Effect.Effect<void>
    readonly read: (key: K) => Effect.Effect<Option.Option<S>>
    readonly table: Effect.Effect<HashMap.HashMap<K, S>>
    readonly settled: (
      probe: readonly [key: K, holds: (state: S) => boolean] | ((table: HashMap.HashMap<K, S>) => boolean),
    ) => Effect.Effect<void>
  }
  type Single<S> = {
    readonly absorb: (state: S) => Effect.Effect<void>
    readonly read: Effect.Effect<S>
    readonly settled: (holds: (state: S) => boolean) => Effect.Effect<void>
  }
  type Shape = {
    readonly instance: <A>(spec: Instance<A>) => Instance<A>
    readonly max: <A>(order: Order.Order<A>) => Instance<A>
    readonly min: <A>(order: Order.Order<A>) => Instance<A>
    readonly lattice: <A>(bounds: Bounded.Bounded<A>) => Lattice<A>
    readonly first: <A>(alike: Equivalence.Equivalence<A>) => Instance<A>
    readonly counter: Instance<number>
    readonly flag: Instance<boolean>
    readonly union: <V>(row: Instance<V>) => Instance<Record.ReadonlyRecord<string, V>>
    readonly hashSet: <A>() => Instance<HashSet.HashSet<A>>
    readonly hashMap: <K, V>(row: Instance<V>) => Instance<HashMap.HashMap<K, V>>
    readonly optional: <A>(row: Instance<A>) => Instance<Option.Option<A>>
    readonly struct: <S extends object>(fields: Fields<S>) => Instance<Types.Simplify<S>>
    readonly imap: <A, B>(row: Instance<A>, to: (value: A) => B, from: (wrapped: B) => A) => Instance<B>
    readonly fold: <A>(instance: Instance<A>, rows: ReadonlyArray<A>) => Option.Option<A>
    readonly monoid: <A>(instance: Instance<A>) => Option.Option<Monoid.Monoid<A>>
    readonly convergent: <A>(instance: Instance<A>) => boolean
    readonly cell: {
      <K, S>(instance: Instance<S>): Effect.Effect<Cell<K, S>>
      <S>(instance: Instance<S>, seed: { readonly initial: S }): Effect.Effect<Single<S>>
    }
  }
}

const _LATTICE: Merge.Posture = { commutative: true, idempotent: true }
const _COMMUTES: Merge.Posture = { commutative: true, idempotent: false }
const _ORDERED: Merge.Posture = { commutative: false, idempotent: true }

const _fromOrder = <A>(order: Order.Order<A>): Equivalence.Equivalence<A> =>
  Equivalence.make((self, that) => order(self, that) === 0)
```

## [03]-[INSTANCE_ROSTER]

[INSTANCE_ROSTER]:
- Owner: the constructor family — every row is a `data/*` atom or a one-line `Semigroup` derivation carrying its true posture; the roster is seed data on the `Instance` shape.
- Law: `Merge.max`/`Merge.min` over an `Order` are the convergence-legal semilattices — join and meet — identity-free because an unbounded scale has no lawful `empty`; `Merge.lattice(bounds)` is the bounded pair whose `join` carries `Some(minBound)` and whose `meet` carries `Some(maxBound)`, so a scale with real bounds folds zero rows to its own floor or ceiling instead of forcing every consumer through the absence fold.
- Law: `Merge.counter` (`SemigroupSum`) is commutative, not idempotent — its convergence argument is op-multiset uniqueness at the fold, the posture row that makes the law surface demand a dedup witness instead of an idempotence proof.
- Law: `Merge.union` merges keyed partial records grow-only — keys present in one side keep, collisions combine through the row instance — and `Merge.optional` lifts an identity-free row to lawful with `Option.none()` as the empty, so absent fields pad folds without forged sentinels.
- Law: `Merge.hashSet` is the grow-only set CRDT on the branch's keyed-set currency — `HashSet.union` joins, the posture is the full lattice, `empty` is the empty set, `alike` is the set's own structural equality — the row causal reach tables and selection axes compose; a 2P set is `Merge.struct` over an add set and a remove set with the read-time difference as its projection, never a third constructor.
- Law: `Merge.imap` carries an instance across a wrapper pair — `Semigroup.imap` maps the combine through `to`/`from`, `alike` re-anchors through `Equivalence.mapInput`, `empty` maps through `to` — so a class owner re-lands its interior field-product instance through one iso, and the hand `Semigroup.make` constructor wrap beside a roster instance is the deleted spelling.
- Law: `Merge.hashMap` is the keyed-map CRDT on the branch's own keyed-state currency — the `HashMap` twin of `union`: present-in-one keeps, present-in-both combines through the row instance, `empty` is the empty map, posture inherits the row's — so a keyed evidence field composes `Merge.struct({ commands: Merge.hashMap(row) })` and a hand-rolled `HashMap.reduce` combine beside the roster is the deleted spelling; its `alike` is the law surface's own keyed-table comparison, so table proofs and instance proofs share one equality.
- Law: `Merge.struct` is the record CRDT — one instance per field, posture the conjunction of field postures, equivalence and empty composed from the same rows — a record merge is exactly as lawful as its weakest field and the whole matrix reads from one declaration.
- Exemption: `_mapped` and `_struct` are the marked reverse-mapped projection kernel — `Record.map` deliberately homogenizes record values and the checker cannot retain each key's `Instance<S[K]>` correlation through that API, so one input cast and the three exact output rebindings live here, carry the `// BOUNDARY ADAPTER` mark, and no asserted value crosses the kernel.
- Growth: a new CRDT type is one constructor row here plus its law row at the law surface; the wire op family binds instances per op case at the interchange decode seam, never by forking this algebra.

```typescript
const _mapped = <S extends object, R>(
  fields: Merge.Fields<S>,
  project: (row: Merge.Instance<unknown>) => R,
): { readonly [K in keyof S]: R } => {
  // BOUNDARY ADAPTER: Record.map homogenizes field values; the mapped-key contract restores the exact key census before the value leaves
  return Record.map(
    fields as unknown as Record.ReadonlyRecord<string, Merge.Instance<unknown>>,
    project,
  ) as unknown as { readonly [K in keyof S]: R }
}

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

const _lattice = <A>(bounds: Bounded.Bounded<A>): Merge.Lattice<A> => ({
  join: { ..._max(bounds.compare), empty: Option.some(bounds.minBound) },
  meet: { ..._min(bounds.compare), empty: Option.some(bounds.maxBound) },
})

const _struct = <S extends object>(fields: Merge.Fields<S>): Merge.Instance<Types.Simplify<S>> => ({
  combine: Semigroup.struct(_mapped(fields, (row) => row.combine)) as unknown as Semigroup.Semigroup<Types.Simplify<S>>,
  posture: _postures(Record.values(_mapped(fields, (row) => row.posture))),
  alike: Equivalence.struct(_mapped(fields, (row) => row.alike)) as unknown as Equivalence.Equivalence<Types.Simplify<S>>,
  empty: Option.all(_mapped(fields, (row) => row.empty)) as unknown as Option.Option<Types.Simplify<S>>,
})
```

## [04]-[FOLD_ENTRY]

[FOLD_ENTRY]:
- Owner: `Merge.fold` — the absence-honest fold: a non-empty collection folds through `combineMany` on its head, an empty collection falls to the instance's lawful `empty`, and the return is `Option` because an identity-free instance has no lawful answer for zero rows; a caller holding a witnessed `NonEmptyReadonlyArray` composes `instance.combine.combineMany(Array.headNonEmpty(rows), Array.tailNonEmpty(rows))` directly — the proven-arity spelling, never a second entrypoint.
- Law: `Merge.monoid` projects the lawful `Monoid` exactly where an `empty` exists — `Monoid.fromSemigroup(combine, empty)` — so `combineAll` is the state-vector fold over whole collections and the instance lift through a functor rides `SemiApplicative.getSemigroup`/`Applicative.getMonoid` at the consuming site; a hand re-fold from a forged zero beside a lawful monoid is the deleted spelling.
- Law: the reducer the incremental engines apply in `fold#MEMORY_LANE` and `fold#VERSIONED_LANE` is the elementwise projection of this same `combineMany` — the merge law and the incremental reducer law are one law at two speeds, declared once here.
- Law: `Merge.convergent` is the gate read — `commutative && idempotent` — the replay retraction guard and the law-obligation selector consume the same predicate, never a re-derived posture check.
- Boundary: law assertion runs in the tests estate — `Foldable.combineMap` folds a generated op container through the projected monoid inside law bodies, over Schema-derived arbitraries and the frozen corpus fixtures.

```typescript
const _fold = <A>(instance: Merge.Instance<A>, rows: ReadonlyArray<A>): Option.Option<A> =>
  Array.isNonEmptyReadonlyArray(rows)
    ? Option.some(instance.combine.combineMany(Array.headNonEmpty(rows), Array.tailNonEmpty(rows)))
    : instance.empty

const _monoid = <A>(instance: Merge.Instance<A>): Option.Option<Monoid.Monoid<A>> =>
  Option.map(instance.empty, (empty) => Monoid.fromSemigroup(instance.combine, empty))
```

## [05]-[LAW_SURFACE]

[LAW_SURFACE]:
- Owner: `Converge` — the `_LAWS` anchor with its `_OBLIGED` gate record and the `_WITNESSES` record, one gate and one total witness per law over an instance and a three-value sample, so a law is data a harness enumerates, never prose a spec restates; `Breach` is the typed fault carrying the broken law and the sample operands themselves — evidence as data the harness shrinks, rendered only at the reporting edge.
- Law: obligations derive as one filter of the `_LAWS` anchor through the `_OBLIGED` gates — `associativity` unconditionally, `commutativity`/`idempotence` from the posture, `identity` from `Option.isSome(empty)` — so an instance cannot under-declare its proof surface, a new law is one anchor entry plus one gate row plus one witness row, and `Merge.counter`'s non-idempotent posture routes it around the idempotence law toward the delivery-uniqueness witness `causal`'s admission provides: redelivered envelopes shed as `Drained` receipt evidence before any op reaches a fold, because the engine lanes compact multiplicities without structural dedup and only an idempotent combine absorbs a duplicate that slips past admission.
- Law: every witness compares through the instance's own `alike` — the equivalence declared at the instance is the equality the law is proven under, so structural classes, plain records, and branded scalars all prove under one spelling.
- Law: `Converge.commutes` is the bridge law between instance and fold — for a convergence-legal instance, folding any two permutations of one delivered op set through the caller-supplied run yields equivalent tables; the run parameter is `fold#PLAN_CONTRACT`'s `Fold.run` closed over the instance's plan — `(ops) => Fold.run(plan, ops)` — so instance proofs and replay proofs share one predicate with zero import cycle, and a non-convergent instance answers `false` by construction rather than sampling its way to a lie.
- Law: `Converge.tables` compares key census and per-key states under the instance's `alike` — the one table comparison every convergence and replay assertion uses; a `JSON.stringify` table diff or reference comparison is the deleted spelling.
- Law: `Converge.Fixture` binds a law set to a frozen corpus asset — `corpus` names the estate root, `seam` the cross-language seam id, `asset` the fixture coordinate — so the C#-minted op families the interchange codec decodes prove their instance laws against pinned bytes, not only generated samples.
- Boundary: permutation generation, run counts, and shrinking are tests-estate decisions; corpus bytes live at `tests/contracts` and no fixture path is hardcoded in the library.
- Growth: a new law is one `_LAWS` row plus one witness arm — the record contract turns a missing witness into a compile error; a new instance adds zero lines here.

```typescript
const _LAWS = ["associativity", "commutativity", "idempotence", "identity"] as const

declare namespace Converge {
  type Law = (typeof _LAWS)[number]
  type Sample<A> = { readonly first: A; readonly second: A; readonly third: A }
  type Fixture = { readonly corpus: string; readonly seam: string; readonly asset: string; readonly laws: ReadonlyArray<Law> }
  type Shape = {
    readonly obligations: <A>(instance: Merge.Instance<A>) => ReadonlyArray<Law>
    readonly witness: <A>(instance: Merge.Instance<A>, sample: Sample<A>) => Either.Either<ReadonlyArray<Law>, Breach>
    readonly commutes: <Op, K, S>(
      instance: Merge.Instance<S>,
      run: (ops: ReadonlyArray<Op>) => HashMap.HashMap<K, S>,
    ) => (left: ReadonlyArray<Op>, right: ReadonlyArray<Op>) => boolean
    readonly tables: <K, S>(
      alike: Equivalence.Equivalence<S>,
    ) => (left: HashMap.HashMap<K, S>, right: HashMap.HashMap<K, S>) => boolean
    readonly fixture: (spec: Fixture) => Fixture
  }
}

class Breach extends Data.TaggedError("Breach")<{
  readonly law: Converge.Law
  readonly operands: ReadonlyArray<unknown>
}> {}

const _OBLIGED: { readonly [L in Converge.Law]: <A>(instance: Merge.Instance<A>) => boolean } = {
  associativity: () => true,
  commutativity: (instance) => instance.posture.commutative,
  idempotence: (instance) => instance.posture.idempotent,
  identity: (instance) => Option.isSome(instance.empty),
}

const _WITNESSES: { readonly [L in Converge.Law]: <A>(instance: Merge.Instance<A>, sample: Converge.Sample<A>) => boolean } = {
  associativity: (instance, { first, second, third }) =>
    instance.alike(
      instance.combine.combine(instance.combine.combine(first, second), third),
      instance.combine.combine(first, instance.combine.combine(second, third)),
    ),
  commutativity: (instance, { first, second }) =>
    instance.alike(instance.combine.combine(first, second), instance.combine.combine(second, first)),
  idempotence: (instance, { first }) => instance.alike(instance.combine.combine(first, first), first),
  identity: (instance, { first }) =>
    Option.match(instance.empty, {
      onNone: () => true,
      onSome: (empty) =>
        instance.alike(instance.combine.combine(first, empty), first)
        && instance.alike(instance.combine.combine(empty, first), first),
    }),
}

const _tables = <K, S>(alike: Equivalence.Equivalence<S>) =>
(left: HashMap.HashMap<K, S>, right: HashMap.HashMap<K, S>): boolean =>
  HashMap.size(left) === HashMap.size(right)
  && HashMap.reduce(left, true, (holds, state, key) =>
    holds && Option.match(HashMap.get(right, key), { onNone: () => false, onSome: (held) => alike(state, held) }))

const _commutes = <Op, K, S>(
  instance: Merge.Instance<S>,
  run: (ops: ReadonlyArray<Op>) => HashMap.HashMap<K, S>,
) =>
(left: ReadonlyArray<Op>, right: ReadonlyArray<Op>): boolean =>
  Merge.convergent(instance) && _tables<K, S>(instance.alike)(run(left), run(right))

const Converge: Converge.Shape = {
  obligations: (instance) => Array.filter(_LAWS, (law) => _OBLIGED[law](instance)),
  witness: (instance, sample) =>
    Option.match(
      Array.findFirst(Converge.obligations(instance), (law) => !_WITNESSES[law](instance, sample)),
      {
        onNone: () => Either.right(Converge.obligations(instance)),
        onSome: (law) =>
          Either.left(new Breach({ law, operands: [sample.first, sample.second, sample.third] })),
      },
    ),
  commutes: _commutes,
  tables: _tables,
  fixture: (spec) => spec,
}
```

## [06]-[MERGE_CELLS]

[MERGE_CELLS]:
- Owner: `Merge.cell` — one input-shaped transactional entry: `cell(instance)` builds the keyed `TMap` table whose `absorb` folds a whole row batch in one commit, while `cell(instance, { initial })` builds the isolated `TRef` twin for one state; both absorb through the same instance, conflicting writers re-run automatically, and readers observe only committed states.
- Law: `settled` is one wait surface over two modalities discriminated on the probe's shape — the `[key, holds]` pair suspends through `STM.check` until the key's state exists and satisfies the predicate; the bare table predicate snapshots the whole committed census inside the transaction and suspends until it holds, the whole-table stability wait `causal#FRONTIER_TRACKER` composes — wait-until-merged without a poll loop; the transaction re-runs when a participating cell changes, so convergence waits are compose-or-retry, never cadence.
- Law: the cell composes the instance it is built from — insert (`none -> value`) and update (`some -> combine`) are two arms of one keyed fold inside the transaction, so the live table and the pure `Merge.fold` agree by construction and the cell adds no second merge semantics.
- Law: the isolated modality carries `absorb(state)`, the transactional `read`, and predicate `settled`; it is the single-state projection of the keyed table, selected by the seed-bearing input shape instead of a `ref` sibling entrypoint.
- Law: `table` snapshots the whole census in one commit — a consistent read of every cell at one transaction point, the read `fold#PLAN_CONTRACT` tables compare against in convergence assertions.
- Boundary: a fold maintained incrementally under engine deltas is `fold#MEMORY_LANE`'s handle; the cell owns cross-fiber shared state whose writers are ordinary effects, and choosing between them is the consumer's altitude selection.
- Growth: a new transactional read is one member composing the same `TMap`; a census-size gate or quorum wait is already the table-probe `settled` spelling, never a new member.

```typescript
function _cell<K, S>(instance: Merge.Instance<S>): Effect.Effect<Merge.Cell<K, S>>
function _cell<S>(instance: Merge.Instance<S>, seed: { readonly initial: S }): Effect.Effect<Merge.Single<S>>
function _cell<K, S>(
  instance: Merge.Instance<S>,
  seed?: { readonly initial: S },
): Effect.Effect<Merge.Cell<K, S> | Merge.Single<S>> {
  return seed === undefined
    ? Effect.gen(function* () {
        const cells = yield* STM.commit(TMap.empty<K, S>())
        const settled = (
          probe: readonly [key: K, holds: (state: S) => boolean] | ((table: HashMap.HashMap<K, S>) => boolean),
        ): Effect.Effect<void> =>
          STM.commit(
            Predicate.isFunction(probe)
              ? STM.flatMap(TMap.toChunk(cells), (rows) => STM.check(() => probe(HashMap.fromIterable(rows))))
              : STM.flatMap(TMap.get(cells, probe[0]), (held) =>
                  STM.check(() => Option.match(held, { onNone: () => false, onSome: probe[1] }))),
          )
        return {
          absorb: (rows: ReadonlyArray<readonly [K, S]>) =>
            STM.commit(
              STM.forEach(rows, ([key, value]) =>
                STM.gen(function* () {
                  const held = yield* TMap.get(cells, key)
                  yield* TMap.set(cells, key, Option.match(held, {
                    onNone: () => value,
                    onSome: (state) => instance.combine.combine(state, value),
                  }))
                }), { discard: true }),
            ),
          read: (key) => STM.commit(TMap.get(cells, key)),
          table: Effect.map(STM.commit(TMap.toChunk(cells)), HashMap.fromIterable),
          settled,
        }
      })
    : Effect.map(STM.commit(TRef.make(seed.initial)), (cell): Merge.Single<S> => ({
        absorb: (state) => STM.commit(TRef.update(cell, (held) => instance.combine.combine(held, state))),
        read: STM.commit(TRef.get(cell)),
        settled: (holds) => STM.commit(STM.flatMap(TRef.get(cell), (held) => STM.check(() => holds(held)))),
      }))
}

const Merge: Merge.Shape = {
  instance: (spec) => spec,
  max: _max,
  min: _min,
  lattice: _lattice,
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
  hashSet: <A>(): Merge.Instance<HashSet.HashSet<A>> => ({
    combine: Semigroup.make((self, that) => HashSet.union(self, that)),
    posture: _LATTICE,
    alike: Equal.equivalence(),
    empty: Option.some(HashSet.empty()),
  }),
  hashMap: <K, V>(row: Merge.Instance<V>): Merge.Instance<HashMap.HashMap<K, V>> => ({
    combine: Semigroup.make((self, that) =>
      HashMap.reduce(that, self, (acc, value, key) =>
        HashMap.modifyAt(acc, key, (slot) =>
          Option.some(Option.match(slot, {
            onNone: () => value,
            onSome: (held) => row.combine.combine(held, value),
          }))))),
    posture: row.posture,
    alike: Equivalence.make(_tables<K, V>(row.alike)),
    empty: Option.some(HashMap.empty()),
  }),
  optional: (row) => ({
    combine: OptionInstances.getOptionalMonoid(row.combine),
    posture: row.posture,
    alike: Option.getEquivalence(row.alike),
    empty: Option.some(Option.none()),
  }),
  struct: _struct,
  imap: (row, to, from) => ({
    combine: Semigroup.imap(row.combine, to, from),
    posture: row.posture,
    alike: Equivalence.mapInput(row.alike, from),
    empty: Option.map(row.empty, to),
  }),
  fold: _fold,
  monoid: _monoid,
  convergent: (instance) => instance.posture.commutative && instance.posture.idempotent,
  cell: _cell,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Breach, Converge, Merge }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
