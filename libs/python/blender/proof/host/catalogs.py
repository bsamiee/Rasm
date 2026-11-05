"""Publishes catalogs into a preferences asset library and re-saves them through an asset browser, whose rows Blender's native loader alone can keep."""

from pathlib import Path
from tempfile import mkdtemp

import bpy

from blender.bpy.extension.identity import Identity, Manifest
from blender.bpy.extension.reports import Reports
from blender.bpy.files.assets import AssetMark, published

# --- [CONSTANTS] ------------------------------------------------------------------------

_LIBRARY = "Catalog Proof"
_CATALOGS = "blender_assets.cats.txt"
_MANIFEST = Manifest(id="catalog_proof", name="Catalog Proof", version="0")
_MARKS = ("Probe/First", "Probe/Second")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _library() -> Path:
    """Returns the folder of the library row `opened` added."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    row: bpy.types.PreferencesAssetLibrary = preferences.filepaths.asset_libraries[_LIBRARY]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return Path(row.path)


def _previewed(catalog: str) -> bpy.types.Material:
    """Returns a new material holding a preview, so no preview job holds its write."""
    material = bpy.data.materials.new(catalog)
    material.preview_ensure()
    return material


def opened() -> dict[str, bool]:
    """Saves the document, whose path the native catalog save needs, publishes one material per catalog into a new library folder, and turns the case area to an asset browser over it."""
    folder = Path(mkdtemp(prefix="catalogs-"))
    bpy.ops.wm.save_as_mainfile(filepath=str(folder.with_suffix(".blend")))
    with Reports(_MANIFEST.id, folder, extension=_MANIFEST.id) as reports:
        identity = Identity(package=_MANIFEST.id, manifest=_MANIFEST, folder=folder, reports=reports)
        outcomes = {catalog: published(identity, folder, catalog.replace("/", "_"), ((_previewed(catalog), AssetMark(key="probe", catalog=catalog)),), kept=True).is_ok() for catalog in _MARKS}
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    preferences.filepaths.asset_libraries.new(name=_LIBRARY, directory=str(folder))
    area: bpy.types.Area = bpy.context.area  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    area.ui_type = "ASSETS"
    return outcomes


def resaved() -> bool:
    """Points the browser at the library, then once the native library loaded adds a catalog, saves, and selects the last published catalog, answering whether the save ran."""
    space: bpy.types.SpaceFileBrowser = bpy.context.space_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    params: bpy.types.FileAssetSelectParams | None = space.params  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    if params is None:
        return False
    if str(params.asset_library_reference) != _LIBRARY:
        params.asset_library_reference = _LIBRARY  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return False
    if not bpy.ops.asset.catalog_new.poll():  # type: ignore[call-arg]  # ty: ignore[missing-argument]
        return False
    bpy.ops.asset.catalog_new(parent_path="")
    bpy.ops.asset.catalogs_save()
    params.catalog_id = bpy.data.materials[_MARKS[-1]].asset_data.catalog_id  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return True


def saved() -> dict[str, list[str]]:
    """Returns the published rows of the re-saved file, and the rows Blender added beside them, which only a native write puts there."""
    lines = (_library() / _CATALOGS).read_text(encoding="utf-8").splitlines()
    rows = [line.split(":") for line in lines if line and not line.startswith(("#", "VERSION "))]
    return {"published": sorted(":".join(row) for row in rows if row[1] in _MARKS), "native": [row[1] for row in rows if row[1] not in _MARKS]}


def shot() -> str:
    """Saves an image of the asset browser beside the library, its catalog tree drawn from what the native loader holds."""
    image = _library().with_suffix(".png")
    bpy.ops.screen.screenshot_area(filepath=str(image))
    return str(image)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["opened", "resaved", "saved", "shot"]
