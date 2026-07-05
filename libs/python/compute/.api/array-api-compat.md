# [PY_COMPUTE_API_ARRAY_API_COMPAT]

`array-api-compat` supplies backend-agnostic namespace resolution, array-type guards, and Array API conformance shims for NumPy, CuPy, PyTorch, JAX, Dask, ndonnx, and pydata-sparse so compute owners operate against a single `xp` namespace without hard-coding a backend.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `array-api-compat`
- package: `array-api-compat`
- version: `1.15.0`
- license: MIT
- module: `array_api_compat`
- owner: `compute`
- asset: runtime library
- rail: array-api

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: namespace and array guards
- rail: array-api

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]    | [ROLE]                                       |
| :-----: | :------------------------------------------------------ | :--------------- | :------------------------------------------- |
|  [01]   | `array_namespace`                                       | function         | resolve the Array API namespace for xs       |
|  [02]   | `get_namespace`                                         | function (alias) | alias for `array_namespace`                  |
|  [03]   | `is_array_api_obj`                                      | type guard       | `TypeGuard[_ArrayApiObj]`                    |
|  [04]   | `is_array_api_strict_namespace`                         | predicate        | checks strict conformance                    |
|  [05]   | `is_numpy_array` / `is_numpy_namespace`                 | predicates       | numpy backend detection                      |
|  [06]   | `is_cupy_array` / `is_cupy_namespace`                   | predicates       | cupy backend detection                       |
|  [07]   | `is_torch_array` / `is_torch_namespace`                 | predicates       | torch backend detection                      |
|  [08]   | `is_jax_array` / `is_jax_namespace`                     | predicates       | jax backend detection                        |
|  [09]   | `is_dask_array` / `is_dask_namespace`                   | predicates       | dask backend detection                       |
|  [10]   | `is_ndonnx_array` / `is_ndonnx_namespace`               | predicates       | ndonnx backend detection                     |
|  [11]   | `is_pydata_sparse_array` / `is_pydata_sparse_namespace` | predicates       | sparse backend detection                     |
|  [12]   | `is_lazy_array`                                         | type guard       | `TypeGuard[_ArrayApiObj]` for lazy arrays    |
|  [13]   | `is_writeable_array`                                    | type guard       | `TypeGuard[_ArrayApiObj]` for mutable arrays |

[PUBLIC_TYPE_SCOPE]: conformance result types
- rail: array-api
- module: `array_api_compat.numpy`
- type-family: named tuple

| [INDEX] | [SYMBOL]              | [FIELDS]                                         |
| :-----: | :-------------------- | :----------------------------------------------- |
|  [01]   | `UniqueAllResult`     | `values`, `indices`, `inverse_indices`, `counts` |
|  [02]   | `UniqueCountsResult`  | `values`, `counts`                               |
|  [03]   | `UniqueInverseResult` | `values`, `inverse_indices`                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: namespace resolution
- rail: array-api

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `array_namespace(*xs, api_version=None, use_compat=None) -> Namespace` | resolver       | primary namespace entry point |
|  [02]   | `get_namespace(*xs, api_version=None, use_compat=None) -> Namespace`   | resolver alias | same as `array_namespace`     |
|  [03]   | `device(x, /) -> Device`                                               | accessor       | extract device from array     |
|  [04]   | `to_device(x, device, /, *, stream=None) -> Array`                     | transfer       | move array to device          |
|  [05]   | `size(x) -> int \| None`                                               | accessor       | total element count or None   |

[ENTRYPOINT_SCOPE]: backend predicates
- rail: array-api

| [INDEX] | [SURFACE]                             | [RETURNS]                 | [BACKEND]      |
| :-----: | :------------------------------------ | :------------------------ | :------------- |
|  [01]   | `is_array_api_obj(x) -> TypeGuard`    | `TypeGuard[_ArrayApiObj]` | any backend    |
|  [02]   | `is_numpy_array(x) -> TypeIs`         | `TypeIs[NDArray[Any]]`    | numpy          |
|  [03]   | `is_cupy_array(x) -> bool`            | `bool`                    | cupy           |
|  [04]   | `is_torch_array(x) -> TypeIs`         | `TypeIs[Tensor]`          | torch          |
|  [05]   | `is_jax_array(x) -> TypeIs`           | `TypeIs[jax.Array]`       | jax            |
|  [06]   | `is_dask_array(x) -> TypeIs`          | `TypeIs[da.Array]`        | dask           |
|  [07]   | `is_ndonnx_array(x) -> TypeIs`        | `TypeIs[ndx.Array]`       | ndonnx         |
|  [08]   | `is_pydata_sparse_array(x) -> TypeIs` | `TypeIs[SparseArray]`     | pydata-sparse  |
|  [09]   | `is_lazy_array(x) -> TypeGuard`       | `TypeGuard[_ArrayApiObj]` | lazy backends  |
|  [10]   | `is_writeable_array(x) -> TypeGuard`  | `TypeGuard[_ArrayApiObj]` | mutable arrays |

## [04]-[IMPLEMENTATION_LAW]

[ARRAY_API_TOPOLOGY]:
- namespace root: `array_api_compat`; backend wrappers at `array_api_compat.numpy`, `.cupy`, `.torch`, `.dask` re-export the backend namespace patched to Array API conformance (`xp.unique_all`, positional-only argument shapes, 2024.12 spec gaps backfilled).
- `common` submodule owns every guard and resolver; the top-level names are the public re-export.
- `UniqueAllResult`, `UniqueCountsResult`, `UniqueInverseResult` are NamedTuples on `array_api_compat.numpy` (fields `values`/`indices`/`inverse_indices`/`counts`, `values`/`counts`, `values`/`inverse_indices`); the spec-standard `xp.unique_all`/`unique_counts`/`unique_inverse` return them.
- Predicate guards split by intent: `is_<backend>_array`/`is_<backend>_namespace` narrow with `TypeIs`/`TypeGuard` for backend-specific fallbacks; `is_lazy_array` and `is_writeable_array` narrow on execution model (graph-traced vs eager-mutable) rather than vendor.

[STACKING]:
- `array-api-compat` is the resolver tier; `array-api-extra` is the extension tier built on top. The single dense rail is `xp = array_namespace(*arrays)` -> `array_api_extra.<op>(..., xp=xp)` so a compute owner resolves the namespace once and threads `xp` into every standard and extension call, never re-resolving and never importing a vendor namespace.
- `is_writeable_array(x)` gates the in-place vs copy branch of `array_api_extra.at(x, idx).set(v)`: a writeable NumPy buffer mutates, a read-only JAX array copies. The guard lets a kernel choose `copy=False` only when the buffer admits it.
- `is_lazy_array(x)` (true for JAX-traced / Dask) routes the deferred path to `array_api_extra.lazy_apply`; the eager path runs the standard `xp` op directly. This is the same lazy/eager fork the `equinox`/`jax` rails consume downstream.
- `device(x)` / `to_device(x, dev, stream=)` carry the device context the JAX and Torch sibling rails require; resolve device once at boundary, thread alongside `xp`.

[LOCAL_ADMISSION]:
- Compute owners call `xp = array_namespace(*arrays)` once at operation entry; all subsequent array ops use `xp.<op>`, never a hardcoded backend import.
- `use_compat=None` (default) injects the compat wrapper only when the raw backend lags the spec; pass `use_compat=False` for the bare backend namespace, `use_compat=True` to force the wrapper.
- `api_version` pins the spec revision a kernel targets when a backend exposes multiple; omit to accept the namespace default.

[RAIL_LAW]:
- Package: `array-api-compat`
- Owns: Array API namespace resolution, spec-conformance wrappers, and backend/execution-model predicate guards
- Accept: `xp = array_namespace(*arrays)` at boundary scope, `xp` threaded into every standard and `array-api-extra` op, `is_writeable_array`/`is_lazy_array` gating the mutate/copy and eager/lazy forks
- Reject: hardcoded `import numpy as np` in generic compute kernels; re-implementing namespace detection or unique-result tuples locally; re-resolving `xp` inside an inner loop instead of threading it
