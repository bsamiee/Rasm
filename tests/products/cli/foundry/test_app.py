"""Faults of a declared font set and its corpus, each with its exit status."""

from functools import partial
from pathlib import Path

import anyio
from expression import Error
import pytest
import sysexits

from rasm.cli import exited
from rasm.foundry import app
from rasm.records import Reason

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("name,status", [pytest.param("set.toml", sysexits.EX_DATAERR, id="empty-set-name"), pytest.param("absent.toml", sysexits.EX_NOINPUT, id="missing-file")])
def test_declared_input_failures_keep_their_cli_status(name: str, status: int, tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    """A declaration with an empty name exits `EX_DATAERR` and a missing one `EX_NOINPUT`, each with one error line naming its path."""
    (tmp_path / "set.toml").write_text('name=""\ncorpus="x"\nfamilies=[]\n', encoding="utf-8")
    declaration = tmp_path / name
    command, bound, _ = app.parse_args(["build", str(declaration), "--output", str(tmp_path / "build")], exit_on_error=False)
    assert exited(anyio.run(partial(command, *bound.args, **bound.kwargs, cache=tmp_path / "cache"))) == status
    captured = capsys.readouterr()
    assert not captured.out
    assert captured.err.startswith(str(declaration))
    assert captured.err.count("\n") == 1


def test_independent_corpus_and_selection_failures_accumulate(tmp_path: Path) -> None:
    """A missing corpus does not hide an unknown family selection, each fault naming its own subject: the corpus path beside the declaration, and the set."""
    declaration = tmp_path / "set.toml"
    declaration.write_text(
        'name="Set"\ncorpus="missing.toml"\n[[families]]\nname="Test"\noriginal="original"\ngenre={kind="kufi"}\n[[families.faces]]\nstyle="Regular"\noriginal="Test.ttf"\n', encoding="utf-8"
    )
    command, bound, _ = app.parse_args(["build", str(declaration), "Missing", "--output", str(tmp_path / "build")], exit_on_error=False)
    outcome = anyio.run(partial(command, *bound.args, **bound.kwargs, cache=tmp_path / "cache"))
    named = outcome.map_error(lambda faults: tuple((fault.reason, fault.subject) for fault in faults))
    assert named == Error(((Reason.UNREADABLE, str(tmp_path / "missing.toml")), (Reason.ABSENT, "Set")))
