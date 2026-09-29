"""Rhino window layout the host and the Rhino side share: panel, ribbon tab, and dock bar ids, the measured record, and the layout it sizes."""

from collections.abc import Mapping
import ctypes
from enum import auto, StrEnum
import math
from types import MappingProxyType
from typing import Final, override, Self, TypedDict

from interface.frame import Place, RIGHT_COLUMN, Role, TREE_ROWS

# --- [TYPES] ----------------------------------------------------------------------------


class PanelId(StrEnum):
    """Registered panel type ids and the package panels."""

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
    SELECTION_FILTERS = "918191ca-1105-43f9-a34a-dda4276883c1"
    NAMED_SELECTIONS = "679af970-96d0-4c3a-831d-b4ff878e2884"


class RibbonTab(StrEnum):
    """Toolbar ids of the ribbon's tabs in ribbon order, each named in `packages.toml` by member name."""

    @override
    @classmethod
    def _missing_(cls, value: object) -> Self | None:
        """Member the name spells."""
        return cls.__members__.get(str(value))

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


class Bar(StrEnum):
    """Dock bars holding no panel tab, by the id Rhino registers each under."""

    RIBBON = "171011a9-a956-41ee-853e-3ccc0c0db1d8"
    SIDEBAR = "7491ffac-ebf2-4214-bf42-d3d1e4e0f0e1"
    COMMAND_HISTORY = "1d3d1785-2332-428b-a838-b2fe39ec50f4"


class Site(StrEnum):
    """Main window dock sites as Rhino names their locations."""

    LEFT = "Left"
    RIGHT = "Right"
    TOP = "Top"
    BOTTOM = "Bottom"


class Extent(StrEnum):
    """Lengths in points and row counts measured beside the dock site heights."""

    TAB_STRIP = auto()
    BUTTON = auto()
    RESIZER = auto()
    STATUS_BAR = auto()
    HISTORY_LINE = auto()
    LAYERS_CHROME = auto()
    LAYERS_HEADER = auto()
    LAYERS_ROW = auto()
    LAYERS_INSET = auto()
    LAYOUTS_INSET = auto()
    MATERIALS_STRIP = auto()
    MATERIALS_ROW = auto()
    LIBRARIES_CHROME = auto()
    LIBRARIES_BORDER = auto()
    LIBRARIES_ROW = auto()
    LIBRARIES_FOLDERS = auto()
    LIBRARIES_LIST_MINIMUM = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

TOGGLES: Final = (PanelId.OSNAP, PanelId.SELECTION_FILTERS)

# --- [MODELS] ---------------------------------------------------------------------------


class Grid(TypedDict):
    """Toggle count and cell pitch of a panel, and the width and height its container spends around the panel's content, in points."""

    count: int
    pitch_x: float
    pitch_y: float
    inset: float
    chrome: float


class Measured(TypedDict):
    """Record the script prints: each dock site's height, each extent, each toggle panel's grid, and each Layers column's default width by `LayerColumns.ColumnType` name in enum order."""

    sites: dict[Site, float]
    extents: dict[Extent, float]
    grids: dict[PanelId, Grid]
    columns: dict[str, int]


class Band(TypedDict):
    """Dock site's one band: its size in whole points and its bars in order, a panel container named by its tabs, each with its single-precision share or None as a band's only bar."""

    size: int
    bars: tuple[tuple[Bar | tuple[PanelId, ...], float | None], ...]


class Layout(TypedDict):
    """Band of each dock site and the container, named by its tabs, each panel returns to."""

    bands: Mapping[Site, Band]
    returns: Mapping[PanelId, tuple[PanelId, ...]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def band_sizes(extents: Mapping[Extent, float]) -> dict[Site, int]:
    """Whole points of the ribbon band showing its tab strip over one button row, the sidebar four buttons wide rounded up to the 5 point step, the right column, and the Command History band showing the whole history lines that fit the bottom stack above the status bar and the resizer."""
    cell, columns, step, bottom_stack, line = round(extents[Extent.BUTTON]), 4, 5, 87.5, extents[Extent.HISTORY_LINE]
    return {
        Site.TOP: round(extents[Extent.TAB_STRIP]) + cell,
        Site.LEFT: math.ceil(columns * cell / step) * step,
        Site.RIGHT: RIGHT_COLUMN,
        Site.BOTTOM: round(math.floor((bottom_stack - extents[Extent.STATUS_BAR] - extents[Extent.RESIZER]) / line) * line),
    }


def layers_height(extents: Mapping[Extent, float]) -> float:
    """Height of the Layers container showing the tree's rows."""
    return extents[Extent.LAYERS_CHROME] + extents[Extent.LAYERS_HEADER] + TREE_ROWS * extents[Extent.LAYERS_ROW]


def side_length(measured: Measured, site: Site) -> float:
    """Length of a side dock site once the top and bottom sites each hold their one band and its resizer, the window's sites summing to one height under any layout."""
    extents, sites = measured["extents"], measured["sites"]
    sizes = band_sizes(extents)
    return sites[site] + sum(sites[end] - sizes[end] - extents[Extent.RESIZER] for end in (Site.TOP, Site.BOTTOM))


def shares(measured: Measured, site: Site, lower: float) -> tuple[float, float]:
    """Single-precision shares of a side band's upper and lower bars, the lower bar at its content height and the upper one at the middle of the whole point Rhino truncates it to."""
    whole = side_length(measured, site)
    share = (whole - measured["extents"][Extent.RESIZER] - lower + 0.5) / whole
    return ctypes.c_float(share).value, ctypes.c_float(1 - share).value


def upper_length(measured: Measured, site: Site, lower: float) -> int:
    """Whole points Rhino gives a side band's upper bar, the single-precision product of the band length and the bar's share truncated."""
    upper, _ = shares(measured, site, lower)
    return math.trunc(ctypes.c_float(side_length(measured, site) * upper).value)


def layout(measured: Measured) -> Layout:
    """Layout the measures size, each side band sharing its length with the lower bar at its content height, the left container as tall as its tallest toggle grid wrapped as Rhino wraps it at the sidebar width."""
    extents, sizes = measured["extents"], band_sizes(measured["extents"])
    roles = {
        Role.PROPERTIES: PanelId.PROPERTIES,
        Role.DOCUMENT: PanelId.LAYOUTS,
        Role.COLOR: PanelId.MATERIALS,
        Role.ASSETS: PanelId.BLOCK_DEFINITIONS,
        Role.LIGHTING: PanelId.SUN,
        Role.DISPLAY: PanelId.DISPLAY,
        Role.VIEWS: PanelId.NAMED_VIEWS,
        Role.SNAPSHOTS: PanelId.SNAPSHOTS,
        Role.STRUCTURE: PanelId.LAYERS,
    }
    top, bottom = (tuple(roles[role] for role in place.value if role in roles) for place in (Place.RIGHT_TOP, Place.RIGHT_BOTTOM))
    height = max(math.ceil(grid["count"] / max(1, (sizes[Site.LEFT] - grid["inset"]) // grid["pitch_x"])) * grid["pitch_y"] + grid["chrome"] for grid in measured["grids"].values())

    def split(site: Site, upper: Bar | tuple[PanelId, ...], lower: tuple[PanelId, ...], height: float) -> Band:
        upper_share, lower_share = shares(measured, site, height)
        return Band(size=sizes[site], bars=((upper, upper_share), (lower, lower_share)))

    return Layout(
        bands=MappingProxyType({
            Site.TOP: Band(size=sizes[Site.TOP], bars=((Bar.RIBBON, None),)),
            Site.LEFT: split(Site.LEFT, Bar.SIDEBAR, TOGGLES, height),
            Site.RIGHT: split(Site.RIGHT, top, bottom, layers_height(extents)),
            Site.BOTTOM: Band(size=sizes[Site.BOTTOM], bars=((Bar.COMMAND_HISTORY, None),)),
        }),
        returns=MappingProxyType({panel: TOGGLES if panel in TOGGLES else bottom if panel in {*bottom, PanelId.LAYER_STATES} else top for panel in PanelId}),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TOGGLES", "Band", "Bar", "Extent", "Grid", "Layout", "Measured", "PanelId", "RibbonTab", "Site", "layers_height", "layout", "upper_length"]
