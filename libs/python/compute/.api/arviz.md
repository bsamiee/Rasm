# [PY_COMPUTE_API_ARVIZ]

`arviz` (1.x line) is the metapackage over `arviz_base` (I/O + reorganization + `rcParams`), `arviz_stats` (diagnostics, model comparison, sensitivity, the `.azstats` xarray accessor), and `arviz_plots` (37 plot functions). It supplies backend-agnostic Bayesian diagnostics, posterior summaries, the full PSIS-LOO family, prior/likelihood sensitivity, and predictive metrics for the compute Bayesian-study rail. The owner reads `xarray.DataTree` posteriors from any backend (PyMC, numpyro, Stan, emcee) and never re-implements a metric the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arviz`
- package: `arviz` (metapackage; pulls `arviz-base`, `arviz-stats`, `arviz-plots`)
- version: `1.2.0`
- license: Apache-2.0
- import: `arviz` (alias `az`)
- owner: `compute`
- rail: Bayesian-study
- capability: backend-agnostic posterior analysis — `xarray.DataTree` I/O, MCMC diagnostics (`rhat`/`ess`/`mcse`/`bfmi`/`rhat_nested`), full LOO family (`loo`/`loo_subsample`/`update_subsample`/`loo_pit`/`loo_expectations`/`loo_metrics`/`loo_r2`/`loo_moment_match`/`loo_kfold`/`reloo`), stacking/BMA `compare`, prior-likelihood sensitivity (`psense`/`psense_summary`), predictive divergences (`wasserstein`/`kl_divergence`), in-sample fit (`metrics`/`bayesian_r2`/`residual_r2`), survival curves (`kaplan_meier`/`generate_survival_curves`), HDI/ETI/ROPE intervals, the chained `.azstats` xarray accessor, the validated `rcParams`/`rc_context` option registry, and 37 plot functions

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result and container types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]    | [CAPABILITY]                                                                 |
| :-----: | :---------------------- | :---------------- | :--------------------------------------------------------------------------- |
|  [01]   | `xarray.DataTree`       | posterior store   | primary posterior container (`arviz.InferenceData` is a name bound to the same class); groups `posterior`/`sample_stats`/`log_likelihood`/`observed_data`/`posterior_predictive` |
|  [02]   | `ELPDData`              | LOO result        | `arviz_stats.utils.ELPDData`; plain object (no longer a `pandas.Series`) — see field table below |
|  [03]   | `PlotCollection`        | plot container    | faceted plot panel collection (`arviz_plots`)                                |
|  [04]   | `PlotMatrix`            | plot container    | matrix of plot panels                                                        |
|  [05]   | `SamplingWrapper`       | refit protocol    | base class for `reloo`/`loo_kfold` exact-refit hooks (`sel_observations`, `sample`, `get_inference_data`, `log_likelihood__i`) |
|  [06]   | `MCMCAdapter` / `NumPyroInferenceAdapter` / `SVIAdapter` | inference adapter | numpyro MCMC/SVI -> `DataTree` conversion adapters                           |
|  [07]   | `AzStatsDsAccessor` / `AzStatsDaAccessor` / `AzStatsDtAccessor` | xarray accessor | `.azstats` on `Dataset`/`DataArray`/`DataTree` — chained diagnostics in-place |
|  [08]   | `rcParams` / `rc_context` | option registry | validated defaults keyed `data.*`/`stats.*`/`plot.*` (`data.sample_dims`, `stats.ci_prob`, `stats.ci_kind`, `plot.backend`); `rc_context` scopes overrides |

[PUBLIC_TYPE_SCOPE]: `ELPDData` fields (`arviz_stats.utils.ELPDData`)
- rail: Bayesian-study
- note: there is no `elpd_loo`/`p_loo`/`looic`; the scalar is `elpd`, the effective-parameter count is `p`, scale is carried in `scale`

| [INDEX] | [FIELD]                                   | [TYPE]                | [ROLE]                                                       |
| :-----: | :---------------------------------------- | :-------------------- | :----------------------------------------------------------- |
|  [01]   | `kind`                                     | `str`                 | `'loo'` / `'loo_subsample'` / `'loo_kfold'` discriminator    |
|  [02]   | `elpd` / `se`                              | `float` / `float`     | expected log pointwise predictive density and its std error  |
|  [03]   | `p`                                        | `float`               | effective number of parameters                               |
|  [04]   | `n_samples` / `n_data_points`              | `int` / `int`         | posterior draw count and observation count                   |
|  [05]   | `scale`                                    | `str`                 | `'log'` / `'negative_log'` / `'deviance'`                    |
|  [06]   | `warning`                                  | `bool`                | high-Pareto-k warning flag                                   |
|  [07]   | `good_k`                                   | `float`               | Pareto k reliability threshold (sample-size dependent)       |
|  [08]   | `elpd_i` / `pareto_k`                      | `DataArray`           | pointwise elpd and per-observation Pareto k (when `pointwise=True`) |
|  [09]   | `approx_posterior` / `elpd_loo_approx` / `log_p` / `log_q` | `bool` / `DataArray` / `DataArray` | approximate-posterior LOO carriers (`loo_approximate_posterior`) |
|  [10]   | `subsample_size` / `subsampling_se` / `loo_subsample_observations` / `n_eff_i` | `int` / `float` / `ndarray` / `DataArray` | subsampled-LOO carriers (`loo_subsample`) |
|  [11]   | `n_folds`                                  | `int`                 | k-fold count (`loo_kfold`)                                   |
|  [12]   | `influence_pareto_k` / `thin_factor` / `log_jacobian` / `log_weights` | `DataArray` | moment-match / thinning / Jacobian carriers                  |

[PUBLIC_TYPE_SCOPE]: I/O converters (`arviz_base`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY]  | [SOURCE]                          |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :-------------- | :-------------------------------- |
|  [01]   | `from_dict(data, *, name, sample_dims, save_warmup, index_origin, coords, dims, pred_dims, pred_coords, attrs, check_conventions=True)` | converter | nested `{group: {var: ndarray}}` dict |
|  [02]   | `from_netcdf(filename_or_obj, *, engine, chunks, decode_cf, ...) -> DataTree`                                  | converter       | NetCDF4 file (xarray backend kwargs threaded) |
|  [03]   | `from_zarr(filename_or_obj, *, engine='zarr', chunks, ...) -> DataTree`                                        | converter       | Zarr store                        |
|  [04]   | `from_numpyro(posterior=None, *, prior, posterior_predictive, predictions, constant_data, log_likelihood=False, num_chains, coords, dims, ...)` | converter | numpyro MCMC object |
|  [05]   | `from_numpyro_svi(svi=None, *, svi_result, model_args, model_kwargs, prior, posterior_predictive, predictions, log_likelihood=False, num_samples=1000, ...)` | converter | numpyro SVI object |
|  [06]   | `from_cmdstanpy(posterior=None, *, posterior_predictive, prior, log_likelihood, observed_data, save_warmup, dtypes, ...)` | converter | CmdStanPy fit            |
|  [07]   | `from_emcee(sampler=None, var_names, slices, arg_names, arg_groups, blob_names, blob_groups, coords, dims, check_conventions=True)` | converter | emcee `EnsembleSampler`   |
|  [08]   | `convert_to_datatree(obj, **kwargs)` / `convert_to_dataset(obj, *, group='posterior', **kwargs)`              | universal       | polymorphic converter — dict / xr object / supported sampler |
|  [09]   | `dict_to_dataset(data, *, attrs, inference_library, coords, dims, sample_dims, index_origin, skip_event_dims=False, check_conventions=True)` | dataset builder | dict -> `xr.Dataset` |
|  [10]   | `extract(data, group='posterior', sample_dims, *, combined=True, var_names, filter_vars, num_samples, weights, resampling_method, keep_dataset=False, random_seed)` | reshaper | flatten `(chain, draw)` -> `sample`; optional weighted resample (PSIS-LOO posteriors) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MCMC diagnostics (`arviz_stats`)
- rail: Bayesian-study
- note: diagnostics accept either a `DataTree` (named groups) or a raw ndarray via `chain_axis=`/`draw_axis=`

| [INDEX] | [SURFACE]                                                                                                       | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `rhat(data, sample_dims, group='posterior', var_names, filter_vars, method='rank', chain_axis=0, draw_axis=1)`  | convergence    | rank-normalized R-hat per variable           |
|  [02]   | `rhat_nested(data, ..., superchain_ids, method='rank')`                                                         | convergence    | nested-R-hat for superchain (parallel-tempering) ensembles |
|  [03]   | `ess(data, sample_dims, group, var_names, method='bulk', relative=False, prob, chain_axis=0, draw_axis=1)`      | convergence    | effective sample size (`bulk`/`tail`/`quantile`/`mean`/`sd`/`median`/`mad`/`local`) |
|  [04]   | `mcse(data, sample_dims, group, var_names, method='mean', prob, circular=False, chain_axis=0, draw_axis=1)`     | convergence    | Monte Carlo standard error                   |
|  [05]   | `bfmi(data, sample_dims, group='sample_stats', var_names='energy')`                                             | HMC diagnostic | Bayesian fraction of missing information     |
|  [06]   | `summary(data, var_names, filter_vars, group='posterior', coords, sample_dims, kind='all', fmt='wide', ci_prob, ci_kind, round_to='auto', skipna=False)` | summary table | combined diagnostic + statistic `pandas.DataFrame` |
|  [07]   | `diagnose(data, *, var_names, filter_vars, group='posterior', rhat_max=1.01, ess_min_ratio, ess_threshold, bfmi_threshold=0.3, show_diagnostics=True, return_diagnostics=False)` | diagnostics | convergence warning report; optional per-diagnostic return |
|  [08]   | `thin(data, sample_dims='draw', group, var_names, factor='auto', chain_axis=0, draw_axis=1)`                    | thinning       | autocorrelation-aware draw thinning          |

[ENTRYPOINT_SCOPE]: model comparison and the LOO family (`arviz_stats`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                       | [ENTRY_FAMILY]      | [RAIL]                                          |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :------------------ | :---------------------------------------------- |
|  [01]   | `loo(data, pointwise, var_name, reff, log_lik_fn, log_weights, pareto_k, log_jacobian, mixture=False) -> ELPDData` | model comparison | PSIS-LOO-CV with Pareto-smoothed importance weights |
|  [02]   | `compare(compare_dict, method='stacking', var_name, reference, round_to='auto') -> DataFrame`                   | model comparison    | stacking / `'BB-pseudo-BMA'` weights across models |
|  [03]   | `loo_subsample(data, observations, pointwise, var_name, reff, log_weights, log_p, log_q, seed=315, method='lpd', thin, log_lik_fn, param_names, log=True, ...)` | scalable LOO | subsampled PSIS-LOO for large `n` |
|  [04]   | `update_subsample(loo_orig, data, observations, var_name, reff, ...)`                                           | scalable LOO        | grow an existing subsampled-LOO with more observations |
|  [05]   | `loo_approximate_posterior(data, log_p, log_q, pointwise, var_name, log_jacobian)`                              | importance sampling | LOO under an approximate (VI/Laplace) posterior |
|  [06]   | `loo_moment_match(data, loo_orig, log_prob_upars_fn, log_lik_i_upars_fn, upars, var_name, reff, max_iters=30, k_threshold, split=True, cov=True)` | moment matching | moment-matched LOO for high-k observations |
|  [07]   | `reloo(wrapper, loo_orig, var_name, log_weights, k_threshold=-inf, pointwise)`                                  | exact refit         | exact LOO by refitting high-k points via a `SamplingWrapper` |
|  [08]   | `loo_kfold(data, wrapper, pointwise, var_name, k=10, folds, stratify_by, group_by, save_fits=False)`            | cross-validation    | k-fold LOO-CV via a `SamplingWrapper`           |
|  [09]   | `loo_pit(data, var_names, log_weights, pareto_k, random_state, pareto_pit=False)`                               | calibration         | LOO probability-integral-transform calibration  |
|  [10]   | `loo_expectations(data, var_name, group='posterior_predictive', sample_dims, log_likelihood_var_name, kind='mean', probs, log_weights, pareto_k)` | expectation | LOO posterior-predictive expectations / quantiles |
|  [11]   | `loo_metrics(data, kind='rmse', var_name, round_to)`                                                            | predictive error    | LOO `rmse`/`mae`/`mse`/`acc` predictive metric  |
|  [12]   | `loo_r2(data, var_name, n_simulations=4000, summary=True, point_estimate, ci_kind, ci_prob, ...)` / `loo_score` / `loo_influence` / `loo_i` | predictive fit | LOO Bayesian R^2, scoring rules, influence, pointwise |

[ENTRYPOINT_SCOPE]: sensitivity, predictive divergence, and credible intervals (`arviz_stats`)
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                       | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `psense(data, var_names, group='prior', coords, sample_dims, alphas=(0.99, 1.01), group_var_names, group_coords)` | sensitivity    | power-scaling prior/likelihood sensitivity per variable |
|  [02]   | `psense_summary(data, var_names, filter_vars, threshold=0.05, alphas=(0.99, 1.01), prior_var_names, likelihood_var_names, round_to=3)` | sensitivity | prior-vs-likelihood sensitivity diagnosis table   |
|  [03]   | `wasserstein(data1, data2, group, var_names, sample_dims, joint=True, num_samples=500, random_seed=212480)`     | divergence     | Wasserstein distance between two posteriors       |
|  [04]   | `kl_divergence(data1, data2, group, var_names, sample_dims, num_samples=500, random_seed=212480)`               | divergence     | KL divergence between two posteriors              |
|  [05]   | `bayes_factor(data, var_names, ref_vals=0, return_ref_vals=False, prior, circular=False)`                       | Bayes factor   | Savage-Dickey density-ratio Bayes factor          |
|  [06]   | `bayesian_r2(data, pred_mean, scale, scale_kind='sd', summary=True, group='posterior', point_estimate, ci_kind, ci_prob, ...)` / `residual_r2(data, pred_mean, obs_name, summary=True, group='posterior', ...)` | model fit | in-sample and residual-based Bayesian R^2 |
|  [07]   | `metrics(data, kind='rmse', var_name, sample_dims, round_to)`                                                   | predictive error | in-sample `rmse`/`mae`/`mse` counterpart of `loo_metrics` |
|  [08]   | `kaplan_meier(dt, var_names)` / `generate_survival_curves(dt, var_names, group='posterior_predictive', num_samples, extrapolation_factor=1.2)` | survival | Kaplan-Meier curves over observed and predictive samples |
|  [09]   | `hdi(data, prob, dim, group='posterior', var_names, method='nearest', circular=False, max_modes=10, skipna=False)` | interval    | highest-density interval; returns a `DataTree`/Dataset with a `ci_bound` coord (`'lower'`/`'upper'`), multimodal up to `max_modes` |
|  [10]   | `eti(data, prob, dim, group, var_names, method='linear', skipna=False)`                                         | interval       | equal-tailed interval (`ci_bound` coord)          |
|  [11]   | `ci_in_rope(data, rope, var_names, group='posterior', dim, ci_prob, ci_kind, rope_dim='rope_dim')`              | decision       | fraction of credible interval inside a ROPE       |
|  [12]   | `mean` / `median` / `mode` / `std` / `var` / `mad` / `iqr` `(data, group, var_names)`                           | statistic      | posterior point/spread statistics per variable    |
|  [13]   | `kde(data, ...)` / `kde2d` / `histogram` / `ecdf(data, ..., pit=False)` / `qds(data, dim, group, var_names, nquantiles=100, ...)` | density | 1-D/2-D KDE, histogram, empirical CDF / PIT-ECDF, quantile dots |
|  [14]   | `weight_predictions(dts, weights, group='posterior_predictive', sample_dims, random_seed)`                      | stacking       | model-averaged predictive draws from `compare` weights |

[ENTRYPOINT_SCOPE]: chained `.azstats` xarray accessor (`arviz_stats`)
- rail: Bayesian-study
- note: every diagnostic above is also a method on `dataset.azstats.<op>()` / `datatree.azstats.<op>()`, so a posterior `Dataset` carries its own diagnostic algebra without a free-function round-trip

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `ds.azstats.{ess,rhat,rhat_nested,mcse,bfmi,thin}(...)`         | diagnostic     | chained convergence diagnostics on the dataset    |
|  [02]   | `ds.azstats.{hdi,eti,mean,median,mode,std,var,mad,iqr}(...)`    | statistic      | chained credible interval / point statistics      |
|  [03]   | `ds.azstats.{loo,loo_pit,loo_r2,loo_score,loo_expectation,loo_summary,loo_quantile,loo_mixture,loo_approximate_posterior}(...)` | LOO | chained LOO family on the dataset           |
|  [04]   | `ds.azstats.{psislw,pareto_khat,pareto_min_ss,power_scale_lw,power_scale_sense}(...)` | PSIS core | raw Pareto-smoothed-IS weights, k-hat, min sample size, power-scaling |
|  [05]   | `ds.azstats.{kde,ecdf,histogram,qds,compute_ranks,autocorr,uniformity_test,get_bins}(...)` | density | chained density / rank / autocorrelation         |

[ENTRYPOINT_SCOPE]: visualization functions (`arviz_plots`, 37 `plot_*` plus `combine_plots`/`add_lines`/`add_bands`)
- rail: Bayesian-study
- note: plots accept a `DataTree`/`Dataset` and return a `PlotCollection`/`PlotMatrix`; backend selected via `rcParams["plot.backend"]` (`matplotlib`/`bokeh`/`plotly`), templates via `style.use(name)`

| [INDEX] | [SURFACE]                                                                                                       | [ENTRY_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :--------------- | :----------------------------------------------- |
|  [01]   | `plot_trace` / `plot_trace_dist` / `plot_rank` / `plot_rank_dist`                                               | trace/rank       | trace, rank histogram, and rank-trace per variable |
|  [02]   | `plot_forest` / `plot_ridge` / `plot_pair` / `plot_pair_focus` / `plot_parallel`                               | marginal/joint   | forest, ridge, pairwise joint, parallel-coordinate |
|  [03]   | `plot_dist` / `plot_convergence_dist` / `plot_ess` / `plot_ess_evolution` / `plot_mcse` / `plot_autocorr`      | diagnostic       | marginal distribution, ESS/MCSE/autocorrelation  |
|  [04]   | `plot_energy` / `plot_khat`                                                                                     | HMC/LOO          | energy-BFMI, Pareto-k scatter                    |
|  [05]   | `plot_compare` / `plot_loo_pit` / `plot_loo_interval` / `plot_bf`                                               | model comparison | ELPD comparison, LOO-PIT calibration, Bayes factor |
|  [06]   | `plot_ppc_dist` / `plot_ppc_pit` / `plot_ppc_interval` / `plot_ppc_rootogram` / `plot_ppc_pava` / `plot_ppc_tstat` / `plot_ppc_censored` / `plot_ppc_dist_pit` / `plot_ppc_pava_residuals` | PPC | posterior-predictive checks |
|  [07]   | `plot_prior_posterior` / `plot_psense_dist` / `plot_psense_quantities`                                          | sensitivity      | prior-vs-posterior overlay, power-scaling sensitivity |
|  [08]   | `plot_dgof` / `plot_dgof_dist` / `plot_ecdf_pit` / `plot_lm` / `combine_plots` / `add_lines` / `add_bands`      | composition | goodness-of-fit, ECDF-PIT, linear-model, plot combination and overlay primitives |

## [04]-[IMPLEMENTATION_LAW]

[ARVIZ_TOPOLOGY]:
- The `arviz` import is a thin re-export over three packages: `arviz_base` owns converters/`extract`/`dict_to_dataset`/`from_*` and the `rcParams`/`rc_context` registry; `arviz_stats` owns every diagnostic, the LOO family, sensitivity, intervals, and the `.azstats` accessor; `arviz_plots` owns the 37 plot functions, `PlotCollection`/`PlotMatrix`, and the `style` module (`use`/`available`/`get`).
- The posterior container is `xarray.DataTree`; `arviz.InferenceData` is a name bound to the same class, so both spellings accept one container. Groups are children (`posterior`, `sample_stats`, `log_likelihood`, `observed_data`, `posterior_predictive`).
- `rcParams` is the validated option registry feeding every entrypoint whose kwarg is `None`: `data.sample_dims=('chain', 'draw')`, `stats.ci_prob=0.89`, `stats.ci_kind='eti'`, `stats.point_estimate`, `stats.module`, `plot.backend='auto'`; `rc_context(rc=...)` scopes overrides.
- `summary(kind='all')` returns a `pandas.DataFrame` indexed by flattened variable name (`theta[0]`) with columns `mean`, `sd`, `eti<pct>_lb`, `eti<pct>_ub`, `ess_bulk`, `ess_tail`, `r_hat`, `mcse_mean`, `mcse_sd`; `kind='stats'` keeps the first four, `kind='diagnostics'` the last five, and `all_median`/`diagnostics_median`/`mc_diagnostics` swap in median/MC variants. The interval columns are ETI by default (`ci_kind`), not HDI, and `r_hat` carries an underscore.
- `hdi`/`eti` return an xarray object with a `ci_bound` coordinate taking string values `'lower'`/`'upper'`; index `.sel(ci_bound='lower')` rather than reading old `hdi_3%`/`hdi_97%` columns.
- `ELPDData` is a plain object: read `elpd`/`se`/`p`/`good_k`/`warning`, and `elpd_i`/`pareto_k` `DataArray`s when `pointwise=True`. `elpd_loo`/`p_loo`/`looic` attributes do not exist.

[STACKING]:
- `arviz` is the diagnostic terminus of the JAX sampler rails: a `blackjax` study drives `util.run_inference_algorithm`, stacks the history into a `(chain, draw, ...)` array, and feeds it to `from_dict({"posterior": {...}, "sample_stats": {"energy": ...}, "log_likelihood": {...}})`; a `numpyro` study calls `from_numpyro`/`from_numpyro_svi` (or the `NumPyroInferenceAdapter`); `pymc`/`nutpie` emit a `DataTree` directly. One `DataTree` then carries `rhat`/`ess`/`loo`/`summary`/`psense`.
- `blackjax.util.psis_weights` and `arviz.loo` share the PSIS-LOO contract: the blackjax weights are the raw `(log_weights, kss)` pair, while `loo`/`ds.azstats.psislw` own the smoothing-plus-elpd rollup over a `log_likelihood` group. Prefer the arviz LOO family for the elpd receipt; use the blackjax weights only inside the sampler loop.
- The `extract(..., num_samples=, weights=, resampling_method=)` reshaper consumes `compare`/LOO stacking weights to produce model-averaged draws, matching `weight_predictions`; chain `compare -> extract`/`weight_predictions` for a BMA predictive without leaving xarray.
- `reloo`/`loo_kfold` require a `SamplingWrapper` subclass that re-runs the backend sampler on held-out folds — the one place arviz calls back into the sampler rail; implement `sel_observations`/`sample`/`get_inference_data`/`log_likelihood__i` over the chosen `SamplerBackend`.

[STUDY_ROUTING]:
- A `NumericIntent` Bayesian study converts the sampler output to a `DataTree`, then captures the convergence receipt (`rhat`/`ess`/`mcse` or `summary(kind='diagnostics')`) before any posterior claim, and the LOO/`compare` receipt before any model-selection claim. `psense_summary` is the prior-robustness receipt.
- Diagnostics are offline study work feeding the C# `Rasm.Compute` model rail; no production runtime imports arviz.

[RAIL_LAW]:
- Package: `arviz`
- Owns: backend-agnostic Bayesian diagnostics, the full PSIS-LOO family, stacking/BMA comparison, prior/likelihood sensitivity, predictive divergence/metrics, survival curves, credible intervals, the `.azstats` xarray accessor, the `rcParams` option registry, and posterior visualization from `xarray.DataTree`
- Accept: a `DataTree` posterior from any backend via `from_dict`/`from_numpyro`/`convert_to_datatree`, analyzed with `rhat`/`ess`/`loo`/`summary`/`psense` (free-function or chained `.azstats`), defaults tuned through `rcParams`/`rc_context`, with a captured diagnostic + LOO receipt before any model claim
- Reject: hand-rolled R-hat/ESS/PSIS-LOO/sensitivity arviz owns; reading nonexistent `elpd_loo`/`p_loo`/`hdi_3%` field names; visualization without a `DataTree` receipt; treating `InferenceData` as a container distinct from `DataTree`
