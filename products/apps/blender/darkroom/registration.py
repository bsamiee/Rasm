"""Darkroom registration over every row the extension declares: preferences, effects, renders, color, and review."""

from rasm.extension.data import Registration
from rasm.extension.ui import PreferencesPage

from . import catalog
from .color import looks, luts, response
from .render import renders
from .review import calibration, metering, results

# --- [COMPOSITION] ----------------------------------------------------------------------

_REGISTRATION = Registration(__name__.rpartition(".")[0], (PreferencesPage(), *catalog.rows, *renders.rows, *luts.rows, *looks.rows, *response.rows, *metering.rows, *calibration.rows, *results.rows))
register, unregister = _REGISTRATION.register, _REGISTRATION.unregister

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
