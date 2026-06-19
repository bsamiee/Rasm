# [PY_DATA_API_PYARROW]

`pyarrow` is the Python binding to Apache Arrow's columnar memory format, owning typed `Array`/`ChunkedArray` columns, `RecordBatch`/`Table` tables, `Field`/`Schema` metadata, and the C data interface for zero-copy interchange. Top-level constructor functions (`array`, `table`, `record_batch`, `chunked_array`) and a dtype factory family (`int64`, `float64`, `timestamp`, `list_`, `struct`, `decimal128`, `dictionary`) build these values; the `compute` module supplies vectorized kernels, and the `parquet`, `csv`, `feather`, `ipc`, `dataset`, and `fs` submodules own format IO and filesystem access. `Table.group_by(...).aggregate(...)` runs grouped aggregation natively.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyarrow`
- package: `pyarrow`
- module: `pyarrow`
- asset: C++ extension (Apache Arrow)
- rail: Arrow columnar memory

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
|  [09]   | `cast(target_schema)` / `combine_chunks()`      | transform      | cast schema, coalesce chunks         |
|  [10]   | `to_batches()` / `to_reader()`                  | stream         | iterate as batches or a reader       |
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
|  [12]   | `fs.LocalFileSystem / S3FileSystem / GcsFileSystem` | filesystem     | local and cloud object stores         |

## [04]-[IMPLEMENTATION_LAW]

[ARROW_TOPOLOGY]:
- a `Table` is a list of `ChunkedArray` columns sharing one `Schema`; a `RecordBatch` is a single contiguous chunk
- `Array`/`ChunkedArray` carry a `DataType`, a validity bitmap, and value buffers; nested types (`struct`, `list`, `map`) compose child arrays
- dtype factory functions return `DataType` values; `field(name, type, nullable)` and `schema(fields)` build metadata; `KeyValueMetadata` attaches string metadata
- `compute` kernels operate on `Array`/`ChunkedArray`/`Table` and return Arrow values; `compute.field`/`compute.scalar` build `Expression` predicates for `dataset` pushdown
- `Table.group_by(keys).aggregate([(col, func)])` returns a `Table`; aggregation functions follow the `compute` hash-aggregate names
- `dataset.dataset(...)` scans many files lazily with projection and filter pushdown; `to_table()`/`to_batches()` materialize
- the C data interface (`from_pandas`, `to_pandas`, `__arrow_c_array__`) enables zero-copy interchange with polars, pandas, DuckDB, and other Arrow consumers

[LOCAL_ADMISSION]:
- Treat `Table`/`RecordBatch` as the canonical zero-copy interchange shape between polars, pandas, Parquet, and DuckDB; build with `Table.from_pydict`/`from_arrays`/`from_pandas`.
- Compose dtypes from the factory functions and assemble `Schema` explicitly; avoid relying on inference where a schema contract is fixed.
- Run vectorized reductions and selection through `compute` kernels and grouped reductions through `group_by(...).aggregate(...)`; do not iterate rows in Python.
- Read and write columnar files through `parquet`/`feather`/`csv` and scan multi-file partitioned data through `dataset` with `compute.field` filter expressions for pushdown.

[RAIL_LAW]:
- Package: `pyarrow`
- Owns: Arrow columnar memory, schema metadata, vectorized compute kernels, and Parquet/IPC/CSV/dataset IO
- Accept: Python sequences, NumPy arrays, pandas frames, Arrow C-interface objects, and columnar files
- Reject: row-wise Python iteration over columns, hand-rolled Parquet/IPC parsing, schema inference where a fixed schema contract exists
