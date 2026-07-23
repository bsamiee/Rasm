# [PY_DATA_API_ARRO3_CORE]

`arro3.core` owns a zero-copy Apache Arrow memory model in Rust over `arrow-rs`: arrays, chunked columns, record batches, tables, and schema/type value objects, each exporting and consuming the Arrow PyCapsule interface (`__arrow_c_schema__`/`__arrow_c_array__`/`__arrow_c_stream__`) for lossless interchange. Structural `arro3.core.types` protocols are the polymorphic discriminator: every `from_arrow` and builder dispatches on the matching dunder, so `pyarrow`, `nanoarrow`, `polars`, and ADBC readers flow through one entrypoint with no producer-named branch.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arro3-core`
- package: `arro3-core` (MIT OR Apache-2.0)
- owner: `data`
- module: `arro3.core`
- asset: native extension (Rust/PyO3) over `arrow-rs`
- rail: arrow-memory

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container types

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]     | [CAPABILITY]                             |
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

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [CAPABILITY]                     |
| :-----: | :--------- | :-------------- | :------------------------------- |
|  [01]   | `Schema`   | schema value    | ordered field-to-type mapping    |
|  [02]   | `Field`    | field value     | named typed nullable field       |
|  [03]   | `DataType` | type descriptor | Arrow logical type with metadata |

[PUBLIC_TYPE_SCOPE]: interchange protocols (`arro3.core.types`)

`ArrowArrayExportable` vs `ArrowStreamExportable` is the dispatch axis every accessor folds through: array input returns an in-memory `Array`, stream input a lazy `ArrayReader`.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]    | [CAPABILITY]                                                 |
| :-----: | :---------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `ArrowSchemaExportable` | structural proto | any object with `__arrow_c_schema__` (Schema/Field/DataType) |
|  [02]   | `ArrowArrayExportable`  | structural proto | any object with `__arrow_c_array__` (Array/RecordBatch)      |
|  [03]   | `ArrowStreamExportable` | structural proto | any object with `__arrow_c_stream__` (reader/ChunkedArray)   |
|  [04]   | `ArrayInput`            | union alias      | `ArrowArrayExportable`/ndarray/Buffer for builders/`take`    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Array construction and operations

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `Array(obj, /, type=None)`                                  | ctor     | from `ArrayInput` or Python sequence + `type`             |
|  [02]   | `Array.from_arrow(input)`                                   | factory  | intake from `ArrowArrayExportable\|ArrowStreamExportable` |
|  [03]   | `Array.from_arrow_pycapsule(schema_capsule, array_capsule)` | factory  | explicit schema + array PyCapsule pair                    |
|  [04]   | `Array.from_buffer(buffer)`                                 | factory  | from Buffer-protocol object                               |
|  [05]   | `Array.from_numpy(array)`                                   | factory  | zero-copy from NumPy ndarray                              |
|  [06]   | `Array.cast(target_type)`                                   | instance | cast to `ArrowSchemaExportable` target type               |
|  [07]   | `Array.slice(offset=0, length=None)`                        | instance | zero-copy window slice                                    |
|  [08]   | `Array.take(indices)`                                       | instance | gather by `ArrayInput` index array                        |
|  [09]   | `Array.to_numpy()` / `.to_pylist()`                         | instance | terminal extraction to NumPy or Python list               |
|  [10]   | `Array.field` / `.type`                                     | property | `Field` descriptor and `DataType`                         |
|  [11]   | `Array.null_count` / `.nbytes`                              | property | null count and byte size                                  |
|  [12]   | `__getitem__(i)` / `__len__`                                | operator | `Scalar` by index; sequence-protocol iteration and length |
|  [13]   | `__arrow_c_array__` / `__arrow_c_schema__` / `__array__`    | instance | PyCapsule + NumPy `__array__` egress                      |

[ENTRYPOINT_SCOPE]: ChunkedArray construction and operations

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `ChunkedArray.from_arrow(input)`                           | factory  | capsule intake from array/stream exportable |
|  [02]   | `ChunkedArray.cast(target_type)`                           | instance | cast all chunks                             |
|  [03]   | `ChunkedArray.combine_chunks()`                            | instance | flatten to single `Array`                   |
|  [04]   | `ChunkedArray.rechunk(*, max_chunksize=None)`              | instance | re-chunk with size cap (keyword-only)       |
|  [05]   | `ChunkedArray.chunk(i)` / `.chunks`                        | instance | chunk by index or all chunks                |
|  [06]   | `ChunkedArray.slice(offset=0, length=None)`                | instance | zero-copy slice across chunks               |
|  [07]   | `ChunkedArray.equals(other)`                               | instance | value equality vs stream exportable         |
|  [08]   | `ChunkedArray.to_numpy()` / `.to_pylist()`                 | instance | export to NumPy or Python list              |
|  [09]   | `ChunkedArray.length()` / `.num_chunks`                    | property | element count and chunk count               |
|  [10]   | `ChunkedArray.field` / `.type` / `.null_count` / `.nbytes` | property | field, type, null count, byte size          |
|  [11]   | `__arrow_c_stream__` / `__getitem__(i)`                    | operator | streaming PyCapsule + `Scalar` by index     |

[ENTRYPOINT_SCOPE]: RecordBatch construction and operations

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :-------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `RecordBatch.from_arrays(arrays, ...)`        | factory  | build from column array list    |
|  [02]   | `RecordBatch.from_pydict(mapping, ...)`       | factory  | build from `{name: array}` dict |
|  [03]   | `RecordBatch.from_struct_array(struct_array)` | factory  | build from struct array         |
|  [04]   | `RecordBatch.from_arrow(input)`               | factory  | PyCapsule batch import          |
|  [05]   | `RecordBatch.column(i)` / `.columns`          | instance | column by index or all          |
|  [06]   | `RecordBatch.field(i)` / `.schema`            | instance | field or schema descriptor      |
|  [07]   | `RecordBatch.select(columns)`                 | instance | select named or indexed columns |
|  [08]   | `RecordBatch.add_column(i, field, column)`    | instance | insert column at position       |
|  [09]   | `RecordBatch.set_column(i, field, column)`    | instance | replace column at position      |
|  [10]   | `RecordBatch.remove_column(i)`                | instance | remove column by index          |
|  [11]   | `RecordBatch.append_column(field, column)`    | instance | append column at end            |
|  [12]   | `RecordBatch.slice(offset, length)`           | instance | zero-copy row slice             |
|  [13]   | `RecordBatch.take(indices)`                   | instance | gather rows by index            |
|  [14]   | `RecordBatch.with_schema(schema)`             | instance | replace schema metadata         |
|  [15]   | `RecordBatch.to_struct_array()`               | instance | export as struct `Array`        |
|  [16]   | `RecordBatch.equals(other)`                   | instance | batch value equality            |
|  [17]   | `RecordBatch.num_rows` / `.num_columns`       | property | row count and column count      |

[ENTRYPOINT_SCOPE]: Table construction and operations

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `Table.from_arrays(arrays, ...)`                          | factory  | build from column list                       |
|  [02]   | `Table.from_pydict(mapping, ...)`                         | factory  | build from `{name: array}` dict              |
|  [03]   | `Table.from_batches(batches, schema)`                     | factory  | build from record-batch list                 |
|  [04]   | `Table.from_arrow(input)`                                 | factory  | PyCapsule table import                       |
|  [05]   | `Table.column(i)` / `.columns`                            | instance | column by index or all columns               |
|  [06]   | `Table.select(columns)`                                   | instance | select named or indexed columns              |
|  [07]   | `Table.drop_columns(columns)`                             | instance | remove named columns                         |
|  [08]   | `Table.rename_columns(names)`                             | instance | rename all columns                           |
|  [09]   | `Table.slice(offset, length)`                             | instance | zero-copy row slice                          |
|  [10]   | `Table.add_column/set_column/append_column/remove_column` | instance | positional column insert/replace/append/drop |
|  [11]   | `Table.with_schema(schema)`                               | instance | replace schema (metadata/types)              |
|  [12]   | `Table.rechunk(*, max_chunksize=None)`                    | instance | re-chunk all columns (keyword-only)          |
|  [13]   | `Table.combine_chunks()`                                  | instance | flatten each column to one chunk             |
|  [14]   | `Table.column(i\|str)` / `.field(i\|str)`                 | instance | column or field by index or name             |
|  [15]   | `.column_names` / `.num_rows` / `.num_columns`            | property | column names, row and column counts          |
|  [16]   | `.shape` / `.chunk_lengths`                               | property | shape tuple and per-chunk lengths            |
|  [17]   | `Table.to_batches()`                                      | instance | emit `RecordBatch` list                      |
|  [18]   | `Table.to_reader()`                                       | instance | emit streaming `RecordBatchReader`           |
|  [19]   | `Table.to_struct_array()`                                 | instance | export as struct `ChunkedArray`              |

[ENTRYPOINT_SCOPE]: schema, field, and scalar operations

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Schema.from_arrow(input)`                                                 | factory  | from `ArrowSchemaExportable`              |
|  [02]   | `Schema.append/insert/set/remove`                                          | instance | field add/insert/replace/drop             |
|  [03]   | `Schema.field(i\|str)`                                                     | instance | field by index or name                    |
|  [04]   | `Schema.get_field_index(name)` / `.get_all_field_indices(name)`            | instance | name to first or all matching indices     |
|  [05]   | `Schema.names` / `.types` / `.metadata` / `.metadata_str`                  | property | column names, types, raw and str metadata |
|  [06]   | `Schema.with_metadata` / `.remove_metadata()` / `.empty_table()`           | instance | retag metadata or mint an empty `Table`   |
|  [07]   | `Field.with_name/with_type/with_nullable/with_metadata/remove_metadata`    | instance | rename, retype, set nullable, retag       |
|  [08]   | `Field.name` / `.type` / `.nullable` / `.metadata` / `.metadata_str`       | property | field descriptor accessors                |
|  [09]   | `Scalar.from_arrow(input)`                                                 | factory  | from `ArrowArrayExportable`               |
|  [10]   | `Scalar.as_py()` / `.cast(target_type)` / `.is_valid` / `.field` / `.type` | instance | Python value, cast, validity, field, type |

`DataType` builds only through classmethod constructors; predicates and introspection accessors operate over any `ArrowSchemaExportable`.
- [01]-[CONSTRUCTORS]: `null/bool/int8..uint64/float16..float64`, `decimal128/256`, `binary/string/large_*`, `binary_view/string_view`, `date32/64`, `time32/64`, `timestamp(unit,tz=)`, `duration`, `month_day_nano_interval`, `list/large_list/list_view/large_list_view`, `map`, `struct`, `dictionary`, `run_end_encoded`.
- [02]-[PREDICATES]: `is_*` classmethods (`is_integer`, `is_nested`, `is_dictionary`, `is_run_end_encoded`, `is_dictionary_key_type`, ...).
- [03]-[INTROSPECTION]: `.value_type` / `.value_field` / `.fields` / `.bit_width` / `.list_size` / `.time_unit` / `.tz` / `.num_fields` expose nested/temporal/decimal type structure.

[ENTRYPOINT_SCOPE]: streaming readers and structural builders

`dictionary_*`/`list_*` accessors `@overload`-dispatch on input protocol: array input returns an `Array`, stream input a lazy `ArrayReader` (zero buffering). Builders take Arrow `type=` and null `mask=` as keyword-only; `struct_array` requires keyword `fields`.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------------- |
|  [01]   | `ArrayReader.from_arrays(field, arrays)` / `.from_stream(data)`     | factory  | from array iterable or capsule stream        |
|  [02]   | `ArrayReader.read_all()`                                            | instance | collect arrays into a `ChunkedArray`         |
|  [03]   | `ArrayReader.read_next_array()` / `__next__` / `__iter__`           | instance | pull next `Array`; reader is single-pass     |
|  [04]   | `ArrayReader.field` / `.closed`                                     | property | element `Field`; exhaustion guard            |
|  [05]   | `RecordBatchReader.from_batches(schema, batches)`                   | factory  | from batch iterable                          |
|  [06]   | `RecordBatchReader.from_stream(data)`                               | factory  | from PyCapsule stream                        |
|  [07]   | `RecordBatchReader.read_all()`                                      | instance | collect remaining batches into a `Table`     |
|  [08]   | `RecordBatchReader.read_next_batch()` / `__next__` / `__iter__`     | instance | pull next `RecordBatch`; single-pass         |
|  [09]   | `RecordBatchReader.schema` / `.closed`                              | property | result `Schema`; exhaustion guard            |
|  [10]   | `list_array(offsets, values, *, type=None, mask=None)`              | static   | variable-length (large-)list array           |
|  [11]   | `fixed_size_list_array(values, list_size, *, type=None, mask=None)` | static   | fixed-size list array                        |
|  [12]   | `struct_array(arrays, *, fields, type=None, mask=None)`             | static   | struct array; `fields` keyword-only          |
|  [13]   | `struct_field(values, /, indices)`                                  | static   | chained struct field by `int\|Sequence[int]` |
|  [14]   | `dictionary_dictionary(array)`                                      | static   | dictionary values (overload-dispatched)      |
|  [15]   | `dictionary_indices(array)`                                         | static   | dictionary indices (overload-dispatched)     |
|  [16]   | `list_flatten(input)`                                               | static   | unnest one list level (overload-dispatched)  |
|  [17]   | `list_offsets(input, *, logical=True)`                              | static   | list offset buffer; `logical` slice-adjust   |
|  [18]   | `Buffer(buffer)` / `Buffer.to_bytes()` / `__buffer__`               | ctor     | wrap and re-export a buffer-protocol object  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `arro3.core` exports the container types, value types, and top-level builder/accessor functions; the four interchange `Protocol`s live in `arro3.core.types`
- every type implements `__arrow_c_schema__` with `__arrow_c_array__` (value types) or `__arrow_c_stream__` (containers/readers) for lossless PyCapsule handoff
- `Array` is contiguous single-chunk; `ChunkedArray` owns the multi-chunk case; `RecordBatch` is a row-aligned batch over one schema; `Table` is the multi-chunk tabular owner
- `DataType`/`Field`/`Schema`/`Scalar` are immutable value objects: `DataType` builds only through classmethod constructors, and `Schema`/`Field` mutators return fresh descriptors
- `ArrayReader`/`RecordBatchReader` are single-pass with `.closed` guarding reuse; the `dictionary_*`/`list_*` accessors mirror this, returning a reader for stream input and a materialized array for array input

[STACKING]:
- `arro3-compute`(`.api/arro3-compute.md`): arro3 `Array`/`ChunkedArray`/`ArrayReader` feed its kernels directly through the shared `arro3.core.types` protocols, and a stream-typed kernel returns a lazy `ArrayReader` so decode/aggregate stays lazy
- `arro3-io`(`.api/arro3-io.md`): its readers emit a lazy `arro3.core.RecordBatchReader` and its writers accept any capsule producer, making arro3 the codec-rail carrier
- `adbc-driver-manager`(`.api/adbc-driver-manager.md`): `Cursor.fetch_record_batch()` yields a reader whose `__arrow_c_stream__` feeds `RecordBatchReader.from_stream`, and `read_all()` collapses partitions into one `Table` with no Python-row materialization
- `polars`(`.api/polars.md`): `polars.from_arrow`/`DataFrame.to_arrow` ride the same capsule, round-tripping a `Table` through a polars lazy frame and back as one rail
- `pyarrow`(`.api/pyarrow.md`): `pyarrow.table(arro3_table)` and `Array.from_arrow(pa_array)` cross the capsule symmetrically, escaping into `pyarrow` compute and back
- `awkward`(`.api/awkward.md`): a ragged column exports via `ak.to_arrow` and re-imports through `Array.from_arrow`; `list_array`/`struct_array` rebuild the nested layout, `list_flatten`/`struct_field` descend it without copy
- data branch: pair a stream-typed accessor (`list_flatten`, `dictionary_indices`) with `ArrayReader`/`RecordBatchReader` so dictionary-decode, list-unnest, and partition reads stay lazy until a terminal `read_all`/`to_numpy`

[LOCAL_ADMISSION]:
- Ingest any PyCapsule producer via `from_arrow` typed on `ArrowArrayExportable`/`ArrowStreamExportable`/`ArrowSchemaExportable`; one call absorbs `pyarrow`, `nanoarrow`, `polars`, and ADBC readers with no producer-named branch.
- Emit via `__arrow_c_array__`/`__arrow_c_stream__`; `to_pylist`/`to_numpy` is terminal extraction only.
- Build composite arrays through `list_array`/`struct_array`/`fixed_size_list_array` with keyword `type=`/`mask=`; never assemble offset/value buffers by hand.

[RAIL_LAW]:
- Package: `arro3-core`
- Owns: zero-copy Arrow memory model, PyCapsule interchange, and the structural export protocols that make ingest producer-agnostic
- Accept: any PyCapsule producer (pyarrow/nanoarrow/polars/ADBC), NumPy, and Python dicts/lists as terminal sources
- Reject: manual offset/buffer construction where a builder exists; re-importing already-Arrow data through Python lists; producer-named ingest branches where the export protocol already discriminates
