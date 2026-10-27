"""Covered samples, exposure metering, frame white, and absolute PQ peak."""

import math
from typing import Annotated, ClassVar, Self

from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.color.processors import converted, GREY, luma
from blender.imaging.files.buffers import unassociated
from blender.imaging.geometry.frames import Region
from blender.records.faults import Outcome
from blender.records.fields import Frozen, Unit

# --- [MODELS] ---------------------------------------------------------------------------


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
    def of(cls, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> Outcome[Self]:
        """Reads associated RGB or RGBA as straight samples."""
        straight, covered = unassociated(plane)[..., :3], np.asarray((plane[..., 3:] > 0.0).all(axis=-1), dtype=np.bool)
        return luma(config).map(lambda coefficients: cls(color=straight, luminance=np.where(covered, np.matmul(straight, coefficients, dtype=np.float64), 0.0), covered=covered))

    @classmethod
    def logarithm(cls, values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Returns log2 of values clamped to `floor`."""
        return np.asarray(np.log2(np.maximum(values, cls.floor)), dtype=np.float64)

    @property
    def finite(self) -> npt.NDArray[np.bool]:
        """Covered pixels with finite luminance."""
        return np.asarray(self.covered & np.isfinite(self.luminance), dtype=np.bool)

    @property
    def central(self) -> npt.NDArray[np.bool]:
        """Middle half along both frame axes."""
        height, width = self.covered.shape
        rows, columns = np.arange(height, dtype=np.float64)[:, None], np.arange(width, dtype=np.float64)[None, :]
        return np.asarray((np.abs(rows + 0.5 - height / 2.0) < height / 4.0) & (np.abs(columns + 0.5 - width / 2.0) < width / 4.0), dtype=np.bool)

    def peak(self) -> Outcome[Option[float]]:
        """Returns covered percentile PQ peak in cd/m², decoded through the OCIO ST-2084 curve, absent without covered samples."""
        if not (found := (greatest := self.color.max(axis=-1))[self.covered & np.isfinite(greatest)]).size:
            return Ok(Nothing)
        code = np.full((1, 3), np.clip(np.percentile(found, Extent.percentile, method="inverted_cdf"), 0.0, 1.0), dtype=np.float32)
        return converted(PyOpenColorIO.Config(), PyOpenColorIO.BuiltinTransform(style="CURVE - ST-2084_to_LINEAR"), code).map(lambda linear: Some(100.0 * float(linear[0, 0])))

    def percentile(self, rank: float) -> Option[float]:
        """Returns covered luminance at `rank` percentile."""
        return Some(self.luminance[self.finite]).filter(lambda found: found.size > 0).map(lambda found: float(np.percentile(found, rank, method="inverted_cdf")))

    def extent(self) -> Option[Extent]:
        """Returns covered luminance extent."""
        return self.percentile(Extent.percentile).map(lambda white: Extent(low=float(self.luminance[self.finite].min()), white=white))

    def log_statistics(self) -> Option[tuple[float, float, float]]:
        """Returns mean, maximum, and minimum covered log luminance."""
        return Some(self.logarithm(self.luminance[self.finite])).filter(lambda found: found.size > 0).map(lambda found: (float(found.mean(dtype=np.float64)), float(found.max()), float(found.min())))


class Meter(msgspec.Struct, frozen=True, kw_only=True):
    """Exposure percentile trims, center weight, and region, the whole frame by default."""

    low: Annotated[float, msgspec.Meta(ge=0.0, le=100.0)] = 0.0
    high: Annotated[float, msgspec.Meta(ge=0.0, le=100.0)] = 100.0
    centre: Unit = 0.0
    region: Region = Region()

    def stops(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> Outcome[Option[float]]:
        """Returns stops to scene gray, or absence without weighted samples."""

        def metered(samples: Samples) -> Option[float]:
            covered = samples.finite
            found, weights = samples.luminance[covered], np.where(samples.central, 1.0, 1.0 - self.centre)[covered]
            start = min(math.floor(found.size * min(self.low, self.high) / 100.0), found.size - 1)
            kept = np.argsort(found, stable=True)[start : max(math.ceil(found.size * max(self.low, self.high) / 100.0), start + 1)]
            return (
                Some(weights[kept])
                .filter(lambda selected: bool(selected.sum(dtype=np.float64) > 0.0))
                .map(lambda selected: math.log2(GREY) - float(np.average(Samples.logarithm(found[kept]), weights=selected)))
            )

        return Samples.of(config, plane[self.region.window(*plane.shape[:2])]).map(metered)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Extent", "Meter", "Samples"]
