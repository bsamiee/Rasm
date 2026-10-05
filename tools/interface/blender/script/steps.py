# ruff: file-ignore[blind-except]
# ty: ignore[invalid-argument-type, invalid-assignment, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr"
"""Blender's scripted session step chain: packages, library, preferences, theme, workspaces, startup scenes, units, add-on settings, and keymaps, then the save and the report."""

from collections.abc import Container, Generator, Iterator, Mapping
from contextlib import contextmanager
from functools import partial
from importlib import import_module
from itertools import chain
from pathlib import Path
import traceback
from types import ModuleType

import bpy

from interface.blender.rows import JSON, Launch
from interface.blender.script.addons import loaded, packages, unloaded_packages
from interface.blender.script.editors import aligned, cleared_history, declared_workspace, filtered, fitted_cameras, navigation_bars, sidebars, slots
from interface.blender.script.library import built_library
from interface.blender.script.preferences import bound_keymaps, converged_preferences
from interface.blender.script.rna import assigned, converge, converge_groups
from interface.blender.script.screens import activate, build_screen, Layout, LAYOUTS, override, region_widths, shaped_workspaces, TICK
from interface.blender.script.settings import configured_addons, styles
from interface.blender.script.startup import ANALYSIS, cleared_objects, declared_scenes, prepared_scene
from interface.blender.script.theme import imported_theme
from interface.frame import Task
from interface.report import converged, Kind, line, Row
from interface.units import Units

# --- [OPERATIONS] -----------------------------------------------------------------------


def stop_listeners(identities: Container[str]) -> None:
    """Stop the listener each loaded MCP add-on runs and Bonsai's message bus subscriptions, so no agent reaches the scripted session and no shading type a slot probes sets a declared color type."""
    if "mcp" in identities:
        bpy.ops.blmcp.server_stop()
    if "MCP for Blender" in identities:
        bpy.ops.blendermcp.stop_server()
    if "bonsai" in identities:
        bpy.msgbus.clear_by_owner(import_module("bonsai.bim.handler").global_subscription_owner)


@contextmanager
def temporary_object(scene: bpy.types.Scene) -> Generator[None]:
    """Active data-only mesh object for the block, which the Properties object contexts need, removed after it."""
    mesh = bpy.data.meshes.new("Temporary Object")
    target = bpy.data.objects.new("Temporary Object", mesh)
    scene.collection.objects.link(target)
    bpy.context.view_layer.objects.active = target
    try:
        yield
    finally:
        bpy.data.batch_remove((target, mesh))


def shaped_workspace(unit_system: ModuleType, window: bpy.types.Window, layout: Layout, placement: Mapping[str, tuple[Task, ...]], modules: Mapping[str, str], units: Units) -> Iterator[float | str]:
    """Show the layout's workspace at the end of the tab order, build its screen, and converge its filter, views, spaces, sidebar tab, and studio light slots, then the camera views one pass later, once the regions drew at their sizes."""
    yield from activate(window, layout.task)
    with override(window):
        bpy.ops.workspace.reorder_to_back()
    yield TICK
    yield from build_screen(window, bpy.context.preferences, unit_system, layout)
    yield from converged(filtered(window.workspace, placement, modules))
    yield from navigation_bars(window)
    yield from chain.from_iterable(map(converged, cleared_history(window)))
    yield from chain.from_iterable(converge(unit_system, label, struct, declared) for label, struct, declared in declared_workspace(window.workspace, layout, units))
    yield from chain.from_iterable(map(converged, aligned(window, layout)))
    if entries := sidebars(window, layout):
        yield TICK
        yield from chain.from_iterable(converge(unit_system, label, struct, declared) for label, struct, declared in entries)
    for label, shading, kind, light, name in slots(window.workspace):
        with assigned((shading, "type", kind), (shading, "light", light)):
            yield from converge(unit_system, label, shading, {"studio_light": name})
    yield TICK
    yield from chain.from_iterable(converge(unit_system, label, space, fitted) for label, space, fitted in fitted_cameras(window))


def interface(window: bpy.types.Window, launch: Launch) -> Iterator[float | str]:
    """Every step in order: packages, library, preferences, the theme one pass later once the window's pixel size and scale follow them, workspaces and the region widths the host stores, the startup scenes, units, add-on settings, and keymaps, then the cleared stock objects and the save."""
    context, preferences, scene, keyconfigs = bpy.context, bpy.context.preferences, bpy.context.scene, bpy.context.window_manager.keyconfigs
    preferences.use_preferences_save = False
    stop_listeners(loaded())
    for item in packages(window, preferences, launch.packages):
        yield from converged(item) if isinstance(item, Row) else (item,)
        yield TICK
    if errors := unloaded_packages(launch.packages):
        yield from errors
        return
    modules = loaded()
    stop_listeners(modules)
    unit_system = import_module(f"{modules[launch.extension]}.unit_system")
    units = unit_system.Units[launch.units]
    yield from built_library(scene, modules["modular_tree"])
    yield from converged_preferences(unit_system, preferences, keyconfigs, launch)
    yield TICK
    yield from imported_theme(context)
    yield from shaped_workspaces(window, unit_system)
    placement = {row.identity: row.workspaces for row in launch.packages}
    with temporary_object(scene):
        yield from chain.from_iterable(shaped_workspace(unit_system, window, layout, placement, modules, units) for layout in LAYOUTS)
        yield region_widths(preferences)
        yield from chain.from_iterable(map(converged, prepared_scene(scene)))
        yield from chain.from_iterable(converge(unit_system, label, owner, declared) for label, owner, declared in declared_scenes(scene, launch))
        yield from chain.from_iterable(map(converged, styles(preferences, scene)))
        yield from converge_groups(unit_system, unit_system.resolved(units, (scene, bpy.data.scenes[ANALYSIS]), preferences))
        yield from configured_addons(unit_system, preferences, scene, launch, modules)
        yield TICK
        yield from bound_keymaps(unit_system, keyconfigs)
        yield from activate(window, Task.MODELING)
    yield from chain.from_iterable(map(converged, cleared_objects(scene)))
    yield from saved_files(window)


def saved_files(window: bpy.types.Window) -> Iterator[float]:
    """Save the preferences and startup files from the window, then pass two ticks."""
    bpy.context.preferences.use_preferences_save = True
    with override(window):
        bpy.ops.wm.save_userpref()
        bpy.ops.wm.save_homefile()
    yield TICK
    yield TICK


def steps(launch: Launch) -> Iterator[float]:
    """Run every step and the save, write the report once with the header and every line, an error that ended the run after the lines before it, and quit."""
    window = bpy.context.window_manager.windows[0]
    body: list[str] = []
    try:
        for step in interface(window, launch):
            if isinstance(step, str):
                body.append(step)
            else:
                yield step
    except Exception as error:
        body.append(line(Kind.ERROR, " ".join("".join(traceback.format_exception_only(error)).split())))
    header = line(Kind.HEADER, bpy.app.version_string, bpy.utils.user_resource("CONFIG"))
    Path(launch.report).write_text("".join(f"{entry}\n" for entry in (header, *body)), encoding="utf-8")
    with override(window):
        bpy.ops.wm.quit_blender()


# --- [COMPOSITION] ----------------------------------------------------------------------


def start(launch: str) -> None:
    """Register the timer that runs every step of the session call the host encoded as JSON, cattrs telling each package row by its fields."""
    generator = steps(JSON.loads(launch, Launch))
    bpy.app.timers.register(partial(next, generator, None), first_interval=0.5, persistent=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["start", "steps"]
