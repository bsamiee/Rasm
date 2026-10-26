"""Frame charts, metering, chart calibration, render slots, render history, and the review key map."""

from collections.abc import Callable
from pathlib import Path

from bl_ui.space_image import IMAGE_HT_header, IMAGE_PT_image_properties, IMAGE_PT_render_slots, IMAGE_PT_view_histogram
from bl_ui.space_node import NODE_PT_active_node_generic
import bpy
from expression import Ok, Some
import msgspec

from blender.bpy.commands.keymaps import Button, Chord, Keymap
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Unchanged
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.registration import Registrable
from blender.bpy.faults import BlenderResult
from blender.bpy.render.outputs.exports import image_type
from blender.bpy.render.review.calibration import ChartCalibration
from blender.bpy.render.review.frames import ClipEnds, Compare, FrameCharts, meter_pick, meter_region, Mode, RESULT, ReviewWindow
from blender.bpy.render.review.history import HistoryStore
from blender.bpy.render.sessions.renders import RenderSlots
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.surfaces.appends import Append
from blender.bpy.ui.surfaces.panels import Joined, Panel
from blender.bpy.ui.surfaces.tools import Appended, ImageEditor, Tool, Tools
from blender.imaging.parameters import Gated

from .effects import stack
from .preferences import reviewing

# --- [OPERATIONS] -----------------------------------------------------------------------


def screened[T: msgspec.Struct](key: str, title: str, description: str, attachment: Attachment[T], change: Callable[[T], T], condition: Condition) -> Operator[Empty]:
    """Returns command writing one change of the calling screen's record while `condition` holds."""

    def run(call: Call[Empty]) -> BlenderResult[Outcome]:
        return call.root(bpy.types.Screen).bind(lambda screen: attachment.revised(call.identity, screen, lambda held: Ok(change(held)))).map(lambda _: Done())

    return Operator(key=key, title=title, description=description, undo=False, form=Empty, run=run, condition=Some(condition))


def shown(mode: Mode) -> Callable[[Gated[Compare]], Gated[Compare]]:
    """Returns compare change showing the before frame in `mode`, hiding it when already shown so."""
    return lambda held: Gated(enabled=not (held.enabled and held.value.mode is mode), value=msgspec.structs.replace(held.value, mode=mode))


def laid(change: Callable[[Compare], Compare]) -> Callable[[Gated[Compare]], Gated[Compare]]:
    """Returns compare change of the shown layout."""
    return lambda held: msgspec.structs.replace(held, value=change(held.value))


def bypassed(call: Call[Empty]) -> BlenderResult[Outcome]:
    """Flips the scene's compositing, every effect running or none."""
    wrote = call.root(bpy.types.Scene).bind(lambda scene: written(scene.render, "use_compositing", not scene.render.use_compositing))
    return wrote.map(lambda changed: Done() if changed.is_some() else Unchanged(reason="Compositing holds its value"))


# --- [COMPOSITION] ----------------------------------------------------------------------

charts = FrameCharts(low="TRIA_LEFT", high="TRIA_RIGHT")
calibration = ChartCalibration(stack=stack, across=Path("flip.svg"), upright=Path("flip-upright.svg"))
tab = Joined.beside(IMAGE_PT_render_slots)
slots = RenderSlots(stack=stack)
history = HistoryStore(stack=stack, retention=reviewing, glyph=Path("vertical-wipe.svg"), scale=6)
window = ReviewWindow(key="review_window", title="Review Window", description="Open the render view with its sidebar of render history, calibration, and charts", tab=tab.category)
compare, framed, clips = history.compare, image_type(history.compare.kinds), FrameCharts.ends
keys = {
    Chord(type="BACK_SLASH"): screened(
        "compare_before", "Before and After", "Show or hide the before frame", compare.state, lambda held: msgspec.structs.replace(held, enabled=not held.enabled), framed
    ),
    Chord(type="Y"): screened("compare_side", "Side by Side", "Show the before frame beside the render", compare.state, shown(Mode.SIDE), framed),
    Chord(type="Y", shift=1): screened("compare_split", "Split", "Split the render with the before frame across a vertical wipe", compare.state, shown(Mode.VERTICAL), framed),
    Chord(type="W"): screened("compare_wipe", "Wipe", "Wipe the before frame over the render along a horizontal wipe", compare.state, shown(Mode.HORIZONTAL), framed),
    Chord(type="W", alt=1): screened(
        "compare_invert", "Invert Wipe", "Mirror the wipe across the frame", compare.state, laid(lambda held: msgspec.structs.replace(held, split=1.0 - held.split)), framed
    ),
    Chord(type="RET"): screened(
        "compare_swap", "Swap", "Swap the before frame and the render across the wipe", compare.state, laid(lambda held: msgspec.structs.replace(held, swapped=not held.swapped)), framed
    ),
    Chord(type="D", shift=1): Operator(key="effects_bypass", title="Bypass Effects", description="Run the scene's compositing or skip it", form=Empty, run=bypassed, condition=Some(framed)),
    Chord(type="O"): screened("clips_high", "Highlight Clipping", "Tint or clear pixels at the written file's highest code", clips, lambda ends: ClipEnds(low=ends.low, high=not ends.high), RESULT),
    Chord(type="U"): screened("clips_low", "Shadow Clipping", "Tint or clear pixels at the written file's lowest code", clips, lambda ends: ClipEnds(low=not ends.low, high=ends.high), RESULT),
}
rows: tuple[Registrable, ...] = (
    window,
    charts,
    Append(target=IMAGE_PT_view_histogram, rows=charts.clips),
    meter_region,
    meter_pick,
    calibration,
    Tools(
        toolbar=ImageEditor(mode="VIEW"),
        tools=(Tool(key="meter", title="Meter Region", operator=meter_pick, icon=Path("tools/region.svg"), bindings={Chord(type=Button.SELECT): meter_pick}),),
        placement=Appended(separator=True),
    ),
    slots,
    history,
    *keys.values(),
    Keymap(keymap="Image", bindings=keys),
    Append(target=IMAGE_HT_header, rows=compare.header),
    Panel(section=charts.section(), at=Joined.beside(IMAGE_PT_view_histogram)),
    Panel(section=calibration.section(), at=Joined.beside(IMAGE_PT_image_properties)),
    Panel(section=calibration.node_section(), at=Joined.beside(NODE_PT_active_node_generic)),
    Panel(section=slots.section(), at=tab),
    Panel(section=history.section(), at=tab),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["rows", "window"]
