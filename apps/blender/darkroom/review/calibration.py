"""Darkroom chart calibration: a color chart placed over an image or node preview, fitted against its reference, and written into the earliest Primaries entry's channel mixer."""

from pathlib import Path
from types import MappingProxyType
from typing import Final

from bl_ui.space_image import IMAGE_PT_image_properties
from bl_ui.space_node import NODE_PT_active_node_generic
import bpy

from rasm_bpy.commands import Answer, Call, Done, Empty, Operator
from rasm_bpy.data import rooted
from rasm_bpy.render import ChartCalibration, ChartEditing, entered, path_key
from rasm_bpy.ui import Beside, OperatorButton, Panel
from rasm_records import Outcome, paired

from ..catalog import chain
from ..effects.grade.colorimetry import MIXER, Primaries, REMAP

# --- [CONSTANTS] ------------------------------------------------------------------------

_FLIP: Final = Path("flip.svg")
_VERTICAL: Final = Path("flip-vertical.svg")
turned: Final = MappingProxyType({_VERTICAL: _FLIP})

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

editing: Final = ChartEditing(horizontal=_FLIP, vertical=_VERTICAL)
calibration: Final = ChartCalibration(charts=editing, actions=(OperatorButton(target=apply_calibration),))
rows: Final = (editing, calibration, Panel(section=calibration.section(), at=Beside(IMAGE_PT_image_properties)), Panel(section=calibration.node_section(), at=Beside(NODE_PT_active_node_generic)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["apply_calibration", "calibration", "editing", "rows", "turned"]
