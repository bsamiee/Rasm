"""Light-bearing media over the frame: a uniform medium along camera rays with SI depth transmittance, seeded opacity, and inscattered light, and a procedural field drifting, twirling, and evolving over the clock, emitting, absorbing, and scattering light."""

from typing import Final

import bpy
import colour
from expression import Ok, Some

from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, mixed, switched, vector, where, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor import lenses, motion
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, depth_precise, precise
from blender.bpy.render.compositor.filters import blurred
from blender.bpy.render.compositor.inputs import FRAME_METERS, GraphInputs
from blender.bpy.render.compositor.shapes import turned
from blender.bpy.scene import cameras
from blender.imaging.color.processors import GRAY
from blender.imaging.sampling import tiles
from blender.records.faults import Outcome
from blender.records.fields import Case

from .bases import bases, Basis, Perlin, textured
from .emission import Emission, emission, emitted, Incandescence

# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [ATMOSPHERE]

_ATTENUATION_LENGTH: Final = Socket(key="attenuation_length", kind=bpy.types.NodeSocketFloat, description="Medium depth in meters passing 1/e of light", min_value=0.0, default_value=30.0)
_START: Final = Socket(key="start", kind=bpy.types.NodeSocketFloat, description="Camera distance in meters where medium starts", min_value=0.0)
_AMBIENT_COLOR: Final = Socket(
    key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear scattered light", default_value=(*(float(colour.cctf_decoding(217 / 255, function="sRGB")),) * 3, 1.0)
)
_TINT: Final = Socket(key="tint", kind=bpy.types.NodeSocketColor, description="Scene-linear spread light factor", default_value=(GRAY, GRAY, GRAY, 1.0))
_SPREAD: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Gaussian spread size", min_value=0.0, default_value=0.75)
_AMBIENT: Final = InterfacePanel.of("ambient", Interface(inputs=(_AMBIENT_COLOR,)), description="Uniformly lit medium scattering one color toward camera")
_DIFFUSED: Final = InterfacePanel.of("diffused", Interface(inputs=(_TINT, _SPREAD)), description="Frame light spread through medium and tinted")
_LIGHTS: Final = Interface(panels=(_AMBIENT, _DIFFUSED))
_LIGHT: Final = Socket(key="source", kind=bpy.types.NodeSocketMenu, description="Scattered light source", default_value=_AMBIENT.title, items=Some(_LIGHTS.menu))
_INSCATTERING: Final = InterfacePanel.of("inscattering", Interface(inputs=(_LIGHT,), panels=_LIGHTS.panels), description="Light the medium scatters toward camera")
_VARIATION: Final = Socket(key="variation", kind=bpy.types.NodeSocketFloatFactor, description="Largest opacity share field removes", default_value=1.0)
_CUTOFF_DISTANCE: Final = Socket(key="distance", kind=bpy.types.NodeSocketFloat, description="Ray distance in meters past which samples keep light", min_value=0.0, default_value=1000.0)
_CUTOFF: Final = InterfacePanel.of("cutoff", Interface(inputs=(_CUTOFF_DISTANCE,)), description="Ray distance past which samples keep light", toggle=Some(False))

# --- [FIELD]

_FIRE: Final = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=2.5, detail=4.0, distortion=1.0), stretch=2.0, cut=textures.MapRange(from_min=0.17))
_GLOW: Final = Emission(spectrum=Incandescence(cool=800.0), intensity=0.0)
_DRIFT: Final = Socket(key="drift", kind=bpy.types.NodeSocketVector2D, description="Velocity along field axes in greater frame sides per clock unit", default_value=(0.0, 2.8))
_EVOLUTION: Final = Socket(key="evolution", kind=bpy.types.NodeSocketFloat, description="Signed feature evolution per clock unit")
_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Axis pivot in frame fractions", default_value=(0.5, 0.0))
_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Turn of field axes")
_DENSITY: Final = Socket(key="density", kind=bpy.types.NodeSocketFloat, description="Optical depth at full density", min_value=0.0, default_value=2.0)
_SCATTERED: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear light scattered toward camera", default_value=(0.04, 0.04, 0.04, 1.0))
_EXTINCTION: Final = InterfacePanel.of("extinction", Interface(inputs=(_DENSITY, _SCATTERED)), description="Light the field absorbs and scatters", toggle=Some(False))
_VORTEX_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Twirl and widening reach over greater frame side", min_value=0.0, default_value=0.25)
_TWIRL: Final = Socket(key="twirl", kind=bpy.types.NodeSocketFloatAngle, description="Turn at center, falling to 0 at radius", default_value=3.0)
_ARMS: Final = Socket(key="arms", kind=bpy.types.NodeSocketInt, description="Field repeats around center, 0 leaving rings", min_value=0, default_value=1)
_SPIN: Final = Socket(key="spin", kind=bpy.types.NodeSocketFloat, description="Vortex turn in radians per clock unit", default_value=1.0)
_TUNNEL: Final = Socket(key="tunnel", kind=bpy.types.NodeSocketFloat, description="Axial sample height per unit radius", min_value=0.0, default_value=1.0)
_VORTEX: Final = InterfacePanel.of("vortex", Interface(inputs=(_VORTEX_RADIUS, _TWIRL, _ARMS, _SPIN, _TUNNEL)), description="Polar twirl of field about center", toggle=Some(False))
_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Horizontal radius over greater frame side", min_value=0.0, default_value=0.5)
_HEIGHT: Final = Socket(key="height", kind=bpy.types.NodeSocketFloat, description="Vertical radius over greater frame side", min_value=0.0, default_value=0.5)
_ENVELOPE: Final = InterfacePanel.of("envelope", Interface(inputs=(_WIDTH, _HEIGHT)), description="Falloff before the texture cut", toggle=Some(True))

# --- [MODELS] ---------------------------------------------------------------------------


class Atmosphere(Case, frozen=True, kw_only=True):
    """Uniform medium absorbing and scattering light along camera rays, its opacity field basis holding the node settings."""

    noise_basis: Basis = Perlin()


class NoiseField(Case, frozen=True, kw_only=True):
    """Procedural light and haze added to frame: fire, smoke, mist, light leaks, and flow, its basis and emission holding their node settings and ramps."""

    basis: Basis = _FIRE
    emission: Emission = _GLOW


# --- [INTERFACES] -----------------------------------------------------------------------

_NOISE_BASIS: Final = bases("basis", Atmosphere().noise_basis, "Procedural opacity field")
_NOISE: Final = InterfacePanel.of(
    "noise",
    Interface(inputs=(_VARIATION, motion.SEED), panels=(_NOISE_BASIS, InterfacePanel.of(motion.CLOCK.key, motion.Clock(speed=0.0)))),
    description="Seeded opacity reduction",
    toggle=Some(False),
)
_BASIS: Final = bases("basis", _FIRE, "Procedural texture")
_EMISSION: Final = emission("emission", _GLOW, "Light the field adds", Some(True))

# --- [OPERATIONS] -----------------------------------------------------------------------


def _hazed(shell: GraphInputs[Atmosphere]) -> Outcome[Expr]:
    """Mixes inscattered light over the stage input by medium coverage along each camera ray, coverage joining alpha."""
    read, source = shell.input, shell.color
    distance = math("MULTIPLY", shell.layer("Depth"), read(FRAME_METERS)) / lenses.chief_ray(source)
    transmittance = math("EXPONENT", math("MINIMUM", read(_START) - distance, 0.0) / read(_ATTENUATION_LENGTH))
    kept = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", where(read(_CUTOFF) * (distance > read(_CUTOFF, _CUTOFF_DISTANCE)), 1.0, transmittance)),), socket="Image")
    ((field, _),) = textured(
        shell,
        _NOISE.panel(_NOISE_BASIS.key),
        shell.record.noise_basis,
        shell.space.point,
        motion.clocked(read, _NOISE.panel(motion.CLOCK.key)),
        read(_NOISE, motion.SEED),
        streams=(tiles.stream(Atmosphere.noise_basis),),
    )
    coverage = (1.0 - kept) * (1.0 - switched(read(_NOISE), 0.0, field * read(_NOISE, _VARIATION)))
    alpha = math("MULTIPLY_ADD", source["Alpha"], (1.0 - coverage).keyed("transmitted"), coverage)
    spread = blurred(source, shell.space.pixel_size(read(_INSCATTERING, _DIFFUSED, _SPREAD)))
    lit = shell.menu(_INSCATTERING, _LIGHT, cases={_AMBIENT.item: read(_INSCATTERING, _AMBIENT, _AMBIENT_COLOR), _DIFFUSED.item: vector("MULTIPLY", spread, read(_INSCATTERING, _DIFFUSED, _TINT))})
    return Ok(with_alpha(mixed(coverage, source, lit), alpha))


def _fielded(shell: GraphInputs[NoiseField]) -> Outcome[Expr]:
    """Emits and extinguishes light over the field's density, coverage as alpha."""
    read, at = shell.input, shell.space
    clock = motion.clocked(read, motion.CLOCK).keyed("clock")
    axis = turned(at, read(_CENTER), read(_ANGLE))
    radius = read(_VORTEX, _VORTEX_RADIUS)
    rho = vector("LENGTH", axis) / radius
    phase = math("MULTIPLY_ADD", math("ARCTAN2", axis["Y"], axis["X"]), read(_VORTEX, _ARMS), math("MULTIPLY_ADD", 1.0 - rho, read(_VORTEX, _TWIRL), clock * read(_VORTEX, _SPIN)))
    cone = vector("SCALE", xyz(math("COSINE", phase), math("SINE", phase), read(_VORTEX, _TUNNEL)), math("MINIMUM", rho, 1.0) * radius)
    reach = math("MAXIMUM", 1.0 - vector("LENGTH", axis / xyz(read(_ENVELOPE, _WIDTH), read(_ENVELOPE, _HEIGHT))), 0.0)
    ((density, tone),) = textured(
        shell,
        _BASIS,
        shell.record.basis,
        switched(read(_VORTEX), axis, cone) - vector("SCALE", read(_DRIFT), clock),
        clock * read(_EVOLUTION),
        read(motion.SEED),
        loss=switched(read(_ENVELOPE), 0.0, 1.0 - reach * reach),
        streams=(tiles.stream(NoiseField.basis),),
    )
    coverage = 1.0 - switched(read(_EXTINCTION), 1.0, math("EXPONENT", -(density * read(_EXTINCTION, _DENSITY))))

    def generated(lights: tuple[Expr, ...]) -> Expr:
        light = switched(read(_EMISSION), (0.0, 0.0, 0.0), vector("SCALE", lights[0], density)) + vector("SCALE", read(_EXTINCTION, _SCATTERED), coverage)
        return with_alpha(light, coverage)

    return emitted(shell, _EMISSION, shell.record.emission, (tone,)).map(generated)


# --- [COMPOSITION] ----------------------------------------------------------------------

atmosphere: Final = CompositorEffect(
    _hazed,
    record=Atmosphere,
    interface=Interface(inputs=(_ATTENUATION_LENGTH, _START), panels=(_INSCATTERING, _NOISE, _CUTOFF)),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(depth_precise),
    check=Some(lambda _identity, scene, _entry, _record, _preview: cameras.camera_type("PERSP", "ORTHO").check(bpy.context, Some(scene))),
    derives=(motion.timing,),
)
noise_field: Final = CompositorEffect(
    _fielded,
    record=NoiseField,
    interface=Interface(inputs=(_DRIFT, _EVOLUTION, _CENTER, _ANGLE, motion.SEED), panels=(_BASIS, _EMISSION, _EXTINCTION, motion.CLOCK, _VORTEX, _ENVELOPE)),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(precise),
    derives=(motion.timing,),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Atmosphere", "NoiseField", "atmosphere", "noise_field"]
