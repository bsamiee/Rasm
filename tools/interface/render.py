"""Render settings, site and its survey rasters, sun moment, and shared asset folders every application renders a scene with."""

from datetime import datetime, timedelta, UTC
from enum import Enum
from pathlib import Path
from typing import Final
from urllib.parse import urlencode
from zoneinfo import ZoneInfo

# --- [CONSTANTS] ------------------------------------------------------------------------

FRAME_SIZE: Final = (1920, 1080)
SAMPLES: Final = 1024
NOISE_THRESHOLD: Final = 0.01
MAX_BOUNCES: Final = 12
DIFFUSE_BOUNCES: Final = 4
GLOSSY_BOUNCES: Final = 4
TRANSMISSION_BOUNCES: Final = 12
VOLUME_BOUNCES: Final = 0
TRANSPARENT_BOUNCES: Final = 8
INDIRECT_CLAMP: Final = 10.0
FILTER_GLOSSY: Final = 1.0
DPI: Final = 300
LENS: Final = 50.0
CAUSTICS: Final = True
LATITUDE: Final = 29.7632836
LONGITUDE: Final = -95.3632715
ELEVATION: Final = 11.0
ZONE: Final = int((LONGITUDE + 180) // 6) + 1
EPSG: Final = (32600 if LATITUDE >= 0 else 32700) + ZONE
NORTH: Final = 0.0
MOMENT: Final = datetime(2026, 3, 20, 12, tzinfo=ZoneInfo("America/Chicago"))
DAYLIGHT: Final = MOMENT.dst() or timedelta()
OFFSET: Final = MOMENT.replace(tzinfo=UTC) - MOMENT - DAYLIGHT
DESIGN_TOOLS: Final = Path.home() / "Library" / "Application Support" / "design-tools"
MATERIALS: Final = DESIGN_TOOLS / "materials"
LOOK_DEVELOPMENT: Final = MATERIALS / "hdri" / "DaySkyHDRI069A_2K" / "DaySkyHDRI069A_2K_HDR.exr"
ASSETS: Final = DESIGN_TOOLS / "assets"
SITE: Final = DESIGN_TOOLS / "site"

# --- [MODELS] ---------------------------------------------------------------------------


class Survey(Enum):
    """USGS site survey rasters by their file under the site folder, each with its image service, pixel pitch in meters, and export parameters."""

    DEM = ("dem.tif", "https://elevation.nationalmap.gov/arcgis/rest/services/3DEPElevation/ImageServer/exportImage", 1.0, (("pixelType", "F32"), ("interpolation", "RSP_BilinearInterpolation")))
    IMAGERY = (
        "imagery.tif",
        "https://imagery.nationalmap.gov/arcgis/rest/services/USGSNAIPPlus/ImageServer/exportImage",
        0.3,
        (("pixelType", "U8"), ("renderingRule", '{"rasterFunction": "NaturalColor"}')),
    )

    def __init__(self, file: str, service: str, pitch: float, parameters: tuple[tuple[str, str], ...]) -> None:
        """Bind the member's path under the site folder, service, pitch, and parameters to their named fields."""
        self.path, self.service, self.pitch, self.parameters = SITE / file, service, pitch, parameters

    def address(self, box: str, reference: int, size: int) -> str:
        """Export address of the raster over the box in the spatial reference, the size in pixels across and down."""
        query = (("bbox", box), ("bboxSR", reference), ("imageSR", reference), ("size", f"{size},{size}"), ("format", "tiff"), *self.parameters, ("f", "image"))
        return f"{self.service}?{urlencode(query, safe='{},')}"


# --- [OPERATIONS] -----------------------------------------------------------------------


def stocked(folder: Path) -> bool:
    """Whether the folder holds a visible file at any depth."""
    return any(path.is_file() and not path.name.startswith(".") for path in folder.rglob("*"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ASSETS",
    "CAUSTICS",
    "DAYLIGHT",
    "DESIGN_TOOLS",
    "DIFFUSE_BOUNCES",
    "DPI",
    "ELEVATION",
    "EPSG",
    "FILTER_GLOSSY",
    "FRAME_SIZE",
    "GLOSSY_BOUNCES",
    "INDIRECT_CLAMP",
    "LATITUDE",
    "LENS",
    "LONGITUDE",
    "LOOK_DEVELOPMENT",
    "MATERIALS",
    "MAX_BOUNCES",
    "MOMENT",
    "NOISE_THRESHOLD",
    "NORTH",
    "OFFSET",
    "SAMPLES",
    "SITE",
    "Survey",
    "stocked",
    "TRANSMISSION_BOUNCES",
    "TRANSPARENT_BOUNCES",
    "VOLUME_BOUNCES",
    "ZONE",
]
