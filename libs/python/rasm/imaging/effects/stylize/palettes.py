"""Display posterization with hue retention, ordered dithering, and luminance gradient mapping."""

import sys
from typing import Annotated

from expression import Some
import msgspec

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.parameters import Choice, Gated, Presentation, Quantity, Ramp

# --- [TYPES] ----------------------------------------------------------------------------


class Channels(Choice):
    """Color channels a posterize rounds."""

    RGB = 1, "RGB"
    LUMINANCE = 3, "Luminance"


# --- [MODELS] ---------------------------------------------------------------------------


class Posterize(msgspec.Struct, frozen=True, kw_only=True):
    """Rounds display values to evenly spaced levels with hue retention and Bayer dithering."""

    levels: Annotated[int, msgspec.Meta(ge=2, title="Levels", description="Values each channel rounds to, evenly spaced from 0 to 1"), Presentation(soft=Some((2.0, 256.0)))] = 9
    channels: Annotated[Channels, msgspec.Meta(title="Channels", description="Each RGB channel or luminance as grey")] = Channels.RGB
    retain_hue: Annotated[float, msgspec.Meta(ge=0.0, le=1.0, title="Retain Hue", description="Blend rounded RGB toward the input hue at rounded HSY saturation and luminance")] = 0.0
    dither: Annotated[Gated[Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]], msgspec.Meta(title="Dither", description="Ordered 8-by-8 Bayer offset of the rounding threshold in levels")] = Gated(
        enabled=False, value=1.0
    )
    dither_cell: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=0.0, le=sys.float_info.max), Presentation(quantity=Some(Quantity.FRAME_GREATER))]],
        msgspec.Meta(title="Dither Cell", description="Square Bayer cell size; disabled or below one pixel uses one pixel"),
    ] = Gated(enabled=False, value=0.0)


class Palette(Effect, frozen=True, kw_only=True, tag=43):
    """Reduces display colors to evenly spaced levels, then maps their luminance through a color ramp."""

    stage = Stage.PALETTE
    title = "Palette"
    pointwise = False
    posterize: Annotated[Gated[Posterize], msgspec.Meta(title="Posterize", description="Rounding to evenly spaced levels")] = Gated(enabled=True, value=Posterize())
    gradient: Annotated[Gated[Ramp], msgspec.Meta(title="Gradient Map", description="Luminance mapped through a color ramp")] = Gated(enabled=False, value=Ramp())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Channels", "Palette", "Posterize"]
