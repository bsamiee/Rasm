"""Resource rows: staged glyphs with a brand, the preview collection, and the texture cache, with probes over icons, previews, textures, theme colors, and metrics."""

from collections.abc import Callable, Mapping, Sequence
from pathlib import Path
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Option, Some, try_downcast
import mathutils
import numpy as np

from blender.bpy.data.rna.paths import definition
from blender.bpy.extension.identity import Identity
from blender.bpy.ui.resources.icons import ICONS, Icons
from blender.bpy.ui.resources.previews import PreviewKey, PREVIEWS
from blender.bpy.ui.resources.textures import TEXTURES, uploaded
from blender.bpy.ui.resources.theme import Checker, color, Metrics, Theme

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

MARK: Final = Path("mark.svg")
_OPAQUE: Final = 0.99

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]


def _preferences() -> bpy.types.Preferences:
    """Returns the running preferences."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return preferences


def _opaque(identity: Identity) -> tuple[int, float]:
    """Counts the mark's opaque icon pixels and their largest distance from the widget text color."""
    preview = identity.held[ICONS].collection[MARK.as_posix()]
    rgba = np.asarray(preview.icon_pixels_float, np.float32).reshape(-1, 4)
    opaque = rgba[rgba[:, 3] > _OPAQUE]
    text = np.asarray(color(_preferences().themes[0].user_interface.wcol_regular.text)[:3], np.float32)
    return len(opaque), float(np.abs(opaque[:, :3] - text).max(initial=0.0))


# --- [PROBES]


def glyphs(identity: Identity) -> dict[str, object]:
    """Reads the brand icon and the values of a staged glyph, a built-in icon, a stub icon name the running build lacks, and a glyph the staged set lacks, a data-block icon left out since its preview render job crashes a background session."""
    icons = identity.held[ICONS]
    stock = Option.of_optional(try_downcast(bpy.types.EnumProperty, definition(bpy.types.UILayout).functions["prop"].parameters["icon"])).map(lambda prop: prop.enum_items["ADD"].value)
    return {
        "brand": icons.brand.is_some(),
        "staged": Icons.value(identity, MARK).map(lambda value: Some(value) == icons.brand),
        "built in": Icons.value(identity, "ADD").map(lambda value: Some(value) == stock),
        "unknown": Icons.value(identity, "BACKDROP_PAN"),
        "missing": Icons.value(identity, Path("missing.svg")),
        "tool path": Path(Icons.path(identity, Path("tools/brush.svg"))).parts[-3:],
    }


def tinted(identity: Identity) -> dict[str, object]:
    """Reads the mark's tint in the widget text color, retints it red twice, and restores the default tint."""
    icons = identity.held[ICONS]
    count, deviation = _opaque(identity)
    red = (icons.retinted(lambda _: Some((1.0, 0.0, 0.0, 1.0))), icons.retinted(lambda _: Some((1.0, 0.0, 0.0, 1.0))))
    restored = icons.retinted(Icons().tint)
    return {"opaque": count > 0, "deviation": deviation, "red": red, "restored": (restored, _opaque(identity)[1])}


def previewed(identity: Identity) -> dict[str, object]:
    """Writes a plane into a preview slot, reads it back, drops it with a pixelless request, and clears it."""
    previews = identity.held[PREVIEWS]
    plane = np.ones((4, 8, 4), np.float32)
    written = previews.written("probe", "first", Some(plane))
    size: Sequence[int] = previews.collection["probe"].image_size  # type: ignore[assignment]
    after_write = (previews.current("probe", "first"), previews.icon_id("probe") == written.to_option().bind(lambda held: held), tuple(size))
    emptied = previews.written("probe", "second", Nothing)
    after_empty = (previews.current("probe", "second"), previews.icon_id("probe"))
    cleared = previews.dropped("probe")
    return {
        "written": written.map(lambda held: held.is_some()),
        "after write": after_write,
        "emptied": emptied,
        "after empty": after_empty,
        "cleared": cleared,
        "after clear": previews.current("probe", "second"),
    }


def checkered(_identity: Identity) -> dict[str, object]:
    """Composites a transparent and an opaque plane over a two-pixel checker, and reads the default request side."""
    key = PreviewKey(key="probe", side=4, checker=Checker(primary=(1.0, 1.0, 1.0, 1.0), secondary=(0.0, 0.0, 0.0, 1.0), cell=2))
    opaque = np.zeros((4, 4, 4), np.float32)
    opaque[..., 0] = opaque[..., 3] = 1.0
    return {
        "transparent": key.checkered(np.zeros((4, 4, 4), np.float32))[..., 0],
        "opaque": key.checkered(opaque)[0, 0],
        "alpha": key.checkered(np.zeros((4, 4, 4), np.float32))[..., 3].min(),
        "side": PreviewKey.read("probe").side == bpy.app.render_preview_size,
    }


def textured(identity: Identity) -> dict[str, object]:
    """Uploads a plane and stores three textures past a limit of two with the first kept."""
    textures = identity.held[TEXTURES]
    plane = np.ones((3, 5, 4), np.float32)
    single = uploaded(plane).map(lambda texture: (texture.width, texture.height, texture.format))
    stored = tuple(textures.stored(key, plane, 2, frozenset({"first"})).map(lambda _: tuple(textures.held)) for key in ("first", "second", "third"))
    return {"uploaded": single, "stored": stored, "memory": textures.memory, "shown": textures.shown("first").is_some(), "order": tuple(textures.held)}


def themed(_identity: Identity) -> dict[str, object]:
    """Reads region theme colors beside the host theme members they follow."""
    theme = _preferences().themes[0]
    preferences = _preferences()

    def of(space: "rna_enums.SpaceTypeItems", region: "rna_enums.RegionTypeItems") -> Theme:
        return Theme(space=space, region=region, preferences=preferences)

    return {
        "view back": (of("VIEW_3D", "WINDOW").back, color(theme.view_3d.space.gradients.high_gradient)),
        "sidebar back": (of("VIEW_3D", "UI").back, color(theme.regions.sidebars.back)),
        "header back": (of("PROPERTIES", "HEADER").back, color(theme.properties.space.header)),
        "preview back": (of("SEQUENCE_EDITOR", "PREVIEW").back, color(theme.sequence_editor.preview_back)),
        "panel text": (of("VIEW_3D", "UI").text, color(theme.user_interface.panel_text)),
        "channel text": (of("DOPESHEET_EDITOR", "CHANNELS").text, color(theme.regions.channels.text)),
        "editor text": (of("TEXT_EDITOR", "WINDOW").text, color(theme.text_editor.space.text)),
        "project editor": of("PROJECT", "WINDOW").editor == theme.preferences.space,
        "absent editor": of("EMPTY", "WINDOW").editor == theme.view_3d.space,
    }


def measured(_identity: Identity) -> dict[str, object]:
    """Reads interface metrics beside the host's header heights and scales."""
    metrics = Metrics.read()
    preferences = _preferences()
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    headers = {region.height for area in manager.windows[0].screen.areas for region in area.regions if region.type == "HEADER"}
    return {
        "scale": (metrics.scale, preferences.system.ui_scale, preferences.view.ui_scale),
        "pixel": (metrics.pixel_size, preferences.system.pixel_size),
        "header": (metrics.header, tuple(headers)),
        "unit": metrics.unit,
        "panel header": metrics.panel_header,
        "line": (metrics.line(0.5), metrics.line(2.0)),
        "points": metrics.font(preferences.ui_styles[0].widget) == float(metrics.scaled(preferences.ui_styles[0].widget.points)),
        "checker": metrics.checker().cell == preferences.themes[0].user_interface.transparent_checker_size * metrics.pixel_size,
    }


def colored(_identity: Identity) -> dict[str, object]:
    """Reads display RGBA from a color value and from RGB and RGBA sequences."""
    return {"color": color(mathutils.Color((0.25, 0.5, 0.75))), "rgb": color((0.25, 0.5, 0.75)), "rgba": color((0.25, 0.5, 0.75, 0.5))}


# --- [COMPOSITION] ----------------------------------------------------------------------

rows: Final = (Icons(brand=Some(MARK)), TEXTURES)
PROBES: Final[Mapping[str, Callable[[Identity], object]]] = MappingProxyType({
    "glyphs": glyphs,
    "tinted": tinted,
    "previewed": previewed,
    "checkered": checkered,
    "textured": textured,
    "themed": themed,
    "measured": measured,
    "colored": colored,
})
RELEASED: Final[Mapping[str, Callable[[], object]]] = MappingProxyType({})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
