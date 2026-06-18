# [PY_COMPUTE_API_EQUINOX]

`equinox` supplies JAX-native neural network modules, filter-aware transforms, and pytree-manipulation primitives for the compute model-asset rail. The package owner defines parametric models as `Module` subclasses and compiles or differentiates them through `filter_jit`, `filter_grad`, and `filter_vmap`; it never re-implements a JAX transform or pytree operation the package owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `equinox`
- package: `equinox`
- import: `equinox` (alias `eqx`); submodule `equinox.nn`
- owner: `compute`
- rail: model
- installed: requires `jax`; marker-gated `python_version<'3.15'` (jaxlib ships no cp315 wheel)
- capability: JAX-native modules via `Module` pytree class, filter-aware JIT/grad/vmap transforms that split arrays from static leaves, and a full neural network layer library under `equinox.nn`

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: module and configuration types
- rail: model

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]       | [CAPABILITY]                                     |
| :-----: | :---------------------- | :------------------- | :----------------------------------------------- |
|   [1]   | `Module`                | pytree base class    | base for all JAX-native parametric model classes |
|   [2]   | `StrictConfig`          | module config        | strict type-checking configuration for `Module`  |
|   [3]   | `AbstractVar`           | abstract field       | declares a required field on a `Module` subclass |
|   [4]   | `AbstractClassVar`      | abstract class field | declares a required class-level field            |
|   [5]   | `Enumeration`           | enum base            | equinox-native enumeration type                  |
|   [6]   | `Partial`               | partial application  | pytree-compatible `functools.partial`            |
|   [7]   | `EquinoxRuntimeError`   | error type           | runtime error raised inside filtered transforms  |
|   [8]   | `EquinoxTracetimeError` | error type           | trace-time error for invalid traced operations   |

[PUBLIC_TYPE_SCOPE]: filter transforms
- rail: model

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]         | [CAPABILITY]                                       |
| :-----: | :----------------------- | :--------------------- | :------------------------------------------------- |
|   [1]   | `filter_jit`             | JIT compile            | JIT-compiles `Module`; static leaves skip tracing  |
|   [2]   | `filter_grad`            | reverse-mode autodiff  | gradient w.r.t. inexact-array leaves only          |
|   [3]   | `filter_vmap`            | vectorizing map        | vmaps over array leaves; static leaves broadcast   |
|   [4]   | `filter_value_and_grad`  | combined grad          | returns (value, grad) pair                         |
|   [5]   | `filter_jvp`             | forward-mode autodiff  | JVP through filtered leaves                        |
|   [6]   | `filter_vjp`             | reverse-mode autodiff  | VJP through filtered leaves                        |
|   [7]   | `filter_jacfwd`          | Jacobian fwd           | forward-mode Jacobian                              |
|   [8]   | `filter_jacrev`          | Jacobian rev           | reverse-mode Jacobian                              |
|   [9]   | `filter_hessian`         | Hessian                | Hessian through filtered leaves                    |
|  [10]   | `filter_pmap`            | parallel map           | pmaps over array leaves across devices             |
|  [11]   | `filter_checkpoint`      | gradient checkpointing | memory-efficient gradient checkpointing            |
|  [12]   | `filter_eval_shape`      | shape inference        | evaluates output shape without computation         |
|  [13]   | `filter_closure_convert` | closure conversion     | converts a function closure to a pytree            |
|  [14]   | `filter_custom_jvp`      | custom JVP             | registers a custom JVP rule on a filtered function |
|  [15]   | `filter_custom_vjp`      | custom VJP             | registers a custom VJP rule on a filtered function |

[PUBLIC_TYPE_SCOPE]: pytree manipulation
- rail: model

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]    | [CAPABILITY]                                           |
| :-----: | :------------------------ | :---------------- | :----------------------------------------------------- |
|   [1]   | `partition`               | pytree split      | splits a pytree into two by a filter spec              |
|   [2]   | `combine`                 | pytree merge      | merges two pytrees back into one                       |
|   [3]   | `filter`                  | pytree filter     | keeps leaves matching a filter spec                    |
|   [4]   | `tree_at`                 | pytree update     | returns a pytree with specified leaves replaced        |
|   [5]   | `tree_equal`              | pytree equality   | structural equality of two pytrees                     |
|   [6]   | `tree_check`              | pytree validation | validates pytree structure                             |
|   [7]   | `tree_inference`          | inference mode    | sets all `BatchNorm` modules to inference mode         |
|   [8]   | `tree_serialise_leaves`   | serialization     | serializes pytree leaves to a file                     |
|   [9]   | `tree_deserialise_leaves` | deserialization   | deserializes pytree leaves from a file                 |
|  [10]   | `apply_updates`           | gradient apply    | applies optax-style gradient updates to a pytree       |
|  [11]   | `is_array`                | predicate         | returns `True` if a leaf is a JAX array                |
|  [12]   | `is_inexact_array`        | predicate         | returns `True` if a leaf is a floating-point JAX array |

[PUBLIC_TYPE_SCOPE]: equinox.nn layer library
- rail: model

| [INDEX] | [SYMBOL]                                                     | [PACKAGE_ROLE]         | [CAPABILITY]                                                 |
| :-----: | :----------------------------------------------------------- | :--------------------- | :----------------------------------------------------------- |
|   [1]   | `nn.Linear`                                                  | dense layer            | `(in_features, out_features, use_bias)`                      |
|   [2]   | `nn.MLP`                                                     | multi-layer perceptron | `(in_size, out_size, width_size, depth, activation)`         |
|   [3]   | `nn.Conv1d` / `Conv2d` / `Conv3d`                            | convolution layers     | 1D/2D/3D convolutions                                        |
|   [4]   | `nn.ConvTranspose1d` / `ConvTranspose2d` / `ConvTranspose3d` | transposed conv        | transposed convolutions                                      |
|   [5]   | `nn.Sequential`                                              | composition            | `(layers: Sequence[Callable])`                               |
|   [6]   | `nn.BatchNorm`                                               | normalization          | `(input_size, axis_name, eps, momentum)`                     |
|   [7]   | `nn.LayerNorm`                                               | normalization          | `(shape, eps, use_weight, use_bias)`                         |
|   [8]   | `nn.GroupNorm`                                               | normalization          | group normalization layer                                    |
|   [9]   | `nn.RMSNorm`                                                 | normalization          | RMS normalization layer                                      |
|  [10]   | `nn.MultiheadAttention`                                      | attention              | `(num_heads, query_size, key_size, value_size, output_size)` |
|  [11]   | `nn.Dropout`                                                 | regularization         | `(p, inference)`                                             |
|  [12]   | `nn.Embedding`                                               | embedding              | lookup table embedding layer                                 |
|  [13]   | `nn.LSTMCell` / `GRUCell`                                    | recurrent cells        | LSTM and GRU single-step cells                               |
|  [14]   | `nn.MaxPool1d` / `AvgPool1d` (+ 2d/3d variants)              | pooling                | spatial pooling layers                                       |
|  [15]   | `nn.RotaryPositionalEmbedding`                               | positional encoding    | RoPE positional embedding                                    |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model definition and transform entrypoints
- rail: model

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|   [1]   | `filter_jit(fun, *, donate, **jitkwargs)`                      | compile        | JIT-compiles function with static-leaf bypass |
|   [2]   | `filter_grad(fun, *, has_aux, **gradkwargs)`                   | autodiff       | gradient w.r.t. inexact-array leaves          |
|   [3]   | `filter_vmap(fun, *, in_axes, out_axes, axis_name, axis_size)` | vectorize      | per-leaf vmap with axis spec                  |
|   [4]   | `partition(pytree, filter_spec, replace, is_leaf)`             | split          | splits leaves into (matching, rest) pair      |
|   [5]   | `combine(*pytrees, is_leaf)`                                   | merge          | merges partitioned pytrees back               |
|   [6]   | `tree_at(where, pytree, replace, replace_fn, is_leaf)`         | update         | immutable leaf replacement by path selector   |
|   [7]   | `tree_serialise_leaves(path, pytree)`                          | persistence    | saves pytree leaves to disk                   |
|   [8]   | `tree_deserialise_leaves(path, like)`                          | persistence    | restores pytree leaves from disk              |
|   [9]   | `apply_updates(model, updates)`                                | gradient apply | applies optax update tree to model            |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `equinox`
- Owns: JAX-native parametric modules, filter-aware autodiff/JIT/vmap transforms, and the `equinox.nn` layer library
- Accept: a `Module` subclass with parameters as pytree leaves compiled through `filter_jit` and differentiated through `filter_grad`, with a captured model structure and gradient receipt
- Reject: `eqx.Module` subclasses with mutable state outside the pytree protocol; filter transforms wrapping non-JAX numeric backends; product model serving; wrapper-renames of filter transform callables
