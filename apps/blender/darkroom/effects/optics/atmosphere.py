"""Uniform medium along camera rays: SI depth transmittance, seeded opacity variation, and the light it scatters toward camera."""

from typing import Annotated

import bpy
import colour
from expression import Some
import msgspec

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import End, Expr, math, mixed, switched, vector, where
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.coordinates import Clock, clocked, pixels
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, Conditions, depth_precise, Inspects, Requests, Shell
from blender.bpy.scene import cameras
from blender.imaging.color.processors import GREY
from blender.imaging.geometry.regions import FrameLength
from blender.records.fields import Case, Gated, Presentation, Quantity, Rgb, Seed, Unit

from ...chain.stages import Effect, Stage
from .procedural import Basis, Perlin, textured

# --- [TYPES] ----------------------------------------------------------------------------

type Inscattering = Ambient | Diffused

# --- [MODELS] ---------------------------------------------------------------------------


class Ambient(Case, frozen=True, kw_only=True):
    """Uniformly lit medium scattering one color toward camera."""

    color: Annotated[Rgb, msgspec.Meta(description="Scene-linear scattered light")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, float(colour.cctf_decoding(217 / 255, function="sRGB"))))


class Diffused(Case, frozen=True, kw_only=True):
    """Frame light spread through medium and tinted."""

    tint: Annotated[Rgb, msgspec.Meta(description="Scene-linear spread light factor")] = Rgb(**dict.fromkeys(Rgb.__struct_fields__, GREY))
    radius: Annotated[FrameLength, msgspec.Meta(description="Gaussian spread size")] = 0.75


class Fluctuation(msgspec.Struct, frozen=True, kw_only=True):
    """Seeded medium opacity variation."""

    variation: Annotated[Unit, msgspec.Meta(description="Largest opacity share field removes")] = 1.0
    basis: Annotated[Basis, msgspec.Meta(description="Procedural opacity field")] = Perlin()
    clock: Annotated[Clock, msgspec.Meta(description="Field clock in feature lengths")] = Clock(speed=0.0)
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting pattern")] = 0


class Atmosphere(Effect, frozen=True, kw_only=True):
    """Uniform medium absorbing and scattering light along camera rays."""

    stage = Stage.ATMOSPHERE
    pointwise = False
    inscatter: Annotated[Inscattering, msgspec.Meta(title="Inscattering", description="Scattered light source")] = Ambient()
    noise: Annotated[Gated[Fluctuation], msgspec.Meta(description="Seeded opacity reduction")] = Gated(enabled=False, value=Fluctuation())
    attenuation: Annotated[float, msgspec.Meta(gt=0.0, title="Attenuation Length", description="Medium depth passing 1/e of light"), Presentation(quantity=Some(Quantity.LENGTH))] = 30.48
    start: Annotated[float, msgspec.Meta(ge=0.0, description="Camera distance where medium starts"), Presentation(quantity=Some(Quantity.LENGTH))] = 0.0
    cutoff: Annotated[Gated[Annotated[float, msgspec.Meta(ge=0.0)]], msgspec.Meta(description="Ray distance past which samples keep light"), Presentation(quantity=Some(Quantity.LENGTH))] = Gated(
        enabled=False, value=914.4
    )


# --- [OPERATIONS] -----------------------------------------------------------------------


def attenuated(shell: Shell[Atmosphere]) -> BlenderResult[Expr]:
    """Mixes inscattered light over the stage input by medium coverage along each camera ray, coverage joining alpha."""
    inscatter, noise, attenuation, start, cutoff = Atmosphere.__struct_fields__
    (color,) = Ambient.__struct_fields__
    tint, radius = Diffused.__struct_fields__
    variation, basis, clock, seed = Fluctuation.__struct_fields__
    speed, hold = Clock.__struct_fields__
    _, _, meters = CameraFrame.__struct_fields__
    _, value = Gated.__struct_fields__
    inlet, source, frame = shell.input, shell.color, shell.space

    def scattered(cosine: Expr) -> Expr:
        distance = math("MULTIPLY", shell.layer("Depth"), inlet(Shell.frame, meters)) / cosine
        transmittance = math("EXPONENT", math("MINIMUM", inlet(start) - distance, 0.0) / inlet(attenuation))
        kept = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", where(shell.gate(cutoff) * (distance > inlet(cutoff)), 1.0, transmittance)),), socket="Image")
        blurred = Expr(
            kind=bpy.types.CompositorNodeBlur,
            inputs=(("Image", source), ("Size", pixels(frame, inlet(inscatter, Diffused, radius), "VECTOR")), ("Type", "Fast Gaussian")),
            socket="Image",
            shape="RGBA",
        )
        light = shell.menu(inscatter, cases={Ambient.title: inlet(inscatter, Ambient, color), Diffused.title: vector("MULTIPLY", blurred, inlet(inscatter, Diffused, tint))}, kind="RGBA")
        ((field, _),) = textured(
            shell,
            (noise, value, basis),
            frame.point,
            clocked(inlet(noise, value, clock, speed), inlet(noise, value, clock, hold)),
            inlet(noise, value, seed),
            streams=(textures.stream(Fluctuation, basis),),
        )
        coverage = (1.0 - kept) * (1.0 - switched(shell.gate(noise), 0.0, field * inlet(noise, value, variation)))
        alpha = math("MULTIPLY_ADD", source["Alpha"], (1.0 - coverage).named("transmitted"), coverage)
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", mixed(coverage, source, light)), ("Alpha", alpha), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    return lenses.chief_ray(source).map(scattered)


# --- [COMPOSITION] ----------------------------------------------------------------------

atmosphere = CompositorEffect(
    record=Atmosphere,
    graph=attenuated,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Atmosphere](request=depth_precise),
        Conditions[Atmosphere](condition=lambda _record: cameras.camera_type("PERSP", "ORTHO")),
        Inspects(key="transmittance_view", end=End(node="transmitted"), description="Light share transmitted through medium"),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ambient", "Atmosphere", "Diffused", "Fluctuation", "Inscattering", "atmosphere"]
