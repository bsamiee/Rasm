# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
"""Centered mirror folds, polar maps, and covered panorama rays through native nodes."""

from collections.abc import Mapping
from math import pi
from typing import get_args

import bpy
from expression import Ok, Some
import msgspec

from rasm.blender.data.rna.conditions import AllOf, Condition
from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import compared, End, ended, Fragment, framed, FrameSpace, item_key, math, menu, MenuItems, node, Operand, sampled, selected, vector
from rasm.blender.render.effects.stack import CompositorEffect, Coverage, precise, Shell
from rasm.blender.render.effects.viewers import inspection
from rasm.blender.render.effects.warp.transforms import extended
from rasm.blender.scene.cameras import camera_type
from rasm.imaging.effects.warp.projection import Kaleidoscope, Modes, PanoramaModes, Panoramic, Polar, PolarConversion, Projection, Sampling, Tiling
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.geometry.panoramas import CentralCylindrical, Equirectangular, FisheyeEquidistant, FisheyeEquisolid, MirrorBall, Panini, Panorama, Stereographic

# --- [OPERATIONS] -----------------------------------------------------------------------


def mapped(
    key: Key, mode: Kaleidoscope | Tiling | Polar | PanoramaModes, frame: FrameSpace, parameters: Mapping[str, Operand], relative: Operand, lens: Operand
) -> BlenderResult[tuple[Fragment, End]]:
    """Builds one mode's source positions from its native field operands.

    Args:
        key: Prefix of the mapping nodes.
        mode: Projection case selecting the declared mathematical map.
        frame: Output positions in the displayed camera frame.
        parameters: Operands keyed by the case's native semantic field names.
        relative: Panorama-to-camera rotation without object scale.
        lens: Actual camera projection matrix onto the processed image.

    Returns:
        Whole-image sampling positions with geometric coverage in Z.
    """
    view = (0.0, 0.0, -1.0)

    def rotated(planar: Operand, angle: Operand) -> BlenderResult[tuple[Fragment, End]]:
        return (
            vector(f"{key}_axis", "CROSS_PRODUCT", view, planar)
            .bind(lambda axis: node(f"{key}_ray", bpy.types.ShaderNodeVectorRotate, {"Vector": view, "Axis": axis, "Angle": angle}, {"rotation_type": "AXIS_ANGLE"}))
            .map(ended(f"{key}_ray", "Vector"))
        )

    def projected(ray: tuple[Fragment, End], coverage: Operand) -> BlenderResult[tuple[Fragment, End]]:
        turned = (
            node(f"{key}_upright", bpy.types.FunctionNodeTransformDirection, {"Direction": ray, "Transform": relative}, {}).map(ended(f"{key}_upright", "Direction"))
            if isinstance(mode, Panorama) and mode.upright
            else Ok(ray)
        )
        front = (
            turned
            .bind(lambda direction: node(f"{key}_depth", bpy.types.ShaderNodeSeparateXYZ, {"Vector": direction}, {}))
            .bind(lambda depth: math(f"{key}_front", "LESS_THAN", ended(f"{key}_depth", "Z")(depth), 0.0))
            .bind(lambda front: math(f"{key}_coverage", "MULTIPLY", front, coverage))
        )
        position = (
            turned
            .bind(lambda direction: node(f"{key}_projected", bpy.types.FunctionNodeProjectPoint, {"Vector": direction, "Transform": lens}, {}))
            .map(ended(f"{key}_projected", "Vector"))
            .bind(lambda device: vector(f"{key}_position", "MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)))
        )
        return paired(position, front).bind(lambda parts: vector(key, "MULTIPLY_ADD", (0.0, 0.0, 1.0), parts[1], parts[0]))

    sides = node(f"{key}_sides", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.fraction}, {})
    centered = vector(f"{key}_centered", "MULTIPLY_ADD", frame.extent, (-0.5, -0.5, 0.0), frame.point)
    match mode:
        case Polar():
            short = node(f"{key}_extent", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {}).bind(
                lambda extent: math(f"{key}_short", "MINIMUM", ended(f"{key}_extent", "X")(extent), End(node=f"{key}_extent", socket="Y"))
            )
            axes = centered.bind(lambda local: node(f"{key}_local", bpy.types.ShaderNodeSeparateXYZ, {"Vector": local}, {}))
            bearing = (
                axes
                .bind(lambda axes: math(f"{key}_bearing", "ARCTAN2", ended(f"{key}_local", "X")(axes), End(node=f"{key}_local", socket="Y")))
                .bind(lambda angle: math(f"{key}_wrapped", "WRAP", angle, 2.0 * pi, 0.0))
                .bind(lambda angle: math(f"{key}_column", "DIVIDE", angle, 2.0 * pi))
            )
            radial = (
                paired(centered.bind(lambda local: vector(f"{key}_radius", "LENGTH", local)), short)
                .bind(lambda parts: math(f"{key}_relative_radius", "DIVIDE", *parts))
                .bind(lambda radius: math(f"{key}_row", "MULTIPLY_ADD", radius, -2.0, 1.0))
            )
            wrapped = (
                paired(bearing, radial)
                .bind(lambda parts: node(f"{key}_wrapped_fraction", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y"), parts)), {}))
                .map(ended(f"{key}_wrapped_fraction", "Vector"))
                .bind(lambda fraction: sampled(f"{key}_wrapped_uv", frame, fraction))
            )
            turn = sides.bind(lambda sides: math(f"{key}_turn", "MULTIPLY", ended(f"{key}_sides", "X")(sides), 2.0 * pi))
            direction = (
                paired(turn.bind(lambda turn: math(f"{key}_sine", "SINE", turn)), turn.bind(lambda turn: math(f"{key}_cosine", "COSINE", turn)))
                .bind(lambda parts: node(f"{key}_direction", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y"), parts)), {}))
                .map(ended(f"{key}_direction", "Vector"))
            )
            reach = (
                sides
                .bind(lambda sides: math(f"{key}_depth", "SUBTRACT", 1.0, ended(f"{key}_sides", "Y")(sides)))
                .bind(lambda depth: short.bind(lambda short: math(f"{key}_reach", "MULTIPLY", depth, short)))
                .bind(lambda reach: math(f"{key}_half_reach", "MULTIPLY", reach, 0.5))
            )
            unwrapped = (
                paired(direction, reach)
                .bind(lambda parts: vector(f"{key}_disc", "SCALE", *parts))
                .bind(lambda local: vector(f"{key}_relative_disc", "DIVIDE", local, frame.extent))
                .bind(lambda local: vector(f"{key}_disc_fraction", "ADD", local, (0.5, 0.5, 0.0)))
                .bind(lambda fraction: sampled(f"{key}_unwrapped_uv", frame, fraction))
            )
            return paired(wrapped, unwrapped).bind(
                lambda parts: menu(
                    key,
                    "VECTOR",
                    parameters[Polar.conversion.__name__],
                    MenuItems.of(msgspec.inspect.type_info(PolarConversion)),
                    {PolarConversion.RECTANGULAR_TO_POLAR.label: parts[0], PolarConversion.POLAR_TO_RECTANGULAR.label: parts[1]},
                )
            )
        case Tiling():
            return (
                vector(f"{key}_local", "SUBTRACT", frame.fraction, parameters[Tiling.center.__name__])
                .bind(lambda local: vector(f"{key}_scaled", "SCALE", local, parameters[Tiling.count.__name__]))
                .bind(lambda local: vector(f"{key}_fraction", "ADD", local, parameters[Tiling.center.__name__]))
                .bind(lambda fraction: sampled(key, frame, fraction))
            )
        case Kaleidoscope():
            center = vector(f"{key}_center", "MULTIPLY", parameters[Kaleidoscope.center.__name__], frame.extent)
            local = center.bind(lambda center: vector(f"{key}_local", "SUBTRACT", frame.point, center))
            bearing = (
                local
                .bind(lambda local: node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": local}, {}))
                .bind(lambda axes: math(f"{key}_bearing", "ARCTAN2", ended(f"{key}_axes", "Y")(axes), End(node=f"{key}_axes", socket="X")))
                .bind(lambda bearing: math(f"{key}_relative", "SUBTRACT", bearing, parameters[Kaleidoscope.angle.__name__]))
            )
            wedge = math(f"{key}_wedge", "DIVIDE", pi, parameters[Kaleidoscope.mirrors.__name__])
            folded = (
                paired(bearing, wedge)
                .bind(lambda parts: math(f"{key}_negative_wedge", "MULTIPLY", parts[1], -1.0).bind(lambda negative: math(f"{key}_wrapped", "WRAP", parts[0], parts[1], negative)))
                .bind(lambda wrapped: math(f"{key}_reflected", "ABSOLUTE", wrapped))
                .bind(lambda reflected: math(f"{key}_turn", "SUBTRACT", reflected, End(node=f"{key}_relative")))
            )
            return (
                paired(local, folded)
                .bind(lambda parts: node(f"{key}_rotated", bpy.types.ShaderNodeVectorRotate, dict(zip(("Vector", "Angle"), parts)), {"rotation_type": "Z_AXIS"}))
                .map(ended(f"{key}_rotated", "Vector"))
                .bind(lambda folded: vector(f"{key}_zoomed", "DIVIDE", folded, parameters[Kaleidoscope.zoom.__name__]))
                .bind(lambda zoomed: vector(f"{key}_placed", "ADD", zoomed, End(node=f"{key}_center", socket="Vector")))
                .bind(lambda placed: vector(f"{key}_fraction", "DIVIDE", placed, frame.extent))
                .bind(lambda fraction: sampled(key, frame, fraction))
            )

        case Equirectangular() | CentralCylindrical():
            longitude = sides.bind(lambda sides: math(f"{key}_longitude", "MULTIPLY_ADD", ended(f"{key}_sides", "X")(sides), -2.0 * pi, pi))
            if isinstance(mode, Equirectangular):
                latitude = sides.bind(lambda sides: math(f"{key}_latitude", "MULTIPLY_ADD", ended(f"{key}_sides", "Y")(sides), pi, -pi / 2.0))
                ray = latitude.bind(lambda latitude: node(f"{key}_latitude_ray", bpy.types.ShaderNodeVectorRotate, {"Vector": view, "Angle": latitude}, {"rotation_type": "X_AXIS"})).map(
                    ended(f"{key}_latitude_ray", "Vector")
                )
            else:
                height = math(f"{key}_height_span", "SUBTRACT", parameters[CentralCylindrical.height_max.__name__], parameters[CentralCylindrical.height_min.__name__]).bind(
                    lambda span: sides.bind(lambda sides: math(f"{key}_height", "MULTIPLY_ADD", ended(f"{key}_sides", "Y")(sides), span, parameters[CentralCylindrical.height_min.__name__]))
                )
                ray = height.bind(lambda height: node(f"{key}_height_ray", bpy.types.ShaderNodeCombineXYZ, {"Y": height, "Z": -1.0}, {})).map(ended(f"{key}_height_ray", "Vector"))
            return (
                paired(ray, longitude)
                .bind(lambda parts: node(f"{key}_longitude_ray", bpy.types.ShaderNodeVectorRotate, dict(zip(("Vector", "Angle"), parts)), {"rotation_type": "Y_AXIS"}))
                .map(ended(f"{key}_longitude_ray", "Vector"))
                .bind(lambda ray: projected(ray, 1.0))
            )
        case MirrorBall() | FisheyeEquidistant() | FisheyeEquisolid() | Stereographic():
            match mode:
                case MirrorBall() | FisheyeEquidistant():
                    planar = vector(f"{key}_disc", "MULTIPLY_ADD", frame.fraction, (2.0, 2.0, 0.0), (-1.0, -1.0, 0.0))
                case FisheyeEquisolid():
                    planar = vector(f"{key}_sensor_local", "SUBTRACT", frame.fraction, (0.5, 0.5, 0.0)).bind(
                        lambda local: vector(f"{key}_sensor", "MULTIPLY", local, parameters[FisheyeEquisolid.sensor.__name__])
                    )
                case Stereographic():
                    planar = centered
            radius = planar.bind(lambda planar: vector(f"{key}_radius", "LENGTH", planar))
            match mode:
                case MirrorBall():
                    angle = radius.bind(lambda radius: math(f"{key}_half_angle", "ARCSINE", radius)).bind(lambda half: math(f"{key}_angle", "MULTIPLY", half, 2.0))
                    coverage = radius.bind(lambda radius: compared(f"{key}_inside", "LESS_EQUAL", radius, 1.0))
                case FisheyeEquidistant():
                    angle = radius.bind(lambda radius: math(f"{key}_field", "MULTIPLY", radius, parameters[FisheyeEquidistant.field_of_view.__name__])).bind(
                        lambda field: math(f"{key}_angle", "MULTIPLY", field, 0.5)
                    )
                    coverage = radius.bind(lambda radius: compared(f"{key}_inside", "LESS_EQUAL", radius, 1.0))
                case FisheyeEquisolid():
                    diameter = math(f"{key}_diameter", "MULTIPLY", parameters[FisheyeEquisolid.lens.__name__], 2.0)
                    angle = (
                        paired(radius, diameter)
                        .bind(lambda parts: math(f"{key}_ratio", "DIVIDE", *parts))
                        .bind(lambda ratio: math(f"{key}_half_angle", "ARCSINE", ratio))
                        .bind(lambda half: math(f"{key}_angle", "MULTIPLY", half, 2.0))
                    )
                    reach = (
                        math(f"{key}_quarter_field", "MULTIPLY", parameters[FisheyeEquisolid.field_of_view.__name__], 0.25)
                        .bind(lambda quarter: math(f"{key}_sine", "SINE", quarter))
                        .bind(lambda sine: diameter.bind(lambda diameter: math(f"{key}_reach", "MULTIPLY", sine, diameter)))
                    )
                    coverage = paired(radius, reach).bind(lambda parts: compared(f"{key}_inside", "LESS_EQUAL", *parts))
                case Stereographic():
                    scale = (
                        math(f"{key}_quarter_field", "MULTIPLY", parameters[Stereographic.field_of_view.__name__], 0.25)
                        .bind(lambda quarter: math(f"{key}_tangent", "TANGENT", quarter))
                        .bind(lambda tangent: math(f"{key}_scale", "MULTIPLY", tangent, 2.0))
                    )
                    angle = (
                        paired(radius, scale)
                        .bind(lambda parts: math(f"{key}_scaled_radius", "MULTIPLY", *parts))
                        .bind(lambda radius: math(f"{key}_half_angle", "ARCTANGENT", radius))
                        .bind(lambda half: math(f"{key}_angle", "MULTIPLY", half, 2.0))
                    )
                    return paired(planar, angle).bind(lambda parts: rotated(*parts)).bind(lambda ray: projected(ray, 1.0))
            return paired(paired(planar, angle).bind(lambda parts: rotated(*parts)), coverage).bind(lambda parts: projected(*parts))
        case Panini():
            total = math(f"{key}_total", "ADD", parameters[Panini.distance.__name__], 1.0)
            perspective = total.bind(lambda total: math(f"{key}_perspective", "DIVIDE", 1.0, total))
            cylindrical = total.bind(lambda total: math(f"{key}_cylindrical", "DIVIDE", parameters[Panini.distance.__name__], total))
            half = math(f"{key}_half_field", "MULTIPLY", parameters[Panini.field_of_view.__name__], 0.5)
            span = (
                paired(paired(half.bind(lambda half: math(f"{key}_sine_field", "SINE", half)), half.bind(lambda half: math(f"{key}_cosine_field", "COSINE", half))), paired(perspective, cylindrical))
                .bind(
                    lambda parts: math(f"{key}_denominator", "MULTIPLY_ADD", parts[1][0], parts[0][1], parts[1][1]).bind(
                        lambda denominator: math(f"{key}_half_span", "DIVIDE", parts[0][0], denominator)
                    )
                )
                .bind(lambda span: math(f"{key}_span", "MULTIPLY", span, 2.0))
            )
            planar_axes = (
                paired(centered, span)
                .bind(lambda parts: vector(f"{key}_planar", "SCALE", *parts))
                .bind(lambda planar: node(f"{key}_planar_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": planar}, {}))
            )
            horizontal = planar_axes.map(ended(f"{key}_planar_axes", "X"))
            length = (
                paired(horizontal, perspective)
                .bind(lambda parts: math(f"{key}_perspective_x", "MULTIPLY", *parts))
                .bind(lambda x: node(f"{key}_length_axes", bpy.types.ShaderNodeCombineXYZ, {"X": x, "Y": 1.0}, {}))
                .bind(lambda axes: vector(f"{key}_length", "LENGTH", ended(f"{key}_length_axes", "Vector")(axes)))
            )
            divided = paired(horizontal, length).bind(lambda parts: math(f"{key}_divided", "DIVIDE", *parts))
            tangent = paired(divided, perspective).bind(lambda parts: math(f"{key}_tangent", "MULTIPLY", *parts))
            distant = paired(divided, cylindrical).bind(lambda parts: math(f"{key}_distant", "MULTIPLY", *parts))
            rest = paired(distant.bind(lambda distant: math(f"{key}_less", "SUBTRACT", 1.0, distant)), distant.bind(lambda distant: math(f"{key}_more", "ADD", 1.0, distant))).bind(
                lambda parts: math(f"{key}_rest", "MULTIPLY", *parts)
            )
            root = rest.bind(lambda rest: math(f"{key}_root", "SQRT", rest))
            cosine = paired(
                paired(root, length).bind(lambda parts: math(f"{key}_root_share", "DIVIDE", *parts)), paired(tangent, distant).bind(lambda parts: math(f"{key}_lean", "MULTIPLY", *parts))
            ).bind(lambda parts: math(f"{key}_cosine", "SUBTRACT", *parts))
            sine = (
                paired(
                    paired(perspective, root).bind(lambda parts: math(f"{key}_perspective_root", "MULTIPLY", *parts)),
                    paired(cylindrical, length).bind(lambda parts: math(f"{key}_cylindrical_share", "DIVIDE", *parts)),
                )
                .bind(lambda parts: math(f"{key}_sine_weight", "ADD", *parts))
                .bind(lambda weight: divided.bind(lambda divided: math(f"{key}_sine", "MULTIPLY", weight, divided)))
            )
            vertical = (
                math(f"{key}_uncompressed", "SUBTRACT", 1.0, parameters[Panini.compression.__name__])
                .bind(lambda weight: cosine.bind(lambda cosine: math(f"{key}_vertical_weight", "MULTIPLY_ADD", weight, cosine, parameters[Panini.compression.__name__])))
                .bind(lambda weight: cosine.bind(lambda cosine: math(f"{key}_vertical", "DIVIDE", cosine, weight)))
            )
            height = (
                paired(paired(cosine, perspective).bind(lambda parts: math(f"{key}_projection", "MULTIPLY_ADD", *parts, End(node=f"{key}_cylindrical"))), vertical)
                .bind(lambda parts: math(f"{key}_height_weight", "MULTIPLY", *parts))
                .bind(lambda weight: planar_axes.bind(lambda axes: math(f"{key}_height", "MULTIPLY", weight, ended(f"{key}_planar_axes", "Y")(axes))))
            )
            depth = cosine.bind(lambda cosine: math(f"{key}_depth_value", "MULTIPLY", cosine, -1.0))
            coverage = paired(rest.bind(lambda rest: compared(f"{key}_real", "GREATER_EQUAL", rest, 0.0)), cosine.bind(lambda cosine: compared(f"{key}_ahead", "GREATER_THAN", cosine, 0.0))).bind(
                lambda parts: math(f"{key}_inside", "MULTIPLY", *parts)
            )
            return paired(
                accumulated((sine, height, depth)).bind(lambda parts: node(f"{key}_ray", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y", "Z"), parts)), {})).map(ended(f"{key}_ray", "Vector")),
                coverage,
            ).bind(lambda parts: projected(*parts))


def projected(shell: Shell[Projection]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds every retained mode and samples the selected covered native map.

    Args:
        shell: Accepted stack inputs, native case values, and menu metadata.

    Returns:
        Associated warped image or accumulated graph and native case faults.
    """
    mode_path = (Projection.mode.__name__,)  # type: ignore[union-attr]
    camera = node("camera", bpy.types.GeometryNodeInputActiveCamera, {}, {}).map(ended("camera", "Active Camera"))
    lens = camera.bind(lambda camera: node("lens", bpy.types.GeometryNodeCameraInfo, {"Camera": camera}, {})).map(ended("lens", "Projection Matrix"))
    attitude = (
        camera
        .bind(lambda camera: node("attitude", bpy.types.GeometryNodeObjectInfo, {"Object": camera}, {}))
        .bind(lambda attitude: node("rotation", bpy.types.FunctionNodeCombineTransform, {"Rotation": ended("attitude", "Rotation")(attitude)}, {}))
        .bind(lambda rotation: node("inverse_rotation", bpy.types.FunctionNodeInvertMatrix, {"Matrix": ended("rotation", "Transform")(rotation)}, {}))
        .map(ended("inverse_rotation", "Matrix"))
    )
    up = attitude.bind(lambda attitude: node("up", bpy.types.FunctionNodeTransformDirection, {"Direction": (0.0, 0.0, 1.0), "Transform": attitude}, {})).bind(
        lambda up: node("up_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": ended("up", "Direction")(up)}, {})
    )
    roll = up.bind(lambda axes: math("roll", "ARCTAN2", ended("up_axes", "X")(axes), End(node="up_axes", socket="Y"))).bind(lambda roll: math("inverse_roll", "MULTIPLY", roll, -1.0))
    elevation = (
        up
        .bind(lambda axes: node("up_planar", bpy.types.ShaderNodeCombineXYZ, {"X": ended("up_axes", "X")(axes), "Y": End(node="up_axes", socket="Y")}, {}))
        .bind(lambda planar: vector("up_length", "LENGTH", ended("up_planar", "Vector")(planar)))
        .bind(lambda length: math("negative_back", "MULTIPLY", End(node="up_axes", socket="Z"), -1.0).bind(lambda back: math("elevation", "ARCTAN2", back, length)))
        .bind(lambda elevation: math("inverse_elevation", "MULTIPLY", elevation, -1.0))
    )
    relative = (
        paired(elevation, roll)
        .bind(lambda angles: node("panorama_euler", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Z"), angles)), {}))
        .bind(lambda euler: node("panorama_rotation", bpy.types.FunctionNodeEulerToRotation, {"Euler": ended("panorama_euler", "Vector")(euler)}, {}))
        .bind(lambda rotation: node("panorama_frame", bpy.types.FunctionNodeCombineTransform, {"Rotation": ended("panorama_rotation", "Rotation")(rotation)}, {}))
        .map(ended("panorama_frame", "Transform"))
    )

    def built(frame: FrameSpace) -> BlenderResult[tuple[Fragment, End]]:
        def arm(kind: type[Modes]) -> BlenderResult[tuple[str, tuple[Fragment, End], tuple[Fragment, End]]]:
            mode = next(case for case in shell.cases[mode_path] if isinstance(case, kind))
            if isinstance(mode, Panoramic):
                panorama_path = (*mode_path, Panoramic, Panoramic.projection.__name__)  # type: ignore[union-attr]

                def panorama(kind: type[PanoramaModes]) -> BlenderResult[tuple[str, tuple[Fragment, End]]]:
                    projection = next(case for case in shell.cases[panorama_path] if isinstance(case, kind))
                    return (
                        paired(relative, lens)
                        .bind(
                            lambda camera: mapped(
                                item_key(Panoramic, kind), projection, frame, {field.name: shell.socket(*panorama_path, kind, field.name) for field in msgspec.structs.fields(kind)}, *camera
                            )
                        )
                        .map(lambda result: (projection.title, result))
                    )

                positions = accumulated(panorama(kind) for kind in get_args(PanoramaModes)).bind(
                    lambda arms: menu("panorama_uv", "VECTOR", shell.socket(*panorama_path), shell.items[panorama_path], dict(arms))
                )
            else:
                positions = paired(relative, lens).bind(
                    lambda camera: mapped(item_key(kind), mode, frame, {field.name: shell.socket(*mode_path, kind, field.name) for field in msgspec.structs.fields(kind)}, *camera)
                )
            border_path = (*mode_path, kind, Sampling.extension.__name__)
            return positions.bind(lambda uv: extended(item_key(kind, "sample"), shell.source, uv, shell.socket(*border_path), shell.items[border_path]).map(lambda image: (mode.title, uv, image)))

        return (
            accumulated(arm(kind) for kind in get_args(Modes))
            .bind(
                lambda arms: selected(
                    "uv",
                    shell.socket(*mode_path),
                    shell.items[mode_path],
                    {"coordinates": ("VECTOR", {title: uv for title, uv, _ in arms}), "image": ("RGBA", {title: image for title, _, image in arms})},
                )
            )
            .map(lambda outputs: outputs["image"])
        )

    return framed("frame", shell.source, shell.aspect, shell.scale).bind(built)


def conditioned(effect: Projection) -> Condition:
    """Requires a perspective camera for panorama mappings and accepts camera-free folds."""
    match effect.mode:
        case Kaleidoscope() | Tiling() | Polar():
            return AllOf(conditions=())
        case Panoramic():
            return camera_type("PERSP")


# --- [COMPOSITION] ----------------------------------------------------------------------

projection: CompositorEffect[Projection] = CompositorEffect(
    record=Projection,
    graph=projected,
    coverage=Coverage.RESULT,
    condition=Some(conditioned),
    requested=Some(precise),
    registrables=(inspection(lambda: projection, "projection_uv", "Inspect Projection UV", "Shows the selected source positions and their coverage", lambda: End(node="uv", socket="Output")),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["conditioned", "mapped", "projected", "projection"]
