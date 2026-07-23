# [PY_DATA_API_AWKWARD]

`awkward` owns variable-length, nested, and ragged array capability over columnar memory: `ak.Array` wraps an irregular payload with NumPy ufunc and jagged-indexing behavior over typed `ak.contents` layouts, `ak.Record` holds one row, and `ak.ArrayBuilder` grows an array by append. Construction and export functions bridge NumPy, Arrow, Parquet, JSON, and pandas, and one array dispatches across `cpu`, `cuda`, and `jax` backends without copying layout structure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `awkward`
- package: `awkward` (`BSD-3-Clause`)
- module: `awkward` (alias `ak`)
- namespaces: `ak.contents`, `ak.forms`, `ak.types`, `ak.behaviors`, `ak.numba`, `ak.jax`
- rail: irregular-arrays

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core array types

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     | [CAPABILITY]                                     |
| :-----: | :---------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `ak.Array`        | high-level array  | variable-length nested array with behavior mixin |
|  [02]   | `ak.Record`       | single record     | one row from a record-typed array                |
|  [03]   | `ak.ArrayBuilder` | incremental build | append-based construction of irregular arrays    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `ak.ArrayBuilder` members

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `snapshot()`                                                          | method   | materialize appended layout to `ak.Array` |
|  [02]   | `append(obj)` / `extend(iterable)`                                    | method   | append one value or extend from iterable  |
|  [03]   | `integer(x)` / `real(x)` / `complex(x)` / `boolean(x)`                | method   | append a typed scalar                     |
|  [04]   | `string(s)` / `bytestring(b)`                                         | method   | append a string or bytestring             |
|  [05]   | `null()`                                                              | method   | append an option-type `None`              |
|  [06]   | `begin_list()` / `end_list()` / `list()`                              | method   | open, close, or context a list level      |
|  [07]   | `begin_record(name)` / `field(key)` / `end_record()` / `record(name)` | method   | open, key, close, or context a record     |
|  [08]   | `begin_tuple(n)` / `index(i)` / `end_tuple()` / `tuple(n)`            | method   | open, slot, close, or context a tuple     |
|  [09]   | `datetime(x)` / `timedelta(x)`                                        | method   | append a temporal scalar                  |
|  [10]   | `to_list()` / `tolist()` / `to_numpy()` / `show(...)`                 | method   | export to Python/NumPy or preview build   |
|  [11]   | `type` / `typestr` / `behavior` / `attrs` / `numba_type`              | property | live build type, metadata, and Numba type |

[ENTRYPOINT_SCOPE]: `ak.Array` members

| [INDEX] | [SURFACE]                         | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :-------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `layout`                          | property | underlying `ak.contents` layout                            |
|  [02]   | `fields` / `is_tuple`             | property | record field names; whether the top record is unnamed      |
|  [03]   | `type` / `typestr`                | property | high-level type descriptor and its string form             |
|  [04]   | `ndim` / `nbytes`                 | property | nesting depth and byte footprint                           |
|  [05]   | `mask`                            | property | option-aware boolean masking accessor (`array.mask[cond]`) |
|  [06]   | `behavior` / `attrs`              | property | behavior-mixin mapping and top-level metadata dict         |
|  [07]   | `named_axis` / `positional_axis`  | property | named-axis mapping and positional axis tuple               |
|  [08]   | `to_list()` / `tolist()`          | method   | convert to nested Python lists                             |
|  [09]   | `to_numpy(allow_missing)`         | method   | convert to a NumPy array                                   |
|  [10]   | `show(limit_rows, type, ...)`     | method   | formatted preview of contents                              |
|  [11]   | `numba_type` / `cpp_type`         | property | Numba and cppyy type handles for JIT/C++ kernels           |
|  [12]   | `getitem` / `setitem` / `getattr` | dunder   | jagged slicing, field assignment, dotted field access      |
|  [13]   | `array_ufunc` / `array_function`  | dunder   | NumPy ufunc and array-function dispatch over the layout    |

[ENTRYPOINT_SCOPE]: construction (`ak`)
- shape: factory, returns `ak.Array`

| [INDEX] | [SURFACE]                                                                                        | [CAPABILITY]         |
| :-----: | :----------------------------------------------------------------------------------------------- | :------------------- |
|  [01]   | `from_iter(iterable, *, allow_record, highlevel, behavior, attrs, initial, resize)`              | from Python iterable |
|  [02]   | `from_numpy(array, *, regulararray, recordarray, highlevel, behavior, primitive_policy, attrs)`  | from NumPy array     |
|  [03]   | `from_arrow(array, *, generate_bitmasks, highlevel, behavior, attrs)`                            | from Arrow producer  |
|  [04]   | `from_json(source, *, line_delimited, schema, nan_string, buffersize, highlevel, ...)`           | from JSON            |
|  [05]   | `from_parquet(path, *, columns, row_groups, storage_options, generate_bitmasks, highlevel, ...)` | from Parquet file    |
|  [06]   | `from_buffers(form, length, container, buffer_key, *, backend, byteorder, highlevel, ...)`       | from form + buffers  |
|  [07]   | `from_rdataframe(rdf, columns, *, keep_order, offsets_type, with_name, highlevel, ...)`          | from ROOT RDataFrame |

[ENTRYPOINT_SCOPE]: export (`ak`)
- shape: static, consumes `ak.Array`

| [INDEX] | [SURFACE]                                                               | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `to_numpy(array, *, allow_missing)`                                     | to NumPy array                              |
|  [02]   | `to_list(array)`                                                        | to Python list                              |
|  [03]   | `to_arrow(array, *, list_to32, extensionarray, count_nulls, ...)`       | to Arrow `pyarrow.Array` (`arrow_c_array`)  |
|  [04]   | `to_arrow_table(array, *, list_to32, extensionarray, ...)`              | to Arrow `pyarrow.Table` (`arrow_c_stream`) |
|  [05]   | `to_parquet(array, destination, *, compression, ...)`                   | to Parquet file                             |
|  [06]   | `to_json(array, file, *, line_delimited, nan_string, ...)`              | to JSON                                     |
|  [07]   | `to_dataframe(array, *, how, levelname, anonymous)`                     | to pandas MultiIndex frame                  |
|  [08]   | `to_backend(array, backend, *, highlevel, behavior, attrs)`             | move to named backend                       |
|  [09]   | `to_layout(array, *, allow_record, none_policy, primitive_policy, ...)` | descend to `ak.contents`                    |
|  [10]   | `to_buffers(array, container, buffer_key, form_key, *, ...)`            | decompose to `(form, length, container)`    |
|  [11]   | `to_packed(array, *, highlevel, behavior, attrs)`                       | compact contiguous layout copy              |

[ENTRYPOINT_SCOPE]: transform and structure (`ak`)
- reduction carry: `(array, axis, *, keepdims, mask_identity, ...)`; weighted stats add `weight`/`ddof`, `min`/`max` add `initial`

| [INDEX] | [SURFACE]                                                                    | [SHAPE] | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `concatenate(arrays, axis, *, mergebool, highlevel, ...)`                    | static  | concatenate along axis                  |
|  [02]   | `zip(arrays, depth_limit, *, with_name, ...)` / `unzip(array, *, how)`       | static  | zip or split records                    |
|  [03]   | `flatten(array, axis, ...)` / `unflatten(array, counts, axis, ...)`          | static  | remove or add a nesting level           |
|  [04]   | `where(condition, *args, mergebool, ...)`                                    | static  | conditional element select              |
|  [05]   | `sort(array, axis, *, ascending, stable, ...)` / `argsort(array, axis, ...)` | static  | sort along axis or get order            |
|  [06]   | `pad_none(array, target, axis, *, clip)` / `fill_none(array, value, axis)`   | static  | pad to length or fill option None       |
|  [07]   | `drop_none` / `is_none` / `mask(array, mask, *, valid_when)`                 | static  | drop, test, or apply option mask        |
|  [08]   | `with_field(array, what, where, ...)` / `without_field(array, where, ...)`   | static  | add or remove a record field            |
|  [09]   | `with_name(array, name, ...)`                                                | static  | attach record type name                 |
|  [10]   | `cartesian(arrays, axis)` / `combinations(array, n, *, replacement)`         | static  | per-list pairings or n-tuples           |
|  [11]   | `num` / `count` / `count_nonzero`                                            | fold    | per-list lengths and non-null counts    |
|  [12]   | `sum` / `prod` / `mean` / `min` / `max` / `any` / `all`                      | fold    | core axis reductions                    |
|  [13]   | `std` / `var` / `ptp` / `moment` / `corr` / `linear_fit`                     | fold    | dispersion, moment, and bivariate stats |
|  [14]   | `fields(array)` / `type(array, *, behavior)` / `backend(*arrays)`            | static  | field names, type, backend              |
|  [15]   | `firsts(...)` / `singletons(...)` / `local_index(...)` / `run_lengths(...)`  | static  | per-list first/singleton/index/runs     |
|  [16]   | `values_astype(array, to)` / `enforce_type(array, type)` / `ravel` / `copy`  | static  | value-cast, type-enforce, flatten, copy |
|  [17]   | `with_parameter(array, parameter, value)` / `parameters(array)`              | static  | parameter set and get                   |
|  [18]   | `validity_error(array)`                                                      | static  | layout validity check                   |
|  [19]   | `metadata_from_parquet(path, *, storage_options, row_groups, ...)`           | static  | parquet metadata without reading rows   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ak.Array` wraps a typed `ak.contents` layout; `to_layout` descends to it and `from_buffers` rebuilds an array from a form and its buffers with no data copy
- backend dispatch: `backend(*arrays)` reads the current backend and `to_backend` moves data across `cpu`, `cuda`, `jax` while preserving layout structure
- option type: `None` entries are `OptionType` layouts that `fill_none`, `drop_none`, `is_none`, and `mask` operate on and `pad_none` produces
- behaviors: custom methods and overloads attach via the `behavior` mapping keyed on record type name, and `with_name` tags arrays so behaviors resolve
- forms: `ak.forms` carries the form algebra that builds typed arrays from buffers without materializing data, the basis of zero-copy interchange

[STACKING]:
- `arro3-core`(`.api/arro3-core.md`): `ak.to_arrow(extensionarray=...)` emits a `pyarrow` array carrying awkward's type as Arrow extension metadata, whose `__arrow_c_array__` feeds `Array.from_arrow` zero-copy
- `pyarrow`(`.api/pyarrow.md`): `ak.to_arrow_table` round-trips a record array as a `pyarrow.Table` over `__arrow_c_stream__`, and `ak.from_arrow` re-imports any PyCapsule producer including whole tables
- `numpy`(`libs/python/.api/numpy.md`): `ak.Array.__array_ufunc__` runs NumPy ufuncs over the jagged layout in place (`np.sqrt(events.pt)`); `to_numpy`/`from_numpy` is the terminal regular-array escape
- `jax`(`libs/python/compute/.api/jax.md`): `to_backend(array, "jax")` moves the layout onto a JAX backend registered via `ak.jax.register_and_check()`, so reductions and ufuncs stay autodiff-traceable without leaving the layout
- `numba`(`libs/python/compute/.api/numba.md`): `ak.Array.numba_type` and the `ak.numba` registration let `@numba.njit` kernels iterate the typed layout directly, the escape when a reduction has no axis-vectorized form
- `pandas`(`.api/pandas.md`): `ak.to_dataframe(how=...)` explodes nested fields into a MultiIndex frame as a terminal tabular projection
- `data`: irregular event payloads compose as `ak.Array` with named fields; element math rides `__array_ufunc__` and backend moves ride `to_backend`, so the layout never round-trips through a regular NumPy array

[LOCAL_ADMISSION]:
- Irregular event-data payloads enter as `ak.Array` with named fields and an explicit backend; the field names and type descriptor join the array-admission record
- Field access uses `fields` and named indexing, keeping the record structure self-describing
- Arrow and Parquet interchange uses `from_arrow`/`to_arrow` and `from_parquet`/`to_parquet`; cloud paths pass `storage_options` resolved through `fsspec`
- Custom domain methods attach through the `behavior` mapping keyed on record name and resolved by `with_name`

[RAIL_LAW]:
- Package: `awkward`
- Owns: variable-length nested arrays, option types, record and union arrays, behavior mixins, and multi-backend (`cpu`/`cuda`/`jax`) columnar irregular data
- Accept: irregular event-data payloads wrapped in `ak.Array` with named fields and an explicit backend
- Reject: hand-rolled ragged list structures, positional-only field access where `fields` applies, NumPy arrays for irregular data awkward owns, Python loops where `__array_ufunc__` or a Numba/JAX backend kernel applies, and `ak.Array` subclassing where the `behavior` mapping applies
