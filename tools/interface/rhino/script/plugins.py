# ty: ignore[invalid-argument-type]
# mypy: disable-error-code="import-untyped"
"""Rhino plug-in rows: the load protection or Grasshopper 2 marker of each package file, the bundled plug-ins' load modes, and package plug-in settings."""

from collections.abc import Iterator
from functools import partial
from pathlib import Path

import Rhino
from Rhino.PlugIns import PlugIn, PlugInLoadTime
from Rhino.Runtime import HostUtils
from System import Guid, String, StringComparison

from interface.report import Error, Item, Row, Skip
from interface.rhino.script.accessors import action, color, found, Internal, key, located, preference
from interface.roles import Status

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [PACKAGES]
def installed(packages: tuple[str, ...]) -> dict[str, dict[Path, Guid | None]]:
    """Declared packages' plug-in files and registered ids, or None for an unregistered file."""
    root, records = Path(HostUtils.AutoInstallPlugInFolder(currentUser=True)), {Path(PlugIn.GetPlugInInfo(plugin).FileName): plugin for plugin in PlugIn.GetInstalledPlugIns().Keys}
    return {
        name: {path: records.get(path) for path in sorted(folder.glob("*.rhp"))}
        for folder in (Path(held.FullName) for held in HostUtils.GetActivePlugInVersionFolders())
        if folder.is_relative_to(root) and any(String.Equals(name := folder.relative_to(root).parts[0], package, StringComparison.InvariantCultureIgnoreCase) for package in packages)
    }


def registry_child(plugin: Guid) -> tuple[str, ...]:
    """Settings path of the plug-in's record under the registry version holding it."""
    registry, record = "PlugInRegistry", str(plugin)
    versions = located((registry,))
    return next((registry, version, record) for version in (() if versions is None else versions.ChildKeys) if located((registry, version, record)) is not None)


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows(packages: tuple[str, ...]) -> Iterator[Item]:
    """Rows for declared package plug-ins, bundled load modes, agent settings, and loaded plug-in settings."""
    held, unwelded = installed(packages), "ShowUnweldedEdges"
    plugins = tuple(plugin for files in held.values() for plugin in files.values() if plugin is not None)
    edges = tuple(plugin for plugin in plugins if unwelded in PlugIn.GetEnglishCommandNames(plugin))
    bundled = {name: PlugIn.IdFromName(name) for name in (f"3DxRhino.{Rhino.RhinoApp.ExeVersion}", "PanelingTools")}

    def protection(plugin: Guid) -> Row:
        """Row of the plug-in's silent load protection."""
        return preference(
            label=f'PlugIns["{PlugIn.GetPlugInInfo(plugin).Name}"].LoadProtection', read=lambda: found(PlugIn.GetLoadProtection(plugin)), write=partial(PlugIn.SetLoadProtection, plugin), target=True
        )

    yield from map(protection, plugins)
    yield from (
        action(label=f'packages["{package}"]["{marker.name}"]', read=marker.is_file, act=marker.touch, target=True)
        for package, files in held.items()
        for path, plugin in files.items()
        if plugin is None
        for marker in (path.with_name(f"{path.name}.grasshopper-only"),)
    )
    yield from (key(registry_child(plugin), "LoadMode", target=int(PlugInLoadTime.WhenNeeded)) if plugin != Guid.Empty else Skip(name) for name, plugin in bundled.items())
    yield from (Internal.AI_SETTINGS.setting(name, target=target) for name, target in (("AutoLoadMCP", True), ("DefaultAgentName", "claude"), ("DisabledAgents", ())))
    yield from (
        row
        for plugin in edges
        for row in (
            tuple(key((PlugIn.Find(plugin), unwelded), name, target=target) for name, target in (("Color", color(Status.ERROR)), ("Thickness", 1)))
            if PlugIn.LoadPlugIn(plugin)
            else (Error(f'PlugIns["{PlugIn.GetPlugInInfo(plugin).Name}"] did not load, its {unwelded} settings stay unwritten'),)
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
