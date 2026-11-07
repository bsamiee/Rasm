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
from cycles.presets import AddPresetIntegrator, AddPresetPerformance, AddPresetSampling, AddPresetViewportSampling  # type: ignore[import-not-found]  # ty: ignore[unresolved-import]
from cycles.ui import CYCLES_RENDER_PT_bake, CYCLES_RENDER_PT_passes  # type: ignore[import-not-found]  # ty: ignore[unresolved-import]
from expression import Some

from rasm_bpy.commands import PopupKind, PopupTarget
from rasm_bpy.render import (
    camera_border,
    Category,
    COLLECTIONS,
    EffectSetups,
    Export,
    Finishing,
    format_listing,
    FrameSize,
    Framing,
    FREESTYLE,
    ids_export,
    LAYERS,
    output_replace,
    output_version,
    OVERRIDES,
    Preflight,
    QueueRun,
    RenderSet,
    RenderSlots,
    rewrite,
    RoutingRow,
    ScenePaths,
    selection_border,
)
from rasm_bpy.ui import Append, Beside, Child, Opens, OperatorButton, Panel, PanelCopy, PresetMenu

from ..catalog import chain, render, stacks, tiles
from ..effects.finish.dither import diffuse
from ..review.results import history
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
export: Final = Export(selected=history.history.selected, layering=(stacks.layering, RoutingRow.layering))
render_sets: Final = RenderSet(capture=EffectSetups(effects=chain, tile=Some(tiles.icon)), categories=categories)
checks: Final = Preflight()
batch: Final = QueueRun(sets=render_sets, render=render.rendered, planning=Some(temporal_denoise.planned), preparing=Some(temporal_denoise.prepared))
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
