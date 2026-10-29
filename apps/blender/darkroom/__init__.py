"""Darkroom extension registration."""

from pathlib import Path

from expression import Some

from blender.bpy.extension.registration import Registration
from blender.bpy.nodes.placement import Arranger
from blender.bpy.render.previews.queue import PreviewQueue
from blender.bpy.ui.resources.icons import Icons

from . import catalog, preferences
from .color import films, looks, luts, response
from .rendering import outputs, queue
from .review import calibration, charts, history, metering

# --- [COMPOSITION] ----------------------------------------------------------------------

REGISTRATION = Registration(
    __name__,
    (
        Icons(brand=Some(Path("mark.svg"))),
        preferences.page,
        PreviewQueue(),
        Arranger(),
        *catalog.rows,
        *outputs.rows,
        *queue.rows,
        *films.rows,
        *luts.rows,
        *looks.rows,
        *response.rows,
        *metering.rows,
        *charts.rows,
        *calibration.rows,
        *history.rows,
    ),
)
register, unregister = REGISTRATION.register, REGISTRATION.unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
