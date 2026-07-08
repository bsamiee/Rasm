"""Pure assertion oracles for algebraic, matrix, rail, and stateful laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import cmath
from collections.abc import Callable, Iterable, Mapping, Sequence  # noqa: TC003  # msgspec.Struct resolves annotations at runtime
from contextlib import nullcontext
import dataclasses
from decimal import Decimal
import fractions
import functools
import operator
from typing import overload, Protocol, runtime_checkable, Self, TYPE_CHECKING

from expression import Option, Result
from expression.collections import Block
from hypothesis import settings as hyp_settings, target
from hypothesis.stateful import (
    Bundle,
    consumes,
    initialize,
    invariant,
    multiple,
    precondition,
    rule,
    RuleBasedStateMachine,
    run_state_machine_as_test,
)
import msgspec
import msgspec.json
import msgspec.msgpack
lazy import numpy as np

from tests.python._testkit.runtime import PROFILE_MUTATION, PROFILE_STATEFUL


if TYPE_CHECKING:
    from contextlib import AbstractContextManager


# --- [TYPES] ----------------------------------------------------------------------------

type _Eq[T] = Callable[[T, T], bool] | None
type _Cmp[T] = Callable[[T, T], int] | None


class _Comparable(Protocol):
    """Structural bound for projection keys with total less-than ordering."""

    def __lt__(self, other: Self, /) -> bool: ...


class RowCarrier(Protocol):
    """Structural per-row failure scope; pytest's native ``subtests`` fixture satisfies it."""

    def test(self, msg: str | None = None, **kwargs: object) -> AbstractContextManager[object]: ...


type _Numeric = int | float | complex | Decimal | fractions.Fraction


@runtime_checkable
class _QuantityLike(Protocol):
    """Structural quantity shape: pint and peers carry ``units`` plus ``magnitude``."""

    @property
    def units(self) -> object: ...
    @property
    def magnitude(self) -> object: ...


# --- [MODELS] ---------------------------------------------------------------------------


class ValidityCase[T](msgspec.Struct, frozen=True, gc=False):
    """Case carrier for ``validity_matrix``.

    Args:
        label: Case identifier surfaced on failure.
        value: The subject value under test.
        expected: Expected predicate verdict.
    """

    label: str
    value: T
    expected: bool


class ProjectionCase[I](msgspec.Struct, frozen=True, gc=False):
    """Case carrier for ``projection_matrix``.

    Args:
        label: Case identifier surfaced on failure.
        intent: Input fed to ``project``.
        supported_out: Expected output when projection succeeds.
        oracle: Optional reference-output function; absent uses ``supported_out``.
        unsupported_out: Expected output when the projection is unsupported or falls back.
    """

    label: str
    intent: I
    supported_out: object
    oracle: Callable[[I], object] | None
    unsupported_out: object


class MetamorphicRelation[T, R](msgspec.Struct, frozen=True, gc=False):
    """Single metamorphic relation used by ``metamorphic_sweep``.

    ``transform`` derives a follow-up input from the source; ``relate`` asserts the required
    output relation and raises ``AssertionError`` on violation.

    Args:
        name: Relation identifier surfaced on failure.
        transform: Maps the source input to the follow-up input.
        relate: Assertion callback over source and follow-up outputs.
    """

    name: str
    transform: Callable[[T], T]
    relate: Callable[[R, R], None]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _eq_law[T](left: T, right: T, eq: _Eq[T]) -> None:
    """Assert structural or custom equality and surface both sides on failure."""
    assert (eq if eq is not None else operator.eq)(left, right), f"law violated: {left!r} != {right!r}"


# --- [TOLERANCE_ORACLES]


def _num_close(a: _Numeric, b: _Numeric, rel_tol: float, abs_tol: float) -> bool:
    # NaN pairs count as close: wire and solver laws compare degraded lanes, not IEEE ordering.
    # The equality fast-path also admits matching infinities, where the difference is NaN.
    fa, fb = complex(a), complex(b)
    if fa == fb or (cmath.isnan(fa) and cmath.isnan(fb)):
        return True
    return abs(fa - fb) <= max(rel_tol * max(abs(fa), abs(fb)), abs_tol)


def _rail_diverge(a: object, b: object, rel_tol: float, abs_tol: float, path: str) -> str | None:
    """Compare two rail carriers: matching tags recurse into the payload slot; tag drift reports the arm.

    Returns:
        Divergence at the payload's structural path, or ``None`` when the carriers are close.
    """
    match (a, b):
        case (Result(tag="ok", ok=left), Result(tag="ok", ok=right)):
            return _diverge(left, right, rel_tol, abs_tol, f"{path}.ok")
        case (Result(tag="error", error=left), Result(tag="error", error=right)):
            return _diverge(left, right, rel_tol, abs_tol, f"{path}.error")
        case (Option(tag="some", some=left), Option(tag="some", some=right)):
            return _diverge(left, right, rel_tol, abs_tol, f"{path}.some")
        case (Option(tag="none"), Option(tag="none")):
            return None
        case _:
            return f"{path}: rail tags differ: {a!r} != {b!r}"


def _diverge(a: object, b: object, rel_tol: float, abs_tol: float, path: str) -> str | None:  # noqa: PLR0911  # one arm per data shape; the closed dispatch owns every return
    """Locate the first tolerance divergence between two values.

    Returns:
        Human-readable divergence at its structural path, or ``None`` when the values are close.
    """
    match (a, b):
        case (bool(), _) | (_, bool()) | (str(), _) | (bytes(), _):
            # Kind parity first: True == 1 in Python, but bool and int are distinct wire facts.
            return None if isinstance(a, bool) == isinstance(b, bool) and a == b else f"{path}: {a!r} != {b!r}"
        case (np.ndarray() | np.generic(), _) | (_, np.ndarray() | np.generic()):
            left, right = np.asarray(a), np.asarray(b)
            if left.shape != right.shape:
                return f"{path}: shape {left.shape} != {right.shape}"
            near = np.atleast_1d(np.isclose(left, right, rtol=rel_tol, atol=abs_tol, equal_nan=True))
            if bool(near.all()):
                return None
            index = tuple(int(i) for i in np.argwhere(~near)[0])
            return f"{path}{list(index)}: {np.atleast_1d(left)[index]!r} !~ {np.atleast_1d(right)[index]!r}"
        case (
            (int() | float() | complex() | Decimal() | fractions.Fraction()) as num_a,
            (int() | float() | complex() | Decimal() | fractions.Fraction()) as num_b,
        ):
            return None if _num_close(num_a, num_b, rel_tol, abs_tol) else f"{path}: |{a!r} - {b!r}| exceeds rel_tol={rel_tol}, abs_tol={abs_tol}"
        case (_QuantityLike() as qty_a, _QuantityLike() as qty_b):
            # Quantity-shaped values (pint and peers): units match exactly, magnitudes compare recursively.
            if qty_b.units != qty_a.units:
                return f"{path}: units {qty_a.units!r} != {qty_b.units!r}"
            return _diverge(qty_a.magnitude, qty_b.magnitude, rel_tol, abs_tol, f"{path}.magnitude")
        case (Result(), Result()) | (Option(), Option()):
            return _rail_diverge(a, b, rel_tol, abs_tol, path)
        case (Block(), Block()):
            return _diverge(tuple(a), tuple(b), rel_tol, abs_tol, path)
        case (msgspec.Struct(), msgspec.Struct()) if type(a) is type(b):
            fields: tuple[str, ...] = a.__struct_fields__
            return next((d for f in fields if (d := _diverge(getattr(a, f), getattr(b, f), rel_tol, abs_tol, f"{path}.{f}")) is not None), None)
        case _ if dataclasses.is_dataclass(a) and not isinstance(a, type) and type(a) is type(b):
            names = (f.name for f in dataclasses.fields(a))
            return next((d for f in names if (d := _diverge(getattr(a, f), getattr(b, f), rel_tol, abs_tol, f"{path}.{f}")) is not None), None)
        case (Mapping(), Mapping()):
            lookup = dict(b.items())
            if set(a) != set(lookup):
                return f"{path}: key sets differ: {sorted(map(repr, set(a) ^ set(lookup)))}"
            return next((d for k, v in a.items() if (d := _diverge(v, lookup[k], rel_tol, abs_tol, f"{path}[{k!r}]")) is not None), None)
        case (Sequence(), Sequence()):
            if len(a) != len(b):
                return f"{path}: length {len(a)} != {len(b)}"
            pairs = zip(a, b, strict=True)
            return next((d for i, (x, y) in enumerate(pairs) if (d := _diverge(x, y, rel_tol, abs_tol, f"{path}[{i}]")) is not None), None)
        case _:
            return None if a == b else f"{path}: {a!r} != {b!r}"


def close(*, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> Callable[[object, object], bool]:
    """Mint a tolerance equality policy for the ``eq`` slot of any algebraic oracle.

    One structural dispatch owns every data shape: numbers (NaN pairs and matching infinities
    close), arrays, quantity values, structs, dataclasses, ``Result``/``Option`` rails, ``Block``
    collections, mappings, and sequences compare recursively under one tolerance.

    Returns:
        Equality policy closing over ``rel_tol``/``abs_tol``.
    """
    return lambda a, b: _diverge(a, b, rel_tol, abs_tol, "$") is None


def assert_close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> None:
    """Assert recursive tolerance equality and name the first diverging structural path."""
    divergence = _diverge(actual, expected, rel_tol, abs_tol, "$")
    assert divergence is None, f"tolerance breach at {divergence}"


# --- [ALGEBRAIC_ORACLES]


def roundtrip[T, U](x: T, forward: Callable[[T], U], back: Callable[[U], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, back(forward(x)))`` for encode/decode identity."""
    _eq_law(x, back(forward(x)), eq)


def identity[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, f(x))`` for a fixed point under ``f``."""
    _eq_law(x, f(x), eq)


def idempotent[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(f(x), f(f(x)))`` for idempotence."""
    _eq_law(f(x), f(f(x)), eq)


def involution[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, f(f(x)))`` for self-inverse functions."""
    _eq_law(x, f(f(x)), eq)


def inverse[T](x: T, f: Callable[[T], T], g: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, g(f(x)))`` for left-inverse pairs."""
    _eq_law(x, g(f(x)), eq)


def commutative[T](a: T, b: T, op: Callable[[T, T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(a, b), op(b, a))``."""
    _eq_law(op(a, b), op(b, a), eq)


def associative[T](a: T, b: T, c: T, op: Callable[[T, T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(op(a, b), c), op(a, op(b, c)))``."""
    _eq_law(op(op(a, b), c), op(a, op(b, c)), eq)


def distributive[T](a: T, b: T, c: T, mul: Callable[[T, T], T], add: Callable[[T, T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(mul(a, add(b, c)), add(mul(a, b), mul(a, c)))``."""
    _eq_law(mul(a, add(b, c)), add(mul(a, b), mul(a, c)), eq)


def absorbing[T](x: T, op: Callable[[T, T], T], zero: T, *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(x, zero), zero)`` and ``eq(op(zero, x), zero)``."""
    _eq_law(op(x, zero), zero, eq)
    _eq_law(op(zero, x), zero, eq)


def identity_element[T](x: T, op: Callable[[T, T], T], unit: T, *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(unit, x), x)`` and ``eq(op(x, unit), x)``."""
    _eq_law(op(unit, x), x, eq)
    _eq_law(op(x, unit), x, eq)


def monotone[T, K: _Comparable](lo: T, hi: T, projection: Callable[[T], K], *, compare: _Cmp[K] = None) -> None:
    """Assert ``compare(projection(lo), projection(hi)) <= 0``.

    ``compare`` defaults to the built-in ``__lt__`` / ``__eq__`` ordering.
    """
    p_lo = projection(lo)
    p_hi = projection(hi)
    result = compare(p_lo, p_hi) if compare is not None else (0 if p_lo == p_hi else (-1 if p_lo < p_hi else 1))
    assert result <= 0, f"monotone violated: projection({lo!r})={p_lo!r} > projection({hi!r})={p_hi!r}"


def permutation_invariant[T, R](original: T, shuffled: T, f: Callable[[T], R], *, eq: _Eq[R] = None) -> None:
    """Assert ``eq(f(original), f(shuffled))`` for caller-drawn permutations."""
    _eq_law(f(original), f(shuffled), eq)


def metamorphic[T, R](x: T, path: Callable[[T], R], oracle: Callable[[T], R], *, eq: _Eq[R] = None) -> None:
    """Assert ``eq(path(x), oracle(x))`` for two computation paths over one input."""
    _eq_law(path(x), oracle(x), eq)


def metamorphic_sweep[T, R](x: T, f: Callable[[T], R], *relations: MetamorphicRelation[T, R]) -> None:
    """Assert every relation holds between source and follow-up outputs.

    Args:
        x: The source input.
        f: The function under test.
        *relations: Metamorphic relations to enforce against ``f(x)``.
    """
    assert relations, "metamorphic_sweep folded zero relations — an empty sweep proves nothing"
    base = f(x)
    functools.reduce(lambda _, r: r.relate(base, f(r.transform(x))), relations, None)


def refutes[T](witness: T, law: Callable[..., None], *args: object, **kwargs: object) -> None:
    """Assert ``law`` raises ``AssertionError`` for a known-broken witness.

    Args:
        witness: Value for which ``law`` must fail.
        law: Oracle invoked as ``law(witness, *args, **kwargs)``.
        *args: Trailing positional arguments forwarded to ``law``.
        **kwargs: Keyword arguments forwarded to ``law``.

    Raises:
        AssertionError: When ``law`` does not raise on ``witness``.
    """
    try:
        law(witness, *args, **kwargs)
    except AssertionError:
        return
    raise AssertionError("law is a tautology — a surviving mutant exploits it")


# --- [MATRIX_ORACLES]


def _row_scope(subtests: RowCarrier | None, label: str) -> AbstractContextManager[object]:
    """Per-row scope: the carrier's independent subtest when present, pass-through otherwise.

    Returns:
        Context manager owning one matrix row's assertion.
    """
    return nullcontext() if subtests is None else subtests.test(msg=label)


@overload
def validity_matrix[T](cases: Iterable[ValidityCase[T]], valid: Callable[[T], bool], *, subtests: RowCarrier | None = None) -> None: ...


@overload
def validity_matrix[T](cases: Iterable[tuple[str, T, bool]], valid: Callable[[T], bool], *, subtests: RowCarrier | None = None) -> None: ...


def validity_matrix[T](
    cases: Iterable[ValidityCase[T]] | Iterable[tuple[str, T, bool]], valid: Callable[[T], bool], *, subtests: RowCarrier | None = None
) -> None:
    """Assert each case's expected validity verdict.

    Args:
        cases: ``ValidityCase[T]`` instances or raw ``(label, value, expected)`` tuples.
        valid: Predicate returning True when the value is valid.
        subtests: Optional row carrier; a failing row reports independently instead of stopping the fold.
    """
    folded = 0
    for raw in cases:
        case_ = raw if isinstance(raw, ValidityCase) else ValidityCase(label=raw[0], value=raw[1], expected=raw[2])
        folded += 1
        with _row_scope(subtests, case_.label):
            actual = valid(case_.value)
            assert actual == case_.expected, f"validity_matrix[{case_.label!r}]: expected {case_.expected}, got {actual} for {case_.value!r}"
    assert folded, "validity_matrix folded zero rows — an empty case set proves nothing"


def support_matrix(*rows: tuple[str, Callable[[], bool], bool], subtests: RowCarrier | None = None) -> None:
    """Assert each labeled zero-argument probe returns the expected boolean.

    Args:
        *rows: Triples of ``(label, probe, expected)``.
        subtests: Optional row carrier; a failing row reports independently instead of stopping the fold.
    """
    assert rows, "support_matrix folded zero rows — an empty probe set proves nothing"
    for label, probe, expected in rows:
        with _row_scope(subtests, label):
            actual = probe()
            assert actual == expected, f"support_matrix[{label!r}]: expected {expected}, got {actual}"


def projection_matrix[I](cases: Iterable[ProjectionCase[I]], project: Callable[[I], object], *, subtests: RowCarrier | None = None) -> None:
    """Assert each case's projection outcome.

    ``ProjectionCase.oracle`` computes the reference when present; otherwise ``supported_out`` is used.

    Args:
        cases: ProjectionCase[I] instances describing each projection scenario.
        project: The function under test mapping intent to output.
        subtests: Optional row carrier; a failing row reports independently instead of stopping the fold.
    """
    folded = 0
    for case_ in cases:
        folded += 1
        with _row_scope(subtests, case_.label):
            actual = project(case_.intent)
            reference = case_.oracle(case_.intent) if case_.oracle is not None else case_.supported_out
            assert actual == reference, f"projection_matrix[{case_.label!r}]: expected {reference!r}, got {actual!r} (intent={case_.intent!r})"
    assert folded, "projection_matrix folded zero rows — an empty case set proves nothing"


# --- [ROP_ORACLES]

_DEFAULT_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")
# Public wire-axis constant: `assert_roundtrip(value, T, codec=MSGPACK_CODEC)` proves the MessagePack leg.
MSGPACK_CODEC: msgspec.msgpack.Encoder = msgspec.msgpack.Encoder(order="deterministic")


def assert_ok[T, E](result: Result[T, E], *, then: Callable[[T], None] | None = None) -> T:
    """Assert Result is Ok and return the inner value; surfaces the error on failure.

    Args:
        result: The Result under test.
        then: Optional callback invoked with the unwrapped Ok value for additional assertions.

    Returns:
        The unwrapped Ok value.

    Raises:
        AssertionError: When the result is Error or has an unexpected shape.
    """
    match result:
        case Result(tag="ok", ok=v):
            if then is not None:
                then(v)
            return v
        case Result(tag="error", error=e):
            raise AssertionError(f"expected Ok, got Error({e!r})")
        case _:
            raise AssertionError(f"unexpected Result shape: {result!r}")


def assert_error[T, E](result: Result[T, E], *, then: Callable[[E], None] | None = None) -> E:
    """Assert Result is Error and return the error; surfaces the Ok value on failure.

    Args:
        result: The Result under test.
        then: Optional callback invoked with the unwrapped error for additional assertions.

    Returns:
        The unwrapped Error value.

    Raises:
        AssertionError: When the result is Ok or has an unexpected shape.
    """
    match result:
        case Result(tag="error", error=e):
            if then is not None:
                then(e)
            return e
        case Result(tag="ok", ok=v):
            raise AssertionError(f"expected Error, got Ok({v!r})")
        case _:
            raise AssertionError(f"unexpected Result shape: {result!r}")


def assert_error_status[T, E](result: Result[T, E], status: object, *, attr: str = "status") -> E:
    """Assert ``Error`` and an identity match on the error status attribute.

    Args:
        result: The Result under test.
        status: The expected status sentinel.
        attr: Attribute name on the error object carrying the status (default "status").

    Returns:
        The unwrapped Error value.
    """
    e = assert_error(result)
    actual = getattr(e, attr)
    assert actual is status, f"expected {attr}={status!r}, got {actual!r}"
    return e


def assert_some[T](opt: Option[T], *, then: Callable[[T], None] | None = None) -> T:
    """Assert Option is Some and return the inner value.

    Args:
        opt: The Option under test.
        then: Optional callback invoked with the unwrapped value for additional assertions.

    Returns:
        The unwrapped Some value.

    Raises:
        AssertionError: When the option is None or has an unexpected shape.
    """
    match opt:
        case Option(tag="some", some=v):
            if then is not None:
                then(v)
            return v
        case Option(tag="none"):
            raise AssertionError("expected Some, got None")
        case _:
            raise AssertionError(f"unexpected Option shape: {opt!r}")


def assert_none(opt: Option[object]) -> None:
    """Assert Option is None (absent).

    Args:
        opt: The Option under test.

    Raises:
        AssertionError: When the option is Some or has an unexpected shape.
    """
    match opt:
        case Option(tag="none"):
            return
        case Option(tag="some", some=v):
            raise AssertionError(f"expected None, got Some({v!r})")
        case _:
            raise AssertionError(f"unexpected Option shape: {opt!r}")


def assert_roundtrip[T](value: T, typ: type[T], *, codec: msgspec.json.Encoder | msgspec.msgpack.Encoder | None = None) -> T:
    """Assert deterministic encode, decode, and re-encode byte identity for a wire value.

    The re-encode step catches non-deterministic codecs that structural equality misses. The
    decode lane derives from the encoder family, so one oracle proves both production wire legs:
    JSON by default, MessagePack when ``codec`` is a ``msgspec.msgpack.Encoder``.

    Args:
        value: The value to encode and round-trip.
        typ: The target type for the family decode.
        codec: Encoder to use; defaults to deterministic JSON ordering.

    Returns:
        The decoded value, structurally equal to ``value``.
    """
    enc = codec if codec is not None else _DEFAULT_ENCODER
    raw = enc.encode(value)
    decoded: T = msgspec.msgpack.decode(raw, type=typ) if isinstance(enc, msgspec.msgpack.Encoder) else msgspec.json.decode(raw, type=typ)
    assert decoded == value, f"decode mismatch for {typ.__name__}: {decoded!r} != {value!r}"
    reencoded = enc.encode(decoded)
    assert reencoded == raw, f"re-encode not byte-identical for {typ.__name__}: {reencoded!r} != {raw!r}"
    return decoded


# --- [STATEFUL_MODEL_BASED]


def model_based[M: RuleBasedStateMachine](machine_cls: type[M], *, profile: str | None = None, settings: hyp_settings | None = None) -> None:
    """Drive a stateful Hypothesis machine under the resolved settings profile.

    Args:
        machine_cls: ``RuleBasedStateMachine`` subclass with rules and invariants.
        profile: Explicit registered profile name; ``None`` derives from the active profile.
        settings: Explicit Hypothesis settings; overrides profile resolution.
    """
    active = hyp_settings.get_current_profile_name()
    resolved = profile if profile is not None else (PROFILE_MUTATION if active == PROFILE_MUTATION else PROFILE_STATEFUL)
    run_state_machine_as_test(  # type: ignore[no-untyped-call]  # hypothesis leaves the driver unannotated
        machine_cls, settings=settings if settings is not None else hyp_settings.get_profile(resolved)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ValidityCase",
    "ProjectionCase",
    "MetamorphicRelation",
    "RowCarrier",
    "MSGPACK_CODEC",
    "close",
    "assert_close",
    "roundtrip",
    "identity",
    "idempotent",
    "involution",
    "inverse",
    "commutative",
    "associative",
    "distributive",
    "absorbing",
    "identity_element",
    "monotone",
    "permutation_invariant",
    "metamorphic",
    "metamorphic_sweep",
    "refutes",
    "validity_matrix",
    "support_matrix",
    "projection_matrix",
    "assert_ok",
    "assert_error",
    "assert_error_status",
    "assert_some",
    "assert_none",
    "assert_roundtrip",
    "model_based",
    "Bundle",
    "RuleBasedStateMachine",
    "consumes",
    "initialize",
    "invariant",
    "multiple",
    "precondition",
    "rule",
    "target",
]
