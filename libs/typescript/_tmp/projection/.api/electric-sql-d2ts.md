# [API_CATALOGUE] @electric-sql/d2ts

`@electric-sql/d2ts` is the full differential-dataflow runtime of the TypeScript branch: `D2` (a versioned graph host carrying a frontier antichain), `RootStreamBuilder<T>` (the versioned input source), the `pipe`-composable `IStreamBuilder<T>`, `MultiSet<T>` (the signed-multiplicity collection), the `Version`/`Antichain`/`Frontier`/`v()` partial-order time domain, `Index<K, V>` (the key-versioned trace behind `join`/`reduce`/`reconstructAt`), the `Message`/`MessageType` protocol, and the complete operator library. Two subpaths carry the ingestion bridges the projection folder is founded on: `@electric-sql/d2ts/electric` connects an ElectricSQL `ShapeStream` to a D2 input and maps the Postgres LSN into the event-time `Version` (`electricStreamToD2Input`, `outputElectricMessages`), and `@electric-sql/d2ts/sqlite` persists the trace `Index` and operators in `better-sqlite3`. `query/watermark`+`query/window` fold on this runtime's version trace, `query/asof` reads it through `Index.reconstructAt`, and `convergence/retention` finalizes against its `Frontier`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@electric-sql/d2ts`
- package: `@electric-sql/d2ts` (0.1.8, Apache-2.0, © Electric DB Ltd)
- module format: ESM (`type: module`), no `sideEffects` field declared; three `exports` entries — `.` → `dist/index.d.ts` (re-exports `d2`, `order`, `multiset`, `version-index`, `operators/index`, `types`), `./electric` → `dist/electric/index.d.ts`, `./sqlite` → `dist/sqlite/index.d.ts` (the `exports` map gates deep imports to exactly these three)
- runtime target: isomorphic core (`.`); `./sqlite` is node-only (`better-sqlite3` native addon), `./electric` needs an `@electric-sql/client` `ShapeStream`
- deps: `fractional-indexing` (the `topKWithFractionalIndex` string keys), `murmurhash-js` + `@types/murmurhash-js` (record hashing); peers `@electric-sql/client >= 1.0.0-beta.4` (`./electric`) and `better-sqlite3 ^11.7.0` (`./sqlite`) — both optional to the isomorphic `.` core
- asset: versioned differential-dataflow graph + operators + ingestion adapters
- rail: stream / dataflow (the event-time engine of `projection`; the versionless twin is `@electric-sql/d2mini`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph host and stream builders (`.` root)
- rail: stream / dataflow

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]                 | [CAPABILITY]                                                      |
| :-----: | :--------------------- | :---------------------------- | :---------------------------------------------------------------- |
|  [01]   | `D2`                   | class                         | dataflow graph host; versioned step execution + frontier scope    |
|  [02]   | `D2Options`            | type                          | `{ initialFrontier: Antichain \| Version \| number \| number[] }` |
|  [03]   | `StreamBuilder<T>`     | class                         | `IStreamBuilder<T>` base; 1–20-arity typed `pipe` overloads        |
|  [04]   | `RootStreamBuilder<T>` | class (extends StreamBuilder) | input source; `sendData(version, collection)` + `sendFrontier`     |
|  [05]   | `ID2`                  | interface                     | `newInput`/`step`/`finalize`/`frontier`/`pushFrontier`/`popFrontier` contract |
|  [06]   | `IStreamBuilder<T>`    | interface                     | `pipe(…operators)` (up to 20 typed + variadic), `writer`, `connectReader`, `graph` |
|  [07]   | `PipedOperator<I, O>`  | type alias                    | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>`                 |

[PUBLIC_TYPE_SCOPE]: collection and the partial-order time domain
- rail: stream / dataflow

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]             | [CAPABILITY]                                                                 |
| :-----: | :----------------- | :------------------------ | :--------------------------------------------------------------------------- |
|  [01]   | `MultiSet<T>`      | class                     | signed-multiplicity collection; instance algebra `map`/`filter`/`negate`/`concat`/`consolidate`/`join`/`reduce`/`count`/`sum`/`min`/`max`/`distinct`/`iterate`/`extend`/`getInner` + `toJSON`/`fromJSON` |
|  [02]   | `MultiSetArray<T>` | type alias                | `[T, number][]` — the raw multiplicity array `sendData` also accepts          |
|  [03]   | `KeyedData<T>`     | type alias                | `[key: string, value: T]`                                                    |
|  [04]   | `KeyValue<K, V>`   | type alias                | `[K, V]` — the canonical keyed-stream element                                |
|  [05]   | `Version`          | class                     | ordered time tuple; `equals`/`lessThan`/`lessEqual`/`join`(lub)/`meet`(glb)/`advanceBy`/`extend`/`truncate`/`applyStep`/`getInner` |
|  [06]   | `Antichain`        | class                     | minimal incomparable-version set (frontier); `static create(value)`, `meet`/`equals`/`lessThan`/`lessEqual`/`lessEqualVersion`/`isEmpty`/`extend`/`truncate`/`applyStep`/`elements` |
|  [07]   | `Frontier`         | class (extends Antichain) | variadic-constructor convenience: `new Frontier(...versions)`                 |
|  [08]   | `v(version)`       | factory                   | cached `Version` factory (`number \| number[]`) — one identity per version, safe as a map key |

[PUBLIC_TYPE_SCOPE]: graph edges, operator bases, and the trace index
- rail: stream / dataflow
- reachability: `Index`/`IndexType` (via `version-index`) and the `I*` interfaces are direct root imports; the edge handles and operator bases below are NOT re-exported at the root (`graph.js`/`operators/base.js` sit outside the barrel) — they surface only as referenced types in the exported `IStreamBuilder`/`IOperator` signatures and as bases the operators subclass, so consumer code reaches them through `newInput`/`pipe`/`output`, never by importing them

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]              | [CAPABILITY]                                                                 |
| :-----: | :-------------------------- | :------------------------ | :--------------------------------------------------------------------------- |
|  [01]   | `DifferenceStreamReader<T>` | class (edge handle)       | read handle in `IStreamBuilder`; `drain()` → `Message<T>[]`, `isEmpty()`, `probeFrontierLessThan` |
|  [02]   | `DifferenceStreamWriter<T>` | class (edge handle)       | write handle in `IStreamBuilder`; `sendData(version, collection)`, `sendFrontier`, `newReader()` |
|  [03]   | `Operator<T>`               | abstract class (internal) | base multi-input operator with frontier tracking (`graph`)                   |
|  [04]   | `UnaryOperator<T>` / `BinaryOperator<T>` | abstract class (internal) | single- / two-input operator bases (`graph`)                    |
|  [05]   | `LinearUnaryOperator<T, U>` | abstract class (internal) | stateless map/filter/negate base — declared in `operators/base`, not `graph` |
|  [06]   | `IOperator<T>`              | interface                 | `run()`, `hasPendingWork()`, `frontiers()` → `[Antichain[], Antichain]`       |
|  [07]   | `Index<K, V>`               | class                     | key-versioned trace; `reconstructAt`, `get`, `entries`, `versions`, `addValue`, `append`, `join`, `compact`, `keys`, `has` |
|  [08]   | `IndexType<K, V>`           | interface                 | the `Index` contract (`reconstructAt`/`versions`/`addValue`/`append`/`join`/`compact`/`keys`/`has`) |

[PUBLIC_TYPE_SCOPE]: message protocol
- rail: stream / dataflow

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                                                     |
| :-----: | :---------------- | :------------------ | :-------------------------------------------------------------- |
|  [01]   | `Message<T>`      | discriminated union | `{ type: DATA, data: DataMessage<T> }` \| `{ type: FRONTIER, data: FrontierMessage }` |
|  [02]   | `DataMessage<T>`  | type                | `{ version: Version; collection: MultiSet<T> }` — the `output` fn payload |
|  [03]   | `FrontierMessage` | type alias          | `Version \| Antichain`                                          |
|  [04]   | `MessageType`     | const object        | `{ DATA: 1, FRONTIER: 2 }` (`const` value + same-named type)     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph lifecycle and frontier scope
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `new D2({ initialFrontier })`                       | constructor    | version-tracking graph with an initial frontier |
|  [02]   | `D2#newInput<T>()`                                  | factory        | returns a `RootStreamBuilder<T>` input source |
|  [03]   | `D2#step()` / `D2#run()`                            | execution      | one incremental step / run to quiescence (`pendingWork()` probes remaining work) |
|  [04]   | `D2#finalize()`                                     | lifecycle      | seals the graph — no operators may be added after |
|  [05]   | `D2#frontier()`                                     | query          | the current output `Antichain`               |
|  [06]   | `D2#pushFrontier(f)` / `D2#popFrontier()`           | scope          | nested frontier-scope management              |
|  [07]   | `RootStreamBuilder#sendData(version, collection)`   | data ingestion | `version: Version \| number \| number[]`, `collection: MultiSet<T> \| MultiSetArray<T>` |
|  [08]   | `RootStreamBuilder#sendFrontier(frontier)`          | frontier push  | advances the input frontier (call after all `sendData` for a version) |

[ENTRYPOINT_SCOPE]: piped operators — transforms, keying, and fixedpoint
- rail: stream / dataflow

| [INDEX] | [SURFACE]                    | [SIGNATURE / PRODUCES]                     | [CAPABILITY]                                  |
| :-----: | :--------------------------- | :----------------------------------------- | :-------------------------------------------- |
|  [01]   | `map<T, O>(f)`               | `(data: T) => O` → `PipedOperator<T, O>`   | element-wise transform (linear)               |
|  [02]   | `filter<T>(f)`               | `(data: T) => boolean` → `PipedOperator<T, T>` | predicate filter (linear)                 |
|  [03]   | `negate<T>()`                | `PipedOperator<T, T>`                       | negate multiplicities                         |
|  [04]   | `concat<T, T2>(other)`       | `IStreamBuilder<T2>` → `PipedOperator<T, T\|T2>` | stream union                            |
|  [05]   | `consolidate<T>()`           | `PipedOperator<T, T>`                       | merge multiplicity cancellations              |
|  [06]   | `buffer<T>()`                | `PipedOperator<T, T>`                       | hold input until the frontier advances, then release |
|  [07]   | `output<T>(fn)`              | `(data: Message<T>) => void` → `PipedOperator<T, T>` | side-effecting observer of `DATA`/`FRONTIER` messages |
|  [08]   | `debug<T>(name, indent?)`    | `PipedOperator<T, T>`                       | log each message under `name` (dev tracing)   |
|  [09]   | `keyBy<T, K>(keyFn)`         | `PipedOperator<T, Keyed<K, T>>`             | add key to elements                           |
|  [10]   | `unkey<K, V>()`              | `PipedOperator<Keyed<K,V>, V>`              | strip key                                     |
|  [11]   | `rekey<K1, K2, V>(keyFn)`    | `PipedOperator<Keyed<K1,V>, Keyed<K2,V>>`   | rekey by a new function                        |
|  [12]   | `filterBy<K, V1>(other)`     | `IStreamBuilder<KeyValue<K, unknown>>` → `PipedOperator<T, KeyValue<K, V1>>` | semi-join: keep keyed elements whose key appears in `other` |
|  [13]   | `iterate<T>(f)`              | `(stream) => stream` → `PipedOperator<T, T>` | dataflow fixedpoint (runs forever if `f` never converges) |
|  [14]   | `pipe(...operators)`         | curried composition                         | free-standing `pipe`; the `IStreamBuilder#pipe` method is the usual site |

[ENTRYPOINT_SCOPE]: piped operators — keyed aggregations, joins, and ordering
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                                        | [PRODUCES]                                          | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------- | :-------------------------------------------------- | :------------------------------------ |
|  [01]   | `join<K, V1, V2>(other, type?)`                                 | `PipedOperator<T, KeyValue<K,[V1\|null,V2\|null]>>` | keyed join; `type: JoinType`          |
|  [02]   | `innerJoin` / `leftJoin` / `rightJoin` / `fullJoin` / `antiJoin` | typed join shorthands             | narrowed null-shape join aliases (e.g. `innerJoin` → `[V1,V2]`, `antiJoin` → `[V1,null]`) |
|  [03]   | `reduce<K, V1, R>(f)`                                            | `(stream) => IStreamBuilder<KeyValue<K,R>>`         | keyed reduce over `[V1, number][]` multiplicity rows |
|  [04]   | `count()`                                                       | `(stream) => IStreamBuilder<KeyValue<K,number>>`    | per-key count                         |
|  [05]   | `distinct()`                                                    | `(stream) => IStreamBuilder<KeyValue<K,V>>`         | deduplicate by key                    |
|  [06]   | `topK<K, V1>(comparator, options?)`                             | `PipedOperator<T, T>`                               | per-key ranked limit (`{ limit?, offset? }`), sorted within key group |
|  [07]   | `topKWithIndex(comparator, options?)`                          | `PipedOperator<T, KeyValue<K, [V1, number]>>`       | topK annotating each element's integer rank |
|  [08]   | `topKWithFractionalIndex(comparator, options?)`                | `PipedOperator<T, KeyValue<K, [V1, string]>>`       | topK with a lexicographic fractional index — only moved elements re-index (list-CRDT-friendly) |
|  [09]   | `orderBy<T>(valueExtractor, options?)`                          | `(stream) => IStreamBuilder<T>`                     | ordered limit within key group (`{ comparator?, limit?, offset? }`) |
|  [10]   | `orderByWithIndex` / `orderByWithFractionalIndex`              | `IStreamBuilder<KeyValue<K, [V, number\|string]>>`  | ordering with integer / fractional position annotation |
|  [11]   | `groupBy<T, K, A>(keyExtractor, aggregates)`                    | `(stream) => IStreamBuilder<KeyValue<string, K & AggResult>>` | multi-aggregate group-by       |

[ENTRYPOINT_SCOPE]: `groupBy` aggregate functions
- rail: stream / dataflow
- access: reached through the `groupByOperators` namespace (`groupByOperators.sum`, …); only `groupBy` and `groupByOperators` are barrelled from `operators/index`

| [INDEX] | [SURFACE]                                     | [PRODUCES]                                          | [CAPABILITY]      |
| :-----: | :-------------------------------------------- | :-------------------------------------------------- | :---------------- |
|  [01]   | `groupByOperators.sum<T>(valueExtractor?)`    | `AggregateFunction<T, number, number>`              | numeric sum       |
|  [02]   | `groupByOperators.count<T>()`                 | `AggregateFunction<T, number, number>`              | element count     |
|  [03]   | `groupByOperators.avg<T>(valueExtractor?)`    | `AggregateFunction<T, number, { sum; count }>`      | arithmetic mean   |
|  [04]   | `groupByOperators.min<T>` / `max<T>(valueExtractor?)` | `AggregateFunction<T, number, number>`      | minimum / maximum |
|  [05]   | `groupByOperators.median<T>(valueExtractor?)` | `AggregateFunction<T, number, number[]>`            | median candidates |
|  [06]   | `groupByOperators.mode<T>(valueExtractor?)`   | `AggregateFunction<T, number, Map<number, number>>` | frequency map     |

[ENTRYPOINT_SCOPE]: `@electric-sql/d2ts/electric` — ElectricSQL ShapeStream bridge (subpath; not on the `.` barrel)
- rail: stream / dataflow
- peer: `@electric-sql/client` (`Row`, `ShapeStreamInterface`, `ChangeMessage`); the `ShapeStream` MUST be configured `replica: 'full'`

| [INDEX] | [SURFACE]                                    | [SIGNATURE / PRODUCES]                                              | [CAPABILITY]                                  |
| :-----: | :------------------------------------------- | :----------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `electricStreamToD2Input(options)`           | `ElectricStreamToD2InputOptions<T>` → `RootStreamBuilder<[key: string, T]>` | pumps a `ShapeStream<T>` into a D2 input, keyed by row primary key |
|  [02]   | `ElectricStreamToD2InputOptions<T>`          | `{ graph, stream, input, lsnToVersion?, lsnToFrontier?, initialLsn?, runOn?, debug? }` | the bridge config — `lsnToVersion`/`lsnToFrontier` map the Postgres LSN onto the event-time `Version`/`Antichain` |
|  [03]   | `options.runOn`                              | `'up-to-date' \| 'lsn-advance' \| false`                           | when to auto-`run()` the graph — on shape catch-up, on every LSN advance, or never (manual) |
|  [04]   | `outputElectricMessages(fn)`                 | `(data: ChangeMessage<Row<V>>[]) => void` → `(stream) => IStreamBuilder<KeyValue<K, V>>` | project the D2 output back into ElectricSQL `ChangeMessage` batches (egress) |
|  [05]   | `OutputElectricMessagesOperator<K, V>`       | `UnaryOperator` subclass                                            | the operator behind `outputElectricMessages` (with `transformMessages`) |

[ENTRYPOINT_SCOPE]: `@electric-sql/d2ts/sqlite` — persistent trace (subpath; not on the `.` barrel)
- rail: stream / dataflow
- peer: `better-sqlite3 ^11.7.0` (node-only native addon)

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------------------------------- |
|  [01]   | `withSQLite(db)(...operators)`                  | DI wrapper      | the injection entrypoint — wraps a pipeline of operators so the SQLite `db` reaches every SQLite operator without threading it explicitly (backed by the static `SQLiteContext.setDb`/`getDb`/`clear`) |
|  [02]   | `SQLiteDb` / `SQLiteStatement`                  | interface       | the DB contract the operators persist into — `SQLiteDb.exec(sql)`/`prepare<P,R>(sql)`; `SQLiteStatement.run`/`get`/`all`/`finalize?` |
|  [03]   | `BetterSQLite3Wrapper`                          | class           | the concrete `SQLiteDb` — `new BetterSQLite3Wrapper(db: import('better-sqlite3').Database)`, `exec`/`prepare`/`close` (the object you pass to `withSQLite`) |
|  [04]   | `SQLIndex<K, V>`                                | class           | the `Index` trace persisted in SQLite (`new SQLIndex(db, name, isTemp?)`): `reconstructAt`/`get`/`entries`/`versions`/`addValue`/`addValues`/`addModifiedKeys`/`append`/`join`/`compact`/`getCompactionFrontier`/`setCompactionFrontier`/`showAll`/`truncate`/`destroy` — for traces larger than memory |
|  [05]   | SQLite operator variants                        | operators       | `consolidate`/`count`/`distinct`/`join`/`reduce`/`buffer`/`filterBy`/`orderBy`/`topK`/`topKWithFractionalIndex` plus `{ groupBy, mode }` re-implemented over `SQLIndex` (`./sqlite/operators`); no `map`/`filter`/`negate` variant — those stateless linear ops stay in-memory |

```ts contract
// @electric-sql/d2ts (.)
declare class D2 {
  constructor(options: { initialFrontier: Antichain | Version | number | number[] })
  newInput<T>(): RootStreamBuilder<T>
  step(): void; run(): void; pendingWork(): boolean; finalize(): void
  frontier(): Antichain; pushFrontier(f: Antichain): void; popFrontier(): void
}
declare class RootStreamBuilder<T> extends StreamBuilder<T> {
  sendData(version: Version | number | number[], collection: MultiSet<T> | MultiSetArray<T>): void
  sendFrontier(frontier: Antichain | Version | number | number[]): void
}
declare class Index<K, V> {
  reconstructAt(key: K, requestedVersion: Version): [V, number][]
  join<V2>(other: Index<K, V2>): [Version, MultiSet<[K, [V, V2]]>][]
  compact(compactionFrontier: Antichain, keys?: K[]): void
  versions(key: K): Version[]; keys(): K[]; has(key: K): boolean
}

// @electric-sql/d2ts/electric
declare function electricStreamToD2Input<T extends Row = Row>(options: {
  graph: D2; stream: ShapeStreamInterface<T>; input: RootStreamBuilder<[key: string, T]>
  lsnToVersion?: (lsn: number) => number | Version
  lsnToFrontier?: (lsn: number) => number | Antichain
  initialLsn?: number; runOn?: 'up-to-date' | 'lsn-advance' | false; debug?: boolean | typeof console.log
}): RootStreamBuilder<[key: string, T]>
declare function outputElectricMessages<K extends string, V, T>(
  fn: (data: ChangeMessage<Row<V>>[]) => void
): (stream: IStreamBuilder<T>) => IStreamBuilder<KeyValue<K, V>>
```

## [04]-[IMPLEMENTATION_LAW]

[DATAFLOW_TOPOLOGY]:
- `D2` is the graph host; `newInput<T>()` returns the only injection point (`RootStreamBuilder`), and `step()`/`run()` process queued messages until `pendingWork()` is false. `finalize()` seals the graph — no operators after.
- `sendData(version, collection)` and `sendFrontier(frontier)` are the two ingress points; every `sendFrontier` advances what downstream operators treat as complete. Call `sendFrontier` after all `sendData` for a version — operators wait for frontier advancement before emitting stable output.
- `Version` is a coordinate tuple with a full lattice API (`join`=least-upper-bound, `meet`=greatest-lower-bound, `lessEqual` for the product partial order); multidimensional versions are partially ordered. Mint through `v()` (cached identity) so a `Version` is safe as a map key — never `new Version()` for a shared key.
- `Antichain` is the frontier (minimal incomparable set); `Antichain.create(value)` is the canonical constructor (`Version[] \| Version \| number \| number[]`); `Frontier` is the variadic-constructor convenience subclass. `advanceBy(frontier)` on a `Version` snaps it forward to the frontier.
- `Index<K, V>` is the join/reduce/reconstruct trace — a key → versions-with-nonzero-multiplicity → `(value, multiplicity)` map. `reconstructAt(key, version)` materializes the value at a requested version (the `query/asof` and `query/window` read); `compact(frontier, keys?)` GCs history below a frontier. Consumer code never instantiates it — `join`/`reduce`/`distinct`/`count` manage it internally.
- `output(fn)` receives full `Message<T>` (`DATA` with `version`+`collection`, or `FRONTIER`) — the version-carrying difference from `d2mini`'s bare-`MultiSet` `output`. `consolidate()` must precede any `output`/keyed operator on a stream whose multiplicities may cancel; `iterate(f)` is fixedpoint and runs forever if `f` never converges.
- `topKWithFractionalIndex` and `orderByWithFractionalIndex` assign lexicographic string indices (via `fractional-indexing`) so a reorder only re-indexes the moved elements — the projection ordering surface a list CRDT consumes without a full re-key.

[ELECTRIC_BRIDGE_LAW]:
- `electricStreamToD2Input({ graph, stream, input, lsnToVersion, lsnToFrontier, runOn })` is the projection ingress: it subscribes an `@electric-sql/client` `ShapeStream` (which MUST run `replica: 'full'` so updates carry the full row), keys each `ChangeMessage` by row primary key into `RootStreamBuilder<[key, T]>`, and maps the Postgres LSN to the event-time `Version`/`Antichain` through `lsnToVersion`/`lsnToFrontier` — this is where the monotone Postgres commit order becomes the dataflow's partial-order time. `runOn: 'up-to-date'` runs the graph once the shape catches up (snapshot-then-live), `'lsn-advance'` runs on every LSN tick (live), `false` leaves `run()` to the caller.
- `outputElectricMessages(fn)` is the egress mirror: it re-serializes the maintained D2 view into `ChangeMessage<Row<V>>[]` batches — the shape a downstream ElectricSQL consumer or a re-published shape reads.

[STACKS_WITH]:
- `effect` (`libs/typescript/.api/effect.md`): the D2 graph is a mutable host, so `projection` wraps it as `Layer.scoped` / an `Effect.acquireRelease` resource; `output(fn)` pushes into an `effect` `Queue`/`SubscriptionRef` that a `Stream.fromQueue` drains into the keyed read model, and `sendData`/`sendFrontier` run inside `Effect.sync`. The `lsnToVersion` mapping composes with `kernel`/`state` `Hlc`/event-time coordinates; `Data.TaggedEnum` models the `Message` `DATA`/`FRONTIER` arms for `Match.exhaustive` dispatch.
- `@electric-sql/d2mini` (`.api/electric-sql-d2mini.md`): the versionless twin — `PipedOperator<I, O>` chains are source-compatible across both, so a transform written once composes in either; `d2ts` owns the frontier/event-time engine (`query/watermark`+`query/window`, `query/asof` reconstruct, `convergence/retention` `Frontier`), `d2mini` owns the fully-arrived/order-insensitive reactive path (`query/reactive`). Choose by frontier requirement, never by preference.
- `@electric-sql/client` (peer, cataloged where `interchange` owns the shape wire): the `ShapeStream` feeding `electricStreamToD2Input` is the transport boundary; `interchange` decodes the C# wire and the LSN, and `projection` never re-mints identity — it maps the already-assembled LSN into a `Version`.
- projection design pages: `query/window` re-founds the signed-delta IVM over this runtime's version trace and `query/asof` reads it via `Index.reconstructAt`; `convergence/retention` advances a `Frontier` antichain and `finalizeBelow` GCs the trace under `compact`; `fast-check` + `@effect/vitest` (`.api/fast-check.md`, `.api/effect-vitest.md`) prove the window-fold and merge laws as mutation-killable specs.

[LOCAL_ADMISSION]:
- Use `@electric-sql/d2ts` (root) when inputs stream and require version-aware completion — event-time windows, as-of reconstruction, frontier-driven retention. Use `@electric-sql/d2mini` when all inputs have arrived and no frontier coordination is needed.
- Ingest ElectricSQL exclusively through `@electric-sql/d2ts/electric` with `replica: 'full'`; never hand-decode `ChangeMessage`s into `sendData` — the adapter owns the keying and the LSN→`Version` mapping.
- Reach the SQLite persistence lane only in a node context (`better-sqlite3`); the browser projection stays on the in-memory `.` core. Inject the DB once via `withSQLite(new BetterSQLite3Wrapper(db))(...operators)` — never thread the handle by hand or construct `SQLIndex` outside the operators; `withSQLite` sets the `SQLiteContext` the persisted operators read.
- Never construct `Index<K, V>` directly — `join`/`reduce`/`distinct`/`count` own the trace; mint shared `Version`s via `v()`, frontiers via `Antichain.create`/`Frontier`.

[RAIL_LAW]:
- Package: `@electric-sql/d2ts`
- Owns: the versioned differential-dataflow graph host, the `Version`/`Antichain`/`Frontier` time domain, version-ordered stream processing, incremental operator computation, and the ElectricSQL/SQLite ingestion bridges
- Accept: `MultiSet<T>` inputs with explicit `Version` coordinates via `sendData`; `Antichain` frontier probes for downstream completion; the `./electric` `ShapeStream` bridge (`replica: 'full'`) as the sole ElectricSQL ingress; the D2 graph wrapped as an Effect scoped resource
- Reject: direct `Index<K, V>` construction; hand-decoding `ChangeMessage`s in place of `electricStreamToD2Input`; `new Version()` for a shared map key where `v()` caches identity; the `./sqlite` lane in a browser context
