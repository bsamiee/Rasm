"""Callback fault reports and deferred main-thread writes."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import functools
from threading import RLock
from typing import Self, TYPE_CHECKING

import bpy
from expression import identity, Nothing, Ok, Option
import msgspec
from structlog.typing import FilteringBoundLogger

from blender.bpy.faults import (
    BlenderAbsentFault,
    BlenderChoiceFault,
    BlenderCollisionFault,
    BlenderEditFault,
    BlenderFault,
    BlenderFaults,
    BlenderImagingFault,
    BlenderOperatorFault,
    BlenderReadFault,
    BlenderRegisterFault,
    BlenderRenderingFault,
    BlenderResult,
    BlenderUnmetFault,
    BlenderWriteFault,
    BlenderZoneFault,
)
from blender.imaging.faults import (
    ImagingBaseFault,
    ImagingBufferFault,
    ImagingCalibrationFault,
    ImagingCoincidentFault,
    ImagingColorFault,
    ImagingComparisonFault,
    ImagingContrastFault,
    ImagingFault,
    ImagingJpegFault,
    ImagingLutShapeFault,
    ImagingMismatchFault,
    ImagingNumericFault,
    ImagingReadFault,
    ImagingSpectralFault,
    ImagingUnresolvedFault,
    ImagingWhiteFault,
    ImagingWriteFault,
)
from blender.rendering.faults import (
    RenderingArithmeticFault,
    RenderingDependencyFault,
    RenderingFault,
    RenderingImagingFault,
    RenderingNameFault,
    RenderingReadFault,
    RenderingRootFault,
    RenderingRunFault,
    RenderingSendFault,
    RenderingSizeFault,
    RenderingSystemFault,
    RenderingTimeFault,
    RenderingWaitFault,
    RenderingWriteFault,
)

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [SERVICES] -------------------------------------------------------------------------


class Reports:
    """Extension callback faults and pending writes."""

    # --- [LIFECYCLE]

    def __init__(self, logger: FilteringBoundLogger) -> None:
        """Initializes empty reports and pending writes.

        Args:
            logger: Logger receiving each new entry with its report type.
        """
        self.logger = logger
        self.held: dict[tuple[rna_enums.WmReportItems, str], None] = {}
        self.pending: dict[tuple[object, object, tuple[object, ...], frozenset[tuple[str, object]]], Callable[[], float | None]] = {}
        self.lock = RLock()
        self.scopes = ExitStack()

    def __enter__(self) -> Self:
        """Opens deferred redraw, entered first in registration scope."""
        self.redraw = self.scopes.enter_context(self.deferred(redrawn))
        return self

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

    def hold(self, faults: BlenderFaults) -> None:
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

    def reported[**P, T](self, callback: Callable[P, BlenderResult[T]]) -> Callable[P, Option[T]]:
        """Wraps callback to return Option, holding its faults.

        Args:
            callback: Callback returning BlenderResult.

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
    def deferred[**P](self, write: Callable[P, BlenderResult[Option[str]]]) -> Generator[Callable[P, None]]:
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
                    if Option.of_optional(bpy.context.window_manager).map(lambda manager: manager.is_interface_locked).default_value(False):
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


def rendered(faults: BlenderFaults) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
    """Returns report entries for every leaf fault in input order.

    Args:
        faults: Blender faults, nested imaging and rendering tuples included.

    Returns:
        One report type and text per leaf fault.
    """

    def imaging(fault: ImagingFault) -> str:
        """Renders one imaging fault."""
        match fault:
            case ImagingColorFault(request=request, error=error):
                return f"Color operation on {request if isinstance(request, str) else type(request).__name__} failed. {error}"
            case ImagingUnresolvedFault(error=names):
                return f"Color configuration lacks color spaces {', '.join(names)}"
            case ImagingCoincidentFault(curve=curve):
                return f"{type(curve).__name__} holds two knots at one input"
            case ImagingBaseFault(space=space, transform=transform):
                return f"Color space {space} transform {transform} lacks linear RGB geometry"
            case ImagingSpectralFault(space=space, values=values):
                return f"{space} spectral matching cannot normalize RGB integrals {values}"
            case ImagingReadFault(request=path, error=error):
                return f"{path} is unreadable. {error}"
            case ImagingWriteFault(request=path, error=error):
                return f"{path} is unwritable. {error}"
            case ImagingBufferFault(error=error):
                return f"Image buffer operation failed. {error}"
            case ImagingMismatchFault(reference=reference, test=test):
                return f"Image shapes {reference} and {test} differ"
            case ImagingComparisonFault(error=error):
                return f"Image comparison failed. {error}"
            case ImagingCalibrationFault(singular_values=values, tolerance=tolerance):
                return f"Calibration matrix singular values {values} fail tolerance {tolerance:.3g}"
            case ImagingNumericFault(request=request, error=error):
                return f"Numeric operation for {request} failed ({type(error).__name__}). {error}"
            case ImagingContrastFault(request=request, black=black, white=white):
                return f"{request} has no contrast between black {black} and white {white}"
            case ImagingLutShapeFault(path=path, shape=shape):
                return f"LUT image {path} has incompatible shape {shape}"
            case ImagingWhiteFault(white=white):
                return white.map(lambda value: f"Scene white {value} lies outside tone operator exposure domain").default_value("White metering covers no finite luminance sample")
            case ImagingJpegFault(request=request, error=error):
                return f"JPEG compression for {request} failed. {error}"

    def rendering(fault: RenderingFault) -> tuple[str, ...]:
        """Renders rendering fault, one text per nested imaging fault."""
        match fault:
            case RenderingNameFault(request=text, error=pattern):
                return (f"{text} does not match {pattern.pattern}",)
            case RenderingRootFault(request=path, error=root):
                return (f"{path} lies outside {root}",)
            case RenderingReadFault(request=path, error=error):
                return (f"{path} is unreadable. {error}",)
            case RenderingWriteFault(request=path, error=error):
                return (f"{path} is unwritable. {error}",)
            case RenderingSendFault(request=endpoint, error=error):
                return (
                    f"{endpoint} answered HTTP {error}"
                    if isinstance(error, int)
                    else f"{endpoint} failed. {error}"
                    if isinstance(error, OverflowError)
                    else f"{endpoint} failed ({error.name.lower().replace('_', ' ')})",
                )
            case RenderingDependencyFault(request=target, error=parts):
                return (f"{target.__name__} cannot enforce prerequisites for queue parts {parts}",)
            case RenderingRunFault(request=arguments, error=error):
                command = " ".join(arguments)
                return (f"{command} exited with code {error}" if isinstance(error, int) else f"{command} failed to start. {error}",)
            case RenderingWaitFault(request=interval, error=limit):
                return (f"Heartbeat interval {interval} s reaches wait limit {limit} ms",)
            case RenderingSystemFault(request=(call, argument), error=error):
                return (f"{call.name.capitalize().replace('_', ' ')} of {argument} failed. {error}",)
            case RenderingTimeFault(request=request, error=error):
                return (f"Calendar calculation for {request} failed. {error}",)
            case RenderingSizeFault(request=size, error=bounds):
                return (bounds.map(lambda limit: f"Pixel dimensions {size} lie outside full-scale overscan bounds {limit}").default_value(f"Pixel dimensions {size} are empty"),)
            case RenderingArithmeticFault(request=operands, error=error):
                named = ", ".join(type(operand).__name__ if isinstance(operand, msgspec.Struct) else f"{operand:g}" for operand in (operands if isinstance(operands, tuple) else (operands,)))
                return (f"Numeric calculation for {named} failed ({type(error).__name__}). {error}",)
            case RenderingImagingFault(faults=inner):
                return tuple(map(imaging, inner))

    def blender(fault: BlenderFault) -> tuple["rna_enums.WmReportItems", tuple[str, ...]]:
        """Renders one Blender fault with its report type."""
        match fault:
            case BlenderRegisterFault(request=names, error=error):
                return "ERROR", (f"Registration of {', '.join(names)} refused. {error}",)
            case BlenderEditFault(request=path, error=error):
                return "ERROR", (f"{path} refused edit. {error}",)
            case BlenderChoiceFault(path=path, identifier=identifier, accepted=accepted):
                return "ERROR_INVALID_INPUT", (f"{path} accepts {', '.join(accepted)}, not {identifier}",)
            case BlenderAbsentFault(owner=owner, key=key):
                return "ERROR_INVALID_INPUT", (f"{owner} holds no {key}",)
            case BlenderCollisionFault(request=name, error=path):
                return "ERROR_INVALID_INPUT", (f"{name} is taken at {path}",)
            case BlenderUnmetFault(requirement=requirement):
                return "ERROR_INVALID_CONTEXT", (requirement.message,)
            case BlenderOperatorFault(request=identifier, error=error):
                return "ERROR", (f"{identifier} answered {', '.join(sorted(error))}" if isinstance(error, frozenset) else f"{identifier} failed. {error}",)
            case BlenderReadFault(request=path, error=error):
                return "ERROR", (f"{path} is unreadable. {error}",)
            case BlenderWriteFault(request=path, error=error):
                return "ERROR", (f"{path} is unwritable. {error}",)
            case BlenderZoneFault(request=identifier, error=error):
                return "ERROR", (f"Time zone {identifier} is unavailable. {error}",)
            case BlenderImagingFault(faults=inner):
                return "ERROR", tuple(map(imaging, inner))
            case BlenderRenderingFault(faults=inner):
                return "ERROR", tuple(text for fault in inner for text in rendering(fault))

    return tuple((kind, text) for fault in faults for kind, texts in (blender(fault),) for text in texts)


def redrawn() -> BlenderResult[Option[str]]:
    """Tags every area of every window for redraw.

    Returns:
        Ok(Nothing), redraw pushing no undo step.
    """
    for area in (area for manager in Option.of_optional(bpy.context.window_manager).to_list() for window in manager.windows for area in window.screen.areas):
        area.tag_redraw()
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Reports", "redrawn", "rendered"]
