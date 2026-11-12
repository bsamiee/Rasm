"""Exception hooks held while `rasm_telemetry.unhandled` is entered, copied from opentelemetry-instrumentation-exceptions `tests/test_exceptions.py` (0.66b2) with entering the scope where the exemplar instruments and leaving it where the exemplar uninstruments."""

import asyncio
from collections.abc import Iterator
import contextlib
import io
import sys
import threading
from types import TracebackType

import pytest

from rasm_telemetry import logger, LogSettings, unhandled

# --- [CONSTANTS] ------------------------------------------------------------------------

PACKAGE = __name__.partition(".")[0]


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def log_exporter() -> Iterator[io.StringIO]:
    """Yields the stream receiving every entry of the package logger, one line each, where the exemplar's `log_exporter` yields an in-memory exporter (`test_exceptions.py:33-45`)."""
    stream = io.StringIO()
    handler = LogSettings().handler(stream, service="rasm-telemetry-tests", version="0")
    log = logger(PACKAGE)
    log.addHandler(handler)
    try:
        yield stream
    finally:
        log.removeHandler(handler)


def test_base_exceptions_are_not_emitted(log_exporter: io.StringIO, monkeypatch: pytest.MonkeyPatch) -> None:
    """A `KeyboardInterrupt` reaching `sys.excepthook` writes no entry and the hook set before still runs (`test_exceptions.py:276-293`)."""
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
    """Entering twice writes one entry per exception (`test_exceptions.py:296-309`)."""
    monkeypatch.setattr(sys, "excepthook", lambda _exc_type, _exc, _tb: None)
    with unhandled(logger(PACKAGE), PACKAGE), unhandled(logger(PACKAGE), PACKAGE):
        exc = ValueError("boom")
        with contextlib.suppress(ValueError):
            raise exc
        sys.excepthook(type(exc), exc, exc.__traceback__)
        assert len(log_exporter.getvalue().splitlines()) == 1


def test_uninstrument_restores_hooks(monkeypatch: pytest.MonkeyPatch) -> None:
    """Leaving the scope puts back the hooks set before it was entered (`test_exceptions.py:312-339`)."""

    def original_sys(_exc_type: type[BaseException], _exc: BaseException, _tb: TracebackType | None) -> None:
        return None

    def original_threading(_args: threading.ExceptHookArgs) -> None:
        return None

    def original_asyncio(_loop: asyncio.BaseEventLoop, _context: dict[str, object]) -> None:
        return None

    monkeypatch.setattr(sys, "excepthook", original_sys)
    monkeypatch.setattr(threading, "excepthook", original_threading)
    monkeypatch.setattr(asyncio.BaseEventLoop, "call_exception_handler", original_asyncio)
    with unhandled(logger(PACKAGE), PACKAGE):
        pass
    assert sys.excepthook is original_sys
    assert threading.excepthook is original_threading
    assert asyncio.BaseEventLoop.call_exception_handler is original_asyncio
