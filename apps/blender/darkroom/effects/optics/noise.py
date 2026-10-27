"""Procedural light and haze: a textured field drifting, twirling, and evolving over the clock, emitting, absorbing, and scattering light."""

from math import exp
from typing import Final

import bpy
from expression import Some

from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.compositor.coordinates import clocked
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, precise, Requests, Shell
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from .procedural import ANGLE, bases, Basis, CLOCK, Emission, emission, emitted, Incandescence, Perlin, PLANAR, textured

# --- [CONSTANTS] ------------------------------------------------------------------------

_FIRE_SHORT_SIDE: Final = 9.0 / 16.0

# --- [MODELS] ---------------------------------------------------------------------------


class NoiseField(Effect, frozen=True, kw_only=True):
    """Procedural light and haze added to frame: fire, smoke, mist, light leaks, and flow, its basis and emission holding their node settings and ramps."""

    stage = Stage.GENERATE
    pointwise = False
    basis: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=1.4 / _FIRE_SHORT_SIDE, detail=4.0, distortion=1.0), stretch=2.0, cut=textures.MapRange(from_min=0.17))
    emission: Emission = Emission(spectrum=Incandescence(cool=800.0), intensity=0.0)


# --- [OPERATIONS] -----------------------------------------------------------------------


def noised(shell: Shell[NoiseField]) -> Outcome[Expr]:
    """Emits and extinguishes light over the field's density, coverage as alpha."""
    inlet, at = shell.input, shell.space
    clock = clocked(inlet("clock_speed"), inlet("clock_hold")).named("clock")
    axis = Expr(
        kind=bpy.types.ShaderNodeVectorRotate,
        inputs=(("Vector", at.point - inlet("center") * at.extent), ("Angle", inlet("angle"))),
        members=(("rotation_type", "Z_AXIS"), ("invert", True)),
        socket="Vector",
        shape="VECTOR",
    )
    radius = inlet("vortex_radius")
    rho = vector("LENGTH", axis) / radius
    phase = math("MULTIPLY_ADD", math("ARCTAN2", axis["Y"], axis["X"]), inlet("vortex_arms"), math("MULTIPLY_ADD", 1.0 - rho, inlet("vortex_twirl"), clock * inlet("vortex_spin")))
    cone = vector("SCALE", xyz(math("COSINE", phase), math("SINE", phase), inlet("vortex_tunnel")), math("MINIMUM", rho, 1.0) * radius)
    reach = math("MAXIMUM", 1.0 - vector("LENGTH", axis / xyz(inlet("envelope_width"), inlet("envelope_height"))), 0.0)
    ((density, tone),) = textured(
        shell,
        "basis",
        shell.record.basis,
        switched(inlet("vortex"), axis, cone) - vector("SCALE", inlet("drift"), clock),
        clock * inlet("evolution"),
        inlet("seed"),
        loss=switched(inlet("envelope"), 0.0, 1.0 - reach * reach),
        streams=(textures.stream(NoiseField, "basis"),),
    )
    coverage = 1.0 - switched(inlet("extinction"), 1.0, math("POWER", exp(-1.0), density * inlet("extinction_density")))

    def generated(lights: tuple[Expr, ...]) -> Expr:
        light = switched(inlet("emission"), (0.0, 0.0, 0.0), vector("SCALE", lights[0], density)) + vector("SCALE", inlet("extinction_color"), coverage)
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", light), ("Alpha", coverage), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    return emitted(shell, "emission", shell.record.emission, (tone,)).map(generated)


# --- [COMPOSITION] ----------------------------------------------------------------------

noise_field = CompositorEffect(
    record=NoiseField,
    graph=noised,
    interface=Interface(
        inputs=tuple(
            Socket(key=key, name=bpy.path.display_name(key), kind=kind, description=text, members=members)
            for key, kind, text, members in (
                ("drift", bpy.types.NodeSocketVector, "Velocity along field axes in greater frame sides per clock frame", {**PLANAR, "default_value": (0.0, 0.208 * _FIRE_SHORT_SIDE)}),
                ("evolution", bpy.types.NodeSocketFloat, "Signed feature evolution per clock frame", {}),
                ("center", bpy.types.NodeSocketVector, "Axis pivot in frame fractions", {**PLANAR, "default_value": (0.5, 0.0)}),
                ("angle", bpy.types.NodeSocketFloat, "Turn of field axes", ANGLE),
                ("seed", bpy.types.NodeSocketInt, "Number selecting pattern", {"min_value": 0, "max_value": 2**24}),
            )
        ),
        panels=(
            bases("basis", NoiseField().basis, "Procedural texture"),
            emission("emission", NoiseField().emission, "Light the field adds", Some(True)),
            InterfacePanel(
                key="extinction",
                name="Extinction",
                description="Light the field absorbs and scatters",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("extinction", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                    for field, kind, text, members in (
                        ("density", bpy.types.NodeSocketFloat, "Optical depth at full density", {"min_value": 0.0, "default_value": 2.0}),
                        ("color", bpy.types.NodeSocketColor, "Scene-linear light scattered toward camera", {"default_value": (0.04, 0.04, 0.04, 1.0)}),
                    )
                ),
            ),
            CLOCK,
            InterfacePanel(
                key="vortex",
                name="Vortex",
                description="Polar twirl of field about center",
                toggle=Some(False),
                inputs=tuple(
                    Socket(key=item_key("vortex", field), name=bpy.path.display_name(field), kind=kind, description=text, members=members)
                    for field, kind, text, members in (
                        ("radius", bpy.types.NodeSocketFloat, "Twirl and widening reach over greater frame side", {"min_value": 0.0, "default_value": 0.25}),
                        ("twirl", bpy.types.NodeSocketFloat, "Turn at center, falling to 0 at radius", {**ANGLE, "default_value": 3.0}),
                        ("arms", bpy.types.NodeSocketInt, "Field repeats around center, 0 leaving rings", {"min_value": 0, "default_value": 1}),
                        ("spin", bpy.types.NodeSocketFloat, "Vortex turn in radians per clock frame", {"default_value": 0.0417}),
                        ("tunnel", bpy.types.NodeSocketFloat, "Axial sample height per unit radius", {"min_value": 0.0, "default_value": 1.0}),
                    )
                ),
            ),
            InterfacePanel(
                key="envelope",
                name="Envelope",
                description="Falloff before the texture cut",
                toggle=Some(True),
                inputs=tuple(
                    Socket(
                        key=item_key("envelope", field),
                        name=bpy.path.display_name(field),
                        kind=bpy.types.NodeSocketFloat,
                        description=f"{axis} radius over greater frame side",
                        members={"min_value": 0.0, "default_value": _FIRE_SHORT_SIDE},
                    )
                    for field, axis in (("width", "Horizontal"), ("height", "Vertical"))
                ),
            ),
        ),
    ),
    alpha=Alpha.RESULT,
    parts=(Requests[NoiseField](request=precise),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NoiseField", "noise_field"]
