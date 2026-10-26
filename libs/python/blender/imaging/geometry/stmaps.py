"""ST maps, filtered warps, and displacement of raster planes through smoothed, sloped, and tapped fields."""

from collections.abc import Callable
import math
from typing import Literal, Self

import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from scipy.ndimage import gaussian_filter, map_coordinates, sobel

from blender.imaging.files.buffers import checked, pixels
from blender.imaging.sampling.integration import Raster
from blender.records.faults import Outcome, paired
from blender.records.fields import Positive

# --- [MODELS] ---------------------------------------------------------------------------


class StMap(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Source positions with coverage in blue and alpha, rows top first, sampled past each source edge by its OIIO wrap."""

    plane: npt.NDArray[np.float32]
    extension: tuple[OpenImageIO.Wrap, OpenImageIO.Wrap]

    @classmethod
    def of(cls, mapped: npt.NDArray[np.float64], extension: tuple[OpenImageIO.Wrap, OpenImageIO.Wrap], *, source_size: tuple[int, int]) -> Self:
        """Returns map of `(rows, columns, 3)` source positions with coverage, folding each axis at source texel centers."""

        def folded(position: npt.NDArray[np.float64], size: int, mode: OpenImageIO.Wrap) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
            center = 0.5 / size
            match mode:
                case OpenImageIO.Wrap.Periodic:
                    return np.mod(position, 1.0), np.ones_like(position)
                case OpenImageIO.Wrap.Mirror:
                    return np.clip(1.0 - np.abs(np.mod(position, 2.0) - 1.0), center, 1.0 - center), np.ones_like(position)
                case OpenImageIO.Wrap.Clamp:
                    return np.clip(position, center, 1.0 - center), np.ones_like(position)
                case _:
                    return np.clip(position, center, 1.0 - center), np.clip(np.minimum(position, 1.0 - position) * size + 0.5, 0.0, 1.0)

        x, y = (folded(mapped[..., axis], size, mode) for axis, (size, mode) in enumerate(zip(source_size, extension)))
        coverage = mapped[..., 2] * x[1] * y[1]
        return cls(plane=np.ascontiguousarray(np.stack((x[0], y[0], coverage, coverage), axis=-1), dtype=np.float32), extension=extension)

    @classmethod
    def bake(
        cls,
        width: int,
        height: int,
        mapping: Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]],
        extension: tuple[OpenImageIO.Wrap, OpenImageIO.Wrap] = (OpenImageIO.Wrap.Black, OpenImageIO.Wrap.Black),
        *,
        source_size: tuple[int, int],
    ) -> Self:
        """Evaluates `mapping` once over output texel centers with unit coverage."""
        s, t = np.meshgrid((np.arange(width) + 0.5) / width, 1.0 - (np.arange(height) + 0.5) / height)
        return cls.of(mapping(np.stack((s, t, np.ones_like(s)), axis=-1)), extension, source_size=source_size)

    def warp(self, plane: npt.NDArray[np.float32], *, filtername: str = "triangle", filterwidth: Positive = 2.0) -> Outcome[npt.NDArray[np.float32]]:
        """Returns `plane` warped through filter, weighted by coverage."""

        def padding(mode: OpenImageIO.Wrap) -> Literal["constant", "edge", "wrap", "symmetric"]:
            match mode:
                case OpenImageIO.Wrap.Periodic:
                    return "wrap"
                case OpenImageIO.Wrap.Mirror:
                    return "symmetric"
                case OpenImageIO.Wrap.Clamp:
                    return "edge"
                case _:
                    return "constant"

        height, width, channels = plane.shape
        x, y = self.extension
        border = math.ceil(filterwidth / 2.0)
        margin = np.array((border, border, 0.0))
        size = np.array((width, height, 1.0))
        source = OpenImageIO.ImageBuf(np.pad(np.pad(plane, ((0, 0), (border, border), (0, 0)), mode=padding(x)), ((border, border), (0, 0), (0, 0)), mode=padding(y)))
        st = OpenImageIO.ImageBuf(np.ascontiguousarray((self.plane[..., :3] * size + margin) / (size + 2.0 * margin), dtype=np.float32))
        roi = OpenImageIO.ROI(0, self.plane.shape[1], 0, self.plane.shape[0], 0, 1, 0, channels)
        return (
            paired(checked(source), checked(st))
            .bind(
                lambda buffers: paired(
                    checked(OpenImageIO.ImageBufAlgo.st_warp(buffers[0], buffers[1], filtername, filterwidth, flip_t=True, roi=roi)),
                    checked(OpenImageIO.ImageBufAlgo.channels(buffers[1], (2,) * channels)),
                )
            )
            .bind(lambda buffers: checked(OpenImageIO.ImageBufAlgo.mul(*buffers)))
            .bind(pixels)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def softened(plane: npt.NDArray[np.float64], raster: Raster, radius: float) -> npt.NDArray[np.float64]:
    """Returns `plane` over `raster` blurred by a Gaussian of `radius` global units, edges extended."""
    step = raster.step
    return gaussian_filter(plane, (radius / step[1], radius / step[0]), axes=(0, 1), mode="nearest")


def slope(plane: npt.NDArray[np.float64], raster: Raster) -> npt.NDArray[np.float64]:
    """Returns the Sobel gradient of a scalar `plane` over `raster` per global unit, rightward and upward, as `(rows, columns, 2)`."""
    step = raster.step
    return np.stack((sobel(plane, axis=1, mode="nearest") / (8.0 * step[0]), -sobel(plane, axis=0, mode="nearest") / (8.0 * step[1])), axis=-1)


def tapped(plane: npt.NDArray[np.float64], raster: Raster, points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns bilinear samples of a scalar `plane` over `raster` at global `(..., 2)` points, edges extended."""
    left, _, _, top = raster.bounds
    step = raster.step
    return map_coordinates(plane, np.stack(((top - points[..., 1]) / step[1] - 0.5, (points[..., 0] - left) / step[0] - 0.5)), order=1, mode="nearest", prefilter=False)


def displaced(
    plane: npt.NDArray[np.float32],
    raster: Raster,
    positions: npt.NDArray[np.float64],
    coverage: npt.NDArray[np.float64],
    extension: tuple[OpenImageIO.Wrap, OpenImageIO.Wrap],
    *,
    filtername: str = "triangle",
    filterwidth: Positive = 2.0,
) -> Outcome[npt.NDArray[np.float32]]:
    """Returns `plane` over `raster` sampled at each pixel's global source position through `extension`, weighted by `coverage`."""
    left, bottom, right, top = raster.bounds
    height, width = raster.shape
    mapped = np.stack(((positions[..., 0] - left) / (right - left), (positions[..., 1] - bottom) / (top - bottom), coverage), axis=-1)
    return StMap.of(mapped, extension, source_size=(width, height)).warp(plane, filtername=filtername, filterwidth=filterwidth)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["StMap", "displaced", "slope", "softened", "tapped"]
