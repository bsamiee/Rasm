# [PY_COMPUTE_API_ARVIZ]

`arviz` supplies Bayesian diagnostics, posterior summaries, LOO/WAIC model comparison, and a 39-function visualization library for the compute Bayesian-study rail. The package owner reads `xarray.DataTree` posteriors from any backend (PyMC, numpyro, Stan, emcee) and computes convergence diagnostics, PSIS-LOO, ESS, Rhat, and HDI; it never re-implements a diagnostic metric the package owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arviz`
- package: `arviz`
- import: `arviz` (alias `az`)
- owner: `compute`
- rail: Bayesian-study
- capability: backend-agnostic posterior analysis — `xarray.DataTree` I/O (`from_dict`, `from_netcdf`, `from_numpyro`, `from_cmdstanpy`, `from_emcee`, `from_zarr`), MCMC diagnostics (`rhat`, `ess`, `mcse`, `bfmi`), model comparison (`loo`, `compare`), HDI/ETI credible intervals, and 39 plot functions

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]  | [CAPABILITY]                                                      |
| :-----: | :---------------- | :-------------- | :---------------------------------------------------------------- |
|   [1]   | `xarray.DataTree` | posterior store | primary posterior container (replaces deprecated `InferenceData`) |
|   [2]   | `ELPDData`        | LOO result      | carries `elpd_loo`, `se`, `p_loo`, `pareto_k`, `n_samples` fields |
|   [3]   | `PlotCollection`  | plot container  | collection of plot panels                                         |
|   [4]   | `PlotMatrix`      | plot container  | matrix of plot panels                                             |

[PUBLIC_TYPE_SCOPE]: I/O converters
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]  | [SOURCE]                    |
| :-----: | :----------------------------------------------------------- | :-------------- | :-------------------------- |
|   [1]   | `from_dict(data, *, name, sample_dims, coords, dims, attrs)` | converter       | nested Python dict          |
|   [2]   | `from_netcdf(filename_or_obj, *, engine, chunks, decode_cf)` | converter       | NetCDF4 / Zarr file         |
|   [3]   | `from_numpyro(posterior, *, prior, posterior_predictive)`    | converter       | numpyro MCMC object         |
|   [4]   | `from_numpyro_svi(posterior, *, prior)`                      | converter       | numpyro SVI object          |
|   [5]   | `from_cmdstanpy(fit)`                                        | converter       | CmdStanPy fit               |
|   [6]   | `from_emcee(sampler, var_names)`                             | converter       | emcee `EnsembleSampler`     |
|   [7]   | `from_zarr(store)`                                           | converter       | Zarr store                  |
|   [8]   | `dict_to_dataset(data, *, coords, dims)`                     | dataset builder | converts dict to xr.Dataset |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MCMC diagnostics
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------------------------ |
|   [1]   | `rhat(data, sample_dims, group, var_names, method)`                 | convergence    | rank-normalized Rhat per variable           |
|   [2]   | `ess(data, sample_dims, group, var_names, method, relative, prob)`  | convergence    | effective sample size (bulk/tail/quantile)  |
|   [3]   | `mcse(data, sample_dims, group, var_names, method, prob, circular)` | convergence    | Monte Carlo standard error                  |
|   [4]   | `bfmi(data)`                                                        | HMC diagnostic | Bayesian fraction of missing information    |
|   [5]   | `summary(data, var_names, group, kind, fmt, ci_prob, ci_kind)`      | summary table  | combined diagnostic and statistic table     |
|   [6]   | `diagnose(data)`                                                    | diagnostics    | prints all convergence warnings             |
|   [7]   | `sampling_diagnostics(data)`                                        | diagnostics    | returns dict of per-chain diagnostic values |

[ENTRYPOINT_SCOPE]: model comparison and LOO
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]      | [RAIL]                                          |
| :-----: | :--------------------------------------------------- | :------------------ | :---------------------------------------------- |
|   [1]   | `loo(data, pointwise, var_name, reff)`               | model comparison    | PSIS-LOO-CV; returns `ELPDData`                 |
|   [2]   | `compare(compare_dict, method, var_name, reference)` | model comparison    | stacking/pseudo-BMA comparison across models    |
|   [3]   | `loo_approximate_posterior(data, log_p, log_q)`      | importance sampling | LOO for approximate posteriors                  |
|   [4]   | `loo_moment_match(data, model)`                      | moment matching     | moment-matched LOO for influential observations |
|   [5]   | `reloo(data, model, k_thresh)`                       | exact refit         | exact LOO by refitting high-k observations      |
|   [6]   | `loo_kfold(data, folds, model)`                      | cross-validation    | k-fold LOO cross-validation                     |
|   [7]   | `bayes_factor(trace, var, ref_val)`                  | Bayes factor        | Savage-Dickey density ratio Bayes factor        |

[ENTRYPOINT_SCOPE]: credible interval and summary statistics
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :--------------------------------------------------------- | :------------- | :------------------------------------------------ |
|   [1]   | `hdi(data, prob, dim, group, var_names, method, circular)` | interval       | highest density interval (unimodal or multimodal) |
|   [2]   | `eti(data, prob, dim, group, var_names)`                   | interval       | equal-tailed interval                             |
|   [3]   | `mean(data, group, var_names)`                             | statistic      | posterior mean per variable                       |
|   [4]   | `median(data, group, var_names)`                           | statistic      | posterior median per variable                     |
|   [5]   | `mode(data, group, var_names)`                             | statistic      | posterior mode per variable                       |
|   [6]   | `std(data, group, var_names)`                              | statistic      | posterior standard deviation                      |
|   [7]   | `var(data, group, var_names)`                              | statistic      | posterior variance                                |
|   [8]   | `mad(data, group, var_names)`                              | statistic      | median absolute deviation                         |

[ENTRYPOINT_SCOPE]: visualization functions
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]   | [CAPABILITY]                            |
| :-----: | :------------------------------------------------ | :--------------- | :-------------------------------------- |
|   [1]   | `plot_trace(dt, *, var_names, group, coords)`     | trace plot       | trace and distribution per variable     |
|   [2]   | `plot_forest(dt, *, var_names, combined)`         | forest plot      | credible interval forest plot           |
|   [3]   | `plot_pair(dt, *, var_names, group, divergences)` | joint plot       | pairwise joint marginal plots           |
|   [4]   | `plot_ess(dt, *, var_names, kind)`                | ESS plot         | ESS vs draw count or quantile           |
|   [5]   | `plot_rank(dt, *, var_names, kind)`               | rank plot        | rank histogram and trace rank           |
|   [6]   | `plot_energy(dt, *, kind)`                        | HMC diagnostic   | energy and BFMI diagnostic              |
|   [7]   | `plot_autocorr(dt, *, var_names)`                 | autocorrelation  | autocorrelation per variable            |
|   [8]   | `plot_dist(dt, *, var_names, kind)`               | marginal dist    | marginal posterior distribution         |
|   [9]   | `plot_ppc_dist(dt, *, group)`                     | PPC              | posterior predictive check distribution |
|  [10]   | `plot_compare(compare_df)`                        | model comparison | stacked ELPD comparison plot            |
|  [11]   | `plot_khat(loo_result)`                           | LOO diagnostic   | Pareto k diagnostic scatter plot        |
|  [12]   | `plot_mcse(dt, *, var_names)`                     | MCSE plot        | Monte Carlo standard error vs quantile  |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `arviz`
- Owns: backend-agnostic Bayesian diagnostics, PSIS-LOO model comparison, credible intervals, and posterior visualization from `xarray.DataTree` posteriors
- Accept: a `DataTree` posterior from any supported backend converted via `from_dict`/`from_numpyro`/`from_netcdf`, analyzed with `rhat`/`ess`/`loo`/`summary`, with a captured diagnostic receipt before any model-comparison claim
- Reject: hand-rolled Rhat, ESS, or LOO computations arviz owns; visualization calls without a `DataTree` receipt; `arviz.InferenceData` (deprecated alias for `xarray.DataTree`)
