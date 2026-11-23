"""Results a command returns, written by `rasm.cli.exited` as JSON lines or fault messages with an exit status."""

from datetime import timedelta

from expression import Error, Ok
import msgspec
import pytest
import sysexits

from rasm.cli import exited
from rasm.records import Fault, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class _Report(msgspec.Struct, frozen=True, kw_only=True):
    face: str
    duration: timedelta


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_a_tuple_result_writes_one_json_line_per_item(capsys: pytest.CaptureFixture[str]) -> None:
    """Each item of a tuple result is one independently decodable line on stdout, and the status is zero."""
    reports = (_Report(face="First", duration=timedelta(microseconds=1)), _Report(face="Second", duration=timedelta(milliseconds=2)))
    assert exited(Ok(reports)) == 0
    captured = capsys.readouterr()
    assert not captured.err
    assert msgspec.json.Decoder(_Report).decode_lines(captured.out) == list(reports)


def test_repeated_faults_print_once_and_the_first_reason_decides_the_status(capsys: pytest.CaptureFixture[str]) -> None:
    """A fault repeated later prints once, in first-seen order, and the first fault's reason maps to its `sysexits` status."""
    first = Fault(reason=Reason.UNREADABLE, subject="font", cause="missing")
    second = Fault(reason=Reason.FAILED, subject="compile", cause="rejected")
    assert exited(Error((first, second, first))) == sysexits.EX_NOINPUT
    captured = capsys.readouterr()
    assert not captured.out
    assert captured.err.splitlines() == [first.message, second.message]
