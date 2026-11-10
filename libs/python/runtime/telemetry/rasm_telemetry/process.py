"""Process instrumentation for a process entry alone: entries of every logger written to one log, the OpenTelemetry SDK the `OTEL_*` variables or a configuration file name with its release bounded, process and runtime metrics, Python warnings, unhandled exceptions, and fatal errors."""

import atexit
from collections.abc import Callable, Collection, Generator, Mapping, Sequence
from contextlib import closing, contextmanager, ExitStack
from datetime import timedelta
import faulthandler
from importlib import import_module
from importlib.util import find_spec
import logging
import os
from pathlib import Path
import sys
import threading
import time
from typing import Final, Literal, TextIO

from opentelemetry import context, metrics, propagate, trace
from opentelemetry._logs import get_logger_provider
from opentelemetry.distro import OpenTelemetryConfigurator
from opentelemetry.environment_variables import OTEL_LOGS_EXPORTER, OTEL_METRICS_EXPORTER
from opentelemetry.instrumentation.structlog import StructlogProcessor
from opentelemetry.propagators._envcarrier import EnvironmentGetter
from opentelemetry.sdk._configuration import _EXPORTER_ENV_BY_SIGNAL_TYPE, _get_exporter_names
from opentelemetry.sdk._logs import LoggerProvider, LogRecordProcessor
from opentelemetry.sdk.environment_variables import OTEL_CONFIG_FILE, OTEL_SDK_DISABLED
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.resources import SERVICE_NAME
from opentelemetry.sdk.trace import SpanProcessor, TracerProvider
from opentelemetry.util._importlib_metadata import entry_points
import structlog
from structlog.dev import ConsoleRenderer
from structlog.stdlib import add_log_level, BoundLogger, ExtraAdder, LoggerFactory, ProcessorFormatter
from structlog.typing import EventDict, Processor, WrappedLogger

from rasm_telemetry._entries import LogSettings, PROCESSORS, unhandled
from rasm_telemetry.export import detected, provider

# --- [CONSTANTS] ------------------------------------------------------------------------

_SDK: Final = logging.Filter(trace.__name__.partition(".")[0])

# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def instrumented(
    sink: Path | TextIO,
    /,
    *,
    service: str,
    version: str,
    settings: LogSettings,
    scopes: Sequence[str],
    environ: Mapping[str, str],
    span_processors: Sequence[SpanProcessor] = (),
    log_record_processors: Sequence[LogRecordProcessor] = (),
    release: timedelta = timedelta(milliseconds=500),
) -> Generator[Callable[[LogSettings], None]]:
    """Holds one process instrumented until scope exit, each part released in reverse and each signal flushed before the log closes, for `release` at most.

    Process-wide state the scope sets and restores: structlog's configuration, the stdlib logger class, the root logger's handlers, and the level of the root logger and of each logger `scopes` and the settings name.
    It routes Python warnings to the `py.warnings` logger, chains the Python exception hooks, and enables the fault handler where none was enabled.
    The root logger writes each structlog entry and `logging` record its logger passes to `sink` as one JSON line, a path rotated under the settings.
    Beside a path, an interactive standard error shows entries at `WARNING` and above as text, the service's own left to the output its entry point prints.
    Loggers under `scopes` pass the settings' level, loggers the settings' filter names pass theirs, and every other logger passes `WARNING`.
    A signal is exported where `OTEL_TRACES_EXPORTER`, `OTEL_METRICS_EXPORTER`, or `OTEL_LOGS_EXPORTER` names its exporter, where `OTEL_CONFIG_FILE` names a declarative configuration, or where a processor is passed, and never under `OTEL_SDK_DISABLED=true`.
    Without one the scope installs a tracer provider holding no processor, so spans give records and child processes their ids, and starts no thread, sends nothing, and detects no resource.
    With one the SDK installs the three API providers and reads every other `OTEL_*` variable itself, and a configuration file replaces the variables and the resource, the SDK ignoring both under it.
    Exported logs are the entries the log receives.
    Each part beyond the SDK loads where a setting asks for it and the install holds it: an exporter by the name its variable resolves to, the declarative configuration, and the process and runtime metrics of `SystemMetricsInstrumentor`.
    Those metrics register without their `system.*` instruments while metrics export, and `OTEL_PYTHON_SYSTEM_METRICS_EXCLUDED_METRICS` drops more.
    A setting naming a part the install lacks (an exporter or OTLP protocol with no installed exporter, `OTEL_CONFIG_FILE` without `opentelemetry-configuration`) is left out as if unset and logged once at `WARNING` with its variable, its value, and the accepted values.
    Records of the SDK's own loggers reach the log once per call site and are never exported, and never reach the interactive standard error.
    Scope exit shuts the three providers down together, each flushing on its own daemon thread, and waits `release` for them, a collector that is down, slow, or refusing costing the exit no more: a flush still running then ends with the process.
    The trace context a parent wrote into `environ` is current, unhandled exceptions log under the service's logger, and fatal errors dump every thread's stack to `<path stem>.crash.log`, a file removed while empty, or to a stream sink.
    A log folder refusing creation raises `OSError`, an SDK setting the SDK refuses raises `RuntimeError`, and a refused configuration file raises `opentelemetry.configuration.ConfigurationError`.

    Args:
        sink: Log file, or the stream of a process whose standard error is its log.
        service: Service name of the resource and of each line, under `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES` where a signal is exported.
        version: Service version of the resource and of each line.
        settings: Levels and rotation of the log.
        scopes: Root names of the program's own loggers.
        environ: Process environment.
        span_processors: Processors receiving every span beside the exporters, an in-memory exporter's for a test.
        log_record_processors: Processors receiving every exported log record beside the exporters.
        release: Longest wait for the final flush of every signal at scope exit.

    Yields:
        Call applying another settings record's levels to the running process, restoring the levels this scope set before it.
    """

    def lacked(signal: Literal["traces", "metrics", "logs"], installed: Collection[str], /) -> bool:
        try:
            return not set(_get_exporter_names(signal)) <= set(installed)
        except RuntimeError:
            return True

    asked = frozenset(variable for variable in _EXPORTER_ENV_BY_SIGNAL_TYPE.values() if environ.get(variable, "").strip().lower() not in {"", "none"})
    refused = {
        _EXPORTER_ENV_BY_SIGNAL_TYPE[signal]: " ".join(sorted(installed))
        for signal in ("traces", "metrics", "logs")
        if _EXPORTER_ENV_BY_SIGNAL_TYPE[signal] in asked
        for installed in (entry_points(group=f"opentelemetry_{signal}_exporter").names,)
        if lacked(signal, installed)
    } | ({OTEL_CONFIG_FILE: "unset"} if environ.get(OTEL_CONFIG_FILE, "") and find_spec("opentelemetry.configuration") is None else {})
    named = asked - refused.keys()
    declared = bool(environ.get(OTEL_CONFIG_FILE, "")) and OTEL_CONFIG_FILE not in refused
    exporting = environ.get(OTEL_SDK_DISABLED, "").strip().lower() != "true" and bool(named or declared or span_processors or log_record_processors)
    name, root, sites = service, logging.getLogger(), set[tuple[str, int]]()
    added = tuple[Processor, ...]()

    def diagnosed(record: logging.LogRecord) -> bool:
        if not _SDK.filter(record):
            return True
        fresh = (site := (record.name, record.lineno)) not in sites
        sites.add(site)
        return fresh

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
        if not exporting:
            trace.set_tracer_provider(provider(service, version))
        if exporting:
            resource = detected(service, version)
            with ExitStack() as masked:
                masked.callback(os.environ.update, {variable: environ[variable] for variable in refused})
                os.environ.update(dict.fromkeys(refused, ""))
                OpenTelemetryConfigurator().configure(
                    **(
                        {}
                        if declared
                        else {"resource_attributes": dict(resource.attributes), "setup_logging_handler": False, "span_processors": span_processors, "log_record_processors": log_record_processors}
                    )
                )
            name = str(resource.attributes[SERVICE_NAME])
        if exporting and (declared or OTEL_LOGS_EXPORTER in named or log_record_processors):
            exporter = StructlogProcessor()

            def exported(_logger: WrappedLogger, method: str, event_dict: EventDict, /) -> EventDict:
                if not _SDK.filter(event_dict["_record"]):
                    exporter(event_dict["_record"], method, dict(add_log_level(None, method, {key: value for key, value in event_dict.items() if key != "_from_structlog"})))
                return event_dict

            added = (exported,)
        handlers = [stack.enter_context(closing(settings.handler(sink, *added, service=name, version=version)))]
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
            for logger, level in {root.name: logging.WARNING, **dict.fromkeys(scopes, held.severity), **dict(held.overrides)}.items():
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
        for variable, accepted in refused.items():
            structlog.stdlib.get_logger(__name__).warning("Setting unrecognized", variable=variable, value=environ[variable], accepted=accepted)
        if exporting:
            stack.callback(released)
        for point in entry_points(group="opentelemetry_instrumentor", name="system_metrics") if exporting and (declared or OTEL_METRICS_EXPORTER in named) else ():
            part = import_module(point.module)
            measured = getattr(part, point.attr)(config={metric: labels for metric, labels in part._build_default_config().items() if not metric.startswith("system.")})
            measured.instrument()
            stack.callback(measured.uninstrument)
        stack.callback(context.detach, context.attach(propagate.extract(environ, context.get_current(), getter=EnvironmentGetter())))
        armed, dumped = not faulthandler.is_enabled(), sink
        if armed and isinstance(dumped, Path):
            crash = dumped.with_suffix(".crash.log")

            def emptied() -> None:
                if not crash.stat().st_size:
                    crash.unlink()

            stack.callback(emptied)
            dumped = stack.enter_context(crash.open("ab"))
            dumped.truncate(0 if dumped.tell() >= settings.size else None)
        if armed and not isinstance(dumped, Path):
            stack.callback(faulthandler.disable)
            faulthandler.enable(file=dumped, all_threads=True)
        stack.enter_context(unhandled(structlog.stdlib.get_logger(service)))
        yield reload


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["instrumented"]
