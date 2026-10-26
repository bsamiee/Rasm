"""Centered mirror folds, polar maps, and covered panorama rays."""

from collections.abc import Callable, Mapping
import functools
from math import pi
import typing
from typing import Literal, TypeIs

import bpy
from expression import Ok, Some
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Condition
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math, menu, Scalar, switched, Term, vector, xyz
from blender.bpy.nodes.interface import MenuItems
from blender.bpy.render.effects.coordinates import extended, FrameSpace, lensed, sampled, shorter
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Inspected, precise, Shell
from blender.bpy.scene.cameras import camera_type
from blender.imaging.effects.warp.projection import Kaleidoscope, Modes, PanoramaModes, Panoramic, Polar, PolarConversion, Projection, Sampling, Tiling
from blender.imaging.geometry.panoramas import CentralCylindrical, Equirectangular, Fisheye, FisheyeEquidistant, FisheyeEquisolid, MirrorBall, Panini, Panorama, Stereographic
from blender.imaging.parameters import Case

# --- [OPERATIONS] -----------------------------------------------------------------------


def _mode(case: Case) -> TypeIs[Modes]:
    """Whether a retained case is a projection mode."""
    return isinstance(case, typing.get_args(Modes.__value__))


def _panoramic(case: Case) -> TypeIs[PanoramaModes]:
    """Whether a retained case is a panorama the perspective frame maps onto."""
    return isinstance(case, typing.get_args(PanoramaModes.__value__))


def mapped(case: Kaleidoscope | Tiling | Polar | PanoramaModes, frame: FrameSpace, parameters: Mapping[str, Expr], relative: Expr, lens: Expr) -> Expr:
    """Builds one mode's source positions from its field inputs.

    Args:
        case: Projection case selecting declared map.
        frame: Output positions in the displayed camera frame.
        parameters: Group inputs keyed by case field names.
        relative: Panorama-to-camera rotation without object scale.
        lens: Camera projection matrix onto processed image.

    Returns:
        Whole-image sampling positions with geometric coverage in Z.
    """
    view = (0.0, 0.0, -1.0)
    sides, centered = frame.fraction, vector("MULTIPLY_ADD", frame.extent, (-0.5, -0.5, 0.0), frame.point)

    def rotated(direction: Term, angle: Scalar, kind: Literal["AXIS_ANGLE", "X_AXIS", "Y_AXIS", "Z_AXIS"], *axis: Term) -> Expr:
        inputs = (("Vector", direction), *(("Axis", held) for held in axis), ("Angle", angle))
        return Expr(kind=bpy.types.ShaderNodeVectorRotate, inputs=inputs, members=(("rotation_type", kind),), socket="Vector", shape="VECTOR")

    def projected(ray: Expr, coverage: Scalar) -> Expr:
        turned = (
            Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", ray), ("Transform", relative)), socket="Direction", shape="VECTOR")
            if isinstance(case, Panorama) and case.upright
            else ray
        )
        device = Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", turned), ("Transform", lens)), socket="Vector", shape="VECTOR")
        return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), (turned["Z"] < 0.0) * coverage, vector("MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)))

    def fisheye(planar: Expr, measured: Callable[[Expr], tuple[Expr, Scalar]]) -> Expr:
        angle, coverage = measured(vector("LENGTH", planar))
        return projected(rotated(view, angle, "AXIS_ANGLE", vector("CROSS_PRODUCT", view, planar)), coverage)

    match case:
        case Polar():
            _, conversion = Polar.__struct_fields__
            short = shorter(frame.extent)
            column = math("WRAP", math("ARCTAN2", centered["X"], centered["Y"]), 2.0 * pi, 0.0) / (2.0 * pi)
            turn = sides["X"] * (2.0 * pi)
            disc = vector("SCALE", xyz(math("SINE", turn), math("COSINE", turn)), (1.0 - sides["Y"]) * short * 0.5)
            return menu(
                parameters[conversion],
                MenuItems.of(PolarConversion),
                {
                    PolarConversion.RECTANGULAR_TO_POLAR.label: sampled(frame, xyz(column, math("MULTIPLY_ADD", vector("LENGTH", centered) / short, -2.0, 1.0))),
                    PolarConversion.POLAR_TO_RECTANGULAR.label: sampled(frame, disc / frame.extent + (0.5, 0.5, 0.0)),
                },
                kind="VECTOR",
            )
        case Tiling():
            _, pivot, count = Tiling.__struct_fields__
            return sampled(frame, vector("SCALE", frame.fraction - parameters[pivot], parameters[count]) + parameters[pivot])
        case Kaleidoscope():
            _, center_field, mirrors, angle, zoom = Kaleidoscope.__struct_fields__
            center = vector("MULTIPLY", parameters[center_field], frame.extent)
            local = frame.point - center
            bearing = math("ARCTAN2", local["Y"], local["X"]) - parameters[angle]
            wedge = pi / parameters[mirrors]
            folded = rotated(local, abs(math("WRAP", bearing, wedge, -wedge)) - bearing, "Z_AXIS")
            return sampled(frame, (folded / parameters[zoom] + center) / frame.extent)
        case Equirectangular():
            latitude = rotated(view, math("MULTIPLY_ADD", sides["Y"], pi, -pi / 2.0), "X_AXIS")
            return projected(rotated(latitude, math("MULTIPLY_ADD", sides["X"], -2.0 * pi, pi), "Y_AXIS"), 1.0)
        case CentralCylindrical():
            low, high = CentralCylindrical.__struct_fields__
            height = xyz(0.0, math("MULTIPLY_ADD", sides["Y"], parameters[high] - parameters[low], parameters[low]), -1.0)
            return projected(rotated(height, math("MULTIPLY_ADD", sides["X"], -2.0 * pi, pi), "Y_AXIS"), 1.0)
        case MirrorBall():
            return fisheye(vector("MULTIPLY_ADD", frame.fraction, (2.0, 2.0, 0.0), (-1.0, -1.0, 0.0)), lambda radius: (math("ARCSINE", radius) * 2.0, 1.0 - (radius > 1.0)))
        case FisheyeEquidistant():
            (field_of_view,) = Fisheye.__struct_fields__
            return fisheye(vector("MULTIPLY_ADD", frame.fraction, (2.0, 2.0, 0.0), (-1.0, -1.0, 0.0)), lambda radius: (radius * parameters[field_of_view] * 0.5, 1.0 - (radius > 1.0)))
        case FisheyeEquisolid():
            field_of_view, lens_field, sensor = FisheyeEquisolid.__struct_fields__
            diameter = parameters[lens_field] * 2.0
            return fisheye(
                vector("MULTIPLY", frame.fraction - (0.5, 0.5, 0.0), parameters[sensor]),
                lambda radius: (math("ARCSINE", radius / diameter) * 2.0, 1.0 - (radius > math("SINE", parameters[field_of_view] * 0.25) * diameter)),
            )
        case Stereographic():
            (field_of_view,) = Stereographic.__struct_fields__
            return fisheye(centered, lambda radius: (math("ARCTANGENT", radius * (math("TANGENT", parameters[field_of_view] * 0.25) * 2.0)) * 2.0, 1.0))
        case Panini():
            field_of_view, distance, compression = Panini.__struct_fields__
            total = parameters[distance] + 1.0
            perspective, cylindrical, half = 1.0 / total, parameters[distance] / total, parameters[field_of_view] * 0.5
            planar = vector("SCALE", centered, math("SINE", half) / math("MULTIPLY_ADD", perspective, math("COSINE", half), cylindrical) * 2.0)
            length = vector("LENGTH", xyz(planar["X"] * perspective, 1.0))
            divided = planar["X"] / length
            distant = divided * cylindrical
            rest = (1.0 - distant) * (1.0 + distant)
            root = math("SQRT", rest)
            cosine = root / length - divided * perspective * distant
            sine = (perspective * root + cylindrical / length) * divided
            vertical = cosine / math("MULTIPLY_ADD", 1.0 - parameters[compression], cosine, parameters[compression])
            height = math("MULTIPLY_ADD", cosine, perspective, cylindrical) * vertical * planar["Y"]
            return projected(xyz(sine, height, -cosine), (1.0 - (rest < 0.0)) * (cosine > 0.0))


def projected(shell: Shell[Projection]) -> BlenderResult[Expr]:
    """Builds every projection mode and samples covered map of selected mode.

    Args:
        shell: Stack inputs, case values, and menu metadata.

    Returns:
        Associated warped image of the selected mode.
    """
    (mode_field,) = Projection.__struct_fields__
    _, projection_field = Panoramic.__struct_fields__
    (extension,) = Sampling.__struct_fields__
    camera = Expr(kind=bpy.types.GeometryNodeInputActiveCamera, socket="Active Camera")
    attitude = Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", camera),), socket="Rotation")
    unturned = Expr(
        kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", Expr(kind=bpy.types.FunctionNodeCombineTransform, inputs=(("Rotation", attitude),), socket="Transform")),), socket="Matrix"
    )
    up = Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", (0.0, 0.0, 1.0)), ("Transform", unturned)), socket="Direction", shape="VECTOR")
    elevation = -math("ARCTAN2", -up["Z"], vector("LENGTH", xyz(up["X"], up["Y"])))
    rotation = Expr(kind=bpy.types.FunctionNodeEulerToRotation, inputs=(("Euler", xyz(elevation, 0.0, -math("ARCTAN2", up["X"], up["Y"]))),), socket="Rotation")
    relative = Expr(kind=bpy.types.FunctionNodeCombineTransform, inputs=(("Rotation", rotation),), socket="Transform")
    frame, lens = shell.space, lensed("Projection Matrix")

    def positions(case: Kaleidoscope | Tiling | Polar | PanoramaModes, *path: str | type[Case]) -> Expr:
        return mapped(case, frame, {field.name: shell.input(*path, type(case), field.name) for field in msgspec.structs.fields(case)}, relative, lens)

    def arm(case: Modes) -> tuple[str, Expr, Expr]:
        match case:
            case Panoramic():
                at = (mode_field, Panoramic, projection_field)
                uv = shell.menu(*at, cases={view.title: positions(view, *at) for view in shell.cases[at] if _panoramic(view)}, kind="VECTOR")
            case Kaleidoscope() | Tiling() | Polar():
                uv = positions(case, mode_field)
        path = (mode_field, type(case), extension)
        border = (shell.input(*path), shell.items[path])
        return case.title, uv, extended(shell.color, uv, (border, border), clipped=True)

    arms = tuple(arm(case) for case in shell.cases[mode_field,] if _mode(case))
    coordinates = shell.menu(mode_field, cases={title: uv for title, uv, _ in arms}, kind="VECTOR").named("uv")

    def switch(held: Expr, step: tuple[str, Expr, Expr]) -> Expr:
        return switched(msgspec.structs.replace(coordinates, socket=step[0], shape="FLOAT"), held, step[2])

    first, *rest = arms
    return Ok(functools.reduce(switch, rest, switch(first[2], first)))


def conditioned(effect: Projection) -> Condition:
    """Requires perspective camera for panorama modes alone."""
    match effect.mode:
        case Kaleidoscope() | Tiling() | Polar():
            return AllOf(conditions=())
        case Panoramic():
            return camera_type("PERSP")


# --- [COMPOSITION] ----------------------------------------------------------------------

projection = CompositorEffect(
    record=Projection,
    graph=projected,
    alpha=Alpha.RESULT,
    condition=Some(conditioned),
    requested=Some(precise),
    inspected=(Inspected(key="projection_uv", end=End(node="uv", socket="Output"), description="Source positions and coverage of selected mode"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["conditioned", "mapped", "projected", "projection"]
