# [TS_CORE_API_ELECTRIC_SQL_D2TS]

[PACKAGE_SURFACE]:
- package: `@electric-sql/d2ts` (Apache-2.0)
- module: ESM only (`type: module`); `.` barrel + two peer-gated subpaths — `./sqlite` (durable operator state), `./electric` (live-replication binding).
- asset: `dist/index.d.ts`; the `.` core bundles `fractional-indexing` + `murmurhash-js`, peer-free.
- runtime: pure-TS in-process dataflow under node, bun, browser, worker; `.` core browser-safe, `./sqlite` needs the `better-sqlite3` peer (node), `./electric` the `@electric-sql/client` peer (Postgres replication) — neither peer crosses `state`'s transport-free import surface.
- ABI: synchronous fixpoint scheduler — `run()`/`step()` drain the operator queue on the calling thread; every barrel-reachable operator is sync.
- plane: `plane:runtime` (W1); folder-local to `state`.
- rail: incremental-dataflow / fold-maintenance.

`@electric-sql/d2ts` maintains a fold as an incremental dataflow graph over signed `MultiSet` deltas at partial-order `Version`s: a delta at a new version threads the operator graph without re-folding the prefix. It is the durable, versioned altitude of one fold algebra, adding the `Version`/`Antichain` frontier and the `Index`/`./sqlite` persistent trace the in-memory altitude omits.

## [01]-[GRAPH_AND_TIME]

`Version` is a partial order — per-replica logical clocks fold under one engine, no global sequence assumed.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY_BOUNDARY]                                                               |
| :-----: | :------------------------------ | :------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `D2` (`D2Options`)              | class          | dataflow graph; `newInput()` mints a `RootStreamBuilder`, `run`/`step` drive it     |
|  [02]   | `RootStreamBuilder<T>`          | class          | input handle — `sendData(version, collection)` + `sendFrontier(frontier)`           |
|  [03]   | `StreamBuilder<T>`              | class          | pipeline node (`IStreamBuilder`); 1..20-arity `pipe(...)` composes `PipedOperator`s |
|  [04]   | `Version` (`v()` factory)       | class          | partially-ordered logical time — `join`/`meet`/`lessThan`/`advanceBy`/`extend`      |
|  [05]   | `Antichain` / `Frontier`        | class          | minimal incomparable version set — `meet`/`lessEqualVersion`; the frontier bound    |
|  [06]   | `MultiSet<T>` (`MultiSetArray`) | class          | signed multiset `[value, multiplicity][]` — delta unit; negative = retraction       |
|  [07]   | `Message<T>` `DATA` arm         | tagged union   | `data.version` + `data.collection` — the delta payload of `output`                  |
|  [08]   | `MessageType` `FRONTIER` arm    | tagged union   | `data`: version\|antichain — the frontier-close payload of `output`                 |
|  [09]   | `KeyValue<K, V>` = `[K, V]`     | tuple alias    | keyed-record shape every keyed operator (`join`/`reduce`/`groupBy`) requires        |
|  [10]   | `PipedOperator<I, O>`           | function alias | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>` — the ONE operator shape         |

[D2]: `D2({initialFrontier:Antichain|Version|number|number[]})` `D2.newInput() -> RootStreamBuilder<T>` `D2.step() -> void` `D2.run() -> void` `D2.finalize() -> void` `D2.pushFrontier(Antichain) -> void` `D2.popFrontier() -> void` `D2.frontier() -> Antichain`
[ROOT_STREAM_BUILDER]: `RootStreamBuilder.sendData(Version|number|number[],MultiSet<T>|MultiSetArray<T>) -> void` `RootStreamBuilder.sendFrontier(Antichain|Version|number|number[]) -> void`
[VERSION]: `Version.lessThan(Version) -> boolean` `Version.lessEqual(Version) -> boolean` `Version.equals(Version) -> boolean` `Version.join(Version) -> Version` `Version.meet(Version) -> Version` `Version.advanceBy(Antichain) -> Version`
[ANTICHAIN]: `Antichain(Version[])` `Antichain.meet(Antichain) -> Antichain` `Antichain.lessEqualVersion(Version) -> boolean`
[SURFACES]: `v(number|number[]) -> Version`

[MULTI_SET]: `MultiSet(MultiSetArray<T>?)` `MultiSet.map((d:T)=>U) -> MultiSet<U>` `MultiSet.filter((d:T)=>boolean) -> MultiSet<T>` `MultiSet.negate() -> MultiSet<T>` `MultiSet.concat(MultiSet<T>) -> MultiSet<T>` `MultiSet.consolidate() -> MultiSet<T>` `MultiSet.join(MultiSet<KeyedData<U>>) -> MultiSet<KeyedData<[T,U]>>` `MultiSet.reduce((vals:[T,number][])=>[U,number][]) -> MultiSet<KeyedData<U>>` `MultiSet.count() -> MultiSet<KeyedData<number>>` `MultiSet.sum() -> MultiSet<KeyedData<number>>` `MultiSet.distinct() -> MultiSet<KeyedData<T>>` `MultiSet.min() -> MultiSet<KeyedData<T>>` `MultiSet.max() -> MultiSet<KeyedData<T>>` `MultiSet.iterate((c:MultiSet<T>)=>MultiSet<T>) -> MultiSet<T>` `MultiSet.extend(MultiSet<T>|MultiSetArray<T>) -> void` `MultiSet.getInner() -> MultiSetArray<T>`

## [02]-[OPERATOR_ALGEBRA]

`pipe` composes one parameterized shape — `PipedOperator<I, O>` — left to right; a new dataflow verb is a new `PipedOperator`, never a new graph type.

| [INDEX] | [FAMILY]     | [OPERATORS]                                                                                    |
| :-----: | :----------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | element-wise | `map` `filter` `negate` `concat` `consolidate` `buffer` `debug` `output`                       |
|  [02]   | keying       | `keyBy(fn)` `unkey()` `rekey(fn)`                                                              |
|  [03]   | keyed fold   | `join`(+`inner`/`left`/`right`/`full`/`anti`) `reduce` `count` `distinct` `groupBy`            |
|  [04]   | ordered      | `topK` `topKWithIndex` `topKWithFractionalIndex` `orderBy`(+`WithIndex`/`WithFractionalIndex`) |
|  [05]   | recursive    | `iterate(f)` (`IngressOperator`/`EgressOperator`/`FeedbackOperator`)                           |

- [01]-[ELEMENT_WISE]: `PipedOperator<T, …>` — no key structure; `output` sees `Message`.
- [02]-[KEYING]: `T ⇄ KeyValue<K, T>` — the adapter into and out of the keyed family.
- [03]-[KEYED_FOLD]: `KeyValue<K, V>` in — a Semigroup applied per key.
- [04]-[ORDERED]: keyed; the fractional index maintains order without a full re-sort.
- [05]-[RECURSIVE]: fixpoint scope looping the sub-graph to convergence.

[COMPOSITION]: `input.pipe(map, filter, keyBy, reduce, consolidate, output)`

[SURFACES]: `JoinType` `join` `innerJoin` `reduce` `groupBy` `groupByOperators`

[SURFACES]: `orderByWithFractionalIndex((v:V)=>Ve,{comparator?;limit?;offset?}?) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<K,[V,string]>>` `topKWithFractionalIndex((a:V1,b:V1)=>number,{limit?;offset?}?) -> PipedOperator<T,KeyValue<K,[V1,string]>>` `iterate((s:IStreamBuilder<T>)=>IStreamBuilder<T>) -> (s:IStreamBuilder<T>)=>IStreamBuilder<T>`

## [03]-[INDEX_AND_PERSISTENCE]

`Index<K, V>` is the versioned trace every keyed operator keeps — a key→versions→signed-values map `reconstructAt` reads at any version and `compact` collapses below a stability frontier.

| [INDEX] | [SURFACE]                                              | [PRODUCES]                   |
| :-----: | :----------------------------------------------------- | :--------------------------- |
|  [01]   | `new Index<K, V>()`                                    | empty versioned trace        |
|  [02]   | `.addValue(key, version, [value, multiplicity])`       | in-place                     |
|  [03]   | `.reconstructAt(key, version)`                         | `[V, number][]`              |
|  [04]   | `.compact(compactionFrontier: Antichain, keys?)`       | in-place                     |
|  [05]   | `.keys()`/`.entries()`/`.versions(key)`/`.join(other)` | `K[]`/rows/`Version[]`/diffs |
|  [06]   | `./sqlite` `SQLiteDb`/`BetterSQLite3Wrapper`           | durable operator state       |
|  [07]   | `./electric` `electricStreamToD2Input(opts)`           | `RootStreamBuilder`          |
|  [08]   | `./electric` `outputElectricMessages(fn)`              | `PipedOperator`              |

[SQLITE_DB]: `SQLiteDb.exec(string) -> void` `SQLiteDb.prepare(string) -> SQLiteStatement<P,R>`
[BETTER_SQLITE3_WRAPPER]: `BetterSQLite3Wrapper(import('better-sqlite3').Database)` `BetterSQLite3Wrapper.close() -> void`
[SURFACES]: `electricStreamToD2Input({stream,input,lsnToVersion?}) -> RootStreamBuilder<[key:string,T]>` `outputElectricMessages((ChangeMessage<Row>[])=>void) -> PipedOperator<T,KeyValue<K,V>>`

## [04]-[INTEGRATION]

[STACK: `D2` + `effect/Data` + `Schema` (`.api/effect.md`)] — `state/fold` folds `Data.TaggedEnum` ops a `Schema` decodes; `map`/`filter` match `_tag` via `effect/Match`. `consolidate`/`distinct` hash a replacer form (`bigint`/`symbol`/`Map`/`Set` stringified first), never consulting `Equal`/`Hash`, so idempotence lives in the fold's combine or upstream dedup, not the collapse.

[STACK: `reduce`/`groupBy` + `@effect/typeclass` (`.api/effect-typeclass.md`)] — a keyed fold applies a Semigroup incrementally: the `reduce` reducer `(vals) => [R, number][]` is `Semigroup.combineMany` over signed multiplicities, and `groupByOperators.{sum,min,max}` specialize `Number.Monoid{Sum,Min,Max}` to signed pairs. `state/merge` declares the merge Semigroup; `state/fold` applies it through `reduce`, so reducer law and merge law are one.

[STACK: `@electric-sql/d2mini`(`.api/electric-sql-d2mini.md`)] — d2mini serves this operator algebra minus time (`sendData(collection)`, no `Version`); d2ts is the durable altitude (`sendData(version, collection)`, `Version`/`Antichain`, `Index.reconstructAt`, `./sqlite`). `state/fold` binds one algebra and the runtime picks the altitude — browser in-memory or node durable through `store/project`.

[STACK: REPLAY_LAW + `state/causal` frontiers] — `Version.meet` is the stability-frontier GLB `state/causal` computes, `Antichain` the honest-uncertainty frontier over `value/clock` windows, `iterate` the happened-before transitive fold, and `Index.compact(frontier)` the retention handoff to `store/journal`. A fold rebuilt from any event prefix replays through the same graph to the live version — the convergence `state/merge` asserts, checked as `@effect/vitest`(`tests/typescript/.api/effect-vitest.md`) `it.prop` laws over the `tests/contracts` fixtures.

## [05]-[RAIL_LAW]

- Owns: versioned incremental dataflow — a `PipedOperator` graph over signed `MultiSet` deltas at partial-order `Version`s, the `Index` trace with `reconstructAt`/`compact`, and the `./sqlite`/`./electric` durable and replication bindings.
- Accept: `sendData(version, delta)` of signed multisets; `pipe(...)` composition of the operator roster; `reduce`/`groupBy` as incremental `@effect/typeclass` Semigroup application; `Index.reconstructAt` for AsOf reads; `iterate` for transitive folds; `./sqlite` only at the durable node altitude.
- Reject: re-folding or re-sorting a whole collection on change; a second fold implementation per runtime; importing `./sqlite`/`./electric` or their `better-sqlite3`/`@electric-sql/client` peers into `state`'s transport-free browser core; treating `Version` as a total order over a partial multi-replica domain.
- Boundary: the `.` core is browser-safe and peer-free; the durable and replication subpaths are node-only peer-gated bindings. `Version` interning via `v()` is required for map-key identity. `min`/`max` reject negative multiplicity — retraction extremum folds route through `reduce`/`consolidate`; `distinct` consolidates signed sums per key and emits only positive-count survivors, so retraction through it is lawful.
