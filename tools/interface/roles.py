"""Color roles, alphas, typefaces, and the point width every application's interface takes, the palette they read, and the template text naming a role by dotted path."""

import cmath
from collections.abc import Callable
from enum import Enum
import math
from operator import attrgetter
from string import Template
from types import SimpleNamespace
from typing import Final
from uuid import NAMESPACE_URL, uuid5

# --- [CONSTANTS] ------------------------------------------------------------------------

FILLS: Final = (17, 26, 33, 50, 57, 62, 71, 82)
POINT_WIDTH: Final = 6

# --- [PALETTE] --------------------------------------------------------------------------


class Palette(Enum):
    """Twelve display colors of every family, darkest first."""

    @staticmethod
    def steps(hue: float, solid: float, chroma: float, dark_drift: float, light_drift: float) -> tuple[tuple[int, int, int], ...]:
        """Twelve display colors of the hue family, eight at the fill grays' OKLab lightness, then the solid, two steps beside it, and the top."""
        knee, top, pale, gamut_share = 0.0031308, 0.935, 0.8, 0.97

        def linear(lightness: float, ab: complex) -> tuple[float, ...]:
            cones = [
                math.sumprod(row, (lightness, ab.real, ab.imag)) ** 3
                for row in ((1.0, 0.3963377773761749, 0.2158037573099136), (1.0, -0.1055613458156586, -0.0638541728258133), (1.0, -0.0894841775298119, -1.2914855480194092))
            ]
            return tuple(math.sumprod(row, cones) for row in ((4.0767416621, -3.3077115913, 0.2309699292), (-1.2684380046, 2.6097574011, -0.3413193965), (-0.0041960863, -0.7034186147, 1.707614701)))

        def srgb(lightness: float, ab: complex) -> tuple[int, int, int]:
            low, high = 0.0, 1 / gamut_share
            while low < (mid := (low + high) / 2) < high:
                low, high = (mid, high) if all(0 <= channel <= 1 for channel in linear(lightness, mid * ab)) else (low, mid)
            red, green, blue = (round(255 * (12.92 * channel if channel <= knee else 1.055 * channel ** (1 / 2.4) - 0.055)) for channel in linear(lightness, gamut_share * low * ab))
            return (red, green, blue)

        fills = [math.cbrt(value / 12.92 if value <= 12.92 * knee else ((value + 0.055) / 1.055) ** 2.4) for value in (gray / 255 for gray in FILLS)]
        lightnesses = [*fills, solid, solid - 0.03, min(0.93, solid + 0.05), top] if solid >= pale else [*fills, solid, solid + 0.035, max(pale, solid + 0.06), top]
        return tuple(
            srgb(lightness, cmath.rect(share * chroma, math.radians(hue + (lightness - solid) * (dark_drift / (fills[0] - solid) if lightness < solid else light_drift / (top - solid)))))
            for lightness, share in zip(lightnesses, (0.1, 0.15, 0.34, 0.48, 0.56, 0.6, 0.64, 0.74, 1, 0.95, 0.8, 0.32), strict=True)
        )

    ROSE = steps(3, 0.7, 0.15, 4, -4)
    CRIMSON = steps(13, 0.53, 0.18, 0, 0)
    RED = steps(29, 0.62, 0.17, -6, 4)
    ORANGE = steps(53, 0.74, 0.14, -8, 6)
    YELLOW = steps(88, 0.85, 0.145, -16, 3)
    LIME = steps(121, 0.79, 0.15, -10, -4)
    GREEN = steps(144, 0.67, 0.145, 6, -4)
    EMERALD = steps(152, 0.57, 0.14, 0, 0)
    TEAL = steps(178, 0.71, 0.11, 4, -4)
    CYAN = steps(213, 0.76, 0.115, 4, -6)
    CERULEAN = steps(227, 0.54, 0.105, 0, 0)
    BLUE = steps(259, 0.62, 0.16, 6, -8)
    ULTRAMARINE = steps(280, 0.56, 0.175, 2, -6)
    VIOLET = steps(307, 0.63, 0.14, -2, 0)
    MAGENTA = steps(339, 0.66, 0.18, 2, -4)
    SLATE = steps(241, 0.62, 0.038, 0, 0)
    NEUTRAL = tuple((gray, gray, gray) for gray in (*FILLS, 96, 123, 180, 238))

    def __getitem__(self, step: int) -> tuple[int, int, int]:
        """Display color of the family's step, 1 the darkest."""
        return self.value[step - 1]


# --- [ROLES] ----------------------------------------------------------------------------


class Alpha:
    """Opacities of grids, fills, wires, marks, and theme overlays drawn over their ground."""

    GRID_MINOR: Final = 0.6
    GRID_MAJOR: Final = 1.0
    FACE_FILL: Final = 0.38
    SELECTION_FILL: Final = 0.12
    CROSSING_FILL: Final = 0.0
    DISABLED: Final = 0.4
    NULL_WIRE: Final = 0.5
    DISABLED_WIRE: Final = 0.3
    ZONE_FILL: Final = 0.2
    WIRE_SELECTED: Final = 0.5
    GLOW: Final = 0.35
    EDIT_FACE: Final = 2 / 255
    UV_FACE: Final = 10 / 255
    HAIRLINE: Final = 17 / 255
    VEIL: Final = 31 / 255
    ROW_ITEM: Final = 51 / 255
    UNSET_ACTION: Final = 77 / 255
    ACTIVE_ACTION: Final = 102 / 255
    EDITED_OBJECT: Final = 102 / 255
    PREVIEW_RANGE: Final = 102 / 255
    WIDGET_ITEM: Final = 128 / 255
    EDIT_WIRE: Final = 128 / 255
    LOCKED_WEIGHT: Final = 128 / 255
    STRIP_RANGE: Final = 128 / 255
    HELD_KEY: Final = 153 / 255
    TOOLBAR_ICON: Final = 179 / 255
    BACK_FACE: Final = 179 / 255
    MIXED_INTERPOLATION: Final = 179 / 255
    INTERPOLATION: Final = 204 / 255


class Ink:
    """Line ink of documents, black for layers and print, and of screens, white for every display member switching never reaches."""

    DOCUMENT: Final = (0, 0, 0)
    SCREEN: Final = (255, 255, 255)


class Surface:
    """Region fills in depth order, shaded bodies and their ambient light, the paper of layout sheets with a form field's rest fill, and the shadow of rims and veils."""

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
    FORM_FIELD: Final = Palette.NEUTRAL[12]
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
    """Stroke colors of borders, form fields, grids behind canvases, panels, and paper, datum grids a user creates, locked objects, edit marks, and timeline keys."""

    BORDER: Final = Palette.NEUTRAL[7]
    BORDER_HOVER: Final = Palette.NEUTRAL[8]
    FORM_FIELD: Final = Palette.NEUTRAL[11]
    GRID: Final = Palette.NEUTRAL[5]
    GRID_PANEL: Final = Palette.NEUTRAL[7]
    PAPER_GRID: Final = Palette.NEUTRAL[12]
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


class Node:
    """Node borders by state."""

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
    """Node wire colors, one core color for every data type."""

    CORE: Final = Palette.NEUTRAL[11]
    CASING: Final = Surface.CANVAS
    SELECTED: Final = Selection.ITEM
    GLOW: Final = Selection.BODY


class Tag(Enum):
    """User-assigned tag slots in Blender's slot order, the neutral slot last."""

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

    GEOMETRY = (Palette.CYAN[6], Palette.CYAN[9], Palette.CYAN[8], Palette.CYAN[11], Palette.CYAN[12])
    SCALAR = (Palette.NEUTRAL[6], Palette.NEUTRAL[9], Palette.NEUTRAL[8], Palette.NEUTRAL[11], Palette.NEUTRAL[12])
    VECTOR = (Palette.ULTRAMARINE[6], Palette.ULTRAMARINE[9], Palette.ULTRAMARINE[8], Palette.ULTRAMARINE[11], Palette.ULTRAMARINE[12])
    TRANSFORM = (Palette.VIOLET[6], Palette.VIOLET[9], Palette.VIOLET[8], Palette.VIOLET[11], Palette.VIOLET[12])
    TEXT = (Palette.BLUE[6], Palette.BLUE[11], Palette.BLUE[8], Palette.BLUE[11], Palette.BLUE[12])
    DATA = (Palette.SLATE[6], Palette.SLATE[9], Palette.SLATE[8], Palette.SLATE[11], Palette.SLATE[12])
    COLOR = (Palette.NEUTRAL[6], Palette.YELLOW[9], Palette.NEUTRAL[8], Palette.YELLOW[11], Palette.YELLOW[12])
    INPUT = (Palette.ROSE[6], Palette.ROSE[9], Palette.ROSE[8], Palette.ROSE[11], Palette.ROSE[12])
    OUTPUT = (Palette.RED[6], Palette.RED[9], Palette.RED[8], Palette.RED[11], Palette.RED[12])
    DISPLAY = (Palette.NEUTRAL[6], Palette.LIME[9], Palette.NEUTRAL[8], Palette.LIME[11], Palette.LIME[12])
    ANALYSIS = (Palette.NEUTRAL[6], Palette.ORANGE[9], Palette.NEUTRAL[8], Palette.ORANGE[11], Palette.ORANGE[12])

    def __init__(self, header: tuple[int, int, int], mark: tuple[int, int, int], border: tuple[int, int, int], token: tuple[int, int, int], shine: tuple[int, int, int]) -> None:
        """Bind the category's steps to their named fields."""
        self.header, self.mark, self.border, self.token, self.shine = header, mark, border, token, shine


class Typography(Enum):
    """Typefaces by text role, each a font file in the user font folder and its family name."""

    INTERFACE = ("Geist[wght].ttf", "Geist")
    MONOSPACE = ("GeistMono[wght].ttf", "Geist Mono")

    def __init__(self, file: str, family: str) -> None:
        """Bind the member's file and family to their named fields."""
        self.file, self.family = file, family


# --- [OPERATIONS] -----------------------------------------------------------------------


def blend(top: tuple[int, int, int], bottom: tuple[int, int, int], alpha: float) -> tuple[int, int, int]:
    """Color of `top` drawn at `alpha` over `bottom`, an alpha above 1 the color that draws `top` at `1 / alpha` over `bottom`."""
    red, green, blue = (round(upper * alpha + lower * (1 - alpha)) for upper, lower in zip(top, bottom, strict=True))
    return (red, green, blue)


def substituted[T](template: str, text: Callable[[T], str], **values: T) -> str:
    """Template text with each `$dotted.path` placeholder replaced by the text of the run-time value or role it names."""

    class Dotted(Template):
        idpattern = r"(?a:[_a-z]\w*(?:\.[_a-z]\w*)*)"

    dotted, names = Dotted(template), SimpleNamespace(**{name: globals()[name] for name in __all__}, **values)
    return dotted.substitute({name: text(attrgetter(name)(names)) for name in dotted.get_identifiers()})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "POINT_WIDTH",
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
    "Selection",
    "Status",
    "Surface",
    "Tag",
    "Text",
    "Typography",
    "Wire",
    "blend",
    "substituted",
]
