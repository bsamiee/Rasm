"""Darkroom render queue: render sets over the setup capture, preflight audits, and the queue rendering through the chain with its temporal denoise follow-up."""

from typing import Final

from bl_ui.properties_render import RENDER_PT_color_management
from bl_ui.space_topbar import TOPBAR_MT_render
import bpy
from expression import Some

from blender.bpy.commands.calls import Called, Opened
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.render.sessions.preflight import Preflight
from blender.bpy.render.sessions.queue import QueueRun
from blender.bpy.render.sessions.sets import RenderSet
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Opens, OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.surfaces.menus import Append
from blender.bpy.ui.surfaces.panels import Beside, Panel

from ..catalog import chain, tiles
from ..effects.optics.denoise import prerequisites, SETTINGS, temporal
from ..preferences import queueing
from .sets import categories, Setups

# --- [COMPOSITION] ----------------------------------------------------------------------

render_sets: Final = RenderSet(capture=Setups(chain=chain, tiles=tiles, categories=categories))
checks: Final = Preflight()
batch: Final = QueueRun(sets=render_sets, settings=queueing, render=chain.rendered, planning=Some(temporal))
rows: Final = (
    render_sets,
    checks,
    batch,
    *prerequisites,
    Append(target=TOPBAR_MT_render, rows=(Opens(target=Opened(RenderSet.key, Called.MENU)), OperatorButton(target=batch.dispatching), *(OperatorButton(target=command) for command in batch.controls))),
    Panel(section=render_sets.section, at=Beside(RENDER_PT_color_management)),
    Panel(
        section=batch.section,
        at=Beside(RENDER_PT_color_management),
        children=(
            checks.section(),
            Section(key=SETTINGS.key, title=bpy.path.display_name(SETTINGS.key), store=Some(Store(attachment=SETTINGS, presets=Some(Preset(key=SETTINGS.key, form=Snapshot))))),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["batch", "render_sets", "rows"]
