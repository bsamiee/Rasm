"""Output lines of a child relayed as log records by `rasm_telemetry.relayed`, copied from the output cases of `ProcmonTests` in twisted `src/twisted/runner/test/test_procmon.py` (commit 72fdbe3)."""

from collections import deque

import pytest

from rasm_telemetry import relayed

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_output_received_complete_line(caplog: pytest.LogCaptureFixture) -> None:
    """Getting a complete output line generates a log message (`test_procmon.py:398-423`)."""
    events = caplog.handler.records
    deque(relayed(iter([b"hello world!\n"]), caplog.handler, "foo"), maxlen=0)
    assert len(events) == 1
    tag = events[0].name
    line = events[0].getMessage()
    assert tag == "foo"
    assert line == "hello world!"


def test_output_received_complete_line_invalid_utf8(caplog: pytest.LogCaptureFixture) -> None:
    """Getting invalid UTF-8 results in the repr of the raw message (`test_procmon.py:452-478`)."""
    events = caplog.handler.records
    deque(relayed(iter([b"\xffhello world!\n"]), caplog.handler, "foo"), maxlen=0)
    assert len(events) == 1
    message = events[0]
    tag = message.name
    output = message.getMessage()
    assert tag == "foo"
    assert output == "b'\\xffhello world!'"


def test_output_received_partial_line(caplog: pytest.LogCaptureFixture) -> None:
    """Getting a partial line results in one event once the output ends (`test_procmon.py:480-507`)."""
    events = caplog.handler.records
    deque(relayed(iter([b"hello world!"]), caplog.handler, "foo"), maxlen=0)
    assert len(events) == 1
    tag = events[0].name
    line = events[0].getMessage()
    assert tag == "foo"
    assert line == "hello world!"
