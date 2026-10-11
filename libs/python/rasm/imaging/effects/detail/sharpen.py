"""Step-gated adaptive sharpening of scene-linear color."""

from typing import Annotated, ClassVar

import msgspec
import PyOpenColorIO

from rasm.imaging.chain import Effect, Stage

# --- [MODELS] ----------------------------------------------------------------------------


class Sharpen(Effect, frozen=True, kw_only=True, tag=21):
    """Four-neighbor sharpening attenuated by exposed luminance steps."""

    stage = Stage.SHARPEN
    title = "Sharpen"
    pointwise = False
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    weight: ClassVar[float] = 1.0 / 6.0
    sharpness: Annotated[float, msgspec.Meta(ge=0.0, le=10.0, title="Sharpness", description="Gain of each pixel's difference from its four axis neighbors")] = 0.0


# --- [EXPORTS] ---------------------------------------------------------------------------

__all__ = ["Sharpen"]
