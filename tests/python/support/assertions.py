"""Reusable assertions for algebraic properties, table-driven tests, results, and state machines."""

# --- [IMPORTS] --------------------------------------------------------------------------

import cmath
from collections.abc import Callable, Iterable, Mapping, Sequence
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
from hypothesis.stateful import Bundle, consumes, initialize, invariant, multiple, precondition, rule, RuleBasedStateMachine, run_state_machine_as_test
import msgspec
import msgspec.json
import msgspec.msgpack
lazy import numpy as np

from tests.python.support.runtime import PROFILE_STATEFUL

if TYPE_CHECKING:
    from contextlib import AbstractContextManager


# --- [TYPES] ----------------------------------------------------------------------------

type _Eq[T] = Callable[[T, T], bool] | None
type _Cmp[T] = Callable[[T, T], int] | None


class _Comparable(Protocol):
    """Structural bound for projection keys with total less-than ordering."""

    def __lt__(self, other: Self, /) -> bool: ...


class SubtestReporter(Protocol):
    """Protocol implemented by pytest's ``subtests`` fixture."""

    def test(self, msg: str | None = None, **kwargs: object) -> AbstractContextManager[object]: ...


type _Numeric = int | float | complex | Decimal | fractions.Fraction


@runtime_checkable
class _QuantityLike(Protocol):
    """Protocol for quantity values exposing units and magnitude."""

    @property
    def units(self) -> object: ...
    @property
    def magnitude(self) -> object: ...


# --- [MODELS] ---------------------------------------------------------------------------


class ValidityCase[T](msgspec.Struct, frozen=True, gc=False):
    """Labeled input and expected result for ``validity_matrix``."""

    label: str
    value: T
    expected: bool


class ProjectionCase[I](msgspec.Struct, frozen=True, gc=False):
    """Projection input with a fixed or computed expected result."""

    label: str
    intent: I
    expected: object
    reference: Callable[[I], object] | None


class MetamorphicRelation[T, R](msgspec.Struct, frozen=True, gc=False):
    """Metamorphic transformation and its output relation assertion."""

    name: str
    transform: Callable[[T], T]
    relate: Callable[[R, R], None]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _assert_equal[T](left: T, right: T, equal: _Eq[T]) -> None:
    """Assert structural or custom equality and report both values."""
    assert (equal if equal is not None else operator.eq)(left, right), f"property failed: {left!r} != {right!r}"


# --- [TOLERANCE_ORACLES]


def _num_close(a: _Numeric, b: _Numeric, rel_tol: float, abs_tol: float) -> bool:
    fa, fb = complex(a), complex(b)
    if fa == fb or (cmath.isnan(fa) and cmath.isnan(fb)):
        return True
    return abs(fa - fb) <= max(rel_tol * max(abs(fa), abs(fb)), abs_tol)


def _result_diverge(a: object, b: object, rel_tol: float, abs_tol: float, path: str) -> str | None:
    """Compare Result or Option pairs and recurse into matching cases."""
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
            return f"{path}: result tags differ: {a!r} != {b!r}"


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
        case ((int() | float() | complex() | Decimal() | fractions.Fraction()) as num_a, (int() | float() | complex() | Decimal() | fractions.Fraction()) as num_b):
            return None if _num_close(num_a, num_b, rel_tol, abs_tol) else f"{path}: |{a!r} - {b!r}| exceeds rel_tol={rel_tol}, abs_tol={abs_tol}"
        case (_QuantityLike() as qty_a, _QuantityLike() as qty_b):
            if qty_b.units != qty_a.units:
                return f"{path}: units {qty_a.units!r} != {qty_b.units!r}"
            return _diverge(qty_a.magnitude, qty_b.magnitude, rel_tol, abs_tol, f"{path}.magnitude")
        case (Result(), Result()) | (Option(), Option()):
            return _result_diverge(a, b, rel_tol, abs_tol, path)
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
    """Return a recursive approximate-equality function over numbers, arrays, quantity values, structs, dataclasses, ``Result``/``Option`` values, ``Block`` collections, mappings, and sequences."""
    return lambda a, b: _diverge(a, b, rel_tol, abs_tol, "$") is None


def assert_close(actual: object, expected: object, *, rel_tol: float = 1e-9, abs_tol: float = 0.0) -> None:
    """Assert recursive tolerance equality and name the first diverging structural path."""
    divergence = _diverge(actual, expected, rel_tol, abs_tol, "$")
    assert divergence is None, f"tolerance violation at {divergence}"


# --- [ALGEBRAIC_PROPERTIES]


def roundtrip[T, U](x: T, forward: Callable[[T], U], back: Callable[[U], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, back(forward(x)))`` for encode/decode identity."""
    _assert_equal(x, back(forward(x)), eq)


def identity[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, f(x))`` for a fixed point under ``f``."""
    _assert_equal(x, f(x), eq)


def idempotent[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(f(x), f(f(x)))`` for idempotence."""
    _assert_equal(f(x), f(f(x)), eq)


def involution[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, f(f(x)))`` for self-inverse functions."""
    _assert_equal(x, f(f(x)), eq)


def inverse[T](x: T, f: Callable[[T], T], g: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, g(f(x)))`` for left-inverse pairs."""
    _assert_equal(x, g(f(x)), eq)


def commutative[T](a: T, b: T, op: Callable[[T, T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(a, b), op(b, a))``."""
    _assert_equal(op(a, b), op(b, a), eq)


def associative[T](a: T, b: T, c: T, op: Callable[[T, T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(op(a, b), c), op(a, op(b, c)))``."""
    _assert_equal(op(op(a, b), c), op(a, op(b, c)), eq)


def distributive[T](a: T, b: T, c: T, mul: Callable[[T, T], T], add: Callable[[T, T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(mul(a, add(b, c)), add(mul(a, b), mul(a, c)))``."""
    _assert_equal(mul(a, add(b, c)), add(mul(a, b), mul(a, c)), eq)


def absorbing[T](x: T, op: Callable[[T, T], T], zero: T, *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(x, zero), zero)`` and ``eq(op(zero, x), zero)``."""
    _assert_equal(op(x, zero), zero, eq)
    _assert_equal(op(zero, x), zero, eq)


def identity_element[T](x: T, op: Callable[[T, T], T], unit: T, *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(unit, x), x)`` and ``eq(op(x, unit), x)``."""
    _assert_equal(op(unit, x), x, eq)
    _assert_equal(op(x, unit), x, eq)


def monotone[T, K: _Comparable](lo: T, hi: T, projection: Callable[[T], K], *, compare: _Cmp[K] = None) -> None:
    """Assert ``compare(projection(lo), projection(hi)) <= 0``, ``compare`` defaults to the built-in ordering."""
    p_lo = projection(lo)
    p_hi = projection(hi)
    result = compare(p_lo, p_hi) if compare is not None else (0 if p_lo == p_hi else (-1 if p_lo < p_hi else 1))
    assert result <= 0, f"monotone violated: projection({lo!r})={p_lo!r} > projection({hi!r})={p_hi!r}"


def permutation_invariant[T, R](original: T, shuffled: T, f: Callable[[T], R], *, eq: _Eq[R] = None) -> None:
    """Assert ``eq(f(original), f(shuffled))`` for caller-drawn permutations."""
    _assert_equal(f(original), f(shuffled), eq)


def differential[T, R](value: T, implementation: Callable[[T], R], reference: Callable[[T], R], *, eq: _Eq[R] = None) -> None:
    """Compare an implementation with an independent reference over an input."""
    _assert_equal(implementation(value), reference(value), eq)


def assert_metamorphic_relations[T, R](value: T, function: Callable[[T], R], *relations: MetamorphicRelation[T, R]) -> None:
    """Assert every relation holds between ``f(x)`` and each follow-up output."""
    assert relations, "assert_metamorphic_relations requires at least one relation"
    baseline = function(value)
    functools.reduce(lambda _, relation: relation.relate(baseline, function(relation.transform(value))), relations, None)


def rejects_counterexample[T](counterexample: T, property_assertion: Callable[..., None], *args: object, **kwargs: object) -> None:
    """Assert a property assertion rejects a known counterexample.

    Raises:
        AssertionError: When the property accepts the counterexample.
    """
    try:
        property_assertion(counterexample, *args, **kwargs)
    except AssertionError:
        return
    raise AssertionError(f"property accepts its counterexample: {counterexample!r}")


# --- [TABLE_DRIVEN_ASSERTIONS]


def _subtest_context(subtests: SubtestReporter | None, label: str) -> AbstractContextManager[object]:
    """Return an independent subtest context when the fixture is available."""
    return nullcontext() if subtests is None else subtests.test(msg=label)


@overload
def validity_matrix[T](cases: Iterable[ValidityCase[T]], valid: Callable[[T], bool], *, subtests: SubtestReporter | None = None) -> None: ...


@overload
def validity_matrix[T](cases: Iterable[tuple[str, T, bool]], valid: Callable[[T], bool], *, subtests: SubtestReporter | None = None) -> None: ...


def validity_matrix[T](cases: Iterable[ValidityCase[T]] | Iterable[tuple[str, T, bool]], valid: Callable[[T], bool], *, subtests: SubtestReporter | None = None) -> None:
    """Assert each case's expected validity as an independent subtest when available."""
    count = 0
    for raw in cases:
        case_ = raw if isinstance(raw, ValidityCase) else ValidityCase(label=raw[0], value=raw[1], expected=raw[2])
        count += 1
        with _subtest_context(subtests, case_.label):
            actual = valid(case_.value)
            assert actual == case_.expected, f"validity_matrix[{case_.label!r}]: expected {case_.expected}, got {actual} for {case_.value!r}"
    assert count, "validity_matrix requires at least one case"


def capability_matrix(*rows: tuple[str, Callable[[], bool], bool], subtests: SubtestReporter | None = None) -> None:
    """Assert labeled capability checks as independent subtests when available."""
    assert rows, "capability_matrix requires at least one case"
    for label, probe, expected in rows:
        with _subtest_context(subtests, label):
            actual = probe()
            assert actual == expected, f"capability_matrix[{label!r}]: expected {expected}, got {actual}"


def projection_matrix[I](cases: Iterable[ProjectionCase[I]], project: Callable[[I], object], *, subtests: SubtestReporter | None = None) -> None:
    """Assert each projection result as an independent subtest when available."""
    count = 0
    for case_ in cases:
        count += 1
        with _subtest_context(subtests, case_.label):
            actual = project(case_.intent)
            expected = case_.reference(case_.intent) if case_.reference is not None else case_.expected
            assert actual == expected, f"projection_matrix[{case_.label!r}]: expected {expected!r}, got {actual!r} (intent={case_.intent!r})"
    assert count, "projection_matrix requires at least one case"


# --- [RESULT_ASSERTIONS]

_DEFAULT_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")
MSGPACK_ENCODER: msgspec.msgpack.Encoder = msgspec.msgpack.Encoder(order="deterministic")


def assert_ok[T, E](result: Result[T, E], *, then: Callable[[T], None] | None = None) -> T:
    """Assert ``Ok`` and return the inner value, running ``then`` over it, an ``Error`` reports its payload.

    Raises:
        AssertionError: When the result is ``Error`` or has an unexpected variant.
    """
    match result:
        case Result(tag="ok", ok=v):
            if then is not None:
                then(v)
            return v
        case Result(tag="error", error=e):
            raise AssertionError(f"expected Ok, got Error({e!r})")
        case _:
            raise AssertionError(f"unexpected Result variant: {result!r}")


def assert_error[T, E](result: Result[T, E], *, then: Callable[[E], None] | None = None) -> E:
    """Assert ``Error`` and return the error, running ``then`` over it, an ``Ok`` reports its value.

    Raises:
        AssertionError: When the result is ``Ok`` or has an unexpected variant.
    """
    match result:
        case Result(tag="error", error=e):
            if then is not None:
                then(e)
            return e
        case Result(tag="ok", ok=v):
            raise AssertionError(f"expected Error, got Ok({v!r})")
        case _:
            raise AssertionError(f"unexpected Result variant: {result!r}")


def assert_error_status[T, E](result: Result[T, E], status: object, *, attr: str = "status") -> E:
    """Assert ``Error`` with ``attr`` identical (``is``) to ``status`` and return the error."""
    e = assert_error(result)
    actual = getattr(e, attr)
    assert actual is status, f"expected {attr}={status!r}, got {actual!r}"
    return e


def assert_some[T](opt: Option[T], *, then: Callable[[T], None] | None = None) -> T:
    """Assert ``Some`` and return the inner value, running ``then`` over it.

    Raises:
        AssertionError: When the option is ``Nothing`` or has an unexpected variant.
    """
    match opt:
        case Option(tag="some", some=v):
            if then is not None:
                then(v)
            return v
        case Option(tag="none"):
            raise AssertionError("expected Some, got None")
        case _:
            raise AssertionError(f"unexpected Option variant: {opt!r}")


def assert_none(opt: Option[object]) -> None:
    """Assert ``Nothing``.

    Raises:
        AssertionError: When the option is ``Some`` or has an unexpected variant.
    """
    match opt:
        case Option(tag="none"):
            return
        case Option(tag="some", some=v):
            raise AssertionError(f"expected None, got Some({v!r})")
        case _:
            raise AssertionError(f"unexpected Option variant: {opt!r}")


def assert_roundtrip[T](value: T, typ: type[T], *, encoder: msgspec.json.Encoder | msgspec.msgpack.Encoder | None = None) -> T:
    """Assert encode → decode equality and re-encode byte identity, returning the decoded value.

    The re-encode step catches non-deterministic codecs that structural equality misses. The encoder type selects
    JSON by default or MessagePack when supplied.
    """
    enc = encoder if encoder is not None else _DEFAULT_ENCODER
    raw = enc.encode(value)
    decoded: T = msgspec.msgpack.decode(raw, type=typ) if isinstance(enc, msgspec.msgpack.Encoder) else msgspec.json.decode(raw, type=typ)
    assert decoded == value, f"decode mismatch for {typ.__name__}: {decoded!r} != {value!r}"
    reencoded = enc.encode(decoded)
    assert reencoded == raw, f"re-encode not byte-identical for {typ.__name__}: {reencoded!r} != {raw!r}"
    return decoded


# --- [STATEFUL_TESTING]


def run_state_machine[M: RuleBasedStateMachine](machine_cls: type[M], *, profile: str | None = None, settings: hyp_settings | None = None) -> None:
    """Run a Hypothesis state machine with explicit settings or a named profile."""
    resolved = profile if profile is not None else PROFILE_STATEFUL
    run_state_machine_as_test(  # type: ignore[no-untyped-call]
        machine_cls, settings=settings if settings is not None else hyp_settings.get_profile(resolved)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ValidityCase",
    "ProjectionCase",
    "MetamorphicRelation",
    "SubtestReporter",
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
    "assert_metamorphic_relations",
    "rejects_counterexample",
    "validity_matrix",
    "capability_matrix",
    "projection_matrix",
    "assert_ok",
    "assert_error",
    "assert_error_status",
    "assert_some",
    "assert_none",
    "assert_roundtrip",
    "run_state_machine",
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
