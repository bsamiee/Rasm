"""CLI dispatch, exit-code, tracing, bootstrap, and channel-separation laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

import msgspec
import pytest

from tests.python.tools.assay.kit import read_one_envelope_from_bytes
from tools.assay import __main__ as _main_mod, bootstrap_error, install_tracing
from tools.assay.core.model import Claim, RailStatus


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import VerbRunner


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (_main_mod.main, bootstrap_error, install_tracing)

_FAULTED_EXIT: int = RailStatus.FAULTED.exit_code

# --- [TABLES] ---------------------------------------------------------------------------


class _ParseFault(msgspec.Struct, frozen=True, gc=False):
    """Parse-fault law row with argv plus optional Envelope discriminants."""

    label: str
    argv: tuple[str, ...]
    claim: Claim | None = None
    message: str | None = None
    also_contains: str | None = None
    failing_step: str | None = None


_PARSE_FAULTS: tuple[_ParseFault, ...] = (
    _ParseFault("empty-argv", (), message="parse: no command", failing_step="parse"),
    _ParseFault("bare-bridge", ("bridge",), claim=Claim.BRIDGE, message="incomplete command"),
    _ParseFault("bare-test", ("test",), claim=Claim.TEST),
    _ParseFault("bare-code", ("code",), claim=Claim.CODE),
    _ParseFault("numeric-validator", ("code", "query", "(module) @m", "--max-results", "-1"), failing_step="parse"),
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MAIN_PARSE_FAULT_MATRIX]


@pytest.mark.parametrize("row", _PARSE_FAULTS, ids=[r.label for r in _PARSE_FAULTS])
def test_main_parse_fault_matrix(cli: VerbRunner, row: _ParseFault) -> None:
    """Pre-dispatch parse faults fold to one FAULTED stdout Envelope."""
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
    match row.also_contains:
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
    """Wire output stays on stdout; stderr carries no schema payload."""
    res = cli("static")
    assert len(res.stdout.splitlines()) == 1
    assert b'"schema_version"' not in res.stderr
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.schema_version == 1


# --- [MAIN_TRACING_LIFECYCLE]


def test_main_tracing_lifecycle_order(monkeypatch: pytest.MonkeyPatch) -> None:
    """Main drains the active trace provider after dispatch returns."""
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
    assert events == [("dispatch", ("self-test",)), ("flush", _main_mod.__dict__["DRAIN_MS"]), ("shutdown",)]


# --- [MAIN_SURROGATE_ARGV]


def test_main_surrogate_argv_does_not_crash(cli: VerbRunner) -> None:
    """Lone surrogate bytes in argv are replaced with U+FFFD before reaching the wire encoder."""
    # POSIX fsdecode can produce this lone surrogate; direct literal avoids platform branching.
    surrogate_token = "\udcff"  # ruff:ignore[hardcoded-password-string]  # not a credential; lone surrogate probes wire_safe sanitization path
    res = cli(surrogate_token)
    assert len(res.stdout.splitlines()) == 1
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.status is RailStatus.FAULTED


# --- [MAIN_SUBPROCESS_EXIT_CODE]


@pytest.mark.subprocess
def test_main_subprocess_exit_code(cli: VerbRunner) -> None:
    """Invalid argv in a real subprocess returns the FAULTED exit code."""
    res = cli(isolate=True)
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED


# --- [MAIN_CONFIG_ENV_FAULT]


def test_main_config_env_fault_surfaces_as_config_step(cli: VerbRunner) -> None:
    """Malformed ASSAY_* during settings construction emits a config-step fault."""
    res = cli("static", extra_env={"ASSAY_MAX_CHECKS": "999"})
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error_context is not None
    assert res.envelope.error_context.failing_step == "config"


# --- [MAIN_UNEXPECTED_DISPATCH]


def test_main_unexpected_dispatch_faults_to_dispatch_step(cli: VerbRunner, monkeypatch: pytest.MonkeyPatch) -> None:
    """Unexpected dispatch exceptions cross the final boundary as dispatch-step faults."""

    class _BoomApp:
        @staticmethod
        def parse_args(_tokens: tuple[str, ...], **_kwargs: object) -> tuple[object, None, None]:
            return object(), None, None  # non-None first item forces Cyclopts into dispatch, not help rendering

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
    """Providers without flush or shutdown lifecycle do not affect the dispatch code."""
    monkeypatch.setattr(_main_mod, "meta", lambda *_tokens: 5)
    monkeypatch.setattr(_main_mod, "get_tracer_provider", SimpleNamespace)
    assert _main_mod.main(["self-test"]) == 5


# --- [MAIN_DRAIN_SWALLOWS_LIFECYCLE]


def test_main_drain_swallows_provider_lifecycle_exceptions(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Provider lifecycle failures drain to stderr only and preserve clean stdout."""

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
    """Tracing-settings validation failures do not replace the dispatch result."""
    monkeypatch.setenv("ASSAY_MAX_CHECKS", "999")
    monkeypatch.setattr(_main_mod, "meta", lambda *_tokens: 0)
    monkeypatch.setattr(_main_mod, "get_tracer_provider", lambda: SimpleNamespace(force_flush=lambda *_a, **_k: True, shutdown=lambda: None))
    assert _main_mod.main([]) == 0


# --- [MAIN_SUBPROCESS_BOOTSTRAP_ERROR]


@pytest.mark.subprocess
def test_main_subprocess_bootstrap_error_config_fault(cli: VerbRunner) -> None:
    """Malformed ASSAY_* at interpreter startup folds to one config-fault Envelope."""
    res = cli("static", isolate=True, extra_env={"ASSAY_MAX_CHECKS": "999"})
    assert res.exit_code == _FAULTED_EXIT
    assert res.envelope.status is RailStatus.FAULTED
    assert res.envelope.error_context is not None
    assert res.envelope.error_context.failing_step == "config"


# --- [MAIN_SUBPROCESS_FAILING_RAIL_CHANNEL_SEPARATION]


@pytest.mark.subprocess
def test_main_subprocess_failing_rail_channel_separation(cli: VerbRunner) -> None:
    """A FAULTED subprocess rail keeps wire output on stdout and diagnostics on stderr."""
    res = cli("api", "resolve", "totally-bogus-key-xyz", "--strict", isolate=True)
    assert res.exit_code == _FAULTED_EXIT
    assert len(res.stdout.splitlines()) == 1
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.status is RailStatus.FAULTED
    assert b"schema_version" not in res.stderr
    assert b"rail.finish" in res.stderr


# --- [MAIN_SUBPROCESS_HUMAN_RENDERER]


@pytest.mark.subprocess
def test_main_subprocess_human_renderer_console_stderr(cli: VerbRunner) -> None:
    """Human log format in a subprocess keeps console diagnostics on stderr.

    The env override is required because piped subprocess stderr is not a tty; ANSI output proves the
    ConsoleRenderer arm, while stdout remains one Envelope line.
    """
    res = cli("api", "resolve", "totally-bogus-key-xyz", "--strict", isolate=True, extra_env={"ASSAY_LOG_FORMAT": "human"})
    assert res.exit_code == _FAULTED_EXIT
    assert len(res.stdout.splitlines()) == 1
    assert b"rail.finish" in res.stderr
    assert b"\x1b[" in res.stderr
    assert b'"event"' not in res.stderr


# --- [INSTALL_TRACING_EMPTY_ENDPOINT]


def test_install_tracing_empty_endpoint_is_noop() -> None:
    """An empty tracing endpoint leaves the current tracer provider untouched."""
    from opentelemetry.trace import get_tracer_provider as _gtp  # ruff:ignore[import-outside-top-level]  # defers OTel global-provider install

    before = _gtp()
    install_tracing("")
    after = _gtp()
    assert before is after


# --- [INSTALL_TRACING_NON_EMPTY_ENDPOINT]


def test_install_tracing_non_empty_endpoint_builds_real_provider(monkeypatch: pytest.MonkeyPatch) -> None:
    """A non-empty tracing endpoint builds and installs a fresh OTLP provider.

    Intercepting the setter observes construction without replacing the session provider, whose once-only
    install and shutdown semantics would contaminate later span-capture tests.
    """
    from opentelemetry.sdk.trace import TracerProvider  # ruff:ignore[import-outside-top-level]  # module-top installs OTel before monkeypatch
    import opentelemetry.trace as _ot  # ruff:ignore[import-outside-top-level]  # deferred: same session-provider contamination reason as TracerProvider

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
    """Valid environment bootstrap has no import-time config fault."""
    assert bootstrap_error() is None


def test_main_version_token_routes_to_verb_params(cli: VerbRunner) -> None:
    """A verb-local ``--version`` token binds params instead of printing app version text.

    Restoring Cyclopts version flags intercepts dispatch and replaces the Envelope with plain text.
    """
    res = cli("package", "plan", "--slug", "nonexistent-slug-xyz", "--version", "9.9.9")
    assert len(res.stdout.splitlines()) == 1
    decoded = read_one_envelope_from_bytes(res.stdout)
    assert decoded.claim is Claim.PACKAGE
    assert decoded.verb == "plan"


def test_main_enum_param_help_renders(monkeypatch: pytest.MonkeyPatch, capsysbinary: pytest.CaptureFixture[bytes]) -> None:
    """Help for an Enum-hinted ``None`` default param renders usage text and exits 0.

    ``show_default=False`` prevents Cyclopts from reading ``default_val.name`` before help rendering.
    The direct main call is required because help output is plain text, not an Envelope.
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
    """_returncode preserves bare ints and folds hook-less results to 0.

    These non-Envelope arms need direct coverage because the CLI laws exercise the hook-bearing Envelope path.
    """
    assert _main_mod._returncode(result) == expected


# --- [MAIN_EXEC_FLAG]


@pytest.mark.parametrize(
    "argv, stripped, target",
    [
        (("static", "--all"), ("static", "--all"), None),  # no flag: tokens unchanged, env untouched
        (("--exec", "ssh://h", "static"), ("static",), "ssh://h"),  # spaced form
        (("--exec=ssh://u@h:2222", "static"), ("static",), "ssh://u@h:2222"),  # inline form
        (("static", "--exec", "local", "--all"), ("static", "--all"), "local"),  # mid-stream spaced form survives
        (("--exec", "local"), (), "local"),  # local target, no subcommand
    ],
    ids=["absent", "spaced", "inline", "mid-stream", "local-only"],
)
def test_extract_exec_splits_global_flag(argv: tuple[str, ...], stripped: tuple[str, ...], target: str | None) -> None:
    """``_extract_exec`` lifts the agent-first ``--exec`` global flag out of the token stream in spaced and inline forms."""
    assert _main_mod._extract_exec(argv) == (stripped, target)


@pytest.mark.parametrize(
    "flag_value, expected_env",
    [("ssh://h:22", "ssh://h:22"), ("local", ""), ("", "")],
    ids=["ssh-target", "local-normalizes-empty", "empty-is-local"],
)
def test_admit_exec_routes_flag_through_validated_env_path(flag_value: str, expected_env: str, monkeypatch: pytest.MonkeyPatch) -> None:
    """``_admit_exec`` admits the flag value through ASSAY_EXEC_TARGET so settings validate it; ``local`` normalizes to empty."""
    # _admit_exec writes os.environ directly; anchor the var via setenv so teardown
    # reverts it when the variable was initially absent.
    monkeypatch.setenv("ASSAY_EXEC_TARGET", "")
    stripped = _main_mod._admit_exec(("--exec", flag_value, "static"))
    assert stripped == ("static",)
    assert _main_mod.os.environ["ASSAY_EXEC_TARGET"] == expected_env

    from tools.assay.composition.settings import (  # ruff:ignore[import-outside-top-level]  # admission round-trip through the validated env path
        AssaySettings,
        Local,
        Ssh,
    )

    target = AssaySettings(exec_known_hosts=None).exec_target
    assert isinstance(target, Ssh if expected_env else Local), f"flag {flag_value!r} did not admit the expected exec target: {target!r}"


def test_admit_exec_absent_flag_leaves_env_untouched(monkeypatch: pytest.MonkeyPatch) -> None:
    """``_admit_exec`` with no ``--exec`` flag preserves any pre-existing ASSAY_EXEC_TARGET env override (env is the fallback)."""
    monkeypatch.setenv("ASSAY_EXEC_TARGET", "ssh://envhost")
    assert _main_mod._admit_exec(("static", "--all")) == ("static", "--all")
    assert _main_mod.os.environ["ASSAY_EXEC_TARGET"] == "ssh://envhost", "absent flag must not clobber the env override"
