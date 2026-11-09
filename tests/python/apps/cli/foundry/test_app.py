"""CLI binding, help, independent input faults, and JSON line output."""

from datetime import timedelta
from functools import partial
from pathlib import Path

from cyclopts.exceptions import CycloptsError
from expression import Result
import msgspec
import pytest
import sysexits

from rasm_foundry import Action, app, exited, Report
from rasm_records import Error, Fault, Ok, Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_help_lists_actions_and_exits_successfully(capsys: pytest.CaptureFixture[str]) -> None:
    """Native help exposes every action without writing error output."""
    assert exited(app(["--help"], result_action="return_value", exit_on_error=False)) == 0
    captured = capsys.readouterr()
    assert all(action in captured.out for action in Action)
    assert not captured.err


def test_optional_positionals_and_options_bind_the_same_typed_call(tmp_path: Path) -> None:
    """Publication supports both CLI forms without a forwarding command layer, the output folder an option alone."""
    declaration, destination, output = tmp_path / "set.toml", tmp_path / "published", tmp_path / "build"
    command, positional, _ = app.parse_args(["publish", str(declaration), "Test", str(destination), "--output", str(output)])
    _, options, _ = app.parse_args(["publish", str(declaration), "--name", "Test", "--destination", str(destination), "--output", str(output)])
    assert isinstance(command, partial)
    assert command.args == (Action.PUBLISH,)
    assert positional.args == options.args == (declaration, "Test", destination)
    assert positional.kwargs == options.kwargs == {"output": output}
    with pytest.raises(CycloptsError):
        app(["build", str(declaration)], result_action="return_value", exit_on_error=False, print_error=False)
    with pytest.raises(CycloptsError):
        app(["unknown-action", str(declaration), "--output", str(output)], result_action="return_value", exit_on_error=False, print_error=False)


@pytest.mark.parametrize("text,status", [('name=""\ncorpus="x"\nfamilies=[]\n', sysexits.EX_DATAERR), (None, sysexits.EX_NOINPUT)])
def test_declared_input_failures_keep_their_cli_status(text: str | None, status: int, tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    """Malformed declarations and missing files produce one path-specific error with the required exit code."""
    declaration = tmp_path / "set.toml"
    if text is not None:
        declaration.write_text(text, encoding="utf-8")
    assert exited(app(["build", str(declaration), "--output", str(tmp_path / "build")], result_action="return_value", exit_on_error=False)) == status
    captured = capsys.readouterr()
    assert not captured.out
    assert captured.err.startswith(str(declaration))
    assert captured.err.count("\n") == 1


def test_independent_corpus_and_selection_failures_accumulate(tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    """A missing corpus does not hide an unknown family selection."""
    declaration = tmp_path / "set.toml"
    declaration.write_text(
        'name="Set"\ncorpus="missing.toml"\n[[families]]\nname="Test"\noriginal="original"\ngenre={kind="kufi"}\n[[families.faces]]\nstyle="Regular"\noriginal="Test.ttf"\n', encoding="utf-8"
    )
    outcome = app(["build", str(declaration), "Missing", "--output", str(tmp_path / "build")], result_action="return_value", exit_on_error=False)
    match outcome:
        case Result(error=faults):
            assert tuple(fault.reason for fault in faults) == (Reason.UNREADABLE, Reason.ABSENT)
        case other:
            pytest.fail(str(other))
    assert exited(outcome) == sysexits.EX_NOINPUT
    assert capsys.readouterr().err.count("\n") == 2


def test_result_action_writes_one_json_line_per_face(capsys: pytest.CaptureFixture[str]) -> None:
    """Report batching retains one independently decodable record per face."""
    reports = (Report(face="First", duration=timedelta(microseconds=1)), Report(face="Second", duration=timedelta(milliseconds=2)))
    assert exited(Ok(reports)) == 0
    captured = capsys.readouterr()
    assert not captured.err
    assert msgspec.json.Decoder(Report).decode_lines(captured.out) == list(reports)


def test_result_action_deduplicates_faults_and_keeps_first_reason_status(capsys: pytest.CaptureFixture[str]) -> None:
    """Repeated faults print once while the first reason determines the process exit."""
    first = Fault(reason=Reason.UNREADABLE, subject="font", cause="missing")
    second = Fault(reason=Reason.FAILED, subject="compile", cause="rejected")
    assert exited(Error((first, second, first))) == sysexits.EX_NOINPUT
    captured = capsys.readouterr()
    assert not captured.out
    assert captured.err.splitlines() == [first.message, second.message]
