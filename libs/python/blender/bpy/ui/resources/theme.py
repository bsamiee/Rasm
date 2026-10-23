"""Region and widget theme colors and interface metrics."""

from collections.abc import Iterable
import math
from typing import ClassVar, Self, TYPE_CHECKING

import bpy
import mathutils
import msgspec
import numpy as np

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Color = tuple[float, float, float, float]


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

    def text(self, style: bpy.types.ThemeFontStyle) -> float:
        """Returns text style font size in device pixels."""
        return float(self.scaled(style.points))

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


# --- [OPERATIONS] -----------------------------------------------------------------------


def color(values: mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]) -> Color:
    """Returns RGB or RGBA as display RGBA, alpha 1 when absent."""
    match values:
        case mathutils.Color():
            return (values.r, values.g, values.b, 1.0)
        case _:
            red, green, blue, *alpha = values  # type: ignore[misc]  # ty: ignore[not-iterable]
            return (red, green, blue, alpha[0] if alpha else 1.0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Checker", "Color", "Metrics", "StyleSpacing", "Theme", "color"]
