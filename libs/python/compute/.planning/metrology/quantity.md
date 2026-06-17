# [PY_COMPUTE_QUANTITY]

The one unit-bearing uncertain-quantity owner. `UncertainQuantity` threads a correlated `uncertainties.UFloat` magnitude through the pint unit algebra via the native `pint.Measurement` bridge, so unit conversion and first-order error propagation compose on one value. `PropagationMode` discriminates a scalar magnitude, a correlated vector sharing one covariance, and an expression propagated through the uncertainties graph. The owner recovers the joint covariance and correlation provenance after propagation. pint and uncertainties are both cp315-clean, so this owner is reflected; linear first-order propagation is the boundary, and a large-uncertainty regime routes to the study Monte-Carlo sampler.

## [1]-[INDEX]

[QUANTITY]: correlated first-order uncertainty threaded through the pint unit algebra on one `UncertainQuantity` owner.

## [2]-[QUANTITY]

- Owner: `UncertainQuantity` — the ONE quantity-evidence owner threading correlated uncertainty through the pint unit algebra; a `pint.Measurement` (value-and-error `Quantity` over the shared `UnitRegistry`) carries the unit dimension and the `uncertainties.UFloat` magnitude in one object, so unit conversion and first-order propagation compose on the same value. `Magnitude` discriminates a scalar `UFloat` from a correlated cell carrying its peer tags; `PropagationMode` discriminates the propagation across scalar, correlated, and expression cases. No parallel uncertain type stands beside the unit-bearing one.
- Entry: `UncertainQuantity.of` admits a nominal, standard deviation, and unit into a `pint.Measurement` and returns `RuntimeRail[UncertainQuantity]`; `correlated` admits a vector with one shared covariance through `uncertainties.correlated_values`; `convert` runs the pint conversion preserving the relative error; `propagate` folds a derived expression through the uncertainties graph with the standard deviation flowing by the chain rule; `claim` projects the unit dimension, nominal, standard deviation, relative error, and correlation provenance into a `QuantityReceipt`.
- Provenance: `covariance_provenance` and `correlation_provenance` recover the joint covariance and correlation matrices over a correlated cohort through `uncertainties.covariance_matrix`/`correlation_matrix`, and `UFloat.error_components` attributes variance back to source variables. The `EXPRESSION` mode routes an elementary function through `uncertainties.umath` or an arbitrary callable through `uncertainties.wrap`, which differentiates numerically when no analytic derivative is registered.
- Packages: `pint` (`UnitRegistry`, `Quantity`, `Measurement`, `to`, `to_base_units`, `dimensionality`, `DimensionalityError`), `uncertainties` (`ufloat`, `correlated_values`, `covariance_matrix`, `correlation_matrix`, `wrap`, `umath`, `UFloat.nominal_value`, `UFloat.std_dev`, `UFloat.error_components`), `numpy` (`asarray`), runtime (`RuntimeRail`, `boundary`, `Receipt`, `ReceiptContributor`).
- Growth: a new quantity family is one registry unit string; a new propagation mode is one `PropagationMode` row; zero new surface.
- Boundary: no C# quantity or unit owner minting and no production error policy; offline propagation evidence only. Linear first-order propagation is the boundary — a second-order or Monte-Carlo error model routes to the `experiments/study.md#STUDY` sampler, never a parallel surrogate. A hand-rolled unit-conversion table, a parallel uncertain-magnitude type beside the unit-bearing one, and an uncorrelated re-derivation of a propagated variance are the deleted forms. pint and uncertainties are both cp315-clean, so the owner is reflected.

```python
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
from __future__ import annotations

from collections.abc import Callable, Sequence
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
import pint
from expression import Error, case, tag, tagged_union
from msgspec import Struct
from uncertainties import UFloat, correlated_values, correlation_matrix, covariance_matrix, ufloat, wrap

from rasm.runtime.observability.receipts import Receipt
from rasm.runtime.faults import BoundaryFault
from rasm.runtime.faults import RuntimeRail, boundary

# --- [CONSTANTS] -----------------------------------------------------------------------
_UREG: pint.UnitRegistry = pint.UnitRegistry()


# --- [TYPES] ---------------------------------------------------------------------------
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


class UncertainQuantity(Struct, frozen=True):
    measurement: pint.Measurement
    magnitude: Magnitude
    mode: PropagationMode

    # --- [BOUNDARIES] ------------------------------------------------------------------
    @classmethod
    def of(cls, nominal: float, std_dev: float, unit: str, /) -> RuntimeRail[UncertainQuantity]:
        def _build() -> UncertainQuantity:
            meas = _UREG.Measurement(nominal, std_dev, unit)
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
        def _to() -> UncertainQuantity:
            converted = self.measurement.to(target_unit)
            factor = float(converted.value.magnitude) / self.magnitude.cell.nominal_value
            scaled = self.magnitude.cell * factor
            mag = Magnitude.Scalar(scaled) if self.mode is PropagationMode.SCALAR else Magnitude.Correlated(scaled, self._peers())
            return UncertainQuantity(converted, mag, self.mode)

        return (
            boundary("quantity.convert", _to)
            if self.measurement.units.dimensionality == _UREG.Unit(target_unit).dimensionality
            else Error(BoundaryFault.Boundary("dimensionality", f"{self._unit_expr()}->{target_unit}"))
        )

    def propagate(self, expr: Callable[[float], float], unit: str, /) -> RuntimeRail[UncertainQuantity]:
        def _apply() -> UncertainQuantity:
            out: UFloat = wrap(expr)(self.magnitude.cell)
            mag = Magnitude.Scalar(out) if self.mode is PropagationMode.SCALAR else Magnitude.Correlated(out, self._peers())
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
    return boundary(
        "quantity.covariance",
        lambda: np.asarray(covariance_matrix([q.magnitude.cell for q in quantities]), dtype=np.float64),
    )


def correlation_provenance(quantities: Sequence[UncertainQuantity]) -> RuntimeRail[np.ndarray]:
    return boundary(
        "quantity.correlation",
        lambda: np.asarray(correlation_matrix([q.magnitude.cell for q in quantities]), dtype=np.float64),
    )
```

## [3]-[RESEARCH]

- [PINT_MEASUREMENT]: `pint` and `uncertainties` are cp315-clean and reflected; the `pint.Measurement(value, error, units)` value-and-error bridge, `Quantity.to`/`.dimensionality`/`.magnitude`/`.units`, `DimensionalityError`, and the `ufloat`/`correlated_values(nominals, covariance, tags=...)`/`covariance_matrix`/`correlation_matrix`/`wrap`/`umath` spellings plus `UFloat.nominal_value`/`.std_dev`/`.error_components` verify against the branch `.api` catalogue.
- [MONTE_CARLO_BRIDGE]: the large-uncertainty regime routes to the GUM-supplement-1 Monte-Carlo path on the `experiments/study.md#STUDY` sampler rather than a parallel surrogate; `mcerp` Monte-Carlo and `soerp` second-order propagation are candidate study-method bridges verified against the branch `.api` catalogue once admitted.
