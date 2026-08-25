# [PY_COMPUTE_QUANTITY]

`UncertainQuantity` is the one unit-bearing uncertain-quantity owner, threading a correlated `uncertainties.UFloat` magnitude through the pint unit algebra via the native `pint.Measurement` bridge, so unit conversion, first-order error propagation, content identity, and graduation compose on one value. Linear first-order propagation is the boundary — a large-uncertainty regime routes to the study Monte-Carlo sampler.

One read-only frozen application registry owns the unit vocabulary, shared through `pint.get_application_registry` — no per-call `UnitRegistry()`, no owner-side `define` — so every minted `Quantity`/`Measurement` stays arithmetically compatible across the folder. Dimensional-consistency claims graduate through the `graduation/handoff#GRADUATION` `unit_law` axis — the same residual-over-ceiling gate `experiments/inference#BAYESIAN` `uncertainty_law` evidence feeds — and a cohort whose data admits as a `numerics/array#PAYLOAD` payload keys under the same `ContentIdentity` seed.

## [01]-[INDEX]

- [02]-[QUANTITY]: correlated first-order uncertainty through the pint unit algebra on one `UncertainQuantity` owner over the `Magnitude`/`Covariance`/`Propagation`/`CohortView` policies.

## [02]-[QUANTITY]

- Owner: `UncertainQuantity` — the `Magnitude` structural tag IS the propagation mode, so no parallel mode field shadows it, and no parallel uncertain type stands beside the unit-bearing one.
- Cases: `Magnitude.join` subsumes the unary map — a scalar with no peers stays scalar — so unary and n-ary propagation are one fold rather than two near-identical rebuild branches; `Umath` members carry their own `arity`, and `propagate` gates the supplied operand count BEFORE the lifted call, so an arity mismatch is the rostered `ARITY` reject carrying both counts as named slots instead of the opaque `wrap`-call `TypeError` the fence would flatten; cohort admission is symmetric with the `CORRELATION` view — a cohort built from a correlation matrix reads back as one.
- Entry: each cohort member re-keys over the cohort key and its own unique tag, so two siblings never share a `content_key` — a shared key collides them as cache keys and as propagation-operand bytes, returning a stale propagation for a different operand; a converted or propagated value re-keys because it is a new value, never a source-key cache collision.
- Receipt: the hub `evidence_run` weave rides the two OPERAND-SCALED entries — the cohort reconstruction and the `CohortView` read, whose inverse arms run a cubic uncertainty-propagating solve — so the branch's universal evidence floor holds at this owner and the folder's most expensive numerics kernel reports its own resource band. The scalar mint, convert, and propagate entries stay bare: banding a single `ufloat` construction prices the instrument rather than the kernel. Composition custody is the caller's on the weave and on the graduation projection alike, defaulted so the root call shape stays scope-free.
- Growth: a new refusal is one `RAISES` row whose `slots` name its coordinates and whose `catch` anchor names the provider set its fence reaches; a new elementary function is one `Umath` member carrying its `(value, arity)` the arity gate consumes for free; a new propagation algebra is one `Propagation` case with its `lifted`/`label` arms; a new cohort construction is one `Covariance` case with its `reconstruct` AND `canonical` arms — the second so the payload participates in the content key as its own framed fields; a new provenance view is one `CohortView` row with its fold arm; a stricter unit bar is one tighter `_UNIT_CEILING` row or the caller's override.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from enum import StrEnum
from math import isqrt
from typing import Final, Literal, assert_never

import numpy as np
import pint
from expression import Block, Error, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from numpy.linalg import LinAlgError
from uncertainties import UFloat, correlated_values, correlated_values_norm, correlation_matrix, covariance_matrix, ufloat, umath, unumpy, wrap
from uncertainties.core import NegativeStdDev
from uncertainties.unumpy import ulinalg

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import TERMINAL, Catch, FaultRow, RuntimeRail, boundary, railed, rostered, traversed
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey

_UREG: pint.UnitRegistry = pint.get_application_registry()


# --- [TYPES] ----------------------------------------------------------------------------


class Umath(StrEnum):
    arity: int

    def __new__(cls, fn: str, arity: int) -> "Umath":
        member = str.__new__(cls, fn)
        member._value_ = fn
        member.arity = arity
        return member

    SQRT = "sqrt", 1
    EXP = "exp", 1
    LOG = "log", 1
    LOG10 = "log10", 1
    LOG1P = "log1p", 1
    EXPM1 = "expm1", 1
    SIN = "sin", 1
    COS = "cos", 1
    TAN = "tan", 1
    SINH = "sinh", 1
    COSH = "cosh", 1
    TANH = "tanh", 1
    ASIN = "asin", 1
    ACOS = "acos", 1
    ATAN = "atan", 1
    ASINH = "asinh", 1
    ACOSH = "acosh", 1
    ATANH = "atanh", 1
    FABS = "fabs", 1
    ERF = "erf", 1
    ERFC = "erfc", 1
    GAMMA = "gamma", 1
    LGAMMA = "lgamma", 1
    DEGREES = "degrees", 1
    RADIANS = "radians", 1
    ATAN2 = "atan2", 2
    HYPOT = "hypot", 2
    POW = "pow", 2


@tagged_union(frozen=True)
class Magnitude:
    tag: Literal["scalar", "correlated"] = tag()
    scalar: UFloat = case()
    correlated: tuple[UFloat, tuple[str, ...]] = case()

    @staticmethod
    def Scalar(value: UFloat) -> "Magnitude":
        return Magnitude(scalar=value)

    @staticmethod
    def Correlated(value: UFloat, peers: tuple[str, ...]) -> "Magnitude":
        return Magnitude(correlated=(value, peers))

    @property
    def cell(self) -> UFloat:
        match self:
            case Magnitude(tag="scalar", scalar=u):
                return u
            case Magnitude(tag="correlated", correlated=(u, _)):
                return u
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def peers(self) -> tuple[str, ...]:
        match self:
            case Magnitude(tag="scalar"):
                return ()
            case Magnitude(tag="correlated", correlated=(_, p)):
                return p
            case _ as unreachable:
                assert_never(unreachable)

    def reseat(self, cell: UFloat, /) -> "Magnitude":
        return Magnitude.Correlated(cell, self.peers) if self.tag == "correlated" else Magnitude.Scalar(cell)

    def join(self, others: "tuple[Magnitude, ...]", apply: Callable[..., UFloat], /) -> "Magnitude":
        cohort = (self, *others)
        cell = apply(*(m.cell for m in cohort))
        peers = tuple(dict.fromkeys(p for m in cohort for p in m.peers))
        return Magnitude.Correlated(cell, peers) if peers else Magnitude.Scalar(cell)


@tagged_union(frozen=True)
class Covariance:
    tag: Literal["full", "norm"] = tag()
    full: tuple[tuple[float, ...], ...] = case()
    norm: tuple[tuple[float, ...], tuple[tuple[float, ...], ...]] = case()

    @staticmethod
    def Full(matrix: Sequence[Sequence[float]]) -> "Covariance":
        return Covariance(full=tuple(tuple(map(float, row)) for row in matrix))

    @staticmethod
    def Norm(std_devs: Sequence[float], correlation: Sequence[Sequence[float]]) -> "Covariance":
        return Covariance(norm=(tuple(map(float, std_devs)), tuple(tuple(map(float, row)) for row in correlation)))

    def reconstruct(self, nominals: Sequence[float], tags: Sequence[str], /) -> Sequence[UFloat]:
        match self:
            case Covariance(tag="full", full=matrix):
                return correlated_values(list(nominals), [list(r) for r in matrix], tags=list(tags))
            case Covariance(tag="norm", norm=(stds, corr)):
                return correlated_values_norm(list(zip(nominals, stds, strict=True)), [list(r) for r in corr], tags=list(tags))
            case _ as unreachable:
                assert_never(unreachable)

    def canonical(self) -> tuple[bytes, ...]:
        match self:
            case Covariance(tag="full", full=matrix):
                return (b"full", np.ascontiguousarray(matrix, dtype=np.float64).tobytes())
            case Covariance(tag="norm", norm=(stds, corr)):
                return (
                    b"norm",
                    np.ascontiguousarray(stds, dtype=np.float64).tobytes(),
                    np.ascontiguousarray(corr, dtype=np.float64).tobytes(),
                )
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class Propagation:
    tag: Literal["named", "wrapped", "analytic"] = tag()
    named: Umath = case()
    wrapped: Callable[..., float] = case()
    analytic: tuple[Callable[..., float], tuple[Callable[..., float], ...]] = case()

    @staticmethod
    def Named(fn: Umath) -> "Propagation":
        return Propagation(named=fn)

    @staticmethod
    def Wrapped(fn: Callable[..., float]) -> "Propagation":
        return Propagation(wrapped=fn)

    @staticmethod
    def Analytic(fn: Callable[..., float], partials: tuple[Callable[..., float], ...]) -> "Propagation":
        return Propagation(analytic=(fn, partials))

    @property
    def arity(self) -> int:
        match self:
            case Propagation(tag="named", named=fn):
                return fn.arity
            case Propagation(tag="analytic", analytic=(_, partials)):
                return len(partials)
            case Propagation(tag="wrapped"):
                return -1
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def label(self) -> str:
        match self:
            case Propagation(tag="named", named=fn):
                return fn.value
            case Propagation(tag="wrapped", wrapped=fn):
                return fn.__qualname__
            case Propagation(tag="analytic", analytic=(fn, _)):
                return fn.__qualname__
            case _ as unreachable:
                assert_never(unreachable)

    def lifted(self) -> Callable[..., UFloat]:
        match self:
            case Propagation(tag="named", named=fn):
                return getattr(umath, fn.value)
            case Propagation(tag="wrapped", wrapped=fn):
                return wrap(fn)
            case Propagation(tag="analytic", analytic=(fn, partials)):
                return wrap(fn, derivatives_args=list(partials))
            case _ as unreachable:
                assert_never(unreachable)

    def apply(self, *cells: UFloat) -> UFloat:
        return self.lifted()(*cells)


class CohortView(StrEnum):
    COVARIANCE = "covariance"
    CORRELATION = "correlation"
    PACKED = "packed"
    INVERSE = "inverse"
    PSEUDOINVERSE = "pseudoinverse"


# --- [CONSTANTS] ------------------------------------------------------------------------

_UNIT_CEILING: Final[Map[str, float]] = Map.of_seq([("consistency", 0.0)])


# --- [TABLES] ---------------------------------------------------------------------------

_UNIT_CATCH: Final[Catch] = (pint.errors.PintError, NegativeStdDev)
_PROPAGATE_CATCH: Final[Catch] = (pint.errors.PintError, NegativeStdDev, TypeError, ValueError)
_COHORT_CATCH: Final[Catch] = (LinAlgError, NegativeStdDev, ValueError)

MINT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.QUANTITY, point="mint", arm="config", defect="mint", retriability=TERMINAL
)
CONVERT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.QUANTITY, point="convert", arm="config", defect="convert", retriability=TERMINAL
)
PROPAGATE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.QUANTITY, point="propagate", arm="boundary", defect="propagate", retriability=TERMINAL
)
ARITY: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.QUANTITY, point="arity", arm="config", defect="arity", retriability=TERMINAL, slots=("declared", "supplied")
)
COHORT_BUILD: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.QUANTITY, point="cohort", arm="config", defect="cohort-build", retriability=TERMINAL
)
COHORT_READ: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.QUANTITY, point="view", arm="boundary", defect="cohort-view", retriability=TERMINAL
)
CONSISTENT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.QUANTITY, point="consistent", arm="boundary", defect="offset-unit", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([MINT, CONVERT, PROPAGATE, ARITY, COHORT_BUILD, COHORT_READ, CONSISTENT]))


# --- [MODELS] ---------------------------------------------------------------------------


class QuantityReceipt(Struct, frozen=True, gc=False):
    unit_expr: str
    dimensionality: str
    nominal: float
    std_dev: float
    rel_error: float
    band: Block[str]
    mode: str
    correlated_with: tuple[str, ...]
    components: tuple[tuple[str, float], ...]
    content_key: ContentKey

    def graduates(self, ceiling: dict[str, float] | None = None, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[GraduationReceipt]":
        measured = {"consistency": float(len(self.band))}
        return GraduationReceipt.graduates(
            EvidenceScope.QUANTITY.value,
            HandoffAxis(unit_law=self.unit_expr),
            self.content_key,
            measured,
            ceiling or dict(_UNIT_CEILING.items()),
            composition=composition,
        )

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "dim": self.dimensionality,
            "nominal": self.nominal,
            "std_dev": self.std_dev,
            "rel_error": self.rel_error,
            "mode": self.mode,
            "correlated_with": self.correlated_with,
            "components": self.components,
        }
        return (
            Receipt.of(
                EvidenceScope.QUANTITY.value,
                ("emitted", self.unit_expr, facts),
                key=Some(self.content_key),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.content_key)),
                band=self.band,
            ),
        )


class UncertainQuantity(Struct, frozen=True):
    measurement: pint.Measurement
    magnitude: Magnitude
    content_key: ContentKey

    @classmethod
    def of(cls, nominal: float, std_dev: float, unit: str, /) -> "RuntimeRail[UncertainQuantity]":
        def _build() -> "RuntimeRail[UncertainQuantity]":
            cell = ufloat(nominal, std_dev)
            measurement = _UREG.Measurement(nominal, std_dev, unit)
            return _scalar_key(nominal, std_dev, unit).map(lambda key: cls(measurement, Magnitude.Scalar(cell), key))

        return boundary(MINT, _build, catch=_UNIT_CATCH).bind(lambda outcome: outcome)

    @classmethod
    def correlated(
        cls, nominals: Sequence[float], covariance: Covariance, unit: str, tags: tuple[str, ...], /, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> "RuntimeRail[tuple[UncertainQuantity, ...]]":
        @railed
        def _build() -> "tuple[UncertainQuantity, ...]":
            cells = covariance.reconstruct(nominals, tags)
            cohort: ContentKey = yield from _cohort_key(nominals, covariance, unit, tags)
            members: Block[UncertainQuantity] = yield from traversed(
                Block.of_seq(
                    _member_key(cohort, self_tag).map(
                        lambda key, cell=cell, self_tag=self_tag: cls(
                            _UREG.Measurement(cell.nominal_value, cell.std_dev, unit),
                            Magnitude.Correlated(cell, tuple(t for t in tags if t != self_tag)),
                            key,
                        )
                    )
                    for cell, self_tag in zip(cells, tags, strict=True)
                )
            )
            return tuple(members)

        facts = {"members": len(tags), "covariance": covariance.tag, "unit": unit}
        return evidence_run(
            EvidenceScope.QUANTITY, "quantity.correlated", lambda: boundary(COHORT_BUILD, _build, catch=_COHORT_CATCH).bind(lambda outcome: outcome),
            facts=facts, composition=composition,
        )

    def convert(self, target_unit: str, /) -> "RuntimeRail[UncertainQuantity]":
        def _to() -> "RuntimeRail[UncertainQuantity]":
            converted = self.measurement.to(target_unit)
            cell = converted.magnitude
            return _scalar_key(cell.nominal_value, cell.std_dev, target_unit).map(
                lambda key: UncertainQuantity(converted, self.magnitude.reseat(cell), key)
            )

        return boundary(CONVERT, _to, catch=_UNIT_CATCH).bind(lambda outcome: outcome)

    def propagate(self, propagation: Propagation, unit: str, /, *operands: "UncertainQuantity") -> "RuntimeRail[UncertainQuantity]":
        def _build() -> "RuntimeRail[UncertainQuantity]":
            supplied = 1 + len(operands)
            if propagation.arity >= 0 and supplied != propagation.arity:
                return Error(ARITY.raised(str(propagation.arity), str(supplied)))
            mag = self.magnitude.join(tuple(o.magnitude for o in operands), propagation.apply)
            out = mag.cell
            return _propagated_key(propagation, unit, (self, *operands)).map(
                lambda key: UncertainQuantity(_UREG.Measurement(out.nominal_value, out.std_dev, unit), mag, key)
            )

        return boundary(PROPAGATE, _build, catch=_PROPAGATE_CATCH).bind(lambda outcome: outcome)

    def claim(self) -> QuantityReceipt:
        cell = self.magnitude.cell
        rel = float(abs(cell.std_dev / cell.nominal_value)) if cell.nominal_value else (0.0 if cell.std_dev == 0.0 else float("inf"))
        dim = dict(self.measurement.units.dimensionality)
        reduced = boundary(CONSISTENT, self.measurement.to_base_units, catch=_UNIT_CATCH)
        return QuantityReceipt(
            unit_expr=f"{self.measurement.units:~}",
            dimensionality=str(dim),
            nominal=float(cell.nominal_value),
            std_dev=float(cell.std_dev),
            rel_error=rel,
            band=Block.of_seq(reduced.swap().to_option().map(lambda fault: fault.subject).to_list()),
            mode=self.magnitude.tag,
            correlated_with=self.magnitude.peers,
            components=tuple((var.tag or repr(var), float(err)) for var, err in cell.error_components().items()),
            content_key=self.content_key,
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def cohort(quantities: Sequence[UncertainQuantity], view: CohortView, /, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[np.ndarray]":
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
            case CohortView.INVERSE | CohortView.PSEUDOINVERSE:
                side = isqrt(len(cells))
                if side * side != len(cells):
                    raise ValueError(f"non-square cohort: {len(cells)} cells admit no square matrix")
                mat = unumpy.umatrix([c.nominal_value for c in cells], [c.std_dev for c in cells]).reshape(side, side)
                solved = ulinalg.inv(mat) if view is CohortView.INVERSE else ulinalg.pinv(mat)
                return np.stack([unumpy.nominal_values(solved), unumpy.std_devs(solved)])
            case _ as unreachable:
                assert_never(unreachable)

    facts = {"view": view.value, "members": len(quantities)}
    return evidence_run(
        EvidenceScope.QUANTITY, f"quantity.cohort.{view.value}", lambda: boundary(COHORT_READ, _read, catch=_COHORT_CATCH),
        facts=facts, composition=composition,
    )


def _scalar_key(nominal: float, std_dev: float, unit: str, /) -> "RuntimeRail[ContentKey]":
    cell = np.ascontiguousarray([nominal, std_dev], dtype=np.float64).tobytes()
    return ContentIdentity.of("quantity", IdentitySource(parts=(cell, unit.encode())))


def _cohort_key(nominals: Sequence[float], covariance: Covariance, unit: str, tags: Sequence[str], /) -> "RuntimeRail[ContentKey]":
    return ContentIdentity.of(
        "quantity.cohort",
        IdentitySource(parts=(
            np.ascontiguousarray(list(nominals), dtype=np.float64).tobytes(),
            *covariance.canonical(),
            unit.encode(),
            str(len(tags)).encode(),
            *(t.encode() for t in tags),
        )),
    )


def _member_key(cohort: ContentKey, member_tag: str, /) -> "RuntimeRail[ContentKey]":
    return ContentIdentity.of("quantity.member", IdentitySource(parts=(cohort.memory, member_tag.encode())))


def _propagated_key(propagation: Propagation, unit: str, operands: tuple["UncertainQuantity", ...], /) -> "RuntimeRail[ContentKey]":
    return ContentIdentity.of(
        "quantity.propagate",
        IdentitySource(parts=(propagation.label.encode(), unit.encode(), *(o.content_key.memory for o in operands))),
    )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
