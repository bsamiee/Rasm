# [PY_COMPUTE_STUDY]

The one study-spine owner over design-of-experiments sampling, global sensitivity analysis, and surrogate fitting: `Study` discriminates by a `StudyMethod` axis over one param-axis and sample-grid spine, and the union owns its `design`/`discrepancy`/`indices` folds, so `experiments/history.md#RUN_HISTORY` composes `study.method.design`/`indices` directly rather than importing a private across the package seam. SALib owns sensitivity analysis — the owner composes its sampler-and-analyzer pairs through `ProblemSpec` rather than reimplementing variance-based, moment-independent, derivative-based, or component sensitivity. Classical polynomial and ensemble/kernel regression surrogates are in scope; a neural surrogate and an acquisition-driven active-learning loop are not.

The run rides the `EvidenceScope.STUDY` weave — span, `boundary` fence, beartype guard, `@receipted` harvest. The seams: `numerics/jit` supplies `JitBackend`/`LoweredSpec` for the batch-lane compile and the symbolic-lowered spec VALUE; `data/tabular` supplies the `FrameAdmission`/`FrameInterop`/`FieldShape`/`Backend` DOE-frame gate and the `columnar.arrow_bytes` render fold — the published surfaces only, no data interior; the lane resolves by payload shape on the runtime `Modality` axis.

## [01]-[INDEX]

- [01]-[STUDY]: DOE sampling, SALib sensitivity, and surrogate fitting on one `Study` owner — the union-owned folds, the `SALIB_ROUTES` table, and the live `Measured` measurement discriminant.

## [02]-[STUDY]

- Owner: `Study` — DOE sampling, global sensitivity, and surrogate fitting are cases on one owner; the benchmark concern is the live `MeasurementMode` discriminant folded into `Measured`, never a parallel benchmark owner; `RunHistory` rides the same spine for persistence and resume.
- Cases: the union's keyword constructor is the one construction surface, no sibling factory family; the eight SALib analyzers are one routed sampler body and one routed analyzer body over `SALIB_ROUTES`, the per-method knobs folded from the case payload through `_salib_args`, never a hardcoded arm beside the table.
- Entry: `Study.run` is one polymorphic entry discriminating by input shape — an `Objective` runs the sampled evaluation, a contract-gated DOE frame grades a pre-measured cohort — never a second entry.
- Output: `Measured` carries the responses, the wallclock, and the `Option[float]` batch-versus-serial speedup that is `Nothing` for a bare row objective — never a fabricated ratio over the identical per-row work timed twice; the `surrogate` row reads the honest cross-validated `R^2` while the `polynomial` row's in-sample `R^2` is the cheap univariate screening diagnostic.
- Growth: a new input marginal is one `AxisDist` member plus one `rescale` arm and one `bounds` arm; a new SALib analyzer is one `StudyMethod` case plus one `SALIB_ROUTES` row, no new body; a new `qmc` engine or numpy floor is one arm on `_qmc`/`design`; a new surrogate estimator is one `SurrogateKind` member plus one `SURROGATE_CLASS` row; a new measurement is one `MeasurementMode` member reading the shared `Measured` fold.

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

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.numerics.jit import JitBackend, LoweredSpec
from rasm.data.tabular.contract import FrameAdmission
from rasm.data.tabular.interop import Backend, FieldShape, FrameInterop
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from SALib import ProblemSpec
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


class Objective(Struct, frozen=True):
    row: RowScorer
    batch: Option[BatchScorer] = Nothing  # vectorized fast lane; absent objectives score per-row only
    jit: Option[JitBackend] = Nothing  # the loop-kernel accelerator row the batch lane compiles through

    @staticmethod
    def lowered(spec: LoweredSpec) -> "Objective":
        # the spec's kernel is the row scorer and its recommended route arms the batch-lane compile — the symbolic->jit->study
        # chain with zero symbolic imports.
        return Objective(row=spec.kernel, jit=Some(spec.route))

    def scorer(self) -> RowScorer:
        # a Some `jit` row compiles the row scorer through `JitBackend.compile`; a compile fault degrades to the host row.
        return self.jit.map(lambda route: route.compile(self.row).map(lambda jitted: jitted.fn).default_value(self.row)).default_value(self.row)

    def rows(self, design: np.ndarray) -> np.ndarray:
        scorer = self.scorer()
        return np.stack([np.asarray(scorer(point), dtype=float) for point in design])


def _importable_kernel(objective: Objective) -> bool:
    # the process-lane payload law: only a module-qualified importable callable set crosses the
    # process band; a closure or lambda anywhere in the objective drops to the THREAD band.
    def importable(fn: object) -> bool:
        name = getattr(fn, "__qualname__", "<lambda>")
        return "<lambda>" not in name and "<locals>" not in name

    return importable(objective.row) and objective.batch.map(importable).default_value(True)


def _run_kernel(study: "Study", objective: Objective, seed: int) -> "RuntimeRail[StudyReceipt]":
    # module-level so the worker resolves it by import; the fence converts a sampler/analyzer/fit raise.
    return boundary(f"study.{study.method.tag}", lambda: study._execute(objective, seed))


def _timed[T](thunk: Callable[[], T]) -> tuple[T, float]:
    # the one perf_counter fold the measurement arms share.
    start = time.perf_counter()
    value = thunk()
    return value, time.perf_counter() - start


class MeasurementMode(StrEnum):
    RESULT = "result"  # evaluate the design but suppress the wallclock; report zero elapsed
    WALLCLOCK = "wallclock"  # time the whole design evaluation as one batch
    SPEEDUP = "speedup"  # fold the vectorized batch wallclock against the per-row serial baseline

    def evaluate(self, objective: Objective, design: np.ndarray) -> "Measured":
        # `RESULT`/`WALLCLOCK` and the bare `SPEEDUP` (no batch lane) collapse to one timed-`fast` arm; only a real
        # `objective.batch` lane earns the second timed pass folding the honest serial-over-batch ratio.
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

    # the support endpoints the sampler scales into; `norm`/`lognorm` are unbounded, so the row reads a wide window off mean±std.
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

    # the qmc path inverse-transforms the unit draw through the exact `scipy.stats.<dist>.ppf` form the SALib `dists` channel
    # resolves, so the two draws are the same marginal rather than two divergent inverse transforms.
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


# named fields, never a four-positional tuple indexed by position; `needs_design` discriminates the paired `(X, Y)` analyzers
# from the `Y`-only routes.
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

    # the union OWNS its three folds — `design`/`discrepancy`/`indices`, each a total `match` taking `axes` as payload — so no
    # detached free function reaches back into the union.
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

    # `qmc.discrepancy` is honest only over an all-`unif` design where the affine de-scale recovers the unit draw; a non-`unif`
    # marginal shapes the design out of the box and the score is `Nothing` rather than a misread.
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
        # one rescale path across the qmc and factorial floors — `qmc.scale` is not a second uniform-only scaling surface.
        return np.stack([ax.rescale(unit[:, j]) for j, ax in enumerate(axes)], axis=1)

    @staticmethod
    def _spec(axes: tuple[ParamAxis, ...]) -> "ProblemSpec":
        from SALib import ProblemSpec

        # the SALib samplers shape their own marginals off `problem['dists']` (emitted only when an axis is non-`unif`); when set,
        # SALib reads each `bounds` row as the dist's FULL parameter vector, so the row is `ax.params` itself, not `(low, high)`.
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

# --- [TABLES] ---------------------------------------------------------------------------

# one route row per tag drives one sampler body and one analyzer body; `result_key` selects the per-method scalar over the `ResultDict`.
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
    response_width: int  # per-cell output arity — a multi-output objective is a parameterized fact
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
        # bounded scalars only — the full `indices` ledger rides the receipt facts, never the span.
        return {
            "method": self.method,
            "mode": self.mode.value,
            "design_cells": self.design_cells,
            "response_width": self.response_width,
            "content_key": self.content_key.hex,
        }

    def render(self, design: np.ndarray, responses: np.ndarray, axes: "tuple[ParamAxis, ...]") -> bytes:
        # the render seam: one self-describing frame crossing as content-keyed Arrow bytes through the data-owned
        # `columnar.arrow_bytes` fold; the artifacts consumer decodes the frame and never re-derives the cohort.
        import pyarrow as pa

        from rasm.data.tabular.columnar import arrow_bytes

        columns = {axis.name: pa.array(np.asarray(design[:, i], dtype=float)) for i, axis in enumerate(axes)}
        columns |= {f"response_{j}": pa.array(np.asarray(responses).reshape(len(design), -1)[:, j]) for j in range(self.response_width)}
        metadata = {b"source": b"compute.study", b"identifier": self.method.encode(), b"unit": b"dimensionless", b"content_key": self.content_key.hex.encode()}
        return bytes(arrow_bytes(pa.table(columns).replace_schema_metadata(metadata)))

    def contribute(self) -> Iterable[Receipt]:
        # native scalars only — no `str()` coerce where the deterministic renderer keeps types.
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
    # the DOE-frame arm's admission source — `FrameInterop.of` is source-bearing; a zero-arity `FrameInterop()` is not an owned shape.
    frame_backend: Backend = Backend.PYARROW

    async def run(self, source: "Objective | object", lane: LanePolicy, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        # the lane resolves BY PAYLOAD SHAPE: a module-qualified objective crosses the PROCESS band as spec data, a closure-bearing
        # objective rides the THREAD band (a lambda on the process lane is a runtime pickle crash), a pre-measured frame decodes
        # inline; the weave owns span, fence, and the `@receipted` receipt harvest.
        async def dispatch() -> RuntimeRail[StudyReceipt]:
            match source:
                case Objective() as objective:
                    modality = Modality.PROCESS if _importable_kernel(objective) else Modality.THREAD
                    return (await lane.offload(_run_kernel, self, objective, seed, modality=modality)).bind(lambda rail: rail)
                case frame:
                    return self._admit_frame(frame).bind(
                        lambda decoded: boundary(f"study.{self.method.tag}", lambda: self._graded_frame(*decoded))
                    )

        return await evidence_run(EvidenceScope.STUDY, f"study.{self.method.tag}", dispatch)

    def _admit_frame(self, frame: object) -> "RuntimeRail[tuple[np.ndarray, np.ndarray]]":
        # the gate proves one Float64 column per `ParamAxis` plus the `response` column, and the decoded design matrix + response
        # vector cross the boundary as numpy buffers — the published data surfaces only.
        shapes = tuple(FieldShape(field=axis.name, logical_type="Float64", nullable=False) for axis in self.axes)
        gate = FrameAdmission.of(FrameInterop.of(self.frame_backend), (*shapes, FieldShape(field="response", logical_type="Float64", nullable=False)))
        return gate.admit(frame).bind(
            lambda admitted: gate.enforce(admitted).map(
                lambda _claim: (
                    np.column_stack([np.asarray(admitted.frame[axis.name], dtype=float) for axis in self.axes]),
                    np.asarray(admitted.frame["response"], dtype=float),
                )
            )
        )

    def _graded_frame(self, design: np.ndarray, responses: np.ndarray) -> StudyReceipt:
        # a pre-measured DOE frame carries its own responses, so evaluation is skipped and the
        # receipt grades the decoded cohort under the same design content key the resume proof reads.
        measured = Measured(responses[:, None] if responses.ndim == 1 else responses, 0.0, Nothing)
        match ContentIdentity.of("study", design.tobytes()):
            case Ok(key):
                return StudyReceipt.graded(self, design, measured, key)
            case Error(fault):
                raise RuntimeError(fault)

    @beartype(conf=FAULT_CONF)
    def _execute(self, objective: Objective, seed: int) -> StudyReceipt:
        design = self.method.design(self.axes, seed)
        measured = self.mode.evaluate(objective, design)
        # the design key is `match`ed off the rail inside the already-fenced body, so a hash `Error` re-raises onto the `boundary`;
        # the default policy keys identically to an explicit allocation, which is what makes `history`'s resume key provably equal.
        match ContentIdentity.of("study", design.tobytes()):
            case Ok(key):
                return StudyReceipt.graded(self, design, measured, key)
            case Error(fault):
                raise RuntimeError(fault)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
