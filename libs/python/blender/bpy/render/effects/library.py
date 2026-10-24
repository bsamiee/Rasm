"""Active effect group saved into user asset libraries."""

from pathlib import Path
from typing import Annotated

import bpy
from expression import Option, Result, Some
import msgspec

from blender.bpy.commands.operators import Call, Done, Operator, Properties
from blender.bpy.data.rna.descriptors import written
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.files.assets import AssetMark, CatalogPath, published
from blender.bpy.settings.properties import Choices
from blender.imaging.faults import paired

# --- [CHOICES] --------------------------------------------------------------------------

libraries = Choices(
    lambda _identity, _owner, context: (
        (row.name, row.name, row.path)
        for preferences in Option.of_optional(context).bind(lambda held: Option.of_optional(held.preferences)).to_list()
        for row in preferences.filepaths.asset_libraries
        if isinstance(row, bpy.types.PreferencesAssetLibrary)
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class Saving(msgspec.Struct, frozen=True, kw_only=True):
    """User asset library and catalog for active effect group."""

    library: Annotated[str, msgspec.Meta(title="Library"), libraries] = ""
    catalog: Annotated[CatalogPath, msgspec.Meta(title="Catalog")] = "Effects"


# --- [OPERATIONS] -----------------------------------------------------------------------


def saved(call: Call[Saving]) -> BlenderResult[Done]:
    """Saves active effect group into chosen user asset library."""
    record = call.record
    group = call.root(bpy.types.Scene).bind(
        lambda scene: Option.of_optional(next((entry.node_group for entry in scene.compositor_effects if entry.is_active), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key="active"),)
        )
    )
    folder = call.root(bpy.types.Preferences).bind(
        lambda preferences: Option.of_optional(
            next((Path(row.path) for row in preferences.filepaths.asset_libraries if isinstance(row, bpy.types.PreferencesAssetLibrary) and row.name == record.library), None)
        ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(preferences.filepaths.asset_libraries), key=record.library),))
    )

    def stored(pair: tuple[bpy.types.NodeTree, Path]) -> BlenderResult[Done]:
        group, folder = pair
        return (
            written(group, "allow_usage_in_scene_compositor_effect", True)
            .bind(lambda _: published(call.identity, folder, bpy.path.clean_name(group.name), ((group, AssetMark(key=save_effect.key, catalog=record.catalog)),), kept=True))
            .map(lambda path: Done(notes=(("INFO", f"Saved {group.name} to {path}"),)))
        )

    return Result[tuple[bpy.types.NodeTree, Path], BlenderFaults].bind(paired(group, folder), stored)


# --- [COMPOSITION] ----------------------------------------------------------------------

save_effect = Operator(key="effect_save_asset", title="Save Effect Asset", description="Save active effect to user asset library", form=Saving, run=saved, invoke=Some(Properties()), undo=False)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Saving", "libraries", "save_effect", "saved"]
