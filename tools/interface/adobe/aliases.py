"""Alias prompts of Illustrator, Photoshop, and InDesign: the leader key that opens each one and the product command each shared alias runs."""

from collections.abc import Mapping
from typing import Final

import msgspec

from interface.adobe.workspaces import Form
from interface.aliases import FAMILIES

# --- [CONSTANTS] ------------------------------------------------------------------------

LABEL: Final = "Alias"

# --- [MODELS] ---------------------------------------------------------------------------


class Command(msgspec.Struct, frozen=True, tag_field="kind", tag=str.lower):
    """Product command by the id the product registers it under."""

    id: str | int


class Tool(Command, frozen=True):
    """Tool the product selects by an Illustrator tool name, a Photoshop tool class string id, or an InDesign `UITools` member."""


class Menu(Command, frozen=True):
    """Menu command the product runs by an Illustrator command name, a Photoshop event string id, or an InDesign menu action id, acting on the selection alone or not."""

    selection: bool


class Prompt(msgspec.Struct, frozen=True):
    """Product's alias prompt with its leader key, a function key number or a UXP shortcut, and each alias's command in list order."""

    form: Form
    leader: int | str
    commands: Mapping[str, Tool | Menu]

    @property
    def families(self) -> Mapping[str, str]:
        """Family names by first key of the families holding a command."""
        return {key: name for key, name in FAMILIES.items() if any(alias.startswith(key) for alias in self.commands)}


# --- [TABLES] ---------------------------------------------------------------------------

ILLUSTRATOR: Final = Prompt(
    Form.ILLUSTRATOR,
    9,
    {
        "Q": Tool("Adobe Line Tool"),
        "QQ": Tool("Adobe Pen Tool"),
        "QW": Tool("Adobe Arc Tool"),
        "QE": Tool("Adobe Curvature Tool"),
        "QR": Menu("Path Blend Make", selection=True),
        "W1": Tool("Adobe Rectangle Shape Tool"),
        "W3": Tool("Adobe Rounded Rectangle Tool"),
        "WQ": Tool("Adobe Shape Construction Regular Polygon Tool"),
        "E": Tool("Adobe Ellipse Shape Tool"),
        "R": Tool("Adobe Rotate Tool"),
        "R2": Tool("Adobe Reflect Tool"),
        "T": Tool("Adobe Type Tool"),
        "TT": Menu("outline", selection=True),
        "T3": Tool("Adobe Scale Tool"),
        "T4": Menu("Transform3", selection=True),
        "TW": Tool("Adobe Shear Tool"),
        "AQ": Tool("Adobe Shape Builder Tool"),
        "D": Tool("Adobe Measure Tool"),
        "FF": Menu("join", selection=True),
        "FQ": Tool("Adobe Scissors Tool"),
        "FD": Tool("Adobe Knife Tool"),
        "G": Menu("group", selection=True),
        "GU": Menu("ungroup", selection=True),
        "GH": Menu("hide", selection=True),
        "GJ": Menu("showAll", selection=False),
        "GL": Menu("lock", selection=True),
        "GP": Menu("unlockAll", selection=False),
        "GW": Menu("makeguide", selection=True),
        "GE": Menu("clearguide", selection=False),
        "Z": Tool("Adobe Zoom Tool"),
        "ZE": Menu("fitall", selection=False),
        "V": Tool("Adobe Select Tool"),
        "VA": Menu("AdobeAlignObjects2", selection=False),
        "VO": Menu("selectall", selection=False),
        "VI": Menu("Inverse menu item", selection=False),
        "B": Menu("Adobe New Symbol Shortcut", selection=True),
        "BE": Menu("Adobe Symbol Palette", selection=False),
        "IM": Menu("AI Place", selection=False),
    },
)
PHOTOSHOP: Final = Prompt(
    Form.PHOTOSHOP,
    13,
    {
        "Q": Tool("lineTool"),
        "QQ": Tool("penTool"),
        "W1": Tool("rectangleTool"),
        "WQ": Tool("polygonTool"),
        "E": Tool("ellipseTool"),
        "T": Tool("typeCreateOrEditTool"),
        "D": Tool("rulerTool"),
        "G": Menu("groupLayersEvent", selection=True),
        "GU": Menu("ungroupLayersEvent", selection=True),
        "Z": Tool("zoomTool"),
        "V": Tool("moveTool"),
        "VO": Menu("selectAllLayers", selection=False),
    },
)
INDESIGN: Final = Prompt(
    Form.INDESIGN,
    "Ctrl+G",
    {
        "Q": Tool("LINE_TOOL"),
        "QQ": Tool("PEN_TOOL"),
        "W1": Tool("RECTANGLE_TOOL"),
        "WQ": Tool("POLYGON_TOOL"),
        "E": Tool("ELLIPSE_TOOL"),
        "R": Tool("ROTATE_TOOL"),
        "T": Tool("TYPE_TOOL"),
        "TT": Menu(61405, selection=True),
        "T3": Tool("SCALE_TOOL"),
        "TW": Tool("SHEAR_TOOL"),
        "D": Tool("MEASURE_TOOL"),
        "FF": Menu(99621, selection=True),
        "FQ": Tool("SCISSORS_TOOL"),
        "G": Menu(118844, selection=True),
        "GU": Menu(118845, selection=True),
        "GH": Menu(118856, selection=True),
        "GJ": Menu(118857, selection=False),
        "GL": Menu(11304, selection=True),
        "GP": Menu(11395, selection=False),
        "GE": Menu(118850, selection=False),
        "Z": Tool("ZOOM_TOOL"),
        "ZE": Menu(118787, selection=False),
        "V": Tool("SELECTION_TOOL"),
        "VA": Menu(25857, selection=False),
        "VO": Menu(276, selection=False),
        "IM": Menu(113409, selection=False),
    },
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ILLUSTRATOR", "INDESIGN", "LABEL", "PHOTOSHOP", "Menu", "Prompt", "Tool"]
