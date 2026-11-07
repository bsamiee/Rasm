"""Callback fault logging, report entries of operator faults, and the main-thread queue of requests the host applies while its interface is unlocked."""

from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
import functools
from itertools import product
import logging
from pathlib import Path
from threading import Event, RLock
from types import MappingProxyType
from typing import Final, override, Protocol, Self, TYPE_CHECKING

import bpy
from expression import Option, Some
import msgspec
import structlog
from structlog.processors import add_log_level, ExceptionRenderer, TimeStamper
from structlog.tracebacks import ExceptionDictTransformer
from structlog.typing import FilteringBoundLogger

from blender.records.faults import Fault, Nothing, Ok, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Pending(Protocol):
    """Request the host applies over later passes."""

    def advanced(self) -> Outcome[Option[Self]]:
        """Applies one step, returning the request left to apply, Nothing once done."""


# --- [CONSTANTS] ------------------------------------------------------------------------

_LOG: Final = "extension.log"
_RECORD: Final = msgspec.json.Encoder(enc_hook=repr)

# --- [MODELS] ---------------------------------------------------------------------------


class _Write(msgspec.Struct, frozen=True, kw_only=True):  # type: ignore[explicit-any]
    """One deferred write call, equal to another call of the same write and scope with equal arguments, applying nothing once its scope closed."""

    applied: Callable[..., Outcome[Option[str]]]  # type: ignore[explicit-any]
    scope: Event
    args: tuple[object, ...]
    kwargs: Mapping[str, object]

    def advanced(self) -> Outcome[Option[Self]]:
        """Runs the scoped write, pushing its undo step through the first window of GUI sessions, a timer pass holding no screen."""
        applied = self.applied(*self.args, **self.kwargs) if self.scope.is_set() else Ok(Nothing)
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        for message, window in product(applied.default_value(Nothing).to_list(), () if bpy.app.background else tuple(manager.windows)[:1]):
            with bpy.context.temp_override(window=window):
                bpy.ops.ed.undo_push(message=message)
        return applied.map(lambda _: Nothing)


# --- [SERVICES] -------------------------------------------------------------------------


class Reports(AbstractContextManager["Reports"]):
    """Extension callback fault log and queued requests, entered first in registration scope."""

    # --- [STATE]

    __slots__ = ("_extension", "_folder", "_idle", "_loaded", "_lock", "_logged", "_pending", "_scope", "logger", "ticked")

    def __init__(self, package: str, folder: Path, /, *, extension: str) -> None:
        """Initializes the empty queue, with the logger writing to the extension's user folder while the scope is open."""
        self._folder, self._extension = folder, extension
        self._scope = ExitStack()
        self._idle: FilteringBoundLogger = structlog.get_logger(package, extension=extension)
        self.logger = self._idle
        self._logged: set[str] = set()
        self._pending: list[Pending] = []
        self._lock = RLock()

        def ticked() -> float | None:
            """Advances each waiting request one step outside a locked interface, the timer pass returning 0.0 while requests remain."""
            manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            if manager.is_interface_locked:
                return 0.0
            with self._lock:
                requests, self._pending[:] = tuple(self._pending), []
                self._pending[:0] = [later for request in requests for later in request.advanced().map_error(self.logged).default_value(Nothing).to_list()]
                return 0.0 if self._pending else None

        @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
        def loaded(*_: object) -> None:
            """Drops requests and logged faults of the file a load replaced, the host having removed the queue timer."""
            with self._lock:
                self._pending.clear()
                self._logged.clear()

        self.ticked, self._loaded = ticked, loaded

    # --- [LIFECYCLE]

    @override
    def __enter__(self) -> Self:
        """Appends JSON lines to the extension's log through its own chain, sibling extensions sharing structlog's process configuration, and clears queued requests and logged faults after every file load until scope exit."""
        self.logger = structlog.wrap_logger(
            structlog.BytesLogger(self._scope.enter_context((self._folder / _LOG).open("ab"))),
            processors=[
                add_log_level,
                TimeStamper(fmt="iso", utc=True),
                ExceptionRenderer(ExceptionDictTransformer(show_locals=False)),
                lambda _logger, _method, event_dict: _RECORD.encode(event_dict),
            ],
            wrapper_class=structlog.make_filtering_bound_logger(logging.INFO),
            extension=self._extension,
        )
        loads: list[Callable[..., None]] = bpy.app.handlers.load_post  # type: ignore[explicit-any]
        loads.append(self._loaded)
        self._scope.callback(loads.remove, self._loaded)
        return self

    @override
    def __exit__(self, *_: object) -> None:
        """Drops queued requests, unregisters the queue timer and load handler, and closes the log file."""
        with self._lock:
            self._pending.clear()
            if bpy.app.timers.is_registered(self.ticked):
                bpy.app.timers.unregister(self.ticked)
        self.logger = self._idle
        self._scope.close()

    # --- [LOGGING]

    def logged(self, faults: tuple[Fault, ...], /) -> None:
        """Logs each distinct fault once per loaded file, a draw body reading the same fault at every redraw."""
        with self._lock:
            fresh = {fault.message: fault for fault in faults if fault.message not in self._logged}
            self._logged.update(fresh)
        for text, fault in fresh.items():
            self.logger.error("callback_failed", report=level(fault), message=text)

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
                self.waiting(_Write(applied=write, scope=scope, args=args, kwargs=MappingProxyType(kwargs)))

        with ExitStack() as stack:
            stack.callback(scope.clear)
            yield called


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ENTRIES]


def level(fault: Fault, /) -> "rna_enums.WmReportItems":
    """Returns the report type a fault raises: `ERROR_INVALID_INPUT` for refused input, `ERROR_INVALID_CONTEXT` for unmet requirements, else `ERROR`."""
    match fault.reason:
        case Reason.OUTSIDE | Reason.INVALID | Reason.ABSENT | Reason.TAKEN:
            return "ERROR_INVALID_INPUT"
        case Reason.REQUIRED:
            return "ERROR_INVALID_CONTEXT"
        case _:
            return "ERROR"


def rendered(faults: tuple[Fault, ...], /) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
    """Returns the report type and message of each fault in input order, each reason template translated before its fields fill it."""
    return tuple((level(fault), fault.filled(bpy.app.translations.pgettext_rpt(fault.reason))) for fault in faults)


# --- [REDRAW]


def redrawn() -> Outcome[Option[str]]:
    """Tags every area of every window for redraw, pushing no undo step."""
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    for area in (area for window in manager.windows for area in window.screen.areas):
        area.tag_redraw()
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pending", "Reports", "level", "redrawn", "rendered"]
