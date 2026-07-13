# [PY_COMPUTE_API_EQUINOX]

`equinox` is the pytree-carrier layer of the JAX rail: it represents any parametrized object (a neural model, a physics parameter vector, a solver configuration) as a `Module` dataclass whose array fields are pytree leaves and whose static fields are folded into the `PyTreeDef`. Its filtered transforms (`filter_jit`/`filter_grad`/`filter_vmap`) are thin generalizations of the `jax` transforms that auto-partition array leaves from static leaves, so a `Module` mixing arrays and Python config flows through `jax.jit`/`jax.grad` without manual `static_argnums`. It is the carrier `optax`/`optimistix`/`diffrax` thread their state against and the type `interpax` interpolants subclass. It never re-implements a JAX transform or pytree operation the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `equinox`
- package: `equinox`
- import: `equinox` (alias `eqx`); submodules `equinox.nn`, `equinox.debug`, `equinox.internal`
- owner: `compute`
- rail: model
- capability: JAX-native modules via the `Module` pytree dataclass with `field`/`static`/`AbstractVar` field discipline, filter-aware JIT/grad/vmap/pmap transforms that auto-split arrays from static leaves, stateful-layer support (`nn.State`/`make_with_state`), pytree partition/combine/`tree_at` manipulation, leaf serialization, and a full neural-network layer library under `equinox.nn`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: module and configuration types
- rail: model
- call: `field(*, converter, static, default, ...)` / `static_field(**kwargs)` declare fields; `nn.StateIndex(init)` keys a `State` bundle (`state.get(index)` / `state.set(index, v)`); `Partial(fun, *args, **kwargs)` is the pytree `functools.partial`

`Enumeration` is the traceable enum base the `optimistix.RESULTS`/`diffrax.RESULTS`/`lineax.RESULTS` termination enums inherit: a member is an `EnumerationItem` exposing ONLY `_value` (the int code) and `_enumeration` (the class), never `.name`/`.value` — `Cls[item]` returns the human message and the member name inverts the class `_name_to_item` map. `Enumeration.promote(item)` widens a member from a parent to a subclass (`ValueError` on a same-class member, inheritance-widening, NOT a vmap reduction); `Enumeration.where(pred, a, b)` is the branchless `jnp.where` 2-way select; a batched-`vmap` worst-case aggregation is `jnp.max` over the `_value` codes plus the `_name_to_item` inversion.

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]       | [CAPABILITY]                                                                       |
| :-----: | :---------------------- | :------------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Module`                | pytree base class    | dataclass base for all JAX-native parametric model classes; auto-registered pytree |
|  [02]   | `field`                 | field declaration    | declares a `Module` field; `static=True` folds it into the `PyTreeDef`             |
|  [03]   | `static_field`          | field declaration    | shorthand for `field(static=True)` — non-array config carried statically           |
|  [04]   | `AbstractVar[T]`        | abstract field       | declares a required instance field on an abstract `Module` subclass                |
|  [05]   | `AbstractClassVar[T]`   | abstract class field | declares a required class-level field                                              |
|  [06]   | `nn.State`              | mutable state bundle | functional state threaded through stateful layers (`BatchNorm`, etc.)              |
|  [07]   | `nn.StateIndex`         | state handle         | typed key into a `State` bundle                                                    |
|  [08]   | `Enumeration`           | enum base            | traceable enum base the sibling `RESULTS` enums inherit (above)                    |
|  [09]   | `Partial`               | partial application  | pytree-compatible `functools.partial` (closure carried as a pytree)                |
|  [10]   | `EquinoxRuntimeError`   | error type           | runtime error raised inside filtered transforms (paired with `error_if`)           |
|  [11]   | `EquinoxTracetimeError` | error type           | trace-time error for invalid traced operations                                     |

[PUBLIC_TYPE_SCOPE]: filter transforms
- rail: model
- The filter transforms generalize the `jax` transforms over a single `arg_filter`: by default they partition on `is_array` (differentiate/trace only array leaves), so a `Module` mixing arrays + Python config needs no manual `static_argnums`. The autodiff mirrors carry the SAME `argnums`/`has_aux` semantics as their `jax` counterparts — confirming the residual question: yes, `filter_value_and_grad`/`filter_jacfwd`/`filter_jacrev`/`filter_hessian` all accept `has_aux`, and the autodiff family differentiates by the `arg`-position `filter_spec` rather than integer `argnums` (the first argument's array leaves are the differentiands). `filter_grad`/`filter_value_and_grad` are restricted to scalar output; `filter_jacfwd`/`filter_jacrev` are the vector-output Jacobian forms; `filter_hessian = filter_jacfwd(filter_jacrev(...))`.

- call: `filter_jit(fun=None, *, donate='none')`, `filter_grad(fun, *, has_aux=False)`, `filter_value_and_grad(fun, *, has_aux=False)`, `filter_vmap(fun, *, in_axes=if_array(0), out_axes=if_array(0), axis_name, axis_size)`, `filter_pmap(fun, *, in_axes, out_axes, axis_name, devices, donate)`
- call: `filter_jvp(fn, primals, tangents, **kwargs)`, `filter_vjp(fn, *primals, has_aux=False)`, `filter_jacfwd/filter_jacrev/filter_hessian(fun, *, has_aux=False)`, `filter_checkpoint(fun, *, prevent_cse=True, policy)`, `filter_eval_shape(fun, *args, **kwargs)`, `filter_closure_convert(fn, *args, **kwargs)`, `filter_custom_jvp(fn)` + `@fn.def_jvp(...)`, `filter_custom_vjp(fn)` + `@fn.def_fwd/def_bwd(...)`

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]         | [CAPABILITY]                                                             |
| :-----: | :----------------------- | :--------------------- | :----------------------------------------------------------------------- |
|  [01]   | `filter_jit`             | JIT compile            | JIT-compiles `Module`; non-array leaves auto-static, no `static_argnums` |
|  [02]   | `filter_grad`            | reverse-mode autodiff  | gradient w.r.t. inexact-array leaves of the first arg (scalar out)       |
|  [03]   | `filter_value_and_grad`  | combined grad          | `(value, grad)` pair; `has_aux` returns `((value, aux), grad)`           |
|  [04]   | `filter_vmap`            | vectorizing map        | vmaps array leaves per `in_axes` filter; static leaves broadcast         |
|  [05]   | `filter_jvp`             | forward-mode autodiff  | JVP through filtered array leaves                                        |
|  [06]   | `filter_vjp`             | reverse-mode autodiff  | `(out, vjp_fn)` (or `(out, vjp_fn, aux)`) through filtered leaves        |
|  [07]   | `filter_jacfwd`          | Jacobian fwd           | forward-mode Jacobian of array leaves (vector output)                    |
|  [08]   | `filter_jacrev`          | Jacobian rev           | reverse-mode Jacobian of array leaves (vector output)                    |
|  [09]   | `filter_hessian`         | Hessian                | `filter_jacfwd(filter_jacrev(fun))` over filtered leaves                 |
|  [10]   | `filter_pmap`            | parallel map           | pmaps array leaves across devices                                        |
|  [11]   | `filter_checkpoint`      | gradient checkpointing | rematerialize forward in backward to save memory                         |
|  [12]   | `filter_eval_shape`      | shape inference        | output `ShapeDtypeStruct` tree without executing                         |
|  [13]   | `filter_closure_convert` | closure conversion     | converts captured closure constants to explicit pytree args              |
|  [14]   | `filter_custom_jvp`      | custom JVP             | attaches a custom JVP rule on a filtered function                        |
|  [15]   | `filter_custom_vjp`      | custom VJP             | attaches a custom VJP rule on a filtered function                        |

[PUBLIC_TYPE_SCOPE]: pytree manipulation
- rail: model

- call: `partition(pytree, filter_spec, replace=None, is_leaf=None)`, `combine(*pytrees, is_leaf=None)`, `filter(pytree, filter_spec, inverse=False, replace=None)`, `tree_at(where, pytree, replace=, replace_fn=, is_leaf=)`, `tree_equal(*pytrees, typematch=False)`
- call: `tree_serialise_leaves(path_or_file, pytree)` / `tree_deserialise_leaves(path_or_file, like)` persist leaves; `apply_updates(model, updates)`, `nn.make_with_state(make_module)`, and the `is_array`/`is_inexact_array`/`error_if`/`branched_error_if` predicates and guards take their bare-name forms

| [INDEX] | [SYMBOL]                                     | [PACKAGE_ROLE]    | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------- | :---------------- | :---------------------------------------------------------------- |
|  [01]   | `partition`                                  | pytree split      | splits a pytree into `(matching, rest)` by a filter spec          |
|  [02]   | `combine`                                    | pytree merge      | merges partitioned pytrees back into one (inverse of `partition`) |
|  [03]   | `filter`                                     | pytree filter     | keeps leaves matching the spec, replacing the rest with `None`    |
|  [04]   | `tree_at`                                    | pytree update     | immutable leaf replacement selected by a `where` lambda           |
|  [05]   | `tree_equal`                                 | pytree equality   | structural + leaf equality of pytrees (returns a traced bool)     |
|  [06]   | `tree_check`                                 | pytree validation | validates pytree structure (no duplicate leaves)                  |
|  [07]   | `tree_inference`                             | inference mode    | sets `inference` flag on all `Dropout`/`BatchNorm` submodules     |
|  [08]   | `tree_serialise_leaves`                      | serialization     | serializes array leaves to a file in tree order                   |
|  [09]   | `tree_deserialise_leaves`                    | deserialization   | restores leaves into the structure of `like`                      |
|  [10]   | `apply_updates`                              | gradient apply    | applies an `optax` update tree (`is_inexact_array` leaves)        |
|  [11]   | `is_array` / `is_array_like`                 | predicate         | leaf is a JAX `Array` (or array-like scalar)                      |
|  [12]   | `is_inexact_array` / `is_inexact_array_like` | predicate         | leaf is a floating/complex JAX array (the default differentiand)  |
|  [13]   | `error_if` / `branched_error_if`             | runtime guard     | raises `EquinoxRuntimeError` inside a transform when `pred` holds |
|  [14]   | `nn.make_with_state`                         | state factory     | splits a stateful `Module` into `(model, State)` at construction  |
|  [15]   | `field(...)` / `nn.inference_mode`           | config            | per-field converters and an inference-mode context for serving    |

[PUBLIC_TYPE_SCOPE]: equinox.nn layer library
- rail: model

The dimension families ship `1d`/`2d`/`3d` variants over a dimension-parametric owner: `nn.Conv`/`nn.ConvTranspose` for convolution, `nn.MaxPool`/`nn.AvgPool`/`nn.Pool` and `nn.AdaptiveMaxPool`/`nn.AdaptiveAvgPool` for pooling. A layer taking array parameters requires a PRNG `key` at construction.

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE]         | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------- | :--------------------- | :-------------------------------------------------------------- |
|  [01]   | `nn.Linear`                                   | dense layer            | affine map; `key` required for parameter init                   |
|  [02]   | `nn.MLP`                                      | multi-layer perceptron | fully-connected stack with per-layer activation                 |
|  [03]   | `nn.Conv`                                     | convolution layers     | N-D convolution; `1d/2d/3d` + dimension-parametric owner        |
|  [04]   | `nn.ConvTranspose`                            | transposed conv        | transposed (fractionally-strided) convolutions                  |
|  [05]   | `nn.Sequential` / `nn.Lambda` / `nn.Identity` | composition            | sequential chain; pytree-wrapped pure fn; identity passthrough  |
|  [06]   | `nn.BatchNorm`                                | normalization          | threads running stats through `State` — needs `make_with_state` |
|  [07]   | `nn.LayerNorm`                                | normalization          | per-sample layer normalization                                  |
|  [08]   | `nn.GroupNorm`                                | normalization          | grouped-channel normalization                                   |
|  [09]   | `nn.RMSNorm`                                  | normalization          | RMS normalization                                               |
|  [10]   | `nn.MultiheadAttention`                       | attention              | scaled dot-product multi-head attention                         |
|  [11]   | `nn.Dropout`                                  | regularization         | keyed; inference flips via `tree_inference`/`nn.inference_mode` |
|  [12]   | `nn.Embedding`                                | embedding              | lookup-table embedding                                          |
|  [13]   | `nn.LSTMCell` / `nn.GRUCell`                  | recurrent cells        | single-step cells; scan over time with `jax.lax.scan`           |
|  [14]   | `nn.MaxPool` / `nn.AvgPool` / `nn.Pool`       | pooling                | spatial pooling; `nn.Pool` is the operator-parametric owner     |
|  [15]   | `nn.AdaptiveMaxPool` / `nn.AdaptiveAvgPool`   | pooling                | output-size-targeted adaptive pooling                           |
|  [16]   | `nn.RotaryPositionalEmbedding`                | positional encoding    | RoPE positional embedding                                       |
|  [17]   | `nn.PositionalEmbedding`                      | positional encoding    | learned positional embedding                                    |
|  [18]   | `nn.PReLU`                                    | param transform        | parametric ReLU activation                                      |
|  [19]   | `nn.WeightNorm`                               | param transform        | weight-norm reparameterization                                  |
|  [20]   | `nn.SpectralNorm`                             | param transform        | spectral-norm reparameterization (stateful)                     |
|  [21]   | `nn.Shared` / `nn.StateIndex`                 | weight tying / state   | tie parameters across submodules; typed handles into `State`    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model definition and transform entrypoints
- rail: model
- call: `partition(pytree, eqx.is_inexact_array) -> (params, static)`, `combine(params, static) -> model`, and `tree_at(lambda m: m.layer.weight, model, new_weight)` are the canonical split/merge/surgery; `nn.make_with_state(Model)(*args, *, key) -> (model, state)` inits a stateful model

| [INDEX] | [SURFACE]                 | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `filter_jit`              | compile        | JIT-compiles function with auto static-leaf bypass      |
|  [02]   | `filter_value_and_grad`   | autodiff       | `(loss, grad)` over inexact-array leaves of arg 0       |
|  [03]   | `filter_vmap`             | vectorize      | per-leaf vmap with array-only axis spec                 |
|  [04]   | `partition`               | split          | the canonical model split: trainable leaves vs static   |
|  [05]   | `combine`                 | merge          | rebuild the model inside the loss for `filter` parity   |
|  [06]   | `tree_at`                 | update         | immutable single-leaf surgery on a `Module`             |
|  [07]   | `tree_serialise_leaves`   | persistence    | checkpoint array leaves to disk                         |
|  [08]   | `tree_deserialise_leaves` | persistence    | restore leaves into a fresh-constructed template        |
|  [09]   | `apply_updates`           | gradient apply | apply an `optax` update tree to the model               |
|  [10]   | `nn.make_with_state`      | state init     | split a stateful model from its `State` at construction |

## [04]-[IMPLEMENTATION_LAW]

[MODULE_TOPOLOGY]:
- namespace: `equinox` (alias `eqx`); `Module`, filter transforms, pytree ops, and predicates at top level; layers under `equinox.nn`.
- a `Module` is a frozen dataclass auto-registered as a pytree: array fields are leaves, fields declared `field(static=True)`/`static_field()` are folded into the `PyTreeDef` and never traced/differentiated.
- the canonical differentiable-loss shape splits trainables from statics: `params, static = eqx.partition(model, eqx.is_inexact_array)`; the loss `combine`s them (`model = eqx.combine(params, static)`) so only `params` is differentiated. Equivalently `filter_grad`/`filter_value_and_grad` apply the same `is_inexact_array` filter automatically over the first argument.
- stateful layers (`BatchNorm`, `SpectralNorm`, stateful `Dropout`) thread a functional `nn.State`: construct with `nn.make_with_state(Model)`, call `out, state = model(x, state)`, and carry `state` like any other pytree leaf — there is no hidden mutation.
- runtime assertions inside a transform use `error_if`/`branched_error_if` (raising `EquinoxRuntimeError`), never a Python `assert` on a `Tracer`.

[SIBLING_INTEGRATION]:
- `equinox` is the pytree carrier the rest of the JAX rail threads state against. `optax`: `opt_state = optimizer.init(eqx.filter(model, eqx.is_inexact_array))`, then per step `updates, opt_state = optimizer.update(grads, opt_state, model); model = eqx.apply_updates(model, updates)` — the optimizer steps exactly the leaves `filter`/`partition` isolate.
- `optimistix`/`lineax`/`diffrax`: a `Module` is a valid `y0`/parameter pytree and a valid differentiand; an ODE vector field or a residual function written as a `Module.__call__` is passed straight into `diffrax.ODETerm`/`optimistix.least_squares`, and the solver's `Solution`/state pytrees combine with the model under the same `tree_map`.
- `interpax` interpolant objects (`Interpolator1D` etc.) are Equinox-style pytree modules, so an interpolated field is a differentiable `Module` leaf inside an Equinox objective with no boundary conversion.
- `jax`: the filtered transforms ARE the `jax` transforms with an `is_array` partition prepended; dropping to bare `jax.jit(model, static_argnums=...)` is the rejected manual form when `filter_jit` derives the static split from the pytree.
- `numpyro`/`blackjax`: a `Module`'s `filter`ed array leaves form a flat parameter pytree a sampler perturbs; the model structure (`static`) is the fixed `combine` template.

[LOCAL_ADMISSION]:
- model parameters live as inexact-array leaves; configuration (sizes, flags, activation choices) is `static_field`/`field(static=True)` so it never enters tracing.
- the model is constructed once outside the JIT loop (with an explicit PRNG `key`); only the `(params, opt_state)` pair flows per step, recombined with the captured `static` inside the loss.
- a captured `(model_structure, gradient)` pair plus the `optax`/`optimistix` step count is the training receipt; serialization is `tree_serialise_leaves` against a fresh-constructed template.

[RAIL_LAW]:
- Package: `equinox`
- Owns: JAX-native parametric `Module` pytrees with `field`/`static` discipline, filter-aware autodiff/JIT/vmap/pmap transforms, functional state (`nn.State`/`make_with_state`), pytree partition/combine/`tree_at`/serialization, and the `equinox.nn` layer library
- Accept: a `Module` whose trainables are inexact-array leaves, split by `partition`/`filter`, differentiated by `filter_value_and_grad`, stepped by an `optax`/`optimistix` update tree through `apply_updates`, and compiled by `filter_jit`; stateful layers threaded through `nn.State`
- Reject: `Module` subclasses with mutable Python state outside the pytree/`State` protocol; manual `static_argnums` where `filter_jit` derives the split; filter transforms wrapping non-JAX numeric backends; Python `assert`/`if` on traced leaves where `error_if` belongs; product model serving; wrapper-renames of filter-transform callables
