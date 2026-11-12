"""Exceptions reaching a process hook while `rasm_telemetry.unhandled` is entered, copied from opentelemetry-instrumentation-exceptions `tests/test_exceptions.py` (0.66b2) with entering the scope where the exemplar instruments."""

import asyncio
from collections.abc import Iterator
import io
import sys
import threading
from types import TracebackType

import msgspec
from opentelemetry._logs import SeverityNumber
from opentelemetry.semconv.attributes import exception_attributes
import pytest

from rasm_telemetry import logger, LogSettings, unhandled

# --- [CONSTANTS] ------------------------------------------------------------------------

PACKAGE = __name__.partition(".")[0]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _raised_value_error() -> ValueError:
    """Returns a caught `ValueError("boom")`, as the exemplar's helper of this name (`test_exceptions.py:57-61`)."""
    error = ValueError("boom")
    try:
        raise error
    except ValueError as exc:
        return exc


def _finished_log(log_exporter: io.StringIO) -> dict[str, object]:
    """Returns the one record written, as the exemplar's helper of this name returns the one exported (`test_exceptions.py:64-67`)."""
    logs = log_exporter.getvalue().splitlines()
    assert len(logs) == 1
    return msgspec.json.decode(logs[0], type=dict[str, object])


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def log_exporter() -> Iterator[io.StringIO]:
    """Yields the stream receiving every record of the package logger, where the exemplar's `log_exporter` yields an in-memory exporter (`test_exceptions.py:33-45`)."""
    stream = io.StringIO()
    handler = LogSettings().handler(stream, service="rasm-telemetry-tests", version="0")
    log = logger(PACKAGE)
    log.addHandler(handler)
    try:
        yield stream
    finally:
        log.removeHandler(handler)


def test_sys_excepthook_emits_log(log_exporter: io.StringIO, monkeypatch: pytest.MonkeyPatch) -> None:
    """`sys.excepthook` writes a fatal record of the exception and calls the hook set before it (`test_exceptions.py:70-99`)."""
    called = {"value": False}

    def stub_excepthook(_exc_type: type[BaseException], _exc: BaseException, _tb: TracebackType | None) -> None:
        called["value"] = True

    monkeypatch.setattr(sys, "excepthook", stub_excepthook)
    with unhandled(logger(PACKAGE), PACKAGE):
        exc = _raised_value_error()
        sys.excepthook(type(exc), exc, exc.__traceback__)
    log_record = _finished_log(log_exporter)
    assert log_record["severity_text"] == "FATAL"
    assert log_record["severity_number"] == SeverityNumber.FATAL.value
    assert log_record.get("event_name") == "ValueError"
    assert log_record["body"] == "boom"
    assert called == {"value": True}
    attributes = log_record["attributes"]
    assert isinstance(attributes, dict)
    assert attributes[exception_attributes.EXCEPTION_TYPE] == "ValueError"
    assert isinstance(attributes[exception_attributes.EXCEPTION_TYPE], str)
    assert attributes[exception_attributes.EXCEPTION_MESSAGE] == "boom"
    assert isinstance(attributes[exception_attributes.EXCEPTION_MESSAGE], str)
    assert "ValueError: boom" in attributes[exception_attributes.EXCEPTION_STACKTRACE]
    assert isinstance(attributes[exception_attributes.EXCEPTION_STACKTRACE], str)


def test_threading_excepthook_emits_log(log_exporter: io.StringIO, monkeypatch: pytest.MonkeyPatch) -> None:
    """`threading.excepthook` writes an error record of the exception and calls the hook set before it (`test_exceptions.py:102-129`)."""
    called = {"value": False}

    def stub_threading_excepthook(_args: threading.ExceptHookArgs) -> None:
        called["value"] = True

    monkeypatch.setattr(threading, "excepthook", stub_threading_excepthook)
    with unhandled(logger(PACKAGE), PACKAGE):
        exc = _raised_value_error()
        args = threading.ExceptHookArgs((type(exc), exc, exc.__traceback__, threading.current_thread()))
        threading.excepthook(args)
    log_record = _finished_log(log_exporter)
    assert log_record["severity_text"] == "ERROR"
    assert log_record["severity_number"] == SeverityNumber.ERROR.value
    assert log_record.get("event_name") == "ValueError"
    assert log_record["body"] == "boom"
    assert called == {"value": True}
    attributes = log_record["attributes"]
    assert isinstance(attributes, dict)
    assert attributes[exception_attributes.EXCEPTION_TYPE] == "ValueError"
    assert attributes[exception_attributes.EXCEPTION_MESSAGE] == "boom"
    assert "ValueError: boom" in attributes[exception_attributes.EXCEPTION_STACKTRACE]


def test_base_exceptions_are_not_emitted(log_exporter: io.StringIO, monkeypatch: pytest.MonkeyPatch) -> None:
    """A `KeyboardInterrupt` reaching `sys.excepthook` writes no record and reaches the hook set before the scope (`test_exceptions.py:276-293`)."""
    called = {"value": False}

    def stub_excepthook(_exc_type: type[BaseException], _exc: BaseException, _tb: TracebackType | None) -> None:
        called["value"] = True

    monkeypatch.setattr(sys, "excepthook", stub_excepthook)
    with unhandled(logger(PACKAGE), PACKAGE):
        exc = KeyboardInterrupt()
        sys.excepthook(type(exc), exc, exc.__traceback__)
    assert not log_exporter.getvalue().splitlines()
    assert called == {"value": True}


def test_double_instrument_does_not_emit_duplicate_logs(log_exporter: io.StringIO, monkeypatch: pytest.MonkeyPatch) -> None:
    """Entering the scope twice writes one record per exception (`test_exceptions.py:296-309`)."""
    monkeypatch.setattr(sys, "excepthook", lambda _exc_type, _exc, _tb: None)
    with unhandled(logger(PACKAGE), PACKAGE), unhandled(logger(PACKAGE), PACKAGE):
        exc = _raised_value_error()
        sys.excepthook(type(exc), exc, exc.__traceback__)
    assert len(log_exporter.getvalue().splitlines()) == 1


def test_uninstrument_restores_hooks(monkeypatch: pytest.MonkeyPatch) -> None:
    """Leaving the scope puts back the hooks set before it (`test_exceptions.py:312-339`)."""

    def original_sys(_exc_type: type[BaseException], _exc: BaseException, _tb: TracebackType | None) -> None:
        """Stands as the hook set before the scope."""

    def original_threading(_args: threading.ExceptHookArgs) -> None:
        """Stands as the hook set before the scope."""

    def original_asyncio(_loop: asyncio.AbstractEventLoop, _context: dict[str, object]) -> None:
        """Stands as the loop handler set before the scope."""

    monkeypatch.setattr(sys, "excepthook", original_sys)
    monkeypatch.setattr(threading, "excepthook", original_threading)
    monkeypatch.setattr(asyncio.BaseEventLoop, "call_exception_handler", original_asyncio)
    with unhandled(logger(PACKAGE), PACKAGE):
        pass
    assert sys.excepthook is original_sys
    assert threading.excepthook is original_threading
    assert asyncio.BaseEventLoop.call_exception_handler is original_asyncio
