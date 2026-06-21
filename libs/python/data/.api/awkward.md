# [PY_DATA_API_AWKWARD]

`awkward` supplies variable-length, nested, and ragged array structures over columnar memory for the data irregular-array rail. `ak.Array` wraps an irregular payload with NumPy-like ufunc and indexing behavior over typed layouts in `ak.contents`, `ak.Record` holds a single row, and `ak.ArrayBuilder` builds arrays incrementally. Construction and export functions bridge NumPy, Arrow, Parquet, JSON, and pandas, and the array dispatches across `"cpu"`, `"cuda"`, and `"jax"` backends without copying layout structure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `awkward`
- package: `awkward`
- version: `2.9.1` (native kernels via `awkward-cpp` `53`)
- module: `awkward` (alias `ak`); submodules `ak.contents`, `ak.forms`, `ak.types`, `ak.behaviors`, `ak.numba`, `ak.jax`
- asset: pure-Python `awkward` (`py3-none-any`) over the `awkward-cpp` native kernel extension; CUDA kernels load lazily through the `"cuda"` backend
- license: `BSD-3-Clause`
- marker: `requires-python >=3.10`; runtime dependencies `awkward-cpp`, `numpy`, `fsspec` (remote `from_parquet`/`from_json` paths), `packaging`; free-threaded (`3.13t`/`3.14t`) safe
- owner: `data`
- rail: irregular-arrays

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core array types
- rail: irregular-arrays

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     | [ROLE]                                           |
| :-----: | :---------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `ak.Array`        | high-level array  | variable-length nested array with behavior mixin |
|  [02]   | `ak.Record`       | single record     | one row from a record-typed array                |
|  [03]   | `ak.ArrayBuilder` | incremental build | append-based construction of irregular arrays    |

[PUBLIC_TYPE_SCOPE]: `ak.ArrayBuilder` members
- rail: irregular-arrays

| [INDEX] | [MEMBER]                                                              | [KIND]          | [ROLE]                                    |
| :-----: | :-------------------------------------------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `snapshot()`                                                          | method          | materialize appended layout to `ak.Array` |
|  [02]   | `append(obj)` / `extend(iterable)`                                    | method          | append one value or extend from iterable  |
|  [03]   | `integer(x)` / `real(x)` / `complex(x)` / `boolean(x)`                | method          | append a typed scalar                     |
|  [04]   | `string(s)` / `bytestring(b)`                                         | method          | append a string or bytestring             |
|  [05]   | `null()`                                                              | method          | append an option-type `None`              |
|  [06]   | `begin_list()` / `end_list()` / `list()`                              | method          | open, close, or context a list level      |
|  [07]   | `begin_record(name)` / `field(key)` / `end_record()` / `record(name)` | method          | open, key, close, or context a record     |
|  [08]   | `begin_tuple(n)` / `index(i)` / `end_tuple()` / `tuple(n)`            | method          | open, slot, close, or context a tuple     |
|  [09]   | `datetime(x)` / `timedelta(x)`                                        | method          | append a temporal scalar                  |
|  [10]   | `type` / `typestr` / `to_list()` / `tolist()` / `to_numpy()`          | property/method | live build type and Python/NumPy export   |
|  [11]   | `behavior` / `attrs` / `numba_type` / `show(...)`                     | property/method | behavior mixin, metadata, Numba JIT type, preview |

[PUBLIC_TYPE_SCOPE]: `ak.Array` members
- rail: irregular-arrays

| [INDEX] | [MEMBER]                              | [KIND]          | [ROLE]                                                       |
| :-----: | :------------------------------------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `layout`                              | property        | underlying `ak.contents` layout                              |
|  [02]   | `fields` / `is_tuple`                 | property        | record field names; whether the top record is unnamed        |
|  [03]   | `type` / `typestr`                    | property        | high-level type descriptor and its string form               |
|  [04]   | `ndim` / `nbytes`                     | property        | nesting depth and byte footprint                             |
|  [05]   | `mask`                                | property        | option-aware boolean masking accessor (`array.mask[cond]`)   |
|  [06]   | `behavior` / `attrs`                  | property        | behavior-mixin mapping and top-level metadata dict           |
|  [07]   | `named_axis` / `positional_axis`      | property        | named-axis mapping and positional axis tuple                 |
|  [08]   | `to_list()` / `tolist()`              | method          | convert to nested Python lists                               |
|  [09]   | `to_numpy(allow_missing)`             | method          | convert to a NumPy array                                     |
|  [10]   | `show(limit_rows, type, ...)`         | method          | formatted preview of contents                                |
|  [11]   | `numba_type` / `cpp_type`             | property        | Numba and cppyy type handles for JIT/C++ kernels             |
|  [12]   | `__getitem__` / `__setitem__` / `__getattr__` | dunder  | jagged slicing, field assignment, dotted field access        |
|  [13]   | `__array_ufunc__` / `__array_function__`      | dunder  | NumPy ufunc and array-function dispatch over the layout      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction (`ak`)
- rail: irregular-arrays
- family: ingest, returns `ak.Array`

| [INDEX] | [SURFACE]                                                                                        | [SOURCE_FORMAT] |
| :-----: | :----------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `from_iter(iterable, *, allow_record, highlevel, behavior, attrs, initial, resize)`              | Python iterable |
|  [02]   | `from_numpy(array, *, regulararray, recordarray, highlevel, behavior, primitive_policy, attrs)`  | NumPy array     |
|  [03]   | `from_arrow(array, *, generate_bitmasks, highlevel, behavior, attrs)`                            | Arrow array     |
|  [04]   | `from_json(source, *, line_delimited, schema, nan_string, buffersize, highlevel, ...)`           | JSON            |
|  [05]   | `from_parquet(path, *, columns, row_groups, storage_options, generate_bitmasks, highlevel, ...)` | Parquet file    |
|  [06]   | `from_buffers(form, length, container, buffer_key, *, backend, byteorder, highlevel, ...)`       | buffer protocol |
|  [07]   | `from_rdataframe(rdf, columns, *, keep_order, offsets_type, with_name, highlevel, ...)`          | ROOT RDataFrame |

[ENTRYPOINT_SCOPE]: export (`ak`)
- rail: irregular-arrays

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `to_numpy(array, *, allow_missing)`                                                    | export         | awkward to NumPy array                   |
|  [02]   | `to_list(array)`                                                                       | export         | awkward to Python list                   |
|  [03]   | `to_arrow(array, *, list_to32, extensionarray, count_nulls, ...)`                      | export         | awkward to Arrow array                   |
|  [04]   | `to_parquet(array, destination, *, compression, row_group_size, parquet_version, ...)` | export         | awkward to Parquet file                  |
|  [05]   | `to_json(array, file, *, line_delimited, nan_string, num_indent_spaces, ...)`          | export         | awkward to JSON                          |
|  [06]   | `to_dataframe(array, *, how, levelname, anonymous)`                                    | export         | awkward to pandas frame                  |
|  [07]   | `to_backend(array, backend, *, highlevel, behavior, attrs)`                            | backend        | move array to named backend              |
|  [08]   | `to_layout(array, *, allow_record, none_policy, primitive_policy, ...)`                | layout         | descend to `ak.contents`                 |
|  [09]   | `to_buffers(array, container, buffer_key, form_key, *, id_start, backend, byteorder)`  | layout         | decompose to `(form, length, container)` |
|  [10]   | `to_packed(array, *, highlevel, behavior, attrs)`                                      | layout         | compact contiguous layout copy           |

[ENTRYPOINT_SCOPE]: transform and structure (`ak`)
- rail: irregular-arrays

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                                                                                                                                                                                                                                     | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :---------------------------------------------------- |
|  [01]   | `concatenate(arrays, axis, *, mergebool, highlevel, ...)`                                                                                                                                                                                                                                                                                                                                                                                                     | combine        | concatenate along axis                                |
|  [02]   | `zip(arrays, depth_limit, *, with_name, optiontype_outside_record, ...)` / `unzip(array, *, how)`                                                                                                                                                                                                                                                                                                                                                             | combine        | zip or split records                                  |
|  [03]   | `flatten(array, axis, ...)` / `unflatten(array, counts, axis, ...)`                                                                                                                                                                                                                                                                                                                                                                                           | reshape        | remove or add a nesting level                         |
|  [04]   | `where(condition, *args, mergebool, ...)`                                                                                                                                                                                                                                                                                                                                                                                                                     | filter         | conditional element select                            |
|  [05]   | `sort(array, axis, *, ascending, stable, ...)` / `argsort(array, axis, ...)`                                                                                                                                                                                                                                                                                                                                                                                  | order          | sort along axis or get order                          |
|  [06]   | `pad_none(array, target, axis, *, clip, ...)` / `fill_none(array, value, axis, ...)`                                                                                                                                                                                                                                                                                                                                                                          | pad            | pad to length or fill option None                     |
|  [07]   | `drop_none(array, axis, ...)` / `is_none(array, axis, ...)` / `mask(array, mask, *, valid_when)`                                                                                                                                                                                                                                                                                                                                                              | option         | drop, test, or apply option mask                      |
|  [08]   | `with_field(array, what, where, ...)` / `without_field(array, where, ...)`                                                                                                                                                                                                                                                                                                                                                                                    | annotation     | add or remove a record field                          |
|  [09]   | `with_name(array, name, ...)`                                                                                                                                                                                                                                                                                                                                                                                                                                 | annotation     | attach record type name                               |
|  [10]   | `cartesian(arrays, axis, *, nested, with_name, ...)` / `combinations(array, n, *, replacement, axis, ...)`                                                                                                                                                                                                                                                                                                                                                    | combine        | per-list pairings or n-tuples                         |
|  [11]   | `num(array, axis, ...)` / `count(array, axis, ...)` / `sum(array, axis, *, keepdims, mask_identity, ...)` / `mean(x, weight, axis, ...)` / `prod` / `min(array, axis, *, initial, ...)` / `max(array, axis, *, initial, ...)` / `any` / `all` / `count_nonzero` / `std(x, weight, ddof, axis, ...)` / `var(x, weight, ddof, axis, ...)` / `ptp` / `corr(x, y, weight, axis, ...)` / `linear_fit(x, y, weight, axis, ...)` / `moment(x, n, weight, axis, ...)` | reduce         | lengths and axis reductions                           |
|  [12]   | `fields(array)` / `type(array, *, behavior)` / `backend(*arrays)`                                                                                                                                                                                                                                                                                                                                                                                             | metadata       | field names, type, backend                            |
|  [13]   | `firsts(array, axis, ...)` / `singletons(array, axis, ...)` / `local_index(array, axis, ...)` / `run_lengths(array, ...)`                                                                                                                                                                                                                                                                                                                                     | structure      | per-list first/singleton/index/runs                   |
|  [14]   | `values_astype(array, to, *, including_unknown, ...)` / `enforce_type(array, type, ...)` / `ravel(array, ...)` / `copy(array)`                                                                                                                                                                                                                                                                                                                                | transform      | value-cast, type-enforce, flatten, copy               |
|  [15]   | `with_parameter(array, parameter, value, ...)` / `validity_error(array, *, exception)` / `parameters(array)` / `metadata_from_parquet(path, *, storage_options, row_groups, ignore_metadata, scan_files)`                                                                                                                                                                                                                                                     | inspect        | parameter set, validity, parameters, parquet metadata |

## [04]-[IMPLEMENTATION_LAW]

[IRREGULAR_TOPOLOGY]:
- namespace: `awkward` (`ak`), `ak.contents` (typed layouts), `ak.forms` (form algebra), `ak.types` (type descriptors), `ak.behaviors` (behavior mixins)
- `ak.Array` is a high-level wrapper over a typed layout in `ak.contents`; `to_layout` descends to the layout and `from_buffers` rebuilds an array from a form plus buffers with no data copy
- backend dispatch: `backend(*arrays)` returns the current backend name; `to_backend` moves data across `"cpu"`, `"cuda"`, `"jax"` while preserving the layout structure
- option type: `None` entries are `OptionType` layouts; `fill_none`, `drop_none`, `is_none`, and `mask` operate on them, and `pad_none` produces them
- behaviors: custom methods and overloads attach via the `behavior` mapping keyed on record type name; `with_name` tags arrays so behaviors resolve
- forms: `ak.forms` carries the form algebra for constructing typed arrays from buffers without materializing data, the basis of zero-copy interchange

[INTEGRATION_RAILS]:
- awkward <-> arro3/pyarrow: `ak.to_arrow(array, extensionarray=...)` emits a `pyarrow` array carrying awkward's type as Arrow extension metadata; that array's `__arrow_c_array__` feeds `arro3.core.Array.from_arrow` zero-copy, and `ak.from_arrow` re-imports any PyCapsule producer. `ak.to_arrow_table`/`ak.from_arrow_table` round-trip whole record arrays as Arrow tables.
- awkward <-> numpy: `ak.Array.__array_ufunc__` makes NumPy ufuncs operate over the jagged layout in place (`np.sqrt(events.pt)`), so element math never flattens to Python; `ak.to_numpy`/`from_numpy` is the terminal regular-array escape only.
- awkward <-> jax: `to_backend(array, "jax")` moves the layout onto a JAX-differentiable backend (registered via `ak.jax.register_and_check()`), so reductions and ufuncs become traceable for autodiff without leaving the awkward layout.
- awkward <-> numba: `ak.Array.numba_type` and the `ak.numba` registration let `@numba.njit` kernels iterate the typed layout directly; reach for this over Python loops when a reduction has no axis-vectorized form.
- awkward <-> pandas: `ak.to_dataframe(array, how=...)` explodes nested fields into a MultiIndex frame for tabular consumers; this is a terminal projection, not a storage form.

[LOCAL_ADMISSION]:
- Irregular event-data payloads enter as `ak.Array` with named fields and an explicit backend; the field names and type descriptor join the array-admission record.
- Field access uses `fields` and named indexing, never positional-only access, so the record structure stays self-describing.
- Arrow and Parquet interchange uses `from_arrow`/`to_arrow` and `from_parquet`/`to_parquet`; cloud paths pass `storage_options` (resolved through `fsspec`).
- Backend moves go through `to_backend`; do not re-materialize through NumPy when awkward owns the irregular layout. Element math rides `__array_ufunc__`, not a Python loop.
- Custom domain methods attach through the `behavior` mapping keyed on record name and resolved by `with_name`; never subclass `ak.Array`.

[RAIL_LAW]:
- Package: `awkward`
- Owns: variable-length nested arrays, option types, record and union arrays, behavior mixins, and multi-backend (`cpu`/`cuda`/`jax`) columnar irregular data
- Accept: irregular event-data payloads wrapped in `ak.Array` with named fields and an explicit backend
- Reject: hand-rolled ragged list structures, positional-only field access where `fields` applies, NumPy arrays for irregular data awkward owns, Python loops where `__array_ufunc__` or a Numba/JAX backend kernel applies, and `ak.Array` subclassing where the `behavior` mapping applies
