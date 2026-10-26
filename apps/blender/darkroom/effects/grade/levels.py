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
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.compositor.effects import CompositorEffect, entered, Named, Registers, Shell
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.files.images import ImageFile
from blender.records.faults import Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage, transform

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
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=Levels.__name__, cause=repr(values)),))


def leveled(shell: Shell[Levels]) -> Outcome[Expr]:
    """Subtracts the coverage-weighted black point and lowers by the white span in stops."""
    color = shell.color
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", color - shell.input("black") * color["Alpha"]), ("Exposure", -shell.input("white"))), socket="Image", shape="RGBA"))


def auto_levels(call: Call[Named]) -> Outcome[Answer]:
    """Measure levels from the stage input."""

    def measured(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
        return Samples.of(PyOpenColorIO.GetCurrentConfig(), file.pixels).bind(
            lambda samples: (
                samples
                .extent()
                .filter(lambda extent: extent.white > extent.low)
                .map(lambda extent: entered(identity, entry, {"black": extent.low, "white": float(np.log2(extent.white - extent.low))}))
                .default_value(Ok(False))
                .map(lambda wrote: Some(bpy.path.display_name(auto_levels.__name__)) if wrote else Nothing)
            )
        )

    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: levels.named(call.identity, scene, call.properties.name).bind(lambda entry: READ.entering(call.identity, scene, entry, measured)))
        .map(lambda _: Done())
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

levels: Final[CompositorEffect[Levels]] = CompositorEffect(
    record=Levels,
    graph=leveled,
    interface=Interface(
        inputs=tuple(
            Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketFloat, description=description, members=members)
            for key, description, members in (("black", "Scene-linear value mapping to 0", {}), ("white", "Stops from black point to the value mapping to 1", {"min_value": -32.0, "max_value": 32.0}))
        )
    ),
    parts=(Registers(rows=(Operator(form=Named, run=auto_levels, icon=Some("SEQ_HISTOGRAM"), undo=False, condition=Some(Job(kind="RENDER"))),)),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Levels", "levels"]
