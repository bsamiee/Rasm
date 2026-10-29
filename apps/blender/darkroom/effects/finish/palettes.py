"""Display palettes: posterization with HSY hue retention under an ordered Bayer threshold, then a luminance gradient map."""

from typing import Annotated, Final

import bpy
from expression import Some
import msgspec
import PyOpenColorIO

from blender.bpy.nodes.content import Ramp
from blender.bpy.nodes.graphs import Expr, mixed, rgba, switched, vector
from blender.bpy.nodes.interface import Interface, InterfacePanel, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.colors import hsy
from blender.bpy.render.compositor.coordinates import gridded, pixels
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect
from blender.bpy.render.compositor.shells import Shell
from blender.imaging.color.processors import Domain
from blender.records.faults import Outcome, paired

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

_RGB: Final = MenuItem(name="RGB")
_LUMINANCE: Final = MenuItem(name="Luminance")
_LEVELS: Final = Socket(key="posterize_levels", name="Levels", kind=bpy.types.NodeSocketInt, description="Values each channel rounds to, evenly spaced from 0 to 1", min_value=2, default_value=9)
_CHANNELS: Final = Socket(
    key="posterize_channels",
    name="Channels",
    kind=bpy.types.NodeSocketMenu,
    description="Each RGB channel or luminance as gray",
    default_value=_RGB.name,
    items=Some(MenuItems(items=(_RGB, _LUMINANCE))),
)
_RETAIN_HUE: Final = Socket(
    key="posterize_retain_hue", name="Retain Hue", kind=bpy.types.NodeSocketFloatFactor, description="Blend toward input hue at rounded HSY saturation and luminance", min_value=0.0, max_value=1.0
)
_DITHER_VALUE: Final = Socket(key="posterize_dither_value", name="Dither", kind=bpy.types.NodeSocketFloatFactor, min_value=0.0, max_value=1.0, default_value=1.0)
_DITHER_CELL_VALUE: Final = Socket(key="posterize_dither_cell_value", name="Dither Cell", kind=bpy.types.NodeSocketFloat, min_value=0.0)
_DITHER: Final = InterfacePanel(key="posterize_dither", name="Dither", description="Ordered 8-by-8 Bayer offset of rounding threshold in levels", toggle=Some(False), inputs=(_DITHER_VALUE,))
_DITHER_CELL: Final = InterfacePanel(
    key="posterize_dither_cell", name="Dither Cell", description="Square Bayer cell size, one pixel when off or smaller", toggle=Some(False), inputs=(_DITHER_CELL_VALUE,)
)
_POSTERIZE: Final = InterfacePanel(
    key="posterize",
    name="Posterize",
    description="Rounds display values to evenly spaced levels with hue retention and Bayer dithering",
    toggle=Some(True),
    inputs=(_LEVELS, _CHANNELS, _RETAIN_HUE),
    panels=(_DITHER, _DITHER_CELL),
)
_GRADIENT: Final = InterfacePanel(key="gradient", name="Gradient Map", description="Luminance mapped through a color ramp", toggle=Some(False))

# --- [MODELS] ---------------------------------------------------------------------------


class Palette(Effect, frozen=True, kw_only=True):
    """Rounds display colors to evenly spaced levels, then maps luminance through a color ramp."""

    stage = Stage.PALETTE
    pointwise = False
    domain = Domain.DISPLAY
    gradient: Annotated[Ramp, msgspec.Meta(description="Scene colors along luminance 0 to 1 of the gradient map")] = Ramp()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _palette(shell: Shell[Palette]) -> Outcome[Expr]:
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

    chosen = shell.menu(_CHANNELS, cases={_RGB: source, _LUMINANCE: toned(source)})

    def quantized() -> Expr:
        cell = switched(shell.input(_DITHER_CELL), (1.0, 1.0, 0.0), vector("MAXIMUM", (1.0, 1.0, 0.0), pixels(shell.space, shell.input(_DITHER_CELL_VALUE), bpy.types.NodeSocketVector)))
        indices = vector("FLOOR", vector("DIVIDE", gridded(source).point, cell))
        x_bits, y_bits = (vector("SNAP", indices[axis], spans) for axis in ("X", "Y"))
        periods = tuple(2.0 * span for span in spans)
        rank = vector("MULTIPLY_ADD", vector("MODULO", x_bits + y_bits, periods), (2.0,) * 3, vector("MODULO", y_bits, periods))
        weights = tuple(4.0 ** (len(spans) - 1 - bit) / (span * ranks) for bit, span in enumerate(spans))
        offset = (vector("DOT_PRODUCT", rank, weights) + (0.5 / ranks - 0.5)) * (shell.input(_DITHER) * shell.input(_DITHER_VALUE))
        steps = shell.input(_LEVELS) - 1.0
        return Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(
                ("Vector", vector("FLOOR", vector("MULTIPLY_ADD", chosen, steps, offset) + (0.5,) * 3)),
                ("From_Min_FLOAT3", (0.0, 0.0, 0.0)),
                ("From_Max_FLOAT3", steps),
                ("To_Min_FLOAT3", (0.0, 0.0, 0.0)),
                ("To_Max_FLOAT3", (1.0, 1.0, 1.0)),
            ),
            members={"data_type": "FLOAT_VECTOR", "clamp": True},
            socket="Vector",
            shape=bpy.types.NodeSocketVector,
        )

    rounded = quantized()

    def finished(kept: Expr) -> Expr:
        retention = shell.input(_RETAIN_HUE) * msgspec.structs.replace(chosen, socket=_RGB.name, shape=bpy.types.NodeSocketFloat)
        posterized = switched(shell.input(_POSTERIZE), source, mixed(retention, rounded, kept))
        ramp = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", toned(posterized)),), content=Some(shell.record.gradient), socket="Color", shape=bpy.types.NodeSocketColor).named(
            "gradient_value"
        )
        return switched(shell.input(_GRADIENT), posterized, ramp)

    forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD
    return (
        paired(hsy(shell.config, forward, source), hsy(shell.config, forward, rounded))
        .bind(lambda pair: hsy(shell.config, PyOpenColorIO.TRANSFORM_DIR_INVERSE, rgba(pair[0]["Red"], pair[1]["Green"], pair[1]["Blue"])))
        .map(finished)
    )


palette: Final = CompositorEffect(graph=_palette, record=Palette, interface=Interface(panels=(_POSTERIZE, _GRADIENT)), mix=Blended(alpha=Alpha.STRAIGHT))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Palette", "palette"]
