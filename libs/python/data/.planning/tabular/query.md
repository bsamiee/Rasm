# [PY_DATA_QUERY]

The relational query owner over one `QuerySpec` axis materializing to uniform Arrow. `QueryEngine` discriminates the `QuerySpec` tagged-union axis — DuckDB SQL gated through the `sqlglot` parse/qualify/optimize plane, the chained DuckDB relational API, the dataframe-agnostic narwhals surface, the Ibis backend-agnostic expression IR with cross-dialect emission, the ADBC/ConnectorX/Flight SQL remote transport over a `RemoteOp` read/stream/ingest/probe/partition sub-axis carrying one `Transport` option owner, and the daft out-of-core/distributed runner — onto one `pyarrow.Table` result; the frontend IS the spec shape, never a parallel backend `StrEnum` knob and never a `read`/`stream`/`ingest` name-suffix family. The `Remote` case carries its DBAPI `dsn` string and the `Streaming` `RAY` runner its cluster-address string as plain `Remote.dsn`/`Streaming.cluster` case-payload inputs the caller supplies, never a parallel `run` signature param describing the value the `QuerySpec` already carries — a secret-bearing DSN resolves caller-side through the `execution/admission#SETTINGS` `SecretBoundary` that mints the outbound credential, never through `transport/roots#RESOURCE` `TransportResource` (the `http`/`ssh` artifact-fetch union whose `acquire(relative, modality) -> RuntimeRail[Acquired]` returns a fetched body, not a database DSN). The `PARTITION` op fans out through the `execute_partitions` handle and the `partition_sql` planner, and the `Transport` model folds the partition, wire-protocol, TLS/mTLS, three-phase `TimeoutPhase`-keyed timeout, header, message-size, cookie, and queue knobs into one `db_kwargs`/`conn_kwargs` projection keyed by the confirmed `DatabaseOptions`/`StatementOptions` enum values. The awaitable `QueryEngine.run` offloads every blocking leg to the `anyio` worker pool: the in-process arms ride `await async_boundary(f"query.{tag}", lambda: anyio.to_thread.run_sync(run))` over the broad `Exception` default (the heterogeneous `duckdb.Error`/`ibis.IbisError`/narwhals/pyarrow taxonomies the in-process arms raise are unrelated classes, so the faults owner's `CLASSIFY` fold classifies each — `duckdb`/`ibis` errors landing the `boundary` catch-all naming the cause — rather than a narrow `catch=duckdb.Error` that would let an `IbisError` escape uncaught), while Remote and `Streaming` acquisition delegates the whole retried-traced-railed leg to the runtime resilience owner — `await guarded(RetryClass.REMOTE_DB, anyio.to_thread.run_sync, _REMOTE[selected], ...)` funnels the Flight SQL/ADBC/ConnectorX driver call under the `reliability/resilience#RESILIENCE` `REMOTE_DB` row whose `_adbc_transient` hook retries an ADBC `dbapi.OperationalError` only on `status_code in {AdbcStatusCode.TIMEOUT, AdbcStatusCode.IO}` (plus stdlib `ConnectionError`/`TimeoutError`), and `await guarded(RetryClass.STREAMING, anyio.to_thread.run_sync, _stream, ...)` the daft runner under the `STREAMING` row whose tuple catches the `daft.exceptions.DaftTransientError` Rust-backed transient base, so a genuine transport-transient retries under the runtime backoff and converts to a `BoundaryFault` on exhaustion, never the `RPC`/`WIRE` rows whose `_named` COMPAS XML-RPC qualnames and `(ConnectionError,)` intra-mesh tuple catch none of these driver faults, never a hand-rolled retry loop, a bare `guard(cls)(...)` re-fenced a second time, or a synchronous `boundary` blocking the event loop on a multi-second scan. The result fold is spec-agnostic, so the egress, content-key, and `QueryReceipt` — the `columnar` scan-receipt owner extended with the optional column-level `lineage_edges` projection this owner populates from the `sqlglot.lineage.lineage` column-provenance engine over the qualified SQL and `ibis.to_sql` over the bound expression — are shared with the `columnar` scan owner, never a second receipt rail.

## [01]-[INDEX]

- [01]-[QUERY]: the relational query engine over one `QuerySpec` axis materializing to uniform Arrow, with the `Dialects`-gated `sqlglot`/`ibis` SQL plane, the `RemoteOp` transport sub-axis under the `guard`/`RetryClass` resilience rail, the daft elasticity runner, and the `sqlglot.lineage.lineage` column-provenance receipt projection folded onto the shared `columnar` receipt.

## [02]-[QUERY]

- Owner: `QueryEngine` — the one relational query owner over `duckdb`/`narwhals`/`ibis`/`sqlglot`/ADBC/`daft` discriminating by the `QuerySpec` tagged-union axis (the single discriminant; the frontend IS the spec shape). `QuerySpec` cases `Sql` (relational SQL gated through the `sqlglot` plane) · `Rel` (the chained relational API) · `Agnostic` (the dataframe-agnostic expression surface) · `Ir` (the Ibis backend-agnostic expression IR with cross-dialect emission/ingest) · `Remote` (ADBC/ConnectorX/Flight SQL transport over the `RemoteOp` read/stream/ingest/probe/partition sub-axis) · `Streaming` (the daft out-of-core/distributed runner). A new query frontend is one `QuerySpec` case; a new SQL sub-mode is one `SqlGate`/`IrEmit` field on the existing case; a new transport operation is one `RemoteOp` row; a new remote driver is one `RemoteDriver` row on the driver table; a new transport knob is one `Transport` field folded into the option-keyed `db_kwargs`/`conn_kwargs` projection; a new timeout phase is one `TimeoutPhase` row; a new runner is one `Runner` row — never a `sql_query`/`rel_query`/`nw_query`/`ibis_query`/`stream_query` method family.
- Cases: `QuerySpec.Sql(text, gate)` runs `con.sql(text)` over the `QueryEngine._duckdb` `with duckdb.connect()` bracket that `register`s every input frame and releases the request-scoped connection on every exit, binding the result with `to_arrow_table()` (zero-copy Arrow) inside the bracket so the connection never leaks past the materialization, the optional `SqlGate` gating `text` through `sqlglot.parse_one(dialect=Dialect.get_or_raise(read), error_level=errors)`/`qualify`/`optimizer.optimize`/`Expr.sql(dialect=get_or_raise(write))` under the `ErrorLevel`-keyed parser policy before DuckDB sees it and folding the parse evidence into the receipt · `Rel(filter, project, group_by)` chains `from_arrow(...).filter(...).project(...).aggregate(...)` returning a `DuckDBPyRelation`, terminal `to_arrow_table()` · `Agnostic(select, predicates, group_by, aggs)` admits any native frame via `narwhals.from_native(...).lazy()`, folds each `(column, Comparator, value)` `Predicate` through the `_COMPARE` dispatch table into a boolean `narwhals.Expr` (the closed `Comparator` `StrEnum` keying `operator.gt`/`ge`/`lt`/`le`/`eq`/`ne` and `Expr.is_in`, never the broken `filter(col_name)` over a bare non-boolean column reference narwhals rejects with `ArrowNotImplementedError`), AND-combines them through the variadic `LazyFrame.filter`, and shapes either a `group_by(*group_by).agg(*aggs)` grouped reduction — each `(column, Aggregator)` `AggExpr` folded through `_aggregation` into the `Aggregator`-keyed `Expr.sum`/`mean`/`min`/`max`/`count`/`n_unique`/`median`/`std` reducer aliased back to its column, never a hardcoded `sum` — or a flat `select(*select)`, landing Arrow through `narwhals.DataFrame.to_arrow` · `Ir(expr, emit)` binds one `ibis` expression — either the supplied `IbisTable` or, when `IrEmit.parse` carries a SQL string, the `ibis.parse_sql(parse, catalog, dialect)` round-trip that admits raw SQL back into the backend-agnostic expression tree — and the `PlanWire` row folds egress: `SQL` materializes through `BaseBackend.to_pyarrow(expr)` (or `to_pyarrow_batches(expr).read_all()` under `IrEmit.streaming` for the incremental record-batch path) against `ibis.connect(backend_uri)` (DuckDB the default when `backend_uri` is `None`) and emits the cross-dialect portable SQL plan through `ibis.to_sql(expr, dialect)` for the wire, while `SUBSTRAIT`/`SUBSTRAIT_JSON` round-trip the compiled `SELECT` through the admitted `duckdb-substrait` extension (`con.install_extension("substrait", repository="community")`/`load_extension`/`get_substrait`/`get_substrait_json` emitting the portable binary/JSON Substrait protobuf plan, `from_substrait`/`from_substrait_json` executing it back to Arrow) — the portable relational-algebra artifact the C# `Rasm.Persistence` federation seam decodes, denser than a dialect SQL string; the same lazy `sqlglot`-compiled expression compiles to DuckDB/DataFusion/Polars/remote SQL with no rewrite · `Remote(sql, dsn, driver, op, transport)` opens the remote connection over the caller-supplied DBAPI `dsn` string (a secret-bearing DSN resolved caller-side through `execution/admission#SETTINGS` `SecretBoundary`, never minted here) and dispatches the `RemoteOp` sub-axis (`READ` whole table, `STREAM` record-batch reader, `INGEST` Arrow write-back, `PROBE` schema introspection, `PARTITION` distributed fan-out — `_adbc_dispatch` answers all five uniformly for the ADBC and Flight SQL drivers, the `PARTITION` arm driving the DBAPI `Cursor.adbc_execute_partitions(sql)` row into `(descriptors, schema)` and opening each descriptor through `Cursor.adbc_read_partition(token)` as an independent `RecordBatchReader` under the `Connection.cursor(adbc_stmt_kwargs={"adbc.rpc.result_queue_size": ...})` read-ahead, ConnectorX answering `PROBE` through `get_meta`, `PARTITION` through the `partition_sql` explicit planner fed back as the `read_sql` per-partition query list, and the read ops through `read_sql`) across the `RemoteDriver` table — ADBC the cp315-core default, ConnectorX the `<3.15`-gated parallel-partitioned fast-path auto-selected only for the read-parallel ops (`READ`/`STREAM`/`PARTITION` — the `_CX_OPS` set) when the `Transport.partition_on` column is supplied, `INGEST` staying on ADBC's `adbc_ingest` (ConnectorX is read-only) and `PROBE` on ADBC's native `adbc_get_table_schema` (the `partition_num`/`protocol` wire-tuning travelling on the same row), Flight SQL the Arrow-native driver whose `Transport.db_kwargs`/`conn_kwargs` fold the confirmed `DatabaseOptions` transport axis (`AUTHORIZATION_HEADER`/`AUTHORITY`/`TLS_ROOT_CERTS`/`TLS_OVERRIDE_HOSTNAME`/`MTLS_CERT_CHAIN`/`MTLS_PRIVATE_KEY`/`TLS_SKIP_VERIFY`/`WITH_BLOCK`/`WITH_COOKIE_MIDDLEWARE`/`WITH_MAX_MSG_SIZE`/the three `TIMEOUT_*` phases keyed by `TimeoutPhase`/`RPC_CALL_HEADER_PREFIX`/the OAuth axis — `OAUTH_FLOW` keyed by the `OAuthFlow` row plus the `OAUTH_*` client-credentials and RFC 8693 token-exchange keys folded from the `Transport.oauth` mapping), the `StatementOptions.QUEUE_SIZE` read-ahead and `SUBSTRAIT_VERSION`, and the `ConnectionOptions.OPTION_SESSION_OPTION_PREFIX` session-option keys into one option-keyed dict — all terminating in the uniform `pyarrow.Table` (or a `RecordBatchReader` collapsed to a table on the `STREAM` op) · `Streaming(plan, runner, cluster)` builds a lazy `daft.DataFrame` from `daft.read_*` (the `_DAFT_READ` row keyed by `LakehouseFormat` carrying the per-format time-travel key — `snapshot_id` for Iceberg, `version` for Delta/Lance, `asof`/`block_size` the Lance scan axes — threading `io_config` for object-store credentials, the SQL row threading `read_sql(partition_col=, num_partitions=, partition_bound_strategy=, disable_pushdowns_to_sql=)` native partitioning), binds every registered input through `daft.from_arrow` into `daft.sql(plan.sql, register_globals=False, **frames)` when `plan.sql` reshapes a lakehouse scan, and pushes the `plan.predicate`/`with_columns`/`group_by`×`agg`/`explode`/`distinct`/`sample`/`sort`/`project`/`limit`/`repartition` shaping (each `daft.sql_expr`-compiled where the verb takes an expression) into the lazy plan before triggering `DataFrame.to_arrow` under the `Runner` row (`NATIVE` single-node out-of-core, `RAY` distributed against the caller-supplied cluster-address string the `Streaming` case carries as its third payload slot), the polars streaming engine's out-of-core successor.
- Entry: `QueryEngine.of` admits the bound Arrow/relation inputs; the awaitable `QueryEngine.run` folds the `QuerySpec` through `match`/`case` closed by `assert_never` (the totality proof — a new case forces a new arm) and returns a `RuntimeRail[pa.Table]`. The in-process `Sql`/`Rel`/`Agnostic`/`Ir` arms ride `_local` — one `await async_boundary(f"query.{tag}", lambda: anyio.to_thread.run_sync(run))` that offloads the blocking DuckDB/narwhals/Ibis materialization off the event loop and converts whatever the arm raises to a `BoundaryFault` exactly once through the faults owner's `CLASSIFY` fold — the broad `Exception` default deliberate because `duckdb.Error`, `ibis.IbisError`, narwhals, and pyarrow are disjoint exception roots (no shared ADBC `dbapi.Error` base), so a narrowed `catch` would let one arm's taxonomy escape the fence — never the synchronous `boundary` that would block the loop on a multi-second scan. The network-touching arms delegate the whole retried-traced-railed leg to the runtime resilience owner — `await guarded(RetryClass.REMOTE_DB, anyio.to_thread.run_sync, _REMOTE[selected], sql, dsn, op, transport, self.inputs, subject="query.remote")` for the remote drivers and `await guarded(RetryClass.STREAMING, anyio.to_thread.run_sync, _stream, plan, runner, cluster, self.inputs, subject="query.streaming")` for the daft runner — so the blocking driver/runner call offloads to the `anyio` worker pool, a transient ADBC `OperationalError` whose `status_code` is `TIMEOUT`/`IO` (the `REMOTE_DB` `_adbc_transient` hook) or a daft `DaftTransientError` (the `STREAMING` tuple) retries under the runtime `stamina` row inside one derivation span, and the terminal raise lifts through the resilience owner's `async_boundary` exactly once, never the `RPC`/`WIRE` rows whose `_named` COMPAS-proxy qualnames and `(ConnectionError,)` intra-mesh tuple catch no ADBC `OperationalError` (which subclasses `DatabaseError`/`Error`/`Exception`, never `ConnectionError`) or daft Rust fault, and never a bare `guard(cls)(...)` re-wrapped in a second fence (the doubled-span/doubled-lift form the resilience owner names deleted). Every connection is request-scoped, never a module global: the DuckDB `Sql`/`Rel` connection rides the `_duckdb` `with duckdb.connect()` bracket (releasing on success, fault, and cancellation once `to_arrow_table` has materialized inside it), the `_ir` Ibis backend releases through a `try`/`finally` `backend.disconnect()` that also closes the native `backend.con` the Substrait round-trip drives, and the remote drivers ride the `with dbapi.connect(...)` context manager — a leaked connection past the run is the deleted form.
- Auto: Arrow result binding is uniform — every case terminates in a `pyarrow.Table` via `DuckDBPyRelation.to_arrow_table`, `narwhals.DataFrame.to_arrow`, `ibis.BaseBackend.to_pyarrow`, `Cursor.fetch_arrow_table`/`Cursor.fetch_record_batch().read_all()`/`Cursor.adbc_ingest`/`Connection.adbc_get_table_schema`, `connectorx.read_sql(return_type="arrow")`/`connectorx.get_meta`, or `daft.DataFrame.to_arrow`, so the egress, content-key, and `QueryReceipt` fold are spec-agnostic; the narwhals path keeps the query lazy (`narwhals.LazyFrame`) until `collect()`; the Ibis path stays an unbound expression until `to_pyarrow`/`to_pyarrow_batches`, so predicate pushdown crosses the backend, `ibis.to_sql(expr, dialect)` emits the portable SQL plan without connecting, and the `PlanWire.SUBSTRAIT`/`SUBSTRAIT_JSON` rows lift the compiled plan to a portable Substrait protobuf the C# federation seam decodes; the daft path stays a lazy logical plan until the `to_arrow` sink, so partition-parallel pushdown owns the cost; Ibis (analytical-intent portability across 20+ backends), narwhals (input-type agnosticism), and daft (where-and-how-big-code-runs scale) are distinct axes all carried on the owner. The `RemoteOp` sub-axis is a closed-enum value the `_adbc_dispatch` fold matches once over `assert_never` for the two ADBC-handle drivers, never a `read`/`stream`/`ingest`/`probe`/`partition` method family; the `RemoteDriver` row carries its connect-and-dispatch closure so the three drivers collapse to one table lookup over the shared `(sql, dsn, op, transport, frames)` signature, never three `if driver ==` arms; ConnectorX auto-selecting for a read-parallel op in `_CX_OPS` when `Transport.partition_on` is supplied (the `run` remote arm rewriting an ADBC `READ`/`STREAM`/`PARTITION` request to ConnectorX, never an `INGEST`/`PROBE`) is its sole reason to exist over ADBC's serial one-shot, the `PARTITION` op is the distributed fan-out the `_partitions` fold drives off the DBAPI `Cursor.adbc_execute_partitions`/`adbc_read_partition` rows on the ADBC/Flight SQL path and the `partition_sql` planner on the ConnectorX path, and the `Transport` model is the one option owner folding partition, wire-protocol, TLS/mTLS, the three-phase `TimeoutPhase`-keyed timeout, header, message-size, cookie, queue, OAuth-flow, session-option, and Substrait-version knobs into `db_kwargs`/`conn_kwargs` keyed by the confirmed `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` enum values rather than scattered `inputs[...]` reads. `ibis`, `sqlglot`, and `adbc-driver-manager` are cp315-clean and import module-top, while `connectorx`, `adbc-driver-flightsql`, and `daft` ride their gated arms importing the dist function-local under `# noqa: PLC0415` (ConnectorX on the `python_version<'3.15'` band, Flight SQL and daft as heavyweight engines), never a module-top import on this cp315-core page.
- Receipt: `receipt_of(spec, table)` folds one total `_provenance` match over the `QuerySpec` axis into the shared `columnar#SCAN` `QueryReceipt.railed` — source, predicate count (the lower `columnar#SCAN` owner's exported `predicate_count(text)` fold — the one `parse_one().find_all(*_PREDICATE_NODES)` application over the `exp.Where`/`exp.Having`/`exp.Qualify`/`exp.Join` widening the lower owner declares and this owner imports and calls rather than re-spelling the byte-identical fold, so the scan and query counts share the application not only the node tuple — applied to the `Sql`/`Ir`/`Remote`/`Streaming` SQL, the `Rel` filter presence as `int(flt is not None)`, the `Agnostic` structured-predicate count as `len(predicates)`, or the no-SQL streaming filter presence), and `lineage_edges` (the column-level source→derived edges the `sqlglot.lineage.lineage` engine traces — `SqlGate.edges` folds one `lineage` call per qualified output column and walks each `lineage.Node` tree to its source-column leaves through `_leaves`, so an unqualified projection resolves to its real physical source rather than a `find_tables`×`exp.Column` cross-product, extracted on every SQL-carrying arm — the `Sql` text, the `Ir` `ibis.to_sql` emission, the `Remote` transport SQL, and the `Streaming` `plan.sql` reshape — and `()` only on the relational/agnostic arms that carry no SQL string) — so `receipt_of` returns `RuntimeRail[QueryReceipt]`, the content key derived off the canonical Arrow bytes through the railed `ContentIdentity.of` inside `QueryReceipt.railed` rather than collapsed into a bare field, and contributed through runtime `ReceiptContributor`; the durable provenance ledger stays the C# `Rasm.Persistence` Version/Provenance owner consumed at the wire, never a Python versioning/provenance/ledger owner. No new receipt rail, and `_provenance` is one total fold over the spec yielding `(source, predicate_count, lineage_edges)` together, never two parallel spec walks.
- Packages: `duckdb` (`connect`/`register`/`sql`/`from_arrow`/`DuckDBPyRelation.filter`/`project`/`aggregate`/`to_arrow_table`/`install_extension(repository=)`/`load_extension`), `duckdb-substrait` (the connection-bound `get_substrait`/`get_substrait_json`/`from_substrait`/`from_substrait_json` round-trip over the loaded `substrait` community extension), `sqlglot` (`parse_one(dialect=, error_level=)`/`Expr.sql(dialect=)`/`Expr.selects`/`optimizer.optimize`/`optimizer.qualify.qualify(infer_schema=, validate_qualify_columns=)`/`exp.Where`/`exp.Having`/`exp.Qualify`/`exp.Join`/`find_all(*types)`/`lineage.lineage(column, sql, schema=, dialect=)`/`lineage.Node`/`Dialect.get_or_raise`/`Dialects`/`ErrorLevel`), `narwhals` (`from_native`/`DataFrame.lazy`/`col`/`Expr`/`Expr.is_in`/`Expr.sum`/`mean`/`min`/`max`/`count`/`n_unique`/`median`/`std`/`Expr.alias`/`LazyFrame.filter`/`LazyFrame.group_by`/`GroupBy.agg`/`LazyFrame.collect`/`DataFrame.to_arrow` — the comparison predicates ride `operator.gt`/`ge`/`lt`/`le`/`eq`/`ne` over the narwhals `Expr`, not a narwhals method), `ibis-framework` (`connect`/`duckdb.connect`/`parse_sql(catalog=, dialect=)`/`BaseBackend.to_pyarrow`/`to_pyarrow_batches`/`to_sql`/`BaseBackend.disconnect`/`BaseBackend.con`, the backend-agnostic expression IR), `adbc-driver-manager` (`dbapi.connect`/`Connection.cursor(adbc_stmt_kwargs=)`/`Cursor.execute`/`fetch_arrow_table`/`fetch_record_batch`/`adbc_ingest`/`adbc_execute_partitions`/`adbc_read_partition`/`Connection.adbc_get_table_schema`), `adbc-driver-flightsql` (`dbapi.connect(db_kwargs=, conn_kwargs=)`/`DatabaseOptions.{AUTHORIZATION_HEADER,AUTHORITY,TLS_ROOT_CERTS,TLS_OVERRIDE_HOSTNAME,MTLS_CERT_CHAIN,MTLS_PRIVATE_KEY,TLS_SKIP_VERIFY,WITH_BLOCK,WITH_COOKIE_MIDDLEWARE,WITH_MAX_MSG_SIZE,TIMEOUT_FETCH,TIMEOUT_QUERY,TIMEOUT_UPDATE,RPC_CALL_HEADER_PREFIX,OAUTH_FLOW,OAUTH_*}`/`OAuthFlowType.{CLIENT_CREDENTIALS,TOKEN_EXCHANGE}`/`ConnectionOptions.OPTION_SESSION_OPTION_PREFIX`/`StatementOptions.{QUEUE_SIZE,SUBSTRAIT_VERSION}`), `connectorx` (`read_sql(return_type="arrow"|"arrow_stream", protocol=, partition_on=, partition_num=)`/`partition_sql(conn, query, partition_on, partition_num)`/`get_meta(conn, query, protocol=)`, the `<3.15` gated partitioned arm), `daft` (`read_parquet`/`read_iceberg(snapshot_id=)`/`read_deltalake(version=)`/`read_hudi`/`read_lance(version=, asof=, block_size=)`/`read_sql(partition_col=, num_partitions=, partition_bound_strategy=, disable_pushdowns_to_sql=)`/`from_arrow`/`sql(register_globals=)`/`sql_expr`/`DataFrame.where`/`with_columns`/`groupby`/`agg`/`explode`/`distinct`/`sample`/`sort(desc=)`/`select`/`limit`/`repartition(num, *by)`/`set_runner_native`/`set_runner_ray`/`DataFrame.to_arrow`), `pyarrow` (`Table`/`Table.from_pandas`/`Table.from_batches`/`RecordBatchReader`/`concat_tables`/`table`), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `of` factory and the caller-facing `run` submission so a non-`QuerySpec` operation or a malformed `inputs` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the enclosing `async_boundary`/`guarded` fence rather than an untyped admission, the shared `FAULT_CONF` the sibling `interop`/`egress`/`columnar` admission seams bind; the spec-agnostic `receipt_of` fold over the produced `pa.Table` carries no decorator), runtime (`RuntimeRail`/`async_boundary`/`RetryClass`/`guarded`/`FAULT_CONF` the shared beartype violation-redirect config/`ContentIdentity`/`ReceiptContributor`, the receipt members reached through the `columnar` `QueryReceipt`, `RetryClass`/`guarded` the resilience rail the remote/streaming arms ride, `async_boundary` the awaitable fault fence the in-process arms ride), `anyio` (`to_thread.run_sync` the worker-pool offload every blocking DuckDB/narwhals/Ibis/driver/runner leg rides so the cp315 event loop never stalls), `tabular/columnar` (`QueryReceipt`).
- Growth: a new query frontend is one `QuerySpec` case; a new SQL dialect target is one `Dialects` member the `SqlGate`/`IrEmit` already thread; a new plan-wire artifact is one `PlanWire` row the `_ir_plan` fold already matches; a new predicate-bearing node is one `_PREDICATE_NODES` row on the lower `columnar#SCAN` owner the exported `predicate_count` fold this owner calls already scans; a new transport operation is one `RemoteOp` row the `_adbc_dispatch` fold already matches under `assert_never`; a new remote driver is one `RemoteDriver` row on the `_REMOTE` table binding its connect closure; a new transport knob is one `Transport` field folded into `db_kwargs`/`conn_kwargs`; a new timeout phase is one `TimeoutPhase` row keyed to its `TIMEOUT_*` member; a new OAuth key is one `Transport.oauth` entry the `OAUTH_*` projection already folds; a new session option is one `Transport.session_options` entry; a new daft runner is one `Runner` row the `_stream` runner branch selects against the caller-supplied `cluster` address; a new lakehouse source is one `LakehouseFormat` row on the `_DAFT_READ` table carrying its time-travel key; a new daft shaping verb is one `StreamingPlan` field threaded into the `_stream` fold; a new agnostic comparator is one `Comparator` member plus one `_COMPARE` row; a new agnostic aggregator is one `Aggregator` member resolved by value through `_aggregation`; a new relational verb composes on the existing chain; zero new surface.
- Boundary: no durable query rail, no global connection, no SQL-string templating engine, no regex SQL rewriting where the `sqlglot` AST owns structure, no hand-rolled Substrait protobuf codec where the `duckdb-substrait` extension owns encode/decode, no parallel plan model per encoding where one `PlanWire` row keys binary-versus-JSON, no parallel `StrEnum` backend discriminant duplicating the `QuerySpec` tag, no second Ray runner owner, no second `QueryReceipt` declaration shadowing the `columnar` owner, no per-setting builder type where the `DatabaseOptions`/`ConnectionOptions`/`StatementOptions` enum value keys the option; the DBAPI `dsn` and Ray cluster-address are caller-supplied `Remote.dsn`/`Streaming.cluster` case-payload strings and OAuth credential identity resolves caller-side through `execution/admission#SETTINGS` `SecretBoundary`, never a Python-minted credential and never `transport/roots#RESOURCE` `TransportResource` (the `http`/`ssh` artifact-fetch union, not a database-DSN or Ray-address resolver); a per-frontend query class family, a `read`/`stream`/`ingest`/`probe`/`partition` remote method family, three `if driver ==` arms, three parallel `timeout_fetch`/`timeout_query`/`timeout_update` fields where one `TimeoutPhase`-keyed row folds them, parallel `oauth_*` fields where one `Transport.oauth` mapping folds them, a hand-stitched gRPC partition loop where `Cursor.adbc_execute_partitions`/`adbc_read_partition`/`partition_sql` own the fan-out, a low-level `AdbcStatement` handle dance where the DBAPI `Cursor` partition rows own the same capability, a `register_globals`-leaking `daft.sql` over unbound globals, scattered `inputs[...]` option reads where the `Transport` model owns the knobs, a backend-specific SQL string inside an Ibis pipeline, a free-string dialect bypassing `Dialect.get_or_raise`, a hand-rolled retry loop, a bare `guard(cls)(...)` caller re-wrapped in a second fence, a synchronous `boundary` blocking the event loop, a narrowed `async_boundary(catch=dbapi.Error)` that lets a `duckdb.Error`/`IbisError` escape the in-process fence where the broad `Exception` default and the faults owner's `CLASSIFY` fold own the conversion, or a re-fenced leg where `await guarded(RetryClass.REMOTE_DB, ...)`/`await guarded(RetryClass.STREAMING, ...)` already own transport resilience, a `RetryClass.RPC`/`RetryClass.WIRE` on the remote/streaming arms whose `_named` COMPAS qualnames and `(ConnectionError,)` intra-mesh tuple catch no ADBC `OperationalError` or daft Rust transient (the no-op-retry form the `REMOTE_DB`/`STREAMING` rows replace), a `find_tables`×`exp.Column` cartesian where `sqlglot.lineage.lineage` owns column provenance, a daft Ray runner as a second owner, a generic dataframe wrapper, a `run(cluster=)` signature param smuggling the Ray address beside the `QuerySpec` the value already carries where the `Streaming` case payload owns it, and an undecorated `of`/`run` admitting a caller `QuerySpec`/`inputs` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `interop`/`egress`/`columnar` admission entrypoints share are the deleted forms.

```python signature
import operator
from builtins import frozendict
from collections.abc import Callable, Mapping
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import anyio
import duckdb
import ibis
import narwhals as nw
import pyarrow as pa
import sqlglot
from adbc_driver_manager import dbapi
from beartype import beartype
from expression import case, tag, tagged_union
from ibis.expr.types import Table as IbisTable
from msgspec import Struct, field
from narwhals import Expr as NwExpr
from sqlglot import Dialects, ErrorLevel, exp
from sqlglot.lineage import Node as LineageNode
from sqlglot.lineage import lineage as sqlglot_lineage
from sqlglot.optimizer import optimize as sqlglot_optimize
from sqlglot.optimizer.qualify import qualify as sqlglot_qualify

from rasm.data.tabular.columnar import QueryReceipt, predicate_count
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from adbc_driver_flightsql import ConnectionOptions, DatabaseOptions, StatementOptions


# --- [TYPES] ----------------------------------------------------------------------------

type Schema = Mapping[str, Mapping[str, str]]
type LineageEdge = tuple[str, str]
type Frames = Mapping[str, Any]
type Predicate = tuple[str, "Comparator", Any]
type AggExpr = tuple[str, "Aggregator"]


class Comparator(StrEnum):
    GT = "gt"
    GE = "ge"
    LT = "lt"
    LE = "le"
    EQ = "eq"
    NE = "ne"
    IN = "in_"


class Aggregator(StrEnum):
    SUM = "sum"
    MEAN = "mean"
    MIN = "min"
    MAX = "max"
    COUNT = "count"
    NUNIQUE = "n_unique"
    MEDIAN = "median"
    STD = "std"


class RemoteDriver(StrEnum):
    ADBC = "adbc"
    CONNECTORX = "connectorx"
    FLIGHTSQL = "flightsql"


class RemoteOp(StrEnum):
    READ = "read"
    STREAM = "stream"
    INGEST = "ingest"
    PROBE = "probe"
    PARTITION = "partition"


class TimeoutPhase(StrEnum):
    FETCH = "fetch"
    QUERY = "query"
    UPDATE = "update"


class OAuthFlow(StrEnum):
    CLIENT_CREDENTIALS = "client_credentials"
    TOKEN_EXCHANGE = "token_exchange"


class Runner(StrEnum):
    NATIVE = "native"
    RAY = "ray"


# the daft read-scan FRONTEND axis keying `_DAFT_READ` — orthogonal to the `lakehouse#LAKEHOUSE`
# `TableFormat` transactional-WRITE provider axis (DELTA/ICEBERG/LANCE). The two share the
# DELTA/ICEBERG/LANCE triple but model distinct concepts: `LakehouseFormat` is the broader
# read-frontend set (PARQUET/HUDI/SQL have no transactional-write owner), `TableFormat` the
# narrower mutate/commit set keyed by `_PORTABLE` op-reachability — not parallel names for one
# bounded concept, so they are NOT collapsed (a merge would pollute the write axis with
# read-only frontend members carrying no commit arm).
class LakehouseFormat(StrEnum):
    PARQUET = "parquet"
    ICEBERG = "iceberg"
    DELTA = "delta"
    HUDI = "hudi"
    LANCE = "lance"
    SQL = "sql"


# --- [MODELS] ---------------------------------------------------------------------------

class PlanWire(StrEnum):
    SQL = "sql"
    SUBSTRAIT = "substrait"
    SUBSTRAIT_JSON = "substrait_json"


class SqlGate(Struct, frozen=True):
    read: Dialects = Dialects.DUCKDB
    write: Dialects = Dialects.DUCKDB
    schema: Schema | None = None
    optimize: bool = True
    errors: ErrorLevel = ErrorLevel.RAISE

    def _qualified(self, text: str, dialect: Dialects) -> exp.Expression:
        tree = sqlglot.parse_one(text, dialect=sqlglot.Dialect.get_or_raise(dialect), error_level=self.errors)
        return sqlglot_qualify(tree, schema=self.schema, dialect=dialect, infer_schema=self.schema is None, validate_qualify_columns=self.schema is not None)

    def transpile(self, text: str) -> str:
        qualified = self._qualified(text, self.read)
        gated = sqlglot_optimize(qualified, schema=self.schema, dialect=self.read) if self.optimize and self.schema is not None else qualified
        return gated.sql(dialect=sqlglot.Dialect.get_or_raise(self.write))

    def edges(self, text: str) -> tuple[LineageEdge, ...]:
        # `text` is raw `read`-dialect SQL, so both qualify and lineage parse it under `read` — a
        # `read != write` gate that traced under `write` would mis-parse the source query.
        outputs = tuple(sel.alias_or_name for sel in self._qualified(text, self.read).selects)
        roots = (sqlglot_lineage(name, text, schema=self.schema, dialect=self.read) for name in outputs)
        return tuple((str(leaf.name), str(node.name)) for node in roots for leaf in _leaves(node))


class IrEmit(Struct, frozen=True):
    backend_uri: str | None = None
    dialect: Dialects = Dialects.DUCKDB
    wire: PlanWire = PlanWire.SQL
    streaming: bool = False
    parse: str | None = None
    catalog: Mapping[str, Mapping[str, str]] | None = None

    def bound(self, expr: IbisTable | None) -> IbisTable:
        return ibis.parse_sql(self.parse, catalog=self.catalog, dialect=self.dialect.value) if self.parse is not None else expr

    def sql(self, expr: IbisTable) -> str:
        return ibis.to_sql(self.bound(expr), dialect=self.dialect.value)

    def edges(self, expr: IbisTable) -> tuple[LineageEdge, ...]:
        return SqlGate(read=self.dialect, write=self.dialect).edges(self.sql(expr))


class Transport(Struct, frozen=True):
    partition_on: str | None = None
    partition_num: int = 4
    protocol: str | None = None
    queue_size: int | None = None
    authorization: str | None = None
    authority: str | None = None
    tls_root_certs: str | None = None
    tls_override_hostname: str | None = None
    mtls_cert_chain: str | None = None
    mtls_private_key: str | None = None
    tls_skip_verify: bool = False
    block: bool = False
    max_msg_size: int | None = None
    cookie_middleware: bool = False
    timeouts: Mapping[TimeoutPhase, float] = field(default_factory=dict)
    rpc_headers: Mapping[str, str] = field(default_factory=dict)
    oauth_flow: OAuthFlow | None = None
    oauth: Mapping[str, str] = field(default_factory=dict)
    session_options: Mapping[str, str] = field(default_factory=dict)
    substrait_version: str | None = None

    def db_kwargs(self, opts: "type[DatabaseOptions]") -> dict[str, str]:
        timeout_keys = {TimeoutPhase.FETCH: opts.TIMEOUT_FETCH, TimeoutPhase.QUERY: opts.TIMEOUT_QUERY, TimeoutPhase.UPDATE: opts.TIMEOUT_UPDATE}
        rows = (
            (opts.AUTHORIZATION_HEADER, self.authorization),
            (opts.AUTHORITY, self.authority),
            (opts.TLS_ROOT_CERTS, self.tls_root_certs),
            (opts.TLS_OVERRIDE_HOSTNAME, self.tls_override_hostname),
            (opts.MTLS_CERT_CHAIN, self.mtls_cert_chain),
            (opts.MTLS_PRIVATE_KEY, self.mtls_private_key),
            (opts.TLS_SKIP_VERIFY, "true" if self.tls_skip_verify else None),
            (opts.WITH_BLOCK, "true" if self.block else None),
            (opts.WITH_COOKIE_MIDDLEWARE, "true" if self.cookie_middleware else None),
            (opts.WITH_MAX_MSG_SIZE, str(self.max_msg_size) if self.max_msg_size is not None else None),
            (opts.OAUTH_FLOW, self.oauth_flow.value if self.oauth_flow is not None else None),
        )
        headers = {f"{opts.RPC_CALL_HEADER_PREFIX.value}{name}": value for name, value in self.rpc_headers.items()}
        timeouts = {timeout_keys[phase].value: str(seconds) for phase, seconds in self.timeouts.items()}
        oauth = {getattr(opts, f"OAUTH_{key.upper()}").value: value for key, value in self.oauth.items()}
        return {key.value: value for key, value in rows if value is not None} | timeouts | headers | oauth

    def conn_kwargs(self, opts: "type[StatementOptions]", conn_opts: "type[ConnectionOptions]") -> dict[str, str]:
        queue = {opts.QUEUE_SIZE.value: str(self.queue_size)} if self.queue_size is not None else {}
        substrait = {opts.SUBSTRAIT_VERSION.value: self.substrait_version} if self.substrait_version is not None else {}
        session = {f"{conn_opts.OPTION_SESSION_OPTION_PREFIX.value}{key}": value for key, value in self.session_options.items()}
        return queue | substrait | session


class StreamingPlan(Struct, frozen=True):
    fmt: LakehouseFormat
    source: str
    sql: str | None = None
    conn: str | None = None
    project: tuple[str, ...] = ()
    with_columns: Mapping[str, str] = field(default_factory=dict)
    predicate: str | None = None
    group_by: tuple[str, ...] = ()
    agg: tuple[str, ...] = ()
    explode: tuple[str, ...] = ()
    sort: tuple[str, ...] = ()
    sort_desc: bool = False
    distinct: tuple[str, ...] | None = None
    sample: float | None = None
    limit: int | None = None
    partition_col: str | None = None
    num_partitions: int | None = None
    partition_strategy: str = "min-max"
    disable_pushdowns: bool = False
    version: str | int | None = None
    asof: str | None = None
    block_size: int | None = None
    repartition: int | None = None
    repartition_by: tuple[str, ...] = ()
    io_config: Any = None


@tagged_union(frozen=True)
class QuerySpec:
    tag: Literal["sql", "rel", "agnostic", "ir", "remote", "streaming"] = tag()
    sql: tuple[str, SqlGate | None] = case()
    rel: tuple[str | None, tuple[str, ...], tuple[str, ...]] = case()
    agnostic: tuple[tuple[str, ...], tuple[Predicate, ...], tuple[str, ...], tuple[AggExpr, ...]] = case()
    ir: tuple[IbisTable, IrEmit] = case()
    remote: tuple[str, str, RemoteDriver, RemoteOp, Transport] = case()
    streaming: tuple[StreamingPlan, Runner, str | None] = case()

    @staticmethod
    def Sql(text: str, gate: SqlGate | None = None) -> "QuerySpec":
        return QuerySpec(sql=(text, gate))

    @staticmethod
    def Rel(filter_expr: str | None, project: tuple[str, ...], group_by: tuple[str, ...] = ()) -> "QuerySpec":
        return QuerySpec(rel=(filter_expr, project, group_by))

    @staticmethod
    def Agnostic(
        select: tuple[str, ...] = (), predicates: tuple[Predicate, ...] = (),
        group_by: tuple[str, ...] = (), aggs: tuple[AggExpr, ...] = (),
    ) -> "QuerySpec":
        return QuerySpec(agnostic=(select, predicates, group_by, aggs))

    @staticmethod
    def Ir(expr: IbisTable, emit: IrEmit = IrEmit()) -> "QuerySpec":
        return QuerySpec(ir=(expr, emit))

    @staticmethod
    def Remote(
        sql: str, dsn: str, driver: RemoteDriver = RemoteDriver.ADBC,
        op: RemoteOp = RemoteOp.READ, transport: Transport = Transport(),
    ) -> "QuerySpec":
        return QuerySpec(remote=(sql, dsn, driver, op, transport))

    @staticmethod
    def Streaming(plan: StreamingPlan, runner: Runner = Runner.NATIVE, cluster: str | None = None) -> "QuerySpec":
        return QuerySpec(streaming=(plan, runner, cluster))


# --- [SERVICES] -------------------------------------------------------------------------

class QueryEngine(Struct, frozen=True):
    inputs: Frames

    @classmethod
    @beartype(conf=FAULT_CONF)
    def of(cls, inputs: Frames) -> "QueryEngine":
        return cls(inputs=inputs)

    @beartype(conf=FAULT_CONF)
    async def run(self, spec: QuerySpec) -> "RuntimeRail[pa.Table]":  # noqa: PLR0911
        match spec:
            case QuerySpec(tag="sql", sql=(text, gate)):
                return await self._local("sql", lambda: self._duckdb(lambda con: con.sql(gate.transpile(text) if gate else text)))
            case QuerySpec(tag="rel", rel=(flt, project, group_by)):
                return await self._local("rel", lambda: self._duckdb(lambda con: self._relation(con, flt, project, group_by)))
            case QuerySpec(tag="agnostic", agnostic=(select, predicates, group_by, aggs)):
                return await self._local("agnostic", lambda: self._agnostic(select, predicates, group_by, aggs))
            case QuerySpec(tag="ir", ir=(expr, emit)):
                return await self._local("ir", lambda: self._ir(expr, emit))
            case QuerySpec(tag="remote", remote=(sql, dsn, driver, op, transport)):
                divert = transport.partition_on is not None and driver is RemoteDriver.ADBC and op in _CX_OPS
                selected = RemoteDriver.CONNECTORX if divert else driver
                return await guarded(RetryClass.REMOTE_DB, anyio.to_thread.run_sync, _REMOTE[selected], sql, dsn, op, transport, self.inputs, subject="query.remote")
            case QuerySpec(tag="streaming", streaming=(plan, runner, cluster)):
                return await guarded(RetryClass.STREAMING, anyio.to_thread.run_sync, _stream, plan, runner, cluster, self.inputs, subject="query.streaming")
            case unreachable:
                assert_never(unreachable)

    async def _local(self, tag: str, run: Callable[[], pa.Table]) -> "RuntimeRail[pa.Table]":
        return await async_boundary(f"query.{tag}", lambda: anyio.to_thread.run_sync(run))

    def _duckdb(self, build: Callable[[duckdb.DuckDBPyConnection], duckdb.DuckDBPyRelation]) -> pa.Table:
        # the connection is request-scoped — the `with` releases it on every exit (success, fault,
        # cancellation) once `to_arrow_table` has eagerly materialized the relation inside the bracket.
        with duckdb.connect() as con:
            for name, frame in self.inputs.items():
                con.register(name, frame)
            return build(con).to_arrow_table()

    def _relation(self, con: duckdb.DuckDBPyConnection, flt: str | None, project: tuple[str, ...], group_by: tuple[str, ...]) -> duckdb.DuckDBPyRelation:
        rel = con.from_arrow(next(iter(self.inputs.values())))
        rel = rel.filter(flt) if flt else rel
        return rel.aggregate(", ".join(project), ", ".join(group_by)) if group_by else rel.project(", ".join(project))

    def _agnostic(
        self, select: tuple[str, ...], predicates: tuple[Predicate, ...],
        group_by: tuple[str, ...], aggs: tuple[AggExpr, ...],
    ) -> pa.Table:
        lf = nw.from_native(next(iter(self.inputs.values()))).lazy()
        lf = lf.filter(*(_predicate(p) for p in predicates)) if predicates else lf
        shaped = lf.group_by(*group_by).agg(*(_aggregation(a) for a in aggs)) if group_by else lf.select(*select)
        return shaped.collect().to_arrow()

    def _ir(self, expr: IbisTable, emit: IrEmit) -> pa.Table:
        # the backend (in-process DuckDB or a remote `backend_uri` driver) is request-scoped —
        # `disconnect()` releases it on every exit, closing the native `backend.con` the Substrait
        # round-trip drives, so a remote `backend_uri` connection never leaks past the run.
        backend = ibis.connect(emit.backend_uri) if emit.backend_uri else ibis.duckdb.connect()
        try:
            bound = emit.bound(expr)
            return _ir_plan(backend, bound, emit) if emit.wire is not PlanWire.SQL else (
                backend.to_pyarrow_batches(bound).read_all() if emit.streaming else backend.to_pyarrow(bound)
            )
        finally:
            backend.disconnect()


# --- [OPERATIONS] -----------------------------------------------------------------------

_QUEUE_SIZE_KEY: Final[str] = "adbc.rpc.result_queue_size"


_COMPARE: Final[frozendict[Comparator, Callable[[NwExpr, Any], NwExpr]]] = frozendict({
    Comparator.GT: operator.gt,
    Comparator.GE: operator.ge,
    Comparator.LT: operator.lt,
    Comparator.LE: operator.le,
    Comparator.EQ: operator.eq,
    Comparator.NE: operator.ne,
    Comparator.IN: lambda col, value: col.is_in(value),
})


def _predicate(pred: Predicate) -> NwExpr:
    column, comparator, value = pred
    return _COMPARE[comparator](nw.col(column), value)


def _aggregation(spec: AggExpr) -> NwExpr:
    column, aggregator = spec
    return getattr(nw.col(column), aggregator.value)().alias(column)


def _leaves(node: LineageNode) -> tuple[LineageNode, ...]:
    return (node,) if not node.downstream else tuple(leaf for child in node.downstream for leaf in _leaves(child))


def _partitions(conn: dbapi.Connection, sql: str, transport: Transport) -> pa.Table:
    stmt_kwargs = {_QUEUE_SIZE_KEY: str(transport.queue_size)} if transport.queue_size is not None else None
    with conn.cursor(adbc_stmt_kwargs=stmt_kwargs) as cur:
        descriptors, schema = cur.adbc_execute_partitions(sql)
        readers = (cur.adbc_read_partition(token).read_all() for token in descriptors)
        return pa.concat_tables(readers) if descriptors else schema.empty_table()


def _ir_plan(backend: Any, expr: IbisTable, emit: IrEmit) -> pa.Table:
    con = backend.con
    con.install_extension("substrait", repository="community")
    con.load_extension("substrait")
    select = ibis.to_sql(expr, dialect=Dialects.DUCKDB.value)
    if emit.wire is PlanWire.SUBSTRAIT_JSON:
        plan = con.get_substrait_json(select).fetchone()[0]
        return con.from_substrait_json(plan).to_arrow_table()
    plan = con.get_substrait(select).fetchone()[0]
    return con.from_substrait(plan).to_arrow_table()


def _adbc_dispatch(conn: dbapi.Connection, sql: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    with conn.cursor() as cur:
        match op:
            case RemoteOp.READ:
                cur.execute(sql)
                return cur.fetch_arrow_table()
            case RemoteOp.STREAM:
                cur.execute(sql)
                return cur.fetch_record_batch().read_all()
            case RemoteOp.INGEST:
                frame = next(iter(frames.values()))
                cur.adbc_ingest(sql, frame, mode="create_append")
                return frame if isinstance(frame, pa.Table) else pa.table(frame)
            case RemoteOp.PROBE:
                schema = conn.adbc_get_table_schema(sql)
                return pa.table({"field": schema.names, "type": [str(t) for t in schema.types]})
            case RemoteOp.PARTITION:
                return _partitions(conn, sql, transport)
            case unreachable:
                assert_never(unreachable)


def _adbc(sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    with dbapi.connect(uri=dsn) as conn:
        return _adbc_dispatch(conn, sql, op, transport, frames)


def _connectorx(sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    import connectorx as cx  # noqa: PLC0415

    if op is RemoteOp.PROBE:
        return pa.Table.from_pandas(cx.get_meta(dsn, sql, protocol=transport.protocol))
    queries = cx.partition_sql(dsn, sql, transport.partition_on, transport.partition_num) if op is RemoteOp.PARTITION and transport.partition_on else sql
    result = cx.read_sql(
        dsn, queries, return_type="arrow_stream" if op is RemoteOp.STREAM else "arrow", protocol=transport.protocol,
        partition_on=transport.partition_on if isinstance(queries, str) else None,
        partition_num=transport.partition_num if isinstance(queries, str) and transport.partition_on else None,
    )
    return result.read_all() if op is RemoteOp.STREAM else result


def _flightsql(sql: str, dsn: str, op: RemoteOp, transport: Transport, frames: Frames) -> pa.Table:
    from adbc_driver_flightsql import ConnectionOptions, DatabaseOptions, StatementOptions  # noqa: PLC0415
    from adbc_driver_flightsql import dbapi as flight  # noqa: PLC0415

    with flight.connect(
        dsn, db_kwargs=transport.db_kwargs(DatabaseOptions) or None,
        conn_kwargs=transport.conn_kwargs(StatementOptions, ConnectionOptions) or None,
    ) as conn:
        return _adbc_dispatch(conn, sql, op, transport, frames)


_REMOTE: Final[frozendict[RemoteDriver, Callable[[str, str, RemoteOp, Transport, Frames], pa.Table]]] = frozendict({
    RemoteDriver.ADBC: _adbc,
    RemoteDriver.CONNECTORX: _connectorx,
    RemoteDriver.FLIGHTSQL: _flightsql,
})


# the read-parallel ops ConnectorX accelerates over ADBC's serial pull; INGEST stays on ADBC
# (ConnectorX is read-only), PROBE on ADBC's native `adbc_get_table_schema`.
_CX_OPS: Final[frozenset[RemoteOp]] = frozenset({RemoteOp.READ, RemoteOp.STREAM, RemoteOp.PARTITION})


_DAFT_READ: Final[frozendict[LakehouseFormat, tuple[str, str | None]]] = frozendict({
    LakehouseFormat.PARQUET: ("read_parquet", None),
    LakehouseFormat.ICEBERG: ("read_iceberg", "snapshot_id"),
    LakehouseFormat.DELTA: ("read_deltalake", "version"),
    LakehouseFormat.HUDI: ("read_hudi", None),
    LakehouseFormat.LANCE: ("read_lance", "version"),
    LakehouseFormat.SQL: ("read_sql", None),
})


def _daft_scan(daft: Any, plan: StreamingPlan) -> Any:
    name, version_key = _DAFT_READ[plan.fmt]
    reader = getattr(daft, name)
    if plan.fmt is LakehouseFormat.SQL:
        return reader(
            plan.sql, plan.conn, partition_col=plan.partition_col, num_partitions=plan.num_partitions,
            partition_bound_strategy=plan.partition_strategy, disable_pushdowns_to_sql=plan.disable_pushdowns,
        )
    travel = {version_key: plan.version} if version_key and plan.version is not None else {}
    travel |= {"asof": plan.asof} if plan.fmt is LakehouseFormat.LANCE and plan.asof else {}
    travel |= {"block_size": plan.block_size} if plan.fmt is LakehouseFormat.LANCE and plan.block_size is not None else {}
    return reader(plan.source, io_config=plan.io_config, **travel)


def _stream(plan: StreamingPlan, runner: Runner, cluster: str | None, frames: Frames) -> pa.Table:
    import daft  # noqa: PLC0415

    daft.set_runner_ray(address=cluster) if runner is Runner.RAY else daft.set_runner_native()
    scan = _daft_scan(daft, plan)
    bound = (
        daft.sql(plan.sql, register_globals=False, **{name: daft.from_arrow(frame) for name, frame in frames.items()} | {"this": scan})
        if plan.sql and plan.fmt is not LakehouseFormat.SQL
        else scan
    )
    shaped = bound.where(plan.predicate) if plan.predicate else bound
    shaped = shaped.with_columns({name: daft.sql_expr(expr) for name, expr in plan.with_columns.items()}) if plan.with_columns else shaped
    shaped = shaped.groupby(*plan.group_by).agg(*(daft.sql_expr(expr) for expr in plan.agg)) if plan.group_by else shaped
    shaped = shaped.explode(*plan.explode) if plan.explode else shaped
    shaped = shaped.distinct(*plan.distinct) if plan.distinct is not None else shaped
    shaped = shaped.sample(plan.sample) if plan.sample is not None else shaped
    shaped = shaped.sort(list(plan.sort), desc=plan.sort_desc) if plan.sort else shaped
    shaped = shaped.select(*plan.project) if plan.project else shaped
    shaped = shaped.limit(plan.limit) if plan.limit is not None else shaped
    shaped = shaped.repartition(plan.repartition, *plan.repartition_by) if plan.repartition else shaped
    return shaped.to_arrow()


# --- [RECEIPTS] -------------------------------------------------------------------------

type Provenance = tuple[str, int, tuple[LineageEdge, ...]]


# the predicate count is the one fold the lower `columnar#SCAN` owner exports — `query` depends on
# `columnar`, so both the `_PREDICATE_NODES` widening AND the `parse_one().find_all()` application
# home in the lower owner and this owner imports and calls `predicate_count`, never re-spelling the
# byte-identical fold (the DERIVED_LOGIC/ONE_HOP correspondence is the application, not only the tuple).
def _provenance(spec: QuerySpec) -> Provenance:
    match spec:
        case QuerySpec(tag="sql", sql=(text, gate)):
            return text, predicate_count(text), (gate or SqlGate()).edges(text)
        case QuerySpec(tag="ir", ir=(expr, emit)):
            text = emit.sql(expr)
            return text, predicate_count(text), SqlGate(read=emit.dialect, write=emit.dialect).edges(text)
        case QuerySpec(tag="rel", rel=(flt, _, _)):
            return spec.tag, int(flt is not None), ()
        case QuerySpec(tag="agnostic", agnostic=(_, predicates, _, _)):
            return spec.tag, len(predicates), ()
        case QuerySpec(tag="remote", remote=(sql, dsn, _, _, _)):
            return dsn, predicate_count(sql), SqlGate().edges(sql)
        case QuerySpec(tag="streaming", streaming=(plan, _, _)):
            return plan.source, predicate_count(plan.sql) if plan.sql else int(plan.predicate is not None), SqlGate().edges(plan.sql) if plan.sql else ()
        case unreachable:
            assert_never(unreachable)


def receipt_of(spec: QuerySpec, table: pa.Table) -> "RuntimeRail[QueryReceipt]":
    # railed because `QueryReceipt.railed` derives the content key off the canonical Arrow bytes
    # through the railed `ContentIdentity.of` — the same fold `columnar#SCAN` `scan`/`write` thread,
    # never the non-railed `QueryReceipt.of` that demands an already-resolved key.
    source, predicates, edges = _provenance(spec)
    return QueryReceipt.railed(spec.tag, source, table, predicate_count=predicates, lineage_edges=edges)
```

## [03]-[RESEARCH]

- [SQLGLOT_AST_ACCESSORS]: the `sqlglot` `parse_one(dialect=, error_level=)`/`Expr.sql(dialect=)`/`optimizer.optimize(schema=, dialect=)`/`optimizer.qualify.qualify(schema=, dialect=)`/`Dialect.get_or_raise`/`Dialects`/`ErrorLevel`/`exp.Column`/`find_all(*expression_types)`/`find_tables(expression) -> set[Table]` surface the `SqlGate.transpile`/`edges` path and the imported `columnar#SCAN` `predicate_count` fold transcribe is catalogue-confirmed against the folder `sqlglot` `.api` — `find_all` confirmed variadic over node types, so the `_PREDICATE_NODES` multi-type predicate scan is one `find_all` call, and `ErrorLevel.{IGNORE,WARN,RAISE,IMMEDIATE}` threaded through `parse_one(error_level=)` is the confirmed parser-error policy row the `SqlGate.errors` field keys; `to_sql(expr, dialect)`/`ibis.duckdb.connect()`/`BaseBackend.to_pyarrow(expr)`/`parse_sql(sqlstring, catalog, dialect)` the `IrEmit`/`Ir` path binds are catalogue-confirmed against the folder `ibis-framework` `.api` — `ibis.duckdb.connect()` the confirmed default-backend accessor, `ibis.to_sql(expr, dialect)` the confirmed module-level emitter, and `ibis.parse_sql(sqlstring, catalog, dialect)` the confirmed SQL→expression ingest row [08] the `IrEmit.bound`/`parse` round-trip drives; the `duckdb-substrait` `con.install_extension("substrait", repository="community")`/`load_extension("substrait")`/`get_substrait`/`get_substrait_json(query) -> DuckDBPyRelation`/`from_substrait(proto)`/`from_substrait_json(json)` round-trip the `PlanWire.SUBSTRAIT`/`SUBSTRAIT_JSON` `_ir_plan` fold binds is catalogue-confirmed against the folder `duckdb-substrait` `.api` (the four connection-bound methods plus the `.fetchone()[0]` plan-artifact read), and the `qualify(infer_schema=, validate_qualify_columns=)` policy the `SqlGate._qualified` fold threads is catalogue-confirmed against the `sqlglot` `.api` row [06]. `to_pyarrow_batches(*, chunk_size, ...) -> RecordBatchReader` the `IrEmit.streaming` arm reads is catalogue-confirmed against the `ibis-framework` `.api` execution-interface row [04] (the incremental record-batch streaming sink on the bound backend), so the streaming arm is settled. `BaseBackend.con` — the native `DuckDBPyConnection` accessor the `_ir_plan` Substrait round-trip drives off the ibis DuckDB backend to reach `install_extension`/`get_substrait` — is the catalogued `ibis-framework` `.api` execution-interface row, confirmed against the distribution as a `DuckDBPyConnection`, so `_ir_plan` is settled; `to_pyarrow`/`to_pyarrow_batches`/`to_sql`/`parse_sql`/`connect`/`duckdb.connect` and the whole `duckdb-substrait` four-method surface are confirmed. The column-level lineage fold `SqlGate.edges` drives is catalogue-confirmed against the `sqlglot` `.api`: `lineage.lineage(column, sql, schema=None, sources=None, dialect=None, ...) -> Node` (entrypoint row [08]) traces one output column to its source columns, and the `lineage.Node` carrier (PUBLIC_TYPES [11]) exposes `name`/`source`/`downstream` so the `_leaves` recursion over the `downstream` tree reaches the physical source columns — the catalogue's own lineage gate, never the `find_tables`×`find_all(exp.Column)` cross-product the prior fence used (which mapped every unqualified column to every source). `Select.selects` enumerates the qualified projection the `edges` fold iterates (each entry exposing `alias_or_name`) — the catalogued `sqlglot` `.api` `Expr` accessor row [10], confirmed against the distribution. The `exp.Where`/`exp.Having`/`exp.Qualify`/`exp.Join` node classes the `_PREDICATE_NODES` predicate count names are the core `sqlglot.expressions` subtypes the catalogue's `exp` subtype namespace [01] exposes over the variadic `find_all(*expression_types)` traversal [02], and the lower `columnar#SCAN` owner exports BOTH the `_PREDICATE_NODES` widening AND the `predicate_count(text)` fold over it that the `ScanPlan` `duckdb` arm and this owner's `_provenance` both call — so the predicate-count application (not only the node tuple) is settled and shared one-hop across both owners with no re-spelled `find_all`. The `parse_one`/`qualify`/`optimize`/`Dialect.get_or_raise`/`lineage.lineage`/`to_sql`/`Expr.selects` plane and the `_ir_plan` `BaseBackend.con` accessor are settled against the distribution. The request-scoped connection brackets are settled against the live distribution: `duckdb.DuckDBPyConnection` is a context manager (`__enter__`/`__exit__` plus the catalogued `close`), so the `_duckdb` `with duckdb.connect()` releases the `Sql`/`Rel` connection on every exit once `to_arrow_table` has materialized inside it, and `ibis` `BaseBackend.disconnect()` is confirmed on the backend class, so the `_ir` `try`/`finally` releases the in-process or `backend_uri` backend and the native `backend.con` it owns — the prior unbracketed `duckdb.connect()`/`ibis.connect()` that never released past the run being the deleted leak. `edges` parses the raw `read`-dialect `text` under `self.read` (qualify and `lineage.lineage` alike), the prior `self.write` parse that mis-read a `read != write` source query being the deleted form.
- [REMOTE_PARTITION_DEEPEN]: the `adbc-driver-manager` `dbapi.connect(uri=)`/`Cursor.execute`/`fetch_arrow_table`/`fetch_record_batch`/`adbc_ingest(table_name, data, mode)`/`Connection.adbc_get_table_schema(table_name)` surface the `_adbc`/`_adbc_dispatch` `READ`/`STREAM`/`INGEST`/`PROBE` arms transcribe is catalogue-confirmed against the folder `adbc-driver-manager` `.api` (the `PROBE` arm reading `Schema.names`/`Schema.types` off the returned `pyarrow.Schema`); the DBAPI `Cursor.adbc_execute_partitions(operation, parameters=None) -> (partitions, schema)` (cursor row [12]) and `Cursor.adbc_read_partition(partition) -> RecordBatchReader` (cursor row [13]) the `PARTITION` arm's `_partitions` fold binds are catalogue-confirmed against the `adbc-driver-manager` `.api` cursor surface, with the per-partition read-ahead set through `Connection.cursor(adbc_stmt_kwargs={"adbc.rpc.result_queue_size": ...})` (cursor row [02]) rather than the low-level `_lib` handle layer, and `adbc-driver-flightsql` confirms `Cursor.adbc_execute_partitions`/`adbc_read_partition` as the Flight SQL partition spine (PARTITION_FLIGHTSQL partition axis); the `<3.15`-gated `connectorx` `read_sql(conn, query, return_type=, protocol=, partition_on=, partition_num=)` partitioned/streaming surface (`arrow`/`arrow_stream` egress), the `partition_sql(conn, query, partition_on, partition_num) -> list[str]` explicit planner the `PARTITION` arm calls and feeds back as the `read_sql` `query` list, and the `get_meta(conn, query, protocol=)` schema-probe surface the `_connectorx` `PROBE` arm binds (its `pd.DataFrame` lifted through `pyarrow.Table.from_pandas`) are catalogue-confirmed against the folder `connectorx` `.api`; the `adbc-driver-flightsql` `dbapi.connect(uri, db_kwargs, conn_kwargs)` and the `DatabaseOptions.{AUTHORIZATION_HEADER,AUTHORITY,TLS_ROOT_CERTS,TLS_OVERRIDE_HOSTNAME,MTLS_CERT_CHAIN,MTLS_PRIVATE_KEY,TLS_SKIP_VERIFY,WITH_BLOCK,WITH_COOKIE_MIDDLEWARE,WITH_MAX_MSG_SIZE,TIMEOUT_FETCH,TIMEOUT_QUERY,TIMEOUT_UPDATE,RPC_CALL_HEADER_PREFIX,OAUTH_FLOW,OAUTH_*}`/`OAuthFlowType.{CLIENT_CREDENTIALS,TOKEN_EXCHANGE}`/`ConnectionOptions.OPTION_SESSION_OPTION_PREFIX`/`StatementOptions.{QUEUE_SIZE,SUBSTRAIT_VERSION}` enum-value keys the `Transport.db_kwargs`/`conn_kwargs` projection folds — `RPC_CALL_HEADER_PREFIX` suffixed with each header name, the three `TIMEOUT_*` phases keyed by the `TimeoutPhase` row, the OAuth axis keyed by the `OAuthFlow` row and the `getattr(opts, f"OAUTH_{key.upper()}")` projection over the `Transport.oauth` mapping, `QUEUE_SIZE`/`SUBSTRAIT_VERSION` threaded as `conn_kwargs` statement options, `OPTION_SESSION_OPTION_PREFIX` suffixed with each session-option key, `TLS_SKIP_VERIFY`/`WITH_BLOCK`/`WITH_COOKIE_MIDDLEWARE` emitting the literal `"true"` — are all catalogue-confirmed against the folder `adbc-driver-flightsql` `.api` (the 29 `DatabaseOptions` rows including the 15 `OAUTH_*` keys, the two `OAuthFlowType` values, the `ConnectionOptions`/`StatementOptions` rows). The `"adbc.rpc.result_queue_size"` `_QUEUE_SIZE_KEY` constant the generic-ADBC `_partitions` path threads through `Connection.cursor(adbc_stmt_kwargs=)` is the canonical RPC read-ahead key catalogue-confirmed against the `adbc-driver-flightsql` `StatementOptions.QUEUE_SIZE` value [09] (`adbc.rpc.result_queue_size`, the driver-agnostic ADBC RPC key the manager honours even though the `adbc-driver-manager` `StatementOptions` row enumerates only ingest/bind/incremental), so the read-ahead is settled. The `Remote.dsn` is a caller-supplied DBAPI connection string — `transport/roots#RESOURCE` `TransportResource` is the `http`/`ssh` artifact-fetch union whose `acquire(relative, modality) -> RuntimeRail[Acquired]` returns a fetched body, not a database DSN, so it never resolves this seam; a secret-bearing DSN is resolved caller-side through the `execution/admission#SETTINGS` `SecretBoundary` that mints the outbound credential as a `pydantic` `SecretStr`, the resolved string threaded as the plain `Remote.dsn` input with no second transport owner. The `Cursor.adbc_execute_partitions`/`adbc_read_partition` partition spine, the `(partitions, schema)` return arity, and the `Connection.cursor(adbc_stmt_kwargs=)` read-ahead seam are settled against the cursor `.api` rows. The serial `dbapi.connect`/`fetch_arrow_table`/`fetch_record_batch`/`adbc_ingest`/`adbc_get_table_schema`/`get_meta`/`partition_sql` transport surface and the whole `Transport` option-keyed `db_kwargs`/`conn_kwargs` projection (transport, OAuth, session, and Substrait-version axes) are settled. The remote-leg resilience binding is settled: the `Remote` arm rides `guarded(RetryClass.REMOTE_DB, ...)`, the `reliability/resilience#RESILIENCE` `REMOTE_DB` row whose `_adbc_transient(ConnectionError, TimeoutError)` `BackoffHook` retries an ADBC `dbapi.OperationalError` only when its `status_code` is `AdbcStatusCode.TIMEOUT`/`IO` — reflection-confirmed that `adbc_driver_manager.dbapi.OperationalError` subclasses `DatabaseError`/`Error`/`Exception` (never `ConnectionError`/`TimeoutError`/`OSError`) and carries a keyword-only `status_code: AdbcStatusCode` slot whose `TIMEOUT`/`IO` members are the transport transients, so the prior `RetryClass.RPC` arm (whose `_named("RPCServerError","RPCClientError")` COMPAS XML-RPC qualname hook matched none of these) retried nothing; both ADBC dists are cp315-clean so the resilience owner type-imports the root rather than `_named`.
- [DAFT_ELASTICITY]: the `daft` `read_parquet`/`read_iceberg(snapshot_id=)`/`read_deltalake(version=)`/`read_hudi`/`read_lance(version=, asof=)`/`read_sql(sql, conn, partition_col=, num_partitions=, partition_bound_strategy=)`/`from_arrow`/`sql(sql, register_globals=, **bindings)`/`DataFrame.where`/`distinct`/`sort`/`select`/`limit`/`repartition`/`set_runner_native`/`set_runner_ray(address=)`/`DataFrame.to_arrow() -> pyarrow.Table` surface the `_stream`/`_daft_scan`/`_DAFT_READ` path transcribes is catalogue-confirmed against the folder `daft` `.api` (cp310-abi3, importing on cp315 without source-build per the catalogue install note; `to_arrow` confirmed to land a `pyarrow.Table` directly, so the uniform-Arrow contract treats the daft egress as a settled `pyarrow.Table` with no `arro3`/`nanoarrow` capsule intermediary); the `read_sql(partition_col=, num_partitions=, partition_bound_strategy=)` native partition pushdown replaces the prior ConnectorX-only partition story for the daft engine, the `_DAFT_READ` row's per-format time-travel key (`snapshot_id` for Iceberg, `version` for Delta/Lance, `asof` the confirmed Lance timestamp axis) and the per-reader `io_config` thread the catalogue-confirmed object-store and snapshot arguments, and `daft.sql(plan.sql, register_globals=False, **{name: daft.from_arrow(frame) ...} | {"this": scan})` seals the binding to the registered input frames plus the lakehouse scan so the SQL transform resolves them rather than leaking an unregistered global, with the `plan.predicate`/`with_columns`/`group_by`×`agg`/`explode`/`distinct`/`sample`/`sort`/`project`/`limit`/`repartition` shaping pushed into the lazy plan through `DataFrame.where`/`with_columns`/`groupby`/`agg`/`explode`/`distinct`/`sample`/`sort(desc=)`/`select`/`limit`/`repartition(num, *partition_by)` before the `to_arrow` sink — `distinct=None` skips dedup, `distinct=()` dedups all columns, `distinct=(cols)` dedups on keys, the one polymorphic dedup row, and each expression-taking verb compiling its string through `daft.sql_expr`. The expanded shaping surface (`with_columns`/`groupby`/`agg`/`explode`/`sample`/`sort(desc=)`/`repartition(num, *partition_by)`/`sql_expr`), the `read_lance(block_size=)` scan tuning, and the `read_sql(disable_pushdowns_to_sql=)` pushdown control are all catalogue-confirmed against the folder `daft` `.api` (the `DataFrame` transform table rows [01]-[16] and the expression-factory `sql_expr` row). The daft `RAY` runner cluster address is a caller-supplied string the `Streaming` case carries as its third payload slot, destructured at the `match` arm and threaded into `daft.set_runner_ray(address=cluster)` — not a `run(cluster=)` signature param beside the `QuerySpec` and not a `TransportResource`-resolved value, since `TransportResource` fetches `http`/`ssh` artifacts and owns no Ray-address resolution; the lazy-plan/runner/read/time-travel/`io_config`/`from_arrow`/`sql`-binding/full-shaping/`to_arrow` surface is settled. The streaming-leg resilience binding is settled: the `Streaming` arm rides `guarded(RetryClass.STREAMING, ...)`, the `reliability/resilience#RESILIENCE` `STREAMING` row whose `(DaftTransientError, TimeoutError, ConnectionError)` tuple catches `daft.exceptions.DaftTransientError` — reflection-confirmed the single Rust-backed base for the `ConnectTimeoutError`/`ReadTimeoutError`/`ByteStreamError`/`SocketError`/`ThrottleError`/`MiscTransientError` object-store-and-network transients (all subclassing `DaftTransientError` -> `DaftCoreException` -> `ValueError`), so the prior `RetryClass.WIRE` arm (whose `(ConnectionError,)` intra-mesh tuple matched no daft Rust fault) retried nothing; `daft` is cp315-clean so the resilience owner type-imports the transient base.
