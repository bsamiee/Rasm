# [PY_COMPUTE_API_OPTAX]

`optax` supplies composable JAX gradient transformations as the first-order-descent axis of the inverse-design loop. A `GradientTransformation` is an `(init, update)` pair threading an opaque `OptState`; `chain` composes transformations, `apply_updates` applies the transformed step to an `equinox`-parameterized PyTree, and every alias optimizer is a pre-composed `chain` of `scale_by_*` plus `scale_by_learning_rate`. It owns first-order, accelerated, adaptive, quasi-Newton (line-search L-BFGS), and projected descent plus the learning-rate-schedule algebra and a loss/projection/tree-utility library. It stacks two ways: an `optax` optimizer wrapped by `optimistix.OptaxMinimiser` becomes a minimiser inside the unified `minimise` solve (carrying `optimistix` adjoints and `RESULTS`), or it is driven directly over an `equinox` PyTree with `apply_updates` under a `jax.value_and_grad`. The second-order, trust-region, and implicit-adjoint solver routes stay on `optimistix`; it never hand-rolls a moment accumulator the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `optax`
- package: `optax`
- import: `optax`; submodules `optax.losses`, `optax.schedules`, `optax.transforms`, `optax.projections`, `optax.assignment`, `optax.perturbations`, `optax.second_order`, `optax.tree_utils`, `optax.contrib`
- owner: `compute`
- rail: first-order gradient-descent optimization
- capability: composable JAX gradient-transformation algebra — `(init, update)` `GradientTransformation`/`GradientTransformationExtraArgs` carriers, ~30 pre-composed alias optimizers, `scale_by_*`/`clip*`/`add_*` primitive transforms, `chain`/`multi_transform`/`masked`/`partition` combinators, zoom/backtracking line searches with `value_and_grad_from_state`, `MultiSteps` accumulation, `inject_hyperparams`, learning-rate schedules, a loss library, projections, and PyTree utilities

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: gradient-transformation carriers and state
- rail: first-order gradient-descent optimization

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [KEY_SIGNATURE]                                                                  |
| :-----: | :-------------------------------- | :---------------- | :------------------------------------------------------------------------------- |
|  [01]   | `GradientTransformation`          | optimizer carrier | `(init, update)` NamedTuple; shapes in `[OPTIMIZER_TOPOLOGY]`                    |
|  [02]   | `GradientTransformationExtraArgs` | optimizer carrier | `(init, update)` subtype; `update` takes solver `**extra_args` (L-BFGS contract) |
|  [03]   | `OptState`                        | opaque state      | PyTree threaded between `update` calls; not introspected by the caller           |
|  [04]   | `Updates` \| `Params`             | type alias        | the PyTree of updates / parameters, structurally parallel                        |
|  [05]   | `Schedule` \| `ScalarOrSchedule`  | type alias        | `Callable[[count], scalar]`; a `learning_rate` accepts a scalar or a `Schedule`  |
|  [06]   | `EmptyState`                      | state             | the unit state for stateless transformations                                     |
|  [07]   | `ScaleByAdamState`                | state             | fields `count`, `mu`, `nu` — Adam first/second moment accumulators               |
|  [08]   | `MultiSteps` / `MultiStepsState`  | accumulator       | applies a transformation every `k` micro-steps (gradient accumulation)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: alias optimizers (top-level pre-composed chains)
- rail: first-order gradient-descent optimization
- shared: the Adam base is `(learning_rate, b1=0.9, b2=0.999, eps=1e-8, eps_root=0.0, mu_dtype=None, *, nesterov=False)`; `adam` is exactly it, `adamw` adds `weight_decay=1e-4, mask=None`, and `lbfgs` defaults `linesearch=scale_by_zoom_linesearch(...)`.

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]   | [RESULT]                       |
| :-----: | :----------------------------------------------------------------------------------- | :--------------- | :----------------------------- |
|  [01]   | `sgd(learning_rate, momentum=None, nesterov=False, accumulator_dtype=None)`          | first-order      | SGD                            |
|  [02]   | `adam(...)`                                                                          | adaptive         | Adam (the base)                |
|  [03]   | `adamw(..., weight_decay=1e-4, mask=None)`                                           | adaptive         | decoupled-decay Adam           |
|  [04]   | `nadamw(...)` / `nadam(...)`                                                         | adaptive         | Nesterov-Adam variants         |
|  [05]   | `amsgrad(...)`                                                                       | adaptive         | AMSGrad                        |
|  [06]   | `radam(..., threshold=5.0)`                                                          | adaptive         | rectified Adam                 |
|  [07]   | `adamax(learning_rate, b1=0.9, b2=0.999, eps=1e-8)`                                  | adaptive         | ∞-norm Adam                    |
|  [08]   | `adabelief(...)`                                                                     | adaptive         | belief-Adam                    |
|  [09]   | `yogi(..., eps=1e-3)`                                                                | adaptive         | Yogi                           |
|  [10]   | `adafactor(learning_rate=None, min_dim_size_to_factor=128, decay_rate=0.8, ...)`     | memory-efficient | factored-moment Adam (no `nu`) |
|  [11]   | `rmsprop(learning_rate, decay=0.9, eps=1e-8, momentum=None, nesterov=False)`         | adaptive         | RMSProp                        |
|  [12]   | `adagrad(learning_rate, initial_accumulator_value=0.1, eps=1e-7)`                    | adaptive         | Adagrad                        |
|  [13]   | `lamb(learning_rate, b1=0.9, b2=0.999, eps=1e-6, weight_decay=0.0, mask=None)`       | layer-wise       | trust-ratio layer-wise         |
|  [14]   | `lars(...)`                                                                          | layer-wise       | LARS layer-wise adaptive       |
|  [15]   | `lbfgs(learning_rate=None, memory_size=10, scale_init_precond=True, linesearch=...)` | quasi-Newton     | L-BFGS; needs solver extras    |
|  [16]   | `lion(learning_rate, b1=0.9, b2=0.99, mu_dtype=None, weight_decay=1e-3, mask=None)`  | sign-momentum    | Lion                           |
|  [17]   | `novograd(learning_rate, b1=0.9, b2=0.25, eps=1e-6, eps_root=0.0, weight_decay=0.0)` | adaptive         | NovoGrad                       |
|  [18]   | `fromage(learning_rate, min_norm=1e-6)`                                              | scale-invariant  | Frobenius (Fromage)            |
|  [19]   | `sm3(learning_rate, momentum=0.9)`                                                   | scale-invariant  | SM3                            |
|  [20]   | `rprop(learning_rate, eta_minus=0.5, eta_plus=1.2, ...)`                             | scale-invariant  | Rprop                          |
|  [21]   | `polyak_sgd(max_learning_rate=1.0, scaling=1.0, f_min=0.0, eps=0.0, variant='sps')`  | adaptive         | Polyak step-size SGD           |
|  [22]   | `optimistic_gradient_descent(learning_rate, alpha=1.0, beta=1.0)`                    | game/sign        | extragradient OGD              |
|  [23]   | `sign_sgd(learning_rate)`                                                            | game/sign        | signSGD                        |

[ENTRYPOINT_SCOPE]: combinators, core update, and accumulation
- rail: first-order gradient-descent optimization
- accumulate: `MultiSteps(opt, every_k_schedule, use_grad_mean=True, should_skip_update_fn=None, accumulator_dtype=float32)` accumulates gradients over `k` micro-steps.

| [INDEX] | [SURFACE]                                                                        | [ENTRY_FAMILY] | [RESULT]                             |
| :-----: | :------------------------------------------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `chain(*transformations)` → `GradientTransformationExtraArgs`                    | combinator     | sequential composition               |
|  [02]   | `named_chain(*name_and_transform_pairs)`                                         | combinator     | named sequential composition         |
|  [03]   | `apply_updates(params, updates)` → `Params`                                      | apply          | leaf-wise `params + updates`         |
|  [04]   | `incremental_update(new_tensors, old_tensors, step_size)`                        | apply          | Polyak/EMA target-parameter blend    |
|  [05]   | `periodic_update`                                                                | apply          | periodic target-parameter copy       |
|  [06]   | `multi_transform(transforms, param_labels, *, mask_compatible_extra_args=False)` | combinator     | per-leaf transform routing by label  |
|  [07]   | `masked(inner, mask, *, mask_compatible_extra_args=False)`                       | combinator     | applies `inner` to masked leaves     |
|  [08]   | `partition(transforms, ...)`                                                     | combinator     | routes partitioned leaves            |
|  [09]   | `conditionally_transform(inner, should_transform_fn)`                            | combinator     | predicate-gated transformation       |
|  [10]   | `conditionally_mask(...)`                                                        | combinator     | predicate-gated masking              |
|  [11]   | `apply_if_finite(inner, max_consecutive_errors)`                                 | combinator     | skips the step on non-finite updates |
|  [12]   | `MultiSteps(...)`                                                                | accumulation   | gradient accumulation over `k` steps |
|  [13]   | `inject_hyperparams(inner_factory, static_args=(), hyperparam_dtype=None)`       | combinator     | scalar hyperparams → stateful leaves |

[ENTRYPOINT_SCOPE]: gradient transformations (`scale_by_*` / `clip*` / `add_*` / line search)
- rail: first-order gradient-descent optimization
- line-search: `scale_by_zoom_linesearch(max_linesearch_steps, max_learning_rate=None, slope_rtol=1e-4, curv_rtol=0.9, ...)` (Wolfe zoom, L-BFGS default); `scale_by_backtracking_linesearch(max_backtracking_steps, slope_rtol=1e-4, decrease_factor=0.8, store_grad=False)` (Armijo).

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY]  | [RESULT]                                     |
| :-----: | :--------------------------------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `scale(step_size)`                                                     | transformation  | constant scale                               |
|  [02]   | `scale_by_learning_rate(learning_rate=None, *, flip_sign=True)`        | transformation  | descent-sign LR scale                        |
|  [03]   | `scale_by_adam(b1, b2, eps, eps_root)`                                 | transformation  | Adam moment rescaling                        |
|  [04]   | `scale_by_rms(decay, eps)`                                             | transformation  | RMS moment rescaling                         |
|  [05]   | `scale_by_rss(...)`                                                    | transformation  | Adagrad moment rescaling                     |
|  [06]   | `scale_by_schedule(step_size_fn)`                                      | transformation  | per-step scaling from a `Schedule`           |
|  [07]   | `trace(decay, nesterov=False)`                                         | transformation  | momentum trace                               |
|  [08]   | `ema(decay, debias=True, accumulator_dtype=None)`                      | transformation  | exponential moving average                   |
|  [09]   | `scale_by_zoom_linesearch(...)`                                        | line search     | Wolfe zoom line search (L-BFGS default)      |
|  [10]   | `scale_by_backtracking_linesearch(...)`                                | line search     | Armijo backtracking line search              |
|  [11]   | `clip(max_delta)`                                                      | transformation  | element clipping                             |
|  [12]   | `clip_by_global_norm(max_norm)`                                        | transformation  | global-norm clipping                         |
|  [13]   | `clip_by_block_rms(threshold)`                                         | transformation  | block-RMS clipping                           |
|  [14]   | `adaptive_grad_clip(clipping)`                                         | transformation  | AGC clipping                                 |
|  [15]   | `add_decayed_weights(weight_decay=0.0, mask=None)`                     | transformation  | L2 weight decay                              |
|  [16]   | `add_noise(eta, gamma, key)`                                           | transformation  | annealed Gaussian gradient noise             |
|  [17]   | `centralize()`                                                         | transformation  | gradient centralization                      |
|  [18]   | `scale_by_trust_ratio(...)`                                            | transformation  | LARS trust ratio                             |
|  [19]   | `scale_by_param_block_norm(...)`                                       | transformation  | param-block-norm scaling                     |
|  [20]   | `zero_nans()`                                                          | transformation  | replaces NaN updates with zero               |
|  [21]   | `keep_params_nonnegative()`                                            | transformation  | projects params ≥ 0                          |
|  [22]   | `value_and_grad_from_state(value_fn)` → `Callable[..., (value, grad)]` | line-search aid | recomputes `(value, grad)` from cached state |
|  [23]   | `global_norm(updates)`                                                 | reduction       | global L2 norm of an update PyTree           |

[ENTRYPOINT_SCOPE]: learning-rate schedules (`optax.schedules`, re-exported at top level)
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                              | [RESULT]                       |
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

[ENTRYPOINT_SCOPE]: loss, projection, assignment, perturbation, and tree-utility submodules
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY]  | [RESULT]                             |
| :-----: | :---------------------------------------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `losses.l2_loss` / `squared_error` / `huber_loss(.., delta=1.0)` / `log_cosh` | regression loss | squared / Huber / log-cosh error     |
|  [02]   | `losses.softmax_cross_entropy(logits, labels, axis=-1, where=None)`           | classification  | softmax cross-entropy                |
|  [03]   | `losses.softmax_cross_entropy_with_integer_labels`                            | classification  | integer-label cross-entropy          |
|  [04]   | `losses.sigmoid_binary_cross_entropy(logits, labels)`                         | classification  | binary cross-entropy                 |
|  [05]   | `losses.cosine_distance` / `cosine_similarity` / `kl_divergence`              | loss            | similarity / divergence              |
|  [06]   | `losses.ctc_loss` / `make_fenchel_young_loss`                                 | loss            | CTC / structured (Fenchel-Young)     |
|  [07]   | `projections.projection_simplex` / `projection_box(x, lower, upper)`          | projection      | simplex / box projection             |
|  [08]   | `projections.projection_l1_ball` / `projection_l2_ball`                       | projection      | L1-ball / L2-ball projection         |
|  [09]   | `projections.projection_non_negative` / `projection_hypercube`                | projection      | non-negative / hypercube projection  |
|  [10]   | `assignment.hungarian_algorithm(cost_matrix)`                                 | combinatorial   | optimal linear-sum assignment        |
|  [11]   | `perturbations.make_perturbed_fun(fun, num_samples, sigma, noise=Normal)`     | smoothing       | Monte-Carlo smoothed surrogate       |
|  [12]   | `second_order.hessian_diag(loss, params, ...)` / `hvp(loss, v, params, ...)`  | curvature       | Hessian diagonal / Hessian-vector    |
|  [13]   | `second_order.fisher_diag(...)`                                               | curvature       | Fisher diagonal                      |
|  [14]   | `tree_utils.tree_add` / `tree_scale` / `tree_vdot` / `tree_norm`              | PyTree math     | leaf-wise arithmetic                 |
|  [15]   | `tree_utils.tree_update_moment` / `tree_bias_correction`                      | PyTree math     | moment update / bias correction      |
|  [16]   | `tree_utils.tree_zeros_like` / `tree_random_like`                             | PyTree math     | zeros-like / random-like state trees |

[ENTRYPOINT_SCOPE]: `optax.contrib` research optimizers
- rail: first-order gradient-descent optimization

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]     | [RESULT]                       |
| :-----: | :------------------------------------------------------------------- | :----------------- | :----------------------------- |
|  [01]   | `contrib.schedule_free(base_optimizer, learning_rate, ...)`          | schedule-free      | Defazio schedule-free wrapper  |
|  [02]   | `contrib.schedule_free_adamw(...)` / `schedule_free_sgd(...)`        | schedule-free      | schedule-free AdamW / SGD      |
|  [03]   | `contrib.schedule_free_eval_params(state, params)`                   | schedule-free      | eval-time parameter projection |
|  [04]   | `contrib.prodigy(...)` / `dadapt_adamw(...)`                         | learning-rate-free | Prodigy / D-Adaptation AdamW   |
|  [05]   | `contrib.dog(...)` / `dowg(...)` / `cocob(...)`                      | learning-rate-free | DoG / DoWG / COCOB             |
|  [06]   | `contrib.muon(...)` / `scale_by_muon(...)`                           | matrix/low-rank    | Newton-Schulz Muon             |
|  [07]   | `contrib.galore(...)` / `sophia(...)` / `ademamix(...)`              | matrix/low-rank    | GaLore / Sophia / AdEMAMix     |
|  [08]   | `contrib.sam(...)`                                                   | meta               | sharpness-aware minimization   |
|  [09]   | `contrib.reduce_on_plateau(factor=0.1, patience=10, rtol=1e-4, ...)` | meta               | plateau LR reduction           |
|  [10]   | `contrib.split_real_and_imaginary(inner)`                            | meta               | complex-split wrapper          |
|  [11]   | `contrib.mechanize(...)`                                             | meta               | Mechanic learning-rate tuner   |

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
