# [PY_COMPUTE_API_NUTPIE]

`nutpie` is admitted ONLY as a `pymc` NUTS-backend string: the compute Bayesian-study rail never imports it, never drives its Rust-native compile/sample/Zarr-store surfaces. `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs=...)` is the sole crossing — PyMC compiles and samples through nutpie, returning the `arviz.InferenceData` / `xarray.DataTree` the `pymc`/`arviz` catalogs own. Installed-never-imported is the admission's nature: the `SamplerBackend` union carries the string, and PyMC owns the handoff.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nutpie`
- package: `nutpie`
- import: NONE — never imported by compute; resolved at run time inside `pymc.sample`
- owner: `compute`
- rail: Bayesian-study (string backend)
- consumer: `experiments/inference.md` — the `SamplerBackend(external_nuts=(sampler, options))` case spreads to `nuts_sampler="nutpie"` + `nuts_sampler_kwargs`
- capability: Rust-native NUTS PyMC dispatches to by name; the `backend` lever (`'numba'`/`'jax'`) selects the accelerator

## [02]-[STRING_BACKEND_CONTRACT]

[BACKEND_DISPATCH]: `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs=...)`
- `nuts_sampler` name `"nutpie"` — `pymc.sampling.mcmc.sample` carries `nuts_sampler: Literal["pymc", "nutpie", "numpyro", "blackjax"]`.
- `nuts_sampler_kwargs` splits inside `_sample_external_nuts`: `backend`/`gradient_backend` pop into the `nutpie.compile_pymc_model` `compile_kwargs`; every remaining key forwards to `nutpie.sample`.

| [INDEX] | [KWARG]                         | [VALUE_DOMAIN]          | [TARGET]             | [ROLE]                                               |
| :-----: | :------------------------------ | :---------------------- | :------------------- | :--------------------------------------------------- |
|  [01]   | `backend`                       | `'numba'` \| `'jax'`    | `compile_pymc_model` | log-density + gradient lowering; accelerator lever   |
|  [02]   | `gradient_backend`              | `'pytensor'` \| `'jax'` | `compile_pymc_model` | gradient autodiff (`'jax'` pairs `backend='jax'`)    |
|  [03]   | `init_mean`                     | `ndarray`               | `nutpie.sample`      | warmup mean seed (`initvals` not forwarded)          |
|  [04]   | `low_rank_modified_mass_matrix` | `bool`                  | `nutpie.sample`      | low-rank mass-matrix adaptation, high-dim posteriors |

## [03]-[DECLINE]

[SEALED_DECLINE]: PyMC owns the compile-and-sample motion; compute never drives a bare nutpie surface, and reopening requires a live compute fence importing `nutpie` under a named consumer.
- Direct compile/sample entry (`compile_pymc_model`/`compile_stan_model`/`compiled_pyfunc.from_pyfunc`, direct `nutpie.sample`): `benchmark_logp`, `with_data`/`with_coords` runtime swaps, and `_BackgroundSampler` async control stay unused.
- Stan path (`compile_stan_model`, `prune_stan_cache`): the study models are PyMC, never Stan.
- Zarr streaming stores (`zarr_store.*`, `store_unconstrained=`): compute owns no durable run store; resumable traces are the C# Persistence ledger's.

## [04]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `nutpie`
- Owns (as admitted): the `"nutpie"` NUTS-backend string PyMC dispatches to, with the `backend`/`gradient_backend` compile levers and `init_mean`/`low_rank_modified_mass_matrix` sample levers as `nuts_sampler_kwargs`
- Accept: `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs={"backend": "jax"})` inside a `pm.Model()` study, graduated through `az.summary`/`az.rhat` on the returned `DataTree`
- Reject: any `import nutpie` in compute; a direct compile/sample/benchmark/Zarr-store call; a Stan model; a posterior claim without the PyMC-returned `InferenceData`/`DataTree` receipt; catalog re-authoring of the declined surfaces
