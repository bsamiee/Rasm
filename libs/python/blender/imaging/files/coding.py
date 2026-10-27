"""Integer and codec coding of straight display values."""

from enum import IntEnum
import functools
import tempfile
from typing import IO, Literal

from expression import Ok
from expression.extra.result import catch
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.faults import ImagingReason
from blender.records.faults import Fault, Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type IntegerDepth = Literal[8, 16]
type Subsampling = Literal["4:4:4", "4:2:2", "4:2:0", "4:1:1"]
type _Coding = Rounded | Diffused


class Depth(IntEnum):
    """Stored channel type, its lowercased name an OpenImageIO type name."""

    UINT8 = 1
    UINT16 = 2
    HALF = 3
    FLOAT = 4

    def coded(self, plane: npt.NDArray[np.float32], coding: _Coding) -> npt.NDArray[np.uint8 | np.uint16 | np.float32]:
        """Returns straight display values stored at depth, integer depths coded through `coding`."""
        match self:
            case Depth.UINT8:
                return coding.coded(plane, 8)
            case Depth.UINT16:
                return coding.coded(plane, 16)
            case Depth.HALF | Depth.FLOAT:
                return plane


# --- [MODELS] ---------------------------------------------------------------------------


class Rounded(Case, frozen=True, kw_only=True):
    """Integer codes nearest each straight display value, ties to even."""

    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> npt.NDArray[np.uint8 | np.uint16]:
        """Returns clamped integer codes of the same shape, NaN and negative infinity at zero."""
        return np.asarray(OpenImageIO.ImageBuf(np.ascontiguousarray(plane).reshape(1, -1, 1)).get_pixels(OpenImageIO.UINT8 if depth == 8 else OpenImageIO.UINT16)).reshape(plane.shape)


class Diffused(Case, frozen=True, kw_only=True):
    """Floyd-Steinberg diffusion of separate color errors in raster order, alpha rounded."""

    @staticmethod
    def coded(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> npt.NDArray[np.uint8 | np.uint16]:
        """Returns codes of a straight BW, RGB, or RGBA frame, each color error spread over unvisited neighbors along anti-diagonal wavefronts."""
        height, width, _ = plane.shape
        coded = Rounded.coded(plane, depth)
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


# --- [OPERATIONS] -----------------------------------------------------------------------


def generations(rgb: npt.NDArray[np.uint8], *, quality: int, subsampling: Subsampling, count: int) -> Outcome[npt.NDArray[np.uint8]]:
    """Returns contiguous eight-bit RGB after `count` JPEG encode and decode cycles through one temporary file removed on return, at IJG `quality` and OpenImageIO `jpeg:subsampling` chroma."""

    @catch(exception=OSError)
    def scoped() -> IO[bytes]:
        return tempfile.NamedTemporaryFile(suffix=".jpg", delete_on_close=False)

    def cycles(file: IO[bytes]) -> Outcome[npt.NDArray[np.uint8]]:
        with file:
            return functools.reduce(lambda held, _: held.bind(functools.partial(cycled, file.name)), range(count), Ok(rgb))

    def cycled(path: str, held: npt.NDArray[np.uint8]) -> Outcome[npt.NDArray[np.uint8]]:
        encoded = OpenImageIO.ImageBuf(held)
        encoded.specmod().attribute("Compression", f"jpeg:{quality}")
        encoded.specmod().attribute("jpeg:subsampling", subsampling)
        decoded = OpenImageIO.ImageBuf(path) if encoded.write(path) else encoded
        return (
            Ok(decoded)
            .filter_with(lambda read: not read.has_error, lambda read: Fault.of(ImagingReason.CODEC, str(rgb.shape), read.geterror()))
            .map(lambda read: np.asarray(read.get_pixels(OpenImageIO.UINT8), dtype=np.uint8))
        )

    return scoped().map_error(functools.partial(Fault.of, ImagingReason.CODEC, str(rgb.shape))).bind(cycles)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Depth", "Diffused", "IntegerDepth", "Rounded", "Subsampling", "generations"]
