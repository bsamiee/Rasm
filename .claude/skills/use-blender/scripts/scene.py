# mypy: disable-error-code="union-attr, attr-defined, arg-type"
# ty: ignore[unresolved-attribute, invalid-argument-type]
"""Evaluated points and world bounds of scene objects and the largest 3D Viewport of open or stored windows."""

from itertools import starmap
from typing import Final

import attrs
import bpy
import numpy as np
from numpy.typing import NDArray

# --- [CONSTANTS] ------------------------------------------------------------------------

SHOWN: Final = frozenset({"SOLID", "TEXTURED"})

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Viewport:
    """3D Viewport area with its window, space, and main region."""

    window: bpy.types.Window
    area: bpy.types.Area
    space: bpy.types.SpaceView3D
    region: bpy.types.Region


# --- [OPERATIONS] -----------------------------------------------------------------------


def drawings(pencil: bpy.types.GreasePencil | None) -> list[bpy.types.GreasePencilDrawing]:
    """Drawing each Grease Pencil layer shows at the current frame."""
    frames = (layer.current_frame() for layer in (pencil.layers if pencil else ()))
    return [frame.drawing for frame in frames if frame and frame.drawing]


def points(obj: bpy.types.Object, *, drawn: bool) -> NDArray[np.float32]:
    """Local positions of an evaluated object's mesh vertices, curve points, cloud points, and current strokes, `drawn` keeping meshes with faces and curves of hair objects."""
    try:
        found = obj.evaluated_geometry()
    except TypeError:
        return np.empty((0, 3), dtype=np.float32)
    mesh, curves, cloud = found.mesh, found.curves, found.pointcloud
    sources = (
        *(((mesh.vertices, "co"),) if mesh and (mesh.polygons or not drawn) else ()),
        *(((curves.points, "position"),) if curves and (obj.type == "CURVES" or not drawn) else ()),
        *(((cloud.points, "co"),) if cloud else ()),
        *((drawing.attributes["position"].data, "vector") for drawing in drawings(found.grease_pencil)),
    )

    def read(collection: "bpy.types.bpy_prop_collection[bpy.types.bpy_struct[object]]", field: str) -> NDArray[np.float32]:
        values = np.empty(len(collection) * 3, dtype=np.float32)
        collection.foreach_get(field, values)
        return values

    return np.concatenate([*starmap(read, sources)] or [np.empty(0, dtype=np.float32)]).reshape(-1, 3)


def bounds(depsgraph: bpy.types.Depsgraph, *, drawn: bool) -> dict[str, NDArray[np.float64]]:
    """Lower and upper world corners of each object's evaluated points and instances by object name, `drawn` keeping what a Solid draw without overlays shows, objects with no point left out."""

    def local(obj: bpy.types.Object) -> NDArray[np.float64]:
        match obj.type:
            case _ if drawn and obj.display_type not in SHOWN:
                return np.empty((0, 3))
            case "MESH":
                return np.array(obj.bound_box) if len(obj.data.polygons if drawn else obj.data.vertices) else np.empty((0, 3))
            case _:
                return points(obj, drawn=drawn).astype(np.float64)

    extents = np.array(
        [
            ((instance.parent if instance.is_instance else instance.object).name, (world := found @ np.array(matrix.to_3x3()).T + np.array(matrix.translation)).min(axis=0), world.max(axis=0))
            for instance in depsgraph.object_instances
            if (found := local(instance.object)).size
            for matrix in (instance.matrix_world,)
        ],
        dtype=[("owner", object), ("low", np.float64, 3), ("high", np.float64, 3)],
    )
    owners, index = np.unique(extents["owner"], return_inverse=True)
    low, high = np.full(((count := len(owners)), 3), np.inf), np.full((count, 3), -np.inf)
    np.minimum.at(low, index, extents["low"])
    np.maximum.at(high, index, extents["high"])
    return {str(owner): np.stack((lower, upper)) for owner, lower, upper in zip(owners, low, high, strict=True)}


def viewport() -> Viewport | None:
    """Largest 3D Viewport by main region across windows, `None` when no window shows one."""
    return max(
        (
            Viewport(window, area, space, region)
            for window in bpy.context.window_manager.windows
            for area in window.screen.areas
            if isinstance(space := area.spaces.active, bpy.types.SpaceView3D)
            for region in area.regions
            if region.type == "WINDOW"
        ),
        key=lambda found: found.region.width * found.region.height,
        default=None,
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["SHOWN", "Viewport", "bounds", "drawings", "points", "viewport"]
