"""Main entrypoint laws."""

from types import SimpleNamespace
from typing import TYPE_CHECKING

from tools.assay import __main__ as main_mod
from tools.assay.core.model import Claim
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    import pytest

    from tests.tools.assay.conftest import VerbRunner


def test_no_command_emits_parse_fault(cli: VerbRunner) -> None:
    """Empty argv emits one FAULTED envelope instead of stdout help."""
    env, code = cli()

    assert code == RailStatus.FAULTED.exit_code
    assert env.status is RailStatus.FAULTED
    assert env.error is not None
    assert env.error.message.startswith("parse: no command")
    assert env.error_context is not None
    assert env.error_context.failing_step == "parse"


def test_incomplete_claim_emits_parse_fault(cli: VerbRunner) -> None:
    """A bare claim is incomplete command input, not a help page."""
    env, code = cli("static")

    assert code == RailStatus.FAULTED.exit_code
    assert env.claim is Claim.STATIC
    assert not env.verb
    assert env.error is not None
    assert "incomplete command" in env.error.message


def test_numeric_validator_faults_before_dispatch(cli: VerbRunner) -> None:
    """Cyclopts numeric validators surface as one parse envelope."""
    env, code = cli("code", "query", "(module) @m", "--max-results", "-1")

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
