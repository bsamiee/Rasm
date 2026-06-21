# [PY_COMPUTE_API_PYMC]

`pymc` supplies a Python probabilistic-programming interface over PyTensor — declarative `Model` context management, ~60 distribution families (continuous, discrete, multivariate, mixture, truncation, time-series), gradient-based and gradient-free step methods, variational families, and posterior/prior predictive sampling — for the compute Bayesian-study rail. A model is built inside a `pm.Model()` context where named random variables become symbolic PyTensor graph nodes; `pm.sample` compiles the joint log-probability and runs NUTS, returning an `xarray.DataTree` posterior. It stacks across the Bayesian rail: the NUTS sampler dispatches to an external backend (`nutpie` Rust, `numpyro`/`blackjax` JAX) via `nuts_sampler=`, the returned `DataTree` is read by `arviz` for `rhat`/`ess`/`loo` diagnostics, and `pm.do`/`pm.observe` graph-surgery enables causal interventions. It never re-implements a distribution, sampler, or PyTensor graph operation the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymc`
- package: `pymc`
- import: `pymc` (alias `pm`); submodules `pymc.gp`, `pymc.math`, `pymc.distributions`, `pymc.step_methods`, `pymc.variational`, `pymc.smc`
- owner: `compute`
- rail: Bayesian-study
- installed: license Apache-2.0; `Requires-Python >=3.12`; backed by `pytensor` (3.0.x, the symbolic graph + C/Numba/JAX compile backend), reads/writes `arviz` 1.x `xarray.DataTree`; marker-gated `python_version<'3.15'` in the manifest because `pytensor`/`numba`/`llvmlite` ship no cp315 wheel — so `assay api resolve pymc` is `unsupported` on the cp315 core (uninstalled, marker-gated); surface confirmed against the pymc 6.0.x module API
- capability: PyTensor-backed probabilistic programming — declarative `Model` context, ~60 distribution families across continuous/discrete/multivariate/mixture/truncation/time-series, NUTS/HMC/Metropolis/SMC step methods with pluggable `nutpie`/`numpyro`/`blackjax` NUTS backends, ADVI/FullRank/SVGD variational families, causal `do`/`observe` graph surgery, GP module, `Minibatch` stochastic-VI data, and `xarray.DataTree` posterior/prior-predictive output

## [02]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: model and context types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]    | [CAPABILITY]                                                              |
| :-----: | :---------------- | :---------------- | :------------------------------------------------------------------------ |
|  [01]   | `Model`           | model context     | context manager for declaring random variables, data, dims, and coords    |
|  [02]   | `Data`            | data container    | mutable shared data tensor inside a model (replaces removed `ConstantData`/`MutableData`); swapped via `pm.set_data` |
|  [03]   | `Deterministic`   | deterministic     | records a named deterministic transformation in the trace                 |
|  [04]   | `Potential`       | log factor        | adds an arbitrary log factor (soft constraint / custom likelihood) to the joint |
|  [05]   | `Minibatch`       | stochastic data   | mini-batched data tensor for scalable stochastic VI                        |
|  [06]   | `modelcontext` / `set_data` | context access | resolves the active model / mutates a `Data` container in place         |

[PUBLIC_TYPE_SCOPE]: continuous distribution families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]       | [SUPPORT]          | [CANONICAL_PARAMS]  |
| :-----: | :------------- | :----------------- | :------------------ |
|  [01]   | `Normal`       | real line          | `mu`, `sigma`/`tau` |
|  [02]   | `Beta` / `Kumaraswamy` | (0, 1)     | `alpha`, `beta` / `a`, `b` |
|  [03]   | `Gamma` / `InverseGamma` | positive reals | `alpha`, `beta`  |
|  [04]   | `Exponential` / `Weibull` / `ChiSquared` | positive reals | `lam` / `alpha`, `beta` / `nu` |
|  [05]   | `StudentT`     | real line          | `nu`, `mu`, `sigma` |
|  [06]   | `HalfNormal` / `HalfCauchy` / `HalfStudentT` | non-negative reals | `sigma` / `beta` / `nu`, `sigma` |
|  [07]   | `LogNormal` / `Wald` / `Rice` | positive reals | `mu`, `sigma` / `mu`, `lam` / `nu`, `sigma` |
|  [08]   | `Uniform` / `Triangular` | bounded interval | `lower`, `upper` / `lower`, `c`, `upper` |
|  [09]   | `Cauchy` / `Laplace` / `AsymmetricLaplace` | real line | `alpha`, `beta` / `mu`, `b` / `b`, `kappa`, `mu` |
|  [10]   | `Logistic` / `Gumbel` / `Moyal` | real line | `mu`, `s` / `mu`, `beta` / `mu`, `sigma` |
|  [11]   | `SkewNormal` / `ExGaussian` / `VonMises` | real / circular | `mu`, `sigma`, `alpha` / `mu`, `sigma`, `nu` / `mu`, `kappa` |
|  [12]   | `Pareto`       | above a minimum    | `alpha`, `m`        |
|  [13]   | `Interpolated` / `PolyaGamma` | empirical / aux | `x_points`, `pdf_points` / `h`, `z` |

[PUBLIC_TYPE_SCOPE]: discrete, multivariate, mixture, and time-series families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]           | [SUPPORT]                  | [CANONICAL_PARAMS]    |
| :-----: | :----------------- | :------------------------- | :-------------------- |
|  [01]   | `Bernoulli` / `Binomial` / `BetaBinomial` | {0,1} / integers | `p` / `n`, `p` / `n`, `alpha`, `beta` |
|  [02]   | `Categorical` / `DiscreteUniform` | finite set / integer range | `p` / `lower`, `upper` |
|  [03]   | `Poisson` / `NegativeBinomial` / `Geometric` | non-negative integers | `mu` / `mu`, `alpha` / `p` |
|  [04]   | `ZeroInflatedPoisson` / `ZeroInflatedBinomial` / `ZeroInflatedNegativeBinomial` / `DiscreteWeibull` | zero-inflated counts | `psi`, ... |
|  [05]   | `MvNormal` / `MvStudentT` | real vector space     | `mu`, `cov`/`chol` / `nu`, `mu`, `scale` |
|  [06]   | `Dirichlet` / `Multinomial` / `DirichletMultinomial` | simplex / counts | `a` / `n`, `p` / `n`, `a` |
|  [07]   | `LKJCholeskyCov` / `LKJCorr` / `Wishart` | correlation / PD matrices | `eta`, `n`, `sd_dist` / `eta`, `n` / `nu`, `V` |
|  [08]   | `MatrixNormal` / `KroneckerNormal` | matrix-variate Gaussian | `mu`, `rowcov`, `colcov` / `mu`, `covs` |
|  [09]   | `CAR` / `ICAR`     | spatial lattice            | `mu`, `W`, `alpha`, `tau` |
|  [10]   | `Mixture` / `NormalMixture` | mixture of components | `w`, `comp_dists` / `w`, `mu`, `sigma` |
|  [11]   | `Truncated` / `Censored` | restricted-support wrapper | `dist`, `lower`, `upper` |
|  [12]   | `CustomDist` / `Simulator` | user-defined / likelihood-free | `dist`/`logp`/`random` / `fn`, `params`, `distance` |
|  [13]   | `GaussianRandomWalk` / `RandomWalk` / `AR` / `GARCH11` / `EulerMaruyama` / `MvGaussianRandomWalk` | time-series | sequential-dependence processes |
|  [14]   | `Flat` / `HalfFlat` | improper prior            | (unnormalized) flat / half-flat |

[PUBLIC_TYPE_SCOPE]: step methods and samplers
- rail: Bayesian-study

| [INDEX] | [SYMBOL]        | [SAMPLER_FAMILY] | [CAPABILITY]                                            |
| :-----: | :-------------- | :--------------- | :------------------------------------------------------ |
|  [01]   | `NUTS`          | gradient-based   | `(vars=None, max_treedepth=10, early_max_treedepth=8, **kwargs)` |
|  [02]   | `HamiltonianMC` | gradient-based   | `(vars, path_length, max_steps)`                        |
|  [03]   | `Metropolis` / `DEMetropolis` / `DEMetropolisZ` | gradient-free | random-walk / differential-evolution / DE-Z Metropolis |
|  [04]   | `BinaryMetropolis` / `BinaryGibbsMetropolis` / `CategoricalGibbsMetropolis` | gradient-free | discrete-variable step methods |
|  [05]   | `Slice`         | gradient-free    | slice sampling step method                              |
|  [06]   | `step_methods.CompoundStep` | composite | assigns distinct step methods per variable block        |

[PUBLIC_TYPE_SCOPE]: variational families
- rail: Bayesian-study

| [INDEX] | [SYMBOL]       | [VI_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `ADVI`         | mean-field VI | automatic-differentiation VI (diagonal Gaussian)          |
|  [02]   | `FullRankADVI` | full-rank VI  | full-rank Gaussian ADVI                                   |
|  [03]   | `SVGD` / `ASVGD` | particle VI | Stein variational gradient descent / amortized SVGD       |
|  [04]   | `Empirical`    | trace VI      | empirical approximation built from an existing trace      |

## [03]-[ENTRYPOINTS_AND_OPERATIONS]

[ENTRYPOINT_SCOPE]: sampling and inference entrypoints
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `sample(draws=1000, *, tune=None, chains=None, cores=None, random_seed=None, step=None, nuts_sampler=Literal['pymc','nutpie','numpyro','blackjax'], initvals=None, init='auto', return_inferencedata=True, idata_kwargs=None, nuts_sampler_kwargs=None, model=None, **kwargs)` → `DataTree | MultiTrace | ZarrTrace` | MCMC | primary MCMC entrypoint; dispatches NUTS to the chosen backend |
|  [02]   | `sample_posterior_predictive(trace, model=None, *, var_names=None, sample_dims=None, random_seed=None, return_inferencedata=True, extend_inferencedata=False, predictions=False)` → `DataTree | dict` | posterior | posterior predictive samples from a fitted model |
|  [03]   | `sample_prior_predictive(samples=500, model=None, *, var_names=None, random_seed=None, return_inferencedata=True)` | prior | prior predictive samples before conditioning |
|  [04]   | `sample_smc(draws=2000, kernel=IMH, *, model=None, chains=None, cores=None, return_inferencedata=True, **kernel_kwargs)` → `DataTree | MultiTrace` | SMC | sequential Monte Carlo (IMH/MH kernel) |
|  [05]   | `fit(n=10000, method='advi', model=None, random_seed=None, start=None, start_sigma=None, inf_kwargs=None, *, backend=None, **kwargs)` → approximation | VI | variational inference fit |
|  [06]   | `find_MAP(start=None, vars=None, method='L-BFGS-B', maxeval=5000, model=None, *, seed=None)` | optimization | MAP point estimate via gradient optimization |
|  [07]   | `find_constrained_prior(distribution, lower, upper, init_guess, mass=0.95, fixed_params=None)` → `dict[str, float]` | prior tuning | solves prior hyperparameters so `mass` lies in `[lower, upper]` |

[ENTRYPOINT_SCOPE]: causal surgery, diagnostics, log-probability, and graph
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :----------------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `do(model, vars_to_interventions, *, make_interventions_shared=True, prune_vars=False)` → `Model` | causal | do-operator intervention (replaces a node with a fixed/exogenous value) |
|  [02]   | `observe(model, vars_to_observations)` → `Model`                        | causal           | converts an unobserved RV into an observed one  |
|  [03]   | `compute_log_likelihood(idata, *, var_names=None, extend_inferencedata=True, model=None, sample_dims=('chain','draw'))` | diagnostics | per-observation log-likelihood (feeds `arviz.loo`/`waic`) |
|  [04]   | `to_inference_data(trace, model=None)` / `predictions_to_inference_data(pred, idata)` | conversion | trace → `xarray.DataTree`; attach predictions |
|  [05]   | `logp(rv, value, warn_rvs=True)` / `logcdf(rv, value, warn_rvs=True)` → PyTensor graph | log-prob | symbolic log-density / log-CDF of a distribution |
|  [06]   | `draw(vars, draws=1, random_seed=None)` → `ndarray | list[ndarray]`      | sampling utility | forward-samples model variables (no conditioning) |
|  [07]   | `model_to_graphviz(model=None, *, var_names=None, formatting='plain', save=None)` | inspection | renders the model DAG via graphviz |

[ENTRYPOINT_SCOPE]: Gaussian-process and math submodules
- rail: Bayesian-study

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY]   | [RAIL]                                          |
| :-----: | :----------------------------------------------------------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `gp.Latent` / `gp.Marginal` / `gp.MarginalApprox` / `gp.TP` / `gp.HSGP` / `gp.LatentKron` / `gp.MarginalKron` | GP prior | latent / marginal / approximate / Student-T-process / Hilbert-space GP families |
|  [02]   | `gp.cov.*` / `gp.mean.*`                                                 | GP kernel        | covariance (`ExpQuad`, `Matern52`, `Periodic`, ...) and mean functions |
|  [03]   | `math.*` (`logsumexp`, `invlogit`, `dot`, `stack`, `switch`, ...)        | tensor ops       | PyTensor tensor primitives for `Deterministic`/`Potential` expressions |

## [04]-[LOCAL_ADMISSION]

[BACKEND_INTEGRATION]:
- NUTS backend dispatch: `sample(nuts_sampler='nutpie')` runs the Rust `nutpie` sampler, `'numpyro'`/`'blackjax'` run JAX samplers, `'pymc'` (default) the C/Numba PyTensor sampler; backend-specific options pass through `nuts_sampler_kwargs`. The choice is a marker on the same model graph, not a re-modelled problem.
- arviz receipt: every `sample`/`sample_posterior_predictive`/`sample_prior_predictive` returns an `xarray.DataTree` (`return_inferencedata=True`), read by `arviz` for `rhat`/`ess`/`mcse`/`loo`/`compare`; `compute_log_likelihood` populates the `log_likelihood` group LOO/WAIC require.
- pytensor backend: random variables are symbolic PyTensor nodes; `pm.math`/`Deterministic`/`Potential` build differentiable graph expressions PyTensor compiles to C/Numba/JAX — never NumPy-eager numerics inside the model context.

[RAIL_LAW]:
- Package: `pymc`
- Owns: PyTensor-backed probabilistic model construction, ~60 distribution families, MCMC/SMC/VI inference with pluggable NUTS backends, causal `do`/`observe` graph surgery, and posterior/prior predictive sampling
- Accept: a model defined inside a `pm.Model()` context with named sample sites, run through `pm.sample`/`pm.sample_smc`/`pm.fit`, returning a `DataTree` receipt with captured draws, tune steps, chains, and `arviz` convergence checks
- Reject: hand-rolled distributions or samplers pymc owns; pymc models outside a `Model()` context; NumPy-eager numerics where a PyTensor graph is required; product serving of posteriors; benchmark claims without a `DataTree` receipt
