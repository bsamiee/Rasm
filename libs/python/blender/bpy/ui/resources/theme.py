"""Region and widget theme colors, interface metrics, and the appearance file Rasm.UI shares."""

from collections.abc import Iterable
import functools
import importlib.resources
import math
import re
from typing import ClassVar, get_args, Literal, Self, TYPE_CHECKING

import bpy
import mathutils
import msgspec
import numpy as np

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Color = tuple[float, float, float, float]
type Channel = Literal["fill", "edge", "text"]
type Ink = Color | HostInk | DataInk

# --- [CONSTANTS] ------------------------------------------------------------------------

CLEAR: Color = (0.0, 0.0, 0.0, 0.0)
CHANNELS: frozenset[str] = frozenset(get_args(Channel.__value__))
HEX: re.Pattern[str] = re.compile(r"#[0-9A-Fa-f]{6}(?:[0-9A-Fa-f]{2})?")


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

    def sized(self, role: str, metric: str) -> float:
        """Returns a shared metric in device pixels, ``font-size`` scaling the widget font, ``stroke`` through ``line``, lengths by interface scale."""
        value = Shipped.read().metric(role, metric)
        match metric:
            case "font-size":
                return float(self.scaled(value * self.preferences.ui_styles[0].widget.points))
            case "stroke":
                return self.line(value)
            case _:
                return float(self.scaled(value))

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


class HostInk(msgspec.Struct, frozen=True, kw_only=True):
    """Host chrome ink, the resolving role's own channel without a role."""

    role: str | None
    channel: str
    alpha: float


class DataInk(msgspec.Struct, frozen=True, kw_only=True):
    """Ink the drawn mark's data series supplies."""

    alpha: float


class Solid(msgspec.Struct, frozen=True, tag_field="kind", tag="solid"):
    """Solid fill material."""

    ink: str


class Stroke(msgspec.Struct, frozen=True, kw_only=True):
    """Line ink of a look."""

    ink: str


class Shadow(msgspec.Struct, frozen=True, kw_only=True):
    """Shadow ink, its offset, blur, and spread drawn by each host's own convention."""

    ink: str


class Look(msgspec.Struct, frozen=True, kw_only=True):
    """Shared appearance row, the parts Blender draws, with the rows it scopes onto its subtree."""

    fills: tuple[Solid, ...] = ()
    strokes: tuple[Stroke, ...] = ()
    text: str | None = None
    shadows: tuple[Shadow, ...] = ()
    within: dict[str, "Look"] = {}

    def color(self, channel: Channel) -> Color:
        """Returns a channel's first fixed ink, clear where the row states none."""
        match channel:
            case "fill":
                return fixed(self.fills[0].ink if self.fills else None)
            case "edge":
                return fixed(self.strokes[0].ink if self.strokes else None)
            case "text":
                return fixed(self.text)


class Native(msgspec.Struct, frozen=True, kw_only=True, rename="kebab"):
    """Native rows Blender draws, the file's other rows left undecoded."""

    well: Look
    overlay_caption: Look
    clip_over: Look
    clip_under: Look


class Shipped(msgspec.Struct, frozen=True, kw_only=True):
    """Native rows, metric defaults, and regular metrics of the appearance file Rasm.UI embeds."""

    native: Native
    defaults: dict[str, float]
    regular: dict[str, dict[str, float]]

    @classmethod
    @functools.cache
    def read(cls) -> Self:
        """Reads the packaged appearance file once."""
        return msgspec.json.decode(importlib.resources.files(__name__).joinpath("appearances.json").read_bytes(), type=cls)

    def metric(self, role: str, metric: str) -> float:
        """Returns a role's regular metric in points, the file's default where the row names none."""
        return self.regular.get(role, {}).get(metric, self.defaults[metric])


# --- [OPERATIONS] -----------------------------------------------------------------------


def ink(text: str) -> Ink:
    """Parses ``#RRGGBB[AA]``, ``[role.]channel[/alpha]``, or ``data[/alpha]`` with alpha in [0, 1], the grammar Rasm.UI reads."""
    reference, slash, written = text.partition("/")
    scale = float(written) if slash else 1.0
    match reference.split(".") if 0.0 <= scale <= 1.0 else []:
        case [hexed] if not slash and HEX.fullmatch(hexed):
            red, green, blue, *opacity = bytes.fromhex(hexed[1:])
            return (red / 255.0, green / 255.0, blue / 255.0, opacity[0] / 255.0 if opacity else 1.0)
        case ["data"]:
            return DataInk(alpha=scale)
        case [channel] if channel in CHANNELS:
            return HostInk(role=None, channel=channel, alpha=scale)
        case [role, channel] if role and channel in CHANNELS:
            return HostInk(role=role, channel=channel, alpha=scale)
        case _:
            raise ValueError(f"ink {text!r} is neither #RRGGBB[AA], [role.]channel[/alpha], nor data[/alpha] with alpha in [0, 1]")


def fixed(text: str | None) -> Color:
    """Returns a fixed ink as RGBA, clear for an absent one."""
    match None if text is None else ink(text):
        case None:
            return CLEAR
        case HostInk() | DataInk() as held:
            raise TypeError(f"{held!r} resolves through host chrome or series data, not the shared file")
        case color:
            return color


def color(values: mathutils.Color | bpy.types.bpy_prop_array | Iterable[float]) -> Color:
    """Returns RGB or RGBA as display RGBA, alpha 1 when absent."""
    match values:
        case mathutils.Color():
            return (values.r, values.g, values.b, 1.0)
        case _:
            red, green, blue, *alpha = values  # type: ignore[misc]  # ty: ignore[not-iterable]
            return (red, green, blue, alpha[0] if alpha else 1.0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLEAR", "Channel", "Checker", "Color", "DataInk", "HostInk", "Ink", "Look", "Metrics", "Native", "Shadow", "Shipped", "Solid", "Stroke", "StyleSpacing", "Theme", "color", "fixed", "ink"]
