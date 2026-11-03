"""Darkroom renders: frame size, scene paths, passes, finishing, and exports in their host panels, render sets over Darkroom's setup categories and chain entries, preflight, the queue with its temporal denoise follow-up, and render slots, every render staged through the chain."""

from typing import Final

from bl_ui.properties_freestyle import VIEWLAYER_PT_freestyle
from bl_ui.properties_output import RENDER_PT_output
from bl_ui.properties_render import RENDER_PT_color_management
from bl_ui.properties_view_layer import VIEWLAYER_PT_layer, VIEWLAYER_PT_layer_passes
from bl_ui.space_image import IMAGE_MT_image, IMAGE_PT_render_slots
from bl_ui.space_node import NODE_MT_node
from bl_ui.space_topbar import TOPBAR_MT_render
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
from cycles.presets import AddPresetIntegrator, AddPresetPerformance, AddPresetSampling, AddPresetViewportSampling
from cycles.ui import CYCLES_RENDER_PT_bake, CYCLES_RENDER_PT_passes
from expression import Some

from blender.bpy.commands.operators import PopupKind, PopupTarget
from blender.bpy.render.outputs.exports import Export
from blender.bpy.render.outputs.formats import format_listing
from blender.bpy.render.outputs.framing import camera_border, FrameSize, Framing, selection_border
from blender.bpy.render.outputs.paths import output_replace, output_version, ScenePaths
from blender.bpy.render.outputs.results import Finishing, rewrite
from blender.bpy.render.previews.tiles import Recipe
from blender.bpy.render.sessions.members import Category
from blender.bpy.render.sessions.preflight import Preflight
from blender.bpy.render.sessions.queue import QueueRun
from blender.bpy.render.sessions.renders import RenderSlots
from blender.bpy.render.sessions.sets import RenderSet
from blender.bpy.render.sessions.setups import EffectSetups
from blender.bpy.render.view_layers.freestyle import FREESTYLE
from blender.bpy.render.view_layers.layers import COLLECTIONS, LAYERS, OVERRIDES
from blender.bpy.render.view_layers.routing import RoutingRow
from blender.bpy.render.view_layers.sources import ids_export
from blender.bpy.ui.elements.menus import Append, PresetMenu
from blender.bpy.ui.elements.panels import Beside, Child, Panel, PanelCopy
from blender.bpy.ui.layout.rows import Opens, OperatorButton

from ..catalog import chain, render, stacks, tiles
from ..effects.finish.dither import diffuse
from ..effects.grade.exposure import gain
from .temporal import temporal_denoise

# --- [CONSTANTS] ------------------------------------------------------------------------

categories: Final = (
    Category(
        key="sampling",
        settings=(
            ("render", "engine"),
            ("cycles", "device"),
            *AddPresetSampling.preset_values,
            *AddPresetViewportSampling.preset_values,
            ("cycles", "denoising_use_gpu"),
            ("cycles", "preview_denoising_use_gpu"),
            ("eevee",),
        ),
    ),
    Category(key="light_paths", settings=(*AddPresetIntegrator.preset_values, ("cycles", "sample_clamp_direct"), ("cycles", "sample_clamp_indirect"))),
    Category(
        key="simplify",
        settings=(
            ("render", "use_simplify"),
            ("render", "simplify_subdivision_render"),
            ("render", "simplify_child_particles_render"),
            ("render", "simplify_volumes"),
            ("cycles", "dicing_rate"),
            ("cycles", "preview_dicing_rate"),
            ("cycles", "offscreen_dicing_scale"),
            ("cycles", "max_subdivisions"),
        ),
    ),
    Category(key="motion_blur", settings=(("render", "use_motion_blur"), ("render", "motion_blur_shutter"), ("render", "motion_blur_position"), ("render", "motion_blur_shutter_curve"))),
    Category(
        key="film",
        settings=(
            ("render", "film_transparent"),
            ("render", "filter_size"),
            ("cycles", "film_exposure"),
            ("cycles", "film_transparent_glass"),
            ("cycles", "film_transparent_roughness"),
            ("cycles", "pixel_filter_type"),
            ("cycles", "filter_width"),
        ),
    ),
    Category(key="performance", settings=(*AddPresetPerformance.preset_values, ("cycles", "use_auto_tile"), ("render", "compositor_device"))),
    Category(key="color_management", settings=(("display_settings",), ("view_settings",), ("sequencer_colorspace_settings",))),
    Category(key="resolution", settings=tuple(("render", member) for member in Framing.native)),
    Category(key="output_format", settings=(("render", "image_settings"),)),
)

# --- [COMPOSITION] ----------------------------------------------------------------------

passes: Final = RoutingRow()
export: Final = Export(gain=gain, layering=(stacks.layering, RoutingRow.layering))
render_sets: Final = RenderSet(capture=EffectSetups(effects=chain, tile=Some(lambda identity, slot, entries: tiles.icon(identity, slot, Recipe(entries=entries)))), categories=categories)
checks: Final = Preflight()
batch: Final = QueueRun(sets=render_sets, render=render.rendered, planning=Some(temporal_denoise.planned))
slots: Final = RenderSlots(render=render.rendered)
rows: Final = (
    FrameSize(),
    ScenePaths(),
    passes,
    Finishing(rows=(rewrite, diffuse, ids_export), files=passes.written),
    export,
    PresetMenu(source=format_listing),
    render_sets,
    checks,
    batch,
    temporal_denoise,
    slots,
    Append(target=RENDER_PT_output, rows=(Opens(target=PopupTarget(format_listing.key, PopupKind.MENU)),)),
    Append(target=IMAGE_MT_image, rows=(OperatorButton(target=export.linear), OperatorButton(target=export.still), *(OperatorButton(target=command) for command in export.layered))),
    Append(target=NODE_MT_node, rows=(OperatorButton(target=output_replace), OperatorButton(target=output_version))),
    Append(target=VIEW3D_MT_object_context_menu, rows=(OperatorButton(target=camera_border), OperatorButton(target=selection_border))),
    Append(target=CYCLES_RENDER_PT_bake, rows=(RenderSlots.bake_presets,)),
    Append(
        target=TOPBAR_MT_render,
        rows=(Opens(target=PopupTarget(RenderSet.key, PopupKind.MENU)), OperatorButton(target=batch.dispatching), *(OperatorButton(target=command) for command in batch.controls)),
    ),
    Panel(section=ScenePaths.section, at=Child(parent=RENDER_PT_output)),
    Panel(section=passes.section(), at=Child(parent=VIEWLAYER_PT_layer_passes), copies=(PanelCopy(at=Child(parent=CYCLES_RENDER_PT_passes)),)),
    Panel(section=LAYERS, at=Child(parent=VIEWLAYER_PT_layer), children=(OVERRIDES, COLLECTIONS)),
    Panel(section=FREESTYLE, at=Child(parent=VIEWLAYER_PT_freestyle)),
    Panel(section=render_sets.section, at=Beside(RENDER_PT_color_management)),
    Panel(section=batch.section, at=Beside(RENDER_PT_color_management), children=(checks.section, temporal_denoise.section)),
    Panel(section=slots.section, at=Beside(IMAGE_PT_render_slots)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["batch", "categories", "checks", "export", "passes", "render_sets", "rows", "slots"]
