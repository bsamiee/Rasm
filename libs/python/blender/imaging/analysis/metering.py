"""Covered samples, exposure metering, frame white, and absolute PQ peak."""

import math
from typing import Annotated, ClassVar, Self

import colour
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.chain import GREY
from blender.imaging.color.processors import luma, unassociated
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import Gated, Presentation, Quantity, Unit
from blender.imaging.sampling import Frozen

# --- [MODELS] ---------------------------------------------------------------------------


class Region(msgspec.Struct, frozen=True, kw_only=True):
    """Frame rectangle in shares from bottom left."""

    left: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 0.0
    bottom: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 0.0
    right: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 1.0
    top: Annotated[Unit, Presentation(quantity=Some(Quantity.FRAME))] = 1.0

    @classmethod
    def bounding(cls, points: npt.NDArray[np.float64]) -> Self:
        """Returns rectangle spanning the first two components of `points`."""
        corners = points[..., :2].reshape(-1, 2)
        (left, bottom), (right, top) = corners.min(axis=0), corners.max(axis=0)
        return cls(left=float(left), bottom=float(bottom), right=float(right), top=float(top))

    def window(self, height: int, width: int) -> tuple["slice[int, int, None]", "slice[int, int, None]"]:
        """Returns row and column slices of a top-first plane."""
        return slice(round((1.0 - self.top) * height), round((1.0 - self.bottom) * height)), slice(round(self.left * width), round(self.right * width))


class Extent(msgspec.Struct, frozen=True, kw_only=True):
    """Lowest covered luminance and percentile frame white."""

    percentile: ClassVar[float] = 99.995
    low: float
    white: float


class Samples(Frozen, frozen=True, kw_only=True, eq=False):
    """Straight RGB, float64 working luminance, and pixel coverage."""

    floor: ClassVar[float] = 1e-5
    color: npt.NDArray[np.float32]
    luminance: npt.NDArray[np.float64]
    covered: npt.NDArray[np.bool]

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> ImagingResult[Self]:
        """Reads associated RGB or RGBA as straight samples."""
        straight, covered = unassociated(plane)[..., :3], (plane[..., 3:] > 0.0).all(axis=-1)
        return luma(config).map(lambda coefficients: cls(color=straight, luminance=np.where(covered, np.matmul(straight, coefficients, dtype=np.float64), 0.0), covered=covered))

    @classmethod
    def logarithm(cls, values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Returns log2 of values clamped to `floor`."""
        return np.log2(np.maximum(values, cls.floor))

    @property
    def finite(self) -> npt.NDArray[np.bool]:
        """Covered pixels with finite luminance."""
        return self.covered & np.isfinite(self.luminance)

    @property
    def central(self) -> npt.NDArray[np.bool]:
        """Middle half along both frame axes."""
        height, width = self.covered.shape
        rows, columns = np.arange(height, dtype=np.float64)[:, None], np.arange(width, dtype=np.float64)[None, :]
        return np.asarray((np.abs(rows + 0.5 - height / 2.0) < height / 4.0) & (np.abs(columns + 0.5 - width / 2.0) < width / 4.0), dtype=np.bool)

    @property
    def peak(self) -> Option[float]:
        """Covered percentile PQ peak in cd/m²."""
        greatest = self.color.max(axis=-1)
        return (
            Some(greatest[self.covered & np.isfinite(greatest)].astype(np.float64))
            .filter(lambda found: found.size > 0)
            .map(lambda found: float(colour.models.eotf_ST2084(np.clip(np.percentile(found, Extent.percentile, method="inverted_cdf"), 0.0, 1.0))))
        )

    def percentile(self, rank: Annotated[float, msgspec.Meta(ge=0.0, le=100.0)]) -> Option[float]:
        """Returns covered luminance at `rank` percentile."""
        return Some(self.luminance[self.finite]).filter(lambda found: found.size > 0).map(lambda found: float(np.percentile(found, rank, method="inverted_cdf")))

    def extent(self) -> Option[Extent]:
        """Returns covered luminance extent."""
        return self.percentile(Extent.percentile).map(lambda white: Extent(low=float(self.luminance[self.finite].min()), white=white))

    def log_statistics(self) -> Option[tuple[float, float, float]]:
        """Returns mean, maximum, and minimum covered log luminance."""
        return Some(self.logarithm(self.luminance[self.finite])).filter(lambda found: found.size > 0).map(lambda found: (float(found.mean(dtype=np.float64)), float(found.max()), float(found.min())))


class Meter(msgspec.Struct, frozen=True, kw_only=True):
    """Exposure percentile trims, center weight, and region."""

    low: Annotated[float, msgspec.Meta(ge=0.0, le=100.0, title="Low Trim"), Presentation(step=Some(1.0), precision=Some(1))] = 0.0
    high: Annotated[float, msgspec.Meta(ge=0.0, le=100.0, title="High Trim"), Presentation(step=Some(1.0), precision=Some(1))] = 100.0
    centre: Annotated[Unit, msgspec.Meta(title="Center Weight")] = 0.0
    region: Gated[Region] = Gated(enabled=False, value=Region())

    def stops(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> ImagingResult[Option[float]]:
        """Returns stops to scene gray, or absence without weighted samples."""
        box = self.region.value if self.region.enabled else Region()

        def metered(samples: Samples) -> Option[float]:
            covered = samples.finite
            levels = samples.luminance[covered]
            weights = np.where(samples.central, 1.0, 1.0 - self.centre)[covered]

            def trimmed(found: npt.NDArray[np.float64]) -> Option[float]:
                start = min(math.floor(found.size * min(self.low, self.high) / 100.0), found.size - 1)
                end = max(math.ceil(found.size * max(self.low, self.high) / 100.0), start + 1)
                kept = np.argsort(found, stable=True)[start:end]
                return (
                    Some(weights[kept])
                    .filter(lambda selected: bool(selected.sum(dtype=np.float64) > 0.0))
                    .map(lambda selected: math.log2(GREY) - float(np.average(Samples.logarithm(found[kept]), weights=selected)))
                )

            return Some(levels).filter(lambda found: found.size > 0).bind(trimmed)

        return Samples.of(config, plane[box.window(*plane.shape[:2])]).map(metered)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Extent", "Meter", "Region", "Samples"]
