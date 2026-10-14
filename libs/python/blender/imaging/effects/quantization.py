"""Display quantization, blue-noise ranks, and whole-frame error diffusion."""

import functools
from typing import Annotated, Literal, overload

from expression import Option
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.faults import ImagingBufferFault, ImagingResult
from blender.imaging.parameters import Case

# --- [TYPES] ----------------------------------------------------------------------------

type IntegerDepth = Literal[8, 16]

# --- [MODELS] ---------------------------------------------------------------------------


class BlueNoise(Case, frozen=True, kw_only=True, tag=1):
    """Triangular blue-noise dither at one LSB with exact black and white endpoints."""

    title = "Blue Noise"

    @staticmethod
    @functools.cache
    def tile() -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns centered provider ranks in one float32 channel, with rows top first."""
        ranks = OpenImageIO.ImageBufAlgo.bluenoise_image()
        return (
            Option
            .of_optional(ranks.get_pixels(OpenImageIO.UINT16))
            .to_result_with(lambda: (ImagingBufferFault(request=ranks, error=ranks.geterror()),))
            .map(lambda pixels: np.ascontiguousarray(pixels[::-1, :, :1], dtype=np.float32))
            .map(lambda table: (table + 0.5) / float(table.size))
        )


class Diffusion(Case, frozen=True, kw_only=True, tag=2):
    """Floyd-Steinberg diffusion of separate color errors in raster order."""

    title = "Error Diffusion"

    @staticmethod
    def quantize(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> npt.NDArray[np.uint8 | np.uint16]:
        """Quantizes a straight BW, RGB, or RGBA frame, rounding alpha without diffusion."""
        height, width, _ = plane.shape
        coded = codes(plane, depth)
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


class Dither(Effect, frozen=True, kw_only=True, tag=45):
    """Blue-noise dither or whole-frame error diffusion before integer output."""

    stage = Stage.QUANTIZATION
    title = "Dither"
    pointwise = False
    method: Annotated[BlueNoise | Diffusion, msgspec.Meta(title="Method", description="Blue-noise dither or whole-frame error diffusion")] = BlueNoise()


# --- [OPERATIONS] -----------------------------------------------------------------------


@overload
def codes(plane: npt.NDArray[np.float32], depth: Literal[8]) -> npt.NDArray[np.uint8]: ...


@overload
def codes(plane: npt.NDArray[np.float32], depth: Literal[16]) -> npt.NDArray[np.uint16]: ...


def codes(plane: npt.NDArray[np.float32], depth: IntegerDepth) -> npt.NDArray[np.uint8 | np.uint16]:
    """Rounds straight display values half up into clamped integer codes of the same shape."""
    dtype = np.uint8 if depth == 8 else np.uint16
    limits = np.iinfo(dtype)
    scaled = np.floor(plane * limits.max + 0.5)
    return np.clip(scaled, 0, limits.max, out=scaled).astype(dtype)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BlueNoise", "Diffusion", "Dither", "IntegerDepth", "codes"]
