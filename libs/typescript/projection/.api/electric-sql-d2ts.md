# [API_CATALOGUE] @electric-sql/d2ts

`@electric-sql/d2ts` supplies a full differential dataflow runtime for TypeScript: `D2` (the dataflow graph host with versioned frontier tracking), `RootStreamBuilder<T>` (typed input source), a `pipe`-composable `IStreamBuilder<T>` interface, `MultiSet<T>` (the signed-multiplicity collection), `Version`/`Antichain`/`Frontier` (the partial-order time domain), `Index<K, V>` (key-versioned trace index for join and reduce), and a complete operator library covering `map`, `filter`, `join`, `reduce`, `count`, `distinct`, `consolidate`, `negate`, `concat`, `iterate`, `keyBy`/`unkey`/`rekey`, `topK`, `orderBy`, `groupBy`, and `output`. The projection convergence layer uses this runtime to maintain incremental views over D2 change streams from ElectricSQL.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@electric-sql/d2ts`
- package: `@electric-sql/d2ts`
- module: `.` → `dist/index.d.ts`; also `./sqlite` and `./electric` sub-entries
- asset: `dist/index.d.ts` (re-exports `d2`, `order`, `multiset`, `version-index`, `operators/index`, `types`)
- rail: stream / dataflow

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph host and stream builders
- rail: stream / dataflow

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]                 | [CAPABILITY]                                                      |
| :-----: | :--------------------- | :---------------------------- | :---------------------------------------------------------------- |
|   [1]   | `D2`                   | class                         | dataflow graph host; versioned step execution                     |
|   [2]   | `D2Options`            | type                          | `{ initialFrontier: Antichain \| Version \| number \| number[] }` |
|   [3]   | `StreamBuilder<T>`     | class                         | `IStreamBuilder<T>` implementation                                |
|   [4]   | `RootStreamBuilder<T>` | class (extends StreamBuilder) | input source; `sendData` and `sendFrontier`                       |
|   [5]   | `ID2`                  | interface                     | `newInput`, `step`, `finalize`, `frontier` contract               |
|   [6]   | `IStreamBuilder<T>`    | interface                     | `pipe(…operators)`, `writer`, `connectReader`, `graph`            |
|   [7]   | `PipedOperator<I, O>`  | type alias                    | `(stream: IStreamBuilder<I>) => IStreamBuilder<O>`                |

[PUBLIC_TYPE_SCOPE]: collection and time domain
- rail: stream / dataflow

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]             | [CAPABILITY]                                        |
| :-----: | :----------------- | :------------------------ | :-------------------------------------------------- |
|   [1]   | `MultiSet<T>`      | class                     | signed-multiplicity collection with map/filter/join |
|   [2]   | `MultiSetArray<T>` | type alias                | `[T, number][]` — raw multiplicity array            |
|   [3]   | `KeyedData<T>`     | type alias                | `[key: string, value: T]`                           |
|   [4]   | `KeyValue<K, V>`   | type alias                | `[K, V]` — canonical keyed-stream element shape     |
|   [5]   | `Version`          | class                     | totally/partially ordered time tuple                |
|   [6]   | `Antichain`        | class                     | minimal set of incomparable versions (frontier)     |
|   [7]   | `Frontier`         | class (extends Antichain) | named antichain convenience alias                   |
|   [8]   | `v(version)`       | factory                   | cached `Version` factory (`number \| number[]`)     |

[PUBLIC_TYPE_SCOPE]: graph edges and operator base classes
- rail: stream / dataflow

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                                                  |
| :-----: | :-------------------------- | :------------- | :------------------------------------------------------------ |
|   [1]   | `DifferenceStreamReader<T>` | class          | read handle; `drain()`, `isEmpty()`, `probeFrontierLessThan`  |
|   [2]   | `DifferenceStreamWriter<T>` | class          | write handle; `sendData`, `sendFrontier`, `newReader`         |
|   [3]   | `Operator<T>`               | abstract class | base multi-input operator with frontier tracking              |
|   [4]   | `UnaryOperator<T>`          | abstract class | single-input operator base                                    |
|   [5]   | `BinaryOperator<T>`         | abstract class | two-input operator base                                       |
|   [6]   | `IOperator<T>`              | interface      | `run()`, `hasPendingWork()`, `frontiers()`                    |
|   [7]   | `Index<K, V>`               | class          | key-versioned trace index; `reconstructAt`, `join`, `compact` |
|   [8]   | `IndexType<K, V>`           | interface      | `Index` contract                                              |

[PUBLIC_TYPE_SCOPE]: message protocol
- rail: stream / dataflow

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]       | [CAPABILITY]                                        |
| :-----: | :---------------- | :------------------ | :-------------------------------------------------- |
|   [1]   | `Message<T>`      | discriminated union | `DATA` (version + collection) or `FRONTIER` message |
|   [2]   | `DataMessage<T>`  | type                | `{ version: Version; collection: MultiSet<T> }`     |
|   [3]   | `FrontierMessage` | type alias          | `Version \| Antichain`                              |
|   [4]   | `MessageType`     | const enum-like     | `DATA = 1`, `FRONTIER = 2`                          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph lifecycle
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `new D2({ initialFrontier })`                       | constructor    | creates dataflow graph with initial frontier |
|   [2]   | `D2#newInput<T>()`                                  | factory        | returns `RootStreamBuilder<T>` input source  |
|   [3]   | `D2#step()`                                         | execution      | runs one incremental step                    |
|   [4]   | `D2#run()`                                          | execution      | runs all pending steps to quiescence         |
|   [5]   | `D2#finalize()`                                     | lifecycle      | advances frontier to signal completion       |
|   [6]   | `D2#frontier()`                                     | query          | returns current `Antichain` output frontier  |
|   [7]   | `D2#pushFrontier(newFrontier)` / `D2#popFrontier()` | scope          | nested frontier scope management             |
|   [8]   | `RootStreamBuilder#sendData(version, collection)`   | data ingestion | sends versioned `MultiSet` into the graph    |
|   [9]   | `RootStreamBuilder#sendFrontier(frontier)`          | frontier push  | advances the input frontier                  |

[ENTRYPOINT_SCOPE]: piped operators — transforms
- rail: stream / dataflow

| [INDEX] | [SURFACE]                 | [SIGNATURE]                  | [PRODUCES]                                |
| :-----: | :------------------------ | :--------------------------- | :---------------------------------------- |
|   [1]   | `map<T, O>(f)`            | `(data: T) => O`             | `PipedOperator<T, O>`                     |
|   [2]   | `filter<T>(f)`            | `(data: T) => boolean`       | `PipedOperator<T, T>`                     |
|   [3]   | `negate<T>()`             | —                            | `PipedOperator<T, T>`                     |
|   [4]   | `concat<T, T2>(other)`    | `IStreamBuilder<T2>`         | `PipedOperator<T, T\|T2>`                 |
|   [5]   | `consolidate<T>()`        | —                            | `PipedOperator<T, T>`                     |
|   [6]   | `output<T>(fn)`           | `(data: Message<T>) => void` | `PipedOperator<T, T>`                     |
|   [7]   | `keyBy<T, K>(keyFn)`      | `(value: T) => K`            | `PipedOperator<T, Keyed<K, T>>`           |
|   [8]   | `unkey<K, V>()`           | —                            | `PipedOperator<Keyed<K,V>, V>`            |
|   [9]   | `rekey<K1, K2, V>(keyFn)` | `(value: V) => K2`           | `PipedOperator<Keyed<K1,V>, Keyed<K2,V>>` |
|  [10]   | `iterate<T>(f)`           | `(stream) => stream`         | `PipedOperator<T, T>` (fixedpoint)        |

[ENTRYPOINT_SCOPE]: piped operators — keyed aggregations
- rail: stream / dataflow

| [INDEX] | [SURFACE]                                    | [PRODUCES]                                                    | [CAPABILITY]                      |
| :-----: | :------------------------------------------- | :------------------------------------------------------------ | :-------------------------------- |
|   [1]   | `join<K, V1, V2>(other, type?)`              | `PipedOperator<T, KeyValue<K,[V1\|null,V2\|null]>>`           | inner/left/right/full/anti join   |
|   [2]   | `innerJoin<K, V1, V2>(other)`                | `PipedOperator<T, KeyValue<K,[V1,V2]>>`                       | inner join shorthand              |
|   [3]   | `leftJoin<K, V1, V2>(other)`                 | `PipedOperator<T, KeyValue<K,[V1,V2\|null]>>`                 | left outer join                   |
|   [4]   | `rightJoin<K, V1, V2>(other)`                | `PipedOperator<T, KeyValue<K,[V1\|null,V2]>>`                 | right outer join                  |
|   [5]   | `antiJoin<K, V1, V2>(other)`                 | `PipedOperator<T, KeyValue<K,[V1,null]>>`                     | anti-join (left minus right)      |
|   [6]   | `fullJoin<K, V1, V2>(other)`                 | `PipedOperator<T, KeyValue<K,[V1\|null,V2\|null]>>`           | full outer join                   |
|   [7]   | `reduce<K, V1, R>(f)`                        | `(stream) => IStreamBuilder<KeyValue<K,R>>`                   | keyed reduce with multiplicity    |
|   [8]   | `count()`                                    | `(stream) => IStreamBuilder<KeyValue<K,number>>`              | per-key count                     |
|   [9]   | `distinct()`                                 | `(stream) => IStreamBuilder<KeyValue<K,V>>`                   | deduplicate by key                |
|  [10]   | `topK<K, V1>(comparator, options?)`          | `PipedOperator<T, T>`                                         | limit per-key group by comparator |
|  [11]   | `orderBy<T>(valueExtractor, options?)`       | `(stream) => IStreamBuilder<T>`                               | order within key group with limit |
|  [12]   | `groupBy<T, K, A>(keyExtractor, aggregates)` | `(stream) => IStreamBuilder<KeyValue<string, K & AggResult>>` | multi-aggregate group-by          |

[ENTRYPOINT_SCOPE]: `groupBy` aggregate functions
- rail: stream / dataflow
- access: the aggregate functions are NOT root-level exports — they are reached through the `groupByOperators` namespace (`groupByOperators.sum`, `groupByOperators.avg`, …); only `groupBy` and `groupByOperators` are barrelled from `operators/index`.

| [INDEX] | [SURFACE]                                     | [PRODUCES]                                            | [CAPABILITY]        |
| :-----: | :-------------------------------------------- | :--------------------------------------------------- | :------------------ |
|   [1]   | `groupByOperators.sum<T>(valueExtractor?)`    | `AggregateFunction<T, number, number>`               | numeric sum         |
|   [2]   | `groupByOperators.count<T>()`                 | `AggregateFunction<T, number, number>`               | element count       |
|   [3]   | `groupByOperators.avg<T>(valueExtractor?)`    | `AggregateFunction<T, number, { sum; count }>`       | arithmetic mean     |
|   [4]   | `groupByOperators.min<T>(valueExtractor?)`    | `AggregateFunction<T, number, number>`               | minimum value       |
|   [5]   | `groupByOperators.max<T>(valueExtractor?)`    | `AggregateFunction<T, number, number>`               | maximum value       |
|   [6]   | `groupByOperators.median<T>(valueExtractor?)` | `AggregateFunction<T, number, number[]>`             | median candidates   |
|   [7]   | `groupByOperators.mode<T>(valueExtractor?)`   | `AggregateFunction<T, number, Map<number, number>>`  | frequency map       |

## [4]-[IMPLEMENTATION_LAW]

[DATAFLOW_TOPOLOGY]:
- `D2` is the graph host; `newInput<T>()` returns a `RootStreamBuilder` that is the only way to inject data; the graph executes via `step()` / `run()` which processes queued messages
- `sendData(version, collection)` and `sendFrontier(frontier)` on `RootStreamBuilder` are the two external ingestion points; every `sendFrontier` advances what downstream operators consider complete
- `pipe(...operators)` on `IStreamBuilder<T>` is the composition surface; each operator is a `PipedOperator<I, O>` function — a plain function that wires one stream builder to another
- `Index<K, V>` is the internal join/reduce state (partially-ordered key-version-value trace); consumer code does not instantiate it directly
- `Version` objects are cached via `v()` factory — use `v()` instead of `new Version()` when sharing version instances as map keys
- `Antichain.create(value)` is the canonical antichain constructor accepting `Version[]`, `Version`, `number`, or `number[]`

[LOCAL_ADMISSION]:
- The `.` export covers all operators; `./sqlite` and `./electric` are adapter sub-entries for SQLite and ElectricSQL ingestion bridges
- `RootStreamBuilder.sendFrontier` must be called after all `sendData` for a version; operators wait for frontier advancement before emitting stable output
- `consolidate()` must precede any `output()` or keyed operator on streams whose multiplicities may cancel — it merges `(record, m)` pairs with matching records into one canonical pair
- `iterate(f)` implements dataflow fixedpoint; if `f` never converges the runtime runs forever

[RAIL_LAW]:
- Package: `@electric-sql/d2ts`
- Owns: differential dataflow graph host, version-ordered stream processing, and incremental operator computation
- Accept: `MultiSet<T>` inputs with explicit `Version` coordinates; `Antichain` frontier probes for downstream completion
- Reject: direct construction of `Index<K, V>` from consumer code; use `reduce`/`join`/`distinct`/`count` operators which manage index state internally
