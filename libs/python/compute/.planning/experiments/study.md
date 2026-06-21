# [PY_COMPUTE_STUDY]

The one study-spine owner over design-of-experiments sampling, global sensitivity analysis, and surrogate fitting. `Study` discriminates by a `StudyMethod` axis over one param-axis and sample-grid spine: low-discrepancy and stratified sampling over the `scipy.stats.qmc` `Sobol`/`Halton`/`LatinHypercube` engine family, the full SALib sensitivity family over Sobol, Morris, FAST, RBD-FAST, delta, PAWN, DGSM, and HDMR composed through one `ProblemSpec` fluent pipeline, and the polynomial-and-estimator surrogate band over `numpy.polynomial` and the scikit-learn estimator protocol are cases on one owner. The eight SALib routes collapse to one `SALIB_ROUTES` table row — sample-module, analyze-module, result-key, and design-matrix demand — driving one sampler body and one analyzer body, never eight near-identical match arms. `MeasurementMode` is a live discriminant, not a label: the owner times the design evaluation through `time.perf_counter` and folds wallclock or a baseline-relative speedup into the receipt rather than standing up a parallel benchmark owner. SALib owns sensitivity analysis, so the owner composes its sampler-and-analyzer pair through `ProblemSpec.sample/analyze` across every method rather than reimplementing variance-based, moment-independent, derivative-based, or component sensitivity; the numpy full-factorial and `numpy.polynomial` surrogate floors run on cp315.

## [01]-[INDEX]

- [01]-[STUDY]: DOE sampling, SALib sensitivity, surrogate fitting, and the live measurement discriminant on one `Study` owner.

## [02]-[STUDY]

- Owner: `Study` — the ONE study-lake owner discriminating by a `StudyMethod` axis over the param-axis, sample-grid, objective-route, and measurement spine; DOE sampling, global sensitivity, and surrogate fitting are cases on one owner. `RunHistory` rides the same spine for persistence and resume in `experiments/history.md#RUN_HISTORY`; the benchmark concern is the live `MeasurementMode` discriminant on the receipt, never a parallel benchmark owner.
- Cases: `StudyMethod` discriminates the DOE samplers (`Lhs(n)`, `Sobol(n)`, and `Halton(n)` over the one `scipy.stats.qmc.LatinHypercube`/`Sobol`/`Halton` engine family with `qmc.scale` and the `qmc.discrepancy` uniformity score, `Factorial(levels)` numpy full-factorial floor), the SALib sensitivity analyzers (`MorrisScreen(trajectories, levels)`, `SobolIndices(n)`, `Fast(n)`, `RbdFast(n)`, `Delta(n)`, `Pawn(n)`, `Dgsm(n)`, `Hdmr(n)`), and the surrogates (`Polynomial(degree)` over the `numpy.polynomial.Polynomial.fit` per-axis least-squares floor reading in-sample `R^2`, `Surrogate(estimator)` over the scikit-learn estimator protocol reading the cross-validated `R^2`). The eight SALib analyzers are not eight bodies: `SALIB_ROUTES` carries one row per tag — `(sample_module, analyze_module, result_key, needs_design)` — so one sampler body resolves `SALib.sample.<module>` and one analyzer body resolves `SALib.analyze.<module>`, reads the row's `ResultDict` key, and feeds the design matrix only when `needs_design` (the Morris/RBD-FAST/delta/PAWN/DGSM/HDMR `X`-and-`Y` analyzers) versus the responses alone (the Sobol/FAST `Y`-only analyzers over their structured design). `MeasurementMode` is a live policy value — `Result` records the indices only, `Wallclock` times the design evaluation through `time.perf_counter`, `Speedup` times the same evaluation against a per-row serial baseline and folds the ratio — never a dead label.
- Entry: `Study.run` matches the method, builds the SALib `ProblemSpec` from the param axes, draws the design through the matching `qmc` engine, numpy floor, or `SALIB_ROUTES` sampler, evaluates the objective per design row under the `MeasurementMode` timing policy, runs the matching SALib analyzer through `ProblemSpec.analyze` for the sensitivity indices, and folds `StudyReceipt.graded` carrying the method, mode, completed and total cells, the per-axis sensitivity indices, the `qmc.discrepancy` design-uniformity score, the elapsed wallclock and optional baseline speedup, and the design `ContentKey`. The `ProblemSpec` sampler-and-analyzer pipeline is the canonical sensitivity owner, so Morris elementary effects, Saltelli first-and-total-order Sobol indices, RBD-FAST random-balance indices, and Borgonovo delta moment-independent measures are read from SALib, never hand-rolled.
- Packages: `SALib` (`ProblemSpec` fluent `sample`/`set_samples`/`set_results`/`analyze` with the `samples`/`analysis` accessors, the `SALib.sample.<morris|sobol|fast_sampler|finite_diff|latin>` and `SALib.analyze.<morris|sobol|fast|rbd_fast|delta|pawn|dgsm|hdmr>` submodules resolved through `importlib.import_module`), `scipy` (`stats.qmc.LatinHypercube`/`Sobol`/`Halton` with the SPEC-007 `rng` constructor keyword, `qmc.scale`, `qmc.discrepancy`), `scikit-learn` (the `pipeline.Pipeline` of `preprocessing.StandardScaler` and a confirmed regressor `ensemble.GradientBoostingRegressor`/`ensemble.RandomForestRegressor`/`svm.SVR`/`linear_model.Ridge`, scored through `model_selection.cross_val_score` reading `metrics.r2_score`), `numpy` (`random.default_rng`, `linspace`, `meshgrid`, `stack`, `argsort`, `polynomial.Polynomial.fit`), `time.perf_counter` for the measurement policy, data-branch `xarray`/`dask` shapes for the grid lane, runtime (`RuntimeRail`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new param axis is one `ParamAxis` coordinate; a new SALib analyzer is one `StudyMethod` case plus one `SALIB_ROUTES` row, no new body; a new `qmc` engine or numpy floor is one case in the `_design` table; a new surrogate estimator is one `SurrogateKind` row resolving its scikit-learn class; a new measurement is one `MeasurementMode` row reading the shared timing fold; zero new surface.
- Boundary: no job framework, farm scheduler, substrate selection, or C# receipt minting; classical DOE, sensitivity, and surrogate only — a neural surrogate or an acquisition-driven active-learning loop is out of charter. A standalone benchmark owner, a parallel experiment tracker, a hand-rolled Morris or Saltelli implementation, a hand-rolled stratified-LHS over a numpy floor where the `qmc` engine owns it, eight near-identical SALib match arms, an in-sample score on the scikit-learn `Surrogate` row where cross-validation is the honest generalization estimate (the per-axis polynomial floor's in-sample `R^2` is a cheap univariate screening diagnostic, not a generalization claim), and a `NotImplementedError` sensitivity stub are the deleted forms. SALib is pure-Python over numpy/scipy, so its core runs where numpy resolves; the scipy `qmc` engine family and scikit-learn surrogate rows carry the `python_version<'3.15'` marker, and the numpy factorial and `numpy.polynomial` surrogate floors run on cp315.

```python signature
from __future__ import annotations

import time
from collections.abc import Callable
from enum import StrEnum
from importlib import import_module
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from expression import Nothing, Option, Some, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from SALib import ProblemSpec

type Objective = Callable[[np.ndarray], float]
type SalibTag = Literal["morris_screen", "sobol_indices", "fast", "rbd_fast", "delta", "pawn", "dgsm", "hdmr"]


class MeasurementMode(StrEnum):
    RESULT = "result"        # record the sensitivity indices only; report zero elapsed
    WALLCLOCK = "wallclock"  # time the whole design evaluation as one batch
    SPEEDUP = "speedup"      # fold the batch wallclock against a per-row serial baseline ratio

    def evaluate(self, objective: Objective, design: np.ndarray) -> tuple[np.ndarray, float, Option[float]]:
        match self:
            case MeasurementMode.RESULT:
                return _responses(objective, design), 0.0, Nothing
            case MeasurementMode.WALLCLOCK:
                start = time.perf_counter()
                responses = _responses(objective, design)
                return responses, time.perf_counter() - start, Nothing
            case MeasurementMode.SPEEDUP:
                start = time.perf_counter()
                responses = _responses(objective, design)
                elapsed = time.perf_counter() - start
                serial = time.perf_counter()
                for row in design:
                    objective(row)
                baseline = time.perf_counter() - serial
                ratio = Some(baseline / elapsed) if elapsed > 0.0 else Nothing
                return responses, elapsed, ratio
            case unreachable:
                assert_never(unreachable)


class SurrogateKind(StrEnum):
    GRADIENT_BOOST = "gradient_boost"  # ensemble.GradientBoostingRegressor
    RANDOM_FOREST = "random_forest"    # ensemble.RandomForestRegressor
    SVR = "svr"                        # svm.SVR
    RIDGE = "ridge"                    # linear_model.Ridge

    def estimator(self) -> object:
        module, name = _SURROGATE_CLASS[self]
        return getattr(import_module(f"sklearn.{module}"), name)()


class ParamAxis(Struct, frozen=True):
    name: str
    low: float
    high: float

    def rescale(self, unit_col: np.ndarray) -> np.ndarray:
        return self.low + (self.high - self.low) * unit_col


@tagged_union(frozen=True)
class StudyMethod:
    tag: Literal[
        "lhs", "factorial", "sobol", "halton", "morris_screen", "sobol_indices", "fast", "rbd_fast", "delta", "pawn", "dgsm", "hdmr", "polynomial", "surrogate"
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

    @staticmethod
    def Lhs(n: int) -> StudyMethod:
        return StudyMethod(lhs=n)

    @staticmethod
    def Factorial(levels: tuple[int, ...]) -> StudyMethod:
        return StudyMethod(factorial=levels)

    @staticmethod
    def Sobol(n: int) -> StudyMethod:
        return StudyMethod(sobol=n)

    @staticmethod
    def Halton(n: int) -> StudyMethod:
        return StudyMethod(halton=n)

    @staticmethod
    def MorrisScreen(trajectories: int, levels: int) -> StudyMethod:
        return StudyMethod(morris_screen=(trajectories, levels))

    @staticmethod
    def SobolIndices(n: int) -> StudyMethod:
        return StudyMethod(sobol_indices=n)

    @staticmethod
    def Fast(n: int) -> StudyMethod:
        return StudyMethod(fast=n)

    @staticmethod
    def RbdFast(n: int) -> StudyMethod:
        return StudyMethod(rbd_fast=n)

    @staticmethod
    def Delta(n: int) -> StudyMethod:
        return StudyMethod(delta=n)

    @staticmethod
    def Pawn(n: int) -> StudyMethod:
        return StudyMethod(pawn=n)

    @staticmethod
    def Dgsm(n: int) -> StudyMethod:
        return StudyMethod(dgsm=n)

    @staticmethod
    def Hdmr(n: int) -> StudyMethod:
        return StudyMethod(hdmr=n)

    @staticmethod
    def Polynomial(degree: int) -> StudyMethod:
        return StudyMethod(polynomial=degree)

    @staticmethod
    def Surrogate(kind: SurrogateKind = SurrogateKind.GRADIENT_BOOST) -> StudyMethod:
        return StudyMethod(surrogate=kind)


class StudyReceipt(Struct, frozen=True):
    method: str
    mode: MeasurementMode
    cells_completed: int
    cells_total: int
    indices: dict[str, float]
    discrepancy: Option[float]  # qmc low-discrepancy uniformity score for a qmc-drawn design, Nothing otherwise
    elapsed: float
    speedup: Option[float]      # batch-versus-serial ratio under MeasurementMode.SPEEDUP, Nothing otherwise
    content_key: ContentKey

    @staticmethod
    def graded(
        study: Study, design: np.ndarray, responses: np.ndarray, indices: dict[str, float],
        elapsed: float, speedup: Option[float], discrepancy: Option[float], key: ContentKey,
    ) -> StudyReceipt:
        return StudyReceipt(
            study.method.tag, study.mode, int(responses.size), int(design.shape[0]),
            indices, discrepancy, elapsed, speedup, key,
        )

    def contribute(self) -> Receipt:
        facts = {
            "mode": self.mode.value,
            "cells": f"{self.cells_completed}/{self.cells_total}",
            "elapsed": f"{self.elapsed:.6g}",
            "discrepancy": repr(self.discrepancy.map(lambda d: round(d, 6)).default_value(None)),
            "speedup": repr(self.speedup.map(lambda s: round(s, 3)).default_value(None)),
            **{f"S[{k}]": f"{v:.6g}" for k, v in self.indices.items()},
        }
        return Receipt.of("emitted", "compute.study", self.method, facts)


class Study(Struct, frozen=True):
    axes: tuple[ParamAxis, ...]
    method: StudyMethod
    mode: MeasurementMode

    def run(self, objective: Objective, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        return boundary(f"study.{self.method.tag}", lambda: _execute(self, objective, seed))

    def problem(self) -> dict[str, object]:
        return {"num_vars": len(self.axes), "names": [ax.name for ax in self.axes], "bounds": [[ax.low, ax.high] for ax in self.axes]}

    def spec(self) -> ProblemSpec:
        from SALib import ProblemSpec

        return ProblemSpec(self.problem())


_SALIB_ROUTES: dict[SalibTag, tuple[str, str, str, bool]] = {
    "morris_screen": ("morris", "morris", "mu_star", True),
    "sobol_indices": ("sobol", "sobol", "ST", False),
    "fast": ("fast_sampler", "fast", "ST", False),
    "rbd_fast": ("latin", "rbd_fast", "S1", True),
    "delta": ("latin", "delta", "delta", True),
    "pawn": ("sobol", "pawn", "median", True),
    "dgsm": ("finite_diff", "dgsm", "dgsm", True),
    "hdmr": ("latin", "hdmr", "Sa", True),
}

_SURROGATE_CLASS: dict[SurrogateKind, tuple[str, str]] = {
    SurrogateKind.GRADIENT_BOOST: ("ensemble", "GradientBoostingRegressor"),
    SurrogateKind.RANDOM_FOREST: ("ensemble", "RandomForestRegressor"),
    SurrogateKind.SVR: ("svm", "SVR"),
    SurrogateKind.RIDGE: ("linear_model", "Ridge"),
}


def _responses(objective: Objective, design: np.ndarray) -> np.ndarray:
    return np.stack([np.asarray(objective(row), dtype=float) for row in design])


def _execute(study: Study, objective: Objective, seed: int) -> StudyReceipt:
    design = _design(study, seed)
    responses, elapsed, speedup = study.mode.evaluate(objective, design)
    key = ContentIdentity.of("study", design.tobytes(), IdentityPolicy())
    indices = _indices(study, design, responses)
    return StudyReceipt.graded(study, design, responses, indices, elapsed, speedup, _discrepancy(study, design), key)


def _design(study: Study, seed: int) -> np.ndarray:
    match study.method:
        case StudyMethod(tag="lhs" | "sobol" | "halton" | "polynomial" | "surrogate"):
            return _qmc_design(study, seed)
        case StudyMethod(tag="factorial", factorial=levels):
            unit = _full_factorial(levels)
            return np.stack([ax.rescale(unit[:, j]) for j, ax in enumerate(study.axes)], axis=1)
        case StudyMethod(tag="morris_screen", morris_screen=(traj, levels)):
            sampler = import_module("SALib.sample.morris").sample
            return study.spec().sample(sampler, traj, num_levels=levels, seed=seed).samples
        case StudyMethod(tag="sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr") as m:
            sampler = import_module(f"SALib.sample.{_SALIB_ROUTES[m.tag][0]}").sample
            return study.spec().sample(sampler, getattr(m, m.tag), seed=seed).samples
        case unreachable:
            assert_never(unreachable)


def _full_factorial(levels: tuple[int, ...]) -> np.ndarray:
    grids = np.meshgrid(*[np.linspace(0.0, 1.0, k) for k in levels], indexing="ij")
    return np.stack([g.reshape(-1) for g in grids], axis=1)


def _qmc_design(study: Study, seed: int) -> np.ndarray:
    from scipy.stats import qmc

    dim = len(study.axes)
    bounds = study.problem()["bounds"]
    lo, hi = [b[0] for b in bounds], [b[1] for b in bounds]
    match study.method:
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
        case unreachable:
            assert_never(unreachable)
    return qmc.scale(unit, lo, hi)


def _discrepancy(study: Study, design: np.ndarray) -> Option[float]:
    match study.method:
        case StudyMethod(tag="lhs" | "sobol" | "halton" | "polynomial" | "surrogate"):
            from scipy.stats import qmc

            lo = np.asarray([ax.low for ax in study.axes], dtype=float)
            hi = np.asarray([ax.high for ax in study.axes], dtype=float)
            return Some(float(qmc.discrepancy((design - lo) / np.where(hi > lo, hi - lo, 1.0))))
        case _:
            return Nothing


def _indices(study: Study, design: np.ndarray, responses: np.ndarray) -> dict[str, float]:
    names = [ax.name for ax in study.axes]
    match study.method:
        case StudyMethod(tag="morris_screen" | "sobol_indices" | "fast" | "rbd_fast" | "delta" | "pawn" | "dgsm" | "hdmr"):
            return _salib_indices(study, study.method.tag, design, responses, names)
        case StudyMethod(tag="polynomial", polynomial=degree):
            return {ax.name: _axis_r2(design[:, j], responses, degree) for j, ax in enumerate(study.axes)}
        case StudyMethod(tag="surrogate", surrogate=kind):
            return {"cv_r2": _surrogate_cv_r2(kind, design, responses)}
        case StudyMethod(tag="lhs" | "factorial" | "sobol" | "halton"):
            return {}
        case unreachable:
            assert_never(unreachable)


def _salib_indices(study: Study, tag: SalibTag, design: np.ndarray, responses: np.ndarray, names: list[str]) -> dict[str, float]:
    _, analyze_mod, result_key, needs_design = _SALIB_ROUTES[tag]
    analyzer = import_module(f"SALib.analyze.{analyze_mod}").analyze
    spec = study.spec().set_results(responses)
    result = (spec.set_samples(design) if needs_design else spec).analyze(analyzer).analysis
    return {n: float(v) for n, v in zip(names, result[result_key], strict=True)}


def _axis_r2(column: np.ndarray, responses: np.ndarray, degree: int) -> float:
    fit = np.polynomial.Polynomial.fit(column, responses, degree)
    residual = responses - fit(column)
    ss_res = float(residual @ residual)
    centered = responses - responses.mean()
    ss_tot = float(centered @ centered) or 1.0
    return 1.0 - ss_res / ss_tot


def _surrogate_cv_r2(kind: SurrogateKind, design: np.ndarray, responses: np.ndarray) -> float:
    from sklearn.model_selection import cross_val_score
    from sklearn.pipeline import Pipeline
    from sklearn.preprocessing import StandardScaler

    pipeline = Pipeline([("scale", StandardScaler()), ("model", kind.estimator())])
    return float(cross_val_score(pipeline, design, responses, cv=5, scoring="r2").mean())
```

The `scipy.stats.qmc` engine family owns every unit-hypercube draw: the `LatinHypercube` engine stratifies each axis into equal-probability bins under SPEC-007 `rng` seeding for the `lhs` DOE row, the polynomial row, and the surrogate sampling, the `Sobol` engine draws `random(n)` scrambled low-discrepancy points, the `Halton` engine draws `random(n)`, `qmc.scale` affinely maps each unit sample to the bounds box, and `qmc.discrepancy` scores the unit-cube uniformity that folds onto the receipt as design-quality evidence; the factorial row builds the full numpy grid through `meshgrid`/`stack`, and the polynomial row fits a univariate `numpy.polynomial.Polynomial.fit` model per axis and reads in-sample `R^2`. SALib owns the sensitivity domain across every method through one `ProblemSpec` fluent pipeline collapsed to one sampler body and one analyzer body over the `_SALIB_ROUTES` row table — `spec.sample(sampler, ...)` draws the structured design and `spec.set_results(Y)`, conditionally `set_samples(X)` for the `needs_design` rows, then `analyze(analyzer).analysis` reads the row's `ResultDict` key: the Morris row reads `mu_star`, the Sobol row reads first-and-total-order `ST` over a Saltelli design, the FAST row reads `ST`, the RBD-FAST row reads the random-balance-design first-order `S1` over a Latin-hypercube design, the delta row reads the Borgonovo moment-independent `delta` over a Latin-hypercube design, the PAWN row reads the `median` over a Sobol design, the DGSM row reads the derivative-based `dgsm` index over a `finite_diff` design, and the HDMR row reads the component `Sa` over a Latin-hypercube design. The `Surrogate(kind)` row composes a `Pipeline` of `StandardScaler` and a `SurrogateKind`-resolved scikit-learn regressor and reads the honest cross-validated `R^2` through `cross_val_score`, never the in-sample fit score. `MeasurementMode.evaluate` is the one timing fold: `Result` records indices alone, `Wallclock` times the batch design evaluation, and `Speedup` additionally re-runs the objective per row to fold the batch-versus-serial ratio onto the receipt. The charter boundary holds at the surrogate seam: classical polynomial and ensemble/kernel regression surrogates are in-scope; a neural surrogate or an acquisition-driven active-learning loop is not.

## [03]-[RESEARCH]

- [SALIB_PROBLEMSPEC]: `SALib` resolves on the cp315 core (pure-Python over numpy/scipy). The owner drives the SALib family through one `ProblemSpec` fluent pipeline — `ProblemSpec(problem)`, `spec.sample(func, *args, **kwargs).samples`, `spec.set_samples(X)`, `spec.set_results(Y)`, `spec.analyze(func).analysis` — per the `.api` `[LOCAL_ADMISSION]` "use `ProblemSpec` for new analysis pipelines". The `ProblemSpec` constructor, the `sample`/`set_samples`/`set_results`/`analyze` pipeline methods, and the `samples`/`analysis` accessors verify against `compute/.api/salib.md` `[03]-[ENTRYPOINTS]`. The eight analyzers collapse to the `_SALIB_ROUTES` row table — `(sample_module, analyze_module, result_key, needs_design)` — resolved through stdlib `importlib.import_module(f"SALib.sample.{module}")`/`import_module(f"SALib.analyze.{module}")`, so the sample and analyze submodules verified in `salib.md`'s sample/analyze submodule tables drive one sampler body and one analyzer body rather than eight near-identical match arms.
- [SALIB_RESULT_KEYS]: the eight analyze submodules (`morris`, `sobol`, `fast`, `rbd_fast`, `delta`, `pawn`, `dgsm`, `hdmr`) are each catalogued in `compute/.api/salib.md` `[02]` analyze-submodule table, and the two `ResultDict` key families the `.api` `[04]-[IMPLEMENTATION_LAW]` documents anchor the table — Sobol `S1`/`S2`/`ST` and Morris `mu`/`mu_star`/`sigma`. `_SALIB_ROUTES` selects the per-method scalar each analyzer returns over its `ResultDict`: `morris_screen` reads `mu_star` and the Sobol/FAST variance routes read `ST` (both confirmed key families); `rbd_fast` reads the first-order `S1`, `delta` the Borgonovo moment-independent `delta`, `pawn` the per-input `median` of the KS statistic, `dgsm` the derivative-based `dgsm` index, and `hdmr` the component `Sa` — the standard `ResultDict` key each analyzer emits, resolved at runtime from the imported analyze module rather than re-declared. `ProblemSpec.analyze` threads the stored sample matrix and result array to each analyzer over its signature — the `needs_design` rows (`rbd_fast.analyze(problem, X, Y, M=10, ...)`, `delta.analyze(problem, X, Y, ..., method='all')`, `pawn.analyze(problem, X, Y, S=10)` are catalogued in `salib.md` `[03]` analyze table; the `dgsm`/`hdmr` analyzers take `(problem, X, Y, ...)` over their imported signature; `morris.analyze(problem, X, Y)`) call `set_samples(design)` before `analyze`, while the `Y`-only rows (`sobol.analyze(problem, Y)`/`fast.analyze(problem, Y)`) read the responses against their structured design — each keyed by axis name so a `RunHistory` compare join lands on matching rows.
- [SCIPY_QMC]: the `scipy.stats.qmc.LatinHypercube`/`Sobol`/`Halton`/`scale`/`discrepancy` spellings and the SPEC-007 `rng` constructor keyword (`seed` is the deprecated interim alias) verify against `compute/.api/scipy.md` `[03]-[ENTRYPOINTS]` `scipy.stats.qmc`; the `.random(n)` draw is the catalogued engine surface, and `qmc.discrepancy(unit)` folds the unit-cube uniformity score onto the receipt as design-quality evidence. The engine family carries the `python_version<'3.15'` marker and resolves once scipy ships cp315 wheels.
- [SKLEARN_SURROGATE]: the surrogate row composes only `.api`-confirmed `scikit-learn` members — `pipeline.Pipeline`, `preprocessing.StandardScaler`, the `SurrogateKind`-resolved regressors `ensemble.GradientBoostingRegressor`/`ensemble.RandomForestRegressor`/`svm.SVR`/`linear_model.Ridge`, and `model_selection.cross_val_score` with `scoring="r2"` over the `metrics.r2_score` rail — all present in `compute/.api/scikit-learn.md`'s supervised-estimator, composition, and model-selection tables; the rejected `gaussian_process` surrogate was an uncatalogued member and the in-sample `model.score` was a generalization-estimate defect, both replaced by the cross-validated pipeline. The estimator class resolves through `getattr(import_module(f"sklearn.{module}"), name)`. The numpy factorial and `numpy.polynomial.Polynomial.fit` floors run unconditionally on cp315 (the `polynomial` namespace is catalogued in `numpy.md`'s `[COMPUTE_TOPOLOGY]`); the `lhs`, polynomial, and surrogate-sampling LHS draws route through the `qmc.LatinHypercube` engine carrying the same `<3.15` marker as the rest of the engine family, and the scikit-learn pipeline carries its own `<3.15` marker.
