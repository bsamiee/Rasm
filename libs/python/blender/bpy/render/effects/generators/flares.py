"""Projected source light, shafts, and procedural lens-flare sections."""

from enum import IntEnum
from functools import partial
from math import tau

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.rna.conditions import check, Present
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import Expr, math, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import item_key, Socket
from blender.bpy.render.effects.coordinates import clocked, lensed, origin, sampled
from blender.bpy.render.effects.generators import noise
from blender.bpy.render.effects.generators.shapes import coverage
from blender.bpy.render.effects.optics.defocus import outlined
from blender.bpy.render.effects.optics.glare import keyed, WHOLE
from blender.bpy.render.effects.records import entry_input
from blender.bpy.render.effects.rows import CompositorEffect, Context, Shell
from blender.bpy.render.view_layers.passes import PassRequest
from blender.bpy.scene.cameras import camera_type
from blender.imaging.effects.generators.flares import Anamorphic, BoundObject, FramePosition, Ghosts, Grading, Halo, Hotspot, LensFlare, LightRays, Reading, Starburst
from blender.imaging.effects.optics.defocus import Aperture
from blender.imaging.effects.optics.glare import Highlights
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Gated, Motion

# --- [TYPES] ----------------------------------------------------------------------------

type SourceEffect = LightRays | LensFlare


class _Noise(IntEnum):
    STAR = 0
    GHOST = 1


# --- [OPERATIONS] -----------------------------------------------------------------------


def located[R: SourceEffect](shell: Shell[R]) -> tuple[Expr, Expr, Option[Expr]]:
    """Projects selected source with front and occlusion fields.

    Args:
        shell: Source menus, object input, and depth endpoint.

    Returns:
        Frame-relative position, front gate, and depth-pass occlusion, `Nothing` for a non-occluding source.
    """
    source, *_ = type(shell.record).__struct_fields__
    reading, _ = BoundObject.__struct_fields__
    (position,) = FramePosition.__struct_fields__
    direction = shell.menu(source, BoundObject, reading, cases={Reading.POSITION.label: False, Reading.DIRECTION.label: True}, kind="BOOLEAN")
    bound = shell.menu(source, cases={FramePosition.title: False, BoundObject.title: True}, kind="BOOLEAN")
    camera = Expr(
        kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", Expr(kind=bpy.types.GeometryNodeInputActiveCamera, socket="Active Camera").named("camera")),), socket="Location", shape="VECTOR"
    )
    target = Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", shell.input(BoundObject)),), socket="Location", shape="VECTOR")
    axis = Expr(kind=bpy.types.FunctionNodeRotateVector, inputs=(("Vector", (0.0, 0.0, 1.0)), ("Rotation", msgspec.structs.replace(target, socket="Rotation"))), socket="Vector", shape="VECTOR")
    inverse = Expr(kind=bpy.types.FunctionNodeInvertRotation, inputs=(("Rotation", msgspec.structs.replace(camera, socket="Rotation")),), socket="Rotation")
    local = Expr(kind=bpy.types.FunctionNodeRotateVector, inputs=(("Vector", switched(direction, target - camera, axis)), ("Rotation", inverse)), socket="Vector", shape="VECTOR")
    device = Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", local), ("Transform", lensed("Projection Matrix"))), socket="Vector", shape="VECTOR")
    reach = switched(direction, -local["Z"], lensed("Clip End"))
    return (
        switched(bound, shell.input(source, FramePosition, position), vector("MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0))),
        switched(bound, 1.0, local["Z"] < 0.0),
        Some(shell.layer("Depth") < reach) if _occluding(shell.record) else Nothing,
    )


def ramped(distance: Expr, length: Scalar) -> tuple[Expr, Expr]:
    """Returns tone `min(distance / length, 1)` and squared remaining share `(max(length - distance, 0) / length)²` of a section reaching `length`."""
    remaining = math("MAXIMUM", math("SUBTRACT", length, distance), 0.0) / length
    return math("MINIMUM", distance / length, 1.0), remaining * remaining


def beamed(shell: Shell[LightRays]) -> BlenderResult[Expr]:
    """Adds highlight shafts through source depth gaps.

    Args:
        shell: Entering light, frame measures, source, highlight, and ray controls.

    Returns:
        Associated light with shafts.
    """
    _, highlights, length, jitter, color = LightRays.__struct_fields__
    threshold, smoothness = Highlights.__struct_fields__
    position, front, occlusion = located(shell)
    key = keyed(shell.color, shell.input(highlights, threshold), shell.input(highlights, smoothness))
    diagonal = vector("LENGTH", Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR"))
    rays = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", occlusion.map(lambda occluded: vector("SCALE", key, 1.0 - occluded)).default_value(key)),
            ("Type", "Sun Beams"),
            *WHOLE,
            ("Sun Position", sampled(shell.space, position)),
            ("Size", shell.input(length) * shell.space.greater / diagonal),
            ("Jitter", shell.input(jitter)),
        ),
        socket="Glare",
        shape="RGBA",
    )
    return Ok(mixed(1.0, shell.color, vector("SCALE", vector("MULTIPLY", rays, shell.input(color)), front), blend="ADD"))


def flared(shell: Shell[LensFlare]) -> BlenderResult[Expr]:
    """Adds source-centered flare sections under finite-disc visibility.

    Args:
        shell: Entering light, frame, projected source, and section controls.

    Returns:
        Entering light with visible section sum, or the working luminance fault.
    """
    inlet, frame = shell.input, shell.space
    _, radius_field, hotspot, starburst, anamorphic, halo, ghosts, seed = LensFlare.__struct_fields__
    _, value = Gated.__struct_fields__
    hotspot_emission, hotspot_softness = Hotspot.__struct_fields__
    star_emission, basis, star_length, density, sharpness, motion = Starburst.__struct_fields__
    bar_emission, bar_length, thickness, angle = Anamorphic.__struct_fields__
    halo_emission, magnification_field, halo_radius, width, halo_softness = Halo.__struct_fields__
    ghost_emission, iris, _, squeeze, fringe_field, _, first, last, ghost_radius, variation, ghost_softness = Ghosts.__struct_fields__
    blades, rotation, roundness, obstruction = Aperture.__struct_fields__
    speed, hold = Motion.__struct_fields__
    spot, star, bar, ring, ghost = ((path, value) for path in (hotspot, starburst, anamorphic, halo, ghosts))
    count, grading = shell.record.ghosts.value.count, shell.record.ghosts.value.grading
    position, front, occlusion = located(shell)
    source = position * frame.extent
    center = frame.extent * 0.5
    axis, offset = source - center, frame.point - source
    rho, pixel, radius = vector("LENGTH", offset), 1.0 / frame.greater, inlet(radius_field)
    edge, diameter = rho - radius, Some(radius * 2.0)
    disc = coverage(edge, diameter, 0.0, pixel)

    def visible(occluded: Expr) -> Expr:
        hidden, shown = (Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", image), ("Channel", "Red")), socket="Mean") for image in (disc * occluded, disc))
        return front * (1.0 - hidden / shown)

    direction = xyz(*(math(operation, math("ARCTAN2", offset["Y"], offset["X"])) for operation in ("COSINE", "SINE")))
    clock = clocked(inlet(*star, motion, speed), inlet(*star, motion, hold))
    ((streak, _),) = noise.sampled(shell, (*star, basis), vector("SCALE", direction, inlet(*star, density) / tau), clock, inlet(seed), streams=(_Noise.STAR,))
    star_tone, star_falloff = ramped(rho, inlet(*star, star_length))
    rotated = Expr(
        kind=bpy.types.ShaderNodeVectorRotate, inputs=(("Vector", offset), ("Angle", inlet(*bar, angle))), members=(("rotation_type", "Z_AXIS"), ("invert", True)), socket="Vector", shape="VECTOR"
    )
    bar_tone, bar_falloff = ramped(abs(rotated["X"]), inlet(*bar, bar_length))
    delta = vector("LENGTH", frame.point - vector("MULTIPLY_ADD", axis, inlet(*ring, magnification_field), center)) - inlet(*ring, halo_radius)
    sections = (
        (spot, hotspot_emission, coverage(edge, diameter, inlet(*spot, hotspot_softness), pixel), ramped(rho, radius)[0]),
        (star, star_emission, math("MAXIMUM", math("MULTIPLY_ADD", streak, 2.0, -1.0), 0.0) ** inlet(*star, sharpness) * star_falloff, star_tone),
        (bar, bar_emission, coverage(math("MULTIPLY_ADD", inlet(*bar, thickness), -0.5, abs(rotated["Y"])), Some(inlet(*bar, thickness)), 0.0, pixel) * bar_falloff, bar_tone),
        (
            ring,
            halo_emission,
            coverage(math("MULTIPLY_ADD", inlet(*ring, width), -0.5, abs(delta)), Some(inlet(*ring, width)), inlet(*ring, halo_softness), pixel),
            math("ADD", delta / inlet(*ring, width), 0.5, clamp=True),
        ),
    )

    def imaged(index: int) -> tuple[Expr, Expr]:
        drawn = vector("SCALE", origin(inlet(seed), _Noise.GHOST, instance=index), 1.0 / 256.0)
        shrink = 1.0 - inlet(*ghost, variation) * drawn["X"]
        slot = math("MULTIPLY_ADD", drawn["Y"] - 0.5, inlet(*ghost, variation), index + 0.5) / count
        magnification = mixed(slot, inlet(*ghost, first), inlet(*ghost, last))
        flipped = vector("SCALE", frame.point - vector("MULTIPLY_ADD", axis, magnification, center), math("MULTIPLY_ADD", magnification < 0.0, -2.0, 1.0))
        point = xyz(flipped["X"] * inlet(*ghost, squeeze), flipped["Y"] * -1.0)
        size = inlet(*ghost, ghost_radius) * shrink
        ramp = inlet(*ghost, ghost_softness) * size + pixel
        fringe, hole = inlet(*ghost, fringe_field), inlet(*ghost, iris, obstruction)

        def channel(factor: Expr) -> Expr:
            extent = size * factor
            outer, inner = outlined(point, extent, *(inlet(*ghost, iris, field) for field in (blades, rotation, roundness, obstruction)))
            covered = math("MINIMUM", math("SUBTRACT", 0.5, outer / ramp, clamp=True), switched(hole > 0.0, 1.0, math("ADD", 1.0, inner / ramp, clamp=True)))
            return switched(extent > 0.0, 0.0, covered)

        channels = xyz(*map(channel, (1.0 - math("MAXIMUM", fringe, 0.0), math("MULTIPLY_ADD", abs(fringe), -0.5, 1.0), 1.0 + math("MINIMUM", fringe, 0.0))))
        return switched(hole < 1.0, (0.0, 0.0, 0.0), vector("SCALE", channels, 1.0 / (shrink * shrink))), slot

    images = tuple(map(imaged, range(count)))
    along = vector("DOT_PRODUCT", frame.point - center, axis) / vector("DOT_PRODUCT", axis, axis)
    graded = math("DIVIDE", along - inlet(*ghost, first), inlet(*ghost, last) - inlet(*ghost, first), clamp=True)
    head, *tail = (slot if grading is Grading.INSTANCE else graded for _, slot in images)

    def lit(lights: tuple[tuple[tuple[Expr, ...], ...], tuple[Expr, ...]]) -> Expr:
        section_lights, ghost_lights = lights
        first_light, *lighted = (switched(shell.gate(gate), (0.0, 0.0, 0.0), vector("SCALE", light, weight)) for ((gate, _), _, weight, _), (light, *_) in zip(sections, section_lights))
        glow = tuple(vector("MULTIPLY", mask, light) for (mask, _), light in zip(images, ghost_lights))
        total = sum((*lighted, switched(shell.gate(ghosts), (0.0, 0.0, 0.0), sum(glow[1:], glow[0]))), first_light)
        return mixed(1.0, shell.color, vector("SCALE", total, occlusion.map(visible).default_value(front)), blend="ADD")

    return paired(accumulated(noise.emitted(shell, (*path, emission), (tone,)) for path, emission, _, tone in sections), noise.emitted(shell, (*ghost, ghost_emission), (head, *tail))).map(lit)


def _source_validated[R: SourceEffect](identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: R, _context: Context) -> BlenderResult[None]:
    match record.source:
        case FramePosition():
            return Ok(None)
        case BoundObject(reading=reading):
            present = entry.map(
                lambda held: entry_input(identity, held, item_key(BoundObject)).bind(
                    lambda value: Descriptor.resolve(value, "value", bpy.types.PointerProperty).bind(lambda descriptor: check(Present(descriptor=descriptor), bpy.context, Some(value)))
                )
            ).default_value(Error((BlenderAbsentFault(owner=repr(scene), key=item_key(BoundObject)),)))
            camera = check(camera_type(*(("PERSP",) if reading is Reading.DIRECTION else ("PERSP", "ORTHO"))), bpy.context, Some(scene))
            return paired(present, camera).map(lambda _: None)


def _occluding(record: SourceEffect) -> bool:
    return isinstance(record.source, BoundObject) and record.source.occlusion


def _requested[R: SourceEffect](record: R, layer: str, *, precise: bool) -> BlenderResult[PassRequest]:
    return PassRequest.depth(layer, precise=precise) if _occluding(record) else Ok(PassRequest(layer=layer, precise=precise))


# --- [COMPOSITION] ----------------------------------------------------------------------

_source_inputs = (Socket(key=item_key(BoundObject), name="Object", kind=bpy.types.NodeSocketObject),)
light_rays = CompositorEffect(record=LightRays, graph=beamed, inputs=_source_inputs, requested=Some(partial(_requested, precise=False)), native_validated=Some(_source_validated))
lens_flare = CompositorEffect(record=LensFlare, graph=flared, inputs=_source_inputs, requested=Some(partial(_requested, precise=True)), native_validated=Some(_source_validated))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["beamed", "flared", "lens_flare", "light_rays", "located", "ramped"]
