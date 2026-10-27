"""Mirror folds, tiling, polar conversions, and the perspective frame placed into a panorama, each map evaluated from the imaging projections over node expressions."""

import functools
from math import pi
from typing import Annotated, Final

import bpy
from expression import Some
import msgspec

from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.coordinates import Border, BORDERS, extended, lensed, sampled, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, precise, Requests, Shell
from blender.imaging.geometry.panoramas import CentralCylindrical, Equirectangular, FisheyeEquidistant, FisheyeEquisolid, folded, from_polar, MirrorBall, Panini, Stereographic, to_polar
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Panoramas = Equirectangular | CentralCylindrical | MirrorBall | FisheyeEquidistant | FisheyeEquisolid | Stereographic | Panini

# --- [CONSTANTS] ------------------------------------------------------------------------

KALEIDOSCOPE: Final = MenuItem(name="Kaleidoscope", description="Mirror lines and zoom about one center")
TILING: Final = MenuItem(name="Tiling", description="Frame copies scaled about a center")
POLAR: Final = MenuItem(name="Polar Coordinates", description="Full rectangular turn wrapped into the frame's inscribed disc or unwrapped from it")
PANORAMA: Final = MenuItem(name="Panorama", description="Perspective frame sampled into a panorama projection")
MODES: Final = MenuItems(items=(KALEIDOSCOPE, TILING, POLAR, PANORAMA))
TO_POLAR: Final = MenuItem(name="Rectangular to Polar")
TO_RECTANGULAR: Final = MenuItem(name="Polar to Rectangular")
CONVERSIONS: Final = MenuItems(items=(TO_POLAR, TO_RECTANGULAR))

# --- [MODELS] ---------------------------------------------------------------------------


class Projection(Effect, frozen=True, kw_only=True):
    """Mirror folds, tiling, polar conversions, or the rendered frame placed into a panorama."""

    stage = Stage.WARP
    pointwise = False
    mirrors: Annotated[int, msgspec.Meta(ge=1, description="Kaleidoscope mirror lines through center")] = 3
    angle: Annotated[float, msgspec.Meta(ge=-pi, le=pi, description="First kaleidoscope mirror line turn from horizontal")] = pi / 2.0
    zoom: Annotated[float, msgspec.Meta(ge=2.0**-15, le=2.0**15, description="Kaleidoscope magnification about fold center")] = 1.0
    panorama: Annotated[Panoramas, msgspec.Meta(description="Panorama receiving perspective frame")] = Equirectangular()


# --- [OPERATIONS] -----------------------------------------------------------------------


def projected(shell: Shell[Projection]) -> Outcome[Expr]:
    """Builds every mode's covered sample and keeps the selected mode's."""
    record, frame = shell.record, shell.space
    offset = vector("MULTIPLY_ADD", frame.extent, (-0.5, -0.5, 0.0), frame.point)
    short = shorter(frame.extent)
    unit = vector("ADD", frame.fraction, (0.0, 0.0, 1.0))["Z"]
    center = vector("MULTIPLY", shell.input("kaleidoscope_center"), frame.extent)
    local = frame.point - center
    x, y = folded(local["X"], local["Y"], mirrors=record.mirrors, angle=record.angle, zoom=record.zoom, ns=Expr)
    s, t, _ = to_polar((offset["X"] / short + 0.5, offset["Y"] / short + 0.5, unit), (1.0, 1.0), Expr)
    u, v, _ = from_polar((frame.fraction["X"], frame.fraction["Y"], unit), (1.0, 1.0), Expr)
    unwrapped = vector("MULTIPLY_ADD", vector("DIVIDE", xyz(u - 0.5, v - 0.5), frame.extent), xyz(short, short, 0.0), (0.5, 0.5, 0.0))
    across, along = (offset["X"], offset["Y"]) if record.panorama.centered else (frame.fraction["X"], frame.fraction["Y"])
    rx, ry, rz, coverage = record.panorama.to_ray((across, along, unit), Expr)

    def projected_ray(ray: Expr) -> Expr:
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
        turned = Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", ray), ("Transform", relative)), socket="Direction", shape="VECTOR") if record.panorama.upright else ray
        device = Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", turned), ("Transform", lensed("Projection Matrix"))), socket="Vector", shape="VECTOR")
        return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), (turned["Z"] < 0.0) * coverage, vector("MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)))

    def selected(polar: Expr) -> Outcome[Expr]:
        arms = (
            (KALEIDOSCOPE, "kaleidoscope", sampled(frame, vector("DIVIDE", xyz(x, y) + center, frame.extent))),
            (TILING, "tiling", sampled(frame, vector("SCALE", frame.fraction - shell.input("tiling_center"), shell.input("tiling_count")) + shell.input("tiling_center"))),
            (POLAR, "polar", polar),
            (PANORAMA, "panorama", projected_ray(xyz(rx, ry, rz))),
        )
        samples = tuple((item.name, extended(shell.color, uv, (shell.input(key, "extension"), shell.input(key, "extension")), clipped=True)) for item, key, uv in arms)

        def switch(held: Expr, step: tuple[str, Expr], coordinates: Expr) -> Expr:
            return switched(msgspec.structs.replace(coordinates, socket=step[0], shape="FLOAT"), held, step[1])

        return (
            shell
            .menu("mode", cases={item.name: uv for item, _, uv in arms}, kind="VECTOR")
            .map(lambda coordinates: coordinates.named("uv"))
            .map(lambda coordinates: functools.reduce(lambda held, step: switch(held, step, coordinates), samples[1:], switch(samples[0][1], samples[0], coordinates)))
        )

    return shell.menu("polar_conversion", cases={TO_POLAR.name: sampled(frame, xyz(s, t)), TO_RECTANGULAR.name: sampled(frame, unwrapped)}, kind="VECTOR").bind(selected)


# --- [COMPOSITION] ----------------------------------------------------------------------

projection = CompositorEffect(
    record=Projection,
    graph=projected,
    interface=Interface(
        inputs=(Socket(key="mode", name="Mode", kind=bpy.types.NodeSocketMenu, description="Frame mapping", members={"default_value": KALEIDOSCOPE.name}, items=Some(MODES)),),
        panels=tuple(
            InterfacePanel(
                key=prefix,
                name=item.name,
                description=item.description,
                inputs=(
                    Socket(
                        key=item_key(prefix, "extension"),
                        name="Extension",
                        kind=bpy.types.NodeSocketMenu,
                        description="Source sampling past frame edges",
                        members={"default_value": border.item.name},
                        items=Some(BORDERS),
                    ),
                    *extras,
                ),
            )
            for prefix, item, border, extras in (
                (
                    "kaleidoscope",
                    KALEIDOSCOPE,
                    Border.MIRROR,
                    (
                        Socket(
                            key="kaleidoscope_center",
                            name="Center",
                            kind=bpy.types.NodeSocketVector,
                            description="Point mirror lines pass through",
                            members={"dimensions": 2, "default_value": (0.5, 0.5)},
                        ),
                    ),
                ),
                (
                    "tiling",
                    TILING,
                    Border.MIRROR,
                    (
                        Socket(key="tiling_center", name="Center", kind=bpy.types.NodeSocketVector, description="Point copies scale about", members={"dimensions": 2, "default_value": (0.5, 0.5)}),
                        Socket(key="tiling_count", name="Count", kind=bpy.types.NodeSocketFloat, description="Frame copies across each side", members={"min_value": 1.0, "default_value": 3.0}),
                    ),
                ),
                (
                    "polar",
                    POLAR,
                    Border.BLACK,
                    (
                        Socket(
                            key="polar_conversion",
                            name="Conversion",
                            kind=bpy.types.NodeSocketMenu,
                            description="Conversion direction",
                            members={"default_value": TO_POLAR.name},
                            items=Some(CONVERSIONS),
                        ),
                    ),
                ),
                ("panorama", PANORAMA, Border.MIRROR, ()),
            )
        ),
    ),
    alpha=Alpha.RESULT,
    parts=(Requests[Projection](request=precise),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Panoramas", "Projection", "projection"]
