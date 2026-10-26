"""Render settings, view layers, output files, and batch rendering."""

from bl_ui.properties_compositor import SCENE_PT_compositor_effects
from bl_ui.properties_data_camera import DATA_PT_camera_safe_areas
from bl_ui.properties_freestyle import VIEWLAYER_PT_freestyle
from bl_ui.properties_output import RENDER_PT_output
from bl_ui.properties_render import RENDER_PT_color_management
from bl_ui.properties_view_layer import VIEWLAYER_PT_layer, VIEWLAYER_PT_layer_passes
from bl_ui.space_image import IMAGE_MT_image
from bl_ui.space_node import NODE_MT_node
from bl_ui.space_topbar import TOPBAR_MT_render
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
from cycles.presets import AddPresetIntegrator, AddPresetPerformance, AddPresetSampling, AddPresetViewportSampling
from cycles.ui import CYCLES_RENDER_PT_passes
from expression import Some

from blender.bpy.commands.operators import Own
from blender.bpy.render.effects.quantization import diffuse
from blender.bpy.render.effects.tone.exposure import exposure
from blender.bpy.render.outputs.exports import Export, Layering
from blender.bpy.render.outputs.finishing import Finishing, rewrite
from blender.bpy.render.outputs.formats import format_listing, output_rows
from blender.bpy.render.outputs.framing import Broadcast, FrameSize, Framing, safe_broadcast, safe_listing, Standard
from blender.bpy.render.outputs.paths import Outputs, ScenePaths
from blender.bpy.render.sessions.preflight import Preflight
from blender.bpy.render.sessions.queue import Denoising, QueueRun
from blender.bpy.render.sessions.sets import Category, RenderSet, Setups
from blender.bpy.render.view_layers.collections import CollectionStates
from blender.bpy.render.view_layers.freestyle import FreestyleSection
from blender.bpy.render.view_layers.overrides import LayerOverride
from blender.bpy.render.view_layers.passes import ids_export, PassSet
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import OperatorButton, Pulldown
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Store
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.menus import PresetMenu
from blender.bpy.ui.surfaces.panels import Child, Mirror, Panel, PopoverPanel, Tab

from .effects import stack, stacks, tiles
from .preferences import queueing
from .review import window

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
        Category(key="resolution", paths=tuple(f"render.{member}" for member in Framing.native)),
        Category(key="output_format", paths=("render.image_settings",)),
    ),
    setup=Some(Setups(stack=stack, tiles=tiles, scale=6)),
)

# --- [LAYERS]

view_layers = LayerOverride()
states = CollectionStates()
lines = FreestyleSection()
passes = PassSet()

# --- [FILES]

paths = ScenePaths()
framing = FrameSize()
layering = (
    Layering(key="export_layers", title="Export Layered Document", description="Write layer stack of displayed render as layered TIFF", source=stacks),
    Layering(key="export_ids", title="Export ID Document", description="Write object or material ID layers of displayed render as layered TIFF", source=passes),
)

# --- [QUEUE]

checks = Preflight()
batch = QueueRun(sets=render_sets, stack=stack, settings=queueing)
export = Export(gain=exposure, layering=layering)
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
    export,
    PresetMenu(source=format_listing),
    PresetMenu(source=safe_listing, entries=tuple(OperatorButton(target=Own(command=safe_broadcast, values=Standard(standard=each)), text=each.label) for each in Broadcast)),
    render_sets,
    checks,
    batch,
    Append(target=RENDER_PT_output, rows=(Pulldown(menu=format_listing.key), output_rows)),
    Append(target=DATA_PT_camera_safe_areas, rows=(Pulldown(menu=safe_listing.key),)),
    Append(target=IMAGE_MT_image, rows=(OperatorButton(target=export.linear), OperatorButton(target=export.still), *(OperatorButton(target=command) for command in export.layered))),
    Append(target=NODE_MT_node, rows=(OperatorButton(target=ScenePaths.replacing), OperatorButton(target=ScenePaths.versioning))),
    Append(target=VIEW3D_MT_object_context_menu, rows=(OperatorButton(target=FrameSize.camera_bordering), OperatorButton(target=FrameSize.selection_bordering))),
    Append(
        target=TOPBAR_MT_render,
        rows=(
            Pulldown(menu=RenderSet.menu),
            *(OperatorButton(target=command) for command in (batch.commands.dispatch, batch.commands.resume, batch.commands.stop, batch.commands.log, window.command)),
        ),
    ),
    Panel(
        section=Single(
            key=output_paths.key,
            title="Output Paths",
            rows=(OperatorButton(target=ScenePaths.taking), OperatorButton(target=ScenePaths.opening)),
            store=Some(Hosted(store=Store(attachment=paths.attachment, presets=Some(output_paths)))),
        ),
        at=Child(parent=RENDER_PT_output),
    ),
    Panel(section=passes.section(), at=Child(parent=VIEWLAYER_PT_layer_passes), mirrors=(Mirror(at=Child(parent=CYCLES_RENDER_PT_passes)),)),
    Panel(section=render_sets, at=Tab.beside(RENDER_PT_color_management)),
    Panel(section=view_layers.section(), at=Child(parent=VIEWLAYER_PT_layer), children=(view_layers.overrides(), states.section())),
    Panel(section=lines.section(), at=Child(parent=VIEWLAYER_PT_freestyle)),
    Panel(section=stacks.section(), at=Tab.beside(SCENE_PT_compositor_effects)),
    Panel(section=stacks.presets(), at=PopoverPanel()),
    Panel(
        section=batch.section,
        at=Tab.beside(RENDER_PT_color_management),
        children=(checks.section(), Single(key=temporal.key, title="Temporal Denoise", store=Some(Hosted(store=Store(attachment=QueueRun.denoise, presets=Some(temporal)))))),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["batch", "rows"]
