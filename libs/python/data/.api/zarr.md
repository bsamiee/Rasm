# [PY_DATA_API_ZARR]

`zarr` owns the chunked, compressed, N-dimensional array store: a Zarr v2/v3 dual-format metadata layer, an explicit serialization-plus-compression codec pipeline, and pluggable `zarr.storage` backends over a sync-facade-over-async rail. `Array`/`Group` are synchronous facades over the public `AsyncArray`/`AsyncGroup`, and every top-level factory mirrors an `async def` in `zarr.api.asynchronous`. Pure-Python with no native extension and no subprocess seam, it resolves native compression through `numcodecs`/`blosc`/`zstd`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `zarr`
- package: `zarr`
- module: `import zarr`
- namespaces: `zarr`, `zarr.api.asynchronous`, `zarr.storage`, `zarr.codecs`, `zarr.abc.store`, `zarr.abc.codec`, `zarr.registry`, `zarr.config`, `zarr.buffer`
- rail: array-store — chunked N-D typed array store for the gridded tensor plane
- entry points: import-only, no console script; `zarr.__version__` and `zarr.print_debug_info()` report the resolved environment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core container types (`zarr`)

`Array`/`Group` are synchronous facades; the `AsyncArray`/`AsyncGroup` they wrap are public and drive the async rail directly through `Array._async_array`.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]  | [CAPABILITY]                                             |
| :-----: | :----------- | :------------- | :------------------------------------------------------- |
|  [01]   | `Array`      | chunked array  | synchronous N-D typed chunked array over any store       |
|  [02]   | `Group`      | hierarchy node | synchronous named container for arrays and sub-groups    |
|  [03]   | `AsyncArray` | async array    | async underlying array driving `Array` (`._async_array`) |
|  [04]   | `AsyncGroup` | async group    | async underlying group driving `Group`                   |

[PUBLIC_TYPE_SCOPE]: storage backends (`zarr.storage`)

Every factory takes a `StoreLike` resolved once and passed in, never re-constructed per access.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [CAPABILITY]                                                       |
| :-----: | :------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `LocalStore`         | filesystem store | local filesystem directory store                                   |
|  [02]   | `MemoryStore`        | in-memory store  | dict-backed memory store                                           |
|  [03]   | `ManagedMemoryStore` | in-memory store  | reference-counted shared memory variant                            |
|  [04]   | `GpuMemoryStore`     | GPU store        | GPU-buffer-backed memory store (paired with `config.enable_gpu`)   |
|  [05]   | `ZipStore`           | archive store    | Zip archive store (single-file packaging)                          |
|  [06]   | `FsspecStore`        | remote store     | fsspec-backed remote store (`FsspecStore.from_url`)                |
|  [07]   | `ObjectStore`        | object store     | `obstore`-backed object-storage store (S3, GCS, Azure)             |
|  [08]   | `LoggingStore`       | wrapper store    | store decorator with access logging                                |
|  [09]   | `WrapperStore`       | wrapper store    | composable store-wrapper base                                      |
|  [10]   | `StorePath`          | path value       | typed store-relative path (`store / "subpath"`)                    |
|  [11]   | `StoreLike`          | type alias       | `Store \| StorePath \| FSMap \| Path \| str \| dict` factory input |

[PUBLIC_TYPE_SCOPE]: codec pipeline (`zarr.codecs`)

A v3 codec chain is `[array->array transforms..., one array->bytes serializer, bytes->bytes compressors...]`; `BytesCodec` is the canonical serializer and `ShardingCodec` the special serializer nesting an inner chain per sub-chunk. Filter and legacy-compressor codecs (`Delta`, `Blosc`, …) resolve from `numcodecs`, never `zarr.codecs`.
- call: `BytesCodec(endian)`; `ShardingCodec(chunk_shape, codecs, index_codecs, index_location)` with `index_location` a `ShardingCodecIndexLocation`; `TransposeCodec(order)`; `ScaleOffset(scale, offset, dtype, astype)`; `BloscCodec(cname, clevel, shuffle, typesize, blocksize)`; `ZstdCodec(level, checksum)`; `GzipCodec(level)`

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]           | [CAPABILITY]                                            |
| :-----: | :--------------------------------- | :---------------------- | :------------------------------------------------------ |
|  [01]   | `BytesCodec`                       | array->bytes serializer | canonical fixed-width serializer; `Endian` little/big   |
|  [02]   | `ShardingCodec`                    | array->bytes serializer | sub-chunk sharding nesting an inner chain per sub-chunk |
|  [03]   | `VLenUTF8Codec` / `VLenBytesCodec` | array->bytes serializer | variable-length UTF-8 string / bytes serialization      |
|  [04]   | `TransposeCodec`                   | array->array transform  | axis-permute transform applied before serialization     |
|  [05]   | `ScaleOffset`                      | array->array transform  | linear scale-offset quantization transform              |
|  [06]   | `CastValue`                        | array->array transform  | dtype-cast transform                                    |
|  [07]   | `BloscCodec`                       | bytes->bytes compressor | Blosc multi-threaded; `BloscCname`, `BloscShuffle`      |
|  [08]   | `ZstdCodec`                        | bytes->bytes compressor | Zstandard compression                                   |
|  [09]   | `GzipCodec`                        | bytes->bytes compressor | DEFLATE/gzip compression                                |
|  [10]   | `Crc32cCodec`                      | bytes->bytes checksum   | CRC32C integrity checksum appended to chunk bytes       |

[PUBLIC_TYPE_SCOPE]: codec-role base classes (`zarr.abc.codec`)

Every concrete `zarr.codecs.*`/`numcodecs` codec subclasses one role base; the pipeline is statically typed `tuple[tuple[ArrayArrayCodec, ...], ArrayBytesCodec, tuple[BytesBytesCodec, ...]]`, the three-slot shape a `create_array(filters=, serializer=, compressors=)` consumer annotates against.

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]           | [CAPABILITY]                                                                     |
| :-----: | :---------------- | :---------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `ArrayArrayCodec` | array->array transform  | base for pre-serialization array transforms (`TransposeCodec`/`ScaleOffset`)     |
|  [02]   | `ArrayBytesCodec` | array->bytes serializer | base for the single chain serializer (`BytesCodec`/`ShardingCodec`/`VLen*Codec`) |
|  [03]   | `BytesBytesCodec` | bytes->bytes compressor | base for byte compressors and checksums (`BloscCodec`/`ZstdCodec`/`Crc32cCodec`) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation

`create_array` mints the typed v3 array; `create`/`array`/`from_array` and the `zeros`/`ones`/`full`/`empty` fill family with their `_like` mirrors share the store/codec surface, each a module-level factory returning `Array`.
- call: `create_array(store, *, name, shape, dtype, chunks='auto', shards, filters, compressors, serializer='auto', fill_value, order, zarr_format=3, attributes, chunk_key_encoding, dimension_names, config, overwrite=False) -> Array`; `from_array(store, *, data, write_data=True, chunks='keep', shards='keep')`

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------ | :------------------------------ |
|  [01]   | `create_array`                                          | canonical typed v3 constructor  |
|  [02]   | `create`                                                | v2-style creation               |
|  [03]   | `array(data)`                                           | intake an in-memory array       |
|  [04]   | `from_array`                                            | intake copying from a source    |
|  [05]   | `zeros` / `ones` / `full`                               | fill creation                   |
|  [06]   | `empty`                                                 | uninitialized creation          |
|  [07]   | `zeros_like` / `ones_like` / `full_like` / `empty_like` | fill mirror of a template array |
|  [08]   | `open_like`                                             | open mirroring a template array |

[ENTRYPOINT_SCOPE]: group, hierarchy, open, and persistence

Each is a module-level factory; the return varies, so it rides the surface signature.

| [INDEX] | [SURFACE]                                                                      | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `create_group(store, *, path, overwrite=False, zarr_format=3) -> Group`        | create an empty group            |
|  [02]   | `create_hierarchy(*, store, nodes, overwrite=False) -> Iterator[Array\|Group]` | bulk-create nodes                |
|  [03]   | `group(store, *, overwrite=False, chunk_store, zarr_format) -> Group`          | open-or-create group             |
|  [04]   | `open(store, *, mode, zarr_format, path) -> Array\|Group`                      | open array or group              |
|  [05]   | `open_array(store, *, zarr_format, path='', mode) -> Array`                    | open array                       |
|  [06]   | `open_group(store, *, mode, cache_attrs, path) -> Group`                       | open group                       |
|  [07]   | `open_consolidated(*args, use_consolidated=True) -> Group`                     | open via consolidated metadata   |
|  [08]   | `save(store, *args, zarr_format, path) -> None`                                | persist arrays                   |
|  [09]   | `save_array(store, arr, *, zarr_format, path) -> None`                         | persist one array                |
|  [10]   | `save_group(store, *args, zarr_format, path) -> None`                          | persist a group                  |
|  [11]   | `load(store, *, path, zarr_format) -> NDArrayLike\|dict`                       | load into memory                 |
|  [12]   | `consolidate_metadata(store, *, path, zarr_format) -> Group`                   | merge node metadata to root      |
|  [13]   | `copy` / `copy_all` / `copy_store`                                             | copy utilities returning counts  |
|  [14]   | `tree(grp, *, expand, level) -> TreeRepr` / `print_debug_info()`               | subtree / environment inspection |

[ENTRYPOINT_SCOPE]: async mirror (`zarr.api.asynchronous`)

Every synchronous factory wraps an `async def` of the same name in `zarr.api.asynchronous`; reach it directly inside an `await` context to skip the sync bridge's loop hop. `create_array`/`create_hierarchy`/`from_array` resolve their async work through `AsyncArray.create`/`AsyncGroup.create_hierarchy`, not a same-named `zarr.api.asynchronous` function.

[ENTRYPOINT_SCOPE]: Array operations

`Array` indexing splits into the bracket/`oindex`/`vindex`/`blocks` accessors and the explicit `get_*_selection`/`set_*_selection` read/write family; mutation, configuration, and metadata round out the surface.
- call: `get_*_selection(selection, *, out=None, fields=None, prototype=None)` and `set_*_selection(selection, value, *, fields=None, prototype=None)` cover basic/orthogonal/coordinate/mask/block

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `Array[selection]` / `Array[selection] = value`                      | operator | NumPy-compatible basic indexing               |
|  [02]   | `Array.oindex[selection]`                                            | property | orthogonal (outer) indexing                   |
|  [03]   | `Array.vindex[selection]`                                            | property | vectorized (fancy) indexing                   |
|  [04]   | `Array.blocks[selection]`                                            | property | block-level chunk indexing                    |
|  [05]   | `Array.get_*_selection`                                              | instance | basic/orthogonal/coordinate/mask/block reads  |
|  [06]   | `Array.set_*_selection`                                              | instance | basic/orthogonal/coordinate/mask/block writes |
|  [07]   | `Array.append(data, axis=0)` / `Array.resize(new_shape)`             | instance | append / resize in store                      |
|  [08]   | `Array.update_attributes(new_attributes)`                            | instance | update user attributes                        |
|  [09]   | `Array.with_config(config)` / `Array.info` / `Array.info_complete()` | instance | per-array config, info report                 |
|  [10]   | `Array.nchunks` / `nchunks_initialized` / `nbytes_stored()`          | property | chunk counts and stored-byte size             |
|  [11]   | `Array.chunks` / `shards` / `cdata_shape`                            | property | chunk-grid and shard shape                    |
|  [12]   | `Array.metadata` / `store_path` / `path`                             | property | node metadata and store path                  |
|  [13]   | `Array.compressors` / `serializer` / `filters`                       | property | resolved codec pipeline                       |

[ENTRYPOINT_SCOPE]: Group operations

`Group` creates and requires child arrays and groups, enumerates the subtree, and mutates or inspects it.
- call: `Group.create_array(name, *, shape, dtype)`; `create_group(name)`; `require_array(name, *, shape)`; `require_group(name)`; `require_groups(*names)`; `members(max_depth=0, *, use_consolidated_for_children=True)`

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Group.create_array` / `Group.create_group`                                       | instance | create child array / group        |
|  [02]   | `Group.create_hierarchy(nodes, *, overwrite=False)`                               | instance | create multiple children          |
|  [03]   | `Group.require_array` / `require_group` / `require_groups`                        | instance | open-or-create child              |
|  [04]   | `Group[path]` / `Group.get(path, default=None)`                                   | operator | access child by path              |
|  [05]   | `Group.keys()` / `Group.members(...)`                                             | instance | list child keys / full subtree    |
|  [06]   | `Group.arrays()` / `Group.groups()` / `Group.array_keys()` / `Group.group_keys()` | instance | iterate child arrays / sub-groups |
|  [07]   | `Group.move(source, dest)`                                                        | instance | rename or relocate child          |
|  [08]   | `Group.update_attributes(new_attributes)`                                         | instance | update user attributes            |
|  [09]   | `Group.tree(expand=None, level=None)`                                             | instance | render subtree as `TreeRepr`      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- async axis: one operation binds the sync facade OR the async mirror, never parallel sync/async call sites for one write; inside `await`, `zarr.api.asynchronous.*` or `AsyncArray`/`AsyncGroup` skip the sync wrapper's loop hop.
- format axis: Zarr v3 is the default (`zarr_format=3`) and v2 round-trips through `zarr_format=2`; `ShardingCodec`, `dimension_names`, and the explicit `[transform, serializer, compressor]` chain are v3-only, where v2 carries the single `compressor=`/`filters=` numcodecs shape.
- codec axis: `create_array(filters=, compressors=, serializer=)` declares the pipeline explicitly; `BytesCodec` serializes and `ShardingCodec` nests an inner chain per sub-chunk to cut small-file counts on object stores, and the `config['codecs']` registry routes any filter or compressor `zarr.codecs` lacks to `numcodecs`.
- indexing axis: `__getitem__`/`__setitem__` implement NumPy basic indexing only; `oindex` is orthogonal, `vindex` vectorized, `blocks` block-level, and the `get_*_selection`/`set_*_selection` family covers basic/orthogonal/coordinate/mask/block with `out`/`fields`/`prototype` control.
- store axis: any `StoreLike` resolves to one store constructed once outside hot paths and passed in; `WrapperStore`/`LoggingStore` decorate an existing store, and `GpuMemoryStore` pairs with `config.enable_gpu()`.
- config axis: `zarr.config` is a donfig `DConfig` singleton — `config.set({...})`, `config.get(key)`, `config.enable_gpu()`, `config.reset()` — carrying `default_zarr_format`, `array.order`, `array.write_empty_chunks`, `async.concurrency`, `async.timeout`, `threading.max_workers`, `codec_pipeline.batch_size`, and the `codecs` registry; `Array.with_config()` overrides per array.
- metadata axis: `consolidate_metadata(store)` merges every node metadata document into one root `zarr.json`/`.zmetadata` read back through `open_consolidated`, run after bulk remote writes before read-only opens.

[STACKING]:
- `icechunk`(`.api/icechunk.md`): `repo.writable_session(branch).store` is an `IcechunkStore` (a `zarr.abc.store.Store`) passed straight into `zarr.open_group(store=)`/`create_array(store=)`; zarr writes chunks, icechunk owns commit/branch/snapshot.
- `tensorstore`(`.api/tensorstore.md`): zarr writes the v3 on-disk format `tensorstore` opens with the `zarr3` driver over an identical chunk/codec layout, so a zarr-written store reads through tensorstore with no conversion.
- `virtualizarr`(`.api/virtualizarr.md`): `ManifestArray` chunk-reference manifests over existing files write to an `IcechunkStore` or kerchunk reference store, and the virtual `xarray.Dataset` reads through the same zarr store protocol without copying source bytes.
- `cubed`(`.api/cubed.md`)/`xarray`(`libs/python/.api/xarray.md`): `xarray.open_zarr`/`Dataset.to_zarr` and `cubed.Array.to_zarr` target a zarr store, so the labelled-cube and bounded-memory chunked-compute layers persist through this owner.
- `numcodecs`(`.api/numcodecs.md`): the filter and alternate-compressor roster binds through `zarr.codecs.numcodecs.<Codec>`, the base family fixing the slot — `ArrayArrayCodec` into `filters=`, `ArrayBytesCodec` as `serializer=`, `BytesBytesCodec` into `compressors=`; admit a numcodecs codec only where `zarr.codecs` lacks the equivalent.
- `obstore`(`libs/python/.api/obstore.md`): `zarr.storage.ObjectStore` wraps an `obstore` store (`S3Store`/`GCSStore`/`AzureStore`), credentials and zero-copy `Bytes` staying in the obstore owner while zarr sees only a `StoreLike`; `FsspecStore.from_url` is the alternate fsspec-backed remote path.
- within-lib: one `StoreLike` resolved once threads through every array and group factory, decorated via `WrapperStore`/`LoggingStore`, while the explicit three-slot codec chain and the `oindex`/`vindex`/`blocks` accessors compose against that single store without a per-backend or per-mode factory family.

[LOCAL_ADMISSION]:
- `create_array` mints typed v3 arrays and `create_group`/`create_hierarchy` mint hierarchies; `open`/`open_array`/`open_group` bind existing stores — one polymorphic factory per concept.
- `filters=`/`compressors=`/`serializer=` declare the codec pipeline on creation, and reproducible chunk encoding never rides `zarr_format`-dependent defaults.
- `oindex`/`vindex`/`blocks` cover non-basic selection; `__getitem__` stays NumPy basic indexing.
- one `StoreLike` resolves outside hot paths and threads across every factory, decorated rather than re-wrapped.
- `zarr.api.asynchronous`/`AsyncArray`/`AsyncGroup` serve `await` contexts, and one write binds sync or async.

[RAIL_LAW]:
- Package: `zarr`
- Owns: the chunked N-D array store with v2/v3 dual-format metadata, the explicit v3 codec pipeline, pluggable `zarr.storage` backends, orthogonal/vectorized/block/coordinate/mask indexing, consolidated metadata, the donfig `config`, and the synchronous-over-async `Array`/`AsyncArray` rail
- Accept: any `StoreLike` resolved once and passed in; an explicit `filters=`/`compressors=`/`serializer=` pipeline; `oindex`/`vindex`/`blocks` for non-basic selection; an `IcechunkStore`/`ObjectStore`/`FsspecStore` for transactional and cloud paths; the async mirror inside `await` contexts
- Reject: a `numcodecs` filter or compressor name addressed as though it lived in `zarr.codecs`; manual chunk-key construction outside the store API; direct metadata-file writes bypassing the codec/store path; a per-backend store factory family where one `StoreLike` discriminates; mixed sync/async call sites for one write; `zarr_format`-dependent default codecs where reproducibility is required
