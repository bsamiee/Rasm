"""Source coordinates, coverage, and filtered sampling of image maps."""

from collections.abc import Callable
import math
import sys
from typing import Annotated, Literal, Self

from expression import Error, Ok, Option
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from rasm.imaging.faults import ImagingBufferFault, ImagingResult, paired
from rasm.imaging.parameters import Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Extension(Choice):
    """Sampling rule past a source edge."""

    CLIP = 1, "Clip"
    EXTEND = 2, "Extend"
    REPEAT = 3, "Repeat"
    MIRROR = 4, "Mirror"


# --- [MODELS] ---------------------------------------------------------------------------


class StMap(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Source positions with coverage in both blue and alpha, rows top first."""

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
        """Evaluates the mapping once and folds each axis at the source's texel centers."""

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
        x, y = (folded(mapped[..., axis], size, mode) for axis, (size, mode) in enumerate(zip(source_size, extension, strict=False)))
        coverage = mapped[..., 2] * x[1] * y[1]
        return cls(plane=np.ascontiguousarray(np.stack((x[0], y[0], coverage, coverage), axis=-1), dtype=np.float32), extension=extension)

    def warp(
        self, plane: npt.NDArray[np.float32], *, filtername: str = "triangle", filterwidth: Annotated[float, msgspec.Meta(gt=0.0, le=sys.float_info.max)] = 2.0
    ) -> ImagingResult[npt.NDArray[np.float32]]:
        """Warps with a native OIIO filter of positive full width, returning typed buffer refusals."""

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

        def lifted(request: OpenImageIO.ImageBuf, answer: OpenImageIO.ImageBuf) -> ImagingResult[OpenImageIO.ImageBuf]:
            return Error((ImagingBufferFault(request=request, error=answer.geterror()),)) if answer.has_error else Ok(answer)

        height, width, channels = plane.shape
        x, y = self.extension
        border = math.ceil(filterwidth / 2.0)
        margin = np.array((border, border, 0.0))
        size = np.array((width, height, 1.0))
        source = OpenImageIO.ImageBuf(np.pad(np.pad(plane, ((0, 0), (border, border), (0, 0)), mode=padding(x)), ((border, border), (0, 0), (0, 0)), mode=padding(y)))
        st = OpenImageIO.ImageBuf(np.ascontiguousarray((self.plane[..., :3] * size + margin) / (size + 2.0 * margin), dtype=np.float32))
        roi = OpenImageIO.ROI(0, self.plane.shape[1], 0, self.plane.shape[0], 0, 1, 0, channels)
        return (
            paired(lifted(source, source), lifted(st, st))
            .bind(
                lambda buffers: paired(
                    lifted(buffers[0], OpenImageIO.ImageBufAlgo.st_warp(buffers[0], buffers[1], filtername, filterwidth, flip_t=True, roi=roi)),
                    lifted(buffers[1], OpenImageIO.ImageBufAlgo.channels(buffers[1], (2,) * channels)),
                )
            )
            .bind(lambda buffers: lifted(buffers[0], OpenImageIO.ImageBufAlgo.mul(*buffers)))
            .bind(lambda product: Option.of_optional(product.get_pixels(OpenImageIO.FLOAT)).to_result_with(lambda: (ImagingBufferFault(request=product, error=product.geterror()),)))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Extension", "StMap"]
