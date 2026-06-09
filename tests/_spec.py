"""Oracle library — Python mirror and extension of tests/csharp/_testkit/Spec.cs.

Pure value-level oracles raising ``AssertionError`` on violation. Driven externally by
hypothesis ``@given`` / ``@parametrize``; this module never samples internally.

``eq`` default is structural ``==``; pass a custom callable for approximate equality.
Match on ``Result``/``Option`` variants uses the verified ``__match_args__`` forms:
``case Result(tag='ok', ok=v)`` etc. — ``Ok``/``Error``/``Some`` are factory functions,
not classes, so ``case Ok(...)`` raises ``TypeError``.

Extends Spec.cs: ``involution`` and ``absorbing`` have no C# counterpart;
``identity_element`` mirrors the private C# ``AdditiveIdentity`` helper.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable, Iterable  # noqa: TC003
import functools
import operator
from typing import overload, Protocol, Self

from dirty_equals import IsApprox
from expression import Option, Result
from hypothesis.stateful import Bundle, consumes, invariant, multiple, RuleBasedStateMachine
import msgspec
import msgspec.json


# --- [TYPES] ----------------------------------------------------------------------------

type _Eq[T] = Callable[[T, T], bool] | None
type _Cmp[T] = Callable[[T, T], int] | None


class _Comparable(Protocol):
    """Structural bound for ordered projection keys — any type whose ``<`` is defined."""

    def __lt__(self, other: Self, /) -> bool: ...


# --- [MODELS] ---------------------------------------------------------------------------


class ValidityCase[T](msgspec.Struct, frozen=True, gc=False):
    """Case carrier for :func:`validity_matrix`.

    Args:
        label: Human-readable case identifier surfaced on failure.
        value: The subject value under test.
        expected: Whether ``valid(value)`` should return ``True``.
    """

    label: str
    value: T
    expected: bool


class ProjectionCase[I](msgspec.Struct, frozen=True, gc=False):
    """Case carrier for :func:`projection_matrix`.

    Args:
        label: Human-readable case identifier surfaced on failure.
        intent: Input fed to ``project``.
        supported_out: Expected output when projection succeeds.
        oracle: Callable returning the reference output for comparison (``None`` → use ``supported_out`` directly).
        unsupported_out: Expected output when the projection is unsupported / falls back.
    """

    label: str
    intent: I
    supported_out: object
    oracle: Callable[[I], object] | None
    unsupported_out: object


class MetamorphicRelation[T, R](msgspec.Struct, frozen=True, gc=False):
    """A single metamorphic relation for :func:`metamorphic_sweep`.

    Models the literature's ``(input-transform, output-relation)`` pair: ``transform`` derives a
    follow-up input from the source, and ``relate`` asserts the required relation between the
    source output ``f(x)`` and the follow-up output ``f(transform(x))``, raising ``AssertionError``
    on violation (mirroring every other oracle body).

    Args:
        name: Human-readable relation identifier surfaced on failure.
        transform: Maps the source input to the follow-up input.
        relate: Asserts the relation between ``f(x)`` and ``f(transform(x))``; raises on violation.
    """

    name: str
    transform: Callable[[T], T]
    relate: Callable[[R, R], None]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _eq_law[T](left: T, right: T, eq: _Eq[T]) -> None:
    """Assert structural (or custom) equality, surfacing both sides on failure.

    The shared collapse point for every equality-shaped law body — mirrors Spec.cs ``EqLaw``.
    """
    assert (eq if eq is not None else operator.eq)(left, right), f"law violated: {left!r} != {right!r}"


def de_eq(*, delta: float | None = None) -> Callable[[float, float], bool]:
    """Build an approximate-equality ``eq`` callable over ``dirty_equals.IsApprox`` for the oracle ``eq`` kwarg.

    Args:
        delta: Absolute tolerance; ``None`` uses ``IsApprox``'s relative default.

    Returns:
        A two-arg predicate ``left == IsApprox(right, delta=delta)``. ``IsApprox.__eq__`` swallows
        ``TypeError``, so non-numeric operands fold to ``False`` rather than raising.
    """
    return lambda left, right: left == IsApprox(right, delta=delta)


# --- ALGEBRAIC ORACLES ------------------------------------------------------------------


def roundtrip[T, U](x: T, forward: Callable[[T], U], back: Callable[[U], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, back(forward(x)))`` — encode/decode identity."""
    _eq_law(x, back(forward(x)), eq)


def identity[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, f(x))`` — fixed-point under ``f``."""
    _eq_law(x, f(x), eq)


def idempotent[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(f(x), f(f(x)))`` — applying ``f`` twice is the same as once."""
    _eq_law(f(x), f(f(x)), eq)


def involution[T](x: T, f: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, f(f(x)))`` — ``f`` is its own inverse (no C# counterpart)."""
    _eq_law(x, f(f(x)), eq)


def inverse[T](x: T, f: Callable[[T], T], g: Callable[[T], T], *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(x, g(f(x)))`` — ``g`` is the left inverse of ``f``."""
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
    """Assert ``eq(op(x, zero), zero)`` and ``eq(op(zero, x), zero)`` (no C# counterpart)."""
    _eq_law(op(x, zero), zero, eq)
    _eq_law(op(zero, x), zero, eq)


def identity_element[T](x: T, op: Callable[[T, T], T], unit: T, *, eq: _Eq[T] = None) -> None:
    """Assert ``eq(op(unit, x), x)`` and ``eq(op(x, unit), x)`` — mirrors Spec.cs private AdditiveIdentity."""
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
    """Assert ``eq(f(original), f(shuffled))``; caller draws ``shuffled`` via ``st.permutations`` under ``@given``."""
    _eq_law(f(original), f(shuffled), eq)


def metamorphic[T, R](x: T, path: Callable[[T], R], oracle: Callable[[T], R], *, eq: _Eq[R] = None) -> None:
    """Assert ``eq(path(x), oracle(x))`` — two computation paths must agree on the same input."""
    _eq_law(path(x), oracle(x), eq)


def metamorphic_sweep[T, R](x: T, f: Callable[[T], R], *relations: MetamorphicRelation[T, R]) -> None:
    """Assert every :class:`MetamorphicRelation` holds between ``f(x)`` and ``f(relation.transform(x))``.

    Generalizes :func:`metamorphic` from one path-vs-oracle agreement to an N-relation fold over the
    ``(input-transform, output-relation)`` form: each relation's ``relate`` is called with the source
    output and the follow-up output, raising ``AssertionError`` on the first violation.

    Args:
        x: The source input.
        f: The function under test.
        *relations: Metamorphic relations to enforce against ``f(x)``.
    """
    base = f(x)
    functools.reduce(lambda _, r: r.relate(base, f(r.transform(x))), relations, None)


def refutes[T](witness: T, law: Callable[..., None], *args: object, **kwargs: object) -> None:
    """Assert ``law`` *raises* ``AssertionError`` on a known-broken ``witness`` — the falsification oracle.

    Inverse of every other oracle: where they prove a law HOLDS, this proves a law is FALSIFIABLE.
    A law with no falsifying witness is a tautology a surviving mutant exploits unobserved; this is the
    in-engine bridge from ``_spec`` to the mutation lane (``refutes(broken_x, idempotent, increment)``
    proves ``idempotent`` actually catches a non-idempotent ``f``).

    Args:
        witness: A value (or first positional) for which ``law`` is expected to fail.
        law: An oracle from this module invoked as ``law(witness, *args, **kwargs)``.
        *args: Trailing positional arguments forwarded to ``law``.
        **kwargs: Keyword arguments forwarded to ``law``.

    Raises:
        AssertionError: When ``law`` does NOT raise on ``witness`` (the law is a tautology).
    """
    try:
        law(witness, *args, **kwargs)
    except AssertionError:
        return  # the law rejected the broken witness — falsifiability proven
    raise AssertionError("law is a tautology — a surviving mutant exploits it")


# --- MATRIX ORACLES ---------------------------------------------------------------------


@overload
def validity_matrix[T](cases: Iterable[ValidityCase[T]], valid: Callable[[T], bool]) -> None: ...


@overload
def validity_matrix[T](cases: Iterable[tuple[str, T, bool]], valid: Callable[[T], bool]) -> None: ...


def validity_matrix[T](cases: Iterable[ValidityCase[T]] | Iterable[tuple[str, T, bool]], valid: Callable[[T], bool]) -> None:
    """Assert each case's expected validity; mirrors Spec.cs ``ValidityMatrix``.

    Args:
        cases: ``ValidityCase[T]`` instances or raw ``(label, value, expected)`` tuples.
        valid: Predicate returning ``True`` when the value is valid.
    """
    for raw in cases:
        case_ = raw if isinstance(raw, ValidityCase) else ValidityCase(label=raw[0], value=raw[1], expected=raw[2])
        actual = valid(case_.value)
        assert actual == case_.expected, f"validity_matrix[{case_.label!r}]: expected {case_.expected}, got {actual} for {case_.value!r}"


def support_matrix(*rows: tuple[str, Callable[[], bool], bool]) -> None:
    """Assert each labeled probe returns the expected boolean; collapses anonymous assertion walls.

    Args:
        *rows: Triples of ``(label, probe, expected)`` where ``probe`` is a zero-arg callable.
    """
    for label, probe, expected in rows:
        actual = probe()
        assert actual == expected, f"support_matrix[{label!r}]: expected {expected}, got {actual}"


def projection_matrix[I](cases: Iterable[ProjectionCase[I]], project: Callable[[I], object]) -> None:
    """Assert each case's projection outcome; mirrors Spec.cs ``ProjectionMatrix``.

    The ``oracle`` field, when present, is called with ``intent`` to compute the reference;
    otherwise ``supported_out`` is used directly.

    Args:
        cases: ``ProjectionCase[I]`` instances describing each projection scenario.
        project: The function under test mapping intent to output.
    """
    for case_ in cases:
        actual = project(case_.intent)
        reference = case_.oracle(case_.intent) if case_.oracle is not None else case_.supported_out
        assert actual == reference, f"projection_matrix[{case_.label!r}]: expected {reference!r}, got {actual!r} (intent={case_.intent!r})"


# --- ROP ORACLES ------------------------------------------------------------------------

_DEFAULT_ENCODER: msgspec.json.Encoder = msgspec.json.Encoder(order="deterministic")


def assert_ok[T, E](result: Result[T, E], *, then: Callable[[T], None] | None = None) -> T:
    """Assert ``Result`` is ``Ok`` and return the inner value; surfaces the error on failure.

    Args:
        result: The ``Result`` under test.
        then: Optional callback invoked with the unwrapped value for additional assertions.

    Returns:
        The unwrapped ``Ok`` value.

    Raises:
        AssertionError: When the result is ``Error`` or has an unexpected shape.
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
    """Assert ``Result`` is ``Error`` and return the error; surfaces the Ok value on failure.

    Args:
        result: The ``Result`` under test.
        then: Optional callback invoked with the unwrapped error for additional assertions.

    Returns:
        The unwrapped ``Error`` value.

    Raises:
        AssertionError: When the result is ``Ok`` or has an unexpected shape.
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
    """Assert ``Error`` and that ``getattr(error, attr) is status``; generalizes ``assert_result_status``.

    Reads the status attribute generically so it serves any ``Result[_, E]`` where ``E``
    carries a status-bearing field — not only assay ``Fault``.

    Args:
        result: The ``Result`` under test.
        status: The expected status sentinel (identity check via ``is``).
        attr: Attribute name on the error object carrying the status (default ``"status"``).

    Returns:
        The unwrapped ``Error`` value.
    """
    e = assert_error(result)
    actual = getattr(e, attr)
    assert actual is status, f"expected {attr}={status!r}, got {actual!r}"
    return e


def assert_some[T](opt: Option[T], *, then: Callable[[T], None] | None = None) -> T:
    """Assert ``Option`` is ``Some`` and return the inner value.

    Args:
        opt: The ``Option`` under test.
        then: Optional callback invoked with the unwrapped value for additional assertions.

    Returns:
        The unwrapped ``Some`` value.

    Raises:
        AssertionError: When the option is ``None`` or has an unexpected shape.
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
    """Assert ``Option`` is ``None`` (absent).

    Args:
        opt: The ``Option`` under test.

    Raises:
        AssertionError: When the option is ``Some`` or has an unexpected shape.
    """
    match opt:
        case Option(tag="none"):
            return
        case Option(tag="some", some=v):
            raise AssertionError(f"expected None, got Some({v!r})")
        case _:
            raise AssertionError(f"unexpected Option shape: {opt!r}")


def assert_roundtrip[T](value: T, typ: type[T], *, codec: msgspec.json.Encoder | None = None) -> T:
    """Assert deterministic encode → decode → re-encode byte-identity for a wire value.

    The re-encode step catches non-deterministic codecs that a plain ``==`` comparison
    would miss (e.g. dict-bearing structs with non-stable key ordering).

    Args:
        value: The value to encode and round-trip.
        typ: The target type for ``msgspec.json.decode``.
        codec: Encoder to use; defaults to ``msgspec.json.Encoder(order='deterministic')``.

    Returns:
        The decoded value (structurally equal to ``value``).
    """
    enc = codec if codec is not None else _DEFAULT_ENCODER
    raw = enc.encode(value)
    decoded: T = msgspec.json.decode(raw, type=typ)
    assert decoded == value, f"decode mismatch for {typ.__name__}: {decoded!r} != {value!r}"
    reencoded = enc.encode(decoded)
    assert reencoded == raw, f"re-encode not byte-identical for {typ.__name__}: {reencoded!r} != {raw!r}"
    return decoded


# --- STATEFUL / MODEL-BASED -------------------------------------------------------------


def model_based[M: RuleBasedStateMachine](machine_cls: type[M]) -> type[M]:
    """Return ``machine_cls`` as a collectable pytest test class via hypothesis stateful machinery.

    Callers define ``@initialize``/``@rule``/``@invariant`` on their ``RuleBasedStateMachine``
    subclass, then assign the result at module level::

        SomeMachineTest = model_based(SomeMachine)

    Args:
        machine_cls: A ``RuleBasedStateMachine`` subclass with rules and invariants defined.

    Returns:
        The same class, now usable as a module-level test case binding for hypothesis collection.
    """
    return machine_cls


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ValidityCase",
    "ProjectionCase",
    "MetamorphicRelation",
    "de_eq",
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
    "consumes",
    "invariant",
    "multiple",
]
