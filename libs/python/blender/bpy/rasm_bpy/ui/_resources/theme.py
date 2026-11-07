"""Theme colors, region overlap, and interface metrics."""

from collections.abc import Iterable
from enum import IntEnum
import math
from typing import Final, Self, TYPE_CHECKING

import bpy
import mathutils
import msgspec
import numpy as np

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Color = tuple[float, float, float, float]
type Shade = mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]


class Spacing(IntEnum):
    """Unscaled interface style spacings no RNA member exposes, truncated to device pixels after interface scaling."""

    COLUMN = 8
    TEMPLATE = 5
    BOX = 5
    BUTTON_X = 8
    BUTTON_Y = 2
    PANEL = 8
    PANEL_OUTER = 4


# --- [CONSTANTS] ------------------------------------------------------------------------

_CLEAR: Final[Color] = (0.0, 0.0, 0.0, 0.0)

# --- [MODELS] ---------------------------------------------------------------------------


class Checker(msgspec.Struct, frozen=True, kw_only=True):
    """Transparency checker, cell size in device pixels."""

    primary: Color
    secondary: Color
    cell: int


class Metrics(msgspec.Struct, frozen=True, kw_only=True):
    """Interface scale and pixel size with Blender's derived sizes."""

    # --- [STATE]

    scale: float
    pixel_size: int
    preferences: bpy.types.Preferences

    # --- [READS]

    @classmethod
    def read(cls) -> Self:
        """Reads metrics from running preferences, the view scale while no window gives the display scale, which then reads 0."""
        preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        display = preferences.system.ui_scale
        return cls(scale=display if display > 0.0 else preferences.view.ui_scale, pixel_size=int(preferences.system.pixel_size), preferences=preferences)

    # --- [SIZES]

    def scaled(self, value: float) -> np.float32:
        """Multiplies by interface scale in float32."""
        return np.float32(self.scale) * np.float32(value)

    def spaced(self, spacing: Spacing) -> int:
        """Returns a style spacing in device pixels."""
        return int(self.scaled(spacing))

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
    def margin(self) -> float:
        """Panel inset from a region edge in device pixels, 0.4 widget units."""
        return self.unit * 0.4

    @property
    def preview(self) -> float:
        """Height in device pixels `template_preview` opens a preview at, 7.6 widget units."""
        return self.unit * 7.6

    def line(self, width: float) -> float:
        """Returns device-pixel line width."""
        return max(1.0, float(np.float32(width) * self.pixel_size))

    # --- [STYLES]

    def font(self, style: bpy.types.ThemeFontStyle, factor: float = 1.0) -> float:
        """Returns a theme font style's size times `factor` in device pixels."""
        return float(self.scaled(style.points * factor))

    def checker(self) -> Checker:
        """Reads transparency checker."""
        ui = self.preferences.themes[0].user_interface
        return Checker(primary=_color(ui.transparent_checker_primary), secondary=_color(ui.transparent_checker_secondary), cell=ui.transparent_checker_size * self.pixel_size)


class Theme(msgspec.Struct, frozen=True, kw_only=True):
    """Theme colors of one editor region."""

    # --- [STATE]

    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems"
    preferences: bpy.types.Preferences

    # --- [READS]

    @property
    def editor(self) -> bpy.types.ThemeSpaceGeneric | bpy.types.ThemeSpaceGradient:
        """Editor theme space, Project Settings reading Preferences and the 3D View answering a space the theme lacks."""
        theme = self.preferences.themes[0]
        member = "preferences" if self.space == "PROJECT" else self.space.lower()
        match getattr(theme, member).space if member in theme.bl_rna.properties else None:
            case bpy.types.ThemeSpaceGeneric() | bpy.types.ThemeSpaceGradient() as space:
                return space
            case _:
                return theme.view_3d.space

    @property
    def text(self) -> Color:
        """Text color."""
        theme = self.preferences.themes[0]
        match self.space, self.region:
            case (_, "UI" | "TOOLS") | ("PROPERTIES" | "PREFERENCES" | "PROJECT", _):
                return _color(theme.user_interface.panel_text)
            case _, "CHANNELS":
                return _color(theme.regions.channels.text)
            case _:
                return _color(self.editor.text)

    @property
    def text_hi(self) -> Color:
        """Highlighted text color."""
        regions = self.preferences.themes[0].regions
        return _color(regions.channels.text_selected if self.region == "CHANNELS" else self.editor.text_hi)

    @property
    def back(self) -> Color:
        """Region background display color, opaque unless the region overlaps."""
        theme, space = self.preferences.themes[0], self.editor
        stored: Shade
        match self.space, self.region:
            case "SEQUENCE_EDITOR", "PREVIEW":
                stored = theme.sequence_editor.preview_back
            case _, "WINDOW" | "PREVIEW":
                stored = space.gradients.high_gradient if isinstance(space, bpy.types.ThemeSpaceGradient) else space.back
            case _, "HEADER" | "FOOTER" | "TOOL_HEADER":
                stored = space.header  # ty: ignore[unsound-assignment]
            case _, "CHANNELS":
                stored = theme.regions.channels.back
            case _, "NAVIGATION_BAR":
                stored = theme.regions.sidebars.tab_back  # ty: ignore[unsound-assignment]
            case _, "ASSET_SHELF":
                stored = theme.regions.asset_shelf.back  # ty: ignore[unsound-assignment]
            case _, "ASSET_SHELF_HEADER":
                stored = theme.regions.asset_shelf.header_back  # ty: ignore[unsound-assignment]
            case _:
                stored = theme.regions.sidebars.back  # ty: ignore[unsound-assignment]
        red, green, blue, alpha = _color(stored)
        return (red, green, blue, alpha if _overlaps(self.preferences, self.space, self.region) else 1.0)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _overlaps(preferences: bpy.types.Preferences, space: "rna_enums.SpaceTypeItems", region: "rna_enums.RegionTypeItems") -> bool:
    """Returns whether a region of the editor draws over its window region under the region overlap preference and the 3D View header alpha."""
    match space, region:
        case _, "HUD":
            return True
        case _ if not preferences.system.use_region_overlap:
            return False
        case "VIEW_3D", "HEADER":
            return _color(preferences.themes[0].view_3d.space.header)[3] < 1.0
        case "NODE_EDITOR", "TOOLS" | "UI" | "ASSET_SHELF" | "ASSET_SHELF_HEADER":
            return True
        case "VIEW_3D" | "IMAGE_EDITOR", "TOOLS" | "UI" | "TOOL_PROPS" | "FOOTER" | "TOOL_HEADER" | "ASSET_SHELF" | "ASSET_SHELF_HEADER":
            return True
        case _:
            return False


def _color(values: Shade) -> Color:
    """Returns RGB or RGBA as display RGBA, alpha 1 when absent."""
    red, green, blue, *alpha = values  # type: ignore[misc]  # ty: ignore[not-iterable]
    return (red, green, blue, alpha[0] if alpha else 1.0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Checker", "Color", "Metrics", "Shade", "Spacing", "Theme"]
