"""Scene camera node forms: Camera Info reads, points seen through the camera, lens tilts and re-aims, imaging lens and panorama maps over node expressions, the chief ray, the circle of confusion, and the iris on the native Bokeh Image node."""

from collections.abc import Callable, Mapping
from types import MappingProxyType
from typing import Annotated, Final, Self

import bpy
from expression import Some
import msgspec

from blender.bpy.extension.identity import Key
from blender.bpy.nodes.functions import EXPRESSIONS, grouped
from blender.bpy.nodes.graphs import equal, Expr, inverted, math, projected, Scalar, switched, Term, vector, xyz
from blender.bpy.render.compositor.coordinates import FrameSpace
from blender.bpy.render.compositor.shapes import rotated
from blender.imaging.geometry.arithmetic import Namespace
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.geometry.panoramas import ImagePoint
from blender.records.fields import member

# --- [CONSTANTS] ------------------------------------------------------------------------

_IRIS: Final = MappingProxyType({
    member(Aperture, lambda kind: kind.blades): "Flaps",
    member(Aperture, lambda kind: kind.rotation): "Angle",
    member(Aperture, lambda kind: kind.roundness): "Roundness",
    member(Aperture, lambda kind: kind.obstruction): "Catadioptric Size",
})

# --- [MODELS] ---------------------------------------------------------------------------


class Sighted(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Frame fractions of a point or direction seen through the scene camera, whether it lies ahead of the camera, and its camera-space depth."""

    position: Expr
    ahead: Expr
    depth: Expr

    @classmethod
    def of(cls, target: Term, direction: Scalar) -> Self:
        """Projects an object's location, or its local Z axis as a direction at the clip end where `direction` holds, through the scene camera.

        Args:
            target: Object socket operand.
            direction: Boolean operand choosing the object's axis over its location.

        Returns:
            Device positions as frame fractions, the gate of points ahead, and the depth a Depth pass occludes them at.
        """
        camera = Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", _camera()),), socket="Location", shape=bpy.types.NodeSocketVector)
        located = Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", target),), socket="Location", shape=bpy.types.NodeSocketVector)
        axis = Expr(
            kind=bpy.types.FunctionNodeRotateVector,
            inputs=(("Vector", (0.0, 0.0, 1.0)), ("Rotation", msgspec.structs.replace(located, socket="Rotation"))),
            socket="Vector",
            shape=bpy.types.NodeSocketVector,
        )
        inverse = Expr(kind=bpy.types.FunctionNodeInvertRotation, inputs=(("Rotation", msgspec.structs.replace(camera, socket="Rotation")),), socket="Rotation")
        local = Expr(
            kind=bpy.types.FunctionNodeRotateVector, inputs=(("Vector", switched(direction, located - camera, axis)), ("Rotation", inverse)), socket="Vector", shape=bpy.types.NodeSocketVector
        )
        device = projected(local, lensed("Projection Matrix"))
        return cls(position=vector("MULTIPLY_ADD", device, (0.5, 0.5, 0.0), (0.5, 0.5, 0.0)), ahead=local["Z"] < 0.0, depth=switched(direction, -local["Z"], lensed("Clip End")))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CAMERA]


def lensed(socket: str) -> Expr:
    """Reads one Camera Info output of the scene camera, nodes keyed `camera` and `lens`."""
    return Expr(kind=bpy.types.GeometryNodeCameraInfo, inputs=(("Camera", _camera()),), socket=socket).named("lens")


def tilted(uv: Expr, lens: Expr, heading: Scalar, elevation: Scalar, *, forward: bool = False) -> Expr:
    """Maps positions through camera lens and YXZ tilt.

    Args:
        uv: Positions in normalized frame of whole image, coverage in Z.
        lens: Camera projection matrix of image.
        heading: Rightward turn of view.
        elevation: Upward tilt of view.
        forward: Forward content placement in place of inverse sampling.

    Returns:
        Source positions and front-facing geometric coverage in Z.
    """
    ray = projected(vector("MULTIPLY_ADD", uv, (2.0, 2.0, 0.0), (-1.0, -1.0, 1.0)), inverted(lens))
    turned = (
        rotated(rotated(ray, elevation, axis="X_AXIS"), math("MULTIPLY", heading, -1.0), axis="Y_AXIS")
        if forward
        else rotated(rotated(ray, heading, axis="Y_AXIS"), math("MULTIPLY", elevation, -1.0), axis="X_AXIS")
    )
    position = vector("MULTIPLY_ADD", projected(turned, lens), (0.5, 0.5, 0.0), (0.5, 0.5, 0.0))
    return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), (turned["Z"] < 0.0) * uv["Z"], position)


def reaimed(fraction: Expr, lens: Expr, heading: Expr, elevation: Expr, share: Scalar, *, forward: bool) -> Expr:
    """Re-aims the camera of an overscanned frame, camera frame fractions in and out, identity at zero turn.

    Args:
        fraction: Camera frame fractions with geometric coverage in Z.
        lens: Camera projection matrix.
        heading: Rightward turn of view.
        elevation: Upward tilt of view.
        share: Camera frame share of each overscanned side.
        forward: Forward content placement in place of inverse sampling.

    Returns:
        Re-aimed fractions with front-facing coverage in Z.
    """
    margin = math("MULTIPLY_ADD", share, -0.5, 0.5)
    device = vector("ADD", vector("MULTIPLY", vector("MULTIPLY_ADD", fraction, share, margin), (1.0, 1.0, 0.0)), (0.0, 0.0, 1.0))
    turned = tilted(device, lens, heading, elevation, forward=forward)
    position = vector("MULTIPLY", vector("DIVIDE", vector("SUBTRACT", turned, margin), share), (1.0, 1.0, 0.0))
    return switched((abs(heading) + abs(elevation)) > 0.0, fraction, vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), fraction["Z"] * turned["Z"], position))


def confusion(depth: Expr, f_number: Expr, meters: Expr, limit: Scalar) -> Expr:
    """Thin-lens circle of confusion radius over the sensor's greater side at scene depths through the scene camera.

    Args:
        depth: Camera-space depth in Blender units, the Depth pass.
        f_number: Lens f-number.
        meters: Meters per Blender unit.
        limit: Largest radius, also the radius focused at the focal length.

    Returns:
        Radius capped at `limit`, 0 under an orthographic camera.
    """
    focal_length, sensor, distance = (lensed(socket) for socket in ("Focal Length", "Sensor", "Focus Distance"))
    focal = focal_length * (0.001 / meters)
    gap = abs(distance - focal)
    radius = focal_length / math("MAXIMUM", sensor["X"], sensor["Y"]) * (focal * 0.5 / (f_number * gap)) * abs(math("DIVIDE", distance, depth) - 1.0)
    return switched(lensed("Is Orthographic"), switched(equal(gap, 0.0), math("MINIMUM", radius, limit), limit), 0.0)


def _camera() -> Expr:
    """Active Camera output keyed `camera`."""
    return Expr(kind=bpy.types.GeometryNodeInputActiveCamera, socket="Active Camera").named("camera")


@grouped(kind=bpy.types.CompositorNodeTree, key=Some("chief_ray"))
def chief_ray(image: Annotated[Expr, bpy.types.NodeSocketColor]) -> Mapping[Key, Expr]:
    """Cosine between the scene camera's axis and the ray through each pixel of an image of its frame, sensor fit and shift included, its fourth power the natural illumination falloff."""
    pixel = FrameSpace.of(image, 1.0).fraction
    inverse = inverted(lensed("Projection Matrix"))
    near, middle = (projected(vector("MULTIPLY_ADD", pixel, (2.0, 2.0, 0.0), (-1.0, -1.0, plane)), inverse) for plane in (-1.0, 0.0))
    return {"cosine": abs(vector("NORMALIZE", middle - near)["Z"])}


# --- [MAPS]


def mapped(position: Expr, transform: Callable[[ImagePoint[Expr], Namespace[Expr, Expr]], ImagePoint[Expr]]) -> Expr:
    """Evaluates an imaging point map over the components of a position vector, coverage in Z.

    Args:
        position: Image positions with geometric coverage in Z.
        transform: Map written over a namespace, such as `Radial.to_lens` or a panorama's `to_image` after its `to_ray`.

    Returns:
        Mapped positions with coverage in Z.
    """
    s, t, coverage = transform((position["X"], position["Y"], position["Z"]), EXPRESSIONS)
    return xyz(s, t, coverage)


# --- [IRIS]


def iris(read: Callable[[object], Scalar], shift: Scalar = 0.0) -> Expr:
    """Builds the Bokeh Image node of an aperture, each `Aperture` field read at the Bokeh Image input it pairs with by member, with a bokeh color shift."""
    return Expr(kind=bpy.types.CompositorNodeBokehImage, inputs=(*((socket, read(field)) for field, socket in _IRIS.items()), ("Color Shift", shift)), socket="Image", shape=bpy.types.NodeSocketColor)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Sighted", "chief_ray", "confusion", "iris", "lensed", "mapped", "reaimed", "tilted"]
