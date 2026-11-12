"""Log entries: the settings of one program's log, the structlog chain handing each entry to stdlib handlers, handlers writing one JSON record per line to a rotated file or a stream, child process output relayed as records, and unhandled exceptions as entries."""

from collections import UserDict
from collections.abc import Generator, Iterable
from contextlib import contextmanager
from datetime import timedelta
from functools import partial
import logging
from pathlib import Path
import re
import sys
import threading
import time
import traceback
from types import TracebackType
from typing import Annotated, Final, override, TextIO, TYPE_CHECKING

from concurrent_log_handler import ConcurrentTimedRotatingFileHandler
from opentelemetry import trace
from opentelemetry.instrumentation.log_utils import std_to_otel
from opentelemetry.semconv.attributes.code_attributes import CODE_FILE_PATH, CODE_FUNCTION_NAME, CODE_LINE_NUMBER
from opentelemetry.semconv.attributes.exception_attributes import EXCEPTION_MESSAGE, EXCEPTION_STACKTRACE, EXCEPTION_TYPE
from opentelemetry.semconv.attributes.otel_attributes import OTEL_EVENT_NAME
from opentelemetry.semconv.attributes.service_attributes import SERVICE_NAME, SERVICE_VERSION
from pydantic import Field, PositiveInt, StringConstraints
from pydantic_settings import BaseSettings
from structlog.contextvars import merge_contextvars
from structlog.processors import CallsiteParameter, CallsiteParameterAdder, EventRenamer, ExceptionRenderer, JSONRenderer
from structlog.stdlib import BoundLogger, ExtraAdder, filter_by_level, NAME_TO_LEVEL, ProcessorFormatter
from structlog.typing import EventDict, ExcInfo, Processor, WrappedLogger

if TYPE_CHECKING:
    from sys import UnraisableHookArgs

# --- [TYPES] ----------------------------------------------------------------------------

type _Level = Annotated[str, StringConstraints(strip_whitespace=True, to_lower=True)]

# --- [CONSTANTS] ------------------------------------------------------------------------

_CORRELATION: Final = ("trace_id", "span_id", "trace_flags")
_LOCATION: Final = ((CallsiteParameter.PATHNAME, CODE_FILE_PATH), (CallsiteParameter.LINENO, CODE_LINE_NUMBER), (CallsiteParameter.FUNC_NAME, CODE_FUNCTION_NAME))
_SENSITIVE: Final = re.compile(r"passw(or)?d|secret|credential|authorization|cookie|(api|private)[._-]?key|(^|[._-])token$", re.IGNORECASE)

# --- [MODELS] ---------------------------------------------------------------------------


class LogSettings(BaseSettings, frozen=True, env_prefix="LOG_"):
    """Settings of one program's log, each field read from its argument, else from the variable `LOG_<FIELD>`, else its default.

    Level names are structlog's, read in any case with surrounding space ignored, and a name no stdlib level holds reads as `NOTSET`.
    A size, age, or count outside its bounds, and any unknown field, raises `pydantic.ValidationError` at construction.
    `_env_prefix` names another variable prefix for one record.
    """

    level: _Level = Field(default="info", description="Least severe entry the program's own loggers pass.")
    filter: dict[str, _Level] = Field(default={}, description="Level of each named logger, over the program's level and the `warning` every other logger passes.")
    size: PositiveInt = Field(default=8 << 20, description="Bytes the log file reaches before it rotates.")
    age: timedelta = Field(default=timedelta(days=1), ge=timedelta(seconds=1), description="Age the log file reaches before it rotates, in whole seconds.")
    files: PositiveInt = Field(default=7, description="Rotated log files kept.")
    gzip: bool = Field(default=False, description="Whether each rotated log file is compressed.")

    @property
    def severity(self) -> int:
        """Stdlib level of `level`."""
        return NAME_TO_LEVEL.get(self.level, logging.NOTSET)

    @property
    def overrides(self) -> dict[str, int]:
        """Stdlib level of each logger `filter` names."""
        return {scope: NAME_TO_LEVEL.get(name, logging.NOTSET) for scope, name in self.filter.items()}

    @property
    def unrecognized(self) -> tuple[tuple[str, str, str], ...]:
        """Setting, value, and accepted values of each level name no stdlib level holds."""
        named = (("level", self.level, self.level), *(("filter", f"{scope}={name}", name) for scope, name in self.filter.items()))
        return tuple((setting, value, " ".join(NAME_TO_LEVEL)) for setting, value, name in named if name not in NAME_TO_LEVEL)

    def handler(self, sink: Path | TextIO, /, *added: Processor, service: str, version: str) -> logging.Handler:
        """Returns the stdlib handler writing each record it receives as one JSON line, a structlog entry and a `logging` record alike, appended to a path it rotates under the settings or written to a stream.

        Lines hold, in order, `timestamp` in UTC, `severity_text`, `severity_number`, `body`, `event_name` of an entry holding `otel.event.name`, `scope` naming the logger, the current span's `trace_id`, `span_id`, and `trace_flags`, `attributes`, and `resource`.
        Attributes hold each field of the entry or `extra` of the record, a field named like a credential redacted, and `code.file.path`, `code.line.number`, and `code.function.name` of the frame that logged it.
        Failures add `exception.type`, `exception.message`, and `exception.stacktrace` with the whole cause chain.
        Every process appending to a path rotates it under one lock file, rotated files named by their UTC time, and the path's folder is created, a refusal raising `OSError`.

        Args:
            sink: Log file, or the stream of a process whose standard error is its log.
            added: Processors reading each entry after its timestamp, code location, and redaction, before its fields nest under `attributes`.
            service: Service name of every line, the program or plug-in.
            version: Service version of every line.
        """
        match sink:
            case Path():
                sink.parent.mkdir(parents=True, exist_ok=True)
                handler: logging.Handler = ConcurrentTimedRotatingFileHandler(
                    str(sink), when="S", interval=self.age // timedelta(seconds=1), backupCount=self.files, encoding="utf-8", utc=True, maxBytes=self.size, use_gzip=self.gzip
                )
            case _:
                handler = logging.StreamHandler(sink)
        handler.setFormatter(
            ProcessorFormatter(
                foreign_pre_chain=(ExtraAdder(), _CALLSITE),
                processors=(
                    _recorded,
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


class _Entry(UserDict[str, object]):
    """Entry a formatter outside structlog prints as its event."""

    @override
    def __str__(self) -> str:
        """Returns the event."""
        return str(self["event"])


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROCESSORS]

_CALLSITE: Final = CallsiteParameterAdder([parameter for parameter, _ in _LOCATION], additional_ignores=[__name__])


def correlated(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    """Adds the current span's `trace_id`, `span_id`, and `trace_flags` in their W3C hexadecimal form, a remote parent's where the process starts no span of its own."""
    span = trace.get_current_span().get_span_context()
    return {**event_dict, **dict(zip(_CORRELATION, (trace.format_trace_id(span.trace_id), trace.format_span_id(span.span_id), f"{span.trace_flags:02x}")))} if span.is_valid else event_dict


def _wrapped(logger: logging.Logger, method: str, event_dict: EventDict, /) -> tuple[tuple[EventDict], dict[str, dict[str, object]]]:
    """Hands an entry and the code location of its call to stdlib handlers in the form `ProcessorFormatter` reads, printing as its event through every other formatter."""
    return ProcessorFormatter.wrap_for_formatter(logger, method, _Entry({**event_dict, **_CALLSITE(logger, method, {})}))


def _recorded(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    """Adds the UTC time the entry's stdlib record was created, to the millisecond, and names its code location by semantic convention."""
    record = event_dict["_record"]
    located = {name: event_dict.pop(parameter.value) for parameter, name in _LOCATION if parameter.value in event_dict}
    return {"timestamp": f"{time.strftime('%Y-%m-%dT%H:%M:%S', time.gmtime(record.created))}.{record.msecs:03.0f}Z", **event_dict, **located}


def _redacted(_logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    """Replaces the value of each field named like a credential."""
    return {key: "[redacted]" if _SENSITIVE.search(key) else value for key, value in event_dict.items()}


def _failure(exc_info: ExcInfo) -> dict[str, str]:
    """Returns the exception attributes of a failure, its stack trace holding the cause chain, notes, and group members Python prints."""
    return {EXCEPTION_TYPE: exc_info[0].__name__, EXCEPTION_MESSAGE: str(exc_info[1]), EXCEPTION_STACKTRACE: "".join(traceback.format_exception(*exc_info))}


def _nested(service: str, version: str, _logger: WrappedLogger, _method: str, event_dict: EventDict, /) -> EventDict:
    """Returns an entry as one log record in key order: severity, scope, and process id from its stdlib record, a record holding no level unspecified, event name and trace context beside the body, every other field under `attributes`."""
    record = event_dict.pop("_record")
    severity = std_to_otel(record.levelno or logging.NOTSET)
    head = {"timestamp": event_dict.pop("timestamp"), "severity_text": severity.name, "severity_number": severity.value, "body": event_dict.pop("body")}
    named = {"event_name": event_dict.pop(OTEL_EVENT_NAME)} if OTEL_EVENT_NAME in event_dict else {}
    context = {key: event_dict.pop(key) for key in _CORRELATION if key in event_dict}
    attributes = {key: value for key, value in {**event_dict.pop("exception", {}), **event_dict}.items() if key != "_from_structlog"}
    return (
        head
        | named
        | {"scope": record.name}
        | context
        | ({"attributes": attributes} if attributes else {})
        | {"resource": {SERVICE_NAME: service, SERVICE_VERSION: version, "process.pid": record.process}}
    )


# --- [LOGGERS]

PROCESSORS: Final[tuple[Processor, ...]] = (merge_contextvars, filter_by_level, _wrapped)


def logger(name: str, /) -> BoundLogger:
    """Returns the bound logger over the stdlib logger of a name, outside structlog's process-wide configuration.

    Each entry at or above that logger's level reaches its handlers with context-local fields merged and the code location of its call.
    With no handler anywhere, stdlib `logging` prints the event of a warning or worse to standard error.
    """
    return BoundLogger(logging.getLogger(name), PROCESSORS, {})


# --- [PROCESSES]


def relayed(lines: Iterable[bytes], handler: logging.Handler, scope: str, /) -> Generator[str]:
    """Writes each line of another program's output through a handler as one `INFO` record under a scope.

    A parent consumes the generator on a thread for the program's whole run, so the handler's rotation bounds the output, and `collections.deque(maxlen=<count>).extend` keeps its last lines.
    Records hold the trace context current where the generator is consumed.

    Yields:
        Each line written, trailing space removed, a line that is no UTF-8 as the `repr` of its bytes, blank lines left out.
    """
    for line in filter(None, map(bytes.rstrip, lines)):
        try:
            text = line.decode()
        except UnicodeDecodeError:
            text = repr(line)
        handler.handle(logging.LogRecord(scope, logging.INFO, "", 0, text, None, None))
        yield text


# --- [EXCEPTIONS]

_UNHANDLED: Final = set[tuple[str, tuple[str, ...]]]()


@contextmanager
def unhandled(log: BoundLogger, /, *packages: str) -> Generator[None]:
    """Logs each `Exception` reaching `sys.excepthook`, `threading.excepthook`, or `sys.unraisablehook`, and each one leaving the scope, then passes it on.

    Entries hold the exception's message as body, its type name as `otel.event.name`, and its exception attributes.
    An exception ending the process (`sys.excepthook`, or leaving the scope) logs at `CRITICAL`, one ending a thread or left unraisable at `ERROR`.
    With `packages`, an exception logs only where a frame of its traceback runs a module under one of them, so a plug-in records the failures of its own code in a host process.
    Each hook calls the hook held before it, so the host prints what it printed.
    Entering again with the same logger name and packages while the scope is open installs nothing, so each exception logs once.
    Scope exit restores a held hook while this one is still installed, and leaves a hook installed over it in place with this one passing every exception on unlogged.
    """
    if (key := (log.name, packages)) in _UNHANDLED:
        yield
        return
    held = (sys.excepthook, threading.excepthook, sys.unraisablehook)

    def logged(level: int, value: BaseException | None, /) -> None:
        if (
            key in _UNHANDLED
            and isinstance(value, Exception)
            and (not packages or any(str(frame.f_globals.get("__name__", "")).startswith(packages) for frame, _ in traceback.walk_tb(value.__traceback__)))
        ):
            message = str(value)
            log.log(level, message, exc_info=value, **{OTEL_EVENT_NAME: type(value).__name__})

    def raised(kind: type[BaseException], value: BaseException, frames: TracebackType | None, /) -> None:
        logged(logging.CRITICAL, value)
        held[0](kind, value, frames)

    def threaded(arguments: threading.ExceptHookArgs, /) -> None:
        logged(logging.ERROR, arguments.exc_value)
        held[1](arguments)

    def unraisable(arguments: "UnraisableHookArgs", /) -> None:
        logged(logging.ERROR, arguments.exc_value)
        held[2](arguments)

    def released(installed: tuple[object, object, object], /) -> None:
        _UNHANDLED.discard(key)
        if sys.excepthook is installed[0]:
            sys.excepthook = held[0]
        if threading.excepthook is installed[1]:
            threading.excepthook = held[1]
        if sys.unraisablehook is installed[2]:
            sys.unraisablehook = held[2]

    _UNHANDLED.add(key)
    sys.excepthook, threading.excepthook, sys.unraisablehook = raised, threaded, unraisable
    try:
        yield
    except Exception as error:
        logged(logging.CRITICAL, error)
        raise
    finally:
        released((raised, threaded, unraisable))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROCESSORS", "LogSettings", "correlated", "logger", "relayed", "unhandled"]
