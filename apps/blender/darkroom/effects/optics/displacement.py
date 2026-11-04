"""Displacement fields composed into one warp in the compositor and sampled with anisotropic filtering: twirl, waves, turbulence, sphere, luminance, bubbles, image maps, glass, and raindrops."""

from enum import auto, Enum
from functools import partial, reduce
from math import pi, tau
from operator import add
from typing import Annotated, Final, override

import bpy
from expression import Some
import msgspec

from blender.bpy.nodes import textures
from blender.bpy.nodes.desired import MenuItem, MenuItems
from blender.bpy.nodes.graphs import Expr, math, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.coordinates import Border, extended, FrameSpace
from blender.bpy.render.compositor.editors import FilePath
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Mix, precise
from blender.bpy.render.compositor.inputs import GraphInputs
from blender.bpy.render.compositor.motion import CLOCK, Clock, clocked, hashed, SEED, timing
from blender.bpy.render.compositor.shapes import PROFILE, profiled, rotated
from blender.imaging.geometry.arithmetic import smoothstep
from blender.imaging.sampling.tiles import stream
from blender.records.faults import Outcome, paired

from ..grade.exposure import LATER, later  # ruff: ignore[relative-imports]
from .bases import bases, Basis, Perlin, textured
from .refraction import bubbled, BUBBLES, GLASS, lifted, LUMA, luminous, MAP, mapped, MapReading, RAINDROPS, rained, refracted

# --- [TYPES] ----------------------------------------------------------------------------


class Front(Enum):
    """Traveling bands or circular waves with radial and tangential motion."""

    LINEAR = auto()
    RADIAL = auto()
    ZIGZAG = auto()
    POND = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Point the field centers on in frame fractions", default_value=(0.5, 0.5))
_TURN: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Counterclockwise turn at center", default_value=6.1)
_REACH: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Distance where the turn reaches zero over greater frame side", min_value=0.0, default_value=0.5)
_FALLOFF: Final = Socket(key="falloff", kind=bpy.types.NodeSocketFloat, description="Radial falloff power", min_value=0.0, default_value=1.0)
_ARMS: Final = Socket(key="arms", kind=bpy.types.NodeSocketInt, description="Angular turn lobes, uniform at zero", min_value=0)
_ARM_OFFSET: Final = Socket(key="arm_offset", kind=bpy.types.NodeSocketFloatAngle, description="Angular lobe turn")
_FRONT: Final = Socket(
    key="front", kind=bpy.types.NodeSocketMenu, description="Wave front shape and displacement direction", default_value=MenuItem.of(Front.LINEAR).name, items=MenuItems.of(Front).items
)
_AMPLITUDE: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloat, description="Farthest offset over greater frame side", min_value=0.0, default_value=0.02)
_WAVELENGTH: Final = Socket(key="wavelength", kind=bpy.types.NodeSocketFloat, description="Distance between crests over greater frame side", min_value=0.0, default_value=tau / (20.0 * 2.0**0.5))
_DIRECTION: Final = Socket(key="direction", kind=bpy.types.NodeSocketFloatAngle, description="Wave travel direction")
_PHASE: Final = Socket(key="phase", kind=bpy.types.NodeSocketFloatAngle, description="Wave phase offset")
_WAVELENGTH_VARIATION: Final = Socket(key="wavelength", kind=bpy.types.NodeSocketFloat, description="Share each wavelength shrinks below maximum", min_value=0.0, max_value=0.999)
_AMPLITUDE_VARIATION: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloatFactor, description="Share each amplitude shrinks below maximum")
_SPREAD: Final = Socket(key="spread", kind=bpy.types.NodeSocketFloatAngle, description="Angular span of wave directions")
_AXES: Final = Socket(key="axes", kind=bpy.types.NodeSocketVector2D, description="Signed displacement multiplier per axis", default_value=(1.0, 1.0))
_FRONT_REACH: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Circular front reach over greater frame side", min_value=0.0, default_value=0.5)
_BULGE: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Sphere radius over greater frame side", min_value=0.0, default_value=0.15)
_AMOUNT: Final = Socket(key="amount", kind=bpy.types.NodeSocketFloat, description="Bulge above zero, pinch below zero", min_value=-1.0, max_value=1.0, default_value=0.1)
_SHARES: Final = Socket(key="axes", kind=bpy.types.NodeSocketVector2D, description="Bulge share per axis", min_value=0.0, max_value=1.0, default_value=(1.0, 1.0))
_INTERPOLATION: Final = Socket(key="interpolation", kind=StockInput(node=bpy.types.CompositorNodeMapUV, input="Interpolation"), default_value="Anisotropic")
_WAVE_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=0.7968))
_VARIATION: Final = InterfacePanel.of("variation", Interface(inputs=(_WAVELENGTH_VARIATION, _AMPLITUDE_VARIATION)), description="Random shrink of each superposed crest below the maximum")
_TURBULENCE_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=0.4992))
_TWIRL: Final = InterfacePanel.of("twirl", Interface(inputs=(_CENTER, _REACH, _TURN, _FALLOFF, _ARMS, _ARM_OFFSET)), description="Turn about a center with a smooth falloff", toggle=Some(False))
_WAVE: Final = InterfacePanel.of(
    "wave",
    Interface(inputs=(_FRONT, *PROFILE.inputs, _AMPLITUDE, _WAVELENGTH, _DIRECTION, _PHASE, _SPREAD, _AXES, _CENTER, _FRONT_REACH, SEED), panels=(_VARIATION, _WAVE_CLOCK)),
    description="Ripple across traveling periodic wave fronts",
    toggle=Some(False),
)
_SPHERE: Final = InterfacePanel.of("sphere", Interface(inputs=(_CENTER, _BULGE, _AMOUNT, _SHARES)), description="Bulge or pinch inside a circle", toggle=Some(False))

# --- [MODELS] ---------------------------------------------------------------------------


class Displacement(CompositorRecord, frozen=True, kw_only=True):
    """Composes geometric, image-driven, glass, and falling-drop displacement fields into one warp, holding the superposed wave count, the turbulence basis, the map file and reading, and the borders."""

    waves: Annotated[int, msgspec.Meta(ge=1, description="Waves superposed with equal total weight")] = 1
    turbulence: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=2.0))
    map_source: FilePath | None = None
    map_reading: MapReading = MapReading.OFFSETS
    extension_x: Border = Border.CLAMP
    extension_y: Border = Border.CLAMP

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Samples the entry input at each point less the summed toggled displacement, drops mixing their inverted, blurred view over the glass view."""
        frame, color = shell.space, shell.color

        def sample(source: Expr, point: Expr, coverage: Scalar) -> Expr:
            uv = frame.sampled(point / frame.extent)
            return extended(source, xyz(uv["X"], uv["Y"], coverage), (self.extension_x, self.extension_y), clipped=True, interpolation=shell.input(_INTERPOLATION))

        def composed(parts: tuple[Expr, tuple[Expr, Scalar]]) -> Expr:
            luminance, (map_offset, map_coverage) = parts
            height = lifted(luminance, shell.input(LATER))
            glass_offset, glass_source = refracted(shell, frame)
            drops, inside, drop_source = rained(shell, frame)
            terms = (
                (_TWIRL, _twirled(shell, frame)),
                (_WAVE, _waved(self, shell, frame)),
                (_TURBULENCE, _turbulent(self, shell, frame)),
                (_SPHERE, _bulged(shell, frame)),
                (LUMA, luminous(shell, frame, height)),
                (BUBBLES, bubbled(shell, frame, height)),
                (GLASS, glass_offset),
                (MAP, map_offset),
            )
            offset = reduce(add, (switched(shell.input(panel), (0.0, 0.0, 0.0), term) for panel, term in terms))
            coverage = switched(shell.input(MAP), 1.0, map_coverage)
            seen = sample(switched(shell.input(GLASS), color, glass_source), frame.point - offset, coverage)
            dropped = sample(drop_source, frame.point - offset - inside, coverage)
            return switched(shell.input(RAINDROPS), seen, with_alpha(mixed(drops, seen, dropped), mixed(drops, seen["Alpha"], dropped["Alpha"])))

        return paired(masks.luminance(shell.config, color), mapped(shell, frame, self.map_source, self.map_reading, shell.input(LATER))).map(composed)

    requested = staticmethod(precise)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TERMS]


def _twirled(shell: GraphInputs, frame: FrameSpace) -> Expr:
    """Offset turning points about the center, the turn falling smoothly to zero at the radius and lobed by the arms."""
    read = partial(shell.input, _TWIRL)
    arm = frame.point - vector("MULTIPLY", read(_CENTER), frame.extent)
    falling = smoothstep(math("SUBTRACT", 1.0, math("POWER", vector("LENGTH", arm) / read(_REACH), 1.0 / read(_FALLOFF)), clamp=True))
    lobed = math("COSINE", read(_ARMS) * (math("ARCTAN2", arm["Y"], arm["X"]) - read(_ARM_OFFSET))) * 0.5 + 0.5
    return arm - rotated(arm, -read(_TURN) * falling * lobed)


def _waved(record: Displacement, shell: GraphInputs, frame: FrameSpace) -> Expr:
    """Offsets of every superposed crest along its front's displacement axis at the held clock, one Menu Switch choosing profile and one choosing front."""
    read, varied, count = partial(shell.input, _WAVE), partial(shell.input, _WAVE.panel(_VARIATION.key)), record.waves
    arm = frame.point - vector("MULTIPLY", read(_CENTER), frame.extent)
    travel = vector("LENGTH", arm)
    spoke = vector("NORMALIZE", arm)
    tangent = xyz(-spoke["Y"], spoke["X"])
    envelope = math("MAXIMUM", 1.0 - travel / read(_FRONT_REACH), 0.0)
    shift = read(_PHASE) / tau - clocked(shell.input, _WAVE.panel(_WAVE_CLOCK.key))

    def crest(index: int) -> tuple[Expr, Expr, Expr, Expr]:
        draw = hashed(read(SEED), float(stream(_WAVE.key)), 0.0, float(index))
        wavelength = read(_WAVELENGTH) * (1.0 - varied(_WAVELENGTH_VARIATION) * draw["Red"])
        amplitude = read(_AMPLITUDE) * (1.0 - varied(_AMPLITUDE_VARIATION) * draw["Green"]) / float(count)
        heading = read(_DIRECTION) + read(_SPREAD) * ((index - (count - 1.0) / 2.0) / max(count - 1, 1))
        along = xyz(math("COSINE", heading), math("SINE", heading))
        phase = shift + (draw["Blue"] if count > 1 else 0.0)
        linear = vector("DOT_PRODUCT", frame.point - frame.extent * 0.5, along) / wavelength + phase
        return linear, travel / wavelength + phase, amplitude, xyz(-along["Y"], along["X"])

    crests = tuple(map(crest, range(count)))
    first, *rest = (cycles for held in crests for cycles in held[:2])
    waves = profiled(read, (first, *rest))
    swings = tuple((amplitude * (straight * 2.0 - 1.0), amplitude * (round_ * 2.0 - 1.0) * envelope, normal) for straight, round_, (_, _, amplitude, normal) in zip(waves[::2], waves[1::2], crests))
    rounds = reduce(add, (round_ for _, round_, _ in swings))
    fronts = {
        Front.LINEAR: reduce(add, (vector("SCALE", normal, straight) for straight, _, normal in swings)),
        Front.RADIAL: vector("SCALE", spoke, rounds),
        Front.ZIGZAG: vector("SCALE", tangent, rounds),
        Front.POND: vector("SCALE", (spoke + tangent) * 0.5**0.5, rounds),
    }
    return vector("MULTIPLY", shell.menu(_WAVE, _FRONT, cases={MenuItem.of(front): held for front, held in fronts.items()}), read(_AXES))


def _turbulent(record: Displacement, shell: GraphInputs, frame: FrameSpace) -> Expr:
    """Offsets of one evolving noise per axis at the held clock."""
    read = partial(shell.input, _TURBULENCE)
    (across, _), (up, _) = textured(
        shell,
        _TURBULENCE.panel(_BASIS.key),
        record.turbulence,
        frame.point,
        clocked(shell.input, _TURBULENCE.panel(_TURBULENCE_CLOCK.key)),
        read(SEED),
        streams=(stream(Displacement.turbulence, 0), stream(Displacement.turbulence, 1)),
    )
    return vector("MULTIPLY", xyz(across * 2.0 - 1.0, up * 2.0 - 1.0), read(_AXES)) * read(_AMPLITUDE)


def _bulged(shell: GraphInputs, frame: FrameSpace) -> Expr:
    """Offsets moving each point inside the circle along the hemisphere mapping, bulging above zero and pinching below."""
    read = partial(shell.input, _SPHERE)
    amount = read(_AMOUNT)
    arm = vector("MULTIPLY", frame.point - vector("MULTIPLY", read(_CENTER), frame.extent), read(_SHARES))
    radial = vector("LENGTH", arm) / read(_BULGE)
    bounded = math("MINIMUM", radial, 1.0)
    target = switched(amount < 0.0, math("ARCSINE", bounded) * (2.0 / pi), math("SINE", bounded * (pi / 2.0)))
    return vector("SCALE", arm, -abs(amount) * ((target - bounded) / bounded) * (radial < 1.0))


# --- [COMPOSITION] ----------------------------------------------------------------------

_BASIS: Final = bases("basis", Displacement().turbulence, "Noise sampled per displacement axis")
_TURBULENCE: Final = InterfacePanel.of(
    "turbulence", Interface(inputs=(_AMPLITUDE, _AXES, SEED), panels=(_BASIS, _TURBULENCE_CLOCK)), description="Movement along evolving procedural noise", toggle=Some(False)
)
displacement: Final = CompositorEffect(
    record=Displacement,
    interface=Interface(inputs=(LATER, _INTERPOLATION), panels=(_TWIRL, _WAVE, _TURBULENCE, _SPHERE, LUMA, BUBBLES, RAINDROPS, MAP, GLASS)),
    mix=Mix.RESULT,
    derives=(timing,),
    animates=Some(later),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Displacement", "Front", "displacement"]
