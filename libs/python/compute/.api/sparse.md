# [PY_COMPUTE_API_SPARSE]

`sparse` supplies `COO`, `GCXS`, and `DOK` n-dimensional sparse array classes under a shared `SparseArray` base implementing the Python Array API, plus creation, conversion, linear-algebra, element-wise, shape, reduction, math, and `.npz` IO over multi-dimensional sparse tensors. Format is discriminated by the `format=` keyword or `asformat`, and `fill_value` sets the implicit dense value preserved through every operation. The library STACKS into the compute array rail as the n-D sparse complement to `scipy.sparse` (which owns only 2-D CSR/CSC/COO and sparse LA): `from_scipy_sparse`/`to_scipy_sparse` bridge a `GCXS`/`COO` to a `scipy.sparse` matrix for `scipy.sparse.linalg` Krylov solves, while `sparse` itself owns the >2-D coordinate algebra (`tensordot`, `einsum`, broadcasting) that `scipy.sparse` cannot express. Because it conforms to `array-api-compat`, the same `array-api-extra` and dispatch code that folds a dense `numpy`/`jax` array folds a `sparse` array unchanged. It never materialises a dense intermediate the library can keep sparse.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sparse`
- package: `sparse` (PyData Sparse; distinct from the C# `CSparse` NuGet — `assay api resolve sparse` name-collides on `CSparse` and is not the resolver path here)
- version: `0.18.0`
- license: BSD-3-Clause
- import: `import sparse`
- owner: `compute`
- rail: array
- asset: pure Python with an optional `numba` JIT backend (`sparse.numba_backend`); the JIT path requires `numba`/`llvmlite`
- capability: COO/GCXS/DOK sparse ndarray classes, Array-API math/reduce/linalg operations over arbitrary rank, inter-format and SciPy/NumPy conversion, `.npz` IO, and a `numba_backend` dispatch surface mirroring the same operations

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse array classes
- rail: array
- format is data, not a type axis — `asformat('coo'|'gcxs'|'dok')` discriminates one polymorphic value rather than branching per class

| [INDEX] | [SYMBOL]             | [FORMAT]             | [CONSTRUCTOR]                                                                            |
| :-----: | :------------------- | :------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `sparse.SparseArray` | abstract base        | `SparseArray(shape, fill_value=None)`                                                    |
|  [02]   | `sparse.COO`         | coordinate           | `COO(coords, data=None, shape=None, has_duplicates=True, sorted=False, fill_value=None)` |
|  [03]   | `sparse.GCXS`        | compressed (CSR/CSC) | `GCXS(arg, shape=None, compressed_axes=None, fill_value=None)`                           |
|  [04]   | `sparse.DOK`         | dictionary of keys   | `DOK(shape, data=None, dtype=None, fill_value=None)`                                     |

[PUBLIC_TYPE_SCOPE]: shared array members
- rail: array

| [INDEX] | [MEMBER_FAMILY]    | [MEMBERS]                                                                                                          |
| :-----: | :----------------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | properties         | `nnz`, `ndim`, `size`, `dtype`, `density`, `fill_value`, `shape`, `nbytes`, `T`, `mT`, `real`, `imag`, `device`   |
|  [02]   | densify/convert    | `todense`, `maybe_densify(max_size, min_density)`, `asformat`, `astype`, `to_device`, `from_numpy`, `from_scipy_sparse` |
|  [03]   | reduction          | `reduce(method, axis, keepdims, **kwargs)`, `sum`, `prod`, `mean`, `std`, `var`, `max`, `min`, `amax`, `amin`, `all`, `any` |
|  [04]   | transform          | `reshape`, `transpose`, `swapaxes`, `flatten`, `squeeze`, `clip`, `conj`, `round`/`round_`, `isnan`, `isinf`, `nonzero` |
|  [05]   | COO-only           | `from_iter`, `linear_loc`, `tocsr`, `tocsc`, `to_scipy_sparse`, `enable_caching`, `copy`                          |
|  [06]   | GCXS-only          | `from_iter`, `from_coo`, `change_compressed_axes`, `compressed_axes`, `tocoo`, `todok`, `to_scipy_sparse`, `copy` |
|  [07]   | DOK-only           | `from_coo`, `to_coo` (DOK carries no `to_scipy_sparse`; round-trip DOK->COO->scipy)                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and conversion
- rail: array
- creation functions accept `format='coo'/'gcxs'/'dok'`; `device` is an Array-API hook only (no GPU backend in the base namespace)

| [INDEX] | [SURFACE]                                                                             | [FAMILY]   | [PRODUCES]            |
| :-----: | :------------------------------------------------------------------------------------ | :--------- | :-------------------- |
|  [01]   | `zeros` / `ones` / `empty` / `full(shape, fill_value, ..., format='coo')`             | creation   | constant sparse array |
|  [02]   | `zeros_like` / `ones_like` / `empty_like` / `full_like(a, ...)`                       | creation   | like-shaped sparse array |
|  [03]   | `eye(N, M=None, k=0, dtype=float, format='coo')`                                      | creation   | identity              |
|  [04]   | `random(shape, density=None, nnz=None, format='coo', fill_value=None)`                | creation   | random sparse array   |
|  [05]   | `asarray(obj, /, *, dtype=None, format=None, copy=False)`                             | conversion | sparse array          |
|  [06]   | `asCOO(x, name='asCOO', check=True)` \| `as_coo(x, shape=None, fill_value=None)`      | conversion | `COO`                 |
|  [07]   | `asnumpy(a, dtype=None, order=None)`                                                  | conversion | dense `ndarray`       |
|  [08]   | `save_npz(filename, matrix, compressed=True)` \| `load_npz(filename)`                 | IO         | `.npz` round-trip     |

[ENTRYPOINT_SCOPE]: linear algebra and shape
- rail: array

| [INDEX] | [SURFACE]                                                                                           | [FAMILY]    |
| :-----: | :-------------------------------------------------------------------------------------------------- | :---------- |
|  [01]   | `dot(a, b)`, `matmul(a, b)`, `vecdot(x1, x2, /, *, axis=-1)`                                        | matrix mul  |
|  [02]   | `tensordot(a, b, axes=2, *, return_type=None)`, `einsum(*operands)`                                 | contraction |
|  [03]   | `kron(a, b)`, `outer(a, b, out=None)`                                                               | products    |
|  [04]   | `stack(arrays, axis=0)`, `concat(arrays, axis=0)`, `concatenate(arrays, axis=0)`                    | join        |
|  [05]   | `broadcast_to(x, /, shape)`, `broadcast_arrays(*arrays)`, `expand_dims(x, /, *, axis=0)`            | broadcast   |
|  [06]   | `reshape(x, /, shape)`, `squeeze(x, /, axis=None)`, `moveaxis(a, source, destination)`              | reshape     |
|  [07]   | `permute_dims(x, /, axes=None)`, `matrix_transpose(x, /)`, `flip(x, /, *, axis=None)`               | axes        |
|  [08]   | `roll(a, shift, axis=None)`, `pad(array, pad_width, mode='constant')`, `unstack(x, axis=0)`         | rearrange   |
|  [09]   | `tile(a, reps)`, `repeat(a, repeats, axis=None)`, `take(x, indices, /, *, axis=None)`               | replicate   |
|  [10]   | `diagonal(a, offset=0, axis1=0, axis2=1)`, `diagonalize(a, axis=0)`, `triu(x, k=0)`, `tril(x, k=0)`, `argwhere(a)` | diagonal/locate |

[ENTRYPOINT_SCOPE]: element-wise, reduction, and dtype
- rail: array
- math, comparison, and bitwise functions follow the NumPy ufunc signature with broadcasting

| [INDEX] | [FAMILY]      | [MEMBERS]                                                                                                                        |
| :-----: | :------------ | :------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | dispatch      | `elemwise(func, *args, **kwargs)`, `where(condition, x=None, y=None)`                                                            |
|  [02]   | binary ufunc  | `add`, `subtract`, `multiply`, `divide`, `floor_divide`, `remainder`, `pow`, `maximum`, `minimum`, `logaddexp`, `nextafter`, `copysign`, `hypot`, `signbit` |
|  [03]   | reduction     | `sum`, `prod`, `mean`, `std`, `var`, `max`, `min`, `all`, `any`, `argmax`, `argmin`, `nonzero`                                   |
|  [04]   | nan reduction | `nansum`, `nanprod`, `nanmean`, `nanmax`, `nanmin`, `nanreduce(arr, method, identity, axis, keepdims)`                           |
|  [05]   | math          | `abs`, `sqrt`, `square`, `exp`, `log`, `log2`, `log10`, `log1p`, `expm1`, `sign`, `reciprocal`, `negative`, `positive`           |
|  [06]   | trig          | `sin`, `cos`, `tan`, `sinh`, `cosh`, `tanh`, `asin`, `acos`, `atan`, `asinh`, `acosh`, `atanh`, `atan2`                          |
|  [07]   | rounding      | `ceil`, `floor`, `round`, `trunc`                                                                                                |
|  [08]   | comparison    | `equal`, `not_equal`, `less`, `less_equal`, `greater`, `greater_equal`                                                           |
|  [09]   | logic         | `logical_and`, `logical_or`, `logical_xor`, `logical_not`, `isfinite`, `isinf`, `isnan`, `isposinf`, `isneginf`                  |
|  [10]   | bitwise       | `bitwise_and`, `bitwise_or`, `bitwise_xor`, `bitwise_invert`, `bitwise_not`, `bitwise_left_shift`, `bitwise_right_shift`         |
|  [11]   | sort/search   | `sort(x, /, *, axis=-1, descending=False)`, `unique_values(x)`, `unique_counts(x)`, `diff(x, axis=-1, n=1)`, `interp(x, xp, fp)` |
|  [12]   | dtype         | `result_type(*arrays_and_dtypes)`, `can_cast(from_, to)`, `isdtype(dtype, kind)`, `astype(x, dtype)`, `finfo`, `iinfo`           |
|  [13]   | numba backend | `sparse.numba_backend` re-exports the same array classes and operations under Numba JIT dispatch                                 |

## [04]-[IMPLEMENTATION_LAW]

[FORMAT_TOPOLOGY]:
- Format is chosen by `format='coo'/'gcxs'/'dok'` on creation or `asformat`; one `asformat` discriminates instead of per-class branching. Inter-format moves are direct methods: COO `tocsr`/`tocsc`, GCXS `tocoo`/`todok`, DOK `to_coo` — no dense round-trip.
- `fill_value` is the implicit dense value; operations preserve sparsity by never materialising fill-value positions. A non-zero `fill_value` changes which entries are "structural" and is carried through every transform.
- `reduce(method, axis, keepdims, **kwargs)` is the generalised reduction; `nanreduce(arr, method, identity, ...)` is its NaN-skipping form. `elemwise(func, *args)` is the generalised element-wise operator, preferred over manual coordinate iteration for a custom scalar function.
- `maybe_densify(max_size, min_density)` is the guarded densification gate — it raises rather than blow memory when the array exceeds the size/density bound, the correct boundary for handing a result to a dense consumer.
- `device`/`to_device` are Array-API compatibility hooks; they do not map to a GPU backend in the base `sparse` namespace.

[INTEGRATION_STACK]:
- scipy.sparse: `from_scipy_sparse` / `COO.from_scipy_sparse` lift a 2-D `scipy.sparse` matrix to a sparse array; `GCXS.to_scipy_sparse`/`COO.to_scipy_sparse` hand a 2-D result back for `scipy.sparse.linalg` (`spsolve`, `cg`, `gmres`, `eigsh`). `sparse` owns the >2-D coordinate algebra (`tensordot`/`einsum`/broadcast) that `scipy.sparse` cannot express; the 2-D LA hand-off rides the SciPy matrix.
- array-api-compat / array-api-extra: `sparse` arrays satisfy the Array-API namespace, so dispatch code written against `array_api_compat.array_namespace(x)` folds a `sparse` array beside `numpy`/`jax`/`dask` with no format-specific branch.
- numba_backend: the `sparse.numba_backend` re-export runs the same operation set under Numba JIT for a hot CPU-bound loop; it is opt-in (requires `numba`/`llvmlite`) and shares the COO/GCXS/DOK class surface.

[LOCAL_ADMISSION]:
- entry: an admitted sparse payload enters via `asarray(..., format=...)` or `from_scipy_sparse`; format is `asformat`-discriminated, never a parallel per-format entrypoint.
- evidence: a sparse-array fold captures the format, `nnz`/`density`, and `fill_value` as the array claim; densification is gated through `maybe_densify` so a memory blow is a typed boundary, not an OOM.
- boundary: sparse results stay sparse through the pipeline; dense materialisation (`todense`/`asnumpy`) happens only at a declared dense-consumer edge.

[RAIL_LAW]:
- Package: `sparse`
- Owns: COO/GCXS/DOK n-dimensional sparse ndarray, Array-API math/reduce/linalg over arbitrary rank, inter-format conversion, SciPy/NumPy bridges, `.npz` IO, and an optional Numba JIT backend
- Accept: an n-D sparse payload with `asformat` discrimination, a `fill_value` for the implicit dense value, and `elemwise`/`reduce` for custom operations
- Reject: per-format parallel entrypoints, manual coordinate iteration for operations the library implements, dense intermediate materialisation inside a sparse pipeline, and re-implementing 2-D sparse LA that `scipy.sparse.linalg` owns
