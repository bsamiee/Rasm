# [PY_DATA_API_VIRTUALIZARR]

`virtualizarr` mints virtual Zarr datasets over remote archival files for the data virtual-dataset rail: a per-format `Parser` reads a URL through an `ObjectStoreRegistry` into a `ManifestStore` — a zarr-v3 read store of `ManifestArray` chunk-reference views — that `to_virtual_dataset` lifts into an xarray `Dataset` with no bytes copied, then the `virtualize` accessor exports to Icechunk or Kerchunk. `virtualizarr` owns the manifest reference model, parser dispatch, and reference serialization; byte fetch is the registry-resolved object store's, the `Dataset`/`DataTree` algebra xarray's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `virtualizarr`
- package: `virtualizarr`
- module: `virtualizarr`
- namespaces: `virtualizarr.manifests`, `virtualizarr.parsers`, `virtualizarr.parallel`, `virtualizarr.codecs`
- rail: virtual-dataset
- capability: `Parser`-dispatched `ManifestStore` construction over registry-resolved object stores, eager `loadable_variables` materialization, executor-driven multi-file combine, zarr-v2↔v3 codec-config bridging, and `virtualize` accessor export to Kerchunk and Icechunk

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: manifest reference stack and read store

`ChunkManifest` → `ManifestArray` → `ManifestGroup` → `ManifestStore` is the one reference stack: chunk refs carry zarr-v3 metadata up to a zarr-v3 `Store` that `to_virtual_dataset` consumes.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]   | [CAPABILITY]                                                             |
| :-----: | :------------------------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `virtualizarr.manifests.ChunkEntry`    | reference dict  | one chunk's `{path, offset, length}`; `dict`-subclass, `with_validation` |
|  [02]   | `virtualizarr.manifests.ChunkManifest` | chunk index     | map of chunk keys to `ChunkEntry`; structured-array store, path rewrite  |
|  [03]   | `virtualizarr.manifests.ManifestArray` | virtual array   | Array-API lazy array of chunk refs over a `ChunkManifest` + metadata     |
|  [04]   | `virtualizarr.manifests.ManifestGroup` | manifest group  | named `ManifestArray`s plus group attrs under one zarr group             |
|  [05]   | `virtualizarr.manifests.ManifestStore` | zarr read store | zarr-v3 `Store` over a `ManifestGroup`; the `to_virtual_dataset` source  |

[PUBLIC_TYPE_SCOPE]: `ManifestArray` Array-API surface

`ManifestArray` is Array-API-conformant over zarr-v3 metadata, casting dtype, rewriting chunk paths, and lifting to an xarray `Variable` without materializing bytes. Standard Array-API scalars: `shape`, `ndim`, `size`, `dtype`.

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `ManifestArray.manifest`               | property      | underlying `ChunkManifest`                                        |
|  [02]   | `ManifestArray.metadata`               | property      | zarr-v3 `ArrayV3Metadata` (dtype/chunks/codecs/fill)              |
|  [03]   | `ManifestArray.nbytes_virtual`         | property      | summed reference byte length (manifest weight)                    |
|  [04]   | `ManifestArray.astype(dtype, *, copy)` | cast          | dtype-cast view returning a new `ManifestArray`                   |
|  [05]   | `ManifestArray.rename_paths(new)`      | mutate        | copy with rewritten chunk paths (`str` or `Callable[[str], str]`) |
|  [06]   | `ManifestArray.to_virtual_variable()`  | lift          | xarray `Variable` wrapping the manifest array                     |

[PUBLIC_TYPE_SCOPE]: `ChunkManifest` construction and iteration

`ChunkManifest(entries, shape=None, separator='.')` builds from a `{chunk_key: {path, offset, length}}` dict; `from_arrays` builds from parallel NumPy `paths`/`offsets`/`lengths` (the parser fast path), and iteration yields only populated references.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------ | :------------ | :---------------------------------------------- |
|  [01]   | `ChunkManifest.from_arrays(*, paths, offsets, lengths)` | factory       | build a manifest from parallel reference arrays |
|  [02]   | `ChunkManifest.dict()`                                  | export        | nested `{chunk_key: ChunkEntry}` dict           |
|  [03]   | `ChunkManifest.get_entry(key)`                          | lookup        | `ChunkEntry` for one chunk key                  |
|  [04]   | `ChunkManifest.iter_refs()` / `iter_nonempty_paths()`   | iterate       | populated `(key, entry)` / path iteration       |
|  [05]   | `ChunkManifest.rename_paths(new)`                       | mutate        | copy with rewritten paths                       |
|  [06]   | `ChunkManifest.nbytes`                                  | property      | manifest in-memory weight                       |
|  [07]   | `ChunkManifest.shape_chunk_grid` / `ndim_chunk_grid`    | property      | chunk-grid shape / rank                         |

[PUBLIC_TYPE_SCOPE]: parsers

Each `Parser` is a runtime-checkable `Protocol` — `__call__(url, registry) -> ManifestStore`; construct one with its format options and pass the instance (never a format string) to `open_virtual_dataset`, which fans the URL through the registry-resolved object store into a `ManifestStore`.

| [INDEX] | [SYMBOL]                                                                                 | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `parsers.HDFParser(group, drop_variables, reader_factory)`                               | HDF5 and NetCDF4 files           |
|  [02]   | `parsers.NetCDF3Parser(group, skip_variables, reader_options)`                           | classic NetCDF3 files            |
|  [03]   | `parsers.ZarrParser(group, skip_variables)`                                              | Zarr v2/v3 stores                |
|  [04]   | `parsers.DMRPPParser(group, skip_variables)`                                             | NASA DMR++ sidecar files         |
|  [05]   | `parsers.FITSParser(group, skip_variables, reader_options)`                              | FITS astronomical image files    |
|  [06]   | `parsers.KerchunkJSONParser(group, fs_root, skip_variables)`                             | kerchunk JSON reference files    |
|  [07]   | `parsers.KerchunkParquetParser(group, fs_root, skip_variables, reader_options)`          | kerchunk Parquet reference files |
|  [08]   | `parsers.IcechunkParser(*, branch, tag, snapshot_id, group, skip_variables, batch_size)` | Icechunk snapshot read           |

[PUBLIC_TYPE_SCOPE]: registry, executors, codecs

`ObjectStoreRegistry` (from `obspec_utils.registry`) is the canonical scheme→store map; `parallel` ships concrete executors for `open_virtual_mfdataset`, and `codecs` bridges zarr-v2 and zarr-v3 codec configs — the boundary that lets a v2 source manifest write a v3 store.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------------------ | :------------ | :------------------------------------------------------------- |
|  [01]   | `ObjectStoreRegistry(stores)`                     | registry      | canonical URL-scheme→`obstore` store map, `register`/`resolve` |
|  [02]   | `parallel.SerialExecutor`                         | executor      | in-process serial mfdataset combine                            |
|  [03]   | `parallel.DaskDelayedExecutor`                    | executor      | `dask.delayed` parallel combine                                |
|  [04]   | `parallel.LithopsEagerFunctionExecutor`           | executor      | serverless (Lithops) parallel combine                          |
|  [05]   | `parallel.get_executor(parallel)`                 | dispatch      | resolve `'dask'`/`'lithops'`/`False`/`type[Executor]`          |
|  [06]   | `codecs.convert_to_codec_pipeline` / `get_codecs` | codec bridge  | build/extract a zarr-v3 `CodecPipeline` from manifest metadata |
|  [07]   | `codecs.zarr_codec_config_to_v3`                  | codec bridge  | convert a zarr-v2 codec config to v3                           |
|  [08]   | `codecs.zarr_codec_config_to_v2`                  | codec bridge  | convert a zarr-v3 codec config to v2                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: virtual dataset constructors

`url`/`urls`, `registry`, and `parser` are the three required positionals; the open family shares `drop_variables`, `loadable_variables` (read eagerly through the registry-resolved store, everything else staying a `ManifestArray`), and `decode_times` (CF time decoding).
- combine carry: `open_virtual_mfdataset` adds the xarray combine keywords `concat_dim`, `compat`, `preprocess`, `data_vars`, `coords`, `combine`, `parallel`, `join`, `attrs_file`, `combine_attrs`

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `open_virtual_dataset(url, registry, parser, *, loadable_variables)`  | function | single-file virtual `Dataset` |
|  [02]   | `open_virtual_datatree(url, registry, parser, *, loadable_variables)` | function | virtual `DataTree`            |
|  [03]   | `open_virtual_mfdataset(urls, registry, parser, *, combine)`          | function | multi-file virtual `Dataset`  |
|  [04]   | `ManifestStore.to_virtual_dataset(group, *, loadable_variables)`      | method   | manifest store → `Dataset`    |
|  [05]   | `ManifestStore.to_virtual_datatree(group, *, loadable_variables)`     | method   | manifest store → `DataTree`   |

[ENTRYPOINT_SCOPE]: `virtualize` accessor exports

`ds.virtualize` is the only write surface: `to_icechunk` writes virtual chunk references into an `IcechunkStore`, `to_kerchunk` emits a reference document, and no `to_zarr` exists since materializing real bytes is out of scope.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `ds.virtualize.to_icechunk(store, *, append_dim, region, validate_containers)` | method   | write references to Icechunk     |
|  [02]   | `ds.virtualize.to_kerchunk(filepath, format, record_size)`                     | method   | write/return kerchunk references |
|  [03]   | `ds.virtualize.rename_paths(new)`                                              | method   | rewrite chunk reference paths    |
|  [04]   | `ds.virtualize.nbytes`                                                         | property | virtual dataset reference size   |
|  [05]   | `dt.virtualize.to_icechunk(store, *, write_inherited_coords)`                  | method   | write data tree to Icechunk      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Parser axis: one `Parser` Protocol (`__call__(url, registry) -> ManifestStore`) owns format intake; the format is a parser-instance row, never a per-format `open_*` family, and `group`/`skip_variables`/`drop_variables`/`reader_options` are constructor rows on the parser.
- Manifest axis: `ChunkManifest` → `ManifestArray` → `ManifestGroup` → `ManifestStore` is the one reference stack; build manifests through `ChunkManifest.from_arrays` over parallel `paths`/`offsets`/`lengths`, storing only `{path, offset, length}` refs — source bytes never copy.
- Read axis: `ManifestStore.to_virtual_dataset` lifts to xarray with `loadable_variables` materialized through the registry-resolved store while every other variable stays a `ManifestArray`; `open_virtual_dataset` is the single-file convenience over the lift.
- Combine axis: `open_virtual_mfdataset` owns multi-file assembly over the xarray `concat`/`combine`/`join` vocabulary, and `get_executor` resolves `parallel` to a serial, Dask, Lithops, or `type[Executor]` runner, never a hand-rolled fan-out.
- Codec axis: `codecs.convert_to_codec_pipeline`/`zarr_codec_config_to_v3` bridge zarr-v2 source codecs into the v3 `CodecPipeline` on `ManifestArray.metadata` — the boundary that lets a v2-encoded source write a v3 manifest store.
- Export axis: the registered `virtualize` accessor is the only write surface — `to_icechunk` (`append_dim`/`region` for incremental writes, `validate_containers` for store integrity) and `to_kerchunk` (`format` in `dict`/`json`/`parquet`).

[STACKING]:
- `icechunk`(`.api/icechunk.md`): `ds.virtualize.to_icechunk(IcechunkStore)` writes the virtual chunk references into an icechunk session store, and `parsers.IcechunkParser` reads a branch/tag/snapshot back into a `ManifestStore`.
- `zarr`(`.api/zarr.md`): `ManifestStore` is a zarr-v3 `Store` and `ManifestArray.metadata` a zarr `ArrayV3Metadata`, so a `codecs`-bridged virtual store composes into every zarr array consumer.
- `obspec-utils`(`.api/obspec-utils.md`): one `ObjectStoreRegistry` resolves each URL scheme to an `obstore` backend, and every `Parser` reads bytes through that registry via the obspec `BlockStoreReader`.
- `xarray`(`libs/python/.api/xarray.md`): `to_virtual_dataset`/`to_virtual_datatree` lift a `ManifestStore` into an `xarray.Dataset`/`DataTree` whose virtual variables carry `ManifestArray` data, joining the lazy xarray rail.
- within-lib: `open_virtual_mfdataset` composes a `Parser`, one shared `ObjectStoreRegistry`, and `get_executor`-selected parallelism into a combined virtual `Dataset` the `virtualize` accessor then exports; sharing one registry across opens reuses credentials and connection pools.

[LOCAL_ADMISSION]:
- Import `ObjectStoreRegistry` from `obspec_utils.registry`, and pass a constructed `Parser` instance carrying its format options to `open_virtual_dataset`.
- Build manifests through `ChunkManifest.from_arrays`, export through the `virtualize` accessor, and share one registry across every open.

[RAIL_LAW]:
- Package: `virtualizarr`
- Owns: virtual Zarr `ManifestStore` construction, `Parser`-Protocol format dispatch, the `ChunkManifest`/`ManifestArray`/`ManifestGroup`/`ManifestStore` reference stack, registry-resolved eager `loadable_variables`, executor-driven multi-file combine, zarr-v2↔v3 codec-config bridging, and `virtualize` accessor export to Kerchunk and Icechunk
- Accept: remote files referenced by URL with a `Parser` instance and an `obspec_utils` `ObjectStoreRegistry`; manifests lifted via `to_virtual_dataset` and exported via accessor methods
- Reject: data-copying ingest where virtual reference applies; a hand-rolled kerchunk builder or per-chunk manifest dict where `ChunkManifest.from_arrays` applies; a per-format `open_*` family where one `Parser` Protocol discriminates
