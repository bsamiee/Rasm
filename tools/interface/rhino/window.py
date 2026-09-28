"""Rhino window layout facts the host and the Rhino side share."""

from collections.abc import Mapping
from enum import auto, StrEnum
import math
from types import MappingProxyType
from typing import Final

from interface.frame import Place, RIGHT_COLUMN, Role

# --- [TYPES] ----------------------------------------------------------------------------


class Panel(StrEnum):
    """Registered panel type ids and the Command History dock bar's id."""

    PROPERTIES = "34ffb674-c504-49d9-9fcd-99cc811dcda2"
    NAMED_VIEWS = "77d33034-194d-4cd5-957c-730d9a9eac50"
    LAYOUTS = "562bda2a-184f-4b22-9607-79d992f28557"
    BLOCK_DEFINITIONS = "6b6ffd64-c279-4b45-9959-e7e5a8eef806"
    MATERIALS = "6df2a957-f12d-42ea-9fa6-95d7920c1b76"
    SUN = "1012681e-d276-49d3-9cd9-7de92dc2404a"
    DISPLAY = "b68e9e9f-c79c-473c-a7ef-846a11dc4e7b"
    SNAPSHOTS = "f4424a46-8281-430a-b03d-911dc9b40294"
    LAYERS = "3610bf83-047d-4f7f-93fd-163ea305b493"
    ENVIRONMENTS = "7df2a957-f12d-42ea-9fa6-95d7920c1b76"
    LIBRARIES = "b70a4973-99ca-40c0-b2b2-f03417a5ff1d"
    NOTES = "1d55d702-028c-4aab-99cc-acfdd441fe5f"
    NAMED_POSITIONS = "91799cf0-059a-46f8-854c-cc1c1419e29f"
    LAYER_STATES = "6df55e69-e102-4a72-b181-11664046c93f"
    NAMED_CPLANES = "8f23551a-a05b-4a03-a8d5-3e2fc55e4d8a"
    TEXTURES = "8df2a957-f12d-42ea-9fa6-95d7920c1b76"
    RENDERING = "d9ac0269-811b-47d1-aa33-777986b13715"
    GROUND_PLANE = "987b1930-ecde-4e62-8282-97ab4ad325fe"
    LIGHTS = "86777b3d-3d68-4965-84f8-9e019c402433"
    CONTEXT_HELP = "0f8fb4f9-c213-4a6e-8e79-0bece02df82a"
    BLOCK_CONTENT = "072f97a4-6861-4113-a2d5-838d97999336"
    FILE_EXPLORER = "cd3ab54f-0213-4d30-b4f5-39a018e39604"
    AI = "fb948c98-5987-45a3-8dcb-2814ed77ee3b"
    CLIPPING_BOXES = "b020ad68-899e-438d-8a3b-ff6455452fa3"
    SCRIPTS = "8e40b456-7e20-43a0-92e6-a6991419cc91"
    WHAT = "2017c0ee-500a-43ae-b920-88465a7132a0"
    OSNAP = "d3c4a392-88de-4c4f-88a4-ba5636ef7f38"
    COMMAND_HISTORY = "1d3d1785-2332-428b-a838-b2fe39ec50f4"


class Toolbar(StrEnum):
    """Toolbar ids of the ribbon's tabs in role order."""

    STANDARD = "4bb9c817-d19f-45fd-8af2-39e9805f3e9f"
    SELECT = "79d0d952-85af-4fe3-8444-46596bbe22fd"
    VISIBILITY = "0608110b-184c-443f-97ec-0612d9d2b605"
    CURVE_TOOLS = "d767ebe9-eebd-4e75-a217-03f7431f71bf"
    SURFACE_TOOLS = "b977d038-c9b6-4a9b-b097-9592a4117052"
    SOLID_TOOLS = "4cd9a071-9337-4389-aa40-2a20f570da3b"
    SUBD_TOOLS = "44619cf6-b73a-46ea-93f8-46f1fa333115"
    MESH_TOOLS = "a5379863-ee51-4b77-ab0c-44ee93c92ca3"
    TRANSFORM = "9f21066b-eb0f-46f5-adeb-d62f80e04fd7"
    DRAFTING = "90fd89fc-e41f-49cc-bcf0-29e0d58017a1"
    RENDER_TOOLS = "d0a817a1-dea9-4e03-89e3-0d63d99b5e51"
    DISPLAY = "720d3154-34fc-4177-8e52-9f417d4b5af3"
    SET_VIEW = "2ed87e2b-d225-4625-aeda-b11a115c9a14"
    CPLANES = "32318c40-46e9-4aa3-8f73-09371ec27a4d"


class Site(StrEnum):
    """Main window dock sites as Rhino names their locations, each naming the height measured for it."""

    LEFT = "Left"
    RIGHT = "Right"
    TOP = "Top"
    BOTTOM = "Bottom"


class Extent(StrEnum):
    """Lengths in points and row and toggle counts measured beside the dock site heights."""

    TAB_STRIP = auto()
    BUTTON = auto()
    RESIZER = auto()
    LAYERS_CHROME = auto()
    LAYERS_HEADER = auto()
    LAYERS_ROW = auto()
    LAYERS_INSET = auto()
    LAYOUTS_INSET = auto()
    OSNAP_TOGGLES = auto()
    OSNAP_PITCH_X = auto()
    OSNAP_PITCH_Y = auto()
    OSNAP_INSET = auto()
    OSNAP_CHROME = auto()
    MATERIALS_STRIP = auto()
    MATERIALS_ROW = auto()
    LIBRARIES_CHROME = auto()
    LIBRARIES_BORDER = auto()
    LIBRARIES_ROW = auto()
    LIBRARIES_FOLDERS = auto()
    LIBRARIES_LIST_MINIMUM = auto()


# --- [TABLES] ---------------------------------------------------------------------------

PANELS: Final = MappingProxyType({
    Role.PROPERTIES: Panel.PROPERTIES,
    Role.DOCUMENT: Panel.LAYOUTS,
    Role.COLOR: Panel.MATERIALS,
    Role.ASSETS: Panel.BLOCK_DEFINITIONS,
    Role.LIGHTING: Panel.SUN,
    Role.DISPLAY: Panel.DISPLAY,
    Role.VIEWS: Panel.NAMED_VIEWS,
    Role.SNAPSHOTS: Panel.SNAPSHOTS,
    Role.STRUCTURE: Panel.LAYERS,
})
RIGHT_TOP: Final = tuple(PANELS[role] for role in Place.RIGHT_TOP.value if role in PANELS)
RIGHT_BOTTOM: Final = tuple(PANELS[role] for role in Place.RIGHT_BOTTOM.value if role in PANELS)
RETURN_TOP: Final = (
    Panel.ENVIRONMENTS,
    Panel.LIBRARIES,
    Panel.NOTES,
    Panel.NAMED_POSITIONS,
    Panel.NAMED_CPLANES,
    Panel.TEXTURES,
    Panel.RENDERING,
    Panel.GROUND_PLANE,
    Panel.LIGHTS,
    Panel.CONTEXT_HELP,
    Panel.BLOCK_CONTENT,
    Panel.FILE_EXPLORER,
)
RETURN_BOTTOM: Final = (Panel.LAYER_STATES,)

# --- [OPERATIONS] -----------------------------------------------------------------------


def osnap_height(measured: Mapping[Site | Extent, float], width: float) -> float:
    """Height the Osnap strip's toggle grid wraps to at the band width, as Rhino's grid layout wraps it."""
    per_row = min(measured[Extent.OSNAP_TOGGLES], max(1, (width - measured[Extent.OSNAP_INSET]) // measured[Extent.OSNAP_PITCH_X]))
    return math.ceil(measured[Extent.OSNAP_TOGGLES] / per_row) * measured[Extent.OSNAP_PITCH_Y] + measured[Extent.OSNAP_CHROME]


def bands(measured: Mapping[Site | Extent, float]) -> dict[Site, int]:
    """Whole points each rewritten dock site's first band settles at, the sidebar four buttons wide above the Osnap strip."""
    cell = round(measured[Extent.BUTTON])
    return {Site.TOP: round(measured[Extent.TAB_STRIP]) + cell, Site.LEFT: 4 * cell, Site.RIGHT: RIGHT_COLUMN}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RETURN_BOTTOM", "RETURN_TOP", "RIGHT_BOTTOM", "RIGHT_TOP", "Extent", "Panel", "Site", "Toolbar", "bands", "osnap_height"]
