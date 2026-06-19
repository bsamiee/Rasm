# [PY_COMPUTE_INFERENCE]

The one classical Bayesian-inference owner over an explicit prior/likelihood/posterior graph. `Inference` builds a `pymc.Model` from a frozen request, draws the posterior with gradient MCMC, scores convergence with arviz, and returns a typed posterior-evidence receipt. The `SamplerPlan` carries a backend discriminant so the one owner selects the MCMC engine by case: PyMC-native NUTS or Metropolis, NumPyro JAX NUTS, Nutpie Rust/Numba NUTS, or BlackJAX JAX NUTS, with arviz as the cross-backend rhat-and-ess diagnostic owner. The owner is bounded at conjugate, hierarchical, and GLM-class models; variational, normalizing-flow, and neural-posterior estimation never enter it. A posterior failing the convergence bar is an admission rejection, never a graduated handoff.

## [01]-[INDEX]

- [01]-[BAYESIAN]: the prior/likelihood/posterior graph, the sampler-backend axis, and arviz diagnostics on one `Inference` owner.

## [02]-[BAYESIAN]

- Owner: `Inference` — the one Bayesian owner over a prior/likelihood/posterior axis. `InferenceSpec` is the frozen request carrying the observed array, the `LatentPrior` family per latent, the `Likelihood` case, the mean latent, and the `SamplerPlan`; `Inference.run` builds the `pymc.Model`, draws the posterior, scores it with arviz diagnostics, and returns an `InferenceReceipt`. Adding a distribution is a `Prior`/`Likelihood` case; adding an engine is a `SamplerBackend` case; the diagnostics are receipt fields.
- Sampler-backend axis: `SamplerBackend` discriminates the MCMC engine — `pymc_native` runs `pymc.sample` with a `pymc.NUTS` or `pymc.Metropolis` step, `numpyro_jax` runs `pymc.sampling.jax.sample_numpyro_nuts` (the NumPyro JAX NUTS path), `nutpie_numba` runs `nutpie.sample` over the compiled PyMC model (the Rust/Numba NUTS path), and `blackjax_jax` runs `pymc.sampling.jax.sample_blackjax_nuts` (the BlackJAX JAX NUTS path). `SamplerKind` selects the step method for the native backend. arviz reads the posterior and the sample-stats group regardless of which engine sampled, so the diagnostic gate is one fold across the four backends; the two JAX backends and Nutpie ride one `pymc.sampling.jax`/compiled-model handoff rather than four parallel draw owners.
- Entry: `Inference.run(spec)` returns `RuntimeRail[InferenceReceipt]` through one `boundary`; `InferenceReceipt.graduates` is the hard convergence gate, returning `Ok(GraduationReceipt(...))` on the uncertainty-law axis when `converged` is true and `Error(BoundaryFault)` when it is false, so a non-converged posterior is an admission rejection on the rail.
- Diagnostics: `arviz.rhat` and `arviz.ess` over the `arviz.InferenceData` trace fill the `r_hat`, `ess_bulk`, and `ess_tail` fields; `converged` is the three-term fold `max(r_hat) < rhat_ceiling and min(ess_bulk) > ess_floor and divergences == 0`. The `model_key` is `ContentIdentity.of` over the full study payload — observed-data bytes, sorted latent specs, likelihood, backend, and `SamplerPlan` — so two studies with the same latent names but different data, backend, or plan key distinctly.
- Packages: `pymc` (`Model`, `sample`, `NUTS`, `Metropolis`, the distribution classes, `sampling.jax.sample_numpyro_nuts`, `sampling.jax.sample_blackjax_nuts`), `numpyro` (the NumPyro JAX NUTS backend pymc dispatches to), `nutpie` (`compile_pymc_model`, `sample` — the Rust/Numba NUTS backend), `blackjax` (the BlackJAX JAX NUTS kernel pymc dispatches to through `sample_blackjax_nuts`), `arviz` (`rhat`, `ess`, `InferenceData`), `numpy`, `msgspec`, `graduation/handoff.md#GRADUATION` (`GraduationReceipt`), runtime (`RuntimeRail`, `boundary`, `BoundaryFault`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new distribution is a `Prior` or `Likelihood` case; a new sampler engine is a `SamplerBackend` case; a new step method is a `SamplerKind` case; the diagnostics are `InferenceReceipt` fields; zero new surface.
- Boundary: classical Bayesian inference only — conjugate, hierarchical, and GLM-class models via gradient MCMC. Variational, normalizing-flow, and neural-posterior estimation and any deep generative model are out of scope. Inference is offline evidence; production uncertainty propagation stays in the C# quantity owners, reached only through the uncertainty-law graduation row. `pymc` pulls `pytensor` to `numba` to `llvmlite` (no cp315 wheel), `arviz` pulls `scipy` (no cp315 wheel), and the `numpyro`/`nutpie`/`blackjax` backends ride the same gate, so every body is authored against the documented API on the marker floor.

```python signature
from typing import TYPE_CHECKING, Literal, assert_never

import msgspec
import numpy as np
from expression import Error, Ok
from msgspec import Struct
from builtins import frozendict

from rasm.compute.graduation.handoff import GraduationReceipt
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    import arviz
    import pymc


type Prior = Literal["normal", "half_normal", "beta", "gamma", "student_t", "uniform"]
type Likelihood = Literal["normal", "bernoulli", "poisson", "binomial", "student_t"]
type SamplerKind = Literal["nuts", "metropolis"]
type SamplerBackend = Literal["pymc_native", "numpyro_jax", "nutpie_numba", "blackjax_jax"]

_PRIOR_CLS: frozendict[Prior, str] = frozendict({
    "normal": "Normal",
    "half_normal": "HalfNormal",
    "beta": "Beta",
    "gamma": "Gamma",
    "student_t": "StudentT",
    "uniform": "Uniform",
})
_LIKELIHOOD_CLS: frozendict[Likelihood, str] = frozendict({
    "normal": "Normal",
    "bernoulli": "Bernoulli",
    "poisson": "Poisson",
    "binomial": "Binomial",
    "student_t": "StudentT",
})


class LatentPrior(Struct, frozen=True):
    name: str
    prior: Prior
    params: dict[str, float]


class SamplerPlan(Struct, frozen=True):
    backend: SamplerBackend = "pymc_native"
    kind: SamplerKind = "nuts"
    draws: int = 2000
    tune: int = 1000
    chains: int = 4
    target_accept: float = 0.9
    seed: int = 0
    ess_floor: float = 400.0
    rhat_ceiling: float = 1.01


class InferenceSpec(Struct, frozen=True):
    observed: np.ndarray
    latents: tuple[LatentPrior, ...]
    likelihood: Likelihood
    mean_latent: str
    plan: SamplerPlan = SamplerPlan()


class InferenceReceipt(Struct, frozen=True):
    likelihood: Likelihood
    backend: SamplerBackend
    posterior_mean: dict[str, float]
    posterior_sd: dict[str, float]
    r_hat: dict[str, float]
    ess_bulk: dict[str, float]
    ess_tail: dict[str, float]
    divergences: int
    draws: int
    converged: bool
    model_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "compute.inference",
            self.mean_subject(),
            {"likelihood": self.likelihood, "backend": self.backend, "converged": str(self.converged)},
        )

    def mean_subject(self) -> str:
        return next(iter(self.posterior_mean), "<empty>")

    def graduates(self) -> "RuntimeRail[GraduationReceipt]":
        if not self.converged:
            return Error(
                BoundaryFault(
                    boundary=(
                        "inference.graduate",
                        f"non-converged posterior: max_rhat={max(self.r_hat.values())}, "
                        f"min_ess_bulk={min(self.ess_bulk.values())}, divergences={self.divergences}",
                    )
                )
            )
        return Ok(
            GraduationReceipt(
                source_package="compute",
                axis="uncertainty-law",
                subject=self.mean_subject(),
                evidence_key=self.model_key,
                residual_limits={"max_rhat": max(self.r_hat.values()), "min_ess_bulk": min(self.ess_bulk.values())},
            )
        )


class Inference:
    @staticmethod
    def run(spec: InferenceSpec) -> "RuntimeRail[InferenceReceipt]":
        return boundary(f"inference.{spec.plan.backend}", lambda: Inference._fit(spec))

    @staticmethod
    def _fit(spec: InferenceSpec) -> InferenceReceipt:
        import pymc

        with pymc.Model() as model:
            nodes = {lat.name: getattr(pymc, _PRIOR_CLS[lat.prior])(lat.name, **lat.params) for lat in spec.latents}
            mu = nodes[spec.mean_latent]
            getattr(pymc, _LIKELIHOOD_CLS[spec.likelihood])("observation", mu, observed=spec.observed)
            trace = _draw(spec.plan, model)
        return Inference._score(spec, trace)

    @staticmethod
    def _score(spec: InferenceSpec, trace: "arviz.InferenceData") -> InferenceReceipt:
        import arviz

        names = [lat.name for lat in spec.latents]
        posterior = trace.posterior
        rhat = arviz.rhat(trace, var_names=names)
        ess_bulk = arviz.ess(trace, var_names=names, method="bulk")
        ess_tail = arviz.ess(trace, var_names=names, method="tail")
        r_hat = {n: float(rhat[n].to_numpy()) for n in names}
        ebk = {n: float(ess_bulk[n].to_numpy()) for n in names}
        plan = spec.plan
        divergences = int(trace.sample_stats["diverging"].to_numpy().sum())
        return InferenceReceipt(
            likelihood=spec.likelihood,
            backend=plan.backend,
            posterior_mean={n: float(posterior[n].mean().to_numpy()) for n in names},
            posterior_sd={n: float(posterior[n].std().to_numpy()) for n in names},
            r_hat=r_hat,
            ess_bulk=ebk,
            ess_tail={n: float(ess_tail[n].to_numpy()) for n in names},
            divergences=divergences,
            draws=plan.draws * plan.chains,
            converged=(max(r_hat.values()) < plan.rhat_ceiling and min(ebk.values()) > plan.ess_floor and divergences == 0),
            model_key=ContentIdentity.of("pymc-model", _study_payload(spec), IdentityPolicy()),
        )


def _draw(plan: SamplerPlan, model: "pymc.Model") -> "arviz.InferenceData":
    import pymc

    match plan.backend:
        case "pymc_native":
            step = pymc.NUTS(model=model) if plan.kind == "nuts" else pymc.Metropolis(model=model)
            return pymc.sample(
                draws=plan.draws,
                tune=plan.tune,
                chains=plan.chains,
                target_accept=plan.target_accept,
                random_seed=plan.seed,
                step=step,
                progressbar=False,
                return_inferencedata=True,
                model=model,
            )
        case "numpyro_jax":
            from pymc.sampling.jax import sample_numpyro_nuts

            return sample_numpyro_nuts(
                draws=plan.draws,
                tune=plan.tune,
                chains=plan.chains,
                target_accept=plan.target_accept,
                random_seed=plan.seed,
                progressbar=False,
                model=model,
            )
        case "blackjax_jax":
            from pymc.sampling.jax import sample_blackjax_nuts

            return sample_blackjax_nuts(
                draws=plan.draws,
                tune=plan.tune,
                chains=plan.chains,
                target_accept=plan.target_accept,
                random_seed=plan.seed,
                progressbar=False,
                model=model,
            )
        case "nutpie_numba":
            import nutpie

            compiled = nutpie.compile_pymc_model(model)
            return nutpie.sample(compiled, draws=plan.draws, tune=plan.tune, chains=plan.chains, seed=plan.seed)
        case unreachable:
            assert_never(unreachable)


def _study_payload(spec: InferenceSpec) -> bytes:
    latents = msgspec.json.encode(sorted(spec.latents, key=lambda lat: lat.name))
    plan = msgspec.json.encode(spec.plan)
    observed = np.ascontiguousarray(spec.observed)
    shape = repr((observed.dtype.str, observed.shape)).encode()
    return b"\x00".join((spec.likelihood.encode(), latents, plan, shape, observed.tobytes()))
```

## [03]-[RESEARCH]

- [PYMC_SAMPLE]: `pymc` and `arviz` carry no cp315 wheel (`pymc` pulls `pytensor` to `numba` to `llvmlite`; `arviz` pulls `scipy`); the `pymc.Model`/`sample`/`NUTS`/`Metropolis`/distribution-class and `arviz.rhat`/`ess`/`InferenceData` spellings verify against the `.api` catalogue once the wheels resolve. The `arviz.InferenceData` group accessor is `trace.sample_stats`, not `trace["sample_stats"]`.
- [NUMPYRO_NUTPIE_BACKENDS]: `numpyro` resolves on the gated `python_version<'3.15'` jaxlib floor (the JAX NUTS path pymc dispatches to through `pymc.sampling.jax.sample_numpyro_nuts`); `nutpie` resolves on the same gated band riding the pymc/numba floor. The `nutpie.compile_pymc_model`/`sample` spellings and the `sample_numpyro_nuts` signature verify against the `.api` catalogue under a uv-sync reflection pass on that band.
- [BLACKJAX_BACKEND]: `blackjax` is installed cp313-only on the gated `python_version<'3.15'` jaxlib companion band beside `numpyro`/`nutpie`, verified against `compute/.api/blackjax.md`. The BlackJAX engine reaches PyMC through `pymc.sampling.jax.sample_blackjax_nuts`, whose `(draws, tune, chains, target_accept, random_seed, progressbar, model)` arity mirrors `sample_numpyro_nuts` and confirms against the `.api` catalogue under the same uv-sync reflection pass; the raw `blackjax.nuts`/`window_adaptation`/`util.run_inference_algorithm` algebra is the bridge PyMC's JAX sampler composes, never re-driven here.
