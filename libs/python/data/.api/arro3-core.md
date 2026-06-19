# [PY_DATA_API_ARRO3_CORE]

`arro3.core` supplies a zero-copy Apache Arrow memory model implemented in Rust for Python 3.15+, exposing typed `Array`, `ChunkedArray`, `RecordBatch`, `Table`, `Schema`, `Field`, `DataType`, `Scalar`, `Buffer`, `ArrayReader`, and `RecordBatchReader` along with structural array builders for list, struct, and dictionary layouts. All types export and consume the Arrow PyCapsule interface (`__arrow_c_array__`, `__arrow_c_stream__`) for lossless interop with `pyarrow`, `nanoarrow`, `polars`, and any PyCapsule-compatible consumer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-core`
- package: `arro3-core`
- module: `arro3.core`
- asset: native extension (Rust/PyO3)
- rail: arrow-memory

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container types
- rail: arrow-memory

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [ROLE]                                   |
| :-----: | :------------------ | :---------------- | :--------------------------------------- |
|  [01]   | `Array`             | contiguous buffer | single-chunk typed array                 |
|  [02]   | `ChunkedArray`      | chunked container | multi-chunk typed column                 |
|  [03]   | `RecordBatch`       | tabular batch     | schema-aligned row batch                 |
|  [04]   | `Table`             | tabular container | multi-chunk schema-aligned table         |
|  [05]   | `Scalar`            | scalar value      | typed single-element Arrow value         |
|  [06]   | `Buffer`            | raw buffer        | opaque bytes container                   |
|  [07]   | `ArrayReader`       | streaming reader  | `__arrow_c_stream__` array source        |
|  [08]   | `RecordBatchReader` | streaming reader  | `__arrow_c_stream__` record-batch source |

[PUBLIC_TYPE_SCOPE]: schema and type system
- rail: arrow-memory

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [ROLE]                           |
| :-----: | :--------- | :-------------- | :------------------------------- |
|  [01]   | `Schema`   | schema value    | ordered field-to-type mapping    |
|  [02]   | `Field`    | field value     | named typed nullable field       |
|  [03]   | `DataType` | type descriptor | Arrow logical type with metadata |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Array construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [CAPABILITY]                    |
| :-----: | :---------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `Array.from_arrow(input)`           | capsule intake  | PyCapsule array import          |
|  [02]   | `Array.from_arrow_pycapsule(s, a)`  | capsule intake  | explicit schema + array capsule |
|  [03]   | `Array.from_buffer(buffer)`         | buffer intake   | construct from `Buffer`         |
|  [04]   | `Array.from_numpy(array)`           | numpy intake    | zero-copy from NumPy array      |
|  [05]   | `Array.cast(target_type)`           | type projection | cast to target `DataType`       |
|  [06]   | `Array.slice(offset, length)`       | window          | zero-copy slice                 |
|  [07]   | `Array.take(indices)`               | gather          | gather by index array           |
|  [08]   | `Array.to_numpy()`                  | export          | export to NumPy                 |
|  [09]   | `Array.to_pylist()`                 | export          | export to Python list           |
|  [10]   | `Array.buffer(i)`                   | buffer access   | access raw buffer by index      |
|  [11]   | `Array.field` / `Array.type`        | metadata        | field descriptor and type       |
|  [12]   | `Array.null_count` / `Array.nbytes` | metadata        | null count and byte size        |

[ENTRYPOINT_SCOPE]: ChunkedArray construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [CAPABILITY]                   |
| :-----: | :----------------------------------------- | :-------------- | :----------------------------- |
|  [01]   | `ChunkedArray.from_arrow(input)`           | capsule intake  | PyCapsule chunked-array import |
|  [02]   | `ChunkedArray.cast(target_type)`           | type projection | cast all chunks                |
|  [03]   | `ChunkedArray.combine_chunks()`            | consolidation   | flatten to single `Array`      |
|  [04]   | `ChunkedArray.rechunk(max_chunksize)`      | repartition     | re-chunk with size cap         |
|  [05]   | `ChunkedArray.chunk(i)` / `.chunks`        | access          | chunk by index or all chunks   |
|  [06]   | `ChunkedArray.slice(offset, length)`       | window          | zero-copy slice across chunks  |
|  [07]   | `ChunkedArray.equals(other)`               | equality        | value equality                 |
|  [08]   | `ChunkedArray.to_numpy()` / `.to_pylist()` | export          | export to NumPy or Python list |
|  [09]   | `ChunkedArray.length()` / `.num_chunks`    | metadata        | element count and chunk count  |

[ENTRYPOINT_SCOPE]: RecordBatch construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------- | :-------------- | :------------------------------ |
|  [01]   | `RecordBatch.from_arrays(arrays, ...)`        | array intake    | build from column array list    |
|  [02]   | `RecordBatch.from_pydict(mapping, ...)`       | dict intake     | build from `{name: array}` dict |
|  [03]   | `RecordBatch.from_struct_array(struct_array)` | struct intake   | build from struct array         |
|  [04]   | `RecordBatch.from_arrow(input)`               | capsule intake  | PyCapsule batch import          |
|  [05]   | `RecordBatch.column(i)` / `.columns`          | column access   | access column by index or all   |
|  [06]   | `RecordBatch.field(i)` / `.schema`            | schema access   | field or schema descriptor      |
|  [07]   | `RecordBatch.select(columns)`                 | projection      | select named or indexed columns |
|  [08]   | `RecordBatch.add_column(i, field, column)`    | mutation        | insert column at position       |
|  [09]   | `RecordBatch.set_column(i, field, column)`    | mutation        | replace column at position      |
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
|  [01]   | `Table.from_arrays(arrays, ...)`      | array intake    | build from column list           |
|  [02]   | `Table.from_pydict(mapping, ...)`     | dict intake     | build from `{name: array}` dict  |
|  [03]   | `Table.from_batches(batches, schema)` | batch intake    | build from record-batch list     |
|  [04]   | `Table.from_arrow(input)`             | capsule intake  | PyCapsule table import           |
|  [05]   | `Table.column(i)` / `.columns`        | column access   | column by index or all columns   |
|  [06]   | `Table.select(columns)`               | projection      | select named or indexed columns  |
|  [07]   | `Table.drop_columns(columns)`         | projection      | remove named columns             |
|  [08]   | `Table.rename_columns(names)`         | schema mutation | rename all columns               |
|  [09]   | `Table.slice(offset, length)`         | window          | zero-copy row slice              |
|  [10]   | `Table.rechunk(max_chunksize)`        | repartition     | re-chunk all columns             |
|  [11]   | `Table.combine_chunks()`              | consolidation   | flatten to single batch each col |
|  [12]   | `Table.to_batches()`                  | export          | emit `RecordBatch` list          |
|  [13]   | `Table.to_reader()`                   | export          | emit `RecordBatchReader`         |
|  [14]   | `Table.to_struct_array()`             | export          | export as struct `Array`         |

[ENTRYPOINT_SCOPE]: schema, field, and type operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY]    | [CAPABILITY]                        |
| :-----: | :------------------------------ | :---------------- | :---------------------------------- |
|  [01]   | `Schema.from_arrow(input)`      | capsule intake    | PyCapsule schema import             |
|  [02]   | `Field.with_name(name)`         | mutation          | rename field                        |
|  [03]   | `Field.with_type(new_type)`     | mutation          | retype field                        |
|  [04]   | `Field.with_nullable(nullable)` | mutation          | set nullability                     |
|  [05]   | `Field.with_metadata(metadata)` | mutation          | attach key-value metadata           |
|  [06]   | `Field.remove_metadata()`       | mutation          | strip all metadata                  |
|  [07]   | `DataType` static constructors  | type construction | `int8`..`float64`, `timestamp`, ... |
|  [08]   | `DataType.is_*` predicates      | type test         | 50+ boolean type predicates         |
|  [09]   | `Scalar.from_arrow(input)`      | capsule intake    | PyCapsule scalar import             |
|  [10]   | `Scalar.as_py()`                | export            | convert to Python scalar            |
|  [11]   | `Scalar.cast(target_type)`      | type projection   | cast scalar type                    |

[ENTRYPOINT_SCOPE]: streaming readers and structural builders
- rail: arrow-memory

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------------------------------ | :------------- | :---------------------------------- |
|  [01]   | `ArrayReader.from_arrays(field, arrays)`    | construction   | build from array iterable           |
|  [02]   | `ArrayReader.from_stream(data)`             | stream intake  | consume PyCapsule stream            |
|  [03]   | `ArrayReader.read_all()`                    | consume        | collect all arrays                  |
|  [04]   | `ArrayReader.read_next_array()`             | consume        | pull next array                     |
|  [05]   | `RecordBatchReader.from_batches(schema, b)` | construction   | build from batch iterable           |
|  [06]   | `RecordBatchReader.from_stream(data)`       | stream intake  | consume PyCapsule stream            |
|  [07]   | `RecordBatchReader.read_all()`              | consume        | collect all batches as `Table`      |
|  [08]   | `RecordBatchReader.read_next_batch()`       | consume        | pull next `RecordBatch`             |
|  [09]   | `list_array(offsets, values, ...)`          | builder        | variable-length list array          |
|  [10]   | `fixed_size_list_array(values, list_size)`  | builder        | fixed-size list array               |
|  [11]   | `struct_array(arrays, fields, ...)`         | builder        | struct array from component columns |
|  [12]   | `struct_field(values, indices)`             | accessor       | extract nested struct field         |
|  [13]   | `dictionary_dictionary(array)`              | accessor       | extract dictionary values array     |
|  [14]   | `dictionary_indices(array)`                 | accessor       | extract dictionary indices array    |
|  [15]   | `list_flatten(input)`                       | transform      | flatten one level of list nesting   |
|  [16]   | `list_offsets(input, logical)`              | accessor       | extract list offset buffer          |

## [04]-[IMPLEMENTATION_LAW]

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
