# [PY_COMPUTE_STUDY]

The one study-spine owner over design-of-experiments sampling, global sensitivity analysis, and surrogate fitting. `Study` discriminates by a `StudyMethod` axis over one param-axis and sample-grid spine, and the union OWNS its fold: `StudyMethod.design`/`discrepancy`/`indices` are three total `match` case methods taking `axes` as payload — the inference `Distribution.declare` and model `ValidationCheck.run` discipline — never a detached free function reaching back into the union. `Study.run` weaves the one `study.run` span over the `boundary` fault fence, the `@beartype(conf=FAULT_CONF)`-guarded `_execute`, and the `@receipted(_REDACTION)` egress aspect into a single rail the way `experiments/inference.md#BAYESIAN` and `experiments/model.md#ASSET` do, so the run is a traced, fault-railed, receipted leg and receipt emission is the decorator rail `observability/receipts.md#RECEIPT` declares, never an inline `Signals.emit` threaded through the body.

Four polymorphic surfaces carry every variation. `StudyMethod` is the `@tagged_union` over the `scipy.stats.qmc` `Sobol`/`Halton`/`LatinHypercube` engines, the SALib sensitivity family (Sobol, Morris, FAST, RBD-FAST, delta, PAWN, DGSM, HDMR) composed through one `ProblemSpec` fluent pipeline, and the `numpy.polynomial`-and-scikit-learn surrogate band — every case a row on one owner, with the eight SALib methods collapsed to one `SALIB_ROUTES` `Map[SalibTag, SalibRoute]` table whose named `sample_module`/`analyze_module`/`result_key`/`needs_design` fields drive one routed sampler body and one routed analyzer body, the per-method knobs (`morris` `num_levels`) folded from the case payload through `_salib_args` rather than a hardcoded arm beside the table. `Objective` is the input-shape-parameterized carrier — a per-row `RowScorer` plus an `Option[BatchScorer]` vectorized fast lane — so a `numpy`-ufunc or SALib-`evaluate`-style matrix objective rides one batch call rather than the per-row loop. `MeasurementMode` is a live discriminant whose `evaluate` folds the design evaluation through `_timed` into one `Measured` value object carrying the responses, the wallclock, and the `Option[float]` batch-versus-serial speedup that is `Nothing` for a bare row objective rather than a fabricated ~1.0 ratio over the identical per-row work timed twice. `AxisDist` is the per-axis input-marginal vocabulary: a non-uniform input is one marginal declared into `problem['dists']` for the SALib samplers and inverse-transformed on the qmc path through `scipy.stats.<dist>.ppf`, never a caller pre-transform. SALib owns sensitivity analysis, so the owner composes its sampler-and-analyzer pair through `ProblemSpec.sample`/`analyze` across every method rather than reimplementing variance-based, moment-independent, derivative-based, or component sensitivity; the numpy full-factorial and `numpy.polynomial` surrogate floors run on runtime.

## [01]-[INDEX]

- [01]-[STUDY]: DOE sampling, SALib sensitivity, surrogate fitting, the union-owned `design`/`discrepancy`/`indices` folds, the `SalibRoute` route table, the live `Measured` measurement discriminant, and the traced/fault-railed/receipted `study.run` weave on one `Study` owner.

## [02]-[STUDY]

- Owner: `Study` — the ONE study-lake owner discriminating by a `StudyMethod` axis over the param-axis, sample-grid, objective-route, and measurement spine; DOE sampling, global sensitivity, and surrogate fitting are cases on one owner. The `StudyMethod` union owns its three folds (`design`/`discrepancy`/`indices`), so `Study` carries no detached `_design`/`_indices` free function and `experiments/history.md#RUN_HISTORY` composes `study.method.design(study.axes, seed)`/`study.method.indices(study.axes, design, responses)` directly rather than importing a `_`-prefixed private across the package seam. `RunHistory` rides the same spine for persistence and resume; the benchmark concern is the live `MeasurementMode` discriminant folded into `Measured`, never a parallel benchmark owner.
- Cases: `StudyMethod` discriminates the DOE samplers (`StudyMethod(lhs=n)`, `StudyMethod(sobol=n)`, and `StudyMethod(halton=n)` over the one `scipy.stats.qmc.LatinHypercube`/`Sobol`/`Halton` engine family with the `ParamAxis.rescale` box-map-or-`ppf` fold and the `qmc.discrepancy` uniformity score, `StudyMethod(factorial=levels)` numpy full-factorial floor), the SALib sensitivity analyzers (`StudyMethod(morris_screen=(trajectories, levels))`, `StudyMethod(sobol_indices=n)`, `StudyMethod(fast=n)`, `StudyMethod(rbd_fast=n)`, `StudyMethod(delta=n)`, `StudyMethod(pawn=n)`, `StudyMethod(dgsm=n)`, `StudyMethod(hdmr=n)`), and the surrogates (`StudyMethod(polynomial=degree)` over the `numpy.polynomial.Polynomial.fit` per-axis least-squares floor reading in-sample `R^2`, `StudyMethod(surrogate=kind)` over the scikit-learn estimator protocol reading the cross-validated `R^2`). The union's keyword constructor is the one construction surface, no sibling factory family. The eight SALib analyzers are not eight bodies: `SALIB_ROUTES` carries one `SalibRoute` value object per tag — `sample_module`/`analyze_module`/`result_key`/`needs_design` read by name, with `route.sampler()`/`route.analyzer()` resolving `SALib.sample.<module>`/`SALib.analyze.<module>` — so `StudyMethod.design` runs one routed sampler body and `StudyMethod.indices` one routed analyzer body across all eight methods including Morris, reads the row's `ResultDict` key, and `set_samples(design)` only when `needs_design` (the Morris/RBD-FAST/delta/PAWN/DGSM/HDMR `X`-and-`Y` analyzers) versus the responses alone (the Sobol/FAST `Y`-only analyzers over their structured design). The per-method sampler/analyzer knobs ride the case payload: `_salib_args` folds `morris_screen=(traj, levels)` to the `(traj, {"num_levels": levels}, {"num_levels": levels})` triple so the SAME `num_levels` threads `morris.sample` and `morris.analyze` — the elementary-effect grid and the index reconstruction agree — while the single-`int` analyzers fold to `(n, {}, {})`, no per-method sampler arm beside the table. `MeasurementMode.evaluate` folds to one `Measured` carrier through the shared `_timed` primitive: `RESULT`/`WALLCLOCK` and the bare `SPEEDUP` (no batch lane) collapse to one timed-`fast` arm differing only in whether the wallclock reports, while `SPEEDUP` with a real `Objective.batch` lane earns the second timed pass folding the honest serial-over-batch ratio — never a dead label, never a positional `(responses, elapsed, speedup)` tuple, never the fabricated ~1.0 ratio of timing the per-row loop twice. Each `ParamAxis` carries one `AxisDist` marginal plus its `params` parameter vector matching the `problem['dists']` distribution's `scipy.stats` parameterization exactly (`unif` `(low, high)`, `norm`/`lognorm` 2-param, `triang` 3-param `(start, end, peak)`, `truncnorm` 4-param `(lower, upper, mean, std)`): `_spec` emits each `bounds` row as the dist's full `params` vector and the non-uniform tags into `problem['dists']` so the SALib samplers shape their own inputs, and `ParamAxis.rescale` reproduces the EXACT `scipy.stats.<dist>.ppf` inverse-transform per dist (`triang.ppf(x, c=peak, loc=start, scale=end-start)`, `exp(norm.ppf(x, ln_mean, ln_std))`, `truncnorm.ppf(x, (lower-mean)/std, (upper-mean)/std, loc=mean, scale=std)`) so the qmc draw and the SALib `dists`-shaped draw are one marginal, never a 2-tuple overload silently truncating the 3- and 4-param vectors into a divergent standard distribution.
- Entry: `Study.run` opens the one `study.run` `opentelemetry-api` span behind an `is_recording()` attribute gate, runs `boundary(f"study.{tag}", lambda: Study._emit(self._execute(objective, seed)))` inside it, and on the `Ok` arm batches `span_facts` behind the `is_recording()` gate and sets `Status(StatusCode.OK)`; the `Error` arm needs no body because the fence's `_convert` already recorded the cause and set `StatusCode.ERROR` on the active span — the same span-then-fence-then-receipt weave `experiments/inference.md#BAYESIAN` and `experiments/model.md#ASSET` hold, so the trace status and the rail outcome never disagree. `_emit` is the `@receipted(_REDACTION)` static aspect harvesting the receipt's `contribute` stream on exit, so the body threads no inline `Signals.emit`. `_execute` is the `@beartype(conf=FAULT_CONF)`-guarded body: it draws the design through `StudyMethod.design`, evaluates the `Objective` under `MeasurementMode.evaluate` into one `Measured` (the batch lane when present, the per-row stack otherwise), `match`es the design `ContentKey` off the railed `ContentIdentity.of` so a hash `Error` re-raises onto the `boundary`, and folds `StudyReceipt.graded` carrying the method, mode, the `design_cells` row count and the `response_width` per-cell output arity, the per-axis sensitivity indices, the `qmc.discrepancy` design-uniformity score, the elapsed wallclock and `Option[float]` batch speedup, and the design `ContentKey`. The `ProblemSpec` sampler-and-analyzer pipeline is the canonical sensitivity owner, so Morris elementary effects, Saltelli first-and-total-order Sobol indices, RBD-FAST random-balance indices, and Borgonovo delta moment-independent measures are read from SALib, never hand-rolled.
- Receipt: `StudyReceipt.contribute` returns the one-element `tuple[Receipt, ...]` the `ReceiptContributor` port streams — `Receipt.of("compute.study", ("emitted", self.method, facts))` against the runtime two-argument `of(owner, evidence)` contract, never a four-positional call and never a single-`Receipt` return against the `Iterable[Receipt]` port. Native `float` elapsed, discrepancy, speedup, and per-axis indices ride the `EventDict` `dict[str, object]` slots the receipts owner's `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce; `span_facts` is the bounded `str | int | float` scalar source the span reads, the full `indices` ledger riding the receipt facts only.
- Packages: `SALib` (`ProblemSpec` fluent `sample`/`set_samples`/`set_results`/`analyze` with the `samples`/`analysis` accessors, the `SALib.sample.<morris|sobol|fast_sampler|finite_diff|latin>` and `SALib.analyze.<morris|sobol|fast|rbd_fast|delta|pawn|dgsm|hdmr>` submodules resolved through `importlib.import_module`), `scipy` (`stats.qmc.LatinHypercube`/`Sobol`/`Halton` with the SPEC-007 `rng` constructor keyword and `qmc.discrepancy`, the `stats.norm`/`triang`/`truncnorm` `.ppf` percent-point functions inverse-transforming the unit draw to a declared `AxisDist` marginal over the exact `problem['dists']` `scipy.stats` parameterization, with `lognorm` as `exp(norm.ppf(x, ln_mean, ln_std))`), `scikit-learn` (the `pipeline.Pipeline` of `preprocessing.StandardScaler` and a `SurrogateKind`-resolved `base.BaseEstimator` regressor `ensemble.GradientBoostingRegressor`/`ensemble.RandomForestRegressor`/`svm.SVR`/`linear_model.Ridge`, scored through `model_selection.cross_val_score` reading `metrics.r2_score`), `numpy` (`linspace`, `meshgrid`, `stack`, `asarray`, `where`, `exp`, `polynomial.Polynomial.fit`), `expression` (`tagged_union`/`case`/`tag`, `Ok`/`Error`/`Some`/`Nothing`/`Option` with `Option.map`/`default_value`/`is_some` the `Objective.batch` fast-lane fold, the `Map` route/class tables), `msgspec` (`Struct`, `gc=False` on the container-free `SalibRoute`/`Measured` leaves, the `params`-`tuple`-carrying `ParamAxis` and the `RowScorer`+`Option[BatchScorer]`-carrying `Objective` GC-tracked exactly as `experiments/inference.md#BAYESIAN` keeps its `hdi`-tuple `PosteriorSummary`), `beartype` (`@beartype(conf=FAULT_CONF)` fencing the `_execute` body so a contract violation folds onto the rail through the `CLASSIFY` `api` row), `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span`/`Span.set_attributes`/`is_recording`/`set_status`/`Status`/`StatusCode` the one `study.run` span), `time.perf_counter` folded once through the `_timed` measurement primitive the `MeasurementMode.evaluate` arms share, runtime (`FAULT_CONF`/`RuntimeRail`/`boundary`, the railed `ContentIdentity.of`/`ContentKey` over the `CANONICAL_POLICY` default, `Receipt`/`Redaction`/`receipted`/`ReceiptContributor` the `@receipted(_REDACTION)` egress aspect).
- Growth: a new param axis is one `ParamAxis` coordinate; a new input marginal is one `AxisDist` member plus one `rescale` `.ppf` arm and one `bounds` arm matching its `problem['dists']` `scipy.stats` parameterization, reaching the SALib `dists` channel and the qmc inverse-transform by that single add over the dist's `params` vector; a new SALib analyzer is one `StudyMethod` case plus one `SALIB_ROUTES` `SalibRoute` row, no new body; a new `qmc` engine or numpy floor is one arm on `StudyMethod._qmc`/`design`; a new surrogate estimator is one `SurrogateKind` member plus one `SURROGATE_CLASS` row resolving its scikit-learn class; a new measurement is one `MeasurementMode` member reading the shared `Measured` fold; zero new surface.

```python signature
import time
from collections.abc import Callable, Iterable
from enum import StrEnum
from importlib import import_module
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted

if TYPE_CHECKING:
    from SALib import ProblemSpec
    from sklearn.base import BaseEstimator

# --- [TYPES] ----------------------------------------------------------------------------

# the objective is parameterized over input shape through a tagged carrier rather than an erased
# `Callable -> Callable` union (both shapes are the same `function` runtime type, so a structural
# `match`/`isinstance` cannot discriminate them — that is the untyped-carrier defect). `Objective` wraps
# a per-row scorer and an optional vectorized `(rows, dim) -> (rows, ...)` fast lane (a numpy ufunc or a
# SALib `evaluate`-style model): `rows(design)` is the always-available serial stack and `Objective.batch`
# the `Option`-typed fast path the SPEEDUP ratio measures against it, `Nothing` when no batch lane exists
# rather than a fabricated ~1.0 ratio over the identical per-row work timed twice.
# the row scorer returns a scalar or a per-output vector, so `response_width` is a real arity: SALib
# analyzers and the `numpy.polynomial`/scikit-learn surrogate floors require the scalar `(rows,)` shape,
# while the multi-output `(rows, k)` shape is admitted on the qmc/factorial sampling-only methods whose
# `indices` fold returns `{}`. A multi-output objective under a SALib/surrogate method rails on the
# analyzer's own 1-D `Y` contract inside the `@beartype`-fenced body rather than silently averaging.
type RowScorer = Callable[[np.ndarray], float | np.ndarray]
type BatchScorer = Callable[[np.ndarray], np.ndarray]
type SalibTag = Literal["morris_screen", "sobol_indices", "fast", "rbd_fast", "delta", "pawn", "dgsm", "hdmr"]


class Objective(Struct, frozen=True):
    row: RowScorer
    batch: Option[BatchScorer] = Nothing  # vectorized fast lane; absent objectives score per-row only

    def rows(self, design: np.ndarray) -> np.ndarray:
        return np.stack([np.asarray(self.row(point), dtype=float) for point in design])


def _timed[T](thunk: Callable[[], T]) -> tuple[T, float]:
    # the one perf_counter fold the measurement arms share, so the `start`/`perf_counter() - start`
    # triple lives once rather than once per arm; the thunk runs after the start mark is taken.
    start = time.perf_counter()
    value = thunk()
    return value, time.perf_counter() - start


class MeasurementMode(StrEnum):
    RESULT = "result"  # evaluate the design but suppress the wallclock; report zero elapsed
    WALLCLOCK = "wallclock"  # time the whole design evaluation as one batch
    SPEEDUP = "speedup"  # fold the vectorized batch wallclock against the per-row serial baseline

    def evaluate(self, objective: Objective, design: np.ndarray) -> "Measured":
        # `fast` is the batch lane when present, the per-row stack otherwise; both are thunks so `_timed`
        # wraps exactly the chosen evaluation. `RESULT`/`WALLCLOCK` and the bare `SPEEDUP` (no batch lane)
        # collapse to one timed-`fast` arm differing only in whether `_timed` reports the wallclock, since
        # timing the per-row loop twice is the fabricated ~1.0 ratio the SPEEDUP fold rejects. Only a real
        # `objective.batch` lane earns the second timed pass that folds the honest serial-over-batch ratio.
        fast: Callable[[], np.ndarray] = objective.batch.map(lambda b: lambda: b(design)).default_value(lambda: objective.rows(design))
        match self:
            case MeasurementMode.SPEEDUP if objective.batch.is_some():
                responses, elapsed = _timed(fast)
                baseline = _timed(lambda: objective.rows(design))[1]
                return Measured(responses, elapsed, Some(baseline / elapsed) if elapsed > 0.0 else Nothing)
            case MeasurementMode.RESULT | MeasurementMode.WALLCLOCK | MeasurementMode.SPEEDUP:
                responses, elapsed = _timed(fast)
                return Measured(responses, elapsed if self is not MeasurementMode.RESULT else 0.0, Nothing)
            case _ as unreachable:
                assert_never(unreachable)


class SurrogateKind(StrEnum):
    GRADIENT_BOOST = "gradient_boost"  # ensemble.GradientBoostingRegressor
    RANDOM_FOREST = "random_forest"  # ensemble.RandomForestRegressor
    SVR = "svr"  # svm.SVR
    RIDGE = "ridge"  # linear_model.Ridge

    def estimator(self) -> "BaseEstimator":
        module, name = SURROGATE_CLASS[self]
        return getattr(import_module(f"sklearn.{module}"), name)()


# the SALib `problem['dists']` parameter vector verbatim: each member's `params` arity and meaning match
# the `scipy.stats.<dist>` parameterization the `dists` channel resolves, so a single axis declares one
# marginal both the qmc-path `rescale` ppf and the SALib `dists` channel read identically — never a 2-tuple
# overload that silently truncates the 3-param `triang` or 4-param `truncnorm` vector into a divergent marginal.
class AxisDist(StrEnum):
    UNIF = "unif"  # params (low, high) bounds
    NORM = "norm"  # params (mean, std)
    LOGNORM = "lognorm"  # params (ln_mean, ln_std) of the underlying normal
    TRIANG = "triang"  # params (start, end, peak_fraction in [0, 1])
    TRUNCNORM = "truncnorm"  # params (lower, upper, mean, std)


class ParamAxis(Struct, frozen=True):
    name: str
    params: tuple[float, ...]  # SALib `dists` parameter vector; per-dist arity raises on the `rescale`/`bounds` unpack inside the fence
    dist: AxisDist = AxisDist.UNIF
    # No `gc=False`: `params` is a `tuple` container field, so the leaf-only opt-out the `SalibRoute`/
    # `Measured` rows take does not apply, exactly as `experiments/inference.md#BAYESIAN` keeps its
    # `hdi`-tuple-carrying `PosteriorSummary` GC-tracked.

    # the `bounds` row the SALib `problem` dict and the qmc discrepancy de-scale read: the support endpoints
    # the sampler scales into. `unif` is the param pair itself; `triang` spans `(start, end)`; `truncnorm`
    # spans its `(lower, upper)`; `norm`/`lognorm` are unbounded so SALib reads a wide `±` window off mean±std.
    @property
    def bounds(self) -> tuple[float, float]:
        match self.dist:
            case AxisDist.UNIF | AxisDist.TRIANG | AxisDist.TRUNCNORM:
                return self.params[0], self.params[1]  # explicit support endpoints lead the param vector
            case AxisDist.NORM:
                mean, std = self.params
                return mean - 4.0 * std, mean + 4.0 * std
            case AxisDist.LOGNORM:
                mean, std = self.params
                return float(np.exp(mean - 4.0 * std)), float(np.exp(mean + 4.0 * std))
            case _ as unreachable:
                assert_never(unreachable)

    # the qmc engines draw a raw unit-cube column; the SALib samplers shape their own marginals off the
    # problem `dists`. On the qmc path a non-`unif` axis inverse-transforms the unit draw through the exact
    # `scipy.stats.<dist>.ppf` form the SALib `problem['dists']` channel resolves, so the qmc draw and the
    # SALib `dists`-shaped draw are the same marginal rather than two divergent inverse transforms.
    def rescale(self, unit_col: np.ndarray) -> np.ndarray:
        from scipy import stats

        match self.dist:
            case AxisDist.UNIF:
                low, high = self.params
                return low + (high - low) * unit_col
            case AxisDist.NORM:
                mean, std = self.params
                return stats.norm.ppf(unit_col, loc=mean, scale=std)
            case AxisDist.LOGNORM:
                # SALib lognorm is `exp(norm.ppf(x, ln_mean, ln_std))`, NOT scipy's `lognorm` shape form.
                ln_mean, ln_std = self.params
                return np.exp(stats.norm.ppf(unit_col, loc=ln_mean, scale=ln_std))
            case AxisDist.TRIANG:
                start, end, peak = self.params
                return stats.triang.ppf(unit_col, c=peak, loc=start, scale=end - start)
            case AxisDist.TRUNCNORM:
                lower, upper, mean, std = self.params
                return stats.truncnorm.ppf(unit_col, (lower - mean) / std, (upper - mean) / std, loc=mean, scale=std)
            case _ as unreachable:
                assert_never(unreachable)


# the named route row replacing the four-positional `(sample, analyze, key, needs_design)` tuple
# an analyzer body would index by position: every fold reads `route.analyze_module`/`route.result_key`
# by name, and `needs_design` discriminates the paired `(X, Y)` analyzers from the `Y`-only routes.
class SalibRoute(Struct, frozen=True, gc=False):
    sample_module: str
    analyze_module: str
    result_key: str
    needs_design: bool

    def sampler(self) -> Callable[..., object]:
        return import_module(f"SALib.sample.{self.sample_module}").sample

    def analyzer(self) -> Callable[..., object]:
        return import_module(f"SALib.analyze.{self.analyze_module}").analyze


class Measured(Struct, frozen=True, gc=False):
    responses: np.ndarray
    elapsed: float
    speedup: Option[float]  # batch-versus-serial ratio under MeasurementMode.SPEEDUP, Nothing otherwise


@tagged_union(frozen=True)
class StudyMethod:
    tag: Literal[
        "lhs",
        "factorial",
        "sobol",
        "halton",
        "morris_screen",
        "sobol_indices",
        "fast",
        "rbd_fast",
        "delta",
        "pawn",
        "dgsm",
        "hdmr",
        "polynomial",
        "surrogate",
    ] = tag()
    lhs: int = case()
    factorial: tuple[int, ...] = case()
    sobol: int = case()
    halton: int = case()
    morris_screen: tuple[int, int] = case()
    sobol_indices: int = case()
    fast: int = case()
    rbd_fast: int = case()
    delta: int = case()
    pawn: int = case()
    dgsm: int = case()
    hdmr: int = case()
    polynomial: int = case()
    surrogate: SurrogateKind = case()

    # The union's own keyword constructor is the construction surface: `StudyMethod(lhs=n)`,
    # `StudyMethod(morris_screen=(traj, levels))`, `StudyMethod(surrogate=kind)` — exactly the
    # `experiments/inference.md#BAYESIAN` `Distribution(normal=(0.0, 1.0))` discipline, no parallel
    # sibling factory family re-wrapping each case.
    #
    # The union OWNS its three folds (`design`/`discrepancy`/`indices`), each a total `match` keyed
    # by `tag` and closed by `assert_never`, taking `axes` as payload exactly as `Distribution.declare`
    # takes its `name`/`observed` — there is no detached free function reaching back into the union.
    def design(self, axes: tuple[ParamAxis, ...], seed: int) -> np.ndarray:
        match self:
            case StudyMethod(tag="lhs" | "sobol" | "halton" | "polynomial" | "surrogate"):
                return self._qmc(axes, seed)
            case StudyMethod(tag="factorial", factorial=levels):
                grids = np.meshgrid(*[np.linspace(0.0, 1.0, k) for k in levels], indexing="ij")
                unit = np.stack([g.reshape(-1) for g in grids], axis=1)
                return np.stack([ax.rescale(unit[:, j]) for j, ax in enumerate(axes)], axis=1)
            case StudyMethod(tag="morris_screen" | "sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr" as t):
                n, sample_kwargs, _ = self._salib_args()
                return StudyMethod._spec(axes).sample(SALIB_ROUTES[t].sampler(), n, seed=seed, **sample_kwargs).samples
            case _ as unreachable:
                assert_never(unreachable)

    # `qmc.discrepancy` scores space-filling uniformity in the unit cube, so it is honest only over an
    # all-`unif` design where the affine de-scale recovers the unit draw; a declared non-`unif` marginal
    # shapes the design out of the uniform box and the score is `Nothing` rather than a misread.
    def discrepancy(self, axes: tuple[ParamAxis, ...], design: np.ndarray) -> Option[float]:
        match self:
            case StudyMethod(tag="lhs" | "sobol" | "halton" | "polynomial" | "surrogate") if all(ax.dist is AxisDist.UNIF for ax in axes):
                from scipy.stats import qmc

                lo = np.asarray([ax.bounds[0] for ax in axes], dtype=float)
                hi = np.asarray([ax.bounds[1] for ax in axes], dtype=float)
                return Some(float(qmc.discrepancy((design - lo) / np.where(hi > lo, hi - lo, 1.0))))
            case _:
                return Nothing

    def indices(self, axes: tuple[ParamAxis, ...], design: np.ndarray, responses: np.ndarray) -> dict[str, float]:
        names = [ax.name for ax in axes]
        match self:
            case StudyMethod(tag="morris_screen" | "sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr" as t):
                _, _, analyze_kwargs = self._salib_args()
                return StudyMethod._salib(axes, SALIB_ROUTES[t], design, responses, names, analyze_kwargs)
            case StudyMethod(tag="polynomial", polynomial=degree):
                return {ax.name: StudyMethod._axis_r2(design[:, j], responses, degree) for j, ax in enumerate(axes)}
            case StudyMethod(tag="surrogate", surrogate=kind):
                return {"cv_r2": StudyMethod._surrogate_cv(kind, design, responses)}
            case StudyMethod(tag="lhs" | "factorial" | "sobol" | "halton"):
                return {}
            case _ as unreachable:
                assert_never(unreachable)

    # the per-method SALib axis: every analyzer rides the one routed sampler/analyzer body, so the
    # method-specific knobs (`morris`/`fast`/`pawn`/`dgsm` shape parameters) live on the case payload
    # and reach both ends through one fold rather than a hardcoded sampler arm beside the route table.
    # `num_levels` MUST match across `morris.sample` and `morris.analyze` or the elementary-effect grid
    # and the index reconstruction disagree, so the same value threads both kwarg channels.
    def _salib_args(self) -> tuple[int, dict[str, object], dict[str, object]]:
        match self:
            case StudyMethod(tag="morris_screen", morris_screen=(traj, levels)):
                return traj, {"num_levels": levels}, {"num_levels": levels}
            case StudyMethod(tag="sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr" as t):
                return getattr(self, t), {}, {}
            case _ as unreachable:
                assert_never(unreachable)

    def _qmc(self, axes: tuple[ParamAxis, ...], seed: int) -> np.ndarray:
        from scipy.stats import qmc

        dim = len(axes)
        match self:
            case StudyMethod(tag="sobol", sobol=n):
                unit = qmc.Sobol(d=dim, scramble=True, rng=seed).random(n)
            case StudyMethod(tag="halton", halton=n):
                unit = qmc.Halton(d=dim, scramble=True, rng=seed).random(n)
            case StudyMethod(tag="lhs", lhs=n):
                unit = qmc.LatinHypercube(d=dim, scramble=True, rng=seed).random(n)
            case StudyMethod(tag="polynomial"):
                unit = qmc.LatinHypercube(d=dim, scramble=True, rng=seed).random(max(8, dim * 4))
            case StudyMethod(tag="surrogate"):
                unit = qmc.LatinHypercube(d=dim, scramble=True, rng=seed).random(max(16, dim * 8))
            case _ as unreachable:
                assert_never(unreachable)
        # one rescale path across the qmc and factorial floors: `ax.rescale` is the affine box map for a
        # `unif` axis and the `scipy.stats.<dist>.ppf` inverse-transform for a declared marginal, so
        # `qmc.scale` is not a second uniform-only scaling surface beside the distribution-aware fold.
        return np.stack([ax.rescale(unit[:, j]) for j, ax in enumerate(axes)], axis=1)

    @staticmethod
    def _spec(axes: tuple[ParamAxis, ...]) -> "ProblemSpec":
        from SALib import ProblemSpec

        # the SALib samplers shape their own marginals off `problem['dists']`, so a non-uniform input is
        # declared in the problem dict and never pre-transformed by the caller; the `dists` key is emitted
        # only when an axis is non-`unif` (an absent key reads as all-uniform). SALib reads each `bounds`
        # row as the dist's full parameter vector when `dists[i]` is set (`truncnorm` -> `[lower, upper,
        # mean, std]`, `triang` -> `[start, end, peak]`), so the row is `ax.params` itself, not `(low, high)`.
        problem: dict[str, object] = {"num_vars": len(axes), "names": [ax.name for ax in axes], "bounds": [list(ax.params) for ax in axes]}
        if any(ax.dist is not AxisDist.UNIF for ax in axes):
            problem["dists"] = [ax.dist.value for ax in axes]
        return ProblemSpec(problem)

    @staticmethod
    def _salib(
        axes: tuple[ParamAxis, ...], route: SalibRoute, design: np.ndarray, responses: np.ndarray, names: list[str], analyze_kwargs: dict[str, object]
    ) -> dict[str, float]:
        spec = StudyMethod._spec(axes).set_results(responses)
        feed = spec.set_samples(design) if route.needs_design else spec
        analysis = feed.analyze(route.analyzer(), **analyze_kwargs).analysis
        return {n: float(v) for n, v in zip(names, analysis[route.result_key], strict=True)}

    @staticmethod
    def _axis_r2(column: np.ndarray, responses: np.ndarray, degree: int) -> float:
        fit = np.polynomial.Polynomial.fit(column, responses, degree)
        residual = responses - fit(column)
        ss_res = float(residual @ residual)
        centered = responses - responses.mean()
        ss_tot = float(centered @ centered) or 1.0
        return 1.0 - ss_res / ss_tot

    @staticmethod
    def _surrogate_cv(kind: SurrogateKind, design: np.ndarray, responses: np.ndarray) -> float:
        from sklearn.model_selection import cross_val_score
        from sklearn.pipeline import Pipeline
        from sklearn.preprocessing import StandardScaler

        pipeline = Pipeline([("scale", StandardScaler()), ("model", kind.estimator())])
        return float(cross_val_score(pipeline, design, responses, cv=5, scoring="r2").mean())


# --- [CONSTANTS] ------------------------------------------------------------------------

_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # study facts carry no secret field

_TRACER: Final[trace.Tracer] = trace.get_tracer("compute.study")

# --- [TABLES] ---------------------------------------------------------------------------

# the eight analyzers collapse to one route row per tag driving one sampler body and one analyzer
# body: `sample_module` resolves `SALib.sample.<mod>`, `analyze_module` resolves `SALib.analyze.<mod>`,
# `result_key` selects the per-method scalar over the `ResultDict`, `needs_design` feeds the design
# matrix only for the paired (X, Y) analyzers versus the Y-only Sobol/FAST routes over their design.
SALIB_ROUTES: Final[Map[SalibTag, SalibRoute]] = Map.of_seq([
    ("morris_screen", SalibRoute("morris", "morris", "mu_star", True)),
    ("sobol_indices", SalibRoute("sobol", "sobol", "ST", False)),
    ("fast", SalibRoute("fast_sampler", "fast", "ST", False)),
    ("rbd_fast", SalibRoute("latin", "rbd_fast", "S1", True)),
    ("delta", SalibRoute("latin", "delta", "delta", True)),
    ("pawn", SalibRoute("sobol", "pawn", "median", True)),
    ("dgsm", SalibRoute("finite_diff", "dgsm", "dgsm", True)),
    (
        "hdmr",
        SalibRoute("latin", "hdmr", "S", True),
    ),  # per-input sensitivity (`S=Sa+Sb`, length num_vars); `ST` is the per-component-term total, longer than num_vars
])

SURROGATE_CLASS: Final[Map[SurrogateKind, tuple[str, str]]] = Map.of_seq([
    (SurrogateKind.GRADIENT_BOOST, ("ensemble", "GradientBoostingRegressor")),
    (SurrogateKind.RANDOM_FOREST, ("ensemble", "RandomForestRegressor")),
    (SurrogateKind.SVR, ("svm", "SVR")),
    (SurrogateKind.RIDGE, ("linear_model", "Ridge")),
])

# --- [MODELS] ---------------------------------------------------------------------------


class StudyReceipt(Struct, frozen=True):
    method: str
    mode: MeasurementMode
    design_cells: int  # evaluated design rows; the run is total (every row evaluates or rails)
    response_width: (
        int  # per-cell output arity, so a multi-output objective is a parameterized fact, not a cells_completed > cells_total contradiction
    )
    indices: dict[str, float]
    discrepancy: Option[float]  # qmc uniformity score for an all-unif qmc design, Nothing for SALib/factorial/non-unif
    elapsed: float
    speedup: Option[float]  # batch-versus-serial ratio under MeasurementMode.SPEEDUP, Nothing otherwise
    content_key: ContentKey

    @staticmethod
    def graded(study: "Study", design: np.ndarray, measured: Measured, key: ContentKey) -> "StudyReceipt":
        rows = int(design.shape[0])
        return StudyReceipt(
            study.method.tag,
            study.mode,
            rows,
            int(measured.responses.size // rows) if rows else 0,
            study.method.indices(study.axes, design, measured.responses),
            study.method.discrepancy(study.axes, design),
            measured.elapsed,
            measured.speedup,
            key,
        )

    @property
    def span_facts(self) -> dict[str, str | int | float]:
        # the bounded-scalar source the `study.run` span reads — exactly the `str | int | float` set
        # `Span.set_attributes` admits; the full `indices` ledger rides the receipt facts only.
        return {
            "method": self.method,
            "mode": self.mode.value,
            "design_cells": self.design_cells,
            "response_width": self.response_width,
            "content_key": self.content_key.hex,
        }

    def contribute(self) -> Iterable[Receipt]:
        # the runtime `Receipt.of(owner, evidence)` two-argument contract: the `(Phase, subject, facts)`
        # triple mints `fact` at `emitted`. Native `float` indices ride the `EventDict` `dict[str, object]`
        # slots the `enc_hook=repr` renderer serializes without a `str()` coerce.
        facts: dict[str, object] = {
            "mode": self.mode.value,
            "design_cells": self.design_cells,
            "response_width": self.response_width,
            "elapsed": self.elapsed,
            "discrepancy": self.discrepancy.to_optional(),
            "speedup": self.speedup.to_optional(),
            **{f"S[{k}]": v for k, v in self.indices.items()},
        }
        return (Receipt.of("compute.study", ("emitted", self.method, facts)),)


# --- [SERVICES] -------------------------------------------------------------------------


class Study(Struct, frozen=True):
    axes: tuple[ParamAxis, ...]
    method: StudyMethod
    mode: MeasurementMode

    def run(self, objective: Objective, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        # one woven rail: the `study.run` span wraps the `boundary` fence over the
        # `@beartype(conf=FAULT_CONF)`-guarded `_execute` and the `@receipted(_REDACTION)` egress
        # aspect, so a sampler/analyzer/fit raise folds onto the rail rather than escaping; the
        # `Ok` arm batches `span_facts` behind an `is_recording()` gate and sets a total `Status`,
        # while the `Error` arm needs no body — the fence's `_convert` already recorded the cause and
        # set ERROR on the active span, exactly the span-then-fence-then-receipt weave
        # `experiments/inference.md#BAYESIAN` and `experiments/model.md#ASSET` hold.
        with _TRACER.start_as_current_span("study.run") as span:
            if span.is_recording():
                span.set_attributes({"study.method": self.method.tag, "study.mode": self.mode.value})
            rail = boundary(f"study.{self.method.tag}", lambda: self._emit(self._execute(objective, seed)))
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
    def _emit(receipt: StudyReceipt) -> StudyReceipt:
        return receipt

    @beartype(conf=FAULT_CONF)
    def _execute(self, objective: Objective, seed: int) -> StudyReceipt:
        design = self.method.design(self.axes, seed)
        measured = self.mode.evaluate(objective, design)
        # `ContentIdentity.of` returns `RuntimeRail[ContentKey]`, so the design key is `match`ed off the
        # rail inside the already-fenced body and a hash `Error` re-raises onto the `boundary` rather
        # than a bare-`ContentKey` use of the railed owner. The design bytes carry no tessellation
        # tolerance, so the `CANONICAL_POLICY` default keys the canonical path — never a fresh
        # `IdentityPolicy()` allocation the runtime content owner rejects.
        match ContentIdentity.of("study", design.tobytes()):
            case Ok(key):
                return StudyReceipt.graded(self, design, measured, key)
            case Error(fault):
                raise RuntimeError(fault)
```

The per-method SALib semantics anchor the `SALIB_ROUTES` table: `route.sampler()`/`route.analyzer()` resolve the submodule callables, `spec.sample(sampler, ...)` draws the structured design and `spec.set_results(Y)`, `set_samples(X)` feeds the design only for the `route.needs_design` rows, then `analyze(analyzer).analysis` reads `route.result_key` over the `ResultDict`. The Morris row reads `mu_star`, the Sobol and FAST rows read first-and-total-order `ST` (over a Saltelli and a FAST-frequency design), the RBD-FAST row reads first-order `S1` over a Latin-hypercube design, the delta row reads the Borgonovo moment-independent `delta` over a Latin-hypercube design, the PAWN row reads the `median` KS statistic over a Sobol design, the DGSM row reads the derivative-based `dgsm` index over a `finite_diff` design, and the HDMR row reads the per-input sensitivity `S` (`S = Sa + Sb`, length `num_vars`) over a Latin-hypercube design (its `ST` key is the per-component-term total expansion of length `n1 + n2 + n3` — strictly longer than `num_vars` for `d >= 2` — so the per-axis `strict=True` zip reads `S`, never `ST`).

The surrogate seam is the one place the charter boundary holds: the `StudyMethod(surrogate=kind)` row composes a `Pipeline` of `StandardScaler` and a `SurrogateKind.estimator()`-resolved `BaseEstimator` regressor and reads the honest cross-validated `R^2` through `cross_val_score`, never the in-sample fit score, while the `polynomial` row's per-axis `numpy.polynomial.Polynomial.fit` in-sample `R^2` is the cheap univariate screening diagnostic. Classical polynomial and ensemble/kernel regression surrogates are in-scope; a neural surrogate and an acquisition-driven active-learning loop are not.

## [03]-[RESEARCH]

- [SALIB_RESULT_KEYS]: the eight analyze submodules (`morris`, `sobol`, `fast`, `rbd_fast`, `delta`, `pawn`, `dgsm`, `hdmr`) are each catalogued in `compute/.api/salib.md` `[02]` analyze-submodule table, and the two `ResultDict` key families the `.api` `[04]-[IMPLEMENTATION_LAW]` documents anchor the table — Sobol `S1`/`S2`/`ST` and Morris `mu`/`mu_star`/`sigma`. The `SalibRoute.result_key` field selects the per-method scalar each analyzer returns over its `ResultDict`: `morris_screen` reads `mu_star` and the Sobol/FAST variance routes read `ST` (both confirmed key families); `rbd_fast` reads the first-order `S1`, `delta` the Borgonovo moment-independent `delta`, `pawn` the per-input `median` of the KS statistic, `dgsm` the derivative-based `dgsm` index, and `hdmr` the per-input sensitivity `S` (the `salib.md` `[03]` HDMR row catalogues an `Sa`/`Sb`/`ST` expansion; the analyzer's `Sa`/`Sb`/`S` arrays are per-input length `num_vars` — `S = Sa + Sb` the total per-input sensitivity, `Sa` its structural and `Sb` its correlated component — while `ST` is the per-component-term total over `n1 + n2 + n3` expansion terms, so reading `ST` overflows the per-axis `strict=True` zip for `d >= 2` and the route reads `S`) — the standard per-input `ResultDict` key each analyzer emits over an array of length `num_vars`, read by `route.result_key` off the `analysis` accessor rather than re-declared. `ProblemSpec.analyze` threads the stored sample matrix and result array to each analyzer over its signature — the `needs_design` rows (`rbd_fast.analyze(problem, X, Y, M=10, ...)`, `delta.analyze(problem, X, Y, ..., method='all')`, `pawn.analyze(problem, X, Y, S=10)` are catalogued in `salib.md` `[03]` analyze table; the `dgsm`/`hdmr` analyzers take `(problem, X, Y, ...)` over their imported signature; `morris.analyze(problem, X, Y)`) call `set_samples(design)` before `analyze`, while the `Y`-only rows (`sobol.analyze(problem, Y)`/`fast.analyze(problem, Y)`) read the responses against their structured design — each keyed by axis name so a `RunHistory` compare join lands on matching rows.
