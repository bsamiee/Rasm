# [PY_COMPUTE_API_NUMPYRO]

`numpyro` is admitted only as the `pymc` NUTS-backend string `"numpyro"`; `pm.sample(nuts_sampler="numpyro", nuts_sampler_kwargs=...)` is the sole crossing, where PyMC compiles the model to JAX, samples through numpyro's NUTS, and returns the `arviz.InferenceData` / `xarray.DataTree` the `pymc`/`arviz` catalogs own.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpyro`
- package: `numpyro`
- import: NONE — resolved at run time inside `pymc.sample`
- owner: `compute`
- rail: Bayesian-study (string backend)
- consumer: `experiments/inference.md` — the `SamplerBackend(external_nuts=(sampler, options))` case spreads to `nuts_sampler="numpyro"` + `nuts_sampler_kwargs`
- capability: JAX-native NUTS PyMC dispatches to by name; `chain_method` is the accelerator lever

## [02]-[STRING_BACKEND_CONTRACT]

[BACKEND_DISPATCH]: `pm.sample(nuts_sampler="numpyro", nuts_sampler_kwargs=...)`
- `nuts_sampler="numpyro"` is one arm of the `pymc.sampling.mcmc.sample` `nuts_sampler` Literal.
- `nuts_sampler_kwargs` forwards verbatim to `pymc.sampling.jax.sample_jax_nuts(nuts_sampler="numpyro", **kwargs)`.

| [INDEX] | [KWARG]                  | [VALUE_DOMAIN]                 | [ROLE]                                                                   |
| :-----: | :----------------------- | :----------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `chain_method`           | `"parallel"` \| `"vectorized"` | `"parallel"` = `pmap` over devices, `"vectorized"` = one `vmap`          |
|  [02]   | `postprocessing_backend` | `"cpu"` \| `"gpu"` \| `None`   | device for `_device_put` of the raw draws before the `DataTree` build    |
|  [03]   | `nuts_kwargs`            | `dict`                         | `NUTS(...)` kwargs: `target_accept_prob`, `max_tree_depth`, `dense_mass` |
|  [04]   | `idata_kwargs`           | `dict`                         | `InferenceData` build: `log_likelihood=`, `coords=`, `dims=`             |

## [03]-[DECLINE]

[SEALED_DECLINE]: PyMC owns the model DSL and the sampling entry; compute authors PyMC models, never numpyro programs, and reopening requires a live compute fence importing `numpyro` under a named consumer.
- Model authoring (`sample`/`plate`/`param`/`deterministic`/`factor` primitives, `handlers.*` effect handlers) and distribution families (`numpyro.distributions.*`, combinators, transforms, reparameterizers): the model lives in `pymc`.
- Inference beyond delegated NUTS (`HMC`/`SA`/`BarkerMH`/`AIES`/`MixedHMC`/`HMCECS`, the `MCMC`/`Predictive` API), variational inference (`SVI`/`Trace_ELBO`/`autoguide.*`/`optim.optax_to_numpyro`), and diagnostics (`diagnostics.*`): `pymc` owns the sampling entry, `arviz` owns diagnostics.

## [04]-[RAIL_LAW]

- Package: `numpyro`
- Owns (as admitted): the `"numpyro"` NUTS-backend string PyMC dispatches to, with `chain_method`/`postprocessing_backend`/`nuts_kwargs` as the accelerator-lever `nuts_sampler_kwargs`
- Accept: `pm.sample(nuts_sampler="numpyro", nuts_sampler_kwargs={"chain_method": ...})` inside a `pm.Model()` study, graduated through `az.summary`/`az.rhat` on the returned `DataTree`
- Reject: any `import numpyro` in compute; a numpyro model, distribution, handler, SVI, or diagnostic call; a posterior claim without the PyMC-returned `InferenceData`/`DataTree` receipt; catalog re-authoring of the declined surfaces
