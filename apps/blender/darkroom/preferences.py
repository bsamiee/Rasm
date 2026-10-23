"""Darkroom preferences, page sections, and typed settings access."""

from collections.abc import Callable
from typing import Annotated

import msgspec

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.render.previews.nodes import NodePreviews
from blender.bpy.render.review.history import Reviewing
from blender.bpy.render.sessions.queue import Queueing
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.status import Status
from blender.bpy.ui.surfaces.preferences import stored, value

# --- [MODELS] ---------------------------------------------------------------------------


class Settings(msgspec.Struct, frozen=True, kw_only=True):
    """Preferences one user keeps across Blender files."""

    queue: Annotated[Queueing, msgspec.Meta(title="Render Queue", description="Target, notices, power hold, heartbeat, and Flamenco scripts folder of queued renders")] = Queueing()
    review: Annotated[Reviewing, msgspec.Meta(title="Stored Renders", description="Render history the image editor keeps")] = Reviewing()
    previews: Annotated[NodePreviews, msgspec.Meta(title="Node Previews", description="Node editor thumbnails")] = NodePreviews()


# --- [COMPOSITION] ----------------------------------------------------------------------

sections = (Single(key="status", title="Status", rows=(Status(),)),)
queueing = value(Settings, lambda settings: settings.queue)
reviewing = value(Settings, lambda settings: settings.review)
previewing = value(Settings, lambda settings: settings.previews)
previewed: Callable[[Identity, NodePreviews], BlenderResult[None]] = stored(Settings, lambda settings, previews: msgspec.structs.replace(settings, previews=previews))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Settings", "previewed", "previewing", "queueing", "reviewing", "sections"]
