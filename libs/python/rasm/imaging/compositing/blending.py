"""Blend modes and weighted compositing shared by effects, layers, and documents."""

from typing import Annotated, Generic, TypeVar

from expression import Some
import msgspec
import psdtags

from rasm.imaging.chain import Effect
from rasm.imaging.compositing.masks import Mask
from rasm.imaging.parameters import Choice, Presentation

# --- [TYPES] ----------------------------------------------------------------------------

_E_co = TypeVar("_E_co", bound=Effect, covariant=True)


class BlendMode(Choice):
    """Blender blend formulas with Photoshop-compatible names."""

    NORMAL = 1, "Normal"
    DARKEN = 2, "Darken"
    MULTIPLY = 3, "Multiply"
    COLOR_BURN = 4, "Color Burn"
    LIGHTEN = 5, "Lighten"
    SCREEN = 6, "Screen"
    COLOR_DODGE = 7, "Color Dodge"
    LINEAR_DODGE = 8, "Add"
    OVERLAY = 9, "Overlay"
    SOFT_LIGHT = 10, "Soft Light"
    LINEAR_LIGHT = 11, "Linear Light"
    DIFFERENCE = 12, "Difference"
    EXCLUSION = 13, "Exclusion"
    SUBTRACT = 14, "Subtract"
    DIVIDE = 15, "Divide"
    HUE = 16, "Hue"
    SATURATION = 17, "Saturation"
    COLOR = 18, "Color"
    VALUE = 19, "Value"

    @property
    def photoshop(self) -> psdtags.PsdBlendMode:
        """Photoshop mode, using luminosity for the value formula."""
        return psdtags.PsdBlendMode.LUMINOSITY if self is BlendMode.VALUE else psdtags.PsdBlendMode[self.name]


# --- [MODELS] ---------------------------------------------------------------------------


class Blend(msgspec.Struct, frozen=True, kw_only=True):
    """Mode, opacity, and masks weighing a result over its associated base."""

    mode: Annotated[BlendMode, msgspec.Meta(title="Blend", description="Formula joining the result with its base")] = BlendMode.NORMAL
    opacity: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, description="Share of the result mixed over its base"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    masks: Annotated[tuple[Mask, ...], msgspec.Meta(description="Keys limiting where the result mixes back")] = ()


class Entry(msgspec.Struct, Generic[_E_co], frozen=True, kw_only=True):
    """Effect and the blend weighing its result over its input."""

    effect: _E_co
    blend: Annotated[Blend, msgspec.Meta(description="Mode, opacity, and masks of the effect's result")] = Blend()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Blend", "BlendMode", "Entry"]
