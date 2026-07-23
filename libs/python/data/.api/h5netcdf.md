# [PY_DATA_API_H5NETCDF]

`h5netcdf` implements the netCDF-4 data model over `h5py` alone — HDF5 under the netCDF dimension-scale convention, no netCDF-C linkage. One core carries two surfaces: the native tree rooted at `h5netcdf.File` and `h5netcdf.legacyapi`, a `netCDF4`-python-shaped shim an `xarray` backend binds as a drop-in. It is the h5py-native `FieldEngine.H5NETCDF` backend beneath `xarray`, which owns CF metadata, coordinate addressing, and reductions above it; the netCDF-C quantization surface has no h5py backing and routes to `netcdf4` alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h5netcdf`
- package: `h5netcdf` (BSD-3-Clause)
- import: `import h5netcdf` (native) / `import h5netcdf.legacyapi as netCDF4` (shim)
- owner: `data`
- rail: field-dataset — the h5py-native `FieldEngine.H5NETCDF` backend
- asset: pure Python over `h5py`; the HDF5 native core arrives transitively through `h5py`, no netCDF-C linkage
- depends: `h5py`, `numpy`, `packaging`; `xarray` binds it through the `h5netcdf` entry in the `xarray.backends` group, never a hard dependency here
- capability: read/write the netCDF-4 data model over HDF5 through `h5py` — groups, unlimited dimensions, HDF5 chunking and gzip/shuffle/fletcher32/szip filters, compound/enum/vlen user types, phony-dimension synthesis for scale-free HDF5, and the `invalid_netcdf` escape hatch for h5py features outside the netCDF model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: native surface `h5netcdf.*`

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :----------------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `File`                               | class         | root container and store owner; group tree + file lifecycle         |
|  [02]   | `Group`                              | class         | nested namespace; `File`'s surface minus the file-lifecycle handles |
|  [03]   | `Variable`                           | class         | array node; slices with numpy fancy-indexing semantics              |
|  [04]   | `Dimension`                          | class         | named axis with `size` and the `isunlimited()` flag                 |
|  [05]   | `EnumType`, `CompoundType`, `VLType` | class         | holders `create_enumtype`/`create_cmptype`/`create_vltype` return   |
|  [06]   | `CompatibilityError`                 | exception     | raised on an h5py construct outside the strict netCDF-4 model       |

- `EnumType`/`CompoundType`/`VLType`/`UserType` live in `h5netcdf.core`, re-exported by `legacyapi`.

[PUBLIC_TYPE_SCOPE]: compatibility shim `h5netcdf.legacyapi.*` — the native surface renamed and defaulted to `netCDF4`-python: `Dataset` subclasses `File`, `createVariable`/`createDimension`/`createGroup` mirror the C-library method names, and `xarray` drives this surface.

[legacyapi]: `Dataset` `Group` `Variable` `Dimension` `CompoundType` `EnumType` `VLType` `UserType` `HasAttributesMixin` `default_fillvals`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: store lifecycle and structure creation — native `File`/`Group` methods

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------ | :------- | :--------------------------- |
|  [01]   | `File(path, mode, format, invalid_netcdf, phony_dims, backend, **kwargs)` | ctor     | open/create the store        |
|  [02]   | `create_group(name)`                                                      | instance | nest a group                 |
|  [03]   | `create_variable(name, dimensions, dtype, **kwargs)`                      | instance | define + return a `Variable` |
|  [04]   | `create_enumtype(datatype, datatype_name, enum_dict)`                     | instance | mint an enum user type       |
|  [05]   | `resize_dimension(dim, size)`                                             | instance | grow an unlimited dimension  |
|  [06]   | `flush()`, `sync()`                                                       | instance | force durability             |
|  [07]   | `close()`                                                                 | instance | flush and close the store    |

- `File(..., backend=)` selects `h5py` (default), `h5pyd` (remote HSDS), or `pyfive` (pure-Python read).
- `create_variable` also takes `data`, `fillvalue`, `chunks`, `chunking_heuristic`; `**kwargs` carries the HDF5 filter band (`compression`/`compression_opts`/`shuffle`/`fletcher32`), and `create_cmptype`/`create_vltype` peer `create_enumtype`.

[File accessors]: `dimensions`/`dims` `variables` `groups` `attrs` `cmptypes`/`enumtypes`/`vltypes` `filename` `mode` `data_model` `get`/`keys`/`items`/`values`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `File` subclasses `Group`, adding file lifecycle (`close`/`flush`/`sync`/`filename`/`mode`) to the shared group surface; `Group` nests arbitrarily.
- reads default to strict netCDF-4 validation: an HDF5 dataset lacking dimension scales requires `phony_dims=` (`sort`/`access`) to open, and an h5py-only construct requires `invalid_netcdf=True` — both opt-in escape hatches.
- Unlimited dimensions, chunking, and the gzip/shuffle/fletcher32 filter stack are HDF5 features `h5netcdf` exposes verbatim from the `h5py` store.

[STACKING]:
- `h5py`(`.api/h5py.md`): the store is an `h5py.File` and every variable an `h5py.Dataset`, so the filter band, chunking, and unlimited `maxshape` are h5py dataset knobs exposed through the selected backend.
- `netcdf4`(`.api/netcdf4.md`): the peer CF engine; both bind `xarray.open_dataset(engine=...)`, and the netCDF-C quantization keys route to `netcdf4` alone.
- `xarray`(`libs/python/.api/xarray.md`): `xarray.open_dataset(path, engine="h5netcdf", chunks="auto", decode_cf=True)` and `Dataset.to_netcdf(path, engine="h5netcdf", encoding=...)` are the `FieldEngine.H5NETCDF` open/write delegates, driving the `legacyapi` `Dataset` surface.
- field-dataset owner: the `FieldEngine.H5NETCDF` row threads only the HDF5-shared compression band (`compression`/`compression_opts`/`shuffle`/`fletcher32`) to this engine via `FieldEncoding.for_vars(names, quantize=False)`, stripping the quantization keys by construction.

[LOCAL_ADMISSION]:
- Open `File`/`legacyapi.Dataset` as a context manager (`with h5netcdf.File(...) as ds:`) so `close()` flushes on exit.
- Leave `invalid_netcdf`/`phony_dims` at their defaults for CF field IO; a CF cube is netCDF-4-valid and dimension-scaled by contract.

[RAIL_LAW]:
- Package: `h5netcdf`
- Owns: the pure-`h5py` netCDF-4 read/write path exposed to `xarray` through the `h5netcdf` backend entry point, with the `legacyapi` `netCDF4`-compatible shim
- Accept: `xarray` `engine="h5netcdf"` open/write as the `FieldEngine.H5NETCDF` delegates; the HDF5-shared compression band on written variables; the engine as the h5py-native alternative to `netcdf4` when netCDF-C linkage is undesirable
- Reject: the netCDF-C quantization keys (`least_significant_digit`/`significant_digits`/`quantize_mode`), which have no h5py backing and route to `FieldEngine.NETCDF4`; authoring `File`/`legacyapi.Dataset` directly for CF cubes, where `xarray` owns CF metadata, coordinate addressing, and reductions; `invalid_netcdf=True`/`phony_dims=` on the CF field path
