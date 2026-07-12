# [PY_COMPUTE_API_NUTPIE]

`nutpie` is admitted ONLY as a `pymc` NUTS-backend string: the compute Bayesian-study rail never imports it and never drives its `compile_pymc_model`/`sample`/Zarr-store surfaces directly. The sole crossing is `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs=...)`, where PyMC's `sampling.mcmc._sample_external_nuts` pops the compile levers, calls `nutpie.compile_pymc_model(model, **compile_kwargs)` then `nutpie.sample(...)`, and returns the `arviz.InferenceData` / `xarray.DataTree` the `pymc`/`arviz` catalogs own. Unlike the JAX backends, nutpie is installed unconditionally (`0.16.10`, no `python_version` gate) — but still installed-never-imported: the `SamplerBackend` tagged union carries the string, PyMC owns the compile-and-sample handoff, and the Numba-vs-JAX `backend` lever is the study's accelerator choice.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nutpie`
- package: `nutpie`
- import: NONE — never imported by compute; resolved at run time inside `pymc.sample`
- owner: `compute`
- rail: Bayesian-study (string backend)
- installed: `0.16.10`
- consumer: `experiments/inference.md` — `NutsSampler = Literal["numpyro", "blackjax", "nutpie"]`; the `SamplerBackend(external_nuts=(sampler, options))` case spreads to `nuts_sampler="nutpie"` + `nuts_sampler_kwargs`
- capability: the Rust-native NUTS backend PyMC dispatches to by name; the accelerator lever is the per-engine `backend` (`'numba'`/`'jax'`)

## [02]-[STRING_BACKEND_CONTRACT]

[BACKEND_DISPATCH]: `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs=...)`
- `nuts_sampler` name: `"nutpie"` (verified live: `pymc.sampling.mcmc.sample` `nuts_sampler: Literal["pymc", "nutpie", "numpyro", "blackjax"]`).
- `nuts_sampler_kwargs` splits inside `_sample_external_nuts`: the `backend`/`gradient_backend` keys are POPPED into `nutpie.compile_pymc_model` `compile_kwargs`; every remaining key forwards to `nutpie.sample(...)` (verified live: `pymc.sampling.mcmc._sample_external_nuts`, the `for kwarg in ("backend", "gradient_backend")` pop).

| [INDEX] | [KWARG]                         | [VALUE_DOMAIN]          | [TARGET]             | [ROLE]                                                                         |
| :-----: | :------------------------------ | :---------------------- | :------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `backend`                       | `'numba'` \| `'jax'`    | `compile_pymc_model` | native-lowering backend for the log-density + gradient — the accelerator lever |
|  [02]   | `gradient_backend`              | `'pytensor'` \| `'jax'` | `compile_pymc_model` | autodiff source for the gradient (`'jax'` pairs with `backend='jax'`)          |
|  [03]   | `init_mean`                     | `ndarray`               | `nutpie.sample`      | warmup mean seed (PyMC `initvals` are NOT forwarded to nutpie — use this)      |
|  [04]   | `low_rank_modified_mass_matrix` | `bool`                  | `nutpie.sample`      | low-rank mass-matrix adaptation for high-dimensional posteriors                |

## [03]-[DECLINE]

[SEALED_DECLINE]: nutpie's authored compile/sample/store surfaces are CLOSED — a sealed decline, never re-opened by catalog authorship.
- Explicit compile/sample entry (`compile_pymc_model`/`compile_stan_model`/`compiled_pyfunc.from_pyfunc`, direct `nutpie.sample`): PyMC owns the compile-and-sample motion; compute never calls nutpie directly, so `benchmark_logp`, `with_data`/`with_coords` runtime swaps, and `_BackgroundSampler` async control are unused.
- Stan path (`compile_stan_model`, `prune_stan_cache`, `cmdstanpy`/CmdStan toolchain): out of scope — the study models are PyMC, never Stan.
- Zarr streaming stores (`zarr_store.{LocalStore,S3Store,GCSStore,AzureStore,HTTPStore,MemoryStore}`, `store_unconstrained=`): unused — compute owns no durable run store; resumable traces are the C# Persistence reuse ledger's, not nutpie's.
- Reopening requires a live compute fence importing `nutpie` under a named consumer (e.g. a `benchmark_logp` accelerator-evidence probe), never catalog preference.

## [04]-[RAIL_LAW]

[RAIL_LAW]:
- Package: `nutpie`
- Owns (as admitted): the `"nutpie"` NUTS-backend string PyMC dispatches to, with `backend`/`gradient_backend` compile levers and `init_mean`/`low_rank_modified_mass_matrix` sample levers as the `nuts_sampler_kwargs`
- Accept: `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs={"backend": "jax"})` inside a `pm.Model()` study, graduated through `az.summary`/`az.rhat` on the returned `DataTree`
- Reject: any `import nutpie` in compute; a direct compile/sample/benchmark/Zarr-store call; a Stan model; a posterior claim without the PyMC-returned `InferenceData`/`DataTree` receipt; catalog re-authoring of the declined surfaces
