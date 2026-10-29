"""OpenImageIO buffers: stored channel depths, checked reads, integer coding, fits, and alpha association.

`unassociated` and `associated` round trip keeping color at zero alpha, and `premultiplied` associates straight color.
"""

from collections.abc import Callable
from enum import IntEnum
import functools
from typing import Final, Literal, overload

from expression import Ok, Option
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.imaging.faults import ImagingReason
from blender.records.faults import Fault, Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type IntegerDepth = Literal[Depth.UINT8, Depth.UINT16]
type _Coding = Rounded | Diffused


class Depth(IntEnum):
    """Stored channel type, its value the OpenImageIO base type."""

    UINT8 = int(OpenImageIO.UINT8)
    UINT16 = int(OpenImageIO.UINT16)
    HALF = int(OpenImageIO.HALF)
    FLOAT = int(OpenImageIO.FLOAT)

    @property
    def basetype(self) -> OpenImageIO.BASETYPE:
        """OpenImageIO base type of the depth."""
        return OpenImageIO.BASETYPE(self)

    @property
    def bits(self) -> int:
        """Bits each stored channel holds."""
        return OpenImageIO.TypeDesc(self.basetype).size() * 8

    def coded(self, plane: npt.NDArray[np.float32], coding: _Coding) -> Outcome[npt.NDArray[np.uint8 | np.uint16 | np.float32]]:
        """Returns straight display values stored at depth, integer depths coded through `coding`, or image fault."""
        match self:
            case Depth.UINT8 | Depth.UINT16 as integer:
                return coding.coded(plane, integer)
            case Depth.HALF | Depth.FLOAT:
                return Ok(plane)


# --- [MODELS] ---------------------------------------------------------------------------


class Rounded(Case, frozen=True, kw_only=True):
    """Integer codes nearest each straight display value, ties to even."""

    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> Outcome[npt.NDArray[np.uint8 | np.uint16]]:
        """Returns clamped integer codes of the same shape, NaN and negative infinity at zero, or image fault."""
        return pixels(OpenImageIO.ImageBuf(np.ascontiguousarray(plane).reshape(1, -1, 1)), depth).map(lambda codes: codes.reshape(plane.shape))


class Diffused(Case, frozen=True, kw_only=True):
    """Floyd-Steinberg diffusion of separate color errors in raster order, alpha rounded."""

    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> Outcome[npt.NDArray[np.uint8 | np.uint16]]:
        """Returns codes of a straight BW, RGB, or RGBA frame, each color error spread over unvisited neighbors along anti-diagonal wavefronts, or image fault."""
        height, width, _ = plane.shape

        def diffused(rounded_codes: npt.NDArray[np.uint8 | np.uint16]) -> npt.NDArray[np.uint8 | np.uint16]:
            coded = rounded_codes.copy()
            levels = float(np.iinfo(coded.dtype).max)
            color = levels * plane[..., :3]
            ring = np.zeros((height + 1, 4, color.shape[2]), np.float32)
            for step in range(width + 2 * height - 2):
                low, high = max(0, (step - width + 2) // 2), min(height - 1, step // 2)
                rows, ahead = np.arange(low, high + 1), min(high, (step - 1) // 2) + 1
                columns = step - 2 * rows
                visited = np.clip(color[rows, columns] + ring[low : high + 1, step % 4], 0.0, levels)
                rounded = np.floor(visited + 0.5)
                coded[rows, columns, :3], error = rounded, visited - rounded
                ring[low : high + 1, step % 4] = 0.0
                ring[low + 1 : ahead + 1, (step + 1) % 4] += error[: ahead - low] * (3 / 16)
                ring[low + 1 : high + 2, (step + 2) % 4] += error * (5 / 16)
                ring[low + 1 : high + 2, (step + 3) % 4] += error * (1 / 16)
                ring[low : high + 1, (step + 1) % 4] += error * (7 / 16)
            return coded

        return Rounded.coded(plane, depth).map(diffused)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CHECKS]


def checked(image: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
    """Returns buffer, or image fault when buffer holds an error."""
    return Ok(image).filter_with(lambda held: not held.has_error, lambda held: Fault.of(ImagingReason.BUFFER, held.name, held.geterror()))


def located(values: npt.NDArray[np.float32 | np.uint8], like: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
    """Returns values as buffer with metadata, origin, and full window of `like`."""
    image = OpenImageIO.ImageBuf(values)
    image.copy_metadata(like)
    image.set_origin(like.xbegin, like.ybegin, like.zbegin)
    image.roi_full = like.roi_full
    return checked(image)


@overload
def pixels(image: OpenImageIO.ImageBuf) -> Outcome[npt.NDArray[np.float32]]: ...
@overload
def pixels(image: OpenImageIO.ImageBuf, depth: Literal[Depth.UINT8]) -> Outcome[npt.NDArray[np.uint8]]: ...
@overload
def pixels(image: OpenImageIO.ImageBuf, depth: Literal[Depth.UINT16]) -> Outcome[npt.NDArray[np.uint16]]: ...
@overload
def pixels(image: OpenImageIO.ImageBuf, depth: IntegerDepth) -> Outcome[npt.NDArray[np.uint8 | np.uint16]]: ...
def pixels(image: OpenImageIO.ImageBuf, depth: Depth = Depth.FLOAT) -> Outcome[npt.NDArray[np.generic]]:
    """Returns pixels of buffer stored at `depth`, float32 by default, or image fault."""
    return Option.of_optional(image.get_pixels(depth.basetype)).to_result_with(lambda: Fault.of(ImagingReason.BUFFER, image.name, image.geterror()))


# --- [FITS]


def resampled(values: npt.NDArray[np.float32], size: tuple[int, int]) -> Outcome[npt.NDArray[np.float32]]:
    """Returns values fitted exactly into `size` width and height, letterboxed at their aspect, for linear-light planes."""
    return checked(OpenImageIO.ImageBufAlgo.fit(OpenImageIO.ImageBuf(values), filtername="", filterwidth=0.0, fillmode="letterbox", exact=True, roi=OpenImageIO.ROI(0, size[0], 0, size[1]))).bind(
        pixels
    )


def fitted(values: npt.NDArray[np.float32], side: int, *, filtername: str = "") -> Outcome[npt.NDArray[np.float32]]:
    """Returns values shrunk through `filtername` so their longest side fits `side` at their aspect, unchanged within it."""
    return (
        checked(OpenImageIO.ImageBufAlgo.fit(OpenImageIO.ImageBuf(values), filtername=filtername, roi=OpenImageIO.ROI(0, side, 0, side))).bind(pixels) if max(values.shape[:2]) > side else Ok(values)
    )


# --- [ASSOCIATION]


def _alpha(operation: Callable[[OpenImageIO.ImageBuf], OpenImageIO.ImageBuf], values: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
    """Returns contiguous float32 copy of color rows or planes through an alpha operation, unchanged without alpha."""
    return np.asarray(operation(OpenImageIO.ImageBuf(np.ascontiguousarray(values).reshape(1, -1, values.shape[-1]))).get_pixels(OpenImageIO.FLOAT), dtype=np.float32).reshape(values.shape)


unassociated: Final = functools.partial(_alpha, OpenImageIO.ImageBufAlgo.unpremult)
associated: Final = functools.partial(_alpha, OpenImageIO.ImageBufAlgo.repremult)
premultiplied: Final = functools.partial(_alpha, OpenImageIO.ImageBufAlgo.premult)


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

__all__ = ["Depth", "Diffused", "IntegerDepth", "Rounded", "applied", "associated", "checked", "fitted", "located", "pixels", "premultiplied", "resampled", "unassociated"]
