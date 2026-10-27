"""Callback fault reports and the main-thread queue of requests the host applies while its interface is unlocked."""

from collections.abc import Callable, Mapping
from contextlib import AbstractContextManager, closing, ExitStack
import functools
from threading import RLock
from types import MappingProxyType
from typing import Never, override, Protocol, Self, TYPE_CHECKING

import bpy
from expression import identity, Nothing, Ok, Option
import msgspec
from structlog.typing import FilteringBoundLogger

from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Pending(Protocol):
    """Request the host applies over later passes."""

    def advanced(self) -> Outcome[Option[Self]]:
        """Applies one step, returning the request left to apply, Nothing once settled."""
        ...


type _Queued = _Deferral[...]

# --- [MODELS] ---------------------------------------------------------------------------


class _Write(msgspec.Struct, frozen=True, kw_only=True):
    """One deferred write call, equal to another call of the same deferral with equal arguments."""

    deferral: _Queued
    args: tuple[object, ...]
    kwargs: Mapping[str, object]

    def advanced(self) -> Outcome[Option[Self]]:
        """Runs the write, pushing the undo step it names."""

        def pushed(step: Option[str]) -> Option[Never]:
            for message in step.to_list():
                bpy.ops.ed.undo_push(message=message)
            return Nothing

        return self.deferral.applied(*self.args, **self.kwargs).map(pushed)


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
        """Closes the write to calls and drops its queued ones."""
        self.open = False
        self.reports.cancelled(self)


class Reports(AbstractContextManager["Reports"]):
    """Extension callback faults and queued requests, entered first in registration scope."""

    __slots__ = ("_entries", "_lock", "_pending", "_redraw", "_scopes", "_ticked", "logger")

    # --- [LIFECYCLE]

    def __init__(self, logger: FilteringBoundLogger) -> None:
        """Initializes empty reports and queue, opening the deferred redraw.

        Args:
            logger: Logger receiving each new entry with its report type.
        """
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

        self._ticked = ticked
        self._scopes = ExitStack()
        self._redraw = self._scopes.enter_context(self.deferred(redrawn))

    @override
    def __exit__(self, *_: object) -> None:
        """Closes deferred redraw, drops queued requests, and unregisters the queue timer."""
        self._scopes.close()
        with self._lock:
            self._pending.clear()
            if bpy.app.timers.is_registered(self._ticked):
                bpy.app.timers.unregister(self._ticked)

    # --- [REPORTING]

    @property
    def entries(self) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
        """Report type and text per distinct held fault, in hold order."""
        return tuple(self._entries)

    def hold(self, faults: Faults) -> None:
        """Holds each distinct fault entry once, logging and redrawing new ones.

        Args:
            faults: Ordered faults of one callback.
        """
        with self._lock:
            fresh = tuple(entry for entry in dict.fromkeys(rendered(faults)) if entry not in self._entries)
            self._entries.update(dict.fromkeys(fresh))
        for kind, text in fresh:
            self.logger.error(text, report=kind)
        if fresh:
            self._redraw()

    def reported[**P, T](self, callback: Callable[P, Outcome[T]]) -> Callable[P, Option[T]]:
        """Wraps callback to return Option, holding its faults.

        Args:
            callback: Callback returning Outcome.

        Returns:
            Callback returning Some(value) on success or Nothing after holding faults.
        """

        @functools.wraps(callback)
        def translated(*args: P.args, **kwargs: P.kwargs) -> Option[T]:
            """Holds faults, returning Option."""
            return callback(*args, **kwargs).map_error(self.hold).to_option()

        return translated

    # --- [QUEUE]

    def queued(self, request: Pending) -> Outcome[None]:
        """Applies the request's first step now, leaving the rest to later passes.

        Args:
            request: Request the host applies one step per pass.

        Returns:
            Ok(None), the first step's faults.
        """
        return request.advanced().map(lambda later: later.map(self.waiting).default_value(None))

    def waiting(self, request: Pending) -> None:
        """Queues request for the next pass the interface leaves unlocked unless an equal one waits, applying it whole now in background mode, where no timer runs.

        Args:
            request: Request the queue timer advances.
        """
        if bpy.app.background:
            _ = self.reported(request.advanced)().bind(identity).map(self.waiting)
        else:
            with self._lock:
                if request not in self._pending:
                    self._pending.append(request)
                if not bpy.app.timers.is_registered(self._ticked):
                    bpy.app.timers.register(self._ticked)

    def deferred[**P](self, write: Callable[P, Outcome[Option[str]]]) -> AbstractContextManager[Callable[P, None]]:
        """Accepts write calls until scope exit, then drops queued ones.

        Args:
            write: Write returning optional undo step name.

        Returns:
            Scope yielding a callable queueing each distinct argument set once.
        """
        return closing(_Deferral(self, write))

    def cancelled(self, deferral: Callable[..., None]) -> None:
        """Drops deferral's queued calls, its scope staying open.

        Args:
            deferral: Callable one deferred scope yielded.
        """
        with self._lock:
            self._pending[:] = [request for request in self._pending if not (isinstance(request, _Write) and request.deferral is deferral)]


# --- [OPERATIONS] -----------------------------------------------------------------------


def level(fault: Fault) -> "rna_enums.WmReportItems":
    """Returns the report type a fault raises.

    Args:
        fault: Fault of any package reason.

    Returns:
        `ERROR_INVALID_INPUT` for values outside their domain, absences, and collisions, `ERROR_INVALID_CONTEXT` for unmet requirements, else `ERROR`.
    """
    match fault.reason:
        case RecordReason.OUTSIDE | BlenderReason.ABSENT | BlenderReason.TAKEN:
            return "ERROR_INVALID_INPUT"
        case BlenderReason.UNMET:
            return "ERROR_INVALID_CONTEXT"
        case _:
            return "ERROR"


def rendered(faults: Faults) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
    """Returns one report entry per fault in input order.

    Args:
        faults: Faults in report order.

    Returns:
        Report type and message per fault.
    """
    return tuple((level(fault), fault.message) for fault in faults)


def redrawn() -> Outcome[Option[str]]:
    """Tags every area of every window for redraw.

    Returns:
        Ok(Nothing), redraw pushing no undo step.
    """
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    for area in (area for window in manager.windows for area in window.screen.areas):
        area.tag_redraw()
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pending", "Reports", "level", "redrawn", "rendered"]
