"""CLI dispatch / exit-code / channel-separation laws for tools.assay.__main__ [main].

Covers tools.assay [install_tracing, bootstrap_error] with smoke laws.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

import msgspec
import pytest

from tests.python._testkit.laws import register_laws
from tests.python.tools.assay.kit import read_one_envelope_from_bytes
from tools.assay import __main__ as _main_mod, bootstrap_error, install_tracing
from tools.assay.core.model import Claim
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import VerbRunner


# --- [CONSTANTS] ------------------------------------------------------------------------

_FAULTED_EXIT: int = RailStatus.FAULTED.exit_code

# --- [TABLES]


class _ParseFault(msgspec.Struct, frozen=True, gc=False):
    """One parse-fault example row: the argv that never reaches dispatch plus the distinguishing claims.

    Every row pins the FAULTED exit/status floor; ``claim``/``message``/``failing_step`` are the per-row
    discriminants (``None`` = unchecked). ``claim`` is the Cyclopts-recovered claim of the bare token,
    ``message`` an ``in``-substring of ``error.message``, ``failing_step`` the ``error_context`` step slug.
    """

    label: str
    argv: tuple[str, ...]
    claim: Claim | None = None
    message: str | None = None
    failing_step: str | None = None


_PARSE_FAULTS: tuple[_ParseFault, ...] = (
    _ParseFault("empty-argv", (), message="parse: no command", failing_step="parse"),
    _ParseFault("bare-static", ("static",), claim=Claim.STATIC, message="incomplete command"),
    _ParseFault("bare-test", ("test",), claim=Claim.TEST),
    _ParseFault("bare-code", ("code",), claim=Claim.CODE),
    _ParseFault("numeric-validator", ("code", "query", "(module) @m", "--max-results", "-1"), failing_step="parse"),
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MAIN_PARSE_FAULT_MATRIX]


@pytest.mark.parametrize("row", _PARSE_FAULTS, ids=[r.label for r in _PARSE_FAULTS])
def test_main_parse_fault_matrix(cli: VerbRunner, row: _ParseFault) -> None:
    """A pre-dispatch parse fault — empty argv, a bare incomplete claim, or a Cyclopts numeric violation — folds to one FAULTED Envelope on stdout.

    The FAULTED exit/status floor holds for every row; each row then pins its discriminant: the Cyclopts-recovered
    ``claim`` of a bare token, an ``error.message`` substring (``parse: no command`` / ``incomplete command``),
    and/or the ``error_context.failing_step`` slug (``parse`` reaches stdout before any verb dispatch).
    """
    res = cli(*row.argv)
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert row.claim is None or res.envelope.claim is row.claim
    match row.message:
        case None:
            pass
        case substring:
            assert res.envelope.error is not None
            assert substring in res.envelope.error.message
    match row.failing_step:
        case None:
            pass
        case step:
            assert res.envelope.error_context is not None
            assert res.envelope.error_context.failing_step == step


# --- [MAIN_CHANNEL_SEPARATION]


def test_main_channel_separation(cli: VerbRunner) -> None:
    """The Envelope wire line is confined to stdout; schema_version never leaks to stderr."""
    res = cli("static", "plan")
    assert len(res.stdout.splitlines()) == 1
    assert b'"schema_version"' not in res.stderr
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.schema_version == 1


# --- [MAIN_TRACING_LIFECYCLE]


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


# --- [MAIN_SURROGATE_ARGV]


def test_main_surrogate_argv_does_not_crash(cli: VerbRunner) -> None:
    """Lone surrogate bytes in argv are replaced with U+FFFD before reaching the wire encoder."""
    # os.fsdecode(b'\xff') yields a surrogated string on POSIX; '\udcff' is the direct equivalent.
    surrogate_token = "\udcff"  # noqa: S105  # not a credential; lone surrogate probes wire_safe sanitization path
    res = cli(surrogate_token)
    assert len(res.stdout.splitlines()) == 1
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.status is RailStatus.FAULTED


# --- [MAIN_SUBPROCESS_EXIT_CODE]


def test_main_subprocess_exit_code(cli: VerbRunner) -> None:
    """Subprocess invocation with invalid argv returns FAULTED exit code (isolation law)."""
    res = cli(isolate=True)
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED


# --- [MAIN_CONFIG_ENV_FAULT]


def test_main_config_env_fault_surfaces_as_config_step(cli: VerbRunner) -> None:
    """A verb that builds AssaySettings under a malformed ASSAY_* env emits one FAULTED Envelope, failing_step='config'."""
    res = cli("static", "plan", extra_env={"ASSAY_MAX_CHECKS": "999"})
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error_context is not None
    assert res.envelope.error_context.failing_step == "config"


# --- [MAIN_UNEXPECTED_DISPATCH]


def test_main_unexpected_dispatch_faults_to_dispatch_step(cli: VerbRunner, monkeypatch: pytest.MonkeyPatch) -> None:
    """A non-Cyclopts dispatch exception reaches stdout as one FAULTED Envelope via the final boundary (failing_step='dispatch')."""

    class _BoomApp:
        @staticmethod
        def parse_args(_tokens: tuple[str, ...], **_kwargs: object) -> tuple[object, None, None]:
            return object(), None, None  # non-None first element signals real dispatch to cyclopts, not help

        def __call__(self, *_args: object, **_kwargs: object) -> object:
            raise RuntimeError("boom")

    monkeypatch.setattr(_main_mod, "app", _BoomApp())
    res = cli("static")
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error_context is not None
    assert res.envelope.error_context.failing_step == "dispatch"
    assert res.envelope.error is not None
    assert "RuntimeError" in res.envelope.error.message


# --- [MAIN_DRAIN_NOOP]


def test_main_drain_noop_on_lifecycleless_provider(monkeypatch: pytest.MonkeyPatch) -> None:
    """Drain over a provider exposing neither force_flush nor shutdown is a no-op; main returns the dispatch code."""
    monkeypatch.setattr(_main_mod, "meta", lambda *_tokens: 5)
    monkeypatch.setattr(_main_mod, "get_tracer_provider", SimpleNamespace)
    assert _main_mod.main(["self-test"]) == 5


# --- [MAIN_DRAIN_SWALLOWS_LIFECYCLE]


def test_main_drain_swallows_provider_lifecycle_exceptions(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """A provider whose force_flush and shutdown raise is drained to stderr only; main returns the dispatch code, stdout stays clean."""

    def _boom_flush(_timeout_millis: int) -> bool:
        raise RuntimeError("flush boom")

    def _boom_shutdown() -> None:
        raise RuntimeError("shutdown boom")

    monkeypatch.setattr(_main_mod, "meta", lambda *_tokens: 7)
    monkeypatch.setattr(_main_mod, "get_tracer_provider", lambda: SimpleNamespace(force_flush=_boom_flush, shutdown=_boom_shutdown))
    code = _main_mod.main(["self-test"])
    cap = capsysbinary.readouterr()
    assert code == 7
    assert cap.out == b""
    assert b"force_flush failed" in cap.err
    assert b"shutdown failed" in cap.err


# --- [MAIN_INSTALL_SWALLOWS_SETTINGS]


def test_main_install_tracing_swallows_settings_validation(monkeypatch: pytest.MonkeyPatch) -> None:
    """A malformed ASSAY_* env raised while building tracing settings is swallowed; main still returns the dispatch code."""
    monkeypatch.setenv("ASSAY_MAX_CHECKS", "999")
    monkeypatch.setattr(_main_mod, "meta", lambda *_tokens: 0)
    monkeypatch.setattr(_main_mod, "get_tracer_provider", lambda: SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None))
    assert _main_mod.main([]) == 0


# --- [MAIN_SUBPROCESS_BOOTSTRAP_ERROR]


def test_main_subprocess_bootstrap_error_config_fault(cli: VerbRunner) -> None:
    """A malformed ASSAY_* env survives real interpreter startup (import-time bootstrap fallback) and folds to one config-fault Envelope."""
    res = cli("static", "plan", isolate=True, extra_env={"ASSAY_MAX_CHECKS": "999"})
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error_context is not None
    assert res.envelope.error_context.failing_step == "config"


# --- [MAIN_SUBPROCESS_FAILING_RAIL_CHANNEL_SEPARATION]


def test_main_subprocess_failing_rail_channel_separation(cli: VerbRunner) -> None:
    """A FAULTED rail in a real subprocess holds the fd-level wire contract: one Envelope line on stdout, rail.finish + diagnostics on stderr."""
    res = cli("api", "resolve", "totally-bogus-key-xyz", "--strict", isolate=True)
    assert res.exit_code == _FAULTED_EXIT
    assert len(res.stdout.splitlines()) == 1
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.status is RailStatus.FAULTED
    assert b"schema_version" not in res.stderr
    assert b"rail.finish" in res.stderr


# --- [MAIN_SUBPROCESS_HUMAN_RENDERER]


def test_main_subprocess_human_renderer_console_stderr(cli: VerbRunner) -> None:
    """ASSAY_LOG_FORMAT=human in a real subprocess routes console-rendered diagnostics to stderr; the wire stays one stdout Envelope line.

    A piped subprocess defaults to CI (stderr is no tty), so the env override is the only way this arm engages;
    ConsoleRenderer(colors=True) emits ANSI escapes regardless of tty, and the JSON event-key shape must be absent.
    """
    res = cli("api", "resolve", "totally-bogus-key-xyz", "--strict", isolate=True, extra_env={"ASSAY_LOG_FORMAT": "human"})
    assert res.exit_code == _FAULTED_EXIT
    assert len(res.stdout.splitlines()) == 1
    assert b"rail.finish" in res.stderr
    assert b"\x1b[" in res.stderr
    assert b'"event"' not in res.stderr


# --- [INSTALL_TRACING_EMPTY_ENDPOINT]


def test_install_tracing_empty_endpoint_is_noop() -> None:
    """install_tracing('') must not install a tracer provider (no exception, no side effect)."""
    from opentelemetry.trace import get_tracer_provider as _gtp  # noqa: PLC0415  # deferred: avoids OTel global-provider side-effect at session scope

    before = _gtp()
    install_tracing("")
    after = _gtp()
    assert before is after


# --- [INSTALL_TRACING_NON_EMPTY_ENDPOINT]


def test_install_tracing_non_empty_endpoint_builds_real_provider(monkeypatch: pytest.MonkeyPatch) -> None:
    """install_tracing(endpoint) builds a real OTLP ``TracerProvider`` and installs it via ``set_tracer_provider``.

    The set is intercepted so the build is OBSERVED without replacing the session provider: OTel's once-only
    install would otherwise mask install_tracing's provider, and shutting the global provider down breaks span
    capture for every later test (the latent order-dependent bug this replaces).

    Falsified by: install_tracing skipping the provider build, attaching no span processor, or reusing the
    existing global provider instead of constructing a new one.
    """
    from opentelemetry.sdk.trace import TracerProvider  # noqa: PLC0415  # deferred: module-level import installs OTel before monkeypatch
    import opentelemetry.trace as _ot  # noqa: PLC0415  # deferred: same session-provider contamination reason as TracerProvider

    captured: list[object] = []
    monkeypatch.setattr(_ot, "set_tracer_provider", captured.append)
    install_tracing("http://127.0.0.1:4318/v1/traces")
    assert len(captured) == 1, "install_tracing must install exactly one provider"
    provider = captured[0]
    assert isinstance(provider, TracerProvider)
    assert provider is not _ot.get_tracer_provider(), "must build a NEW provider, not reuse the global"
    assert provider._active_span_processor is not None


# --- [BOOTSTRAP_ERROR_NOMINAL]


def test_bootstrap_error_nominal_returns_none() -> None:
    """Under a valid environment bootstrap_error() returns None (no import-time config fault)."""
    result = bootstrap_error()
    assert result is None


# --- [BOOTSTRAP_ERROR_TYPE_CONTRACT]


def test_bootstrap_error_type_contract() -> None:
    """bootstrap_error() always returns ValidationError | None, never an unexpected type."""
    from pydantic import ValidationError  # noqa: PLC0415  # deferred: avoids mandatory pydantic import at collection time; type-contract check only

    result = bootstrap_error()
    assert result is None or isinstance(result, ValidationError)


def test_main_version_token_routes_to_verb_params(cli: VerbRunner) -> None:
    """--version after a verb binds the verb's params field; it never triggers a global version print.

    The root App and every claim sub-App carry version_flags=() — with cyclopts' default flag active,
    'package plan --version X' is intercepted before dispatch and bare app-version text replaces the
    Envelope on stdout (a total wire-contract violation). Falsified by restoring any version flag.
    """
    res = cli("package", "plan", "--slug", "nonexistent-slug-xyz", "--version", "9.9.9")
    assert len(res.stdout.splitlines()) == 1
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.claim is Claim.PACKAGE
    assert decoded.verb == "plan"


def test_main_enum_param_help_renders(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """--help on a verb with an Enum-hinted None-default param renders usage text and exits 0.

    cyclopts 4.16.1 renders Enum defaults via default_val.name before consulting show_default
    callables, so language's Parameter must keep show_default=False; falsified by any show_default
    form that re-enters the Enum branch (AttributeError -> FAULTED exit 2 instead of usage).
    Help output is plain text, not an Envelope, so this drives main directly rather than the cli runner.
    """
    neutralized = SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None)
    monkeypatch.setattr(_main_mod, "get_tracer_provider", lambda: neutralized)
    code = _main_mod.main(["code", "search", "--help"])
    cap = capsysbinary.readouterr()
    assert code == 0
    assert b"Usage:" in cap.out
    assert b'"status":"faulted"' not in cap.out


@pytest.mark.parametrize("result, expected", [(3, 3), (object(), 0)], ids=["bare-int", "hookless-object"])
def test_main_returncode_non_envelope_arms(result: object, expected: int) -> None:
    """_returncode passes a bare int through verbatim and folds hook-less results to 0.

    The Envelope arm (__cyclopts_returncode__ hook) rides every CLI law; these two arms only fire on
    non-Envelope dispatch results, so they need a direct probe. Falsified by a _returncode that coerces
    every result through the hook lookup (bare int would raise) or returns a non-zero default.
    """
    assert _main_mod._returncode(result) == expected


# --- [COMPOSITION] ----------------------------------------------------------------------
# Registrations run at import time so MANIFEST is fully populated before test collection begins.

register_laws(
    (
        _main_mod.main,
        (
            "test_main_parse_fault_matrix",
            "test_main_version_token_routes_to_verb_params",
            "test_main_enum_param_help_renders",
            "test_main_returncode_non_envelope_arms",
            "test_main_channel_separation",
            "test_main_tracing_lifecycle_order",
            "test_main_surrogate_argv_does_not_crash",
            "test_main_subprocess_exit_code",
            "test_main_config_env_fault_surfaces_as_config_step",
            "test_main_unexpected_dispatch_faults_to_dispatch_step",
            "test_main_drain_noop_on_lifecycleless_provider",
            "test_main_drain_swallows_provider_lifecycle_exceptions",
            "test_main_install_tracing_swallows_settings_validation",
            "test_main_subprocess_bootstrap_error_config_fault",
            "test_main_subprocess_failing_rail_channel_separation",
            "test_main_subprocess_human_renderer_console_stderr",
        ),
    ),
    (install_tracing, ("test_install_tracing_empty_endpoint_is_noop", "test_install_tracing_non_empty_endpoint_builds_real_provider")),
    (bootstrap_error, ("test_bootstrap_error_nominal_returns_none", "test_bootstrap_error_type_contract")),
)
