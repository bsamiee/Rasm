"""Gain and offset wheels, signed contrast, and working-space vibrance."""

import bpy
from expression import Result

from blender.bpy.faults import BlenderFaults, BlenderImagingFault, BlenderResult
from blender.bpy.nodes.graphs import End, ended, Fragment, INPUTS, math, node, switched, vector
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.stack import CompositorEffect, Shell
from blender.imaging.chain import GREY
from blender.imaging.color.processors import luma
from blender.imaging.effects.grade.primary import Contrast, Primary, Saturation, Wheel

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WHEELS]


def graded(shell: Shell[Primary]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns additive offset, then gain in stops."""
    red, green, blue, master = Wheel.__struct_fields__
    channels = (("X", red), ("Y", green), ("Z", blue))
    gain, offset = Primary.__struct_fields__
    return Fragment.joined(
        *(
            math(key, "ADD", End(node=INPUTS, socket=key), End(node=INPUTS, socket=item_key(wheel, master)))
            for wheel in (gain, offset)
            for _, channel in channels
            for key in (item_key(wheel, channel),)
        ),
        *(node(item_key(wheel, "vector"), bpy.types.ShaderNodeCombineXYZ, {axis: End(node=item_key(wheel, channel)) for axis, channel in channels}, {}) for wheel in (gain, offset)),
        vector("gain", "POWER", (2.0, 2.0, 2.0), End(node=item_key(gain, "vector"), socket="Vector")),
        vector("offset", "ADD", shell.source, End(node=item_key(offset, "vector"), socket="Vector")),
        vector("graded", "MULTIPLY", End(node="offset", socket="Vector"), End(node="gain", socket="Vector")),
    ).map(ended("graded", "Vector"))


# --- [CONTRAST]


def contrasted(shell: Shell[Contrast]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns signed power contrast around grey pivot."""
    pivot, power = End(node="pivot"), End(node="pivot_power")
    red, green, blue, _ = Wheel.__struct_fields__
    channels = (("X", red), ("Y", green), ("Z", blue))
    slope, pivot_field = Contrast.__struct_fields__
    return Fragment.joined(
        math(power.node, "POWER", 2.0, End(node=INPUTS, socket=item_key(pivot_field))),
        math(pivot.node, "MULTIPLY", GREY, power),
        node("channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": shell.source}, {}),
        *(
            Fragment.joined(
                math(sign.node, "SIGN", value),
                math(magnitude.node, "ABSOLUTE", value),
                math(normalized.node, "DIVIDE", magnitude, pivot),
                math(raised.node, "POWER", normalized, End(node=INPUTS, socket=item_key(slope))),
                math(scaled.node, "MULTIPLY", raised, pivot),
                math(channel, "MULTIPLY", scaled, sign),
            )
            for axis, channel in channels
            for value in (End(node="channels", socket=axis),)
            for sign, magnitude, normalized, raised, scaled in (tuple(End(node=item_key(channel, term)) for term in ("sign", "magnitude", "normalized", "power", "scaled")),)
        ),
        node("contrasted", bpy.types.ShaderNodeCombineXYZ, {axis: End(node=channel) for axis, channel in channels}, {}),
    ).map(ended("contrasted", "Vector"))


# --- [SATURATION]


def saturated(shell: Shell[Saturation]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns saturation and multiplicative vibrance about configured luminance.

    Args:
        shell: Entry inputs and working configuration supplying luminance coefficients.

    Returns:
        Body and result endpoint, or imaging and graph faults.
    """
    entering = shell.source
    scale, vibrance = (End(node=INPUTS, socket=item_key(field)) for field in Saturation.__struct_fields__)
    red, green, blue = (End(node="channels", socket=axis) for axis in ("X", "Y", "Z"))
    low_pair, low, high_pair, high, positive, ratio, sigma, inverse = (End(node=key) for key in ("low_pair", "low", "high_pair", "high", "positive", "ratio", "sigma", "inverse"))
    positive_weight, negative_square, negative_weight, increasing, weighted, multiplier, gain = (
        End(node=key) for key in ("positive_weight", "negative_square", "negative_weight", "increasing", "weighted", "multiplier", "gain")
    )

    def built(weights: tuple[float, ...]) -> BlenderResult[tuple[Fragment, End]]:
        return Fragment.joined(
            node("channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": entering}, {}),
            math(low_pair.node, "MINIMUM", red, green),
            math(low.node, "MINIMUM", low_pair, blue),
            math(high_pair.node, "MAXIMUM", red, green),
            math(high.node, "MAXIMUM", high_pair, blue),
            math(positive.node, "MAXIMUM", low, 0.0),
            math(ratio.node, "DIVIDE", positive, high),
            math(sigma.node, "SUBTRACT", 1.0, ratio),
            math(inverse.node, "SUBTRACT", 1.0, sigma),
            math(positive_weight.node, "MULTIPLY", inverse, inverse),
            math(negative_square.node, "MULTIPLY", sigma, sigma),
            math(negative_weight.node, "MULTIPLY", negative_square, 0.5),
            math(increasing.node, "GREATER_THAN", vibrance, 0.0),
            switched(weighted.node, "FLOAT", increasing, negative_weight, positive_weight),
            math(multiplier.node, "MULTIPLY_ADD", vibrance, End(node=weighted.node, socket="Output"), 1.0),
            math(gain.node, "MULTIPLY", scale, multiplier),
            vector("luma", "DOT_PRODUCT", entering, weights),
            vector("chroma", "SUBTRACT", entering, End(node="luma")),
            vector("saturated", "MULTIPLY_ADD", End(node="chroma", socket="Vector"), gain, End(node="luma")),
        ).map(ended("saturated", "Vector"))

    weights: BlenderResult[tuple[float, ...]] = luma(shell.config).map(lambda values: tuple(float(value) for value in values)).map_error(BlenderImagingFault.lifted)
    return Result[tuple[float, ...], BlenderFaults].bind(weights, built)


# --- [COMPOSITION] ----------------------------------------------------------------------

primary = CompositorEffect(record=Primary, graph=graded, straight=True)
contrast = CompositorEffect(record=Contrast, graph=contrasted, straight=True)
saturation = CompositorEffect(record=Saturation, graph=saturated, straight=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["contrast", "contrasted", "graded", "primary", "saturated", "saturation"]
