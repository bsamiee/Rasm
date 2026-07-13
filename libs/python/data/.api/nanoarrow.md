# [PY_DATA_API_NANOARROW]

`nanoarrow` supplies a lightweight Apache Arrow C Data Interface binding for Python, exposing `Array`, `ArrayStream`, `Schema`, `Type`, and `TimeUnit` as the primary interaction surface alongside a complete family of schema-factory functions (`int8`..`uint64`, `float16`..`float64`, `timestamp`, `list_`, `struct`, `dictionary`, etc.), the C-level factories (`c_array`, `c_array_stream`, `c_schema`, `c_buffer`, `c_array_from_buffers`) that mint the low-level `CArray`/`CArrayStream`/`CSchema`/`CBuffer` holders housed in the `nanoarrow._array`/`._schema`/`._buffer` modules, and the `extension`/`extension_canonical` packages for canonical (`bool8`) and custom extension types. All objects round-trip through the Arrow PyCapsule interface (`__arrow_c_array__`/`__arrow_c_stream__`/`__arrow_c_schema__`) and interoperate with any compatible producer or consumer without copying buffers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nanoarrow`
- package: `nanoarrow`
- owner: `data`
- module: `nanoarrow`
- version: `0.8.0`
- license: Apache-2.0
- asset: native extension (C core + Cython `_array`/`_schema`/`_buffer`/`_device` modules); pure-Python high-level layer over them
- rail: arrow-memory

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: high-level wrapper types
- rail: arrow-memory

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [ROLE]                                       |
| :-----: | :------------ | :-------------- | :------------------------------------------- |
|  [01]   | `Array`       | chunked wrapper | multi-chunk typed array with Python protocol |
|  [02]   | `ArrayStream` | stream wrapper  | consumable Arrow stream with iteration       |
|  [03]   | `Schema`      | schema wrapper  | Arrow schema with parameter accessors        |
|  [04]   | `TimeUnit`    | enum            | `s`, `ms`, `us`, `ns` time-unit vocabulary   |
|  [05]   | `Type`        | enum            | Arrow type identifier vocabulary             |

[PUBLIC_TYPE_SCOPE]: C-level primitives
- rail: arrow-memory

The C-level holders are NOT top-level `nanoarrow` names; they live in the Cython submodules and are minted by the top-level `c_*` factories (`c_array`/`c_array_stream`/`c_schema`/`c_buffer`/`c_array_from_buffers`). Reach them by return value of a factory, not by `nanoarrow.CArray`; the device holders import from `nanoarrow.device` though the Cython holder lives in `_array`.

| [INDEX] | [SYMBOL]                       | [MODULE]                  | [TYPE_FAMILY]   | [ROLE]                                              |
| :-----: | :----------------------------- | :------------------------ | :-------------- | :-------------------------------------------------- |
|  [01]   | `CArray`                       | `nanoarrow._array`        | C array holder  | low-level `ArrowArray` C struct wrapper             |
|  [02]   | `CArrayStream`                 | `nanoarrow._array_stream` | C stream holder | low-level `ArrowArrayStream` C struct               |
|  [03]   | `CSchema`                      | `nanoarrow._schema`       | C schema holder | low-level `ArrowSchema` C struct wrapper            |
|  [04]   | `CBuffer`                      | `nanoarrow._buffer`       | C buffer holder | typed buffer from Arrow buffer accessor             |
|  [05]   | `CArrayBuilder` / `CArrayView` | `nanoarrow._array`        | C builder/view  | incremental build and zero-copy view of `CArray`    |
|  [06]   | `CDeviceArray` / `DeviceType`  | `nanoarrow.device`        | device holder   | C Device Data Interface holder and device-type enum |

- [06]-[DEVICE]: the `nanoarrow.device` submodule mints device arrays via `c_device_array(obj, schema=None)` and carries `Device`, `DeviceType`, `DEVICE_CPU`, `cpu`, `resolve`; device access is array-level, with no device-array-stream constructor.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema factory functions
- rail: arrow-memory

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `int8/16/32/64(nullable)` / `uint8/16/32/64(nullable)` | integer types  | fixed-width integer schemas     |
|  [02]   | `float16/32/64(nullable)`                              | float types    | IEEE floating-point schemas     |
|  [03]   | `bool_(nullable)` / `bool8(nullable)`                  | boolean types  | Arrow boolean and bool8 schemas |
|  [04]   | `timestamp(unit, timezone, nullable)`                  | temporal type  | timestamp schema with tz        |
|  [05]   | `date32(nullable)` / `date64(nullable)`                | temporal type  | calendar date schemas           |
|  [06]   | `time32(unit, nullable)` / `time64(unit, nullable)`    | temporal type  | time-of-day schemas             |
|  [07]   | `duration(unit, nullable)`                             | temporal type  | duration schema                 |
|  [08]   | `string(nullable)` / `large_string(nullable)`          | string types   | UTF-8 and large-UTF-8 schemas   |
|  [09]   | `binary(nullable)` / `large_binary(nullable)`          | binary types   | opaque bytes schemas            |
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

`extension_type(storage_schema, extension_name, extension_metadata=None, nullable=True)` mints a custom extension schema; the union factories take `(fields, type_codes, nullable)` and the interval/null factories take `(nullable)`.

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `dense_union(fields, type_codes, nullable)`  | union type     | dense union schema                           |
|  [02]   | `sparse_union(fields, type_codes, nullable)` | union type     | sparse union schema                          |
|  [03]   | `extension_type(...)`                        | extension type | custom extension schema with name + metadata |
|  [04]   | `null(nullable)`                             | null type      | Arrow null schema                            |
|  [05]   | `interval_months(nullable)`                  | interval type  | year-month interval schema                   |
|  [06]   | `interval_day_time(nullable)`                | interval type  | day-time interval schema                     |
|  [07]   | `interval_month_day_nano(nullable)`          | interval type  | month-day-nanosecond interval                |
|  [08]   | `fixed_size_binary(byte_width, nullable)`    | binary type    | fixed-size opaque bytes schema               |

[ENTRYPOINT_SCOPE]: array and stream construction
- rail: arrow-memory

`array`/`array_stream` wrap any Arrow-compatible object; the `c_*` factories mint the low-level C holders. `c_array_from_buffers(schema, length, buffers, null_count=-1, offset=0, children=(), validation_level=None, move=False, device=None)` is the full raw-buffer constructor returning a `CArray`.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]     | [CAPABILITY]                                         |
| :-----: | :--------------------------------------------------- | :----------------- | :--------------------------------------------------- |
|  [01]   | `array(obj, schema=None) -> Array`                   | array factory      | wrap any Arrow-compatible object                     |
|  [02]   | `array_stream(obj, schema=None) -> ArrayStream`      | stream factory     | wrap any stream-compatible object                    |
|  [03]   | `Array(obj, schema=None, device=None)`               | array constructor  | multi-chunk wrapper construction                     |
|  [04]   | `Array.from_chunks(obj, schema=None, validate=True)` | chunked intake     | build from iterable of chunks                        |
|  [05]   | `ArrayStream(obj, schema=None)`                      | stream constructor | construct stream wrapper                             |
|  [06]   | `ArrayStream.from_path(obj, *args, **kwargs)`        | IO intake          | open IPC stream from file path                       |
|  [07]   | `ArrayStream.from_readable(obj)`                     | IO intake          | open IPC stream from file-like object                |
|  [08]   | `ArrayStream.from_url(obj, *args, **kwargs)`         | IO intake          | open IPC stream from URL                             |
|  [09]   | `c_array(obj, schema=None) -> CArray`                | C-level factory    | low-level `CArray` construction                      |
|  [10]   | `c_array_from_buffers(...) -> CArray`                | C-level factory    | construct from raw buffer list with validation level |
|  [11]   | `c_array_stream(obj, schema=None) -> CArrayStream`   | C-level factory    | low-level `CArrayStream` wrap                        |
|  [12]   | `c_buffer(obj, schema=None) -> CBuffer`              | C-level factory    | low-level `CBuffer` wrap                             |
|  [13]   | `c_schema(obj) -> CSchema`                           | C-level factory    | low-level `CSchema` wrap                             |
|  [14]   | `schema(obj, **kwargs) -> Schema`                    | schema factory     | wrap as high-level `Schema`                          |

[ENTRYPOINT_SCOPE]: Array operations
- rail: arrow-memory

Every surface below is an `Array` method.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `schema` / `device` / `offset`                | metadata       | active `Schema`, device handle, logical offset                     |
|  [02]   | `inspect()`                                   | introspection  | print buffer/child/chunk layout summary                            |
|  [03]   | `n_chunks` / `chunk(i)` / `iter_chunks()`     | chunk access   | chunk count, single chunk, chunk iterator                          |
|  [04]   | `iter_chunk_views()`                          | chunk access   | iterate zero-copy `CArrayView` per chunk                           |
|  [05]   | `n_buffers` / `buffer(i)` / `buffers`         | buffer access  | buffer count, buffer by index, all buffers                         |
|  [06]   | `n_children` / `child(i)` / `iter_children()` | child access   | nested child count, child by index, child iterator                 |
|  [07]   | `iter_py()`                                   | Python export  | iterate as Python scalars                                          |
|  [08]   | `iter_scalar()`                               | scalar export  | iterate as `Scalar` objects                                        |
|  [09]   | `iter_tuples()`                               | tuple export   | iterate struct array as tuples                                     |
|  [10]   | `to_pylist()`                                 | Python export  | collect to Python list                                             |
|  [11]   | `to_pysequence(*, handle_nulls=None)`         | Python export  | collect with null policy (NumPy/array-protocol sequence)           |
|  [12]   | `to_columns_pysequence(*, handle_nulls=None)` | Python export  | struct array to per-column sequences                               |
|  [13]   | `to_string()`                                 | display        | repr-style string of the array                                     |
|  [14]   | `serialize(dst=None) -> bytes \| None`        | serialization  | IPC-serialize; returns `bytes`, or writes `dst` and returns `None` |

[ENTRYPOINT_SCOPE]: ArrayStream operations
- rail: arrow-memory

Every surface below is an `ArrayStream` method.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------ | :--------------- | :----------------------------------------------- |
|  [01]   | `read_all()` / `read_next()`                                  | consume          | collect all chunks; pull the next `Array` chunk  |
|  [02]   | `iter_chunks()` / `iter_py()` / `iter_tuples()`               | consume          | iterate chunks, Python scalars, or struct tuples |
|  [03]   | `to_pylist()` / `to_columns_pysequence(*, handle_nulls=None)` | Python export    | collect stream to list or column sequences       |
|  [04]   | `schema` / `close()`                                          | metadata/release | stream schema; release the underlying C stream   |

[ENTRYPOINT_SCOPE]: null policies and version
- rail: arrow-memory

The module-level `nulls_*` policies feed the `handle_nulls=` argument at any `to_*`/`iter_*` call site; `c_version()` reports the C library version.

| [INDEX] | [SURFACE]                     | [ENTRY_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------------------- | :------------- | :-------------------------------- |
|  [01]   | `nulls_as_sentinel(sentinel)` | null policy    | replace nulls with sentinel value |
|  [02]   | `nulls_forbid()`              | null policy    | raise on null encounter           |
|  [03]   | `nulls_separate()`            | null policy    | emit `(mask, values)` tuple       |
|  [04]   | `c_version()`                 | version query  | nanoarrow C library version       |

[ENTRYPOINT_SCOPE]: schema accessors, extension registry, and iteration
- rail: arrow-memory

`Schema` exposes every Arrow type parameter directly so a consumer reads `unit`/`timezone`/`precision`/`scale`/`list_size`/`byte_width`/`index_type`/`value_type`/`key_type`/`keys_sorted`/`type_codes`/`fields`/`field(i)`/`metadata`/`extension` without re-parsing format strings. The `extension` package owns the `Extension` base and the register/resolve lifecycle; `extension_canonical` carries the canonical `bool8` extension. `iterator`/`visitor` own the array-view traversal machinery the high-level `iter_*`/`to_*` methods dispatch into.

Every surface in the first sub-table is a `Schema` accessor.

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `type` / `params` / `field(i)` / `fields`     | schema query   | type id, parameter dict, child field access           |
|  [02]   | `unit` / `timezone` / `precision` / `scale`   | schema query   | temporal and decimal scalar parameters                |
|  [03]   | `byte_width` / `list_size`                    | schema query   | fixed-width and list-size parameters                  |
|  [04]   | `index_type` / `value_type` / `key_type`      | schema query   | dictionary and map key/value parameters               |
|  [05]   | `keys_sorted` / `dictionary_ordered`          | schema query   | map sort flag and dictionary order flag               |
|  [06]   | `nullable` / `name` / `metadata` / `n_fields` | schema query   | nullability, field name, custom metadata, field count |
|  [07]   | `extension` / `serialize()`                   | extension/IPC  | extension descriptor; serialize schema to PyCapsule   |

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `extension.register(extension_cls)` / `register_extension(...)` | extension reg  | register a custom `Extension` subclass               |
|  [02]   | `extension.resolve_extension(schema)`                           | extension reg  | look up a registered extension                       |
|  [03]   | `unregister_extension(name)`                                    | extension reg  | remove a registered extension                        |
|  [04]   | `extension_canonical.bool8()` / `Bool8Extension`                | canonical ext  | canonical Arrow `bool8` extension type and converter |
|  [05]   | `iterator.iter_py` / `iter_tuples` / `iter_array_views`         | traversal      | functional iterators over any stream source          |
|  [06]   | `visitor.ArrayViewVisitor` / `ArrayViewVisitable`               | traversal      | subclassable visitor base for custom column reducers |

## [04]-[IMPLEMENTATION_LAW]

[ARROW_MEMORY_TOPOLOGY]:
- namespace: `nanoarrow` (high-level `Array`/`ArrayStream`/`Schema`/`Type`/`TimeUnit`, factories, `extension`/`extension_canonical`/`iterator`/`visitor`) and the Cython `nanoarrow._array` / `._array_stream` / `._schema` / `._buffer` / `._device` modules (C-level `CArray`/`CArrayStream`/`CSchema`/`CBuffer`/`CArrayView`/`CDeviceArray`)
- `Array` wraps one or more `CArray` chunks (`n_chunks`/`chunk(i)`) and exposes the PyCapsule interface; `iter_chunk_views()` yields zero-copy `CArrayView` per chunk for buffer-level reads
- `Schema` wraps `CSchema` and exposes every Arrow type parameter (`unit`, `timezone`, `precision`, `scale`, `list_size`, `byte_width`, `index_type`/`value_type`/`key_type`, `type_codes`, `fields`/`field(i)`, `metadata`, `extension`)
- `TimeUnit` enum carries `s`, `ms`, `us`, `ns`; `Type` enum carries the full Arrow type vocabulary including `MAP`, `RUN_END_ENCODED`, `DENSE_UNION`/`SPARSE_UNION`, `STRING_VIEW`/`BINARY_VIEW`, the interval kinds, and `EXTENSION`
- `c_array_from_buffers(schema, length, buffers, ...)` is the low-level zero-copy entry for constructing a `CArray` directly from raw numeric buffers with a chosen `validation_level`

[LOCAL_ADMISSION]:
- Use schema factory functions to build `Schema` objects; never hand-build an `ArrowSchema` format string.
- Consume streams through `ArrayStream.read_all()`, `read_next()`, or `iter_chunks()`; `close()` the stream before discarding (it owns a C resource).
- Null handling policy is caller-controlled at `to_pysequence` / `to_columns_pysequence` / `iter_*` call sites via the `handle_nulls=` argument fed by `nulls_as_sentinel`/`nulls_forbid`/`nulls_separate`.
- Use `c_array_from_buffers` only when the source is already a raw numeric buffer; prefer `array()` for protocol-compatible sources.

[STACK]:
- arrow-spine: `nanoarrow` is the zero-copy Arrow C-data spine for the data rail — it is the cheapest producer/consumer of the `__arrow_c_array__`/`__arrow_c_stream__` capsules that `narwhals.from_arrow`, `pyarrow`, and Polars all accept, so a `nanoarrow.Array` flows into the dataframe-agnostic owner without a Python-list round-trip.
- buffer-in / dataframe-out: build a `CArray` from raw numeric/NumPy buffers via `c_array_from_buffers(schema, length, [validity, data], ...)`, lift to `Array`, and hand the capsule straight to `narwhals.from_arrow(..., backend=)` or a Polars frame — the validity bitmap, the offset, and the dtype travel in the schema, never re-derived downstream.
- struct-decode stack: an IPC byte stream opens through `ArrayStream.from_readable`/`from_path`/`from_url`, decodes struct rows via `iter_tuples()`/`to_columns_pysequence(handle_nulls=nulls_separate())`, and the per-column `(mask, values)` tuples feed a `msgspec.Struct` decode or a NumPy column build — the Arrow null mask is the single source of nullability, never re-scanned.
- extension stack: register a domain `Extension` subclass through `extension.register` so the storage schema plus extension name/metadata round-trip end-to-end; canonical `bool8` is consumed via `extension_canonical`, not re-encoded as `int8`.

[RAIL_LAW]:
- Package: `nanoarrow`
- Owns: Arrow C Data Interface Python binding, schema factory vocabulary, zero-copy buffer construction, IPC stream IO, and the extension-type registry
- Accept: any `__arrow_c_array__` / `__arrow_c_stream__` / `__arrow_c_schema__` compatible input plus raw numeric buffers
- Reject: manual ArrowSchema format-string construction; re-wrapping already-Arrow objects through Python lists; a Python-list hop where a capsule passes straight to a dataframe backend; treating `CArray`/`CSchema`/`CBuffer` as top-level `nanoarrow` names rather than factory return values
