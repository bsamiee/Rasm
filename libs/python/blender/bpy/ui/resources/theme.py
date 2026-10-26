"""Region and widget theme colors, interface metrics, and the appearance file Rasm.UI shares."""

from collections.abc import Iterable
import functools
import importlib.resources
import math
from typing import Annotated, ClassVar, Literal, Self, TYPE_CHECKING

import bpy
from expression import Option
from expression.collections import Block
import mathutils
import msgspec
import numpy as np

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Color = tuple[float, float, float, float]
type Hex = Annotated[str, msgspec.Meta(pattern=r"\A#[0-9a-fA-F]{6}(?:[0-9a-fA-F]{2})?\Z")]
type SkiaBlend = Literal["src-over", "plus"]

# --- [CONSTANTS] ------------------------------------------------------------------------

CLEAR: Color = (0.0, 0.0, 0.0, 0.0)


# --- [MODELS] ---------------------------------------------------------------------------


class Checker(msgspec.Struct, frozen=True, kw_only=True):
    """Transparency checker, cell size in device pixels."""

    primary: Color
    secondary: Color
    cell: int


class StyleSpacing(msgspec.Struct, frozen=True, kw_only=True):
    """Style spacings in device pixels."""

    column: int
    template: int
    box: int
    button_x: int
    button_y: int
    panel: int
    panel_outer: int


class Metrics(msgspec.Struct, frozen=True, kw_only=True):
    """Interface scale and pixel size with Blender's derived sizes."""

    scale: float
    pixel_size: int
    preferences: bpy.types.Preferences
    unscaled: ClassVar[StyleSpacing] = StyleSpacing(column=8, template=5, box=5, button_x=8, button_y=2, panel=8, panel_outer=4)

    # --- [READING]

    @classmethod
    def read(cls) -> Self:
        """Reads metrics from running preferences, the view scale while no window gives the display scale, which then reads 0."""
        preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        display = preferences.system.ui_scale
        return cls(scale=display if display > 0.0 else preferences.view.ui_scale, pixel_size=int(preferences.system.pixel_size), preferences=preferences)

    # --- [SIZING]

    def scaled(self, value: float) -> np.float32:
        """Multiplies by interface scale in float32."""
        return np.float32(self.scale) * np.float32(value)

    @property
    def unit(self) -> int:
        """Widget unit in device pixels."""
        return math.floor(float(self.scaled(18.0)) + 0.5) + 2 * self.pixel_size

    @property
    def header(self) -> int:
        """Area header height in device pixels."""
        return self.unit + int(self.scaled(6.0))

    @property
    def panel_header(self) -> int:
        """Panel header height in device pixels."""
        return int(self.unit * 1.25)

    @property
    def spacing(self) -> StyleSpacing:
        """Style spacings at interface scale."""
        return StyleSpacing(**{name: int(self.scaled(value)) for name, value in msgspec.structs.asdict(Metrics.unscaled).items()})

    def line(self, width: float) -> float:
        """Returns device-pixel line width."""
        return max(1.0, float(np.float32(width) * self.pixel_size))

    # --- [STYLING]

    def points(self, factor: float = 1.0) -> float:
        """Returns the widget font size times `factor` in device pixels."""
        return float(self.scaled(self.preferences.ui_styles[0].widget.points * factor))

    def checker(self) -> Checker:
        """Reads transparency checker."""
        ui = self.preferences.themes[0].user_interface
        return Checker(primary=color(ui.transparent_checker_primary), secondary=color(ui.transparent_checker_secondary), cell=ui.transparent_checker_size * self.pixel_size)


class Theme(msgspec.Struct, frozen=True, kw_only=True):
    """Theme colors of one editor region."""

    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems"
    preferences: bpy.types.Preferences

    # --- [READING]

    @property
    def editor(self) -> bpy.types.ThemeSpaceGeneric | bpy.types.ThemeSpaceGradient:
        """Editor theme space, 3D View space when theme lacks one."""
        theme = self.preferences.themes[0]
        member = "preferences" if self.space == "PROJECT" else self.space.lower()
        match theme.path_resolve(f"{member}.space") if member in theme.bl_rna.properties else None:
            case bpy.types.ThemeSpaceGeneric() | bpy.types.ThemeSpaceGradient() as space:
                return space
            case _:
                return theme.view_3d.space

    @property
    def overlaps(self) -> bool:
        """Whether region draws over window region."""
        match self.space, self.region:
            case _, "HUD":
                return True
            case _ if not self.preferences.system.use_region_overlap:
                return False
            case "VIEW_3D", "HEADER":
                return color(self.preferences.themes[0].view_3d.space.header)[3] < 1.0
            case "NODE_EDITOR", "TOOLS" | "UI" | "ASSET_SHELF" | "ASSET_SHELF_HEADER":
                return True
            case ("VIEW_3D" | "IMAGE_EDITOR", "TOOLS" | "UI" | "TOOL_PROPS" | "FOOTER" | "TOOL_HEADER" | "ASSET_SHELF" | "ASSET_SHELF_HEADER"):
                return True
            case _:
                return False

    @property
    def text(self) -> Color:
        """Text color by Blender's TH_TEXT rule."""
        theme = self.preferences.themes[0]
        match self.space, self.region:
            case (_, "UI" | "TOOLS") | ("PROPERTIES" | "PREFERENCES" | "PROJECT", _):
                return color(theme.user_interface.panel_text)
            case _, "CHANNELS":
                return color(theme.regions.channels.text)
            case _:
                return color(self.editor.text)

    @property
    def text_hi(self) -> Color:
        """Highlighted text color by Blender's TH_TEXT_HI rule."""
        regions = self.preferences.themes[0].regions
        return color(regions.channels.text_selected if self.region == "CHANNELS" else self.editor.text_hi)

    @property
    def back(self) -> Color:
        """Region background display color."""
        theme, space = self.preferences.themes[0], self.editor
        stored: mathutils.Color | bpy.types.bpy_prop_array
        match self.region:
            case "WINDOW":
                stored = space.gradients.high_gradient if isinstance(space, bpy.types.ThemeSpaceGradient) else space.back
            case "PREVIEW":
                stored = theme.sequence_editor.preview_back
            case "HEADER" | "FOOTER" | "TOOL_HEADER":
                stored = space.header
            case "CHANNELS":
                stored = theme.regions.channels.back
            case "NAVIGATION_BAR":
                stored = theme.regions.sidebars.tab_back
            case "ASSET_SHELF":
                stored = theme.regions.asset_shelf.back
            case "ASSET_SHELF_HEADER":
                stored = theme.regions.asset_shelf.header_back
            case _:
                stored = theme.regions.sidebars.back
        red, green, blue, alpha = color(stored)
        return (red, green, blue, alpha if self.overlaps else 1.0)


class Ink(msgspec.Struct, frozen=True, kw_only=True):
    """Part of a look in one fixed ink."""

    ink: Hex


class Solid(Ink, frozen=True, kw_only=True, tag_field="kind", tag="solid"):
    """Solid fill of a look in one fixed ink."""


class Stroke(Ink, frozen=True, kw_only=True):
    """Stroke without dash, `Canvas` drawing none."""

    dash: Annotated[tuple[tuple[float, ...], ...], msgspec.Meta(max_length=0)] = ()


class Shadow(Ink, frozen=True, kw_only=True):
    """Hard text shadow offset in points with y down, one per blf font."""

    blur: Annotated[float, msgspec.Meta(ge=0.0, le=0.0)]
    x: float = 0.0
    y: float = 0.0
    spread: Annotated[float, msgspec.Meta(ge=0.0, le=0.0)] = 0.0


class Look(msgspec.Struct, frozen=True, kw_only=True):
    """Shared appearance row in fixed inks, the parts Blender draws, with the rows it scopes onto its subtree decoded where read."""

    fills: tuple[Solid, ...] = ()
    strokes: tuple[Stroke, ...] = ()
    shadows: Annotated[tuple[Shadow, ...], msgspec.Meta(max_length=1)] = ()
    lettering: Hex | None = msgspec.field(default=None, name="text")
    blend: SkiaBlend = "src-over"
    within: dict[str, msgspec.Raw] = {}

    # --- [READING]

    @property
    def fill(self) -> Color:
        """First fill ink, clear where the row states none."""
        return next((rgba(part.ink) for part in self.fills), CLEAR)

    @property
    def edge(self) -> Color:
        """First stroke ink, clear where the row states none."""
        return next((rgba(part.ink) for part in self.strokes), CLEAR)

    @property
    def shadow(self) -> Option[Shadow]:
        """Text shadow, absent where the row states none."""
        return Block.of_seq(self.shadows).try_head()

    @property
    def text(self) -> Color:
        """Text ink, clear where the row states none."""
        return Option.of_optional(self.lettering).map(rgba).default_value(CLEAR)

    @property
    def mode(self) -> Literal["ALPHA", "ADDITIVE"]:
        """Blend mode drawing the row's Skia blend in `gpu` over straight-alpha inks."""
        match self.blend:
            case "src-over":
                return "ALPHA"
            case "plus":
                return "ADDITIVE"

    def scoped(self, role: str) -> Self:
        """Decodes the look this row scopes onto a role in its subtree, refusing host and data inks."""
        return msgspec.json.decode(self.within[role], type=type(self))


class Sizes(msgspec.Struct, frozen=True, kw_only=True, rename="kebab"):
    """Shared metrics of one role Blender draws, in points, font size as a factor of the widget font."""

    inset: float
    radius: float
    stroke: float
    font_size: float


class Shipped(msgspec.Struct, frozen=True, kw_only=True):
    """Appearances, metric defaults, and densities of the file Rasm.UI embeds, Blender drawing the native looks at regular density, each look decoded where read."""

    appearances: dict[str, dict[str, msgspec.Raw]]
    defaults: dict[str, float]
    densities: dict[str, dict[str, dict[str, float]]]

    @classmethod
    @functools.cache
    def read(cls) -> Self:
        """Reads the packaged appearance file once."""
        return msgspec.json.decode(importlib.resources.files(__name__).joinpath("appearances.json").read_bytes(), type=cls)

    def look(self, role: str) -> Look:
        """Decodes a native role's look, refusing host and data inks."""
        return msgspec.json.decode(self.appearances["native"][role], type=Look)

    def sizes(self, role: str) -> Sizes:
        """Returns a role's regular metrics over the file's defaults."""
        return msgspec.convert(self.defaults | self.densities["regular"].get(role, {}), type=Sizes)


# --- [OPERATIONS] -----------------------------------------------------------------------


def color(values: mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]) -> Color:
    """Returns RGB or RGBA as display RGBA, alpha 1 when absent."""
    match values:
        case mathutils.Color():
            return (values.r, values.g, values.b, 1.0)
        case _:
            red, green, blue, *alpha = values  # type: ignore[misc]  # ty: ignore[not-iterable]
            return (red, green, blue, alpha[0] if alpha else 1.0)


def rgba(ink: Hex) -> Color:
    """Returns `#RRGGBB[AA]` as display RGBA, alpha 1 when absent."""
    return color(byte / 255.0 for byte in bytes.fromhex(ink[1:]))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLEAR", "Checker", "Color", "Hex", "Ink", "Look", "Metrics", "Shadow", "Shipped", "Sizes", "SkiaBlend", "Solid", "Stroke", "StyleSpacing", "Theme", "color", "rgba"]
