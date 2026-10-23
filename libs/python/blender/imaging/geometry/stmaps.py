"""ST map baking and filtered warps."""

from collections.abc import Callable
import math
from typing import Literal, Self

import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.imaging.faults import ImagingBufferFault, ImagingResult, paired
from blender.imaging.parameters import Choice, Positive

# --- [TYPES] ----------------------------------------------------------------------------


class Extension(Choice):
    """Sampling rule past source edge."""

    CLIP = 1
    EXTEND = 2
    REPEAT = 3
    MIRROR = 4


# --- [MODELS] ---------------------------------------------------------------------------


class StMap(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Source positions with coverage in blue and alpha, rows top first."""

    plane: npt.NDArray[np.float32]
    extension: tuple[Extension, Extension]

    @classmethod
    def bake(
        cls,
        width: int,
        height: int,
        mapping: Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]],
        extension: tuple[Extension, Extension] = (Extension.CLIP, Extension.CLIP),
        *,
        source_size: tuple[int, int],
    ) -> Self:
        """Evaluates `mapping` once, folding each axis at source texel centers."""

        def folded(position: npt.NDArray[np.float64], size: int, mode: Extension) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
            center = 0.5 / size
            match mode:
                case Extension.CLIP:
                    weight = np.clip(np.minimum(position, 1.0 - position) * size + 0.5, 0.0, 1.0)
                    return np.clip(position, center, 1.0 - center), weight
                case Extension.EXTEND:
                    return np.clip(position, center, 1.0 - center), np.ones_like(position)
                case Extension.REPEAT:
                    return np.mod(position, 1.0), np.ones_like(position)
                case Extension.MIRROR:
                    return np.clip(1.0 - np.abs(np.mod(position, 2.0) - 1.0), center, 1.0 - center), np.ones_like(position)

        s, t = np.meshgrid((np.arange(width) + 0.5) / width, 1.0 - (np.arange(height) + 0.5) / height)
        mapped = mapping(np.stack((s, t, np.ones_like(s)), axis=-1))
        x, y = (folded(mapped[..., axis], size, mode) for axis, (size, mode) in enumerate(zip(source_size, extension)))
        coverage = mapped[..., 2] * x[1] * y[1]
        return cls(plane=np.ascontiguousarray(np.stack((x[0], y[0], coverage, coverage), axis=-1), dtype=np.float32), extension=extension)

    def warp(self, plane: npt.NDArray[np.float32], *, filtername: str = "triangle", filterwidth: Positive = 2.0) -> ImagingResult[npt.NDArray[np.float32]]:
        """Returns `plane` warped through filter, weighted by coverage."""

        def padding(mode: Extension) -> Literal["constant", "edge", "wrap", "symmetric"]:
            match mode:
                case Extension.CLIP:
                    return "constant"
                case Extension.EXTEND:
                    return "edge"
                case Extension.REPEAT:
                    return "wrap"
                case Extension.MIRROR:
                    return "symmetric"

        height, width, channels = plane.shape
        x, y = self.extension
        border = math.ceil(filterwidth / 2.0)
        margin = np.array((border, border, 0.0))
        size = np.array((width, height, 1.0))
        source = OpenImageIO.ImageBuf(np.pad(np.pad(plane, ((0, 0), (border, border), (0, 0)), mode=padding(x)), ((border, border), (0, 0), (0, 0)), mode=padding(y)))
        st = OpenImageIO.ImageBuf(np.ascontiguousarray((self.plane[..., :3] * size + margin) / (size + 2.0 * margin), dtype=np.float32))
        roi = OpenImageIO.ROI(0, self.plane.shape[1], 0, self.plane.shape[0], 0, 1, 0, channels)
        return (
            paired(ImagingBufferFault.checked(source), ImagingBufferFault.checked(st))
            .bind(
                lambda buffers: paired(
                    ImagingBufferFault.checked(OpenImageIO.ImageBufAlgo.st_warp(buffers[0], buffers[1], filtername, filterwidth, flip_t=True, roi=roi)),
                    ImagingBufferFault.checked(OpenImageIO.ImageBufAlgo.channels(buffers[1], (2,) * channels)),
                )
            )
            .bind(lambda buffers: ImagingBufferFault.checked(OpenImageIO.ImageBufAlgo.mul(*buffers)))
            .bind(ImagingBufferFault.pixels)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Extension", "StMap"]
