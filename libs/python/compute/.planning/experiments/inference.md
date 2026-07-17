# [PY_COMPUTE_INFERENCE]

One classical Bayesian-inference owner over an explicit prior/likelihood/posterior graph: `Inference.run` builds a `pymc.Model` from a frozen request, draws the posterior with gradient MCMC across a backend axis, scores convergence and predictive fit with `arviz`, and graduates a typed posterior-evidence receipt through the `uncertainty_law` admission rail. This owner is bounded at conjugate and GLM-class models over scalar latent nodes — a vector group-level latent the per-variable summary fold cannot key by a single name is out of scope, as are variational, normalizing-flow, and neural-posterior estimation. A posterior failing the `ConvergenceBar` is an admission rejection on the graduation rail, never a graduated handoff.

Three polymorphic surfaces carry every variation: `Distribution` over the `pymc` families, read in both the prior and likelihood roles off one vocabulary; `SamplerBackend` over the MCMC engine and its per-engine policy; the `ConvergenceBar` policy row folded against the `_RESIDUALS` dimension table, so a stricter bar is a tighter row, never a new gate. This run rides the `EvidenceScope.INFERENCE` weave — span, `boundary` fence, beartype guard, `@receipted` harvest — the same composed form `experiments/model.md#ASSET` and `graduation/handoff.md#GRADUATION` hold.

## [01]-[INDEX]

- [01]-[BAYESIAN]: the prior/likelihood/posterior graph on one `Inference` owner — the `Distribution` and `SamplerBackend` unions, the `arviz` diagnostic fold, and the graduation-rail convergence gate.

## [02]-[BAYESIAN]

- Owner: `Inference` — `InferenceSpec` is the frozen request; `InferenceReceipt.graduates` routes the measured-versus-ceiling ledger through the shared `graduation/handoff.md#GRADUATION` admission rail, the same gate the sibling solver, convex, and array-layout owners feed, never a parallel admission body.
- Cases: `Distribution` is one union read in both roles, each case carrying its canonical parameters as a typed tuple — never a stringly `dict[str, float]` drifting from the class signature; the union's own keyword constructor is the construction surface, no parallel factory family re-wraps the cases.
- Auto: PyMC owns the model lowering and the JAX/Numba handoff — this page never re-drives `pymc.sampling.jax`, the `nutpie.compile_pymc_model`/`sample` pair, or the raw `blackjax` kernel algebra, and the accelerated engines install only so PyMC's own dispatch resolves them, never as imports here. Sampling never retries: the posterior draw is the evidence, and worker-death handling stays the lane's.
- Output: `ConvergenceBar` folds against the `_RESIDUALS` table, so a new convergence dimension is one `_Residual` row plus one bar field; a `metropolis` trace carries no `diverging` sample stat — divergence counting is a gradient-sampler diagnostic — so the membership gate contributes `0` rather than a spurious `KeyError`, and a non-gradient sampler trivially clears the default bar.
- Growth: a new distribution is one `Distribution` case plus one `declare` arm usable in either role; a new sampler engine is one `SamplerBackend` case or one `external_nuts` name; a new convergence dimension is one `ConvergenceBar` field plus one `_Residual`; a new per-variable diagnostic is one `PosteriorSummary` field.

```python signature
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from typing import TYPE_CHECKING, Final, Literal, assert_never

import msgspec
import numpy as np
from beartype import beartype
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    from xarray import DataTree

# --- [TYPES] ----------------------------------------------------------------------------

type SamplerKind = Literal["nuts", "metropolis"]
type NutsSampler = Literal["numpyro", "blackjax", "nutpie"]
type NutsOption = str | int | float | bool  # an accelerator-lever value (`backend`, `chain_method`, device count)
type NutsOptions = tuple[tuple[str, NutsOption], ...]  # immutable sorted `(key, value)` pairs, frozen-union-hashable


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
        # `mu is None` keeps the case a latent prior; a supplied `mu` is the unconstrained real-valued latent node the likelihood
        # mean reads off, so the bounded/positive-support GLM cases route it through the canonical inverse-link (`invlogit` for a
        # `[0, 1]` rate, `exp` for a positive rate) rather than feeding a real node into a support that rejects it.
        import pymc

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
        # encoder-native projection the identity payload carries — the no-`enc_hook` `_ENCODER` rejects a raw `@tagged_union`.
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
    # `nuts_sampler_kwargs` ride as immutable sorted `(key, value)` pairs — a `dict` payload is unhashable and mutable,
    # defeating the `frozen=True` contract and letting options drift from the `canonical` content-key projection.
    external_nuts: tuple[NutsSampler, NutsOptions] = case()

    @property
    def engine(self) -> str:
        return self.pymc_native if self.tag == "pymc_native" else self.external_nuts[0]

    @property
    def canonical(self) -> tuple[str, NutsOptions]:
        # engine name plus its already-sorted option pairs, so an accelerator lever keys the study distinctly.
        match self:
            case SamplerBackend(tag="pymc_native", pymc_native=kind):
                return kind, ()
            case SamplerBackend(tag="external_nuts", external_nuts=(sampler, options)):
                return sampler, options
            case _ as unreachable:
                assert_never(unreachable)

    def draw(self, /, *, draws: int, tune: int, chains: int, seed: int) -> DataTree:
        # `step` binds the context-bound NUTS/Metropolis method; `nuts_sampler`+`nuts_sampler_kwargs` name the accelerated engine
        # and its lever (nutpie `backend`, numpyro `chain_method`).
        import pymc

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
    bar: ConvergenceBar = msgspec.field(default_factory=ConvergenceBar)


class InferenceSpec(Struct, frozen=True):
    observed: np.ndarray
    latents: tuple[Latent, ...]
    likelihood: Distribution
    mean_latent: str
    plan: SamplerPlan = msgspec.field(default_factory=SamplerPlan)


class StudyPayload(Struct, frozen=True):
    # every field is encoder-native — the unions lowered to their `canonical` projections — so the runtime content owner mints the
    # key, never a hand-rolled byte builder; container fields keep the struct GC-tracked, so the leaf-only `gc=False` opt-out does not apply.
    likelihood: tuple[str, tuple[float, ...]]
    latents: tuple[tuple[str, tuple[str, tuple[float, ...]]], ...]
    mean_latent: str  # the latent node the likelihood mean reads off; rewiring it re-shapes the graph, so it keys distinctly
    backend: tuple[str, NutsOptions]
    observed_dtype: str
    observed_shape: tuple[int, ...]
    observed_bytes: bytes


@dataclass(slots=True, frozen=True)
class _Residual:
    # a slots dataclass carries the extractor lambdas (never wire-decoded, so not a `msgspec.Struct`); the forward reference
    # to the later `InferenceReceipt` resolves lazily under PEP 749 deferred annotations.
    key: str
    measure: Callable[[InferenceReceipt], float]
    ceiling: Callable[[ConvergenceBar], float]


class PosteriorSummary(Struct, frozen=True):
    # one value object per latent name, never six parallel `dict[str, ...]` maps the residual extractors keep in stringly lockstep.
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
    p_eff: float  # arviz-1.x `ELPDData.p` effective-parameter count; never the removed `p_loo`
    pareto_k_max: float
    prior_sensitivity_max: float
    divergences: int
    draws: int
    bar: ConvergenceBar
    model_key: ContentKey

    @property
    def measured(self) -> dict[str, float]:
        return {row.key: row.measure(self) for row in _RESIDUALS}

    @property
    def ceiling(self) -> dict[str, float]:
        return {row.key: row.ceiling(self.bar) for row in _RESIDUALS}

    @property
    def converged(self) -> bool:
        ceiling = self.ceiling
        return all(value <= ceiling[key] for key, value in self.measured.items())

    @property
    def span_facts(self) -> dict[str, str | int | float | bool]:
        # bounded scalars only — the full per-variable `summaries` and `measured` ledger ride the receipt facts, never the span.
        return {
            "subject": self.subject(),
            "converged": self.converged,
            "draws": self.draws,
            "max_pareto_k": self.pareto_k_max,
            "model_key": self.model_key.hex,
        }

    def subject(self) -> str:
        return f"{self.likelihood}:{self.backend}"

    def contribute(self) -> Iterable[Receipt]:
        # native scalars only — no `str()` coerce where the deterministic renderer keeps types.
        facts: dict[str, object] = {
            "likelihood": self.likelihood,
            "backend": self.backend,
            "converged": self.converged,
            "ppc_mean": self.ppc_mean,
            "elpd": self.elpd,
            "p_eff": self.p_eff,
            "draws": self.draws,
            **self.measured,
        }
        return (Receipt.of("compute.inference", ("emitted", self.subject(), facts)),)

    def graduates(self) -> RuntimeRail[GraduationReceipt]:
        return GraduationReceipt.graduates("compute", HandoffAxis(uncertainty_law=self.subject()), self.model_key, self.measured, self.ceiling)


# --- [TABLES] ---------------------------------------------------------------------------

# each row pairs a residual key with its measured- and ceiling-extractors, so `measured` and `ceiling` fold one table rather than
# two near-identical dicts; the ess floor enters negated so the shared `measured <= ceiling` fold reads a max-deficit.
_RESIDUALS: Final[Block[_Residual]] = Block.of_seq([
    _Residual("max_rhat", lambda r: max(s.r_hat for s in r.summaries.values()), lambda b: b.rhat_ceiling),
    _Residual("neg_min_ess_bulk", lambda r: -min(s.ess_bulk for s in r.summaries.values()), lambda b: -b.ess_floor),
    _Residual("divergences", lambda r: float(r.divergences), lambda b: float(b.max_divergences)),
    _Residual("pareto_k_max", lambda r: r.pareto_k_max, lambda b: b.pareto_k_ceiling),
    _Residual("prior_sensitivity_max", lambda r: r.prior_sensitivity_max, lambda b: b.prior_sensitivity_ceiling),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _fit_kernel(spec: "InferenceSpec") -> "RuntimeRail[InferenceReceipt]":
    # module-level so the worker resolves it by import; the fence converts a sampler raise.
    return boundary(f"inference.{spec.plan.backend.engine}", lambda: Inference._fit(spec))


class Inference:
    @staticmethod
    async def run(spec: InferenceSpec, lane: LanePolicy) -> RuntimeRail[InferenceReceipt]:
        # weave owns span, fence, and the `@receipted` receipt harvest. Trait keys on the backend tag: the pytensor-C
        # native path releases the GIL (thread), an external_nuts engine is JAX-backed whose x64 flag is process-global
        # native state (process) — one fixed trait cannot serve both arms. A seeded draw re-runs identically, so the
        # worker-death retry default stands.
        engine = spec.plan.backend.engine
        trait = KernelTrait.HOSTILE if spec.plan.backend.tag == "external_nuts" else KernelTrait.RELEASING

        async def dispatch() -> RuntimeRail[InferenceReceipt]:
            return (await lane.offload(Kernel.of(_fit_kernel, trait), spec)).bind(lambda rail: rail)

        return await evidence_run(EvidenceScope.INFERENCE, f"inference.{engine}", dispatch)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _fit(spec: InferenceSpec) -> InferenceReceipt:
        import arviz
        import pymc

        plan, names = spec.plan, [lat.name for lat in spec.latents]
        with pymc.Model() as model:
            nodes = {lat.name: lat.prior.declare(lat.name) for lat in spec.latents}
            spec.likelihood.declare("observation", mu=nodes[spec.mean_latent], observed=spec.observed)
            trace = plan.backend.draw(draws=plan.draws, tune=plan.tune, chains=plan.chains, seed=plan.seed)
            pymc.compute_log_likelihood(trace, model=model)  # populate the group `arviz.loo`/`psense_summary` read
            ppc = pymc.sample_posterior_predictive(trace, model=model, var_names=["observation"], random_seed=plan.seed, return_inferencedata=True)
        summary = arviz.summary(trace, var_names=names, kind="all")
        hdi = arviz.hdi(trace, var_names=names, prob=plan.hdi_prob)
        loo = arviz.loo(trace, var_name="observation", pointwise=True)
        psense = arviz.psense_summary(trace)
        # `r_hat` column carries the underscore and the credible interval reads the `hdi` Dataset's `ci_bound` coordinate,
        # never the removed `hdi_3%`/`hdi_97%` summary columns.
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
        # key is `match`ed off the rail inside the already-fenced body, so a hash `Error` re-raises onto the `boundary`
        # rather than masking a fabricated empty key.
        match ContentIdentity.of("pymc-model", _study_payload(spec)):
            case Result(tag="ok", ok=model_key):
                pass
            case Result(tag="error", error=fault):
                raise RuntimeError(fault)
        return InferenceReceipt(
            likelihood=spec.likelihood.tag,
            backend=plan.backend.engine,
            summaries=summaries,
            ppc_mean=float(ppc.posterior_predictive["observation"].mean().to_numpy()),
            elpd=float(loo.elpd),
            p_eff=float(loo.p),
            pareto_k_max=float(np.asarray(loo.pareto_k).max()),
            prior_sensitivity_max=float(np.asarray(psense["prior"]).max()),
            divergences=int(trace.sample_stats["diverging"].to_numpy().sum()) if "diverging" in trace.sample_stats else 0,
            draws=plan.draws * plan.chains,
            bar=plan.bar,
            model_key=model_key,
        )


def _study_payload(spec: InferenceSpec) -> StudyPayload:
    # latents sort by name so a reorder does not key distinctly; the observed array contributes dtype/shape plus its contiguous byte view.
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
