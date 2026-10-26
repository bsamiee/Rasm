"""Soft disc populations over integer cells, their pixel coverage integrals, and splatting with occlusion into rasters."""

from collections.abc import Callable
import math
from typing import Annotated

from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.imaging.sampling.integration import Estimate, Precision, Raster, smoothstep
from blender.records.faults import Fault, Outcome, RecordReason
from blender.records.fields import NonNegative, Positive, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Draws = Callable[[npt.NDArray[np.int64], int, int], npt.NDArray[np.float64]]

# --- [MODELS] ---------------------------------------------------------------------------


class Mark(msgspec.Struct, frozen=True, kw_only=True):
    """Stable population member with global center and physical radius."""

    cell: tuple[int, ...]
    center: tuple[float, float]
    radius: float
    gain: float
    tone: float


class Population(msgspec.Struct, frozen=True, kw_only=True):
    """Soft discs in square cells with presence, size, fade, and edge, each cell drawing stable uniforms from caller `Draws` of (keys, stream, count)."""

    spacing: Annotated[Positive, msgspec.Meta(description="Cell width at nearest layer over greater frame side")] = 0.01
    density: Annotated[Unit, msgspec.Meta(description="Share of cells holding a particle")] = 0.5
    size: Annotated[float, msgspec.Meta(gt=0.0, le=0.5, description="Particle diameter over cell width")] = 0.2
    variation: Annotated[Unit, msgspec.Meta(description="Diameter shrink share below size")] = 0.5
    spread: Annotated[NonNegative, msgspec.Meta(description="Per-particle coverage drop in stops")] = 1.0
    softness: Annotated[Unit, msgspec.Meta(description="Edge fade over radius before pixel filtering")] = 0.5

    def covering(self, bounds: tuple[float, float, float, float]) -> npt.NDArray[np.int64]:
        """Returns cells with discs able to intersect global bounds, rows bottom first."""
        left, bottom, right, top = bounds
        radius = self.size / 2.0
        columns = np.arange(math.floor(left / self.spacing - radius) - 1, math.floor(right / self.spacing + radius) + 1)
        rows = np.arange(math.floor(bottom / self.spacing - radius) - 1, math.floor(top / self.spacing + radius) + 1)
        return np.stack(np.meshgrid(columns, rows), axis=-1).reshape(-1, 2)

    def occupied(self, cells: npt.NDArray[np.int64], draws: Draws, stream: int) -> tuple[npt.NDArray[np.int64], npt.NDArray[np.float64]]:
        """Returns the cells holding a disc with their six uniforms: presence, size, gain, tone, and two center offsets."""
        values = draws(cells, stream, 6)
        kept = values[:, 0] < self.density
        return cells[kept], values[kept]

    def placed(
        self, cells: npt.NDArray[np.int64], values: npt.NDArray[np.float64], spacing: float, scale: float | npt.NDArray[np.float64] = 1.0
    ) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64], npt.NDArray[np.float64]]:
        """Returns centers, radii, and peak gains of occupied cells from their uniforms at lattice spacing and depth scale."""
        return (cells[:, :2] + 0.25 + 0.5 * values[:, 4:6]) * spacing, self.spacing * scale * self.size * (1.0 - self.variation * values[:, 1]) / 2.0, np.exp2(-self.spread * values[:, 2])

    def marks(self, bounds: tuple[float, float, float, float], draws: Draws, stream: int = 0) -> tuple[Mark, ...]:
        """Returns occupied members of cells covering global bounds, in cell order."""
        cells, values = self.occupied(self.covering(bounds), draws, stream)
        centers, radii, gains = self.placed(cells, values, self.spacing)
        return tuple(
            Mark(cell=tuple(cell), center=(center[0], center[1]), radius=radius, gain=gain, tone=tone)
            for cell, center, radius, gain, tone in zip(cells.tolist(), centers.tolist(), radii.tolist(), gains.tolist(), values[:, 3].tolist())
        )

    def integrated(self, raster: Raster, draws: Draws, *, precision: Precision, stream: int = 0, paint: Callable[[float], float] = float) -> Outcome[Estimate[npt.NDArray[np.float64]]]:
        """Estimates mean disc coverage and covered tone over each pixel of `raster`.

        Args:
            raster: Global rectangle and its pixel grid.
            draws: Stable uniforms per cell key.
            precision: Sobol resolution and independent error replicates, each disc's net rotated by its own draws.
            stream: Independent population under one draw source.
            paint: Tone weight of covered ink.

        Returns:
            `(rows, columns, 2)` coverage and covered paint, or numeric refusal.
        """

        @np.errstate(over="raise", divide="raise", invalid="raise")
        def computed() -> Estimate[npt.NDArray[np.float64]]:
            cells, values = self.occupied(self.covering(raster.bounds), draws, stream)
            centers, radii, gains = self.placed(cells, values, self.spacing)
            paints = np.column_stack((np.ones(len(values)), np.array([paint(tone) for tone in values[:, 3].tolist()], dtype=np.float64).reshape(-1)))
            area = float(np.prod(raster.step))

            def replicated(replicate: int) -> npt.NDArray[np.float64]:
                rotation = draws(np.column_stack((cells, np.full((len(cells), 2), (precision.seed, replicate)))), stream, 2)
                net = (precision.points(2, replicate) + rotation[:, None]) % 1.0
                radial, angle = np.sqrt(net[..., 0]), math.tau * net[..., 1]
                points = centers[:, None] + radii[:, None, None] * radial[..., None] * np.stack((np.cos(angle), np.sin(angle)), axis=-1)
                profile = np.ones_like(radial) if self.softness == 0.0 else smoothstep(np.minimum((1.0 - radial) / self.softness, 1.0))
                mass = profile * (math.pi * radii**2 * gains / (area * net.shape[1]))[:, None]
                return deposited(raster, points.reshape(-1, 2), (mass[..., None] * paints[:, None]).reshape(-1, 2))

            return Estimate.of(np.stack([replicated(replicate) for replicate in range(precision.replicates)]), precision)

        return catch(computed, exception=FloatingPointError)().map_error(lambda error: (Fault(reason=RecordReason.NUMERIC, subject=repr(self), cause=error),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def deposited(raster: Raster, points: npt.NDArray[np.float64], values: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns the `(rows, columns, channels)` sums of `(points, channels)` values landing in each pixel of `raster`, points outside dropped."""
    left, bottom, _, _ = raster.bounds
    height, width = raster.shape
    step = raster.step
    columns = np.floor((points[:, 0] - left) / step[0]).astype(np.intp)
    rows = height - 1 - np.floor((points[:, 1] - bottom) / step[1]).astype(np.intp)
    inside = (columns >= 0) & (columns < width) & (rows >= 0) & (rows < height)
    sums = np.zeros((height * width, values.shape[-1]), dtype=np.float64)
    np.add.at(sums, (rows * width + columns)[inside], values[inside])
    return sums.reshape(height, width, -1)


def transmitted(gains: npt.NDArray[np.float64], coverage: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
    """Returns light passing every nearer disc, the product over `(discs, points)` coverage of one less each disc's gain-weighted coverage."""
    return np.prod(1.0 - gains[:, None] * coverage, axis=0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Draws", "Mark", "Population", "deposited", "transmitted"]
