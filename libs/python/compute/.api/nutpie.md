# [PY_COMPUTE_API_NUTPIE]

`nutpie` supplies a Rust-native NUTS sampler that compiles PyMC and Stan models to native code and samples them through a high-performance NUTS implementation, returning an `xarray.DataTree` posterior. The package owner compiles a PyMC `Model` via `compile_pymc_model` or a Stan program via `compile_stan_model`, then calls `sample`; it never re-implements a sampler or model compiler the package owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nutpie`
- package: `nutpie`
- import: `nutpie`; submodules `nutpie.compile_pymc`, `nutpie.compile_stan`, `nutpie.compiled_pyfunc`, `nutpie.zarr_store`
- owner: `compute`
- rail: Bayesian-study
- installed: cp313 only (manifest pin `nutpie; python_version<'3.15'`; gated by the PyMC cp315 floor)
- capability: Rust-native NUTS sampling of PyMC and Stan models — Numba or JAX backend for PyMC, CmdStan for Stan, diagonal/low-rank/normalizing-flow mass adaptation, an MCLMC sampler, and Zarr-backed trace streaming

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compiled-model types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]      | [CAPABILITY]                                                                     |
| :-----: | :------------------------------- | :------------------ | :------------------------------------------------------------------------------- |
|   [1]   | `compile_pymc.CompiledPyMCModel` | compiled PyMC model | compiled PyMC model extending `CompiledModel`                                    |
|   [2]   | `compile_stan.CompiledStanModel` | compiled Stan model | compiled Stan model extending `CompiledModel`                                    |
|   [3]   | `compiled_pyfunc.CompiledModel`  | base compiled type  | base with `n_dim`, `coords`, `shapes`, `reparameterized_names`, `benchmark_logp` |
|   [4]   | `compiled_pyfunc.PyFuncModel`    | pyfunc model type   | model compiled from a Python log-density callable                                |

[PUBLIC_TYPE_SCOPE]: progress and Zarr store types
- rail: Bayesian-study

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]  | [CAPABILITY]                                                                                                                                                          |
| :-----: | :----------------------- | :-------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `ChainProgress`          | progress record | `divergences`, `divergent_draws`, `finished_draws`, `latest_num_steps`, `num_steps`, `total_num_steps`, `runtime_ms`, `started`, `step_size`, `total_draws`, `tuning` |
|   [2]   | `zarr_store.LocalStore`  | local store     | write trace to a local filesystem Zarr group                                                                                                                          |
|   [3]   | `zarr_store.S3Store`     | cloud store     | stream trace to an S3-compatible Zarr group                                                                                                                           |
|   [4]   | `zarr_store.GCSStore`    | cloud store     | stream trace to a Google Cloud Storage Zarr group                                                                                                                     |
|   [5]   | `zarr_store.AzureStore`  | cloud store     | stream trace to an Azure Blob Zarr group                                                                                                                              |
|   [6]   | `zarr_store.HTTPStore`   | remote store    | read or write a Zarr group over HTTP                                                                                                                                  |
|   [7]   | `zarr_store.MemoryStore` | in-memory store | ephemeral in-memory Zarr group                                                                                                                                        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compile, sample, and cache operations
- rail: Bayesian-study

| [INDEX] | [SURFACE]                      | [ENTRY_FAMILY]   | [CAPABILITY]                                                                                                                                                                                                                                                                            |
| :-----: | :----------------------------- | :--------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `compile_pymc_model`           | PyMC compiler    | `(model, *, backend='numba', gradient_backend='pytensor', initial_points, jitter_rvs, default_initialization_strategy='support_point', var_names, freeze_model, **kwargs) -> CompiledModel`                                                                                             |
|   [2]   | `compile_stan_model`           | Stan compiler    | `(*, code, filename, extra_compile_args, extra_stanc_args, dims, coords, model_name, cleanup=True, cache=False, prune_cache=True) -> CompiledStanModel`                                                                                                                                 |
|   [3]   | `sample`                       | NUTS runner      | `(compiled_model, *, draws, tune, chains, cores, seed, save_warmup=True, progress_bar=True, sampler='nuts', adaptation='diag', init_mean, return_raw_trace=False, blocking=True, progress_rate=100, zarr_store, store_unconstrained=False, **kwargs) -> DataTree \| _BackgroundSampler` |
|   [4]   | `zarr_store.from_url`          | store factory    | constructs the matching Zarr store from a URL scheme                                                                                                                                                                                                                                    |
|   [5]   | `prune_stan_cache`             | cache management | `(max_entries=16, min_age=timedelta(days=14)) -> None`                                                                                                                                                                                                                                  |
|   [6]   | `CompiledModel.benchmark_logp` | logp bench       | times log-density and gradient evaluations on the compiled model                                                                                                                                                                                                                        |

## [4]-[IMPLEMENTATION_LAW]

[SAMPLING_TOPOLOGY]:
- `compile_pymc_model` lowers a PyMC `Model` to native code through the `numba` or `jax` backend; `compile_stan_model` invokes CmdStan and returns a `CompiledStanModel`.
- `sample` returns an `xarray.DataTree` by default; `blocking=False` returns a `_BackgroundSampler` whose chains report through `ChainProgress`.
- `adaptation` selects the mass-matrix scheme: `'diag'`, `'draw_diag'`, `'low_rank'`, or `'flow'` (normalizing-flow); `sampler` selects `'nuts'` or `'mclmc'`.
- `zarr_store` plus `store_unconstrained=True` streams draws to a Zarr group during sampling for resumable, out-of-core traces.
- Stan compilation requires `cmdstanpy` and a CmdStan toolchain; the Stan cache is pruned by `prune_stan_cache`.

[STUDY_ROUTING]:
- A study compiles a PyMC or Stan model once, then runs `sample` with explicit `draws`, `tune`, `chains`, and `seed`; the `DataTree` plus per-chain `ChainProgress` is the study receipt.
- `benchmark_logp` supplies log-density timing as accelerator evidence comparing the Numba and JAX PyMC backends.
- No production runtime imports nutpie; sampling is offline Bayesian-study work feeding the C# `Rasm.Compute` rail.

[RAIL_LAW]:
- Package: `nutpie`
- Owns: Rust-native NUTS sampling of PyMC and Stan models with adaptive mass-matrix tuning and Zarr-backed streaming
- Accept: a `compile_pymc_model(pm.Model(...))` or `compile_stan_model(code=...)` result passed to `sample`, with a `DataTree` posterior receipt capturing `draws`, `tune`, `chains`, and `ChainProgress` diagnostics
- Reject: hand-rolled NUTS implementations; nutpie in any product runtime path; posterior claims without a `DataTree` receipt; Stan compilation without `cmdstanpy` installed
