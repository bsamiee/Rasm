"""Window layout and sizes every application shares."""

from enum import auto, Enum, StrEnum
from typing import Final

# --- [TYPES] ----------------------------------------------------------------------------


class Task(StrEnum):
    """Workspace tasks by tab name in tab order."""

    MODELING = "Modeling"
    SITE = "Site"
    NODES = "Nodes"
    BIM = "BIM"
    SHADING = "Shading"
    DRAFTING = "Drafting"
    RENDERING = "Rendering"
    SCRIPTING = "Scripting"


class Role(Enum):
    """Right-column panel roles each application maps to its own ids, COLOR holding material and swatch editors, DOCUMENT sheet lists, and STRUCTURE the document tree."""

    PROPERTIES = auto()
    ALIGNMENT = auto()
    DOCUMENT = auto()
    COLOR = auto()
    AUTOMATION = auto()
    LIBRARIES = auto()
    TRANSFORM = auto()
    CHARACTER = auto()
    STYLES = auto()
    GLYPHS = auto()
    ASSETS = auto()
    PATTERN = auto()
    BLEND = auto()
    EFFECTS = auto()
    EXPORT = auto()
    INFORMATION = auto()
    OUTPUT = auto()
    LIGHTING = auto()
    DISPLAY = auto()
    VIEWS = auto()
    SNAPSHOTS = auto()
    STRUCTURE = auto()


class Place(Enum):
    """Right column of the frame, selection properties on top and the document tree at the bottom, with the roles each holds in order."""

    RIGHT_TOP = (
        Role.PROPERTIES,
        Role.ALIGNMENT,
        Role.DOCUMENT,
        Role.COLOR,
        Role.AUTOMATION,
        Role.LIBRARIES,
        Role.TRANSFORM,
        Role.CHARACTER,
        Role.STYLES,
        Role.GLYPHS,
        Role.ASSETS,
        Role.PATTERN,
        Role.BLEND,
        Role.EFFECTS,
        Role.EXPORT,
        Role.INFORMATION,
        Role.OUTPUT,
        Role.LIGHTING,
        Role.DISPLAY,
        Role.VIEWS,
        Role.SNAPSHOTS,
    )
    RIGHT_BOTTOM = (Role.STRUCTURE,)


# --- [CONSTANTS] ------------------------------------------------------------------------

RIGHT_COLUMN: Final = 315
LOWER_EDITOR: Final = 450
TREE_ROWS: Final = 13

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LOWER_EDITOR", "RIGHT_COLUMN", "TREE_ROWS", "Place", "Role", "Task"]
