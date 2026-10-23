"""Step-gated adaptive sharpening in pixel coordinates."""

from itertools import starmap

import bpy
from expression import Some

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import gridded, PixelGrid
from blender.bpy.nodes.graphs import End, Fragment, math, nested, node, NodeGroup, Operand, vector
from blender.bpy.render.color.spaces import COLOR_INPUT
from blender.bpy.render.effects.compositing.masks import luminance
from blender.bpy.render.effects.stack import CompositorEffect, Shell
from blender.bpy.render.effects.tone.exposure import exposure
from blender.imaging.effects.detail.sharpen import Sharpen
from blender.imaging.faults import accumulated, paired

# --- [OPERATIONS] -----------------------------------------------------------------------


def sharpened(shell: Shell[Sharpen]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds exposure-gated four-neighbor Laplacian sharpening.

    Args:
        shell: Associated image, sharpness, working config, and later exposure gain.

    Returns:
        Sharpened color with its graph, or accumulated node faults.
    """
    axes = {"horizontal": {"right": (1.0, 0.0, 0.0), "left": (-1.0, 0.0, 0.0)}, "vertical": {"up": (0.0, 1.0, 0.0), "down": (0.0, -1.0, 0.0)}}
    fetching = {"Interpolation": "Nearest", "Extension X": "Extend", "Extension Y": "Extend"}
    taps = float(sum(map(len, axes.values())))

    def joined(suffix: str, parts: tuple[tuple[Operand, Operand], ...]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        return paired(vector(f"sum{suffix}", "ADD", *(color for color, _ in parts)), math(f"edge{suffix}", "MAXIMUM", *(size for _, size in parts)))

    def kernel(working: NodeGroup, grid: PixelGrid, central: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        def tap(side: Key, direction: tuple[float, float, float]) -> BlenderResult[tuple[End, tuple[Fragment, End]]]:
            fetched = End(node=f"fetch_{side}", socket="Image")
            return (
                grid
                .at(f"uv_{side}", direction, grid.inverse)
                .bind(lambda uv: node(f"fetch_{side}", bpy.types.CompositorNodeMapUV, {"Image": shell.source, "UV": uv, **fetching}, {}))
                .bind(lambda fetch: nested(f"level_{side}", working, {COLOR_INPUT.key: (fetch, fetched)}, "luminance"))
                .bind(lambda neighbor: math(f"step_{side}", "SUBTRACT", central, neighbor))
                .bind(lambda step: math(f"size_{side}", "ABSOLUTE", step))
                .map(lambda size: (fetched, size))
            )

        return accumulated(accumulated(starmap(tap, sides.items())).bind(lambda found: joined(f"_{axis}", found)) for axis, sides in axes.items()).bind(lambda found: joined("", found))

    def applied(total: Operand, edge: Operand) -> BlenderResult[tuple[Fragment, End]]:
        (sharpness,) = Sharpen.__struct_fields__
        return paired(
            vector("laplacian", "MULTIPLY_ADD", shell.source, (-taps, -taps, -taps), total),
            paired(
                math("strength", "MULTIPLY", shell.socket(sharpness), -Sharpen.weight),
                math("exposed", "MULTIPLY", shell.socket(exposure.gain.key), edge).bind(lambda exposed: math("mask", "SUBTRACT", 1.0, exposed, clamp=True)),
            ).bind(lambda factors: math("amount", "MULTIPLY", *factors)),
        ).bind(lambda parts: vector("sharpened", "MULTIPLY_ADD", *parts, shell.source))

    return (
        luminance(shell.config)
        .bind(lambda working: paired(gridded("grid", shell.source), nested("central", working, {COLOR_INPUT.key: shell.source}, "luminance")).bind(lambda parts: kernel(working, *parts)))
        .bind(lambda parts: applied(*parts))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

sharpen = CompositorEffect(record=Sharpen, graph=sharpened, inputs=(exposure.gain,), animated=Some(exposure.keyed))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["sharpen", "sharpened"]
