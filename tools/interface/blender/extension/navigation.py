# ty: ignore[invalid-argument-type, invalid-assignment, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, no-any-return, union-attr"
# ruff: file-ignore[invalid-class-name, mutable-class-default]
"""Rhino's navigation rule in the 3D Viewport: the stock view operators and an orbit that run with the rotation lock released, the draw handler that locks every view outside perspective, and the keys that bind them."""

from types import MappingProxyType
from typing import override, TYPE_CHECKING

import bpy
from bpy.props import BoolProperty, EnumProperty, FloatProperty

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [CONSTANTS] ------------------------------------------------------------------------

VIEW_OPERATIONS = ("view_axis", "view_persportho", "view_camera", "view_orbit")
KEYMAPS = MappingProxyType({"VIEW_3D": "3D View", "NODE_EDITOR": "Node Editor"})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [VIEW]
class ViewOperator(bpy.types.Operator):
    """Operator base of the unlocked view operators, logged without an undo step."""

    bl_options = {"REGISTER"}


def mirrored(prop: bpy.types.Property) -> object:
    """Operator property declaring the same name, default, and items as the stock operator's property."""
    match prop:
        case bpy.types.EnumProperty():
            return EnumProperty(name=prop.name, items=[(item.identifier, item.name, item.description) for item in prop.enum_items], default=prop.default)
        case bpy.types.FloatProperty():
            return FloatProperty(name=prop.name, default=prop.default, subtype=prop.subtype)
        case _:
            return BoolProperty(name=prop.name, default=prop.default)


def wrapper(name: str) -> type[bpy.types.Operator]:
    """Operator `interface.<name>` taking the stock view operator's own properties, which releases the rotation lock and runs the stock operator."""
    stock = getattr(bpy.ops.view3d, name)
    rna = stock.get_rna_type()
    annotations = {prop.identifier: mirrored(prop) for prop in rna.properties if prop.identifier not in bpy.types.OperatorProperties.bl_rna.properties}

    def execute(self: bpy.types.Operator, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        context.region_data.lock_rotation = False
        return stock(**{key: getattr(self, key) for key in annotations if self.properties.is_property_set(key)})

    return type(
        f"INTERFACE_OT_{name}", (ViewOperator,), {"bl_idname": f"interface.{name}", "bl_label": rna.name, "bl_description": rna.description, "__annotations__": annotations, "execute": execute}
    )


class INTERFACE_OT_orbit(ViewOperator):
    """Orbit out of a locked plan, elevation, or camera view."""

    bl_idname = "interface.orbit"
    bl_label = "Orbit Unlocked"

    @override
    def invoke(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        context.region_data.lock_rotation = False
        bpy.ops.view3d.rotate("INVOKE_DEFAULT")
        return {"FINISHED"}


def sync_lock() -> None:
    """Lock the drawn view's rotation outside perspective and release it in perspective, written only where it differs."""
    view = bpy.context.region_data
    locked = view.view_perspective != "PERSP"
    if view.lock_rotation != locked:
        view.lock_rotation = locked


def local_view(self: bpy.types.Menu, _context: bpy.types.Context) -> None:
    """Local View at the head of the object context menu, Blender's counterpart of Rhino's Isolate."""
    layout = self.layout
    layout.operator("view3d.localview")
    layout.separator()


# --- [KEYMAP]
def bind(keyconfigs: bpy.types.KeyConfigurations) -> list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]:
    """Add-on keyconfig items: the wrapped view operators' stock items, the right-drag and swipe navigation in precedence order, the orbit out of a locked view, the node editor's right-drag pan, and the mesh select modes on Alt with the digits the numpad emulation takes."""
    addon = keyconfigs.addon
    view, nodes = (addon.keymaps.new(name=name, space_type=space, region_type="WINDOW") for space, name in KEYMAPS.items())
    items = []
    for source in [source for source in keyconfigs.default.keymaps[view.name].keymap_items if source.idname in WRAPPERS]:
        cls, item = WRAPPERS[source.idname], view.keymap_items.new_from_item(source)
        item.idname = cls.bl_idname
        for key in [key for key in cls.__annotations__ if source.properties.is_property_set(key)]:
            setattr(item.properties, key, getattr(source.properties, key))
        items.append((view, item))
    items.extend(
        (view, view.keymap_items.new(idname, event, value))
        for idname, event, value in reversed((
            ("view3d.rotate", "RIGHTMOUSE", "CLICK_DRAG"),
            ("view3d.move", "RIGHTMOUSE", "CLICK_DRAG"),
            ("view3d.rotate", "TRACKPADPAN", "ANY"),
            ("view3d.move", "TRACKPADPAN", "ANY"),
        ))
    )
    items.extend((view, view.keymap_items.new(INTERFACE_OT_orbit.bl_idname, event, value, alt=alt)) for event, value, alt in (("RIGHTMOUSE", "CLICK_DRAG", True), ("MIDDLEMOUSE", "PRESS", False)))
    items.append((nodes, nodes.keymap_items.new("view2d.pan", "RIGHTMOUSE", "CLICK_DRAG")))
    mesh = addon.keymaps.new(name="Mesh", space_type="EMPTY", region_type="WINDOW")
    for event, mode in (("NUMPAD_1", "VERT"), ("NUMPAD_2", "EDGE"), ("NUMPAD_3", "FACE")):
        item = mesh.keymap_items.new("mesh.select_mode", event, "PRESS", alt=True)
        item.properties.type = mode
        items.append((mesh, item))
    return items


# --- [COMPOSITION] ----------------------------------------------------------------------

WRAPPERS = MappingProxyType({f"view3d.{name}": wrapper(name) for name in VIEW_OPERATIONS})
CLASSES = (*WRAPPERS.values(), INTERFACE_OT_orbit)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bind", "CLASSES", "KEYMAPS", "local_view", "sync_lock"]
