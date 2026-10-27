"""Image editor views: the displayed image's kind, the frame it draws in, and the render view opened at a Sidebar tab."""

from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, rooted
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.ui.drawing.canvas import Rect
from blender.bpy.ui.screens.areas import AreaQuery, Arrangement, Located
from blender.bpy.ui.screens.editors import EditorState
from blender.records.faults import Outcome

# --- [MODELS] ---------------------------------------------------------------------------


class ImageWindow(msgspec.Struct, frozen=True, kw_only=True):
    """Render view where the render display preference places it, its Sidebar opened, then turned to `tab` on the next host pass once the region has drawn its tabs."""

    tab: str

    @property
    def command(self) -> Operator[Empty]:
        """Command opening the view at `tab`, one per extension, equal across reads."""
        return Operator(undo=False, form=Empty, run=self.show_render_view)

    def show_render_view(self, call: Call[Empty]) -> Outcome[Answer]:
        """Opens the render view unless the window is temporary or shows one, keeping a view the host toggle would hide, then queues its Sidebar."""
        sidebar = EditorState(values=((Descriptor.declared(bpy.types.SpaceImageEditor, "show_region_ui", bpy.types.BoolProperty), True),))
        tabbed = EditorState(region=Some("UI"), values=((Descriptor.declared(bpy.types.Region, "active_panel_category", bpy.types.EnumProperty), self.tab),))
        return (
            Located
            .current(call.context)
            .bind(lambda located: Ok(None) if located.window.screen.is_temporary or RENDERED.located((located.window,)) else located.run(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT"))
            .bind(lambda _: viewing(call.context))
            .bind(lambda located: call.identity.reports.queued(Arrangement(at=located, steps=(sidebar, tabbed))))
            .map(lambda _: Done())
        )

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the command opening the view."""
        return Ok((self.command,))


# --- [OPERATIONS] -----------------------------------------------------------------------


def image_type(kinds: tuple[str, ...]) -> Setting:
    """Returns the condition that an image editor shows an image of one of `kinds`."""
    return Setting(descriptor=Descriptor.declared(bpy.types.SpaceImageEditor, ("image", "type"), bpy.types.EnumProperty), accepted=kinds)


def framed(region: bpy.types.Region) -> Rect:
    """Returns the displayed frame's rectangle in region pixels."""
    view = region.view2d
    (left, bottom), (right, top) = np.asarray(view.region_to_view(0.0, 0.0)), np.asarray(view.region_to_view(1.0, 1.0))
    across, up = float(right - left), float(top - bottom)
    return Rect(x=float(-left) / across, y=float(-bottom) / up, width=1.0 / across, height=1.0 / up)


def viewing(context: bpy.types.Context) -> Outcome[Located]:
    """Returns the largest editor showing Render Result in a window of the context scene."""
    return rooted(bpy.types.WindowManager, context).bind(lambda manager: RENDERED.first(window for window in manager.windows if window.scene == context.scene))


def revealed(context: bpy.types.Context) -> Outcome[Option[Located]]:
    """Returns the editor `viewing` finds, `Nothing` after opening the render view when none shows Render Result."""
    return viewing(context).map(Some).or_else_with(lambda _: called(bpy.ops.render.view_show, {}, context="INVOKE_DEFAULT").map(lambda _: Nothing))


# --- [COMPOSITION] ----------------------------------------------------------------------

RESULT: Final = image_type(("RENDER_RESULT",))
RENDERED: Final = AreaQuery(space="IMAGE_EDITOR", ui_type=Some("IMAGE_EDITOR"), shows=Some(RESULT))
WINDOW: Final = Setting(descriptor=Descriptor.declared(bpy.types.Context, ("region", "type"), bpy.types.EnumProperty), accepted=("WINDOW",))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RENDERED", "RESULT", "WINDOW", "ImageWindow", "framed", "image_type", "revealed", "viewing"]
