"""Distance-field figures, booleans, morphs, lightning, and windows painted over the frame in solid, gradient, corner, or cell colors with their light."""

from collections.abc import Mapping
from functools import cache, partial, reduce
from math import sqrt, tau
from typing import Annotated

import bpy
from expression import Nothing, Some
import msgspec

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import End, Expr, Group, grouped, math, mixed, NODES, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import item_key
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.render.compositor.coordinates import Border, Clock, clocked, FrameSpace, origin, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Inspects, precise, Requests, Shell
from blender.bpy.render.compositor.inputs import KeyPath
from blender.bpy.render.compositor.shapes import (
    angular,
    Bounded,
    box,
    coverage,
    cross,
    diamond,
    ellipse,
    grid,
    heart,
    lattice,
    line,
    linear,
    polygon,
    radial,
    rectangular,
    rotated,
    stroke,
    triangle,
    turned,
)
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.geometry.regions import FrameExtent, FrameLength, Position
from blender.records.fields import AtLeastOne, Case, Choice, Finite, Gated, NonNegative, Positive, Presentation, Quantity, Rgb, Seed, Unit

from ...chain.stages import Effect, Stage
from .procedural import Basis, Emission, emitted, Perlin, profiled, textured, Tint, WaveProfile

# --- [TYPES] ----------------------------------------------------------------------------


class Growth(Choice):
    """Where a bar's fill starts."""

    START = 1, "From Start"
    CENTER = 2, "From Center"
    EDGES = 3, "From Edges"


class Operation(Choice):
    """Join of two figure fields."""

    UNION = 1
    SUBTRACT = 2
    INTERSECT = 3


class CellPattern(Choice):
    """Alternating or seeded colors of lattice cells."""

    CHECKER = 1
    RANDOM = 2


type Primitive = Box | Ellipse | Triangle | Cross | Heart | Segment | Bar | Grid | Polygon | Ring
type Region = Frame | Figure | Boolean | Morph | Lightning | Window
type Geometry = Linear | Radial | Angular | Diamond | Rectangular
type Paint = Solid | Gradient | Corners | Cells
type Point = Annotated[Expr, bpy.types.NodeSocketVector]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [PRIMITIVES]


class Box(Case, frozen=True, kw_only=True):
    """Rectangle with rounded corners."""

    width: Annotated[FrameLength, msgspec.Meta(description="Full width over greater frame side")] = 0.4
    height: Annotated[FrameLength, msgspec.Meta(description="Full height over greater frame side")] = 0.2
    roundness: Annotated[Unit, msgspec.Meta(description="Corner radius over shorter half side")] = 0.0


class Ellipse(Case, frozen=True, kw_only=True):
    """Ellipse inside width and height."""

    width: Annotated[FrameLength, msgspec.Meta(description="Full width over greater frame side")] = 0.4
    height: Annotated[FrameLength, msgspec.Meta(description="Full height over greater frame side")] = 0.2
    exponent: Annotated[AtLeastOne, msgspec.Meta(description="Superellipse power, 1 giving diamond and 2 ellipse"), Presentation(soft=Some((1.0, 8.0)))] = 2.0


class Triangle(Case, frozen=True, kw_only=True):
    """Triangle with vertices offset from figure position."""

    a: Annotated[tuple[Finite, Finite], msgspec.Meta(title="Vertex A", description="First vertex offset over greater frame side"), Presentation(quantity=Some(Quantity.FRAME))] = (0.0, 0.1)
    b: Annotated[tuple[Finite, Finite], msgspec.Meta(title="Vertex B", description="Second vertex offset over greater frame side"), Presentation(quantity=Some(Quantity.FRAME))] = (
        -0.05 * sqrt(3.0),
        -0.05,
    )
    c: Annotated[tuple[Finite, Finite], msgspec.Meta(title="Vertex C", description="Third vertex offset over greater frame side"), Presentation(quantity=Some(Quantity.FRAME))] = (
        0.05 * sqrt(3.0),
        -0.05,
    )
    roundness: Annotated[Unit, msgspec.Meta(description="Vertex rounding over incircle radius")] = 0.0


class Cross(Case, frozen=True, kw_only=True):
    """Plus sign of two crossing bars, an X when turned 45 degrees."""

    length: Annotated[FrameLength, msgspec.Meta(description="Arm-to-arm length over greater frame side")] = 0.2
    thickness: Annotated[Unit, msgspec.Meta(description="Bar width over length")] = 0.3
    aspect: Annotated[Positive, msgspec.Meta(description="Horizontal over vertical arm length")] = 1.0
    roundness: Annotated[Unit, msgspec.Meta(description="Corner radius over arm half width")] = 0.0


class Heart(Case, frozen=True, kw_only=True):
    """Heart centered on figure position."""

    height: Annotated[FrameLength, msgspec.Meta(description="Tip-to-lobe height over greater frame side")] = 0.2


class Segment(Case, frozen=True, kw_only=True):
    """Round-capped line drawn between two shares of its length."""

    length: Annotated[FrameLength, msgspec.Meta(description="Full length over greater frame side")] = 0.4
    thickness: Annotated[FrameLength, msgspec.Meta(description="Line width over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.005
    start: Annotated[Unit, msgspec.Meta(description="Line start along length")] = 0.0
    end: Annotated[Unit, msgspec.Meta(description="Line end along length")] = 1.0


class Bar(Case, frozen=True, kw_only=True):
    """Rounded box filled to a completion share."""

    box: Annotated[Box, msgspec.Meta(description="Bar boundary before completion clip")] = Box(width=0.8, height=0.05)
    completion: Annotated[Unit, msgspec.Meta(description="Filled share of bar")] = 0.5
    growth: Annotated[Growth, msgspec.Meta(description="Fill start")] = Growth.START


class Grid(Case, frozen=True, kw_only=True):
    """Lattice lines through figure position, every second course shifted."""

    width: Annotated[FrameExtent, msgspec.Meta(title="Cell Width", description="Horizontal spacing over greater frame side")] = 0.1
    height: Annotated[FrameExtent, msgspec.Meta(title="Cell Height", description="Vertical spacing over greater frame side")] = 0.1
    line: Annotated[FrameLength, msgspec.Meta(description="Line width over greater frame side"), Presentation(soft=Some((0.0, 0.01)))] = 0.002
    offset: Annotated[Unit, msgspec.Meta(description="Course shift over cell width")] = 0.0


class Polygon(Case, frozen=True, kw_only=True):
    """Regular polygon or inset star with rounded vertices."""

    radius: Annotated[FrameLength, msgspec.Meta(description="Distance from center to outer vertex")] = 0.2
    sides: Annotated[int, msgspec.Meta(ge=3, description="Outer vertices around center"), GraphForm.SETTING] = 5
    inset: Annotated[Unit, msgspec.Meta(description="Side midpoint pull toward center")] = 0.0
    roundness: Annotated[Unit, msgspec.Meta(description="Vertex rounding radius over outer radius")] = 0.0


class Ring(Case, frozen=True, kw_only=True):
    """Band inside a superellipse boundary."""

    ellipse: Annotated[Ellipse, msgspec.Meta(title="Boundary", description="Outer superellipse")] = Ellipse()
    thickness: Annotated[Unit, msgspec.Meta(description="Band width over shorter half extent")] = 0.1


# --- [REGIONS]


class Figure(Case, frozen=True, kw_only=True):
    """Primitive placed and turned on frame."""

    primitive: Annotated[Primitive, msgspec.Meta(description="Figure drawn about position")] = Ellipse()
    position: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    rotation: Annotated[Finite, msgspec.Meta(description="Counterclockwise turn about position"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    short_side: Annotated[bool, msgspec.Meta(title="Fit Short Side", description="Measure lengths against shorter frame side")] = False


class Frame(Case, frozen=True, kw_only=True):
    """Whole frame."""


class Window(Case, frozen=True, kw_only=True):
    """Centered aspect-fitted aperture with inset and rounded corners."""

    aspect: Annotated[Gated[Positive], msgspec.Meta(description="Aperture width over height, frame aspect when off")] = Gated(enabled=True, value=2.39)
    inset: Annotated[Unit, msgspec.Meta(description="Inset over fitted shorter half side")] = 0.0
    roundness: Annotated[Unit, msgspec.Meta(description="Corner radius over shorter half side")] = 0.0


class Boolean(Case, frozen=True, kw_only=True):
    """Two figures joined with a smooth seam."""

    operation: Annotated[Operation, msgspec.Meta(description="Union, base minus cutter, or overlap")] = Operation.SUBTRACT
    base: Annotated[Figure, msgspec.Meta(description="First figure to combine")] = Figure(primitive=Box())
    cutter: Annotated[Figure, msgspec.Meta(description="Figure joined with base")] = Figure(position=(0.6, 0.5))
    smoothness: Annotated[FrameLength, msgspec.Meta(description="Seam blend radius over greater frame side"), Presentation(soft=Some((0.0, 0.1)))] = 0.0


class Morph(Case, frozen=True, kw_only=True):
    """Blend from one figure's field to another's."""

    start: Annotated[Figure, msgspec.Meta(description="Figure shown at progress 0")] = Figure(primitive=Box())
    end: Annotated[Figure, msgspec.Meta(description="Figure shown at progress 1")] = Figure()
    progress: Annotated[Unit, msgspec.Meta(description="Blend from start to end")] = 0.0


class Lightning(Case, frozen=True, kw_only=True):
    """Jagged bolt between two frame points, forking from its first subdivision points."""

    start: Annotated[Position, msgspec.Meta(description="Point bolt leaves in frame fractions")] = (0.2, 1.0)
    end: Annotated[Position, msgspec.Meta(description="Point bolt strikes in frame fractions")] = (0.6, 0.4)
    thickness: Annotated[FrameLength, msgspec.Meta(description="Channel width over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.004
    depth: Annotated[int, msgspec.Meta(ge=1, description="Channel subdivision levels"), GraphForm.SETTING] = 5
    displacement: Annotated[NonNegative, msgspec.Meta(description="Midpoint offset over split segment"), Presentation(soft=Some((0.0, 1.0)))] = 0.5
    branches: Annotated[int, msgspec.Meta(ge=0, description="Forks leaving channel in dyadic level order"), GraphForm.SETTING] = 3
    reach: Annotated[NonNegative, msgspec.Meta(description="Fork length over bolt length"), Presentation(soft=Some((0.0, 1.0)))] = 0.4
    spread: Annotated[float, msgspec.Meta(ge=0.0, le=tau / 6.0, description="Fork turn off bolt direction"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.5
    taper: Annotated[Unit, msgspec.Meta(description="Fork root width over channel width")] = 0.5
    clock: Annotated[Clock, msgspec.Meta(description="Clock each bolt redraws over")] = Clock()
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0


# --- [GEOMETRIES]


class Linear(Case, frozen=True, kw_only=True):
    """Bands across a direction from an origin, swept back into chevrons."""

    origin: Annotated[Position, msgspec.Meta(description="Point at first stop in frame fractions")] = (0.0, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="Direction toward last stop, 0 running left to right"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    length: Annotated[FrameExtent, msgspec.Meta(description="Distance from first stop to last over greater frame side")] = 1.0
    sweep: Annotated[NonNegative, msgspec.Meta(description="Chevron slope, 0 straight and 1 at 45 degrees"), Presentation(soft=Some((0.0, 4.0)))] = 0.0


class Radial(Case, frozen=True, kw_only=True):
    """Rings about an origin."""

    origin: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="Ellipse turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    radius: Annotated[FrameExtent, msgspec.Meta(description="Distance to last stop over greater frame side")] = 0.5
    aspect: Annotated[Positive, msgspec.Meta(description="Horizontal radius over vertical radius")] = 1.0


class Angular(Case, frozen=True, kw_only=True):
    """Sweep around an origin."""

    origin: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="First stop direction"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Diamond(Case, frozen=True, kw_only=True):
    """Square rings about an origin."""

    origin: Annotated[Position, msgspec.Meta(description="Center in frame fractions")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="Ring turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    radius: Annotated[FrameExtent, msgspec.Meta(description="Distance to last stop over greater frame side")] = 0.5


class Rectangular(Case, frozen=True, kw_only=True):
    """Rectangular gradient rings with independent half extents."""

    origin: Annotated[Position, msgspec.Meta(description="Ring center")] = (0.5, 0.5)
    angle: Annotated[Finite, msgspec.Meta(description="Ring turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    width: Annotated[FrameExtent, msgspec.Meta(description="Horizontal distance to last stop")] = 0.5
    height: Annotated[FrameExtent, msgspec.Meta(description="Vertical distance to last stop")] = 0.5


# --- [PAINTS]


class Solid(Case, frozen=True, kw_only=True):
    """One scene-linear color."""

    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear fill color")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Gradient(Case, frozen=True, kw_only=True):
    """Colors along a geometric parameter with optional periodic motion."""

    geometry: Annotated[Geometry, msgspec.Meta(description="Band shape")] = Linear()
    extension: Annotated[Border, msgspec.Meta(description="Parameter past ramp ends")] = Border.CLAMP
    ramp: Annotated[Ramp, msgspec.Meta(description="Scene-linear colors along parameter 0 to 1")] = Ramp()
    profile: Annotated[Gated[WaveProfile], msgspec.Meta(description="Periodic wave over parameter"), GraphForm.SETTING] = Gated(enabled=False, value="SIN")
    phase: Annotated[Finite, msgspec.Meta(description="Profile phase offset"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    clock: Annotated[Clock, msgspec.Meta(description="Clock advancing profile")] = Clock(speed=0.0)


class Corners(Case, frozen=True, kw_only=True):
    """Four scene-linear corner colors interpolated across frame with smootherstep."""

    top_left: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    top_right: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)
    bottom_right: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    bottom_left: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)


class Cells(Case, frozen=True, kw_only=True):
    """Alternating or seeded cell colors with separate mortar color."""

    grid: Annotated[Grid, msgspec.Meta(description="Lattice spacing, row offset, and mortar width")] = Grid()
    pattern: Annotated[CellPattern, msgspec.Meta(description="Alternating or seeded color weights"), GraphForm.SETTING] = CellPattern.CHECKER
    first: Rgb[Finite] = Rgb(red=0.0, green=0.0, blue=0.0)
    second: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    mortar: Rgb[Finite] = Rgb(red=1.0, green=1.0, blue=1.0)
    seed: Seed = 0


class Contour(msgspec.Struct, frozen=True, kw_only=True):
    """Ring outside the region."""

    width: Annotated[FrameLength, msgspec.Meta(description="Ring width over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01
    offset: Annotated[FrameLength, msgspec.Meta(description="Gap between region and ring over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.0
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear ring color")] = Rgb(red=1.0, green=0.0, blue=0.0)


class Shapes(Effect, frozen=True, kw_only=True):
    """Paints distance-field shapes and gradients over frame with their light."""

    stage = Stage.GENERATE
    pointwise = False
    field: Annotated[Region, msgspec.Meta(description="Region paint and light fill")] = Figure()
    fill: Annotated[Gated[Paint], msgspec.Meta(description="Color covering field")] = Gated(enabled=True, value=Solid())
    outline: Annotated[Gated[Contour], msgspec.Meta(description="Ring outside field")] = Gated(enabled=False, value=Contour())
    emission: Annotated[Gated[Emission], msgspec.Meta(description="Light the field adds")] = Gated(enabled=False, value=Emission(spectrum=Tint()))
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp past one pixel over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.0
    roughness: Annotated[FrameLength, msgspec.Meta(description="Edge displacement amplitude"), Presentation(soft=Some((0.0, 0.05)))] = 0.0
    edge_noise: Annotated[Basis, msgspec.Meta(description="Texture displacing field before coverage")] = Perlin(
        texture=textures.Noise(noise_dimensions="4D", noise_type="RIDGED_MULTIFRACTAL", scale=2.5, detail=15.0, roughness=0.9, gain=74.5, distortion=1.9)
    )
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting edge noise pattern")] = 0
    invert: Annotated[bool, msgspec.Meta(description="Fill outside field")] = False


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FIGURES]


def primitive[R: Effect](shell: Shell[R], path: KeyPath, shape: Primitive, point: Expr) -> Bounded:
    """Returns a primitive's local distance and bounded width."""
    inlet = partial(shell.input, *path)
    match shape:
        case Box():
            return box(point, *map(inlet, Box.__struct_fields__))
        case Ellipse():
            return ellipse(point, *map(inlet, Ellipse.__struct_fields__))
        case Cross():
            return cross(point, *map(inlet, Cross.__struct_fields__))
        case Triangle():
            return triangle(point, *map(inlet, Triangle.__struct_fields__))
        case Polygon(sides=sides):
            radius, _, inset, roundness = map(inlet, Polygon.__struct_fields__)
            return polygon(point, radius, sides, inset, roundness)
        case Heart():
            return heart(point, *map(inlet, Heart.__struct_fields__))
        case Segment():
            return line(point, *map(inlet, Segment.__struct_fields__))
        case Grid():
            width, height, thickness, offset = map(inlet, Grid.__struct_fields__)
            return grid(point, width, height, thickness, offset)
        case Bar(box=boundary):
            box_field, completion_field, growth = Bar.__struct_fields__
            width, height, _ = (inlet(box_field, member) for member in Box.__struct_fields__)
            completion, x = inlet(completion_field), point["X"]
            half = width * 0.5
            clips = (x - math("MULTIPLY_ADD", completion, 2.0, -1.0) * half, abs(x) - completion * half, (1.0 - completion) * half - abs(x))
            clip = shell.menu(*path, growth, cases={member.label: term for member, term in zip(Growth, clips)}, kind="FLOAT")
            length = width * completion
            filled = math("MINIMUM", switched(msgspec.structs.replace(clip, socket=Growth.EDGES.label), length, length * 0.5), height)
            return Bounded(distance=math("MAXIMUM", primitive(shell, (*path, box_field), boundary, point).distance, clip), width=filled)
        case Ring(ellipse=rim):
            ellipse_field, band = Ring.__struct_fields__
            return primitive(shell, (*path, ellipse_field), rim, point).ring(inlet(band))


def chosen[R: Effect](shell: Shell[R], path: KeyPath, cases: Mapping[str, Bounded]) -> Expr:
    """Selects one case's field through the path's Menu Switch, distance in X and width in Y."""
    return shell.menu(*path, cases={name: case.packed for name, case in cases.items()}, kind="VECTOR")


def figured[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> Bounded:
    """Returns a placed primitive's field in greater-side units."""
    primitive_field, position, rotation, short_side = Figure.__struct_fields__
    at = (*path, primitive_field)
    scale = switched(shell.input(*path, short_side), 1.0, shorter(frame.extent))
    point = turned(frame, shell.input(*path, position), shell.input(*path, rotation)) / scale
    cases = {
        type(shape).title: primitive(shell, (*at, type(shape)), shape, point)
        for shape in shell.cases[at]
        if isinstance(shape, Box | Ellipse | Triangle | Cross | Heart | Segment | Bar | Grid | Polygon | Ring)
    }
    return Bounded.of(chosen(shell, at, cases) * scale)


def combined[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, region: Boolean | Morph) -> Bounded:
    """Returns the boolean or morph field of two figures."""
    match region:
        case Morph():
            start, end, progress = Morph.__struct_fields__
            return figured(shell, (*path, start), frame).morphed(figured(shell, (*path, end), frame), shell.input(*path, progress))
        case Boolean():
            operation, base, cutter, smoothness = Boolean.__struct_fields__
            a, b, seam = figured(shell, (*path, base), frame), figured(shell, (*path, cutter), frame), shell.input(*path, smoothness)
            joins = {Operation.UNION: a.union(b, seam), Operation.SUBTRACT: a.difference(b, seam), Operation.INTERSECT: a.intersection(b, seam)}
            return Bounded.of(chosen(shell, (*path, operation), {member.label: joins[member] for member in Operation}))


def windowed[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> Bounded:
    """Returns the centered rounded aperture fitted to the displayed frame."""
    aspect_field, inset, roundness = Window.__struct_fields__
    extent = frame.extent
    aspect = switched(shell.gate(*path, aspect_field), extent["X"] / extent["Y"], shell.input(*path, aspect_field))
    width = math("MINIMUM", extent["Y"] * aspect, extent["X"])
    height = width / aspect
    margin = math("MINIMUM", width, height) * shell.input(*path, inset)
    return box(frame.point - extent * 0.5, width - margin, height - margin, shell.input(*path, roundness))


# --- [LIGHTNING]


def drawn(family: Scalar, time: Scalar, seed: Scalar) -> Expr:
    """Draws one White Noise value per draw family, held time, and seed."""
    return textures.White(noise_dimensions="4D").node(vector("FLOOR", origin(seed, family)), math("FLOOR", time))


@grouped(item_key(Shapes, Lightning, "midpoint"), f"{Lightning.title} Midpoint")
def bisection(start: Point, end: Point, family: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
    """Midpoint of a bolt segment pushed along its normal by a signed draw."""
    return mixed(0.5, start, end) + rotated(end - start, tau / 4.0) * (math("MULTIPLY_ADD", drawn(family, time, seed), 2.0, -1.0) * displacement)


@cache
def channel(depth: int) -> Group[[Point, Point, Point, Point, Scalar, Scalar, Scalar, Scalar]]:
    """Bolt channel group of `depth` levels, each splitting at a drawn midpoint between end radii in `radii` X and Y, its field packed."""

    def body(point: Point, start: Point, end: Point, radii: Point, family: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
        match depth:
            case 0:
                return stroke(point, start, end, radii["X"], radii["Y"]).packed
            case _:
                half = float(2 ** (depth - 1))
                split = bisection(start, end, math("ADD", family, half), time, seed, displacement)
                middle = mixed(0.5, radii["X"], radii["Y"])
                lower = channel(depth - 1)
                left = Bounded.of(lower(point, start, split, xyz(radii["X"], middle), family, time, seed, displacement))
                return left.nearer(Bounded.of(lower(point, split, end, xyz(middle, radii["Y"]), math("ADD", family, half), time, seed, displacement))).packed

    return grouped(item_key(Shapes, Lightning, "channel", depth), f"{Lightning.title} Channel {depth}")(body)


def lightning[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, shape: Lightning) -> Bounded:
    """Builds the dyadic channel with tapered forks from its first subdivision points."""
    inlet = partial(shell.input, *path)
    start_field, end_field, thickness, _, displacement_field, _, reach, spread, taper, clock, seed_field = Lightning.__struct_fields__
    speed, hold = Clock.__struct_fields__
    time = clocked(inlet(clock, speed), inlet(clock, hold)) * 2.0
    radius = inlet(thickness) * 0.5
    start, end = inlet(start_field) * frame.extent, inlet(end_field) * frame.extent
    seed, displacement = inlet(seed_field), inlet(displacement_field)
    span = 2**shape.depth
    family = textures.stream(Lightning)

    def subdivided(first: Expr, last: Expr, radii: Expr, offset: int) -> Bounded:
        return Bounded.of(channel(shape.depth)(frame.point, first, last, radii, float(family + offset), time, seed, displacement))

    @cache
    def vertex(index: int) -> Expr:
        low = index & -index
        return start if index == 0 else end if index == span else bisection(vertex(index - low), vertex(index + low), float(family + index), time, seed, displacement)

    def fork(index: int) -> Bounded:
        level = (index + 1).bit_length()
        cell, remainder = divmod((2 * (index + 1 - 2 ** (level - 1)) + 1) * span, 2**level)
        offset = span + index * (span + 2)
        root = mixed(remainder / 2**level, vertex(cell), vertex(cell + 1))
        angle = (drawn(float(family + offset), time, seed) + 0.5) * inlet(spread) * (-1.0 if index % 2 else 1.0)
        tip = root + vector("SCALE", rotated(end - start, angle), (drawn(float(family + offset + 1), time, seed) + 0.5) * inlet(reach))
        return subdivided(root, tip, xyz(radius * inlet(taper)), offset + 2)

    return reduce(Bounded.nearer, (fork(index) for index in range(shape.branches)), subdivided(start, end, xyz(radius, radius), 0))


# --- [REGIONS]


def fielded(shell: Shell[Shapes], frame: FrameSpace, growths: tuple[Scalar, ...] = (0.0,)) -> tuple[Expr, ...]:
    """Returns grown coverages of the selected region under one noisy boundary, the whole frame covered under `Frame`."""
    field, _, _, _, softness_field, roughness, edge_noise, seed, invert_field = Shapes.__struct_fields__
    path = (field,)

    def geometric(region: Figure | Boolean | Morph | Lightning | Window) -> Bounded:
        at = (*path, type(region))
        match region:
            case Figure():
                return figured(shell, at, frame)
            case Boolean() | Morph():
                return combined(shell, at, frame, region)
            case Lightning():
                return lightning(shell, at, frame, region)
            case Window():
                return windowed(shell, at, frame)

    selection = chosen(shell, path, {type(region).title: geometric(region) for region in shell.cases[path] if isinstance(region, Figure | Boolean | Morph | Lightning | Window)})
    region = Bounded.of(selection)
    invert, softness, pixel = shell.input(invert_field), shell.input(softness_field), 1.0 / frame.greater
    ((noise, _),) = textured(shell, (edge_noise,), frame.point, 0.0, shell.input(seed), streams=(textures.stream(Shapes, edge_noise),))
    displaced = region.distance - math("MULTIPLY_ADD", noise, 2.0, -1.0) * shell.input(roughness)
    signed = switched(invert, displaced, -displaced)
    whole = msgspec.structs.replace(selection, socket=Frame.title, shape="FLOAT")
    return tuple(
        switched(whole, switched(invert, coverage(signed, Some(region.width), softness, pixel, growth), coverage(signed, Nothing, softness, pixel, growth)), 1.0 - invert) for growth in growths
    )


# --- [PAINT]


def graded[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, record: Gradient) -> tuple[Expr, Expr]:
    """Returns ramp color along the selected geometric parameter and its coverage past the ramp ends."""
    geometry_field, extension, ramp, _, phase, clock = Gradient.__struct_fields__
    speed, hold = Clock.__struct_fields__
    geometry = (*path, geometry_field)

    def parameter(shape: Geometry) -> Expr:
        inlet = partial(shell.input, *geometry, type(shape))
        local = turned(frame, inlet("origin"), inlet("angle"))
        match shape:
            case Linear():
                return linear(local, inlet("length"), inlet("sweep"))
            case Radial():
                return radial(local, inlet("radius"), inlet("aspect"))
            case Angular():
                return angular(local)
            case Diamond():
                return diamond(local, inlet("radius"))
            case Rectangular():
                return rectangular(local, inlet("width"), inlet("height"))

    coordinate = shell.menu(
        *geometry, cases={type(shape).title: parameter(shape) for shape in shell.cases[geometry] if isinstance(shape, Linear | Radial | Angular | Diamond | Rectangular)}, kind="FLOAT"
    )
    cycles = coordinate + (shell.input(*path, phase) / tau - clocked(shell.input(*path, clock, speed), shell.input(*path, clock, hold)))
    held = profiled(record.profile.value, cycles, NODES) if record.profile.enabled else coordinate
    clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", held),), socket="Result")
    folds = {Border.BLACK: clamped, Border.CLAMP: clamped, Border.PERIODIC: math("FRACT", held), Border.MIRROR: math("PINGPONG", held, 1.0)}
    extended = shell.menu(*path, extension, cases={member.label: folds[member] for member in Border}, kind="FLOAT")
    inside = (1.0 - (held < 0.0)) * (1.0 - (held > 1.0))
    tint = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", extended),), content=Some(record.ramp), socket="Color", shape="RGBA").named(item_key(*path, ramp))
    return tint, msgspec.structs.replace(tint, socket="Alpha", shape="FLOAT") * switched(msgspec.structs.replace(extended, socket=Border.BLACK.label), 1.0, inside)


def painted[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> tuple[Expr, Scalar]:
    """Returns the frame-fixed paint color and its alpha coverage."""
    (color,) = Solid.__struct_fields__
    top_left, top_right, bottom_right, bottom_left = Corners.__struct_fields__
    lines, _, first, second, mortar, seed = Cells.__struct_fields__

    def paint(case: Paint) -> tuple[Expr, Scalar]:
        inlet = partial(shell.input, *path, type(case))
        match case:
            case Solid():
                return inlet(color), 1.0
            case Corners():
                across, up = (textures.MapRange(interpolation_type="SMOOTHERSTEP").node(frame.fraction[axis]) for axis in ("X", "Y"))
                return mixed(up, mixed(across, inlet(bottom_left), inlet(bottom_right)), mixed(across, inlet(top_left), inlet(top_right))), 1.0
            case Cells(pattern=pattern):
                width, height, thickness, offset = (inlet(lines, member) for member in Grid.__struct_fields__)
                distance, cell = lattice(frame.point, width, height, offset, thickness)
                match pattern:
                    case CellPattern.CHECKER:
                        weight = (cell["X"] + cell["Y"]) % 2.0
                    case CellPattern.RANDOM:
                        weight = textures.White(noise_dimensions="3D").node(vector("FLOOR", cell + origin(inlet(seed), float(textures.stream(Cells)))))
                return mixed(coverage(distance, Some(thickness), 0.0, 1.0 / frame.greater), mixed(weight, inlet(first), inlet(second)), inlet(mortar)), 1.0
            case Gradient():
                return graded(shell, (*path, Gradient), frame, case)

    paints = {type(case).title: paint(case) for case in shell.cases[path] if isinstance(case, Solid | Gradient | Corners | Cells)}
    tint = shell.menu(*path, cases={name: term for name, (term, _) in paints.items()}, kind="RGBA")
    opaque: Scalar = 1.0
    return tint, reduce(
        lambda held, item: switched(msgspec.structs.replace(tint, socket=item[0], shape="FLOAT"), held, item[1]), ((name, part) for name, (_, part) in paints.items() if isinstance(part, Expr)), opaque
    )


# --- [LIGHT]


def shaped(shell: Shell[Shapes]) -> BlenderResult[Expr]:
    """Returns the region's fill, outline, and emitted light with their coverage as alpha."""
    _, fill, outline, emission, *_ = Shapes.__struct_fields__
    width, offset, color = Contour.__struct_fields__
    _, value = Gated.__struct_fields__
    ring = partial(shell.input, outline, value)
    region, inner, outer = fielded(shell, shell.space, (0.0, ring(offset), ring(offset) + ring(width)))
    field = region.named("shape_field")
    tint, opacity = painted(shell, (fill, value), shell.space)
    filled = switched(shell.gate(fill), 0.0, field * opacity)
    outlined = switched(shell.gate(outline), 0.0, outer - inner)

    def lit(lights: tuple[Expr, ...]) -> Expr:
        light = vector("SCALE", tint, filled) + vector("SCALE", ring(color), outlined) + switched(shell.gate(emission), (0.0, 0.0, 0.0), vector("SCALE", lights[0], field))
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", light), ("Alpha", filled + outlined), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    return emitted(shell, (emission, value), (field,)).map(lit)


# --- [COMPOSITION] ----------------------------------------------------------------------

shapes = CompositorEffect(
    record=Shapes,
    graph=shaped,
    alpha=Alpha.RESULT,
    parts=(Requests[Shapes](request=precise), Inspects(key="shape_coverage", end=End(node="shape_field", socket="Output"), description="Inner shape field coverage")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Angular",
    "Bar",
    "Boolean",
    "Box",
    "CellPattern",
    "Cells",
    "Contour",
    "Corners",
    "Cross",
    "Diamond",
    "Ellipse",
    "Figure",
    "Frame",
    "Geometry",
    "Gradient",
    "Grid",
    "Growth",
    "Heart",
    "Lightning",
    "Linear",
    "Morph",
    "Operation",
    "Paint",
    "Polygon",
    "Primitive",
    "Radial",
    "Rectangular",
    "Region",
    "Ring",
    "Segment",
    "Shapes",
    "Solid",
    "Triangle",
    "Window",
    "shapes",
]
