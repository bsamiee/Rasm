"""Logging laws for stderr routing, stdlib bridging, fallback encoding, and reconfigure idempotence."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # runtime: SinkLaw.drive/check are msgspec.Struct field annotations resolved at definition time
import io
from itertools import starmap
import logging
import threading

import msgspec
import pytest
import structlog

from tests.python._testkit.laws import register_laws
from tools.assay import configure_logging
from tools.assay._logging import _StderrBridgeHandler, _StderrLogger  # private factory products are the test surface
from tools.assay.composition.settings import LogFormat


# --- [TABLES]


class _SinkLaw(msgspec.Struct, frozen=True, gc=False):
    """One stderr-sink law row with pre-config env, driver, and payload check."""

    label: str
    drive: Callable[[_StderrLogger, pytest.MonkeyPatch, io.StringIO], str]
    check: Callable[[str], None]
    env: tuple[tuple[str, str], ...] = ()


def _emit_then_value(emit: Callable[[], None]) -> Callable[[_StderrLogger, pytest.MonkeyPatch, io.StringIO], str]:
    return lambda _logger, _mp, sink: (emit(), sink.getvalue())[1]


def _stdlib_warn(event: str) -> Callable[[], None]:
    return lambda: logging.getLogger("asyncssh.bridge.law").warning(event)  # noqa: TID251  # the stdlib rail is the law's subject


def _structured_payload(event: str) -> Callable[[str], None]:
    """Build a check for the structured stdlib-bridge payload."""

    def check(text: str) -> None:
        lines = text.splitlines()
        assert len(lines) == 1
        payload = msgspec.json.decode(lines[0])
        assert payload["event"] == event
        assert payload["level"] == "warning"
        assert "timestamp" in payload

    return check


def _module_callsite(text: str) -> None:
    """Foreign LogRecords expose ``module`` only; dropping MODULE removes the field entirely."""
    payload = msgspec.json.decode(text)
    assert payload["module"] == "test_logging"
    assert "qual_module" not in payload


def _unencodable_degrades(text: str) -> None:
    """Unencodable payload values degrade to strings instead of raising."""
    payload = msgspec.json.decode(text)
    assert payload["event"] == "unencodable-event"
    assert isinstance(payload["lock"], str)
    assert isinstance(payload["fn"], str)


def _resolve_per_write(logger: _StderrLogger, monkeypatch: pytest.MonkeyPatch, first: io.StringIO) -> str:
    """Prove per-write stderr resolution after the configured sink is closed."""
    logger.info("before-close")
    assert "before-close" in first.getvalue()
    first.close()
    second = io.StringIO()
    monkeypatch.setattr("sys.stderr", second)
    logger.info("after-reassign")
    return second.getvalue()


def _error_suppresses_info(_logger: _StderrLogger, _monkeypatch: pytest.MonkeyPatch, sink: io.StringIO) -> str:
    """Under ``ASSAY_LOG_LEVEL=error``, both rails drop info and keep error."""
    structlog.get_logger("assay.bridge.law").info("suppressed-event")
    logging.getLogger("assay.bridge.law").info("suppressed-stdlib-event")  # noqa: TID251  # the stdlib rail is the law's subject
    assert not sink.getvalue()
    structlog.get_logger("assay.bridge.law").error("emitted-event")
    return sink.getvalue()


def _contains(substring: str) -> Callable[[str], None]:
    return lambda text: _assert_in(substring, text)


def _assert_in(substring: str, text: str) -> None:
    assert substring in text, f"{substring!r} absent from sink text {text!r}"


_SINK_LAWS: tuple[_SinkLaw, ...] = (
    _SinkLaw("resolves-stderr-per-write", _resolve_per_write, _contains("after-reassign")),
    _SinkLaw("stdlib-bridge-structured", _emit_then_value(_stdlib_warn("stdlib-bridge-event")), _structured_payload("stdlib-bridge-event")),
    _SinkLaw("stdlib-bridge-module-callsite", _emit_then_value(_stdlib_warn("module-callsite-event")), _module_callsite),
    _SinkLaw(
        "unencodable-degrades-to-str",
        _emit_then_value(lambda: structlog.get_logger("assay.bridge.law").warning("unencodable-event", lock=threading.Lock(), fn=lambda: None)),
        _unencodable_degrades,
    ),
    _SinkLaw("env-error-suppresses-info", _error_suppresses_info, _contains("emitted-event"), env=(("ASSAY_LOG_LEVEL", "error"),)),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("row", _SINK_LAWS, ids=[r.label for r in _SINK_LAWS])
def test_configure_logging_stderr_sink_matrix(row: _SinkLaw, monkeypatch: pytest.MonkeyPatch) -> None:
    """Matrix stderr sink laws through the installed logger factory."""
    list(starmap(monkeypatch.setenv, row.env))
    configure_logging(LogFormat.CI)
    logger = structlog.get_config()["logger_factory"]()
    assert isinstance(logger, _StderrLogger)
    sink = io.StringIO()
    monkeypatch.setattr("sys.stderr", sink)
    row.check(row.drive(logger, monkeypatch, sink))
    monkeypatch.undo()
    configure_logging(LogFormat.CI)


def test_reconfigure_does_not_stack_bridge_handlers() -> None:
    configure_logging(LogFormat.CI)
    configure_logging(LogFormat.CI)

    root = logging.getLogger()  # noqa: TID251  # the root-handler set is the law's subject
    bridges = [h for h in root.handlers if isinstance(h, _StderrBridgeHandler)]
    assert len(bridges) == 1
    assert root.handlers == bridges


# --- [COMPOSITION] ----------------------------------------------------------------------

register_laws((configure_logging, ("test_configure_logging_stderr_sink_matrix", "test_reconfigure_does_not_stack_bridge_handlers")))
