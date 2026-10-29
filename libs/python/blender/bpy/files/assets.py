"""Asset metadata and previews, catalogs over the host catalog files, extension asset libraries, imports by import method, and publication."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
from pathlib import Path
from types import MappingProxyType
from typing import Annotated, Final, Protocol, Self, TYPE_CHECKING
from uuid import NAMESPACE_URL, uuid5

from _bpy_internal.assets.remote_library.listing_asset_catalogs import parse_catalogs
import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderReason
from blender.bpy.files.libraries import blocks_loaded, library_written, listed
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason
from blender.records.stores import folder_made

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type CatalogPath = Annotated[str, msgspec.Meta(pattern=r"\A[^:/\\\r\n]+(?:/[^:/\\\r\n]+)*\Z")]
type ImportMethod = rna_enums.AssetImportMethodItems


class _Catalog(Protocol):
    """Catalog row `parse_catalogs` returns, whose stub lacks its fields."""

    path: str
    uuids: list[str]


class _Resource(Protocol):
    """`bpy.utils.system_resource`, which the stubs lack, its path keyword kept."""

    def __call__(self, kind: str, /, *, path: str) -> str: ...


# --- [CONSTANTS] ------------------------------------------------------------------------

SYSTEM_RESOURCE: Final[_Resource] = bpy.utils.system_resource  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_PARSED: Final[Callable[[Path], list[_Catalog]]] = parse_catalogs  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_UNREADABLE: Final[type[OSError | UnicodeError]] = (OSError, UnicodeError)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_ESSENTIALS: Final = frozenset({"ESSENTIALS", "ONLINE_ESSENTIALS"})
_LOADS: Final[Mapping[str, Mapping[str, bool]]] = MappingProxyType({
    "LINK": MappingProxyType({"link": True}),
    "PACK": MappingProxyType({"link": True, "pack": True}),
    "APPEND": MappingProxyType({"link": False, "relative": False, "recursive": True, "clear_asset_data": True}),
    "APPEND_REUSE": MappingProxyType({"link": False, "relative": False, "recursive": True, "clear_asset_data": True, "reuse_local_id": True}),
})

# --- [MODELS] ---------------------------------------------------------------------------


class AssetMark(msgspec.Struct, frozen=True, kw_only=True):
    """Asset metadata under extension key."""

    key: Key
    catalog: CatalogPath
    description: str = ""
    author: str = ""
    copyright: str = ""
    license: str = ""
    tags: frozenset[str] = frozenset()
    method: Option[ImportMethod] = Nothing

    @staticmethod
    def key_of(identity: Identity, asset: bpy.types.AssetRepresentation) -> Option[Key]:
        """Returns the mark key the extension stored on an asset, `Nothing` for an asset it never marked."""
        return Option.of_optional(asset.metadata).bind(lambda data: Option.of_optional(try_downcast(str, data.get(identity.manifest.id))))

    def marked[T: bpy.types.ID](self, identity: Identity, block: T, uid: str) -> Outcome[T]:
        """Marks block as asset, generating a preview where it holds none, and replaces its metadata.

        Args:
            identity: Extension with manifest ID keying mark.
            block: ID to mark.
            uid: Catalog UUID publication read or created.

        Returns:
            Marked block, invalid fault for a catalog path outside its bound, edit fault for unmarkable block, faults per refused member write.
        """

        def applied(data: bpy.types.AssetMetaData) -> Outcome[T]:
            for tag in tuple(data.tags):
                if tag.name not in self.tags:
                    data.tags.remove(tag)
            for name in sorted(self.tags - frozenset(tag.name for tag in data.tags)):
                data.tags.new(name)
            data[identity.manifest.id] = self.key
            return assigned(
                data,
                {
                    ("description",): self.description,
                    ("author",): self.author,
                    ("copyright",): self.copyright,
                    ("license",): self.license,
                    ("catalog_id",): uid,
                    ("use_preferred_import_method",): self.method.is_some(),
                    ("preferred_import_method",): self.method.default_value(data.preferred_import_method),
                },
            ).map(lambda _: block)

        def mark(_: str) -> Outcome[T]:
            block.asset_mark()
            if block.preview is None:
                block.asset_generate_preview()
            return Option.of_optional(block.asset_data).map(applied).default_with(lambda: Error(Fault.of(BlenderReason.ABSENT, repr(block), member="asset_data")))

        return catch(msgspec.convert, exception=msgspec.ValidationError)(self.catalog, CatalogPath).map_error(partial(Fault.of, RecordReason.INVALID, self.catalog)).bind(mark)


class UserLibrary(msgspec.Struct, frozen=True, kw_only=True):
    """Preferences asset library row over the extension's asset folder, stamped with the extension ID the extension manager reads, held while the extension is enabled."""

    title: str
    folder: Key
    method: Option[ImportMethod] = Nothing

    def located(self, identity: Identity) -> Path:
        """Returns library folder under extension user folder."""
        return identity.folder / "assets" / self.folder

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns row acquisition."""
        return Ok(((Band.RESOURCES, self.acquired(identity)),))

    @contextmanager
    def acquired(self, identity: Identity) -> Generator[Outcome[None]]:
        """Holds the extension's library row, reusing the saved one, until release.

        Args:
            identity: Extension owning library folder, its `{repository}.{id}` package tail the row's extension ID.

        Yields:
            Success while row is held, absent fault when Blender adds none.
        """
        stamp = identity.package.partition(".")[2]
        with ExitStack() as lifetime:

            def held(folder: Path, rows: bpy.types.AssetLibraryCollection) -> Outcome[bpy.types.PreferencesAssetLibrary]:
                made = (
                    Block
                    .of_seq(rows)
                    .choose(lambda row: Option.of_optional(try_downcast(bpy.types.PreferencesAssetLibrary, row)))
                    .filter(lambda row: row.extension_id == stamp)
                    .try_head()
                    .map(Ok)
                    .default_with(
                        lambda: catch(rows.new, exception=RuntimeError)(name=self.title, directory=str(folder), extension_id=stamp).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(rows)))
                    )
                    .bind(lambda row: Option.of_optional(try_downcast(bpy.types.PreferencesAssetLibrary, row)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(rows), member=self.title)))
                )
                for row in made.to_option().to_list():
                    lifetime.callback(rows.remove, row)
                return made

            preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            yield (
                folder_made(self.located(identity))
                .bind(lambda folder: held(folder, preferences.filepaths.asset_libraries).map(lambda row: (folder, row)))
                .bind(lambda pair: assigned(pair[1], {("name",): self.title, ("path",): str(pair[0]), ("import_method",): self.method.default_value(pair[1].import_method)}))
            )


class _Publishing(msgspec.Struct, frozen=True, kw_only=True):
    """Library write the reports queue holds until the preview job finishes, clearing the asset marks publication added."""

    target: Path
    blocks: frozenset[bpy.types.ID]
    fresh: tuple[bpy.types.ID, ...]

    def advanced(self) -> Outcome[Option[Self]]:
        """Waits while a preview renders, else writes the marked blocks with relative paths and fake users, clearing the added marks after."""

        def written() -> Outcome[Option[Self]]:
            with ExitStack() as clearing:
                for block in self.fresh:
                    clearing.callback(block.asset_clear)
                return library_written(self.target, self.blocks, path_remap="RELATIVE_ALL", fake_user=True).map(lambda _: Nothing)

        return Ok(Some(self)) if bpy.app.is_job_running("RENDER_PREVIEW") else written()


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PUBLICATION]


@catch(exception=OSError)
def _appended(target: Path, rows: Mapping[str, str]) -> Path:
    """Appends catalog rows to the definition file, the version line opening a new one."""
    with target.open("a", encoding="utf-8") as file:
        file.write(("VERSION 1\n\n" if file.tell() == 0 else "") + "".join(f"{uid}:{path}:{path.replace('/', '-')}\n" for path, uid in rows.items()))
    return target


def published(identity: Identity, folder: Path, stem: str, entries: tuple[tuple[bpy.types.ID, AssetMark], ...], *, kept: bool = False) -> Outcome[Path]:
    """Marks blocks, appends the catalogs they add, and writes them into the library once their previews render.

    Args:
        identity: Extension with manifest ID keying each mark and seeding catalog UUIDs, its reports queue holding the write.
        folder: Library folder for catalog and blend files.
        stem: Blend file name without suffix.
        entries: IDs with asset metadata.
        kept: Whether added asset marks stay.

    Returns:
        Library blend file, written now or on a later pass while a preview renders, accumulating catalog, mark, and write faults.
    """
    target, catalogs = folder / f"{stem}.blend", folder / "blender_assets.cats.txt"
    fresh = () if kept else tuple(block for block, _ in entries if block.asset_data is None)

    def catalogued(rows: list[_Catalog]) -> Outcome[Mapping[str, str]]:
        held = {row.path: uid for row in rows for uid in Block.of_seq(row.uuids).try_head().to_list()}
        added = {path: str(uuid5(NAMESPACE_URL, f"{identity.manifest.id}/{path}")) for path in sorted({mark.catalog for _, mark in entries} - held.keys())}
        return (_appended(catalogs, added).map_error(partial(Fault.of, RecordReason.UNWRITABLE, str(catalogs))) if added else Ok(catalogs)).map(lambda _: held | added)

    def marked(uids: Mapping[str, str]) -> Outcome[Path]:
        return (
            accumulated(mark.marked(identity, block, uids[mark.catalog]) for block, mark in entries)
            .bind(lambda blocks: identity.reports.queued(_Publishing(target=target, blocks=frozenset(blocks), fresh=fresh)))
            .map(lambda _: target)
        )

    return folder_made(folder).bind(lambda _: catch(_PARSED, exception=_UNREADABLE)(folder).map_error(partial(Fault.of, RecordReason.UNREADABLE, str(catalogs)))).bind(catalogued).bind(marked)


# --- [IMPORTS]


def loading(method: ImportMethod, *, relative: bool) -> Mapping[str, bool]:
    """Returns the library load keywords of an asset import method, relative paths applying to links."""
    return {"relative": relative, **_LOADS[method]}


def essential() -> ImportMethod:
    """Returns Essentials import method for packing preference."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return "APPEND_REUSE" if preferences.experimental.no_data_block_packing else "PACK"


def imported[T: bpy.types.ID](asset: bpy.types.AssetRepresentation, kind: type[T]) -> Outcome[T]:
    """Returns local asset data, else imports it by the preferred or owning library's import method.

    Args:
        asset: Representation from asset shelf, browser, or menu.
        kind: Expected ID type.

    Returns:
        Local or imported block, absent fault for another kind or a library holding no preferences row.
    """
    preferred = Option.of_optional(asset.metadata).filter(lambda data: data.use_preferred_import_method).map(lambda data: data.preferred_import_method)

    def keywords(library: bpy.types.AssetLibrary) -> Outcome[Mapping[str, bool]]:
        if library.type in _ESSENTIALS:
            return Ok(loading(preferred.default_with(essential), relative=True))
        return (
            Option
            .of_optional(library.user_library)
            .map(lambda row: loading(preferred.default_value(row.import_method), relative=row.use_relative_path))
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, asset.full_path, member="user_library"))
        )

    match Option.of_optional(asset.local_id):
        case Option(tag="some", some=block):
            return Option.of_optional(try_downcast(kind, block)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, asset.full_path, member=asset.name))
        case Option():
            return (
                Option
                .of_optional(asset.owner_asset_library)
                .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, asset.full_path, member="owner_asset_library"))
                .bind(keywords)
                .bind(lambda load: blocks_loaded(Path(asset.full_library_path), kind, (asset.name,), **load))
                .map(lambda blocks: blocks[0])
            )


@catch(exception=OSError)
def _blends(root: Path) -> tuple[Path, ...]:
    """Lists every blend file under root in path order, a folder the walk cannot read raising."""
    return tuple(sorted(root.rglob("*.blend")))


def bundled[T: bpy.types.ID](kind: type[T], name: str) -> Outcome[T]:
    """Imports named asset from bundled asset libraries.

    Args:
        kind: ID type to list and import.
        name: Asset name in bundled blend file.

    Returns:
        Imported block, absent fault for missing name.
    """
    root = Path(SYSTEM_RESOURCE("DATAFILES", path="assets"))
    return (
        Result[tuple[Path, ...], Faults]
        .bind(
            _blends(root).map_error(partial(Fault.of, RecordReason.UNREADABLE, str(root))),
            lambda paths: accumulated(listed(path, kind, assets_only=True) for path in paths).bind(
                lambda listings: (
                    Block
                    .of_seq(zip(paths, listings))
                    .filter(lambda pair: name in pair[1])
                    .map(lambda pair: pair[0])
                    .try_head()
                    .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, str(root), member=name))
                )
            ),
        )
        .bind(lambda path: blocks_loaded(path, kind, (name,), **loading(essential(), relative=True)))
        .map(lambda blocks: blocks[0])
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["SYSTEM_RESOURCE", "AssetMark", "CatalogPath", "ImportMethod", "UserLibrary", "bundled", "essential", "imported", "loading", "published"]
