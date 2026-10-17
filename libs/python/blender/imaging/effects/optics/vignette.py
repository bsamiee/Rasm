"""Natural camera falloff and shaped vignetting in scene-linear light."""

from typing import Annotated

import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.compositing.masks import Shape
from blender.imaging.parameters import Case, Rgb

# --- [MODELS] ---------------------------------------------------------------------------


class Natural(Case, frozen=True, kw_only=True, tag=1):
    """Relative illumination as the fourth power of the chief-ray cosine."""


class Shaped(Case, frozen=True, kw_only=True, tag=2):
    """Light retained inside a feathered outline and pulled toward a color outside."""

    shape: Annotated[Shape, msgspec.Meta(description="Outline inside which the frame keeps its light")] = Shape(size=(0.8, 0.45), feather=0.2)
    color: Annotated[Rgb, msgspec.Meta(description="Scene-linear color the frame falls toward outside the shape")] = Rgb(red=0.0, green=0.0, blue=0.0)


class Vignette(Effect, frozen=True, kw_only=True, tag=31):
    """Frame-edge falloff from a camera's chief rays or a feathered outline."""

    stage = Stage.VIGNETTE
    pointwise = False
    falloff: Annotated[Natural | Shaped, msgspec.Meta(description="Lens law of the scene camera or a drawn outline")] = Natural()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Natural", "Shaped", "Vignette"]
