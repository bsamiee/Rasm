"""Darkroom preferences, page sections, and typed settings access."""

from collections.abc import Callable
from typing import Annotated

import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.render.previews.nodes import NodePreviews
from blender.bpy.render.sessions.queue import Queueing
from blender.bpy.settings.history import Retention
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.status import Status
from blender.bpy.ui.surfaces.preferences import PreferencesPage

# --- [MODELS] ---------------------------------------------------------------------------


class Settings(msgspec.Struct, frozen=True, kw_only=True):
    """Preferences one user keeps across Blender files."""

    queue: Annotated[Queueing, msgspec.Meta(title="Render Queue", description="Target, notices, power hold, heartbeat, and Flamenco scripts folder of queued renders")] = Queueing()
    review: Annotated[Retention, msgspec.Meta(title="Render History", description="Renders the image editor keeps, newest first")] = Retention(interval=0.0)
    previews: Annotated[NodePreviews, msgspec.Meta(title="Node Previews", description="Node editor thumbnails")] = NodePreviews()


# --- [COMPOSITION] ----------------------------------------------------------------------

page = PreferencesPage(record=Settings, sections=(Single(key="status", title="Status", rows=(Status(),)),))
queueing = page.value(lambda settings: settings.queue)
reviewing = page.value(lambda settings: settings.review)
previewing = page.value(lambda settings: settings.previews)
previewed: Callable[[Identity, NodePreviews], BlenderResult[None]] = page.stored(lambda settings, previews: msgspec.structs.replace(settings, previews=previews))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Settings", "page", "previewed", "previewing", "queueing", "reviewing"]
