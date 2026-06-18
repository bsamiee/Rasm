# [PY_COMPUTE_API_PYMC]

`pymc` supplies a Python probabilistic programming interface over PyTensor — model context management, distribution families, sampling via NUTS/HMC/SMC/VI, and posterior predictive sampling — for the compute Bayesian-study rail. The package owner builds models inside a `pm.Model()` context and samples with `pm.sample`, returning `xarray.DataTree` posteriors; it never re-implements a distribution or sampler the package owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymc`
- package: `pymc`
- import: `pymc` (alias `pm`)
- owner: `compute`
- rail: Bayesian-study
- capability: PyTensor-backed probabilistic programming — declarative `Model` context, 60+ distribution families, NUTS/HMC/Metropolis/SMC step methods, ADVI/FullRank/SVGD variational families, and `xarray.DataTree` posterior output

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: model and context types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE] | [CAPABILITY]                                            |
| :-----: | :-------------- | :------------- | :------------------------------------------------------ |
|   [1]   | `Model`         | model context  | context manager for declaring random variables and data |
|   [2]   | `Data`          | data container | mutable shared data tensor inside a model               |
|   [3]   | `Deterministic` | deterministic  | records a named deterministic transformation            |
|   [4]   | `Potential`     | log factor     | adds an arbitrary log factor to the joint               |

[PUBLIC_TYPE_SCOPE]: continuous distribution families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]       | [SUPPORT]          | [CANONICAL_PARAMS]  |
| :-----: | :------------- | :----------------- | :------------------ |
|   [1]   | `Normal`       | real line          | `mu`, `sigma`       |
|   [2]   | `Beta`         | (0, 1)             | `alpha`, `beta`     |
|   [3]   | `Gamma`        | positive reals     | `alpha`, `beta`     |
|   [4]   | `Exponential`  | positive reals     | `lam`               |
|   [5]   | `StudentT`     | real line          | `nu`, `mu`, `sigma` |
|   [6]   | `HalfNormal`   | non-negative reals | `sigma`             |
|   [7]   | `HalfCauchy`   | non-negative reals | `beta`              |
|   [8]   | `LogNormal`    | positive reals     | `mu`, `sigma`       |
|   [9]   | `Uniform`      | bounded interval   | `lower`, `upper`    |
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
|   [1]   | `Bernoulli`        | {0, 1}                     | `p`                   |
|   [2]   | `Binomial`         | non-negative integers      | `n`, `p`              |
|   [3]   | `Categorical`      | finite set                 | `p`                   |
|   [4]   | `Poisson`          | non-negative integers      | `mu`                  |
|   [5]   | `NegativeBinomial` | non-negative integers      | `mu`, `alpha`         |
|   [6]   | `Geometric`        | positive integers          | `p`                   |
|   [7]   | `DiscreteUniform`  | integer range              | `lower`, `upper`      |
|   [8]   | `MvNormal`         | real vector space          | `mu`, `cov`           |
|   [9]   | `MvStudentT`       | real vector space          | `nu`, `mu`, `scale`   |
|  [10]   | `Dirichlet`        | probability simplex        | `a`                   |
|  [11]   | `LKJCholeskyCov`   | Cholesky correlation       | `eta`, `n`, `sd_dist` |
|  [12]   | `Wishart`          | positive-definite matrices | `nu`, `V`             |

[PUBLIC_TYPE_SCOPE]: step methods and samplers
- rail: Bayesian-study

| [INDEX] | [SYMBOL]        | [SAMPLER_FAMILY] | [CAPABILITY]                                 |
| :-----: | :-------------- | :--------------- | :------------------------------------------- |
|   [1]   | `NUTS`          | gradient-based   | `(vars, max_treedepth, early_max_treedepth)` |
|   [2]   | `HamiltonianMC` | gradient-based   | `(vars, path_length, max_steps)`             |
|   [3]   | `Metropolis`    | gradient-free    | `(vars, proposal_dist, scaling, tune)`       |
|   [4]   | `DEMetropolis`  | gradient-free    | differential-evolution Metropolis            |
|   [5]   | `Slice`         | gradient-free    | slice sampling step method                   |

[PUBLIC_TYPE_SCOPE]: variational families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]       | [VI_FAMILY]   | [CAPABILITY]                       |
| :-----: | :------------- | :------------ | :--------------------------------- |
|   [1]   | `ADVI`         | mean-field VI | automatic differentiation VI       |
|   [2]   | `FullRankADVI` | full-rank VI  | full-rank Gaussian ADVI            |
|   [3]   | `SVGD`         | particle VI   | Stein variational gradient descent |
|   [4]   | `ASVGD`        | amortized VI  | amortized SVGD                     |

## [3]-[ENTRYPOINTS_AND_OPERATIONS]

[ENTRYPOINT_SCOPE]: sampling and inference entrypoints
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|   [1]   | `sample(draws, *, tune, chains, cores, random_seed, step, nuts_sampler, initvals, init, return_inferencedata)` | MCMC           | primary MCMC entrypoint; returns `DataTree`      |
|   [2]   | `sample_posterior_predictive(trace, model, *, var_names, random_seed, return_inferencedata)`                   | posterior      | posterior predictive samples from fitted model   |
|   [3]   | `sample_prior_predictive(samples, model, *, var_names, random_seed)`                                           | prior          | prior predictive samples before conditioning     |
|   [4]   | `sample_smc(draws, *, kernel, model)`                                                                          | SMC            | sequential Monte Carlo sampling                  |
|   [5]   | `fit(n, method, model, random_seed, start, **kwargs)`                                                          | VI             | variational inference fit; returns approximation |
|   [6]   | `find_MAP(start, vars, method, model, maxeval, seed)`                                                          | optimization   | MAP point estimate via gradient optimization     |

[ENTRYPOINT_SCOPE]: diagnostics and log-probability
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                                        |
| :-----: | :------------------------------------------- | :--------------- | :-------------------------------------------- |
|   [1]   | `compute_log_likelihood(idata, model)`       | diagnostics      | per-observation log-likelihood from posterior |
|   [2]   | `to_inference_data(trace, model)`            | conversion       | converts trace to `xarray.DataTree`           |
|   [3]   | `predictions_to_inference_data(pred, idata)` | conversion       | attaches predictions to existing `DataTree`   |
|   [4]   | `logp(rv, value)`                            | log-prob         | evaluates distribution log-probability        |
|   [5]   | `draw(vars, draws, model, random_seed)`      | sampling utility | draws samples from a model variable           |

## [4]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pymc`
- Owns: PyTensor-backed probabilistic model construction, distribution families, MCMC/SMC/VI inference, and posterior/prior predictive sampling
- Accept: a model defined inside a `pm.Model()` context with named `sample` sites, run through `pm.sample` or `pm.fit`, returning a `DataTree` receipt with captured draws, tune steps, chains, and convergence checks
- Reject: hand-rolled distributions or samplers pymc owns; pymc models outside a `Model()` context; product serving of posteriors; benchmark claims without a `DataTree` receipt
