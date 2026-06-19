# [PY_DATA_API_VIRTUALIZARR]

`virtualizarr` supplies virtual Zarr dataset construction over remote files for the data virtual-dataset rail. The package owner builds `ManifestArray`-backed xarray `Dataset` objects that reference existing HDF5, NetCDF, Zarr, FITS, DMRPP, and kerchunk files without copying data, and writes the resulting reference manifests to Kerchunk or Icechunk stores through the registered `virtualize` accessor; it never re-implements the manifest reference model or the underlying parsers.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `virtualizarr`
- package: `virtualizarr`
- import: `import virtualizarr as vz; from virtualizarr import open_virtual_dataset, open_virtual_datatree, open_virtual_mfdataset`
- owner: `data`
- rail: virtual-dataset
- capability: virtual Zarr reference datasets — manifest construction from HDF5/NetCDF/Zarr/FITS/DMRPP/kerchunk/icechunk, multi-file concatenation, and accessor export to Kerchunk and Icechunk reference stores

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: manifest types
- rail: virtual-dataset

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE] | [CAPABILITY]                                            |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------------------ |
|   [1]   | `virtualizarr.manifests.ManifestArray` | virtual array  | array of chunk references backed by a `ChunkManifest`   |
|   [2]   | `virtualizarr.manifests.ChunkManifest` | chunk index    | map of chunk keys to `path`, `offset`, `length` entries |

[PUBLIC_TYPE_SCOPE]: parsers
- rail: virtual-dataset

| [INDEX] | [SYMBOL]                                                                                              | [PACKAGE_ROLE] | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `virtualizarr.parsers.HDFParser(group, drop_variables, reader_factory)`                               | parser         | HDF5 and NetCDF4 files           |
|   [2]   | `virtualizarr.parsers.NetCDF3Parser(group, skip_variables, reader_options)`                           | parser         | classic NetCDF3 files            |
|   [3]   | `virtualizarr.parsers.ZarrParser(group, skip_variables)`                                              | parser         | Zarr v2/v3 stores                |
|   [4]   | `virtualizarr.parsers.DMRPPParser(group, skip_variables)`                                             | parser         | NASA DMRPP sidecar files         |
|   [5]   | `virtualizarr.parsers.FITSParser(group, skip_variables, reader_options)`                              | parser         | FITS astronomical image files    |
|   [6]   | `virtualizarr.parsers.KerchunkJSONParser(group, fs_root, skip_variables)`                             | parser         | kerchunk JSON reference files    |
|   [7]   | `virtualizarr.parsers.KerchunkParquetParser(group, fs_root, skip_variables, reader_options)`          | parser         | kerchunk Parquet reference files |
|   [8]   | `virtualizarr.parsers.IcechunkParser(*, branch, tag, snapshot_id, group, skip_variables, batch_size)` | parser         | Icechunk snapshot                |

[PUBLIC_TYPE_SCOPE]: ManifestArray properties
- rail: virtual-dataset

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE] | [CAPABILITY]                           |
| :-----: | :-------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `ManifestArray.manifest`          | property       | underlying `ChunkManifest`             |
|   [2]   | `ManifestArray.metadata`          | property       | Zarr v3 `ArrayV3Metadata`              |
|   [3]   | `ManifestArray.chunks`            | property       | chunk shape tuple                      |
|   [4]   | `ManifestArray.dtype`             | property       | array dtype                            |
|   [5]   | `ManifestArray.shape`             | property       | array shape tuple                      |
|   [6]   | `ManifestArray.rename_paths(new)` | mutate         | return copy with rewritten chunk paths |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: virtual dataset constructors
- rail: virtual-dataset

| [INDEX] | [SURFACE]                                                                                                            | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------- |
|   [1]   | `open_virtual_dataset(url, registry, parser, drop_variables, loadable_variables, decode_times) -> xr.Dataset`        | open           | single virtual dataset     |
|   [2]   | `open_virtual_datatree(url, registry, parser, *, loadable_variables, decode_times) -> xr.DataTree`                   | open           | virtual data tree          |
|   [3]   | `open_virtual_mfdataset(urls, registry, parser, concat_dim, compat, preprocess, combine, parallel, join) -> Dataset` | open           | multi-file virtual dataset |

[ENTRYPOINT_SCOPE]: xarray accessor exports
- rail: virtual-dataset

| [INDEX] | [SURFACE]                                                                                                            | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `VirtualiZarrDatasetAccessor.to_icechunk(store, *, group, append_dim, region, validate_containers, last_updated_at)` | export         | write to Icechunk store       |
|   [2]   | `VirtualiZarrDatasetAccessor.to_kerchunk(filepath, format, record_size, categorical_threshold)`                      | export         | write kerchunk reference file |
|   [3]   | `VirtualiZarrDatasetAccessor.rename_paths(new)`                                                                      | mutate         | rewrite chunk reference paths |
|   [4]   | `VirtualiZarrDatasetAccessor.nbytes`                                                                                 | metadata       | virtual dataset size in bytes |
|   [5]   | `VirtualiZarrDataTreeAccessor.to_icechunk(store, *, group, ...)`                                                     | export         | write data tree to Icechunk   |

## [4]-[IMPLEMENTATION_LAW]

[VIRTUAL_DATASET_TOPOLOGY]:
- no data copy: `ManifestArray` stores only chunk reference dicts (`path`, `offset`, `length`); actual bytes remain in the source files
- accessor: virtual datasets expose `ds.virtualize.to_icechunk(...)` and `ds.virtualize.to_kerchunk(...)` via the registered xarray accessor (`VirtualiZarrDatasetAccessor` bound to the `virtualize` name); there is no accessor `to_zarr` method
- parsers accept an `ObjectStoreRegistry` via `open_virtual_dataset`; the registry maps URL schemes to obstore backends
- `open_virtual_mfdataset` `parallel` accepts `False`, `"dask"`, `"lithops"`, or a custom `Executor` subclass
- `to_kerchunk` `format` values: `"dict"`, `"json"`, `"parquet"`
- `ChunkManifest` entries are `dict` of `{chunk_key: {"path": str, "offset": int, "length": int}}`

[RAIL_LAW]:
- Package: `virtualizarr`
- Owns: virtual Zarr manifest construction, multi-format parser dispatch, and accessor export to Kerchunk and Icechunk reference stores
- Accept: remote files referenced by URL with an explicit parser and ObjectStoreRegistry; manifests exported via accessor methods
- Reject: data-copying ingest where virtual reference applies; hand-rolled kerchunk reference builders; direct parser internals
