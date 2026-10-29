"""Callback fault reports, their alert rows, and the main-thread queue of requests the host applies while its interface is unlocked."""

from collections.abc import Callable, Iterable, Mapping
from contextlib import AbstractContextManager, closing
import functools
from threading import RLock
from types import MappingProxyType
from typing import Final, override, Protocol, Self, TYPE_CHECKING

import bpy
from expression import identity, Nothing, Ok, Option
import msgspec
from structlog.typing import FilteringBoundLogger

from blender.records.faults import Fault, Faults, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Pending(Protocol):
    """Request the host applies over later passes."""

    def advanced(self) -> Outcome[Option[Self]]:
        """Applies one step, returning the request left to apply, Nothing once settled."""


type _Queued = _Deferral[...]

# --- [CONSTANTS] ------------------------------------------------------------------------

_REPORTED: Final[Mapping[str, "rna_enums.IconItems"]] = MappingProxyType({
    "ERROR": "STATUS_ERROR_FILLED",
    "WARNING": "STATUS_WARNING_FILLED",
    "INFO": "STATUS_INFO_FILLED",
    "DEBUG": "SYSTEM",
    "PROPERTY": "OPTIONS",
    "OPERATOR": "CHECKMARK",
})

# --- [MODELS] ---------------------------------------------------------------------------


class _Write(msgspec.Struct, frozen=True, kw_only=True):
    """One deferred write call, equal to another call of the same deferral with equal arguments, applying nothing once its scope closed."""

    deferral: _Queued
    args: tuple[object, ...]
    kwargs: Mapping[str, object]

    def advanced(self) -> Outcome[Option[Self]]:
        """Runs the write while its scope stands, pushing the undo step it names."""
        applied = self.deferral.applied(*self.args, **self.kwargs) if self.deferral.open else Ok(Nothing)
        for message in applied.default_value(Nothing).to_list():
            bpy.ops.ed.undo_push(message=message)
        return applied.map(lambda _: Nothing)


# --- [SERVICES] -------------------------------------------------------------------------


class _Deferral[**P]:
    """Write accepting calls while its scope stands."""

    __slots__ = ("applied", "open", "reports")

    def __init__(self, reports: "Reports", applied: Callable[P, Outcome[Option[str]]]) -> None:
        """Opens the write to calls."""
        self.reports, self.applied, self.open = reports, applied, True

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> None:
        """Queues the write unless its scope closed."""
        if self.open:
            self.reports.waiting(_Write(deferral=self, args=args, kwargs=MappingProxyType(kwargs)))

    def close(self) -> None:
        """Closes the write to calls, its queued calls applying nothing."""
        self.open = False


class Reports(AbstractContextManager["Reports"]):
    """Extension callback faults and queued requests, entered first in registration scope."""

    __slots__ = ("_entries", "_loaded", "_lock", "_pending", "_redraw", "_ticked", "logger")

    # --- [LIFECYCLE]

    def __init__(self, logger: FilteringBoundLogger) -> None:
        """Initializes empty reports and queue, opening the deferred redraw, the logger receiving each new entry with its report type."""
        self.logger = logger
        self._entries: dict[tuple[rna_enums.WmReportItems, str], None] = {}
        self._pending: list[Pending] = []
        self._lock = RLock()

        def ticked() -> float | None:
            manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            if manager.is_interface_locked:
                return 0.0
            with self._lock:
                requests, self._pending[:] = tuple(self._pending), []
                self._pending[:0] = [later for request in requests for later in self.reported(request.advanced)().bind(identity).to_list()]
                return 0.0 if self._pending else None

        @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
        def loaded(*_: object) -> None:
            """Drops requests and faults of the file a load replaced, the host having removed the queue timer."""
            with self._lock:
                self._pending.clear()
                self._entries.clear()

        self._ticked, self._loaded = ticked, loaded
        self._redraw = _Deferral(self, redrawn)

    @override
    def __enter__(self) -> Self:
        """Clears queued requests and held faults after every file load until scope exit."""
        loads: list[Callable[..., None]] = bpy.app.handlers.load_post
        loads.append(self._loaded)
        return self

    @override
    def __exit__(self, *_: object) -> None:
        """Closes deferred redraw, drops queued requests, and unregisters the queue timer and load handler."""
        loads: list[Callable[..., None]] = bpy.app.handlers.load_post
        loads.remove(self._loaded)
        self._redraw.close()
        with self._lock:
            self._pending.clear()
            if bpy.app.timers.is_registered(self._ticked):
                bpy.app.timers.unregister(self._ticked)

    # --- [REPORTING]

    @property
    def entries(self) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
        """Report type and text per distinct held fault, in hold order."""
        return tuple(self._entries)

    def hold(self, faults: Faults, /) -> None:
        """Holds each distinct entry of a callback's faults once, logging and redrawing new ones."""
        with self._lock:
            fresh = tuple(entry for entry in dict.fromkeys(rendered(faults)) if entry not in self._entries)
            self._entries.update(dict.fromkeys(fresh))
        for kind, text in fresh:
            self.logger.error(text, report=kind)
        if fresh:
            self._redraw()

    def cleared(self) -> bool:
        """Drops every held fault entry, redrawing and answering True once any was held."""
        with self._lock:
            held = bool(self._entries)
            self._entries.clear()
        if held:
            self._redraw()
        return held

    def reported[**P, T](self, callback: Callable[P, Outcome[T]], /) -> Callable[P, Option[T]]:
        """Wraps callback to answer Some(value) on success, or Nothing after holding its faults."""

        @functools.wraps(callback)
        def translated(*args: P.args, **kwargs: P.kwargs) -> Option[T]:
            """Holds faults, returning Option."""
            return callback(*args, **kwargs).map_error(self.hold).to_option()

        return translated

    # --- [QUEUE]

    def queued(self, request: Pending, /) -> Outcome[None]:
        """Applies the request's first step now, answering its faults, and leaves the rest to later passes."""
        return request.advanced().map(lambda later: later.map(self.waiting).default_value(None))

    def waiting(self, request: Pending, /) -> None:
        """Queues request for the next pass the interface leaves unlocked unless an equal one waits, applying it whole now in background mode, where no timer runs."""
        if bpy.app.background:
            pending: Option[Pending] = Option.of_optional(request)
            while (current := pending.to_optional()) is not None:
                pending = self.reported(current.advanced)().bind(identity)
        else:
            with self._lock:
                if request not in self._pending:
                    self._pending.append(request)
                if not bpy.app.timers.is_registered(self._ticked):
                    bpy.app.timers.register(self._ticked)

    def deferred[**P](self, write: Callable[P, Outcome[Option[str]]], /) -> AbstractContextManager[Callable[P, None]]:
        """Scope yielding a callable that queues each distinct argument set of a write answering its optional undo step name once, queued calls applying nothing after scope exit."""
        return closing(_Deferral(self, write))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [ENTRIES]


def level(fault: Fault, /) -> "rna_enums.WmReportItems":
    """Returns the report type a fault raises: `ERROR_INVALID_INPUT` for values outside their domain, invalid values, absences, and collisions, `ERROR_INVALID_CONTEXT` for unmet requirements, else `ERROR`."""
    match fault.reason:
        case Reason.OUTSIDE | Reason.INVALID | Reason.ABSENT | Reason.TAKEN:
            return "ERROR_INVALID_INPUT"
        case Reason.REQUIRED:
            return "ERROR_INVALID_CONTEXT"
        case _:
            return "ERROR"


def rendered(faults: Faults, /) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
    """Returns the report type and message of each fault in input order, each reason template translated before its fields fill it."""
    return tuple((level(fault), bpy.app.translations.pgettext_rpt(fault.reason).format(subject=fault.subject, member=fault.member, cause=fault.cause)) for fault in faults)


# --- [ROWS]


def listed(layout: bpy.types.UILayout, entries: Iterable[tuple["rna_enums.WmReportItems", str]], /) -> None:
    """Draws report entries untranslated under the icon their severity takes in the host's report drawing, errors on alert rows."""
    for kind, text in entries:
        severity = "ERROR" if kind.startswith("ERROR") else kind
        line = layout.row()
        line.alert = severity == "ERROR"
        line.label(text=text, icon=_REPORTED[severity], translate=False)


def alerted[T](layout: bpy.types.UILayout, result: Outcome[T], /) -> None:
    """Draws the report entries of a failed result."""
    for faults in result.swap().to_option().to_list():
        listed(layout.column(align=True), rendered(faults))


# --- [REDRAW]


def redrawn() -> Outcome[Option[str]]:
    """Tags every area of every window for redraw, pushing no undo step."""
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    for area in (area for window in manager.windows for area in window.screen.areas):
        area.tag_redraw()
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pending", "Reports", "alerted", "level", "listed", "redrawn", "rendered"]
