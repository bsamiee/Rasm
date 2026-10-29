"""Laplace sharpening of scene-linear color, attenuated where the later exposure gain lifts luminance steps."""

from typing import Final

import bpy
from expression import Some

from blender.bpy.nodes.desired import Interface, Socket
from blender.bpy.nodes.graphs import Expr, math, vector
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.effects import CompositorEffect
from blender.bpy.render.compositor.inputs import Shell
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from ..grade.exposure import LATER, later

# --- [CONSTANTS] ------------------------------------------------------------------------

_SHARPNESS: Final = Socket(key="sharpness", kind=bpy.types.NodeSocketFloat, description="Gain of each pixel's difference from the mean of its eight neighbors", min_value=0.0, max_value=10.0)

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
        return Expr(kind=bpy.types.CompositorNodeFilter, inputs=(("Image", image), ("Type", kind)), socket="Image", shape=bpy.types.NodeSocketColor)

    def built(level: Expr) -> Expr:
        steps = filtered(level, "Sobel")["Red"]
        gate = math("SUBTRACT", 1.0, shell.input(LATER) * steps * 0.25, clamp=True)
        return vector("MULTIPLY_ADD", filtered(source, "Laplace"), shell.input(_SHARPNESS) * gate, source)

    return masks.luminance(shell.config, source).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

sharpen: Final = CompositorEffect(sharpened, record=Sharpen, interface=Interface(inputs=(_SHARPNESS, LATER)), animates=Some(later))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Sharpen", "sharpen"]
