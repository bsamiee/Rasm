# [PY_COMPUTE_API_NUMPYRO]

`numpyro` is admitted ONLY as a `pymc` NUTS-backend string: the compute Bayesian-study rail never imports it, never authors a numpyro model, and never touches its effect-handler / distribution / SVI surfaces. The sole crossing is `pm.sample(nuts_sampler="numpyro", nuts_sampler_kwargs=...)`, where PyMC's `sampling.mcmc._sample_external_nuts` delegates to `pymc.sampling.jax.sample_jax_nuts(nuts_sampler="numpyro")`, compiles the PyMC model to JAX, samples through numpyro's NUTS, and returns the `arviz.InferenceData` / `xarray.DataTree` the `pymc`/`arviz` catalogs own. Installation is transitive-optional (`python_version<'3.15'`, JAX-gated), so installed-never-imported is the admission's nature — the `SamplerBackend` tagged union carries the string, PyMC owns the JAX handoff.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `numpyro`
- package: `numpyro`
- import: NONE — never imported by compute; resolved at run time inside `pymc.sample`
- owner: `compute`
- rail: Bayesian-study (string backend)
- consumer: `experiments/inference.md` — `NutsSampler = Literal["numpyro", "blackjax", "nutpie"]`; the `SamplerBackend(external_nuts=(sampler, options))` case spreads to `nuts_sampler="numpyro"` + `nuts_sampler_kwargs`
- capability: the JAX-native NUTS backend PyMC dispatches to by name; the accelerator lever is `chain_method`

## [02]-[STRING_BACKEND_CONTRACT]

[BACKEND_DISPATCH]: `pm.sample(nuts_sampler="numpyro", nuts_sampler_kwargs=...)`
- `nuts_sampler` name: `"numpyro"` (verified live: `pymc.sampling.mcmc.sample` `nuts_sampler: Literal["pymc", "nutpie", "numpyro", "blackjax"]`).
- `nuts_sampler_kwargs` forwards verbatim to `pymc.sampling.jax.sample_jax_nuts(nuts_sampler="numpyro", **kwargs)`; the accelerator-lever keys are the only surface the study drives.

| [INDEX] | [KWARG]                  | [VALUE_DOMAIN]                 | [ROLE]                                                                   |
| :-----: | :----------------------- | :----------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `chain_method`           | `"parallel"` \| `"vectorized"` | `"parallel"` = `pmap` over devices, `"vectorized"` = one `vmap`          |
|  [02]   | `postprocessing_backend` | `"cpu"` \| `"gpu"` \| `None`   | device for `_device_put` of the raw draws before the `DataTree` build    |
|  [03]   | `nuts_kwargs`            | `dict`                         | `NUTS(...)` kwargs: `target_accept_prob`, `max_tree_depth`, `dense_mass` |
|  [04]   | `idata_kwargs`           | `dict`                         | `InferenceData` build: `log_likelihood=`, `coords=`, `dims=`             |

## [03]-[DECLINE]

[SEALED_DECLINE]: numpyro's authored surfaces are CLOSED — a sealed decline, never re-opened by catalog authorship.
- Model authoring (`sample`/`plate`/`param`/`deterministic`/`factor` primitive sites, `handlers.*` effect handlers): PyMC owns the model DSL; compute authors PyMC models, not numpyro programs.
- Distribution families (`numpyro.distributions.*`), combinators, transforms, and reparameterizers: unused — the model lives in `pymc`.
- Inference engines beyond delegated NUTS (`HMC`/`SA`/`BarkerMH`/`AIES`/`MixedHMC`/`HMCECS`, the whole `MCMC`/`Predictive` API), variational inference (`SVI`/`Trace_ELBO`/`autoguide.*`/`optim.optax_to_numpyro`), and diagnostics (`diagnostics.*`): not surfaced — `arviz` owns diagnostics, `pymc` owns sampling entry, and a direct numpyro run is not a compute path.
- Reopening requires a live compute fence importing `numpyro` under a named consumer, never catalog preference.

## [04]-[RAIL_LAW]

[RAIL_LAW]:
- Package: `numpyro`
- Owns (as admitted): the `"numpyro"` NUTS-backend string PyMC dispatches to, with `chain_method`/`postprocessing_backend`/`nuts_kwargs` as the accelerator-lever `nuts_sampler_kwargs`
- Accept: `pm.sample(nuts_sampler="numpyro", nuts_sampler_kwargs={"chain_method": ...})` inside a `pm.Model()` study, graduated through `az.summary`/`az.rhat` on the returned `DataTree`
- Reject: any `import numpyro` in compute; a numpyro model, distribution, handler, SVI, or diagnostic call; a posterior claim without the PyMC-returned `InferenceData`/`DataTree` receipt; catalog re-authoring of the declined surfaces
