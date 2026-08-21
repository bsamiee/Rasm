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

```python signature
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

# a tagged carrier rather than an erased `Callable -> Callable` union — both shapes are the same `function` runtime type, so a
# structural `match`/`isinstance` cannot discriminate them. The row scorer returns a scalar or a per-output vector: SALib and the
# surrogate floors require the scalar `(rows,)` shape, the multi-output `(rows, k)` shape is admitted on the sampling-only methods
# whose `indices` fold returns `{}`, and a multi-output objective under a SALib/surrogate method rails on the analyzer's own 1-D
# `Y` contract rather than silently averaging.
type RowScorer = Callable[[np.ndarray], float | np.ndarray]
type BatchScorer = Callable[[np.ndarray], np.ndarray]
type SalibTag = Literal["morris_screen", "sobol_indices", "fast", "rbd_fast", "delta", "pawn", "dgsm", "hdmr"]
# the row-position beat a scoring fold reports through, bound by the CALLER so each long fold names a member of its
# OWN closed roster: `Study.run` beats `StudyStage`, `RunHistory.resume` beats `ResumeStage`, and neither vocabulary
# reaches the other. A shared roster spanning two folds is exactly the cross-fold ladder the stage law refuses.
type RowBeat = Callable[[int], None]


class StudyStage(StrEnum):
    # the CLOSED milestone roster of `Study.run`'s own fold, erased at the lane conduit: the design draw, the
    # per-row evaluation sweep, the SALib/surrogate analysis, and the key mint. Four named positions replace the
    # two the weave's lifecycle pair reported over an N-row design.
    SAMPLED = "sampled"
    EVALUATED = "evaluated"
    ANALYSED = "analysed"
    KEYED = "keyed"


_SPEC: Final = msgspec.msgpack.Encoder(order="deterministic")  # deterministic encoder for the NON-array spec parts the key preimage carries


class Objective(Struct, frozen=True):
    row: RowScorer
    batch: Option[BatchScorer] = Nothing  # vectorized fast lane; absent objectives score per-row only
    jit: Option[JitBackend] = Nothing  # the loop-kernel accelerator row the batch lane compiles through

    @staticmethod
    def lowered(spec: LoweredSpec) -> "Objective":
        # spec's kernel is the row scorer and its recommended route arms the batch-lane compile — the symbolic->jit->study
        # chain with zero symbolic imports.
        return Objective(row=spec.kernel, jit=Some(spec.route))

    def scorer(self) -> RowScorer:
        # a Some `jit` row compiles the row scorer through `JitBackend.compile`; a compile fault degrades to the host row.
        return self.jit.map(lambda route: route.compile(self.row).map(lambda jitted: jitted.fn).default_value(self.row)).default_value(self.row)

    def rows(self, design: np.ndarray, beat: RowBeat = _unbeaten) -> np.ndarray:
        # the per-row beat is this sweep's ONE monotonic interior position, so an N-row design reports N positions
        # where the weave's lifecycle pair reports two. `mapi` carries the ordinal, so the beat rides the same fold
        # that scores rather than a counter beside it, and the CALLER binds which roster member the position names.
        scorer = self.scorer()
        return np.stack(list(Block.of_seq(design).mapi(lambda ordinal, point: _scored(scorer, point, beat, ordinal + 1))))

    def identity(self) -> tuple[object, ...]:
        # complete yield-affecting identity `Study.spec_key` folds: row scorer, batch lane, and the jit route
        # row — a batch or jit configuration change re-keys exactly as a scorer change does.
        route = self.jit.map(lambda held: (held.tag, getattr(held, held.tag))).default_value(())
        return (_scorer_identity(self.row), self.batch.map(_scorer_identity).default_value(()), route)


def _unbeaten(_done: int) -> None:
    # the no-position default: a sweep with no conduit threads this and its fold is byte-identical to the unbeaten
    # one, which is what keeps the stage slot optional at every caller that has no positions to report.
    return None


def _scored(scorer: RowScorer, point: np.ndarray, beat: RowBeat, done: int) -> np.ndarray:
    # one scored row beside its position beat — the pair the sweep repeats, named once so the reported position can
    # never drift out of step with the row it reports.
    scored = np.asarray(scorer(point), dtype=float)
    beat(done)
    return scored


def _scorer_identity(fn: Callable[..., object]) -> tuple[str, str, bytes]:
    # `Kernel.of` is the one payload-classification surface: an importable scorer keys by its module-qualified
    # name (REFERENCE ships an empty payload), a closure or bound method by its cloudpickle content bytes (VALUE) —
    # so two distinct scorers sharing a `<lambda>` label never share an identity.
    kernel = Kernel.of(fn)
    return (kernel.module, kernel.name, kernel.payload)


def _study_kernel(study: "Study", objective: Objective, seed: int, mark: StageTap) -> "RuntimeRail[StudyReceipt]":
    # module-level so REFERENCE shipping resolves it by import; the fence converts a sampler/analyzer/fit raise. A closure-bearing
    # objective still crosses the process band as an argument on the pool's cloudpickle wire, and the conduit tap rides
    # beside it as the ordinary pickled kernel argument the lane law makes it.
    return boundary(STUDY_EXECUTE, lambda: study._execute(objective, seed, mark), catch=_DESIGN_RAISES).bind(lambda outcome: outcome)


def _timed[T](thunk: Callable[[], T]) -> tuple[T, float]:
    # one perf_counter fold the measurement arms share.
    start = time.perf_counter()
    value = thunk()
    return value, time.perf_counter() - start


class MeasurementMode(StrEnum):
    RESULT = "result"  # evaluate the design but suppress the wallclock; report zero elapsed
    WALLCLOCK = "wallclock"  # time the whole design evaluation as one batch
    SPEEDUP = "speedup"  # fold the vectorized batch wallclock against the per-row serial baseline

    def evaluate(self, objective: Objective, design: np.ndarray, beat: RowBeat = _unbeaten) -> "Measured":
        # `RESULT`/`WALLCLOCK` and the bare `SPEEDUP` (no batch lane) collapse to one timed-`fast` arm; only a real
        # `objective.batch` lane earns the second timed pass folding the honest serial-over-batch ratio. The beat
        # rides the PRIMARY pass alone — the SPEEDUP baseline re-walks rows the primary already reported, so beating
        # it would publish one design's positions twice and read as a fold that doubled back.
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
    ORTHOGONAL = "orthogonal"  # axial distance preserving block orthogonality
    ROTATABLE = "rotatable"  # axial distance giving constant prediction variance on spheres


class CcFace(StrEnum):
    CIRCUMSCRIBED = "circumscribed"  # axial points OUTSIDE the cube (|coded| > 1); the box map absorbs the overshoot
    INSCRIBED = "inscribed"  # cube shrunk so axial points sit on the bounds
    FACED = "faced"  # axial points on the cube faces


class SurrogateKind(StrEnum):
    GRADIENT_BOOST = "gradient_boost"  # ensemble.GradientBoostingRegressor
    RANDOM_FOREST = "random_forest"  # ensemble.RandomForestRegressor
    SVR = "svr"  # svm.SVR
    RIDGE = "ridge"  # linear_model.Ridge

    def estimator(self) -> "BaseEstimator":
        return SURROGATE_CLASS[self]()


# each member's `params` arity matches the `scipy.stats.<dist>` parameterization the SALib `dists` channel resolves, so one axis
# declares one marginal both paths read identically — never a 2-tuple overload truncating the 3- and 4-param vectors.
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

    # support endpoints the sampler scales into; `norm`/`lognorm` are unbounded, so the row reads a wide window off mean±std.
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

    # qmc path inverse-transforms the unit draw through the exact `scipy.stats.<dist>.ppf` form the SALib `dists` channel
    # resolves, so the two draws are the same marginal rather than two divergent inverse transforms.
    def rescale(self, unit_col: np.ndarray) -> np.ndarray:
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


# named fields, never a four-positional tuple indexed by position; `needs_design` discriminates the paired `(X, Y)` analyzers
# from the `Y`-only routes.
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
    speedup: Option[float]  # batch-versus-serial ratio under MeasurementMode.SPEEDUP, Nothing otherwise


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
    fractional: int = case()  # minimum-aberration resolution; `fracfact_by_res` raises `design not possible` onto the fence
    box_behnken: int = case()  # center-point count; pyDOE3 asserts >= 3 factors onto the fence
    central_composite: tuple[tuple[int, int], CcAlpha, CcFace] = case()  # (factorial-block, axial-block) centers
    plackett_burman: bool = case()  # True appends the sign-reversed `fold` mirror, decoupling main effects
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

    # union OWNS its three folds — `design`/`discrepancy`/`indices`, each a total `match` taking `axes` as payload — so no
    # detached free function reaches back into the union.
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

    # `qmc.discrepancy` is honest only over an all-`unif` design where the affine de-scale recovers the unit draw; a non-`unif`
    # marginal shapes the design out of the box and the score is `Nothing` rather than a misread.
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

    # `num_levels` MUST match across `morris.sample` and `morris.analyze` or the elementary-effect grid and the index
    # reconstruction disagree, so the same value threads both kwarg channels.
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
        # one rescale path across the qmc and factorial floors — `qmc.scale` is not a second uniform-only scaling surface.
        return np.stack([ax.rescale(unit[:, j]) for j, ax in enumerate(axes)], axis=1)

    # classical coded designs: pyDOE3 emits run-by-factor level matrices in its own coded coordinates. Deterministic
    # constructions, so the seed reaches none of them; row counts derive from the factor count and payload alone.
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

    # normalize a coded matrix onto [0, 1] by its own global extreme, so a circumscribed CCD's |coded| > 1 axial
    # points land ON the axis bounds and every cube point lands inside them.
    @staticmethod
    def _unit(coded: np.ndarray) -> np.ndarray:
        extreme = max(1.0, float(np.abs(coded).max()))
        return (coded + extreme) / (2.0 * extreme)

    # classical designs are box-geometric: levels place on the support box by construction, so the fold maps
    # linearly onto each axis's `bounds` — never through the marginal ppf, whose 0/1 tails are unbounded (a NORM
    # axis would place corner runs at ±inf).
    @staticmethod
    def _box(axes: tuple[ParamAxis, ...], unit: np.ndarray) -> np.ndarray:
        lo = np.asarray([ax.bounds[0] for ax in axes], dtype=float)
        hi = np.asarray([ax.bounds[1] for ax in axes], dtype=float)
        return lo + unit * (hi - lo)

    @staticmethod
    def _spec(axes: tuple[ParamAxis, ...]) -> "ProblemSpec":
        # SALib samplers shape their own marginals off `problem['dists']` (emitted only when an axis is non-`unif`); when set,
        # SALib reads each `bounds` row as the dist's FULL parameter vector, so the row is `ax.params` itself, not `(low, high)`.
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

# the provider raise surface both study fences reach, named once because both bodies run the SAME stack — the design
# draw, the evaluation sweep, and the index fold. Every member is proved against the installed distributions rather
# than authored: pyDOE3 raises `ValueError` across its coded constructors ("design not possible", generator and
# factor-count refusals) and ASSERTS its factor floors in `bbdesign`/`ccdesign`; SALib raises `ValueError`,
# `RuntimeError`, and `numpy.linalg.LinAlgError` out of its samplers and analyzers; scipy's `qmc` engines and
# scikit-learn's pipeline and CV folds raise `ValueError` (which `NotFittedError` and `LinAlgError` both subclass);
# the `@beartype(conf=FAULT_CONF)` contract on `_execute` raises the canonical violation the `CLASSIFY` `api` row
# folds. `RuntimeError` is SALib's own and nothing else's — the key-mint rail returns its refusal typed rather than
# re-raising it here. A bare `Exception` would swallow the interpreter and lane faults the lane owner must see whole.
_DESIGN_RAISES: Final[Catch] = (BeartypeCallHintViolation, AssertionError, RuntimeError, ValueError)

# this page's raise-side roster under the hub `ComputeLeg` roster: the retired `f"study.{method.tag}"` subject forked
# ONE refusal law into eighteen coordinates no roster could enumerate and no shared census read could seat — the method
# is a span fact and a receipt column, never a subject segment. The two rows stay distinct because the fences differ
# in kind: one converts a raise from the sampled evaluation crossing a worker, the other from grading a cohort a
# caller already measured.
STUDY_EXECUTE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.STUDY, point="execute", arm="boundary", defect="study-execute", retriability=TERMINAL
)
STUDY_FRAME: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.STUDY, point="frame", arm="boundary", defect="frame-grade", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([STUDY_EXECUTE, STUDY_FRAME]))

# one route row per tag drives one sampler body and one analyzer body; `result_key` selects the per-method scalar over the `ResultDict`.
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
    ),  # per-input sensitivity (`S=Sa+Sb`, length num_vars); `ST` is the per-component-term total, longer than num_vars
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
    design_cells: int  # whole-grid census in design rows; the run is total (every row evaluates or rails)
    evaluated_cells: int  # rows THIS run admitted fresh — the non-cached census `meter` prices; a resume's cached prefix stays the original run's
    response_width: Option[int]  # per-cell output arity; ABSENT on an empty design, where no cell measured one
    indices: dict[str, float]
    discrepancy: Option[float]  # qmc uniformity score for an all-unif qmc design, Nothing for SALib/factorial/non-unif
    elapsed: float
    speedup: Option[float]  # batch-versus-serial ratio under MeasurementMode.SPEEDUP, Nothing otherwise
    seed: int  # sampler and analyzer share this deterministic policy value
    subject: str  # content-keyed benchmark series; objective identity is already admitted into the key
    content_key: ContentKey

    @staticmethod
    def graded(
        study: "Study", design: np.ndarray, measured: Measured, key: ContentKey, seed: int, *, evaluated: Option[int] = Nothing
    ) -> "StudyReceipt":
        # `evaluated` defaults to the whole grid — an unbroken run admits every row fresh; only history's resume
        # grades a narrower fresh-admission census.
        rows = int(design.shape[0])
        return StudyReceipt(
            study.method.tag,
            study.mode,
            rows,
            evaluated.default_value(rows),
            # an empty design measured no cell, so the arity is ABSENT: the retired `0` fabricated a per-cell width
            # nothing produced, and the `meter` fold priced the run at zero cells for a reason no reader could
            # separate from a genuinely zero-width response.
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
        # the spine's warning roster: a bar that did not fail yet qualifies the evidence beside it. An absent
        # response width names a design that evaluated no cell, and the `polynomial` row's indices are IN-SAMPLE
        # screening where every other method's are validated — a reader joining the two without that qualifier
        # compares an optimistic fit against a held-out one and reads the difference as signal.
        return Block.of_seq([
            *(("empty-design",) if self.response_width.is_none() else ()),
            *(("in-sample-r2",) if self.method == "polynomial" else ()),
        ])

    @property
    def span_facts(self) -> dict[str, str | int | float]:
        # bounded scalars only — the full `indices` ledger rides the receipt facts, never the span — and NOT the
        # spine's own columns: subject and content key are the settlement's `concern` and `key`, so re-spelling
        # them here would let one crossing's span and receipt disagree about its own coordinate.
        return {
            "method": self.method,
            "mode": self.mode.value,
            "design_cells": self.design_cells,
            "evaluated_cells": self.evaluated_cells,
            "seed": self.seed,
        }

    def meter(self) -> Option[MeterFact]:
        # `Resource.RECORD` prices the FRESH-ADMISSION surface — every non-cached cell times its response arity — so
        # wide multi-output objectives bill the work they did and a resume bills only the rows it evaluated, never
        # re-billing the cached prefix the original run already charged. Zero quantity charges nothing: a wholly-cached
        # resume lands no row. Series naming lives at the journal owner (`Series.TALLY`), so this charge mints no
        # metric row beside the receipt fan, and the surface is the study method, the one axis a cost fold cuts a
        # study estate on. An ABSENT arity charges nothing for the same reason a zero census does — there is no
        # measured surface to price — and it reaches that answer without a fabricated width standing in for one.
        return self.response_width.bind(
            lambda width: Some(MeterFact(resource=Resource.RECORD, quantity=self.evaluated_cells * width, surface=self.method))
            if self.evaluated_cells * width
            else Nothing
        )

    def benched(self, subject: Option[str] = Nothing) -> tuple[BenchmarkReceipt, ...]:
        # bench projection from the held measurement: `BenchmarkReceipt.of` consumes the elapsed this run already
        # paid for — never a `Bench.run` re-execution — and a SPEEDUP run recovers its serial baseline
        # (elapsed x ratio) as the sibling `.serial` duration series, so the ratio reads off two honest series
        # while the ratio scalar stays receipt evidence, never a bench field. RESULT's zero elapsed contributes
        # nothing; the receipt's default subject already carries the objective-bearing content identity, and a
        # caller may narrow it further. Observability evidence only — no graduation verdict rides it.
        keyed = subject.default_value(self.subject)
        serial = self.speedup.map(lambda ratio: (BenchmarkReceipt.of(f"{keyed}.serial", BenchMode.LATENCY, 0, (self.elapsed * ratio * 1000.0,)),))
        return () if self.elapsed <= 0.0 else (BenchmarkReceipt.of(keyed, BenchMode.LATENCY, 0, (self.elapsed * 1000.0,)), *serial.default_value(()))

    def contribute(self) -> Iterable[Receipt]:
        # ONE settled-receipt spine, never a study-local receipt shape: the payload is this producer's own per-case
        # ledger — native scalars, no `str()` coerce where the deterministic renderer keeps types — while the key,
        # the provenance pair, the warning band, and the stamp are the spine's columns. Provenance names the produced
        # key alone: a study derives its design from its own spec and consumes no upstream key, so a consumed roster
        # here would forge a lineage this fold never walked. The bench projection rides the same harvest, its
        # receipts contributing the `domain="bench"` duration rows with zero runtime edits.
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
    # DOE-frame arm's admission source — `FrameInterop.of` is source-bearing; a zero-arity `FrameInterop()` is not an owned shape.
    frame_backend: Backend = Backend.PYARROW

    async def run(
        self, source: "Objective | object", lane: LanePolicy, /, *, seed: int = 0, composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[StudyReceipt]:
        # an Objective crosses as an argument of one HOSTILE-trait Kernel — sampler natives hold process-global state — while a
        # pre-measured frame decodes inline; the weave owns span, fence, and the fenced contributor harvest. Only the RESULT
        # mode declares idempotent: a worker-death re-run under WALLCLOCK/SPEEDUP would report a post-crash retry as the measurement.
        # ONE mark for the whole entry, threaded to the weave and to the kernel alike. Its census is ABSENT here by
        # construction — the design does not exist until the worker draws it — so the worker RE-STAMPS this carrier
        # once it can state the extent rather than minting a second one beside it. The pre-measured frame arm beats
        # no interior position and its stream carries the open alone, which is the honest report of a fold graded whole.
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
        # this fold is the nearest async owner of the fresh-admission count — the kernel that produced it binds no
        # plane — so the charge lands here, off the CLEARED receipt's own `meter` projection: a refused study
        # evaluated nothing to bill, and an empty charge records nothing.
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.meter().to_list(), scope=composition)).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    def _admit_frame(self, frame: object) -> "RuntimeRail[tuple[np.ndarray, np.ndarray]]":
        # gate proves one Float64 column per `ParamAxis` plus the `response` column, and the decoded design matrix + response
        # vector cross the boundary as numpy buffers — the published data surfaces only.
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
        # ONE study-key builder over the COMPLETE specification — axes, method, mode, the objective's full identity
        # (row/batch scorer shipping identity, the jit route row), the sampler/analyzer seed, and the shape-and-buffer
        # pair of every canonical float64 array — shared by `_execute`, `_graded_frame`, and `RunHistory.resume`, so
        # distinct closures, configurations, or measured response sets never collide onto one cache slot. The parts
        # ride `IdentitySource(parts=...)`, whose count-and-length framing is the `evidence/identity#IDENTITY`
        # `[PREIMAGE_FRAMING]` law: the retired page-local `len(chunk).to_bytes(8, "little")` fold chose a width at a
        # CALL SITE, which forks the key namespace with no surface able to report it, and a shape stays its own part
        # rather than a prefix inside one buffer, so a transposed or reshaped design sharing one byte payload keys
        # apart while dtype and layout normalize onto contiguous float64. An absent response set contributes no parts
        # at all, and the framing counts parts, so a design-only key and a measured key can never collide.
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
        # a pre-measured DOE frame carries its own responses, so evaluation is skipped and the response bytes join the
        # key — two frames sharing one design matrix but carrying different measurements are different studies.
        # The digest rail THREADS rather than re-raising: the retired `raise RuntimeError(fault)` smuggled an
        # already-typed `BoundaryFault` back out through an untyped exception for its own fence to re-classify, and
        # the conversion keeps `str(cause)` — so a key refusal reached its consumer as a message string with its
        # subject, leg, arm, and defect token erased. A rail in hand is returned, never re-raised to be re-classified
        # by a fence that knows less than the value already did.
        measured = Measured(responses[:, None] if responses.ndim == 1 else responses, 0.0, Nothing)
        return self.spec_key(design, responses=Some(responses), seed=seed).map(
            lambda key: StudyReceipt.graded(self, design, measured, key, seed)
        )

    @beartype(conf=FAULT_CONF)
    def _execute(self, objective: Objective, seed: int, mark: StageTap) -> "RuntimeRail[StudyReceipt]":
        design = self.method.design(self.axes, seed)
        # the census exists only once the design is drawn, so the ONE carrier the parent opened with an absent extent
        # gains its total HERE rather than a second mark being minted beside it; the parent could not have stated the
        # extent and did not forge one.
        cells = int(design.shape[0])
        staged = structs.replace(mark, total=Some(cells))
        staged.beat(StudyStage.SAMPLED, cells)
        measured = self.mode.evaluate(objective, design, lambda done: staged.beat(StudyStage.EVALUATED, done))

        def graded(key: ContentKey) -> StudyReceipt:
            # each beat lands AFTER the work it names — `graded` is where the SALib and surrogate folds actually run,
            # so the analysis position reports a completed analysis rather than an intended one.
            staged.beat(StudyStage.KEYED, cells)
            receipt = StudyReceipt.graded(self, design, measured, key, seed)
            staged.beat(StudyStage.ANALYSED, cells)
            return receipt

        # the digest rail THREADS rather than re-raising, for the reason `_graded_frame` states: the retired
        # `raise RuntimeError(fault)` handed an already-typed `BoundaryFault` to its own fence to re-classify, which
        # flattens subject, leg, arm, and defect token into one message string. `spec_key` is the one mint every path
        # shares, which is what makes `history`'s resume key provably equal.
        return self.spec_key(design, Some(objective), seed=seed).map(graded)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
