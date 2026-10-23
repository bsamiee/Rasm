"""Assertions for tolerance, algebraic laws, case tables, results, and codec round trips."""

import cmath
from collections.abc import Callable, Iterable, Mapping, Sequence
from decimal import Decimal
from fractions import Fraction
import operator
from typing import Concatenate, Protocol, runtime_checkable, Self

from expression import Option, Result
from expression.collections import Block
import msgspec
import msgspec.json
import msgspec.msgpack
import numpy as np
import pytest

# --- [TYPES] ----------------------------------------------------------------------------

type _Equality[T] = Callable[[T, T], bool]
type Case[I, O] = tuple[str, I, O]
type Relation[T, R] = tuple[str, Callable[[T], T], Callable[[R, R], None]]


class _Comparable(Protocol):
    """Projection key bound with total ``<`` ordering."""

    def __lt__(self, other: Self, /) -> bool: ...


@runtime_checkable
class _Quantity(Protocol):
    """Quantity with units and magnitude."""

    @property
    def units(self) -> object: ...
    @property
    def magnitude(self) -> object: ...


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [TOLERANCE]


def _diverge(a: object, b: object, rel_tol: float, abs_tol: float, path: str) -> str | None:
    """Return first tolerance divergence at its structural path, or ``None``."""
    match (a, b):
        case (bool() | str() | bytes(), _) | (_, bool() | str() | bytes()):
            return None if isinstance(a, bool) == isinstance(b, bool) and a == b else f"{path}: {a!r} != {b!r}"
        case (np.ndarray() | np.generic(), _) | (_, np.ndarray() | np.generic()) if (left := np.asarray(a)).shape != (right := np.asarray(b)).shape:
            return f"{path}: shape {left.shape} != {right.shape}"
        case (np.ndarray() | np.generic(), _) | (_, np.ndarray() | np.generic()):
            left, right = np.atleast_1d(np.asarray(a)), np.atleast_1d(np.asarray(b))
            far = np.argwhere(~np.isclose(left, right, rtol=rel_tol, atol=abs_tol, equal_nan=True))
            return None if far.size == 0 else f"{path}{far[0].tolist()}: {left[tuple(far[0])]!r} !~ {right[tuple(far[0])]!r}"
        case ((int() | float() | complex() | Decimal() | Fraction()) as left, (int() | float() | complex() | Decimal() | Fraction()) as right):
            fa, fb = complex(left), complex(right)
            return None if cmath.isclose(fa, fb, rel_tol=rel_tol, abs_tol=abs_tol) or (cmath.isnan(fa) and cmath.isnan(fb)) else f"{path}: {a!r} !~ {b!r} at rel_tol={rel_tol}, abs_tol={abs_tol}"
        case (_Quantity() as left, _Quantity() as right) if left.units != right.units:
            return f"{path}: units {left.units!r} != {right.units!r}"
        case (_Quantity() as left, _Quantity() as right):
            children: Iterable[tuple[str, object, object]] = ((".magnitude", left.magnitude, right.magnitude),)
        case (Result(tag=tag), Result(tag=other)) | (Option(tag=tag), Option(tag=other)) if tag != other:
            return f"{path}: tags differ, {a!r} != {b!r}"
        case (Result(tag=tag), Result()) | (Option(tag=tag), Option()):
            children = ((f".{tag}", getattr(a, tag), getattr(b, tag)),)
        case (msgspec.Struct(), msgspec.Struct()) if type(a) is type(b):
            children = ((f".{field}", getattr(a, field), getattr(b, field)) for field in a.__struct_fields__)
        case (Mapping(), Mapping()) if set(a) != set(b):
            return f"{path}: unshared keys {sorted(map(repr, set(a) ^ set(b)))}"
        case (Mapping(), Mapping()):
            children = ((f"[{key!r}]", value, b[key]) for key, value in a.items())
        case (Block() as left, Block() as right) | (Sequence() as left, Sequence() as right) if len(left) != len(right):
            return f"{path}: length {len(left)} != {len(right)}"
        case (Block() as left, Block() as right) | (Sequence() as left, Sequence() as right):
            children = ((f"[{index}]", x, y) for index, (x, y) in enumerate(zip(left, right)))
        case _:
            return None if a == b else f"{path}: {a!r} != {b!r}"
    return next((divergence for suffix, x, y in children if (divergence := _diverge(x, y, rel_tol, abs_tol, f"{path}{suffix}")) is not None), None)


def close(*, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> Callable[[object, object], bool]:
    """Return recursive approximate-equality function over nested values."""
    return lambda a, b: _diverge(a, b, rel_tol, abs_tol, "$") is None


def assert_close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> None:
    """Assert recursive tolerance equality, naming first diverging structural path."""
    divergence = _diverge(actual, expected, rel_tol, abs_tol, "$")
    assert divergence is None, f"tolerance violation at {divergence}"


# --- [LAWS]


def _assert_equal[T](left: T, right: T, equal: _Equality[T]) -> None:
    assert equal(left, right), f"property failed: {left!r} != {right!r}"


def roundtrip[T, U](x: T, forward: Callable[[T], U], back: Callable[[U], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(x, back(forward(x)))`` for encode/decode identity."""
    _assert_equal(x, back(forward(x)), eq)


def identity[T](x: T, f: Callable[[T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(x, f(x))`` for fixed point of ``f``."""
    _assert_equal(x, f(x), eq)


def idempotent[T](x: T, f: Callable[[T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(f(x), f(f(x)))`` for idempotence."""
    once = f(x)
    _assert_equal(once, f(once), eq)


def involution[T](x: T, f: Callable[[T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(x, f(f(x)))`` for self-inverse functions."""
    _assert_equal(x, f(f(x)), eq)


def inverse[T](x: T, f: Callable[[T], T], g: Callable[[T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(x, g(f(x)))`` for left-inverse pairs."""
    _assert_equal(x, g(f(x)), eq)


def commutative[T](a: T, b: T, op: Callable[[T, T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(op(a, b), op(b, a))``."""
    _assert_equal(op(a, b), op(b, a), eq)


def associative[T](a: T, b: T, c: T, op: Callable[[T, T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(op(op(a, b), c), op(a, op(b, c)))``."""
    _assert_equal(op(op(a, b), c), op(a, op(b, c)), eq)


def distributive[T](a: T, b: T, c: T, mul: Callable[[T, T], T], add: Callable[[T, T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(mul(a, add(b, c)), add(mul(a, b), mul(a, c)))``."""
    _assert_equal(mul(a, add(b, c)), add(mul(a, b), mul(a, c)), eq)


def absorbing[T](x: T, op: Callable[[T, T], T], zero: T, *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(op(x, zero), zero)`` and ``eq(op(zero, x), zero)``."""
    _assert_equal(op(x, zero), zero, eq)
    _assert_equal(op(zero, x), zero, eq)


def identity_element[T](x: T, op: Callable[[T, T], T], unit: T, *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(op(unit, x), x)`` and ``eq(op(x, unit), x)``."""
    _assert_equal(op(unit, x), x, eq)
    _assert_equal(op(x, unit), x, eq)


def monotone[T, K: _Comparable](lo: T, hi: T, projection: Callable[[T], K]) -> None:
    """Assert ``not projection(hi) < projection(lo)``."""
    p_lo = projection(lo)
    p_hi = projection(hi)
    assert not p_hi < p_lo, f"monotone violated: projection({lo!r})={p_lo!r} > projection({hi!r})={p_hi!r}"


def permutation_invariant[T, R](original: T, shuffled: T, f: Callable[[T], R], *, eq: _Equality[R] = operator.eq) -> None:
    """Assert ``eq(f(original), f(shuffled))`` for caller-drawn permutations."""
    _assert_equal(f(original), f(shuffled), eq)


def differential[T, R](value: T, implementation: Callable[[T], R], reference: Callable[[T], R], *, eq: _Equality[R] = operator.eq) -> None:
    """Assert ``eq(implementation(value), reference(value))``."""
    _assert_equal(implementation(value), reference(value), eq)


def metamorphic[T, R](value: T, function: Callable[[T], R], relations: Iterable[Relation[T, R]], subtests: pytest.Subtests) -> None:
    """Assert ``relate(function(value), function(transform(value)))`` per relation in its own subtest."""
    baseline = function(value)
    for name, transform, relate in relations:
        with subtests.test(msg=name):
            relate(baseline, function(transform(value)))


def rejects_counterexample[T, **P](counterexample: T, property_assertion: Callable[Concatenate[T, P], None], *args: P.args, **kwargs: P.kwargs) -> None:
    """Raise ``AssertionError`` when ``property_assertion`` accepts ``counterexample``."""
    try:
        property_assertion(counterexample, *args, **kwargs)
    except AssertionError:
        return
    raise AssertionError(f"property accepts counterexample {counterexample!r}")


# --- [TABLES]


def assert_table[I, O](cases: Iterable[Case[I, O]], function: Callable[[I], O], subtests: pytest.Subtests) -> None:
    """Assert ``function(value) == expected`` per row in its own subtest."""
    for label, value, expected in cases:
        with subtests.test(msg=label):
            actual = function(value)
            assert actual == expected, f"{label!r}: expected {expected!r}, got {actual!r} for {value!r}"


# --- [RESULTS]


def assert_ok[T, E](result: Result[T, E]) -> T:
    """Return ``Ok`` value, raising ``AssertionError`` on ``Error``."""
    match result:
        case Result(tag="ok", ok=v):
            return v
        case _:
            raise AssertionError(f"expected Ok, got {result!r}")


def assert_error[T, E](result: Result[T, E]) -> E:
    """Return ``Error`` value, raising ``AssertionError`` on ``Ok``."""
    match result:
        case Result(tag="error", error=e):
            return e
        case _:
            raise AssertionError(f"expected Error, got {result!r}")


def assert_some[T](option: Option[T]) -> T:
    """Return ``Some`` value, raising ``AssertionError`` on ``Nothing``."""
    match option:
        case Option(tag="some", some=v):
            return v
        case _:
            raise AssertionError("expected Some, got Nothing")


def assert_none(option: Option[object]) -> None:
    """Raise ``AssertionError`` on ``Some``."""
    match option:
        case Option(tag="none"):
            return
        case _:
            raise AssertionError(f"expected Nothing, got {option!r}")


# --- [CODECS]


def assert_roundtrip[T](value: T, typ: type[T], *, codec: type[msgspec.json.Encoder | msgspec.msgpack.Encoder] = msgspec.json.Encoder) -> T:
    """Return decoded value after asserting equality and byte-identical deterministic re-encode."""
    encoder = codec(order="deterministic")
    raw = encoder.encode(value)
    decoded: T = msgspec.msgpack.decode(raw, type=typ) if isinstance(encoder, msgspec.msgpack.Encoder) else msgspec.json.decode(raw, type=typ)
    assert decoded == value, f"{typ.__name__} decode: {decoded!r} != {value!r}"
    reencoded = encoder.encode(decoded)
    assert reencoded == raw, f"{typ.__name__} re-encode: {reencoded!r} != {raw!r}"
    return decoded


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Case",
    "Relation",
    "absorbing",
    "assert_close",
    "assert_error",
    "assert_none",
    "assert_ok",
    "assert_roundtrip",
    "assert_some",
    "assert_table",
    "associative",
    "close",
    "commutative",
    "differential",
    "distributive",
    "idempotent",
    "identity",
    "identity_element",
    "inverse",
    "involution",
    "metamorphic",
    "monotone",
    "permutation_invariant",
    "rejects_counterexample",
    "roundtrip",
]
