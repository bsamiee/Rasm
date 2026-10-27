"""White balance from a source white to a target white through Bradford cone response, its matrix derived from the live config."""

from collections.abc import Mapping
from functools import partial

import bpy
from expression import Error, Nothing, Ok, Option, Some
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Derives, entered, Named, Registers, Shell
from blender.bpy.render.compositor.grades import MATRIX, matrixed, MatrixRows
from blender.bpy.render.compositor.inputs import captured, valued
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import matrix, xyz
from blender.imaging.color.white import adaptation, White
from blender.imaging.files.images import ImageFile
from blender.records.faults import Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage, transform

# --- [MODELS] ---------------------------------------------------------------------------


class WhiteBalance(Effect, frozen=True, kw_only=True):
    """Adapts the source white to the target white through Bradford cone response."""

    stage = Stage.WHITE_BALANCE
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------


@transform.register
def _(_record: WhiteBalance, values: Mapping[Key, Value], config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns the XYZ adaptation from the source white to the target white between the scene-linear conversion and its inverse."""
    match values:
        case {"source_temperature": float(source_temperature), "source_duv": float(source_duv), "target_temperature": float(target_temperature), "target_duv": float(target_duv)}:
            reading = xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)
            bradford = matrix(adaptation(White(temperature=source_temperature, duv=source_duv).xyz, White(temperature=target_temperature, duv=target_duv).xyz))
            return Ok(Some(PyOpenColorIO.GroupTransform([reading, bradford, PyOpenColorIO.GroupTransform([reading], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)])))
        case _:
            return Error(Fault.of(RecordReason.UNREADABLE, WhiteBalance.__name__, repr(values)))


def rows(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, record: WhiteBalance) -> Outcome[Mapping[Key, Value]]:
    """Reduces the entry's adaptation through the live config to the matrix rows the graph reads."""
    config = PyOpenColorIO.GetCurrentConfig()
    return (
        captured(identity, entry)
        .bind(lambda values: transform(record, valued(values), config))
        .bind(lambda operation: operation.map(lambda held: MatrixRows.of(config, held)).default_value(Ok(MatrixRows())))
        .map(lambda found: found.values)
    )


def balanced(shell: Shell[WhiteBalance]) -> Outcome[Expr]:
    """Applies the derived adaptation rows to straight scene-linear color."""
    red, green, blue, offset = (group_input(socket.key, "VECTOR") for socket in MATRIX)
    return Ok(matrixed(shell.color, (red, green, blue), offset))


def white_pick(call: Call[Named]) -> Outcome[Answer]:
    """Measure the source white from the stage input's covered mean."""

    def measured(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
        config = PyOpenColorIO.GetCurrentConfig()
        return (
            Samples
            .of(config, file.pixels)
            .bind(lambda samples: White.of(config, samples.color[samples.covered].mean(axis=0, dtype=np.float64)) if samples.covered.any() else Ok(Nothing))
            .bind(lambda white: white.to_result_with(partial(Fault.of, BlenderReason.UNMET, f"Region of frame entering {entry.name} holds no value its effect reads")))
            .bind(lambda white: entered(identity, entry, {"source_temperature": white.temperature, "source_duv": white.duv}))
            .map(lambda wrote: Some(bpy.path.display_name(white_pick.__name__)) if wrote else Nothing)
        )

    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: white_balance.named(call.identity, scene, call.properties.name).bind(lambda entry: READ.entering(call.identity, scene, entry, measured)))
        .map(lambda _: Done())
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

white_balance = CompositorEffect(
    record=WhiteBalance,
    graph=balanced,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        panels=tuple(
            InterfacePanel(
                key=key,
                name=bpy.path.display_name(key),
                description=description,
                inputs=tuple(
                    Socket(key=item_key(key, field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
                    for field, text, members in (
                        ("temperature", "Correlated color temperature", {"min_value": 1800.0, "max_value": White.hottest, "default_value": 6500.0}),
                        ("duv", "Distance from the Planckian locus in CIE 1960 uv, positive toward green", {"min_value": -0.05, "max_value": 0.05}),
                    )
                ),
            )
            for key, description in (("source", "Frame white"), ("target", "Output white"))
        )
    ),
    parts=(Derives[WhiteBalance](of=rows, sockets=MATRIX), Registers(rows=(Operator(form=Named, run=white_pick, icon=Some("EYEDROPPER"), undo=False, condition=Some(Job(kind="RENDER"))),))),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["WhiteBalance", "white_balance"]
