"""Render settings with sample clamps in exposed radiance where 1.0 is display white, render passes, site, sun moment, ground albedo, and the material folder every application renders a scene with."""

from datetime import datetime, UTC
from enum import auto, Enum
from pathlib import Path
from typing import Final
from zoneinfo import ZoneInfo

# --- [TYPES] ----------------------------------------------------------------------------


class Pass(Enum):
    """Render passes every renderer writes beside the combined image."""

    DEPTH = auto()
    NORMAL = auto()
    ALBEDO = auto()
    MATERIAL_INDEX = auto()
    OBJECT_INDEX = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

FRAME_SIZE: Final = (1920, 1080)
SAMPLES: Final = 1024
NOISE_THRESHOLD: Final = 0.01
ADAPTIVE_MIN_SAMPLES: Final = 0
MAX_BOUNCES: Final = 12
DIFFUSE_BOUNCES: Final = 4
GLOSSY_BOUNCES: Final = 4
TRANSMISSION_BOUNCES: Final = 12
VOLUME_BOUNCES: Final = 0
TRANSPARENT_BOUNCES: Final = 8
EXPOSURE: Final = -5.3
DIRECT_CLAMP: Final = 0.0
INDIRECT_CLAMP: Final = 10.0
FILTER_GLOSSY: Final = 1.0
LIGHT_TREE: Final = True
DPI: Final = 300
LENS: Final = 50.0
CAUSTICS: Final = True
SUN_IRRADIANCE: Final = 139.3
GROUND_ALBEDO: Final = 0.2
LATITUDE: Final = 29.7632836
LONGITUDE: Final = -95.3632715
ELEVATION: Final = 11.0
NORTH: Final = 0.0

# --- [MOMENT] ---------------------------------------------------------------------------

MOMENT: Final = datetime(2026, 3, 20, 12, tzinfo=ZoneInfo("America/Chicago"))
OFFSET: Final = min(moment.replace(tzinfo=UTC) - moment for moment in (MOMENT.replace(month=1), MOMENT.replace(month=7)))
DAYLIGHT: Final = MOMENT.replace(tzinfo=UTC) - MOMENT - OFFSET

# --- [FOLDERS] --------------------------------------------------------------------------

DESIGN_TOOLS: Final = Path.home() / "Library" / "Application Support" / "design-tools"
MATERIALS: Final = DESIGN_TOOLS / "materials"

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ADAPTIVE_MIN_SAMPLES",
    "CAUSTICS",
    "DAYLIGHT",
    "DESIGN_TOOLS",
    "DIFFUSE_BOUNCES",
    "DIRECT_CLAMP",
    "DPI",
    "ELEVATION",
    "EXPOSURE",
    "FILTER_GLOSSY",
    "FRAME_SIZE",
    "GLOSSY_BOUNCES",
    "GROUND_ALBEDO",
    "INDIRECT_CLAMP",
    "LATITUDE",
    "LENS",
    "LIGHT_TREE",
    "LONGITUDE",
    "MATERIALS",
    "MAX_BOUNCES",
    "MOMENT",
    "NOISE_THRESHOLD",
    "NORTH",
    "OFFSET",
    "SAMPLES",
    "SUN_IRRADIANCE",
    "TRANSMISSION_BOUNCES",
    "TRANSPARENT_BOUNCES",
    "VOLUME_BOUNCES",
    "Pass",
]
