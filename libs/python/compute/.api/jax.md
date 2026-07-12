# [PY_COMPUTE_API_JAX]

`jax` is the XLA-compiled, autodiff-capable array substrate every JAX-ecosystem sibling in this rail (`diffrax`, `lineax`, `optimistix`, `optax`, `equinox`, `interpax`, `quadax`, `numpyro`, `blackjax`) builds on: each consumes `jax.numpy` arrays, registers its carriers as `tree_util` pytrees, and is differentiated/compiled by the same `grad`/`jit`/`vmap` transforms cataloged here. The package owner compiles, differentiates, vectorizes, and parallelizes numeric-study kernels through composable function transforms; it never re-implements an autodiff or XLA pipeline jax owns. The distribution is manifest row worker because jaxlib ships no package.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jax`
- package: `jax` (runtime backend `jaxlib`; XLA)
- import: `jax`; submodules `jax.numpy`, `jax.lax`, `jax.random`, `jax.nn`, `jax.scipy`, `jax.tree_util`, `jax.sharding`, `jax.debug`, `jax.experimental`
- owner: `compute`
- rail: accelerator
- default precision: arrays default to 32-bit; `jax.config.update("jax_enable_x64", True)` promotes the rail to float64 (required when feeding `lineax`/`optimistix` solves that assume double precision)
- capability: XLA-compiled array computation with composable transforms — JIT compilation, forward/reverse-mode autodiff, automatic vectorization, device-parallel mapping, structured control-flow primitives, pytree registration, and a NumPy/SciPy-API mirror on XLA

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array, device, and sharding types
- rail: accelerator

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY]    | [CAPABILITY]                                                                |
| :-----: | :--------------------------------- | :--------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Array`                            | array type       | unified on-device array type (`jax.Array`); the leaf every sibling carries  |
|  [02]   | `ShapeDtypeStruct(shape, dtype)`   | shape spec       | abstract shape/dtype for `eval_shape`/`make_jaxpr` and `pure_callback` decl |
|  [03]   | `Device`                           | device handle    | a single addressable XLA device                                             |
|  [04]   | `Sharding`                         | sharding base    | abstract base for `NamedSharding`/`PositionalSharding`/`SingleDeviceShard.` |
|  [05]   | `NamedSharding(mesh, spec)`        | sharding spec    | mesh-and-partition-spec array sharding                                      |
|  [06]   | `P` / `PartitionSpec`              | partition spec   | partition spec for `shard_map` and `NamedSharding`                          |
|  [07]   | `Shard`                            | shard view       | one device's slice of a global array (`.device`, `.data`, `.index`)         |
|  [08]   | `custom_jvp(fun, nondiff_argnums)` | custom-rule type | forward-mode rule object; bind via `@f.defjvp` / `f.defjvps(*tangent_outs)` |
|  [09]   | `custom_vjp(fun, nondiff_argnums)` | custom-rule type | reverse-mode rule object; bind via `@f.defvjp(fwd, bwd)`                    |
|  [10]   | `Tracer`                           | trace value      | the abstract value flowing through a transform; never `bool()`/`.item()`-ed |
|  [11]   | `dtypes.canonicalize_dtype(dtype)` | dtype policy     | resolves a dtype under the active x32/x64 precision mode                    |

[PUBLIC_TYPE_SCOPE]: submodule namespaces
- rail: accelerator

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                                 |
| :-----: | :------------------------ | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `jax.numpy` (`jnp`)       | array namespace | NumPy-compatible array API on XLA; the array constructor every sibling reads |
|  [02]   | `jax.lax`                 | primitive ops   | low-level XLA primitives, control flow, and lowered linear algebra           |
|  [03]   | `jax.random`              | PRNG namespace  | splittable counter-based random generation                                   |
|  [04]   | `jax.nn`                  | NN primitives   | activations (`relu`, `gelu`, `softmax`, `softplus`) and `nn.initializers`    |
|  [05]   | `jax.scipy`               | SciPy on XLA    | XLA-backed subset of SciPy (`linalg`, `optimize`, `special`, `stats`)        |
|  [06]   | `jax.tree_util`           | pytree ops      | pytree flatten, map, leaves, structure, and node registration                |
|  [07]   | `jax.sharding`            | sharding API    | `Mesh`, `Sharding`, `NamedSharding`, and `PartitionSpec`                     |
|  [08]   | `jax.debug`               | side-effect ops | `print`/`callback`/`breakpoint` legal inside `jit`/`grad`/`vmap`             |
|  [09]   | `jax.config`              | runtime config  | `update("jax_enable_x64", ...)`, `jax_default_matmul_precision`, etc.        |
|  [10]   | `jax.experimental.ode`    | ODE staging     | `odeint` (legacy ODE); production ODE solves route to `diffrax`              |
|  [11]   | `jax.experimental.sparse` | sparse staging  | `BCOO`/`BCSR` sparse arrays and sparsified transforms                        |

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

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]   | [CAPABILITY]                             |
| :-----: | :------------------------------------------------ | :--------------- | :--------------------------------------- |
|  [01]   | `device_put(x, device=None, *, donate=False)`     | placement        | move an array to a device or sharding    |
|  [02]   | `device_get(x)`                                   | placement        | copy an array back to host NumPy         |
|  [03]   | `block_until_ready(x)`                            | synchronization  | block until async computation completes  |
|  [04]   | `devices(backend=None)` / `device_count(backend)` | discovery        | enumerate and count addressable devices  |
|  [05]   | `random.key(seed)` / `random.PRNGKey(seed)`       | PRNG key         | construct a splittable PRNG key          |
|  [06]   | `random.split(key, num=2)`                        | PRNG split       | derive independent subkeys               |
|  [07]   | `random.normal(key, shape, dtype)`                | PRNG sample      | standard-normal samples                  |
|  [08]   | `lax.scan(f, init, xs, length, reverse, unroll)`  | control flow     | compiled loop with carried state         |
|  [09]   | `lax.fori_loop(lower, upper, body_fun, init_val)` | control flow     | compiled counted loop                    |
|  [10]   | `lax.cond(pred, true_fun, false_fun, *operands)`  | control flow     | compiled conditional branch              |
|  [11]   | `lax.while_loop(cond_fun, body_fun, init_val)`    | control flow     | compiled while loop                      |
|  [12]   | `lax.associative_scan(fn, elems, reverse, axis)`  | control flow     | parallel-prefix (log-depth) scan         |
|  [13]   | `lax.switch(index, branches, *operands)`          | control flow     | compiled n-way branch by integer index   |
|  [14]   | `lax.stop_gradient(x)`                            | autodiff barrier | identity forward, zero cotangent         |
|  [15]   | `lax.linalg.{cholesky,qr,svd,eigh,lu}`            | dense linalg     | XLA-lowered factorizations behind solves |

[ENTRYPOINT_SCOPE]: `jax.numpy` array construction, predicates, and reductions
- rail: accelerator
- the array constructor surface every sibling carrier is built from; arrays are immutable, so writes go through `x.at[idx].set/add/mul(v)`. `jnp.asarray` is the canonical host->device adoption boundary; `isfinite`/`isnan` are the finiteness predicates a solver/receipt rail reads to gate divergence. `jnp.float64` is the explicit double-precision dtype handed to `asarray`/`ShapeDtypeStruct` once `config.update("jax_enable_x64", True)` is set (the x32 default truncates it).

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                                                                                                                                     |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `numpy.asarray(x, dtype=None)` / `array(...)`                                            | adoption       | adopt host/NumPy data to a device `Array` under active precision                                                                                                                                                                 |
|  [02]   | `numpy.isfinite(x)` / `isnan(x)` / `isinf(x)`                                            | predicate      | element-wise finiteness mask; the divergence/finiteness gate                                                                                                                                                                     |
|  [03]   | `numpy.diagonal(a, offset=0, axis1=0, axis2=1)`                                          | extraction     | diagonal of a matrix/batched matrix (Hessian-diag, trace prep)                                                                                                                                                                   |
|  [04]   | `numpy.where(cond, x, y)` / `clip(a, min, max)`                                          | selection      | branchless select / clamp inside a traced kernel                                                                                                                                                                                 |
|  [05]   | `numpy.einsum(subscripts, *operands)`                                                    | contraction    | named-index tensor contraction lowered to XLA dot-general                                                                                                                                                                        |
|  [06]   | `numpy.linalg.{solve,lstsq,norm,cholesky,svd,eigh}`                                      | dense linalg   | NumPy-API linear algebra; `lineax` owns the iterative/structured path                                                                                                                                                            |
|  [07]   | `numpy.max(a, axis=None, keepdims=False)` / `sum(a, axis=None)` / `argmin(a, axis=None)` | reduction      | worst-case / total / index reductions over a traced array (verdict-code fold, residual total, candidate-index select); the `jnp` reduction replaces Python `max`/`sum`/`float()` over a `Tracer`, which raise inside a transform |

[ENTRYPOINT_SCOPE]: pytree operations (`jax.tree_util`)
- rail: accelerator
- every sibling carrier (`equinox.Module`, `diffrax`/`optimistix`/`lineax` solver states, `optax.OptState`) is a registered pytree; these are the flatten/map/leaf operations that walk and reconstruct those carriers. `tree_leaves` is the canonical "all array leaves of a model/state" extraction the receipt and finiteness rails fold over.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :-------------------------------------------------------- | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `tree_util.tree_map(f, tree, *rest)`                      | pytree map     | map a function over the leaves of one-or-more aligned trees              |
|  [02]   | `tree_util.tree_leaves(tree, is_leaf=None)`               | pytree extract | flat list of leaves in deterministic order (fold/finiteness scan source) |
|  [03]   | `tree_util.tree_structure(tree)` / `tree_flatten(tree)`   | pytree split   | `PyTreeDef` (+ leaves) for structure-preserving reconstruction           |
|  [04]   | `tree_util.tree_unflatten(treedef, leaves)`               | pytree rebuild | rebuild a tree from a `PyTreeDef` and a leaf list                        |
|  [05]   | `tree_util.tree_reduce(f, tree, initializer)`             | pytree fold    | fold a binary op over all leaves (norm/sum receipts)                     |
|  [06]   | `tree_util.register_pytree_node(cls, flatten, unflatten)` | registration   | register a custom carrier as a pytree node                               |
|  [07]   | `tree_util.Partial(fun, *args, **kwargs)`                 | pytree closure | pytree-compatible partial application (traceable closure)                |

[ENTRYPOINT_SCOPE]: neural-net and SciPy mirrors (`jax.nn`, `jax.scipy`)
- rail: accelerator

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `nn.{relu,gelu,sigmoid,softmax,log_softmax,softplus}`   | activation     | differentiable activations for `equinox.nn` layers     |
|  [02]   | `nn.initializers.{glorot_normal,he_normal,orthogonal}`  | init           | parameter initializers seeded by a PRNG key            |
|  [03]   | `scipy.linalg.{solve,cho_factor,cho_solve,expm}`        | dense linalg   | SciPy-API factorizations on XLA                        |
|  [04]   | `scipy.optimize.minimize(fun, x0, method='BFGS')`       | optimize       | basic minimize; production routes to `optimistix`      |
|  [05]   | `scipy.special.{gammaln,logsumexp,erf}` / `scipy.stats` | special/stats  | special functions and log-densities (`numpyro` priors) |

[ENTRYPOINT_SCOPE]: side effects, configuration, and sharding context
- rail: accelerator

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------- | :----------------- | :---------------------------------------------------- |
|  [01]   | `debug.print(fmt, *args)` / `debug.callback(cb, *a)` | traced side effect | print/callback legal inside `jit`/`grad`/`vmap`       |
|  [02]   | `config.update("jax_enable_x64", True)`              | precision          | promote the rail to float64 (double-precision solves) |
|  [03]   | `sharding.Mesh(devices, axis_names)`                 | device mesh        | named device mesh for `NamedSharding`/`shard_map`     |
|  [04]   | `make_mesh(axis_shapes, axis_names)`                 | device mesh        | construct a `Mesh` from axis shapes/names             |

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

[SIBLING_INTEGRATION]:
- `jax` is the substrate; the sibling rails compose ON it, never beside it. The canonical stacked rail is: define carriers as `equinox.Module` pytrees -> assemble the objective from `jax.numpy`/`interpax` interpolants and `quadax` quadrature -> integrate dynamics with `diffrax.diffeqsolve` (inner linear solves via `lineax`) -> differentiate the whole pipeline with `jax.grad`/`equinox.filter_grad` -> descend with `optax`/`optimistix` -> compile the step with `jax.jit`/`equinox.filter_jit`.
- `tree_util` is the seam: `diffrax.Solution`, `optimistix.Solution`, `lineax` operators/states, `optax.OptState`, and `equinox.Module` are all registered pytree nodes, so `tree_map`/`tree_leaves`/`tree_structure` walk and reconstruct any sibling carrier uniformly. A finiteness/divergence receipt folds `numpy.isfinite` over `tree_leaves(state)`.
- `random.key`/`split` is the determinism backbone for every stochastic sibling — `diffrax` Brownian paths, `numpyro`/`blackjax` MCMC chains, `equinox.nn` initialization, and `optax.add_noise` all consume a split key, never a reseeded global RNG.
- `config.update("jax_enable_x64", True)` is a rail-wide precondition: `lineax`/`optimistix`/`diffrax` solves and `numpyro` log-densities assume float64; the x32 default silently degrades solver accuracy.
- `custom_jvp`/`custom_vjp` attach analytic derivative rules where a sibling's internal solve is non-differentiable or where the implicit-function theorem gives a closed-form adjoint (the same mechanism `lineax`/`optimistix` use internally for differentiable solves).

[LOCAL_ADMISSION]:
- Numeric-study kernels are written once as pure functions and wrapped with `jit` (or `equinox.filter_jit` for `Module` carriers) at the call boundary, not inside hot loops.
- Gradients use `grad`/`value_and_grad`; second-order studies use `hessian` or nested `jacfwd`/`jacrev`; for `Module` carriers the filtered mirrors (`equinox.filter_grad`) replace the bare transforms so static leaves bypass differentiation.
- `argnums` is `int | Sequence[int]` across `grad`/`value_and_grad`/`jacfwd`/`jacrev`/`hessian`: a bare `int` differentiates the one positional argument and the transform returns the single product, while a `tuple[int, ...]` differentiates several and the transform returns the per-argument product TUPLE — `value_and_grad((0, 1))` returns `(value, (grad₀, grad₁))` and `jacrev((0, 1))` returns `(J₀, J₁)` — so a multi-argument differentiation reads back as one block tuple a consumer concatenates rather than re-entering per argument. The filtered `equinox.filter_*` mirrors carry no integer `argnums` (they differentiate the first argument's inexact-array leaves by `filter_spec`); the bare `jax.*` transforms own the multi-argument index set.
- Batch dimensions are expressed through `vmap`; device parallelism uses `shard_map` over a `Mesh` (`pmap` is the legacy single-axis form).
- PRNG keys thread explicitly through a study; a captured key plus its `split` lineage is the determinism receipt shared with every stochastic sibling.
- Host interaction inside a transform routes through `pure_callback` (functionally pure) or `debug.callback` (ordered side effect) with declared `result_shape_dtypes`/`ShapeDtypeStruct`.
- Custom array carriers register via `tree_util.register_pytree_node` so the sibling transforms treat them as first-class leaves; ad-hoc Python containers break tracing.

[RAIL_LAW]:
- Package: `jax`
- Owns: XLA compilation, automatic differentiation, vectorization, device parallelism, structured control flow, the `jax.numpy`/`jax.scipy` array+linalg mirror, and the `tree_util` pytree protocol every sibling carrier registers against
- Accept: pure numeric-study functions compiled through `jit`, differentiated through `grad`/`jacrev`/`custom_vjp`, batched through `vmap`, with state carried as registered pytrees and randomness threaded through split keys
