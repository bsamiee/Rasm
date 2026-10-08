# ty: ignore[invalid-argument-type, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr"
"""Register and unregister pair of the interface extension over one exit stack."""

from collections.abc import Callable
import contextlib
from pathlib import Path

import bpy

from . import commands, navigation, panels, unit_system

# --- [COMPOSITION] ----------------------------------------------------------------------


def registration() -> tuple[Callable[[], None], Callable[[], None]]:
    """Register and unregister pair over one exit stack, each register step pushing the step that reverses it and unregister closing the stack in reverse order."""
    stack = contextlib.ExitStack()
    register_classes, unregister_classes = bpy.utils.register_classes_factory((*navigation.CLASSES, *commands.CLASSES, *panels.CLASSES, *unit_system.CLASSES))

    def cancelled() -> None:
        """Cancel the first-tick and OSM dialog timers still pending."""
        for timer in (first_tick, unit_system.set_dialogs):
            if bpy.app.timers.is_registered(timer):
                bpy.app.timers.unregister(timer)

    def first_tick() -> None:
        """Collapse panels, restore the stock status bar, and add the Local View row, keymap items, and OSM dialog values once every add-on registered."""
        keyconfigs, menu, status = bpy.context.window_manager.keyconfigs, bpy.types.VIEW3D_MT_object_context_menu, bpy.types.STATUSBAR_HT_header
        stack.enter_context(panels.collapsed(bpy.context.preferences))
        menu.prepend(navigation.local_view)
        stack.callback(menu.remove, navigation.local_view)
        for draw in reversed(panels.remove_appended(status)):
            stack.callback(status.append, draw)
        for keymap, item in (*navigation.bind(keyconfigs), *commands.bind(keyconfigs)):
            stack.callback(keymap.keymap_items.remove, item)
        unit_system.set_dialogs()

    def register() -> None:
        """Enter the working directory an untitled file's relative paths resolve against, then register the classes, toolbar layout, tool reads, alias store, unit switch, load handler, and first-tick timer."""
        working = Path(bpy.context.preferences.filepaths.render_output_directory).parent
        working.mkdir(parents=True, exist_ok=True)
        stack.enter_context(contextlib.chdir(working))
        register_classes()
        stack.callback(unregister_classes)
        bpy.types.SCENE_PT_unit.prepend(unit_system.switch)
        stack.callback(bpy.types.SCENE_PT_unit.remove, unit_system.switch)
        bpy.app.handlers.load_post.append(unit_system.match_loaded_scene)
        stack.callback(bpy.app.handlers.load_post.remove, unit_system.match_loaded_scene)
        stack.enter_context(panels.toolbars())
        bpy.types.WindowManager.interface_alias = commands.LAST
        stack.callback(delattr, bpy.types.WindowManager, "interface_alias")
        bpy.app.timers.register(first_tick, first_interval=0.0, persistent=True)
        stack.callback(cancelled)

    return register, stack.close


register, unregister = registration()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
