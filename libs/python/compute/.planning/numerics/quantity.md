# [PY_COMPUTE_QUANTITY]

The one unit-bearing uncertain-quantity owner. `UncertainQuantity` threads a correlated `uncertainties.UFloat` magnitude through the pint unit algebra via the native `pint.Measurement` bridge, so unit conversion and first-order error propagation compose on one value. `Magnitude` is the single discriminant — a scalar `UFloat` or a correlated cell carrying its peer tags — and owns the `through` fold that re-mints the cell under any linear map while preserving the tag and the peer dependence, so every transform (convert, propagate) is one fold arm rather than a per-method rebuild. `Propagation` collapses the propagation algebra into one policy union: a named `uncertainties.umath` elementary function, an arbitrary callable lifted through `uncertainties.wrap` (numerically differentiated), or a callable with registered analytic partials through `wrap(f, derivatives_args=...)`. The owner recovers the joint covariance and correlation provenance and attributes the standard-deviation contribution of each source variable through `UFloat.error_components`. The unit vocabulary is one frozen application registry shared across every module through `pint.get_application_registry`, never a per-call instance whose `Quantity` types fail to interoperate. pint and uncertainties are both cp315-clean, so this owner is reflected; linear first-order propagation is the boundary, and a large-uncertainty regime routes to the study Monte-Carlo sampler.

## [01]-[INDEX]

- [01]-[QUANTITY]: correlated first-order uncertainty threaded through the pint unit algebra on one `UncertainQuantity` owner, every transform a `Magnitude.through` fold and every propagation a `Propagation` policy row.

## [02]-[QUANTITY]

- Owner: `UncertainQuantity` — the ONE quantity-evidence owner threading correlated uncertainty through the pint unit algebra; a `pint.Measurement` (value-and-error `Quantity` over the shared `UnitRegistry`) carries the unit dimension and the `uncertainties.UFloat` magnitude in one object, so unit conversion and first-order propagation compose on the same value. `Magnitude` is the single scalar/correlated discriminant and the only place propagation mode is recorded — the structural tag IS the mode, so no parallel `PropagationMode` field shadows it; `Propagation` is the closed propagation-algebra policy the owner folds the cell through. No parallel uncertain type stands beside the unit-bearing one.
- Fold collapse: `Magnitude.through(linear)` and `Magnitude.reseat(cell)` are the two rebuild folds over the one tag algebra — `through` applies a `UFloat -> UFloat` map and keeps the tag, `reseat` swaps in an externally-produced cell and keeps the tag (scalar stays scalar, correlated keeps its `peers`), so `propagate` is one `through` call and `convert` is one `reseat` of the pint-produced cell rather than a rebuilt `Magnitude.Scalar`/`Magnitude.Correlated` branch with a re-extracted peer tuple. The `cell` and `peers` projections read the tag's two facts without a second `match` per call site, each a total match closed by `assert_never`.
- Registry: `_UREG` is the one frozen module handle bound from `pint.get_application_registry()` (and seeded once with `pint.set_application_registry(pint.UnitRegistry())` at import so the process owns a definite vocabulary before any module pulls the handle). It is read-only at the owner — no `define`, no per-call `UnitRegistry()`, no mutation — so every `Quantity`/`Measurement` this owner mints shares one unit vocabulary and stays arithmetically compatible across `array`, `interval`, study results, and the graduation rail. A second registry beside this handle is the deleted form.
- Entry: `UncertainQuantity.of` admits a nominal, standard deviation, and unit into a `pint.Measurement`; `correlated` admits a cohort discriminated by its `Covariance` value — a full covariance matrix through `uncertainties.correlated_values` or a standard-deviation-and-correlation pair through `correlated_values_norm` — both on one entry, the construction matrix never a second method; `convert` runs the pint conversion inside the boundary thunk so pint's own `DimensionalityError` converts once through `boundary` (no hand-minted dimensionality fault, no pre-check branch) and reseats the `Magnitude` onto the `UFloat` pint's `Measurement.to` already produced, so the unit algebra — including an affine offset-unit conversion (temperature) a pure multiplicative ratio would corrupt — and the correlation graph stay owned by pint/`uncertainties`, a zero-nominal value with finite uncertainty converts without a value-ratio division, and a correlated cell keeps its peer dependence through `Magnitude.reseat`; `propagate` folds a `Propagation` policy through the uncertainties graph with the standard deviation flowing by the chain rule; `claim` projects the unit dimension, nominal, standard deviation, relative error, the per-source standard-deviation contribution from `UFloat.error_components`, and correlation provenance into a `QuantityReceipt`, the relative error `0.0` for an exact zero (zero nominal and zero std) and `inf` only when a zero nominal carries genuine spread.
- Provenance: `cohort` is the one cohort-provenance entry — it discriminates a `CohortView` request (`covariance`, `correlation`, or the `unumpy.uarray`/`nominal_values`/`std_devs` packed array of nominal-and-error) and folds one rail over a correlated cohort through `uncertainties.covariance_matrix`/`correlation_matrix` and `uncertainties.unumpy`, never three sibling provenance functions; `claim`'s `components` field carries the `UFloat.error_components` per-variable standard-deviation contribution back to each source variable (the contributions combine in quadrature, never linearly).
- Packages: `pint` (`get_application_registry`, `set_application_registry`, `UnitRegistry`, `Quantity`, `Measurement`, `Quantity.to`, `Quantity.to_base_units`, `Quantity.magnitude`, `Quantity.units`, `Quantity.dimensionality`, `DimensionalityError`), `uncertainties` (`ufloat`, `correlated_values`, `correlated_values_norm`, `covariance_matrix`, `correlation_matrix`, `wrap`, `umath`, `UFloat.nominal_value`, `UFloat.std_dev`, `UFloat.error_components`, `unumpy.uarray`, `unumpy.nominal_values`, `unumpy.std_devs`), `numpy` (`asarray`, `ndarray`, `stack`), runtime (`RuntimeRail`, `boundary`, `Receipt`).
- Growth: a new quantity family is one registry unit string; a new propagation algebra is one `Propagation` case; a new cohort-provenance view is one `CohortView` row; zero new surface.
- Boundary: no C# quantity or unit owner minting and no production error policy; offline propagation evidence only. Linear first-order propagation is the boundary — a second-order or Monte-Carlo error model routes to the `experiments/study.md#STUDY` sampler, never a parallel surrogate. A hand-rolled unit-conversion table, a multiplicative unit-ratio scaling of the cell beside pint's own `Measurement.to` (which corrupts an affine offset-unit conversion), a parallel uncertain-magnitude type beside the unit-bearing one, a `PropagationMode` field shadowing the `Magnitude` tag, a free-form propagation-function string beside the bounded `Umath` vocabulary, sibling per-view provenance functions, and an uncorrelated re-derivation of a propagated variance are the deleted forms. pint and uncertainties are both cp315-clean, so the owner is reflected.

```python signature
from __future__ import annotations

from collections.abc import Callable, Sequence
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
import pint
from expression import case, tag, tagged_union
from msgspec import Struct
from uncertainties import (
    UFloat,
    correlated_values,
    correlated_values_norm,
    correlation_matrix,
    covariance_matrix,
    ufloat,
    umath,
    unumpy,
    wrap,
)

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

pint.set_application_registry(pint.UnitRegistry())
_UREG: pint.UnitRegistry = pint.get_application_registry()


# --- [TYPES] -------------------------------------------------------------------------------
@tagged_union(frozen=True)
class Magnitude:
    tag: Literal["scalar", "correlated"] = tag()
    scalar: UFloat = case()
    correlated: tuple[UFloat, tuple[str, ...]] = case()

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
            case unreachable:
                assert_never(unreachable)

    @property
    def peers(self) -> tuple[str, ...]:
        match self:
            case Magnitude(tag="scalar"):
                return ()
            case Magnitude(tag="correlated", correlated=(_, p)):
                return p
            case unreachable:
                assert_never(unreachable)

    def through(self, linear: Callable[[UFloat], UFloat], /) -> Magnitude:
        match self:
            case Magnitude(tag="scalar", scalar=u):
                return Magnitude.Scalar(linear(u))
            case Magnitude(tag="correlated", correlated=(u, p)):
                return Magnitude.Correlated(linear(u), p)
            case unreachable:
                assert_never(unreachable)

    def reseat(self, cell: UFloat, /) -> Magnitude:
        match self:
            case Magnitude(tag="scalar"):
                return Magnitude.Scalar(cell)
            case Magnitude(tag="correlated", correlated=(_, p)):
                return Magnitude.Correlated(cell, p)
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class Covariance:
    tag: Literal["full", "norm"] = tag()
    full: Sequence[Sequence[float]] = case()
    norm: tuple[Sequence[float], Sequence[Sequence[float]]] = case()

    @staticmethod
    def Full(matrix: Sequence[Sequence[float]]) -> Covariance:
        return Covariance(full=matrix)

    @staticmethod
    def Norm(std_devs: Sequence[float], correlation: Sequence[Sequence[float]]) -> Covariance:
        return Covariance(norm=(std_devs, correlation))


class Umath(StrEnum):
    SQRT = "sqrt"
    EXP = "exp"
    LOG = "log"
    LOG10 = "log10"
    SIN = "sin"
    COS = "cos"
    TAN = "tan"
    ASIN = "asin"
    ACOS = "acos"
    ATAN = "atan"
    HYPOT = "hypot"
    ERF = "erf"
    ERFC = "erfc"
    GAMMA = "gamma"
    LGAMMA = "lgamma"


@tagged_union(frozen=True)
class Propagation:
    tag: Literal["named", "wrapped", "analytic"] = tag()
    named: Umath = case()
    wrapped: Callable[..., float] = case()
    analytic: tuple[Callable[..., float], tuple[Callable[..., float], ...]] = case()

    @staticmethod
    def Named(fn: Umath) -> Propagation:
        return Propagation(named=fn)

    @staticmethod
    def Wrapped(fn: Callable[..., float]) -> Propagation:
        return Propagation(wrapped=fn)

    @staticmethod
    def Analytic(fn: Callable[..., float], partials: tuple[Callable[..., float], ...]) -> Propagation:
        return Propagation(analytic=(fn, partials))

    def lifted(self) -> Callable[..., UFloat]:
        match self:
            case Propagation(tag="named", named=fn):
                return getattr(umath, fn.value)
            case Propagation(tag="wrapped", wrapped=fn):
                return wrap(fn)
            case Propagation(tag="analytic", analytic=(fn, partials)):
                return wrap(fn, derivatives_args=list(partials))
            case unreachable:
                assert_never(unreachable)


class CohortView(StrEnum):
    COVARIANCE = "covariance"
    CORRELATION = "correlation"
    PACKED = "packed"


# --- [MODELS] ------------------------------------------------------------------------------
class QuantityReceipt(Struct, frozen=True):
    unit_expr: str
    dimensionality: str
    nominal: float
    std_dev: float
    rel_error: float
    mode: str
    correlated_with: tuple[str, ...]
    components: tuple[tuple[str, float], ...]

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "compute.quantity",
            self.unit_expr,
            {
                "dim": self.dimensionality,
                "nominal": repr(self.nominal),
                "std_dev": repr(self.std_dev),
                "rel_error": repr(self.rel_error),
                "mode": self.mode,
                "correlated_with": ",".join(self.correlated_with),
                "components": ";".join(f"{tag}={var:.3e}" for tag, var in self.components),
            },
        )


class UncertainQuantity(Struct, frozen=True):
    measurement: pint.Measurement
    magnitude: Magnitude

    @classmethod
    def of(cls, nominal: float, std_dev: float, unit: str, /) -> RuntimeRail[UncertainQuantity]:
        return boundary(
            "quantity.of",
            lambda: cls(_UREG.Measurement(nominal, std_dev, unit), Magnitude.Scalar(ufloat(nominal, std_dev))),
        )

    @classmethod
    def correlated(
        cls, nominals: Sequence[float], covariance: Covariance, unit: str, tags: tuple[str, ...], /
    ) -> RuntimeRail[tuple[UncertainQuantity, ...]]:
        def _build() -> tuple[UncertainQuantity, ...]:
            match covariance:
                case Covariance(tag="full", full=matrix):
                    cells = correlated_values(list(nominals), [list(r) for r in matrix], tags=list(tags))
                case Covariance(tag="norm", norm=(stds, corr)):
                    cells = correlated_values_norm(
                        list(zip(nominals, stds, strict=True)), [list(r) for r in corr], tags=list(tags)
                    )
                case unreachable:
                    assert_never(unreachable)
            return tuple(
                cls(
                    _UREG.Measurement(cell.nominal_value, cell.std_dev, unit),
                    Magnitude.Correlated(cell, tuple(t for t in tags if t != self_tag)),
                )
                for cell, self_tag in zip(cells, tags, strict=True)
            )

        return boundary("quantity.correlated", _build)

    def convert(self, target_unit: str, /) -> RuntimeRail[UncertainQuantity]:
        def _to() -> UncertainQuantity:
            converted = self.measurement.to(target_unit)
            return UncertainQuantity(converted, self.magnitude.reseat(converted.magnitude))

        return boundary("quantity.convert", _to)

    def propagate(self, propagation: Propagation, unit: str, /) -> RuntimeRail[UncertainQuantity]:
        def _apply() -> UncertainQuantity:
            mag = self.magnitude.through(propagation.lifted())
            out = mag.cell
            return UncertainQuantity(_UREG.Measurement(out.nominal_value, out.std_dev, unit), mag)

        return boundary("quantity.propagate", _apply)

    def claim(self) -> QuantityReceipt:
        cell = self.magnitude.cell
        rel = (
            float(abs(cell.std_dev / cell.nominal_value))
            if cell.nominal_value
            else (0.0 if cell.std_dev == 0.0 else float("inf"))
        )
        return QuantityReceipt(
            unit_expr=f"{self.measurement.units:~}",
            dimensionality=str(dict(self.measurement.units.dimensionality)),
            nominal=float(cell.nominal_value),
            std_dev=float(cell.std_dev),
            rel_error=rel,
            mode=self.magnitude.tag,
            correlated_with=self.magnitude.peers,
            components=tuple((var.tag or repr(var), float(err)) for var, err in cell.error_components().items()),
        )


# --- [OPERATIONS] --------------------------------------------------------------------------
def cohort(quantities: Sequence[UncertainQuantity], view: CohortView, /) -> RuntimeRail[np.ndarray]:
    def _read() -> np.ndarray:
        cells = [q.magnitude.cell for q in quantities]
        match view:
            case CohortView.COVARIANCE:
                return np.asarray(covariance_matrix(cells), dtype=np.float64)
            case CohortView.CORRELATION:
                return np.asarray(correlation_matrix(cells), dtype=np.float64)
            case CohortView.PACKED:
                arr = unumpy.uarray([c.nominal_value for c in cells], [c.std_dev for c in cells])
                return np.stack([unumpy.nominal_values(arr), unumpy.std_devs(arr)])
            case unreachable:
                assert_never(unreachable)

    return boundary(f"quantity.cohort.{view.value}", _read)
```

## [03]-[RESEARCH]

- [PINT_MEASUREMENT]: `pint` and `uncertainties` are cp315-clean and reflected; the `pint.Measurement(value, error, units)` value-and-error bridge, `Quantity.to`/`.dimensionality`/`.magnitude`/`.units`, `DimensionalityError`, and the `ufloat`/`correlated_values(nominals, covariance, tags=...)`/`correlated_values_norm(values_with_std_dev, correlation_mat, tags=...)`/`covariance_matrix`/`correlation_matrix`/`wrap(f, derivatives_args=...)`/`umath` spellings plus `UFloat.nominal_value`/`.std_dev`/`.tag`/`.error_components()` and `unumpy.uarray`/`nominal_values`/`std_devs` verify against the `.api` catalogue (`.api/uncertainties.md` ENTRYPOINTS [03][01]-[09], `unumpy` [03][01]/[03]/[04], `UFloat` members [02][01]/[02]/[04]/[07]).
- [APPLICATION_REGISTRY]: `pint.get_application_registry()`/`pint.set_application_registry(registry)` are the module-level shared-registry entries (`.api/pint.md` ENTRYPOINTS [03][01]/[02], IMPLEMENTATION_LAW UNIT_TOPOLOGY and LOCAL_ADMISSION). One process-wide `UnitRegistry` is installed once and read through the frozen `_UREG` handle; the `.api` RAIL_LAW rejects per-claim registries because their `Quantity` types do not interoperate, so the prior `_UREG = pint.UnitRegistry()` per-module instance was the defect and is replaced by the application-registry handle.
- [MAGNITUDE_FOLD]: the scalar/correlated structural tag IS the propagation mode, so the prior parallel `PropagationMode` `StrEnum` field shadowing the `Magnitude` tag was a duplicate discriminant and is deleted; `through` (linear map) and `reseat` (externally-produced cell) are the two rebuild folds over the one tag, collapsing the near-identical `Magnitude.Scalar`/`Magnitude.Correlated`-rebuild-plus-`_peers()` branches into two total arms — `propagate` folds through `through`, `convert` reseats the pint-produced cell. The `cell`/`peers` projections and both folds are total matches closed by `assert_never`, never a `case _` wildcard that erases exhaustiveness. The receipt's `mode` reads the tag directly.
- [PROPAGATION_POLICY]: `Propagation` collapses the propagation algebra the prior single hardcoded `wrap(expr)` left half-unrealized — `Named` carries a bounded `Umath` `StrEnum` of the admitted `uncertainties.umath` functions (`.api/uncertainties.md` `umath` [03][01]-[05]) resolved through `getattr(umath, fn.value)`, never a free-form string the registry might not own, `Wrapped` lifts an arbitrary callable through `wrap` (numerical differentiation), and `Analytic` registers analytic partials through `wrap(f, derivatives_args=...)` (`.api/uncertainties.md` ENTRYPOINTS [03][08]); `propagate` takes the policy value, never a bare callable, so the umath and analytic-derivative paths the prose claimed are now reachable rows.
- [COHORT_PROVENANCE]: `cohort` collapses the prior sibling `covariance_provenance`/`correlation_provenance` functions into one `CohortView`-discriminated entry and adds the `Packed` view over `unumpy.uarray`/`nominal_values`/`std_devs`, so the nominal-and-error array provenance the catalogue admits is realized rather than absent; `correlated` admits both a full covariance matrix and a `Covariance.Norm` standard-deviation-and-correlation pair through `correlated_values_norm`, making admission symmetric with the `correlation` provenance view.
- [DIMENSIONALITY_RAIL]: `convert` runs `self.measurement.to(target)` inside the `boundary` thunk, so pint's native `DimensionalityError` (`.api/pint.md` PUBLIC_TYPES [02][03]) converts to a `BoundaryFault` exactly once through the one conversion surface — the prior hand-minted `Error(BoundaryFault(boundary=("dimensionality", ...)))` pre-check both duplicated pint's own dimensionality test and minted the `boundary` case the conversion surface reserves, and is deleted. `convert` reseats the `Magnitude` onto `converted.magnitude` (`.api/pint.md` ENTRYPOINTS [03-convert][01], PUBLIC_TYPES [02][04] — pint's `Measurement` backs its magnitude with the `uncertainties` `UFloat`), so the conversion — affine offset units (`OffsetUnitCalculusError` domain) included — and the correlation graph stay owned by pint/`uncertainties`; the prior `_UREG.Quantity(1.0, units).to(target).magnitude` ratio scaled the cell by a single multiplicative factor that silently corrupts an affine temperature conversion and re-derived a transform pint already performs, and is deleted. A `0.0 ± σ` measurement converts through pint without a `converted/nominal` value ratio; `claim` separates an exact zero (`rel_error == 0.0`) from a zero-nominal value carrying genuine spread (`rel_error == inf`) and emits the per-source standard-deviation contribution through `UFloat.error_components()`.
- [MONTE_CARLO_BRIDGE]: the large-uncertainty regime routes to the GUM-supplement-1 Monte-Carlo path on the `experiments/study.md#STUDY` sampler rather than a parallel surrogate; `mcerp` Monte-Carlo and `soerp` second-order propagation are candidate study-method bridges verified against the `.api` catalogue once admitted.
