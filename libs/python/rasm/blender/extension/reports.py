"""Held callback failures, their report entries, and deferred main-thread writes."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
import functools
import logging
from threading import RLock
from typing import Self, TYPE_CHECKING

import bpy
from expression import identity, Nothing, Ok, Option

from rasm.blender.faults import (
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
from rasm.imaging.faults import (
    ImagingBaseFault,
    ImagingBufferFault,
    ImagingCalibrationFault,
    ImagingCoincidentFault,
    ImagingComparisonFault,
    ImagingConfigFault,
    ImagingContrastFault,
    ImagingCurveFault,
    ImagingDiffractionFault,
    ImagingFault,
    ImagingGeneratorFault,
    ImagingJpegFault,
    ImagingLutFault,
    ImagingLutShapeFault,
    ImagingMatrixFault,
    ImagingMismatchFault,
    ImagingProcessorFault,
    ImagingReadFault,
    ImagingRepresentationFault,
    ImagingSpecksFault,
    ImagingSpectralFault,
    ImagingUnresolvedFault,
    ImagingWhiteFault,
    ImagingWriteFault,
)
from rasm.rendering.faults import (
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
    """One extension's callback failures and pending main-thread writes."""

    # --- [LIFECYCLE]

    def __init__(self, logger: logging.Logger) -> None:
        """Initializes empty reports and pending writes.

        Args:
            logger: Extension logger receiving every held report entry.
        """
        self.logger = logger
        self.held: dict[tuple[rna_enums.WmReportItems, str], None] = {}
        self.pending: dict[tuple[object, object, tuple[object, ...], frozenset[tuple[str, object]]], Callable[[], float | None]] = {}
        self.lock = RLock()
        self.scopes = ExitStack()

    def __enter__(self) -> Self:
        """Returns the reports held at the head of the registration scope."""
        self.redraw = self.scopes.enter_context(self.deferred(redrawn))
        return self

    def __exit__(self, *_: object) -> None:
        """Retires redraw admission and unregisters pending writes held by Blender."""
        self.scopes.close()
        with self.lock:
            for write in dict.fromkeys(original for original, *_ in self.pending):
                self.cancelled(write)

    # --- [REPORTING]

    @property
    def entries(self) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
        """Native report type and text of every distinct held leaf failure in first-hold order."""
        return tuple(self.held)

    def hold(self, faults: BlenderFaults) -> None:
        """Holds each distinct failure entry once, logging and redrawing for entries not yet held.

        Args:
            faults: Complete ordered failures from a callback.
        """
        with self.lock:
            fresh = tuple(entry for entry in dict.fromkeys(rendered(faults)) if entry not in self.held)
            self.held.update(dict.fromkeys(fresh))
        for _, text in fresh:
            self.logger.error(text)
        if fresh:
            self.redraw()

    def reported[**P, T](self, callback: Callable[P, BlenderResult[T]]) -> Callable[P, Option[T]]:
        """Returns a callback translating its result into an option and holding failures.

        Args:
            callback: Callback answering a Blender result.

        Returns:
            A callback answering Some(value) on success or Nothing after holding its failures.
        """

        @functools.wraps(callback)
        def translated(*args: P.args, **kwargs: P.kwargs) -> Option[T]:
            """Translates one callback result at the host boundary."""
            return callback(*args, **kwargs).map_error(self.hold).to_option()

        return translated

    # --- [DEFERRAL]

    def cancelled(self, write: object) -> None:
        """Cancels queued calls without retiring their live resource scopes.

        Args:
            write: Original write or callable yielded by one deferred scope.
        """
        with self.lock:
            for key in tuple((original, called, args, kwargs) for original, called, args, kwargs in self.pending if original is write or called is write):
                ticked = self.pending.pop(key)
                if bpy.app.timers.is_registered(ticked):
                    bpy.app.timers.unregister(ticked)

    @contextmanager
    def deferred[**P](self, write: Callable[P, BlenderResult[Option[str]]]) -> Generator[Callable[P, None]]:
        """Holds write admission until its resources retire, canceling queued calls on exit.

        Args:
            write: Write answering an optional undo-step name; arguments are hashable handles or records.

        Yields:
            A callable running each argument set at its call in background mode, where no event loop runs, and coalescing it into one main-thread timer otherwise.
        """
        active = True
        applied = self.reported(write)

        @functools.wraps(write)
        def immediate(*args: P.args, **kwargs: P.kwargs) -> None:
            """Runs the write at its call, dropping its step name since background runs hold no undo stack."""
            if active:
                _ = applied(*args, **kwargs)

        @functools.wraps(write)
        def queued(*args: P.args, **kwargs: P.kwargs) -> None:
            """Schedules one pending write for these arguments."""
            key = (write, queued, args, frozenset(kwargs.items()))

            def ticked() -> float | None:
                """Runs this queued call while its scope and native timer still own it, waiting out a locked interface."""
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
    """Returns native report entries for every leaf failure in input order.

    Args:
        faults: Complete Blender failures, including lower-package tuples.

    Returns:
        One native report type and rendered text per leaf failure.
    """

    def imaging(fault: ImagingFault) -> str:
        """Renders one imaging failure from its native payload."""
        match fault:
            case ImagingConfigFault(request=request, error=error):
                return f"Color configuration {request} failed: {error}" if isinstance(request, str) else f"Color configuration failed: {error}"
            case ImagingUnresolvedFault(error=names):
                return f"Color configuration resolves no colorspace for {', '.join(names)}"
            case ImagingProcessorFault(error=error):
                return f"Color processor failed: {error}"
            case ImagingCurveFault(request=curve, error=error):
                return f"{type(curve).__name__} curves failed validation: {error}"
            case ImagingCoincidentFault(curve=curve):
                return f"{type(curve).__name__} holds two knots of one curve at one input"
            case ImagingBaseFault(space=space, transform=transform):
                return f"Color space {space} has unsupported linear RGB geometry: {transform}"
            case ImagingSpectralFault(space=space, values=values):
                return f"Spectral matching for {space} cannot normalize RGB channels: {values}"
            case ImagingDiffractionFault(request=pupil, size=size, frame=frame, pixel_aspect=pixel_aspect, error=error):
                return f"Diffraction for {pupil} at size {size}, frame {frame}, and pixel aspect {pixel_aspect} failed ({type(error).__name__}): {error}"
            case ImagingSpecksFault(request=specks, size=size, error=error):
                return f"Lens dirt for {specks} at size {size} failed ({type(error).__name__}): {error}"
            case ImagingLutFault(request=request, error=error):
                return f"LUT {request} failed: {error}" if isinstance(request, str) else f"LUT failed: {error}"
            case ImagingReadFault(request=path, error=error):
                return f"{path} is unreadable: {error}"
            case ImagingWriteFault(request=path, error=error):
                return f"{path} is unwritable: {error}"
            case ImagingBufferFault(error=error):
                return f"Image buffer operation failed: {error}"
            case ImagingMismatchFault(reference=reference, test=test):
                return f"Images of shape {reference} and {test} differ"
            case ImagingComparisonFault(error=error):
                return f"Image comparison failed: {error}"
            case ImagingCalibrationFault(singular_values=values, tolerance=tolerance):
                return f"Calibration matrix has singular values {values}, outside tolerance {tolerance:.3g}"
            case ImagingMatrixFault(request=matrix, error=error):
                return f"Matrix operation for {matrix} failed: {error}"
            case ImagingContrastFault(request=request, black=black, white=white):
                return f"{request} has zero normalization contrast between endpoints {black} and {white}"
            case ImagingLutShapeFault(path=path, shape=shape):
                return f"LUT image {path} has incompatible shape {shape}"
            case ImagingWhiteFault(white=white):
                return white.map(lambda value: f"Measured scene white {value} lies outside the tone operator's exposure domain").default_value("White metering has no covered finite luminance samples")
            case ImagingJpegFault(request=request, error=error):
                return f"JPEG compression for {request} failed: {error}"
            case ImagingGeneratorFault(request=request, error=error):
                return f"Procedural field for {request} failed ({type(error).__name__}): {error}"
            case ImagingRepresentationFault(request=request, error=error):
                return f"Image representation for {request} failed ({type(error).__name__}): {error}"

    def rendering(fault: RenderingFault) -> tuple[str, ...]:
        """Renders rendering failures, preserving complete nested imaging tuples."""
        match fault:
            case RenderingNameFault(request=text, error=pattern):
                return (f"{text} does not match {pattern.pattern}",)
            case RenderingRootFault(request=path, error=root):
                return (f"{path} lies outside {root}",)
            case RenderingReadFault(request=path, error=error):
                return (f"{path} is unreadable: {error}",)
            case RenderingWriteFault(request=path, error=error):
                return (f"{path} is unwritable: {error}",)
            case RenderingSendFault(request=endpoint, error=error):
                return (
                    f"{endpoint} answered HTTP {error}"
                    if isinstance(error, int)
                    else f"{endpoint} failed: {error}"
                    if isinstance(error, OverflowError)
                    else f"{endpoint} failed: {error.name.lower().replace('_', ' ')}",
                )
            case RenderingDependencyFault(request=target, error=parts):
                return (f"{target.__name__} cannot enforce prerequisites for queue parts {parts}",)
            case RenderingRunFault(request=arguments, error=error):
                command = " ".join(arguments)
                return (f"{command} exited with code {error}" if isinstance(error, int) else f"{command} failed to start: {error}",)
            case RenderingWaitFault(request=interval, error=limit):
                return (f"Heartbeat interval {interval} seconds cannot fit a native timed wait below {limit} milliseconds",)
            case RenderingSystemFault(request=(call, argument), error=error):
                return (f"{call.name.lower().replace('_', ' ')} of {argument} failed: {error}",)
            case RenderingTimeFault(request=request, error=error):
                return (f"Calendar calculation for {request} failed: {error}",)
            case RenderingSizeFault(request=size, error=error):
                return (
                    error.map(lambda bounds: f"Pixel dimensions {size} lie outside full-scale overscan bounds {bounds}").default_value(f"Pixel dimensions {size} are empty")
                    if isinstance(error, Option)
                    else f"Dimensions {size} could not be materialized ({type(error).__name__}): {error}",
                )
            case RenderingArithmeticFault(request=operands, error=error):
                return (f"Numeric calculation for {operands} failed: {error}",)
            case RenderingImagingFault(faults=inner):
                return tuple(map(imaging, inner))

    def blender(fault: BlenderFault) -> tuple["rna_enums.WmReportItems", tuple[str, ...]]:
        """Renders native Blender cases with their host report severity."""
        match fault:
            case BlenderRegisterFault(request=names, error=error):
                return "ERROR", (f"{', '.join(names)} refused registration: {error}",)
            case BlenderEditFault(request=path, error=error):
                return "ERROR", (f"{path} refused the operation: {error}",)
            case BlenderChoiceFault(path=path, identifier=identifier, accepted=accepted):
                return "ERROR_INVALID_INPUT", (f"{identifier} is outside {path}, which accepts {', '.join(accepted)}",)
            case BlenderAbsentFault(owner=owner, key=key):
                return "ERROR_INVALID_INPUT", (f"{owner} holds no {key}",)
            case BlenderCollisionFault(request=name, error=path):
                return "ERROR_INVALID_INPUT", (f"{name} is taken, and the item stands at {path}",)
            case BlenderUnmetFault(requirement=requirement):
                return "ERROR_INVALID_CONTEXT", (requirement.message,)
            case BlenderOperatorFault(request=identifier, error=error):
                return "ERROR", (f"{identifier} answered {', '.join(sorted(error))}" if isinstance(error, frozenset) else f"{identifier} failed: {error}",)
            case BlenderReadFault(request=path, error=error):
                return "ERROR", (f"{path} is unreadable: {error}",)
            case BlenderWriteFault(request=path, error=error):
                return "ERROR", (f"{path} is unwritable: {error}",)
            case BlenderZoneFault(request=identifier, error=error):
                return "ERROR", (f"Time zone {identifier} is unavailable: {error}",)
            case BlenderImagingFault(faults=inner):
                return "ERROR", tuple(map(imaging, inner))
            case BlenderRenderingFault(faults=inner):
                return "ERROR", tuple(text for fault in inner for text in rendering(fault))

    return tuple((kind, text) for fault in faults for kind, texts in [blender(fault)] for text in texts)


def redrawn() -> BlenderResult[Option[str]]:
    """Tags every area of every window for redraw.

    Returns:
        Ok(Nothing), since redraw adds no undo step.
    """
    for area in (area for manager in Option.of_optional(bpy.context.window_manager).to_list() for window in manager.windows for area in window.screen.areas):
        area.tag_redraw()
    return Ok(Nothing)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Reports", "redrawn", "rendered"]
