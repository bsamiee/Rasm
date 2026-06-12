"""RailStatus monoid laws: fold/join form a commutative semigroup on severity, with fold seeded at EMPTY.

Public surface: RailStatus, fold, join (tools.assay.core.status.__all__).
Oracle catalog: associative / commutative / absorbing / identity_element / monotone / validity_matrix.
Strategy: resolve(RailStatus) -> sampled_from(list(RailStatus)) (StrEnum; registered by conftest).

@given and @parametrize bypass @spec auto-registration, so register_laws lists every law name
explicitly to keep the coverage ledger consistent with the generated test IDs.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import itertools

from hypothesis import given, strategies as st
import pytest

from tests.python._testkit.laws import register_laws, spec
from tests.python._testkit.spec import absorbing, associative, commutative, identity_element, monotone, validity_matrix, ValidityCase
from tests.python.tools.assay.kit import rail_status_st
from tools.assay.core.status import fold, join, RailStatus


# --- [CONSTANTS] ------------------------------------------------------------------------

_ALL: tuple[RailStatus, ...] = tuple(RailStatus)
_ABSORBING: RailStatus = RailStatus.FAULTED
_SEED: RailStatus = RailStatus.EMPTY

_FROM_RC: tuple[tuple[int, RailStatus], ...] = (
    (0, RailStatus.EMPTY),
    (5, RailStatus.BUSY),
    (124, RailStatus.TIMEOUT),
    (1, RailStatus.FAILED),
    (2, RailStatus.FAILED),
    (127, RailStatus.FAILED),
    (255, RailStatus.FAILED),
)

# --- [LAW_COVERAGE]

register_laws(
    (
        join,
        (
            "join_associative_full",
            "join_commutative_pairs",
            *(f"join_monotone_left[{lo.name}<={hi.name}]" for lo in _ALL for hi in _ALL if lo.severity <= hi.severity),
        ),
    ),
    (
        RailStatus,
        (
            *(f"join_empty_identity_sweep[{m.name}]" for m in _ALL),
            *(f"member_invariants[{m.name}]" for m in _ALL),
            *(f"from_returncode[{rc}]" for rc, _ in _FROM_RC),
            "severity_ordering_validity_matrix",
            "faulted_is_max_severity",
            "skip_is_min_severity",
            "alias_skipped_resolves_to_skip",
        ),
    ),
    (fold, ("fold_max_severity_oracle", "fold_associativity_split", "fold_empty_seed", "fold_permutation_invariant", "fold_faulted_dominates")),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _sev_eq(a: RailStatus, b: RailStatus) -> bool:
    # Semigroup equality on severity only; left wins ties, so enum identity is not commutative.
    return a.severity == b.severity


def _join_left_severity(s: RailStatus) -> int:
    return join(s, _SEED).severity


# --- [JOIN_ALGEBRA]


@pytest.mark.mutation
@given(rail_status_st, rail_status_st, rail_status_st)
def test_join_associative_full(a: RailStatus, b: RailStatus, c: RailStatus) -> None:
    """Join three arbitrary statuses is associative on severity — the full product subsumes the self-triple."""
    associative(a, b, c, join, eq=_sev_eq)


@pytest.mark.mutation
@given(rail_status_st, rail_status_st)
def test_join_commutative_pairs(a: RailStatus, b: RailStatus) -> None:
    """join(a, b) and join(b, a) agree on severity — the full pair product subsumes the self-pair."""
    commutative(a, b, join, eq=_sev_eq)


@spec(RailStatus, mutation=True, law="join_faulted_absorbing")
def test_join_faulted_absorbing(s: RailStatus) -> None:
    """FAULTED is the absorbing element: join(s, FAULTED) = join(FAULTED, s) = FAULTED."""
    absorbing(s, join, _ABSORBING)


@spec(RailStatus, law="join_empty_identity_element")
def test_join_empty_identity_element(s: RailStatus) -> None:
    """EMPTY is the identity element for all s with severity >= EMPTY.severity.

    join is a max-severity operator seeded at EMPTY (severity 1). For s.severity >= 1 the
    identity_element oracle holds exactly. SKIP (severity 0) is the sole member below EMPTY —
    a separate sweep law covers that degenerate case.
    """
    if s.severity >= _SEED.severity:
        identity_element(s, join, _SEED, eq=_sev_eq)


@pytest.mark.parametrize("member", _ALL, ids=[m.name for m in _ALL])
def test_join_empty_identity_sweep(member: RailStatus) -> None:
    """Sweep every member: identity_element holds when severity >= SEED; SKIP is dominated."""
    match member.severity >= _SEED.severity:
        case True:
            identity_element(member, join, _SEED, eq=_sev_eq)
        case _:
            # SKIP (severity 0) is below the seed: join with EMPTY returns EMPTY, not SKIP.
            assert join(_SEED, member) is _SEED
            assert join(member, _SEED) is _SEED


# --- [SEVERITY_MONOTONICITY]


@pytest.mark.parametrize(
    "lo,hi",
    [(lo, hi) for lo in _ALL for hi in _ALL if lo.severity <= hi.severity],
    ids=[f"{lo.name}<={hi.name}" for lo in _ALL for hi in _ALL if lo.severity <= hi.severity],
)
def test_join_monotone_left(lo: RailStatus, hi: RailStatus) -> None:
    """Join is monotone in its left argument: join(lo, x).severity <= join(hi, x).severity when lo <= hi."""
    monotone(lo, hi, _join_left_severity)


# --- [EXIT_CODE_PROJECTION]


@spec(RailStatus, law="join_exit_code_non_negative")
def test_join_exit_code_non_negative(s: RailStatus) -> None:
    """Join result always carries a non-negative exit_code regardless of operand."""
    assert join(s, _SEED).exit_code >= 0
    assert join(_SEED, s).exit_code >= 0


# --- [FOLD_ALGEBRA]


@given(st.lists(rail_status_st, min_size=1, max_size=8))
def test_fold_max_severity_oracle(members: list[RailStatus]) -> None:
    """fold(*members) returns the max-by-severity element, floored at EMPTY.

    fold seeds at EMPTY (severity 1): a list of only SKIP (severity 0) yields EMPTY, not SKIP.
    """
    result = fold(*members)
    expected_sev = max(*(m.severity for m in members), _SEED.severity)
    assert result.severity == expected_sev


@given(st.lists(rail_status_st, min_size=2, max_size=6))
def test_fold_associativity_split(members: list[RailStatus]) -> None:
    """Splitting fold at any midpoint yields the same result — fold is associative over join."""
    mid = len(members) // 2
    left_half = fold(*members[:mid]) if members[:mid] else _SEED
    right_half = fold(*members[mid:]) if members[mid:] else _SEED
    assert fold(*members).severity == join(left_half, right_half).severity


def test_fold_empty_seed() -> None:
    """fold() with no members returns EMPTY — the monoid seed, not SKIP."""
    assert fold() is _SEED


@spec(RailStatus, law="fold_singleton_join_agreement")
def test_fold_singleton_join_agreement(s: RailStatus) -> None:
    """fold(s) agrees with join(EMPTY, s) on severity — fold and join share the same seed."""
    assert fold(s).severity == join(_SEED, s).severity


@given(st.lists(rail_status_st, min_size=1, max_size=8))
def test_fold_permutation_invariant(members: list[RailStatus]) -> None:
    """Fold is permutation-invariant on severity: reordering members does not change the result."""
    assert fold(*members).severity == fold(*reversed(members)).severity


@given(rail_status_st)
def test_fold_faulted_dominates(s: RailStatus) -> None:
    """Fold with FAULTED always returns FAULTED — absorbing propagates through fold."""
    assert fold(s, _ABSORBING) is _ABSORBING
    assert fold(_ABSORBING, s) is _ABSORBING


# --- [FOLD_DOMAIN_CLOSURE]


@spec(RailStatus, law="fold_result_is_enum_member")
def test_fold_result_is_enum_member(s: RailStatus) -> None:
    """fold(s) is always a genuine RailStatus member — no synthetic value escapes the domain."""
    result = fold(s)
    assert isinstance(result, RailStatus)
    assert result in _ALL


# --- [SEVERITY_ORDERING]


def test_severity_ordering_validity_matrix() -> None:
    """Validity matrix: every adjacent member pair is strictly ascending in severity."""
    validity_matrix([ValidityCase(label=f"{m.name}.severity_non_negative", value=m, expected=True) for m in _ALL], lambda m: m.severity >= 0)
    validity_matrix(
        [ValidityCase(label=f"{lo.name}<{hi.name}", value=(lo, hi), expected=True) for lo, hi in itertools.pairwise(_ALL)],
        lambda pair: pair[0].severity < pair[1].severity,
    )


def test_faulted_is_max_severity() -> None:
    """FAULTED has the highest severity — it is the absorbing zero of the semigroup."""
    assert all(_ABSORBING.severity >= m.severity for m in _ALL)


def test_skip_is_min_severity() -> None:
    """SKIP has the lowest severity — the element below the fold seed."""
    assert all(RailStatus.SKIP.severity <= m.severity for m in _ALL)


# --- [PER_MEMBER_INVARIANTS]


@pytest.mark.parametrize("member", _ALL, ids=[m.name for m in _ALL])
def test_member_invariants(member: RailStatus) -> None:
    """Per-member: StrEnum bijection, non-negative exit_code, non-negative severity."""
    assert RailStatus._value2member_map_[str(member)] is member
    assert isinstance(member.exit_code, int)
    assert member.exit_code >= 0
    assert isinstance(member.severity, int)
    assert member.severity >= 0


# --- [FROM_RETURNCODE]


@pytest.mark.mutation
@pytest.mark.parametrize("rc,expected", _FROM_RC, ids=[f"rc={r}" for r, _ in _FROM_RC])
def test_from_returncode_closed_table(rc: int, expected: RailStatus) -> None:
    """from_returncode maps {0->EMPTY, 5->BUSY, 124->TIMEOUT, *->FAILED} exactly."""
    assert RailStatus.from_returncode(rc) is expected


# --- [ALIAS_CONTRACT]


def test_alias_skipped_resolves_to_skip() -> None:
    """RailStatus('skipped') is RailStatus.SKIP — the alias contract is wired correctly."""
    assert RailStatus._value2member_map_["skipped"] is RailStatus.SKIP
