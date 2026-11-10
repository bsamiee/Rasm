"""Distance-field regions painted over the frame in solid, gradient, corner, or cell colors, with their outline and light: placed figures, two figures joined or morphed, an aspect-fitted window, and a lightning bolt of recursive midpoint displacement."""

from collections.abc import Callable
from enum import auto, Enum
from functools import cache, partial, reduce
from math import sqrt, tau
from typing import Annotated, Final, override

import bpy
from expression import Some
import msgspec

from rasm_bpy.nodes import Expr, Group, grouped, Interface, InterfacePanel, item_key, math, MenuItem, MenuItems, mixed, Ramp, Scalar, Socket, switched, Term, textures, vector, xyz
from rasm_bpy.render import (
    angular,
    Border,
    BORDERS,
    Bounded,
    box,
    CLOCK,
    Clock,
    clocked,
    CompositorEffect,
    CompositorRecord,
    coverage,
    cross,
    diamond,
    ellipse,
    folded,
    FrameSpace,
    GraphInputs,
    hashed,
    heart,
    Join,
    lattice,
    line,
    linear,
    Mix,
    polygon,
    precise,
    PROFILE,
    profiled,
    radial,
    rectangular,
    rotated,
    SEED,
    shorter,
    Step,
    stroke,
    timing,
    triangle,
    turned,
    with_alpha,
)
from rasm_imaging import stream
from rasm_records import Nothing, Outcome

from .bases import bases, Basis, Perlin, textured
from .emission import Emission, emission, emitted, Tint

# --- [TYPES] ----------------------------------------------------------------------------

type _Point = Annotated[Expr, bpy.types.NodeSocketVector]


class _Growth(Enum):
    """Point a bar fills from toward its completion share."""

    START = auto()
    CENTER = auto()
    EDGES = auto()


class _CellPattern(Enum):
    """Alternating or seeded colors of lattice cells."""

    CHECKER = auto()
    RANDOM = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [SIZES]

_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Full width over greater frame side", min_value=0.0, default_value=0.4)
_HEIGHT: Final = Socket(key="height", kind=bpy.types.NodeSocketFloat, description="Full height over greater frame side", min_value=0.0, default_value=0.2)
_ROUNDNESS: Final = Socket(key="roundness", kind=bpy.types.NodeSocketFloatFactor, description="Corner radius over shorter half side")
_EXPONENT: Final = Socket(key="exponent", kind=bpy.types.NodeSocketFloat, description="Superellipse power, 1 giving diamond and 2 ellipse", min_value=1.0, default_value=2.0)

# --- [PRIMITIVES]

_VERTEX_A: Final = Socket(key="vertex_a", kind=bpy.types.NodeSocketVector2D, description="First vertex offset over greater frame side", default_value=(0.0, 0.1))
_VERTEX_B: Final = Socket(key="vertex_b", kind=bpy.types.NodeSocketVector2D, description="Second vertex offset over greater frame side", default_value=(-0.05 * sqrt(3.0), -0.05))
_VERTEX_C: Final = Socket(key="vertex_c", kind=bpy.types.NodeSocketVector2D, description="Third vertex offset over greater frame side", default_value=(0.05 * sqrt(3.0), -0.05))
_VERTEX_ROUNDNESS: Final = Socket(key="roundness", kind=bpy.types.NodeSocketFloatFactor, description="Vertex rounding over incircle radius")
_ARM_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Arm-to-arm length over greater frame side", min_value=0.0, default_value=0.2)
_ARM_WIDTH: Final = Socket(key="thickness", kind=bpy.types.NodeSocketFloatFactor, description="Bar width over length", default_value=0.3)
_ARM_ASPECT: Final = Socket(key="aspect", kind=bpy.types.NodeSocketFloat, description="Horizontal over vertical arm length", min_value=0.0, default_value=1.0)
_ARM_ROUNDNESS: Final = Socket(key="roundness", kind=bpy.types.NodeSocketFloatFactor, description="Corner radius over arm half width")
_LOBES: Final = Socket(key="height", kind=bpy.types.NodeSocketFloat, description="Tip-to-lobe height over greater frame side", min_value=0.0, default_value=0.2)
_SEGMENT_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Full length over greater frame side", min_value=0.0, default_value=0.4)
_SEGMENT_WIDTH: Final = Socket(key="thickness", kind=bpy.types.NodeSocketFloat, description="Line width over greater frame side", min_value=0.0, default_value=0.005)
_SEGMENT_START: Final = Socket(key="start", kind=bpy.types.NodeSocketFloatFactor, description="Line start along length")
_SEGMENT_END: Final = Socket(key="end", kind=bpy.types.NodeSocketFloatFactor, description="Line end along length", default_value=1.0)
_COMPLETION: Final = Socket(key="completion", kind=bpy.types.NodeSocketFloatFactor, description="Filled share of bar", default_value=0.5)
_GROWTH: Final = Socket(key="growth", kind=bpy.types.NodeSocketMenu, description="Point the fill grows from", default_value=MenuItem.of(_Growth.START).name, items=MenuItems.of(_Growth).items)
_CELL_WIDTH: Final = Socket(key="cell_width", kind=bpy.types.NodeSocketFloat, description="Horizontal spacing over greater frame side", min_value=0.0, default_value=0.1)
_CELL_HEIGHT: Final = Socket(key="cell_height", kind=bpy.types.NodeSocketFloat, description="Vertical spacing over greater frame side", min_value=0.0, default_value=0.1)
_LINE: Final = Socket(key="line", kind=bpy.types.NodeSocketFloat, description="Line width over greater frame side", min_value=0.0, default_value=0.002)
_COURSE: Final = Socket(key="offset", kind=bpy.types.NodeSocketFloatFactor, description="Course shift over cell width")
_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Distance from center to outer vertex", min_value=0.0, default_value=0.2)
_SIDES: Final = Socket(key="sides", kind=bpy.types.NodeSocketInt, description="Outer vertices", min_value=3, default_value=5)
_INSET: Final = Socket(key="inset", kind=bpy.types.NodeSocketFloatFactor, description="Side midpoint pull toward center")
_TIP_ROUNDNESS: Final = Socket(key="roundness", kind=bpy.types.NodeSocketFloatFactor, description="Vertex rounding radius over outer radius")
_BAND: Final = Socket(key="thickness", kind=bpy.types.NodeSocketFloatFactor, description="Band width over shorter half extent", default_value=0.1)
_GRID: Final = Interface(inputs=(_CELL_WIDTH, _CELL_HEIGHT, _LINE, _COURSE))
_BOX: Final = InterfacePanel.of("box", Interface(inputs=(_WIDTH, _HEIGHT, _ROUNDNESS)), description="Rectangle with rounded corners")
_ELLIPSE: Final = InterfacePanel.of("ellipse", Interface(inputs=(_WIDTH, _HEIGHT, _EXPONENT)), description="Ellipse inside width and height")
_TRIANGLE: Final = InterfacePanel.of("triangle", Interface(inputs=(_VERTEX_A, _VERTEX_B, _VERTEX_C, _VERTEX_ROUNDNESS)), description="Triangle with vertices offset from figure position")
_CROSS: Final = InterfacePanel.of("cross", Interface(inputs=(_ARM_LENGTH, _ARM_WIDTH, _ARM_ASPECT, _ARM_ROUNDNESS)), description="Plus sign of two crossing bars, an X when turned 45 degrees")
_HEART: Final = InterfacePanel.of("heart", Interface(inputs=(_LOBES,)), description="Heart centered on figure position")
_SEGMENT: Final = InterfacePanel.of(
    "segment", Interface(inputs=(_SEGMENT_LENGTH, _SEGMENT_WIDTH, _SEGMENT_START, _SEGMENT_END)), description="Round-capped line drawn between two shares of its length"
)
_BAR: Final = InterfacePanel.of(
    "bar", Interface(inputs=(_WIDTH, _HEIGHT, _ROUNDNESS, _COMPLETION, _GROWTH)).defaulted({_WIDTH.key: 0.8, _HEIGHT.key: 0.05}), description="Rounded box filled to a completion share"
)
_GRID_LINES: Final = InterfacePanel.of("grid", _GRID, description="Lattice lines through figure position, every second course shifted")
_POLYGON: Final = InterfacePanel.of("polygon", Interface(inputs=(_RADIUS, _SIDES, _INSET, _TIP_ROUNDNESS)), description="Regular polygon or inset star with rounded vertices")
_RING: Final = InterfacePanel.of("ring", Interface(inputs=(_WIDTH, _HEIGHT, _EXPONENT, _BAND)), description="Band inside a superellipse boundary")
_PRIMITIVES: Final = Interface(panels=(_BOX, _ELLIPSE, _TRIANGLE, _CROSS, _HEART, _SEGMENT, _BAR, _GRID_LINES, _POLYGON, _RING))

# --- [FIGURES]

_PRIMITIVE: Final = Socket(key="primitive", kind=bpy.types.NodeSocketMenu, description="Figure drawn about position", items=_PRIMITIVES.menu.items)
_POSITION: Final = Socket(key="position", kind=bpy.types.NodeSocketVector2D, description="Center in frame fractions", default_value=(0.5, 0.5))
_ROTATION: Final = Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Counterclockwise turn about position")
_FIT: Final = Socket(key="fit_short_side", kind=bpy.types.NodeSocketBool, description="Measure lengths against shorter frame side")
_PLACEMENT: Final = Interface(inputs=(_PRIMITIVE, _POSITION, _ROTATION, _FIT), panels=_PRIMITIVES.panels)

# --- [REGIONS]

_OPERATION: Final = Socket(
    key="operation", kind=bpy.types.NodeSocketMenu, description="Union, base minus cutter, or overlap", default_value=MenuItem.of(Join.DIFFERENCE).name, items=MenuItems.of(Join).items
)
_SMOOTHNESS: Final = Socket(key="smoothness", kind=bpy.types.NodeSocketFloat, description="Seam blend radius over greater frame side", min_value=0.0)
_PROGRESS: Final = Socket(key="progress", kind=bpy.types.NodeSocketFloatFactor, description="Blend from start to end")
_INSET_SHARE: Final = Socket(key="inset", kind=bpy.types.NodeSocketFloatFactor, description="Inset over fitted shorter half side")
_APERTURE_ASPECT: Final = Socket(key="value", kind=bpy.types.NodeSocketFloat, description="Aperture width over height", min_value=0.0, default_value=2.39)
_APERTURE: Final = InterfacePanel.of("aspect", Interface(inputs=(_APERTURE_ASPECT,)), description="Aperture width over height, frame aspect when off", toggle=Some(True))

# --- [BOLTS]

_BOLT_START: Final = Socket(key="start", kind=bpy.types.NodeSocketVector2D, description="Point bolt leaves in frame fractions", default_value=(0.2, 1.0))
_BOLT_END: Final = Socket(key="end", kind=bpy.types.NodeSocketVector2D, description="Point bolt strikes in frame fractions", default_value=(0.6, 0.4))
_THICKNESS: Final = Socket(key="thickness", kind=bpy.types.NodeSocketFloat, description="Channel width over greater frame side", min_value=0.0, default_value=0.004)
_DISPLACEMENT: Final = Socket(key="displacement", kind=bpy.types.NodeSocketFloat, description="Midpoint offset over split segment", min_value=0.0, default_value=0.5)
_FORK_REACH: Final = Socket(key="reach", kind=bpy.types.NodeSocketFloat, description="Fork length over bolt length", min_value=0.0, default_value=0.4)
_SPREAD: Final = Socket(key="spread", kind=bpy.types.NodeSocketFloatAngle, description="Fork turn off bolt direction", min_value=0.0, max_value=tau / 6.0, default_value=0.5)
_TAPER: Final = Socket(key="taper", kind=bpy.types.NodeSocketFloatFactor, description="Fork root width over channel width", default_value=0.5)
_BOLT_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=24.0))
_LIGHTNING: Final = InterfacePanel.of(
    "lightning",
    Interface(inputs=(_BOLT_START, _BOLT_END, _THICKNESS, _DISPLACEMENT, _FORK_REACH, _SPREAD, _TAPER, SEED), panels=(_BOLT_CLOCK,)),
    description="Jagged bolt between two frame points, forking from its first subdivision points",
)

# --- [GEOMETRY]

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
_GEOMETRY: Final = Socket(key="geometry", kind=bpy.types.NodeSocketMenu, description="Band shape", default_value=_LINEAR.title, items=_GEOMETRIES.menu.items)
_EXTENSION: Final = Socket(key="extension", kind=bpy.types.NodeSocketMenu, description="Parameter past ramp ends", default_value=MenuItem.of(Border.CLAMP).name, items=BORDERS.items)
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
    items=MenuItems.of(_CellPattern).items,
)
_FIRST: Final = Socket(key="first", kind=bpy.types.NodeSocketColor, description="Scene-linear first cell color", default_value=(0.0, 0.0, 0.0, 1.0))
_SECOND: Final = Socket(key="second", kind=bpy.types.NodeSocketColor, description="Scene-linear second cell color", default_value=(1.0, 1.0, 1.0, 1.0))
_MORTAR: Final = Socket(key="mortar_color", kind=bpy.types.NodeSocketColor, description="Scene-linear mortar color", default_value=(1.0, 1.0, 1.0, 1.0))
_LATTICE: Final = InterfacePanel.of("grid", _GRID, description="Cell lattice, every second course shifted")
_SOLID: Final = InterfacePanel.of("solid", Interface(inputs=(_COLOR,)), description="One scene-linear color")
_GRADIENT: Final = InterfacePanel.of(
    "gradient", Interface(inputs=(_GEOMETRY, _EXTENSION), panels=(*_GEOMETRIES.panels, _WAVE)), description="Colors along a geometric parameter with optional periodic motion"
)
_CORNERS: Final = InterfacePanel.of(
    "corners", Interface(inputs=(_TOP_LEFT, _TOP_RIGHT, _BOTTOM_RIGHT, _BOTTOM_LEFT)), description="Four scene-linear corner colors interpolated across frame with smootherstep"
)
_CELLS: Final = InterfacePanel.of("cells", Interface(inputs=(_PATTERN, _FIRST, _SECOND, _MORTAR, SEED), panels=(_LATTICE,)), description="Alternating or seeded cell colors with separate mortar color")
_PAINTS: Final = Interface(panels=(_SOLID, _GRADIENT, _CORNERS, _CELLS))
_PAINT: Final = Socket(key="paint", kind=bpy.types.NodeSocketMenu, description="Fill paint", default_value=_SOLID.title, items=_PAINTS.menu.items)
_FILL: Final = InterfacePanel.of("fill", Interface(inputs=(_PAINT,), panels=_PAINTS.panels), description="Color covering field", toggle=Some(True))

# --- [OUTLINE]

_RING_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Ring width over greater frame side", min_value=0.0, default_value=0.01)
_GAP: Final = Socket(key="offset", kind=bpy.types.NodeSocketFloat, description="Gap between region and ring over greater frame side", min_value=0.0)
_RING_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear ring color", default_value=(1.0, 0.0, 0.0, 1.0))
_OUTLINE: Final = InterfacePanel.of("outline", Interface(inputs=(_RING_WIDTH, _GAP, _RING_COLOR)), description="Ring outside field", toggle=Some(False))
_FRAME: Final = MenuItem(name="Frame", description="Whole frame")
_SOFTNESS: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Edge ramp past one pixel over greater frame side", min_value=0.0)
_ROUGHNESS: Final = Socket(key="roughness", kind=bpy.types.NodeSocketFloat, description="Edge displacement amplitude", min_value=0.0)
_INVERT: Final = Socket(key="invert", kind=bpy.types.NodeSocketBool, description="Fill outside field")

# --- [MODELS] ---------------------------------------------------------------------------


class Shapes(CompositorRecord, frozen=True, kw_only=True):
    """Paints distance-field shapes and gradients over frame with their light, holding the bolt depth and forks, the gradient ramp, the edge noise, and the emitted spectrum."""

    # --- [SETTINGS]

    depth: Annotated[int, msgspec.Meta(ge=1)] = 5
    branches: Annotated[int, msgspec.Meta(ge=0)] = 3
    ramp: Ramp = Ramp()
    edge_noise: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", noise_type="RIDGED_MULTIFRACTAL", scale=2.5, detail=15.0, roughness=0.9, gain=74.5, distortion=1.9))
    emission: Emission = Emission(spectrum=Tint())

    # --- [RENDERING]

    requested = staticmethod(precise)

    # --- [LIGHT]

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Returns the selected region's fill, outline, and emitted light with their coverage as alpha, its grown coverages under one noisy boundary, the whole frame covered under `Frame`."""
        frame, ring = shell.space, partial(shell.input, _OUTLINE)
        base, cutter, seam = _figured(shell, frame, _BOOLEAN, _BASE), _figured(shell, frame, _BOOLEAN, _CUTTER), shell.input(_BOOLEAN, _SMOOTHNESS)
        extent = frame.extent
        aspect = switched(shell.input(_WINDOW, _APERTURE), extent["X"] / extent["Y"], shell.input(_WINDOW, _APERTURE, _APERTURE_ASPECT))
        width = math("MINIMUM", extent["Y"] * aspect, extent["X"])
        height = width / aspect
        margin = math("MINIMUM", width, height) * shell.input(_WINDOW, _INSET_SHARE)

        bolt, family, span = partial(shell.input, _LIGHTNING), stream(Shapes, _LIGHTNING.key), 2**self.depth
        time = clocked(shell.input, _LIGHTNING.panel(_BOLT_CLOCK.key)) * 2.0
        radius = bolt(_THICKNESS) * 0.5
        start, end = bolt(_BOLT_START) * frame.extent, bolt(_BOLT_END) * frame.extent
        seed, displacement = bolt(SEED), bolt(_DISPLACEMENT)

        def subdivided(first: Expr, last: Expr, radii: Expr, offset: int) -> Bounded:
            return Bounded.of(_channel(self.depth)(frame.point, first, last, radii, float(family + offset), time, seed, displacement))

        @cache
        def vertex(index: int) -> Expr:
            low = index & -index
            return start if index == 0 else end if index == span else _bisection(vertex(index - low), vertex(index + low), float(family + index), time, seed, displacement)

        def fork(index: int) -> Bounded:
            level = (index + 1).bit_length()
            cell, remainder = divmod((2 * (index + 1 - 2 ** (level - 1)) + 1) * span, 2**level)
            offset = span + index * (span + 2)
            root = mixed(remainder / 2**level, vertex(cell), vertex(cell + 1))
            angle = (_drawn(float(family + offset), time, seed) * 0.5 + 1.0) * bolt(_SPREAD) * (-1.0 if index % 2 else 1.0)
            tip = root + vector("SCALE", rotated(end - start, angle), (_drawn(float(family + offset + 1), time, seed) * 0.5 + 1.0) * bolt(_FORK_REACH))
            return subdivided(root, tip, xyz(radius * bolt(_TAPER)), offset + 2)

        regions: dict[MenuItem, Term] = {
            _FRAME: (0.0, 0.0, 0.0),
            _FIGURE.item: _figured(shell, frame, _FIGURE).packed,
            _BOOLEAN.item: Bounded.of(shell.menu(_BOOLEAN, _OPERATION, cases={MenuItem.of(join): base.joined(cutter, seam, join).packed for join in Join})).packed,
            _MORPH.item: _figured(shell, frame, _MORPH, _START).morphed(_figured(shell, frame, _MORPH, _END), shell.input(_MORPH, _PROGRESS)).packed,
            _LIGHTNING.item: reduce(Bounded.nearer, (fork(index) for index in range(self.branches)), subdivided(start, end, xyz(radius, radius), 0)).packed,
            _WINDOW.item: box(frame.point - extent * 0.5, width - margin, height - margin, shell.input(_WINDOW, _ROUNDNESS)).packed,
        }
        selection = shell.menu(_FIELD, cases=regions)
        region = Bounded.of(selection)
        invert, softness, pixel = shell.input(_INVERT), shell.input(_SOFTNESS), 1.0 / frame.greater
        ((noise, _),) = textured(shell, _EDGE_NOISE, self.edge_noise, frame.point, 0.0, shell.input(SEED), streams=(stream(Shapes.edge_noise),))
        displaced = region.distance - (noise * 2.0 - 1.0) * shell.input(_ROUGHNESS)
        signed = switched(invert, displaced, -displaced)
        whole = selection.at(_FRAME.name)
        grown, inner, outer = (
            switched(whole, switched(invert, coverage(signed, Some(region.width), softness, pixel, growth), coverage(signed, Nothing, softness, pixel, growth)), 1.0 - invert)
            for growth in (0.0, ring(_GAP), ring(_GAP) + ring(_RING_WIDTH))
        )

        def read(*path: Step) -> Expr:
            return shell.input(_FILL, *path)

        def parameter(panel: InterfacePanel, geometry: Callable[[Expr, Callable[[Socket], Expr]], Expr]) -> Expr:
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
        ramp = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", extended),), content=Some(self.ramp), socket="Color", socket_type=bpy.types.NodeSocketColor)
        graded = ramp.keyed(item_key(_FILL.key, _GRADIENT.key, Shapes.ramp))
        across, up = (textures.MapRange(interpolation_type="SMOOTHERSTEP").node(frame.fraction[axis]) for axis in ("X", "Y"))
        distance, cell, thickness = _gridded(partial(read, _CELLS, _LATTICE), frame.point)
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
            _GRADIENT.item: graded,
            _CORNERS.item: mixed(up, mixed(across, read(_CORNERS, _BOTTOM_LEFT), read(_CORNERS, _BOTTOM_RIGHT)), mixed(across, read(_CORNERS, _TOP_LEFT), read(_CORNERS, _TOP_RIGHT))),
            _CELLS.item: mixed(coverage(distance, Some(thickness), 0.0, 1.0 / frame.greater), mixed(weight, read(_CELLS, _FIRST), read(_CELLS, _SECOND)), read(_CELLS, _MORTAR)),
        }
        tint = shell.menu(_FILL, _PAINT, cases=paints)
        field = grown.keyed("shape_field")
        filled = switched(shell.input(_FILL), 0.0, field * switched(tint.at(_GRADIENT.title), 1.0, graded.at("Alpha") * inside))
        outlined = switched(shell.input(_OUTLINE), 0.0, outer - inner)

        def summed(lights: tuple[Expr, ...]) -> Expr:
            light = vector("SCALE", tint, filled) + vector("SCALE", ring(_RING_COLOR), outlined) + switched(shell.input(_EMISSION), (0.0, 0.0, 0.0), vector("SCALE", lights[0], field))
            return with_alpha(light, filled + outlined)

        return emitted(shell, _EMISSION, self.emission, (field,)).map(summed)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PANELS]


def _figure(key: str, primitive: InterfacePanel, description: str, position: tuple[float, float] = (0.5, 0.5)) -> InterfacePanel:
    """Figure panel at `key`: its primitive menu defaulting to `primitive`, its placement, and one panel per primitive."""
    return InterfacePanel.of(key, _PLACEMENT.defaulted({_PRIMITIVE.key: primitive.title, _POSITION.key: position}), description=description)


# --- [FIELDS]


def _gridded(read: Callable[[Socket], Expr], point: Expr) -> tuple[Expr, Expr, Expr]:
    """Returns the lattice distance, cell index, and line width of a `_GRID` panel at a point, every second course shifted."""
    thickness = read(_LINE)
    distance, cell = lattice(point, read(_CELL_WIDTH), read(_CELL_HEIGHT), read(_COURSE), thickness)
    return distance, cell, thickness


def _figured(shell: GraphInputs, frame: FrameSpace, *path: Step) -> Bounded:
    """Returns the field of the figure panel a path ends at, its primitive placed, turned, and measured against the greater or shorter frame side."""

    def read(*steps: Step) -> Expr:
        return shell.input(*path, *steps)

    scale = switched(read(_FIT), 1.0, shorter(frame.extent))
    point = turned(frame, read(_POSITION), read(_ROTATION)) / scale
    width, height, completion = read(_BAR, _WIDTH), read(_BAR, _HEIGHT), read(_BAR, _COMPLETION)
    clips = {
        _Growth.START: point["X"] - math("MULTIPLY_ADD", completion, 2.0, -1.0) * width * 0.5,
        _Growth.CENTER: abs(point["X"]) - completion * width * 0.5,
        _Growth.EDGES: (1.0 - completion) * width * 0.5 - abs(point["X"]),
    }
    clip = shell.menu(*path, _BAR, _GROWTH, cases={MenuItem.of(growth): held for growth, held in clips.items()})
    filled = width * completion
    bar = Bounded(
        distance=math("MAXIMUM", box(point, width, height, read(_BAR, _ROUNDNESS)).distance, clip),
        width=math("MINIMUM", switched(clip.at(MenuItem.of(_Growth.EDGES).name), filled, filled * 0.5), height),
    )
    lines = _gridded(partial(read, _GRID_LINES), point)
    cases = {
        _BOX.item: box(point, read(_BOX, _WIDTH), read(_BOX, _HEIGHT), read(_BOX, _ROUNDNESS)),
        _ELLIPSE.item: ellipse(point, read(_ELLIPSE, _WIDTH), read(_ELLIPSE, _HEIGHT), read(_ELLIPSE, _EXPONENT)),
        _TRIANGLE.item: triangle(point, read(_TRIANGLE, _VERTEX_A), read(_TRIANGLE, _VERTEX_B), read(_TRIANGLE, _VERTEX_C), read(_TRIANGLE, _VERTEX_ROUNDNESS)),
        _CROSS.item: cross(point, read(_CROSS, _ARM_LENGTH), read(_CROSS, _ARM_WIDTH), read(_CROSS, _ARM_ASPECT), read(_CROSS, _ARM_ROUNDNESS)),
        _HEART.item: heart(point, read(_HEART, _LOBES)),
        _SEGMENT.item: line(point, read(_SEGMENT, _SEGMENT_LENGTH), read(_SEGMENT, _SEGMENT_WIDTH), read(_SEGMENT, _SEGMENT_START), read(_SEGMENT, _SEGMENT_END)),
        _BAR.item: bar,
        _GRID_LINES.item: Bounded(distance=lines[0], width=lines[2]),
        _POLYGON.item: polygon(point, read(_POLYGON, _RADIUS), read(_POLYGON, _SIDES), read(_POLYGON, _INSET), read(_POLYGON, _TIP_ROUNDNESS)),
        _RING.item: ellipse(point, read(_RING, _WIDTH), read(_RING, _HEIGHT), read(_RING, _EXPONENT)).ring(read(_RING, _BAND)),
    }
    return Bounded.of(shell.menu(*path, _PRIMITIVE, cases={item: case.packed for item, case in cases.items()}) * scale)


# --- [BOLTS]


def _drawn(family: Scalar, time: Scalar, seed: Scalar) -> Expr:
    """Signed uniform draw in [-1, 1) per draw family, held time, and seed."""
    return math("MULTIPLY_ADD", hashed(seed, family, math("FLOOR", time))["Red"], 2.0, -1.0)


@grouped(kind=bpy.types.CompositorNodeTree)
def _bisection(start: _Point, end: _Point, family: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
    """Midpoint of a bolt segment pushed along its normal by a signed draw."""
    return mixed(0.5, start, end) + rotated(end - start, tau / 4.0) * (_drawn(family, time, seed) * displacement)


@cache
def _channel(depth: int) -> Group[[_Point, _Point, _Point, _Point, Scalar, Scalar, Scalar, Scalar]]:
    """Bolt channel group of `depth` levels, each splitting at a drawn midpoint between end radii in `radii` X and Y, its field packed."""

    def body(point: _Point, start: _Point, end: _Point, radii: _Point, family: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
        match depth:
            case 0:
                return stroke(point, start, end, radii["X"], radii["Y"]).packed
            case _:
                half = float(2 ** (depth - 1))
                split = _bisection(start, end, math("ADD", family, half), time, seed, displacement)
                middle = mixed(0.5, radii["X"], radii["Y"])
                lower = _channel(depth - 1)
                left = Bounded.of(lower(point, start, split, xyz(radii["X"], middle), family, time, seed, displacement))
                return left.nearer(Bounded.of(lower(point, split, end, xyz(middle, radii["Y"]), math("ADD", family, half), time, seed, displacement))).packed

    return grouped(kind=bpy.types.CompositorNodeTree, key=Some(item_key(_LIGHTNING.key, "channel", depth)))(body)


# --- [COMPOSITION] ----------------------------------------------------------------------

_FIGURE: Final = _figure("figure", _ELLIPSE, "Primitive placed and turned on frame")
_BASE: Final = _figure("base", _BOX, "First figure to combine")
_CUTTER: Final = _figure("cutter", _ELLIPSE, "Figure joined with base", (0.6, 0.5))
_START: Final = _figure("start", _BOX, "Figure shown at progress 0")
_END: Final = _figure("end", _ELLIPSE, "Figure shown at progress 1")
_BOOLEAN: Final = InterfacePanel.of("boolean", Interface(inputs=(_OPERATION, _SMOOTHNESS), panels=(_BASE, _CUTTER)), description="Two figures joined with a smooth seam")
_MORPH: Final = InterfacePanel.of("morph", Interface(inputs=(_PROGRESS,), panels=(_START, _END)), description="Blend from one figure's field to another's")
_WINDOW: Final = InterfacePanel.of("window", Interface(inputs=(_INSET_SHARE, _ROUNDNESS), panels=(_APERTURE,)), description="Centered aspect-fitted aperture with inset and rounded corners")

_FIELD: Final = Socket(
    key="field",
    kind=bpy.types.NodeSocketMenu,
    description="Region paint and light fill",
    default_value=_FIGURE.title,
    items=(_FRAME, *(panel.item for panel in (_FIGURE, _BOOLEAN, _MORPH, _LIGHTNING, _WINDOW))),
)
_EMISSION: Final = emission("emission", Shapes().emission, "Light the field adds", Some(False))
_EDGE_NOISE: Final = bases("edge_noise", Shapes().edge_noise, "Texture displacing field before coverage")
shapes: Final = CompositorEffect(
    record=Shapes,
    interface=Interface(inputs=(_FIELD, _SOFTNESS, _ROUGHNESS, SEED, _INVERT), panels=(_FIGURE, _BOOLEAN, _MORPH, _LIGHTNING, _WINDOW, _FILL, _OUTLINE, _EMISSION, _EDGE_NOISE)),
    mix=Mix.RESULT,
    derives=(timing,),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Shapes", "shapes"]
