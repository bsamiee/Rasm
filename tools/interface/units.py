"""Unit systems every application shows, imperial by default and metric beside it, with every length in meters."""

from enum import Enum
from typing import Final

# --- [CONSTANTS] ------------------------------------------------------------------------

INCH: Final = 0.0254
FOOT: Final = 12 * INCH
MILLIMETER: Final = 0.001
GRID_THICK_EVERY: Final = 10
GRID_EXTENT: Final = 250 * FOOT
NUDGE: Final = (INCH, INCH / 8, FOOT)

# --- [MODELS] ---------------------------------------------------------------------------


class Units(Enum):
    """One unit system: its model and page length tokens, its temperature token, and its resolution, tolerance, grid, and snap in meters."""

    length: str
    page: str
    temperature: str
    resolution: float
    tolerance: float
    grid: float
    snap: float

    IMPERIAL = ("FEET", "INCHES", "FAHRENHEIT", INCH / 16, 0.001 * FOOT, FOOT, INCH)
    METRIC = ("MILLIMETERS", "MILLIMETERS", "CELSIUS", MILLIMETER, 0.01 * MILLIMETER, 100 * MILLIMETER, 10 * MILLIMETER)

    def __init__(self, length: str, page: str, temperature: str, resolution: float, tolerance: float, grid: float, snap: float) -> None:
        """Bind the member's tuple to its named fields."""
        self.length, self.page, self.temperature = length, page, temperature
        self.resolution, self.tolerance, self.grid, self.snap = resolution, tolerance, grid, snap


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FOOT", "GRID_EXTENT", "GRID_THICK_EVERY", "INCH", "MILLIMETER", "NUDGE", "Units"]
