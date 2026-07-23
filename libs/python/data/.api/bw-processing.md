# [PY_DATA_API_BW_PROCESSING]

`bw-processing` (import `bw_processing`) mints the Brightway data-package format — the `fsspec`-served bundle of `numpy` structured arrays and JSON metadata that encodes every LCA matrix as COO triples (`INDICES_DTYPE` indices, a float `data` array, an optional sign `flip`, an optional `reference` production-exchange marker, an optional `UNCERTAINTY_DTYPE` distributions array). It is the serialization contract between the graph store and the solver — `bw2data` writes it, `bw2calc` maps it into `scipy` sparse matrices — and never solves, holds a graph, or reaches a database.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `bw-processing`
- package: `bw-processing` (BSD-3-Clause)
- module: `bw_processing` (import `bwp`)
- owner: `data`
- rail: lca-substrate (EPD/LCA cluster)
- asset: pure-Python `py3-none-any` purelib, zero compiled extensions, ABI-agnostic
- depends: `fsspec` (the filesystem the bundle writes through), `jsonschema` (datapackage/label schema validation), `morefs` (in-memory + overlay filesystems), `numpy`, `pandas` (CSV-metadata resources); `pyarrow` is optional, required only for `MatrixSerializeFormat.PARQUET`
- capability: build a multi-matrix `Datapackage` from persistent or dynamic COO vectors and arrays; serialize each group as `.npy` or parquet with a Frictionless `datapackage.json`; persist to a directory, a zip, memory, or any `fsspec` backend; carry per-resource CSV/JSON metadata; filter to a group subset without rewriting; masked-merge two datapackages; reindex or reset the integer matrix ids; round-trip with `mmap` and lazy-proxy loading

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: bundle carriers, matrix dtypes, and label shapes

A `Datapackage` is a set of named resource *groups*, each group the arrays for one matrix (`indices`, `data`, optional `flip`, optional `distributions`); `.resources` exposes the descriptors and `.groups` the grouped arrays.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `Datapackage`           | class         | writable multi-matrix resource-group bundle           |
|  [02]   | `DatapackageBase`       | class         | read and filter base of every datapackage             |
|  [03]   | `FilteredDatapackage`   | class         | read-only projection filtering returns                |
|  [04]   | `MatrixSerializeFormat` | str-enum      | on-disk array codec `NUMPY` (default) or `PARQUET`    |
|  [05]   | `INDICES_DTYPE`         | dtype         | `(row, col)` int64 COO index dtype                    |
|  [06]   | `UNCERTAINTY_DTYPE`     | dtype         | `stats_arrays` Monte-Carlo distribution row dtype     |
|  [07]   | `MatrixEntry`           | class         | typed COO entry for `create_datapackage_from_entries` |
|  [08]   | `ArrayEntry`            | class         | typed array entry for `add_array_entries`             |
|  [09]   | `UndefinedInterface`    | class         | dehydrated placeholder for a dynamic resource         |
|  [10]   | `DEFAULT_LICENSES`      | constant      | default datapackage license list (`ODC-PDDL-1.0`)     |

[LABEL_SCHEMAS]: `StringLabelSchema` `ParamLabelSchema` `AnyLabelSchema` `ParamLabelField` `MatrixName` — the typed shapes for parameterized-array labels and the matrix-name vocabulary.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction (factories)

`create_datapackage` mints an empty bundle; `combinatorial`/`sequential`/`seed` drive stochastic column sampling and `sum_intra_duplicates`/`sum_inter_duplicates` decide whether duplicate COO entries within and across groups sum or override.

| [INDEX] | [SURFACE]                                                                               | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `create_datapackage(fs, *, seed, matrix_serialize_format_type) -> Datapackage`          | mint an empty in-memory bundle        |
|  [02]   | `load_datapackage(fs_or_obj, mmap_mode, proxy) -> Datapackage`                          | reload with `mmap` and lazy proxy     |
|  [03]   | `create_datapackage_from_entries(data, fs, **metadata) -> Datapackage`                  | build from typed `MatrixEntry` stream |
|  [04]   | `create_array(iterable, nrows, dtype) -> ndarray`                                       | stream an unstructured array          |
|  [05]   | `create_structured_array(iterable, dtype, *, sort, sort_fields) -> ndarray`             | stream a structured array             |
|  [06]   | `generic_directory_filesystem(*, dirpath) -> DirFileSystem`                             | directory `fs` target                 |
|  [07]   | `generic_zipfile_filesystem(*, dirpath, filename, write, compression) -> ZipFileSystem` | single-zip `fs` target                |

[ENTRYPOINT_SCOPE]: matrix build (`Datapackage` instance methods)
- add carry: `name`, `flip_array`, `rescale_array`, `reference_array`, `params_array`, `param_labels`, `param_label_schema`, `keep_proxy`, `matrix_serialize_format_type`.

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `add_persistent_vector(*, matrix, indices_array, data_array, distributions_array)` | static COO vector group                    |
|  [02]   | `add_persistent_array(*, matrix, data_array, indices_array)`                       | pre-sampled 2-D scenario columns           |
|  [03]   | `add_persistent_vector_from_iterator(*, matrix, dict_iterator, nrows)`             | stream rows without materializing          |
|  [04]   | `add_dynamic_vector(*, matrix, interface, indices_array)`                          | solve-time interface vector                |
|  [05]   | `add_dynamic_array(*, matrix, interface, indices_array)`                           | solve-time interface array                 |
|  [06]   | `add_entries(*, matrix, entries, name)`                                            | append typed `MatrixEntry` iterable        |
|  [07]   | `add_array_entries(*, matrix, entries)`                                            | append typed `ArrayEntry` iterable         |
|  [08]   | `add_csv_metadata(*, dataframe, valid_for, name)`                                  | attach a `pandas` table to groups          |
|  [09]   | `add_json_metadata(*, data, valid_for, name)`                                      | attach a JSON blob to groups               |
|  [10]   | `filter_by_attribute(key, value) -> FilteredDatapackage`                           | project to a matching group subset         |
|  [11]   | `exclude(filters) -> FilteredDatapackage`                                          | project excluding matched groups           |
|  [12]   | `get_resource(name_or_index) -> (array, descriptor)`                               | fetch one resource array + descriptor      |
|  [13]   | `del_resource(name_or_index)`                                                      | drop one resource                          |
|  [14]   | `del_resource_group(name)`                                                         | drop a group in place                      |
|  [15]   | `get_max_index_value() -> int`                                                     | largest integer matrix id                  |
|  [16]   | `rehydrate_interface(name_or_index, resource, *, initialize_with_config)`          | swap a live object into a dynamic resource |
|  [17]   | `dehydrated_interfaces() -> list[str]`                                             | names of dehydrated interfaces             |
|  [18]   | `finalize_serialization()`                                                         | flush the bundle to its `fs`               |
|  [19]   | `write_modified()`                                                                 | persist in-place group mutations           |

[ENTRYPOINT_SCOPE]: transforms (module functions)

| [INDEX] | [SURFACE]                                                                                  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `merge_datapackages_with_mask(first_dp, first_label, second_dp, second_label, mask_array)` | masked splice to a new `DatapackageBase` |
|  [02]   | `reindex(datapackage, metadata_name, data_iterable, *, fields)`                            | remap matrix ids via a metadata join     |
|  [03]   | `reset_index(datapackage, metadata_name) -> Datapackage`                                   | collapse ids to a dense `0..n`           |

[UTILITIES]: `as_unique_attributes` `as_unique_attributes_dataframe` `schema_from_json_schema` `safe_filename` `clean_datapackage_name` `md5` — the naming, hashing, uniqueness, and schema helpers.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One group encodes one matrix's contribution; its index array is `INDICES_DTYPE` global `bw2data` mapping ids, never dense positions, and `matrix_utils` resolves them to dense positions at solve time.
- `flip_array`, `rescale_array`, and `reference_array` are optional per-entry side-resources aligned to the index array: `flip` negates the sign, `rescale` multiplies before insertion, and `reference` (`kind="reference"`) marks the production exchange so `bw_graph_tools` reads it directly instead of inferring column structure. Each writes only when at least one entry sets it.
- Persistent bakes the numbers at write time; dynamic defers to an `interface` evaluated per iteration. Persistent 2-D arrays (`use_arrays=True`) and `UNCERTAINTY_DTYPE` distributions (`use_distributions=True`, via `stats_arrays`) are the two stochastic sources.
- Serialization routes through `fsspec` end to end: the same `Datapackage` writes to a directory, a zip, or `morefs` memory by swapping `fs`, and `mmap_mode`/`proxy=True` on load keep large background datapackages out of resident memory.
- `filter_by_attribute`/`exclude` return a `FilteredDatapackage` sharing the underlying arrays, so selective loading never copies; `del_resource_group` mutates in place and needs `write_modified()` to persist.

[STACKING]:
- `numpy`(`.api/numpy.md`): every group array and both matrix dtypes are `numpy` structured buffers — compose array construction through `create_structured_array`, never Python lists.
- `fsspec`+`universal-pathlib`(`.api/fsspec.md`, `.api/universal-pathlib.md`): a `Datapackage` IS an `fsspec` bundle; pass an `obstore`/`UPath`-derived `AbstractFileSystem` as `create_datapackage(fs=...)` to land it in the object store, and `generic_zipfile_filesystem` produces the content-addressed single-file archive.
- `msgspec`(`.api/msgspec.md`): type `add_json_metadata(data=...)` with a `msgspec.Struct` at the boundary so the metadata is schema-checked on the way in.
- `bw2data`(`.api/bw2data.md`): `Database.process()`/`Method.process()` emit the datapackage via `add_persistent_vector`, and `reindex` rebuilds ids against the `bw2data` mapping store; author `data_objs` through `prepare_lca_inputs`.
- `bw2calc`(`.api/bw2calc.md`): `LCA(data_objs=[...])` consumes these packages, and `matrix_utils.MappedMatrix` maps each group's `INDICES_DTYPE` triples into the `scipy` technosphere/biosphere/characterization matrices.
- `premise`(`.api/premise.md`): prospective-scenario workflows overlay future coefficients onto a baseline background datapackage through `merge_datapackages_with_mask`.
- `pandas` -> `narwhals`/`polars`(`.api/pandas.md`, `.api/narwhals.md`): build the `add_csv_metadata(dataframe=...)` table through the tabular rail and hand the frame across.

[LOCAL_ADMISSION]:
- `bw2data.process()` emits every datapackage and `bw2calc` consumes it; the data owner never hand-authors one, so `bw_processing` stays the id-keyed COO serialization contract between the two.

[RAIL_LAW]:
- Package: `bw-processing`
- Owns: the Brightway data-package format — multi-matrix COO bundles of `numpy` structured arrays (`INDICES_DTYPE` indices, `data`, `flip`, `UNCERTAINTY_DTYPE` distributions) with JSON/CSV metadata, `fsspec`-serialized to directory/zip/memory, with persistent and dynamic groups, filtering, masked merge, and id reindexing
- Accept: `create_datapackage(fs=...)` + `add_persistent_*`/`add_dynamic_*` as the build surface; `INDICES_DTYPE`/`UNCERTAINTY_DTYPE` as the only matrix array dtypes; `load_datapackage(mmap_mode=, proxy=True)` for large backgrounds; `filter_by_attribute`/`exclude` for selective load; `merge_datapackages_with_mask`/`reindex` for scenario and id work; any `fsspec` fs as the serialization target
- Reject: storing dense matrix positions instead of global ids; hand-rolled `.npy`/zip IO when the `fsspec` serialization owns it; per-row Python assembly when `create_structured_array`/`add_persistent_vector_from_iterator` streams it; re-implementing the COO-to-sparse mapping `matrix_utils` owns on the `bw2calc` side; bypassing `bw2data.process()` to author what a database already serializes
