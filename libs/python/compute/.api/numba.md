# [PY_COMPUTE_API_NUMBA]

`numba` supplies LLVM-backed JIT compilation of NumPy-typed Python kernels for the compute accelerator rail. The package owner compiles a hot numeric-study kernel to native code through `njit`, builds NumPy ufuncs through `vectorize`/`guvectorize`, and drives parallel loops with `prange`; it never re-implements the JIT pipeline numba owns. Decorating a function yields a `CPUDispatcher` carrying `signatures`, `inspect_types`, `inspect_asm`, and `parallel_diagnostics`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numba`
- package: `numba`
- import: `numba`; submodules `numba.types`, `numba.typed`, `numba.extending`, `numba.experimental`
- owner: `compute`
- version: `0.65.1`
- license: BSD-2-Clause
- rail: accelerator
- capability: LLVM JIT of NumPy-typed functions in nopython and object modes, automatic parallelization, threadpool/threading-layer control, scalar-to-ufunc lifting, C-callback emission, and `jitclass`/`structref`/`overload` extension

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

[ENTRYPOINT_SCOPE]: compilation decorators — every JIT decorator accepts `cache`, `parallel`, `fastmath`, `boundscheck`, `nogil` options
- rail: accelerator

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY]  | [CAPABILITY]                                       |
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
- rail: accelerator

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]       | [CAPABILITY]                                         |
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

[EXTENSION_SURFACES]: the `numba.extending`/`numba.experimental` registrars that teach nopython about foreign types.
- [01]-[OVERLOAD]: `extending.overload(fn)` / `overload_method(typ, name)` / `overload_attribute(typ, name)` — register nopython implementations for a function, method, or attribute.
- [02]-[INTRINSIC]: `extending.intrinsic(fn)` — declares a low-level typed code-generation primitive.
- [03]-[JITABLE]: `extending.register_jitable(fn)` — marks a pure-Python helper callable from nopython.
- [04]-[TYPE_MAPPING]: `extending.as_numba_type(py_type)` / `typeof_impl.register` — maps a Python/annotation type to a numba type; teaches `typeof`.
- [05]-[NATIVE_MODEL]: `extending.box` / `unbox` / `models` / `make_attribute_wrapper` / `lower_builtin` / `type_callable` — full data-model extension to expose a foreign type to nopython.
- [06]-[STRUCT_REF]: `experimental.structref.register` / `StructRefProxy` / `define_proxy` / `define_boxing` — mutable, reference-semantics struct type for nopython (the `jitclass` alternative).

## [04]-[IMPLEMENTATION_LAW]

[DISPATCHER_TOPOLOGY]:
- A decorated kernel becomes a `CPUDispatcher`; calling it specializes and caches one compiled signature per argument-type tuple. `signatures`/`nopython_signatures` list the compiled specializations; `overloads` maps signature → compile result; `py_func` recovers the uncompiled original.
- Evidence capture: `inspect_types()`, `inspect_asm()`, `inspect_llvm()`, `inspect_cfg()`, and `get_metadata()` expose the lowered Python-typed source, native assembly, LLVM IR, control-flow graph, and compile metadata respectively — capture these as the study receipt rather than a bare timing.
- Lifecycle control: `recompile()` forces recompilation; `disable_compile()` freezes the dispatcher to its cached specializations; `enable_caching()` arms the on-disk cache.
- `parallel=True` plus `prange` enables auto-parallelization; `parallel_diagnostics()` reports loop fusion, allocation hoisting, and parallel-region decisions — the canonical parallelization-evidence source.
- `cache=True` persists compiled code to an on-disk cache keyed by source hash (requires a real source file, not `<string>`); `fastmath=True` relaxes IEEE ordering for speed; `boundscheck=True` adds array-bounds checks; `nogil=True` releases the GIL for the compiled region.
- `njit` is `jit` with `nopython=True`; object-mode fallback exists only on `jit` and is the slow path. `cfunc` yields a C-callable artifact carrying `ctypes`, `address`, and `native_name` — the bridge into `scipy.LowLevelCallable` and ctypes FFI. `vectorize` yields a `DUFunc` (a true NumPy ufunc) that broadcasts, reduces, and accumulates like any ufunc.

[STUDY_ROUTING]:
- A `NumericIntent` kernel marked for the LLVM-JIT accelerator row compiles through `njit`; parallel kernels add `prange` under `parallel=True`.
- The accelerator row captures the compile mode, the resolved signature, and the speedup class as study evidence; no production runtime depends on numba.
- Typed containers (`typed.List`, `typed.Dict`) cross the nopython boundary; reflected Python lists and dicts do not.

[INTEGRATION_TOPOLOGY]:
- `numpy`: the type substrate: `from_dtype`/`typeof` map NumPy dtypes to numba types; `vectorize`/`guvectorize` emit real `numpy` ufuncs/gufuncs that compose with broadcasting and `np.einsum`-shaped reductions.
- `scipy`: a `cfunc`-compiled kernel's `.ctypes`/`.address` feeds `scipy.LowLevelCallable`, so a numba kernel drops directly into `scipy.integrate.quad`/`scipy.ndimage` C callbacks with zero Python-call overhead per sample.
- `mpmath`: the validation oracle for an `njit` kernel: run the compiled fast path, then `mpmath.almosteq` against a high-`dps` reference to certify the speedup did not cost accuracy.
- `extending`: when an admitted domain type must enter nopython, register it via `as_numba_type`/`box`/`unbox`/`models` rather than copying its fields into a numba-native shape.

[RAIL_LAW]:
- Package: `numba`
- Owns: offline LLVM-JIT acceleration of numeric-study kernels (the `NumericIntent` LLVM-JIT row), ufunc/gufunc emission, and C-callback bridging
- Accept: a study kernel compiled through `njit`, with the compiled signature, lowered IR (`inspect_*`), and speedup class captured as a study receipt
