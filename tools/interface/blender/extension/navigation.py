# ty: ignore[invalid-argument-type, invalid-assignment, invalid-type-form, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, explicit-any, union-attr, valid-type"
# ruff: file-ignore[invalid-class-name, mutable-class-default]
"""3D Viewport and canvas navigation that orbits a perspective view and pans every other view, unit system nudges, and their keys."""

from collections.abc import Callable
from types import MappingProxyType
from typing import Final, override, TYPE_CHECKING

import bpy
from bpy.props import EnumProperty, FloatVectorProperty, IntProperty

from .unit_system import scene_units
from .units import Units

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import EventTypeItems, EventValueItems, OperatorReturnItems

# --- [CONSTANTS] ------------------------------------------------------------------------

KEYMAPS: Final = MappingProxyType({"VIEW_3D": "3D View", "NODE_EDITOR": "Node Editor"})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [VIEW]
def navigator(context: bpy.types.Context) -> Callable[..., "set[OperatorReturnItems]"]:
    """Stock view operator the drag runs: orbit in a perspective view, pan in a parallel or camera view."""
    return bpy.ops.view3d.rotate if context.region_data.view_perspective == "PERSP" else bpy.ops.view3d.move


class INTERFACE_OT_navigate(bpy.types.Operator):
    """Orbit a perspective view and pan a parallel or camera view with the invoking drag."""

    bl_idname = "interface.navigate"
    bl_label = "Navigate"

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context) -> bool:
        return context.region_data is not None and navigator(context).poll()

    @override
    def invoke(self, context: bpy.types.Context, event: bpy.types.Event) -> "set[OperatorReturnItems]":
        return {"CANCELLED"} if "CANCELLED" in navigator(context)("INVOKE_DEFAULT") else {"FINISHED"}


class INTERFACE_OT_nudge(bpy.types.Operator):
    """Move the selection one nudge step of the scene's unit system along a direction of the view or the world."""

    bl_idname = "interface.nudge"
    bl_label = "Nudge"
    bl_options = {"REGISTER", "UNDO"}

    direction: FloatVectorProperty(name="Direction", size=3)
    orientation: EnumProperty(name="Orientation", items=[(name, name.title(), "") for name in ("VIEW", "GLOBAL")])
    step: IntProperty(name="Step", min=0, max=len(Units.IMPERIAL.nudge) - 1)

    @classmethod
    @override
    def poll(cls, context: bpy.types.Context) -> bool:
        return scene_units(context.scene) is not None

    @override
    def execute(self, context: bpy.types.Context) -> "set[OperatorReturnItems]":
        return bpy.ops.transform.translate(value=tuple(scene_units(context.scene).nudge[self.step] * part for part in self.direction), orient_type=self.orientation)


def local_view(self: bpy.types.Menu, _context: bpy.types.Context) -> None:
    """Local View at the head of the object context menu."""
    layout = self.layout
    layout.operator("view3d.localview")
    layout.separator()


# --- [KEYMAP]
def bind(keyconfigs: bpy.types.KeyConfigurations) -> list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]:
    """Add-on keymap items for Alt digit views, right-drag and trackpad navigation, canvas pans, and nudges."""
    addon, default = keyconfigs.addon, keyconfigs.default
    view, sculpt = addon.keymaps.new(name=KEYMAPS["VIEW_3D"], space_type="VIEW_3D"), addon.keymaps.new(name="Sculpt")
    keys = {item.name: item.identifier for item in bpy.types.KeyMapItem.bl_rna.properties["type"].enum_items}
    numpad = {f"NUMPAD_{digit}": keys[str(digit)] for digit in range(10)}
    items = [(keymap, keymap.keymap_items.new_from_item(source)) for source in default.keymaps[view.name].keymap_items if source.type in numpad and not source.alt for keymap in (view, sculpt)]
    for _, item in items:
        item.type, item.alt = numpad[item.type], True
    drags: tuple[tuple[EventTypeItems, EventValueItems], ...] = (("RIGHTMOUSE", "CLICK_DRAG"), ("TRACKPADPAN", "ANY"))
    items.extend((view, view.keymap_items.new(INTERFACE_OT_navigate.bl_idname, event, value)) for event, value in drags)
    for name, idname in (("View2D", "view2d.pan"), ("Image", "image.view_pan"), ("Clip Editor", "clip.view_pan")):
        source = default.keymaps[name]
        canvas = addon.keymaps.new(name=name, space_type=source.space_type, region_type=source.region_type)
        for pan in [
            item
            for item in source.keymap_items
            if item.idname == idname and (item.type, item.value) == ("MIDDLEMOUSE", "PRESS") and not (item.any or item.shift or item.ctrl or item.alt or item.oskey)
        ]:
            item = canvas.keymap_items.new_from_item(pan)
            item.type, item.value = "RIGHTMOUSE", "CLICK_DRAG"
            items.append((canvas, item))
    nudges: tuple[tuple[EventTypeItems, tuple[float, float, float], str], ...] = (
        ("LEFT_ARROW", (-1.0, 0.0, 0.0), "VIEW"),
        ("RIGHT_ARROW", (1.0, 0.0, 0.0), "VIEW"),
        ("DOWN_ARROW", (0.0, -1.0, 0.0), "VIEW"),
        ("UP_ARROW", (0.0, 1.0, 0.0), "VIEW"),
        ("PAGE_DOWN", (0.0, 0.0, -1.0), "GLOBAL"),
        ("PAGE_UP", (0.0, 0.0, 1.0), "GLOBAL"),
    )
    for step, (ctrl, shift) in enumerate(((False, False), (True, False), (False, True))):
        for event, direction, orientation in nudges:
            item = view.keymap_items.new(INTERFACE_OT_nudge.bl_idname, event, "PRESS", alt=True, ctrl=ctrl, shift=shift)
            item.properties.direction, item.properties.orientation, item.properties.step = direction, orientation, step
            items.append((view, item))
    return items


# --- [COMPOSITION] ----------------------------------------------------------------------

CLASSES: Final = (INTERFACE_OT_navigate, INTERFACE_OT_nudge)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CLASSES", "KEYMAPS", "bind", "local_view"]
