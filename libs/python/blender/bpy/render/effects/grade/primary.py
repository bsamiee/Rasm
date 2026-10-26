"""Gain and offset wheels, signed contrast, and working-space vibrance."""

from expression import Ok

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Shell
from blender.imaging.chain import GREY
from blender.imaging.color.processors import luma
from blender.imaging.effects.grade.primary import Contrast, Primary, Saturation, Wheel

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WHEELS]


def graded(shell: Shell[Primary]) -> BlenderResult[Expr]:
    """Returns additive offset of color less white plus master, then gain of color times two to the master."""
    color, master = Wheel.__struct_fields__
    gain, offset = Primary.__struct_fields__
    shifted = vector("ADD", vector("SUBTRACT", shell.input(offset, color), (1.0, 1.0, 1.0)), shell.input(offset, master))
    return Ok(vector("MULTIPLY", vector("ADD", shell.color, shifted), vector("SCALE", shell.input(gain, color), 2.0 ** shell.input(gain, master))))


# --- [CONTRAST]


def contrasted(shell: Shell[Contrast]) -> BlenderResult[Expr]:
    """Returns signed power contrast around grey pivot."""
    slope, pivot = (shell.input(field) for field in Contrast.__struct_fields__)
    center, color = GREY * 2.0**pivot, shell.color

    def signed(value: Expr) -> Expr:
        return (abs(value) / center) ** slope * center * math("SIGN", value)

    return Ok(xyz(signed(color["X"]), signed(color["Y"]), signed(color["Z"])))


# --- [SATURATION]


def saturated(shell: Shell[Saturation]) -> BlenderResult[Expr]:
    """Returns saturation and multiplicative vibrance about configured luminance.

    Args:
        shell: Entry inputs and working configuration supplying luminance coefficients.

    Returns:
        Saturated color, or luminance coefficient faults.
    """
    scale, vibrance = (shell.input(field) for field in Saturation.__struct_fields__)
    color = shell.color
    red, green, blue = color["X"], color["Y"], color["Z"]
    sigma = 1.0 - math("MAXIMUM", math("MINIMUM", math("MINIMUM", red, green), blue), 0.0) / math("MAXIMUM", math("MAXIMUM", red, green), blue)
    inverse = 1.0 - sigma
    gain = scale * math("MULTIPLY_ADD", vibrance, switched(vibrance > 0.0, sigma * sigma * 0.5, inverse * inverse), 1.0)

    def built(weights: tuple[float, ...]) -> Expr:
        luminance = vector("DOT_PRODUCT", color, weights)
        return vector("MULTIPLY_ADD", vector("SUBTRACT", color, luminance), gain, luminance)

    return luma(shell.config).map(lambda values: built(tuple(map(float, values))))


# --- [COMPOSITION] ----------------------------------------------------------------------

primary = CompositorEffect(record=Primary, graph=graded, alpha=Alpha.STRAIGHT)
contrast = CompositorEffect(record=Contrast, graph=contrasted, alpha=Alpha.STRAIGHT)
saturation = CompositorEffect(record=Saturation, graph=saturated, alpha=Alpha.STRAIGHT)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["contrast", "contrasted", "graded", "primary", "saturated", "saturation"]
