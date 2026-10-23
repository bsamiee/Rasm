"""Library imports, import records, overrides, and library writes."""

from collections.abc import Callable, Mapping, Sequence
from pathlib import Path
from typing import Literal, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.store import Handle, member
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult, BlenderWriteFault
from blender.bpy.files.blends import folder_made, sourced
from blender.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type LibraryLink = Linked | LocalCopy | Overrides
type OverrideScope = Isolated | Hierarchy
type OverrideChange = Reset | Destroy | Resync

# --- [MODELS] ---------------------------------------------------------------------------


class Linked(msgspec.Struct, frozen=True, kw_only=True):
    """Library link import."""

    pack: bool = False
    relative: bool = True


class Overrides(msgspec.Struct, frozen=True, kw_only=True):
    """Link import with library overrides."""

    relative: bool = True
    reuse_liboverrides: bool = False
    create_liboverrides_runtime: bool = False


class LocalCopy(msgspec.Struct, frozen=True, kw_only=True):
    """Append import."""

    relative: bool = False
    reuse_local_id: bool = False
    set_fake: bool = False
    recursive: bool = True
    clear_asset_data: bool = True


class Isolated(msgspec.Struct, frozen=True, kw_only=True):
    """Single-ID override."""

    remap_local_usages: bool = False


class Hierarchy(msgspec.Struct, frozen=True, kw_only=True):
    """Override hierarchy instantiated in scene and view layer."""

    scene: bpy.types.Scene
    view_layer: bpy.types.ViewLayer
    reference: Option[bpy.types.ID] = Nothing
    do_fully_editable: bool = False


class Reset(msgspec.Struct, frozen=True, kw_only=True):
    """Override reset to reference values."""

    do_hierarchy: bool = True
    set_system_override: bool = False


class Destroy(msgspec.Struct, frozen=True, kw_only=True):
    """Override deleted after remapping usages to its linked reference."""

    do_hierarchy: bool = True


class Resync(msgspec.Struct, frozen=True, kw_only=True):
    """Override hierarchy rebuilt against linked references in scene."""

    scene: bpy.types.Scene
    view_layer: bpy.types.ViewLayer
    residual_storage: Option[bpy.types.Collection] = Nothing
    do_hierarchy_enforce: bool = False
    do_whole_hierarchy: bool = False


class ImportItem(msgspec.Struct, frozen=True, kw_only=True):
    """Import item metadata with ID handles."""

    name: str
    kind: "rna_enums.IdTypeItems"
    action: str
    info: frozenset[str]
    block: Option[Handle[bpy.types.ID]]
    source: Option[Handle[bpy.types.Library]]
    override: Option[Handle[bpy.types.ID]]
    reusable: Option[Handle[bpy.types.ID]]


class Imported(msgspec.Struct, frozen=True, kw_only=True):
    """Import metadata that outlives handler context."""

    stage: str
    options: frozenset[str]
    items: tuple[ImportItem, ...]

    @classmethod
    def of(cls, context: bpy.types.BlendImportContext) -> Self:
        """Copies import context during its handler."""
        return cls(
            stage=context.process_stage,
            options=frozenset(context.options),
            items=tuple(
                ImportItem(
                    name=item.name,
                    kind=item.id_type,
                    action=item.append_action,
                    info=frozenset(item.import_info),
                    block=Option.of_optional(item.id).map(Handle.of),
                    source=Option.of_optional(item.source_library).map(Handle.of),
                    override=Option.of_optional(item.library_override_id).map(Handle.of),
                    reusable=Option.of_optional(item.reusable_local_id).map(Handle.of),
                )
                for item in context.import_items
            ),
        )


class Stale(msgspec.Struct, frozen=True, kw_only=True):
    """Libraries to resave after resync, with resync leftover objects."""

    libraries: tuple[Handle[bpy.types.Library], ...]
    leftovers: tuple[Handle[bpy.types.Object], ...]

    @classmethod
    def read(cls) -> Self:
        """Returns resync-flagged libraries and `OVERRIDE_RESYNC_LEFTOVERS` objects."""
        return cls(
            libraries=tuple(Handle.of(library) for library in bpy.data.libraries if library.needs_liboverride_resync),
            leftovers=catch(lambda: bpy.data.collections["OVERRIDE_RESYNC_LEFTOVERS"].all_objects, exception=KeyError)()
            .map(lambda objects: tuple(Handle.of(block) for block in objects))
            .default_value(()),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOADS]


def _listing(source: object, kind: type[bpy.types.ID], path: Path) -> BlenderResult[tuple[str, Sequence[str]]]:
    store = member(kind)
    faults = (BlenderAbsentFault(owner=str(path), key=store),)
    return Error(faults) if issubclass(kind, bpy.types.Library) else catch(lambda: (store, getattr(source, store)), exception=AttributeError)().map_error(lambda _: faults)


def listed(path: Path, kind: type[bpy.types.ID], *, assets_only: bool = False) -> BlenderResult[tuple[str, ...]]:
    """Returns names in blend file store of kind."""
    return sourced(bpy.data, path, lambda source: (_listing(source, kind, path), {}), assets_only=assets_only).bind(lambda answer: answer[0].map(lambda listing: tuple(listing[1])))


def blocks_loaded[T: bpy.types.ID](path: Path, kind: type[T], names: tuple[str, ...], link: LibraryLink) -> BlenderResult[tuple[T, ...]]:
    """Links or appends named blocks, absent fault per missing name or kind."""

    def chosen(source: object) -> tuple[BlenderResult[tuple[str, tuple[str, ...]]], Mapping[str, Sequence[str]]]:
        found = _listing(source, kind, path).bind(
            lambda listing: accumulated(Ok(name) if name in listing[1] else Error((BlenderAbsentFault(owner=str(path), key=name),)) for name in names).map(lambda selected: (listing[0], selected))
        )
        return found, found.map(lambda selected: {selected[0]: selected[1]}).default_value({})

    match link:
        case Linked():
            keywords = {"link": True} | msgspec.structs.asdict(link)
        case LocalCopy():
            keywords = {"link": False} | msgspec.structs.asdict(link)
        case Overrides():
            keywords = {"link": True, "create_liboverrides": True} | msgspec.structs.asdict(link)

    return sourced(bpy.data, path, chosen, **keywords).bind(
        lambda answer: answer[0].bind(
            lambda selected: accumulated(Ok(block) if isinstance(block, kind) else Error((BlenderAbsentFault(owner=str(path), key=name),)) for name, block in zip(selected[1], answer[1][selected[0]]))
        )
    )


def library_written(
    path: Path, blocks: frozenset[bpy.types.ID], *, remap: Literal["NONE", "RELATIVE", "RELATIVE_ALL", "ABSOLUTE"] = "RELATIVE_ALL", fake_user: bool = True, compress: bool = False
) -> BlenderResult[Path]:
    """Writes blocks to library file, returning path."""
    return folder_made(path.parent).bind(
        lambda _: (
            catch(lambda: bpy.data.libraries.write(str(path), set(blocks), path_remap=remap, fake_user=fake_user, compress=compress), exception=OSError)()
            .map(lambda _: path)
            .map_error(lambda error: (BlenderWriteFault(request=path, error=error),))
        )
    )


# --- [LIBRARIES]


def reloaded(library: bpy.types.Library, path: Option[Path] = Nothing) -> BlenderResult[bpy.types.Library]:
    """Reloads library, relocating it to path when given."""
    try:
        for target in path.to_list():
            library.filepath = str(target)
        library.reload()
    except (AttributeError, RuntimeError) as error:
        return Error((BlenderEditFault(request=f"{library!r}.filepath", error=error),))
    return Ok(library)


# --- [OVERRIDES]


def overridden(block: bpy.types.ID, scope: OverrideScope) -> BlenderResult[bpy.types.ID]:
    """Creates override, absent fault when ID takes none."""
    match scope:
        case Isolated(remap_local_usages=remap):
            made = block.override_create(remap_local_usages=remap)
        case Hierarchy(scene=scene, view_layer=view_layer, reference=reference, do_fully_editable=editable):
            made = block.override_hierarchy_create(scene, view_layer, reference=reference.to_optional(), do_fully_editable=editable)
    return Option.of_optional(made).to_result_with(lambda: (BlenderAbsentFault(owner=repr(block), key="override_library"),))


def overriding(block: bpy.types.ID) -> BlenderResult[bpy.types.IDOverrideLibrary]:
    """Returns override data of ID."""
    return Option.of_optional(block.override_library).to_result_with(lambda: (BlenderAbsentFault(owner=repr(block), key="override_library"),))


def override_changed(block: bpy.types.ID, change: OverrideChange) -> BlenderResult[None]:
    """Resets, destroys, or resyncs override, absent fault for missing reference."""

    def changed(held: bpy.types.IDOverrideLibrary) -> BlenderResult[None]:
        request = repr(held)
        apply: Callable[[], bool | None]
        match change:
            case Reset():
                apply = lambda: held.reset(**msgspec.structs.asdict(change))
            case Destroy():
                apply = lambda: held.destroy(**msgspec.structs.asdict(change))
            case Resync(scene=scene, view_layer=view_layer, residual_storage=residual, do_hierarchy_enforce=enforce, do_whole_hierarchy=whole):
                apply = lambda: held.resync(scene=scene, view_layer=view_layer, residual_storage=residual.to_optional(), do_hierarchy_enforce=enforce, do_whole_hierarchy=whole)
        try:
            done = apply()
        except RuntimeError as error:
            return Error((BlenderEditFault(request=request, error=error),))
        return Error((BlenderAbsentFault(owner=request, key="reference"),)) if done is False else Ok(None)

    return overriding(block).bind(changed)


def edits(block: bpy.types.ID) -> BlenderResult[tuple[Descriptor[bpy.types.Property], ...]]:
    """Returns descriptor per override property."""
    return overriding(block).bind(lambda held: accumulated(Descriptor.resolve(type(block), item.rna_path, bpy.types.Property) for item in held.properties))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Destroy",
    "Hierarchy",
    "ImportItem",
    "Imported",
    "Isolated",
    "LibraryLink",
    "Linked",
    "LocalCopy",
    "OverrideChange",
    "OverrideScope",
    "Overrides",
    "Reset",
    "Resync",
    "Stale",
    "blocks_loaded",
    "edits",
    "library_written",
    "listed",
    "overridden",
    "override_changed",
    "overriding",
    "reloaded",
]
