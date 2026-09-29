# mypy: disable-error-code="union-attr"
# ty: ignore[unresolved-attribute]
"""Evaluated world bounds of scene objects and the largest 3D Viewport of open or stored windows."""

import attrs
import bpy
import numpy as np
from numpy.typing import NDArray

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Viewport:
    """3D Viewport area with its window, space, and main region."""

    window: bpy.types.Window
    area: bpy.types.Area
    space: bpy.types.SpaceView3D
    region: bpy.types.Region


# --- [OPERATIONS] -----------------------------------------------------------------------


def bounds(depsgraph: bpy.types.Depsgraph) -> dict[str, NDArray[np.float64]]:
    """Lower and upper world corners of each object's evaluated geometry and instances by object name, objects with empty geometry left out."""
    solid = np.rec.fromrecords(
        [
            ((instance.parent if instance.is_instance else instance.object).name, np.array(instance.matrix_world), box)
            for instance in depsgraph.object_instances
            if np.ptp(box := np.array(instance.object.bound_box), axis=0).any()
        ],
        dtype=[("owner", object), ("matrix", np.float64, (4, 4)), ("box", np.float64, (8, 3))],
    )
    corners = solid.box @ solid.matrix[:, :3, :3].mT + solid.matrix[:, None, :3, 3]
    owners, index = np.unique(solid.owner, return_inverse=True)
    low, high = np.full(((count := len(owners)), 3), np.inf), np.full((count, 3), -np.inf)
    np.minimum.at(low, index, corners.min(axis=1))
    np.maximum.at(high, index, corners.max(axis=1))
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

__all__ = ["Viewport", "bounds", "viewport"]
