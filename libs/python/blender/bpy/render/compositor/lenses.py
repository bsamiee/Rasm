"""Lens node forms: imaging lens and panorama maps evaluated over node expressions, the scene camera's chief ray and circle of confusion, and the iris bound to the native Bokeh Image node."""

from collections.abc import Callable
import functools

import bpy
import msgspec

from blender.bpy.nodes.graphs import Expr, group_input, math, NodeGroup, Scalar, switched, Term, vector, xyz
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.coordinates import framed, lensed
from blender.imaging.geometry.arithmetic import Namespace
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.geometry.panoramas import Point
from blender.records.faults import Outcome

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MAPS]


def mapped(position: Expr, transform: Callable[[Point[Expr], Namespace[Expr, Expr]], Point[Expr]]) -> Expr:
    """Evaluates an imaging point map over the components of a position vector, coverage in Z.

    Args:
        position: Image positions with geometric coverage in Z.
        transform: Map written over a namespace, such as `Radial.to_lens` or a panorama's `to_image` after its `to_ray`.

    Returns:
        Mapped positions with coverage in Z.
    """
    s, t, coverage = transform((position["X"], position["Y"], position["Z"]), Expr)
    return xyz(s, t, coverage)


# --- [CAMERA]


@functools.cache
def _chief_ray() -> Outcome[NodeGroup]:
    """Chief-ray cosine group of the scene camera over its image's frame, sensor fit and shift included."""
    pixel = framed(group_input(COLOR_INPUT.key, "RGBA"), 1.0).fraction
    inverse = Expr(kind=bpy.types.FunctionNodeInvertMatrix, inputs=(("Matrix", lensed("Projection Matrix")),), socket="Matrix")
    near, middle = (
        Expr(kind=bpy.types.FunctionNodeProjectPoint, inputs=(("Vector", vector("MULTIPLY_ADD", pixel, (2.0, 2.0, 0.0), (-1.0, -1.0, plane))), ("Transform", inverse)), socket="Vector", shape="VECTOR")
        for plane in (-1.0, 0.0)
    )
    face = Interface(inputs=(COLOR_INPUT,), outputs=(Socket(key="cosine", name="Cosine", kind=bpy.types.NodeSocketFloat),))
    return NodeGroup.built("chief_ray", "Chief Ray", face, {"cosine": abs(vector("NORMALIZE", middle - near)["Z"])}, bpy.types.CompositorNodeTree)


def chief_ray(image: Term) -> Outcome[Expr]:
    """Cosine between the scene camera's axis and the ray through each pixel of an image of its frame, its fourth power the natural illumination falloff."""
    return _chief_ray().map(lambda group: group.placed({COLOR_INPUT.key: image}, "cosine"))


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
    singular = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", gap), ("B", 0.0), ("Epsilon", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
    radius = focal_length / math("MAXIMUM", sensor["X"], sensor["Y"]) * (focal * 0.5 / (f_number * gap)) * abs(math("DIVIDE", distance, depth) - 1.0)
    return switched(lensed("Is Orthographic"), switched(singular, math("MINIMUM", radius, limit), limit), 0.0)


# --- [IRIS]


def iris(read: Callable[[msgspec.structs.FieldInfo], Scalar], shift: Scalar = 0.0) -> Expr:
    """Builds the Bokeh Image node of an aperture, each `Aperture` field read at its socket, with a bokeh color shift."""
    sockets = ("Flaps", "Angle", "Roundness", "Catadioptric Size")
    return Expr(
        kind=bpy.types.CompositorNodeBokehImage,
        inputs=(*((socket, read(field)) for socket, field in zip(sockets, msgspec.structs.fields(Aperture))), ("Color Shift", shift)),
        socket="Image",
        shape="RGBA",
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["chief_ray", "confusion", "iris", "mapped"]
