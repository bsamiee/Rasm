"""Window layout and sizes every application shares."""

from enum import StrEnum
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


# --- [CONSTANTS] ------------------------------------------------------------------------

RIGHT_COLUMN: Final = 315
LOWER_EDITOR: Final = 450
TREE_ROWS: Final = 13

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LOWER_EDITOR", "RIGHT_COLUMN", "TREE_ROWS", "Task"]
