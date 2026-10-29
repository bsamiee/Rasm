"""Procedural light and haze: a textured field drifting, twirling, and evolving over the clock, emitting, absorbing, and scattering light."""

from math import exp
from typing import Final

import bpy
from expression import Some

from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.motion import clocked
from blender.bpy.render.compositor.shells import Shell
from blender.imaging.sampling import tiles
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from .procedural import bases, Basis, CLOCK, Emission, emission, emitted, Incandescence, Perlin, textured

# --- [CONSTANTS] ------------------------------------------------------------------------

_FIRE_SHORT_SIDE: Final = 9.0 / 16.0
_FIRE: Final = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=1.4 / _FIRE_SHORT_SIDE, detail=4.0, distortion=1.0), stretch=2.0, cut=textures.MapRange(from_min=0.17))
_GLOW: Final = Emission(spectrum=Incandescence(cool=800.0), intensity=0.0)
_DRIFT: Final = Socket(key="drift", kind=bpy.types.NodeSocketVector2D, description="Velocity along field axes in greater frame sides per clock unit", default_value=(0.0, 0.208 * _FIRE_SHORT_SIDE))
_EVOLUTION: Final = Socket(key="evolution", kind=bpy.types.NodeSocketFloat, description="Signed feature evolution per clock unit")
_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Axis pivot in frame fractions", default_value=(0.5, 0.0))
_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Turn of field axes")
_SEED: Final = Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting pattern", min_value=0, max_value=2**24)
_BASIS: Final = bases("basis", _FIRE, "Procedural texture")
_EMISSION: Final = emission("emission", _GLOW, "Light the field adds", Some(True))
_EXTINCTION_DENSITY: Final = Socket(key="extinction_density", name="Density", kind=bpy.types.NodeSocketFloat, description="Optical depth at full density", min_value=0.0, default_value=2.0)
_EXTINCTION_COLOR: Final = Socket(key="extinction_color", name="Color", kind=bpy.types.NodeSocketColor, description="Scene-linear light scattered toward camera", default_value=(0.04, 0.04, 0.04, 1.0))
_EXTINCTION: Final = InterfacePanel(key="extinction", name="Extinction", description="Light the field absorbs and scatters", toggle=Some(False), inputs=(_EXTINCTION_DENSITY, _EXTINCTION_COLOR))
_VORTEX_RADIUS: Final = Socket(key="vortex_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Twirl and widening reach over greater frame side", min_value=0.0, default_value=0.25)
_VORTEX_TWIRL: Final = Socket(key="vortex_twirl", name="Twirl", kind=bpy.types.NodeSocketFloatAngle, description="Turn at center, falling to 0 at radius", default_value=3.0)
_VORTEX_ARMS: Final = Socket(key="vortex_arms", name="Arms", kind=bpy.types.NodeSocketInt, description="Field repeats around center, 0 leaving rings", min_value=0, default_value=1)
_VORTEX_SPIN: Final = Socket(key="vortex_spin", name="Spin", kind=bpy.types.NodeSocketFloat, description="Vortex turn in radians per clock unit", default_value=0.0417)
_VORTEX_TUNNEL: Final = Socket(key="vortex_tunnel", name="Tunnel", kind=bpy.types.NodeSocketFloat, description="Axial sample height per unit radius", min_value=0.0, default_value=1.0)
_VORTEX: Final = InterfacePanel(
    key="vortex", name="Vortex", description="Polar twirl of field about center", toggle=Some(False), inputs=(_VORTEX_RADIUS, _VORTEX_TWIRL, _VORTEX_ARMS, _VORTEX_SPIN, _VORTEX_TUNNEL)
)
_ENVELOPE_WIDTH: Final = Socket(
    key="envelope_width", name="Width", kind=bpy.types.NodeSocketFloat, description="Horizontal radius over greater frame side", min_value=0.0, default_value=_FIRE_SHORT_SIDE
)
_ENVELOPE_HEIGHT: Final = Socket(
    key="envelope_height", name="Height", kind=bpy.types.NodeSocketFloat, description="Vertical radius over greater frame side", min_value=0.0, default_value=_FIRE_SHORT_SIDE
)
_ENVELOPE: Final = InterfacePanel(key="envelope", name="Envelope", description="Falloff before the texture cut", toggle=Some(True), inputs=(_ENVELOPE_WIDTH, _ENVELOPE_HEIGHT))

# --- [MODELS] ---------------------------------------------------------------------------


class NoiseField(Effect, frozen=True, kw_only=True):
    """Procedural light and haze added to frame: fire, smoke, mist, light leaks, and flow, its basis and emission holding their node settings and ramps."""

    stage = Stage.GENERATE
    pointwise = False
    basis: Basis = _FIRE
    emission: Emission = _GLOW


# --- [OPERATIONS] -----------------------------------------------------------------------


def fielded(shell: Shell[NoiseField]) -> Outcome[Expr]:
    """Emits and extinguishes light over the field's density, coverage as alpha."""
    inlet, at = shell.input, shell.space
    speed, hold = CLOCK.inputs
    clock = clocked(inlet(speed), inlet(hold)).named("clock")
    axis = Expr(
        kind=bpy.types.ShaderNodeVectorRotate,
        inputs=(("Vector", at.point - inlet(_CENTER) * at.extent), ("Angle", inlet(_ANGLE))),
        members={"rotation_type": "Z_AXIS", "invert": True},
        socket="Vector",
        shape=bpy.types.NodeSocketVector,
    )
    radius = inlet(_VORTEX_RADIUS)
    rho = vector("LENGTH", axis) / radius
    phase = math("MULTIPLY_ADD", math("ARCTAN2", axis["Y"], axis["X"]), inlet(_VORTEX_ARMS), math("MULTIPLY_ADD", 1.0 - rho, inlet(_VORTEX_TWIRL), clock * inlet(_VORTEX_SPIN)))
    cone = vector("SCALE", xyz(math("COSINE", phase), math("SINE", phase), inlet(_VORTEX_TUNNEL)), math("MINIMUM", rho, 1.0) * radius)
    reach = math("MAXIMUM", 1.0 - vector("LENGTH", axis / xyz(inlet(_ENVELOPE_WIDTH), inlet(_ENVELOPE_HEIGHT))), 0.0)
    ((density, tone),) = textured(
        shell,
        _BASIS.key,
        shell.record.basis,
        switched(inlet(_VORTEX), axis, cone) - vector("SCALE", inlet(_DRIFT), clock),
        clock * inlet(_EVOLUTION),
        inlet(_SEED),
        loss=switched(inlet(_ENVELOPE), 0.0, 1.0 - reach * reach),
        streams=(tiles.stream(NoiseField, "basis"),),
    )
    coverage = 1.0 - switched(inlet(_EXTINCTION), 1.0, math("POWER", exp(-1.0), density * inlet(_EXTINCTION_DENSITY)))

    def generated(lights: tuple[Expr, ...]) -> Expr:
        light = switched(inlet(_EMISSION), (0.0, 0.0, 0.0), vector("SCALE", lights[0], density)) + vector("SCALE", inlet(_EXTINCTION_COLOR), coverage)
        return with_alpha(light, coverage)

    return emitted(shell, _EMISSION.key, shell.record.emission, (tone,)).map(generated)


# --- [COMPOSITION] ----------------------------------------------------------------------

noise_field: Final = CompositorEffect(
    fielded,
    record=NoiseField,
    interface=Interface(inputs=(_DRIFT, _EVOLUTION, _CENTER, _ANGLE, _SEED), panels=(_BASIS, _EMISSION, _EXTINCTION, CLOCK, _VORTEX, _ENVELOPE)),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(precise),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NoiseField", "noise_field"]
