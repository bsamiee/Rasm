# [PY_DATA_API_PYICEBERG]

`pyiceberg` supplies catalog, table, schema, partitioning, transform, expression, and scan surfaces for Apache Iceberg table operations. `Catalog`, `Table`, `Transaction`, `TableScan`, `DataScan`, and `Schema` are the primary owners; `load_catalog` is the single polymorphic catalog entry, all writes flow through `Transaction`, and scans expose Arrow, Pandas, Polars, DuckDB, Ray, and batch-reader egress at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyiceberg`
- package: `pyiceberg`
- import: `import pyiceberg`
- owner: `data`
- rail: iceberg
- capability: catalog DDL, table reads/writes, schema/type/partition construction, expression-based scan filtering, and multi-engine tabular egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: catalog, table, and scan owners
- rail: iceberg

| [INDEX] | [SYMBOL]                             | [ROLE]              | [ENTRY]                                                            |
| :-----: | :----------------------------------- | :------------------ | :----------------------------------------------------------------- |
|  [01]   | `pyiceberg.catalog.Catalog`          | abstract catalog    | `load_catalog(name, **props)`                                      |
|  [02]   | `pyiceberg.catalog.MetastoreCatalog` | metastore catalog   | same surface as `Catalog`                                          |
|  [03]   | `pyiceberg.catalog.CatalogType`      | catalog enum        | `REST`, `HIVE`, `SQL`, `GLUE`, `DYNAMODB`, `BIGQUERY`, `IN_MEMORY` |
|  [04]   | `pyiceberg.table.Table`              | live managed table  | scan, write, snapshot, transaction entry                           |
|  [05]   | `pyiceberg.table.StaticTable`        | read-only table     | `StaticTable.from_metadata(path, io_props)`                        |
|  [06]   | `pyiceberg.table.Transaction`        | multi-op write unit | `table.transaction()`                                              |
|  [07]   | `pyiceberg.table.TableScan`          | base scan           | Arrow/Pandas/Polars egress                                         |
|  [08]   | `pyiceberg.table.DataScan`           | data-file scan      | DuckDB/Ray/batch-reader egress                                     |
|  [09]   | `pyiceberg.table.InspectTable`       | metadata inspection | `table.inspect`                                                    |
|  [10]   | `pyiceberg.table.ManageSnapshots`    | snapshot lifecycle  | `table.manage_snapshots()`                                         |
|  [11]   | `pyiceberg.schema.Schema`            | Iceberg schema      | holds `NestedField` columns, `as_arrow()`                          |
|  [12]   | `pyiceberg.schema.NestedField`       | column descriptor   | field id, name, type, required, doc                                |

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

| [INDEX] | [SURFACE]                                                                                                       | [FAMILY]      | [RETURNS]                |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :------------ | :----------------------- |
|  [01]   | `load_catalog(name=None, **properties)`                                                                         | construction  | `Catalog`                |
|  [02]   | `list_catalogs()`                                                                                               | construction  | `list[str]`              |
|  [03]   | `Catalog.create_namespace(namespace, properties={})`                                                            | namespace DDL | `None`                   |
|  [04]   | `Catalog.list_namespaces(namespace=())`, `Catalog.list_tables(namespace)`                                       | namespace DDL | identifiers              |
|  [05]   | `Catalog.create_table(identifier, schema, location=None, partition_spec=..., sort_order=..., properties={})`    | table DDL     | `Table`                  |
|  [06]   | `Catalog.load_table(identifier)`, `Catalog.drop_table(identifier)`, `Catalog.purge_table(identifier)`           | table DDL     | `Table` or `None`        |
|  [07]   | `Catalog.rename_table(from_identifier, to_identifier)`, `Catalog.register_table(identifier, metadata_location)` | table DDL     | `Table`                  |
|  [08]   | `Catalog.table_exists(identifier)`                                                                              | table DDL     | `bool`                   |
|  [09]   | `Catalog.create_table_transaction(identifier, schema, ...)`                                                     | table DDL     | `CreateTableTransaction` |

[ENTRYPOINT_SCOPE]: scan and egress
- rail: iceberg
- `Table.scan(...)` returns `DataScan`; scan construction is lazy until a materialiser runs

| [INDEX] | [SURFACE]                                                                                                        | [FAMILY]   | [RETURNS]                    |
| :-----: | :--------------------------------------------------------------------------------------------------------------- | :--------- | :--------------------------- |
|  [01]   | `Table.scan(row_filter=AlwaysTrue(), selected_fields=('*',), case_sensitive=True, snapshot_id=None, limit=None)` | scan       | `DataScan`                   |
|  [02]   | `TableScan.filter(expr)`, `TableScan.select(*field_names)`, `TableScan.use_ref(name)`                            | scan chain | scan                         |
|  [03]   | `TableScan.to_arrow()`                                                                                           | egress     | `pa.Table`                   |
|  [04]   | `TableScan.to_pandas(**kwargs)`                                                                                  | egress     | `pd.DataFrame`               |
|  [05]   | `TableScan.to_polars()`                                                                                          | egress     | `pl.DataFrame`               |
|  [06]   | `DataScan.to_arrow_batch_reader()`                                                                               | egress     | `pa.RecordBatchReader`       |
|  [07]   | `DataScan.to_duckdb(table_name, connection=None)`                                                                | egress     | `DuckDBPyConnection`         |
|  [08]   | `DataScan.to_ray()`                                                                                              | egress     | `ray.data.Dataset`           |
|  [09]   | `TableScan.count()`, `TableScan.plan_files()`                                                                    | scan       | `int` or `ScanTask` iterable |

[ENTRYPOINT_SCOPE]: write, evolution, and inspection
- rail: iceberg
- direct `Table.append`/`overwrite`/`delete` are single-operation shorthands over `Transaction`

| [INDEX] | [SURFACE]                                                                                                                               | [FAMILY]   | [RETURNS]                        |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------- | :--------- | :------------------------------- |
|  [01]   | `Transaction.append(df, snapshot_properties={}, branch='main')`                                                                         | write      | `None`                           |
|  [02]   | `Transaction.overwrite(df, overwrite_filter=AlwaysTrue(), ...)`                                                                         | write      | `None`                           |
|  [03]   | `Transaction.delete(delete_filter, snapshot_properties={}, ...)`                                                                        | write      | `None`                           |
|  [04]   | `Transaction.upsert(df, join_cols=None, when_matched_update_all=True, ...)`                                                             | write      | `UpsertResult`                   |
|  [05]   | `Transaction.add_files(file_paths, check_duplicate_files=True, branch='main')`                                                          | write      | `None`                           |
|  [06]   | `Transaction.commit_transaction()`                                                                                                      | write      | `Table`                          |
|  [07]   | `Transaction.update_schema(allow_incompatible_changes=False, case_sensitive=True)`                                                      | evolution  | `UpdateSchema`                   |
|  [08]   | `Transaction.update_spec()`, `Transaction.update_snapshot(branch='main')`                                                               | evolution  | `UpdateSpec` or `UpdateSnapshot` |
|  [09]   | `ManageSnapshots.rollback_to_snapshot(snapshot_id)`, `.create_branch(name, snapshot_id)`, `.create_tag(name, snapshot_id)`, `.commit()` | snapshot   | branch/tag/commit                |
|  [10]   | `InspectTable.snapshots()`, `.manifests()`, `.files()`, `.entries()`, `.partitions()`, `.history()`                                     | inspection | metadata table                   |

## [04]-[IMPLEMENTATION_LAW]

[CATALOG_TOPOLOGY]:
- `load_catalog` is the single polymorphic entry; catalog type is discriminated by `uri` scheme or explicit `type` property, not by `load_rest`/`load_hive` call-site branching.
- Scan construction is lazy; no I/O until `to_arrow()`, `to_pandas()`, `plan_files()`, or `count()` materialises the result.
- All writes flow through `Transaction`; direct `Table.append(df)` is shorthand that opens and commits a single-append transaction.
- Expression predicates are unbound at construction; binding to a schema happens inside the scan engine, not at predicate construction time.

[SCHEMA_BRIDGE]:
- Schema types use the `pyiceberg.types.*` primitives; Arrow schemas convert via `Schema.as_arrow()` and are accepted by `create_table` and `create_table_transaction`.
- `DataScan.to_duckdb` registers the result as a named relation in the supplied or new `DuckDBPyConnection` and returns that connection.
- `StaticTable.from_metadata` builds a read-only `Table` from a metadata path without a live catalog.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyiceberg`
- Owns: Iceberg catalog DDL, table scan and write, schema and type construction, partition and expression building, multi-engine tabular egress
- Accept: `load_catalog` polymorphic construction, `Transaction` for all writes, `DataScan` filter/select/egress chains, `Schema.as_arrow()` for Arrow bridge
- Reject: direct `load_rest`/`load_hive`/`load_sql` at call sites, hand-rolled filter predicates, duplicate per-engine scan entry points, and raw manifest or metadata-file access outside `InspectTable`
