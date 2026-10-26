"""Light shafts and lens flares generated about a frame position or a bound scene object seen through the camera."""

from math import tau
from typing import Annotated, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.conditions import check, Present
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import Expr, math, mixed, NODES, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import item_key, Socket
from blender.bpy.nodes.ramps import Ramp, Stop
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.coordinates import Clock, clocked, origin, sampled, Sighted
from blender.bpy.render.compositor.effects import Checks, CompositorEffect, Context, Inputs, Requests, Shell
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.compositor.shapes import coverage, rotated
from blender.bpy.render.view_layers.passes import PassRequest
from blender.bpy.scene.cameras import camera_type
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.geometry.regions import FrameLength, Position
from blender.records.faults import accumulated, paired
from blender.records.fields import AtLeastOne, Case, Choice, Finite, Gated, NonNegative, Positive, Presentation, Quantity, Rgb, Seed, Unit

from ...chain.stages import Effect, Stage
from .procedural import Basis, Emission, emitted, Gradient, Perlin, textured, Tint

# --- [TYPES] ----------------------------------------------------------------------------


class Reading(Choice):
    """Object location or direction toward a light at infinity."""

    POSITION = 1
    DIRECTION = 2


class Grading(Choice):
    """Ghost color sampled per aperture image or continuously along axis."""

    INSTANCE = 1, "Per Ghost"
    AXIS = 2, "Along Axis"


type Source = FramePosition | BoundObject
type SourceEffect = LightRays | LensFlare

# --- [MODELS] ---------------------------------------------------------------------------


class FramePosition(Case, frozen=True, kw_only=True):
    """Source at frame position, needing no camera or depth."""

    position: Annotated[Position, msgspec.Meta(description="Source in frame fractions")] = (0.3, 0.6)


class BoundObject(Case, frozen=True, kw_only=True):
    """Bound scene object read as location or local +Z direction."""

    title = "Object"
    reading: Annotated[Reading, msgspec.Meta(description="Location or local +Z toward distant light")] = Reading.POSITION
    occlusion: Annotated[bool, msgspec.Meta(description="Nearer surfaces hide source")] = True


class LightRays(Effect, frozen=True, kw_only=True):
    """Highlight shafts marched toward a source through gaps in nearer surfaces."""

    stage = Stage.GENERATE
    pointwise = False
    source: Annotated[Source, msgspec.Meta(description="Point shafts converge on")] = FramePosition()
    highlights: Annotated[masks.Highlights, msgspec.Meta(description="Light streaming toward source")] = masks.Highlights()
    length: Annotated[FrameLength, msgspec.Meta(description="Longest march over greater frame side"), Presentation(soft=Some((0.0, 1.0)))] = 0.5
    jitter: Annotated[Unit, msgspec.Meta(description="Share of march steps skipped at hashed shifts")] = 0.0
    color: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear shaft tint and strength")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Hotspot(msgspec.Struct, frozen=True, kw_only=True):
    """Source disc graded from center to rim."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from disc center to rim")] = Emission(
        spectrum=Tint(start=Rgb(red=1.0, green=0.9, blue=0.8), end=Rgb(red=1.0, green=0.9, blue=0.8)), intensity=3.0
    )
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp past one pixel over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01


class Starburst(msgspec.Struct, frozen=True, kw_only=True):
    """Radial streaks with periodic angular noise and quadratic length falloff."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from streak root to tip")] = Emission(
        spectrum=Tint(start=Rgb(red=1.0, green=0.9, blue=0.8), end=Rgb(red=1.0, green=0.9, blue=0.8)), intensity=0.0
    )
    basis: Annotated[Basis, msgspec.Meta(title="Noise", description="Texture forming streaks")] = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=1.0, roughness=0.563))
    length: Annotated[FrameLength, msgspec.Meta(description="Streak reach over greater frame side")] = 0.25
    density: Annotated[NonNegative, msgspec.Meta(description="Noise cells around source"), Presentation(soft=Some((0.0, 64.0)))] = 24.0
    sharpness: Annotated[AtLeastOne, msgspec.Meta(description="Exponent narrowing each streak"), Presentation(soft=Some((1.0, 8.0)))] = 3.0
    clock: Annotated[Clock, msgspec.Meta(description="Clock advancing streaks through noise cells")] = Clock(speed=0.0)


class Anamorphic(msgspec.Struct, frozen=True, kw_only=True):
    """Bar through source along cylindrical lens squeeze axis."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from bar center to ends")] = Emission(
        spectrum=Tint(start=Rgb(red=0.04, green=0.34, blue=1.0), end=Rgb(red=0.04, green=0.34, blue=1.0)), intensity=0.0
    )
    length: Annotated[FrameLength, msgspec.Meta(description="Bar half length over greater frame side")] = 0.6
    thickness: Annotated[FrameLength, msgspec.Meta(description="Bar width over greater frame side"), Presentation(soft=Some((0.0, 0.02)))] = 0.004
    angle: Annotated[Finite, msgspec.Meta(description="Counterclockwise squeeze axis turn"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0


class Halo(msgspec.Struct, frozen=True, kw_only=True):
    """Ring centered along axis from frame center through source."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from inner to outer ring edge")] = Emission(
        spectrum=Tint(start=Rgb(red=0.124, green=0.009, blue=0.194), end=Rgb(red=0.124, green=0.009, blue=0.194)), intensity=0.0
    )
    magnification: Annotated[Finite, msgspec.Meta(description="Ring offset from frame center over source offset"), Presentation(soft=Some((-2.0, 2.0)))] = -0.4
    radius: Annotated[FrameLength, msgspec.Meta(description="Ring radius over greater frame side")] = 0.2
    width: Annotated[FrameLength, msgspec.Meta(description="Ring width over greater frame side"), Presentation(soft=Some((0.0, 0.1)))] = 0.02
    softness: Annotated[FrameLength, msgspec.Meta(description="Edge ramp past one pixel over greater frame side"), Presentation(soft=Some((0.0, 0.1)))] = 0.02


class Ghosts(msgspec.Struct, frozen=True, kw_only=True):
    """Aperture images along source axis with seeded spacing and conserved area gain."""

    emission: Annotated[Emission, msgspec.Meta(description="Light from first ghost to last")] = Emission(
        spectrum=Gradient(
            ramp=Ramp(
                stops=(
                    Stop(position=0.0, color=Rgb(red=0.12, green=0.02, blue=0.2)),
                    Stop(position=0.5, color=Rgb(red=0.02, green=0.1, blue=0.08)),
                    Stop(position=1.0, color=Rgb(red=0.2, green=0.08, blue=0.02)),
                )
            )
        ),
        intensity=0.0,
    )
    iris: Annotated[Aperture, msgspec.Meta(description="Aperture stop imaged by each ghost"), GraphForm.SETTING] = Aperture()
    grading: Annotated[Grading, msgspec.Meta(description="Spectrum sampling"), GraphForm.SETTING] = Grading.INSTANCE
    squeeze: Annotated[Positive, msgspec.Meta(description="Horizontal aperture image compression"), Presentation(soft=Some((1.0, 2.0)))] = 1.0
    fringe: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Channel radius shift, toward blue above zero")] = 0.0
    count: Annotated[int, msgspec.Meta(ge=1, description="Aperture images along axis"), Presentation(soft=Some((1.0, 8.0))), GraphForm.SETTING] = 5
    first: Annotated[Finite, msgspec.Meta(description="First ghost magnification"), Presentation(soft=Some((-2.0, 2.0)))] = 0.6
    last: Annotated[Finite, msgspec.Meta(description="Last ghost magnification"), Presentation(soft=Some((-2.0, 2.0)))] = -1.0
    radius: Annotated[FrameLength, msgspec.Meta(description="Largest ghost radius over greater frame side"), Presentation(soft=Some((0.0, 0.2)))] = 0.03
    variation: Annotated[float, msgspec.Meta(ge=0.0, lt=1.0, description="Size and position jitter within slots")] = 0.5
    softness: Annotated[Unit, msgspec.Meta(description="Edge ramp over ghost radius")] = 0.3


class LensFlare(Effect, frozen=True, kw_only=True):
    """Source disc, radial streaks, anamorphic bar, halo, and ghosts generated as light."""

    stage = Stage.GENERATE
    pointwise = False
    source: Annotated[Source, msgspec.Meta(description="Light the lens images")] = FramePosition()
    radius: Annotated[FrameLength, msgspec.Meta(description="Source disc radius over greater frame side"), Presentation(soft=Some((0.0, 0.05)))] = 0.01
    hotspot: Annotated[Gated[Hotspot], msgspec.Meta(description="Source disc")] = Gated(enabled=True, value=Hotspot())
    starburst: Annotated[Gated[Starburst], msgspec.Meta(description="Radial streaks about source")] = Gated(enabled=True, value=Starburst())
    anamorphic: Annotated[Gated[Anamorphic], msgspec.Meta(title="Anamorphic Bar", description="Line through source along squeeze axis")] = Gated(enabled=False, value=Anamorphic())
    halo: Annotated[Gated[Halo], msgspec.Meta(description="Ring centered along source axis")] = Gated(enabled=True, value=Halo())
    ghosts: Annotated[Gated[Ghosts], msgspec.Meta(description="Aperture images along source axis")] = Gated(enabled=True, value=Ghosts())
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting streak and ghost pattern")] = 0


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SOURCE]


def located[R: SourceEffect](shell: Shell[R]) -> tuple[Expr, Expr, Option[Expr]]:
    """Returns the source's frame fractions, its ahead gate, and its depth-pass occlusion where the record occludes."""
    source, *_ = type(shell.record).__struct_fields__
    reading, _ = BoundObject.__struct_fields__
    (position,) = FramePosition.__struct_fields__
    direction = shell.menu(source, BoundObject, reading, cases={Reading.POSITION.label: False, Reading.DIRECTION.label: True}, kind="BOOLEAN")
    bound = shell.menu(source, cases={FramePosition.title: False, BoundObject.title: True}, kind="BOOLEAN")
    sighted = Sighted.of(shell.input(BoundObject), direction)
    return (
        switched(bound, shell.input(source, FramePosition, position), sighted.position),
        switched(bound, 1.0, sighted.ahead),
        Some(shell.layer("Depth") < sighted.depth) if _occluding(shell.record) else Nothing,
    )


def ramped(distance: Expr, length: Scalar) -> tuple[Expr, Expr]:
    """Returns tone `min(distance / length, 1)` and squared remaining share of a section reaching `length`."""
    remaining = math("MAXIMUM", math("SUBTRACT", length, distance), 0.0) / length
    return math("MINIMUM", distance / length, 1.0), remaining * remaining


# --- [GRAPHS]


def beamed(shell: Shell[LightRays]) -> BlenderResult[Expr]:
    """Adds highlight shafts marched toward the source through its depth gaps."""
    _, highlights, length, jitter, color = LightRays.__struct_fields__
    threshold, smoothness = masks.Highlights.__struct_fields__
    position, front, occlusion = located(shell)
    key = masks.highlight(shell.color, shell.input(highlights, threshold), shell.input(highlights, smoothness))
    diagonal = vector("LENGTH", Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR"))
    rays = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", occlusion.map(lambda occluded: vector("SCALE", key, 1.0 - occluded)).default_value(key)),
            ("Type", "Sun Beams"),
            ("Quality", "High"),
            ("Clamp Highlights", False),
            ("Highlights Threshold", 0.0),
            ("Highlights Smoothness", 0.0),
            ("Sun Position", sampled(shell.space, position)),
            ("Size", shell.input(length) * shell.space.greater / diagonal),
            ("Jitter", shell.input(jitter)),
        ),
        socket="Glare",
        shape="RGBA",
    )
    return Ok(mixed(1.0, shell.color, vector("SCALE", vector("MULTIPLY", rays, shell.input(color)), front), blend="ADD"))


def flared(shell: Shell[LensFlare]) -> BlenderResult[Expr]:
    """Adds the source-centered flare sections to the stage input under finite-disc visibility."""
    inlet, frame = shell.input, shell.space
    _, radius_field, hotspot, starburst, anamorphic, halo, ghosts, seed = LensFlare.__struct_fields__
    _, value = Gated.__struct_fields__
    hotspot_emission, hotspot_softness = Hotspot.__struct_fields__
    star_emission, basis, star_length, density, sharpness, clock = Starburst.__struct_fields__
    bar_emission, bar_length, thickness, angle = Anamorphic.__struct_fields__
    halo_emission, magnification_field, halo_radius, width, halo_softness = Halo.__struct_fields__
    ghost_emission, _, _, squeeze, fringe_field, _, first, last, ghost_radius, variation, ghost_softness = Ghosts.__struct_fields__
    speed, hold = Clock.__struct_fields__
    spot, star, bar, ring, ghost = ((path, value) for path in (hotspot, starburst, anamorphic, halo, ghosts))
    ghosting = shell.record.ghosts.value
    position, front, occlusion = located(shell)
    center = frame.extent * 0.5
    axis, offset = position * frame.extent - center, frame.point - position * frame.extent
    rho, pixel, radius = vector("LENGTH", offset), 1.0 / frame.greater, inlet(radius_field)
    edge, diameter = rho - radius, Some(radius * 2.0)
    disc = coverage(edge, diameter, 0.0, pixel)

    def visible(occluded: Expr) -> Expr:
        hidden, shown = (Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", image), ("Channel", "Red")), socket="Mean") for image in (disc * occluded, disc))
        return front * (1.0 - hidden / shown)

    direction = xyz(*(math(operation, math("ARCTAN2", offset["Y"], offset["X"])) for operation in ("COSINE", "SINE")))
    ((streak, _),) = textured(
        shell,
        (*star, basis),
        vector("SCALE", direction, inlet(*star, density) / tau),
        clocked(inlet(*star, clock, speed), inlet(*star, clock, hold)),
        inlet(seed),
        streams=(textures.stream(Starburst, basis),),
    )
    star_tone, star_falloff = ramped(rho, inlet(*star, star_length))
    turned = rotated(offset, inlet(*bar, angle), inverted=True)
    bar_tone, bar_falloff = ramped(abs(turned["X"]), inlet(*bar, bar_length))
    delta = vector("LENGTH", frame.point - vector("MULTIPLY_ADD", axis, inlet(*ring, magnification_field), center)) - inlet(*ring, halo_radius)
    sections = (
        (spot, hotspot_emission, coverage(edge, diameter, inlet(*spot, hotspot_softness), pixel), ramped(rho, radius)[0]),
        (star, star_emission, math("MAXIMUM", math("MULTIPLY_ADD", streak, 2.0, -1.0), 0.0) ** inlet(*star, sharpness) * star_falloff, star_tone),
        (bar, bar_emission, coverage(math("MULTIPLY_ADD", inlet(*bar, thickness), -0.5, abs(turned["Y"])), Some(inlet(*bar, thickness)), 0.0, pixel) * bar_falloff, bar_tone),
        (
            ring,
            halo_emission,
            coverage(math("MULTIPLY_ADD", inlet(*ring, width), -0.5, abs(delta)), Some(inlet(*ring, width)), inlet(*ring, halo_softness), pixel),
            math("ADD", delta / inlet(*ring, width), 0.5, clamp=True),
        ),
    )

    def imaged(index: int) -> tuple[Expr, Expr]:
        drawn = vector("SCALE", origin(inlet(seed), float(textures.stream(Ghosts)), instance=index), 1.0 / 256.0)
        shrink = 1.0 - inlet(*ghost, variation) * drawn["X"]
        slot = math("MULTIPLY_ADD", drawn["Y"] - 0.5, inlet(*ghost, variation), index + 0.5) / ghosting.count
        magnification = mixed(slot, inlet(*ghost, first), inlet(*ghost, last))
        flipped = vector("SCALE", frame.point - vector("MULTIPLY_ADD", axis, magnification, center), math("MULTIPLY_ADD", magnification < 0.0, -2.0, 1.0))
        point = xyz(flipped["X"] * inlet(*ghost, squeeze), flipped["Y"] * -1.0)
        bearing, reach = math("ARCTAN2", point["Y"], point["X"]), vector("LENGTH", point)
        outer, inner = ghosting.iris.radial(bearing, NODES), ghosting.iris.radial(bearing, NODES, ghosting.iris.obstruction)
        size = inlet(*ghost, ghost_radius) * shrink
        ramp = inlet(*ghost, ghost_softness) * size + pixel
        fringe = inlet(*ghost, fringe_field)

        def channel(factor: Expr) -> Expr:
            extent = size * factor
            kept = math("SUBTRACT", 0.5, (reach - extent * outer) / ramp, clamp=True)
            covered = math("MINIMUM", kept, math("ADD", 1.0, (reach - extent * inner) / ramp, clamp=True)) if ghosting.iris.obstruction > 0.0 else kept
            return switched(extent > 0.0, 0.0, covered)

        channels = xyz(*map(channel, (1.0 - math("MAXIMUM", fringe, 0.0), math("MULTIPLY_ADD", abs(fringe), -0.5, 1.0), 1.0 + math("MINIMUM", fringe, 0.0))))
        return (vector("SCALE", channels, 1.0 / (shrink * shrink)) if ghosting.iris.obstruction < 1.0 else xyz()), slot

    images = tuple(map(imaged, range(ghosting.count)))
    along = vector("DOT_PRODUCT", frame.point - center, axis) / vector("DOT_PRODUCT", axis, axis)
    graded = math("DIVIDE", along - inlet(*ghost, first), inlet(*ghost, last) - inlet(*ghost, first), clamp=True)
    head, *tail = (slot if ghosting.grading is Grading.INSTANCE else graded for _, slot in images)

    def lit(lights: tuple[tuple[tuple[Expr, ...], ...], tuple[Expr, ...]]) -> Expr:
        section_lights, ghost_lights = lights
        first_light, *lighted = (switched(shell.gate(gate), (0.0, 0.0, 0.0), vector("SCALE", light, weight)) for ((gate, _), _, weight, _), (light, *_) in zip(sections, section_lights))
        glow = tuple(vector("MULTIPLY", mask, light) for (mask, _), light in zip(images, ghost_lights))
        total = sum((*lighted, switched(shell.gate(ghosts), (0.0, 0.0, 0.0), sum(glow[1:], glow[0]))), first_light)
        return mixed(1.0, shell.color, vector("SCALE", total, occlusion.map(visible).default_value(front)), blend="ADD")

    return paired(accumulated(emitted(shell, (*path, emission), (tone,)) for path, emission, _, tone in sections), emitted(shell, (*ghost, ghost_emission), (head, *tail))).map(lit)


# --- [CHECKS]


def _source_checked[R: SourceEffect](identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: R, _context: Context) -> BlenderResult[None]:
    """Refuses a bound object source without an object or under a camera that cannot project it."""
    match record.source:
        case FramePosition():
            return Ok(None)
        case BoundObject(reading=reading):
            present = entry.map(
                lambda held: entry_input(identity, held, item_key(BoundObject)).bind(
                    lambda socket: Descriptor.resolve(socket, "value", bpy.types.PointerProperty).bind(lambda descriptor: check(Present(descriptor=descriptor), bpy.context, Some(socket)))
                )
            ).default_value(Error((BlenderAbsentFault(owner=repr(scene), key=item_key(BoundObject)),)))
            camera = check(camera_type(*(("PERSP",) if reading is Reading.DIRECTION else ("PERSP", "ORTHO"))), bpy.context, Some(scene))
            return paired(present, camera).map(lambda _: None)


def _occluding(record: SourceEffect) -> bool:
    """Whether nearer surfaces hide the record's source."""
    return isinstance(record.source, BoundObject) and record.source.occlusion


def _requested[R: SourceEffect](record: R, layer: str) -> BlenderResult[PassRequest]:
    """Requests the Depth pass for an occluding source, full precision for the flare's thin sections."""
    precise = isinstance(record, LensFlare)
    return PassRequest.depth(layer, precise=precise) if _occluding(record) else Ok(PassRequest(layer=layer, precise=precise))


# --- [COMPOSITION] ----------------------------------------------------------------------

_SOURCE: Final = Inputs(sockets=(Socket(key=item_key(BoundObject), name="Object", kind=bpy.types.NodeSocketObject),))
light_rays = CompositorEffect(record=LightRays, graph=beamed, parts=(_SOURCE, Requests[LightRays](request=_requested), Checks[LightRays](check=_source_checked)))
lens_flare = CompositorEffect(record=LensFlare, graph=flared, parts=(_SOURCE, Requests[LensFlare](request=_requested), Checks[LensFlare](check=_source_checked)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Anamorphic", "BoundObject", "FramePosition", "Ghosts", "Grading", "Halo", "Hotspot", "LensFlare", "LightRays", "Reading", "Source", "Starburst", "lens_flare", "light_rays"]
