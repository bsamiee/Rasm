"""Frame rectangles and quads in shares from the bottom left, and projective maps from the unit plane onto convex quads."""

from typing import Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
from skimage.transform import ProjectiveTransform

from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import Finite, Unit

# --- [MODELS] ---------------------------------------------------------------------------


class Region(msgspec.Struct, frozen=True, kw_only=True):
    """Frame rectangle in shares from bottom left."""

    left: Unit = 0.0
    bottom: Unit = 0.0
    right: Unit = 1.0
    top: Unit = 1.0

    @classmethod
    def bounding(cls, points: npt.NDArray[np.float64]) -> Self:
        """Returns rectangle spanning the first two components of `points`."""
        corners = points[..., :2].reshape(-1, 2)
        (left, bottom), (right, top) = corners.min(axis=0), corners.max(axis=0)
        return cls(left=float(left), bottom=float(bottom), right=float(right), top=float(top))

    def window(self, height: int, width: int) -> tuple["slice[int, int, None]", "slice[int, int, None]"]:
        """Returns row and column slices of a top-first plane."""
        return slice(round((1.0 - self.top) * height), round((1.0 - self.bottom) * height)), slice(round(self.left * width), round(self.right * width))


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
                return Error(Fault.of(RecordReason.OUTSIDE, cls.__name__, str(quad.corners), member=f"corners of a projective quad, {failed}"))

    def to_image(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps plane samples to image, zero coverage beyond vanishing line."""
        return _projected(self.matrix, samples)

    def to_plane(self, samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Maps image samples to plane, zero coverage beyond vanishing line."""
        return _projected(np.asarray(np.linalg.inv(self.matrix), dtype=np.float64), samples)


# --- [OPERATIONS] -----------------------------------------------------------------------


@np.errstate(over="ignore")
def _projected(matrix: npt.NDArray[np.float64], samples: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    finite = np.isfinite(samples[..., :2]).all(axis=-1, keepdims=True)
    homogeneous = np.where(finite, samples[..., :2], 0.0) @ matrix[:, :2].T + matrix[:, 2]
    near = finite & (homogeneous[..., 2:] > 0.0)
    planar = np.divide(homogeneous[..., :2], homogeneous[..., 2:], out=np.zeros_like(homogeneous[..., :2]), where=near)
    covered = near & np.isfinite(planar).all(axis=-1, keepdims=True)
    return np.concatenate((np.where(covered, planar, 0.0), np.where(covered, samples[..., 2:], 0.0)), axis=-1)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Homography", "Quad", "Region"]
