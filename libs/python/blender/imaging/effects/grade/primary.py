"""Scene-linear gain and offset wheels, contrast about gray, and working-space saturation."""

from typing import Annotated, override

from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.imaging.chain import Effect, Stage
from blender.imaging.color.processors import luma
from blender.imaging.faults import ImagingResult
from blender.imaging.parameters import Finite, NonNegative, Presentation, Stops

# --- [TYPES] ----------------------------------------------------------------------------

type Component = Annotated[Finite, Presentation(soft=Some((-1.0, 1.0)), step=Some(0.01), decimals=Some(3))]

# --- [MODELS] ---------------------------------------------------------------------------


class Wheel(msgspec.Struct, frozen=True, kw_only=True):
    """Per-channel and master components of grade wheel."""

    red: Component = 0.0
    green: Component = 0.0
    blue: Component = 0.0
    master: Component = 0.0


class Primary(Effect, frozen=True, kw_only=True, tag=13):
    """Applies scene-linear gain in stops after an additive offset."""

    stage = Stage.PRIMARY
    title = "Primary Wheels"
    pointwise = True
    gain: Annotated[Wheel, msgspec.Meta(description="Channel gain in stops")] = Wheel()
    offset: Annotated[Wheel, msgspec.Meta(description="Scene-linear value added to each channel")] = Wheel()

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
        linear.exposure, linear.offset = (PyOpenColorIO.GradingRGBM(*msgspec.structs.astuple(wheel)) for wheel in (self.gain, self.offset))
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

__all__ = ["Component", "Contrast", "Primary", "Saturation", "Wheel"]
