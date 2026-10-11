"""Scene-linear exposure gain and bilateral local contrast about middle grey."""

from typing import Annotated, ClassVar, override

from expression import Ok, Option, Some
import msgspec
import PyOpenColorIO

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.faults import ImagingResult
from rasm.imaging.parameters import Stops

# --- [MODELS] ---------------------------------------------------------------------------


class Exposure(Effect, frozen=True, kw_only=True, tag=1):
    """Scales scene-linear light by a power of two while preserving alpha."""

    stage = Stage.EXPOSURE
    title = "Exposure"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    stops: Annotated[Stops, msgspec.Meta(title="Exposure", description="Gain in stops, each stop doubling the light")] = 0.0

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's unclamped RGB scale with an identity alpha channel."""
        return Ok(Some(PyOpenColorIO.MatrixTransform.Scale([2.0**self.stops] * 3 + [1.0])))


class LocalExposure(Effect, frozen=True, kw_only=True, tag=2):
    """Compresses broad tones toward middle grey and scales detail about their base."""

    stage = Stage.LOCAL_EXPOSURE
    title = "Local Exposure"
    pointwise = False
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    blurred_share: ClassVar[float] = 0.6
    blurred_radius: ClassVar[float] = 0.25
    grid_radius: ClassVar[float] = 3.0 * 64.0 / 1920.0
    grid_slices: ClassVar[int] = 32
    grid_range: ClassVar[tuple[float, float]] = (-8.0, 4.0)
    highlights: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Base contrast above grey, 0 flattening it to grey")] = 1.0
    shadows: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Base contrast below grey, 0 flattening it to grey")] = 1.0
    clarity: Annotated[float, msgspec.Meta(ge=1.0, le=2.2, description="Detail scale about the base")] = 1.0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Exposure", "LocalExposure"]
