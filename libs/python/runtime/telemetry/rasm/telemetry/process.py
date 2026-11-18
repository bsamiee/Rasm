"""Process instrumentation for a process entry alone: entries of every logger written to one log, the OpenTelemetry SDK the process environment configures with its release bounded, process and runtime metrics, Python warnings, unhandled exceptions, and fatal errors."""

import atexit
from collections.abc import Callable, Generator, Sequence
from contextlib import closing, contextmanager, ExitStack
from datetime import timedelta
import faulthandler
from importlib import import_module
from io import BufferedWriter
import logging
import os
from pathlib import Path
import re
import sys
import threading
import time
from typing import Final, TextIO

from opentelemetry import context, metrics, propagate, trace
from opentelemetry._logs import get_logger_provider
from opentelemetry.distro import OpenTelemetryConfigurator
from opentelemetry.instrumentation.structlog import StructlogProcessor
from opentelemetry.propagators._envcarrier import EnvironmentGetter
from opentelemetry.sdk._logs import LoggerProvider, LogRecordProcessor
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.trace import SpanProcessor, TracerProvider
from opentelemetry.util._importlib_metadata import entry_points
import portalocker
import structlog
from structlog.dev import ConsoleRenderer
from structlog.stdlib import add_log_level, BoundLogger, ExtraAdder, LoggerFactory, ProcessorFormatter
from structlog.typing import EventDict, WrappedLogger

from rasm.telemetry._entries import LogSettings, PROCESSORS, unhandled
from rasm.telemetry.export import detected

# --- [CONSTANTS] ------------------------------------------------------------------------

_SDK: Final = logging.Filter(trace.__name__.partition(".")[0])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _held(path: Path, /) -> BufferedWriter:
    held = path.open("ab")
    portalocker.lock(held, portalocker.LockFlags.EXCLUSIVE)
    return held


@contextmanager
def instrumented(
    sink: Path | TextIO,
    /,
    *,
    service: str,
    version: str,
    settings: LogSettings,
    scopes: Sequence[str],
    span_processors: Sequence[SpanProcessor] = (),
    log_record_processors: Sequence[LogRecordProcessor] = (),
    release: timedelta = timedelta(milliseconds=500),
) -> Generator[Callable[[LogSettings], None]]:
    """Holds one process instrumented until scope exit, each part released in reverse and each signal flushed before the log closes, for `release` at most.

    Process-wide state the scope sets and restores: structlog's configuration, the stdlib logger class, the root logger's handlers, and the level of the root logger and of each logger `scopes` and the settings name.
    It routes Python warnings to the `py.warnings` logger, chains the Python exception hooks, and enables the fault handler where none was enabled.
    The root logger writes each structlog entry and `logging` record its logger passes to `sink` as one JSON line, a folder holding the process's own file `<service>.<pid>.log` rotated under the settings.
    Beside a folder, an interactive standard error shows entries at `WARNING` and above as text, the service's own left to the output its entry point prints.
    Loggers under `scopes` pass the settings' level, loggers the settings' filter names pass theirs, and every other logger passes `WARNING`.
    The SDK configures itself from the process environment as `opentelemetry.distro.OpenTelemetryConfigurator` reads it, and installs the three API providers over the processors passed.
    `OTEL_TRACES_EXPORTER`, `OTEL_METRICS_EXPORTER`, and `OTEL_LOGS_EXPORTER` name each signal's exporters, and `OTEL_SDK_DISABLED=true` leaves every provider recording and sending nothing.
    `OTEL_CONFIG_FILE` names a declarative configuration replacing the variables, the resource, and the processors passed.
    The service name and version are the resource's over `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES`, and exported logs are the entries the log receives.
    Process and runtime metrics of `SystemMetricsInstrumentor` register where its distribution is installed, without its `system.*` instruments, and `OTEL_PYTHON_SYSTEM_METRICS_EXCLUDED_METRICS` drops more.
    Records of the SDK's own loggers reach the log once per call site and are never exported, and never reach the interactive standard error.
    Scope exit shuts the three providers down together, each flushing on its own daemon thread, and waits `release` for them, a collector that is down, slow, or refusing costing the exit no more: a flush still running then ends with the process.
    The trace context a parent wrote into the process environment is current, and unhandled exceptions log under the service's logger.
    Fatal errors dump every thread's stack to the folder's `<service>.<pid>.crash.log`, a file the process alone writes, locks, and removes while empty, or to a stream sink.
    Entry keeps the service's newest `settings.files` log files across its processes, its own counted, and removes older files and the lock files left without one, a process holding its crash log locked keeping every file.
    Entry removes the crash logs no running process locks, keeping the newest `settings.files` holding a dump, and a child forked inside the scope dumps to a crash log of its own pid.
    A log folder refusing creation raises `OSError`, an exporter, protocol, or setting the SDK refuses and `OTEL_CONFIG_FILE` without `opentelemetry-configuration` raise `RuntimeError`, and a refused configuration file raises `opentelemetry.configuration.ConfigurationError`.

    Args:
        sink: Log folder, or the stream of a process whose standard error is its log.
        service: Service name of the resource and of each line.
        version: Service version of the resource and of each line.
        settings: Levels and rotation of the log.
        scopes: Root names of the program's own loggers.
        span_processors: Processors receiving every span beside the exporters, an in-memory exporter's for a test.
        log_record_processors: Processors receiving every exported log record beside the exporters.
        release: Longest wait for the final flush of every signal at scope exit.

    Yields:
        Call applying another settings record's levels to the running process, restoring the levels this scope set before it.
    """
    root, sites = logging.getLogger(), set[tuple[str, int]]()

    def diagnosed(record: logging.LogRecord) -> bool:
        if not _SDK.filter(record):
            return True
        fresh = (site := (record.name, record.lineno)) not in sites
        sites.add(site)
        return fresh

    def abandoned(path: Path, /) -> os.stat_result | None:
        try:
            with path.open("rb") as probe:
                portalocker.lock(probe, portalocker.LockFlags.EXCLUSIVE | portalocker.LockFlags.NON_BLOCKING)
                return os.fstat(probe.fileno())
        except (portalocker.AlreadyLocked, FileNotFoundError):
            return None

    def modified(path: Path, /) -> int | None:
        try:
            return path.stat().st_mtime_ns
        except FileNotFoundError:
            return None

    def released() -> None:
        owned = tuple(held for held in (trace.get_tracer_provider(), metrics.get_meter_provider(), get_logger_provider()) if isinstance(held, TracerProvider | MeterProvider | LoggerProvider))
        flushes = tuple(threading.Thread(target=held.shutdown, daemon=True) for held in owned)
        ends = time.monotonic() + release.total_seconds()
        for held, flush in zip(owned, flushes):
            atexit.unregister(held.shutdown)
            flush.start()
        for flush in flushes:
            flush.join(max(ends - time.monotonic(), 0.0))
        if any(flush.is_alive() for flush in flushes):
            logging.getLogger(_SDK.name).addHandler(logging.NullHandler())

    with ExitStack() as stack:
        OpenTelemetryConfigurator().configure(
            resource_attributes=dict(detected(service, version).attributes), setup_logging_handler=False, span_processors=span_processors, log_record_processors=log_record_processors
        )
        exporter = StructlogProcessor()

        def exported(_logger: WrappedLogger, method: str, event_dict: EventDict, /) -> EventDict:
            if not _SDK.filter(event_dict["_record"]):
                exporter(event_dict["_record"], method, dict(add_log_level(None, method, {key: value for key, value in event_dict.items() if key != "_from_structlog"})))
            return event_dict

        handlers = [stack.enter_context(closing(settings.handler(sink / f"{service}.{os.getpid()}.log" if isinstance(sink, Path) else sink, exported, service=service, version=version)))]
        handlers[0].addFilter(diagnosed)
        if isinstance(sink, Path) and sys.stderr.isatty():
            console = logging.StreamHandler()
            console.setLevel(logging.WARNING)
            console.addFilter(lambda record: record.name != service and not _SDK.filter(record))
            console.setFormatter(ProcessorFormatter(foreign_pre_chain=(ExtraAdder(),), processors=(add_log_level, ProcessorFormatter.remove_processors_meta, ConsoleRenderer(colors=False))))
            handlers.append(console)
        for handler in handlers:
            stack.callback(root.removeHandler, handler)
            root.addHandler(handler)
        levels = stack.enter_context(ExitStack())

        def reload(held: LogSettings, /) -> None:
            levels.close()
            for logger, level in {root.name: logging.WARNING, **dict.fromkeys(scopes, held.severity), **held.overrides}.items():
                leveled = logging.getLogger(logger)
                levels.callback(leveled.setLevel, leveled.level)
                leveled.setLevel(level)

        reload(settings)
        if structlog.is_configured():
            stack.callback(structlog.configure, **structlog.get_config())
        else:
            stack.callback(structlog.reset_defaults)
        stack.callback(logging.setLoggerClass, logging.getLoggerClass())
        structlog.configure(processors=PROCESSORS, wrapper_class=BoundLogger, logger_factory=LoggerFactory(), cache_logger_on_first_use=True)
        stack.callback(logging.captureWarnings, capture=False)
        logging.captureWarnings(capture=True)
        stack.callback(released)
        for point in entry_points(group="opentelemetry_instrumentor", name="system_metrics"):
            part = import_module(point.module)
            measured = getattr(part, point.attr)(config={metric: labels for metric, labels in part._build_default_config().items() if not metric.startswith("system.")})
            measured.instrument()
            stack.callback(measured.uninstrument)
        stack.callback(context.detach, context.attach(propagate.extract(os.environ, context.get_current(), getter=EnvironmentGetter())))
        if isinstance(sink, Path):
            named = re.compile(rf"(?:\.__)?{re.escape(service)}\.(\d+)\.(crash\.log|lock|log(?:\..+)?)")
            found = {path: (int(hit[1]), hit[2]) for path in sink.iterdir() if (hit := named.fullmatch(path.name)) and int(hit[1]) != os.getpid()}
            ended = {path: held for path, (_, kind) in found.items() if kind == "crash.log" and (held := abandoned(path))}
            live = {pid for path, (pid, kind) in found.items() if kind == "crash.log" and path not in ended}
            logs = sorted(((stamp, path) for path, (pid, kind) in found.items() if kind.startswith("log") and pid not in live and (stamp := modified(path)) is not None), reverse=True)
            dumps = sorted((path for path, held in ended.items() if held.st_size), key=lambda path: ended[path].st_mtime_ns, reverse=True)
            kept = live | {found[path][0] for _, path in logs[: settings.files - 1]}
            stale = {path for _, path in logs[settings.files - 1 :]} | {path for path, (pid, kind) in found.items() if kind == "lock" and pid not in kept}
            for path in stale | (ended.keys() - set(dumps[: settings.files])):
                path.unlink(missing_ok=True)
        armed, dumped = not faulthandler.is_enabled(), sink
        if armed and isinstance(dumped, Path):
            folder, running = dumped, threading.Event()

            def crashed() -> Path:
                return folder / f"{service}.{os.getpid()}.crash.log"

            def emptied() -> None:
                if not (crash := crashed()).stat().st_size:
                    crash.unlink()

            def forked() -> None:
                if running.is_set():
                    faulthandler.enable(file=_held(crashed()), all_threads=True)

            stack.callback(emptied)
            dumped = stack.enter_context(_held(crashed()))
            running.set()
            stack.callback(running.clear)
            if hasattr(os, "register_at_fork"):
                os.register_at_fork(after_in_child=forked)
        if armed and not isinstance(dumped, Path):
            stack.callback(faulthandler.disable)
            faulthandler.enable(file=dumped, all_threads=True)
        stack.enter_context(unhandled(structlog.stdlib.get_logger(service)))
        yield reload


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["instrumented"]
