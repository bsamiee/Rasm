"""Procedural light and haze: a textured field drifting, twirling, and evolving over the clock, emitting, absorbing, and scattering light."""

from math import exp, pi
from typing import Annotated, Final

import bpy
from expression import Some
import msgspec

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.render.compositor.coordinates import Clock, clocked
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, precise, Requests, Shell
from blender.imaging.geometry.regions import FrameExtent, Position
from blender.records.fields import Finite, Gated, NonNegative, Presentation, Quantity, Rgb, Seed

from ...chain.stages import Effect, Stage
from .procedural import Basis, Emission, emitted, Incandescence, Perlin, textured

# --- [CONSTANTS] ------------------------------------------------------------------------

_FIRE_SHORT_SIDE: Final = 9.0 / 16.0

# --- [MODELS] ---------------------------------------------------------------------------


class Extinction(msgspec.Struct, frozen=True, kw_only=True):
    """Light the field absorbs and scatters toward camera."""

    density: Annotated[NonNegative, msgspec.Meta(description="Optical depth at full density"), Presentation(soft=Some((0.0, 8.0)))] = 2.0
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear light scattered toward camera")] = Rgb(red=0.04, green=0.04, blue=0.04)


class Vortex(msgspec.Struct, frozen=True, kw_only=True):
    """Polar twirl of the field about its center."""

    radius: Annotated[FrameExtent, msgspec.Meta(description="Twirl and widening reach over greater frame side"), Presentation(soft=Some((0.01, 1.0)), precision=Some(3))] = 0.25
    twirl: Annotated[Finite, msgspec.Meta(description="Turn at center, falling to 0 at radius"), Presentation(soft=Some((-4.0 * pi, 4.0 * pi)), quantity=Some(Quantity.ANGLE))] = 3.0
    arms: Annotated[int, msgspec.Meta(ge=0, description="Field repeats around center, 0 leaving rings"), Presentation(soft=Some((0.0, 32.0)))] = 1
    spin: Annotated[Finite, msgspec.Meta(description="Vortex turn in radians per clock frame"), Presentation(soft=Some((-pi / 12.0, pi / 12.0)), precision=Some(4))] = 1.0 / 24.0
    tunnel: Annotated[NonNegative, msgspec.Meta(description="Axial sample height per unit radius"), Presentation(soft=Some((0.0, 4.0)))] = 1.0


class Envelope(msgspec.Struct, frozen=True, kw_only=True):
    """Elliptical falloff subtracted from the texture before its cut."""

    width: Annotated[FrameExtent, msgspec.Meta(description="Horizontal radius over greater frame side"), Presentation(soft=Some((0.01, 1.0)))] = _FIRE_SHORT_SIDE
    height: Annotated[FrameExtent, msgspec.Meta(description="Vertical radius over greater frame side"), Presentation(soft=Some((0.01, 1.0)))] = _FIRE_SHORT_SIDE


class NoiseField(Effect, frozen=True, kw_only=True):
    """Procedural light and haze added to frame: fire, smoke, mist, light leaks, and flow."""

    stage = Stage.GENERATE
    pointwise = False
    basis: Annotated[Basis, msgspec.Meta(description="Procedural texture")] = Perlin(
        texture=textures.Noise(noise_dimensions="4D", scale=1.4 / _FIRE_SHORT_SIDE, detail=4.0, distortion=1.0), stretch=2.0, cut=textures.MapRange(from_min=0.17)
    )
    emission: Annotated[Gated[Emission], msgspec.Meta(description="Light the field adds")] = Gated(enabled=True, value=Emission(spectrum=Incandescence(cool=800.0), intensity=0.0))
    extinction: Annotated[Gated[Extinction], msgspec.Meta(description="Light the field absorbs and scatters")] = Gated(enabled=False, value=Extinction())
    clock: Annotated[Clock, msgspec.Meta(description="Clock the field drifts and changes over")] = Clock()
    drift: Annotated[
        tuple[Finite, Finite], msgspec.Meta(description="Velocity along field axes in greater frame sides per clock frame"), Presentation(precision=Some(4), quantity=Some(Quantity.FRAME))
    ] = (0.0, 5.0 * _FIRE_SHORT_SIDE / 24.0)
    evolution: Annotated[Finite, msgspec.Meta(description="Signed feature evolution per clock frame"), Presentation(soft=Some((-0.2, 0.2)), precision=Some(4))] = 0.0
    vortex: Annotated[Gated[Vortex], msgspec.Meta(description="Polar twirl of field about center")] = Gated(enabled=False, value=Vortex())
    envelope: Annotated[Gated[Envelope], msgspec.Meta(description="Falloff before the texture cut")] = Gated(enabled=True, value=Envelope())
    center: Annotated[Position, msgspec.Meta(description="Axis pivot in frame fractions")] = (0.5, 0.0)
    angle: Annotated[Finite, msgspec.Meta(description="Turn of field axes"), Presentation(soft=Some((-pi, pi)), quantity=Some(Quantity.ANGLE))] = 0.0
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0


# --- [OPERATIONS] -----------------------------------------------------------------------


def noised(shell: Shell[NoiseField]) -> BlenderResult[Expr]:
    """Emits and extinguishes light over the field's density, coverage as alpha."""
    basis, emission, extinction, clock_field, drift, evolution, vortex, envelope, center, angle, seed = NoiseField.__struct_fields__
    _, value = Gated.__struct_fields__
    speed, hold = Clock.__struct_fields__
    radius_field, twirl, arms, spin, tunnel = Vortex.__struct_fields__
    width, height = Envelope.__struct_fields__
    depth, color = Extinction.__struct_fields__
    inlet, at = shell.input, shell.space
    clock = clocked(inlet(clock_field, speed), inlet(clock_field, hold)).named("clock")
    axis = Expr(
        kind=bpy.types.ShaderNodeVectorRotate,
        inputs=(("Vector", at.point - inlet(center) * at.extent), ("Angle", inlet(angle))),
        members=(("rotation_type", "Z_AXIS"), ("invert", True)),
        socket="Vector",
        shape="VECTOR",
    )
    radius = inlet(vortex, value, radius_field)
    rho = vector("LENGTH", axis) / radius
    phase = math("MULTIPLY_ADD", math("ARCTAN2", axis["Y"], axis["X"]), inlet(vortex, value, arms), math("MULTIPLY_ADD", 1.0 - rho, inlet(vortex, value, twirl), clock * inlet(vortex, value, spin)))
    cone = vector("SCALE", xyz(math("COSINE", phase), math("SINE", phase), inlet(vortex, value, tunnel)), math("MINIMUM", rho, 1.0) * radius)
    reach = math("MAXIMUM", 1.0 - vector("LENGTH", axis / xyz(inlet(envelope, value, width), inlet(envelope, value, height))), 0.0)
    ((density, tone),) = textured(
        shell,
        (basis,),
        switched(shell.gate(vortex), axis, cone) - vector("SCALE", inlet(drift), clock),
        clock * inlet(evolution),
        inlet(seed),
        loss=switched(shell.gate(envelope), 0.0, 1.0 - reach * reach),
        streams=(textures.stream(NoiseField, basis),),
    )
    coverage = 1.0 - switched(shell.gate(extinction), 1.0, math("POWER", exp(-1.0), density * inlet(extinction, value, depth)))

    def generated(lights: tuple[Expr, ...]) -> Expr:
        light = switched(shell.gate(emission), (0.0, 0.0, 0.0), vector("SCALE", lights[0], density)) + vector("SCALE", inlet(extinction, value, color), coverage)
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", light), ("Alpha", coverage), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    return emitted(shell, (emission, value), (tone,)).map(generated)


# --- [COMPOSITION] ----------------------------------------------------------------------

noise_field = CompositorEffect(record=NoiseField, graph=noised, alpha=Alpha.RESULT, parts=(Requests[NoiseField](request=precise),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Envelope", "Extinction", "NoiseField", "Vortex", "noise_field"]
