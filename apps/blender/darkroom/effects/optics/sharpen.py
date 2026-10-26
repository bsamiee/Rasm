"""Laplace sharpening of scene-linear color, attenuated where the later exposure gain lifts luminance steps."""

import bpy

from blender.bpy.nodes.graphs import Expr, math, NodeGroup, vector
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.effects import Animates, CompositorEffect, Shell
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from ..grade.exposure import LATER, later

# --- [MODELS] ---------------------------------------------------------------------------


class Sharpen(Effect, frozen=True, kw_only=True):
    """Laplace sharpening attenuated by exposed luminance steps."""

    stage = Stage.SHARPEN
    pointwise = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def sharpened(shell: Shell[Sharpen]) -> Outcome[Expr]:
    """Adds the Laplace response scaled by sharpness, gated by the Sobel step of luminance under the later exposure gain."""
    source = shell.color

    def filtered(image: Expr, kind: str) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeFilter, inputs=(("Image", image), ("Type", kind)), socket="Image", shape="RGBA")

    def built(working: NodeGroup) -> Expr:
        steps = filtered(working.placed({COLOR_INPUT.key: source}, masks.LUMINANCE.key), "Sobel")["Red"]
        gate = math("SUBTRACT", 1.0, shell.input(LATER.key) * steps * 0.25, clamp=True)
        return vector("MULTIPLY_ADD", filtered(source, "Laplace"), shell.input("sharpness") * gate, source)

    return masks.luminance(shell.config).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

sharpen = CompositorEffect(
    record=Sharpen,
    graph=sharpened,
    interface=Interface(
        inputs=(
            Socket(
                key="sharpness",
                name="Sharpness",
                kind=bpy.types.NodeSocketFloat,
                description="Gain of each pixel's difference from the mean of its eight neighbors",
                members={"min_value": 0.0, "max_value": 10.0},
            ),
            LATER,
        )
    ),
    parts=(Animates[Sharpen](keys=later),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Sharpen", "sharpen"]
