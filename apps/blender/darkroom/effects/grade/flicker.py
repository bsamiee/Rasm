"""Uniform lamp flicker: scene-linear exposure varying in stops along a one-dimensional noise over the effect clock."""

from typing import Final

import bpy
from expression import Ok, Some

from blender.bpy.nodes.graphs import Expr, math
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.textures import Noise
from blender.bpy.render.compositor.effects import CompositorEffect, precise
from blender.bpy.render.compositor.motion import clocked, origin
from blender.bpy.render.compositor.shells import Shell
from blender.imaging.sampling.tiles import stream
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from ..optics.procedural import CLOCK

# --- [CONSTANTS] ------------------------------------------------------------------------

_SEED: Final = Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting the variation", min_value=0, max_value=2**24)
_AMPLITUDE: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloat, description="Largest signed variation in stops", min_value=-32.0, max_value=32.0, default_value=0.05)
_RATE: Final = Socket(key="rate", kind=bpy.types.NodeSocketFloat, description="Lamp evolution in noise cells per clock unit", min_value=0.0, default_value=2.5)

# --- [MODELS] ---------------------------------------------------------------------------


class Flicker(Effect, frozen=True, kw_only=True):
    """Uniform scene-linear lamp exposure variation in stops."""

    stage = Stage.EXPOSURE
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------


def _flicker(shell: Shell[Flicker]) -> Outcome[Expr]:
    """Exposes the entry color by the signed noise wave scaled to the amplitude in stops."""
    speed, hold = (shell.input(socket) for socket in CLOCK.inputs)
    phase = clocked(speed, hold) * shell.input(_RATE) + origin(shell.input(_SEED), float(stream(Flicker)))["X"]
    wave = Noise(noise_dimensions="1D", scale=1.0).node(0.0, phase)
    stops = math("MULTIPLY_ADD", wave, 2.0, -1.0) * shell.input(_AMPLITUDE)
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", stops)), socket="Image", shape=bpy.types.NodeSocketColor))


flicker: Final = CompositorEffect(graph=_flicker, record=Flicker, interface=Interface(inputs=(_SEED, _AMPLITUDE, _RATE), panels=(CLOCK,)), request=Some(precise))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Flicker", "flicker"]
