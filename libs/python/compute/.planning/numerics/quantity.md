# [PY_COMPUTE_QUANTITY]

The one unit-bearing uncertain-quantity owner. `UncertainQuantity` threads a correlated `uncertainties.UFloat` magnitude through the pint unit algebra via the native `pint.Measurement` bridge, so unit conversion and first-order error propagation compose on one value, key through the runtime `ContentIdentity`, and graduate as one dimensional-consistency claim.

Four orthogonal axes carry the variation no single owner should fragment. `Magnitude` is the scalar/correlated discriminant whose structural tag IS the propagation mode, owning the `reseat`/`join` fold pair that re-mints the cell under an externally produced cell or an n-ary arity-aware map while preserving tag and unioning peer dependence — `join` subsumes the unary linear map, so no separate `through` arm stands beside it. `Covariance` is the cohort-construction policy folding a full matrix or a standard-deviation-and-correlation pair through `correlated_values`/`correlated_values_norm`. `Propagation` is the propagation-algebra policy folding the operand cells through a named arity-aware `umath` elementary function, an arbitrary `uncertainties.wrap`-lifted callable, or a callable with registered analytic partials. `CohortView` is the cohort-provenance policy folding one rail over `covariance_matrix`/`correlation_matrix`, the `unumpy.uarray` packed array, and the `unumpy.umatrix`/`ulinalg` uncertainty-propagating matrix inverse and pseudo-inverse.

Every transform is one fold arm and every elementary function is one `Umath` SmartEnum member carrying its own `arity` rather than a per-method rebuild, a parallel arity table, or a parallel surface. The unit vocabulary is one frozen application registry shared across every module through `pint.get_application_registry`, never a per-call instance whose `Quantity` types fail to interoperate. pint and uncertainties are both cp315-clean, so this owner is reflected; linear first-order propagation is the boundary, and a large-uncertainty regime routes to the study Monte-Carlo sampler.

## [01]-[INDEX]

- [01]-[QUANTITY]: correlated first-order uncertainty threaded through the pint unit algebra on one `UncertainQuantity` owner — every transform a `Magnitude.reseat`/`join` fold, every cohort construction a `Covariance` policy, every propagation an arity-aware `Propagation` policy over the `Umath` SmartEnum's per-member arity, every provenance read a `CohortView` row spanning the scalar-stat and `unumpy` matrix-linalg paths, every quantity content-keyed through `ContentIdentity` and graduating its dimensional-consistency claim through the `handoff.md` `unit_law` axis.

## [02]-[QUANTITY]

- Owner: `UncertainQuantity` — the ONE quantity-evidence owner threading correlated uncertainty through the pint unit algebra; a `pint.Measurement` (value-and-error `Quantity` over the shared `UnitRegistry`) carries the unit dimension and the `uncertainties.UFloat` magnitude in one object plus the railed `ContentKey`, so unit conversion, first-order propagation, content identity, and graduation compose on the same value. `Magnitude` is the single scalar/correlated discriminant and the only place propagation mode is recorded — the structural tag IS the mode, so no parallel `PropagationMode` field shadows it; `Covariance`, `Propagation`, and `CohortView` are the three closed policies the owner folds the cell through. No parallel uncertain type stands beside the unit-bearing one.
- Fold collapse: `Magnitude.reseat(cell)` and `Magnitude.join(others, apply)` are the two rebuild folds over the one tag algebra — `reseat` swaps in an externally produced cell and keeps the tag (scalar stays scalar, correlated keeps its `peers`), and `join` folds an arity-aware map over `(self, *others)` cells into one result whose peer set is the dedup-stable union of every operand's peers and whose tag is correlated when any operand is. `join` subsumes a unary `through`: a scalar with no peers stays scalar, a correlated cell keeps its own peers, so the unary linear map and the n-ary propagation are one fold rather than two near-identical `Scalar`/`Correlated`-rebuild branches. `convert` is one `reseat` of the pint-produced cell, and `propagate` is one `join` so a binary function lands on the correct joint correlation graph. The `cell` and `peers` projections read the tag's two facts each as one total match closed by `assert_never`, never a `case _` wildcard, and `reseat` reads `peers` once rather than re-matching the correlated payload.
- Propagation algebra: `Propagation` collapses the propagation algebra into one policy union — `Named(fn)` carries a bounded `Umath` `StrEnum` of the catalogued `uncertainties.umath` functions, `Wrapped(fn)` lifts an arbitrary callable through `wrap` (numerically differentiated), and `Analytic(fn, partials)` registers analytic partials through `wrap(f, derivatives_args=...)`. `Umath` is one SmartEnum-with-data: each member carries `(value, arity)` so the catalogued operand count rides the vocabulary itself rather than a parallel `FrozenDict` table, and `Propagation.arity` reads `self.named.arity`. `Propagation.lifted()` resolves the case to its `Callable[..., UFloat]` through one total `match`/`assert_never`, `Propagation.label` projects the stable identity (`Umath.value` or the callable `__qualname__`) the propagated content key seeds over, and `Propagation.apply(*cells)` threads the resolved callable across the operand cells so a binary `atan2`/`hypot`/`pow` propagates the joint two-cell chain-rule gradient. `propagate(propagation, unit, *operands)` supplies the extra operand quantities a binary or multi-argument function needs, and gates the supplied count against `Propagation.arity` BEFORE the lifted call so a `Named(ATAN2)` driven at one operand is a typed `boundary` arity reject rather than the opaque `TypeError` a stranded `arity` property would let through — the catalogued arity is consumed dispatch, never inert design pressure.
- Cohort construction: `Covariance` discriminates the construction matrix — `Full(matrix)` through `uncertainties.correlated_values` and `Norm(std_devs, correlation)` through `correlated_values_norm` — folded by `Covariance.reconstruct(nominals, tags)` into the correlated `UFloat` cohort through one total `match`/`assert_never`, so the construction matrix is one policy value the `correlated` entry consumes rather than a second classmethod. `Covariance.canonical()` is the second total fold: the whole construction payload as native `float64` bytes the cohort content key seeds over, and the `norm` arm folds BOTH the std-dev vector AND the correlation block so two cohorts differing only in `std_devs` key distinctly. Admission is symmetric with the `CORRELATION` provenance view: a cohort built from a correlation matrix reads back as one.
- Registry: `_UREG` is the one frozen module handle bound from `pint.get_application_registry()` (seeded once with `pint.set_application_registry(pint.UnitRegistry())` at import so the process owns a definite vocabulary before any module pulls the handle). It is read-only at the owner — no `define`, no per-call `UnitRegistry()`, no mutation — so every `Quantity`/`Measurement` this owner mints shares one unit vocabulary and stays arithmetically compatible across `array`, `interval`, study results, and the graduation rail. A second registry beside this handle is the deleted form.
- Entry: `UncertainQuantity.of` admits a nominal, standard deviation, and unit into a `pint.Measurement` and `bind`s the railed `ContentIdentity.of` content key over the canonical `(nominal, std_dev, unit)` buffer. `correlated` admits a cohort discriminated by its `Covariance` value on one entry — the construction matrix never a second method — keying the whole cohort through one `ContentIdentity.of` over the canonical nominals/covariance/unit/tags buffer so a repeated cohort on identical data is a cache hit by reference, then re-keying each member through `_member_key` over the cohort key bytes plus its own unique tag (the member rails joined through one `traversed` ABORT fold) so two cohort siblings never share one `content_key` — a shared cohort key would collide them as cache keys and as `_propagated_key` operand bytes, returning a stale propagation for a different operand. `convert` runs the pint conversion inside the boundary thunk so pint's own `DimensionalityError` converts once through `boundary` (no hand-minted dimensionality fault, no pre-check branch), reseats the `Magnitude` onto the `UFloat` pint's `Measurement.to` already produced — so the unit algebra (an affine offset-unit conversion a pure multiplicative ratio would corrupt included) and the correlation graph stay owned by pint/`uncertainties`, a zero-nominal value with finite uncertainty converts without a value-ratio division — and re-keys over the converted `(nominal, std_dev, target_unit)` so a unit change is a distinct content identity rather than a source-key cache collision. `propagate(propagation, unit, *operands)` gates the supplied operand count against `Propagation.arity` (a `Named` mismatch short-circuits to a typed `Error(BoundaryFault(boundary=...))` on the rail, a variadic `Wrapped`/`Analytic` clears), then folds the `Propagation` policy across the operand cells through `Magnitude.join` with the standard deviation flowing by the chain rule, and re-keys the new value over `Propagation.label`, the result unit, and each operand's content-key bytes. `claim` projects the unit dimension, nominal, standard deviation, relative error, the dimensional-consistency verdict, the per-source standard-deviation contribution from `UFloat.error_components`, and correlation provenance into a `QuantityReceipt`, the relative error `0.0` for an exact zero (zero nominal and zero std) and `inf` only when a zero nominal carries genuine spread.
- Provenance: `cohort` is the one cohort-provenance entry — it discriminates a `CohortView` request and folds one rail over a correlated cohort, never four sibling provenance functions: `COVARIANCE`/`CORRELATION` read `uncertainties.covariance_matrix`/`correlation_matrix`, `PACKED` reads the `unumpy.uarray`/`nominal_values`/`std_devs` stacked nominal-and-error array, and `INVERSE`/`PSEUDOINVERSE` lift the cohort into a square `unumpy.umatrix` and read back the uncertainty-propagating `ulinalg.inv`/`ulinalg.pinv` so a linear-system provenance over a correlated cohort stays inside the propagation graph rather than dropping to a bare numpy solve. The square side is `math.isqrt(len(cells))` guarded against a non-square cohort — a count admitting no square matrix raises inside the `boundary` thunk and converts to a `BoundaryFault` exactly once — never a silent `int(round(... ** 0.5))` that mis-shapes the matrix. Each view returns `RuntimeRail[np.ndarray]` through one `boundary` fence.
- Receipt: `QuantityReceipt.contribute` returns the one-element `tuple[Receipt, ...]` the runtime `ReceiptContributor` port streams — `Receipt.of("compute.quantity", ("emitted", self.unit_expr, facts))` against the runtime two-argument `of(owner, evidence)` contract over the `(Phase, subject, facts)` triple, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes and never a single-`Receipt` return against the `Iterable[Receipt]` port. The `facts` map folds the dimensionality, nominal, standard deviation, relative error, mode, correlation peers, dimensional-consistency flag, content key, and the per-source `error_components` contributions as native scalars the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `repr`/`f""` coerce — the components ride as a native `tuple[tuple[str, float], ...]` the deterministic encoder lowers, never a pre-joined `";".join(f"{t}={v:.3e}")` string. `QuantityReceipt.graduate` projects the dimensional-consistency claim onto the `graduation/handoff.md#GRADUATION` `unit_law` axis: a quantity whose dimensional reduction is consistent under the unit algebra crosses as policy evidence, an inconsistent reduction is an admission rejection the gate reads.
- Packages: `pint` (`get_application_registry`, `set_application_registry`, `UnitRegistry`, `Quantity`, `Measurement`, `Quantity.to`, `Quantity.to_base_units`, `Quantity.magnitude`, `Quantity.units`, `Quantity.dimensionality`, `DimensionalityError`), `uncertainties` (`ufloat`, `correlated_values`, `correlated_values_norm`, `covariance_matrix`, `correlation_matrix`, `wrap`, `umath`, `UFloat.nominal_value`, `UFloat.std_dev`, `UFloat.error_components`, `unumpy.uarray`, `unumpy.umatrix`, `unumpy.nominal_values`, `unumpy.std_devs`, and the `uncertainties.unumpy.ulinalg` submodule's `ulinalg.inv`/`ulinalg.pinv`), `numpy` (`asarray`, `ndarray`, `stack`, `ascontiguousarray`), `expression` (`tag`/`case`/`tagged_union` the four discriminated unions, `Result.bind`/`Result.map` the rail join, `Error` the typed arity-gate short-circuit, `Block`/`Block.of_seq` the per-member rail carrier the cohort fold threads), `msgspec` (`Struct` the receipt carrier, `gc=False` on the container-free leaf), stdlib (`math.isqrt` the square-cohort side, `enum.StrEnum` the `Umath` SmartEnum-with-data whose `__new__` binds the per-member `arity`), `numerics/array.md#PAYLOAD` (a quantity cohort whose data admits as an `ArrayPayload` keys under the same `ContentIdentity.of` seed), `graduation/handoff.md#GRADUATION` (`HandoffAxis` the `unit_law` graduation case the dimensional-consistency claim crosses), runtime (`RuntimeRail`, `boundary`, `traversed` the cohort per-member rail join under `Disposition.ABORT`, `BoundaryFault` the arity-reject fault, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new quantity family is one registry unit string; a new elementary propagation function is one `Umath` member carrying its `(value, arity)` the `propagate` arity gate consumes for free; a new propagation algebra is one `Propagation` case plus its `lifted`/`label` arms; a new cohort-construction matrix is one `Covariance` case plus its `reconstruct` AND `canonical` arms (the second so the new payload participates in the content key); a new cohort-provenance view is one `CohortView` row plus its fold arm; zero new surface, never a per-view provenance function, never a parallel arity table, never a unary/binary propagation method pair, never a per-construction classmethod.
- Boundary: no C# quantity or unit owner minting and no production error policy; offline propagation evidence only. Linear first-order propagation is the boundary — a second-order or Monte-Carlo error model routes to the `experiments/study.md#STUDY` sampler, never a parallel surrogate. The deleted forms: a hand-rolled unit-conversion table, a multiplicative unit-ratio scaling of the cell beside pint's own `Measurement.to` (which corrupts an affine offset-unit conversion), a parallel uncertain-magnitude type beside the unit-bearing one, a `PropagationMode` field shadowing the `Magnitude` tag, a free-form propagation-function string beside the bounded `Umath` vocabulary, a parallel `_UMATH_ARITY` `FrozenDict` beside the `Umath` SmartEnum that already carries each member's arity, an inert `Propagation.arity` property no entry consumes (so a `Named(ATAN2)` driven at one operand crashes with an opaque `TypeError` inside `boundary` rather than the typed arity reject the gate mints), a unary-only `propagate` signature that strands the catalogued binary `atan2`/`hypot`/`pow` functions where `*operands` and `Magnitude.join` reach them, a `_cohort_key` reading only the `norm` arm's correlation block (`covariance.norm[1]`) and dropping the std-dev vector so two cohorts differing only in `std_devs` collide on one key where `Covariance.canonical()` folds the whole construction payload, a separate unary `Magnitude.through` beside `join` where the n-ary fold already subsumes it, sibling per-view provenance functions, a bare numpy `linalg.inv` over a correlated cohort that drops the propagation graph the `ulinalg` path keeps, an `int(round(len(cells) ** 0.5))` side that silently mis-shapes a non-square cohort where `math.isqrt` guards it, an uncorrelated re-derivation of a propagated variance, a `convert`/`propagate` reusing the source `content_key` across a value change where the converted/propagated value re-keys, a quantity with no `content_key` where the sibling owners key by `ContentIdentity`, a cohort assigning every member the one cohort key so two siblings collide as cache and `_propagated_key` operand keys where `_member_key` folds the member's unique tag, a bare-`ContentKey` assignment off the railed `ContentIdentity.of` that drops the canonical-encode fault, a four-positional `Receipt.of` against the two-argument contract, a single-`Receipt` `contribute` against the `Iterable[Receipt]` port, and an `f""`/`repr`-pre-coerced facts map where the renderer carries native scalars. pint and uncertainties are both cp315-clean, so the owner is reflected.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Sequence
from enum import StrEnum
from math import isqrt
from typing import Literal, assert_never

import numpy as np
import pint
from expression import Block, Error, case, tag, tagged_union
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
from uncertainties.unumpy import ulinalg

from rasm.compute.graduation.handoff import HandoffAxis
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, traversed
from rasm.runtime.receipts import Receipt

pint.set_application_registry(pint.UnitRegistry())
_UREG: pint.UnitRegistry = pint.get_application_registry()


# --- [TYPES] -------------------------------------------------------------------------------

class Umath(StrEnum):
    # one SmartEnum-with-data: the `str` value is the `uncertainties.umath` attribute name and `arity`
    # rides each member, collapsing the parallel arity table. `Propagation.lifted` reads `self.value`,
    # `Propagation.arity` reads `self.arity`; a new function is one member carrying its operand count.
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

    def reseat(self, cell: UFloat, /) -> "Magnitude":
        # one tag-preserving swap reading `peers` once rather than re-matching the correlated payload.
        return Magnitude.Correlated(cell, self.peers) if self.tag == "correlated" else Magnitude.Scalar(cell)

    def join(self, others: "tuple[Magnitude, ...]", apply: Callable[..., UFloat], /) -> "Magnitude":
        # the one propagation fold over (self, *others): an arity-aware map (a unary `sqrt`, a binary
        # `atan2`/`hypot`/`pow`, a multi-argument `Wrapped`) lands one `UFloat` whose `uncertainties`
        # derivatives graph already carries the joint chain-rule gradient against all inputs, so the
        # result is correlated with the dedup-stable union of every operand's peers, scalar only when
        # none carries peers. Subsumes a unary `through` — a scalar with no peers stays scalar.
        cohort = (self, *others)
        cell = apply(*(m.cell for m in cohort))
        peers = tuple(dict.fromkeys(p for m in cohort for p in m.peers))
        return Magnitude.Correlated(cell, peers) if peers else Magnitude.Scalar(cell)


@tagged_union(frozen=True)
class Covariance:
    tag: Literal["full", "norm"] = tag()
    full: Sequence[Sequence[float]] = case()
    norm: tuple[Sequence[float], Sequence[Sequence[float]]] = case()

    @staticmethod
    def Full(matrix: Sequence[Sequence[float]]) -> "Covariance":
        return Covariance(full=matrix)

    @staticmethod
    def Norm(std_devs: Sequence[float], correlation: Sequence[Sequence[float]]) -> "Covariance":
        return Covariance(norm=(std_devs, correlation))

    def reconstruct(self, nominals: Sequence[float], tags: Sequence[str], /) -> Sequence[UFloat]:
        # one cohort-construction fold: a full covariance matrix or a (std, correlation) pair rebuilds
        # the correlated `UFloat` cohort through the polymorphic inverse of `covariance_matrix`.
        match self:
            case Covariance(tag="full", full=matrix):
                return correlated_values(list(nominals), [list(r) for r in matrix], tags=list(tags))
            case Covariance(tag="norm", norm=(stds, corr)):
                return correlated_values_norm(
                    list(zip(nominals, stds, strict=True)), [list(r) for r in corr], tags=list(tags)
                )
            case unreachable:
                assert_never(unreachable)

    def canonical(self) -> bytes:
        # the whole construction-matrix payload as native `float64` bytes the content key seeds over:
        # the `norm` arm folds BOTH the std-dev vector AND the correlation block so two cohorts differing
        # only in `std_devs` key distinctly, never the prior `norm[1]`-only buffer that collided them.
        match self:
            case Covariance(tag="full", full=matrix):
                return b"full" + np.ascontiguousarray([list(r) for r in matrix], dtype=np.float64).tobytes()
            case Covariance(tag="norm", norm=(stds, corr)):
                return (
                    b"norm"
                    + np.ascontiguousarray(list(stds), dtype=np.float64).tobytes()
                    + np.ascontiguousarray([list(r) for r in corr], dtype=np.float64).tobytes()
                )
            case unreachable:
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
        # a named function reads its catalogued arity off the `Umath` member; a lifted callable is
        # applied at whatever arity the caller supplies, so the operand count is unconstrained (`-1`).
        return self.named.arity if self.tag == "named" else -1

    @property
    def label(self) -> str:
        # the stable function identity `_propagated_key` seeds over: the `Umath` value or the lifted
        # callable's qualified name, one total match so a new case forces its label arm.
        match self:
            case Propagation(tag="named", named=fn):
                return fn.value
            case Propagation(tag="wrapped", wrapped=fn):
                return fn.__qualname__
            case Propagation(tag="analytic", analytic=(fn, _)):
                return fn.__qualname__
            case unreachable:
                assert_never(unreachable)

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

    def apply(self, *cells: UFloat) -> UFloat:
        # threads the resolved callable over the operand cells so a binary `atan2`/`hypot`/`pow`
        # propagates the joint two-cell chain-rule gradient, unreachable behind a unary-only signature.
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

    def graduate(self) -> HandoffAxis:
        # the `handoff.md` `unit_law` axis carries the dimensional-consistency subject as policy
        # evidence; a consistent reduction crosses, an inconsistent one is an admission rejection.
        return HandoffAxis(unit_law=self.unit_expr if self.consistent else f"inconsistent:{self.unit_expr}")

    def contribute(self) -> tuple[Receipt, ...]:
        # the runtime two-argument `Receipt.of(owner, evidence)` contract: the `(Phase, subject, facts)`
        # triple mints `fact` at `emitted`. Native scalars (the float fields, the `components` tuple,
        # the `ContentKey`) ride the `EventDict` `dict[str, object]` slots the `enc_hook=repr` renderer
        # serializes without a `repr`/`f""` coerce.
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
        return (Receipt.of("compute.quantity", ("emitted", self.unit_expr, facts)),)


class UncertainQuantity(Struct, frozen=True):
    measurement: pint.Measurement
    magnitude: Magnitude
    content_key: ContentKey

    @classmethod
    def of(cls, nominal: float, std_dev: float, unit: str, /) -> "RuntimeRail[UncertainQuantity]":
        # the railed `ContentIdentity.of` key threads through `.map` so a digest fault propagates on the
        # one rail; `boundary` fences the pint `Measurement` build, joined under `.bind` (the sibling shape).
        def _build() -> "RuntimeRail[UncertainQuantity]":
            cell = ufloat(nominal, std_dev)
            measurement = _UREG.Measurement(nominal, std_dev, unit)
            return _scalar_key(nominal, std_dev, unit).map(
                lambda key: cls(measurement, Magnitude.Scalar(cell), key)
            )

        return boundary("quantity.of", _build).bind(lambda outcome: outcome)

    @classmethod
    def correlated(
        cls, nominals: Sequence[float], covariance: Covariance, unit: str, tags: tuple[str, ...], /
    ) -> "RuntimeRail[tuple[UncertainQuantity, ...]]":
        # one cohort keyed through one `ContentIdentity.of` over the canonical nominals/covariance/unit/tags
        # buffer, so a repeated cohort on identical data is a cache hit by reference; `Covariance.reconstruct`
        # owns the construction matrix and `Magnitude.reseat`'s correlated arm keeps each cell's peer tuple.
        # Each member re-keys through `_member_key` over the cohort key bytes plus its own (unique) tag, so
        # two cohort siblings never share one `content_key` (which would collide them as cache/operand keys).
        def _build() -> "RuntimeRail[tuple[UncertainQuantity, ...]]":
            cells = covariance.reconstruct(nominals, tags)
            return _cohort_key(nominals, covariance, unit, tags).bind(
                lambda cohort: traversed(
                    Block.of_seq(
                        _member_key(cohort, self_tag).map(
                            lambda key: cls(
                                _UREG.Measurement(cell.nominal_value, cell.std_dev, unit),
                                Magnitude.Correlated(cell, tuple(t for t in tags if t != self_tag)),
                                key,
                            )
                        )
                        for cell, self_tag in zip(cells, tags, strict=True)
                    )
                ).map(tuple)
            )

        return boundary("quantity.correlated", _build).bind(lambda outcome: outcome)

    def convert(self, target_unit: str, /) -> "RuntimeRail[UncertainQuantity]":
        # pint owns the unit algebra (affine offset units included) and the correlation graph through
        # `Measurement.to`; `reseat` keeps the cell's tag/peers. The converted value re-keys over its
        # target unit so a unit change is a distinct content identity, not a source-key cache collision.
        def _to() -> "RuntimeRail[UncertainQuantity]":
            converted = self.measurement.to(target_unit)
            cell = converted.magnitude
            return _scalar_key(cell.nominal_value, cell.std_dev, target_unit).map(
                lambda key: UncertainQuantity(converted, self.magnitude.reseat(cell), key)
            )

        return boundary("quantity.convert", _to).bind(lambda outcome: outcome)

    def propagate(
        self, propagation: Propagation, unit: str, /, *operands: "UncertainQuantity"
    ) -> "RuntimeRail[UncertainQuantity]":
        # `*operands` carry the extra cells a binary `atan2`/`hypot`/`pow` (or a multi-argument `Wrapped`)
        # needs, so the catalogued arity is reachable rather than stranded behind a unary signature.
        # The `Propagation.arity` gate rejects a `Named` count mismatch as a typed `boundary` fault BEFORE
        # the lifted call, so `atan2` with one operand is a named arity reject, not an opaque `TypeError`;
        # a `Wrapped`/`Analytic` (`arity == -1`) is variadic and clears unconditionally. `Magnitude.join`
        # then folds every operand cell through one `Propagation.apply`, unioning the peer graph; the
        # result re-keys through the railed `ContentIdentity.of` because it is a new value.
        def _build() -> "RuntimeRail[UncertainQuantity]":
            supplied = 1 + len(operands)
            if propagation.arity >= 0 and supplied != propagation.arity:
                return Error(BoundaryFault(boundary=(
                    f"quantity.propagate.{propagation.label}", f"arity {propagation.arity} != {supplied}"
                )))
            mag = self.magnitude.join(tuple(o.magnitude for o in operands), propagation.apply)
            out = mag.cell
            return _propagated_key(propagation, unit, (self, *operands)).map(
                lambda key: UncertainQuantity(_UREG.Measurement(out.nominal_value, out.std_dev, unit), mag, key)
            )

        return boundary(f"quantity.propagate.{propagation.tag}", _build).bind(lambda outcome: outcome)

    def claim(self) -> QuantityReceipt:
        cell = self.magnitude.cell
        rel = (
            float(abs(cell.std_dev / cell.nominal_value))
            if cell.nominal_value
            else (0.0 if cell.std_dev == 0.0 else float("inf"))
        )
        dim = dict(self.measurement.units.dimensionality)
        return QuantityReceipt(
            unit_expr=f"{self.measurement.units:~}",
            dimensionality=str(dim),
            nominal=float(cell.nominal_value),
            std_dev=float(cell.std_dev),
            rel_error=rel,
            consistent=self.measurement.to_base_units().units.dimensionality == self.measurement.units.dimensionality,
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
                # the cohort lifts into a square uncertain matrix and reads back the uncertainty-propagating
                # inverse/pseudo-inverse, so a linear-system provenance stays inside the propagation graph.
                # the square side is `isqrt` of the count, guarded against a non-square cohort rather than a
                # silent `round` that mis-shapes; `np.reshape` lays the cells out row-major.
                side = isqrt(len(cells))
                if side * side != len(cells):
                    raise ValueError(f"non-square cohort: {len(cells)} cells admit no square matrix")
                mat = unumpy.umatrix([c.nominal_value for c in cells], [c.std_dev for c in cells]).reshape(side, side)
                solved = ulinalg.inv(mat) if view is CohortView.INVERSE else ulinalg.pinv(mat)
                return np.stack([unumpy.nominal_values(solved), unumpy.std_devs(solved)])
            case unreachable:
                assert_never(unreachable)

    return boundary(f"quantity.cohort.{view.value}", _read)


def _scalar_key(nominal: float, std_dev: float, unit: str, /) -> "RuntimeRail[ContentKey]":
    buffer = np.ascontiguousarray([nominal, std_dev], dtype=np.float64).tobytes() + unit.encode()
    return ContentIdentity.of("quantity", buffer, IdentityPolicy())


def _cohort_key(
    nominals: Sequence[float], covariance: Covariance, unit: str, tags: Sequence[str], /
) -> "RuntimeRail[ContentKey]":
    buffer = (
        np.ascontiguousarray(list(nominals), dtype=np.float64).tobytes()
        + covariance.canonical()
        + unit.encode()
        + "\x00".join(tags).encode()
    )
    return ContentIdentity.of("quantity.cohort", buffer, IdentityPolicy())


def _member_key(cohort: ContentKey, member_tag: str, /) -> "RuntimeRail[ContentKey]":
    # each cohort member re-keys over the cohort key's LE bytes plus its own unique tag, so siblings of one
    # cohort key distinctly (a shared cohort key would collide them as cache and `_propagated_key` operand
    # keys); identical cohort data yields an identical cohort key yields identical per-member keys.
    return ContentIdentity.of("quantity.member", cohort.memory + b"\x00" + member_tag.encode(), IdentityPolicy())


def _propagated_key(propagation: Propagation, unit: str, operands: tuple["UncertainQuantity", ...], /) -> "RuntimeRail[ContentKey]":
    # a propagated value is a NEW value, so it re-keys over the function label, the result unit, and each
    # operand's LE-span key bytes; an identical propagation over identical operands keys identically.
    buffer = propagation.label.encode() + b"\x00" + unit.encode() + b"".join(o.content_key.memory for o in operands)
    return ContentIdentity.of(f"quantity.propagate.{propagation.tag}", buffer, IdentityPolicy())
```

## [03]-[RESEARCH]

- [PINT_MEASUREMENT]: `pint` and `uncertainties` are cp315-clean and reflected; the `pint.Measurement(value, error, units)` value-and-error bridge, `Quantity.to`/`.to_base_units`/`.dimensionality`/`.magnitude`/`.units`, `DimensionalityError`, and the `ufloat`/`ufloat_fromstr`/`correlated_values(nominals, covariance, tags=...)`/`correlated_values_norm(values_with_std_dev, correlation_mat, tags=...)`/`covariance_matrix`/`correlation_matrix`/`wrap(f, derivatives_args=...)`/`umath` spellings plus `UFloat.nominal_value`/`.std_dev`/`.std_score`/`.tag`/`.error_components()` and `unumpy.uarray`/`umatrix`/`nominal_values`/`std_devs`/`ulinalg.inv`/`ulinalg.pinv` verify against the `.api` catalogue (`.api/uncertainties.md` ENTRYPOINTS [03][01]-[09], `unumpy` [03][01]/[02]/[03]/[04]/[05]/[06], `UFloat` members [02][01]/[02]/[04]/[05]/[07], `unumpy.matrix` PUBLIC_TYPES [02][03]).
- [APPLICATION_REGISTRY]: `pint.get_application_registry()`/`pint.set_application_registry(registry)` are the module-level shared-registry entries (`.api/pint.md` ENTRYPOINTS [03][01]/[02], IMPLEMENTATION_LAW UNIT_TOPOLOGY and LOCAL_ADMISSION). One process-wide `UnitRegistry` is installed once and read through the frozen `_UREG` handle; the `.api` RAIL_LAW rejects per-claim registries because their `Quantity` types do not interoperate, so a `_UREG = pint.UnitRegistry()` per-module instance is the defect and the application-registry handle is the owned form.
- [MAGNITUDE_FOLD]: the scalar/correlated structural tag IS the propagation mode, so a parallel `PropagationMode` `StrEnum` field shadowing the `Magnitude` tag is a duplicate discriminant and the deleted form. `reseat` (an externally produced cell) and `join` (an arity-aware map over `(self, *others)` cells) are the two rebuild folds over the one tag, collapsing the near-identical `Scalar`/`Correlated`-rebuild-plus-peer-extract branches: `convert` reseats the pint-produced cell, and `propagate` folds through `join`, which lands the result on `Correlated` with the dedup-stable union of every operand's peers when any operand is correlated and on `Scalar` otherwise. `join` subsumes a unary `through` because a scalar with no peers stays scalar and a single correlated operand keeps its own peers, so one fold owns both the unary linear map and the n-ary propagation. The `uncertainties` `derivatives` graph already carries the joint chain-rule gradient against all input variables, so `join` reads the peer union for the receipt's `correlated_with` slot without re-deriving covariance. The `cell`/`peers` projections are total matches closed by `assert_never`, never a `case _` wildcard, and `reseat` reads the `peers` property once rather than re-matching the correlated payload. The receipt's `mode` reads the tag directly.
- [PROPAGATION_POLICY]: `Propagation` collapses the propagation algebra into one policy union — `Named` carries a bounded `Umath` `StrEnum` of the catalogued `uncertainties.umath` functions (`.api/uncertainties.md` `umath` [03][01]-[07]) resolved through `getattr(umath, fn.value)`, never a free-form string, `Wrapped` lifts an arbitrary callable through `wrap` (numerical differentiation), and `Analytic` registers analytic partials through `wrap(f, derivatives_args=...)` (`.api/uncertainties.md` ENTRYPOINTS [03][08]). `Umath` is a SmartEnum-with-data whose `__new__` binds each member's `(value, arity)`, so the catalogued operand count rides the vocabulary rather than a parallel `_UMATH_ARITY` `FrozenDict`: the catalogue documents `atan2`/`hypot`/`pow` as two-argument propagation ([03][03]/[03][04]), and `propagate(propagation, unit, *operands)` gates `1 + len(operands)` against `Propagation.arity` (a `Named` mismatch short-circuits to `Error(BoundaryFault(boundary=...))` on the rail, a variadic `Wrapped`/`Analytic` `arity == -1` clears) then threads those extra operand quantities through `Magnitude.join` into `Propagation.apply(*cells)` so a binary function propagates the joint two-cell chain-rule gradient rather than being unreachable behind a unary-only signature, and an arity-blind dispatch is the deleted form: the catalogued arity is consumed dispatch, never an inert property. `Propagation.label` projects the `Umath.value` or the lifted callable's `__qualname__` as the stable identity the propagated content key seeds over. `propagate` takes the policy value, never a bare callable.
- [COHORT_PROVENANCE]: `cohort` collapses any sibling `covariance_provenance`/`correlation_provenance` functions into one `CohortView`-discriminated entry over five views: `COVARIANCE`/`CORRELATION` read `covariance_matrix`/`correlation_matrix`, `PACKED` reads the `unumpy.uarray`/`nominal_values`/`std_devs` stacked nominal-and-error array, and `INVERSE`/`PSEUDOINVERSE` lift the cohort into a square `unumpy.umatrix` and read back the uncertainty-propagating `ulinalg.inv`/`ulinalg.pinv` (`.api/uncertainties.md` ENTRYPOINTS [03][02]/[03][05]/[03][06]), so the matrix-linalg provenance the catalogue admits stays inside the propagation graph rather than dropping to a bare `numpy.linalg.inv` that loses the correlation gradient. `Covariance.reconstruct` admits both a full covariance matrix and a `Norm` standard-deviation-and-correlation pair through `correlated_values_norm`, making cohort admission symmetric with the `CORRELATION` provenance view, and the matrix is one policy value the `correlated` entry consumes rather than a second classmethod.
- [DIMENSIONALITY_RAIL]: `convert` runs `self.measurement.to(target)` inside the `boundary` thunk, so pint's native `DimensionalityError` (`.api/pint.md` PUBLIC_TYPES [02][04]) converts to a `BoundaryFault` exactly once through the one conversion surface — a hand-minted `Error(BoundaryFault(boundary=("dimensionality", ...)))` pre-check both duplicates pint's own dimensionality test and mints the `boundary` case the conversion surface reserves, and is the deleted form. `convert` reseats the `Magnitude` onto `converted.magnitude` (`.api/pint.md` ENTRYPOINTS [03-convert][01], PUBLIC_TYPES [02][06] — pint's `Measurement` backs its magnitude with the `uncertainties` `UFloat`), so the conversion — affine offset units (`OffsetUnitCalculusError` domain, `.api/pint.md` PUBLIC_TYPES [02][05]) included — and the correlation graph stay owned by pint/`uncertainties`; a `_UREG.Quantity(1.0, units).to(target).magnitude` ratio scaling the cell by a single multiplicative factor silently corrupts an affine temperature conversion and re-derives a transform pint already performs, and is the deleted form. The converted value re-keys over its `(nominal, std_dev, target_unit)` through the railed `_scalar_key` joined under `.bind`, so a unit change is a distinct content identity rather than the source key reused across two dimensionally distinct values. A `0.0 ± σ` measurement converts through pint without a `converted/nominal` value ratio; `claim` separates an exact zero (`rel_error == 0.0`) from a zero-nominal value carrying genuine spread (`rel_error == inf`), reads the dimensional-consistency verdict off `to_base_units().units.dimensionality` against the declared dimensionality, and emits the per-source standard-deviation contribution through `UFloat.error_components()`.
- [CONTENT_KEY]: every quantity carries the railed `ContentKey` the runtime `ContentIdentity.of(fmt, source, policy) -> RuntimeRail[ContentKey]` mints, threaded through `Result.map` into the constructed quantity and joined under `.bind` exactly as `optimization/program.md#PROGRAM`/`numerics/array.md#PAYLOAD`/`experiments/inference.md#BAYESIAN` key their owners — never a bare-`ContentKey` assignment dropping the canonical-encode fault. A cohort re-keys each member through `_member_key` over the cohort key bytes plus the member's unique tag (the member rails joined under one `traversed` ABORT fold), so two siblings of one cohort key distinctly rather than sharing the cohort key — a shared per-member key would collide them as `execution/lanes#LANE` cache keys and fold identical bytes into `_propagated_key`, returning a stale propagation result for a distinct operand. `_scalar_key` seeds over the canonical `(nominal, std_dev)` contiguous `float64` buffer plus the unit string; `_cohort_key` over the concatenated nominals, the `Covariance.canonical()` construction payload (the full covariance matrix, or the `norm`-arm std-dev vector AND correlation block together so two cohorts differing only in `std_devs` key distinctly), the unit, and the NUL-joined tags; and `_propagated_key` over the `Propagation.label`, the result unit, and each operand's `ContentKey.memory` LE-span bytes, so a propagation or conversion that yields a new value gets a distinct identity while a repeat on identical inputs is a cache hit by reference. The buffer carries only msgspec-native bytes because the runtime `_ENCODER` carries no `enc_hook` and cannot serialize an `expression.@tagged_union` — the key seeds over each union's `canonical` byte projection, never the raw `Covariance`/`Magnitude` union — matching the `experiments/inference.md#BAYESIAN` rule that the content payload carries each union's native projection. A quantity cohort whose data admits through `numerics/array.md#PAYLOAD` keys under the same `ContentIdentity.of` seed algebra.
- [RECEIPT_GRADUATION]: `QuantityReceipt.contribute` mints through the runtime two-argument `Receipt.of(owner, evidence)` contract — `Receipt.of("compute.quantity", ("emitted", self.unit_expr, facts))`, the `(Phase, subject, facts)` triple the runtime factory discriminates — returning the one-element `tuple[Receipt, ...]` the `ReceiptContributor` port declares, never the four-positional `Receipt.of("emitted", owner, subject, facts)` form the runtime owner deletes and never a single-`Receipt` return, matching the sibling `numerics/statistics.md#STATISTICS` and `optimization/program.md#PROGRAM` contributors. The facts ride as native `float`/`bool`/`tuple`/`str` and the `ContentKey` through the runtime `Signals` `msgspec` `Encoder(enc_hook=repr, order="deterministic")` rather than a `repr(...)`/`f"{var:.3e}"`/`";".join(...)` pre-coerce; `QuantityReceipt` is `gc=False` because its fields are container-free scalars and tuples. `QuantityReceipt.graduate` projects the dimensional-consistency claim onto the `graduation/handoff.md#GRADUATION` `unit_law` `HandoffAxis` case (`handoff.md` `[02]` Unit-law row: the case carries "the `numerics/quantity.md#QUANTITY` pint unit-algebra dimensional-consistency subject"), so a quantity whose dimensional reduction is consistent under the unit algebra crosses as policy evidence and an inconsistent reduction is the admission rejection the gate reads — the forward consumer the prior receipt ignored. A quantity with no `content_key`, a four-positional `Receipt.of`, a single-`Receipt` `contribute`, and an `f""`/`repr`-pre-coerced facts map are the deleted forms.
- [MONTE_CARLO_BRIDGE]: the large-uncertainty regime routes to the GUM-supplement-1 Monte-Carlo path on the `experiments/study.md#STUDY` sampler rather than a parallel surrogate; `mcerp` Monte-Carlo and `soerp` second-order propagation are candidate study-method bridges verified against the `.api` catalogue once admitted.
