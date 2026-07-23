# [PY_DATA_API_PYICEBERG]

`pyiceberg` binds the Apache Iceberg table format from Python: catalog DDL, table read and write, schema and partition construction, expression-filtered scans, and multi-engine tabular egress. `Catalog` owns metastore lifecycle behind the single polymorphic `load_catalog` entry, `Transaction` owns every write, and `TableScan`/`DataScan` own lazy filtered egress across Arrow, Polars, DuckDB, and Ray.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyiceberg`
- package: `pyiceberg` (Apache-2.0)
- module: `pyiceberg`
- namespaces: `pyiceberg.catalog`, `pyiceberg.table`, `pyiceberg.schema`, `pyiceberg.types`, `pyiceberg.transforms`, `pyiceberg.partitioning`, `pyiceberg.expressions`, `pyiceberg.exceptions`
- owner: `data`
- rail: iceberg

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: catalog, table, scan, and evolution owners

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                      |
| :-----: | :----------------- | :------------ | :-------------------------------------------------------------------------------- |
|  [01]   | `Catalog`          | class         | abstract metastore lifecycle owner behind `load_catalog`                          |
|  [02]   | `MetastoreCatalog` | class         | metastore base sharing the `Catalog` surface                                      |
|  [03]   | `CatalogType`      | enum          | backend discriminant `REST` `HIVE` `SQL` `GLUE` `DYNAMODB` `BIGQUERY` `IN_MEMORY` |
|  [04]   | `Table`            | class         | live managed table: scan, write, snapshot, transaction, inspect                   |
|  [05]   | `StaticTable`      | class         | read-only table from a metadata path, no live catalog                             |
|  [06]   | `Transaction`      | class         | multi-op write unit off `table.transaction()`                                     |
|  [07]   | `TableScan`        | class         | base lazy scan; Arrow/Pandas/Polars egress                                        |
|  [08]   | `DataScan`         | class         | data-file scan; DuckDB/Ray/batch-reader egress                                    |
|  [09]   | `InspectTable`     | class         | metadata inspection off `table.inspect`                                           |
|  [10]   | `ManageSnapshots`  | class         | snapshot, branch, and tag lifecycle                                               |
|  [11]   | `MaintenanceTable` | class         | maintenance owner off `table.maintenance`                                         |
|  [12]   | `ExpireSnapshots`  | class         | snapshot-expiry builder, autocommit on `commit`                                   |
|  [13]   | `UpdateSchema`     | class         | schema-mutation builder, commit on `with`-exit                                    |
|  [14]   | `Schema`           | class         | Iceberg schema of `NestedField` columns; `as_arrow()`                             |
|  [15]   | `NestedField`      | class         | typed column descriptor (id, name, type, required, doc)                           |

[PUBLIC_TYPE_SCOPE]: schema types, transforms, and expression vocabulary

[PRIMITIVE_TYPES]: `BooleanType` `IntegerType` `LongType` `FloatType` `DoubleType` `DecimalType` `StringType` `BinaryType` `UUIDType`
[TEMPORAL_TYPES]: `DateType` `TimeType` `TimestampType` `TimestamptzType` `TimestampNanoType` `TimestamptzNanoType`
[CONTAINER_TYPES]: `ListType` `MapType` `StructType` `FixedType` `UnknownType`
[PARTITIONING]: `PartitionSpec` `PartitionField` `PartitionKey`
[TRANSFORMS]: `IdentityTransform` `BucketTransform` `TruncateTransform` `VoidTransform` `YearTransform` `MonthTransform` `DayTransform` `HourTransform`
[UNBOUND_PREDICATES]: `EqualTo` `NotEqualTo` `LessThan` `LessThanOrEqual` `GreaterThan` `GreaterThanOrEqual` `In` `NotIn` `IsNull` `NotNull` `IsNaN` `NotNaN` `StartsWith` `NotStartsWith`
[COMBINATORS]: `And` `Or` `Not` `AlwaysTrue` `AlwaysFalse`

[PUBLIC_TYPE_SCOPE]: typed failure rail (`pyiceberg.exceptions`)

| [INDEX] | [SYMBOL]                                                                                     | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------------------------------- | :---------------------------- |
|  [01]   | `NoSuchTableError`, `NoSuchNamespaceError`, `NoSuchViewError`                                | identifier not found          |
|  [02]   | `TableAlreadyExistsError`, `NamespaceAlreadyExistsError`                                     | duplicate create              |
|  [03]   | `CommitFailedException`, `CommitStateUnknownException`                                       | write-conflict, unknown state |
|  [04]   | `ValidationError`, `ValidationException`                                                     | schema or spec validation     |
|  [05]   | `ResolveError`                                                                               | expression binding failure    |
|  [06]   | `RESTError`, `ServerError`, `ServiceUnavailableError`, `UnauthorizedError`, `ForbiddenError` | REST transport                |
|  [07]   | `NotInstalledError`                                                                          | optional extra not installed  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: catalog construction and DDL
- `create_table` carry: `location`, `partition_spec`, `sort_order`, `properties`

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :------------------------------------------------------------------------ | :------- | :---------------------------------- |
|  [01]   | `load_catalog(name=None, **properties) -> Catalog`                        | function | polymorphic catalog construction    |
|  [02]   | `list_catalogs() -> list[str]`                                            | function | configured catalog names            |
|  [03]   | `Catalog.create_namespace(namespace, properties={})`                      | instance | create a namespace                  |
|  [04]   | `Catalog.list_namespaces(namespace=())` / `.list_tables(namespace)`       | instance | enumerate namespaces / tables       |
|  [05]   | `Catalog.create_table(identifier, schema, ...) -> Table`                  | instance | create a table                      |
|  [06]   | `Catalog.load_table` / `.drop_table` / `.purge_table`                     | instance | load, drop, or purge a table        |
|  [07]   | `Catalog.rename_table` / `.register_table -> Table`                       | instance | rename or register a table          |
|  [08]   | `Catalog.table_exists` / `.namespace_exists` / `.view_exists -> bool`     | instance | existence probes                    |
|  [09]   | `Catalog.create_table_transaction(...) -> CreateTableTransaction`         | instance | staged table creation               |
|  [10]   | `Catalog.create_table_if_not_exists` / `.create_namespace_if_not_exists`  | instance | idempotent create                   |
|  [11]   | `Catalog.update_namespace_properties(...)` / `.load_namespace_properties` | instance | namespace property evolution        |
|  [12]   | `Catalog.list_views(namespace)` / `.drop_view(identifier)`                | instance | view enumeration / drop             |
|  [13]   | `Catalog.commit_table` / `.supports_server_side_planning -> bool`         | instance | commit metadata / server-plan probe |

[ENTRYPOINT_SCOPE]: scan and egress
- `Table.scan` carry: `row_filter`, `selected_fields`, `case_sensitive`, `snapshot_id`, `options`, `limit`; chain members each return the scan

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Table.scan(...) -> DataScan`                                               | instance | open a lazy filtered scan          |
|  [02]   | `TableScan.filter(expr)` / `.select(*names)` / `.projection()`              | instance | narrow the scan set                |
|  [03]   | `TableScan.use_ref(name)` / `.with_case_sensitive(bool)` / `.update(**kw)`  | instance | ref, case, and option refinement   |
|  [04]   | `TableScan.to_arrow() -> pa.Table`                                          | instance | eager filtered Arrow               |
|  [05]   | `TableScan.to_pandas(**kwargs) -> pd.DataFrame`                             | instance | eager filtered pandas              |
|  [06]   | `TableScan.to_polars() -> pl.DataFrame`                                     | instance | eager filtered Polars              |
|  [07]   | `DataScan.to_arrow_batch_reader() -> pa.RecordBatchReader`                  | instance | out-of-core batch stream           |
|  [08]   | `DataScan.to_duckdb(table_name, connection=None) -> DuckDBPyConnection`     | instance | register scan as a DuckDB relation |
|  [09]   | `DataScan.to_ray() -> ray.data.Dataset`                                     | instance | Ray dataset egress                 |
|  [10]   | `TableScan.count() -> int` / `.plan_files()` / `DataScan.partition_filters` | instance | row count / file plan / filters    |
|  [11]   | `pyiceberg.expressions.parser.parse(expr) -> BooleanExpression`             | function | parse a filter string              |
|  [12]   | `Table.to_polars() -> pl.LazyFrame` / `.to_daft()` / `.to_bodo()`           | instance | whole-table lazy egress            |

[ENTRYPOINT_SCOPE]: write, evolution, and inspection
- write carry: `snapshot_properties`, `branch`
- `upsert` carry: `join_cols`, `when_matched_update_all`, `when_not_matched_insert_all`, `case_sensitive`
- `update_schema` carry: `allow_incompatible_changes`, `case_sensitive`; `add_column` carry: `doc`, `required`, `default_value`

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :----------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `Transaction.append(df) -> None`                                               | instance | append rows                             |
|  [02]   | `Transaction.overwrite(df, overwrite_filter=AlwaysTrue()) -> None`             | instance | filtered overwrite                      |
|  [03]   | `Transaction.delete(delete_filter) -> None`                                    | instance | delete matching rows                    |
|  [04]   | `Transaction.upsert(df, ...) -> UpsertResult`                                  | instance | merge upsert                            |
|  [05]   | `Transaction.dynamic_partition_overwrite(df) -> None`                          | instance | replace only df's partitions            |
|  [06]   | `Transaction.add_files(file_paths, check_duplicate_files=True) -> None`        | instance | register existing data files            |
|  [07]   | `Transaction.commit_transaction() -> Table`                                    | instance | commit the batched ops                  |
|  [08]   | `Transaction.update_schema(...) -> UpdateSchema`                               | instance | open a schema-evolution builder         |
|  [09]   | `Transaction.update_spec() -> UpdateSpec`                                      | instance | partition-spec evolution                |
|  [10]   | `Transaction.update_snapshot(branch='main') -> UpdateSnapshot`                 | instance | snapshot-producing op builder           |
|  [11]   | `Transaction.update_sort_order() -> UpdateSortOrder`                           | instance | sort-order evolution                    |
|  [12]   | `Transaction.set_properties` / `.remove_properties` / `.update_location`       | instance | property and location evolution         |
|  [13]   | `Transaction.update_statistics` / `.upgrade_table_version`                     | instance | statistics and format-version evolution |
|  [14]   | `Table.update_schema(...) -> UpdateSchema`                                     | instance | schema builder off the live table       |
|  [15]   | `UpdateSchema.add_column(path, field_type, ...)` / `.update_column(...)`       | instance | add or alter a column                   |
|  [16]   | `UpdateSchema.delete_column(path)` / `.rename_column(path_from, new_name)`     | instance | portable column drop and rename         |
|  [17]   | `ManageSnapshots.rollback_to_snapshot` / `.create_branch`                      | instance | rollback and branch creation            |
|  [18]   | `ManageSnapshots.create_tag` / `.commit`                                       | instance | tag creation and commit                 |
|  [19]   | `InspectTable.snapshots` / `.manifests` / `.files`                             | property | snapshot, manifest, file tables         |
|  [20]   | `InspectTable.entries` / `.partitions` / `.history`                            | property | entry, partition, history tables        |
|  [21]   | `Table.current_snapshot()` / `.snapshot_by_id(id)` / `.snapshot_by_name(name)` | instance | snapshot lookup by id or name           |
|  [22]   | `Table.snapshot_as_of_timestamp(ts) -> Snapshot`                               | instance | snapshot as of a timestamp              |
|  [23]   | `Table.snapshots` / `.refs` / `.history`                                       | property | snapshot log, refs, history             |
|  [24]   | `Table.schema` / `.schemas` / `.spec` / `.specs` / `.sort_order`               | property | live schema, spec, sort metadata        |
|  [25]   | `Table.properties` / `.name_mapping` / `.format_version`                       | property | live table config metadata              |
|  [26]   | `Table.location` / `.location_provider`                                        | property | table location metadata                 |
|  [27]   | `Table.maintenance -> MaintenanceTable`                                        | property | maintenance accessor                    |
|  [28]   | `MaintenanceTable.expire_snapshots() -> ExpireSnapshots`                       | instance | open the snapshot-expiry builder        |
|  [29]   | `ExpireSnapshots.older_than(dt)` / `.by_id(id)` / `.by_ids(ids)` / `.commit()` | instance | expire clauses, apply on commit         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `load_catalog` is the single polymorphic entry; catalog type discriminates on the `uri` scheme or explicit `type` property, never a `load_rest`/`load_hive` call-site branch.
- Scan construction is lazy: no I/O until `to_arrow`/`to_pandas`/`plan_files`/`count` materializes, and `Table.scan` defaults `row_filter` to `AlwaysTrue()`.
- Every write folds through `Transaction`; `Table.append`/`overwrite`/`delete`/`upsert`/`dynamic_partition_overwrite`/`add_files` are autocommit shorthands opening and committing a one-op transaction, while `Table.transaction()` batches many ops into one snapshot. `upsert` requires `join_cols` or an identifier-field schema; `dynamic_partition_overwrite` replaces only the partitions present in `df`.
- Expression predicates are unbound at construction; binding to a schema happens inside the scan engine.
- Egress is two-grained: `TableScan.to_arrow`/`to_pandas`/`to_polars` materialize a filtered scan eagerly, while `Table.to_polars()` returns a whole-table `pl.LazyFrame` whose lazy graph lets both PyIceberg's planner and Polars' optimizer push predicates.
- `expire_snapshots` is the only maintenance op with a Python entry; data-file compaction and orphan-file removal have none.

[STACKING]:
- `pyarrow`(`pyarrow.md`): `to_arrow()`/`to_arrow_batch_reader()` yield the exact `pa.Table`/`pa.RecordBatchReader` shape `Transaction.append`/`overwrite`/`add_files`/`dynamic_partition_overwrite` accept, so a read-transform-write loop never leaves Arrow memory; `Schema.as_arrow()` and a raw `pa.Schema` both feed `create_table`/`create_table_transaction`, and `IcebergType.model_validate("string")` yields `StringType()` for a `(name, type-string)` column-add.
- `polars`(`polars.md`): `Table.to_polars()` returns a `pl.LazyFrame`, and `pl.scan_iceberg(table)` adopts the live `Table` delegating filter planning to PyIceberg — the inverse entry when the pipeline root is a Polars `LazyFrame`.
- `duckdb`(`duckdb.md`): `DataScan.to_duckdb(table_name, connection)` registers the scan as a named relation in the supplied or new `DuckDBPyConnection` and returns that connection.
- `daft`(`daft.md`): `Table.to_daft()` opens the whole table as a lazy Daft dataframe.
- `pandas`(`pandas.md`): `TableScan.to_pandas()` materializes the filtered scan as a boundary `pd.DataFrame`.
- within-lib: the data folder's lakehouse rail crosses Iceberg on the `LakeOp` axis; `load_catalog` is the sole catalog entry, `Transaction` the sole write unit, and `StaticTable.from_metadata` opens a read-only `Table` without a live catalog.

[LOCAL_ADMISSION]:
- construct through `load_catalog`; batch writes through `Table.transaction()`, one-op writes through the autocommit shorthands.
- filter through `DataScan.filter`/`.select` chains built from `pyiceberg.expressions` or `parser.parse`; inspect through `InspectTable`, never raw manifest or metadata-file access.
- egress through `to_arrow`/`to_arrow_batch_reader` for the Arrow bridge, `Table.to_polars()`/`pl.scan_iceberg` for lazy Polars, `to_duckdb` for DuckDB.
- evolve schema through the `UpdateSchema` builder — `delete_column` the portable drop, `rename_column` the portable rename — and build column types via `IcebergType.model_validate` on the `(name, type-string)` path.

[RAIL_LAW]:
- Package: `pyiceberg`
- Owns: Iceberg catalog DDL, table scan and write (append/overwrite/delete/upsert/dynamic-partition-overwrite/add-files), schema/type/partition/sort-order construction, expression building, property/location/statistics evolution, snapshot/branch/tag lifecycle, snapshot expiry, and multi-engine tabular egress
- Accept: `load_catalog` polymorphic construction, `Transaction` batched writes, `DataScan` filter/select/egress chains, `Table.to_polars()`/`pl.scan_iceberg` lazy egress, `Schema.as_arrow()`/`pa.Schema` for the Arrow bridge, `pa.Table`/`pa.RecordBatchReader` on the write side
- Reject: `load_rest`/`load_hive`/`load_sql` call sites, hand-rolled filter predicates, duplicate per-engine scan entrypoints, eager `to_polars` where lazy `Table.to_polars()`/`pl.scan_iceberg` composes into the graph, and raw manifest or metadata access outside `InspectTable`
