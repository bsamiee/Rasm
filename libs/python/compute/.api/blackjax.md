# [PY_COMPUTE_API_BLACKJAX]

`blackjax` is admitted ONLY as a `pymc` NUTS-backend string: the compute Bayesian-study rail never imports it, never touches its stateless MCMC/SMC/SG-MCMC/VI kernel algebra. `pm.sample(nuts_sampler="blackjax", nuts_sampler_kwargs=...)` is the sole crossing — PyMC compiles the model to JAX, drives window-adapted NUTS, and returns the `arviz.InferenceData` / `xarray.DataTree` the `pymc`/`arviz` catalogs own. Installed-never-imported is the admission's nature: transitive-optional install, the `SamplerBackend` union carries the string, PyMC owns the JAX handoff.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `blackjax`
- package: `blackjax`
- import: NONE — never imported by compute; resolved at run time inside `pymc.sample`
- owner: `compute`
- rail: Bayesian-study (string backend)
- consumer: `experiments/inference.md` — `NutsSampler = Literal["numpyro", "blackjax", "nutpie"]`; the `SamplerBackend(external_nuts=(sampler, options))` case spreads to `nuts_sampler="blackjax"` + `nuts_sampler_kwargs`
- capability: the JAX-native NUTS backend PyMC dispatches to by name; the accelerator lever is `chain_method`

## [02]-[STRING_BACKEND_CONTRACT]

[BACKEND_DISPATCH]: `pm.sample(nuts_sampler="blackjax", nuts_sampler_kwargs=...)`
- `nuts_sampler` name `"blackjax"` — `pymc.sampling.mcmc.sample` carries `nuts_sampler: Literal["pymc", "nutpie", "numpyro", "blackjax"]`.
- `nuts_sampler_kwargs` forwards verbatim to `pymc.sampling.jax.sample_jax_nuts(nuts_sampler="blackjax", **kwargs)` — the shared JAX entry, so blackjax's admitted levers equal numpyro's; PyMC's blackjax path is window-adapted NUTS only.

| [INDEX] | [KWARG]                  | [VALUE_DOMAIN]                 | [ROLE]                                                                |
| :-----: | :----------------------- | :----------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `chain_method`           | `"parallel"` \| `"vectorized"` | chain-to-device mapping — `pmap` across host devices vs one `vmap`    |
|  [02]   | `postprocessing_backend` | `"cpu"` \| `"gpu"` \| `None`   | device for `_device_put` of the raw draws before the `DataTree` build |
|  [03]   | `idata_kwargs`           | `dict`                         | `InferenceData` build kwargs — `log_likelihood=`, `coords=`, `dims=`  |

## [03]-[DECLINE]

[SEALED_DECLINE]: PyMC owns the model→sampler compile; compute never drives a bare blackjax kernel, and catalog authorship never re-opens a declined surface.
- Kernel builders (`hmc`/`nuts`/`mala`/`ghmc`/`barker`/`rmhmc`/`mclmc`/`rmh` + the `SamplingAlgorithm(init, step)` `lax.scan` loop), SMC (`adaptive_tempered_smc`/`tempered_smc`/`partial_posteriors_smc`/`persistent_sampling`, `smc.resampling.*`), SG-MCMC (`sgld`/`sghmc`/`sgnht`/`csgld`), VI (`meanfield_vi`/`fullrank_vi`/`svgd`/`pathfinder`/`multipathfinder`): the study rail is NUTS-via-PyMC, not a bare kernel, tempering ladder, or VI/SG-MCMC algorithm.
- Adaptation, diagnostics, utility (`window_adaptation`/`chees_adaptation`/`meads_adaptation`, `diagnostics.*`, `util.run_inference_algorithm`/`psis_weights`): PyMC's blackjax path applies its own window adaptation, `arviz` owns diagnostics.
- Reopening requires a live compute fence importing `blackjax` under a named consumer, never catalog preference.

## [04]-[RAIL_LAW]

- Package: `blackjax`
- Owns (as admitted): the `"blackjax"` NUTS-backend string PyMC dispatches to, with `chain_method`/`postprocessing_backend` as the accelerator-lever `nuts_sampler_kwargs`
- Accept: `pm.sample(nuts_sampler="blackjax", nuts_sampler_kwargs={"chain_method": ...})` inside a `pm.Model()` study, graduated through `az.summary`/`az.rhat` on the returned `DataTree`
- Reject: any `import blackjax` in compute; a raw kernel, SMC, SG-MCMC, VI, adaptation, or diagnostic call; a posterior claim without the PyMC-returned `InferenceData`/`DataTree` receipt; catalog re-authoring of the declined surfaces
