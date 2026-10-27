"""Light shafts and lens flares generated about a frame position or a bound scene object seen through the camera."""

from enum import auto, Enum
from math import tau
from typing import Annotated, Final

import bpy
from expression import Ok, Option, Some
import msgspec

from blender.bpy.data.rna.conditions import Present
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.ramps import Ramp, Stop
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.coordinates import clocked, origin, sampled, Sighted
from blender.bpy.render.compositor.effects import Checks, CompositorEffect, Context, Requests, Shell
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.compositor.shapes import coverage, rotated
from blender.bpy.render.view_layers.passes import PassRequest
from blender.bpy.scene.cameras import camera_type
from blender.imaging.geometry.lenses import Aperture
from blender.records.faults import accumulated, Outcome, paired

from ...chain.stages import Effect, Stage
from .procedural import ANGLE, bases, Basis, clock, Emission, emission, emitted, FACTOR, Gradient, Perlin, textured, Tint

# --- [TYPES] ----------------------------------------------------------------------------


class Grading(Enum):
    """Ghost color sampled per aperture image or continuously along axis."""

    INSTANCE = auto()
    AXIS = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_SOURCE: Final = InterfacePanel(
    key="source",
    name="Source",
    description="Frame position or bound scene object the light comes from",
    inputs=(
        Socket(
            key="source_kind",
            name="Source",
            kind=bpy.types.NodeSocketMenu,
            description="Frame position or bound scene object",
            members={"default_value": "Frame Position"},
            items=Some(
                MenuItems(
                    items=(
                        MenuItem(name="Frame Position", description="Source at frame position, needing no camera or depth"),
                        MenuItem(name="Object", description="Bound scene object read as location or local +Z direction"),
                    )
                )
            ),
        ),
        Socket(key="source_position", name="Position", kind=bpy.types.NodeSocketVector, description="Source in frame fractions", members={"dimensions": 2, "default_value": (0.3, 0.6)}),
        Socket(key="source_object", name="Object", kind=bpy.types.NodeSocketObject, description="Bound scene object"),
        Socket(
            key="source_reading",
            name="Reading",
            kind=bpy.types.NodeSocketMenu,
            description="Location or local +Z toward distant light",
            members={"default_value": "Position"},
            items=Some(MenuItems(items=(MenuItem(name="Position", description="Object location"), MenuItem(name="Direction", description="Direction toward a light at infinity")))),
        ),
        Socket(key="source_occlusion", name="Occlusion", kind=bpy.types.NodeSocketBool, description="Nearer surfaces hide source", members={"default_value": True}),
    ),
)

# --- [MODELS] ---------------------------------------------------------------------------


class LightRays(Effect, frozen=True, kw_only=True):
    """Highlight shafts marched toward a source through gaps in nearer surfaces."""

    stage = Stage.GENERATE
    pointwise = False


class LensFlare(Effect, frozen=True, kw_only=True):
    """Source disc, radial streaks, anamorphic bar, halo, and ghosts generated as light, each section's emission and the streak basis holding their cases, the ghosts their iris, grading, and count."""

    stage = Stage.GENERATE
    pointwise = False
    hotspot: Emission = Emission(spectrum=Tint(start=(1.0, 0.9, 0.8), end=(1.0, 0.9, 0.8)), intensity=3.0)
    starburst: Emission = Emission(spectrum=Tint(start=(1.0, 0.9, 0.8), end=(1.0, 0.9, 0.8)), intensity=0.0)
    basis: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=1.0, roughness=0.563))
    anamorphic: Emission = Emission(spectrum=Tint(start=(0.04, 0.34, 1.0), end=(0.04, 0.34, 1.0)), intensity=0.0)
    halo: Emission = Emission(spectrum=Tint(start=(0.124, 0.009, 0.194), end=(0.124, 0.009, 0.194)), intensity=0.0)
    ghosts: Emission = Emission(
        spectrum=Gradient(ramp=Ramp(stops=(Stop(position=0.0, color=(0.12, 0.02, 0.2)), Stop(position=0.5, color=(0.02, 0.1, 0.08)), Stop(position=1.0, color=(0.2, 0.08, 0.02))))), intensity=0.0
    )
    iris: Aperture = Aperture()
    grading: Grading = Grading.INSTANCE
    count: Annotated[int, msgspec.Meta(ge=1)] = 5


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SOURCE]


def located[R: LightRays | LensFlare](shell: Shell[R]) -> Outcome[tuple[Expr, Expr, Expr]]:
    """Returns the source's frame fractions, its ahead gate, and its depth-pass occlusion, zero unless a bound object occludes."""

    def placed(direction: Expr, bound: Expr) -> tuple[Expr, Expr, Expr]:
        sighted = Sighted.of(shell.input("source", "object"), direction)
        return (
            switched(bound, shell.input("source", "position"), sighted.position),
            switched(bound, 1.0, sighted.ahead),
            (shell.layer("Depth") < sighted.depth) * shell.input("source", "occlusion") * bound,
        )

    return paired(
        shell.menu("source", "reading", cases={"Position": False, "Direction": True}, kind="BOOLEAN"), shell.menu("source", "kind", cases={"Frame Position": False, "Object": True}, kind="BOOLEAN")
    ).map(lambda pair: placed(*pair))


def ramped(distance: Expr, length: Scalar) -> tuple[Expr, Expr]:
    """Returns tone `min(distance / length, 1)` and squared remaining share of a section reaching `length`."""
    remaining = math("MAXIMUM", math("SUBTRACT", length, distance), 0.0) / length
    return math("MINIMUM", distance / length, 1.0), remaining * remaining


# --- [GRAPHS]


def beamed(shell: Shell[LightRays]) -> Outcome[Expr]:
    """Adds highlight shafts marched toward the source through its depth gaps."""
    key = masks.highlight(shell.color, shell.input("threshold"), shell.input("smoothness"))
    diagonal = vector("LENGTH", Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR"))

    def marched(source: tuple[Expr, Expr, Expr]) -> Expr:
        position, front, occluded = source
        rays = Expr(
            kind=bpy.types.CompositorNodeGlare,
            inputs=(
                ("Image", vector("SCALE", key, 1.0 - occluded)),
                ("Type", "Sun Beams"),
                ("Quality", "High"),
                ("Clamp Highlights", False),
                ("Highlights Threshold", 0.0),
                ("Highlights Smoothness", 0.0),
                ("Sun Position", sampled(shell.space, position)),
                ("Size", shell.input("length") * shell.space.greater / diagonal),
                ("Jitter", shell.input("jitter")),
            ),
            socket="Glare",
            shape="RGBA",
        )
        return mixed(1.0, shell.color, vector("SCALE", vector("MULTIPLY", rays, shell.input("color")), front), blend="ADD")

    return located(shell).map(marched)


def flared(shell: Shell[LensFlare]) -> Outcome[Expr]:
    """Adds the source-centered flare sections to the stage input under finite-disc visibility."""
    inlet, frame, record = shell.input, shell.space, shell.record
    center = frame.extent * 0.5

    def lit(source: tuple[Expr, Expr, Expr]) -> Outcome[Expr]:
        position, front, occluded = source
        axis, offset = position * frame.extent - center, frame.point - position * frame.extent
        rho, pixel, radius = vector("LENGTH", offset), 1.0 / frame.greater, inlet("radius")
        edge, diameter = rho - radius, Some(radius * 2.0)
        disc = coverage(edge, diameter, 0.0, pixel)
        hidden, shown = (Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", image), ("Channel", "Red")), socket="Mean") for image in (disc * occluded, disc))
        visible = front * (1.0 - hidden / shown)
        direction = xyz(*(math(operation, math("ARCTAN2", offset["Y"], offset["X"])) for operation in ("COSINE", "SINE")))
        ((streak, _),) = textured(
            shell,
            "starburst_basis",
            record.basis,
            vector("SCALE", direction, inlet("starburst_density") / tau),
            clocked(inlet("starburst_clock_speed"), inlet("starburst_clock_hold")),
            inlet("seed"),
            streams=(textures.stream(LensFlare, "starburst_basis"),),
        )
        star_tone, star_falloff = ramped(rho, inlet("starburst_length"))
        turned = rotated(offset, inlet("anamorphic_angle"), inverted=True)
        bar_tone, bar_falloff = ramped(abs(turned["X"]), inlet("anamorphic_length"))
        delta = vector("LENGTH", frame.point - vector("MULTIPLY_ADD", axis, inlet("halo_magnification"), center)) - inlet("halo_radius")
        width = inlet("halo_width")
        sections = (
            ("hotspot", record.hotspot, coverage(edge, diameter, inlet("hotspot_softness"), pixel), ramped(rho, radius)[0]),
            ("starburst", record.starburst, math("MAXIMUM", math("MULTIPLY_ADD", streak, 2.0, -1.0), 0.0) ** inlet("starburst_sharpness") * star_falloff, star_tone),
            (
                "anamorphic",
                record.anamorphic,
                coverage(math("MULTIPLY_ADD", inlet("anamorphic_thickness"), -0.5, abs(turned["Y"])), Some(inlet("anamorphic_thickness")), 0.0, pixel) * bar_falloff,
                bar_tone,
            ),
            ("halo", record.halo, coverage(math("MULTIPLY_ADD", width, -0.5, abs(delta)), Some(width), inlet("halo_softness"), pixel), math("ADD", delta / width, 0.5, clamp=True)),
        )

        def imaged(index: int) -> tuple[Expr, Expr]:
            drawn = vector("SCALE", origin(inlet("seed"), float(textures.stream(LensFlare, "ghosts")), instance=index), 1.0 / 256.0)
            shrink = 1.0 - inlet("ghosts_variation") * drawn["X"]
            slot = math("MULTIPLY_ADD", drawn["Y"] - 0.5, inlet("ghosts_variation"), index + 0.5) / record.count
            magnification = mixed(slot, inlet("ghosts_first"), inlet("ghosts_last"))
            flipped = vector("SCALE", frame.point - vector("MULTIPLY_ADD", axis, magnification, center), math("MULTIPLY_ADD", magnification < 0.0, -2.0, 1.0))
            point = xyz(flipped["X"] * inlet("ghosts_squeeze"), flipped["Y"] * -1.0)
            bearing, reach = math("ARCTAN2", point["Y"], point["X"]), vector("LENGTH", point)
            outer, inner = record.iris.radial(bearing, Expr), record.iris.radial(bearing, Expr, record.iris.obstruction)
            size = inlet("ghosts_radius") * shrink
            ramp = inlet("ghosts_softness") * size + pixel
            fringe = inlet("ghosts_fringe")

            def channel(factor: Expr) -> Expr:
                extent = size * factor
                kept = math("SUBTRACT", 0.5, (reach - extent * outer) / ramp, clamp=True)
                covered = math("MINIMUM", kept, math("ADD", 1.0, (reach - extent * inner) / ramp, clamp=True)) if record.iris.obstruction > 0.0 else kept
                return switched(extent > 0.0, 0.0, covered)

            channels = xyz(*map(channel, (1.0 - math("MAXIMUM", fringe, 0.0), math("MULTIPLY_ADD", abs(fringe), -0.5, 1.0), 1.0 + math("MINIMUM", fringe, 0.0))))
            return (vector("SCALE", channels, 1.0 / (shrink * shrink)) if record.iris.obstruction < 1.0 else xyz()), slot

        images = tuple(map(imaged, range(record.count)))
        along = vector("DOT_PRODUCT", frame.point - center, axis) / vector("DOT_PRODUCT", axis, axis)
        graded = math("DIVIDE", along - inlet("ghosts_first"), inlet("ghosts_last") - inlet("ghosts_first"), clamp=True)
        head, *tail = (slot if record.grading is Grading.INSTANCE else graded for _, slot in images)

        def summed(lights: tuple[tuple[tuple[Expr, ...], ...], tuple[Expr, ...]]) -> Expr:
            section_lights, ghost_lights = lights
            first_light, *lighted = (switched(inlet(gate), (0.0, 0.0, 0.0), vector("SCALE", light, weight)) for (gate, _, weight, _), (light, *_) in zip(sections, section_lights))
            glow = tuple(vector("MULTIPLY", mask, light) for (mask, _), light in zip(images, ghost_lights))
            total = sum((*lighted, switched(inlet("ghosts"), (0.0, 0.0, 0.0), sum(glow[1:], glow[0]))), first_light)
            return mixed(1.0, shell.color, vector("SCALE", total, visible), blend="ADD")

        return paired(accumulated(emitted(shell, f"{key}_emission", light, (tone,)) for key, light, _, tone in sections), emitted(shell, "ghosts_emission", record.ghosts, (head, *tail))).map(summed)

    return located(shell).bind(lit)


# --- [CHECKS]


def _source_checked[R: LightRays | LensFlare](identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], _record: R, _context: Context) -> Outcome[None]:
    """Refuses a bound object source without an object or under a camera that cannot project it."""

    def valued(held: bpy.types.SceneCompositorEffect, key: Key) -> Outcome[Value]:
        return entry_input(identity, held, key).bind(lambda socket: Descriptor.resolve(socket, "value", bpy.types.Property).map(lambda descriptor: descriptor.read(socket)))

    def bound(held: bpy.types.SceneCompositorEffect, kind: Value, reading: Value) -> Outcome[None]:
        present = entry_input(identity, held, "source_object").bind(
            lambda socket: Descriptor.resolve(socket, "value", bpy.types.PointerProperty).bind(lambda descriptor: Present(descriptor=descriptor).check(bpy.context, Some(socket)))
        )
        camera = camera_type(*(("PERSP",) if reading == "Direction" else ("PERSP", "ORTHO"))).check(bpy.context, Some(scene))
        return paired(present, camera).map(lambda _: None) if kind == "Object" else Ok(None)

    return entry.map(lambda held: paired(valued(held, "source_kind"), valued(held, "source_reading")).bind(lambda pair: bound(held, *pair))).default_value(Ok(None))


# --- [COMPOSITION] ----------------------------------------------------------------------

light_rays = CompositorEffect(
    record=LightRays,
    graph=beamed,
    interface=Interface(
        inputs=(
            *masks.HIGHLIGHTS.inputs,
            Socket(key="length", name="Length", kind=bpy.types.NodeSocketFloat, description="Longest march over greater frame side", members={"min_value": 0.0, "default_value": 0.5}),
            Socket(key="jitter", name="Jitter", kind=bpy.types.NodeSocketFloat, description="Share of march steps skipped at hashed shifts", members=FACTOR),
            Socket(key="color", name="Color", kind=bpy.types.NodeSocketColor, description="Scene-linear shaft tint and strength", members={"default_value": (1.0, 1.0, 1.0, 1.0)}),
        ),
        panels=(_SOURCE,),
    ),
    parts=(Requests[LightRays](request=lambda _record, layer: PassRequest.depth(layer, precise=False)), Checks[LightRays](check=_source_checked)),
)
lens_flare = CompositorEffect(
    record=LensFlare,
    graph=flared,
    interface=Interface(
        inputs=(
            Socket(key="radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Source disc radius over greater frame side", members={"min_value": 0.0, "default_value": 0.01}),
            Socket(key="seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting streak and ghost pattern", members={"min_value": 0, "max_value": 2**24}),
        ),
        panels=(
            _SOURCE,
            *(
                InterfacePanel(
                    key=key,
                    name=name,
                    description=description,
                    toggle=Some(enabled),
                    inputs=tuple(
                        Socket(key=item_key(key, field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members) for field, text, members in rows
                    ),
                    panels=(emission(item_key(key, "emission"), light, light_text), *extra),
                )
                for key, name, description, enabled, light, light_text, extra, rows in (
                    (
                        "hotspot",
                        "Hotspot",
                        "Source disc",
                        True,
                        LensFlare().hotspot,
                        "Light from disc center to rim",
                        (),
                        (("softness", "Edge ramp past one pixel over greater frame side", {"min_value": 0.0, "default_value": 0.01}),),
                    ),
                    (
                        "starburst",
                        "Starburst",
                        "Radial streaks about source",
                        True,
                        LensFlare().starburst,
                        "Light from streak root to tip",
                        (bases("starburst_basis", LensFlare().basis, "Texture forming streaks", "Noise"), clock("starburst_clock", speed=0.0)),
                        (
                            ("length", "Streak reach over greater frame side", {"min_value": 0.0, "default_value": 0.25}),
                            ("density", "Noise cells around source", {"min_value": 0.0, "default_value": 24.0}),
                            ("sharpness", "Exponent narrowing each streak", {"min_value": 1.0, "default_value": 3.0}),
                        ),
                    ),
                    (
                        "anamorphic",
                        "Anamorphic Bar",
                        "Line through source along squeeze axis",
                        False,
                        LensFlare().anamorphic,
                        "Light from bar center to ends",
                        (),
                        (
                            ("length", "Bar half length over greater frame side", {"min_value": 0.0, "default_value": 0.6}),
                            ("thickness", "Bar width over greater frame side", {"min_value": 0.0, "default_value": 0.004}),
                            ("angle", "Counterclockwise squeeze axis turn", ANGLE),
                        ),
                    ),
                    (
                        "halo",
                        "Halo",
                        "Ring centered along source axis",
                        True,
                        LensFlare().halo,
                        "Light from inner to outer ring edge",
                        (),
                        (
                            ("magnification", "Ring offset from frame center over source offset", {"default_value": -0.4}),
                            ("radius", "Ring radius over greater frame side", {"min_value": 0.0, "default_value": 0.2}),
                            ("width", "Ring width over greater frame side", {"min_value": 0.0, "default_value": 0.02}),
                            ("softness", "Edge ramp past one pixel over greater frame side", {"min_value": 0.0, "default_value": 0.02}),
                        ),
                    ),
                    (
                        "ghosts",
                        "Ghosts",
                        "Aperture images along source axis",
                        True,
                        LensFlare().ghosts,
                        "Light from first ghost to last",
                        (),
                        (
                            ("squeeze", "Horizontal aperture image compression", {"min_value": 0.0, "default_value": 1.0}),
                            ("fringe", "Channel radius shift, toward blue above zero", {"min_value": -1.0, "max_value": 1.0}),
                            ("first", "First ghost magnification", {"default_value": 0.6}),
                            ("last", "Last ghost magnification", {"default_value": -1.0}),
                            ("radius", "Largest ghost radius over greater frame side", {"min_value": 0.0, "default_value": 0.03}),
                            ("variation", "Size and position jitter within slots", {"min_value": 0.0, "max_value": 0.999, "default_value": 0.5}),
                            ("softness", "Edge ramp over ghost radius", {**FACTOR, "default_value": 0.3}),
                        ),
                    ),
                )
            ),
        ),
    ),
    parts=(Requests[LensFlare](request=lambda _record, layer: PassRequest.depth(layer, precise=True)), Checks[LensFlare](check=_source_checked)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Grading", "LensFlare", "LightRays", "lens_flare", "light_rays"]
