"""Covered samples, exposure metering, frame white, and absolute PQ peak."""

import math
from typing import Annotated, ClassVar

from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.imaging.chain import GREY
from blender.imaging.color.processors import applied, luma, unassociated
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import Gated, Presentation, Quantity

# --- [MODELS] ---------------------------------------------------------------------------


class Region(msgspec.Struct, frozen=True, kw_only=True):
    """Frame rectangle as bottom-left shares, with y increasing upward."""

    left: Annotated[float, msgspec.Meta(ge=0.0, le=1.0), Presentation(quantity=Some(Quantity.FRAME_WIDTH))] = 0.0
    bottom: Annotated[float, msgspec.Meta(ge=0.0, le=1.0), Presentation(quantity=Some(Quantity.FRAME_HEIGHT))] = 0.0
    right: Annotated[float, msgspec.Meta(ge=0.0, le=1.0), Presentation(quantity=Some(Quantity.FRAME_WIDTH))] = 1.0
    top: Annotated[float, msgspec.Meta(ge=0.0, le=1.0), Presentation(quantity=Some(Quantity.FRAME_HEIGHT))] = 1.0

    def window(self, height: int, width: int) -> tuple["slice[int, int, None]", "slice[int, int, None]"]:
        """Returns row and column slices of a top-first plane."""
        return slice(round((1.0 - self.top) * height), round((1.0 - self.bottom) * height)), slice(round(self.left * width), round(self.right * width))


class Extent(msgspec.Struct, frozen=True, kw_only=True):
    """Least covered working luminance and percentile frame white."""

    percentile: ClassVar[float] = 99.995
    low: float
    white: float


class Samples(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Straight RGB, float64 working luminance, and pixel coverage."""

    floor: ClassVar[float] = 1e-5
    color: npt.NDArray[np.float32]
    luminance: npt.NDArray[np.float64]
    covered: npt.NDArray[np.bool]

    @classmethod
    def logarithm(cls, values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        """Returns base-two luminance with the measured black floor."""
        logarithms = np.log2(np.maximum(values, cls.floor))
        logarithms.setflags(write=False)
        return logarithms

    @property
    def finite(self) -> npt.NDArray[np.bool]:
        """Mask of covered pixels with finite working luminance."""
        mask = self.covered & np.isfinite(self.luminance)
        mask.setflags(write=False)
        return mask

    @property
    def central(self) -> npt.NDArray[np.bool]:
        """Mask of the middle half along both frame axes."""
        height, width = self.covered.shape
        rows, columns = np.arange(height, dtype=np.float64)[:, None], np.arange(width, dtype=np.float64)[None, :]
        mask = np.asarray((np.abs(rows + 0.5 - height / 2.0) < height / 4.0) & (np.abs(columns + 0.5 - width / 2.0) < width / 4.0), dtype=np.bool)
        mask.setflags(write=False)
        return mask

    def extent(self) -> Option[Extent]:
        """Returns minimum and observed percentile white, or absence without coverage."""
        return (
            Some(self.luminance[self.finite])
            .filter(lambda found: found.size > 0)
            .map(lambda found: Extent(low=float(found.min()), white=float(np.percentile(found, Extent.percentile, method="inverted_cdf"))))
        )

    def percentile(self, rank: Annotated[float, msgspec.Meta(ge=0.0, le=100.0)]) -> Option[float]:
        """Returns an observed covered finite luminance at the requested percentile."""
        return Some(self.luminance[self.finite]).filter(lambda found: found.size > 0).map(lambda found: float(np.percentile(found, rank, method="inverted_cdf")))

    def log_statistics(self) -> Option[tuple[float, float, float]]:
        """Returns mean, maximum, and minimum finite covered log luminance."""
        return Some(self.logarithm(self.luminance[self.finite])).filter(lambda found: found.size > 0).map(lambda found: (float(found.mean(dtype=np.float64)), float(found.max()), float(found.min())))


def samples_of(config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> ImagingResult[Samples]:
    """Reads associated RGB or RGBA without counting uncovered pixels as black."""

    def sampled(coefficients: npt.NDArray[np.float32]) -> Samples:
        straight = unassociated(plane)
        covered = (plane[..., 3:] > 0.0).all(axis=-1)
        luminance = np.where(covered, np.matmul(straight[..., :3], coefficients, dtype=np.float64), 0.0)
        for array in (straight, luminance, covered):
            array.setflags(write=False)
        return Samples(color=straight[..., :3], luminance=luminance, covered=covered)

    return luma(config).map(sampled)


class Meter(msgspec.Struct, frozen=True, kw_only=True):
    """Percentile trims, central weighting, and region of an exposure reading."""

    low: Annotated[float, msgspec.Meta(ge=0.0, le=100.0, title="Low Trim"), Presentation(step=Some(1.0), decimals=Some(1))] = 0.0
    high: Annotated[float, msgspec.Meta(ge=0.0, le=100.0, title="High Trim"), Presentation(step=Some(1.0), decimals=Some(1))] = 100.0
    centre: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Centre Weight")] = 0.0
    region: Gated[Region] = Gated(enabled=False, value=Region())

    def stops(self, config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> ImagingResult[Option[float]]:
        """Returns stops to scene gray, or absence when covered samples have no weight."""
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

        return samples_of(config, plane[box.window(*plane.shape[:2])]).map(metered)


# --- [OPERATIONS] -----------------------------------------------------------------------


def peak(samples: Samples) -> Option[float]:
    """Returns the observed percentile PQ peak in cd/m², or absence without coverage."""
    decoder = PyOpenColorIO.Config().getProcessor(PyOpenColorIO.BuiltinTransform(style="CURVE - LINEAR_to_ST-2084"), PyOpenColorIO.TRANSFORM_DIR_INVERSE).getDefaultCPUProcessor()
    greatest = samples.color.max(axis=-1)
    return (
        Some(greatest[samples.covered & np.isfinite(greatest)].astype(np.float64))
        .filter(lambda found: found.size > 0)
        .map(lambda found: float(applied(decoder, np.full((1, 3), np.clip(np.percentile(found, Extent.percentile, method="inverted_cdf"), 0.0, 1.0), np.float32))[0, 0]) * 100.0)
    )


def sample(plane: npt.NDArray[np.float32], position: tuple[int, int], radius: Annotated[int, msgspec.Meta(ge=0)] = 0) -> Option[npt.NDArray[np.float64]]:
    """Returns the covered channel mean around a top-first (column, row) pixel."""
    column, row = position
    height, width = plane.shape[:2]
    square = plane[max(0, min(height, row - radius)) : max(0, min(height, row + radius + 1)), max(0, min(width, column - radius)) : max(0, min(width, column + radius + 1))]
    mean = Some(square[(square[..., 3:] > 0.0).all(axis=-1)]).filter(lambda found: found.shape[0] > 0).map(lambda found: found.mean(axis=0, dtype=np.float64))
    for array in mean.to_list():
        array.setflags(write=False)
    return mean


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Extent", "Meter", "Region", "Samples", "peak", "sample", "samples_of"]
