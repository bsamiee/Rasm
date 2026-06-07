"""Automation model codec and description laws."""

import msgspec
import pytest

from tools.assay.automation.model import Debounce, decode, describe, encode, Manual, Rail, Schedule, Sequence, Watch, WatchFilter
from tools.assay.core.model import Claim


def test_recursive_action_codec_round_trips() -> None:
    """Nested Sequence/Debounce actions preserve their tagged wire shape."""
    action = Sequence(actions=(Debounce(action=Rail(claim=Claim.STATIC, verb="report"), window_ms=250),))

    assert decode(encode(action), trigger=False) == action
    assert describe(action) == "seq[debounce[rail[static:report] @ 250ms]]"


@pytest.mark.parametrize(
    "node,trigger", [(Debounce(action=Rail(claim=Claim.STATIC, verb="report"), window_ms=250, collapse=False), False), (Manual(), True)]
)
def test_leaf_node_codec_round_trips(node: Debounce | Manual, *, trigger: bool) -> None:
    """The collapse=False debounce flag and the niladic Manual trigger survive the tagged-union codec."""
    assert decode(encode(node), trigger=trigger) == node


def test_watch_and_schedule_knobs_decode() -> None:
    """Watchfiles and cron knobs are first-class trigger fields."""
    watch = Watch(paths=("tools/assay",), filter=WatchFilter.PYTHON, step=25, force_polling=True, recursive=False, ignore_permission_denied=True)
    schedule = Schedule(cron="*/5 * * * *", timezone="UTC")

    assert decode(encode(watch), trigger=True) == watch
    assert decode(encode(schedule), trigger=True) == schedule
    assert "UTC" in describe(schedule)


def test_schedule_defaults_to_utc_and_threshold_is_normalized() -> None:
    """Schedules are explicit UTC by default and CPU thresholds are bounded fractions."""
    schedule = decode(b'{"kind":"schedule","cron":"*/5 * * * *"}', trigger=True)

    assert isinstance(schedule, Schedule)
    assert schedule == Schedule(cron="*/5 * * * *")
    assert schedule.timezone == "UTC"
    with pytest.raises(msgspec.ValidationError, match=r"<= 1\.0"):
        decode(b'{"kind":"schedule","cron":"*/5 * * * *","cpu_threshold":1.5}', trigger=True)


def test_program_decode_requires_argv() -> None:
    """Program actions must name an executable before reaching the engine."""
    with pytest.raises(msgspec.ValidationError, match="length >= 1"):
        decode(b'{"kind":"program","argv":[]}', trigger=False)
