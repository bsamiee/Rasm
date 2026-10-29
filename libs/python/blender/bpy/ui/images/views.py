"""Image editor views: the displayed image's kind, the editors showing it, the frame it draws in, pointer picks read as frame regions, and the render view revealed where the render display preference places it, optionally at a Sidebar tab."""

from collections.abc import Callable
from functools import cached_property, partial, wraps
import math
from typing import Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
import msgspec
import numpy as np

from blender.bpy.commands.calls import called
from blender.bpy.commands.invocations import Pick
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.drawing.canvas import Rect
from blender.bpy.ui.screens.areas import AreaQuery, Arrangement, EditorState, Located
from blender.bpy.ui.screens.regions import visibility
from blender.bpy.ui.surfaces.panels import Beside, Sidebar
from blender.imaging.geometry.frames import Region
from blender.records.faults import Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

_WINDOW: Final = Setting(descriptor=Descriptor.declared(bpy.types.Context, ("region", "type"), bpy.types.EnumProperty), accepted=("WINDOW",))

# --- [MODELS] ---------------------------------------------------------------------------


class RenderView(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render view revealed, its Sidebar opened, then turned on the next host pass to the tab of the panel placement `at`, once the region has drawn its tabs."""

    at: Beside | Sidebar

    @cached_property
    def command(self) -> Operator[Empty]:
        """Command revealing the view at the placement's tab."""
        return Operator(undo=Nothing, form=Empty, run=self.show_render_view)

    def show_render_view(self, call: Call[Empty]) -> Outcome[Answer]:
        """Shows the render view, opening one unless an editor shows Render Result, then queues the Sidebar tab the placement's members name, none for a stock panel declaring no category."""
        tabbed = (
            Option
            .of_optional(try_downcast(str, self.at.members(call.identity).get("bl_category")))
            .map(lambda tab: EditorState.of(bpy.types.Region, {("active_panel_category",): tab}, Some("UI")).map(lambda state: (state,)))
            .default_value(Ok(()))
        )
        return (
            paired(revealed(call.context), paired(visibility(bpy.types.SpaceImageEditor, "UI", visible=True), tabbed))
            .bind(lambda found: call.identity.reports.queued(Arrangement(at=found[0], steps=(found[1][0], *found[1][1]))))
            .map(lambda _: Done())
        )

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the command revealing the view."""
        return Ok((self.command,))


class Picked(bpy.types.PropertyGroup):
    """Pick point and sampled square half side."""

    x: bpy.props.IntProperty(name="X", description="Pick column in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    y: bpy.props.IntProperty(name="Y", description="Pick row in region pixels", options={"HIDDEN", "SKIP_SAVE"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    radius: bpy.props.IntProperty(name="Sample Radius", description="Pixels sampled each side of pick", min=0, max=3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [EDITORS]


def image_type(kinds: tuple[str, ...]) -> Setting:
    """Returns the condition that an image editor shows an image of one of `kinds`.

    Args:
        kinds: `Image.type` items accepted.

    Returns:
        Condition over the image editor space.
    """
    return Setting(descriptor=Descriptor.declared(bpy.types.SpaceImageEditor, ("image", "type"), bpy.types.EnumProperty), accepted=kinds)


def editor(shown: Setting) -> AreaQuery:
    """Returns the query over image editors whose space `shown` accepts.

    Args:
        shown: Condition over the image editor space.

    Returns:
        Area query matching those editors.
    """
    return AreaQuery(space=bpy.types.SpaceImageEditor, ui_type=Some("IMAGE_EDITOR"), shows=Some(shown))


def framed(region: bpy.types.Region) -> Rect:
    """Returns the displayed frame's rectangle in region pixels, the region pixels of image fractions 0 and 1.

    Args:
        region: Image editor main region.

    Returns:
        Frame rectangle in region pixels.
    """
    view = region.view2d
    (left, bottom), (right, top) = np.asarray(view.region_to_view(0.0, 0.0)), np.asarray(view.region_to_view(1.0, 1.0))
    across, up = float(right - left), float(top - bottom)
    return Rect(x=float(-left) / across, y=float(-bottom) / up, width=1.0 / across, height=1.0 / up)


def viewing(context: bpy.types.Context) -> Outcome[Located]:
    """Returns the largest editor showing Render Result in a window of the context scene.

    Args:
        context: Context naming the scene and window manager.

    Returns:
        Located editor, absent fault while none shows Render Result.
    """
    return rooted(bpy.types.WindowManager, context).bind(lambda manager: RENDERED.first(window for window in manager.windows if window.scene == context.scene))


def revealed(context: bpy.types.Context) -> Outcome[Located]:
    """Returns the editor `viewing` finds, first opening the render view where the render display preference places it when none shows Render Result.

    A temporary context window opens nothing, the host toggle lowering that window in place of opening a view.

    Args:
        context: Context the render view opens from.

    Returns:
        Located editor showing Render Result, the `viewing` fault while none shows after the open.
    """
    return viewing(context).or_else_with(
        lambda missed: (
            rooted(bpy.types.Window, context)
            .filter_with(lambda window: not window.screen.is_temporary, lambda _: missed)
            .bind(lambda _: called(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT"))
            .bind(lambda _: viewing(context))
        )
    )


# --- [PICKS]


def picking[P: Picked](cursor: "rna_enums.WindowCursorItems", form: type[P], taken: Callable[[Call[P], Region], Outcome[Answer]], shown: Option[Setting] = Nothing) -> Operator[P]:
    """Returns an operator reading a pick in the main region while `shown` holds as the frame region `taken` edits, keyed and described by `taken`.

    The button opening the pick draws anywhere, the host deferring the operator until the pointer picks a location, so `shown` tests the editor picked in.

    Args:
        cursor: Cursor the pending pick shows.
        form: Pick properties, `Picked` or a group extending it.
        taken: Edit of the picked frame region.
        shown: Condition over the image editor space, an editor showing Render Result while unstated.

    Returns:
        Operator seeding the pick from the event's region position.
    """

    @wraps(taken)
    def ran(call: Call[P]) -> Outcome[Answer]:
        return (
            (shown.default_value(RESULT) & _WINDOW)
            .check(call.context)
            .bind(lambda _: paired(rooted(bpy.types.SpaceImageEditor, call.context), rooted(bpy.types.Region, call.context)))
            .bind(partial(_regioned, call.properties))
            .bind(lambda region: taken(call, region))
        )

    return Operator(form=form, run=ran, invoke=Some(Pick(cursor=cursor)), seed=Some(lambda _properties, event, _identity: {"x": event.mouse_region_x, "y": event.mouse_region_y}))


def _regioned(record: Picked, located: tuple[bpy.types.SpaceImageEditor, bpy.types.Region]) -> Outcome[Region]:
    """Returns the frame region of the square around the picked image pixel, absent fault for a pick outside the image."""
    space, region = located
    frame, (zoom_x, zoom_y) = framed(region), np.asarray(space.zoom)
    width, height = round(frame.width / zoom_x), round(frame.height / zoom_y)
    across, up = np.asarray(region.view2d.region_to_view(record.x, record.y))
    column, row = math.floor(across * width), math.floor(up * height)
    reach = np.array(((-record.radius, -record.radius), (record.radius + 1, record.radius + 1)))
    return (
        Some((column, row))
        .filter(lambda pixel: 0 <= pixel[0] < width and 0 <= pixel[1] < height)
        .map(lambda pixel: Region.bounding(np.clip((np.array(pixel) + reach) / (width, height), 0.0, 1.0)))
        .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(space.image), member=f"pixel {column}, {row}"))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

RESULT: Final = image_type(("RENDER_RESULT",))
RENDERED: Final = editor(RESULT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RENDERED", "RESULT", "Picked", "RenderView", "editor", "framed", "image_type", "picking", "revealed", "viewing"]
