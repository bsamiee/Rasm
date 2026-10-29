"""Layout probes: rows over bound members, placements, openers, labels, templates, and sub-layouts realized once under the extension identity, and the keyword checks they run against UILayout functions."""

from collections.abc import Callable, Mapping
from pathlib import Path
from types import MappingProxyType
from typing import Final

from bl_ui.space_view3d import VIEW3D_MT_add, VIEW3D_MT_pivot_pie
import bpy
from expression import Option, Some

from blender.bpy.commands.calls import Called, Opened, Placement
from blender.bpy.extension.identity import Identity
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import accepted, acquired, FUNCTIONS, Label, Layout, Opens, OperatorButton, Property, realized, RowContent, Status
from blender.bpy.ui.layout.templates import ImageFormat, Preview, Shortcut, wheel

from .commands import COUNTED, KEYS

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [READS]


def _drawn(identity: Identity, row: RowContent) -> object:
    """Realizes one row, answering whether a draw came back or the refusal reasons."""
    return realized((row,), identity).map(callable)


def _separated(layout: bpy.types.UILayout, _holder: object, _context: bpy.types.Context) -> None:
    """Draws a line separator, a plain draw as row content."""
    layout.separator(type="LINE")


# --- [PROBES]


def bound(identity: Identity) -> dict[str, object]:
    """Realizes property rows over present, absent, reached, and wrongly keyworded members."""
    return {
        "present": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), keywords={"slider": True})),
        "element": _drawn(identity, Property(target=Member(root=bpy.types.Object, path=("location", 0)))),
        "reached": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_end",), reach=Some(lambda context: Option.of_optional(context.scene))))),
        "absent": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("fixture_absent",)))),
        "keyword": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), keywords={"fixture": 1})),
        "glyph": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), icon=Some("ADD"))),
        "unknown glyph": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("frame_start",)), icon=Some(Path("fixture_absent.png")))),
        "function": _drawn(identity, Property(target=Member(root=bpy.types.Scene, path=("render", "engine")), function=FUNCTIONS["prop_tabs_enum"])),
    }


def pressed(identity: Identity) -> dict[str, object]:
    """Realizes operator buttons and openers over commands, placements, menus, and pies."""
    return {
        "command": _drawn(identity, OperatorButton(target=COUNTED)),
        "placement": _drawn(identity, OperatorButton(target=Placement(bpy.ops.wm.context_toggle, {"data_path": "scene.use_gravity"}), keywords={"text": "Gravity"})),
        "keyword": _drawn(identity, OperatorButton(target=COUNTED, keywords={"fixture": 1})),
        "acquired": tuple(type(row).__name__ for row in OperatorButton(target=COUNTED).acquisitions(identity).default_value(())),
        "menu": _drawn(identity, Opens(target=Opened(VIEW3D_MT_add, Called.MENU))),
        "panel": _drawn(identity, Opens(target=Opened("main", Called.PANEL))),
        "pie": _drawn(identity, Opens(target=Opened(VIEW3D_MT_pivot_pie, Called.PIE))),
    }


def labelled(identity: Identity) -> dict[str, object]:
    """Realizes fixed and read labels, a label through another function, and the report status row."""
    return {
        "fixed": _drawn(identity, Label(text="Fixture")),
        "read": _drawn(identity, Label(text=lambda context: Option.of_optional(context.scene).map(lambda scene: scene.name).to_list())),
        "single": _drawn(identity, Label(text="Fixture", function=FUNCTIONS["label"])),
        "keyword": _drawn(identity, Label(text="Fixture", keywords={"fixture": 1})),
        "status": _drawn(identity, Status()),
    }


def templated(identity: Identity) -> dict[str, object]:
    """Realizes template rows over data-block, format, color, and keymap members."""
    return {
        "preview": _drawn(identity, Preview(target=Member(root=bpy.types.Scene, path=("world",)))),
        "preview of value": _drawn(identity, Preview(target=Member(root=bpy.types.Scene, path=("frame_start",)))),
        "format": _drawn(identity, ImageFormat(target=Member(root=bpy.types.RenderSettings, path=("image_settings",)))),
        "format keyword": _drawn(identity, ImageFormat(target=Member(root=bpy.types.RenderSettings, path=("image_settings",)), keywords={"fixture": 1})),
        "wheel": _drawn(identity, wheel(Member(root=bpy.types.View3DShading, path=("single_color",)))),
        "shortcut": _drawn(identity, Shortcut(keymap=KEYS)),
    }


def composed(identity: Identity) -> dict[str, object]:
    """Realizes a sub-layout of rows and plain draws, a layout built per identity, and rows whose refusals accumulate."""
    rows = (Label(text="Fixture"), _separated, OperatorButton(target=COUNTED))
    return {
        "layout": _drawn(identity, Layout(rows=rows, container=Some(lambda layout: layout.box()))),
        "built": _drawn(identity, Layout(rows=lambda held: realized((), held).map(lambda _: rows))),
        "acquired": tuple(type(row).__name__ for row in acquired(rows)),
        "refusals": realized((Label(text="Fixture", keywords={"fixture": 1}), Property(target=Member(root=bpy.types.Scene, path=("fixture_absent",)))), identity).map(callable),
    }


def checked(_identity: Identity) -> dict[str, object]:
    """Checks keywords against UILayout function parameters."""
    return {"prop": accepted(FUNCTIONS["prop"], {"text": "Fixture", "slider": True}), "unknown": accepted(FUNCTIONS["prop"], {"txt": "Fixture"})}


# --- [COMPOSITION] ----------------------------------------------------------------------

rows: Final = ()
PROBES: Final[Mapping[str, Callable[[Identity], object]]] = MappingProxyType({
    "bound": bound,
    "pressed": pressed,
    "labelled": labelled,
    "templated": templated,
    "composed": composed,
    "checked": checked,
})
RELEASED: Final[Mapping[str, Callable[[], object]]] = MappingProxyType({})

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PROBES", "RELEASED", "rows"]
