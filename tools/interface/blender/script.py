# ty: ignore[invalid-argument-type, invalid-assignment, invalid-context-manager, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr"
"""Blender's scripted session, which converges every declared value and writes its report as it goes."""

from collections.abc import Callable, Iterable, Iterator, Mapping
from contextlib import contextmanager
from functools import partial
from importlib import import_module
from itertools import chain
from operator import setitem
from pathlib import Path
from types import ModuleType
from typing import TextIO

import bpy
from cattrs.preconf.json import make_converter

from interface.blender.addons import declared_files, extension, loaded, options, orphans, packages, Store
from interface.blender.catalog import Listed, Local
from interface.blender.editors import align_view, declared_workspace, filter_workspace, fitted_cameras, reveal, sidebars, slots, studio_slot
from interface.blender.preferences import declared_devices, declared_keymaps, declared_preferences, folders, KEYCONFIG, prepared
from interface.blender.screens import activate, build_screen, edges, Layout, override, resize, shape_workspaces, size_regions, TICK, Workspace
from interface.blender.startup import ANALYSIS, cleared, declared_scene, LIBRARY, prepared_scene
from interface.blender.theme import declared_theme, Paint
from interface.report import converged, Kind, line, Row

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [CONVERGE]
def applied(system: ModuleType, rows: Iterable[Row]) -> Iterator[str]:
    """Converge each row through the extension's stored-value form."""
    yield from chain.from_iterable(converged(row, system.stored) for row in rows)


def converge(system: ModuleType, label: str, owner: bpy.types.bpy_struct, declared: Mapping[str, object], assign: Callable[[bpy.types.bpy_struct, str, object], None] = setattr) -> Iterator[str]:
    """Converge every declared path under the owner in order through the assignment, a color role through the channels its property stores."""
    for path, target in declared.items():
        parent, _, name = path.rpartition(".")
        struct = owner.path_resolve(parent) if parent else owner
        value = target.stored(struct.bl_rna.properties[name]) if isinstance(target, Paint) else target
        yield from converged(Row(label=f"{label}.{path}", read=partial(system.current, struct, name), write=partial(assign, struct, name), target=value), system.stored)


# --- [DECLARED]
def provisioned(window: bpy.types.Window, preferences: bpy.types.Preferences, declared: tuple[Local | Listed, ...]) -> Iterator[float | str]:
    """Converge every declared package with a tick after each, then clear the startup data a removed add-on left."""
    for row in packages(window, preferences, declared):
        yield from converged(row, tuple)
        yield TICK
    yield from chain.from_iterable(converged(row, tuple) for row in orphans())


def preferred(system: ModuleType, preferences: bpy.types.Preferences, keyconfigs: bpy.types.KeyConfigurations, declared: Mapping[str, object]) -> Iterator[str]:
    """Converge the preferences, the keyconfig preferences, and the Cycles devices of the declared compute type."""
    yield from converge(system, "preferences", preferences, declared)
    yield from converge(system, "keyconfig", keyconfigs.active.preferences, KEYCONFIG)
    preferences.addons["cycles"].preferences.refresh_devices()
    yield from converge(system, "preferences", preferences, declared_devices(preferences))


def converged_workspace(system: ModuleType, window: bpy.types.Window, layout: Layout, modules: Mapping[str, str]) -> Iterator[float | str]:
    """Converge the shown workspace, its sidebar tab, and its studio light slots."""
    workspace = window.workspace
    for item in chain(declared_workspace(workspace, layout, modules), sidebars(window, layout)):
        match item:
            case (label, struct, rows):
                yield from converge(system, label, struct, rows)
            case _:
                yield item
    for label, shading, kind, light, name in slots(workspace):
        with studio_slot(shading, kind, light):
            yield from converge(system, label, shading, {"studio_light": name})


def configured(system: ModuleType, preferences: bpy.types.Preferences, scene: bpy.types.Scene, port: int, modules: Mapping[str, str], inkscape: str | None) -> Iterator[str]:
    """Converge every registered add-on setting through its store, then every declared file."""
    rows, skipped = options(preferences, scene, port, modules, system, inkscape)
    yield from skipped
    for (name, store), settings in rows.items():
        match store:
            case Store.PREFERENCES:
                group = preferences.addons[modules[name]].preferences
                yield from applied(system, folders(name, group, settings))
                yield from converge(system, name, group, settings)
            case Store.SCENE:
                yield from converge(system, name, scene, settings)
            case Store.ANALYSIS:
                yield from converge(system, name, bpy.data.scenes[ANALYSIS], settings)
            case Store.SOLAR:
                yield from converge(system, name, scene.BIMSolarProperties, settings, setitem)
            case Store.GEOREFERENCE:
                geo = import_module(f"{modules[name]}.geoscene").GeoScene(scene)
                yield from applied(system, (Row(label=f"{name}.{key}", read=partial(getattr, geo, key), write=partial(setattr, geo, key), target=target) for key, target in settings.items()))
    yield from applied(system, declared_files(modules))


def measured(system: ModuleType, preferences: bpy.types.Preferences, scene: bpy.types.Scene) -> Iterator[str]:
    """Converge every imperial unit setting in each startup scene and the preferences."""
    units = import_module(f"{system.__package__}.units").Units.IMPERIAL
    yield from chain.from_iterable(
        converge(system, f'scenes["{owner.name}"].{path}', struct, rows)
        for owner in (scene, bpy.data.scenes[ANALYSIS], bpy.data.scenes[LIBRARY])
        for path, struct, rows in system.declared(units, owner)
    )
    yield from chain.from_iterable(converge(system, f"preferences.{path}", struct, rows) for path, struct, rows in system.preferred(units, preferences))
    yield from chain.from_iterable(converge(system, f"preferences.{path}", struct, rows, setitem) for path, struct, rows in system.parameters(units, preferences))


def bound(system: ModuleType, keyconfigs: bpy.types.KeyConfigurations) -> Iterator[str]:
    """Converge the user keymap edits and write each moved item's active flag so the preferences save stores its diff."""
    for label, item, edits in declared_keymaps(keyconfigs.user):
        if lines := tuple(converge(system, label, item, edits)):
            item.active = bool(edits.get("active", item.active))
        yield from lines


# --- [STEPS]
@contextmanager
def subject(scene: bpy.types.Scene) -> Iterator[None]:
    """Active data-only mesh object for the block, which the Properties object contexts need."""
    mesh = bpy.data.meshes.new("Subject")
    target = bpy.data.objects.new("Subject", mesh)
    scene.collection.objects.link(target)
    bpy.context.view_layer.objects.active = target
    try:
        yield
    finally:
        bpy.data.batch_remove((target, mesh))


def stop_servers(modules: Mapping[str, str]) -> None:
    """Stop the listener each registered MCP add-on starts at registration, so no agent reaches a scripted session."""
    if "mcp" in modules:
        bpy.ops.blmcp.server_stop()
    if "MCP for Blender" in modules:
        bpy.ops.blendermcp.stop_server()


def shaped(window: bpy.types.Window, layout: Layout, placement: Mapping[str, tuple[str, ...]], modules: Mapping[str, str]) -> Iterator[float | str]:
    """Move the shown workspace to the end of the tab order and build its screen, then converge its filter, tab columns, and view."""
    with override(window):
        bpy.ops.workspace.reorder_to_back()
    yield from build_screen(window, layout)
    yield from filter_workspace(window.workspace, placement, modules)
    yield from reveal(window)
    yield from align_view(window, layout)


def sized(system: ModuleType, window: bpy.types.Window) -> Iterator[float | str]:
    """Move every edge the layout sizes onto its target, converge every toolbar and sidebar on screen, then fit each camera view to its camera bounds."""
    pixel = int(bpy.context.preferences.system.pixel_size)
    yield from chain.from_iterable(resize(window, area, side, target, pixel) for area, side, target in tuple(edges(window.workspace)))
    yield from size_regions(window)
    yield from chain.from_iterable(converge(system, label, space, fitted) for label, space, fitted in fitted_cameras(window))


def interface(window: bpy.types.Window, name: str, port: int, declared: tuple[Local | Listed, ...], editor: str | None, inkscape: str | None) -> Iterator[float | str]:
    """Every step in order from packages, preferences, and theme to workspaces, scene and add-on settings, and the cleared stock objects."""
    preferences, scene, keyconfigs = bpy.context.preferences, bpy.context.scene, bpy.context.window_manager.keyconfigs
    placement = {row.identity: row.workspaces for row in declared}
    preferences.use_preferences_save = False
    stop_servers(loaded())
    yield from provisioned(window, preferences, declared)
    stop_servers(loaded())
    system, settings = import_module(f"{extension(preferences, name)}.system"), declared_preferences(preferences, editor)
    structures, absent = prepared(preferences, keyconfigs, settings)
    yield from absent
    yield from applied(system, structures)
    yield from preferred(system, preferences, keyconfigs, settings)
    yield from converge(system, "theme", preferences.themes[0], declared_theme(preferences.themes[0], preferences.system))
    yield from shape_workspaces(window)
    with subject(scene):
        modules = loaded()
        for workspace in Workspace:
            yield from activate(window, workspace.name)
            yield from shaped(window, workspace.value, placement, modules)
            yield from converged_workspace(system, window, workspace.value, modules)
            yield from sized(system, window)
        yield from applied(system, prepared_scene(scene))
        yield from converge(system, "scene", scene, declared_scene(scene, preferences))
        yield from measured(system, preferences, scene)
        yield from configured(system, preferences, scene, port, modules, inkscape)
        yield TICK
        yield from bound(system, keyconfigs)
        yield from activate(window, Workspace.Modeling.name)
    yield from applied(system, cleared(scene))


# --- [FILES]
def save(window: bpy.types.Window) -> Iterator[float]:
    """Save the preferences and startup files from Modeling's viewport."""
    area = max((area for area in window.screen.areas if area.type == "VIEW_3D"), key=lambda area: area.width * area.height)
    bpy.context.preferences.use_preferences_save = True
    with override(window, area, next(region for region in area.regions if region.type == "WINDOW")):
        bpy.ops.wm.save_userpref()
        bpy.ops.wm.save_homefile()
    yield TICK
    yield TICK


# --- [COMPOSITION] ----------------------------------------------------------------------


def recorded(source: Iterator[float | str], sink: TextIO) -> Iterator[float]:
    """Pass ticks on and write each report line to the partial report as it comes."""
    for step in source:
        match step:
            case str():
                sink.write(f"{step}\n")
                sink.flush()
            case float():
                yield step


def steps(report: Path, name: str, port: int, declared: tuple[Local | Listed, ...], editor: str | None, inkscape: str | None) -> Iterator[float]:
    """Run every step into the partial report, save the preferences and startup files, move the report into place, and quit on every path."""
    window, part = bpy.context.window_manager.windows[0], report.with_suffix(".part")
    head = line(Kind.HEADER, bpy.app.version_string, bpy.utils.user_resource("CONFIG"))
    try:
        with part.open("w", encoding="utf-8") as sink:
            yield from recorded(chain((head,), interface(window, name, port, declared, editor, inkscape)), sink)
        yield from save(window)
        part.replace(report)
        yield TICK
    finally:
        with override(bpy.context.window_manager.windows[0]):
            bpy.ops.wm.quit_blender()


def start(report: str, name: str, port: int, rows: str, editor: str | None, inkscape: str | None) -> None:
    """Register the timer that runs every step over the package rows the host resolved, the extension named by its manifest id."""
    generator = steps(Path(report), name, port, make_converter().loads(rows, tuple[Local | Listed, ...]), editor, inkscape)
    bpy.app.timers.register(partial(next, generator, None), first_interval=0.5, persistent=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["start"]
