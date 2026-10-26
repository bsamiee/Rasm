"""Uniform medium along camera rays: SI depth transmittance, seeded opacity variation, and the light it scatters toward camera."""

import bpy
import colour
from expression import Some

from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, mixed, switched, vector, where
from blender.bpy.nodes.interface import Interface, InterfacePanel, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.coordinates import clocked, pixels
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Conditions, depth_precise, Requests, Shell
from blender.bpy.scene import cameras
from blender.imaging.color.processors import GREY
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from .procedural import bases, Basis, clock, FACTOR, Perlin, textured

# --- [MODELS] ---------------------------------------------------------------------------


class Atmosphere(Effect, frozen=True, kw_only=True):
    """Uniform medium absorbing and scattering light along camera rays, its opacity field basis holding the node settings."""

    stage = Stage.ATMOSPHERE
    pointwise = False
    basis: Basis = Perlin()


# --- [OPERATIONS] -----------------------------------------------------------------------


def attenuated(shell: Shell[Atmosphere]) -> Outcome[Expr]:
    """Mixes inscattered light over the stage input by medium coverage along each camera ray, coverage joining alpha."""
    inlet, source, frame = shell.input, shell.color, shell.space

    def scattered(parts: tuple[Expr, Expr]) -> Expr:
        cosine, light = parts
        distance = math("MULTIPLY", shell.layer("Depth"), inlet(Shell.frame, "meters")) / cosine
        transmittance = math("EXPONENT", math("MINIMUM", inlet("start") - distance, 0.0) / inlet("attenuation_length"))
        kept = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", where(inlet("cutoff") * (distance > inlet("cutoff_distance")), 1.0, transmittance)),), socket="Image")
        ((field, _),) = textured(
            shell, "noise_basis", shell.record.basis, frame.point, clocked(inlet("noise_clock_speed"), inlet("noise_clock_hold")), inlet("noise_seed"), streams=(textures.stream(Atmosphere, "basis"),)
        )
        coverage = (1.0 - kept) * (1.0 - switched(inlet("noise"), 0.0, field * inlet("noise_variation")))
        alpha = math("MULTIPLY_ADD", source["Alpha"], (1.0 - coverage).named("transmitted"), coverage)
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", mixed(coverage, source, light)), ("Alpha", alpha), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    blurred = Expr(
        kind=bpy.types.CompositorNodeBlur,
        inputs=(("Image", source), ("Size", pixels(frame, inlet("inscatter", "diffused", "radius"), "VECTOR")), ("Type", "Fast Gaussian")),
        socket="Image",
        shape="RGBA",
    )
    lit = shell.menu("inscatter", "kind", cases={"Ambient": inlet("inscatter", "ambient", "color"), "Diffused": vector("MULTIPLY", blurred, inlet("inscatter", "diffused", "tint"))}, kind="RGBA")
    return lenses.chief_ray(source).bind(lambda cosine: lit.map(lambda light: scattered((cosine, light))))


# --- [COMPOSITION] ----------------------------------------------------------------------

atmosphere = CompositorEffect(
    record=Atmosphere,
    graph=attenuated,
    interface=Interface(
        inputs=(
            Socket(
                key="attenuation_length",
                name="Attenuation Length",
                kind=bpy.types.NodeSocketFloat,
                description="Medium depth passing 1/e of light",
                members={"subtype": "DISTANCE", "min_value": 0.0, "default_value": 30.48},
            ),
            Socket(key="start", name="Start", kind=bpy.types.NodeSocketFloat, description="Camera distance where medium starts", members={"subtype": "DISTANCE", "min_value": 0.0}),
        ),
        panels=(
            InterfacePanel(
                key="inscatter",
                name="Inscattering",
                description="Scattered light source",
                inputs=(
                    Socket(
                        key="inscatter_kind",
                        name="Source",
                        kind=bpy.types.NodeSocketMenu,
                        description="Scattered light source",
                        members={"default_value": "Ambient"},
                        items=Some(
                            MenuItems(
                                items=(
                                    MenuItem(name="Ambient", description="Uniformly lit medium scattering one color toward camera"),
                                    MenuItem(name="Diffused", description="Frame light spread through medium and tinted"),
                                )
                            )
                        ),
                    ),
                ),
                panels=(
                    InterfacePanel(
                        key="inscatter_ambient",
                        name="Ambient",
                        description="Uniformly lit medium scattering one color toward camera",
                        inputs=(
                            Socket(
                                key="inscatter_ambient_color",
                                name="Color",
                                kind=bpy.types.NodeSocketColor,
                                description="Scene-linear scattered light",
                                members={"default_value": (*(float(colour.cctf_decoding(217 / 255, function="sRGB")),) * 3, 1.0)},
                            ),
                        ),
                    ),
                    InterfacePanel(
                        key="inscatter_diffused",
                        name="Diffused",
                        description="Frame light spread through medium and tinted",
                        inputs=(
                            Socket(
                                key="inscatter_diffused_tint",
                                name="Tint",
                                kind=bpy.types.NodeSocketColor,
                                description="Scene-linear spread light factor",
                                members={"default_value": (GREY, GREY, GREY, 1.0)},
                            ),
                            Socket(
                                key="inscatter_diffused_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Gaussian spread size", members={"min_value": 0.0, "default_value": 0.75}
                            ),
                        ),
                    ),
                ),
            ),
            InterfacePanel(
                key="noise",
                name="Noise",
                description="Seeded opacity reduction",
                toggle=Some(False),
                inputs=(
                    Socket(key="noise_variation", name="Variation", kind=bpy.types.NodeSocketFloat, description="Largest opacity share field removes", members={**FACTOR, "default_value": 1.0}),
                    Socket(key="noise_seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting pattern", members={"min_value": 0, "max_value": 2**24}),
                ),
                panels=(bases("noise_basis", Atmosphere().basis, "Procedural opacity field"), clock("noise_clock", speed=0.0)),
            ),
            InterfacePanel(
                key="cutoff",
                name="Cutoff",
                description="Ray distance past which samples keep light",
                toggle=Some(False),
                inputs=(
                    Socket(
                        key="cutoff_distance",
                        name="Distance",
                        kind=bpy.types.NodeSocketFloat,
                        description="Ray distance past which samples keep light",
                        members={"subtype": "DISTANCE", "min_value": 0.0, "default_value": 914.4},
                    ),
                ),
            ),
        ),
    ),
    alpha=Alpha.RESULT,
    parts=(Requests[Atmosphere](request=depth_precise), Conditions[Atmosphere](condition=lambda _record: cameras.camera_type("PERSP", "ORTHO"))),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Atmosphere", "atmosphere"]
