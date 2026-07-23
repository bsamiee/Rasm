# [TS_CORE_API_ELECTRIC_SQL_D2TS]

[PACKAGE_SURFACE]:
- package: `@electric-sql/d2ts` (Apache-2.0)
- module: ESM only (`type: module`, no CJS mirror); `.` barrel plus two peer-gated subpaths — `./sqlite` (durable operator state) and `./electric` (live-replication binding).
- asset: `dist/index.d.ts` (barrel over `d catalog` / `order` / `multiset` / `version-index` / `operators` / `types`); the `.` core bundles `fractional-indexing` + `murmurhash-js` and has NO peer requirement.
- runtime: pure-TS in-process dataflow; runs in node / bun / browser / worker. The `.` core is browser-safe; `./sqlite` requires the `better-sqlite catalog` peer (node durable altitude) and `./electric` the `@electric-sql/client` peer (Postgres replication) — neither peer crosses into `state`'s transport-free import surface.
- ABI: synchronous fixpoint scheduler — `graph.run()` / `graph.step()` drain the operator queue on the calling thread; every barrel-reachable operator is sync.
- plane: `plane:runtime` (W1); folder-local to `state`, catalogued here.
- rail: incremental-dataflow / fold-maintenance.

`@electric-sql/d2ts` is the incremental engine the `state/fold` REPLAY_LAW rides: a fold is a dataflow graph over signed multisets at partially-ordered versions, and a delta pushed at a new version is maintained through the operator graph WITHOUT re-folding the prefix — the "array re-sort recovery" the design names as a defect is exactly the whole-collection recompute this package deletes. The `Version`/`Antichain` partial order IS the AsOf three-coordinate time the `state/fold` watermarks and `state/causal` frontiers are expressed in; `Index.reconstructAt(key, version)` is the time-travel read; `iterate` is the fixpoint the transitive folds (causal reachability, Merkle closure) run under. It is the durable-altitude counterpart to `d2mini.md`'s browser-in-memory core: same operator algebra, d2ts adds the versioned frontier and the `./sqlite` persistent trace so a node fold survives restart. A merge folded through `reduce`/`groupBy` is a `@effect/typeclass` Semigroup applied incrementally — the reducer law and the merge law are the same law at two speeds.

## [01]-[GRAPH_AND_TIME]

The graph is constructed once, wired with `pipe`, then driven; time is a partial order so multi-dimensional versions (per-replica logical clocks) fold under one engine.

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

The operator library is ONE parameterized shape — `PipedOperator<I, O>` composed left-to-right by `pipe` — not a fixed method wall. The roster below is SEED DATA on that shape: a new dataflow verb is a new `PipedOperator`, never a new graph type. Four families vary by what structure they exploit — element-wise, keyed-fold, ordered, and recursive.

| [INDEX] | [FAMILY]     | [OPERATORS]                                                                                    |
| :-----: | :----------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | element-wise | `map` `filter` `negate` `concat` `consolidate` `buffer` `debug` `output`                       |
|  [02]   | keying       | `keyBy(fn)` `unkey()` `rekey(fn)`                                                              |
|  [03]   | keyed fold   | `join`(+`inner`/`left`/`right`/`full`/`anti`) `reduce` `count` `distinct` `groupBy`            |
|  [04]   | ordered      | `topK` `topKWithIndex` `topKWithFractionalIndex` `orderBy`(+`WithIndex`/`WithFractionalIndex`) |
|  [05]   | recursive    | `iterate(f)` (`IngressOperator`/`EgressOperator`/`FeedbackOperator`)                           |

- [01]-[ELEMENT_WISE]: `PipedOperator<T, …>` — no key structure; `output` sees `Message`.
- [02]-[KEYING]: `T ⇄ KeyValue<K, T>` — the adapter into and out of the keyed family.
- [03]-[KEYED_FOLD]: `KeyValue<K, V>` in; the incremental fold — a Semigroup applied per key.
- [04]-[ORDERED]: keyed; the fractional index maintains order WITHOUT a full re-sort.
- [05]-[RECURSIVE]: fixpoint scope — loops the sub-graph to convergence.

[COMPOSITION]: `input.pipe(map, filter, keyBy, reduce, consolidate, output)`

[SURFACES]: `JoinType` `join` `innerJoin` `reduce` `groupBy` `groupByOperators`

[SURFACES]: `orderByWithFractionalIndex((v:V)=>Ve,{comparator?;limit?;offset?}?) -> (s:IStreamBuilder<T>)=>IStreamBuilder<KeyValue<K,[V,string]>>` `topKWithFractionalIndex((a:V1,b:V1)=>number,{limit?;offset?}?) -> PipedOperator<T,KeyValue<K,[V1,string]>>` `iterate((s:IStreamBuilder<T>)=>IStreamBuilder<T>) -> (s:IStreamBuilder<T>)=>IStreamBuilder<T>`

## [03]-[INDEX_AND_PERSISTENCE]

`Index<K, V>` is the versioned trace the keyed operators keep — a key → versions → signed values map that `reconstructAt` reads at any requested version and `compact` collapses below a stability frontier. It is the mechanism `reconstructAt` gives `state/fold` AsOf reads and `state/causal` retention-frontier handoff. Rows [02]-[05] are `Index<K, V>.` receiver methods; the `./sqlite`/`./electric` subpath rows carry the peers named in the surface lead.

| [INDEX] | [SURFACE]                                              | [PRODUCES]                   |
| :-----: | :----------------------------------------------------- | :--------------------------- |
|  [01]   | `new Index<K, V>()`                                    | empty versioned trace        |
|  [02]   | `.addValue(key, version, [value, multiplicity])`       | in-place                     |
|  [03]   | `.reconstructAt(key, version)`                         | `[V, number][]`              |
|  [04]   | `.compact(compactionFrontier: Antichain, keys?)`       | in-place                     |
|  [05]   | `.keys()`/`.entries()`/`.versions(key)`/`.join(other)` | `K[]`/rows/`Version[]`/diffs |
|  [06]   | `./sqlite` `SQLiteDb`/`BetterSQLite catalogWrapper`    | durable operator state       |
|  [07]   | `./electric` `electricStreamToD2Input(opts)`           | `RootStreamBuilder`          |
|  [08]   | `./electric` `outputElectricMessages(fn)`              | `PipedOperator`              |

- [01]-[NEW]: default-constructible — an owned trace an `output` sink appends.
- [02]-[ADD_VALUE]: the trace append — one signed row at a partial-order version.
- [03]-[RECONSTRUCT_AT]: time-travel read — the AsOf materialization at a partial-order version.
- [04]-[COMPACT]: collapse the trace below the stability frontier — the retention handoff.
- [05]-[WALK]: key census, entry walk, per-key version set, and incremental join.
- [06]-[SQLITE]: the persistent trace — a node fold survives restart.
- [07]-[ELECTRIC_IN]: binds a Postgres `ShapeStream` as graph input; `lsnToVersion` maps LSN→`Version`.
- [08]-[ELECTRIC_OUT]: emits `ChangeMessage<Row>[]` — the replication-shaped egress.

[SQLITE_DB]: `SQLiteDb.exec(string) -> void` `SQLiteDb.prepare(string) -> SQLiteStatement<P,R>`
[BETTER_SQLITE3_WRAPPER]: `BetterSQLite3Wrapper(import('better-sqlite3').Database)` `BetterSQLite3Wrapper.close() -> void`
[SURFACES]: `electricStreamToD2Input({…}) -> RootStreamBuilder<[key:string,T]>`

## [04]-[INTEGRATION]

[STACK: `D2` + `effect/Data` + `Schema` (`.api/effect.md`)] — the collection element is not a bare object: `state/fold` folds `Data.TaggedEnum` op values (the CRDT/journal op vocabulary) decoded by a `Schema`. `map`/`filter` bodies pattern-match with `effect/Match` on the op `_tag`; `consolidate` and `distinct` hash a replacer-serialized form of each value (`bigint`/`symbol`/`Map`/`Set` stringified before murmur) — structural over same-schema decoded ops whose field order is declaration-stable, while `Equal`/`Hash` are never consulted — so redelivery idempotence lives in the fold's own instance combine or upstream delivery dedup, not in the serialization collapse. `state` expresses the graph in core `effect` only; d2ts adds the incremental engine under it.

[STACK: `reduce`/`groupBy` + `@effect/typeclass` (`.api/effect-typeclass.md`)] — a keyed fold IS a Semigroup applied incrementally: the `reduce` reducer `(vals) => [R, number][]` is the elementwise projection of `Semigroup.combineMany`, and `groupByOperators.{sum,min,max}` are the `Number.Monoid{Sum,Min,Max}` instances specialized to signed pairs. `state/merge` declares the merge as one lawful `Semigroup`; `state/fold` applies it through `reduce` so the reducer law and the merge law are proven once and shared.

[STACK: two altitudes — `d2mini.md` core, d2ts durable] — `d2mini` is this same operator algebra minus time (browser in-memory, `sendData(collection)`, no `Version`); d2ts is the durable/versioned altitude (`sendData(version, collection)`, `Version`/`Antichain`, `Index.reconstructAt`, `./sqlite` trace). `state/fold` binds one algebra; the runtime picks d2mini (browser, in-memory) or d2ts (node, durable through `store/project`) — never a second fold implementation.

[STACK: REPLAY_LAW + `state/causal` frontiers] — `Version.meet` is the stability-frontier GLB `state/causal` computes; `Antichain` is the honest-uncertainty frontier over `value/clock` windows; `iterate` runs the happened-before transitive fold; `Index.compact(frontier)` is the retention-frontier handoff to `store/journal`. A fold rebuilt from any event prefix replays through the same graph to the live version — the convergence the `state/merge` laws assert, proven in kit-driven specs via `@effect/vitest` `it.prop` (`.api/effect-vitest.md`) over fixtures pinned in the `tests/contracts` corpus.

## [05]-[RAIL_LAW]

- Owns: versioned incremental dataflow — a graph of `PipedOperator`s over signed `MultiSet` deltas at partial-order `Version`s; the keyed folds (`join`/`reduce`/`count`/`distinct`/`groupBy`), ordered maintenance via fractional index (`topK`/`orderBy`), the `iterate` fixpoint, the `Index` versioned trace with `reconstructAt`/`compact`, and the `./sqlite`+`./electric` durable/replication bindings.
- Accept: `sendData(version, delta)` with signed multisets; `pipe(...)` composition of the operator roster; `reduce`/`groupBy` as the incremental application of a `@effect/typeclass` Semigroup; `topKWithFractionalIndex`/`orderByWithFractionalIndex` for ordered views; `Index.reconstructAt` for AsOf reads; `iterate` for transitive/recursive folds; `./sqlite` only at the durable node altitude.
- Reject: re-folding or re-sorting a whole collection on change (the named defect — use the incremental operator); a second fold implementation per runtime (d2mini and d2ts are two altitudes of ONE algebra); importing `./sqlite`/`./electric` (or their `better-sqlite3`/`@electric-sql/client` peers) into `state`'s transport-free browser core; treating `Version` as a total order when a partial (multi-replica) order is the domain.
- Boundary: the `.` core is browser-safe and peer-free; the durable and replication subpaths are node-only peer-gated bindings. `Version` interning via `v()` is required for map-key identity. `min`/`max` reject negative multiplicity — retraction extremum folds run through `reduce`/`consolidate`; `distinct` consolidates signed sums per key and emits only positive-count survivors, so retraction through it is lawful.
