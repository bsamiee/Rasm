# [PY_COMPUTE_API_NUTPIE]

`nutpie` is admitted ONLY as a `pymc` NUTS-backend string: the compute Bayesian-study rail never imports it, never drives its Rust-native compile/sample/Zarr-store surfaces. `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs=...)` is the sole crossing — PyMC compiles and samples through nutpie, returning the `arviz.InferenceData` / `xarray.DataTree` the `pymc`/`arviz` catalogs own. Installed-never-imported is the admission's nature: the `SamplerBackend` union carries the string, and PyMC owns the handoff.

## [01]-[STRING_BACKEND_CONTRACT]

[BACKEND_DISPATCH]: `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs=...)`
- `nuts_sampler` name `"nutpie"` — `pymc.sampling.mcmc.sample` carries `nuts_sampler: Literal["pymc", "nutpie", "numpyro", "blackjax"]`.
- `nuts_sampler_kwargs` splits inside `_sample_external_nuts`: `backend`/`gradient_backend` pop into the `nutpie.compile_pymc_model` `compile_kwargs`; every remaining key forwards to `nutpie.sample`.

| [INDEX] | [KWARG]                         | [VALUE_DOMAIN]          | [TARGET]             | [ROLE]                                               |
| :-----: | :------------------------------ | :---------------------- | :------------------- | :--------------------------------------------------- |
|  [01]   | `backend`                       | `'numba'` \| `'jax'`    | `compile_pymc_model` | log-density + gradient lowering; accelerator lever   |
|  [02]   | `gradient_backend`              | `'pytensor'` \| `'jax'` | `compile_pymc_model` | gradient autodiff (`'jax'` pairs `backend='jax'`)    |
|  [03]   | `init_mean`                     | `ndarray`               | `nutpie.sample`      | warmup mean seed (`initvals` not forwarded)          |
|  [04]   | `low_rank_modified_mass_matrix` | `bool`                  | `nutpie.sample`      | low-rank mass-matrix adaptation, high-dim posteriors |

## [02]-[DECLINE]

[SEALED_DECLINE]: PyMC owns the compile-and-sample motion; compute never drives a bare nutpie surface, and reopening requires a live compute fence importing `nutpie` under a named consumer.
- Direct compile/sample entry (`compile_pymc_model`/`compile_stan_model`/`compiled_pyfunc.from_pyfunc`, direct `nutpie.sample`): `benchmark_logp`, `with_data`/`with_coords` runtime swaps, and `_BackgroundSampler` async control stay unused.
- Stan path (`compile_stan_model`, `prune_stan_cache`): the study models are PyMC, never Stan.
- Zarr streaming stores (`zarr_store.*`, `store_unconstrained=`): compute owns no durable run store; resumable traces are the C# Persistence ledger's.

## [03]-[IMPLEMENTATION_LAW]
