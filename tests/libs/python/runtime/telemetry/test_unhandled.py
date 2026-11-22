"""Exceptions reaching the process hooks or leaving the scope of `telemetry.unhandled`: their records, the package filter, the hooks held before, and the hooks restored after."""

from collections.abc import Iterator
import functools
import gc
import logging
import sys
import threading
from types import TracebackType

import msgspec
import pytest
from structlog.stdlib import BoundLogger

from rasm.telemetry import logger, unhandled

# --- [OPERATIONS] -----------------------------------------------------------------------


def _raised(error: Exception, /) -> BaseException:
    with pytest.raises(type(error)) as caught:
        raise error
    return caught.value


class _Collected:
    """Object whose finalizer raises, so the interpreter hands its exception to `sys.unraisablehook`."""

    def __del__(self) -> None:
        raise ValueError("finalizer failed")


def test_an_exception_reaching_the_process_hook_is_one_fatal_record(log: BoundLogger, written: list[dict[str, object]], held: list[object], monkeypatch: pytest.MonkeyPatch) -> None:
    """`sys.excepthook` writes a `FATAL` record whose body is the message, `event_name` the type, and attributes the exception, then calls the hook held before."""
    monkeypatch.setattr(sys, "excepthook", lambda kind, _value, _frames: held.append(kind))
    with unhandled(log):
        error = _raised(KeyError("frames"))
        sys.excepthook(type(error), error, error.__traceback__)
    (line,) = written
    attributes = line["attributes"]
    assert isinstance(attributes, dict)
    assert (line["severity_text"], line["body"], line["event_name"], attributes["exception.type"]) == ("FATAL", "'frames'", "KeyError", "KeyError")
    assert held == [KeyError]


def test_an_exception_ending_a_thread_is_one_error_record(log: BoundLogger, written: list[dict[str, object]], held: list[object], monkeypatch: pytest.MonkeyPatch) -> None:
    """An exception ending a thread writes an `ERROR` record and reaches the thread hook held before."""
    monkeypatch.setattr(threading, "excepthook", lambda arguments: held.append(arguments.exc_type))
    with unhandled(log):
        thread = threading.Thread(target=functools.partial(msgspec.json.decode, b"frames"))
        thread.start()
        thread.join()
    assert [(line["severity_text"], line["event_name"]) for line in written] == [("ERROR", "DecodeError")]
    assert held == [msgspec.DecodeError]


def test_an_unraisable_exception_is_one_error_record(log: BoundLogger, written: list[dict[str, object]], held: list[object], monkeypatch: pytest.MonkeyPatch) -> None:
    """An exception a finalizer raises writes an `ERROR` record and reaches the unraisable hook held before."""
    monkeypatch.setattr(sys, "unraisablehook", lambda arguments: held.append(arguments.exc_type))
    with unhandled(log):
        _Collected()
        gc.collect()
    assert [(line["severity_text"], line["body"]) for line in written] == [("ERROR", "finalizer failed")]
    assert held == [ValueError]


def test_an_exception_leaving_the_scope_is_logged_and_raised(log: BoundLogger, written: list[dict[str, object]]) -> None:
    """An exception leaving the scope writes a `FATAL` record and propagates unchanged."""
    with pytest.raises(LookupError, match="no frame"), unhandled(log):
        raise LookupError("no frame")
    assert [(line["severity_text"], line["event_name"]) for line in written] == [("FATAL", "LookupError")]


def test_a_base_exception_is_passed_on_unlogged(log: BoundLogger, written: list[dict[str, object]], held: list[object], monkeypatch: pytest.MonkeyPatch) -> None:
    """A `KeyboardInterrupt` reaching the hook writes nothing and reaches the hook held before."""
    monkeypatch.setattr(sys, "excepthook", lambda kind, _value, _frames: held.append(kind))
    with unhandled(log):
        sys.excepthook(KeyboardInterrupt, KeyboardInterrupt(), None)
    assert (written, held) == ([], [KeyboardInterrupt])


@pytest.mark.parametrize("packages,logged", [((), 1), (("tests", "elsewhere"), 1), (("elsewhere",), 0)])
def test_packages_keep_the_exceptions_their_modules_raised(log: BoundLogger, written: list[dict[str, object]], monkeypatch: pytest.MonkeyPatch, packages: tuple[str, ...], logged: int) -> None:
    """With packages named, an exception logs only where a frame of its traceback runs a module under one of them."""
    monkeypatch.setattr(sys, "excepthook", lambda *_: None)
    with unhandled(log, *packages):
        error = _raised(ValueError("frames"))
        sys.excepthook(type(error), error, error.__traceback__)
    assert len(written) == logged


def test_entering_twice_logs_each_exception_once(log: BoundLogger, written: list[dict[str, object]], monkeypatch: pytest.MonkeyPatch) -> None:
    """A nested scope of the same logger and packages installs nothing, so one exception writes one record."""
    monkeypatch.setattr(sys, "excepthook", lambda *_: None)
    with unhandled(log), unhandled(log):
        error = _raised(ValueError("frames"))
        sys.excepthook(type(error), error, error.__traceback__)
    assert len(written) == 1


def test_leaving_restores_the_held_hooks(log: BoundLogger, monkeypatch: pytest.MonkeyPatch) -> None:
    """Scope exit puts back the process, thread, and unraisable hooks held at entry."""

    def process(_kind: type[BaseException], _value: BaseException, _frames: TracebackType | None, /) -> None: ...

    def thread(_arguments: threading.ExceptHookArgs, /) -> None: ...

    def unraisable(_arguments: object, /) -> None: ...

    monkeypatch.setattr(sys, "excepthook", process)
    monkeypatch.setattr(threading, "excepthook", thread)
    monkeypatch.setattr(sys, "unraisablehook", unraisable)
    with unhandled(log):
        assert (sys.excepthook, threading.excepthook, sys.unraisablehook) != (process, thread, unraisable)
    assert (sys.excepthook, threading.excepthook, sys.unraisablehook) == (process, thread, unraisable)


def test_a_hook_installed_over_the_scope_stays_and_reaches_the_held_hook_unlogged(log: BoundLogger, written: list[dict[str, object]], held: list[object], monkeypatch: pytest.MonkeyPatch) -> None:
    """A hook another owner installs over the scope's stays after exit, and an exception it passes on reaches the held hook without a record."""
    monkeypatch.setattr(sys, "excepthook", lambda kind, _value, _frames: held.append(kind))
    with unhandled(log):
        installed = sys.excepthook
        monkeypatch.setattr(sys, "excepthook", functools.partial(installed))
        over = sys.excepthook
    error = _raised(ValueError("frames"))
    sys.excepthook(type(error), error, error.__traceback__)
    assert sys.excepthook is over
    assert (written, held) == ([], [ValueError])


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def log(handler: logging.Handler) -> Iterator[BoundLogger]:
    """Yields the bound logger the scope logs to, its stdlib logger writing through the test's handler."""
    bound = logger("suite.hooks")
    bound.addHandler(handler)
    yield bound
    bound.removeHandler(handler)


@pytest.fixture
def held() -> list[object]:
    """Returns the exception types each hook held before the scope received."""
    return []
