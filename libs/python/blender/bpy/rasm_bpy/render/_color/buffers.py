"""Image Editor OCIO view policy."""

import bpy
import PyOpenColorIO

from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.render._color.spaces import viewed
from rasm_imaging import DisplayView
from rasm_records import Outcome, paired

# --- [OPERATIONS] -----------------------------------------------------------------------


def editor_view(context: bpy.types.Context, image: bpy.types.Image, config: PyOpenColorIO.Config) -> Outcome[DisplayView]:
    """Returns the editor's live OCIO view policy for its shown image.

    Args:
        context: Image Editor context supplying its live scene and display-channel mode.
        image: Shown image whose source and view-as-render setting choose the view.
        config: Configuration naming the display's default view.

    Returns:
        Scene viewing pipeline or display default, or absent context faults.
    """

    def selected(scene: bpy.types.Scene, space: bpy.types.SpaceImageEditor) -> DisplayView:
        view = viewed(scene)
        return (
            view
            if space.display_channels not in {"ALPHA", "Z_BUFFER"} and image.source != "GENERATED" and image.use_view_as_render
            else DisplayView(display=view.display, view=config.getDefaultView(view.display))
        )

    return paired(rooted(bpy.types.Scene, context), rooted(bpy.types.SpaceImageEditor, context)).map(lambda found: selected(*found))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["editor_view"]
