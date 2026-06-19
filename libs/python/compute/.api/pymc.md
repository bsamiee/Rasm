# [PY_COMPUTE_API_PYMC]

`pymc` supplies a Python probabilistic programming interface over PyTensor — model context management, distribution families, sampling via NUTS/HMC/SMC/VI, and posterior predictive sampling — for the compute Bayesian-study rail. The package owner builds models inside a `pm.Model()` context and samples with `pm.sample`, returning `xarray.DataTree` posteriors; it never re-implements a distribution or sampler the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymc`
- package: `pymc`
- import: `pymc` (alias `pm`)
- owner: `compute`
- rail: Bayesian-study
- capability: PyTensor-backed probabilistic programming — declarative `Model` context, 60+ distribution families, NUTS/HMC/Metropolis/SMC step methods, ADVI/FullRank/SVGD variational families, and `xarray.DataTree` posterior output

## [02]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: model and context types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                                            |
| :-----: | :-------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `Model`         | model context  | context manager for declaring random variables and data |
|  [02]   | `Data`          | data container | mutable shared data tensor inside a model               |
|  [03]   | `Deterministic` | deterministic  | records a named deterministic transformation            |
|  [04]   | `Potential`     | log factor     | adds an arbitrary log factor to the joint               |

[PUBLIC_TYPE_SCOPE]: continuous distribution families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]       | [SUPPORT]          | [CANONICAL_PARAMS]  |
| :-----: | :------------- | :----------------- | :------------------ |
|  [01]   | `Normal`       | real line          | `mu`, `sigma`       |
|  [02]   | `Beta`         | (0, 1)             | `alpha`, `beta`     |
|  [03]   | `Gamma`        | positive reals     | `alpha`, `beta`     |
|  [04]   | `Exponential`  | positive reals     | `lam`               |
|  [05]   | `StudentT`     | real line          | `nu`, `mu`, `sigma` |
|  [06]   | `HalfNormal`   | non-negative reals | `sigma`             |
|  [07]   | `HalfCauchy`   | non-negative reals | `beta`              |
|  [08]   | `LogNormal`    | positive reals     | `mu`, `sigma`       |
|  [09]   | `Uniform`      | bounded interval   | `lower`, `upper`    |
|  [10]   | `Cauchy`       | real line          | `alpha`, `beta`     |
|  [11]   | `Laplace`      | real line          | `mu`, `b`           |
|  [12]   | `Logistic`     | real line          | `mu`, `s`           |
|  [13]   | `Gumbel`       | real line          | `mu`, `beta`        |
|  [14]   | `InverseGamma` | positive reals     | `alpha`, `beta`     |
|  [15]   | `Pareto`       | above a minimum    | `alpha`, `m`        |
|  [16]   | `Wald`         | positive reals     | `mu`, `lam`         |

[PUBLIC_TYPE_SCOPE]: discrete and multivariate distribution families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]           | [SUPPORT]                  | [CANONICAL_PARAMS]    |
| :-----: | :----------------- | :------------------------- | :-------------------- |
|  [01]   | `Bernoulli`        | {0, 1}                     | `p`                   |
|  [02]   | `Binomial`         | non-negative integers      | `n`, `p`              |
|  [03]   | `Categorical`      | finite set                 | `p`                   |
|  [04]   | `Poisson`          | non-negative integers      | `mu`                  |
|  [05]   | `NegativeBinomial` | non-negative integers      | `mu`, `alpha`         |
|  [06]   | `Geometric`        | positive integers          | `p`                   |
|  [07]   | `DiscreteUniform`  | integer range              | `lower`, `upper`      |
|  [08]   | `MvNormal`         | real vector space          | `mu`, `cov`           |
|  [09]   | `MvStudentT`       | real vector space          | `nu`, `mu`, `scale`   |
|  [10]   | `Dirichlet`        | probability simplex        | `a`                   |
|  [11]   | `LKJCholeskyCov`   | Cholesky correlation       | `eta`, `n`, `sd_dist` |
|  [12]   | `Wishart`          | positive-definite matrices | `nu`, `V`             |

[PUBLIC_TYPE_SCOPE]: step methods and samplers
- rail: Bayesian-study

| [INDEX] | [SYMBOL]        | [SAMPLER_FAMILY] | [CAPABILITY]                                 |
| :-----: | :-------------- | :--------------- | :------------------------------------------- |
|  [01]   | `NUTS`          | gradient-based   | `(vars, max_treedepth, early_max_treedepth)` |
|  [02]   | `HamiltonianMC` | gradient-based   | `(vars, path_length, max_steps)`             |
|  [03]   | `Metropolis`    | gradient-free    | `(vars, proposal_dist, scaling, tune)`       |
|  [04]   | `DEMetropolis`  | gradient-free    | differential-evolution Metropolis            |
|  [05]   | `Slice`         | gradient-free    | slice sampling step method                   |

[PUBLIC_TYPE_SCOPE]: variational families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]       | [VI_FAMILY]   | [CAPABILITY]                       |
| :-----: | :------------- | :------------ | :--------------------------------- |
|  [01]   | `ADVI`         | mean-field VI | automatic differentiation VI       |
|  [02]   | `FullRankADVI` | full-rank VI  | full-rank Gaussian ADVI            |
|  [03]   | `SVGD`         | particle VI   | Stein variational gradient descent |
|  [04]   | `ASVGD`        | amortized VI  | amortized SVGD                     |

## [03]-[ENTRYPOINTS_AND_OPERATIONS]

[ENTRYPOINT_SCOPE]: sampling and inference entrypoints
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `sample(draws, *, tune, chains, cores, random_seed, step, nuts_sampler, initvals, init, return_inferencedata)` | MCMC           | primary MCMC entrypoint; returns `DataTree`      |
|  [02]   | `sample_posterior_predictive(trace, model, *, var_names, random_seed, return_inferencedata)`                   | posterior      | posterior predictive samples from fitted model   |
|  [03]   | `sample_prior_predictive(samples, model, *, var_names, random_seed)`                                           | prior          | prior predictive samples before conditioning     |
|  [04]   | `sample_smc(draws, *, kernel, model)`                                                                          | SMC            | sequential Monte Carlo sampling                  |
|  [05]   | `fit(n, method, model, random_seed, start, **kwargs)`                                                          | VI             | variational inference fit; returns approximation |
|  [06]   | `find_MAP(start, vars, method, model, maxeval, seed)`                                                          | optimization   | MAP point estimate via gradient optimization     |

[ENTRYPOINT_SCOPE]: diagnostics and log-probability
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                                        |
| :-----: | :------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `compute_log_likelihood(idata, model)`       | diagnostics      | per-observation log-likelihood from posterior |
|  [02]   | `to_inference_data(trace, model)`            | conversion       | converts trace to `xarray.DataTree`           |
|  [03]   | `predictions_to_inference_data(pred, idata)` | conversion       | attaches predictions to existing `DataTree`   |
|  [04]   | `logp(rv, value)`                            | log-prob         | evaluates distribution log-probability        |
|  [05]   | `draw(vars, draws, model, random_seed)`      | sampling utility | draws samples from a model variable           |

## [04]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pymc`
- Owns: PyTensor-backed probabilistic model construction, distribution families, MCMC/SMC/VI inference, and posterior/prior predictive sampling
- Accept: a model defined inside a `pm.Model()` context with named `sample` sites, run through `pm.sample` or `pm.fit`, returning a `DataTree` receipt with captured draws, tune steps, chains, and convergence checks
- Reject: hand-rolled distributions or samplers pymc owns; pymc models outside a `Model()` context; product serving of posteriors; benchmark claims without a `DataTree` receipt
