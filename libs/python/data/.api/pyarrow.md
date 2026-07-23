# [PY_DATA_API_PYARROW]

`pyarrow` owns Arrow columnar memory in Python: typed `Array`/`ChunkedArray` columns, `RecordBatch`/`Table` tables, `Field`/`Schema` metadata, and the PyCapsule C-data/C-stream interface (`__arrow_c_array__`/`__arrow_c_stream__`/`__arrow_c_schema__`) that carries every frame zero-copy across the data plane. Constructor and dtype-factory families build these values; the `compute` vectorized kernels with their Python-UDF registry, the `acero` streaming execution engine, `substrait`/`flight` transport, and the columnar-file submodules fold over them as one interchange rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyarrow`
- package: `pyarrow` (Apache-2.0)
- module: `pyarrow`
- namespaces: `compute`, `acero`, `substrait`, `dataset`, `parquet`, `csv`, `json`, `orc`, `feather`, `ipc`, `flight`, `fs`, `cuda`, `interchange`, `types`, `util`
- owner: `data`, `geometry`
- rail: Arrow columnar memory

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tabular and array values

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [CAPABILITY]                           |
| :-----: | :------------------ | :------------- | :------------------------------------- |
|  [01]   | `Table`             | columnar table | chunked column-major table with schema |
|  [02]   | `RecordBatch`       | columnar table | contiguous (single-chunk) batch        |
|  [03]   | `Array`             | column         | contiguous typed column                |
|  [04]   | `ChunkedArray`      | column         | logical column over multiple chunks    |
|  [05]   | `Scalar`            | scalar value   | single typed value                     |
|  [06]   | `Schema`            | schema value   | ordered fields plus metadata           |
|  [07]   | `Field`             | schema field   | named typed nullable field             |
|  [08]   | `DataType`          | dtype          | logical type descriptor                |
|  [09]   | `TableGroupBy`      | grouping       | `aggregate`-driven grouped reduction   |
|  [10]   | `Buffer`            | memory         | contiguous immutable byte buffer       |
|  [11]   | `MemoryPool`        | allocator      | tracked allocation backend             |
|  [12]   | `RecordBatchReader` | stream         | iterator of `RecordBatch`              |

[PUBLIC_TYPE_SCOPE]: nested and extension types

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :----------------------------------- | :------------ | :---------------------------- |
|  [01]   | `StructArray` / `StructType`         | nested        | struct column and type        |
|  [02]   | `ListArray` / `LargeListArray`       | nested        | variable-length list columns  |
|  [03]   | `FixedSizeListArray`                 | nested        | fixed-length list column      |
|  [04]   | `MapArray` / `MapType`               | nested        | key-value map column          |
|  [05]   | `DictionaryArray` / `DictionaryType` | encoded       | dictionary-encoded column     |
|  [06]   | `UnionArray` / `DenseUnionType`      | nested        | tagged union column           |
|  [07]   | `RunEndEncodedArray`                 | encoded       | run-end-encoded column        |
|  [08]   | `ExtensionType` / `ExtensionArray`   | extension     | user-defined logical type     |
|  [09]   | `FixedShapeTensorType`               | extension     | fixed-shape tensor extension  |
|  [10]   | `Tensor` / `SparseCOOTensor`         | tensor        | dense and sparse N-D tensors  |
|  [11]   | `Decimal128Type` / `Decimal256Type`  | decimal       | fixed-precision decimal types |
|  [12]   | `KeyValueMetadata`                   | metadata      | schema and field metadata map |

[PUBLIC_TYPE_SCOPE]: execution-engine and dataset types
- call: `acero.Declaration` carries `from_sequence`/`to_table`/`to_reader`; `dataset.Scanner` takes `columns`/`filter`/`batch_size` and yields `to_table`/`to_batches`; `compute.Expression` composes `field()`/`scalar()`/`&`/`|`/`~`/comparisons; `RecordBatchReader` carries `from_stream`/`from_batches`/`read_all`/`arrow_c_stream`

| [INDEX] | [SYMBOL]                                                      | [TYPE_FAMILY]     | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------ | :---------------- | :------------------------------------ |
|  [01]   | `acero.Declaration`                                           | exec plan         | composable streaming exec-node graph  |
|  [02]   | `acero.ScanNodeOptions`                                       | exec node         | scan node config                      |
|  [03]   | `acero.FilterNodeOptions`                                     | exec node         | filter node config                    |
|  [04]   | `acero.ProjectNodeOptions`                                    | exec node         | project node config                   |
|  [05]   | `acero.AggregateNodeOptions`                                  | exec node         | aggregate node config                 |
|  [06]   | `acero.HashJoinNodeOptions`                                   | exec node         | hash-join node config                 |
|  [07]   | `acero.AsofJoinNodeOptions`                                   | exec node         | as-of-join node config                |
|  [08]   | `acero.OrderByNodeOptions`                                    | exec node         | order-by node config                  |
|  [09]   | `compute.Expression`                                          | predicate algebra | filter/project expression tree        |
|  [10]   | `dataset.Dataset` / `FileSystemDataset` / `UnionDataset`      | dataset           | multi-file lazy dataset with pushdown |
|  [11]   | `dataset.Scanner`                                             | scanner           | configured scan                       |
|  [12]   | `dataset.ParquetFileFormat`                                   | file format       | per-format Parquet scan/write options |
|  [13]   | `dataset.CsvFileFormat`                                       | file format       | CSV scan/write options                |
|  [14]   | `dataset.JsonFileFormat`                                      | file format       | JSON scan/write options               |
|  [15]   | `dataset.OrcFileFormat`                                       | file format       | ORC scan/write options                |
|  [16]   | `dataset.IpcFileFormat`                                       | file format       | Arrow IPC scan/write options          |
|  [17]   | `dataset.HivePartitioning`                                    | partitioning      | Hive-style partition layout           |
|  [18]   | `dataset.DirectoryPartitioning`                               | partitioning      | directory partition layout            |
|  [19]   | `dataset.FilenamePartitioning`                                | partitioning      | filename partition layout             |
|  [20]   | `dataset.ParquetEncryptionConfig` / `ParquetDecryptionConfig` | encryption        | modular Parquet encryption configs    |
|  [21]   | `RecordBatchReader`                                           | stream            | C-stream-importable batch iterator    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and dtype factories
- call: `FixedSizeListArray.from_arrays(values, list_size=None, type=None, mask=None)` and `ListArray.from_arrays(offsets, values, type=None, pool=None, mask=None)` build a fixed-length/variable list column from a flat value buffer

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `array(obj, type, ...)`                                    | static   | build an `Array` from a sequence                          |
|  [02]   | `chunked_array(arrays, type)`                              | static   | build a `ChunkedArray`                                    |
|  [03]   | `record_batch(data, ...)`                                  | static   | build a `RecordBatch`                                     |
|  [04]   | `table(data, schema, ...)`                                 | static   | build a `Table` from dict/arrays                          |
|  [05]   | `scalar(value, type)` / `nulls(size, type)`                | static   | scalar and all-null array                                 |
|  [06]   | `Table.from_pydict / from_pylist / from_arrays`            | factory  | build `Table` from Python/arrays                          |
|  [07]   | `Table.from_pandas(df)` / `from_batches`                   | factory  | build `Table` from pandas/batches                         |
|  [08]   | `schema(fields)` / `field(name, type)`                     | static   | build schema and field                                    |
|  [09]   | `int8/16/32/64` / `uint8/16/32/64`                         | static   | integer dtypes                                            |
|  [10]   | `float16/32/64` / `bool_` / `string` / `binary`            | static   | float, bool, string, binary dtypes                        |
|  [11]   | `timestamp(unit, tz)` / `date32` / `time64`                | static   | temporal dtypes                                           |
|  [12]   | `list_/large_list/struct/map_/dictionary`                  | static   | nested and encoded dtypes                                 |
|  [13]   | `decimal128/decimal256(precision, scale)`                  | static   | decimal dtypes                                            |
|  [14]   | `concat_tables / concat_arrays / concat_batches`           | static   | concatenate Arrow values                                  |
|  [15]   | `unify_schemas(schemas)` / `infer_type(values)`            | static   | merge schemas, infer a dtype                              |
|  [16]   | `FixedSizeListArray.from_arrays` / `ListArray.from_arrays` | factory  | build a fixed/variable list column (see `- call:`)        |
|  [17]   | `Schema.empty_table()` / `Schema.names` / `Schema.types`   | instance | zero-row `Table`; ordered field-name and `DataType` lists |

[ENTRYPOINT_SCOPE]: Table and RecordBatch operations
- call: `join(right, keys, join_type='left outer', ..., filter_expression=None)` is the hash join with an optional residual `Expression` filter; the `serialize`/content-key and schema-metadata semantics are the `[TOPOLOGY]` law

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                                  |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------------------ |
|  [01]   | `select(columns)` / `column(name)`                  | instance | select columns, extract one column                            |
|  [02]   | `filter(mask)` / `take(indices)`                    | instance | boolean filter and gather                                     |
|  [03]   | `slice(offset, length)` / `drop_null()`             | instance | row slice and drop nulls                                      |
|  [04]   | `sort_by(sorting)`                                  | instance | sort by one or more columns                                   |
|  [05]   | `group_by(keys).aggregate(aggregations)`            | instance | grouped reduction via `TableGroupBy`                          |
|  [06]   | `join(other, keys, ...)` / `join_asof`              | instance | hash and as-of joins                                          |
|  [07]   | `append_column / add_column / set_column`           | instance | add or replace a column                                       |
|  [08]   | `drop_columns / remove_column / rename_columns`     | instance | drop and rename columns                                       |
|  [09]   | `join(..., filter_expression=None)`                 | instance | hash join with a residual `Expression` filter (see `- call:`) |
|  [10]   | `cast(target_schema)` / `combine_chunks()`          | instance | cast schema, coalesce chunks                                  |
|  [11]   | `to_batches()` / `to_reader()` / `arrow_c_stream()` | instance | iterate as batches/reader; export PyCapsule C-stream          |
|  [12]   | `RecordBatch.serialize(memory_pool=None)`           | instance | write one batch to a `Buffer` as a schema-less IPC message    |
|  [13]   | `Buffer.size` / `bytes(buffer)`                     | property | byte length and zero-copy `bytes` view of a `Buffer`          |
|  [14]   | `to_pandas()` / `to_pydict()` / `to_pylist()`       | instance | export to pandas, dicts, rows                                 |
|  [15]   | `to_struct_array()` / `flatten()`                   | instance | struct-array view and struct flatten                          |
|  [16]   | `num_rows` / `num_columns` / `schema`               | property | row count, column count, and the `Schema`                     |
|  [17]   | `replace_schema_metadata(metadata=None)`            | instance | copy with schema-level `{bytes: bytes}` metadata replaced     |

[ENTRYPOINT_SCOPE]: compute, IO, and filesystem submodules

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `compute.sum/min/max/mean/count`                                             | static  | vectorized reductions                 |
|  [02]   | `compute.filter/take/sort_indices`                                           | static  | selection and ordering kernels        |
|  [03]   | `compute.if_else/case_when/fill_null`                                        | static  | conditional and null-fill kernels     |
|  [04]   | `compute.field(name)` / `compute.scalar(v)`                                  | static  | dataset filter expressions            |
|  [05]   | `parquet.read_table` / `parquet.write_table`                                 | static  | read/write Parquet files              |
|  [06]   | `parquet.ParquetFile` / `ParquetDataset`                                     | ctor    | row-group and partitioned access      |
|  [07]   | `parquet.write_to_dataset`                                                   | static  | partitioned dataset write             |
|  [08]   | `csv.read_csv` / `csv.write_csv` / `open_csv`                                | static  | read, write, and stream CSV           |
|  [09]   | `feather.read_table` / `feather.write_feather`                               | static  | Arrow IPC file (Feather)              |
|  [10]   | `ipc.open_stream` / `ipc.new_stream`                                         | static  | Arrow streaming IPC                   |
|  [11]   | `OSFile(path, mode='r', memory_pool=None)`                                   | ctor    | on-disk `NativeFile` sink/source      |
|  [12]   | `dataset.dataset(source, format, partitioning)`                              | factory | multi-file lazy dataset with pushdown |
|  [13]   | `fs.LocalFileSystem`                                                         | ctor    | local filesystem                      |
|  [14]   | `fs.S3FileSystem` / `GcsFileSystem` / `AzureFileSystem` / `HadoopFileSystem` | ctor    | cloud object stores                   |

[ENTRYPOINT_SCOPE]: streaming execution, dataset write, UDFs, and cross-engine plans
- call: `acero.Declaration(kind, options, inputs)` with `kind` in `"scan"`/`"filter"`/`"project"`/`"aggregate"`/`"hashjoin"`/`"asofjoin"`/`"order_by"`, then `Declaration.from_sequence([decls]) -> .to_table()`/`.to_reader()`
- call: `dataset.write_dataset(data, base_dir, format=, partitioning=, existing_data_behavior=, max_rows_per_file=, file_visitor=, ...)`; `dataset.Scanner.from_dataset(ds, columns=, filter=, batch_size=) -> to_table()`/`to_batches()`; `parquet.write_table(table, where, compression='snappy', row_group_size=, sorting_columns=, encryption_properties=, write_page_index=, ...)`

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------------------- | :------ | :------------------------------------------------------ |
|  [01]   | `acero.Declaration(kind, options, inputs)`                  | ctor    | build a streaming exec-node (kinds in `- call:`)        |
|  [02]   | `acero.Declaration.from_sequence([decls])`                  | factory | chain nodes; materialize to `Table`/`RecordBatchReader` |
|  [03]   | `compute.field(name)` / `compute.scalar(v)`                 | static  | predicate algebra (`&`/`\|`/`~`/`==`/`<`/`isin`)        |
|  [04]   | `compute.register_scalar_function`                          | static  | register a Python scalar kernel                         |
|  [05]   | `compute.register_aggregate_function`                       | static  | register a hash-aggregate kernel                        |
|  [06]   | `compute.register_vector_function`                          | static  | register a vector kernel                                |
|  [07]   | `compute.register_tabular_function`                         | static  | register a tabular kernel                               |
|  [08]   | `compute.call_function` / `list_functions` / `get_function` | static  | invoke a registered kernel; introspect registry         |
|  [09]   | `dataset.write_dataset(...)`                                | static  | partitioned multi-file write (args in `- call:`)        |
|  [10]   | `dataset.Scanner.from_dataset(...)`                         | factory | configured scan with pushdown (args in `- call:`)       |
|  [11]   | `parquet.write_table(...)`                                  | static  | full-control Parquet write (args in `- call:`)          |
|  [12]   | `parquet.encryption.CryptoFactory`                          | ctor    | build file encryption/decryption properties             |
|  [13]   | `parquet.encryption.KmsConnectionConfig`                    | ctor    | KMS endpoint and credentials                            |
|  [14]   | `parquet.encryption.EncryptionConfiguration`                | ctor    | per-column encryption key policy                        |
|  [15]   | `RecordBatchReader.from_stream` / `from_batches`            | factory | import a C-stream object; wrap a batch iterator         |
|  [16]   | `substrait.run_query`                                       | static  | run a Substrait plan against Arrow tables               |
|  [17]   | `substrait.serialize_expressions`                           | static  | Arrow `Expression` -> Substrait bytes                   |
|  [18]   | `substrait.deserialize_expressions`                         | static  | Substrait bytes -> Arrow `Expression`                   |
|  [19]   | `flight.connect` / `FlightClient` / `FlightServerBase`      | static  | Arrow Flight client/server for columnar RPC             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- a `Table` is a list of `ChunkedArray` columns sharing one `Schema`; a `RecordBatch` is a single contiguous chunk
- `Array`/`ChunkedArray` carry a `DataType`, a validity bitmap, and value buffers; nested types (`struct`, `list`, `map`) compose child arrays
- dtype factory functions return `DataType` values; `field(name, type, nullable)` and `schema(fields)` build metadata; `KeyValueMetadata` attaches string metadata
- `compute` kernels operate on `Array`/`ChunkedArray`/`Table` and return Arrow values; `compute.field`/`compute.scalar` build `Expression` predicates whose `&`/`|`/`~`/comparison operators compose for `dataset`/`acero`/`Table.join` pushdown
- `Table.group_by(keys).aggregate([(col, func)])` returns a `Table`; aggregation functions follow the `compute` hash-aggregate names
- `dataset.dataset(...)` scans many files lazily with projection and filter pushdown; `Scanner.from_dataset(filter=, columns=)` materializes via `to_table()`/`to_batches()`
- `acero.Declaration` builds an explicit streaming exec-node graph (scan/filter/project/aggregate/join/order) that runs out-of-core and emits a `Table` or `RecordBatchReader`, the native alternative to chaining `Table` ops in memory
- `__arrow_c_stream__`/`RecordBatchReader.from_stream` (C-stream) and `__arrow_c_array__` (C-array) carry zero-copy interchange with polars, pandas, DuckDB, nanoarrow, and arro3
- `RecordBatch.serialize(memory_pool=None)` writes one contiguous batch to a `Buffer` as a schema-less encapsulated IPC message; `Table.combine_chunks().to_batches()[0].serialize()` is the canonical whole-table byte source for a content key (an empty table keys off `b""`), and `Buffer.size`/`bytes(buffer)` read its span, distinct from the full-stream `nanoarrow.ArrayStream(...).read_all().serialize()` IPC path that carries a `Schema`

[STACKING]:
- `substrait`(`.api/substrait.md`): `substrait.serialize_expressions`/`run_query` carry a cross-engine query plan and `flight.connect` carries columnar RPC when a plan or batch crosses a process/engine boundary.
- `deltalake`(`.api/deltalake.md`): `parquet.write_table(sorting_columns=, write_page_index=, encryption_properties=)` and `dataset.write_dataset(partitioning=HivePartitioning(...), existing_data_behavior=)` write the columnar Parquet files a lakehouse writer registers; `parquet.encryption.CryptoFactory`+`KmsConnectionConfig` own column-level modular encryption.
- `duckdb`(`.api/duckdb.md`): `Table`/`RecordBatchReader` cross zero-copy through the PyCapsule C-stream, and `compute.register_scalar_function`/`register_aggregate_function` kernels dispatch through the `call_function` registry inside `acero`/`dataset` expressions.
- within-lib: geometry scan ingestion decodes `arrow_las` output to `pyarrow.Table`; the energy/simulate plane crosses result frames through `Table.from_pydict` at the frame edge.

[LOCAL_ADMISSION]:
- Treat `Table`/`RecordBatch`/`RecordBatchReader` as the canonical zero-copy interchange shape between polars, pandas, Parquet, Iceberg, and DuckDB; build with `Table.from_pydict`/`from_arrays`/`from_pandas` or import a C-stream via `RecordBatchReader.from_stream`.
- Compose dtypes from the factory functions and assemble `Schema` explicitly where a schema contract is fixed.
- Run vectorized reductions and selection through `compute` kernels, grouped reductions through `group_by(...).aggregate(...)`, and multi-stage pipelines through `acero.Declaration`.
- Read and write columnar files through `parquet`/`feather`/`csv`/`orc`, scan multi-file partitioned data through `dataset`/`Scanner` with `compute.field` filter expressions for pushdown, and write partitioned output with `dataset.write_dataset`.

[RAIL_LAW]:
- Package: `pyarrow`
- Owns: Arrow columnar memory, schema metadata, vectorized compute kernels with the Python-UDF registry, the Acero streaming execution engine, Substrait/Flight transport, and Parquet/IPC/CSV/JSON/ORC/dataset IO
- Accept: Python sequences, NumPy arrays, pandas frames, Arrow C-interface/C-stream objects, columnar files, and registered Python kernels
- Reject: row-wise Python iteration over columns, hand-rolled Parquet/IPC parsing or Parquet encryption, schema inference where a fixed schema contract exists, bespoke plan/batch serialization where Substrait/Flight/C-stream apply
