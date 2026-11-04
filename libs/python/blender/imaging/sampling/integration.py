"""Scrambled Sobol integration with replicate error, pixel footprint integrals, and soft disc populations splatted with occlusion into rasters."""

from collections.abc import Callable, Sequence
from functools import partial
import math
import sys
from typing import Annotated, Generic, TypeVar

from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
from optype import CanAdd
from scipy.stats import qmc, sem

from blender.imaging.geometry.arithmetic import Arithmetic, smoothstep
from blender.imaging.geometry.frames import Raster, Region
from blender.imaging.sampling.tiles import uniforms
from blender.records.faults import accumulated, Fault, Outcome, Reason
from blender.records.fields import NonNegative, Positive, Seed, Unit
from blender.records.stores import content_hash, rng

# --- [TYPES] ----------------------------------------------------------------------------

_T_co = TypeVar("_T_co", covariant=True)
type _Integrand = Callable[[npt.NDArray[np.float64]], Outcome[npt.NDArray[np.float64]]]

# --- [MODELS] ---------------------------------------------------------------------------


class Precision(msgspec.Struct, frozen=True, kw_only=True):
    """Scrambled Sobol resolution and independent error replicates."""

    power: Annotated[int, msgspec.Meta(ge=0, le=64)] = 6
    replicates: Annotated[int, msgspec.Meta(ge=2)] = 4
    seed: Seed = 0
    absolute: NonNegative = 1e-4
    relative: NonNegative = 1e-3

    def points(self, dimensions: int, replicate: int, *, key: tuple[int, ...] = ()) -> npt.NDArray[np.float64]:
        """Returns complete scrambled Sobol net for one replicate and non-negative key."""
        return qmc.Sobol(dimensions, scramble=True, bits=64, rng=rng(content_hash(Precision), self.seed, replicate, *key)).random_base2(self.power)


class Estimate(msgspec.Struct, Generic[_T_co], frozen=True, kw_only=True, eq=False):  # ruff: ignore[non-pep695-generic-class]
    """Estimated value with replicate standard error."""

    value: _T_co
    standard_error: npt.NDArray[np.float64]
    precision: Precision
    replicates: npt.NDArray[np.float64]

    @staticmethod
    def of(replicates: npt.NDArray[np.float64], precision: Precision) -> "Estimate[npt.NDArray[np.float64]]":
        """Returns replicate mean and standard error of the mean."""
        return Estimate(value=np.asarray(np.mean(replicates, axis=0)), standard_error=np.asarray(sem(replicates, axis=0)), precision=precision, replicates=replicates)

    def holding[U](self, value: U) -> "Estimate[U]":
        """Returns same statistics over another value."""
        return Estimate(value=value, standard_error=self.standard_error, precision=self.precision, replicates=self.replicates)


class Disc(msgspec.Struct, frozen=True, kw_only=True):
    """Stable population member with global center and physical radius."""

    cell: tuple[int, ...]
    center: tuple[float, float]
    radius: float
    gain: float
    tone: float


class Population(msgspec.Struct, frozen=True, kw_only=True):
    """Soft discs in square cells with presence, size, fade, and edge, each cell drawing stable uniforms from a draw stream."""

    spacing: Annotated[Positive, msgspec.Meta(description="Cell width")] = 0.01
    density: Annotated[Unit, msgspec.Meta(description="Share of cells holding a disc")] = 0.5
    size: Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=0.5, description="Disc diameter over cell width")] = 0.2
    variation: Annotated[Unit, msgspec.Meta(description="Diameter shrink share below size")] = 0.5
    spread: Annotated[NonNegative, msgspec.Meta(description="Per-disc gain drop in stops")] = 1.0
    softness: Annotated[Unit, msgspec.Meta(description="Edge fade over radius before pixel filtering")] = 0.5

    def covering(self, bounds: Region) -> npt.NDArray[np.int64]:
        """Returns cells with discs able to intersect global bounds, rows bottom first."""
        radius = self.size / 2.0
        columns = np.arange(math.floor(bounds.left / self.spacing - radius) - 1, math.floor(bounds.right / self.spacing + radius) + 1, dtype=np.int64)
        rows = np.arange(math.floor(bounds.bottom / self.spacing - radius) - 1, math.floor(bounds.top / self.spacing + radius) + 1, dtype=np.int64)
        return np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)

    def occupied(self, cells: npt.NDArray[np.int64], stream: int) -> tuple[npt.NDArray[np.int64], npt.NDArray[np.float64]]:
        """Returns the cells holding a disc with their six uniforms: presence, size, gain, tone, and two center offsets."""
        values = uniforms(cells, stream, 6)
        kept = values[:, 0] < self.density
        return cells[kept], values[kept]

    def placed[V: Arithmetic](self, cell: tuple[CanAdd[float, V], CanAdd[float, V]], draws: Sequence[V], exp2: Callable[[V], V], *, spacing: float, scale: V | float = 1.0) -> tuple[V, V, V, V]:
        """Returns the center abscissa and ordinate, radius, and peak gain of an occupied cell from its six uniforms at lattice `spacing` and depth `scale`."""
        return placement(cell, draws, exp2, spacing=spacing, diameter=self.spacing * self.size, variation=self.variation, spread=self.spread, scale=scale)

    def discs(self, bounds: Region, stream: int = 0) -> tuple[Disc, ...]:
        """Returns occupied members of cells covering global bounds, in cell order."""
        cells, values = self.occupied(self.covering(bounds), stream)
        centers, radii, gains = self._placements(cells, values)
        return tuple(
            Disc(cell=tuple(cell), center=(center[0], center[1]), radius=radius, gain=gain, tone=tone)
            for cell, center, radius, gain, tone in zip(cells.tolist(), centers.tolist(), radii.tolist(), gains.tolist(), values[:, 3].tolist())
        )

    def integrated(self, raster: Raster, *, precision: Precision, stream: int = 0, paint: Callable[[float], float] = float) -> Outcome[Estimate[npt.NDArray[np.float64]]]:
        """Estimates mean disc coverage and covered `paint` tone over each pixel of `raster`, each disc's Sobol net rotated by its own draws.

        Returns:
            `(rows, columns, 2)` coverage and covered paint, or `FAILED` fault.
        """

        @catch(exception=FloatingPointError)
        @np.errstate(over="raise", divide="raise", invalid="raise")
        def computed() -> Estimate[npt.NDArray[np.float64]]:
            cells, values = self.occupied(self.covering(raster.bounds), stream)
            centers, radii, gains = self._placements(cells, values)
            paints = np.column_stack((np.ones(len(values)), np.array([paint(tone) for tone in values[:, 3].tolist()], dtype=np.float64).reshape(-1)))
            step, (height, width) = raster.step, raster.shape
            area = float(np.prod(step))

            def replicated(replicate: int) -> npt.NDArray[np.float64]:
                rotation = uniforms(np.column_stack((cells, np.full((len(cells), 2), (precision.seed, replicate)))), stream, 2)
                net = (precision.points(2, replicate) + rotation[:, None]) % 1.0
                radial, angle = np.sqrt(net[..., 0]), math.tau * net[..., 1]
                points = (centers[:, None] + radii[:, None, None] * radial[..., None] * np.stack((np.cos(angle), np.sin(angle)), axis=-1)).reshape(-1, 2)
                profile = np.ones_like(radial) if self.softness == 0.0 else smoothstep(np.minimum((1.0 - radial) / self.softness, 1.0))
                masses = ((profile * (math.pi * radii**2 * gains / (area * net.shape[1]))[:, None])[..., None] * paints[:, None]).reshape(-1, 2)
                columns = np.floor((points[:, 0] - raster.bounds.left) / step[0]).astype(np.intp)
                rows = height - 1 - np.floor((points[:, 1] - raster.bounds.bottom) / step[1]).astype(np.intp)
                inside = (columns >= 0) & (columns < width) & (rows >= 0) & (rows < height)
                sums = np.zeros((height * width, 2), dtype=np.float64)
                np.add.at(sums, (rows * width + columns)[inside], masses[inside])
                return sums.reshape(height, width, 2)

            return Estimate.of(np.stack([replicated(replicate) for replicate in range(precision.replicates)]), precision)

        return computed().map_error(partial(Fault.of, Reason.FAILED, repr(self)))

    def _placements(self, cells: npt.NDArray[np.int64], values: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64], npt.NDArray[np.float64]]:
        """Returns `(discs, 2)` centers, radii, and peak gains of occupied cells and their uniforms at the population's own spacing."""
        column, row = cells[:, :2].T
        across, up, radii, gains = self.placed((column, row), tuple(values.T), np.exp2, spacing=self.spacing)
        return np.column_stack((across, up)), radii, gains


# --- [OPERATIONS] -----------------------------------------------------------------------


def placement[V: Arithmetic](
    cell: tuple[CanAdd[float, V], CanAdd[float, V]],
    draws: Sequence[V],
    exp2: Callable[[V], V],
    *,
    spacing: V | float,
    diameter: V | float,
    variation: V | float,
    spread: V | float,
    scale: V | float = 1.0,
) -> tuple[V, V, V, V]:
    """Returns the center abscissa and ordinate, radius, and peak gain of an occupied cell from its six uniforms.

    Args:
        cell: Lattice column and row.
        draws: Six uniforms of the cell: presence, size, gain, tone, and two center offsets.
        exp2: Exponentiation over the operands.
        spacing: Lattice spacing.
        diameter: Largest disc diameter at unit scale.
        variation: Diameter shrink share below the largest.
        spread: Gain drop in stops at the largest draw.
        scale: Depth scale of the radius.

    Returns:
        Center abscissa, center ordinate, radius, and peak gain.
    """
    column, row = cell
    _, size, gain, _, across, up = draws
    return (column + 0.25 + across * 0.5) * spacing, (row + 0.25 + up * 0.5) * spacing, (1.0 - size * variation) * scale * (diameter * 0.5), exp2(gain * spread * -1.0)


def integrated(request: msgspec.Struct, field: _Integrand, bounds: tuple[tuple[float, ...], tuple[float, ...]], *, precision: Precision) -> Outcome[Estimate[npt.NDArray[np.float64]]]:
    """Estimates a positive rectangular field integral with replicate standard error."""
    lower, upper = np.asarray(bounds, dtype=np.float64)
    width = upper - lower
    volume = np.prod(width)

    @catch(exception=FloatingPointError)
    @np.errstate(over="raise", divide="raise", invalid="raise")
    def averaged(values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
        return np.asarray(np.mean(values, axis=0) * volume)

    return accumulated(
        field(lower + width * precision.points(len(lower), replicate)).bind(lambda values: averaged(values).map_error(partial(Fault.of, Reason.FAILED, repr(request))))
        for replicate in range(precision.replicates)
    ).map(lambda values: Estimate.of(np.stack(values), precision))


def footprint(field: _Integrand, raster: Raster, precision: Precision) -> Outcome[Estimate[npt.NDArray[np.float64]]]:
    """Estimates the mean of `field`, read at `(points, 2)` global positions as `(points, channels)`, over each pixel's square footprint.

    Returns:
        `(rows, columns, channels)` footprint means with replicate error, or the field's faults.
    """
    corners, step = raster.corners, raster.step
    _, columns = raster.shape

    def replicated(net: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float64]]:
        return accumulated(field((row[:, None] + net * step).reshape(-1, 2)).map(lambda values: values.reshape(columns, len(net), -1).mean(axis=1)) for row in corners).map(
            lambda rows: np.stack(rows, dtype=np.float64)
        )

    return accumulated(replicated(precision.points(2, replicate)) for replicate in range(precision.replicates)).map(lambda replicates: Estimate.of(np.stack(replicates, dtype=np.float64), precision))


def transmitted(gains: npt.NDArray[np.float64], coverage: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns light passing every nearer disc, the product over `(discs, points)` coverage of one less each disc's gain-weighted coverage."""
    return np.asarray(np.prod(1.0 - gains[:, None] * coverage, axis=0), dtype=np.float64)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Disc", "Estimate", "Population", "Precision", "footprint", "integrated", "placement", "transmitted"]
