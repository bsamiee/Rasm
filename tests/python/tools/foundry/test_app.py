"""Help, refused declarations, and fault exits of the foundry app."""

from pathlib import Path

import pytest

from tools.foundry.app import app, exited
from typography.builds.pipeline import Action

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_help_lists_every_action(capsys: pytest.CaptureFixture[str]) -> None:
    """Root help names each action the one command takes."""
    app(["--help"], result_action="return_value", exit_on_error=False)
    out = capsys.readouterr().out
    assert all(action in out for action in Action)


@pytest.mark.parametrize("text, status", [('name = ""\ncorpus = "corpus.toml"\noutput = "build"\nfamilies = []\n', 65), ("", 66)])
def test_refused_declaration_exits_with_its_reason(text: str, status: int, tmp_path: Path, capsys: pytest.CaptureFixture[str]) -> None:
    """Invalid or missing declaration exits with its reason's status and one line naming it on stderr."""
    declaration = tmp_path / "set.toml"
    if text:
        declaration.write_text(text)
    code = exited(app(["build", str(declaration)], result_action="return_value", exit_on_error=False))
    err = capsys.readouterr().err
    assert (code, err.startswith(str(declaration)), err.count("\n")) == (status, True, 1)
