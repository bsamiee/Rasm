# [API_CATALOGUE] @electric-sql/d2mini

`@electric-sql/d2mini` is the versionless differential-dataflow runtime — the browser-resident twin of `@electric-sql/d2ts` with the version/frontier/message machinery stripped out. `D2` is a version-free graph host (no `initialFrontier`, no `pushFrontier`/`popFrontier`), `RootStreamBuilder<T>#sendData(collection)` takes only the collection, `MultiSet<T>` carries a reduced instance algebra (the operator graph owns join/reduce), and the flat `Index<K, V>` has no version axis. It exposes the same `pipe`-composable `PipedOperator<I, O>` set as `d2ts` minus the version-dependent operators (`iterate`, `buffer`). `query/reactive` folds a `LiveQuery`/`queryStore` `MultiSet` pipeline into a `SubscriptionRef` here — the path for a fully-arrived or order-insensitive composite view where no frontier coordination is required.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@electric-sql/d2mini`
- package: `@electric-sql/d2mini` (0.1.8, Apache-2.0, © Electric DB Ltd)
- module format: ESM (`type: module`), `sideEffects` unset; single entry `.` → `dist/index.d.ts` (re-exports `d2`, `multiset`, `operators/index`, `types`) — no subpath exports (no `./electric`, no `./sqlite`)
- runtime target: isomorphic, browser-first; no native addon, no peer dependencies
- deps: `fractional-indexing` (the `topKWithFractionalIndex` string keys), `sorted-btree` (backs the ordered structure and the shipped-but-unreachable `orderByBTree`/`topKWithFractionalIndexBTree` variant files — an implementation dependency, never an exported B-tree operator surface), `murmurhash-js` (record hashing); no peer dependencies
- asset: version-free differential-dataflow graph + operators over snapshot-style inputs
- rail: stream / dataflow (the versionless reactive path of `projection`; the event-time twin is `@electric-sql/d2ts`)
- tier: `neutral` (isomorphic, browser-first; the `effect` universal rail is the only stacking peer — d2mini itself imports no `node:*` and carries no runtime dependency the folder does not already admit)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph host and stream builders
- rail: stream / dataflow

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]                 | [CAPABILITY]                                                    |
| :-----: | :--------------------- | :---------------------------- | :-------------------------------------------------------------- |
|  [01]   | `D2`                   | class                         | version-free dataflow graph host (`constructor()` — no options) |
|  [02]   | `StreamBuilder<T>`     | class                         | `IStreamBuilder<T>` base; 1–20-arity typed `pipe` overloads      |
|  [03]   | `RootStreamBuilder<T>` | class (extends StreamBuilder) | input source with `sendData(collection)` only — no version arg   |
|  [04]   | `ID2`                  | interface                     | `newInput`/`step`/`finalize` contract (no `frontier`/`pushFrontier`) |
|  [05]   | `IStreamBuilder<T>`    | interface                     | `pipe(…operators)` (up to 20 typed + variadic), `writer`, `connectReader`, `graph` |
|  [06]   | `PipedOperator<I, O>`  | type alias                    | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>` — source-compatible with `d2ts` |

[PUBLIC_TYPE_SCOPE]: collection family — the root-barrelled value surface
- rail: stream / dataflow
- reachability: only `MultiSet`/`MultiSetArray`/`KeyedData` (`multiset.js`) and `KeyValue` + the `I*` interfaces + `PipedOperator` (`types.js`) are direct root imports — those are the two barrelled files under `index.d.ts` beside `d2.js` and `operators/index.js`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                                                                     |
| :-----: | :-------------------------- | :------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `MultiSet<T>`               | class          | signed-multiplicity collection; reduced instance set `map`/`filter`/`negate`/`concat`/`consolidate`/`extend`/`getInner` + `toJSON`/`fromJSON` — `join`/`reduce`/`count`/`sum`/`min`/`max`/`distinct`/`iterate` live on the operator graph, not the value |
|  [02]   | `MultiSetArray<T>`          | type alias     | `[T, number][]` — the raw multiplicity array `sendData` also accepts                              |
|  [03]   | `KeyedData<T>`              | type alias     | `[key: string, value: T]`                                                                        |
|  [04]   | `KeyValue<K, V>`            | type alias     | `[K, V]`                                                                                          |

[PUBLIC_TYPE_SCOPE]: graph internals — NOT exported (unreachable via the single `.` entry)
- rail: stream / dataflow
- reachability: `graph.js`, `indexes.js`, and `utils.js` sit OUTSIDE the `index.d.ts` barrel AND the package `exports` map lists only `.` (no subpath), so every symbol below is genuinely unreachable to a consumer — they surface only as referenced types in the exported `IStreamBuilder`/`IOperator` signatures and as the bases the operators subclass, reached through `newInput`/`pipe`/`output`, never imported. `Index` is the join/reduce trace the operators own internally; it is not the `d2ts` root-barrelled `Index`.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                                                                     |
| :-----: | :-------------------------- | :------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `Index<K, V>`               | class (internal) | flat key-value join trace (no version axis, `indexes.js`, unbarrelled): `get`→`[V,number][]`, `getMultiplicity`, `entries`, `keys`, `has`, `size`, `addValue(key, value)`, `append`, `join`→`MultiSet<[K,[V,V2]]>` — owned by `join`/`reduce`, never a consumer import |
|  [02]   | `DifferenceStreamReader<T>` | class          | read handle; `drain()` → `MultiSet<T>[]`, `isEmpty()` (no frontier probe)                          |
|  [03]   | `DifferenceStreamWriter<T>` | class          | write handle; `sendData(collection)`, `newReader()` (no `sendFrontier`)                            |
|  [04]   | `Operator<T>`               | abstract class | base operator (no frontier fields)                                                               |
|  [05]   | `UnaryOperator<Tin, Tout>`  | abstract class | single-input operator; `inputMessages()` → `MultiSet<Tin>[]`                                       |
|  [06]   | `BinaryOperator<T>`         | abstract class | two-input operator; `inputAMessages()`/`inputBMessages()`                                          |
|  [07]   | `LinearUnaryOperator<T, U>` | abstract class | stateless linear operator base with `inner(collection)` (the `map`/`negate` base)                 |

[PUBLIC_TYPE_SCOPE]: operator interfaces
- rail: stream / dataflow
- note: there are NO `Message`/`MessageType`/`DataMessage`/`FrontierMessage`/`Version`/`Antichain`/`Frontier` types — the versionless model reads a bare `MultiSet<T>`, not a versioned message

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `IOperator<T>`               | interface     | `run()`, `hasPendingWork()` (no `frontiers()`) |
|  [02]   | `IDifferenceStreamReader<T>` | interface     | `drain()` → `MultiSet<T>[]`, `isEmpty()`       |
|  [03]   | `IDifferenceStreamWriter<T>` | interface     | `sendData(collection)`, `newReader()`          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph lifecycle
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                         |
| :-----: | :--------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `new D2()`                               | constructor    | version-free dataflow graph (no frontier arg) |
|  [02]   | `D2#newInput<T>()`                       | factory        | returns `RootStreamBuilder<T>`       |
|  [03]   | `D2#step()` / `D2#run()`                 | execution      | one incremental step / run to quiescence (`pendingWork()` probes) |
|  [04]   | `D2#finalize()`                          | lifecycle      | seals the graph                      |
|  [05]   | `RootStreamBuilder#sendData(collection)` | data ingestion | sends `MultiSet<T> \| MultiSetArray<T>` — no version arg (the key API difference from `d2ts`) |

[ENTRYPOINT_SCOPE]: piped operators — the versionless operator set (`d2ts` minus `iterate`/`buffer`)
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                                        | [SIGNATURE / PRODUCES]                                        | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------ | :------------------------------- |
|  [01]   | `map<T, O>(f)` / `filter<T>(f)` / `negate<T>()`                 | `PipedOperator<T, …>`                                         | element-wise transform / predicate / multiplicity negate |
|  [02]   | `concat<T, T2>(other)`                                          | `PipedOperator<T, T\|T2>`                                     | stream union                     |
|  [03]   | `consolidate<T>()`                                             | `PipedOperator<T, T>`                                         | merge multiplicity cancellations |
|  [04]   | `output<T>(fn)`                                                 | `(data: MultiSet<T>) => void` → `PipedOperator<T, T>`         | side-effecting observer — receives a bare `MultiSet<T>` (no `Message` wrapper, unlike `d2ts`) |
|  [05]   | `debug<T>(name, indent?)`                                       | `PipedOperator<T, T>`                                         | log each batch under `name`      |
|  [06]   | `keyBy<T, K>(keyFn)` / `unkey<K, V>()` / `rekey<K1, K2, V>(keyFn)` | keying `PipedOperator`s                                    | add / strip / replace key        |
|  [07]   | `filterBy<K, V1>(other)`                                        | `IStreamBuilder<KeyValue<K, unknown>>` → `PipedOperator<T, KeyValue<K, V1>>` | semi-join by key presence |
|  [08]   | `join<K, V1, V2>(other, type?)` + `innerJoin`/`leftJoin`/`rightJoin`/`fullJoin`/`antiJoin` | `PipedOperator<T, KeyValue<K,[…]>>`   | keyed join + typed shorthands    |
|  [09]   | `reduce<K, V1, R>(f)` / `count()`                              | `(stream) => IStreamBuilder<KeyValue<K, …>>`                 | keyed reduce / per-key count — operators here, not `MultiSet` methods |
|  [10]   | `distinct<T>(by?)`                                             | `(by?: (value: T) => any) => (stream) => IStreamBuilder<T>`  | whole-stream dedup with an optional key selector — NOT keyed like `d2ts`'s `distinct` (the one d2mini divergence from the `d2ts` operator set beyond `iterate`/`buffer`) |
|  [11]   | `topK<K, V1>(comparator, options?)` / `topKWithIndex`         | `PipedOperator<T, …>` (`{ limit?, offset? }`)                | per-key ranked limit / with integer rank |
|  [12]   | `topKWithFractionalIndex(comparator, options?)`               | `PipedOperator<T, KeyValue<K, [V1, string]>>`                | ranked limit with a lexicographic fractional index (only moved elements re-index); ships the `indexedValue`/`getValue`/`getIndex` accessors + `IndexedValue`/`FractionalIndex` types a list-CRDT consumer reads |
|  [13]   | `orderBy<T>(valueExtractor, options?)` / `orderByWithIndex` / `orderByWithFractionalIndex` | `(stream) => IStreamBuilder<…>` (`{ comparator?, limit?, offset? }`) | ordered limit within key group, optionally position-annotated |
|  [14]   | `groupBy<T, K, A>(keyExtractor, aggregates?)`                  | `(stream) => IStreamBuilder<KeyValue<string, K & AggResult>>` | multi-aggregate group-by (`aggregates` optional here, required in `d2ts`); aggregates via the `groupByOperators` namespace (`sum`/`count`/`avg`/`min`/`max`/`median`/`mode`) |
|  [15]   | `pipe(...operators)`                                          | curried composition                                          | free-standing `pipe`; `IStreamBuilder#pipe` is the usual site |

## [04]-[IMPLEMENTATION_LAW]

[DATAFLOW_TOPOLOGY]:
- `D2` has no frontier: `new D2()` takes no `initialFrontier`, there is no `pushFrontier`/`popFrontier`/`frontier()`, and it operates over a single implicit time step per `run()` — no partial-order versioning.
- `RootStreamBuilder#sendData(collection)` takes only the collection (no `version`) — the defining API difference from `@electric-sql/d2ts`. There is no `sendFrontier`.
- `Index<K, V>` (`indexes.js`) is the internal, flat join/reduce trace — it is NOT exported (`indexes.js` is unbarrelled and the `exports` map lists only `.`, so no consumer can import it). Internally it is version-free: `get(key)` returns `[V, number][]` directly, `getMultiplicity(key, value)` reads one multiplicity, and `join` produces a `MultiSet<[K, [V, V2]]>` rather than a version-indexed result — no `reconstructAt`/`versions`/`compact`. Consumer code touches it only through `join`/`reduce`.
- `MultiSet<T>` carries only `map`/`filter`/`negate`/`concat`/`consolidate`/`extend`/`getInner` (+ `toJSON`/`fromJSON`); the `join`/`reduce`/`count`/`sum`/`min`/`max`/`distinct`/`iterate` of the `d2ts` `MultiSet` are absent because the operator graph owns that logic.
- `distinct` diverges from `d2ts`: `distinct<T>(by?: (value: T) => any)` dedups the WHOLE stream (optionally keyed by the `by` selector) and returns `IStreamBuilder<T>`, whereas `d2ts`'s `distinct` is keyed and returns `IStreamBuilder<KeyValue<K, V>>`. A pipeline ported from `d2ts` that relied on keyed `distinct` must re-key explicitly here.
- `output(fn)` receives a bare `MultiSet<T>`, not a `Message<T>` — there is no version to carry, so an observer reads the consolidated batch directly. `consolidate()` remains the prerequisite before `output`/keyed operators when multiplicities may cancel within a batch.
- No B-tree operator surface is reachable. `sorted-btree` backs the ordered structure and the shipped `orderByBTree.js`/`topKWithFractionalIndexBTree.js` files (`orderByWithFractionalIndexBTree`, `topKWithFractionalIndexBTree`, `loadBTree`), but those files are BOTH unbarrelled (`operators/index.js` never re-exports them) AND gated out by the single-`.` `exports` map — genuinely unreachable. The reachable fractional-index operators are `topKWithFractionalIndex`/`orderByWithFractionalIndex` (shared with `d2ts`).

[STACKS_WITH]:
- `effect` (`libs/typescript/.api/effect.md`): `query/reactive`'s `queryStore` builds one `D2` graph as a scoped resource — `input.pipe(query.pipeline, consolidate(), output(consolidatedSink(ref)))` then `finalize()` — and folds the terminal `output` into a `SubscriptionRef<HashMap<string, Out>>` the `@effect-atom/atom` bridge exposes at the `ui` boundary. `consolidatedSink` reads the reduced batch's `MultiSet.getInner()` `[[key, value], multiplicity][]` pairs and nets each into the map (positive net → `HashMap.set`, non-positive → `HashMap.remove`), discharging through `Effect.runSync(Ref.update(...))` at the one synchronous sink interop seam. The source drives `Stream.runForEach(event => Effect.sync(() => { input.sendData(new MultiSet([[event, 1]])); graph.run() }))` under `fold/policy#STREAM_POLICY` `withPolicy` and `Effect.forkScoped`. No frontier means no `Deferred`/completion coordination — the graph runs to quiescence per arrived chunk because every input is implicitly complete at `run`.
- `@electric-sql/d2ts` (`.api/electric-sql-d2ts.md`): the frontier-tracking twin. A `PipedOperator<I, O>` written for one composes in the other (source-compatible), so a shared transform library serves both paths; `d2mini` handles the fully-arrived/order-insensitive composite view, `d2ts` the event-time windowed/as-of engine. The windowed IVM never re-founds here, and a `d2mini` graph never carries a `Version`.
- projection design pages: `query/reactive` is the sole `d2mini` consumer — a versionless reactive query folded into a `SubscriptionRef`; it is disjoint by frontier requirement from `query/watermark`+`query/window` (which are `d2ts`). `fast-check` + `@effect/vitest` (`.api/fast-check.md`, `.api/effect-vitest.md`) prove the reactive fold's snapshot equivalence.

[LOCAL_ADMISSION]:
- Use `@electric-sql/d2mini` when all inputs arrive before the graph runs and no frontier coordination is needed — primarily in-process view maintenance over a fully-arrived snapshot. Use `@electric-sql/d2ts` when streaming inputs require version-aware completion signaling.
- The `pipe` API and most operators are source-compatible with `@electric-sql/d2ts` — operators typed as `PipedOperator<I, O>` compose identically across both — with two divergences: d2mini has no `iterate`/`buffer` (version-dependent), and its `distinct<T>(by?)` is whole-stream rather than keyed. Re-key an explicitly-keyed `distinct` when porting from `d2ts`.
- `consolidate()` remains a prerequisite before `output()` and keyed operators when multiplicities may cancel within a batch.
- Read `output` as `(MultiSet<T>) => void`; never expect a `Message`/`Version` — those do not exist here. Never import `Index` (unreachable, operator-owned). For fractional ordering, use `topKWithFractionalIndex`/`orderByWithFractionalIndex` (the B-tree variant files ship but are unreachable via the single `.` export).

[RAIL_LAW]:
- Package: `@electric-sql/d2mini`
- Owns: version-free differential-dataflow graph execution and incremental operator computation over snapshot-style inputs
- Accept: `MultiSet<T>` batch inputs via `RootStreamBuilder#sendData` (no version); `PipedOperator<I, O>` chains for view computation, wrapped as an Effect scoped resource folded into a `SubscriptionRef`
- Reject: calling `sendFrontier` or passing `Version`/`Antichain` arguments (no frontier protocol — use `@electric-sql/d2ts`); expecting a `Message`-wrapped `output`; reaching for a B-tree operator export that does not exist
