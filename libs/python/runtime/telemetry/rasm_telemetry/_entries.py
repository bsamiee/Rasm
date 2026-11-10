"""Log entries: the settings of one program's log, the structlog chain handing each entry to stdlib handlers, handlers writing one JSON record per line to a rotated file or a stream, child process output relayed as records, and unhandled exceptions as entries."""

from collections.abc import Generator, Iterable
from contextlib import contextmanager
from dataclasses import dataclass
from datetime import timedelta
from functools import cached_property, partial
import logging
from pathlib import Path
import re
import sys
import threading
import traceback
from types import TracebackType
from typing import Final, TextIO, TYPE_CHECKING
from urllib.parse import parse_qsl

from concurrent_log_handler import ConcurrentTimedRotatingFileHandler
from opentelemetry import trace
from opentelemetry.instrumentation.log_utils import std_to_otel
from opentelemetry.semconv.attributes.code_attributes import CODE_FILE_PATH, CODE_FUNCTION_NAME, CODE_LINE_NUMBER
from opentelemetry.semconv.attributes.exception_attributes import EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, EXCEPTION_TYPE
from opentelemetry.semconv.attributes.service_attributes import SERVICE_NAME, SERVICE_VERSION
from structlog.contextvars import merge_contextvars
from structlog.processors import CallsiteParameter, CallsiteParameterAdder, EventRenamer, ExceptionRenderer, JSONRenderer, TimeStamper
from structlog.stdlib import BoundLogger, ExtraAdder, filter_by_level, NAME_TO_LEVEL, ProcessorFormatter
from structlog.typing import EventDict, ExcInfo, Processor, WrappedLogger

if TYPE_CHECKING:
    from sys import UnraisableHookArgs

# --- [CONSTANTS] ------------------------------------------------------------------------

_CORRELATION: Final = ("trace_id", "span_id", "trace_flags")
_LOCATION: Final = ((CallsiteParameter.PATHNAME, CODE_FILE_PATH), (CallsiteParameter.LINENO, CODE_LINE_NUMBER), (CallsiteParameter.FUNC_NAME, CODE_FUNCTION_NAME))
_SENSITIVE: Final = re.compile(r"passw(or)?d|secret|credential|authorization|cookie|(api|private)[._-]?key|(^|[._-])token$", re.IGNORECASE)

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, kw_only=True)
class LogSettings:
    """Settings of one program's log, each level a stdlib level name the record resolves into its `logging` level, and each value outside its domain leaving its default in effect.

    Attributes:
        level: Least severe entry the program's own loggers pass, a stdlib level name.
        filter: Level of each named logger as comma-separated `<logger>=<level>` pairs, over the program's level and the `warning` every other logger passes.
        size: Bytes the log file reaches before it rotates.
        age: Age the log file reaches before it rotates.
        files: Rotated log files kept.
    """

    level: str = "info"
    filter: str = ""
    size: int = 8 << 20
    age: timedelta = timedelta(days=1)
    files: int = 7

    @cached_property
    def _pairs(self) -> tuple[tuple[str, str], ...]:
        """Logger name and lowercase level name of each `filter` pair."""
        return tuple((scope.strip(), name.strip().lower()) for scope, name in parse_qsl(self.filter, keep_blank_values=True, separator=","))

    @cached_property
    def _bounds(self) -> tuple[tuple[str, int, int], ...]:
        """Setting, given value, and value in effect of the size in bytes, age in whole seconds, and count, the default in effect for a value under one."""
        default = type(self)
        given = (("size", self.size, default.size), ("age", int(self.age.total_seconds()), int(default.age.total_seconds())), ("files", self.files, default.files))
        return tuple((name, held, held if held >= 1 else fallback) for name, held, fallback in given)

    @property
    def severity(self) -> int:
        """Stdlib level of `level`, of the default level where no stdlib level holds the name."""
        return NAME_TO_LEVEL.get(self.level.lower(), NAME_TO_LEVEL[type(self).level])

    @property
    def overrides(self) -> tuple[tuple[str, int], ...]:
        """Logger name and stdlib level of each `filter` pair naming a stdlib level."""
        return tuple((scope, NAME_TO_LEVEL[name]) for scope, name in self._pairs if name in NAME_TO_LEVEL)

    @property
    def unrecognized(self) -> tuple[tuple[str, str, str], ...]:
        """Setting, value, and accepted values of each value outside its domain: a level name no stdlib level holds, and a size, age, or count under one."""
        levels = " ".join(NAME_TO_LEVEL)
        return (
            *((("level", self.level, levels),) if self.level.lower() not in NAME_TO_LEVEL else ()),
            *(("filter", f"{scope}={name}", levels) for scope, name in self._pairs if name not in NAME_TO_LEVEL),
            *((name, str(held), "1 or more") for name, held, effective in self._bounds if held != effective),
        )

    def handler(self, sink: Path | TextIO, /, *added: Processor, service: str, version: str) -> logging.Handler:
        """Returns the stdlib handler writing each record it receives as one JSON line, a structlog entry and a `logging` record alike, appended to a path it rotates under the settings or written to a stream.

        Lines hold, in order, `timestamp` in UTC, `severity_text`, `severity_number`, `body`, `scope` naming the logger, the current span's `trace_id`, `span_id`, and `trace_flags`, `attributes`, and `resource`.
        Attributes hold each field of the entry or `extra` of the record, a field named like a credential redacted, and `code.file.path`, `code.line.number`, and `code.function.name` at `WARNING` and above.
        Failures add `exception.type`, `exception.message`, and at `ERROR` and above `exception.stacktrace` with the whole cause chain.
        Every process appending to a path rotates it under one lock file, rotated files named by their UTC time, and the path's folder is created, a refusal raising `OSError`.

        Args:
            sink: Log file, or the stream of a process whose standard error is its log.
            added: Processors reading each entry after its timestamp, code location, and redaction, before its fields nest under `attributes`.
            service: Service name of every line, the program or plug-in.
            version: Service version of every line.
        """
        (_, _, size), (_, _, age), (_, _, files) = self._bounds
        match sink:
            case Path():
                sink.parent.mkdir(parents=True, exist_ok=True)
                handler: logging.Handler = ConcurrentTimedRotatingFileHandler(str(sink), when="S", interval=age, backupCount=files, encoding="utf-8", utc=True, maxBytes=size)
            case _:
                handler = logging.StreamHandler(sink)
        handler.setFormatter(
            ProcessorFormatter(
                foreign_pre_chain=(ExtraAdder(),),
                processors=(
                    TimeStamper(fmt="iso", utc=True),
                    _located,
                    _redacted,
                    *added,
                    correlated,
                    ExceptionRenderer(_failure),
                    EventRenamer("body"),
                    partial(_nested, service, version),
                    JSONRenderer(ensure_ascii=False, separators=(",", ":")),
                ),
            )
        )
        return handler


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOGGERS]

PROCESSORS: Final[tuple[Processor, ...]] = (merge_contextvars, filter_by_level, ProcessorFormatter.wrap_for_formatter)


def logger(name: str, /) -> BoundLogger:
    """Returns the bound logger over the stdlib logger of a name, outside structlog's process-wide configuration, each entry at or above that logger's level reaching its handlers with context-local fields merged."""
    return BoundLogger(logging.getLogger(name), PROCESSORS, {})


# --- [PROCESSORS]

_CALLSITE: Final = CallsiteParameterAdder([parameter for parameter, _ in _LOCATION], additional_ignores=[__name__, ConcurrentTimedRotatingFileHandler.__module__])


def correlated(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    """Adds the current span's `trace_id`, `span_id`, and `trace_flags` in their W3C hexadecimal form, a remote parent's where the process starts no span of its own."""
    span = trace.get_current_span().get_span_context()
    return {**event_dict, **dict(zip(_CORRELATION, (trace.format_trace_id(span.trace_id), trace.format_span_id(span.span_id), f"{span.trace_flags:02x}")))} if span.is_valid else event_dict


def _located(logger: WrappedLogger, method: str, event_dict: EventDict, /) -> EventDict:
    """Adds the code location of an entry at `WARNING` and above under its semantic-convention names, the frame that logged it."""
    if event_dict["_record"].levelno < logging.WARNING:
        return event_dict
    located = _CALLSITE(logger, method, event_dict)
    renamed = {name: located.pop(parameter.value) for parameter, name in _LOCATION}
    return {**located, **renamed}


def _redacted(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    """Replaces the value of each field named like a credential."""
    return {key: "[redacted]" if _SENSITIVE.search(key) else value for key, value in event_dict.items()}


def _failure(exc_info: ExcInfo) -> dict[str, str]:
    """Returns the exception attributes of a failure, its stack trace holding the cause chain, notes, and group members Python prints."""
    return {EXCEPTION_TYPE: exc_info[0].__name__, EXCEPTION_MESSAGE: str(exc_info[1]), EXCEPTION_STACKTRACE: "".join(traceback.format_exception(*exc_info))}


def _nested(service: str, version: str, _logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    """Returns an entry as one log record in key order: severity, scope, and process id from its stdlib record, trace context beside the body, every other field under `attributes`, a stack trace kept at `ERROR` and above."""
    record = event_dict.pop("_record")
    severity = std_to_otel(record.levelno)
    head = {"timestamp": event_dict.pop("timestamp"), "severity_text": severity.name, "severity_number": severity.value, "body": event_dict.pop("body"), "scope": record.name}
    context = {key: event_dict.pop(key) for key in _CORRELATION if key in event_dict}
    fields = {**event_dict.pop("exception", {}), **event_dict}
    attributes = {key: value for key, value in fields.items() if key != "_from_structlog" and (key != EXCEPTION_STACKTRACE or record.levelno >= logging.ERROR)}
    return head | context | ({"attributes": attributes} if attributes else {}) | {"resource": {SERVICE_NAME: service, SERVICE_VERSION: version, "process.pid": record.process}}


# --- [PROCESSES]


def relayed(lines: Iterable[bytes], handler: logging.Handler, scope: str, /) -> Generator[str]:
    """Writes each line of another program's output through a handler as one `INFO` record under a scope.

    A parent consumes the generator on a thread for the program's whole run, so the handler's rotation bounds the output, and `collections.deque(maxlen=<count>).extend` keeps its last lines.
    Records hold the trace context current where the generator is consumed.

    Yields:
        Each line written, blank lines left out.
    """
    for text in filter(None, (line.decode(errors="replace").rstrip() for line in lines)):
        handler.handle(logging.LogRecord(scope, logging.INFO, "", 0, text, None, None))
        yield text


# --- [EXCEPTIONS]


@contextmanager
def unhandled(log: BoundLogger, /, *packages: str) -> Generator[None]:
    """Logs each exception reaching `sys.excepthook`, `threading.excepthook`, or `sys.unraisablehook`, and each one leaving the scope, as a `CRITICAL` entry, then passes it on.

    With `packages`, an exception logs only where a frame of its traceback runs a module under one of them, so a plug-in records the failures of its own code in a host process.
    Each hook calls the hook held before it, so the host prints what it printed.
    Scope exit restores a held hook while this one is still installed, and leaves a hook installed over it in place with this one passing every exception on unlogged.
    """
    held, scope = (sys.excepthook, threading.excepthook, sys.unraisablehook), threading.Event()

    def logged(kind: type[BaseException] | None, value: BaseException | None, frames: TracebackType | None, /) -> None:
        if scope.is_set() and kind is not None and (not packages or any(str(frame.f_globals.get("__name__", "")).startswith(packages) for frame, _ in traceback.walk_tb(frames))):
            log.critical("Exception unhandled", exc_info=(kind, value, frames))

    def raised(kind: type[BaseException], value: BaseException, frames: TracebackType | None, /) -> None:
        logged(kind, value, frames)
        held[0](kind, value, frames)

    def threaded(arguments: threading.ExceptHookArgs, /) -> None:
        logged(arguments.exc_type, arguments.exc_value, arguments.exc_traceback)
        held[1](arguments)

    def unraisable(arguments: "UnraisableHookArgs", /) -> None:
        logged(arguments.exc_type, arguments.exc_value, arguments.exc_traceback)
        held[2](arguments)

    def released(installed: tuple[object, object, object], /) -> None:
        scope.clear()
        if sys.excepthook is installed[0]:
            sys.excepthook = held[0]
        if threading.excepthook is installed[1]:
            threading.excepthook = held[1]
        if sys.unraisablehook is installed[2]:
            sys.unraisablehook = held[2]

    scope.set()
    sys.excepthook, threading.excepthook, sys.unraisablehook = raised, threaded, unraisable
    try:
        yield
    except Exception as error:
        logged(type(error), error, error.__traceback__)
        raise
    finally:
        released((raised, threaded, unraisable))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROCESSORS", "LogSettings", "correlated", "logger", "relayed", "unhandled"]
