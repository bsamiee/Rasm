"""SI ray attenuation, seeded opacity variation, and associated inscattering."""

import bpy
from expression import Some

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, pixels
from blender.bpy.nodes.graphs import Clock, compared, End, ended, Fragment, masked, math, menu, mixed, nested, node, switched, vector
from blender.bpy.render.effects.generators.noise import sampled
from blender.bpy.render.effects.optics.vignette import chief_ray
from blender.bpy.render.effects.stack import CompositorEffect, Coverage, Shell
from blender.bpy.render.effects.viewers import inspection
from blender.bpy.render.view_layers.passes import PassRequest
from blender.bpy.scene import cameras
from blender.imaging.effects.optics.atmosphere import Ambient, Atmosphere, Diffused, Fluctuation
from blender.imaging.faults import paired
from blender.imaging.parameters import Gated, Motion

# --- [OPERATIONS] -----------------------------------------------------------------------


def attenuated(shell: Shell[Atmosphere]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds distance-only attenuation and the medium's own associated coverage.

    Args:
        shell: Source light, scene meters per unit, Depth, and medium inputs.

    Returns:
        Scattered light and graph, or independent native construction faults.
    """
    inscatter, noise, attenuation, start, cutoff = Atmosphere.__struct_fields__
    _, value = Gated.__struct_fields__
    distance = paired(math("meters", "MULTIPLY", shell.layer("Depth"), shell.meters), chief_ray().bind(lambda group: nested("chief_ray", group, {"image": shell.source}, "cosine"))).bind(
        lambda ends: math("distance", "DIVIDE", *ends)
    )

    def medium(distance: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        transmission = (
            math("offset", "SUBTRACT", shell.socket(start), distance)
            .bind(lambda offset: math("path", "MINIMUM", offset, 0.0))
            .bind(lambda path: math("exponent", "DIVIDE", path, shell.socket(attenuation)))
            .bind(lambda exponent: math("transmittance", "EXPONENT", exponent))
        )
        beyond = (
            compared("beyond", "GREATER_THAN", distance, shell.socket(cutoff, value))
            .bind(lambda beyond: node("cut", bpy.types.FunctionNodeBooleanMath, {"Boolean": shell.socket(cutoff), "Boolean_001": beyond}, {"operation": "AND"}))
            .map(ended("cut", "Boolean"))
        )
        return (
            paired(beyond, transmission)
            .bind(lambda ends: masked("kept", "FLOAT", ends[0], ends[1], 1.0))
            .bind(lambda kept: node("smoothed", bpy.types.CompositorNodeAntiAliasing, {"Image": kept}, {}))
            .map(ended("smoothed", "Image"))
        )

    def lit(frame: FrameSpace) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        (color,) = Ambient.__struct_fields__
        tint, radius = Diffused.__struct_fields__
        variation_field, basis, motion, seed = Fluctuation.__struct_fields__
        speed, hold = Motion.__struct_fields__
        spread = (
            pixels("spread", frame, shell.socket(inscatter, Diffused, radius), "VECTOR")
            .bind(lambda spread: node("blurred", bpy.types.CompositorNodeBlur, {"Image": shell.source, "Size": spread, "Type": "Fast Gaussian"}, {}))
            .map(ended("blurred", "Image"))
            .bind(lambda blurred: vector("diffused", "MULTIPLY", blurred, shell.socket(inscatter, Diffused, tint)))
            .bind(lambda diffused: menu("inscattered", "RGBA", shell.socket(inscatter), shell.items[inscatter,], {Ambient.title: shell.socket(inscatter, Ambient, color), Diffused.title: diffused}))
        )
        clock = Clock(key="clock", speed=shell.socket(noise, value, motion, speed), hold=shell.socket(noise, value, motion, hold)).value
        field = (
            clock
            .bind(lambda time: sampled(shell, (noise, value, basis), frame.point, time, shell.socket(noise, value, seed)))
            .map(lambda fields: fields[0][0])
            .bind(lambda field: math("variation", "MULTIPLY", field, shell.socket(noise, value, variation_field)))
            .bind(lambda variation: switched("opacity_reduction", "FLOAT", shell.socket(noise), 0.0, variation))
        )
        return paired(spread, field)

    def scattered(parts: tuple[tuple[Fragment, End], tuple[tuple[Fragment, End], tuple[Fragment, End]], Fragment]) -> BlenderResult[tuple[Fragment, End]]:
        transmission, (light, reduction), source = parts
        opacity = math("opacity", "SUBTRACT", 1.0, transmission).bind(
            lambda opacity: math("remaining_opacity", "SUBTRACT", 1.0, reduction).bind(lambda remaining: math("coverage", "MULTIPLY", opacity, remaining))
        )
        return (
            opacity
            .bind(
                lambda opacity: paired(
                    mixed("scattered", "VECTOR", opacity, shell.source, light),
                    math("transmitted", "SUBTRACT", 1.0, opacity).bind(lambda transmitted: math("covered", "MULTIPLY_ADD", ended("source_alpha", "Alpha")(source), transmitted, opacity)),
                )
            )
            .bind(lambda ends: node("result", bpy.types.CompositorNodeSetAlpha, {"Image": ends[0], "Alpha": ends[1], "Type": "Replace Alpha"}, {}))
            .map(ended("result", "Image"))
        )

    return paired(
        paired(distance.bind(medium), framed("frame", shell.source, shell.aspect, shell.scale).bind(lit)),
        node("source_alpha", bpy.types.CompositorNodeSeparateColor, {"Image": shell.source}, {"mode": "RGB"}),
    ).bind(lambda parts: scattered((*parts[0], parts[1])))


# --- [COMPOSITION] ----------------------------------------------------------------------

atmosphere: CompositorEffect[Atmosphere] = CompositorEffect(
    record=Atmosphere,
    graph=attenuated,
    coverage=Coverage.RESULT,
    requested=Some(lambda _, layer: PassRequest.depth(layer, precise=True)),
    condition=Some(lambda _: cameras.camera_type("PERSP", "ORTHO")),
    registrables=(
        inspection(
            row=lambda: atmosphere,
            key="transmittance_view",
            title=f"View {Atmosphere.title} Transmittance",
            description="Shows the share of light kept through the medium",
            target=lambda: End(node="transmitted", socket="Value"),
        ),
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["atmosphere", "attenuated"]
