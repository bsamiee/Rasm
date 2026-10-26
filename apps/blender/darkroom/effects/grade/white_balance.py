"""White balance from a source white to a target white through Bradford cone response, its matrix derived from the live config."""

from typing import Annotated, override, Self

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderFaults, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Derives, Measures, Shell
from blender.bpy.render.compositor.grades import matrixed, MatrixRows
from blender.bpy.render.compositor.viewers import Unread
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import matrix, xyz
from blender.imaging.color.white import adaptation, White
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import ImageFile
from blender.records.fields import Rgb

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class WhiteBalance(Effect, frozen=True, kw_only=True):
    """Adapts the source white to the target white through Bradford cone response."""

    stage = Stage.WHITE_BALANCE
    pointwise = True
    source: Annotated[White, msgspec.Meta(description="Frame white")] = White()
    target: Annotated[White, msgspec.Meta(description="Output white")] = White()

    def adapting(self, config: PyOpenColorIO.Config) -> PyOpenColorIO.GroupTransform:
        """Returns the XYZ adaptation between the scene-linear conversion and its inverse."""
        reading = xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)
        return PyOpenColorIO.GroupTransform([reading, matrix(adaptation(self.source.xyz, self.target.xyz)), PyOpenColorIO.GroupTransform([reading], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)])

    @override
    def transform(self, config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the adaptation."""
        return Ok(Some(self.adapting(config)))

    @classmethod
    def measured(cls, config: PyOpenColorIO.Config, record: Self, file: ImageFile[np.float32]) -> BlenderResult[Option[Self]]:
        """Sets the source white to the covered mean of the entering frame, refusing a frame with no readable white."""
        return Result[Option[White], BlenderFaults].bind(
            Samples.of(config, file.pixels).bind(
                lambda samples: White.of(config, msgspec.convert(samples.color[samples.covered].mean(axis=0, dtype=np.float64).tolist(), Rgb)) if samples.covered.any() else Ok(Nothing)
            ),
            lambda white: white.map(lambda source: Some(msgspec.structs.replace(record, source=source))).to_result_with(lambda: (BlenderUnmetFault(requirement=Unread(entry=record.title)),)),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def rows(_identity: Identity, _scene: bpy.types.Scene, record: WhiteBalance) -> BlenderResult[MatrixRows]:
    """Reduces the record's adaptation through the live config to the matrix rows the graph reads."""
    return config_of().bind(lambda config: MatrixRows.of(config, record.adapting(config)))


def balanced(shell: Shell[WhiteBalance]) -> BlenderResult[Expr]:
    """Applies the derived adaptation rows to straight scene-linear color."""
    red, green, blue, offset = (group_input(name, "VECTOR") for name in MatrixRows.__struct_fields__)
    return Ok(matrixed(shell.color, (red, green, blue), offset))


# --- [COMPOSITION] ----------------------------------------------------------------------

white_balance = CompositorEffect(
    record=WhiteBalance, graph=balanced, alpha=Alpha.STRAIGHT, parts=(Derives[WhiteBalance, msgspec.Struct](record=Some(MatrixRows), of=rows), Measures(revise=WhiteBalance.measured))
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["WhiteBalance", "white_balance"]
