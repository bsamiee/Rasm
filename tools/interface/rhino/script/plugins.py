# ty: ignore[unresolved-import, unresolved-attribute, invalid-argument-type]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, attr-defined, misc"
# ruff: file-ignore[import-outside-top-level]
"""Rhino plug-in rows: the load protection or Grasshopper 2 marker of each package file, the bundled plug-ins' load modes, and package plug-in settings."""

from collections.abc import Iterator
from functools import partial
from itertools import chain
from pathlib import Path
import tomllib
from typing import Final

import clr
import Rhino
from Rhino.PlugIns import PlugIn, PlugInLoadTime
from Rhino.Runtime import HostUtils
from System import Guid

from interface.report import Row
from interface.rhino.script.accessors import color, found, Internal, key, located, opened
from interface.roles import Status

# --- [CONSTANTS] ------------------------------------------------------------------------

UNWELDED_EDGES: Final = "ShowUnweldedEdges"

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [PACKAGES]
def defines_plugin(path: Path) -> bool:
    """Whether the assembly file exports a concrete `PlugIn` subclass, read from its metadata and that of the assemblies beside it without loading any."""
    clr.AddReference("System.Reflection.Metadata")
    from System.IO import File
    from System.Reflection import TypeAttributes
    from System.Reflection.Metadata import AssemblyReferenceHandle, HandleKind, PEReaderExtensions, TypeDefinitionHandle, TypeReferenceHandle, TypeSpecificationHandle
    from System.Reflection.PortableExecutable import PEReader

    rhino = clr.GetClrType(PlugIn)

    def exported(reader: object, held: object) -> bool:
        """Whether the type definition is visible outside its assembly."""
        visibility = held.Attributes & TypeAttributes.VisibilityMask
        return visibility == TypeAttributes.Public or (visibility == TypeAttributes.NestedPublic and exported(reader, reader.GetTypeDefinition(held.GetDeclaringType())))

    def defined(reader: object, namespace: str, name: str) -> object:
        """Type definition of the namespace and name, None when the assembly defines none."""
        return next((held for held in map(reader.GetTypeDefinition, reader.TypeDefinitions) if reader.GetString(held.Namespace) == namespace and reader.GetString(held.Name) == name), None)

    def derived(reader: object, base: object) -> bool:
        """Whether the base type resolves to a RhinoCommon type `PlugIn` is assignable from."""
        match base.Kind:
            case HandleKind.TypeDefinition:
                return derived(reader, reader.GetTypeDefinition(TypeDefinitionHandle.op_Explicit(base)).BaseType)
            case HandleKind.TypeSpecification:
                signature = reader.GetBlobReader(reader.GetTypeSpecification(TypeSpecificationHandle.op_Explicit(base)).Signature)
                signature.ReadSignatureTypeCode()
                signature.ReadSignatureTypeCode()
                return derived(reader, signature.ReadTypeHandle())
            case HandleKind.TypeReference:
                held = reader.GetTypeReference(TypeReferenceHandle.op_Explicit(base))
                scope, namespace, name = held.ResolutionScope, reader.GetString(held.Namespace), reader.GetString(held.Name)
                assembly = reader.GetString(reader.GetAssemblyReference(AssemblyReferenceHandle.op_Explicit(scope)).Name) if scope.Kind == HandleKind.AssemblyReference else None
                sibling = readers.get(assembly)
                definition = None if sibling is None else defined(sibling, namespace, name)
                return (
                    rhino.IsAssignableFrom(rhino.Assembly.GetType(f"{namespace}.{name}"))
                    if assembly == rhino.Assembly.GetName().Name
                    else definition is not None and derived(sibling, definition.BaseType)
                )
            case _:
                return False

    images = [PEReader(File.OpenRead(str(file))) for file in (path, *sorted(each for each in path.parent.glob("*.dll") if each.is_file()))]
    try:
        if not images[0].HasMetadata:
            return False
        own, *_ = metadata = [PEReaderExtensions.GetMetadataReader(image) for image in images if image.HasMetadata]
        readers = {reader.GetString(reader.GetAssemblyDefinition().Name): reader for reader in metadata if reader.IsAssembly}
        return any(exported(own, held) and not held.Attributes.HasFlag(TypeAttributes.Abstract) and derived(own, held.BaseType) for held in map(own.GetTypeDefinition, own.TypeDefinitions))
    finally:
        for image in images:
            image.Dispose()


def installed() -> dict[str, dict[Path, Guid | None]]:
    """Plug-in files of each package folder Rhino resolves under the user packages folder by package id, each with its plug-in id, a native plug-in bundle included, or None for a managed file holding no Rhino plug-in."""
    root = Path(HostUtils.AutoInstallPlugInFolder(currentUser=True))
    return {
        folder.relative_to(root).parts[0]: {path: PlugIn.IdFromPath(str(path)) if path.is_dir() or defines_plugin(path) else None for path in sorted(folder.glob("*.rhp"))}
        for folder in (Path(held.FullName) for held in HostUtils.GetActivePlugInVersionFolders())
        if folder.is_relative_to(root)
    }


def registry_child(plugin: Guid) -> tuple[str, ...]:
    """Settings path of the plug-in's record under the registry version holding it."""
    path, record = ("PlugInRegistry",), str(plugin)
    return next((*path, version, record) for version in opened(path).ChildKeys if located((*path, version, record)) is not None)


def unwelded_edges(plugin: Guid) -> tuple[Row, ...]:
    """Rows of the unwelded edge command's color and line thickness, the plug-in loaded first."""
    PlugIn.LoadPlugIn(plugin)
    command = next(each for each in PlugIn.Find(plugin).GetCommands() if each.EnglishName == UNWELDED_EDGES)
    return (key((command,), "Color", target=color(Status.ERROR)), key((command,), "Thickness", target=1))


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> Iterator[Row]:
    """Rows of every package plug-in's silent load and marker, then the bundled plug-ins' load modes and the agent settings, then the settings of plug-ins a row loads."""
    held, declared = installed(), tomllib.loads(Path(__file__).parents[1].joinpath("packages.toml").read_text(encoding="utf-8"))["packages"]
    edges = next(row["id"] for row in declared if UNWELDED_EDGES in chain.from_iterable(row.get("commands", {}).values()))
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
        Row(label=f'packages["{package}"]["{marker.name}"]', read=marker.is_file, write=lambda _, marker=marker: marker.touch(), target=True)
        for package, files in held.items()
        for path, plugin in files.items()
        if plugin is None
        for marker in (path.with_name(f"{path.name}.grasshopper-only"),)
    )
    yield from (key(registry_child(PlugIn.IdFromName(name)), "LoadMode", target=int(PlugInLoadTime.WhenNeeded)) for name in (f"3DxRhino.{Rhino.RhinoApp.ExeVersion}", "PanelingTools"))
    yield from (Internal.AI_SETTINGS.setting(name, target=target) for name, target in (("AutoLoadMCP", True), ("DefaultAgentName", "claude"), ("DisabledAgents", ())))
    yield from (row for plugin in held[edges].values() if plugin is not None for row in unwelded_edges(plugin))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows"]
