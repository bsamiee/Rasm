"""Distance-field figures placed and turned on frame, two figures joined or morphed, an aspect-fitted window, and a jagged lightning bolt of recursive midpoint displacement forking from its first subdivision points, each a region panel and the bounded field it reads."""

from collections.abc import Callable
from enum import auto, Enum
from functools import cache, partial, reduce
from math import sqrt, tau
from typing import Annotated, Final

import bpy
from expression import Some

from blender.bpy.nodes.desired import item_key, MenuItem, MenuItems
from blender.bpy.nodes.functions import Group, grouped
from blender.bpy.nodes.graphs import Expr, math, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor.coordinates import FrameSpace, shorter
from blender.bpy.render.compositor.inputs import GraphInputs, Step
from blender.bpy.render.compositor.motion import CLOCK, Clock, clocked, hashed, SEED
from blender.bpy.render.compositor.shapes import Bounded, box, cross, ellipse, heart, Join, lattice, line, polygon, rotated, stroke, triangle, turned

# --- [TYPES] ----------------------------------------------------------------------------

type _Point = Annotated[Expr, bpy.types.NodeSocketVector]


class _Growth(Enum):
    """Point a bar fills from toward its completion share."""

    START = auto()
    CENTER = auto()
    EDGES = auto()


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
GRID: Final = Interface(inputs=(_CELL_WIDTH, _CELL_HEIGHT, _LINE, _COURSE))
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
_LATTICE: Final = InterfacePanel.of("grid", GRID, description="Lattice lines through figure position, every second course shifted")
_POLYGON: Final = InterfacePanel.of("polygon", Interface(inputs=(_RADIUS, _SIDES, _INSET, _TIP_ROUNDNESS)), description="Regular polygon or inset star with rounded vertices")
_RING: Final = InterfacePanel.of("ring", Interface(inputs=(_WIDTH, _HEIGHT, _EXPONENT, _BAND)), description="Band inside a superellipse boundary")
_PRIMITIVES: Final = Interface(panels=(_BOX, _ELLIPSE, _TRIANGLE, _CROSS, _HEART, _SEGMENT, _BAR, _LATTICE, _POLYGON, _RING))

# --- [FIGURES]

_PRIMITIVE: Final = Socket(key="primitive", kind=bpy.types.NodeSocketMenu, description="Figure drawn about position", items=_PRIMITIVES.menu.items)
_POSITION: Final = Socket(key="position", kind=bpy.types.NodeSocketVector2D, description="Center in frame fractions", default_value=(0.5, 0.5))
_ROTATION: Final = Socket(key="rotation", kind=bpy.types.NodeSocketFloatAngle, description="Counterclockwise turn about position")
_FIT: Final = Socket(key="fit_short_side", kind=bpy.types.NodeSocketBool, description="Measure lengths against shorter frame side")
_FIGURE: Final = Interface(inputs=(_PRIMITIVE, _POSITION, _ROTATION, _FIT), panels=_PRIMITIVES.panels)

# --- [REGIONS]

_OPERATION: Final = Socket(
    key="operation", kind=bpy.types.NodeSocketMenu, description="Union, base minus cutter, or overlap", default_value=MenuItem.of(Join.DIFFERENCE).name, items=MenuItems.of(Join).items
)
_SMOOTHNESS: Final = Socket(key="smoothness", kind=bpy.types.NodeSocketFloat, description="Seam blend radius over greater frame side", min_value=0.0)
_PROGRESS: Final = Socket(key="progress", kind=bpy.types.NodeSocketFloatFactor, description="Blend from start to end")
_INSET_SHARE: Final = Socket(key="inset", kind=bpy.types.NodeSocketFloatFactor, description="Inset over fitted shorter half side")
_ASPECT: Final = Socket(key="value", kind=bpy.types.NodeSocketFloat, description="Aperture width over height", min_value=0.0, default_value=2.39)
_APERTURE: Final = InterfacePanel.of("aspect", Interface(inputs=(_ASPECT,)), description="Aperture width over height, frame aspect when off", toggle=Some(True))

# --- [BOLTS]

_BOLT_START: Final = Socket(key="start", kind=bpy.types.NodeSocketVector2D, description="Point bolt leaves in frame fractions", default_value=(0.2, 1.0))
_BOLT_END: Final = Socket(key="end", kind=bpy.types.NodeSocketVector2D, description="Point bolt strikes in frame fractions", default_value=(0.6, 0.4))
_THICKNESS: Final = Socket(key="thickness", kind=bpy.types.NodeSocketFloat, description="Channel width over greater frame side", min_value=0.0, default_value=0.004)
_DISPLACEMENT: Final = Socket(key="displacement", kind=bpy.types.NodeSocketFloat, description="Midpoint offset over split segment", min_value=0.0, default_value=0.5)
_REACH: Final = Socket(key="reach", kind=bpy.types.NodeSocketFloat, description="Fork length over bolt length", min_value=0.0, default_value=0.4)
_SPREAD: Final = Socket(key="spread", kind=bpy.types.NodeSocketFloatAngle, description="Fork turn off bolt direction", min_value=0.0, max_value=tau / 6.0, default_value=0.5)
_TAPER: Final = Socket(key="taper", kind=bpy.types.NodeSocketFloatFactor, description="Fork root width over channel width", default_value=0.5)
_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=24.0))
LIGHTNING: Final = InterfacePanel.of(
    "lightning",
    Interface(inputs=(_BOLT_START, _BOLT_END, _THICKNESS, _DISPLACEMENT, _REACH, _SPREAD, _TAPER, SEED), panels=(_CLOCK,)),
    description="Jagged bolt between two frame points, forking from its first subdivision points",
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PANELS]


def _figure(key: str, primitive: InterfacePanel, description: str, position: tuple[float, float] = (0.5, 0.5)) -> InterfacePanel:
    """Figure panel at `key`: its primitive menu defaulting to `primitive`, its placement, and one panel per primitive."""
    return InterfacePanel.of(key, _FIGURE.defaulted({_PRIMITIVE.key: primitive.title, _POSITION.key: position}), description=description)


# --- [FIELDS]


def gridded(read: Callable[[Socket], Expr], point: Expr) -> tuple[Expr, Expr, Expr]:
    """Returns the lattice distance, cell index, and line width of a `GRID` panel at a point, every second course shifted."""
    thickness = read(_LINE)
    distance, cell = lattice(point, read(_CELL_WIDTH), read(_CELL_HEIGHT), read(_COURSE), thickness)
    return distance, cell, thickness


def figured(shell: GraphInputs, frame: FrameSpace, *path: Step) -> Bounded:
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
    lines = gridded(partial(read, _LATTICE), point)
    cases = {
        _BOX.item: box(point, read(_BOX, _WIDTH), read(_BOX, _HEIGHT), read(_BOX, _ROUNDNESS)),
        _ELLIPSE.item: ellipse(point, read(_ELLIPSE, _WIDTH), read(_ELLIPSE, _HEIGHT), read(_ELLIPSE, _EXPONENT)),
        _TRIANGLE.item: triangle(point, read(_TRIANGLE, _VERTEX_A), read(_TRIANGLE, _VERTEX_B), read(_TRIANGLE, _VERTEX_C), read(_TRIANGLE, _VERTEX_ROUNDNESS)),
        _CROSS.item: cross(point, read(_CROSS, _ARM_LENGTH), read(_CROSS, _ARM_WIDTH), read(_CROSS, _ARM_ASPECT), read(_CROSS, _ARM_ROUNDNESS)),
        _HEART.item: heart(point, read(_HEART, _LOBES)),
        _SEGMENT.item: line(point, read(_SEGMENT, _SEGMENT_LENGTH), read(_SEGMENT, _SEGMENT_WIDTH), read(_SEGMENT, _SEGMENT_START), read(_SEGMENT, _SEGMENT_END)),
        _BAR.item: bar,
        _LATTICE.item: Bounded(distance=lines[0], width=lines[2]),
        _POLYGON.item: polygon(point, read(_POLYGON, _RADIUS), read(_POLYGON, _SIDES), read(_POLYGON, _INSET), read(_POLYGON, _TIP_ROUNDNESS)),
        _RING.item: ellipse(point, read(_RING, _WIDTH), read(_RING, _HEIGHT), read(_RING, _EXPONENT)).ring(read(_RING, _BAND)),
    }
    return Bounded.of(shell.menu(*path, _PRIMITIVE, cases={item: case.packed for item, case in cases.items()}) * scale)


def joined(shell: GraphInputs, frame: FrameSpace) -> Bounded:
    """Returns the base and cutter figures joined under the boolean operation menu with a smooth seam."""
    base, cutter, seam = figured(shell, frame, BOOLEAN, _BASE), figured(shell, frame, BOOLEAN, _CUTTER), shell.input(BOOLEAN, _SMOOTHNESS)
    return Bounded.of(shell.menu(BOOLEAN, _OPERATION, cases={MenuItem.of(join): base.joined(cutter, seam, join).packed for join in Join}))


def morphed(shell: GraphInputs, frame: FrameSpace) -> Bounded:
    """Returns the start figure's field blended toward the end figure's by progress."""
    return figured(shell, frame, MORPH, _START).morphed(figured(shell, frame, MORPH, _END), shell.input(MORPH, _PROGRESS))


def windowed(shell: GraphInputs, frame: FrameSpace) -> Bounded:
    """Returns the centered aperture fitted to the frame at its aspect, the frame's own aspect while the aspect toggle is off."""
    extent = frame.extent
    aspect = switched(shell.input(WINDOW, _APERTURE), extent["X"] / extent["Y"], shell.input(WINDOW, _APERTURE, _ASPECT))
    width = math("MINIMUM", extent["Y"] * aspect, extent["X"])
    height = width / aspect
    margin = math("MINIMUM", width, height) * shell.input(WINDOW, _INSET_SHARE)
    return box(frame.point - extent * 0.5, width - margin, height - margin, shell.input(WINDOW, _ROUNDNESS))


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

    return grouped(kind=bpy.types.CompositorNodeTree, key=Some(item_key(LIGHTNING.key, "channel", depth)))(body)


def bolt(shell: GraphInputs, frame: FrameSpace, *, depth: int, branches: int, family: int) -> Bounded:
    """Returns the bolt's field: the main channel subdivided `depth` levels, then `branches` forks rooted at its first subdivision points.

    Args:
        shell: Entry inputs.
        frame: Camera frame of the entry input.
        depth: Subdivision levels of every channel.
        branches: Forks off the main channel.
        family: First draw family, from `tiles.stream`.

    Returns:
        Field of the nearest channel per pixel, with its width.
    """
    read = partial(shell.input, LIGHTNING)
    time = clocked(shell.input, LIGHTNING.panel(_CLOCK.key)) * 2.0
    radius = read(_THICKNESS) * 0.5
    start, end = read(_BOLT_START) * frame.extent, read(_BOLT_END) * frame.extent
    seed, displacement = read(SEED), read(_DISPLACEMENT)
    span = 2**depth

    def subdivided(first: Expr, last: Expr, radii: Expr, offset: int) -> Bounded:
        return Bounded.of(_channel(depth)(frame.point, first, last, radii, float(family + offset), time, seed, displacement))

    @cache
    def vertex(index: int) -> Expr:
        low = index & -index
        return start if index == 0 else end if index == span else _bisection(vertex(index - low), vertex(index + low), float(family + index), time, seed, displacement)

    def fork(index: int) -> Bounded:
        level = (index + 1).bit_length()
        cell, remainder = divmod((2 * (index + 1 - 2 ** (level - 1)) + 1) * span, 2**level)
        offset = span + index * (span + 2)
        root = mixed(remainder / 2**level, vertex(cell), vertex(cell + 1))
        angle = (_drawn(float(family + offset), time, seed) * 0.5 + 1.0) * read(_SPREAD) * (-1.0 if index % 2 else 1.0)
        tip = root + vector("SCALE", rotated(end - start, angle), (_drawn(float(family + offset + 1), time, seed) * 0.5 + 1.0) * read(_REACH))
        return subdivided(root, tip, xyz(radius * read(_TAPER)), offset + 2)

    return reduce(Bounded.nearer, (fork(index) for index in range(branches)), subdivided(start, end, xyz(radius, radius), 0))


# --- [COMPOSITION] ----------------------------------------------------------------------

FIGURE: Final = _figure("figure", _ELLIPSE, "Primitive placed and turned on frame")
_BASE: Final = _figure("base", _BOX, "First figure to combine")
_CUTTER: Final = _figure("cutter", _ELLIPSE, "Figure joined with base", (0.6, 0.5))
_START: Final = _figure("start", _BOX, "Figure shown at progress 0")
_END: Final = _figure("end", _ELLIPSE, "Figure shown at progress 1")
BOOLEAN: Final = InterfacePanel.of("boolean", Interface(inputs=(_OPERATION, _SMOOTHNESS), panels=(_BASE, _CUTTER)), description="Two figures joined with a smooth seam")
MORPH: Final = InterfacePanel.of("morph", Interface(inputs=(_PROGRESS,), panels=(_START, _END)), description="Blend from one figure's field to another's")
WINDOW: Final = InterfacePanel.of("window", Interface(inputs=(_INSET_SHARE, _ROUNDNESS), panels=(_APERTURE,)), description="Centered aspect-fitted aperture with inset and rounded corners")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["BOOLEAN", "FIGURE", "GRID", "LIGHTNING", "MORPH", "WINDOW", "bolt", "figured", "gridded", "joined", "morphed", "windowed"]
