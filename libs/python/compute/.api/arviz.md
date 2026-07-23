# [PY_COMPUTE_API_ARVIZ]

`arviz` owns backend-agnostic Bayesian posterior analysis for the compute Bayesian-study rail: MCMC convergence diagnostics, the PSIS-LOO model-comparison family, prior/likelihood sensitivity, predictive divergence and metrics, credible intervals, and posterior visualization over an `xarray.DataTree`. It reads a posterior `DataTree` emitted by any sampler backend and never re-implements a diagnostic the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `arviz`
- package: `arviz` (Apache-2.0)
- import: `arviz` (alias `az`)
- owner: `compute`
- rail: Bayesian-study
- capability: `xarray.DataTree` I/O and converters, MCMC convergence diagnostics, the full PSIS-LOO family, stacking/BMA comparison, prior/likelihood sensitivity, predictive divergence and metrics, survival curves, HDI/ETI/ROPE intervals, the chained `.azstats` accessor, the validated `rcParams`/`rc_context` registry, and the `plot_*` visualization surface

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: result and container types

| [INDEX] | [SYMBOL]                                             | [PACKAGE_ROLE]    | [CAPABILITY]                                             |
| :-----: | :--------------------------------------------------- | :---------------- | :------------------------------------------------------- |
|  [01]   | `xarray.DataTree`                                    | posterior store   | `InferenceData` binds the same class; 5 named groups     |
|  [02]   | `ELPDData`                                           | LOO result        | `arviz_stats.utils.ELPDData`; plain object, fields below |
|  [03]   | `PlotCollection`                                     | plot container    | faceted plot panel collection (`arviz_plots`)            |
|  [04]   | `PlotMatrix`                                         | plot container    | matrix of plot panels                                    |
|  [05]   | `SamplingWrapper`                                    | refit protocol    | `reloo`/`loo_kfold` held-out refit base class            |
|  [06]   | `MCMCAdapter`/`NumPyroInferenceAdapter`/`SVIAdapter` | inference adapter | numpyro MCMC/SVI -> `DataTree` adapters                  |
|  [07]   | `AzStats{Ds,Da,Dt}Accessor`                          | xarray accessor   | `.azstats` on `Dataset`/`DataArray`/`DataTree`           |
|  [08]   | `rcParams` / `rc_context`                            | option registry   | `data`/`stats`/`plot` defaults; `rc_context` overrides   |

[PUBLIC_TYPE_SCOPE]: `ELPDData` fields (`arviz_stats.utils.ELPDData`)
- note: the scalar is `elpd`, the effective-parameter count is `p`, scale rides `scale`; no `elpd_loo`/`p_loo`/`looic` attribute exists

| [INDEX] | [FIELD]                      | [TYPE]      | [ROLE]                                                       |
| :-----: | :--------------------------- | :---------- | :----------------------------------------------------------- |
|  [01]   | `kind`                       | `str`       | `'loo'` / `'loo_subsample'` / `'loo_kfold'` discriminator    |
|  [02]   | `elpd`                       | `float`     | expected log pointwise predictive density                    |
|  [03]   | `se`                         | `float`     | standard error of `elpd`                                     |
|  [04]   | `p`                          | `float`     | effective number of parameters                               |
|  [05]   | `n_samples`                  | `int`       | posterior draw count                                         |
|  [06]   | `n_data_points`              | `int`       | observation count                                            |
|  [07]   | `scale`                      | `str`       | `'log'` / `'negative_log'` / `'deviance'`                    |
|  [08]   | `warning`                    | `bool`      | high-Pareto-k warning flag                                   |
|  [09]   | `good_k`                     | `float`     | Pareto k reliability threshold (sample-size dependent)       |
|  [10]   | `elpd_i`                     | `DataArray` | pointwise elpd when `pointwise=True`                         |
|  [11]   | `pareto_k`                   | `DataArray` | per-observation Pareto k when `pointwise=True`               |
|  [12]   | `approx_posterior`           | `bool`      | approximate-posterior LOO flag (`loo_approximate_posterior`) |
|  [13]   | `elpd_loo_approx`            | `DataArray` | approximate-posterior LOO carrier                            |
|  [14]   | `log_p`                      | `DataArray` | approximate-posterior LOO carrier                            |
|  [15]   | `log_q`                      | `DataArray` | approximate-posterior LOO carrier                            |
|  [16]   | `subsample_size`             | `int`       | subsampled-LOO carrier (`loo_subsample`)                     |
|  [17]   | `subsampling_se`             | `float`     | subsampled-LOO carrier (`loo_subsample`)                     |
|  [18]   | `loo_subsample_observations` | `ndarray`   | subsampled-LOO carrier (`loo_subsample`)                     |
|  [19]   | `n_eff_i`                    | `DataArray` | subsampled-LOO carrier (`loo_subsample`)                     |
|  [20]   | `n_folds`                    | `int`       | k-fold count (`loo_kfold`)                                   |
|  [21]   | `influence_pareto_k`         | `DataArray` | moment-match carrier                                         |
|  [22]   | `thin_factor`                | `DataArray` | thinning carrier                                             |
|  [23]   | `log_jacobian`               | `DataArray` | Jacobian carrier                                             |
|  [24]   | `log_weights`                | `DataArray` | importance log-weights carrier                               |

[PUBLIC_TYPE_SCOPE]: I/O converters (`arviz_base`)
- carry: every `from_*`/`convert_*` returns a `DataTree` sharing the `coords`, `dims`, `check_conventions=True` tail and the group-routing kwargs (`prior`, `posterior_predictive`, `predictions`, `constant_data`, `log_likelihood`, `save_warmup`); signatures keyed below

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]  | [SOURCE]                                               |
| :-----: | :------------------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `from_dict`                                  | converter       | nested `{group: {var: ndarray}}` dict                  |
|  [02]   | `from_netcdf`                                | converter       | NetCDF4 file (xarray backend kwargs threaded)          |
|  [03]   | `from_zarr`                                  | converter       | Zarr store                                             |
|  [04]   | `from_numpyro`                               | converter       | numpyro MCMC object                                    |
|  [05]   | `from_numpyro_svi`                           | converter       | numpyro SVI object                                     |
|  [06]   | `from_cmdstanpy`                             | converter       | CmdStanPy fit                                          |
|  [07]   | `from_emcee`                                 | converter       | emcee `EnsembleSampler`                                |
|  [08]   | `convert_to_datatree` / `convert_to_dataset` | universal       | dict / xr object / supported sampler                   |
|  [09]   | `dict_to_dataset`                            | dataset builder | dict -> `xr.Dataset`                                   |
|  [10]   | `extract`                                    | reshaper        | flatten `(chain, draw)` -> `sample`; weighted resample |

- [01]-[FROM_DICT]: `from_dict(data, *, name, sample_dims, save_warmup, index_origin, coords, dims, pred_dims, pred_coords, attrs, check_conventions=True)`
- [02]-[FROM_NETCDF]: `from_netcdf(filename_or_obj, *, engine, chunks, decode_cf, ...) -> DataTree`
- [03]-[FROM_ZARR]: `from_zarr(filename_or_obj, *, engine='zarr', chunks, ...) -> DataTree`
- [04]-[FROM_NUMPYRO]: `from_numpyro(posterior=None, *, prior, posterior_predictive, predictions, constant_data, log_likelihood=False, num_chains, coords, dims, ...)`
- [05]-[FROM_NUMPYRO_SVI]: `from_numpyro_svi(svi=None, *, svi_result, model_args, model_kwargs, prior, posterior_predictive, predictions, log_likelihood=False, num_samples=1000, ...)`
- [06]-[FROM_CMDSTANPY]: `from_cmdstanpy(posterior=None, *, posterior_predictive, prior, log_likelihood, observed_data, save_warmup, dtypes, ...)`
- [07]-[FROM_EMCEE]: `from_emcee(sampler=None, var_names, slices, arg_names, arg_groups, blob_names, blob_groups, coords, dims, check_conventions=True)`
- [08]-[CONVERT]: `convert_to_datatree(obj, **kwargs)` / `convert_to_dataset(obj, *, group='posterior', **kwargs)`
- [09]-[DICT_TO_DATASET]: `dict_to_dataset(data, *, attrs, inference_library, coords, dims, sample_dims, index_origin, skip_event_dims=False, check_conventions=True)`
- [10]-[EXTRACT]: `extract(data, group='posterior', sample_dims, *, combined=True, var_names, filter_vars, num_samples, weights, resampling_method, keep_dataset=False, random_seed)`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MCMC diagnostics (`arviz_stats`)
- carry: each shares `(data, sample_dims, group, var_names, filter_vars, chain_axis=0, draw_axis=1)` and accepts a `DataTree` or a raw ndarray via `chain_axis=`/`draw_axis=`; the cell carries the discriminating `method`/threshold args

| [INDEX] | [SURFACE]     | [ENTRY_FAMILY] | [RAIL]                                                                                             |
| :-----: | :------------ | :------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | `rhat`        | convergence    | rank-normalized R-hat; `method='rank'`                                                             |
|  [02]   | `rhat_nested` | convergence    | nested R-hat for superchain ensembles; `superchain_ids`, `method='rank'`                           |
|  [03]   | `ess`         | convergence    | ESS; `method=` `bulk`/`tail`/`quantile`/`mean`/`sd`/`median`/`mad`/`local`, `relative`, `prob`     |
|  [04]   | `mcse`        | convergence    | Monte Carlo std error; `method='mean'`, `prob`, `circular`                                         |
|  [05]   | `bfmi`        | HMC diagnostic | Bayesian fraction of missing info; `group='sample_stats'`, `var_names='energy'`                    |
|  [06]   | `summary`     | summary table  | combined `pandas.DataFrame`; `kind='all'`, `fmt='wide'`, `ci_prob`, `ci_kind`, `round_to`          |
|  [07]   | `diagnose`    | diagnostics    | convergence warnings; `rhat_max=1.01`, `bfmi_threshold=0.3`, `ess_min_ratio`, `return_diagnostics` |
|  [08]   | `thin`        | thinning       | autocorrelation-aware draw thinning; `factor='auto'`                                               |

[ENTRYPOINT_SCOPE]: model comparison and the LOO family (`arviz_stats`)
- carry: every entry takes `data` and returns an `ELPDData` (a `DataFrame` for `compare`); `pointwise`, `var_name`, `reff`, `log_weights` are the shared optional tail, full signatures keyed below

| [INDEX] | [SURFACE]                   | [ENTRY_FAMILY]      | [RAIL]                                                     |
| :-----: | :-------------------------- | :------------------ | :--------------------------------------------------------- |
|  [01]   | `loo`                       | model comparison    | PSIS-LOO-CV with Pareto-smoothed importance weights        |
|  [02]   | `compare`                   | model comparison    | stacking / `'BB-pseudo-BMA'` weights across models         |
|  [03]   | `loo_subsample`             | scalable LOO        | subsampled PSIS-LOO for large `n`                          |
|  [04]   | `update_subsample`          | scalable LOO        | grow a subsampled-LOO with more observations               |
|  [05]   | `loo_approximate_posterior` | importance sampling | LOO under an approximate (VI/Laplace) posterior            |
|  [06]   | `loo_moment_match`          | moment matching     | moment-matched LOO for high-k observations                 |
|  [07]   | `reloo`                     | exact refit         | exact LOO refitting high-k points via `SamplingWrapper`    |
|  [08]   | `loo_kfold`                 | cross-validation    | k-fold LOO-CV via a `SamplingWrapper`                      |
|  [09]   | `loo_pit`                   | calibration         | LOO probability-integral-transform calibration             |
|  [10]   | `loo_expectations`          | expectation         | LOO posterior-predictive expectations / quantiles          |
|  [11]   | `loo_metrics`               | predictive error    | LOO `rmse`/`mae`/`mse`/`acc` predictive metric             |
|  [12]   | `loo_r2`                    | predictive fit      | LOO Bayesian R^2; also `loo_score`/`loo_influence`/`loo_i` |

- [01]-[LOO]: `loo(data, pointwise, var_name, reff, log_lik_fn, log_weights, pareto_k, log_jacobian, mixture=False) -> ELPDData`
- [02]-[COMPARE]: `compare(compare_dict, method='stacking', var_name, reference, round_to='auto') -> DataFrame`
- [03]-[LOO_SUBSAMPLE]: `loo_subsample(data, observations, pointwise, var_name, reff, log_weights, log_p, log_q, seed=315, method='lpd', thin, log_lik_fn, param_names, log=True, ...)`
- [04]-[UPDATE_SUBSAMPLE]: `update_subsample(loo_orig, data, observations, var_name, reff, ...)`
- [05]-[LOO_APPROXIMATE_POSTERIOR]: `loo_approximate_posterior(data, log_p, log_q, pointwise, var_name, log_jacobian)`
- [06]-[LOO_MOMENT_MATCH]: `loo_moment_match(data, loo_orig, log_prob_upars_fn, log_lik_i_upars_fn, upars, var_name, reff, max_iters=30, k_threshold, split=True, cov=True)`
- [07]-[RELOO]: `reloo(wrapper, loo_orig, var_name, log_weights, k_threshold=-inf, pointwise)`
- [08]-[LOO_KFOLD]: `loo_kfold(data, wrapper, pointwise, var_name, k=10, folds, stratify_by, group_by, save_fits=False)`
- [09]-[LOO_PIT]: `loo_pit(data, var_names, log_weights, pareto_k, random_state, pareto_pit=False)`
- [10]-[LOO_EXPECTATIONS]: `loo_expectations(data, var_name, group='posterior_predictive', sample_dims, log_likelihood_var_name, kind='mean', probs, log_weights, pareto_k)`
- [11]-[LOO_METRICS]: `loo_metrics(data, kind='rmse', var_name, round_to)`
- [12]-[LOO_R2]: `loo_r2(data, var_name, n_simulations=4000, summary=True, point_estimate, ci_kind, ci_prob, ...)` / `loo_score` / `loo_influence` / `loo_i`

[ENTRYPOINT_SCOPE]: sensitivity, predictive divergence, and credible intervals (`arviz_stats`)

| [INDEX] | [SURFACE]                                   | [ENTRY_FAMILY]   | [RAIL]                                                     |
| :-----: | :------------------------------------------ | :--------------- | :--------------------------------------------------------- |
|  [01]   | `psense`                                    | sensitivity      | power-scaling prior/likelihood sensitivity per variable    |
|  [02]   | `psense_summary`                            | sensitivity      | prior-vs-likelihood sensitivity diagnosis table            |
|  [03]   | `wasserstein`                               | divergence       | Wasserstein distance between two posteriors                |
|  [04]   | `kl_divergence`                             | divergence       | KL divergence between two posteriors                       |
|  [05]   | `bayes_factor`                              | Bayes factor     | Savage-Dickey density-ratio Bayes factor                   |
|  [06]   | `bayesian_r2` / `residual_r2`               | model fit        | in-sample and residual-based Bayesian R^2                  |
|  [07]   | `metrics`                                   | predictive error | in-sample `rmse`/`mae`/`mse`; counterpart of `loo_metrics` |
|  [08]   | `kaplan_meier` / `generate_survival_curves` | survival         | Kaplan-Meier over observed and predictive samples          |

- [01]-[PSENSE]: `psense(data, var_names, group='prior', coords, sample_dims, alphas=(0.99, 1.01), group_var_names, group_coords)`
- [02]-[PSENSE_SUMMARY]: `psense_summary(data, var_names, filter_vars, threshold=0.05, alphas=(0.99, 1.01), prior_var_names, likelihood_var_names, round_to=3)`
- [03]-[WASSERSTEIN]: `wasserstein(data1, data2, group, var_names, sample_dims, joint=True, num_samples=500, random_seed=212480)`
- [04]-[KL_DIVERGENCE]: `kl_divergence(data1, data2, group, var_names, sample_dims, num_samples=500, random_seed=212480)`
- [05]-[BAYES_FACTOR]: `bayes_factor(data, var_names, ref_vals=0, return_ref_vals=False, prior, circular=False)`
- [06]-[BAYESIAN_R2]: `bayesian_r2(data, pred_mean, scale, scale_kind='sd', summary=True, group='posterior', point_estimate, ci_kind, ci_prob, ...)` / `residual_r2(data, pred_mean, obs_name, summary=True, group='posterior', ...)`
- [07]-[METRICS]: `metrics(data, kind='rmse', var_name, sample_dims, round_to)`
- [08]-[SURVIVAL]: `kaplan_meier(dt, var_names)` / `generate_survival_curves(dt, var_names, group='posterior_predictive', num_samples, extrapolation_factor=1.2)`

[ENTRYPOINT_SCOPE]: credible intervals, decision, point statistics, density, and stacking (`arviz_stats`)

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                                                                |
| :-----: | :--------------------------------------------- | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `hdi`                                          | interval       | highest-density interval; `ci_bound` coord, multimodal to `max_modes` |
|  [02]   | `eti`                                          | interval       | equal-tailed interval; `ci_bound` coord                               |
|  [03]   | `ci_in_rope`                                   | decision       | fraction of a credible interval inside a ROPE                         |
|  [04]   | `mean`/`median`/`mode`/`std`/`var`/`mad`/`iqr` | statistic      | posterior point/spread statistics per variable                        |
|  [05]   | `kde`/`kde2d`/`histogram`/`ecdf`/`qds`         | density        | 1-D/2-D KDE, histogram, empirical CDF / PIT-ECDF, quantile dots       |
|  [06]   | `weight_predictions`                           | stacking       | model-averaged predictive draws from `compare` weights                |

- [01]-[HDI]: `hdi(data, prob, dim, group='posterior', var_names, method='nearest', circular=False, max_modes=10, skipna=False)` — a `DataTree`/Dataset with a `ci_bound` coord (`'lower'`/`'upper'`)
- [02]-[ETI]: `eti(data, prob, dim, group, var_names, method='linear', skipna=False)`
- [03]-[CI_IN_ROPE]: `ci_in_rope(data, rope, var_names, group='posterior', dim, ci_prob, ci_kind, rope_dim='rope_dim')`
- [04]-[STATISTIC]: `mean` / `median` / `mode` / `std` / `var` / `mad` / `iqr` `(data, group, var_names)`
- [05]-[DENSITY]: `kde(data, ...)` / `kde2d` / `histogram` / `ecdf(data, ..., pit=False)` / `qds(data, dim, group, var_names, nquantiles=100, ...)`
- [06]-[WEIGHT_PREDICTIONS]: `weight_predictions(dts, weights, group='posterior_predictive', sample_dims, random_seed)`

[ENTRYPOINT_SCOPE]: chained `.azstats` xarray accessor (`arviz_stats`)
- carry: every diagnostic above rides as a method `ds.azstats.<name>(...)` on a `Dataset`/`DataTree`, so a posterior carries its own diagnostic algebra without a free-function round-trip; each brace set lists the group's `<name>`s

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `{ess, rhat, rhat_nested, mcse, bfmi, thin}`           | diagnostic     | chained convergence diagnostics              |
|  [02]   | `{hdi, eti, mean, median, mode, std, var, mad, iqr}`   | statistic      | chained credible interval / point statistics |
|  [03]   | `{loo, loo_pit, loo_r2, loo_score, loo_expectation}`   | LOO            | chained LOO family                           |
|  [04]   | `{loo_summary, loo_quantile, loo_mixture}`             | LOO            | chained LOO summaries and mixtures           |
|  [05]   | `{loo_approximate_posterior}`                          | LOO            | chained approximate-posterior LOO            |
|  [06]   | `{psislw, pareto_khat, pareto_min_ss}`                 | PSIS core      | raw PSIS weights, k-hat, min sample size     |
|  [07]   | `{power_scale_lw, power_scale_sense}`                  | PSIS core      | power-scaling log-weights and sensitivity    |
|  [08]   | `{kde, ecdf, histogram, qds}`                          | density        | chained density estimates                    |
|  [09]   | `{compute_ranks, autocorr, uniformity_test, get_bins}` | density        | rank, autocorrelation, uniformity, binning   |

[ENTRYPOINT_SCOPE]: visualization functions (`arviz_plots`)
- carry: each `plot_<name>` accepts a `DataTree`/`Dataset` and returns a `PlotCollection`/`PlotMatrix`; backend rides `rcParams["plot.backend"]` (`matplotlib`/`bokeh`/`plotly`), templates ride `style.use(name)`, and `combine_plots`/`add_lines`/`add_bands` are the composition primitives

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]   | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------- | :--------------- | :-------------------------------------------------- |
|  [01]   | `trace`/`trace_dist`/`rank`/`rank_dist`                        | trace/rank       | trace, rank histogram, rank-trace per variable      |
|  [02]   | `forest`/`ridge`/`pair`/`pair_focus`/`parallel`                | marginal/joint   | forest, ridge, pairwise joint, parallel-coordinate  |
|  [03]   | `dist`/`convergence_dist`/`autocorr`                           | diagnostic       | marginal distribution, convergence, autocorrelation |
|  [04]   | `ess`/`ess_evolution`/`mcse`                                   | diagnostic       | ESS, ESS-evolution, MCSE                            |
|  [05]   | `energy`/`khat`                                                | HMC/LOO          | energy-BFMI, Pareto-k scatter                       |
|  [06]   | `compare`/`loo_pit`/`loo_interval`/`bf`                        | model comparison | ELPD comparison, LOO-PIT calibration, Bayes factor  |
|  [07]   | `ppc_dist`/`ppc_pit`/`ppc_interval`/`ppc_rootogram`/`ppc_pava` | PPC              | predictive dist, PIT, interval, rootogram, PAVA     |
|  [08]   | `ppc_tstat`/`ppc_censored`/`ppc_dist_pit`/`ppc_pava_residuals` | PPC              | test-stat, censored, dist-PIT, PAVA-residual        |
|  [09]   | `prior_posterior`/`psense_dist`/`psense_quantities`            | sensitivity      | prior-vs-posterior overlay, power-scaling           |
|  [10]   | `dgof`/`dgof_dist`/`ecdf_pit`/`lm`                             | goodness-of-fit  | GOF, ECDF-PIT, linear-model plots                   |
|  [11]   | `combine_plots`/`add_lines`/`add_bands`                        | composition      | plot combination and overlay primitives             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `arviz` re-exports three owners: `arviz_base` owns the converters, `extract`, `dict_to_dataset`, and the `rcParams`/`rc_context` registry; `arviz_stats` owns every diagnostic, the LOO family, sensitivity, intervals, and the `.azstats` accessor; `arviz_plots` owns the `plot_*` surface, `PlotCollection`/`PlotMatrix`, and the `style` module (`use`/`available`/`get`).
- `xarray.DataTree` is the posterior container and `arviz.InferenceData` binds the same class, so both spellings accept one container; groups ride as children (`posterior`, `sample_stats`, `log_likelihood`, `observed_data`, `posterior_predictive`).
- `rcParams` feeds every entrypoint whose kwarg resolves `None` — `data.sample_dims=('chain','draw')`, `stats.ci_prob=0.89`, `stats.ci_kind='eti'`, `stats.point_estimate`, `stats.module`, `plot.backend='auto'` — and `rc_context(rc=...)` scopes overrides.
- `summary` returns a `pandas.DataFrame` indexed by flattened variable name (`theta[0]`); `kind='stats'` yields point/interval columns, `kind='diagnostics'` the `ess_bulk`/`ess_tail`/`r_hat`/`mcse_*` columns, `kind='all'` both, and `all_median`/`diagnostics_median`/`mc_diagnostics` swap median/MC variants. Interval columns are ETI by default (`ci_kind`), and `r_hat` carries the underscore.

[STACKING]:
- `pymc`(`.api/pymc.md`): `pm.sample`/`sample_posterior_predictive`/`sample_prior_predictive` under any `nuts_sampler=` backend return the `xarray.DataTree` this surface reads through `from_dict`/`convert_to_datatree`, then `rhat`/`ess`/`loo`/`summary`/`psense`; `pm.compute_log_likelihood` populates the `log_likelihood` group `loo`/`compare` consume.
- within-lib: the `.azstats` accessor mirrors every free diagnostic as a chained `ds.azstats.<op>()` method; `compare -> extract`/`weight_predictions` folds stacking weights into model-averaged draws without leaving xarray; `reloo`/`loo_kfold` drive a `SamplingWrapper` subclass (`sel_observations`/`sample`/`get_inference_data`/`log_likelihood__i`) for exact held-out refit.

[LOCAL_ADMISSION]:
- arviz is offline study work reading the `pm.sample` `DataTree`: capture the `rhat`/`ess`/`mcse` (or `summary(kind='diagnostics')`) receipt before any posterior claim, the `loo`/`compare` receipt before any model-selection claim, and `psense_summary` as the prior-robustness receipt. Diagnostics feed the C# `Rasm.Compute` model rail; no production runtime imports arviz.

[RAIL_LAW]:
- Package: `arviz`
- Owns: backend-agnostic Bayesian diagnostics, the full PSIS-LOO family, stacking/BMA comparison, prior/likelihood sensitivity, predictive divergence and metrics, survival curves, credible intervals, the `.azstats` accessor, the `rcParams` registry, and posterior visualization over `xarray.DataTree`
- Accept: a `DataTree` posterior via `from_dict`/`convert_to_datatree`, analyzed with `rhat`/`ess`/`loo`/`summary`/`psense` (free-function or chained `.azstats`), defaults tuned through `rcParams`/`rc_context`, with a captured diagnostic + LOO receipt before any model claim
- Reject: hand-rolled R-hat/ESS/PSIS-LOO/sensitivity arviz owns; reading nonexistent `elpd_loo`/`p_loo`/`hdi_3%` field names; visualization without a `DataTree` receipt; treating `InferenceData` as a container distinct from `DataTree`
