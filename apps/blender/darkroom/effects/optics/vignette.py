"""Frame-edge falloff in scene-linear light: natural chief-ray illumination or a feathered outline pulling toward a color."""

from typing import Annotated

import bpy
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Condition
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, mixed, NodeGroup, switched
from blender.bpy.render.compositor import lenses, masks
from blender.bpy.render.compositor.effects import CameraFrame, CompositorEffect, Conditions, Inspects, Shell
from blender.bpy.scene import cameras
from blender.records.faults import paired
from blender.records.fields import Case, Rgb

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Natural(Case, frozen=True, kw_only=True):
    """Relative illumination as fourth power of chief-ray cosine."""


class Shaped(Case, frozen=True, kw_only=True):
    """Light kept inside a feathered outline, falling to a color outside."""

    shape: Annotated[masks.Shape, msgspec.Meta(description="Outline keeping frame light")] = masks.Shape(size=(0.8, 0.45), feather=0.2)
    color: Annotated[Rgb, msgspec.Meta(description="Scene-linear color outside shape")] = Rgb(red=0.0, green=0.0, blue=0.0)


class Vignette(Effect, frozen=True, kw_only=True):
    """Frame-edge falloff from camera chief rays or a feathered outline."""

    stage = Stage.VIGNETTE
    pointwise = False
    falloff: Annotated[Natural | Shaped, msgspec.Meta(description="Lens law or drawn outline")] = Natural()


# --- [OPERATIONS] -----------------------------------------------------------------------


def vignetted(shell: Shell[Vignette]) -> BlenderResult[Expr]:
    """Mixes the stage input toward the pull color by the selected falloff weight."""
    (falloff,) = Vignette.__struct_fields__
    shape, color = Shaped.__struct_fields__
    aspect, scale, _ = CameraFrame.__struct_fields__
    source = shell.color
    entering = {"image": source, "aspect": shell.input(Shell.frame, aspect), "scale": shell.input(Shell.frame, scale)}

    def built(cosine: Expr, outline: NodeGroup) -> Expr:
        shaped = outline.placed(masks.relayed(lambda key: entering[key] if key in entering else shell.input(falloff, Shaped, shape, key), outline.interface), masks.KEY.key)
        pull = shell.menu(falloff, cases={Natural.title: (0.0, 0.0, 0.0, 1.0), Shaped.title: shell.input(falloff, Shaped, color)}, kind="RGBA")
        weight = switched(msgspec.structs.replace(pull, socket=Shaped.title, shape="FLOAT"), cosine**4.0, shaped).named("weight")
        covered = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", pull), ("Alpha", source["Alpha"]), ("Type", "Apply Mask")), socket="Image", shape="RGBA")
        return mixed(weight, covered, source)

    return paired(lenses.chief_ray(source), masks.shape()).map(lambda parts: built(*parts))


def conditioned(record: Vignette) -> Condition:
    """Requires a projecting camera for the natural falloff alone."""
    match record.falloff:
        case Natural():
            return cameras.camera_type("PERSP", "ORTHO")
        case Shaped():
            return AllOf(conditions=())


# --- [COMPOSITION] ----------------------------------------------------------------------

vignette = CompositorEffect(
    record=Vignette,
    graph=vignetted,
    parts=(Conditions[Vignette](condition=conditioned), Inspects(key="vignette_weight", end=End(node="weight", socket="Output"), description="Selected natural or shaped falloff")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Natural", "Shaped", "Vignette", "vignette"]
