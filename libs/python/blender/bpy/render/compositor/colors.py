"""Compositor color graph operations: colorspace conversion, straight-color scopes, OCIO coordinate groups, matrix forms, channel ordering, and luma-chroma planes."""

from collections.abc import Callable
import functools
from math import log
from typing import Final, Literal

import bpy
from expression import Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Component, Expr, INPUTS, math, mixed, NodeGroup, rgba, Term, vector, where, xyz
from blender.bpy.nodes.interface import Interface, Socket
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import lin_to_log, luma
from blender.records.faults import DomainFault, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

COLOR_INPUT: Final = Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor)
COLOR_RESULT: Final = Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor)
AXES: Final[tuple[Component, Component, Component]] = ("X", "Y", "Z")

# --- [MODELS] ---------------------------------------------------------------------------


class HsyLinear(msgspec.Struct, frozen=True, kw_only=True):
    """Constants of OCIO's `FIXED_FUNCTION_RGB_TO_HSY_LIN`: the luma ramp between low and high saturation forms, the low form's gain, and the high form's chroma floor, offset, and gain."""

    ramp: tuple[float, float] = (0.001, 0.01)
    low_gain: float = 5.0
    floor: float = 0.07
    offset: float = 0.15
    gain: float = 1.4


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPACES]


def space_item(config: PyOpenColorIO.Config, name: str) -> BlenderResult[str]:
    """Returns conversion-node item of a role or colorspace."""
    return (
        Ok(name)
        if name == PyOpenColorIO.ROLE_SCENE_LINEAR
        else Option.of_optional(config.getColorSpace(name)).map(PyOpenColorIO.ColorSpace.getName).to_result_with(lambda: (BlenderAbsentFault(owner=config.getName(), key=name),))
    )


def converted(key: Key, config: PyOpenColorIO.Config, source: str, target: str, entering: Term) -> BlenderResult[Expr]:
    """Converts straight RGB between colorspaces, scene-linear role resolved live."""
    return paired(space_item(config, source), space_item(config, target)).map(
        lambda pair: Expr(
            kind=bpy.types.CompositorNodeConvertColorSpace, inputs=(("Image", entering),), members=(("from_color_space", pair[0]), ("to_color_space", pair[1])), socket="Image", shape="RGBA"
        ).named(key)
    )


def pointwise(key: Key, entering: Term, operation: Callable[[Expr], BlenderResult[Expr]]) -> BlenderResult[Expr]:
    """Applies straight-color graph to associated RGB, zero alpha keeping transformed RGB."""
    alpha = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", entering),), members=(("mode", "RGB"),), socket="Alpha")

    def associated(adjusted: Expr) -> Expr:
        restored = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", adjusted), ("Alpha", alpha), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")
        transparent = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", alpha), ("B", 0.0), ("Epsilon", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
        premultiplied = Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", restored), ("Type", "To Premultiplied")), socket="Image", shape="RGBA")
        return where(transparent, restored, premultiplied).named(key)

    return operation(Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", entering), ("Type", "To Straight")), socket="Image", shape="RGBA")).map(associated)


# --- [COORDINATES]


def _converter(key: Key, name: str, body: Callable[[Expr, Expr, Expr], tuple[Expr, Expr, Expr]]) -> BlenderResult[NodeGroup]:
    """Converts RGB channels and passes alpha unchanged."""
    image = Expr(kind=bpy.types.NodeGroupInput, socket=COLOR_INPUT.key, shape="RGBA", label=Some(INPUTS))
    return NodeGroup.built(key, name, Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: rgba(*body(image["Red"], image["Green"], image["Blue"]), image["Alpha"])})


@functools.cache
def logarithmic(direction: PyOpenColorIO.TransformDirection) -> BlenderResult[NodeGroup]:
    """Builds OCIO channel stop coordinates from LogCamera parameters."""
    transform = lin_to_log()
    base, forward = transform.getBase(), direction == PyOpenColorIO.TRANSFORM_DIR_FORWARD
    log_slope, log_offset, lin_slope, lin_offset, lin_break, linear_slope = (
        transform.getLogSideSlopeValue(),
        transform.getLogSideOffsetValue(),
        transform.getLinSideSlopeValue(),
        transform.getLinSideOffsetValue(),
        transform.getLinSideBreakValue(),
        transform.getLinearSlopeValue(),
    )
    log_break = tuple(side * log(slope * point + offset, base) + shift for side, shift, slope, offset, point in zip(log_slope, log_offset, lin_slope, lin_offset, lin_break))
    linear_offset = tuple(held - slope * point for held, slope, point in zip(log_break, linear_slope, lin_break))

    def channel(value: Expr, at: int) -> Expr:
        if forward:
            logged = math("MULTIPLY_ADD", math("LOGARITHM", math("MULTIPLY_ADD", value, lin_slope[at], lin_offset[at]), base), log_slope[at], log_offset[at])
            return where(value < lin_break[at], math("MULTIPLY_ADD", value, linear_slope[at], linear_offset[at]), logged)
        powered = math("POWER", base, math("MULTIPLY_ADD", value, 1.0 / log_slope[at], -log_offset[at] / log_slope[at]))
        linear = math("MULTIPLY_ADD", value, 1.0 / linear_slope[at], -linear_offset[at] / linear_slope[at])
        return where(value < log_break[at], linear, math("MULTIPLY_ADD", powered, 1.0 / lin_slope[at], -lin_offset[at] / lin_slope[at]))

    return _converter(
        "lin_to_log" if forward else "log_to_lin", "Lin to Log" if forward else "Log to Lin", lambda *channels: (channel(channels[0], 0), channel(channels[1], 1), channel(channels[2], 2))
    )


def hsy(config: PyOpenColorIO.Config, direction: PyOpenColorIO.TransformDirection) -> BlenderResult[NodeGroup]:
    """Builds OCIO linear HSY coordinates as compositor math nodes over the configuration's scene-linear luma weights."""
    return Result[npt.NDArray[np.float64], BlenderFaults].bind(luma(config), lambda weights: _hsy(direction, tuple(map(float, weights))))


@functools.cache
def _hsy(direction: PyOpenColorIO.TransformDirection, weights: tuple[float, ...]) -> BlenderResult[NodeGroup]:
    """HSY group of one direction and luma weight triple."""
    constants = HsyLinear()
    low_luma, high_luma = constants.ramp
    forward = direction == PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def luminance(values: tuple[Expr, Expr, Expr]) -> Expr:
        red, green, blue = (value * weight for value, weight in zip(values, weights))
        return red + green + blue

    def lit(level: Expr) -> Expr:
        return math("MULTIPLY_ADD", level, 1.0 / (high_luma - low_luma), -low_luma / (high_luma - low_luma), clamp=True)

    def converted(first: Expr, second: Expr, third: Expr) -> tuple[Expr, Expr, Expr]:
        channels = first, second, third
        level = luminance(channels)
        distance = abs(first - level) + abs(second - level) + abs(third - level)
        high = math("MAXIMUM", first, math("MAXIMUM", second, third))
        low = math("MINIMUM", first, math("MINIMUM", second, third))
        delta = high - low
        sectors = tuple((channels[(index + 1) % 3] - channels[(index + 2) % 3]) / delta + float(2 * index + 1) for index in range(3))
        hue = where(low < high, where(first < high, where(second < high, sectors[2], sectors[1]), sectors[0]), 0.0) / 6.0
        high_saturation = distance / math("MAXIMUM", math("MULTIPLY_ADD", distance, constants.floor, 1e-6), first + second + third + constants.offset)
        return hue, mixed(lit(level), distance * constants.low_gain, high_saturation) * constants.gain, level

    def inverted(hue: Expr, saturation: Expr, level: Expr) -> tuple[Expr, Expr, Expr]:
        sector = math("FRACT", math("MULTIPLY_ADD", level < 0.0, 0.5, hue) - 1.0 / 6.0) * 6.0
        unit = (math("SUBTRACT", abs(sector - 3.0), 1.0, clamp=True), math("SUBTRACT", 2.0, abs(sector - 2.0), clamp=True), math("SUBTRACT", 2.0, abs(sector - 4.0), clamp=True))
        scale = level / luminance(unit)
        bases = tuple(value * scale for value in unit)
        offsets = tuple(value - level for value in bases)
        distance = abs(offsets[0]) + abs(offsets[1]) + abs(offsets[2])
        chroma, triple = saturation / constants.gain, level * 3.0
        excess, reach = bases[0] + bases[1] + bases[2] - triple, triple + constants.offset
        chroma_excess, chroma_reach = chroma * excess, chroma * reach
        high_gain = math("MINIMUM", chroma_reach / math("MAXIMUM", distance - chroma_excess, 1e-6), 50.0)
        low_distance = distance * constants.low_gain
        blend = lit(level)
        low_weight = low_distance * (1.0 - blend)
        minus_b = chroma_excess - math("MULTIPLY_ADD", distance, blend, low_weight * reach)
        discriminant = math("SQRT", math("MULTIPLY_ADD", low_weight * excess * chroma_reach, 4.0, minus_b * minus_b))
        twice_c = chroma_reach * -2.0
        near = twice_c / (minus_b - discriminant)
        far = twice_c / math("MULTIPLY_ADD", discriminant, 2.0, minus_b - discriminant)
        gain = where(blend < 1.0, where(blend > 0.0, where(near < 0.0, far, near), chroma / math("MAXIMUM", low_distance, 1e-10)), high_gain)
        red, green, blue = (math("MULTIPLY_ADD", offset, gain, level) for offset in offsets)
        return red, green, blue

    return _converter("rgb_to_hsy" if forward else "hsy_to_rgb", "RGB to HSY" if forward else "HSY to RGB", converted if forward else inverted)


# --- [MATRICES]


def constant(values: npt.NDArray[np.float64]) -> Expr:
    """Combine Matrix node of a literal matrix up to 4x4."""
    return Expr(kind=bpy.types.FunctionNodeCombineMatrix, inputs=tuple((f"Column {column + 1} Row {row + 1}", float(value)) for (row, column), value in np.ndenumerate(values)), socket="Matrix")


def composed(first: Expr, second: Expr) -> Expr:
    """Matrix product of two matrix terms, `second` applied first."""
    return Expr(kind=bpy.types.FunctionNodeMatrixMultiply, inputs=(("Matrix", first), ("Matrix_001", second)), socket="Matrix")


def inverted(matrix: Expr) -> Expr:
    """Inverse of a matrix term, its `Invertible` output beside."""
    return Expr(kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", matrix),), socket="Matrix")


def directed(direction: Term, matrix: Expr) -> Expr:
    """Direction transformed by a matrix term."""
    return Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", direction), ("Transform", matrix)), socket="Direction", shape="VECTOR")


def spanned(points: tuple[Expr, ...], white: Term) -> Expr:
    """RGB-to-XYZ matrix of chromaticity points, columns scaled to the XYZ white."""
    columns = Expr(
        kind=bpy.types.FunctionNodeCombineMatrix,
        inputs=tuple(
            entry
            for column, point in enumerate(points, 1)
            for entry in ((f"Column {column} Row 1", point["X"] / point["Y"]), (f"Column {column} Row 2", 1.0), (f"Column {column} Row 3", (1.0 - point["X"] - point["Y"]) / point["Y"]))
        ),
        socket="Matrix",
    )
    scales = directed(white, inverted(columns))
    return composed(columns, Expr(kind=bpy.types.FunctionNodeCombineMatrix, inputs=tuple((f"Column {index} Row {index}", scales[axis]) for index, axis in enumerate(AXES, 1)), socket="Matrix"))


# --- [CHANNELS]


def extreme(operation: Literal["MINIMUM", "MAXIMUM"], value: Expr) -> Expr:
    """Smallest or largest component of a vector."""
    x, y, z = (value[axis] for axis in AXES)
    return math(operation, math(operation, x, y), z)


def nonnegative(color: Expr) -> Expr:
    """Desaturates negative channels toward the non-negative channel mean."""
    level = math("MAXIMUM", vector("DOT_PRODUCT", color, (1.0 / 3.0,) * 3), 0.0)
    return mixed(math("MINIMUM", level / (level - extreme("MINIMUM", color)), 1.0), xyz(level, level, level), color)


def preserved(rendering: Expr, shaped: Expr, hue: Expr) -> Expr:
    """Restores the rendering channel order's hue across curved channels, channel sum kept."""
    swapped = rendering["X"] > rendering["Y"]

    def xy(value: Expr) -> tuple[Expr, Expr]:
        return where(swapped, value["Y"], value["X"]), where(swapped, value["X"], value["Y"])

    low, high = xy(rendering)
    below, above = rendering["Z"] < low, rendering["Z"] > high

    def ranked(value: Expr) -> tuple[Expr, Expr, Expr]:
        smaller, larger = xy(value)
        return where(below, value["Z"], smaller), where(above, larger, where(below, smaller, value["Z"])), where(above, value["Z"], larger)

    (r_min, r_mid, r_max), (f_min, f_mid, f_max) = ranked(rendering), ranked(shaped)
    share = (r_mid - r_min) / (r_max - r_min)
    naive = mixed(hue, f_mid, math("MULTIPLY_ADD", f_max - f_min, share, f_min))
    total = mixed(r_min / (r_min + r_mid) * 2.0, f_min + f_max + naive, vector("DOT_PRODUCT", shaped, (1.0,) * 3))
    rest_low, rest_high = total - f_max, total - f_min
    mid_low = mixed(hue, f_mid, mixed(share, rest_low, f_max)) / math("MULTIPLY_ADD", hue, 1.0 - share, 1.0)
    mid_high = mixed(hue, f_mid, mixed(share, f_min, rest_high)) / math("MULTIPLY_ADD", hue, share, 1.0)
    high_hue = naive > f_mid
    middle, minimum, maximum = where(high_hue, mid_high, mid_low), where(high_hue, f_min, rest_low - mid_low), where(high_hue, rest_high - mid_high, f_max)
    toward = vector("MULTIPLY_ADD", vector("SIGN", rendering - r_max), maximum - middle, maximum + minimum - middle)
    return vector("MULTIPLY_ADD", vector("SIGN", rendering - r_min), middle - minimum, toward)


# --- [LUMA_CHROMA]


def ycc(color: Expr, weights: tuple[float, float, float]) -> tuple[Expr, Expr, Expr]:
    """Splits RGB into luma and blue and red differences, each difference scaled to unit swing, over a space's luma weights.

    Args:
        color: RGB expression.
        weights: Luma weights of the space, the Y row of its RGB-to-XYZ matrix.

    Returns:
        Luma, blue difference, and red difference.
    """
    red, _, blue = weights
    level = vector("DOT_PRODUCT", color, weights)
    channels = Expr(kind=bpy.types.ShaderNodeSeparateXYZ, inputs=(("Vector", color),))
    return level, (msgspec.structs.replace(channels, socket="Z") - level) / (2.0 * (1.0 - blue)), (msgspec.structs.replace(channels, socket="X") - level) / (2.0 * (1.0 - red))


def from_ycc(level: Expr, blue_difference: Expr, red_difference: Expr, weights: tuple[float, float, float]) -> Expr:
    """Joins luma and unit-swing blue and red differences into RGB over a space's luma weights, the inverse of `ycc`."""
    red_weight, green_weight, blue_weight = weights
    red, blue = level + red_difference * (2.0 * (1.0 - red_weight)), level + blue_difference * (2.0 * (1.0 - blue_weight))
    return xyz(red, (level - red * red_weight - blue * blue_weight) / green_weight, blue)


# --- [WHITE]


def whitened(config: PyOpenColorIO.Config, plane: npt.NDArray[np.float32]) -> BlenderResult[float]:
    """Returns the percentile white of covered scene-linear pixels, refusing a frame holding no covered finite luminance."""
    return (
        Samples
        .of(config, plane)
        .bind(lambda samples: samples.extent().to_result_with(lambda: (DomainFault(subject="White metering", value=plane.shape, accepted="covered pixels holding finite luminance"),)))
        .map(lambda extent: extent.white)
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AXES",
    "COLOR_INPUT",
    "COLOR_RESULT",
    "HsyLinear",
    "composed",
    "constant",
    "converted",
    "directed",
    "extreme",
    "from_ycc",
    "hsy",
    "inverted",
    "logarithmic",
    "nonnegative",
    "pointwise",
    "preserved",
    "space_item",
    "spanned",
    "whitened",
    "ycc",
]
