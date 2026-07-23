# [PY_COMPUTE_API_SPARSE]

`sparse` owns n-dimensional sparse arrays for the compute array rail: `COO`, `GCXS`, and `DOK` classes under a `SparseArray` base implementing the Python Array API, with creation, conversion, linear-algebra, element-wise, shape, reduction, and `.npz` IO over arbitrary rank. `format=`/`asformat` discriminates one polymorphic value across the three formats and `fill_value` fixes the implicit dense value carried through every operation; `sparse` complements `scipy.sparse` by owning the >2-D `tensordot`/`einsum`/broadcast algebra 2-D CSR/CSC cannot express, and keeps every intermediate sparse.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sparse`
- package: `sparse` (BSD-3-Clause, PyData)
- import: `sparse`
- owner: `compute`
- rail: array
- asset: pure Python; the optional `sparse.numba_backend` JIT re-export requires `numba`/`llvmlite`
- capability: n-dimensional sparse arrays with the full Array-API operation set and an optional Numba JIT backend

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: sparse array classes
- format is data, not a type axis — `asformat('coo'|'gcxs'|'dok')` discriminates one polymorphic value rather than branching per class

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------- | :------------ | :----------------------------------------- |
|  [01]   | `sparse.SparseArray` | abstract base | Array-API base every format subclasses     |
|  [02]   | `sparse.COO`         | coordinate    | coordinate-list n-D array                  |
|  [03]   | `sparse.GCXS`        | compressed    | generalized CSR/CSC compressed n-D array   |
|  [04]   | `sparse.DOK`         | dictionary    | dictionary-of-keys incremental-build array |

[PUBLIC_TYPE_SCOPE]: shared array members

| [INDEX] | [MEMBER_FAMILY] | [MEMBERS]                                                                                                         |
| :-----: | :-------------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | properties      | `nnz`, `ndim`, `size`, `dtype`, `density`, `fill_value`, `shape`, `nbytes`, `T`, `mT`, `real`, `imag`, `device`   |
|  [02]   | densify/convert | `todense`, `maybe_densify`, `asformat`, `astype`, `to_device`, `from_numpy`, `from_scipy_sparse`                  |
|  [03]   | reduction       | `reduce`, `sum`, `prod`, `mean`, `std`, `var`, `max`, `min`, `amax`, `amin`, `all`, `any`                         |
|  [04]   | transform       | `reshape`, `transpose`, `swapaxes`, `flatten`, `squeeze`, `clip`, `conj`, `round`/`round_`                        |
|  [05]   | predicate       | `isnan`, `isinf`, `nonzero`                                                                                       |
|  [06]   | COO-only        | `from_iter`, `linear_loc`, `tocsr`, `tocsc`, `to_scipy_sparse`, `enable_caching`, `copy`                          |
|  [07]   | GCXS-only       | `from_iter`, `from_coo`, `change_compressed_axes`, `compressed_axes`, `tocoo`, `todok`, `to_scipy_sparse`, `copy` |
|  [08]   | DOK-only        | `from_coo`, `to_coo` (no `to_scipy_sparse`; round-trip DOK->COO->scipy)                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: construction and conversion
- creation functions take `format='coo'/'gcxs'/'dok'` and `fill_value`; class constructors build a format directly

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [RESULT]                 |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :----------------------- |
|  [01]   | `COO(coords, data=None, shape=None, has_duplicates=True, sorted=False, fill_value=None)` | ctor           | coordinate array         |
|  [02]   | `GCXS(arg, shape=None, compressed_axes=None, fill_value=None)`                           | ctor           | compressed array         |
|  [03]   | `DOK(shape, data=None, dtype=None, fill_value=None)`                                     | ctor           | dictionary-of-keys array |
|  [04]   | `zeros` / `ones` / `empty` / `full(shape, fill_value, ..., format='coo')`                | creation       | constant sparse array    |
|  [05]   | `zeros_like` / `ones_like` / `empty_like` / `full_like(a, ...)`                          | creation       | like-shaped sparse array |
|  [06]   | `eye(N, M=None, k=0, dtype=float, format='coo')`                                         | creation       | identity                 |
|  [07]   | `random(shape, density=None, nnz=None, format='coo', fill_value=None)`                   | creation       | random sparse array      |
|  [08]   | `asarray(obj, /, *, dtype=None, format=None, copy=False)`                                | conversion     | sparse array             |
|  [09]   | `asCOO(x, name='asCOO', check=True)` \| `as_coo(x, shape=None, fill_value=None)`         | conversion     | `COO`                    |
|  [10]   | `asnumpy(a, dtype=None, order=None)`                                                     | conversion     | dense `ndarray`          |
|  [11]   | `save_npz(filename, matrix, compressed=True)` \| `load_npz(filename)`                    | IO             | `.npz` round-trip        |

[ENTRYPOINT_SCOPE]: linear algebra and shape

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY]  |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `dot(a, b)`, `matmul(a, b)`, `vecdot(x1, x2, /, *, axis=-1)`                                                       | matrix mul      |
|  [02]   | `tensordot(a, b, axes=2, *, return_type=None)`, `einsum(*operands)`                                                | contraction     |
|  [03]   | `kron(a, b)`, `outer(a, b, out=None)`                                                                              | products        |
|  [04]   | `stack(arrays, axis=0)`, `concat(arrays, axis=0)`, `concatenate(arrays, axis=0)`                                   | join            |
|  [05]   | `broadcast_to(x, /, shape)`, `broadcast_arrays(*arrays)`, `expand_dims(x, /, *, axis=0)`                           | broadcast       |
|  [06]   | `reshape(x, /, shape)`, `squeeze(x, /, axis=None)`, `moveaxis(a, source, destination)`                             | reshape         |
|  [07]   | `permute_dims(x, /, axes=None)`, `matrix_transpose(x, /)`, `flip(x, /, *, axis=None)`                              | axes            |
|  [08]   | `roll(a, shift, axis=None)`, `pad(array, pad_width, mode='constant')`, `unstack(x, axis=0)`                        | rearrange       |
|  [09]   | `tile(a, reps)`, `repeat(a, repeats, axis=None)`, `take(x, indices, /, *, axis=None)`                              | replicate       |
|  [10]   | `diagonal(a, offset=0, axis1=0, axis2=1)`, `diagonalize(a, axis=0)`, `triu(x, k=0)`, `tril(x, k=0)`, `argwhere(a)` | diagonal/locate |

[ENTRYPOINT_SCOPE]: element-wise, reduction, and dtype
- element-wise families follow the NumPy ufunc signature with broadcasting

| [INDEX] | [FAMILY]      | [MEMBERS]                                                                                                                |
| :-----: | :------------ | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | dispatch      | `elemwise(func, *args, **kwargs)`, `where(condition, x=None, y=None)`                                                    |
|  [02]   | binary ufunc  | `add`, `subtract`, `multiply`, `divide`, `floor_divide`, `remainder`, `pow`                                              |
|  [03]   | pairwise      | `maximum`, `minimum`, `logaddexp`, `nextafter`, `copysign`, `hypot`, `signbit`                                           |
|  [04]   | reduction     | `sum`, `prod`, `mean`, `std`, `var`, `max`, `min`, `all`, `any`, `argmax`, `argmin`, `nonzero`                           |
|  [05]   | nan reduction | `nansum`, `nanprod`, `nanmean`, `nanmax`, `nanmin`, `nanreduce(arr, method, identity, axis, keepdims)`                   |
|  [06]   | math          | `abs`, `sqrt`, `square`, `exp`, `log`, `log2`, `log10`, `log1p`, `expm1`, `sign`, `reciprocal`, `negative`, `positive`   |
|  [07]   | trig          | `sin`, `cos`, `tan`, `sinh`, `cosh`, `tanh`, `asin`, `acos`, `atan`, `asinh`, `acosh`, `atanh`, `atan2`                  |
|  [08]   | rounding      | `ceil`, `floor`, `round`, `trunc`                                                                                        |
|  [09]   | comparison    | `equal`, `not_equal`, `less`, `less_equal`, `greater`, `greater_equal`                                                   |
|  [10]   | logic         | `logical_and`, `logical_or`, `logical_xor`, `logical_not`, `isfinite`, `isinf`, `isnan`, `isposinf`, `isneginf`          |
|  [11]   | bitwise       | `bitwise_and`, `bitwise_or`, `bitwise_xor`, `bitwise_invert`, `bitwise_not`, `bitwise_left_shift`, `bitwise_right_shift` |
|  [12]   | sort/search   | `sort(x, /, *, axis=-1, descending=False)`, `unique_values`, `unique_counts`, `diff(x, axis=-1, n=1)`, `interp`          |
|  [13]   | dtype         | `result_type(*arrays_and_dtypes)`, `can_cast(from_, to)`, `isdtype(dtype, kind)`, `astype(x, dtype)`, `finfo`, `iinfo`   |
|  [14]   | numba backend | `sparse.numba_backend` re-exports the array classes and operations under Numba JIT dispatch                              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `format='coo'/'gcxs'/'dok'` on creation or `asformat` selects storage; one `asformat` discriminates instead of per-class branching, and inter-format moves are direct methods — COO `tocsr`/`tocsc`, GCXS `tocoo`/`todok`, DOK `to_coo` — never a dense round-trip.
- `fill_value` is the implicit dense value; every operation preserves sparsity by never materialising fill-value positions, and a non-zero `fill_value` redefines which entries are structural and rides through every transform.
- `reduce(method, axis, keepdims, **kwargs)` is the generalised reduction, `nanreduce(arr, method, identity, ...)` its NaN-skipping form, and `elemwise(func, *args)` the generalised element-wise operator for a custom scalar function.
- `maybe_densify(max_size, min_density)` gates densification — it raises past the size/density bound instead of blowing memory, the boundary for handing a result to a dense consumer.
- `device`/`to_device` are Array-API compatibility hooks and bind no GPU backend in the base namespace.

[STACKING]:
- `scipy`(`.api/scipy.md`): `from_scipy_sparse`/`COO.from_scipy_sparse` lift a 2-D `scipy.sparse` matrix in, and `GCXS.to_scipy_sparse`/`COO.to_scipy_sparse` hand a 2-D result back for `scipy.sparse.linalg` (`spsolve`, `cg`, `gmres`, `eigsh`); `sparse` keeps the >2-D `tensordot`/`einsum`/broadcast algebra scipy.sparse cannot express.
- `array-api-compat`(`.api/array-api-compat.md`) / `array-api-extra`(`.api/array-api-extra.md`): a `sparse` array satisfies the Array-API namespace, so `array_namespace(x)` dispatch folds it beside `numpy`/`jax`/`dask` with no format branch.
- `numba`(`.api/numba.md`): `sparse.numba_backend` re-exports the class and operation set under Numba JIT for a hot CPU-bound loop.
- `numerics/array.md`: `ArrayPayload` admits a `sparse` array as one namespace-dispatched backend, so a sparse payload folds the compute array rail unchanged.

[LOCAL_ADMISSION]:
- entry: a sparse payload enters through `asarray(..., format=...)` or `from_scipy_sparse`, `asformat`-discriminated, never a parallel per-format entrypoint.
- evidence: a sparse-array fold captures format, `nnz`/`density`, and `fill_value` as the array claim, and `maybe_densify` gates densification so a memory blow rails as a typed boundary, not an OOM.
- boundary: results stay sparse across the pipeline, and `todense`/`asnumpy` materialise only at a declared dense-consumer edge.

[RAIL_LAW]:
- Package: `sparse`
- Owns: COO/GCXS/DOK n-dimensional sparse arrays, the Array-API math/reduce/linalg surface over arbitrary rank, inter-format conversion, the SciPy/NumPy bridges, `.npz` IO, and the optional Numba JIT backend
- Accept: an n-D sparse payload under `asformat` discrimination, a `fill_value` for the implicit dense value, and `elemwise`/`reduce` for custom operations
- Reject: per-format parallel entrypoints, manual coordinate iteration for library-owned operations, dense intermediate materialisation inside a sparse pipeline, and re-implementing the 2-D sparse LA `scipy.sparse.linalg` owns
