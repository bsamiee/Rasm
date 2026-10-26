"""Display quantization with ordered Bayer thresholds and HSY hue retention."""

from typing import Final

import bpy
from expression import Some
import msgspec
import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, mixed, NodeGroup, rgba, switched, vector
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, hsy
from blender.bpy.render.effects.coordinates import gridded, pixels
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Shell
from blender.imaging.effects.stylize.palettes import Channels, Palette, Posterize
from blender.imaging.faults import paired
from blender.imaging.parameters import Gated

# --- [CONSTANTS] ------------------------------------------------------------------------

_SPANS: Final = (1.0, 2.0, 4.0)
_RANKS: Final = 4 ** len(_SPANS)
_WEIGHTS: Final = tuple(4.0 ** (len(_SPANS) - 1 - bit) / (span * _RANKS) for bit, span in enumerate(_SPANS))

# --- [OPERATIONS] -----------------------------------------------------------------------


def paletted(shell: Shell[Palette]) -> BlenderResult[Expr]:
    """Quantizes straight display color and maps luminance through color ramp.

    Args:
        shell: Entry inputs and retained gradient.

    Returns:
        Straight color for stack alpha scope, or HSY group faults.
    """
    posterize, gradient = Palette.__struct_fields__
    levels, channels, retain_hue, dither, dither_cell = Posterize.__struct_fields__
    _, value = Gated.__struct_fields__
    held = (posterize, value)
    source = shell.color

    def built(forward: NodeGroup, inverse: NodeGroup) -> Expr:
        def toned(image: Expr) -> Expr:
            return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")

        cell = switched(shell.gate(*held, dither_cell), (1.0, 1.0, 0.0), vector("MAXIMUM", (1.0, 1.0, 0.0), pixels(shell.space, shell.input(*held, dither_cell), "VECTOR")))
        indices = vector("FLOOR", vector("DIVIDE", gridded(source).point, cell))
        x_bits, y_bits = (vector("SNAP", indices[axis], _SPANS) for axis in ("X", "Y"))
        periods = tuple(2.0 * span for span in _SPANS)
        rank = vector("MULTIPLY_ADD", vector("MODULO", x_bits + y_bits, periods), (2.0,) * 3, vector("MODULO", y_bits, periods))
        offset = (vector("DOT_PRODUCT", rank, _WEIGHTS) + (0.5 / _RANKS - 0.5)) * (shell.gate(*held, dither) * shell.input(*held, dither))
        steps = shell.input(*held, levels) - 1.0
        chosen = shell.menu(*held, channels, cases={Channels.RGB.label: source, Channels.LUMINANCE.label: toned(source)}, kind="VECTOR")
        quantized = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(
                ("Vector", vector("FLOOR", vector("MULTIPLY_ADD", chosen, steps, offset) + (0.5,) * 3)),
                ("From_Min_FLOAT3", (0.0, 0.0, 0.0)),
                ("From_Max_FLOAT3", steps),
                ("To_Min_FLOAT3", (0.0, 0.0, 0.0)),
                ("To_Max_FLOAT3", (1.0, 1.0, 1.0)),
            ),
            members=(("data_type", "FLOAT_VECTOR"), ("clamp", True)),
            socket="Vector",
            shape="VECTOR",
        )
        entered, rounded = (forward.placed({COLOR_INPUT.key: image}, COLOR_RESULT.key) for image in (source, quantized))
        kept = inverse.placed({COLOR_INPUT.key: rgba(entered["Red"], rounded["Green"], rounded["Blue"])}, COLOR_RESULT.key)
        retention = shell.input(*held, retain_hue) * msgspec.structs.replace(chosen, socket=Channels.RGB.label, shape="FLOAT")
        posterized = switched(shell.gate(posterize), source, mixed(retention, quantized, kept))
        ramp = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", toned(posterized)),), content=Some(shell.record.gradient.value), socket="Color", shape="RGBA").named(item_key(gradient, value))
        return switched(shell.gate(gradient), posterized, ramp)

    return paired(hsy(PyOpenColorIO.TRANSFORM_DIR_FORWARD), hsy(PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(lambda groups: built(*groups))


# --- [COMPOSITION] ----------------------------------------------------------------------

palette = CompositorEffect(record=Palette, graph=paletted, alpha=Alpha.STRAIGHT)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["palette", "paletted"]
