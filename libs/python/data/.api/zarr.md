# [PY_DATA_API_ZARR]

`zarr` supplies a chunked N-dimensional array store with pluggable storage backends, a Zarr v2/v3 dual-format metadata layer, and a rich codec pipeline. The primary interaction surface is `Array` and `Group`, with top-level factory functions for creation (`create_array`, `create_group`, `open`, `open_array`, `open_group`), in-memory construction (`zeros`, `ones`, `full`, `empty`, and `_like` variants), and persistence utilities (`save`, `save_array`, `save_group`, `load`, `consolidate_metadata`). Storage backends live in `zarr.storage` (`MemoryStore`, `LocalStore`, `ZipStore`, `FsspecStore`, `ObjectStore`). Codecs live in `zarr.codecs` and include Blosc, GZip, Zstd, LZ4, LZMA, ShardingCodec, BytesCodec, TransposeCodec, and numcodecs-backed filters.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zarr`
- package: `zarr`
- module: `zarr`
- asset: pure Python + optional native codecs
- rail: array-store

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core container types
- rail: array-store

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]  | [ROLE]                                    |
| :-----: | :----------- | :------------- | :---------------------------------------- |
|   [1]   | `Array`      | chunked array  | N-D typed chunked array over any store    |
|   [2]   | `Group`      | hierarchy node | named container for arrays and sub-groups |
|   [3]   | `AsyncArray` | async array    | async underlying array (drives `Array`)   |
|   [4]   | `AsyncGroup` | async group    | async underlying group (drives `Group`)   |

[PUBLIC_TYPE_SCOPE]: storage backends
- rail: array-store

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [ROLE]                               |
| :-----: | :------------------- | :--------------- | :----------------------------------- |
|   [1]   | `MemoryStore`        | in-memory store  | dict-backed memory store             |
|   [2]   | `LocalStore`         | filesystem store | local filesystem directory store     |
|   [3]   | `ZipStore`           | archive store    | Zip archive store                    |
|   [4]   | `FsspecStore`        | remote store     | fsspec-backed remote store           |
|   [5]   | `ObjectStore`        | object store     | object-storage store (S3, GCS, etc.) |
|   [6]   | `ManagedMemoryStore` | memory store     | managed-memory variant               |
|   [7]   | `LoggingStore`       | wrapper store    | store with access logging            |
|   [8]   | `WrapperStore`       | wrapper store    | composable store wrapper base        |
|   [9]   | `StorePath`          | path value       | typed store-relative path object     |

[PUBLIC_TYPE_SCOPE]: codec types
- rail: array-store

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]   | [ROLE]                               |
| :-----: | :--------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `BytesCodec`                       | bytes codec     | raw byte serialisation               |
|   [2]   | `TransposeCodec`                   | transform codec | array axis transpose                 |
|   [3]   | `ShardingCodec`                    | sharding codec  | sub-chunk sharding (Zarr v3)         |
|   [4]   | `GzipCodec` / `GZip`               | compression     | DEFLATE/gzip compression             |
|   [5]   | `ZstdCodec` / `Zstd`               | compression     | Zstandard compression                |
|   [6]   | `BloscCodec` / `Blosc`             | compression     | Blosc multi-threaded compression     |
|   [7]   | `LZ4` / `LZMA` / `BZ2` / `Zlib`    | compression     | LZ4, LZMA, BZ2, and Zlib codecs      |
|   [8]   | `Delta` / `FixedScaleOffset`       | filter codec    | delta and scale-offset filters       |
|   [9]   | `ScaleOffset` / `Quantize`         | filter codec    | scale-offset and quantize filters    |
|  [10]   | `BitRound` / `PackBits`            | filter codec    | bit-rounding and bit-packing filters |
|  [11]   | `AsType` / `CastValue`             | cast codec      | dtype cast codecs                    |
|  [12]   | `VLenUTF8Codec` / `VLenBytesCodec` | variable-length | variable-length string/bytes codecs  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation
- rail: array-store

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]  | [RAIL]                             |
| :-----: | :----------------------------------------------------- | :-------------- | :--------------------------------- |
|   [1]   | `create_array(store, name, shape, dtype, chunks, ...)` | creation        | create typed array in store        |
|   [2]   | `create(shape, chunks, dtype, ...)`                    | creation        | create array with inline store     |
|   [3]   | `array(data, **kwargs)`                                | data intake     | create and fill from array-like    |
|   [4]   | `from_array(store, data, write_data, name, ...)`       | data intake     | create from existing array-like    |
|   [5]   | `zeros(shape, **kwargs)` / `ones(shape, **kwargs)`     | fill creation   | zero-filled or one-filled array    |
|   [6]   | `full(shape, fill_value, **kwargs)`                    | fill creation   | constant-filled array              |
|   [7]   | `empty(shape, **kwargs)`                               | fill creation   | uninitialized array                |
|   [8]   | `zeros_like(a, **kwargs)` / `ones_like(a, ...)`        | mirror creation | shape/dtype-matched zero/one array |
|   [9]   | `full_like(a, **kwargs)` / `empty_like(a, ...)`        | mirror creation | fill/empty matching source array   |
|  [10]   | `open_like(a, path, **kwargs)`                         | mirror open     | open array matching source shape   |

[ENTRYPOINT_SCOPE]: group and hierarchy creation
- rail: array-store

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `create_group(store, path, zarr_format, overwrite, ...)` | creation       | create group in store         |
|   [2]   | `create_hierarchy(store, nodes, overwrite)`              | bulk creation  | create multiple arrays/groups |
|   [3]   | `group(store, overwrite, chunk_store, ...)`              | open or create | open or create a group        |

[ENTRYPOINT_SCOPE]: open and persistence operations
- rail: array-store

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :----------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `open(store, mode, zarr_format, ...)`                  | open           | open array or group by mode        |
|   [2]   | `open_array(store, zarr_format, path, ...)`            | open           | open typed array                   |
|   [3]   | `open_group(store, mode, cache_attrs, ...)`            | open           | open group                         |
|   [4]   | `open_consolidated(*args, use_consolidated, **kwargs)` | open           | open with consolidated metadata    |
|   [5]   | `save(store, *args, zarr_format, path, ...)`           | persistence    | save arrays to store               |
|   [6]   | `save_array(store, arr, zarr_format, path, ...)`       | persistence    | save single array                  |
|   [7]   | `save_group(store, *args, zarr_format, path, ...)`     | persistence    | save multiple arrays as group      |
|   [8]   | `load(store, path, zarr_format)`                       | persistence    | load array or group to memory      |
|   [9]   | `consolidate_metadata(store, path, zarr_format)`       | metadata       | consolidate all metadata into root |
|  [10]   | `copy(...)` / `copy_all(...)` / `copy_store(...)`      | copy utilities | copy arrays or stores              |
|  [11]   | `tree(grp, expand, level)`                             | inspection     | render group hierarchy as tree     |

[ENTRYPOINT_SCOPE]: Array operations
- rail: array-store

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------------ | :------------- | :------------------------------ |
|   [1]   | `Array[selection]` / `Array[selection] = value`         | indexing       | NumPy-compatible basic indexing |
|   [2]   | `Array.oindex[selection]`                               | indexing       | orthogonal (outer) indexing     |
|   [3]   | `Array.vindex[selection]`                               | indexing       | vectorised (fancy) indexing     |
|   [4]   | `Array.blocks[selection]`                               | indexing       | block-level chunk indexing      |
|   [5]   | `Array.get_basic_selection(selection, out, ...)`        | selection      | explicit basic selection        |
|   [6]   | `Array.get_orthogonal_selection(selection, out, ...)`   | selection      | explicit orthogonal selection   |
|   [7]   | `Array.get_coordinate_selection(selection, out, ...)`   | selection      | explicit coordinate selection   |
|   [8]   | `Array.get_mask_selection(mask, out, ...)`              | selection      | explicit boolean mask selection |
|   [9]   | `Array.get_block_selection(selection, out, ...)`        | selection      | explicit block selection        |
|  [10]   | `Array.set_basic_selection(selection, value, ...)`      | write          | explicit basic write            |
|  [11]   | `Array.set_orthogonal_selection(selection, value, ...)` | write          | explicit orthogonal write       |
|  [12]   | `Array.set_coordinate_selection(selection, value, ...)` | write          | explicit coordinate write       |
|  [13]   | `Array.set_mask_selection(mask, value, ...)`            | write          | explicit boolean mask write     |
|  [14]   | `Array.set_block_selection(selection, value, ...)`      | write          | explicit block write            |
|  [15]   | `Array.append(data, axis)`                              | mutation       | append data along axis          |
|  [16]   | `Array.resize(new_shape)`                               | mutation       | resize array in-place           |
|  [17]   | `Array.update_attributes(new_attributes)`               | metadata       | update user attributes          |
|  [18]   | `Array.with_config(config)`                             | configuration  | return array with new config    |

[ENTRYPOINT_SCOPE]: Group operations
- rail: array-store

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `Group.create_array(name, shape, dtype, data, ...)` | creation       | create child array                 |
|   [2]   | `Group.create_group(name, **kwargs)`                | creation       | create child group                 |
|   [3]   | `Group.create_hierarchy(nodes, overwrite)`          | bulk creation  | create multiple children           |
|   [4]   | `Group.require_array(name, shape, **kwargs)`        | idempotent     | open or create array               |
|   [5]   | `Group.require_group(name, **kwargs)`               | idempotent     | open or create sub-group           |
|   [6]   | `Group.require_groups(*names)`                      | idempotent     | open or create multiple sub-groups |
|   [7]   | `Group[path]` / `Group.get(path, default)`          | access         | access child by path               |
|   [8]   | `Group.keys()` / `Group.members(max_depth, ...)`    | enumeration    | list child keys or full tree       |
|   [9]   | `Group.arrays()` / `Group.groups()`                 | enumeration    | iterate child arrays or sub-groups |
|  [10]   | `Group.move(source, dest)`                          | mutation       | rename or relocate child           |
|  [11]   | `Group.update_attributes(new_attributes)`           | metadata       | update user attributes             |
|  [12]   | `Group.tree(expand, level, max_nodes, plain)`       | inspection     | render subtree as text             |

## [4]-[IMPLEMENTATION_LAW]

[ARRAY_STORE_TOPOLOGY]:
- namespace: `zarr` (top-level), `zarr.storage` (stores), `zarr.codecs` (codecs)
- `Array` and `Group` are synchronous wrappers over `AsyncArray` and `AsyncGroup`; async access is through `zarr.api.asynchronous`
- Zarr v3 is the default (`zarr_format=3`); v2 is supported via `zarr_format=2` on all open/create paths
- `ShardingCodec` is Zarr v3 only; it enables sub-chunk sharding to reduce small-file counts on object stores
- `consolidate_metadata` merges all `.zarray`/`.zgroup` keys into a single consolidated metadata document at the root
- `zarr.config` is the global config object (a `zarr.core.config.Config` instance); per-array configuration uses `Array.with_config()`

[LOCAL_ADMISSION]:
- Use `create_array` for typed arrays and `create_group` for hierarchies; use `open` for existing stores.
- Specify the codec pipeline explicitly via `codecs=` on creation; default codec selection varies by `zarr_format`.
- Use `oindex` for orthogonal selection and `vindex` for vectorised selection; `__getitem__` implements basic NumPy selection only.
- Keep `FsspecStore` and `ObjectStore` construction outside hot paths; store construction is I/O; pass an already-open store into array/group factories.
- `consolidate_metadata` must be called after bulk writes to remote stores before opening in read-only consolidated mode.

[RAIL_LAW]:
- Package: `zarr`
- Owns: chunked N-D array store with pluggable storage and codec pipeline
- Accept: any `StoreLike` (path string, `Path`, store instance), NumPy-compatible array-like for data
- Reject: manual chunk-key construction outside the store API, direct metadata file writes bypassing zarr's write path
