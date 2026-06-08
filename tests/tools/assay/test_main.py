"""CLI dispatch / exit-code / channel-separation laws for tools.assay.__main__ [main].

Covers tools.assay [install_tracing, bootstrap_error] with smoke laws.
Every law must be falsifiable by a real defect. Anti-theater: no tautological asserts,
no re-stating construction. register_law keeps law-coverage total for the three SUT symbols.
"""

# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

import pytest

from tests._aspect import register_law  # noqa: PLC2701  # sibling test-internal module; _-named by S1 design
from tests._spec import assert_error_status  # noqa: F401, PLC2701  # imported for symmetry; _-named by S1 design
from tests.tools.assay.conftest import read_one_envelope_from_bytes
from tools.assay import __main__ as _main_mod, bootstrap_error, install_tracing
from tools.assay.core.model import Claim
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tests.tools.assay.conftest import VerbRunner


# --- [CONSTANTS] -------------------------------------------------------------------------

_FAULTED_EXIT: int = RailStatus.FAULTED.exit_code


# --- [OPERATIONS] -----------------------------------------------------------------------
# Laws are organized by SUT symbol so register_law calls are co-located.


# -- main: empty-argv → parse fault -------------------------------------------------------


def test_main_empty_argv_emits_parse_fault(cli: VerbRunner) -> None:
    """Empty argv emits exactly one FAULTED Envelope on stdout; exit code matches FAULTED."""
    res = cli()
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error is not None
    assert res.envelope.error.message.startswith("parse: no command")


register_law(_main_mod.main, "test_main_empty_argv_emits_parse_fault")


# -- main: bare claim (incomplete command) ------------------------------------------------


def test_main_bare_claim_emits_parse_fault(cli: VerbRunner) -> None:
    """A bare claim token is an incomplete command; parse fault Envelope carries the claim."""
    res = cli("static")
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.claim is Claim.STATIC
    assert res.envelope.error is not None
    assert "incomplete command" in res.envelope.error.message


register_law(_main_mod.main, "test_main_bare_claim_emits_parse_fault")


# -- main: exit-code matrix (parse faults carry _DRAIN_MS-independent exit codes) --------


@pytest.mark.parametrize(
    "argv, expected_claim", [(("static",), Claim.STATIC), (("test",), Claim.TEST), (("code",), Claim.CODE)], ids=["static", "test", "code"]
)
def test_main_bare_claim_exit_code_matrix(cli: VerbRunner, argv: tuple[str, ...], expected_claim: Claim) -> None:
    """Every bare-claim parse fault carries exit code 7 (FAULTED) regardless of claim type."""
    res = cli(*argv)
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.claim is expected_claim


register_law(_main_mod.main, "test_main_bare_claim_exit_code_matrix")


# -- main: numeric-validator fault surfaces before dispatch -------------------------------


def test_main_numeric_validator_faults_before_dispatch(cli: VerbRunner) -> None:
    """Cyclopts numeric constraint violations reach stdout as one parse-fault Envelope."""
    res = cli("code", "query", "(module) @m", "--max-results", "-1")
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error is not None
    assert res.envelope.error_context is not None
    assert res.envelope.error_context.failing_step == "parse"


register_law(_main_mod.main, "test_main_numeric_validator_faults_before_dispatch")


# -- main: error_context.failing_step is "parse" on no-command fault ---------------------


def test_main_no_command_fault_context(cli: VerbRunner) -> None:
    """No-command fault Envelope carries error_context with failing_step='parse'."""
    res = cli()
    assert res.envelope.error_context is not None
    assert res.envelope.error_context.failing_step == "parse"


register_law(_main_mod.main, "test_main_no_command_fault_context")


# -- main: channel separation — one Envelope line on stdout, structlog on stderr ----------


def test_main_channel_separation(cli: VerbRunner) -> None:
    """The Envelope wire line is confined to stdout; schema_version never leaks to stderr."""
    res = cli("static", "plan")
    assert len(res.stdout.splitlines()) == 1
    assert b'"schema_version"' not in res.stderr
    # Decoding the single stdout line must succeed (proves it is a valid Envelope).
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.schema_version == 1


register_law(_main_mod.main, "test_main_channel_separation")


# -- main: tracing lifecycle — flush then shutdown after dispatch -------------------------


def test_main_tracing_lifecycle_order(monkeypatch: pytest.MonkeyPatch) -> None:
    """Main flushes then shuts down the active trace provider AFTER dispatch returns."""
    events: list[object] = []

    def _flush(timeout_millis: int) -> bool:
        events.append(("flush", timeout_millis))
        return True

    def _shutdown() -> None:
        events.append(("shutdown",))

    def _fake_meta(*tokens: str) -> int:
        events.append(("dispatch", tokens))
        return 3

    monkeypatch.setattr(_main_mod, "meta", _fake_meta)
    monkeypatch.setattr(_main_mod, "get_tracer_provider", lambda: SimpleNamespace(force_flush=_flush, shutdown=_shutdown))

    code = _main_mod.main(["self-test"])
    assert code == 3
    assert events == [("dispatch", ("self-test",)), ("flush", _main_mod.__dict__["_DRAIN_MS"]), ("shutdown",)]


register_law(_main_mod.main, "test_main_tracing_lifecycle_order")


# -- main: surrogates scrubbed before wire encode ----------------------------------------


def test_main_surrogate_argv_does_not_crash(cli: VerbRunner) -> None:
    """Lone surrogate bytes in argv are replaced with U+FFFD before reaching the wire encoder."""
    # os.fsdecode(b'\xff') → surrogated string on POSIX; mimic with the lone-surrogate char directly.
    surrogate_token = "\udcff"  # noqa: S105  # lone surrogate, not a password; tests wire_safe sanitisation
    # Running with a bad token in the verb position: should produce a fault, never a UnicodeEncodeError.
    res = cli(surrogate_token)
    # Must produce exactly one decodable Envelope on stdout.
    assert len(res.stdout.splitlines()) == 1
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.status is RailStatus.FAULTED


register_law(_main_mod.main, "test_main_surrogate_argv_does_not_crash")


# -- main: isolate=True subprocess exit code for invalid input ---------------------------


def test_main_subprocess_exit_code(cli: VerbRunner) -> None:
    """Subprocess invocation with invalid argv returns FAULTED exit code (isolation law)."""
    res = cli(isolate=True)
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED


register_law(_main_mod.main, "test_main_subprocess_exit_code")


# -- install_tracing: empty endpoint is a no-op ------------------------------------------


def test_install_tracing_empty_endpoint_is_noop() -> None:
    """install_tracing('') must not install a tracer provider (no exception, no side effect)."""
    from opentelemetry.trace import get_tracer_provider as _gtp  # noqa: PLC0415

    before = _gtp()
    install_tracing("")
    after = _gtp()
    assert before is after


register_law(install_tracing, "test_install_tracing_empty_endpoint_is_noop")


# -- install_tracing: non-empty endpoint installs a real provider -------------------------


def test_install_tracing_non_empty_endpoint_builds_real_provider(monkeypatch: pytest.MonkeyPatch) -> None:
    """install_tracing(endpoint) builds a real OTLP ``TracerProvider`` and installs it via ``set_tracer_provider``.

    The set is intercepted so the build is OBSERVED without replacing the session provider: OTel's once-only
    install would otherwise mask install_tracing's provider, and shutting the global provider down breaks span
    capture for every later test (the latent order-dependent bug this replaces).

    Falsified by: install_tracing skipping the provider build, attaching no span processor, or reusing the
    existing global provider instead of constructing a new one.
    """
    from opentelemetry.sdk.trace import TracerProvider  # noqa: PLC0415
    import opentelemetry.trace as _ot  # noqa: PLC0415

    captured: list[object] = []
    monkeypatch.setattr(_ot, "set_tracer_provider", captured.append)
    install_tracing("http://127.0.0.1:4318/v1/traces")
    assert len(captured) == 1, "install_tracing must install exactly one provider"
    provider = captured[0]
    assert isinstance(provider, TracerProvider)
    assert provider is not _ot.get_tracer_provider(), "must build a NEW provider, not reuse the global"
    assert provider._active_span_processor is not None  # OTLP BatchSpanProcessor was attached


register_law(install_tracing, "test_install_tracing_non_empty_endpoint_builds_real_provider")


# -- bootstrap_error: nominal path returns None ------------------------------------------


def test_bootstrap_error_nominal_returns_none() -> None:
    """Under a valid environment bootstrap_error() returns None (no import-time config fault)."""
    result = bootstrap_error()
    # The import-time settings load succeeded in the current environment; None is the healthy signal.
    assert result is None


register_law(bootstrap_error, "test_bootstrap_error_nominal_returns_none")


# -- bootstrap_error: smoke law — result is ValidationError or None ----------------------


def test_bootstrap_error_type_contract() -> None:
    """bootstrap_error() always returns ValidationError | None, never an unexpected type."""
    from pydantic import ValidationError  # noqa: PLC0415

    result = bootstrap_error()
    assert result is None or isinstance(result, ValidationError)


register_law(bootstrap_error, "test_bootstrap_error_type_contract")
