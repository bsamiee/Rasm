"""Main entrypoint laws."""

import functools
from pathlib import Path
import shutil
import tomllib
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
import pytest

from tests.tools.assay.conftest import read_one_envelope_from_bytes
from tools.assay import __main__ as main_mod
from tools.assay.core.model import Claim
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tests.tools.assay.conftest import VerbRunner


_REPO_ROOT = Path(__file__).resolve().parents[3]


def test_no_command_emits_parse_fault(cli: VerbRunner) -> None:
    """Empty argv emits one FAULTED envelope instead of stdout help."""
    res = cli()
    env, code = res.envelope, res.exit_code

    assert code == RailStatus.FAULTED.exit_code
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert env.error.message.startswith("parse: no command")
    assert env.error_context is not None
    assert env.error_context.failing_step == "parse"


def test_incomplete_claim_emits_parse_fault(cli: VerbRunner) -> None:
    """A bare claim is incomplete command input, not a help page."""
    res = cli("static")
    env, code = res.envelope, res.exit_code

    assert code == RailStatus.FAULTED.exit_code
    assert env.claim is Claim.STATIC
    assert not env.verb
    assert env.error is not None
    assert "incomplete command" in env.error.message


def test_numeric_validator_faults_before_dispatch(cli: VerbRunner) -> None:
    """Cyclopts numeric validators surface as one parse envelope."""
    res = cli("code", "query", "(module) @m", "--max-results", "-1")
    env, code = res.envelope, res.exit_code

    assert code == RailStatus.FAULTED.exit_code
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert env.error_context is not None
    assert env.error_context.failing_step == "parse"


def test_main_drains_tracing_provider_after_dispatch(monkeypatch: pytest.MonkeyPatch) -> None:
    """CLI lifecycle dispatches first, then force-flushes and shuts down the active trace provider."""
    events: list[object] = []

    def force_flush(timeout_millis: int) -> bool:
        events.append(("flush", timeout_millis))
        return True

    def shutdown() -> None:
        events.append(("shutdown",))

    def fake_meta(*tokens: str) -> int:
        events.append(("dispatch", tokens))
        return 7

    monkeypatch.setattr(main_mod, "meta", fake_meta)
    monkeypatch.setattr(main_mod, "get_tracer_provider", lambda: SimpleNamespace(force_flush=force_flush, shutdown=shutdown))

    assert main_mod.main(["self-test"]) == 7
    assert events == [("dispatch", ("self-test",)), ("flush", main_mod.__dict__["_DRAIN_MS"]), ("shutdown",)]


def test_envelope_on_stdout_diagnostics_on_stderr(cli: VerbRunner) -> None:
    """The Envelope wire line is confined to stdout; it never leaks onto the stderr diagnostics channel."""
    res = cli("static", "plan")
    assert len(res.stdout.splitlines()) == 1      # exactly one Envelope line on stdout
    assert b'"schema_version"' not in res.stderr  # the Envelope (carrying schema_version) never leaks to stderr
    _ = read_one_envelope_from_bytes(res.stdout)  # stdout decodes as exactly one Envelope


def test_version_matches_pyproject() -> None:
    """``--version`` emits Cyclopts meta-text outside the Envelope contract — assert it carries the project version."""
    if shutil.which("uv") is None:
        pytest.skip("uv not on PATH")
    version = tomllib.loads((_REPO_ROOT / "pyproject.toml").read_text(encoding="utf-8"))["project"]["version"]
    spawn = functools.partial(anyio.run_process, cwd=str(_REPO_ROOT), check=False)
    res = anyio.run(spawn, ["uv", "run", "python", "-m", "tools.assay", "--version"])
    assert res.returncode == 0
    assert version in res.stdout.decode()
