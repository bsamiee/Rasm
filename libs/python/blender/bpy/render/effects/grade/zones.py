"""Piecewise tonal-zone grading on OCIO scene-linear stop axis."""

import functools
import itertools

import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, grouped, math, NodeGroup, rgba, switched, vector, where
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, logarithmic
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Shell
from blender.imaging.effects.grade.zones import Zone, Zones
from blender.imaging.faults import accumulated

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PIECES]


@grouped("zones_segment", f".{Zones.title} Segment")
def _segment(value: Expr | float, start: Expr | float, end: Expr | float, level: Expr | float, start_slope: Expr | float, end_slope: Expr | float) -> Expr:
    """Quadratic piece with linear slope between endpoints."""
    run = value - start
    return math("MULTIPLY_ADD", run, math("MULTIPLY_ADD", run / (end - start) * 0.5, end_slope - start_slope, start_slope), level)


@grouped("zones_inverse", f".{Zones.title} Inverse")
def _inverse(value: Expr, start: Expr, end: Expr, level: Expr, start_slope: Expr | float, end_slope: Expr | float) -> Expr:
    """Stable root of quadratic piece."""
    span, constant = end - start, level - value
    linear = start_slope * span
    root = math("SQRT", math("MULTIPLY_ADD", (end_slope - start_slope) * span * constant, -2.0, linear * linear))
    return math("MULTIPLY_ADD", constant * -2.0 / (root + linear), span, start)


@grouped("zones_pivot", f".{Zones.title} Pivot")
def _pivot(value: Expr, low: Expr, high: Expr, low_slope: Expr | float, high_slope: Expr | float, reverse: Expr) -> Expr:
    """Forward and inverse quadratic pairs joined at pivot midpoint."""
    middle, knee = math("MULTIPLY_ADD", high - low, 0.5, low), math("MULTIPLY_ADD", low_slope + high_slope, -0.5, 2.0)
    crest = _segment(middle, low, middle, low, low_slope, knee)
    below, above, under, within = value - low, value - high, value < low, value < high
    rising = where(value < middle, _segment(value, low, middle, low, low_slope, knee), _segment(value, middle, high, crest, knee, high_slope))
    falling = where(value < crest, _inverse(value, low, middle, low, low_slope, knee), _inverse(value, middle, high, crest, knee, high_slope))
    forward = where(within, where(under, math("MULTIPLY_ADD", below, low_slope, low), rising), math("MULTIPLY_ADD", above, high_slope, high))
    return switched(reverse, forward, where(within, where(under, below / low_slope + low, falling), above / high_slope + high))


@grouped("zones_slope", f".{Zones.title} Slope")
def _slope(adjustment: Expr) -> Expr:
    """OCIO end slope symmetric about adjustment one."""
    return math("MAXIMUM", 1.0 - abs(adjustment - 1.0), 0.01)


# --- [ZONES]


@grouped("zones_highlights", f".{Zones.title} Highlights")
def _highlights(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """Highlight pivot pair with minimum gap."""
    pivot = start + width
    return _pivot(value, math("MINIMUM", start, pivot - 0.01), pivot, 1.0, _slope(adjustment), adjustment < 1.0)


@grouped("zones_shadows", f".{Zones.title} Shadows")
def _shadows(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """Shadow pivot pair with minimum gap."""
    pivot = start - width
    return _pivot(value, pivot, math("MAXIMUM", start, pivot + 0.01), _slope(adjustment), 1.0, adjustment > 1.0)


@grouped("zones_midtones", f".{Zones.title} Midtones")
def _midtones(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """Equal-area midtone curve between linear-style endpoints."""
    low, high = -5.5, 7.5
    extent = math("MINIMUM", math("MAXIMUM", width, 0.01), 12.35)
    reach, tenth = extent * 0.51, extent * -0.1
    center = math("MINIMUM", math("MAXIMUM", start, reach + low), high - reach)
    first = math("MULTIPLY_ADD", extent, -0.5, center)
    second, third, fourth = math("MULTIPLY_ADD", extent, 0.25, first), math("MULTIPLY_ADD", extent, 0.75, first), first + extent
    swing = math("MULTIPLY_ADD", math("MINIMUM", math("MAXIMUM", adjustment, 0.01), 1.99), 0.9, -0.9)
    fall = math("MULTIPLY_ADD", swing, math("MULTIPLY_ADD", first - low, -0.4, tenth) / (high - third), 1.0)
    rise = math("MULTIPLY_ADD", swing, math("MULTIPLY_ADD", high - fourth, -0.4, tenth) / (low - second), 1.0)
    upper = center > 1.0
    knots: tuple[Expr | float, ...] = (low, first, second, third, fourth, high)
    slopes: tuple[Expr | float, ...] = (1.0, switched(upper, math("MULTIPLY_ADD", swing, 0.4, 1.0), rise), swing + 1.0, 1.0 - swing, switched(upper, fall, math("MULTIPLY_ADD", swing, -0.4, 1.0)), 1.0)
    spans = tuple(zip(knots, knots[1:], slopes, slopes[1:]))

    def leveled(held: tuple[Expr | float, ...], span: tuple[Expr | float, Expr | float, Expr | float, Expr | float]) -> tuple[Expr | float, ...]:
        left, right, entering, leaving = span
        return (*held, _segment(right, left, right, held[-1], entering, leaving))

    levels = functools.reduce(leveled, spans, knots[:1])
    pieces = tuple(_segment(value, left, right, level, entering, leaving) for (left, right, entering, leaving), level in zip(spans, levels))
    picked = functools.reduce(lambda previous, step: where(value < step[0], previous, step[1]), zip(knots[1:-1], pieces[1:]), pieces[0])
    return where(value < high, where(value < low, value, picked), value - high + levels[-1])


@grouped("zones_whites", f".{Zones.title} Whites")
def _whites(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """White compression and inverse expansion with quadratic HDR extension."""
    end, slope = start + width, _slope(adjustment)
    mean = slope + 1.0
    gain, flat, under, inside = mean * 0.5, 1.0 / slope, value < start, value < end
    falling = where(under, value, where(inside, _segment(value, start, end, start, 1.0, slope), math("MULTIPLY_ADD", value - end, slope, math("MULTIPLY_ADD", mean, width * 0.5, start))))
    square = (flat - 1.0 / math("MULTIPLY_ADD", slope, 0.99, 0.01)) * 50.0 / width
    twice = square * end
    linear = math("MULTIPLY_ADD", twice, -2.0, flat)
    quadratic = math("MULTIPLY_ADD", math("MULTIPLY_ADD", square, value, linear), value, width / gain + start - (twice + linear) * end)
    back = (_inverse(math("MULTIPLY_ADD", value - start, gain, start), start, end, start, 1.0, slope) - start) / gain + start
    return switched(adjustment < 1.0, where(inside, where(under, value, back), quadratic), falling)


@grouped("zones_blacks", f".{Zones.title} Blacks")
def _blacks(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """Black compression and inverse expansion from shadow-derived start."""
    low, slope = start - width, _slope(adjustment)
    mean = slope + 1.0
    floor, gain, inside = start - mean * (width * 0.5), mean * 0.5, value < start
    falling = where(value < low, math("MULTIPLY_ADD", value - low, slope, floor), where(inside, _segment(value, low, start, floor, slope, 1.0), value))
    scaled = math("MULTIPLY_ADD", value - start, gain, start)
    inner = where(scaled < floor, (scaled - floor) / slope + low, _inverse(scaled, low, start, floor, slope, 1.0))
    return switched(adjustment > 1.0, where(inside, (inner - start) / gain + start, value), falling)


# --- [CONTRAST]


@grouped("zones_contrast_end", f".{Zones.title} Contrast End")
def _contrast_end(value: Expr, gain: Expr, limit: Expr | float) -> Expr:
    """Contrast shoulder, reflected for negative endpoint."""
    flat, level = 1.0 / gain, limit * 0.25
    first = flat * level
    least, doubled = (limit - first) * 0.3, limit / (gain + 1.0) * 2.0
    last = doubled - first
    wide, narrow, middle = limit < last, last - first < least, math("MULTIPLY_ADD", least, 0.5, first)
    start, end = switched(wide, first, doubled - limit), switched(wide, switched(narrow, last, first + least), limit)
    slope = switched(wide, switched(narrow, flat, (limit - gain * middle) / (limit - middle)), flat)
    tail = math("MULTIPLY_ADD", value - end, slope, _segment(end, start, end, level, gain, slope))
    return where(value < end, where(value < start, value * gain, _segment(value, start, end, level, gain, slope)), tail)


@grouped("zones_contrast", f".{Zones.title} Contrast")
def _contrast(value: Expr, contrast: Expr) -> Expr:
    """S-contrast with independent positive and negative shoulders."""
    steep = 1.0 / math("MULTIPLY_ADD", math("MINIMUM", contrast, 1.99), -0.8125, 1.8125)
    gain = switched(contrast > 1.0, math("MULTIPLY_ADD", math("MAXIMUM", contrast, 0.01), 0.71875, 0.28125), steep)
    return where(value < 0.0, -_contrast_end(-value, gain, 5.5), _contrast_end(value, gain, 6.5))


# --- [GRAPH]


def zoned(shell: Shell[Zones]) -> BlenderResult[Expr]:
    """Returns per-channel tonal-zone chain with master placement and clamp, zone colors spanning each adjustment range."""
    blacks, shadows, midtones, highlights, whites, s_contrast = Zones.__struct_fields__
    color, master, start, width = Zone.__struct_fields__
    tints = {zone: vector("MULTIPLY_ADD", shell.input(zone, color), (reach,) * 3, (1.0 - reach,) * 3) for zone, reach in Zones.reaches().items()}
    adjusted = {zone: (tint["X"], tint["Y"], tint["Z"]) for zone, tint in tints.items()}
    placement = {zone: (shell.input(zone, master), shell.input(zone, start), shell.input(zone, width)) for zone in tints}
    whites_start, whites_end = (_highlights(edge, *placement[highlights]) for edge in (shell.input(whites, start), shell.input(whites, start) + shell.input(whites, width)))
    blacks_start, blacks_end = (_shadows(edge, *placement[shadows]) for edge in (shell.input(blacks, start), shell.input(blacks, start) - shell.input(blacks, width)))
    placed = (
        (midtones, _midtones, *placement[midtones][1:]),
        (highlights, _highlights, *placement[highlights][1:]),
        (whites, _whites, whites_start, whites_end - whites_start),
        (shadows, _shadows, *placement[shadows][1:]),
        (blacks, _blacks, blacks_start, blacks_start - blacks_end),
    )
    offsets = (*(deviation - 1.0 for zone in tints for deviation in (placement[zone][0], *adjusted[zone])), shell.input(s_contrast) - 1.0)
    total = functools.reduce(lambda held, offset: math("MULTIPLY_ADD", offset, offset, held), offsets[1:], offsets[0] * offsets[0])

    def built(groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse = groups
        entering = forward.placed({COLOR_INPUT.key: shell.color}, COLOR_RESULT.key)

        def graded(index: int, value: Expr) -> Expr:
            steps = ((group, adjustment, low, extent) for zone, group, low, extent in placed for adjustment in (adjusted[zone][index], placement[zone][0]))
            return _contrast(functools.reduce(lambda held, step: step[0](held, *step[1:]), steps, value), shell.input(s_contrast))

        linear = inverse.placed({COLOR_INPUT.key: rgba(*itertools.starmap(graded, enumerate((entering["Red"], entering["Green"], entering["Blue"]))))}, COLOR_RESULT.key)
        return switched(total > 0.0, linear, vector("MINIMUM", linear, (65504.0, 65504.0, 65504.0)))

    return accumulated(logarithmic(direction) for direction in (PyOpenColorIO.TRANSFORM_DIR_FORWARD, PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

zones = CompositorEffect(record=Zones, graph=zoned, alpha=Alpha.STRAIGHT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["zoned", "zones"]
