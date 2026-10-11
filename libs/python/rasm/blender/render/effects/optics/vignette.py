"""Camera chief rays and scene-linear vignette falloffs."""

import functools

import bpy
from expression import Some

from rasm.blender.data.rna.conditions import AllOf, Condition
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, framed, INPUTS, Interface, math, menu, mixed, nested, node, NodeGroup, Socket, switched, vector
from rasm.blender.render.effects.compositing import masks
from rasm.blender.render.effects.stack import CompositorEffect, Shell
from rasm.blender.render.effects.viewers import inspection
from rasm.blender.scene import cameras
from rasm.imaging.effects.optics.vignette import Natural, Shaped, Vignette
from rasm.imaging.faults import accumulated, paired

# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def chief_ray() -> BlenderResult[NodeGroup]:
    """Returns the frame camera's chief-ray cosine, including sensor fit and shift."""
    pixel = framed("coordinates", End(node=INPUTS, socket="image"), 1.0).map(lambda frame: frame.fraction)
    inverse = (
        node("camera", bpy.types.GeometryNodeInputActiveCamera, {}, {})
        .bind(lambda camera: node("lens", bpy.types.GeometryNodeCameraInfo, {"Camera": ended("camera", "Active Camera")(camera)}, {}))
        .bind(lambda lens: node("unprojection", bpy.types.FunctionNodeInvertMatrix, {"Matrix": ended("lens", "Projection Matrix")(lens)}, {}))
        .map(ended("unprojection", "Matrix"))
    )
    return (
        paired(pixel, inverse)
        .bind(
            lambda ends: accumulated(
                vector(f"{depth}_device", "MULTIPLY_ADD", ends[0], (2.0, 2.0, 0.0), (-1.0, -1.0, plane))
                .bind(lambda device: node(depth, bpy.types.FunctionNodeProjectPoint, {"Vector": device, "Transform": ends[1]}, {}))
                .map(ended(depth, "Vector"))
                for depth, plane in (("near", -1.0), ("middle", 0.0))
            )
        )
        .bind(lambda points: vector("ray", "SUBTRACT", points[1], points[0]))
        .bind(lambda ray: vector("direction", "NORMALIZE", ray))
        .bind(lambda direction: node("axis", bpy.types.ShaderNodeSeparateXYZ, {"Vector": direction}, {}))
        .bind(lambda axis: math("cosine", "ABSOLUTE", ended("axis", "Z")(axis)))
        .bind(
            lambda cosine: NodeGroup.built(
                "chief_ray",
                "Chief Ray",
                Interface(inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),), outputs=(Socket(key="cosine", name="Cosine", kind=bpy.types.NodeSocketFloat),)),
                {"cosine": cosine},
            )
        )
    )


def vignetted(shell: Shell[Vignette]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds natural and shaped falloffs with associated pull color.

    Args:
        shell: Source image, frame measures, field sockets, and falloff menu.

    Returns:
        The vignetted image and graph, or accumulated native construction faults.
    """
    entering = {"image": shell.source, "aspect": shell.aspect, "scale": shell.scale}
    falloff = Vignette.falloff.__name__  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    natural = chief_ray().bind(lambda group: nested("chief_ray", group, {"image": shell.source}, "cosine")).bind(lambda cosine: math("natural", "POWER", cosine, 4.0))
    shaped = masks.shape().bind(
        lambda group: nested("shape", group, masks.relayed(lambda key: entering[key] if key in entering else shell.socket(falloff, Shaped, Shaped.shape.__name__, key), group.interface), "key")  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    )
    pull = menu("pull", "RGBA", shell.socket(falloff), shell.items[falloff,], {Natural.title: (0.0, 0.0, 0.0, 1.0), Shaped.title: shell.socket(falloff, Shaped, Shaped.color.__name__)})  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    alpha = node("pull_alpha", bpy.types.CompositorNodeSeparateColor, {"Image": shell.source}, {"mode": "RGB"}).map(ended("pull_alpha", "Alpha"))
    weight = paired(natural, shaped).bind(lambda ends: switched("weight", "FLOAT", End(node="pull", socket=Shaped.title), *ends))
    covered = (
        paired(pull, alpha)
        .bind(lambda ends: node("pull_coverage", bpy.types.CompositorNodeSetAlpha, {"Image": ends[0], "Alpha": ends[1], "Type": "Apply Mask"}, {}))
        .map(ended("pull_coverage", "Image"))
    )
    return paired(weight, covered).bind(lambda ends: mixed("result", "RGBA", *ends, shell.source))


def conditioned(effect: Vignette) -> Condition:
    """Returns the camera requirement of the selected falloff."""
    match effect.falloff:
        case Natural():
            return cameras.camera_type("PERSP", "ORTHO")
        case Shaped():
            return AllOf(conditions=())


# --- [COMPOSITION] ----------------------------------------------------------------------

vignette: CompositorEffect[Vignette] = CompositorEffect(
    record=Vignette,
    graph=vignetted,
    condition=Some(conditioned),
    registrables=(inspection(lambda: vignette, "vignette_weight", "View Weight", "Shows the selected natural or shaped falloff", lambda: End(node="weight", socket="Output")),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["chief_ray", "conditioned", "vignette", "vignetted"]
