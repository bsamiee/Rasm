# [PY_BRANCH_API_NUMPY]

`numpy` owns the branch N-dimensional array substrate: the `ndarray` container, the dtype and abstract-scalar hierarchy, C-dispatched ufuncs, and the `linalg`/`fft`/`random` numeric kernels every compute and geometry owner composes for creation, broadcasting, reduction, and linear algebra.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpy`
- package: `numpy` (`BSD-3-Clause`)
- module: `numpy`
- namespaces: `numpy`, `numpy.linalg`, `numpy.fft`, `numpy.random`, `numpy.ma`, `numpy.polynomial`, `numpy.lib.stride_tricks`, `numpy.testing`, `numpy.typing`
- asset: C/Cython-extension runtime; compiled `_core`
- rail: compute

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array family

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [CAPABILITY]                       |
| :-----: | :--------------- | :---------------- | :--------------------------------- |
|  [01]   | `ndarray`        | n-d array         | primary array container            |
|  [02]   | `dtype`          | type descriptor   | element type for array creation    |
|  [03]   | `ufunc`          | universal func    | element-wise C-dispatch operations |
|  [04]   | `memmap`         | file-backed array | memory-mapped array I/O            |
|  [05]   | `recarray`       | record array      | field-named structured array       |
|  [06]   | `flatiter`       | flat iterator     | row-major element iteration        |
|  [07]   | `ndindex`        | index iterator    | multi-dimensional index scan       |
|  [08]   | `ndenumerate`    | enum iterator     | (index, value) iteration           |
|  [09]   | `nditer`         | advanced iterator | controlled multi-array iteration   |
|  [10]   | `broadcast`      | broadcast state   | broadcast shape computation        |
|  [11]   | `poly1d`         | polynomial        | polynomial arithmetic              |
|  [12]   | `busdaycalendar` | calendar          | business-day mask owner            |

[PUBLIC_TYPE_SCOPE]: dtype hierarchy

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [CAPABILITY]                                                          |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `generic`                      | scalar base      | root of all scalar types                                              |
|  [02]   | `bool_`                        | boolean          | numpy boolean scalar                                                  |
|  [03]   | `int8`/`int16`/`int32`/`int64` | signed int       | fixed-width integers                                                  |
|  [04]   | `uint8`..`uint64`              | unsigned int     | fixed-width unsigned ints                                             |
|  [05]   | `float16`/`float32`/`float64`  | float            | IEEE 754 float scalars                                                |
|  [06]   | `complex64`/`complex128`       | complex          | complex scalar types                                                  |
|  [07]   | `datetime64`                   | datetime scalar  | ns-resolution datetime                                                |
|  [08]   | `timedelta64`                  | timedelta scalar | duration scalar                                                       |
|  [09]   | `str_`                         | Unicode string   | fixed-width Unicode scalar                                            |
|  [10]   | `bytes_`                       | byte string      | fixed-width bytes scalar                                              |
|  [11]   | `intp`/`uintp`                 | index int        | pointer-sized platform index dtype (`astype(np.intp)` on index array) |

[PUBLIC_TYPE_SCOPE]: abstract scalar bases
- `issubdtype(dtype, base)` targets and `NDArray[base]` annotation bounds sitting between `generic` and the concrete scalars

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :---------------- | :------------ | :------------------------------------ |
|  [01]   | `number`          | scalar base   | numeric root under `generic`          |
|  [02]   | `integer`         | scalar base   | signed + unsigned ints under `number` |
|  [03]   | `signedinteger`   | scalar base   | signed ints under `integer`           |
|  [04]   | `unsignedinteger` | scalar base   | unsigned ints under `integer`         |
|  [05]   | `inexact`         | scalar base   | float + complex under `number`        |
|  [06]   | `floating`        | scalar base   | real floats under `inexact`           |
|  [07]   | `complexfloating` | scalar base   | complex floats under `inexact`        |

[PUBLIC_TYPE_SCOPE]: info and error types

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [CAPABILITY]                    |
| :-----: | :--------- | :-------------- | :------------------------------ |
|  [01]   | `finfo`    | float info      | float dtype precision metadata  |
|  [02]   | `iinfo`    | int info        | int dtype bounds metadata       |
|  [03]   | `errstate` | context manager | floating-point error mode scope |

[PUBLIC_TYPE_SCOPE]: submodule classes

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [CAPABILITY]                                                                             |
| :-----: | :------------------------- | :--------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `linalg.LinAlgError`       | linear alg error | singular/non-converging ops                                                              |
|  [02]   | `random.Generator`         | RNG generator    | stateful PRNG surface                                                                    |
|  [03]   | `random.BitGenerator`      | bit source       | PRNG state engine base                                                                   |
|  [04]   | `random.PCG64`/`PCG64DXSM` | bit source       | PCG-64 engines; `PCG64` backs `default_rng`, `PCG64DXSM` is the cheap-multiplier variant |
|  [05]   | `random.MT19937`           | bit source       | Mersenne-Twister engine                                                                  |
|  [06]   | `random.Philox`/`SFC64`    | bit source       | counter-based / fast bit generators                                                      |
|  [07]   | `random.SeedSequence`      | seed spawner     | reproducible seed derivation; `spawn(n)` for parallel streams                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation

| [INDEX] | [SURFACE]                                                     | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------ | :------ | :----------------------------------------------- |
|  [01]   | `zeros(shape, dtype)`                                         | factory | zero-filled array                                |
|  [02]   | `ones(shape, dtype)`                                          | factory | one-filled array                                 |
|  [03]   | `empty(shape, dtype)`                                         | factory | uninitialized array                              |
|  [04]   | `full(shape, fill_value)`                                     | factory | constant-filled array                            |
|  [05]   | `zeros_like`/`ones_like`/`empty_like`/`full_like(a, v)`       | factory | shape/dtype-matched fill of an existing array    |
|  [06]   | `fromiter(iterable, dtype, count)`                            | factory | 1-D array from an iterable, no temp list         |
|  [07]   | `array(object, dtype=None, *, copy=True, order='K', ndmin=0)` | factory | new copied array (vs `asarray` no-copy intake)   |
|  [08]   | `arange(start, stop, step)`                                   | factory | evenly spaced integers/floats                    |
|  [09]   | `linspace(start, stop, num)`                                  | factory | evenly spaced floats                             |
|  [10]   | `logspace(start, stop, num)`                                  | factory | log-spaced floats                                |
|  [11]   | `eye(N, M, k)`                                                | factory | identity / shifted diagonal                      |
|  [12]   | `meshgrid(*xi)`                                               | factory | coordinate grid from 1-D axes                    |
|  [13]   | `asarray(a, dtype, *, copy)`                                  | factory | array-like to `ndarray`, no copy when conforming |
|  [14]   | `ascontiguousarray(a)`                                        | factory | force C-contiguous layout for kernel/FFI         |
|  [15]   | `frombuffer(buffer, dtype)`                                   | factory | wrap a bytes buffer as an `ndarray` view         |
|  [16]   | `geomspace(start, stop, num)`                                 | factory | same-sign or complex geometric progression       |
|  [17]   | `load(file)`                                                  | static  | load `.npy`/`.npz` from path                     |
|  [18]   | `save(file, arr)` / `savez(file, *arrays)`                    | static  | write `.npy` / `.npz` archive                    |
|  [19]   | `loadtxt(fname)`                                              | static  | load text-file array                             |

[ENTRYPOINT_SCOPE]: shape and manipulation

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------- | :------ | :------------------------------------------- |
|  [01]   | `reshape(a, newshape)`                                          | static  | change array shape                           |
|  [02]   | `ravel(a)`                                                      | static  | 1-D contiguous view                          |
|  [03]   | `transpose(a, axes)`                                            | static  | permute axes                                 |
|  [04]   | `swapaxes(a, ax1, ax2)`                                         | static  | swap two axes                                |
|  [05]   | `moveaxis(a, source, dest)`                                     | static  | move axis to new position                    |
|  [06]   | `expand_dims(a, axis)`                                          | static  | insert a new axis                            |
|  [07]   | `squeeze(a, axis)`                                              | static  | remove size-1 axes                           |
|  [08]   | `concatenate(arrays, axis)`                                     | static  | join arrays along existing axis              |
|  [09]   | `stack(arrays, axis)`                                           | static  | stack arrays along new axis                  |
|  [10]   | `hstack`/`vstack`/`dstack`                                      | static  | horizontal/vertical/depth join               |
|  [11]   | `split(a, indices, axis)`                                       | static  | split into sub-arrays                        |
|  [12]   | `broadcast_to(a, shape)` / `broadcast_arrays(*arrays)`          | static  | explicit broadcast view(s) without copy      |
|  [13]   | `lib.stride_tricks.sliding_window_view(x, window_shape, axis)`  | static  | strided rolling-window view (no copy)        |
|  [14]   | `lib.stride_tricks.as_strided(x, shape, strides)`               | static  | raw stride view (unsafe; bounds not checked) |
|  [15]   | `pad(array, pad_width, mode)`                                   | static  | bordered/edge-padded copy                    |
|  [16]   | `atleast_1d(*arys)` / `atleast_2d(*arys)` / `atleast_3d(*arys)` | static  | promote inputs to at-least 1/2/3-D           |
|  [17]   | `column_stack(tup)`                                             | static  | stack 1-D arrays as columns of a 2-D array   |
|  [18]   | `append(arr, values, axis)`                                     | static  | concatenated copy with `values` appended     |
|  [19]   | `delete(arr, obj, axis)`                                        | static  | copy with elements at `obj` indices removed  |
|  [20]   | `roll(a, shift, axis)`                                          | static  | cyclic shift along axis (wraps; no fill)     |
|  [21]   | `ix_(*args)`                                                    | static  | open mesh for cross-axis advanced indexing   |

[ENTRYPOINT_SCOPE]: math and reduction

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------- | :------ | :------------------------------------------------------------- |
|  [01]   | `sum`/`prod`/`cumsum`/`cumprod`                  | static  | aggregation along axes                                         |
|  [02]   | `mean`/`std`/`var`                               | static  | mean, std dev, variance                                        |
|  [03]   | `min`/`max`/`argmin`/`argmax`                    | static  | extrema and their indices                                      |
|  [04]   | `dot(a, b)`                                      | static  | vector/matrix dot product                                      |
|  [05]   | `matmul(x1, x2)`                                 | static  | matrix multiplication (`@`)                                    |
|  [06]   | `inner`/`outer`/`tensordot`                      | static  | inner, outer, tensor contractions                              |
|  [07]   | `einsum(subscripts, *operands, optimize)`        | static  | Einstein-summation contraction owner                           |
|  [08]   | `diagonal(a, offset, axis1, axis2)` / `trace(a)` | static  | extract diagonal / sum of diagonal                             |
|  [09]   | `where(cond, x, y)`                              | static  | conditional element selection (1-arg form returns indices)     |
|  [10]   | `nonzero(a)` / `flatnonzero(a)`                  | static  | indices of nonzero elements                                    |
|  [11]   | `clip(a, a_min, a_max)`                          | static  | bound values to interval                                       |
|  [12]   | `isfinite(x)` / `isnan(x)` / `isinf(x)`          | static  | element-wise finiteness / NaN / inf mask                       |
|  [13]   | `allclose(a, b, rtol, atol)` / `isclose(...)`    | static  | tolerant equality (scalar / element-wise)                      |
|  [14]   | `nan_to_num(x, nan, posinf, neginf)`             | static  | replace non-finite values with finite substitutes              |
|  [15]   | `sort(a, axis)`/`argsort`/`lexsort(keys)`        | static  | sort, argsort, or stable multi-key `lexsort`                   |
|  [16]   | `unique(ar, return_counts, return_index)`        | static  | unique elements with optional counts/indices                   |
|  [17]   | `exp`/`log`/`sqrt`/`power`/`divide`              | static  | exponential/logarithm/power; masked `divide(out=, where=)`     |
|  [18]   | `sin`/`cos`/`tan`/`arctan2`                      | static  | trigonometric operations                                       |
|  [19]   | `abs`/`sign`/`round`/`floor`/`ceil`              | static  | absolute, rounding operations                                  |
|  [20]   | `nansum`/`nanmean`/`nanstd`/`nanmax`             | static  | reductions skipping non-finite entries                         |
|  [21]   | `angle(z, deg)` / `conj(x)` / `conjugate(x)`     | static  | phase angle and complex conjugate                              |
|  [22]   | `maximum(x1, x2)` / `minimum(x1, x2)`            | static  | element-wise pairwise extrema (vs `max`/`min` reductions)      |
|  [23]   | `add`/`subtract`/`multiply`                      | static  | arithmetic binary ufuncs                                       |
|  [24]   | `bitwise_or`/`bitwise_and`/`bitwise_xor`         | static  | bitwise binary ufuncs (`bitwise_or.reduce` unions flags)       |
|  [25]   | `median(a, axis)`                                | static  | median along axis (`nanmedian` skips non-finite)               |
|  [26]   | `count_nonzero(a, axis, keepdims)`               | static  | count of nonzero/`True` elements                               |
|  [27]   | `interp(x, xp, fp, left, right, period)`         | static  | 1-D piecewise-linear interpolation against monotonic `xp`/`fp` |
|  [28]   | `gradient(f, *varargs, axis, edge_order)`        | static  | central-difference numerical gradient                          |
|  [29]   | `diff(a, n, axis)`                               | static  | n-th discrete difference along axis                            |
|  [30]   | `trapezoid(y, x, dx, axis)`                      | static  | trapezoidal integral                                           |
|  [31]   | `issubdtype(arg1, arg2)`                         | static  | dtype subclass test vs an abstract scalar base                 |
|  [32]   | `searchsorted(a, v, side)`                       | static  | insertion indices in a sorted array                            |
|  [33]   | `percentile(a, q, axis, method)`                 | static  | percentile reduction along an axis                             |
|  [34]   | `signbit(x)`                                     | static  | element-wise negative-sign mask                                |
|  [35]   | `any(a, axis)` / `all(a, axis)`                  | static  | existential or universal boolean reduction                     |
|  [36]   | `deg2rad(x)` / `rad2deg(x)`                      | static  | degree-radian angle conversion                                 |
|  [37]   | `cross(a, b, axis)`                              | static  | 3-vector cross product over the chosen axis                    |

[ENTRYPOINT_SCOPE]: top-level constants

| [INDEX] | [SYMBOL]    | [VALUE]            | [CAPABILITY]                                |
| :-----: | :---------- | :----------------- | :------------------------------------------ |
|  [01]   | `pi`        | `3.14159...`       | circle constant for phase/frequency scaling |
|  [02]   | `e`         | `2.71828...`       | natural-log base for geometric spacing      |
|  [03]   | `inf`/`nan` | IEEE 754 sentinels | infinity and not-a-number literals          |

[ENTRYPOINT_SCOPE]: linalg submodule

| [INDEX] | [SURFACE]                                        | [SHAPE] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------- | :------ | :------------------------------------------------------ |
|  [01]   | `linalg.solve(a, b)`                             | static  | exact solution to `Ax = b`                              |
|  [02]   | `linalg.inv(a)`                                  | static  | matrix inverse                                          |
|  [03]   | `linalg.det(a)`                                  | static  | matrix determinant                                      |
|  [04]   | `linalg.eig(a)`/`eigh(a)`                        | static  | eigenvalues and eigenvectors                            |
|  [05]   | `linalg.svd(a)`/`svdvals`                        | static  | singular value decomposition                            |
|  [06]   | `linalg.qr(a)`                                   | static  | QR factorisation                                        |
|  [07]   | `linalg.cholesky(a)`                             | static  | Cholesky factorisation                                  |
|  [08]   | `linalg.norm(x, ord, axis)`                      | static  | vector and matrix norms                                 |
|  [09]   | `linalg.lstsq(a, b)`                             | static  | minimum-norm least-squares                              |
|  [10]   | `linalg.pinv(a)`                                 | static  | Moore-Penrose pseudo-inverse                            |
|  [11]   | `linalg.eigvalsh(a, UPLO)` / `linalg.eigvals(a)` | static  | eigenvalues-only (Hermitian / general), no eigenvectors |

[ENTRYPOINT_SCOPE]: fft submodule

| [INDEX] | [SURFACE]                  | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------- | :------ | :----------------------------- |
|  [01]   | `fft.fft(a, n, axis)`      | static  | 1-D complex DFT                |
|  [02]   | `fft.ifft(a, n, axis)`     | static  | 1-D inverse DFT                |
|  [03]   | `fft.rfft`/`irfft`         | static  | real-input 1-D FFT/inverse     |
|  [04]   | `fft.fft2`/`ifft2`         | static  | 2-D DFT/inverse                |
|  [05]   | `fft.fftn`/`ifftn`         | static  | N-D DFT/inverse                |
|  [06]   | `fft.fftfreq(n, d)`        | static  | DFT sample frequencies         |
|  [07]   | `fft.fftshift`/`ifftshift` | static  | shift zero-frequency to center |

[ENTRYPOINT_SCOPE]: random.Generator

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]                |
| :-----: | :----------------------------------- | :------- | :-------------------------- |
|  [01]   | `random.default_rng(seed)`           | factory  | create default `Generator`  |
|  [02]   | `Generator.integers(low, high)`      | instance | discrete uniform integers   |
|  [03]   | `Generator.random(size)`             | instance | uniform floats [0, 1)       |
|  [04]   | `Generator.normal(loc, scale, size)` | instance | normal distribution         |
|  [05]   | `Generator.uniform(low, high, size)` | instance | continuous uniform          |
|  [06]   | `Generator.choice(a, size, replace)` | instance | random selection from array |
|  [07]   | `Generator.shuffle(x)`               | instance | in-place shuffle            |
|  [08]   | `Generator.permutation(x)`           | instance | permuted copy               |
|  [09]   | `Generator.standard_normal(size)`    | instance | standard normal samples     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ndarray` is the universal container: every array function admits array-like input and returns `ndarray`.
- ufuncs dispatch in C over `out`/`axis`/`keepdims`/`where`/`casting` and carry method rails `reduce`/`accumulate`/`reduceat`/`outer`/`at` — `at` scatters unbuffered in-place (`np.add.at(a, idx, vals)`) — the owners for fused reductions and indexed accumulation.
- `einsum` owns general contraction, subsuming `dot`/`matmul`/`inner`/`outer`/`tensordot`/`trace`/`diagonal` under one subscript algebra with `optimize=True` path search.
- Broadcasting aligns shapes right-to-left, stretches size-1 axes, and raises `ValueError` on incompatible non-1 sizes; `broadcast_to`/`sliding_window_view`/`as_strided` yield no-copy strided views.
- Integer, boolean-mask, fancy, and slice indexing yield views or copies by contiguity; boolean-mask assignment and `where` own branchless selection.
- `linalg` batches over leading axes: a `(..., M, N)` stack applies the op on the trailing two axes, `LinAlgError` signaling singular/non-converging input.
- `isfinite`/`isnan`/`isinf`, `nan_to_num`, and the `nan*` reductions own non-finite handling; `errstate` scopes the FP error mode for a kernel block.
- `random.Generator` owns sampling; `SeedSequence.spawn(n)` derives independent child seeds for parallel streams.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): `ndarray.tobytes()` (C-order copy sized from `ndarray.nbytes`) flattens a numeric block into a `bytes`/`Raw` `Struct` field, `frombuffer(buf, dtype).reshape(shape)` rebuilds the zero-copy view, and `ascontiguousarray` guarantees C-contiguity before re-encode — `numpy` owns the dtype, `msgspec` the envelope.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): scalar reductions (`mean`/`std`/`isfinite().sum()`) cast through `float(x)` feed `Histogram.record`/`Gauge.set`, whose attribute map rejects raw NumPy scalar types.
- `meshio`(`.api/meshio.md`): `ascontiguousarray` + `ndarray.__array_interface__`/buffer protocol expose the raw pointer downstream C-extension mesh/geometry owners read without a copy.
- within-lib: `asarray(dtype=float64)` conditions input for `linalg.solve`/`lstsq`/`pinv` and `isfinite(result).all()` gates the post-solve result onto a typed solver-failure rail; `einsum` + ufunc method rails + batched `linalg` compose fused numeric kernels with no Python-loop fallback.

[LOCAL_ADMISSION]:
- Array construction names `dtype` when precision matters; `float64` is the default and `asarray(x, dtype=...)` the no-copy-when-conforming intake.
- `random.default_rng(seed)` is the single reproducible-sampling entry; `SeedSequence` derives parallel-stream seeds.
- `linalg` admits real and complex arrays and batches over leading axes; `eigh`/`cholesky` take the Hermitian/SPD shortcut over `eig`/`solve`.
- `fft` frequency bins follow `fftfreq(n, d)` returning cycles per `d` unit.
- `isfinite(...).all()` gates a numeric output before it graduates across a boundary.

[RAIL_LAW]:
- Package: `numpy`
- Owns: N-d array construction, the dtype and abstract-scalar algebra (`issubdtype`/`intp`), ufunc dispatch and its `reduce`/`accumulate`/`outer`/`at` rails, numerical calculus and statistics, top-level math constants, `einsum` contraction, batched `linalg`/`fft`, finiteness predicates, and random sampling.
- Accept: `ndarray`-first APIs, explicit `dtype`, `asarray`/`frombuffer` zero-copy intake, `einsum`/ufunc-method fused reductions, `random.default_rng`/`SeedSequence` seeding, `isfinite` gates, batched `linalg`/`fft`/`random`.
- Reject: hand-rolled numerical loops an ufunc or `einsum` replaces, indexed Python accumulation `ufunc.at` replaces, module-level `numpy.random` functions, `numpy.matrix`, finiteness comparison without `isfinite`.
