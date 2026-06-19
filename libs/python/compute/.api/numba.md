# [PY_COMPUTE_API_NUMBA]

`numba` supplies LLVM-backed JIT compilation of NumPy-typed Python kernels for the compute accelerator rail. The package owner compiles a hot numeric-study kernel to native code through `njit`, builds NumPy ufuncs through `vectorize`/`guvectorize`, and drives parallel loops with `prange`; it never re-implements the JIT pipeline numba owns. Decorating a function yields a `CPUDispatcher` carrying `signatures`, `inspect_types`, `inspect_asm`, and `parallel_diagnostics`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numba`
- package: `numba`
- import: `numba`; submodules `numba.types`, `numba.typed`, `numba.extending`, `numba.experimental`
- owner: `compute`
- rail: accelerator
- installed: cp313 only (manifest pin `numba>=0.65.1; python_version<'3.15'`; llvmlite ships no cp315 wheel)
- capability: LLVM JIT of NumPy-typed functions in nopython and object modes, automatic parallelization, threadpool control, scalar-to-ufunc lifting, and `jitclass`/`overload` extension

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiled artifacts and numba type vocabulary
- rail: accelerator

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE] | [CAPABILITY]                                           |
| :-----: | :------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `numba.core.types.Array`        | array type     | typed n-D array spec `Array(dtype, ndim, layout)`      |
|  [02]   | `numba.types`                   | type namespace | scalar/array/optional/deferred type constructors       |
|  [03]   | `numba.typed.List`              | typed list     | reflected-free list with `empty_list(item_type)`       |
|  [04]   | `numba.typed.Dict`              | typed dict     | reflected-free dict with `empty(key_type, value_type)` |
|  [05]   | `numba.optional`                | optional type  | nullable wrapper over a base type                      |
|  [06]   | `numba.deferred_type`           | forward type   | recursive/forward type reference                       |
|  [07]   | `numba.NumbaError`              | error root     | base for all numba compile and type errors             |
|  [08]   | `numba.TypingError`             | typing error   | nopython type-resolution failure                       |
|  [09]   | `numba.UnsupportedError`        | lowering error | unsupported Python construct in nopython               |
|  [10]   | `numba.NumbaPerformanceWarning` | perf warning   | parallel/fastmath performance advisory                 |

[PUBLIC_TYPE_SCOPE]: scalar type aliases (`numba.<alias>`)
- rail: accelerator

| [INDEX] | [INTEGER]         | [FLOAT_COMPLEX]     | [SHORT_ALIAS]   | [WIDTH]                         |
| :-----: | :---------------- | :------------------ | :-------------- | :------------------------------ |
|  [01]   | `int8`            | `float32`           | `i1` / `f4`     | 8-bit int / 32-bit float        |
|  [02]   | `int16`           | `float64`           | `i2` / `f8`     | 16-bit int / 64-bit float       |
|  [03]   | `int32`           | `complex64`         | `i4` / `c8`     | 32-bit int / 64-bit complex     |
|  [04]   | `int64`           | `complex128`        | `i8` / `c16`    | 64-bit int / 128-bit complex    |
|  [05]   | `uint8`..`uint64` | `double`            | `u1`..`u8`      | unsigned int / alias of float64 |
|  [06]   | `intp` / `uintp`  | `boolean` / `bool_` | `b1`            | pointer-width int / bool        |
|  [07]   | `intc` / `size_t` | `void` / `none`     | `byte` / `char` | C int / void return / byte      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compilation decorators
- rail: accelerator

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY]  | [CAPABILITY]                                                                 |
| :-----: | :---------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `njit(*args, **kws)`                                                                | nopython JIT    | `jit(nopython=True)`; accepts `cache`, `parallel`, `fastmath`, `boundscheck` |
|  [02]   | `jit(signature_or_function, locals, cache, pipeline_class, boundscheck, **options)` | JIT             | compiles with optional object-mode and explicit signatures                   |
|  [03]   | `cfunc(sig, locals, cache, pipeline_class, **options)`                              | C callback      | compiles a function to a C-callable function pointer                         |
|  [04]   | `vectorize(ftylist_or_function, **kws)`                                             | ufunc builder   | lifts a scalar kernel into a NumPy ufunc; `target='cpu'\|'parallel'`         |
|  [05]   | `guvectorize(*args, **kwargs)`                                                      | gufunc builder  | builds a generalized ufunc from a layout signature                           |
|  [06]   | `stencil(func_or_mode, **options)`                                                  | stencil builder | compiles a fixed-window neighborhood kernel                                  |
|  [07]   | `experimental.jitclass`                                                             | class JIT       | compiles a class with typed instance fields to native layout                 |
|  [08]   | `jit_module(**kwargs)`                                                              | module JIT      | applies `jit` to every top-level function in the calling module              |

[ENTRYPOINT_SCOPE]: parallel, runtime, and extension surfaces
- rail: accelerator

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]     | [CAPABILITY]                                               |
| :-----: | :------------------------------------ | :----------------- | :--------------------------------------------------------- |
|  [01]   | `prange(*args)`                       | parallel range     | parallel loop range honored under `parallel=True`          |
|  [02]   | `get_num_threads()`                   | thread query       | active numba thread count                                  |
|  [03]   | `set_num_threads(n)`                  | thread control     | bounds the numba threadpool                                |
|  [04]   | `parallel_chunksize(n)`               | chunk control      | context manager setting parallel chunk size                |
|  [05]   | `objmode(**vars)`                     | escape hatch       | runs a Python block in object mode inside nopython         |
|  [06]   | `literally(x)` / `literal_unroll(it)` | literal force      | forces literal typing / unrolls a heterogeneous tuple loop |
|  [07]   | `typeof(x)` / `from_dtype(dt)`        | type reflection    | infers a numba type from a value or NumPy dtype            |
|  [08]   | `extending.overload(fn)`              | overload registrar | registers a nopython implementation for `fn`               |
|  [09]   | `extending.intrinsic(fn)`             | intrinsic          | declares a low-level typed code-generation primitive       |
|  [10]   | `extending.register_jitable(fn)`      | jitable mark       | marks a pure-Python helper callable from nopython          |

## [04]-[IMPLEMENTATION_LAW]

[DISPATCHER_TOPOLOGY]:
- A decorated kernel becomes a `CPUDispatcher`; calling it specializes and caches one compiled signature per argument-type tuple.
- `signatures` lists the compiled specializations; `inspect_types`, `inspect_asm`, and `inspect_llvm` expose the lowered IR for evidence capture.
- `parallel=True` plus `prange` enables auto-parallelization; `parallel_diagnostics()` reports fusion, allocation hoisting, and parallel-region decisions.
- `cache=True` persists compiled code to an on-disk cache keyed by source hash; `fastmath=True` relaxes IEEE ordering for speed.
- `njit` is `jit` with `nopython=True`; object-mode fallback exists only on `jit` and is the slow path.

[STUDY_ROUTING]:
- A `NumericIntent` kernel marked for the LLVM-JIT accelerator row compiles through `njit`; parallel kernels add `prange` under `parallel=True`.
- The accelerator row captures the compile mode, the resolved signature, and the speedup class as study evidence; no production runtime depends on numba.
- Typed containers (`typed.List`, `typed.Dict`) cross the nopython boundary; reflected Python lists and dicts do not.

[RAIL_LAW]:
- Package: `numba`
- Owns: offline LLVM-JIT acceleration of numeric-study kernels (the `NumericIntent` LLVM-JIT row)
- Accept: a study kernel compiled through `njit`, with the compiled signature and speedup class captured as a study receipt
- Reject: numba in any product runtime import path; wrapper-renames of the JIT decorators; accelerator claims without a study receipt
