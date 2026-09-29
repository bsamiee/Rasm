# ruff: file-ignore[import-private-name, suspicious-xml-element-tree-usage, suspicious-xml-etree-import]
# ty: ignore[unresolved-attribute]
# mypy: disable-error-code="union-attr"
"""Blender's interface theme preset, the theme file's role placeholders rendered as bytes beside the members Blender's draw rules solve."""

from collections.abc import Iterator
from functools import partial
from io import StringIO
from itertools import chain
from math import ceil, floor, sqrt, sumprod
from pathlib import Path
from statistics import fmean
from types import SimpleNamespace
from xml.etree.ElementTree import Element, fromstring

from _rna_xml import rna2xml, xml_file_run
from bl_ui.space_userpref import USERPREF_MT_interface_theme_presets
import bpy
from mathutils import Color

from interface.report import changes, converged, Row, subscript
from interface.roles import Accent, Alpha, Axis, blend, Line, POINT_WIDTH, substituted, Surface, Text

# --- [OPERATIONS] -----------------------------------------------------------------------


def solved(system: bpy.types.PreferencesSystem) -> SimpleNamespace:
    """Theme members Blender's draw rules solve from the roles by member name: grids, floor axis brightness, menu row fill, list outline, and point sizes at the system's pixel size and interface scale."""

    def unlifted(rgb: tuple[int, int, int], shade: int) -> tuple[int, int, int]:
        """Theme color the 3D view draws as `rgb` after adding `shade` and lifting the sum by 255 x srgb_to_linear(b / 255)^(1 / 2.2), each channel at its nearest byte."""
        red, green, blue = (max(0, round(channel * 255) - shade) for channel in Color(tuple((channel / 255) ** 2.2 for channel in rgb)).from_scene_linear_to_srgb()[:])
        return (red, green, blue)

    minor, major = (blend(Line.GRID, Surface.CANVAS, alpha) for alpha in (Alpha.GRID_MINOR, Alpha.GRID_MAJOR))
    grid = unlifted(minor, 10)
    offset = round(fmean(role - floor(0.15 * ground + 0.85 * role) for axis in (Axis.X, Axis.Y) for role, ground in zip(axis, grid, strict=True)))
    slopes = [10 - low + ground for low, ground in zip(minor, Surface.CANVAS, strict=True)]
    rises = [high - 2 * low + ground for low, high, ground in zip(minor, major, Surface.CANVAS, strict=True)]
    image_alpha = round(255 * sumprod(rises, slopes) / sumprod(slopes, slopes)) / 255
    image_red, image_green, image_blue = (round(ground + (low - ground) / image_alpha - 10) for low, ground in zip(minor, Surface.CANVAS, strict=True))
    menu_red, menu_green, menu_blue = (ceil((hover - 0.2 * text) / 0.8) for hover, text in zip(Surface.HOVER, Text.PRIMARY, strict=True))
    pixel, scale = system.pixel_size, system.ui_scale
    return SimpleNamespace(
        canvas=unlifted(Surface.CANVAS, 0),
        grid=grid,
        grid_major=unlifted(major, 20),
        grid_axis_brightness=f"{(510 + 2 * offset + (1 if offset >= 0 else -1)) / 1020:.6g}",
        image_grid=(image_red, image_green, image_blue),
        image_grid_alpha=image_alpha,
        menu_inner=(menu_red, menu_green, menu_blue),
        list_outline=blend(Accent.ROW_SELECTED, Accent.ROW_ACTIVE, 2),
        vertex_size=round(POINT_WIDTH / (sqrt(2) * pixel)),
        gp_vertex_size=round(POINT_WIDTH / (2 * pixel)),
        facedot_size=round(POINT_WIDTH / pixel),
        object_origin_size=round((POINT_WIDTH + 1) / pixel - 1),
        graph_vertex_size=round((POINT_WIDTH + 1) / scale),
        image_vertex_size=round(((POINT_WIDTH + 2.5) / sqrt(2) - 1.5) / scale),
        image_facedot_size=round(POINT_WIDTH / scale),
        handle_vertex_size=round((POINT_WIDTH + 1) / (1.4 * scale)),
    )


def imported_theme(context: bpy.types.Context) -> Iterator[str]:
    """Change line per attribute of Blender's theme and text style export whose held value differs from the factory value or the rendered preset's value over it, the preset imported over the factory theme when any line exists and the held state restored otherwise."""
    menu = USERPREF_MT_interface_theme_presets
    owners = {tag: path for path, tag in menu.preset_xml_map}
    preset = Path(bpy.utils.user_resource("SCRIPTS", path=f"presets/{menu.preset_subdir}", create=True), "Interface.xml")

    def spelled(value: object) -> str:
        """Placeholder value in the file's spelling, a role as `#rrggbb`, an alpha as its byte in two hex digits, and a solved size or factor as its text."""
        match value:
            case (red, green, blue):
                return f"#{red:02x}{green:02x}{blue:02x}"
            case float():
                return f"{round(value * 255):02x}"
            case _:
                return str(value)

    def attributes(element: Element, label: str) -> Iterator[tuple[str, str]]:
        """Each attribute of the element and the elements it nests, labeled by its RNA path under the label."""
        properties = getattr(bpy.types, element.tag).bl_rna.properties
        yield from ((f"{label}.{name}", value) for name, value in element.attrib.items())
        for member in element:
            indexed = properties[member.tag].type == "COLLECTION"
            yield from chain.from_iterable(attributes(item, f"{label}.{member.tag}[{index}]" if indexed else f"{label}.{member.tag}") for index, item in enumerate(member))

    def read(text: str) -> dict[str, str]:
        """Attributes of a preset document by RNA path, each root element under the owner the menu maps its tag to."""
        return dict(chain.from_iterable(attributes(element, owners[element.tag]) for element in fromstring(text)))

    def exported() -> str:
        """Blender's export of the theme and text styles the menu maps, as one preset document."""
        export = StringIO()
        for path, _ in menu.preset_xml_map:
            rna2xml(fw=export.write, root_rna=context.path_resolve(path), method="ATTR")
        return f"<bpy>{export.getvalue()}</bpy>"

    def stored_preset() -> str | None:
        """Text of the preset file, None while none is written."""
        try:
            return preset.read_text(encoding="utf-8")
        except FileNotFoundError:
            return None

    theme, text, held = context.preferences.themes[0], substituted(Path(__file__).with_name("theme.xml").read_text(encoding="utf-8"), spelled, solved=solved(context.preferences.system)), exported()
    source = theme.filepath
    bpy.ops.preferences.reset_default_theme()
    before, wanted = read(held), {**read(exported()), **read(text)}
    lines = tuple(chain.from_iterable(changes(label, before[label], value) for label, value in wanted.items()))
    yield from converged(Row(label=subscript("presets", f"{menu.preset_subdir}/{preset.name}"), read=stored_preset, write=partial(preset.write_text, encoding="utf-8"), target=text))
    if lines:
        bpy.ops.script.execute_preset(filepath=str(preset), menu_idname=menu.__name__)
    else:
        xml_file_run(context, StringIO(held), menu.preset_xml_map, menu.preset_xml_secure_types)
        theme.filepath = source
    yield from lines


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["imported_theme"]
