"""Step-gated adaptive sharpening in pixel coordinates."""

import bpy
from expression import Some

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, NodeGroup, vector
from blender.bpy.render.color.spaces import COLOR_INPUT
from blender.bpy.render.effects.compositing import masks
from blender.bpy.render.effects.coordinates import gridded
from blender.bpy.render.effects.rows import CompositorEffect, Shell
from blender.bpy.render.effects.tone.exposure import exposure
from blender.imaging.effects.detail.sharpen import Sharpen
from blender.imaging.geometry.stmaps import Extension

# --- [OPERATIONS] -----------------------------------------------------------------------


def sharpened(shell: Shell[Sharpen]) -> BlenderResult[Expr]:
    """Builds exposure-gated four-neighbor Laplacian sharpening.

    Args:
        shell: Associated image, sharpness, working config, and later exposure gain.

    Returns:
        Sharpened color, or working luminance faults.
    """
    (sharpness,) = Sharpen.__struct_fields__
    source = shell.color
    grid = gridded(source)
    axes = (((1.0, 0.0, 0.0), (-1.0, 0.0, 0.0)), ((0.0, 1.0, 0.0), (0.0, -1.0, 0.0)))
    extend = Extension.EXTEND.label

    def built(working: NodeGroup) -> Expr:
        def level(image: Expr) -> Expr:
            return working.placed({COLOR_INPUT.key: image}, masks.LUMINANCE.key)

        central = level(source)

        def axis(directions: tuple[tuple[float, float, float], ...]) -> tuple[Expr, Expr]:
            taps = tuple(
                Expr(
                    kind=bpy.types.CompositorNodeMapUV,
                    inputs=(("Image", source), ("UV", grid.at(direction, grid.inverse)), ("Interpolation", "Nearest"), ("Extension X", extend), ("Extension Y", extend)),
                    socket="Image",
                    shape="RGBA",
                )
                for direction in directions
            )
            return vector("ADD", *taps), math("MAXIMUM", *(abs(central - level(tap)) for tap in taps))

        (across, across_edge), (up, up_edge) = map(axis, axes)
        count = float(sum(map(len, axes)))
        laplacian = vector("MULTIPLY_ADD", source, (-count, -count, -count), vector("ADD", across, up))
        mask = math("SUBTRACT", 1.0, shell.input(exposure.gain.key) * math("MAXIMUM", across_edge, up_edge), clamp=True)
        return vector("MULTIPLY_ADD", laplacian, shell.input(sharpness) * -Sharpen.weight * mask, source)

    return masks.luminance(shell.config).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

sharpen = CompositorEffect(record=Sharpen, graph=sharpened, inputs=(exposure.gain,), animated=Some(exposure.keyed))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["sharpen", "sharpened"]
