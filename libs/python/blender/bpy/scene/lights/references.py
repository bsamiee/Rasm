"""Reference images a matched light rig reads: catalog picks, seeded draws, gels, and the rendered recipe."""

from collections.abc import Callable
from functools import partial

import bpy
from expression import Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.ids.images import Generated, Tag
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.scene.lights.placement import REFERENCE, resolved, RigSetup, RigState, SETUP, states
from blender.bpy.settings.attachments import Stored
from blender.records.faults import Outcome, paired
from blender.records.identity import content_hash
from blender.rendering.lighting.references import Gel, RECIPES, ReferenceDistribution, ReferenceImage, REFERENCES

# --- [MODELS] ---------------------------------------------------------------------------


class ReferencePick(bpy.types.PropertyGroup):
    """Reference recipe name from catalog."""

    name: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Reference", items=choices((name, name, bpy.path.display_name(family.name.lower())) for family, recipes in REFERENCES.items() for name in recipes)
    )


class ReferenceDraw(bpy.types.PropertyGroup):
    """Reference seed and color distribution."""

    distribution: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Distribution", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in ReferenceDistribution), default=ReferenceDistribution.BALANCED.name
    )
    seed: bpy.props.IntProperty(name="Seed", min=0, max=2**24)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class GelPick(bpy.types.PropertyGroup):
    """Gel name for reference tint."""

    name: bpy.props.EnumProperty(name="Gel", items=choices((gel.name, bpy.path.display_name(gel.name.lower()), "") for gel in Gel))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _referenced[T: bpy.types.PropertyGroup](call: Call[T], recipe: Callable[[RigState], ReferenceImage]) -> Outcome[Done]:
    """Renders recipe into the resolved rig's owned scene-linear reference image, removing unused reference images."""
    config = PyOpenColorIO.GetCurrentConfig()

    def imaged(holder: bpy.types.Object, setup: RigSetup, store: Stored[RigState], state: RigState) -> Outcome[None]:
        chosen = recipe(state)
        return (
            Generated(
                ownership=REFERENCE,
                content=content_hash(RigState, chosen, config.getCacheID().encode(), arrays=(np.asarray(setup.size, dtype=np.int64),)),
                name=f"{holder.name} Reference",
                plane=lambda: chosen.plane(setup.size).map(Some),
                tag=Tag(space=Some(config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()), view_as_render=Some(True)),
            )
            .made(call.identity, of=Some(holder))
            .bind(lambda image: paired(store.write(holder, msgspec.structs.replace(state, recipe=chosen)), written(setup, ("reference",), image.to_optional())).map(lambda _: None))
        )

    def stored(holder: bpy.types.Object) -> Outcome[int]:
        return paired(SETUP.read(call.identity, holder), states(call.identity, holder)).bind(lambda pair: imaged(holder, pair[0], *pair[1])).bind(lambda _: REFERENCE.removed(call.identity))

    return rooted(bpy.types.ViewLayer, call.context).bind(lambda layer: resolved(layer, call.identity)).bind(stored).map(lambda _: Done())


def rig_reference_pick(call: Call[ReferencePick]) -> Outcome[Done]:
    """Renders a catalog reference as the matched rig image."""
    return _referenced(call, lambda _: RECIPES[call.properties.name])


def rig_reference_draw(call: Call[ReferenceDraw]) -> Outcome[Done]:
    """Draws a seeded reference as the matched rig image."""
    return _referenced(call, lambda _: ReferenceImage.drawn(ReferenceDistribution[call.properties.distribution], call.properties.seed))


def rig_reference_gel(call: Call[GelPick]) -> Outcome[Done]:
    """Tints the reference with a catalog gel."""
    return _referenced(call, lambda state: msgspec.structs.replace(state.recipe, tint=Gel[call.properties.name].value))


def rig_reference_apply(call: Call[Empty]) -> Outcome[Done]:
    """Renders the edited reference as the matched rig image."""
    return _referenced(call, lambda state: state.recipe)


# --- [COMPOSITION] ----------------------------------------------------------------------

rig_reference_pick_command: Operator[ReferencePick] = Operator(
    form=ReferencePick, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"), run=rig_reference_pick
)
rig_reference_draw_command: Operator[ReferenceDraw] = Operator(form=ReferenceDraw, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_popup))), run=rig_reference_draw)
rig_reference_gel_command: Operator[GelPick] = Operator(form=GelPick, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"), run=rig_reference_gel)
rig_reference_apply_command: Operator[Empty] = Operator(form=Empty, run=rig_reference_apply)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GelPick", "ReferenceDraw", "ReferencePick", "rig_reference_apply_command", "rig_reference_draw_command", "rig_reference_gel_command", "rig_reference_pick_command"]
