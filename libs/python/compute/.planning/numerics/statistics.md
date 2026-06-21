# [PY_COMPUTE_STATISTICS]

The one in-memory classical-statistics owner producing hypothesis-test and distribution-fit evidence over `scipy.stats`. `TestIntent` discriminates the two-sample Kolmogorov-Smirnov test, the Anderson-Darling goodness-of-fit, the Shapiro-Wilk normality test, the Mann-Whitney U rank-sum test, and a maximum-likelihood distribution fit, every route folding the scalar `statistic`, the route's reject yardstick (`criterion`), the `Decision`-graded `Verdict`, the fitted parameter tuple, and the fitted moments into the `StatReport` receipt and keying by `ContentIdentity` over the canonical sample buffer. The five routes share one tag-keyed `scipy.stats` dispatch — the test entrypoint, the result-field projection, and one `StatReport.graded` reduction — behind the gated import, and the sample data admits through `numerics/array.md#PAYLOAD` keying on the same `ContentIdentity` seed. The reject rule is the one defect this owner refuses to fragment: a `Decision` policy value owns both regimes — `SIGNIFICANCE` rejects when a p-value falls below `alpha`, `CRITICAL` rejects when the statistic exceeds the interpolated critical level — so the criterion slot carries one typed yardstick per route instead of the field overload where a p-value column smuggles `alpha` for the critical-value route. Like `optimization/program.md#PROGRAM`, this owner carries **no numpy floor**: the hypothesis test *is* `scipy.stats`, so a cp315 run without the scipy wheel returns `Error(Import)` rather than a degraded estimate. Columnar and gridded statistical aggregation stays in the `data` branch gridded/field owner; this owner operates on an in-memory sample array only and never re-catalogues a grouped-reduction or labelled-array aggregation.

## [01]-[INDEX]

- [01]-[STATISTICS]: two-sample KS / Anderson-Darling / Shapiro-Wilk / Mann-Whitney U hypothesis tests plus MLE distribution fit over `scipy.stats` folding one `StatReport` on the one `Statistics` owner.

## [02]-[STATISTICS]

- Owner: `Statistics` — the test-and-fit cases discriminated by the statistical question recoverable from the sample shape and the named distribution, never a parallel per-test surface; `TwoSampleKS(a, b)` over `scipy.stats.ks_2samp`, `AndersonDarling(x, dist)` over `scipy.stats.anderson` (the reference distribution is the narrower `Goodness` vocabulary `anderson` accepts, never the full fit `Distribution` set) reading the statistic against the per-significance critical-value ladder, `ShapiroWilk(x)` over `scipy.stats.shapiro`, `MannWhitneyU(a, b, alternative)` over `scipy.stats.mannwhitneyu` threading the `Alternative` side, and `Fit(x, dist)` over the frozen `<dist>.fit(data)` maximum-likelihood estimator with the fitted enclosure scored back through `ks_2samp` and its `.stats(moments="mv")` mean/variance read as the fitted-distribution moments. The five routes share one tag-keyed dispatch in `_stat_report` — the `scipy.stats` entrypoint, the `statistic`/`criterion` projection, and one `StatReport.graded` reduction are the row behind the gated import, never five parallel helper bodies, never an inline verdict branch beside a shared static method. `Distribution` is the MLE-fittable continuous family and `Goodness` is the strictly narrower Anderson-Darling reference set (`scipy.stats.anderson` rejects any distribution outside `norm`/`expon`/`logistic`/`gumbel_l`/`gumbel_r`/`weibull_min`), so the AD intent cannot carry a distribution the route would raise on — two bounded vocabularies for two distinct admissible domains, never one over-wide enum shared across both; `Alternative` is the two-sided/less/greater rank-test side as a bounded vocabulary, never a string knob; `Decision` is the reject-rule regime as a policy value carrying its own `reject(statistic, criterion, alpha)` algebra, never a per-route inline comparison; `Verdict` is the reject/retain outcome the decision grades, never a boolean flag.
- Entry: `Statistics.test` enters one `boundary(f"stat.{intent.tag}", ...)`; every route reads the named `scipy.stats` result and folds through `StatReport.graded`, which carries the typed `Tag`, the `Decision`, the `statistic`, the route `criterion`, the graded `Verdict`, the fitted parameter tuple (empty for a pure hypothesis test), the fitted `moments` (`Nothing` for a pure hypothesis test), and the `ContentIdentity.of` content key. The `TwoSampleKS`/`ShapiroWilk`/`MannWhitneyU` routes read `result.statistic`/`result.pvalue` and grade under `Decision.SIGNIFICANCE` (the p-value is the criterion); the `AndersonDarling` route reads `result.statistic` against the `result.critical_values`/`result.significance_level` ladder interpolated at `alpha` and grades under `Decision.CRITICAL` (the critical level is the criterion); the `Fit` route reads the MLE parameter tuple, freezes the distribution at the estimate, draws a parametric sample through `<dist>(*params).rvs(size)`, scores the empirical-against-parametric `ks_2samp` for the goodness-of-fit statistic/p-value criterion under `Decision.SIGNIFICANCE`, and reads the frozen `.stats(moments="mv")` mean/variance into the `moments` slot.
- Receipt: `StatReport.contribute` emits one `Receipt.of("emitted", "compute.statistics", ...)` row carrying the test tag, the decision regime, the statistic, the criterion, the verdict, the fitted parameters, and the fitted moments; the report graduates outward through `graduation/handoff.md#GRADUATION` on the existing `solver` `HandoffAxis` case — no new literal, no graduation edit — as the distributional-evidence sibling of the optimization and validated-numerics receipts on the one rail.
- Packages: `scipy` (`stats.ks_2samp`, `stats.anderson`, `stats.shapiro`, `stats.mannwhitneyu`, the frozen `stats.norm`/`stats.lognorm`/`stats.gamma`/`stats.beta`/`stats.t`/`stats.chi2`/`stats.expon`/`stats.weibull_min` continuous distributions and their `.fit`/`.rvs`/`.stats`, `stats.rv_continuous` base — all catalogued in `compute/.api/scipy.md`'s `scipy.stats` entrypoint and public-type tables), `numpy` (`asarray`, `ascontiguousarray`, `interp` — the canonical sample buffer and the critical-value-ladder interpolation), `expression` (`Option`/`Some`/`Nothing` — the fitted-moments slot, present only for the MLE fit), `numerics/array.md#PAYLOAD` (the sample arrays admit as an `ArrayPayload` keying through the same `ContentIdentity.of` seed), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new hypothesis test is one `TestIntent` case plus one row in the `_stat_report` route table folding the shared `StatReport` through `graded`; a new fittable distribution is one `Distribution` row resolving its frozen `scipy.stats` object, and a new Anderson-Darling reference is one `Goodness` row only when `scipy.stats.anderson` documents it; a new rank-test side is one `Alternative` row; a new reject regime is one `Decision` row carrying its own `reject` rule, never a per-route inline comparison; zero new surface, never a per-test owner, never a parallel fit-and-test struct, never a per-route `_*_report` helper body, never a second statistics receipt beside `StatReport`, never a criterion field that overloads its meaning per route.
- Boundary: in-memory classical statistics over `scipy.stats` only — the two-sample KS test, the Anderson-Darling goodness-of-fit, the Shapiro-Wilk normality test, the Mann-Whitney U rank-sum test, and the maximum-likelihood distribution fit on a sample array are in-scope. Columnar and gridded statistical aggregation — grouped reductions, rolling windows, per-cell/per-band summary statistics over a labelled or gridded array — is the `data` branch gridded/field owner; this owner reads an in-memory sample only and never re-catalogues a grouped-reduction or labelled-array aggregation, mirroring the `numerics/array.md#PAYLOAD` rule that the data-branch `xarray`/`dask` shapes are composed, never re-owned. This owner carries **no numpy floor**: the test *is* `scipy.stats` (the KS empirical-CDF supremum, the Anderson-Darling weighted statistic, the Shapiro-Wilk W, the Mann-Whitney U exact/asymptotic ladder, the MLE optimizer), so a cp315 run without the scipy wheel returns `Error(Import)` rather than a degraded estimate — the deliberate floor asymmetry against `numerics/interval.md#ENCLOSURE` (which carries a reachable numpy outward-rounding band), matching the no-floor math-program routes of `optimization/program.md#PROGRAM` and the no-floor hull/Delaunay routes of `analysis/spatial.md#QUERY`. Bayesian posterior inference stays on `experiments/inference.md#INFERENCE`, design-of-experiments sensitivity on `experiments/study.md#STUDY`, and a hand-rolled empirical-CDF, rank-sum, or W-statistic kernel `scipy.stats` owns are the deleted forms.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import Nothing, Option, Some, case, default_arg, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

type Tag = Literal["two_sample_ks", "anderson", "shapiro", "mannwhitneyu", "fit"]


class Distribution(StrEnum):
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
    CRITICAL = "critical"          # reject H0 when the statistic exceeds the interpolated critical value

    def reject(self, statistic: float, criterion: float, alpha: float) -> bool:
        return statistic > criterion if self is Decision.CRITICAL else criterion < alpha


class Verdict(StrEnum):
    REJECT = "reject"
    RETAIN = "retain"


class StatReport(Struct, frozen=True):
    test: Tag
    decision: Decision
    statistic: float
    criterion: float             # the route's reject yardstick: a p-value (SIGNIFICANCE) or a critical level (CRITICAL)
    verdict: Verdict
    parameters: tuple[float, ...]
    moments: Option[tuple[float, float]]  # fitted (mean, variance) for the MLE fit, Nothing for a pure hypothesis test
    content_key: ContentKey

    @staticmethod
    def graded(
        test: Tag, decision: Decision, statistic: float, criterion: float, alpha: float,
        key: ContentKey, *, parameters: tuple[float, ...] = (), moments: Option[tuple[float, float]] = Nothing,
    ) -> "StatReport":
        verdict = Verdict.REJECT if decision.reject(statistic, criterion, alpha) else Verdict.RETAIN
        return StatReport(test, decision, statistic, criterion, verdict, parameters, moments, key)

    def contribute(self) -> Receipt:
        facts = {
            "test": self.test,
            "decision": self.decision.value,
            "statistic": f"{self.statistic:.6g}",
            "criterion": f"{self.criterion:.6g}",
            "verdict": self.verdict.value,
            "parameters": repr(self.parameters),
            "moments": repr(default_arg(self.moments, ())),
        }
        return Receipt.of("emitted", "compute.statistics", self.test, facts)


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


def _stat_key(intent: TestIntent, samples: tuple[np.ndarray, ...]) -> ContentKey:
    buffer = b"".join(np.ascontiguousarray(sample, dtype=float).tobytes() for sample in samples)
    return ContentIdentity.of(f"stat.{intent.tag}", buffer, IdentityPolicy())


def test(intent: TestIntent, *, alpha: float = 0.05, fit_sample: int = 4096) -> "RuntimeRail[StatReport]":
    return boundary(f"stat.{intent.tag}", lambda: _stat_report(intent, alpha, fit_sample))


def _stat_report(intent: TestIntent, alpha: float, fit_sample: int) -> StatReport:
    from scipy import stats

    match intent:
        case TestIntent(tag="two_sample_ks", two_sample_ks=(a, b)):
            first, second = np.asarray(a, dtype=float), np.asarray(b, dtype=float)
            result = stats.ks_2samp(first, second)
            return StatReport.graded(
                "two_sample_ks", Decision.SIGNIFICANCE, float(result.statistic), float(result.pvalue),
                alpha, _stat_key(intent, (first, second)),
            )
        case TestIntent(tag="anderson", anderson=(x, dist)):
            sample = np.asarray(x, dtype=float)
            result = stats.anderson(sample, dist=dist.value)
            level = float(np.interp(alpha * 100.0, result.significance_level[::-1], result.critical_values[::-1]))
            return StatReport.graded(
                "anderson", Decision.CRITICAL, float(result.statistic), level, alpha, _stat_key(intent, (sample,)),
            )
        case TestIntent(tag="shapiro", shapiro=x):
            sample = np.asarray(x, dtype=float)
            result = stats.shapiro(sample)
            return StatReport.graded(
                "shapiro", Decision.SIGNIFICANCE, float(result.statistic), float(result.pvalue),
                alpha, _stat_key(intent, (sample,)),
            )
        case TestIntent(tag="mannwhitneyu", mannwhitneyu=(a, b, alternative)):
            first, second = np.asarray(a, dtype=float), np.asarray(b, dtype=float)
            result = stats.mannwhitneyu(first, second, alternative=alternative.value)
            return StatReport.graded(
                "mannwhitneyu", Decision.SIGNIFICANCE, float(result.statistic), float(result.pvalue),
                alpha, _stat_key(intent, (first, second)),
            )
        case TestIntent(tag="fit", fit=(x, dist)):
            sample = np.asarray(x, dtype=float)
            frozen = getattr(stats, dist.value)
            params = tuple(float(p) for p in frozen.fit(sample))
            estimate = frozen(*params)
            gof = stats.ks_2samp(sample, estimate.rvs(size=fit_sample))
            mean, var = estimate.stats(moments="mv")
            return StatReport.graded(
                "fit", Decision.SIGNIFICANCE, float(gof.statistic), float(gof.pvalue), alpha,
                _stat_key(intent, (sample,)), parameters=params, moments=Some((float(mean), float(var))),
            )
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

- [SCIPY_STATS]: the `scipy.stats.ks_2samp(data1, data2)`, `anderson(x, dist)`, `shapiro(x)`, and `mannwhitneyu(x, y, alternative)` spellings — each returning a named result carrying `.statistic`/`.pvalue` (the Anderson result instead carrying `.statistic`/`.critical_values`/`.significance_level`) — and the frozen continuous distributions (`norm`/`lognorm`/`gamma`/`beta`/`t`/`chi2`/`expon`/`weibull_min`) with `.fit(data)`/`.rvs(size)`/`.stats(moments="mv")` are fully catalogued in `compute/.api/scipy.md`'s `scipy.stats` entrypoint table and the `rv_continuous` public-type row, so the body verifies against the present catalogue directly. The spellings carry the `python_version<'3.15'` scipy marker and settle against the installed wheel under a uv-sync reflection pass on that gated companion band; this owner carries no numpy floor because `scipy.stats` is the gated capability itself, so a cp315 run without the scipy wheel returns `Error(Import)` for every route — the deliberate floor asymmetry against `numerics/interval.md#ENCLOSURE`.
- [STAT_DECISION]: the reject rule is one `Decision` policy vocabulary, not a static `Verdict.of` beside an inline per-route branch — `SIGNIFICANCE` rejects when the criterion (a p-value) falls below `alpha`, `CRITICAL` rejects when the statistic exceeds the interpolated critical level — so the `criterion` slot of `StatReport` carries one typed yardstick per route (a p-value for the four significance routes, the `np.interp`-interpolated `result.critical_values`/`result.significance_level` level for Anderson-Darling) and the field never overloads its meaning, the defect where the prior shape stored `alpha` in the p-value column for the critical-value route. `StatReport.graded` is the one verdict reduction every route folds through; the `Fit` route additionally reads the frozen estimate's `.stats(moments="mv")` mean/variance into the `Option`-shaped `moments` slot, `Nothing` for the four pure hypothesis tests, so the MLE fit carries the fitted-distribution moments as real distributional evidence beyond the re-scored goodness-of-fit p-value.
- [STAT_CONTENT_KEY]: `_stat_key` derives the `ContentKey` over the canonical contiguous sample buffer (the concatenated `tobytes()` of the one or two `float64` sample arrays) through `ContentIdentity.of("stat.<tag>", ...)` under the runtime `IdentityPolicy`, so a sample whose data admits through `numerics/array.md#PAYLOAD` keys under the same `ContentIdentity` seed algebra and a repeated test on identical data is a cache hit by reference, matching the `optimization/program.md#PROGRAM` problem-data key algebra. The `Distribution`/`Alternative` discriminants are excluded from the key buffer — only the array-shaped sample data seeds identity — so the buffer derivation stays uniform with the payload admission's host-transfer buffer.
