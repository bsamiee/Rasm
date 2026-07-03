"""Oracle falsification laws: every spec oracle proves on a lawful subject and fails on a witness."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import enum
import operator
from types import SimpleNamespace
from typing import TYPE_CHECKING

from expression import Error, Nothing, Ok, Some
from hypothesis import given, Phase, settings as hyp_settings, strategies as st
import msgspec
import pytest

from tests.python._testkit.spec import (
    absorbing,
    assert_error,
    assert_error_status,
    assert_none,
    assert_ok,
    assert_roundtrip,
    assert_some,
    associative,
    Bundle,
    commutative,
    consumes,
    distributive,
    idempotent,
    identity,
    identity_element,
    initialize,
    invariant,
    inverse,
    involution,
    metamorphic,
    metamorphic_sweep,
    MetamorphicRelation,
    model_based,
    monotone,
    multiple,
    permutation_invariant,
    precondition,
    projection_matrix,
    ProjectionCase,
    refutes,
    roundtrip,
    rule,
    RuleBasedStateMachine,
    support_matrix,
    target,
    validity_matrix,
    ValidityCase,
)


if TYPE_CHECKING:
    from collections.abc import Callable


# --- [CONSTANTS] ------------------------------------------------------------------------

type _Thunk = Callable[[], None]

# Row law: every algebraic oracle carries one lawful subject and one witness it must reject.
_ORACLES: tuple[tuple[str, _Thunk, _Thunk], ...] = (
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
    ("metamorphic", lambda: metamorphic(4, lambda n: n + n, lambda n: 2 * n), lambda: metamorphic(4, lambda n: n + n, lambda n: n * n)),
    ("custom-eq", lambda: identity(-1, abs, eq=lambda a, b: abs(a) == abs(b)), lambda: identity(-1, abs, eq=operator.is_)),
)

# Fast deterministic driver budget for the machine laws.
_MACHINE = hyp_settings(max_examples=15, stateful_step_count=15, deadline=None, database=None, derandomize=True)


# --- [MODELS] ---------------------------------------------------------------------------


class _Status(enum.StrEnum):
    DENIED = "denied"


class _Wire(msgspec.Struct, frozen=True):
    key: str
    rank: int = 0


class _Ledger(RuleBasedStateMachine):
    """Lawful counter machine: withdrawals guard, so the balance never underflows."""

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
        assert self.balance >= 0, f"balance underflowed: {self.balance}"


class _BrokenLedger(_Ledger):
    """Witness machine: one unguarded withdrawal rule drives the invariant negative."""

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
    def seeded(self) -> None:
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
        # A multiple() batch lands as individual bundle entries; a tuple drawn here is the seeded defect.
        assert isinstance(slot, str), f"bundle entry is not an individual slot: {slot!r}"
        self.live.discard(slot)
        self.retired.add(slot)

    @invariant()
    def partitions_stay_disjoint(self) -> None:
        assert not (self.live & self.retired), f"slot in both partitions: {self.live & self.retired}"


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ALGEBRAIC_ORACLES]


def _must_fail(label: str, witness: _Thunk) -> None:
    """Assert the witness thunk raises ``AssertionError``.

    Raises:
        AssertionError: When the witness survives — the oracle under test is a tautology.
    """
    try:
        witness()
    except AssertionError:
        return
    raise AssertionError(f"{label}: witness survived — the oracle is a tautology")


def test_every_algebraic_oracle_proves_and_refutes() -> None:
    """Each oracle row passes its lawful subject and raises on its witness."""
    for label, proves, fails in _ORACLES:
        proves()
        _must_fail(label, fails)


def test_refutes_is_bidirectional_and_transparent() -> None:
    """``refutes`` passes when the law fails, names the tautology when it holds, and never masks defects."""
    refutes(-1, lambda witness: identity(witness, abs))
    with pytest.raises(AssertionError, match="tautology"):
        refutes(1, lambda witness: identity(witness, abs))
    with pytest.raises(TypeError, match="len"):
        refutes(object(), lambda witness: identity(witness, len))


# --- [MATRIX_ORACLES]


def test_validity_matrix_accepts_both_row_shapes_and_names_the_breach() -> None:
    """Typed rows and raw tuples both fold; a wrong verdict surfaces its row label."""
    validity_matrix([ValidityCase(label="pos", value=3, expected=True), ValidityCase(label="zero", value=0, expected=False)], valid=lambda n: n > 0)
    validity_matrix([("neg", -3, False)], valid=lambda n: n > 0)
    with pytest.raises(AssertionError, match="wrong-row"):
        validity_matrix([("wrong-row", 3, False)], valid=lambda n: n > 0)


def test_projection_matrix_prefers_oracle_and_names_the_breach() -> None:
    """A present oracle computes the reference; ``supported_out`` covers the static rows."""
    cases = [
        ProjectionCase(label="derived", intent=4, supported_out=None, oracle=lambda n: n * 2, unsupported_out=None),
        ProjectionCase(label="static", intent=3, supported_out=6, oracle=None, unsupported_out=None),
    ]
    projection_matrix(cases, project=lambda n: n * 2)
    with pytest.raises(AssertionError, match="static"):
        projection_matrix([ProjectionCase(label="static", intent=3, supported_out=7, oracle=None, unsupported_out=None)], project=lambda n: n * 2)


def test_support_matrix_gates_each_probe() -> None:
    """Each probe row gates on its expected verdict and surfaces its label on breach."""
    support_matrix(("live", lambda: True, True), ("dead", lambda: False, False))
    with pytest.raises(AssertionError, match="flipped"):
        support_matrix(("flipped", lambda: True, False))


def test_metamorphic_sweep_enforces_every_relation() -> None:
    """Every relation must hold between source and follow-up outputs; one violation fails the sweep."""

    def _doubles(base: int, follow: int) -> None:
        assert follow == base * 2, f"scaling relation broke: {base} -> {follow}"

    def _breaks(base: int, follow: int) -> None:
        assert follow == base * 3, f"witness relation must fail: {base} -> {follow}"

    lawful = MetamorphicRelation[int, int](name="scaled", transform=lambda n: n * 2, relate=_doubles)
    metamorphic_sweep(5, lambda n: n, lawful)
    with pytest.raises(AssertionError, match="witness relation"):
        metamorphic_sweep(5, lambda n: n, lawful, MetamorphicRelation[int, int](name="broken", transform=lambda n: n * 2, relate=_breaks))


# --- [ROP_ORACLES]


def test_rail_asserts_unwrap_surface_and_refuse() -> None:
    """Rail asserts unwrap the matching case, run ``then`` callbacks, and name the mismatched case."""
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
    """The status check is ``is``: an equal-but-distinct token refuses, the singleton passes."""
    fault = SimpleNamespace(status=_Status.DENIED, code=_Status.DENIED)
    assert assert_error_status(Error(fault), _Status.DENIED) is fault
    assert_error_status(Error(fault), _Status.DENIED, attr="code")
    with pytest.raises(AssertionError, match="status"):
        assert_error_status(Error(SimpleNamespace(status="denied")), _Status.DENIED)


def test_assert_roundtrip_proves_byte_identity_and_fails_on_lossy_decode() -> None:
    """A wire struct round-trips byte-identically; a shape-changing decode fails the equality arm."""
    assert assert_roundtrip(_Wire(key="a", rank=2), _Wire) == _Wire(key="a", rank=2)
    with pytest.raises(AssertionError, match="decode mismatch"):
        assert_roundtrip((1, 2), list[int])


# --- [STATEFUL_MODEL_BASED]


def test_model_based_drives_and_falsifies_state_machines() -> None:
    """The driver passes a lawful machine and surfaces a broken invariant as a failure."""
    model_based(_Ledger, settings=_MACHINE)
    with pytest.raises(AssertionError, match="underflowed"):
        model_based(_BrokenLedger, settings=_MACHINE)


def test_model_based_composes_the_full_stateful_vocabulary() -> None:
    """initialize, precondition, Bundle, consumes, and multiple compose into one lawful lifecycle machine."""
    model_based(_Pool, settings=_MACHINE)


def test_target_steers_the_search_toward_extremal_observations() -> None:
    """The target phase hill-climbs measurably nearer a needle than the identical derandomized budget without it."""
    needle = 41733

    def nearest(phases: tuple[Phase, ...]) -> int:
        misses: list[int] = []

        @hyp_settings(max_examples=80, database=None, derandomize=True, deadline=None, phases=phases)
        @given(st.integers(min_value=0, max_value=1 << 20))
        def probe(n: int) -> None:
            misses.append(abs(n - needle))
            target(-float(abs(n - needle)))

        probe()
        return min(misses)

    assert nearest((Phase.generate, Phase.target)) < nearest((Phase.generate,)), "target phase produced no measurable hill-climb"
