"""Gamut compression toward a colorspace's boundary."""

from typing import Annotated, override

from expression import Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import ColorSpace, Presentation

# --- [MODELS] ---------------------------------------------------------------------------


class Channel(msgspec.Struct, frozen=True, kw_only=True):
    """Compression of channel distance from achromatic axis."""

    limit: Annotated[float, msgspec.Meta(ge=1.001, le=65504.0, description="Distance compressed onto gamut boundary"), Presentation(soft=Some((1.001, 2.0)), decimals=Some(3))]
    threshold: Annotated[float, msgspec.Meta(ge=0.0, le=0.9995, description="Largest unchanged distance"), Presentation(decimals=Some(3))]


class GamutCompression(Effect, frozen=True, kw_only=True, tag=10):
    """Compresses scene-linear excursions toward gamut boundary."""

    stage = Stage.GAMUT
    pointwise = True
    space: Annotated[ColorSpace, msgspec.Meta(title="Gamut", description="Target colorspace")] = PyOpenColorIO.ROLE_SCENE_LINEAR
    cyan: Annotated[Channel, msgspec.Meta(description="Red channel toward cyan")] = Channel(limit=1.147, threshold=0.815)
    magenta: Annotated[Channel, msgspec.Meta(description="Green channel toward magenta")] = Channel(limit=1.264, threshold=0.803)
    yellow: Annotated[Channel, msgspec.Meta(description="Blue channel toward yellow")] = Channel(limit=1.312, threshold=0.880)
    power: Annotated[float, msgspec.Meta(ge=1.0, le=65504.0, description="Curve sharpness between threshold and limit"), Presentation(soft=Some((1.0, 2.0)), step=Some(0.01), decimals=Some(3))] = 1.2

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns ACES gamut compression transform."""
        channels = (self.cyan, self.magenta, self.yellow)
        return Ok(
            Some(
                PyOpenColorIO.FixedFunctionTransform(
                    style=PyOpenColorIO.FIXED_FUNCTION_ACES_GAMUT_COMP_13, params=[*(channel.limit for channel in channels), *(channel.threshold for channel in channels), self.power]
                )
            )
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channel", "GamutCompression"]
