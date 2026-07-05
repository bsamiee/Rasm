# [PY_DATA_API_H5NETCDF]

`h5netcdf` is the pure-`h5py` implementation of the netCDF-4 data model: it reads and writes NETCDF4-format files (HDF5 with the netCDF dimension-scale convention) entirely through `h5py`, with no dependency on the Unidata netCDF-C library. Two surfaces sit on one core — a native API rooted at `h5netcdf.File` (group/variable/dimension/attribute tree, unlimited dimensions, compound/enum/vlen user types) and a `netCDF4`-python-compatible shim (`h5netcdf.legacyapi`) that mirrors the `netCDF4.Dataset` surface so an `xarray` backend or foreign reader binds it as a drop-in. It is the h5py-native leg of the data field engine axis: `gridded/field.md`'s `FieldEngine.H5NETCDF` row dispatches `xarray.open_dataset(engine="h5netcdf")` / `Dataset.to_netcdf(engine="h5netcdf")` through it. It owns the HDF5-native CF-4 path and nothing above it — the CF metadata layer, coordinate addressing, and reductions are `xarray`'s; h5netcdf never re-implements the netCDF-C quantization surface (it rejects it) and never becomes a second CF owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h5netcdf`
- package: `h5netcdf`
- import: `import h5netcdf` (native) / `import h5netcdf.legacyapi as netCDF4` (`netCDF4`-compatible shim)
- rail: field-dataset (the h5py-native `FieldEngine.H5NETCDF` backend)
- version: `1.8.1`
- license: `BSD-3-Clause`
- asset: pure Python over `h5py` (the HDF5 native core arrives transitively through `h5py`); no netCDF-C linkage
- depends-on: `h5py` (the HDF5 store), `numpy`, `packaging`; `xarray` consumes it through the `xarray.backends` `h5netcdf` entry point, never a hard dependency here
- entry points: registers the `h5netcdf` backend in the `xarray.backends` group; library-only, no console script
- capability: read/write the netCDF-4 data model over HDF5 through `h5py` alone — groups, unlimited dimensions, HDF5 chunking and filters (gzip/shuffle/fletcher32/szip), compound/enum/vlen user types, phony-dimension synthesis for scale-free HDF5, and an `invalid_netcdf` escape hatch for h5py features outside the netCDF data model

## [02]-[CAPTURE]

[PUBLIC_TYPES]: the native surface (`h5netcdf.*`)
- `File(path, mode="r", format="NETCDF4", invalid_netcdf=False, phony_dims=None, backend=None, **kwargs)` — the root container and store owner: `mode` ("r"/"r+"/"a"/"w"/"x"), `format` fixed to `"NETCDF4"` (HDF5), `invalid_netcdf=True` admits h5py-only dtypes/features the strict netCDF-4 model forbids, `phony_dims` ("sort"/"access") synthesizes dimensions for HDF5 datasets carrying no dimension scales, `backend` selects the HDF5 engine ("h5py"/"h5pyd" for remote HSDS). Members: `dimensions`/`dims`, `variables`, `groups`, `attrs`, `create_group`, `create_variable`, `resize_dimension`, `create_cmptype`/`create_enumtype`/`create_vltype` (+ `cmptypes`/`enumtypes`/`vltypes`), `flush`/`sync`, `close`, `filename`, `mode`, `data_model`, and the mapping protocol (`get`/`keys`/`items`/`values`).
- `Group` — the same member surface as `File` minus the file-lifecycle handles; nests arbitrarily and carries `name`/`parent`.
- `Variable` — the array node: `dimensions`, `shape`/`ndim`, `dtype`/`datatype`, `attrs`, `chunks`, `compression`/`compression_opts`, `shuffle`, `fletcher32`, `filters`, and `name`; slices with numpy fancy-indexing semantics.
- `Dimension` — `name`, `size`, `isunlimited()`, `group`.
- `EnumType`/`CompoundType`/`VLType` — the user-defined type holders `create_enumtype`/`create_cmptype`/`create_vltype` mint.

[PUBLIC_TYPES]: the compatibility shim (`h5netcdf.legacyapi`)
- `Dataset`/`Group`/`Variable`/`Dimension` plus `CompoundType`/`EnumType`/`VLType`/`UserType`/`HasAttributesMixin` and `default_fillvals` — the surface renamed and defaulted to match `netCDF4`-python (`Dataset` is `File`, `createVariable`/`createDimension`/`createGroup` mirror the C-library method names) so an `xarray` `h5netcdf` backend or a `netCDF4`-shaped reader binds without a branch. This is the surface `xarray` drives.

[IMPLEMENTATION_LAW]:
- `create_variable(name, dimensions=(), dtype=None, data=None, fillvalue=None, chunks=None, chunking_heuristic=None, **kwargs)` takes the HDF5-shared filter kwargs — `compression="gzip"`, `compression_opts=<level>`, `shuffle=True`, `fletcher32=True` — and rejects the netCDF-C lossy-quantization keys entirely: `least_significant_digit`, `significant_digits`, and `quantize_mode` have no h5py backing, so a write carrying them fails. The `field.md` write arm strips them by construction (`FieldEncoding.for_vars(names, quantize=False)`), threading only the shared compression band to this engine — the quantization band is `netcdf4`'s alone.
- reads default to strict netCDF-4 validation; HDF5 files with datasets lacking dimension scales require `phony_dims=` to open, and h5py-only constructs require `invalid_netcdf=True` — both are opt-in escape hatches the CF field owner does not enable (a CF cube is dimension-scaled and netCDF-4-valid by contract).
- the store is `h5py`-native: unlimited dimensions, chunking, and the gzip/shuffle/fletcher32 filter stack are HDF5 features h5netcdf exposes verbatim, never a re-implementation; `File.flush`/`close` own durability.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `h5netcdf`
- Owns: the pure-`h5py` netCDF-4 read/write path (no netCDF-C), exposed to `xarray` through the `h5netcdf` backend entry point, plus the `legacyapi` `netCDF4`-compatible shim
- Accept: `xarray.open_dataset(path, engine="h5netcdf", chunks="auto", decode_cf=True)` and `Dataset.to_netcdf(path, engine="h5netcdf", encoding=...)` as the `FieldEngine.H5NETCDF` open/write delegates; the HDF5-shared compression band (`compression`/`compression_opts`/`shuffle`/`fletcher32`) on written variables; the engine as the h5py-native alternative to `netcdf4` when the netCDF-C linkage is undesirable
- Reject: passing the netCDF-C quantization keys (`least_significant_digit`/`significant_digits`/`quantize_mode`) — this backend has no lossy-quantization surface, so those encodings route to `FieldEngine.NETCDF4` only; authoring `h5netcdf.File`/`legacyapi.Dataset` directly for CF cubes (`xarray` owns CF metadata, coordinate addressing, and reductions — h5netcdf is the byte-level engine beneath it); `invalid_netcdf=True` and `phony_dims=` for CF field IO (a CF cube is netCDF-4-valid and dimension-scaled by contract); the low-level CF time/group access that reaches `netCDF4` direct
