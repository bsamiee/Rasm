# [PY_COMPUTE_API_NUMBA]

`numba` supplies LLVM-backed JIT compilation of NumPy-typed Python kernels for the compute accelerator rail. The package owner compiles hot numeric-study kernels to native code as the `NumericIntent` LLVM-JIT accelerator row; it never re-implements a JIT pipeline numba owns. The distribution is marker-gated `python_version<'3.15'` and is absent from the cp315 lock; member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numba`
- package: `numba`
- import: `numba`
- owner: `compute`
- rail: accelerator
- installed: ABSENT on cp315 (manifest pin `numba>=0.65.1; python_version<'3.15'`; numba lags CPython, no cp315 wheel — manifest gaps 1+2)
- capability: LLVM JIT compilation of NumPy-typed Python functions; nopython and parallel execution modes

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: JIT owners
- rail: accelerator

| [INDEX] | [SYMBOL]            | [PACKAGE_ROLE] | [CAPABILITY]                              |
| :-----: | :------------------ | :------------- | :---------------------------------------- |
|   [1]   | `numba.njit`        | nopython JIT   | compiles a kernel in nopython mode        |
|   [2]   | `numba.jit`         | JIT decorator  | compiles with object-mode fallback        |
|   [3]   | `numba.vectorize`   | ufunc builder  | builds a NumPy ufunc from a scalar kernel |
|   [4]   | `numba.guvectorize` | gufunc builder | builds a generalized ufunc                |
|   [5]   | `numba.prange`      | parallel range | parallel loop range for `parallel=True`   |

[ENTRYPOINTS]:
- UN_REFLECTED: exact decorator keyword spellings (`njit(cache=, parallel=, fastmath=)`) and verified signatures require a reflectable install to capture; decorator names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- routing: a `NumericIntent` kernel marked for the LLVM-JIT accelerator row compiles through `njit`; parallel kernels use `prange` under `parallel=True`.
- evidence: the accelerator row captures the compile mode, the kernel signature, and the speedup class as study evidence; no production runtime depends on numba.
- boundary: numba kernels are offline accelerator experiments; production JIT/substrate selection stays in `Rasm.Compute`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `numba`
- Owns: offline LLVM-JIT acceleration of numeric-study kernels (the `NumericIntent` LLVM-JIT row)
- Accept: a study kernel compiled through `njit` with a captured speedup class
- Reject: numba in any product runtime import path; wrapper-renames of the JIT decorators; accelerator claims without a study receipt
