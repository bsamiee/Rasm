# [PY_COMPUTE_API_BLACKJAX]

`blackjax` supplies JAX-native sampling algorithms — gradient and gradient-free MCMC, sequential Monte Carlo, stochastic-gradient MCMC, variational inference, step-size and mass-matrix adaptation, and convergence diagnostics — for the compute Bayesian-study rail as the fourth JAX sampler backend beside `numpyro`, `pymc`, and `nutpie`. Every algorithm follows one stateless shape: a top-level factory returns a `SamplingAlgorithm(init, step)` (or `VIAlgorithm(init, step, sample)`) pair the caller drives with `jax.lax.scan`; the package owner never re-implements a kernel, integrator, or resampler the package owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `blackjax`
- package: `blackjax`
- import: `blackjax`; submodules `blackjax.mcmc`, `blackjax.smc`, `blackjax.sgmcmc`, `blackjax.vi`, `blackjax.adaptation`, `blackjax.diagnostics`, `blackjax.util`
- owner: `compute`
- rail: Bayesian-study
- installed: cp313 only (manifest gate `blackjax; python_version<'3.15'`; jaxlib ships no cp315 wheel — companion band beside `numpyro`/`nutpie`)
- capability: stateless JAX sampling algebra — euclidean/Riemannian HMC and NUTS, MALA/Barker/RMH random walk, MCLMC, SMC tempering, stochastic-gradient MCMC, mean-field/full-rank/Pathfinder variational inference, window/MEADS/ChEES/Pathfinder adaptation, and ESS/R-hat diagnostics

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: algorithm carriers (`blackjax.base`)
- rail: Bayesian-study

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [ROLE]                                                                              |
| :-----: | :-------------------- | :------------ | :---------------------------------------------------------------------------------- |
|   [1]   | `SamplingAlgorithm`   | NamedTuple    | sampler pair `(init, step)`; `init(position, rng_key=None)`, `step(rng_key, state)` |
|   [2]   | `VIAlgorithm`         | NamedTuple    | variational triple `(init, step, sample)`                                           |
|   [3]   | `AdaptationAlgorithm` | NamedTuple    | adaptation wrapper `(run,)`; `run(rng_key, position, num_steps)`                    |

[PUBLIC_TYPE_SCOPE]: top-level builders (`blackjax.<name>`)
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                 | [BUILD_PATTERN]  | [ROLE]                                                                   |
| :-----: | :----------------------- | :--------------- | :----------------------------------------------------------------------- |
|   [1]   | `GenerateSamplingAPI`    | callable wrapper | exposes `.init`, `.build_kernel`, `.differentiable` on a sampler builder |
|   [2]   | `GenerateVariationalAPI` | callable wrapper | same triple for a variational builder                                    |
|   [3]   | `GeneratePathfinderAPI`  | callable wrapper | Pathfinder builder exposing `.approximate` and `.sample`                 |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: gradient and gradient-free MCMC (`blackjax.mcmc`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|   [1]   | `hmc(logdensity_fn, step_size, inverse_mass_matrix, num_integration_steps, *, divergence_threshold=1000, integrator)`                            | gradient       | euclidean Hamiltonian Monte Carlo                    |
|   [2]   | `nuts(logdensity_fn, step_size, inverse_mass_matrix, *, max_num_doublings=10, divergence_threshold=1000, integrator)`                            | gradient       | No-U-Turn Sampler                                    |
|   [3]   | `dynamic_hmc(logdensity_fn, step_size, inverse_mass_matrix, *, divergence_threshold=1000, integrator, next_random_arg_fn, integration_steps_fn)` | gradient       | HMC with stochastic integration length               |
|   [4]   | `mala(logdensity_fn, step_size)`                                                                                                                 | gradient       | Metropolis-adjusted Langevin algorithm               |
|   [5]   | `ghmc(logdensity_fn, step_size, momentum_inverse_scale, alpha, delta, *, divergence_threshold=1000, noise_gn)`                                   | gradient       | generalized (persistent-momentum) HMC                |
|   [6]   | `barker(logdensity_fn, step_size, inverse_mass_matrix=None)`                                                                                     | gradient       | Barker proposal Metropolis-Hastings                  |
|   [7]   | `rmhmc(logdensity_fn, ...)`                                                                                                                      | gradient       | Riemannian-manifold HMC                              |
|   [8]   | `mclmc(logdensity_fn, L, step_size, integrator, inverse_mass_matrix=1.0, desired_energy_var_max_ratio=inf)`                                      | gradient-free  | microcanonical Langevin Monte Carlo                  |
|   [9]   | `rmh(logdensity_fn, proposal_generator, proposal_logdensity_fn=None)`                                                                            | gradient-free  | generic random-walk Metropolis-Hastings              |
|  [10]   | `additive_step_random_walk(logdensity_fn, random_step)`                                                                                          | gradient-free  | additive-step random walk                            |
|  [11]   | `normal_random_walk(logdensity_fn, sigma)`                                                                                                       | gradient-free  | Gaussian random-walk convenience builder             |
|  [12]   | `elliptical_slice(loglikelihood_fn, *, mean, cov)`                                                                                               | gradient-free  | elliptical slice sampling under a Gaussian prior     |
|  [13]   | `marginal_latent_gaussian`                                                                                                                       | gradient-free  | marginal sampler for latent-Gaussian models          |
|  [14]   | `irmh` / `orbital_hmc` / `periodic_orbital` / `mgrad_gaussian`                                                                                   | specialized    | independent-RMH, orbital HMC, marginal-grad Gaussian |

[ENTRYPOINT_SCOPE]: sequential Monte Carlo (`blackjax.smc`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                                                                               | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|   [1]   | `adaptive_tempered_smc(logprior_fn, loglikelihood_fn, mcmc_step_fn, mcmc_init_fn, mcmc_parameters, resampling_fn, target_ess, root_solver, num_mcmc_steps=10, **extra)` | tempering      | adaptive-temperature SMC targeting an ESS               |
|   [2]   | `tempered_smc(logprior_fn, loglikelihood_fn, mcmc_step_fn, mcmc_init_fn, mcmc_parameters, resampling_fn, num_mcmc_steps=10, update_strategy, update_particles_fn=None)` | tempering      | fixed-schedule tempered SMC                             |
|   [3]   | `partial_posteriors_smc(mcmc_step_fn, mcmc_init_fn, mcmc_parameters, resampling_fn, num_mcmc_steps, partial_logposterior_factory, update_strategy)`                     | data tempering | partial-posterior (data-tempered) SMC                   |
|   [4]   | `inner_kernel_tuning(...)` / `pretuning`                                                                                                                                | tuning         | per-step inner-kernel parameter tuning                  |
|   [5]   | `smc.resampling.{systematic, stratified, multinomial, residual}(rng_key, weights, num_samples)`                                                                         | resampler      | particle resampling schemes for the SMC `resampling_fn` |

[ENTRYPOINT_SCOPE]: stochastic-gradient MCMC (`blackjax.sgmcmc`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------- |
|   [1]   | `sgld(grad_estimator)`                                                                                      | SG-MCMC        | stochastic-gradient Langevin dynamics         |
|   [2]   | `sghmc(grad_estimator, num_integration_steps=10, alpha=0.01, beta=0)`                                       | SG-MCMC        | stochastic-gradient HMC                       |
|   [3]   | `sgnht(grad_estimator, alpha=0.01, beta=0.0)`                                                               | SG-MCMC        | stochastic-gradient Nosé-Hoover thermostat    |
|   [4]   | `csgld(logdensity_estimator, gradient_estimator, zeta=1, num_partitions=512, energy_gap=100, min_energy=0)` | SG-MCMC        | contour stochastic-gradient Langevin dynamics |

[ENTRYPOINT_SCOPE]: variational inference (`blackjax.vi`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                                                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------- |
|   [1]   | `meanfield_vi(logdensity_fn, optimizer, num_samples=100, objective=KL(), stl_estimator=True)`                                                                                   | Gaussian VI    | diagonal-covariance mean-field ADVI                |
|   [2]   | `fullrank_vi(logdensity_fn, optimizer, num_samples=100, objective=KL(), stl_estimator=True)`                                                                                    | Gaussian VI    | full-covariance Gaussian ADVI                      |
|   [3]   | `svgd(grad_logdensity_fn, optimizer, kernel=rbf_kernel, update_kernel_parameters=update_median_heuristic)`                                                                      | particle VI    | Stein variational gradient descent                 |
|   [4]   | `pathfinder(logdensity_fn)` / `pathfinder.approximate(rng_key, logdensity_fn, initial_position, num_samples=200, *, maxiter=30, maxcor=10, maxls=1000, gtol=1e-08, ftol=1e-05)` | Laplace path   | quasi-Newton Pathfinder approximation              |
|   [5]   | `multipathfinder` / `schrodinger_follmer(logdensity_fn, n_steps, n_inner_samples)`                                                                                              | path VI        | multi-path Pathfinder; Schrödinger-Föllmer sampler |

[ENTRYPOINT_SCOPE]: adaptation, diagnostics, and the inference loop
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                                                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                                         |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|   [1]   | `window_adaptation(algorithm, logdensity_fn, is_mass_matrix_diagonal=True, initial_step_size=1.0, target_acceptance_rate=0.8, progress_bar=False, **extra)`                   | adaptation     | Stan-style step-size and mass-matrix warmup                          |
|   [2]   | `meads_adaptation(logdensity_fn, num_chains, num_folds=4, step_size_multiplier=0.5, damping_slowdown=1.0)`                                                                    | adaptation     | MEADS cross-chain GHMC adaptation                                    |
|   [3]   | `chees_adaptation(logdensity_fn, num_chains, *, jitter_generator=None, jitter_amount=1.0, target_acceptance_rate=0.651, decay_rate=0.5, max_leapfrog_steps=1000)`             | adaptation     | ChEES-HMC trajectory-length adaptation                               |
|   [4]   | `pathfinder_adaptation(algorithm, logdensity_fn, initial_step_size=1.0, target_acceptance_rate=0.8, **extra)`                                                                 | adaptation     | Pathfinder-initialized HMC warmup                                    |
|   [5]   | `mclmc_find_L_and_step_size(mclmc_kernel, num_steps, state, rng_key, frac_tune1=0.1, frac_tune2=0.1, frac_tune3=0.1, desired_energy_var=5e-4, diagonal_preconditioning=True)` | tuning         | MCLMC step-size and trajectory-length tuner                          |
|   [6]   | `AdaptationAlgorithm.run(rng_key, position, num_steps=1000)`                                                                                                                  | adaptation run | returns `(last_state, parameters)` for the sampler                   |
|   [7]   | `diagnostics.effective_sample_size(input_array, chain_axis=0, sample_axis=1)` / `ess(...)`                                                                                    | diagnostic     | effective sample size across chains                                  |
|   [8]   | `diagnostics.potential_scale_reduction(input_array, chain_axis=0, sample_axis=1)` / `rhat(...)`                                                                               | diagnostic     | Gelman-Rubin R-hat convergence statistic                             |
|   [9]   | `util.run_inference_algorithm(rng_key, inference_algorithm, num_steps, initial_state=None, initial_position=None, progress_bar=False, transform)`                             | inference loop | drives `init`/`step` via `lax.scan`, returns final state and history |

## [4]-[IMPLEMENTATION_LAW]

[ALGORITHM_TOPOLOGY]:
- Every sampler is a pure pair: `algorithm = blackjax.nuts(logdensity_fn, step_size, inverse_mass_matrix)` yields `algorithm.init(position)` to seed state and `algorithm.step(rng_key, state)` to advance one transition; neither closes over mutable state.
- The canonical loop is `jax.lax.scan` over `algorithm.step`, or `blackjax.util.run_inference_algorithm` which wraps the scan and returns the final state plus stacked history.
- `inverse_mass_matrix` accepts a `Metric`, a dense/diagonal `jax.Array`, or a callable; `dynamic_hmc` randomizes integration length through `integration_steps_fn`.
- Builders expose `.init`, `.build_kernel`, and `.differentiable`; call `.build_kernel(integrator=..., divergence_threshold=...)` to obtain a bare transition kernel for custom loops.

[ADAPTATION_ROUTING]:
- `window_adaptation(blackjax.nuts, logdensity_fn).run(rng_key, position, num_steps)` returns `(last_state, parameters)`; feed `parameters` (`step_size`, `inverse_mass_matrix`) into the production sampler builder, then sample.
- `chees_adaptation` and `meads_adaptation` adapt across `num_chains` parallel chains for vectorized warmup; `mclmc_find_L_and_step_size` tunes the MCLMC `L` and `step_size` before the sampling phase.
- SMC composes existing MCMC builders: pass an MCMC `step`/`init` pair plus a `resampling_fn` from `smc.resampling` into `adaptive_tempered_smc` or `tempered_smc`.

[STUDY_ROUTING]:
- A `NumericIntent` Bayesian study defines a `logdensity_fn`, warms up through an `AdaptationAlgorithm`, samples with the tuned `SamplingAlgorithm`, and captures `effective_sample_size`/`potential_scale_reduction` as the study receipt — the same receipt contract `numpyro`, `pymc`, and `nutpie` satisfy.
- `blackjax` is the JAX-native `SamplerBackend` selected when the model is differentiable JAX and the study wants explicit kernel/adaptation control; `numpyro` owns effect-handler model authoring, `nutpie` owns compiled NUTS throughput, `pymc` owns the PyMC model DSL.
- No production runtime imports blackjax; inference is offline study work feeding the C# `Rasm.Compute` model rail.

[RAIL_LAW]:
- Package: `blackjax`
- Owns: stateless JAX sampling algebra — MCMC/SMC/SG-MCMC/VI builders, step-size and mass-matrix adaptation, and convergence diagnostics
- Accept: a `NumericIntent` Bayesian study that builds a `SamplingAlgorithm`, warms up via an `AdaptationAlgorithm`, drives the kernel with `lax.scan` or `run_inference_algorithm`, and captures ESS/R-hat as a receipt
- Reject: hand-rolled HMC integrators, resamplers, or warmup schedules blackjax owns; blackjax in any product runtime path; posterior claims without a captured sampler-state and diagnostic receipt
