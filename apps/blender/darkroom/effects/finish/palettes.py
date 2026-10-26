"""Display palettes: posterization with HSY hue retention under an ordered Bayer threshold, then a luminance gradient map."""

from typing import Annotated

import bpy
from expression import Some
import msgspec
import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, mixed, NodeGroup, rgba, switched, vector
from blender.bpy.nodes.interface import item_key
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, hsy
from blender.bpy.render.compositor.coordinates import gridded, pixels
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell
from blender.imaging.color.processors import Domain
from blender.imaging.geometry.regions import FrameLength
from blender.records.faults import paired
from blender.records.fields import Choice, Gated, Presentation, Unit

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class Channels(Choice):
    """Color channels a posterize rounds."""

    RGB = 1, "RGB"
    LUMINANCE = 3


# --- [MODELS] ---------------------------------------------------------------------------


class Posterize(msgspec.Struct, frozen=True, kw_only=True):
    """Rounds display values to evenly spaced levels with hue retention and Bayer dithering."""

    levels: Annotated[int, msgspec.Meta(ge=2, description="Values each channel rounds to, evenly spaced from 0 to 1"), Presentation(soft=Some((2.0, 256.0)))] = 9
    channels: Annotated[Channels, msgspec.Meta(description="Each RGB channel or luminance as gray")] = Channels.RGB
    retain_hue: Annotated[Unit, msgspec.Meta(description="Blend toward input hue at rounded HSY saturation and luminance")] = 0.0
    dither: Annotated[Gated[Unit], msgspec.Meta(description="Ordered 8-by-8 Bayer offset of rounding threshold in levels")] = Gated(enabled=False, value=1.0)
    dither_cell: Annotated[Gated[Annotated[FrameLength, Presentation(soft=Some((0.0, 0.01)))]], msgspec.Meta(description="Square Bayer cell size, one pixel when off or smaller")] = Gated(
        enabled=False, value=0.0
    )


class Palette(Effect, frozen=True, kw_only=True):
    """Rounds display colors to evenly spaced levels, then maps luminance through a color ramp."""

    stage = Stage.PALETTE
    pointwise = False
    posterize: Annotated[Gated[Posterize], msgspec.Meta(description="Rounding to evenly spaced levels")] = Gated(enabled=True, value=Posterize())
    gradient: Annotated[Gated[Ramp], msgspec.Meta(title="Gradient Map", description="Luminance mapped through a color ramp")] = Gated(enabled=False, value=Ramp())


# --- [OPERATIONS] -----------------------------------------------------------------------


def paletted(shell: Shell[Palette]) -> BlenderResult[Expr]:
    """Quantizes straight display color under an 8-by-8 Bayer threshold, three interleaved bit planes of the cell index, and maps luminance through the color ramp.

    Args:
        shell: Entry inputs and retained gradient.

    Returns:
        Straight color, or the HSY group faults.
    """
    posterize, gradient = Palette.__struct_fields__
    levels, channels, retain_hue, dither, dither_cell = Posterize.__struct_fields__
    _, value = Gated.__struct_fields__
    held = (posterize, value)
    source = shell.color
    spans = (1.0, 2.0, 4.0)
    ranks = 4 ** len(spans)

    def built(forward: NodeGroup, inverse: NodeGroup) -> Expr:
        def toned(image: Expr) -> Expr:
            return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")

        cell = switched(shell.gate(*held, dither_cell), (1.0, 1.0, 0.0), vector("MAXIMUM", (1.0, 1.0, 0.0), pixels(shell.space, shell.input(*held, dither_cell), "VECTOR")))
        indices = vector("FLOOR", vector("DIVIDE", gridded(source).point, cell))
        x_bits, y_bits = (vector("SNAP", indices[axis], spans) for axis in ("X", "Y"))
        periods = tuple(2.0 * span for span in spans)
        rank = vector("MULTIPLY_ADD", vector("MODULO", x_bits + y_bits, periods), (2.0,) * 3, vector("MODULO", y_bits, periods))
        weights = tuple(4.0 ** (len(spans) - 1 - bit) / (span * ranks) for bit, span in enumerate(spans))
        offset = (vector("DOT_PRODUCT", rank, weights) + (0.5 / ranks - 0.5)) * (shell.gate(*held, dither) * shell.input(*held, dither))
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

    return paired(hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_FORWARD), hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_INVERSE)).map(lambda groups: built(*groups))


# --- [COMPOSITION] ----------------------------------------------------------------------

palette = CompositorEffect(record=Palette, graph=paletted, domain=Domain.DISPLAY, alpha=Alpha.STRAIGHT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channels", "Palette", "Posterize", "palette"]
