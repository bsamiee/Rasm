# [PY_COMPUTE_QUANTITY]

`UncertainQuantity` is the one unit-bearing uncertain-quantity owner, threading a correlated `uncertainties.UFloat` magnitude through the pint unit algebra via the native `pint.Measurement` bridge, so unit conversion, first-order error propagation, content identity, and graduation compose on one value. Linear first-order propagation is the boundary — a large-uncertainty regime routes to the study Monte-Carlo sampler.

One read-only frozen application registry owns the unit vocabulary, shared through `pint.get_application_registry` — no per-call `UnitRegistry()`, no owner-side `define` — so every minted `Quantity`/`Measurement` stays arithmetically compatible across the folder. Dimensional-consistency claims graduate through the `graduation/handoff.md#GRADUATION` `unit_law` axis — the same residual-over-ceiling gate `experiments/inference.md#BAYESIAN` `uncertainty_law` evidence feeds — and a cohort whose data admits as a `numerics/array.md#PAYLOAD` payload keys under the same `ContentIdentity` seed.

## [01]-[INDEX]

- [01]-[QUANTITY]: correlated first-order uncertainty through the pint unit algebra on one `UncertainQuantity` owner over the `Magnitude`/`Covariance`/`Propagation`/`CohortView` policies.

## [02]-[QUANTITY]

- Owner: `UncertainQuantity` — the `Magnitude` structural tag IS the propagation mode, so no parallel mode field shadows it, and no parallel uncertain type stands beside the unit-bearing one.
- Cases: `Magnitude.join` subsumes the unary map — a scalar with no peers stays scalar — so unary and n-ary propagation are one fold rather than two near-identical rebuild branches; `Umath` members carry their own `arity`, and `propagate` gates the supplied operand count BEFORE the lifted call, so an arity mismatch is a typed reject instead of the opaque `wrap`-call `TypeError`; cohort admission is symmetric with the `CORRELATION` view — a cohort built from a correlation matrix reads back as one.
- Entry: each cohort member re-keys over the cohort key and its own unique tag, so two siblings never share a `content_key` — a shared key collides them as cache keys and as propagation-operand bytes, returning a stale propagation for a different operand; a converted or propagated value re-keys because it is a new value, never a source-key cache collision.
- Growth: a new elementary function is one `Umath` member carrying its `(value, arity)` the arity gate consumes for free; a new propagation algebra is one `Propagation` case with its `lifted`/`label` arms; a new cohort construction is one `Covariance` case with its `reconstruct` AND `canonical` arms — the second so the payload participates in the content key; a new provenance view is one `CohortView` row with its fold arm.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from enum import StrEnum
from math import isqrt
from typing import Literal, assert_never

import numpy as np
import pint
from expression import Block, Error, case, tag, tagged_union
from msgspec import Struct
from uncertainties import UFloat, correlated_values, correlated_values_norm, correlation_matrix, covariance_matrix, ufloat, umath, unumpy, wrap
from uncertainties.unumpy import ulinalg

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, railed, traversed
from rasm.runtime.receipts import Receipt

# read-once handle on the shared application registry; the composition root alone binds a custom registry,
# so this module runs no import-time `set_application_registry` mutation.
_UREG: pint.UnitRegistry = pint.get_application_registry()


# --- [TYPES] -------------------------------------------------------------------------------


class Umath(StrEnum):
    # `str` value is the `uncertainties.umath` attribute name and `arity` rides each member, collapsing the parallel arity table.
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
        # one tag-preserving swap reading `peers` once rather than re-matching the correlated payload.
        return Magnitude.Correlated(cell, self.peers) if self.tag == "correlated" else Magnitude.Scalar(cell)

    def join(self, others: "tuple[Magnitude, ...]", apply: Callable[..., UFloat], /) -> "Magnitude":
        # applied `UFloat` already carries the joint chain-rule gradient against all inputs, so the result is correlated with the
        # dedup-stable union of every operand's peers, scalar only when none carries peers.
        cohort = (self, *others)
        cell = apply(*(m.cell for m in cohort))
        peers = tuple(dict.fromkeys(p for m in cohort for p in m.peers))
        return Magnitude.Correlated(cell, peers) if peers else Magnitude.Scalar(cell)


@tagged_union(frozen=True)
class Covariance:
    # nested-`tuple` payloads keep the frozen union genuinely immutable and hashable — never a mutable `Sequence` field the freeze
    # cannot enforce; the factories accept an ergonomic `Sequence` and normalize at the construction edge.
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

    def canonical(self) -> bytes:
        # `norm` arm folds BOTH the std-dev vector AND the correlation block, so two cohorts differing only in `std_devs` key distinctly.
        match self:
            case Covariance(tag="full", full=matrix):
                return b"full" + np.ascontiguousarray(matrix, dtype=np.float64).tobytes()
            case Covariance(tag="norm", norm=(stds, corr)):
                return b"norm" + np.ascontiguousarray(stds, dtype=np.float64).tobytes() + np.ascontiguousarray(corr, dtype=np.float64).tobytes()
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
        # an `Analytic` is exactly `len(partials)`-ary because `wrap(f, derivatives_args=[...])` registers one partial per
        # positional argument; only a numerically-differentiated `Wrapped` is genuinely variadic (`-1`).
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
        # stable function identity `_propagated_key` seeds over: the `Umath` value or the callable's qualified name.
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


# --- [MODELS] ------------------------------------------------------------------------------


class QuantityReceipt(Struct, frozen=True, gc=False):
    unit_expr: str
    dimensionality: str
    nominal: float
    std_dev: float
    rel_error: float
    consistent: bool
    mode: str
    correlated_with: tuple[str, ...]
    components: tuple[tuple[str, float], ...]
    content_key: ContentKey

    def graduates(self) -> "RuntimeRail[GraduationReceipt]":
        # verdict is a `consistency` residual rejected against a `0.0` ceiling, so an inconsistent quantity is the
        # `Error(BoundaryFault)` the gate returns; the axis case IS the subject, never a parallel `subject: str` field.
        measured = {"consistency": 0.0 if self.consistent else 1.0}
        ceiling = {"consistency": 0.0}
        return GraduationReceipt.graduates(EvidenceScope.QUANTITY.value, HandoffAxis(unit_law=self.unit_expr), self.content_key, measured, ceiling)

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "dim": self.dimensionality,
            "nominal": self.nominal,
            "std_dev": self.std_dev,
            "rel_error": self.rel_error,
            "consistent": self.consistent,
            "mode": self.mode,
            "correlated_with": self.correlated_with,
            "components": self.components,
            "content_key": self.content_key,
        }
        return (Receipt.of(EvidenceScope.QUANTITY.value, ("emitted", self.unit_expr, facts)),)


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

        return boundary("quantity.of", _build).bind(lambda outcome: outcome)

    @classmethod
    def correlated(
        cls, nominals: Sequence[float], covariance: Covariance, unit: str, tags: tuple[str, ...], /
    ) -> "RuntimeRail[tuple[UncertainQuantity, ...]]":
        # a repeated cohort on identical data is a cache hit by reference; the per-member rails fold through `traversed` under the
        # default `Disposition.ABORT`, the fold `railed` does not subsume.
        @railed
        def _build() -> "tuple[UncertainQuantity, ...]":
            cells = covariance.reconstruct(nominals, tags)
            cohort: ContentKey = yield from _cohort_key(nominals, covariance, unit, tags)
            members: Block[UncertainQuantity] = yield from traversed(
                Block.of_seq(
                    # `cell`/`self_tag` default-bind per row: `traversed` forces each `.map` after the
                    # genexpr frame is exhausted, so a free closure would read only the last operand.
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

        return boundary("quantity.correlated", _build).bind(lambda outcome: outcome)

    def convert(self, target_unit: str, /) -> "RuntimeRail[UncertainQuantity]":
        # pint owns the unit algebra (affine offset units included) and the correlation graph through `Measurement.to`; a zero-nominal
        # value with finite uncertainty converts without a value-ratio division, and pint's own `DimensionalityError` converts once.
        def _to() -> "RuntimeRail[UncertainQuantity]":
            converted = self.measurement.to(target_unit)
            cell = converted.magnitude
            return _scalar_key(cell.nominal_value, cell.std_dev, target_unit).map(
                lambda key: UncertainQuantity(converted, self.magnitude.reseat(cell), key)
            )

        return boundary("quantity.convert", _to).bind(lambda outcome: outcome)

    def propagate(self, propagation: Propagation, unit: str, /, *operands: "UncertainQuantity") -> "RuntimeRail[UncertainQuantity]":
        # `*operands` make the catalogued arity reachable rather than stranded behind a unary signature.
        def _build() -> "RuntimeRail[UncertainQuantity]":
            supplied = 1 + len(operands)
            if propagation.arity >= 0 and supplied != propagation.arity:
                return Error(BoundaryFault(boundary=(f"quantity.propagate.{propagation.label}", f"arity {propagation.arity} != {supplied}")))
            mag = self.magnitude.join(tuple(o.magnitude for o in operands), propagation.apply)
            out = mag.cell
            return _propagated_key(propagation, unit, (self, *operands)).map(
                lambda key: UncertainQuantity(_UREG.Measurement(out.nominal_value, out.std_dev, unit), mag, key)
            )

        return boundary(f"quantity.propagate.{propagation.tag}", _build).bind(lambda outcome: outcome)

    def claim(self) -> QuantityReceipt:
        # `consistent` is the FENCED base-unit reduction read as a bool — `to_base_units` preserves dimensionality by construction,
        # so the dimensionality comparison is tautologically `True`; an offset unit (`degC`/`degF`) raising under the multiplicative
        # registry folds to a falsifiable `False`, and `claim` stays total rather than raising the offset-unit fault out of domain code.
        cell = self.magnitude.cell
        rel = float(abs(cell.std_dev / cell.nominal_value)) if cell.nominal_value else (0.0 if cell.std_dev == 0.0 else float("inf"))
        dim = dict(self.measurement.units.dimensionality)
        return QuantityReceipt(
            unit_expr=f"{self.measurement.units:~}",
            dimensionality=str(dim),
            nominal=float(cell.nominal_value),
            std_dev=float(cell.std_dev),
            rel_error=rel,
            consistent=boundary("quantity.consistent", self.measurement.to_base_units).is_ok(),
            mode=self.magnitude.tag,
            correlated_with=self.magnitude.peers,
            components=tuple((var.tag or repr(var), float(err)) for var, err in cell.error_components().items()),
            content_key=self.content_key,
        )


# --- [OPERATIONS] --------------------------------------------------------------------------


def cohort(quantities: Sequence[UncertainQuantity], view: CohortView, /) -> "RuntimeRail[np.ndarray]":
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
                # uncertainty-propagating inverse keeps linear-system provenance inside the propagation graph, never a bare numpy
                # solve; the square side is `isqrt`-guarded — a silent `round` would mis-shape the matrix.
                side = isqrt(len(cells))
                if side * side != len(cells):
                    raise ValueError(f"non-square cohort: {len(cells)} cells admit no square matrix")
                mat = unumpy.umatrix([c.nominal_value for c in cells], [c.std_dev for c in cells]).reshape(side, side)
                solved = ulinalg.inv(mat) if view is CohortView.INVERSE else ulinalg.pinv(mat)
                return np.stack([unumpy.nominal_values(solved), unumpy.std_devs(solved)])
            case _ as unreachable:
                assert_never(unreachable)

    return boundary(f"quantity.cohort.{view.value}", _read)


def _scalar_key(nominal: float, std_dev: float, unit: str, /) -> "RuntimeRail[ContentKey]":
    buffer = np.ascontiguousarray([nominal, std_dev], dtype=np.float64).tobytes() + unit.encode()
    return ContentIdentity.of("quantity", buffer)


def _cohort_key(nominals: Sequence[float], covariance: Covariance, unit: str, tags: Sequence[str], /) -> "RuntimeRail[ContentKey]":
    buffer = np.ascontiguousarray(list(nominals), dtype=np.float64).tobytes() + covariance.canonical() + unit.encode() + "\x00".join(tags).encode()
    return ContentIdentity.of("quantity.cohort", buffer)


def _member_key(cohort: ContentKey, member_tag: str, /) -> "RuntimeRail[ContentKey]":
    # identical cohort data yields an identical cohort key yields identical per-member keys.
    return ContentIdentity.of("quantity.member", cohort.memory + b"\x00" + member_tag.encode())


def _propagated_key(propagation: Propagation, unit: str, operands: tuple["UncertainQuantity", ...], /) -> "RuntimeRail[ContentKey]":
    # a propagated value is a NEW value, so it re-keys over the function label, the result unit, and each
    # operand's LE-span key bytes; an identical propagation over identical operands keys identically.
    buffer = propagation.label.encode() + b"\x00" + unit.encode() + b"".join(o.content_key.memory for o in operands)
    return ContentIdentity.of(f"quantity.propagate.{propagation.tag}", buffer)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
