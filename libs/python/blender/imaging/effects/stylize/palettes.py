"""Display posterization with hue retention, ordered dithering, and luminance gradient mapping."""

from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.parameters import Choice, FrameLength, Gated, Presentation, Ramp, Unit

# --- [TYPES] ----------------------------------------------------------------------------


class Channels(Choice):
    """Color channels a posterize rounds."""

    RGB = 1, "RGB"
    LUMINANCE = 3


# --- [MODELS] ---------------------------------------------------------------------------


class Posterize(msgspec.Struct, frozen=True, kw_only=True):
    """Rounds display values to evenly spaced levels with hue retention and Bayer dithering."""

    levels: Annotated[int, msgspec.Meta(ge=2, description="Values each channel rounds to, evenly spaced from 0 to 1"), Presentation(soft=Some((2.0, 256.0)))] = 9
    channels: Annotated[Channels, msgspec.Meta(description="Each RGB channel or luminance as gray")] = Channels.RGB
    retain_hue: Annotated[Unit, msgspec.Meta(description="Blend toward input hue at rounded HSY saturation and luminance")] = 0.0
    dither: Annotated[Gated[Unit], msgspec.Meta(description="Ordered 8-by-8 Bayer offset of rounding threshold in levels")] = Gated(enabled=False, value=1.0)
    dither_cell: Annotated[Gated[Annotated[FrameLength, Presentation(soft=Some((0.0, 0.01)))]], msgspec.Meta(description="Square Bayer cell size, one pixel when off or smaller")] = Gated(
        enabled=False, value=0.0
    )


class Palette(Effect, frozen=True, kw_only=True, tag=43):
    """Rounds display colors to evenly spaced levels, then maps luminance through a color ramp."""

    stage = Stage.PALETTE
    pointwise = False
    posterize: Annotated[Gated[Posterize], msgspec.Meta(description="Rounding to evenly spaced levels")] = Gated(enabled=True, value=Posterize())
    gradient: Annotated[Gated[Ramp], msgspec.Meta(title="Gradient Map", description="Luminance mapped through a color ramp")] = Gated(enabled=False, value=Ramp())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channels", "Palette", "Posterize"]
