"""Tonal zone grade of OCIO's linear tone transform over blacks, shadows, midtones, highlights, and whites, with s-contrast."""

import sys
import typing
from typing import Annotated, ClassVar, override

from expression import Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, vector
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.colors import AXES
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell
from blender.imaging.color.processors import Stops
from blender.imaging.faults import ImagingResult
from blender.records.fields import markers, Presentation, Rgb

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Adjustment = Annotated[float, msgspec.Meta(ge=0.1, le=1.9), Presentation(step=Some(0.01), precision=Some(3))]
type PivotAdjustment = Annotated[float, msgspec.Meta(ge=0.2, le=1.8), Presentation(step=Some(0.01), precision=Some(3))]

# --- [MODELS] ---------------------------------------------------------------------------


class Zone[A: float](msgspec.Struct, frozen=True, kw_only=True):
    """Color, master adjustment, and placement in stops of one tonal zone."""

    color: Annotated[Rgb[Annotated[float, msgspec.Meta(ge=0.0, le=2.0)]], msgspec.Meta(description="Channel color, white neutral, black and twice white spanning the adjustment range")] = Rgb(
        red=1.0, green=1.0, blue=1.0
    )
    master: Annotated[A, msgspec.Meta(description="All-channel adjustment, above 1 brighter")]
    start: Annotated[Stops, msgspec.Meta(description="Boundary nearest gray in stops from 0.18, midtone center")]
    width: Annotated[
        float,
        msgspec.Meta(ge=0.01, le=sys.float_info.max, description="Extent in stops, start to pivot for shadows and highlights"),
        Presentation(soft=Some((0.01, 16.0)), step=Some(0.1), precision=Some(2)),
    ]

    @staticmethod
    def reach(kind: object) -> float:
        """Returns half the adjustment range about neutral 1 that `kind`, a `Zone` annotation, spans."""
        return next(meta.le for meta in markers(typing.get_args(kind)[0], msgspec.Meta) if meta.le is not None) - 1.0

    @classmethod
    def of(cls, values: PyOpenColorIO.GradingRGBMSW, width: float) -> "Zone[float]":
        """Reads OCIO's master and start at neutral color with the zone's extent in stops."""
        return Zone(master=values.master, start=values.start, width=width)

    def values(self, width: float, reach: float) -> PyOpenColorIO.GradingRGBMSW:
        """Writes OCIO channel adjustments from color over `reach` about neutral, with width or pivot."""
        red, green, blue = (1.0 + reach * (channel - 1.0) for channel in msgspec.structs.astuple(self.color))
        return PyOpenColorIO.GradingRGBMSW(red, green, blue, self.master, self.start, width)


class Zones(Effect, frozen=True, kw_only=True):
    """Grades five scene-linear tonal zones per channel and applies s-contrast."""

    stage = Stage.ZONES
    title = "Tonal Zones"
    pointwise = True
    neutral: ClassVar[PyOpenColorIO.GradingTone] = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN)
    blacks: Zone[Adjustment] = Zone.of(neutral.blacks, neutral.blacks.width)
    shadows: Zone[PivotAdjustment] = Zone.of(neutral.shadows, neutral.shadows.start - neutral.shadows.width)
    midtones: Zone[Adjustment] = Zone.of(neutral.midtones, neutral.midtones.width)
    highlights: Zone[PivotAdjustment] = Zone.of(neutral.highlights, neutral.highlights.width - neutral.highlights.start)
    whites: Zone[Adjustment] = Zone.of(neutral.whites, neutral.whites.width)
    s_contrast: Annotated[
        float, msgspec.Meta(ge=0.01, le=1.99, title="S-Contrast", description="S-curve about middle gray with rolled-off ends, neutral at 1"), Presentation(step=Some(0.01), precision=Some(3))
    ] = neutral.scontrast

    @classmethod
    def reaches(cls) -> dict[str, float]:
        """Returns the adjustment half-range of each zone field."""
        return {field.name: Zone.reach(field.type) for field in msgspec.structs.fields(cls) if typing.get_origin(field.type) is Zone}

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's linear tonal zone operation, shadows and highlights placed by pivot."""
        tone, reaches = PyOpenColorIO.GradingTone(PyOpenColorIO.GRADING_LIN), self.reaches()
        blacks, shadows, midtones, highlights, whites, _ = self.__struct_fields__
        tone.blacks = self.blacks.values(self.blacks.width, reaches[blacks])
        tone.shadows = self.shadows.values(self.shadows.start - self.shadows.width, reaches[shadows])
        tone.midtones = self.midtones.values(self.midtones.width, reaches[midtones])
        tone.highlights = self.highlights.values(self.highlights.start + self.highlights.width, reaches[highlights])
        tone.whites = self.whites.values(self.whites.width, reaches[whites])
        tone.scontrast = self.s_contrast
        return Ok(Some(PyOpenColorIO.GradingToneTransform(values=tone, style=PyOpenColorIO.GRADING_LIN)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def zoned(shell: Shell[Zones]) -> BlenderResult[Expr]:
    """Binds each zone's color over its adjustment range, master, and placement, and the s-contrast, to the node form of OCIO's tone transform."""
    *_, s_contrast = Zones.__struct_fields__
    color, master, start, width = Zone.__struct_fields__

    def zone(key: str, reach: float) -> grades.Zone:
        tint = vector("MULTIPLY_ADD", shell.input(key, color), (reach,) * 3, (1.0 - reach,) * 3)
        red, green, blue = (tint[axis] for axis in AXES)
        return grades.Zone(rgb=(red, green, blue), master=shell.input(key, master), start=shell.input(key, start), width=shell.input(key, width))

    held = {key: zone(key, reach) for key, reach in Zones.reaches().items()}
    blacks, shadows, midtones, highlights, whites = held.values()
    return grades.tone(shell.color, blacks=blacks, shadows=shadows, midtones=midtones, highlights=highlights, whites=whites, contrast=shell.input(s_contrast))


# --- [COMPOSITION] ----------------------------------------------------------------------

zones = CompositorEffect(record=Zones, graph=zoned, alpha=Alpha.STRAIGHT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Zone", "Zones", "zones"]
