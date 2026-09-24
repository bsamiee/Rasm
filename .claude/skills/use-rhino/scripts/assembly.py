# ty: ignore[unresolved-import, unresolved-attribute]
# mypy: disable-error-code="import-not-found, import-untyped, attr-defined"
"""Load a compiled plugin or library into Rhino and prove the held build matches the file on disk."""

from pathlib import Path

import clr
from records import Fault, Record
from Rhino.PlugIns import LoadPlugInResult, PlugIn
from System import AppDomain, Guid, Reflection
from System.IO import File, MemoryStream
from System.Reflection import Assembly, Metadata

# --- [MODELS] ---------------------------------------------------------------------------


class AssemblyRecord(Record, frozen=True):
    """Assembly Rhino holds, with plugin id and English command names when it registered as a plugin."""

    name: str
    path: str
    plugin: str | None = None
    commands: tuple[str, ...] = ()


# --- [COMPOSITION] ----------------------------------------------------------------------


def load(path: str) -> AssemblyRecord | tuple[Fault, ...]:
    """Load a file as a Rhino plugin, else as a library, refused when the held build's module version differs from the file's."""
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
    match held.ManifestModule.ModuleVersionId, PlugIn.IdFromPath(held.Location) if loaded else None:
        case version, _ if version != disk:
            return (Fault(Assembly, str(version), (str(disk),)),)
        case _, Guid() as plugin:
            return AssemblyRecord(held.FullName, held.Location, str(plugin), tuple(PlugIn.GetEnglishCommandNames(plugin)))
        case _:
            return AssemblyRecord(held.FullName, held.Location)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AssemblyRecord", "load"]
