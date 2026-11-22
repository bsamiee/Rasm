"""One process instrumented by `telemetry.process.instrumented`: its log file and levels, the state it restores, the trace context and exceptions it reads, the records it exports, the files it prunes, and its bounded release."""

from collections.abc import Callable, Sequence
from contextlib import AbstractContextManager
from datetime import timedelta
import functools
import logging
import os
from pathlib import Path
import subprocess
import sys
import threading
import time
from typing import override, Protocol
import warnings

import msgspec
from opentelemetry import trace
from opentelemetry.sdk._logs import LogRecordProcessor
from opentelemetry.sdk._logs.export import InMemoryLogRecordExporter, SimpleLogRecordProcessor
from opentelemetry.sdk.trace import SpanProcessor
import portalocker
import pytest
import structlog

from rasm.telemetry import LogSettings
from rasm.telemetry.process import instrumented

# --- [TYPES] ----------------------------------------------------------------------------


class Scope(Protocol):
    """Process scope of the test's folder, each option the `instrumented` argument of its name."""

    def __call__(
        self,
        *,
        settings: LogSettings = ...,
        scopes: Sequence[str] = ...,
        span_processors: Sequence[SpanProcessor] = ...,
        log_record_processors: Sequence[LogRecordProcessor] = ...,
        release: timedelta = ...,
    ) -> AbstractContextManager[Callable[[LogSettings], None]]:
        """Returns the scope over the options given, every other option its fixture default."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------


QUIET = {"OTEL_TRACES_EXPORTER": "none", "OTEL_METRICS_EXPORTER": "none", "OTEL_LOGS_EXPORTER": "none", "OTEL_PYTHON_DISABLED_INSTRUMENTATIONS": "system_metrics"}

# --- [SERVICES] -------------------------------------------------------------------------


class _Stuck(SpanProcessor):
    """Span processor whose shutdown blocks until released, as an exporter facing a collector that never answers."""

    def __init__(self) -> None:
        """Holds the event its shutdown waits on."""
        self.released = threading.Event()

    @override
    def shutdown(self) -> None:
        self.released.wait()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bodies(folder: Path, /) -> list[tuple[object, object]]:
    path = folder / f"suite.{os.getpid()}.log"
    return [(line["scope"], line["body"]) for line in map(msgspec.json.Decoder(dict[str, object]).decode, path.read_bytes().splitlines())]


def test_entries_and_records_pass_the_levels_their_loggers_take(scope: Scope, tmp_path: Path) -> None:
    """Loggers under `scopes` pass the settings' level, a logger the filter names its own, and every other logger `WARNING`, structlog entries and stdlib records alike, into `<service>.<pid>.log`."""
    with scope(settings=LogSettings(level="info", filter={"httpx": "debug"}), scopes=("suite",)):
        structlog.get_logger("suite.part").info("own info")
        logging.getLogger("httpx").debug("filtered debug")
        logging.getLogger("anyio").info("other info")
        logging.getLogger("anyio").warning("other warning")
    assert _bodies(tmp_path) == [("suite.part", "own info"), ("httpx", "filtered debug"), ("anyio", "other warning")]


def test_the_yielded_call_applies_other_settings_and_exit_restores_levels(scope: Scope, tmp_path: Path) -> None:
    """The call the scope yields moves the scope loggers to another settings record's level, and exit gives each logger the level it held before entry."""
    held = logging.getLogger("suite.reloaded")
    held.setLevel(logging.CRITICAL)
    with scope(settings=LogSettings(level="warning"), scopes=("suite.reloaded",)) as reload:
        held.info("before")
        reload(LogSettings(level="info"))
        held.info("after")
    assert (_bodies(tmp_path), held.level) == ([("suite.reloaded", "after")], logging.CRITICAL)


def test_exit_restores_the_process_state_entry_changed(scope: Scope) -> None:
    """Root handlers, structlog's configuration, the stdlib logger class, and warning capture are what they were before entry."""
    root = logging.getLogger()
    before = (root.handlers.copy(), structlog.is_configured(), logging.getLoggerClass(), warnings.showwarning)
    with scope():
        assert (structlog.is_configured(), len(root.handlers), warnings.showwarning is before[3]) == (True, len(before[0]) + 1, False)
    assert (root.handlers.copy(), structlog.is_configured(), logging.getLoggerClass(), warnings.showwarning) == before


def test_the_trace_context_a_parent_wrote_is_current_inside_the_scope(scope: Scope, monkeypatch: pytest.MonkeyPatch) -> None:
    """`TRACEPARENT` and `BAGGAGE` from the environment are the remote parent and baggage inside the scope, and gone after it."""
    monkeypatch.setenv("TRACEPARENT", "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01")
    with scope():
        inside = trace.get_current_span().get_span_context()
    assert (inside.trace_id, inside.span_id, inside.is_remote) == (0x4BF92F3577B34DA6A3CE929D0E0E4736, 0x00F067AA0BA902B7, True)
    assert not trace.get_current_span().get_span_context().is_valid


def test_an_unhandled_exception_logs_under_the_service(scope: Scope, tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """An exception reaching `sys.excepthook` inside the scope is a `FATAL` line of the service's logger."""
    monkeypatch.setattr(sys, "excepthook", lambda *_: None)
    with scope():
        sys.excepthook(LookupError, LookupError("no frame"), None)
    assert _bodies(tmp_path) == [("suite", "no frame")]


def test_entries_are_exported_and_sdk_diagnostics_are_not(scope: Scope, tmp_path: Path) -> None:
    """Each entry reaches the log record processors with its fields, while an SDK logger's record reaches the log once per call site and no exporter."""
    exported = InMemoryLogRecordExporter()
    with scope(scopes=("suite",), log_record_processors=(SimpleLogRecordProcessor(exported),)):
        structlog.get_logger("suite.part").warning("frame late", frame=12)
        for _ in range(3):
            logging.getLogger("opentelemetry.sdk.trace").warning("exporter down")
    (record,) = (held.log_record for held in exported.get_finished_logs())
    assert (record.body, record.severity_text, dict(record.attributes or {})["frame"]) == ("frame late", "WARN", 12)
    assert _bodies(tmp_path) == [("suite.part", "frame late"), ("opentelemetry.sdk.trace", "exporter down")]


def test_exit_waits_for_a_stuck_flush_no_longer_than_its_release(scope: Scope) -> None:
    """A span processor whose shutdown never returns delays exit by `release` and no more."""
    stuck = _Stuck()
    started = time.monotonic()
    with scope(span_processors=(stuck,), release=timedelta(milliseconds=200)):
        pass
    waited = time.monotonic() - started
    stuck.released.set()
    assert 0.2 <= waited < 1.5


def test_entry_keeps_the_newest_logs_and_every_file_of_a_running_process(scope: Scope, tmp_path: Path) -> None:
    """With two files kept, entry keeps the newest other log beside its own, removes older logs with their locks and orphaned locks, keeps every file of a process locking its crash log, and of exited crash logs keeps those holding a dump."""
    files = {
        "suite.101.log": b"old",
        ".__suite.101.lock": b"",
        "suite.102.log": b"newer",
        ".__suite.102.lock": b"",
        ".__suite.103.lock": b"",
        "suite.104.log": b"running",
        "suite.104.crash.log": b"",
        "suite.105.crash.log": b"Fatal Python error",
        "suite.106.crash.log": b"",
        "other.107.log": b"another service",
    }
    for stamp, (name, content) in enumerate(files.items()):
        (tmp_path / name).write_bytes(content)
        os.utime(tmp_path / name, ns=(stamp * 10**9, stamp * 10**9))
    with (tmp_path / "suite.104.crash.log").open("ab") as running:
        portalocker.lock(running, portalocker.LockFlags.EXCLUSIVE)
        with scope(settings=LogSettings(files=2)):
            pass
    kept = {path.name for path in tmp_path.iterdir()} - {f"suite.{os.getpid()}.log", f".__suite.{os.getpid()}.lock"}
    assert kept == {"suite.102.log", ".__suite.102.lock", "suite.104.log", "suite.104.crash.log", "suite.105.crash.log", "other.107.log"}


def test_a_crash_log_holding_a_dump_outlives_a_second_process(tmp_path: Path) -> None:
    """A process dumping a fatal error into its crash log keeps the dump after a second process of the service enters and exits, and the exiting process removes its own empty crash log."""
    entered = (
        "import faulthandler, sys; from pathlib import Path; from rasm.telemetry import LogSettings; from rasm.telemetry.process import instrumented; "
        "scope = instrumented(Path(sys.argv[1]), service='suite', version='1.2.3', settings=LogSettings(), scopes=()); scope.__enter__(); "
    )
    env = {key: value for key, value in os.environ.items() if not key.startswith(("OTEL_", "PYTHONFAULTHANDLER"))} | QUIET
    with subprocess.Popen(
        [sys.executable, "-c", f"{entered}print(flush=True); sys.stdin.readline(); faulthandler._sigsegv()", str(tmp_path)], env=env, stdin=subprocess.PIPE, stdout=subprocess.PIPE, text=True
    ) as crashing:
        assert crashing.stdout is not None
        crashing.stdout.readline()
        clean = subprocess.run([sys.executable, "-c", f"{entered}scope.__exit__(None, None, None)", str(tmp_path)], env=env, capture_output=True, text=True, check=False)
        crashing.communicate("\n")
    assert clean.returncode == 0, clean.stderr
    assert [path.name for path in tmp_path.glob("*.crash.log")] == [f"suite.{crashing.pid}.crash.log"]
    assert "Fatal Python error: Segmentation fault" in (tmp_path / f"suite.{crashing.pid}.crash.log").read_text(encoding="utf-8")


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def scope(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> Scope:
    """Returns the scope over the test's folder for service `suite`, default settings and no scope loggers unless a case names its own, under an environment naming no exporter."""
    for name in [name for name in os.environ if name.startswith(("OTEL_", "TRACEPARENT", "BAGGAGE"))]:
        monkeypatch.delenv(name)
    for name, value in QUIET.items():
        monkeypatch.setenv(name, value)
    return functools.partial(instrumented, tmp_path, service="suite", version="1.2.3", settings=LogSettings(), scopes=())
