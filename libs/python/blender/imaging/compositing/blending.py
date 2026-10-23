"""Blend modes and blends shared by effects, layers, and documents."""

from typing import Annotated, Generic, TypeVar

from expression import Some
import msgspec
import psdtags

from blender.imaging.chain import Effect
from blender.imaging.compositing.masks import Mask
from blender.imaging.parameters import Choice, Presentation, Unit

# --- [TYPES] ----------------------------------------------------------------------------

_E_co = TypeVar("_E_co", bound=Effect, covariant=True)


class BlendMode(Choice):
    """Blender blend formulas with Photoshop-compatible names."""

    NORMAL = 1
    DARKEN = 2
    MULTIPLY = 3
    COLOR_BURN = 4
    LIGHTEN = 5
    SCREEN = 6
    COLOR_DODGE = 7
    LINEAR_DODGE = 8, "Add"
    OVERLAY = 9
    SOFT_LIGHT = 10
    LINEAR_LIGHT = 11
    DIFFERENCE = 12
    EXCLUSION = 13
    SUBTRACT = 14
    DIVIDE = 15
    HUE = 16
    SATURATION = 17
    COLOR = 18
    VALUE = 19

    @property
    def photoshop(self) -> psdtags.PsdBlendMode:
        """Photoshop mode, luminosity for value formula."""
        return psdtags.PsdBlendMode.LUMINOSITY if self is BlendMode.VALUE else psdtags.PsdBlendMode[self.name]


# --- [MODELS] ---------------------------------------------------------------------------


class Blend(msgspec.Struct, frozen=True, kw_only=True):
    """Mode, opacity, and masks mixing result over base."""

    mode: Annotated[BlendMode, msgspec.Meta(title="Blend", description="Formula mixing result with base")] = BlendMode.NORMAL
    opacity: Annotated[Unit, msgspec.Meta(description="Share of result mixed over base"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    masks: Annotated[tuple[Mask, ...], msgspec.Meta(description="Keys limiting where result mixes")] = ()


class Entry(msgspec.Struct, Generic[_E_co], frozen=True, kw_only=True):
    """Effect with blend over its input."""

    effect: _E_co
    blend: Annotated[Blend, msgspec.Meta(description="Mix of effect result over input")] = Blend()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Blend", "BlendMode", "Entry"]
