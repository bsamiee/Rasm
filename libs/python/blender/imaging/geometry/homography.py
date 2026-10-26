"""Projective maps between convex image quads and the unit plane."""

from typing import Self

from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
from skimage.transform import ProjectiveTransform

from blender.imaging.faults import ImagingResult
from blender.imaging.geometry.regions import Position, Region
from blender.records.faults import DomainFault

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
    def of(cls, quad: Quad) -> ImagingResult[Option[Self]]:
        """Returns map of `quad`, absent for a quad that is not strictly convex, domain fault when its corners admit no projective map."""
        if not quad.convex:
            return Ok(Nothing)
        match ProjectiveTransform.from_estimate(Quad().samples[:, :2], quad.samples[:, :2]):  # type: ignore[no-untyped-call]
            case ProjectiveTransform() as found:
                return Ok(Some(cls(quad=quad, matrix=np.asarray(found.params, dtype=np.float64))))
            case failed:
                return Error((DomainFault(subject="Homography", value=msgspec.structs.astuple(quad), accepted=f"corners of a projective quad, {failed}"),))

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
