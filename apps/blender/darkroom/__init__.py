"""Darkroom extension registration."""

from blender.bpy.extension.registration import scope
from blender.bpy.nodes.placement import Arranger
from blender.bpy.render.previews.queue import PreviewQueue
from blender.bpy.ui.resources.icons import Icons
from blender.bpy.ui.surfaces.preferences import PreferencesPage

from . import color, effects, preferences, rendering, review

# --- [COMPOSITION] ----------------------------------------------------------------------

register, unregister = scope(
    __name__,
    (
        Icons(),
        PreferencesPage(record=preferences.Settings, sections=(*preferences.sections, rendering.batch.setup), bindings=rendering.batch.bindings),
        PreviewQueue(),
        Arranger(),
        *color.rows,
        *effects.rows,
        *rendering.rows,
        *review.rows,
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
