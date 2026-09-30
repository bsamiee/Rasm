# ty: ignore[invalid-argument-type]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, misc"
"""Rhino plug-in rows: the load protection or Grasshopper 2 marker of each package file, the bundled plug-ins' load modes, and package plug-in settings."""

from collections.abc import Iterator
from functools import partial
from itertools import chain
from pathlib import Path
import tomllib

import Rhino
from Rhino.PlugIns import PlugIn, PlugInLoadTime
from Rhino.Runtime import HostUtils
from System import Guid

from interface.report import Action, Kind, line, Row
from interface.rhino.script.accessors import color, found, Internal, key, located, opened
from interface.roles import Status

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [PACKAGES]
def installed() -> dict[str, dict[Path, Guid | None]]:
    """Plug-in files of each package folder Rhino resolves under the user packages folder by package id, each with the id of the plug-in record Rhino registered for its file, a native plug-in bundle included, or None for a file Rhino registered no plug-in from."""
    root, records = Path(HostUtils.AutoInstallPlugInFolder(currentUser=True)), {Path(PlugIn.GetPlugInInfo(plugin).FileName): plugin for plugin in PlugIn.GetInstalledPlugIns().Keys}
    return {
        folder.relative_to(root).parts[0]: {path: records.get(path) for path in sorted(folder.glob("*.rhp"))}
        for folder in (Path(held.FullName) for held in HostUtils.GetActivePlugInVersionFolders())
        if folder.is_relative_to(root)
    }


def registry_child(plugin: Guid) -> tuple[str, ...]:
    """Settings path of the plug-in's record under the registry version holding it."""
    path, record = ("PlugInRegistry",), str(plugin)
    return next((*path, version, record) for version in opened(path).ChildKeys if located((*path, version, record)) is not None)


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> Iterator[Row | str]:
    """Rows of every package plug-in's silent load and marker, then the bundled plug-ins' load modes, a skip line for one Rhino registered no record of, and the agent settings, then the settings of plug-ins a row loads."""
    held, declared = installed(), tomllib.loads(Path(__file__).parents[1].joinpath("packages.toml").read_text(encoding="utf-8"))["packages"]
    unwelded = "ShowUnweldedEdges"
    edges = next(row["id"] for row in declared if unwelded in chain.from_iterable(row.get("commands", {}).values()))
    bundled = {name: PlugIn.IdFromName(name) for name in (f"3DxRhino.{Rhino.RhinoApp.ExeVersion}", "PanelingTools")}
    yield from (
        Row(
            label=f'PlugIns["{PlugIn.GetPlugInInfo(plugin).Name}"].LoadProtection',
            read=lambda plugin=plugin: found(PlugIn.GetLoadProtection(plugin)),
            write=partial(PlugIn.SetLoadProtection, plugin),
            target=True,
        )
        for files in held.values()
        for plugin in files.values()
        if plugin is not None
    )
    yield from (
        Action(label=f'packages["{package}"]["{marker.name}"]', read=marker.is_file, act=marker.touch, target=True)
        for package, files in held.items()
        for path, plugin in files.items()
        if plugin is None
        for marker in (path.with_name(f"{path.name}.grasshopper-only"),)
    )
    yield from (key(registry_child(plugin), "LoadMode", target=int(PlugInLoadTime.WhenNeeded)) if plugin != Guid.Empty else line(Kind.SKIP, name) for name, plugin in bundled.items())
    yield from (Internal.AI_SETTINGS.setting(name, target=target) for name, target in (("AutoLoadMCP", True), ("DefaultAgentName", "claude"), ("DisabledAgents", ())))
    yield from (
        row
        for plugin in held[edges].values()
        if plugin is not None
        for row in (
            tuple(key((PlugIn.Find(plugin), unwelded), name, target=target) for name, target in (("Color", color(Status.ERROR)), ("Thickness", 1)))
            if PlugIn.LoadPlugIn(plugin)
            else (line(Kind.ERROR, f'PlugIns["{PlugIn.GetPlugInInfo(plugin).Name}"] did not load, its {unwelded} settings stay unwritten'),)
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
