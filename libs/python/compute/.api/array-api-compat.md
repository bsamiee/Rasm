# [PY_COMPUTE_API_ARRAY_API_COMPAT]

`array-api-compat` resolves one backend-agnostic `xp` namespace across every admitted array backend, narrows array and namespace type per backend, and shims each backend to Array API conformance so a compute owner writes against `xp` alone.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `array-api-compat`
- package: `array-api-compat` (MIT)
- module: `array_api_compat`
- owner: `compute`
- rail: array-api

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: namespace resolver and backend/execution-model guards

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]    | [CAPABILITY]                                 |
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
- module: `array_api_compat.numpy`
- type-family: named tuple

| [INDEX] | [SYMBOL]              | [CAPABILITY]                                     |
| :-----: | :-------------------- | :----------------------------------------------- |
|  [01]   | `UniqueAllResult`     | `values`, `indices`, `inverse_indices`, `counts` |
|  [02]   | `UniqueCountsResult`  | `values`, `counts`                               |
|  [03]   | `UniqueInverseResult` | `values`, `inverse_indices`                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: namespace resolution and device transfer

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------------------- | :-------------------------- |
|  [01]   | `array_namespace(*xs, api_version=None, use_compat=None) -> Namespace` | primary namespace resolver  |
|  [02]   | `get_namespace(*xs, api_version=None, use_compat=None) -> Namespace`   | alias of `array_namespace`  |
|  [03]   | `device(x, /) -> Device`                                               | extract device from array   |
|  [04]   | `to_device(x, device, /, *, stream=None) -> Array`                     | move array to device        |
|  [05]   | `size(x) -> int \| None`                                               | total element count or None |

[ENTRYPOINT_SCOPE]: backend and execution-model predicates

| [INDEX] | [SURFACE]                                          | [CAPABILITY]                      |
| :-----: | :------------------------------------------------- | :-------------------------------- |
|  [01]   | `is_array_api_obj(x) -> TypeGuard[_ArrayApiObj]`   | any-backend array-object guard    |
|  [02]   | `is_numpy_array(x) -> TypeIs[NDArray[Any]]`        | numpy array narrowing             |
|  [03]   | `is_cupy_array(x) -> bool`                         | cupy array detection              |
|  [04]   | `is_torch_array(x) -> TypeIs[Tensor]`              | torch array narrowing             |
|  [05]   | `is_jax_array(x) -> TypeIs[jax.Array]`             | jax array narrowing               |
|  [06]   | `is_dask_array(x) -> TypeIs[da.Array]`             | dask array narrowing              |
|  [07]   | `is_ndonnx_array(x) -> TypeIs[ndx.Array]`          | ndonnx array narrowing            |
|  [08]   | `is_pydata_sparse_array(x) -> TypeIs[SparseArray]` | pydata-sparse array narrowing     |
|  [09]   | `is_lazy_array(x) -> TypeGuard[_ArrayApiObj]`      | graph-traced (JAX/Dask) narrowing |
|  [10]   | `is_writeable_array(x) -> TypeGuard[_ArrayApiObj]` | eager-mutable buffer narrowing    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `array_api_compat` is the namespace root; `.numpy`/`.cupy`/`.torch`/`.dask` wrappers re-export each backend patched to Array API conformance (`xp.unique_all`, positional-only argument shapes, spec gaps backfilled).
- `common` submodule owns every guard and resolver; the top-level names re-export it.
- Vendor predicates `is_<backend>_array`/`is_<backend>_namespace` narrow by library with `TypeIs`; `is_lazy_array`/`is_writeable_array` narrow by execution model — graph-traced versus eager-mutable — the fork every backend-agnostic branch reads.

[STACKING]:
- `array-api-extra`(`.api/array-api-extra.md`): `xp = array_namespace(*arrays)` resolves once, then `array_api_extra.<op>(..., xp=xp)` threads it into every standard and extension call — one rail, no re-resolution, no vendor import.
- `is_writeable_array(x)` gates `array_api_extra.at(x, idx).set(v, copy=False)`: a writeable NumPy buffer mutates, a read-only JAX array copies.
- `is_lazy_array(x)` routes JAX-traced and Dask arrays to `array_api_extra.lazy_apply` and eager arrays to the direct `xp` op — the same lazy/eager fork the `diffrax`/`equinox`/`optimistix` JAX rails consume.
- `device(x)`/`to_device(x, dev, stream=)` carry the device context the JAX and Torch sibling rails thread alongside `xp`, resolved once at boundary.

[LOCAL_ADMISSION]:
- Compute owners resolve `xp = array_namespace(*arrays)` once at operation entry and route every array op through `xp.<op>`.
- `use_compat=None` injects the compat wrapper only when the raw backend lags the spec — `False` forces the bare namespace, `True` forces the wrapper — and `api_version` pins the spec revision a kernel targets.

[RAIL_LAW]:
- Package: `array-api-compat`
- Owns: Array API namespace resolution, spec-conformance wrappers, and backend/execution-model predicate guards
- Accept: `xp = array_namespace(*arrays)` at boundary scope, `xp` threaded into every standard and `array-api-extra` op, `is_writeable_array`/`is_lazy_array` gating the mutate/copy and eager/lazy forks
- Reject: a hardcoded backend import in a generic kernel, a locally re-implemented namespace detector or unique-result tuple, `xp` re-resolved inside an inner loop
