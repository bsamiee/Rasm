"""Final-render frame in pixels, samples, adaptive noise threshold, light path counts, indirect clamp in exposed scene-linear units, glossy filter, pixel density in dots per inch, caustics, and the shared asset folders every application renders a scene with."""

from pathlib import Path
from typing import Final

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
CAUSTICS: Final = True
DESIGN_TOOLS: Final = Path.home() / "Library" / "Application Support" / "design-tools"
MATERIALS: Final = DESIGN_TOOLS / "materials"

# --- [OPERATIONS] -----------------------------------------------------------------------


def stocked(folder: Path) -> bool:
    """Whether the folder holds a file that is not hidden at any depth."""
    return any(path.is_file() and not path.name.startswith(".") for path in folder.rglob("*"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CAUSTICS",
    "DESIGN_TOOLS",
    "DIFFUSE_BOUNCES",
    "DPI",
    "FILTER_GLOSSY",
    "FRAME_SIZE",
    "GLOSSY_BOUNCES",
    "INDIRECT_CLAMP",
    "MATERIALS",
    "MAX_BOUNCES",
    "NOISE_THRESHOLD",
    "SAMPLES",
    "stocked",
    "TRANSMISSION_BOUNCES",
    "TRANSPARENT_BOUNCES",
    "VOLUME_BOUNCES",
]
