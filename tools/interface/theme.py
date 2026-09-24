"""The color roles every application's interface takes from the Radix Colors dark steps, and the typefaces of its text.

Roles group by category, then role, then state, each naming one step, and alphas are fractions.
"""

from enum import Enum
from pathlib import Path
from typing import Final

from radix import Radix, Rgb

# --- [CONSTANTS] ------------------------------------------------------------------------

GRID_MINOR_ALPHA: Final = 0.6
GRID_MAJOR_ALPHA: Final = 1.0
FACE_FILL_ALPHA: Final = 0.2
FRAME_NODE_ALPHA: Final = 0.5
SELECTION_FILL_ALPHA: Final = 0.12
DISABLED_ALPHA: Final = 0.4
TEXT_WEIGHT: Final = 400


class OffScale:
    """The two values outside the scales, each answering a threshold no Radix step meets."""

    black: Final[Rgb] = (0, 0, 0)
    canvas: Final[Rgb] = (77, 77, 77)


# --- [ROLES] ----------------------------------------------------------------------------


class Surface:
    """Region fills in depth order, from the window frame to the lightest surface drawn on the canvas."""

    FRAME: Final = Radix.gray2
    WELL: Final = Radix.gray3
    PANEL: Final = Radix.gray4
    BOX: Final = Radix.gray5
    FIELD: Final = Radix.gray6
    HOVER: Final = Radix.gray8
    CANVAS: Final = OffScale.canvas
    SECTION: Final = Radix.gray9
    SHADED: Final = Radix.gray11


class Text:
    """Text and glyph colors, the code tokens of text editors included."""

    PRIMARY: Final = Radix.gray12
    SECONDARY: Final = Radix.gray11
    DISABLED: Final = Radix.gray10
    ON_SOLID: Final = OffScale.black
    ICON: Final = Radix.gray11
    ICON_SHADE: Final = Radix.gray8
    KEYWORD: Final = Radix.plum11
    STRING: Final = Radix.yellow11
    NUMBER: Final = Radix.blue11
    BUILTIN: Final = Radix.grass11
    ERROR: Final = Radix.red11


class Line:
    """Strokes: geometry, outlines, grids, locked objects, edit marks, and the sides of a timeline."""

    GEOMETRY: Final = OffScale.black
    BORDER: Final = Radix.gray7
    BORDER_HOVER: Final = Radix.gray8
    GRID: Final = Radix.gray8
    GRID_WELL: Final = Radix.gray5
    GRID_PANEL: Final = Radix.gray6
    LOCKED: Final = Radix.gray10
    SEAM: Final = Radix.red11
    SHARP: Final = Radix.violet11
    CREASE: Final = Radix.yellow11
    BEVEL: Final = Radix.grass11
    KEY_EXTREME: Final = Radix.red11
    KEY_BREAKDOWN: Final = Radix.violet11
    KEY_JITTER: Final = Radix.grass11
    BEFORE_FRAME: Final = Radix.red11
    AFTER_FRAME: Final = Radix.grass11


class Accent:
    """The interface accent in the blue family of the macOS system accent: rows, tabs, pressed and checked controls, indication, links, and focus, never a mark drawn on a canvas."""

    ROW_SELECTED: Final = Radix.blue5
    TAB_ACTIVE: Final = Radix.blue6
    ROW_ACTIVE: Final = Radix.blue7
    TEXT_SELECTED: Final = Radix.blue7
    VIEW_ACTIVE: Final = Radix.blue6
    CONTROL_PRESSED: Final = Radix.blue7
    CONTROL_PRESSED_DISABLED: Final = Radix.blue5
    INDICATOR: Final = Radix.blue8
    FOCUS: Final = Radix.blue9
    CHECKBOX_CHECKED: Final = Radix.blue9
    LINK: Final = Radix.blue11
    TAB_ACTIVE_BAR: Final = Radix.blue9
    ITEM_SELECTED: Final = Radix.blue10
    ITEM_ACTIVE: Final = Radix.blue12
    ITEM_HOVER: Final = Radix.blue12


class Selection:
    """The canvas accent in the cyan family: what is selected, active, or hovered where geometry, nodes, wires, and keys draw, and the gizmos that act on them."""

    ITEM: Final = Radix.cyan10
    ACTIVE: Final = Radix.cyan12
    HOVER: Final = Radix.cyan12
    INACTIVE: Final = Radix.cyan8
    BODY: Final = Radix.cyan6
    BODY_HIDDEN: Final = Radix.cyan8
    GIZMO: Final = Radix.cyan9
    GIZMO_SECONDARY: Final = Radix.cyan11


class Field:
    """Fills of a property field by the state of its value."""

    ANIMATED: Final = Radix.blue4
    KEYED: Final = Radix.blue6
    DRIVEN: Final = Radix.violet7
    OVERRIDDEN: Final = Radix.violet4
    CHANGED: Final = Radix.red5
    EDITING: Final = Radix.gray2


class Guide:
    """Construction marks: persistent guides in a quiet orange, handles in neutral gray, and transient command feedback alone in magenta."""

    CONSTRUCTION: Final = Radix.orange11
    HANDLE: Final = Radix.gray12
    TRACKING: Final = Radix.plum10
    TRACKING_ACTIVE: Final = Radix.plum11
    TENTATIVE: Final = Radix.plum6


class Axis:
    """The axis triad of every coordinate system and handle."""

    X: Final = Radix.red9
    Y: Final = Radix.grass9
    Z: Final = Radix.indigo9


class Status:
    """Message kinds in severity order."""

    ERROR: Final = Radix.red8
    WARNING: Final = Radix.yellow9
    INFO: Final = Radix.gray8
    SUCCESS: Final = Radix.grass9


class Tag(Enum):
    """User-assigned category slots in Blender's slot order, none in the canvas selection, construction, or tracking hue, each a slot's solid step and the steps its selected and active members take."""

    solid: Rgb
    selected: Rgb
    active: Rgb

    COLOR_01 = (Radix.red9, Radix.red10, Radix.red11)
    COLOR_02 = (Radix.lime9, Radix.lime10, Radix.lime11)
    COLOR_03 = (Radix.yellow9, Radix.yellow10, Radix.yellow11)
    COLOR_04 = (Radix.grass9, Radix.grass10, Radix.grass11)
    COLOR_05 = (Radix.blue9, Radix.blue10, Radix.blue11)
    COLOR_06 = (Radix.violet9, Radix.violet10, Radix.violet11)
    COLOR_07 = (Radix.pink9, Radix.pink10, Radix.pink11)
    COLOR_08 = (Radix.mint9, Radix.mint10, Radix.mint11)
    COLOR_09 = (Radix.gray9, Radix.gray10, Radix.gray11)

    def __init__(self, solid: Rgb, selected: Rgb, active: Rgb) -> None:
        """Bind the member's steps to their named fields."""
        self.solid, self.selected, self.active = solid, selected, active


class Typography(Enum):
    """Faces by text role, each by its file in the user font folder and the family name applications resolve it by."""

    file: str
    family: str

    INTERFACE = ("Geist[wght].ttf", "Geist")
    MONOSPACE = ("GeistMono[wght].ttf", "Geist Mono")

    def __init__(self, file: str, family: str) -> None:
        """Bind the member's tuple to its named fields."""
        self.file, self.family = file, family

    @property
    def path(self) -> Path:
        """The face's file under the user font folder, where CoreText, Eto, and Blender read it."""
        return next(font for font in (Path.home() / "Library" / "Fonts").rglob("*.ttf") if font.name == self.file)


# --- [OPERATIONS] -----------------------------------------------------------------------


def blend(top: Rgb, bottom: Rgb, alpha: float) -> Rgb:
    """Color of `top` drawn at `alpha` over `bottom`."""
    red, green, blue = (round(upper * alpha + lower * (1 - alpha)) for upper, lower in zip(top, bottom, strict=True))
    return (red, green, blue)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DISABLED_ALPHA",
    "FACE_FILL_ALPHA",
    "FRAME_NODE_ALPHA",
    "GRID_MAJOR_ALPHA",
    "GRID_MINOR_ALPHA",
    "SELECTION_FILL_ALPHA",
    "TEXT_WEIGHT",
    "Accent",
    "Axis",
    "Field",
    "Guide",
    "Line",
    "OffScale",
    "Selection",
    "Status",
    "Surface",
    "Tag",
    "Text",
    "Typography",
    "blend",
]
