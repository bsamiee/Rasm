"""BT.2390 peak mapping of formed absolute PQ display codes into a display's peak and black."""

from typing import Annotated

from expression import Ok, Some
import msgspec

from blender.bpy.commands.operators import Operator
from blender.bpy.data.rna.conditions import AllOf, HdrWindow, Job
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import CompositorEffect, Measures, Named, Shell
from blender.bpy.render.compositor.viewers import measuring
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import Domain
from blender.records.fields import Presentation, Quantity, Unit

from ...chain.stages import Effect, Stage
from .display import shown_space

# --- [TYPES] ----------------------------------------------------------------------------

type Luminance = Annotated[float, msgspec.Meta(ge=0.0, le=10000.0), Presentation(soft=Some((100.0, 10000.0)), step=Some(1.0), precision=Some(0), quantity=Some(Quantity.LUMINANCE))]

# --- [MODELS] ---------------------------------------------------------------------------


class PeakMapping(Effect, frozen=True, kw_only=True):
    """Per-channel PQ peak compression with Hermite roll-off and black lift."""

    stage = Stage.PEAK
    pointwise = True
    source_peak: Annotated[Luminance, msgspec.Meta(description="Brightest frame luminance")] = 10000.0
    target_peak: Annotated[Luminance, msgspec.Meta(description="Display peak luminance")] = 1000.0
    black: Annotated[
        Unit,
        msgspec.Meta(title="Display Black", description="Share of fitted peak luminance, inverse of display contrast ratio"),
        Presentation(soft=Some((0.0, 0.001)), step=Some(0.00001), precision=Some(6)),
    ] = 0.0
    knee_offset: Annotated[float, msgspec.Meta(ge=0.5, le=2.0, description="Offset from fitted peak down to knee, 0.5 in BT.2390"), Presentation(step=Some(0.01), precision=Some(2))] = 0.5


# --- [OPERATIONS] -----------------------------------------------------------------------


def peaked(shell: Shell[PeakMapping]) -> BlenderResult[Expr]:
    """Maps formed PQ codes through the BT.2390 EETF over live peak, black, and knee inputs."""
    source_peak, target_peak, black, knee_offset = map(shell.input, PeakMapping.__struct_fields__)
    return Ok(grades.eetf(shell.color, source=source_peak, target=target_peak, black=black, knee=knee_offset))


# --- [COMPOSITION] ----------------------------------------------------------------------

measure_peak = Operator(
    key="measure_peak",
    title="Measure Source Peak",
    description="Measure source peak from stage input",
    form=Named,
    undo=False,
    condition=Some(AllOf(conditions=(Job(kind="RENDER"), HdrWindow(space=shown_space)))),
    run=lambda call: measuring(peak_mapping, call),
)
peak_mapping: CompositorEffect[PeakMapping] = CompositorEffect(
    record=PeakMapping,
    graph=peaked,
    domain=Domain.DISPLAY,
    parts=(
        Measures(
            revise=lambda config, record, file: Samples.of(config, file.pixels).bind(Samples.peak).map(lambda peak: peak.map(lambda value: msgspec.structs.replace(record, source_peak=value))),
            command=Some(measure_peak),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Luminance", "PeakMapping", "measure_peak", "peak_mapping"]
