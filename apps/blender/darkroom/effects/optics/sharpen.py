"""Laplace sharpening of scene-linear color, attenuated where the later exposure gain lifts luminance steps."""

from typing import Annotated

import bpy
import msgspec

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, NodeGroup, vector
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.effects import CompositorEffect, Shell

from ...chain.stages import Effect, Stage
from ..grade.exposure import LATER, later

# --- [MODELS] ---------------------------------------------------------------------------


class Sharpen(Effect, frozen=True, kw_only=True):
    """Laplace sharpening attenuated by exposed luminance steps."""

    stage = Stage.SHARPEN
    pointwise = False
    sharpness: Annotated[float, msgspec.Meta(ge=0.0, le=10.0, description="Gain of each pixel's difference from the mean of its eight neighbors")] = 0.0


# --- [OPERATIONS] -----------------------------------------------------------------------


def _filtered(image: Expr, kind: str) -> Expr:
    """Filter node of one kernel at full factor."""
    return Expr(kind=bpy.types.CompositorNodeFilter, inputs=(("Image", image), ("Type", kind)), socket="Image", shape="RGBA")


def sharpened(shell: Shell[Sharpen]) -> BlenderResult[Expr]:
    """Adds the Laplace response scaled by sharpness, gated by the Sobel step of luminance under the later exposure gain."""
    (sharpness,) = Sharpen.__struct_fields__
    source = shell.color

    def built(working: NodeGroup) -> Expr:
        steps = _filtered(working.placed({COLOR_INPUT.key: source}, masks.LUMINANCE.key), "Sobel")["Red"]
        gate = math("SUBTRACT", 1.0, shell.input(LATER.key) * steps * 0.25, clamp=True)
        return vector("MULTIPLY_ADD", _filtered(source, "Laplace"), shell.input(sharpness) * gate, source)

    return masks.luminance(shell.config).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

sharpen = CompositorEffect(record=Sharpen, graph=sharpened, parts=later)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Sharpen", "sharpen"]
