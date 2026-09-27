# ty: ignore[invalid-argument-type, invalid-assignment, unresolved-attribute]
# mypy: disable-error-code="arg-type, assignment, attr-defined, union-attr"
# ruff: file-ignore[non-empty-init-module, private-member-access, relative-imports]
"""Control extension with lock-aware navigation, collapsed add-on panels, icon sidebar tabs, packed toolbars of the workspace's owners, asset shelves, command aliases, and the unit switch."""

from collections.abc import Callable

from bl_ui.space_toolsystem_common import ToolSelectPanelHelper
import bpy

from . import families, navigation, panels, system

# --- [COMPOSITION] ----------------------------------------------------------------------


def registration() -> tuple[Callable[[], None], Callable[[], None]]:
    """Register and unregister pair sharing the handlers, keymap items, header draws, toolbar layout, and tool reads a register pass replaces."""
    register_classes, unregister_classes = bpy.utils.register_classes_factory((*navigation.CLASSES, *families.CLASSES, *panels.CLASSES, *system.CLASSES))
    columns = vars(ToolSelectPanelHelper)["_layout_generator_multi_columns"]
    readers = {helper: vars(helper)["tools_from_context"] for helper in ToolSelectPanelHelper.__subclasses__()}
    handlers: list[object] = []
    bindings: list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]] = []
    status: list[Callable[[bpy.types.Header, bpy.types.Context], None]] = []

    def first_tick() -> None:
        """Collapse panels, restore the stock status bar, and add the Local View row, keymap items, and OSM dialog values once every add-on registered."""
        keyconfigs = bpy.context.window_manager.keyconfigs
        panels.collapse(bpy.context.preferences)
        bpy.types.VIEW3D_MT_object_context_menu.remove(navigation.local_view)
        bpy.types.VIEW3D_MT_object_context_menu.prepend(navigation.local_view)
        status.extend(panels.remove_appended(bpy.types.STATUSBAR_HT_header))
        bindings.extend((*navigation.bind(keyconfigs), *families.bind(keyconfigs)))
        system.set_dialogs()

    def register() -> None:
        """Register the classes, toolbar layout, tool reads, alias store, menu row, unit switch, load handler, draw handler, and first-tick timer."""
        register_classes()
        ToolSelectPanelHelper._layout_generator_multi_columns = staticmethod(panels.packed)
        for helper, reader in readers.items():
            helper.tools_from_context = classmethod(panels.placed(reader.__func__))
        bpy.types.WindowManager.control_alias = families.LAST
        bpy.types.VIEW3D_MT_object_context_menu.prepend(navigation.local_view)
        bpy.types.SCENE_PT_unit.prepend(system.switch)
        bpy.app.handlers.load_post.append(system.match_loaded_scene)
        handlers.append(bpy.types.SpaceView3D.draw_handler_add(navigation.sync_lock, (), "WINDOW", "POST_PIXEL"))
        bpy.app.timers.register(first_tick, first_interval=0.0)

    def unregister() -> None:
        """Cancel pending timers and undo every registration."""
        for timer in (first_tick, system.set_dialogs):
            if bpy.app.timers.is_registered(timer):
                bpy.app.timers.unregister(timer)
        for draw in status:
            bpy.types.STATUSBAR_HT_header.append(draw)
        status.clear()
        ToolSelectPanelHelper._layout_generator_multi_columns = columns
        for helper, reader in readers.items():
            helper.tools_from_context = reader
        for keymap, item in bindings:
            keymap.keymap_items.remove(item)
        bindings.clear()
        bpy.app.handlers.load_post.remove(system.match_loaded_scene)
        bpy.types.SCENE_PT_unit.remove(system.switch)
        bpy.types.VIEW3D_MT_object_context_menu.remove(navigation.local_view)
        for handler in handlers:
            bpy.types.SpaceView3D.draw_handler_remove(handler, "WINDOW")
        handlers.clear()
        del bpy.types.WindowManager.control_alias
        unregister_classes()

    return register, unregister


register, unregister = registration()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
