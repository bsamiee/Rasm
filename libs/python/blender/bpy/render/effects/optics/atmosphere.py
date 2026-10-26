"""SI ray attenuation, seeded opacity variation, and associated inscattering."""

import bpy
from expression import Some

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math, mixed, NodeGroup, switched, vector, where
from blender.bpy.render.effects.coordinates import clocked, pixels
from blender.bpy.render.effects.generators.noise import sampled
from blender.bpy.render.effects.optics.vignette import chief_ray
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, depth_precise, Inspected, Shell
from blender.bpy.scene import cameras
from blender.imaging.effects.optics.atmosphere import Ambient, Atmosphere, Diffused, Fluctuation
from blender.imaging.parameters import Gated, Motion

# --- [OPERATIONS] -----------------------------------------------------------------------


def attenuated(shell: Shell[Atmosphere]) -> BlenderResult[Expr]:
    """Builds distance-only attenuation, inscattering, and alpha medium coverage.

    Args:
        shell: Source light, scene meters per unit, Depth, and medium inputs.

    Returns:
        Scattered light, or chief-ray group faults.
    """
    inscatter, noise, attenuation, start, cutoff = Atmosphere.__struct_fields__
    (color,) = Ambient.__struct_fields__
    tint, radius = Diffused.__struct_fields__
    variation, basis, motion, seed = Fluctuation.__struct_fields__
    speed, hold = Motion.__struct_fields__
    _, _, meters = CameraFrame.__struct_fields__
    _, value = Gated.__struct_fields__
    inlet, source, frame = shell.input, shell.color, shell.space

    def scattered(ray: NodeGroup) -> Expr:
        distance = math("MULTIPLY", shell.layer("Depth"), inlet(Shell.frame, meters)) / ray.placed({"image": source}, "cosine")
        transmittance = math("EXPONENT", math("MINIMUM", inlet(start) - distance, 0.0) / inlet(attenuation))
        kept = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", where(shell.gate(cutoff) * (distance > inlet(cutoff)), 1.0, transmittance)),), socket="Image")
        blurred = Expr(
            kind=bpy.types.CompositorNodeBlur,
            inputs=(("Image", source), ("Size", pixels(frame, inlet(inscatter, Diffused, radius), "VECTOR")), ("Type", "Fast Gaussian")),
            socket="Image",
            shape="RGBA",
        )
        light = shell.menu(inscatter, cases={Ambient.title: inlet(inscatter, Ambient, color), Diffused.title: vector("MULTIPLY", blurred, inlet(inscatter, Diffused, tint))}, kind="RGBA")
        ((field, _),) = sampled(shell, (noise, value, basis), frame.point, clocked(inlet(noise, value, motion, speed), inlet(noise, value, motion, hold)), inlet(noise, value, seed))
        coverage = (1.0 - kept) * (1.0 - switched(shell.gate(noise), 0.0, field * inlet(noise, value, variation)))
        alpha = math("MULTIPLY_ADD", source["Alpha"], (1.0 - coverage).named("transmitted"), coverage)
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", mixed(coverage, source, light)), ("Alpha", alpha), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    return chief_ray().map(scattered)


# --- [COMPOSITION] ----------------------------------------------------------------------

atmosphere = CompositorEffect(
    record=Atmosphere,
    graph=attenuated,
    alpha=Alpha.RESULT,
    requested=Some(depth_precise),
    condition=Some(lambda _: cameras.camera_type("PERSP", "ORTHO")),
    inspected=(Inspected(key="transmittance_view", end=End(node="transmitted"), description="Light share transmitted through medium"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["atmosphere", "attenuated"]
