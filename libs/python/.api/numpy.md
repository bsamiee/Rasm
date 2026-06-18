# [PY_BRANCH_API_NUMPY]

`numpy` supplies the N-dimensional array type `ndarray`, a complete dtype hierarchy, ufuncs for element-wise math, and submodules for linear algebra (`linalg`), discrete Fourier transforms (`fft`), and random sampling (`random.Generator`). It is the primary numerical substrate for array creation, shape manipulation, broadcasting, reduction, and numerical kernels consumed across compute and geometry owners.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpy`
- package: `numpy`
- module: `numpy`
- asset: runtime library
- rail: compute

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array family
- rail: compute

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]     | [RAIL]                             |
| :-----: | :--------------- | :---------------- | :--------------------------------- |
|   [1]   | `ndarray`        | n-d array         | primary array container            |
|   [2]   | `dtype`          | type descriptor   | element type for array creation    |
|   [3]   | `ufunc`          | universal func    | element-wise C-dispatch operations |
|   [4]   | `matrix`         | 2-d array subtype | legacy 2-d multiplication surface  |
|   [5]   | `memmap`         | file-backed array | memory-mapped array I/O            |
|   [6]   | `recarray`       | record array      | field-named structured array       |
|   [7]   | `flatiter`       | flat iterator     | row-major element iteration        |
|   [8]   | `ndindex`        | index iterator    | multi-dimensional index scan       |
|   [9]   | `ndenumerate`    | enum iterator     | (index, value) iteration           |
|  [10]   | `nditer`         | advanced iterator | controlled multi-array iteration   |
|  [11]   | `broadcast`      | broadcast state   | broadcast shape computation        |
|  [12]   | `poly1d`         | polynomial        | polynomial arithmetic              |
|  [13]   | `busdaycalendar` | calendar          | business-day mask owner            |

[PUBLIC_TYPE_SCOPE]: dtype hierarchy
- rail: compute

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]    | [RAIL]                     |
| :-----: | :----------------------------- | :--------------- | :------------------------- |
|   [1]   | `generic`                      | scalar base      | root of all scalar types   |
|   [2]   | `bool_`                        | boolean          | numpy boolean scalar       |
|   [3]   | `int8`/`int16`/`int32`/`int64` | signed int       | fixed-width integers       |
|   [4]   | `uint8`..`uint64`              | unsigned int     | fixed-width unsigned ints  |
|   [5]   | `float16`/`float32`/`float64`  | float            | IEEE 754 float scalars     |
|   [6]   | `complex64`/`complex128`       | complex          | complex scalar types       |
|   [7]   | `datetime64`                   | datetime scalar  | ns-resolution datetime     |
|   [8]   | `timedelta64`                  | timedelta scalar | duration scalar            |
|   [9]   | `str_`                         | Unicode string   | fixed-width Unicode scalar |
|  [10]   | `bytes_`                       | byte string      | fixed-width bytes scalar   |

[PUBLIC_TYPE_SCOPE]: info and error types
- rail: compute

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [RAIL]                          |
| :-----: | :--------- | :-------------- | :------------------------------ |
|   [1]   | `finfo`    | float info      | float dtype precision metadata  |
|   [2]   | `iinfo`    | int info        | int dtype bounds metadata       |
|   [3]   | `errstate` | context manager | floating-point error mode scope |

[PUBLIC_TYPE_SCOPE]: submodule classes
- rail: compute

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [RAIL]                       |
| :-----: | :-------------------- | :--------------- | :--------------------------- |
|   [1]   | `linalg.LinAlgError`  | linear alg error | singular/non-converging ops  |
|   [2]   | `random.Generator`    | RNG generator    | stateful PRNG surface        |
|   [3]   | `random.BitGenerator` | bit source       | PRNG state engine base       |
|   [4]   | `random.PCG64`        | bit source       | PCG-64 PRNG engine           |
|   [5]   | `random.MT19937`      | bit source       | Mersenne-Twister engine      |
|   [6]   | `random.SeedSequence` | seed spawner     | reproducible seed derivation |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: array creation
- rail: compute

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------------------- | :------------- | :---------------------------- |
|   [1]   | `zeros(shape, dtype)`        | creation       | zero-filled array             |
|   [2]   | `ones(shape, dtype)`         | creation       | one-filled array              |
|   [3]   | `empty(shape, dtype)`        | creation       | uninitialized array           |
|   [4]   | `full(shape, fill_value)`    | creation       | constant-filled array         |
|   [5]   | `arange(start, stop, step)`  | range creation | evenly spaced integers/floats |
|   [6]   | `linspace(start, stop, num)` | range creation | evenly spaced floats          |
|   [7]   | `logspace(start, stop, num)` | range creation | log-spaced floats             |
|   [8]   | `eye(N, M, k)`               | creation       | identity / shifted diagonal   |
|   [9]   | `meshgrid(*xi)`              | creation       | coordinate grid from 1-D axes |
|  [10]   | `load(file)`                 | I/O            | load `.npy`/`.npz` from path  |
|  [11]   | `loadtxt(fname)`             | I/O            | load text-file array          |

[ENTRYPOINT_SCOPE]: shape and manipulation
- rail: compute

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------- | :------------- | :------------------------------ |
|   [1]   | `reshape(a, newshape)`      | shape          | change array shape              |
|   [2]   | `ravel(a)`                  | shape          | 1-D contiguous view             |
|   [3]   | `transpose(a, axes)`        | shape          | permute axes                    |
|   [4]   | `swapaxes(a, ax1, ax2)`     | shape          | swap two axes                   |
|   [5]   | `moveaxis(a, source, dest)` | shape          | move axis to new position       |
|   [6]   | `expand_dims(a, axis)`      | shape          | insert a new axis               |
|   [7]   | `squeeze(a, axis)`          | shape          | remove size-1 axes              |
|   [8]   | `concatenate(arrays, axis)` | join           | join arrays along existing axis |
|   [9]   | `stack(arrays, axis)`       | join           | stack arrays along new axis     |
|  [10]   | `hstack`/`vstack`/`dstack`  | join           | horizontal/vertical/depth join  |
|  [11]   | `split(a, indices, axis)`   | split          | split into sub-arrays           |

[ENTRYPOINT_SCOPE]: math and reduction
- rail: compute

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :---------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `sum`/`prod`/`cumsum`/`cumprod`     | reduction      | aggregation along axes            |
|   [2]   | `mean`/`std`/`var`                  | statistics     | mean, std dev, variance           |
|   [3]   | `min`/`max`/`argmin`/`argmax`       | reduction      | extrema and their indices         |
|   [4]   | `dot(a, b)`                         | linear alg     | vector/matrix dot product         |
|   [5]   | `matmul(x1, x2)`                    | linear alg     | matrix multiplication (`@`)       |
|   [6]   | `inner`/`outer`/`tensordot`         | linear alg     | inner, outer, tensor contractions |
|   [7]   | `where(cond, x, y)`                 | selection      | conditional element selection     |
|   [8]   | `clip(a, a_min, a_max)`             | elementwise    | bound values to interval          |
|   [9]   | `sort(a, axis)`/`argsort`           | sort           | sort or return sort indices       |
|  [10]   | `unique(ar)`                        | set ops        | unique elements with counts       |
|  [11]   | `exp`/`log`/`sqrt`/`power`          | ufunc          | exponential/logarithm/power       |
|  [12]   | `sin`/`cos`/`tan`                   | ufunc          | trigonometric operations          |
|  [13]   | `abs`/`sign`/`round`/`floor`/`ceil` | ufunc          | absolute, rounding operations     |

[ENTRYPOINT_SCOPE]: linalg submodule
- rail: compute

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY] | [RAIL]                       |
| :-----: | :-------------------------- | :------------- | :--------------------------- |
|   [1]   | `linalg.solve(a, b)`        | linear solver  | exact solution to `Ax = b`   |
|   [2]   | `linalg.inv(a)`             | inversion      | matrix inverse               |
|   [3]   | `linalg.det(a)`             | determinant    | matrix determinant           |
|   [4]   | `linalg.eig(a)`/`eigh(a)`   | eigen          | eigenvalues and eigenvectors |
|   [5]   | `linalg.svd(a)`/`svdvals`   | decomposition  | singular value decomposition |
|   [6]   | `linalg.qr(a)`              | decomposition  | QR factorisation             |
|   [7]   | `linalg.cholesky(a)`        | decomposition  | Cholesky factorisation       |
|   [8]   | `linalg.norm(x, ord, axis)` | norm           | vector and matrix norms      |
|   [9]   | `linalg.lstsq(a, b)`        | least squares  | minimum-norm least-squares   |
|  [10]   | `linalg.pinv(a)`            | pseudo-inverse | Moore-Penrose pseudo-inverse |

[ENTRYPOINT_SCOPE]: fft submodule
- rail: compute

| [INDEX] | [SURFACE]                  | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :------------------------- | :------------- | :----------------------------- |
|   [1]   | `fft.fft(a, n, axis)`      | transform      | 1-D complex DFT                |
|   [2]   | `fft.ifft(a, n, axis)`     | transform      | 1-D inverse DFT                |
|   [3]   | `fft.rfft`/`irfft`         | transform      | real-input 1-D FFT/inverse     |
|   [4]   | `fft.fft2`/`ifft2`         | transform      | 2-D DFT/inverse                |
|   [5]   | `fft.fftn`/`ifftn`         | transform      | N-D DFT/inverse                |
|   [6]   | `fft.fftfreq(n, d)`        | frequency      | DFT sample frequencies         |
|   [7]   | `fft.fftshift`/`ifftshift` | shift          | shift zero-frequency to center |

[ENTRYPOINT_SCOPE]: random.Generator
- rail: compute

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :----------------------------------- | :------------- | :-------------------------- |
|   [1]   | `random.default_rng(seed)`           | factory        | create default `Generator`  |
|   [2]   | `Generator.integers(low, high)`      | sampling       | discrete uniform integers   |
|   [3]   | `Generator.random(size)`             | sampling       | uniform floats [0, 1)       |
|   [4]   | `Generator.normal(loc, scale, size)` | sampling       | normal distribution         |
|   [5]   | `Generator.uniform(low, high, size)` | sampling       | continuous uniform          |
|   [6]   | `Generator.choice(a, size, replace)` | sampling       | random selection from array |
|   [7]   | `Generator.shuffle(x)`               | permutation    | in-place shuffle            |
|   [8]   | `Generator.permutation(x)`           | permutation    | permuted copy               |

## [4]-[IMPLEMENTATION_LAW]

[COMPUTE_TOPOLOGY]:
- namespaces: `numpy`, `numpy.linalg`, `numpy.fft`, `numpy.random`, `numpy.ma`, `numpy.polynomial`, `numpy.testing`
- `ndarray` is the universal container; all array functions accept array-like inputs and return `ndarray` unless documented otherwise
- ufuncs dispatch at the C layer and support `out`, `axis`, `keepdims`, `where`, and `casting` kwargs
- broadcasting: shapes align right-to-left; size-1 axes stretch; incompatible non-1 sizes raise `ValueError`
- indexing: integer, boolean mask, fancy (array of indices), and slice indexing all produce views or copies depending on contiguity
- random Generator is the preferred API; `numpy.random.RandomState` is legacy and non-reentrant

[LOCAL_ADMISSION]:
- Array construction always specifies `dtype` when precision matters; default float is `float64`.
- `random.default_rng(seed)` is the single entry point for reproducible sampling; never use module-level random functions.
- `linalg` operations accept real and complex arrays; check `eigh` vs `eig` for Hermitian shortcuts.
- `fft` frequency bins follow NumPy convention: `fftfreq(n, d)` returns cycles per `d` unit.

[RAIL_LAW]:
- Package: `numpy`
- Owns: N-d array construction, dtype algebra, ufunc dispatch, linalg, fft, and random sampling
- Accept: `ndarray`-first APIs, explicit `dtype`, `random.default_rng` seeding, `linalg`/`fft`/`random` submodule usage
- Reject: hand-rolled numerical loops replaceable by ufuncs, module-level `numpy.random` functions, `numpy.matrix` for new code
