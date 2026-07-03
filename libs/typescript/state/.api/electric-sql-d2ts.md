# [@electric-sql/d2ts] — the versioned differential-dataflow engine behind fold/replay: partial-order time, signed multisets, an incremental operator algebra

[PACKAGE_SURFACE]:
- package: `@electric-sql/d2ts` · version `0.1.8` · license `Apache-2.0`
- module: ESM only (`type: module`, no CJS mirror); `.` barrel plus two peer-gated subpaths — `./sqlite` (durable operator state) and `./electric` (live-replication binding).
- asset: `dist/index.d.ts` (barrel over `d2` / `order` / `multiset` / `version-index` / `operators` / `types`); the `.` core bundles `fractional-indexing` + `murmurhash-js` and has NO peer requirement.
- runtime: pure-TS in-process dataflow; runs in node / bun / browser / worker. The `.` core is browser-safe; `./sqlite` requires the `better-sqlite3` peer (node durable altitude) and `./electric` the `@electric-sql/client` peer (Postgres replication) — neither peer crosses into `state`'s transport-free import surface.
- ABI: synchronous fixpoint scheduler — `graph.run()` / `graph.step()` drain the operator queue on the calling thread; `topKWithFractionalIndexBTree`'s d2mini sibling is async-loaded, but every d2ts operator here is sync.
- plane: `plane:runtime` (W1); folder-local to `state`, catalogued here.
- rail: incremental-dataflow / fold-maintenance.

`@electric-sql/d2ts` is the incremental engine the `fold/replay` REPLAY_LAW rides: a fold is a dataflow graph over signed multisets at partially-ordered versions, and a delta pushed at a new version is maintained through the operator graph WITHOUT re-folding the prefix — the "array re-sort fallback" the design names as a defect is exactly the whole-collection recompute this package deletes. The `Version`/`Antichain` partial order IS the AsOf three-coordinate time the `query/window` watermarks and `causal/order` frontiers are expressed in; `Index.reconstructAt(key, version)` is the time-travel read; `iterate` is the fixpoint the transitive folds (causal reachability, Merkle closure) run under. It is the durable-altitude counterpart to `d2mini.md`'s browser-in-memory core: same operator algebra, d2ts adds the versioned frontier and the `./sqlite` persistent trace so a node fold survives restart. A merge folded through `reduce`/`groupBy` is a `@effect/typeclass` Semigroup applied incrementally — the reducer law and the merge law are the same law at two speeds.

## [01]-[GRAPH_AND_TIME]

The graph is constructed once, wired with `pipe`, then driven; time is a partial order so multi-dimensional versions (per-replica logical clocks) fold under one engine.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]      | [CAPABILITY / BOUNDARY]                                                        |
| :-----: | :------------------------------ | :----------------- | :----------------------------------------------------------------------------- |
|  [01]   | `D2` (`D2Options`)              | class              | the dataflow graph; `newInput()` mints a `RootStreamBuilder`, `run`/`step` drive it |
|  [02]   | `RootStreamBuilder<T>`          | class              | input handle — `sendData(version, collection)` + `sendFrontier(frontier)`      |
|  [03]   | `StreamBuilder<T>`              | class (`IStreamBuilder`) | pipeline node; the 1..20-arity `pipe(...)` composes `PipedOperator`s      |
|  [04]   | `Version` (`v()` factory)       | class              | partially-ordered logical time — `join`/`meet`/`lessThan`/`advanceBy`/`extend` |
|  [05]   | `Antichain` / `Frontier`        | class              | minimal incomparable version set — `meet`, `lessEqualVersion`; the frontier bound |
|  [06]   | `MultiSet<T>` (`MultiSetArray`) | class              | signed multiset (`[value, multiplicity][]`) — the delta unit; negative = retraction |
|  [07]   | `Message<T>` / `MessageType`    | tagged union       | `DATA`(version+collection) \| `FRONTIER`(version\|antichain) — the `output` payload |
|  [08]   | `KeyValue<K, V>` = `[K, V]`     | tuple alias        | the keyed-record shape every keyed operator (`join`/`reduce`/`groupBy`) requires |
|  [09]   | `PipedOperator<I, O>`           | function alias     | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>` — the ONE operator shape      |

```ts contract
// One graph, wired declaratively, driven synchronously. sendData carries a signed delta AT a version; the frontier closes a version.
declare class D2 implements ID2 {
  constructor(opts: { initialFrontier: Antichain | Version | number | number[] })
  newInput<T>(): RootStreamBuilder<T>
  step(): void                    // run one scheduler tick
  run(): void                     // drain to quiescence
  finalize(): void                // seal the graph — no more operators
  pushFrontier(f: Antichain): void; popFrontier(): void; frontier(): Antichain
}
declare class RootStreamBuilder<T> extends StreamBuilder<T> {
  sendData(version: Version | number | number[], collection: MultiSet<T> | MultiSetArray<T>): void
  sendFrontier(frontier: Antichain | Version | number | number[]): void
}
// Version is a partial order: 1-D versions are totally ordered (a lamport tick); N-D versions fold per-replica clocks under the product order.
declare class Version {
  lessThan(o: Version): boolean; lessEqual(o: Version): boolean; equals(o: Version): boolean
  join(o: Version): Version       // least upper bound
  meet(o: Version): Version       // greatest lower bound — the stability-frontier GLB
  advanceBy(frontier: Antichain): Version
}
declare function v(version: number | number[]): Version   // cached/interned — safe as a Map key
```

```ts contract
// The signed multiset is the delta algebra: map/filter/negate/concat/consolidate are pure, the keyed folds (join/reduce/count/min/max/distinct/iterate) exploit KeyValue structure.
declare class MultiSet<T> {
  constructor(data?: MultiSetArray<T>)               // [value, multiplicity][]
  map<U>(f: (d: T) => U): MultiSet<U>; filter(f: (d: T) => boolean): MultiSet<T>
  negate(): MultiSet<T>                              // flip every multiplicity — retraction
  concat(o: MultiSet<T>): MultiSet<T>; consolidate(): MultiSet<T>   // merge identical records, drop zero-sum
  join<U>(o: MultiSet<KeyedData<U>>): MultiSet<KeyedData<[T, U]>>   // KeyedData<T> = [key: string, value: T]
  reduce<U>(f: (vals: [T, number][]) => [U, number][]): MultiSet<KeyedData<U>>
  count(): MultiSet<KeyedData<number>>; sum(): MultiSet<KeyedData<number>>; distinct(): MultiSet<KeyedData<T>>
  min(): MultiSet<KeyedData<T>>; max(): MultiSet<KeyedData<T>>      // per-key; no negative multiplicity
  iterate(f: (c: MultiSet<T>) => MultiSet<T>): MultiSet<T>          // fixpoint — runs to convergence
  extend(o: MultiSet<T> | MultiSetArray<T>): void; getInner(): MultiSetArray<T>
}
```

## [02]-[OPERATOR_ALGEBRA]

The operator library is ONE parameterized shape — `PipedOperator<I, O>` composed left-to-right by `pipe` — not a fixed method wall. The roster below is SEED DATA on that shape: a new dataflow verb is a new `PipedOperator`, never a new graph type. Four families vary by what structure they exploit — element-wise, keyed-fold, ordered, and recursive.

| [INDEX] | [FAMILY]        | [OPERATORS]                                                                 | [SHAPE / BOUNDARY]                                              |
| :-----: | :-------------- | :-------------------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | element-wise    | `map` `filter` `negate` `concat` `consolidate` `buffer` `debug` `output`     | `PipedOperator<T, …>` — no key structure; `output` sees `Message` |
|  [02]   | keying          | `keyBy(fn)` `unkey()` `rekey(fn)`                                            | `T ⇄ KeyValue<K, T>` — the adapter into/out of the keyed family |
|  [03]   | keyed fold      | `join`(+`inner`/`left`/`right`/`full`/`anti`) `reduce` `count` `distinct` `groupBy` | `KeyValue<K, V>` in; the incremental fold — a Semigroup applied per key |
|  [04]   | ordered         | `topK` `topKWithIndex` `topKWithFractionalIndex` `orderBy`(+`WithIndex`/`WithFractionalIndex`) | keyed; fractional index maintains order WITHOUT a full re-sort |
|  [05]   | recursive       | `iterate(f)` (`IngressOperator`/`EgressOperator`/`FeedbackOperator`)          | fixpoint scope — loops the sub-graph to convergence            |

```ts contract
// pipe is variadic (1..20 typed arities + a rest fallback); every stage is a PipedOperator. This is the whole composition mechanism.
input.pipe(
  map((row) => row.payload),
  filter((p) => p.live),
  keyBy((p) => p.entityId),
  reduce((vals: [Payload, number][]) => foldSemigroup(vals)),   // the keyed incremental fold
  consolidate(),
  output((msg: Message<KeyValue<Payload>>) => sink(msg)),
)
```

```ts contract
// join is ONE operator parameterized by JoinType; inner/anti/left/right/full are convenience rows on it, not five hand-rolled joins.
type JoinType = 'inner' | 'left' | 'right' | 'full' | 'anti'
declare function join<K, V1, V2, T>(other: IStreamBuilder<KeyValue<K, V2>>, type?: JoinType): PipedOperator<T, KeyValue<K, [V1 | null, V2 | null]>>
declare function innerJoin<…>(other): PipedOperator<T, KeyValue<K, [V1, V2]>>          // + antiJoin/leftJoin/rightJoin/fullJoin
// reduce is the fold primitive — signed input pairs to signed output pairs; count/distinct/min/max are reduce specializations.
declare function reduce<K, V1, R, T>(f: (values: [V1, number][]) => [R, number][]): (s: IStreamBuilder<T>) => IStreamBuilder<KeyValue<K, R>>
// groupBy composes named aggregate combinators — one parameterized aggregate shape, the roster is seed data.
declare function groupBy<T, K, A extends Record<string, AggregateFunction<T, any>>>(keyFn: (d: T) => K, aggs: A): (s: IStreamBuilder<T>) => IStreamBuilder<KeyValue<string, K & AggregatesReturnType<T, A>>>
declare const groupByOperators: { sum; count; avg; min; max; median; mode }   // AggregateFunction rows; `avg` carries {sum,count} state
```

```ts contract
// The named-defect deletion: orderBy re-sorts on every change; topKWithFractionalIndex assigns a fractional string index so an insert is one key, not a re-sort.
declare function orderByWithFractionalIndex<T, Ve>(valueExtractor: (v: V) => Ve, opts?: { comparator?; limit?; offset? }): (s: IStreamBuilder<T>) => IStreamBuilder<KeyValue<K, [V, string]>>
declare function topKWithFractionalIndex<K, V1, T>(cmp: (a: V1, b: V1) => number, opts?: { limit?; offset? }): PipedOperator<T, KeyValue<K, [V1, string]>>
// iterate is the recursive scope — the sub-graph loops to a fixpoint (transitive closure, causal reachability, Merkle-DAG walk).
declare function iterate<T>(f: (s: IStreamBuilder<T>) => IStreamBuilder<T>): (s: IStreamBuilder<T>) => IStreamBuilder<T>
```

## [03]-[INDEX_AND_PERSISTENCE]

`Index<K, V>` is the versioned trace the keyed operators keep — a key → versions → signed values map that `reconstructAt` reads at any requested version and `compact` collapses below a stability frontier. It is the mechanism `reconstructAt` gives `query/window` AsOf reads and `causal/order` retention-frontier handoff.

| [INDEX] | [SURFACE]                                     | [PRODUCES]              | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------- | :---------------------- | :------------------------------------------------------------- |
|  [01]   | `Index<K, V>.reconstructAt(key, version)`     | `[V, number][]`         | time-travel read — the AsOf materialization at a partial-order version |
|  [02]   | `Index<K, V>.compact(frontier, keys?)`        | — (in-place)            | collapse trace below the stability frontier — the retention handoff |
|  [03]   | `Index<K, V>.versions(key)` / `.join(other)`  | `Version[]` / diff rows | the per-key version set; incremental join at matching versions  |
|  [04]   | `./sqlite` `SQLiteDb` / `BetterSQLite3Wrapper`| durable operator state  | the persistent trace — a node fold survives restart (peer: `better-sqlite3`) |
|  [05]   | `./electric` `electricStreamToD2Input(opts)`  | `RootStreamBuilder`     | binds a Postgres `ShapeStream` as graph input, `lsnToVersion` maps LSN → `Version` |
|  [06]   | `./electric` `outputElectricMessages(fn)`     | `PipedOperator`         | emits `ChangeMessage<Row>[]` — the replication-shaped egress (peer: `@electric-sql/client`) |

```ts contract
// The ./sqlite subpath mirrors the core operator set (join/reduce/distinct/groupBy/orderBy/topK/consolidate/buffer) over a SQLite-backed version index — the durable node altitude.
interface SQLiteDb { exec(sql: string): void; prepare<P, R>(sql: string): SQLiteStatement<P, R> }
declare class BetterSQLite3Wrapper implements SQLiteDb { constructor(db: import('better-sqlite3').Database); close(): void }
// The ./electric subpath is the live-replication bridge — LSN becomes the version, so replication order IS dataflow time.
declare function electricStreamToD2Input<T extends Row>(opts: {
  graph: D2; stream: ShapeStreamInterface<T>; input: RootStreamBuilder<[key: string, T]>
  lsnToVersion?: (lsn: number) => number | Version; lsnToFrontier?: (lsn: number) => number | Antichain
  runOn?: 'up-to-date' | 'lsn-advance' | false
}): RootStreamBuilder<[key: string, T]>
```

## [04]-[INTEGRATION]

[STACK: `D2` + `effect/Data` + `Schema` (`.api/effect.md`)] — the collection element is not a bare object: `fold/algebra` folds `Data.TaggedEnum` op values (the CRDT/journal op vocabulary) decoded by a `Schema`. `map`/`filter` bodies pattern-match with `effect/Match` on the op `_tag`; `Equal.equals`/`Hash` make `consolidate` and `distinct` structural, so two equal op values collapse to one record — idempotence by construction. `state` expresses the graph in core `effect` only; d2ts adds the incremental engine under it.

[STACK: `reduce`/`groupBy` + `@effect/typeclass` (`.api/effect-typeclass.md`)] — a keyed fold IS a Semigroup applied incrementally: the `reduce` reducer `(vals) => [R, number][]` is the elementwise projection of `Semigroup.combineMany`, and `groupByOperators.{sum,min,max}` are the `Number.Monoid{Sum,Min,Max}` instances specialized to signed pairs. `crdt/merge` declares the merge as one lawful `Semigroup`; `fold/replay` applies it through `reduce` so the reducer law and the merge law are proven once and shared.

[STACK: two altitudes — `d2mini.md` core, d2ts durable] — `d2mini` is this same operator algebra minus time (browser in-memory, `sendData(collection)`, no `Version`); d2ts is the durable/versioned altitude (`sendData(version, collection)`, `Version`/`Antichain`, `Index.reconstructAt`, `./sqlite` trace). `fold/algebra` binds one algebra; the runtime picks d2mini (browser, in-memory) or d2ts (node, durable through `store/project`) — never a second fold implementation.

[STACK: REPLAY_LAW + `causal/order` frontiers] — `Version.meet` is the stability-frontier GLB `causal/order` computes; `Antichain` is the honest-uncertainty frontier over `kernel/clock` windows; `iterate` runs the happened-before transitive fold; `Index.compact(frontier)` is the retention-frontier handoff to `store/journal`. A fold rebuilt from any event prefix replays through the same graph to the live version — the convergence the `crdt/converge` laws assert, proven in `proof` via `@effect/vitest` `it.prop` (`.api/effect-vitest.md`) over `proof/corpus` fixtures.

## [05]-[RAIL_LAW]

- Owns: versioned incremental dataflow — a graph of `PipedOperator`s over signed `MultiSet` deltas at partial-order `Version`s; the keyed folds (`join`/`reduce`/`count`/`distinct`/`groupBy`), ordered maintenance via fractional index (`topK`/`orderBy`), the `iterate` fixpoint, the `Index` versioned trace with `reconstructAt`/`compact`, and the `./sqlite`+`./electric` durable/replication bindings.
- Accept: `sendData(version, delta)` with signed multisets; `pipe(...)` composition of the operator roster; `reduce`/`groupBy` as the incremental application of a `@effect/typeclass` Semigroup; `topKWithFractionalIndex`/`orderByWithFractionalIndex` for ordered views; `Index.reconstructAt` for AsOf reads; `iterate` for transitive/recursive folds; `./sqlite` only at the durable node altitude.
- Reject: re-folding or re-sorting a whole collection on change (the named defect — use the incremental operator); a second fold implementation per runtime (d2mini and d2ts are two altitudes of ONE algebra); importing `./sqlite`/`./electric` (or their `better-sqlite3`/`@electric-sql/client` peers) into `state`'s transport-free browser core; treating `Version` as a total order when a partial (multi-replica) order is the domain.
- Boundary: the `.` core is browser-safe and peer-free; the durable and replication subpaths are node-only peer-gated bindings. `Version` interning via `v()` is required for map-key identity. `min`/`max`/`distinct` reject negative multiplicity — retraction folds run through `reduce`/`consolidate`, never those.
