# [CORE_MERGE]

The lawful merge owner. `Merge.Instance<A>` couples a semigroup, `Merge.Law`, equivalence, and optional identity.

## [01]-[INDEX]

- [02]-[INSTANCE_CONTRACT]: combine, closed law, alike, and identity; `Merge.Law`, `Merge.Instance`.
- [03]-[INSTANCE_ROSTER]: scalar, keyed, set, and product constructor rows — `Merge.max` through `Merge.tuple`.
- [04]-[FOLD_ENTRY]: fold, monoid projection, commutativity, and idempotence; `Merge`.
- [05]-[LAW_SURFACE]: obligations, witnesses, replay; `Merge.laws`, `Merge.Breach`.
- [06]-[MERGE_CELLS]: keyed transactional cell table; `Merge.cell`.

## [02]-[INSTANCE_CONTRACT]

[INSTANCE_CONTRACT]:
- Owner: `Merge.Instance<A>` carries `combine`, `Merge.Law`, `alike`, and an optional lawful identity.
- Law: `Merge.Law` is the estate's commutation vocabulary, row-identical to the `csharp:Rasm.Persistence/Version/ledger#CHANGEFEED` `OpLaw` a lane stance and a CRDT op arm both answer; a mutation kind grading `ordered` needs a total order to settle a concurrent pair, so exactly one side loses and the loss is evidence a receipt carries rather than a merge silently absorbing it.
- Growth: a new merge semantic is a new constructor row or a `data/*` atom lift; the `Instance` shape never widens per semantic.

```typescript
import type * as Bounded from "@effect/typeclass/Bounded"
import * as Monoid from "@effect/typeclass/Monoid"
import * as Semigroup from "@effect/typeclass/Semigroup"
import * as BooleanInstances from "@effect/typeclass/data/Boolean"
import * as NumberInstances from "@effect/typeclass/data/Number"
import * as OptionInstances from "@effect/typeclass/data/Option"
import * as RecordInstances from "@effect/typeclass/data/Record"
import { Array, Data, Effect, Either, Equal, Equivalence, HashMap, HashSet, Option, type Order, Predicate, Record, STM, TMap, TRef, type Types } from "effect"
import { Fault } from "../value/fault.ts"

declare namespace Merge {
  type Law = "ordered" | "commutative" | "semilattice"
  type Instance<A> = {
    readonly combine: Semigroup.Semigroup<A>
    readonly law: Law
    readonly alike: Equivalence.Equivalence<A>
    readonly empty: Option.Option<A>
  }
  type Fields<S> = { readonly [K in keyof S]: Instance<S[K]> }
  type Slots<T extends ReadonlyArray<unknown>> = { readonly [I in keyof T]: Instance<T[I]> }
  type Lattice<A> = { readonly join: Instance<A>; readonly meet: Instance<A> }
  type Cell<K, S> = {
    readonly absorb: (rows: ReadonlyArray<readonly [K, S]>) => Effect.Effect<void, CellFault<K>>
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
  type CellFault<K> = _CellFault<K>
  type Shape = {
    readonly max: <A>(order: Order.Order<A>) => Instance<A>
    readonly min: <A>(order: Order.Order<A>) => Instance<A>
    readonly lattice: <A>(bounds: Bounded.Bounded<A>) => Lattice<A>
    readonly first: <A>(alike: Equivalence.Equivalence<A>) => Instance<A>
    readonly counter: Instance<number>
    readonly flag: Instance<boolean>
    readonly union: <K extends string, V>(row: Instance<V>) => Instance<Record.ReadonlyRecord<K, V>>
    readonly hashSet: <A>() => Instance<HashSet.HashSet<A>>
    readonly hashMap: <K, V>(row: Instance<V>) => Instance<HashMap.HashMap<K, V>>
    readonly optional: <A>(row: Instance<A>) => Instance<Option.Option<A>>
    readonly struct: <S extends object>(fields: Fields<S>) => Instance<Types.Simplify<S>>
    readonly tuple: <T extends ReadonlyArray<unknown>>(...rows: Slots<T>) => Instance<T>
    readonly imap: <A, B>(row: Instance<A>, to: (value: A) => B, from: (wrapped: B) => A) => Instance<B>
    readonly fold: <A>(instance: Instance<A>, rows: ReadonlyArray<A>) => Option.Option<A>
    readonly monoid: <A>(instance: Instance<A>) => Option.Option<Monoid.Monoid<A>>
    readonly commutative: <A>(instance: Instance<A>) => boolean
    readonly idempotent: <A>(instance: Instance<A>) => boolean
    readonly cell: {
      <K, S>(instance: Instance<S>): Effect.Effect<Cell<K, S>>
      <K, S>(instance: Instance<S>, seed: { readonly keys: Array.NonEmptyReadonlyArray<K> }): Effect.Effect<Cell<K, S>>
      <S>(instance: Instance<S>, seed: { readonly initial: S }): Effect.Effect<Single<S>>
    }
    readonly laws: Converge.Shape
    readonly Breach: typeof Breach
    readonly CellFault: typeof _CellFault
  }
}

const _LATTICE: Merge.Law = "semilattice"
const _COMMUTES: Merge.Law = "commutative"
const _ORDERED: Merge.Law = "ordered"

const _commutative = (law: Merge.Law): boolean => law !== _ORDERED
const _idempotent = (law: Merge.Law): boolean => law === _LATTICE

const _fromOrder = <A>(order: Order.Order<A>): Equivalence.Equivalence<A> =>
  Equivalence.make((self, that) => order(self, that) === 0)
```

## [03]-[INSTANCE_ROSTER]

[INSTANCE_ROSTER]:

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

const _laws = (rows: ReadonlyArray<Merge.Law>): Merge.Law =>
  Array.every(rows, _idempotent) ? _LATTICE : Array.every(rows, _commutative) ? _COMMUTES : _ORDERED

const _max = <A>(order: Order.Order<A>): Merge.Instance<A> => ({
  combine: Semigroup.max(order),
  law: _LATTICE,
  alike: _fromOrder(order),
  empty: Option.none(),
})

const _min = <A>(order: Order.Order<A>): Merge.Instance<A> => ({
  combine: Semigroup.min(order),
  law: _LATTICE,
  alike: _fromOrder(order),
  empty: Option.none(),
})

const _lattice = <A>(bounds: Bounded.Bounded<A>): Merge.Lattice<A> => ({
  join: { ..._max(bounds.compare), empty: Option.some(bounds.minBound) },
  meet: { ..._min(bounds.compare), empty: Option.some(bounds.maxBound) },
})

const _struct = <S extends object>(fields: Merge.Fields<S>): Merge.Instance<Types.Simplify<S>> => ({
  // BOUNDARY ADAPTER: the typeclass composers state their own record shape, so each rebinding restores the flattened
  // owner type the mapped-key projection already proved; no asserted value crosses
  combine: Semigroup.struct(_mapped(fields, (row) => row.combine)) as unknown as Semigroup.Semigroup<Types.Simplify<S>>,
  law: _laws(Record.values(_mapped(fields, (row) => row.law))),
  alike: Equivalence.struct(_mapped(fields, (row) => row.alike)) as unknown as Equivalence.Equivalence<Types.Simplify<S>>,
  empty: Option.all(_mapped(fields, (row) => row.empty)) as unknown as Option.Option<Types.Simplify<S>>,
})

const _indexed = <T extends ReadonlyArray<unknown>, R>(
  rows: Merge.Slots<T>,
  project: (row: Merge.Instance<unknown>) => R,
): { readonly [I in keyof T]: R } => {
  // BOUNDARY ADAPTER: Array.map homogenizes tuple elements; the mapped-index contract restores the exact arity before the value leaves
  return Array.map(
    rows as unknown as ReadonlyArray<Merge.Instance<unknown>>,
    project,
  ) as unknown as { readonly [I in keyof T]: R }
}

const _tuple = <T extends ReadonlyArray<unknown>>(...rows: Merge.Slots<T>): Merge.Instance<T> => ({
  // BOUNDARY ADAPTER: the positional twin of the record rebinding — the spread composers homogenize their slots, so
  // each rebinding restores the arity the mapped-index projection already proved
  combine: Semigroup.tuple(..._indexed<T, Semigroup.Semigroup<unknown>>(rows, (row) => row.combine)) as unknown as Semigroup.Semigroup<T>,
  law: _laws(_indexed<T, Merge.Law>(rows, (row) => row.law)),
  alike: Equivalence.tuple(..._indexed<T, Equivalence.Equivalence<unknown>>(rows, (row) => row.alike)) as unknown as Equivalence.Equivalence<T>,
  empty: Option.all(_indexed<T, Option.Option<unknown>>(rows, (row) => row.empty)) as unknown as Option.Option<T>,
})
```

## [04]-[FOLD_ENTRY]

[FOLD_ENTRY]:

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
- Owner: `Merge.laws` — one gate and one witness per law; `Merge.Breach` carries typed failure evidence under the same owner.

```typescript
const _LAWS = ["associativity", "commutativity", "idempotence", "identity"] as const

declare namespace Converge {
  type Law = (typeof _family.reasons)[number] // the frozen snapshot the family mint publishes, so a caller cannot drift the law roster the witnesses enumerate
  type Sample<A> = { readonly first: A; readonly second: A; readonly third: A }
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
  }
}

// One row per law carrying the core kind alone: a merge law that fails is a torn convergence invariant — system-blamed,
// never re-driven, and no repair report to quarantine into — so retryability and blame read off the core row table and
// no local rank, retry, or status column rides beside `class`. `Data` is the declaration form because the operands are
// the caller's own `A` values a harness shrinks in process; nothing here crosses a wire to earn a codec.
const _family = Fault.Class.family(_LAWS, {
  associativity: { class: "breached" },
  commutativity: { class: "breached" },
  idempotence: { class: "breached" },
  identity: { class: "breached" },
})

class Breach extends Data.TaggedError("Breach")<{
  readonly law: Converge.Law
  readonly operands: ReadonlyArray<unknown>
}> {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.law)
  }
  override get message(): string {
    return `<merge:${this.law}> refused over ${this.operands.length} operands`
  }
}

class _CellFault<K> extends Data.TaggedError("CellFault")<{
  readonly key: K
}> {
  readonly class = "invalid" as const
  override get message(): string {
    return "<merge:unknown-key>"
  }
}

const _OBLIGED: { readonly [L in Converge.Law]: <A>(instance: Merge.Instance<A>) => boolean } = {
  associativity: () => true,
  commutativity: (instance) => _commutative(instance.law),
  idempotence: (instance) => _idempotent(instance.law),
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
  Merge.commutative(instance) && _tables<K, S>(instance.alike)(run(left), run(right))

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
}
```

## [06]-[MERGE_CELLS]

[MERGE_CELLS]:

```typescript
function _cell<K, S>(instance: Merge.Instance<S>): Effect.Effect<Merge.Cell<K, S>>
function _cell<K, S>(instance: Merge.Instance<S>, seed: { readonly keys: Array.NonEmptyReadonlyArray<K> }): Effect.Effect<Merge.Cell<K, S>>
function _cell<S>(instance: Merge.Instance<S>, seed: { readonly initial: S }): Effect.Effect<Merge.Single<S>>
function _cell<K, S>(
  instance: Merge.Instance<S>,
  seed?: { readonly keys: Array.NonEmptyReadonlyArray<K> } | { readonly initial: S },
): Effect.Effect<Merge.Cell<K, S> | Merge.Single<S>> {
  return seed === undefined || Predicate.hasProperty(seed, "keys")
    ? Effect.gen(function* () {
        const cells = yield* STM.commit(TMap.empty<K, S>())
        const topology = seed === undefined ? Option.none<HashSet.HashSet<K>>() : Option.some(HashSet.fromIterable(seed.keys))
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
            Option.match(
              Array.findFirst(rows, ([key]) => Option.exists(topology, (keys) => !HashSet.has(keys, key))),
              {
                onNone: () => STM.commit(
                  STM.forEach(rows, ([key, value]) =>
                    STM.gen(function* () {
                      const held = yield* TMap.get(cells, key)
                      yield* TMap.set(cells, key, Option.match(held, {
                        onNone: () => value,
                        onSome: (state) => instance.combine.combine(state, value),
                      }))
                    }), { discard: true }),
                ),
                onSome: ([key]) => Effect.fail(new _CellFault({ key })),
              },
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
  max: _max,
  min: _min,
  lattice: _lattice,
  first: (alike) => ({ combine: Semigroup.first(), law: _ORDERED, alike, empty: Option.none() }),
  counter: {
    combine: NumberInstances.SemigroupSum,
    law: _COMMUTES,
    alike: Equivalence.number,
    empty: Option.some(0),
  },
  flag: {
    combine: BooleanInstances.SemigroupSome,
    law: _LATTICE,
    alike: Equivalence.boolean,
    empty: Option.some(false),
  },
  union: <K extends string, V>(row: Merge.Instance<V>): Merge.Instance<Record.ReadonlyRecord<K, V>> => ({
    combine: RecordInstances.getSemigroupUnion(row.combine),
    law: row.law,
    alike: Record.getEquivalence(row.alike),
    empty: Option.some(Record.empty()),
  }),
  hashSet: <A>(): Merge.Instance<HashSet.HashSet<A>> => ({
    combine: Semigroup.make((self, that) => HashSet.union(self, that)),
    law: _LATTICE,
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
    law: row.law,
    alike: Equivalence.make(_tables<K, V>(row.alike)),
    empty: Option.some(HashMap.empty()),
  }),
  optional: (row) => ({
    combine: OptionInstances.getOptionalMonoid(row.combine),
    law: row.law,
    alike: Option.getEquivalence(row.alike),
    empty: Option.some(Option.none()),
  }),
  struct: _struct,
  tuple: _tuple,
  imap: (row, to, from) => ({
    combine: Semigroup.imap(row.combine, to, from),
    law: row.law,
    alike: Equivalence.mapInput(row.alike, from),
    empty: Option.map(row.empty, to),
  }),
  fold: _fold,
  monoid: _monoid,
  commutative: (instance) => _commutative(instance.law),
  idempotent: (instance) => _idempotent(instance.law),
  cell: _cell,
  laws: Converge,
  Breach,
  CellFault: _CellFault,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Merge }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
