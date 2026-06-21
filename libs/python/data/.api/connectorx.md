# [PY_DATA_API_CONNECTORX]

`connectorx` is the parallel, zero-copy database-to-dataframe reader for the data query rail: a single `read_sql` polymorphic entry executes one SQL query across range partitions in a Rust thread pool and reconstructs the wire stream directly into Pandas, Polars, Arrow (table or `RecordBatchReader`), Modin, or Dask without an intermediate Python-object roundtrip. `partition_sql` exposes the partition planner, `get_meta` probes the result schema, and `ConnectionUrl` builds the typed connection string per backend. A `dict` connection federates multiple sources behind one query. The data package owner composes `read_sql` + `ConnectionUrl` + `partition_on` into the in-process SQL ingest path and feeds the Arrow/Polars frame straight into the columnar interop or contract owner; it never hand-rolls cursor iteration, per-frame read wrappers, or string-concatenated connection strings.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `connectorx`
- package: `connectorx`
- import: `import connectorx as cx`
- owner: `data`
- rail: query
- asset: native Rust/maturin extension (`connectorx.connectorx` PyO3 core); license MIT
- floor: marker-gated `python_full_version < '3.15'` (specifier `>=0.4.5`); the maturin build ships no CPython 3.15 wheel, so this reader is unavailable on the cp315 core and resolves only on the `<3.15` companion band
- entry points: library use is import-only; no console script
- capability: parallel partitioned database reads, zero-copy reconstruction into Pandas/Polars/Arrow/`RecordBatchReader`/Modin/Dask, multi-source federation through a `dict` connection, per-backend wire-protocol selection (`binary`/`cursor`/`text`/`csv`/`simple`), pre-execution session setup, server-side-pagination strategy, and result-schema probing without materializing rows

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `connectorx.ConnectionUrl` — generic `str` subclass `ConnectionUrl[BackendT]` building a typed connection string through overloaded `__new__`: `ConnectionUrl(raw_connection)` accepts a finished DSN; `ConnectionUrl(*, backend='sqlite'|'bigquery', db_path=...)` builds a file/path backend; `ConnectionUrl(*, backend=<server>, username=, password='', server=, port=, database='', database_options=None)` builds a server DSN where `<server>` is one of `redshift`/`clickhouse`/`postgres`/`postgresql`/`mysql`/`mssql`/`oracle`/`duckdb`. The result is usable anywhere a connection string is accepted, including as a `dict` value for federation.
- `connectorx.Protocol` — `Literal['csv', 'binary', 'cursor', 'simple', 'text']` wire-protocol selector threaded through every read and metadata entry.

[ENTRYPOINTS]:
- primary read: `read_sql(conn, query, *, return_type='pandas', protocol=None, partition_on=None, partition_range=None, partition_num=None, index_col=None, strategy=None, pre_execution_query=None, **kwargs) -> pd.DataFrame | pl.DataFrame | pa.Table | pa.RecordBatchReader | mpd.DataFrame | dd.DataFrame`.
- pandas alias: `read_sql_pandas(sql, con, index_col=None, protocol=None, partition_on=None, partition_range=None, partition_num=None, pre_execution_queries=None) -> pd.DataFrame` — `con` accepts `str`/`ConnectionUrl`/federation `dict`; this is the SQLAlchemy-shaped positional alias, not a distinct capability.
- partition planning: `partition_sql(conn, query, partition_on, partition_num, partition_range=None) -> list[str]` — returns the explicit per-partition subqueries the planner would run, for inspection or hand-tuned re-issue.
- schema probe: `get_meta(conn, query, protocol=None) -> pd.DataFrame` — returns an empty frame carrying the resolved column dtypes without fetching rows.
- connection rewrite: `rewrite_conn(conn, protocol=None) -> tuple[str, Protocol]` — normalizes a DSN/`ConnectionUrl` and resolves the default protocol for the backend.
- reconstruction helpers: `reconstruct_arrow(result) -> pa.Table`, `reconstruct_arrow_rb(results) -> pa.RecordBatchReader`, `reconstruct_pandas(df_infos) -> pd.DataFrame` — internal FFI reassembly entries the high-level `return_type` rail dispatches into; consume `read_sql(return_type=...)`, not these directly.

[IMPLEMENTATION_LAW]:
- `read_sql` is the sole polymorphic read; `return_type` discriminates `pandas`/`polars`/`arrow`/`arrow_stream`/`modin`/`dask` egress over one wire reconstruction, never separate per-frame entry points. `arrow_stream` returns a `pa.RecordBatchReader` for incremental consumption; every other return type materializes a full frame.
- parallelism comes from `partition_on` (a numeric or temporal column) plus `partition_num`; the Rust planner splits the query into balanced range subqueries fetched concurrently, and `partition_range` bounds the split when the min/max are known to skip the bounding probe.
- `query` accepts either a single SQL string for planner-partitioned reads or a `list[str]` of explicit per-partition queries; the list form bypasses the planner and runs the supplied subqueries directly.
- `conn` accepts a connection string, a `ConnectionUrl`, or a `dict` mapping source names to connection strings/`ConnectionUrl`s; the `dict` form federates a multi-source query in one read.
- `protocol` selects the per-backend wire format as performance tuning, not a separate API: `binary` is the fastest PostgreSQL path, `cursor` streams server-side, `simple` is the unprepared statement form, and `csv`/`text` suit engines without a binary protocol; `None` lets `rewrite_conn` pick the backend default.
- `strategy` selects the server-side pagination/keyset strategy for partitioned reads; `pre_execution_query` runs one or more session-setup statements (timeouts, `SET` parameters, temp tables) before the main query on the same connection.

## [03]-[INTEGRATION]

[STACKS_WITH]:
- arro3-core / pyarrow: `read_sql(return_type='arrow')` reconstructs the wire stream straight into a `pa.Table`, and `return_type='arrow_stream'` yields a `pa.RecordBatchReader` that feeds the columnar interop owner or a `datafusion`/`duckdb` registration without a Python-row hop; this is the canonical zero-copy database egress.
- polars: `read_sql(return_type='polars')` returns a `pl.DataFrame` over the same Arrow buffers, landing the read directly in the eager columnar owner; the lazy `pl.scan_*` rail does not cover live SQL sources, so connectorx is the parallel database-to-Polars front door.
- dataframely: the `pl.DataFrame` from `return_type='polars'` flows into a `Schema.validate`/`filter` gate so a partitioned database read is contract-checked at the ingest boundary before downstream use; `get_meta` pre-validates the column dtypes against `Schema.to_polars_schema` before a full fetch.
- daft: daft's own `read_sql(partition_col=, num_partitions=)` owns the lazy out-of-core distributed scan; connectorx owns the eager in-process parallel pull into a single host frame. Route narrow, fully-materializable result sets through connectorx and large out-of-core lakehouse-scale SQL through daft — they are the in-process and distributed halves of one SQL-ingest concern, not duplicate readers.
- adbc-driver-manager: ADBC owns standards-driven driver-managed Arrow database access with prepared statements and bulk ingest; connectorx owns parallel range-partitioned reads with per-backend protocol tuning. Both egress Arrow — select connectorx when partition-parallel throughput on a single query dominates, ADBC when driver-managed statement lifecycle or write-back is needed.
- duckdb / datafusion: feed the `pa.RecordBatchReader` from `arrow_stream` into `con.register`/`SessionContext.register_record_batches` to land an external-database result inside the embedded engine for a federated join without an intermediate file.

[LOCAL_ADMISSION]:
- A database read composes as `read_sql(ConnectionUrl(...), query, return_type='arrow'|'polars', partition_on=..., partition_num=...)`; the egress frame type is a call row on the one read, never a per-frame wrapper.
- `ConnectionUrl` is the only connection constructor; backends, credentials, and options are typed kwargs, never string concatenation, and federation is a `dict` of `ConnectionUrl`s, never N serial reads stitched in Python.
- `partition_on`/`partition_num` own parallelism; a serial single-partition read where a numeric/temporal partition column exists is rejected. `get_meta` probes the schema before a heavy fetch; `pre_execution_query` carries session setup; `strategy` carries pagination.
- connectorx reads are companion-band (`<3.15`) offline ingest evidence; the resulting Arrow/Polars frame crosses to the cp315 core through the columnar interop owner, never as a live connectorx handle.

## [04]-[RAIL_LAW]

- Package: `connectorx`
- Owns: parallel range-partitioned database-to-dataframe reads, zero-copy multi-frame reconstruction (Pandas/Polars/Arrow/`RecordBatchReader`/Modin/Dask), multi-source federation, per-backend wire-protocol selection, server-side pagination strategy, pre-execution session setup, and result-schema probing
- Accept: `read_sql` as the polymorphic read with `return_type` egress selection, `partition_on`/`partition_num`/`partition_range` for parallelism, `ConnectionUrl` for typed connection construction, a `dict` connection for federation, `arrow`/`arrow_stream` egress feeding the columnar interop owner, and the resulting `pl.DataFrame` feeding a dataframely contract gate
- Reject: per-frame read wrappers, serial single-partition reads where a partition column exists, hand-rolled cursor iteration, connection strings assembled by string concatenation instead of `ConnectionUrl`, and treating connectorx as a duplicate of daft's distributed `read_sql` rather than its eager in-process counterpart
