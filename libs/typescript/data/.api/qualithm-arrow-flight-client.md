# [TS_DATA_API_QUALITHM_ARROW_FLIGHT_CLIENT]

`@qualithm/arrow-flight-client` speaks the Arrow Flight and Flight SQL wire for the analytical lane: `FlightSqlClient` wraps `FlightClient` and returns results as `apache-arrow` `Table`/`RecordBatch`, executing updates, prepared statements, and transactions. Its transport rides the estate `@connectrpc/connect` substrate over `node:http2`, so Flight RPC reuses the one connect stack rather than a second gRPC client. Engine-blind by design: `lane/olap` consumes it as a columnar ingress/egress row beside the ClickHouse and DuckDB rows, every decode landing zero-copy on Arrow columns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@qualithm/arrow-flight-client`
- package: `@qualithm/arrow-flight-client` (Apache-2.0)
- backing: `@connectrpc/connect` + `@connectrpc/connect-node` transport, `@bufbuild/protobuf` Flight descriptors, `node:http2` session
- column-plane: `apache-arrow` `Table`/`RecordBatch` — the decode and encode surface every result crosses
- runtime: `runtime:node`/bun services and CLI; the client rides `node:http2`, no browser row
- rail: `lane/olap` Flight SQL row — no Effect peer; boundary-kernel wrap is the lane's

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two clients, their config, and the decode + error shapes
- rail: data/lane
- `FlightSqlClient` composes a `FlightClient` (owns or wraps one) and reaches core Flight RPC through it; `FlightClientOptions` is the single config both factories take, and the `FlightError` family carries `isError` type guards the lane lifts into typed faults.

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                         |
| :-----: | :--------------------------------------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `FlightClient`                                       | low-level client | Flight RPC — `getFlightInfo`/`doGet`/`doPut`/`doAction`     |
|  [02]   | `FlightSqlClient`                                    | SQL client       | `query`/`executeUpdate`/`prepare` + transaction driver      |
|  [03]   | `FlightClientOptions`                                | config           | `url`/`headers?`/`timeoutMs?`/`auth?`/`tls?`/`nodeOptions?` |
|  [04]   | `AuthOptions`                                        | auth union       | `bearer` token / `basic` handshake / `none`                 |
|  [05]   | `DecodedFlightData`                                  | decode result    | `schema` / `batch` / `empty` keep-alive discriminant        |
|  [06]   | `PreparedStatement` / `Transaction` / `UpdateResult` | SQL shapes       | prepared handle, txn handle, affected-row count             |
|  [07]   | `FlightError` (+ family)                             | error base       | `isError`-guarded tagged failures the lane boundary lifts   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory construction, SQL execution, Arrow IPC codecs
- rail: lane/olap
- `FlightClientOptions.url` includes protocol; `auth` is one `AuthOptions` arm; `tls` carries mTLS PEM material; `timeoutMs` defaults to `DEFAULT_TIMEOUT_MS`. Every SQL result decodes to `apache-arrow` through the codec helpers, and `close()` releases the client.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                 |
| :-----: | :------------------------------------------------------------ | :------------- | :-------------------------------------------------- |
|  [01]   | `createFlightClient(options)` → `FlightClient`                | factory        | low-level client construction                       |
|  [02]   | `createFlightSqlClient(options)` → `FlightSqlClient`          | factory        | SQL client construction                             |
|  [03]   | `new FlightSqlClient(optionsOrClient)`                        | ctor           | wrap options or an existing `FlightClient`          |
|  [04]   | `query` / `queryBatches` / `queryStream`                      | read           | `Table` / `RecordBatch` stream / raw `FlightData`   |
|  [05]   | `executeUpdate` / `prepare` / `executePrepared*`              | write          | DML/DDL and prepared-statement execution            |
|  [06]   | `decodeFlightDataToTable` / `decodeFlightDataStream`          | decode         | `FlightData` → Arrow `Table` / `RecordBatch` stream |
|  [07]   | `encodeRecordBatchesToFlightData` / `createFlightDataFromIpc` | encode         | Arrow → `FlightData` for `doPut`                    |
|  [08]   | `DEFAULT_TIMEOUT_MS`                                          | constant       | request-timeout default the config resolves against |

## [04]-[IMPLEMENTATION_LAW]

[FLIGHT_TOPOLOGY]:
- engine-blind columnar wire — `lane/olap` consumes the Flight SQL row as ingress/egress beside the ClickHouse and DuckDB rows; the server engine stays opaque behind the wire.
- Arrow is the column plane — `decodeFlightDataToTable`/`decodeFlightDataStream` land on `apache-arrow` `Table`/`RecordBatch` zero-copy; a row re-materialization never intervenes.
- one connect transport — `@connectrpc/connect` over `node:http2` carries every RPC; a second gRPC stack never enters the lane.

[INTEGRATION_LAW]:
- Stack on `apache-arrow` (`../../ui/.api/apache-arrow.md`): results decode to `Table`/`RecordBatch` and encode from them — the same Arrow interchange the DuckDB and ClickHouse rows carry, never a bespoke row codec.
- Stack across `data`: the Flight SQL row reaches an Arrow-Flight-speaking engine (analytical serving, a remote columnar backend); below the distributed trigger the embedded `.api/duckdb-node-api.md` row owns the workload.
- Stack with the error family: `FlightError`/`FlightAuthError`/`FlightConnectionError`/`FlightServerError`/`FlightTimeoutError`/`FlightCancelledError` carry `isError` guards the boundary-kernel lifts into typed lane faults, never a raw throw.

[LOCAL_ADMISSION]:
- `scope:data`, node lane — the client rides `node:http2`; construction is scoped in the lane's acquire-release graph, `close()` on release.
- OLAP lane is correctness-adjacent, never the record of truth — analytical rows read and write columns; nothing folds back as authority.

[RAIL_LAW]:
- Package: `@qualithm/arrow-flight-client`
- Owns: the Flight SQL wire of the analytical lane — `createFlightClient`/`createFlightSqlClient`, `query`/`executeUpdate`/prepared/transaction, the Arrow IPC codecs, the `FlightError` family
- Accept: the engine-blind columnar ingress/egress row on `@connectrpc/connect` transport, Arrow `Table`/`RecordBatch` decode, scoped acquire-release with `close()` on release
- Reject: a second gRPC stack beside connect, row-materialized re-encoding off the Arrow plane, the Flight client as a record of truth, an unscoped client leak
