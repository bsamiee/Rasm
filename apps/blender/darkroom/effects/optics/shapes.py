"""Distance-field regions painted over the frame in solid, gradient, corner, or cell colors, with their outline and light."""

from collections.abc import Callable
from enum import auto, Enum
from functools import partial
from math import tau
from typing import Annotated, Final

import bpy
from expression import Nothing, Some
import msgspec

from blender.bpy.nodes import textures
from blender.bpy.nodes.content import Ramp
from blender.bpy.nodes.desired import item_key, MenuItem, MenuItems
from blender.bpy.nodes.graphs import Expr, mixed, Scalar, switched, Term, vector
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.coordinates import Border, BORDERS, folded, FrameSpace
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.inputs import Shell, Step
from blender.bpy.render.compositor.motion import CLOCK, Clock, clocked, hashed, SEED, timing
from blender.bpy.render.compositor.shapes import angular, Bounded, coverage, diamond, linear, PROFILE, profiled, radial, rectangular, turned
from blender.imaging.sampling.tiles import stream
from blender.records.faults import Outcome
from blender.records.fields import Case

from .bases import bases, Basis, Perlin, textured
from .emission import Emission, emission, emitted, Tint
from .figures import bolt, BOOLEAN, FIGURE, figured, GRID, gridded, joined, LIGHTNING, MORPH, morphed, WINDOW, windowed

# --- [TYPES] ----------------------------------------------------------------------------

type _Geometry = Callable[[Expr, Callable[[Socket], Expr]], Expr]


class _CellPattern(Enum):
    """Alternating or seeded colors of lattice cells."""

    CHECKER = auto()
    RANDOM = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [GEOMETRIES]

_ORIGIN: Final = Socket(key="origin", kind=bpy.types.NodeSocketVector2D, description="Point the parameter grows from in frame fractions", default_value=(0.5, 0.5))
_TURN: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Turn of the parameter's axes")
_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Distance from first stop to last over greater frame side", min_value=0.0, default_value=1.0)
_SWEEP: Final = Socket(key="sweep", kind=bpy.types.NodeSocketFloat, description="Chevron slope, 0 straight and 1 at 45 degrees", min_value=0.0)
_REACH: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Distance to last stop over greater frame side", min_value=0.0, default_value=0.5)
_ASPECT: Final = Socket(key="aspect", kind=bpy.types.NodeSocketFloat, description="Horizontal radius over vertical radius", min_value=0.0, default_value=1.0)
_HALF_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Horizontal distance to last stop", min_value=0.0, default_value=0.5)
_HALF_HEIGHT: Final = Socket(key="height", kind=bpy.types.NodeSocketFloat, description="Vertical distance to last stop", min_value=0.0, default_value=0.5)
_LINEAR: Final = InterfacePanel.of(
    "linear", Interface(inputs=(_ORIGIN, _TURN, _LENGTH, _SWEEP)).defaulted({_ORIGIN.key: (0.0, 0.5)}), description="Bands across a direction from an origin, swept back into chevrons"
)
_RADIAL: Final = InterfacePanel.of("radial", Interface(inputs=(_ORIGIN, _TURN, _REACH, _ASPECT)), description="Rings about an origin")
_ANGULAR: Final = InterfacePanel.of("angular", Interface(inputs=(_ORIGIN, _TURN)), description="Sweep around an origin")
_DIAMOND: Final = InterfacePanel.of("diamond", Interface(inputs=(_ORIGIN, _TURN, _REACH)), description="Square rings about an origin")
_RECTANGULAR: Final = InterfacePanel.of("rectangular", Interface(inputs=(_ORIGIN, _TURN, _HALF_WIDTH, _HALF_HEIGHT)), description="Rectangular rings with independent half extents")
_GEOMETRIES: Final = Interface(panels=(_LINEAR, _RADIAL, _ANGULAR, _DIAMOND, _RECTANGULAR))

# --- [PAINTS]

_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear fill color", default_value=(1.0, 1.0, 1.0, 1.0))
_GEOMETRY: Final = Socket(key="geometry", kind=bpy.types.NodeSocketMenu, description="Band shape", default_value=_LINEAR.title, items=Some(_GEOMETRIES.menu))
_EXTENSION: Final = Socket(key="extension", kind=bpy.types.NodeSocketMenu, description="Parameter past ramp ends", default_value=MenuItem.of(Border.CLAMP).name, items=Some(BORDERS))
_PHASE: Final = Socket(key="phase", kind=bpy.types.NodeSocketFloatAngle, description="Profile phase offset")
_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=0.0))
_WAVE: Final = InterfacePanel.of("wave", Interface(inputs=(*PROFILE.inputs, _PHASE), panels=(_CLOCK,)), description="Periodic profile of the parameter traveling with the clock", toggle=Some(False))
_TOP_LEFT: Final = Socket(key="top_left", kind=bpy.types.NodeSocketColor, description="Scene-linear top left color", default_value=(1.0, 1.0, 1.0, 1.0))
_TOP_RIGHT: Final = Socket(key="top_right", kind=bpy.types.NodeSocketColor, description="Scene-linear top right color", default_value=(0.0, 0.0, 0.0, 1.0))
_BOTTOM_RIGHT: Final = Socket(key="bottom_right", kind=bpy.types.NodeSocketColor, description="Scene-linear bottom right color", default_value=(1.0, 1.0, 1.0, 1.0))
_BOTTOM_LEFT: Final = Socket(key="bottom_left", kind=bpy.types.NodeSocketColor, description="Scene-linear bottom left color", default_value=(0.0, 0.0, 0.0, 1.0))
_PATTERN: Final = Socket(
    key="pattern",
    kind=bpy.types.NodeSocketMenu,
    description="Cell colors alternating or drawn from the seed",
    default_value=MenuItem.of(_CellPattern.CHECKER).name,
    items=Some(MenuItems.of(_CellPattern)),
)
_FIRST: Final = Socket(key="first", kind=bpy.types.NodeSocketColor, description="Scene-linear first cell color", default_value=(0.0, 0.0, 0.0, 1.0))
_SECOND: Final = Socket(key="second", kind=bpy.types.NodeSocketColor, description="Scene-linear second cell color", default_value=(1.0, 1.0, 1.0, 1.0))
_MORTAR: Final = Socket(key="mortar_color", kind=bpy.types.NodeSocketColor, description="Scene-linear mortar color", default_value=(1.0, 1.0, 1.0, 1.0))
_LATTICE: Final = InterfacePanel.of("grid", GRID, description="Cell lattice, every second course shifted")
_SOLID: Final = InterfacePanel.of("solid", Interface(inputs=(_COLOR,)), description="One scene-linear color")
_GRADIENT: Final = InterfacePanel.of(
    "gradient", Interface(inputs=(_GEOMETRY, _EXTENSION), panels=(*_GEOMETRIES.panels, _WAVE)), description="Colors along a geometric parameter with optional periodic motion"
)
_CORNERS: Final = InterfacePanel.of(
    "corners", Interface(inputs=(_TOP_LEFT, _TOP_RIGHT, _BOTTOM_RIGHT, _BOTTOM_LEFT)), description="Four scene-linear corner colors interpolated across frame with smootherstep"
)
_CELLS: Final = InterfacePanel.of("cells", Interface(inputs=(_PATTERN, _FIRST, _SECOND, _MORTAR, SEED), panels=(_LATTICE,)), description="Alternating or seeded cell colors with separate mortar color")
_PAINTS: Final = Interface(panels=(_SOLID, _GRADIENT, _CORNERS, _CELLS))
_PAINT: Final = Socket(key="paint", kind=bpy.types.NodeSocketMenu, description="Fill paint", default_value=_SOLID.title, items=Some(_PAINTS.menu))
_FILL: Final = InterfacePanel.of("fill", Interface(inputs=(_PAINT,), panels=_PAINTS.panels), description="Color covering field", toggle=Some(True))

# --- [REGIONS]

_RING_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Ring width over greater frame side", min_value=0.0, default_value=0.01)
_GAP: Final = Socket(key="offset", kind=bpy.types.NodeSocketFloat, description="Gap between region and ring over greater frame side", min_value=0.0)
_RING_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear ring color", default_value=(1.0, 0.0, 0.0, 1.0))
_OUTLINE: Final = InterfacePanel.of("outline", Interface(inputs=(_RING_WIDTH, _GAP, _RING_COLOR)), description="Ring outside field", toggle=Some(False))
_FRAME: Final = MenuItem(name="Frame", description="Whole frame")
_FIELD: Final = Socket(
    key="field",
    kind=bpy.types.NodeSocketMenu,
    description="Region paint and light fill",
    default_value=FIGURE.title,
    items=Some(MenuItems(items=(_FRAME, *(panel.item for panel in (FIGURE, BOOLEAN, MORPH, LIGHTNING, WINDOW))))),
)
_SOFTNESS: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Edge ramp past one pixel over greater frame side", min_value=0.0)
_ROUGHNESS: Final = Socket(key="roughness", kind=bpy.types.NodeSocketFloat, description="Edge displacement amplitude", min_value=0.0)
_INVERT: Final = Socket(key="invert", kind=bpy.types.NodeSocketBool, description="Fill outside field")

# --- [MODELS] ---------------------------------------------------------------------------


class Shapes(Case, frozen=True, kw_only=True):
    """Paints distance-field shapes and gradients over frame with their light, holding the bolt depth and forks, the gradient ramp, the edge noise, and the emitted spectrum."""

    depth: Annotated[int, msgspec.Meta(ge=1)] = 5
    branches: Annotated[int, msgspec.Meta(ge=0)] = 3
    ramp: Ramp = Ramp()
    edge_noise: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", noise_type="RIDGED_MULTIFRACTAL", scale=2.5, detail=15.0, roughness=0.9, gain=74.5, distortion=1.9))
    emission: Emission = Emission(spectrum=Tint())


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REGIONS]


def _fielded(shell: Shell[Shapes], frame: FrameSpace, growths: tuple[Scalar, ...]) -> tuple[Expr, ...]:
    """Returns grown coverages of the selected region under one noisy boundary, the whole frame covered under `Frame`."""
    record = shell.record
    regions: dict[MenuItem, Term] = {
        _FRAME: (0.0, 0.0, 0.0),
        FIGURE.item: figured(shell, frame, FIGURE).packed,
        BOOLEAN.item: joined(shell, frame).packed,
        MORPH.item: morphed(shell, frame).packed,
        LIGHTNING.item: bolt(shell, frame, depth=record.depth, branches=record.branches, family=stream(Shapes, LIGHTNING.key)).packed,
        WINDOW.item: windowed(shell, frame).packed,
    }
    selection = shell.menu(_FIELD, cases=regions)
    region = Bounded.of(selection)
    invert, softness, pixel = shell.input(_INVERT), shell.input(_SOFTNESS), 1.0 / frame.greater
    ((noise, _),) = textured(shell, _EDGE_NOISE, record.edge_noise, frame.point, 0.0, shell.input(SEED), streams=(stream(Shapes.edge_noise),))
    displaced = region.distance - (noise * 2.0 - 1.0) * shell.input(_ROUGHNESS)
    signed = switched(invert, displaced, -displaced)
    whole = selection.at(_FRAME.name)
    return tuple(
        switched(whole, switched(invert, coverage(signed, Some(region.width), softness, pixel, growth), coverage(signed, Nothing, softness, pixel, growth)), 1.0 - invert) for growth in growths
    )


# --- [PAINT]


def _painted(shell: Shell[Shapes], frame: FrameSpace) -> tuple[Expr, Expr]:
    """Returns the frame-fixed fill color and its alpha coverage, the gradient's coverage zero past black ramp ends."""

    def read(*path: Step) -> Expr:
        return shell.input(_FILL, *path)

    def parameter(panel: InterfacePanel, geometry: _Geometry) -> Expr:
        at = partial(read, _GRADIENT, panel)
        return geometry(turned(frame, at(_ORIGIN), at(_TURN)), at)

    geometries = {
        _LINEAR.item: parameter(_LINEAR, lambda local, at: linear(local, at(_LENGTH), at(_SWEEP))),
        _RADIAL.item: parameter(_RADIAL, lambda local, at: radial(local, at(_REACH), at(_ASPECT))),
        _ANGULAR.item: parameter(_ANGULAR, lambda local, _at: angular(local)),
        _DIAMOND.item: parameter(_DIAMOND, lambda local, at: diamond(local, at(_REACH))),
        _RECTANGULAR.item: parameter(_RECTANGULAR, lambda local, at: rectangular(local, at(_HALF_WIDTH), at(_HALF_HEIGHT))),
    }
    coordinate = shell.menu(_FILL, _GRADIENT, _GEOMETRY, cases=geometries)
    wave = partial(read, _GRADIENT, _WAVE)
    (waved,) = profiled(wave, (coordinate + (wave(_PHASE) / tau - clocked(shell.input, _FILL.panel(_GRADIENT.key, _WAVE.key, _CLOCK.key))),))
    extended, inside = folded(read(_GRADIENT, _EXTENSION), switched(read(_GRADIENT, _WAVE), coordinate, waved))
    ramp = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", extended),), content=Some(shell.record.ramp), socket="Color", shape=bpy.types.NodeSocketColor)
    tint = ramp.named(item_key(_FILL.key, _GRADIENT.key, Shapes.ramp))
    across, up = (textures.MapRange(interpolation_type="SMOOTHERSTEP").node(frame.fraction[axis]) for axis in ("X", "Y"))
    distance, cell, thickness = gridded(partial(read, _CELLS, _LATTICE), frame.point)
    weight = shell.menu(
        _FILL,
        _CELLS,
        _PATTERN,
        cases={
            MenuItem.of(_CellPattern.CHECKER): (cell["X"] + cell["Y"]) % 2.0,
            MenuItem.of(_CellPattern.RANDOM): hashed(read(_CELLS, SEED), float(stream(Shapes, _CELLS.key)), cell["X"], cell["Y"])["Red"],
        },
    )
    paints = {
        _SOLID.item: read(_SOLID, _COLOR),
        _GRADIENT.item: tint,
        _CORNERS.item: mixed(up, mixed(across, read(_CORNERS, _BOTTOM_LEFT), read(_CORNERS, _BOTTOM_RIGHT)), mixed(across, read(_CORNERS, _TOP_LEFT), read(_CORNERS, _TOP_RIGHT))),
        _CELLS.item: mixed(coverage(distance, Some(thickness), 0.0, 1.0 / frame.greater), mixed(weight, read(_CELLS, _FIRST), read(_CELLS, _SECOND)), read(_CELLS, _MORTAR)),
    }
    color = shell.menu(_FILL, _PAINT, cases=paints)
    return color, switched(color.at(_GRADIENT.title), 1.0, tint.at("Alpha") * inside)


# --- [LIGHT]


def _shaped(shell: Shell[Shapes]) -> Outcome[Expr]:
    """Returns the region's fill, outline, and emitted light with their coverage as alpha."""
    ring = partial(shell.input, _OUTLINE)
    region, inner, outer = _fielded(shell, shell.space, (0.0, ring(_GAP), ring(_GAP) + ring(_RING_WIDTH)))
    tint, opacity = _painted(shell, shell.space)
    field = region.named("shape_field")
    filled = switched(shell.input(_FILL), 0.0, field * opacity)
    outlined = switched(shell.input(_OUTLINE), 0.0, outer - inner)

    def summed(lights: tuple[Expr, ...]) -> Expr:
        light = vector("SCALE", tint, filled) + vector("SCALE", ring(_RING_COLOR), outlined) + switched(shell.input(_EMISSION), (0.0, 0.0, 0.0), vector("SCALE", lights[0], field))
        return with_alpha(light, filled + outlined)

    return emitted(shell, _EMISSION, shell.record.emission, (field,)).map(summed)


# --- [COMPOSITION] ----------------------------------------------------------------------

_EMISSION: Final = emission("emission", Shapes().emission, "Light the field adds", Some(False))
_EDGE_NOISE: Final = bases("edge_noise", Shapes().edge_noise, "Texture displacing field before coverage")
shapes: Final = CompositorEffect(
    _shaped,
    record=Shapes,
    interface=Interface(inputs=(_FIELD, _SOFTNESS, _ROUGHNESS, SEED, _INVERT), panels=(FIGURE, BOOLEAN, MORPH, LIGHTNING, WINDOW, _FILL, _OUTLINE, _EMISSION, _EDGE_NOISE)),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(precise),
    derives=(timing,),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Shapes", "shapes"]
