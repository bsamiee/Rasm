# [PY_COMPUTE_API_PYMC]

`pymc` owns PyTensor-backed probabilistic programming for the compute Bayesian-study rail: a declarative `Model` context where named random variables become symbolic PyTensor nodes, ~60 distribution families, gradient-based and gradient-free step methods, variational families, SMC, and posterior/prior-predictive sampling. `pm.sample` compiles the joint log-probability, dispatches NUTS to a pluggable backend, and returns an `xarray.DataTree` the `arviz` catalog reads; it never re-implements a distribution, sampler, or PyTensor graph operation the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pymc`
- package: `pymc`
- import: `pymc` (alias `pm`); submodules `pymc.gp`, `pymc.math`, `pymc.distributions`, `pymc.step_methods`, `pymc.variational`, `pymc.smc`
- owner: `compute`
- rail: Bayesian-study
- capability: PyTensor-backed probabilistic programming — declarative `Model` context, ~60 distribution families across continuous/discrete/multivariate/mixture/truncation/time-series, NUTS/HMC/Metropolis/SMC step methods with pluggable `nutpie`/`numpyro`/`blackjax` NUTS backends, ADVI/FullRank/SVGD variational families, causal `do`/`observe` graph surgery, a Gaussian-process module, and `xarray.DataTree` posterior/prior-predictive output

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model and context types

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]                                                                    |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------------------------------------ |
|  [01]   | `Model`                     | model context   | context manager for declaring random variables, data, dims, and coords          |
|  [02]   | `Data`                      | data container  | mutable shared data tensor in a model; swapped via `pm.set_data`                |
|  [03]   | `Deterministic`             | deterministic   | records a named deterministic transformation in the trace                       |
|  [04]   | `Potential`                 | log factor      | adds an arbitrary log factor (soft constraint / custom likelihood) to the joint |
|  [05]   | `Minibatch`                 | stochastic data | mini-batched data tensor for scalable stochastic VI                             |
|  [06]   | `modelcontext` / `set_data` | context access  | resolves the active model / mutates a `Data` container in place                 |

[PUBLIC_TYPE_SCOPE]: continuous distribution families

| [INDEX] | [SYMBOL]                                     | [SUPPORT]          | [CANONICAL_PARAMS]                                           |
| :-----: | :------------------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `Normal`                                     | real line          | `mu`, `sigma`/`tau`                                          |
|  [02]   | `Beta` / `Kumaraswamy`                       | (0, 1)             | `alpha`, `beta` / `a`, `b`                                   |
|  [03]   | `Gamma` / `InverseGamma`                     | positive reals     | `alpha`, `beta`                                              |
|  [04]   | `Exponential` / `Weibull` / `ChiSquared`     | positive reals     | `lam` / `alpha`, `beta` / `nu`                               |
|  [05]   | `StudentT`                                   | real line          | `nu`, `mu`, `sigma`                                          |
|  [06]   | `HalfNormal` / `HalfCauchy` / `HalfStudentT` | non-negative reals | `sigma` / `beta` / `nu`, `sigma`                             |
|  [07]   | `LogNormal` / `Wald` / `Rice`                | positive reals     | `mu`, `sigma` / `mu`, `lam` / `nu`, `sigma`                  |
|  [08]   | `Uniform` / `Triangular`                     | bounded interval   | `lower`, `upper` / `lower`, `c`, `upper`                     |
|  [09]   | `Cauchy` / `Laplace` / `AsymmetricLaplace`   | real line          | `alpha`, `beta` / `mu`, `b` / `b`, `kappa`, `mu`             |
|  [10]   | `Logistic` / `Gumbel` / `Moyal`              | real line          | `mu`, `s` / `mu`, `beta` / `mu`, `sigma`                     |
|  [11]   | `SkewNormal` / `ExGaussian` / `VonMises`     | real / circular    | `mu`, `sigma`, `alpha` / `mu`, `sigma`, `nu` / `mu`, `kappa` |
|  [12]   | `Pareto`                                     | above a minimum    | `alpha`, `m`                                                 |
|  [13]   | `Interpolated` / `PolyaGamma`                | empirical / aux    | `x_points`, `pdf_points` / `h`, `z`                          |

[PUBLIC_TYPE_SCOPE]: discrete, multivariate, mixture, and time-series families

| [INDEX] | [SYMBOL]                                             | [SUPPORT]                  | [CANONICAL_PARAMS]                     |
| :-----: | :--------------------------------------------------- | :------------------------- | :------------------------------------- |
|  [01]   | `Bernoulli` / `Binomial` / `BetaBinomial`            | {0,1} / integers           | `p` / `n, p` / `n, alpha, beta`        |
|  [02]   | `Categorical` / `DiscreteUniform`                    | finite set / integer range | `p` / `lower, upper`                   |
|  [03]   | `Poisson` / `NegativeBinomial` / `Geometric`         | non-negative integers      | `mu` / `mu, alpha` / `p`               |
|  [04]   | zero-inflated families (note [04])                   | zero-inflated counts       | `psi`, ...                             |
|  [05]   | `MvNormal` / `MvStudentT`                            | real vector space          | `mu, cov`/`chol` / `nu, mu, scale`     |
|  [06]   | `Dirichlet` / `Multinomial` / `DirichletMultinomial` | simplex / counts           | `a` / `n, p` / `n, a`                  |
|  [07]   | `LKJCholeskyCov` / `LKJCorr` / `Wishart`             | correlation / PD matrices  | `eta, n, sd_dist` / `eta, n` / `nu, V` |
|  [08]   | `MatrixNormal` / `KroneckerNormal`                   | matrix-variate Gaussian    | `mu, rowcov, colcov` / `mu, covs`      |
|  [09]   | `CAR` / `ICAR`                                       | spatial lattice            | `mu, W, alpha, tau`                    |
|  [10]   | `Mixture` / `NormalMixture`                          | mixture of components      | `w, comp_dists` / `w, mu, sigma`       |
|  [11]   | `Truncated` / `Censored`                             | restricted-support wrapper | `dist, lower, upper`                   |
|  [12]   | `CustomDist`                                         | user-defined               | `dist`/`logp`/`random`                 |
|  [13]   | `Simulator`                                          | likelihood-free            | `fn, params, distance`                 |
|  [14]   | time-series families (note [14])                     | time-series                | sequential-dependence processes        |
|  [15]   | `Flat` / `HalfFlat`                                  | improper prior             | (unnormalized) flat / half-flat        |

- [04]-[zero-inflated]: `ZeroInflatedPoisson`, `ZeroInflatedBinomial`, `ZeroInflatedNegativeBinomial`, `DiscreteWeibull`.
- [14]-[time-series]: `GaussianRandomWalk`, `RandomWalk`, `AR`, `GARCH11`, `EulerMaruyama`, `MvGaussianRandomWalk`.

[PUBLIC_TYPE_SCOPE]: step methods and samplers

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                                     |
| :-----: | :--------------------------- | :------------- | :--------------------------------------------------------------- |
|  [01]   | `NUTS`                       | gradient-based | `(vars=None, max_treedepth=10, early_max_treedepth=8, **kwargs)` |
|  [02]   | `HamiltonianMC`              | gradient-based | `(vars, path_length, max_steps)`                                 |
|  [03]   | `Metropolis`                 | gradient-free  | random-walk Metropolis                                           |
|  [04]   | `DEMetropolis`               | gradient-free  | differential-evolution Metropolis                                |
|  [05]   | `DEMetropolisZ`              | gradient-free  | DE-Z Metropolis                                                  |
|  [06]   | `BinaryMetropolis`           | gradient-free  | binary-variable Metropolis                                       |
|  [07]   | `BinaryGibbsMetropolis`      | gradient-free  | binary Gibbs Metropolis                                          |
|  [08]   | `CategoricalGibbsMetropolis` | gradient-free  | categorical Gibbs Metropolis                                     |
|  [09]   | `Slice`                      | gradient-free  | slice sampling step method                                       |
|  [10]   | `step_methods.CompoundStep`  | composite      | assigns distinct step methods per variable block                 |

[PUBLIC_TYPE_SCOPE]: variational families

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :--------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `ADVI`           | mean-field VI | automatic-differentiation VI (diagonal Gaussian)     |
|  [02]   | `FullRankADVI`   | full-rank VI  | full-rank Gaussian ADVI                              |
|  [03]   | `SVGD` / `ASVGD` | particle VI   | Stein variational gradient descent / amortized SVGD  |
|  [04]   | `Empirical`      | trace VI      | empirical approximation built from an existing trace |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: sampling and inference entrypoints

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :--------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `sample(...)`                      | MCMC           | primary MCMC entrypoint; dispatches NUTS to the chosen backend  |
|  [02]   | `sample_posterior_predictive(...)` | posterior      | posterior predictive samples from a fitted model                |
|  [03]   | `sample_prior_predictive(...)`     | prior          | prior predictive samples before conditioning                    |
|  [04]   | `sample_smc(...)`                  | SMC            | sequential Monte Carlo (IMH/MH kernel)                          |
|  [05]   | `fit(...)`                         | VI             | variational inference fit                                       |
|  [06]   | `find_MAP(...)`                    | optimization   | MAP point estimate via gradient optimization                    |
|  [07]   | `find_constrained_prior(...)`      | prior tuning   | solves prior hyperparameters so `mass` lies in `[lower, upper]` |

- [01]-[SAMPLE]: `sample(draws=1000, *, tune=None, chains=None, cores=None, random_seed=None, step=None, nuts_sampler=Literal['pymc','nutpie','numpyro','blackjax'], initvals=None, init='auto', return_inferencedata=True, idata_kwargs=None, nuts_sampler_kwargs=None, model=None, **kwargs)` -> `DataTree | MultiTrace | ZarrTrace`.
- [02]-[SAMPLE_POSTERIOR_PREDICTIVE]: `sample_posterior_predictive(trace, model=None, *, var_names=None, sample_dims=None, random_seed=None, return_inferencedata=True, extend_inferencedata=False, predictions=False)` -> `DataTree | dict`.
- [03]-[SAMPLE_PRIOR_PREDICTIVE]: `sample_prior_predictive(samples=500, model=None, *, var_names=None, random_seed=None, return_inferencedata=True)`.
- [04]-[SAMPLE_SMC]: `sample_smc(draws=2000, kernel=IMH, *, model=None, chains=None, cores=None, return_inferencedata=True, **kernel_kwargs)` -> `DataTree | MultiTrace`.
- [05]-[FIT]: `fit(n=10000, method='advi', model=None, random_seed=None, start=None, start_sigma=None, inf_kwargs=None, *, backend=None, **kwargs)` -> approximation.
- [06]-[FIND_MAP]: `find_MAP(start=None, vars=None, method='L-BFGS-B', maxeval=5000, model=None, *, seed=None)`.
- [07]-[FIND_CONSTRAINED_PRIOR]: `find_constrained_prior(distribution, lower, upper, init_guess, mass=0.95, fixed_params=None)` -> `dict[str, float]`.

[ENTRYPOINT_SCOPE]: causal surgery, diagnostics, log-probability, and graph

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]   | [CAPABILITY]                                              |
| :-----: | :----------------------------------- | :--------------- | :-------------------------------------------------------- |
|  [01]   | `do(...)`                            | causal           | do-operator: replace a node with a fixed/exogenous value  |
|  [02]   | `observe(...)`                       | causal           | converts an unobserved RV into an observed one            |
|  [03]   | `compute_log_likelihood(...)`        | diagnostics      | per-observation log-likelihood (feeds `arviz.loo`/`waic`) |
|  [04]   | `to_inference_data(...)`             | conversion       | trace -> `xarray.DataTree`                                |
|  [05]   | `predictions_to_inference_data(...)` | conversion       | attach predictions to `idata`                             |
|  [06]   | `logp(...)` / `logcdf(...)`          | log-prob         | symbolic log-density / log-CDF of a distribution          |
|  [07]   | `draw(...)`                          | sampling utility | forward-samples model variables (no conditioning)         |
|  [08]   | `model_to_graphviz(...)`             | inspection       | renders the model DAG via graphviz                        |

- [01]-[DO]: `do(model, vars_to_interventions, *, make_interventions_shared=True, prune_vars=False)` -> `Model`.
- [02]-[OBSERVE]: `observe(model, vars_to_observations)` -> `Model`.
- [03]-[COMPUTE_LOG_LIKELIHOOD]: `compute_log_likelihood(idata, *, var_names=None, extend_inferencedata=True, model=None, sample_dims=('chain','draw'))`.
- [04]-[TO_INFERENCE_DATA]: `to_inference_data(trace, model=None)`.
- [05]-[PREDICTIONS_TO_INFERENCE_DATA]: `predictions_to_inference_data(pred, idata)`.
- [06]-[LOGP]: `logp(rv, value, warn_rvs=True)` / `logcdf(rv, value, warn_rvs=True)` -> PyTensor graph.
- [07]-[DRAW]: `draw(vars, draws=1, random_seed=None)` -> `ndarray | list[ndarray]`.
- [08]-[MODEL_TO_GRAPHVIZ]: `model_to_graphviz(model=None, *, var_names=None, formatting='plain', save=None)`.

[ENTRYPOINT_SCOPE]: Gaussian-process and math submodules

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :----------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `gp.Latent`              | GP prior       | latent GP                                                              |
|  [02]   | `gp.Marginal`            | GP prior       | marginal-likelihood GP                                                 |
|  [03]   | `gp.MarginalApprox`      | GP prior       | sparse/approximate marginal GP                                         |
|  [04]   | `gp.TP`                  | GP prior       | Student-T process                                                      |
|  [05]   | `gp.HSGP`                | GP prior       | Hilbert-space reduced-rank GP                                          |
|  [06]   | `gp.LatentKron`          | GP prior       | Kronecker latent GP                                                    |
|  [07]   | `gp.MarginalKron`        | GP prior       | Kronecker marginal GP                                                  |
|  [08]   | `gp.cov.*` / `gp.mean.*` | GP kernel      | covariance (`ExpQuad`, `Matern52`, `Periodic`, ...) and mean functions |
|  [09]   | `math.*`                 | tensor ops     | PyTensor tensor primitives for `Deterministic`/`Potential` expressions |

- [09]-[MATH]: `logsumexp`, `invlogit`, `dot`, `stack`, `switch`, ... PyTensor tensor primitives.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Random variables are symbolic PyTensor nodes bound inside a `pm.Model()` context; the joint log-probability is one compiled graph, and every sampler, VI, predictive, and causal-surgery op consumes that same graph. `pm.math`/`Deterministic`/`Potential` build differentiable graph expressions PyTensor lowers to C/Numba/JAX — never NumPy-eager numerics inside the model context.

[STACKING]:
- `arviz`(`.api/arviz.md`): `sample`/`sample_posterior_predictive`/`sample_prior_predictive` return the `xarray.DataTree` arviz reads via `from_dict`/`convert_to_datatree` for `rhat`/`ess`/`loo`/`summary`/`psense`; `compute_log_likelihood` populates the `log_likelihood` group `loo`/`compare` consume.
- `nutpie`(`.api/nutpie.md`) / `numpyro`(`.api/numpyro.md`) / `blackjax`(`.api/blackjax.md`): `sample(nuts_sampler=...)` dispatches NUTS to the Rust or JAX backend by name, backend options threading through `nuts_sampler_kwargs`; the choice is a marker on the same model graph, never a re-modelled problem.
- within-lib: `do`/`observe` chain graph surgery for causal interventions; `CompoundStep` assigns a distinct step method per variable block; `Minibatch` drives stochastic VI through `fit`; `logp`/`logcdf` expose the symbolic densities `Potential` folds back into the joint.

[LOCAL_ADMISSION]:
- pymc is study-time inference reading into an `xarray.DataTree`: define a model inside `pm.Model()` with named sample sites, sample through `pm.sample`/`sample_smc`/`fit`, and graduate the returned `DataTree` through `arviz` diagnostics before any posterior claim. `Rasm.Compute` consumes that `DataTree` receipt on its C# model rail; no production runtime imports pymc.

[RAIL_LAW]:
- Package: `pymc`
- Owns: PyTensor-backed probabilistic model construction, ~60 distribution families, MCMC/SMC/VI inference with pluggable NUTS backends, causal `do`/`observe` graph surgery, and posterior/prior predictive sampling
- Accept: a model defined inside a `pm.Model()` context with named sample sites, run through `pm.sample`/`pm.sample_smc`/`pm.fit`, returning a `DataTree` receipt with captured draws, tune steps, chains, and `arviz` convergence checks
- Reject: hand-rolled distributions or samplers pymc owns; a model outside a `Model()` context; NumPy-eager numerics where a PyTensor graph is required; a posterior claim without a `DataTree` receipt
