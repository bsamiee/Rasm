"""Scene-linear gain and offset wheels, contrast about gray, and working-space saturation."""

import math
from typing import Annotated, override

from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import luma
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import Finite, NonNegative, Positive, Presentation, Rgb, Stops

# --- [TYPES] ----------------------------------------------------------------------------

type Component = Annotated[Finite, Presentation(soft=Some((-1.0, 1.0)), step=Some(0.01), decimals=Some(3))]

# --- [MODELS] ---------------------------------------------------------------------------


class Wheel(msgspec.Struct, frozen=True, kw_only=True):
    """Grade wheel color, white neutral, and master component."""

    color: Annotated[Rgb[Finite], msgspec.Meta(description="Channel color, white neutral")] = Rgb(red=1.0, green=1.0, blue=1.0)
    master: Component = 0.0


class Gain(Wheel, frozen=True, kw_only=True):
    """Gain wheel of a positive channel multiplier color and master stops."""

    color: Annotated[Rgb[Positive], msgspec.Meta(description="Channel multiplier color, white neutral")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Primary(Effect, frozen=True, kw_only=True, tag=13):
    """Applies scene-linear gain after an additive offset, each a color wheel with master."""

    stage = Stage.PRIMARY
    title = "Primary Wheels"
    pointwise = True
    gain: Annotated[Gain, msgspec.Meta(description="Channel multiplier color and master gain in stops")] = Gain()
    offset: Annotated[Wheel, msgspec.Meta(description="Channel color less white and master value added to each channel")] = Wheel()

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
        gain, offset = self.gain.color, self.offset.color
        linear.exposure = PyOpenColorIO.GradingRGBM(math.log2(gain.red), math.log2(gain.green), math.log2(gain.blue), self.gain.master)
        linear.offset = PyOpenColorIO.GradingRGBM(offset.red - 1.0, offset.green - 1.0, offset.blue - 1.0, self.offset.master)
        return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))


class Contrast(Effect, frozen=True, kw_only=True, tag=14):
    """Raises scene-linear values to a power about gray pivot, keeping sign."""

    stage = Stage.CONTRAST
    pointwise = True
    slope: Annotated[float, msgspec.Meta(ge=0.01, le=2.0, title="Contrast", description="Power about pivot, slope in stops"), Presentation(step=Some(0.01), decimals=Some(3))] = 1.0
    pivot: Annotated[Stops, msgspec.Meta(description="Pivot in stops from scene-linear gray")] = 0.0

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
        linear.contrast, linear.pivot = PyOpenColorIO.GradingRGBM(1.0, 1.0, 1.0, self.slope), self.pivot
        return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))


class Saturation(Effect, frozen=True, kw_only=True, tag=15):
    """Scales chroma about working-space luminance with saturation-dependent vibrance."""

    stage = Stage.SATURATION
    pointwise = True
    scale: Annotated[NonNegative, msgspec.Meta(title="Saturation", description="Chroma scale about luminance"), Presentation(soft=Some((0.0, 2.0)), step=Some(0.01), decimals=Some(3))] = 1.0
    vibrance: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Saturation gain weighted by pixel saturation"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.0

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        return Ok(Nothing) if self.vibrance != 0.0 else luma(config).map(lambda weights: Some(PyOpenColorIO.MatrixTransform.Sat(self.scale, weights.tolist())))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Component", "Contrast", "Gain", "Primary", "Saturation", "Wheel"]
