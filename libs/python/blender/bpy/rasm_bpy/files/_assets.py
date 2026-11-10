"""Asset metadata and previews, catalogs over the host catalog files, extension asset libraries, imports by import method, and publication."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
from pathlib import Path, PurePosixPath
from types import MappingProxyType
from typing import Annotated, Final, Never, Protocol, Self, TYPE_CHECKING
from uuid import NAMESPACE_URL, uuid5

from _bpy_internal.assets.remote_library.blender_asset_library_openapi import AssetLibraryMeta, CatalogV1  # ruff: ignore[import-private-name]
from _bpy_internal.assets.remote_library.listing_asset_catalogs import _parse_catalog, write  # ty: ignore[import-not-found]  # ruff: ignore[import-private-name]
import bpy
from expression import Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Tier
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.ownership import tracked
from rasm_bpy.data._rna.descriptors import Descriptor
from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.files._libraries import blocks_loaded, library_listing, sourced
from rasm_records import accumulated, committed, Error, Fault, folder_made, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _CatalogPath = Annotated[str, msgspec.Meta(pattern=r"\A(?!(?:.*/)?\.(?:/|\Z))[^:/\\\s]+(?:[ /][^:/\\\s]+)*\Z")]


class _Catalog(Protocol):
    """_Catalog row `_parse_catalog` returns, whose stub lacks its fields."""

    uuid: str
    path: PurePosixPath
    simple_name: str


# --- [CONSTANTS] ------------------------------------------------------------------------

_ESSENTIALS: Final = frozenset({"ESSENTIALS", "ONLINE_ESSENTIALS"})
_LOADS: Final[Mapping[str, Mapping[str, bool]]] = MappingProxyType({
    "LINK": MappingProxyType({"link": True}),
    "PACK": MappingProxyType({"link": True, "pack": True}),
    "APPEND": MappingProxyType({"link": False, "relative": False, "recursive": True, "clear_asset_data": True}),
    "APPEND_REUSE": MappingProxyType({"link": False, "relative": False, "recursive": True, "clear_asset_data": True, "reuse_local_id": True}),
})

# --- [MODELS] ---------------------------------------------------------------------------


class AssetMark(msgspec.Struct, frozen=True, kw_only=True):
    """Asset metadata under extension key, an absent text member leaving the host's value in place."""

    key: Key
    catalog: _CatalogPath
    description: str | None = None
    author: str | None = None
    copyright: str | None = None
    license: str | None = None
    tags: frozenset[str] = frozenset()
    method: Option["rna_enums.AssetImportMethodItems"] = Nothing

    def __post_init__(self) -> None:
        """Refuses a catalog path `_CatalogPath` excludes, which direct construction leaves unchecked."""
        msgspec.convert(self.catalog, _CatalogPath)

    @staticmethod
    def key_of(identity: Identity, asset: bpy.types.AssetRepresentation) -> Option[Key]:
        """Returns the mark key the extension stored on an asset, `Nothing` for an asset it never marked."""
        return Option.of_optional(asset.metadata).bind(lambda data: Option.of_optional(try_downcast(str, data.get(identity.manifest.id))))

    def marked[T: bpy.types.ID](self, identity: Identity, block: T, uid: str) -> Outcome[T]:
        """Marks block as asset, generating a preview where it holds none, and replaces its metadata.

        Args:
            identity: Extension with manifest ID keying mark.
            block: ID to mark.
            uid: _Catalog UUID publication read or created.

        Returns:
            Marked block, absent fault for an unmarkable block, or faults per refused member write.
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

        block.asset_mark()
        if block.preview is None:
            block.asset_generate_preview()
        return Option.of_optional(block.asset_data).map(applied).default_with(lambda: Error(Fault.of(Reason.ABSENT, repr(block), member="asset_data")))


class UserLibrary(msgspec.Struct, frozen=True, kw_only=True):
    """Preferences asset library row over the extension's asset folder, stamped with its extension ID, held while the extension is enabled."""

    title: str
    folder: Key
    method: Option["rna_enums.AssetImportMethodItems"] = Nothing

    def located(self, identity: Identity) -> Path:
        """Returns library folder under extension user folder."""
        return identity.folder / "assets" / self.folder

    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Returns row acquisition."""
        return ((Tier.RESOURCES, self.acquired(identity)),)

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
                found: Outcome[bpy.types.PreferencesAssetLibrary] = (
                    Block
                    .of_seq(rows)
                    .choose(lambda row: Option.of_optional(try_downcast(bpy.types.PreferencesAssetLibrary, row)))
                    .filter(lambda row: row.extension_id == stamp)
                    .try_head()
                    .map(Ok)
                    .default_with(lambda: catch(rows.new, exception=RuntimeError)(name=self.title, directory=str(folder), extension_id=stamp).map_error(partial(Fault.of, Refusal.REFUSED, repr(rows))))
                    .bind(lambda row: Option.of_optional(try_downcast(bpy.types.PreferencesAssetLibrary, row)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(rows), member=self.title)))
                )
                return tracked(lifetime, rows.remove, found)

            yield (
                paired(folder_made(self.located(identity)), rooted(bpy.types.Preferences, bpy.context))
                .bind(lambda pair: held(pair[0], pair[1].filepaths.asset_libraries).map(lambda row: (pair[0], row)))
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
                return (
                    folder_made(self.target.parent)
                    .bind(
                        lambda _: catch(bpy.data.libraries.write, exception=OSError)(str(self.target), set(self.blocks), path_remap="RELATIVE_ALL", fake_user=True).map_error(
                            partial(Fault.of, Reason.UNWRITABLE, str(self.target))
                        )
                    )
                    .map(lambda _: Nothing)
                )

        return Ok(Some(self)) if bpy.app.is_job_running("RENDER_PREVIEW") else written()


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PUBLICATION]


def _text_called[T](step: Callable[[], T], /) -> Result[T, OSError | UnicodeError]:
    """Runs one catalog file call, which raises for an unreadable or unwritable file and for text outside UTF-8."""
    try:
        return Result[Never, Never].Ok(step())
    except (OSError, UnicodeError) as error:
        return Error(error)


def _catalog_rows(catalogs: Path) -> tuple[tuple[_Catalog, ...], frozenset[tuple[str, ...]]]:
    """Returns the rows the host parser keeps from one catalog file and the UUID and path fields of each line beyond comments, a version or unparsable line whole, an absent file holding neither."""
    try:
        text = catalogs.read_text(encoding="utf-8")
    except FileNotFoundError:
        return (), frozenset[tuple[str, ...]]()
    rows: tuple[_Catalog, ...] = tuple(_parse_catalog(catalogs).values())  # ty: ignore[unsound-assignment]
    lines = map(str.strip, text.splitlines())
    return rows, frozenset(tuple(line.split(":", 2)[:2]) for line in lines if line and not line.startswith("#"))


def published(identity: Identity, folder: Path, stem: str, entries: tuple[tuple[bpy.types.ID, AssetMark], ...], *, kept: bool = False) -> Outcome[Path]:
    """Marks blocks, merges the catalogs they add through the host catalog writer, and writes them into the library once their previews render.

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
    limit = Descriptor.of(bpy.types.AssetMetaData, ("catalog_simple_name",), bpy.types.StringProperty).prop.length_max

    def catalogued(read: tuple[tuple[_Catalog, ...], frozenset[tuple[str, ...]]]) -> Outcome[Mapping[str, str]]:
        rows, declared = read
        held = {row.path.as_posix(): row.uuid for row in reversed(rows)}
        added = {path: str(uuid5(NAMESPACE_URL, f"{identity.manifest.id}/{path}")) for path in sorted({mark.catalog for _, mark in entries} - held.keys())}
        simple = {path: name if len(encoded := name.encode()) < limit - 1 else f"...{encoded[4 - limit :].decode(errors='ignore')}" for path in added for name in (path.replace("/", "-"),)}
        merged = [
            *(msgspec.convert({"path": row.path.as_posix(), "uuids": [row.uuid], "simple_name": row.simple_name}, CatalogV1) for row in rows),
            *(msgspec.convert({"path": path, "uuids": [uid], "simple_name": simple[path]}, CatalogV1) for path, uid in added.items()),
        ]
        meta = msgspec.convert({"api_versions": {}, "name": folder.name, "contact": {"name": identity.manifest.name}}, AssetLibraryMeta)

        def staged(stage: Path) -> Outcome[object]:
            return (
                _text_called(partial(write, merged, stage, meta))
                .bind(lambda _: _text_called(partial(_catalog_rows, stage)))
                .map_error(partial(Fault.of, Reason.UNWRITABLE, str(catalogs)))
                .filter_with(lambda found: declared <= found[1], lambda found: Fault.of(Reason.UNREADABLE, str(catalogs), sorted(map(":".join, declared - found[1]))))
            )

        return (committed(catalogs, staged) if added else Ok(catalogs)).map(lambda _: held | added)

    def marked(uids: Mapping[str, str]) -> Outcome[Path]:
        return (
            accumulated(mark.marked(identity, block, uids[mark.catalog]) for block, mark in entries)
            .bind(lambda blocks: identity.reports.queued(_Publishing(target=target, blocks=frozenset(blocks), fresh=fresh)))
            .map(lambda _: target)
        )

    return folder_made(folder).bind(lambda _: _text_called(partial(_catalog_rows, catalogs)).map_error(partial(Fault.of, Reason.UNREADABLE, str(catalogs)))).bind(catalogued).bind(marked)


# --- [IMPORTS]


def _loading(method: "rna_enums.AssetImportMethodItems", *, relative: bool) -> Mapping[str, bool]:
    """Returns the library load keywords of an asset import method, relative paths applying to links."""
    return {"relative": relative, **_LOADS[method]}


def _essential() -> Outcome["rna_enums.AssetImportMethodItems"]:
    """Returns Essentials import method for packing preference."""
    return rooted(bpy.types.Preferences, bpy.context).map(lambda preferences: "APPEND_REUSE" if preferences.experimental.no_data_block_packing else "PACK")


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
            return preferred.map(Ok).default_with(_essential).map(lambda method: _loading(method, relative=True))
        return (
            Option
            .of_optional(library.user_library)
            .map(lambda row: _loading(preferred.default_value(row.import_method), relative=row.use_relative_path))
            .to_result_with(lambda: Fault.of(Reason.ABSENT, asset.full_path, member="user_library"))
        )

    match Option.of_optional(asset.local_id):
        case Option(tag="some", some=block):
            return Option.of_optional(try_downcast(kind, block)).to_result_with(lambda: Fault.of(Reason.ABSENT, asset.full_path, member=asset.name))
        case Option():
            return (
                Option
                .of_optional(asset.owner_asset_library)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, asset.full_path, member="owner_asset_library"))
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
    folder: str = bpy.utils.system_resource("DATAFILES", path="assets")  # ty: ignore[missing-attribute, unsound-assignment]
    root = Path(folder)
    found = (
        _blends(root)
        .map_error(partial(Fault.of, Reason.UNREADABLE, str(root)))
        .bind(
            lambda paths: accumulated(
                sourced(bpy.data, path, lambda source: (library_listing(source, kind, path), {}), assets_only=True).bind(lambda answer: answer[0]).map(lambda listing: listing[1]) for path in paths
            ).bind(
                lambda listings: (
                    Block.of_seq(zip(paths, listings)).filter(lambda pair: name in pair[1]).map(lambda pair: pair[0]).try_head().to_result_with(lambda: Fault.of(Reason.ABSENT, str(root), member=name))
                )
            )
        )
    )
    return paired(found, _essential()).bind(lambda pair: blocks_loaded(pair[0], kind, (name,), **_loading(pair[1], relative=True))).map(lambda blocks: blocks[0])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AssetMark", "UserLibrary", "bundled", "imported", "published"]
