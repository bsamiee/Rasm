# [TS_DATA_API_QUALITHM_ARROW_FLIGHT_CLIENT]

`@qualithm/arrow-flight-client` speaks the Arrow Flight and Flight SQL wire for the OLAP lane: `FlightSqlClient` wraps `FlightClient`, runs queries, updates, prepared statements, transactions, and metadata discovery, and decodes every result onto `apache-arrow` `Table`/`RecordBatch`. `lane/olap` consumes it as an engine-blind columnar ingress/egress row beside the ClickHouse and DuckDB rows, its transport reusing the one `@connectrpc/connect` stack over `node:http2` rather than a second gRPC client.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the two clients, their config, credential, and addressing shapes, the decode discriminant, and the SQL + error shapes

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `FlightClient`                                        | class         | low-level RPC; `close()` returns `void`, never a promise        |
|  [02]   | `FlightSqlClient`                                     | class         | composes `FlightClient`, reached through its `flight` accessor  |
|  [03]   | `FlightClientOptions` / `ResolvedFlightClientOptions` | struct        | `url`/`headers?`/`timeoutMs?`/`auth?`/`authProvider?`/`tls?`    |
|  [04]   | `TlsOptions` / `BasicAuthCredentials`                 | struct        | mTLS `cert`/`key`/`ca` PEM-or-`Buffer`, `passphrase`; user+pass |
|  [05]   | `AuthOptions`                                         | union         | `bearer` / `basic` handshake / `none`; token a bare `string`    |
|  [06]   | `AuthProvider`                                        | delegate      | `AuthOptions` thunk, sync or promised; outranks `auth`          |
|  [07]   | `ExecuteQueryOptions` / `ExecuteUpdateOptions`        | struct        | `{ transactionId?: Uint8Array }` — the one call-scoped knob     |
|  [08]   | `DecodedFlightData`                                   | union         | `type: "schema"\|"batch"\|"empty"` — keep-alive-aware arm       |
|  [09]   | `PreparedStatement` / `Transaction` / `UpdateResult`  | struct        | prepared/txn handles, param schema, affected `recordCount`      |
|  [10]   | `FlightError` (+ family)                              | class         | `isError`-guarded tagged failures the lane boundary lifts       |
|  [11]   | `FlightDescriptorInput` / `FlightTicket`              | union/struct  | `{type:"path",path}\|{type:"cmd",cmd}` and `{ticket}` — plain   |
|  [12]   | `FlightCriteria` / `FlightAction`                     | struct        | `listFlights` filter; `doAction` `{type, body?}` request        |
|  [13]   | `FlightInfo` / `PollInfo` / `FlightEndpoint`          | message       | plan, progress, and the per-split endpoints `doGet` redeems     |
|  [14]   | `FlightData` / `FlightDescriptor` / `Ticket`          | message       | wire frames, type-only at the root; `Schema` values unreachable |
|  [15]   | `ActionType` / `Result` / `SchemaResult`              | message       | action vocabulary, action results, IPC schema bytes             |

- `FlightClientOptions.nodeOptions` carries the `node:http2` connect knobs straight through to `createGrpcTransport`, the one leg of the record the client forwards to the transport beside `baseUrl`.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory construction, SQL execution and transaction driver, the low-level RPC set, metadata discovery, the Arrow IPC codec pairs

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------------ | :------- | :---------------------------------------------------- |
|  [01]   | `createFlightClient(options) -> FlightClient`                       | factory  | low-level construction, SYNCHRONOUS — never a promise |
|  [02]   | `createFlightSqlClient(options) -> FlightSqlClient`                 | factory  | SQL construction, SYNCHRONOUS — never a promise       |
|  [03]   | `new FlightSqlClient(FlightClientOptions \| FlightClient)`          | ctor     | wrap options or an existing `FlightClient`            |
|  [04]   | `query`/`queryBatches`/`queryStream`/`getQueryInfo`                 | instance | `Table`, `RecordBatch`, raw `FlightData`, or a plan   |
|  [05]   | `executeUpdate` / `prepare` / `closePreparedStatement`              | instance | DML/DDL and prepared-plan lifecycle, opaque handle    |
|  [06]   | `executePrepared` / `executePreparedStream`                         | instance | prepared re-execution — NEITHER takes parameters      |
|  [07]   | `executePreparedUpdate(statement, parameters?)`                     | instance | the ONE parameterized leg — `Iterable` or async       |
|  [08]   | `beginTransaction` / `commit` / `rollback`                          | instance | transaction lifecycle over `transactionId`            |
|  [09]   | `getCatalogs` / `getTableTypes`                                     | instance | zero-argument catalog discovery → `Table`             |
|  [10]   | `getDbSchemas({catalog?, dbSchemaFilterPattern?})`                  | instance | schema discovery → `Table`; both filters optional     |
|  [11]   | `getTables({tableNameFilterPattern?, tableTypes?, includeSchema?})` | instance | plus the row [10] filters; schema fetch is opt-in     |
|  [12]   | `getPrimaryKeys(table, {catalog?, dbSchema?})`                      | instance | key discovery → `Table`; table name positional        |
|  [13]   | `flight` / `url` / `closed`; `authenticated` on `FlightClient`      | getter   | the wrapped `FlightClient` and the client state       |
|  [14]   | `getFlightInfo` / `pollFlightInfo` / `getSchema`                    | instance | descriptor-addressed plan, progress, schema bytes     |
|  [15]   | `doGet(FlightTicket)` / `doPut(AsyncIterable<FlightData>)`          | instance | endpoint redemption; upload's FIRST frame holds it    |
|  [16]   | `listFlights` / `listActions` / `doAction`                          | instance | dataset discovery and the server's action set         |
|  [17]   | `authenticate()`; `handshake(payload?)` on `FlightClient` alone     | instance | handshake, and the eager `authProvider` re-resolve    |
|  [18]   | `decodeFlightDataToTable` / `decodeFlightDataStream`                | static   | `FlightData` → `Table` / `RecordBatch` stream         |
|  [19]   | `encodeRecordBatchesToFlightData(batches, schema)`                  | static   | batch stream → frames; the schema is required         |
|  [20]   | `encodeTableToFlightData` / `createFlightDataFromIpc`               | static   | `Table` → frames; one raw IPC message → one frame     |
|  [21]   | `getSchemaFromFlightData` / `parseIpcMessage`                       | static   | schema off a frame stream, CONSUMING it; IPC split    |
|  [22]   | `DEFAULT_TIMEOUT_MS`                                                | static   | `30000`, applied by `resolveOptions`, read by nothing |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `lane/olap` consumes the Flight SQL row as engine-blind columnar ingress/egress; the server engine stays opaque behind the wire.
- every result decodes zero-copy onto `apache-arrow` `Table`/`RecordBatch`; a row re-materialization never intervenes.
- one `@connectrpc/connect` transport over `node:http2` carries every RPC; a second gRPC stack never enters the lane.
- `timeoutMs` is INERT — `resolveOptions` defaults it onto the resolved record and the constructor passes `createGrpcTransport` only `baseUrl` and `nodeOptions`, so no deadline, `AbortSignal`, or per-call timeout reaches the wire and a consumer's whole bound is its own.
- `authProvider` outranks `auth` and makes a rotated credential adoptable without reconstructing the client: the thunk resolves before the first request and again whenever the server refuses a call as unauthenticated, and the resolved credential caches between those points rather than being consulted per request.
- `authenticate()` is the eager arm of that same rotation — a holder that already knows a credential turned over re-resolves through the provider rather than paying a refusal to learn it.
- three refusal classes mint at this pin, not six: `FlightServerError` carries every ConnectRPC verdict, `FlightAuthError` reaches only a handshake answering nothing, `FlightConnectionError` only a raw `Error` carrying no `code` whose message names `ECONNREFUSED`, and `FlightTimeoutError`/`FlightCancelledError` are exported and thrown nowhere.
- `FlightServerError.code` is DECLARED `string` and POPULATED from ConnectRPC's `ConnectError.code`, whose own declaration is the numeric `Code` enum, so the field holds a NUMBER for every transport verdict and a syscall STRING only when a raw socket error reaches the wrap; the package's internal auth branch compares that field against `"UNAUTHENTICATED"`/`"PERMISSION_DENIED"` NAMES and therefore never fires, which is why a 401 arrives as a server error rather than an auth error.
- `FlightServerError.details` carries `ConnectError.rawMessage` — the server's own diagnostic without the status prefix — and a classifier reading only `String(cause)` drops it.
- `doPut` reads its descriptor off the first frame as a `FlightDescriptor` MESSAGE while the root exports that type without its `Schema` value, so the message arrives from `FlightInfo.flightDescriptor` — the echo `getFlightInfo(FlightDescriptorInput)` answers — and an assembled literal is unspellable at this pin.
- `PutResult` is the one Flight message absent from the root export list, so an upload acknowledgement types through `ReturnType<FlightClient["doPut"]>` rather than an import.
- `FlightEndpoint.location` is `Location[]` and `Location` is ITSELF absent from the root export list, so its `uri` reads structurally; an empty roster and the `arrow-flight-reuse-connection://?` sentinel both mean "redeem where the ticket was minted", while any other authority names services that alone hold the split — grpc URIs the `http`/`https` transport cannot dial, or an http URI whose ticket the protocol says to ignore in favor of a direct GET.
- `FlightEndpoint.expirationTime` is the protocol's retry-admission window and `Ticket` is single-use by its own declaration, so a redeemed split never replays.
- consuming `getSchemaFromFlightData` exhausts the stream it reads, so a caller wanting both schema and batches reads the batches and takes the schema off the first one.
- every read member is generic over `apache-arrow` `TypeMap` and validates nothing against it, so a threaded type parameter publishes an unchecked phantom rather than a decode guarantee.

[STACKING]:
- `apache-arrow`(`.api/apache-arrow.md`): `decodeFlightDataToTable`/`decodeFlightDataStream` land `FlightData` on `Table`/`RecordBatch`, `encodeRecordBatchesToFlightData`/`encodeTableToFlightData` re-encode for `doPut` — the same IPC interchange the DuckDB and ClickHouse rows carry.
- `@duckdb/node-api`(`.api/duckdb-node-api.md`): the Flight SQL row reaches a remote Arrow-Flight-speaking engine; below the distributed trigger the embedded row owns the workload.
- `core/interchange/codec#LANDING_WIRE`: `Hops` carries the numeric gRPC code beside its retryability and fault class, so the lane resolves `FlightServerError.code` through that one status algebra and a second code roster never lands beside this client.
- `lane/olap`: boundary-kernel wrapping scopes construction in an acquire-release graph with `close()` on release, seals `auth` and `tls` material behind `Redacted` through one unwrap, bounds every answer on its own governor and every emission on an idle budget, and lifts the refusal classes through `isError` guards into typed lane faults carrying `details` beside the thrown text.
- `lane/olap`: `authProvider` carries a rotating credential as a thunk unwrapping `Redacted` per resolve, so sealed material never lands on the options record, and the lane's own rotation signal fires `authenticate()` rather than rebuilding the client under a fresh scope.
- `lane/olap`: uploads reach `doPut` through the descriptor `getFlightInfo` echoes, and a caller already holding that message hands it back on its own `$typeName`, so a bulk run stamps every frame set off one plan read.

[LOCAL_ADMISSION]:
- `scope:data`, node lane — the client rides `node:http2`, scoped in the lane's acquire-release graph, `close()` on release.
- OLAP rows are correctness-adjacent, never the record of truth — they read and write columns; nothing folds back as authority.
