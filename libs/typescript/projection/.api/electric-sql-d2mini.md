# [API_CATALOGUE] @electric-sql/d2mini

`@electric-sql/d2mini` supplies a simplified differential dataflow runtime without version or frontier tracking: `D2` (a version-free graph host), `RootStreamBuilder<T>` (input source with single-argument `sendData`), `MultiSet<T>` (a reduced multiset without join/reduce/iterate on the value level), `Index<K, V>` (a flat key-value index without version axis), and the same `pipe`-composable operator set as `@electric-sql/d2ts` minus iteration-scope operators, plus B-tree-backed `orderByWithFractionalIndexBTree` and `topKWithFractionalIndexBTree` for sort-stable fractional indexing. The projection package uses `@electric-sql/d2mini` where no frontier coordination is required — primarily for in-process view maintenance over a fully-arrived snapshot.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@electric-sql/d2mini`
- package: `@electric-sql/d2mini`
- module: `.` → `dist/index.d.ts`
- asset: `dist/index.d.ts` (re-exports `d2`, `multiset`, `operators/index`, `types`)
- rail: stream / dataflow

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph host and stream builders
- rail: stream / dataflow

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]                 | [CAPABILITY]                                            |
| :-----: | :--------------------- | :---------------------------- | :------------------------------------------------------ |
|   [1]   | `D2`                   | class                         | version-free dataflow graph host                        |
|   [2]   | `StreamBuilder<T>`     | class                         | `IStreamBuilder<T>` base implementation                 |
|   [3]   | `RootStreamBuilder<T>` | class (extends StreamBuilder) | input source with `sendData(collection)` only           |
|   [4]   | `ID2`                  | interface                     | `newInput`, `step`, `finalize` contract (no `frontier`) |
|   [5]   | `IStreamBuilder<T>`    | interface                     | `pipe(…operators)`, `writer`, `connectReader`, `graph`  |
|   [6]   | `PipedOperator<I, O>`  | type alias                    | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>`      |

[PUBLIC_TYPE_SCOPE]: collection, index, and graph edges
- rail: stream / dataflow

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                                                                                                              |
| :-----: | :-------------------------- | :------------- | :---------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `MultiSet<T>`               | class          | signed-multiplicity collection; subset of d2ts `MultiSet` — no `join`/`reduce`/`iterate`/`count`/`min`/`max`/`distinct`/`sum` on instance |
|   [2]   | `MultiSetArray<T>`          | type alias     | `[T, number][]`                                                                                                                           |
|   [3]   | `KeyedData<T>`              | type alias     | `[key: string, value: T]`                                                                                                                 |
|   [4]   | `KeyValue<K, V>`            | type alias     | `[K, V]`                                                                                                                                  |
|   [5]   | `Index<K, V>`               | class          | flat key-value index; `get`, `join`, `addValue`, `append`, `entries`, `keys`, `has`, `size` — no version axis                             |
|   [6]   | `DifferenceStreamReader<T>` | class          | read handle; `drain()` → `MultiSet<T>[]`, `isEmpty()`                                                                                     |
|   [7]   | `DifferenceStreamWriter<T>` | class          | write handle; `sendData(collection)`, `newReader()`                                                                                       |
|   [8]   | `Operator<T>`               | abstract class | base operator (no frontier fields)                                                                                                        |
|   [9]   | `UnaryOperator<Tin, Tout>`  | abstract class | single-input operator; `inputMessages()` → `MultiSet<Tin>[]`                                                                              |
|  [10]   | `BinaryOperator<T>`         | abstract class | two-input operator                                                                                                                        |
|  [11]   | `LinearUnaryOperator<T, U>` | abstract class | stateless linear operator base with `inner(collection)`                                                                                   |

[PUBLIC_TYPE_SCOPE]: operator interfaces
- rail: stream / dataflow

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `IOperator<T>`               | interface     | `run()`, `hasPendingWork()` (no `frontiers()`) |
|   [2]   | `IDifferenceStreamReader<T>` | interface     | `drain()` → `MultiSet<T>[]`, `isEmpty()`       |
|   [3]   | `IDifferenceStreamWriter<T>` | interface     | `sendData(collection)`, `newReader()`          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph lifecycle
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :--------------------------------------- | :------------- | :----------------------------------- |
|   [1]   | `new D2()`                               | constructor    | creates version-free dataflow graph  |
|   [2]   | `D2#newInput<T>()`                       | factory        | returns `RootStreamBuilder<T>`       |
|   [3]   | `D2#step()`                              | execution      | runs one incremental step            |
|   [4]   | `D2#run()`                               | execution      | runs to quiescence                   |
|   [5]   | `D2#finalize()`                          | lifecycle      | signals graph completion             |
|   [6]   | `RootStreamBuilder#sendData(collection)` | data ingestion | sends `MultiSet<T>` (no version arg) |

[ENTRYPOINT_SCOPE]: piped operators — transforms and keyed aggregations
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                                        | [SIGNATURE / PRODUCES]                                        | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------ | :------------------------------- |
|   [1]   | `map<T, O>(f)`                                                   | `PipedOperator<T, O>`                                         | element-wise transform           |
|   [2]   | `filter<T>(f)`                                                   | `PipedOperator<T, T>`                                         | predicate filter                 |
|   [3]   | `negate<T>()`                                                    | `PipedOperator<T, T>`                                         | negate multiplicities            |
|   [4]   | `concat<T, T2>(other)`                                           | `PipedOperator<T, T\|T2>`                                     | stream union                     |
|   [5]   | `consolidate<T>()`                                               | `PipedOperator<T, T>`                                         | merge multiplicity cancellations |
|   [6]   | `output<T>(fn)`                                                  | `PipedOperator<T, T>`                                         | side-effecting observer          |
|   [7]   | `keyBy<T, K>(keyFn)`                                             | `PipedOperator<T, Keyed<K, T>>`                               | add key to stream elements       |
|   [8]   | `unkey<K, V>()`                                                  | `PipedOperator<Keyed<K,V>, V>`                                | strip key from keyed stream      |
|   [9]   | `rekey<K1, K2, V>(keyFn)`                                        | `PipedOperator<Keyed<K1,V>, Keyed<K2,V>>`                     | rekey by new function            |
|  [10]   | `join<K, V1, V2>(other, type?)`                                  | `PipedOperator<T, KeyValue<K,[V1\|null,V2\|null]>>`           | inner/left/right/full/anti join  |
|  [11]   | `innerJoin` / `leftJoin` / `rightJoin` / `antiJoin` / `fullJoin` | join shorthand variants                                       | typed join aliases               |
|  [12]   | `reduce<K, V1, R>(f)`                                            | `(stream) => IStreamBuilder<KeyValue<K,R>>`                   | keyed reduce                     |
|  [13]   | `count()`                                                        | `(stream) => IStreamBuilder<KeyValue<K,number>>`              | per-key count                    |
|  [14]   | `distinct()`                                                     | `(stream) => IStreamBuilder<KeyValue<K,V>>`                   | deduplicate by key               |
|  [15]   | `topK<K, V1>(comparator, options?)`                              | `PipedOperator<T, T>`                                         | per-key ranked limit             |
|  [16]   | `orderBy<T>(valueExtractor, options?)`                           | `(stream) => IStreamBuilder<T>`                               | ordered limit within key group   |
|  [17]   | `groupBy<T, K, A>(keyExtractor, aggregates)`                     | `(stream) => IStreamBuilder<KeyValue<string, K & AggResult>>` | multi-aggregate                  |

[ENTRYPOINT_SCOPE]: B-tree fractional index operators (d2mini-only)
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                                      | [PRODUCES]                                             | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------- | :----------------------------------------------------- | :--------------------------------------- |
|   [1]   | `topKWithFractionalIndexBTree<K, V1>(comparator, options?)`    | `PipedOperator<T, KeyValue<K, [V1, string]>>`          | B-tree-backed fractional-indexed topK    |
|   [2]   | `orderByWithFractionalIndexBTree<T>(valueExtractor, options?)` | `(stream) => IStreamBuilder<KeyValue<K, [V, string]>>` | B-tree-backed fractional-indexed orderBy |
|   [3]   | `loadBTree()`                                                  | `Promise<void>`                                        | loads B-tree module before first use     |

## [4]-[IMPLEMENTATION_LAW]

[DATAFLOW_TOPOLOGY]:
- `D2` has no `frontier` or `pushFrontier`/`popFrontier` — it operates over a single implicit time step per `run()` call; there is no partial-order versioning
- `RootStreamBuilder#sendData(collection)` takes only the collection — no `version` argument; this is the key API difference from `@electric-sql/d2ts`
- `Index<K, V>` is flat (no version axis): `get(key)` returns `[V, number][]` directly, and `join` produces `MultiSet<[K, [V, V2]]>` rather than a version-indexed result
- `MultiSet<T>` in d2mini has a reduced instance method set: `map`, `filter`, `negate`, `concat`, `consolidate`, `extend`, `getInner` — the `join`, `reduce`, `count`, `sum`, `min`, `max`, `distinct`, `iterate` methods of d2ts `MultiSet` are absent because the operator graph owns that logic
- `loadBTree()` must be awaited once before using `topKWithFractionalIndexBTree` or `orderByWithFractionalIndexBTree`; these use a lazy-loaded B-tree that is not bundled synchronously

[LOCAL_ADMISSION]:
- Use `@electric-sql/d2mini` when all inputs arrive before the graph runs and no frontier coordination is needed; use `@electric-sql/d2ts` when streaming inputs require version-aware completion signaling
- The `pipe` API and operator set are interface-compatible with `@electric-sql/d2ts`; operators written as `PipedOperator<I, O>` compose identically across both packages
- `consolidate()` remains a prerequisite before `output()` and keyed operators when multiplicities may cancel within a batch

[RAIL_LAW]:
- Package: `@electric-sql/d2mini`
- Owns: version-free differential dataflow graph execution and incremental operator computation over snapshot-style inputs
- Accept: `MultiSet<T>` batch inputs via `RootStreamBuilder#sendData`; piped `PipedOperator<I, O>` chains for view computation
- Reject: calling `sendFrontier` or passing `Version`/`Antichain` arguments — d2mini has no frontier protocol; use `@electric-sql/d2ts` when those are required
