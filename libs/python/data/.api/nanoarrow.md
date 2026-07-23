# [PY_DATA_API_NANOARROW]

`nanoarrow` binds the Apache Arrow C Data Interface into Python: high-level `Array`/`ArrayStream`/`Schema` wrappers, a schema-factory vocabulary, and raw-buffer `CArray` construction, every object crossing the Arrow PyCapsule interface (`__arrow_c_array__`/`__arrow_c_stream__`/`__arrow_c_schema__`) so any compatible producer or consumer interoperates without buffer copies. It is the data rail's zero-copy capsule spine — the cheapest producer and consumer of the interchange the dataframe and IPC owners compose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nanoarrow`
- package: `nanoarrow` (`Apache-2.0`)
- owner: `data`
- module: `nanoarrow`
- asset: native extension (C core + Cython `_array`/`_schema`/`_buffer`/`_device` modules) under a pure-Python high-level layer
- rail: arrow-memory

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: high-level wrapper types

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]   | [CAPABILITY]                                 |
| :-----: | :------------ | :-------------- | :------------------------------------------- |
|  [01]   | `Array`       | chunked wrapper | multi-chunk typed array with Python protocol |
|  [02]   | `ArrayStream` | stream wrapper  | consumable Arrow stream with iteration       |
|  [03]   | `Schema`      | schema wrapper  | Arrow schema with parameter accessors        |
|  [04]   | `TimeUnit`    | enum            | `s`, `ms`, `us`, `ns` time-unit vocabulary   |
|  [05]   | `Type`        | enum            | Arrow type identifier vocabulary             |

[PUBLIC_TYPE_SCOPE]: C-level holders — Cython-submodule types minted by the `c_*` factories, reached by factory return value, never as `nanoarrow.CArray` attributes.

| [INDEX] | [SYMBOL]                       | [MODULE]                  | [TYPE_FAMILY]   | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :------------------------ | :-------------- | :-------------------------------------------------- |
|  [01]   | `CArray`                       | `nanoarrow._array`        | C array holder  | low-level `ArrowArray` C struct wrapper             |
|  [02]   | `CArrayStream`                 | `nanoarrow._array_stream` | C stream holder | low-level `ArrowArrayStream` C struct               |
|  [03]   | `CSchema`                      | `nanoarrow._schema`       | C schema holder | low-level `ArrowSchema` C struct wrapper            |
|  [04]   | `CBuffer`                      | `nanoarrow._buffer`       | C buffer holder | typed buffer from Arrow buffer accessor             |
|  [05]   | `CArrayBuilder` / `CArrayView` | `nanoarrow._array`        | C builder/view  | incremental build and zero-copy view of `CArray`    |
|  [06]   | `CDeviceArray` / `DeviceType`  | `nanoarrow.device`        | device holder   | C Device Data Interface holder and device-type enum |

- [06]-[DEVICE]: `nanoarrow.device` mints device arrays via `c_device_array(obj, schema=None)` and carries `Device`, `DeviceType`, `DEVICE_CPU`, `cpu`, `resolve`; device access is array-level, with no device-array-stream constructor.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schema factories — module-level static functions minting a `Schema`; nullable types carry `(nullable)`, temporal types carry `(unit, timezone)`, decimals carry `(precision, scale)`.

| [INDEX] | [SURFACE]                                                                      | [CAPABILITY]                                 |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `int8/16/32/64(nullable)` / `uint8/16/32/64(nullable)`                         | fixed-width integer schemas                  |
|  [02]   | `float16/32/64(nullable)`                                                      | IEEE floating-point schemas                  |
|  [03]   | `bool_(nullable)` / `bool8(nullable)`                                          | Arrow boolean and bool8 schemas              |
|  [04]   | `timestamp(unit, timezone, nullable)`                                          | timestamp schema with timezone               |
|  [05]   | `date32(nullable)` / `date64(nullable)`                                        | calendar date schemas                        |
|  [06]   | `time32(unit, nullable)` / `time64(unit, nullable)`                            | time-of-day schemas                          |
|  [07]   | `duration(unit, nullable)`                                                     | duration schema                              |
|  [08]   | `string(nullable)` / `large_string(nullable)`                                  | UTF-8 and large-UTF-8 schemas                |
|  [09]   | `binary(nullable)` / `large_binary(nullable)`                                  | opaque bytes schemas                         |
|  [10]   | `string_view(nullable)` / `binary_view(nullable)`                              | string-view and binary-view schemas          |
|  [11]   | `decimal128(precision, scale, nullable)`                                       | 128-bit decimal schema                       |
|  [12]   | `decimal256(precision, scale, nullable)`                                       | 256-bit decimal schema                       |
|  [13]   | `list_(value_type, nullable)`                                                  | variable-length list schema                  |
|  [14]   | `large_list(value_type, nullable)`                                             | large list schema                            |
|  [15]   | `fixed_size_list(value_type, list_size, nullable)`                             | fixed-size list schema                       |
|  [16]   | `struct(fields, nullable)`                                                     | struct schema from field list                |
|  [17]   | `dictionary(index_type, value_type, ...)`                                      | dictionary-encoded schema                    |
|  [18]   | `map_(key_type, value_type, keys_sorted, nullable)`                            | map schema                                   |
|  [19]   | `dense_union(fields, type_codes, nullable)`                                    | dense union schema                           |
|  [20]   | `sparse_union(fields, type_codes, nullable)`                                   | sparse union schema                          |
|  [21]   | `extension_type(storage_schema, extension_name, extension_metadata, nullable)` | custom extension schema with name + metadata |
|  [22]   | `null(nullable)`                                                               | Arrow null schema                            |
|  [23]   | `interval_months(nullable)`                                                    | year-month interval schema                   |
|  [24]   | `interval_day_time(nullable)`                                                  | day-time interval schema                     |
|  [25]   | `interval_month_day_nano(nullable)`                                            | month-day-nanosecond interval schema         |
|  [26]   | `fixed_size_binary(byte_width, nullable)`                                      | fixed-size opaque bytes schema               |

[ENTRYPOINT_SCOPE]: array and stream construction — `array`/`array_stream` wrap any Arrow-compatible object; the `c_*` factories mint the low-level C holders.

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                         |
| :-----: | :--------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `array(obj, schema=None) -> Array`                   | factory | wrap any Arrow-compatible object     |
|  [02]   | `array_stream(obj, schema=None) -> ArrayStream`      | factory | wrap any stream-compatible object    |
|  [03]   | `Array(obj, schema=None, device=None)`               | ctor    | multi-chunk wrapper construction     |
|  [04]   | `Array.from_chunks(obj, schema=None, validate=True)` | factory | build from iterable of chunks        |
|  [05]   | `ArrayStream(obj, schema=None)`                      | ctor    | construct stream wrapper             |
|  [06]   | `ArrayStream.from_path(obj, *args, **kwargs)`        | factory | open IPC stream from file path       |
|  [07]   | `ArrayStream.from_readable(obj)`                     | factory | open IPC stream from file-like       |
|  [08]   | `ArrayStream.from_url(obj, *args, **kwargs)`         | factory | open IPC stream from URL             |
|  [09]   | `c_array(obj, schema=None) -> CArray`                | factory | low-level `CArray` construction      |
|  [10]   | `c_array_from_buffers(...) -> CArray`                | factory | raw-buffer construct with validation |
|  [11]   | `c_array_stream(obj, schema=None) -> CArrayStream`   | factory | low-level `CArrayStream` wrap        |
|  [12]   | `c_buffer(obj, schema=None) -> CBuffer`              | factory | low-level `CBuffer` wrap             |
|  [13]   | `c_schema(obj) -> CSchema`                           | factory | low-level `CSchema` wrap             |
|  [14]   | `schema(obj, **kwargs) -> Schema`                    | factory | wrap as high-level `Schema`          |

- `c_array_from_buffers`: `(schema, length, buffers, *, null_count, offset, children, validation_level, move, device) -> CArray` — the raw-buffer constructor, used only when the source is already a raw numeric buffer.

[ENTRYPOINT_SCOPE]: `Array` operations — every surface is an `Array` instance member.

| [INDEX] | [SURFACE]                                     | [CAPABILITY]                                                       |
| :-----: | :-------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `schema` / `device` / `offset`                | active `Schema`, device handle, logical offset                     |
|  [02]   | `inspect()`                                   | print buffer/child/chunk layout summary                            |
|  [03]   | `n_chunks` / `chunk(i)` / `iter_chunks()`     | chunk count, single chunk, chunk iterator                          |
|  [04]   | `iter_chunk_views()`                          | iterate zero-copy `CArrayView` per chunk                           |
|  [05]   | `n_buffers` / `buffer(i)` / `buffers`         | buffer count, buffer by index, all buffers                         |
|  [06]   | `n_children` / `child(i)` / `iter_children()` | child count, child by index, child iterator                        |
|  [07]   | `iter_py()`                                   | iterate as Python scalars                                          |
|  [08]   | `iter_scalar()`                               | iterate as `Scalar` objects                                        |
|  [09]   | `iter_tuples()`                               | iterate struct array as tuples                                     |
|  [10]   | `to_pylist()`                                 | collect to Python list                                             |
|  [11]   | `to_pysequence(*, handle_nulls=None)`         | collect with null policy (NumPy/array-protocol sequence)           |
|  [12]   | `to_columns_pysequence(*, handle_nulls=None)` | struct array to per-column sequences                               |
|  [13]   | `to_string()`                                 | repr-style string of the array                                     |
|  [14]   | `serialize(dst=None) -> bytes \| None`        | IPC-serialize; returns `bytes`, or writes `dst` and returns `None` |

[ENTRYPOINT_SCOPE]: `ArrayStream` operations — every surface is an `ArrayStream` instance member.

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------ | :----------------------------------------------- |
|  [01]   | `read_all()` / `read_next()`                                  | collect all chunks; pull the next `Array` chunk  |
|  [02]   | `iter_chunks()` / `iter_py()` / `iter_tuples()`               | iterate chunks, Python scalars, or struct tuples |
|  [03]   | `to_pylist()` / `to_columns_pysequence(*, handle_nulls=None)` | collect stream to list or column sequences       |
|  [04]   | `schema` / `close()`                                          | stream schema; release the underlying C stream   |

[ENTRYPOINT_SCOPE]: null policies and version — module-level functions; `nulls_*` feed the `handle_nulls=` argument at any `to_*`/`iter_*` call site.

| [INDEX] | [SURFACE]                     | [CAPABILITY]                      |
| :-----: | :---------------------------- | :-------------------------------- |
|  [01]   | `nulls_as_sentinel(sentinel)` | replace nulls with sentinel value |
|  [02]   | `nulls_forbid()`              | raise on null encounter           |
|  [03]   | `nulls_separate()`            | emit `(mask, values)` tuple       |
|  [04]   | `c_version()`                 | nanoarrow C library version       |

[ENTRYPOINT_SCOPE]: schema accessors, extension registry, and traversal — `Schema` exposes every Arrow type parameter directly; the `extension` package owns the `Extension` base and register/resolve lifecycle; `extension_canonical` carries canonical `bool8`; `iterator`/`visitor` own the array-view traversal the high-level `iter_*`/`to_*` methods dispatch into.

Every surface in the first table is a `Schema` accessor.

| [INDEX] | [SURFACE]                                     | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `type` / `params` / `field(i)` / `fields`     | type id, parameter dict, child field access           |
|  [02]   | `unit` / `timezone` / `precision` / `scale`   | temporal and decimal scalar parameters                |
|  [03]   | `byte_width` / `list_size`                    | fixed-width and list-size parameters                  |
|  [04]   | `index_type` / `value_type` / `key_type`      | dictionary and map key/value parameters               |
|  [05]   | `keys_sorted` / `dictionary_ordered`          | map sort flag and dictionary order flag               |
|  [06]   | `nullable` / `name` / `metadata` / `n_fields` | nullability, field name, custom metadata, field count |
|  [07]   | `extension` / `serialize()`                   | extension descriptor; serialize schema to PyCapsule   |

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `extension.register(extension_cls)` / `register_extension(...)` | register a custom `Extension` subclass               |
|  [02]   | `extension.resolve_extension(schema)`                           | look up a registered extension                       |
|  [03]   | `unregister_extension(name)`                                    | remove a registered extension                        |
|  [04]   | `extension_canonical.bool8()` / `Bool8Extension`                | canonical Arrow `bool8` extension type and converter |
|  [05]   | `iterator.iter_py` / `iter_tuples` / `iter_array_views`         | functional iterators over any stream source          |
|  [06]   | `visitor.ArrayViewVisitor` / `ArrayViewVisitable`               | subclassable visitor base for custom column reducers |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Array` wraps one or more `CArray` chunks (`n_chunks`/`chunk(i)`) and exposes the PyCapsule interface; `iter_chunk_views()` yields a zero-copy `CArrayView` per chunk for buffer-level reads.
- `Schema` wraps `CSchema` and surfaces every Arrow type parameter directly, so a consumer reads `unit`/`precision`/`list_size`/`index_type`/`type_codes`/`fields`/`metadata` without re-parsing a format string.
- `c_array_from_buffers` is the zero-copy entry constructing a `CArray` from raw numeric buffers at a chosen `validation_level`.

[STACKING]:
- `narwhals`(`narwhals.md`): a `nanoarrow.Array` capsule hands straight to `narwhals.from_arrow(..., backend=)` — the dataframe-agnostic owner consumes the `__arrow_c_array__`/`__arrow_c_stream__` export with no Python-list round-trip.
- `msgspec`(`../../.api/msgspec.md`): `to_columns_pysequence(handle_nulls=nulls_separate())` per-column `(mask, values)` tuples feed a `msgspec.Struct` decode, the Arrow null mask the sole nullability source.
- within-lib buffer-in: build a `CArray` from raw numeric/NumPy buffers via `c_array_from_buffers(schema, length, [validity, data], ...)`, lift to `Array`, and pass the capsule to a Polars frame or IPC sink — validity bitmap, offset, and dtype travel in the schema.
- within-lib IPC decode: an IPC byte stream opens through `ArrayStream.from_readable`/`from_path`/`from_url` and decodes via `iter_tuples()`/`to_columns_pysequence(...)`.
- within-lib extension: a domain `Extension` registers through `extension.register` so storage schema and name/metadata round-trip end-to-end; canonical `bool8` rides `extension_canonical`.

[LOCAL_ADMISSION]:
- Build `Schema` objects through the schema factory functions; format-string construction is foreclosed.
- Consume streams through `ArrayStream.read_all()`, `read_next()`, or `iter_chunks()`, and `close()` before discarding — the stream owns a C resource.
- Null handling is caller-controlled at every `to_*`/`iter_*` call site via `handle_nulls=` fed by `nulls_as_sentinel`/`nulls_forbid`/`nulls_separate`.
- `c_array_from_buffers` admits a raw numeric buffer source; a protocol-compatible source enters through `array()`.

[RAIL_LAW]:
- Package: `nanoarrow`
- Owns: Arrow C Data Interface Python binding, schema factory vocabulary, zero-copy buffer construction, IPC stream IO, and the extension-type registry
- Accept: any `__arrow_c_array__` / `__arrow_c_stream__` / `__arrow_c_schema__` compatible input plus raw numeric buffers
- Reject: manual ArrowSchema format-string construction; a Python-list hop where a capsule passes straight to a dataframe backend; `CArray`/`CSchema`/`CBuffer` treated as top-level names rather than factory return values
