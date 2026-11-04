"""Scene camera node forms: camera reads and projections, lens tilts and re-aims, imaging point maps and radial lens maps over a frame, the chief ray, the circle of confusion, and the iris on the native Bokeh Image node."""

from collections.abc import Callable, Mapping
from types import MappingProxyType
from typing import Annotated, Final, Self

import bpy
from expression import Some
import msgspec

from blender.bpy.extension.identity import Key
from blender.bpy.nodes.functions import EXPRESSIONS, grouped
from blender.bpy.nodes.graphs import equal, Expr, inverted, math, projected, Scalar, SocketType, switched, Term, vector, xyz
from blender.bpy.nodes.interfaces import fielded, Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.compositor.coordinates import FrameSpace
from blender.bpy.render.compositor.shapes import rotated
from blender.imaging.geometry.arithmetic import Namespace
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.geometry.panoramas import ImagePoint

# --- [TYPES] ----------------------------------------------------------------------------

type PointMap = Callable[[ImagePoint[Expr], Namespace[Expr, Expr]], ImagePoint[Expr]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_BOKEH: Final = MappingProxyType({
    Aperture.blades: StockInput(node=bpy.types.CompositorNodeBokehImage, input="Flaps"),
    Aperture.rotation: StockInput(node=bpy.types.CompositorNodeBokehImage, input="Angle"),
    Aperture.roundness: StockInput(node=bpy.types.CompositorNodeBokehImage, input="Roundness"),
    Aperture.obstruction: StockInput(node=bpy.types.CompositorNodeBokehImage, input="Catadioptric Size"),
})
IRIS: Final = InterfacePanel.of(
    "iris", Interface(inputs=tuple(msgspec.structs.replace(socket, kind=_BOKEH[getattr(Aperture, socket.key)]) for socket in fielded(Aperture()).inputs)), description="Bokeh iris outline"
)

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
            Frame fractions with the gate of points ahead in Z, that gate, and the depth a Depth pass occludes them at.
        """
        located = Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", target),), socket="Location", socket_type=bpy.types.NodeSocketVector)
        axis = Expr(
            kind=bpy.types.FunctionNodeRotateVector,
            inputs=(("Vector", (0.0, 0.0, 1.0)), ("Rotation", located.at("Rotation", bpy.types.NodeSocketRotation))),
            socket="Vector",
            socket_type=bpy.types.NodeSocketVector,
        )
        local = to_camera_space(switched(direction, located - camera_object("Location", bpy.types.NodeSocketVector), axis))
        position = seen(local, camera_info("Projection Matrix"))
        return cls(position=position, ahead=position["Z"], depth=switched(direction, -local["Z"], camera_info("Clip End")))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CAMERA]


def camera_info(socket: str) -> Expr:
    """Reads one Camera Info output of the scene camera, nodes keyed `camera` and `lens`."""
    return Expr(kind=bpy.types.GeometryNodeCameraInfo, inputs=(("Camera", _active()),), socket=socket).keyed("lens")


def camera_object(socket: str, socket_type: SocketType = bpy.types.NodeSocketFloat, /) -> Expr:
    """Reads one Object Info output of the scene camera (world `Location`, `Rotation`) in a socket class's shape."""
    return Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", _active()),), socket=socket, socket_type=socket_type)


def to_camera_space(direction: Term) -> Expr:
    """Turns a world vector into the scene camera's space by the inverse of its world rotation."""
    inverse = Expr(kind=bpy.types.FunctionNodeInvertRotation, inputs=(("Rotation", camera_object("Rotation", bpy.types.NodeSocketRotation)),), socket="Rotation")
    return Expr(kind=bpy.types.FunctionNodeRotateVector, inputs=(("Vector", direction), ("Rotation", inverse)), socket="Vector", socket_type=bpy.types.NodeSocketVector)


def seen(local: Expr, lens: Expr, coverage: Scalar = 1.0) -> Expr:
    """Projects a camera-space point or direction through a projection matrix to frame fractions, its coverage in Z where it lies ahead of the camera and 0 behind.

    Args:
        local: Camera-space point or direction.
        lens: Camera projection matrix (`camera_info("Projection Matrix")`).
        coverage: Coverage kept ahead of the camera.

    Returns:
        Frame fractions from the bottom-left corner with front-facing coverage in Z.
    """
    position = vector("MULTIPLY_ADD", projected(local, lens), (0.5, 0.5, 0.0), (0.5, 0.5, 0.0))
    return vector("MULTIPLY_ADD", (0.0, 0.0, 1.0), (local["Z"] < 0.0) * coverage, position)


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
    return seen(turned, lens, uv["Z"])


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
        limit: Largest radius, and the radius where focus distance equals focal length.

    Returns:
        Radius capped at `limit`, 0 under an orthographic camera.
    """
    focal_length, sensor, distance = (camera_info(socket) for socket in ("Focal Length", "Sensor", "Focus Distance"))
    focal = focal_length * (0.001 / meters)
    gap = abs(distance - focal)
    radius = focal_length / math("MAXIMUM", sensor["X"], sensor["Y"]) * (focal * 0.5 / (f_number * gap)) * abs(math("DIVIDE", distance, depth) - 1.0)
    return switched(camera_info("Is Orthographic"), switched(equal(gap, 0.0), math("MINIMUM", radius, limit), limit), 0.0)


def _active() -> Expr:
    """Active Camera output keyed `camera`."""
    return Expr(kind=bpy.types.GeometryNodeInputActiveCamera, socket="Active Camera").keyed("camera")


@grouped(kind=bpy.types.CompositorNodeTree, key=Some("chief_ray"))
def chief_ray(image: Annotated[Expr, bpy.types.NodeSocketColor]) -> Mapping[Key, Expr]:
    """Cosine between the scene camera's axis and the ray through each pixel of an image of its frame, sensor fit and shift included, its fourth power the natural illumination falloff."""
    pixel = FrameSpace.of(image, 1.0).fraction
    inverse = inverted(camera_info("Projection Matrix"))
    near, middle = (projected(vector("MULTIPLY_ADD", pixel, (2.0, 2.0, 0.0), (-1.0, -1.0, plane)), inverse) for plane in (-1.0, 0.0))
    return {"cosine": abs(vector("NORMALIZE", middle - near)["Z"])}


# --- [MAPS]


def mapped(position: Expr, transform: PointMap) -> Expr:
    """Evaluates an imaging point map over the components of a position vector, coverage in Z.

    Args:
        position: Image positions with geometric coverage in Z.
        transform: Map written over a namespace (`Radial.to_lens` or a panorama's `to_image` after its `to_ray`).

    Returns:
        Mapped positions with coverage in Z.
    """
    s, t, coverage = transform((position["X"], position["Y"], position["Z"]), EXPRESSIONS)
    return xyz(s, t, coverage)


def radial(frame: FrameSpace, transform: PointMap) -> Expr:
    """Maps displayed frame positions through a radial lens map of unit aspect, displayed offsets from the frame center entering as fractions about 0.5.

    Args:
        frame: Displayed camera frame of the source.
        transform: Radial map over node expressions (`Radial(distortion=..., scale=..., aspect=1.0).to_rectilinear`, its `to_lens`).

    Returns:
        Camera frame fractions with the map's real-branch coverage in Z.
    """
    held = mapped(vector("ADD", frame.point - frame.extent * 0.5, (0.5, 0.5, 1.0)), transform)
    fraction = vector("DIVIDE", xyz(held["X"] - 0.5, held["Y"] - 0.5) + frame.extent * 0.5, frame.extent)
    return xyz(fraction["X"], fraction["Y"], held["Z"])


# --- [IRIS]


def iris(read: Callable[[Socket], Scalar], shift: Scalar = 0.0) -> Expr:
    """Builds the Bokeh Image node of the `IRIS` aperture, each panel socket read into the Bokeh Image input it declares, with a bokeh color shift.

    Args:
        read: Group input of a declaring socket (`Shell.input` or a reader prefixing the key of a panel nesting `IRIS`).
        shift: Bokeh color shift, toward blue above 0.

    Returns:
        Bokeh Image `Image` expression.
    """
    inputs = tuple((stock.input, read(socket)) for socket in IRIS.inputs for stock in socket.stock.to_list())
    return Expr(kind=bpy.types.CompositorNodeBokehImage, inputs=(*inputs, ("Color Shift", shift)), socket="Image", socket_type=bpy.types.NodeSocketColor)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["IRIS", "PointMap", "Sighted", "camera_info", "camera_object", "chief_ray", "confusion", "iris", "mapped", "radial", "reaimed", "seen", "tilted", "to_camera_space"]
