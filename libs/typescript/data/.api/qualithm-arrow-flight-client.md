# [TS_DATA_API_QUALITHM_ARROW_FLIGHT_CLIENT]

`@qualithm/arrow-flight-client` speaks the Arrow Flight and Flight SQL wire for the OLAP lane: `FlightSqlClient` wraps `FlightClient`, runs queries, updates, prepared statements, transactions, and metadata discovery, and decodes every result onto `apache-arrow` `Table`/`RecordBatch`. `lane/olap` consumes it as an engine-blind columnar ingress/egress row beside the ClickHouse and DuckDB rows, its transport reusing the one `@connectrpc/connect` stack over `node:http2` rather than a second gRPC client.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@qualithm/arrow-flight-client`
- package: `@qualithm/arrow-flight-client` (Apache-2.0)
- module: `exports["."]` (node/bun/deno) + `./testing` helper subpath; `types` first-class, single build
- runtime: `runtime:node` services and CLI — rides `node:http2`, no browser row
- depends: `@connectrpc/connect`/`@connectrpc/connect-node` transport, `@bufbuild/protobuf` Flight descriptors, `apache-arrow` column plane
- rail: `lane/olap` Flight SQL row — no Effect peer; the lane owns the boundary-kernel wrap

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two clients, their config and auth union, the decode discriminant, and the SQL + error shapes

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :--------------------------------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `FlightClient`                                       | class         | low-level RPC — `getFlightInfo`/`doGet`/`doPut`/`doAction`       |
|  [02]   | `FlightSqlClient`                                    | class         | composes `FlightClient`; query/execute/prepare, txn, metadata    |
|  [03]   | `FlightClientOptions`                                | struct        | `url`/`headers?`/`timeoutMs?`/`auth?`/`tls?`/`nodeOptions?`      |
|  [04]   | `AuthOptions`                                        | union         | `bearer` token / `basic` handshake / `none`                      |
|  [05]   | `DecodedFlightData`                                  | union         | `type: "schema"\|"batch"\|"empty"` — keep-alive-aware decode arm |
|  [06]   | `PreparedStatement` / `Transaction` / `UpdateResult` | struct        | prepared/txn handles, param schema, affected `recordCount`       |
|  [07]   | `FlightError` (+ family)                             | class         | `isError`-guarded tagged failures the lane boundary lifts        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory construction, SQL execution and transaction driver, metadata discovery, the Arrow IPC codec pair

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `createFlightClient(FlightClientOptions) -> FlightClient`       | factory  | low-level client construction                       |
|  [02]   | `createFlightSqlClient(FlightClientOptions) -> FlightSqlClient` | factory  | SQL client construction                             |
|  [03]   | `new FlightSqlClient(FlightClientOptions \| FlightClient)`      | ctor     | wrap options or an existing `FlightClient`          |
|  [04]   | `query` / `queryBatches` / `queryStream`                        | instance | `Table` / `RecordBatch` stream / raw `FlightData`   |
|  [05]   | `executeUpdate` / `prepare` / `executePrepared*`                | instance | DML/DDL and prepared-statement execution            |
|  [06]   | `beginTransaction` / `commit` / `rollback`                      | instance | transaction lifecycle over `transactionId`          |
|  [07]   | `getCatalogs` / `getDbSchemas` / `getTables` / `getPrimaryKeys` | instance | Flight SQL metadata discovery → `Table`             |
|  [08]   | `decodeFlightDataToTable` / `decodeFlightDataStream`            | static   | `FlightData` → `Table` / `RecordBatch` stream       |
|  [09]   | `encodeRecordBatchesToFlightData` / `encodeTableToFlightData`   | static   | Arrow batches/`Table` → `FlightData` for `doPut`    |
|  [10]   | `DEFAULT_TIMEOUT_MS`                                            | static   | request-timeout default the config resolves against |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `lane/olap` consumes the Flight SQL row as engine-blind columnar ingress/egress; the server engine stays opaque behind the wire.
- every result decodes zero-copy onto `apache-arrow` `Table`/`RecordBatch`; a row re-materialization never intervenes.
- one `@connectrpc/connect` transport over `node:http2` carries every RPC; a second gRPC stack never enters the lane.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): `decodeFlightDataToTable`/`decodeFlightDataStream` land `FlightData` on `Table`/`RecordBatch`, `encodeRecordBatchesToFlightData`/`encodeTableToFlightData` re-encode for `doPut` — the same IPC interchange the DuckDB and ClickHouse rows carry.
- `@duckdb/node-api`(`.api/duckdb-node-api.md`): the Flight SQL row reaches a remote Arrow-Flight-speaking engine; below the distributed trigger the embedded row owns the workload.
- `lane/olap`: boundary-kernel wrapping scopes construction in an acquire-release graph with `close()` on release and lifts the `FlightError` family through `isError` guards into typed lane faults.

[LOCAL_ADMISSION]:
- `scope:data`, node lane — the client rides `node:http2`, scoped in the lane's acquire-release graph, `close()` on release.
- OLAP rows are correctness-adjacent, never the record of truth — they read and write columns; nothing folds back as authority.

[RAIL_LAW]:
- Package: `@qualithm/arrow-flight-client`
- Owns: the Flight SQL wire of the OLAP lane — client construction, `query`/`executeUpdate`/prepared/transaction/metadata, the Arrow IPC codecs, the `FlightError` family
- Accept: the engine-blind columnar ingress/egress row on `@connectrpc/connect` transport, `Table`/`RecordBatch` decode, scoped acquire-release with `close()` on release
- Reject: a second gRPC stack beside connect, row-materialized re-encoding off the Arrow plane, the Flight client as a record of truth, an unscoped client leak
