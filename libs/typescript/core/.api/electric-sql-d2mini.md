# [TS_CORE_API_ELECTRIC_SQL_D2MINI]

[PACKAGE_SURFACE]:
- package: `@electric-sql/d2mini` · version `` · license `Apache-2.0`
- module: ESM only (`type: module`); single `.` barrel — no subpaths, no durable/replication bindings.
- asset: `dist/index.d.ts` (barrel over `d catalog` / `multiset` / `operators` / `types`); bundles `fractional-indexing` + `murmurhash-js` + `sorted-btree`.
- runtime: pure-TS in-process dataflow; runs in node / bun / browser / worker. ZERO peer dependencies — the whole engine is self-contained, so it is browser-safe by construction and the natural in-memory lane for `state`.
- ABI: fully synchronous scheduler (`graph.run()`/`graph.step()`); the `sorted-btree` `*BTree` ordered files ship in `dist/operators/` but are NOT re-exported by the `.`-only barrel, so no async entry is reachable.
- plane: `plane:runtime` (W1); folder-local to `state`, catalogued here.
- rail: incremental-dataflow / fold-maintenance.

`@electric-sql/d2mini` is the browser altitude of the two-altitude fold: the SAME operator algebra as `d2ts.md`, stripped of partial-order time (no `Version`/`Antichain`/`Frontier`, no `Message` frontier signal, no versioned `Index.reconstructAt`). A collection is a signed `MultiSet` pushed with `sendData(collection)` — no version coordinate — so the graph maintains the live fold in memory with the least possible surface. It is the lane `state/fold` binds when the runtime is a browser app folding wire-decoded events in memory: no durability, no time-travel, no peer runtime. Its reachable ordered lane is the fractional-index family (`topKWithFractionalIndex`/`orderByWithFractionalIndex`) — incremental order maintenance that replaces the array re-sort the design names as the defect; the `sorted-btree` twins (`orderByWithFractionalIndexBTree`/`topKWithFractionalIndexBTree`/`loadBTree`) exist in package source but the catalog `exports` map is `.`-only and the operators barrel omits their files, so they are unreachable and never composed. When a fold needs versioned time-travel, durable trace, or replication, that is a d2ts-altitude fold by definition; d2mini is the in-memory minimum.

## [01]-[GRAPH_AND_DELTA]

Time-free: the graph is constructed, wired with `pipe`, driven synchronously; the delta unit is a signed `MultiSet` with no version. `output` receives the `MultiSet` directly — there is no `Message`/`FRONTIER` envelope.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]            | [CAPABILITY_BOUNDARY]                                                                |
| :-----: | :------------------------------ | :----------------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `D2` (no options)               | class                    | the graph; `newInput()` mints a `RootStreamBuilder`, `run`/`step`/`finalize`         |
|  [02]   | `RootStreamBuilder<T>`          | class                    | input handle — `sendData(collection)` ONLY; no `sendFrontier`, no version            |
|  [03]   | `StreamBuilder<T>`              | class (`IStreamBuilder`) | pipeline node; 1..20-arity `pipe(...)` over `PipedOperator`s                         |
|  [04]   | `MultiSet<T>` (`MultiSetArray`) | class                    | signed multiset — `map`/`filter`/`negate`/`concat`/`consolidate`/`extend`/`getInner` |
|  [05]   | `Index<K, V>`                   | class                    | in-memory keyed trace — `get`/`getMultiplicity`/`join`; NO `reconstructAt`/`compact` |
|  [06]   | `KeyValue<K, V>` = `[K, V]`     | tuple alias              | the keyed-record shape the keyed operators require                                   |
|  [07]   | `PipedOperator<I, O>`           | function alias           | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>` — the ONE operator shape          |

```ts contract
// No initialFrontier, no version on sendData — the minimal, time-free graph. output sees the raw MultiSet, not a Message.
declare class D2 implements ID2 {
  constructor()
  newInput<T>(): RootStreamBuilder<T>
  step(): void; run(): void; finalize(): void
}
declare class RootStreamBuilder<T> extends StreamBuilder<T> {
  sendData(collection: MultiSet<T> | MultiSetArray<T>): void   // no version coordinate
}
declare function output<T>(fn: (data: MultiSet<T>) => void): PipedOperator<T, T>   // direct MultiSet, not Message<T>
// The MultiSet here is the leaner core: elementwise transforms live on it, the folds (join/reduce/count/distinct) are operators only.
declare class MultiSet<T> {
  constructor(data?: MultiSetArray<T>)
  map<U>(f: (d: T) => U): MultiSet<U>; filter(f: (d: T) => boolean): MultiSet<T>
  negate(): MultiSet<T>; concat(o: MultiSet<T>): MultiSet<T>; consolidate(): MultiSet<T>
  extend(o: MultiSet<T> | MultiSetArray<T>): void; getInner(): MultiSetArray<T>
}
// The in-memory Index — a key→value→multiplicity map with an incremental join; no versioned trace, no compaction frontier.
declare class Index<K, V> {
  get(key: K): [V, number][]; getMultiplicity(key: K, value: V): number
  addValue(key: K, value: [V, number]): void; append(o: Index<K, V>): void
  join<V2>(o: Index<K, V2>): MultiSet<[K, [V, V2]]>
  entries(): MapIterator<[K, Map<V, number>]>; keys(): MapIterator<K>; has(key: K): boolean; get size(): number
}
```

## [02]-[OPERATOR_ALGEBRA]

The operator library is the SAME ONE-shape `PipedOperator<I, O>` composed by `pipe` as d2ts — the roster is SEED DATA on it. d2mini drops the versioned operators (`iterate` fixpoint, `buffer`) and adds the `sorted-btree` ordered lane. Four families, same as d2ts minus recursion.

| [INDEX] | [FAMILY]      | [OPERATORS]                                                                                                                                       | [SHAPE_BOUNDARY]                                                        |
| :-----: | :------------ | :------------------------------------------------------------------------------------------------------------------------------------------------ | :---------------------------------------------------------------------- |
|  [01]   | element-wise  | `map` `filter` `negate` `concat` `consolidate` `debug` `output`                                                                                   | `PipedOperator<T, …>`; `output` sees the raw `MultiSet`                 |
|  [02]   | keying        | `keyBy(fn)` `unkey()` `rekey(fn)` `filterBy(other)`                                                                                               | `T ⇄ KeyValue<K, T>`; `filterBy` semijoins against a key stream         |
|  [03]   | keyed fold    | `join`(+`inner`/`left`/`right`/`full`/`anti`, `JoinType`) `reduce` `count` `distinct` `groupBy`(+`sum`/`count`/`avg`/`min`/`max`/`median`/`mode`) | `KeyValue<K, V>` in — the incremental Semigroup fold per key            |
|  [04]   | ordered       | `topK` `topKWithIndex` `topKWithFractionalIndex` `orderBy`(+`WithIndex`/`WithFractionalIndex`)                                                    | fractional-index maintenance; NO full re-sort                           |
|  [05]   | ordered/BTree | `topKWithFractionalIndexBTree` `orderByWithFractionalIndexBTree` (`loadBTree()`)                                                                  | SEALED — files unreferenced by the catalog-bound barrel; not importable |

```ts contract
// Same pipe/join/reduce/groupBy algebra as d2ts — the reducer is a @effect/typeclass Semigroup applied per key.
type JoinType = 'inner' | 'left' | 'right' | 'full' | 'anti'
declare function join<K, V1, V2, T>(other: IStreamBuilder<KeyValue<K, V2>>, type?: JoinType): PipedOperator<T, KeyValue<K, [V1 | null, V2 | null]>>
declare function reduce<K, V1, R, T>(f: (values: [V1, number][]) => [R, number][]): (s: IStreamBuilder<T>) => IStreamBuilder<KeyValue<K, R>>
declare function groupBy<T, K, A extends Record<string, AggregateFunction<T, any>>>(keyFn: (d: T) => K, aggs?: A): (s: IStreamBuilder<T>) => IStreamBuilder<KeyValue<string, K & AggregatesReturnType<T, A>>>
declare const groupByOperators: { sum; count; avg; min; max; median; mode }
declare function filterBy<K, V1, T>(other: IStreamBuilder<KeyValue<K, unknown>>): PipedOperator<T, KeyValue<K, V1>>   // semijoin
```

```ts contract
// The reachable ordered lane: fractional string indices give a stable position without renumbering, so an insert moves one key, never a re-sort. The *BTree twins are defined in dist/operators/orderByBTree.js and topKWithFractionalIndexBTree.js but neither file is re-exported by dist/operators/index.js and no subpath exists — sealed until upstream re-exports them.
declare function topKWithFractionalIndex<K, V1, T>(cmp: (a: V1, b: V1) => number, opts?: { limit?; offset? }): PipedOperator<T, KeyValue<K, [V1, string]>>
declare function orderByWithFractionalIndex<T, Ve>(valueExtractor: (v: V) => Ve, opts?: { comparator?; limit?; offset? }): (s: IStreamBuilder<T>) => IStreamBuilder<KeyValue<K, [V, string]>>
```

## [03]-[INTEGRATION]

[STACK: `D2` + `effect/Data` + `Schema` (`.api/effect.md`)] — identical to the d2ts altitude: the collection element is a `Data.TaggedEnum` op decoded by a `Schema`; `map`/`filter` bodies match on `_tag` via `effect/Match`; `Equal.equals`/`Hash` make `consolidate`/`distinct` structural so equal ops collapse — idempotence. `state` folds this in core `effect`; d2mini is the in-memory engine under it.

[STACK: `reduce`/`groupBy` + `@effect/typeclass` (`.api/effect-typeclass.md`)] — the keyed fold is a Semigroup applied incrementally; `state/merge` declares the merge as one lawful `Semigroup` and `state/fold` applies it through `reduce` at the in-memory altitude exactly as d2ts applies it at the durable one. The same reducer law is shared across both altitudes.

[STACK: two altitudes — d2mini core, `d2ts.md` durable] — `state/fold` binds ONE algebra; the browser runtime folds through d2mini (in-memory, `sendData(collection)`, no time), the node runtime folds through d2ts (durable, `sendData(version, collection)`, `Index.reconstructAt`, `./sqlite`). d2mini has no `Version`, so it carries no AsOf time-travel and no retention frontier — a fold that needs either is a d2ts fold. Never a second fold implementation; the two are one algebra at two surfaces.

[STACK: presence + `state/fold` (`.api/effect.md` `Subscribable`)] — the in-memory fold is what `state/fold` exposes as an `effect` `Subscribable`: an `output(fn)` sink drives a `SubscriptionRef`, so the browser presence view is the d2mini fold re-fired on each `run()`. `edge/live` serves that presence semantics; the `orderByWithFractionalIndex` lane keeps the live-list ordering incremental as rows churn.

## [04]-[RAIL_LAW]

- Owns: minimal, time-free incremental dataflow — a graph of `PipedOperator`s over signed `MultiSet` deltas with no version, the keyed folds (`join`/`reduce`/`count`/`distinct`/`groupBy`), the fractional-index ordered lane, and the in-memory `Index` join. Zero peer dependencies — the browser-safe in-memory altitude of the `state/fold`.
- Accept: `sendData(delta)` with signed multisets; `pipe(...)` composition of the operator roster; `reduce`/`groupBy` as the incremental application of a `@effect/typeclass` Semigroup; `output(fn)` driving a `Subscribable` for `state/fold`; `topKWithFractionalIndex`/`orderByWithFractionalIndex` for ordered views; d2mini for every browser/in-memory fold path.
- Reject: reaching for a `Version`, `Antichain`, frontier, `reconstructAt`, or durable trace here (that is the d2ts altitude); a full-collection re-sort where `topKWithFractionalIndex` maintains order; citing `loadBTree`/`*BTree` operators as importable — the catalog barrel omits their files and no subpath exists; a second in-memory fold implementation beside this one.
- Boundary: no time coordinate means no time-travel, no retention frontier, no replication — those are d2ts capabilities. `min`/`max` exist only as `groupBy` aggregates (not bare operators, unlike d2ts's `MultiSet.min`/`max`); `distinct` and those aggregates reject negative multiplicity, so retraction folds run through `reduce`/`consolidate`. Every reachable surface drains synchronously.
