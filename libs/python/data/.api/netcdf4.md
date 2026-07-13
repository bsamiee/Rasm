# [PY_DATA_API_NETCDF4]

`netCDF4` supplies Python bindings to the Unidata netCDF-4 C library — hierarchical group/dimension/variable containers, CF-compliant time conversion, multi-file dataset aggregation, compression filters, and chunking controls. It is the `FieldDataset` CF reader engine — the first consumer of the admitted-but-previously-unconsumed `netcdf4` — bound through `xarray.open_dataset(engine="netcdf4")` and reachable directly for low-level CF metadata, dimension/variable creation, and CF time conversion.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `netCDF4`
- package: `netcdf4` (import name `netCDF4`)
- version: `1.7.4`
- license: MIT
- import: `import netCDF4`
- owner: `data`
- rail: field-dataset
- capability: the `FieldDataset` CF reader engine — netCDF-4/HDF5 file I/O via C extension — hierarchical groups, CF-compliant time conversion, multi-file aggregation, MPI-parallel collective I/O, compression filters, and chunking controls

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and group containers
- rail: field-dataset

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [ROLE]                                                       |
| :-----: | :---------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Dataset`   | class         | root netCDF file handle; create/read/write                   |
|  [02]   | `Group`     | class         | named sub-group within a Dataset (same API as Dataset)       |
|  [03]   | `MFDataset` | class         | multi-file aggregation view (read-only); same API as Dataset |
|  [04]   | `MFTime`    | class         | time-axis wrapper for multi-file CF time decoding            |

[PUBLIC_TYPE_SCOPE]: dimension and variable
- rail: field-dataset

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [ROLE]                                        |
| :-----: | :---------- | :------------ | :-------------------------------------------- |
|  [01]   | `Dimension` | class         | named dimension with optional unlimited flag  |
|  [02]   | `Variable`  | class         | n-D data array with attributes, masks, scales |

[PUBLIC_TYPE_SCOPE]: compound and custom types
- rail: field-dataset

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [ROLE]                                   |
| :-----: | :------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `CompoundType`                   | class         | user-defined structured (compound) dtype |
|  [02]   | `VLType`                         | class         | variable-length (vlen) type              |
|  [03]   | `EnumType`                       | class         | enumeration type with dict mapping       |
|  [04]   | `NetCDF4MissingFeatureException` | exception     | raised when HDF5/netCDF-4 feature absent |

[PUBLIC_TYPE_SCOPE]: Dataset members
- rail: field-dataset

| [INDEX] | [SYMBOL]      | [KIND]   | [ROLE]                                             |
| :-----: | :------------ | :------- | :------------------------------------------------- |
|  [01]   | `dimensions`  | property | mapping of dimension names to `Dimension`          |
|  [02]   | `variables`   | property | mapping of variable names to `Variable`            |
|  [03]   | `groups`      | property | mapping of group names to `Group`                  |
|  [04]   | `cmptypes`    | property | mapping of compound type names                     |
|  [05]   | `vltypes`     | property | mapping of vlen type names                         |
|  [06]   | `enumtypes`   | property | mapping of enum type names                         |
|  [07]   | `data_model`  | property | format string (`NETCDF4`, `NETCDF4_CLASSIC`, etc.) |
|  [08]   | `disk_format` | property | on-disk format string                              |
|  [09]   | `file_format` | property | file format string                                 |
|  [10]   | `name`        | property | group name                                         |
|  [11]   | `parent`      | property | parent group or `None` for root                    |
|  [12]   | `path`        | property | slash-separated path to this group                 |

[PUBLIC_TYPE_SCOPE]: Variable members
- rail: field-dataset

| [INDEX] | [SYMBOL]      | [KIND]   | [ROLE]                                           |
| :-----: | :------------ | :------- | :----------------------------------------------- |
|  [01]   | `dimensions`  | property | tuple of dimension name strings                  |
|  [02]   | `dtype`       | property | NumPy dtype of the variable                      |
|  [03]   | `datatype`    | property | dtype or user-defined type object                |
|  [04]   | `ndim`        | property | number of dimensions                             |
|  [05]   | `shape`       | property | tuple of dimension sizes                         |
|  [06]   | `size`        | property | total number of elements                         |
|  [07]   | `mask`        | property | enable/disable automatic masking                 |
|  [08]   | `scale`       | property | enable/disable `scale_factor`/`add_offset` apply |
|  [09]   | `always_mask` | property | force masked array return even without fill      |
|  [10]   | `name`        | property | variable name string                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Dataset lifecycle
- rail: field-dataset

`Dataset.__init__(filename, mode='r', clobber=True, format='NETCDF4', diskless=False, persist=False, keepweakref=False, memory=None, encoding=None, parallel=False, comm=None, info=None, auto_complex=False)` opens or creates the file handle: `diskless`/`persist`/`memory` drive in-memory datasets, `parallel`/`comm`/`info` open an MPI-collective handle, and `auto_complex` reconstructs complex variables.

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `Dataset.__init__(filename, mode='r', ...)`                                 | open           | open/create file handle      |
|  [02]   | `Dataset.close()`                                                           | lifecycle      | flush and close file         |
|  [03]   | `Dataset.sync()`                                                            | lifecycle      | flush pending writes to disk |
|  [04]   | `Dataset.filepath(encoding=None)`                                           | accessor       | resolve on-disk path         |
|  [05]   | `Dataset.isopen()`                                                          | accessor       | check if file is still open  |
|  [06]   | `Dataset.fromcdl(cdlfilename, ncfilename=None, mode='a', format='NETCDF4')` | factory        | create from CDL text         |
|  [07]   | `Dataset.tocdl(coordvars=False, data=False, outfile=None)`                  | export         | dump CDL text representation |

[ENTRYPOINT_SCOPE]: structure creation
- rail: field-dataset

`Dataset.createVariable(varname, datatype, dimensions=(), compression=None, zlib=False, complevel=4, shuffle=True, szip_coding='nn', szip_pixels_per_block=8, blosc_shuffle=1, fletcher32=False, contiguous=False, chunksizes=None, endian='native', least_significant_digit=None, significant_digits=None, quantize_mode='BitGroom', fill_value=None, chunk_cache=None)` defines and returns a `Variable`; the compression, chunking, and quantization kwargs carry the codec policy.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Dataset.createDimension(dimname, size=None)`                   | create         | define dimension (unlimited if `size=None`) |
|  [02]   | `Dataset.createVariable(varname, datatype, dimensions=(), ...)` | create         | define and return Variable                  |
|  [03]   | `Dataset.createGroup(groupname)`                                | create         | create nested group                         |
|  [04]   | `Dataset.createCompoundType(datatype, datatype_name)`           | create type    | register compound dtype                     |
|  [05]   | `Dataset.createEnumType(datatype, datatype_name, enum_dict)`    | create type    | register enum type                          |
|  [06]   | `Dataset.createVLType(datatype, datatype_name)`                 | create type    | register vlen type                          |

[ENTRYPOINT_SCOPE]: attribute and rename operations
- rail: field-dataset

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `Dataset.ncattrs()`                             | attributes     | list attribute names          |
|  [02]   | `Dataset.getncattr(name)`                       | attributes     | get one attribute             |
|  [03]   | `Dataset.setncattr(name, value)`                | attributes     | set one attribute             |
|  [04]   | `Dataset.setncattr_string(name, value)`         | attributes     | force string attribute type   |
|  [05]   | `Dataset.setncatts(attdict)`                    | attributes     | set multiple attributes       |
|  [06]   | `Dataset.delncattr(name)`                       | attributes     | delete attribute              |
|  [07]   | `Dataset.renameVariable(oldname, newname)`      | rename         | rename variable in file       |
|  [08]   | `Dataset.renameDimension(oldname, newname)`     | rename         | rename dimension              |
|  [09]   | `Dataset.renameGroup(oldname, newname)`         | rename         | rename group                  |
|  [10]   | `Dataset.renameAttribute(oldname, newname)`     | rename         | rename global attribute       |
|  [11]   | `Dataset.get_variables_by_attributes(**kwargs)` | query          | filter variables by attribute |

[ENTRYPOINT_SCOPE]: Variable operations
- rail: field-dataset

Every surface below is a `Variable` method.

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `chunking()`                                                   | accessor       | chunksizes tuple or `'contiguous'`                |
|  [02]   | `filters()`                                                    | accessor       | dict of active compression filters                |
|  [03]   | `get_fill_value()`                                             | accessor       | fill value                                        |
|  [04]   | `get_var_chunk_cache()`                                        | accessor       | chunk cache settings                              |
|  [05]   | `set_var_chunk_cache(size=None, nelems=None, preemption=None)` | mutator        | configure chunk cache                             |
|  [06]   | `set_auto_mask(mask)`                                          | masking        | toggle auto-masking                               |
|  [07]   | `set_auto_scale(scale)`                                        | scale          | toggle `scale_factor`/`add_offset`                |
|  [08]   | `set_auto_maskandscale(maskandscale)`                          | masking+scale  | toggle mask and scale together                    |
|  [09]   | `set_always_mask(always_mask)`                                 | masking        | force masked-array return                         |
|  [10]   | `set_collective(value)`                                        | parallel I/O   | per-variable collective vs independent MPI access |
|  [11]   | `use_nc_get_vars(use_nc_get_vars)`                             | read tuning    | toggle strided `nc_get_vars` reads                |
|  [12]   | `get_dims()`                                                   | accessor       | tuple of `Dimension` objects                      |
|  [13]   | `setncattr_string(name, value)`                                | attributes     | force string attribute type                       |
|  [14]   | `renameAttribute(oldname, newname)`                            | rename         | rename variable attribute                         |
|  [15]   | `endian()`                                                     | accessor       | endianness string                                 |
|  [16]   | `quantization()`                                               | accessor       | quantization settings dict                        |

[ENTRYPOINT_SCOPE]: module-level time and utility functions
- rail: field-dataset

The CF time functions delegate to `cftime`: `date2num(dates, units, calendar=None, has_year_zero=None, longdouble=False)`, `num2date(times, units, calendar='standard', only_use_cftime_datetimes=True, only_use_python_datetimes=False, has_year_zero=None)`, and `date2index(dates, nctime, calendar=None, select='exact', has_year_zero=None)`.

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :--------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `date2num(...)`                                            | time           | datetime objects to numeric values        |
|  [02]   | `num2date(...)`                                            | time           | numeric values to datetime objects        |
|  [03]   | `date2index(...)`                                          | time           | datetime to index in time variable        |
|  [04]   | `stringtoarr(string, NUMCHARS, dtype='S')`                 | string util    | string to fixed-length char array         |
|  [05]   | `chartostring(b, encoding='utf-8')`                        | string util    | char array to string                      |
|  [06]   | `stringtochar(a, encoding='utf-8', n_strlen=None)`         | string util    | string array to char array                |
|  [07]   | `getlibversion()`                                          | info           | netCDF-C library version string           |
|  [08]   | `dtype_is_complex(dtype)`                                  | type query     | complex-dtype test, paired `auto_complex` |
|  [09]   | `get_chunk_cache()`                                        | config         | global HDF5 chunk cache settings          |
|  [10]   | `set_chunk_cache(size=None, nelems=None, preemption=None)` | config         | configure global HDF5 chunk cache         |
|  [11]   | `get_alignment()`                                          | config         | HDF5 alignment parameters                 |
|  [12]   | `set_alignment(threshold, alignment)`                      | config         | set HDF5 alignment                        |
|  [13]   | `rc_get(key)`                                              | config         | get runtime configuration value           |
|  [14]   | `rc_set(key, value)`                                       | config         | set runtime configuration value           |

CF time (`date2num`/`num2date`/`date2index`) is provided by the `cftime` dependency and re-exported here; calendar vocabulary (`standard`, `gregorian`, `noleap`, `360_day`, `julian`, `proleptic_gregorian`) and `has_year_zero` semantics live in `cftime`. The field-dataset rail decodes time through these, never through hand-parsed CF unit strings.

## [04]-[IMPLEMENTATION_LAW]

[NETCDF4_TOPOLOGY]:
- `Dataset` and `Group` share the same method surface; `Group` is a child container obtained via `createGroup` or `groups[name]`
- `MFDataset` aggregates multiple files along an unlimited dimension; read-only; shares Dataset property/method API; `MFTime` decodes a heterogeneous-calendar/units time axis across the aggregated files
- `default_fillvals` is a `dict` mapping dtype strings to fill values (`'f4'`, `'f8'`, `'i4'`, etc.); `is_native_little`/`is_native_big` report host byte order
- `NC_DISKLESS` and `NC_PERSIST` flags available as module-level constants for in-memory datasets; combine with the `diskless`/`persist`/`memory` `Dataset.__init__` kwargs for in-RAM read/write and zero-copy `memoryview` egress on close
- compression: `zlib`, `szip` (`szip_coding`, `szip_pixels_per_block`), `blosc` (`blosc_shuffle`), `bzip2`, `zstd` — and lossy `quantize` (`significant_digits`/`quantize_mode='BitGroom'|'BitRound'|'GranularBitRound'`, default `'BitGroom'`)
- build/capability flags are module-level `BoolInt` booleans, read once at boundary scope to gate codec/parallel selection: `__has_blosc_support__`, `__has_bzip2_support__`, `__has_szip_support__`, `__has_zstandard_support__`, `__has_quantization_support__`, `__has_ncfilter__`, `__has_set_alignment__`, `__has_nc_rc_set__`, `__has_parallel_support__`, `__has_parallel4_support__`, `__has_pnetcdf_support__`, `__has_cdf5_format__`, `__has_nc_open_mem__`, `__has_nc_create_mem__`; `__netcdf4libversion__`/`__hdf5libversion__` report linked native versions
- parallel I/O: `Dataset(..., parallel=True, comm=<mpi4py comm>, info=<mpi4py info>)` opens an MPI-collective handle (requires a parallel-enabled netCDF-C build, gated by `__has_parallel_support__`); per-variable access mode toggles via `Variable.set_collective(True|False)`

[LOCAL_ADMISSION]:
- Open `Dataset` as a context manager (`with netCDF4.Dataset(...) as ds:`) to guarantee `close()` on exit; `close()` returns the in-memory buffer as a `memoryview` when `memory=` was supplied.
- Set `auto_complex=True` on the Dataset to reconstruct complex variables stored as paired real/imag; `dtype_is_complex` classifies a dtype string.
- Time handling routes through `date2num`/`num2date` (owned by `cftime`) with explicit `calendar` and `units`; never hand-decode CF time strings.
- Variable read returns a masked array by default; `set_auto_mask(False)`/`set_auto_scale(False)`/`set_auto_maskandscale(False)` only when downstream handles fill values and `scale_factor`/`add_offset` explicitly.
- Gate codec and parallel choices on the `__has_*` build flags before `createVariable(compression=...)` or `parallel=True`, since the source build links whichever native filters/MPI the Forge toolchain enabled.

[INTEGRATION]:
- xarray seam: `xarray.open_dataset(path, engine='netcdf4')` is the canonical field-dataset entry; this owner is reached directly only for low-level CF metadata, group/dimension/variable authoring, MPI-collective write, or in-memory `memory=`/`diskless=` round-trips that xarray does not expose. The decoded cube flows into the same lazy `xarray.Dataset` rail that `odc-stac` and `zarr`/`tensorstore` feed.
- cftime stack: CF time decode delegates to `cftime`; the resolved numeric axis pairs with `pandas`/`numpy` datetime indices at the xarray boundary, never re-parsed locally.
- in-memory rail: `memory=`/`diskless=` write paired with `close() -> memoryview` lets the field-dataset rail emit a netCDF byte payload straight to the `obstore` object-store rail (`put`) without a temp file, and read a fetched `Bytes` back via `Dataset('inmem', memory=<bytes>)`.

[RAIL_LAW]:
- Package: `netCDF4`
- Owns: netCDF-4/HDF5 file I/O, dimension/variable creation, CF time conversion (via `cftime`), multi-file aggregation, MPI-parallel collective I/O, in-memory datasets, and compression/quantization filters
- Accept: context-manager file open; `date2num`/`num2date` for CF time; `createVariable` with build-flag-gated compression and chunksizes; `memory=`/`diskless=` for object-store round-trips; `parallel=True` only when `__has_parallel_support__`
- Reject: hand-rolling netCDF byte parsing; direct HDF5 manipulation when netCDF4 covers the operation; hand-decoding CF unit strings when `cftime` owns the calendar; a second engine path when xarray's `engine='netcdf4'` already routes here
