"""Mirror folds, tiling, polar conversions, and the perspective frame placed into a panorama, each map evaluated from the imaging projections over node expressions."""

import functools
from math import pi
import typing
from typing import Annotated, TypeIs

import bpy
from expression import Ok, Some
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Condition
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math, NODES, switched, vector, xyz
from blender.bpy.render.compositor.coordinates import Border, extended, FrameSpace, lensed, sampled, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Conditions, Inspects, precise, Requests, Shell
from blender.bpy.scene.cameras import camera_type
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.geometry.panoramas import CentralCylindrical, Equirectangular, FisheyeEquidistant, FisheyeEquisolid, folded, from_polar, MirrorBall, Panini, Stereographic, to_polar
from blender.imaging.geometry.regions import Position
from blender.records.fields import AtLeastOne, Case, Choice, Presentation, Turn

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class PolarConversion(Choice):
    """Directions between a rectangular strip and its inscribed polar disc."""

    RECTANGULAR_TO_POLAR = 1, "Rectangular to Polar"
    POLAR_TO_RECTANGULAR = 2, "Polar to Rectangular"


type PanoramaModes = Equirectangular | CentralCylindrical | MirrorBall | FisheyeEquidistant | FisheyeEquisolid | Stereographic | Panini
type Modes = Kaleidoscope | Tiling | Polar | Panoramic

# --- [MODELS] ---------------------------------------------------------------------------


class Kaleidoscope(Case, frozen=True, kw_only=True):
    """Mirror lines and zoom about one center."""

    extension: Annotated[Border, msgspec.Meta(description="Source sampling past frame edges")] = Border.MIRROR
    center: Annotated[Position, msgspec.Meta(description="Point mirror lines pass through")] = (0.5, 0.5)
    mirrors: Annotated[int, msgspec.Meta(ge=1, description="Mirror lines through center"), Presentation(soft=Some((1.0, 16.0))), GraphForm.SETTING] = 3
    angle: Annotated[Turn, msgspec.Meta(description="First mirror line turn from horizontal"), GraphForm.SETTING] = pi / 2.0
    zoom: Annotated[float, msgspec.Meta(ge=2.0**-15, le=2.0**15, description="Magnification about fold center"), Presentation(soft=Some((1.0 / 16.0, 16.0))), GraphForm.SETTING] = 1.0


class Tiling(Case, frozen=True, kw_only=True):
    """Frame copies scaled about a center."""

    extension: Annotated[Border, msgspec.Meta(description="Source sampling past frame edges")] = Border.MIRROR
    center: Annotated[Position, msgspec.Meta(description="Point copies scale about")] = (0.5, 0.5)
    count: Annotated[AtLeastOne, msgspec.Meta(description="Frame copies across each side"), Presentation(soft=Some((1.0, 8.0)))] = 3.0


class Polar(Case, frozen=True, kw_only=True):
    """Full rectangular turn wrapped into the frame's inscribed disc or unwrapped from it."""

    title = "Polar Coordinates"
    extension: Annotated[Border, msgspec.Meta(description="Source sampling past frame edges")] = Border.BLACK
    conversion: Annotated[PolarConversion, msgspec.Meta(description="Conversion direction")] = PolarConversion.RECTANGULAR_TO_POLAR


class Panoramic(Case, frozen=True, kw_only=True):
    """Perspective frame sampled into a panorama projection."""

    title = "Panorama"
    extension: Annotated[Border, msgspec.Meta(description="Source sampling past frame edges")] = Border.MIRROR
    projection: Annotated[PanoramaModes, msgspec.Meta(description="Panorama receiving perspective frame"), GraphForm.SETTING] = Equirectangular()


class Projection(Effect, frozen=True, kw_only=True):
    """Mirror folds, tiling, polar conversions, or the rendered frame placed into a panorama."""

    stage = Stage.WARP
    pointwise = False
    mode: Annotated[Modes, msgspec.Meta(description="Frame mapping")] = Kaleidoscope()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _mode(case: Case) -> TypeIs[Modes]:
    """Whether a retained case is a projection mode."""
    return isinstance(case, typing.get_args(Modes.__value__))


def mapped(shell: Shell[Projection], case: Modes, frame: FrameSpace) -> Expr:
    """Builds one mode's Map UV source positions with geometric coverage in Z.

    Args:
        shell: Mode inputs and menus.
        case: Mode whose graph-held parameters the map reads.
        frame: Output positions in the displayed camera frame.

    Returns:
        Whole-image sampling positions with coverage in Z.
    """
    (mode_field,) = Projection.__struct_fields__
    path = (mode_field, type(case))
    offset = vector("MULTIPLY_ADD", frame.extent, (-0.5, -0.5, 0.0), frame.point)
    short = shorter(frame.extent)
    unit = vector("ADD", frame.fraction, (0.0, 0.0, 1.0))["Z"]

    def inlet(name: str) -> Expr:
        return shell.input(*path, name)

    match case:
        case Kaleidoscope(mirrors=mirrors, angle=angle, zoom=zoom):
            center = vector("MULTIPLY", inlet("center"), frame.extent)
            local = frame.point - center
            x, y = folded(local["X"], local["Y"], mirrors=mirrors, angle=angle, zoom=zoom, ns=NODES)
            return sampled(frame, vector("DIVIDE", xyz(x, y) + center, frame.extent))
        case Tiling():
            return sampled(frame, vector("SCALE", frame.fraction - inlet("center"), inlet("count")) + inlet("center"))
        case Polar():
            s, t, _ = to_polar((offset["X"] / short + 0.5, offset["Y"] / short + 0.5, unit), (1.0, 1.0), NODES)
            u, v, _ = from_polar((frame.fraction["X"], frame.fraction["Y"], unit), (1.0, 1.0), NODES)
            unwrapped = vector("MULTIPLY_ADD", vector("DIVIDE", xyz(u - 0.5, v - 0.5), frame.extent), xyz(short, short, 0.0), (0.5, 0.5, 0.0))
            conversions = {PolarConversion.RECTANGULAR_TO_POLAR.label: sampled(frame, xyz(s, t)), PolarConversion.POLAR_TO_RECTANGULAR.label: sampled(frame, unwrapped)}
            return shell.menu(*path, "conversion", cases=conversions, kind="VECTOR")
        case Panoramic(projection=projection):
            s, t = (offset["X"], offset["Y"]) if projection.centered else (frame.fraction["X"], frame.fraction["Y"])
            x, y, z, coverage = projection.to_ray((s, t, unit), NODES)
            return _projected(xyz(x, y, z), coverage, upright=projection.upright)


def _projected(ray: Expr, coverage: Expr | float, *, upright: bool) -> Expr:
    """Projects a camera-space ray through the scene camera, leveled first for upright panoramas, coverage kept ahead of the camera."""
    camera = Expr(kind=bpy.types.GeometryNodeInputActiveCamera, socket="Active Camera")
    attitude = Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", camera),), socket="Rotation")
    unturned = Expr(
        kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", Expr(kind=bpy.types.FunctionNodeCombineTransform, inputs=(("Rotation", attitude),), socket="Transform")),), socket="Matrix"
    )
    up = Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", (0.0, 0.0, 1.0)), ("Transform", unturned)), socket="Direction", shape="VECTOR")
    level = Expr(
        kind=bpy.types.FunctionNodeEulerToRotation,
        inputs=(("Euler", xyz(-math("ARCTAN2", -up["Z"], vector("LENGTH", xyz(up["X"], up["Y"]))), 0.0, -math("ARCTAN2", up["X"], up["Y"]))),),
        socket="Rotation",
    )
    relative = Expr(kind=bpy.types.FunctionNodeCombineTransform, inputs=(("Rotation", level),), socket="Transform")
    turned = Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", ray), ("Transform", relative)), socket="Direction", shape="VECTOR") if upright else ray
    device = Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", turned), ("Transform", lensed("Projection Matrix"))), socket="Vector", shape="VECTOR")
    return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), (turned["Z"] < 0.0) * coverage, vector("MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)))


def projected(shell: Shell[Projection]) -> BlenderResult[Expr]:
    """Builds every mode's covered sample and keeps the selected mode's."""
    (mode_field,) = Projection.__struct_fields__

    def arm(case: Modes) -> tuple[str, Expr, Expr]:
        path = (mode_field, type(case))
        uv = mapped(shell, case, shell.space)
        border = shell.input(*path, "extension")
        return case.title, uv, extended(shell.color, uv, (border, border), clipped=True)

    arms = tuple(arm(case) for case in shell.cases[mode_field,] if _mode(case))
    coordinates = shell.menu(mode_field, cases={title: uv for title, uv, _ in arms}, kind="VECTOR").named("uv")

    def switch(held: Expr, step: tuple[str, Expr, Expr]) -> Expr:
        return switched(msgspec.structs.replace(coordinates, socket=step[0], shape="FLOAT"), held, step[2])

    first, *rest = arms
    return Ok(functools.reduce(switch, rest, switch(first[2], first)))


def conditioned(record: Projection) -> Condition:
    """Requires a perspective camera for panorama modes alone."""
    match record.mode:
        case Kaleidoscope() | Tiling() | Polar():
            return AllOf(conditions=())
        case Panoramic():
            return camera_type("PERSP")


# --- [COMPOSITION] ----------------------------------------------------------------------

projection = CompositorEffect(
    record=Projection,
    graph=projected,
    alpha=Alpha.RESULT,
    parts=(
        Requests[Projection](request=precise),
        Conditions[Projection](condition=conditioned),
        Inspects(key="projection_uv", end=End(node="uv", socket="Output"), description="Source positions and coverage of selected mode"),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Kaleidoscope", "Modes", "PanoramaModes", "Panoramic", "Polar", "PolarConversion", "Projection", "Tiling", "projection"]
