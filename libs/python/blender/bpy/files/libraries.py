"""Library loads through the host's keywords, inspected blend files, import records, overrides through the host calls, and library writes."""

from collections.abc import Callable, Generator, Mapping, Sequence
from contextlib import AbstractContextManager, contextmanager, ExitStack
from functools import partial
from pathlib import Path
from typing import Literal, Self, TYPE_CHECKING, TypedDict, Unpack

import bpy
from expression import Nothing, Ok, Option, Result, try_downcast
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.store import Handle, member, STORES
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import parsed
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome, RecordReason
from blender.records.stores import folder_made

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class _Writing(TypedDict, total=False):
    """`BlendDataLibraries.write` keywords, the host defaulting each one left out."""

    path_remap: Literal["NONE", "RELATIVE", "RELATIVE_ALL", "ABSOLUTE"]
    fake_user: bool
    compress: bool


# --- [MODELS] ---------------------------------------------------------------------------


class ImportItem(msgspec.Struct, frozen=True, kw_only=True):
    """Import item metadata with ID handles."""

    name: str
    kind: "rna_enums.IdTypeItems"
    action: Literal["UNSET", "KEEP_LINKED", "REUSE_LOCAL", "MAKE_LOCAL", "COPY_LOCAL"]
    info: frozenset[str]
    block: Option[Handle[bpy.types.ID]]
    source: Option[Handle[bpy.types.Library]]
    sources: tuple[Path, ...]
    override: Option[Handle[bpy.types.ID]]
    reusable: Option[Handle[bpy.types.ID]]


class Imported(msgspec.Struct, frozen=True, kw_only=True):
    """Import metadata that outlives handler context."""

    stage: Literal["INIT", "DONE"]
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
                    sources=tuple(Path(library.filepath) for library in item.source_libraries),
                    override=Option.of_optional(item.library_override_id).map(Handle.of),
                    reusable=Option.of_optional(item.reusable_local_id).map(Handle.of),
                )
                for item in context.import_items
            ),
        )


class OverrideResync(msgspec.Struct, frozen=True, kw_only=True):
    """Libraries to resave after resync, with resync leftover objects."""

    libraries: tuple[Handle[bpy.types.Library], ...]
    leftovers: tuple[Handle[bpy.types.Object], ...]

    @classmethod
    def read(cls) -> Self:
        """Returns resync-flagged libraries and `OVERRIDE_RESYNC_LEFTOVERS` objects."""
        leftovers: bpy.types.Collection | None = bpy.data.collections.get("OVERRIDE_RESYNC_LEFTOVERS")
        return cls(
            libraries=tuple(Handle.of(library) for library in bpy.data.libraries if library.needs_liboverride_resync),
            leftovers=tuple(Handle.of(block) for held in Option.of_optional(leftovers).to_list() for block in held.all_objects),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LOADS]


@catch(exception=ValueError)
def _opened(data: bpy.types.BlendData, path: Path, load: Mapping[str, bool]) -> AbstractContextManager[tuple[object, object]]:
    """Opens a library load, the host refusing the open session file, its entry refusing an unreadable one."""
    opened: AbstractContextManager[tuple[object, object]] = data.libraries.load(str(path), **load)  # type: ignore[func-returns-value, assignment]  # ty: ignore[invalid-assignment]
    return opened


def sourced[T](
    data: bpy.types.BlendData, path: Path, take: Callable[[object], tuple[T, Mapping[str, Sequence[str]]]], **load: bool
) -> Outcome[tuple[T, dict[str, tuple[bpy.types.ID | str | None, ...]]]]:
    """Returns source selection with loaded destination slots, read fault when the library load refuses the file.

    Args:
        data: Blend data receiving the load.
        path: Blend file to read.
        take: Selection from the file's listing and the names to load per store.
        **load: `BlendDataLibraries.load` keywords.

    Returns:
        Selection and the loaded blocks per store.
    """

    def requested(ends: tuple[object, object]) -> tuple[object, T, Mapping[str, Sequence[str]]]:
        value, chosen = take(ends[0])
        for name, names in chosen.items():
            setattr(ends[1], name, list(names))
        return ends[1], value, chosen

    unreadable = partial(Fault.of, RecordReason.UNREADABLE, str(path))
    with ExitStack() as loading:

        @catch(exception=OSError)
        def entered(opened: AbstractContextManager[tuple[object, object]]) -> tuple[object, object]:
            return loading.enter_context(opened)

        requests = _opened(data, path, load).map_error(unreadable).bind(lambda opened: entered(opened).map_error(unreadable)).map(requested)
    return requests.map(lambda held: (held[1], {name: tuple(getattr(held[0], name)) for name in held[2]}))


@contextmanager
def inspected(path: Path) -> Generator[Outcome[bpy.types.BlendData]]:
    """Yields temporary BlendData of another file until scope exit, libraries and workspaces left unlinked."""
    stores = tuple(identifier for kind, identifier in STORES.items() if not issubclass(kind, bpy.types.Library | bpy.types.WorkSpace))
    with bpy.data.temp_data(filepath="") as data:  # type: ignore[var-annotated]
        yield sourced(data, path, lambda source: (None, {name: names for name in stores for names in Option.of_optional(getattr(source, name, None)).to_list()}), link=True).map(lambda _: data)  # ty: ignore[unsound-yield]


def _listing(source: object, kind: type[bpy.types.ID], path: Path) -> Outcome[tuple[str, Sequence[str]]]:
    store = member(kind)
    return (
        Option
        .of_optional(None if issubclass(kind, bpy.types.Library) else getattr(source, store, None))
        .map(lambda names: (store, names))
        .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, str(path), member=store))
    )


def listed(path: Path, kind: type[bpy.types.ID], **load: bool) -> Outcome[tuple[str, ...]]:
    """Returns names in blend file store of kind, `assets_only` among the load keywords listing assets alone."""
    return sourced(bpy.data, path, lambda source: (_listing(source, kind, path), {}), **load).bind(lambda answer: answer[0].map(lambda listing: tuple(listing[1])))


def blocks_loaded[T: bpy.types.ID](path: Path, kind: type[T], names: tuple[str, ...], **load: bool) -> Outcome[tuple[T, ...]]:
    """Links or appends named blocks through the host's load keywords, absent fault per missing name or kind.

    Args:
        path: Blend file holding the blocks.
        kind: ID type of every block.
        names: Block names in the file's store of kind.
        **load: `BlendDataLibraries.load` keywords (`link`, `pack`, `relative`, `reuse_local_id`, `create_liboverrides`, ...).

    Returns:
        Loaded blocks in name order.
    """

    def chosen(source: object) -> tuple[Outcome[tuple[str, tuple[str, ...]]], Mapping[str, Sequence[str]]]:
        found = _listing(source, kind, path).bind(
            lambda listing: accumulated(Ok(name).filter_with(listing[1].__contains__, lambda held: Fault.of(BlenderReason.ABSENT, str(path), member=held)) for name in names).map(
                lambda selected: (listing[0], selected)
            )
        )
        return found, found.map(lambda selected: {selected[0]: selected[1]}).default_value({})

    return sourced(bpy.data, path, chosen, **load).bind(
        lambda answer: answer[0].bind(
            lambda picked: accumulated(
                Option.of_optional(try_downcast(kind, block)).to_result(Fault.of(BlenderReason.ABSENT, str(path), member=name)) for name, block in zip(picked[1], answer[1][picked[0]])
            )
        )
    )


def library_written(path: Path, blocks: frozenset[bpy.types.ID], **write: Unpack[_Writing]) -> Outcome[Path]:
    """Writes blocks to library file through the host's write keywords, returning path."""
    return folder_made(path.parent).bind(
        lambda _: catch(bpy.data.libraries.write, exception=OSError)(str(path), set(blocks), **write).map(lambda _: path).map_error(partial(Fault.of, RecordReason.UNWRITABLE, str(path)))
    )


# --- [LIBRARIES]


def reloaded(library: bpy.types.Library, path: Option[Path] = Nothing) -> Outcome[bpy.types.Library]:
    """Reloads library, relocating it to path when given."""
    return (
        accumulated(written(library, ("filepath",), str(target)) for target in path.to_list())
        .bind(lambda _: catch(library.reload, exception=RuntimeError)().map_error(partial(Fault.of, BlenderReason.REFUSED, f"{library!r}.reload")))
        .map(lambda _: library)
    )


# --- [OVERRIDES]


def _made(block: bpy.types.ID, request: str, made: Result[bpy.types.ID | None, RuntimeError]) -> Outcome[bpy.types.ID]:
    """Translates an override creation into the override, refused fault for the host's error, absent fault for an ID taking none."""
    return made.map_error(partial(Fault.of, BlenderReason.REFUSED, request)).bind(
        lambda held: Option.of_optional(held).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(block), member="override_library"))
    )


def _overriding(block: bpy.types.ID) -> Outcome[bpy.types.IDOverrideLibrary]:
    """Returns the ID's library override, absent fault for an ID holding none."""
    return Option.of_optional(block.override_library).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(block), member="override_library"))


def override_created(block: bpy.types.ID, **flags: bool) -> Outcome[bpy.types.ID]:
    """Creates a single-ID override through `ID.override_create` and its keywords (`remap_local_usages`)."""
    return _made(block, f"{block!r}.override_create", catch(block.override_create, exception=RuntimeError)(**flags))


def hierarchy_created(block: bpy.types.ID, scene: bpy.types.Scene, view_layer: bpy.types.ViewLayer, *, reference: Option[bpy.types.ID] = Nothing, **flags: bool) -> Outcome[bpy.types.ID]:
    """Creates an override hierarchy instantiated in scene and view layer through `ID.override_hierarchy_create` and its keywords (`do_fully_editable`)."""
    return _made(block, f"{block!r}.override_hierarchy_create", catch(block.override_hierarchy_create, exception=RuntimeError)(scene, view_layer, reference=reference.to_optional(), **flags))


def override_reset(block: bpy.types.ID, **flags: bool) -> Outcome[None]:
    """Resets the ID's override to its reference values through `IDOverrideLibrary.reset` and its keywords (`do_hierarchy`, `set_system_override`)."""
    return _overriding(block).bind(lambda held: catch(held.reset, exception=RuntimeError)(**flags).map_error(partial(Fault.of, BlenderReason.REFUSED, f"{held!r}.reset")))


def override_destroyed(block: bpy.types.ID, **flags: bool) -> Outcome[None]:
    """Deletes the ID's override after remapping its usages to the linked reference through `IDOverrideLibrary.destroy` and its keywords (`do_hierarchy`)."""
    return _overriding(block).bind(lambda held: catch(held.destroy, exception=RuntimeError)(**flags).map_error(partial(Fault.of, BlenderReason.REFUSED, f"{held!r}.destroy")))


def override_resynced(
    block: bpy.types.ID, scene: bpy.types.Scene, *, view_layer: Option[bpy.types.ViewLayer] = Nothing, residual_storage: Option[bpy.types.Collection] = Nothing, **flags: bool
) -> Outcome[None]:
    """Rebuilds the ID's override hierarchy against its linked references through `IDOverrideLibrary.resync` and its keywords (`do_hierarchy_enforce`, `do_whole_hierarchy`).

    Returns:
        Ok(None), refused fault for the host's error, absent fault when the host finds no linked reference.
    """

    def resynced(held: bpy.types.IDOverrideLibrary) -> Outcome[None]:
        request = f"{held!r}.resync"
        return (
            catch(held.resync, exception=RuntimeError)(scene=scene, view_layer=view_layer.to_optional(), residual_storage=residual_storage.to_optional(), **flags)
            .map_error(partial(Fault.of, BlenderReason.REFUSED, request))
            .filter_with(bool, lambda _: Fault.of(BlenderReason.ABSENT, request, member="reference"))
            .map(lambda _: None)
        )

    return _overriding(block).bind(resynced)


def edits(block: bpy.types.ID) -> Outcome[tuple[Descriptor[bpy.types.Property], ...]]:
    """Returns descriptor per property the ID's library override edits, absent fault for an ID holding no override."""
    return _overriding(block).bind(lambda held: accumulated(parsed(item.rna_path).bind(lambda path: Descriptor.resolve(block, path, bpy.types.Property)) for item in held.properties))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ImportItem",
    "Imported",
    "OverrideResync",
    "blocks_loaded",
    "edits",
    "hierarchy_created",
    "inspected",
    "library_written",
    "listed",
    "override_created",
    "override_destroyed",
    "override_reset",
    "override_resynced",
    "reloaded",
    "sourced",
]
