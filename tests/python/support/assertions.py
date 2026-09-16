"""Reusable assertions for algebraic properties, tables, tolerance, results, and state machines."""

# --- [IMPORTS] --------------------------------------------------------------------------

import cmath
from collections.abc import Callable, Iterable, Mapping, Sequence
import dataclasses
from decimal import Decimal
from fractions import Fraction
import operator
from typing import Protocol, runtime_checkable, Self

from expression import Option, Result
from expression.collections import Block
from hypothesis import settings as hyp_settings
from hypothesis.stateful import RuleBasedStateMachine, run_state_machine_as_test
import msgspec
import msgspec.json
import msgspec.msgpack
import pytest
lazy import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type _Equality[T] = Callable[[T, T], bool]
type Case[I, O] = tuple[str, I, O]
type Relation[T, R] = tuple[str, Callable[[T], T], Callable[[R, R], None]]


class _Comparable(Protocol):
    """Structural bound for projection keys with total less-than ordering."""

    def __lt__(self, other: Self, /) -> bool: ...


@runtime_checkable
class _Quantity(Protocol):
    """Structural shape of a quantity exposing units and magnitude."""

    @property
    def units(self) -> object: ...
    @property
    def magnitude(self) -> object: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

JSON_ENCODER = msgspec.json.Encoder(order="deterministic")
MSGPACK_ENCODER = msgspec.msgpack.Encoder(order="deterministic")

# --- [TOLERANCE_ORACLES] ----------------------------------------------------------------


def _diverge(a: object, b: object, rel_tol: float, abs_tol: float, path: str) -> str | None:
    """Name the first tolerance divergence at its structural path, or ``None`` when the values are close."""
    match (a, b):
        case (bool(), _) | (_, bool()) | (str(), _) | (bytes(), _):
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
        case ((int() | float() | complex() | Decimal() | Fraction()) as num_a, (int() | float() | complex() | Decimal() | Fraction()) as num_b):
            fa, fb = complex(num_a), complex(num_b)
            close_enough = fa == fb or (cmath.isnan(fa) and cmath.isnan(fb)) or cmath.isclose(fa, fb, rel_tol=rel_tol, abs_tol=abs_tol)
            return None if close_enough else f"{path}: |{a!r} - {b!r}| exceeds rel_tol={rel_tol}, abs_tol={abs_tol}"
        case (_Quantity() as qty_a, _Quantity() as qty_b):
            if qty_b.units != qty_a.units:
                return f"{path}: units {qty_a.units!r} != {qty_b.units!r}"
            return _diverge(qty_a.magnitude, qty_b.magnitude, rel_tol, abs_tol, f"{path}.magnitude")
        case (Result(tag="ok", ok=left), Result(tag="ok", ok=right)):
            return _diverge(left, right, rel_tol, abs_tol, f"{path}.ok")
        case (Result(tag="error", error=left), Result(tag="error", error=right)):
            return _diverge(left, right, rel_tol, abs_tol, f"{path}.error")
        case (Option(tag="some", some=left), Option(tag="some", some=right)):
            return _diverge(left, right, rel_tol, abs_tol, f"{path}.some")
        case (Option(tag="none"), Option(tag="none")):
            return None
        case (Result(), Result()) | (Option(), Option()):
            return f"{path}: result tags differ: {a!r} != {b!r}"
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
    """Return a recursive approximate-equality function over nested values."""
    return lambda a, b: _diverge(a, b, rel_tol, abs_tol, "$") is None


def assert_close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> None:
    """Assert recursive tolerance equality and name the first diverging structural path."""
    divergence = _diverge(actual, expected, rel_tol, abs_tol, "$")
    assert divergence is None, f"tolerance violation at {divergence}"


# --- [ALGEBRAIC_LAWS] -------------------------------------------------------------------


def _assert_equal[T](left: T, right: T, equal: _Equality[T]) -> None:
    assert equal(left, right), f"property failed: {left!r} != {right!r}"


def roundtrip[T, U](x: T, forward: Callable[[T], U], back: Callable[[U], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(x, back(forward(x)))`` for encode/decode identity."""
    _assert_equal(x, back(forward(x)), eq)


def identity[T](x: T, f: Callable[[T], T], *, eq: _Equality[T] = operator.eq) -> None:
    """Assert ``eq(x, f(x))`` for a fixed point under ``f``."""
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
    """Assert ``projection(lo)`` does not exceed ``projection(hi)`` under the key's ordering."""
    p_lo = projection(lo)
    p_hi = projection(hi)
    assert not p_hi < p_lo, f"monotone violated: projection({lo!r})={p_lo!r} > projection({hi!r})={p_hi!r}"


def permutation_invariant[T, R](original: T, shuffled: T, f: Callable[[T], R], *, eq: _Equality[R] = operator.eq) -> None:
    """Assert ``eq(f(original), f(shuffled))`` for caller-drawn permutations."""
    _assert_equal(f(original), f(shuffled), eq)


def differential[T, R](value: T, implementation: Callable[[T], R], reference: Callable[[T], R], *, eq: _Equality[R] = operator.eq) -> None:
    """Compare an implementation with an independent reference over an input."""
    _assert_equal(implementation(value), reference(value), eq)


def metamorphic[T, R](value: T, function: Callable[[T], R], *relations: Relation[T, R]) -> None:
    """Assert every relation ``(name, transform, relate)`` holds between ``function(value)`` and ``function(transform(value))``."""
    assert relations, "metamorphic requires at least one relation"
    baseline = function(value)
    for _, transform, relate in relations:
        relate(baseline, function(transform(value)))


# --- [TABLE_ASSERTIONS] -----------------------------------------------------------------


def assert_table[I, O](cases: Iterable[Case[I, O]], function: Callable[[I], O], subtests: pytest.Subtests) -> None:
    """Assert ``function(value) == expected`` for each ``(label, value, expected)`` row as an independent subtest."""
    rows = tuple(cases)
    assert rows, "assert_table requires at least one case"
    for label, value, expected in rows:
        with subtests.test(msg=label):
            actual = function(value)
            assert actual == expected, f"{label!r}: expected {expected!r}, got {actual!r} for {value!r}"


# --- [RESULT_ASSERTIONS] ----------------------------------------------------------------


def assert_ok[T, E](result: Result[T, E]) -> T:
    """Assert ``Ok``.

    Returns:
        The inner value.

    Raises:
        AssertionError: The result is ``Error``.
    """
    match result:
        case Result(tag="ok", ok=v):
            return v
        case _:
            raise AssertionError(f"expected Ok, got {result!r}")


def assert_error[T, E](result: Result[T, E]) -> E:
    """Assert ``Error``.

    Returns:
        The error.

    Raises:
        AssertionError: The result is ``Ok``.
    """
    match result:
        case Result(tag="error", error=e):
            return e
        case _:
            raise AssertionError(f"expected Error, got {result!r}")


def assert_some[T](opt: Option[T]) -> T:
    """Assert ``Some``.

    Returns:
        The inner value.

    Raises:
        AssertionError: The option is ``Nothing``.
    """
    match opt:
        case Option(tag="some", some=v):
            return v
        case _:
            raise AssertionError("expected Some, got Nothing")


def assert_none(opt: Option[object]) -> None:
    """Assert ``Nothing``.

    Raises:
        AssertionError: The option is ``Some``.
    """
    match opt:
        case Option(tag="some", some=v):
            raise AssertionError(f"expected Nothing, got Some({v!r})")
        case _:
            return


def assert_roundtrip[T](value: T, typ: type[T], *, encoder: msgspec.json.Encoder | msgspec.msgpack.Encoder = JSON_ENCODER) -> T:
    """Assert encode then decode equality and re-encode byte identity, the re-encode step catches non-deterministic codecs that structural equality misses.

    Returns:
        The decoded value, JSON by default or MessagePack with that encoder.
    """
    raw = encoder.encode(value)
    decoded: T = msgspec.msgpack.decode(raw, type=typ) if isinstance(encoder, msgspec.msgpack.Encoder) else msgspec.json.decode(raw, type=typ)
    assert decoded == value, f"decode mismatch for {typ.__name__}: {decoded!r} != {value!r}"
    reencoded = encoder.encode(decoded)
    assert reencoded == raw, f"re-encode not byte-identical for {typ.__name__}: {reencoded!r} != {raw!r}"
    return decoded


# --- [STATEFUL_TESTING] -----------------------------------------------------------------


def run_state_machine[M: RuleBasedStateMachine](machine_cls: type[M], *, steps: int = 200) -> None:
    """Run a Hypothesis state machine for ``steps`` rule applications per example under the active profile."""
    run_state_machine_as_test(machine_cls, settings=hyp_settings(stateful_step_count=steps))  # type: ignore[no-untyped-call]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Case",
    "Relation",
    "JSON_ENCODER",
    "MSGPACK_ENCODER",
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
    "differential",
    "metamorphic",
    "assert_table",
    "assert_ok",
    "assert_error",
    "assert_some",
    "assert_none",
    "assert_roundtrip",
    "run_state_machine",
]
