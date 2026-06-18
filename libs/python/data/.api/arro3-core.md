# [PY_DATA_API_ARRO3_CORE]

`arro3.core` supplies a zero-copy Apache Arrow memory model implemented in Rust for Python 3.15+, exposing typed `Array`, `ChunkedArray`, `RecordBatch`, `Table`, `Schema`, `Field`, `DataType`, `Scalar`, `Buffer`, `ArrayReader`, and `RecordBatchReader` along with structural array builders for list, struct, and dictionary layouts. All types export and consume the Arrow PyCapsule interface (`__arrow_c_array__`, `__arrow_c_stream__`) for lossless interop with `pyarrow`, `nanoarrow`, `polars`, and any PyCapsule-compatible consumer.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-core`
- package: `arro3-core`
- module: `arro3.core`
- asset: native extension (Rust/PyO3)
- rail: arrow-memory

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container types
- rail: arrow-memory

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [ROLE]                                   |
| :-----: | :------------------ | :---------------- | :--------------------------------------- |
|   [1]   | `Array`             | contiguous buffer | single-chunk typed array                 |
|   [2]   | `ChunkedArray`      | chunked container | multi-chunk typed column                 |
|   [3]   | `RecordBatch`       | tabular batch     | schema-aligned row batch                 |
|   [4]   | `Table`             | tabular container | multi-chunk schema-aligned table         |
|   [5]   | `Scalar`            | scalar value      | typed single-element Arrow value         |
|   [6]   | `Buffer`            | raw buffer        | opaque bytes container                   |
|   [7]   | `ArrayReader`       | streaming reader  | `__arrow_c_stream__` array source        |
|   [8]   | `RecordBatchReader` | streaming reader  | `__arrow_c_stream__` record-batch source |

[PUBLIC_TYPE_SCOPE]: schema and type system
- rail: arrow-memory

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [ROLE]                           |
| :-----: | :--------- | :-------------- | :------------------------------- |
|   [1]   | `Schema`   | schema value    | ordered field-to-type mapping    |
|   [2]   | `Field`    | field value     | named typed nullable field       |
|   [3]   | `DataType` | type descriptor | Arrow logical type with metadata |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Array construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [CAPABILITY]                    |
| :-----: | :---------------------------------- | :-------------- | :------------------------------ |
|   [1]   | `Array.from_arrow(input)`           | capsule intake  | PyCapsule array import          |
|   [2]   | `Array.from_arrow_pycapsule(s, a)`  | capsule intake  | explicit schema + array capsule |
|   [3]   | `Array.from_buffer(buffer)`         | buffer intake   | construct from `Buffer`         |
|   [4]   | `Array.from_numpy(array)`           | numpy intake    | zero-copy from NumPy array      |
|   [5]   | `Array.cast(target_type)`           | type projection | cast to target `DataType`       |
|   [6]   | `Array.slice(offset, length)`       | window          | zero-copy slice                 |
|   [7]   | `Array.take(indices)`               | gather          | gather by index array           |
|   [8]   | `Array.to_numpy()`                  | export          | export to NumPy                 |
|   [9]   | `Array.to_pylist()`                 | export          | export to Python list           |
|  [10]   | `Array.buffer(i)`                   | buffer access   | access raw buffer by index      |
|  [11]   | `Array.field` / `Array.type`        | metadata        | field descriptor and type       |
|  [12]   | `Array.null_count` / `Array.nbytes` | metadata        | null count and byte size        |

[ENTRYPOINT_SCOPE]: ChunkedArray construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------- | :-------------- | :----------------------------- |
|   [1]   | `ChunkedArray.from_arrow(input)`           | capsule intake  | PyCapsule chunked-array import |
|   [2]   | `ChunkedArray.cast(target_type)`           | type projection | cast all chunks                |
|   [3]   | `ChunkedArray.combine_chunks()`            | consolidation   | flatten to single `Array`      |
|   [4]   | `ChunkedArray.rechunk(max_chunksize)`      | repartition     | re-chunk with size cap         |
|   [5]   | `ChunkedArray.chunk(i)` / `.chunks`        | access          | chunk by index or all chunks   |
|   [6]   | `ChunkedArray.slice(offset, length)`       | window          | zero-copy slice across chunks  |
|   [7]   | `ChunkedArray.equals(other)`               | equality        | value equality                 |
|   [8]   | `ChunkedArray.to_numpy()` / `.to_pylist()` | export          | export to NumPy or Python list |
|   [9]   | `ChunkedArray.length()` / `.num_chunks`    | metadata        | element count and chunk count  |

[ENTRYPOINT_SCOPE]: RecordBatch construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------- | :-------------- | :------------------------------ |
|   [1]   | `RecordBatch.from_arrays(arrays, ...)`        | array intake    | build from column array list    |
|   [2]   | `RecordBatch.from_pydict(mapping, ...)`       | dict intake     | build from `{name: array}` dict |
|   [3]   | `RecordBatch.from_struct_array(struct_array)` | struct intake   | build from struct array         |
|   [4]   | `RecordBatch.from_arrow(input)`               | capsule intake  | PyCapsule batch import          |
|   [5]   | `RecordBatch.column(i)` / `.columns`          | column access   | access column by index or all   |
|   [6]   | `RecordBatch.field(i)` / `.schema`            | schema access   | field or schema descriptor      |
|   [7]   | `RecordBatch.select(columns)`                 | projection      | select named or indexed columns |
|   [8]   | `RecordBatch.add_column(i, field, column)`    | mutation        | insert column at position       |
|   [9]   | `RecordBatch.set_column(i, field, column)`    | mutation        | replace column at position      |
|  [10]   | `RecordBatch.remove_column(i)`                | mutation        | remove column by index          |
|  [11]   | `RecordBatch.append_column(field, column)`    | mutation        | append column at end            |
|  [12]   | `RecordBatch.slice(offset, length)`           | window          | zero-copy row slice             |
|  [13]   | `RecordBatch.take(indices)`                   | gather          | gather rows by index            |
|  [14]   | `RecordBatch.with_schema(schema)`             | schema mutation | replace schema metadata         |
|  [15]   | `RecordBatch.to_struct_array()`               | export          | export as struct `Array`        |
|  [16]   | `RecordBatch.equals(other)`                   | equality        | batch value equality            |

[ENTRYPOINT_SCOPE]: Table construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]  | [CAPABILITY]                     |
| :-----: | :------------------------------------ | :-------------- | :------------------------------- |
|   [1]   | `Table.from_arrays(arrays, ...)`      | array intake    | build from column list           |
|   [2]   | `Table.from_pydict(mapping, ...)`     | dict intake     | build from `{name: array}` dict  |
|   [3]   | `Table.from_batches(batches, schema)` | batch intake    | build from record-batch list     |
|   [4]   | `Table.from_arrow(input)`             | capsule intake  | PyCapsule table import           |
|   [5]   | `Table.column(i)` / `.columns`        | column access   | column by index or all columns   |
|   [6]   | `Table.select(columns)`               | projection      | select named or indexed columns  |
|   [7]   | `Table.drop_columns(columns)`         | projection      | remove named columns             |
|   [8]   | `Table.rename_columns(names)`         | schema mutation | rename all columns               |
|   [9]   | `Table.slice(offset, length)`         | window          | zero-copy row slice              |
|  [10]   | `Table.rechunk(max_chunksize)`        | repartition     | re-chunk all columns             |
|  [11]   | `Table.combine_chunks()`              | consolidation   | flatten to single batch each col |
|  [12]   | `Table.to_batches()`                  | export          | emit `RecordBatch` list          |
|  [13]   | `Table.to_reader()`                   | export          | emit `RecordBatchReader`         |
|  [14]   | `Table.to_struct_array()`             | export          | export as struct `Array`         |

[ENTRYPOINT_SCOPE]: schema, field, and type operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [CAPABILITY]                        |
| :-----: | :------------------------------ | :---------------- | :---------------------------------- |
|   [1]   | `Schema.from_arrow(input)`      | capsule intake    | PyCapsule schema import             |
|   [2]   | `Field.with_name(name)`         | mutation          | rename field                        |
|   [3]   | `Field.with_type(new_type)`     | mutation          | retype field                        |
|   [4]   | `Field.with_nullable(nullable)` | mutation          | set nullability                     |
|   [5]   | `Field.with_metadata(metadata)` | mutation          | attach key-value metadata           |
|   [6]   | `Field.remove_metadata()`       | mutation          | strip all metadata                  |
|   [7]   | `DataType` static constructors  | type construction | `int8`..`float64`, `timestamp`, ... |
|   [8]   | `DataType.is_*` predicates      | type test         | 50+ boolean type predicates         |
|   [9]   | `Scalar.from_arrow(input)`      | capsule intake    | PyCapsule scalar import             |
|  [10]   | `Scalar.as_py()`                | export            | convert to Python scalar            |
|  [11]   | `Scalar.cast(target_type)`      | type projection   | cast scalar type                    |

[ENTRYPOINT_SCOPE]: streaming readers and structural builders
- rail: arrow-memory

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------------ | :------------- | :---------------------------------- |
|   [1]   | `ArrayReader.from_arrays(field, arrays)`    | construction   | build from array iterable           |
|   [2]   | `ArrayReader.from_stream(data)`             | stream intake  | consume PyCapsule stream            |
|   [3]   | `ArrayReader.read_all()`                    | consume        | collect all arrays                  |
|   [4]   | `ArrayReader.read_next_array()`             | consume        | pull next array                     |
|   [5]   | `RecordBatchReader.from_batches(schema, b)` | construction   | build from batch iterable           |
|   [6]   | `RecordBatchReader.from_stream(data)`       | stream intake  | consume PyCapsule stream            |
|   [7]   | `RecordBatchReader.read_all()`              | consume        | collect all batches as `Table`      |
|   [8]   | `RecordBatchReader.read_next_batch()`       | consume        | pull next `RecordBatch`             |
|   [9]   | `list_array(offsets, values, ...)`          | builder        | variable-length list array          |
|  [10]   | `fixed_size_list_array(values, list_size)`  | builder        | fixed-size list array               |
|  [11]   | `struct_array(arrays, fields, ...)`         | builder        | struct array from component columns |
|  [12]   | `struct_field(values, indices)`             | accessor       | extract nested struct field         |
|  [13]   | `dictionary_dictionary(array)`              | accessor       | extract dictionary values array     |
|  [14]   | `dictionary_indices(array)`                 | accessor       | extract dictionary indices array    |
|  [15]   | `list_flatten(input)`                       | transform      | flatten one level of list nesting   |
|  [16]   | `list_offsets(input, logical)`              | accessor       | extract list offset buffer          |

## [4]-[IMPLEMENTATION_LAW]

[ARROW_MEMORY_TOPOLOGY]:
- namespace: `arro3.core` only; 11 public types plus 8 top-level builder/accessor functions
- all types implement `__arrow_c_array__` or `__arrow_c_stream__` for lossless PyCapsule handoff
- `Array` is always contiguous and single-chunk; `ChunkedArray` owns the multi-chunk case
- `RecordBatch` is a row-aligned batch over one schema; `Table` is the multi-chunk tabular owner
- `DataType` is a value object; construct via its static factory methods, not direct instantiation
- streaming types (`ArrayReader`, `RecordBatchReader`) are single-read; `.closed` property guards re-use

[LOCAL_ADMISSION]:
- Ingest from any PyCapsule-compatible source via `from_arrow` or `from_arrow_pycapsule`; never copy data through Python lists unless the source is already Python-native.
- Emit to downstream consumers via `__arrow_c_array__` or `__arrow_c_stream__`; explicit `to_pylist` / `to_numpy` is for terminal extraction only.
- Build composite arrays through `list_array`, `struct_array`, or `fixed_size_list_array` rather than constructing buffers manually.
- Keep schema and type objects immutable; use `Field.with_*` and `Schema.with_*` mutation methods to produce new descriptors.

[RAIL_LAW]:
- Package: `arro3-core`
- Owns: zero-copy Arrow memory model and PyCapsule interchange
- Accept: PyCapsule input, NumPy, and Python dicts/lists as terminal sources
- Reject: manual buffer construction where builder functions exist, re-importing already-Arrow data through Python lists
