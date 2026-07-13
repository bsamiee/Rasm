# [PY_DATA_API_PYICEBERG]

`pyiceberg` supplies catalog, table, schema, partitioning, transform, expression, and scan surfaces for Apache Iceberg table operations. `Catalog`, `Table`, `Transaction`, `TableScan`, `DataScan`, and `Schema` are the primary owners; `load_catalog` is the single polymorphic catalog entry, all writes flow through `Transaction`, and scans expose Arrow, Pandas, Polars, DuckDB, Ray, and batch-reader egress at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyiceberg`
- package: `pyiceberg`
- import: `import pyiceberg`
- owner: `data`
- rail: iceberg
- version: `0.11.1`
- license: `Apache-2.0`
- capability: catalog DDL, table reads/writes, schema/type/partition construction, expression-based scan filtering, and multi-engine tabular egress (Arrow / Pandas / Polars eager + lazy / DuckDB / Ray / Daft / Bodo)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: catalog, table, and scan owners
- rail: iceberg

| [INDEX] | [SYMBOL]                                          | [ROLE]               | [ENTRY]                                           |
| :-----: | :------------------------------------------------ | :------------------- | :------------------------------------------------ |
|  [01]   | `pyiceberg.catalog.Catalog`                       | abstract catalog     | `load_catalog(name, **props)`                     |
|  [02]   | `pyiceberg.catalog.MetastoreCatalog`              | metastore catalog    | same surface as `Catalog`                         |
|  [03]   | `pyiceberg.catalog.CatalogType`                   | catalog enum         | `REST`, `HIVE`, `SQL`, `GLUE`                     |
|  [04]   | `pyiceberg.catalog.CatalogType`                   | catalog enum         | `DYNAMODB`, `BIGQUERY`, `IN_MEMORY`               |
|  [05]   | `pyiceberg.table.Table`                           | live managed table   | scan, write, snapshot, transaction entry          |
|  [06]   | `pyiceberg.table.StaticTable`                     | read-only table      | `StaticTable.from_metadata(path, io_props)`       |
|  [07]   | `pyiceberg.table.Transaction`                     | multi-op write unit  | `table.transaction()`                             |
|  [08]   | `pyiceberg.table.TableScan`                       | base scan            | Arrow/Pandas/Polars egress                        |
|  [09]   | `pyiceberg.table.DataScan`                        | data-file scan       | DuckDB/Ray/batch-reader egress                    |
|  [10]   | `pyiceberg.table.InspectTable`                    | metadata inspection  | `table.inspect`                                   |
|  [11]   | `pyiceberg.table.ManageSnapshots`                 | snapshot lifecycle   | `table.manage_snapshots()`                        |
|  [12]   | `pyiceberg.schema.Schema`                         | Iceberg schema       | holds `NestedField` columns, `as_arrow()`         |
|  [13]   | `pyiceberg.schema.NestedField`                    | column descriptor    | field id, name, type, required, doc               |
|  [14]   | `pyiceberg.table.maintenance.MaintenanceTable`    | maintenance owner    | `table.maintenance`; `expire_snapshots()` builder |
|  [15]   | `pyiceberg.table.update.snapshot.ExpireSnapshots` | expire builder       | autocommit; `older_than`/`by_id`/`commit`         |
|  [16]   | `pyiceberg.table.update.schema.UpdateSchema`      | schema-mutation unit | chained mutators, `commit` on `with`-exit         |

[PUBLIC_TYPE_SCOPE]: schema types, transforms, and expressions
- rail: iceberg

| [INDEX] | [FAMILY]           | [MEMBERS]                                                                                   |
| :-----: | :----------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | primitive types    | `BooleanType`, `IntegerType`, `LongType`, `FloatType`, `DoubleType`                         |
|  [02]   | primitive types    | `DecimalType`, `StringType`, `BinaryType`, `UUIDType`                                       |
|  [03]   | temporal types     | `DateType`, `TimeType`, `TimestampType`                                                     |
|  [04]   | temporal types     | `TimestamptzType`, `TimestampNanoType`, `TimestamptzNanoType`                               |
|  [05]   | container types    | `ListType`, `MapType`, `StructType`, `FixedType`, `UnknownType`                             |
|  [06]   | partitioning       | `PartitionSpec`, `PartitionField`, `PartitionKey`                                           |
|  [07]   | transforms         | `IdentityTransform`, `BucketTransform`, `TruncateTransform`, `VoidTransform`                |
|  [08]   | transforms         | `YearTransform`, `MonthTransform`, `DayTransform`, `HourTransform`                          |
|  [09]   | unbound predicates | `EqualTo`, `NotEqualTo`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual` |
|  [10]   | unbound predicates | `In`, `NotIn`, `IsNull`, `NotNull`, `IsNaN`, `NotNaN`, `StartsWith`, `NotStartsWith`        |
|  [11]   | combinators        | `And`, `Or`, `Not`, `AlwaysTrue`, `AlwaysFalse`                                             |

[PUBLIC_TYPE_SCOPE]: exception families
- rail: iceberg

| [INDEX] | [SYMBOL]                                                                                     | [CAUSE]                       |
| :-----: | :------------------------------------------------------------------------------------------- | :---------------------------- |
|  [01]   | `NoSuchTableError`, `NoSuchNamespaceError`, `NoSuchViewError`                                | identifier not found          |
|  [02]   | `TableAlreadyExistsError`, `NamespaceAlreadyExistsError`                                     | duplicate create              |
|  [03]   | `CommitFailedException`, `CommitStateUnknownException`                                       | write-conflict, unknown state |
|  [04]   | `ValidationError`, `ValidationException`                                                     | schema or spec validation     |
|  [05]   | `ResolveError`                                                                               | expression binding failure    |
|  [06]   | `RESTError`, `ServerError`, `ServiceUnavailableError`, `UnauthorizedError`, `ForbiddenError` | REST transport                |
|  [07]   | `NotInstalledError`                                                                          | optional extra not installed  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: catalog and DDL
- rail: iceberg
- backend-specific loaders (`load_rest`, `load_hive`, `load_sql`, `load_glue`, `load_in_memory`) exist; `load_catalog` is the polymorphic entry
- call: `Catalog.create_table(identifier, schema, location=None, partition_spec=..., sort_order=..., properties={})`; `Catalog.create_table_transaction(identifier, schema, ...)`; `Catalog.create_table_if_not_exists(...)`; `Catalog.create_namespace_if_not_exists(namespace, properties={})`; `Catalog.update_namespace_properties(namespace, removals, updates)`

| [INDEX] | [SURFACE]                                                                | [FAMILY]       | [RETURNS]                                |
| :-----: | :----------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `load_catalog(name=None, **properties)`                                  | construction   | `Catalog`                                |
|  [02]   | `list_catalogs()`                                                        | construction   | `list[str]`                              |
|  [03]   | `Catalog.create_namespace(namespace, properties={})`                     | namespace DDL  | `None`                                   |
|  [04]   | `Catalog.list_namespaces(namespace=())` / `list_tables(namespace)`       | namespace DDL  | identifiers                              |
|  [05]   | `Catalog.create_table(...)`                                              | table DDL      | `Table`                                  |
|  [06]   | `Catalog.load_table` / `drop_table` / `purge_table`                      | table DDL      | `Table` or `None`                        |
|  [07]   | `Catalog.rename_table` / `register_table`                                | table DDL      | `Table`                                  |
|  [08]   | `Catalog.table_exists` / `namespace_exists` / `view_exists`              | existence      | `bool`                                   |
|  [09]   | `Catalog.create_table_transaction(...)`                                  | table DDL      | `CreateTableTransaction`                 |
|  [10]   | `Catalog.create_table_if_not_exists` / `create_namespace_if_not_exists`  | idempotent DDL | `Table` / `None`                         |
|  [11]   | `Catalog.update_namespace_properties(...)` / `load_namespace_properties` | namespace DDL  | `PropertiesUpdateSummary` / `Properties` |
|  [12]   | `Catalog.list_views(namespace)` / `drop_view(identifier)`                | view DDL       | identifiers / `None`                     |
|  [13]   | `Catalog.commit_table` / `supports_server_side_planning`                 | commit/plan    | `CommitTableResponse` / `bool`           |

[ENTRYPOINT_SCOPE]: scan and egress
- rail: iceberg
- `Table.scan(...)` returns `DataScan`; scan construction is lazy until a materialiser runs
- call: `Table.scan(row_filter: str | BooleanExpression = AlwaysTrue(), selected_fields=('*',), case_sensitive=True, snapshot_id=None, options={}, limit=None) -> DataScan`
- call: the `TableScan` chain — `.filter(expr)` / `.select(*field_names)` / `.projection()` / `.use_ref(name)` / `.with_case_sensitive(bool)` / `.update(**kw)`, each returning the scan

| [INDEX] | [SURFACE]                                                           | [FAMILY]   | [RETURNS]                                       |
| :-----: | :------------------------------------------------------------------ | :--------- | :---------------------------------------------- |
|  [01]   | `Table.scan(...)`                                                   | scan       | `DataScan`                                      |
|  [02]   | `TableScan` chain (see `- call:`)                                   | scan chain | scan                                            |
|  [03]   | `TableScan.to_arrow()`                                              | egress     | `pa.Table`                                      |
|  [04]   | `TableScan.to_pandas(**kwargs)`                                     | egress     | `pd.DataFrame`                                  |
|  [05]   | `TableScan.to_polars()`                                             | egress     | `pl.DataFrame` (eager)                          |
|  [06]   | `DataScan.to_arrow_batch_reader()`                                  | egress     | `pa.RecordBatchReader`                          |
|  [07]   | `DataScan.to_duckdb(table_name, connection=None)`                   | egress     | `DuckDBPyConnection`                            |
|  [08]   | `DataScan.to_ray()`                                                 | egress     | `ray.data.Dataset`                              |
|  [09]   | `TableScan.count()` / `plan_files()` / `DataScan.partition_filters` | scan       | `int` / `ScanTask` iterable                     |
|  [10]   | `pyiceberg.expressions.parser.parse(expr)`                          | scan       | `BooleanExpression`                             |
|  [11]   | `Table.to_polars()` / `to_daft()` / `to_bodo()`                     | egress     | `pl.LazyFrame` / Daft / Bodo (whole-table lazy) |

[ENTRYPOINT_SCOPE]: write, evolution, and inspection
- rail: iceberg
- direct `Table.append`/`overwrite`/`delete` are single-operation shorthands over `Transaction`
- call: `Transaction.append(df, snapshot_properties={}, branch='main')`; `Transaction.overwrite(df, overwrite_filter=AlwaysTrue(), ...)`; `Transaction.delete(delete_filter, snapshot_properties={}, ...)`; `Transaction.upsert(df, join_cols=None, when_matched_update_all=True, when_not_matched_insert_all=True, case_sensitive=True, branch='main', ...)`; `Transaction.dynamic_partition_overwrite(df, snapshot_properties={}, branch='main')`; `Transaction.add_files(file_paths, check_duplicate_files=True, branch='main')`
- call: `Transaction.update_schema(allow_incompatible_changes=False, case_sensitive=True) -> UpdateSchema`
- call: on the `UpdateSchema` builder, `.add_column(path, field_type, doc=None, required=False, default_value=None)` / `.delete_column(path)` / `.rename_column(path_from, new_name)` / `.update_column(...)` chain and commit on `with`-exit

| [INDEX] | [SURFACE]                                                                | [FAMILY]    | [RETURNS]                              |
| :-----: | :----------------------------------------------------------------------- | :---------- | :------------------------------------- |
|  [01]   | `Transaction.append`                                                     | write       | `None`                                 |
|  [02]   | `Transaction.overwrite`                                                  | write       | `None`                                 |
|  [03]   | `Transaction.delete`                                                     | write       | `None`                                 |
|  [04]   | `Transaction.upsert`                                                     | write       | `UpsertResult`                         |
|  [05]   | `Transaction.dynamic_partition_overwrite`                                | write       | `None`                                 |
|  [06]   | `Transaction.add_files`                                                  | write       | `None`                                 |
|  [07]   | `Transaction.commit_transaction()`                                       | write       | `Table`                                |
|  [08]   | `Transaction.update_schema(...)`                                         | evolution   | `UpdateSchema` (args in `- call:`)     |
|  [09]   | `Transaction.update_spec()`                                              | evolution   | `UpdateSpec`                           |
|  [10]   | `Transaction.update_snapshot(branch='main')`                             | evolution   | `UpdateSnapshot`                       |
|  [11]   | `Transaction.update_sort_order()`                                        | evolution   | `UpdateSortOrder`                      |
|  [12]   | `Transaction.set_properties` / `.remove_properties` / `.update_location` | evolution   | `Transaction` / builder                |
|  [13]   | `Transaction.update_statistics` / `.upgrade_table_version`               | evolution   | `Transaction` / builder                |
|  [14]   | `ManageSnapshots.rollback_to_snapshot` / `.create_branch`                | snapshot    | branch creation                        |
|  [15]   | `ManageSnapshots.create_tag` / `.commit`                                 | snapshot    | tag / commit                           |
|  [16]   | `InspectTable.snapshots` / `.manifests` / `.files`                       | inspection  | metadata table                         |
|  [17]   | `InspectTable.entries` / `.partitions` / `.history`                      | inspection  | metadata table                         |
|  [18]   | `Table.current_snapshot` / `.snapshot_by_id`                             | snapshot    | `Snapshot`                             |
|  [19]   | `Table.snapshot_by_name` / `.snapshot_as_of_timestamp`                   | snapshot    | `Snapshot`                             |
|  [20]   | `Table.snapshots` / `.refs` / `.history`                                 | snapshot    | `Snapshot` / refs / history            |
|  [21]   | `Table.schema` / `.schemas` / `.spec` / `.specs` / `.sort_order`         | metadata    | live table metadata accessors          |
|  [22]   | `Table.properties` / `.name_mapping` / `.format_version`                 | metadata    | live table metadata accessors          |
|  [23]   | `Table.location` / `.location_provider`                                  | metadata    | live table metadata accessors          |
|  [24]   | `Table.update_schema(...)`                                               | evolution   | `UpdateSchema` (mutators in `- call:`) |
|  [25]   | `Table.maintenance` / `MaintenanceTable.expire_snapshots`                | maintenance | `MaintenanceTable` / `ExpireSnapshots` |
|  [26]   | `ExpireSnapshots.older_than` / `.by_id` / `.by_ids` / `.commit`          | maintenance | `ExpireSnapshots` / commit             |

## [04]-[IMPLEMENTATION_LAW]

[CATALOG_TOPOLOGY]:
- `load_catalog` is the single polymorphic entry; catalog type is discriminated by `uri` scheme or explicit `type` property, not by `load_rest`/`load_hive` call-site branching.
- Scan construction is lazy; no I/O until `to_arrow()`, `to_pandas()`, `plan_files()`, or `count()` materialises the result.
- All writes flow through `Transaction`; direct `Table.append(df)`/`overwrite`/`delete`/`upsert`/`dynamic_partition_overwrite`/`add_files` are autocommit shorthands that open and commit a single-operation transaction, while `Table.transaction()` batches many ops into one snapshot. `upsert` requires `join_cols` (or an identifier-field schema); `dynamic_partition_overwrite` replaces only the partitions present in `df`.
- Expression predicates are unbound at construction; binding to a schema happens inside the scan engine, not at predicate construction time. `Table.scan` default `row_filter` is `AlwaysTrue()`.

[ENGINE_BRIDGE]:
- Egress is two-grained: `TableScan.to_arrow()`/`to_pandas()`/`to_polars()` materialize a FILTERED scan EAGERLY, while `Table.to_polars()` returns a `pl.LazyFrame` over the WHOLE table with no explicit scan — the lazy form is the one to compose into a Polars `LazyFrame` graph so PyIceberg's planner and Polars' optimizer both push predicates. `DataScan.to_arrow_batch_reader()` yields a `pa.RecordBatchReader` for out-of-core streaming into pyarrow/Polars.
- The Arrow `pa.Table`/`pa.RecordBatchReader` produced by egress is the same shape `Transaction.append`/`overwrite`/`add_files`/`dynamic_partition_overwrite` accept on the write side; a read-transform-write loop never leaves Arrow memory.
- `pl.scan_iceberg(table_obj, use_pyiceberg_filter=True)` (Polars side) accepts the live `Table` and delegates filter planning to PyIceberg — the inverse of `Table.to_polars()` and the preferred entry when the pipeline root is a Polars `LazyFrame`.

[SCHEMA_BRIDGE]:
- Schema types use the `pyiceberg.types.*` primitives; Arrow schemas convert via `Schema.as_arrow()` and are accepted by `create_table` and `create_table_transaction`. `create_table` also accepts a `pa.Schema` directly.
- `DataScan.to_duckdb` registers the result as a named relation in the supplied or new `DuckDBPyConnection` and returns that connection.
- `StaticTable.from_metadata` builds a read-only `Table` from a metadata path without a live catalog.
- `Transaction.update_schema()` yields an `UpdateSchema` builder whose `add_column`/`delete_column`/`rename_column`/`update_column` mutators chain and apply on the builder's `__exit__`/`commit`; `delete_column(path)` is the portable column-drop member and `rename_column(path_from, new_name)` the portable rename.
- `add_column(path, field_type, ...)` takes an `IcebergType`; `pyiceberg.types.IcebergType` is a Pydantic model whose wrap validator parses a primitive type string, so `IcebergType.model_validate("string")` yields `StringType()` — the settled string-to-type bridge for a `(name, type-string)` column-add tuple.

[MAINTENANCE_BRIDGE]:
- `Table.maintenance` returns a `MaintenanceTable`; `expire_snapshots()` opens an autocommitting `Transaction` and returns an `ExpireSnapshots` builder whose `older_than(dt)`/`by_id(snapshot_id)`/`by_ids(snapshot_ids)` clauses chain and apply on `commit()` (or the builder's `__exit__`).
- `expire_snapshots` is the only maintenance op the Python `pyiceberg` API exposes: data-file compaction (`rewrite_data_files`) and orphan-file removal (`remove_orphan_files`) have no public Python entry — `ObjectStoreLocationProvider` only references orphan removal in a layout comment, not as an API.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyiceberg`
- Owns: Iceberg catalog DDL, table scan and write (append/overwrite/delete/upsert/dynamic-partition-overwrite/add-files), schema/type/partition/sort-order construction, expression building, property/location/statistics evolution, and multi-engine tabular egress (Arrow/Pandas/Polars eager+lazy/DuckDB/Ray/Daft/Bodo)
- Accept: `load_catalog` polymorphic construction, `Transaction` for batched writes, `DataScan` filter/select/egress chains, `Table.to_polars()` lazy egress, `Schema.as_arrow()`/`pa.Schema` for the Arrow bridge, and `pa.Table`/`pa.RecordBatchReader` on the write side
- Reject: direct `load_rest`/`load_hive`/`load_sql` at call sites, hand-rolled filter predicates, duplicate per-engine scan entry points, eager `to_polars` where a lazy `Table.to_polars()`/`pl.scan_iceberg` composes into the query graph, and raw manifest or metadata-file access outside `InspectTable`
