"""Stable ACES gamut compression through native compositor math."""

from expression import Ok

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, rgba, where
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Shell
from blender.imaging.effects.grade.gamut import Channel, GamutCompression

# --- [OPERATIONS] -----------------------------------------------------------------------


def compressed(shell: Shell[GamutCompression]) -> BlenderResult[Expr]:
    """Returns compression with bounded powers and defined zero-achromatic case."""
    _, cyan, magenta, yellow, power = GamutCompression.__struct_fields__
    color, exponent = shell.color, shell.input(power)
    channels = (color["Red"], color["Green"], color["Blue"])
    achromatic = math("MAXIMUM", math("MAXIMUM", channels[0], channels[1]), channels[2])
    sign, inverse = math("SIGN", achromatic), 1.0 / exponent

    def channel(key: str, value: Expr) -> Expr:
        bound, floor = (shell.input(key, member) for member in Channel.__struct_fields__)
        headroom = 1.0 - floor
        scale = headroom / (1.0 - (headroom / (bound - floor)) ** exponent) ** inverse
        distance = sign - value / abs(achromatic)
        excess = math("MAXIMUM", distance - floor, 0.0)
        lower = math("MINIMUM", excess, scale)
        pulled = math("MINIMUM", distance, floor) + lower / (1.0 + (lower / math("MAXIMUM", excess, scale)) ** exponent) ** inverse
        return where(floor < distance, achromatic * (1.0 - sign * pulled), where(abs(sign), value, 0.0))

    return Ok(rgba(*map(channel, (cyan, magenta, yellow), channels)))


# --- [COMPOSITION] ----------------------------------------------------------------------

gamut_compression = CompositorEffect(record=GamutCompression, graph=compressed, alpha=Alpha.STRAIGHT)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["compressed", "gamut_compression"]
