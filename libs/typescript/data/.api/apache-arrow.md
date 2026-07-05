# [apache-arrow] — the ONE columnar wire of the data plane: zero-copy interchange between the OLAP rows, the pg spine, and the viewer

`apache-arrow` is the single columnar interchange every analytical engine row meets at: DuckDB node and wasm emit and ingest Arrow, ClickHouse outputs Arrow format, and the viewer's geoarrow plane consumes the same tables — no row-materialization tax anywhere between engines. The data folder composes only the interchange surface: `Table` as the in-memory columnar value, IPC serialization both directions, and streaming record-batch readers. The branch-wide catalogue is the ui folder's (`libs/typescript/ui/.api/apache-arrow.md`); this overlay records the data-lane seam facts only.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `apache-arrow`
- package: `apache-arrow`
- version: `21.1.0`
- license: `Apache-2.0`
- runtime: universal (dom/node builds); the wasm OLAP row peers it
- rail: `lane/olap` interchange — Table/IPC/record-batch surface only

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY]  | [CONSUMER / BOUNDARY]                                        |
| :-----: | :-------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `Table<T>`                                                      | columnar value  | the interchange value between DuckDB rows, ClickHouse, viewer |
|  [02]   | `RecordBatchReader` / `RecordBatchStreamReader` / `AsyncRecordBatchStreamReader` | batch stream | incremental IPC consumption — the lane's `Stream` lift |
|  [03]   | `Schema` / `Field`                                              | shape           | column typing carried inside the table value                 |

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                          |
| :-----: | :------------------------------------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `tableFromIPC(source)` → `Table` (sync bytes or async stream sources)                       | decode wire    | ClickHouse/remote Arrow output into a live table |
|  [02]   | `tableToIPC(table)` → bytes                                                                 | encode wire    | shipping a table to the wasm row or the viewer   |
|  [03]   | `tableFromArrays(input)` / `makeTable(input)`                                               | construct      | typed-array column assembly at a boundary        |
|  [04]   | `RecordBatchReader.from(source)`                                                            | stream decode  | bounded-memory batch iteration                   |

## [04]-[IMPLEMENTATION_LAW]

[INTEGRATION_LAW]:
- One wire law: an analytical result crossing any engine seam travels as Arrow (`Table` in memory, IPC on the wire); a JSON/row re-encoding between engines is the named defect.
- The DuckDB wasm row returns `arrow.Table` natively and ingests via `insertArrowTable`/`insertArrowFromIPCStream` (`.api/duckdb-wasm.md`); the node row and ClickHouse meet the same wire at the lane seam.

[RAIL_LAW]:
- Package: `apache-arrow`
- Owns: the columnar interchange value and its IPC codecs at the data-lane seams
- Accept: `Table`/IPC/record-batch interchange between engine rows and to the viewer
- Reject: per-engine bespoke result shapes, row-materialized inter-engine transfer, a second columnar vocabulary
