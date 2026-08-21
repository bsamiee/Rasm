# [PY_COMPUTE_STATISTICS]

One in-memory classical-statistics owner producing hypothesis-test and distribution-fit evidence over `scipy.stats`: every route is one `_STAT_ROUTES` row folding one `StatReport` on the one `TestIntent` owner. This owner carries no numpy floor — the hypothesis test IS `scipy.stats`, so a run without the package returns `Error(Import)` rather than a degraded estimate — and columnar or gridded statistical aggregation stays in the `data` branch gridded/field owner, never re-catalogued here.

`test` rides the hub `evidence_run` weave under the `compute.statistics` scope row, and the owner is graduation-free by charter: a frequentist reject/retain verdict is none of the graduation axes, so a `StatReport` streams onto the receipt rail and stops — the same egress boundary `experiments/study#STUDY` holds, and composing the weave is an observability import, never a graduation admission. Sample arrays admit as `numerics/array#PAYLOAD` payloads keying through the same `ContentIdentity` seed; the report key is intent-owned over the sample bytes and every active discriminant, so the key names the report, never merely the operand.

## [01]-[INDEX]

- [02]-[STATISTICS]: hypothesis tests and MLE distribution fit over `scipy.stats`, one `_STAT_ROUTES` row per route folding one `StatReport` on the `TestIntent` owner.

## [02]-[STATISTICS]

- Owner: `TestIntent` — `Goodness` is the strictly narrower Anderson-Darling reference set because `scipy.stats.anderson` rejects any distribution outside its published set, so a reference the route raises on is unspellable on the AD intent — two bounded vocabularies for two admissible domains, never one over-wide enum; `Decision` owns both reject regimes as a policy value carrying its own `reject` algebra, so `criterion` is one typed yardstick per route, never a field overload where a p-value column smuggles `alpha` for the critical-value route.
- Cases: the three `(statistic, pvalue)` routes share the one `_significance` body keyed by `_SIGNIFICANCE_CALLS` because their bodies differed only in the bound entrypoint and one keyword; `anderson` and `fit` read divergent result shapes and keep dedicated readers — only truly-identical bodies collapse to the table.
- Law: the report `ContentKey` resolves ahead of the route and IS the replay seed a drawing route takes, so one derivation over the sample bytes serves both the report identity and the reference draw. A second entropy source over those same bytes is the deleted form on both axes — it can fork from the identity it mirrors, and its own cost is quadratic in the sample where the bounded digest is flat.
- Packages: the scipy result carriers are typed through local `TYPE_CHECKING` `Protocol`s because the catalogue documents the `.statistic`/`.pvalue` shape rather than a public result-type name, and the gated package never imports at runtime; entrypoints stay boundary-scoped per the manifest import policy. This page opens NO fence of its own — the hub `evidence_run` weave is its one fault seam, so a `scipy.stats` raise and the gated `ImportError` both classify there and a rail already in hand returns rather than re-raising into it.
- Growth: a new `(statistic, pvalue)` test is one `Tag` literal, one `TestIntent` case, one `_SIGNIFICANCE_CALLS` row, and one `_STAT_ROUTES` row; a divergent-shape test adds one dedicated reader instead; a new fittable distribution is one `Distribution` row; a new Anderson-Darling reference is one `Goodness` row only when `scipy.stats.anderson` documents it; a new reject regime is one `Decision` row carrying its own `reject` rule.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, Protocol, assert_never

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import FAULT_CONF, RuntimeRail
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey

# cold scientific dependency: the `lazy` bind defers the whole `scipy.stats` distribution tree to the first
# route body that fires. No module-scope cell reifies the proxy at import: `_SIGNIFICANCE_CALLS` rows carry
# entry NAMES inside call-time thunks and `_STAT_ROUTES` rows bind the LOCAL route bodies through
# `StatRoute.run`, so only a route body's call-time read — the `getattr(stats, entry)` seam and the direct
# `stats.<member>` calls — touches the live proxy.
lazy from scipy import stats

if TYPE_CHECKING:
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


class Goodness(StrEnum):  # the Anderson-Darling reference set; the enum value is the `scipy.stats.anderson` reference name
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
        # total over the closed regime, so a new `Decision` row is a compile-surfaced arm, never silently routed to significance.
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
    # one per-route projection the unified fold consumes; `parameters` empty and `moments` `Nothing` for a pure hypothesis test.
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
        # ONE settled-receipt spine. The band names the REJECT verdict rather than leaving a reader to compare a
        # `verdict` string against a vocabulary it does not hold — this page graduates nothing by charter, so the
        # band is its only outward finding channel — and absent `moments` OMIT their key where `default_arg(..., ())`
        # published an empty tuple a consumer could not tell from a fitted pair of zero moments.
        facts: dict[str, object] = {
            "test": self.test,
            "decision": self.decision.value,
            "statistic": self.statistic,
            "criterion": self.criterion,
            "verdict": self.verdict.value,
            "parameters": self.parameters,
        } | self.moments.map(lambda pair: {"moments": pair}).default_value({})
        return (
            Receipt.of(
                EvidenceScope.STATISTICS.value,
                ("emitted", self.test, facts),
                key=Some(self.content_key),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.content_key)),
                band=Block.of_seq(("rejected",) if self.verdict is Verdict.REJECT else ()),
            ),
        )

    @property
    def span_facts(self) -> dict[str, str | int | float]:
        # exactly the `str | int | float` set `Span.set_attributes` admits; the fitted `parameters`/`moments` ledger rides the receipt facts only.
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
        # identity is `identity_source`'s concern, never a second projection here.
        match self:
            case TestIntent(tag="two_sample_ks", two_sample_ks=(a, b)) | TestIntent(tag="mannwhitneyu", mannwhitneyu=(a, b, _)):
                return (np.asarray(a, dtype=float), np.asarray(b, dtype=float))
            case TestIntent(tag="anderson", anderson=(x, _)) | TestIntent(tag="shapiro", shapiro=x) | TestIntent(tag="fit", fit=(x, _)):
                return (np.asarray(x, dtype=float),)
            case _ as unreachable:
                assert_never(unreachable)

    def identity_source(self, alpha: float, fit_sample: int) -> IdentitySource:
        # sample bytes plus every discriminant the graded verdict reads, so two intents that can grade differently never share a
        # `ContentKey`. These are N SEMANTIC FIELDS and hand `IdentitySource(parts=...)`, so the `[PREIMAGE_FRAMING]`
        # count-and-length framing runs at its ONE owner: the retired tail re-spelled that owner's `_framed` fold
        # verbatim, and a width or byte-order change there would have forked this page's key namespace silently.
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
        # the sample roster states its own cardinality, so a one-sample and a two-sample intent whose concatenated
        # bytes coincide under different splits still key apart.
        samples = tuple(np.ascontiguousarray(s).tobytes() for s in self.samples)
        return IdentitySource(parts=(self.tag.encode(), str(len(samples)).encode(), *samples, np.float64(alpha).tobytes(), *tail))


class StatRoute(Struct, frozen=True):
    # `run` takes the report's own `ContentKey` because a route needing a reproducible draw seeds off THAT key rather
    # than re-deriving a second identity from the same bytes; a route that draws nothing ignores the argument.
    run: Callable[[TestIntent, float, int, ContentKey], Reading]  # binds the `scipy.stats` entrypoint, projects the typed `Reading`
    decision: Decision  # the reject-regime the row grades under


# --- [OPERATIONS] --------------------------------------------------------------------------

def test(intent: TestIntent, *, alpha: float = 0.05, fit_sample: int = 4096, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[StatReport]":
    # a `scipy.stats` raise, the gated `ImportError`, a contract violation, or an in-body digest `Error` all fold onto the ONE rail.
    facts = {"test": intent.tag, "alpha": alpha}
    return evidence_run(EvidenceScope.STATISTICS, f"stat.{intent.tag}", lambda: _stat_report(intent, alpha, fit_sample), facts=facts, composition=composition)



@beartype(conf=FAULT_CONF)
def _stat_report(intent: TestIntent, alpha: float, fit_sample: int) -> "RuntimeRail[StatReport]":
    # `alpha` threads into `run` (the AD criterion selects its critical value at the configured level) AND into `graded` (every
    # `Decision.reject` grades against the same level); the `_stat_key` rail is matched HERE inside the already-fenced body so a
    # digest `Error` re-raises onto the boundary rather than flattening a double rail. The key resolves BEFORE the route runs,
    # so the one identity the report carries is also the one seed a drawing route replays from — never two derivations over the
    # same bytes that a later change to either can silently fork.
    route = _STAT_ROUTES[intent.tag]
    # the digest rail THREADS rather than re-raising: the retired `raise RuntimeError(fault)` smuggled an already-typed
    # `BoundaryFault` back out through an untyped exception, and the weave fence's catch-all keeps `str(cause)` — so a
    # digest refusal reached its consumer as a message string with its subject, leg, and case erased. A rail in hand
    # is returned, never re-raised to be re-classified by a fence that knows less than the value already did.
    return _stat_key(intent, alpha, fit_sample).map(
        lambda key: StatReport.graded(intent.tag, route.decision, route.run(intent, alpha, fit_sample, key), alpha, key)
    )


def _stat_key(intent: TestIntent, alpha: float, fit_sample: int) -> "RuntimeRail[ContentKey]":
    # `CANONICAL_POLICY` default keys the canonical path — an explicit `IdentityPolicy()` allocation keys identically and is ceremony.
    return ContentIdentity.of(f"stat.{intent.tag}", intent.identity_source(alpha, fit_sample))


def _significance(intent: TestIntent, _alpha: float, _sample: int, _key: ContentKey) -> Reading:
    # `_SIGNIFICANCE_CALLS[tag]` projects `(entry_name, kwargs)` off the intent and the gated `getattr(stats, entry_name)` binds
    # entrypoint inside the boundary; the named result's `.pvalue` is the SIGNIFICANCE criterion.
    entry, kwargs = _SIGNIFICANCE_CALLS[intent.tag](intent)
    result: TestResult = getattr(stats, entry)(*intent.samples, **kwargs)
    return Reading(float(result.statistic), float(result.pvalue))


def _run_anderson(intent: TestIntent, alpha: float, _sample: int, _key: ContentKey) -> Reading:
    (x,) = intent.samples
    _, dist = intent.anderson
    result: AndersonResult = stats.anderson(x, dist=dist.value)
    # criterion is the critical value at the tightest published significance level still at or above `alpha`, picked by masking
    # sub-`alpha` levels to `+inf` and taking `np.argmin` — never `np.interp` between grid points.
    levels = np.asarray(result.significance_level, dtype=float)
    admissible = np.where(levels >= alpha * 100.0, levels, np.inf)
    pick = int(np.argmin(admissible))
    return Reading(float(result.statistic), float(np.asarray(result.critical_values, dtype=float)[pick]))


def _run_fit(intent: TestIntent, _alpha: float, fit_sample: int, key: ContentKey) -> Reading:
    (x,) = intent.samples
    _, dist = intent.fit
    frozen = getattr(stats, dist.value)
    params = tuple(float(p) for p in frozen.fit(x))
    estimate = frozen(*params)
    # reference draw seeds off the report's own 128-bit digest so the GOF p-value is reproducible per input — an unseeded `rvs`
    # would re-score a fresh verdict on identical data and break the `ContentKey` cache-hit-by-reference contract, while the
    # digest already folds the samples, `alpha`, `fit_sample`, and every route discriminant the identity buffer names.
    # Seeding off the raw sample bytes is the deleted form and the cost is QUADRATIC, MEASURED:
    # `SeedSequence(int.from_bytes(x.tobytes(), "big"))` takes 0.027 s at 512 float64 samples, 0.419 s at 2048, and 7.05 s at
    # 8192 — the coercion divides a megabit-wide integer down to 32-bit words — where the bounded digest is flat.
    rng = np.random.default_rng(np.random.SeedSequence(key.project("digest")))
    gof: TestResult = stats.ks_2samp(x, estimate.rvs(size=fit_sample, random_state=rng))
    mean, var = estimate.stats(moments="mv")
    return Reading(float(gof.statistic), float(gof.pvalue), parameters=params, moments=Some((float(mean), float(var))))


# --- [TABLES] ------------------------------------------------------------------------------

# `ks_2samp`/`shapiro` take only the projected samples; `mannwhitneyu` threads the `Alternative.value` rank-test side.
_SIGNIFICANCE_CALLS: Map[Tag, Callable[[TestIntent], tuple[str, dict[str, object]]]] = Map.of_seq([
    ("two_sample_ks", lambda _: ("ks_2samp", {})),
    ("shapiro", lambda _: ("shapiro", {})),
    ("mannwhitneyu", lambda i: ("mannwhitneyu", {"alternative": i.mannwhitneyu[2].value})),
])


# one route row per tag driving one `_stat_report` fold; `decision` carries the reject regime the row grades under.
_STAT_ROUTES: Map[Tag, StatRoute] = Map.of_seq([
    ("two_sample_ks", StatRoute(_significance, Decision.SIGNIFICANCE)),
    ("anderson", StatRoute(_run_anderson, Decision.CRITICAL)),
    ("shapiro", StatRoute(_significance, Decision.SIGNIFICANCE)),
    ("mannwhitneyu", StatRoute(_significance, Decision.SIGNIFICANCE)),
    ("fit", StatRoute(_run_fit, Decision.SIGNIFICANCE)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
