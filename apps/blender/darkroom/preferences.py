"""Darkroom preferences: the library forms one user keeps across Blender files, its page, and their reads."""

from typing import Final

import bpy

from blender.bpy.render.previews.nodes import NodePreviews
from blender.bpy.render.sessions.queue import Queueing
from blender.bpy.settings.history import Retaining
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.status import Status
from blender.bpy.ui.surfaces.preferences import PreferencesPage
from blender.records.trees import Retention

# --- [MODELS] ---------------------------------------------------------------------------


class Preferences(bpy.types.PropertyGroup):
    """Preferences one user keeps across Blender files."""

    queue: bpy.props.PointerProperty(type=Queueing, name="Render Queue")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    review: bpy.props.PointerProperty(type=Retaining, name="Render History")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    previews: bpy.props.PointerProperty(type=NodePreviews, name="Node Previews")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [COMPOSITION] ----------------------------------------------------------------------

page: Final = PreferencesPage(form=Preferences, sections=(Single(key="status", title="Status", rows=(Status(),)),))
queueing: Final = page.value(lambda preferences: preferences.queue)
reviewing: Final = page.value(lambda preferences: Retention(capacity=preferences.review.capacity, interval=0.0))
previewing: Final = page.value(lambda preferences: preferences.previews)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Preferences", "page", "previewing", "queueing", "reviewing"]
