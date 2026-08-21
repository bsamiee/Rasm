# [PY_COMPUTE_API_JAX]

`jax` owns the XLA-compiled, autodiff-capable array substrate the compute numeric-study rail differentiates, compiles, vectorizes, and parallelizes through composable function transforms. Every JAX-ecosystem sibling composes on it — each consumes `jax.numpy` arrays, registers its carriers as `tree_util` pytrees, and rides the `grad`/`jit`/`vmap` transforms cataloged here — and the owner never re-implements an autodiff or XLA pipeline jax owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `jax`
- package: `jax` (runtime backend `jaxlib`; XLA)
- import: `jax`
- owner: `compute`
- rail: accelerator
- default precision: 32-bit arrays; `config.update("jax_enable_x64", True)` promotes the rail to float64
- capability: XLA-compiled array computation with composable transforms — JIT compilation, forward/reverse-mode autodiff, automatic vectorization, device-parallel mapping, structured control flow, pytree registration, a NumPy/SciPy-API mirror on XLA, the ahead-of-time staging ladder with its compile reports, and trace capture with an in-process trace reader

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: array, device, and sharding types

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
|  [10]   | `jax.stages`              | staging ladder  | `Traced`/`Lowered`/`Compiled` ahead-of-time compile stages and their reports |
|  [11]   | `jax.profiler`            | profiling       | trace capture, device-memory pprof, annotations, and the in-process reader   |
|  [12]   | `jax.experimental.ode`    | ODE staging     | `odeint` (legacy ODE); production ODE solves route to `diffrax`              |
|  [13]   | `jax.experimental.sparse` | sparse staging  | `BCOO`/`BCSR` sparse arrays and sparsified transforms                        |

[PUBLIC_TYPE_SCOPE]: ahead-of-time staging ladder (`jax.stages`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :---------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `Wrapped`         | protocol      | the object `jit` returns; `.trace`/`.lower` open the staging ladder         |
|  [02]   | `Traced`          | class         | a traced-but-unlowered stage; `.lower()` advances it, `.out_avals` reports  |
|  [03]   | `Lowered`         | class         | a lowered stage carrying StableHLO text and pre-compile cost estimates      |
|  [04]   | `Compiled`        | class         | a compiled executable carrying optimized-HLO text, cost, and memory reports |
|  [05]   | `CompilerOptions` | type alias    | the `dict` of backend compiler flags `Lowered.compile` accepts              |
|  [06]   | `ArgInfo`         | struct        | one argument's aval and donation flag on `Lowered.args_info`                |

[PUBLIC_TYPE_SCOPE]: profiling annotations and the in-process trace reader (`jax.profiler`)

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `ProfileOptions()`          | class         | collector levels the trace arms; subclasses the jaxlib native options      |
|  [02]   | `ProfileData`               | native class  | an opened `.xplane.pb` trace; the root of the in-process reader walk       |
|  [03]   | `ProfilePlane`              | native class  | one device or host plane inside a trace; `.name`, `.lines`, `.stats`       |
|  [04]   | `ProfileLine`               | native class  | one timeline row inside a plane; `.name`, `.events`                        |
|  [05]   | `ProfileEvent`              | native class  | one timed event; `.name`, `.start_ns`, `.duration_ns`, `.end_ns`, `.stats` |
|  [06]   | `TraceAnnotation(name)`     | context class | names a span on the trace timeline for the enclosed block                  |
|  [07]   | `StepTraceAnnotation(name)` | context class | names a step span; `step_num=` keyword drives per-step analysis            |

- `ProfileData`/`ProfilePlane`/`ProfileLine`/`ProfileEvent` bind from the jaxlib native extension, so they carry no stub file and reflect only under an installed `jaxlib`.
- Plane traversal is four tiers: `ProfileData.planes` -> `ProfilePlane.lines` -> `ProfileLine.events` -> `ProfileEvent`; reading events off a plane skips the line tier and yields nothing.

[PUBLIC_TYPE_SCOPE]: raise surface the boundary `catch=` sets name

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------------- | :------------ | :----------------------------------------------------- |
|  [01]   | `jax.errors.ConcretizationTypeError`   | `TypeError`   | a traced value read as concrete inside a transform     |
|  [02]   | `jax.errors.TracerBoolConversionError` | `TypeError`   | a tracer used in Python control flow                   |
|  [03]   | `TypeError`                            | builtin       | transform-time shape, dtype, or pytree-structure break |
|  [04]   | `ValueError`                           | builtin       | axis, shape, or argument-domain breach                 |
|  [05]   | `ImportError`                          | builtin       | absent backend plugin at the gated engine mint ONLY    |

Rows [01]/[02] subclass `TypeError`, so `(TypeError, ValueError)` is the total narrow tuple over the transform lane. Row [05] belongs to the ENGINE MINT alone: a gated fold brackets the mint, never the solve, so an `ImportError` raised inside a running transform propagates as the defect it is. UNVERIFIED BY PROBE — `jax` is not installed in this environment; the rows read from the documented surface and a probe is owed.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compilation and differentiation transforms
- call: `jit(fun, *, static_argnums, static_argnames, donate_argnums, in_shardings, out_shardings)`, `grad(fun, argnums=0, has_aux=False, holomorphic=False, allow_int=False)`, `value_and_grad(fun, argnums=0, has_aux=False)`, `jacfwd(fun, argnums=0, has_aux=False, holomorphic=False)`, `jacrev(fun, argnums=0, has_aux=False, allow_int=False)`, `hessian(fun, argnums=0, has_aux=False)`
- call: `jvp(fun, primals, tangents, has_aux=False)`, `vjp(fun, *primals, has_aux=False)`, `linearize(fun, *primals, has_aux=False)`, `checkpoint(fun, *, prevent_cse=True, policy, static_argnums)` (alias `remat`), `custom_jvp(fun, nondiff_argnums=())`, `custom_vjp(fun, nondiff_argnums=())`

| [INDEX] | [SURFACE]        | [ENTRY_FAMILY]    | [CAPABILITY]                           |
| :-----: | :--------------- | :---------------- | :------------------------------------- |
|  [01]   | `jit`            | compile           | XLA compilation of a function          |
|  [02]   | `grad`           | reverse autodiff  | gradient of a scalar-valued function   |
|  [03]   | `value_and_grad` | reverse autodiff  | value and gradient in one pass         |
|  [04]   | `jacfwd`         | forward autodiff  | forward-mode Jacobian                  |
|  [05]   | `jacrev`         | reverse autodiff  | reverse-mode Jacobian                  |
|  [06]   | `hessian`        | second-order      | Hessian via `jacfwd(jacrev(...))`      |
|  [07]   | `jvp`            | forward autodiff  | Jacobian-vector product                |
|  [08]   | `vjp`            | reverse autodiff  | vector-Jacobian product closure        |
|  [09]   | `linearize`      | linearization     | value plus linear tangent map          |
|  [10]   | `checkpoint`     | rematerialization | gradient checkpointing (alias `remat`) |
|  [11]   | `custom_jvp`     | custom rule       | attaches a custom JVP rule             |
|  [12]   | `custom_vjp`     | custom rule       | attaches a custom VJP rule             |

[ENTRYPOINT_SCOPE]: vectorization, parallelism, and staging

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

[ENTRYPOINT_SCOPE]: ahead-of-time staging and compile reports (`jax.stages`)
- `Lowered.cost_analysis` and `Compiled.cost_analysis`/`memory_analysis` answer `None` on a backend or runtime that reports nothing, so every read admits the absent case rather than assuming a mapping.
- Cost and memory payloads are unstructured nested data whose keys shift across jax and jaxlib releases; fold them by tally or key lookup, never by a pinned field roster.

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Wrapped.trace(*args, **kwargs) -> Traced`       | instance | stage a jitted function to its traced form          |
|  [02]   | `Wrapped.lower(*args, **kwargs) -> Lowered`      | instance | trace and lower in one step                         |
|  [03]   | `Traced.lower(*, lowering_platforms) -> Lowered` | instance | lower a traced stage for named target platforms     |
|  [04]   | `Lowered.compile(compiler_options) -> Compiled`  | instance | compile a lowered stage to an executable            |
|  [05]   | `Lowered.as_text(dialect, *, debug_info) -> str` | instance | StableHLO (or `"hlo"`) text of the lowered program  |
|  [06]   | `Lowered.compiler_ir(dialect)`                   | instance | the lowering's IR object; `None` when unavailable   |
|  [07]   | `Lowered.cost_analysis()`                        | instance | pre-compile execution-cost estimates; `None` if any |
|  [08]   | `Compiled.as_text() -> str`                      | instance | optimized-HLO text of the compiled executable       |
|  [09]   | `Compiled.cost_analysis()`                       | instance | post-compile cost estimates; `None` when absent     |
|  [10]   | `Compiled.memory_analysis()`                     | instance | peak/argument/output memory estimates; `None` if so |
|  [11]   | `Compiled.input_shardings` / `output_shardings`  | property | resolved per-argument and per-output shardings      |

[ENTRYPOINT_SCOPE]: profiling capture, annotation, and the in-process reader (`jax.profiler`)
- `trace`/`start_trace` write a TensorBoard-layout tree under `log_dir`: `<log_dir>/plugins/profile/<run>/` holds one `*.xplane.pb` beside one gzipped `*.trace.json.gz` Chrome trace, both written on every run.
- `perfetto_trace.json.gz` lands in that same run folder only under `create_perfetto_trace=True` or `create_perfetto_link=True`, and `create_perfetto_link` blocks the process until the served trace is opened.
- `device_memory_profile` returns gzip-compressed pprof bytes, so a caller writing them itself reproduces `save_device_memory_profile` and never re-compresses.
- trace-capture carry: `log_dir`, `create_perfetto_link`, `create_perfetto_trace`, `profiler_options`

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `trace(...)`                                               | static   | context manager bracketing one trace capture            |
|  [02]   | `start_trace(...)`                                         | static   | begin a trace; one runs at a time per process           |
|  [03]   | `stop_trace()`                                             | static   | end the running trace and write it to `log_dir`         |
|  [04]   | `device_memory_profile(backend) -> bytes`                  | static   | gzipped pprof snapshot of live device allocations       |
|  [05]   | `save_device_memory_profile(filename, backend)`            | static   | write that snapshot straight to a file                  |
|  [06]   | `annotate_function(func, name, **decorator_kwargs)`        | static   | decorator naming a function's span on the timeline      |
|  [07]   | `start_server(port, requires_backend) -> ProfilerServer`   | static   | serve on-demand profiling; one server per process       |
|  [08]   | `stop_server()`                                            | static   | tear the profiler server down                           |
|  [09]   | `register_subprocess(pid, port) -> Callable`               | static   | fold a subprocess's server into this process's captures |
|  [10]   | `ProfileData.from_file(path) -> ProfileData`               | factory  | open an emitted `.xplane.pb` with no TensorBoard hop    |
|  [11]   | `ProfileData.from_serialized_xspace(bytes) -> ProfileData` | factory  | open a trace already held as serialized bytes           |
|  [12]   | `ProfileData.find_plane_with_name(name) -> ProfilePlane`   | instance | select one plane by exact name                          |

- `start_server`: raises when a server is already active, and `stop_server` raises when none is; both guard on one process-global handle.
- `start_trace`: raises when a trace is already running, so concurrent captures serialize at the caller.
- `register_subprocess`: covers CPU profiling of the subprocess only, and its returned callable unregisters on call or on garbage collection.

[ENTRYPOINT_SCOPE]: device placement, PRNG, and control flow

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
- `jnp.asarray` is the host->device adoption boundary; `jnp.float64` is the explicit double dtype handed to `asarray`/`ShapeDtypeStruct` once x64 is enabled.
- `jnp` reductions (`max`/`sum`/`argmin`) fold verdict codes, residual totals, and candidate indices over a `Tracer`; Python `max`/`sum`/`float()` raise inside a transform.
- call: `asarray(x, dtype=None)`, `diagonal(a, offset=0, axis1=0, axis2=1)`, `where(cond, x, y)`, `clip(a, min, max)`, `einsum(subscripts, *operands)`, `max(a, axis=None, keepdims=False)`, `sum(a, axis=None)`, `argmin(a, axis=None)`

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :-------------------------------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `numpy.asarray` / `array`                           | adoption       | adopt host/NumPy data to a device `Array` under active precision |
|  [02]   | `numpy.isfinite` / `isnan` / `isinf`                | predicate      | element-wise finiteness mask; the divergence/finiteness gate     |
|  [03]   | `numpy.diagonal`                                    | extraction     | diagonal of a matrix/batched matrix (Hessian-diag, trace prep)   |
|  [04]   | `numpy.where` / `clip`                              | selection      | branchless select / clamp inside a traced kernel                 |
|  [05]   | `numpy.einsum`                                      | contraction    | named-index tensor contraction lowered to XLA dot-general        |
|  [06]   | `numpy.linalg.{solve,lstsq,norm,cholesky,svd,eigh}` | dense linalg   | NumPy-API linear algebra; `lineax` owns the iterative path       |
|  [07]   | `numpy.max` / `sum` / `argmin`                      | reduction      | worst-case / total / index reductions over a traced array        |

[ENTRYPOINT_SCOPE]: pytree operations (`jax.tree_util`)
- `tree_leaves` yields the ordered leaf list the receipt and finiteness rails fold; every sibling carrier (`equinox.Module`, `diffrax`/`optimistix`/`lineax` solver states, `optax.OptState`) is a registered pytree these ops walk and reconstruct.
- call: `tree_map(f, tree, *rest)`, `tree_leaves(tree, is_leaf=None)`, `tree_structure(tree)` / `tree_flatten(tree)`, `tree_unflatten(treedef, leaves)`, `tree_reduce(f, tree, initializer)`, `register_pytree_node(cls, flatten, unflatten)`, `Partial(fun, *args, **kwargs)`

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------------------ |
|  [01]   | `tree_util.tree_map`                        | pytree map     | map a function over the leaves of one-or-more aligned trees         |
|  [02]   | `tree_util.tree_leaves`                     | pytree extract | flat list of leaves in deterministic order (fold/finiteness source) |
|  [03]   | `tree_util.tree_structure` / `tree_flatten` | pytree split   | `PyTreeDef` (+ leaves) for structure-preserving reconstruction      |
|  [04]   | `tree_util.tree_unflatten`                  | pytree rebuild | rebuild a tree from a `PyTreeDef` and a leaf list                   |
|  [05]   | `tree_util.tree_reduce`                     | pytree fold    | fold a binary op over all leaves (norm/sum receipts)                |
|  [06]   | `tree_util.register_pytree_node`            | registration   | register a custom carrier as a pytree node                          |
|  [07]   | `tree_util.Partial`                         | pytree closure | pytree-compatible partial application (traceable closure)           |

[ENTRYPOINT_SCOPE]: neural-net and SciPy mirrors (`jax.nn`, `jax.scipy`)

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `nn.{relu,gelu,sigmoid,softmax,log_softmax,softplus}`   | activation     | differentiable activations for `equinox.nn` layers     |
|  [02]   | `nn.initializers.{glorot_normal,he_normal,orthogonal}`  | init           | parameter initializers seeded by a PRNG key            |
|  [03]   | `scipy.linalg.{solve,cho_factor,cho_solve,expm}`        | dense linalg   | SciPy-API factorizations on XLA                        |
|  [04]   | `scipy.optimize.minimize(fun, x0, method='BFGS')`       | optimize       | basic minimize; production routes to `optimistix`      |
|  [05]   | `scipy.special.{gammaln,logsumexp,erf}` / `scipy.stats` | special/stats  | special functions and log-densities (`numpyro` priors) |

[ENTRYPOINT_SCOPE]: side effects, configuration, and sharding context

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------- | :----------------- | :---------------------------------------------------- |
|  [01]   | `debug.print(fmt, *args)` / `debug.callback(cb, *a)` | traced side effect | print/callback legal inside `jit`/`grad`/`vmap`       |
|  [02]   | `config.update("jax_enable_x64", True)`              | precision          | promote the rail to float64 (double-precision solves) |
|  [03]   | `sharding.Mesh(devices, axis_names)`                 | device mesh        | named device mesh for `NamedSharding`/`shard_map`     |
|  [04]   | `make_mesh(axis_shapes, axis_names)`                 | device mesh        | construct a `Mesh` from axis shapes/names             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace: `jax`; transforms and types at top level, array and primitive ops under submodules
- transforms compose: `jit(grad(vmap(fun)))` is the canonical compile-differentiate-vectorize stack
- transformed functions are pure; side effects route through `pure_callback` or `jax.debug`
- PRNG keys are explicit, immutable, and splittable; `split` before every sample, reuse never
- control flow inside a transform uses `lax.scan`/`fori_loop`/`cond`/`while_loop`, never a Python loop over a traced value
- `static_argnums`/`static_argnames` mark JIT arguments compile-time constant; changing one retraces
- shapes are static at trace time; `eval_shape` returns `ShapeDtypeStruct` without executing
- arrays are immutable; in-place updates use the indexed-update syntax `x.at[idx].set(v)`

[STACKING]:
- canonical stacked rail: define carriers as `equinox.Module` pytrees -> assemble the objective from `jax.numpy`/`interpax` interpolants and `quadax` quadrature -> integrate dynamics with `diffrax.diffeqsolve` (inner linear solves via `lineax`) -> differentiate with `jax.grad`/`equinox.filter_grad` -> descend with `optax`/`optimistix` -> compile with `jax.jit`/`equinox.filter_jit`
- `tree_util` is the seam: `diffrax.Solution`, `optimistix.Solution`, `lineax` operators/states, `optax.OptState`, and `equinox.Module` are registered pytree nodes, so `tree_map`/`tree_leaves`/`tree_structure` walk and reconstruct any sibling carrier; a finiteness/divergence receipt folds `numpy.isfinite` over `tree_leaves(state)`
- `random.key`/`split` is the determinism backbone every stochastic sibling consumes — `diffrax` Brownian paths, `numpyro`/`blackjax` MCMC chains, `equinox.nn` initialization, `optax.add_noise` — never a reseeded global RNG
- `config.update("jax_enable_x64", True)` is a rail-wide precondition: `lineax`/`optimistix`/`diffrax` solves and `numpyro` log-densities assume float64
- `custom_jvp`/`custom_vjp` attach analytic derivative rules where a sibling's internal solve is non-differentiable or the implicit-function theorem gives a closed-form adjoint (the mechanism `lineax`/`optimistix` use internally for differentiable solves)
- `numerics/jit#JIT`: `_capture_jax` stages one `jit(kernel).lower(*probe)`, advances it to `Compiled`, and folds `Lowered.as_text` and `Compiled.as_text` extents beside `cost_analysis` entries into the same `EngineProfile` band the numba `inspect_llvm`/`inspect_asm` extents fill, so one profile shape spans both engines
- `numerics/jit#JIT`: `_traced` brackets the warm probe in `profiler.trace`, walks the emitted `.xplane.pb` through `ProfileData.from_file` -> `planes` -> `lines` -> `events`, and folds event counts and `duration_ns` totals beside `device_memory_profile` byte length into the xla case's trace band

[LOCAL_ADMISSION]:
- numeric-study kernels are pure functions wrapped with `jit` (or `equinox.filter_jit` for `Module` carriers) at the call boundary, never inside a hot loop
- gradients use `grad`/`value_and_grad`; second-order studies use `hessian` or nested `jacfwd`/`jacrev`; `Module` carriers swap the bare transforms for `equinox.filter_grad` so static leaves bypass differentiation
- `argnums` is `int | Sequence[int]` across `grad`/`value_and_grad`/`jacfwd`/`jacrev`/`hessian`: a bare `int` returns the single product, a `tuple[int, ...]` returns the per-argument product tuple (`value_and_grad((0, 1))` -> `(value, (grad₀, grad₁))`, `jacrev((0, 1))` -> `(J₀, J₁)`); the `equinox.filter_*` mirrors carry no integer `argnums` and differentiate the first argument's inexact-array leaves by `filter_spec`
- batch dimensions express through `vmap`; device parallelism uses `shard_map` over a `Mesh`
- host interaction inside a transform routes through `pure_callback` (functionally pure) or `debug.callback` (ordered side effect) with declared `result_shape_dtypes`/`ShapeDtypeStruct`
- custom array carriers register via `tree_util.register_pytree_node` so the sibling transforms treat them as first-class leaves
- compile evidence reads the staging ladder rather than wall-clock timing: `lower`/`compile` report the program the backend built, and a `None` cost or memory report records absent evidence rather than a zero
- trace capture stays caller-armed and disarmed by default — one trace runs per process, and an always-on capture pays device-trace cost on every compile while serializing concurrent solves at the profiler's own lock

[RAIL_LAW]:
- Package: `jax`
- Owns: XLA compilation, automatic differentiation, vectorization, device parallelism, structured control flow, the `jax.numpy`/`jax.scipy` array+linalg mirror, the `jax.stages` compile-report ladder, the `jax.profiler` capture and reader surface, and the `tree_util` pytree protocol every sibling carrier registers against
- Accept: pure numeric-study functions compiled through `jit`, differentiated through `grad`/`jacrev`/`custom_vjp`, batched through `vmap`, with state carried as registered pytrees, randomness threaded through split keys, and compile evidence read off the staging ladder
- Reject: hand-rolled autodiff or XLA pipelines; a Python loop over traced values where `lax` control flow applies; a reseeded global RNG where a split key threads; ad-hoc Python containers where a registered pytree carries state; a wall-clock stopwatch or a TensorBoard round trip where the staging reports and the in-process trace reader answer directly
