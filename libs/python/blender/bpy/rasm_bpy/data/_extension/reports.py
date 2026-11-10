"""Extension log file under its settings with span export, a background command run as its own program, callback fault logging, report entries of operator faults, and the main-thread queue of requests the host applies while its interface is unlocked."""

from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, closing, contextmanager, ExitStack
from dataclasses import dataclass, field
from enum import StrEnum
import functools
from itertools import product
import logging
from pathlib import Path
import subprocess
from threading import Event, RLock
from types import MappingProxyType
from typing import override, Protocol, Self, TYPE_CHECKING

import bpy
from expression import Option, Some
from expression.extra.result import catch
from opentelemetry import trace
from opentelemetry.sdk.environment_variables import OTEL_SDK_DISABLED
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.semconv._incubating.attributes.process_attributes import PROCESS_EXIT_CODE
from opentelemetry.semconv.attributes.error_attributes import ERROR_TYPE
from opentelemetry.trace import Status, StatusCode

from rasm_bpy.data._rna.paths import root_in
from rasm_records import Fault, Nothing, Ok, Outcome, Reason
from rasm_telemetry import logger, LogSettings, unhandled
from rasm_telemetry.export import ManualSpanProcessor, provider
from rasm_telemetry.otlp import Collector
from rasm_telemetry.process import instrumented

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Pending(Protocol):
    """Request the host applies over later passes."""

    def advanced(self) -> Outcome[Option[Self]]:
        """Applies one step, returning the request left to apply, Nothing once done."""


class Refusal(StrEnum):
    """Host refusals the extension reports, each value the report template of its fault."""

    TAKEN = "{subject} is taken at {member}"
    REQUIRED = "Requires {subject}"
    REFUSED = "{subject} refused edit"
    UNREGISTERED = "Registration of {subject} refused"


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class _Write:
    """One deferred write call, equal to another call of the same write and scope with equal arguments, applying nothing once its scope closed."""

    applied: functools.partial[Outcome[Option[str]]] = field(compare=False)
    scope: Event
    call: tuple[object, tuple[object, ...], Mapping[str, object]]

    def advanced(self) -> Outcome[Option[Self]]:
        """Runs the scoped write, pushing its undo step through the first window of GUI sessions, a timer pass holding no screen."""
        applied = self.applied() if self.scope.is_set() else Ok(Nothing)
        windows = () if bpy.app.background else tuple(window for manager in root_in(bpy.types.WindowManager, bpy.context).to_list() for window in manager.windows)[:1]
        for message, window in product(applied.default_value(Nothing).to_list(), windows):
            with bpy.context.temp_override(window=window):
                bpy.ops.ed.undo_push(message=message)
        return applied.map(lambda _: Nothing)


# --- [SERVICES] -------------------------------------------------------------------------


class Reports(AbstractContextManager["Reports"]):
    """Extension log file, callback fault log, and queued requests, entered first in registration scope.

    The log file is `logs/<extension id>.log` under the extension's user folder, JSON records every Blender session of the extension appends and rotates under one lock.
    The logger binds the stdlib logger named for the extension package and sets no process-wide logging state, so outside the scope its warnings and errors print through stdlib `logging` alone.
    The file opens under default settings and reopens under each `configured` call once preferences are readable.
    Spans come from a tracer provider of the extension alone, so each carries the ids its records and the processes it starts continue, and they export only to the collector a preference names.
    A background process running one command of the extension records as that command's own program through `commanded`, into `logs/<command>.log`.
    Host facts alone sit here: the extension folder, the preferences a `configured` call reads, `bpy.app.online_access`, and the save and render events that flush spans.
    Log records of a session are not exported: the SDK holds its logger provider and OTLP log exporter in private modules alone (`opentelemetry.sdk._logs`, `opentelemetry.exporter.otlp.proto.http._log_exporter`).
    """

    # --- [STATE]

    __slots__ = (
        "_collector",
        "_export",
        "_extension",
        "_flushed",
        "_folder",
        "_loaded",
        "_lock",
        "_logged",
        "_package",
        "_pending",
        "_scope",
        "_service",
        "_telemetry",
        "_version",
        "logger",
        "settings",
        "ticked",
        "tracer",
    )

    def __init__(self, package: str, folder: Path, /, *, extension: str, version: str) -> None:
        """Initializes the empty queue, with the logger writing to the extension's user folder while the scope is open."""
        self._package, self._folder, self._extension, self._version, self._service = package, folder / "logs", extension, version, extension
        self._scope, self._telemetry = ExitStack(), ExitStack()
        self.logger, self.settings = logger(package), LogSettings()
        self.tracer = trace.get_tracer(package)
        self._export: Option[TracerProvider] = Nothing
        self._collector: Option[Collector] = Nothing
        self._logged: set[str] = set()
        self._pending: list[Pending] = []
        self._lock = RLock()

        def ticked() -> float | None:
            """Advances each waiting request one step outside a locked interface, the timer pass returning 0.0 while requests remain."""
            if any(manager.is_interface_locked for manager in root_in(bpy.types.WindowManager, bpy.context).to_list()):
                return 0.0
            with self._lock:
                requests, self._pending[:] = tuple(self._pending), []
                self._pending[:0] = [later for request in requests for later in request.advanced().map_error(self.logged).default_value(Nothing).to_list()]
                return 0.0 if self._pending else None

        def loaded(*_: object) -> None:
            """Drops requests and logged faults of the file a load replaced, the host having removed the queue timer."""
            with self._lock:
                self._pending.clear()
                self._logged.clear()

        def flushed(*_: object) -> None:
            """Sends ended spans while the user's export preference holds a provider and the host allows online access."""
            for held in self._export.filter(lambda _: bpy.app.online_access).to_list():
                held.force_flush()

        bpy.app.handlers.persistent(loaded)
        bpy.app.handlers.persistent(flushed)
        self.ticked, self._loaded, self._flushed = ticked, loaded, flushed

    # --- [LIFECYCLE]

    @override
    def __enter__(self) -> Self:
        """Opens the log file, clears queued requests and logged faults after every file load, and flushes export after every save and render, until scope exit, a log folder refusing creation raising `OSError`."""
        self._opened(self.settings, Nothing)
        for handlers, body in (
            (bpy.app.handlers.load_post, self._loaded),
            (bpy.app.handlers.save_post, self._flushed),
            (bpy.app.handlers.render_complete, self._flushed),
            (bpy.app.handlers.render_cancel, self._flushed),
        ):
            handlers.append(body)
            self._scope.callback(handlers.remove, body)
        return self

    @override
    def __exit__(self, *_: object) -> None:
        """Drops queued requests, unregisters the queue timer and handlers, flushes export, and closes the log file."""
        with self._lock:
            self._pending.clear()
            if bpy.app.timers.is_registered(self.ticked):
                bpy.app.timers.unregister(self.ticked)
        self._scope.close()
        self._telemetry.close()

    def _opened(self, settings: LogSettings, collector: Option[Collector], /) -> None:
        """Closes the held log file and export, then opens the log file under the settings, their level on the extension's own logger, and span export to a collector while the host allows online access.

        Exceptions reaching a Python exception hook log where a frame of their traceback belongs to the extension package, the host's and every other extension's left out.
        Spans come from a provider of the extension alone and export on the thread that flushes, the host sharing its process-wide provider with every extension.
        """
        self._telemetry.close()
        self.logger.setLevel(settings.severity)
        self.settings, self._collector = settings, collector.filter(lambda _: bpy.app.online_access)
        handler = self._telemetry.enter_context(closing(self.handler(self.path)))
        self.logger.addHandler(handler)
        self._telemetry.callback(self.logger.removeHandler, handler)
        self._telemetry.enter_context(unhandled(self.logger, self._package))
        held = provider(self._extension, self._version, *(ManualSpanProcessor(target.exporter()) for target in self._collector.to_list()))
        self.tracer, self._export = held.get_tracer(self._package), Some(held)
        self._telemetry.callback(held.shutdown)
        self._telemetry.callback(self._flushed)

    def configured(self, settings: LogSettings, collector: Option[Collector], /) -> Outcome[None]:
        """Reopens the log file under the settings a user's preferences hold, exporting spans to a collector where the user turned export on.

        Returns:
            Ok, `UNWRITABLE` fault for a log folder refusing creation, the log file closed.
        """
        return catch(self._opened, exception=OSError)(settings, collector).map_error(functools.partial(Fault.of, Reason.UNWRITABLE, str(self.path)))

    @contextmanager
    def commanded(self, command: str, environ: Mapping[str, str], /) -> Generator[None]:
        """Holds a background process running one command of the extension instrumented as the command's own program until the command ends.

        The process is the command's alone, so the process part of `rasm_telemetry` takes it whole in place of the extension log.
        The root logger writes every record to `logs/<command>.log` under the command's name, spans continue the trace context the launcher wrote into the environment, and unhandled exceptions and fatal errors of any code record there.
        Signals export as the `OTEL_*` variables of `environ` name while the host allows online access, spans also to the collector a preference names.

        Yields:
            Nothing, while the process records as the command.
        """
        self._telemetry.close()
        self._service = command
        with instrumented(
            self.path,
            service=command,
            version=self._version,
            settings=self.settings,
            scopes=(self._package,),
            environ=environ if bpy.app.online_access else {**environ, OTEL_SDK_DISABLED: "true"},
            span_processors=tuple(BatchSpanProcessor(target.exporter()) for target in self._collector.to_list()),
        ):
            self.tracer, self._export = trace.get_tracer(self._package), Nothing
            yield

    @property
    def path(self) -> Path:
        """Log file of the program this process records as: the extension, or the command a background process runs."""
        return self.log(self._service)

    def log(self, program: str, /) -> Path:
        """Returns the log file of a program of the extension: the extension ID, or the identifier of one of its commands."""
        return self._folder / f"{program}.log"

    def handler(self, path: Path, /) -> logging.Handler:
        """Returns the handler writing records to a path under the held settings and the name of the program this process records as, the log file's and each run log's."""
        return self.settings.handler(path, service=self._service, version=self._version)

    # --- [TRACING]

    def spanned[T](self, name: str, operation: Callable[[], Outcome[T]], /) -> Outcome[T]:
        """Runs an operation inside one span of a constant name, failed by the operation's first fault with the fault's reason as `error.type`."""
        with self.tracer.start_as_current_span(name) as span:
            answer = operation()
            for fault in answer.swap().default_value(())[:1]:
                span.set_status(Status(StatusCode.ERROR, fault.message))
                span.set_attribute(ERROR_TYPE, fault.reason.name)
            return answer

    # --- [LOGGING]

    def logged(self, faults: tuple[Fault, ...], /) -> None:
        """Logs each distinct fault once per loaded file, a draw body reading the same fault at every redraw, with the exit code and last output lines of a failed process and the exception of any other caught cause."""
        with self._lock:
            fresh = {fault.message: fault for fault in faults if fault.message not in self._logged}
            self._logged.update(fresh)
        for text, fault in fresh.items():
            cause = fault.cause
            detail = (
                {PROCESS_EXIT_CODE: cause.returncode, "output": cause.output} if isinstance(cause, subprocess.CalledProcessError) else {"exc_info": cause} if isinstance(cause, BaseException) else {}
            )
            self.logger.error("Callback failed", report=_level(fault), message=text, **detail)

    def settled[T](self, result: Outcome[T], /) -> Option[T]:
        """Returns Some(value) on success, or Nothing after logging the faults."""
        return result.map_error(self.logged).to_option()

    def faulted[T](self, result: Outcome[T], /) -> None:
        """Logs the faults of a failed result whose value no later read takes."""
        for faults in result.swap().to_option().to_list():
            self.logged(faults)

    def reported[**P, T](self, callback: Callable[P, Outcome[T]], /) -> Callable[P, Option[T]]:
        """Wraps callback to return Some(value) on success, or Nothing after logging its faults."""

        @functools.wraps(callback)
        def translated(*args: P.args, **kwargs: P.kwargs) -> Option[T]:
            """Logs faults, returning Option."""
            return self.settled(callback(*args, **kwargs))

        return translated

    # --- [QUEUE]

    def queued(self, request: Pending, /) -> Outcome[None]:
        """Applies the request's first step, returning its faults, and leaves the rest to later passes."""
        return request.advanced().map(lambda later: later.map(self.waiting).default_value(None))

    def waiting(self, request: Pending, /) -> None:
        """Queues request for the next unlocked pass unless an equal one waits, applying it whole in background mode, where no timer runs."""
        if bpy.app.background:
            pending: Option[Pending] = Some(request)
            while (current := pending.to_optional()) is not None:
                pending = current.advanced().map_error(self.logged).default_value(Nothing)
        else:
            with self._lock:
                if request not in self._pending:
                    self._pending.append(request)
                if not bpy.app.timers.is_registered(self.ticked):
                    bpy.app.timers.register(self.ticked)

    @contextmanager
    def deferred[**P](self, write: Callable[P, Outcome[Option[str]]], /) -> Generator[Callable[P, None]]:
        """Scope yielding a callable queueing each distinct argument set of a write returning its undo step name, calls applying nothing after exit."""
        scope = Event()
        scope.set()

        def called(*args: P.args, **kwargs: P.kwargs) -> None:
            """Queues the write while the scope is open."""
            if scope.is_set():
                self.waiting(_Write(applied=functools.partial(write, *args, **kwargs), scope=scope, call=(write, args, MappingProxyType(kwargs))))

        with ExitStack() as stack:
            stack.callback(scope.clear)
            yield called


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ENTRIES]


def _level(fault: Fault, /) -> "rna_enums.WmReportItems":
    """Returns the report type a fault raises: `ERROR_INVALID_INPUT` for refused input, `ERROR_INVALID_CONTEXT` for unmet requirements, else `ERROR`."""
    match fault.reason:
        case Reason.OUTSIDE | Reason.INVALID | Reason.ABSENT | Refusal.TAKEN:
            return "ERROR_INVALID_INPUT"
        case Refusal.REQUIRED:
            return "ERROR_INVALID_CONTEXT"
        case _:
            return "ERROR"


def rendered(faults: tuple[Fault, ...], /) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
    """Returns the report type and message of each fault in input order, each reason template translated before its fields fill it."""
    return tuple((_level(fault), fault.filled(bpy.app.translations.pgettext_rpt(fault.reason))) for fault in faults)


# --- [REDRAW]


def redrawn() -> Outcome[Option[str]]:
    """Tags every area of every window for redraw, pushing no undo step."""
    for area in (area for manager in root_in(bpy.types.WindowManager, bpy.context).to_list() for window in manager.windows for area in window.screen.areas):
        area.tag_redraw()
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pending", "Refusal", "Reports", "redrawn", "rendered"]
