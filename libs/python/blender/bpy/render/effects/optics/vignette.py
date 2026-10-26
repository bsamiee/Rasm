"""Camera chief rays and scene-linear vignette falloffs."""

import functools

import bpy
from expression import Some
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Condition
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, group_input, mixed, NodeGroup, switched, vector
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.effects.compositing import masks
from blender.bpy.render.effects.coordinates import framed, lensed
from blender.bpy.render.effects.rows import CameraFrame, CompositorEffect, Inspected, Shell
from blender.bpy.scene import cameras
from blender.imaging.effects.optics.vignette import Natural, Shaped, Vignette
from blender.imaging.faults import paired

# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def chief_ray() -> BlenderResult[NodeGroup]:
    """Returns frame camera chief-ray cosine with sensor fit and shift."""
    pixel = framed(group_input("image", "RGBA"), 1.0).fraction
    inverse = Expr(kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", lensed("Projection Matrix")),), socket="Matrix")
    near, middle = (
        Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", vector("MULTIPLY_ADD", pixel, (2.0, 2.0, 0.0), (-1.0, -1.0, plane))), ("Transform", inverse)), socket="Vector", shape="VECTOR")
        for plane in (-1.0, 0.0)
    )
    return NodeGroup.built(
        "chief_ray",
        "Chief Ray",
        Interface(inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),), outputs=(Socket(key="cosine", name="Cosine", kind=bpy.types.NodeSocketFloat),)),
        {"cosine": abs(vector("NORMALIZE", middle - near)["Z"])},
    )


def vignetted(shell: Shell[Vignette]) -> BlenderResult[Expr]:
    """Builds natural and shaped falloffs with associated pull color.

    Args:
        shell: Source image, frame measures, field sockets, and falloff menu.

    Returns:
        Vignetted image, or chief-ray and shape group faults.
    """
    (falloff,) = Vignette.__struct_fields__
    shape, color = Shaped.__struct_fields__
    aspect, scale, _ = CameraFrame.__struct_fields__
    source = shell.color
    entering = {"image": source, "aspect": shell.input(Shell.frame, aspect), "scale": shell.input(Shell.frame, scale)}

    def built(ray: NodeGroup, outline: NodeGroup) -> Expr:
        natural = ray.placed({"image": source}, "cosine") ** 4.0
        shaped = outline.placed(masks.relayed(lambda key: entering[key] if key in entering else shell.input(falloff, Shaped, shape, key), outline.interface), masks.KEY.key)
        pull = shell.menu(falloff, cases={Natural.title: (0.0, 0.0, 0.0, 1.0), Shaped.title: shell.input(falloff, Shaped, color)}, kind="RGBA")
        weight = switched(msgspec.structs.replace(pull, socket=Shaped.title, shape="FLOAT"), natural, shaped).named("weight")
        covered = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", pull), ("Alpha", source["Alpha"]), ("Type", "Apply Mask")), socket="Image", shape="RGBA")
        return mixed(weight, covered, source)

    return paired(chief_ray(), masks.shape()).map(lambda groups: built(*groups))


def conditioned(effect: Vignette) -> Condition:
    """Returns camera requirement of selected falloff."""
    match effect.falloff:
        case Natural():
            return cameras.camera_type("PERSP", "ORTHO")
        case Shaped():
            return AllOf(conditions=())


# --- [COMPOSITION] ----------------------------------------------------------------------

vignette = CompositorEffect(
    record=Vignette,
    graph=vignetted,
    condition=Some(conditioned),
    inspected=(Inspected(key="vignette_weight", end=End(node="weight", socket="Output"), description="Selected natural or shaped falloff"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["chief_ray", "conditioned", "vignette", "vignetted"]
