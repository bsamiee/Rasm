# [PY_COMPUTE_API_OPTAX]

`optax` supplies composable JAX gradient transformations as the first-order-descent axis of the inverse-design loop. A `GradientTransformation` is an `(init, update)` pair threading an opaque `OptState`; `chain` composes transformations, `apply_updates` applies the transformed step to an `equinox`-parameterized PyTree, and every alias optimizer is a pre-composed `chain` of `scale_by_*` plus `scale_by_learning_rate`. It owns first-order, accelerated, adaptive, quasi-Newton (line-search L-BFGS), and projected descent plus the learning-rate-schedule algebra and a loss/projection/tree-utility library. It stacks two ways: an `optax` optimizer wrapped by `optimistix.OptaxMinimiser` becomes a minimiser inside the unified `minimise` solve (carrying `optimistix` adjoints and `RESULTS`), or it is driven directly over an `equinox` PyTree with `apply_updates` under a `jax.value_and_grad`. The second-order, trust-region, and implicit-adjoint solver routes stay on `optimistix`; it never hand-rolls a moment accumulator the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optax`
- package: `optax`
- import: `optax`; submodules `optax.losses`, `optax.schedules`, `optax.transforms`, `optax.projections`, `optax.assignment`, `optax.perturbations`, `optax.second_order`, `optax.tree_utils`, `optax.contrib`
- owner: `compute`
- rail: first-order gradient-descent optimization
- capability: composable JAX gradient-transformation algebra — `(init, update)` `GradientTransformation`/`GradientTransformationExtraArgs` carriers, ~30 pre-composed alias optimizers, `scale_by_*`/`clip*`/`add_*` primitive transforms, `chain`/`multi_transform`/`masked`/`partition` combinators, zoom/backtracking line searches with `value_and_grad_from_state`, `MultiSteps` accumulation, `inject_hyperparams`, learning-rate schedules, a loss library, constraint projections, and PyTree utilities

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gradient-transformation carriers and state
- rail: first-order gradient-descent optimization

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [KEY_SIGNATURE]                                                                                                                    |
| :-----: | :-------------------------------- | :---------------- | :--------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `GradientTransformation`          | optimizer carrier | NamedTuple `(init: Callable[[Params], OptState], update: Callable[[Updates, OptState, Params?], tuple[Updates, OptState]])`        |
|  [02]   | `GradientTransformationExtraArgs` | optimizer carrier | subtype whose `update(updates, state, params=None, **extra_args)` accepts solver-supplied extras (the line-search/L-BFGS contract) |
|  [03]   | `OptState`                        | opaque state      | PyTree threaded between `update` calls; not introspected by the caller                                                             |
|  [04]   | `Updates` \| `Params`             | type alias        | the PyTree of updates / parameters, structurally parallel                                                                          |
|  [05]   | `Schedule` \| `ScalarOrSchedule`  | type alias        | `Callable[[count], scalar]`; a `learning_rate` accepts a scalar or a `Schedule`                                                    |
|  [06]   | `EmptyState`                      | state             | the unit state for stateless transformations                                                                                       |
|  [07]   | `ScaleByAdamState`                | state             | fields `count`, `mu`, `nu` — Adam first/second moment accumulators                                                                 |
|  [08]   | `MultiSteps` / `MultiStepsState`  | accumulator       | wraps a transformation to apply once every `k` micro-steps (gradient accumulation)                                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: alias optimizers (top-level pre-composed chains)
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                                                                               | [ENTRY_FAMILY]   | [RESULT]                                                                           |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------- | :--------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `sgd(learning_rate, momentum=None, nesterov=False, accumulator_dtype=None)`                                                             | first-order      | SGD (`GradientTransformationExtraArgs`)                                            |
|  [02]   | `adam(learning_rate, b1=0.9, b2=0.999, eps=1e-8, eps_root=0.0, mu_dtype=None, *, nesterov=False)`                                       | adaptive         | Adam                                                                               |
|  [03]   | `adamw(learning_rate, b1=0.9, b2=0.999, eps=1e-8, eps_root=0.0, mu_dtype=None, weight_decay=1e-4, mask=None, *, nesterov=False)`        | adaptive         | decoupled-weight-decay Adam                                                        |
|  [04]   | `nadamw(...)` / `nadam(...)` / `amsgrad(...)` / `radam(..., threshold=5.0)`                                                             | adaptive         | Nesterov-Adam / AMSGrad / rectified-Adam variants                                  |
|  [05]   | `adamax(learning_rate, b1=0.9, b2=0.999, eps=1e-8)` / `adabelief(...)` / `yogi(..., eps=1e-3)`                                          | adaptive         | ∞-norm Adam / belief / Yogi                                                        |
|  [06]   | `adafactor(learning_rate=None, min_dim_size_to_factor=128, decay_rate=0.8, ...)`                                                        | memory-efficient | factored second-moment Adam (no per-element `nu`)                                  |
|  [07]   | `rmsprop(learning_rate, decay=0.9, eps=1e-8, momentum=None, nesterov=False)`                                                            | adaptive         | RMSProp                                                                            |
|  [08]   | `adagrad(learning_rate, initial_accumulator_value=0.1, eps=1e-7)`                                                                       | adaptive         | Adagrad                                                                            |
|  [09]   | `lamb(learning_rate, b1=0.9, b2=0.999, eps=1e-6, weight_decay=0.0, mask=None)` / `lars(...)`                                            | layer-wise       | trust-ratio layer-wise adaptive                                                    |
|  [10]   | `lbfgs(learning_rate=None, memory_size=10, scale_init_precond=True, linesearch=scale_by_zoom_linesearch(...))`                          | quasi-Newton     | L-BFGS (`GradientTransformationExtraArgs`); needs `value`/`grad`/`value_fn` extras |
|  [11]   | `lion(learning_rate, b1=0.9, b2=0.99, mu_dtype=None, weight_decay=1e-3, mask=None)`                                                     | sign-momentum    | Lion                                                                               |
|  [12]   | `novograd(learning_rate, b1=0.9, b2=0.25, eps=1e-6, eps_root=0.0, weight_decay=0.0)`                                                    | adaptive         | NovoGrad                                                                           |
|  [13]   | `fromage(learning_rate, min_norm=1e-6)` / `sm3(learning_rate, momentum=0.9)` / `rprop(learning_rate, eta_minus=0.5, eta_plus=1.2, ...)` | scale-invariant  | Frobenius / SM3 / Rprop                                                            |
|  [14]   | `polyak_sgd(max_learning_rate=1.0, scaling=1.0, f_min=0.0, eps=0.0, variant='sps')`                                                     | adaptive         | Polyak step-size SGD                                                               |
|  [15]   | `optimistic_gradient_descent(learning_rate, alpha=1.0, beta=1.0)` / `sign_sgd(learning_rate)`                                           | game/sign        | extragradient OGD / signSGD                                                        |

[ENTRYPOINT_SCOPE]: combinators, core update, and accumulation
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY] | [RESULT]                                                             |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `chain(*transformations)` → `GradientTransformationExtraArgs`                                                  | combinator     | sequential composition; the alias optimizers are pre-composed chains |
|  [02]   | `named_chain(*name_and_transform_pairs)`                                                                       | combinator     | named sequential composition                                         |
|  [03]   | `apply_updates(params, updates)` → `Params`                                                                    | apply          | leaf-wise `params + updates` over the PyTree                         |
|  [04]   | `incremental_update(new_tensors, old_tensors, step_size)` / `periodic_update`                                  | apply          | Polyak/EMA target-parameter blend                                    |
|  [05]   | `multi_transform(transforms, param_labels, *, mask_compatible_extra_args=False)`                               | combinator     | per-leaf transform routing by label (heterogeneous design vector)    |
|  [06]   | `masked(inner, mask, *, mask_compatible_extra_args=False)` / `partition(transforms, ...)`                      | combinator     | applies `inner` only to masked/partitioned leaves                    |
|  [07]   | `conditionally_transform(inner, should_transform_fn)` / `conditionally_mask(...)`                              | combinator     | predicate-gated transformation                                       |
|  [08]   | `apply_if_finite(inner, max_consecutive_errors)`                                                               | combinator     | skips the step on non-finite updates                                 |
|  [09]   | `MultiSteps(opt, every_k_schedule, use_grad_mean=True, should_skip_update_fn=None, accumulator_dtype=float32)` | accumulation   | gradient accumulation over `k` micro-steps                           |
|  [10]   | `inject_hyperparams(inner_factory, static_args=(), hyperparam_dtype=None)`                                     | combinator     | turns scalar hyperparameters into stateful, schedule-driven leaves   |

[ENTRYPOINT_SCOPE]: gradient transformations (`scale_by_*` / `clip*` / `add_*` / line search)
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                                                             | [ENTRY_FAMILY]  | [RESULT]                                                                           |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `scale(step_size)` / `scale_by_learning_rate(learning_rate=None, *, flip_sign=True)`                                  | transformation  | constant scale / descent-sign LR scale                                             |
|  [02]   | `scale_by_adam(b1, b2, eps, eps_root)` / `scale_by_rms(decay, eps)` / `scale_by_rss(...)`                             | transformation  | Adam / RMS / Adagrad moment rescaling                                              |
|  [03]   | `scale_by_schedule(step_size_fn)`                                                                                     | transformation  | per-step scaling from a `Schedule`                                                 |
|  [04]   | `trace(decay, nesterov=False)` / `ema(decay, debias=True, accumulator_dtype=None)`                                    | transformation  | momentum trace / exponential moving average                                        |
|  [05]   | `scale_by_zoom_linesearch(max_linesearch_steps, max_learning_rate=None, slope_rtol=1e-4, curv_rtol=0.9, ...)`         | line search     | Wolfe-condition zoom line search (the L-BFGS default)                              |
|  [06]   | `scale_by_backtracking_linesearch(max_backtracking_steps, slope_rtol=1e-4, decrease_factor=0.8, store_grad=False)`    | line search     | Armijo backtracking line search                                                    |
|  [07]   | `clip(max_delta)` / `clip_by_global_norm(max_norm)` / `clip_by_block_rms(threshold)` / `adaptive_grad_clip(clipping)` | transformation  | element / global-norm / block-RMS / AGC clipping                                   |
|  [08]   | `add_decayed_weights(weight_decay=0.0, mask=None)` / `add_noise(eta, gamma, key)`                                     | transformation  | L2 weight decay / annealed Gaussian gradient noise                                 |
|  [09]   | `centralize()` / `scale_by_trust_ratio(...)` / `scale_by_param_block_norm(...)`                                       | transformation  | gradient centralization / LARS trust ratio                                         |
|  [10]   | `zero_nans()` / `keep_params_nonnegative()`                                                                           | transformation  | replaces NaN updates with zero / projects params ≥ 0                               |
|  [11]   | `value_and_grad_from_state(value_fn)` → `Callable[..., (value, grad)]`                                                | line-search aid | recomputes `(value, grad)` from cached state for the L-BFGS/line-search extra args |
|  [12]   | `global_norm(updates)`                                                                                                | reduction       | the global L2 norm of an update PyTree                                             |

[ENTRYPOINT_SCOPE]: learning-rate schedules (`optax.schedules`, re-exported at top level)
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                                                                         | [ENTRY_FAMILY] | [RESULT]                         |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `constant_schedule(value)` / `linear_schedule(init_value, end_value, transition_steps, transition_begin=0)`                       | schedule       | constant / linear ramp           |
|  [02]   | `exponential_decay(init_value, transition_steps, decay_rate, staircase=False, ...)` / `polynomial_schedule(...)`                  | schedule       | geometric / polynomial decay     |
|  [03]   | `cosine_decay_schedule(init_value, decay_steps, alpha=0.0)` / `cosine_onecycle_schedule(...)` / `linear_onecycle_schedule(...)`   | schedule       | cosine annealing / one-cycle     |
|  [04]   | `warmup_cosine_decay_schedule(init_value, peak_value, warmup_steps, decay_steps, ...)` / `warmup_exponential_decay_schedule(...)` | schedule       | warmup-then-decay                |
|  [05]   | `piecewise_constant_schedule(init_value, boundaries_and_scales)` / `sgdr_schedule(...)`                                           | schedule       | step-wise scaled / SGDR restarts |
|  [06]   | `join_schedules(schedules, boundaries)`                                                                                           | schedule       | concatenated schedule segments   |

[ENTRYPOINT_SCOPE]: loss, projection, assignment, perturbation, and tree-utility submodules
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                                                                                                               | [ENTRY_FAMILY]        | [RESULT]                                                                |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------- | :---------------------------------------------------------------------- |
|  [01]   | `losses.l2_loss` / `squared_error` / `huber_loss(.., delta=1.0)` / `log_cosh`                                                                                           | regression loss       | squared / Huber / log-cosh error                                        |
|  [02]   | `losses.softmax_cross_entropy(logits, labels, axis=-1, where=None)` / `softmax_cross_entropy_with_integer_labels` / `sigmoid_binary_cross_entropy(logits, labels)`      | classification        | cross-entropy variants                                                  |
|  [03]   | `losses.cosine_distance` / `cosine_similarity` / `kl_divergence` / `ctc_loss` / `make_fenchel_young_loss`                                                               | loss                  | similarity / divergence / CTC / structured                              |
|  [04]   | `projections.projection_simplex` / `projection_box(x, lower, upper)` / `projection_l1_ball` / `projection_l2_ball` / `projection_non_negative` / `projection_hypercube` | constraint projection | Euclidean projection onto a feasible set (constrained inverse design)   |
|  [05]   | `assignment.hungarian_algorithm(cost_matrix)`                                                                                                                           | combinatorial         | optimal linear-sum assignment (matching)                                |
|  [06]   | `perturbations.make_perturbed_fun(fun, num_samples, sigma, noise=Normal)`                                                                                               | smoothing             | Monte-Carlo smoothed/differentiable surrogate of a non-smooth `fun`     |
|  [07]   | `second_order.hessian_diag(loss, params, ...)` / `hvp(loss, v, params, ...)` / `fisher_diag(...)`                                                                       | curvature             | Hessian-diagonal / Hessian-vector-product / Fisher diagonal             |
|  [08]   | `tree_utils.tree_add` / `tree_scale` / `tree_vdot` / `tree_norm` / `tree_update_moment` / `tree_bias_correction` / `tree_zeros_like` / `tree_random_like`               | PyTree math           | leaf-wise arithmetic and moment updates over the optimizer state PyTree |

[ENTRYPOINT_SCOPE]: `optax.contrib` research optimizers
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                                                                                                        | [ENTRY_FAMILY]     | [RESULT]                                                                    |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------- | :-------------------------------------------------------------------------- |
|  [01]   | `contrib.schedule_free(base_optimizer, learning_rate, ...)` / `schedule_free_adamw(...)` / `schedule_free_sgd(...)` / `schedule_free_eval_params(state, params)` | schedule-free      | Defazio schedule-free wrapper + eval-time parameter projection              |
|  [02]   | `contrib.prodigy(...)` / `dadapt_adamw(...)` / `dog(...)` / `dowg(...)` / `cocob(...)`                                                                           | learning-rate-free | parameter-free adaptive step-size optimizers                                |
|  [03]   | `contrib.muon(...)` / `scale_by_muon(...)` / `galore(...)` / `sophia(...)` / `ademamix(...)`                                                                     | matrix/low-rank    | Newton-Schulz Muon / GaLore low-rank / Sophia / AdEMAMix                    |
|  [04]   | `contrib.sam(...)` / `reduce_on_plateau(factor=0.1, patience=10, rtol=1e-4, ...)` / `split_real_and_imaginary(inner)` / `mechanize(...)`                         | meta               | sharpness-aware / plateau-LR / complex-split / Mechanic learning-rate tuner |

## [04]-[IMPLEMENTATION_LAW]

[OPTIMIZER_TOPOLOGY]:
- namespace: `optax`; alias optimizers, primitive transformations, combinators, and schedules at top level; losses/projections/assignment/perturbations/second_order/tree_utils/contrib as submodules.
- a `GradientTransformation` is the `(init, update)` NamedTuple: `init(params)` returns the opaque `OptState`; `update(grads, state, params=None)` returns `(updates, new_state)`.
- the canonical step is `updates, opt_state = optimizer.update(grads, opt_state, params); params = optax.apply_updates(params, updates)`; `apply_updates` adds the updates to the params PyTree leaf-wise.
- alias optimizers are pre-composed `chain` of `scale_by_*` plus `scale_by_learning_rate(flip_sign=True)`; the `flip_sign` folds the descent sign so `apply_updates` subtracts the scaled gradient. All aliases return a `GradientTransformationExtraArgs`.
- `learning_rate` accepts a scalar or a `Schedule` callable; passing a `Schedule` threads `scale_by_schedule` so the step decays with the iteration count; `inject_hyperparams` lifts a scalar into a schedule-driven state leaf for runtime tuning.
- `lbfgs`/line-search transforms return `GradientTransformationExtraArgs`; their `update` requires `value`, `grad`, and `value_fn` extra args, supplied by wrapping the objective with `value_and_grad_from_state` rather than a bare `jax.grad`.

[INTEGRATION_LAW]:
- carrier: the params PyTree the optimizer threads is the same `equinox.Module` (or partitioned filter spec) that `equinox.partition`/`combine` split; grads come from `jax.value_and_grad` / `equinox.filter_value_and_grad` over the objective, so `optax` never sees the static config leaves.
- solver stacking: `optimistix.OptaxMinimiser(optim, rtol, atol, norm=max_norm)` wraps any `optax` optimizer as an `AbstractMinimiser`, so a custom `chain` runs inside the unified `optimistix.minimise` solve — gaining the `Solution`/`RESULTS` receipt, `ImplicitAdjoint`/`RecursiveCheckpointAdjoint` differentiation, and `max_steps` termination — instead of a hand-written training loop.
- constrained design: `projections.projection_box`/`projection_simplex`/`projection_non_negative` are applied after `apply_updates` (projected gradient descent) to keep a design vector (densities ∈ [0,1], a simplex of material fractions) feasible without a penalty term; `keep_params_nonnegative` folds the box constraint into the chain.
- robustness: `apply_if_finite`/`zero_nans` guard the inverse-design loop against a non-finite gradient from a diverged inner `diffrax`/`optimistix` solve; `clip_by_global_norm` bounds the step.
- whole-optimizer construction is done once outside the JIT loop with its hyperparameters; only the `(updates, OptState)` pair flows per step, and the `Schedule`/`MultiSteps` state lives in the threaded `OptState`.

[LOCAL_ADMISSION]:
- `chain` composes a custom optimizer from `clip_by_global_norm`, a `scale_by_*` moment rescaler, and `scale_by_learning_rate`; a hand-rolled Adam/momentum accumulator loop is the rejected form.
- `multi_transform`/`masked`/`partition` route distinct transforms to labelled or masked PyTree leaves so a heterogeneous design vector (geometry params versus material density) descends under per-block policy without a parallel optimizer.
- `optax.tree_utils` owns the leaf-wise arithmetic over optimizer-state PyTrees; re-deriving `tree_norm`/`tree_add` over `jax.tree_util` by hand is rejected.

[RAIL_LAW]:
- Package: `optax`
- Owns: composable first-order/accelerated/adaptive/quasi-Newton/projected JAX gradient transformations, learning-rate schedules, and the loss/projection/assignment/perturbation/tree-utility library
- Accept: an alias optimizer or a `chain` of transformations as the `optax`-descent axis, wrapped by `optimistix.OptaxMinimiser` for the unified solve, or driven directly over an `equinox` PyTree with `apply_updates`, with feasibility kept via `optax.projections`
- Reject: a hand-rolled momentum/Adam accumulator loop; re-implemented PyTree math; `optax` for the second-order, trust-region, or implicit-adjoint solves that `optimistix` owns
