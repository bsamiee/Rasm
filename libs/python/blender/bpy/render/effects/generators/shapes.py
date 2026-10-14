"""Native geometric fields, painted shapes, and shared distance-ramp coverage."""

from collections.abc import Mapping
from functools import cache, partial, reduce
from math import cos, pi, sin, tau

import bpy
from expression import Nothing, Ok, Option, Some

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace
from blender.bpy.nodes.graphs import Clock, compared, End, ended, Fragment, masked, math, menu, mixed, nested, node, NodeGroup, Operand, selected, summed, switched, vector
from blender.bpy.nodes.interface import Interface, item_key, Socket
from blender.bpy.render.effects.generators.noise import emitted, periodic, sample, sampled
from blender.bpy.render.effects.stack import CompositorEffect, Coverage, KeyPath, precise, Shell
from blender.bpy.render.effects.viewers import inspection
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
from blender.imaging.faults import accumulated, paired
from blender.imaging.geometry.stmaps import Extension
from blender.imaging.parameters import Gated, Motion

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
    width_box, height, roundness_box = Box.__struct_fields__
    match shape:
        case Box():
            return boxed(key, point, inlet(width_box), inlet(height), inlet(roundness_box))
        case Ellipse():
            width_ellipse, height_ellipse, exponent_ellipse = Ellipse.__struct_fields__
            exponent = inlet(exponent_ellipse)
            half = (
                node(f"{key}_size", bpy.types.ShaderNodeCombineXYZ, {"X": inlet(width_ellipse), "Y": inlet(height_ellipse)}, {})
                .map(ended(f"{key}_size", "Vector"))
                .bind(lambda size: vector(f"{key}_half", "SCALE", size, 0.5))
            )
            narrow = math(f"{key}_width", "MINIMUM", inlet(width_ellipse), inlet(height_ellipse))
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
            length_cross, thickness_cross, aspect, roundness_cross = Cross.__struct_fields__
            length = inlet(length_cross)
            roundness = inlet(roundness_cross)
            width = math(f"{key}_width", "MULTIPLY", length, inlet(thickness_cross))
            horizontal = math(f"{key}_horizontal", "MULTIPLY", length, inlet(aspect))
            bars = paired(width, horizontal).bind(lambda parts: paired(boxed(f"{key}_across", point, parts[1], parts[0], roundness), boxed(f"{key}_up", point, parts[0], length, roundness)))
            return bars.bind(lambda parts: paired(math(f"{key}_distance", "MINIMUM", parts[0][0], parts[1][0]), math(f"{key}_narrow", "MAXIMUM", parts[0][1], parts[1][1])))
        case Triangle():
            a, b, c, roundness_triangle = Triangle.__struct_fields__
            vertices = tuple(inlet(member) for member in (a, b, c))
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
                lambda positive: switched(f"{key}_roundness", "FLOAT", positive, 0.0, inlet(roundness_triangle))
            )
            shrunk = paired(center, fraction).bind(lambda parts: accumulated(mixed(f"{key}_vertex_{index}", "VECTOR", parts[1], vertex, parts[0]) for index, vertex in enumerate(vertices)))
            return paired(paired(area, perimeter).bind(lambda parts: math(f"{key}_incircle", "DIVIDE", *parts)), shrunk).bind(
                lambda parts: paired(
                    math(f"{key}_rounding", "MULTIPLY", parts[0], inlet(roundness_triangle)).bind(lambda rounding: polygonal(key, point, parts[1], rounding)),
                    math(f"{key}_width", "MULTIPLY", parts[0], 2.0),
                )
            )
        case Polygon():
            radius_polygon, _, inset, roundness_polygon = Polygon.__struct_fields__
            sides = shape.sides
            radius = inlet(radius_polygon)
            rounding = math(f"{key}_rounding", "MULTIPLY", radius, inlet(roundness_polygon))
            outer = paired(Ok(radius), rounding).bind(lambda parts: math(f"{key}_outer", "SUBTRACT", *parts))
            inner = (
                math(f"{key}_remaining", "SUBTRACT", 1.0, inlet(inset))
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
            (height_heart,) = Heart.__struct_fields__
            lobe_radius = 2.0**0.5 / 4.0
            unit_height = 0.75 + lobe_radius
            scale = math(f"{key}_scale", "DIVIDE", inlet(height_heart), unit_height)
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
            return paired(paired(chosen, scale).bind(lambda parts: math(f"{key}_distance", "MULTIPLY", *parts)), Ok((Fragment(), inlet(height_heart))))
        case Segment():
            length_segment, thickness_segment, start, end = Segment.__struct_fields__
            ends = accumulated(
                math(f"{key}_{edge}", "MINIMUM" if edge == "start" else "MAXIMUM", inlet(start), inlet(end))
                .bind(lambda fraction: math(f"{key}_{edge}_centered", "SUBTRACT", fraction, 0.5))
                .bind(lambda fraction: math(f"{key}_{edge}_offset", "MULTIPLY", fraction, inlet(length_segment)))
                .bind(lambda offset: node(f"{key}_{edge}_point", bpy.types.ShaderNodeCombineXYZ, {"X": offset}, {}))
                .map(ended(f"{key}_{edge}_point", "Vector"))
                for edge in ("start", "end")
            )
            return paired(ends, math(f"{key}_half_thickness", "MULTIPLY", inlet(thickness_segment), 0.5)).bind(lambda parts: stroked(key, point, parts[0][0], parts[0][1], parts[1], parts[1]))
        case Grid():
            width_grid, height_grid, line, offset_grid = Grid.__struct_fields__
            return lattice(key, point, inlet(width_grid), inlet(height_grid), inlet(offset_grid), inlet(line)).map(lambda parts: (parts[0], (Fragment(), inlet(line))))
        case Bar():
            box_bar, completion_bar, growth_bar = Bar.__struct_fields__
            box = (*path, box_bar)
            boundary = boxed(f"{key}_box", point, shell.socket(*box, width_box), shell.socket(*box, height), shell.socket(*box, roundness_box))
            half = math(f"{key}_half", "MULTIPLY", shell.socket(*box, width_box), 0.5)
            x = node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}).map(ended(f"{key}_axes", "X"))
            completion = inlet(completion_bar)
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
            chosen = clips.bind(lambda clips: menu(f"{key}_growth", "FLOAT", inlet(growth_bar), shell.items[*path, growth_bar], dict(zip((growth.label for growth in Growth), clips))))
            filled = (
                math(f"{key}_filled_length", "MULTIPLY", shell.socket(*box, width_box), completion)
                .bind(
                    lambda length: math(f"{key}_half_length", "MULTIPLY", length, 0.5).bind(
                        lambda half: switched(f"{key}_piece_length", "FLOAT", End(node=f"{key}_growth", socket=Growth.EDGES.label), length, half)
                    )
                )
                .bind(lambda length: math(f"{key}_width", "MINIMUM", length, shell.socket(*box, height)))
            )
            return paired(paired(boundary, chosen).bind(lambda parts: math(f"{key}_distance", "MAXIMUM", parts[0][0], parts[1])), filled)
        case Ring():
            ellipse, thickness_ring = Ring.__struct_fields__
            boundary = primitive(shell, (*path, ellipse), shape.ellipse, point)
            half = boundary.bind(lambda boundary: math(f"{key}_thickness", "MULTIPLY", boundary[1], inlet(thickness_ring))).bind(
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
    primitive_figure, position, rotation, short_side = Figure.__struct_fields__
    at = (*path, primitive_figure)
    cases = tuple(shape for shape in shell.cases[at] if isinstance(shape, Box | Ellipse | Triangle | Cross | Heart | Segment | Bar | Grid | Polygon | Ring))
    extent = node(f"{key}_extent", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {}).bind(
        lambda axes: math(f"{key}_short", "MINIMUM", ended(f"{key}_extent", "X")(axes), End(node=f"{key}_extent", socket="Y"))
    )
    scale = extent.bind(lambda short: switched(f"{key}_scale", "FLOAT", shell.socket(*path, short_side), 1.0, short))
    local = (
        vector(f"{key}_anchor", "MULTIPLY", shell.socket(*path, position), frame.extent)
        .bind(lambda anchor: vector(f"{key}_offset", "SUBTRACT", frame.point, anchor))
        .bind(lambda offset: node(f"{key}_rotation", bpy.types.ShaderNodeVectorRotate, {"Vector": offset, "Angle": shell.socket(*path, rotation)}, {"rotation_type": "Z_AXIS", "invert": True}))
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
            start, end, progress = Morph.__struct_fields__
            return (
                paired(figured(shell, (*path, start), frame), figured(shell, (*path, end), frame))
                .bind(
                    lambda fields: accumulated(mixed(f"{key}_{name}", "FLOAT", shell.socket(*path, progress), fields[0][index], fields[1][index]) for index, name in enumerate(("distance", "width")))
                )
                .map(lambda fields: (fields[0], fields[1]))
            )
        case Boolean():
            operation_boolean, base, cutter, smoothness_boolean = Boolean.__struct_fields__
            inputs = paired(figured(shell, (*path, base), frame), figured(shell, (*path, cutter), frame))
            smoothness = shell.socket(*path, smoothness_boolean)

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
                operation = (*path, operation_boolean)
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
    aspect_window, inset_window, roundness = Window.__struct_fields__
    _, value = Gated.__struct_fields__
    axes = node(f"{key}_extent", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {})
    ratio = axes.bind(lambda axes: math(f"{key}_frame_aspect", "DIVIDE", ended(f"{key}_extent", "X")(axes), End(node=f"{key}_extent", socket="Y")))
    aspect = ratio.bind(lambda ratio: switched(f"{key}_aspect", "FLOAT", shell.socket(*path, aspect_window), ratio, shell.socket(*path, aspect_window, value)))
    width = paired(axes, aspect).bind(
        lambda parts: math(f"{key}_fitted_width", "MULTIPLY", ended(f"{key}_extent", "Y")(parts[0]), parts[1]).bind(
            lambda width: math(f"{key}_width", "MINIMUM", width, ended(f"{key}_extent", "X")(parts[0]))
        )
    )
    height = paired(width, aspect).bind(lambda parts: math(f"{key}_height", "DIVIDE", *parts))
    inset = paired(width, height).bind(lambda parts: math(f"{key}_short", "MINIMUM", *parts)).bind(lambda short: math(f"{key}_inset", "MULTIPLY", short, shell.socket(*path, inset_window)))
    point = vector(f"{key}_center", "SCALE", frame.extent, 0.5).bind(lambda center: vector(f"{key}_point", "SUBTRACT", frame.point, center))
    return paired(paired(width, height), paired(inset, point)).bind(
        lambda parts: paired(math(f"{key}_inner_width", "SUBTRACT", parts[0][0], parts[1][0]), math(f"{key}_inner_height", "SUBTRACT", parts[0][1], parts[1][0])).bind(
            lambda size: boxed(f"{key}_box", parts[1][1], *size, shell.socket(*path, roundness))
        )
    )


def lightning[R: Effect](shell: Shell[R], path: KeyPath, frame: FrameSpace, shape: Lightning) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Builds the dyadic channel and its tapered forks, a channel subdivided `k` times one group holding its two halves' groups subdivided `k - 1` times."""
    key = item_key(*path)
    inlet = partial(shell.socket, *path)
    start_lightning, end_lightning, thickness, _, displacement_lightning, _, reach_lightning, spread, taper, motion, seed_lightning = Lightning.__struct_fields__
    speed, hold = Motion.__struct_fields__
    clock = Clock(key=f"{key}_clock", speed=inlet(motion, speed), hold=inlet(motion, hold)).value.bind(lambda clock: math(f"{key}_redraw", "MULTIPLY", clock, 2.0))
    radius = math(f"{key}_radius", "MULTIPLY", inlet(thickness), 0.5)
    start = vector(f"{key}_start", "MULTIPLY", inlet(start_lightning), frame.extent)
    end = vector(f"{key}_end", "MULTIPLY", inlet(end_lightning), frame.extent)
    seed, displacement = inlet(seed_lightning), inlet(displacement_lightning)
    span = 2**shape.depth
    endpoints = (("start", bpy.types.NodeSocketVector), ("end", bpy.types.NodeSocketVector))
    drawn = (("stream", bpy.types.NodeSocketFloat), ("time", bpy.types.NodeSocketFloat), ("seed", bpy.types.NodeSocketFloat), ("displacement", bpy.types.NodeSocketFloat))
    traced = (("point", bpy.types.NodeSocketVector), *endpoints, *((name, bpy.types.NodeSocketFloat) for name in ("index", "span", "first_radius", "last_radius")))
    fields = (("distance", bpy.types.NodeSocketFloat), ("width", bpy.types.NodeSocketFloat))

    def faced(inputs: tuple[tuple[Key, type[bpy.types.NodeSocket]], ...], outputs: tuple[tuple[Key, type[bpy.types.NodeSocket]], ...]) -> Interface:
        def sockets(rows: tuple[tuple[Key, type[bpy.types.NodeSocket]], ...]) -> tuple[Socket, ...]:
            return tuple(Socket(key=name, name=name.replace("_", " ").title(), kind=kind) for name, kind in rows)

        return Interface(outputs=sockets(outputs), inputs=sockets(inputs))

    def draw(prefix: Key, stream: Operand, time: Operand, seed: Operand) -> BlenderResult[tuple[Fragment, End]]:
        return sample(prefix, White(spacing=1.0), (0.0, 0.0, 0.0), time, seed, stream, {})

    def nearer(
        prefix: Key, held: tuple[tuple[Fragment, End], tuple[Fragment, End]], candidate: tuple[tuple[Fragment, End], tuple[Fragment, End]]
    ) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        return compared(f"{prefix}_nearer", "LESS_THAN", candidate[0], held[0]).bind(
            lambda gate: paired(masked(f"{prefix}_distance", "FLOAT", gate, held[0], candidate[0]), masked(f"{prefix}_width", "FLOAT", gate, held[1], candidate[1]))
        )

    bisection = (
        paired(
            vector("chord", "SUBTRACT", Shell.socket("end"), Shell.socket("start"))
            .bind(lambda chord: node("normal", bpy.types.ShaderNodeVectorRotate, {"Vector": chord, "Angle": pi / 2.0}, {"rotation_type": "Z_AXIS"}))
            .map(ended("normal", "Vector")),
            draw("draw", Shell.socket("stream"), Shell.socket("time"), Shell.socket("seed"))
            .bind(lambda value: math("signed", "MULTIPLY_ADD", value, 2.0, -1.0))
            .bind(lambda signed: math("offset_length", "MULTIPLY", signed, Shell.socket("displacement"))),
        )
        .bind(lambda parts: vector("offset", "SCALE", *parts))
        .bind(lambda offset: mixed("middle", "VECTOR", 0.5, Shell.socket("start"), Shell.socket("end")).bind(lambda middle: vector("point", "ADD", middle, offset)))
        .bind(
            lambda point: NodeGroup.built(
                item_key(Shapes, Lightning, "midpoint"), f"{Lightning.title} Midpoint", faced((*endpoints, *drawn), (("point", bpy.types.NodeSocketVector),)), {"point": point}
            )
        )
    )

    @cache
    def channel(level: int) -> BlenderResult[NodeGroup]:
        def grouped(built: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[NodeGroup]:
            face = faced((*traced, *(drawn if level else ())), fields)
            return NodeGroup.built(item_key(Shapes, Lightning, "channel", level), f"{Lightning.title} Channel {level}", face, dict(zip((name for name, _ in fields), built)))

        def halved(groups: tuple[NodeGroup, NodeGroup]) -> BlenderResult[NodeGroup]:
            child, middle = groups
            index = math("middle_index", "ADD", Shell.socket("index"), float(2 ** (level - 1)))
            point = index.bind(lambda at: math("middle_stream", "ADD", Shell.socket("stream"), at)).bind(
                lambda stream: nested("middle", middle, {**{name: Shell.socket(name) for name, _ in (*endpoints, *drawn)}, "stream": stream}, "point")
            )

            def half(side: Key, bounds: Mapping[Key, Operand]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
                return nested(side, child, {socket.key: bounds.get(socket.key, Shell.socket(socket.key)) for socket in child.interface.inputs}, "distance").map(
                    lambda distance: (distance, (Fragment(), End(node=side, socket="width")))
                )

            return (
                paired(index, point)
                .bind(lambda parts: paired(half("left", {"end": parts[1]}), half("right", {"start": parts[1], "index": parts[0]})))
                .bind(lambda sides: nearer("halves", *sides))
                .bind(grouped)
            )

        if level:
            return paired(channel(level - 1), bisection).bind(halved)
        first, last = Shell.socket("first_radius"), Shell.socket("last_radius")
        shares = paired(
            math("first_share", "DIVIDE", Shell.socket("index"), Shell.socket("span")),
            math("next_index", "ADD", Shell.socket("index"), 1.0).bind(lambda following: math("last_share", "DIVIDE", following, Shell.socket("span"))),
        )
        return (
            shares
            .bind(lambda shares: paired(mixed("start_radius", "FLOAT", shares[0], first, last), mixed("end_radius", "FLOAT", shares[1], first, last)))
            .bind(lambda radii: stroked("piece", Shell.socket("point"), Shell.socket("start"), Shell.socket("end"), *radii))
            .bind(grouped)
        )

    def subdivided(at: Key, bounds: tuple[Operand, Operand], radii: tuple[Operand, Operand], stream: int) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        return (
            paired(channel(shape.depth), clock)
            .bind(
                lambda parts: nested(
                    at,
                    parts[0],
                    {
                        "point": frame.point,
                        "start": bounds[0],
                        "end": bounds[1],
                        "index": 0.0,
                        "span": float(span),
                        "first_radius": radii[0],
                        "last_radius": radii[1],
                        "stream": float(stream),
                        "time": parts[1],
                        "seed": seed,
                        "displacement": displacement,
                    },
                    "distance",
                )
            )
            .map(lambda distance: (distance, (Fragment(), End(node=at, socket="width"))))
        )

    @cache
    def vertex(index: int) -> BlenderResult[tuple[Fragment, End]]:
        low = index & -index
        return (
            start
            if index == 0
            else end
            if index == span
            else paired(paired(vertex(index - low), vertex(index + low)), paired(bisection, clock)).bind(
                lambda parts: nested(
                    f"{key}_vertex_{index}", parts[1][0], {"start": parts[0][0], "end": parts[0][1], "stream": float(index), "time": parts[1][1], "seed": seed, "displacement": displacement}, "point"
                )
            )
        )

    def fork(index: int, ends: tuple[tuple[Fragment, End], tuple[Fragment, End]], radius: tuple[Fragment, End]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        prefix = f"{key}_fork_{index}"
        first, last = ends
        level = (index + 1).bit_length()
        numerator = 2 * (index + 1 - 2 ** (level - 1)) + 1
        cell, remainder = divmod(numerator * span, 2**level)
        stream = span + index * (span + 2)
        root = paired(vertex(cell), vertex(cell + 1)).bind(lambda points: mixed(f"{prefix}_root", "VECTOR", remainder / 2**level, *points))
        angle = (
            clock
            .bind(lambda time: draw(f"{prefix}_turn", stream, time, seed))
            .bind(lambda draw: math(f"{prefix}_turn_share", "ADD", draw, 0.5))
            .bind(lambda share: math(f"{prefix}_angle", "MULTIPLY", share, inlet(spread)))
            .bind(lambda angle: math(f"{prefix}_side", "MULTIPLY", angle, -1.0 if index % 2 else 1.0))
        )
        direction = (
            paired(vector(f"{prefix}_chord", "SUBTRACT", last, first), angle)
            .bind(lambda parts: node(f"{prefix}_direction", bpy.types.ShaderNodeVectorRotate, {"Vector": parts[0], "Angle": parts[1]}, {"rotation_type": "Z_AXIS"}))
            .map(ended(f"{prefix}_direction", "Vector"))
        )
        reach = (
            clock
            .bind(lambda time: draw(f"{prefix}_length", stream + 1, time, seed))
            .bind(lambda draw: math(f"{prefix}_length_share", "ADD", draw, 0.5))
            .bind(lambda share: math(f"{prefix}_reach", "MULTIPLY", share, inlet(reach_lightning)))
        )
        tip = paired(root, paired(direction, reach).bind(lambda parts: vector(f"{prefix}_offset", "SCALE", *parts))).bind(lambda parts: vector(f"{prefix}_tip", "ADD", *parts))
        width = math(f"{prefix}_radius", "MULTIPLY", radius, inlet(taper))
        return paired(paired(root, tip), width).bind(lambda parts: subdivided(f"{prefix}_channel", parts[0], (parts[1], 0.0), stream + 2))

    def joined(parts: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], tuple[Fragment, End]]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        bounds, radius = parts
        channels = paired(subdivided(f"{key}_main", bounds, (radius, radius), 0), accumulated(fork(index, bounds, radius) for index in range(shape.branches)))
        return channels.bind(lambda held: reduce(lambda done, step: done.bind(lambda nearest: nearer(f"{key}_{step[0]}", nearest, step[1])), enumerate(held[1], 1), Ok(held[0])))

    return paired(paired(start, end), radius).bind(joined)


def fielded(shell: Shell[Shapes], frame: FrameSpace, growths: tuple[Operand, *tuple[Operand, ...]] = (0.0,)) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
    """Returns grown coverages from one field selector and one noisy boundary."""
    field_shapes, _, _, _, softness_shapes, roughness, edge_noise, seed, invert_shapes = Shapes.__struct_fields__
    path = (field_shapes,)
    cases = tuple(field for field in shell.cases[path] if isinstance(field, Figure | Boolean | Morph | Lightning | Window))
    pixel = math("shape_pixel", "DIVIDE", 1.0, frame.greater)
    displacement = (
        sampled(shell, (edge_noise,), frame.point, 0.0, shell.socket(seed))
        .bind(lambda fields: math("shape_signed_noise", "MULTIPLY_ADD", fields[0][0], 2.0, -1.0))
        .bind(lambda signed: math("shape_roughness", "MULTIPLY", signed, shell.socket(roughness)))
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

    invert = shell.socket(invert_shapes)
    softness = shell.socket(softness_shapes)

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
    top_left, top_right, bottom_right, bottom_left = Corners.__struct_fields__
    grid_cells, _, first, second, mortar_cells, seed = Cells.__struct_fields__
    width, height, line, offset_grid = Grid.__struct_fields__
    geometry_gradient, extension_gradient, ramp_gradient, _, phase_gradient, motion_gradient = Gradient.__struct_fields__
    speed, hold = Motion.__struct_fields__
    (color_solid,) = Solid.__struct_fields__
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
            mixed(f"{key}_bottom", "RGBA", weights[0], shell.socket(*corners, bottom_left), shell.socket(*corners, bottom_right)),
            mixed(f"{key}_top", "RGBA", weights[0], shell.socket(*corners, top_left), shell.socket(*corners, top_right)),
        ).bind(lambda colors: mixed(f"{key}_corners", "RGBA", weights[1], *colors))
    )
    cells = (*path, Cells)
    grid = (*cells, grid_cells)
    cell_field = lattice(f"{key}_cells", frame.point, shell.socket(*grid, width), shell.socket(*grid, height), shell.socket(*grid, offset_grid), shell.socket(*grid, line))

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
                weight = sample(f"{key}_cell_random", White(dimensions=3, spacing=1.0), cell, 0.0, shell.socket(*cells, seed), 0, {})
        mortar = pixel.bind(lambda pixel: coverage(f"{key}_mortar", distance, Some(shell.socket(*grid, line)), 0.0, pixel))
        color = weight.bind(lambda weight: mixed(f"{key}_cell_color", "RGBA", weight, shell.socket(*cells, first), shell.socket(*cells, second)))
        return paired(color, mortar).bind(lambda parts: mixed(f"{key}_cells_color", "RGBA", parts[1], parts[0], shell.socket(*cells, mortar_cells)))

    gradient = (*path, Gradient)
    geometry = (*gradient, geometry_gradient)
    geometries = tuple(shape for shape in shell.cases[geometry] if isinstance(shape, Linear | Radial | Angular | Diamond | Rectangular))

    def parameter(shape: Geometry) -> BlenderResult[tuple[Fragment, End]]:
        kind = type(shape)
        prefix = item_key(*geometry, kind)
        at = (*geometry, kind)

        def located(origin: Key, angle: Key) -> BlenderResult[tuple[Fragment, End]]:
            return (
                vector(f"{prefix}_origin", "MULTIPLY", shell.socket(*at, origin), frame.extent)
                .bind(lambda anchor: vector(f"{prefix}_offset", "SUBTRACT", frame.point, anchor))
                .bind(lambda offset: node(f"{prefix}_rotation", bpy.types.ShaderNodeVectorRotate, {"Vector": offset, "Angle": shell.socket(*at, angle)}, {"rotation_type": "Z_AXIS", "invert": True}))
                .map(ended(f"{prefix}_rotation", "Vector"))
            )

        def split(point: tuple[Fragment, End]) -> BlenderResult[Fragment]:
            return node(f"{prefix}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {})

        match shape:
            case Linear():
                origin, angle, length, sweep_linear = Linear.__struct_fields__
                return (
                    located(origin, angle)
                    .bind(split)
                    .bind(
                        lambda axes: (
                            math(f"{prefix}_absolute_y", "ABSOLUTE", ended(f"{prefix}_axes", "Y")(axes))
                            .bind(lambda y: math(f"{prefix}_sweep", "MULTIPLY", y, shell.socket(*at, sweep_linear)))
                            .bind(lambda sweep: math(f"{prefix}_along", "SUBTRACT", ended(f"{prefix}_axes", "X")(axes), sweep))
                        )
                    )
                    .bind(lambda along: math(f"{prefix}_parameter", "DIVIDE", along, shell.socket(*at, length)))
                )
            case Radial():
                origin, radius_radial, aspect_radial, angle = Radial.__struct_fields__
                return (
                    node(f"{prefix}_aspect", bpy.types.ShaderNodeCombineXYZ, {"X": shell.socket(*at, aspect_radial), "Y": 1.0}, {})
                    .map(ended(f"{prefix}_aspect", "Vector"))
                    .bind(lambda aspect: located(origin, angle).bind(lambda point: vector(f"{prefix}_elliptical", "DIVIDE", point, aspect)))
                    .bind(lambda point: vector(f"{prefix}_radius", "LENGTH", point))
                    .bind(lambda radius: math(f"{prefix}_parameter", "DIVIDE", radius, shell.socket(*at, radius_radial)))
                )
            case Angular():
                origin, angle = Angular.__struct_fields__
                return (
                    located(origin, angle)
                    .bind(split)
                    .bind(lambda axes: math(f"{prefix}_phase", "ARCTAN2", ended(f"{prefix}_axes", "Y")(axes), End(node=f"{prefix}_axes", socket="X")))
                    .bind(lambda phase: math(f"{prefix}_parameter", "DIVIDE", phase, tau))
                )
            case Diamond():
                origin, angle, radius_diamond = Diamond.__struct_fields__
                return (
                    located(origin, angle)
                    .bind(lambda point: vector(f"{prefix}_absolute", "ABSOLUTE", point))
                    .bind(lambda point: node(f"{prefix}_absolute_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}))
                    .bind(lambda axes: math(f"{prefix}_sum", "ADD", ended(f"{prefix}_absolute_axes", "X")(axes), End(node=f"{prefix}_absolute_axes", socket="Y")))
                    .bind(lambda total: math(f"{prefix}_parameter", "DIVIDE", total, shell.socket(*at, radius_diamond)))
                )
            case Rectangular():
                origin, angle, width_rectangular, height_rectangular = Rectangular.__struct_fields__
                return (
                    node(f"{prefix}_size", bpy.types.ShaderNodeCombineXYZ, {"X": shell.socket(*at, width_rectangular), "Y": shell.socket(*at, height_rectangular)}, {})
                    .map(ended(f"{prefix}_size", "Vector"))
                    .bind(lambda size: located(origin, angle).bind(lambda point: vector(f"{prefix}_normalized", "DIVIDE", point, size)))
                    .bind(lambda point: vector(f"{prefix}_absolute", "ABSOLUTE", point))
                    .bind(lambda point: node(f"{prefix}_absolute_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": point}, {}))
                    .bind(lambda axes: math(f"{prefix}_parameter", "MAXIMUM", ended(f"{prefix}_absolute_axes", "X")(axes), End(node=f"{prefix}_absolute_axes", socket="Y")))
                )

    held = next(case for case in shell.cases[path] if isinstance(case, Gradient))
    motion = (*gradient, motion_gradient)
    clock = Clock(key=f"{key}_clock", speed=shell.socket(*motion, speed), hold=shell.socket(*motion, hold)).value
    extension = (*gradient, extension_gradient)

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
        ramp = item_key(*gradient, ramp_gradient)
        colors = chosen.bind(lambda chosen: node(ramp, bpy.types.ShaderNodeValToRGB, {"Fac": chosen}, {}, Some(held.ramp)))
        return paired(colors, clip).bind(
            lambda parts: math(f"{key}_ramp_coverage", "MULTIPLY", ended(ramp, "Alpha")(parts[0]), parts[1]).map(lambda alpha: ((parts[0], End(node=ramp, socket="Color")), alpha))
        )

    coordinate = accumulated(parameter(shape) for shape in geometries).bind(
        lambda values: menu(f"{key}_geometry", "FLOAT", shell.socket(*geometry), shell.items[geometry], {type(shape).title: value for shape, value in zip(geometries, values)})
    )
    phase = paired(math(f"{key}_phase", "DIVIDE", shell.socket(*gradient, phase_gradient), tau), clock).bind(lambda parts: math(f"{key}_cycle_phase", "SUBTRACT", *parts))
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
                "color": ("RGBA", {Solid.title: shell.socket(*path, Solid, color_solid), Corners.title: parts[0][0], Cells.title: parts[0][1], Gradient.title: parts[1][0]}),
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
    _, fill_shapes, outline_shapes, emission_shapes, _, _, _, _, _ = Shapes.__struct_fields__
    width, offset, color_outline = Outline.__struct_fields__
    _, value = Gated.__struct_fields__
    outline = (outline_shapes, value)
    outer = math("shape_outer_growth", "ADD", shell.socket(*outline, offset), shell.socket(*outline, width))

    def light(parts: tuple[tuple[tuple[Fragment, End], ...], tuple[tuple[Fragment, End], tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
        (fill, inner, outer), (color, paint_coverage) = parts
        filled = math("shape_painted_coverage", "MULTIPLY", fill, paint_coverage).bind(lambda coverage: switched("shape_fill_coverage", "FLOAT", shell.socket(fill_shapes), 0.0, coverage))
        outlined = math("shape_outline_ring", "SUBTRACT", outer, inner).bind(lambda coverage: switched("shape_outline_coverage", "FLOAT", shell.socket(outline_shapes), 0.0, coverage))
        painted_light = filled.bind(lambda coverage: vector("shape_fill_light", "SCALE", color, coverage))
        outlined_light = outlined.bind(lambda coverage: vector("shape_outline_light", "SCALE", shell.socket(*outline, color_outline), coverage))
        emission = (
            emitted(shell, (emission_shapes, value), (fill,))
            .bind(lambda emitted: vector("shape_emitted_light", "SCALE", emitted[0], fill))
            .bind(lambda light: switched("shape_emission", "VECTOR", shell.socket(emission_shapes), (0.0, 0.0, 0.0), light))
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
        .bind(lambda parts: paired(fielded(shell, parts[0], (0.0, shell.socket(*outline, offset), parts[1])), painted(shell, (fill_shapes, value), parts[0])))
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
