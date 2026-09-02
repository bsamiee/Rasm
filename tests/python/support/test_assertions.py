"""Tests for reusable assertions, including known passing and failing cases."""

# --- [IMPORTS] --------------------------------------------------------------------------

from contextlib import contextmanager
import enum
import operator
from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Error, Nothing, Ok, Some
from expression.collections import Block
from hypothesis import given, Phase, settings as hyp_settings, strategies as st
import msgspec
import pytest
lazy import numpy as np

from tests.python.support.assertions import (
    absorbing,
    assert_close,
    assert_error,
    assert_error_status,
    assert_metamorphic_relations,
    assert_none,
    assert_ok,
    assert_roundtrip,
    assert_some,
    associative,
    Bundle,
    capability_matrix,
    close,
    commutative,
    consumes,
    differential,
    distributive,
    idempotent,
    identity,
    identity_element,
    initialize,
    invariant,
    inverse,
    involution,
    MetamorphicRelation,
    monotone,
    MSGPACK_ENCODER,
    multiple,
    permutation_invariant,
    precondition,
    projection_matrix,
    ProjectionCase,
    rejects_counterexample,
    roundtrip,
    rule,
    RuleBasedStateMachine,
    run_state_machine,
    target,
    validity_matrix,
    ValidityCase,
)

if TYPE_CHECKING:
    from collections.abc import Callable, Generator

    from tests.python.support.assertions import SubtestReporter


# --- [CONSTANTS] ------------------------------------------------------------------------

type _Thunk = Callable[[], None]

_ASSERTION_CASES: tuple[tuple[str, _Thunk, _Thunk], ...] = (
    ("roundtrip", lambda: roundtrip(7, str, int), lambda: roundtrip(7, str, lambda s: int(s) + 1)),
    ("identity", lambda: identity(3, abs), lambda: identity(-3, abs)),
    ("idempotent", lambda: idempotent(-3, abs), lambda: idempotent(2, lambda n: n + 1)),
    ("involution", lambda: involution(5, operator.neg), lambda: involution(5, lambda n: n + 1)),
    ("inverse", lambda: inverse(7, lambda n: n * 2, lambda n: n // 2), lambda: inverse(7, lambda n: n * 2, lambda n: n // 3)),
    ("commutative", lambda: commutative(3, 4, operator.add), lambda: commutative(3, 4, operator.sub)),
    ("associative", lambda: associative(1, 2, 3, operator.add), lambda: associative(1, 2, 3, operator.sub)),
    ("distributive", lambda: distributive(2, 3, 4, operator.mul, operator.add), lambda: distributive(2, 3, 4, operator.add, operator.mul)),
    ("absorbing", lambda: absorbing(9, operator.mul, 0), lambda: absorbing(9, operator.add, 0)),
    ("identity_element", lambda: identity_element(9, operator.add, 0), lambda: identity_element(9, operator.add, 1)),
    ("monotone", lambda: monotone(2, 5, lambda n: n * n), lambda: monotone(2, 5, operator.neg)),
    (
        "permutation_invariant",
        lambda: permutation_invariant((1, 2, 3), (3, 2, 1), sorted),
        lambda: permutation_invariant((1, 2, 3), (3, 2, 1), tuple),
    ),
    ("differential", lambda: differential(4, lambda n: n + n, lambda n: 2 * n), lambda: differential(4, lambda n: n + n, lambda n: n * n)),
    ("custom-eq", lambda: identity(-1, abs, eq=lambda a, b: abs(a) == abs(b)), lambda: identity(-1, abs, eq=operator.is_)),
)

_MACHINE = hyp_settings(max_examples=15, stateful_step_count=15, deadline=None, database=None, derandomize=True)


# --- [MODELS] ---------------------------------------------------------------------------


class _Status(enum.StrEnum):
    DENIED = "denied"


class _VersionedRecord(msgspec.Struct, frozen=True):
    key: str
    version: int = 0


class _SubtestRecorder:
    """Record subtest labels and assertion failures."""

    def __init__(self) -> None:
        self.labels: list[str | None] = []
        self.failures: list[str | None] = []

    @contextmanager
    def test(self, msg: str | None = None, **kwargs: object) -> Generator[None]:
        _ = kwargs
        self.labels.append(msg)
        try:
            yield
        except AssertionError:
            self.failures.append(msg)


class _Ledger(RuleBasedStateMachine):
    """State machine that guards withdrawals to keep a nonnegative balance."""

    def __init__(self) -> None:
        super().__init__()
        self.balance = 0

    @rule(amount=st.integers(min_value=1, max_value=9))
    def deposit(self, amount: int) -> None:
        self.balance += amount

    @rule(amount=st.integers(min_value=1, max_value=9))
    def withdraw(self, amount: int) -> None:
        self.balance -= min(amount, self.balance)

    @invariant()
    def non_negative(self) -> None:
        assert self.balance >= 0, f"balance became negative: {self.balance}"


class _BrokenLedger(_Ledger):
    """State machine with an unguarded withdrawal that violates the invariant."""

    @rule(amount=st.integers(min_value=1, max_value=9))
    def withdraw_unguarded(self, amount: int) -> None:
        self.balance -= amount


class _Pool(RuleBasedStateMachine):
    """Bundle lifecycle machine composing initialize, precondition, Bundle, consumes, and multiple."""

    slots = Bundle("slots")

    def __init__(self) -> None:
        super().__init__()
        self.counter = 0
        self.live: set[str] = set()
        self.retired: set[str] = set()

    @initialize()
    def initialized(self) -> None:
        self.counter = 0

    @rule(target=slots)
    def created(self) -> str:
        self.counter += 1
        name = f"slot-{self.counter}"
        self.live.add(name)
        return name

    @rule(target=slots)
    def created_pair(self) -> object:
        self.counter += 2
        pair = (f"slot-{self.counter - 1}", f"slot-{self.counter}")
        self.live.update(pair)
        return multiple(*pair)

    @precondition(lambda self: bool(self.live))
    @rule(slot=consumes(slots))
    def retired_slot(self, slot: str) -> None:
        assert isinstance(slot, str), f"bundle entry is not an individual slot: {slot!r}"
        self.live.discard(slot)
        self.retired.add(slot)

    @invariant()
    def partitions_stay_disjoint(self) -> None:
        assert not (self.live & self.retired), f"slot in both partitions: {self.live & self.retired}"


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ALGEBRAIC_PROPERTIES]


def _must_fail(label: str, failing_case: _Thunk) -> None:
    """Assert the known failing case raises ``AssertionError``.

    Raises:
        AssertionError: The assertion accepts the failing case.
    """
    try:
        failing_case()
    except AssertionError:
        return
    raise AssertionError(f"{label}: assertion accepted the known failing case")


def test_every_algebraic_assertion_accepts_valid_and_rejects_invalid_cases() -> None:
    """Each assertion accepts its valid case and rejects its invalid case."""
    for label, passes, fails in _ASSERTION_CASES:
        passes()
        _must_fail(label, fails)


def test_rejects_counterexample_requires_property_failure_and_preserves_other_exceptions() -> None:
    """``rejects_counterexample`` requires an assertion failure and does not hide unrelated exceptions."""
    rejects_counterexample(-1, lambda counterexample: identity(counterexample, abs))
    with pytest.raises(AssertionError, match="accepts its counterexample"):
        rejects_counterexample(1, lambda counterexample: identity(counterexample, abs))
    with pytest.raises(TypeError, match="len"):
        rejects_counterexample(object(), lambda counterexample: identity(counterexample, len))


# --- [TABLE_DRIVEN_ASSERTIONS]


def test_validity_matrix_accepts_structs_and_tuples_and_reports_the_failed_case() -> None:
    """Typed cases and tuples are accepted, and a wrong result reports its case label."""
    validity_matrix([ValidityCase(label="pos", value=3, expected=True), ValidityCase(label="zero", value=0, expected=False)], valid=lambda n: n > 0)
    validity_matrix([("neg", -3, False)], valid=lambda n: n > 0)
    with pytest.raises(AssertionError, match="wrong-row"):
        validity_matrix([("wrong-row", 3, False)], valid=lambda n: n > 0)


def test_projection_matrix_prefers_reference_function_and_reports_the_failed_case() -> None:
    """Reference functions compute expected results, other cases use fixed expected values."""
    cases = [
        ProjectionCase(label="derived", intent=4, expected=None, reference=lambda n: n * 2),
        ProjectionCase(label="static", intent=3, expected=6, reference=None),
    ]
    projection_matrix(cases, project=lambda n: n * 2)
    with pytest.raises(AssertionError, match="static"):
        projection_matrix([ProjectionCase(label="static", intent=3, expected=7, reference=None)], project=lambda n: n * 2)


def test_capability_matrix_checks_each_case() -> None:
    """Each capability case compares its observed and expected result."""
    capability_matrix(("live", lambda: True, True), ("dead", lambda: False, False))
    with pytest.raises(AssertionError, match="flipped"):
        capability_matrix(("flipped", lambda: True, False))


def test_matrices_report_independent_subtests_and_stop_without_reporter(subtests: SubtestReporter) -> None:
    """Subtest reporters record every case, without them the first assertion failure stops execution."""
    recorder = _SubtestRecorder()
    validity_matrix([("first", 1, True), ("broken", -1, True), ("last", 2, True)], valid=lambda n: n > 0, subtests=recorder)
    assert recorder.labels == ["first", "broken", "last"], f"subtest reporting stopped before all cases ran: {recorder.labels}"
    assert recorder.failures == ["broken"], f"subtest reporter recorded the wrong failures: {recorder.failures}"
    capability_matrix(("hit", lambda: True, True), ("miss", lambda: True, False), subtests=recorder)
    projection_matrix([ProjectionCase(label="off", intent=3, expected=7, reference=None)], project=lambda n: n * 2, subtests=recorder)
    assert recorder.failures == ["broken", "miss", "off"], f"subtest reporter missed failures: {recorder.failures}"

    calls: list[int] = []
    with pytest.raises(AssertionError, match="broken"):
        validity_matrix([("broken", -1, True), ("after", 2, True)], valid=lambda n: (calls.append(n), n > 0)[-1])
    assert calls == [-1], f"matrix continued after the first failure without a subtest reporter: {calls}"

    validity_matrix([("live", 3, True)], valid=lambda n: n > 0, subtests=subtests)


def test_matrix_assertions_require_nonempty_case_sets() -> None:
    """Table-driven assertions reject empty case and relation sets."""
    with pytest.raises(AssertionError, match="at least one case"):
        validity_matrix([], valid=lambda n: n > 0)
    with pytest.raises(AssertionError, match="at least one case"):
        capability_matrix()
    with pytest.raises(AssertionError, match="at least one case"):
        projection_matrix([], project=lambda n: n)
    with pytest.raises(AssertionError, match="at least one relation"):
        assert_metamorphic_relations(1, lambda n: n)


def test_metamorphic_assertion_enforces_every_relation() -> None:
    """Every relation must hold between source and follow-up outputs, any violation fails the assertion."""

    def _doubles(base: int, follow: int) -> None:
        assert follow == base * 2, f"scaling relation failed: {base} -> {follow}"

    def _breaks(base: int, follow: int) -> None:
        assert follow == base * 3, f"known-invalid relation must fail: {base} -> {follow}"

    scaling = MetamorphicRelation[int, int](name="scaled", transform=lambda n: n * 2, relate=_doubles)
    assert_metamorphic_relations(5, lambda n: n, scaling)
    with pytest.raises(AssertionError, match="known-invalid relation"):
        assert_metamorphic_relations(5, lambda n: n, scaling, MetamorphicRelation[int, int](name="broken", transform=lambda n: n * 2, relate=_breaks))


# --- [TOLERANCE_ORACLES]


class _Reading(msgspec.Struct, frozen=True):
    label: str
    values: tuple[float, ...]


def test_close_dispatches_every_supported_value_type_and_names_the_diverging_path() -> None:
    """The tolerance policy compares numbers, arrays, quantities, structs, results, mappings, and sequences and reports the differing path."""
    assert_close(1.0, 1.0 + 1e-12)
    assert_close(float("nan"), float("nan"))
    assert_close(float("inf"), float("inf"))
    assert_close({"k": (1.0, 2.0)}, {"k": (1.0, 2.0 + 1e-12)})
    assert_close(_Reading(label="a", values=(0.1, 0.2)), _Reading(label="a", values=(0.1, 0.2 + 1e-12)))
    assert_close(np.array([[1.0, 2.0], [3.0, 4.0]]), np.array([[1.0, 2.0], [3.0, 4.0 + 1e-12]]))
    assert_close(SimpleNamespace(units="mm", magnitude=2.0), SimpleNamespace(units="mm", magnitude=2.0 + 1e-12))
    assert_close(2, 2.5, abs_tol=0.5)

    with pytest.raises(AssertionError, match=r"\$\.values\[1\]"):
        assert_close(_Reading(label="a", values=(0.1, 0.2)), _Reading(label="a", values=(0.1, 0.9)))
    with pytest.raises(AssertionError, match=r"\$\[1, 1\]"):
        assert_close(np.array([[1.0, 2.0], [3.0, 4.0]]), np.array([[1.0, 2.0], [3.0, 9.0]]))
    with pytest.raises(AssertionError, match="shape"):
        assert_close(np.zeros(3), np.zeros(4))
    with pytest.raises(AssertionError, match="units"):
        assert_close(SimpleNamespace(units="mm", magnitude=2.0), SimpleNamespace(units="m", magnitude=2.0))
    with pytest.raises(AssertionError, match="key sets"):
        assert_close({"k": 1.0}, {"other": 1.0})
    with pytest.raises(AssertionError, match="length"):
        assert_close([1.0], [1.0, 2.0])
    flag = True
    with pytest.raises(AssertionError, match=r"\$:"):
        assert_close(flag, 1)


def test_close_recurses_results_and_blocks_and_reports_the_diverging_case() -> None:
    """Result, option, and ``Block`` values compare recursively and report tag or value differences."""
    assert_close(Ok(_Reading(label="a", values=(0.1,))), Ok(_Reading(label="a", values=(0.1 + 1e-12,))))
    assert_close(Error((1.0, 2.0)), Error((1.0, 2.0 + 1e-12)))
    assert_close(Some(1.0), Some(1.0 + 1e-12))
    assert_close(Nothing, Nothing)
    assert_close(Block.of_seq([1.0, 2.0]), Block.of_seq([1.0, 2.0 + 1e-12]))

    with pytest.raises(AssertionError, match=r"\$\.ok\.values\[0\]"):
        assert_close(Ok(_Reading(label="a", values=(0.1,))), Ok(_Reading(label="a", values=(0.9,))))
    with pytest.raises(AssertionError, match="result tags differ"):
        assert_close(Ok(1.0), Error(1.0))
    with pytest.raises(AssertionError, match="result tags differ"):
        assert_close(Some(1.0), Nothing)
    with pytest.raises(AssertionError, match=r"\$\[1\]"):
        assert_close(Block.of_seq([1.0, 2.0]), Block.of_seq([1.0, 9.0]))
    with pytest.raises(AssertionError, match="length"):
        assert_close(Block.of_seq([1.0]), Block.of_seq([1.0, 2.0]))


def test_close_comparator_applies_to_algebraic_assertions_and_counterexamples() -> None:
    """The approximate comparator works with algebraic assertions and known counterexamples."""
    offset = lambda x: x + 1e-12  # ruff:ignore[lambda-assignment]
    with pytest.raises(AssertionError, match="property failed"):
        identity(1.0, offset)
    identity(1.0, offset, eq=close())
    rejects_counterexample(1.0, identity, lambda x: x + 0.5, eq=close())


# --- [RESULT_ASSERTIONS]


def test_result_assertions_unwrap_expected_cases_and_reject_mismatches() -> None:
    """Result asserts unwrap the matching case, run ``then`` callbacks, and name the mismatched case."""
    seen: list[int] = []
    assert assert_ok(Ok(3)) == 3
    assert_ok(Ok(4), then=seen.append)
    assert seen == [4]
    with pytest.raises(AssertionError, match="Error"):
        assert_ok(Error("boom"))
    assert assert_error(Error("boom")) == "boom"
    with pytest.raises(AssertionError, match="Ok"):
        assert_error(Ok(1))
    assert assert_some(Some(5)) == 5
    with pytest.raises(AssertionError, match="Some"):
        assert_some(Nothing)
    assert_none(Nothing)
    with pytest.raises(AssertionError, match="Some"):
        assert_none(Some(1))


def test_assert_error_status_matches_by_identity_not_equality() -> None:
    """The status check uses identity: an equal but distinct token fails, while the singleton passes."""
    error = SimpleNamespace(status=_Status.DENIED, code=_Status.DENIED)
    assert assert_error_status(Error(error), _Status.DENIED) is error
    assert_error_status(Error(error), _Status.DENIED, attr="code")
    with pytest.raises(AssertionError, match="status"):
        assert_error_status(Error(SimpleNamespace(status="denied")), _Status.DENIED)


def test_assert_roundtrip_proves_byte_identity_and_fails_on_lossy_decode() -> None:
    """Serialized structs round-trip byte-identically, a type-changing decode fails equality."""
    assert assert_roundtrip(_VersionedRecord(key="a", version=2), _VersionedRecord) == _VersionedRecord(key="a", version=2)
    assert assert_roundtrip(_VersionedRecord(key="a", version=2), _VersionedRecord, encoder=MSGPACK_ENCODER) == _VersionedRecord(key="a", version=2)
    with pytest.raises(AssertionError, match="decode mismatch"):
        assert_roundtrip((1, 2), list[int])
    with pytest.raises(AssertionError, match="decode mismatch"):
        assert_roundtrip((1, 2), list[int], encoder=MSGPACK_ENCODER)


# --- [STATEFUL_TESTING]


def test_run_state_machine_accepts_valid_and_rejects_invalid_machines() -> None:
    """The runner accepts a valid state machine and reports a broken invariant."""
    run_state_machine(_Ledger, settings=_MACHINE)
    with pytest.raises(AssertionError, match="became negative"):
        run_state_machine(_BrokenLedger, settings=_MACHINE)


def test_run_state_machine_supports_hypothesis_stateful_primitives() -> None:
    """``initialize``, ``precondition``, ``Bundle``, ``consumes``, and ``multiple`` support a resource lifecycle."""
    run_state_machine(_Pool, settings=_MACHINE)


def test_target_moves_the_search_toward_the_objective() -> None:
    """The target phase approaches a chosen numeric objective more closely than generation alone."""
    objective = 41733

    def nearest(phases: tuple[Phase, ...]) -> int:
        misses: list[int] = []

        @hyp_settings(max_examples=80, database=None, derandomize=True, deadline=None, phases=phases)
        @given(st.integers(min_value=0, max_value=1 << 20))
        def observe(n: int) -> None:
            misses.append(abs(n - objective))
            target(-float(abs(n - objective)))

        observe()
        return min(misses)

    assert nearest((Phase.generate, Phase.target)) < nearest((Phase.generate,)), "target phase produced no measurable hill-climb"
