"""Color roles, sizes, and typefaces every application's interface takes, and the template text naming a role by dotted path."""

from collections.abc import Callable
from enum import Enum
from glob import escape
from operator import attrgetter
from pathlib import Path
from string import Template
from types import SimpleNamespace
from typing import Final
from uuid import NAMESPACE_URL, uuid5

from interface.palette import Palette

# --- [TYPES] ----------------------------------------------------------------------------

type Rgb = tuple[int, int, int]

# --- [CONSTANTS] ------------------------------------------------------------------------

RIGHT_COLUMN: Final = 315
TREE_ROWS: Final = 13
LOWER_EDITOR: Final = 450
POINT_WIDTH: Final = 6

# --- [MODELS] ---------------------------------------------------------------------------


class Opacity(float):
    """Opacity fraction, the one float a template renders as a percent."""


class Placeholders(Template):
    """Text whose placeholders name a role export or run-time value by dotted path, `$Surface.WELL` or `$Modality.INPUT.mark`."""

    idpattern = r"(?a:[_a-z][_a-z0-9]*(?:\.[_a-z][_a-z0-9]*)*)"


# --- [ROLES] ----------------------------------------------------------------------------


class Alpha:
    """Opacities of grids, fills, wires, disabled marks, and theme overlays drawn over their ground, the overlays as byte fractions."""

    GRID_MINOR: Final = Opacity(0.6)
    GRID_MAJOR: Final = Opacity(1.0)
    FACE_FILL: Final = Opacity(0.38)
    SELECTION_FILL: Final = Opacity(0.12)
    CROSSING_FILL: Final = Opacity(0.0)
    DISABLED: Final = Opacity(0.4)
    NULL_WIRE: Final = Opacity(0.5)
    DISABLED_WIRE: Final = Opacity(0.3)
    ZONE_FILL: Final = Opacity(0.2)
    WIRE_SELECTED: Final = Opacity(0.5)
    GLOW: Final = Opacity(0.35)
    EDIT_FACE: Final = Opacity(2 / 255)
    UV_FACE: Final = Opacity(10 / 255)
    HAIRLINE: Final = Opacity(17 / 255)
    VEIL: Final = Opacity(31 / 255)
    ROW_ITEM: Final = Opacity(51 / 255)
    UNSET: Final = Opacity(77 / 255)
    TINT: Final = Opacity(102 / 255)
    HALF: Final = Opacity(128 / 255)
    HOLD: Final = Opacity(153 / 255)
    RECEDED: Final = Opacity(179 / 255)
    INTERPOLATION: Final = Opacity(204 / 255)


class Ink:
    """Line ink of documents, black for layers and print, and of screens, white for every display member switching never reaches."""

    DOCUMENT: Final = (0, 0, 0)
    SCREEN: Final = (255, 255, 255)


class Surface:
    """Region fills in depth order, body fills and the ambient light lifting a shaded body's unlit side, the white paper of layout sheets, and the shadow of rims and veils."""

    RECESS: Final = Palette.NEUTRAL[1]
    FRAME: Final = Palette.NEUTRAL[2]
    WELL: Final = Palette.NEUTRAL[3]
    PANEL: Final = Palette.NEUTRAL[4]
    BOX: Final = Palette.NEUTRAL[5]
    FIELD: Final = Palette.NEUTRAL[6]
    HOVER: Final = Palette.NEUTRAL[8]
    CANVAS: Final = Palette.NEUTRAL[3]
    SECTION: Final = Palette.NEUTRAL[9]
    SHADED: Final = Palette.NEUTRAL[10]
    AMBIENT: Final = Palette.NEUTRAL[9]
    PAPER: Final = Ink.SCREEN
    SHADOW: Final = Ink.DOCUMENT


class Text:
    """Text, glyph, and code token colors."""

    PRIMARY: Final = Palette.NEUTRAL[12]
    SECONDARY: Final = Palette.NEUTRAL[11]
    DISABLED: Final = Palette.NEUTRAL[10]
    ON_SOLID: Final = Ink.DOCUMENT
    ICON: Final = Palette.NEUTRAL[11]
    KEYWORD: Final = Palette.VIOLET[11]
    STRING: Final = Palette.BLUE[11]
    NUMBER: Final = Palette.GREEN[11]
    FUNCTION: Final = Palette.YELLOW[11]
    DECORATOR: Final = Palette.LIME[11]
    ERROR: Final = Palette.RED[11]
    COMMENT: Final = Palette.NEUTRAL[10]


class Line:
    """Stroke colors of borders, a form field's rest border on paper, the neutral grids applications draw behind canvases and panels, the green datum grids a user creates, locked objects, edit marks, and timelines."""

    BORDER: Final = Palette.NEUTRAL[7]
    BORDER_HOVER: Final = Palette.NEUTRAL[8]
    FORM_FIELD: Final = Palette.NEUTRAL[11]
    GRID: Final = Palette.NEUTRAL[5]
    GRID_PANEL: Final = Palette.NEUTRAL[7]
    DATUM_GRID: Final = Palette.GREEN[8]
    LOCKED: Final = Palette.NEUTRAL[8]
    SEAM: Final = Palette.RED[11]
    SHARP: Final = Palette.VIOLET[11]
    CREASE: Final = Palette.YELLOW[11]
    BEVEL: Final = Palette.GREEN[11]
    KEY_EXTREME: Final = Palette.RED[11]
    KEY_BREAKDOWN: Final = Palette.VIOLET[11]
    KEY_JITTER: Final = Palette.GREEN[11]
    BEFORE_FRAME: Final = Palette.RED[11]
    AFTER_FRAME: Final = Palette.GREEN[11]


class Accent:
    """Chrome accent steps in the blue family of the macOS system accent."""

    ROW_SELECTED: Final = Palette.BLUE[5]
    TAB_ACTIVE: Final = Palette.BLUE[6]
    ROW_ACTIVE: Final = Palette.BLUE[7]
    TEXT_SELECTED: Final = Palette.BLUE[7]
    CONTROL_PRESSED: Final = Palette.BLUE[7]
    CONTROL_PRESSED_DISABLED: Final = Palette.BLUE[5]
    INDICATOR: Final = Palette.BLUE[8]
    FOCUS: Final = Palette.BLUE[9]
    CHECKBOX_CHECKED: Final = Palette.BLUE[9]
    TAB_ACTIVE_BAR: Final = Palette.BLUE[9]
    ITEM_SELECTED: Final = Palette.BLUE[10]
    ITEM_ACTIVE: Final = Palette.BLUE[11]
    LINK: Final = Palette.BLUE[11]


class Selection:
    """Canvas selection steps in the teal family, gizmos included."""

    BODY: Final = Palette.TEAL[6]
    BODY_HIDDEN: Final = Palette.TEAL[7]
    INACTIVE: Final = Palette.TEAL[8]
    ITEM: Final = Palette.TEAL[9]
    ACTIVE: Final = Palette.TEAL[11]
    HOVER: Final = Palette.TEAL[11]
    GIZMO: Final = Palette.TEAL[9]
    GIZMO_SECONDARY: Final = Palette.TEAL[11]
    GIZMO_HOVER: Final = Palette.TEAL[12]


class Field:
    """Property field fills by the state of their value, and the bar a slider fills to its value."""

    ANIMATED: Final = Palette.BLUE[4]
    KEYED: Final = Palette.BLUE[6]
    DRIVEN: Final = Palette.VIOLET[7]
    OVERRIDDEN: Final = Palette.VIOLET[4]
    CHANGED: Final = Palette.RED[5]
    EDITING: Final = Palette.NEUTRAL[2]
    SLIDER: Final = Palette.NEUTRAL[9]


class Guide:
    """Construction, handle, and command feedback marks."""

    CONSTRUCTION: Final = Palette.ORANGE[11]
    HANDLE: Final = Palette.NEUTRAL[11]
    TRACKING: Final = Palette.MAGENTA[11]
    TRACKING_ACTIVE: Final = Palette.MAGENTA[12]
    TENTATIVE: Final = Palette.MAGENTA[6]


class Preview:
    """Colors of geometry a definition computes and has not baked."""

    INK: Final = Palette.LIME[9]
    BODY: Final = Surface.SHADED
    SELECTED: Final = Selection.ITEM


class Node:
    """Node borders by state, each brighter than the body it surrounds."""

    BORDER: Final = Palette.NEUTRAL[11]
    SELECTED_BORDER: Final = Selection.ACTIVE
    ACTIVE_BORDER: Final = Palette.TEAL[12]


class Axis:
    """Axis triad of every coordinate system and handle."""

    X: Final = Palette.RED[9]
    Y: Final = Palette.GREEN[10]
    Z: Final = Palette.ULTRAMARINE[9]


class Status:
    """Message kinds in severity order, each the body and border of its message."""

    ERROR: Final = Palette.RED[9]
    WARNING: Final = Palette.YELLOW[9]
    INFO: Final = Palette.SLATE[8]
    SUCCESS: Final = Palette.GREEN[9]


class Wire:
    """Node wire colors, one for every data type over a casing in the canvas color, and widths in logical pixels at zoom 1."""

    CORE: Final = Palette.NEUTRAL[11]
    CASING: Final = Surface.CANVAS
    SELECTED: Final = Selection.ITEM
    GLOW: Final = Selection.BODY
    ITEM_WIDTH: Final = 1
    TWIG_WIDTH: Final = 2
    TREE_WIDTH: Final = 2


class Tag(Enum):
    """User-assigned tag slots in Blender's slot order, the neutral slot last, each an identity solid clear of the axis, error, construction, tracking, preview, datum grid, and selection steps."""

    COLOR_01 = Palette.CRIMSON[9]
    COLOR_02 = Palette.SLATE[9]
    COLOR_03 = Palette.YELLOW[9]
    COLOR_04 = Palette.EMERALD[9]
    COLOR_05 = Palette.CERULEAN[9]
    COLOR_06 = Palette.VIOLET[9]
    COLOR_07 = Palette.ROSE[9]
    COLOR_08 = Palette.CYAN[9]
    NEUTRAL = Palette.NEUTRAL[9]


class Annotation:
    """Annotation layer every application shares, printed in document ink."""

    NAME: Final = "ANNO"
    ID: Final = uuid5(NAMESPACE_URL, NAME)
    TAG: Final = Tag.COLOR_07


class Modality(Enum):
    """Node, icon, and ribbon categories by data type, orange, yellow, and lime marking neutral bodies."""

    GEOMETRY = (Palette.CYAN[3], Palette.CYAN[6], Palette.CYAN[9], Palette.CYAN[8], Palette.CYAN[11], Palette.CYAN[12])
    SCALAR = (Palette.NEUTRAL[3], Palette.NEUTRAL[6], Palette.NEUTRAL[9], Palette.NEUTRAL[8], Palette.NEUTRAL[11], Palette.NEUTRAL[12])
    VECTOR = (Palette.ULTRAMARINE[3], Palette.ULTRAMARINE[6], Palette.ULTRAMARINE[9], Palette.ULTRAMARINE[8], Palette.ULTRAMARINE[11], Palette.ULTRAMARINE[12])
    TRANSFORM = (Palette.VIOLET[3], Palette.VIOLET[6], Palette.VIOLET[9], Palette.VIOLET[8], Palette.VIOLET[11], Palette.VIOLET[12])
    TEXT = (Palette.BLUE[3], Palette.BLUE[6], Palette.BLUE[11], Palette.BLUE[8], Palette.BLUE[11], Palette.BLUE[12])
    DATA = (Palette.SLATE[3], Palette.SLATE[6], Palette.SLATE[9], Palette.SLATE[8], Palette.SLATE[11], Palette.SLATE[12])
    COLOR = (Palette.NEUTRAL[3], Palette.NEUTRAL[6], Palette.YELLOW[9], Palette.NEUTRAL[8], Palette.YELLOW[11], Palette.YELLOW[12])
    INPUT = (Palette.ROSE[3], Palette.ROSE[6], Palette.ROSE[9], Palette.ROSE[8], Palette.ROSE[11], Palette.ROSE[12])
    OUTPUT = (Palette.RED[3], Palette.RED[6], Palette.RED[9], Palette.RED[8], Palette.RED[11], Palette.RED[12])
    DISPLAY = (Palette.NEUTRAL[3], Palette.NEUTRAL[6], Preview.INK, Palette.NEUTRAL[8], Palette.LIME[11], Palette.LIME[12])
    ANALYSIS = (Palette.NEUTRAL[3], Palette.NEUTRAL[6], Palette.ORANGE[9], Palette.NEUTRAL[8], Palette.ORANGE[11], Palette.ORANGE[12])

    def __init__(self, fill: Rgb, header: Rgb, mark: Rgb, border: Rgb, token: Rgb, shine: Rgb) -> None:
        """Bind the category's steps to their named fields."""
        self.fill, self.header, self.mark, self.border, self.token, self.shine = fill, header, mark, border, token, shine


class Zone:
    """Node zones, each drawn in the modality whose hue it takes."""

    SIMULATION: Final = Modality.INPUT
    REPEAT: Final = Modality.TRANSFORM
    FOR_EACH: Final = Modality.VECTOR
    CLOSURE: Final = Modality.DATA


class Typography(Enum):
    """Typefaces by text role, each a font file and its family name."""

    INTERFACE = ("Geist[wght].ttf", "Geist")
    MONOSPACE = ("GeistMono[wght].ttf", "Geist Mono")

    def __init__(self, file: str, family: str) -> None:
        """Bind the member's file and family to their named fields."""
        self.file, self.family = file, family

    @property
    def path(self) -> Path:
        """Face's file under the user font folder."""
        return next((Path.home() / "Library" / "Fonts").rglob(escape(self.file)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def blend(top: Rgb, bottom: Rgb, alpha: float) -> Rgb:
    """Color of `top` drawn at `alpha` over `bottom`."""
    red, green, blue = (round(upper * alpha + lower * (1 - alpha)) for upper, lower in zip(top, bottom, strict=True))
    return (red, green, blue)


def rendered[T](template: str, spelled: Callable[[T], str], **values: T) -> str:
    """Template text with each placeholder replaced by the spelling of the run-time value or role its dotted path names."""
    text, names = Placeholders(template), SimpleNamespace(**globals(), **values)
    return text.substitute({name: spelled(attrgetter(name)(names)) for name in text.get_identifiers()})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "LOWER_EDITOR",
    "POINT_WIDTH",
    "RIGHT_COLUMN",
    "TREE_ROWS",
    "Accent",
    "Alpha",
    "Annotation",
    "Axis",
    "Field",
    "Guide",
    "Ink",
    "Line",
    "Modality",
    "Node",
    "Opacity",
    "Preview",
    "Rgb",
    "Selection",
    "Status",
    "Surface",
    "Tag",
    "Text",
    "Typography",
    "Wire",
    "Zone",
    "blend",
    "rendered",
]
