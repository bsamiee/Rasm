"""Darkroom native extension registration."""

from pathlib import Path

from expression import Some

from rasm.blender.extension.registration import scope
from rasm.blender.ui.layout.rows import OperatorButton
from rasm.blender.ui.resources.icons import Icons, ink
from rasm.blender.ui.surfaces.panels import Panels, Sidebar
from rasm.blender.ui.surfaces.preferences import PreferencesPage, show

from . import color, effects, preferences, rendering, review

# --- [COMPOSITION] ----------------------------------------------------------------------

modules = (color, effects, rendering, review)
page = PreferencesPage(record=preferences.Settings, sections=preferences.sections, rows=tuple(row for module in modules for row in module.page_rows), bindings=rendering.batch.bindings)
register, unregister = scope(
    __name__,
    (
        Icons(brand=Some(Path("mark.svg")), tint=lambda _: Some(ink())),
        page,
        effects.preview_queue,
        *(row for module in modules for row in module.rows),
        Panels(panels=tuple(panel for module in modules for panel in module.panels), page=Some(page), header={Sidebar: (OperatorButton(target=show),)}),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
