"""Rectangles, pixel rasters, and quads from the bottom left, projective maps from the unit plane onto convex quads, ST maps, filtered warps, and displacement through smoothed, sloped, and tapped fields."""

from collections.abc import Callable, Mapping
import math
from types import MappingProxyType
from typing import Final, Literal, Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
from scipy.ndimage import gaussian_filter, map_coordinates, sobel
from skimage.transform import ProjectiveTransform

from blender.imaging.files.buffers import checked, pixels
from blender.records.faults import Fault, Outcome, paired, Reason
from blender.records.fields import Count, Finite

# --- [CONSTANTS] ------------------------------------------------------------------------

_PADDING: Final[Mapping[OpenImageIO.Wrap, Literal["wrap", "symmetric", "edge"]]] = MappingProxyType({
    OpenImageIO.Wrap.Periodic: "wrap",
    OpenImageIO.Wrap.Mirror: "symmetric",
    OpenImageIO.Wrap.Clamp: "edge",
})

# --- [MODELS] ---------------------------------------------------------------------------


class Region(msgspec.Struct, frozen=True, kw_only=True):
    """Rectangle sides from the bottom left, in frame shares or in a field's own coordinates, left at most right and bottom at most top, the unit frame by default."""

    left: Finite = 0.0
    bottom: Finite = 0.0
    right: Finite = 1.0
    top: Finite = 1.0

    def __post_init__(self) -> None:
        """Refuses sides out of order."""
        if self.left > self.right or self.bottom > self.top:
            raise ValueError(f"{self!r} holds sides out of order")

    @classmethod
    def bounding(cls, points: npt.NDArray[np.float64]) -> Self:
        """Returns rectangle spanning the first two components of `points`."""
        corners = points[..., :2].reshape(-1, 2)
        (left, bottom), (right, top) = corners.min(axis=0), corners.max(axis=0)
        return cls(left=float(left), bottom=float(bottom), right=float(right), top=float(top))

    def window(self, height: int, width: int) -> tuple["slice[int, int, None]", "slice[int, int, None]"]:
        """Returns row and column slices of a top-first plane holding the pixels whose centers lie strictly inside, intersected with the frame."""

        def span(low: float, high: float, extent: int) -> "slice[int, int, None]":
            start = min(max(math.floor(low * extent - 0.5) + 1, 0), extent)
            return slice(start, min(max(math.ceil(high * extent - 0.5), start), extent))

        return span(1.0 - self.top, 1.0 - self.bottom, height), span(self.left, self.right, width)

    def mask(self, height: int, width: int) -> npt.NDArray[np.bool]:
        """Returns the top-first plane mask of the pixels `window` holds."""
        held = np.zeros((height, width), dtype=np.bool)
        held[self.window(height, width)] = True
        return held


class Raster(msgspec.Struct, frozen=True, kw_only=True):
    """Pixel rows and columns splitting a global rectangle, rows top first."""

    bounds: Region
    shape: tuple[Count, Count]

    @property
    def step(self) -> npt.NDArray[np.float64]:
        """Pixel width and height."""
        return np.array(((self.bounds.right - self.bounds.left) / self.shape[1], (self.bounds.top - self.bounds.bottom) / self.shape[0]))

    @property
    def corners(self) -> npt.NDArray[np.float64]:
        """Bottom-left corner of each pixel as `(rows, columns, 2)`."""
        step = self.step
        return np.stack(np.meshgrid(self.bounds.left + np.arange(self.shape[1]) * step[0], self.bounds.top - (np.arange(self.shape[0]) + 1) * step[1]), axis=-1)

    @property
    def centers(self) -> npt.NDArray[np.float64]:
        """Center of each pixel as `(rows, columns, 2)`."""
        return self.corners + self.step / 2.0


class Quad(msgspec.Struct, frozen=True, kw_only=True):
    """Image quad corners clockwise from top left."""

    corners: tuple[tuple[Finite, Finite], tuple[Finite, Finite], tuple[Finite, Finite], tuple[Finite, Finite]] = ((0.0, 1.0), (1.0, 1.0), (1.0, 0.0), (0.0, 0.0))

    @classmethod
    def of(cls, samples: npt.NDArray[np.float64]) -> Self:
        """Reads corners from first two components of the first four samples."""
        first, second, third, fourth = ((float(row[0]), float(row[1])) for row in samples[:4])
        return cls(corners=(first, second, third, fourth))

    @classmethod
    def of_region(cls, region: Region) -> Self:
        """Returns corners of an axis-aligned frame rectangle."""
        return cls(corners=((region.left, region.top), (region.right, region.top), (region.right, region.bottom), (region.left, region.bottom)))

    @property
    def samples(self) -> npt.NDArray[np.float64]:
        """Corner positions with unit coverage clockwise from top left."""
        return np.array([(*corner, 1.0) for corner in self.corners])

    @property
    def convex(self) -> bool:
        """Whether every corner turn shares one sign."""
        corners = self.samples[:, :2]
        edges = np.roll(corners, -1, axis=0) - corners
        following = np.roll(edges, -1, axis=0)
        turns = edges[:, 0] * following[:, 1] - edges[:, 1] * following[:, 0]
        return bool((turns > 0.0).all() or (turns < 0.0).all())


class Homography(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Projective map from the unit plane onto a strictly convex quad, homogeneous scale one at the plane origin."""

    quad: Quad
    matrix: npt.NDArray[np.float64]

    @classmethod
    def of(cls, quad: Quad) -> Outcome[Option[Self]]:
        """Returns map of `quad`, absent for a quad that is not strictly convex, domain fault when its corners admit no projective map."""
        if not quad.convex:
            return Ok(Nothing)
        match ProjectiveTransform.from_estimate(Quad().samples[:, :2], quad.samples[:, :2]):
            case ProjectiveTransform() as found:
                return Ok(Some(cls(quad=quad, matrix=np.asarray(found.params, dtype=np.float64))))
            case failed:
                return Error(Fault.of(Reason.OUTSIDE, cls.__name__, str(quad.corners), member=f"corners of a projective quad, {failed}"))

    def to_image(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps plane samples to image, zero coverage beyond vanishing line."""
        return _projected(self.matrix, samples)

    def to_plane(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps image samples to plane, zero coverage beyond vanishing line."""
        return _projected(np.asarray(np.linalg.inv(self.matrix), dtype=np.float64), samples)


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

    def warp(self, plane: npt.NDArray[np.float32], *, filtername: str = "triangle", filterwidth: float = 2.0) -> Outcome[npt.NDArray[np.float32]]:
        """Returns `plane` warped through filter, weighted by coverage."""
        height, width, channels = plane.shape
        x, y = self.extension
        border = math.ceil(filterwidth / 2.0)
        margin = np.array((border, border, 0.0))
        size = np.array((width, height, 1.0))
        source = OpenImageIO.ImageBuf(np.pad(np.pad(plane, ((0, 0), (border, border), (0, 0)), mode=_PADDING.get(x, "constant")), ((border, border), (0, 0), (0, 0)), mode=_PADDING.get(y, "constant")))
        st = OpenImageIO.ImageBuf(np.ascontiguousarray((self.plane[..., :3] * size + margin) / (size + 2.0 * margin), dtype=np.float32))
        roi = OpenImageIO.ROI(0, self.plane.shape[1], 0, self.plane.shape[0], 0, 1, 0, channels)
        return (
            paired(checked(OpenImageIO.ImageBufAlgo.st_warp(source, st, filtername, filterwidth, flip_t=True, roi=roi)), checked(OpenImageIO.ImageBufAlgo.channels(st, (2,) * channels)))
            .bind(lambda buffers: checked(OpenImageIO.ImageBufAlgo.mul(*buffers)))
            .bind(pixels)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


@np.errstate(over="ignore")
def _projected(matrix: npt.NDArray[np.float64], samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    finite = np.isfinite(samples[..., :2]).all(axis=-1, keepdims=True)
    homogeneous = np.where(finite, samples[..., :2], 0.0) @ matrix[:, :2].T + matrix[:, 2]
    near = finite & (homogeneous[..., 2:] > 0.0)
    planar = np.divide(homogeneous[..., :2], homogeneous[..., 2:], out=np.zeros_like(homogeneous[..., :2]), where=near)
    covered = near & np.isfinite(planar).all(axis=-1, keepdims=True)
    return np.concatenate((np.where(covered, planar, 0.0), np.where(covered, samples[..., 2:], 0.0)), axis=-1)


def softened(plane: npt.NDArray[np.float64], raster: Raster, radius: float) -> npt.NDArray[np.float64]:
    """Returns `plane` over `raster` blurred by a Gaussian of `radius` global units, edges extended."""
    return gaussian_filter(plane, radius / raster.step[::-1], axes=(0, 1), mode="nearest")


def slope(plane: npt.NDArray[np.float64], raster: Raster) -> npt.NDArray[np.float64]:
    """Returns the Sobel gradient of a scalar `plane` over `raster` per global unit, rightward and upward, as `(rows, columns, 2)`."""
    return np.stack((sobel(plane, axis=1, mode="nearest") / (8.0 * raster.step[0]), -sobel(plane, axis=0, mode="nearest") / (8.0 * raster.step[1])), axis=-1)


def tapped(plane: npt.NDArray[np.float64], raster: Raster, points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns bilinear samples of a scalar `plane` over `raster` at global `(..., 2)` points, edges extended."""
    step = raster.step
    return map_coordinates(plane, np.stack(((raster.bounds.top - points[..., 1]) / step[1] - 0.5, (points[..., 0] - raster.bounds.left) / step[0] - 0.5)), order=1, mode="nearest", prefilter=False)


def displaced(
    plane: npt.NDArray[np.float32],
    raster: Raster,
    positions: npt.NDArray[np.float64],
    coverage: npt.NDArray[np.float64],
    extension: tuple[OpenImageIO.Wrap, OpenImageIO.Wrap],
    *,
    filtername: str = "triangle",
    filterwidth: float = 2.0,
) -> Outcome[npt.NDArray[np.float32]]:
    """Returns `plane` over `raster` sampled at each pixel's global source position through `extension`, weighted by `coverage`."""
    bounds = raster.bounds
    height, width = raster.shape
    mapped = np.stack(((positions[..., 0] - bounds.left) / (bounds.right - bounds.left), (positions[..., 1] - bounds.bottom) / (bounds.top - bounds.bottom), coverage), axis=-1)
    return StMap.of(mapped, extension, source_size=(width, height)).warp(plane, filtername=filtername, filterwidth=filterwidth)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Homography", "Quad", "Raster", "Region", "StMap", "displaced", "slope", "softened", "tapped"]
