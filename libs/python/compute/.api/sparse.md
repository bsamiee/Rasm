# [PY_COMPUTE_API_SPARSE]

`sparse` supplies `COO`, `GCXS`, and `DOK` sparse array classes under a shared `SparseArray` base implementing the Python Array API, plus creation, conversion, linear-algebra, element-wise, shape, reduction, math, and IO operations over multi-dimensional sparse tensors. Format is discriminated by the `format=` keyword or `asformat`, and `fill_value` sets the implicit dense value preserved through every operation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sparse`
- package: `sparse`
- import: `import sparse`
- owner: `compute`
- rail: array
- capability: COO/GCXS/DOK sparse ndarray classes, Array API math/reduce/linalg operations, `.npz` IO, SciPy/NumPy interop, and a `numba_backend` dispatch surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse array classes
- rail: array
- all classes carry `from_numpy`, `from_scipy_sparse`, `todense`, `asformat`, `astype`, and `reduce`

| [INDEX] | [SYMBOL]             | [FORMAT]             | [CONSTRUCTOR]                                                                            |
| :-----: | :------------------- | :------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `sparse.SparseArray` | abstract base        | `SparseArray(shape, fill_value=None)`                                                    |
|  [02]   | `sparse.COO`         | coordinate           | `COO(coords, data=None, shape=None, has_duplicates=True, sorted=False, fill_value=None)` |
|  [03]   | `sparse.GCXS`        | compressed (CSR/CSC) | `GCXS(arg, shape=None, compressed_axes=None, fill_value=None)`                           |
|  [04]   | `sparse.DOK`         | dictionary of keys   | `DOK(shape, data=None, dtype=None, fill_value=None)`                                     |

[PUBLIC_TYPE_SCOPE]: shared array members
- rail: array

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                      |
| :-----: | :-------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | properties      | `nnz`, `ndim`, `size`, `dtype`, `density`, `fill_value`, `shape`, `nbytes`                     |
|  [02]   | conversion      | `todense`, `asformat`, `astype`, `to_scipy_sparse`, `from_numpy`, `from_scipy_sparse`          |
|  [03]   | reduction       | `reduce`, `sum`, `prod`, `mean`, `std`, `var`, `max`, `min`, `all`, `any`                      |
|  [04]   | transform       | `reshape`, `transpose`, `T`, `mT`, `clip`, `conj`, `round`                                     |
|  [05]   | COO/GCXS extra  | `from_iter` (COO), `linear_loc` (COO), `change_compressed_axes` (GCXS), `from_coo` (GCXS, DOK) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and conversion
- rail: array
- creation functions accept `format='coo'/'gcxs'/'dok'`; `device` is an Array API hook only

| [INDEX] | [SURFACE]                                                                      | [FAMILY]   | [PRODUCES]            |
| :-----: | :----------------------------------------------------------------------------- | :--------- | :-------------------- |
|  [01]   | `zeros(shape, dtype=float, format='coo')`                                      | creation   | empty sparse array    |
|  [02]   | `ones(shape, dtype=float, format='coo')`                                       | creation   | filled sparse array   |
|  [03]   | `full(shape, fill_value, dtype=None, format='coo')`                            | creation   | constant sparse array |
|  [04]   | `eye(N, M=None, k=0, dtype=float, format='coo')`                               | creation   | identity              |
|  [05]   | `empty(shape, dtype=float, format='coo')`                                      | creation   | uninitialised         |
|  [06]   | `random(shape, density=None, nnz=None, format='coo', fill_value=None)`         | creation   | random sparse array   |
|  [07]   | `asarray(obj, /, *, dtype=None, format=None, copy=False)`                      | conversion | sparse array          |
|  [08]   | `asCOO(x, name='asCOO', check=True)`, `as_coo(x, shape=None, fill_value=None)` | conversion | `COO`                 |
|  [09]   | `asnumpy(a, dtype=None, order=None)`                                           | conversion | dense `ndarray`       |
|  [10]   | `save_npz(filename, matrix, compressed=True)`, `load_npz(filename)`            | IO         | `.npz` round-trip     |

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
|  [10]   | `diagonal(a, offset=0, axis1=0, axis2=1)`, `diagonalize(a, axis=0)`, `triu(x, k=0)`, `tril(x, k=0)` | diagonal    |

[ENTRYPOINT_SCOPE]: element-wise, reduction, and dtype
- rail: array
- math, comparison, and bitwise functions follow the NumPy ufunc signature with broadcasting

| [INDEX] | [FAMILY]      | [MEMBERS]                                                                                                                        |
| :-----: | :------------ | :------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | dispatch      | `elemwise(func, *args, **kwargs)`, `where(condition, x=None, y=None)`                                                            |
|  [02]   | reduction     | `sum`, `prod`, `mean`, `std`, `var`, `max`, `min`, `all`, `any`, `argmax`, `argmin`, `nonzero`                                   |
|  [03]   | nan reduction | `nansum`, `nanprod`, `nanmean`, `nanmax`, `nanmin`, `nanreduce`                                                                  |
|  [04]   | math          | `abs`, `sqrt`, `square`, `exp`, `log`, `log2`, `log10`, `log1p`, `expm1`, `sign`, `reciprocal`, `negative`                       |
|  [05]   | trig          | `sin`, `cos`, `tan`, `sinh`, `cosh`, `tanh`, `asin`, `acos`, `atan`, `atan2`                                                     |
|  [06]   | rounding      | `ceil`, `floor`, `round`, `trunc`                                                                                                |
|  [07]   | comparison    | `equal`, `not_equal`, `less`, `less_equal`, `greater`, `greater_equal`                                                           |
|  [08]   | logic         | `logical_and`, `logical_or`, `logical_xor`, `logical_not`, `isfinite`, `isinf`, `isnan`, `isposinf`, `isneginf`                  |
|  [09]   | bitwise       | `bitwise_and`, `bitwise_or`, `bitwise_xor`, `bitwise_invert`, `bitwise_left_shift`, `bitwise_right_shift`                        |
|  [10]   | sort/search   | `sort(x, /, *, axis=-1, descending=False)`, `unique_values(x)`, `unique_counts(x)`, `diff(x, axis=-1, n=1)`, `interp(x, xp, fp)` |
|  [11]   | dtype         | `result_type(*arrays_and_dtypes)`, `can_cast(from_, to)`, `isdtype(dtype, kind)`, `astype(x, dtype)`                             |
|  [12]   | numba backend | `sparse.numba_backend` exposes the same array classes and operations under Numba dispatch                                        |

## [04]-[IMPLEMENTATION_LAW]

[FORMAT_TOPOLOGY]:
- Array format is chosen by passing `format='coo'/'gcxs'/'dok'` to creation functions or via `asformat`; one `asformat` discriminates instead of branching per class.
- `fill_value` determines the implicit dense value; operations preserve sparsity by never materialising fill-value positions.
- `device` and `to_device` are Array API compatibility hooks; they do not map to GPU backends in the base `sparse` namespace.
- `elemwise` is the generalised element-wise operator; it is preferred over manual loop expansion for custom scalar functions.
- SciPy interop flows through `from_scipy_sparse`/`COO.from_scipy_sparse`; the reverse path is the `todense()`-then-SciPy round-trip.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `sparse`
- Owns: COO/GCXS/DOK sparse ndarray, Array API math/reduce/linalg, `.npz` IO, SciPy/NumPy bridges
- Accept: format discrimination via `format=` keyword or `asformat`, `fill_value` for implicit dense value, `elemwise` for custom element-wise ops
- Reject: per-format parallel entrypoints, manual coordinate iteration for operations the library already implements, and dense intermediate materialisation inside sparse pipelines
