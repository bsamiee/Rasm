"""Uniform lamp flicker: scene-linear exposure varying in stops along a one-dimensional noise over the effect clock."""

import bpy
from expression import Ok

from blender.bpy.nodes.graphs import Expr, math
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.textures import Noise, stream
from blender.bpy.render.compositor.coordinates import clocked, origin
from blender.bpy.render.compositor.effects import CompositorEffect, precise, Requests, Shell
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from ..optics.procedural import CLOCK

# --- [MODELS] ---------------------------------------------------------------------------


class Flicker(Effect, frozen=True, kw_only=True):
    """Uniform scene-linear lamp exposure variation in stops."""

    stage = Stage.EXPOSURE
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------


def flickered(shell: Shell[Flicker]) -> Outcome[Expr]:
    """Exposes the entry color by the signed noise wave scaled to the amplitude in stops."""
    phase = clocked(shell.input("clock_speed"), shell.input("clock_hold")) * shell.input("rate") + origin(shell.input("seed"), float(stream(Flicker)))["X"]
    wave = Noise(noise_dimensions="1D", scale=1.0).node(0.0, phase)
    stops = math("MULTIPLY_ADD", wave, 2.0, -1.0) * shell.input("amplitude")
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", stops)), socket="Image", shape="RGBA"))


# --- [COMPOSITION] ----------------------------------------------------------------------

flicker = CompositorEffect(
    record=Flicker,
    graph=flickered,
    interface=Interface(
        inputs=(
            Socket(key="seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting the variation", members={"min_value": 0, "max_value": 2**24}),
            *(
                Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketFloat, description=description, members=members)
                for key, description, members in (
                    ("amplitude", "Largest signed variation in stops", {"min_value": -32.0, "max_value": 32.0, "default_value": 0.05}),
                    ("rate", "Lamp evolution in noise cells per clock frame", {"min_value": 0.0, "default_value": 2.5}),
                )
            ),
        ),
        panels=(CLOCK,),
    ),
    parts=(Requests[Flicker](request=precise),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Flicker", "flicker"]
