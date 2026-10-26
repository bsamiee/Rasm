"""Picks in the image editor's frame: a pointer square read as a frame region for the pick's edit."""

from collections.abc import Callable
import math
from typing import Annotated, TYPE_CHECKING

import bpy
from expression import Some
import msgspec
import numpy as np

from blender.bpy.commands.operators import Call, Operator, Outcome, Pick
from blender.bpy.data.rna.conditions import AllOf, check
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.settings.properties import Definition
from blender.bpy.ui.images.views import framed, image_type, WINDOW
from blender.imaging.geometry.regions import Region
from blender.records.faults import paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Picked(msgspec.Struct, frozen=True, kw_only=True):
    """Pick point and sampled square half side."""

    x: Annotated[int, msgspec.Meta(title="X", description="Pick column in region pixels"), Definition(options=frozenset({"HIDDEN", "SKIP_SAVE"}))] = 0
    y: Annotated[int, msgspec.Meta(title="Y", description="Pick row in region pixels"), Definition(options=frozenset({"HIDDEN", "SKIP_SAVE"}))] = 0
    radius: Annotated[int, msgspec.Meta(ge=0, le=3, title="Sample Radius", description="Pixels sampled each side of pick")] = 0


# --- [OPERATIONS] -----------------------------------------------------------------------


def picking[P: Picked](
    key: Key,
    title: str,
    description: str,
    cursor: "rna_enums.WindowCursorItems",
    form: type[P],
    taken: Callable[[Call[P], Region], BlenderResult[Outcome]],
    kinds: tuple[str, ...] = ("RENDER_RESULT",),
) -> Operator[P]:
    """Returns an operator reading a pick over an image of `kinds` as the frame region `taken` edits."""

    def ran(call: Call[P]) -> BlenderResult[Outcome]:
        context, record = call.context, call.record

        def regioned(space: bpy.types.SpaceImageEditor, region: bpy.types.Region) -> BlenderResult[Region]:
            frame, (zoom_x, zoom_y) = framed(region), np.asarray(space.zoom)
            width, height = round(frame.width / zoom_x), round(frame.height / zoom_y)
            column, row = math.floor((record.x - frame.x) / zoom_x), math.floor((record.y - frame.y) / zoom_y)
            reach = np.array(((-record.radius, -record.radius), (record.radius + 1, record.radius + 1)))
            return (
                Some((column, row))
                .filter(lambda pixel: 0 <= pixel[0] < width and 0 <= pixel[1] < height)
                .map(lambda pixel: Region.bounding(np.clip((np.array(pixel) + reach) / (width, height), 0.0, 1.0)))
                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(space.image), key=f"pixel {column}, {row}"),))
            )

        return (
            check(AllOf(conditions=(image_type(kinds), WINDOW)), context)
            .bind(lambda _: paired(rooted(bpy.types.SpaceImageEditor, context), call.root(bpy.types.Region)))
            .bind(lambda pair: regioned(*pair))
            .bind(lambda region: taken(call, region))
        )

    return Operator(
        key=key,
        title=title,
        description=description,
        form=form,
        run=ran,
        invoke=Some(Pick(cursor=cursor)),
        seed=Some(lambda record, event, _identity: msgspec.structs.replace(record, x=event.mouse_region_x, y=event.mouse_region_y)),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Picked", "picking"]
