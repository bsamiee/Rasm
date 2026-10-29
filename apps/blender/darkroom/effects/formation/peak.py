"""BT.2390 peak mapping of formed absolute PQ display codes into a display's peak and black."""

from typing import Final, override

import bpy
from expression import Nothing, Ok, Option, Some
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator
from blender.bpy.data.rna.conditions import Condition, Job
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes.desired import Interface, Socket
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import CompositorEffect, Named
from blender.bpy.render.compositor.inputs import entered, Shell
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import Domain, pq
from blender.imaging.files.images import ImageFile
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage
from .display import shown_space

# --- [CONSTANTS] ------------------------------------------------------------------------

_SOURCE_PEAK: Final = Socket(key="source_peak", kind=bpy.types.NodeSocketFloat, description="Brightest frame luminance", min_value=0.0, max_value=10000.0, default_value=10000.0)
_TARGET_PEAK: Final = Socket(key="target_peak", kind=bpy.types.NodeSocketFloat, description="Display peak luminance", min_value=0.0, max_value=10000.0, default_value=1000.0)
_DISPLAY_BLACK: Final = Socket(key="display_black", kind=bpy.types.NodeSocketFloatFactor, description="Share of fitted peak luminance, inverse of display contrast ratio", min_value=0.0, max_value=1.0)
_KNEE_OFFSET: Final = Socket(key="knee_offset", kind=bpy.types.NodeSocketFloat, description="Offset from fitted peak down to knee, 0.5 in BT.2390", min_value=0.5, max_value=2.0, default_value=0.5)

# --- [MODELS] ---------------------------------------------------------------------------


class PeakMapping(Effect, frozen=True, kw_only=True):
    """Per-channel PQ peak compression with Hermite roll-off and black lift."""

    stage = Stage.PEAK
    pointwise = True
    domain = Domain.DISPLAY


class _HdrWindow(Condition, frozen=True, kw_only=True):
    """Window with HDR color showing a frame formed into an absolute PQ display space."""

    @override
    def holds(self, context: bpy.types.Context, item: Option[Struct] = Nothing) -> bool:
        return (
            shown_space(context).bind(lambda name: Option.of_optional(PyOpenColorIO.GetCurrentConfig().getColorSpace(name))).filter(pq).is_some()
            and Option.of_optional(context.window).filter(lambda window: window.support_hdr_color).is_some()
        )

    @property
    @override
    def message(self) -> str:
        return "frame formed for PQ display in window with HDR color"


# --- [OPERATIONS] -----------------------------------------------------------------------


def measure_peak(call: Call[Named]) -> Outcome[Answer]:
    """Measure source peak from stage input."""

    def measured(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[Option[str]]:
        return (
            Samples
            .of(PyOpenColorIO.GetCurrentConfig(), file.pixels)
            .bind(Samples.peak)
            .bind(lambda peak: peak.map(lambda value: entered(identity, entry, {_SOURCE_PEAK.key: value})).default_value(Ok(False)))
            .map(lambda wrote: Some(bpy.path.display_name(measure_peak.__name__)) if wrote else Nothing)
        )

    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: peak_mapping.named(call.identity, scene, call.properties.name).bind(lambda entry: READ.entering(call.identity, scene, entry, measured)))
        .map(lambda _: Done())
    )


def _peak_mapping(shell: Shell[PeakMapping]) -> Outcome[Expr]:
    """Maps formed PQ codes through the BT.2390 EETF over live peak, black, and knee inputs."""
    return Ok(grades.eetf(shell.color, source=shell.input(_SOURCE_PEAK), target=shell.input(_TARGET_PEAK), black=shell.input(_DISPLAY_BLACK), knee=shell.input(_KNEE_OFFSET)))


# --- [COMPOSITION] ----------------------------------------------------------------------

peak_mapping: Final = CompositorEffect(
    graph=_peak_mapping,
    record=PeakMapping,
    interface=Interface(inputs=(_SOURCE_PEAK, _TARGET_PEAK, _DISPLAY_BLACK, _KNEE_OFFSET)),
    registers=(Operator(form=Named, run=measure_peak, undo=Nothing, condition=Some(Job(kind="RENDER") & _HdrWindow())),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["PeakMapping", "peak_mapping"]
