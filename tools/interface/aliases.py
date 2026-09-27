"""Alias families by first key, and the command aliases with their Rhino macros that `aliases.txt` beside this module holds."""

from pathlib import Path
from types import MappingProxyType
from typing import Final

# --- [TABLES] ---------------------------------------------------------------------------

FAMILIES: Final = MappingProxyType({
    "Q": "Curves and points",
    "W": "Rectangles and polygonal primitives",
    "E": "Circles and round primitives",
    "R": "Rotate, orient, mirror, and view",
    "T": "Text, scale, and deformation",
    "A": "Offset, boolean, arrays, and flow",
    "S": "Surfaces",
    "D": "Dimensions and measurement",
    "F": "Trim, join, fillet, and cut",
    "G": "Group, visibility, and guides",
    "Z": "Zoom and views",
    "X": "Extrude, extract, and project",
    "C": "Copy, construction planes, and clipping",
    "V": "Move, align, and select",
    "B": "Blocks",
    "M": "Merge and match",
    "L": "Layouts",
    "I": "Import and insert",
    "P": "Purge and cleanup",
})
COMMAND_ALIASES: Final = MappingProxyType({
    name: macro for name, _, macro in (entry.partition(" ") for entry in Path(__file__).with_name("aliases.txt").read_text(encoding="utf-8").splitlines() if entry)
})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COMMAND_ALIASES", "FAMILIES"]
