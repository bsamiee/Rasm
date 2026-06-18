# [PY_COMPUTE_API_ARRAY_API_COMPAT]

`array-api-compat` supplies backend-agnostic namespace resolution, array-type guards, and Array API conformance shims for NumPy, CuPy, PyTorch, JAX, Dask, ndonnx, and pydata-sparse so compute owners operate against a single `xp` namespace without hard-coding a backend.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `array-api-compat`
- package: `array-api-compat`
- module: `array_api_compat`
- asset: runtime library
- rail: array-api

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: namespace and array guards
- rail: array-api

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]    | [ROLE]                                       |
| :-----: | :------------------------------------------------------ | :--------------- | :------------------------------------------- |
|   [1]   | `array_namespace`                                       | function         | resolve the Array API namespace for xs       |
|   [2]   | `get_namespace`                                         | function (alias) | alias for `array_namespace`                  |
|   [3]   | `is_array_api_obj`                                      | type guard       | `TypeGuard[_ArrayApiObj]`                    |
|   [4]   | `is_array_api_strict_namespace`                         | predicate        | checks strict conformance                    |
|   [5]   | `is_numpy_array` / `is_numpy_namespace`                 | predicates       | numpy backend detection                      |
|   [6]   | `is_cupy_array` / `is_cupy_namespace`                   | predicates       | cupy backend detection                       |
|   [7]   | `is_torch_array` / `is_torch_namespace`                 | predicates       | torch backend detection                      |
|   [8]   | `is_jax_array` / `is_jax_namespace`                     | predicates       | jax backend detection                        |
|   [9]   | `is_dask_array` / `is_dask_namespace`                   | predicates       | dask backend detection                       |
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
|   [1]   | `UniqueAllResult`     | `values`, `indices`, `inverse_indices`, `counts` |
|   [2]   | `UniqueCountsResult`  | `values`, `counts`                               |
|   [3]   | `UniqueInverseResult` | `values`, `inverse_indices`                      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: namespace resolution
- rail: array-api

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `array_namespace(*xs, api_version=None, use_compat=None) -> Namespace` | resolver       | primary namespace entry point |
|   [2]   | `get_namespace(*xs, api_version=None, use_compat=None) -> Namespace`   | resolver alias | same as `array_namespace`     |
|   [3]   | `device(x, /) -> Device`                                               | accessor       | extract device from array     |
|   [4]   | `to_device(x, device, /, *, stream=None) -> Array`                     | transfer       | move array to device          |
|   [5]   | `size(x) -> int \| None`                                               | accessor       | total element count or None   |

[ENTRYPOINT_SCOPE]: backend predicates
- rail: array-api

| [INDEX] | [SURFACE]                             | [RETURNS]                 | [BACKEND]      |
| :-----: | :------------------------------------ | :------------------------ | :------------- |
|   [1]   | `is_array_api_obj(x) -> TypeGuard`    | `TypeGuard[_ArrayApiObj]` | any backend    |
|   [2]   | `is_numpy_array(x) -> TypeIs`         | `TypeIs[NDArray[Any]]`    | numpy          |
|   [3]   | `is_cupy_array(x) -> bool`            | `bool`                    | cupy           |
|   [4]   | `is_torch_array(x) -> TypeIs`         | `TypeIs[Tensor]`          | torch          |
|   [5]   | `is_jax_array(x) -> TypeIs`           | `TypeIs[jax.Array]`       | jax            |
|   [6]   | `is_dask_array(x) -> TypeIs`          | `TypeIs[da.Array]`        | dask           |
|   [7]   | `is_ndonnx_array(x) -> TypeIs`        | `TypeIs[ndx.Array]`       | ndonnx         |
|   [8]   | `is_pydata_sparse_array(x) -> TypeIs` | `TypeIs[SparseArray]`     | pydata-sparse  |
|   [9]   | `is_lazy_array(x) -> TypeGuard`       | `TypeGuard[_ArrayApiObj]` | lazy backends  |
|  [10]   | `is_writeable_array(x) -> TypeGuard`  | `TypeGuard[_ArrayApiObj]` | mutable arrays |

## [4]-[IMPLEMENTATION_LAW]

[ARRAY_API_TOPOLOGY]:
- namespace root: `array_api_compat`; backend wrappers at `array_api_compat.numpy`, `.cupy`, `.torch`, `.dask`
- `common` submodule owns all guard and resolver implementations
- `UniqueAllResult`, `UniqueCountsResult`, `UniqueInverseResult` live in `array_api_compat.numpy` as named tuples

[LOCAL_ADMISSION]:
- Compute owners call `xp = array_namespace(*arrays)` once at operation entry; all subsequent array ops use `xp.<op>`, never a hardcoded backend import.
- Backend detection predicates route type-specific fallbacks; the common path stays backend-agnostic.
- `use_compat=None` (default) injects the compat wrapper only when needed; pass `use_compat=False` to get the raw backend namespace.

[RAIL_LAW]:
- Package: `array-api-compat`
- Owns: Array API namespace resolution and backend predicate guards
- Accept: `xp = array_namespace(*arrays)` at boundary scope; use `xp` for all array operations
- Reject: hardcoded `import numpy as np` in generic compute kernels; re-implementing namespace detection locally
