"""Display palettes: posterization with HSY hue retention under an ordered Bayer threshold, then a luminance gradient map."""

from typing import Annotated, Final

import bpy
from expression import Some
import msgspec
import PyOpenColorIO

from blender.bpy.nodes.graphs import Expr, mixed, NodeGroup, rgba, switched, vector
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, hsy
from blender.bpy.render.compositor.coordinates import gridded, pixels
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell
from blender.imaging.color.processors import Domain
from blender.records.faults import Outcome, paired

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Palette(Effect, frozen=True, kw_only=True):
    """Rounds display colors to evenly spaced levels, then maps luminance through a color ramp."""

    stage = Stage.PALETTE
    pointwise = False
    gradient: Annotated[Ramp, msgspec.Meta(description="Scene colors along luminance 0 to 1 of the gradient map")] = Ramp()


# --- [OPERATIONS] -----------------------------------------------------------------------


def paletted(shell: Shell[Palette]) -> Outcome[Expr]:
    """Quantizes straight display color under an 8-by-8 Bayer threshold, three interleaved bit planes of the cell index, and maps luminance through the color ramp.

    Args:
        shell: Entry inputs and gradient ramp.

    Returns:
        Straight color, or the HSY group and channel menu faults.
    """
    source = shell.color
    spans = (1.0, 2.0, 4.0)
    ranks = 4 ** len(spans)

    def toned(image: Expr) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeRGBToBW, inputs=(("Image", image),), socket="Val")

    def built(forward: NodeGroup, inverse: NodeGroup, chosen: Expr) -> Expr:
        cell = switched(shell.input("posterize_dither_cell"), (1.0, 1.0, 0.0), vector("MAXIMUM", (1.0, 1.0, 0.0), pixels(shell.space, shell.input("posterize_dither_cell_value"), "VECTOR")))
        indices = vector("FLOOR", vector("DIVIDE", gridded(source).point, cell))
        x_bits, y_bits = (vector("SNAP", indices[axis], spans) for axis in ("X", "Y"))
        periods = tuple(2.0 * span for span in spans)
        rank = vector("MULTIPLY_ADD", vector("MODULO", x_bits + y_bits, periods), (2.0,) * 3, vector("MODULO", y_bits, periods))
        weights = tuple(4.0 ** (len(spans) - 1 - bit) / (span * ranks) for bit, span in enumerate(spans))
        offset = (vector("DOT_PRODUCT", rank, weights) + (0.5 / ranks - 0.5)) * (shell.input("posterize_dither") * shell.input("posterize_dither_value"))
        steps = shell.input("posterize_levels") - 1.0
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
        retention = shell.input("posterize_retain_hue") * msgspec.structs.replace(chosen, socket="RGB", shape="FLOAT")
        posterized = switched(shell.input("posterize"), source, mixed(retention, quantized, kept))
        ramp = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", toned(posterized)),), content=Some(shell.record.gradient), socket="Color", shape="RGBA").named("gradient_value")
        return switched(shell.input("gradient"), posterized, ramp)

    return paired(
        paired(hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_FORWARD), hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_INVERSE)),
        shell.menu("posterize_channels", cases={"RGB": source, "Luminance": toned(source)}, kind="VECTOR"),
    ).map(lambda parts: built(*parts[0], parts[1]))


# --- [COMPOSITION] ----------------------------------------------------------------------

palette: Final = CompositorEffect(
    record=Palette,
    graph=paletted,
    interface=Interface(
        panels=(
            InterfacePanel(
                key="posterize",
                name="Posterize",
                description="Rounds display values to evenly spaced levels with hue retention and Bayer dithering",
                toggle=Some(True),
                inputs=(
                    Socket(
                        key="posterize_levels",
                        name="Levels",
                        kind=bpy.types.NodeSocketInt,
                        description="Values each channel rounds to, evenly spaced from 0 to 1",
                        members={"min_value": 2, "default_value": 9},
                    ),
                    Socket(
                        key="posterize_channels",
                        name="Channels",
                        kind=bpy.types.NodeSocketMenu,
                        description="Each RGB channel or luminance as gray",
                        members={"default_value": "RGB"},
                        items=Some(MenuItems(items=(MenuItem(name="RGB"), MenuItem(name="Luminance")))),
                    ),
                    Socket(
                        key="posterize_retain_hue",
                        name="Retain Hue",
                        kind=bpy.types.NodeSocketFloat,
                        description="Blend toward input hue at rounded HSY saturation and luminance",
                        members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0},
                    ),
                ),
                panels=tuple(
                    InterfacePanel(
                        key=item_key("posterize", field),
                        name=bpy.path.display_name(field),
                        description=description,
                        toggle=Some(False),
                        inputs=(Socket(key=item_key("posterize", field, "value"), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, members={"subtype": subtype, **members}),),
                    )
                    for field, subtype, description, members in (
                        ("dither", "FACTOR", "Ordered 8-by-8 Bayer offset of rounding threshold in levels", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                        ("dither_cell", "NONE", "Square Bayer cell size, one pixel when off or smaller", {"min_value": 0.0}),
                    )
                ),
            ),
            InterfacePanel(key="gradient", name="Gradient Map", description="Luminance mapped through a color ramp", toggle=Some(False)),
        )
    ),
    domain=Domain.DISPLAY,
    alpha=Alpha.STRAIGHT,
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Palette", "palette"]
