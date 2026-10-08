# ty: ignore[invalid-argument-type, invalid-assignment, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr"
"""Blender interface configuration, timed workspace updates, report writing, and shutdown."""

from collections.abc import Container, Generator, Iterator, Mapping
from contextlib import closing, contextmanager, ExitStack
from functools import partial
from importlib import import_module
from itertools import chain, starmap
from types import ModuleType

import bpy

from interface.blender.rows import JSON, Launch
from interface.blender.script.addons import loaded, packages
from interface.blender.script.editors import aligned, cleared_history, declared_workspace, filtered, fitted_cameras, navigation_bars, sidebars, slots
from interface.blender.script.library import built_library
from interface.blender.script.preferences import bound_keymaps, converged_preferences
from interface.blender.script.rna import assigned, converge, converge_groups
from interface.blender.script.screens import activate, build_screen, Layout, LAYOUTS, operate, region_widths, shaped_workspaces, TICK
from interface.blender.script.settings import configured_addons, styles
from interface.blender.script.startup import ANALYSIS, cleared_objects, declared_scenes, prepared_scene
from interface.blender.script.theme import imported_theme
from interface.frame import Task
from interface.report import converged, Error, Header, reported, Update
from interface.units import Units

# --- [OPERATIONS] -----------------------------------------------------------------------


def stop_listeners(identities: Container[str]) -> None:
    """Stop loaded MCP servers and Bonsai subscriptions before changing workspace shading."""
    if "mcp" in identities:
        bpy.ops.blmcp.server_stop()
    if "MCP for Blender" in identities:
        bpy.ops.blendermcp.stop_server()
    if "bonsai" in identities:
        bpy.msgbus.clear_by_owner(import_module("bonsai.bim.handler").global_subscription_owner)


@contextmanager
def temporary_object(scene: bpy.types.Scene) -> Generator[None]:
    """Provide an active mesh object for Properties contexts and remove its data after use."""
    mesh = bpy.data.meshes.new("Temporary Object")
    target = bpy.data.objects.new(mesh.name, mesh)
    scene.collection.objects.link(target)
    bpy.context.view_layer.objects.active = target
    try:
        yield
    finally:
        bpy.data.batch_remove((target, mesh))


def shaped_workspace(unit_system: ModuleType, window: bpy.types.Window, layout: Layout, placement: Mapping[str, tuple[Task, ...]], modules: Mapping[str, str], units: Units) -> Iterator[Update]:
    """Configure a workspace after its screen and regions draw at their final sizes."""
    converging = partial(converge, unit_system)
    yield from activate(window, layout.task)
    operate(bpy.ops.workspace.reorder_to_back, window)
    yield TICK
    yield from build_screen(window, bpy.context.preferences, unit_system, layout)
    yield filtered(window.workspace, placement, modules)
    yield from navigation_bars(window)
    yield from cleared_history(window)
    yield from chain.from_iterable(starmap(converging, declared_workspace(window.workspace, layout, units)))
    yield from aligned(window, layout)
    if entries := sidebars(window, layout):
        yield TICK
        yield from chain.from_iterable(starmap(converging, entries))
    for label, shading, kind, light, name in slots(window.workspace):
        with assigned((shading, "type", kind), (shading, "light", light)):
            yield from converging(label, shading, {"studio_light": name})
    yield TICK
    yield from chain.from_iterable(starmap(converging, fitted_cameras(window)))


def interface(window: bpy.types.Window, launch: Launch) -> Generator[Update]:
    """Ticks, lines, and rows applying packages, assets, preferences, workspaces, startup scenes, units, add-on settings, and keymaps in dependency order."""
    context, preferences, scene, keyconfigs = bpy.context, bpy.context.preferences, bpy.context.scene, bpy.context.window_manager.keyconfigs
    preferences.use_preferences_save = False
    stop_listeners(loaded())
    yield from chain.from_iterable((item, TICK) for item in packages(preferences, launch.packages))
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
        yield from prepared_scene(scene)
        yield from chain.from_iterable(starmap(partial(converge, unit_system), declared_scenes(scene, launch)))
        yield from styles(preferences, scene)
        yield from converge_groups(unit_system, unit_system.resolved(units, (scene, bpy.data.scenes[ANALYSIS]), preferences))
        yield from configured_addons(unit_system, preferences, scene, launch, modules)
        yield TICK
        yield from bound_keymaps(unit_system, keyconfigs)
        yield from activate(window, Task.MODELING)
    yield from cleared_objects(scene)


def steps(launch: Launch) -> Iterator[float]:
    """Converge each row the interface yields, save a completed apply, write the report through the first error line or exception, and quit even when applying or reporting raises."""
    window = bpy.context.window_manager.windows[0]
    with ExitStack() as quitting, reported(launch.report) as body, closing(interface(window, launch)) as updates:
        quitting.callback(operate, bpy.ops.wm.quit_blender, window)
        body.append(Header(bpy.app.version_string, bpy.utils.user_resource("CONFIG")))
        for update in updates:
            match update:
                case int() | float():
                    yield update
                case _:
                    body.extend(converged(update))
            if isinstance(body[-1], Error):
                return
        with bpy.context.temp_override(window=window), assigned((bpy.context.preferences, "use_preferences_save", True)):
            bpy.ops.wm.save_homefile()
            bpy.ops.wm.save_userpref()


# --- [COMPOSITION] ----------------------------------------------------------------------


def start(launch: str) -> None:
    """Schedule decoded interface configuration through Blender's timer."""
    bpy.app.timers.register(partial(next, steps(JSON.loads(launch, Launch)), None))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["start"]
