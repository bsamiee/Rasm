# [TS_CORE_API_ELECTRIC_SQL_D2MINI]

[PACKAGE_SURFACE]:
- package: `@electric-sql/d2mini` (Apache-2.0)
- module: ESM only (`type: module`); single `.` barrel — no subpaths, no durable/replication bindings.
- asset: `dist/index.d.ts` (barrel over `d catalog` / `multiset` / `operators` / `types`); bundles `fractional-indexing` + `murmurhash-js` + `sorted-btree`.
- runtime: pure-TS in-process dataflow; runs in node / bun / browser / worker. ZERO peer dependencies — the whole engine is self-contained, so it is browser-safe by construction and the natural in-memory lane for `state`.
- ABI: fully synchronous scheduler (`graph.run()`/`graph.step()`); the `sorted-btree` `*BTree` ordered files ship in `dist/operators/` but are NOT re-exported by the `.`-only barrel, so no async entry is reachable.
- plane: `plane:runtime` (W1); folder-local to `state`, catalogued here.
- rail: incremental-dataflow / fold-maintenance.

`@electric-sql/d2mini` is the browser altitude of the two-altitude fold: the SAME operator algebra as `d2ts.md`, stripped of partial-order time (no `Version`/`Antichain`/`Frontier`, no `Message` frontier signal, no versioned `Index.reconstructAt`). A collection is a signed `MultiSet` pushed with `sendData(collection)` — no version coordinate — so the graph maintains the live fold in memory with the least possible surface. It is the lane `state/fold` binds when the runtime is a browser app folding wire-decoded events in memory: no durability, no time-travel, no peer runtime. Its reachable ordered lane is the fractional-index family (`topKWithFractionalIndex`/`orderByWithFractionalIndex`) — incremental order maintenance that replaces the array re-sort the design names as the defect; the `sorted-btree` twins (`orderByWithFractionalIndexBTree`/`topKWithFractionalIndexBTree`/`loadBTree`) exist in package source but the catalog `exports` map is `.`-only and the operators barrel omits their files, so they are unreachable and never composed. When a fold needs versioned time-travel, durable trace, or replication, that is a d2ts-altitude fold by definition; d2mini is the in-memory minimum.

## [01]-[GRAPH_AND_DELTA]

Time-free: the graph is constructed, wired with `pipe`, driven synchronously; the delta unit is a signed `MultiSet` with no version. `output` receives the `MultiSet` directly — there is no `Message`/`FRONTIER` envelope.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY_BOUNDARY]                                                                |
| :-----: | :------------------------------ | :------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `D2` (no options)               | class          | the graph; `newInput()` mints a `RootStreamBuilder`, `run`/`step`/`finalize`         |
|  [02]   | `RootStreamBuilder<T>`          | class          | input handle — `sendData(collection)` ONLY; no `sendFrontier`, no version            |
|  [03]   | `StreamBuilder<T>`              | class          | pipeline node (`IStreamBuilder`); 1..20-arity `pipe(...)` over `PipedOperator`s      |
|  [04]   | `MultiSet<T>` (`MultiSetArray`) | class          | elementwise `map`/`filter`/`negate`/`concat`/`consolidate`/`extend`/`getInner`       |
|  [05]   | `Index<K, V>`                   | class          | in-memory keyed trace — `get`/`getMultiplicity`/`join`; NO `reconstructAt`/`compact` |
|  [06]   | `KeyValue<K, V>` = `[K, V]`     | tuple alias    | the keyed-record shape the keyed operators require                                   |
|  [07]   | `PipedOperator<I, O>`           | function alias | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>` — the ONE operator shape          |

[D2]: `D2()` `D2.newInput() -> RootStreamBuilder<T>` `D2.step() -> void` `D2.run() -> void` `D2.finalize() -> void`
[ROOT_STREAM_BUILDER]: `RootStreamBuilder.sendData(MultiSet<T>|MultiSetArray<T>) -> void`
[MULTI_SET]: `MultiSet(MultiSetArray<T>?)` `MultiSet.map((d:T)=>U) -> MultiSet<U>` `MultiSet.filter((d:T)=>boolean) -> MultiSet<T>` `MultiSet.negate() -> MultiSet<T>` `MultiSet.concat(MultiSet<T>) -> MultiSet<T>` `MultiSet.consolidate() -> MultiSet<T>` `MultiSet.extend(MultiSet<T>|MultiSetArray<T>) -> void` `MultiSet.getInner() -> MultiSetArray<T>`
[INDEX]: `Index.get(K) -> [V,number][]` `Index.getMultiplicity(K,V) -> number` `Index.addValue(K,[V,number]) -> void` `Index.append(Index<K,V>) -> void` `Index.join(Index<K,V2>) -> MultiSet<[K,[V,V2]]>` `Index.entries() -> MapIterator<[K,Map<V,number>]>` `Index.keys() -> MapIterator<K>` `Index.has(K) -> boolean` `Index.size: number`
[SURFACES]: `output((data:MultiSet<T>)=>void) -> PipedOperator<T,T>`

## [02]-[OPERATOR_ALGEBRA]

The operator library is the SAME ONE-shape `PipedOperator<I, O>` composed by `pipe` as d2ts — the roster is SEED DATA on it. d2mini drops the versioned operators (`iterate` fixpoint, `buffer`) and adds the `sorted-btree` ordered lane. Four families, same as d2ts minus recursion.

| [INDEX] | [OPERATOR]                        | [FAMILY]      | [SHAPE_BOUNDARY]                                                  |
| :-----: | :-------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `map`                             | element-wise  | `MultiSet<T>→U` elementwise transform                             |
|  [02]   | `filter`                          | element-wise  | drop elements failing the predicate                               |
|  [03]   | `negate`                          | element-wise  | flip multiplicities (retraction)                                  |
|  [04]   | `concat`                          | element-wise  | union two `MultiSet` deltas                                       |
|  [05]   | `consolidate`                     | element-wise  | collapse equal elements, sum multiplicity                         |
|  [06]   | `debug`                           | element-wise  | tap the delta stream for tracing                                  |
|  [07]   | `output(fn)`                      | element-wise  | sink — `fn` sees the raw `MultiSet`, not a `Message`              |
|  [08]   | `keyBy(fn)`                       | keying        | `T → KeyValue<K, T>`                                              |
|  [09]   | `unkey()`                         | keying        | `KeyValue<K, T> → T`                                              |
|  [10]   | `rekey(fn)`                       | keying        | remap the key of a `KeyValue`                                     |
|  [11]   | `filterBy(other)`                 | keying        | semijoin against a key stream                                     |
|  [12]   | `join`                            | keyed fold    | `+ JoinType` inner/left/right/full/anti; incremental per-key join |
|  [13]   | `reduce`                          | keyed fold    | the incremental Semigroup fold per key                            |
|  [14]   | `count`                           | keyed fold    | per-key element count                                             |
|  [15]   | `distinct`                        | keyed fold    | dedupe per key; rejects negative multiplicity                     |
|  [16]   | `groupBy`                         | keyed fold    | `+ sum/count/avg/min/max/median/mode` aggregates                  |
|  [17]   | `topK`                            | ordered       | keep the top-K per key by comparator                              |
|  [18]   | `topKWithIndex`                   | ordered       | top-K with a numeric position index                               |
|  [19]   | `topKWithFractionalIndex`         | ordered       | top-K with a stable fractional index; NO re-sort                  |
|  [20]   | `orderBy`                         | ordered       | `+ WithIndex`/`WithFractionalIndex`; incremental order            |
|  [21]   | `topKWithFractionalIndexBTree`    | ordered/BTree | SEALED — barrel omits the file; not importable                    |
|  [22]   | `orderByWithFractionalIndexBTree` | ordered/BTree | SEALED — barrel omits the file; not importable                    |
|  [23]   | `loadBTree()`                     | ordered/BTree | SEALED — no subpath; unreachable                                  |

[JOIN_TYPE]: `JoinType = 'inner'|'left'|'right'|'full'|'anti'`
[GROUP_BY_OPERATORS]: `groupByOperators.sum: unknown` `groupByOperators.count: unknown` `groupByOperators.avg: unknown` `groupByOperators.min: unknown` `groupByOperators.max: unknown` `groupByOperators.median: unknown` `groupByOperators.mode: unknown`
[SURFACES]: `join(IStreamBuilder<KeyValue<K,V2>>,JoinType?) -> PipedOperator<T,KeyValue<K,[V1|null,V2|null]>>` `reduce((values:[V1,number][])=>[R,number][]) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<K,R>>` `groupBy((d:T)=>K,A?) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<string,K&AggregatesReturnType<T,A>>>` `filterBy(IStreamBuilder<KeyValue<K,unknown>>) -> PipedOperator<T,KeyValue<K,V1>>`

[SURFACES]: `topKWithFractionalIndex((a:V1,b:V1)=>number,{limit?;offset?}?) -> PipedOperator<T,KeyValue<K,[V1,string]>>` `orderByWithFractionalIndex((v:V)=>Ve,{comparator?;limit?;offset?}?) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<K,[V,string]>>`

## [03]-[INTEGRATION]

[STACK: `D2` + `effect/Data` + `Schema` (`.api/effect.md`)] — identical to the d2ts altitude: the collection element is a `Data.TaggedEnum` op decoded by a `Schema`; `map`/`filter` bodies match on `_tag` via `effect/Match`. The engine never consults `Equal`/`Hash`: keyed `consolidate` requires string/number keys and compares object VALUES by reference (WeakMap object IDs), the unkeyed lane hashes a replacer-serialized form (`bigint`/`symbol`/`Map`/`Set` stringified before murmur), and `groupBy` serializes its group key with bare `JSON.stringify` — a `bigint` group dimension throws, an `undefined` member aliases two groups — so redelivery idempotence lives in the fold's own instance combine or upstream delivery dedup, never in engine consolidation. `state` folds this in core `effect`; d2mini is the in-memory engine under it.

[STACK: `reduce`/`groupBy` + `@effect/typeclass` (`.api/effect-typeclass.md`)] — the keyed fold is a Semigroup applied incrementally; `state/merge` declares the merge as one lawful `Semigroup` and `state/fold` applies it through `reduce` at the in-memory altitude exactly as d2ts applies it at the durable one. The same reducer law is shared across both altitudes.

[STACK: two altitudes — d2mini core, `d2ts.md` durable] — `state/fold` binds ONE algebra; the browser runtime folds through d2mini (in-memory, `sendData(collection)`, no time), the node runtime folds through d2ts (durable, `sendData(version, collection)`, `Index.reconstructAt`, `./sqlite`). d2mini has no `Version`, so it carries no AsOf time-travel and no retention frontier — a fold that needs either is a d2ts fold. Never a second fold implementation; the two are one algebra at two surfaces.

[STACK: presence + `state/fold` (`.api/effect.md` `Subscribable`)] — the in-memory fold is what `state/fold` exposes as an `effect` `Subscribable`: an `output(fn)` sink drives a `SubscriptionRef`, so the browser presence view is the d2mini fold re-fired on each `run()`. `edge/live` serves that presence semantics; the `orderByWithFractionalIndex` lane keeps the live-list ordering incremental as rows churn.

## [04]-[RAIL_LAW]

- Owns: minimal, time-free incremental dataflow — a graph of `PipedOperator`s over signed `MultiSet` deltas with no version, the keyed folds (`join`/`reduce`/`count`/`distinct`/`groupBy`), the fractional-index ordered lane, and the in-memory `Index` join. Zero peer dependencies — the browser-safe in-memory altitude of the `state/fold`.
- Accept: `sendData(delta)` with signed multisets; `pipe(...)` composition of the operator roster; `reduce`/`groupBy` as the incremental application of a `@effect/typeclass` Semigroup; `output(fn)` driving a `Subscribable` for `state/fold`; `topKWithFractionalIndex`/`orderByWithFractionalIndex` for ordered views; d2mini for every browser/in-memory fold path.
- Reject: reaching for a `Version`, `Antichain`, frontier, `reconstructAt`, or durable trace here (that is the d2ts altitude); a full-collection re-sort where `topKWithFractionalIndex` maintains order; citing `loadBTree`/`*BTree` operators as importable — the catalog barrel omits their files and no subpath exists; a second in-memory fold implementation beside this one.
- Boundary: no time coordinate means no time-travel, no retention frontier, no replication — those are d2ts capabilities. `min`/`max` exist only as `groupBy` aggregates (not bare operators, unlike d2ts's `MultiSet.min`/`max`); `distinct` and those aggregates reject negative multiplicity, so retraction folds run through `reduce`/`consolidate`. Every reachable surface drains synchronously.
