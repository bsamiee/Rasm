"""Darkroom extension registration."""

from pathlib import Path

from expression import Some

from blender.bpy.extension.registration import Registration
from blender.bpy.ui.resources.icons import Icons
from blender.bpy.ui.surfaces.panels import PreferencesPage

from . import catalog
from .color import looks, luts, response
from .rendering import renders
from .review import calibration, metering, results

# --- [COMPOSITION] ----------------------------------------------------------------------

REGISTRATION = Registration(
    __name__, (Icons(brand=Some(Path("mark.svg"))), PreferencesPage(), *catalog.rows, *renders.rows, *luts.rows, *looks.rows, *response.rows, *metering.rows, *calibration.rows, *results.rows)
)
register, unregister = REGISTRATION.register, REGISTRATION.unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
