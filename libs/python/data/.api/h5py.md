# [PY_DATA_API_H5PY]

`h5py` supplies a Pythonic interface to HDF5 files exposing groups, datasets, attributes, and virtual datasets with NumPy-style slicing and dtype semantics. `File` owns the on-disk container, `Group` owns the hierarchical namespace, `Dataset` owns typed n-dimensional array storage, and `AttributeManager` owns per-object metadata. Datasets read and write as `numpy` arrays at the boundary, so an h5py read is the array source for the compute rail and an h5py write is the durable sink for a `numpy`/`pyarrow`-shaped result; the low-level `h5py.h5*` modules expose the C HDF5 API directly when the high-level wrappers do not reach a property-list knob, and h5py never re-implements the HDF5 codec/filter pipeline the C library owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `h5py`
- package: `h5py`
- import: `import h5py`
- owner: `data`
- rail: array
- version: `3.16.0`
- entry points: library use is import-only; `h5py.run_tests()` runs the bundled suite
- capability: HDF5 file IO, hierarchical group namespace, typed chunked/compressed dataset storage, NumPy-style slicing, attribute metadata, special dtypes (vlen, enum, string, complex, opaque, reference), virtual datasets, dimension scales, filter pipeline (`h5py.filters`), SWMR access, and the full low-level C API (`h5f`/`h5d`/`h5g`/`h5p`/`h5s`/`h5t`/`h5a`/`h5o`/`h5l`/`h5r`/`h5z`/`h5fd`/`h5ds`/`h5i`/`h5pl`)

## [02]-[CAPTURE]

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
- `h5py.UNLIMITED` — sentinel for an unlimited `maxshape` axis on a resizable dataset.

[ENTRYPOINTS]:
- group creation: `Group.create_dataset(name, shape=None, dtype=None, data=None, *, chunks=None, maxshape=None, compression=None, compression_opts=None, shuffle=None, fletcher32=None, scaleoffset=None, fillvalue=None, track_order=None, external=None) -> Dataset`, `Group.require_dataset(name, shape, dtype, exact=False, **kwds) -> Dataset`, `Group.create_dataset_like(name, other, **kwds)`, `Group.create_group(name, track_order=None) -> Group`, `Group.require_group(name) -> Group`.
- virtual datasets: `Group.create_virtual_dataset(name, layout, fillvalue=None) -> Dataset`, `Group.build_virtual_dataset()` (context manager), `Dataset.virtual_sources()`.
- linking and structure: `Group.__setitem__(name, SoftLink|ExternalLink|HardLink|object)`, `Group.get(name, default=None, getclass=False, getlink=False)`, `Group.move(source, dest)`, `Group.copy(source, dest, ...)`.
- traversal: `Group.visit(callable)`, `Group.visititems(callable)`, `Group.visit_links(callable)`, `Group.visititems_links(callable)`.
- dataset IO: `Dataset.__getitem__(selection)`, `Dataset.__setitem__(selection, value)`, `Dataset.read_direct(dest, source_sel=None, dest_sel=None)`, `Dataset.write_direct(source, source_sel=None, dest_sel=None)`, `Dataset.astype(dtype)`, `Dataset.asstr(encoding=None, errors='strict')`, `Dataset.fields(names)`, `Dataset.iter_chunks(sel=None)`, `Dataset.resize(size, axis=None)`, `Dataset.flush()`, `Dataset.refresh()`, `Dataset.make_scale(name='')`.
- attributes: `AttributeManager.create(name, data, shape=None, dtype=None)`, `.modify(name, value)`, `.get(name, default=None)`.
- special dtypes: `string_dtype(encoding='utf-8', length=None)`, `vlen_dtype(basetype)`, `enum_dtype(values_dict, basetype=numpy.uint8)`, `opaque_dtype(np_dtype)`, `complex_compat_dtype(real_dtype, imag_name)`, `special_dtype(**kwds)`, the `ref_dtype`/`regionref_dtype` object-reference dtypes.
- dtype inspection: `check_string_dtype(dt)`, `check_vlen_dtype(dt)`, `check_enum_dtype(dt)`, `check_opaque_dtype(dt)`, `check_complex_dtype(dt)`, `check_ref_dtype(dt)`, `check_dtype(**kwds)`.
- filter pipeline (`h5py.filters`): the `compression` knob accepts `'gzip'`/`'lzf'`/`'szip'` or a registered filter id; `h5py.filters` exposes `encode`/`decode`, `fill_dcpl` (build the dataset creation property list from filter args), `guess_chunk` (default chunk-shape heuristic), `get_filters`, and the `Gzip` filter-ref helper, and third-party plugins resolve through `h5pl` (the HDF5 plugin path) — set them at `create_dataset` time, never re-encode bytes in Python.
- file utilities: `is_hdf5(fname) -> bool`, `get_config()`, `register_driver(name, set_fapl)`, `unregister_driver(name)`, `registered_drivers() -> frozenset` (`{'sec2','stdio','core','split','family','fileobj','mpio'}`), `enable_ipython_completer()`.
- low-level C API: `h5py.h5f` (files), `h5d` (datasets), `h5g` (groups), `h5p` (property lists), `h5s` (dataspaces/selections), `h5t` (datatypes), `h5a` (attributes), `h5o`/`h5l` (objects/links), `h5r` (references), `h5z` (filters), `h5fd` (drivers), `h5ds` (dimension scales), `h5i` (identifiers), `h5pl` (filter plugins) — the escape hatch when a property-list or selection knob is not reachable from the high-level wrappers.

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

[INTEGRATION_LAW]:
- numpy seam: `Dataset[...]` returns a `numpy.ndarray` and `Dataset[...] = arr` writes one; `read_direct(dest, source_sel, dest_sel)`/`write_direct` move into a preallocated `numpy` buffer with no intermediate copy for large transfers. The dataset is the durable backing for a compute-rail array — read once into the array kernel, never iterate scalar reads.
- chunk/codec discipline: `chunks`, `compression`, `shuffle`, `fletcher32`, `scaleoffset`, and `fillvalue` are fixed at `create_dataset`; choose a chunk shape matching the dominant read selection so `Dataset[sel]` touches whole chunks, and pair `shuffle=True` with `compression='gzip'` for typed numeric arrays — this is the on-disk analogue of a columnar codec, configured once.
- virtual-dataset seam: assemble many per-file datasets into one logical array with `VirtualLayout`/`VirtualSource` + `create_virtual_dataset` instead of concatenating in memory; the VDS reads source regions lazily, mirroring a partitioned table without materializing it.
- icechunk/zarr boundary: h5py owns the HDF5 container; the chunked-array rail owner (`icechunk`/zarr) owns transactional, object-store chunked arrays. Route durable single-file scientific arrays through h5py and versioned cloud-native chunked arrays through the icechunk owner — do not re-implement chunk indexing in either against the other.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `h5py`
- Owns: HDF5 file IO, hierarchical group namespace, typed chunked/compressed dataset storage, NumPy-style slicing, attribute metadata, special dtypes, the filter pipeline, virtual datasets, dimension scales, SWMR access, and the low-level C API
- Accept: `File` as a context-managed container, `create_dataset` with chunk/compression policy at creation, NumPy-style slicing and `read_direct`/`write_direct` for IO, `string_dtype`/`vlen_dtype`/`enum_dtype`/`complex_compat_dtype` for special types, `VirtualLayout`/`VirtualSource` for logical assembly, `require_*` for idempotent structure, the `h5*` low-level modules as the property-list escape hatch
- Reject: leaked file handles outside `with`, post-creation chunk/compression mutation, raw NumPy dtypes where HDF5 special-type metadata is required, per-scalar read/write loops where a slice/`read_direct` reads the block, re-encoding compressed bytes in Python instead of the filter pipeline, and existence-check branching where `require_*` is the idempotent entry
