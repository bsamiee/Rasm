# [PY_COMPUTE_API_NUTPIE]

`nutpie` supplies a Rust-native NUTS sampler that compiles PyMC and Stan models to native code and samples them through a high-performance NUTS implementation, returning an `arviz.InferenceData` / `xarray.DataTree` posterior. The package owner compiles a PyMC `Model` via `compile_pymc_model` or a Stan program via `compile_stan_model`, then calls `sample`; it never re-implements a sampler or model compiler the package owns. nutpie is the fast NUTS backend behind PyMC: the dominant integration is `pm.sample(nuts_sampler="nutpie")`, where PyMC's `sampling.mcmc` delegates compilation and sampling to nutpie and returns the same `xarray.DataTree` the `pymc` rail documents. Its posterior is directly `arviz`-shaped (`InferenceData`/`DataTree`), so `arviz.from_datatree`/`az.summary` consume it with no conversion, and the Numba-vs-JAX backend choice is the accelerator-evidence lever the study receipt records via `benchmark_logp`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nutpie`
- package: `nutpie`
- import: `nutpie`; submodules `nutpie.compile_pymc`, `nutpie.compile_stan`, `nutpie.compiled_pyfunc`, `nutpie.zarr_store`
- owner: `compute`
- rail: Bayesian-study
- capability: Rust-native NUTS sampling of PyMC and Stan models — Numba or JAX backend for PyMC, CmdStan for Stan, diagonal/low-rank/normalizing-flow mass adaptation, an MCLMC sampler, and Zarr-backed trace streaming

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiled-model types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                                    |
| :-----: | :------------------------------- | :------------------ | :------------------------------------------------------------------------------------------------------------- |
|  [01]   | `compile_pymc.CompiledPyMCModel` | compiled PyMC model | compiled PyMC model extending `CompiledModel`; carries the lowered `numba`/`jax` log-density + gradient        |
|  [02]   | `compile_stan.CompiledStanModel` | compiled Stan model | compiled Stan model extending `CompiledModel`; wraps the CmdStan-compiled binary                               |
|  [03]   | `compiled_pyfunc.CompiledModel`  | base compiled type  | frozen dataclass base with `n_dim`, `coords`, `dims`, `shapes`, `shape_info`, `reparameterized_names`          |
|  [04]   | `compiled_pyfunc.PyFuncModel`    | pyfunc model type   | model compiled from a Python log-density callable (`from_pyfunc`)                                              |
|  [05]   | `CompiledModel.with_data(**data)` | runtime data swap   | returns a new compiled model with mutable-data values replaced — re-sample posterior-predictive without recompiling |
|  [06]   | `CompiledModel.with_coords(**coords)` | runtime coord swap | returns a new compiled model with dim coordinates replaced (the dims propagate into the `DataTree`)            |
|  [07]   | `_BackgroundSampler` (from `sample(blocking=False)`) | async sampler | `.wait(timeout)`, `.pause()`/`.resume()`, `.abort()`, `.cancel()`, `.is_finished`, `.inspect()` (live partial `DataTree`); iterate `ChainProgress` while sampling |

[PUBLIC_TYPE_SCOPE]: progress and Zarr store types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]  | [CAPABILITY]                                                                                                                                                          |
| :-----: | :----------------------- | :-------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ChainProgress`          | progress record | `divergences`, `divergent_draws`, `finished_draws`, `latest_num_steps`, `num_steps`, `total_num_steps`, `runtime_ms`, `started`, `step_size`, `total_draws`, `tuning` |
|  [02]   | `zarr_store.LocalStore`  | local store     | write trace to a local filesystem Zarr group                                                                                                                          |
|  [03]   | `zarr_store.S3Store`     | cloud store     | stream trace to an S3-compatible Zarr group                                                                                                                           |
|  [04]   | `zarr_store.GCSStore`    | cloud store     | stream trace to a Google Cloud Storage Zarr group                                                                                                                     |
|  [05]   | `zarr_store.AzureStore`  | cloud store     | stream trace to an Azure Blob Zarr group                                                                                                                              |
|  [06]   | `zarr_store.HTTPStore`   | remote store    | read or write a Zarr group over HTTP                                                                                                                                  |
|  [07]   | `zarr_store.MemoryStore` | in-memory store | ephemeral in-memory Zarr group                                                                                                                                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compile, sample, and cache operations
- rail: Bayesian-study

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY]   | [CAPABILITY]                                                                                                                                                                                                                                                                            |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `compile_pymc_model`           | PyMC compiler    | `(model, *, backend='numba', gradient_backend='pytensor', initial_points, jitter_rvs, default_initialization_strategy='support_point', var_names, freeze_model, **kwargs) -> CompiledPyMCModel`; `backend='jax'` lowers the log-density through JAX, `gradient_backend='jax'` uses JAX autodiff |
|  [02]   | `compile_stan_model`           | Stan compiler    | `(*, code, filename, extra_compile_args, extra_stanc_args, dims, coords, model_name, cleanup=True, cache=False, prune_cache=True) -> CompiledStanModel`; `.with_data(**stan_data)` binds Stan data before sampling                                                                       |
|  [03]   | `compiled_pyfunc.from_pyfunc`  | pyfunc compiler  | `(ndim, make_logp_fn, make_expand_fn, expanded_shapes, expanded_names, expanded_dtypes, *, initial_mean=None, coords=None, dims=None) -> PyFuncModel` — wrap a raw JAX/Numba log-density without PyMC or Stan                                                                            |
|  [04]   | `sample`                       | NUTS runner      | `(compiled_model, *, draws=1000, tune=300, chains=6, cores, seed, save_warmup=True, progress_bar=True, low_rank_modified_mass_matrix=False, init_mean, sampler='nuts', adaptation='diag', return_raw_trace=False, blocking=True, progress_rate=100, progress_template, zarr_store, store_unconstrained=False, **kwargs) -> InferenceData \| _BackgroundSampler` |
|  [05]   | `zarr_store.from_url`          | store factory    | constructs the matching Zarr store (`LocalStore`/`S3Store`/`GCSStore`/`AzureStore`/`HTTPStore`/`MemoryStore`) from a URL scheme                                                                                                                                                          |
|  [06]   | `prune_stan_cache`             | cache management | `(max_entries=16, min_age=timedelta(days=14)) -> None`                                                                                                                                                                                                                                  |
|  [07]   | `CompiledModel.benchmark_logp` | logp bench       | `(point=None, n_evals=1000, gradient=True)` times log-density and gradient evaluations — the Numba-vs-JAX accelerator-evidence lever                                                                                                                                                     |

## [04]-[IMPLEMENTATION_LAW]

[SAMPLING_TOPOLOGY]:
- `compile_pymc_model` lowers a PyMC `Model` to native code through the `numba` or `jax` backend; `compile_stan_model` invokes CmdStan and returns a `CompiledStanModel`.
- `sample` returns an `arviz.InferenceData` (the `xarray.DataTree` posterior) by default — directly consumable by `az.summary`/`az.rhat`/`az.loo` with no conversion step; `return_raw_trace=True` returns the raw draw arrays, and `blocking=False` returns a `_BackgroundSampler` whose chains report through `ChainProgress` and whose `.inspect()` yields a live partial `DataTree`.
- `adaptation` selects the mass-matrix scheme: `'diag'`, `'draw_diag'`, `'low_rank'`, or `'flow'` (normalizing-flow); `sampler` selects `'nuts'` or `'mclmc'` (microcanonical Langevin Monte Carlo).
- `zarr_store` plus `store_unconstrained=True` streams draws to a Zarr group during sampling for resumable, out-of-core traces; `zarr_store.from_url(url)` selects the cloud backend by URL scheme.
- Stan compilation requires `cmdstanpy` and a CmdStan toolchain; the Stan cache is pruned by `prune_stan_cache`.

[STACKING_TOPOLOGY]:
- nutpie ↔ `pymc`: the canonical seam is `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs={'backend':'jax'})` inside a `pm.Model()` context — PyMC compiles via `compile_pymc_model` and samples via nutpie, returning the `pymc`-catalog `DataTree`. The explicit form `nutpie.compile_pymc_model(model)` → `nutpie.sample(...)` is used when the study needs `benchmark_logp` or `with_data` runtime swaps PyMC's wrapper hides.
- nutpie → `arviz`: `sample` returns `InferenceData` directly, so the study graduates with `az.summary(idata)`/`az.rhat`/`az.loo` and no converter — contrast numpyro/Stan which need `az.from_numpyro`/`az.from_cmdstanpy`. Streaming traces written to a `zarr_store` are read back with `az.from_zarr`.
- nutpie backend lever: `backend='numba'` (default, PyTensor-lowered) versus `backend='jax'`/`gradient_backend='jax'` is a per-study accelerator choice; `benchmark_logp(gradient=True)` quantifies the per-evaluation cost so the receipt records which backend the posterior used.

[STUDY_ROUTING]:
- A study compiles a PyMC or Stan model once, then runs `sample` with explicit `draws`, `tune`, `chains`, and `seed`; the `InferenceData`/`DataTree` plus per-chain `ChainProgress` (`divergences`, `step_size`, `num_steps`) is the study receipt.
- `with_data`/`with_coords` re-bind mutable data on the already-compiled model for posterior-predictive or cross-validation folds without paying recompilation cost.
- `benchmark_logp` supplies log-density timing as accelerator evidence comparing the Numba and JAX PyMC backends.
- No production runtime imports nutpie; sampling is offline Bayesian-study work feeding the C# `Rasm.Compute` rail.

[RAIL_LAW]:
- Package: `nutpie`
- Owns: Rust-native NUTS sampling of PyMC and Stan models with adaptive mass-matrix tuning and Zarr-backed streaming
- Accept: a `compile_pymc_model(pm.Model(...))`/`compile_stan_model(code=...)` result passed to `sample`, or `pm.sample(nuts_sampler="nutpie")`, graduated through `az.summary`/`az.rhat` into a `DataTree` receipt capturing `draws`, `tune`, `chains`, the chosen backend, and `ChainProgress` diagnostics
- Reject: hand-rolled NUTS implementations; nutpie in any product runtime path; posterior claims without an `InferenceData`/`DataTree` receipt; Stan compilation without `cmdstanpy` installed; recompiling per posterior-predictive fold instead of `with_data`
