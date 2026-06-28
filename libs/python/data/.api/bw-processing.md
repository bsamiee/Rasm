# [PY_DATA_API_BW_PROCESSING]

`bw-processing` (import `bw_processing`) is the matrix-processing substrate of the Brightway LCA cluster: it owns the on-disk/in-memory *data package* format — a Frictionless-style bundle of `numpy` structured arrays plus JSON metadata over an `fsspec` filesystem — that encodes every LCA matrix as COO triples (an `INDICES_DTYPE` `(row, col)` index array, a float `data` array, an optional `flip` sign array, and an optional `UNCERTAINTY_DTYPE` distributions array for Monte Carlo). It is the wire format on both edges of the calculation: `bw2data` *writes* it (`Database.process()` / `Method.process()` emit a `Datapackage`), and `bw2calc` *reads* it (its `matrix_utils.MappedMatrix` maps the resource groups into `scipy` sparse matrices). `bw_processing` never solves, never holds a graph, and never reaches a database — it is the pure serialization layer between the graph store and the solver, and it never re-implements the `fsspec` filesystem, the `numpy` structured-array buffer, or the `scipy` sparse assembly its consumers own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw-processing`
- package: `bw-processing`
- import: `import bw_processing as bwp`
- owner: `data`
- rail: lca-substrate (EPD/LCA cluster)
- version: `1.5`
- license: `BSD-3-Clause` (`LICENSE` ships in `dist-info/licenses/`; Copyright Chris Mutel)
- asset: pure Python (zero compiled extensions) — `numpy` structured arrays serialized over an `fsspec` `AbstractFileSystem`; `Requires-Python >=3.9`
- depends-on: `fsspec` (the filesystem abstraction the datapackage is written through), `jsonschema>=4.0` (datapackage/label schema validation), `morefs` (the in-memory / overlay filesystems), `numpy`, `pandas` (CSV-metadata resources)
- marker: COMPANION-GATED. Pinned `bw-processing; python_version<'3.15'`. The package is pure-Python, so the gate is TRANSITIVE, not a source floor: its cluster siblings (`bw2data`, `bw2calc`) pull `numpy<3`, `scipy`, `pandas`, `lxml`, `peewee`, `rapidfuzz` whose `cp315` wheels are not yet published, so the whole Brightway cluster pins `<3.15`. `assay api resolve bw-processing` cannot reflect on the active `cp315` interpreter; this surface is verified against the real `bw_processing 1.5` wheel on an isolated `cp313` install and is authoritative until the scientific stack ships `cp315` wheels and the marker is removed.
- entry points: library-only; no console script
- capability: build a multi-matrix `Datapackage` from persistent (static) or dynamic (interface-driven) COO vectors/arrays; serialize each matrix group as `numpy` `.npy` (default) or `parquet` arrays plus a Frictionless `datapackage.json`; persist to a directory, a single zip archive, an in-memory filesystem, or any `fsspec` backend; carry per-resource CSV/JSON metadata; filter a datapackage to a subset of resource groups without rewriting; merge two datapackages under a boolean mask; reindex/reset the integer matrix ids; and round-trip the bundle with optional `mmap` and lazy proxy loading

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `bwp.Datapackage` — the writable bundle: a set of named *resource groups*, each group holding the arrays for one matrix (`indices`, `data`, optional `flip`, optional `distributions`) plus JSON/CSV metadata. Exposes `.resources` (the resource descriptors) and `.groups` (the grouped arrays). `DatapackageBase` is the read/filter base; `FilteredDatapackage` is the read-only projection returned by filtering.
- `bwp.MatrixSerializeFormat` — enum `NUMPY` (`'numpy'`, the default, `.npy` arrays) | `PARQUET` (`'parquet'`); selects the on-disk array codec per datapackage or per dynamic group.
- `bwp.INDICES_DTYPE` — `[('row', int64), ('col', int64)]`; the structured dtype of every matrix index array (the COO coordinates that `matrix_utils` later maps to dense matrix positions).
- `bwp.UNCERTAINTY_DTYPE` — `[('uncertainty_type', uint8), ('loc', f32), ('scale', f32), ('shape', f32), ('minimum', f32), ('maximum', f32), ('negative', bool)]`; the `stats_arrays` distribution row consumed by `bw2calc` Monte Carlo (`use_distributions=True`).
- `bwp.DEFAULT_LICENSES` — the default datapackage license list (`ODC-PDDL-1.0`).
- `bwp.UndefinedInterface` — the placeholder a dynamic (interface-backed) resource dehydrates to on serialization; `rehydrate_interface` swaps the live object back in.
- label schemas: `bwp.StringLabelSchema`, `bwp.ParamLabelSchema`, `bwp.AnyLabelSchema`, `bwp.ParamLabelField`, `bwp.ArrayEntry`, `bwp.MatrixEntry`, `bwp.MatrixName` — the typed shapes for parameterized-array labels and entry iterables.

[CONSTRUCTION]:
- `create_datapackage(fs=None, name=None, id_=None, metadata=None, combinatorial=False, sequential=False, seed=None, sum_intra_duplicates=True, sum_inter_duplicates=False, matrix_serialize_format_type=MatrixSerializeFormat.NUMPY) -> Datapackage` — the factory; `fs` is the target `fsspec` filesystem (omit for in-memory), and the `combinatorial`/`sequential`/`seed` knobs control how multiple array columns are sampled during stochastic iteration. The duplicate flags decide whether duplicate COO entries within (`intra`) or across (`inter`) groups sum or override.
- `load_datapackage(fs_or_obj, mmap_mode=None, proxy=False) -> Datapackage` — reload from a filesystem or an existing `DatapackageBase`; `mmap_mode` memory-maps the arrays and `proxy=True` defers array reads until accessed (large-background-database path).
- `create_datapackage_from_entries(data, fs=None, **metadata)` / `simple_graph(data, fs=None, **metadata) -> Datapackage` — one-call builders from an entries dict / a `{matrix: [(row, col, value), ...]}` adjacency dict.
- `create_array(iterable, nrows=None, dtype=numpy.float32)` / `create_structured_array(iterable, dtype, nrows=None, sort=False, sort_fields=None)` — stream an iterable into a (structured) `numpy` array, sized lazily by `nrows`.
- `generic_directory_filesystem(*, dirpath) -> DirFileSystem` / `generic_zipfile_filesystem(*, dirpath, filename, write=True, compression=8, compresslevel=None) -> ZipFileSystem` — the two stock `fsspec` targets (directory tree | single zip) to pass as `create_datapackage(fs=...)`.

[MATRIX_BUILD]: `Datapackage` methods (the COO-group surface)
- `add_persistent_vector(*, matrix, indices_array, name=None, data_array=None, flip_array=None, distributions_array=None, ...)` — the core call: one static matrix group from an `INDICES_DTYPE` index array plus a `data` array (and optional sign-`flip` / `UNCERTAINTY_DTYPE` `distributions`).
- `add_persistent_array(*, matrix, data_array, indices_array, name=None, flip_array=None, ...)` — a 2-D `data_array` group whose columns are pre-sampled scenarios (array-iteration via `use_arrays=True`).
- `add_persistent_vector_from_iterator(*, matrix=None, name=None, dict_iterator=None, nrows=None, ...)` — stream rows from an iterator of `{row, col, amount, ...}` dicts without materializing arrays first.
- `add_dynamic_vector(*, matrix, interface, indices_array, name=None, ...)` / `add_dynamic_array(*, matrix, interface, indices_array, ...)` — a group whose `data` is produced at solve time by a live `interface` object (the dehydrate/rehydrate path).
- `add_entries(*, matrix, entries, name=None)` / `add_array_entries(*, matrix, entries)` — append typed `MatrixEntry` / `ArrayEntry` iterables.
- `add_csv_metadata(*, dataframe, valid_for, name=None)` / `add_json_metadata(*, data, valid_for, name=None)` — attach a `pandas` table / JSON blob scoped to named resource groups.
- `filter_by_attribute(key, value) -> FilteredDatapackage` / `exclude(filters) -> FilteredDatapackage` — project to a subset of resource groups (the selective-load path `bw2calc` uses).
- `get_resource(name_or_index) -> (array, descriptor)`, `del_resource(name_or_index)`, `del_resource_group(name)`, `get_max_index_value()`, `rehydrate_interface(name_or_index, resource, initialize_with_config=False)`, `dehydrated_interfaces() -> list[str]`, `finalize_serialization()`, `write_modified()`.

[TRANSFORMS]:
- `merge_datapackages_with_mask(first_dp, first_resource_group_label, second_dp, second_resource_group_label, mask_array, output_fs=None, metadata=None) -> DatapackageBase` — splice two datapackages selecting `first` where `mask` is true, `second` elsewhere (the scenario-overlay path `premise` and prospective workflows lean on).
- `reindex(datapackage, metadata_name, data_iterable, fields=None, id_field_datapackage='id', id_field_destination='id')` — remap the integer matrix ids in a datapackage to a new id space using a metadata join (the `bw2data` mapping rebuild).
- `reset_index(datapackage, metadata_name) -> Datapackage` — collapse ids to a dense `0..n` range.
- `as_unique_attributes(data, exclude=None, include=None, raise_error=False)` / `schema_from_json_schema(...)` / `safe_filename(string, add_hash=True, full=False)` / `clean_datapackage_name(name)` / `md5(...)` — naming, hashing, and schema helpers.

[IMPLEMENTATION_LAW]:
- A `Datapackage` is a collection of resource *groups*; one group encodes one matrix's contribution. The index array is always `INDICES_DTYPE` (`row`/`col` int64 ids in the `bw2data` mapping space, not dense positions); `matrix_utils` resolves them to dense positions at solve time. Never store dense row/col offsets — store the global integer ids.
- Persistent vs dynamic is the static/live axis: a persistent vector/array bakes the numbers at write time; a dynamic vector/array defers to an `interface` evaluated per iteration. Persistent arrays (2-D) and `distributions` arrays are the two stochastic sources — `use_arrays=True` cycles array columns, `use_distributions=True` samples the `UNCERTAINTY_DTYPE` rows via `stats_arrays`.
- Serialization is `fsspec`-routed end to end: the same `Datapackage` writes to a directory, a zip, or `morefs` memory by swapping the `fs`; there is no bespoke file IO. `mmap_mode`/`proxy=True` on load keep large background datapackages out of resident memory.
- `filter_by_attribute`/`exclude` return a `FilteredDatapackage` that shares the underlying arrays — selective loading without a copy. `del_resource_group` mutates in place and needs `write_modified()` to persist.

[SUBSTRATE_STACK]: stacking onto the universal Python rails (`libs/python/.api/`)
- `numpy`: `INDICES_DTYPE`/`UNCERTAINTY_DTYPE` are `numpy` structured dtypes; `create_structured_array` and every group array is a `numpy` buffer — compose array construction with the `numpy` rail, never Python lists.
- `universal-pathlib` / `fsspec`: a `Datapackage` IS an `fsspec` datapackage. Pass an `obstore`/`UPath`-derived `AbstractFileSystem` as `create_datapackage(fs=...)` to land the bundle directly in the artifact store / object store; `generic_zipfile_filesystem` produces the single-file archive for content-addressed caching.
- `msgspec`: `add_json_metadata(data=...)` carries arbitrary JSON — type it with a `msgspec.Struct` at the boundary rather than a bare dict so the metadata is schema-checked on the way in.
- `pandas` -> `narwhals`/`polars`: `add_csv_metadata(dataframe=...)` ingests a `pandas` table; build it through the tabular rail and hand the frame across.

[SIBLING_STACK]: stacking with the EPD/LCA folder cluster
- `bw2data`: `Database.process()` / `Method.process()` are the producers — they call `add_persistent_vector` under the hood to emit the datapackage; `reindex` rebuilds ids against the `bw2data` mapping store.
- `bw2calc`: the consumer — `LCA(data_objs=[...])` takes a list of these datapackages (or `fsspec` paths to them), and `matrix_utils.MappedMatrix` maps each resource group's `INDICES_DTYPE` triples into the `scipy` technosphere/biosphere/characterization matrices. `bw_processing` is the contract between the two; build `data_objs` through `bw2data.prepare_lca_inputs`, not by hand.
- `premise`: prospective-scenario workflows use `merge_datapackages_with_mask` to overlay future-scenario coefficients onto a baseline background datapackage; the modern `bw_processing>=1.0` line arrives transitively via premise's `bw2calc>=2.0.1` (`premise[bw25]`).

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `bw-processing`
- Owns: the Brightway data-package format — multi-matrix COO bundles of `numpy` structured arrays (`INDICES_DTYPE` indices, `data`, `flip`, `UNCERTAINTY_DTYPE` distributions) plus JSON/CSV metadata, serialized over `fsspec` to directory/zip/memory, with persistent + dynamic groups, filtering, masked merge, and id reindexing
- Accept: `create_datapackage(fs=...)` + `add_persistent_vector`/`add_persistent_array`/`add_dynamic_vector` as the build surface; `INDICES_DTYPE`/`UNCERTAINTY_DTYPE` as the only matrix array dtypes; `load_datapackage(..., mmap_mode=, proxy=True)` for large backgrounds; `filter_by_attribute`/`exclude` for selective load; `merge_datapackages_with_mask`/`reindex` for scenario and id work; `generic_zipfile_filesystem`/`generic_directory_filesystem` (or any `fsspec` fs) as the serialization target
- Reject: storing dense matrix positions instead of global ids; hand-rolled `.npy`/zip IO when the `fsspec` serialization owns it; per-row Python assembly of matrix arrays when `create_structured_array`/`add_persistent_vector_from_iterator` stream them; re-implementing the COO->sparse mapping that `matrix_utils` owns on the `bw2calc` side; bypassing `bw2data.process()` to author datapackages a database already serializes
