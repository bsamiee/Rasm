# [PY_DATA_API_NETCDF4]

`netCDF4` supplies Python bindings to the Unidata netCDF-4 C library — hierarchical group/dimension/variable containers, CF-compliant time conversion, multi-file dataset aggregation, compression filters, and chunking controls. It is the `FieldDataset` CF reader engine — the first consumer of the admitted-but-previously-unconsumed `netcdf4` — bound through `xarray.open_dataset(engine="netcdf4")` and reachable directly for low-level CF metadata, dimension/variable creation, and CF time conversion.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `netCDF4`
- package: `netCDF4`
- import: `netCDF4`
- owner: `data`
- rail: field-dataset
- capability: the `FieldDataset` CF reader engine — netCDF-4/HDF5 file I/O via C extension — hierarchical groups, CF-compliant time conversion, multi-file aggregation, compression filters, and chunking controls

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document and group containers
- rail: field-dataset

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [ROLE]                                                       |
| :-----: | :---------- | :------------ | :----------------------------------------------------------- |
|   [1]   | `Dataset`   | class         | root netCDF file handle; create/read/write                   |
|   [2]   | `Group`     | class         | named sub-group within a Dataset (same API as Dataset)       |
|   [3]   | `MFDataset` | class         | multi-file aggregation view (read-only); same API as Dataset |
|   [4]   | `MFTime`    | class         | time-axis wrapper for multi-file CF time decoding            |

[PUBLIC_TYPE_SCOPE]: dimension and variable
- rail: field-dataset

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [ROLE]                                        |
| :-----: | :---------- | :------------ | :-------------------------------------------- |
|   [1]   | `Dimension` | class         | named dimension with optional unlimited flag  |
|   [2]   | `Variable`  | class         | n-D data array with attributes, masks, scales |

[PUBLIC_TYPE_SCOPE]: compound and custom types
- rail: field-dataset

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [ROLE]                                   |
| :-----: | :------------------------------- | :------------ | :--------------------------------------- |
|   [1]   | `CompoundType`                   | class         | user-defined structured (compound) dtype |
|   [2]   | `VLType`                         | class         | variable-length (vlen) type              |
|   [3]   | `EnumType`                       | class         | enumeration type with dict mapping       |
|   [4]   | `NetCDF4MissingFeatureException` | exception     | raised when HDF5/netCDF-4 feature absent |

[PUBLIC_TYPE_SCOPE]: Dataset members
- rail: field-dataset

| [INDEX] | [SYMBOL]      | [KIND]   | [ROLE]                                             |
| :-----: | :------------ | :------- | :------------------------------------------------- |
|   [1]   | `dimensions`  | property | mapping of dimension names to `Dimension`          |
|   [2]   | `variables`   | property | mapping of variable names to `Variable`            |
|   [3]   | `groups`      | property | mapping of group names to `Group`                  |
|   [4]   | `cmptypes`    | property | mapping of compound type names                     |
|   [5]   | `vltypes`     | property | mapping of vlen type names                         |
|   [6]   | `enumtypes`   | property | mapping of enum type names                         |
|   [7]   | `data_model`  | property | format string (`NETCDF4`, `NETCDF4_CLASSIC`, etc.) |
|   [8]   | `disk_format` | property | on-disk format string                              |
|   [9]   | `file_format` | property | file format string                                 |
|  [10]   | `name`        | property | group name                                         |
|  [11]   | `parent`      | property | parent group or `None` for root                    |
|  [12]   | `path`        | property | slash-separated path to this group                 |

[PUBLIC_TYPE_SCOPE]: Variable members
- rail: field-dataset

| [INDEX] | [SYMBOL]      | [KIND]   | [ROLE]                                           |
| :-----: | :------------ | :------- | :----------------------------------------------- |
|   [1]   | `dimensions`  | property | tuple of dimension name strings                  |
|   [2]   | `dtype`       | property | NumPy dtype of the variable                      |
|   [3]   | `datatype`    | property | dtype or user-defined type object                |
|   [4]   | `ndim`        | property | number of dimensions                             |
|   [5]   | `shape`       | property | tuple of dimension sizes                         |
|   [6]   | `size`        | property | total number of elements                         |
|   [7]   | `mask`        | property | enable/disable automatic masking                 |
|   [8]   | `scale`       | property | enable/disable `scale_factor`/`add_offset` apply |
|   [9]   | `always_mask` | property | force masked array return even without fill      |
|  [10]   | `name`        | property | variable name string                             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Dataset lifecycle
- rail: field-dataset

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :--------------------------- |
|   [1]   | `Dataset.__init__(filename, mode='r', clobber=True, ...)`                   | open           | open/create file handle      |
|   [2]   | `Dataset.close()`                                                           | lifecycle      | flush and close file         |
|   [3]   | `Dataset.sync()`                                                            | lifecycle      | flush pending writes to disk |
|   [4]   | `Dataset.filepath(encoding=None)`                                           | accessor       | resolve on-disk path         |
|   [5]   | `Dataset.isopen()`                                                          | accessor       | check if file is still open  |
|   [6]   | `Dataset.fromcdl(cdlfilename, ncfilename=None, mode='a', format='NETCDF4')` | factory        | create from CDL text         |
|   [7]   | `Dataset.tocdl(coordvars=False, data=False, outfile=None)`                  | export         | dump CDL text representation |

[ENTRYPOINT_SCOPE]: structure creation
- rail: field-dataset

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                                                                                                                                                 | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------ |
|   [1]   | `Dataset.createDimension(dimname, size=None)`                                                                                                                                                                                                                                                                                                                             | create         | define dimension (unlimited if `size=None`) |
|   [2]   | `Dataset.createVariable(varname, datatype, dimensions=(), compression=None, zlib=False, complevel=4, shuffle=True, szip_coding='nn', szip_pixels_per_block=8, blosc_shuffle=1, fletcher32=False, contiguous=False, chunksizes=None, endian='native', least_significant_digit=None, significant_digits=None, quantize_mode='BitGroom', fill_value=None, chunk_cache=None)` | create         | define and return Variable                  |
|   [3]   | `Dataset.createGroup(groupname)`                                                                                                                                                                                                                                                                                                                                          | create         | create nested group                         |
|   [4]   | `Dataset.createCompoundType(datatype, datatype_name)`                                                                                                                                                                                                                                                                                                                     | create type    | register compound dtype                     |
|   [5]   | `Dataset.createEnumType(datatype, datatype_name, enum_dict)`                                                                                                                                                                                                                                                                                                              | create type    | register enum type                          |
|   [6]   | `Dataset.createVLType(datatype, datatype_name)`                                                                                                                                                                                                                                                                                                                           | create type    | register vlen type                          |

[ENTRYPOINT_SCOPE]: attribute and rename operations
- rail: field-dataset

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `Dataset.ncattrs()`                             | attributes     | list attribute names          |
|   [2]   | `Dataset.getncattr(name)`                       | attributes     | get one attribute             |
|   [3]   | `Dataset.setncattr(name, value)`                | attributes     | set one attribute             |
|   [4]   | `Dataset.setncattr_string(name, value)`         | attributes     | force string attribute type   |
|   [5]   | `Dataset.setncatts(attdict)`                    | attributes     | set multiple attributes       |
|   [6]   | `Dataset.delncattr(name)`                       | attributes     | delete attribute              |
|   [7]   | `Dataset.renameVariable(oldname, newname)`      | rename         | rename variable in file       |
|   [8]   | `Dataset.renameDimension(oldname, newname)`     | rename         | rename dimension              |
|   [9]   | `Dataset.renameGroup(oldname, newname)`         | rename         | rename group                  |
|  [10]   | `Dataset.renameAttribute(oldname, newname)`     | rename         | rename global attribute       |
|  [11]   | `Dataset.get_variables_by_attributes(**kwargs)` | query          | filter variables by attribute |

[ENTRYPOINT_SCOPE]: Variable operations
- rail: field-dataset

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :---------------------------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `Variable.chunking()`                                                   | accessor       | chunksizes tuple or `'contiguous'` |
|   [2]   | `Variable.filters()`                                                    | accessor       | dict of active compression filters |
|   [3]   | `Variable.get_fill_value()`                                             | accessor       | fill value                         |
|   [4]   | `Variable.get_var_chunk_cache()`                                        | accessor       | chunk cache settings               |
|   [5]   | `Variable.set_var_chunk_cache(size=None, nelems=None, preemption=None)` | mutator        | configure chunk cache              |
|   [6]   | `Variable.set_auto_mask(mask)`                                          | masking        | toggle auto-masking                |
|   [7]   | `Variable.set_auto_maskandscale(maskandscale)`                          | masking+scale  | toggle mask and scale together     |
|   [8]   | `Variable.endian()`                                                     | accessor       | endianness string                  |
|   [9]   | `Variable.quantization()`                                               | accessor       | quantization settings dict         |

[ENTRYPOINT_SCOPE]: module-level time and utility functions
- rail: field-dataset

| [INDEX] | [SURFACE]                                                                                                                          | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `date2num(dates, units, calendar=None, has_year_zero=None, longdouble=False)`                                                      | time           | datetime objects to numeric values |
|   [2]   | `num2date(times, units, calendar='standard', only_use_cftime_datetimes=True, only_use_python_datetimes=False, has_year_zero=None)` | time           | numeric values to datetime objects |
|   [3]   | `date2index(dates, nctime, calendar=None, select='exact', has_year_zero=None)`                                                     | time           | datetime to index in time variable |
|   [4]   | `stringtoarr(string, NUMCHARS, dtype='S')`                                                                                         | string util    | string to fixed-length char array  |
|   [5]   | `chartostring(b, encoding='utf-8')`                                                                                                | string util    | char array to string               |
|   [6]   | `stringtochar(a, encoding='utf-8', n_strlen=None)`                                                                                 | string util    | string array to char array         |
|   [7]   | `getlibversion()`                                                                                                                  | info           | netCDF-C library version string    |
|   [8]   | `get_chunk_cache()`                                                                                                                | config         | global HDF5 chunk cache settings   |
|   [9]   | `set_chunk_cache(size=None, nelems=None, preemption=None)`                                                                         | config         | configure global HDF5 chunk cache  |
|  [10]   | `get_alignment()`                                                                                                                  | config         | HDF5 alignment parameters          |
|  [11]   | `set_alignment(threshold, alignment)`                                                                                              | config         | set HDF5 alignment                 |
|  [12]   | `rc_get(key)`                                                                                                                      | config         | get runtime configuration value    |
|  [13]   | `rc_set(key, value)`                                                                                                               | config         | set runtime configuration value    |

## [4]-[IMPLEMENTATION_LAW]

[NETCDF4_TOPOLOGY]:
- `Dataset` and `Group` share the same method surface; `Group` is a child container obtained via `createGroup` or `groups[name]`
- `MFDataset` aggregates multiple files along an unlimited dimension; read-only; shares Dataset property/method API
- `default_fillvals` is a `dict` mapping dtype strings to fill values (`'f4'`, `'f8'`, `'i4'`, etc.)
- `NC_DISKLESS` and `NC_PERSIST` flags available as module-level constants for in-memory datasets
- compression: `zlib`, `szip` (`szip_coding`, `szip_pixels_per_block`), `blosc` (`blosc_shuffle`), `bzip2`, `zstd` — build support is read from module-level boolean flags `netCDF4.__has_blosc_support__`, `__has_bzip2_support__`, `__has_szip_support__`, `__has_zstandard_support__`, and `__has_quantization_support__`

[LOCAL_ADMISSION]:
- Open `Dataset` as a context manager (`with netCDF4.Dataset(...) as ds:`) to guarantee `close()` on exit.
- Set `auto_complex` on the Dataset to enable automatic complex variable reconstruction when stored as paired real/imag.
- Time handling routes through `date2num`/`num2date` with explicit `calendar` and `units`; never hand-decode CF time strings.
- Variable read returns a masked array by default; set `set_auto_mask(False)` only when downstream code handles fill values explicitly.

[RAIL_LAW]:
- Package: `netCDF4`
- Owns: netCDF-4/HDF5 file I/O, dimension/variable creation, CF time conversion, multi-file aggregation
- Accept: context-manager file open; `date2num`/`num2date` for CF time; `createVariable` with explicit compression and chunksizes
- Reject: hand-rolling netCDF byte parsing; direct HDF5 manipulation when netCDF4 covers the operation
