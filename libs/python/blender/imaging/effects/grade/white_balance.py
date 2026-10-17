"""White balance from a source white to a target white."""

from typing import Annotated, override

from expression import Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import matrix, xyz
from blender.imaging.color.white import adaptation, White
from blender.imaging.faults import ImagingResult

# --- [MODELS] ---------------------------------------------------------------------------


class WhiteBalance(Effect, frozen=True, kw_only=True, tag=8):
    """Adapts the source white to the target white through the Bradford cone response."""

    stage = Stage.WHITE_BALANCE
    pointwise = True
    source: Annotated[White, msgspec.Meta(description="White in the frame that becomes the target white")] = White()
    target: Annotated[White, msgspec.Meta(description="White the source white becomes")] = White()

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the XYZ adaptation between the scene-linear conversion and its inverse."""
        reading = xyz(config, PyOpenColorIO.ROLE_SCENE_LINEAR)
        return Ok(
            Some(PyOpenColorIO.GroupTransform([reading, matrix(adaptation(self.source.xyz, self.target.xyz)), PyOpenColorIO.GroupTransform([reading], direction=PyOpenColorIO.TRANSFORM_DIR_INVERSE)]))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["WhiteBalance"]
