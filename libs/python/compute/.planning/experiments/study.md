# [PY_COMPUTE_STUDY]

The one study-spine owner over design-of-experiments sampling, global sensitivity analysis, and surrogate fitting. `Study` discriminates by a `StudyMethod` axis over one param-axis and sample-grid spine: low-discrepancy and stratified sampling over `scipy.stats.qmc`, Sobol/Morris/FAST sensitivity over SALib, and polynomial or Gaussian-process surrogates over scikit-learn are cases on one owner. `BenchmarkData` collapses into a `MeasurementMode` discriminant on the receipt rather than a parallel benchmark owner. SALib owns sensitivity analysis, so the owner composes its sampler-and-analyzer pair rather than reimplementing Morris or Saltelli; the numpy stratified-LHS and factorial floors run on cp315.

## [1]-[INDEX]

[STUDY]: DOE sampling, SALib sensitivity, surrogate fitting, and the benchmark discriminant on one `Study` owner.

## [2]-[STUDY]

- Owner: `Study` — the ONE study-lake owner discriminating by a `StudyMethod` axis over the param-axis, sample-grid, objective-route, and measurement spine; DOE sampling, global sensitivity, and surrogate fitting are cases on one owner. `RunHistory` rides the same spine for persistence and resume in `experiments/run_history.md#RUN_HISTORY`; `BenchmarkData` collapses into a `MeasurementMode` discriminant on the receipt.
- Cases: `StudyMethod` discriminates the DOE samplers (`Lhs(n)` numpy stratified Latin-hypercube floor, `Factorial(levels)` numpy full-factorial floor, `Sobol(m)` and `Halton(n)` over `scipy.stats.qmc.Sobol`/`Halton` with `qmc.scale`), the SALib sensitivity analyzers (`MorrisScreen(trajectories, levels)` over `SALib.sample.morris`/`SALib.analyze.morris`, `SobolIndices(n)` over `SALib.sample.sobol`/`SALib.analyze.sobol`, `Fast(n)` over `SALib.sample.fast_sampler`/`SALib.analyze.fast`), and the surrogates (`Polynomial(degree)` numpy least-squares Vandermonde floor, `GaussianProcess(length_scale)` over `sklearn.gaussian_process.GaussianProcessRegressor` with an `RBF` kernel). `MeasurementMode` discriminates result, wallclock, and speedup measurement on the receipt.
- Entry: `Study.run` matches the method, builds the SALib `problem` dict from the param axes, draws the design through the matching SALib sampler or numpy floor, evaluates the objective per design row, runs the matching SALib analyzer for the sensitivity indices, and returns `RuntimeRail[StudyReceipt]` carrying the method, mode, completed and total cells, the per-axis sensitivity indices, and the design `ContentKey`. The SALib sampler-and-analyzer pair is the canonical sensitivity owner, so Morris elementary effects and Saltelli first-and-total-order Sobol indices are read from SALib, never hand-rolled.
- Packages: `SALib` (`sample.morris`, `sample.sobol`, `sample.fast_sampler`, `analyze.morris`, `analyze.sobol`, `analyze.fast`, the `ProblemSpec`/`problem` dict), `scipy` (`stats.qmc.Sobol`, `stats.qmc.Halton`, `stats.qmc.scale`), `scikit-learn` (`gaussian_process.GaussianProcessRegressor`, `gaussian_process.kernels.RBF`), `numpy` (`random.default_rng`, `argsort`, `linspace`, `meshgrid`, `column_stack`, `vander`, `linalg.lstsq`, `apply_along_axis`), data-branch `xarray`/`dask` shapes for the grid lane, runtime (`RuntimeRail`, `ContentIdentity`, `Receipt`, `ReceiptContributor`).
- Growth: a new param axis is one `ParamAxis` coordinate; a new sampler, analyzer, or surrogate is one `StudyMethod` case; a new measurement is one `MeasurementMode` row; zero new surface.
- Boundary: no job framework, farm scheduler, substrate selection, or C# receipt minting; classical DOE, sensitivity, and surrogate only — a neural surrogate or an acquisition-driven active-learning loop is out of charter. A standalone benchmark owner, a parallel experiment tracker, a hand-rolled Morris or Saltelli implementation, and a `NotImplementedError` sensitivity stub are the deleted forms. SALib is pure-Python over numpy/scipy, so its core runs where numpy resolves; the scipy `qmc` and scikit-learn surrogate rows carry the `python_version<'3.15'` marker, and the numpy LHS, factorial, and polynomial floors run on cp315.

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from collections.abc import Callable
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability.receipts import Receipt
from rasm.runtime.faults import RuntimeRail, boundary

# --- [TYPES] ---------------------------------------------------------------------------
type Objective = Callable[[np.ndarray], float]


class MeasurementMode(StrEnum):
    RESULT = "result"
    WALLCLOCK = "wallclock"
    SPEEDUP = "speedup"


# --- [MODELS] --------------------------------------------------------------------------
class ParamAxis(Struct, frozen=True):
    name: str
    low: float
    high: float

    def rescale(self, unit_col: np.ndarray) -> np.ndarray:
        return self.low + (self.high - self.low) * unit_col


@tagged_union(frozen=True)
class StudyMethod:
    tag: Literal[
        "lhs", "factorial", "sobol", "halton", "morris_screen", "sobol_indices", "fast", "polynomial", "gaussian_process"
    ] = tag()
    lhs: int = case()
    factorial: tuple[int, ...] = case()
    sobol: int = case()
    halton: int = case()
    morris_screen: tuple[int, int] = case()
    sobol_indices: int = case()
    fast: int = case()
    polynomial: int = case()
    gaussian_process: float = case()

    @staticmethod
    def Lhs(n: int) -> StudyMethod:
        return StudyMethod(lhs=n)

    @staticmethod
    def Factorial(levels: tuple[int, ...]) -> StudyMethod:
        return StudyMethod(factorial=levels)

    @staticmethod
    def Sobol(log2_n: int) -> StudyMethod:
        return StudyMethod(sobol=log2_n)

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
    def Polynomial(degree: int) -> StudyMethod:
        return StudyMethod(polynomial=degree)

    @staticmethod
    def GaussianProcess(length_scale: float) -> StudyMethod:
        return StudyMethod(gaussian_process=length_scale)


class StudyReceipt(Struct, frozen=True):
    method: str
    mode: MeasurementMode
    cells_completed: int
    cells_total: int
    indices: dict[str, float]
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.Emitted(
            "compute.study",
            self.method,
            {
                "mode": self.mode.value,
                "cells": f"{self.cells_completed}/{self.cells_total}",
                **{f"S[{k}]": repr(v) for k, v in self.indices.items()},
            },
        )


class Study(Struct, frozen=True):
    axes: tuple[ParamAxis, ...]
    method: StudyMethod
    mode: MeasurementMode

    def run(self, objective: Objective, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        return boundary(f"study.{self.method.tag}", lambda: _execute(self, objective, seed))

    def problem(self) -> dict[str, object]:
        return {
            "num_vars": len(self.axes),
            "names": [ax.name for ax in self.axes],
            "bounds": [[ax.low, ax.high] for ax in self.axes],
        }


# --- [OPERATIONS] ----------------------------------------------------------------------
def _execute(study: Study, objective: Objective, seed: int) -> StudyReceipt:
    design = _design(study, seed)
    responses = np.apply_along_axis(objective, 1, design)
    key = ContentIdentity.key("study", design.tobytes())
    indices = _indices(study, design, responses)
    return StudyReceipt(study.method.tag, study.mode, int(responses.size), int(design.shape[0]), indices, key)


def _design(study: Study, seed: int) -> np.ndarray:
    rng = np.random.default_rng(seed)
    dim = len(study.axes)
    match study.method:
        case StudyMethod(tag="lhs", lhs=n):
            unit = _latin_hypercube(rng, n, dim)
        case StudyMethod(tag="factorial", factorial=levels):
            unit = _full_factorial(levels)
        case StudyMethod(tag="polynomial"):
            unit = _latin_hypercube(rng, max(8, dim * 4), dim)
        case StudyMethod(tag="sobol" | "halton"):
            return _qmc_design(study, seed)
        case StudyMethod(tag="morris_screen" | "sobol_indices" | "fast" | "gaussian_process"):
            return _salib_or_sklearn_design(study, seed)
        case unreachable:
            assert_never(unreachable)
    return np.column_stack([ax.rescale(unit[:, j]) for j, ax in enumerate(study.axes)])


def _latin_hypercube(rng: np.random.Generator, n: int, dim: int) -> np.ndarray:
    cut = np.linspace(0.0, 1.0, n + 1)
    jitter = rng.random((n, dim))
    centers = cut[:-1, None] + jitter * (cut[1, None] - cut[0])
    return np.take_along_axis(centers, np.argsort(rng.random((n, dim)), axis=0), axis=0)


def _full_factorial(levels: tuple[int, ...]) -> np.ndarray:
    grids = np.meshgrid(*[np.linspace(0.0, 1.0, k) for k in levels], indexing="ij")
    return np.column_stack([g.reshape(-1) for g in grids])


def _qmc_design(study: Study, seed: int) -> np.ndarray:
    from scipy.stats import qmc

    bounds = study.problem()
    lo = [b[0] for b in bounds["bounds"]]
    hi = [b[1] for b in bounds["bounds"]]
    match study.method:
        case StudyMethod(tag="sobol", sobol=m):
            sample = qmc.Sobol(d=len(study.axes), scramble=True, seed=seed).random_base2(m)
        case StudyMethod(tag="halton", halton=n):
            sample = qmc.Halton(d=len(study.axes), scramble=True, seed=seed).random(n)
        case _:
            raise AssertionError(study.method.tag)
    return qmc.scale(sample, lo, hi)


def _salib_or_sklearn_design(study: Study, seed: int) -> np.ndarray:
    problem = study.problem()
    match study.method:
        case StudyMethod(tag="morris_screen", morris_screen=(traj, levels)):
            from SALib.sample import morris

            return morris.sample(problem, traj, num_levels=levels, seed=seed)
        case StudyMethod(tag="sobol_indices", sobol_indices=n):
            from SALib.sample import sobol

            return sobol.sample(problem, n, seed=seed)
        case StudyMethod(tag="fast", fast=n):
            from SALib.sample import fast_sampler

            return fast_sampler.sample(problem, n, seed=seed)
        case StudyMethod(tag="gaussian_process"):
            rng = np.random.default_rng(seed)
            unit = _latin_hypercube(rng, max(16, len(study.axes) * 8), len(study.axes))
            return np.column_stack([ax.rescale(unit[:, j]) for j, ax in enumerate(study.axes)])
        case _:
            raise AssertionError(study.method.tag)


def _indices(study: Study, design: np.ndarray, responses: np.ndarray) -> dict[str, float]:
    problem = study.problem()
    names = [ax.name for ax in study.axes]
    match study.method:
        case StudyMethod(tag="morris_screen"):
            from SALib.analyze import morris

            result = morris.analyze(problem, design, responses)
            return {n: float(v) for n, v in zip(names, result["mu_star"], strict=True)}
        case StudyMethod(tag="sobol_indices"):
            from SALib.analyze import sobol

            result = sobol.analyze(problem, responses)
            return {n: float(v) for n, v in zip(names, result["ST"], strict=True)}
        case StudyMethod(tag="fast"):
            from SALib.analyze import fast

            result = fast.analyze(problem, responses)
            return {n: float(v) for n, v in zip(names, result["ST"], strict=True)}
        case StudyMethod(tag="polynomial", polynomial=degree):
            return _polynomial_r2(study, design, responses, degree)
        case StudyMethod(tag="gaussian_process", gaussian_process=length_scale):
            return _gaussian_process_score(design, responses, length_scale)
        case _:
            return {}


def _polynomial_r2(study: Study, design: np.ndarray, responses: np.ndarray, degree: int) -> dict[str, float]:
    out: dict[str, float] = {}
    for j, ax in enumerate(study.axes):
        vander = np.vander(design[:, j], degree + 1)
        coef, *_ = np.linalg.lstsq(vander, responses, rcond=None)
        pred = vander @ coef
        ss_res = float(np.sum((responses - pred) ** 2))
        ss_tot = float(np.sum((responses - responses.mean()) ** 2)) or 1.0
        out[ax.name] = 1.0 - ss_res / ss_tot
    return out


def _gaussian_process_score(design: np.ndarray, responses: np.ndarray, length_scale: float) -> dict[str, float]:
    from sklearn.gaussian_process import GaussianProcessRegressor
    from sklearn.gaussian_process.kernels import RBF

    model = GaussianProcessRegressor(kernel=RBF(length_scale=length_scale)).fit(design, responses)
    return {"r2": float(model.score(design, responses))}
```

The numpy stratified-LHS row stratifies each axis into `n` equal-probability bins, places one jittered sample per bin, and independently permutes the per-axis column order; the factorial row builds the full grid; the polynomial row fits a univariate Vandermonde least-squares model per axis and reports `R^2`. SALib owns the sensitivity domain: the Morris row reads `mu_star` from `SALib.analyze.morris`, the Sobol row reads first-and-total-order indices from `SALib.analyze.sobol` over a Saltelli design, and the FAST row reads `ST` from `SALib.analyze.fast`. The Gaussian-process surrogate fits a `GaussianProcessRegressor` with an `RBF` kernel and reports the fit score. The charter boundary holds at the surrogate seam: classical GP and polynomial regression are in-scope; a neural surrogate or an acquisition-driven active-learning loop is not.

## [3]-[RESEARCH]

- [SALIB_SENSITIVITY]: `SALib` is NOT yet in the root manifest; it is pure-Python over numpy/scipy. The `sample.morris`/`sample.sobol`/`sample.fast_sampler`/`analyze.morris`/`analyze.sobol`/`analyze.fast` spellings and the `problem`/`ProblemSpec` dict shape are admitted to the `scientific` group and verified against the branch `.api` catalogue before any fence names them. SALib owns Morris elementary effects and Saltelli Sobol indices, so the owner composes its sampler-and-analyzer pair rather than reimplementing them.
- [SCIPY_QMC]: the `scipy.stats.qmc.Sobol`/`Halton`/`scale` spellings carry the `python_version<'3.15'` marker; the bodies verify against the branch `.api` catalogue once the scipy wheel resolves.
- [SKLEARN_SURROGATE]: the `sklearn.gaussian_process.GaussianProcessRegressor`/`kernels.RBF` spellings carry the `python_version<'3.15'` marker; the surrogate row verifies against the branch `.api` catalogue once the scikit-learn wheel resolves. The numpy LHS, factorial, and polynomial floors run unconditionally on cp315.
