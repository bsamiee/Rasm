# [PY_DATA_API_VIRTUALIZARR]

`virtualizarr` supplies virtual Zarr dataset construction over remote files for the data virtual-dataset rail. A `Parser` (one per source format) is called with a URL plus an `ObjectStoreRegistry` and yields a `ManifestStore` — a zarr-v3 read store whose arrays are `ManifestArray` chunk-reference views over a `ChunkManifest`; `ManifestStore.to_virtual_dataset` lifts that store into an xarray `Dataset` whose data variables are `ManifestArray`-backed (no bytes copied), while declared `loadable_variables` are eagerly read through the registry-resolved object store. The owner composes `open_virtual_dataset`/`open_virtual_mfdataset` (the `Parser` + `registry` + manifest path) with the registered `virtualize` accessor (`ds.virtualize.to_icechunk` / `to_kerchunk`) to write reference manifests; it never re-implements the manifest reference model, the parser dispatch, or the kerchunk/icechunk serializers virtualizarr already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `virtualizarr`
- package: `virtualizarr`
- import: `import virtualizarr as vz; from virtualizarr import open_virtual_dataset, open_virtual_datatree, open_virtual_mfdataset`
- owner: `data`
- rail: virtual-dataset
- installed: `2.6.2` reflected on cp315 (`python3.15`), rides the xarray `2026.4.0` / zarr `3.2.1` / obstore `0.10.1` floor; pure-Python, no native ABI of its own
- entry points: library use is import-only; no console script
- capability: virtual Zarr reference datasets — `ManifestStore` (zarr-v3 read store) construction from HDF5/NetCDF4/NetCDF3/Zarr-v2v3/FITS/DMRPP/kerchunk-JSON/kerchunk-Parquet/Icechunk via per-format `Parser`, registry-resolved `obstore` backends, eager `loadable_variables` materialization, multi-file concat/combine with serial/Dask/Lithops/process-pool executors, zarr-v2<->v3 codec-config bridging, and `virtualize` accessor export to Kerchunk (dict/json/parquet) and Icechunk reference stores

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: manifest model and read store
- rail: virtual-dataset

The manifest model is a three-level stack: a `ChunkManifest` maps chunk keys to `{path, offset, length}` `ChunkEntry` dicts; a `ManifestArray` wraps one manifest plus zarr-v3 `ArrayV3Metadata` as an Array-API-conformant lazy array; a `ManifestGroup` collects named `ManifestArray`s and a `ManifestStore` exposes that group as a zarr-v3 `Store` (the read path that `to_virtual_dataset` consumes).

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]   | [CAPABILITY]                                                            |
| :-----: | :---------------------------------------- | :--------------- | :--------------------------------------------------------------------- |
|  [01]   | `virtualizarr.manifests.ChunkEntry`       | reference dict   | one chunk's `{path, offset, length}`; `dict`-subclass, `with_validation` |
|  [02]   | `virtualizarr.manifests.ChunkManifest`    | chunk index      | map of chunk keys to `ChunkEntry`; structured-array store, path rewrite |
|  [03]   | `virtualizarr.manifests.ManifestArray`    | virtual array    | Array-API lazy array of chunk refs over a `ChunkManifest` + metadata   |
|  [04]   | `virtualizarr.manifests.ManifestGroup`    | manifest group   | named `ManifestArray`s plus group attrs under one zarr group           |
|  [05]   | `virtualizarr.manifests.ManifestStore`    | zarr read store  | zarr-v3 `Store` over a `ManifestGroup`; the `to_virtual_dataset` source |

[PUBLIC_TYPE_SCOPE]: `ManifestArray` Array-API surface
- rail: virtual-dataset

`ManifestArray` is Array-API-conformant: it carries zarr-v3 metadata and supports dtype cast, path rewrite, and lift to an xarray `Variable` without materializing bytes. The owner reads its shape/chunks/dtype for receipts and lifts it with `to_virtual_variable`.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE] | [CAPABILITY]                                       |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `ManifestArray.manifest`          | property       | underlying `ChunkManifest`                         |
|  [02]   | `ManifestArray.metadata`          | property       | zarr-v3 `ArrayV3Metadata` (dtype/chunks/codecs/fill) |
|  [03]   | `ManifestArray.shape`             | property       | array shape tuple                                  |
|  [04]   | `ManifestArray.chunks`            | property       | chunk shape tuple                                  |
|  [05]   | `ManifestArray.dtype`             | property       | array dtype                                        |
|  [06]   | `ManifestArray.ndim`              | property       | dimension count                                    |
|  [07]   | `ManifestArray.size`              | property       | logical element count                              |
|  [08]   | `ManifestArray.nbytes_virtual`    | property       | summed reference byte length (manifest weight)     |
|  [09]   | `ManifestArray.astype(dtype, /, *, copy=True)` | cast | dtype-cast view returning a new `ManifestArray`    |
|  [10]   | `ManifestArray.rename_paths(new)` | mutate         | copy with rewritten chunk paths (`str` or `Callable[[str], str]`) |
|  [11]   | `ManifestArray.to_virtual_variable()` | lift       | xarray `Variable` wrapping the manifest array      |

[PUBLIC_TYPE_SCOPE]: `ChunkManifest` construction and iteration
- rail: virtual-dataset

`ChunkManifest(entries, shape=None, separator='.')` builds from a `{chunk_key: {path, offset, length}}` dict; `from_arrays` builds from parallel `paths`/`offsets`/`lengths` NumPy arrays (the parser fast path). Iteration yields only populated references.

| [INDEX] | [SYMBOL]                                            | [PACKAGE_ROLE] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `ChunkManifest.from_arrays(*, paths, offsets, lengths)` | factory    | build a manifest from parallel reference arrays       |
|  [02]   | `ChunkManifest.dict()`                              | export         | nested `{chunk_key: ChunkEntry}` dict                 |
|  [03]   | `ChunkManifest.get_entry(key)`                      | lookup         | `ChunkEntry` for one chunk key                        |
|  [04]   | `ChunkManifest.iter_refs()` / `iter_nonempty_paths()` | iterate      | populated `(key, entry)` / path iteration             |
|  [05]   | `ChunkManifest.rename_paths(new)`                   | mutate         | copy with rewritten paths                             |
|  [06]   | `ChunkManifest.nbytes`                              | property       | manifest in-memory weight                             |
|  [07]   | `ChunkManifest.shape_chunk_grid` / `ndim_chunk_grid` | property      | chunk-grid shape / rank                               |

[PUBLIC_TYPE_SCOPE]: parsers
- rail: virtual-dataset

Each `Parser` is a runtime-checkable `Protocol`: `__call__(url: str, registry: ObjectStoreRegistry) -> ManifestStore`. Construct a parser with its format options, then pass the instance (not a string) to `open_virtual_dataset`; the parser fans the URL through the registry-resolved object store and emits the `ManifestStore`.

| [INDEX] | [SYMBOL]                                                                                              | [PACKAGE_ROLE] | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `parsers.HDFParser(group=None, drop_variables=None, reader_factory=BlockStoreReader)`                 | parser         | HDF5 and NetCDF4 files           |
|  [02]   | `parsers.NetCDF3Parser(group=None, skip_variables=None, reader_options=None)`                         | parser         | classic NetCDF3 files            |
|  [03]   | `parsers.ZarrParser(group=None, skip_variables=None)`                                                 | parser         | Zarr v2/v3 stores                |
|  [04]   | `parsers.DMRPPParser(group=None, skip_variables=None)`                                                | parser         | NASA DMR++ sidecar files         |
|  [05]   | `parsers.FITSParser(group=None, skip_variables=None, reader_options=None)`                            | parser         | FITS astronomical image files    |
|  [06]   | `parsers.KerchunkJSONParser(group=None, fs_root=None, skip_variables=None)`                           | parser         | kerchunk JSON reference files    |
|  [07]   | `parsers.KerchunkParquetParser(group=None, fs_root=None, skip_variables=None, reader_options=None)`   | parser         | kerchunk Parquet reference files |
|  [08]   | `parsers.IcechunkParser(*, branch=None, tag=None, snapshot_id=None, group=None, skip_variables=None, batch_size=100000)` | parser | Icechunk snapshot read |

[PUBLIC_TYPE_SCOPE]: registry, executors, codecs
- rail: virtual-dataset

`ObjectStoreRegistry` is the canonical scheme->store map; import it from `obspec_utils.registry` (the `virtualizarr` re-export is deprecated). The `parallel` module ships concrete executors for `open_virtual_mfdataset`. The `codecs` module bridges zarr-v2 and zarr-v3 codec configs, the boundary that lets a v2 source manifest write a v3 store.

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `obspec_utils.registry.ObjectStoreRegistry(stores=None)`            | registry       | scheme->`obstore` map; `register(url, store)` / `resolve(url) -> (store, path)` |
|  [02]   | `virtualizarr.parallel.SerialExecutor`                              | executor       | in-process serial mfdataset combine                    |
|  [03]   | `virtualizarr.parallel.DaskDelayedExecutor`                         | executor       | `dask.delayed` parallel combine                        |
|  [04]   | `virtualizarr.parallel.LithopsEagerFunctionExecutor`                | executor       | serverless (Lithops) parallel combine                  |
|  [05]   | `virtualizarr.parallel.get_executor(parallel)`                      | dispatch       | resolve `'dask'`/`'lithops'`/`False`/`type[Executor]` to an executor |
|  [06]   | `virtualizarr.codecs.convert_to_codec_pipeline(...)` / `get_codecs` | codec bridge   | build/extract a zarr-v3 `CodecPipeline` from manifest metadata |
|  [07]   | `virtualizarr.codecs.zarr_codec_config_to_v3` / `zarr_codec_config_to_v2` | codec bridge | convert zarr codec configs between v2 and v3            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: virtual dataset constructors
- rail: virtual-dataset

`url`/`urls`, `registry`, and `parser` are the three required positionals; `loadable_variables` selects the variables read eagerly (everything else stays a `ManifestArray`), and `decode_times` toggles CF time decoding. `open_virtual_mfdataset` adds the full `xarray.combine_*` vocabulary plus a `parallel` executor selector.

| [INDEX] | [SURFACE]                                                                                                                                                                                                            | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------- |
|  [01]   | `open_virtual_dataset(url, registry, parser, drop_variables=None, loadable_variables=None, decode_times=None) -> xr.Dataset`                                                                                          | open           | single virtual dataset     |
|  [02]   | `open_virtual_datatree(url, registry, parser, *, loadable_variables=None, decode_times=None) -> xr.DataTree`                                                                                                          | open           | virtual data tree          |
|  [03]   | `open_virtual_mfdataset(urls, registry, parser, concat_dim=None, compat='no_conflicts', preprocess=None, data_vars='all', coords='different', combine='by_coords', parallel=False, join='outer', attrs_file=None, combine_attrs='override', **kwargs) -> xr.Dataset` | open | multi-file virtual dataset |
|  [04]   | `ManifestStore.to_virtual_dataset(group='', loadable_variables=None, decode_times=None) -> xr.Dataset`                                                                                                                | lift           | manifest store -> dataset  |
|  [05]   | `ManifestStore.to_virtual_datatree(group='', *, loadable_variables=None, decode_times=None) -> xr.DataTree`                                                                                                           | lift           | manifest store -> datatree |

[ENTRYPOINT_SCOPE]: `virtualize` accessor exports
- rail: virtual-dataset

The accessor is registered on the `virtualize` name; `ds.virtualize.to_icechunk(...)` and `ds.virtualize.to_kerchunk(...)` are the only write surfaces. There is no accessor `to_zarr` (writing real bytes is out of scope); `to_icechunk` writes virtual chunk references into an `IcechunkStore`, `to_kerchunk` emits a reference document.

| [INDEX] | [SURFACE]                                                                                                              | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `ds.virtualize.to_icechunk(store, *, group=None, append_dim=None, region=None, validate_containers=True, last_updated_at=None) -> None` | export | write references to Icechunk |
|  [02]   | `ds.virtualize.to_kerchunk(filepath=None, format='dict', record_size=100000, categorical_threshold=10) -> KerchunkStoreRefs \| None` | export | write/return kerchunk references |
|  [03]   | `ds.virtualize.rename_paths(new) -> xr.Dataset`                                                                        | mutate         | rewrite chunk reference paths |
|  [04]   | `ds.virtualize.nbytes`                                                                                                 | metadata       | virtual dataset reference size |
|  [05]   | `dt.virtualize.to_icechunk(store, *, write_inherited_coords=False, validate_containers=True, last_updated_at=None, **kwargs) -> None` | export | write data tree to Icechunk |

## [04]-[IMPLEMENTATION_LAW]

[VIRTUAL_DATASET_TOPOLOGY]:
- import: `import virtualizarr as vz`; import `ObjectStoreRegistry` from `obspec_utils.registry` (the `virtualizarr` re-export is deprecation-flagged). Boundary scope only; module-level import is banned by the manifest import policy.
- parser axis: one `Parser` Protocol (`__call__(url, registry) -> ManifestStore`) owns format intake; the format is the parser instance row (`HDFParser`/`NetCDF3Parser`/`ZarrParser`/`DMRPPParser`/`FITSParser`/`KerchunkJSONParser`/`KerchunkParquetParser`/`IcechunkParser`), never a per-format `open_*` function family. `group`/`skip_variables`/`drop_variables`/`reader_options` are constructor rows on the parser.
- manifest axis: `ChunkManifest` (refs) -> `ManifestArray` (Array-API lazy array + zarr-v3 metadata) -> `ManifestGroup` -> `ManifestStore` (zarr-v3 read `Store`) is the single reference stack; build manifests via `ChunkManifest.from_arrays` (parallel `paths`/`offsets`/`lengths`), never hand-assembled per-chunk dicts. `ManifestArray` stores only `{path, offset, length}` `ChunkEntry` references; source bytes are never copied.
- read axis: `ManifestStore.to_virtual_dataset(loadable_variables=...)` is the lift to xarray; `loadable_variables` are materialized through the registry-resolved object store while every other variable stays a `ManifestArray`. `open_virtual_dataset` is the single-file convenience over this lift.
- registry axis: one `ObjectStoreRegistry` maps URL scheme to an `obstore` backend; `register(url, store)` then `resolve(url) -> (store, path)`. Share one registry across opens so credentials and connection pools are reused, never re-minted per URL.
- combine axis: `open_virtual_mfdataset` owns multi-file assembly with the full xarray `concat_dim`/`compat`/`data_vars`/`coords`/`combine`(`'by_coords'`/`'nested'`)/`join`/`combine_attrs` vocabulary; `parallel` selects the executor (`False` serial, `'dask'` -> `DaskDelayedExecutor`, `'lithops'` -> `LithopsEagerFunctionExecutor`, or a `type[Executor]`) via `get_executor`, never a hand-rolled fan-out.
- codec axis: `codecs.convert_to_codec_pipeline`/`zarr_codec_config_to_v3` bridge zarr-v2 source codecs into the v3 `CodecPipeline` carried by `ManifestArray.metadata`; this is the boundary that lets a v2-encoded source write a v3 manifest store, never a re-implemented codec.
- export axis: the registered `virtualize` accessor is the only write surface — `to_icechunk` (append_dim/region for incremental writes, `validate_containers` for store-integrity) and `to_kerchunk` (`format` in `'dict'`/`'json'`/`'parquet'`). There is no `to_zarr`; writing materialized bytes is out of scope.
- evidence: each open captures parser kind, registry scheme set, variable count, loadable-vs-virtual split, summed `nbytes_virtual`, chunk-grid shape, and export format/target as a virtual-dataset receipt.
- boundary: virtualizarr owns the manifest reference model, parser dispatch, and kerchunk/icechunk serialization; `obstore` owns byte fetch via the registry; xarray owns the `Dataset`/`DataTree` algebra; zarr owns the codec/metadata model bridged through `codecs`. Real-byte materialization, CRS handling, and raster analytics route to their own owners.

[RAIL_LAW]:
- Package: `virtualizarr`
- Owns: virtual Zarr `ManifestStore` construction, `Parser`-Protocol format dispatch, the `ChunkManifest`/`ManifestArray`/`ManifestGroup`/`ManifestStore` reference stack, registry-resolved eager `loadable_variables`, executor-driven multi-file combine, zarr-v2<->v3 codec-config bridging, and `virtualize` accessor export to Kerchunk and Icechunk
- Accept: remote files referenced by URL with an explicit `Parser` instance and an `obspec_utils` `ObjectStoreRegistry`; manifests lifted via `to_virtual_dataset` and exported via accessor methods
- Reject: data-copying ingest where virtual reference applies; hand-rolled kerchunk reference builders or per-chunk manifest dicts where `ChunkManifest.from_arrays` applies; a per-format `open_*` family where one `Parser` Protocol discriminates; direct parser internals; the deprecated `virtualizarr.ObjectStoreRegistry` import where `obspec_utils.registry` is canonical
