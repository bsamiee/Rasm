# [PY_BRANCH_API_NUMPY]

`numpy` supplies the N-dimensional array type `ndarray`, a complete dtype hierarchy, ufuncs for element-wise math, and submodules for linear algebra (`linalg`), discrete Fourier transforms (`fft`), and random sampling (`random.Generator`). It is the primary numerical substrate for array creation, shape manipulation, broadcasting, reduction, and numerical kernels consumed across compute and geometry owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpy`
- package: `numpy`
- module: `numpy`
- version: `2.x` (floor `>=2.0`; manifest pins for cp315 wheel availability)
- license: `BSD-3-Clause`
- asset: C/Cython-extension runtime library (compiled `_core`; not pure-Python)
- abi: per-interpreter binary wheel (`cp31x`-tagged), `Requires-Python >=3.11`
- rail: compute
- namespaces: `numpy`, `numpy.linalg`, `numpy.fft`, `numpy.random`, `numpy.ma`, `numpy.polynomial`, `numpy.lib.stride_tricks`, `numpy.testing`, `numpy.typing`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array family
- rail: compute

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [RAIL]                             |
| :-----: | :--------------- | :---------------- | :--------------------------------- |
|  [01]   | `ndarray`        | n-d array         | primary array container            |
|  [02]   | `dtype`          | type descriptor   | element type for array creation    |
|  [03]   | `ufunc`          | universal func    | element-wise C-dispatch operations |
|  [04]   | `matrix`         | 2-d array subtype | legacy 2-d multiplication surface  |
|  [05]   | `memmap`         | file-backed array | memory-mapped array I/O            |
|  [06]   | `recarray`       | record array      | field-named structured array       |
|  [07]   | `flatiter`       | flat iterator     | row-major element iteration        |
|  [08]   | `ndindex`        | index iterator    | multi-dimensional index scan       |
|  [09]   | `ndenumerate`    | enum iterator     | (index, value) iteration           |
|  [10]   | `nditer`         | advanced iterator | controlled multi-array iteration   |
|  [11]   | `broadcast`      | broadcast state   | broadcast shape computation        |
|  [12]   | `poly1d`         | polynomial        | polynomial arithmetic              |
|  [13]   | `busdaycalendar` | calendar          | business-day mask owner            |

[PUBLIC_TYPE_SCOPE]: dtype hierarchy
- rail: compute

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                     |
| :-----: | :----------------------------- | :--------------- | :------------------------- |
|  [01]   | `generic`                      | scalar base      | root of all scalar types   |
|  [02]   | `bool_`                        | boolean          | numpy boolean scalar       |
|  [03]   | `int8`/`int16`/`int32`/`int64` | signed int       | fixed-width integers       |
|  [04]   | `uint8`..`uint64`              | unsigned int     | fixed-width unsigned ints  |
|  [05]   | `float16`/`float32`/`float64`  | float            | IEEE 754 float scalars     |
|  [06]   | `complex64`/`complex128`       | complex          | complex scalar types       |
|  [07]   | `datetime64`                   | datetime scalar  | ns-resolution datetime     |
|  [08]   | `timedelta64`                  | timedelta scalar | duration scalar            |
|  [09]   | `str_`                         | Unicode string   | fixed-width Unicode scalar |
|  [10]   | `bytes_`                       | byte string      | fixed-width bytes scalar   |

[PUBLIC_TYPE_SCOPE]: info and error types
- rail: compute

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [RAIL]                          |
| :-----: | :--------- | :-------------- | :------------------------------ |
|  [01]   | `finfo`    | float info      | float dtype precision metadata  |
|  [02]   | `iinfo`    | int info        | int dtype bounds metadata       |
|  [03]   | `errstate` | context manager | floating-point error mode scope |

[PUBLIC_TYPE_SCOPE]: submodule classes
- rail: compute

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [RAIL]                       |
| :-----: | :-------------------- | :--------------- | :--------------------------- |
|  [01]   | `linalg.LinAlgError`  | linear alg error | singular/non-converging ops  |
|  [02]   | `random.Generator`    | RNG generator    | stateful PRNG surface        |
|  [03]   | `random.BitGenerator` | bit source       | PRNG state engine base       |
|  [04]   | `random.PCG64`/`PCG64DXSM` | bit source  | PCG-64 engines; `PCG64` backs `default_rng`, `PCG64DXSM` is the cheap-multiplier variant |
|  [05]   | `random.MT19937`      | bit source       | Mersenne-Twister engine      |
|  [06]   | `random.Philox`/`SFC64` | bit source     | counter-based / fast bit generators |
|  [07]   | `random.SeedSequence` | seed spawner     | reproducible seed derivation; `spawn(n)` for parallel streams |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation
- rail: compute

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------- | :------------- | :---------------------------- |
|  [01]   | `zeros(shape, dtype)`        | creation       | zero-filled array             |
|  [02]   | `ones(shape, dtype)`         | creation       | one-filled array              |
|  [03]   | `empty(shape, dtype)`        | creation       | uninitialized array           |
|  [04]   | `full(shape, fill_value)`    | creation       | constant-filled array         |
|  [05]   | `arange(start, stop, step)`  | range creation | evenly spaced integers/floats |
|  [06]   | `linspace(start, stop, num)` | range creation | evenly spaced floats          |
|  [07]   | `logspace(start, stop, num)` | range creation | log-spaced floats             |
|  [08]   | `eye(N, M, k)`               | creation       | identity / shifted diagonal   |
|  [09]   | `meshgrid(*xi)`              | creation       | coordinate grid from 1-D axes |
|  [10]   | `asarray(a, dtype, *, copy)`| conversion     | array-like to `ndarray`, no copy when already conforming |
|  [11]   | `ascontiguousarray(a)`      | conversion     | force C-contiguous layout for kernel/FFI hand-off |
|  [12]   | `frombuffer(buffer, dtype)` | zero-copy      | wrap a bytes buffer as an `ndarray` view (no copy) |
|  [13]   | `load(file)`                 | I/O            | load `.npy`/`.npz` from path  |
|  [14]   | `save(file, arr)` / `savez(file, *arrays)` | I/O | write `.npy` / `.npz` archive |
|  [15]   | `loadtxt(fname)`             | I/O            | load text-file array          |

[ENTRYPOINT_SCOPE]: shape and manipulation
- rail: compute

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------- | :------------- | :------------------------------ |
|  [01]   | `reshape(a, newshape)`      | shape          | change array shape              |
|  [02]   | `ravel(a)`                  | shape          | 1-D contiguous view             |
|  [03]   | `transpose(a, axes)`        | shape          | permute axes                    |
|  [04]   | `swapaxes(a, ax1, ax2)`     | shape          | swap two axes                   |
|  [05]   | `moveaxis(a, source, dest)` | shape          | move axis to new position       |
|  [06]   | `expand_dims(a, axis)`      | shape          | insert a new axis               |
|  [07]   | `squeeze(a, axis)`          | shape          | remove size-1 axes              |
|  [08]   | `concatenate(arrays, axis)` | join           | join arrays along existing axis |
|  [09]   | `stack(arrays, axis)`       | join           | stack arrays along new axis     |
|  [10]   | `hstack`/`vstack`/`dstack`  | join           | horizontal/vertical/depth join  |
|  [11]   | `split(a, indices, axis)`   | split          | split into sub-arrays           |
|  [12]   | `broadcast_to(a, shape)` / `broadcast_arrays(*arrays)` | broadcast | explicit broadcast view(s) without copy |
|  [13]   | `lib.stride_tricks.sliding_window_view(x, window_shape, axis)` | window view | strided rolling-window view (no copy) |
|  [14]   | `lib.stride_tricks.as_strided(x, shape, strides)` | strided view | raw stride view (unsafe; bounds not checked) |
|  [15]   | `pad(array, pad_width, mode)` | pad          | bordered/edge-padded copy        |

[ENTRYPOINT_SCOPE]: math and reduction
- rail: compute

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `sum`/`prod`/`cumsum`/`cumprod`     | reduction      | aggregation along axes            |
|  [02]   | `mean`/`std`/`var`                  | statistics     | mean, std dev, variance           |
|  [03]   | `min`/`max`/`argmin`/`argmax`       | reduction      | extrema and their indices         |
|  [04]   | `dot(a, b)`                         | linear alg     | vector/matrix dot product         |
|  [05]   | `matmul(x1, x2)`                    | linear alg     | matrix multiplication (`@`)       |
|  [06]   | `inner`/`outer`/`tensordot`         | linear alg     | inner, outer, tensor contractions |
|  [07]   | `einsum(subscripts, *operands, optimize)` | linear alg | Einstein-summation contraction, the general reduction/contraction owner |
|  [08]   | `diagonal(a, offset, axis1, axis2)` / `trace(a)` | extraction | extract diagonal / sum of diagonal |
|  [09]   | `where(cond, x, y)`                 | selection      | conditional element selection (1-arg form returns indices) |
|  [10]   | `nonzero(a)` / `flatnonzero(a)`     | selection      | indices of nonzero elements       |
|  [11]   | `clip(a, a_min, a_max)`             | elementwise    | bound values to interval          |
|  [12]   | `isfinite(x)` / `isnan(x)` / `isinf(x)` | predicate ufunc | element-wise finiteness / NaN / inf mask |
|  [13]   | `allclose(a, b, rtol, atol)` / `isclose(...)` | predicate | tolerant equality (scalar / element-wise) |
|  [14]   | `nan_to_num(x, nan, posinf, neginf)`| sanitize       | replace non-finite values with finite substitutes |
|  [15]   | `sort(a, axis)`/`argsort`           | sort           | sort or return sort indices       |
|  [16]   | `unique(ar, return_counts, return_index)` | set ops  | unique elements with optional counts/indices |
|  [17]   | `exp`/`log`/`sqrt`/`power`          | ufunc          | exponential/logarithm/power       |
|  [18]   | `sin`/`cos`/`tan`/`arctan2`         | ufunc          | trigonometric operations          |
|  [19]   | `abs`/`sign`/`round`/`floor`/`ceil` | ufunc          | absolute, rounding operations     |
|  [20]   | `nansum`/`nanmean`/`nanstd`/`nanmax`| nan-aware      | reductions skipping non-finite entries |

[ENTRYPOINT_SCOPE]: linalg submodule
- rail: compute

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :-------------------------- | :------------- | :--------------------------- |
|  [01]   | `linalg.solve(a, b)`        | linear solver  | exact solution to `Ax = b`   |
|  [02]   | `linalg.inv(a)`             | inversion      | matrix inverse               |
|  [03]   | `linalg.det(a)`             | determinant    | matrix determinant           |
|  [04]   | `linalg.eig(a)`/`eigh(a)`   | eigen          | eigenvalues and eigenvectors |
|  [05]   | `linalg.svd(a)`/`svdvals`   | decomposition  | singular value decomposition |
|  [06]   | `linalg.qr(a)`              | decomposition  | QR factorisation             |
|  [07]   | `linalg.cholesky(a)`        | decomposition  | Cholesky factorisation       |
|  [08]   | `linalg.norm(x, ord, axis)` | norm           | vector and matrix norms      |
|  [09]   | `linalg.lstsq(a, b)`        | least squares  | minimum-norm least-squares   |
|  [10]   | `linalg.pinv(a)`            | pseudo-inverse | Moore-Penrose pseudo-inverse |

[ENTRYPOINT_SCOPE]: fft submodule
- rail: compute

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :------------------------- | :------------- | :----------------------------- |
|  [01]   | `fft.fft(a, n, axis)`      | transform      | 1-D complex DFT                |
|  [02]   | `fft.ifft(a, n, axis)`     | transform      | 1-D inverse DFT                |
|  [03]   | `fft.rfft`/`irfft`         | transform      | real-input 1-D FFT/inverse     |
|  [04]   | `fft.fft2`/`ifft2`         | transform      | 2-D DFT/inverse                |
|  [05]   | `fft.fftn`/`ifftn`         | transform      | N-D DFT/inverse                |
|  [06]   | `fft.fftfreq(n, d)`        | frequency      | DFT sample frequencies         |
|  [07]   | `fft.fftshift`/`ifftshift` | shift          | shift zero-frequency to center |

[ENTRYPOINT_SCOPE]: random.Generator
- rail: compute

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :----------------------------------- | :------------- | :-------------------------- |
|  [01]   | `random.default_rng(seed)`           | factory        | create default `Generator`  |
|  [02]   | `Generator.integers(low, high)`      | sampling       | discrete uniform integers   |
|  [03]   | `Generator.random(size)`             | sampling       | uniform floats [0, 1)       |
|  [04]   | `Generator.normal(loc, scale, size)` | sampling       | normal distribution         |
|  [05]   | `Generator.uniform(low, high, size)` | sampling       | continuous uniform          |
|  [06]   | `Generator.choice(a, size, replace)` | sampling       | random selection from array |
|  [07]   | `Generator.shuffle(x)`               | permutation    | in-place shuffle            |
|  [08]   | `Generator.permutation(x)`           | permutation    | permuted copy               |

## [04]-[IMPLEMENTATION_LAW]

[COMPUTE_TOPOLOGY]:
- `ndarray` is the universal container; all array functions accept array-like inputs and return `ndarray` unless documented otherwise
- ufuncs dispatch at the C layer and support `out`, `axis`, `keepdims`, `where`, and `casting` kwargs; each ufunc also carries method attributes `reduce`, `accumulate`, `reduceat`, `outer`, and `at` (unbuffered in-place scatter, e.g. `np.add.at(a, idx, vals)`) — these are the canonical owners for fused reductions and indexed accumulation, never a Python loop
- `einsum` is the general contraction owner: it subsumes `dot`/`matmul`/`inner`/`outer`/`tensordot`/`trace`/`diagonal` under one subscript algebra and accepts `optimize=True` for a contracted path search
- broadcasting: shapes align right-to-left; size-1 axes stretch; incompatible non-1 sizes raise `ValueError`; `broadcast_to`/`sliding_window_view`/`as_strided` produce strided views with no copy
- indexing: integer, boolean mask, fancy (array of indices), and slice indexing all produce views or copies depending on contiguity; boolean-mask assignment and `np.where` cover branchless selection
- linalg in 2.x: `solve`/`inv`/`eig`/`svd`/`qr`/`cholesky` are batched (stacked) — a leading `(..., M, N)` shape applies the op over the trailing two axes, and `LinAlgError` signals singular/non-converging input
- finiteness gating: `isfinite`/`isnan`/`isinf` masks, `nan_to_num`, and the `nan*` reductions are the canonical owners for non-finite handling; `errstate` scopes the FP error mode (`raise`/`warn`/`ignore`) for a kernel block
- random Generator is the preferred API; `numpy.random.RandomState` is legacy and non-reentrant; `SeedSequence.spawn(n)` derives independent child seeds for parallel streams

[STACKS_WITH]:
- msgspec wire round-trip: a numeric block crosses the wire as a base64/`bytes` field or `Raw`; `np.frombuffer(buf, dtype).reshape(shape)` reconstructs a zero-copy view on the consumer side, and `ascontiguousarray` guarantees the C-contiguous layout before a `Struct` re-encodes the buffer — `numpy` owns the dtype, `msgspec` owns the envelope.
- otel measurement: scalar reductions (`mean`/`std`/`isfinite().sum()`) yield the `float`/`int` values fed to `Histogram.record` / `Gauge.set`; cast NumPy scalars with `float(x)` before handing to the attribute map (which rejects NumPy scalar types).
- linalg solver hand-off: `asarray(..., dtype=float64)` conditions an array before `linalg.solve`/`lstsq`/`pinv`; `isfinite(result).all()` is the post-solve finiteness gate that maps a non-converged `LinAlgError`-adjacent result to a typed solver-failure rail.
- FFI / kernel boundary: `ascontiguousarray` + `ndarray.__array_interface__`/buffer protocol expose the raw pointer that downstream C-extension owners (geometry/mesh kernels) read without a copy.

[LOCAL_ADMISSION]:
- Array construction always specifies `dtype` when precision matters; default float is `float64`; `asarray(x, dtype=...)` is the canonical no-copy-when-conforming intake.
- `random.default_rng(seed)` is the single entry point for reproducible sampling; never use module-level random functions; `SeedSequence` derives parallel-stream seeds.
- `linalg` operations accept real and complex arrays and batch over leading axes; choose `eigh`/`cholesky` for Hermitian/SPD shortcuts over `eig`/`solve`.
- `fft` frequency bins follow NumPy convention: `fftfreq(n, d)` returns cycles per `d` unit.
- Validate numeric outputs with `isfinite(...).all()` (not bare comparisons) before graduating a result across a boundary.

[RAIL_LAW]:
- Package: `numpy`
- Owns: N-d array construction, dtype algebra, ufunc dispatch (incl. `reduce`/`accumulate`/`at`), `einsum` contraction, linalg, fft, finiteness predicates, and random sampling
- Accept: `ndarray`-first APIs, explicit `dtype`, `asarray`/`frombuffer` zero-copy intake, `einsum`/ufunc-method fused reductions, `random.default_rng`/`SeedSequence` seeding, `isfinite` finiteness gates, batched `linalg`/`fft`/`random` usage
- Reject: hand-rolled numerical loops replaceable by ufuncs or `einsum`, indexed Python accumulation replaceable by `ufunc.at`, module-level `numpy.random` functions, `numpy.matrix` for new code, comparing floats for finiteness without `isfinite`
