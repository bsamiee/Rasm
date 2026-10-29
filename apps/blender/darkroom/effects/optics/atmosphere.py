"""Uniform medium along camera rays: SI depth transmittance, seeded opacity variation, and the light it scatters toward camera."""

from typing import Final

import bpy
import colour
from expression import Some

from blender.bpy.nodes.graphs import Expr, math, mixed, switched, vector, where
from blender.bpy.nodes.interface import Interface, InterfacePanel, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.coordinates import pixels
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, depth_precise
from blender.bpy.render.compositor.inputs import FRAME_METERS
from blender.bpy.render.compositor.motion import clocked
from blender.bpy.render.compositor.shells import Shell
from blender.bpy.scene import cameras
from blender.imaging.color.processors import GREY
from blender.imaging.sampling import tiles
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from .procedural import bases, Basis, clock, Perlin, textured

# --- [CONSTANTS] ------------------------------------------------------------------------

_AMBIENT: Final = MenuItem(name="Ambient", description="Uniformly lit medium scattering one color toward camera")
_DIFFUSED: Final = MenuItem(name="Diffused", description="Frame light spread through medium and tinted")
_ATTENUATION_LENGTH: Final = Socket(key="attenuation_length", kind=bpy.types.NodeSocketFloatDistance, description="Medium depth passing 1/e of light", min_value=0.0, default_value=30.48)
_START: Final = Socket(key="start", kind=bpy.types.NodeSocketFloatDistance, description="Camera distance where medium starts", min_value=0.0)
_INSCATTER_KIND: Final = Socket(
    key="inscatter_kind", name="Source", kind=bpy.types.NodeSocketMenu, description="Scattered light source", default_value=_AMBIENT.name, items=Some(MenuItems(items=(_AMBIENT, _DIFFUSED)))
)
_AMBIENT_COLOR: Final = Socket(
    key="inscatter_ambient_color",
    name="Color",
    kind=bpy.types.NodeSocketColor,
    description="Scene-linear scattered light",
    default_value=(*(float(colour.cctf_decoding(217 / 255, function="sRGB")),) * 3, 1.0),
)
_DIFFUSED_TINT: Final = Socket(key="inscatter_diffused_tint", name="Tint", kind=bpy.types.NodeSocketColor, description="Scene-linear spread light factor", default_value=(GREY, GREY, GREY, 1.0))
_DIFFUSED_RADIUS: Final = Socket(key="inscatter_diffused_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Gaussian spread size", min_value=0.0, default_value=0.75)
_NOISE_VARIATION: Final = Socket(key="noise_variation", name="Variation", kind=bpy.types.NodeSocketFloatFactor, description="Largest opacity share field removes", default_value=1.0)
_NOISE_SEED: Final = Socket(key="noise_seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting pattern", min_value=0, max_value=2**24)
_NOISE_CLOCK: Final = clock("noise_clock", speed=0.0)
_NOISE_CLOCK_SPEED, _NOISE_CLOCK_HOLD = _NOISE_CLOCK.inputs
_CUTOFF_DISTANCE: Final = Socket(
    key="cutoff_distance", name="Distance", kind=bpy.types.NodeSocketFloatDistance, description="Ray distance past which samples keep light", min_value=0.0, default_value=914.4
)
_CUTOFF: Final = InterfacePanel(key="cutoff", name="Cutoff", description="Ray distance past which samples keep light", toggle=Some(False), inputs=(_CUTOFF_DISTANCE,))

# --- [MODELS] ---------------------------------------------------------------------------


class Atmosphere(Effect, frozen=True, kw_only=True):
    """Uniform medium absorbing and scattering light along camera rays, its opacity field basis holding the node settings."""

    stage = Stage.ATMOSPHERE
    pointwise = False
    basis: Basis = Perlin()


# --- [INTERFACES] -----------------------------------------------------------------------

_NOISE: Final = InterfacePanel(
    key="noise",
    name="Noise",
    description="Seeded opacity reduction",
    toggle=Some(False),
    inputs=(_NOISE_VARIATION, _NOISE_SEED),
    panels=(bases("noise_basis", Atmosphere().basis, "Procedural opacity field"), _NOISE_CLOCK),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def hazed(shell: Shell[Atmosphere]) -> Outcome[Expr]:
    """Mixes inscattered light over the stage input by medium coverage along each camera ray, coverage joining alpha."""
    inlet, source, frame = shell.input, shell.color, shell.space

    def scattered(cosine: Expr, light: Expr) -> Expr:
        distance = math("MULTIPLY", shell.layer("Depth"), inlet(FRAME_METERS)) / cosine
        transmittance = math("EXPONENT", math("MINIMUM", inlet(_START) - distance, 0.0) / inlet(_ATTENUATION_LENGTH))
        kept = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", where(inlet(_CUTOFF) * (distance > inlet(_CUTOFF_DISTANCE)), 1.0, transmittance)),), socket="Image")
        ((field, _),) = textured(
            shell, "noise_basis", shell.record.basis, frame.point, clocked(inlet(_NOISE_CLOCK_SPEED), inlet(_NOISE_CLOCK_HOLD)), inlet(_NOISE_SEED), streams=(tiles.stream(Atmosphere, "basis"),)
        )
        coverage = (1.0 - kept) * (1.0 - switched(inlet(_NOISE), 0.0, field * inlet(_NOISE_VARIATION)))
        alpha = math("MULTIPLY_ADD", source["Alpha"], (1.0 - coverage).named("transmitted"), coverage)
        return with_alpha(mixed(coverage, source, light), alpha)

    blurred = Expr(
        kind=bpy.types.CompositorNodeBlur,
        inputs=(("Image", source), ("Size", pixels(frame, inlet(_DIFFUSED_RADIUS), bpy.types.NodeSocketVector)), ("Type", "Fast Gaussian")),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )
    lit = shell.menu(_INSCATTER_KIND, cases={_AMBIENT: inlet(_AMBIENT_COLOR), _DIFFUSED: vector("MULTIPLY", blurred, inlet(_DIFFUSED_TINT))})
    return lenses.chief_ray(source).map(lambda cosine: scattered(cosine, lit))


# --- [COMPOSITION] ----------------------------------------------------------------------

atmosphere: Final = CompositorEffect(
    hazed,
    record=Atmosphere,
    interface=Interface(
        inputs=(_ATTENUATION_LENGTH, _START),
        panels=(
            InterfacePanel(
                key="inscatter",
                name="Inscattering",
                description="Scattered light source",
                inputs=(_INSCATTER_KIND,),
                panels=(
                    InterfacePanel(key="inscatter_ambient", name="Ambient", description="Uniformly lit medium scattering one color toward camera", inputs=(_AMBIENT_COLOR,)),
                    InterfacePanel(key="inscatter_diffused", name="Diffused", description="Frame light spread through medium and tinted", inputs=(_DIFFUSED_TINT, _DIFFUSED_RADIUS)),
                ),
            ),
            _NOISE,
            _CUTOFF,
        ),
    ),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(depth_precise),
    check=Some(lambda _identity, scene, _entry, _record, _preview: cameras.camera_type("PERSP", "ORTHO").check(bpy.context, Some(scene))),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Atmosphere", "atmosphere"]
