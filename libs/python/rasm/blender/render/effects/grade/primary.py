"""Native gain and offset wheels, signed contrast, and working-space vibrance."""

import bpy
from expression import Result

from rasm.blender.faults import BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, INPUTS, item_key, math, node, switched, vector
from rasm.blender.render.effects.stack import CompositorEffect, Shell
from rasm.imaging.chain import GREY
from rasm.imaging.color.processors import luma
from rasm.imaging.effects.grade.primary import Contrast, Primary, Saturation, Wheel

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WHEELS]


def graded(shell: Shell[Primary]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns additive offset followed by gain in stops."""
    channels = (("X", Wheel.red.__name__), ("Y", Wheel.green.__name__), ("Z", Wheel.blue.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    wheels = (Primary.gain.__name__, Primary.offset.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return Fragment.joined(
        *(
            math(key, "ADD", End(node=INPUTS, socket=key), master)
            for wheel in wheels
            for master in (End(node=INPUTS, socket=item_key(wheel, Wheel.master.__name__)),)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            for _, channel in channels
            for key in (item_key(wheel, channel),)
        ),
        *(node(item_key(wheel, "vector"), bpy.types.ShaderNodeCombineXYZ, {axis: End(node=item_key(wheel, channel)) for axis, channel in channels}, {}) for wheel in wheels),
        vector("gain", "POWER", (2.0, 2.0, 2.0), End(node=item_key(Primary.gain.__name__, "vector"), socket="Vector")),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        vector("offset", "ADD", shell.source, End(node=item_key(Primary.offset.__name__, "vector"), socket="Vector")),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        vector("graded", "MULTIPLY", End(node="offset", socket="Vector"), End(node="gain", socket="Vector")),
    ).map(ended("graded", "Vector"))


# --- [CONTRAST]


def contrasted(shell: Shell[Contrast]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns signed power contrast about the scene-linear grey pivot."""
    pivot, power = End(node="pivot"), End(node="pivot_power")
    channels = (("X", Wheel.red.__name__), ("Y", Wheel.green.__name__), ("Z", Wheel.blue.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return Fragment.joined(
        math(power.node, "POWER", 2.0, End(node=INPUTS, socket=item_key(Contrast.pivot.__name__))),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        math(pivot.node, "MULTIPLY", GREY, power),
        node("channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": shell.source}, {}),
        *(
            Fragment.joined(
                math(sign.node, "SIGN", value),
                math(magnitude.node, "ABSOLUTE", value),
                math(normalized.node, "DIVIDE", magnitude, pivot),
                math(raised.node, "POWER", normalized, End(node=INPUTS, socket=item_key(Contrast.slope.__name__))),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
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
        Native body and result endpoint, or imaging and graph construction faults.
    """
    entering = shell.source
    scale, vibrance = (End(node=INPUTS, socket=item_key(field)) for field in (Saturation.scale.__name__, Saturation.vibrance.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
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

    weights: BlenderResult[tuple[float, ...]] = luma(shell.config).map(lambda values: tuple(float(value) for value in values)).map_error(lambda faults: (BlenderImagingFault(faults=faults),))
    return Result[tuple[float, ...], BlenderFaults].bind(weights, built)


# --- [COMPOSITION] ----------------------------------------------------------------------

primary = CompositorEffect(record=Primary, graph=graded, straight=True)
contrast = CompositorEffect(record=Contrast, graph=contrasted, straight=True)
saturation = CompositorEffect(record=Saturation, graph=saturated, straight=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["contrast", "contrasted", "graded", "primary", "saturated", "saturation"]
