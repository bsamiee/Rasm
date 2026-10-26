"""Active compositor effect group saved into user asset libraries."""

import functools
from pathlib import Path

import bpy
from expression import Option, Result, Some

from blender.bpy.commands.operators import Call, Done, Operator, Properties
from blender.bpy.data.rna.descriptors import Struct, written
from blender.bpy.faults import BlenderReason
from blender.bpy.files.assets import AssetMark, published
from blender.records.faults import Fault, Faults, Outcome, paired

# --- [CHOICES] --------------------------------------------------------------------------


@functools.cache
def _libraries(held: tuple[tuple[str, str], ...]) -> tuple[tuple[str, str, str], ...]:
    """Enum rows of user asset libraries, cached so the strings Blender reads stay referenced."""
    return tuple((name, name, path) for name, path in held)


def libraries(_owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[str, str, str], ...]:
    """User asset library rows of the context preferences."""
    return _libraries(
        tuple(
            (row.name, row.path)
            for preferences in Option.of_optional(context).bind(lambda held: Option.of_optional(held.preferences)).to_list()
            for row in preferences.filepaths.asset_libraries
            if isinstance(row, bpy.types.PreferencesAssetLibrary)
        )
    )


# --- [MODELS] ---------------------------------------------------------------------------


class Saving(bpy.types.PropertyGroup):
    """User asset library and catalog for active effect group."""

    library: bpy.props.EnumProperty(name="Library", items=libraries)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    catalog: bpy.props.StringProperty(name="Catalog", default="Effects")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------


def save_effect_asset(call: Call[Saving]) -> Outcome[Done]:
    """Save active effect to user asset library."""
    chosen = call.properties
    group = call.root(bpy.types.Scene).bind(
        lambda scene: Option.of_optional(next((entry.node_group for entry in scene.compositor_effects if entry.is_active), None)).to_result_with(
            lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene.compositor_effects), member="active"),)
        )
    )
    folder = call.root(bpy.types.Preferences).bind(
        lambda preferences: Option.of_optional(
            next((Path(row.path) for row in preferences.filepaths.asset_libraries if isinstance(row, bpy.types.PreferencesAssetLibrary) and row.name == chosen.library), None)
        ).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(preferences.filepaths.asset_libraries), member=chosen.library),))
    )

    def stored(pair: tuple[bpy.types.NodeTree, Path]) -> Outcome[Done]:
        group, folder = pair
        return (
            written(group, "allow_usage_in_scene_compositor_effect", True)
            .bind(lambda _: published(call.identity, folder, bpy.path.clean_name(group.name), ((group, AssetMark(key=save_effect.key, catalog=chosen.catalog)),), kept=True))
            .map(lambda path: Done(notes=(("INFO", f"Saved {group.name} to {path}"),)))
        )

    return Result[tuple[bpy.types.NodeTree, Path], Faults].bind(paired(group, folder), stored)


# --- [COMPOSITION] ----------------------------------------------------------------------

save_effect = Operator(form=Saving, run=save_effect_asset, invoke=Some(Properties()), undo=False)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Saving", "libraries", "save_effect", "save_effect_asset"]
