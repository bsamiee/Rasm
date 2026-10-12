"""Tonal zone grade over blacks, shadows, midtones, highlights, and whites."""

import sys
from typing import Annotated, ClassVar, override

from expression import Ok, Option, Some
import msgspec
import PyOpenColorIO

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.faults import ImagingResult
from rasm.imaging.parameters import Presentation, Stops

# --- [TYPES] ----------------------------------------------------------------------------

type Adjustment = Annotated[float, msgspec.Meta(ge=0.1, le=1.9), Presentation(step=Some(0.01), decimals=Some(3))]
type PivotAdjustment = Annotated[float, msgspec.Meta(ge=0.2, le=1.8), Presentation(step=Some(0.01), decimals=Some(3))]

# --- [MODELS] ---------------------------------------------------------------------------


class Zone[A: float](msgspec.Struct, frozen=True, kw_only=True):
    """Channel adjustments and placement of a tonal zone in stops."""

    master: Annotated[A, msgspec.Meta(description="Zone adjustment on every channel, above 1 brighter")]
    red: Annotated[A, msgspec.Meta(description="Zone adjustment on the red channel, above 1 brighter")]
    green: Annotated[A, msgspec.Meta(description="Zone adjustment on the green channel, above 1 brighter")]
    blue: Annotated[A, msgspec.Meta(description="Zone adjustment on the blue channel, above 1 brighter")]
    start: Annotated[Stops, msgspec.Meta(description="Zone boundary nearest mid grey in stops about scene-linear 0.18, the center for midtones")]
    width: Annotated[
        float,
        msgspec.Meta(ge=0.01, le=sys.float_info.max, description="Zone extent in stops, from the start to the pivot for shadows and highlights"),
        Presentation(soft=Some((0.01, 16.0)), step=Some(0.1), decimals=Some(2)),
    ]

    def values(self, width: float) -> PyOpenColorIO.GradingRGBMSW:
        """Writes OCIO channel values with its width or pivot member."""
        return PyOpenColorIO.GradingRGBMSW(self.red, self.green, self.blue, self.master, self.start, width)


def zoned(values: PyOpenColorIO.GradingRGBMSW, width: float) -> Zone[float]:
    """Reads OCIO channel values with the zone's extent in stops."""
    return Zone(master=values.master, red=values.red, green=values.green, blue=values.blue, start=values.start, width=width)


class Zones(Effect, frozen=True, kw_only=True, tag=11):
    """Grades five scene-linear tonal zones per channel and applies s-contrast."""

    stage = Stage.ZONES
    title = "Tonal Zones"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    neutral: ClassVar[PyOpenColorIO.GradingTone] = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN)
    blacks: Zone[Adjustment] = zoned(neutral.blacks, neutral.blacks.width)
    shadows: Zone[PivotAdjustment] = zoned(neutral.shadows, neutral.shadows.start - neutral.shadows.width)
    midtones: Zone[Adjustment] = zoned(neutral.midtones, neutral.midtones.width)
    highlights: Zone[PivotAdjustment] = zoned(neutral.highlights, neutral.highlights.width - neutral.highlights.start)
    whites: Zone[Adjustment] = zoned(neutral.whites, neutral.whites.width)
    s_contrast: Annotated[
        float, msgspec.Meta(ge=0.01, le=1.99, title="S-Contrast", description="S-curve contrast about mid grey with rolled-off ends, 1 neutral"), Presentation(step=Some(0.01), decimals=Some(3))
    ] = neutral.scontrast

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Builds the OCIO scene-linear tonal zone operation."""
        tone = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN)
        tone.blacks, tone.midtones, tone.whites = (zone.values(zone.width) for zone in (self.blacks, self.midtones, self.whites))
        tone.shadows = self.shadows.values(self.shadows.start - self.shadows.width)
        tone.highlights = self.highlights.values(self.highlights.start + self.highlights.width)
        tone.scontrast = self.s_contrast
        return Ok(Some(PyOpenColorIO.GradingToneTransform(values=tone, style=PyOpenColorIO.GRADING_LIN)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjustment", "PivotAdjustment", "Zone", "Zones", "zoned"]
