# [PY_DATA_API_CONNECTORX]

`connectorx` supplies a parallel, zero-copy database-to-dataframe reader that executes SQL across partitions and reconstructs Pandas, Polars, Arrow, Modin, or Dask results. `read_sql` is the single polymorphic entry; `partition_sql` and `get_meta` expose the partition planner and schema probe; `ConnectionUrl` builds the source connection string.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `connectorx`
- package: `connectorx`
- import: `import connectorx as cx`
- owner: `data`
- rail: query
- capability: parallel partitioned database reads, zero-copy reconstruction into Pandas/Polars/Arrow/Modin/Dask, multi-source federation, wire-protocol selection, and schema metadata probing

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `connectorx.ConnectionUrl` — `str` subclass that builds a connection string from components; constructed via `ConnectionUrl(raw_connection=None, *, backend='', username='', password='', server='', port=None, database='', database_options=None, db_path='')`; usable anywhere a connection string is accepted.
- `connectorx.Protocol` — `Literal['csv', 'binary', 'cursor', 'simple', 'text']` wire-protocol selector passed as `protocol=` to read and metadata entrypoints.

[ENTRYPOINTS]:
- primary read: `read_sql(conn, query, *, return_type='pandas', protocol=None, partition_on=None, partition_range=None, partition_num=None, index_col=None, strategy=None, pre_execution_query=None, **kwargs) -> pd.DataFrame | pl.DataFrame | pa.Table | pa.RecordBatchReader | mpd.DataFrame | dd.DataFrame`.
- pandas alias: `read_sql_pandas(sql, con, index_col=None, protocol=None, partition_on=None, partition_range=None, partition_num=None, pre_execution_queries=None) -> pd.DataFrame`.
- partition planning: `partition_sql(conn, query, partition_on, partition_num, partition_range=None) -> list[str]`.
- schema probe: `get_meta(conn, query, protocol=None) -> pd.DataFrame`.
- connection rewrite: `rewrite_conn(conn, protocol=None) -> tuple[str, Protocol]`.
- reconstruction helpers: `reconstruct_arrow(result) -> pa.Table`, `reconstruct_arrow_rb(results) -> pa.RecordBatchReader`, `reconstruct_pandas(df_infos) -> pd.DataFrame`.

[IMPLEMENTATION_LAW]:
- `read_sql` is the sole polymorphic read; `return_type` discriminates `pandas`/`polars`/`arrow`/`arrow_stream`/`modin`/`dask` egress without separate per-frame entry points.
- Parallelism comes from `partition_on` (a numeric or temporal column) plus `partition_num`; the planner splits the query into balanced range subqueries, and `partition_range` bounds the split when known.
- `query` accepts either a single SQL string for partitioned reads or a `list[str]` of explicit per-partition queries; the list form bypasses the automatic planner.
- `conn` accepts a connection string, a `ConnectionUrl`, or a `dict` mapping source names to connections for federated multi-source queries.
- `protocol` selects the wire format per backend; `binary` is fastest for PostgreSQL, `cursor` streams, and `csv`/`text` suit other engines, so the choice is performance tuning, not a separate API.
- `arrow_stream` returns a `pa.RecordBatchReader` for incremental consumption; the other return types materialise a full frame.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `connectorx`
- Owns: parallel partitioned database-to-dataframe reads, zero-copy multi-frame reconstruction, multi-source federation, wire-protocol selection, and schema probing
- Accept: `read_sql` as the polymorphic read with `return_type` egress selection, `partition_on`/`partition_num` for parallelism, `ConnectionUrl` for connection construction, `dict` connections for federation
- Reject: per-frame read wrappers, serial single-partition reads where a partition column exists, hand-rolled cursor iteration, and connection strings assembled by string concatenation instead of `ConnectionUrl`
