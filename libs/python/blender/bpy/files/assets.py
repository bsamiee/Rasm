"""Native asset metadata, catalogs, extension libraries, imports, and publication."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from operator import itemgetter
from pathlib import Path
from typing import Annotated, Literal, TYPE_CHECKING
from uuid import NAMESPACE_URL, UUID, uuid5

from _bpy_internal.assets.remote_library.listing_asset_catalogs import parse_catalogs
import bpy
from expression import Error, Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult, BlenderWriteFault
from blender.bpy.files.blends import folder_made
from blender.bpy.files.libraries import blocks_loaded, library_written, LibraryLink, Linked, listed, LocalCopy
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type CatalogPath = Annotated[str, msgspec.Meta(pattern=r"\A[^:/\\\r\n]+(?:/[^:/\\\r\n]+)*\Z")]
type LinkOrAppend = Literal["LINK", "APPEND"]

# --- [MODELS] ---------------------------------------------------------------------------


class AssetMark(msgspec.Struct, frozen=True, kw_only=True):
    """Metadata, catalog, import preference, and extension identity of an asset."""

    key: Key
    catalog: CatalogPath
    description: str = ""
    author: str = ""
    copyright: str = ""
    license: str = ""
    tags: frozenset[str] = frozenset()
    method: Option[LinkOrAppend] = Nothing

    def marked[T: bpy.types.ID](self, identity: Identity, block: T, uid: UUID) -> BlenderResult[T]:
        """Marks a data-block and replaces its asset metadata.

        Args:
            identity: Extension whose manifest id owns the metadata mark.
            block: Data-block receiving the metadata.
            uid: Catalog UUID read or created by publication.

        Returns:
            The block, or an absent fault when Blender cannot mark it and an edit fault for refused metadata.
        """

        def applied(data: bpy.types.AssetMetaData) -> T:
            data.description = self.description
            data.author = self.author
            data.copyright = self.copyright
            data.license = self.license
            data.catalog_id = str(uid)
            data.use_preferred_import_method = self.method.is_some()
            for method in self.method.to_list():
                data.preferred_import_method = method
            names = frozenset(tag.name for tag in data.tags)
            for tag in tuple(data.tags):
                if tag.name not in self.tags:
                    data.tags.remove(tag)
            for name in sorted(self.tags - names):
                data.tags.new(name)
            data[identity.manifest.id] = self.key
            return block

        try:
            block.asset_mark()
            return Option.of_optional(block.asset_data).map(applied).to_result_with(lambda: (BlenderAbsentFault(owner=repr(block), key="asset_data"),))
        except AttributeError as error:
            return Error((BlenderEditFault(request=repr(block), error=error),))


class AssetLibrary(msgspec.Struct, frozen=True, kw_only=True):
    """Extension library held as a preference row while the extension is enabled."""

    title: str
    folder: Key
    method: Option[LinkOrAppend] = Nothing

    def located(self, identity: Identity) -> Path:
        """Returns the library folder beneath the extension's user folder."""
        return identity.folder / "assets" / self.folder

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Returns the preference row acquisition."""
        return Ok(((Band.RESOURCES, self.acquired(identity)),))

    @contextmanager
    def acquired(self, identity: Identity) -> Generator[BlenderResult[None]]:
        """Places the library row, or takes the row an earlier session's preference save kept, and removes it on release.

        Args:
            identity: Extension owning the library folder.

        Yields:
            Success while the row is acquired, the folder's write fault, or the absent row Blender refused to add.
        """
        with ExitStack() as lifetime:

            def held(folder: Path, rows: bpy.types.AssetLibraryCollection) -> BlenderResult[bpy.types.UserAssetLibrary]:
                made: BlenderResult[bpy.types.UserAssetLibrary] = Option.of_optional(
                    next((row for row in rows if isinstance(row, bpy.types.PreferencesAssetLibrary) and Path(row.path) == folder), None) or rows.new(name=self.title, directory=str(folder))
                ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(rows), key=self.title),))
                for row in made.to_option().to_list():
                    lifetime.callback(rows.remove, row)
                return made

            def configured(row: bpy.types.UserAssetLibrary) -> None:
                row.name = self.title
                for method in self.method.to_list():
                    row.import_method = method

            yield folder_made(self.located(identity)).bind(lambda folder: held(folder, bpy.context.preferences.filepaths.asset_libraries)).map(configured)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PUBLICATION]


def published(identity: Identity, folder: Path, stem: str, entries: tuple[tuple[bpy.types.ID, AssetMark], ...], *, kept: bool = False) -> BlenderResult[Path]:
    """Writes marked data-blocks into a library with their catalogs.

    Args:
        identity: Extension whose manifest id owns each mark and seeds catalog UUIDs.
        folder: Library folder receiving the catalog and blend files.
        stem: Blend filename without its suffix.
        entries: Data-blocks paired with their asset metadata.
        kept: Whether newly added asset marks survive publication.

    Returns:
        The written file, or catalog read faults, write faults, and every refused asset mark.
    """
    target = folder / "blender_assets.cats.txt"
    fresh = () if kept else tuple(block for block, _ in entries if block.asset_data is None)

    def unread(error: msgspec.ValidationError | ValueError) -> BlenderFaults:
        return (BlenderReadFault(request=target, error=error),)

    def ensured(_: Path) -> BlenderResult[dict[str, UUID]]:
        try:
            return accumulated(
                paired(
                    catch(lambda: msgspec.convert(catalog.path, CatalogPath), exception=msgspec.ValidationError)().map_error(unread),
                    catch(lambda: UUID(catalog.uuids[0]), exception=ValueError)().map_error(unread),
                )
                for catalog in parse_catalogs(folder)  # type: ignore[attr-defined, func-returns-value]  # ty: ignore[not-iterable]
            ).map(dict)
        except (OSError, UnicodeError) as error:
            return Error((BlenderReadFault(request=target, error=error),))

    def appended(held: dict[str, UUID]) -> BlenderResult[dict[str, UUID]]:
        added = {path: uuid5(NAMESPACE_URL, f"{identity.manifest.id}/{path}") for path in sorted({mark.catalog for _, mark in entries} - held.keys())}

        def written() -> dict[str, UUID]:
            with target.open("a", encoding="utf-8") as file:
                file.write(("VERSION 1\n\n" if file.tell() == 0 else "\n") + "".join(f"{uid}:{path}:{path.replace('/', '-')}\n" for path, uid in added.items()))
            return held | added

        return catch(written, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=target, error=error),)) if added else Ok(held)

    def marked(catalogs: dict[str, UUID]) -> BlenderResult[Path]:
        try:
            return accumulated(mark.marked(identity, block, catalogs[mark.catalog]) for block, mark in entries).bind(lambda blocks: library_written(folder / f"{stem}.blend", frozenset(blocks)))
        finally:
            for block in fresh:
                block.asset_clear()

    return folder_made(folder).bind(ensured).bind(appended).bind(marked)


def linking(method: "rna_enums.AssetImportMethodItems", *, relative: bool) -> LibraryLink:
    """Returns the native library operation for an asset import method."""
    match method:
        case "LINK":
            return Linked(relative=relative)
        case "PACK":
            return Linked(pack=True, relative=relative)
        case "APPEND":
            return LocalCopy()
        case "APPEND_REUSE":
            return LocalCopy(reuse_local_id=True)


def essential() -> "rna_enums.AssetImportMethodItems":
    """Returns Blender's Essentials import method under its packing preference."""
    return "APPEND_REUSE" if bpy.context.preferences.experimental.no_data_block_packing else "PACK"  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def imported[T: bpy.types.ID](asset: bpy.types.AssetRepresentation, kind: type[T], library: Option[bpy.types.UserAssetLibrary] = Nothing) -> BlenderResult[T]:
    """Returns an asset's local data or imports it by Blender's asset method.

    Args:
        asset: Representation supplied by an asset shelf, browser, or menu.
        kind: Native data-block type expected by the consumer.
        library: Selected native preferences or project library for an external custom asset.

    Returns:
        The local or imported block, or an absent fault and the native library load faults.
    """
    match Option.of_optional(asset.local_id):
        case Option(tag="some", some=block):
            return Ok(block) if isinstance(block, kind) else Error((BlenderAbsentFault(owner=asset.full_path, key=asset.name),))
        case Option():
            path = Path(asset.full_library_path)
            preferred = Option.of_optional(asset.metadata).filter(lambda data: data.use_preferred_import_method).map(lambda data: data.preferred_import_method)
            link: BlenderResult[LibraryLink] = (
                Ok(linking(preferred.default_with(essential), relative=True))
                if Option.of_optional(asset.owner_asset_library).filter(lambda owner: owner.type == "ESSENTIALS").is_some()
                else library.map(lambda row: linking(preferred.default_value(row.import_method), relative=row.use_relative_path)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=asset.full_path, key="owner_asset_library"),)
                )
            )
            return link.bind(lambda method: blocks_loaded(path, kind, (asset.name,), method)).map(itemgetter(0))


def bundled[T: bpy.types.ID](kind: type[T], name: str) -> BlenderResult[T]:
    """Imports a named asset from Blender's bundled libraries.

    Args:
        kind: Native data-block type to list and import.
        name: Asset name contained in a bundled blend file.

    Returns:
        The imported block, or every listing read fault, an absent fault, and native load faults.
    """
    root = Path(bpy.utils.system_resource("DATAFILES", path="assets"))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    try:
        paths = tuple(sorted(root.rglob("*.blend")))
    except OSError as error:
        return Error((BlenderReadFault(request=root, error=error),))
    return (
        accumulated(listed(path, kind, assets_only=True) for path in paths)
        .bind(lambda listings: Option.of_optional(next((path for path, names in zip(paths, listings) if name in names), None)).to_result_with(lambda: (BlenderAbsentFault(owner=str(root), key=name),)))
        .bind(lambda path: blocks_loaded(path, kind, (name,), linking(essential(), relative=True)))
        .map(itemgetter(0))
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["AssetLibrary", "AssetMark", "CatalogPath", "LinkOrAppend", "bundled", "essential", "imported", "linking", "published"]
