# [PY_COMPUTE_API_NUMBA]

`numba` owns LLVM-backed JIT compilation of NumPy-typed Python kernels for the compute accelerator rail, lowering a decorated kernel to native machine code and lifting scalar functions to NumPy ufuncs and gufuncs. Compilation is offline study evidence; no production runtime binds numba.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numba`
- package: `numba` (BSD-2-Clause, Anaconda Inc.)
- module: `numba`; submodules `numba.types`, `numba.typed`, `numba.extending`, `numba.experimental`
- rail: accelerator — offline LLVM JIT of numeric-study kernels

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiled-artifact and numba type vocabulary

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [CAPABILITY]                                           |
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

[ENTRYPOINT_SCOPE]: compilation decorators — each JIT decorator carries `cache`, `parallel`, `fastmath`, `boundscheck`, `nogil`

| [INDEX] | [SURFACE]                                                       | [SHAPE]         | [CAPABILITY]                                       |
| :-----: | :-------------------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `njit(*args, **kws)`                                            | nopython JIT    | `jit(nopython=True)`                               |
|  [02]   | `jit(signature_or_function, locals, pipeline_class, **options)` | JIT             | object-mode fallback + explicit signatures         |
|  [03]   | `cfunc(sig, locals, cache, pipeline_class, **options)`          | C callback      | compiles to a C-callable function pointer          |
|  [04]   | `vectorize(ftylist_or_function, **kws)`                         | ufunc builder   | scalar → NumPy ufunc; `target='cpu'\|'parallel'`   |
|  [05]   | `guvectorize(*args, **kwargs)`                                  | gufunc builder  | builds a generalized ufunc from a layout signature |
|  [06]   | `stencil(func_or_mode, **options)`                              | stencil builder | compiles a fixed-window neighborhood kernel        |
|  [07]   | `experimental.jitclass`                                         | class JIT       | compiles a typed-field class to native layout      |
|  [08]   | `jit_module(**kwargs)`                                          | module JIT      | applies `jit` to every module-level function       |

[ENTRYPOINT_SCOPE]: parallel and runtime surfaces

| [INDEX] | [SURFACE]                                                | [SHAPE]              | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------- | :------------------- | :--------------------------------------------------- |
|  [01]   | `prange(*args)`                                          | parallel range       | parallel loop range honored under `parallel=True`    |
|  [02]   | `get_num_threads()`                                      | thread query         | active numba thread count                            |
|  [03]   | `set_num_threads(n)`                                     | thread control       | bounds the numba threadpool                          |
|  [04]   | `parallel_chunksize(n)`                                  | chunk control        | context manager setting parallel chunk size          |
|  [05]   | `objmode(**vars)`                                        | escape hatch         | runs a Python block in object mode inside nopython   |
|  [06]   | `literally(x)` / `literal_unroll(it)`                    | literal force        | literal typing / unroll a heterogeneous tuple loop   |
|  [07]   | `typeof(x)` / `from_dtype(dt)`                           | type reflection      | infers a numba type from a value or NumPy dtype      |
|  [08]   | `get_thread_id()` / `threading_layer()`                  | thread introspection | worker thread id / backend (`tbb`/`omp`/`workqueue`) |
|  [09]   | `get_parallel_chunksize()` / `set_parallel_chunksize(n)` | chunk query/set      | read/set parallel chunk size outside a context mgr   |
|  [10]   | `carray(ptr, shape, dtype=None)` / `farray(...)`         | FFI bridge           | raw C pointer → C-/F-ordered NumPy array in nopython |
|  [11]   | `gdb()` / `gdb_init(...)` / `gdb_breakpoint()`           | debug                | attach gdb to a running compiled kernel              |

[EXTENSION_SURFACES]: `numba.extending`/`numba.experimental` registrars that teach nopython a foreign type.
- [01]-[OVERLOAD]: `extending.overload(fn)` / `overload_method(typ, name)` / `overload_attribute(typ, name)` — nopython implementations for a function, method, or attribute.
- [02]-[INTRINSIC]: `extending.intrinsic(fn)` — low-level typed code-generation primitive.
- [03]-[JITABLE]: `extending.register_jitable(fn)` — marks a pure-Python helper callable from nopython.
- [04]-[TYPE_MAPPING]: `extending.as_numba_type.register(py_type)` / `typeof_impl.register(cls)` — maps a Python/annotation type to a numba type and teaches `typeof`.
- [05]-[NATIVE_MODEL]: `extending.box` / `unbox` / `models` / `make_attribute_wrapper` / `lower_builtin` / `type_callable` — full data-model extension exposing a foreign type to nopython.
- [06]-[STRUCT_REF]: `experimental.structref.register` / `StructRefProxy` / `define_proxy` / `define_boxing` — mutable reference-semantics struct type for nopython.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A decorated kernel becomes a `CPUDispatcher` specializing and caching one compiled signature per argument-type tuple; `signatures`/`nopython_signatures`/`overloads` expose the specialization set and `py_func` recovers the uncompiled original. `njit` is `jit(nopython=True)`; object-mode fallback rides only `jit` as the slow path.
- `cache=True` persists compiled code to an on-disk cache keyed by source hash (a real source file, not `<string>`); `fastmath=True` relaxes IEEE ordering; `boundscheck=True` adds array-bounds checks; `nogil=True` releases the GIL for the compiled region; `parallel=True` auto-parallelizes `prange` loops.

[STACKING]:
- `numpy`(`.api/numpy.md`): `from_dtype`/`typeof` map NumPy dtypes to numba types; `vectorize`/`guvectorize` emit real `numpy` ufuncs/gufuncs that broadcast, reduce, and accumulate.
- `scipy`(`.api/scipy.md`): a `cfunc` kernel's `.ctypes`/`.address` feeds `scipy.LowLevelCallable` into `scipy.integrate.quad`/`scipy.ndimage` with zero per-sample Python overhead.
- `mpmath`(`.api/mpmath.md`): the accuracy oracle — run the `njit` fast path, then `mpmath.almosteq` against a high-`dps` reference to certify the speedup held precision.
- within-lib: `inspect_types`/`inspect_asm`/`inspect_llvm`/`inspect_cfg`/`get_metadata` capture the lowered source, native assembly, LLVM IR, CFG, and compile metadata as the study receipt; `parallel_diagnostics()` reports loop fusion, allocation hoisting, and parallel-region decisions; `cfunc` yields `ctypes`/`address`/`native_name`, `vectorize` yields a `DUFunc`, and `recompile`/`disable_compile`/`enable_caching` drive dispatcher lifecycle.
- within-lib: a domain type enters nopython through `extending.as_numba_type`/`box`/`unbox`/`models`, materializing a native data model rather than a field copy into a numba-native shape.

[LOCAL_ADMISSION]:
- A `NumericIntent` kernel on the LLVM-JIT accelerator row compiles through `njit`, parallel kernels adding `prange` under `parallel=True`; the row captures compile mode, resolved signature, lowered IR (`inspect_*`), and speedup class as study evidence. Typed containers (`typed.List`, `typed.Dict`) cross the nopython boundary; reflected Python lists and dicts do not.

[RAIL_LAW]:
- Package: `numba`
- Owns: offline LLVM-JIT acceleration of numeric-study kernels, ufunc/gufunc emission, and C-callback bridging
- Accept: a study kernel compiled through `njit` with its compiled signature, lowered IR (`inspect_*`), and speedup class captured as a receipt
- Reject: a hand-written C extension or a re-rolled JIT loop where `njit` plus `prange` lowers the same kernel
