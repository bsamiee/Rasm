"""Projected source light, native shafts, and procedural lens-flare sections."""

from enum import IntEnum
from math import tau

import bpy
from expression import Error, Nothing, Ok, Option, Some

from rasm.blender.data.rna.conditions import check, Present
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.extension.identity import Identity
from rasm.blender.faults import BlenderAbsentFault, BlenderResult
from rasm.blender.nodes.coordinates import framed, FrameSpace, sampled
from rasm.blender.nodes.graphs import Clock, End, ended, Fragment, math, menu, mixed, node, Operand, origin, summed, switched, vector
from rasm.blender.nodes.interface import item_key, Socket
from rasm.blender.render.effects.generators import noise
from rasm.blender.render.effects.generators.shapes import coverage
from rasm.blender.render.effects.optics.defocus import outlined
from rasm.blender.render.effects.optics.glare import keyed
from rasm.blender.render.effects.stack import CompositorEffect, Context, entry_input, Shell
from rasm.blender.render.view_layers.passes import PassRequest
from rasm.blender.scene.cameras import camera_type
from rasm.imaging.effects.generators.flares import Anamorphic, BoundObject, FramePosition, Ghosts, Grading, Halo, Hotspot, LensFlare, LightRays, Reading, Starburst
from rasm.imaging.effects.optics.defocus import Aperture
from rasm.imaging.effects.optics.glare import Highlights
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Gated, Motion

# --- [TYPES] ----------------------------------------------------------------------------

type SourceEffect = LightRays | LensFlare


class _Noise(IntEnum):
    STAR = 0
    GHOST = 1


# --- [OPERATIONS] -----------------------------------------------------------------------


def located[R: SourceEffect](shell: Shell[R]) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End], Option[tuple[Fragment, End]]]]:
    """Projects the selected source and returns its front and occlusion fields.

    Args:
        shell: Source menus, external object input, and native depth endpoint.

    Returns:
        Frame-relative position and front gate, each with its graph, and per-pixel occlusion read from the depth pass a bound
        occluding source requests, `Nothing` for a source that occludes nothing; or accumulated native construction faults.
    """
    match shell.record:
        case LightRays():
            source, _, _, _, _ = LightRays.__struct_fields__
        case LensFlare():
            source, _, _, _, _, _, _, _ = LensFlare.__struct_fields__
    reading, _ = BoundObject.__struct_fields__
    direction = menu(
        "source_direction", "BOOLEAN", shell.socket(source, BoundObject, reading), shell.items[source, BoundObject, reading], {Reading.POSITION.label: False, Reading.DIRECTION.label: True}
    )
    bound = menu("source_bound", "BOOLEAN", shell.socket(source), shell.items[source,], {FramePosition.title: False, BoundObject.title: True})
    camera = node("source_camera", bpy.types.GeometryNodeInputActiveCamera, {}, {}).bind(
        lambda camera: paired(
            node("source_lens", bpy.types.GeometryNodeCameraInfo, {"Camera": ended("source_camera", "Active Camera")(camera)}, {}),
            node("source_camera_transform", bpy.types.GeometryNodeObjectInfo, {"Object": ended("source_camera", "Active Camera")(camera)}, {}),
        )
    )
    object_info = node("source_object", bpy.types.GeometryNodeObjectInfo, {"Object": shell.socket(BoundObject)}, {})

    def projected(
        lens: Fragment, camera_transform: Fragment, object_transform: Fragment, direction: Operand, bound: Operand
    ) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End], Option[tuple[Fragment, End]]]]:
        location = vector("source_translation", "SUBTRACT", ended("source_object", "Location")(object_transform), ended("source_camera_transform", "Location")(camera_transform))
        axis = node("source_axis", bpy.types.FunctionNodeRotateVector, {"Vector": (0.0, 0.0, 1.0), "Rotation": ended("source_object", "Rotation")(object_transform)}, {}).map(
            ended("source_axis", "Vector")
        )
        inverse = node("source_inverse", bpy.types.FunctionNodeInvertRotation, {"Rotation": ended("source_camera_transform", "Rotation")(camera_transform)}, {}).map(
            ended("source_inverse", "Rotation")
        )
        world = paired(location, axis).bind(lambda points: switched("source_world", "VECTOR", direction, *points))
        local = paired(world, inverse).bind(lambda parts: node("source_local", bpy.types.FunctionNodeRotateVector, {"Vector": parts[0], "Rotation": parts[1]}, {})).map(ended("source_local", "Vector"))

        def selected(local: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End], Option[tuple[Fragment, End]]]]:
            (position_field,) = FramePosition.__struct_fields__
            position = (
                node("source_projected", bpy.types.FunctionNodeProjectPoint, {"Vector": local, "Transform": ended("source_lens", "Projection Matrix")(lens)}, {})
                .map(ended("source_projected", "Vector"))
                .bind(lambda device: vector("source_position", "MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)))
                .bind(lambda position: switched("source_selected_position", "VECTOR", bound, shell.socket(source, FramePosition, position_field), position))
            )
            depth = node("source_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": local}, {}).map(ended("source_axes", "Z"))
            front = depth.bind(lambda depth: math("source_front", "LESS_THAN", depth, 0.0)).bind(lambda front: switched("source_selected_front", "FLOAT", bound, 1.0, front))
            occlusion: BlenderResult[Option[tuple[Fragment, End]]] = (
                depth
                .bind(lambda depth: math("source_distance", "MULTIPLY", depth, -1.0))
                .bind(lambda distance: switched("source_reach", "FLOAT", direction, distance, ended("source_lens", "Clip End")(lens)))
                .bind(lambda reach: math("source_occluded", "LESS_THAN", shell.layer("Depth"), reach))
                .map(Some)
                if _occluding(shell.record)
                else Ok(Nothing)
            )
            return paired(position, paired(front, occlusion)).map(lambda fields: (fields[0], *fields[1]))

        return local.bind(selected)

    return paired(paired(camera, object_info), paired(direction, bound)).bind(lambda parts: projected(*parts[0][0], parts[0][1], *parts[1]))


def beamed(shell: Shell[LightRays]) -> BlenderResult[tuple[Fragment, End]]:
    """Adds native highlight shafts through the selected source's depth gaps.

    Args:
        shell: Entering light, frame measures, source, highlight, and ray controls.

    Returns:
        Associated light with generated shafts, or native construction faults.
    """
    _, highlights_field, length, jitter, color = LightRays.__struct_fields__
    threshold, smoothness = Highlights.__struct_fields__
    highlights = keyed("ray_key", shell.source, shell.socket(highlights_field, threshold), shell.socket(highlights_field, smoothness))

    def built(source: tuple[tuple[Fragment, End], tuple[Fragment, End], Option[tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
        position, front, occlusion = source

        def unoccluded(occluded: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
            return paired(highlights, math("ray_unoccluded", "SUBTRACT", 1.0, occluded)).bind(lambda parts: vector("ray_visible_key", "SCALE", *parts))

        coordinates = framed("ray_frame", shell.source, shell.aspect, shell.scale).bind(
            lambda frame: paired(
                sampled("ray_position", frame, position),
                paired(
                    node("ray_dimensions", bpy.types.CompositorNodeImageInfo, {"Image": shell.source}, {}).bind(
                        lambda dimensions: vector("ray_diagonal", "LENGTH", ended("ray_dimensions", "Dimensions")(dimensions))
                    ),
                    math("ray_camera_length", "MULTIPLY", shell.socket(length), frame.greater),
                ).bind(lambda measures: math("ray_size", "DIVIDE", measures[1], measures[0])),
            )
        )
        return (
            paired(occlusion.map(unoccluded).default_value(highlights), coordinates)
            .bind(
                lambda parts: node(
                    "rays",
                    bpy.types.CompositorNodeGlare,
                    {
                        "Image": parts[0],
                        "Type": "Sun Beams",
                        "Quality": "High",
                        "Highlights Threshold": 0.0,
                        "Highlights Smoothness": 0.0,
                        "Clamp Highlights": False,
                        "Sun Position": parts[1][0],
                        "Size": parts[1][1],
                        "Jitter": shell.socket(jitter),
                    },
                    {},
                )
            )
            .map(ended("rays", "Glare"))
            .bind(lambda rays: vector("ray_tint", "MULTIPLY", rays, shell.socket(color)))
            .bind(lambda tint: vector("ray_front", "SCALE", tint, front))
            .bind(lambda light: mixed("result", "RGBA", 1.0, shell.source, light, "ADD"))
        )

    return located(shell).bind(built)


def flared(shell: Shell[LensFlare]) -> BlenderResult[tuple[Fragment, End]]:
    """Adds five source-centered flare sections with finite-disc visibility.

    Args:
        shell: Entering light, frame, projected source, and section controls.

    Returns:
        Entering light plus the visible section sum, or native graph faults.
    """
    inlet = shell.socket
    _, radius_field, hotspot_field, starburst, anamorphic, halo_field, ghosts_field, seed = LensFlare.__struct_fields__
    _, value = Gated.__struct_fields__
    hotspot = hotspot_field, value
    star = starburst, value
    bar = anamorphic, value
    halo = halo_field, value
    ghosts = ghosts_field, value

    def built(frame: FrameSpace, source: tuple[tuple[Fragment, End], tuple[Fragment, End], Option[tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
        emission_hotspot, softness_hotspot = Hotspot.__struct_fields__
        emission_starburst, basis, length_starburst, density, sharpness, motion = Starburst.__struct_fields__
        emission_anamorphic, length_anamorphic, thickness, angle_anamorphic = Anamorphic.__struct_fields__
        emission_halo, magnification_halo, radius_halo, width, softness_halo = Halo.__struct_fields__
        emission_ghosts, iris, _, squeeze, fringe_ghosts, _, first, last, radius_ghosts, variation, softness_ghosts = Ghosts.__struct_fields__
        speed, hold = Motion.__struct_fields__
        position, front, occlusion = source
        source_point = vector("flare_source", "MULTIPLY", position, frame.extent)
        center = vector("flare_center", "SCALE", frame.extent, 0.5)
        axis = paired(source_point, center).bind(lambda parts: vector("flare_axis", "SUBTRACT", *parts))
        offset = source_point.bind(lambda source: vector("flare_offset", "SUBTRACT", frame.point, source))
        rho = offset.bind(lambda offset: vector("flare_rho", "LENGTH", offset))
        pixel = math("flare_pixel", "DIVIDE", 1.0, frame.greater)
        radius = inlet(radius_field)
        diameter = math("source_diameter", "MULTIPLY", radius, 2.0)
        disc_edge = rho.bind(lambda rho: math("source_edge", "SUBTRACT", rho, radius))

        def visible(occluded: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
            disc = paired(paired(disc_edge, diameter), pixel).bind(lambda parts: coverage("source_disc", parts[0][0], Some(parts[0][1]), 0.0, parts[1]))
            obscured = disc.bind(lambda disc: math("source_obscured", "MULTIPLY", disc, occluded))
            return (
                paired(
                    obscured.bind(lambda obscured: node("occlusion_mean", bpy.types.CompositorNodeLevels, {"Image": obscured, "Channel": "Red"}, {})).map(ended("occlusion_mean", "Mean")),
                    disc.bind(lambda disc: node("disc_mean", bpy.types.CompositorNodeLevels, {"Image": disc, "Channel": "Red"}, {})).map(ended("disc_mean", "Mean")),
                )
                .bind(lambda means: math("occluded_share", "DIVIDE", *means))
                .bind(lambda share: math("visibility", "SUBTRACT", 1.0, share))
                .bind(lambda visibility: math("flare_visible", "MULTIPLY", front, visibility))
            )

        hotspot_weight = paired(paired(disc_edge, diameter), pixel).bind(lambda parts: coverage("hotspot_coverage", parts[0][0], Some(parts[0][1]), inlet(*hotspot, softness_hotspot), parts[1]))
        hotspot_tone = rho.bind(lambda rho: math("hotspot_ratio", "DIVIDE", rho, radius)).bind(lambda tone: math("hotspot_tone", "MINIMUM", tone, 1.0))
        circle = (
            offset
            .bind(lambda offset: node("star_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": offset}, {}))
            .bind(lambda axes: math("star_angle", "ARCTAN2", ended("star_axes", "Y")(axes), ended("star_axes", "X")(axes)))
            .bind(lambda angle: paired(math("star_cosine", "COSINE", angle), math("star_sine", "SINE", angle)))
            .bind(lambda axes: node("star_direction", bpy.types.ShaderNodeCombineXYZ, {"X": axes[0], "Y": axes[1], "Z": 0.0}, {}))
            .map(ended("star_direction", "Vector"))
        )
        circle = paired(circle, math("star_circle_radius", "DIVIDE", inlet(*star, density), tau)).bind(lambda parts: vector("star_circle", "SCALE", *parts))
        clock = Clock(key="star_clock", speed=inlet(*star, motion, speed), hold=inlet(*star, motion, hold)).value
        streaks = (
            paired(circle, clock)
            .bind(lambda parts: noise.sampled(shell, (*star, basis), *parts, inlet(seed), streams=(_Noise.STAR,)))
            .bind(lambda fields: math("star_signed", "MULTIPLY_ADD", fields[0][0], 2.0, -1.0))
            .bind(lambda signed: math("star_positive", "MAXIMUM", signed, 0.0))
            .bind(lambda positive: math("star_sharp", "POWER", positive, inlet(*star, sharpness)))
        )
        star_ratio = rho.bind(lambda rho: math("star_ratio", "DIVIDE", rho, inlet(*star, length_starburst)))
        star_tone = star_ratio.bind(lambda tone: math("star_tone", "MINIMUM", tone, 1.0))
        star_falloff = (
            rho
            .bind(lambda rho: math("star_remaining", "SUBTRACT", inlet(*star, length_starburst), rho))
            .bind(lambda remaining: math("star_reach", "MAXIMUM", remaining, 0.0))
            .bind(lambda reach: math("star_fraction", "DIVIDE", reach, inlet(*star, length_starburst)))
            .bind(lambda fraction: math("star_falloff", "MULTIPLY", fraction, fraction))
        )
        star_weight = paired(streaks, star_falloff).bind(lambda parts: math("star_weight", "MULTIPLY", *parts))
        bar_axes = offset.bind(
            lambda offset: node("bar_rotation", bpy.types.ShaderNodeVectorRotate, {"Vector": offset, "Angle": inlet(*bar, angle_anamorphic)}, {"rotation_type": "Z_AXIS", "invert": True})
        ).bind(lambda rotated: node("bar_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": ended("bar_rotation", "Vector")(rotated)}, {}))
        bar_along = bar_axes.bind(lambda axes: math("bar_along", "ABSOLUTE", ended("bar_axes", "X")(axes)))
        bar_across = bar_axes.bind(lambda axes: math("bar_across", "ABSOLUTE", ended("bar_axes", "Y")(axes)))
        bar_ratio = bar_along.bind(lambda along: math("bar_ratio", "DIVIDE", along, inlet(*bar, length_anamorphic)))
        bar_tone = bar_ratio.bind(lambda tone: math("bar_tone", "MINIMUM", tone, 1.0))
        bar_falloff = (
            bar_along
            .bind(lambda along: math("bar_remaining", "SUBTRACT", inlet(*bar, length_anamorphic), along))
            .bind(lambda remaining: math("bar_reach", "MAXIMUM", remaining, 0.0))
            .bind(lambda reach: math("bar_fraction", "DIVIDE", reach, inlet(*bar, length_anamorphic)))
            .bind(lambda fraction: math("bar_falloff", "MULTIPLY", fraction, fraction))
        )
        bar_edge = bar_across.bind(lambda across: math("bar_edge", "MULTIPLY_ADD", inlet(*bar, thickness), -0.5, across))
        bar_weight = paired(paired(bar_edge, pixel).bind(lambda parts: coverage("bar_coverage", parts[0], Some(inlet(*bar, thickness)), 0.0, parts[1])), bar_falloff).bind(
            lambda parts: math("bar_weight", "MULTIPLY", *parts)
        )
        halo_center = paired(axis, center).bind(lambda parts: vector("halo_center", "MULTIPLY_ADD", parts[0], inlet(*halo, magnification_halo), parts[1]))
        halo_delta = (
            halo_center
            .bind(lambda center: vector("halo_offset", "SUBTRACT", frame.point, center))
            .bind(lambda offset: vector("halo_rho", "LENGTH", offset))
            .bind(lambda rho: math("halo_delta", "SUBTRACT", rho, inlet(*halo, radius_halo)))
        )
        halo_tone = halo_delta.bind(lambda delta: math("halo_ratio", "DIVIDE", delta, inlet(*halo, width))).bind(lambda ratio: math("halo_tone", "ADD", ratio, 0.5, clamp=True))
        halo_edge = halo_delta.bind(lambda delta: math("halo_absolute", "ABSOLUTE", delta)).bind(lambda delta: math("halo_edge", "MULTIPLY_ADD", inlet(*halo, width), -0.5, delta))
        halo_weight = paired(halo_edge, pixel).bind(lambda parts: coverage("halo_coverage", parts[0], Some(inlet(*halo, width)), inlet(*halo, softness_halo), parts[1]))
        profiles = (
            (hotspot, emission_hotspot, hotspot_weight, hotspot_tone),
            (star, emission_starburst, star_weight, star_tone),
            (bar, emission_anamorphic, bar_weight, bar_tone),
            (halo, emission_halo, halo_weight, halo_tone),
        )
        sections = accumulated(
            paired(weight, tone.bind(lambda tone: noise.emitted(shell, (*path, emission), (tone,))))
            .bind(lambda parts: vector(f"{path[0]}_light", "SCALE", parts[1][0], parts[0]))
            .bind(lambda light: switched(f"{path[0]}_enabled", "VECTOR", inlet(path[0]), (0.0, 0.0, 0.0), light))
            for path, emission, weight, tone in profiles
        )

        def ghost(index: int) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
            blades, rotation, roundness, obstruction = Aperture.__struct_fields__
            key = f"ghost_{index}"
            random = (
                origin(f"{key}_origin", inlet(seed), _Noise.GHOST, instance=index)
                .bind(lambda origin: vector(f"{key}_uniform", "SCALE", origin, 1.0 / 256.0))
                .bind(lambda uniform: node(f"{key}_attributes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": uniform}, {}))
            )
            shrink = random.bind(lambda values: math(f"{key}_variation", "MULTIPLY", inlet(*ghosts, variation), ended(f"{key}_attributes", "X")(values))).bind(
                lambda reduction: math(f"{key}_shrink", "SUBTRACT", 1.0, reduction)
            )
            slot = (
                random
                .bind(lambda values: math(f"{key}_jitter", "SUBTRACT", ended(f"{key}_attributes", "Y")(values), 0.5))
                .bind(lambda jitter: math(f"{key}_slot_offset", "MULTIPLY_ADD", jitter, inlet(*ghosts, variation), index + 0.5))
                .bind(lambda slot: math(f"{key}_slot", "DIVIDE", slot, shell.record.ghosts.value.count))
            )
            magnification = slot.bind(lambda slot: mixed(f"{key}_magnification", "FLOAT", slot, inlet(*ghosts, first), inlet(*ghosts, last)))
            location = paired(paired(axis, center), magnification).bind(lambda parts: vector(f"{key}_center", "MULTIPLY_ADD", parts[0][0], parts[1], parts[0][1]))
            flip = magnification.bind(lambda magnification: math(f"{key}_negative", "LESS_THAN", magnification, 0.0)).bind(lambda negative: math(f"{key}_flip", "MULTIPLY_ADD", negative, -2.0, 1.0))
            offset = location.bind(lambda location: vector(f"{key}_offset", "SUBTRACT", frame.point, location))
            mask_point = (
                paired(offset, flip)
                .bind(lambda parts: vector(f"{key}_flipped", "SCALE", *parts))
                .bind(lambda flipped: node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": flipped}, {}))
                .bind(
                    lambda axes: paired(
                        math(f"{key}_squeezed", "MULTIPLY", ended(f"{key}_axes", "X")(axes), inlet(*ghosts, squeeze)), math(f"{key}_reflected", "MULTIPLY", ended(f"{key}_axes", "Y")(axes), -1.0)
                    )
                )
                .bind(lambda axes: node(f"{key}_point", bpy.types.ShaderNodeCombineXYZ, {"X": axes[0], "Y": axes[1], "Z": 0.0}, {}))
                .map(ended(f"{key}_point", "Vector"))
            )
            radius = shrink.bind(lambda shrink: math(f"{key}_radius", "MULTIPLY", inlet(*ghosts, radius_ghosts), shrink))
            ramp = paired(radius.bind(lambda radius: math(f"{key}_softness", "MULTIPLY", inlet(*ghosts, softness_ghosts), radius)), pixel).bind(lambda parts: math(f"{key}_ramp", "ADD", *parts))
            fringe = inlet(*ghosts, fringe_ghosts)
            factors = (
                math(f"{key}_red_shift", "MAXIMUM", fringe, 0.0).bind(lambda shift: math(f"{key}_red_scale", "SUBTRACT", 1.0, shift)),
                math(f"{key}_green_shift", "ABSOLUTE", fringe).bind(lambda shift: math(f"{key}_green_scale", "MULTIPLY_ADD", shift, -0.5, 1.0)),
                math(f"{key}_blue_shift", "MINIMUM", fringe, 0.0).bind(lambda shift: math(f"{key}_blue_scale", "ADD", 1.0, shift)),
            )
            aperture = *ghosts, iris
            channels = (
                accumulated(
                    paired(
                        paired(mask_point, paired(radius, factor)).bind(
                            lambda parts: math(f"{key}_{channel}_radius", "MULTIPLY", *parts[1]).bind(
                                lambda radius: paired(
                                    outlined(
                                        f"{key}_{channel}_iris", parts[0], radius, inlet(*aperture, blades), inlet(*aperture, rotation), inlet(*aperture, roundness), inlet(*aperture, obstruction)
                                    ),
                                    math(f"{key}_{channel}_positive_radius", "GREATER_THAN", radius, 0.0),
                                )
                            )
                        ),
                        ramp,
                    ).bind(
                        lambda parts: (
                            paired(
                                math(f"{key}_{channel}_outer_ratio", "DIVIDE", parts[0][0][0], parts[1]).bind(lambda ratio: math(f"{key}_{channel}_outer", "SUBTRACT", 0.5, ratio, clamp=True)),
                                math(f"{key}_{channel}_inner_ratio", "DIVIDE", parts[0][0][1], parts[1])
                                .bind(lambda ratio: math(f"{key}_{channel}_inner", "ADD", 1.0, ratio, clamp=True))
                                .bind(
                                    lambda inner: math(f"{key}_{channel}_obstructed", "GREATER_THAN", inlet(*aperture, obstruction), 0.0).bind(
                                        lambda obstructed: switched(f"{key}_{channel}_hole", "FLOAT", obstructed, 1.0, inner)
                                    )
                                ),
                            )
                            .bind(lambda ramps: math(f"{key}_{channel}_coverage", "MINIMUM", *ramps))
                            .bind(lambda mask: switched(f"{key}_{channel}_nonempty", "FLOAT", parts[0][1], 0.0, mask))
                        )
                    )
                    for channel, factor in zip(("red", "green", "blue"), factors)
                )
                .bind(lambda channels: node(f"{key}_channels", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y", "Z"), channels)), {}))
                .map(ended(f"{key}_channels", "Vector"))
            )
            gain = shrink.bind(lambda shrink: math(f"{key}_area", "MULTIPLY", shrink, shrink)).bind(lambda area: math(f"{key}_gain", "DIVIDE", 1.0, area))
            masks = paired(math(f"{key}_open_iris", "LESS_THAN", inlet(*aperture, obstruction), 1.0), paired(channels, gain).bind(lambda parts: vector(f"{key}_mask", "SCALE", *parts))).bind(
                lambda parts: switched(f"{key}_nonempty", "VECTOR", parts[0], (0.0, 0.0, 0.0), parts[1])
            )
            return paired(masks, slot)

        ghost_fields = paired(ghost(0), accumulated(ghost(index) for index in range(1, shell.record.ghosts.value.count))).map(lambda parts: (parts[0], *parts[1]))
        axis_tone = (
            paired(axis, center)
            .bind(
                lambda parts: vector("ghost_axis_offset", "SUBTRACT", frame.point, parts[1]).bind(
                    lambda offset: paired(vector("ghost_axis_dot", "DOT_PRODUCT", offset, parts[0]), vector("ghost_axis_squared", "DOT_PRODUCT", parts[0], parts[0]))
                )
            )
            .bind(lambda parts: math("ghost_axis_magnification", "DIVIDE", *parts))
            .bind(lambda magnification: math("ghost_axis_start", "SUBTRACT", magnification, inlet(*ghosts, first)))
        )
        axis_tone = paired(axis_tone, math("ghost_axis_span", "SUBTRACT", inlet(*ghosts, last), inlet(*ghosts, first))).bind(lambda parts: math("ghost_axis_tone", "DIVIDE", *parts, clamp=True))

        def illuminated(parts: tuple[tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...], tuple[Fragment, End]]) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
            fields, axis_tone = parts
            (_, first_tone), *following = fields
            return noise.emitted(
                shell,
                (*ghosts, emission_ghosts),
                (
                    first_tone if shell.record.ghosts.value.grading is Grading.INSTANCE else axis_tone,
                    *(tone if shell.record.ghosts.value.grading is Grading.INSTANCE else axis_tone for _, tone in following),
                ),
            ).bind(lambda lights: accumulated(vector(f"ghost_{index}_light", "MULTIPLY", mask, light) for index, ((mask, _), light) in enumerate(zip(fields, lights))))

        ghost_light = (
            paired(ghost_fields, axis_tone)
            .bind(illuminated)
            .bind(lambda lights: summed("ghosts", lights, kind="VECTOR"))
            .bind(lambda light: switched("ghosts_enabled", "VECTOR", inlet(ghosts_field), (0.0, 0.0, 0.0), light))
        )
        light = paired(sections, ghost_light).bind(lambda parts: summed("flare_sections", (*parts[0], parts[1]), kind="VECTOR"))
        return (
            paired(light, occlusion.map(visible).default_value(Ok(front)))
            .bind(lambda parts: vector("flare_light", "SCALE", *parts))
            .bind(lambda light: mixed("result", "RGBA", 1.0, shell.source, light, "ADD"))
        )

    return paired(framed("flare_frame", shell.source, shell.aspect, shell.scale), located(shell)).bind(lambda parts: built(*parts))


def _source_admitted[R: SourceEffect](identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: R, _context: Context) -> BlenderResult[None]:
    match record.source:
        case FramePosition():
            return Ok(None)
        case BoundObject(reading=reading):
            present = entry.map(
                lambda entry: entry_input(identity, entry, item_key(BoundObject)).bind(
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
light_rays = CompositorEffect(
    record=LightRays, graph=beamed, inputs=_source_inputs, requested=Some(lambda record, layer: _requested(record, layer, precise=False)), native_admitted=Some(_source_admitted)
)
lens_flare = CompositorEffect(
    record=LensFlare, graph=flared, inputs=_source_inputs, requested=Some(lambda record, layer: _requested(record, layer, precise=True)), native_admitted=Some(_source_admitted)
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["beamed", "flared", "lens_flare", "light_rays", "located"]
