# [PY_COMPUTE_UNITS_STUDY]

Quantity claims, study orchestration, and model-asset validation. `UncertainQuantity` is ONE quantity-evidence owner threading correlated `uncertainties.UFloat` magnitudes THROUGH the pint unit algebra via the native `pint.Measurement` bridge, propagation discriminated by a `PropagationMode` axis; `Study` is ONE study-lake owner discriminating by a `StudyMethod` axis (DOE sampling LHS/factorial/Sobol/Halton, sensitivity Morris/Sobol-indices, surrogate polynomial/GP) over the numpy/scipy/sklearn spine, with `BenchmarkData` collapsed in as a measurement-mode discriminant and `RunHistory` owning experiment-run persistence/resume/comparison on the same study spine; `ModelAsset` owns file identity/checksum/io-names/preprocessing/model-card/validation over onnx + onnxruntime + skl2onnx (the sklearn-to-ONNX export + runtime-check path, all three deploy-gated) backing the graduation seam.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                           |
| :-----: | :-------- | :--------------------------------------------------------------- |
|   [1]   | QUANTITY  | correlated uncertainty threaded through the pint unit algebra    |
|   [2]   | STUDY     | DOE / sensitivity / surrogate study lake, benchmark, run history |
|   [3]   | MODEL     | model-asset file identity, validation, sklearn-to-ONNX export    |

## [2]-[QUANTITY]

- Owner: `UncertainQuantity` — ONE quantity-evidence owner threading correlated uncertainty THROUGH the pint unit algebra: a `pint.Measurement` (value+error `Quantity` over the shared `UnitRegistry`) carries the unit dimension and the linear `uncertainties.UFloat` magnitude in one object, so unit conversion and first-order error propagation compose on the same value — no parallel uncertain-type beside the unit-bearing one.
- Cases: `PropagationMode` rows `SCALAR` (single `ufloat` magnitude) · `CORRELATED` (a `correlated_values` vector sharing one covariance) · `EXPRESSION` (a callable propagated through `uncertainties.wrap`/`umath`) — one discriminant on the propagation, not a parallel uncertain owner. A new quantity family is one registry unit string; a new propagation mode is one `PropagationMode` row.
- Entry: `UncertainQuantity.of` admits a magnitude+std-dev+unit (or a `correlated_values` vector with its covariance) into a `pint.Measurement` and returns a `RuntimeRail[UncertainQuantity]`; `convert` runs the pint conversion preserving the relative error; `propagate` folds a derived expression over the `uncertainties` graph, the std-dev flowing automatically through the correlation structure; `claim` projects the unit dimension, nominal, std-dev, and correlation provenance into a `QuantityReceipt` for the study spine.
- Packages: `pint` (`UnitRegistry`/`Quantity`/`Measurement`/`to`/`to_base_units`/`dimensionality`/`DimensionalityError`), `uncertainties` (`ufloat`/`correlated_values`/`covariance_matrix`/`correlation_matrix`/`wrap`/`umath`, `UFloat.nominal_value`/`.std_dev`/`.error_components`), runtime (`RuntimeRail`/`boundary`/`Receipt`).
- Boundary: no C# quantity/unit owner minting and no production error policy; offline propagation evidence only. A hand-rolled unit conversion table, a parallel uncertain-magnitude type beside the unit-bearing one, and uncorrelated re-derivation of a propagated variance are the deleted forms. `SPIKE` on the marker floor (pint/uncertainties reflected; live on the active env).

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from collections.abc import Callable, Sequence
from enum import StrEnum
from typing import TYPE_CHECKING, Literal

import numpy as np
import pint
import uncertainties
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct
from uncertainties import UFloat, correlated_values, correlation_matrix, covariance_matrix, ufloat, umath, wrap

from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import BoundaryFault, RuntimeRail, boundary

if TYPE_CHECKING:
    from rasm.runtime.observability import ReceiptContributor

# --- [CONSTANTS] -----------------------------------------------------------------------
_UREG: pint.UnitRegistry = pint.UnitRegistry()  # one shared registry owns the unit vocabulary


class PropagationMode(StrEnum):
    SCALAR = "scalar"
    CORRELATED = "correlated"
    EXPRESSION = "expression"


# --- [MODELS] --------------------------------------------------------------------------
class QuantityReceipt(Struct, frozen=True):
    unit_expr: str
    dimensionality: str
    nominal: float
    std_dev: float
    rel_error: float
    mode: PropagationMode
    correlated_with: tuple[str, ...]

    def contribute(self) -> Receipt:
        return Receipt.Emitted(
            "compute.quantity",
            self.unit_expr,
            {
                "dim": self.dimensionality,
                "nominal": repr(self.nominal),
                "std_dev": repr(self.std_dev),
                "rel_error": repr(self.rel_error),
                "mode": self.mode.value,
                "correlated_with": ",".join(self.correlated_with),
            },
        )


@tagged_union(frozen=True)
class Magnitude:
    """The uncertainty-bearing magnitude under one pint unit: a scalar UFloat or a correlated vector cell."""

    tag: Literal["scalar", "correlated"] = tag()
    scalar: UFloat = case()
    correlated: tuple[UFloat, tuple[str, ...]] = case()  # (cell, peer-tags sharing the covariance)

    @staticmethod
    def Scalar(value: UFloat) -> Magnitude:
        return Magnitude(scalar=value)

    @staticmethod
    def Correlated(value: UFloat, peers: tuple[str, ...]) -> Magnitude:
        return Magnitude(correlated=(value, peers))

    @property
    def cell(self) -> UFloat:
        match self:
            case Magnitude(tag="scalar", scalar=u):
                return u
            case Magnitude(tag="correlated", correlated=(u, _)):
                return u
        raise AssertionError(self.tag)


class UncertainQuantity(Struct, frozen=True):
    measurement: pint.Measurement  # value+error Quantity over _UREG — unit and UFloat in one object
    magnitude: Magnitude
    mode: PropagationMode

    # --- [BOUNDARIES] ------------------------------------------------------------------
    @classmethod
    def of(cls, nominal: float, std_dev: float, unit: str, /) -> RuntimeRail[UncertainQuantity]:
        """Admit a scalar measurement: nominal +/- std_dev [unit] over the shared registry."""

        def _build() -> UncertainQuantity:
            meas = _UREG.Measurement(nominal, std_dev, unit)  # raises UndefinedUnitError at the boundary
            return cls(meas, Magnitude.Scalar(ufloat(nominal, std_dev)), PropagationMode.SCALAR)

        return boundary("quantity.of", _build)

    @classmethod
    def correlated(
        cls,
        nominals: Sequence[float],
        covariance: Sequence[Sequence[float]],
        unit: str,
        tags: tuple[str, ...],
        /,
    ) -> RuntimeRail[tuple[UncertainQuantity, ...]]:
        """Admit a correlated vector: one covariance shared across cells, each cell a unit-bearing quantity."""

        def _build() -> tuple[UncertainQuantity, ...]:
            cells: list[UFloat] = correlated_values(list(nominals), [list(r) for r in covariance], tags=list(tags))
            return tuple(
                cls(
                    _UREG.Measurement(cell.nominal_value, cell.std_dev, unit),
                    Magnitude.Correlated(cell, tuple(t for t in tags if t != tag_self)),
                    PropagationMode.CORRELATED,
                )
                for cell, tag_self in zip(cells, tags, strict=True)
            )

        return boundary("quantity.correlated", _build)

    # --- [OPERATIONS] ------------------------------------------------------------------
    def convert(self, target_unit: str, /) -> RuntimeRail[UncertainQuantity]:
        """Pint conversion preserving the relative error; a dimension mismatch surfaces on the rail."""

        def _to() -> UncertainQuantity:
            converted = self.measurement.to(target_unit)
            factor = float(converted.value.magnitude) / self.magnitude.cell.nominal_value
            scaled = self.magnitude.cell * factor
            mag = (
                Magnitude.Scalar(scaled)
                if self.mode is PropagationMode.SCALAR
                else Magnitude.Correlated(scaled, self._peers())
            )
            return UncertainQuantity(converted, mag, self.mode)

        match self.measurement.units.dimensionality == _UREG.Unit(target_unit).dimensionality:
            case True:
                return boundary("quantity.convert", _to)
            case _:
                return Error(BoundaryFault.Boundary("dimensionality", f"{self._unit_expr()}->{target_unit}"))

    def propagate(self, expr: Callable[[float], float], unit: str, /) -> RuntimeRail[UncertainQuantity]:
        """Fold a derived expression through the uncertainties graph; std-dev flows by the chain rule."""

        def _apply() -> UncertainQuantity:
            wrapped = wrap(expr)
            out: UFloat = wrapped(self.magnitude.cell)
            mag = (
                Magnitude.Scalar(out)
                if self.mode is PropagationMode.SCALAR
                else Magnitude.Correlated(out, self._peers())
            )
            return UncertainQuantity(_UREG.Measurement(out.nominal_value, out.std_dev, unit), mag, self.mode)

        return boundary("quantity.propagate", _apply)

    def claim(self) -> QuantityReceipt:
        cell = self.magnitude.cell
        rel = float(abs(cell.std_dev / cell.nominal_value)) if cell.nominal_value else float("inf")
        return QuantityReceipt(
            unit_expr=self._unit_expr(),
            dimensionality=str(dict(self.measurement.units.dimensionality)),
            nominal=float(cell.nominal_value),
            std_dev=float(cell.std_dev),
            rel_error=rel,
            mode=self.mode,
            correlated_with=self._peers(),
        )

    # --- [OPERATIONS] (private) --------------------------------------------------------
    def _unit_expr(self) -> str:
        return f"{self.measurement.units:~}"

    def _peers(self) -> tuple[str, ...]:
        match self.magnitude:
            case Magnitude(tag="correlated", correlated=(_, peers)):
                return peers
            case _:
                return ()


# --- [OPERATIONS] ----------------------------------------------------------------------
def covariance_provenance(quantities: Sequence[UncertainQuantity]) -> RuntimeRail[np.ndarray]:
    """Recover the joint covariance over a correlated cohort — the propagated-variance provenance receipt."""

    def _cov() -> np.ndarray:
        return np.asarray(covariance_matrix([q.magnitude.cell for q in quantities]), dtype=np.float64)

    return boundary("quantity.covariance", _cov)


def correlation_provenance(quantities: Sequence[UncertainQuantity]) -> RuntimeRail[np.ndarray]:
    def _corr() -> np.ndarray:
        return np.asarray(correlation_matrix([q.magnitude.cell for q in quantities]), dtype=np.float64)

    return boundary("quantity.correlation", _corr)
```

`pint.Measurement` is the native value+error carrier (`ureg.Measurement(value, error, units)`, exposing `.value`/`.error` as `Quantity`); it is the canonical pint-uncertainties bridge, so the owner threads ONE `UFloat`-backed magnitude under one unit rather than minting a parallel uncertain shape. `correlated_values(nominals, covariance, tags=...)` builds a cohort whose cells share one covariance; `covariance_matrix`/`correlation_matrix` recover the joint structure after propagation, and `UFloat.error_components()` attributes variance back to source variables for the provenance receipt. The `EXPRESSION` mode routes any documented elementary function through `uncertainties.umath` (`sqrt`/`sin`/`cos`/`exp`/`log`) or an arbitrary callable through `uncertainties.wrap`, which differentiates numerically when no analytic derivative is registered. The classical-statistics boundary: linear (first-order) propagation only — a second-order or Monte-Carlo error model would route to the STUDY DOE sampler, never a generative surrogate.

## [3]-[STUDY]

- Owner: `Study` — ONE study-lake owner discriminating by a `StudyMethod` axis over the same param-axis/sample-grid/solver-route/measurement spine: design-of-experiments sampling, global sensitivity analysis, and surrogate fitting are CASES on one owner, not parallel surfaces. `RunHistory` rides the same spine for experiment-run persistence/resume/cross-run comparison; `BenchmarkData` collapses into the study receipt with a `MeasurementMode` discriminant.
- Cases — `StudyMethod` tagged union, matched total by `match`/`case`:
  - DOE sampling: `Lhs(n)` (numpy stratified Latin-hypercube, live) · `Factorial(levels)` (numpy full-factorial mesh, live) · `Sobol(n)` (scipy `qmc.Sobol` low-discrepancy, deploy-gated) · `Halton(n)` (scipy `qmc.Halton`, deploy-gated).
  - Sensitivity: `MorrisScreen(trajectories, levels)` (numpy elementary-effects screening, live) · `SobolIndices(n)` (scipy/Saltelli first+total-order variance decomposition, deploy-gated).
  - Surrogate: `Polynomial(degree)` (numpy least-squares Vandermonde fit, live) · `GaussianProcess(length_scale)` (scikit-learn `GaussianProcessRegressor`, deploy-gated).
- `MeasurementMode` rows `RESULT` · `WALLCLOCK` · `SPEEDUP` — one discriminant on the receipt, not a parallel benchmark owner. A new param axis is one `NamedAxis` coordinate; a new sampler/index/surrogate is one `StudyMethod` case; zero new surface.
- Entry: `Study.run` matches the method, walks the sample grid (numpy folds for the live rows; the gated rows route to scipy/sklearn), evaluates the objective per cell, and returns a `RuntimeRail[StudyReceipt]` carrying the method, mode, completed/total cells, and the design `ContentKey`; `RunHistory.resume` re-runs only the incomplete grid cells; `RunHistory.compare` joins two run receipts.
- Packages: `numpy` (`random.default_rng`/`argsort`/`linspace`/`meshgrid`/`column_stack`/`vander`/`linalg.lstsq`/`var`, live), `scipy.stats` (`qmc.Sobol`/`qmc.Halton`/Saltelli sequence, deploy-gated), `scikit-learn` (`gaussian_process.GaussianProcessRegressor`, deploy-gated), `dask`/`xarray` composed from the data-branch catalogues for the grid lane, runtime (`LanePolicy`/`RuntimeRail`/`ContentIdentity`/`Receipt`).
- Boundary: no job framework, farm scheduler, production substrate selection, or C# `ComputeReceipt` minting; a standalone benchmark owner, a parallel experiment tracker, and a generative/active-learning sampler are the deleted forms. Classical DOE/sensitivity/surrogate only — a neural surrogate or Bayesian-optimization acquisition loop is out of charter. `SPIKE` on the marker floor (numpy live; scipy/sklearn rows deploy-gated).

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from collections.abc import Callable, Sequence
from enum import StrEnum
from typing import Literal

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary

# --- [TYPES] ---------------------------------------------------------------------------
type Objective = Callable[[np.ndarray], float]  # one design row (param vector) -> scalar response


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
    """One axis: DOE samplers, sensitivity analyzers, and surrogate fitters as cases on one owner."""

    tag: Literal[
        "lhs", "factorial", "sobol", "halton", "morris_screen", "sobol_indices", "polynomial", "gaussian_process"
    ] = tag()
    lhs: int = case()  # sample count
    factorial: tuple[int, ...] = case()  # levels per axis
    sobol: int = case()  # log2 sample count (gated)
    halton: int = case()  # sample count (gated)
    morris_screen: tuple[int, int] = case()  # (trajectories, levels)
    sobol_indices: int = case()  # base sample count (gated)
    polynomial: int = case()  # fit degree
    gaussian_process: float = case()  # RBF length scale (gated)

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
    def MorrisScreen(trajectories: int, levels: int) -> StudyMethod:
        return StudyMethod(morris_screen=(trajectories, levels))

    @staticmethod
    def SobolIndices(n: int) -> StudyMethod:
        return StudyMethod(sobol_indices=n)

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
    indices: dict[str, float]  # sensitivity index per axis, empty for sampling/surrogate
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


# --- [OPERATIONS] ----------------------------------------------------------------------
def _design(study: Study, seed: int) -> np.ndarray:
    """Total dispatch on the method axis -> a (cells, dim) design matrix in real units."""
    rng = np.random.default_rng(seed)
    dim = len(study.axes)
    match study.method:
        case StudyMethod(tag="lhs", lhs=n):
            unit = _latin_hypercube(rng, n, dim)
        case StudyMethod(tag="factorial", factorial=levels):
            unit = _full_factorial(levels)
        case StudyMethod(tag="morris_screen", morris_screen=(traj, levels)):
            unit = _morris_trajectories(rng, traj, levels, dim)
        case StudyMethod(tag="polynomial", polynomial=_):
            unit = _latin_hypercube(rng, max(8, dim * 4), dim)
        case StudyMethod(tag="sobol" | "halton" | "sobol_indices" | "gaussian_process"):
            return _gated_design(study, seed)
        case _:
            raise AssertionError(study.method.tag)
    return np.column_stack([ax.rescale(unit[:, j]) for j, ax in enumerate(study.axes)])


def _latin_hypercube(rng: np.random.Generator, n: int, dim: int) -> np.ndarray:
    """Stratified LHS: one sample per stratum per axis, columns independently permuted."""
    cut = np.linspace(0.0, 1.0, n + 1)
    jitter = rng.random((n, dim))
    centers = cut[:-1, None] + jitter * (cut[1, None] - cut[0])
    return np.take_along_axis(centers, np.argsort(rng.random((n, dim)), axis=0), axis=0)


def _full_factorial(levels: tuple[int, ...]) -> np.ndarray:
    grids = np.meshgrid(*[np.linspace(0.0, 1.0, k) for k in levels], indexing="ij")
    return np.column_stack([g.reshape(-1) for g in grids])


def _morris_trajectories(rng: np.random.Generator, trajectories: int, levels: int, dim: int) -> np.ndarray:
    """Morris elementary-effects design: r one-at-a-time trajectories on a (levels)-grid."""
    step = 1.0 / (levels - 1)
    rows: list[np.ndarray] = []
    for _ in range(trajectories):
        base = rng.integers(0, levels, dim) * step
        order = rng.permutation(dim)
        point = base.astype(np.float64)
        rows.append(point.copy())
        for axis in order:
            point[axis] = min(point[axis] + step, 1.0) if point[axis] <= 0.5 else max(point[axis] - step, 0.0)
            rows.append(point.copy())
    return np.vstack(rows)


def _execute(study: Study, objective: Objective, seed: int) -> StudyReceipt:
    design = _design(study, seed)
    responses = np.apply_along_axis(objective, 1, design)
    key = ContentIdentity.key("study", design.tobytes())
    return _receipt(study, design, responses, 0, key)


def _receipt(
    study: Study, full: np.ndarray, responses: np.ndarray, offset: int, key: ContentKey
) -> StudyReceipt:
    """One receipt projection over the full design: index dispatch on the method axis, completed = offset + scored.

    Sensitivity/surrogate indices need the complete design, so a partial resume (responses.size < full rows)
    carries an empty index map; only a fully scored grid emits mu*/R^2.
    """
    complete = offset == 0 and responses.size == full.shape[0]
    match study.method:
        case StudyMethod(tag="morris_screen", morris_screen=(traj, levels)) if complete:
            indices = _morris_indices(study, full, responses, traj, levels)
        case StudyMethod(tag="polynomial", polynomial=degree) if complete:
            indices = _polynomial_r2(study, full, responses, degree)
        case _:
            indices = {}
    return StudyReceipt(study.method.tag, study.mode, offset + int(responses.size), int(full.shape[0]), indices, key)


def _resume(runs: tuple[StudyReceipt, ...], study: Study, objective: Objective, seed: int) -> StudyReceipt:
    """Partial-cell resume keyed by the design ContentKey: a complete prior run short-circuits, an incomplete
    prior run evaluates only its remaining cells, and a fresh design runs the whole grid once."""
    design = _design(study, seed)
    key = ContentIdentity.key("study", design.tobytes())
    prior = next((r for r in runs if r.content_key == key), None)
    match prior:
        case StudyReceipt(cells_completed=done, cells_total=total) if done >= total:
            return prior
        case StudyReceipt(cells_completed=done):
            remaining = np.apply_along_axis(objective, 1, design[done:])
            return _receipt(study, design, remaining, done, key)
        case _:
            responses = np.apply_along_axis(objective, 1, design)
            return _receipt(study, design, responses, 0, key)


def _morris_indices(
    study: Study, design: np.ndarray, responses: np.ndarray, trajectories: int, levels: int
) -> dict[str, float]:
    """mu* per axis: mean absolute elementary effect — the live screening sensitivity index."""
    step = 1.0 / (levels - 1)
    per_traj = len(study.axes) + 1
    effects: dict[int, list[float]] = {j: [] for j in range(len(study.axes))}
    for t in range(trajectories):
        block = slice(t * per_traj, (t + 1) * per_traj)
        pts, ys = design[block], responses[block]
        for k in range(len(study.axes)):
            moved = int(np.argmax(np.abs(pts[k + 1] - pts[k]) > 0))
            span = (study.axes[moved].high - study.axes[moved].low) * step
            effects[moved].append(abs((ys[k + 1] - ys[k]) / span) if span else 0.0)
    return {study.axes[j].name: float(np.mean(v)) if v else 0.0 for j, v in effects.items()}


def _polynomial_r2(study: Study, design: np.ndarray, responses: np.ndarray, degree: int) -> dict[str, float]:
    """Per-axis univariate Vandermonde least-squares fit; index = coefficient-of-determination R^2.

    Keyed by study.axes[j].name so the receipt's index keys stay axis-stable across methods and a cross-run
    compare join on axis name lands on the matching rows (the Morris row keys the same axis names).
    """
    out: dict[str, float] = {}
    for j, ax in enumerate(study.axes):
        vander = np.vander(design[:, j], degree + 1)
        coef, *_ = np.linalg.lstsq(vander, responses, rcond=None)
        pred = vander @ coef
        ss_res = float(np.sum((responses - pred) ** 2))
        ss_tot = float(np.sum((responses - responses.mean()) ** 2)) or 1.0
        out[ax.name] = 1.0 - ss_res / ss_tot
    return out


# --- [BOUNDARIES] (deploy-asset-gate: scipy/scikit-learn carry python_version<'3.15', no cp315 wheel) -----
def _gated_design(study: Study, seed: int) -> np.ndarray:
    """CATALOGUE_PENDING — documented spellings, verified-by-stability on the marker floor (TASKLOG PY_API_003).

    Sobol/Halton -> scipy.stats.qmc.Sobol(d, scramble=True, seed).random_base2(m) / .random(n), scaled by
    qmc.scale(sample, l_bounds, u_bounds). SobolIndices -> Saltelli A/B/AB matrices over qmc samples, first-order
    S_i = var(E[Y|X_i]) / var(Y) and total-order S_Ti via the (A,AB) and (B,AB) variance estimators. GaussianProcess
    -> sklearn.gaussian_process.GaussianProcessRegressor(kernel=RBF(length_scale)).fit(X, y).predict(X, return_std=True).
    """
    raise NotImplementedError("deploy-asset-gate: scipy/scikit-learn install on the marker floor before re-reflect")


# --- [REPOSITORIES] --------------------------------------------------------------------
class RunHistory(Struct, frozen=True):
    runs: tuple[StudyReceipt, ...]

    def resume(self, study: Study, objective: Objective, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        """Re-run only the incomplete grid cells: a prior run on the SAME design ContentKey short-circuits when
        complete; otherwise evaluate the remaining (total - completed) cells and emit the merged receipt."""
        return boundary("study.resume", lambda: _resume(self.runs, study, objective, seed))

    def compare(self, left: ContentKey, right: ContentKey, /) -> dict[str, tuple[int, int]]:
        by_key = {r.content_key: r for r in self.runs}
        a, b = by_key[left], by_key[right]
        return {"cells": (a.cells_completed, b.cells_completed), "total": (a.cells_total, b.cells_total)}
```

The LHS row stratifies each axis into `n` equal-probability bins, places one jittered sample per bin, then independently permutes the per-axis column order via `argsort` of a uniform draw — the textbook stratified design with no scipy dependency. The Morris row builds `r` one-at-a-time trajectories on a `levels`-grid and folds the elementary effects into `mu*` (mean absolute effect), the standard screening index. The polynomial-surrogate row fits a univariate Vandermonde least-squares model per axis and reports `R^2` as the fit-quality index. The Sobol/Halton low-discrepancy samplers, the Saltelli variance-decomposition Sobol indices, and the Gaussian-process surrogate route to `scipy.stats.qmc`/`scikit-learn` and are authored against their documented API behind the deploy-asset-gate marker (same posture as the native-BLAS deploy gate) — the documented spellings are transcribed in `_gated_design`, never invented members. The charter boundary holds at the surrogate seam: classical GP/polynomial regression is in-scope; a neural surrogate or an acquisition-driven active-learning loop is not.

## [4]-[MODEL]

- Owner: `ModelAsset` — file identity/checksum/io-names/preprocessing/model-card/validation over onnx + onnxruntime + skl2onnx; `ModelAssetManifest` the io-names/preprocessing/model-card/validation value object directly backing the graduation seam, every field carried by the struct (no prose-only schema claim).
- Cases: `ValidationCheck` tagged union — `Structural` (`onnx.checker.check_model` graph well-formedness) · `IoBinding` (declared io-names match the `InferenceSession` graph) · `Smoke` (a zero-tensor inference returns finite output) — one discriminant folded total by `match`/`case` in `validate`, not a parallel check family. A new validation check is one `ValidationCheck` row; a new export source is one `ExportSource` row; zero new surface.
- Entry: `ModelAsset.validate` loads the ONNX graph, folds the `ValidationCheck` cases (structural check, io-name binding, smoke `onnxruntime.InferenceSession.run`) over the declared io-names, derives `checksum` via `ContentIdentity.key`, and returns a `RuntimeRail[ModelAssetManifest]` proving the asset loads and infers before the C# `Rasm.Compute` row accepts it; `ModelAsset.export` discriminates an `ExportSource` and runs the skl2onnx export (`convert_sklearn`/`to_onnx`) validated before graduation.
- Packages: `onnx` (`load`/`checker.check_model`/`ModelProto`/`.opset_import`), `onnxruntime` (`InferenceSession`/`get_inputs`/`get_outputs`/`run`), `skl2onnx` (`convert_sklearn`/`to_onnx`, `FloatTensorType`), runtime (`RuntimeRail`/`boundary`/`ContentIdentity`/`ResourceRef`). onnx/onnxruntime/skl2onnx are GATED (`python_version<'3.15'`, no cp315 wheel); the fence transcribes the documented spellings behind a deploy-asset-gate marker, verified-by-stability (same posture as `_gated_design` and the native-BLAS deploy gate).
- Boundary: no production inference-session runtime; production tensor-session authority stays in C#; an unvalidated graduation is the deleted form. The classical/generative boundary holds: validating and exporting a classical sklearn estimator graph is in-scope; authoring or training a neural/generative model is not. `SPIKE` on the marker floor (onnx/onnxruntime/skl2onnx deploy-gated).

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from pathlib import Path
from typing import TYPE_CHECKING, Any, Literal

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import RuntimeRail, boundary
from rasm.runtime.resources_lanes import ResourceRef

if TYPE_CHECKING:
    from rasm.runtime.observability import ReceiptContributor

# --- [TYPES] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ValidationCheck:
    """One axis: the validation steps folded total over an ONNX asset, cases on one owner."""

    tag: Literal["structural", "io_binding", "smoke"] = tag()
    structural: None = case()  # onnx.checker.check_model graph well-formedness
    io_binding: tuple[str, ...] = case()  # declared input names asserted against the session graph
    smoke: tuple[str, ...] = case()  # output names a zero-tensor inference must return finite

    @staticmethod
    def Structural() -> ValidationCheck:
        return ValidationCheck(structural=None)

    @staticmethod
    def IoBinding(inputs: tuple[str, ...]) -> ValidationCheck:
        return ValidationCheck(io_binding=inputs)

    @staticmethod
    def Smoke(outputs: tuple[str, ...]) -> ValidationCheck:
        return ValidationCheck(smoke=outputs)


@tagged_union(frozen=True)
class ExportSource:
    """One axis: the graduation export entry points, cases on one owner."""

    tag: Literal["sklearn"] = tag()
    sklearn: tuple[Any, tuple[int, ...]] = case()  # (fitted estimator, input feature shape)

    @staticmethod
    def Sklearn(estimator: Any, feature_shape: tuple[int, ...]) -> ExportSource:
        return ExportSource(sklearn=(estimator, feature_shape))


# --- [MODELS] --------------------------------------------------------------------------
class ModelAssetManifest(Struct, frozen=True):
    checksum: ContentKey
    input_names: tuple[str, ...]
    output_names: tuple[str, ...]
    opset: int
    preprocessing: tuple[str, ...]  # ordered preprocessing-op names lifted from the graph
    model_card: dict[str, str]  # producer/domain/ir-version provenance carried on the manifest
    validated: bool

    def contribute(self) -> Receipt:
        return Receipt.Emitted(
            "compute.model",
            self.checksum.value,
            {
                "inputs": ",".join(self.input_names),
                "outputs": ",".join(self.output_names),
                "opset": repr(self.opset),
                "preprocessing": ",".join(self.preprocessing),
                "validated": repr(self.validated),
                **self.model_card,
            },
        )


class ModelAsset(Struct, frozen=True):
    ref: ResourceRef

    # --- [BOUNDARIES] (deploy-asset-gate: onnx/onnxruntime/skl2onnx carry python_version<'3.15', no cp315 wheel) --
    def validate(self) -> RuntimeRail[ModelAssetManifest]:
        """CATALOGUE_PENDING — fold the ValidationCheck cases over the ONNX graph; deploy-gated (TASKLOG PY_API_003).

        Loads onnx.load(path) -> ModelProto, runs onnx.checker.check_model(model) for the structural case, opens an
        onnxruntime.InferenceSession(path) and asserts session.get_inputs()/get_outputs() names against the declared
        io-names for the io_binding case, then session.run(out_names, {name: zeros}) for the smoke case, derives the
        checksum via ContentIdentity.key, and returns the populated manifest. onnx/onnxruntime are deploy-gated.
        """
        return boundary("model.validate", lambda: self._load_and_run(self.ref.path))

    def export(self, source: ExportSource, /) -> RuntimeRail[ModelAssetManifest]:
        """CATALOGUE_PENDING — convert a fitted classical estimator to ONNX, then validate; deploy-gated (PY_API_003).

        skl2onnx.convert_sklearn(estimator, initial_types=[("input", FloatTensorType([None, *shape]))]) (or to_onnx)
        serializes the fitted sklearn pipeline to a ModelProto, persisted to self.ref.path, then re-validated through
        validate() so the graduation seam only accepts an asset proven to load and infer. skl2onnx is deploy-gated.
        """
        match source:
            case ExportSource(tag="sklearn", sklearn=(estimator, feature_shape)):
                return boundary("model.export.sklearn", lambda: self._export_sklearn(estimator, feature_shape))
        raise AssertionError(source.tag)

    # --- [OPERATIONS] (private, deploy-gated bodies) -----------------------------------
    def _load_and_run(self, path: Path) -> ModelAssetManifest:
        raise NotImplementedError(
            "deploy-asset-gate: install onnx/onnxruntime on the marker floor before re-reflect — "
            "onnx.load(path)/checker.check_model + InferenceSession(path).run over the io-names; PY_API_003"
        )

    def _export_sklearn(self, estimator: Any, feature_shape: tuple[int, ...]) -> ModelAssetManifest:
        raise NotImplementedError(
            "deploy-asset-gate: install skl2onnx on the marker floor before re-reflect — "
            "convert_sklearn(estimator, initial_types=[('input', FloatTensorType([None, *shape]))]); PY_API_003"
        )
```

## [5]-[RESEARCH]

- [QUANTITY_REFLECTED]: `pint` 0.25.3 and `uncertainties` 3.2.3 are installed and reflected on the active 3.15 env; the `pint.Measurement(value, error, units)` value+error bridge, `Quantity.to`/`.dimensionality`/`.magnitude`/`.units`, `DimensionalityError`, and the `ufloat`/`correlated_values(nominals, covariance, tags=...)`/`covariance_matrix`/`correlation_matrix`/`wrap`/`umath` spellings plus `UFloat.nominal_value`/`.std_dev`/`.error_components` are used verbatim. The QUANTITY owner is live; the `.api/api-pint.md`/`.api/api-uncertainties.md` `installed: ABSENT` lines are stale against the active env and re-catalogue on the next API pass.
- [STUDY_DOE]: the numpy LHS/factorial/Morris/polynomial bodies are live (`numpy` 1.26.4 installed). The `Sobol`/`Halton`/`SobolIndices`/`GaussianProcess` rows route to `scipy.stats.qmc`/`scikit-learn`, both GATED (`python_version<'3.15'`, no cp315 wheel); they are authored against the documented `qmc.Sobol`/`qmc.scale`/Saltelli and `GaussianProcessRegressor`+`RBF` API behind the `_gated_design` deploy-asset-gate marker and verify on the marker floor (suite TASKLOG `PY_API_003`).
- [ONNXRUNTIME_SESSION]: `onnx`, `onnxruntime`, and `skl2onnx` are GATED (`ModuleNotFoundError` on the active 3.15 env, `python_version<'3.15'` marker, no cp315 wheel); the MODEL fence transcribes their documented spellings (`onnx.load`/`checker.check_model`/`ModelProto.opset_import`, `onnxruntime.InferenceSession`/`get_inputs`/`get_outputs`/`run`, `skl2onnx.convert_sklearn`/`to_onnx`/`FloatTensorType`) behind the `validate`/`export` deploy-asset-gate `CATALOGUE_PENDING` markers with `NotImplementedError("deploy-asset-gate: ...")` bodies, verified-by-stability on the marker floor (same posture as `_gated_design`). They re-catalogue against `.api/api-onnxruntime.md`, `.api/api-onnx.md`, `.api/api-skl2onnx.md` once the marker-floor environment installs them (suite TASKLOG `PY_API_003`).
