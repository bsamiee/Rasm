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

| [INDEX] | [SYMBOL]                                          | [ROLE]               | [ENTRY]                                                               |
| :-----: | :------------------------------------------------ | :------------------- | :-------------------------------------------------------------------- |
|  [01]   | `pyiceberg.catalog.Catalog`                       | abstract catalog     | `load_catalog(name, **props)`                                         |
|  [02]   | `pyiceberg.catalog.MetastoreCatalog`              | metastore catalog    | same surface as `Catalog`                                             |
|  [03]   | `pyiceberg.catalog.CatalogType`                   | catalog enum         | `REST`, `HIVE`, `SQL`, `GLUE`, `DYNAMODB`, `BIGQUERY`, `IN_MEMORY`    |
|  [04]   | `pyiceberg.table.Table`                           | live managed table   | scan, write, snapshot, transaction entry                              |
|  [05]   | `pyiceberg.table.StaticTable`                     | read-only table      | `StaticTable.from_metadata(path, io_props)`                           |
|  [06]   | `pyiceberg.table.Transaction`                     | multi-op write unit  | `table.transaction()`                                                 |
|  [07]   | `pyiceberg.table.TableScan`                       | base scan            | Arrow/Pandas/Polars egress                                            |
|  [08]   | `pyiceberg.table.DataScan`                        | data-file scan       | DuckDB/Ray/batch-reader egress                                        |
|  [09]   | `pyiceberg.table.InspectTable`                    | metadata inspection  | `table.inspect`                                                       |
|  [10]   | `pyiceberg.table.ManageSnapshots`                 | snapshot lifecycle   | `table.manage_snapshots()`                                            |
|  [11]   | `pyiceberg.schema.Schema`                         | Iceberg schema       | holds `NestedField` columns, `as_arrow()`                             |
|  [12]   | `pyiceberg.schema.NestedField`                    | column descriptor    | field id, name, type, required, doc                                   |
|  [13]   | `pyiceberg.table.maintenance.MaintenanceTable`    | maintenance owner    | `table.maintenance`; `expire_snapshots()` builder                     |
|  [14]   | `pyiceberg.table.update.snapshot.ExpireSnapshots` | expire builder       | autocommit `Transaction` context; `older_than`/`by_id`/`commit`       |
|  [15]   | `pyiceberg.table.update.schema.UpdateSchema`      | schema-mutation unit | `add_column`/`delete_column`/`rename_column`/`update_column`/`commit` |

[PUBLIC_TYPE_SCOPE]: schema types, transforms, and expressions
- rail: iceberg

| [INDEX] | [FAMILY]           | [MEMBERS]                                                                                                                                        |
| :-----: | :----------------- | :----------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | primitive types    | `BooleanType`, `IntegerType`, `LongType`, `FloatType`, `DoubleType`, `DecimalType`, `StringType`, `BinaryType`, `UUIDType`                       |
|  [02]   | temporal types     | `DateType`, `TimeType`, `TimestampType`, `TimestamptzType`, `TimestampNanoType`, `TimestamptzNanoType`                                           |
|  [03]   | container types    | `ListType`, `MapType`, `StructType`, `FixedType`, `UnknownType`                                                                                  |
|  [04]   | partitioning       | `PartitionSpec`, `PartitionField`, `PartitionKey`                                                                                                |
|  [05]   | transforms         | `IdentityTransform`, `BucketTransform`, `TruncateTransform`, `YearTransform`, `MonthTransform`, `DayTransform`, `HourTransform`, `VoidTransform` |
|  [06]   | unbound predicates | `EqualTo`, `NotEqualTo`, `LessThan`, `LessThanOrEqual`, `GreaterThan`, `GreaterThanOrEqual`, `In`, `NotIn`                                       |
|  [07]   | unbound predicates | `IsNull`, `NotNull`, `IsNaN`, `NotNaN`, `StartsWith`, `NotStartsWith`                                                                            |
|  [08]   | combinators        | `And`, `Or`, `Not`, `AlwaysTrue`, `AlwaysFalse`                                                                                                  |

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

| [INDEX] | [SURFACE]                                                                                                           | [FAMILY]       | [RETURNS]                                |
| :-----: | :------------------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------- |
|  [01]   | `load_catalog(name=None, **properties)`                                                                             | construction   | `Catalog`                                |
|  [02]   | `list_catalogs()`                                                                                                   | construction   | `list[str]`                              |
|  [03]   | `Catalog.create_namespace(namespace, properties={})`                                                                | namespace DDL  | `None`                                   |
|  [04]   | `Catalog.list_namespaces(namespace=())`, `Catalog.list_tables(namespace)`                                           | namespace DDL  | identifiers                              |
|  [05]   | `Catalog.create_table(identifier, schema, location=None, partition_spec=..., sort_order=..., properties={})`        | table DDL      | `Table`                                  |
|  [06]   | `Catalog.load_table(identifier)`, `Catalog.drop_table(identifier)`, `Catalog.purge_table(identifier)`               | table DDL      | `Table` or `None`                        |
|  [07]   | `Catalog.rename_table(from_identifier, to_identifier)`, `Catalog.register_table(identifier, metadata_location)`     | table DDL      | `Table`                                  |
|  [08]   | `Catalog.table_exists(identifier)`, `Catalog.namespace_exists(namespace)`, `Catalog.view_exists(identifier)`        | existence      | `bool`                                   |
|  [09]   | `Catalog.create_table_transaction(identifier, schema, ...)`                                                         | table DDL      | `CreateTableTransaction`                 |
|  [10]   | `Catalog.create_table_if_not_exists(...)`, `Catalog.create_namespace_if_not_exists(namespace, properties={})`       | idempotent DDL | `Table` / `None`                         |
|  [11]   | `Catalog.update_namespace_properties(namespace, removals, updates)`, `Catalog.load_namespace_properties(namespace)` | namespace DDL  | `PropertiesUpdateSummary` / `Properties` |
|  [12]   | `Catalog.list_views(namespace)`, `Catalog.drop_view(identifier)`                                                    | view DDL       | identifiers / `None`                     |
|  [13]   | `Catalog.commit_table(table, requirements, updates)`, `Catalog.supports_server_side_planning`                       | commit/plan    | `CommitTableResponse` / `bool`           |

[ENTRYPOINT_SCOPE]: scan and egress
- rail: iceberg
- `Table.scan(...)` returns `DataScan`; scan construction is lazy until a materialiser runs

| [INDEX] | [SURFACE]                                                                                                                                                | [FAMILY]   | [RETURNS]                                                         |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------- | :---------------------------------------------------------------- |
|  [01]   | `Table.scan(row_filter: str \| BooleanExpression = AlwaysTrue(), selected_fields=('*',), case_sensitive=True, snapshot_id=None, options={}, limit=None)` | scan       | `DataScan`                                                        |
|  [02]   | `TableScan.filter(expr)`, `.select(*field_names)`, `.projection()`, `.use_ref(name)`, `.with_case_sensitive(bool)`, `.update(**kw)`                      | scan chain | scan                                                              |
|  [03]   | `TableScan.to_arrow()`                                                                                                                                   | egress     | `pa.Table`                                                        |
|  [04]   | `TableScan.to_pandas(**kwargs)`                                                                                                                          | egress     | `pd.DataFrame`                                                    |
|  [05]   | `TableScan.to_polars()`                                                                                                                                  | egress     | `pl.DataFrame` (eager)                                            |
|  [06]   | `DataScan.to_arrow_batch_reader()`                                                                                                                       | egress     | `pa.RecordBatchReader`                                            |
|  [07]   | `DataScan.to_duckdb(table_name, connection=None)`                                                                                                        | egress     | `DuckDBPyConnection`                                              |
|  [08]   | `DataScan.to_ray()`                                                                                                                                      | egress     | `ray.data.Dataset`                                                |
|  [09]   | `TableScan.count()`, `TableScan.plan_files()`, `DataScan.partition_filters`                                                                              | scan       | `int` / `ScanTask` iterable                                       |
|  [10]   | `pyiceberg.expressions.parser.parse(expr)`; `scan(row_filter=)` parses a `str` filter internally                                                         | scan       | `BooleanExpression`                                               |
|  [11]   | `Table.to_polars()`, `Table.to_daft()`, `Table.to_bodo()`                                                                                                | egress     | `pl.LazyFrame` / Daft / Bodo (whole-table lazy, no explicit scan) |

[ENTRYPOINT_SCOPE]: write, evolution, and inspection
- rail: iceberg
- direct `Table.append`/`overwrite`/`delete` are single-operation shorthands over `Transaction`

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                                          | [FAMILY]    | [RETURNS]                                           |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------- | :-------------------------------------------------- |
|  [01]   | `Transaction.append(df, snapshot_properties={}, branch='main')`                                                                                                                                                                                                    | write       | `None`                                              |
|  [02]   | `Transaction.overwrite(df, overwrite_filter=AlwaysTrue(), ...)`                                                                                                                                                                                                    | write       | `None`                                              |
|  [03]   | `Transaction.delete(delete_filter, snapshot_properties={}, ...)`                                                                                                                                                                                                   | write       | `None`                                              |
|  [04]   | `Transaction.upsert(df, join_cols=None, when_matched_update_all=True, when_not_matched_insert_all=True, case_sensitive=True, branch='main', ...)`                                                                                                                  | write       | `UpsertResult`                                      |
|  [05]   | `Transaction.dynamic_partition_overwrite(df, snapshot_properties={}, branch='main')`                                                                                                                                                                               | write       | `None`                                              |
|  [06]   | `Transaction.add_files(file_paths, check_duplicate_files=True, branch='main')`                                                                                                                                                                                     | write       | `None`                                              |
|  [07]   | `Transaction.commit_transaction()`                                                                                                                                                                                                                                 | write       | `Table`                                             |
|  [08]   | `Transaction.update_schema(allow_incompatible_changes=False, case_sensitive=True)`                                                                                                                                                                                 | evolution   | `UpdateSchema`                                      |
|  [09]   | `Transaction.update_spec()`, `.update_snapshot(branch='main')`, `.update_sort_order()`                                                                                                                                                                             | evolution   | `UpdateSpec` / `UpdateSnapshot` / `UpdateSortOrder` |
|  [10]   | `Transaction.set_properties(**kw)`, `.remove_properties(*keys)`, `.update_location(loc)`, `.update_statistics()`, `.upgrade_table_version(fmt)`                                                                                                                    | evolution   | `Transaction` / builder                             |
|  [11]   | `ManageSnapshots.rollback_to_snapshot(snapshot_id)`, `.create_branch(name, snapshot_id)`, `.create_tag(name, snapshot_id)`, `.commit()`                                                                                                                            | snapshot    | branch/tag/commit                                   |
|  [12]   | `InspectTable.snapshots()`, `.manifests()`, `.files()`, `.entries()`, `.partitions()`, `.history()`                                                                                                                                                                | inspection  | metadata table                                      |
|  [13]   | `Table.current_snapshot()`, `.snapshot_by_id(id)`, `.snapshot_by_name(name)`, `.snapshot_as_of_timestamp(ts)`, `.snapshots()`, `.refs()`, `.history()`                                                                                                             | snapshot    | `Snapshot` / refs / history                         |
|  [14]   | `Table.schema()`, `.schemas()`, `.spec()`, `.specs()`, `.sort_order()`, `.properties`, `.name_mapping()`, `.format_version`, `.location()`, `.location_provider()`                                                                                                 | metadata    | live table metadata accessors                       |
|  [15]   | `Table.update_schema(...)` (autocommit) yields `UpdateSchema`; `.add_column(path, field_type, doc=None, required=False, default_value=None)`, `.delete_column(path)`, `.rename_column(path_from, new_name)`, `.update_column(...)` chain and commit on `with`-exit | evolution   | `UpdateSchema`                                      |
|  [16]   | `Table.maintenance -> MaintenanceTable`, `MaintenanceTable.expire_snapshots() -> ExpireSnapshots`                                                                                                                                                                  | maintenance | `MaintenanceTable`/`ExpireSnapshots`                |
|  [17]   | `ExpireSnapshots.older_than(dt)`, `.by_id(snapshot_id)`, `.by_ids(snapshot_ids)`, `.commit()`                                                                                                                                                                      | maintenance | `ExpireSnapshots`/commit                            |

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
