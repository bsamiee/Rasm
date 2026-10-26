"""Closed-form projective maps between convex image quads and unit plane."""

from typing import Self

from expression import Nothing, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.imaging.analysis.metering import Region
from blender.imaging.parameters import Position

# --- [MODELS] ---------------------------------------------------------------------------


class Quad(msgspec.Struct, frozen=True, kw_only=True):
    """Image quad corners."""

    top_left: Position = (0.0, 1.0)
    top_right: Position = (1.0, 1.0)
    bottom_right: Position = (1.0, 0.0)
    bottom_left: Position = (0.0, 0.0)

    @classmethod
    def of(cls, samples: npt.NDArray[np.float64]) -> Self:
        """Reads corners from first two sample components."""
        return cls(**{name: (float(samples[index, 0]), float(samples[index, 1])) for index, name in enumerate(cls.__struct_fields__)})

    @classmethod
    def of_region(cls, region: Region) -> Self:
        """Returns corners of an axis-aligned frame rectangle."""
        return cls(top_left=(region.left, region.top), top_right=(region.right, region.top), bottom_right=(region.right, region.bottom), bottom_left=(region.left, region.bottom))

    @property
    def samples(self) -> npt.NDArray[np.float64]:
        """Corner positions with unit coverage in declaration order."""
        return np.array([(*corner, 1.0) for corner in msgspec.structs.astuple(self)])


class Homography(msgspec.Struct, frozen=True, kw_only=True):
    """Projective map between strictly convex quad and unit plane."""

    quad: Quad

    @classmethod
    def of(cls, quad: Quad) -> Option[Self]:
        """Returns map of quad whose corner turns share one sign."""
        corners = quad.samples[:, :2]
        edges = np.roll(corners, -1, axis=0) - corners
        following = np.roll(edges, -1, axis=0)
        turns = edges[:, 0] * following[:, 1] - edges[:, 1] * following[:, 0]
        return Some(cls(quad=quad)) if bool((turns > 0.0).all() or (turns < 0.0).all()) else Nothing

    @property
    def matrix(self) -> npt.NDArray[np.float64]:
        """Square-to-quad coefficients with positive near-side homogeneous scale."""
        (x0, y0), (x1, y1), (x2, y2), (x3, y3) = self.quad.bottom_left, self.quad.bottom_right, self.quad.top_right, self.quad.top_left
        sx, sy = x0 - x1 + x2 - x3, y0 - y1 + y2 - y3
        turn = (x1 - x2) * (y3 - y2) - (x3 - x2) * (y1 - y2)
        sign = 1.0 if turn > 0.0 else -1.0
        g = (sx * (y3 - y2) - (x3 - x2) * sy) * sign
        h = ((x1 - x2) * sy - sx * (y1 - y2)) * sign
        i = abs(turn)
        return np.array((((x1 - x0) * i + g * x1, (x3 - x0) * i + h * x3, x0 * i), ((y1 - y0) * i + g * y1, (y3 - y0) * i + h * y3, y0 * i), (g, h, i)), dtype=np.float64)

    def to_image(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps plane samples to image, zero coverage beyond vanishing line."""
        return _projected(self.matrix, samples)

    def to_plane(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps image samples to plane, zero coverage beyond vanishing line."""
        return _projected(np.linalg.inv(self.matrix), samples)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _projected(matrix: npt.NDArray[np.float64], samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    finite = np.isfinite(samples[..., :2]).all(axis=-1, keepdims=True)
    homogeneous = np.where(finite, samples[..., :2], 0.0) @ matrix[:, :2].T + matrix[:, 2]
    near = finite & (homogeneous[..., 2:] > 0.0)
    with np.errstate(over="ignore"):
        planar = np.divide(homogeneous[..., :2], homogeneous[..., 2:], out=np.zeros_like(homogeneous[..., :2]), where=near)
    covered = near & np.isfinite(planar).all(axis=-1, keepdims=True)
    return np.concatenate((np.where(covered, planar, 0.0), np.where(covered, samples[..., 2:], 0.0)), axis=-1)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Homography", "Quad"]
