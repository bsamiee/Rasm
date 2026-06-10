"""Multi-source logging laws for tools.assay [configure_logging].

Proves the four caller-visible contracts of the process-global logging owner: the configured
``logger_factory`` resolves ``sys.stderr`` per write (a stream closed since configure time cannot
strand a later log on a dead fd); stdlib ``logging`` records bridge structured to stderr through the
same processor chain; values msgspec cannot natively encode degrade to ``str`` instead of raising;
``ASSAY_LOG_LEVEL`` gates both rails; and reconfiguring never stacks bridge handlers on the root
logger. Drives the installed logger directly rather than through ``capture_logs`` (which bypasses
the factory).
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import io
import logging
import threading
from typing import TYPE_CHECKING

import msgspec
import pytest
import structlog

from tests.python._testkit._aspect import register_laws  # underscore-prefixed internal test helper
from tools.assay import configure_logging
from tools.assay._logging import _StderrBridgeHandler, _StderrLogger  # private factory products are the test surface
from tools.assay.composition.settings import LogFormat


if TYPE_CHECKING:
    from collections.abc import Iterator


# --- [SERVICES] -------------------------------------------------------------------------


@pytest.fixture
def _error_level_env() -> Iterator[None]:
    """Set ``ASSAY_LOG_LEVEL=error`` for one law, then reinstall the default-level config."""
    with pytest.MonkeyPatch.context() as mp:
        mp.setenv("ASSAY_LOG_LEVEL", "error")
        yield
    configure_logging(LogFormat.CI)


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_configure_logging_resolves_stderr_per_write(monkeypatch: pytest.MonkeyPatch) -> None:
    configure_logging(LogFormat.CI)
    logger = structlog.get_config()["logger_factory"]()
    assert isinstance(logger, _StderrLogger)

    first = io.StringIO()
    monkeypatch.setattr("sys.stderr", first)
    logger.info("before-close")
    assert "before-close" in first.getvalue()

    first.close()
    second = io.StringIO()
    monkeypatch.setattr("sys.stderr", second)
    logger.info("after-reassign")

    assert "after-reassign" in second.getvalue()


def test_stdlib_bridge_routes_records_structured_to_stderr(monkeypatch: pytest.MonkeyPatch) -> None:
    configure_logging(LogFormat.CI)
    sink = io.StringIO()
    monkeypatch.setattr("sys.stderr", sink)

    logging.getLogger("asyncssh.bridge.law").warning("stdlib-bridge-event")  # noqa: TID251  # the stdlib rail is the law's subject

    lines = sink.getvalue().splitlines()
    assert len(lines) == 1
    payload = msgspec.json.decode(lines[0])
    assert payload["event"] == "stdlib-bridge-event"
    assert payload["level"] == "warning"
    assert "timestamp" in payload


def test_unencodable_value_degrades_to_str_never_raises(monkeypatch: pytest.MonkeyPatch) -> None:
    configure_logging(LogFormat.CI)
    sink = io.StringIO()
    monkeypatch.setattr("sys.stderr", sink)

    structlog.get_logger("assay.bridge.law").warning("unencodable-event", lock=threading.Lock(), fn=lambda: None)

    payload = msgspec.json.decode(sink.getvalue())
    assert payload["event"] == "unencodable-event"
    assert isinstance(payload["lock"], str)
    assert isinstance(payload["fn"], str)


@pytest.mark.usefixtures("_error_level_env")
def test_env_log_level_error_suppresses_info(monkeypatch: pytest.MonkeyPatch) -> None:
    configure_logging(LogFormat.CI)
    sink = io.StringIO()
    monkeypatch.setattr("sys.stderr", sink)

    structlog.get_logger("assay.bridge.law").info("suppressed-event")
    logging.getLogger("assay.bridge.law").info("suppressed-stdlib-event")  # noqa: TID251  # the stdlib rail is the law's subject
    assert not sink.getvalue()

    structlog.get_logger("assay.bridge.law").error("emitted-event")
    assert "emitted-event" in sink.getvalue()


def test_reconfigure_does_not_stack_bridge_handlers() -> None:
    configure_logging(LogFormat.CI)
    configure_logging(LogFormat.CI)

    root = logging.getLogger()  # noqa: TID251  # the root-handler set is the law's subject
    bridges = [h for h in root.handlers if isinstance(h, _StderrBridgeHandler)]
    assert len(bridges) == 1
    assert root.handlers == bridges


# --- [COMPOSITION] ----------------------------------------------------------------------

register_laws((
    configure_logging,
    (
        "test_configure_logging_resolves_stderr_per_write",
        "test_stdlib_bridge_routes_records_structured_to_stderr",
        "test_unencodable_value_degrades_to_str_never_raises",
        "test_env_log_level_error_suppresses_info",
        "test_reconfigure_does_not_stack_bridge_handlers",
    ),
))
