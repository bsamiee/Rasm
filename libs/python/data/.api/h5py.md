# [PY_DATA_API_H5PY]

`h5py` supplies a Pythonic interface to HDF5 files exposing groups, datasets, attributes, and virtual datasets with NumPy-style slicing and dtype semantics. `File` owns the on-disk container, `Group` owns the hierarchical namespace, `Dataset` owns typed n-dimensional array storage, and `AttributeManager` owns per-object metadata.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h5py`
- package: `h5py`
- import: `import h5py`
- owner: `data`
- rail: array
- capability: HDF5 file IO, hierarchical group namespace, typed chunked/compressed dataset storage, NumPy-style slicing, attribute metadata, special dtypes (vlen, enum, string, reference), virtual datasets, dimension scales, and SWMR access

## [2]-[CAPTURE]

[PUBLIC_TYPES]:
- `h5py.File` — top-level HDF5 container and root group; `File(name, mode='r', driver=None, libver=None, userblock_size=None, swmr=False, rdcc_nslots=None, rdcc_nbytes=None, rdcc_w0=None, track_order=None, ...)`; properties `filename`, `mode`, `driver`, `libver`, `swmr_mode`, `userblock_size`, `meta_block_size`.
- `h5py.Group` — hierarchical namespace mapping names to groups and datasets; dict-like `keys`/`values`/`items`/`get`/`__getitem__`/`__setitem__`; owns creation, linking, traversal, and copy/move.
- `h5py.Dataset` — typed n-dimensional array; properties `shape`, `dtype`, `size`, `nbytes`, `ndim`, `chunks`, `maxshape`, `compression`, `compression_opts`, `shuffle`, `fletcher32`, `scaleoffset`, `fillvalue`, `is_virtual`, `dims`, `external`; NumPy-style `__getitem__`/`__setitem__`.
- `h5py.AttributeManager` — per-object attribute mapping accessed via `obj.attrs`; `create`, `modify`, `get`, `keys`, `values`, `items`, dict-like access.
- `h5py.Datatype` — committed named datatype object stored in the file.
- `h5py.VirtualLayout` — virtual dataset layout target; `VirtualLayout(shape, dtype, maxshape=None, filename=None)`; mapped regions assigned by slice.
- `h5py.VirtualSource` — source mapping for a virtual dataset; `VirtualSource(path_or_dataset, name=None, shape=None, dtype=None, maxshape=None)`.
- `h5py.HLObject` — common base for `File`, `Group`, `Dataset`, `Datatype`; carries `name`, `parent`, `file`, `id`, `ref`, `regionref`, `attrs`.
- link types: `h5py.HardLink`, `h5py.SoftLink(path)`, `h5py.ExternalLink(filename, path)` for `Group.__setitem__` link creation.
- reference types: `h5py.Reference`, `h5py.RegionReference` object/region reference values.
- `h5py.Empty(dtype)` — null-dataspace value for scalar empty datasets and attributes.
- `h5py.MultiBlockSlice(start, count, stride, block)` — strided block selection for advanced dataset indexing.

[ENTRYPOINTS]:
- group creation: `Group.create_dataset(name, shape=None, dtype=None, data=None, *, chunks=None, maxshape=None, compression=None, compression_opts=None, shuffle=None, fletcher32=None, scaleoffset=None, fillvalue=None, track_order=None, external=None) -> Dataset`, `Group.require_dataset(name, shape, dtype, exact=False, **kwds) -> Dataset`, `Group.create_dataset_like(name, other, **kwds)`, `Group.create_group(name, track_order=None) -> Group`, `Group.require_group(name) -> Group`.
- virtual datasets: `Group.create_virtual_dataset(name, layout, fillvalue=None) -> Dataset`, `Group.build_virtual_dataset()` (context manager), `Dataset.virtual_sources()`.
- linking and structure: `Group.__setitem__(name, SoftLink|ExternalLink|HardLink|object)`, `Group.get(name, default=None, getclass=False, getlink=False)`, `Group.move(source, dest)`, `Group.copy(source, dest, ...)`.
- traversal: `Group.visit(callable)`, `Group.visititems(callable)`, `Group.visit_links(callable)`, `Group.visititems_links(callable)`.
- dataset IO: `Dataset.__getitem__(selection)`, `Dataset.__setitem__(selection, value)`, `Dataset.read_direct(dest, source_sel=None, dest_sel=None)`, `Dataset.write_direct(source, source_sel=None, dest_sel=None)`, `Dataset.astype(dtype)`, `Dataset.asstr(encoding=None, errors='strict')`, `Dataset.fields(names)`, `Dataset.iter_chunks(sel=None)`, `Dataset.resize(size, axis=None)`, `Dataset.flush()`, `Dataset.refresh()`, `Dataset.make_scale(name='')`.
- attributes: `AttributeManager.create(name, data, shape=None, dtype=None)`, `.modify(name, value)`, `.get(name, default=None)`.
- special dtypes: `string_dtype(encoding='utf-8', length=None)`, `vlen_dtype(basetype)`, `enum_dtype(values_dict, basetype=numpy.uint8)`, `opaque_dtype(np_dtype)`, `special_dtype(**kwds)`, `ref_dtype` via `h5py.ref_dtype`/`h5py.regionref_dtype`.
- dtype inspection: `check_string_dtype(dt)`, `check_vlen_dtype(dt)`, `check_enum_dtype(dt)`, `check_opaque_dtype(dt)`, `check_ref_dtype(dt)`, `check_dtype(**kwds)`.
- file utilities: `is_hdf5(fname) -> bool`, `get_config()`, `register_driver(name, set_fapl)`, `unregister_driver(name)`, `registered_drivers()`, `enable_ipython_completer()`.

[EXCEPTIONS]:
- `KeyError` — name not present in a `Group` or `AttributeManager`.
- `OSError` — file open, driver, or HDF5 library-level failure surfaced from the C layer.
- `TypeError` / `ValueError` — dtype mismatch, incompatible shape, or invalid selection on dataset IO.

[IMPLEMENTATION_LAW]:
- `File` is a context manager; open in a `with` block so the underlying HDF5 file handle flushes and closes deterministically, since dangling handles corrupt or lock the file.
- Datasets read and write through NumPy-style fancy indexing on `__getitem__`/`__setitem__`; `read_direct`/`write_direct` avoid intermediate copies for large transfers into a preallocated buffer.
- Chunked storage is mandatory for compression, resizing, and partial IO; `chunks`, `compression`, `shuffle`, `fletcher32`, and `scaleoffset` are set at `create_dataset` time and are immutable afterward.
- Special dtypes (variable-length strings, enums, references) are constructed with `string_dtype`/`vlen_dtype`/`enum_dtype` and inspected with the matching `check_*` predicate; raw NumPy dtypes do not carry HDF5 type metadata.
- Virtual datasets map regions of external source datasets without copying: build a `VirtualLayout`, assign `VirtualSource` slices, then `create_virtual_dataset`.
- `require_dataset`/`require_group` are idempotent get-or-create entries; `create_*` raises on an existing name, so the require form replaces existence-check branching.
- SWMR mode (`swmr=True` or `swmr_mode`) admits a single writer and multiple concurrent readers; `Dataset.refresh()`/`flush()` synchronise the reader and writer views.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `h5py`
- Owns: HDF5 file IO, hierarchical group namespace, typed chunked/compressed dataset storage, NumPy-style slicing, attribute metadata, special dtypes, virtual datasets, and SWMR access
- Accept: `File` as a context-managed container, `create_dataset` with chunk/compression policy at creation, NumPy-style slicing for IO, `string_dtype`/`vlen_dtype`/`enum_dtype` for special types, `require_*` for idempotent structure
- Reject: leaked file handles outside `with`, post-creation chunk/compression mutation, raw NumPy dtypes where HDF5 special-type metadata is required, and existence-check branching where `require_*` is the idempotent entry
