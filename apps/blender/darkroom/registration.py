"""Darkroom registration over every row the extension declares: icons, preferences, effects, renders, color, and review."""

from rasm_bpy.data import Registration
from rasm_bpy.ui import Icons, PreferencesPage

from . import catalog
from .color import looks, luts, response
from .rendering import renders
from .review import calibration, metering, results

# --- [COMPOSITION] ----------------------------------------------------------------------

_REGISTRATION = Registration(
    __name__.rpartition(".")[0],
    (Icons(turned=calibration.turned), PreferencesPage(), *catalog.rows, *renders.rows, *luts.rows, *looks.rows, *response.rows, *metering.rows, *calibration.rows, *results.rows),
)
register, unregister = _REGISTRATION.register, _REGISTRATION.unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
