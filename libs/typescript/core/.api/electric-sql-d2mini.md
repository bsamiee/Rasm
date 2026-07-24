# [TS_CORE_API_ELECTRIC_SQL_D2MINI]

[PACKAGE_SURFACE]:
- package: `@electric-sql/d2mini` (Apache-2.0)
- module: ESM only (`type: module`); single `.` barrel, no subpaths.
- asset: `dist/index.d.ts`; bundles `fractional-indexing`, `murmurhash-js`, `sorted-btree`.
- runtime: pure-TS in-process dataflow under node, bun, browser, worker; zero peer dependencies.
- ABI: fully synchronous scheduler — `run()`/`step()` drain on the calling thread; every barrel-reachable operator is sync.
- plane: `plane:runtime` (W1); folder-local to `state`.
- rail: incremental-dataflow / fold-maintenance.

`@electric-sql/d2mini` maintains a time-free incremental fold in memory: a dataflow graph of one-shape `PipedOperator`s over signed `MultiSet` deltas, pushed by `sendData(collection)` and drained synchronously by `run()`. Zero peer dependencies make it the browser-safe altitude of `state/fold` — no durability, time coordinate, or replication.

## [01]-[GRAPH_AND_DELTA]

`D2` builds the graph, `pipe` wires it, `run`/`step` drive it synchronously; the delta is a signed `MultiSet` with no version coordinate, and `output` receives it directly.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY_BOUNDARY]                                                           |
| :-----: | :------------------------------ | :------------- | :------------------------------------------------------------------------------ |
|  [01]   | `D2` (no options)               | class          | the graph; `newInput()` mints a `RootStreamBuilder`, `run`/`step`/`finalize`    |
|  [02]   | `RootStreamBuilder<T>`          | class          | input handle — `sendData(collection)` is the sole push; no version coordinate   |
|  [03]   | `StreamBuilder<T>`              | class          | pipeline node (`IStreamBuilder`); 1..20-arity `pipe(...)` over `PipedOperator`s |
|  [04]   | `MultiSet<T>` (`MultiSetArray`) | class          | elementwise `map`/`filter`/`negate`/`concat`/`consolidate`/`extend`/`getInner`  |
|  [05]   | `Index<K, V>`                   | class          | in-memory keyed trace — `get`/`getMultiplicity`/`join`; no `reconstructAt`      |
|  [06]   | `KeyValue<K, V>` = `[K, V]`     | tuple alias    | the keyed-record shape the keyed operators require                              |
|  [07]   | `PipedOperator<I, O>`           | function alias | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>` — the ONE operator shape     |

[D2]: `D2()` `D2.newInput() -> RootStreamBuilder<T>` `D2.step() -> void` `D2.run() -> void` `D2.finalize() -> void`
[ROOT_STREAM_BUILDER]: `RootStreamBuilder.sendData(MultiSet<T>|MultiSetArray<T>) -> void`
[MULTI_SET]: `MultiSet(MultiSetArray<T>?)` `MultiSet.map((d:T)=>U) -> MultiSet<U>` `MultiSet.filter((d:T)=>boolean) -> MultiSet<T>` `MultiSet.negate() -> MultiSet<T>` `MultiSet.concat(MultiSet<T>) -> MultiSet<T>` `MultiSet.consolidate() -> MultiSet<T>` `MultiSet.extend(MultiSet<T>|MultiSetArray<T>) -> void` `MultiSet.getInner() -> MultiSetArray<T>`
[INDEX]: `Index.get(K) -> [V,number][]` `Index.getMultiplicity(K,V) -> number` `Index.addValue(K,[V,number]) -> void` `Index.append(Index<K,V>) -> void` `Index.join(Index<K,V2>) -> MultiSet<[K,[V,V2]]>` `Index.entries() -> MapIterator<[K,Map<V,number>]>` `Index.keys() -> MapIterator<K>` `Index.has(K) -> boolean` `Index.size: number`
[SURFACES]: `output((data:MultiSet<T>)=>void) -> PipedOperator<T,T>`

## [02]-[OPERATOR_ALGEBRA]

Every operator is one `PipedOperator<I, O>` composed by `pipe`; the roster is seed data on that shape, and the keyed folds and fractional-index ordered lane maintain their results incrementally.

| [INDEX] | [OPERATOR]                | [FAMILY]     | [SHAPE_BOUNDARY]                                                  |
| :-----: | :------------------------ | :----------- | :---------------------------------------------------------------- |
|  [01]   | `map`                     | element-wise | `MultiSet<T>→U` elementwise transform                             |
|  [02]   | `filter`                  | element-wise | drop elements failing the predicate                               |
|  [03]   | `negate`                  | element-wise | flip multiplicities (retraction)                                  |
|  [04]   | `concat`                  | element-wise | union two `MultiSet` deltas                                       |
|  [05]   | `consolidate`             | element-wise | collapse equal elements, sum multiplicity                         |
|  [06]   | `debug`                   | element-wise | tap the delta stream for tracing                                  |
|  [07]   | `output(fn)`              | element-wise | sink — `fn` sees the raw `MultiSet`                               |
|  [08]   | `keyBy(fn)`               | keying       | `T → KeyValue<K, T>`                                              |
|  [09]   | `unkey()`                 | keying       | `KeyValue<K, T> → T`                                              |
|  [10]   | `rekey(fn)`               | keying       | remap the key of a `KeyValue`                                     |
|  [11]   | `filterBy(other)`         | keying       | semijoin against a key stream                                     |
|  [12]   | `join`                    | keyed fold   | `+ JoinType` inner/left/right/full/anti; incremental per-key join |
|  [13]   | `reduce`                  | keyed fold   | the incremental Semigroup fold per key                            |
|  [14]   | `count`                   | keyed fold   | per-key element count                                             |
|  [15]   | `distinct`                | keyed fold   | dedupe per key; rejects negative multiplicity                     |
|  [16]   | `groupBy`                 | keyed fold   | `+ sum/count/avg/min/max/median/mode` aggregates                  |
|  [17]   | `topK`                    | ordered      | keep the top-K per key by comparator                              |
|  [18]   | `topKWithIndex`           | ordered      | top-K with a numeric position index                               |
|  [19]   | `topKWithFractionalIndex` | ordered      | top-K with a stable fractional index; no re-sort                  |
|  [20]   | `orderBy`                 | ordered      | `+ WithIndex`/`WithFractionalIndex`; incremental order            |

[JOIN_TYPE]: `JoinType = 'inner'|'left'|'right'|'full'|'anti'`
[GROUP_BY_OPERATORS]: `groupByOperators.sum: unknown` `groupByOperators.count: unknown` `groupByOperators.avg: unknown` `groupByOperators.min: unknown` `groupByOperators.max: unknown` `groupByOperators.median: unknown` `groupByOperators.mode: unknown`
[SURFACES]: `join(IStreamBuilder<KeyValue<K,V2>>,JoinType?) -> PipedOperator<T,KeyValue<K,[V1|null,V2|null]>>` `reduce((values:[V1,number][])=>[R,number][]) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<K,R>>` `groupBy((d:T)=>K,A?) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<string,K&AggregatesReturnType<T,A>>>` `filterBy(IStreamBuilder<KeyValue<K,unknown>>) -> PipedOperator<T,KeyValue<K,V1>>`
[SURFACES]: `topKWithFractionalIndex((a:V1,b:V1)=>number,{limit?;offset?}?) -> PipedOperator<T,KeyValue<K,[V1,string]>>` `orderByWithFractionalIndex((v:V)=>Ve,{comparator?;limit?;offset?}?) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<K,[V,string]>>`

## [03]-[INTEGRATION]

[STACK: `D2` + `effect/Data` + `Schema` (`.api/effect.md`)] — the collection element is a `Data.TaggedEnum` op decoded by a `Schema`, and `map`/`filter` bodies match `_tag` through `effect/Match`. Keyed `consolidate` ignores `Equal`/`Hash`: it demands string/number keys and compares object values by reference (WeakMap IDs), the unkeyed lane murmur-hashes a replacer form (`bigint`/`symbol`/`Map`/`Set` stringified first), and `groupBy` keys on bare `JSON.stringify` — a `bigint` dimension throws and an `undefined` member aliases two groups. Idempotence rides the fold's combine or upstream dedup, never engine consolidation.

[STACK: `reduce`/`groupBy` + `@effect/typeclass` (`.api/effect-typeclass.md`)] — the keyed fold applies a lawful `Semigroup` incrementally: `state/merge` declares the merge `Semigroup`, `state/fold` applies it through `reduce`, so the reducer law and the merge law are one.

[STACK: `@electric-sql/d2ts`(`.api/electric-sql-d2ts.md`) durable counterpart] — `state/fold` binds one algebra at two altitudes: d2mini folds in memory (`sendData(collection)`, no time coordinate), d2ts folds durably (`sendData(version, collection)`, `Index.reconstructAt`, `./sqlite`).

[STACK: presence + `state/fold` (`.api/effect.md` `Subscribable`)] — an `output(fn)` sink drives a `SubscriptionRef`, so `state/fold` exposes the in-memory fold as an `effect` `Subscribable` re-fired each `run()`; `edge/live` serves that presence view, and `orderByWithFractionalIndex` keeps its live-list order incremental as rows churn.

## [04]-[RAIL_LAW]

- Owns: minimal time-free incremental dataflow — a graph of `PipedOperator`s over signed `MultiSet` deltas with no version, the keyed folds, the fractional-index ordered lane, and the in-memory `Index` join; the browser-safe altitude of `state/fold`.
- Accept: `sendData(delta)` of signed multisets, `pipe(...)` composition of the operator roster, `reduce`/`groupBy` as incremental `@effect/typeclass` `Semigroup` application, `output(fn)` driving a `Subscribable`, and `topKWithFractionalIndex`/`orderByWithFractionalIndex` for ordered views.
- Reject: reaching for a `Version`, `Antichain`, frontier, `reconstructAt`, or durable trace here — that is the d2ts altitude; a full-collection re-sort where `topKWithFractionalIndex` maintains order; citing `loadBTree`/`*BTree` operators as importable, since the `.`-only barrel omits their files and no subpath exists; a second in-memory fold implementation beside this one.
- Boundary: no time coordinate means no time-travel, retention frontier, or replication — those are d2ts capabilities. `min`/`max` exist only as `groupBy` aggregates, and `distinct` rejects negative multiplicity, so retraction folds route through `reduce`/`consolidate`. Every reachable surface drains synchronously.
