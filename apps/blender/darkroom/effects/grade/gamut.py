"""ACES gamut compression of scene-linear excursions toward a colorspace's boundary."""

from typing import Annotated, override

from expression import Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell, Spaces
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.processors import ColorSpace
from blender.imaging.faults import ImagingResult
from blender.records.fields import Presentation

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Channel(msgspec.Struct, frozen=True, kw_only=True):
    """Compression of one channel's distance from the achromatic axis."""

    limit: Annotated[float, msgspec.Meta(ge=1.001, le=65504.0, description="Distance compressed onto the gamut boundary"), Presentation(soft=Some((1.001, 2.0)), precision=Some(3))]
    threshold: Annotated[float, msgspec.Meta(ge=0.0, le=0.9995, description="Largest unchanged distance"), Presentation(precision=Some(3))]


class GamutCompression(Effect, frozen=True, kw_only=True):
    """Compresses scene-linear excursions toward the gamut boundary of the target colorspace."""

    stage = Stage.GAMUT
    pointwise = True
    space: Annotated[ColorSpace, GraphForm.COLOR_SPACE, msgspec.Meta(title="Gamut", description="Target colorspace")] = PyOpenColorIO.ROLE_SCENE_LINEAR
    cyan: Annotated[Channel, msgspec.Meta(description="Red channel toward cyan")] = Channel(limit=1.147, threshold=0.815)
    magenta: Annotated[Channel, msgspec.Meta(description="Green channel toward magenta")] = Channel(limit=1.264, threshold=0.803)
    yellow: Annotated[Channel, msgspec.Meta(description="Blue channel toward yellow")] = Channel(limit=1.312, threshold=0.880)
    power: Annotated[float, msgspec.Meta(ge=1.0, le=65504.0, description="Curve sharpness between threshold and limit"), Presentation(soft=Some((1.0, 2.0)), step=Some(0.01), precision=Some(3))] = 1.2

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's ACES gamut compression with the channels' limits, then thresholds, then power."""
        channels = (self.cyan, self.magenta, self.yellow)
        return Ok(
            Some(
                PyOpenColorIO.FixedFunctionTransform(
                    style=PyOpenColorIO.FIXED_FUNCTION_ACES_GAMUT_COMP_13, params=[*(channel.limit for channel in channels), *(channel.threshold for channel in channels), self.power]
                )
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def compressed(shell: Shell[GamutCompression]) -> BlenderResult[Expr]:
    """Binds each channel's limit and threshold and the power to the node form of OCIO's gamut compression."""
    _, cyan, magenta, yellow, power = GamutCompression.__struct_fields__
    limit, threshold = Channel.__struct_fields__
    return Ok(
        grades.gamut_compression(
            shell.color,
            limits=(shell.input(cyan, limit), shell.input(magenta, limit), shell.input(yellow, limit)),
            thresholds=(shell.input(cyan, threshold), shell.input(magenta, threshold), shell.input(yellow, threshold)),
            power=shell.input(power),
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

gamut_compression = CompositorEffect(record=GamutCompression, graph=compressed, alpha=Alpha.STRAIGHT, parts=(Spaces(space=lambda record: record.space),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channel", "GamutCompression", "gamut_compression"]
