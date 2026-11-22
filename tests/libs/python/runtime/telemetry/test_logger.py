"""Bound loggers of `telemetry.logger`: context-local fields, the stdlib logger's level, independence from structlog's process-wide configuration, and output with no handler."""

import logging

import pytest
import structlog
from structlog.contextvars import bound_contextvars
from structlog.typing import EventDict, WrappedLogger

from rasm import telemetry

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_an_entry_merges_context_local_fields(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """Fields bound to the context reach every entry logged inside it, beside the entry's own."""
    log = telemetry.logger("suite.context")
    log.addHandler(handler)
    log.setLevel(logging.INFO)
    with bound_contextvars(job="Scene 1-24"):
        log.info("started", frames=48)
    log.info("idle")
    log.removeHandler(handler)
    attributes = [line["attributes"] for line in written]
    assert [{key: value for key, value in held.items() if not key.startswith("code.")} for held in attributes if isinstance(held, dict)] == [{"job": "Scene 1-24", "frames": 48}, {}]


def test_an_entry_below_the_stdlib_level_is_dropped(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """The wrapped stdlib logger's level decides which entries reach its handlers."""
    log = telemetry.logger("suite.levels")
    log.addHandler(handler)
    log.setLevel(telemetry.LogSettings(level="warning").severity)
    log.info("hidden")
    log.warning("shown")
    log.removeHandler(handler)
    assert [line["body"] for line in written] == ["shown"]


def test_the_logger_ignores_structlog_process_configuration(handler: logging.Handler, written: list[dict[str, object]]) -> None:
    """A process-wide structlog configuration dropping every event leaves the library's logger writing."""

    def dropped(_logger: WrappedLogger, _method: str, _event: EventDict, /) -> EventDict:
        raise structlog.DropEvent

    structlog.configure(processors=[dropped])
    log = telemetry.logger("suite.configured")
    log.addHandler(handler)
    log.setLevel(logging.INFO)
    log.info("kept")
    log.removeHandler(handler)
    assert [line["body"] for line in written] == ["kept"]


def test_a_warning_with_no_handler_prints_its_event(monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]) -> None:
    """With no handler on any logger, a warning reaches standard error as its event text alone, an info entry nothing."""
    monkeypatch.setattr(logging.getLogger(), "handlers", [])
    log = telemetry.logger("suite.unhandled")
    log.info("quiet")
    log.warning("Final chance")
    assert capsys.readouterr().err == "Final chance\n"
