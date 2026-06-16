# [PY_COMPUTE_API_NUMPY]

`numpy` supplies the n-dimensional array, dtype system, and vectorized numeric primitives that back the compute array-admission rail. The package owner admits a scientific payload into an explicit dtype/shape/finite-policy record and is the substrate every other compute rail (scipy, xarray, dask, sympy lambdify) layers on; it never re-implements an array primitive numpy owns. Member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpy`
- package: `numpy`
- import: `numpy`
- owner: `compute`
- rail: arrays
- installed: ABSENT on cp315 (requires-python `>=3.15`, no cp315 wheel; sdist build blocked by manifest gaps 1+2)
- capability: n-dimensional array container, dtype system, broadcasting, linear algebra (`numpy.linalg`), random generation (`numpy.random`), and the buffer protocol backing the whole scientific stack

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: array owners
- rail: arrays

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
| :-----: | :------- | :------------- | :----------- |
| [1] | `numpy.ndarray` | array container | strided n-dimensional buffer |
| [2] | `numpy.dtype` | type descriptor | element type and itemsize |
| [3] | `numpy.linalg` | linear algebra submodule | dense factorizations and solve |
| [4] | `numpy.random` | random submodule | Generator-based random sampling |
| [5] | `numpy.typing.NDArray` | typing alias | shape/dtype-parametric array annotation |

[ENTRYPOINTS]:
- UN_REFLECTED: exact callable spellings (`numpy.asarray`, `numpy.array`, `numpy.isfinite`, `numpy.ascontiguousarray`, `numpy.linalg.solve`, `numpy.random.default_rng`) and verified signatures require a reflectable install to capture; type/submodule names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- admission: a scientific payload enters as an `ndarray` with an explicit `dtype`, captured shape, memory-layout fact, and finite policy; the array-admission record carries these plus payload identity.
- substrate: numpy is the shared buffer every other compute rail consumes; scipy/xarray/dask/sympy-lambdify layer on the same `ndarray`.
- boundary: array admission is offline evidence; no product tensor allocation or model session enters here.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `numpy`
- Owns: n-dimensional array admission, dtype/shape/finite policy, and the numeric substrate for every compute rail
- Accept: a payload admitted to an `ndarray` with an explicit dtype, shape, layout, and finite-policy record
- Reject: hand-rolled buffer math; wrapper-renames of array constructors; product tensor allocation
