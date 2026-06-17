# [PY_COMPUTE_API_JAX]

`jax` supplies XLA-compiled, autodiff-capable array computation for the compute accelerator rail. The package owner compiles and differentiates numeric-study kernels as the `NumericIntent` XLA accelerator row; it never re-implements an autodiff or XLA pipeline jax owns. The distribution is marker-gated `python_version<'3.15'` (jaxlib ships no cp315 wheel) and is absent from the cp315 lock; member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jax`
- package: `jax` (runtime `jaxlib`)
- import: `jax`; submodule `jax.numpy`
- owner: `compute`
- rail: accelerator
- installed: ABSENT on cp315 (manifest pin `jax>=0.10.1; python_version<'3.15'`; jaxlib ships no cp315 wheel — manifest gaps 1+2)
- capability: XLA-compiled array computation with composable transforms — JIT, automatic differentiation, vectorization, and parallel mapping

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: transform owners
- rail: accelerator

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE]        | [CAPABILITY]                         |
| :-----: | :---------- | :-------------------- | :----------------------------------- |
|   [1]   | `jax.jit`   | XLA compile           | compiles a function through XLA      |
|   [2]   | `jax.grad`  | reverse-mode autodiff | gradient of a scalar-valued function |
|   [3]   | `jax.vmap`  | vectorizing map       | auto-vectorizes over a batch axis    |
|   [4]   | `jax.pmap`  | parallel map          | maps across devices                  |
|   [5]   | `jax.numpy` | array namespace       | NumPy-compatible array API on XLA    |

[ENTRYPOINTS]:
- UN_REFLECTED: exact transform keyword spellings (`jit(static_argnums=, donate_argnums=)`, `grad(argnums=)`) and verified signatures require a reflectable install to capture; transform names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- routing: a `NumericIntent` kernel marked for the XLA accelerator row compiles through `jax.jit`; gradient-bearing studies use `jax.grad`; batched studies use `jax.vmap`.
- evidence: the accelerator row captures the transform stack, the device class, and the speedup/precision class as study evidence; no production runtime depends on jax.
- boundary: jax kernels are offline accelerator experiments; production accelerator/substrate selection stays in `Rasm.Compute`.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `jax`
- Owns: offline XLA acceleration and autodiff of numeric-study kernels (the `NumericIntent` XLA row)
- Accept: a study kernel compiled through `jax.jit` or differentiated through `jax.grad` with a captured class
- Reject: jax in any product runtime import path; wrapper-renames of the transforms; accelerator claims without a study receipt
