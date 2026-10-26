"""Geometric fields, painted shapes, and shared distance-ramp coverage."""

from collections.abc import Mapping
from functools import cache, partial, reduce
from itertools import starmap
from math import cos, pi, sin, tau
from typing import Annotated, Self

import bpy
from expression import Nothing, Option, Some
import msgspec

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, Group, grouped, math, mixed, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.coordinates import clocked, FrameSpace, shorter
from blender.bpy.render.effects.generators.noise import emitted, periodic, sample, sampled
from blender.bpy.render.effects.records import KeyPath
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Inspected, precise, Shell
from blender.imaging.chain import Effect
from blender.imaging.effects.generators.noise import White
from blender.imaging.effects.generators.shapes import (
    Angular,
    Bar,
    Boolean,
    Box,
    CellPattern,
    Cells,
    Corners,
    Cross,
    Diamond,
    Ellipse,
    Figure,
    Frame,
    Geometry,
    Gradient,
    Grid,
    Growth,
    Heart,
    Lightning,
    Linear,
    Morph,
    Operation,
    Outline,
    Paint,
    Polygon,
    Primitive,
    Radial,
    Rectangular,
    Ring,
    Segment,
    Shapes,
    Solid,
    Triangle,
    Window,
)
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import Gated, Motion

# --- [TYPES] ----------------------------------------------------------------------------

type Region = Figure | Boolean | Morph | Lightning | Window
type Point = Annotated[Expr, bpy.types.NodeSocketVector]

# --- [MODELS] ---------------------------------------------------------------------------


class Bounded(msgspec.Struct, frozen=True, kw_only=True):
    """Signed region distance in greater-side frame shares and the region's local narrowest width."""

    distance: Expr
    width: Expr

    @classmethod
    def of(cls, packed: Expr) -> Self:
        """Reads the pair a `packed` vector carries."""
        return cls(distance=packed["X"], width=packed["Y"])

    @property
    def packed(self) -> Expr:
        """Distance in X and width in Y of one vector, for one Menu Switch or group output to carry."""
        return xyz(self.distance, self.width)

    def nearer(self, other: "Bounded") -> "Bounded":
        """Keeps per pixel whichever field lies nearer, with its width."""
        closer = other.distance < self.distance
        return Bounded(distance=where(closer, other.distance, self.distance), width=where(closer, other.width, self.width))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DISTANCES]


def rotated(direction: Expr, angle: Scalar, *, inverted: bool = False) -> Expr:
    """Turns a vector about Z through Vector Rotate, clockwise when inverted."""
    return Expr(kind=bpy.types.ShaderNodeVectorRotate, inputs=(("Vector", direction), ("Angle", angle)), members=(("rotation_type", "Z_AXIS"), ("invert", inverted)), socket="Vector", shape="VECTOR")


def turned(frame: FrameSpace, origin: Expr, angle: Scalar) -> Expr:
    """Returns the frame position about an origin in frame fractions, turned back through angle."""
    return rotated(frame.point - origin * frame.extent, angle, inverted=True)


def boxed(point: Expr, width: Scalar, height: Scalar, roundness: Scalar) -> Bounded:
    """Returns rounded-box distance and shorter full side."""
    narrow = math("MINIMUM", width, height)
    radius = narrow * roundness * 0.5
    corner = abs(point) - (xyz(width, height) * 0.5 - radius * (1.0, 1.0, 0.0))
    inside = math("MINIMUM", math("MAXIMUM", corner["X"], corner["Y"]), 0.0)
    return Bounded(distance=vector("LENGTH", vector("MAXIMUM", corner, (0.0, 0.0, 0.0))) + inside - radius, width=narrow)


def stroked(point: Expr, start: Expr, end: Expr, first: Scalar, last: Scalar) -> Bounded:
    """Returns projected segment distance and its interpolated diameter."""
    chord = end - start
    fraction = math("DIVIDE", vector("DOT_PRODUCT", point - start, chord), vector("DOT_PRODUCT", chord, chord), clamp=True)
    radius = mixed(fraction, first, last)
    return Bounded(distance=vector("LENGTH", point - (start + vector("SCALE", chord, fraction))) - radius, width=radius * 2.0)


def lattice(point: Expr, width: Scalar, height: Scalar, offset: Scalar, line: Scalar) -> tuple[Expr, Expr]:
    """Returns mortar distance and integer cell coordinate, every second course shifted."""
    course = math("FLOOR", point["Y"] / height) % 2.0
    parameter = xyz((point["X"] - course * offset * width) / width, point["Y"] / height)
    fraction = vector("FRACTION", parameter)
    distances = vector("MINIMUM", fraction, (1.0, 1.0, 0.0) - fraction) * xyz(width, height)
    return math("MULTIPLY_ADD", line, -0.5, math("MINIMUM", distances["X"], distances["Y"])), vector("FLOOR", parameter)


def polygonal(point: Expr, vertices: tuple[Expr, *tuple[Expr, ...]], rounding: Scalar) -> Expr:
    """Returns rounded boundary distance of ordered polygon, inside by even-odd edge crossings."""
    edges = tuple(zip(vertices, (*vertices[1:], vertices[0])))

    def crossing(start: Expr, end: Expr) -> Expr:
        straddled = abs((start["Y"] > point["Y"]) - (end["Y"] > point["Y"]))
        return straddled * (point["X"] < math("MULTIPLY_ADD", (point["Y"] - start["Y"]) / (end["Y"] - start["Y"]), end["X"] - start["X"], start["X"]))

    nearest = reduce(lambda held, distance: math("MINIMUM", held, distance), (stroked(point, start, end, 0.0, 0.0).distance for start, end in edges))
    first, *rest = starmap(crossing, edges)
    return where(sum(rest, first) % 2.0, -nearest, nearest) - rounding


def primitive[R: Effect](shell: Shell[R], path: KeyPath, shape: Primitive, point: Expr) -> Bounded:
    """Returns primitive's local distance and bounded width."""
    inlet = partial(shell.input, *path)
    match shape:
        case Box():
            return boxed(point, *map(inlet, Box.__struct_fields__))
        case Ellipse():
            width, height, exponent = map(inlet, Ellipse.__struct_fields__)
            half = xyz(width, height) * 0.5
            normalized = abs(point / half)
            powers = normalized**exponent
            level = (powers["X"] + powers["Y"]) ** (1.0 / exponent)
            slope = vector("LENGTH", normalized ** (exponent - 1.0) / half * level ** (1.0 - exponent))
            narrow = math("MINIMUM", width, height)
            centered = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", vector("LENGTH", point)), ("B", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
            return Bounded(distance=where(centered, narrow * -0.5, (level - 1.0) / slope), width=narrow)
        case Cross():
            length, thickness, aspect, roundness = map(inlet, Cross.__struct_fields__)
            across, up = boxed(point, length * aspect, length * thickness, roundness), boxed(point, length * thickness, length, roundness)
            return Bounded(distance=math("MINIMUM", across.distance, up.distance), width=math("MAXIMUM", across.width, up.width))
        case Triangle():
            a, b, c, roundness = map(inlet, Triangle.__struct_fields__)
            corners = (a, b, c)
            lengths = tuple(vector("DISTANCE", corners[(index + 1) % 3], corners[(index + 2) % 3]) for index in range(3))
            area = vector("LENGTH", vector("CROSS_PRODUCT", b - a, c - a))
            perimeter = sum(lengths[1:], lengths[0])
            weighted = tuple(vector("SCALE", corner, length) for corner, length in zip(corners, lengths))
            center = sum(weighted[1:], weighted[0]) / perimeter
            share = switched(area > 0.0, 0.0, roundness)
            incircle = area / perimeter
            first, second, third = (mixed(share, corner, center) for corner in corners)
            return Bounded(distance=polygonal(point, (first, second, third), incircle * roundness), width=incircle * 2.0)
        case Polygon(sides=sides):
            radius, _, inset, roundness = map(inlet, Polygon.__struct_fields__)
            rounding = radius * roundness
            outer = radius - rounding
            inner = outer * ((1.0 - inset) * cos(pi / sides))
            first, *rest = (vector("SCALE", (cos(pi / 2.0 + pi * index / sides), sin(pi / 2.0 + pi * index / sides), 0.0), inner if index % 2 else outer) for index in range(2 * sides))
            return Bounded(distance=polygonal(point, (first, *rest), rounding), width=(inner + rounding) * 2.0)
        case Heart():
            (height,) = map(inlet, Heart.__struct_fields__)
            lobe = 2.0**0.5 / 4.0
            span = 0.75 + lobe
            scale = height / span
            located = point / scale + (0.0, span / 2.0, 0.0)
            folded = xyz(abs(located["X"]), located["Y"])
            total = folded["X"] + folded["Y"]
            tip = vector("LENGTH", folded - math("MAXIMUM", total, 0.0) * 0.5 * (1.0, 1.0, 0.0))
            lower = math("MINIMUM", vector("LENGTH", folded - (0.0, 1.0, 0.0)), tip) * math("SIGN", folded["X"] - folded["Y"])
            return Bounded(distance=where(total > 1.0, vector("LENGTH", folded - (0.25, 0.75, 0.0)) - lobe, lower) * scale, width=height)
        case Segment():
            length, thickness, start, end = map(inlet, Segment.__struct_fields__)
            first, last = xyz((math("MINIMUM", start, end) - 0.5) * length), xyz((math("MAXIMUM", start, end) - 0.5) * length)
            return stroked(point, first, last, thickness * 0.5, thickness * 0.5)
        case Grid():
            width, height, line, offset = map(inlet, Grid.__struct_fields__)
            return Bounded(distance=lattice(point, width, height, offset, line)[0], width=line)
        case Bar(box=box):
            box_field, completion_field, growth = Bar.__struct_fields__
            width, height, _ = (inlet(box_field, member) for member in Box.__struct_fields__)
            completion, x = inlet(completion_field), point["X"]
            half = width * 0.5
            clips = (x - math("MULTIPLY_ADD", completion, 2.0, -1.0) * half, abs(x) - completion * half, (1.0 - completion) * half - abs(x))
            clip = shell.menu(*path, growth, cases={member.label: term for member, term in zip(Growth, clips)}, kind="FLOAT")
            length = width * completion
            filled = math("MINIMUM", switched(msgspec.structs.replace(clip, socket=Growth.EDGES.label), length, length * 0.5), height)
            return Bounded(distance=math("MAXIMUM", primitive(shell, (*path, box_field), box, point).distance, clip), width=filled)
        case Ring(ellipse=ellipse):
            ellipse_field, band = Ring.__struct_fields__
            boundary = primitive(shell, (*path, ellipse_field), ellipse, point)
            half = boundary.width * inlet(band) * 0.25
            return Bounded(distance=abs(boundary.distance + half) - half, width=half * 2.0)


# --- [FIELDS]


def chosen[R: Effect](shell: Shell[R], path: KeyPath, cases: Mapping[str, Bounded]) -> Expr:
    """Selects one case's field through the path's one Menu Switch, distance in X and width in Y beside each item's boolean."""
    return shell.menu(*path, cases={name: case.packed for name, case in cases.items()}, kind="VECTOR")


def figured[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> Bounded:
    """Returns positioned primitive field in greater-side units."""
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


def combined[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, field: Boolean | Morph) -> Bounded:
    """Returns boolean or morph field of two figures."""
    match field:
        case Morph():
            start, end, progress = Morph.__struct_fields__
            first, last, share = figured(shell, (*path, start), frame), figured(shell, (*path, end), frame), shell.input(*path, progress)
            return Bounded(distance=mixed(share, first.distance, last.distance), width=mixed(share, first.width, last.width))
        case Boolean():
            operation, base, cutter, smoothness = Boolean.__struct_fields__
            a, b, seam = figured(shell, (*path, base), frame), figured(shell, (*path, cutter), frame), shell.input(*path, smoothness)
            signs = {Operation.UNION: (1.0, 1.0), Operation.SUBTRACT: (-1.0, 1.0), Operation.INTERSECT: (-1.0, -1.0)}
            widths = {Operation.UNION: math("MAXIMUM", a.width, b.width), Operation.SUBTRACT: a.width, Operation.INTERSECT: math("MINIMUM", a.width, b.width)}

            def joined(member: Operation) -> Expr:
                outer, inner = signs[member]
                left, right = a.distance * outer, b.distance * inner
                overlap = math("MAXIMUM", seam - abs(left - right), 0.0)
                return (math("MINIMUM", left, right) - overlap * overlap / (seam * 4.0)) * outer

            return Bounded.of(chosen(shell, (*path, operation), {member.label: Bounded(distance=joined(member), width=widths[member]) for member in Operation}))


def windowed[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> Bounded:
    """Returns centered rounded aperture fitted to displayed frame."""
    aspect_field, inset, roundness = Window.__struct_fields__
    extent = frame.extent
    aspect = switched(shell.gate(*path, aspect_field), extent["X"] / extent["Y"], shell.input(*path, aspect_field))
    width = math("MINIMUM", extent["Y"] * aspect, extent["X"])
    height = width / aspect
    margin = math("MINIMUM", width, height) * shell.input(*path, inset)
    return boxed(frame.point - extent * 0.5, width - margin, height - margin, shell.input(*path, roundness))


def drawn(stream: Scalar, time: Scalar, seed: Scalar) -> Expr:
    """Draws one White Noise lattice value at the origin cell per stream, time, and seed."""
    return sample(White(spacing=1.0), xyz(), time, seed, stream)


@grouped(item_key(Shapes, Lightning, "midpoint"), f"{Lightning.title} Midpoint")
def bisection(start: Point, end: Point, stream: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
    """Midpoint of a bolt segment pushed along its normal by a signed draw."""
    return mixed(0.5, start, end) + rotated(end - start, pi / 2.0) * (math("MULTIPLY_ADD", drawn(stream, time, seed), 2.0, -1.0) * displacement)


@cache
def channel(depth: int) -> Group[[Point, Point, Point, Point, Scalar, Scalar, Scalar, Scalar]]:
    """Bolt channel group of `depth` levels, each splitting at a drawn midpoint between end radii in `radii` X and Y, its field `packed`."""

    def body(point: Point, start: Point, end: Point, radii: Point, stream: Scalar, time: Scalar, seed: Scalar, displacement: Scalar) -> Expr:
        match depth:
            case 0:
                return stroked(point, start, end, radii["X"], radii["Y"]).packed
            case _:
                half = float(2 ** (depth - 1))
                split = bisection(start, end, math("ADD", stream, half), time, seed, displacement)
                middle = mixed(0.5, radii["X"], radii["Y"])
                lower = channel(depth - 1)
                left = Bounded.of(lower(point, start, split, xyz(radii["X"], middle), stream, time, seed, displacement))
                return left.nearer(Bounded.of(lower(point, split, end, xyz(middle, radii["Y"]), math("ADD", stream, half), time, seed, displacement))).packed

    return grouped(item_key(Shapes, Lightning, "channel", depth), f"{Lightning.title} Channel {depth}")(body)


def lightning[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, shape: Lightning) -> Bounded:
    """Builds dyadic channel with tapered forks from its first subdivision points."""
    inlet = partial(shell.input, *path)
    start_field, end_field, thickness, _, displacement_field, _, reach, spread, taper, motion, seed_field = Lightning.__struct_fields__
    speed, hold = Motion.__struct_fields__
    clock = clocked(inlet(motion, speed), inlet(motion, hold)) * 2.0
    radius = inlet(thickness) * 0.5
    start, end = inlet(start_field) * frame.extent, inlet(end_field) * frame.extent
    seed, displacement = inlet(seed_field), inlet(displacement_field)
    span = 2**shape.depth

    def subdivided(first: Expr, last: Expr, radii: Expr, stream: int) -> Bounded:
        return Bounded.of(channel(shape.depth)(frame.point, first, last, radii, float(stream), clock, seed, displacement))

    @cache
    def vertex(index: int) -> Expr:
        low = index & -index
        return start if index == 0 else end if index == span else bisection(vertex(index - low), vertex(index + low), float(index), clock, seed, displacement)

    def fork(index: int) -> Bounded:
        level = (index + 1).bit_length()
        cell, remainder = divmod((2 * (index + 1 - 2 ** (level - 1)) + 1) * span, 2**level)
        stream = span + index * (span + 2)
        root = mixed(remainder / 2**level, vertex(cell), vertex(cell + 1))
        angle = (drawn(float(stream), clock, seed) + 0.5) * inlet(spread) * (-1.0 if index % 2 else 1.0)
        tip = root + vector("SCALE", rotated(end - start, angle), (drawn(float(stream + 1), clock, seed) + 0.5) * inlet(reach))
        return subdivided(root, tip, xyz(radius * inlet(taper)), stream + 2)

    return reduce(Bounded.nearer, (fork(index) for index in range(shape.branches)), subdivided(start, end, xyz(radius, radius), 0))


def coverage(distance: Scalar, width: Option[Scalar], softness: Scalar, pixel: Scalar, growth: Scalar = 0.0) -> Expr:
    """Builds grown distance-ramp coverage, width-capped when bounded.

    Args:
        distance: Signed region distance in greater-side frame shares.
        width: Local narrowest width, absent for an unbounded region.
        softness: Authored ramp width in distance units.
        pixel: One displayed pixel in distance units.
        growth: Outward region growth in distance units.

    Returns:
        Approximate covered fraction.
    """
    span = math("ADD", softness, pixel)
    ramp = math("SUBTRACT", 0.5, math("SUBTRACT", distance, growth) / span, clamp=True)
    return width.map(lambda bound: math("MINIMUM", ramp, math("MULTIPLY_ADD", growth, 2.0, bound) / span)).default_value(ramp)


def fielded(shell: Shell[Shapes], frame: FrameSpace, growths: tuple[Scalar, ...] = (0.0,)) -> tuple[Expr, ...]:
    """Returns grown coverages of the one selected field under one noisy boundary, the whole frame covered under `Frame`."""
    field, _, _, _, softness_field, roughness, edge_noise, seed, invert_field = Shapes.__struct_fields__
    path = (field,)

    def geometric(region: Region) -> Bounded:
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
    ((noise, _),) = sampled(shell, (edge_noise,), frame.point, 0.0, shell.input(seed))
    displaced = region.distance - math("MULTIPLY_ADD", noise, 2.0, -1.0) * shell.input(roughness)
    signed = switched(invert, displaced, -displaced)
    whole = msgspec.structs.replace(selection, socket=Frame.title, shape="FLOAT")
    return tuple(
        switched(whole, switched(invert, coverage(signed, Some(region.width), softness, pixel, growth), coverage(signed, Nothing, softness, pixel, growth)), 1.0 - invert) for growth in growths
    )


# --- [PAINT]


def graded[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, record: Gradient) -> tuple[Expr, Expr]:
    """Returns ramp color along the selected geometric parameter and its coverage past the ramp ends."""
    geometry_field, extension, ramp, _, phase, motion = Gradient.__struct_fields__
    speed, hold = Motion.__struct_fields__
    geometry = (*path, geometry_field)

    def parameter(shape: Geometry) -> Expr:
        inlet = partial(shell.input, *geometry, type(shape))
        match shape:
            case Linear():
                origin, angle, length, sweep = map(inlet, Linear.__struct_fields__)
                local = turned(frame, origin, angle)
                return (local["X"] - abs(local["Y"]) * sweep) / length
            case Radial():
                origin, radius, aspect, angle = map(inlet, Radial.__struct_fields__)
                return vector("LENGTH", turned(frame, origin, angle) / xyz(aspect, 1.0)) / radius
            case Angular():
                origin, angle = map(inlet, Angular.__struct_fields__)
                local = turned(frame, origin, angle)
                return math("ARCTAN2", local["Y"], local["X"]) / tau
            case Diamond():
                origin, angle, radius = map(inlet, Diamond.__struct_fields__)
                local = abs(turned(frame, origin, angle))
                return (local["X"] + local["Y"]) / radius
            case Rectangular():
                origin, angle, width, height = map(inlet, Rectangular.__struct_fields__)
                local = abs(turned(frame, origin, angle) / xyz(width, height))
                return math("MAXIMUM", local["X"], local["Y"])

    coordinate = shell.menu(
        *geometry, cases={type(shape).title: parameter(shape) for shape in shell.cases[geometry] if isinstance(shape, Linear | Radial | Angular | Diamond | Rectangular)}, kind="FLOAT"
    )
    cycles = coordinate + (shell.input(*path, phase) / tau - clocked(shell.input(*path, motion, speed), shell.input(*path, motion, hold)))
    held = periodic(record.profile.value, cycles) if record.profile.enabled else coordinate
    folds = {Extension.REPEAT: math("FRACT", held), Extension.MIRROR: 1.0 - abs(held % 2.0 - 1.0)}
    clamped = Expr(kind=bpy.types.ShaderNodeClamp, inputs=(("Value", held),), socket="Result")
    extended = shell.menu(*path, extension, cases={member.label: folds.get(member, clamped) for member in Extension}, kind="FLOAT")
    inside = (1.0 - (held < 0.0)) * (1.0 - (held > 1.0))
    tint = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", extended),), content=Some(record.ramp), socket="Color", shape="RGBA").named(item_key(*path, ramp))
    return tint, msgspec.structs.replace(tint, socket="Alpha", shape="FLOAT") * switched(msgspec.structs.replace(extended, socket=Extension.CLIP.label), 1.0, inside)


def painted[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> tuple[Expr, Scalar]:
    """Returns frame-fixed paint color and its independent alpha coverage."""
    (color,) = Solid.__struct_fields__
    top_left, top_right, bottom_right, bottom_left = Corners.__struct_fields__
    grid, _, first, second, mortar, seed = Cells.__struct_fields__

    def paint(case: Paint) -> tuple[Expr, Scalar]:
        inlet = partial(shell.input, *path, type(case))
        match case:
            case Solid():
                return inlet(color), 1.0
            case Corners():
                across, up = (
                    Expr(kind=bpy.types.ShaderNodeMapRange, inputs=(("Value", frame.fraction[axis]),), members=(("interpolation_type", "SMOOTHERSTEP"), ("clamp", True)), socket="Result")
                    for axis in ("X", "Y")
                )
                return mixed(up, mixed(across, inlet(bottom_left), inlet(bottom_right)), mixed(across, inlet(top_left), inlet(top_right))), 1.0
            case Cells(pattern=pattern):
                width, height, line, offset = (inlet(grid, member) for member in Grid.__struct_fields__)
                distance, cell = lattice(frame.point, width, height, offset, line)
                match pattern:
                    case CellPattern.CHECKER:
                        weight = (cell["X"] + cell["Y"]) % 2.0
                    case CellPattern.RANDOM:
                        weight = sample(White(dimensions=3, spacing=1.0), cell, 0.0, inlet(seed), 0.0)
                return mixed(coverage(distance, Some(line), 0.0, 1.0 / frame.greater), mixed(weight, inlet(first), inlet(second)), inlet(mortar)), 1.0
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
    """Returns associated fill, outline, and independently emitted shape light."""
    _, fill, outline, emission, *_ = Shapes.__struct_fields__
    width, offset, color = Outline.__struct_fields__
    _, value = Gated.__struct_fields__
    ring = partial(shell.input, outline, value)
    region, inner, outer = fielded(shell, shell.space, (0.0, ring(offset), ring(offset) + ring(width)))
    field = region.named("shape_field")
    tint, painted_coverage = painted(shell, (fill, value), shell.space)
    filled = switched(shell.gate(fill), 0.0, field * painted_coverage)
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
    requested=Some(precise),
    inspected=(Inspected(key="shape_coverage", end=End(node="shape_field", socket="Output"), description="Inner shape field coverage"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Bounded",
    "bisection",
    "boxed",
    "channel",
    "chosen",
    "combined",
    "coverage",
    "drawn",
    "fielded",
    "figured",
    "graded",
    "lattice",
    "lightning",
    "painted",
    "polygonal",
    "primitive",
    "rotated",
    "shaped",
    "shapes",
    "stroked",
    "turned",
    "windowed",
]
