# ty: ignore[invalid-argument-type, unresolved-attribute]
# mypy: disable-error-code="arg-type, union-attr"
# ruff: file-ignore[non-empty-init-module, relative-imports]
"""Blender extension of the interface: Rhino's navigation rule, add-on panels that open collapsed, and the alias families as pies behind a leader key."""

from typing import ClassVar

import bpy

from . import families, navigation, panels

# --- [SERVICES] -------------------------------------------------------------------------


class Registered:
    """The draw handler and add-on keyconfig items `register` adds and `unregister` removes."""

    handler: ClassVar[object] = None
    items: ClassVar[list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]]] = []


# --- [COMPOSITION] ----------------------------------------------------------------------

CLASSES = (*navigation.CLASSES, *families.CLASSES)


def first_tick() -> None:
    """The panel pass and the keymap items, once every add-on has registered and the default keyconfig is built."""
    keyconfigs = bpy.context.window_manager.keyconfigs
    panels.collapse(bpy.context.preferences)
    Registered.items.extend((*navigation.bind(keyconfigs), *families.bind(keyconfigs)))


def register() -> None:
    """Register the operators and pies, the context menu row, the draw handler, and the first-tick pass."""
    for cls in CLASSES:
        bpy.utils.register_class(cls)
    bpy.types.VIEW3D_MT_object_context_menu.prepend(navigation.local_view)
    Registered.handler = bpy.types.SpaceView3D.draw_handler_add(navigation.sync_lock, (), "WINDOW", "POST_PIXEL")
    bpy.app.timers.register(first_tick, first_interval=0.0)


def unregister() -> None:
    """Remove the keymap items, the context menu row, the draw handler, and the classes."""
    for keymap, item in Registered.items:
        keymap.keymap_items.remove(item)
    Registered.items.clear()
    bpy.types.VIEW3D_MT_object_context_menu.remove(navigation.local_view)
    bpy.types.SpaceView3D.draw_handler_remove(Registered.handler, "WINDOW")
    for cls in reversed(CLASSES):
        bpy.utils.unregister_class(cls)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
