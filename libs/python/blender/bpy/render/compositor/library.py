"""Active compositor effect group saved into user asset libraries."""

from functools import partial
from pathlib import Path

import bpy
from expression import Nothing, Option, Result, Some

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Call, Done, Operator
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.mirrors import held_choices
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.faults import BlenderReason
from blender.bpy.files.assets import AssetMark, published
from blender.records.faults import Fault, Faults, Outcome, paired

# --- [CHOICES] --------------------------------------------------------------------------


def libraries(_owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
    """User asset library items of the context preferences."""
    return held_choices(
        tuple(
            (row.name, row.name, row.path)
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
    group = rooted(bpy.types.Scene, call.context).bind(
        lambda scene: (
            Option
            .of_optional(scene.compositor_effects.active)
            .bind(lambda entry: Option.of_optional(entry.node_group))
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.compositor_effects), member="active"))
        )
    )
    folder = rooted(bpy.types.Preferences, call.context).bind(
        lambda preferences: (
            Option
            .of_optional(preferences.filepaths.asset_libraries.get(chosen.library))
            .map(lambda row: Path(row.path))
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(preferences.filepaths.asset_libraries), member=chosen.library))
        )
    )

    def stored(pair: tuple[bpy.types.NodeTree, Path]) -> Outcome[Done]:
        group, folder = pair
        return (
            written(group, ("allow_usage_in_scene_compositor_effect",), True)
            .bind(lambda _: published(call.identity, folder, bpy.path.clean_name(group.name), ((group, AssetMark(key=save_effect.key, catalog=chosen.catalog)),), kept=True))
            .map(lambda path: Done(notes=(("INFO", f"Saved {group.name} to {path}"),)))
        )

    return Result[tuple[bpy.types.NodeTree, Path], Faults].bind(paired(group, folder), stored)


# --- [COMPOSITION] ----------------------------------------------------------------------

save_effect = Operator(form=Saving, run=save_effect_asset, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), undo=Nothing)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Saving", "libraries", "save_effect", "save_effect_asset"]
