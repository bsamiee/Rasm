"""Picks in the image editor's frame: a pointer square read as a frame region for the pick's edit."""

from collections.abc import Callable
from functools import partial, wraps
import math
from typing import TYPE_CHECKING

import bpy
from expression import Some
import numpy as np

from blender.bpy.commands.operators import Answer, Call, Operator, Pick
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.images.views import framed, RESULT, WINDOW
from blender.imaging.geometry.frames import Region
from blender.records.faults import Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Picked(bpy.types.PropertyGroup):
    """Pick point and sampled square half side."""

    x: bpy.props.IntProperty(name="X", description="Pick column in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    y: bpy.props.IntProperty(name="Y", description="Pick row in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    radius: bpy.props.IntProperty(name="Sample Radius", description="Pixels sampled each side of pick", min=0, max=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------


def picking[P: Picked](key: Key, cursor: "rna_enums.WindowCursorItems", form: type[P], taken: Callable[[Call[P], Region], Outcome[Answer]], shown: Setting = RESULT) -> Operator[P]:
    """Returns an operator reading a pick while `shown` holds as the frame region `taken` edits, described by its docstring."""

    @wraps(taken)
    def ran(call: Call[P]) -> Outcome[Answer]:
        return (
            (shown & WINDOW)
            .check(call.context)
            .bind(lambda _: paired(rooted(bpy.types.SpaceImageEditor, call.context), call.root(bpy.types.Region)))
            .bind(partial(_regioned, call.properties))
            .bind(lambda region: taken(call, region))
        )

    ran.__name__ = ran.__qualname__ = key
    return Operator(form=form, run=ran, invoke=Some(Pick(cursor=cursor)), seed=Some(lambda _properties, event, _identity: {"x": event.mouse_region_x, "y": event.mouse_region_y}))


def _regioned(record: Picked, located: tuple[bpy.types.SpaceImageEditor, bpy.types.Region]) -> Outcome[Region]:
    space, region = located
    frame, (zoom_x, zoom_y) = framed(region), np.asarray(space.zoom)
    width, height = round(frame.width / zoom_x), round(frame.height / zoom_y)
    column, row = math.floor((record.x - frame.x) / zoom_x), math.floor((record.y - frame.y) / zoom_y)
    reach = np.array(((-record.radius, -record.radius), (record.radius + 1, record.radius + 1)))
    return (
        Some((column, row))
        .filter(lambda pixel: 0 <= pixel[0] < width and 0 <= pixel[1] < height)
        .map(lambda pixel: Region.bounding(np.clip((np.array(pixel) + reach) / (width, height), 0.0, 1.0)))
        .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(space.image), member=f"pixel {column}, {row}"),))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Picked", "picking"]
