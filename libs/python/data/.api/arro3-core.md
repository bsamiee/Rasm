# [PY_DATA_API_ARRO3_CORE]

`arro3.core` supplies a zero-copy Apache Arrow memory model implemented in Rust over `arrow-rs`, exposing typed `Array`, `ChunkedArray`, `RecordBatch`, `Table`, `Schema`, `Field`, `DataType`, `Scalar`, `Buffer`, `ArrayReader`, and `RecordBatchReader` along with eight top-level builder/accessor functions for list, struct, and dictionary layouts. All types export and consume the Arrow PyCapsule interface (`__arrow_c_schema__`, `__arrow_c_array__`, `__arrow_c_stream__`) for lossless zero-copy interchange with `pyarrow`, `nanoarrow`, `polars`, and any PyCapsule-compatible consumer; the `arro3.core.types` protocols (`ArrowSchemaExportable`, `ArrowArrayExportable`, `ArrowStreamExportable`, `ArrayInput`) are the structural discriminators that make every `from_arrow`/builder polymorphic across producer libraries without naming the producer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-core`
- package: `arro3-core`
- owner: `data`
- version: `0.8.1`
- module: `arro3.core`
- asset: native extension (Rust/PyO3) over `arrow-rs`
- license: `MIT OR Apache-2.0`
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

[PUBLIC_TYPE_SCOPE]: interchange protocols (`arro3.core.types`)
- rail: arrow-memory

These structural `Protocol` types are the polymorphic discriminator: every `from_arrow` and builder accepts any object implementing the matching dunder, so `pyarrow`, `nanoarrow`, `polars`, ADBC readers, and arro3 itself all flow through one entrypoint with no producer-specific branch. `ArrowArrayExportable` vs `ArrowStreamExportable` is the load-bearing dispatch axis: the same accessor returns an in-memory `Array` for the former and a streaming `ArrayReader` for the latter.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]    | [ROLE]                                                        |
| :-----: | :----------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `ArrowSchemaExportable`  | structural proto | any object with `__arrow_c_schema__` (Schema/Field/DataType) |
|  [02]   | `ArrowArrayExportable`   | structural proto | any object with `__arrow_c_array__` (Array/RecordBatch)      |
|  [03]   | `ArrowStreamExportable`  | structural proto | any object with `__arrow_c_stream__` (reader/ChunkedArray)   |
|  [04]   | `ArrayInput`             | union alias      | `ArrowArrayExportable`, NumPy ndarray, or Buffer-protocol object accepted by builders/`take` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Array construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Array(obj, /, type=None)`                         | construct       | from `ArrayInput` or Python sequence + `type` |
|  [02]   | `Array.from_arrow(input)`                          | capsule intake  | from `ArrowArrayExportable\|ArrowStreamExportable` |
|  [03]   | `Array.from_arrow_pycapsule(schema_capsule, array_capsule)` | capsule intake | explicit schema + array PyCapsule pair |
|  [04]   | `Array.from_buffer(buffer)`                        | buffer intake   | construct from Buffer-protocol object         |
|  [05]   | `Array.from_numpy(array)`                          | numpy intake    | zero-copy from NumPy ndarray                  |
|  [06]   | `Array.cast(target_type)`                          | type projection | cast to `ArrowSchemaExportable` target type   |
|  [07]   | `Array.slice(offset=0, length=None)`               | window          | zero-copy slice                               |
|  [08]   | `Array.take(indices)`                              | gather          | gather by `ArrayInput` index array            |
|  [09]   | `Array.to_numpy()` / `Array.to_pylist()`           | export          | terminal extraction to NumPy or Python list   |
|  [10]   | `Array.field` / `Array.type`                       | metadata        | `Field` descriptor and `DataType`             |
|  [11]   | `Array.null_count` / `Array.nbytes`                | metadata        | null count and byte size                      |
|  [12]   | `__getitem__(i)` / `__iter__` / `__len__`          | element access  | `Scalar` by index, scalar iteration, length   |
|  [13]   | `__arrow_c_array__` / `__arrow_c_schema__` / `__array__` | export      | PyCapsule + NumPy `__array__` egress          |

[ENTRYPOINT_SCOPE]: ChunkedArray construction and operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [CAPABILITY]                          |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------ |
|  [01]   | `ChunkedArray.from_arrow(input)`                | capsule intake  | from array/stream exportable          |
|  [02]   | `ChunkedArray.cast(target_type)`                | type projection | cast all chunks                       |
|  [03]   | `ChunkedArray.combine_chunks()`                 | consolidation   | flatten to single `Array`             |
|  [04]   | `ChunkedArray.rechunk(*, max_chunksize=None)`   | repartition     | re-chunk with size cap (keyword-only) |
|  [05]   | `ChunkedArray.chunk(i)` / `.chunks`             | access          | chunk by index or all chunks          |
|  [06]   | `ChunkedArray.slice(offset=0, length=None)`     | window          | zero-copy slice across chunks         |
|  [07]   | `ChunkedArray.equals(other)`                    | equality        | value equality vs stream exportable   |
|  [08]   | `ChunkedArray.to_numpy()` / `.to_pylist()`      | export          | export to NumPy or Python list        |
|  [09]   | `ChunkedArray.length()` / `.num_chunks`         | metadata        | element count and chunk count         |
|  [10]   | `ChunkedArray.field` / `.type` / `.null_count` / `.nbytes` | metadata | field, type, null count, byte size |
|  [11]   | `__arrow_c_stream__` / `__getitem__(i)` / `__iter__` | export     | streaming PyCapsule + `Scalar` access |

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
|  [17]   | `RecordBatch.num_rows` / `.num_columns`        | metadata        | row count and column count      |

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
|  [10]   | `Table.add_column/set_column/append_column/remove_column` | mutation | positional column insert/replace/append/drop |
|  [11]   | `Table.with_schema(schema)`           | schema mutation | replace schema (metadata/types)  |
|  [12]   | `Table.rechunk(*, max_chunksize=None)`| repartition     | re-chunk all columns (keyword-only) |
|  [13]   | `Table.combine_chunks()`              | consolidation   | flatten each column to one chunk |
|  [14]   | `Table.column(i\|str)` / `.field(i\|str)` / `.column_names` / `.num_rows` / `.num_columns` / `.shape` / `.chunk_lengths` | access | index-or-name column/field plus shape metadata |
|  [15]   | `Table.to_batches()`                  | export          | emit `RecordBatch` list          |
|  [16]   | `Table.to_reader()`                   | export          | emit `RecordBatchReader` (streaming `__arrow_c_stream__`) |
|  [17]   | `Table.to_struct_array()`             | export          | export as struct `ChunkedArray`  |

[ENTRYPOINT_SCOPE]: schema, field, and type operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY]    | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `Schema.from_arrow(input)`                     | capsule intake    | from `ArrowSchemaExportable`                       |
|  [02]   | `Schema.append/insert/set/remove`              | mutation          | field add/insert-at/replace/drop (returns new)     |
|  [03]   | `Schema.field(i\|str)` / `.get_field_index(name)` / `.get_all_field_indices(name)` | access | field by index-or-name, name->index lookup |
|  [04]   | `Schema.names` / `.types` / `.metadata` / `.metadata_str` | metadata | column names, types, raw and str metadata     |
|  [05]   | `Schema.with_metadata(metadata)` / `.remove_metadata()` / `.empty_table()` | mutation | retag metadata or mint an empty `Table` |
|  [06]   | `Field.with_name/with_type/with_nullable/with_metadata/remove_metadata` | mutation | rename, retype, set nullability, retag (returns new) |
|  [07]   | `Field.name` / `.type` / `.nullable` / `.metadata` / `.metadata_str` | metadata | field descriptor accessors             |
|  [08]   | `DataType` classmethod constructors            | type construction | `null/bool/int8..uint64/float16..float64`, `decimal128/256`, `binary/string/large_*`, `binary_view/string_view`, `date32/64`, `time32/64`, `timestamp(unit,tz=)`, `duration`, `month_day_nano_interval`, `list/large_list/list_view/large_list_view`, `map`, `struct`, `dictionary`, `run_end_encoded` |
|  [09]   | `DataType.is_*` predicates                     | type test         | ~55 classmethod predicates over any `ArrowSchemaExportable` (`is_integer`, `is_nested`, `is_dictionary`, `is_run_end_encoded`, `is_dictionary_key_type`, ...) |
|  [10]   | `DataType.value_type` / `.value_field` / `.fields` / `.bit_width` / `.list_size` / `.time_unit` / `.tz` / `.num_fields` | introspection | nested/temporal/decimal type structure |
|  [11]   | `Scalar.from_arrow(input)`                     | capsule intake    | from `ArrowArrayExportable`                        |
|  [12]   | `Scalar.as_py()` / `.cast(target_type)` / `.is_valid` / `.field` / `.type` | export/test | Python value, cast, validity, field, type |

[ENTRYPOINT_SCOPE]: streaming readers and structural builders
- rail: arrow-memory

The four `dictionary_*`/`list_*` accessor functions are `@overload`-dispatched on input protocol: an `ArrowArrayExportable` argument returns an in-memory `Array`, an `ArrowStreamExportable` argument returns a streaming `ArrayReader` (zero buffering). Builders take Arrow type and null `mask` as keyword-only refinements; `struct_array` requires `fields` as a keyword.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `ArrayReader.from_arrays(field, arrays)` / `.from_stream(data)`    | construction   | build from array iterable or PyCapsule stream |
|  [02]   | `ArrayReader.read_all()`                                           | consume        | collect remaining arrays into a `ChunkedArray`|
|  [03]   | `ArrayReader.read_next_array()` / `__next__` / `__iter__`          | consume        | pull next `Array`; reader is single-pass      |
|  [04]   | `ArrayReader.field` / `.closed`                                    | metadata       | element `Field`; exhaustion guard             |
|  [05]   | `RecordBatchReader.from_batches(schema, batches)` / `.from_stream(data)` | construction | build from batch iterable or PyCapsule stream |
|  [06]   | `RecordBatchReader.read_all()`                                     | consume        | collect remaining batches into a `Table`      |
|  [07]   | `RecordBatchReader.read_next_batch()` / `__next__` / `__iter__`    | consume        | pull next `RecordBatch`; single-pass          |
|  [08]   | `RecordBatchReader.schema` / `.closed`                             | metadata       | result `Schema`; exhaustion guard             |
|  [09]   | `list_array(offsets, values, *, type=None, mask=None)`             | builder        | variable-length (large-)list array            |
|  [10]   | `fixed_size_list_array(values, list_size, *, type=None, mask=None)`| builder        | fixed-size list array                         |
|  [11]   | `struct_array(arrays, *, fields, type=None, mask=None)`            | builder        | struct array; `fields` keyword-only           |
|  [12]   | `struct_field(values, /, indices)`                                | accessor       | chained struct field by `int\|Sequence[int]`  |
|  [13]   | `dictionary_dictionary(array)`                                    | accessor       | dictionary values; `Array`/`ArrayReader` by input proto |
|  [14]   | `dictionary_indices(array)`                                       | accessor       | dictionary indices; `Array`/`ArrayReader` by input proto |
|  [15]   | `list_flatten(input)`                                             | transform      | unnest one list level; `Array`/`ArrayReader` by input proto |
|  [16]   | `list_offsets(input, *, logical=True)`                            | accessor       | list offset buffer; `logical` adjusts for slicing |
|  [17]   | `Buffer(buffer)` / `Buffer.to_bytes()` / `__buffer__`             | raw buffer     | wrap and re-export a Python buffer-protocol object |

## [04]-[IMPLEMENTATION_LAW]

[ARROW_MEMORY_TOPOLOGY]:
- namespace: `arro3.core` exposes exactly 11 public types (`__all__`) plus the 8 top-level builder/accessor functions; the four interchange `Protocol`s live in `arro3.core.types`
- every type implements `__arrow_c_schema__` plus `__arrow_c_array__` (value types) or `__arrow_c_stream__` (containers/readers) for lossless PyCapsule handoff
- `Array` is always contiguous and single-chunk; `ChunkedArray` owns the multi-chunk case; `RecordBatch` is a row-aligned batch over one schema; `Table` is the multi-chunk tabular owner
- `DataType`/`Field`/`Schema`/`Scalar` are immutable value objects: `DataType` is built only through classmethod constructors, and `Schema`/`Field` mutation methods return fresh descriptors
- streaming types (`ArrayReader`, `RecordBatchReader`) are single-pass; `.closed` guards re-use; the `dictionary_*`/`list_*` accessors mirror this by returning a reader for stream input and a materialized array for array input

[LOCAL_ADMISSION]:
- Ingest from any PyCapsule producer via `from_arrow` typed on `ArrowArrayExportable`/`ArrowStreamExportable`/`ArrowSchemaExportable`; the same call absorbs `pyarrow`, `nanoarrow`, `polars`, and ADBC readers with no producer-named branch.
- Emit to downstream consumers via `__arrow_c_array__`/`__arrow_c_stream__`; explicit `to_pylist`/`to_numpy` is terminal extraction only.
- Build composite arrays through `list_array`/`struct_array`/`fixed_size_list_array` with keyword `type=`/`mask=`; never assemble offset/value buffers by hand.

[INTEGRATION_RAILS]:
- ADBC -> arro3: `Cursor.fetch_record_batch()` (`adbc_driver_manager`/`adbc_driver_flightsql`) yields a `pyarrow.RecordBatchReader` whose `__arrow_c_stream__` feeds `RecordBatchReader.from_stream` zero-copy; `read_all()` collapses partitions into one `Table` without materializing Python rows.
- arro3 <-> polars: `polars.from_arrow`/`DataFrame.to_arrow` ride the same capsule, so a `Table` round-trips into a `polars` lazy frame and back as one rail; `pyarrow.table(arro3_table)` is the symmetric escape into `pyarrow` compute.
- arro3 <-> awkward: an `awkward` ragged column exports via `ak.to_arrow` and re-imports through `Array.from_arrow`; `list_array`/`struct_array` reconstruct the nested layout when arro3 owns the wire shape, and `list_flatten`/`struct_field` descend it without copy.
- streaming spine: pair a stream-typed accessor (`list_flatten`, `dictionary_indices`) with `ArrayReader`/`RecordBatchReader` so dictionary-decode, list-unnest, and partition reads stay lazy until a terminal `read_all`/`to_numpy`.

[RAIL_LAW]:
- Package: `arro3-core`
- Owns: zero-copy Arrow memory model, PyCapsule interchange, and the structural export protocols that make ingest producer-agnostic
- Accept: any PyCapsule producer (pyarrow/nanoarrow/polars/ADBC), NumPy, and Python dicts/lists as terminal sources
- Reject: manual offset/buffer construction where builder functions exist; re-importing already-Arrow data through Python lists; producer-named ingest branches where the export protocol already discriminates
