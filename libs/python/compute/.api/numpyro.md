# [PY_COMPUTE_API_NUMPYRO]

`numpyro` supplies probabilistic programming primitives, distribution families, effect handlers, and JAX-native inference engines for the compute Bayesian-study rail. The package owner declares a model with `sample`, `plate`, and `param` sites, transforms it with effect handlers, then runs inference through `MCMC(NUTS(...))` or `SVI(model, guide, optim, loss)`; it never re-implements a sampler or distribution the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpyro`
- package: `numpyro`
- import: `numpyro`; submodules `numpyro.distributions`, `numpyro.infer`, `numpyro.handlers`, `numpyro.optim`, `numpyro.diagnostics`
- owner: `compute`
- rail: Bayesian-study
- installed: cp313 only (manifest pin `numpyro>=0.21.0; python_version<'3.15'`; jaxlib ships no cp315 wheel)
- capability: effect-handler probabilistic programming on JAX — distribution families, MCMC (NUTS/HMC/SA/ensemble), variational inference (SVI/ELBO/autoguide), posterior-predictive sampling, and convergence diagnostics

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: MCMC inference family (`numpyro.infer`)
- rail: Bayesian-study

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]        | [CAPABILITY]                                                                                             |
| :-----: | :----------------- | :-------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `MCMC`             | MCMC runner           | `(sampler, num_warmup, num_samples, num_chains, chain_method)`                                           |
|  [02]   | `NUTS`             | gradient sampler      | `(model, step_size, adapt_step_size, adapt_mass_matrix, dense_mass, target_accept_prob, max_tree_depth)` |
|  [03]   | `HMC`              | gradient sampler      | Hamiltonian Monte Carlo with tunable path length                                                         |
|  [04]   | `SA`               | gradient-free sampler | Sample Adaptive algorithm                                                                                |
|  [05]   | `BarkerMH`         | gradient sampler      | Barker Metropolis-Hastings                                                                               |
|  [06]   | `AIES`             | ensemble sampler      | affine-invariant ensemble sampler                                                                        |
|  [07]   | `DiscreteHMCGibbs` | mixed sampler         | Gibbs step for discrete variables plus HMC                                                               |
|  [08]   | `HMCGibbs`         | mixed sampler         | HMC inner kernel with Gibbs outer step                                                                   |
|  [09]   | `MixedHMC`         | mixed sampler         | mixed continuous/discrete HMC                                                                            |
|  [10]   | `HMCECS`           | scalable sampler      | HMC with energy-conserving subsampling                                                                   |
|  [11]   | `Predictive`       | posterior sampling    | `(model, posterior_samples, guide, params, num_samples, return_sites)`                                   |

[PUBLIC_TYPE_SCOPE]: variational inference family (`numpyro.infer`)
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                             | [PACKAGE_ROLE]      | [CAPABILITY]                                   |
| :-----: | :----------------------------------- | :------------------ | :--------------------------------------------- |
|  [01]   | `SVI`                                | VI runner           | `(model, guide, optim, loss, **static_kwargs)` |
|  [02]   | `Trace_ELBO`                         | ELBO objective      | standard single-sample ELBO                    |
|  [03]   | `TraceMeanField_ELBO`                | ELBO objective      | mean-field KL closed-form ELBO                 |
|  [04]   | `TraceGraph_ELBO`                    | ELBO objective      | ELBO with variance-reduced score function      |
|  [05]   | `TraceEnum_ELBO`                     | ELBO objective      | ELBO with discrete-variable enumeration        |
|  [06]   | `RenyiELBO`                          | ELBO objective      | Renyi alpha-divergence ELBO bound              |
|  [07]   | `ESS`                                | importance sampling | effective sample size for importance weights   |
|  [08]   | `autoguide.AutoNormal`               | guide automate      | diagonal-normal mean-field guide               |
|  [09]   | `autoguide.AutoDelta`                | guide automate      | MAP point-estimate guide                       |
|  [10]   | `autoguide.AutoMultivariateNormal`   | guide automate      | full-covariance Gaussian guide                 |
|  [11]   | `autoguide.AutoLaplaceApproximation` | guide automate      | Laplace approximation around the mode          |

[PUBLIC_TYPE_SCOPE]: distribution families (`numpyro.distributions`)
- rail: Bayesian-study

| [INDEX] | [SYMBOL]             | [DISTRIBUTION_CLASS] | [SUPPORT]                        |
| :-----: | :------------------- | :------------------- | :------------------------------- |
|  [01]   | `Normal`             | continuous           | real line                        |
|  [02]   | `Beta`               | continuous           | (0, 1)                           |
|  [03]   | `Gamma`              | continuous           | positive reals                   |
|  [04]   | `StudentT`           | continuous           | real line                        |
|  [05]   | `Exponential`        | continuous           | positive reals                   |
|  [06]   | `HalfNormal`         | continuous           | non-negative reals               |
|  [07]   | `HalfCauchy`         | continuous           | non-negative reals               |
|  [08]   | `LogNormal`          | continuous           | positive reals                   |
|  [09]   | `Uniform`            | continuous           | bounded interval                 |
|  [10]   | `Dirichlet`          | multivariate         | probability simplex              |
|  [11]   | `MultivariateNormal` | multivariate         | real vector space                |
|  [12]   | `LKJCholesky`        | matrix               | Cholesky of correlation matrices |
|  [13]   | `Bernoulli`          | discrete             | {0, 1}                           |
|  [14]   | `Binomial`           | discrete             | non-negative integers            |
|  [15]   | `Categorical`        | discrete             | finite set                       |
|  [16]   | `Poisson`            | discrete             | non-negative integers            |
|  [17]   | `NegativeBinomial2`  | discrete             | non-negative integers            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model primitive sites (`numpyro.<fn>`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `sample(name, fn, obs, rng_key, sample_shape, infer, obs_mask)` | sample site    | declares a random variable and records the site |
|  [02]   | `plate(name, size, subsample_size, dim)`                        | independence   | declares a conditional-independence plate       |
|  [03]   | `plate_stack(prefix, sizes, rightmost_dim)`                     | nested plate   | stacks plates over the rightmost dimensions     |
|  [04]   | `param(name, init_value, **kwargs)`                             | VI parameter   | declares a variational parameter site           |
|  [05]   | `deterministic(name, value)`                                    | deterministic  | records a deterministic function of latents     |
|  [06]   | `factor(name, log_factor)`                                      | log factor     | adds an arbitrary log factor to the joint       |
|  [07]   | `subsample(data, event_dim)`                                    | subsampling    | subsamples data for mini-batch inference        |
|  [08]   | `module(name, nn, input_shape)`                                 | neural module  | registers a parameterized neural network site   |
|  [09]   | `prng_key()`                                                    | RNG access     | draws a PRNG key inside an effectful model      |
|  [10]   | `render_model(model, model_args, model_kwargs, ...)`            | model graph    | renders the model as a graphical-model diagram  |

[ENTRYPOINT_SCOPE]: effect handlers and inference operations
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `handlers.seed(fn, rng_seed)`                               | effect handler | supplies a PRNG seed to a model's sample sites            |
|  [02]   | `handlers.trace(fn)`                                        | effect handler | records every site's value, distribution, and log-density |
|  [03]   | `handlers.substitute(fn, data)`                             | effect handler | replaces site values with supplied data                   |
|  [04]   | `handlers.condition(fn, data)`                              | effect handler | conditions latent sites on observed values                |
|  [05]   | `handlers.replay(fn, trace)`                                | effect handler | replays sites from a recorded trace                       |
|  [06]   | `handlers.block(fn, hide, expose)`                          | effect handler | hides sites from outer handlers                           |
|  [07]   | `handlers.reparam(fn, config)`                              | effect handler | applies a reparameterization to named sites               |
|  [08]   | `handlers.do(fn, data)`                                     | effect handler | applies a causal intervention on sites                    |
|  [09]   | `MCMC.run(rng_key, *args)` / `.get_samples()`               | inference run  | warms up, samples, then returns posterior draws           |
|  [10]   | `SVI.run(rng_key, num_steps)` / `.get_params()`             | inference run  | optimizes the guide, returns the optimized parameters     |
|  [11]   | `diagnostics.effective_sample_size(x)` / `.gelman_rubin(x)` | diagnostic     | ESS and R-hat convergence statistics                      |
|  [12]   | `infer.init_to_median` family                               | init strategy  | `init_to_uniform/feasible/mean/median/sample/value`       |
|  [13]   | `enable_x64()` / `set_platform(p)`                          | runtime config | enables 64-bit precision; selects the JAX backend device  |

## [04]-[IMPLEMENTATION_LAW]

[EFFECT_TOPOLOGY]:
- A model is a plain Python function calling `sample`, `param`, `plate`, and `deterministic`; effect handlers are context managers or callables that intercept every site.
- `MCMC(NUTS(model), num_warmup, num_samples).run(rng_key)` warms up, samples, and stores draws; `.get_samples()` returns a site-keyed dict of posterior arrays.
- `SVI(model, guide, optim, loss).run(rng_key, num_steps)` returns an `SVIRunResult` carrying optimized params and the loss trace; `Predictive` then draws posterior-predictive samples.
- `enable_x64()` switches JAX to 64-bit floats before model construction; sampler precision and mass-matrix conditioning depend on it.
- `init_to_*` strategies seed the warmup; `dense_mass=True` on `NUTS` learns a full mass matrix at higher per-step cost.

[STUDY_ROUTING]:
- A `NumericIntent` Bayesian study defines a model function, runs it through `MCMC(NUTS(...))` or `SVI`, and captures posterior samples plus `effective_sample_size`/`gelman_rubin` diagnostics as the study receipt.
- Effect handlers compose: `seed` wraps `trace` wraps `substitute` to inspect a model deterministically without an MCMC run.
- No production runtime imports numpyro; inference is offline study work feeding the C# `Rasm.Compute` model rail.

[RAIL_LAW]:
- Package: `numpyro`
- Owns: JAX-native probabilistic programming — model effect handlers, MCMC/VI inference engines, distribution families, and posterior-predictive sampling
- Accept: a `NumericIntent` Bayesian study run through `MCMC(NUTS(...))` or `SVI(model, guide, optim, loss)`, with posterior samples and convergence diagnostics captured as a receipt
- Reject: hand-rolled Metropolis or VI objectives numpyro owns; numpyro in any product runtime path; posterior claims without a captured MCMC or SVI result receipt
