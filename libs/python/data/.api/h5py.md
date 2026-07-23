# [PY_DATA_API_H5PY]

`h5py` maps the HDF5 container to Python: `File` owns the on-disk file, `Group` the hierarchical namespace, `Dataset` typed n-dimensional chunked storage, and `AttributeManager` per-object metadata, every dataset crossing the boundary as a `numpy` array. It is the single-file HDF5 store beneath the gridded field rail — a read sources the compute-rail array and a write is its durable sink; the low-level `h5*` C-API modules reach every property-list knob the wrappers do not, and the HDF5 codec and filter pipeline stays C-owned.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h5py`
- package: `h5py` (`BSD-3-Clause`)
- module: `import h5py`
- namespaces: `h5py`, the low-level `h5py.h5*` C-API modules
- owner: `data`
- rail: array — the single-file HDF5 store beneath the gridded field rail
- capability: HDF5 file IO, hierarchical group namespace, typed chunked and compressed dataset storage, NumPy-style slicing, attribute metadata, special dtypes (vlen, enum, string, complex, opaque, reference), virtual datasets, dimension scales, the `h5py.filters` pipeline, SWMR access, and the full low-level C API

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: HDF5 object tree and value types

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------- | :------------ | :------------------------------------ |
|  [01]   | `File`             | class         | on-disk container and root group      |
|  [02]   | `Group`            | class         | hierarchical name-to-object namespace |
|  [03]   | `Dataset`          | class         | typed n-dimensional array storage     |
|  [04]   | `AttributeManager` | class         | per-object `attrs` metadata map       |
|  [05]   | `Datatype`         | class         | committed named datatype              |
|  [06]   | `HLObject`         | class         | shared base for tree objects          |
|  [07]   | `VirtualLayout`    | class         | virtual-dataset layout target         |
|  [08]   | `VirtualSource`    | class         | virtual-dataset source mapping        |
|  [09]   | `HardLink`         | value-object  | hard-link assignment value            |
|  [10]   | `SoftLink`         | value-object  | soft-link assignment value            |
|  [11]   | `ExternalLink`     | value-object  | cross-file link value                 |
|  [12]   | `Reference`        | value-object  | object reference value                |
|  [13]   | `RegionReference`  | value-object  | dataset-region reference value        |
|  [14]   | `Empty`            | value-object  | null-dataspace scalar value           |
|  [15]   | `MultiBlockSlice`  | value-object  | strided block selection               |
|  [16]   | `UNLIMITED`        | sentinel      | unlimited `maxshape` axis marker      |

[OBJECT_PROPERTIES]: instance attributes threaded onto the tree objects
- `File`: `filename` `mode` `driver` `libver` `swmr_mode` `userblock_size` `meta_block_size`
- `Dataset`: `shape` `dtype` `size` `nbytes` `ndim` `chunks` `maxshape` `compression` `compression_opts` `shuffle` `fletcher32` `scaleoffset` `fillvalue` `is_virtual` `dims` `external`
- `HLObject`: `name` `parent` `file` `id` `ref` `regionref` `attrs`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: group creation and structure
- creation carry: `chunks` `maxshape` `compression` `compression_opts` `shuffle` `fletcher32` `scaleoffset` `fillvalue` `track_order` `external`

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :----------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Group.create_dataset(name, shape, dtype, data, **) -> Dataset`    | instance | create dataset with fixed codec    |
|  [02]   | `Group.require_dataset(name, shape, dtype, exact, **) -> Dataset`  | instance | idempotent get-or-create dataset   |
|  [03]   | `Group.create_dataset_like(name, other, **) -> Dataset`            | instance | clone another dataset spec         |
|  [04]   | `Group.create_group(name, track_order) -> Group`                   | instance | create subgroup                    |
|  [05]   | `Group.require_group(name) -> Group`                               | instance | idempotent get-or-create group     |
|  [06]   | `Group.create_virtual_dataset(name, layout, fillvalue) -> Dataset` | instance | materialize a VDS from a layout    |
|  [07]   | `Group.build_virtual_dataset()`                                    | instance | assemble a VDS in a with-block     |
|  [08]   | `Group[name] = link_or_object`                                     | operator | bind link or object into namespace |
|  [09]   | `Group.get(name, default, getclass, getlink)`                      | instance | fetch member class or link         |
|  [10]   | `Group.move(source, dest)`                                         | instance | rename within the file             |
|  [11]   | `Group.copy(source, dest)`                                         | instance | deep-copy a subtree                |

[ENTRYPOINT_SCOPE]: dataset IO

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :--------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `Dataset[selection]`                                 | operator | read selection as `ndarray`    |
|  [02]   | `Dataset[selection] = value`                         | operator | write a selection              |
|  [03]   | `Dataset.read_direct(dest, source_sel, dest_sel)`    | instance | read into preallocated buffer  |
|  [04]   | `Dataset.write_direct(source, source_sel, dest_sel)` | instance | write from preallocated buffer |
|  [05]   | `Dataset.astype(dtype)`                              | instance | read-time dtype view           |
|  [06]   | `Dataset.asstr(encoding, errors)`                    | instance | decode string dataset to `str` |
|  [07]   | `Dataset.fields(names)`                              | instance | select compound-dtype fields   |
|  [08]   | `Dataset.iter_chunks(sel)`                           | instance | iterate chunk selections       |
|  [09]   | `Dataset.resize(size, axis)`                         | instance | grow a chunked dataset         |
|  [10]   | `Dataset.flush()`                                    | instance | flush writer view              |
|  [11]   | `Dataset.refresh()`                                  | instance | refresh reader view            |
|  [12]   | `Dataset.make_scale(name)`                           | instance | mark as dimension scale        |
|  [13]   | `Dataset.virtual_sources()`                          | instance | enumerate VDS source mappings  |

[ENTRYPOINT_SCOPE]: attributes and traversal

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]              |
| :-----: | :-------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `AttributeManager.create(name, data, shape, dtype)` | instance | create typed attribute    |
|  [02]   | `AttributeManager.modify(name, value)`              | instance | overwrite attribute value |
|  [03]   | `AttributeManager.get(name, default)`               | instance | read attribute or default |

- traversal: `Group.visit` `Group.visititems` `Group.visit_links` `Group.visititems_links` — recursive name and item callbacks, the `_links` forms yielding link objects

[ENTRYPOINT_SCOPE]: special dtypes

| [INDEX] | [SURFACE]                               | [SHAPE] | [CAPABILITY]                  |
| :-----: | :-------------------------------------- | :------ | :---------------------------- |
|  [01]   | `string_dtype(encoding, length)`        | factory | vlen or fixed string dtype    |
|  [02]   | `vlen_dtype(basetype)`                  | factory | variable-length element dtype |
|  [03]   | `enum_dtype(values, basetype)`          | factory | enumerated integer dtype      |
|  [04]   | `opaque_dtype(np_dtype)`                | factory | opaque fixed-width byte dtype |
|  [05]   | `complex_compat_dtype(real, imag_name)` | factory | split-field complex dtype     |

- reference dtypes: `ref_dtype` `regionref_dtype` — object and region reference dtypes
- predicates: `check_string_dtype` `check_vlen_dtype` `check_enum_dtype` `check_opaque_dtype` `check_complex_dtype` `check_ref_dtype` — each returns the special-type metadata or `None`

[ENTRYPOINT_SCOPE]: filter pipeline, file utilities, and low-level C API
- `compression` accepts `'gzip'` `'lzf'` `'szip'` or a registered filter id, and third-party plugins resolve through `h5pl`
- filters (`h5py.filters`): `fill_dcpl` `guess_chunk` `get_filters` `encode` `decode` `Gzip` — build the creation property list and drive the filter pipeline at `create_dataset`
- utilities: `is_hdf5` `get_config` `register_driver` `unregister_driver` `registered_drivers` `enable_ipython_completer` `run_tests`
- `registered_drivers()` returns `{'sec2', 'stdio', 'core', 'split', 'family', 'fileobj', 'mpio'}`
- low-level C API: `h5f` `h5d` `h5g` `h5p` `h5s` `h5t` `h5a` `h5o` `h5l` `h5r` `h5z` `h5fd` `h5ds` `h5i` `h5pl` — the property-list and selection escape hatch

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `File` is a context manager; a `with` block flushes and closes the handle, and a leaked handle locks or corrupts the file.
- `chunks` `compression` `shuffle` `fletcher32` `scaleoffset` `fillvalue` bind at `create_dataset` and never change, and chunked storage is the precondition for compression, resize, and partial IO.
- A special dtype carries HDF5 type metadata a raw NumPy dtype drops; the `*_dtype` factory mints it and the matching `check_*` predicate reads it back.
- `require_*` folds the existence check into one idempotent entry, and the `create_*` form raises on a live name.
- SWMR admits one writer and many readers, and `flush` and `refresh` synchronise the two views.
- h5py raises `OSError` for a library or driver fault, `KeyError` for an absent name, and `TypeError` or `ValueError` for a dtype or selection mismatch.

[STACKING]:
- `numpy`(`.api/../numpy.md`): `Dataset[sel]` returns an `ndarray` and `Dataset[sel] = arr` writes one, and `read_direct`/`write_direct` move a block into a preallocated `ndarray` with no intermediate copy — read once into the array kernel, never scalar-iterate.
- `h5netcdf`(`.api/h5netcdf.md`): this surface is the HDF5 store beneath the pure-h5py netCDF-4 engine, which reads and writes every group, dimension scale, and filter through it.
- `icechunk`(`.api/icechunk.md`): a boundary, not a composition — h5py owns the single-file HDF5 container, icechunk and zarr own transactional object-store chunked arrays, and neither re-implements the other chunk index.
- gridded field rail: `VirtualLayout`/`VirtualSource` with `create_virtual_dataset` assemble many per-file datasets into one lazily-read logical array, pairing `shuffle=True` with `compression='gzip'` and matching the chunk shape to the dominant read selection.

[LOCAL_ADMISSION]:
- `File` opens in a `with` block, `create_dataset` fixes chunk and compression policy at creation, IO crosses through NumPy-style slicing or `read_direct`/`write_direct`, special types come from the `*_dtype` factories, `VirtualLayout`/`VirtualSource` assemble logical arrays, `require_*` owns idempotent structure, and the `h5*` modules are the property-list escape hatch.

[RAIL_LAW]:
- Package: `h5py`
- Owns: HDF5 single-file IO, the group namespace, chunked and compressed typed dataset storage, NumPy-style slicing, attribute metadata, special dtypes, the `h5py.filters` pipeline, virtual datasets, dimension scales, SWMR, and the low-level C API
- Accept: a context-managed `File`, creation-time codec policy, slice and direct-buffer IO, `*_dtype` special types, `VirtualLayout`/`VirtualSource` assembly, and `require_*` idempotent structure
- Reject: a file handle leaked outside `with`, post-creation codec mutation, a raw NumPy dtype where HDF5 special metadata is required, per-scalar IO loops where a slice reads the block, Python-side re-encoding of compressed bytes, and existence-check branching the `require_*` form owns
