"""Callback fault reports and deferred main-thread writes."""

from collections.abc import Callable, Generator
from contextlib import AbstractContextManager, contextmanager, ExitStack
import functools
from threading import RLock
from typing import override, TYPE_CHECKING

import bpy
from expression import identity, Nothing, Ok, Option
from structlog.typing import FilteringBoundLogger

from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [SERVICES] -------------------------------------------------------------------------


class Reports(AbstractContextManager["Reports"]):
    """Extension callback faults and pending writes, entered first in registration scope."""

    # --- [LIFECYCLE]

    def __init__(self, logger: FilteringBoundLogger) -> None:
        """Initializes empty reports and pending writes, opening the deferred redraw.

        Args:
            logger: Logger receiving each new entry with its report type.
        """
        self.logger = logger
        self.held: dict[tuple[rna_enums.WmReportItems, str], None] = {}
        self.pending: dict[tuple[object, object, tuple[object, ...], frozenset[tuple[str, object]]], Callable[[], float | None]] = {}
        self.lock = RLock()
        self.scopes = ExitStack()
        self.redraw = self.scopes.enter_context(self.deferred(redrawn))

    @override
    def __exit__(self, *_: object) -> None:
        """Closes deferred redraw and unregisters pending write timers."""
        self.scopes.close()
        with self.lock:
            for write in dict.fromkeys(original for original, *_ in self.pending):
                self.cancelled(write)

    # --- [REPORTING]

    @property
    def entries(self) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
        """Report type and text per distinct held fault, in hold order."""
        return tuple(self.held)

    def hold(self, faults: Faults) -> None:
        """Holds each distinct fault entry once, logging and redrawing new ones.

        Args:
            faults: Ordered faults of one callback.
        """
        with self.lock:
            fresh = tuple(entry for entry in dict.fromkeys(rendered(faults)) if entry not in self.held)
            self.held.update(dict.fromkeys(fresh))
        for kind, text in fresh:
            self.logger.error(text, report=kind)
        if fresh:
            self.redraw()

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

    # --- [DEFERRAL]

    def cancelled(self, write: object) -> None:
        """Cancels write's queued calls, scopes staying open.

        Args:
            write: Original write or callable one deferred scope yielded.
        """
        with self.lock:
            for key in tuple((original, called, args, kwargs) for original, called, args, kwargs in self.pending if original is write or called is write):
                ticked = self.pending.pop(key)
                if bpy.app.timers.is_registered(ticked):
                    bpy.app.timers.unregister(ticked)

    @contextmanager
    def deferred[**P](self, write: Callable[P, Outcome[Option[str]]]) -> Generator[Callable[P, None]]:
        """Accepts write calls until scope exit, then cancels queued ones.

        Args:
            write: Write returning optional undo step name, its arguments hashable handles or records.

        Yields:
            Callable running write at call in background mode, else coalescing each argument set into one main-thread timer.
        """
        active = True
        applied = self.reported(write)

        @functools.wraps(write)
        def immediate(*args: P.args, **kwargs: P.kwargs) -> None:
            """Runs write at call with no undo push."""
            if active:
                _ = applied(*args, **kwargs)

        @functools.wraps(write)
        def queued(*args: P.args, **kwargs: P.kwargs) -> None:
            """Schedules one write per argument set."""
            key = (write, queued, args, frozenset(kwargs.items()))

            def ticked() -> float | None:
                """Runs queued call while scope and timer own it, rescheduling under interface lock."""
                with self.lock:
                    if not active or self.pending.get(key) is not ticked:
                        return None
                    if bpy.context.window_manager.is_interface_locked:  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                        return 0.0
                    del self.pending[key]
                    for step in applied(*args, **kwargs).bind(identity).to_list():
                        bpy.ops.ed.undo_push(message=step)
                    return None

            with self.lock:
                if active and (key not in self.pending or not bpy.app.timers.is_registered(self.pending[key])):
                    self.pending[key] = ticked
                    bpy.app.timers.register(ticked)

        called = immediate if bpy.app.background else queued
        try:
            yield called
        finally:
            with self.lock:
                active = False
                self.cancelled(called)


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
    for area in (area for window in bpy.context.window_manager.windows for area in window.screen.areas):  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        area.tag_redraw()
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Reports", "level", "redrawn", "rendered"]
