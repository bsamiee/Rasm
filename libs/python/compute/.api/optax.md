# [PY_COMPUTE_API_OPTAX]

`optax` supplies composable JAX gradient-transformation optimizers as the first-order-descent axis row beside the Optimistix solver core in the inverse-design loop. A `GradientTransformation` is an `(init, update)` pair threading an opaque `OptState`; `chain` composes transformations, and `apply_updates` applies the transformed step to an Equinox-parameterized PyTree. The package owns first-order, accelerated, and adaptive descent and the learning-rate-schedule algebra; the second-order, trust-region, and adjoint solver routes stay on `optimistix`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optax`
- package: `optax`
- module: `optax`
- asset: runtime library (JAX extension)
- rail: first-order gradient-descent optimization

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gradient-transformation carriers
- rail: first-order gradient-descent optimization

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [KEY_SIGNATURE]                                                        |
| :-----: | :-------------------------------- | :---------------- | :--------------------------------------------------------------------- |
|   [1]   | `GradientTransformation`          | optimizer carrier | NamedTuple `(init: Callable[[Params], OptState], update: Callable)`    |
|   [2]   | `GradientTransformationExtraArgs` | optimizer carrier | `update(updates, state, params=None, **extra_args)` variant            |
|   [3]   | `OptState`                        | opaque state      | PyTree threaded between `update` calls; not introspected by the caller |
|   [4]   | `Updates`                         | type alias        | PyTree of updates parallel to the params PyTree                        |
|   [5]   | `Params`                          | type alias        | PyTree of parameters the transformation steps                          |
|   [6]   | `Schedule`                        | type alias        | `Callable[[chex.Numeric], chex.Numeric]` step-count → scalar           |
|   [7]   | `EmptyState`                      | state             | the unit state for stateless transformations                           |
|   [8]   | `ScaleByAdamState`                | state             | fields `count`, `mu`, `nu` — Adam first/second moment accumulators     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: optimizer aliases
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY] | [RESULT]                                 |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|   [1]   | `sgd(learning_rate, momentum=None, nesterov=False, accumulator_dtype=None)`          | first-order    | SGD `GradientTransformation`             |
|   [2]   | `adam(learning_rate, b1=0.9, b2=0.999, eps=1e-8, eps_root=0.0)`                      | adaptive       | Adam `GradientTransformation`            |
|   [3]   | `adamw(learning_rate, b1=0.9, b2=0.999, eps=1e-8, weight_decay=1e-4)`                | adaptive       | decoupled-weight-decay Adam              |
|   [4]   | `rmsprop(learning_rate, decay=0.9, eps=1e-8, momentum=None, nesterov=False)`         | adaptive       | RMSProp `GradientTransformation`         |
|   [5]   | `adagrad(learning_rate, initial_accumulator_value=0.1, eps=1e-7)`                    | adaptive       | Adagrad `GradientTransformation`         |
|   [6]   | `lbfgs(learning_rate=None, memory_size=10, scale_init_precond=True, linesearch=...)` | quasi-Newton   | L-BFGS `GradientTransformationExtraArgs` |
|   [7]   | `lion(learning_rate, b1=0.9, b2=0.99, weight_decay=0.0)`                             | sign-momentum  | Lion `GradientTransformation`            |
|   [8]   | `novograd(learning_rate, b1=0.9, b2=0.25, eps=1e-8, weight_decay=0.0)`               | adaptive       | NovoGrad `GradientTransformation`        |

[ENTRYPOINT_SCOPE]: gradient-transformation combinators and core update
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RESULT]                                        |
| :-----: | :------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|   [1]   | `chain(*transformations)` → `GradientTransformationExtraArgs` | combinator     | sequential composition of transformations       |
|   [2]   | `named_chain(*name_and_transform_pairs)`                      | combinator     | named sequential composition                    |
|   [3]   | `apply_updates(params, updates)` → `Params`                   | apply          | `tree_map(lambda p, u: p + u, params, updates)` |
|   [4]   | `apply_if_finite(inner, max_consecutive_errors)`              | combinator     | skips the step on non-finite updates            |
|   [5]   | `incremental_update(new_tensors, old_tensors, step_size)`     | apply          | Polyak/EMA parameter blend                      |
|   [6]   | `multi_transform(transforms, param_labels)`                   | combinator     | per-leaf transform routing by label             |
|   [7]   | `masked(inner, mask)`                                         | combinator     | applies `inner` only to masked leaves           |

[ENTRYPOINT_SCOPE]: gradient transformations (`scale_by_*` / `clip*` / `add_*`)
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY] | [RESULT]                              |
| :-----: | :------------------------------------------------------ | :------------- | :------------------------------------ |
|   [1]   | `scale(step_size)`                                      | transformation | scales updates by a constant          |
|   [2]   | `scale_by_adam(b1, b2, eps, eps_root)`                  | transformation | Adam moment rescaling                 |
|   [3]   | `scale_by_rms(decay, eps)`                              | transformation | RMS rescaling                         |
|   [4]   | `scale_by_schedule(step_size_fn)`                       | transformation | per-step scaling from a `Schedule`    |
|   [5]   | `scale_by_learning_rate(learning_rate, flip_sign=True)` | transformation | learning-rate scale with descent sign |
|   [6]   | `clip(max_delta)`                                       | transformation | element-wise update clip              |
|   [7]   | `clip_by_global_norm(max_norm)`                         | transformation | global-norm gradient clip             |
|   [8]   | `add_decayed_weights(weight_decay, mask=None)`          | transformation | L2 weight decay                       |
|   [9]   | `add_noise(eta, gamma, seed)`                           | transformation | annealed Gaussian gradient noise      |
|  [10]   | `trace(decay, nesterov=False)`                          | transformation | momentum trace accumulation           |
|  [11]   | `zero_nans()`                                           | transformation | replaces NaN updates with zero        |

[ENTRYPOINT_SCOPE]: learning-rate schedules
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [RESULT]                       |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :----------------------------- |
|   [1]   | `constant_schedule(value)`                                                        | schedule       | constant step size             |
|   [2]   | `linear_schedule(init_value, end_value, transition_steps, transition_begin=0)`    | schedule       | linear ramp                    |
|   [3]   | `exponential_decay(init_value, transition_steps, decay_rate, staircase=False)`    | schedule       | geometric decay                |
|   [4]   | `cosine_decay_schedule(init_value, decay_steps, alpha=0.0)`                       | schedule       | cosine annealing               |
|   [5]   | `warmup_cosine_decay_schedule(init_value, peak_value, warmup_steps, decay_steps)` | schedule       | warmup-then-cosine             |
|   [6]   | `piecewise_constant_schedule(init_value, boundaries_and_scales)`                  | schedule       | step-wise scaled schedule      |
|   [7]   | `join_schedules(schedules, boundaries)`                                           | schedule       | concatenated schedule segments |

[ENTRYPOINT_SCOPE]: loss functions
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]  | [RESULT]              |
| :-----: | :--------------------------------------------------- | :-------------- | :-------------------- |
|   [1]   | `losses.l2_loss(predictions, targets=None)`          | regression loss | half squared error    |
|   [2]   | `losses.huber_loss(predictions, targets, delta=1.0)` | regression loss | Huber robust loss     |
|   [3]   | `losses.softmax_cross_entropy(logits, labels)`       | classification  | softmax cross-entropy |
|   [4]   | `losses.cosine_distance(predictions, targets)`       | similarity loss | cosine distance       |

## [4]-[IMPLEMENTATION_LAW]

[OPTIMIZER_TOPOLOGY]:
- namespace: `optax`; optimizer aliases, transformations, combinators, and schedules at top level; loss functions under `optax.losses`.
- a `GradientTransformation` is the `(init, update)` NamedTuple; `init(params)` returns the opaque `OptState`, `update(grads, state, params=None)` returns `(updates, new_state)`.
- the canonical step is `updates, opt_state = optimizer.update(grads, opt_state, params); params = optax.apply_updates(params, updates)`; `apply_updates` adds the updates to the params PyTree leaf-wise.
- alias optimizers (`adam`, `sgd`, `rmsprop`, `adamw`, `lbfgs`) are pre-composed `chain` of `scale_by_*` and `scale_by_learning_rate`; the descent sign is folded by `scale_by_learning_rate(flip_sign=True)` so the update subtracts the scaled gradient.
- `learning_rate` accepts a scalar or a `Schedule` callable; passing a schedule threads `scale_by_schedule` so the step decays with the iteration count.
- `lbfgs` returns a `GradientTransformationExtraArgs`; its `update` requires `value`, `grad`, and `value_fn` extra args for the line search, so it is driven through `optax.value_and_grad_from_state` rather than a bare gradient.
- grads are obtained from `jax.grad`/`jax.value_and_grad` over the Equinox-parameterized objective; the params PyTree the optimizer threads is the same PyTree `equinox.partition`/`combine` splits.

[LOCAL_ADMISSION]:
- the optimizer instance is constructed once outside the JIT loop with its hyperparameters; only the `(updates, state)` pair flows per step.
- `chain` composes a custom optimizer from `clip_by_global_norm`, a `scale_by_*` moment rescaler, and `scale_by_learning_rate`; a hand-rolled Adam moment loop is the rejected form.
- `multi_transform` and `masked` route distinct transforms to labelled or masked PyTree leaves so a heterogeneous design vector (geometry params versus material density) descends under per-block policy without a parallel optimizer.
- `apply_if_finite`/`zero_nans` guard the inverse-design loop against a non-finite gradient from a diverged inner solve.

[RAIL_LAW]:
- Package: `optax`
- Owns: composable first-order, accelerated, and adaptive JAX gradient-descent optimizers and the learning-rate-schedule algebra
- Accept: an alias optimizer or a `chain` of transformations as the `optax`-descent axis row threaded through `optimistix.OptaxMinimiser`, or driven directly over an Equinox PyTree with `apply_updates`
- Reject: a hand-rolled momentum/Adam accumulator loop; `optax` for second-order, trust-region, or implicit-adjoint solves that `optimistix` owns
