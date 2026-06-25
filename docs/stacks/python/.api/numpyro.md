# [PY_COMPUTE_API_NUMPYRO]

`numpyro` supplies probabilistic programming primitives, distribution families, effect handlers, and JAX-native inference engines for the compute Bayesian-study rail. The package owner declares a model with `sample`, `plate`, and `param` sites, transforms it with effect handlers, then runs inference through `MCMC(NUTS(...))` or `SVI(model, guide, optim, loss)`; it never re-implements a sampler or distribution the package owns. numpyro is a JAX program: every model runs under a `jax.random.PRNGKey`, draws are JAX arrays, and `enable_x64()` toggles the same float64 flag the sibling `jax`/`diffrax`/`optimistix` rails depend on. Its posterior graduates through `arviz.from_numpyro` (MCMC) or `arviz.from_numpyro_svi` (SVI) into the backend-agnostic `xarray.DataTree` the `arviz` catalog owns; its SVI optimizer is an `optax` chain wrapped by `optim.optax_to_numpyro`; and `blackjax` is the lower-level JAX sampler sibling for kernels numpyro does not expose.

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

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]        | [CAPABILITY]                                                                                                                                                              |
| :-----: | :----------------- | :-------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `MCMC`             | MCMC runner           | `(sampler, *, num_warmup, num_samples, num_chains=1, thinning=1, postprocess_fn, chain_method='parallel', progress_bar=True, jit_model_args=False)`                      |
|  [02]   | `NUTS`             | gradient sampler      | `(model=None, potential_fn=None, *, step_size=1.0, adapt_step_size=True, adapt_mass_matrix=True, dense_mass=False, target_accept_prob=0.8, max_tree_depth=10, init_strategy, forward_mode_differentiation=False)` |
|  [03]   | `HMC`              | gradient sampler      | `(model, *, step_size, num_steps, trajectory_length, adapt_step_size, adapt_mass_matrix, dense_mass, target_accept_prob)` — fixed-path HMC                              |
|  [04]   | `SA`               | gradient-free sampler | Sample Adaptive algorithm; no gradients, tunes from an adaptive proposal                                                                                                 |
|  [05]   | `BarkerMH`         | gradient sampler      | Barker Metropolis-Hastings; robust to step-size mis-scaling                                                                                                              |
|  [06]   | `AIES`             | ensemble sampler      | affine-invariant ensemble (emcee-style); requires `num_chains>1`, `chain_method='vectorized'`                                                                           |
|  [07]   | `ESS` (`infer.ensemble`) | ensemble sampler | ensemble slice sampler; vectorized-chain affine-invariant alternative to `AIES`                                                                                          |
|  [08]   | `DiscreteHMCGibbs` | mixed sampler         | Gibbs step for discrete variables plus an HMC inner kernel                                                                                                               |
|  [09]   | `HMCGibbs`         | mixed sampler         | HMC inner kernel with a user `gibbs_fn` outer step                                                                                                                       |
|  [10]   | `MixedHMC`         | mixed sampler         | mixed continuous/discrete HMC over a `HMC` inner kernel                                                                                                                  |
|  [11]   | `HMCECS`           | scalable sampler      | HMC with energy-conserving subsampling for tall-data likelihoods; pairs with `plate(subsample_size=)`                                                                    |
|  [12]   | `Predictive`       | posterior sampling    | `(model, posterior_samples=None, *, guide=None, params=None, num_samples=None, return_sites=None, infer_discrete=False, parallel=False, batch_ndims=1)`; callable on `(rng_key, *args)` → site-keyed draws |

[PUBLIC_TYPE_SCOPE]: variational inference family (`numpyro.infer`)
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                                  |
| :-----: | :---------------------------------------- | :------------------ | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `SVI`                                     | VI runner           | `(model, guide, optim, loss, **static_kwargs)`; `.init`/`.update`/`.stable_update`/`.run`/`.get_params(state)`/`.evaluate` |
|  [02]   | `Trace_ELBO`                              | ELBO objective      | `(num_particles=1, vectorize_particles=True)` standard reparam-gradient ELBO                                  |
|  [03]   | `TraceMeanField_ELBO`                     | ELBO objective      | mean-field KL closed-form ELBO; lower variance when guide factorizes                                          |
|  [04]   | `TraceGraph_ELBO`                         | ELBO objective      | ELBO with Rao-Blackwellized score-function gradients for non-reparam sites                                    |
|  [05]   | `TraceEnum_ELBO`                          | ELBO objective      | ELBO with discrete-variable enumeration (pairs with `config_enumerate`)                                       |
|  [06]   | `RenyiELBO`                               | ELBO objective      | `(alpha, num_particles)` Renyi alpha-divergence bound                                                         |
|  [07]   | `optim.Adam` / `optim.ClippedAdam`        | VI optimizer        | numpyro-native optimizers (`(step_size, ...)`); `ClippedAdam` adds gradient clipping                          |
|  [08]   | `optim.optax_to_numpyro(transform)`       | optimizer bridge    | wraps any `optax` gradient transform as a numpyro `_NumPyroOptim` — the canonical SVI optimizer stack         |
|  [09]   | `autoguide.AutoNormal`                    | guide automate      | diagonal-normal mean-field guide                                                                              |
|  [10]   | `autoguide.AutoDelta`                     | guide automate      | MAP point-estimate guide                                                                                      |
|  [11]   | `autoguide.AutoMultivariateNormal`        | guide automate      | full-covariance Gaussian guide                                                                                |
|  [12]   | `autoguide.AutoLowRankMultivariateNormal` | guide automate      | low-rank-plus-diagonal Gaussian guide                                                                         |
|  [13]   | `autoguide.AutoLaplaceApproximation`      | guide automate      | Laplace approximation around the MAP mode                                                                     |
|  [14]   | `autoguide.AutoBNAFNormal` / `AutoIAFNormal` | guide automate   | normalizing-flow guides (block-NAF / inverse-autoregressive) for non-Gaussian posteriors                      |
|  [15]   | `autoguide.AutoGuideList`                 | guide composite     | composes per-site autoguides into one guide                                                                  |

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

[PUBLIC_TYPE_SCOPE]: distribution combinators, transforms, and reparameterizers
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                                                  | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                          |
| :-----: | :-------------------------------------------------------- | :------------------ | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `Distribution.to_event(reinterpreted_batch_ndims)`        | dependency reshape  | folds trailing batch dims into the event shape (vector likelihoods)                                  |
|  [02]   | `Distribution.expand(batch_shape)` / `.expand_by(shape)`  | batch broadcast     | broadcasts a distribution across a plate dimension                                                   |
|  [03]   | `Distribution.mask(mask)`                                 | partial observation | zeroes log-density at masked sites (missing/censored data)                                           |
|  [04]   | `TransformedDistribution(base, transforms)`               | pushforward         | applies a `transforms.Transform` chain to a base distribution                                        |
|  [05]   | `MixtureSameFamily` / `MixtureGeneral`                    | mixture             | mixture-model likelihoods over a `Categorical` mixing distribution                                   |
|  [06]   | `Independent(base, reinterpreted_batch_ndims)`            | event reinterpret   | explicit form of `.to_event`                                                                          |
|  [07]   | `constraints.{positive,simplex,unit_interval,ordered_vector,...}` | support constraint  | the support algebra autoguides and bijectors read to choose unconstraining transforms       |
|  [08]   | `transforms.{AffineTransform,ExpTransform,StickBreakingTransform}` / `transforms.biject_to(constraint)` | bijector | constrained↔unconstrained bijectors; `biject_to(constraint)` selects the transform for a support |
|  [09]   | `infer.reparam.{TransformReparam,LocScaleReparam,NeuTraReparam,ProjectedNormalReparam}` | reparam | non-centered / NeuTra reparameterizations applied via `handlers.reparam(model, config)`              |

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
|  [08]   | `handlers.do(fn, data)`                                     | effect handler | applies a causal intervention (`do`-calculus) on sites    |
|  [09]   | `handlers.scale(fn, scale)` / `handlers.mask(fn, mask)`     | effect handler | scales/masks site log-densities (subsampling, missing data) |
|  [10]   | `handlers.scope(fn, prefix, divider)` / `handlers.lift(fn, prior)` | effect handler | namespaces site names; lifts `param` sites to random `sample` sites |
|  [11]   | `MCMC.run(rng_key, *args, init_params=None, extra_fields=())` / `.get_samples(group_by_chain=False)` | inference run  | warms up, samples, returns site-keyed draws; `extra_fields=('potential_energy','diverging','num_steps')` collects sampler stats |
|  [12]   | `MCMC.print_summary()` / `.last_state` / `.post_warmup_state` | run inspection | prints per-site R-hat/ESS summary; `post_warmup_state` reuse warm-resumes `.run(state.rng_key)` without re-tuning |
|  [13]   | `SVI.run(rng_key, num_steps, *args)` → `SVIRunResult(params, state, losses)` / `.get_params(state)` | inference run  | optimizes the guide; `losses` is the per-step ELBO trace  |
|  [14]   | `infer.util.{log_likelihood,log_density,init_to_value,Predictive}` | inference util | pointwise log-lik for `arviz.loo`/`waic`; `init_to_value(values=)` seeds warmup from a dict |
|  [15]   | `diagnostics.{effective_sample_size,gelman_rubin,split_gelman_rubin,autocorrelation,hpdi,summary}` | diagnostic | ESS, R-hat, split-R-hat, autocorrelation, HPDI intervals, summary table |
|  [16]   | `infer.init_to_median` family                               | init strategy  | `init_to_uniform/feasible/mean/median/sample/value` warmup seeds |
|  [17]   | `enable_x64()` / `set_platform(p)` / `set_host_device_count(n)` | runtime config | enables float64; selects JAX device; sets the CPU device count for `chain_method='parallel'` |

## [04]-[IMPLEMENTATION_LAW]

[EFFECT_TOPOLOGY]:
- A model is a plain Python function calling `sample`, `param`, `plate`, and `deterministic`; effect handlers are context managers or callables that intercept every site.
- `MCMC(NUTS(model), num_warmup, num_samples).run(rng_key)` warms up, samples, and stores draws; `.get_samples()` returns a site-keyed dict of posterior arrays.
- `SVI(model, guide, optim, loss).run(rng_key, num_steps)` returns an `SVIRunResult` carrying optimized params and the loss trace; `Predictive` then draws posterior-predictive samples.
- `enable_x64()` switches JAX to 64-bit floats before model construction; sampler precision and mass-matrix conditioning depend on it.
- `init_to_*` strategies seed the warmup; `dense_mass=True` on `NUTS` learns a full mass matrix at higher per-step cost.

[STACKING_TOPOLOGY]:
- numpyro → `arviz`: the MCMC object goes through `az.from_numpyro(mcmc, prior=, posterior_predictive=)`, the SVI object through `az.from_numpyro_svi(svi)`, yielding the `xarray.DataTree` posterior the `arviz` rail diagnoses with `rhat`/`ess`/`loo`/`summary`. `MCMC.run(..., extra_fields=('diverging','potential_energy'))` is what populates the `sample_stats` group `az.bfmi`/divergence checks read. `infer.util.log_likelihood` feeds the `log_likelihood` group `arviz.loo`/`waic` require — never recompute pointwise log-lik by hand.
- numpyro → `jax`: a study threads one `jax.random.PRNGKey(seed)` split across `MCMC.run` and `Predictive.__call__`; `enable_x64()` flips the same JAX float64 flag the `diffrax`/`optimistix`/`lineax` rails share, so set it once at study entry before any model construction.
- numpyro → `optax`: SVI optimizers are `optim.optax_to_numpyro(optax.chain(optax.clip_by_global_norm(...), optax.adam(...)))` — compose the `optax` transform stack, wrap it once, pass it as `SVI(..., optim=)`. The numpyro-native `optim.Adam`/`optim.ClippedAdam` are the no-`optax` shortcut.
- numpyro ↔ `blackjax`/`pymc`: `blackjax` is the lower-level JAX sampler sibling (window adaptation, custom kernels numpyro does not expose); `pymc` is the non-JAX PPL sibling that hands its NUTS to `nutpie`. A study picks numpyro when the model is JAX-native and benefits from `vmap`/`jit` over the log-density.

[STUDY_ROUTING]:
- A `NumericIntent` Bayesian study defines a model function, runs it through `MCMC(NUTS(...))` or `SVI`, then graduates via `az.from_numpyro`; the captured receipt is the `DataTree` plus R-hat/ESS/divergence diagnostics, never raw arrays.
- Effect handlers compose as nested context managers: `seed(condition(substitute(model, params), obs), key)` inspects a conditioned model deterministically without an MCMC run; `handlers.reparam(model, {site: LocScaleReparam()})` rewrites a centered model to non-centered before sampling a funnel geometry.
- `dense_mass=True` learns a full mass matrix at higher per-step cost; `chain_method='vectorized'` runs chains under one `vmap` (required by `AIES`/ensemble `ESS`), `'parallel'` maps over `set_host_device_count(n)` CPU devices.
- No production runtime imports numpyro; inference is offline study work feeding the C# `Rasm.Compute` model rail.

[RAIL_LAW]:
- Package: `numpyro`
- Owns: JAX-native probabilistic programming — model effect handlers, MCMC/VI inference engines, distribution families, and posterior-predictive sampling
- Accept: a `NumericIntent` Bayesian study run through `MCMC(NUTS(...))` (with `extra_fields` for divergence/energy stats) or `SVI(model, guide, optim=optim.optax_to_numpyro(...), loss)`, graduated through `az.from_numpyro`/`az.from_numpyro_svi` into a `DataTree` receipt with R-hat/ESS/divergence diagnostics
- Reject: hand-rolled Metropolis or VI objectives numpyro owns; hand-recomputed pointwise log-lik that `infer.util.log_likelihood` provides for `arviz.loo`; numpyro in any product runtime path; posterior claims without a captured `DataTree` receipt
