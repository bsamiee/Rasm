"""Displacement fields composed into one warp in the compositor and sampled with anisotropic filtering: twirl, waves, turbulence, sphere, luminance, bubbles, image maps, glass, and raindrops, heights blurred and differentiated one pixel each way."""

from enum import auto, Enum
from functools import partial, reduce
from math import pi, tau
from operator import add
from pathlib import Path
from typing import Annotated, Final, override

import bpy
from expression import Some
import msgspec
import PyOpenColorIO

from rasm_bpy.data import Tag
from rasm_bpy.nodes import Expr, EXPRESSIONS, Interface, InterfacePanel, Loaded, math, MenuItem, MenuItems, mixed, Scalar, Socket, StockInput, switched, textures, vector, xyz
from rasm_bpy.render import (
    blurred,
    Border,
    CLOCK,
    Clock,
    clocked,
    CompositorEffect,
    CompositorRecord,
    dimensions,
    extended,
    FilePath,
    FrameSpace,
    GraphInputs,
    hashed,
    luminance,
    Mix,
    origin,
    precise,
    PROFILE,
    profiled,
    rotated,
    SEED,
    timing,
    uv_mapped,
    with_alpha,
)
from rasm_imaging import GRAY, placement, Population, smoothstep, stream
from rasm_records import Ok, Outcome, paired

from ..grade.exposure import LATER, later
from .bases import bases, Basis, Perlin, textured

# --- [TYPES] ----------------------------------------------------------------------------


class _Front(Enum):
    """Traveling bands or circular waves with radial and tangential motion."""

    LINEAR = auto()
    RADIAL = auto()
    ZIGZAG = auto()
    POND = auto()


class MapReading(Enum):
    """Source image reading as a coordinate field."""

    GRADIENT = auto()
    LUMA = auto()
    OFFSETS = auto()
    COORDINATES = auto()


class _Surface(Enum):
    """Periodic glass heights and a stochastic frosted height."""

    BLOCKS = auto()
    CANVAS = auto()
    FROSTED = auto()
    TINY_LENS = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Point the field centers on in frame fractions", default_value=(0.5, 0.5))
_TURN: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Counterclockwise turn at center", default_value=6.1)
_REACH: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Distance where the turn reaches zero over greater frame side", min_value=0.0, default_value=0.5)
_FALLOFF: Final = Socket(key="falloff", kind=bpy.types.NodeSocketFloat, description="Radial falloff power", min_value=0.0, default_value=1.0)
_ARMS: Final = Socket(key="arms", kind=bpy.types.NodeSocketInt, description="Angular turn lobes, uniform at zero", min_value=0)
_ARM_OFFSET: Final = Socket(key="arm_offset", kind=bpy.types.NodeSocketFloatAngle, description="Angular lobe turn")
_FRONT: Final = Socket(
    key="front", kind=bpy.types.NodeSocketMenu, description="Wave front shape and displacement direction", default_value=MenuItem.of(_Front.LINEAR).name, items=MenuItems.of(_Front).items
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
_LUMA_AMPLITUDE: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloat, description="Offset approached by bright content, black moving against it", min_value=0.0, default_value=0.02)
_LUMA_DIRECTION: Final = Socket(key="direction", kind=bpy.types.NodeSocketFloatAngle, description="Direction bright content moves")
_SOFTNESS: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Height blur radius over greater frame side", min_value=0.0, default_value=0.015)
_SPAN: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Height blur and finite-difference span, no displacement at zero", min_value=0.0, default_value=0.015)
_STRENGTH: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloat, description="Refraction strength", min_value=0.0, default_value=0.02)
_MAP_AXES: Final = Socket(key="axes", kind=bpy.types.NodeSocketVector2D, description="Signed offset per axis", default_value=(0.02, 0.02))
_ROTATION: Final = Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Map and displacement axes turn")
_TILE: Final = Socket(key="value", kind=bpy.types.NodeSocketVector2D, description="Map tile size over greater frame side", default_value=(1.0, 1.0))
_TILING: Final = InterfacePanel.of("tile", Interface(inputs=(_TILE,)), description="Map tile size, the map spanning the frame when off", toggle=Some(False))
_SURFACE: Final = Socket(key="surface", kind=bpy.types.NodeSocketMenu, description="Glass height function", default_value=MenuItem.of(_Surface.FROSTED).name, items=MenuItems.of(_Surface).items)
_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketFloat, description="Glass cell width over greater frame side", min_value=0.0, default_value=0.1)
_SLOPE: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloat, description="Signed displacement per unit slope in glass-cell coordinates", default_value=0.02)
_GLASS_BLUR: Final = Socket(key="blur", kind=bpy.types.NodeSocketFloat, description="Refracted image blur radius over greater frame side", min_value=0.0)
_INVERSION: Final = Socket(key="refraction", kind=bpy.types.NodeSocketFloat, description="Scene radii each drop shows inverted", min_value=0.0, default_value=2.0)
_DROP_BLUR: Final = Socket(key="blur", kind=bpy.types.NodeSocketFloat, description="View blur radius inside each drop over greater frame side", min_value=0.0, default_value=0.0076)
_SLANT: Final = Socket(key="direction", kind=bpy.types.NodeSocketFloatAngle, description="Drop fall slant from straight down")
_TRAILS: Final = Socket(key="trails", kind=bpy.types.NodeSocketFloatFactor, description="Trail height behind each drop")
_TRAIL_SLOPE: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloat, description="Signed refraction through trail height")
_TRAIL_SOFTNESS: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Trail height blur radius over greater frame side", min_value=0.0, default_value=0.015)
_DROPS: Final = InterfacePanel.of("drops", Population(spacing=1.0 / (2.0 * 3.8), size=0.5, spread=0.0, softness=0.05), description="Drops placed one per cell at most")
_RAIN_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=0.010008 * 2.0 * 3.8))
_LUMA: Final = InterfacePanel.of("luma", Interface(inputs=(_LUMA_AMPLITUDE, _LUMA_DIRECTION, _SOFTNESS)), description="Bright and dark content moving apart", toggle=Some(False))
_BUBBLES: Final = InterfacePanel.of("bubbles", Interface(inputs=(_STRENGTH, _SPAN)), description="Refraction through frame luminance height", toggle=Some(False))
_MAP: Final = InterfacePanel.of("map", Interface(inputs=(_MAP_AXES, _ROTATION, _SOFTNESS), panels=(_TILING,)), description="Image-driven offsets or absolute coordinates", toggle=Some(False))
_GLASS: Final = InterfacePanel.of("refraction", Interface(inputs=(_SURFACE, _SIZE, _SLOPE, _SOFTNESS, _GLASS_BLUR, SEED)), description="Refraction through glass height", toggle=Some(False))
_RAINDROPS: Final = InterfacePanel.of(
    "raindrops",
    Interface(inputs=(_INVERSION, _DROP_BLUR, _SLANT, _TRAILS, SEED, _TRAIL_SLOPE, _TRAIL_SOFTNESS), panels=(_DROPS, _RAIN_CLOCK)),
    description="Drops on lens showing scene inverted",
    toggle=Some(False),
)

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

        twirl = partial(shell.input, _TWIRL)
        arm = frame.point - vector("MULTIPLY", twirl(_CENTER), frame.extent)
        falling = smoothstep(math("SUBTRACT", 1.0, math("POWER", vector("LENGTH", arm) / twirl(_REACH), 1.0 / twirl(_FALLOFF)), clamp=True))
        lobed = math("COSINE", twirl(_ARMS) * (math("ARCTAN2", arm["Y"], arm["X"]) - twirl(_ARM_OFFSET))) * 0.5 + 0.5
        twirled = arm - rotated(arm, -twirl(_TURN) * falling * lobed)

        wave, varied, count = partial(shell.input, _WAVE), partial(shell.input, _WAVE.panel(_VARIATION.key)), self.waves
        outward = frame.point - vector("MULTIPLY", wave(_CENTER), frame.extent)
        travel = vector("LENGTH", outward)
        spoke = vector("NORMALIZE", outward)
        tangent = xyz(-spoke["Y"], spoke["X"])
        envelope = math("MAXIMUM", 1.0 - travel / wave(_FRONT_REACH), 0.0)
        shift = wave(_PHASE) / tau - clocked(shell.input, _WAVE.panel(_WAVE_CLOCK.key))

        def crest(index: int) -> tuple[Expr, Expr, Expr, Expr]:
            draw = hashed(wave(SEED), float(stream(_WAVE.key)), 0.0, float(index))
            wavelength = wave(_WAVELENGTH) * (1.0 - varied(_WAVELENGTH_VARIATION) * draw["Red"])
            amplitude = wave(_AMPLITUDE) * (1.0 - varied(_AMPLITUDE_VARIATION) * draw["Green"]) / float(count)
            heading = wave(_DIRECTION) + wave(_SPREAD) * ((index - (count - 1.0) / 2.0) / max(count - 1, 1))
            along = xyz(math("COSINE", heading), math("SINE", heading))
            phase = shift + (draw["Blue"] if count > 1 else 0.0)
            linear = vector("DOT_PRODUCT", frame.point - frame.extent * 0.5, along) / wavelength + phase
            return linear, travel / wavelength + phase, amplitude, xyz(-along["Y"], along["X"])

        crests = tuple(map(crest, range(count)))
        first, *rest = (cycles for held in crests for cycles in held[:2])
        profiles = profiled(wave, (first, *rest))
        swings = tuple(
            (amplitude * (straight * 2.0 - 1.0), amplitude * (round_ * 2.0 - 1.0) * envelope, normal) for straight, round_, (_, _, amplitude, normal) in zip(profiles[::2], profiles[1::2], crests)
        )
        rounds = reduce(add, (round_ for _, round_, _ in swings))
        fronts = {
            _Front.LINEAR: reduce(add, (vector("SCALE", normal, straight) for straight, _, normal in swings)),
            _Front.RADIAL: vector("SCALE", spoke, rounds),
            _Front.ZIGZAG: vector("SCALE", tangent, rounds),
            _Front.POND: vector("SCALE", (spoke + tangent) * 0.5**0.5, rounds),
        }
        waved = vector("MULTIPLY", shell.menu(_WAVE, _FRONT, cases={MenuItem.of(front): held for front, held in fronts.items()}), wave(_AXES))

        turbulence = partial(shell.input, _TURBULENCE)
        (across, _), (up, _) = textured(
            shell,
            _TURBULENCE.panel(_BASIS.key),
            self.turbulence,
            frame.point,
            clocked(shell.input, _TURBULENCE.panel(_TURBULENCE_CLOCK.key)),
            turbulence(SEED),
            streams=(stream(Displacement.turbulence, 0), stream(Displacement.turbulence, 1)),
        )
        turbulent = vector("MULTIPLY", xyz(across * 2.0 - 1.0, up * 2.0 - 1.0), turbulence(_AXES)) * turbulence(_AMPLITUDE)

        sphere = partial(shell.input, _SPHERE)
        amount = sphere(_AMOUNT)
        bulge = vector("MULTIPLY", frame.point - vector("MULTIPLY", sphere(_CENTER), frame.extent), sphere(_SHARES))
        radial = vector("LENGTH", bulge) / sphere(_BULGE)
        bounded = math("MINIMUM", radial, 1.0)
        target = switched(amount < 0.0, math("ARCSINE", bounded) * (2.0 / pi), math("SINE", bounded * (pi / 2.0)))
        bulged = vector("SCALE", bulge, -abs(amount) * ((target - bounded) / bounded) * (radial < 1.0))

        glass = partial(shell.input, _GLASS)
        size = glass(_SIZE)
        cell = frame.point / size
        fraction = vector("FRACTION", cell)
        centered = xyz(fraction["X"] - 0.5, fraction["Y"] - 0.5)
        frosted = textures.Noise(noise_dimensions="2D", scale=1.0).node(cell + origin(glass(SEED), float(stream(_GLASS.key))))
        heights = {
            MenuItem.of(_Surface.BLOCKS): (1.0 - abs(fraction["X"] * 2.0 - 1.0) ** 4.0) * (1.0 - abs(fraction["Y"] * 2.0 - 1.0) ** 4.0),
            MenuItem.of(_Surface.CANVAS): math("SINE", cell["X"] * tau) * math("SINE", cell["Y"] * tau) * 0.5 + 0.5,
            MenuItem.of(_Surface.FROSTED): frosted,
            MenuItem.of(_Surface.TINY_LENS): math("SQRT", math("MAXIMUM", 1.0 - vector("DOT_PRODUCT", centered, centered) * 4.0, 0.0)),
        }
        level = _softened(shell.menu(_GLASS, _SURFACE, cases=heights), frame.pixel_size(glass(_SOFTNESS)))
        glass_offset, glass_source = vector("SCALE", _slope(level, frame), glass(_SLOPE) * size), blurred(color, frame.pixel_size(glass(_GLASS_BLUR)))

        rain = partial(shell.input, _RAINDROPS)

        def drops(field: object) -> Expr:
            return rain(_DROPS.socket(field))

        spacing, softness, slant = drops(Population.spacing), drops(Population.softness), rain(_SLANT)
        falling_point = rotated(frame.point, slant) + xyz(0.0, clocked(shell.input, _RAINDROPS.panel(_RAIN_CLOCK.key)) * spacing)
        site = vector("FLOOR", falling_point / spacing)
        primary, secondary = (hashed(rain(SEED), float(stream(_RAINDROPS.key, index)), site["X"], site["Y"]) for index in range(2))
        draws = (primary["Red"], primary["Green"], primary["Blue"], secondary["Red"], secondary["Green"], secondary["Blue"])
        x, y, drop_radius, gain = placement(
            (site["X"], site["Y"]), draws, EXPRESSIONS.exp2, spacing=spacing, diameter=spacing * drops(Population.size), variation=drops(Population.variation), spread=drops(Population.spread)
        )
        present = draws[0] < drops(Population.density)
        from_drop = falling_point - xyz(x, y)

        def edged(distance: Expr) -> Expr:
            return switched(softness, distance < 1.0, smoothstep(math("DIVIDE", 1.0 - distance, softness, clamp=True)))

        drop = edged(vector("LENGTH", from_drop) / drop_radius)
        lag = rain(_TRAILS) * switched(from_drop["Y"] > 0.0, math("MAXIMUM", 1.0 + from_drop["Y"] / (y - site["Y"] * spacing), 0.0), 0.0)
        sideways = abs(from_drop["X"]) / (drop_radius * 0.5)
        trail = lag * (1.0 - drop) * math("SQRT", math("MAXIMUM", 1.0 - sideways * sideways, 0.0)) * present
        inversion = vector("SCALE", rotated(from_drop, slant, inverted=True), (1.0 + rain(_INVERSION)) * (drop > 0.0) * present)
        trail_slope = vector("SCALE", _slope(_softened(trail, frame.pixel_size(rain(_TRAIL_SOFTNESS))), frame), rain(_TRAIL_SLOPE) * spacing)
        drop_coverage, inside, drop_source = math("MAXIMUM", drop, lag * edged(sideways)) * gain * present, inversion + trail_slope, blurred(color, frame.pixel_size(rain(_DROP_BLUR)))

        mapping = partial(shell.input, _MAP)
        turn, tiled, step = mapping(_ROTATION), mapping(_TILING), 1.0 / frame.greater
        plane = (
            Expr(
                kind=bpy.types.CompositorNodeImage,
                content=Some(Loaded(path=Path(self.map_source), tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)) if self.map_reading in {MapReading.OFFSETS, MapReading.COORDINATES} else Tag())),
                socket="Image",
                socket_type=bpy.types.NodeSocketColor,
            )
            if self.map_source is not None
            else color
        )

        def at(image: Expr, point: Expr, socket_type: type[bpy.types.NodeSocket] = bpy.types.NodeSocketColor) -> Expr:
            local = rotated(point, turn, inverted=True)
            tile = switched(tiled, local / frame.extent, vector("FRACTION", local / mapping(_TILING, _TILE)))
            return uv_mapped(image, tile if self.map_source is not None else frame.sampled(tile), extension=("Extend", "Extend"), socket_type=socket_type)

        def turned(field: Expr) -> tuple[Expr, Scalar]:
            return rotated(vector("MULTIPLY", vector("MULTIPLY", field, mapping(_MAP_AXES)), frame.extent), turn), 1.0

        def leveled(luminance: Expr) -> tuple[Expr, Scalar]:
            blur = vector("MULTIPLY", dimensions(plane), mapping(_SOFTNESS) / switched(tiled, frame.extent, mapping(_TILING, _TILE)))
            lifted = _softened(_lifted(luminance, shell.input(LATER)), blur)
            match self.map_reading:
                case MapReading.GRADIENT:
                    return turned(
                        xyz(
                            *(
                                (at(lifted, frame.point + delta, bpy.types.NodeSocketFloat) - at(lifted, frame.point - delta, bpy.types.NodeSocketFloat)) / (step * 2.0)
                                for delta in (xyz(step), xyz(0.0, step))
                            )
                        )
                    )
                case _:
                    held = at(lifted, frame.point, bpy.types.NodeSocketFloat) * 2.0 - 1.0
                    return turned(xyz(held, held))

        texel = at(plane, frame.point)
        mapped: Outcome[tuple[Expr, Scalar]]
        match self.map_reading:
            case MapReading.OFFSETS:
                mapped = Ok(turned(xyz(texel["Red"] * 2.0 - 1.0, texel["Green"] * 2.0 - 1.0)))
            case MapReading.COORDINATES:
                mapped = Ok((frame.point - rotated(vector("MULTIPLY", xyz(texel["Red"], texel["Green"]), frame.extent), turn), texel["Blue"]))
            case MapReading.GRADIENT | MapReading.LUMA:
                mapped = luminance(shell.config, plane).map(leveled)

        def composed(parts: tuple[Expr, tuple[Expr, Scalar]]) -> Expr:
            luminance, (map_offset, map_coverage) = parts
            height = _lifted(luminance, shell.input(LATER))
            luma, bubbles = partial(shell.input, _LUMA), partial(shell.input, _BUBBLES)
            angle, span = luma(_LUMA_DIRECTION), bubbles(_SPAN)
            terms = (
                (_TWIRL, twirled),
                (_WAVE, waved),
                (_TURBULENCE, turbulent),
                (_SPHERE, bulged),
                (_LUMA, vector("SCALE", xyz(math("COSINE", angle), math("SINE", angle)), (_softened(height, frame.pixel_size(luma(_SOFTNESS))) * 2.0 - 1.0) * luma(_LUMA_AMPLITUDE))),
                (_BUBBLES, vector("SCALE", _difference(_softened(height, frame.pixel_size(span)), frame, span), -bubbles(_STRENGTH))),
                (_GLASS, glass_offset),
                (_MAP, map_offset),
            )
            offset = reduce(add, (switched(shell.input(panel), (0.0, 0.0, 0.0), term) for panel, term in terms))
            coverage = switched(shell.input(_MAP), 1.0, map_coverage)
            seen = sample(switched(shell.input(_GLASS), color, glass_source), frame.point - offset, coverage)
            dropped = sample(drop_source, frame.point - offset - inside, coverage)
            return switched(shell.input(_RAINDROPS), seen, with_alpha(mixed(drop_coverage, seen, dropped), mixed(drop_coverage, seen["Alpha"], dropped["Alpha"])))

        return paired(luminance(shell.config, color), mapped).map(composed)

    requested = staticmethod(precise)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _lifted(luminance: Scalar, gain: Expr) -> Expr:
    """Height in [0, 1) of scene-linear luminance under the keyed gain of later exposure, middle gray at one half."""
    positive = math("MAXIMUM", luminance, 0.0)
    return positive / (positive + GRAY / gain)


_softened: Final = partial(blurred, socket_type=bpy.types.NodeSocketFloat)


def _difference(image: Expr, frame: FrameSpace, span: Scalar) -> Expr:
    """Central difference of a height image along each axis, `span` each way."""

    def sampled(point: Expr) -> Expr:
        return uv_mapped(image, frame.sampled(point / frame.extent), extension=("Extend", "Extend"), socket_type=bpy.types.NodeSocketFloat)

    return xyz(*(sampled(frame.point + shift) - sampled(frame.point - shift) for shift in (xyz(span), xyz(0.0, span))))


def _slope(image: Expr, frame: FrameSpace) -> Expr:
    """Central-difference gradient of a height image per greater-side unit, one pixel each way."""
    return vector("SCALE", _difference(image, frame, 1.0 / frame.greater), frame.greater * 0.5)


# --- [COMPOSITION] ----------------------------------------------------------------------

_BASIS: Final = bases("basis", Displacement().turbulence, "Noise sampled per displacement axis")
_TURBULENCE: Final = InterfacePanel.of(
    "turbulence", Interface(inputs=(_AMPLITUDE, _AXES, SEED), panels=(_BASIS, _TURBULENCE_CLOCK)), description="Movement along evolving procedural noise", toggle=Some(False)
)
displacement: Final = CompositorEffect(
    record=Displacement,
    interface=Interface(inputs=(LATER, _INTERPOLATION), panels=(_TWIRL, _WAVE, _TURBULENCE, _SPHERE, _LUMA, _BUBBLES, _RAINDROPS, _MAP, _GLASS)),
    mix=Mix.RESULT,
    derives=(timing,),
    animates=Some(later),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Displacement", "MapReading", "displacement"]
