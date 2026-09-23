# ruff: file-ignore[non-empty-init-module, invalid-class-name, mutable-class-default]
"""Rhino's navigation rule for Blender's 3D Viewport: perspective views orbit, parallel and camera views pan.

A draw handler on every 3D Viewport keeps `RegionView3D.lock_rotation` equal to "the view is not perspective". Under that lock
`view3d.rotate` polls false and passes the event through to the next keymap item (view3d_navigate.cc view3d_rotation_poll,
blender-v5.2-release), so the right-button drag and the two-finger swipe that orbit a perspective view pan a plan, elevation,
or camera view instead. `ui-refine.py` writes those keymap items and repoints the numpad view items to the wrappers here,
because `view3d.view_axis`, `view3d.view_persportho`, and `view3d.view_camera` return early under the lock (axis_set_view,
viewpersportho_exec). Ctrl+Shift+RMB orbits out of a locked view.

The write in the draw handler is guarded: setting `lock_rotation` fires `NC_SPACE | ND_SPACE_VIEW3D`, and an unguarded write
would redraw on every draw. Blender loads an extension through this `__init__.py` with `register` and `unregister`, operator
classes carry the `_OT_` name Blender's registration expects, and `bl_options` is the set the Operator API declares.
"""

from typing import cast, ClassVar, override, TYPE_CHECKING

import bpy
from bpy.props import BoolProperty, EnumProperty

if TYPE_CHECKING:
    from bpy.stub_internal.rna_enums import OperatorReturnItems

# --- [CONSTANTS] ------------------------------------------------------------------------

AXES = (
    ("LEFT", "Left", "View from the left"),
    ("RIGHT", "Right", "View from the right"),
    ("BOTTOM", "Bottom", "View from the bottom"),
    ("TOP", "Top", "View from the top"),
    ("FRONT", "Front", "View from the front"),
    ("BACK", "Back", "View from the back"),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def unlock(context: bpy.types.Context | None) -> None:
    """Clear the rotation lock on the region the operator runs in."""
    cast("bpy.types.RegionView3D", cast("bpy.types.Context", context).region_data).lock_rotation = False


class RASM_OT_view_axis(bpy.types.Operator):
    """Numpad axis view that clears the rotation lock first, the handler re-locks the orthographic result."""

    bl_idname = "rasm.view_axis"
    bl_label = "View Axis"
    bl_options = {"REGISTER"}

    type: EnumProperty(name="View", items=AXES, default="FRONT")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    align_active: BoolProperty(name="Align Active", default=False)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    relative: BoolProperty(name="Relative", default=False)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        unlock(context)
        return bpy.ops.view3d.view_axis(type=self.type, align_active=self.align_active, relative=self.relative)


class RASM_OT_view_persportho(bpy.types.Operator):
    """Perspective and orthographic toggle that clears the rotation lock first."""

    bl_idname = "rasm.view_persportho"
    bl_label = "View Persp/Ortho"
    bl_options = {"REGISTER"}

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        unlock(context)
        return bpy.ops.view3d.view_persportho()


class RASM_OT_view_camera(bpy.types.Operator):
    """Camera view toggle that clears the rotation lock first."""

    bl_idname = "rasm.view_camera"
    bl_label = "View Camera"
    bl_options = {"REGISTER"}

    @override
    def execute(self, context: bpy.types.Context | None) -> "set[OperatorReturnItems]":
        unlock(context)
        return bpy.ops.view3d.view_camera()


class RASM_OT_orbit(bpy.types.Operator):
    """Orbit out of a locked plan, elevation, or camera view, the handler unlocks the perspective result."""

    bl_idname = "rasm.orbit"
    bl_label = "Orbit Unlocked"
    bl_options = {"REGISTER"}

    @override
    def invoke(self, context: bpy.types.Context | None, event: bpy.types.Event | None) -> "set[OperatorReturnItems]":
        unlock(context)
        bpy.ops.view3d.rotate("INVOKE_DEFAULT")
        return {"FINISHED"}


class LockSync:
    """Draw handler that keeps every 3D Viewport's rotation lock equal to its projection, written only when it changes."""

    handle: ClassVar[object] = None

    @staticmethod
    def draw() -> None:
        """Lock rotation on a non-perspective view and release it on perspective."""
        view = cast("bpy.types.RegionView3D", bpy.context.region_data)
        want = view.view_perspective != "PERSP"
        if view.lock_rotation != want:
            view.lock_rotation = want

    @classmethod
    def add(cls) -> None:
        """Register the handler on the 3D Viewport's main region."""
        cls.handle = bpy.types.SpaceView3D.draw_handler_add(cls.draw, (), "WINDOW", "POST_PIXEL")

    @classmethod
    def remove(cls) -> None:
        """Remove the handler."""
        bpy.types.SpaceView3D.draw_handler_remove(cls.handle, "WINDOW")
        cls.handle = None


# --- [COMPOSITION] ----------------------------------------------------------------------

CLASSES = (RASM_OT_view_axis, RASM_OT_view_persportho, RASM_OT_view_camera, RASM_OT_orbit)


def addon_keymap() -> bpy.types.KeyMap:
    """The add-on keyconfig's 3D View keymap, created on first use."""
    keyconfig = cast("bpy.types.KeyConfig", cast("bpy.types.WindowManager", bpy.context.window_manager).keyconfigs.addon)
    return keyconfig.keymaps.new(name="3D View", space_type="VIEW_3D", region_type="WINDOW")


def register() -> None:
    """Register the operators, the draw handler, and the Ctrl+Shift+RMB item in the add-on keyconfig."""
    for cls in CLASSES:
        bpy.utils.register_class(cls)
    LockSync.add()
    addon_keymap().keymap_items.new(RASM_OT_orbit.bl_idname, "RIGHTMOUSE", "PRESS", ctrl=True, shift=True)


def unregister() -> None:
    """Remove the keymap item, the draw handler, and the operators."""
    keymap = addon_keymap()
    for item in [item for item in keymap.keymap_items if item.idname == RASM_OT_orbit.bl_idname]:
        keymap.keymap_items.remove(item)
    LockSync.remove()
    for cls in reversed(CLASSES):
        bpy.utils.unregister_class(cls)


__all__ = ["register", "unregister"]
