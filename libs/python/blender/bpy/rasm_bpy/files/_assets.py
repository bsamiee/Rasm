"""Asset metadata and previews, catalogs over the host catalog files, extension asset libraries, imports by import method, and publication."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
from pathlib import Path, PurePosixPath
from types import MappingProxyType
from typing import Annotated, Final, Protocol, Self, TYPE_CHECKING
from uuid import NAMESPACE_URL, uuid5

from _bpy_internal.assets.remote_library.blender_asset_library_openapi import AssetLibraryMeta, CatalogV1, Contact  # ruff: ignore[import-private-name]
from _bpy_internal.assets.remote_library.listing_asset_catalogs import _parse_catalog, write  # type: ignore[attr-defined]  # ty: ignore[unresolved-import]  # ruff: ignore[import-private-name]
import bpy
from expression import Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _Tier, Acquisitions
from rasm_bpy.data._ids.ownership import _tracked
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.files._libraries import _blocks_loaded, _library_written, _listed
from rasm_records import accumulated, committed, Error, Fault, folder_made, Nothing, Ok, Outcome, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _CatalogPath = Annotated[str, msgspec.Meta(pattern=r"\A(?!(?:.*/)?\.(?:/|\Z))[^:/\\\s]+(?:[ /][^:/\\\s]+)*\Z")]
type _ImportMethod = rna_enums.AssetImportMethodItems


class _Catalog(Protocol):
    """Catalog row `_parse_catalog` returns, whose stub lacks its fields."""

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


class _AssetMark(msgspec.Struct, frozen=True, kw_only=True):
    """Asset metadata under extension key."""

    key: Key
    catalog: _CatalogPath
    description: str = ""
    author: str = ""
    copyright: str = ""
    license: str = ""
    tags: frozenset[str] = frozenset()
    method: Option[_ImportMethod] = Nothing

    def __post_init__(self) -> None:
        """Refuses a catalog path `CatalogPath` excludes, which direct construction otherwise leaves unchecked."""
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
            uid: Catalog UUID publication read or created.

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
    method: Option[_ImportMethod] = Nothing

    def located(self, identity: Identity) -> Path:
        """Returns library folder under extension user folder."""
        return identity.folder / "assets" / self.folder

    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Returns row acquisition."""
        return ((_Tier.RESOURCES, self.acquired(identity)),)

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
                    .default_with(lambda: catch(rows.new, exception=RuntimeError)(name=self.title, directory=str(folder), extension_id=stamp).map_error(partial(Fault.of, Reason.REFUSED, repr(rows))))
                    .bind(lambda row: Option.of_optional(try_downcast(bpy.types.PreferencesAssetLibrary, row)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(rows), member=self.title)))
                )
                return _tracked(lifetime, rows.remove, found)

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
                return _library_written(self.target, self.blocks, path_remap="RELATIVE_ALL", fake_user=True).map(lambda _: Nothing)

        return Ok(Some(self)) if bpy.app.is_job_running("RENDER_PREVIEW") else written()


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PUBLICATION]


def _text_faulted[**P, T](operation: Callable[P, T], /) -> Callable[P, Result[T, OSError | UnicodeError]]:
    """Returns the file operation with its file and text encoding failures as its error."""
    return catch(exception=UnicodeError)(catch(operation, exception=OSError))  # ty: ignore[invalid-return-type]


@_text_faulted
def _read(catalogs: Path) -> tuple[tuple[_Catalog, ...], frozenset[tuple[str, ...]]]:
    """Returns the rows the host parser keeps from one catalog file and the UUID and path fields of each line beyond comments, a version or unparsable line whole, an absent file holding neither."""
    try:
        text = catalogs.read_text(encoding="utf-8")
    except FileNotFoundError:
        return (), frozenset[tuple[str, ...]]()
    rows: tuple[_Catalog, ...] = tuple(_parse_catalog(catalogs).values())  # ty: ignore[unsound-assignment]
    lines = map(str.strip, text.splitlines())
    return rows, frozenset(tuple(line.split(":", 2)[:2]) for line in lines if line and not line.startswith("#"))


def _simple_name(path: str) -> str:
    """Returns the simple name Blender derives for a catalog path, a name past the host field keeping its tail on a character boundary."""
    field: bpy.types.StringProperty = bpy.types.AssetMetaData.bl_rna.properties["catalog_simple_name"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    name, elided = path.replace("/", "-"), "..."
    encoded = name.encode()
    return name if len(encoded) < field.length_max - 1 else elided + encoded[len(elided) + 1 - field.length_max :].decode(errors="ignore")


def _published(identity: Identity, folder: Path, stem: str, entries: tuple[tuple[bpy.types.ID, _AssetMark], ...], *, kept: bool = False) -> Outcome[Path]:
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

    def catalogued(read: tuple[tuple[_Catalog, ...], frozenset[tuple[str, ...]]]) -> Outcome[Mapping[str, str]]:
        rows, declared = read
        held = {row.path.as_posix(): row.uuid for row in reversed(rows)}
        added = {path: str(uuid5(NAMESPACE_URL, f"{identity.manifest.id}/{path}")) for path in sorted({mark.catalog for _, mark in entries} - held.keys())}
        merged = [
            *(CatalogV1(path=row.path.as_posix(), uuids=[row.uuid], simple_name=row.simple_name) for row in rows),  # type: ignore[call-arg]  # ty: ignore[unknown-argument]
            *(CatalogV1(path=path, uuids=[uid], simple_name=_simple_name(path)) for path, uid in added.items()),  # type: ignore[call-arg]  # ty: ignore[unknown-argument]
        ]
        meta = AssetLibraryMeta(api_versions={}, name=folder.name, contact=Contact(name=identity.manifest.name))  # type: ignore[call-arg]  # ty: ignore[unknown-argument]

        def staged(stage: Path) -> Outcome[object]:
            return (
                _text_faulted(write)(merged, stage, meta)
                .bind(lambda _: _read(stage))
                .map_error(partial(Fault.of, Reason.UNWRITABLE, str(catalogs)))
                .filter_with(lambda written: declared <= written[1], lambda written: Fault.of(Reason.UNREADABLE, str(catalogs), sorted(map(":".join, declared - written[1]))))
            )

        return (committed(catalogs, staged) if added else Ok(catalogs)).map(lambda _: held | added)

    def marked(uids: Mapping[str, str]) -> Outcome[Path]:
        return (
            accumulated(mark.marked(identity, block, uids[mark.catalog]) for block, mark in entries)
            .bind(lambda blocks: identity.reports.queued(_Publishing(target=target, blocks=frozenset(blocks), fresh=fresh)))
            .map(lambda _: target)
        )

    return folder_made(folder).bind(lambda _: _read(catalogs).map_error(partial(Fault.of, Reason.UNREADABLE, str(catalogs)))).bind(catalogued).bind(marked)


# --- [IMPORTS]


def _loading(method: _ImportMethod, *, relative: bool) -> Mapping[str, bool]:
    """Returns the library load keywords of an asset import method, relative paths applying to links."""
    return {"relative": relative, **_LOADS[method]}


def _essential() -> _ImportMethod:
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
            return Ok(_loading(preferred.default_with(_essential), relative=True))
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
                .bind(lambda load: _blocks_loaded(Path(asset.full_library_path), kind, (asset.name,), **load))
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
    folder: str = bpy.utils.system_resource("DATAFILES", path="assets")  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute, unsound-assignment]
    root = Path(folder)
    return (
        _blends(root)
        .map_error(partial(Fault.of, Reason.UNREADABLE, str(root)))
        .bind(
            lambda paths: accumulated(_listed(path, kind, assets_only=True) for path in paths).bind(
                lambda listings: (
                    Block.of_seq(zip(paths, listings)).filter(lambda pair: name in pair[1]).map(lambda pair: pair[0]).try_head().to_result_with(lambda: Fault.of(Reason.ABSENT, str(root), member=name))
                )
            )
        )
        .bind(lambda path: _blocks_loaded(path, kind, (name,), **_loading(_essential(), relative=True)))
        .map(lambda blocks: blocks[0])
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["UserLibrary", "bundled", "imported"]
