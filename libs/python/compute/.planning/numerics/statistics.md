# [PY_COMPUTE_STATISTICS]

The one in-memory classical-statistics owner producing hypothesis-test and distribution-fit evidence over `scipy.stats`. `TestIntent` discriminates the two-sample Kolmogorov-Smirnov test, the Anderson-Darling goodness-of-fit, the Shapiro-Wilk normality test, the Mann-Whitney U rank-sum test, and a maximum-likelihood distribution fit; every route is one `_STAT_ROUTES` row over one fold — a `StatRoute` value object naming the `scipy.stats` entrypoint, the `Decision` reject-regime, and the typed `(statistic, criterion, parameters, moments)` projection as data rather than five `match` arms — folding through one `StatReport.graded` reduction behind the gated import and keying by `ContentIdentity` over `TestIntent.identity_buffer` — the intent-owned fold of the canonical sample bytes plus every active discriminant (`Goodness`/`Alternative`/`Distribution`, `alpha`, the Fit draw size), so the key names the report, never merely the operand. The sample data admits through `numerics/array.md#PAYLOAD` keying on the same `ContentIdentity` seed. The reject rule is the one defect this owner refuses to fragment: a `Decision` policy value owns both regimes — `SIGNIFICANCE` rejects when a p-value falls below `alpha`, `CRITICAL` rejects when the statistic exceeds the selected critical level — so the `criterion` slot carries one typed yardstick per route instead of the field overload where a p-value column smuggles `alpha` for the critical-value route. Like `optimization/program.md#PROGRAM`, this owner carries **no numpy floor**: the hypothesis test *is* `scipy.stats`, so a runtime run without the scipy package returns `Error(Import)` rather than a degraded estimate. Columnar and gridded statistical aggregation stays in the `data` branch gridded/field owner; this owner operates on an in-memory sample array only and never re-catalogues a grouped-reduction or labelled-array aggregation.

## [01]-[INDEX]

- [01]-[STATISTICS]: two-sample KS / Anderson-Darling / Shapiro-Wilk / Mann-Whitney U hypothesis tests plus MLE distribution fit over `scipy.stats`, driven by the one `_STAT_ROUTES` data table (each row a `(run, decision)` pair over a typed `TestResult` carrier) folding one `StatReport` on the one `TestIntent` owner.

## [02]-[STATISTICS]

- Owner: `TestIntent` — the test-and-fit cases discriminated by the statistical question recoverable from the sample shape and the named distribution, never a parallel per-test surface; `TwoSampleKS(a, b)` over `scipy.stats.ks_2samp`, `AndersonDarling(x, dist)` over `scipy.stats.anderson` (the reference distribution is the narrower `Goodness` vocabulary `anderson` accepts, never the full fit `Distribution` set) reading the statistic against the per-significance critical-value ladder, `ShapiroWilk(x)` over `scipy.stats.shapiro`, `MannWhitneyU(a, b, alternative)` over `scipy.stats.mannwhitneyu` threading the `Alternative` side, and `Fit(x, dist)` over the frozen `<dist>.fit(data)` maximum-likelihood estimator with the fitted enclosure scored back through `ks_2samp` and its `.stats(moments="mv")` mean/variance read as the fitted-distribution moments. The five routes are five `_STAT_ROUTES` rows over one fold, the three `(statistic, pvalue)` rows sharing the one `_significance` `run` body keyed by `_SIGNIFICANCE_CALLS` and only `anderson`/`fit` carrying dedicated readers: each `StatRoute` carries the `run` closure binding the route's `scipy.stats` entrypoint and projecting the typed `Reading` (`statistic`/`criterion`/`parameters`/`moments`) plus the `Decision` reject-regime as data, so `_stat_report` resolves the row, runs it, and folds the shared `StatReport.graded` — never five parallel helper bodies, never three near-identical significance helpers, never an inline verdict branch beside a shared static method. `Distribution` is the MLE-fittable continuous family and `Goodness` is the strictly narrower Anderson-Darling reference set (`scipy.stats.anderson` rejects any distribution outside `norm`/`expon`/`logistic`/`gumbel_l`/`gumbel_r`/`weibull_min`), so the AD intent cannot carry a distribution the route would raise on — two bounded vocabularies for two distinct admissible domains, never one over-wide enum shared across both; `Alternative` is the two-sided/less/greater rank-test side as a bounded vocabulary, never a string knob; `Decision` is the reject-rule regime as a policy value carrying its own `reject(statistic, criterion, alpha)` algebra, never a per-route inline comparison; `Verdict` is the reject/retain outcome the decision grades, never a boolean flag.
- Route table: each `StatRoute` `Struct` carries two orthogonal cells — `run` (`(TestIntent, alpha, fit_sample) -> Reading`, binding the route's `scipy.stats` entrypoint, reading the named result off the typed `TestResult`/`AndersonResult` carrier, and projecting the `Reading` four-tuple) and `decision` (the `Decision` reject-regime the row grades under). The three `(statistic, pvalue)` routes (`two_sample_ks`, `shapiro`, `mannwhitneyu`) share the one `_significance` body keyed by `_SIGNIFICANCE_CALLS[tag]` — a `(entry_name, kwargs)` projection the gated `getattr(stats, entry_name)(*samples, **kwargs)` binds — rather than three near-identical `_run_*` helpers, since the bodies differed only in the bound entrypoint and the `mannwhitneyu` `Alternative.value` keyword; only `_run_anderson` and `_run_fit` read divergent result shapes (`critical_values`/`significance_level` and the MLE parameters/moments) so they keep dedicated readers. The single `@beartype(conf=FAULT_CONF)`-fenced `_stat_report` body resolves the row from `intent.tag`, evaluates `run` into a `Reading`, and `match`es the railed `_stat_key` IN-BODY into `StatReport.graded` (a digest `Error` re-raising onto the enclosing `boundary` rather than threading a second rail) — so a new `(statistic, pvalue)` test is one `Tag` literal plus one `_SIGNIFICANCE_CALLS` row pointing the shared `_significance` body, and a divergent-shape test is one `_STAT_ROUTES` row plus one dedicated reader, never an arm in a five-way fold and never a per-route `_*_report` body. `alpha` threads into `run` (so the Anderson-Darling criterion selects its critical value at the configured level) and into `graded` (so every `Decision.reject` grades against the same level); `fit_sample` reaches only the `Fit` route's parametric-sample draw. `Reading` carries the per-route `(statistic, criterion, parameters, moments)` to the unified fold rather than the prior per-arm `StatReport.graded(...)` reconstruction copied across five bodies.
- Typed result carrier: `scipy.stats.ks_2samp`/`shapiro`/`mannwhitneyu` each return a named result the catalogue documents as carrying `.statistic`/`.pvalue`, and `anderson` a named result carrying `.statistic`/`.critical_values`/`.significance_level`; the route `run` closures read those fields off the `TestResult`/`AndersonResult` structural `Protocol`s declared under `TYPE_CHECKING`, so the body reads a named field on a typed carrier rather than a phantom bare `object` — the same typed-result discipline `optimization/program.md#PROGRAM` holds over `opt.OptimizeResult`, expressed as a local `Protocol` because the catalogue documents the `.statistic`/`.pvalue` shape rather than a public result-type name. The gated scipy package never imports at runtime; the `Protocol`s annotate the carrier only.
- Woven rail: `test` is the one entrypoint, riding the hub `evidence_run` weave (`graduation/handoff.md#EVIDENCE_WEAVE`) under the `compute.statistics` scope row — span, fault fence, and the fenced `@receipted(REDACTION)` harvest composed, the former page-local `_TRACER`/`_REDACTION` mints and inline span open the deleted forms. Composing the weave is an observability import, never a graduation admission: this owner's graduation-free charter bans `graduates()` calls, not the hub import.
- Entry: `test(intent, *, alpha, fit_sample)` returns `RuntimeRail[StatReport]` through the inline weave above. The fenced `_stat_report` body resolves the `_STAT_ROUTES` row, projects the samples, evaluates `run` into a `Reading`, and folds `StatReport.graded`, which carries the typed `Tag`, the `Decision`, the `statistic`, the route `criterion`, the graded `Verdict`, the fitted parameter tuple (empty for a pure hypothesis test), the fitted `moments` (`Nothing` for a pure hypothesis test), and the `ContentKey`. The `TwoSampleKS`/`ShapiroWilk`/`MannWhitneyU` routes read `result.statistic`/`result.pvalue` and grade under `Decision.SIGNIFICANCE` (the p-value is the criterion); the `AndersonDarling` route reads `result.statistic` against the `result.critical_values`/`result.significance_level` ladder selected at `alpha` and grades under `Decision.CRITICAL` (the critical level is the criterion); the `Fit` route reads the MLE parameter tuple, freezes the distribution at the estimate, draws a parametric reference through `<dist>(*params).rvs(size, random_state=rng)` where `rng` is `default_rng(SeedSequence(int.from_bytes(buffer)))` seeded off the sample buffer so the draw is reproducible per input (an unseeded `rvs` would re-score a fresh GOF p-value on identical data and break the `ContentKey` cache-hit-by-reference contract), scores the empirical-against-parametric `ks_2samp` for the goodness-of-fit statistic/p-value criterion under `Decision.SIGNIFICANCE`, and reads the frozen `.stats(moments="mv")` mean/variance into the `moments` slot.
- Receipt: `StatReport.contribute` returns the one-element `tuple[Receipt, ...]` the runtime `ReceiptContributor` port streams — `Receipt.of("compute.statistics", ("emitted", self.test, facts))` against the runtime two-argument `of(owner, evidence)` contract, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes and never a single-`Receipt` return against the `Iterable[Receipt]` port — carrying the test tag, the decision regime, the statistic, the criterion, the verdict, the fitted parameters, and the fitted moments as native scalars the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()`/`f""` coerce. `span_facts` is the disjoint bounded `str | int | float` scalar source the span reads, the full fitted ledger riding the receipt facts only — the span/receipt split every own-receipt sibling holds. Egress is the weave's fenced `@receipted(REDACTION)` harvest, never an inline `Signals.emit` threaded through `test` and never a downstream consumer the owner forgets to wire. Statistical evidence does not cross the graduation rail: `graduation/handoff.md#GRADUATION` admits the eight `solver`/`symbolic`/`model_asset`/`array_layout`/`unit_law`/`uncertainty_law`/`geometry`/`convex_program` axis cases, none of which a frequentist reject/retain verdict is (the `solver` case is a first-order/`OptimizeResult` convergence verdict and `uncertainty_law` the Bayesian posterior diagnostics), so the `StatReport` streams onto the receipt rail and stops, the same egress boundary `experiments/study.md#STUDY` holds.
- Packages: `scipy` (`stats.ks_2samp`, `stats.anderson`, `stats.shapiro`, `stats.mannwhitneyu`, the frozen `stats.norm`/`stats.lognorm`/`stats.gamma`/`stats.beta`/`stats.t`/`stats.chi2`/`stats.expon`/`stats.weibull_min` continuous distributions and their `.fit`/`.rvs(size, random_state)`/`.stats`, `stats.rv_continuous` base — all catalogued in `compute/.api/scipy.md`'s `scipy.stats` entrypoint and public-type tables, the named result carriers' `.statistic`/`.pvalue`/`.critical_values`/`.significance_level` fields annotated through the `TYPE_CHECKING` `TestResult`/`AndersonResult` `Protocol`s so the route `run` closures read a typed field rather than a phantom bare `object` while the gated package never imports at runtime, entrypoints staying boundary-scoped per the manifest import policy), `numpy` (`asarray`, `ascontiguousarray`, `where`, `argmin`, `inf` — the canonical sample buffer and the critical-value-ladder selection of the smallest admissible significance level at or above the configured `alpha`; `random.default_rng`/`random.SeedSequence` — the deterministic generator seeded off the `Fit` route's sample buffer so the parametric `rvs` draw is reproducible per input), `expression` (`tagged_union`/`tag`/`case` — the `TestIntent` union; `Option`/`Some`/`Nothing`/`default_arg` — the fitted-moments slot, present only for the MLE fit; `Ok`/`Error` — the `test` rail arm and the in-body `_stat_key` re-raise match; `Map`/`Map.empty` from `expression.collections` — the keep-all `_REDACTION` policy table), `msgspec` (`Struct` — the frozen `StatRoute` row carrier and the `StatReport`/`Reading` receipts, all GC-tracked rather than `gc=False` because each holds container/closure fields — `StatRoute` the `run` closure, `Reading` the `parameters` tuple and `Option` moments, `StatReport` the same plus the `ContentKey` — the program.md container-carrier rule, never the `gc=False`-on-a-tuple-carrier deleted form study.md rejects), `expression.collections` (`Map` — the `_STAT_ROUTES` route table the dispatch resolves by tag and the `_SIGNIFICANCE_CALLS` `(entry_name, kwargs)` projection table the one `_significance` body resolves the three trivial routes through; `@beartype(conf=FAULT_CONF)` — the `_stat_report` contract fence whose violation the `CLASSIFY` `api` row folds onto the rail), hub (`EvidenceScope`/`evidence_run` — the span/fence/harvest weave), `numerics/array.md#PAYLOAD` (the sample arrays admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), runtime (`RuntimeRail`, `boundary`, `FAULT_CONF`; `ContentIdentity`/`ContentKey` over the `CANONICAL_POLICY` default; `Receipt`/`ReceiptContributor` — receipt egress riding the weave's `@receipted(REDACTION)` harvest).
- Growth: a new `(statistic, pvalue)` hypothesis test is one `Tag` literal, one `TestIntent` case, and one `_SIGNIFICANCE_CALLS` row pointing the shared `_significance` body plus one `_STAT_ROUTES` row carrying its `(_significance, decision)` pair — never a sixth `_run_*` helper; a test whose result shape diverges (a third critical-value ladder, a second MLE estimator) is one `_STAT_ROUTES` row plus one dedicated reader. A new fittable distribution is one `Distribution` row resolving its frozen `scipy.stats` object, and a new Anderson-Darling reference is one `Goodness` row only when `scipy.stats.anderson` documents it; a new rank-test side is one `Alternative` row; a new reject regime is one `Decision` row carrying its own `reject` rule, never a per-route inline comparison; zero new surface, never a per-test owner, never a parallel fit-and-test struct, never a per-route `_*_report` helper body, never a fan of near-identical `_run_*` significance helpers parallel to the `_SIGNIFICANCE_CALLS` table, never a five-arm dispatch fold parallel to the route table, never a second statistics receipt beside `StatReport`, never a criterion field that overloads its meaning per route.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, Protocol, assert_never

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Some, case, default_arg, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # the `scipy.stats` named result carriers, typed structurally because the catalogue documents the
    # `.statistic`/`.pvalue` shape rather than a public result-type name; the gated package never imports
    # at runtime, so the route `run` closures annotate the carrier through these `Protocol`s only.
    class TestResult(Protocol):
        statistic: float
        pvalue: float

    class AndersonResult(Protocol):
        statistic: float
        critical_values: np.ndarray
        significance_level: np.ndarray


# --- [TYPES] -------------------------------------------------------------------------------

type Tag = Literal["two_sample_ks", "anderson", "shapiro", "mannwhitneyu", "fit"]


class Distribution(StrEnum):  # the MLE-fittable continuous families; the enum value is the `scipy.stats` attribute name
    NORM = "norm"
    LOGNORM = "lognorm"
    GAMMA = "gamma"
    BETA = "beta"
    T = "t"
    CHI2 = "chi2"
    EXPON = "expon"
    WEIBULL_MIN = "weibull_min"


class Goodness(StrEnum):  # the Anderson-Darling-supported reference distributions; scipy.stats.anderson rejects any other dist
    NORM = "norm"
    EXPON = "expon"
    LOGISTIC = "logistic"
    GUMBEL_L = "gumbel_l"
    GUMBEL_R = "gumbel_r"
    WEIBULL_MIN = "weibull_min"


class Alternative(StrEnum):
    TWO_SIDED = "two-sided"
    LESS = "less"
    GREATER = "greater"


class Decision(StrEnum):
    SIGNIFICANCE = "significance"  # reject H0 when the criterion (a p-value) falls below alpha
    CRITICAL = "critical"  # reject H0 when the statistic exceeds the selected critical value

    def reject(self, statistic: float, criterion: float, alpha: float) -> bool:
        # total over the closed regime so a new `Decision` row is a compile-surfaced `reject` arm rather
        # than silently routed to the significance branch, the `Termination.adjudicate` discipline.
        match self:
            case Decision.CRITICAL:
                return statistic > criterion
            case Decision.SIGNIFICANCE:
                return criterion < alpha
            case _ as unreachable:
                assert_never(unreachable)


class Verdict(StrEnum):
    REJECT = "reject"
    RETAIN = "retain"


# --- [MODELS] ------------------------------------------------------------------------------


class Reading(Struct, frozen=True):  # GC-tracked: carries the `parameters` tuple and the `Option` moments container
    # the per-route projection off the `scipy.stats` result the unified fold consumes: the scalar
    # statistic, the route's reject yardstick, the fitted MLE parameters (empty for a pure hypothesis
    # test), and the fitted moments (`Nothing` for a pure hypothesis test). One carrier across every
    # route rather than a per-arm `StatReport.graded(...)` reconstruction copied five times. GC-tracked
    # rather than `gc=False` because `parameters`/`moments` are container fields, the program.md rule.
    statistic: float
    criterion: float
    parameters: tuple[float, ...] = ()
    moments: Option[tuple[float, float]] = Nothing


class StatReport(Struct, frozen=True):
    test: Tag
    decision: Decision
    statistic: float
    criterion: float  # the route's reject yardstick: a p-value (SIGNIFICANCE) or a critical level (CRITICAL)
    verdict: Verdict
    parameters: tuple[float, ...]
    moments: Option[tuple[float, float]]  # fitted (mean, variance) for the MLE fit, Nothing for a pure hypothesis test
    content_key: ContentKey

    @staticmethod
    def graded(test: Tag, decision: Decision, reading: Reading, alpha: float, key: ContentKey) -> "StatReport":
        verdict = Verdict.REJECT if decision.reject(reading.statistic, reading.criterion, alpha) else Verdict.RETAIN
        return StatReport(test, decision, reading.statistic, reading.criterion, verdict, reading.parameters, reading.moments, key)

    def contribute(self) -> Iterable[Receipt]:
        # the runtime two-argument `Receipt.of(owner, evidence)` contract: the `(Phase, subject, facts)`
        # triple mints `fact` at `emitted`. Native scalars ride the `EventDict` `dict[str, object]` slots
        # the `enc_hook=repr` renderer serializes without a coerce; the `Option` moments lower to `None`.
        facts: dict[str, object] = {
            "test": self.test,
            "decision": self.decision.value,
            "statistic": self.statistic,
            "criterion": self.criterion,
            "verdict": self.verdict.value,
            "parameters": self.parameters,
            "moments": default_arg(self.moments, ()),
        }
        return (Receipt.of("compute.statistics", ("emitted", self.test, facts)),)

    @property
    def span_facts(self) -> dict[str, str | int | float]:
        # the bounded-scalar source the `EvidenceScope.STATISTICS`-scoped `stat.{tag}` span reads — exactly the `str | int | float`
        # set `Span.set_attributes` admits; the fitted `parameters`/`moments` ledger rides the receipt
        # facts only, the span/receipt split the sibling `experiments/study.md#STUDY` owner holds.
        return {
            "stat.test": self.test,
            "stat.decision": self.decision.value,
            "stat.statistic": self.statistic,
            "stat.criterion": self.criterion,
            "stat.verdict": self.verdict.value,
        }


@tagged_union(frozen=True)
class TestIntent:
    tag: Tag = tag()
    two_sample_ks: tuple[np.ndarray, np.ndarray] = case()
    anderson: tuple[np.ndarray, Goodness] = case()
    shapiro: np.ndarray = case()
    mannwhitneyu: tuple[np.ndarray, np.ndarray, Alternative] = case()
    fit: tuple[np.ndarray, Distribution] = case()

    @staticmethod
    def TwoSampleKS(a: np.ndarray, b: np.ndarray) -> "TestIntent":
        return TestIntent(two_sample_ks=(a, b))

    @staticmethod
    def AndersonDarling(x: np.ndarray, dist: Goodness = Goodness.NORM) -> "TestIntent":
        return TestIntent(anderson=(x, dist))

    @staticmethod
    def ShapiroWilk(x: np.ndarray) -> "TestIntent":
        return TestIntent(shapiro=x)

    @staticmethod
    def MannWhitneyU(a: np.ndarray, b: np.ndarray, alternative: Alternative = Alternative.TWO_SIDED) -> "TestIntent":
        return TestIntent(mannwhitneyu=(a, b, alternative))

    @staticmethod
    def Fit(x: np.ndarray, dist: Distribution) -> "TestIntent":
        return TestIntent(fit=(x, dist))

    @property
    def samples(self) -> tuple[np.ndarray, ...]:
        # the float64 sample arrays each route runs over, projected total over the tag and closed by
        # `assert_never`; identity is `identity_buffer`'s concern, never a second projection here.
        match self:
            case TestIntent(tag="two_sample_ks", two_sample_ks=(a, b)) | TestIntent(tag="mannwhitneyu", mannwhitneyu=(a, b, _)):
                return (np.asarray(a, dtype=float), np.asarray(b, dtype=float))
            case TestIntent(tag="anderson", anderson=(x, _)) | TestIntent(tag="shapiro", shapiro=x) | TestIntent(tag="fit", fit=(x, _)):
                return (np.asarray(x, dtype=float),)
            case _ as unreachable:
                assert_never(unreachable)

    def identity_buffer(self, alpha: float, fit_sample: int) -> bytes:
        # the FULL report identity, owned by the intent: sample bytes plus every non-array discriminant
        # the graded verdict reads — the per-case `Goodness`/`Alternative`/`Distribution` value, `alpha`
        # (every route grades against it), and `fit_sample` (the Fit draw size only) — so two intents
        # that can grade differently never share a `ContentKey`. Length-prefixed parts keep the buffer
        # unambiguous under arbitrary sample bytes; a routes-active discriminant absent from its case
        # tail is the defect this fold exists to make impossible.
        tail: tuple[bytes, ...]
        match self:
            case TestIntent(tag="anderson", anderson=(_, dist)):
                tail = (dist.value.encode(),)
            case TestIntent(tag="mannwhitneyu", mannwhitneyu=(_, _, side)):
                tail = (side.value.encode(),)
            case TestIntent(tag="fit", fit=(_, dist)):
                tail = (dist.value.encode(), fit_sample.to_bytes(8, "big"))
            case _:
                tail = ()
        parts = (self.tag.encode(), *(np.ascontiguousarray(s).tobytes() for s in self.samples), np.float64(alpha).tobytes(), *tail)
        return b"".join(len(part).to_bytes(8, "big") + part for part in parts)


class StatRoute(Struct, frozen=True):
    run: Callable[[TestIntent, float, int], Reading]  # binds the route's `scipy.stats` entrypoint and projects the typed `Reading`
    decision: Decision  # the reject-regime the row grades under


# --- [OPERATIONS] --------------------------------------------------------------------------

def test(intent: TestIntent, *, alpha: float = 0.05, fit_sample: int = 4096) -> "RuntimeRail[StatReport]":
    # the one entrypoint rides the hub `evidence_run` weave — span from the `compute.statistics` scope
    # row, `boundary` fence over the `@beartype`-fenced `_stat_report`, and the fenced
    # `@receipted(REDACTION)` harvest of the returned `StatReport` — so a `scipy.stats` raise, the
    # gated `ImportError`, a contract violation, or an in-body digest `Error` all fold onto the ONE
    # rail and emission is composed, never page-owned. Composing the weave is an observability
    # import only: this owner stays graduation-free by charter and calls no `graduates()`.
    return evidence_run(EvidenceScope.STATISTICS, f"stat.{intent.tag}", lambda: _stat_report(intent, alpha, fit_sample))



@beartype(conf=FAULT_CONF)
def _stat_report(intent: TestIntent, alpha: float, fit_sample: int) -> StatReport:
    # `@beartype(conf=FAULT_CONF)` raises the canonical violation the `CLASSIFY` `api` row folds onto the
    # rail. `alpha` threads into `run` so the Anderson-Darling criterion selects its critical value at the
    # configured level, and into `graded` so every `Decision.reject` grades against the same `alpha`. The
    # `_stat_key` rail is matched HERE inside the already-fenced body so a digest `Error` re-raises onto
    # the `boundary` rather than masking the key behind a fabricated fallback or flattening a double rail.
    route = _STAT_ROUTES[intent.tag]
    reading = route.run(intent, alpha, fit_sample)
    match _stat_key(intent, alpha, fit_sample):
        case Ok(key):
            return StatReport.graded(intent.tag, route.decision, reading, alpha, key)
        case Error(fault):
            raise RuntimeError(fault)  # the `boundary` `_convert` re-folds it; `BoundaryFault` is no exception


def _stat_key(intent: TestIntent, alpha: float, fit_sample: int) -> "RuntimeRail[ContentKey]":
    # the intent OWNS its identity: `identity_buffer` folds samples plus every active discriminant, so
    # the key names the report, never merely the operand. The buffer carries no tessellation tolerance,
    # so the `CANONICAL_POLICY` default keys the canonical path — an explicit `IdentityPolicy()`
    # allocation keys identically (frozen value equality) and is deleted as pure ceremony.
    return ContentIdentity.of(f"stat.{intent.tag}", intent.identity_buffer(alpha, fit_sample))


def _significance(intent: TestIntent, _alpha: float, _: int) -> Reading:
    # the three `(statistic, pvalue)` routes collapse to ONE parameterized body keyed by tag rather than
    # three near-identical `_run_*` helpers: `_SIGNIFICANCE_CALLS[tag]` projects `(entry_name, kwargs)`
    # off the intent, the gated `getattr(stats, entry_name)` binds the entrypoint inside the boundary, and
    # the named result's `.statistic`/`.pvalue` is the SIGNIFICANCE criterion. A new significance test is
    # one `_SIGNIFICANCE_CALLS` row, never a sixth `_run_*` body.
    from scipy import stats

    entry, kwargs = _SIGNIFICANCE_CALLS[intent.tag](intent)
    result: TestResult = getattr(stats, entry)(*intent.samples, **kwargs)
    return Reading(float(result.statistic), float(result.pvalue))


def _run_anderson(intent: TestIntent, alpha: float, _: int) -> Reading:
    from scipy import stats

    (x,) = intent.samples
    _, dist = intent.anderson
    result: AndersonResult = stats.anderson(x, dist=dist.value)
    # `anderson` returns the critical values at fixed significance percents (`[15, 10, 5, 2.5, 1]`);
    # the criterion is the critical value at the tightest published level still at or above `alpha`
    # (the smallest admissible percent, so `alpha=0.05` selects the `5` grid point exactly), picked by
    # masking the sub-`alpha` levels to `+inf` and taking the catalogued `np.argmin` rather than `np.interp`.
    levels = np.asarray(result.significance_level, dtype=float)
    admissible = np.where(levels >= alpha * 100.0, levels, np.inf)
    pick = int(np.argmin(admissible))
    return Reading(float(result.statistic), float(np.asarray(result.critical_values, dtype=float)[pick]))


def _run_fit(intent: TestIntent, _alpha: float, fit_sample: int) -> Reading:
    from scipy import stats

    (x,) = intent.samples
    _, dist = intent.fit
    frozen = getattr(stats, dist.value)
    params = tuple(float(p) for p in frozen.fit(x))
    estimate = frozen(*params)
    # the parametric reference draw is seeded off the sample buffer through the catalogued
    # `SeedSequence`/`default_rng` (the contiguous `tobytes()` is the entropy, taken big-endian) so the
    # goodness-of-fit p-value is reproducible per input — the determinism the `ContentKey`
    # cache-hit-by-reference contract requires, which an unseeded `rvs` draw would break (identical
    # input must re-key AND re-score to one verdict).
    entropy = int.from_bytes(np.ascontiguousarray(x).tobytes(), "big")
    rng = np.random.default_rng(np.random.SeedSequence(entropy))
    gof: TestResult = stats.ks_2samp(x, estimate.rvs(size=fit_sample, random_state=rng))
    mean, var = estimate.stats(moments="mv")
    return Reading(float(gof.statistic), float(gof.pvalue), parameters=params, moments=Some((float(mean), float(var))))


# --- [TABLES] ------------------------------------------------------------------------------

# the three `(statistic, pvalue)` routes project `(entry_name, kwargs)` off the intent so the one
# `_significance` body binds `getattr(stats, entry_name)(*samples, **kwargs)` rather than carrying three
# near-identical `_run_*` helpers: `ks_2samp`/`shapiro` take only the projected samples, `mannwhitneyu`
# threads the `Alternative.value` rank-test side. The Anderson-Darling and Fit routes read divergent
# result shapes (`critical_values`/`significance_level`, MLE parameters/moments) so they keep dedicated
# `_run_anderson`/`_run_fit` readers — only the truly-identical bodies collapse to the table.
_SIGNIFICANCE_CALLS: Map[Tag, Callable[[TestIntent], tuple[str, dict[str, object]]]] = Map.of_seq([
    ("two_sample_ks", lambda _: ("ks_2samp", {})),
    ("shapiro", lambda _: ("shapiro", {})),
    ("mannwhitneyu", lambda i: ("mannwhitneyu", {"alternative": i.mannwhitneyu[2].value})),
])


# the five tests collapse to one route row per tag driving one `_stat_report` fold: `run` binds the
# route's `scipy.stats` entrypoint and projects the typed `Reading`, `decision` carries the reject
# regime the row grades under — the three `(statistic, pvalue)` tests share the one table-driven
# `_significance` body and grade SIGNIFICANCE against the p-value, the Anderson-Darling route grades
# CRITICAL against the selected critical level, and the Fit route grades its re-scored GOF p-value.
_STAT_ROUTES: Map[Tag, StatRoute] = Map.of_seq([
    ("two_sample_ks", StatRoute(_significance, Decision.SIGNIFICANCE)),
    ("anderson", StatRoute(_run_anderson, Decision.CRITICAL)),
    ("shapiro", StatRoute(_significance, Decision.SIGNIFICANCE)),
    ("mannwhitneyu", StatRoute(_significance, Decision.SIGNIFICANCE)),
    ("fit", StatRoute(_run_fit, Decision.SIGNIFICANCE)),
])
```
