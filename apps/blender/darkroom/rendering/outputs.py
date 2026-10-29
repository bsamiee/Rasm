"""Darkroom outputs: frame size, scene paths, view layer, collection and line set editors, passes, finishing, exports through the chain gain, and their panel placements."""

from typing import Final

from bl_ui.properties_freestyle import VIEWLAYER_PT_freestyle
from bl_ui.properties_output import RENDER_PT_output
from bl_ui.properties_view_layer import VIEWLAYER_PT_layer, VIEWLAYER_PT_layer_passes
from bl_ui.space_image import IMAGE_MT_image
from bl_ui.space_node import NODE_MT_node
from bl_ui.space_view3d import VIEW3D_MT_object_context_menu
import bpy
from cycles.ui import CYCLES_RENDER_PT_bake, CYCLES_RENDER_PT_passes
from expression import Some

from blender.bpy.commands.calls import Called, Opened
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.render.outputs.exports import Export, Layering
from blender.bpy.render.outputs.finishing import Finishing, rewrite
from blender.bpy.render.outputs.formats import format_listing
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.paths import ScenePaths
from blender.bpy.render.sessions.renders import RenderSlots
from blender.bpy.render.view_layers.collections import CollectionStates
from blender.bpy.render.view_layers.freestyle import FreestyleSection
from blender.bpy.render.view_layers.ids import document, ids_export
from blender.bpy.render.view_layers.overrides import LayerOverride
from blender.bpy.render.view_layers.routing import PassSet
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Opens, OperatorButton
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.surfaces.menus import Append, PresetMenu
from blender.bpy.ui.surfaces.panels import Child, Panel, PanelCopy

from ..catalog import chain, stacks
from ..effects.finish.dither import diffuse

# --- [COMPOSITION] ----------------------------------------------------------------------

view_layers: Final = LayerOverride()
states: Final = CollectionStates()
lines: Final = FreestyleSection()
passes: Final = PassSet()
paths: Final = ScenePaths()
framing: Final = FrameSize()
export: Final = Export(gain=chain.gain, layering=(Layering(key="layer_stacks", document=stacks.document), Layering(key=PassSet.key, document=document)))
output_paths: Final = Preset(key="output_paths", form=Snapshot)
rows: Final = (
    framing,
    paths,
    view_layers,
    states,
    lines,
    passes,
    Finishing(rows=(rewrite, diffuse, ids_export), files=passes),
    export,
    PresetMenu(source=format_listing),
    Append(target=RENDER_PT_output, rows=(Opens(target=Opened(format_listing.key, Called.MENU)),)),
    Append(target=IMAGE_MT_image, rows=(OperatorButton(target=export.linear), OperatorButton(target=export.still), *(OperatorButton(target=command) for command in export.layered))),
    Append(target=NODE_MT_node, rows=(OperatorButton(target=ScenePaths.replacing), OperatorButton(target=ScenePaths.versioning))),
    Append(target=VIEW3D_MT_object_context_menu, rows=(OperatorButton(target=FrameSize.camera_bordering), OperatorButton(target=FrameSize.selection_bordering))),
    Append(target=CYCLES_RENDER_PT_bake, rows=(RenderSlots.bake_presets,)),
    Panel(
        section=Section(
            key=output_paths.key,
            title=bpy.path.display_name(output_paths.key),
            rows=(OperatorButton(target=ScenePaths.taking), OperatorButton(target=ScenePaths.opening)),
            store=Some(Store(attachment=ScenePaths.attachment, presets=Some(output_paths))),
        ),
        at=Child(parent=RENDER_PT_output),
    ),
    Panel(section=passes.section(), at=Child(parent=VIEWLAYER_PT_layer_passes), copies=(PanelCopy(at=Child(parent=CYCLES_RENDER_PT_passes)),)),
    Panel(section=view_layers.section(), at=Child(parent=VIEWLAYER_PT_layer), children=(view_layers.overrides(), states.section())),
    Panel(section=lines.section(), at=Child(parent=VIEWLAYER_PT_freestyle)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["export", "passes", "rows"]
