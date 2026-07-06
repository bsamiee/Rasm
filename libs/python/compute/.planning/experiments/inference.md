# [PY_COMPUTE_INFERENCE]

The one classical Bayesian-inference owner over an explicit prior/likelihood/posterior graph. `Inference.run` builds a `pymc.Model` from a frozen request, draws the posterior with gradient MCMC across a backend axis, scores convergence and predictive fit with `arviz`, and graduates a typed posterior-evidence receipt through the one `uncertainty_law` admission rail — all inside one `inference.{engine}` span (the `EvidenceScope.INFERENCE` scope row) weaving the `boundary` fault fence over the `@beartype(conf=FAULT_CONF)`-guarded body and the weave's fenced `@receipted(REDACTION)` harvest, the same composed weave `experiments/model.md#ASSET` and `graduation/handoff.md#GRADUATION` hold. The owner is bounded at conjugate and GLM-class models over scalar latent nodes — one `summary.loc[name]` row and one scalar `hdi[name]` interval per latent name; a vector group-level latent the per-variable summary fold cannot key by a single name is out of scope, as are variational, normalizing-flow, and neural-posterior estimation. A posterior failing the `ConvergenceBar` is an admission rejection on the graduation rail, never a graduated handoff.

Three polymorphic surfaces carry every variation. `Distribution` is the single `@tagged_union` over the `pymc` distribution families, each case carrying the canonical parameters the `pymc` class names, read in both the prior and likelihood roles off one vocabulary. `SamplerBackend` is the `@tagged_union` discriminating the MCMC engine, each case carrying the per-engine policy `pymc.sample` threads — native step kind, or the accelerated-NUTS name plus its `nuts_sampler_kwargs`. The convergence gate is one `ConvergenceBar` policy row folded against one `_RESIDUALS` dimension table, so a stricter bar is a tighter row, never a new gate.

## [01]-[INDEX]

- [01]-[BAYESIAN]: the prior/likelihood/posterior graph, the polymorphic distribution union, the parameterized sampler-backend axis, the woven `arviz` diagnostic fold, the data-driven residual table, the traced/fault-fenced/`@receipted` `EvidenceScope.INFERENCE` weave, and the graduation-rail convergence gate on one `Inference` owner.

## [02]-[BAYESIAN]

- Owner: `Inference` is the one Bayesian owner over a prior/likelihood/posterior axis. `InferenceSpec` is the frozen request carrying the observed array, the `Latent` family (each a name plus a `Distribution` prior case), the observation `Distribution` likelihood case, the mean latent, and the `SamplerPlan`. `Inference.run` folds each `Distribution.declare` over a `pymc.Model` context, draws the posterior through `SamplerBackend.draw`, populates the `log_likelihood` group through `pymc.compute_log_likelihood`, draws the predictive check through `pymc.sample_posterior_predictive`, scores all four `arviz` reductions, mints the `model_key` through `ContentIdentity.of`, and returns a railed `InferenceReceipt`.
- Distribution union: `Distribution` is the ONE `@tagged_union` over the `pymc` distribution families catalogued in `compute/.api/pymc.md` continuous/discrete tables — `normal(mu, sigma)`, `half_normal(sigma)`, `beta(alpha, beta_)`, `gamma(alpha, beta_)`, `student_t(nu, mu, sigma)`, `uniform(lower, upper)`, `bernoulli(p)`, `poisson(mu)`, `binomial(n, p)` — each case carrying the canonical parameters as a typed tuple, type-checked per case rather than a stringly-typed `dict[str, float]` drifting from the class signature. The union's own keyword constructor (`Distribution(normal=(0.0, 1.0))`) is the construction surface; no parallel sibling factory family re-wraps each case. `Distribution.declare(name, *, mu, observed)` is the one total `match` projecting a case to its explicit `pymc` distribution-class construction, `observed=None` keeping a latent prior and the data array minting the observation likelihood. A supplied `mu` is the unconstrained real-valued latent node the likelihood mean reads off a fitted prior: the identity-link `normal`/`student_t` location takes the node directly, while the bounded/positive-support GLM cases route it through the canonical inverse-link the catalogued `pymc.math` owns — `pymc.math.invlogit(mu)` for the `bernoulli`/`binomial` `[0, 1]` rate, `pymc.math.exp(mu)` for the `poisson` positive rate — so a GLM mean rides a valid-support link rather than a real-valued node the `p`/`mu` support rejects. Prior and likelihood are the same union read in two roles, closed by `assert_never`. The union's second fold is `Distribution.canonical`, the one total `match` projecting each case to its `tuple[str, tuple[float, ...]]` tag-and-flattened-parameter pair so the identity payload carries the prior as a deterministic-encoder-native tuple rather than the raw union the no-`enc_hook` `_ENCODER` rejects.
- Sampler-backend axis: `SamplerBackend` is the `@tagged_union` discriminating the MCMC engine, parameterized over both the engine and its per-engine options. The `pymc_native(kind)` case runs `pymc.sample` with a context-bound `pymc.NUTS` or `pymc.Metropolis` step selected by the carried `SamplerKind`. The `external_nuts(sampler, options)` case routes the three accelerated NUTS engines through the one `pymc.sample(nuts_sampler=..., nuts_sampler_kwargs=...)` dispatch confirmed in `compute/.api/pymc.md` ENTRYPOINTS [01], the `options` `NutsOptions` payload carrying the `nutpie` `backend='jax'`/`'numba'` lever (`nutpie.md` ENTRYPOINTS [01]) or the `numpyro` `chain_method` (`numpyro.md` MCMC [01]) as immutable sorted `(key, value)` pairs so the engine choice and its accelerator knob ride one hashable case rather than a lost kwarg channel or a mutable `dict` defeating the `frozen=True` hash contract; `draw` rebuilds `dict(options)` only at the `pymc.sample` boundary. PyMC owns the model lowering and the JAX/Numba handoff, so the page never re-drives `pymc.sampling.jax`, the `nutpie.compile_pymc_model`/`sample` pair, or the raw `blackjax` kernel algebra. The `engine` projection reads the discriminated name for the receipt and the `boundary` subject; the `canonical` projection folds the case to the `(tag, params)` pair the identity payload carries — the `pymc_native` step kind or the `external_nuts` name plus its already-sorted accelerator pairs — so the backend choice keys the study distinctly without nesting the raw union in the payload. `arviz` reads the posterior and the `sample_stats` group regardless of which engine sampled, so the diagnostic gate is one fold across every backend.
- Woven diagnostic fold: one `_fit` pass stacks four `arviz` reductions over the fitted trace, then folds the per-variable rows into one `dict[str, PosteriorSummary]`. `PosteriorSummary` is the value object carrying one latent's `mean`/`sd`/`r_hat`/`ess_bulk`/`ess_tail`/`hdi`, so the receipt holds one per-variable summary map rather than six parallel `dict[str, ...]` fields keyed identically — the same output-parameterization discipline `experiments/model.md#ASSET` and `analysis/signal.md#DSP` apply to their evidence unions, carried here as a per-variable row. `arviz.summary(trace, var_names, kind="all")` yields the `mean`/`sd`/`r_hat`/`ess_bulk`/`ess_tail` columns in one frame (ENTRYPOINTS summary [06]) rather than four separate `rhat`/`ess` reductions; the `r_hat` column carries the underscore the `compute/.api/arviz.md` IMPLEMENTATION_LAW fixes, and the interval columns are ETI, so the credible interval reads off the separate `arviz.hdi`. `arviz.hdi(trace, var_names, prob=...)` returns a Dataset with the `ci_bound` coordinate taking `'lower'`/`'upper'` (interval [07]); the field reads `.sel(ci_bound=...)`, never the removed `hdi_3%`/`hdi_97%` columns. `arviz.loo(trace, var_name, pointwise=True)` returns the `ELPDData` whose arviz-1.x fields are `elpd`, `p`, and the pointwise `pareto_k` plus `good_k`/`warning` (PUBLIC_TYPES `ELPDData` [02]) — the page reads `elpd`/`p`/`pareto_k`, never the removed arviz-0.x `elpd_loo`/`p_loo`. `arviz.psense_summary(trace)` is the prior-robustness receipt (sensitivity [02]) — the prior-vs-likelihood power-scaling diagnosis table whose `prior` column folds to one `prior_sensitivity_max` scalar beside the convergence evidence, the column the `ConvergenceBar.prior_sensitivity_ceiling` row gates. `ppc_mean` reads the mean of the `posterior_predictive["observation"]` group off the standalone `ppc.posterior_predictive` child of the `sample_posterior_predictive` `DataTree` (its own tree under the `extend_inferencedata=False` default, not the fitted `trace`), accessed by the group attribute rather than `ppc["posterior_predictive"]`.
- Log-likelihood seam: `arviz.loo` and `arviz.psense_summary` read the `log_likelihood` group, which `pymc.sample` does NOT populate by default. `Inference._fit` calls `pymc.compute_log_likelihood(trace, model=model)` over the fitted trace (`compute/.api/pymc.md` ENTRYPOINTS [03]) so the PSIS-LOO rollup has the per-observation log-density it requires — never a hand-recomputed pointwise log-lik, and never a `loo` call against an unpopulated group that the runtime would reject into the `boundary`.
- Convergence bar: `ConvergenceBar` is the policy row carrying `rhat_ceiling`, `ess_floor`, `max_divergences`, `pareto_k_ceiling`, and `prior_sensitivity_ceiling`. `_RESIDUALS` is the `Block[_Residual]` dimension table pairing each residual key with its measured-extractor and its ceiling-extractor over the receipt, so `InferenceReceipt.measured` and `ceiling` each fold the one table rather than two hand-built near-identical dicts; the ess floor enters negated (`neg_min_ess_bulk = -min(ess)` against `-ess_floor`) so the one `measured <= ceiling` fold reads it as a max-deficit, the same negated-floor convention the `graduation/handoff.md#GRADUATION` `_clear` ceiling fold admits. A new convergence dimension is one `_Residual` plus one `ConvergenceBar` field; `converged` folds `measured` against `ceiling`. The `divergences` residual reads `trace.sample_stats["diverging"]` behind a `"diverging" in trace.sample_stats` membership gate so a random-walk `pymc_native="metropolis"` trace — which carries no `diverging` sample stat, the divergence count being a gradient-sampler (NUTS/HMC) diagnostic — contributes `0` rather than raising a spurious `KeyError` the `boundary` fence would mis-rail as a fault; a non-gradient sampler trivially clears the default `max_divergences=0`.
- Woven rail: `Inference.run(spec, lane)` is the one `async` entrypoint, composing `lane.offload(_fit_kernel, spec, modality=Modality.THREAD)` under the hub `evidence_run` weave — span from the `compute.inference` scope row, fault fence, and the fenced `@receipted(REDACTION)` harvest of the `InferenceReceipt` composed; the former page-local `_TRACER`/`_REDACTION` mints, inline span open, and `_emit` aspect are the deleted forms. Sampling never retries — the posterior draw is the evidence, and worker-death handling stays the lane's.
- Entry: `Inference.run(spec)` returns `RuntimeRail[InferenceReceipt]` through the inline span-then-fence weave above. `InferenceReceipt.graduates` routes the measured-versus-ceiling ledger through the one `GraduationReceipt.graduates("compute", HandoffAxis(uncertainty_law=subject), model_key, measured, ceiling)` admission rail from `graduation/handoff.md#GRADUATION`, so a non-converged posterior is the `Error(BoundaryFault)` the shared `_admit` fold returns and a converged one is the admitted `GraduationReceipt` — the same residual-over-ceiling gate the sibling solver, convex, and array-layout owners feed, never a parallel admission body. `InferenceReceipt.contribute` returns the `tuple[Receipt, ...]` the `ReceiptContributor` port streams, each row minted through the two-argument `Receipt.of(owner, evidence)` contract over an `("emitted", subject, facts)` triple, never a four-positional call.
- Model key: `model_key` is `ContentIdentity.of("pymc-model", _study_payload(spec))` over the canonical study payload, resting on the `of` `CANONICAL_POLICY` default rather than a fresh `IdentityPolicy()` allocation, and threaded as the railed `RuntimeRail[ContentKey]` the owner returns rather than assigned raw — the same default-policy threading `numerics/jit.md#JIT` holds. `StudyPayload` is the canonical `msgspec.Struct` the `canonical` `IdentitySource` modality folds through the one cached deterministic `_ENCODER`, but every field is msgspec-native because the runtime `_ENCODER` carries no `enc_hook` and CANNOT serialize an `expression.@tagged_union`: a raw `Distribution`/`SamplerBackend`/`Latent.prior` field nested in the payload is the unencodable shape that would `EncodeError` on every key. The payload instead carries each union's `canonical` projection — the `tuple[str, tuple[float, ...]]` tag-and-flattened-parameter pair the union folds itself to — so the sorted `(name, Distribution.canonical)` latent rows, the likelihood `Distribution.canonical`, the `mean_latent` graph-wiring tag, the backend `SamplerBackend.canonical`, and the observed array's dtype/shape/contiguous-bytes view are all native tuples/bytes/scalars the deterministic encoder lowers, and the runtime content owner mints the key rather than a hand-rolled `msgspec.json.encode` plus `b"\x00".join` reinvention. The `mean_latent` field is load-bearing: it names the latent node the likelihood mean reads off (`mu=nodes[spec.mean_latent]`), so two specs identical in latents, likelihood, backend, and data but rewiring the likelihood mean to a different latent are genuinely different model graphs and key distinctly rather than colliding on one content key. Two studies with the same latent names but different data, prior parameters, mean wiring, backend, or accelerator options key distinctly.
- Packages: `pymc` (`Model`, `sample` over the `step`/`nuts_sampler`/`nuts_sampler_kwargs`/`return_inferencedata` axis, `compute_log_likelihood`, `sample_posterior_predictive`, `NUTS`, `Metropolis`, the distribution classes, `math.invlogit`/`math.exp` the GLM inverse-links), `numpyro`/`blackjax`/`nutpie` (the accelerated NUTS engines `pymc.sample(nuts_sampler=...)` dispatches to, installed only so PyMC's own dispatch resolves them, never imported here), `arviz` (`summary`, `hdi`, `loo`, `psense_summary`, `ELPDData`), `xarray` (`DataTree` — the posterior container), `numpy`, `expression` (`tagged_union`/`case`/`tag`, `Ok`/`Error` the `run` rail match, `Block` the residual-table fold; `Result` underlies `RuntimeRail`), `msgspec` (`Struct`, `field`), `beartype` (`@beartype(conf=FAULT_CONF)` fencing the `_fit` body), hub (`EvidenceScope`/`evidence_run` — the span/fence/harvest weave), `graduation/handoff.md#GRADUATION` (`GraduationReceipt`, `HandoffAxis`), runtime (`RuntimeRail`, `boundary`, `FAULT_CONF`; `ContentIdentity`/`ContentKey` over the `CANONICAL_POLICY` default; `Receipt`/`ReceiptContributor`/`Redaction`/`receipted`, the `Signals.emit` egress owned by the `@receipted` aspect rather than imported here).
- Growth: a new distribution is one `Distribution` case plus one `declare` arm usable in either role; a new sampler engine is one `SamplerBackend` case or one `external_nuts` name; a new convergence dimension is one `ConvergenceBar` field plus one `_Residual`; a new per-variable diagnostic is one `PosteriorSummary` field; the LOO and prior-sensitivity scalars are `InferenceReceipt` fields; zero new surface.

```python signature
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from typing import TYPE_CHECKING, Final, Literal, assert_never

import msgspec
import numpy as np
from beartype import beartype
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt

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
        # one total projection to the explicit `pymc` class. `mu is None` keeps the case a latent prior
        # built from its literal in-support parameters; a supplied `mu` is the unconstrained real-valued
        # latent node the likelihood mean reads off, so the bounded/positive-support GLM cases route it
        # through the canonical inverse-link (`pymc.math.invlogit` for a `[0, 1]` rate, `pymc.math.exp`
        # for a positive rate) rather than feeding a real-valued node straight into a `p`/`mu` the
        # support rejects, while the identity-link `normal`/`student_t` location takes the node directly.
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
        # the encoder-native projection the identity payload carries: the tag plus the case
        # parameters flattened to one float tuple, so a `Distribution` keys through the no-`enc_hook`
        # `_ENCODER` as native data rather than the raw `@tagged_union` the encoder rejects.
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
    # the `nuts_sampler_kwargs` ride as immutable sorted `(key, value)` pairs, never a `dict`: a `dict`
    # payload is unhashable and mutable, defeating the `frozen=True` hash/immutability contract and
    # letting the post-construction options drift from the `canonical` content-key projection.
    external_nuts: tuple[NutsSampler, NutsOptions] = case()

    @property
    def engine(self) -> str:
        return self.pymc_native if self.tag == "pymc_native" else self.external_nuts[0]

    @property
    def canonical(self) -> tuple[str, NutsOptions]:
        # the encoder-native projection: the engine name plus its already-sorted `(key, value)` option
        # pairs, so an accelerator lever (`backend`/`chain_method`) keys the study distinctly and the
        # immutable pair tuple lowers through the no-`enc_hook` `_ENCODER` as native data with no `repr` coerce.
        match self:
            case SamplerBackend(tag="pymc_native", pymc_native=kind):
                return kind, ()
            case SamplerBackend(tag="external_nuts", external_nuts=(sampler, options)):
                return sampler, options
            case _ as unreachable:
                assert_never(unreachable)

    def draw(self, /, *, draws: int, tune: int, chains: int, seed: int) -> DataTree:
        # the engine channels `pymc.sample` exposes: `step` binds the context-bound NUTS/Metropolis
        # method, `nuts_sampler`+`nuts_sampler_kwargs` name the accelerated engine and its accelerator
        # lever (nutpie `backend`, numpyro `chain_method`); PyMC owns the JAX/Numba handoff.
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
    # the canonical `IdentitySource.canonical` carrier the one content owner folds through its cached
    # deterministic `_ENCODER` — every field is encoder-native (the unions lowered to their `canonical`
    # `(tag, params)` projections, never the raw `@tagged_union` the no-`enc_hook` encoder rejects), so
    # the runtime mints the key off this shape rather than a hand-rolled `b"\x00".join` byte builder.
    # Container fields (tuples/bytes) keep the struct GC-tracked, never the leaf-only `gc=False` opt-out.
    likelihood: tuple[str, tuple[float, ...]]
    latents: tuple[tuple[str, tuple[str, tuple[float, ...]]], ...]
    mean_latent: str  # the latent node the likelihood mean reads off; rewiring it re-shapes the graph, so it keys distinctly
    backend: tuple[str, NutsOptions]
    observed_dtype: str
    observed_shape: tuple[int, ...]
    observed_bytes: bytes


@dataclass(slots=True, frozen=True)
class _Residual:
    # one residual dimension: its key, the measured-extractor over the receipt, and the ceiling-extractor
    # over the bar, so `measured` and `ceiling` fold this one row family rather than two parallel dicts.
    # A slots dataclass carries the extractor lambdas (never wire-decoded, so not a `msgspec.Struct`); the
    # forward reference to the later `InferenceReceipt` resolves lazily under PEP 749 deferred annotations.
    key: str
    measure: Callable[[InferenceReceipt], float]
    ceiling: Callable[[ConvergenceBar], float]


class PosteriorSummary(Struct, frozen=True):
    # the one per-variable posterior row `arviz.summary` returns, plus the `arviz.hdi` credible
    # interval — the central tendency, spread, and per-variable convergence diagnostics keyed by one
    # latent name, so a six-field summary is one value object rather than six parallel `dict[str, ...]`
    # maps the residual extractors would have to keep in stringly-keyed lockstep. No `gc=False`: the
    # `hdi` `tuple` is a container field, so the leaf-only opt-out the sibling `gc=False` rows take
    # does not apply, the same container-aware split `experiments/model.md#ASSET` holds.
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
        # the bounded-scalar source the `EvidenceScope.INFERENCE`-scoped `inference.{engine}` span reads — exactly the set
        # `Span.set_attributes` admits; the full per-variable `summaries` and `measured` ledger ride
        # the receipt facts only, the same span/receipt split the sibling owners hold.
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
        # the convergence evidence rides the receipt stream as native scalars the `EventDict`
        # `dict[str, object]` slots and the `enc_hook=repr` renderer serialize without a `str()` coerce:
        # the discriminants, the verdict, the LOO/sensitivity scalars, and the measured residual ledger.
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


# --- [SERVICES] -------------------------------------------------------------------------

# the inference family modality row: PyMC sampling is heavy native/compiled work riding the
# runtime THREAD band (the external NUTS backends manage their own device state); policy DATA.
_MODALITY: Final[Modality] = Modality.THREAD


# --- [TABLES] ---------------------------------------------------------------------------

# the one residual-dimension catalog: each row pairs a residual key with its measured-extractor over the
# receipt and its ceiling-extractor over the bar, so `measured` and `ceiling` fold one table rather than
# two near-identical hand-built dicts. A new convergence dimension is one row plus one `ConvergenceBar`
# field; the ess floor enters negated so the shared `measured <= ceiling` fold reads a max-deficit.
_RESIDUALS: Final[Block[_Residual]] = Block.of_seq([
    _Residual("max_rhat", lambda r: max(s.r_hat for s in r.summaries.values()), lambda b: b.rhat_ceiling),
    _Residual("neg_min_ess_bulk", lambda r: -min(s.ess_bulk for s in r.summaries.values()), lambda b: -b.ess_floor),
    _Residual("divergences", lambda r: float(r.divergences), lambda b: float(b.max_divergences)),
    _Residual("pareto_k_max", lambda r: r.pareto_k_max, lambda b: b.pareto_k_ceiling),
    _Residual("prior_sensitivity_max", lambda r: r.prior_sensitivity_max, lambda b: b.prior_sensitivity_ceiling),
])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _fit_kernel(spec: "InferenceSpec") -> "RuntimeRail[InferenceReceipt]":
    # the module-level measured kernel — resolvable by import in the worker; the fence converts a
    # sampler raise and the weave harvests the resolved contributor.
    return boundary(f"inference.{spec.plan.backend.engine}", lambda: Inference._fit(spec))


class Inference:
    @staticmethod
    async def run(spec: InferenceSpec, lane: LanePolicy) -> RuntimeRail[InferenceReceipt]:
        # the sampling offloads on the family modality row under the hub weave — span, fault fence,
        # and the fenced `@receipted(REDACTION)` harvest composed; the module-level `_fit_kernel`
        # crosses the lane, so a `pymc`/`arviz` raise converts at the fence and the receipt streams
        # by composition, never a page-local tracer or `_emit` aspect.
        engine = spec.plan.backend.engine

        async def dispatch() -> RuntimeRail[InferenceReceipt]:
            return (await lane.offload(_fit_kernel, spec, modality=_MODALITY)).bind(lambda rail: rail)

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
        # one per-variable `PosteriorSummary` row off the `summary` frame and the separate `hdi` Dataset:
        # the `r_hat` column carries the arviz-1.x underscore and the interval reads the `ci_bound` coord,
        # never the removed `hdi_3%`/`hdi_97%` columns.
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
        # the impure draw/score above and the pure content fold ride this one fence: `ContentIdentity.of`
        # returns `RuntimeRail[ContentKey]`, so the key is `match`ed off the rail inside the already-fenced
        # body and a hash `Error` re-raises onto the `boundary` rather than masking a fabricated empty key.
        match ContentIdentity.of("pymc-model", _study_payload(spec)):
            case Ok(model_key):
                pass
            case Error(fault):
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
    # each union lowers to its `canonical` `(tag, params)` projection so the payload is fully
    # encoder-native; latents sort by name so reorder does not key distinctly, and the observed
    # array contributes its dtype/shape plus the contiguous byte view the same way the
    # `numerics/array.md#PAYLOAD`/`optimization/convex.md#CONVEX` identity buffers fold their arrays.
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
