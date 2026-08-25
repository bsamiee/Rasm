# [PY_COMPUTE_INFERENCE]

One classical Bayesian-inference owner over an explicit prior/likelihood/posterior graph: `Inference.run` builds a `pymc.Model` from a frozen request, draws the posterior with gradient MCMC across a backend axis, scores convergence and predictive fit with `arviz`, and graduates a typed posterior-evidence receipt through the `uncertainty_law` admission rail. This owner is bounded at conjugate and GLM-class models over scalar latent nodes — a vector group-level latent the per-variable summary fold cannot key by a single name is out of scope, as are variational, normalizing-flow, and neural-posterior estimation. A posterior failing the `ConvergenceBar` is an admission rejection on the graduation rail, never a graduated handoff.

Three polymorphic surfaces carry every variation: `Distribution` over the `pymc` families, read in both the prior and likelihood roles off one vocabulary; `SamplerBackend` over the MCMC engine and its per-engine policy; the `ConvergenceBar` policy row folded against the `_RESIDUALS` dimension table, so a stricter bar is a tighter row, never a new gate. This run rides the `EvidenceScope.INFERENCE` weave — span, a `boundary` fence narrowed to the posterior stack's own raise set, beartype guard, fenced contributor harvest onto the one runtime receipt spine — the same composed form `experiments/model#ASSET` and `graduation/handoff#GRADUATION` hold. The narrowing resolves at first dispatch, so naming `pymc`'s exception classes never reifies the compile chain the page defers.

## [01]-[INDEX]

- [02]-[BAYESIAN]: the prior/likelihood/posterior graph on one `Inference` owner — the `Distribution` and `SamplerBackend` unions, the `arviz` diagnostic fold, and the graduation-rail convergence gate.

## [02]-[BAYESIAN]

- Owner: `Inference` — `InferenceSpec` is the frozen request; `InferenceReceipt.graduates` routes the measured-versus-ceiling ledger through the shared `graduation/handoff#GRADUATION` admission rail, the same gate the sibling solver, convex, and array-layout owners feed, never a parallel admission body.
- Cases: `Distribution` is one union read in both roles, each case carrying its canonical parameters as a typed tuple — never a stringly `dict[str, float]` drifting from the class signature; the union's own keyword constructor is the construction surface, no parallel factory family re-wraps the cases.
- Law: the async `run` fold charges one `Resource.RECORD` `MeterFact` over the sample population — draws times chains off the `SamplerPlan`, surfaced by the sampler engine — because that fold is the nearest async owner of a count the offloaded kernel produced and binds no plane for. The charge lands off the cleared arm alone, since a run refused at admission drew nothing, and the plan carries the two factors separately where the receipt fuses them into one `draws` column. The resource already names its series at the journal owner, so no metric row is minted beside the receipt fan.
- Auto: PyMC owns the model lowering and the JAX/Numba handoff — this page never re-drives `pymc.sampling.jax`, the `nutpie.compile_pymc_model`/`sample` pair, or the raw `blackjax` kernel algebra, and the accelerated engines install only so PyMC's own dispatch resolves them, never as imports here. Sampling never retries: the posterior draw is the evidence, and worker-death handling stays the lane's.
- Output: `ConvergenceBar` folds against the `_RESIDUALS` table, so a new convergence dimension is one `_Residual` row and one bar field; a `metropolis` trace carries no `diverging` sample stat — divergence counting is a gradient-sampler diagnostic — so that dimension is ABSENT from both the measured ledger and the ceiling it would be graded against, and the receipt's warning band names it. A fabricated `0` there read as a gradient run that diverged never and cleared the bar by construction, and the ceiling projects over exactly the measured keys so the hub's key-coverage gate never refuses a run for a dimension nobody asked it to measure. Predictive fit is one `_score` fold with two rows behind the `loo_cells` pointwise budget — the full PSIS-LOO matrix within it, the `loo_subsample` difference estimator above it with one `update_subsample` refinement while the sub-sampling SE dominates; `ELPDData.kind` stays `"loo"` on BOTH rows, so the receipt discriminates on the typed `subsample_obs`/`subsample_se` pair (`None` spells the full fold), and the subsampled `pareto_k` keeps full length with NaN at unsampled rows, read nan-aware.
- Growth: a new distribution is one `Distribution` case and one `declare` arm usable in either role; a new sampler engine is one `SamplerBackend` case or one `external_nuts` name; a new convergence dimension is one `ConvergenceBar` field and one `_Residual`; a new per-variable diagnostic is one `PosteriorSummary` field; a new predictive-scoring row is one `_score` arm behind its `SamplerPlan` policy field.

```python
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from typing import TYPE_CHECKING, Final, Literal, assert_never

import msgspec
import numpy as np
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.journal import Journal, MeterFact, Resource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import arviz
lazy import pymc

if TYPE_CHECKING:
    from xarray import DataTree

# --- [TYPES] ----------------------------------------------------------------------------

type SamplerKind = Literal["nuts", "metropolis"]
type NutsSampler = Literal["numpyro", "blackjax", "nutpie"]
type NutsOption = str | int | float | bool
type NutsOptions = tuple[tuple[str, NutsOption], ...]


@tagged_union(frozen=True)
class Distribution:
    tag: Literal["normal", "half_normal", "beta", "gamma", "student_t", "uniform", "bernoulli", "poisson", "binomial"] = tag()
    normal: tuple[float, float] = case()
    half_normal: float = case()
    beta: tuple[float, float] = case()
    gamma: tuple[float, float] = case()
    student_t: tuple[float, float, float] = case()
    uniform: tuple[float, float] = case()
    bernoulli: float = case()
    poisson: float = case()
    binomial: tuple[int, float] = case()

    def declare(self, name: str, /, *, mu: object = None, observed: np.ndarray | None = None) -> object:
        match self:
            case Distribution(tag="normal", normal=(m, s)):
                return pymc.Normal(name, mu=m if mu is None else mu, sigma=s, observed=observed)
            case Distribution(tag="half_normal", half_normal=s):
                return pymc.HalfNormal(name, sigma=s, observed=observed)
            case Distribution(tag="beta", beta=(a, b)):
                return pymc.Beta(name, alpha=a, beta=b, observed=observed)
            case Distribution(tag="gamma", gamma=(a, b)):
                return pymc.Gamma(name, alpha=a, beta=b, observed=observed)
            case Distribution(tag="student_t", student_t=(nu, m, s)):
                return pymc.StudentT(name, nu=nu, mu=m if mu is None else mu, sigma=s, observed=observed)
            case Distribution(tag="uniform", uniform=(lo, hi)):
                return pymc.Uniform(name, lower=lo, upper=hi, observed=observed)
            case Distribution(tag="bernoulli", bernoulli=p):
                return pymc.Bernoulli(name, p=p if mu is None else pymc.math.invlogit(mu), observed=observed)
            case Distribution(tag="poisson", poisson=m):
                return pymc.Poisson(name, mu=m if mu is None else pymc.math.exp(mu), observed=observed)
            case Distribution(tag="binomial", binomial=(n, p)):
                return pymc.Binomial(name, n=n, p=p if mu is None else pymc.math.invlogit(mu), observed=observed)
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def canonical(self) -> tuple[str, tuple[float, ...]]:
        match self:
            case (
                Distribution(tag="normal", normal=p)
                | Distribution(tag="beta", beta=p)
                | Distribution(tag="gamma", gamma=p)
                | Distribution(tag="uniform", uniform=p)
                | Distribution(tag="student_t", student_t=p)
            ):
                return self.tag, p
            case Distribution(tag="binomial", binomial=(n, p)):
                return self.tag, (float(n), p)
            case Distribution(tag="half_normal", half_normal=v) | Distribution(tag="bernoulli", bernoulli=v) | Distribution(tag="poisson", poisson=v):
                return self.tag, (v,)
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class SamplerBackend:
    tag: Literal["pymc_native", "external_nuts"] = tag()
    pymc_native: SamplerKind = case()
    external_nuts: tuple[NutsSampler, NutsOptions] = case()

    @property
    def engine(self) -> str:
        return self.pymc_native if self.tag == "pymc_native" else self.external_nuts[0]

    @property
    def canonical(self) -> tuple[str, NutsOptions]:
        match self:
            case SamplerBackend(tag="pymc_native", pymc_native=kind):
                return kind, ()
            case SamplerBackend(tag="external_nuts", external_nuts=(sampler, options)):
                return sampler, options
            case _ as unreachable:
                assert_never(unreachable)

    def draw(self, /, *, draws: int, tune: int, chains: int, seed: int) -> DataTree:
        match self:
            case SamplerBackend(tag="pymc_native", pymc_native=kind):
                step = pymc.NUTS() if kind == "nuts" else pymc.Metropolis()
                return pymc.sample(draws=draws, tune=tune, chains=chains, random_seed=seed, step=step, return_inferencedata=True)
            case SamplerBackend(tag="external_nuts", external_nuts=(sampler, options)):
                return pymc.sample(
                    draws=draws,
                    tune=tune,
                    chains=chains,
                    random_seed=seed,
                    nuts_sampler=sampler,
                    nuts_sampler_kwargs=dict(options) or None,
                    return_inferencedata=True,
                )
            case _ as unreachable:
                assert_never(unreachable)


# --- [MODELS] ---------------------------------------------------------------------------


class Latent(Struct, frozen=True):
    name: str
    prior: Distribution


class ConvergenceBar(Struct, frozen=True):
    rhat_ceiling: float = 1.01
    ess_floor: float = 400.0
    max_divergences: int = 0
    pareto_k_ceiling: float = 0.7
    prior_sensitivity_ceiling: float = 0.2


class SamplerPlan(Struct, frozen=True):
    backend: SamplerBackend = msgspec.field(default_factory=lambda: SamplerBackend(pymc_native="nuts"))
    draws: int = 2000
    tune: int = 1000
    chains: int = 4
    seed: int = 0
    hdi_prob: float = 0.94
    loo_cells: int = 10_000_000
    loo_obs: int = 400
    loo_refine: float = 0.25
    bar: ConvergenceBar = msgspec.field(default_factory=ConvergenceBar)


class InferenceSpec(Struct, frozen=True):
    observed: np.ndarray
    latents: tuple[Latent, ...]
    likelihood: Distribution
    mean_latent: str
    plan: SamplerPlan = msgspec.field(default_factory=SamplerPlan)


class StudyPayload(Struct, frozen=True):
    likelihood: tuple[str, tuple[float, ...]]
    latents: tuple[tuple[str, tuple[str, tuple[float, ...]]], ...]
    mean_latent: str
    backend: tuple[str, NutsOptions]
    observed_dtype: str
    observed_shape: tuple[int, ...]
    observed_bytes: bytes


@dataclass(slots=True, frozen=True)
class _Residual:
    key: str
    measure: Callable[[InferenceReceipt], Option[float]]
    ceiling: Callable[[ConvergenceBar], float]


class PosteriorSummary(Struct, frozen=True):
    mean: float
    sd: float
    r_hat: float
    ess_bulk: float
    ess_tail: float
    hdi: tuple[float, float]


class InferenceReceipt(Struct, frozen=True):
    likelihood: str
    backend: str
    summaries: dict[str, PosteriorSummary]
    ppc_mean: float
    elpd: float
    p_eff: float
    subsample_obs: Option[int]
    subsample_se: Option[float]
    pareto_k_max: float
    prior_sensitivity_max: float
    divergences: Option[int]
    draws: int
    bar: ConvergenceBar
    model_key: ContentKey

    @property
    def measured(self) -> dict[str, float]:
        return dict(Block.of_seq(_RESIDUALS).choose(lambda row: row.measure(self).map(lambda value: (row.key, value))))

    @property
    def ceiling(self) -> dict[str, float]:
        measured = self.measured
        return {row.key: row.ceiling(self.bar) for row in _RESIDUALS if row.key in measured}

    @property
    def band(self) -> Block[str]:
        measured = self.measured
        return Block.of_seq(f"unmeasured:{row.key}" for row in _RESIDUALS if row.key not in measured)

    @property
    def converged(self) -> bool:
        ceiling = self.ceiling
        return all(value <= ceiling[key] for key, value in self.measured.items())

    @property
    def span_facts(self) -> dict[str, str | int | float | bool]:
        return {"converged": self.converged, "draws": self.draws, "max_pareto_k": self.pareto_k_max, "unmeasured": len(self.band)}

    def subject(self) -> str:
        return f"{self.likelihood}:{self.backend}"

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "likelihood": self.likelihood,
            "backend": self.backend,
            "converged": self.converged,
            "ppc_mean": self.ppc_mean,
            "elpd": self.elpd,
            "p_eff": self.p_eff,
            "draws": self.draws,
            "subsample_obs": self.subsample_obs.to_optional(),
            "subsample_se": self.subsample_se.to_optional(),
            **self.measured,
        }
        return (
            Receipt.of(
                EvidenceScope.INFERENCE.value,
                ("emitted", self.subject(), facts),
                key=Some(self.model_key),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.model_key)),
                band=self.band,
            ),
        )

    def graduates(self, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[GraduationReceipt]:
        return GraduationReceipt.graduates(
            EvidenceScope.INFERENCE.value,
            HandoffAxis(uncertainty_law=self.subject()),
            self.model_key,
            self.measured,
            self.ceiling,
            composition=composition,
        )


# --- [TABLES] ---------------------------------------------------------------------------

INFERENCE_FIT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.INFERENCE, point="fit", arm="boundary", defect="posterior-draw", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([INFERENCE_FIT]))

_RESIDUALS: Final[Block[_Residual]] = Block.of_seq([
    _Residual("max_rhat", lambda r: Some(max(s.r_hat for s in r.summaries.values())), lambda b: b.rhat_ceiling),
    _Residual("neg_min_ess_bulk", lambda r: Some(-min(s.ess_bulk for s in r.summaries.values())), lambda b: -b.ess_floor),
    _Residual("divergences", lambda r: r.divergences.map(float), lambda b: float(b.max_divergences)),
    _Residual("pareto_k_max", lambda r: Some(r.pareto_k_max), lambda b: b.pareto_k_ceiling),
    _Residual("prior_sensitivity_max", lambda r: Some(r.prior_sensitivity_max), lambda b: b.prior_sensitivity_ceiling),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _posterior_raises() -> Catch:
    return (BeartypeCallHintViolation, pymc.exceptions.ShapeError, RuntimeError, TypeError, ValueError)


def _fit_kernel(spec: "InferenceSpec") -> "RuntimeRail[InferenceReceipt]":
    return boundary(INFERENCE_FIT, lambda: Inference._fit(spec), catch=_posterior_raises()).bind(lambda outcome: outcome)


def _metered(engine: str, plan: SamplerPlan) -> MeterFact:
    return MeterFact(resource=Resource.RECORD, quantity=plan.draws * plan.chains, surface=engine)


class Inference:
    @staticmethod
    async def run(spec: InferenceSpec, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[InferenceReceipt]:
        engine = spec.plan.backend.engine
        trait = KernelTrait.HOSTILE if spec.plan.backend.tag == "external_nuts" else KernelTrait.RELEASING

        async def dispatch() -> RuntimeRail[InferenceReceipt]:
            return (await lane.offload(Kernel.of(_fit_kernel, trait), spec)).bind(lambda rail: rail)

        facts = {"engine": engine, "likelihood": spec.likelihood.tag, "draws": spec.plan.draws, "chains": spec.plan.chains}
        settled = await evidence_run(EvidenceScope.INFERENCE, f"inference.{engine}", dispatch, facts=facts, composition=composition)
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(_metered(engine, spec.plan), scope=composition)).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _fit(spec: InferenceSpec) -> "RuntimeRail[InferenceReceipt]":
        plan, names = spec.plan, [lat.name for lat in spec.latents]
        with pymc.Model() as model:
            nodes = {lat.name: lat.prior.declare(lat.name) for lat in spec.latents}
            spec.likelihood.declare("observation", mu=nodes[spec.mean_latent], observed=spec.observed)
            trace = plan.backend.draw(draws=plan.draws, tune=plan.tune, chains=plan.chains, seed=plan.seed)
            pymc.compute_log_likelihood(trace, model=model)
            ppc = pymc.sample_posterior_predictive(trace, model=model, var_names=["observation"], random_seed=plan.seed, return_inferencedata=True)
        summary = arviz.summary(trace, var_names=names, kind="all")
        hdi = arviz.hdi(trace, var_names=names, prob=plan.hdi_prob)
        loo = _score(trace, plan, n_obs=int(np.ascontiguousarray(spec.observed).size))
        psense = arviz.psense_summary(trace)
        summaries = {
            n: PosteriorSummary(
                mean=float(summary.loc[n, "mean"]),
                sd=float(summary.loc[n, "sd"]),
                r_hat=float(summary.loc[n, "r_hat"]),
                ess_bulk=float(summary.loc[n, "ess_bulk"]),
                ess_tail=float(summary.loc[n, "ess_tail"]),
                hdi=(float(hdi[n].sel(ci_bound="lower")), float(hdi[n].sel(ci_bound="upper"))),
            )
            for n in names
        }
        def settled(model_key: ContentKey) -> InferenceReceipt:
            return InferenceReceipt(
                likelihood=spec.likelihood.tag,
                backend=plan.backend.engine,
                summaries=summaries,
                ppc_mean=float(ppc.posterior_predictive["observation"].mean().to_numpy()),
                elpd=float(loo.elpd),
                p_eff=float(loo.p),
                subsample_obs=Option.of_optional(loo.subsample_size).map(int),
                subsample_se=Option.of_optional(loo.subsampling_se).map(float),
                pareto_k_max=float(np.nanmax(np.asarray(loo.pareto_k))),
                prior_sensitivity_max=float(np.asarray(psense["prior"]).max()),
                divergences=Some(int(trace.sample_stats["diverging"].to_numpy().sum())) if "diverging" in trace.sample_stats else Nothing,
                draws=plan.draws * plan.chains,
                bar=plan.bar,
                model_key=model_key,
            )

        return ContentIdentity.of("pymc-model", _study_payload(spec)).map(settled)


def _score(trace: "DataTree", plan: SamplerPlan, n_obs: int) -> object:
    if n_obs * plan.draws * plan.chains <= plan.loo_cells:
        return arviz.loo(trace, var_name="observation", pointwise=True)
    scored = arviz.loo_subsample(trace, observations=min(plan.loo_obs, n_obs), var_name="observation", seed=plan.seed)
    remaining = n_obs - int(scored.subsample_size)
    if remaining > 0 and float(scored.subsampling_se) > plan.loo_refine * float(scored.se):
        return arviz.update_subsample(scored, trace, observations=min(plan.loo_obs, remaining), seed=plan.seed + 1)
    return scored


def _study_payload(spec: InferenceSpec) -> StudyPayload:
    observed = np.ascontiguousarray(spec.observed)
    return StudyPayload(
        likelihood=spec.likelihood.canonical,
        latents=tuple((lat.name, lat.prior.canonical) for lat in sorted(spec.latents, key=lambda lat: lat.name)),
        mean_latent=spec.mean_latent,
        backend=spec.plan.backend.canonical,
        observed_dtype=observed.dtype.str,
        observed_shape=observed.shape,
        observed_bytes=observed.tobytes(),
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
