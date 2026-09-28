"""Render settings, site, sun moment, and shared asset folders every application renders a scene with."""

from datetime import datetime, UTC
from pathlib import Path
from typing import Final
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
NORTH: Final = 0.0
MOMENT: Final = datetime(2026, 3, 20, 12, tzinfo=ZoneInfo("America/Chicago"))

# --- [MOMENT] ---------------------------------------------------------------------------

OFFSET: Final = min(moment.replace(tzinfo=UTC) - moment for moment in (MOMENT.replace(month=1), MOMENT.replace(month=7)))
DAYLIGHT: Final = MOMENT.replace(tzinfo=UTC) - MOMENT - OFFSET

# --- [FOLDERS] --------------------------------------------------------------------------

DESIGN_TOOLS: Final = Path.home() / "Library" / "Application Support" / "design-tools"
MATERIALS: Final = DESIGN_TOOLS / "materials"
LOOK_DEVELOPMENT: Final = MATERIALS / "hdri" / "DaySkyHDRI069A_2K" / "DaySkyHDRI069A_2K_HDR.exr"
ASSETS: Final = DESIGN_TOOLS / "assets"

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
    "TRANSMISSION_BOUNCES",
    "TRANSPARENT_BOUNCES",
    "VOLUME_BOUNCES",
    "stocked",
]
