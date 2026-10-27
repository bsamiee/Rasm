"""OpenImageIO buffer checks, fits, and alpha association."""

from collections.abc import Callable
import functools
from typing import Final

from expression import Ok, Option
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.faults import ImagingReason
from blender.records.faults import Fault, Outcome

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CHECKS]


def checked(image: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
    """Returns buffer, or image fault when buffer holds an error."""
    return Ok(image).filter_with(lambda held: not held.has_error, lambda held: (Fault(reason=ImagingReason.BUFFER, subject=held.name, cause=held.geterror()),))


def located(values: npt.NDArray[np.float32 | np.uint8], like: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
    """Returns values as buffer with metadata, origin, and full window of `like`."""
    image = OpenImageIO.ImageBuf(values)
    image.copy_metadata(like)
    image.set_origin(like.xbegin, like.ybegin, like.zbegin)
    image.roi_full = like.roi_full
    return checked(image)


def pixels(image: OpenImageIO.ImageBuf) -> Outcome[npt.NDArray[np.float32]]:
    """Returns float32 pixels of buffer, or image fault."""
    return Option.of_optional(image.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (Fault(reason=ImagingReason.BUFFER, subject=image.name, cause=image.geterror()),))


# --- [FITS]


def resampled(values: npt.NDArray[np.float32], size: tuple[int, int]) -> Outcome[npt.NDArray[np.float32]]:
    """Returns values fitted exactly into `size` width and height, letterboxed at their aspect, for linear-light planes."""
    return (
        checked(OpenImageIO.ImageBuf(values))
        .map(lambda source: OpenImageIO.ImageBufAlgo.fit(source, filtername="", filterwidth=0.0, fillmode="letterbox", exact=True, roi=OpenImageIO.ROI(0, size[0], 0, size[1])))
        .bind(checked)
        .bind(pixels)
    )


def fitted(values: npt.NDArray[np.float32], side: int) -> Outcome[npt.NDArray[np.float32]]:
    """Returns values shrunk so their longest side fits `side`, unchanged within it."""
    height, width, channels = values.shape
    longest = max(width, height)
    size = tuple(-(-extent * side // longest) for extent in (width, height))
    return checked(OpenImageIO.ImageBufAlgo.resize(OpenImageIO.ImageBuf(values), roi=OpenImageIO.ROI(0, size[0], 0, size[1], 0, 1, 0, channels))).bind(pixels) if longest > side else Ok(values)


# --- [ASSOCIATION]


def _alpha(operation: Callable[[OpenImageIO.ImageBuf], OpenImageIO.ImageBuf], values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
    """Returns contiguous float32 copy of color rows or planes through an alpha operation, color kept at zero alpha, unchanged without alpha."""
    return np.asarray(operation(OpenImageIO.ImageBuf(np.ascontiguousarray(values).reshape(1, -1, values.shape[-1]))).get_pixels(OpenImageIO.FLOAT), dtype=np.float32).reshape(values.shape)


unassociated: Final = functools.partial(_alpha, OpenImageIO.ImageBufAlgo.unpremult)
associated: Final = functools.partial(_alpha, OpenImageIO.ImageBufAlgo.repremult)


def applied(operation: PyOpenColorIO.CPUProcessor | Callable[[npt.NDArray[np.float32]], npt.NDArray[np.float32]], values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
    """Applies OCIO processor or straight-RGB operation to associated colors."""
    straight = unassociated(values)
    match operation:
        case PyOpenColorIO.CPUProcessor() if straight.shape[-1] == 4:
            operation.applyRGBA(straight)
        case PyOpenColorIO.CPUProcessor():
            operation.applyRGB(straight)
        case _:
            straight[..., :3] = operation(straight[..., :3])
    return associated(straight)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["applied", "associated", "checked", "fitted", "located", "pixels", "resampled", "unassociated"]
