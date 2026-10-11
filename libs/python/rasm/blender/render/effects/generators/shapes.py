"""Native geometric fields, painted shapes, and shared distance-ramp coverage."""

from functools import partial
from math import cos, pi, sin, tau

import bpy
from expression import Nothing, Ok, Option, Some

from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import Clock, compared, End, ended, Fragment, framed, FrameSpace, item_key, masked, math, menu, mixed, node, Operand, selected, summed, switched, vector
from rasm.blender.render.effects.generators.noise import emitted, periodic, sample, sampled
from rasm.blender.render.effects.stack import CompositorEffect, Coverage, KeyPath, precise, Shell
from rasm.blender.render.effects.viewers import inspection
from rasm.imaging.chain import Effect
from rasm.imaging.effects.generators.noise import White
from rasm.imaging.effects.generators.shapes import (
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
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.geometry.stmaps import Extension
from rasm.imaging.parameters import Gated, Motion

# --- [TYPES] ----------------------------------------------------------------------------

type Region = Figure | Boolean | Morph | Lightning | Window


# --- [OPERATIONS] -----------------------------------------------------------------------


def boxed(key: Key, point: Operand, width: Operand, height: Operand, roundness: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns rounded-box distance and the shorter full side."""
    extents = node(f"{key}_size", bpy.types.ShaderNodeCombineXYZ, {"X": width, "Y": height}, {}).map(ended(f"{key}_size", "Vector")).bind(lambda size: vector(f"{key}_half", "SCALE", size, 0.5))
    narrow = math(f"{key}_width", "MINIMUM", width, height)
    radius = narrow.bind(lambda narrow: math(f"{key}_radius", "MULTIPLY", narrow, roundness)).bind(lambda radius: math(f"{key}_half_radius", "MULTIPLY", radius, 0.5))
    inset = paired(extents, radius.bind(lambda radius: vector(f"{key}_rounding", "MULTIPLY", radius, (1.0, 1.0, 0.0)))).bind(lambda parts: vector(f"{key}_inset", "SUBTRACT", *parts))
    corner = paired(vector(f"{key}_absolute", "ABSOLUTE", point), inset).bind(lambda parts: vector(f"{key}_corner", "SUBTRACT", *parts))
    outside = corner.bind(lambda corner: vector(f"{key}_outside", "MAXIMUM", corner, (0.0, 0.0, 0.0))).bind(lambda outside: vector(f"{key}_length", "LENGTH", outside))
    inside = (
        corner
        .bind(lambda corner: node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": corner}, {}))
        .bind(lambda axes: math(f"{key}_greater", "MAXIMUM", ended(f"{key}_axes", "X")(axes), End(node=f"{key}_axes", socket="Y")))
        .bind(lambda greater: math(f"{key}_inside", "MINIMUM", greater, 0.0))
    )
    distance = paired(paired(outside, inside).bind(lambda parts: math(f"{key}_joined", "ADD", *parts)), radius).bind(lambda parts: math(f"{key}_distance", "SUBTRACT", *parts))
    return paired(distance, narrow)


def stroked(key: Key, point: Operand, start: Operand, end: Operand, first: Operand, last: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns projected segment distance and its interpolated diameter."""
    chord = vector(f"{key}_chord", "SUBTRACT", end, start)
    offset = vector(f"{key}_offset", "SUBTRACT", point, start)
    fraction = (
        paired(chord, offset)
        .bind(lambda parts: paired(vector(f"{key}_dot", "DOT_PRODUCT", parts[1], parts[0]), vector(f"{key}_square", "DOT_PRODUCT", parts[0], parts[0])))
        .bind(lambda parts: math(f"{key}_fraction", "DIVIDE", *parts, clamp=True))
    )
    radius = fraction.bind(lambda fraction: mixed(f"{key}_radius", "FLOAT", fraction, first, last))
    nearest = paired(chord, fraction).bind(lambda parts: vector(f"{key}_along", "SCALE", *parts)).bind(lambda along: vector(f"{key}_nearest", "ADD", start, along))
    distance = nearest.bind(lambda nearest: vector(f"{key}_distance_vector", "SUBTRACT", point, nearest)).bind(lambda delta: vector(f"{key}_length", "LENGTH", delta))
    return paired(paired(distance, radius).bind(lambda parts: math(f"{key}_distance", "SUBTRACT", *parts)), radius.bind(lambda radius: math(f"{key}_width", "MULTIPLY", radius, 2.0)))


def lattice(key: Key, point: Operand, width: Operand, height: Operand, offset: Operand, line: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns mortar distance and the shared integer-cell coordinate."""
    axes = node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {})
    row = axes.bind(lambda axes: math(f"{key}_row_parameter", "DIVIDE", ended(f"{key}_axes", "Y")(axes), height)).bind(lambda parameter: math(f"{key}_row", "FLOOR", parameter))
    shift = (
        row
        .bind(lambda row: math(f"{key}_parity", "FLOORED_MODULO", row, 2.0))
        .bind(lambda parity: math(f"{key}_course", "MULTIPLY", parity, offset))
        .bind(lambda course: math(f"{key}_shift", "MULTIPLY", course, width))
    )
    x = paired(axes, shift).bind(lambda parts: math(f"{key}_x", "SUBTRACT", ended(f"{key}_axes", "X")(parts[0]), parts[1]))
    normalized = (
        paired(x, axes)
        .bind(lambda parts: paired(math(f"{key}_u", "DIVIDE", parts[0], width), math(f"{key}_v", "DIVIDE", ended(f"{key}_axes", "Y")(parts[1]), height)))
        .bind(lambda parts: node(f"{key}_parameter", bpy.types.ShaderNodeCombineXYZ, {"X": parts[0], "Y": parts[1]}, {}))
        .map(ended(f"{key}_parameter", "Vector"))
    )
    cell = normalized.bind(lambda parameter: vector(f"{key}_cell", "FLOOR", parameter))
    nearest = normalized.bind(lambda parameter: vector(f"{key}_fraction", "FRACTION", parameter)).bind(
        lambda fraction: vector(f"{key}_opposite", "SUBTRACT", (1.0, 1.0, 0.0), fraction).bind(lambda opposite: vector(f"{key}_nearest", "MINIMUM", fraction, opposite))
    )
    scaled = paired(nearest, node(f"{key}_spacing", bpy.types.ShaderNodeCombineXYZ, {"X": width, "Y": height}, {}).map(ended(f"{key}_spacing", "Vector"))).bind(
        lambda parts: vector(f"{key}_distances", "MULTIPLY", *parts)
    )
    distance = (
        scaled
        .bind(lambda distances: node(f"{key}_distances_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": distances}, {}))
        .bind(lambda axes: math(f"{key}_nearest_line", "MINIMUM", ended(f"{key}_distances_axes", "X")(axes), End(node=f"{key}_distances_axes", socket="Y")))
        .bind(lambda distance: math(f"{key}_distance", "MULTIPLY_ADD", line, -0.5, distance))
    )
    return paired(distance, cell)


def polygonal(key: Key, point: Operand, vertices: tuple[Operand, ...], rounding: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Returns the rounded boundary distance of an ordered polygon."""
    axes = node(f"{key}_point", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {})

    def crossing(index: int, at: Fragment, first: Fragment, last: Fragment) -> BlenderResult[tuple[Fragment, End]]:
        prefix = f"{key}_{index}"
        px, py = ended(f"{key}_point", "X")(at), End(node=f"{key}_point", socket="Y")
        ax, ay = ended(f"{prefix}_start", "X")(first), End(node=f"{prefix}_start", socket="Y")
        bx, by = ended(f"{prefix}_end", "X")(last), End(node=f"{prefix}_end", socket="Y")
        straddled = (
            paired(compared(f"{prefix}_start_above", "GREATER_THAN", ay, py), compared(f"{prefix}_end_above", "GREATER_THAN", by, py))
            .bind(lambda above: node(f"{prefix}_straddled", bpy.types.FunctionNodeBooleanMath, {"Boolean": above[0], "Boolean_001": above[1]}, {"operation": "XOR"}))
            .map(ended(f"{prefix}_straddled", "Boolean"))
        )
        along = paired(math(f"{prefix}_dy", "SUBTRACT", by, ay), math(f"{prefix}_y", "SUBTRACT", py, ay)).bind(lambda sides: math(f"{prefix}_along", "DIVIDE", sides[1], sides[0]))
        edge = (
            paired(along, math(f"{prefix}_dx", "SUBTRACT", bx, ax)).bind(lambda parts: math(f"{prefix}_x", "MULTIPLY_ADD", *parts, ax)).bind(lambda x: compared(f"{prefix}_right", "LESS_THAN", px, x))
        )
        return (
            paired(straddled, edge)
            .bind(lambda values: node(f"{prefix}_crossing", bpy.types.FunctionNodeBooleanMath, {"Boolean": values[0], "Boolean_001": values[1]}, {"operation": "AND"}))
            .map(ended(f"{prefix}_crossing", "Boolean"))
        )

    edges = tuple(zip(vertices, (*vertices[1:], vertices[0])))
    distances = accumulated(stroked(f"{key}_edge_{index}", point, start, end, 0.0, 0.0).map(lambda field: field[0]) for index, (start, end) in enumerate(edges))
    crossings = accumulated(
        paired(
            axes, paired(node(f"{key}_{index}_start", bpy.types.ShaderNodeSeparateXYZ, {"Vector": start}, {}), node(f"{key}_{index}_end", bpy.types.ShaderNodeSeparateXYZ, {"Vector": end}, {}))
        ).bind(lambda parts: crossing(index, parts[0], *parts[1]))
        for index, (start, end) in enumerate(edges)
    )

    def joined(parts: tuple[tuple[tuple[Fragment, End], ...], tuple[tuple[Fragment, End], ...]]) -> BlenderResult[tuple[Fragment, End]]:
        distances, crossings = parts
        first_distance, *following_distances = distances
        nearest = Ok(first_distance)
        for index, distance in enumerate(following_distances, 1):
            nearest = nearest.bind(lambda nearest: math(f"{key}_nearest_{index}", "MINIMUM", nearest, distance))
        inside = summed(f"{key}_count", crossings, kind="FLOAT").bind(lambda count: math(f"{key}_inside", "FLOORED_MODULO", count, 2.0))
        negative = nearest.bind(lambda nearest: math(f"{key}_negative", "MULTIPLY", nearest, -1.0))
        return (
            paired(paired(nearest, negative), inside)
            .bind(lambda parts: masked(f"{key}_signed", "FLOAT", parts[1], *parts[0]))
            .bind(lambda signed: math(f"{key}_distance", "SUBTRACT", signed, rounding))
        )

    return paired(distances, crossings).bind(joined)


def primitive[R: Effect](shell: Shell[R], path: KeyPath, shape: Primitive, point: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns the selected primitive's local distance and bounded width."""
    key = item_key(*path)
    inlet = partial(shell.socket, *path)
    match shape:
        case Box():
            return boxed(key, point, inlet(Box.width.__name__), inlet(Box.height.__name__), inlet(Box.roundness.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case Ellipse():
            exponent = inlet(Ellipse.exponent.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            half = (
                node(f"{key}_size", bpy.types.ShaderNodeCombineXYZ, {"X": inlet(Ellipse.width.__name__), "Y": inlet(Ellipse.height.__name__)}, {})  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .map(ended(f"{key}_size", "Vector"))
                .bind(lambda size: vector(f"{key}_half", "SCALE", size, 0.5))
            )
            narrow = math(f"{key}_width", "MINIMUM", inlet(Ellipse.width.__name__), inlet(Ellipse.height.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            normalized = half.bind(lambda half: vector(f"{key}_normalized", "DIVIDE", point, half)).bind(lambda normalized: vector(f"{key}_absolute", "ABSOLUTE", normalized))
            powers = normalized.bind(lambda normalized: vector(f"{key}_powers", "POWER", normalized, exponent))
            level = (
                powers
                .bind(lambda powers: node(f"{key}_powers_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": powers}, {}))
                .bind(lambda axes: math(f"{key}_sum", "ADD", ended(f"{key}_powers_axes", "X")(axes), End(node=f"{key}_powers_axes", socket="Y")))
                .bind(lambda total: math(f"{key}_reciprocal", "DIVIDE", 1.0, exponent).bind(lambda reciprocal: math(f"{key}_level", "POWER", total, reciprocal)))
            )
            gradient = (
                paired(normalized, math(f"{key}_gradient_exponent", "SUBTRACT", exponent, 1.0))
                .bind(lambda parts: vector(f"{key}_gradient_powers", "POWER", *parts))
                .bind(lambda powers: half.bind(lambda half: vector(f"{key}_gradient_scale", "DIVIDE", powers, half)))
            )
            derivative = (
                paired(level, math(f"{key}_outer_exponent", "SUBTRACT", 1.0, exponent))
                .bind(lambda parts: math(f"{key}_outer", "POWER", *parts))
                .bind(lambda outer: gradient.bind(lambda gradient: vector(f"{key}_gradient", "SCALE", gradient, outer)))
                .bind(lambda gradient: vector(f"{key}_slope", "LENGTH", gradient))
            )
            distance = paired(level, derivative).bind(lambda parts: math(f"{key}_edge", "SUBTRACT", parts[0], 1.0).bind(lambda edge: math(f"{key}_distance", "DIVIDE", edge, parts[1])))
            center = vector(f"{key}_radius", "LENGTH", point).bind(lambda radius: compared(f"{key}_center", "EQUAL", radius, 0.0))
            middle = narrow.bind(lambda width: math(f"{key}_middle", "MULTIPLY", width, -0.5))
            return paired(paired(distance, middle).bind(lambda distances: center.bind(lambda center: masked(f"{key}_centered", "FLOAT", center, *distances))), narrow)
        case Cross():
            length = inlet(Cross.length.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            roundness = inlet(Cross.roundness.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            width = math(f"{key}_width", "MULTIPLY", length, inlet(Cross.thickness.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            horizontal = math(f"{key}_horizontal", "MULTIPLY", length, inlet(Cross.aspect.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            bars = paired(width, horizontal).bind(lambda parts: paired(boxed(f"{key}_across", point, parts[1], parts[0], roundness), boxed(f"{key}_up", point, parts[0], length, roundness)))
            return bars.bind(lambda parts: paired(math(f"{key}_distance", "MINIMUM", parts[0][0], parts[1][0]), math(f"{key}_narrow", "MAXIMUM", parts[0][1], parts[1][1])))
        case Triangle():
            vertices = tuple(inlet(member) for member in (Triangle.a.__name__, Triangle.b.__name__, Triangle.c.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            lengths = accumulated(vector(f"{key}_side_{index}", "DISTANCE", vertices[(index + 1) % 3], vertices[(index + 2) % 3]) for index in range(3))
            area = (
                paired(vector(f"{key}_ab", "SUBTRACT", vertices[1], vertices[0]), vector(f"{key}_ac", "SUBTRACT", vertices[2], vertices[0]))
                .bind(lambda edges: vector(f"{key}_area_vector", "CROSS_PRODUCT", *edges))
                .bind(lambda area: vector(f"{key}_area", "LENGTH", area))
            )

            perimeter = lengths.bind(lambda lengths: summed(f"{key}_perimeter", lengths, kind="FLOAT"))
            weighted = lengths.bind(lambda lengths: accumulated(vector(f"{key}_weighted_{index}", "SCALE", vertex, length) for index, (vertex, length) in enumerate(zip(vertices, lengths))))
            center = paired(weighted.bind(lambda weighted: summed(f"{key}_weighted_sum", weighted, kind="VECTOR")), perimeter).bind(lambda parts: vector(f"{key}_center", "DIVIDE", *parts))
            fraction = area.bind(lambda area: compared(f"{key}_nondegenerate", "GREATER_THAN", area, 0.0)).bind(
                lambda positive: switched(f"{key}_roundness", "FLOAT", positive, 0.0, inlet(Triangle.roundness.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
            shrunk = paired(center, fraction).bind(lambda parts: accumulated(mixed(f"{key}_vertex_{index}", "VECTOR", parts[1], vertex, parts[0]) for index, vertex in enumerate(vertices)))
            return paired(paired(area, perimeter).bind(lambda parts: math(f"{key}_incircle", "DIVIDE", *parts)), shrunk).bind(
                lambda parts: paired(
                    math(f"{key}_rounding", "MULTIPLY", parts[0], inlet(Triangle.roundness.__name__)).bind(lambda rounding: polygonal(key, point, parts[1], rounding)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    math(f"{key}_width", "MULTIPLY", parts[0], 2.0),
                )
            )
        case Polygon():
            sides = shape.sides
            radius = inlet(Polygon.radius.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            rounding = math(f"{key}_rounding", "MULTIPLY", radius, inlet(Polygon.roundness.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            outer = paired(Ok(radius), rounding).bind(lambda parts: math(f"{key}_outer", "SUBTRACT", *parts))
            inner = (
                math(f"{key}_remaining", "SUBTRACT", 1.0, inlet(Polygon.inset.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .bind(lambda remaining: math(f"{key}_inner_fraction", "MULTIPLY", remaining, cos(pi / sides)))
                .bind(lambda fraction: outer.bind(lambda outer: math(f"{key}_inner", "MULTIPLY", outer, fraction)))
            )
            distance = paired(
                accumulated(
                    (outer if index % 2 == 0 else inner).bind(
                        lambda radius: vector(f"{key}_vertex_{index}", "SCALE", (cos(pi / 2.0 + pi * index / sides), sin(pi / 2.0 + pi * index / sides), 0.0), radius)
                    )
                    for index in range(2 * sides)
                ),
                rounding,
            ).bind(lambda parts: polygonal(key, point, *parts))
            width = paired(inner, rounding).bind(lambda parts: math(f"{key}_inner_bound", "ADD", *parts)).bind(lambda radius: math(f"{key}_width", "MULTIPLY", radius, 2.0))
            return paired(distance, width)
        case Heart():
            lobe_radius = 2.0**0.5 / 4.0
            unit_height = 0.75 + lobe_radius
            scale = math(f"{key}_scale", "DIVIDE", inlet(Heart.height.__name__), unit_height)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            located = scale.bind(lambda scale: vector(f"{key}_normalized", "DIVIDE", point, scale)).bind(lambda point: vector(f"{key}_centered", "ADD", point, (0.0, unit_height / 2.0, 0.0)))
            absolute = (
                located
                .bind(lambda point: node(f"{key}_local_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}))
                .bind(
                    lambda axes: math(f"{key}_absolute_x", "ABSOLUTE", ended(f"{key}_local_axes", "X")(axes)).bind(
                        lambda x: node(f"{key}_folded", bpy.types.ShaderNodeCombineXYZ, {"X": x, "Y": (axes, End(node=f"{key}_local_axes", socket="Y"))}, {})
                    )
                )
                .map(ended(f"{key}_folded", "Vector"))
            )

            axes = absolute.bind(lambda point: node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}))
            sumxy = axes.bind(lambda axes: math(f"{key}_sum", "ADD", ended(f"{key}_axes", "X")(axes), End(node=f"{key}_axes", socket="Y")))
            lobe = (
                absolute
                .bind(lambda point: vector(f"{key}_lobe_offset", "SUBTRACT", point, (0.25, 0.75, 0.0)))
                .bind(lambda offset: vector(f"{key}_lobe_radius", "LENGTH", offset))
                .bind(lambda radius: math(f"{key}_lobe", "SUBTRACT", radius, lobe_radius))
            )
            cusp = absolute.bind(lambda point: vector(f"{key}_cusp_offset", "SUBTRACT", point, (0.0, 1.0, 0.0))).bind(lambda offset: vector(f"{key}_cusp", "LENGTH", offset))
            diagonal = (
                sumxy
                .bind(lambda total: math(f"{key}_positive", "MAXIMUM", total, 0.0))
                .bind(lambda positive: math(f"{key}_half", "MULTIPLY", positive, 0.5))
                .bind(lambda half: vector(f"{key}_diagonal", "MULTIPLY", half, (1.0, 1.0, 0.0)))
            )
            tip = paired(absolute, diagonal).bind(lambda parts: vector(f"{key}_tip_offset", "SUBTRACT", *parts)).bind(lambda offset: vector(f"{key}_tip", "LENGTH", offset))
            sign = axes.bind(lambda axes: math(f"{key}_side", "SUBTRACT", ended(f"{key}_axes", "X")(axes), End(node=f"{key}_axes", socket="Y"))).bind(lambda side: math(f"{key}_sign", "SIGN", side))
            lower = paired(paired(cusp, tip).bind(lambda parts: math(f"{key}_lower_radius", "MINIMUM", *parts)), sign).bind(lambda parts: math(f"{key}_lower", "MULTIPLY", *parts))
            chosen = paired(paired(lower, lobe), sumxy.bind(lambda total: compared(f"{key}_upper", "GREATER_THAN", total, 1.0))).bind(
                lambda parts: masked(f"{key}_boundary", "FLOAT", parts[1], *parts[0])
            )
            return paired(paired(chosen, scale).bind(lambda parts: math(f"{key}_distance", "MULTIPLY", *parts)), Ok((Fragment(), inlet(Heart.height.__name__))))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case Segment():
            ends = accumulated(
                math(f"{key}_{edge}", "MINIMUM" if edge == "start" else "MAXIMUM", inlet(Segment.start.__name__), inlet(Segment.end.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .bind(lambda fraction: math(f"{key}_{edge}_centered", "SUBTRACT", fraction, 0.5))
                .bind(lambda fraction: math(f"{key}_{edge}_offset", "MULTIPLY", fraction, inlet(Segment.length.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .bind(lambda offset: node(f"{key}_{edge}_point", bpy.types.ShaderNodeCombineXYZ, {"X": offset}, {}))
                .map(ended(f"{key}_{edge}_point", "Vector"))
                for edge in ("start", "end")
            )
            return paired(ends, math(f"{key}_radius", "MULTIPLY", inlet(Segment.thickness.__name__), 0.5)).bind(lambda parts: stroked(key, point, parts[0][0], parts[0][1], parts[1], parts[1]))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case Grid():
            return lattice(key, point, inlet(Grid.width.__name__), inlet(Grid.height.__name__), inlet(Grid.offset.__name__), inlet(Grid.line.__name__)).map(  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                lambda parts: (parts[0], (Fragment(), inlet(Grid.line.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
        case Bar():
            box = (*path, Bar.box.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            boundary = boxed(f"{key}_box", point, shell.socket(*box, Box.width.__name__), shell.socket(*box, Box.height.__name__), shell.socket(*box, Box.roundness.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            half = math(f"{key}_half", "MULTIPLY", shell.socket(*box, Box.width.__name__), 0.5)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            x = node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}).map(ended(f"{key}_axes", "X"))
            completion = inlet(Bar.completion.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            clips = paired(half, x).bind(
                lambda parts: accumulated((
                    math(f"{key}_start_share", "MULTIPLY_ADD", completion, 2.0, -1.0)
                    .bind(lambda share: math(f"{key}_start_edge", "MULTIPLY", share, parts[0]))
                    .bind(lambda edge: math(f"{key}_start", "SUBTRACT", parts[1], edge)),
                    math(f"{key}_absolute", "ABSOLUTE", parts[1]).bind(
                        lambda absolute: math(f"{key}_center_edge", "MULTIPLY", completion, parts[0]).bind(lambda edge: math(f"{key}_center", "SUBTRACT", absolute, edge))
                    ),
                    math(f"{key}_remaining", "SUBTRACT", 1.0, completion)
                    .bind(lambda share: math(f"{key}_edges_edge", "MULTIPLY", share, parts[0]))
                    .bind(lambda edge: math(f"{key}_edges_absolute", "ABSOLUTE", parts[1]).bind(lambda absolute: math(f"{key}_edges", "SUBTRACT", edge, absolute))),
                ))
            )
            chosen = clips.bind(
                lambda clips: menu(f"{key}_growth", "FLOAT", inlet(Bar.growth.__name__), shell.items[*path, Bar.growth.__name__], dict(zip((growth.label for growth in Growth), clips)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
            filled = (
                math(f"{key}_filled_length", "MULTIPLY", shell.socket(*box, Box.width.__name__), completion)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .bind(
                    lambda length: math(f"{key}_half_length", "MULTIPLY", length, 0.5).bind(
                        lambda half: switched(f"{key}_piece_length", "FLOAT", End(node=f"{key}_growth", socket=Growth.EDGES.label), length, half)
                    )
                )
                .bind(lambda length: math(f"{key}_width", "MINIMUM", length, shell.socket(*box, Box.height.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
            return paired(paired(boundary, chosen).bind(lambda parts: math(f"{key}_distance", "MAXIMUM", parts[0][0], parts[1])), filled)
        case Ring():
            boundary = primitive(shell, (*path, Ring.ellipse.__name__), shape.ellipse, point)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            half = boundary.bind(lambda boundary: math(f"{key}_thickness", "MULTIPLY", boundary[1], inlet(Ring.thickness.__name__))).bind(  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                lambda thickness: math(f"{key}_half_thickness", "MULTIPLY", thickness, 0.25)
            )
            return paired(boundary, half).bind(
                lambda parts: paired(
                    math(f"{key}_middle", "ADD", parts[0][0], parts[1])
                    .bind(lambda middle: math(f"{key}_absolute", "ABSOLUTE", middle))
                    .bind(lambda absolute: math(f"{key}_distance", "SUBTRACT", absolute, parts[1])),
                    math(f"{key}_width", "MULTIPLY", parts[1], 2.0),
                )
            )


def figured[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns a positioned primitive's field in displayed greater-side units."""
    key = item_key(*path)
    at = (*path, Figure.primitive.__name__)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    cases = tuple(shape for shape in shell.cases[at] if isinstance(shape, Box | Ellipse | Triangle | Cross | Heart | Segment | Bar | Grid | Polygon | Ring))
    extent = node(f"{key}_extent", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {}).bind(
        lambda axes: math(f"{key}_short", "MINIMUM", ended(f"{key}_extent", "X")(axes), End(node=f"{key}_extent", socket="Y"))
    )
    scale = extent.bind(lambda short: switched(f"{key}_scale", "FLOAT", shell.socket(*path, Figure.short_side.__name__), 1.0, short))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    local = (
        vector(f"{key}_anchor", "MULTIPLY", shell.socket(*path, Figure.position.__name__), frame.extent)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        .bind(lambda anchor: vector(f"{key}_offset", "SUBTRACT", frame.point, anchor))
        .bind(
            lambda offset: node(
                f"{key}_rotation",
                bpy.types.ShaderNodeVectorRotate,
                {"Vector": offset, "Angle": shell.socket(*path, Figure.rotation.__name__)},  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                {"rotation_type": "Z_AXIS", "invert": True},
            )
        )
        .map(ended(f"{key}_rotation", "Vector"))
    )
    point = paired(local, scale).bind(lambda parts: vector(f"{key}_point", "DIVIDE", *parts))

    labels = tuple(type(shape).title for shape in cases)
    fields = point.bind(lambda point: accumulated(primitive(shell, (*at, type(shape)), shape, point) for shape in cases))
    chosen = fields.bind(
        lambda fields: selected(f"{key}_primitive", shell.socket(*at), shell.items[at], {name: ("FLOAT", dict(zip(labels, column))) for name, column in zip(("distance", "width"), zip(*fields))})
    )
    return paired(chosen, scale).bind(lambda parts: paired(math(f"{key}_distance", "MULTIPLY", parts[0]["distance"], parts[1]), math(f"{key}_width", "MULTIPLY", parts[0]["width"], parts[1])))


def combined[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, field: Boolean | Morph) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns the boolean or interpolated field of two independent figures."""
    key = item_key(*path)
    match field:
        case Morph():
            return (
                paired(figured(shell, (*path, Morph.start.__name__), frame), figured(shell, (*path, Morph.end.__name__), frame))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .bind(
                    lambda fields: accumulated(
                        mixed(f"{key}_{name}", "FLOAT", shell.socket(*path, Morph.progress.__name__), fields[0][index], fields[1][index])  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        for index, name in enumerate(("distance", "width"))
                    )
                )
                .map(lambda fields: (fields[0], fields[1]))
            )
        case Boolean():
            inputs = paired(figured(shell, (*path, Boolean.base.__name__), frame), figured(shell, (*path, Boolean.cutter.__name__), frame))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            smoothness = shell.socket(*path, Boolean.smoothness.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

            def minimum(operation: Operation, a: Operand, b: Operand) -> BlenderResult[tuple[Fragment, End]]:
                prefix = f"{key}_{operation.name.lower()}"
                left = math(f"{prefix}_left", "MULTIPLY", a, 1.0 if operation is Operation.UNION else -1.0)
                right = math(f"{prefix}_right", "MULTIPLY", b, -1.0 if operation is Operation.INTERSECT else 1.0)
                terms = paired(left, right)
                smaller = terms.bind(lambda parts: math(f"{prefix}_minimum", "MINIMUM", *parts))
                square = (
                    terms
                    .bind(lambda parts: math(f"{prefix}_difference", "SUBTRACT", *parts))
                    .bind(lambda difference: math(f"{prefix}_absolute", "ABSOLUTE", difference))
                    .bind(lambda absolute: math(f"{prefix}_overlap", "SUBTRACT", smoothness, absolute))
                    .bind(lambda overlap: math(f"{prefix}_positive", "MAXIMUM", overlap, 0.0))
                    .bind(lambda positive: math(f"{prefix}_squared", "MULTIPLY", positive, positive))
                )
                correction = paired(square, math(f"{prefix}_denominator", "MULTIPLY", smoothness, 4.0)).bind(lambda parts: math(f"{prefix}_correction", "DIVIDE", *parts))
                return (
                    paired(smaller, correction)
                    .bind(lambda parts: math(f"{prefix}_smooth", "SUBTRACT", *parts))
                    .bind(lambda smooth: math(f"{prefix}_distance", "MULTIPLY", smooth, 1.0 if operation is Operation.UNION else -1.0))
                )

            def joined(
                fields: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], tuple[tuple[Fragment, End], tuple[Fragment, End]]],
            ) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
                (a, an), (b, bn) = fields
                distances = accumulated(minimum(operation, a, b) for operation in Operation)
                widths = accumulated((math(f"{key}_union_width", "MAXIMUM", an, bn), Ok(an), math(f"{key}_intersect_width", "MINIMUM", an, bn)))
                operation = (*path, Boolean.operation.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                labels = tuple(operation.label for operation in Operation)
                return (
                    paired(distances, widths)
                    .bind(
                        lambda parts: selected(
                            f"{key}_operation", shell.socket(*operation), shell.items[operation], {name: ("FLOAT", dict(zip(labels, fields))) for name, fields in zip(("distance", "width"), parts)}
                        )
                    )
                    .map(lambda fields: (fields["distance"], fields["width"]))
                )

            return inputs.bind(joined)


def windowed[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns the centered rounded aperture fitted to the displayed frame."""
    key = item_key(*path)
    axes = node(f"{key}_extent", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {})
    ratio = axes.bind(lambda axes: math(f"{key}_frame_aspect", "DIVIDE", ended(f"{key}_extent", "X")(axes), End(node=f"{key}_extent", socket="Y")))
    aspect = ratio.bind(lambda ratio: switched(f"{key}_aspect", "FLOAT", shell.socket(*path, Window.aspect.__name__), ratio, shell.socket(*path, Window.aspect.__name__, Gated.value.__name__)))  # type: ignore[attr-defined, misc]  # ty: ignore[invalid-attribute-access, unresolved-attribute]
    width = paired(axes, aspect).bind(
        lambda parts: math(f"{key}_fitted_width", "MULTIPLY", ended(f"{key}_extent", "Y")(parts[0]), parts[1]).bind(
            lambda width: math(f"{key}_width", "MINIMUM", width, ended(f"{key}_extent", "X")(parts[0]))
        )
    )
    height = paired(width, aspect).bind(lambda parts: math(f"{key}_height", "DIVIDE", *parts))
    inset = paired(width, height).bind(lambda parts: math(f"{key}_short", "MINIMUM", *parts)).bind(lambda short: math(f"{key}_inset", "MULTIPLY", short, shell.socket(*path, Window.inset.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    point = vector(f"{key}_center", "SCALE", frame.extent, 0.5).bind(lambda center: vector(f"{key}_point", "SUBTRACT", frame.point, center))
    return paired(paired(width, height), paired(inset, point)).bind(
        lambda parts: paired(math(f"{key}_inner_width", "SUBTRACT", parts[0][0], parts[1][0]), math(f"{key}_inner_height", "SUBTRACT", parts[0][1], parts[1][0])).bind(
            lambda size: boxed(key, parts[1][1], *size, shell.socket(*path, Window.roundness.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        )
    )


def lightning[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, shape: Lightning) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Builds the requested dyadic channel and tapered forks without topology caps."""
    key = item_key(*path)
    inlet = partial(shell.socket, *path)
    motion = Lightning.motion.__name__  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    clock = Clock(key=f"{key}_clock", speed=inlet(motion, Motion.speed.__name__), hold=inlet(motion, Motion.hold.__name__)).value.bind(lambda clock: math(f"{key}_redraw", "MULTIPLY", clock, 2.0))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    radius = math(f"{key}_radius", "MULTIPLY", inlet(Lightning.thickness.__name__), 0.5)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    start = vector(f"{key}_start", "MULTIPLY", inlet(Lightning.start.__name__), frame.extent)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    end = vector(f"{key}_end", "MULTIPLY", inlet(Lightning.end.__name__), frame.extent)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    span = 2**shape.depth

    def draw(prefix: Key, stream: int) -> BlenderResult[tuple[Fragment, End]]:
        return clock.bind(lambda time: sample(prefix, White(spacing=1.0), (0.0, 0.0, 0.0), time, inlet(Lightning.seed.__name__), stream, {}))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def channel(
        prefix: Key, first: tuple[Fragment, End], last: tuple[Fragment, End], first_radius: Operand, last_radius: Operand, stream: int
    ) -> tuple[dict[int, BlenderResult[tuple[Fragment, End]]], BlenderResult[tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...]]]:
        vertices = {0: Ok(first), span: Ok(last)}

        def midpoint(index: int, parents: tuple[Operand, Operand]) -> BlenderResult[tuple[Fragment, End]]:
            at = f"{prefix}_{index}"
            first, last = parents
            chord = vector(f"{at}_chord", "SUBTRACT", last, first)
            normal = chord.bind(lambda chord: node(f"{at}_normal", bpy.types.ShaderNodeVectorRotate, {"Vector": chord, "Angle": pi / 2.0}, {"rotation_type": "Z_AXIS"})).map(
                ended(f"{at}_normal", "Vector")
            )
            displacement = (
                draw(at, stream + index)
                .bind(lambda value: math(f"{at}_signed", "MULTIPLY_ADD", value, 2.0, -1.0))
                .bind(lambda signed: math(f"{at}_displacement", "MULTIPLY", signed, inlet(Lightning.displacement.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
            offset = paired(normal, displacement).bind(lambda parts: vector(f"{at}_offset", "SCALE", *parts))
            middle = mixed(f"{at}_middle", "VECTOR", 0.5, *parents)
            return paired(middle, offset).bind(lambda parts: vector(f"{at}_point", "ADD", *parts))

        for level in range(shape.depth):
            step = span >> level
            for index in range(step // 2, span, step):
                vertices[index] = paired(vertices[index - step // 2], vertices[index + step // 2]).bind(lambda parents: midpoint(index, parents))
        radii = tuple(mixed(f"{prefix}_{index}_radius", "FLOAT", index / span, first_radius, last_radius) for index in range(span + 1))
        pieces = accumulated(
            paired(paired(vertices[index], vertices[index + 1]), paired(radii[index], radii[index + 1])).bind(lambda parts: stroked(f"{prefix}_piece_{index}", frame.point, *parts[0], *parts[1]))
            for index in range(span)
        )
        return vertices, pieces

    def fork(
        index: int, vertices: dict[int, BlenderResult[tuple[Fragment, End]]], ends: tuple[tuple[Fragment, End], tuple[Fragment, End]], radius: tuple[Fragment, End]
    ) -> BlenderResult[tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...]]:
        prefix = f"{key}_fork_{index}"
        first, last = ends
        level = (index + 1).bit_length()
        numerator = 2 * (index + 1 - 2 ** (level - 1)) + 1
        cell, remainder = divmod(numerator * span, 2**level)
        stream = span + index * (span + 2)
        root = paired(vertices[cell], vertices[cell + 1]).bind(lambda points: mixed(f"{prefix}_root", "VECTOR", remainder / 2**level, *points))
        angle = (
            draw(f"{prefix}_turn", stream)
            .bind(lambda draw: math(f"{prefix}_turn_share", "ADD", draw, 0.5))
            .bind(lambda share: math(f"{prefix}_angle", "MULTIPLY", share, inlet(Lightning.spread.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(lambda angle: math(f"{prefix}_side", "MULTIPLY", angle, -1.0 if index % 2 else 1.0))
        )
        direction = (
            paired(vector(f"{prefix}_chord", "SUBTRACT", last, first), angle)
            .bind(lambda parts: node(f"{prefix}_direction", bpy.types.ShaderNodeVectorRotate, {"Vector": parts[0], "Angle": parts[1]}, {"rotation_type": "Z_AXIS"}))
            .map(ended(f"{prefix}_direction", "Vector"))
        )
        reach = (
            draw(f"{prefix}_length", stream + 1)
            .bind(lambda draw: math(f"{prefix}_length_share", "ADD", draw, 0.5))
            .bind(lambda share: math(f"{prefix}_reach", "MULTIPLY", share, inlet(Lightning.reach.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        )
        tip = paired(root, paired(direction, reach).bind(lambda parts: vector(f"{prefix}_offset", "SCALE", *parts))).bind(lambda parts: vector(f"{prefix}_tip", "ADD", *parts))
        width = math(f"{prefix}_radius", "MULTIPLY", radius, inlet(Lightning.taper.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return paired(paired(root, tip), width).bind(lambda parts: channel(prefix, *parts[0], parts[1], 0.0, stream + 2)[1])

    def nearest(pieces: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        first, *following = pieces
        result = Ok(first)
        for index, (distance, width) in enumerate(following, 1):
            nearer = result.bind(lambda previous: compared(f"{key}_nearer_{index}", "LESS_THAN", distance, previous[0]))
            result = paired(result, nearer).bind(
                lambda parts: paired(masked(f"{key}_distance_{index}", "FLOAT", parts[1], parts[0][0], distance), masked(f"{key}_width_{index}", "FLOAT", parts[1], parts[0][1], width))
            )
        return result

    def joined(parts: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], tuple[Fragment, End]]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        ends, radius = parts
        vertices, main = channel(f"{key}_main", *ends, radius, radius, 0)
        pieces = paired(main, accumulated(fork(index, vertices, ends, radius) for index in range(shape.branches))).map(lambda parts: (*parts[0], *(piece for branch in parts[1] for piece in branch)))
        return pieces.bind(nearest)

    return paired(paired(start, end), radius).bind(joined)


def fielded(shell: Shell[Shapes], frame: FrameSpace, growths: tuple[Operand, *tuple[Operand, ...]] = (0.0,)) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
    """Returns grown coverages from one field selector and one noisy boundary."""
    path = (Shapes.field.__name__,)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    cases = tuple(field for field in shell.cases[path] if isinstance(field, Figure | Boolean | Morph | Lightning | Window))
    pixel = math("shape_pixel", "DIVIDE", 1.0, frame.greater)
    displacement = (
        sampled(shell, (Shapes.edge_noise.__name__,), frame.point, 0.0, shell.socket(Shapes.seed.__name__))  # type: ignore[attr-defined, union-attr]  # ty: ignore[unresolved-attribute]
        .bind(lambda fields: math("shape_signed_noise", "MULTIPLY_ADD", fields[0][0], 2.0, -1.0))
        .bind(lambda signed: math("shape_roughness", "MULTIPLY", signed, shell.socket(Shapes.roughness.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    )

    def geometric(field: Region) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        at = (*path, type(field))
        match field:
            case Figure():
                return figured(shell, at, frame)
            case Boolean() | Morph():
                return combined(shell, at, frame, field)
            case Lightning():
                return lightning(shell, at, frame, field)
            case Window():
                return windowed(shell, at, frame)

    invert = shell.socket(Shapes.invert.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    softness = shell.socket(Shapes.softness.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def grown(
        prefix: Key, distance: tuple[Fragment, End], width: tuple[Fragment, End], displacement: tuple[Fragment, End], pixel: tuple[Fragment, End]
    ) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
        displaced = math(f"{prefix}_displaced", "SUBTRACT", distance, displacement)
        negative = displaced.bind(lambda distance: math(f"{prefix}_negative", "MULTIPLY", distance, -1.0))
        signed = paired(displaced, negative).bind(lambda parts: switched(f"{prefix}_signed", "FLOAT", invert, *parts))
        return accumulated(
            paired(
                signed.bind(lambda distance: coverage(f"{prefix}_{index}_bounded", distance, Some(width), softness, pixel, growth)),
                signed.bind(lambda distance: coverage(f"{prefix}_{index}_unbounded", distance, Nothing, softness, pixel, growth)),
            ).bind(lambda coverages: switched(f"{prefix}_{index}_coverage", "FLOAT", invert, *coverages))
            for index, growth in enumerate(growths)
        )

    labels = tuple(type(field).title for field in cases)
    coverages = paired(accumulated(geometric(field) for field in cases), paired(displacement, pixel)).bind(
        lambda parts: accumulated(grown(item_key("shape", type(field)), *values, *parts[1]) for field, values in zip(cases, parts[0]))
    )
    frame_coverage = math("shape_frame_coverage", "SUBTRACT", 1.0, invert)
    return (
        paired(coverages, frame_coverage)
        .bind(
            lambda parts: selected(
                "shape_field", shell.socket(*path), shell.items[path], {str(index): ("FLOAT", {Frame.title: parts[1], **dict(zip(labels, column))}) for index, column in enumerate(zip(*parts[0]))}
            )
        )
        .map(lambda outputs: tuple(outputs[str(index)] for index in range(len(growths))))
    )


def painted[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Returns frame-fixed paint color and its independent alpha coverage."""
    key = item_key(*path)
    pixel = math(f"{key}_pixel", "DIVIDE", 1.0, frame.greater)
    corners = (*path, Corners)
    fractions = node(f"{key}_fraction", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.fraction}, {})
    weights = fractions.bind(
        lambda axes: accumulated(
            node(
                weight,
                bpy.types.ShaderNodeMapRange,
                {"Value": ended(f"{key}_fraction", axis)(axes), "From Min": 0.0, "From Max": 1.0, "To Min": 0.0, "To Max": 1.0},
                {"interpolation_type": "SMOOTHERSTEP", "clamp": True},
            ).map(ended(weight, "Result"))
            for axis in ("X", "Y")
            for weight in (f"{key}_weight_{axis.lower()}",)
        )
    )
    corner_color = weights.bind(
        lambda weights: paired(
            mixed(f"{key}_bottom", "RGBA", weights[0], shell.socket(*corners, Corners.bottom_left.__name__), shell.socket(*corners, Corners.bottom_right.__name__)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            mixed(f"{key}_top", "RGBA", weights[0], shell.socket(*corners, Corners.top_left.__name__), shell.socket(*corners, Corners.top_right.__name__)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        ).bind(lambda colors: mixed(f"{key}_corners", "RGBA", weights[1], *colors))
    )
    cells = (*path, Cells)
    grid = (*cells, Cells.grid.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    cell_field = lattice(
        f"{key}_cells",
        frame.point,
        shell.socket(*grid, Grid.width.__name__),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        shell.socket(*grid, Grid.height.__name__),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        shell.socket(*grid, Grid.offset.__name__),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        shell.socket(*grid, Grid.line.__name__),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    )

    def colored_cells(fields: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        distance, cell = fields
        pattern = next(case.pattern for case in shell.cases[path] if isinstance(case, Cells))
        match pattern:
            case CellPattern.CHECKER:
                weight = (
                    node(f"{key}_cell_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": cell}, {})
                    .bind(lambda axes: math(f"{key}_cell_sum", "ADD", ended(f"{key}_cell_axes", "X")(axes), End(node=f"{key}_cell_axes", socket="Y")))
                    .bind(lambda total: math(f"{key}_checker", "FLOORED_MODULO", total, 2.0))
                )
            case CellPattern.RANDOM:
                weight = sample(f"{key}_cell_random", White(dimensions=3, spacing=1.0), cell, 0.0, shell.socket(*cells, Cells.seed.__name__), 0, {})  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        mortar = pixel.bind(lambda pixel: coverage(f"{key}_mortar", distance, Some(shell.socket(*grid, Grid.line.__name__)), 0.0, pixel))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        color = weight.bind(lambda weight: mixed(f"{key}_cell_color", "RGBA", weight, shell.socket(*cells, Cells.first.__name__), shell.socket(*cells, Cells.second.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        return paired(color, mortar).bind(lambda parts: mixed(f"{key}_cells_color", "RGBA", parts[1], parts[0], shell.socket(*cells, Cells.mortar.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    gradient = (*path, Gradient)
    geometry = (*gradient, Gradient.geometry.__name__)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    geometries = tuple(shape for shape in shell.cases[geometry] if isinstance(shape, Linear | Radial | Angular | Diamond | Rectangular))

    def parameter(shape: Geometry) -> BlenderResult[tuple[Fragment, End]]:
        kind = type(shape)
        prefix = item_key(*geometry, kind)
        at = (*geometry, kind)
        point = (
            vector(f"{prefix}_origin", "MULTIPLY", shell.socket(*at, kind.origin.__name__), frame.extent)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(lambda origin: vector(f"{prefix}_offset", "SUBTRACT", frame.point, origin))
            .bind(
                lambda offset: node(
                    f"{prefix}_rotation",
                    bpy.types.ShaderNodeVectorRotate,
                    {"Vector": offset, "Angle": shell.socket(*at, kind.angle.__name__)},  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    {"rotation_type": "Z_AXIS", "invert": True},
                )
            )
            .map(ended(f"{prefix}_rotation", "Vector"))
        )
        axes = point.bind(lambda point: node(f"{prefix}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}))
        match shape:
            case Linear():
                return axes.bind(
                    lambda axes: (
                        math(f"{prefix}_absolute_y", "ABSOLUTE", ended(f"{prefix}_axes", "Y")(axes))
                        .bind(lambda y: math(f"{prefix}_sweep", "MULTIPLY", y, shell.socket(*at, Linear.sweep.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        .bind(lambda sweep: math(f"{prefix}_along", "SUBTRACT", ended(f"{prefix}_axes", "X")(axes), sweep))
                    )
                ).bind(lambda along: math(f"{prefix}_parameter", "DIVIDE", along, shell.socket(*at, Linear.length.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            case Radial():
                return (
                    node(f"{prefix}_aspect", bpy.types.ShaderNodeCombineXYZ, {"X": shell.socket(*at, Radial.aspect.__name__), "Y": 1.0}, {})  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    .map(ended(f"{prefix}_aspect", "Vector"))
                    .bind(lambda aspect: point.bind(lambda point: vector(f"{prefix}_elliptical", "DIVIDE", point, aspect)))
                    .bind(lambda point: vector(f"{prefix}_radius", "LENGTH", point))
                    .bind(lambda radius: math(f"{prefix}_parameter", "DIVIDE", radius, shell.socket(*at, Radial.radius.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                )
            case Angular():
                return axes.bind(lambda axes: math(f"{prefix}_phase", "ARCTAN2", ended(f"{prefix}_axes", "Y")(axes), End(node=f"{prefix}_axes", socket="X"))).bind(
                    lambda phase: math(f"{prefix}_parameter", "DIVIDE", phase, tau)
                )
            case Diamond():
                return (
                    point
                    .bind(lambda point: vector(f"{prefix}_absolute", "ABSOLUTE", point))
                    .bind(lambda point: node(f"{prefix}_absolute_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}))
                    .bind(lambda axes: math(f"{prefix}_sum", "ADD", ended(f"{prefix}_absolute_axes", "X")(axes), End(node=f"{prefix}_absolute_axes", socket="Y")))
                    .bind(lambda total: math(f"{prefix}_parameter", "DIVIDE", total, shell.socket(*at, Diamond.radius.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                )
            case Rectangular():
                return (
                    node(f"{prefix}_size", bpy.types.ShaderNodeCombineXYZ, {"X": shell.socket(*at, Rectangular.width.__name__), "Y": shell.socket(*at, Rectangular.height.__name__)}, {})  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    .map(ended(f"{prefix}_size", "Vector"))
                    .bind(lambda size: point.bind(lambda point: vector(f"{prefix}_normalized", "DIVIDE", point, size)))
                    .bind(lambda point: vector(f"{prefix}_absolute", "ABSOLUTE", point))
                    .bind(lambda point: node(f"{prefix}_absolute_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}))
                    .bind(lambda axes: math(f"{prefix}_parameter", "MAXIMUM", ended(f"{prefix}_absolute_axes", "X")(axes), End(node=f"{prefix}_absolute_axes", socket="Y")))
                )

    held = next(case for case in shell.cases[path] if isinstance(case, Gradient))
    motion = (*gradient, Gradient.motion.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    clock = Clock(key=f"{key}_clock", speed=shell.socket(*motion, Motion.speed.__name__), hold=shell.socket(*motion, Motion.hold.__name__)).value  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    extension = (*gradient, Gradient.extension.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def extended(coordinate: tuple[Fragment, End]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        repeated = math(f"{key}_repeat", "FRACT", coordinate)
        mirrored = (
            math(f"{key}_period", "FLOORED_MODULO", coordinate, 2.0)
            .bind(lambda period: math(f"{key}_mirror_offset", "SUBTRACT", period, 1.0))
            .bind(lambda offset: math(f"{key}_mirror_absolute", "ABSOLUTE", offset))
            .bind(lambda absolute: math(f"{key}_mirror", "SUBTRACT", 1.0, absolute))
        )
        clamped = node(f"{key}_clamp", bpy.types.ShaderNodeClamp, {"Value": coordinate, "Min": 0.0, "Max": 1.0}, {}).map(ended(f"{key}_clamp", "Result"))
        chosen = accumulated((clamped, repeated, mirrored)).bind(
            lambda values: menu(
                f"{key}_extension",
                "FLOAT",
                shell.socket(*extension),
                shell.items[extension],
                {Extension.CLIP.label: values[0], Extension.EXTEND.label: values[0], Extension.REPEAT.label: values[1], Extension.MIRROR.label: values[2]},
            )
        )
        clip = (
            paired(compared(f"{key}_above_start", "GREATER_EQUAL", coordinate, 0.0), compared(f"{key}_below_end", "LESS_EQUAL", coordinate, 1.0))
            .bind(lambda parts: node(f"{key}_inside", bpy.types.FunctionNodeBooleanMath, {"Boolean": parts[0], "Boolean_001": parts[1]}, {"operation": "AND"}))
            .map(ended(f"{key}_inside", "Boolean"))
            .bind(lambda inside: switched(f"{key}_paint_coverage", "FLOAT", End(node=f"{key}_extension", socket=Extension.CLIP.label), 1.0, inside))
        )
        ramp = item_key(*gradient, Gradient.ramp.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        colors = chosen.bind(lambda chosen: node(ramp, bpy.types.ShaderNodeValToRGB, {"Fac": chosen}, {}, Some(held.ramp)))
        return paired(colors, clip).bind(
            lambda parts: math(f"{key}_ramp_coverage", "MULTIPLY", ended(ramp, "Alpha")(parts[0]), parts[1]).map(lambda alpha: ((parts[0], End(node=ramp, socket="Color")), alpha))
        )

    coordinate = accumulated(parameter(shape) for shape in geometries).bind(
        lambda values: menu(f"{key}_geometry", "FLOAT", shell.socket(*geometry), shell.items[geometry], {type(shape).title: value for shape, value in zip(geometries, values)})
    )
    phase = paired(math(f"{key}_phase", "DIVIDE", shell.socket(*gradient, Gradient.phase.__name__), tau), clock).bind(lambda parts: math(f"{key}_cycle_phase", "SUBTRACT", *parts))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    cycled = (
        paired(coordinate, phase).bind(lambda parts: math(f"{key}_cycles", "ADD", *parts)).bind(lambda cycles: periodic(f"{key}_profile", held.profile.value, cycles))
        if held.profile.enabled
        else coordinate
    )
    colors = paired(paired(corner_color, cell_field.bind(colored_cells)), cycled.bind(extended))
    return colors.bind(
        lambda parts: selected(
            f"{key}_paint",
            shell.socket(*path),
            shell.items[path],
            {
                "color": ("RGBA", {Solid.title: shell.socket(*path, Solid, Solid.color.__name__), Corners.title: parts[0][0], Cells.title: parts[0][1], Gradient.title: parts[1][0]}),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                "coverage": ("FLOAT", {Solid.title: 1.0, Corners.title: 1.0, Cells.title: 1.0, Gradient.title: parts[1][1]}),
            },
        )
    ).map(lambda fields: (fields["color"], fields["coverage"]))


def coverage(key: str, distance: Operand, width: Option[Operand], softness: Operand, pixel: Operand, growth: Operand = 0.0) -> BlenderResult[tuple[Fragment, End]]:
    """Builds grown distance-ramp coverage with an optional bounded-width cap.

    Args:
        key: Prefix of the coverage's native node keys.
        distance: Signed region distance in displayed greater-side frame shares.
        width: Local narrowest width, absent for an unbounded region.
        softness: Authored ramp width in the distance's units.
        pixel: One displayed pixel in the distance's units.
        growth: Outward region growth in the distance's units.

    Returns:
        The approximate covered fraction and its fragment, or native graph faults.
    """
    return (
        paired(math(f"{key}_distance", "SUBTRACT", distance, growth), math(f"{key}_span", "ADD", softness, pixel))
        .map(lambda values: (math(f"{key}_scaled", "DIVIDE", values[0], values[1]).bind(lambda scaled: math(f"{key}_ramp", "SUBTRACT", 0.5, scaled, clamp=True)), values[1]))
        .bind(
            lambda values: width.map(
                lambda value: paired(values[0], math(f"{key}_grown_width", "MULTIPLY_ADD", growth, 2.0, value).bind(lambda grown: math(f"{key}_width_limit", "DIVIDE", grown, values[1]))).bind(
                    lambda limits: math(f"{key}_covered", "MINIMUM", limits[0], limits[1])
                )
            ).default_value(values[0])
        )
    )


def shaped(shell: Shell[Shapes]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns associated fill, outline, and independently emitted shape light."""
    outline = (Shapes.outline.__name__, Gated.value.__name__)  # type: ignore[attr-defined, misc]  # ty: ignore[invalid-attribute-access, unresolved-attribute]
    outer = math("shape_outer_growth", "ADD", shell.socket(*outline, Outline.offset.__name__), shell.socket(*outline, Outline.width.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def light(parts: tuple[tuple[tuple[Fragment, End], ...], tuple[tuple[Fragment, End], tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
        (fill, inner, outer), (color, paint_coverage) = parts
        filled = math("shape_painted_coverage", "MULTIPLY", fill, paint_coverage).bind(lambda coverage: switched("shape_fill_coverage", "FLOAT", shell.socket(Shapes.fill.__name__), 0.0, coverage))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        outlined = math("shape_outline_ring", "SUBTRACT", outer, inner).bind(lambda coverage: switched("shape_outline_coverage", "FLOAT", shell.socket(Shapes.outline.__name__), 0.0, coverage))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        painted_light = filled.bind(lambda coverage: vector("shape_fill_light", "SCALE", color, coverage))
        outlined_light = outlined.bind(lambda coverage: vector("shape_outline_light", "SCALE", shell.socket(*outline, Outline.color.__name__), coverage))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        emission = (
            emitted(shell, (Shapes.emission.__name__, Gated.value.__name__), (fill,))  # type: ignore[attr-defined, misc]  # ty: ignore[invalid-attribute-access, unresolved-attribute]
            .bind(lambda emitted: vector("shape_emitted_light", "SCALE", emitted[0], fill))
            .bind(lambda light: switched("shape_emission", "VECTOR", shell.socket(Shapes.emission.__name__), (0.0, 0.0, 0.0), light))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        )
        alpha = paired(filled, outlined).bind(lambda coverages: math("shape_alpha", "ADD", *coverages))
        paint = paired(painted_light, outlined_light).bind(lambda lights: vector("shape_painted_light", "ADD", *lights))
        radiance = paired(paint, emission).bind(lambda lights: vector("shape_light", "ADD", *lights))
        return (
            paired(radiance, alpha)
            .bind(lambda parts: node("shape_generated", bpy.types.CompositorNodeSetAlpha, {"Image": parts[0], "Alpha": parts[1], "Type": "Replace Alpha"}, {}))
            .map(ended("shape_generated", "Image"))
        )

    return (
        paired(framed("shape_frame", shell.source, shell.aspect, shell.scale), outer)
        .bind(lambda parts: paired(fielded(shell, parts[0], (0.0, shell.socket(*outline, Outline.offset.__name__), parts[1])), painted(shell, (Shapes.fill.__name__, Gated.value.__name__), parts[0])))  # type: ignore[attr-defined, misc]  # ty: ignore[invalid-attribute-access, unresolved-attribute]
        .bind(light)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

shapes: CompositorEffect[Shapes] = CompositorEffect(
    record=Shapes,
    graph=shaped,
    coverage=Coverage.RESULT,
    requested=Some(precise),
    registrables=(inspection(lambda: shapes, "shape_coverage", "View Coverage", "Inspect the shape's inner field coverage", lambda: End(node="shape_field", socket="Output")),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["boxed", "combined", "coverage", "fielded", "figured", "lattice", "lightning", "painted", "polygonal", "primitive", "shaped", "shapes", "stroked", "windowed"]
