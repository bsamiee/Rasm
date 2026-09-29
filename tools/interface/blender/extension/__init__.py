# ty: ignore[invalid-argument-type, invalid-assignment, unresolved-attribute]
# mypy: disable-error-code="arg-type, assignment, attr-defined, union-attr"
# ruff: file-ignore[non-empty-init-module, private-member-access, relative-imports]
"""Interface extension with viewport navigation, collapsed add-on panels, icon sidebar tabs, packed toolbars of the workspace's owners, asset shelves, command aliases, and the unit switch."""

from collections.abc import Callable
import os
from pathlib import Path

from bl_ui.space_toolsystem_common import ToolSelectPanelHelper
import bpy

from . import commands, navigation, panels, unit_system

# --- [COMPOSITION] ----------------------------------------------------------------------


def registration() -> tuple[Callable[[], None], Callable[[], None]]:
    """Register and unregister pair sharing the working directory, keymap items, header draws, panel attributes, toolbar layout, and tool reads a register pass replaces."""
    register_classes, unregister_classes = bpy.utils.register_classes_factory((*navigation.CLASSES, *commands.CLASSES, *panels.CLASSES, *unit_system.CLASSES))
    columns = vars(ToolSelectPanelHelper)["_layout_generator_multi_columns"]
    readers = {helper: members["tools_from_context"] for helper in ToolSelectPanelHelper.__subclasses__() if "tools_from_context" in (members := vars(helper))}
    found: list[Path] = []
    bindings: list[tuple[bpy.types.KeyMap, bpy.types.KeyMapItem]] = []
    status: list[Callable[[bpy.types.Header, bpy.types.Context], None]] = []
    collapsed: list[tuple[type[bpy.types.Panel], dict[str, object]]] = []

    def first_tick() -> None:
        """Collapse panels, restore the stock status bar, and add the Local View row, keymap items, and OSM dialog values once every add-on registered."""
        keyconfigs = bpy.context.window_manager.keyconfigs
        collapsed.extend(panels.collapse(bpy.context.preferences))
        bpy.types.VIEW3D_MT_object_context_menu.prepend(navigation.local_view)
        status.extend(panels.remove_appended(bpy.types.STATUSBAR_HT_header))
        bindings.extend((*navigation.bind(keyconfigs), *commands.bind(keyconfigs)))
        unit_system.set_dialogs()

    def register() -> None:
        """Set the working directory an untitled file's relative paths resolve against, then register the classes, toolbar layout, tool reads, alias store, unit switch, load handler, and first-tick timer."""
        found.append(Path.cwd())
        working = Path(bpy.context.preferences.filepaths.render_output_directory).parent
        working.mkdir(parents=True, exist_ok=True)
        os.chdir(working)
        register_classes()
        ToolSelectPanelHelper._layout_generator_multi_columns = staticmethod(panels.packed)
        for helper, reader in readers.items():
            helper.tools_from_context = classmethod(panels.placed(reader.__func__))
        bpy.types.WindowManager.interface_alias = commands.LAST
        bpy.types.SCENE_PT_unit.prepend(unit_system.switch)
        bpy.app.handlers.load_post.append(unit_system.match_loaded_scene)
        bpy.app.timers.register(first_tick, first_interval=0.0)

    def unregister() -> None:
        """Cancel pending timers, reverse every registration, and restore the working directory register found."""
        for timer in (first_tick, unit_system.set_dialogs):
            if bpy.app.timers.is_registered(timer):
                bpy.app.timers.unregister(timer)
        for draw in status:
            bpy.types.STATUSBAR_HT_header.append(draw)
        status.clear()
        registered = [cls for cls, _ in collapsed if cls.is_registered]
        for cls in reversed(registered):
            bpy.utils.unregister_class(cls)
        for cls, prior in collapsed:
            for key, value in prior.items():
                if value is not None:
                    setattr(cls, key, value)
                elif key in vars(cls):
                    delattr(cls, key)
        for cls in registered:
            bpy.utils.register_class(cls)
        collapsed.clear()
        ToolSelectPanelHelper._layout_generator_multi_columns = columns
        for helper, reader in readers.items():
            helper.tools_from_context = reader
        for keymap, item in bindings:
            keymap.keymap_items.remove(item)
        bindings.clear()
        bpy.app.handlers.load_post.remove(unit_system.match_loaded_scene)
        bpy.types.SCENE_PT_unit.remove(unit_system.switch)
        bpy.types.VIEW3D_MT_object_context_menu.remove(navigation.local_view)
        del bpy.types.WindowManager.interface_alias
        unregister_classes()
        os.chdir(found.pop())

    return register, unregister


register, unregister = registration()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
