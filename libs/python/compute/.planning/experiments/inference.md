# [PY_COMPUTE_INFERENCE]

The one classical Bayesian-inference owner over an explicit prior/likelihood/posterior graph. `Inference` builds a `pymc.Model` from a frozen request, draws the posterior with gradient MCMC, scores convergence and predictive fit with arviz, and returns a typed posterior-evidence receipt. The `Distribution` tagged union is the single polymorphic distribution owner carrying typed canonical parameters per case in both the prior and likelihood roles, so a latent prior and the observation likelihood read off one vocabulary rather than two parallel name-string maps. The `SamplerBackend` tagged union carries the per-engine policy each backend needs as case payload — native step kind, JAX chain method, Nutpie compile backend — so the one owner selects the MCMC engine by case: PyMC-native NUTS or Metropolis, NumPyro JAX NUTS, Nutpie Rust/Numba NUTS, or BlackJAX JAX NUTS, with arviz as the cross-backend diagnostic owner. The owner is bounded at conjugate, hierarchical, and GLM-class models; variational, normalizing-flow, and neural-posterior estimation never enter it. A posterior failing the `ConvergenceBar` is an admission rejection on the `uncertainty_law` graduation rail, never a graduated handoff.

## [01]-[INDEX]

- [01]-[BAYESIAN]: the prior/likelihood/posterior graph, the polymorphic distribution union, the sampler-backend axis, the arviz diagnostics, and the graduation-rail convergence gate on one `Inference` owner.

## [02]-[BAYESIAN]

- Owner: `Inference` — the one Bayesian owner over a prior/likelihood/posterior axis. `InferenceSpec` is the frozen request carrying the observed array, the `Latent` family per latent (each a name plus a `Distribution` prior case), the observation `Distribution` likelihood case, the mean latent, and the `SamplerBackend`; `Inference.run` builds the `pymc.Model` by folding each `Distribution.declare` over the model context, draws the posterior, runs `pymc.sample_posterior_predictive` over the fitted model for the predictive-check `DataTree`, scores both with arviz diagnostics, and returns an `InferenceReceipt`. Adding a distribution is one `Distribution` case usable in either role; adding an engine is one `SamplerBackend` case; tightening the bar is a `ConvergenceBar` row; the diagnostics, HDI intervals, and LOO evidence are receipt fields.
- Distribution union: `Distribution` is the ONE `@tagged_union` over the pymc distribution families — `normal(mu, sigma)`, `half_normal(sigma)`, `beta(alpha, beta_)`, `gamma(alpha, beta_)`, `student_t(nu, mu, sigma)`, `uniform(lower, upper)`, `bernoulli(p)`, `poisson(mu)`, `binomial(n, p)` — each case carrying the canonical parameters the pymc class names, so the parameter set is type-checked per case rather than a stringly-typed `dict[str, float]` that drifts from the class signature. `Distribution.declare(name, *, mu, observed)` is the one total `match` projecting a case to its explicit `pymc` distribution-class construction with `observed=None` for a latent prior or the data array for the observation likelihood, the optional `mu` re-parameterizing the location-bearing cases to a latent node so the likelihood mean reads off a fitted prior — prior and likelihood are the same union read in two roles, never two parallel name maps plus an untyped param bag. A case unsupported as a likelihood (a bounded or strictly-positive support against unbounded observations) is the modeler's choice; the union admits the full family and the `declare` fold is closed by `assert_never`.
- Sampler-backend axis: `SamplerBackend` is the `@tagged_union` discriminating the MCMC engine — `pymc_native(kind)` runs `pymc.sample` with a context-bound `pymc.NUTS` or `pymc.Metropolis` step selected by the carried `SamplerKind`, and `external_nuts(sampler)` routes the three accelerated NUTS engines through the one `pymc.sample(nuts_sampler=...)` dispatch on the `NutsSampler` name `numpyro`/`blackjax`/`nutpie` — PyMC owns the model lowering and the JAX/Numba handoff, so the page never re-drives `pymc.sampling.jax` callables, the `nutpie.compile_pymc_model`/`sample` pair, or the raw `blackjax` kernel algebra. The step kind lives inside the `pymc_native` case rather than a parallel `SamplerKind` Literal meaningless for the NUTS-only engines; the three accelerated engines collapse to one `external_nuts` arm parameterized by the `nuts_sampler` name, not three near-identical bodies, and the `engine` projection reads the discriminated name for the receipt and boundary subject. arviz reads the posterior and the `sample_stats` group regardless of which engine sampled, so the diagnostic gate is one fold across every backend.
- Diagnostics: `arviz.summary` over the trace yields the per-variable mean, sd, `r_hat`, `ess_bulk`, and `ess_tail` columns in one call rather than four separate `rhat`/`ess` reductions plus hand-rolled `xarray` mean/sd; `arviz.hdi` fills the credible-interval `hdi` field; `arviz.loo` returns the `ELPDData` whose `elpd_loo`, `p_loo`, and `pareto_k` feed the `elpd_loo`, `p_loo`, and `pareto_k_max` receipt fields as the predictive-reliability evidence (`pareto_k_max` above the `ConvergenceBar.pareto_k_ceiling` is an importance-sampling unreliability signal beside the rhat/ess deficit); `ppc_mean` reads the mean of the `posterior_predictive["observation"]` group. The `model_key` is `ContentIdentity.of` over the full study payload — observed-data bytes, sorted latent specs, likelihood, and backend — so two studies with the same latent names but different data, backend, or plan key distinctly.
- Convergence bar: `ConvergenceBar` is the policy row carrying `rhat_ceiling`, `ess_floor`, `max_divergences`, and `pareto_k_ceiling`; the `measured` and `ceiling` properties project the four residuals (`max_rhat`, neg-`min_ess_bulk`, `divergences`, `pareto_k_max`) into the ledger pair the graduation rail admits, and `converged` folds `measured` against `ceiling`, so a stricter bar is a tighter `ConvergenceBar` row, never a new gate.
- Entry: `Inference.run(spec)` returns `RuntimeRail[InferenceReceipt]` through one `boundary`; `InferenceReceipt.graduates` routes the measured-vs-ceiling ledger through the one `GraduationReceipt.graduates(source_package, HandoffAxis(uncertainty_law=subject), evidence_key, measured, ceiling)` admission rail rather than inlining a residual comparison, so a non-converged posterior is the `Error(BoundaryFault)` the shared `_admit` fold returns and a converged one is the admitted `GraduationReceipt` — the same residual-over-ceiling gate the sibling solver, convex, and array-layout owners feed, never a parallel admission body.
- Packages: `pymc` (`Model`, `sample` with the `step`/`nuts_sampler`/`return_inferencedata` axis, `sample_posterior_predictive`, `NUTS`, `Metropolis`, the distribution classes), `numpyro`/`blackjax`/`nutpie` (the accelerated NUTS engines `pymc.sample(nuts_sampler=...)` dispatches to — installed only so PyMC's own dispatch resolves them, never imported here), `arviz` (`summary`, `hdi`, `loo`, `ELPDData`), `xarray` (`DataTree` — the posterior container), `numpy`, `expression` (`tagged_union`/`case`/`tag`; `Result` underlies the runtime `RuntimeRail`), `msgspec`, `graduation/handoff.md#GRADUATION` (`GraduationReceipt`, `HandoffAxis`), runtime (`RuntimeRail`, `boundary`, `BoundaryFault`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new distribution is one `Distribution` case plus one `declare` arm usable in either role; a new sampler engine is one `SamplerBackend` case; a new convergence dimension is one `ConvergenceBar` row threaded into the measured/ceiling ledger; the diagnostics, HDI, and LOO evidence are `InferenceReceipt` fields; zero new surface.
- Boundary: classical Bayesian inference only — conjugate, hierarchical, and GLM-class models via gradient MCMC. Variational, normalizing-flow, and neural-posterior estimation and any deep generative model are out of scope. Inference is offline evidence; production uncertainty propagation stays in the C# quantity owners, reached only through the `uncertainty_law` graduation row. A stringly-typed distribution-name map plus an untyped param bag, a parallel `SamplerKind` Literal duplicating the backend axis, three near-identical accelerated-engine draw arms re-driving `pymc.sampling.jax`/`nutpie`/`blackjax` instead of PyMC's own `nuts_sampler` dispatch, a hand-rolled residual-ceiling comparison duplicating the `GraduationReceipt._admit` fold, and a stale `GraduationReceipt(axis=..., subject=..., residual_limits=...)` constructor that does not match the handoff owner are the deleted forms. `pymc` pulls `pytensor` to `numba` to `llvmlite` (no cp315 wheel), `arviz` pulls `scipy` (no cp315 wheel), and the `numpyro`/`nutpie`/`blackjax` engines ride the same gated `python_version<'3.15'` jaxlib/numba companion band, so every body is authored against the documented API on the marker floor.

```python signature
from __future__ import annotations

from typing import TYPE_CHECKING, Literal, assert_never

import msgspec
import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from xarray import DataTree


type SamplerKind = Literal["nuts", "metropolis"]
type NutsSampler = Literal["numpyro", "blackjax", "nutpie"]


@tagged_union(frozen=True)
class Distribution:
    tag: Literal[
        "normal", "half_normal", "beta", "gamma", "student_t", "uniform", "bernoulli", "poisson", "binomial"
    ] = tag()
    normal: tuple[float, float] = case()
    half_normal: float = case()
    beta: tuple[float, float] = case()
    gamma: tuple[float, float] = case()
    student_t: tuple[float, float, float] = case()
    uniform: tuple[float, float] = case()
    bernoulli: float = case()
    poisson: float = case()
    binomial: tuple[int, float] = case()

    @staticmethod
    def Normal(mu: float, sigma: float) -> Distribution:
        return Distribution(normal=(mu, sigma))

    @staticmethod
    def HalfNormal(sigma: float) -> Distribution:
        return Distribution(half_normal=sigma)

    @staticmethod
    def Beta(alpha: float, beta_: float) -> Distribution:
        return Distribution(beta=(alpha, beta_))

    @staticmethod
    def Gamma(alpha: float, beta_: float) -> Distribution:
        return Distribution(gamma=(alpha, beta_))

    @staticmethod
    def StudentT(nu: float, mu: float, sigma: float) -> Distribution:
        return Distribution(student_t=(nu, mu, sigma))

    @staticmethod
    def Uniform(lower: float, upper: float) -> Distribution:
        return Distribution(uniform=(lower, upper))

    @staticmethod
    def Bernoulli(p: float) -> Distribution:
        return Distribution(bernoulli=p)

    @staticmethod
    def Poisson(mu: float) -> Distribution:
        return Distribution(poisson=mu)

    @staticmethod
    def Binomial(n: int, p: float) -> Distribution:
        return Distribution(binomial=(n, p))

    def declare(self, name: str, /, *, mu: object = None, observed: np.ndarray | None = None) -> object:
        import pymc

        match self:
            case Distribution(tag="normal", normal=(m, s)):
                return pymc.Normal(name, mu=mu if mu is not None else m, sigma=s, observed=observed)
            case Distribution(tag="half_normal", half_normal=s):
                return pymc.HalfNormal(name, sigma=s, observed=observed)
            case Distribution(tag="beta", beta=(a, b)):
                return pymc.Beta(name, alpha=a, beta=b, observed=observed)
            case Distribution(tag="gamma", gamma=(a, b)):
                return pymc.Gamma(name, alpha=a, beta=b, observed=observed)
            case Distribution(tag="student_t", student_t=(nu, m, s)):
                return pymc.StudentT(name, nu=nu, mu=mu if mu is not None else m, sigma=s, observed=observed)
            case Distribution(tag="uniform", uniform=(lo, hi)):
                return pymc.Uniform(name, lower=lo, upper=hi, observed=observed)
            case Distribution(tag="bernoulli", bernoulli=p):
                return pymc.Bernoulli(name, p=mu if mu is not None else p, observed=observed)
            case Distribution(tag="poisson", poisson=m):
                return pymc.Poisson(name, mu=mu if mu is not None else m, observed=observed)
            case Distribution(tag="binomial", binomial=(n, p)):
                return pymc.Binomial(name, n=n, p=mu if mu is not None else p, observed=observed)
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class SamplerBackend:
    tag: Literal["pymc_native", "external_nuts"] = tag()
    pymc_native: SamplerKind = case()
    external_nuts: NutsSampler = case()

    @staticmethod
    def Native(kind: SamplerKind = "nuts") -> SamplerBackend:
        return SamplerBackend(pymc_native=kind)

    @staticmethod
    def Numpyro() -> SamplerBackend:
        return SamplerBackend(external_nuts="numpyro")

    @staticmethod
    def Blackjax() -> SamplerBackend:
        return SamplerBackend(external_nuts="blackjax")

    @staticmethod
    def Nutpie() -> SamplerBackend:
        return SamplerBackend(external_nuts="nutpie")

    @property
    def engine(self) -> str:
        return self.pymc_native if self.tag == "pymc_native" else self.external_nuts

    def draw(self, /, *, draws: int, tune: int, chains: int, seed: int) -> DataTree:
        import pymc

        match self:
            case SamplerBackend(tag="pymc_native", pymc_native=kind):
                step = pymc.NUTS() if kind == "nuts" else pymc.Metropolis()
                return pymc.sample(
                    draws=draws, tune=tune, chains=chains,
                    random_seed=seed, step=step, return_inferencedata=True,
                )
            case SamplerBackend(tag="external_nuts", external_nuts=sampler):
                return pymc.sample(
                    draws=draws, tune=tune, chains=chains,
                    random_seed=seed, nuts_sampler=sampler, return_inferencedata=True,
                )
            case _ as unreachable:
                assert_never(unreachable)


class Latent(Struct, frozen=True):
    name: str
    prior: Distribution


class ConvergenceBar(Struct, frozen=True):
    rhat_ceiling: float = 1.01
    ess_floor: float = 400.0
    max_divergences: int = 0
    pareto_k_ceiling: float = 0.7


class SamplerPlan(Struct, frozen=True):
    backend: SamplerBackend = msgspec.field(default_factory=SamplerBackend.Native)
    draws: int = 2000
    tune: int = 1000
    chains: int = 4
    seed: int = 0
    bar: ConvergenceBar = msgspec.field(default_factory=ConvergenceBar)


class InferenceSpec(Struct, frozen=True):
    observed: np.ndarray
    latents: tuple[Latent, ...]
    likelihood: Distribution
    mean_latent: str
    plan: SamplerPlan = msgspec.field(default_factory=SamplerPlan)


class InferenceReceipt(Struct, frozen=True):
    likelihood: str
    backend: str
    posterior_mean: dict[str, float]
    posterior_sd: dict[str, float]
    r_hat: dict[str, float]
    ess_bulk: dict[str, float]
    ess_tail: dict[str, float]
    hdi: dict[str, tuple[float, float]]
    ppc_mean: float
    elpd_loo: float
    p_loo: float
    pareto_k_max: float
    divergences: int
    draws: int
    bar: ConvergenceBar
    model_key: ContentKey

    @property
    def measured(self) -> dict[str, float]:
        return {
            "max_rhat": max(self.r_hat.values()),
            "neg_min_ess_bulk": -min(self.ess_bulk.values()),
            "divergences": float(self.divergences),
            "pareto_k_max": self.pareto_k_max,
        }

    @property
    def ceiling(self) -> dict[str, float]:
        return {
            "max_rhat": self.bar.rhat_ceiling,
            "neg_min_ess_bulk": -self.bar.ess_floor,
            "divergences": float(self.bar.max_divergences),
            "pareto_k_max": self.bar.pareto_k_ceiling,
        }

    @property
    def converged(self) -> bool:
        return all(self.measured[k] <= cap for k, cap in self.ceiling.items())

    def mean_subject(self) -> str:
        return next(iter(self.posterior_mean), "<empty>")

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "compute.inference",
            self.mean_subject(),
            {"likelihood": self.likelihood, "backend": self.backend, "converged": str(self.converged)},
        )

    def graduates(self) -> RuntimeRail[GraduationReceipt]:
        return GraduationReceipt.graduates(
            "compute", HandoffAxis(uncertainty_law=self.mean_subject()), self.model_key, self.measured, self.ceiling
        )


class Inference:
    @staticmethod
    def run(spec: InferenceSpec) -> RuntimeRail[InferenceReceipt]:
        return boundary(f"inference.{spec.plan.backend.engine}", lambda: Inference._fit(spec))

    @staticmethod
    def _fit(spec: InferenceSpec) -> InferenceReceipt:
        import pymc

        plan = spec.plan
        with pymc.Model() as model:
            nodes = {lat.name: lat.prior.declare(lat.name) for lat in spec.latents}
            spec.likelihood.declare("observation", mu=nodes[spec.mean_latent], observed=spec.observed)
            trace = plan.backend.draw(draws=plan.draws, tune=plan.tune, chains=plan.chains, seed=plan.seed)
            ppc = pymc.sample_posterior_predictive(
                trace, model=model, var_names=["observation"], random_seed=plan.seed, return_inferencedata=True
            )
        return Inference._score(spec, trace, ppc)

    @staticmethod
    def _score(spec: InferenceSpec, trace: DataTree, ppc: DataTree) -> InferenceReceipt:
        import arviz

        names = [lat.name for lat in spec.latents]
        plan = spec.plan
        summary = arviz.summary(trace, var_names=names, kind="all")
        hdi = arviz.hdi(trace, var_names=names)
        loo = arviz.loo(trace, pointwise=True)
        col = lambda key: {n: float(summary.loc[n, key]) for n in names}
        return InferenceReceipt(
            likelihood=spec.likelihood.tag,
            backend=plan.backend.engine,
            posterior_mean=col("mean"),
            posterior_sd=col("sd"),
            r_hat=col("r_hat"),
            ess_bulk=col("ess_bulk"),
            ess_tail=col("ess_tail"),
            hdi={n: (float(hdi[n].sel(hdi="lower")), float(hdi[n].sel(hdi="higher"))) for n in names},
            ppc_mean=float(ppc.posterior_predictive["observation"].mean().to_numpy()),
            elpd_loo=float(loo.elpd_loo),
            p_loo=float(loo.p_loo),
            pareto_k_max=float(np.asarray(loo.pareto_k).max()),
            divergences=int(trace.sample_stats["diverging"].to_numpy().sum()),
            draws=plan.draws * plan.chains,
            bar=plan.bar,
            model_key=ContentIdentity.of("pymc-model", _study_payload(spec), IdentityPolicy()),
        )


def _study_payload(spec: InferenceSpec) -> bytes:
    latents = msgspec.json.encode(sorted(spec.latents, key=lambda lat: lat.name))
    likelihood = msgspec.json.encode(spec.likelihood)
    backend = msgspec.json.encode(spec.plan.backend)
    observed = np.ascontiguousarray(spec.observed)
    shape = repr((observed.dtype.str, observed.shape)).encode()
    return b"\x00".join((likelihood, latents, backend, shape, observed.tobytes()))
```

## [03]-[RESEARCH]

- [PYMC_SAMPLE]: `pymc` and `arviz` carry no cp315 wheel (`pymc` pulls `pytensor` to `numba` to `llvmlite`; `arviz` pulls `scipy`); the `pymc.Model`/`sample`/`sample_posterior_predictive`/`NUTS`/`Metropolis` and the `Normal`/`HalfNormal`/`Beta`/`Gamma`/`StudentT`/`Uniform`/`Bernoulli`/`Poisson`/`Binomial` distribution-class spellings with their `.api`-confirmed canonical params verify against the catalogue once the wheels resolve. `sample(draws, *, tune, chains, cores, random_seed, step, nuts_sampler, initvals, init, return_inferencedata)` (`compute/.api/pymc.md` ENTRYPOINTS [01]) is the one MCMC entrypoint: the `step` arm carries the context-bound `NUTS`/`Metropolis` step method, and the `nuts_sampler` arm names the accelerated NUTS engine PyMC dispatches to — `step` and `nuts_sampler` are the two confirmed engine channels, and the page passes neither `target_accept` nor `progressbar` nor a `model=` kwarg because the catalogued `sample` and the `NUTS(vars, max_treedepth, ...)`/`Metropolis(vars, proposal_dist, scaling, tune)` step constructors do not list them (the step methods and `sample` bind to the active `pm.Model()` context). The posterior container is `xarray.DataTree` (the `.api` flags `arviz.InferenceData` as the deprecated alias to reject); the group accessor is `trace.sample_stats`/`trace.posterior_predictive`, not `trace["sample_stats"]`. `sample_posterior_predictive(trace, model, *, var_names, random_seed, return_inferencedata)` with `return_inferencedata=True` returns a `DataTree` carrying the `posterior_predictive` group beside the fitted-model draws.
- [ARVIZ_DIAGNOSTICS]: `arviz.summary(data, var_names, kind="all")` returns the combined mean/sd/`r_hat`/`ess_bulk`/`ess_tail` table in one reduction (ENTRYPOINTS summary [05]), collapsing the prior four-call `rhat`+`ess(method=)` fan-out into one frame the receipt reads by column; `arviz.hdi(data, var_names)` (interval [01]) fills the credible-interval field; `arviz.loo(data, pointwise=True)` returns the `ELPDData` carrying `elpd_loo`, `p_loo`, and the pointwise `pareto_k` (model-comparison [01], PUBLIC_TYPES `ELPDData` [02]) — the predictive-reliability evidence, `pareto_k_max` the importance-sampling unreliability signal beside the rhat/ess deficit. The `summary` column labels, the `hdi` `lower`/`higher` coordinate, and the `ELPDData` field names verify against `compute/.api/arviz.md` once the scipy wheel resolves.
- [ACCELERATED_NUTS_ENGINES]: `numpyro`, `blackjax`, and `nutpie` resolve on the gated `python_version<'3.15'` jaxlib/numba companion band beside the `pymc`/`arviz` floor, verified against `compute/.api/{numpyro,blackjax,nutpie}.md`. All three reach PyMC through the one confirmed `pymc.sample(..., nuts_sampler=<name>)` dispatch (`compute/.api/pymc.md` ENTRYPOINTS [01]) — PyMC owns the model lowering and the JAX/Numba handoff, so the page imports none of them. The phantom `pymc.sampling.jax.sample_numpyro_nuts`/`sample_blackjax_nuts` callables do not appear in the `pymc` catalogue and are not driven here; the raw `nutpie.compile_pymc_model`/`sample` pair (`nutpie.md` ENTRYPOINTS [01]/[03]) and the `blackjax.nuts`/`window_adaptation`/`util.run_inference_algorithm` algebra (`blackjax.md`) are the lower-level surfaces PyMC's `nuts_sampler` dispatch composes, never re-driven from this owner. The accelerated engines collapse to the one `external_nuts` arm parameterized by the `nuts_sampler` name, not three near-identical bodies.
