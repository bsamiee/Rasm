# [PY_COMPUTE_API_JAX]

`jax` supplies XLA-compiled, autodiff-capable array computation with composable function transforms for the compute accelerator rail. The package owner compiles, differentiates, vectorizes, and parallelizes numeric-study kernels through `jit`, `grad`, `vmap`, and `pmap`; it never re-implements an autodiff or XLA pipeline jax owns. The distribution is marker-gated `python_version<'3.15'` because jaxlib ships no cp315 wheel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jax`
- package: `jax` (runtime `jaxlib`)
- import: `jax`; submodules `jax.numpy`, `jax.lax`, `jax.random`, `jax.nn`, `jax.scipy`, `jax.tree_util`
- owner: `compute`
- rail: accelerator
- installed: marker-gated `python_version<'3.15'` (`jax>=0.10.1`; jaxlib ships no cp315 wheel)
- capability: XLA-compiled array computation with composable transforms — JIT compilation, forward/reverse-mode autodiff, automatic vectorization, device-parallel mapping, and structured control-flow primitives

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array, device, and sharding types
- rail: accelerator

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [CAPABILITY]                                        |
| :-----: | :----------------- | :--------------- | :-------------------------------------------------- |
|  [01]   | `Array`            | array type       | unified on-device array type                        |
|  [02]   | `ShapeDtypeStruct` | shape spec       | abstract shape/dtype for `eval_shape` and tracing   |
|  [03]   | `Device`           | device handle    | a single addressable XLA device                     |
|  [04]   | `NamedSharding`    | sharding spec    | mesh-and-partition-spec array sharding              |
|  [05]   | `P`                | partition spec   | `PartitionSpec` for `shard_map` and `NamedSharding` |
|  [06]   | `Shard`            | shard view       | one device's slice of a global array                |
|  [07]   | `custom_jvp`       | custom-rule type | user-defined forward-mode differentiation rule      |
|  [08]   | `custom_vjp`       | custom-rule type | user-defined reverse-mode differentiation rule      |

[PUBLIC_TYPE_SCOPE]: submodule namespaces
- rail: accelerator

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :----------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `jax.numpy`        | array namespace | NumPy-compatible array API on XLA                |
|  [02]   | `jax.lax`          | primitive ops   | low-level XLA primitives and control flow        |
|  [03]   | `jax.random`       | PRNG namespace  | splittable counter-based random generation       |
|  [04]   | `jax.nn`           | NN primitives   | activations and neural-network helpers           |
|  [05]   | `jax.scipy`        | SciPy on XLA    | XLA-backed subset of the SciPy API               |
|  [06]   | `jax.tree_util`    | pytree ops      | pytree flatten, map, and node registration       |
|  [07]   | `jax.sharding`     | sharding API    | `Mesh`, `Sharding`, and partition specifications |
|  [08]   | `jax.experimental` | staging area    | sparse, ODE, and pallas experimental APIs        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compilation and differentiation transforms
- rail: accelerator

| [INDEX] | [SURFACE]                                                                                   | [ENTRY_FAMILY]    | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------------------ | :---------------- | :------------------------------------- |
|  [01]   | `jit(fun, *, static_argnums, static_argnames, donate_argnums, in_shardings, out_shardings)` | compile           | XLA compilation of a function          |
|  [02]   | `grad(fun, argnums=0, has_aux=False, holomorphic=False, allow_int=False)`                   | reverse autodiff  | gradient of a scalar-valued function   |
|  [03]   | `value_and_grad(fun, argnums=0, has_aux=False)`                                             | reverse autodiff  | value and gradient in one pass         |
|  [04]   | `jacfwd(fun, argnums=0, has_aux=False, holomorphic=False)`                                  | forward autodiff  | forward-mode Jacobian                  |
|  [05]   | `jacrev(fun, argnums=0, has_aux=False, allow_int=False)`                                    | reverse autodiff  | reverse-mode Jacobian                  |
|  [06]   | `hessian(fun, argnums=0, has_aux=False)`                                                    | second-order      | Hessian via `jacfwd(jacrev(...))`      |
|  [07]   | `jvp(fun, primals, tangents, has_aux=False)`                                                | forward autodiff  | Jacobian-vector product                |
|  [08]   | `vjp(fun, *primals, has_aux=False)`                                                         | reverse autodiff  | vector-Jacobian product closure        |
|  [09]   | `linearize(fun, *primals, has_aux=False)`                                                   | linearization     | value plus linear tangent map          |
|  [10]   | `checkpoint(fun, *, prevent_cse=True, policy, static_argnums)`                              | rematerialization | gradient checkpointing (alias `remat`) |
|  [11]   | `custom_jvp(fun, nondiff_argnums=())`                                                       | custom rule       | attaches a custom JVP rule             |
|  [12]   | `custom_vjp(fun, nondiff_argnums=())`                                                       | custom rule       | attaches a custom VJP rule             |

[ENTRYPOINT_SCOPE]: vectorization, parallelism, and staging
- rail: accelerator

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `vmap(fun, in_axes=0, out_axes=0, axis_name=None, axis_size=None)`      | vectorize       | auto-vectorize over a batch axis          |
|  [02]   | `pmap(fun, axis_name=None, *, in_axes=0, out_axes=0, devices=None)`     | device parallel | SPMD map across devices                   |
|  [03]   | `shard_map(f, *, out_specs, in_specs, mesh, axis_names)`                | manual SPMD     | explicit per-shard computation            |
|  [04]   | `eval_shape(fun, *args, **kwargs)`                                      | shape inference | output `ShapeDtypeStruct` without compute |
|  [05]   | `make_jaxpr(fun, static_argnums=(), return_shape=False)`                | tracing         | traces a function to a jaxpr              |
|  [06]   | `closure_convert(fun, *example_args)`                                   | tracing         | converts captured constants to arguments  |
|  [07]   | `pure_callback(callback, result_shape_dtypes, *args, vmap_method=None)` | host callback   | pure Python callback inside a transform   |
|  [08]   | `disable_jit(disable=True)`                                             | tracing control | context manager disabling JIT             |

[ENTRYPOINT_SCOPE]: device placement, PRNG, and control flow
- rail: accelerator

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------ | :-------------- | :-------------------------------------- |
|  [01]   | `device_put(x, device=None, *, donate=False)`     | placement       | move an array to a device or sharding   |
|  [02]   | `device_get(x)`                                   | placement       | copy an array back to host NumPy        |
|  [03]   | `block_until_ready(x)`                            | synchronization | block until async computation completes |
|  [04]   | `devices(backend=None)` / `device_count(backend)` | discovery       | enumerate and count addressable devices |
|  [05]   | `random.key(seed)` / `random.PRNGKey(seed)`       | PRNG key        | construct a splittable PRNG key         |
|  [06]   | `random.split(key, num=2)`                        | PRNG split      | derive independent subkeys              |
|  [07]   | `random.normal(key, shape, dtype)`                | PRNG sample     | standard-normal samples                 |
|  [08]   | `lax.scan(f, init, xs, length, reverse, unroll)`  | control flow    | compiled loop with carried state        |
|  [09]   | `lax.fori_loop(lower, upper, body_fun, init_val)` | control flow    | compiled counted loop                   |
|  [10]   | `lax.cond(pred, true_fun, false_fun, *operands)`  | control flow    | compiled conditional branch             |
|  [11]   | `lax.while_loop(cond_fun, body_fun, init_val)`    | control flow    | compiled while loop                     |
|  [12]   | `tree_util.tree_map(f, tree, *rest)`              | pytree map      | map a function over pytree leaves       |

## [04]-[IMPLEMENTATION_LAW]

[TRANSFORM_TOPOLOGY]:
- namespace: `jax`; transforms and types at top level, array and primitive ops under submodules
- transforms compose: `jit(grad(vmap(fun)))` is the canonical compile-differentiate-vectorize stack
- purity: transformed functions must be pure; side effects route through `pure_callback` or `jax.debug`
- PRNG: keys are explicit, immutable, and splittable; reuse a key never, `split` before sampling always
- control flow inside transforms uses `lax.scan`, `lax.fori_loop`, `lax.cond`, and `lax.while_loop`, not Python loops over traced values
- `static_argnums` and `static_argnames` mark JIT arguments as compile-time constants; changing them retraces
- shapes are static at trace time; `eval_shape` returns `ShapeDtypeStruct` without executing the computation
- arrays are immutable; in-place updates use the indexed-update syntax `x.at[idx].set(v)`

[LOCAL_ADMISSION]:
- Numeric-study kernels are written once as pure functions and wrapped with `jit` at the call boundary, not inside hot loops.
- Gradients use `grad` and `value_and_grad`; second-order studies use `hessian` or nested `jacfwd`/`jacrev`.
- Batch dimensions are expressed through `vmap`; device parallelism uses `pmap` or `shard_map`.
- PRNG keys thread explicitly through a study; a captured key plus its `split` lineage is the determinism receipt.
- Host interaction inside a transform routes through `pure_callback` with declared `result_shape_dtypes`.

[RAIL_LAW]:
- Package: `jax`
- Owns: XLA compilation, automatic differentiation, vectorization, device parallelism, and structured control flow
- Accept: pure numeric-study functions compiled through `jit`, differentiated through `grad`/`jacrev`, and batched through `vmap`
- Reject: hand-rolled autodiff or XLA lowering; Python control flow over traced values; PRNG key reuse; jax in a deterministic product runtime path on cp315
