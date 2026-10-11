"""SI ray attenuation, seeded opacity variation, and associated inscattering."""

import bpy
from expression import Some

from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import Clock, compared, End, ended, Fragment, framed, FrameSpace, masked, math, menu, mixed, nested, node, pixels, switched, vector
from rasm.blender.render.effects.generators.noise import sampled
from rasm.blender.render.effects.optics.vignette import chief_ray
from rasm.blender.render.effects.stack import CompositorEffect, Coverage, Shell
from rasm.blender.render.effects.viewers import inspection
from rasm.blender.render.view_layers.passes import PassRequest
from rasm.blender.scene import cameras
from rasm.imaging.effects.optics.atmosphere import Ambient, Atmosphere, Diffused, Fluctuation
from rasm.imaging.faults import paired
from rasm.imaging.parameters import Gated, Motion

# --- [OPERATIONS] -----------------------------------------------------------------------


def attenuated(shell: Shell[Atmosphere]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds distance-only attenuation and the medium's own associated coverage.

    Args:
        shell: Source light, scene meters per unit, Depth, and medium inputs.

    Returns:
        Scattered light and graph, or independent native construction faults.
    """
    noise = (Atmosphere.noise.__name__, Gated.value.__name__)  # type: ignore[attr-defined, misc]  # ty: ignore[invalid-attribute-access, unresolved-attribute]
    inscatter = Atmosphere.inscatter.__name__  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    distance = paired(math("meters", "MULTIPLY", shell.layer("Depth"), shell.meters), chief_ray().bind(lambda group: nested("chief_ray", group, {"image": shell.source}, "cosine"))).bind(
        lambda ends: math("distance", "DIVIDE", *ends)
    )

    def medium(distance: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        transmission = (
            math("offset", "SUBTRACT", shell.socket(Atmosphere.start.__name__), distance)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(lambda offset: math("path", "MINIMUM", offset, 0.0))
            .bind(lambda path: math("exponent", "DIVIDE", path, shell.socket(Atmosphere.attenuation.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(lambda exponent: math("transmittance", "EXPONENT", exponent))
        )
        beyond = (
            compared("beyond", "GREATER_THAN", distance, shell.socket(Atmosphere.cutoff.__name__, Gated.value.__name__))  # type: ignore[attr-defined, misc]  # ty: ignore[invalid-attribute-access, unresolved-attribute]
            .bind(lambda beyond: node("cut", bpy.types.FunctionNodeBooleanMath, {"Boolean": shell.socket(Atmosphere.cutoff.__name__), "Boolean_001": beyond}, {"operation": "AND"}))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .map(ended("cut", "Boolean"))
        )
        return (
            paired(beyond, transmission)
            .bind(lambda ends: masked("kept", "FLOAT", ends[0], ends[1], 1.0))
            .bind(lambda kept: node("smoothed", bpy.types.CompositorNodeAntiAliasing, {"Image": kept}, {}))
            .map(ended("smoothed", "Image"))
        )

    def lit(frame: FrameSpace) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        spread = (
            pixels("spread", frame, shell.socket(inscatter, Diffused, Diffused.radius.__name__), "VECTOR")  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(lambda spread: node("blurred", bpy.types.CompositorNodeBlur, {"Image": shell.source, "Size": spread, "Type": "Fast Gaussian"}, {}))
            .map(ended("blurred", "Image"))
            .bind(lambda blurred: vector("diffused", "MULTIPLY", blurred, shell.socket(inscatter, Diffused, Diffused.tint.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(
                lambda diffused: menu(
                    "inscattered",
                    "RGBA",
                    shell.socket(inscatter),
                    shell.items[inscatter,],
                    {Ambient.title: shell.socket(inscatter, Ambient, Ambient.color.__name__), Diffused.title: diffused},  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                )
            )
        )
        clock = Clock(key="clock", speed=shell.socket(*noise, Fluctuation.motion.__name__, Motion.speed.__name__), hold=shell.socket(*noise, Fluctuation.motion.__name__, Motion.hold.__name__)).value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        field = (
            clock
            .bind(lambda time: sampled(shell, (*noise, Fluctuation.basis.__name__), frame.point, time, shell.socket(*noise, Fluctuation.seed.__name__)))  # type: ignore[attr-defined, union-attr]  # ty: ignore[unresolved-attribute]
            .map(lambda fields: fields[0][0])
            .bind(lambda field: math("variation", "MULTIPLY", field, shell.socket(*noise, Fluctuation.variation.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(lambda variation: switched("opacity_reduction", "FLOAT", shell.socket(Atmosphere.noise.__name__), 0.0, variation))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
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
