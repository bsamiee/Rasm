"""Main entrypoint laws."""

from typing import TYPE_CHECKING

from tools.assay.core.model import Claim
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
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
