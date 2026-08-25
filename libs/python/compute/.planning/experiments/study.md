# [PY_COMPUTE_STUDY]

One study-spine owner spans design-of-experiments sampling, global sensitivity analysis, and surrogate fitting: `Study` discriminates by a `StudyMethod` axis over one param-axis and sample-grid spine, and the union owns its `design`/`discrepancy`/`indices` folds, so `experiments/history#RUN_HISTORY` composes `study.method.design`/`indices` directly rather than importing a private across the package seam. SALib owns sensitivity analysis — the owner composes its sampler-and-analyzer pairs through `ProblemSpec` rather than reimplementing variance-based, moment-independent, derivative-based, or component sensitivity. Classical polynomial and ensemble/kernel regression surrogates are in scope; a neural surrogate and an acquisition-driven active-learning loop are not.

Runs ride the `EvidenceScope.STUDY` weave — span, narrowed `boundary` fence, beartype guard, fenced contributor harvest, and the optional stage stream its own `StudyStage` roster names. Receipts settle on the one runtime spine, so the content key, the provenance pair, the warning band, and the stamp are the spine's columns and this page carries no receipt shape of its own. Seams: `numerics/jit` supplies `JitBackend`/`LoweredSpec` for the batch-lane compile and the symbolic-lowered spec VALUE; `data/tabular` supplies the `FrameAdmission`/`FrameInterop`/`FieldShape`/`Backend` DOE-frame gate through published surfaces only; runtime `profiles` supplies the `BenchmarkReceipt`/`BenchMode` bench fabric the receipt's `benched` projection feeds from held measurements; the objective crosses the process band as an argument of one `HOSTILE`-trait runtime `Kernel` — the module-level kernel ships `REFERENCE`, and a closure-bearing objective rides the pool's cloudpickle wire.

## [01]-[INDEX]

- [02]-[STUDY]: DOE sampling, SALib sensitivity, and surrogate fitting on one `Study` owner — the union-owned folds, the `SALIB_ROUTES` table, the live `Measured` measurement discriminant, and its projection onto the runtime bench fabric.

## [02]-[STUDY]

- Owner: `Study` — DOE sampling, global sensitivity, and surrogate fitting are cases on one owner; the benchmark concern is the live `MeasurementMode` discriminant folded into `Measured`, never a parallel benchmark owner; `RunHistory` rides the same spine for persistence and resume.
- Cases: the union's keyword constructor is the one construction surface, no sibling factory family; the eight SALib analyzers are one routed sampler body and one routed analyzer body over `SALIB_ROUTES`, the per-method knobs folded from the case payload through `_salib_args`; one seed policy crosses sampler, analyzer, content identity, receipt, and evidence. Classical coded designs — fractional-factorial by resolution, Box-Behnken, central-composite, folded Plackett-Burman — are sampling-only members whose `indices` return `{}` and whose `design` folds ride one `_coded` match through the shared `_unit`/`_box` map: coded levels normalize by the design's own extreme (a circumscribed CCD's axial overshoot lands on the bounds) and map LINEARLY onto each axis's `bounds`, the same box map the `factorial` grid takes, because a coded design is box-geometric and the marginal ppf's 0/1 tails are unbounded.
- Law: the nearest async fold — `run` here, `RunHistory.resume` at the history owner — settles one `Resource.RECORD` `MeterFact` off the CLEARED receipt's own `meter` projection: the fresh-admission census `evaluated_cells` times response arity, surfaced by the study method, because that fold is the nearest async owner of a count the HOSTILE kernel produced and binds no plane for. Refused studies evaluated nothing and bill nothing, a zero fresh census charges nothing, and a wholly-cached resume lands no row. Series naming lives at the journal owner, so no metric row is minted beside the receipt fan.
- Entry: `Study.run` is one polymorphic entry discriminating by input shape — an `Objective` runs the sampled evaluation, a contract-gated DOE frame grades a pre-measured cohort — never a second entry; the caller's composition `ScopeKey` threads onto the weave so an embedded composition's lifecycle facts key to it, defaulted so the root call shape stays scope-free.
- Output: `Measured` carries the responses, the wallclock, and the `Option[float]` batch-versus-serial speedup that is `Nothing` for a bare row objective — never a fabricated ratio over the identical per-row work timed twice; the `surrogate` row reads the honest cross-validated `R^2` while the `polynomial` row's in-sample `R^2` is the cheap univariate screening diagnostic. `StudyReceipt.benched` projects the held wallclock onto the runtime bench fabric under the receipt's content-keyed subject — `BenchmarkReceipt.of` consumes the measurement the run already paid for, a SPEEDUP run recovers its serial baseline as the sibling `.serial` duration series, `RESULT`'s zero elapsed suppresses the contribution, and distinct objectives never merge into one method-only benchmark series.
- Stage: `Study.run` reports FOUR named positions off its own closed `StudyStage` roster — the design draw, the per-row evaluation sweep, the key mint, and the completed analysis — where the weave's lifecycle pair reported two over an N-row design. The mark is ONE `StageTap` the entry opens with an absent census and the worker re-stamps once the design states its extent; the row beat is the caller's closure, so the roster stays this fold's and never spans into the history owner's.
- Growth: a new input marginal is one `AxisDist` member with one `rescale` arm and one `bounds` arm; a new SALib analyzer is one `StudyMethod` case and one `SALIB_ROUTES` row, no new body; a new interior position is one `StudyStage` member and one `beat` call, never a weave edit; a new refusal is one `FaultRow` anchor in `RAISES`; a new `qmc` engine or numpy floor is one arm on `_qmc`/`design`; a new classical coded design is one `StudyMethod` case and one `_coded` arm reaching the shared `_unit`/`_box` map; a new surrogate estimator is one `SurrogateKind` member and one `SURROGATE_CLASS` row; a new measurement is one `MeasurementMode` member reading the shared `Measured` fold; a new bench statistic is one runtime `BenchmarkReceipt` field under the bench growth law, reached with zero study edits.

```python
import time
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

import msgspec
import numpy as np
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, StageTap, evidence_run
from rasm.compute.numerics.jit import JitBackend, LoweredSpec
from rasm.data.tabular.contract import FrameAdmission
from rasm.data.tabular.interop import Backend, FieldShape, FrameInterop
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.journal import Journal, MeterFact, Resource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.profiles import BenchMode, BenchmarkReceipt
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy from SALib import ProblemSpec
lazy from SALib.analyze import delta as delta_analysis
lazy from SALib.analyze import dgsm as dgsm_analysis
lazy from SALib.analyze import fast as fast_analysis
lazy from SALib.analyze import hdmr as hdmr_analysis
lazy from SALib.analyze import morris as morris_analysis
lazy from SALib.analyze import pawn as pawn_analysis
lazy from SALib.analyze import rbd_fast as rbd_fast_analysis
lazy from SALib.analyze import sobol as sobol_analysis
lazy from SALib.sample import fast_sampler, finite_diff, latin
lazy from SALib.sample import morris as morris_sampling
lazy from SALib.sample import sobol as sobol_sampling
lazy from pyDOE3 import bbdesign, ccdesign, fold, fracfact_by_res, pbdesign
lazy from scipy import stats
lazy from scipy.stats import qmc
lazy from sklearn.ensemble import GradientBoostingRegressor, RandomForestRegressor
lazy from sklearn.linear_model import Ridge
lazy from sklearn.model_selection import cross_val_score
lazy from sklearn.pipeline import Pipeline
lazy from sklearn.preprocessing import StandardScaler
lazy from sklearn.svm import SVR

if TYPE_CHECKING:
    from sklearn.base import BaseEstimator

# --- [TYPES] ----------------------------------------------------------------------------

type RowScorer = Callable[[np.ndarray], float | np.ndarray]
type BatchScorer = Callable[[np.ndarray], np.ndarray]
type SalibTag = Literal["morris_screen", "sobol_indices", "fast", "rbd_fast", "delta", "pawn", "dgsm", "hdmr"]
type RowBeat = Callable[[int], None]


class StudyStage(StrEnum):
    SAMPLED = "sampled"
    EVALUATED = "evaluated"
    ANALYSED = "analysed"
    KEYED = "keyed"


_SPEC: Final = msgspec.msgpack.Encoder(order="deterministic")


class Objective(Struct, frozen=True):
    row: RowScorer
    batch: Option[BatchScorer] = Nothing
    jit: Option[JitBackend] = Nothing

    @staticmethod
    def lowered(spec: LoweredSpec) -> "Objective":
        return Objective(row=spec.kernel, jit=Some(spec.route))

    def scorer(self) -> RowScorer:
        return self.jit.map(lambda route: route.compile(self.row).map(lambda jitted: jitted.fn).default_value(self.row)).default_value(self.row)

    def rows(self, design: np.ndarray, beat: RowBeat = _unbeaten) -> np.ndarray:
        scorer = self.scorer()
        return np.stack(list(Block.of_seq(design).mapi(lambda ordinal, point: _scored(scorer, point, beat, ordinal + 1))))

    def identity(self) -> tuple[object, ...]:
        route = self.jit.map(lambda held: (held.tag, getattr(held, held.tag))).default_value(())
        return (_scorer_identity(self.row), self.batch.map(_scorer_identity).default_value(()), route)


def _unbeaten(_done: int) -> None:
    return None


def _scored(scorer: RowScorer, point: np.ndarray, beat: RowBeat, done: int) -> np.ndarray:
    scored = np.asarray(scorer(point), dtype=float)
    beat(done)
    return scored


def _scorer_identity(fn: Callable[..., object]) -> tuple[str, str, bytes]:
    kernel = Kernel.of(fn)
    return (kernel.module, kernel.name, kernel.payload)


def _study_kernel(study: "Study", objective: Objective, seed: int, mark: StageTap) -> "RuntimeRail[StudyReceipt]":
    return boundary(STUDY_EXECUTE, lambda: study._execute(objective, seed, mark), catch=_DESIGN_RAISES).bind(lambda outcome: outcome)


def _timed[T](thunk: Callable[[], T]) -> tuple[T, float]:
    start = time.perf_counter()
    value = thunk()
    return value, time.perf_counter() - start


class MeasurementMode(StrEnum):
    RESULT = "result"
    WALLCLOCK = "wallclock"
    SPEEDUP = "speedup"

    def evaluate(self, objective: Objective, design: np.ndarray, beat: RowBeat = _unbeaten) -> "Measured":
        fast: Callable[[], np.ndarray] = objective.batch.map(lambda b: lambda: b(design)).default_value(lambda: objective.rows(design, beat))
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


class CcAlpha(StrEnum):
    ORTHOGONAL = "orthogonal"
    ROTATABLE = "rotatable"


class CcFace(StrEnum):
    CIRCUMSCRIBED = "circumscribed"
    INSCRIBED = "inscribed"
    FACED = "faced"


class SurrogateKind(StrEnum):
    GRADIENT_BOOST = "gradient_boost"
    RANDOM_FOREST = "random_forest"
    SVR = "svr"
    RIDGE = "ridge"

    def estimator(self) -> "BaseEstimator":
        return SURROGATE_CLASS[self]()


class AxisDist(StrEnum):
    UNIF = "unif"
    NORM = "norm"
    LOGNORM = "lognorm"
    TRIANG = "triang"
    TRUNCNORM = "truncnorm"


class ParamAxis(Struct, frozen=True):
    name: str
    params: tuple[float, ...]
    dist: AxisDist = AxisDist.UNIF

    @property
    def bounds(self) -> tuple[float, float]:
        match self.dist:
            case AxisDist.UNIF | AxisDist.TRIANG | AxisDist.TRUNCNORM:
                return self.params[0], self.params[1]
            case AxisDist.NORM:
                mean, std = self.params
                return mean - 4.0 * std, mean + 4.0 * std
            case AxisDist.LOGNORM:
                mean, std = self.params
                return float(np.exp(mean - 4.0 * std)), float(np.exp(mean + 4.0 * std))
            case _ as unreachable:
                assert_never(unreachable)

    def rescale(self, unit_col: np.ndarray) -> np.ndarray:
        match self.dist:
            case AxisDist.UNIF:
                low, high = self.params
                return low + (high - low) * unit_col
            case AxisDist.NORM:
                mean, std = self.params
                return stats.norm.ppf(unit_col, loc=mean, scale=std)
            case AxisDist.LOGNORM:
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


class SalibRoute(Struct, frozen=True):
    sample: Callable[[], Callable[..., object]]
    analyze: Callable[[], Callable[..., object]]
    result_key: str
    needs_design: bool

    def sampler(self) -> Callable[..., object]:
        return self.sample()

    def analyzer(self) -> Callable[..., object]:
        return self.analyze()


class Measured(Struct, frozen=True):
    responses: np.ndarray
    elapsed: float
    speedup: Option[float]


@tagged_union(frozen=True)
class StudyMethod:
    tag: Literal[
        "lhs",
        "factorial",
        "fractional",
        "box_behnken",
        "central_composite",
        "plackett_burman",
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
    fractional: int = case()
    box_behnken: int = case()
    central_composite: tuple[tuple[int, int], CcAlpha, CcFace] = case()
    plackett_burman: bool = case()
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

    def design(self, axes: tuple[ParamAxis, ...], seed: int) -> np.ndarray:
        match self:
            case StudyMethod(tag="lhs" | "sobol" | "halton" | "polynomial" | "surrogate"):
                return self._qmc(axes, seed)
            case StudyMethod(tag="factorial", factorial=levels):
                grids = np.meshgrid(*[np.linspace(0.0, 1.0, k) for k in levels], indexing="ij")
                unit = np.stack([g.reshape(-1) for g in grids], axis=1)
                return StudyMethod._box(axes, unit)
            case StudyMethod(tag="fractional" | "box_behnken" | "central_composite" | "plackett_burman"):
                return StudyMethod._box(axes, StudyMethod._unit(self._coded(len(axes))))
            case StudyMethod(tag="morris_screen" | "sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr" as t):
                n, sample_kwargs, _ = self._salib_args()
                return StudyMethod._spec(axes).sample(SALIB_ROUTES[t].sampler(), n, seed=seed, **sample_kwargs).samples
            case _ as unreachable:
                assert_never(unreachable)

    def discrepancy(self, axes: tuple[ParamAxis, ...], design: np.ndarray) -> Option[float]:
        match self:
            case StudyMethod(tag="lhs" | "sobol" | "halton" | "polynomial" | "surrogate") if all(ax.dist is AxisDist.UNIF for ax in axes):
                lo = np.asarray([ax.bounds[0] for ax in axes], dtype=float)
                hi = np.asarray([ax.bounds[1] for ax in axes], dtype=float)
                return Some(float(qmc.discrepancy((design - lo) / np.where(hi > lo, hi - lo, 1.0))))
            case _:
                return Nothing

    def indices(self, axes: tuple[ParamAxis, ...], design: np.ndarray, responses: np.ndarray, seed: int) -> dict[str, float]:
        names = [ax.name for ax in axes]
        match self:
            case StudyMethod(tag="morris_screen" | "sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr" as t):
                _, _, analyze_kwargs = self._salib_args()
                return StudyMethod._salib(axes, SALIB_ROUTES[t], design, responses, names, analyze_kwargs, seed)
            case StudyMethod(tag="polynomial", polynomial=degree):
                return {ax.name: StudyMethod._axis_r2(design[:, j], responses, degree) for j, ax in enumerate(axes)}
            case StudyMethod(tag="surrogate", surrogate=kind):
                return {"cv_r2": StudyMethod._surrogate_cv(kind, design, responses)}
            case StudyMethod(tag="lhs" | "factorial" | "fractional" | "box_behnken" | "central_composite" | "plackett_burman" | "sobol" | "halton"):
                return {}
            case _ as unreachable:
                assert_never(unreachable)

    def _salib_args(self) -> tuple[int, dict[str, object], dict[str, object]]:
        match self:
            case StudyMethod(tag="morris_screen", morris_screen=(traj, levels)):
                return traj, {"num_levels": levels}, {"num_levels": levels}
            case StudyMethod(tag="sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr" as t):
                return getattr(self, t), {}, {}
            case _ as unreachable:
                assert_never(unreachable)

    def _qmc(self, axes: tuple[ParamAxis, ...], seed: int) -> np.ndarray:
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
        return np.stack([ax.rescale(unit[:, j]) for j, ax in enumerate(axes)], axis=1)

    def _coded(self, n: int) -> np.ndarray:
        match self:
            case StudyMethod(tag="fractional", fractional=resolution):
                return fracfact_by_res(n, resolution)
            case StudyMethod(tag="box_behnken", box_behnken=center):
                return bbdesign(n, center=center)
            case StudyMethod(tag="central_composite", central_composite=((cube, axial), alpha, face)):
                return ccdesign(n, center=(cube, axial), alpha=alpha.value, face=face.value)
            case StudyMethod(tag="plackett_burman", plackett_burman=folded):
                coded = pbdesign(n)
                return fold(coded) if folded else coded
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _unit(coded: np.ndarray) -> np.ndarray:
        extreme = max(1.0, float(np.abs(coded).max()))
        return (coded + extreme) / (2.0 * extreme)

    @staticmethod
    def _box(axes: tuple[ParamAxis, ...], unit: np.ndarray) -> np.ndarray:
        lo = np.asarray([ax.bounds[0] for ax in axes], dtype=float)
        hi = np.asarray([ax.bounds[1] for ax in axes], dtype=float)
        return lo + unit * (hi - lo)

    @staticmethod
    def _spec(axes: tuple[ParamAxis, ...]) -> "ProblemSpec":
        problem: dict[str, object] = {"num_vars": len(axes), "names": [ax.name for ax in axes], "bounds": [list(ax.params) for ax in axes]}
        if any(ax.dist is not AxisDist.UNIF for ax in axes):
            problem["dists"] = [ax.dist.value for ax in axes]
        return ProblemSpec(problem)

    @staticmethod
    def _salib(
        axes: tuple[ParamAxis, ...],
        route: SalibRoute,
        design: np.ndarray,
        responses: np.ndarray,
        names: list[str],
        analyze_kwargs: dict[str, object],
        seed: int,
    ) -> dict[str, float]:
        spec = StudyMethod._spec(axes).set_results(responses)
        feed = spec.set_samples(design) if route.needs_design else spec
        analysis = feed.analyze(route.analyzer(), seed=seed, **analyze_kwargs).analysis
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
        pipeline = Pipeline([("scale", StandardScaler()), ("model", kind.estimator())])
        return float(cross_val_score(pipeline, design, responses, cv=5, scoring="r2").mean())


# --- [TABLES] ---------------------------------------------------------------------------

_DESIGN_RAISES: Final[Catch] = (BeartypeCallHintViolation, AssertionError, RuntimeError, ValueError)

STUDY_EXECUTE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.STUDY, point="execute", arm="boundary", defect="study-execute", retriability=TERMINAL
)
STUDY_FRAME: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.STUDY, point="frame", arm="boundary", defect="frame-grade", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([STUDY_EXECUTE, STUDY_FRAME]))

SALIB_ROUTES: Final[Map[SalibTag, SalibRoute]] = Map.of_seq([
    ("morris_screen", SalibRoute(lambda: morris_sampling.sample, lambda: morris_analysis.analyze, "mu_star", True)),
    ("sobol_indices", SalibRoute(lambda: sobol_sampling.sample, lambda: sobol_analysis.analyze, "ST", False)),
    ("fast", SalibRoute(lambda: fast_sampler.sample, lambda: fast_analysis.analyze, "ST", False)),
    ("rbd_fast", SalibRoute(lambda: latin.sample, lambda: rbd_fast_analysis.analyze, "S1", True)),
    ("delta", SalibRoute(lambda: latin.sample, lambda: delta_analysis.analyze, "delta", True)),
    ("pawn", SalibRoute(lambda: sobol_sampling.sample, lambda: pawn_analysis.analyze, "median", True)),
    ("dgsm", SalibRoute(lambda: finite_diff.sample, lambda: dgsm_analysis.analyze, "dgsm", True)),
    (
        "hdmr",
        SalibRoute(lambda: latin.sample, lambda: hdmr_analysis.analyze, "S", True),
    ),
])

SURROGATE_CLASS: Final[Map[SurrogateKind, Callable[[], "BaseEstimator"]]] = Map.of_seq([
    (SurrogateKind.GRADIENT_BOOST, lambda: GradientBoostingRegressor()),
    (SurrogateKind.RANDOM_FOREST, lambda: RandomForestRegressor()),
    (SurrogateKind.SVR, lambda: SVR()),
    (SurrogateKind.RIDGE, lambda: Ridge()),
])

# --- [MODELS] ---------------------------------------------------------------------------


class StudyReceipt(Struct, frozen=True):
    method: str
    mode: MeasurementMode
    design_cells: int
    evaluated_cells: int
    response_width: Option[int]
    indices: dict[str, float]
    discrepancy: Option[float]
    elapsed: float
    speedup: Option[float]
    seed: int
    subject: str
    content_key: ContentKey

    @staticmethod
    def graded(
        study: "Study", design: np.ndarray, measured: Measured, key: ContentKey, seed: int, *, evaluated: Option[int] = Nothing
    ) -> "StudyReceipt":
        rows = int(design.shape[0])
        return StudyReceipt(
            study.method.tag,
            study.mode,
            rows,
            evaluated.default_value(rows),
            Some(int(measured.responses.size // rows)) if rows else Nothing,
            study.method.indices(study.axes, design, measured.responses, seed),
            study.method.discrepancy(study.axes, design),
            measured.elapsed,
            measured.speedup,
            seed,
            f"study.{study.method.tag}.{key.hex}",
            key,
        )

    @property
    def band(self) -> Block[str]:
        return Block.of_seq([
            *(("empty-design",) if self.response_width.is_none() else ()),
            *(("in-sample-r2",) if self.method == "polynomial" else ()),
        ])

    @property
    def span_facts(self) -> dict[str, str | int | float]:
        return {
            "method": self.method,
            "mode": self.mode.value,
            "design_cells": self.design_cells,
            "evaluated_cells": self.evaluated_cells,
            "seed": self.seed,
        }

    def meter(self) -> Option[MeterFact]:
        return self.response_width.bind(
            lambda width: Some(MeterFact(resource=Resource.RECORD, quantity=self.evaluated_cells * width, surface=self.method))
            if self.evaluated_cells * width
            else Nothing
        )

    def benched(self, subject: Option[str] = Nothing) -> tuple[BenchmarkReceipt, ...]:
        keyed = subject.default_value(self.subject)
        serial = self.speedup.map(lambda ratio: (BenchmarkReceipt.of(f"{keyed}.serial", BenchMode.LATENCY, 0, (self.elapsed * ratio * 1000.0,)),))
        return () if self.elapsed <= 0.0 else (BenchmarkReceipt.of(keyed, BenchMode.LATENCY, 0, (self.elapsed * 1000.0,)), *serial.default_value(()))

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "mode": self.mode.value,
            "design_cells": self.design_cells,
            "evaluated_cells": self.evaluated_cells,
            "response_width": self.response_width.to_optional(),
            "seed": self.seed,
            "elapsed": self.elapsed,
            "discrepancy": self.discrepancy.to_optional(),
            "speedup": self.speedup.to_optional(),
            **{f"S[{k}]": v for k, v in self.indices.items()},
        }
        return (
            Receipt.of(
                EvidenceScope.STUDY.value,
                ("emitted", self.subject, facts),
                key=Some(self.content_key),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.content_key)),
                band=self.band,
            ),
            *(row for bench in self.benched() for row in bench.contribute()),
        )


# --- [SERVICES] -------------------------------------------------------------------------


class Study(Struct, frozen=True):
    axes: tuple[ParamAxis, ...]
    method: StudyMethod
    mode: MeasurementMode
    frame_backend: Backend = Backend.PYARROW

    async def run(
        self, source: "Objective | object", lane: LanePolicy, /, *, seed: int = 0, composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[StudyReceipt]:
        mark = StageTap.of(EvidenceScope.STUDY, lane.pulses.tap)

        async def dispatch() -> RuntimeRail[StudyReceipt]:
            match source:
                case Objective() as objective:
                    kernel = Kernel.of(_study_kernel, KernelTrait.HOSTILE, idempotent=self.mode is MeasurementMode.RESULT)
                    return (await lane.offload(kernel, self, objective, seed, mark)).bind(lambda rail: rail)
                case frame:
                    return self._admit_frame(frame).bind(
                        lambda decoded: boundary(
                            STUDY_FRAME,
                            lambda: self._graded_frame(decoded[0], decoded[1], seed),
                            catch=_DESIGN_RAISES,
                        ).bind(lambda outcome: outcome)
                    )

        facts = {"method": self.method.tag, "mode": self.mode.value, "axes": len(self.axes), "seed": seed}
        settled = await evidence_run(
            EvidenceScope.STUDY, f"study.{self.method.tag}", dispatch, facts=facts, composition=composition, stage=Some(mark)
        )
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.meter().to_list(), scope=composition)).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    def _admit_frame(self, frame: object) -> "RuntimeRail[tuple[np.ndarray, np.ndarray]]":
        shapes = tuple(FieldShape(field=axis.name, logical_type="Float64", nullable=False) for axis in self.axes)
        response = FieldShape(field="response", logical_type="Float64", nullable=False)
        gate = FrameAdmission.of(FrameInterop.of(self.frame_backend), (*shapes, response))
        return gate.admit(frame).bind(
            lambda admitted: gate.enforce(admitted).map(
                lambda _claim: (
                    np.column_stack([np.asarray(admitted.frame[axis.name], dtype=float) for axis in self.axes]),
                    np.asarray(admitted.frame["response"], dtype=float),
                )
            )
        )

    def spec_key(
        self,
        design: np.ndarray,
        objective: Option[Objective] = Nothing,
        /,
        *,
        responses: "Option[np.ndarray]" = Nothing,
        seed: int = 0,
    ) -> "RuntimeRail[ContentKey]":
        spec = objective.map(Objective.identity).default_value(())
        measured = responses.map(lambda held: (_SPEC.encode(held.shape), np.ascontiguousarray(held, dtype=np.float64).tobytes())).default_value(())
        parts = (
            _SPEC.encode((self.axes, self.method.tag, self.mode, spec, seed)),
            _SPEC.encode(design.shape),
            np.ascontiguousarray(design, dtype=np.float64).tobytes(),
            *measured,
        )
        return ContentIdentity.of("study", IdentitySource(parts=parts))

    def _graded_frame(self, design: np.ndarray, responses: np.ndarray, seed: int) -> "RuntimeRail[StudyReceipt]":
        measured = Measured(responses[:, None] if responses.ndim == 1 else responses, 0.0, Nothing)
        return self.spec_key(design, responses=Some(responses), seed=seed).map(
            lambda key: StudyReceipt.graded(self, design, measured, key, seed)
        )

    @beartype(conf=FAULT_CONF)
    def _execute(self, objective: Objective, seed: int, mark: StageTap) -> "RuntimeRail[StudyReceipt]":
        design = self.method.design(self.axes, seed)
        cells = int(design.shape[0])
        staged = structs.replace(mark, total=Some(cells))
        staged.beat(StudyStage.SAMPLED, cells)
        measured = self.mode.evaluate(objective, design, lambda done: staged.beat(StudyStage.EVALUATED, done))

        def graded(key: ContentKey) -> StudyReceipt:
            staged.beat(StudyStage.KEYED, cells)
            receipt = StudyReceipt.graded(self, design, measured, key, seed)
            staged.beat(StudyStage.ANALYSED, cells)
            return receipt

        return self.spec_key(design, Some(objective), seed=seed).map(graded)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
