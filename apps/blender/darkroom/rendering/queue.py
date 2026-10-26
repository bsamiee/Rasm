"""Darkroom render queue: render sets over the setup capture, preflight audits, and the queue rendering through the chain with its temporal denoise follow-up."""

from typing import Final

from bl_ui.properties_render import RENDER_PT_color_management
from bl_ui.space_topbar import TOPBAR_MT_render
from expression import Some

from blender.bpy.render.sessions.preflight import Preflight
from blender.bpy.render.sessions.queue import QueueRun
from blender.bpy.render.sessions.sets import RenderSet
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import OperatorButton, Pulldown
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Panel, Tab
from blender.records.fields import heading

from ..catalog import chain, tiles
from ..effects.optics.denoise import SETTINGS, temporal, TemporalDenoise
from ..preferences import queueing
from .sets import categories, Setups

# --- [COMPOSITION] ----------------------------------------------------------------------

render_sets: Final = RenderSet(capture=Setups(chain=chain, tiles=tiles, categories=categories))
checks: Final = Preflight()
batch: Final = QueueRun(sets=render_sets, settings=queueing, render=chain.rendered, follow=(temporal,))
rows: Final = (
    render_sets,
    checks,
    batch,
    Append(
        target=TOPBAR_MT_render,
        rows=(Pulldown(menu=RenderSet.menu), *(OperatorButton(target=command) for command in (batch.commands.dispatch, batch.commands.resume, batch.commands.stop, batch.commands.log))),
    ),
    Panel(section=render_sets, at=Tab.beside(RENDER_PT_color_management)),
    Panel(
        section=batch.section,
        at=Tab.beside(RENDER_PT_color_management),
        children=(
            checks.section(),
            Single(key=SETTINGS.key, title=heading(TemporalDenoise.__name__), store=Some(Hosted(store=Store(attachment=SETTINGS, presets=Some(Preset(key=SETTINGS.key, form=TemporalDenoise)))))),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["batch", "render_sets", "rows"]
