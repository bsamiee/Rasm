# [PY_DATA_API_ZARR]

`zarr` supplies a chunked, compressed, N-dimensional array store with pluggable storage backends, a Zarr v2/v3 dual-format metadata layer, and a sync-over-async codec pipeline. `Array` and `Group` are thin synchronous wrappers over `AsyncArray`/`AsyncGroup`; every top-level factory has an `async def` mirror in `zarr.api.asynchronous`. It is the canonical `array-store` owner: the data owner mints arrays through `create_array`/`open_array`, hierarchies through `create_group`/`create_hierarchy`, declares the codec pipeline explicitly via `codecs=`, selects a backend from `zarr.storage`, and routes the array bytes through whatever `StoreLike` the campaign owns — a `LocalStore` for the work directory, an `obstore`-backed `ObjectStore` for S3/GCS/Azure, an `IcechunkStore` for the transactional/versioned path, or the same on-disk v3 format `tensorstore` reads asynchronously and `virtualizarr` references without copy. `zarr` is pure-Python (no native extension, no CPython floor), so it rides the cp315 core with no subprocess seam; native compression rides the optional `numcodecs`/`blosc`/`zstd` wheels its codec registry resolves.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zarr`
- package: `zarr`
- import: `import zarr; from zarr import Array, Group, AsyncArray, AsyncGroup, config; from zarr.storage import LocalStore, MemoryStore, ZipStore, FsspecStore, ObjectStore; from zarr.codecs import BloscCodec, BytesCodec, ShardingCodec, ZstdCodec`
- version: `3.x` (manifest floor `>=3.2.1`)
- license: MIT
- owner: `data`
- rail: array-store
- asset: pure Python (Zarr v3 core); no native extension, no CPython floor (rides cp315 core); native compression resolves through optional `numcodecs`/`blosc`/`zstandard`/`crc32c` wheels via the codec registry, the only ABI-bearing surface
- entry points: library use is import-only; no console script. `zarr.__version__` and `zarr.print_debug_info()` report the resolved environment
- capability: chunked N-dimensional typed array store with v2/v3 dual-format metadata, an explicit serialization+compression codec pipeline (`BytesCodec` serializer plus `BloscCodec`/`ZstdCodec`/`GzipCodec` byte compressors, `TransposeCodec`/`ScaleOffset` array transforms, `ShardingCodec` sub-chunk sharding, and `numcodecs.zarr3.*` filter/checksum codecs), pluggable `zarr.storage` backends (local/memory/zip/fsspec/object-store/GPU), orthogonal/vectorized/block/coordinate/mask indexing beyond NumPy basic indexing, consolidated metadata, a global donfig `config`, and a fully async `AsyncArray`/`AsyncGroup` rail mirrored by the synchronous surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core container types (`zarr`)
- rail: array-store

`zarr.__all__` is `('Array', 'AsyncArray', 'AsyncGroup', 'Group', '__version__', 'array', 'config', 'consolidate_metadata', 'copy', 'copy_all', 'copy_store', 'create', 'create_array', 'create_group', 'create_hierarchy', 'empty', 'empty_like', 'from_array', 'full', 'full_like', 'group', 'load', 'ones', 'ones_like', 'open', 'open_array', 'open_consolidated', 'open_group', 'open_like', 'print_debug_info', 'save', 'save_array', 'save_group', 'tree', 'zeros', 'zeros_like')`. `Array`/`Group` are synchronous facades; the `AsyncArray`/`AsyncGroup` they wrap are public and drive the async rail directly.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]  | [ROLE]                                                |
| :-----: | :----------- | :------------- | :---------------------------------------------------- |
|  [01]   | `Array`      | chunked array  | synchronous N-D typed chunked array over any store    |
|  [02]   | `Group`      | hierarchy node | synchronous named container for arrays and sub-groups |
|  [03]   | `AsyncArray` | async array    | async underlying array driving `Array` (`._async_array`) |
|  [04]   | `AsyncGroup` | async group    | async underlying group driving `Group`                |

[PUBLIC_TYPE_SCOPE]: storage backends (`zarr.storage`)
- rail: array-store

`zarr.storage.__all__` is `('FsspecStore', 'GpuMemoryStore', 'LocalStore', 'LoggingStore', 'ManagedMemoryStore', 'MemoryStore', 'ObjectStore', 'StoreLike', 'StorePath', 'WrapperStore', 'ZipStore')`. Every factory accepts a `StoreLike` (a `Store` instance, a `StorePath`, a path `str`/`Path`, or a dict for an implicit `MemoryStore`); the store is resolved once and passed in, never re-constructed per access.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [ROLE]                                                       |
| :-----: | :------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `LocalStore`         | filesystem store | local filesystem directory store                            |
|  [02]   | `MemoryStore`        | in-memory store  | dict-backed memory store                                    |
|  [03]   | `ManagedMemoryStore` | in-memory store  | reference-counted shared memory variant                     |
|  [04]   | `GpuMemoryStore`     | GPU store        | GPU-buffer-backed memory store (paired with `config.enable_gpu`) |
|  [05]   | `ZipStore`           | archive store    | Zip archive store (single-file packaging)                   |
|  [06]   | `FsspecStore`        | remote store     | fsspec-backed remote store (`FsspecStore.from_url`)         |
|  [07]   | `ObjectStore`        | object store     | `obstore`-backed object-storage store (S3, GCS, Azure)      |
|  [08]   | `LoggingStore`       | wrapper store    | store decorator with access logging                         |
|  [09]   | `WrapperStore`       | wrapper store    | composable store-wrapper base                               |
|  [10]   | `StorePath`          | path value       | typed store-relative path (`store / "subpath"`)             |
|  [11]   | `StoreLike`          | type alias       | `Store \| StorePath \| FSMap \| Path \| str \| dict` factory input |

[PUBLIC_TYPE_SCOPE]: codec pipeline (`zarr.codecs`)
- rail: array-store

`zarr.codecs.__all__` is `('BloscCname', 'BloscCodec', 'BloscShuffle', 'BytesCodec', 'CastValue', 'Crc32cCodec', 'Endian', 'GzipCodec', 'ScaleOffset', 'ShardingCodec', 'ShardingCodecIndexLocation', 'SubchunkWriteOrder', 'TransposeCodec', 'VLenBytesCodec', 'VLenUTF8Codec', 'ZstdCodec')`. A v3 codec chain is `[array->array transforms..., one array->bytes serializer, bytes->bytes compressors...]`; `BytesCodec` is the canonical serializer, `ShardingCodec` is the special serializer that nests an inner chain per sub-chunk. The legacy filter names (`Delta`, `FixedScaleOffset`, `Quantize`, `BitRound`, `PackBits`, `AsType`) and the alternate compressors (`Blosc`, `Zstd`, `GZip`, `LZ4`, `LZMA`, `BZ2`, `Zlib`, `Shuffle`) live in `numcodecs.zarr3`, NOT `zarr.codecs`.

| [INDEX] | [SYMBOL]                       | [CODEC_KIND]      | [ROLE]                                                       |
| :-----: | :----------------------------- | :---------------- | :---------------------------------------------------------- |
|  [01]   | `BytesCodec(endian=Endian.little)` | array->bytes serializer | canonical fixed-width serializer; `Endian` little/big   |
|  [02]   | `ShardingCodec(chunk_shape, codecs, index_codecs, index_location)` | array->bytes serializer | sub-chunk sharding; `ShardingCodecIndexLocation` start/end, `SubchunkWriteOrder` |
|  [03]   | `VLenUTF8Codec` / `VLenBytesCodec` | array->bytes serializer | variable-length UTF-8 string / bytes serialization      |
|  [04]   | `TransposeCodec(order)`        | array->array transform | axis-permute transform applied before serialization     |
|  [05]   | `ScaleOffset(scale, offset, dtype, astype)` | array->array transform | linear scale-offset quantization transform          |
|  [06]   | `CastValue(...)`               | array->array transform | dtype-cast transform                                     |
|  [07]   | `BloscCodec(cname, clevel, shuffle, typesize, blocksize)` | bytes->bytes compressor | Blosc multi-threaded; `BloscCname`, `BloscShuffle` |
|  [08]   | `ZstdCodec(level, checksum)`   | bytes->bytes compressor | Zstandard compression                                   |
|  [09]   | `GzipCodec(level)`             | bytes->bytes compressor | DEFLATE/gzip compression                                |
|  [10]   | `Crc32cCodec`                  | bytes->bytes checksum | CRC32C integrity checksum appended to chunk bytes        |

[PUBLIC_TYPE_SCOPE]: numcodecs v3 codec wrappers (`numcodecs.zarr3`)
- rail: array-store

`numcodecs.zarr3` (resolved by the `codecs` registry, available in `zarr>=3.1.3`) wraps the full numcodecs catalog as v3 codecs for the filter/legacy-compressor surface `zarr.codecs` does not carry. The module is marked deprecated upstream and a future-removal candidate; admit a name only when `zarr.codecs` lacks the equivalent.

| [INDEX] | [SYMBOL]                                                                 | [CODEC_KIND]            | [ROLE]                                          |
| :-----: | :----------------------------------------------------------------------- | :---------------------- | :---------------------------------------------- |
|  [01]   | `Blosc` / `LZ4` / `Zstd` / `Zlib` / `GZip` / `BZ2` / `LZMA` / `Shuffle`  | bytes->bytes compressor | alternate compressors not in `zarr.codecs`      |
|  [02]   | `Delta` / `BitRound` / `FixedScaleOffset` / `Quantize` / `PackBits` / `AsType` | array->array filter | numcodecs filters (quantize/delta/bit-pack)  |
|  [03]   | `PCodec` / `ZFPY`                                                         | array->bytes serializer | scientific-array serializers                    |
|  [04]   | `CRC32` / `CRC32C` / `Adler32` / `Fletcher32` / `JenkinsLookup3`         | bytes->bytes checksum   | integrity checksum codecs                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation
- rail: array-store

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY]  | [RETURNS] |
| :-----: | :---------------------------------------------------------------------------------------------- | :-------------- | :-------- |
|  [01]   | `create_array(store, *, name=None, shape, dtype, chunks='auto', shards=None, filters=..., compressors=..., serializer='auto', fill_value=None, order=None, zarr_format=3, attributes=None, chunk_key_encoding=None, dimension_names=None, config=None, overwrite=False)` | creation        | `Array`   |
|  [02]   | `create(shape, *, chunks=True, dtype=None, compressor=..., fill_value=0, store=None, ...)`       | creation (v2-style) | `Array` |
|  [03]   | `array(data, **kwargs)`                                                                          | data intake     | `Array`   |
|  [04]   | `from_array(store, *, data, write_data=True, name=None, chunks='keep', shards='keep', ...)`      | data intake     | `Array`   |
|  [05]   | `zeros(shape, **kwargs)` / `ones(shape, **kwargs)` / `full(shape, fill_value, **kwargs)`         | fill creation   | `Array`   |
|  [06]   | `empty(shape, **kwargs)`                                                                         | fill creation   | `Array`   |
|  [07]   | `zeros_like(a, **kwargs)` / `ones_like(a, ...)` / `full_like(a, ...)` / `empty_like(a, ...)`     | mirror creation | `Array`   |
|  [08]   | `open_like(a, path, **kwargs)`                                                                   | mirror open     | `Array`   |

[ENTRYPOINT_SCOPE]: group, hierarchy, open, and persistence
- rail: array-store

| [INDEX] | [SURFACE]                                                                                 | [ENTRY_FAMILY] | [RETURNS]                |
| :-----: | :---------------------------------------------------------------------------------------- | :------------- | :----------------------- |
|  [01]   | `create_group(store, *, path=None, overwrite=False, zarr_format=3, attributes=None)`      | creation       | `Group`                  |
|  [02]   | `create_hierarchy(*, store, nodes, overwrite=False)`                                      | bulk creation  | `Iterator[Array\|Group]` |
|  [03]   | `group(store=None, *, overwrite=False, chunk_store=None, zarr_format=None, ...)`           | open-or-create | `Group`                  |
|  [04]   | `open(store=None, *, mode=None, zarr_format=None, path=None, **kwargs)`                    | open           | `Array \| Group`         |
|  [05]   | `open_array(store, *, zarr_format=None, path='', mode=None, **kwargs)`                     | open           | `Array`                  |
|  [06]   | `open_group(store, *, mode=None, cache_attrs=None, path=None, **kwargs)`                   | open           | `Group`                  |
|  [07]   | `open_consolidated(*args, use_consolidated=True, **kwargs)`                                | open           | `Group`                  |
|  [08]   | `save(store, *args, zarr_format=None, path=None, **kwargs)`                                | persistence    | `None`                   |
|  [09]   | `save_array(store, arr, *, zarr_format=None, path=None, **kwargs)`                         | persistence    | `None`                   |
|  [10]   | `save_group(store, *args, zarr_format=None, path=None, **kwargs)`                          | persistence    | `None`                   |
|  [11]   | `load(store, *, path=None, zarr_format=None)`                                              | persistence    | `NDArrayLike \| dict`    |
|  [12]   | `consolidate_metadata(store, *, path=None, zarr_format=None)`                              | metadata       | `Group`                  |
|  [13]   | `copy(...)` / `copy_all(...)` / `copy_store(...)`                                          | copy utilities | counts tuple             |
|  [14]   | `tree(grp, *, expand=None, level=None)` / `print_debug_info()`                             | inspection     | `TreeRepr` / `None`      |

[ENTRYPOINT_SCOPE]: async mirror (`zarr.api.asynchronous`)
- rail: array-store

Every synchronous factory above wraps an `async def` of the same name in `zarr.api.asynchronous`, run through the package sync bridge. Reach the async surface directly inside an `await` context to avoid the sync wrapper's event-loop hop. Async-mirrored: `open`, `open_array`, `open_group`, `open_consolidated`, `open_like`, `create`, `create_group`, `group`, `array`, `zeros`/`ones`/`empty`/`full` and `_like` variants, `save`/`save_array`/`save_group`, `load`, `consolidate_metadata`, `copy`/`copy_all`/`copy_store`, `tree`. The synchronous `create_array`/`create_hierarchy`/`from_array` resolve their async work through `AsyncArray.create`/`AsyncGroup.create_hierarchy`, not a same-named `zarr.api.asynchronous` function.

[ENTRYPOINT_SCOPE]: Array operations
- rail: array-store

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `Array[selection]` / `Array[selection] = value`                                          | indexing       | NumPy-compatible basic indexing |
|  [02]   | `Array.oindex[selection]`                                                                | indexing       | orthogonal (outer) indexing     |
|  [03]   | `Array.vindex[selection]`                                                                | indexing       | vectorized (fancy) indexing     |
|  [04]   | `Array.coordinate_index` / `Array.blocks[selection]`                                     | indexing       | coordinate / block-level chunk indexing |
|  [05]   | `Array.get_basic_selection` / `get_orthogonal_selection` / `get_coordinate_selection` / `get_mask_selection` / `get_block_selection` `(selection, *, out=None, fields=None, prototype=None)` | selection | explicit read selections        |
|  [06]   | `Array.set_basic_selection` / `set_orthogonal_selection` / `set_coordinate_selection` / `set_mask_selection` / `set_block_selection` `(selection, value, *, fields=None, prototype=None)` | write | explicit write selections       |
|  [07]   | `Array.append(data, axis=0)` / `Array.resize(new_shape)`                                  | mutation       | append / resize in store        |
|  [08]   | `Array.update_attributes(new_attributes)`                                                 | metadata       | update user attributes          |
|  [09]   | `Array.with_config(config)` / `Array.info` / `Array.info_complete()`                      | configuration  | per-array config, info report   |
|  [10]   | `Array.nchunks` / `nchunks_initialized` / `nbytes_stored()` / `chunks` / `shards` / `cdata_shape` | metadata | chunk-grid and storage metadata |
|  [11]   | `Array.metadata` / `Array.store_path` / `Array.path` / `Array.compressors` / `Array.serializer` / `Array.filters` | metadata | resolved codec pipeline and store path |

[ENTRYPOINT_SCOPE]: Group operations
- rail: array-store

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `Group.create_array(name, *, shape, dtype, ...)` / `Group.create_group(name, **kwargs)` | creation       | create child array / group         |
|  [02]   | `Group.create_hierarchy(nodes, *, overwrite=False)`                                     | bulk creation  | create multiple children           |
|  [03]   | `Group.require_array(name, *, shape, **kwargs)` / `require_group(name, **kwargs)` / `require_groups(*names)` | idempotent | open-or-create child       |
|  [04]   | `Group[path]` / `Group.get(path, default=None)`                                         | access         | access child by path               |
|  [05]   | `Group.keys()` / `Group.members(max_depth=0, *, use_consolidated_for_children=True)`    | enumeration    | list child keys / full subtree     |
|  [06]   | `Group.arrays()` / `Group.groups()` / `Group.array_keys()` / `Group.group_keys()`       | enumeration    | iterate child arrays / sub-groups  |
|  [07]   | `Group.move(source, dest)`                                                              | mutation       | rename or relocate child           |
|  [08]   | `Group.update_attributes(new_attributes)`                                               | metadata       | update user attributes             |
|  [09]   | `Group.tree(expand=None, level=None)`                                                   | inspection     | render subtree as `TreeRepr`       |

## [04]-[IMPLEMENTATION_LAW]

[ARRAY_STORE_TOPOLOGY]:
- namespace: `zarr` (top-level sync facade), `zarr.api.asynchronous` (async mirror), `zarr.storage` (stores), `zarr.codecs` (v3 codec pipeline), `zarr.abc.store`/`zarr.abc.codec` (extension protocols), `zarr.registry` (codec/store/pipeline registries), `zarr.config` (global donfig config), `zarr.buffer` (`Buffer`/`NDBuffer`, CPU and GPU implementations).
- async axis: `Array`/`Group` are synchronous facades over public `AsyncArray`/`AsyncGroup`; one operation chooses the sync facade OR the async mirror, never parallel sync/async call sites for the same write. Inside an `await` context use `zarr.api.asynchronous.*` or `AsyncArray`/`AsyncGroup` directly to skip the sync wrapper's loop hop.
- format axis: Zarr v3 is the default (`zarr_format=3`); v2 round-trips through `zarr_format=2` on every open/create path. `ShardingCodec`, `dimension_names`, and the explicit `[transform, serializer, compressor]` codec chain are v3-only; v2 uses the single `compressor=`/`filters=` numcodecs shape.
- codec axis: declare the pipeline explicitly via `create_array(filters=, compressors=, serializer=)` (or v2 `compressor=`); `BytesCodec` is the canonical serializer, `ShardingCodec` nests an inner chain per sub-chunk to cut small-file counts on object stores. Default codec selection varies by `zarr_format` and the `config['codecs']` registry, which resolves `numcodecs.zarr3.*` for any filter/compressor `zarr.codecs` does not carry.
- indexing axis: `__getitem__`/`__setitem__` implement NumPy *basic* indexing only; `oindex` is orthogonal, `vindex` vectorized, `blocks` block-level, and the explicit `get_*_selection`/`set_*_selection` family covers basic/orthogonal/coordinate/mask/block reads and writes with `out`/`fields`/`prototype` control.
- store axis: any `StoreLike` (a `Store`, `StorePath`, path `str`/`Path`, or dict) resolves to one store; construct it once outside hot paths (store construction is I/O) and pass it in. `WrapperStore`/`LoggingStore` decorate an existing store; `GpuMemoryStore` pairs with `config.enable_gpu()`.
- config axis: `zarr.config` is a donfig `DConfig` singleton; `config.set({...})` (context manager or global), `config.get(key)`, `config.enable_gpu()`, `config.reset()`. Keys include `default_zarr_format`, `array.order`, `array.write_empty_chunks`, `async.concurrency`, `async.timeout`, `threading.max_workers`, `codec_pipeline.batch_size`, and the `codecs` registry. Per-array overrides use `Array.with_config()`.
- metadata axis: `consolidate_metadata(store)` merges all node metadata documents into one consolidated `zarr.json`/`.zmetadata` at the root, read back via `open_consolidated`; call it after bulk writes to remote stores before opening read-only.

[LOCAL_ADMISSION]:
- Use `create_array` for typed v3 arrays and `create_group`/`create_hierarchy` for hierarchies; use `open`/`open_array`/`open_group` for existing stores; one polymorphic factory per concept, never a per-backend or per-mode factory family.
- Specify the codec pipeline explicitly via `filters=`/`compressors=`/`serializer=` on creation; do not rely on `zarr_format`-dependent defaults for reproducible chunk encoding.
- Use `oindex`/`vindex`/`blocks` for non-basic selection; `__getitem__` is NumPy basic indexing only.
- Resolve any `StoreLike` to one store outside hot paths; reuse it across array/group factories; decorate via `WrapperStore`/`LoggingStore` rather than re-wrapping the backend.
- Reach the async mirror (`zarr.api.asynchronous` / `AsyncArray` / `AsyncGroup`) inside `await` contexts; never mix sync and async call sites for one write.

[INTEGRATION]:
- icechunk seam: the transactional/versioned path passes `repo.writable_session(branch).store` (an `IcechunkStore`, a `zarr.abc.store.Store`) straight into `zarr.open_group(store=...)`/`create_array(store=...)`; zarr writes chunks, icechunk owns commit/branch/snapshot. Never hand-roll chunk-key versioning zarr+icechunk already own.
- tensorstore seam: zarr writes the v3 on-disk format that `tensorstore` opens with the `zarr3` driver for the high-throughput async read/write path; the two speak the identical chunk/codec layout, so a zarr-written store is a tensorstore-readable store with no conversion.
- obstore seam: the cloud backend is `zarr.storage.ObjectStore` wrapping an `obstore` store (`S3Store`/`GCSStore`/`AzureStore`); object-store credentials and zero-copy `Bytes` stay in the obstore owner, zarr sees only a `StoreLike`. `FsspecStore.from_url` is the alternate fsspec-backed remote path.
- virtualizarr seam: `virtualizarr` builds `ManifestArray` chunk-reference manifests over existing files and writes them to an `IcechunkStore` or kerchunk reference store; the resulting virtual `xarray.Dataset` reads through the same zarr store protocol without copying source bytes.
- xarray/cubed seam: `xarray.open_zarr`/`Dataset.to_zarr` and `cubed.Array.to_zarr` both target a zarr store; the labelled-cube and bounded-memory chunked-compute layers persist through this owner, and `numcodecs.zarr3` codecs carry the CF mask/scale/quantize filters the field-dataset rail declares.
- numcodecs seam: filters and alternate compressors (`Delta`/`FixedScaleOffset`/`Quantize`/`BitRound` and `LZ4`/`Shuffle`/`PCodec`) resolve from `numcodecs.zarr3` through the `config['codecs']` registry; admit a numcodecs name only where `zarr.codecs` lacks the equivalent, since the module is upstream-deprecated.

[RAIL_LAW]:
- Package: `zarr`
- Owns: chunked N-D array store with v2/v3 dual-format metadata, the explicit v3 codec pipeline (serializer + array/byte transforms + compressors + checksum), pluggable `zarr.storage` backends, orthogonal/vectorized/block/coordinate/mask indexing, consolidated metadata, the donfig `config`, and the synchronous-over-async `Array`/`AsyncArray` rail
- Accept: any `StoreLike` resolved once and passed in; an explicit `filters=`/`compressors=`/`serializer=` codec pipeline; `oindex`/`vindex`/`blocks` for non-basic selection; an `IcechunkStore`/`ObjectStore`/`FsspecStore` for transactional/cloud paths; the async mirror inside `await` contexts
- Reject: phantom `zarr.codecs` names that actually live in `numcodecs.zarr3` (`Delta`, `Quantize`, `Blosc`, `LZ4`, `LZMA`, `BZ2`, `Zlib`, `AsType`); manual chunk-key construction outside the store API; direct metadata-file writes bypassing the codec/store write path; a per-backend store factory family where one `StoreLike` discriminates; mixed sync/async call sites for one write; relying on `zarr_format`-dependent default codecs where reproducibility is required
