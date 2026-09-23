# ty: ignore[unresolved-import, unresolved-attribute]
# mypy: disable-error-code="import-not-found, import-untyped, attr-defined"
"""Load a compiled plugin or library into Rhino and prove the held build is the file on disk, imported inside Rhino's Python."""

from pathlib import Path

import clr
from records import Fault, Record
from Rhino.PlugIns import LoadPlugInResult, PlugIn
from System import AppDomain, Guid, Reflection
from System.IO import File, MemoryStream
from System.Reflection import Assembly, Metadata

# --- [MODELS] ---------------------------------------------------------------------------


class AssemblyRecord(Record, frozen=True):
    """Assembly the process holds, built from the file on disk, with its plugin id and English command names when it registered as a plugin."""

    name: str
    path: str
    plugin: str | None = None
    commands: tuple[str, ...] = ()


# --- [COMPOSITION] ----------------------------------------------------------------------


def load(path: str) -> AssemblyRecord | tuple[Fault, ...]:
    """Load a file as a Rhino plugin, or as a library when Rhino refuses it as a plugin, then compare the held module version id with the file's."""
    location = str(Path(path).resolve())
    loaded = PlugIn.LoadPlugIn(location)[0] in {LoadPlugInResult.Success, LoadPlugInResult.SuccessAlreadyLoaded}
    clr.AddReference("System.Reflection.Metadata")
    reader = Reflection.PortableExecutable.PEReader(MemoryStream(File.ReadAllBytes(location)))
    try:
        metadata = Metadata.PEReaderExtensions.GetMetadataReader(reader)
        module = metadata.GetModuleDefinition()
        disk, scope = metadata.GetGuid(module.Mvid), metadata.GetString(module.Name)
    finally:
        reader.Dispose()
    held = next((assembly for assembly in AppDomain.CurrentDomain.GetAssemblies() if assembly.ManifestModule.ScopeName == scope), None) or Assembly.LoadFrom(location)
    return (
        (Fault(Assembly, str(held.ManifestModule.ModuleVersionId), (str(disk),)),)
        if held.ManifestModule.ModuleVersionId != disk
        else AssemblyRecord(held.FullName, held.Location, str(plugin), tuple(PlugIn.GetEnglishCommandNames(plugin)))
        if loaded and (plugin := PlugIn.IdFromPath(held.Location)) != Guid.Empty
        else AssemblyRecord(held.FullName, held.Location)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AssemblyRecord", "load"]
