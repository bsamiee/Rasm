"""OCIO operations as compositor node forms over live entry inputs, and the matrix form of transforms whose optimized processor is one matrix."""

from functools import reduce
import inspect
import itertools
from typing import Final, Self

import bpy
from colour.models.rgb.transfer_functions.st_2084 import CONSTANTS_ST2084, eotf_inverse_ST2084
from expression import Error, Nothing, Ok, Option
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, grouped, math, NodeGroup, rgba, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interface import Socket
from blender.bpy.render.compositor.colors import AXES, COLOR_INPUT, COLOR_RESULT, logarithmic
from blender.imaging.color.processors import GREY, processor
from blender.records.faults import accumulated, Fault, Outcome, RecordReason
from blender.records.fields import Finite

# --- [CONSTANTS] ------------------------------------------------------------------------

MATRIX: Final = tuple(
    Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketVector, description=description, members={"hide_in_modifier": True})
    for key, description in (("red", "First matrix row"), ("green", "Second matrix row"), ("blue", "Third matrix row"), ("offset", "Matrix offset"))
)

# --- [MODELS] ---------------------------------------------------------------------------


class Zone(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One `GradingRGBMSW` zone of `GradingToneTransform` as node terms: channel adjustments, master adjustment, start, and width in stops."""

    rgb: tuple[Expr, Expr, Expr]
    master: Expr
    start: Expr
    width: Expr


class MatrixRows(msgspec.Struct, frozen=True, kw_only=True):
    """Rows of an RGB matrix and its offset, the values of the derived matrix sockets."""

    rows: tuple[tuple[Finite, Finite, Finite], tuple[Finite, Finite, Finite], tuple[Finite, Finite, Finite]] = ((1.0, 0.0, 0.0), (0.0, 1.0, 0.0), (0.0, 0.0, 1.0))
    offset: tuple[Finite, Finite, Finite] = (0.0, 0.0, 0.0)

    @property
    def values(self) -> dict[Key, Value]:
        """Matrix socket values by key, rows then offset."""
        return dict(zip((socket.key for socket in MATRIX), (*self.rows, self.offset)))

    @classmethod
    def of(cls, config: PyOpenColorIO.Config, transform: PyOpenColorIO.Transform) -> Outcome[Self]:
        """Reduces a transform through its optimized processor to one matrix, identity for no operation, refusing any other operation."""

        def reduced(processed: PyOpenColorIO.Processor) -> Outcome[Self]:
            match list(processed.getOptimizedProcessor(PyOpenColorIO.OPTIMIZATION_DEFAULT).createGroupTransform()):
                case []:
                    return Ok(cls())
                case [PyOpenColorIO.MatrixTransform() as matrix]:
                    return Ok(msgspec.convert({"rows": np.asarray(matrix.getMatrix(), dtype=np.float64).reshape(4, 4)[:3, :3].tolist(), "offset": matrix.getOffset()[:3]}, cls))
                case operations:
                    return Error(
                        Fault.of(RecordReason.OUTSIDE, type(transform).__name__, ", ".join(type(operation).__name__ for operation in operations), member=PyOpenColorIO.MatrixTransform.__name__)
                    )

        return processor(config, transform).bind(reduced)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MATRIX]


def matrixed(color: Expr, rows: tuple[Expr, Expr, Expr], offset: Expr) -> Expr:
    """Applies RGB matrix rows and their offset, the `MATRIX` inputs, to straight RGB."""
    matrix = Expr(
        kind=bpy.types.FunctionNodeCombineMatrix,
        inputs=tuple(
            cell
            for row, (held, axis) in enumerate(zip(rows, AXES), 1)
            for cell in (*((f"Column {column} Row {row}", held[component]) for column, component in enumerate(AXES, 1)), (f"Column 4 Row {row}", offset[axis]))
        ),
        socket="Matrix",
    )
    return Expr(kind=bpy.types.FunctionNodeTransformPoint, inputs=(("Vector", color), ("Transform", matrix)), socket="Vector", shape="VECTOR")


# --- [CDL]


def cdl(color: Expr, slope: Expr, offset: Expr, power: Expr, saturation: Scalar) -> Expr:
    """ASC CDL in OCIO's `CDL_NO_CLAMP` style, negative slopes passing power unchanged, saturation about OCIO's CDL luma."""
    sloped = vector("MULTIPLY_ADD", color, slope, offset)
    graded = vector("POWER", vector("MAXIMUM", sloped, (0.0, 0.0, 0.0)), power) + vector("MINIMUM", sloped, (0.0, 0.0, 0.0))
    luma = vector("DOT_PRODUCT", graded, tuple(PyOpenColorIO.CDLTransform().getSatLumaCoefs()))
    return vector("MULTIPLY_ADD", graded - luma, saturation, luma)


# --- [PRIMARY]


def primary(
    color: Expr,
    *,
    offset: Expr | tuple[float, float, float] = (0.0, 0.0, 0.0),
    exposure: Expr | tuple[float, float, float] = (0.0, 0.0, 0.0),
    contrast: Expr | tuple[float, float, float] = (1.0, 1.0, 1.0),
    pivot: Option[Scalar] = Nothing,
    saturation: Scalar = 1.0,
) -> Expr:
    """`GradingPrimaryTransform` in `GRADING_LIN` style: offset, exposure in stops, signed power contrast about `GREY * 2^pivot` its pivot OCIO's default when absent, and saturation about OCIO's luma."""
    exposed = vector("MULTIPLY", vector("ADD", color, offset), vector("POWER", (2.0, 2.0, 2.0), exposure))
    center = math("MULTIPLY", GREY, math("POWER", 2.0, pivot.default_with(lambda: PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN).pivot)))
    contrasted = vector("MULTIPLY", vector("SIGN", exposed), vector("SCALE", vector("POWER", vector("DIVIDE", vector("ABSOLUTE", exposed), xyz(center, center, center)), contrast), center))
    luma = vector("DOT_PRODUCT", contrasted, tuple(PyOpenColorIO.CDLTransform().getSatLumaCoefs()))
    return vector("MULTIPLY_ADD", contrasted - luma, saturation, luma)


# --- [TONE]


@grouped("tone_segment", ".Tone Segment", bpy.types.CompositorNodeTree)
def _segment(value: Expr | float, start: Expr | float, end: Expr | float, level: Expr | float, start_slope: Expr | float, end_slope: Expr | float) -> Expr:
    """Quadratic piece with linear slope between endpoints."""
    run = value - start
    return math("MULTIPLY_ADD", run, math("MULTIPLY_ADD", run / (end - start) * 0.5, end_slope - start_slope, start_slope), level)


@grouped("tone_inverse", ".Tone Inverse", bpy.types.CompositorNodeTree)
def _inverse(value: Expr, start: Expr, end: Expr, level: Expr, start_slope: Expr | float, end_slope: Expr | float) -> Expr:
    """Stable root of quadratic piece."""
    span, constant = end - start, level - value
    linear = start_slope * span
    root = math("SQRT", math("MULTIPLY_ADD", (end_slope - start_slope) * span * constant, -2.0, linear * linear))
    return math("MULTIPLY_ADD", constant * -2.0 / (root + linear), span, start)


@grouped("tone_pivot", ".Tone Pivot", bpy.types.CompositorNodeTree)
def _pivot(value: Expr, low: Expr, high: Expr, low_slope: Expr | float, high_slope: Expr | float, reverse: Expr) -> Expr:
    """Forward and inverse quadratic pairs joined at pivot midpoint."""
    middle, knee = math("MULTIPLY_ADD", high - low, 0.5, low), math("MULTIPLY_ADD", low_slope + high_slope, -0.5, 2.0)
    crest = _segment(middle, low, middle, low, low_slope, knee)
    below, above, under, within = value - low, value - high, value < low, value < high
    rising = where(value < middle, _segment(value, low, middle, low, low_slope, knee), _segment(value, middle, high, crest, knee, high_slope))
    falling = where(value < crest, _inverse(value, low, middle, low, low_slope, knee), _inverse(value, middle, high, crest, knee, high_slope))
    forward = where(within, where(under, math("MULTIPLY_ADD", below, low_slope, low), rising), math("MULTIPLY_ADD", above, high_slope, high))
    return switched(reverse, forward, where(within, where(under, below / low_slope + low, falling), above / high_slope + high))


@grouped("tone_slope", ".Tone Slope", bpy.types.CompositorNodeTree)
def _slope(adjustment: Expr) -> Expr:
    """OCIO end slope symmetric about adjustment one."""
    return math("MAXIMUM", 1.0 - abs(adjustment - 1.0), 0.01)


@grouped("tone_highlights", ".Tone Highlights", bpy.types.CompositorNodeTree)
def _highlights(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """Highlight pivot pair with minimum gap."""
    pivot = start + width
    return _pivot(value, math("MINIMUM", start, pivot - 0.01), pivot, 1.0, _slope(adjustment), adjustment < 1.0)


@grouped("tone_shadows", ".Tone Shadows", bpy.types.CompositorNodeTree)
def _shadows(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """Shadow pivot pair with minimum gap."""
    pivot = start - width
    return _pivot(value, pivot, math("MAXIMUM", start, pivot + 0.01), _slope(adjustment), 1.0, adjustment > 1.0)


@grouped("tone_midtones", ".Tone Midtones", bpy.types.CompositorNodeTree)
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
    levels = reduce(lambda held, span: (*held, _segment(span[1], span[0], span[1], held[-1], span[2], span[3])), spans, knots[:1])
    pieces = tuple(_segment(value, left, right, level, entering, leaving) for (left, right, entering, leaving), level in zip(spans, levels))
    picked = reduce(lambda previous, step: where(value < step[0], previous, step[1]), zip(knots[1:-1], pieces[1:]), pieces[0])
    return where(value < high, where(value < low, value, picked), value - high + levels[-1])


@grouped("tone_whites", ".Tone Whites", bpy.types.CompositorNodeTree)
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


@grouped("tone_blacks", ".Tone Blacks", bpy.types.CompositorNodeTree)
def _blacks(value: Expr, adjustment: Expr, start: Expr, width: Expr) -> Expr:
    """Black compression and inverse expansion from shadow-derived start."""
    low, slope = start - width, _slope(adjustment)
    mean = slope + 1.0
    floor, gain, inside = start - mean * (width * 0.5), mean * 0.5, value < start
    falling = where(value < low, math("MULTIPLY_ADD", value - low, slope, floor), where(inside, _segment(value, low, start, floor, slope, 1.0), value))
    scaled = math("MULTIPLY_ADD", value - start, gain, start)
    inner = where(scaled < floor, (scaled - floor) / slope + low, _inverse(scaled, low, start, floor, slope, 1.0))
    return switched(adjustment > 1.0, where(inside, (inner - start) / gain + start, value), falling)


@grouped("tone_contrast_end", ".Tone Contrast End", bpy.types.CompositorNodeTree)
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


@grouped("tone_contrast", ".Tone Contrast", bpy.types.CompositorNodeTree)
def _contrast(value: Expr, contrast: Expr) -> Expr:
    """S-contrast with independent positive and negative shoulders."""
    steep = 1.0 / math("MULTIPLY_ADD", math("MINIMUM", contrast, 1.99), -0.8125, 1.8125)
    gain = switched(contrast > 1.0, math("MULTIPLY_ADD", math("MAXIMUM", contrast, 0.01), 0.71875, 0.28125), steep)
    return where(value < 0.0, -_contrast_end(-value, gain, 5.5), _contrast_end(value, gain, 6.5))


def tone(color: Expr, *, blacks: Zone, shadows: Zone, midtones: Zone, highlights: Zone, whites: Zone, contrast: Expr) -> Outcome[Expr]:
    """`GradingToneTransform` in `GRADING_LIN` style on OCIO's log stop axis: zone chain per channel with master placement, then S-contrast, clamped to half range at identity.

    Args:
        color: Straight scene-linear RGB.
        blacks: Blacks zone, its start and width placed past the shadows.
        shadows: Shadows zone.
        midtones: Midtones zone.
        highlights: Highlights zone.
        whites: Whites zone, its start and width placed past the highlights.
        contrast: `scontrast` term.

    Returns:
        Graded RGB, or the stop-axis group faults.
    """
    whites_start, whites_end = (_highlights(edge, highlights.master, highlights.start, highlights.width) for edge in (whites.start, whites.start + whites.width))
    blacks_start, blacks_end = (_shadows(edge, shadows.master, shadows.start, shadows.width) for edge in (blacks.start, blacks.start - blacks.width))
    placed = (
        (midtones, _midtones, midtones.start, midtones.width),
        (highlights, _highlights, highlights.start, highlights.width),
        (whites, _whites, whites_start, whites_end - whites_start),
        (shadows, _shadows, shadows.start, shadows.width),
        (blacks, _blacks, blacks_start, blacks_start - blacks_end),
    )
    offsets = (*(deviation - 1.0 for zone in (blacks, shadows, midtones, highlights, whites) for deviation in (zone.master, *zone.rgb)), contrast - 1.0)
    total = reduce(lambda held, offset: math("MULTIPLY_ADD", offset, offset, held), offsets[1:], offsets[0] * offsets[0])

    def built(groups: tuple[NodeGroup, ...]) -> Expr:
        forward, inverse = groups
        entering = forward.placed({COLOR_INPUT.key: color}, COLOR_RESULT.key)

        def graded(index: int, value: Expr) -> Expr:
            steps = ((group, adjustment, low, extent) for zone, group, low, extent in placed for adjustment in (zone.rgb[index], zone.master))
            return _contrast(reduce(lambda held, step: step[0](held, *step[1:]), steps, value), contrast)

        linear = inverse.placed({COLOR_INPUT.key: rgba(*itertools.starmap(graded, enumerate((entering["Red"], entering["Green"], entering["Blue"]))))}, COLOR_RESULT.key)
        return switched(total > 0.0, linear, vector("MINIMUM", linear, (65504.0, 65504.0, 65504.0)))

    return accumulated(logarithmic(direction) for direction in (PyOpenColorIO.TRANSFORM_DIR_FORWARD, PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(built)


# --- [GAMUT]


def gamut_compression(color: Expr, *, limits: tuple[Expr, Expr, Expr], thresholds: tuple[Expr, Expr, Expr], power: Expr) -> Expr:
    """`FixedFunctionTransform` `FIXED_FUNCTION_ACES_GAMUT_COMP_13` with bounded powers and a defined zero-achromatic case, cyan, magenta, and yellow limits and thresholds in its parameter order."""
    channels = (color["Red"], color["Green"], color["Blue"])
    achromatic = math("MAXIMUM", math("MAXIMUM", channels[0], channels[1]), channels[2])
    sign, inverse = math("SIGN", achromatic), 1.0 / power

    def channel(value: Expr, bound: Expr, floor: Expr) -> Expr:
        headroom = 1.0 - floor
        scale = headroom / (1.0 - (headroom / (bound - floor)) ** power) ** inverse
        distance = sign - value / abs(achromatic)
        excess = math("MAXIMUM", distance - floor, 0.0)
        lower = math("MINIMUM", excess, scale)
        pulled = math("MINIMUM", distance, floor) + lower / (1.0 + (lower / math("MAXIMUM", excess, scale)) ** power) ** inverse
        return where(floor < distance, achromatic * (1.0 - sign * pulled), where(abs(sign), value, 0.0))

    return rgba(*map(channel, channels, limits, thresholds))


# --- [PQ]


def st2084(values: Expr, direction: PyOpenColorIO.TransformDirection) -> Expr:
    """SMPTE ST 2084 encoding of luminance shares of its 10000 nit peak, decoding in the inverse direction, over colour's published constants."""
    m_1, m_2, c_1, c_2, c_3 = CONSTANTS_ST2084.m_1, CONSTANTS_ST2084.m_2, CONSTANTS_ST2084.c_1, CONSTANTS_ST2084.c_2, CONSTANTS_ST2084.c_3
    match direction:
        case PyOpenColorIO.TRANSFORM_DIR_FORWARD:
            powered = vector("POWER", vector("MAXIMUM", values, (0.0, 0.0, 0.0)), (m_1,) * 3)
            return vector("POWER", vector("DIVIDE", vector("MULTIPLY_ADD", powered, (c_2,) * 3, (c_1,) * 3), vector("MULTIPLY_ADD", powered, (c_3,) * 3, (1.0,) * 3)), (m_2,) * 3)
        case _:
            powered = vector("POWER", vector("MAXIMUM", values, (0.0, 0.0, 0.0)), (1.0 / m_2,) * 3)
            return vector("POWER", vector("DIVIDE", vector("MAXIMUM", powered - (c_1,) * 3, (0.0, 0.0, 0.0)), vector("MULTIPLY_ADD", powered, (-c_3,) * 3, (c_2,) * 3)), (1.0 / m_1,) * 3)


def eetf(codes: Expr, *, source: Scalar, target: Scalar, black: Scalar = 0.0, knee: Scalar = 0.5) -> Expr:
    """BT.2390 EETF over PQ codes relative to the source peak code: hermite shoulder into the target peak and black lift, peaks in nits.

    Args:
        codes: PQ codes.
        source: Source peak luminance.
        target: Target peak luminance, the source's where higher.
        black: Target black as a share of the target peak.
        knee: Shoulder start as a share of the gap from the target peak to the source peak, BT.2390 at one half.

    Returns:
        Mapped PQ codes.
    """
    limit = math("MINIMUM", target, source)
    encoded = st2084(
        vector("DIVIDE", xyz(source, limit, math("MULTIPLY", black, limit)), (float(inspect.signature(eotf_inverse_ST2084).parameters["L_p"].default),) * 3), PyOpenColorIO.TRANSFORM_DIR_FORWARD
    )
    reference = encoded["X"]
    peak, floor = encoded["Y"] / reference, encoded["Z"] / reference
    start = math("MULTIPLY_ADD", knee, peak - 1.0, peak)
    exponent = math("MINIMUM", 1.0 / floor, 4.0)
    gain = 1.0 / ((1.0 - peak) ** exponent * (floor / peak) + 1.0)
    lift = gain * floor
    clipped = vector("MINIMUM", vector("MAXIMUM", vector("DIVIDE", codes, reference), (0.0, 0.0, 0.0)), (1.0, 1.0, 1.0))
    position = vector("MAXIMUM", clipped - start, (0.0, 0.0, 0.0)) / (1.0 - start)
    rolled = position * position * (position * (start + 1.0 - 2.0 * peak) + (peak * 3.0 - 2.0 - start)) + clipped
    lifted = rolled * gain + ((1.0 - rolled) ** exponent * lift + (floor - lift))
    return vector("SCALE", vector("MINIMUM", vector("MAXIMUM", lifted, floor), peak), reference)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["MATRIX", "MatrixRows", "Zone", "cdl", "eetf", "gamut_compression", "matrixed", "primary", "st2084", "tone"]
