"""Affine black and white point adjustment."""

import bpy
from expression import Ok, Some

from blender.bpy.commands.operators import Operator
from blender.bpy.data.rna.conditions import Job
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.effects.rows import CompositorEffect, Measured, Named, Shell
from blender.bpy.render.effects.viewers import measuring
from blender.imaging.analysis.metering import Samples
from blender.imaging.effects.tone.levels import Levels

# --- [OPERATIONS] -----------------------------------------------------------------------


def leveled(shell: Shell[Levels]) -> BlenderResult[Expr]:
    """Subtracts coverage-weighted black point and applies white span in stops."""
    black, white = map(shell.input, Levels.__struct_fields__)
    color = shell.color
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", color - black * color["Alpha"]), ("Exposure", -white)), socket="Image", shape="RGBA"))


# --- [COMPOSITION] ----------------------------------------------------------------------

auto_levels = Operator(
    key="auto_levels",
    title="Auto Levels",
    description=f"Measure {Levels.title} from stage input",
    icon=Some("SEQ_HISTOGRAM"),
    form=Named,
    undo=False,
    condition=Some(Job(kind="RENDER")),
    run=lambda call: measuring(levels, call),
)
levels: CompositorEffect[Levels] = CompositorEffect(
    record=Levels,
    graph=leveled,
    measured=Some(
        Measured(
            revise=lambda config, _record, file: Samples.of(config, file.pixels).map(lambda samples: samples.extent().bind(lambda extent: Levels.of(extent.low, extent.white))),
            command=Some(auto_levels),
        )
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["auto_levels", "leveled", "levels"]
