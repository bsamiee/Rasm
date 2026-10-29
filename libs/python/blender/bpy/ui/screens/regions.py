"""Region visible bounds, visibility, alignment, and editor coordinates."""

from collections.abc import Mapping
from functools import partial, reduce
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from expression import Error, Ok, Option, Some, try_downcast
from expression.collections import Block

from blender.bpy.faults import BlenderReason
from blender.bpy.ui.resources.theme import Metrics, overlaps
from blender.bpy.ui.screens.areas import Edge, EditorState, Located
from blender.records.faults import Fault, Outcome, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Bounds = tuple[int, int, int, int]

# --- [CONSTANTS] ------------------------------------------------------------------------

_AXES: Final[tuple[tuple[Edge, Edge], ...]] = (("LEFT", "RIGHT"), ("TOP", "BOTTOM"))
_RENAMED: Final[Mapping["rna_enums.RegionTypeItems", str]] = MappingProxyType({"TOOLS": "toolbar", "ASSET_SHELF_HEADER": "asset_shelf"})

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GEOMETRY]


def visible_bounds(area: bpy.types.Area, region: bpy.types.Region) -> _Bounds:
    """Returns the region's `(x, y, width, height)` in its own pixels less overlapping siblings, each inset from the bounds earlier siblings left, by Blender's `region_visible_rect_calc`."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def inset(bounds: _Bounds, sibling: bpy.types.Region) -> _Bounds:
        left, bottom, right, top = bounds
        x, y = sibling.x, sibling.y
        xmax, ymax = x + sibling.width - 1, y + sibling.height - 1
        match sibling.alignment:
            case _ if not (left <= xmax and right >= x and bottom <= ymax and top >= y):
                return bounds
            case "LEFT" | "RIGHT":
                return (xmax if abs(left - x) < 2 else left), bottom, (x if abs(right - xmax) < 2 else right), top
            case "TOP" | "BOTTOM":
                return left, (ymax if abs(bottom - y) < 2 else bottom), right, (y if abs(top - ymax) < 2 else top)
            case _:
                return bounds

    siblings = (sibling for sibling in area.regions if sibling != region and overlaps(preferences, area.type, sibling.type))
    left, bottom, right, top = reduce(inset, siblings, (region.x, region.y, region.x + region.width - 1, region.y + region.height - 1))
    return left - region.x, bottom - region.y, right - left + 1, top - bottom + 1


# --- [VISIBILITY]


def visibility(space: type[bpy.types.Space], kind: "rna_enums.RegionTypeItems", *, visible: bool) -> Outcome[EditorState]:
    """Returns the editor state showing or hiding regions of a type through the space's `show_region_<type>` member, the toolbar and asset shelf header named apart, absent fault for a member the space lacks."""
    return EditorState.of(space, {(f"show_region_{_RENAMED.get(kind, kind.lower())}",): visible})


def flipped(located: Located, side: Edge) -> Outcome[None]:
    """Moves the located region to `side` across its axis."""

    def moved(region: bpy.types.Region) -> Outcome[None]:
        aligned = region.alignment
        match Block.of_seq(_AXES).filter(lambda axis: aligned in axis).try_head().default_value(()):
            case _ if aligned == side:
                return Ok(None)
            case axis if side in axis:
                return located.run(bpy.ops.screen.region_flip, {})
            case axis:
                return Error(Fault.of(RecordReason.OUTSIDE, repr(located.area), str(side), member=", ".join(axis)))

    return located.region.to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(located.area), member="region")).bind(moved)


def quadded(located: Located, *, shown: bool) -> Outcome[None]:
    """Toggles quad view of the located 3D View region when its state differs."""
    return (
        Option
        .of_optional(try_downcast(bpy.types.SpaceView3D, located.area.spaces.active))
        .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(located.area), member=bpy.types.SpaceView3D.__name__))
        .bind(lambda space: Ok(None) if (len(space.region_quadviews) > 0) == shown else located.run(bpy.ops.screen.region_quadview, {}))
    )


# --- [COORDINATES]


def to_view(located: Located, x: float, y: float) -> Option[tuple[float, float]]:
    """Returns editor coordinates of located region pixels, Nothing without a region."""

    def viewed(region: bpy.types.Region) -> tuple[float, float]:
        point: tuple[float, float] = region.view2d.region_to_view(x, y)  # type: ignore[assignment]
        view_x, view_y = point
        scale = _scale(located.area)
        return view_x / scale, view_y / scale

    return located.region.map(viewed)


def to_region(located: Located, x: float, y: float) -> Option[tuple[int, int]]:
    """Returns located region pixels of an editor point, Nothing without a region or outside it."""

    def placed(region: bpy.types.Region) -> Option[tuple[int, int]]:
        scale = _scale(located.area)
        point: tuple[int, int] = region.view2d.view_to_region(x * scale, y * scale, clip=False)  # type: ignore[assignment]
        column, row = point
        return Some(point).filter(lambda _: 0 <= column < region.width and 0 <= row < region.height)

    return located.region.bind(placed)


def _scale(area: bpy.types.Area) -> float:
    """Returns the editor to View2D scale, the interface scale in a node editor and 1 elsewhere."""
    return Metrics.read().scale if area.type == "NODE_EDITOR" else 1.0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["flipped", "quadded", "to_region", "to_view", "visibility", "visible_bounds"]
