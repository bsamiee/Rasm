"""Native piecewise tonal-zone grading on OCIO's scene-linear stop axis."""

import functools

import bpy
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, ended, Fragment, INPUTS, masked, math, nested, node, NodeGroup, Operand, switched, vector
from blender.bpy.nodes.interface import Interface, item_key, Socket
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, logarithmic
from blender.bpy.render.effects.stack import CompositorEffect, Shell
from blender.imaging.effects.grade.zones import Zone, Zones
from blender.imaging.faults import accumulated, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

_PIECE = ("input", "start", "end", "level", "start_slope", "end_slope")
_PIVOT = ("input", "low", "high", "low_slope", "high_slope", "reverse")
_, _, _, _, start, width = Zone.__struct_fields__
_ZONE = ("input", "adjustment", start, width)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ASSEMBLY]


def _grouped(key: Key, title: str, inputs: tuple[Key, ...], body: BlenderResult[tuple[Fragment, End]]) -> BlenderResult[NodeGroup]:
    """Builds a hidden scalar group from its interface and complete curve body."""
    return body.bind(
        lambda built: NodeGroup.built(
            f"zones_{key}",
            f".{Zones.title} {title}",
            Interface(
                outputs=(Socket(key="value", name="Value", kind=bpy.types.NodeSocketFloat),),
                inputs=tuple(Socket(key=item, name=item.replace("_", " ").title(), kind=bpy.types.NodeSocketFloat) for item in inputs),
            ),
            {"value": built},
        )
    )


# --- [PIECES]


@functools.cache
def _segment() -> BlenderResult[NodeGroup]:
    """Builds the quadratic piece whose slope varies linearly between endpoints."""
    value, start, end, level, entering, leaving = (End(node=INPUTS, socket=key) for key in _PIECE)
    run, span, share, bend, half, slope, piece = (End(node=key) for key in ("run", "span", "share", "bend", "half", "slope", "piece"))
    return _grouped(
        "segment",
        "Segment",
        _PIECE,
        Fragment.joined(
            math(run.node, "SUBTRACT", value, start),
            math(span.node, "SUBTRACT", end, start),
            math(share.node, "DIVIDE", run, span),
            math(bend.node, "SUBTRACT", leaving, entering),
            math(half.node, "MULTIPLY", share, 0.5),
            math(slope.node, "MULTIPLY_ADD", half, bend, entering),
            math(piece.node, "MULTIPLY_ADD", run, slope, level),
        ).map(ended(piece.node, piece.socket)),
    )


@functools.cache
def _inverse() -> BlenderResult[NodeGroup]:
    """Builds the stable root of a quadratic piece."""
    value, start, end, level, entering, leaving = (End(node=INPUTS, socket=key) for key in _PIECE)
    span, bend, square, linear, constant, squared, product, discriminant, root, denominator, numerator, share, piece = (
        End(node=key) for key in ("span", "bend", "square", "linear", "constant", "squared", "product", "discriminant", "root", "denominator", "numerator", "share", "piece")
    )
    return _grouped(
        "inverse",
        "Inverse",
        _PIECE,
        Fragment.joined(
            math(span.node, "SUBTRACT", end, start),
            math(bend.node, "SUBTRACT", leaving, entering),
            math(square.node, "MULTIPLY", bend, span),
            math(linear.node, "MULTIPLY", entering, span),
            math(constant.node, "SUBTRACT", level, value),
            math(squared.node, "MULTIPLY", linear, linear),
            math(product.node, "MULTIPLY", square, constant),
            math(discriminant.node, "MULTIPLY_ADD", product, -2.0, squared),
            math(root.node, "SQRT", discriminant),
            math(denominator.node, "ADD", root, linear),
            math(numerator.node, "MULTIPLY", constant, -2.0),
            math(share.node, "DIVIDE", numerator, denominator),
            math(piece.node, "MULTIPLY_ADD", share, span, start),
        ).map(ended(piece.node, piece.socket)),
    )


@functools.cache
def _pivot() -> BlenderResult[NodeGroup]:
    """Builds forward and inverse quadratic pairs joined at the pivot midpoint."""
    value, low, high, low_slope, high_slope, reverse = (End(node=INPUTS, socket=key) for key in _PIVOT)
    span, middle, slopes, knee, below, above, entry, leave, inward, entry_back, outward, leave_back, under, inside, beneath, within = (
        End(node=key) for key in ("span", "middle", "slopes", "knee", "below", "above", "entry", "leave", "inward", "entry_back", "outward", "leave_back", "under", "inside", "beneath", "within")
    )
    crest, rise_low, rise_high, fall_low, fall_high = (End(node=key, socket="value") for key in ("crest", "rise_low", "rise_high", "fall_low", "fall_high"))
    forward_knee, forward_low, forward, backward_knee, backward_low, backward = (End(node=key) for key in ("forward_knee", "forward_low", "forward", "backward_knee", "backward_low", "backward"))

    def built(segment: NodeGroup, inverse: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "pivot",
            "Pivot",
            _PIVOT,
            Fragment.joined(
                math(span.node, "SUBTRACT", high, low),
                math(middle.node, "MULTIPLY_ADD", span, 0.5, low),
                math(slopes.node, "ADD", low_slope, high_slope),
                math(knee.node, "MULTIPLY_ADD", slopes, -0.5, 2.0),
                nested(crest.node, segment, dict(zip(_PIECE, (middle, low, middle, low, low_slope, knee))), "value"),
                math(below.node, "SUBTRACT", value, low),
                math(above.node, "SUBTRACT", value, high),
                math(entry.node, "MULTIPLY_ADD", below, low_slope, low),
                math(leave.node, "MULTIPLY_ADD", above, high_slope, high),
                math(inward.node, "DIVIDE", below, low_slope),
                math(entry_back.node, "ADD", inward, low),
                math(outward.node, "DIVIDE", above, high_slope),
                math(leave_back.node, "ADD", outward, high),
                math(under.node, "LESS_THAN", value, low),
                math(inside.node, "LESS_THAN", value, middle),
                math(beneath.node, "LESS_THAN", value, crest),
                math(within.node, "LESS_THAN", value, high),
                nested(rise_low.node, segment, dict(zip(_PIECE, (value, low, middle, low, low_slope, knee))), "value"),
                nested(rise_high.node, segment, dict(zip(_PIECE, (value, middle, high, crest, knee, high_slope))), "value"),
                nested(fall_low.node, inverse, dict(zip(_PIECE, (value, low, middle, low, low_slope, knee))), "value"),
                nested(fall_high.node, inverse, dict(zip(_PIECE, (value, middle, high, crest, knee, high_slope))), "value"),
                masked(forward_knee.node, "FLOAT", inside, rise_high, rise_low),
                masked(forward_low.node, "FLOAT", under, forward_knee, entry),
                masked(forward.node, "FLOAT", within, leave, forward_low),
                masked(backward_knee.node, "FLOAT", beneath, fall_high, fall_low),
                masked(backward_low.node, "FLOAT", under, backward_knee, entry_back),
                masked(backward.node, "FLOAT", within, leave_back, backward_low),
                switched("curve", "FLOAT", reverse, forward, backward),
            ).map(ended("curve", "Output")),
        )

    return paired(_segment(), _inverse()).bind(lambda groups: built(*groups))


@functools.cache
def _slope() -> BlenderResult[NodeGroup]:
    """Builds OCIO's end slope symmetric about adjustment one."""
    adjustment = End(node=INPUTS, socket="adjustment")
    offset, deviation, lean, slope = (End(node=key) for key in ("offset", "deviation", "lean", "slope"))
    return _grouped(
        "slope",
        "Slope",
        ("adjustment",),
        Fragment.joined(
            math(offset.node, "SUBTRACT", adjustment, 1.0), math(deviation.node, "ABSOLUTE", offset), math(lean.node, "SUBTRACT", 1.0, deviation), math(slope.node, "MAXIMUM", lean, 0.01)
        ).map(ended(slope.node, slope.socket)),
    )


# --- [ZONES]


@functools.cache
def _highlights() -> BlenderResult[NodeGroup]:
    """Builds the highlight pivot pair with its native minimum gap."""
    value, adjustment, start, width = (End(node=INPUTS, socket=key) for key in _ZONE)
    pivot, limit, low, reverse = (End(node=key) for key in ("pivot", "limit", "low", "reverse"))
    slope = End(node="slope", socket="value")

    def built(sloped: NodeGroup, pivoted: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "highlights",
            "Highlights",
            _ZONE,
            Fragment.joined(
                nested(slope.node, sloped, {"adjustment": adjustment}, "value"),
                math(pivot.node, "ADD", start, width),
                math(limit.node, "SUBTRACT", pivot, 0.01),
                math(low.node, "MINIMUM", start, limit),
                math(reverse.node, "LESS_THAN", adjustment, 1.0),
                nested("curve", pivoted, dict(zip(_PIVOT, (value, low, pivot, 1.0, slope, reverse))), "value"),
            ).map(ended("curve", "value")),
        )

    return paired(_slope(), _pivot()).bind(lambda groups: built(*groups))


@functools.cache
def _shadows() -> BlenderResult[NodeGroup]:
    """Builds the shadow pivot pair with its native minimum gap."""
    value, adjustment, start, width = (End(node=INPUTS, socket=key) for key in _ZONE)
    pivot, limit, high, reverse = (End(node=key) for key in ("pivot", "limit", "high", "reverse"))
    slope = End(node="slope", socket="value")

    def built(sloped: NodeGroup, pivoted: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "shadows",
            "Shadows",
            _ZONE,
            Fragment.joined(
                nested(slope.node, sloped, {"adjustment": adjustment}, "value"),
                math(pivot.node, "SUBTRACT", start, width),
                math(limit.node, "ADD", pivot, 0.01),
                math(high.node, "MAXIMUM", start, limit),
                math(reverse.node, "LESS_THAN", 1.0, adjustment),
                nested("curve", pivoted, dict(zip(_PIVOT, (value, pivot, high, slope, 1.0, reverse))), "value"),
            ).map(ended("curve", "value")),
        )

    return paired(_slope(), _pivot()).bind(lambda groups: built(*groups))


@functools.cache
def _midtones() -> BlenderResult[NodeGroup]:
    """Builds the equal-area midtone curve between native linear-style endpoints."""
    value, adjustment, start, width = (End(node=INPUTS, socket=key) for key in _ZONE)
    (
        least,
        extent,
        reach,
        lowest,
        highest,
        raised,
        center,
        first,
        second,
        third,
        fourth,
        held,
        bounded,
        swing,
        steep,
        shallow,
        rise,
        fall,
        toe,
        head,
        tenth,
        toe_spread,
        toe_room,
        toe_share,
        solved_fall,
        head_spread,
        head_room,
        head_share,
        solved_rise,
        upper,
        past,
        tail,
    ) = (
        End(node=key)
        for key in (
            "least",
            "extent",
            "reach",
            "lowest",
            "highest",
            "raised",
            "center",
            "first",
            "second",
            "third",
            "fourth",
            "held",
            "bounded",
            "swing",
            "steep",
            "shallow",
            "rise",
            "fall",
            "toe",
            "head",
            "tenth",
            "toe_spread",
            "toe_room",
            "toe_share",
            "solved_fall",
            "head_spread",
            "head_room",
            "head_share",
            "solved_rise",
            "upper",
            "past",
            "tail",
        )
    )
    slope_1, slope_4 = (End(node=key, socket="Output") for key in ("slope_1", "slope_4"))
    low, high = -5.5, 7.5
    x: tuple[Operand, ...] = (low, first, second, third, fourth, high)
    slopes = (1.0, slope_1, steep, shallow, slope_4, 1.0)
    levels = tuple(End(node=f"level_{index}", socket="value") for index in range(1, len(x)))
    ordinates: tuple[Operand, ...] = (low, *levels)
    pieces = tuple(End(node=f"piece_{index}", socket="value") for index in range(len(x) - 1))
    before = tuple(End(node=f"before_{index}") for index in range(len(x)))
    picks = tuple(End(node=f"pick_{index}") for index in range(1, len(pieces)))
    *_, last_level = levels
    first_piece, *later_pieces = pieces
    first_before, *_, last_before = before
    *_, last_pick = picks

    def built(segment: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "midtones",
            "Midtones",
            _ZONE,
            Fragment.joined(
                math(least.node, "MAXIMUM", width, 0.01),
                math(extent.node, "MINIMUM", least, 12.35),
                math(reach.node, "MULTIPLY", extent, 0.51),
                math(lowest.node, "ADD", reach, low),
                math(highest.node, "SUBTRACT", high, reach),
                math(raised.node, "MAXIMUM", start, lowest),
                math(center.node, "MINIMUM", raised, highest),
                math(first.node, "MULTIPLY_ADD", extent, -0.5, center),
                math(second.node, "MULTIPLY_ADD", extent, 0.25, first),
                math(third.node, "MULTIPLY_ADD", extent, 0.75, first),
                math(fourth.node, "ADD", first, extent),
                math(held.node, "MAXIMUM", adjustment, 0.01),
                math(bounded.node, "MINIMUM", held, 1.99),
                math(swing.node, "MULTIPLY_ADD", bounded, 0.9, -0.9),
                math(steep.node, "ADD", swing, 1.0),
                math(shallow.node, "SUBTRACT", 1.0, swing),
                math(rise.node, "MULTIPLY_ADD", swing, 0.4, 1.0),
                math(fall.node, "MULTIPLY_ADD", swing, -0.4, 1.0),
                math(toe.node, "SUBTRACT", first, low),
                math(head.node, "SUBTRACT", high, fourth),
                math(tenth.node, "MULTIPLY", extent, -0.1),
                math(toe_spread.node, "MULTIPLY_ADD", toe, -0.4, tenth),
                math(toe_room.node, "SUBTRACT", high, third),
                math(toe_share.node, "DIVIDE", toe_spread, toe_room),
                math(solved_fall.node, "MULTIPLY_ADD", swing, toe_share, 1.0),
                math(head_spread.node, "MULTIPLY_ADD", head, -0.4, tenth),
                math(head_room.node, "SUBTRACT", low, second),
                math(head_share.node, "DIVIDE", head_spread, head_room),
                math(solved_rise.node, "MULTIPLY_ADD", swing, head_share, 1.0),
                math(upper.node, "LESS_THAN", 1.0, center),
                switched(slope_1.node, "FLOAT", upper, rise, solved_rise),
                switched(slope_4.node, "FLOAT", upper, solved_fall, fall),
                *(
                    nested(level.node, segment, dict(zip(_PIECE, (right, left, right, previous, entering, leaving))), "value")
                    for level, left, right, previous, entering, leaving in zip(levels, x, x[1:], ordinates, slopes, slopes[1:])
                ),
                *(
                    nested(piece.node, segment, dict(zip(_PIECE, (value, left, right, level, entering, leaving))), "value")
                    for piece, left, right, level, entering, leaving in zip(pieces, x, x[1:], ordinates, slopes, slopes[1:])
                ),
                *(math(end.node, "LESS_THAN", value, knot) for end, knot in zip(before, x)),
                *(masked(pick.node, "FLOAT", gate, piece, previous) for pick, gate, piece, previous in zip(picks, before[1:], later_pieces, (first_piece, *picks[:-1]))),
                math(past.node, "SUBTRACT", value, high),
                math(tail.node, "ADD", past, last_level),
                masked("lower", "FLOAT", first_before, last_pick, value),
                masked("curve", "FLOAT", last_before, tail, End(node="lower")),
            ).map(ended("curve", "Value")),
        )

    return _segment().bind(built)


@functools.cache
def _whites() -> BlenderResult[NodeGroup]:
    """Builds white compression and inverse expansion with quadratic HDR extension."""
    value, adjustment, start, width = (End(node=INPUTS, socket=key) for key in _ZONE)
    (
        end,
        mean,
        reach,
        top,
        under,
        inside,
        past,
        tail,
        gain,
        run,
        scaled,
        lifted,
        unscaled,
        back,
        near,
        ease,
        flat,
        gap,
        bow,
        square,
        twice,
        linear,
        inner,
        ratio,
        crest,
        lead,
        constant,
        sloped,
        quadratic,
        decreasing,
    ) = (
        End(node=key)
        for key in (
            "end",
            "mean",
            "reach",
            "top",
            "under",
            "inside",
            "past",
            "tail",
            "gain",
            "run",
            "scaled",
            "lifted",
            "unscaled",
            "back",
            "near",
            "ease",
            "flat",
            "gap",
            "bow",
            "square",
            "twice",
            "linear",
            "inner",
            "ratio",
            "crest",
            "lead",
            "constant",
            "sloped",
            "quadratic",
            "decreasing",
        )
    )
    slope, bend, rooted = (End(node=key, socket="value") for key in ("slope", "bend", "rooted"))
    falling_knee, falling, rising_low, rising = (End(node=key) for key in ("falling_knee", "falling", "rising_low", "rising"))

    def built(sloped_group: NodeGroup, segment: NodeGroup, inverse: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "whites",
            "Whites",
            _ZONE,
            Fragment.joined(
                math(end.node, "ADD", start, width),
                nested(slope.node, sloped_group, {"adjustment": adjustment}, "value"),
                math(mean.node, "ADD", slope, 1.0),
                math(reach.node, "MULTIPLY", width, 0.5),
                math(top.node, "MULTIPLY_ADD", mean, reach, start),
                math(under.node, "LESS_THAN", value, start),
                math(inside.node, "LESS_THAN", value, end),
                nested(bend.node, segment, dict(zip(_PIECE, (value, start, end, start, 1.0, slope))), "value"),
                math(past.node, "SUBTRACT", value, end),
                math(tail.node, "MULTIPLY_ADD", past, slope, top),
                masked(falling_knee.node, "FLOAT", inside, tail, bend),
                masked(falling.node, "FLOAT", under, falling_knee, value),
                math(gain.node, "MULTIPLY", mean, 0.5),
                math(run.node, "SUBTRACT", value, start),
                math(scaled.node, "MULTIPLY_ADD", run, gain, start),
                nested(rooted.node, inverse, dict(zip(_PIECE, (scaled, start, end, start, 1.0, slope))), "value"),
                math(lifted.node, "SUBTRACT", rooted, start),
                math(unscaled.node, "DIVIDE", lifted, gain),
                math(back.node, "ADD", unscaled, start),
                math(near.node, "MULTIPLY_ADD", slope, 0.99, 0.01),
                math(ease.node, "DIVIDE", 1.0, near),
                math(flat.node, "DIVIDE", 1.0, slope),
                math(gap.node, "SUBTRACT", flat, ease),
                math(bow.node, "MULTIPLY", gap, 50.0),
                math(square.node, "DIVIDE", bow, width),
                math(twice.node, "MULTIPLY", square, end),
                math(linear.node, "MULTIPLY_ADD", twice, -2.0, flat),
                math(inner.node, "ADD", twice, linear),
                math(ratio.node, "DIVIDE", width, gain),
                math(crest.node, "ADD", ratio, start),
                math(lead.node, "MULTIPLY", inner, end),
                math(constant.node, "SUBTRACT", crest, lead),
                math(sloped.node, "MULTIPLY_ADD", square, value, linear),
                math(quadratic.node, "MULTIPLY_ADD", sloped, value, constant),
                masked(rising_low.node, "FLOAT", under, back, value),
                masked(rising.node, "FLOAT", inside, quadratic, rising_low),
                math(decreasing.node, "LESS_THAN", adjustment, 1.0),
                switched("curve", "FLOAT", decreasing, rising, falling),
            ).map(ended("curve", "Output")),
        )

    return accumulated((_slope(), _segment(), _inverse())).bind(lambda groups: built(*groups))


@functools.cache
def _blacks() -> BlenderResult[NodeGroup]:
    """Builds black compression and inverse expansion around the shadow-derived start."""
    value, adjustment, start, width = (End(node=INPUTS, socket=key) for key in _ZONE)
    low, mean, reach, drop, floor, under, inside, below, toe, gain, run, scaled, sunk, shifted, inward, line, lifted, unscaled, back, increasing = (
        End(node=key)
        for key in (
            "low",
            "mean",
            "reach",
            "drop",
            "floor",
            "under",
            "inside",
            "below",
            "toe",
            "gain",
            "run",
            "scaled",
            "sunk",
            "shifted",
            "inward",
            "line",
            "lifted",
            "unscaled",
            "back",
            "increasing",
        )
    )
    slope, bend, rooted = (End(node=key, socket="value") for key in ("slope", "bend", "rooted"))
    falling_knee, falling, inner, rising = (End(node=key) for key in ("falling_knee", "falling", "inner", "rising"))

    def built(sloped_group: NodeGroup, segment: NodeGroup, inverse: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "blacks",
            "Blacks",
            _ZONE,
            Fragment.joined(
                math(low.node, "SUBTRACT", start, width),
                nested(slope.node, sloped_group, {"adjustment": adjustment}, "value"),
                math(mean.node, "ADD", slope, 1.0),
                math(reach.node, "MULTIPLY", width, 0.5),
                math(drop.node, "MULTIPLY", mean, reach),
                math(floor.node, "SUBTRACT", start, drop),
                math(under.node, "LESS_THAN", value, low),
                math(inside.node, "LESS_THAN", value, start),
                nested(bend.node, segment, dict(zip(_PIECE, (value, low, start, floor, slope, 1.0))), "value"),
                math(below.node, "SUBTRACT", value, low),
                math(toe.node, "MULTIPLY_ADD", below, slope, floor),
                masked(falling_knee.node, "FLOAT", inside, value, bend),
                masked(falling.node, "FLOAT", under, falling_knee, toe),
                math(gain.node, "MULTIPLY", mean, 0.5),
                math(run.node, "SUBTRACT", value, start),
                math(scaled.node, "MULTIPLY_ADD", run, gain, start),
                nested(rooted.node, inverse, dict(zip(_PIECE, (scaled, low, start, floor, slope, 1.0))), "value"),
                math(sunk.node, "LESS_THAN", scaled, floor),
                math(shifted.node, "SUBTRACT", scaled, floor),
                math(inward.node, "DIVIDE", shifted, slope),
                math(line.node, "ADD", inward, low),
                masked(inner.node, "FLOAT", sunk, rooted, line),
                math(lifted.node, "SUBTRACT", inner, start),
                math(unscaled.node, "DIVIDE", lifted, gain),
                math(back.node, "ADD", unscaled, start),
                masked(rising.node, "FLOAT", inside, value, back),
                math(increasing.node, "LESS_THAN", 1.0, adjustment),
                switched("curve", "FLOAT", increasing, rising, falling),
            ).map(ended("curve", "Output")),
        )

    return accumulated((_slope(), _segment(), _inverse())).bind(lambda groups: built(*groups))


# --- [CONTRAST]


@functools.cache
def _contrast_end() -> BlenderResult[NodeGroup]:
    """Builds one contrast shoulder, reflected for the negative endpoint."""
    value, gain, limit = (End(node=INPUTS, socket=key) for key in ("input", "gain", "limit"))
    flat, plus, level, first, center, room, least, doubled, last, extent, wide, narrow, shifted, stretched, middle, lift, rest, gap, tilt, past, tail, linear, before, inside = (
        End(node=key)
        for key in (
            "flat",
            "plus",
            "level",
            "first",
            "center",
            "room",
            "least",
            "doubled",
            "last",
            "extent",
            "wide",
            "narrow",
            "shifted",
            "stretched",
            "middle",
            "lift",
            "rest",
            "gap",
            "tilt",
            "past",
            "tail",
            "linear",
            "before",
            "inside",
        )
    )
    start, narrow_end, end, narrow_slope, slope = (End(node=key, socket="Output") for key in ("start", "narrow_end", "end", "narrow_slope", "slope"))
    lower = End(node="lower")
    end_level, piece = (End(node=key, socket="value") for key in ("end_level", "piece"))

    def built(segment: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "contrast_end",
            "Contrast End",
            ("input", "gain", "limit"),
            Fragment.joined(
                math(flat.node, "DIVIDE", 1.0, gain),
                math(plus.node, "ADD", gain, 1.0),
                math(level.node, "MULTIPLY", limit, 0.25),
                math(first.node, "MULTIPLY", flat, level),
                math(center.node, "DIVIDE", limit, plus),
                math(room.node, "SUBTRACT", limit, first),
                math(least.node, "MULTIPLY", room, 0.3),
                math(doubled.node, "MULTIPLY", center, 2.0),
                math(last.node, "SUBTRACT", doubled, first),
                math(extent.node, "SUBTRACT", last, first),
                math(wide.node, "LESS_THAN", limit, last),
                math(narrow.node, "LESS_THAN", extent, least),
                math(shifted.node, "SUBTRACT", doubled, limit),
                math(stretched.node, "ADD", first, least),
                math(middle.node, "MULTIPLY_ADD", least, 0.5, first),
                math(lift.node, "MULTIPLY", gain, middle),
                math(rest.node, "SUBTRACT", limit, lift),
                math(gap.node, "SUBTRACT", limit, middle),
                math(tilt.node, "DIVIDE", rest, gap),
                switched(start.node, "FLOAT", wide, first, shifted),
                switched(narrow_end.node, "FLOAT", narrow, last, stretched),
                switched(end.node, "FLOAT", wide, narrow_end, limit),
                switched(narrow_slope.node, "FLOAT", narrow, flat, tilt),
                switched(slope.node, "FLOAT", wide, narrow_slope, flat),
                nested(end_level.node, segment, dict(zip(_PIECE, (end, start, end, level, gain, slope))), "value"),
                nested(piece.node, segment, dict(zip(_PIECE, (value, start, end, level, gain, slope))), "value"),
                math(past.node, "SUBTRACT", value, end),
                math(tail.node, "MULTIPLY_ADD", past, slope, end_level),
                math(linear.node, "MULTIPLY", value, gain),
                math(before.node, "LESS_THAN", value, start),
                math(inside.node, "LESS_THAN", value, end),
                masked(lower.node, "FLOAT", before, piece, linear),
                masked("curve", "FLOAT", inside, tail, lower),
            ).map(ended("curve", "Value")),
        )

    return _segment().bind(built)


@functools.cache
def _contrast() -> BlenderResult[NodeGroup]:
    """Builds native S-contrast with independent positive and negative shoulders."""
    value, contrast = (End(node=INPUTS, socket=key) for key in ("input", "contrast"))
    stretch, capped, squeeze, steep, floored, gentle, reflected, negative, lower = (
        End(node=key) for key in ("stretch", "capped", "squeeze", "steep", "floored", "gentle", "reflected", "negative", "lower")
    )
    gain = End(node="gain", socket="Output")
    top, bottom = (End(node=key, socket="value") for key in ("top", "bottom"))

    def built(shoulder: NodeGroup) -> BlenderResult[NodeGroup]:
        return _grouped(
            "contrast",
            "Contrast",
            ("input", "contrast"),
            Fragment.joined(
                math(stretch.node, "LESS_THAN", 1.0, contrast),
                math(capped.node, "MINIMUM", contrast, 1.99),
                math(squeeze.node, "MULTIPLY_ADD", capped, -0.8125, 1.8125),
                math(steep.node, "DIVIDE", 1.0, squeeze),
                math(floored.node, "MAXIMUM", contrast, 0.01),
                math(gentle.node, "MULTIPLY_ADD", floored, 0.71875, 0.28125),
                switched(gain.node, "FLOAT", stretch, gentle, steep),
                math(reflected.node, "MULTIPLY", value, -1.0),
                nested(top.node, shoulder, {"input": value, "gain": gain, "limit": 6.5}, "value"),
                nested(bottom.node, shoulder, {"input": reflected, "gain": gain, "limit": 5.5}, "value"),
                math(negative.node, "MULTIPLY", bottom, -1.0),
                math(lower.node, "LESS_THAN", value, 0.0),
                masked("curve", "FLOAT", lower, top, negative),
            ).map(ended("curve", "Value")),
        )

    return _contrast_end().bind(built)


# --- [GRAPH]


def zoned(shell: Shell[Zones]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns the full per-channel tonal-zone chain with master placement and native clamp."""
    blacks, shadows, midtones, highlights, whites, s_contrast = Zones.__struct_fields__
    master, red, green, blue, start, width = Zone.__struct_fields__
    channels = ((red, "Red"), (green, "Green"), (blue, "Blue"))
    paths = (*((zone, field) for zone in (blacks, shadows, midtones, highlights, whites) for field in (master, red, green, blue)), (s_contrast,))
    whites_reach, whites_width, blacks_reach, blacks_width, graded = (End(node=key) for key in ("whites_reach", "whites_width", "blacks_reach", "blacks_width", "graded"))
    whites_start, whites_end, blacks_start, blacks_end = (End(node=key, socket="value") for key in ("whites_start", "whites_end", "blacks_start", "blacks_end"))
    linear = End(node="linear", socket=COLOR_RESULT.key)

    def built(groups: tuple[NodeGroup, ...]) -> BlenderResult[tuple[Fragment, End]]:
        middle, high, white, shade, black, contrast, forward, inverse = groups
        placed = (
            (midtones, middle, End(node=INPUTS, socket=item_key(midtones, start)), End(node=INPUTS, socket=item_key(midtones, width))),
            (highlights, high, End(node=INPUTS, socket=item_key(highlights, start)), End(node=INPUTS, socket=item_key(highlights, width))),
            (whites, white, whites_start, whites_width),
            (shadows, shade, End(node=INPUTS, socket=item_key(shadows, start)), End(node=INPUTS, socket=item_key(shadows, width))),
            (blacks, black, blacks_start, blacks_width),
        )
        knots = (
            (whites_start, high, End(node=INPUTS, socket=item_key(whites, start)), highlights),
            (whites_end, high, whites_reach, highlights),
            (blacks_start, shade, End(node=INPUTS, socket=item_key(blacks, start)), shadows),
            (blacks_end, shade, blacks_reach, shadows),
        )
        chains = {
            channel: tuple(
                (item_key(channel, zone, adjustment), group, End(node=INPUTS, socket=item_key(zone, adjustment)), low, extent)
                for zone, group, low, extent in placed
                for adjustment in (channel, master)
            )
            for channel, _ in channels
        }
        offsets = tuple(End(node=item_key("offset", *path)) for path in paths)
        sums = tuple(End(node=item_key("deviation", *path)) for path in paths)
        previous: tuple[Operand, ...] = (0.0, *sums[:-1])
        return Fragment.joined(
            nested("stops", forward, {COLOR_INPUT.key: shell.source}, COLOR_RESULT.key),
            node("split", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="stops", socket=COLOR_RESULT.key)}, {"mode": "RGB"}),
            *(nested(end.node, group, dict(zip(_ZONE, (value, *(End(node=INPUTS, socket=item_key(zone, field)) for field in (master, start, width))))), "value") for end, group, value, zone in knots),
            *(
                nested(key, group, dict(zip(_ZONE, (value, adjustment, low, extent))), "value")
                for channel, socket in channels
                for (key, group, adjustment, low, extent), value in zip(chains[channel], (End(node="split", socket=socket), *(End(node=held, socket="value") for held, *_ in chains[channel][:-1])))
            ),
            *(
                nested(
                    item_key(channel, s_contrast), contrast, {"input": End(node=item_key(channel, blacks, master), socket="value"), "contrast": End(node=INPUTS, socket=item_key(s_contrast))}, "value"
                )
                for channel, _ in channels
            ),
            node("combined", bpy.types.CompositorNodeCombineColor, {socket: End(node=item_key(channel, s_contrast), socket="value") for channel, socket in channels}, {"mode": "RGB"}),
            nested(linear.node, inverse, {COLOR_INPUT.key: End(node="combined", socket="Image")}, COLOR_RESULT.key),
            math(whites_reach.node, "ADD", End(node=INPUTS, socket=item_key(whites, start)), End(node=INPUTS, socket=item_key(whites, width))),
            math(whites_width.node, "SUBTRACT", whites_end, whites_start),
            math(blacks_reach.node, "SUBTRACT", End(node=INPUTS, socket=item_key(blacks, start)), End(node=INPUTS, socket=item_key(blacks, width))),
            math(blacks_width.node, "SUBTRACT", blacks_start, blacks_end),
            *(math(offset.node, "SUBTRACT", End(node=INPUTS, socket=item_key(*path)), 1.0) for offset, path in zip(offsets, paths)),
            *(math(total.node, "MULTIPLY_ADD", offset, offset, prior) for total, offset, prior in zip(sums, offsets, previous)),
            math(graded.node, "LESS_THAN", 0.0, sums[-1]),
            vector("clamped", "MINIMUM", linear, (65504.0, 65504.0, 65504.0)),
            switched("formed", "VECTOR", graded, linear, End(node="clamped", socket="Vector")),
        ).map(ended("formed", "Output"))

    return accumulated((
        _midtones(),
        _highlights(),
        _whites(),
        _shadows(),
        _blacks(),
        _contrast(),
        logarithmic(PyOpenColorIO.TRANSFORM_DIR_FORWARD),
        logarithmic(PyOpenColorIO.TRANSFORM_DIR_INVERSE),
    )).bind(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

zones = CompositorEffect(record=Zones, graph=zoned, straight=True)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["zoned", "zones"]
