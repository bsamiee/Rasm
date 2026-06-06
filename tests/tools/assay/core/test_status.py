"""RailStatus algebra: join/fold oracle, severity ordering, from_returncode table, alias contract."""

# --- [IMPORTS] ------------------------------------------------------------------------

from hypothesis import given
from hypothesis.strategies import lists, sampled_from
import pytest

from tools.assay.core.status import fold, join, RailStatus


# --- [CONSTANTS] -----------------------------------------------------------------------

_ALL = list(RailStatus)
_ABSORBING = RailStatus.FAULTED
_SEED = RailStatus.EMPTY
_FROM_RC: tuple[tuple[int, RailStatus], ...] = (
    (0, RailStatus.EMPTY),
    (5, RailStatus.BUSY),
    (124, RailStatus.TIMEOUT),
    (1, RailStatus.FAILED),
    (2, RailStatus.FAILED),
    (127, RailStatus.FAILED),
    (255, RailStatus.FAILED),
)


# --- [ALGEBRAIC] -----------------------------------------------------------------------


@given(sampled_from(_ALL), sampled_from(_ALL))
def test_join_max_severity_oracle(left: RailStatus, right: RailStatus) -> None:
    """``join`` returns the operand with the strictly greater severity; ties keep left."""
    result = join(left, right)
    assert result.severity == max(left.severity, right.severity)
    assert result is left if left.severity >= right.severity else right


@given(sampled_from([s for s in _ALL if s.severity >= _SEED.severity]))
def test_join_seed_dominated_by_non_skip(s: RailStatus) -> None:
    """``join(EMPTY, s) is s`` when ``s.severity >= EMPTY.severity`` — EMPTY dominates only SKIP."""
    assert join(_SEED, s) is s


def test_join_seed_dominates_skip() -> None:
    """``join(EMPTY, SKIP) is EMPTY`` — EMPTY (severity 1) > SKIP (severity 0); SKIP is NOT an absorbing zero."""
    assert join(_SEED, RailStatus.SKIP) is _SEED


@given(sampled_from(_ALL))
def test_join_absorbing_element(s: RailStatus) -> None:
    """``join(s, FAULTED) is FAULTED`` — FAULTED absorbs all."""
    assert join(s, _ABSORBING) is _ABSORBING


@given(sampled_from(_ALL))
def test_join_faulted_absorbs_left(s: RailStatus) -> None:
    """``join(FAULTED, s) is FAULTED`` — absorbing from both sides."""
    assert join(_ABSORBING, s) is _ABSORBING


@given(lists(sampled_from(_ALL), min_size=1, max_size=8))
def test_fold_max_severity_oracle(members: list[RailStatus]) -> None:
    """``fold(*members)`` returns max-by-severity, floored at EMPTY (the seed).

    ``fold`` seeds at ``EMPTY`` (severity 1), so a list of only ``SKIP`` (severity 0) yields ``EMPTY``,
    not ``SKIP``. The oracle mirrors this: ``max(member_severity, EMPTY.severity)``.
    """
    result = fold(*members)
    expected_severity = max(*(m.severity for m in members), _SEED.severity)
    assert result.severity == expected_severity


def test_fold_empty_seed() -> None:
    """``fold()`` with no members returns ``EMPTY`` (the monoid seed, not ``SKIP``)."""
    assert fold() is _SEED


@given(sampled_from(_ALL))
def test_fold_singleton_identity(s: RailStatus) -> None:
    """``fold(s)`` equals ``join(EMPTY, s)`` — fold and join agree on single members."""
    assert fold(s).severity == join(_SEED, s).severity


@given(lists(sampled_from(_ALL), min_size=2, max_size=6))
def test_fold_associativity(members: list[RailStatus]) -> None:
    """Splitting the fold at any midpoint yields the same severity — fold is associative."""
    mid = len(members) // 2
    left_half = fold(*members[:mid]) if members[:mid] else _SEED
    right_half = fold(*members[mid:]) if members[mid:] else _SEED
    assert fold(*members).severity == join(left_half, right_half).severity


# --- [EDGE_CASES] -----------------------------------------------------------------------


@pytest.mark.parametrize("rc,expected", _FROM_RC)
def test_from_returncode_closed_table(rc: int, expected: RailStatus) -> None:
    """``from_returncode`` maps {0→EMPTY, 5→BUSY, 124→TIMEOUT, *→FAILED}."""
    assert RailStatus.from_returncode(rc) is expected


@pytest.mark.parametrize("member", _ALL)
def test_member_invariants(member: RailStatus) -> None:
    """Per-member invariants: StrEnum bijection, non-negative exit_code, non-negative severity."""
    # StrEnum round-trip
    assert RailStatus._value2member_map_[str(member)] is member
    # exit_code is a non-negative int
    assert isinstance(member.exit_code, int)
    assert member.exit_code >= 0
    # severity is a non-negative int
    assert isinstance(member.severity, int)
    assert member.severity >= 0


def test_alias_skipped_resolves_to_skip() -> None:
    """``RailStatus("skipped") is RailStatus.SKIP`` — the alias contract is wired correctly."""
    assert RailStatus._value2member_map_["skipped"] is RailStatus.SKIP


def test_faulted_is_max_severity() -> None:
    """``FAULTED`` has the highest severity of all members — it is the absorbing element."""
    assert all(_ABSORBING.severity >= m.severity for m in _ALL)


def test_skip_is_min_severity() -> None:
    """``SKIP`` has the lowest severity — the zero element below EMPTY."""
    assert all(RailStatus.SKIP.severity <= m.severity for m in _ALL)
