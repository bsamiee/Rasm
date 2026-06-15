"""Laws for ``tools.assay.automation.model``.

Covers action/trigger codec round-trips, describe projections, wire validation, and WatchFilter
coverage through the public encode/decode surfaces.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from hypothesis import example, given, strategies as st
import msgspec
import pytest

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import roundtrip
from tests.python._testkit.strategies import resolve
from tools.assay.automation.model import (
    Action,
    ACTION_DECODER,
    Debounce,
    decode,
    describe,
    Edge,
    encode,
    Manual,
    Program,
    Rail,
    Schedule,
    Sequence,
    Trigger,
    TRIGGER_DECODER,
    Watch,
    WatchFilter,
)
from tools.assay.core.model import Claim


# --- [CONSTANTS] ------------------------------------------------------------------------
# ``Rail.params`` is raw wire data; recursive actions anchor on Rail/Program leaves.

_claim_st: st.SearchStrategy[Claim] = st.sampled_from(list(Claim))
_verb_st: st.SearchStrategy[str] = st.text(min_size=1, max_size=32)

_rail_st: st.SearchStrategy[Rail] = st.builds(Rail, claim=_claim_st, verb=_verb_st)
_program_st: st.SearchStrategy[Program] = resolve(Program)
_action_st: st.SearchStrategy[Action] = st.deferred(
    lambda: st.one_of(
        _rail_st,
        _program_st,
        st.builds(Debounce, action=_action_st, window_ms=st.integers(min_value=1, max_value=10_000)),
        st.builds(Sequence, actions=st.lists(_action_st, min_size=1, max_size=3).map(tuple)),
    )
)
_trigger_st: st.SearchStrategy[Trigger] = st.one_of(resolve(Watch), resolve(Schedule), resolve(Manual))

# --- [OPERATIONS] -----------------------------------------------------------------------


@example(action=Rail(claim=Claim.STATIC, verb="x"))
@example(action=Program(argv=("a",)))
@example(action=Sequence(actions=(Rail(claim=Claim.STATIC, verb="x"),)))
@example(action=Debounce(action=Rail(claim=Claim.STATIC, verb="x"), window_ms=1))
@given(_action_st)
def test_action_encode_decode_roundtrip(action: Action) -> None:
    """Action variants survive encoder/decoder round-trip."""
    roundtrip(action, encode, ACTION_DECODER.decode)


register_law(Action, "test_action_encode_decode_roundtrip")
register_law("ACTION_DECODER", "test_action_encode_decode_roundtrip")


@example(trigger=Manual())
@example(trigger=Watch(paths=("p",)))
@example(trigger=Schedule(cron="* * * * *"))
@given(_trigger_st)
def test_trigger_encode_decode_roundtrip(trigger: Trigger) -> None:
    """Trigger variants survive encoder/decoder round-trip."""
    roundtrip(trigger, encode, TRIGGER_DECODER.decode)


register_law(Trigger, "test_trigger_encode_decode_roundtrip")
register_law("TRIGGER_DECODER", "test_trigger_encode_decode_roundtrip")

# Per-variant registrations reuse the two parametric round-trip laws.
register_law(Rail, "test_action_encode_decode_roundtrip")
register_law(Program, "test_action_encode_decode_roundtrip")
register_law(Debounce, "test_action_encode_decode_roundtrip")
register_law(Sequence, "test_action_encode_decode_roundtrip")
register_law(Watch, "test_trigger_encode_decode_roundtrip")
register_law(Schedule, "test_trigger_encode_decode_roundtrip")
register_law(Manual, "test_trigger_encode_decode_roundtrip")

# --- [DESCRIBE_PREFIX]

_DESCRIBE_PREFIXES: tuple[tuple[type[Watch | Schedule | Manual | Rail | Program | Sequence | Debounce], str], ...] = (
    (Watch, "watch["),
    (Schedule, "schedule["),
    (Manual, "manual"),
    (Rail, "rail["),
    (Program, "program["),
    (Sequence, "seq["),
    (Debounce, "debounce["),
)


@pytest.mark.parametrize(
    "node,prefix",
    [
        (Watch(paths=("src/",)), "watch["),
        (Schedule(cron="*/5 * * * *"), "schedule["),
        (Manual(), "manual"),
        (Rail(claim=Claim.STATIC, verb="check"), "rail["),
        (Program(argv=("python",)), "program["),
        (Sequence(actions=(Rail(claim=Claim.STATIC, verb="v"),)), "seq["),
        (Debounce(action=Rail(claim=Claim.STATIC, verb="v")), "debounce["),
    ],
    ids=[cls for _, cls in _DESCRIBE_PREFIXES],
)
def test_describe_prefix_per_variant(node: Watch | Schedule | Manual | Rail | Program | Sequence | Debounce, prefix: str) -> None:
    """Every described variant exposes its discriminant prefix."""
    assert describe(node).startswith(prefix)


register_law(describe, "test_describe_prefix_per_variant")


@given(_action_st)
def test_describe_action_nonempty(action: Action) -> None:
    """Action descriptions never collapse to an empty label."""
    assert len(describe(action)) > 0


register_law(describe, "test_describe_action_nonempty")

# --- [SEQUENCE_DESCRIBE]


def test_describe_sequence_compositional() -> None:
    """Sequence descriptions compose child descriptions in order."""
    r1 = Rail(claim=Claim.STATIC, verb="a")
    r2 = Rail(claim=Claim.CODE, verb="b")
    seq = Sequence(actions=(r1, r2))
    expected = "seq[" + " > ".join(describe(a) for a in seq.actions) + "]"
    assert describe(seq) == expected


register_law(Sequence, "test_describe_sequence_compositional")

# --- [RAIL_DESCRIBE]


@pytest.mark.parametrize("claim", list(Claim))
def test_describe_rail_claim_verb_projection(claim: Claim) -> None:
    """Rail descriptions project claim value and verb without remapping."""
    rail = Rail(claim=claim, verb="run")
    assert describe(rail) == f"rail[{claim.value}:run]"


register_law(Rail, "test_describe_rail_claim_verb_projection")

# --- [SCHEDULE_TIMEZONE]


def test_describe_schedule_timezone_suffix() -> None:
    """Schedule descriptions include timezone only when the wire value is truthy."""
    assert "@ UTC" in describe(Schedule(cron="0 * * * *"))
    # msgspec is the only route to the empty-timezone branch; the constructor defaults to UTC.
    no_tz = msgspec.json.decode(b'{"kind":"schedule","cron":"* * * * *","timezone":""}', type=Schedule)
    out = describe(no_tz)
    assert "@" not in out
    assert out.startswith("schedule[")


register_law(Schedule, "test_describe_schedule_timezone_suffix")

# --- [WATCH_DESCRIBE]


@given(resolve(Watch))
def test_describe_watch_path_count(watch: Watch) -> None:
    """Watch descriptions expose exact path cardinality."""
    label = describe(watch)
    assert f"{len(watch.paths)} paths" in label


register_law(Watch, "test_describe_watch_path_count")

# --- [WATCHFILTER_SWEEP]


@pytest.mark.parametrize("flt", list(WatchFilter))
def test_watch_filter_roundtrip(flt: WatchFilter) -> None:
    """Every WatchFilter member survives Watch wire round-trip."""
    w = Watch(paths=("p",), filter=flt)
    decoded = decode(encode(w))
    assert isinstance(decoded, Watch)
    assert decoded.filter == flt


register_law(WatchFilter, "test_watch_filter_roundtrip")

# --- [CONSTRAINT_VIOLATION]


@pytest.mark.parametrize(
    "blob,match",
    [
        (b'{"kind":"schedule","cron":"* * * * *","cpu_threshold":1.5}', r"<= 1\.0"),
        (b'{"kind":"watch","paths":["src"],"debounce":0}', r">= 1"),
        (b'{"kind":"program","argv":[]}', r"length >= 1"),
        (b'{"kind":"manual","unknown_field":1}', r"unknown field"),
    ],
    ids=["cpu_threshold_gt1", "debounce_lt1", "program_empty_argv", "unknown_field"],
)
def test_decode_constraint_violations(blob: bytes, match: str) -> None:
    """Out-of-bound wire inputs surface msgspec validation diagnostics."""
    with pytest.raises(msgspec.ValidationError, match=match):
        decode(blob)


register_law(decode, "test_decode_constraint_violations")

# --- [ENCODE_PURITY]


@given(_action_st)
def test_encode_deterministic(action: Action) -> None:
    """Encoding is byte-stable across repeated calls."""
    assert encode(action) == encode(action)


register_law(encode, "test_encode_deterministic")

# --- [EDGE]


def test_debounce_edge_default_is_trailing() -> None:
    """Edge is the leading/trailing debounce vocabulary; Debounce coalesces to the trailing edge by default."""
    assert set(Edge) == {Edge.LEADING, Edge.TRAILING}
    assert Debounce(action=Rail(claim=Claim.STATIC, verb="x")).edge is Edge.TRAILING


register_law(Edge, "test_debounce_edge_default_is_trailing")
