"""Region and widget theme colors and interface metrics."""

from collections.abc import Iterable
from enum import auto, Flag
import math
from typing import ClassVar, Self, TYPE_CHECKING

import bpy
from expression import Option
import mathutils
import msgspec
import numpy as np

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Color = tuple[float, float, float, float]


class State(Flag):
    """Widget states a drawn part imitates."""

    NONE = 0
    HOVER = auto()
    SELECT = auto()
    DEFAULT = auto()
    INACTIVE = auto()


class Field(Flag):
    """Property states that tint a field's fill."""

    NONE = 0
    CHANGED = auto()
    KEY = auto()
    ANIM = auto()
    DRIVEN = auto()
    OVERRIDDEN = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Widget(msgspec.Struct, frozen=True, kw_only=True):
    """Display colors and shade offsets of widget state."""

    inner: Color
    outline: Color
    item: Color
    text: Color
    shadetop: int
    shadedown: int


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
        """Reads metrics from running preferences."""
        preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return cls(scale=preferences.system.ui_scale, pixel_size=int(preferences.system.pixel_size), preferences=preferences)

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


def widget(colors: bpy.types.ThemeWidgetColors, state: State = State.NONE, field: Field = Field.NONE) -> Widget:
    """Derives Blender's widget colors and shades for interface and property states."""
    ui = bpy.context.preferences.themes[0].user_interface  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    states = ui.wcol_state
    factor = 0.5 if State.INACTIVE in state else 1.0
    slider = colors == ui.wcol_numslider
    reciprocal = np.float32(1) / 255

    type Bytes = tuple[int, int, int, int]

    def byte(value: float | np.float32) -> int:
        """Quantizes unit float by Blender's float32 byte rule."""
        channel = np.float32(value)
        return 0 if channel <= 0 else 255 if channel > np.float32(1) - np.float32(0.5) / 255 else int(channel * 255 + np.float32(0.5))

    def gray(rgb: Bytes) -> int:
        """Computes Blender's integer grayscale byte."""
        return (54 * rgb[0] + 182 * rgb[1] + 19 * rgb[2]) // 255

    def hsl(rgba: Bytes, saturation: float, lightness: float) -> Bytes:
        """Scales HSL saturation and lightness, keeping alpha."""
        red, green, blue = (np.float32(channel) * reciprocal for channel in rgba[:3])
        high, low = max(red, green, blue), min(red, green, blue)
        light, delta = min(np.float32(1), (high + low) / 2), high - low
        if high == low:
            hue, sat = np.float32(0), np.float32(0)
        else:
            sat = delta / (2 - high - low) if light > np.float32(0.5) else delta / (high + low)
            hue = ((green - blue) / delta + (6 if green < blue else 0) if high == red else (blue - red) / delta + 2 if high == green else (red - green) / delta + 4) / 6
        shifted = light * np.float32(lightness)
        sat *= np.float32(saturation)
        chroma = (1 - abs(2 * shifted - 1)) * sat
        red_byte, green_byte, blue_byte = (
            byte((np.minimum(np.maximum(channel, 0), 1) - np.float32(0.5)) * chroma + shifted) for channel in (abs(hue * 6 - 3) - 1, 2 - abs(hue * 6 - 2), 2 - abs(hue * 6 - 4))
        )
        return (red_byte, green_byte, blue_byte, rgba[3])

    def faded(values: mathutils.Color | bpy.types.bpy_prop_array) -> Bytes:
        """Converts to bytes, inactive factor scaling alpha."""
        red, green, blue, alpha = map(byte, color(values))
        return (red, green, blue, int(alpha * factor))

    def blend(a: Bytes, b: Iterable[int], f: float) -> Bytes:
        """Blends RGB through Blender's float32 rule, keeping alpha."""
        weight = np.float32(f)
        red, green, blue = (int((1 - weight) * x + weight * y) for x, y in zip(a[:3], b))
        return (red, green, blue, a[3])

    def contrasted(item: Bytes, inner: Bytes) -> Bytes:
        """Shifts item channels to Blender's slider contrast."""
        delta = gray(item) - gray(inner)
        shift = max(0, 30 - delta) if delta >= 0 else min(0, -30 - delta)
        red, green, blue = (min(max(channel + shift, 0), 255) for channel in item[:3])
        return (red, green, blue, item[3])

    def unit(rgba: Bytes) -> Color:
        """Normalizes bytes by Blender's float32 reciprocal."""
        red, green, blue, alpha = (float(channel * reciprocal) for channel in rgba)
        return (red, green, blue, alpha)

    tints = (
        (Field.CHANGED, states.inner_changed_sel),
        (Field.KEY, states.inner_key_sel),
        (Field.ANIM, states.inner_anim_sel),
        (Field.DRIVEN, states.inner_driven_sel),
        (Field.OVERRIDDEN, states.inner_overridden_sel),
    )
    tint = Option.of_optional(next((stored for member, stored in tints if member in field), None)).map(lambda stored: tuple(map(byte, (stored.r, stored.g, stored.b))))
    inner, outline, text = (
        faded(stored) for stored in ((colors.inner_sel, colors.outline_sel, colors.text_sel) if state & (State.SELECT | State.DEFAULT) else (colors.inner, colors.outline, colors.text))
    )
    hovered = State.HOVER in state and State.SELECT not in state
    tinted = tint.map(lambda shade: blend(inner, shade, states.blend)).default_value(inner)
    dark = gray(text) > gray(tinted)
    fill = hsl(tinted, 1.15, 1.2 if dark else 1.1) if hovered else tinted
    item = faded(colors.item)
    shades = (0, 0) if not colors.show_shaded else (colors.shadedown, colors.shadetop) if State.SELECT in state and not slider else (colors.shadetop, colors.shadedown)
    if hovered:
        mix = byte(0.5)
        red, green, blue, alpha = ((mix * x + (255 - mix) * y) // 256 for x, y in zip(outline, faded(colors.outline_sel)))
        outline = (red, green, blue, alpha)
    level = gray(item)
    return Widget(
        inner=unit(fill),
        outline=unit(outline),
        item=unit(tint.filter(lambda _: slider).map(lambda shade: contrasted(blend((level, level, level, item[3]), shade, states.blend), fill)).default_value(item)),
        text=unit(hsl(text, 1.15, 1.25 if dark else 0.8) if hovered else text),
        shadetop=shades[0],
        shadedown=shades[1],
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Checker", "Color", "Field", "Metrics", "State", "StyleSpacing", "Theme", "Widget", "color", "widget"]
