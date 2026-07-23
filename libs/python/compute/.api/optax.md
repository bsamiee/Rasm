# [PY_COMPUTE_API_OPTAX]

`optax` owns the first-order gradient-descent axis of the inverse-design loop as composable JAX gradient transformations: a `GradientTransformation` threads an opaque `OptState` through an `(init, update)` pair, and every alias optimizer is a pre-composed `chain` of `scale_by_*` and `scale_by_learning_rate`. It spans first-order, adaptive, quasi-Newton, and projected descent, the schedule algebra, and a loss/projection/tree-utility library. `optimistix.OptaxMinimiser` lifts any optimizer into the unified `minimise` solve, or `apply_updates` drives it directly over an `equinox` PyTree; second-order routes stay on `optimistix`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optax`
- package: `optax`
- module: `optax`
- namespaces: `optax.losses`, `optax.schedules`, `optax.transforms`, `optax.projections`, `optax.assignment`, `optax.perturbations`, `optax.second_order`, `optax.tree_utils`, `optax.contrib`
- rail: first-order gradient-descent optimization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gradient-transformation carriers and state

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [CAPABILITY]                                                                     |
| :-----: | :-------------------------------- | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `GradientTransformation`          | optimizer carrier | `(init, update)` NamedTuple; shapes in `[TOPOLOGY]`                              |
|  [02]   | `GradientTransformationExtraArgs` | optimizer carrier | `(init, update)` subtype; `update` takes solver `**extra_args` (L-BFGS contract) |
|  [03]   | `OptState`                        | opaque state      | PyTree threaded between `update` calls; not introspected by the caller           |
|  [04]   | `Updates` \| `Params`             | type alias        | the PyTree of updates / parameters, structurally parallel                        |
|  [05]   | `Schedule` \| `ScalarOrSchedule`  | type alias        | `Callable[[count], scalar]`; a `learning_rate` accepts a scalar or a `Schedule`  |
|  [06]   | `EmptyState`                      | state             | the unit state for stateless transformations                                     |
|  [07]   | `ScaleByAdamState`                | state             | fields `count`, `mu`, `nu` — Adam first/second moment accumulators               |
|  [08]   | `MultiSteps` / `MultiStepsState`  | accumulator       | applies a transformation every `k` micro-steps (gradient accumulation)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: alias optimizers (top-level pre-composed chains)
- shape: each alias is a factory returning `GradientTransformationExtraArgs`.
- shared: the Adam base is `(learning_rate, b1=0.9, b2=0.999, eps=1e-8, eps_root=0.0, mu_dtype=None, *, nesterov=False)` — `adam` is it, `adamw` adds `weight_decay=1e-4, mask=None`, `lbfgs` defaults `linesearch=scale_by_zoom_linesearch(...)`.

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------------------------- | :----------------------------- |
|  [01]   | `sgd(learning_rate, momentum=None, nesterov=False, accumulator_dtype=None)`          | first-order SGD                |
|  [02]   | `adam(...)`                                                                          | adaptive Adam base             |
|  [03]   | `adamw(..., weight_decay=1e-4, mask=None)`                                           | decoupled-decay Adam           |
|  [04]   | `nadamw(...)` / `nadam(...)`                                                         | Nesterov-Adam variants         |
|  [05]   | `amsgrad(...)`                                                                       | AMSGrad                        |
|  [06]   | `radam(..., threshold=5.0)`                                                          | rectified Adam                 |
|  [07]   | `adamax(learning_rate, b1=0.9, b2=0.999, eps=1e-8)`                                  | infinity-norm Adam             |
|  [08]   | `adabelief(...)`                                                                     | belief Adam                    |
|  [09]   | `yogi(..., eps=1e-3)`                                                                | Yogi                           |
|  [10]   | `adafactor(learning_rate=None, min_dim_size_to_factor=128, decay_rate=0.8, ...)`     | factored-moment Adam (no `nu`) |
|  [11]   | `rmsprop(learning_rate, decay=0.9, eps=1e-8, momentum=None, nesterov=False)`         | RMSProp                        |
|  [12]   | `adagrad(learning_rate, initial_accumulator_value=0.1, eps=1e-7)`                    | Adagrad                        |
|  [13]   | `lamb(learning_rate, b1=0.9, b2=0.999, eps=1e-6, weight_decay=0.0, mask=None)`       | layer-wise trust-ratio         |
|  [14]   | `lars(...)`                                                                          | LARS layer-wise adaptive       |
|  [15]   | `lbfgs(learning_rate=None, memory_size=10, scale_init_precond=True, linesearch=...)` | quasi-Newton L-BFGS            |
|  [16]   | `lion(learning_rate, b1=0.9, b2=0.99, mu_dtype=None, weight_decay=1e-3, mask=None)`  | sign-momentum Lion             |
|  [17]   | `novograd(learning_rate, b1=0.9, b2=0.25, eps=1e-6, eps_root=0.0, weight_decay=0.0)` | NovoGrad                       |
|  [18]   | `fromage(learning_rate, min_norm=1e-6)`                                              | scale-invariant Fromage        |
|  [19]   | `sm3(learning_rate, momentum=0.9)`                                                   | scale-invariant SM3            |
|  [20]   | `rprop(learning_rate, eta_minus=0.5, eta_plus=1.2, ...)`                             | scale-invariant Rprop          |
|  [21]   | `polyak_sgd(max_learning_rate=1.0, scaling=1.0, f_min=0.0, eps=0.0, variant='sps')`  | Polyak step-size SGD           |
|  [22]   | `optimistic_gradient_descent(learning_rate, alpha=1.0, beta=1.0)`                    | extragradient OGD              |
|  [23]   | `sign_sgd(learning_rate)`                                                            | signSGD                        |

[ENTRYPOINT_SCOPE]: combinators, core update, and accumulation
- accumulate: `MultiSteps(opt, every_k_schedule, use_grad_mean=True, should_skip_update_fn=None, accumulator_dtype=float32)` accumulates gradients over `k` micro-steps.

| [INDEX] | [SURFACE]                                                                        | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `chain(*transformations)` → `GradientTransformationExtraArgs`                    | factory | sequential composition             |
|  [02]   | `named_chain(*name_and_transform_pairs)`                                         | factory | named sequential composition       |
|  [03]   | `apply_updates(params, updates)` → `Params`                                      | static  | leaf-wise `params + updates`       |
|  [04]   | `incremental_update(new_tensors, old_tensors, step_size)`                        | static  | Polyak/EMA target-parameter blend  |
|  [05]   | `periodic_update`                                                                | static  | periodic target-parameter copy     |
|  [06]   | `multi_transform(transforms, param_labels, *, mask_compatible_extra_args=False)` | factory | per-leaf transform routing         |
|  [07]   | `masked(inner, mask, *, mask_compatible_extra_args=False)`                       | factory | applies `inner` to masked leaves   |
|  [08]   | `partition(transforms, ...)`                                                     | factory | routes partitioned leaves          |
|  [09]   | `conditionally_transform(inner, should_transform_fn)`                            | factory | predicate-gated transformation     |
|  [10]   | `conditionally_mask(...)`                                                        | factory | predicate-gated masking            |
|  [11]   | `apply_if_finite(inner, max_consecutive_errors)`                                 | factory | skips the step on non-finite       |
|  [12]   | `MultiSteps(...)`                                                                | ctor    | gradient accumulation over `k`     |
|  [13]   | `inject_hyperparams(inner_factory, static_args=(), hyperparam_dtype=None)`       | factory | scalar hyperparams to state leaves |

[ENTRYPOINT_SCOPE]: gradient transformations (`scale_by_*` / `clip*` / `add_*` / line search)
- line-search: `scale_by_zoom_linesearch(max_linesearch_steps, max_learning_rate=None, slope_rtol=1e-4, curv_rtol=0.9, ...)` (Wolfe zoom, L-BFGS default); `scale_by_backtracking_linesearch(max_backtracking_steps, slope_rtol=1e-4, decrease_factor=0.8, store_grad=False)` (Armijo).

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `scale(step_size)`                                                     | constant scale                               |
|  [02]   | `scale_by_learning_rate(learning_rate=None, *, flip_sign=True)`        | descent-sign LR scale                        |
|  [03]   | `scale_by_adam(b1, b2, eps, eps_root)`                                 | Adam moment rescaling                        |
|  [04]   | `scale_by_rms(decay, eps)`                                             | RMS moment rescaling                         |
|  [05]   | `scale_by_rss(...)`                                                    | Adagrad moment rescaling                     |
|  [06]   | `scale_by_schedule(step_size_fn)`                                      | per-step scaling from a `Schedule`           |
|  [07]   | `trace(decay, nesterov=False)`                                         | momentum trace                               |
|  [08]   | `ema(decay, debias=True, accumulator_dtype=None)`                      | exponential moving average                   |
|  [09]   | `scale_by_zoom_linesearch(...)`                                        | Wolfe zoom line search (L-BFGS default)      |
|  [10]   | `scale_by_backtracking_linesearch(...)`                                | Armijo backtracking line search              |
|  [11]   | `clip(max_delta)`                                                      | element clipping                             |
|  [12]   | `clip_by_global_norm(max_norm)`                                        | global-norm clipping                         |
|  [13]   | `clip_by_block_rms(threshold)`                                         | block-RMS clipping                           |
|  [14]   | `adaptive_grad_clip(clipping)`                                         | AGC clipping                                 |
|  [15]   | `add_decayed_weights(weight_decay=0.0, mask=None)`                     | L2 weight decay                              |
|  [16]   | `add_noise(eta, gamma, key)`                                           | annealed Gaussian gradient noise             |
|  [17]   | `centralize()`                                                         | gradient centralization                      |
|  [18]   | `scale_by_trust_ratio(...)`                                            | LARS trust ratio                             |
|  [19]   | `scale_by_param_block_norm(...)`                                       | param-block-norm scaling                     |
|  [20]   | `zero_nans()`                                                          | replaces NaN updates with zero               |
|  [21]   | `keep_params_nonnegative()`                                            | projects params >= 0                         |
|  [22]   | `value_and_grad_from_state(value_fn)` → `Callable[..., (value, grad)]` | recomputes `(value, grad)` from cached state |
|  [23]   | `global_norm(updates)`                                                 | reduction: global L2 norm of updates         |

[ENTRYPOINT_SCOPE]: learning-rate schedules (`optax.schedules`, re-exported at top level)
- shape: each schedule is a factory returning a `Schedule` callable `Callable[[count], scalar]`.

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------------------------- | :----------------------------- |
|  [01]   | `constant_schedule(value)`                                                             | constant                       |
|  [02]   | `linear_schedule(init_value, end_value, transition_steps, transition_begin=0)`         | linear ramp                    |
|  [03]   | `exponential_decay(init_value, transition_steps, decay_rate, staircase=False, ...)`    | geometric decay                |
|  [04]   | `polynomial_schedule(...)`                                                             | polynomial decay               |
|  [05]   | `cosine_decay_schedule(init_value, decay_steps, alpha=0.0)`                            | cosine annealing               |
|  [06]   | `cosine_onecycle_schedule(...)`                                                        | cosine one-cycle               |
|  [07]   | `linear_onecycle_schedule(...)`                                                        | linear one-cycle               |
|  [08]   | `warmup_cosine_decay_schedule(init_value, peak_value, warmup_steps, decay_steps, ...)` | warmup-then-cosine-decay       |
|  [09]   | `warmup_exponential_decay_schedule(...)`                                               | warmup-then-exponential-decay  |
|  [10]   | `piecewise_constant_schedule(init_value, boundaries_and_scales)`                       | step-wise scaled               |
|  [11]   | `sgdr_schedule(...)`                                                                   | SGDR restarts                  |
|  [12]   | `join_schedules(schedules, boundaries)`                                                | concatenated schedule segments |

[ENTRYPOINT_SCOPE]: loss, projection, assignment, perturbation, second-order, and tree-utility submodules

| [INDEX] | [SURFACE]                                                                     | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `losses.l2_loss` / `squared_error` / `huber_loss(.., delta=1.0)` / `log_cosh` | squared / Huber / log-cosh error     |
|  [02]   | `losses.softmax_cross_entropy(logits, labels, axis=-1, where=None)`           | softmax cross-entropy                |
|  [03]   | `losses.softmax_cross_entropy_with_integer_labels`                            | integer-label cross-entropy          |
|  [04]   | `losses.sigmoid_binary_cross_entropy(logits, labels)`                         | binary cross-entropy                 |
|  [05]   | `losses.cosine_distance` / `cosine_similarity` / `kl_divergence`              | similarity / divergence              |
|  [06]   | `losses.ctc_loss` / `make_fenchel_young_loss`                                 | CTC / structured (Fenchel-Young)     |
|  [07]   | `projections.projection_simplex` / `projection_box(x, lower, upper)`          | simplex / box projection             |
|  [08]   | `projections.projection_l1_ball` / `projection_l2_ball`                       | L1-ball / L2-ball projection         |
|  [09]   | `projections.projection_non_negative` / `projection_hypercube`                | non-negative / hypercube projection  |
|  [10]   | `assignment.hungarian_algorithm(cost_matrix)`                                 | optimal linear-sum assignment        |
|  [11]   | `perturbations.make_perturbed_fun(fun, num_samples, sigma, noise=Normal)`     | Monte-Carlo smoothed surrogate       |
|  [12]   | `second_order.hessian_diag(loss, params, ...)` / `hvp(loss, v, params, ...)`  | Hessian diagonal / Hessian-vector    |
|  [13]   | `second_order.fisher_diag(...)`                                               | Fisher diagonal                      |
|  [14]   | `tree_utils.tree_add` / `tree_scale` / `tree_vdot` / `tree_norm`              | leaf-wise arithmetic                 |
|  [15]   | `tree_utils.tree_update_moment` / `tree_bias_correction`                      | moment update / bias correction      |
|  [16]   | `tree_utils.tree_zeros_like` / `tree_random_like`                             | zeros-like / random-like state trees |

[ENTRYPOINT_SCOPE]: `optax.contrib` research optimizers

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `contrib.schedule_free(base_optimizer, learning_rate, ...)`          | Defazio schedule-free wrapper         |
|  [02]   | `contrib.schedule_free_adamw(...)` / `schedule_free_sgd(...)`        | schedule-free AdamW / SGD             |
|  [03]   | `contrib.schedule_free_eval_params(state, params)`                   | eval-time parameter projection        |
|  [04]   | `contrib.prodigy(...)` / `dadapt_adamw(...)`                         | learning-rate-free Prodigy / D-Adapt  |
|  [05]   | `contrib.dog(...)` / `dowg(...)` / `cocob(...)`                      | learning-rate-free DoG / DoWG / COCOB |
|  [06]   | `contrib.muon(...)` / `scale_by_muon(...)`                           | Newton-Schulz Muon                    |
|  [07]   | `contrib.galore(...)` / `sophia(...)` / `ademamix(...)`              | GaLore / Sophia / AdEMAMix            |
|  [08]   | `contrib.sam(...)`                                                   | sharpness-aware minimization          |
|  [09]   | `contrib.reduce_on_plateau(factor=0.1, patience=10, rtol=1e-4, ...)` | plateau LR reduction                  |
|  [10]   | `contrib.split_real_and_imaginary(inner)`                            | complex-split wrapper                 |
|  [11]   | `contrib.mechanize(...)`                                             | Mechanic learning-rate tuner          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace: `optax`; alias optimizers, primitive transformations, combinators, and schedules at top level; losses/projections/assignment/perturbations/second_order/tree_utils/contrib as submodules.
- a `GradientTransformation` is the `(init, update)` NamedTuple: `init(params)` returns the opaque `OptState`; `update(grads, state, params=None)` returns `(updates, new_state)`.
- canonical step: `updates, opt_state = optimizer.update(grads, opt_state, params); params = optax.apply_updates(params, updates)`, adding updates to the params PyTree leaf-wise.
- alias optimizers are a pre-composed `chain` of `scale_by_*` and `scale_by_learning_rate(flip_sign=True)`; `flip_sign` folds the descent sign so `apply_updates` subtracts the scaled gradient, and every alias returns a `GradientTransformationExtraArgs`.
- `learning_rate` accepts a scalar or a `Schedule`; a `Schedule` threads `scale_by_schedule` so the step decays with the iteration count, and `inject_hyperparams` lifts a scalar into a schedule-driven state leaf for runtime tuning.
- `lbfgs`/line-search transforms return `GradientTransformationExtraArgs`; their `update` requires `value`, `grad`, and `value_fn` extras, supplied by wrapping the objective with `value_and_grad_from_state` rather than a bare `jax.grad`.

[STACKING]:
- `optimistix`(`.api/optimistix.md`): `OptaxMinimiser(optim, rtol, atol, norm=max_norm)` lifts any optimizer or `chain` into an `AbstractMinimiser`, so it solves inside `optimistix.minimise` with the `Solution`/`RESULTS` receipt and `ImplicitAdjoint`/`RecursiveCheckpointAdjoint` differentiation rather than a hand-written loop.
- `equinox`(`.api/equinox.md`): the threaded params PyTree is the `equinox.Module` that `equinox.partition`/`combine` split; grads arrive from `equinox.filter_value_and_grad`, so `optax` never sees the static config leaves.
- `diffrax`(`.api/diffrax.md`): `apply_if_finite`/`zero_nans` guard the loop against a non-finite gradient from a diverged inner `diffrax`/`optimistix` solve, and `clip_by_global_norm` bounds the step.
- within-lib: `chain` sequences primitive transforms into a custom optimizer, `multi_transform`/`masked`/`partition` route per-block policy over a heterogeneous design vector, and `projections.projection_box`/`projection_simplex`/`projection_non_negative` (with `keep_params_nonnegative`) fold feasibility after `apply_updates` for projected descent. Whole-optimizer construction happens once outside the JIT loop; only the `(updates, OptState)` pair flows per step.

[LOCAL_ADMISSION]:
- `chain` composes a custom optimizer from `clip_by_global_norm`, a `scale_by_*` moment rescaler, and `scale_by_learning_rate`; a hand-rolled Adam/momentum accumulator loop is rejected.
- `multi_transform`/`masked`/`partition` route distinct transforms to labelled or masked PyTree leaves so a heterogeneous design vector (geometry params versus material density) descends under per-block policy without a parallel optimizer.
- `optax.tree_utils` owns the leaf-wise arithmetic over optimizer-state PyTrees; re-deriving `tree_norm`/`tree_add` over `jax.tree_util` by hand is rejected.

[RAIL_LAW]:
- Package: `optax`
- Owns: composable first-order/accelerated/adaptive/quasi-Newton/projected JAX gradient transformations, learning-rate schedules, and the loss/projection/assignment/perturbation/tree-utility library
- Accept: an alias optimizer or a `chain` of transformations as the descent axis, wrapped by `optimistix.OptaxMinimiser` for the unified solve, or driven directly over an `equinox` PyTree with `apply_updates`, feasibility kept via `optax.projections`
- Reject: a hand-rolled momentum/Adam accumulator loop; re-implemented PyTree math; `optax` for the second-order, trust-region, or implicit-adjoint solves that `optimistix` owns
