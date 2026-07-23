# [PY_DATA_API_CONNECTORX]

`connectorx` is the parallel, zero-copy database-to-dataframe reader on the data query rail: `read_sql` executes one SQL query across range partitions in a Rust thread pool and reconstructs the wire stream directly into Pandas, Polars, Arrow (table or `RecordBatchReader`), Modin, or Dask with no Python-object roundtrip. `partition_sql` exposes the planner, `get_meta` probes the result schema, `ConnectionUrl` builds the typed per-backend connection string, and a `dict` connection federates multiple sources behind one query.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `connectorx`
- package: `connectorx` (MIT)
- module: `connectorx`
- asset: native Rust/maturin extension (`connectorx.connectorx` PyO3 core)
- owner: `data`
- rail: query

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: typed connection constructor and the wire-protocol vocabulary

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :-------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `ConnectionUrl` | class         | typed per-backend DSN builder, a `str` subclass usable as any DSN value |
|  [02]   | `Protocol`      | literal       | wire-protocol selector `csv`/`binary`/`cursor`/`simple`/`text`          |

- `ConnectionUrl` `backend` admits `sqlite`/`bigquery` (file/path) and `redshift`/`clickhouse`/`postgres`/`postgresql`/`mysql`/`mssql`/`oracle`/`duckdb` (server).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the polymorphic read, planner, schema probe, and connection construction
- partition carry (`read_sql`, `partition_sql`, `read_sql_pandas`): `partition_on`, `partition_num`, `partition_range`

| [INDEX] | [SURFACE]                                                 | [SHAPE] | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------- | :------ | :---------------------------------------------- |
|  [01]   | `read_sql(conn, query, *, return_type, ...) -> DataFrame` | static  | the sole polymorphic read; `return_type` egress |
|  [02]   | `partition_sql(conn, query) -> list[str]`                 | static  | the per-partition subqueries the planner issues |
|  [03]   | `get_meta(conn, query, protocol) -> pd.DataFrame`         | static  | resolved column dtypes, no rows fetched         |
|  [04]   | `ConnectionUrl(raw)` / `ConnectionUrl(*, backend, ...)`   | ctor    | typed per-backend DSN construction              |
|  [05]   | `rewrite_conn(conn, protocol) -> tuple[str, Protocol]`    | static  | normalize a DSN, resolve the backend default    |

- `read_sql` keyword contract: `return_type`, `protocol`, `index_col`, `strategy`, `pre_execution_query`, over the partition carry.
- `ConnectionUrl` server form carries `username`, `password`, `server`, `port`, `database`, `database_options`; the file/path form carries `db_path`.
- `read_sql`: `query` accepts one SQL string (planner-partitioned) or a `list[str]` of explicit per-partition queries (planner bypassed); `conn` accepts a string, a `ConnectionUrl`, or a `dict` federating source-name to DSN.
- `read_sql(return_type='arrow_stream')` yields a `pa.RecordBatchReader` for incremental consumption; every other return type materializes a full frame.
- `read_sql_pandas(sql, con, ...)` is the SQLAlchemy-shaped positional alias over `read_sql`, not a distinct capability; `reconstruct_arrow`/`reconstruct_arrow_rb`/`reconstruct_pandas` are internal FFI reassembly the `return_type` rail dispatches into, not a direct call surface.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `read_sql` is the sole polymorphic read; `return_type` discriminates `pandas`/`polars`/`arrow`/`arrow_stream`/`modin`/`dask` egress over one wire reconstruction.
- parallelism binds `partition_on` (a numeric or temporal column) with `partition_num`; the Rust planner splits into balanced range subqueries fetched concurrently, and `partition_range` supplies known min/max to skip the bounding probe.
- `protocol` tunes the per-backend wire format: `binary` fastest for PostgreSQL, `cursor` streams server-side, `simple` the unprepared form, `csv`/`text` for engines without a binary protocol; `None` defers to `rewrite_conn` for the backend default.
- `strategy` selects the server-side pagination/keyset strategy; `pre_execution_query` runs session-setup statements (timeouts, `SET`, temp tables) on the connection before the main query.

[STACKING]:
- `arro3-core`(`arro3-core.md`), `pyarrow`(`pyarrow.md`): `return_type='arrow'` reconstructs straight into a `pa.Table`; `'arrow_stream'` yields a `pa.RecordBatchReader` feeding the columnar interop owner with no Python-row hop.
- `polars`(`polars.md`): `return_type='polars'` lands the read in the eager columnar owner over the same Arrow buffers; the lazy `pl.scan_*` rail carries no live SQL source, so `connectorx` is the parallel database-to-Polars front door.
- `dataframely`(`dataframely.md`): the `polars` frame flows into a `Schema.validate`/`filter` gate; `get_meta` pre-checks column dtypes against `Schema.to_polars_schema` before a full fetch.
- `duckdb`(`duckdb.md`), `datafusion`(`datafusion.md`): the `arrow_stream` `RecordBatchReader` registers via `con.register`/`SessionContext.register_record_batches` for a federated join with no intermediate file.
- `daft`(`daft.md`): `daft.read_sql(partition_col, num_partitions)` owns the lazy out-of-core distributed scan; `connectorx` owns the eager in-process parallel pull — narrow materializable result sets here, lakehouse-scale SQL to daft.
- `adbc-driver-manager`(`adbc-driver-manager.md`): ADBC owns driver-managed Arrow access with prepared statements and write-back; `connectorx` owns partition-parallel single-query throughput — select connectorx for throughput on one query, ADBC for statement lifecycle or write-back.

[LOCAL_ADMISSION]:
- read through `read_sql(ConnectionUrl(...), query, return_type=..., partition_on=..., partition_num=...)`; the egress frame is a call arg, never a per-frame wrapper.
- construct connections through `ConnectionUrl` typed kwargs; federate through a `dict` of `ConnectionUrl`s; `get_meta` probes schema before a heavy fetch.

[RAIL_LAW]:
- Package: `connectorx`
- Owns: parallel range-partitioned database-to-dataframe reads, zero-copy multi-frame reconstruction (Pandas/Polars/Arrow/`RecordBatchReader`/Modin/Dask), multi-source federation, per-backend wire-protocol selection, server-side pagination, pre-execution session setup, and result-schema probing
- Accept: `read_sql` as the polymorphic read with `return_type` egress, `partition_on`/`partition_num`/`partition_range` for parallelism, `ConnectionUrl` for typed connection construction, a `dict` connection for federation, `arrow`/`arrow_stream` feeding the columnar interop owner, and the `pl.DataFrame` feeding a dataframely contract gate
- Reject: per-frame read wrappers, serial single-partition reads where a partition column exists, hand-rolled cursor iteration, connection strings built by concatenation instead of `ConnectionUrl`, and treating connectorx as a duplicate of daft's distributed `read_sql` rather than its eager in-process counterpart
