"""Asset metadata, catalogs, extension libraries, imports, and publication."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from pathlib import Path
from typing import Annotated, Literal, TYPE_CHECKING
from uuid import NAMESPACE_URL, UUID, uuid5

from _bpy_internal.assets.remote_library.listing_asset_catalogs import parse_catalogs
import bpy
from expression import Error, Nothing, Ok, Option, Result
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderReason
from blender.bpy.files.blends import folder_made
from blender.bpy.files.libraries import blocks_loaded, library_written, LibraryLink, Linked, listed, LocalCopy
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type CatalogPath = Annotated[str, msgspec.Meta(pattern=r"\A[^:/\\\r\n]+(?:/[^:/\\\r\n]+)*\Z")]
type LinkOrAppend = Literal["LINK", "APPEND"]

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
    method: Option[LinkOrAppend] = Nothing

    def marked[T: bpy.types.ID](self, identity: Identity, block: T, uid: UUID) -> Outcome[T]:
        """Marks block as asset, replacing its metadata.

        Args:
            identity: Extension with manifest ID keying mark.
            block: ID to mark.
            uid: Catalog UUID publication read or created.

        Returns:
            Marked block, edit fault for unmarkable block, faults per refused member write.
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
                    "description": self.description,
                    "author": self.author,
                    "copyright": self.copyright,
                    "license": self.license,
                    "catalog_id": str(uid),
                    "use_preferred_import_method": self.method.is_some(),
                    **self.method.map(lambda method: {"preferred_import_method": method}).default_value({}),
                },
            ).map(lambda _: block)

        block.asset_mark()
        return Option.of_optional(block.asset_data).map(applied).default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject=repr(block), member="asset_data"),)))


class AssetLibrary(msgspec.Struct, frozen=True, kw_only=True):
    """Asset library preference row held while extension is enabled."""

    title: str
    folder: Key
    method: Option[LinkOrAppend] = Nothing

    def located(self, identity: Identity) -> Path:
        """Returns library folder under extension user folder."""
        return identity.folder / "assets" / self.folder

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns row acquisition."""
        return Ok(((Band.RESOURCES, self.acquired(identity)),))

    @contextmanager
    def acquired(self, identity: Identity) -> Generator[Outcome[None]]:
        """Holds library row, reusing saved one, until release.

        Args:
            identity: Extension owning library folder.

        Yields:
            Success while row is held, absent fault when Blender adds none.
        """
        with ExitStack() as lifetime:

            def held(folder: Path, rows: bpy.types.AssetLibraryCollection) -> Outcome[bpy.types.UserAssetLibrary]:
                made = (
                    Option
                    .of_optional(next((row for row in rows if isinstance(row, bpy.types.PreferencesAssetLibrary) and Path(row.path) == folder), None))
                    .map(Ok)
                    .default_with(
                        lambda: catch(rows.new, exception=RuntimeError)(name=self.title, directory=str(folder)).map_error(
                            lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(rows), cause=error),)
                        )
                    )
                    .bind(lambda row: Option.of_optional(row).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(rows), member=self.title),)))
                )
                for row in made.to_option().to_list():
                    lifetime.callback(rows.remove, row)
                return made

            yield (
                folder_made(self.located(identity))
                .bind(lambda folder: held(folder, bpy.context.preferences.filepaths.asset_libraries))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                .bind(lambda row: assigned(row, {"name": self.title, **self.method.map(lambda method: {"import_method": method}).default_value({})}))
            )


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PUBLICATION]


def published(identity: Identity, folder: Path, stem: str, entries: tuple[tuple[bpy.types.ID, AssetMark], ...], *, kept: bool = False) -> Outcome[Path]:
    """Writes marked blocks and catalogs into library.

    Args:
        identity: Extension with manifest ID keying each mark and seeding catalog UUIDs.
        folder: Library folder for catalog and blend files.
        stem: Blend file name without suffix.
        entries: IDs with asset metadata.
        kept: Whether added asset marks stay.

    Returns:
        Written blend file, accumulating catalog and mark faults.
    """
    target = folder / "blender_assets.cats.txt"
    fresh = () if kept else tuple(block for block, _ in entries if block.asset_data is None)

    def unread(error: msgspec.ValidationError | ValueError) -> Faults:
        return (Fault(reason=RecordReason.UNREADABLE, subject=str(target), cause=error),)

    def ensured(_: Path) -> Outcome[dict[str, UUID]]:
        try:
            return accumulated(
                paired(
                    catch(lambda: msgspec.convert(catalog.path, CatalogPath), exception=msgspec.ValidationError)().map_error(unread),
                    catch(lambda: UUID(catalog.uuids[0]), exception=ValueError)().map_error(unread),
                )
                for catalog in parse_catalogs(folder)  # type: ignore[attr-defined, func-returns-value]  # ty: ignore[not-iterable]
            ).map(dict)
        except (OSError, UnicodeError) as error:
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=str(target), cause=error),))

    def appended(held: dict[str, UUID]) -> Outcome[dict[str, UUID]]:
        added = {path: uuid5(NAMESPACE_URL, f"{identity.manifest.id}/{path}") for path in sorted({mark.catalog for _, mark in entries} - held.keys())}

        def written() -> dict[str, UUID]:
            with target.open("a", encoding="utf-8") as file:
                file.write(("VERSION 1\n\n" if file.tell() == 0 else "\n") + "".join(f"{uid}:{path}:{path.replace('/', '-')}\n" for path, uid in added.items()))
            return held | added

        return catch(written, exception=OSError)().map_error(lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=str(target), cause=error),)) if added else Ok(held)

    def marked(catalogs: dict[str, UUID]) -> Outcome[Path]:
        try:
            return accumulated(mark.marked(identity, block, catalogs[mark.catalog]) for block, mark in entries).bind(lambda blocks: library_written(folder / f"{stem}.blend", frozenset(blocks)))
        finally:
            for block in fresh:
                block.asset_clear()

    return folder_made(folder).bind(ensured).bind(appended).bind(marked)


def linking(method: "rna_enums.AssetImportMethodItems", *, relative: bool) -> LibraryLink:
    """Returns library link mode for asset import method."""
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
    """Returns Essentials import method for packing preference."""
    return "APPEND_REUSE" if bpy.context.preferences.experimental.no_data_block_packing else "PACK"  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def imported[T: bpy.types.ID](asset: bpy.types.AssetRepresentation, kind: type[T], library: Option[bpy.types.UserAssetLibrary] = Nothing) -> Outcome[T]:
    """Returns local asset data, else imports it by import method.

    Args:
        asset: Representation from asset shelf, browser, or menu.
        kind: Expected ID type.
        library: Selected preferences or project library of external custom asset.

    Returns:
        Local or imported block, absent fault for another kind or missing library.
    """
    match Option.of_optional(asset.local_id):
        case Option(tag="some", some=block):
            return Ok(block) if isinstance(block, kind) else Error((Fault(reason=BlenderReason.ABSENT, subject=asset.full_path, member=asset.name),))
        case Option():
            path = Path(asset.full_library_path)
            preferred = Option.of_optional(asset.metadata).filter(lambda data: data.use_preferred_import_method).map(lambda data: data.preferred_import_method)
            link: Outcome[LibraryLink] = (
                Ok(linking(preferred.default_with(essential), relative=True))
                if Option.of_optional(asset.owner_asset_library).filter(lambda owner: owner.type == "ESSENTIALS").is_some()
                else library.map(lambda row: linking(preferred.default_value(row.import_method), relative=row.use_relative_path)).to_result_with(
                    lambda: (Fault(reason=BlenderReason.ABSENT, subject=asset.full_path, member="owner_asset_library"),)
                )
            )
            return link.bind(lambda method: blocks_loaded(path, kind, (asset.name,), method)).map(lambda blocks: blocks[0])


def bundled[T: bpy.types.ID](kind: type[T], name: str) -> Outcome[T]:
    """Imports named asset from bundled asset libraries.

    Args:
        kind: ID type to list and import.
        name: Asset name in bundled blend file.

    Returns:
        Imported block, absent fault for missing name.
    """
    root = Path(bpy.utils.system_resource("DATAFILES", path="assets"))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return (
        Result[tuple[Path, ...], Faults]
        .bind(
            catch(lambda: tuple(sorted(root.rglob("*.blend"))), exception=OSError)().map_error(lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=str(root), cause=error),)),
            lambda paths: accumulated(listed(path, kind, assets_only=True) for path in paths).bind(
                lambda listings: Option.of_optional(next((path for path, names in zip(paths, listings) if name in names), None)).to_result_with(
                    lambda: (Fault(reason=BlenderReason.ABSENT, subject=str(root), member=name),)
                )
            ),
        )
        .bind(lambda path: blocks_loaded(path, kind, (name,), linking(essential(), relative=True)))
        .map(lambda blocks: blocks[0])
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["AssetLibrary", "AssetMark", "CatalogPath", "LinkOrAppend", "bundled", "essential", "imported", "linking", "published"]
