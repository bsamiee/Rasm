"""Laws for ``tools.assay.automation.model``.

Covers action/trigger codec round-trips, describe projections, and wire validation through the
public encode/decode surfaces.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from hypothesis import example, given, strategies as st
import msgspec
import pytest

from tests.python._testkit.spec import roundtrip
from tests.python._testkit.strategies import resolve
from tools.assay.automation.model import (
    Action,  # noqa: TC001  # @given resolves parameter annotations at decoration time
    ACTION_DECODER,
    Debounce,
    decode,
    describe,
    encode,
    Manual,
    Program,
    Rail,
    Schedule,
    Sequence,
    Trigger,  # noqa: TC001  # @given resolves parameter annotations at decoration time
    TRIGGER_DECODER,
    Watch,
)
from tools.assay.core.model import Claim


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (decode, describe, encode)

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

# --- [WIRE_ROUNDTRIP]


@example(action=Rail(claim=Claim.STATIC, verb="x"))
@example(action=Program(argv=("a",)))
@example(action=Sequence(actions=(Rail(claim=Claim.STATIC, verb="x"),)))
@example(action=Debounce(action=Rail(claim=Claim.STATIC, verb="x"), window_ms=1))
@given(_action_st)
def test_action_encode_decode_roundtrip(action: Action) -> None:
    """Action variants survive encoder/decoder round-trip."""
    roundtrip(action, encode, ACTION_DECODER.decode)


@example(trigger=Manual())
@example(trigger=Watch(paths=("p",)))
@example(trigger=Schedule(cron="* * * * *"))
@given(_trigger_st)
def test_trigger_encode_decode_roundtrip(trigger: Trigger) -> None:
    """Trigger variants survive encoder/decoder round-trip."""
    roundtrip(trigger, encode, TRIGGER_DECODER.decode)


# --- [DESCRIBE_PROJECTION]


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
    ids=["watch", "schedule", "manual", "rail", "program", "sequence", "debounce"],
)
def test_describe_prefix_per_variant(node: Trigger | Action, prefix: str) -> None:
    """Every described variant exposes its discriminant prefix."""
    assert describe(node).startswith(prefix)


def test_describe_sequence_compositional() -> None:
    """Sequence descriptions compose child descriptions in order."""
    seq = Sequence(actions=(Rail(claim=Claim.STATIC, verb="a"), Rail(claim=Claim.CODE, verb="b")))
    assert describe(seq) == "seq[" + " > ".join(describe(a) for a in seq.actions) + "]"


@pytest.mark.parametrize("claim", list(Claim))
def test_describe_rail_claim_verb_projection(claim: Claim) -> None:
    """Rail descriptions project claim value and verb without remapping."""
    assert describe(Rail(claim=claim, verb="run")) == f"rail[{claim.value}:run]"


def test_describe_schedule_timezone_suffix() -> None:
    """Schedule descriptions include timezone only when the wire value is truthy."""
    assert "@ UTC" in describe(Schedule(cron="0 * * * *"))
    # msgspec is the only route to the empty-timezone branch; the constructor defaults to UTC.
    no_tz = msgspec.json.decode(b'{"kind":"schedule","cron":"* * * * *","timezone":""}', type=Schedule)
    out = describe(no_tz)
    assert "@" not in out
    assert out.startswith("schedule[")


@given(resolve(Watch))
def test_describe_watch_path_count(watch: Watch) -> None:
    """Watch descriptions expose exact path cardinality."""
    assert f"{len(watch.paths)} paths" in describe(watch)


# --- [WIRE_CONSTRAINTS]


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
