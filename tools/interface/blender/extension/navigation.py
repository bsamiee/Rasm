# ty: ignore[invalid-argument-type, invalid-assignment, invalid-type-form, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, no-any-return, union-attr, valid-type"
# ruff: file-ignore[invalid-class-name, mutable-class-default, relative-imports]
"""3D Viewport and canvas navigation with view operators that release the rotation lock, unit system nudges, and their keys."""

from types import MappingProxyType
from typing import Final, override, TYPE_CHECKING

import bpy
from bpy.props import BoolProperty, EnumProperty, FloatProperty, FloatVectorProperty, IntProperty

from .units import Units

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import EventTypeItems, OperatorReturnItems

# --- [CONSTANTS] ------------------------------------------------------------------------

KEYMAPS: Final = MappingProxyType({"VIEW_3D": "3D View", "NODE_EDITOR": "Node Editor"})

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [VIEW]
class ViewOperator(bpy.types.Operator):
    """Base of the unlocked view operators, registered without an undo step."""

    bl_options = {"REGISTER"}


def unlocked(name: str) -> type[bpy.types.Operator]:
    """Operator `control.<name>` that releases the rotation lock and runs the stock view operator with its properties."""
    stock = getattr(bpy.ops.view3d, name)
    rna = stock.get_rna_type()
    declare = {
        "ENUM": lambda prop: EnumProperty(name=prop.name, items=[(item.identifier, item.name, item.description) for item in prop.enum_items], default=prop.default),
        "FLOAT": lambda prop: FloatProperty(name=prop.name, default=prop.default, subtype=prop.subtype),
        "BOOLEAN": lambda prop: BoolProperty(name=prop.name, default=prop.default),
    }
    annotations = {prop.identifier: declare[prop.type](prop) for prop in rna.properties if prop.identifier not in bpy.types.OperatorProperties.bl_rna.properties}

    def execute(self: bpy.types.Operator, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        context.region_data.lock_rotation = False
        return stock(**{key: getattr(self, key) for key in annotations if self.properties.is_property_set(key)})

    return type(f"CONTROL_OT_{name}", (ViewOperator,), {"bl_idname": f"control.{name}", "bl_label": rna.name, "bl_description": rna.description, "__annotations__": annotations, "execute": execute})


class CONTROL_OT_orbit(ViewOperator):
    """Orbit out of a locked plan, elevation, or camera view."""

    bl_idname = "control.orbit"
    bl_label = "Orbit Unlocked"

    @override
    def invoke(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        context.region_data.lock_rotation = False
        bpy.ops.view3d.rotate("INVOKE_DEFAULT")
        return {"FINISHED"}


class CONTROL_OT_nudge(bpy.types.Operator):
    """Move the selection one nudge step of the scene's unit system along a direction of the view or the world."""

    bl_idname = "control.nudge"
    bl_label = "Nudge"
    bl_options = {"REGISTER", "UNDO"}

    direction: FloatVectorProperty(name="Direction", size=3)
    orientation: EnumProperty(name="Orientation", items=[(name, name.title(), "") for name in ("VIEW", "GLOBAL")])
    step: IntProperty(name="Step", min=0, max=len(Units.IMPERIAL.nudge) - 1)

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        length = Units[context.scene.unit_settings.system].nudge[self.step]
        return bpy.ops.transform.translate(value=tuple(length * part for part in self.direction), orient_type=self.orientation)


def sync_lock() -> None:
    """Lock the drawn view's rotation outside perspective and release it in perspective."""
    view = bpy.context.region_data
    locked = view.view_perspective != "PERSP"
    if view.lock_rotation != locked:
        view.lock_rotation = locked


def local_view(self: bpy.types.Menu, _context: bpy.types.Context) -> None:
    """Local View at the head of the object context menu."""
    layout = self.layout
    layout.operator("view3d.localview")
    layout.separator()


# --- [KEYMAP]
def rebound(keymap: bpy.types.KeyMap, source: bpy.types.KeyMapItem, event: "EventTypeItems", *, alt: bool) -> bpy.types.KeyMapItem:
    """Copy of a stock keymap item on the event and Alt state, running the unlocked operator where one replaces the stock one."""
    item = keymap.keymap_items.new_from_item(source)
    item.type, item.alt = event, alt
    if (cls := UNLOCKED.get(source.idname)) is not None:
        item.idname = cls.bl_idname
        for key in [key for key in cls.__annotations__ if source.properties.is_property_set(key)]:
            setattr(item.properties, key, getattr(source.properties, key))
    return item


def bind(keyconfigs: bpy.types.KeyConfigurations) -> list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]:
    """Add-on keymap items for the unlocked view operators, Alt digit views, right-drag and trackpad navigation, the orbit, canvas pans, and nudges."""
    addon, default = keyconfigs.addon, keyconfigs.default
    view, sculpt = addon.keymaps.new(name=KEYMAPS["VIEW_3D"], space_type="VIEW_3D", region_type="WINDOW"), addon.keymaps.new(name="Sculpt", space_type="EMPTY", region_type="WINDOW")
    keys = {item.name: item.identifier for item in bpy.types.KeyMapItem.bl_rna.properties["type"].enum_items}
    numpad = {f"NUMPAD_{digit}": keys[str(digit)] for digit in range(10)}
    stock = [source for source in default.keymaps[view.name].keymap_items if source.idname in UNLOCKED or (source.type in numpad and not source.alt)]
    items = [(view, rebound(view, source, source.type, alt=source.alt)) for source in stock if source.idname in UNLOCKED]
    items.extend((keymap, rebound(keymap, source, numpad[source.type], alt=True)) for source in stock if source.type in numpad and not source.alt for keymap in (view, sculpt))
    items.extend(
        (view, view.keymap_items.new(idname, event, value))
        for idname, event, value in reversed((
            ("view3d.rotate", "RIGHTMOUSE", "CLICK_DRAG"),
            ("view3d.move", "RIGHTMOUSE", "CLICK_DRAG"),
            ("view3d.rotate", "TRACKPADPAN", "ANY"),
            ("view3d.move", "TRACKPADPAN", "ANY"),
        ))
    )
    items.extend((view, view.keymap_items.new(CONTROL_OT_orbit.bl_idname, event, value, alt=alt)) for event, value, alt in (("RIGHTMOUSE", "CLICK_DRAG", True), ("MIDDLEMOUSE", "PRESS", False)))
    for name, idname in (("View2D", "view2d.pan"), ("Image", "image.view_pan"), ("Clip Editor", "clip.view_pan")):
        source, canvas = default.keymaps[name], addon.keymaps.new(name=name, space_type=default.keymaps[name].space_type, region_type=default.keymaps[name].region_type)
        for pan in [
            item
            for item in source.keymap_items
            if item.idname == idname and (item.type, item.value) == ("MIDDLEMOUSE", "PRESS") and not (item.any or item.shift or item.ctrl or item.alt or item.oskey)
        ]:
            item = canvas.keymap_items.new_from_item(pan)
            item.type, item.value = "RIGHTMOUSE", "CLICK_DRAG"
            items.append((canvas, item))
    for step, (ctrl, shift) in enumerate(((False, False), (True, False), (False, True))):
        for event, direction, orientation in (
            ("LEFT_ARROW", (-1.0, 0.0, 0.0), "VIEW"),
            ("RIGHT_ARROW", (1.0, 0.0, 0.0), "VIEW"),
            ("DOWN_ARROW", (0.0, -1.0, 0.0), "VIEW"),
            ("UP_ARROW", (0.0, 1.0, 0.0), "VIEW"),
            ("PAGE_DOWN", (0.0, 0.0, -1.0), "GLOBAL"),
            ("PAGE_UP", (0.0, 0.0, 1.0), "GLOBAL"),
        ):
            item = view.keymap_items.new(CONTROL_OT_nudge.bl_idname, event, "PRESS", alt=True, ctrl=ctrl, shift=shift)
            item.properties.direction, item.properties.orientation, item.properties.step = direction, orientation, step
            items.append((view, item))
    return items


# --- [COMPOSITION] ----------------------------------------------------------------------

UNLOCKED: Final = MappingProxyType({f"view3d.{name}": unlocked(name) for name in ("view_axis", "view_persportho", "view_camera", "view_orbit")})
CLASSES: Final = (*UNLOCKED.values(), CONTROL_OT_orbit, CONTROL_OT_nudge)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bind", "CLASSES", "KEYMAPS", "local_view", "sync_lock"]
