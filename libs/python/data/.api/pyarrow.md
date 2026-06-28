# [PY_DATA_API_PYARROW]

`pyarrow` is the Python binding to Apache Arrow's columnar memory format, owning typed `Array`/`ChunkedArray` columns, `RecordBatch`/`Table` tables, `Field`/`Schema` metadata, and the PyCapsule C data/stream interface (`__arrow_c_array__`/`__arrow_c_stream__`/`__arrow_c_schema__`) for zero-copy interchange. Top-level constructor functions (`array`, `table`, `record_batch`, `chunked_array`) and a dtype factory family (`int64`, `float64`, `timestamp`, `list_`, `struct`, `decimal128`, `dictionary`) build these values; the `compute` module supplies vectorized kernels plus a user-UDF registry, `acero` exposes the streaming `Declaration` execution engine, `substrait` consumes cross-engine query plans, and the `parquet`, `csv`, `json`, `orc`, `feather`, `ipc`, `dataset`, `flight`, and `fs` submodules own format IO, RPC, and filesystem access. `Table.group_by(...).aggregate(...)` runs grouped aggregation natively.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyarrow`
- package: `pyarrow`
- module: `pyarrow`
- version: `23.0.1`
- license: `Apache-2.0`
- rail: Arrow columnar memory
- submodules: `compute`, `acero`, `substrait`, `dataset`, `parquet`, `csv`, `json`, `orc`, `feather`, `ipc`, `flight`, `fs`, `cuda`, `interchange`, `types`, `util`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tabular and array values
- rail: Arrow columnar memory

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [ROLE]                                 |
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
- rail: Arrow columnar memory

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [ROLE]                        |
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
- rail: Arrow columnar memory

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]   | [ROLE]                                                       |
| :-----: | :-------------------------------------------------------- | :-------------- | :----------------------------------------------------------- |
|  [01]   | `acero.Declaration`                                       | exec plan       | composable streaming exec-node graph (`from_sequence`/`to_table`/`to_reader`) |
|  [02]   | `acero.ScanNodeOptions` / `FilterNodeOptions` / `ProjectNodeOptions` | exec node | scan/filter/project node configs                             |
|  [03]   | `acero.AggregateNodeOptions` / `HashJoinNodeOptions` / `AsofJoinNodeOptions` / `OrderByNodeOptions` | exec node | aggregate/join/order node configs |
|  [04]   | `compute.Expression`                                      | predicate algebra | filter/project expression tree (`field()`/`scalar()`/`&`/`|`/`~`/comparisons) |
|  [05]   | `dataset.Dataset` / `FileSystemDataset` / `UnionDataset`  | dataset         | multi-file lazy dataset with pushdown                        |
|  [06]   | `dataset.Scanner`                                         | scanner         | configured scan (`columns`/`filter`/`batch_size`) -> `to_table`/`to_batches` |
|  [07]   | `dataset.ParquetFileFormat` / `CsvFileFormat` / `JsonFileFormat` / `OrcFileFormat` / `IpcFileFormat` | file format | per-format scan/write options |
|  [08]   | `dataset.HivePartitioning` / `DirectoryPartitioning` / `FilenamePartitioning` | partitioning | dataset partition layout schemes              |
|  [09]   | `dataset.ParquetEncryptionConfig` / `ParquetDecryptionConfig` | encryption  | column-level Parquet modular encryption configs              |
|  [10]   | `RecordBatchReader`                                       | stream          | C-stream-importable batch iterator (`from_stream`/`from_batches`/`read_all`/`__arrow_c_stream__`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and dtype factories
- rail: Arrow columnar memory

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `array(obj, type, ...)`                          | construct      | build an `Array` from a sequence   |
|  [02]   | `chunked_array(arrays, type)`                    | construct      | build a `ChunkedArray`             |
|  [03]   | `record_batch(data, ...)`                        | construct      | build a `RecordBatch`              |
|  [04]   | `table(data, schema, ...)`                       | construct      | build a `Table` from dict/arrays   |
|  [05]   | `scalar(value, type)` / `nulls(size, type)`      | construct      | scalar and all-null array          |
|  [06]   | `Table.from_pydict / from_pylist / from_arrays`  | construct      | build `Table` from Python/arrays   |
|  [07]   | `Table.from_pandas(df)` / `from_batches`         | interop        | build `Table` from pandas/batches  |
|  [08]   | `schema(fields)` / `field(name, type)`           | metadata       | build schema and field             |
|  [09]   | `int8/16/32/64` / `uint8/16/32/64`               | dtype factory  | integer dtypes                     |
|  [10]   | `float16/32/64` / `bool_` / `string` / `binary`  | dtype factory  | float, bool, string, binary dtypes |
|  [11]   | `timestamp(unit, tz)` / `date32` / `time64`      | dtype factory  | temporal dtypes                    |
|  [12]   | `list_/large_list/struct/map_/dictionary`        | dtype factory  | nested and encoded dtypes          |
|  [13]   | `decimal128/decimal256(precision, scale)`        | dtype factory  | decimal dtypes                     |
|  [14]   | `concat_tables / concat_arrays / concat_batches` | combine        | concatenate Arrow values           |
|  [15]   | `unify_schemas(schemas)` / `infer_type(values)`  | metadata       | merge schemas, infer a dtype       |
|  [16]   | `FixedSizeListArray.from_arrays(values, list_size=None, type=None, mask=None)` / `ListArray.from_arrays(offsets, values, type=None, pool=None, mask=None)` | construct | build a fixed-length/variable list column from a flat value buffer (the multi-dim column builder) |
|  [17]   | `Schema.empty_table()` / `Schema.names` / `Schema.types` | metadata | zero-row `Table` matching the schema; the ordered field-name `list[str]` and `list[DataType]` |

[ENTRYPOINT_SCOPE]: Table and RecordBatch operations
- rail: Arrow columnar memory

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :---------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `select(columns)` / `column(name)`              | projection     | select columns, extract one column   |
|  [02]   | `filter(mask)` / `take(indices)`                | filter         | boolean filter and gather            |
|  [03]   | `slice(offset, length)` / `drop_null()`         | filter         | row slice and drop nulls             |
|  [04]   | `sort_by(sorting)`                              | sort           | sort by one or more columns          |
|  [05]   | `group_by(keys).aggregate(aggregations)`        | aggregation    | grouped reduction via `TableGroupBy` |
|  [06]   | `join(other, keys, ...)` / `join_asof`          | join           | hash and as-of joins                 |
|  [07]   | `append_column / add_column / set_column`       | mutation       | add or replace a column              |
|  [08]   | `drop_columns / remove_column / rename_columns` | mutation       | drop and rename columns              |
|  [06b]  | `join(right, keys, join_type='left outer', ..., filter_expression=None)` | join | hash join with optional residual `Expression` filter |
|  [09]   | `cast(target_schema)` / `combine_chunks()`      | transform      | cast schema, coalesce chunks         |
|  [10]   | `to_batches()` / `to_reader()` / `__arrow_c_stream__()` | stream | iterate as batches/reader; export PyCapsule C-stream |
|  [10b]  | `RecordBatch.serialize(memory_pool=None)` -> `Buffer` | serialize | write one contiguous batch to a `Buffer` as a schema-less encapsulated IPC message (the canonical whole-batch content-key byte source over `Table.combine_chunks().to_batches()`); reconstruct via a separately-carried `Schema` |
|  [10c]  | `Buffer.size` / `bytes(buffer)`                 | memory         | byte length and zero-copy `bytes` view of a `Buffer` (the IPC-serialized batch span the content key reads) |
|  [11]   | `to_pandas()` / `to_pydict()` / `to_pylist()`   | interop        | export to pandas, dicts, rows        |
|  [12]   | `to_struct_array()` / `flatten()`               | reshape        | struct-array view and struct flatten |

[ENTRYPOINT_SCOPE]: compute, IO, and filesystem submodules
- rail: Arrow columnar memory

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `compute.sum/min/max/mean/count`                    | kernel         | vectorized reductions                 |
|  [02]   | `compute.filter/take/sort_indices`                  | kernel         | selection and ordering kernels        |
|  [03]   | `compute.if_else/case_when/fill_null`               | kernel         | conditional and null-fill kernels     |
|  [04]   | `compute.field(name)` / `compute.scalar(v)`         | expression     | dataset filter expressions            |
|  [05]   | `parquet.read_table` / `parquet.write_table`        | Parquet IO     | read/write Parquet files              |
|  [06]   | `parquet.ParquetFile` / `ParquetDataset`            | Parquet IO     | row-group and partitioned access      |
|  [07]   | `parquet.write_to_dataset`                          | Parquet IO     | partitioned dataset write             |
|  [08]   | `csv.read_csv` / `csv.write_csv` / `open_csv`       | CSV IO         | read, write, and stream CSV           |
|  [09]   | `feather.read_table` / `feather.write_feather`      | Feather IO     | Arrow IPC file (Feather)              |
|  [10]   | `ipc.open_stream` / `ipc.new_stream`                | IPC            | Arrow streaming IPC                   |
|  [11]   | `dataset.dataset(source, format, partitioning)`     | dataset        | multi-file lazy dataset with pushdown |
|  [12]   | `fs.LocalFileSystem / S3FileSystem / GcsFileSystem / AzureFileSystem / HadoopFileSystem` | filesystem | local and cloud object stores |

[ENTRYPOINT_SCOPE]: streaming execution, dataset write, UDFs, and cross-engine plans
- rail: Arrow columnar memory

| [INDEX] | [SURFACE]                                                                                                | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `acero.Declaration("scan"/"filter"/"project"/"aggregate"/"hashjoin"/"asofjoin"/"order_by", options, inputs)` | exec plan  | build a streaming exec-node                              |
|  [02]   | `acero.Declaration.from_sequence([decls])` -> `.to_table()` / `.to_reader()`                             | exec plan      | chain nodes; materialize to `Table`/`RecordBatchReader` |
|  [03]   | `compute.field(name)` / `compute.scalar(v)` -> `Expression` (`&` `|` `~` `==` `<` `isin`)                | expression     | predicate algebra for dataset/acero pushdown            |
|  [04]   | `compute.register_scalar_function(func, name, doc, in_types, out_type)`                                  | udf            | register a Python scalar kernel into the function registry |
|  [05]   | `compute.register_aggregate_function / register_vector_function / register_tabular_function`             | udf            | register hash-aggregate/vector/tabular Python kernels   |
|  [06]   | `compute.call_function(name, args)` / `list_functions()` / `get_function(name)`                          | kernel dispatch | invoke any registered kernel by name; introspect registry |
|  [07]   | `dataset.write_dataset(data, base_dir, format=, partitioning=, existing_data_behavior=, max_rows_per_file=, file_visitor=, ...)` | dataset write | partitioned multi-file write with overwrite policy |
|  [08]   | `dataset.Scanner.from_dataset(ds, columns=, filter=, batch_size=)` -> `to_table()` / `to_batches()`     | scanner        | configured scan with projection/filter pushdown          |
|  [09]   | `parquet.write_table(table, where, compression='snappy', row_group_size=, sorting_columns=, encryption_properties=, write_page_index=, ...)` | Parquet IO | full-control Parquet write |
|  [10]   | `parquet.encryption.CryptoFactory` / `KmsConnectionConfig` / `EncryptionConfiguration`                  | Parquet IO     | modular column-encryption key management                 |
|  [11]   | `RecordBatchReader.from_stream(obj)` / `from_batches(schema, batches)`                                   | stream         | import any C-stream object; wrap an iterator of batches  |
|  [12]   | `substrait.run_query(plan, table_provider=)` / `serialize_expressions` / `deserialize_expressions`      | cross-engine   | execute/round-trip a Substrait plan against Arrow tables |
|  [13]   | `flight.connect(location)` / `FlightClient` / `FlightServerBase`                                         | RPC            | Arrow Flight client/server for columnar RPC transport    |

## [04]-[IMPLEMENTATION_LAW]

[ARROW_TOPOLOGY]:
- a `Table` is a list of `ChunkedArray` columns sharing one `Schema`; a `RecordBatch` is a single contiguous chunk
- `Array`/`ChunkedArray` carry a `DataType`, a validity bitmap, and value buffers; nested types (`struct`, `list`, `map`) compose child arrays
- dtype factory functions return `DataType` values; `field(name, type, nullable)` and `schema(fields)` build metadata; `KeyValueMetadata` attaches string metadata
- `compute` kernels operate on `Array`/`ChunkedArray`/`Table` and return Arrow values; `compute.field`/`compute.scalar` build `Expression` predicates whose `&`/`|`/`~`/comparison operators compose for `dataset`/`acero`/`Table.join` pushdown
- `Table.group_by(keys).aggregate([(col, func)])` returns a `Table`; aggregation functions follow the `compute` hash-aggregate names
- `dataset.dataset(...)` scans many files lazily with projection and filter pushdown; `Scanner.from_dataset(filter=, columns=)` materializes via `to_table()`/`to_batches()`
- `acero.Declaration` builds an explicit streaming exec-node graph (scan/filter/project/aggregate/join/order) that runs out-of-core and emits a `Table` or `RecordBatchReader`; it is the native alternative to chaining `Table` ops in memory
- the PyCapsule C-stream interface (`__arrow_c_stream__`/`RecordBatchReader.from_stream`) and C-array interface (`__arrow_c_array__`) enable zero-copy interchange with polars, pandas, DuckDB, nanoarrow, and arro3
- `RecordBatch.serialize(memory_pool=None)` writes one contiguous batch to a `Buffer` as a schema-less encapsulated IPC message; `Table.combine_chunks().to_batches()[0].serialize()` is the canonical whole-table byte source for a content key (an empty table keys off `b""`), and `Buffer.size`/`bytes(buffer)` read its span — distinct from the full-stream `nanoarrow.ArrayStream(...).read_all().serialize()` IPC path that carries a `Schema`

[STACKING_LAW]:
- `parquet.write_table(..., sorting_columns=, write_page_index=, encryption_properties=)` and `dataset.write_dataset(partitioning=HivePartitioning(...), existing_data_behavior=)` are the canonical columnar writers feeding the Iceberg `add_files` path; `parquet.encryption.CryptoFactory`+`KmsConnectionConfig` own column-level modular encryption — never a hand-rolled cipher pass.
- Register Python domain kernels with `compute.register_scalar_function`/`register_aggregate_function` so they dispatch through the same `call_function` registry as native kernels and remain usable inside `acero`/`dataset` expressions.
- `substrait.run_query`/`serialize_expressions` carry cross-engine plans; `flight.connect` carries columnar RPC. Prefer these over bespoke serialization when a plan or batch must cross a process/engine boundary.

[LOCAL_ADMISSION]:
- Treat `Table`/`RecordBatch`/`RecordBatchReader` as the canonical zero-copy interchange shape between polars, pandas, Parquet, Iceberg, and DuckDB; build with `Table.from_pydict`/`from_arrays`/`from_pandas` or import a C-stream via `RecordBatchReader.from_stream`.
- Compose dtypes from the factory functions and assemble `Schema` explicitly; avoid relying on inference where a schema contract is fixed.
- Run vectorized reductions and selection through `compute` kernels, grouped reductions through `group_by(...).aggregate(...)`, and multi-stage pipelines through `acero.Declaration`; do not iterate rows in Python.
- Read and write columnar files through `parquet`/`feather`/`csv`/`orc` and scan multi-file partitioned data through `dataset`/`Scanner` with `compute.field` filter expressions for pushdown; write partitioned output with `dataset.write_dataset`.

[RAIL_LAW]:
- Package: `pyarrow`
- Owns: Arrow columnar memory, schema metadata, vectorized compute kernels + Python-UDF registry, the Acero streaming execution engine, Substrait/Flight transport, and Parquet/IPC/CSV/JSON/ORC/dataset IO
- Accept: Python sequences, NumPy arrays, pandas frames, Arrow C-interface/C-stream objects, columnar files, and registered Python kernels
- Reject: row-wise Python iteration over columns, hand-rolled Parquet/IPC parsing or Parquet encryption, schema inference where a fixed schema contract exists, bespoke plan/batch serialization where Substrait/Flight/C-stream apply
