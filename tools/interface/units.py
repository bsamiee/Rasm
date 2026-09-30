# ruff: file-ignore[banned-api]
"""Unit systems every application shows, imperial by default and metric beside it, every length in meters."""

from enum import Enum
from fractions import Fraction
from typing import Final, NamedTuple

# --- [TYPES] ----------------------------------------------------------------------------


class Length(float, Enum):
    """Length units in meters."""

    INCHES = 0.0254
    FEET = 12 * INCHES
    MILLIMETERS = 0.001
    POINTS = INCHES / 72


class Pen(float, Enum):
    """Plotted line widths on the sheet in meters, by their NCS weight."""

    FINE = 0.18 * Length.MILLIMETERS
    THIN = 0.25 * Length.MILLIMETERS
    MEDIUM = 0.35 * Length.MILLIMETERS


# --- [CONSTANTS] ------------------------------------------------------------------------

GRID_THICK_EVERY: Final = 10
ANGLE_STEP: Final = 15
ANGLE_PRECISION: Final = 0

# --- [MODELS] ---------------------------------------------------------------------------


class Standard(NamedTuple):
    """Unit system's model and page units, sheet scale denominator, and lengths in meters, the text cap height, sheet and document sizes, and margin on paper."""

    length: Length
    page: Length
    resolution: float
    grid: float
    snap: float
    extent: float
    sheet_scale: int
    text: float
    paper: tuple[float, float]
    document: tuple[float, float]
    margin: float


class Units(Standard, Enum):
    """Unit systems by the name applications store for them."""

    IMPERIAL = Standard(
        length=Length.FEET,
        page=Length.INCHES,
        resolution=Length.INCHES / 16,
        grid=Length.FEET,
        snap=Length.INCHES,
        extent=250 * Length.FEET,
        sheet_scale=48,
        text=3 * Length.INCHES / 32,
        paper=(36 * Length.INCHES, 24 * Length.INCHES),
        document=(8.5 * Length.INCHES, 11 * Length.INCHES),
        margin=Length.INCHES / 2,
    )
    METRIC = Standard(
        length=Length.MILLIMETERS,
        page=Length.MILLIMETERS,
        resolution=Length.MILLIMETERS,
        grid=100 * Length.MILLIMETERS,
        snap=10 * Length.MILLIMETERS,
        extent=75.0,
        sheet_scale=50,
        text=2.5 * Length.MILLIMETERS,
        paper=(841 * Length.MILLIMETERS, 594 * Length.MILLIMETERS),
        document=(210 * Length.MILLIMETERS, 297 * Length.MILLIMETERS),
        margin=13 * Length.MILLIMETERS,
    )

    @property
    def tolerance(self) -> float:
        """Modeling tolerance in meters."""
        return self.resolution / 16

    @property
    def nudge(self) -> tuple[float, float, float]:
        """Nudge steps in meters."""
        return (self.snap, 2 * self.resolution, self.grid)

    @property
    def grid_lines(self) -> int:
        """Grid lines from the origin to the modeling extent."""
        return round(self.extent / self.grid)

    @property
    def far(self) -> float:
        """Far clipping distance in meters."""
        return 12 * self.extent

    @property
    def first_offset(self) -> float:
        """Paper distance from the object to the first dimension string."""
        return 6 * self.text

    @property
    def offset(self) -> float:
        """Paper gap between the object and an extension line."""
        return self.resolution

    @property
    def extension(self) -> float:
        """Paper length an extension line runs past its dimension line."""
        return 2 * self.resolution

    def places(self, unit: Length, base: int) -> int:
        """Fewest digits in the base stating the resolution in the unit exactly."""
        ratio = Fraction(self.resolution / unit).limit_denominator()
        return next(digits for digits in range(ratio.denominator.bit_length()) if (ratio * base**digits).denominator == 1)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ANGLE_PRECISION", "ANGLE_STEP", "GRID_THICK_EVERY", "Length", "Pen", "Units"]
