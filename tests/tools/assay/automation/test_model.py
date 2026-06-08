"""Laws for tools.assay.automation.model.

Scope: ACTION_DECODER, Action, Debounce, Manual, Program, Rail, Schedule, Sequence,
TRIGGER_DECODER, Trigger, Watch, WatchFilter, decode, describe, encode.

Law-coverage strategy:
- decode∘encode roundtrip (inverse) drives Action and Trigger via the custom strategies below.
  This single parametric law covers Rail/Program/Sequence/Debounce (Action) and Watch/Schedule/
  Manual (Trigger). ACTION_DECODER/TRIGGER_DECODER are registered at module level against the same
  roundtrip law (decode delegates to them); no separate law function is needed.
- describe projection laws assert prefix/structural invariants for each variant.
- Constraint violation laws assert msgspec.ValidationError on out-of-bounds wire inputs.
- WatchFilter StrEnum sweep asserts every member round-trips through Watch.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from hypothesis import given, strategies as st
import msgspec
import pytest

from tests._aspect import register_law  # noqa: PLC2701  # sibling test-internal module; `_`-named by S1 design
from tests._spec import roundtrip  # noqa: PLC2701  # sibling test-internal module; `_`-named by S1 design
from tests._strategies import resolve  # noqa: PLC2701  # sibling test-internal module; `_`-named by S1 design
from tools.assay.automation.model import (
    Action,
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
    Trigger,
    TRIGGER_DECODER,
    Watch,
    WatchFilter,
)
from tools.assay.core.model import Claim


# --- [STRATEGIES] -----------------------------------------------------------------------
# Rail.params is msgspec.Raw (RawType): unhandled by _node → register a bounded custom strategy
# that omits params (uses the default Raw()). Debounce/Sequence are mutually recursive with Action
# so they need an explicit deferred st.one_of tree that anchors on Rail/Program leaves.

_claim_st: st.SearchStrategy[Claim] = st.sampled_from(list(Claim))
_verb_st: st.SearchStrategy[str] = st.text(min_size=1, max_size=32)
_argv_st: st.SearchStrategy[tuple[str, ...]] = (
    st.lists(st.text(min_size=1, max_size=16), min_size=1, max_size=4).map(tuple)
)

_rail_st: st.SearchStrategy[Rail] = st.builds(Rail, claim=_claim_st, verb=_verb_st)
_program_st: st.SearchStrategy[Program] = st.builds(Program, argv=_argv_st)
_action_st: st.SearchStrategy[Action] = st.deferred(
    lambda: st.one_of(
        _rail_st,
        _program_st,
        st.builds(Debounce, action=_action_st, window_ms=st.integers(min_value=1, max_value=10_000)),
        st.builds(Sequence, actions=st.lists(_action_st, min_size=1, max_size=3).map(tuple)),
    )
)
_trigger_st: st.SearchStrategy[Trigger] = st.one_of(resolve(Watch), resolve(Schedule), resolve(Manual))

# Typed decode surfaces: ACTION_DECODER.decode / TRIGGER_DECODER.decode return correctly-typed unions so
# roundtrip[T] resolves without cast or helper. The `decode` public function delegates to these singletons
# (encode∘decode roundtrip therefore exercises ACTION_DECODER/TRIGGER_DECODER transitively).


# --- [LAWS] -----------------------------------------------------------------------------


@given(_action_st)
def test_action_encode_decode_roundtrip(action: Action) -> None:
    """decode(encode(x), trigger=False) == x for all Action variants (Rail/Program/Sequence/Debounce)."""
    roundtrip(action, encode, ACTION_DECODER.decode)


register_law(Action, "test_action_encode_decode_roundtrip")
register_law("ACTION_DECODER", "test_action_encode_decode_roundtrip")


@given(_trigger_st)
def test_trigger_encode_decode_roundtrip(trigger: Trigger) -> None:
    """decode(encode(x), trigger=True) == x for all Trigger variants (Watch/Schedule/Manual)."""
    roundtrip(trigger, encode, TRIGGER_DECODER.decode)


register_law(Trigger, "test_trigger_encode_decode_roundtrip")
register_law("TRIGGER_DECODER", "test_trigger_encode_decode_roundtrip")

# Per-variant coverage: the two roundtrip laws above exercise all 7 concrete types. register_law records
# coverage for each so assert_law_coverage sees them; no duplicate test function is needed.
register_law(Rail, "test_action_encode_decode_roundtrip")
register_law(Program, "test_action_encode_decode_roundtrip")
register_law(Debounce, "test_action_encode_decode_roundtrip")
register_law(Sequence, "test_action_encode_decode_roundtrip")
register_law(Watch, "test_trigger_encode_decode_roundtrip")
register_law(Schedule, "test_trigger_encode_decode_roundtrip")
register_law(Manual, "test_trigger_encode_decode_roundtrip")


# --- describe prefix laws ---


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
def test_describe_prefix_per_variant(
    node: Watch | Schedule | Manual | Rail | Program | Sequence | Debounce,
    prefix: str,
) -> None:
    """describe(node) starts with the tag prefix for each union variant."""
    assert describe(node).startswith(prefix)


register_law(describe, "test_describe_prefix_per_variant")


@given(_action_st)
def test_describe_action_nonempty(action: Action) -> None:
    """describe(action) is always a non-empty string for any Action."""
    assert len(describe(action)) > 0


register_law(describe, "test_describe_action_nonempty")


# --- Sequence describe compositional law ---


def test_describe_sequence_compositional() -> None:
    """describe(Sequence(actions)) == 'seq[' + ' > '.join(describe(a) for a in actions) + ']'."""
    r1 = Rail(claim=Claim.STATIC, verb="a")
    r2 = Rail(claim=Claim.CODE, verb="b")
    seq = Sequence(actions=(r1, r2))
    expected = "seq[" + " > ".join(describe(a) for a in seq.actions) + "]"
    assert describe(seq) == expected


register_law(Sequence, "test_describe_sequence_compositional")


# --- Rail describe projection ---


@pytest.mark.parametrize("claim", list(Claim))
def test_describe_rail_claim_verb_projection(claim: Claim) -> None:
    """describe(Rail(claim=c, verb=v)) == 'rail[{c.value}:{v}]' for all Claim members."""
    rail = Rail(claim=claim, verb="run")
    assert describe(rail) == f"rail[{claim.value}:run]"


register_law(Rail, "test_describe_rail_claim_verb_projection")


# --- Schedule timezone describe ---


def test_describe_schedule_timezone_suffix() -> None:
    """describe(Schedule) appends ' @ {tz}' when timezone is truthy; omits it for empty string."""
    assert "@ UTC" in describe(Schedule(cron="0 * * * *"))
    # Fabricate a wire instance with empty timezone via msgspec to probe the omit branch.
    no_tz = msgspec.json.decode(
        b'{"kind":"schedule","cron":"* * * * *","timezone":""}', type=Schedule
    )
    out = describe(no_tz)
    assert "@" not in out
    assert out.startswith("schedule[")


register_law(Schedule, "test_describe_schedule_timezone_suffix")


# --- Watch describe path-count and debounce projection ---


@given(resolve(Watch))
def test_describe_watch_path_count(watch: Watch) -> None:
    """describe(Watch) embeds the exact path count."""
    label = describe(watch)
    assert f"{len(watch.paths)} paths" in label


register_law(Watch, "test_describe_watch_path_count")


# --- WatchFilter sweep ---


@pytest.mark.parametrize("flt", list(WatchFilter))
def test_watch_filter_roundtrip(flt: WatchFilter) -> None:
    """Every WatchFilter member survives encode→decode round-trip inside a Watch."""
    w = Watch(paths=("p",), filter=flt)
    decoded = decode(encode(w), trigger=True)
    assert isinstance(decoded, Watch)
    assert decoded.filter == flt


register_law(WatchFilter, "test_watch_filter_roundtrip")


# --- Constraint violation laws ---


@pytest.mark.parametrize(
    "blob,trigger,match",
    [
        (b'{"kind":"schedule","cron":"* * * * *","cpu_threshold":1.5}', True, r"<= 1\.0"),
        (b'{"kind":"watch","paths":["src"],"debounce":0}', True, r">= 1"),
        (b'{"kind":"program","argv":[]}', False, r"length >= 1"),
        (b'{"kind":"manual","unknown_field":1}', True, r"unknown field"),
    ],
    ids=["cpu_threshold_gt1", "debounce_lt1", "program_empty_argv", "unknown_field"],
)
def test_decode_constraint_violations(blob: bytes, trigger: bool, match: str) -> None:  # noqa: FBT001  # parametrized bool flag in test params is idiomatic pytest; not a production API
    """Out-of-bound wire inputs raise msgspec.ValidationError with the expected diagnostic."""
    with pytest.raises(msgspec.ValidationError, match=match):
        decode(blob, trigger=trigger)


register_law(decode, "test_decode_constraint_violations")


# --- encode purity ---


@given(_action_st)
def test_encode_deterministic(action: Action) -> None:
    """encode(x) twice yields byte-identical output (deterministic encoder, no dict-ordering drift)."""
    assert encode(action) == encode(action)


register_law(encode, "test_encode_deterministic")
