"""Uniform lamp flicker: scene-linear exposure varying in stops along a one-dimensional noise over the effect clock."""

from typing import Annotated

import bpy
from expression import Ok, Some
import msgspec

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math
from blender.bpy.nodes.textures import Noise, stream
from blender.bpy.render.compositor.coordinates import Clock, clocked, origin
from blender.bpy.render.compositor.effects import CompositorEffect, precise, Requests, Shell
from blender.imaging.color.processors import Stops
from blender.records.fields import NonNegative, Presentation, Seed

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Flicker(Effect, frozen=True, kw_only=True):
    """Uniform scene-linear lamp exposure variation in stops."""

    stage = Stage.EXPOSURE
    pointwise = True
    clock: Clock = Clock()
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting the variation")] = 0
    amplitude: Annotated[Stops, msgspec.Meta(description="Largest signed variation in stops")] = 0.05
    rate: Annotated[NonNegative, msgspec.Meta(description="Lamp evolution in noise cells per clock frame"), Presentation(soft=Some((0.0, 10.0)))] = 2.5


# --- [OPERATIONS] -----------------------------------------------------------------------


def flickered(shell: Shell[Flicker]) -> BlenderResult[Expr]:
    """Exposes the entry color by the signed noise wave scaled to the amplitude in stops."""
    clock, seed, amplitude, rate = Flicker.__struct_fields__
    speed, hold = Clock.__struct_fields__
    phase = clocked(shell.input(clock, speed), shell.input(clock, hold)) * shell.input(rate) + origin(shell.input(seed), float(stream(Flicker)))["X"]
    wave = Noise(noise_dimensions="1D", scale=1.0).node(0.0, phase)
    stops = math("MULTIPLY_ADD", wave, 2.0, -1.0) * shell.input(amplitude)
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", stops)), socket="Image", shape="RGBA"))


# --- [COMPOSITION] ----------------------------------------------------------------------

flicker = CompositorEffect(record=Flicker, graph=flickered, parts=(Requests[Flicker](request=precise),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Flicker", "flicker"]
