"""Black and white points of scene-linear values, white in stops above black, measured from the entering frame's covered extent."""

from typing import Annotated, Final, override, Self

import bpy
from expression import Ok, Option, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Operator
from blender.bpy.data.rna.conditions import Job
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.compositor.effects import CompositorEffect, Measures, Named, Shell
from blender.bpy.render.compositor.viewers import measuring
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import Stops
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import ImageFile
from blender.records.fields import Finite, Presentation

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Levels(Effect, frozen=True, kw_only=True):
    """Maps a scene-linear black point to 0 and a white point stops above it to 1."""

    stage = Stage.LEVELS
    pointwise = True
    black: Annotated[Finite, msgspec.Meta(description="Scene-linear value mapping to 0"), Presentation(soft=Some((-0.1, 0.1)), precision=Some(4))] = 0.0
    white: Annotated[Stops, msgspec.Meta(description="Stops from black point to the value mapping to 1")] = 0.0

    @classmethod
    def of(cls, black: float, white: float) -> Option[Self]:
        """Builds the levels mapping two scene-linear points to 0 and 1, Nothing when their span is empty or past the stop domain."""
        return (
            Some(white - black)
            .filter(lambda span: span > 0.0)
            .bind(lambda span: catch(lambda: msgspec.convert({"black": black, "white": float(np.log2(span))}, cls), exception=msgspec.ValidationError)().to_option())
        )

    @classmethod
    def measured(cls, config: PyOpenColorIO.Config, _record: Self, file: ImageFile[np.float32]) -> BlenderResult[Option[Self]]:
        """Reads the levels spanning the entering frame's covered luminance extent."""
        return Samples.of(config, file.pixels).map(lambda samples: samples.extent().bind(lambda extent: cls.of(extent.low, extent.white)))

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's linear scale and offset, alpha passed through."""
        scale = 2.0**-self.white
        transform = PyOpenColorIO.MatrixTransform.Scale((scale,) * 3 + (1.0,))
        transform.setOffset((-self.black * scale,) * 3 + (0.0,))
        return Ok(Some(transform))


# --- [OPERATIONS] -----------------------------------------------------------------------


def leveled(shell: Shell[Levels]) -> BlenderResult[Expr]:
    """Subtracts the coverage-weighted black point and lowers by the white span in stops."""
    black, white = map(shell.input, Levels.__struct_fields__)
    color = shell.color
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", color - black * color["Alpha"]), ("Exposure", -white)), socket="Image", shape="RGBA"))


# --- [COMPOSITION] ----------------------------------------------------------------------

auto_levels = Operator(
    key="auto_levels",
    title="Auto Levels",
    description="Measure levels from the stage input",
    icon=Some("SEQ_HISTOGRAM"),
    form=Named,
    undo=False,
    condition=Some(Job(kind="RENDER")),
    run=lambda call: measuring(levels, call),
)
levels: Final[CompositorEffect[Levels]] = CompositorEffect(record=Levels, graph=leveled, parts=(Measures(revise=Levels.measured, command=Some(auto_levels)),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Levels", "levels"]
