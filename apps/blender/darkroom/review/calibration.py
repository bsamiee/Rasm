"""Darkroom chart calibration: a color chart placed over an image or node preview, fitted against its reference, and written into the earliest Primaries entry's channel mixer."""

from pathlib import Path
from typing import Final

from bl_ui.space_image import IMAGE_PT_image_properties
from bl_ui.space_node import NODE_PT_active_node_generic
import bpy

from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator
from blender.bpy.data.rna.paths import rooted
from blender.bpy.render.compositor.inputs import entered, path_key
from blender.bpy.render.review.calibration import ChartCalibration
from blender.bpy.render.review.editing import ChartEditing
from blender.bpy.ui.elements.panels import Beside, Panel
from blender.bpy.ui.layout.rows import OperatorButton
from blender.records.faults import Outcome, paired

from ..catalog import chain
from ..effects.grade.colorimetry import MIXER, Primaries, REMAP

# --- [OPERATIONS] -----------------------------------------------------------------------


@Operator.of(Empty)
def apply_calibration(call: Call[Empty]) -> Outcome[Answer]:
    """Write calibration into scene primaries."""

    def mixed(entry: bpy.types.SceneCompositorEffect, rows: tuple[tuple[float, float, float], ...]) -> Outcome[bool]:
        return entered(call.identity, entry, {path_key(REMAP): MIXER.item.name, **{path_key(socket): row for socket, row in zip(MIXER.inputs, rows)}})

    return paired(calibration.fitting(call), rooted(bpy.types.Scene, call.context)).bind(
        lambda pair: chain.assigned(call.identity, pair[1], Primaries()).bind(
            lambda entry: mixed(entry, pair[0].calibration.color_matrix).map(lambda _: Done(notes=(*calibration.conditioned(pair[0].fit), ("INFO", f"Calibrated {entry.name}"))))
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

editing: Final = ChartEditing(horizontal=Path("flip.svg"), vertical=Path("flip-vertical.svg"))
calibration: Final = ChartCalibration(charts=editing, actions=(OperatorButton(target=apply_calibration),))
rows: Final = (editing, calibration, Panel(section=calibration.section(), at=Beside(IMAGE_PT_image_properties)), Panel(section=calibration.node_section(), at=Beside(NODE_PT_active_node_generic)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply_calibration", "calibration", "editing", "rows"]
