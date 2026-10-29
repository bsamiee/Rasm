"""OpenImageIO buffers: stored channel depths, integer codings, checked reads, fits, and alpha association.

`unassociated` and `associated` round trip keeping color at zero alpha, and `premultiplied` associates straight color.
"""

from collections.abc import Callable
from enum import auto, Enum, IntEnum
import functools
from typing import Final, Literal, overload

from expression import Ok, Option
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.records.faults import Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Coded = np.uint8 | np.uint16 | np.float32
type IntegerDepth = Literal[Depth.UINT8, Depth.UINT16]


class Coding(Enum):
    """Integer coding of straight display values: nearest codes, or Floyd-Steinberg diffusion of separate color errors in raster order with alpha rounded."""

    ROUNDED = auto()
    DIFFUSED = auto()

    def coded(self, plane: npt.NDArray[np.float32], depth: IntegerDepth, /) -> Outcome[npt.NDArray[np.uint8 | np.uint16]]:
        """Returns clamped integer codes of the same shape, NaN and negative infinity at zero, ties to even, each diffused color error spread over unvisited neighbors along anti-diagonal wavefronts, or image fault."""
        rounded = pixels(OpenImageIO.ImageBuf(np.ascontiguousarray(plane).reshape(1, -1, 1)), depth).map(lambda codes: codes.reshape(plane.shape))
        if self is Coding.ROUNDED:
            return rounded
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
                rounded_step = np.floor(visited + 0.5)
                coded[rows, columns, :3], error = rounded_step, visited - rounded_step
                ring[low : high + 1, step % 4] = 0.0
                ring[low + 1 : ahead + 1, (step + 1) % 4] += error[: ahead - low] * (3 / 16)
                ring[low + 1 : high + 2, (step + 2) % 4] += error * (5 / 16)
                ring[low + 1 : high + 2, (step + 3) % 4] += error * (1 / 16)
                ring[low : high + 1, (step + 1) % 4] += error * (7 / 16)
            return coded

        return rounded.map(diffused)


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

    def coded(self, plane: npt.NDArray[np.float32], coding: Coding = Coding.ROUNDED) -> Outcome[npt.NDArray[Coded]]:
        """Returns straight display values stored at the depth, integer depths coded through `coding`, or image fault."""
        match self:
            case Depth.UINT8 | Depth.UINT16 as integer:
                return coding.coded(plane, integer)
            case Depth.HALF | Depth.FLOAT:
                return Ok(plane)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CHECKS]


def checked(image: OpenImageIO.ImageBuf) -> Outcome[OpenImageIO.ImageBuf]:
    """Returns buffer, or image fault when buffer holds an error."""
    return Ok(image).filter_with(lambda held: not held.has_error, lambda held: Fault.of(Reason.FAILED, held.name, held.geterror()))


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
def pixels(image: OpenImageIO.ImageBuf, depth: IntegerDepth) -> Outcome[npt.NDArray[np.uint8 | np.uint16]]: ...
def pixels(image: OpenImageIO.ImageBuf, depth: Depth = Depth.FLOAT) -> Outcome[npt.NDArray[np.generic]]:
    """Returns pixels of buffer stored at `depth`, float32 by default, or image fault."""
    return Option.of_optional(image.get_pixels(depth.basetype)).to_result_with(lambda: Fault.of(Reason.FAILED, image.name, image.geterror()))


# --- [FITS]


def fitted(values: npt.NDArray[np.float32], size: tuple[int, int], /, *, filtername: str = "", exact: bool = False) -> Outcome[npt.NDArray[np.float32]]:
    """Returns linear-light values shrunk through `filtername` to fit inside `size` width and height at their aspect, unchanged within it, or with `exact` resized to `size` at their fractional fit and letterboxed."""
    width, height = size
    return (
        checked(OpenImageIO.ImageBufAlgo.fit(OpenImageIO.ImageBuf(values), filtername=filtername, exact=exact, roi=OpenImageIO.ROI(0, width, 0, height))).bind(pixels)
        if values.shape[1] > width or values.shape[0] > height or (exact and values.shape[:2] != (height, width))
        else Ok(values)
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

__all__ = ["Coded", "Coding", "Depth", "IntegerDepth", "applied", "associated", "checked", "fitted", "located", "pixels", "premultiplied", "unassociated"]
