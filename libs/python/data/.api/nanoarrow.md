# [PY_DATA_API_NANOARROW]

`nanoarrow` supplies a lightweight Apache Arrow C Data Interface binding for Python, exposing `Array`, `ArrayStream`, `Schema`, and `TimeUnit` as the primary interaction surface alongside a complete family of schema-factory functions (`int8`..`uint64`, `float16`..`float64`, `timestamp`, `list_`, `struct`, `dictionary`, etc.), C-level primitives (`CArray`, `CArrayStream`, `CSchema`, `CBuffer`), and an `extension` sub-package for canonical and custom extension types. All objects round-trip through the Arrow PyCapsule interface and interoperate with any `__arrow_c_array__` or `__arrow_c_stream__` consumer.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nanoarrow`
- package: `nanoarrow`
- module: `nanoarrow`
- asset: native extension (C/Cython)
- rail: arrow-memory

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: high-level wrapper types
- rail: arrow-memory

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [ROLE]                                       |
| :-----: | :------------ | :-------------- | :------------------------------------------- |
|   [1]   | `Array`       | chunked wrapper | multi-chunk typed array with Python protocol |
|   [2]   | `ArrayStream` | stream wrapper  | consumable Arrow stream with iteration       |
|   [3]   | `Schema`      | schema wrapper  | Arrow schema with parameter accessors        |
|   [4]   | `TimeUnit`    | enum            | `s`, `ms`, `us`, `ns` time-unit vocabulary   |
|   [5]   | `Type`        | enum            | Arrow type identifier vocabulary             |

[PUBLIC_TYPE_SCOPE]: C-level primitives
- rail: arrow-memory

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]   | [ROLE]                                   |
| :-----: | :------------- | :-------------- | :--------------------------------------- |
|   [1]   | `CArray`       | C array holder  | low-level `ArrowArray` C struct wrapper  |
|   [2]   | `CArrayStream` | C stream holder | low-level `ArrowArrayStream` C struct    |
|   [3]   | `CSchema`      | C schema holder | low-level `ArrowSchema` C struct wrapper |
|   [4]   | `CBuffer`      | C buffer holder | typed buffer from Arrow buffer accessor  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema factory functions
- rail: arrow-memory

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------ |
|   [1]   | `int8/16/32/64(nullable)` / `uint8/16/32/64(nullable)` | integer types  | fixed-width integer schemas     |
|   [2]   | `float16/32/64(nullable)`                              | float types    | IEEE floating-point schemas     |
|   [3]   | `bool_(nullable)` / `bool8(nullable)`                  | boolean types  | Arrow boolean and bool8 schemas |
|   [4]   | `timestamp(unit, timezone, nullable)`                  | temporal type  | timestamp schema with tz        |
|   [5]   | `date32(nullable)` / `date64(nullable)`                | temporal type  | calendar date schemas           |
|   [6]   | `time32(unit, nullable)` / `time64(unit, nullable)`    | temporal type  | time-of-day schemas             |
|   [7]   | `duration(unit, nullable)`                             | temporal type  | duration schema                 |
|   [8]   | `string(nullable)` / `large_string(nullable)`          | string types   | UTF-8 and large-UTF-8 schemas   |
|   [9]   | `binary(nullable)` / `large_binary(nullable)`          | binary types   | opaque bytes schemas            |
|  [10]   | `string_view(nullable)` / `binary_view(nullable)`      | view types     | string-view and binary-view     |
|  [11]   | `decimal128(precision, scale, nullable)`               | decimal type   | 128-bit decimal schema          |
|  [12]   | `decimal256(precision, scale, nullable)`               | decimal type   | 256-bit decimal schema          |
|  [13]   | `list_(value_type, nullable)`                          | nested type    | variable-length list schema     |
|  [14]   | `large_list(value_type, nullable)`                     | nested type    | large list schema               |
|  [15]   | `fixed_size_list(value_type, list_size, nullable)`     | nested type    | fixed-size list schema          |
|  [16]   | `struct(fields, nullable)`                             | nested type    | struct schema from field list   |
|  [17]   | `dictionary(index_type, value_type, ...)`              | nested type    | dictionary-encoded schema       |
|  [18]   | `map_(key_type, value_type, keys_sorted, nullable)`    | nested type    | map schema                      |

[ENTRYPOINT_SCOPE]: union and extension schemas
- rail: arrow-memory

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------- |
|   [1]   | `dense_union(fields, type_codes, nullable)`       | union type     | dense union schema             |
|   [2]   | `sparse_union(fields, type_codes, nullable)`      | union type     | sparse union schema            |
|   [3]   | `extension_type(storage_schema, name, meta, ...)` | extension type | custom extension schema        |
|   [4]   | `null(nullable)`                                  | null type      | Arrow null schema              |
|   [5]   | `interval_months(nullable)`                       | interval type  | year-month interval schema     |
|   [6]   | `interval_day_time(nullable)`                     | interval type  | day-time interval schema       |
|   [7]   | `interval_month_day_nano(nullable)`               | interval type  | month-day-nanosecond interval  |
|   [8]   | `fixed_size_binary(byte_width, nullable)`         | binary type    | fixed-size opaque bytes schema |

[ENTRYPOINT_SCOPE]: array and stream construction
- rail: arrow-memory

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]    | [CAPABILITY]                      |
| :-----: | :--------------------------------------------------- | :---------------- | :-------------------------------- |
|   [1]   | `array(obj, schema)`                                 | array factory     | wrap any Arrow-compatible object  |
|   [2]   | `Array(obj, schema, device)`                         | array constructor | multi-chunk wrapper construction  |
|   [3]   | `Array.from_chunks(obj, schema, validate)`           | chunked intake    | build from iterable of chunks     |
|   [4]   | `ArrayStream(obj, schema)`                           | stream factory    | wrap any stream-compatible object |
|   [5]   | `ArrayStream.from_path(obj, *args, **kwargs)`        | IO intake         | open stream from file path        |
|   [6]   | `ArrayStream.from_readable(obj)`                     | IO intake         | open stream from file-like object |
|   [7]   | `ArrayStream.from_url(obj, *args, **kwargs)`         | IO intake         | open stream from URL              |
|   [8]   | `ArrayStream.read_all()`                             | consume           | collect all chunks into `Array`   |
|   [9]   | `ArrayStream.read_next()`                            | consume           | pull next `Array` chunk           |
|  [10]   | `ArrayStream.iter_chunks()`                          | consume           | iterate `Array` chunks            |
|  [11]   | `c_array(obj, schema)`                               | C-level factory   | low-level `CArray` construction   |
|  [12]   | `c_array_from_buffers(schema, length, buffers, ...)` | C-level factory   | construct from raw buffer list    |
|  [13]   | `c_array_stream(obj, schema)`                        | C-level factory   | low-level `CArrayStream` wrap     |
|  [14]   | `c_buffer(obj, schema)`                              | C-level factory   | low-level `CBuffer` wrap          |
|  [15]   | `c_schema(obj)`                                      | C-level factory   | low-level `CSchema` wrap          |
|  [16]   | `schema(obj, **kwargs)`                              | schema factory    | wrap as `Schema`                  |

[ENTRYPOINT_SCOPE]: Array and ArrayStream operations
- rail: arrow-memory

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------ | :------------- | :-------------------------------- |
|   [1]   | `Array.buffer(i)`                                 | buffer access  | access buffer by index            |
|   [2]   | `Array.child(i)` / `Array.iter_children()`        | child access   | access nested array child         |
|   [3]   | `Array.iter_py()`                                 | Python export  | iterate as Python scalars         |
|   [4]   | `Array.iter_scalar()`                             | scalar export  | iterate as `Scalar` objects       |
|   [5]   | `Array.iter_tuples()`                             | tuple export   | iterate struct array as tuples    |
|   [6]   | `Array.to_pylist()`                               | Python export  | collect to Python list            |
|   [7]   | `Array.to_pysequence(handle_nulls)`               | Python export  | collect with null policy          |
|   [8]   | `Array.to_columns_pysequence(handle_nulls)`       | Python export  | struct array to column sequences  |
|   [9]   | `Array.serialize(dst)`                            | serialization  | IPC-serialize to bytes or stream  |
|  [10]   | `ArrayStream.to_pylist()`                         | Python export  | collect stream to Python list     |
|  [11]   | `ArrayStream.to_columns_pysequence(handle_nulls)` | Python export  | stream to column sequences        |
|  [12]   | `nulls_as_sentinel(sentinel)`                     | null policy    | replace nulls with sentinel value |
|  [13]   | `nulls_forbid()`                                  | null policy    | raise on null encounter           |
|  [14]   | `nulls_separate()`                                | null policy    | emit `(mask, values)` tuple       |
|  [15]   | `c_version()`                                     | version query  | nanoarrow C library version       |

## [4]-[IMPLEMENTATION_LAW]

[ARROW_MEMORY_TOPOLOGY]:
- namespace: `nanoarrow` (high-level) and `nanoarrow._array` / `nanoarrow._schema` / `nanoarrow._buffer` (C-level)
- `Array` wraps one or more `CArray` chunks and exposes the PyCapsule interface
- `Schema` wraps `CSchema` and provides parameter accessors (`unit`, `timezone`, `precision`, `scale`, etc.)
- `TimeUnit` enum carries `s`, `ms`, `us`, `ns`; `Type` enum carries Arrow type identifier vocabulary
- `c_array_from_buffers` is the low-level entry for constructing from raw numeric buffers

[LOCAL_ADMISSION]:
- Use schema factory functions to build `Schema` objects; never pass raw format strings directly.
- Consume streams through `ArrayStream.read_all()`, `read_next()`, or `iter_chunks()`; close the stream before discarding.
- Null handling policy is caller-controlled at `to_pysequence` / `to_columns_pysequence` call sites via `handle_nulls` argument.
- Use `c_array_from_buffers` only when the source is already a raw numeric buffer; prefer `array()` for protocol-compatible sources.

[RAIL_LAW]:
- Package: `nanoarrow`
- Owns: Arrow C Data Interface Python binding and schema factory vocabulary
- Accept: any `__arrow_c_array__` / `__arrow_c_stream__` compatible input plus raw buffers
- Reject: manual ArrowSchema format-string construction, re-wrapping already-Arrow objects through Python lists
