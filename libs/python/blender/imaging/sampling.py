"""Scrambled Sobol integration with replicate error, Hermite smoothing, and read-only array records."""

from collections.abc import Callable
import math
from typing import Annotated

from expression import Nothing, Option
import msgspec
import numpy as np
from numpy.polynomial import Polynomial
import numpy.typing as npt
from scipy.stats import qmc

from blender.imaging.faults import accumulated, guarded, ImagingNumericFault, ImagingResult
from blender.imaging.hashing import content_hash, rng
from blender.imaging.parameters import NonNegative, Seed

# --- [MODELS] ---------------------------------------------------------------------------


class Frozen(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Record whose array fields, held directly or in an option, are read-only from construction."""

    def __post_init__(self) -> None:
        """Marks every array field read-only."""
        for field in msgspec.structs.fields(self):
            match getattr(self, field.name):
                case np.ndarray() as array:
                    array.setflags(write=False)
                case Option(tag="some", some=np.ndarray() as array):
                    array.setflags(write=False)
                case _:
                    pass


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


# --- [OPERATIONS] -----------------------------------------------------------------------


def integrated(
    request: msgspec.Struct, field: Callable[[npt.NDArray[np.float64]], ImagingResult[npt.NDArray[np.float64]]], bounds: tuple[tuple[float, ...], tuple[float, ...]], *, precision: Precision
) -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
    """Estimates a positive rectangular field integral with replicate standard error."""

    def computed() -> ImagingResult[Estimate[npt.NDArray[np.float64]]]:
        lower, upper = np.asarray(bounds, dtype=np.float64)
        width = upper - lower
        volume = np.prod(width)
        results = accumulated(
            field(lower + width * precision.points(len(lower), replicate)).map(lambda values: np.asarray(np.mean(values, axis=0) * volume)) for replicate in range(precision.replicates)
        )
        return results.map(lambda values: Estimate.of(np.stack(values), precision))

    return guarded(ImagingNumericFault, request, computed)


def smoothstep[V: (npt.NDArray[np.float64], Polynomial)](values: V) -> V:
    """Returns cubic Hermite ease of unit-interval values or of a polynomial over them."""
    return values * values * (3.0 - 2.0 * values)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Estimate", "Frozen", "Precision", "integrated", "smoothstep"]
