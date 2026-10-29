"""Black and white points of scene-linear values, white in stops above black, measured from the entering frame's covered extent."""

from collections.abc import Mapping
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.color.spaces import EXPOSURE
from blender.bpy.render.compositor.effects import CompositorEffect, Named
from blender.bpy.render.compositor.inputs import entered
from blender.bpy.render.compositor.shells import Shell
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.files.images import ImageFile
from blender.records.faults import Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage, transform

# --- [CONSTANTS] ------------------------------------------------------------------------

_BLACK: Final = Socket(key="black", kind=bpy.types.NodeSocketFloat, description="Scene-linear value mapping to 0")
_WHITE: Final = Socket(key="white", kind=bpy.types.NodeSocketFloat, description="Stops from black point to the value mapping to 1", min_value=EXPOSURE.prop.hard_min, max_value=EXPOSURE.prop.hard_max)

# --- [MODELS] ---------------------------------------------------------------------------


class Levels(Effect, frozen=True, kw_only=True):
    """Maps a scene-linear black point to 0 and a white point stops above it to 1."""

    stage = Stage.LEVELS
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------


@transform.register
def _(_record: Levels, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns OCIO's linear scale and offset, alpha passed through."""
    match values:
        case {"black": float(black), "white": float(white)}:
            operation = PyOpenColorIO.MatrixTransform.Scale((2.0**-white,) * 3 + (1.0,))
            operation.setOffset((-black * 2.0**-white,) * 3 + (0.0,))
            return Ok(Some(operation))
        case _:
            return Error(Fault.of(RecordReason.UNREADABLE, Levels.__name__, repr(values)))


def auto_levels(call: Call[Named]) -> Outcome[Answer]:
    """Measure levels from the stage input."""

    def measured(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
        return Samples.of(PyOpenColorIO.GetCurrentConfig(), file.pixels).bind(
            lambda samples: (
                samples
                .extent()
                .filter(lambda extent: extent.white > extent.low)
                .map(lambda extent: entered(identity, entry, {_BLACK.key: extent.low, _WHITE.key: float(np.log2(extent.white - extent.low))}))
                .default_value(Ok(False))
                .map(lambda wrote: Some(bpy.path.display_name(auto_levels.__name__)) if wrote else Nothing)
            )
        )

    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: levels.named(call.identity, scene, call.properties.name).bind(lambda entry: READ.entering(call.identity, scene, entry, measured)))
        .map(lambda _: Done())
    )


def _levels(shell: Shell[Levels]) -> Outcome[Expr]:
    """Subtracts the coverage-weighted black point and lowers by the white span in stops."""
    color = shell.color
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeExposure, inputs=(("Image", color - shell.input(_BLACK) * color["Alpha"]), ("Exposure", -shell.input(_WHITE))), socket="Image", shape=bpy.types.NodeSocketColor
        )
    )


levels: Final = CompositorEffect(
    graph=_levels,
    record=Levels,
    interface=Interface(inputs=(_BLACK, _WHITE)),
    registers=(Operator(form=Named, run=auto_levels, icon=Some("SEQ_HISTOGRAM"), undo=Nothing, condition=Some(Job(kind="RENDER"))),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Levels", "levels"]
