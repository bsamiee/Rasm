"""Black and white points of scene-linear values, the white in stops above the black."""

import math
from typing import Annotated, override, Self

from expression import Ok, Option, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import Finite, Presentation, Stops

# --- [MODELS] ---------------------------------------------------------------------------


class Levels(Effect, frozen=True, kw_only=True, tag=3):
    """Maps a black point to 0 and a white point stops above it to 1 on scene-linear values."""

    stage = Stage.LEVELS
    pointwise = True
    black: Annotated[Finite, msgspec.Meta(description="Scene-linear value mapping to 0"), Presentation(soft=Some((-0.1, 0.1)), decimals=Some(4))] = 0.0
    white: Annotated[Stops, msgspec.Meta(description="Stops from the black point to the value mapping to 1")] = 0.0

    # --- [CONSTRUCTION]

    @classmethod
    def of(cls, black: float, white: float) -> Option[Self]:
        """Builds the record mapping two scene-linear points to 0 and 1, if their span fits Stops."""
        return (
            Some(white - black)
            .filter(lambda span: span > 0.0)
            .bind(lambda span: catch(lambda: msgspec.convert({"black": black, "white": math.log2(span)}, cls), exception=msgspec.ValidationError)().to_option())
        )

    # --- [TRANSFORM]

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the linear scale and offset with alpha passed through."""
        scale = 2.0**-self.white
        transform = PyOpenColorIO.MatrixTransform.Scale((scale,) * 3 + (1.0,))
        transform.setOffset((-self.black * scale,) * 3 + (0.0,))
        return Ok(Some(transform))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Levels"]
