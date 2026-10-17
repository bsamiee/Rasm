"""Step-gated adaptive sharpening of scene-linear color."""

from typing import Annotated, ClassVar

import msgspec

from blender.imaging.chain import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Sharpen(Effect, frozen=True, kw_only=True, tag=21):
    """Four-neighbor sharpening attenuated by exposed luminance steps."""

    stage = Stage.SHARPEN
    pointwise = False
    weight: ClassVar[float] = 1.0 / 6.0
    sharpness: Annotated[float, msgspec.Meta(ge=0.0, le=10.0, description="Gain of each pixel's difference from its four axis neighbors")] = 0.0


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Sharpen"]
