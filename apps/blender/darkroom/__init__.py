"""Darkroom extension registration."""

from pathlib import Path

from expression import Some

from blender.bpy.extension.registration import scope
from blender.bpy.nodes.placement import Arranger
from blender.bpy.render.previews.queue import PreviewQueue
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.resources.icons import Icons, ink
from blender.bpy.ui.surfaces.panels import Panels, Sidebar
from blender.bpy.ui.surfaces.preferences import PreferencesPage, show

from . import color, effects, preferences, rendering, review

# --- [COMPOSITION] ----------------------------------------------------------------------

modules = (color, effects, rendering, review)
page = PreferencesPage(record=preferences.Settings, sections=preferences.sections, rows=tuple(row for module in modules for row in module.page_rows), bindings=rendering.batch.bindings)
register, unregister = scope(
    __name__,
    (
        Icons(brand=Some(Path("mark.svg")), tint=lambda _: Some(ink())),
        page,
        PreviewQueue(),
        Arranger(),
        *(row for module in modules for row in module.rows),
        Panels(panels=tuple(panel for module in modules for panel in module.panels), page=Some(page), header={Sidebar: (OperatorButton(target=show),)}),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["register", "unregister"]
