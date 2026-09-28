# ruff: file-ignore[banned-api]
"""Unit systems every application shows, imperial by default and metric beside it, every length in meters."""

from enum import Enum
from typing import Final, NamedTuple

# --- [CONSTANTS] ------------------------------------------------------------------------

INCH: Final = 0.0254
FOOT: Final = 12 * INCH
MILLIMETER: Final = 0.001
POINT: Final = INCH / 72
GRID_THICK_EVERY: Final = 10
ANGLE_STEP: Final = 15
ANGLE_PRECISION: Final = 0

# --- [MODELS] ---------------------------------------------------------------------------


class Pen(float, Enum):
    """Plotted line widths on the sheet in meters, by their NCS weight."""

    FINE = 0.18 * MILLIMETER
    THIN = 0.25 * MILLIMETER
    MEDIUM = 0.35 * MILLIMETER


class Standard(NamedTuple):
    """Unit system's tokens, sheet scale denominator, and lengths in meters, the page unit, text cap height, sheet and document sizes, and margin on paper."""

    length: str
    page: str
    page_unit: float
    mass: str
    temperature: str
    separate: bool
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
        length="FEET",
        page="INCHES",
        page_unit=INCH,
        mass="POUNDS",
        temperature="FAHRENHEIT",
        separate=True,
        resolution=INCH / 16,
        grid=FOOT,
        snap=INCH,
        extent=250 * FOOT,
        sheet_scale=48,
        text=3 * INCH / 32,
        paper=(36 * INCH, 24 * INCH),
        document=(8.5 * INCH, 11 * INCH),
        margin=INCH / 2,
    )
    METRIC = Standard(
        length="MILLIMETERS",
        page="MILLIMETERS",
        page_unit=MILLIMETER,
        mass="KILOGRAMS",
        temperature="CELSIUS",
        separate=False,
        resolution=MILLIMETER,
        grid=100 * MILLIMETER,
        snap=10 * MILLIMETER,
        extent=75.0,
        sheet_scale=50,
        text=2.5 * MILLIMETER,
        paper=(841 * MILLIMETER, 594 * MILLIMETER),
        document=(210 * MILLIMETER, 297 * MILLIMETER),
        margin=13 * MILLIMETER,
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ANGLE_PRECISION", "ANGLE_STEP", "FOOT", "GRID_THICK_EVERY", "INCH", "MILLIMETER", "POINT", "Pen", "Units"]
