"""Output lines of another program relayed as records through a handler by `telemetry.relayed`."""

import pytest

from rasm.telemetry import relayed

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_each_nonblank_line_is_one_info_record_under_the_scope(caplog: pytest.LogCaptureFixture) -> None:
    """Lines lose trailing space and keep leading space, blank lines are left out, a line that is no UTF-8 is its bytes' `repr`, and each yielded line is the message of one `INFO` record under the scope."""
    output = [b"  indented frame 1 \r\n", b"\n", b" \t\n", b"\xffraw\n", b"last"]
    yielded = list(relayed(iter(output), caplog.handler, "blender.child"))
    assert yielded == ["  indented frame 1", "b'\\xffraw'", "last"]
    assert [(record.name, record.levelname, record.getMessage()) for record in caplog.records] == [("blender.child", "INFO", text) for text in yielded]


def test_records_are_written_as_the_lines_are_consumed(caplog: pytest.LogCaptureFixture) -> None:
    """Each record is handled when its line is consumed, before the program's next line exists."""
    lines = relayed(iter([b"first\n", b"second\n"]), caplog.handler, "blender.child")
    next(lines)
    assert [record.getMessage() for record in caplog.records] == ["first"]
