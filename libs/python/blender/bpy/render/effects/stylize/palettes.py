"""Display quantization with ordered Bayer thresholds and HSY hue retention."""

import bpy
from expression import Some
import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, gridded, PixelGrid, pixels
from blender.bpy.nodes.graphs import End, ended, Fragment, math, menu, mixed, nested, node, NodeGroup, switched, vector
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, hsy
from blender.bpy.render.effects.stack import CompositorEffect, Shell
from blender.imaging.effects.stylize.palettes import Channels, Palette, Posterize
from blender.imaging.faults import paired
from blender.imaging.parameters import Gated

# --- [OPERATIONS] -----------------------------------------------------------------------


def paletted(shell: Shell[Palette]) -> BlenderResult[tuple[Fragment, End]]:
    """Quantizes straight display color and maps luminance through color ramp.

    Args:
        shell: Entry inputs and retained gradient.

    Returns:
        Straight color for stack alpha scope, or accumulated builder faults.
    """
    source = shell.source
    posterize, gradient_field = Palette.__struct_fields__
    levels, channels_field, retain_hue, dither_field, dither_cell = Posterize.__struct_fields__
    _, value = Gated.__struct_fields__
    prefix = (posterize, value)
    channels, dither, cell = ((*prefix, field) for field in (channels_field, dither_field, dither_cell))
    gradient = item_key(gradient_field, value)
    bits = range(3)
    spans = tuple(2.0**bit for bit in bits)
    periods = tuple(2.0 * span for span in spans)
    count = 4 ** len(bits)

    def retained(converters: tuple[NodeGroup, NodeGroup], frame: FrameSpace, grid: PixelGrid) -> BlenderResult[tuple[Fragment, End]]:
        forward, inverse = converters
        return Fragment.joined(
            pixels("cell_width", frame, shell.socket(*cell, value), "VECTOR"),
            vector("cell_minimum", "MAXIMUM", (1.0, 1.0, 0.0), End(node="cell_width", socket="Vector")),
            switched("cell", "VECTOR", shell.socket(*cell), (1.0, 1.0, 0.0), End(node="cell_minimum", socket="Vector")),
            math("amplitude", "MULTIPLY", shell.socket(*dither), shell.socket(*dither, value)),
            vector("cells", "DIVIDE", grid.point, End(node="cell", socket="Output")),
            vector("indices", "FLOOR", End(node="cells", socket="Vector")),
            node("coordinates", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="indices", socket="Vector")}, {}),
            vector("x_bits", "SNAP", End(node="coordinates", socket="X"), spans),
            vector("y_bits", "SNAP", End(node="coordinates", socket="Y"), spans),
            vector("sum", "ADD", End(node="x_bits", socket="Vector"), End(node="y_bits", socket="Vector")),
            vector("parity", "MODULO", End(node="sum", socket="Vector"), periods),
            vector("y_parity", "MODULO", End(node="y_bits", socket="Vector"), periods),
            vector("rank", "MULTIPLY_ADD", End(node="parity", socket="Vector"), (2.0,) * 3, End(node="y_parity", socket="Vector")),
            vector("threshold", "DOT_PRODUCT", End(node="rank", socket="Vector"), tuple(4.0 ** (len(bits) - 1 - bit) / (span * count) for bit, span in zip(bits, spans))),
            math("centered", "ADD", End(node="threshold"), 0.5 / count - 0.5),
            math("offset", "MULTIPLY", End(node="centered"), End(node="amplitude")),
            math("steps", "SUBTRACT", shell.socket(*prefix, levels), 1.0),
            node("grey", bpy.types.CompositorNodeRGBToBW, {"Image": source}, {}),
            menu("channels", "VECTOR", shell.socket(*channels), shell.items[channels], {Channels.RGB.label: source, Channels.LUMINANCE.label: End(node="grey", socket="Val")}),
            vector("scaled", "MULTIPLY_ADD", End(node="channels", socket="Output"), End(node="steps"), End(node="offset")),
            vector("half", "ADD", End(node="scaled", socket="Vector"), (0.5,) * 3),
            vector("rounded", "FLOOR", End(node="half", socket="Vector")),
            node(
                "quantized",
                bpy.types.ShaderNodeMapRange,
                {
                    "Vector": End(node="rounded", socket="Vector"),
                    "From_Min_FLOAT3": (0.0, 0.0, 0.0),
                    "From_Max_FLOAT3": End(node="steps"),
                    "To_Min_FLOAT3": (0.0, 0.0, 0.0),
                    "To_Max_FLOAT3": (1.0, 1.0, 1.0),
                },
                {"data_type": "FLOAT_VECTOR", "clamp": True},
            ),
            nested("input_hsy", forward, {COLOR_INPUT.key: source}, COLOR_RESULT.key),
            nested("quantized_hsy", forward, {COLOR_INPUT.key: End(node="quantized", socket="Vector")}, COLOR_RESULT.key),
            node("input_components", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="input_hsy", socket=COLOR_RESULT.key)}, {"mode": "RGB"}),
            node("quantized_components", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="quantized_hsy", socket=COLOR_RESULT.key)}, {"mode": "RGB"}),
            node(
                "kept_hsy",
                bpy.types.CompositorNodeCombineColor,
                {"Red": End(node="input_components", socket="Red"), "Green": End(node="quantized_components", socket="Green"), "Blue": End(node="quantized_components", socket="Blue")},
                {"mode": "RGB"},
            ),
            nested("kept_rgb", inverse, {COLOR_INPUT.key: End(node="kept_hsy", socket="Image")}, COLOR_RESULT.key),
            math("retention", "MULTIPLY", shell.socket(*prefix, retain_hue), End(node="channels", socket=Channels.RGB.label)),
            mixed("hue_retained", "RGBA", End(node="retention"), End(node="quantized", socket="Vector"), End(node="kept_rgb", socket=COLOR_RESULT.key)),
            switched("posterized", "RGBA", shell.socket(posterize), source, End(node="hue_retained", socket="Result_Color")),
            node("tone", bpy.types.CompositorNodeRGBToBW, {"Image": End(node="posterized", socket="Output")}, {}),
            node(gradient, bpy.types.ShaderNodeValToRGB, {"Fac": End(node="tone", socket="Val")}, {}, Some(shell.record.gradient.value)),
            switched("mapped", "RGBA", shell.socket(gradient_field), End(node="posterized", socket="Output"), End(node=gradient, socket="Color")),
        ).map(ended("mapped", "Output"))

    return paired(paired(hsy(PyOpenColorIO.TRANSFORM_DIR_FORWARD), hsy(PyOpenColorIO.TRANSFORM_DIR_INVERSE)), paired(framed("frame", source, shell.aspect, shell.scale), gridded("grid", source))).bind(
        lambda parts: retained(parts[0], *parts[1])
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

palette = CompositorEffect(record=Palette, graph=paletted, straight=True)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["palette", "paletted"]
