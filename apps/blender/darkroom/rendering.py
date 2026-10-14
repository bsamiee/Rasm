"""Render settings, view layers, output files, and batch rendering."""

from bl_ui.properties_data_camera import DATA_PT_camera_safe_areas
from bl_ui.properties_freestyle import VIEWLAYER_PT_freestyle
from bl_ui.properties_output import RENDER_PT_output
from bl_ui.properties_view_layer import VIEWLAYER_PT_layer, VIEWLAYER_PT_layer_passes
from bl_ui.space_image import IMAGE_MT_image
from bl_ui.space_node import NODE_HT_header, NODE_MT_node
from bl_ui.space_statusbar import STATUSBAR_HT_header
from bl_ui.space_topbar import TOPBAR_MT_render
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from cycles.presets import AddPresetIntegrator, AddPresetPerformance, AddPresetSampling, AddPresetViewportSampling  # type: ignore[import-not-found]  # ty: ignore[unresolved-import]
from cycles.ui import CYCLES_RENDER_PT_passes  # type: ignore[import-not-found]  # ty: ignore[unresolved-import]
from expression import Some

from blender.bpy.commands.operators import Acquired, Own
from blender.bpy.render.effects.compositing.layers import COMPOSITED, LayerStacks, Source
from blender.bpy.render.effects.quantization import diffuse
from blender.bpy.render.effects.sections import COMPOSITING
from blender.bpy.render.effects.stack import formation
from blender.bpy.render.effects.tone.exposure import exposure
from blender.bpy.render.outputs.exports import Export, Layering
from blender.bpy.render.outputs.finishing import Finishing, rewrite
from blender.bpy.render.outputs.formats import format_listing, output_rows
from blender.bpy.render.outputs.framing import FrameSize, safe_broadcast, safe_listing, Standard
from blender.bpy.render.outputs.paths import Outputs, ScenePaths
from blender.bpy.render.sessions.preflight import Preflight
from blender.bpy.render.sessions.queue import Denoising, QueueRun
from blender.bpy.render.sessions.sets import Category, RenderSet, Setups
from blender.bpy.render.view_layers.collections import CollectionStates
from blender.bpy.render.view_layers.freestyle import FreestyleSection
from blender.bpy.render.view_layers.overrides import LayerOverride
from blender.bpy.render.view_layers.passes import ids_export, PassSet
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Member, OperatorButton, Pulldown
from blender.bpy.ui.layout.sections import Hosted, Single, Store
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.menus import PresetMenu
from blender.bpy.ui.surfaces.panels import Child, Mirror, Panel, PopoverPanel, Sidebar, Tab
from blender.imaging.compositing.layers import LayerStack
from blender.rendering.outputs.framing import Broadcast

from .effects import Effects, stack, tiles
from .preferences import category, queueing

# --- [COMPOSITION] ----------------------------------------------------------------------

# --- [SETTINGS]

render_sets = RenderSet(
    categories=(
        Category(
            key="sampling",
            paths=(
                "render.engine",
                "cycles.device",
                *AddPresetSampling.preset_values,
                *AddPresetViewportSampling.preset_values,
                "cycles.denoising_use_gpu",
                "cycles.preview_denoising_use_gpu",
                "eevee",
            ),
        ),
        Category(key="light_paths", paths=(*AddPresetIntegrator.preset_values, "cycles.sample_clamp_direct", "cycles.sample_clamp_indirect")),
        Category(
            key="simplify",
            paths=(
                "render.use_simplify",
                "render.simplify_subdivision_render",
                "render.simplify_child_particles_render",
                "render.simplify_volumes",
                "cycles.dicing_rate",
                "cycles.preview_dicing_rate",
                "cycles.offscreen_dicing_scale",
                "cycles.max_subdivisions",
            ),
        ),
        Category(key="motion_blur", paths=("render.use_motion_blur", "render.motion_blur_shutter", "render.motion_blur_position", "render.motion_blur_shutter_curve")),
        Category(
            key="film",
            paths=(
                "render.film_transparent",
                "render.filter_size",
                "cycles.film_exposure",
                "cycles.film_transparent_glass",
                "cycles.film_transparent_roughness",
                "cycles.pixel_filter_type",
                "cycles.filter_width",
            ),
        ),
        Category(key="performance", paths=(*AddPresetPerformance.preset_values, "cycles.use_auto_tile", "render.compositor_device")),
        Category(key="color_management", paths=("display_settings", "view_settings", "sequencer_colorspace_settings")),
        Category(key="resolution", paths=tuple(f"render.{member}" for member in FrameSize.members)),
        Category(key="output_format", paths=("render.image_settings",)),
    ),
    setup=Some(Setups(stack=stack, tiles=tiles, main=AreaQuery(space="IMAGE_EDITOR"))),
)

# --- [LAYERS]

view_layers = LayerOverride()
states = CollectionStates()
lines = FreestyleSection()
stacks = LayerStacks(form=LayerStack[Source, Effects], stack=stack)
passes = PassSet()

# --- [FILES]

paths = ScenePaths()
framing = FrameSize()
layering = (
    Layering(key="export_layers", title="Export Layered Document", description="Writes the displayed render's layer stack as a layered TIFF an image editor opens", source=stacks),
    Layering(key="export_ids", title="Export ID Document", description="Writes one layer per object or material ID of the displayed render as a layered TIFF", source=passes),
)

# --- [QUEUE]

checks = Preflight()
batch = QueueRun(sets=render_sets, stack=stack, settings=queueing)
temporal = Preset(key="temporal_denoise", form=Denoising)
output_paths = Preset(key="output_paths", form=Outputs)

# --- [REGISTRATION]

rows = (
    framing,
    paths,
    view_layers,
    states,
    lines,
    stacks,
    passes,
    Finishing(rows=(rewrite, diffuse, ids_export), files=passes),
    Export(viewing=formation, gain=exposure, layering=layering),
    PresetMenu(source=format_listing),
    PresetMenu(source=safe_listing, entries=tuple(OperatorButton(target=Own(command=safe_broadcast, values=Standard(standard=each)), text=Some(each.label)) for each in Broadcast)),
    render_sets,
    checks,
    batch,
    Append(
        target=RENDER_PT_output,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=(Pulldown(menu=format_listing.key), output_rows),
    ),
    Append(
        target=DATA_PT_camera_safe_areas,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=(Pulldown(menu=safe_listing.key),),
    ),
    Append(
        target=IMAGE_MT_image,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=(OperatorButton(target=Acquired(key=Export.dng)), OperatorButton(target=Acquired(key=Export.image)), *(OperatorButton(target=Acquired(key=each.key)) for each in layering)),
    ),
    Append(
        target=NODE_MT_node,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=(OperatorButton(target=Acquired(key=ScenePaths.replace)), OperatorButton(target=Acquired(key=ScenePaths.version))),
    ),
    Append(
        target=VIEW3D_MT_object_context_menu,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=(OperatorButton(target=Acquired(key=FrameSize.camera_border)), OperatorButton(target=Acquired(key=FrameSize.selection_border))),
    ),
    Append(
        target=TOPBAR_MT_render,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=(Pulldown(menu=RenderSet.menu),),
    ),
    Append(
        target=TOPBAR_MT_render,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=tuple(OperatorButton(target=Acquired(key=key)) for key in (QueueRun.dispatch, QueueRun.resume, QueueRun.stop, QueueRun.log)),
    ),
    Append(
        target=STATUSBAR_HT_header,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=checks.measures,
    ),
    Append(
        target=NODE_HT_header,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=checks.counts,
    ),
)
panels = (
    Panel(
        section=Single(
            key=output_paths.key,
            title="Output Paths",
            rows=(OperatorButton(target=Acquired(key=ScenePaths.take)), OperatorButton(target=Acquired(key=ScenePaths.folder))),
            store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=paths.attachment, presets=Some(output_paths)))),
        ),
        at=Child(
            parent=RENDER_PT_output  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        ),
    ),
    Panel(
        section=passes.section(),
        at=Child(
            parent=VIEWLAYER_PT_layer_passes  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        ),
        mirrors=(Mirror(at=Child(parent=CYCLES_RENDER_PT_passes)),),
    ),
    Panel(section=render_sets, at=Tab(tab="RENDER"), mirrors=(Mirror(at=Sidebar(space="VIEW_3D", category=category)),)),
    Panel(
        section=view_layers.section(),
        at=Child(
            parent=VIEWLAYER_PT_layer  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        ),
        children=(view_layers.overrides(), states.section()),
    ),
    Panel(
        section=lines.section(),
        at=Child(
            parent=VIEWLAYER_PT_freestyle  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        ),
    ),
    Panel(
        section=stacks.section(),
        at=Sidebar(space="NODE_EDITOR", category=category),
        condition=Some(COMPOSITING),
        mirrors=(Mirror(at=Sidebar(space="VIEW_3D", category=category)), Mirror(at=Sidebar(space="IMAGE_EDITOR", category=category), condition=Some(COMPOSITED))),
    ),
    Panel(section=stacks.presets(), at=PopoverPanel()),
    Panel(
        section=batch,
        at=Tab(tab="RENDER"),
        mirrors=(Mirror(at=Sidebar(space="VIEW_3D", category=category)),),
        children=(
            checks.section(),
            Single(key=temporal.key, title="Temporal Denoise", store=Some(Hosted(host=Member(root=bpy.types.Context, path="scene"), store=Store(attachment=QueueRun.denoise, presets=Some(temporal))))),
        ),
    ),
)
page_rows = (*paths.page_rows, *framing.page_rows, *passes.page_rows, *render_sets.page_rows, *view_layers.page_rows, *stacks.page_rows, *batch.page_rows)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["batch", "page_rows", "panels", "rows"]
