# [PY_COMPUTE_INFERENCE]

The one classical Bayesian-inference owner over an explicit prior/likelihood/posterior graph. `Inference.run` builds a `pymc.Model` from a frozen request, draws the posterior with gradient MCMC across a backend axis, scores convergence and predictive fit with `arviz`, and graduates a typed posterior-evidence receipt through the one `uncertainty_law` admission rail — all inside one `content.inference.{engine}` span weaving the `boundary` fault fence over the `@beartype(conf=FAULT_CONF)`-guarded body and the `@receipted(_REDACTION)` emit aspect, the same `_traced`-shaped weave `experiments/model.md#ASSET` and `graduation/handoff.md#GRADUATION` hold. The owner is bounded at conjugate, hierarchical, and GLM-class models; variational, normalizing-flow, and neural-posterior estimation never enter it. A posterior failing the `ConvergenceBar` is an admission rejection on the graduation rail, never a graduated handoff.

Three polymorphic surfaces carry every variation. `Distribution` is the single `@tagged_union` over the `pymc` distribution families, each case carrying the canonical parameters the `pymc` class names, read in both the prior and likelihood roles off one vocabulary. `SamplerBackend` is the `@tagged_union` discriminating the MCMC engine, each case carrying the per-engine policy `pymc.sample` threads — native step kind, or the accelerated-NUTS name plus its `nuts_sampler_kwargs`. The convergence gate is one `ConvergenceBar` policy row folded against one `_RESIDUALS` dimension table, so a stricter bar is a tighter row, never a new gate.

## [01]-[INDEX]

- [01]-[BAYESIAN]: the prior/likelihood/posterior graph, the polymorphic distribution union, the parameterized sampler-backend axis, the woven `arviz` diagnostic fold, the data-driven residual table, the traced/fault-fenced/`@receipted` `content.inference` weave, and the graduation-rail convergence gate on one `Inference` owner.

## [02]-[BAYESIAN]

- Owner: `Inference` is the one Bayesian owner over a prior/likelihood/posterior axis. `InferenceSpec` is the frozen request carrying the observed array, the `Latent` family (each a name plus a `Distribution` prior case), the observation `Distribution` likelihood case, the mean latent, and the `SamplerPlan`. `Inference.run` folds each `Distribution.declare` over a `pymc.Model` context, draws the posterior through `SamplerBackend.draw`, populates the `log_likelihood` group through `pymc.compute_log_likelihood`, draws the predictive check through `pymc.sample_posterior_predictive`, scores all four `arviz` reductions, mints the `model_key` through `ContentIdentity.of`, and returns a railed `InferenceReceipt`.
- Distribution union: `Distribution` is the ONE `@tagged_union` over the `pymc` distribution families catalogued in `compute/.api/pymc.md` continuous/discrete tables — `normal(mu, sigma)`, `half_normal(sigma)`, `beta(alpha, beta_)`, `gamma(alpha, beta_)`, `student_t(nu, mu, sigma)`, `uniform(lower, upper)`, `bernoulli(p)`, `poisson(mu)`, `binomial(n, p)` — each case carrying the canonical parameters as a typed tuple, type-checked per case rather than a stringly-typed `dict[str, float]` drifting from the class signature. The union's own keyword constructor (`Distribution(normal=(0.0, 1.0))`) is the construction surface; no parallel sibling factory family re-wraps each case. `Distribution.declare(name, *, mu, observed)` is the one total `match` projecting a case to its explicit `pymc` distribution-class construction, `observed=None` keeping a latent prior and the data array minting the observation likelihood. A supplied `mu` is the unconstrained real-valued latent node the likelihood mean reads off a fitted prior: the identity-link `normal`/`student_t` location takes the node directly, while the bounded/positive-support GLM cases route it through the canonical inverse-link the catalogued `pymc.math` owns — `pymc.math.invlogit(mu)` for the `bernoulli`/`binomial` `[0, 1]` rate, `pymc.math.exp(mu)` for the `poisson` positive rate — so a GLM mean rides a valid-support link rather than a real-valued node the `p`/`mu` support rejects. Prior and likelihood are the same union read in two roles, closed by `assert_never`. The union's second fold is `Distribution.canonical`, the one total `match` projecting each case to its `tuple[str, tuple[float, ...]]` tag-and-flattened-parameter pair so the identity payload carries the prior as a deterministic-encoder-native tuple rather than the raw union the no-`enc_hook` `_ENCODER` rejects.
- Sampler-backend axis: `SamplerBackend` is the `@tagged_union` discriminating the MCMC engine, parameterized over both the engine and its per-engine options. The `pymc_native(kind)` case runs `pymc.sample` with a context-bound `pymc.NUTS` or `pymc.Metropolis` step selected by the carried `SamplerKind`. The `external_nuts(sampler, options)` case routes the three accelerated NUTS engines through the one `pymc.sample(nuts_sampler=..., nuts_sampler_kwargs=...)` dispatch confirmed in `compute/.api/pymc.md` ENTRYPOINTS [01], the `options` `NutsOptions` payload carrying the `nutpie` `backend='jax'`/`'numba'` lever (`nutpie.md` ENTRYPOINTS [01]) or the `numpyro` `chain_method` (`numpyro.md` MCMC [01]) as immutable sorted `(key, value)` pairs so the engine choice and its accelerator knob ride one hashable case rather than a lost kwarg channel or a mutable `dict` defeating the `frozen=True` hash contract; `draw` rebuilds `dict(options)` only at the `pymc.sample` boundary. PyMC owns the model lowering and the JAX/Numba handoff, so the page never re-drives `pymc.sampling.jax`, the `nutpie.compile_pymc_model`/`sample` pair, or the raw `blackjax` kernel algebra. The `engine` projection reads the discriminated name for the receipt and the `boundary` subject; the `canonical` projection folds the case to the `(tag, params)` pair the identity payload carries — the `pymc_native` step kind or the `external_nuts` name plus its already-sorted accelerator pairs — so the backend choice keys the study distinctly without nesting the raw union in the payload. `arviz` reads the posterior and the `sample_stats` group regardless of which engine sampled, so the diagnostic gate is one fold across every backend.
- Woven diagnostic fold: one `_fit` pass stacks four `arviz` reductions over the fitted trace, then folds the per-variable rows into one `dict[str, PosteriorSummary]`. `PosteriorSummary` is the value object carrying one latent's `mean`/`sd`/`r_hat`/`ess_bulk`/`ess_tail`/`hdi`, so the receipt holds one per-variable summary map rather than six parallel `dict[str, ...]` fields keyed identically — the same output-parameterization discipline `experiments/model.md#ASSET` and `analysis/signal.md#SIGNAL` apply to their evidence unions, carried here as a per-variable row. `arviz.summary(trace, var_names, kind="all")` yields the `mean`/`sd`/`r_hat`/`ess_bulk`/`ess_tail` columns in one frame (ENTRYPOINTS summary [06]) rather than four separate `rhat`/`ess` reductions; the `r_hat` column carries the underscore the `compute/.api/arviz.md` IMPLEMENTATION_LAW fixes, and the interval columns are ETI, so the credible interval reads off the separate `arviz.hdi`. `arviz.hdi(trace, var_names, prob=...)` returns a Dataset with the `ci_bound` coordinate taking `'lower'`/`'upper'` (interval [07]); the field reads `.sel(ci_bound=...)`, never the removed `hdi_3%`/`hdi_97%` columns. `arviz.loo(trace, var_name, pointwise=True)` returns the `ELPDData` whose arviz-1.x fields are `elpd`, `p`, and the pointwise `pareto_k` plus `good_k`/`warning` (PUBLIC_TYPES `ELPDData` [02]) — the page reads `elpd`/`p`/`pareto_k`, never the removed arviz-0.x `elpd_loo`/`p_loo`. `arviz.psense_summary(trace)` is the prior-robustness receipt (sensitivity [02]), folding the power-scaling prior/likelihood sensitivity into one `prior_sensitivity_max` scalar beside the convergence evidence. `ppc_mean` reads the mean of the `posterior_predictive["observation"]` group accessed by the `trace.posterior_predictive` child, never `trace["posterior_predictive"]`.
- Log-likelihood seam: `arviz.loo` and `arviz.psense_summary` read the `log_likelihood` group, which `pymc.sample` does NOT populate by default. `Inference._fit` calls `pymc.compute_log_likelihood(trace, model=model)` over the fitted trace (`compute/.api/pymc.md` ENTRYPOINTS [03]) so the PSIS-LOO rollup has the per-observation log-density it requires — never a hand-recomputed pointwise log-lik, and never a `loo` call against an unpopulated group that the runtime would reject into the `boundary`.
- Convergence bar: `ConvergenceBar` is the policy row carrying `rhat_ceiling`, `ess_floor`, `max_divergences`, `pareto_k_ceiling`, and `prior_sensitivity_ceiling`. `_RESIDUALS` is the `Block[_Residual]` dimension table pairing each residual key with its measured-extractor and its ceiling-extractor over the receipt, so `InferenceReceipt.measured` and `ceiling` each fold the one table rather than two hand-built near-identical dicts; the ess floor enters negated (`neg_min_ess_bulk = -min(ess)` against `-ess_floor`) so the one `measured <= ceiling` fold reads it as a max-deficit, the same negated-floor convention the `graduation/handoff.md#GRADUATION` `_clear` ceiling fold admits. A new convergence dimension is one `_Residual` plus one `ConvergenceBar` field; `converged` folds `measured` against `ceiling`.
- Woven rail: `Inference.run` is the one entrypoint, so it inlines the span-then-fence weave the multi-entry siblings `experiments/model.md#ASSET` and `graduation/handoff.md#GRADUATION` factor into a shared `_traced` helper — a single-caller `_traced` method here would be the thin-indirection deleted form. `run` opens `_TRACER.start_as_current_span(f"content.inference.{engine}")`, runs `boundary(f"inference.{engine}", lambda: Inference._emit(Inference._fit(spec)))` inside it, and on the `Ok` arm sets `Status(StatusCode.OK)` plus the bounded `InferenceReceipt.span_facts` scalars behind the `is_recording()` gate the sibling `experiments/study.md#STUDY` and `graduation/handoff.md#GRADUATION` owners hold, so a no-op span pays no attribute build and the full `summaries`/`measured` ledger rides the receipt facts only. `_fit` is the one `@beartype(conf=FAULT_CONF)`-fenced body returning a plain `InferenceReceipt` (never a `RuntimeRail`), so a `pymc` sampler raise, an `arviz` diagnostic failure, or an `arviz.loo` call against an unpopulated `log_likelihood` group folds onto the rail through the `boundary` fence rather than escaping, and the page never re-wraps a rail-returning thunk into `RuntimeRail[RuntimeRail[InferenceReceipt]]` then unwinds it through an identity `.bind`. The `model_key` is `match`ed off `ContentIdentity.of`'s `RuntimeRail[ContentKey]` inside the already-fenced body, so a hash `Error` re-raises onto the `boundary` rather than masking the key behind a fabricated empty-key `default_value` fallback, and the impure `pymc`/`arviz` raise and the pure content-key fold ride the one fence rather than a flattened double rail. `@receipted(_REDACTION)` harvests the receipt's `contribute` stream on exit, so the body threads no inline `Signals.emit`; the fault fence's `_convert` already records the cause and sets `Status(StatusCode.ERROR)` on the active span, so the page never re-annotates a status the conversion owns.
- Entry: `Inference.run(spec)` returns `RuntimeRail[InferenceReceipt]` through the inline span-then-fence weave above. `InferenceReceipt.graduates` routes the measured-versus-ceiling ledger through the one `GraduationReceipt.graduates("compute", HandoffAxis(uncertainty_law=subject), model_key, measured, ceiling)` admission rail from `graduation/handoff.md#GRADUATION`, so a non-converged posterior is the `Error(BoundaryFault)` the shared `_admit` fold returns and a converged one is the admitted `GraduationReceipt` — the same residual-over-ceiling gate the sibling solver, convex, and array-layout owners feed, never a parallel admission body. `InferenceReceipt.contribute` returns the `tuple[Receipt, ...]` the `ReceiptContributor` port streams, each row minted through the two-argument `Receipt.of(owner, evidence)` contract over an `("emitted", subject, facts)` triple, never a four-positional call.
- Model key: `model_key` is `ContentIdentity.of("pymc-model", _study_payload(spec), IdentityPolicy())` over the canonical study payload, threaded as the railed `RuntimeRail[ContentKey]` the owner returns rather than assigned raw. `StudyPayload` is the canonical `msgspec.Struct` the `canonical` `IdentitySource` modality folds through the one cached deterministic `_ENCODER`, but every field is msgspec-native because the runtime `_ENCODER` carries no `enc_hook` and CANNOT serialize an `expression.@tagged_union`: a raw `Distribution`/`SamplerBackend`/`Latent.prior` field nested in the payload is the unencodable shape that would `EncodeError` on every key. The payload instead carries each union's `canonical` projection — the `tuple[str, tuple[float, ...]]` tag-and-flattened-parameter pair the union folds itself to — so the sorted `(name, Distribution.canonical)` latent rows, the likelihood `Distribution.canonical`, the backend `SamplerBackend.canonical`, and the observed array's dtype/shape/contiguous-bytes view are all native tuples/bytes the deterministic encoder lowers, and the runtime content owner mints the key rather than a hand-rolled `msgspec.json.encode` plus `b"\x00".join` reinvention. Two studies with the same latent names but different data, prior parameters, backend, or accelerator options key distinctly.
- Packages: `pymc` (`Model`, `sample` over the `step`/`nuts_sampler`/`nuts_sampler_kwargs`/`return_inferencedata` axis, `compute_log_likelihood`, `sample_posterior_predictive`, `NUTS`, `Metropolis`, the distribution classes, `math.invlogit`/`math.exp` the GLM inverse-links), `numpyro`/`blackjax`/`nutpie` (the accelerated NUTS engines `pymc.sample(nuts_sampler=...)` dispatches to, installed only so PyMC's own dispatch resolves them, never imported here), `arviz` (`summary`, `hdi`, `loo`, `psense_summary`, `ELPDData`), `xarray` (`DataTree` — the posterior container), `numpy`, `expression` (`tagged_union`/`case`/`tag`, `Ok`/`Error` the `run` rail match, `Block` the residual-table fold; `Result` underlies `RuntimeRail`), `msgspec` (`Struct`, `field`), `beartype` (`@beartype(conf=FAULT_CONF)` fencing the `_fit` body), `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span`/`Span.set_attributes`/`set_status`/`Status`/`StatusCode` the one `content.inference` span), `graduation/handoff.md#GRADUATION` (`GraduationReceipt`, `HandoffAxis`), runtime (`RuntimeRail`, `boundary`, `FAULT_CONF`; `ContentIdentity`/`ContentKey`/`IdentityPolicy`; `Receipt`/`ReceiptContributor`/`Redaction`/`receipted`/`Signals`).
- Growth: a new distribution is one `Distribution` case plus one `declare` arm usable in either role; a new sampler engine is one `SamplerBackend` case or one `external_nuts` name; a new convergence dimension is one `ConvergenceBar` field plus one `_Residual`; a new per-variable diagnostic is one `PosteriorSummary` field; the LOO and prior-sensitivity scalars are `InferenceReceipt` fields; zero new surface.
- Boundary: classical Bayesian inference only — conjugate, hierarchical, and GLM-class models via gradient MCMC. Variational, normalizing-flow, neural-posterior estimation, and any deep generative model are out of scope. Inference is offline evidence; production uncertainty propagation stays in the C# quantity owners, reached only through the `uncertainty_law` graduation row. The deleted forms are a stringly-typed distribution-name map plus an untyped param bag, sibling factory families re-wrapping each `@tagged_union` case, an `external_nuts` arm that drops the per-engine `nuts_sampler_kwargs` channel, a mutable `dict[str, object]` `external_nuts` options payload defeating the `frozen=True` union's hash/immutability contract and letting the post-construction kwargs drift from the `canonical` content-key projection where the immutable sorted `NutsOptions` `(key, value)` pairs key the study and `draw` rebuilds `dict(options)` only at the `pymc.sample` boundary, three near-identical accelerated-engine draw arms re-driving `pymc.sampling.jax`/`nutpie`/`blackjax`, the arviz-0.x `elpd_loo`/`p_loo`/`hdi_3%` field reads the arviz-1.x catalog removed, an `arviz.loo` call against an unpopulated `log_likelihood` group, two hand-built `measured`/`ceiling` dicts where the `_RESIDUALS` table folds them, six parallel `dict[str, float]`/`dict[str, tuple]` per-variable fields where one `dict[str, PosteriorSummary]` holds the row, a `gc=False` `PosteriorSummary` opting a `hdi`-tuple-carrying struct out of GC tracking against the container-free-leaf-only opt-out, a `Receipt.of` four-positional call against the two-argument contract, a `contribute` returning one `Receipt` against the `Iterable[Receipt]` port, a raw `ContentKey` assignment off the bare rail-typed return of `ContentIdentity.of` or a fabricated empty-key `default_value` fallback where the in-fence rail `match` re-raises an `Error` onto the `boundary`, a raw `Distribution`/`SamplerBackend`/`Latent.prior` `@tagged_union` field nested in the `canonical` `StudyPayload` where the no-`enc_hook` `_ENCODER` would `EncodeError` (the payload carries each union's native `canonical` `(tag, params)` projection instead), a bare untraced `boundary(...)` lambda where the sibling owners weave one `content.inference` span, a `_fit` returning `RuntimeRail[InferenceReceipt]` the fence re-wraps to `RuntimeRail[RuntimeRail[...]]` then unwinds through an identity `.bind(lambda inner: inner)`, an unfenced `_fit` body where `@beartype(conf=FAULT_CONF)` raises the canonical violation the `api` row catches, and an inline `Signals.emit` where `@receipted(_REDACTION)` owns egress. `pymc` pulls `pytensor` to `numba` to `llvmlite` (no cp315 wheel), `arviz` pulls `scipy` (no cp315 wheel), and the `numpyro`/`nutpie`/`blackjax` engines ride the same gated `python_version<'3.15'` jaxlib/numba companion band, so every body is authored against the documented API on the marker floor.

```python signature
from collections.abc import Callable
from dataclasses import dataclass
from typing import TYPE_CHECKING, Final, Literal, assert_never

import msgspec
import numpy as np
from beartype import beartype
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, Signals, receipted

if TYPE_CHECKING:
    from xarray import DataTree

# --- [TYPES] ----------------------------------------------------------------------------

type SamplerKind = Literal["nuts", "metropolis"]
type NutsSampler = Literal["numpyro", "blackjax", "nutpie"]
type NutsOption = str | int | float | bool  # an accelerator-lever value (`backend`, `chain_method`, device count)
type NutsOptions = tuple[tuple[str, NutsOption], ...]  # immutable sorted `(key, value)` pairs, frozen-union-hashable


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
            case Distribution(tag="normal", normal=p) | Distribution(tag="beta", beta=p) | Distribution(tag="gamma", gamma=p) | Distribution(tag="uniform", uniform=p) | Distribution(tag="student_t", student_t=p):
                return self.tag, p
            case Distribution(tag="binomial", binomial=(n, p)):
                return self.tag, (float(n), p)
            case Distribution(tag="half_normal", half_normal=s) | Distribution(tag="bernoulli", bernoulli=s) | Distribution(tag="poisson", poisson=s):
                return self.tag, (s,)
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
                return pymc.sample(
                    draws=draws, tune=tune, chains=chains,
                    random_seed=seed, step=step, return_inferencedata=True,
                )
            case SamplerBackend(tag="external_nuts", external_nuts=(sampler, options)):
                return pymc.sample(
                    draws=draws, tune=tune, chains=chains,
                    random_seed=seed, nuts_sampler=sampler, nuts_sampler_kwargs=dict(options) or None,
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
        # the bounded-scalar source the `content.inference` span reads — exactly the set
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

    def contribute(self) -> tuple[Receipt, ...]:
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
        return GraduationReceipt.graduates(
            "compute", HandoffAxis(uncertainty_law=self.subject()), self.model_key, self.measured, self.ceiling
        )


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final = trace.get_tracer("compute.inference")
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # posterior diagnostics carry no secret field


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


class Inference:
    @staticmethod
    def run(spec: InferenceSpec) -> RuntimeRail[InferenceReceipt]:
        # one woven rail: the `boundary` fence wraps the `@beartype(conf=FAULT_CONF)`-guarded `_fit`
        # inside the one span, so a `pymc`/`arviz` raise folds onto the rail rather than escaping; the
        # `Ok` arm sets OK plus the convergence attributes and `@receipted` harvests the stream on exit.
        engine = spec.plan.backend.engine
        with _TRACER.start_as_current_span(f"content.inference.{engine}") as span:
            rail = boundary(f"inference.{engine}", lambda: Inference._emit(Inference._fit(spec)))
            match rail:
                case Ok(receipt):
                    if span.is_recording():
                        span.set_attributes(receipt.span_facts)
                    span.set_status(Status(StatusCode.OK))
                case Error(_):
                    pass
            return rail

    @staticmethod
    @receipted(_REDACTION)
    def _emit(receipt: InferenceReceipt) -> InferenceReceipt:
        return receipt

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
            ppc = pymc.sample_posterior_predictive(
                trace, model=model, var_names=["observation"], random_seed=plan.seed, return_inferencedata=True
            )
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
        match ContentIdentity.of("pymc-model", _study_payload(spec), IdentityPolicy()):
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
            divergences=int(trace.sample_stats["diverging"].to_numpy().sum()),
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
        backend=spec.plan.backend.canonical,
        observed_dtype=observed.dtype.str,
        observed_shape=observed.shape,
        observed_bytes=observed.tobytes(),
    )
```

## [03]-[RESEARCH]

- [PYMC_SAMPLE]: `pymc` and `arviz` carry no cp315 wheel (`pymc` pulls `pytensor` to `numba` to `llvmlite`; `arviz` pulls `scipy`); the `pymc.Model`/`sample`/`compute_log_likelihood`/`sample_posterior_predictive`/`NUTS`/`Metropolis` and the `Normal`/`HalfNormal`/`Beta`/`Gamma`/`StudentT`/`Uniform`/`Bernoulli`/`Poisson`/`Binomial` distribution-class spellings with their `.api`-confirmed canonical params verify against `compute/.api/pymc.md` once the wheels resolve. `sample(draws, *, tune, chains, cores, random_seed, step, nuts_sampler, nuts_sampler_kwargs, idata_kwargs, return_inferencedata, model)` (ENTRYPOINTS [01]) is the one MCMC entrypoint: the `step` arm carries the context-bound `NUTS`/`Metropolis` step, and `nuts_sampler`+`nuts_sampler_kwargs` name the accelerated NUTS engine and its per-engine options PyMC dispatches to. The page passes neither `target_accept` nor `progressbar` nor `model=` to `sample` because the catalogued `NUTS(vars, max_treedepth, ...)`/`Metropolis(vars, proposal_dist, scaling, tune)` constructors and `sample` bind to the active `pm.Model()` context. `compute_log_likelihood(idata, *, var_names, extend_inferencedata=True, model, sample_dims)` (ENTRYPOINTS [03]) populates the `log_likelihood` group `arviz.loo`/`psense_summary` require — `pm.sample` does not populate it by default, so the explicit call is the load-bearing seam. The posterior container is `xarray.DataTree` (the `.api` flags `arviz.InferenceData` as the deprecated alias); the group accessor is `trace.sample_stats`/`trace.posterior_predictive`, not `trace["sample_stats"]`. `sample_posterior_predictive(trace, model, *, var_names, random_seed, return_inferencedata)` returns a `DataTree` carrying the `posterior_predictive` group beside the fitted-model draws. The GLM mean-link wraps the latent node in `pymc.math.invlogit`/`pymc.math.exp` (ENTRYPOINTS [03] `math.*` PyTensor tensor primitives) so a `bernoulli`/`binomial` `p` lands in `[0, 1]` and a `poisson` `mu` stays positive — feeding the raw real-valued latent into the bounded/positive `p`/`mu` is the deleted form pymc rejects at the support check.
- [ARVIZ_DIAGNOSTICS]: the arviz-1.x field and coordinate renames are load-bearing and verify against `compute/.api/arviz.md` IMPLEMENTATION_LAW. `arviz.summary(data, var_names, kind="all")` returns the combined `mean`/`sd`/`r_hat`/`ess_bulk`/`ess_tail` frame in one reduction (ENTRYPOINTS summary [06]) with the `r_hat` underscore and ETI (not HDI) interval columns. `arviz.hdi(data, var_names, prob)` (interval [07]) returns a Dataset with the `ci_bound` coordinate taking `'lower'`/`'upper'` — `.sel(ci_bound='lower')`/`.sel(ci_bound='upper')`, never the removed `hdi_3%`/`hdi_97%` columns and never the `higher` value. `arviz.loo(data, var_name, pointwise=True)` returns the `ELPDData` whose arviz-1.x fields are `elpd`/`se`/`p`/`good_k`/`warning` plus the pointwise `pareto_k` `DataArray` (PUBLIC_TYPES `ELPDData` [02]/[03]/[06]/[07]/[08]) — the arviz-0.x `elpd_loo`/`p_loo`/`looic` attributes do not exist in 1.x and reading them is the deleted form. `arviz.psense_summary(data)` (sensitivity [02]) is the prior-vs-likelihood power-scaling robustness table, folded to one `prior_sensitivity_max` scalar beside the rhat/ess/pareto-k deficit so a posterior over-sensitive to its prior fails the bar alongside a convergence failure.
- [ACCELERATED_NUTS_ENGINES]: `numpyro`, `blackjax`, and `nutpie` resolve on the gated `python_version<'3.15'` jaxlib/numba companion band beside the `pymc`/`arviz` floor, verified against `compute/.api/{numpyro,blackjax,nutpie}.md`. All three reach PyMC through the one confirmed `pymc.sample(..., nuts_sampler=<name>, nuts_sampler_kwargs=<options>)` dispatch (`pymc.md` ENTRYPOINTS [01], LOCAL_ADMISSION BACKEND_INTEGRATION) — PyMC owns the model lowering and the JAX/Numba handoff, so the page imports none of them. The per-engine `options` payload threads the `nutpie` `backend='numba'`/`'jax'` accelerator lever (`nutpie.md` ENTRYPOINTS [01], STACKING_TOPOLOGY `pm.sample(nuts_sampler="nutpie", nuts_sampler_kwargs={'backend':'jax'})`) and the `numpyro` `chain_method='parallel'`/`'vectorized'` (`numpyro.md` MCMC [01], STUDY_ROUTING) without a parallel draw body. The phantom `pymc.sampling.jax.sample_numpyro_nuts`/`sample_blackjax_nuts` callables do not appear in the `pymc` catalogue and are not driven here; the raw `nutpie.compile_pymc_model`/`sample` pair and the `blackjax.nuts`/`window_adaptation`/`util.run_inference_algorithm` algebra are the lower-level surfaces PyMC's `nuts_sampler` dispatch composes, never re-driven from this owner. The three accelerated engines collapse to the one `external_nuts(name, options)` case parameterized by the `nuts_sampler` name and its kwargs, not three near-identical bodies.
