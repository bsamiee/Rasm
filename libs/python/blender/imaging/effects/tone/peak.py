"""BT.2390 peak mapping parameters for absolute PQ display codes."""

from typing import Annotated

from expression import Some
import msgspec

from blender.imaging.chain import Effect, Stage
from blender.imaging.parameters import Presentation, Quantity, Unit

# --- [TYPES] ----------------------------------------------------------------------------

type Luminance = Annotated[float, msgspec.Meta(ge=0.0, le=10000.0), Presentation(soft=Some((100.0, 10000.0)), step=Some(1.0), decimals=Some(0), quantity=Some(Quantity.LUMINANCE))]

# --- [MODELS] ---------------------------------------------------------------------------


class PeakMapping(Effect, frozen=True, kw_only=True, tag=7):
    """Per-channel PQ peak compression with Hermite roll-off and black lift."""

    stage = Stage.PEAK
    pointwise = True
    source_peak: Annotated[Luminance, msgspec.Meta(description="Brightest frame luminance")] = 10000.0
    target_peak: Annotated[Luminance, msgspec.Meta(description="Display peak luminance")] = 1000.0
    black: Annotated[
        Unit,
        msgspec.Meta(title="Display Black", description="Share of fitted peak luminance, inverse of display contrast ratio"),
        Presentation(soft=Some((0.0, 0.001)), step=Some(0.00001), decimals=Some(6)),
    ] = 0.0
    knee_offset: Annotated[float, msgspec.Meta(ge=0.5, le=2.0, description="Offset from fitted peak down to knee, 0.5 in BT.2390"), Presentation(step=Some(0.01), decimals=Some(2))] = 0.5


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Luminance", "PeakMapping"]
