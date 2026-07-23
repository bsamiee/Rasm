# [PY_DATA_API_NETCDF4]

`netCDF4` binds the Unidata netCDF-4 C library: hierarchical group/dimension/variable containers, CF time conversion through `cftime`, multi-file aggregation, MPI-collective I/O, in-memory datasets, and HDF5 compression/quantization filters. `xarray.open_dataset(engine="netcdf4")` is the `data` field-dataset CF entry, and this owner is reached directly for low-level CF metadata, structure authoring, and byte-payload round-trips `xarray` does not expose.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `netCDF4`
- package: `netcdf4` (MIT)
- import: `import netCDF4`
- owner: `data`
- rail: field-dataset — the CF reader engine over netCDF-4/HDF5
- asset: C extension `_netCDF4` links netCDF-C over HDF5; blosc/bzip2/zstd/szip filter plugins ship bundled, gated to whichever the Forge build enabled
- capability: netCDF-4/HDF5 file I/O, group/dimension/variable authoring, CF time conversion (`cftime`), multi-file aggregation, MPI-collective I/O, in-memory datasets, and compression/quantization filters

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and group containers

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :---------- | :------------ | :------------------------------------------------- |
|  [01]   | `Dataset`   | class         | root netCDF file handle; create/read/write         |
|  [02]   | `Group`     | class         | named sub-group; same surface as `Dataset`         |
|  [03]   | `MFDataset` | class         | multi-file read-only aggregation view; Dataset API |
|  [04]   | `MFTime`    | class         | time-axis wrapper for multi-file CF time decoding  |

[PUBLIC_TYPE_SCOPE]: dimension and variable

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------- | :------------ | :-------------------------------------------- |
|  [01]   | `Dimension` | class         | named dimension with optional unlimited flag  |
|  [02]   | `Variable`  | class         | n-D data array with attributes, masks, scales |

[PUBLIC_TYPE_SCOPE]: compound and custom types

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `CompoundType`                   | class         | user-defined structured (compound) dtype       |
|  [02]   | `VLType`                         | class         | variable-length (vlen) type                    |
|  [03]   | `EnumType`                       | class         | enumeration type with dict mapping             |
|  [04]   | `NetCDF4MissingFeatureException` | exception     | raised when an HDF5/netCDF-4 feature is absent |

[Dataset members]: `dimensions` `variables` `groups` `cmptypes` `vltypes` `enumtypes` `data_model` `disk_format` `file_format` `name` `parent` `path`

[Variable members]: `dimensions` `dtype` `datatype` `ndim` `shape` `size` `mask` `scale` `always_mask` `name`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Dataset lifecycle — the ctor, the `fromcdl` factory, and `Dataset` instance methods
- ctor carry: `mode`, `clobber`, `format`, `diskless`, `persist`, `memory`, `keepweakref`, `parallel`, `comm`, `info`, `auto_complex`

Ctor kwarg groups: `diskless`/`persist`/`memory` open an in-memory dataset, `parallel`/`comm`/`info` an MPI-collective handle, and `auto_complex` reconstructs complex variables.

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------- |
|  [01]   | `Dataset(filename, mode='r', ...)`                                  | ctor     | open/create the file handle      |
|  [02]   | `close()`                                                           | instance | flush and close the file         |
|  [03]   | `sync()`                                                            | instance | flush pending writes to disk     |
|  [04]   | `filepath(encoding=None)`                                           | instance | resolve on-disk path             |
|  [05]   | `isopen()`                                                          | instance | file still open                  |
|  [06]   | `fromcdl(cdlfilename, ncfilename=None, mode='a', format='NETCDF4')` | factory  | build a Dataset from CDL text    |
|  [07]   | `tocdl(coordvars=False, data=False, outfile=None)`                  | instance | dump the CDL text representation |

[ENTRYPOINT_SCOPE]: structure creation — `Dataset`/`Group` instance methods
- createVariable codec carry: `compression`, `zlib`, `complevel`, `shuffle`, `szip_coding`, `szip_pixels_per_block`, `blosc_shuffle`, `fletcher32`, `contiguous`, `chunksizes`, `endian`, `least_significant_digit`, `significant_digits`, `quantize_mode`, `fill_value`, `chunk_cache`

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------ | :----------------------------------------------------------------- |
|  [01]   | `createDimension(dimname, size=None)`                   | define dimension (unlimited when `size=None`)                      |
|  [02]   | `createVariable(varname, datatype, dimensions=(), ...)` | define and return a `Variable`; codec carry drives the filter band |
|  [03]   | `createGroup(groupname)`                                | create nested group                                                |
|  [04]   | `createCompoundType(datatype, datatype_name)`           | register compound dtype                                            |
|  [05]   | `createEnumType(datatype, datatype_name, enum_dict)`    | register enum type                                                 |
|  [06]   | `createVLType(datatype, datatype_name)`                 | register vlen type                                                 |

[ENTRYPOINT_SCOPE]: attributes, rename, query — `Dataset`/`Group` instance methods

| [INDEX] | [SURFACE]                               | [CAPABILITY]                  |
| :-----: | :-------------------------------------- | :---------------------------- |
|  [01]   | `ncattrs()`                             | list attribute names          |
|  [02]   | `getncattr(name, encoding='utf-8')`     | get one attribute             |
|  [03]   | `setncattr(name, value)`                | set one attribute             |
|  [04]   | `setncattr_string(name, value)`         | force string attribute type   |
|  [05]   | `setncatts(attdict)`                    | set multiple attributes       |
|  [06]   | `delncattr(name)`                       | delete attribute              |
|  [07]   | `renameVariable(oldname, newname)`      | rename variable               |
|  [08]   | `renameDimension(oldname, newname)`     | rename dimension              |
|  [09]   | `renameGroup(oldname, newname)`         | rename group                  |
|  [10]   | `renameAttribute(oldname, newname)`     | rename global attribute       |
|  [11]   | `get_variables_by_attributes(**kwargs)` | filter variables by attribute |

[ENTRYPOINT_SCOPE]: Variable operations — `Variable` instance methods

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `chunking()`                                                   | chunksizes tuple or `'contiguous'`                |
|  [02]   | `filters()`                                                    | active compression filters dict                   |
|  [03]   | `get_fill_value()`                                             | fill value                                        |
|  [04]   | `get_var_chunk_cache()`                                        | chunk cache settings                              |
|  [05]   | `set_var_chunk_cache(size=None, nelems=None, preemption=None)` | configure chunk cache                             |
|  [06]   | `set_auto_mask(mask)`                                          | toggle auto-masking                               |
|  [07]   | `set_auto_scale(scale)`                                        | toggle `scale_factor`/`add_offset`                |
|  [08]   | `set_auto_maskandscale(maskandscale)`                          | toggle mask and scale together                    |
|  [09]   | `set_always_mask(always_mask)`                                 | force masked-array return                         |
|  [10]   | `set_collective(value)`                                        | per-variable collective vs independent MPI access |
|  [11]   | `use_nc_get_vars(use_nc_get_vars)`                             | toggle strided `nc_get_vars` reads                |
|  [12]   | `get_dims()`                                                   | tuple of `Dimension` objects                      |
|  [13]   | `setncattr_string(name, value)`                                | force string attribute type                       |
|  [14]   | `renameAttribute(oldname, newname)`                            | rename variable attribute                         |
|  [15]   | `endian()`                                                     | endianness string                                 |
|  [16]   | `quantization()`                                               | quantization settings dict                        |

[ENTRYPOINT_SCOPE]: module-level time and utility functions — static functions; CF time delegates to `cftime`
- time carry: `units`, `calendar`, `has_year_zero`

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `date2num(dates, units, calendar=None, ...)`                    | datetime objects to numeric values             |
|  [02]   | `num2date(times, units, calendar='standard', ...)`              | numeric values to datetime objects             |
|  [03]   | `date2index(dates, nctime, calendar=None, select='exact', ...)` | datetime to index in a time variable           |
|  [04]   | `stringtoarr(string, NUMCHARS, dtype='S')`                      | string to fixed-length char array              |
|  [05]   | `chartostring(b, encoding='utf-8')`                             | char array to string                           |
|  [06]   | `stringtochar(a, encoding='utf-8', n_strlen=None)`              | string array to char array                     |
|  [07]   | `getlibversion()`                                               | linked netCDF-C library version string         |
|  [08]   | `dtype_is_complex(dtype)`                                       | complex-dtype test, paired with `auto_complex` |
|  [09]   | `get_chunk_cache()`                                             | global HDF5 chunk cache settings               |
|  [10]   | `set_chunk_cache(size=None, nelems=None, preemption=None)`      | configure global HDF5 chunk cache              |
|  [11]   | `get_alignment()`                                               | HDF5 alignment parameters                      |
|  [12]   | `set_alignment(threshold, alignment)`                           | set HDF5 alignment                             |
|  [13]   | `rc_get(key)`                                                   | get runtime configuration value                |
|  [14]   | `rc_set(key, value)`                                            | set runtime configuration value                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Dataset` and `Group` share one method surface; `Group` is a child container obtained via `createGroup` or `groups[name]`.
- `MFDataset` aggregates files along an unlimited dimension (read-only, Dataset API); `MFTime` decodes a heterogeneous-calendar/units time axis across the aggregated files.
- `data_model`/`disk_format`/`file_format` report the format string (`NETCDF4`, `NETCDF4_CLASSIC`, `NETCDF3_64BIT`); `default_fillvals` maps dtype strings (`'f4'`, `'f8'`, `'i4'`) to fill values; `is_native_little`/`is_native_big` report host byte order.
- CF time decode delegates to `cftime`: `date2num`/`num2date`/`date2index` own the calendar vocabulary (`standard`, `noleap`, `360_day`, `julian`, `proleptic_gregorian`) and `has_year_zero` semantics; the rail never hand-parses CF unit strings.
- codec and parallel selection gate on module-level `__has_*` boolean flags read once at boundary scope, and `has_blosc_filter`/`has_bzip2_filter`/`has_szip_filter`/`has_zstd_filter` probe a Dataset's live filter availability before `createVariable(compression=...)`.
    - [BUILD_FLAGS]: `__has_blosc_support__` `__has_bzip2_support__` `__has_szip_support__` `__has_zstandard_support__` `__has_quantization_support__` `__has_ncfilter__` `__has_set_alignment__` `__has_nc_rc_set__` `__has_parallel_support__` `__has_parallel4_support__` `__has_pnetcdf_support__` `__has_cdf5_format__` `__has_nc_open_mem__` `__has_nc_create_mem__`
    - [CODECS]: `zlib` `szip` `blosc` `bzip2` `zstd`; lossy `quantize` via `quantize_mode='BitGroom'|'BitRound'|'GranularBitRound'`
- in-memory datasets ride `NC_DISKLESS`/`NC_PERSIST` with the `diskless`/`persist`/`memory` ctor kwargs; `close()` returns the buffer as a `memoryview` when `memory=` was supplied.
- an MPI-collective handle opens via `Dataset(..., parallel=True, comm=<mpi4py>, info=<mpi4py>)` under `__has_parallel_support__`, and `Variable.set_collective(True|False)` toggles per-variable access mode.

[STACKING]:
- `xarray`(`libs/python/.api/xarray.md`): `xarray.open_dataset(path, engine='netcdf4')` and `Dataset.to_netcdf(path, engine='netcdf4', encoding=...)` are the field-dataset open/write delegates; the decoded cube joins the lazy `xarray.Dataset` rail that `odc-stac`, `zarr`, and `tensorstore` also feed.
- `h5netcdf`(`.api/h5netcdf.md`): the peer CF engine over pure `h5py`; both bind `xarray.open_dataset(engine=...)`, and the netCDF-C quantization keys (`least_significant_digit`/`significant_digits`/`quantize_mode`) route here alone, having no h5py backing.
- `obstore`(`libs/python/.api/obstore.md`): `memory=`/`diskless=` write paired with `close() -> memoryview` emits a netCDF byte payload straight to the object-store `put` without a temp file; a fetched `Bytes` reads back through `Dataset('inmem', memory=<bytes>)`.
- field-dataset owner: this surface is reached directly for low-level CF metadata, group/dimension/variable authoring, MPI-collective write, and in-memory round-trips `xarray` does not expose; CF time flows through `date2num`/`num2date` into the `pandas`/`numpy` datetime index at the `xarray` boundary.

[LOCAL_ADMISSION]:
- Open `Dataset` as a context manager (`with netCDF4.Dataset(...) as ds:`) so `close()` flushes on exit.
- Route CF time through `date2num`/`num2date` with explicit `units` and `calendar`; `auto_complex=True` reconstructs paired real/imag complex variables, and `dtype_is_complex` classifies a dtype string.
- Variable reads return masked, scaled arrays; drop to `set_auto_mask(False)`/`set_auto_scale(False)`/`set_auto_maskandscale(False)` only where downstream owns fill values and `scale_factor`/`add_offset`.
- Gate every `compression=` codec and `parallel=True` on the matching `__has_*` flag, since the build links only the native filters and MPI the Forge toolchain enabled.

[RAIL_LAW]:
- Package: `netCDF4`
- Owns: netCDF-4/HDF5 file I/O, dimension/variable authoring, CF time conversion via `cftime`, multi-file aggregation, MPI-collective I/O, in-memory datasets, and compression/quantization filters
- Accept: context-manager open; `date2num`/`num2date` for CF time; `createVariable` with build-flag-gated compression and chunksizes; `memory=`/`diskless=` for object-store round-trips; `parallel=True` under `__has_parallel_support__`
- Reject: hand-rolled netCDF byte parsing; direct HDF5 manipulation netCDF4 already covers; hand-decoded CF unit strings `cftime` owns; a second engine path when `xarray` `engine='netcdf4'` already routes here
