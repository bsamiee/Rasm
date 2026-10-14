"""Frame charts, metering, calibration, render controls, and persistent history."""

from pathlib import Path

from bl_ui.space_image import IMAGE_HT_header, IMAGE_PT_image_properties, IMAGE_PT_render_slots, IMAGE_PT_view_histogram
from bl_ui.space_node import NODE_PT_active_node_generic

from blender.bpy.commands.keymaps import Button, Chord
from blender.bpy.render.review.calibration import ChartCalibration
from blender.bpy.render.review.frames import FrameCharts, meter_pick, meter_region, read_pick
from blender.bpy.render.review.history import HistoryStore
from blender.bpy.render.sessions.renders import RenderSlots
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Joined, Mirror, Panel, Sidebar
from blender.bpy.ui.surfaces.tools import Appended, ImageEditor, Tool, Tools

from .effects import stack, viewer
from .preferences import category, reviewing

# --- [COMPOSITION] ----------------------------------------------------------------------

charts = FrameCharts(low="TRIA_LEFT", high="TRIA_RIGHT")
calibration = ChartCalibration(stack=stack, across=Path("flip.svg"), upright=Path("flip-upright.svg"))
tab = Joined(space=IMAGE_PT_render_slots.bl_space_type, category=IMAGE_PT_render_slots.bl_category)
slots = RenderSlots(stack=stack)
history = HistoryStore(settings=reviewing, glyph=Path("vertical-wipe.svg"), scale=6)
rows = (
    charts,
    Append(
        target=IMAGE_PT_view_histogram,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=charts.clips,
    ),
    meter_region,
    meter_pick,
    read_pick(viewer),
    calibration,
    Tools(
        toolbar=ImageEditor(mode="VIEW"),
        tools=(Tool(key="meter", title="Meter Region", operator=meter_pick, icon=Path("tools/region.svg"), bindings={Chord(type=Button.SELECT): meter_pick}),),
        placement=Appended(separator=True),
    ),
    slots,
    history,
    Append(
        target=IMAGE_HT_header,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        rows=history.compare.header,
    ),
)
panels = (
    Panel(section=charts.section(), at=Joined(space=IMAGE_PT_view_histogram.bl_space_type, category=IMAGE_PT_view_histogram.bl_category)),
    Panel(section=calibration.section(), at=Joined(space=IMAGE_PT_image_properties.bl_space_type, category=IMAGE_PT_image_properties.bl_category)),
    Panel(section=calibration.node_section(), at=Joined(space=NODE_PT_active_node_generic.bl_space_type, category=NODE_PT_active_node_generic.bl_category)),
    Panel(section=slots.section(), at=tab, mirrors=(Mirror(at=Sidebar(space="VIEW_3D", category=category)),)),
    Panel(section=history.section(), at=tab),
)
page_rows = (*charts.page_rows, *calibration.page_rows, *slots.page_rows, *history.page_rows)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["page_rows", "panels", "rows"]
