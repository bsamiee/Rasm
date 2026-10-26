"""BT.2390 peak compression and black adaptation over formed PQ codes."""

from collections.abc import Mapping
from typing import Final

import bpy
from colour.models.rgb.transfer_functions.st_2084 import CONSTANTS_ST2084
from expression import Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import Operator
from blender.bpy.data.rna.conditions import AllOf, HdrWindow, Job
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, vector, xyz
from blender.bpy.render.effects.formation import viewed
from blender.bpy.render.effects.rows import CompositorEffect, Measured, Named, Shell
from blender.bpy.render.effects.viewers import measuring
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import DisplayView
from blender.imaging.effects.tone.peak import PeakMapping

# --- [CONSTANTS] ------------------------------------------------------------------------

PQ: Final[Mapping[str, float]] = CONSTANTS_ST2084

# --- [OPERATIONS] -----------------------------------------------------------------------


def peaked(shell: Shell[PeakMapping]) -> BlenderResult[Expr]:
    """Builds forward-only EETF over source-peak-relative PQ codes, black adaptation kept without peak compression."""
    source_peak, target_peak, black, knee_offset = map(shell.input, PeakMapping.__struct_fields__)
    limit = math("MINIMUM", target_peak, source_peak)
    powered = (xyz(source_peak, limit, black * limit) / 10000.0) ** PQ["m_1"]
    codes = ((powered * PQ["c_2"] + PQ["c_1"]) / (powered * PQ["c_3"] + 1.0)) ** PQ["m_2"]
    reference = codes["X"]
    peak, floor = codes["Y"] / reference, codes["Z"] / reference
    knee = knee_offset * (peak - 1.0) + peak
    exponent = math("MINIMUM", 1.0 / floor, 4.0)
    gain = 1.0 / ((1.0 - peak) ** exponent * (floor / peak) + 1.0)
    lift = gain * floor
    clipped = vector("MINIMUM", vector("MAXIMUM", vector("DIVIDE", shell.color, reference), (0.0, 0.0, 0.0)), (1.0, 1.0, 1.0))
    position = vector("MAXIMUM", clipped - knee, (0.0, 0.0, 0.0)) / (1.0 - knee)
    rolled = position * position * (position * (knee + 1.0 - 2.0 * peak) + (peak * 3.0 - 2.0 - knee)) + clipped
    lifted = rolled * gain + ((1.0 - rolled) ** exponent * lift + (floor - lift))
    return Ok(vector("SCALE", vector("MINIMUM", vector("MAXIMUM", lifted, floor), peak), reference))


def formed_view(context: bpy.types.Context) -> Option[DisplayView]:
    """Returns preview-enabled formation view, else scene display view."""
    return Option.of_optional(context.scene).map(
        lambda scene: next(
            (
                viewed(node)
                for entry in scene.compositor_effects
                if entry.enable_for_preview
                for group in Option.of_optional(entry.node_group).to_list()
                for node in group.nodes
                if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)
            ),
            viewed(scene),
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

measure_peak = Operator(
    key="measure_peak",
    title="Measure Source Peak",
    description="Measure source peak from stage input",
    form=Named,
    undo=False,
    condition=Some(AllOf(conditions=(Job(kind="RENDER"), HdrWindow(formed=formed_view)))),
    run=lambda call: measuring(peak_mapping, call),
)
peak_mapping: CompositorEffect[PeakMapping] = CompositorEffect(
    record=PeakMapping,
    graph=peaked,
    measured=Some(
        Measured(
            revise=lambda config, record, file: Samples.of(config, file.pixels).map(lambda samples: samples.peak.map(lambda value: msgspec.structs.replace(record, source_peak=value))),
            command=Some(measure_peak),
        )
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["formed_view", "measure_peak", "peak_mapping", "peaked"]
