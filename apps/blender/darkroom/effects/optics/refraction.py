"""Displacement terms read from heights in the compositor: luminance, bubbles, image maps, glass, and raindrops, each a toggled panel and the offset it adds, heights blurred and differentiated one pixel each way."""

from enum import auto, Enum
from functools import partial
from math import tau
from pathlib import Path
from typing import Final

import bpy
from expression import Some
import PyOpenColorIO

from blender.bpy.data.ids.images import Tag
from blender.bpy.nodes import textures
from blender.bpy.nodes.content import Loaded
from blender.bpy.nodes.desired import MenuItem, MenuItems
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import Expr, math, Scalar, SocketType, switched, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor import filters, masks
from blender.bpy.render.compositor.coordinates import dimensions, FrameSpace, uv_mapped
from blender.bpy.render.compositor.editors import FilePath
from blender.bpy.render.compositor.inputs import GraphInputs
from blender.bpy.render.compositor.motion import CLOCK, Clock, clocked, hashed, origin, SEED
from blender.bpy.render.compositor.shapes import rotated
from blender.imaging.color.processors import GRAY
from blender.imaging.geometry.arithmetic import smoothstep
from blender.imaging.sampling.integration import placement, Population
from blender.imaging.sampling.tiles import stream
from blender.records.faults import Ok, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class MapReading(Enum):
    """Source image reading as a coordinate field."""

    GRADIENT = auto()
    LUMA = auto()
    OFFSETS = auto()
    COORDINATES = auto()


class Surface(Enum):
    """Periodic glass heights and a stochastic frosted height."""

    BLOCKS = auto()
    CANVAS = auto()
    FROSTED = auto()
    TINY_LENS = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_AMPLITUDE: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloat, description="Offset approached by bright content, black moving against it", min_value=0.0, default_value=0.02)
_DIRECTION: Final = Socket(key="direction", kind=bpy.types.NodeSocketFloatAngle, description="Direction bright content moves")
_SOFTNESS: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Height blur radius over greater frame side", min_value=0.0, default_value=0.015)
_SPAN: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Height blur and finite-difference span, no displacement at zero", min_value=0.0, default_value=0.015)
_STRENGTH: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloat, description="Refraction strength", min_value=0.0, default_value=0.02)
_AXES: Final = Socket(key="axes", kind=bpy.types.NodeSocketVector2D, description="Signed offset per axis", default_value=(0.02, 0.02))
_ROTATION: Final = Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Map and displacement axes turn")
_TILE: Final = Socket(key="value", kind=bpy.types.NodeSocketVector2D, description="Map tile size over greater frame side", default_value=(1.0, 1.0))
_TILING: Final = InterfacePanel.of("tile", Interface(inputs=(_TILE,)), description="Map tile size, the map spanning the frame when off", toggle=Some(False))
_SURFACE: Final = Socket(key="surface", kind=bpy.types.NodeSocketMenu, description="Glass height function", default_value=MenuItem.of(Surface.FROSTED).name, items=MenuItems.of(Surface).items)
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
LUMA: Final = InterfacePanel.of("luma", Interface(inputs=(_AMPLITUDE, _DIRECTION, _SOFTNESS)), description="Bright and dark content moving apart", toggle=Some(False))
BUBBLES: Final = InterfacePanel.of("bubbles", Interface(inputs=(_STRENGTH, _SPAN)), description="Refraction through frame luminance height", toggle=Some(False))
MAP: Final = InterfacePanel.of("map", Interface(inputs=(_AXES, _ROTATION, _SOFTNESS), panels=(_TILING,)), description="Image-driven offsets or absolute coordinates", toggle=Some(False))
GLASS: Final = InterfacePanel.of("refraction", Interface(inputs=(_SURFACE, _SIZE, _SLOPE, _SOFTNESS, _GLASS_BLUR, SEED)), description="Refraction through glass height", toggle=Some(False))
RAINDROPS: Final = InterfacePanel.of(
    "raindrops",
    Interface(inputs=(_INVERSION, _DROP_BLUR, _SLANT, _TRAILS, SEED, _TRAIL_SLOPE, _TRAIL_SOFTNESS), panels=(_DROPS, _RAIN_CLOCK)),
    description="Drops on lens showing scene inverted",
    toggle=Some(False),
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [HEIGHTS]


def lifted(luminance: Scalar, gain: Expr) -> Expr:
    """Height in [0, 1) of scene-linear luminance under the keyed gain of later exposure, middle gray at one half."""
    positive = math("MAXIMUM", luminance, 0.0)
    return positive / (positive + GRAY / gain)


_softened: Final = partial(filters.blurred, socket_type=bpy.types.NodeSocketFloat)


def _difference(image: Expr, frame: FrameSpace, span: Scalar) -> Expr:
    """Central difference of a height image along each axis, `span` each way."""

    def sampled(point: Expr) -> Expr:
        return uv_mapped(image, frame.sampled(point / frame.extent), extension=("Extend", "Extend"), socket_type=bpy.types.NodeSocketFloat)

    return xyz(*(sampled(frame.point + shift) - sampled(frame.point - shift) for shift in (xyz(span), xyz(0.0, span))))


def _slope(image: Expr, frame: FrameSpace) -> Expr:
    """Central-difference gradient of a height image per greater-side unit, one pixel each way."""
    return vector("SCALE", _difference(image, frame, 1.0 / frame.greater), frame.greater * 0.5)


# --- [TERMS]


def luminous(shell: GraphInputs, frame: FrameSpace, height: Expr) -> Expr:
    """Offset moving bright content toward the luma direction and dark content against it."""
    read = partial(shell.input, LUMA)
    angle = read(_DIRECTION)
    return vector("SCALE", xyz(math("COSINE", angle), math("SINE", angle)), (_softened(height, frame.pixel_size(read(_SOFTNESS))) * 2.0 - 1.0) * read(_AMPLITUDE))


def bubbled(shell: GraphInputs, frame: FrameSpace, height: Expr) -> Expr:
    """Offset down the blurred luminance height, differences taken one softness each way."""
    read = partial(shell.input, BUBBLES)
    span = read(_SPAN)
    return vector("SCALE", _difference(_softened(height, frame.pixel_size(span)), frame, span), -read(_STRENGTH))


def mapped(shell: GraphInputs, frame: FrameSpace, source: FilePath | None, reading: MapReading, gain: Expr) -> Outcome[tuple[Expr, Scalar]]:
    """Returns the map offset and its coverage: a file or the entry input read as a gradient, luma, offset, or coordinate field, spanning the frame or tiled, turned with the map axes.

    Args:
        shell: Working config and entry inputs.
        frame: Camera frame of the entry input.
        source: Map file, the entry input without one.
        reading: Map channel meaning, deciding the file color tag.
        gain: Keyed gain of later exposure entries.

    Returns:
        Offset per greater-side unit and the coordinate coverage, whole outside coordinate readings, or the working luminance fault.
    """
    read = partial(shell.input, MAP)
    turn, tiled, step = read(_ROTATION), read(_TILING), 1.0 / frame.greater
    data = reading in {MapReading.OFFSETS, MapReading.COORDINATES}
    plane = (
        Expr(
            kind=bpy.types.CompositorNodeImage,
            content=Some(Loaded(path=Path(source), tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)) if data else Tag())),
            socket="Image",
            socket_type=bpy.types.NodeSocketColor,
        )
        if source is not None
        else shell.color
    )

    def at(image: Expr, point: Expr, socket_type: SocketType = bpy.types.NodeSocketColor) -> Expr:
        local = rotated(point, turn, inverted=True)
        fraction = switched(tiled, local / frame.extent, vector("FRACTION", local / read(_TILING, _TILE)))
        return uv_mapped(image, fraction if source is not None else frame.sampled(fraction), extension=("Extend", "Extend"), socket_type=socket_type)

    def turned(field: Expr) -> tuple[Expr, Scalar]:
        return rotated(vector("MULTIPLY", vector("MULTIPLY", field, read(_AXES)), frame.extent), turn), 1.0

    def leveled(luminance: Expr) -> tuple[Expr, Scalar]:
        size = vector("MULTIPLY", dimensions(plane), read(_SOFTNESS) / switched(tiled, frame.extent, read(_TILING, _TILE)))
        level = _softened(lifted(luminance, gain), size)
        match reading:
            case MapReading.GRADIENT:
                return turned(
                    xyz(
                        *(
                            (at(level, frame.point + shift, bpy.types.NodeSocketFloat) - at(level, frame.point - shift, bpy.types.NodeSocketFloat)) / (step * 2.0)
                            for shift in (xyz(step), xyz(0.0, step))
                        )
                    )
                )
            case _:
                held = at(level, frame.point, bpy.types.NodeSocketFloat) * 2.0 - 1.0
                return turned(xyz(held, held))

    texel = at(plane, frame.point)
    match reading:
        case MapReading.OFFSETS:
            return Ok(turned(xyz(texel["Red"] * 2.0 - 1.0, texel["Green"] * 2.0 - 1.0)))
        case MapReading.COORDINATES:
            return Ok((frame.point - rotated(vector("MULTIPLY", xyz(texel["Red"], texel["Green"]), frame.extent), turn), texel["Blue"]))
        case MapReading.GRADIENT | MapReading.LUMA:
            return masks.luminance(shell.config, plane).map(leveled)


def refracted(shell: GraphInputs, frame: FrameSpace) -> tuple[Expr, Expr]:
    """Returns the offset down the blurred glass height of the chosen surface, scaled by strength over cell size, and the entry input blurred behind the glass."""
    read = partial(shell.input, GLASS)
    size = read(_SIZE)
    cell = frame.point / size
    fraction = vector("FRACTION", cell)
    centered = xyz(fraction["X"] - 0.5, fraction["Y"] - 0.5)
    frosted = textures.Noise(noise_dimensions="2D", scale=1.0).node(cell + origin(read(SEED), float(stream(GLASS.key))))
    heights = {
        MenuItem.of(Surface.BLOCKS): (1.0 - abs(fraction["X"] * 2.0 - 1.0) ** 4.0) * (1.0 - abs(fraction["Y"] * 2.0 - 1.0) ** 4.0),
        MenuItem.of(Surface.CANVAS): math("SINE", cell["X"] * tau) * math("SINE", cell["Y"] * tau) * 0.5 + 0.5,
        MenuItem.of(Surface.FROSTED): frosted,
        MenuItem.of(Surface.TINY_LENS): math("SQRT", math("MAXIMUM", 1.0 - vector("DOT_PRODUCT", centered, centered) * 4.0, 0.0)),
    }
    level = _softened(shell.menu(GLASS, _SURFACE, cases=heights), frame.pixel_size(read(_SOFTNESS)))
    return vector("SCALE", _slope(level, frame), read(_SLOPE) * size), filters.blurred(shell.color, frame.pixel_size(read(_GLASS_BLUR)))


def rained(shell: GraphInputs, frame: FrameSpace) -> tuple[Expr, Expr, Expr]:
    """Returns drop coverage, the offset showing each drop's inverted view with trails refracting through their blurred height, and the view blurred inside drops.

    Args:
        shell: Entry inputs.
        frame: Camera frame of the entry input.

    Returns:
        Coverage of drops and trails, the offset inside them per greater-side unit, and the entry input blurred inside them.
    """
    read = partial(shell.input, RAINDROPS)

    def drops(field: object) -> Expr:
        return read(_DROPS.socket(field))

    spacing, softness, turn = drops(Population.spacing), drops(Population.softness), read(_SLANT)
    local = rotated(frame.point, turn) + xyz(0.0, clocked(shell.input, RAINDROPS.panel(_RAIN_CLOCK.key)) * spacing)
    cell = vector("FLOOR", local / spacing)
    first, second = (hashed(read(SEED), float(stream(RAINDROPS.key, index)), cell["X"], cell["Y"]) for index in range(2))
    draws = (first["Red"], first["Green"], first["Blue"], second["Red"], second["Green"], second["Blue"])
    x, y, radius, gain = placement(
        (cell["X"], cell["Y"]), draws, EXPRESSIONS.exp2, spacing=spacing, diameter=spacing * drops(Population.size), variation=drops(Population.variation), spread=drops(Population.spread)
    )
    present = draws[0] < drops(Population.density)
    arm = local - xyz(x, y)

    def edged(distance: Expr) -> Expr:
        return switched(softness, distance < 1.0, smoothstep(math("DIVIDE", 1.0 - distance, softness, clamp=True)))

    drop = edged(vector("LENGTH", arm) / radius)
    lag = read(_TRAILS) * switched(arm["Y"] > 0.0, math("MAXIMUM", 1.0 + arm["Y"] / (y - cell["Y"] * spacing), 0.0), 0.0)
    across = abs(arm["X"]) / (radius * 0.5)
    trail = lag * (1.0 - drop) * math("SQRT", math("MAXIMUM", 1.0 - across * across, 0.0)) * present
    inversion = vector("SCALE", rotated(arm, turn, inverted=True), (1.0 + read(_INVERSION)) * (drop > 0.0) * present)
    gradient = vector("SCALE", _slope(_softened(trail, frame.pixel_size(read(_TRAIL_SOFTNESS))), frame), read(_TRAIL_SLOPE) * spacing)
    return math("MAXIMUM", drop, lag * edged(across)) * gain * present, inversion + gradient, filters.blurred(shell.color, frame.pixel_size(read(_DROP_BLUR)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BUBBLES", "GLASS", "LUMA", "MAP", "RAINDROPS", "MapReading", "Surface", "bubbled", "lifted", "luminous", "mapped", "rained", "refracted"]
