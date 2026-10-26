"""Scrambled Sobol integration with replicate error, pixel rasters and footprint integrals, and Hermite smoothing."""

from collections.abc import Callable
import math
from typing import Annotated

from expression import Nothing, Option
import msgspec
import numpy as np
import numpy.typing as npt
from scipy.stats import qmc

from blender.imaging.faults import ImagingResult
from blender.imaging.geometry.arithmetic import Field
from blender.records.faults import accumulated, guarded, NumericFault
from blender.records.fields import NonNegative, Seed, Side
from blender.records.identity import content_hash, rng

# --- [TYPES] ----------------------------------------------------------------------------

type Integrand = Callable[[npt.NDArray[np.float64]], ImagingResult[npt.NDArray[np.float64]]]

# --- [MODELS] ---------------------------------------------------------------------------


class Precision(msgspec.Struct, frozen=True, kw_only=True):
    """Scrambled Sobol resolution and independent error replicates."""

    power: Annotated[int, msgspec.Meta(ge=0, le=64, title="Samples Exponent", description="Base-2 exponent of Sobol samples per support and replicate")] = 6
    replicates: Annotated[int, msgspec.Meta(ge=2, description="Scrambles estimating standard error")] = 4
    seed: Annotated[Seed, msgspec.Meta(title="Scramble Seed", description="Number selecting integration scrambles")] = 0
    absolute: Annotated[NonNegative, msgspec.Meta(title="Profile Absolute Tolerance", description="Angular quadrature target in unit-radius profile area")] = 1e-4
    relative: Annotated[NonNegative, msgspec.Meta(title="Profile Relative Tolerance", description="Angular quadrature target over profile area")] = 1e-3

    def points(self, dimensions: int, replicate: int, *, key: tuple[int, ...] = ()) -> npt.NDArray[np.float64]:
        """Returns complete scrambled Sobol net for one replicate and non-negative key."""
        return qmc.Sobol(dimensions, scramble=True, bits=64, rng=rng(content_hash(Precision), self.seed, replicate, *key)).random_base2(self.power)


class Estimate[T](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Estimate with replicate standard error and no exactness certificate."""

    value: T
    standard_error: npt.NDArray[np.float64]
    precision: Precision
    replicates: npt.NDArray[np.float64]
    normalization: Option[tuple[float, float]] = Nothing

    @staticmethod
    def of(replicates: npt.NDArray[np.float64], precision: Precision) -> "Estimate[npt.NDArray[np.float64]]":
        """Returns replicate mean and standard error of the mean."""
        return Estimate(
            value=np.asarray(np.mean(replicates, axis=0)), standard_error=np.asarray(np.std(replicates, axis=0, ddof=1) / math.sqrt(precision.replicates)), precision=precision, replicates=replicates
        )

    def holding[U](self, value: U) -> "Estimate[U]":
        """Returns same statistics over another value."""
        return Estimate(value=value, standard_error=self.standard_error, precision=self.precision, replicates=self.replicates, normalization=self.normalization)


class Raster(msgspec.Struct, frozen=True, kw_only=True):
    """Pixel rows and columns splitting a global rectangle given as left, bottom, right, top, rows top first."""

    bounds: tuple[float, float, float, float]
    shape: tuple[Side, Side]

    @property
    def step(self) -> npt.NDArray[np.float64]:
        """Pixel width and height."""
        left, bottom, right, top = self.bounds
        return np.array(((right - left) / self.shape[1], (top - bottom) / self.shape[0]))

    @property
    def corners(self) -> npt.NDArray[np.float64]:
        """Bottom-left corner of each pixel as `(rows, columns, 2)`."""
        left, _, _, top = self.bounds
        step = self.step
        return np.stack(np.meshgrid(left + np.arange(self.shape[1]) * step[0], top - (np.arange(self.shape[0]) + 1) * step[1]), axis=-1)

    @property
    def centers(self) -> npt.NDArray[np.float64]:
        """Center of each pixel as `(rows, columns, 2)`."""
        return self.corners + self.step / 2.0


# --- [OPERATIONS] -----------------------------------------------------------------------


def integrated(request: msgspec.Struct, field: Integrand, bounds: tuple[tuple[float, ...], tuple[float, ...]], *, precision: Precision) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
    """Estimates a positive rectangular field integral with replicate standard error."""

    def computed() -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
        lower, upper = np.asarray(bounds, dtype=np.float64)
        width = upper - lower
        volume = np.prod(width)
        results = accumulated(
            field(lower + width * precision.points(len(lower), replicate)).map(lambda values: np.asarray(np.mean(values, axis=0) * volume)) for replicate in range(precision.replicates)
        )
        return results.map(lambda values: Estimate.of(np.stack(values), precision))

    return guarded(NumericFault, request, computed)


def footprint(field: Integrand, raster: Raster, precision: Precision) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
    """Estimates the mean of `field`, read at `(points, 2)` global positions as `(points, channels)`, over each pixel's square footprint.

    Returns:
        `(rows, columns, channels)` footprint means with replicate error, or the field's faults.
    """
    corners, step = raster.corners, raster.step
    _, columns = raster.shape

    def replicated(net: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
        return accumulated(field((row[:, None] + net * step).reshape(-1, 2)).map(lambda values: values.reshape(columns, len(net), -1).mean(axis=1)) for row in corners).map(
            lambda rows: np.stack(rows, dtype=np.float64)
        )

    return accumulated(replicated(precision.points(2, replicate)) for replicate in range(precision.replicates)).map(lambda replicates: Estimate.of(np.stack(replicates, dtype=np.float64), precision))


def smoothstep[V: Field](values: V) -> V:
    """Returns cubic Hermite ease of unit-interval values."""
    return values * values * (3.0 - values * 2.0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Estimate", "Integrand", "Precision", "Raster", "footprint", "integrated", "smoothstep"]
