"""Tonal zone grade over blacks, shadows, midtones, highlights, and whites."""

import sys
import typing
from typing import Annotated, ClassVar, override

from expression import Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.effects.grade.primary import Wheel
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import markers, Presentation, Rgb, Stops

# --- [TYPES] ----------------------------------------------------------------------------

type Adjustment = Annotated[float, msgspec.Meta(ge=0.1, le=1.9), Presentation(step=Some(0.01), decimals=Some(3))]
type PivotAdjustment = Annotated[float, msgspec.Meta(ge=0.2, le=1.8), Presentation(step=Some(0.01), decimals=Some(3))]
type Tint = Annotated[float, msgspec.Meta(ge=0.0, le=2.0)]

# --- [MODELS] ---------------------------------------------------------------------------


class Zone[A: float](Wheel, frozen=True, kw_only=True):
    """Color, master adjustment, and placement of a tonal zone in stops."""

    color: Annotated[Rgb[Tint], msgspec.Meta(description="Channel color, white neutral, black and twice white spanning the adjustment range")] = Rgb(red=1.0, green=1.0, blue=1.0)
    master: Annotated[A, msgspec.Meta(description="All-channel adjustment, above 1 brighter")]
    start: Annotated[Stops, msgspec.Meta(description="Boundary nearest gray in stops from 0.18, midtone center")]
    width: Annotated[
        float,
        msgspec.Meta(ge=0.01, le=sys.float_info.max, description="Extent in stops, start to pivot for shadows and highlights"),
        Presentation(soft=Some((0.01, 16.0)), step=Some(0.1), decimals=Some(2)),
    ]

    @staticmethod
    def reach(kind: object) -> float:
        """Returns half the adjustment range about neutral 1 that `kind`, a `Zone` annotation, spans."""
        return next(meta.le for meta in markers(typing.get_args(kind)[0], msgspec.Meta) if meta.le is not None) - 1.0

    def values(self, width: float, reach: float) -> PyOpenColorIO.GradingRGBMSW:
        """Writes OCIO channel adjustments from color over `reach` about neutral, with width or pivot."""
        red, green, blue = (1.0 + reach * (channel - 1.0) for channel in (self.color.red, self.color.green, self.color.blue))
        return PyOpenColorIO.GradingRGBMSW(red, green, blue, self.master, self.start, width)


def zoned(values: PyOpenColorIO.GradingRGBMSW, width: float) -> Zone[float]:
    """Reads OCIO master value and zone extent in stops at neutral color."""
    return Zone(master=values.master, start=values.start, width=width)


class Zones(Effect, frozen=True, kw_only=True, tag=11):
    """Grades five scene-linear tonal zones per channel and applies s-contrast."""

    stage = Stage.ZONES
    title = "Tonal Zones"
    pointwise = True
    neutral: ClassVar[PyOpenColorIO.GradingTone] = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN)
    blacks: Zone[Adjustment] = zoned(neutral.blacks, neutral.blacks.width)
    shadows: Zone[PivotAdjustment] = zoned(neutral.shadows, neutral.shadows.start - neutral.shadows.width)
    midtones: Zone[Adjustment] = zoned(neutral.midtones, neutral.midtones.width)
    highlights: Zone[PivotAdjustment] = zoned(neutral.highlights, neutral.highlights.width - neutral.highlights.start)
    whites: Zone[Adjustment] = zoned(neutral.whites, neutral.whites.width)
    s_contrast: Annotated[
        float, msgspec.Meta(ge=0.01, le=1.99, title="S-Contrast", description="S-curve about middle gray with rolled-off ends, neutral at 1"), Presentation(step=Some(0.01), decimals=Some(3))
    ] = neutral.scontrast

    @classmethod
    def reaches(cls) -> dict[str, float]:
        """Returns adjustment half-range per zone field."""
        return {field.name: Zone.reach(field.type) for field in msgspec.structs.fields(cls) if typing.get_origin(field.type) is Zone}

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Builds OCIO scene-linear tonal zone operation."""
        tone, reaches = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN), self.reaches()
        blacks, shadows, midtones, highlights, whites, _ = self.__struct_fields__
        tone.blacks = self.blacks.values(self.blacks.width, reaches[blacks])
        tone.shadows = self.shadows.values(self.shadows.start - self.shadows.width, reaches[shadows])
        tone.midtones = self.midtones.values(self.midtones.width, reaches[midtones])
        tone.highlights = self.highlights.values(self.highlights.start + self.highlights.width, reaches[highlights])
        tone.whites = self.whites.values(self.whites.width, reaches[whites])
        tone.scontrast = self.s_contrast
        return Ok(Some(PyOpenColorIO.GradingToneTransform(values=tone, style=PyOpenColorIO.GRADING_LIN)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjustment", "PivotAdjustment", "Tint", "Zone", "Zones", "zoned"]
