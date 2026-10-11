"""Scene-linear gain and offset wheels, contrast about grey, and working-space saturation."""

import sys
from typing import Annotated, ClassVar, override

from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from rasm.imaging.chain import Effect, Stage
from rasm.imaging.color.processors import luma
from rasm.imaging.faults import ImagingResult
from rasm.imaging.parameters import Presentation, Stops

# --- [TYPES] ------------------------------------------------------------------------------

type Component = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max), Presentation(soft=Some((-1.0, 1.0)), step=Some(0.01), decimals=Some(3))]

# --- [MODELS] -----------------------------------------------------------------------------


class Wheel(msgspec.Struct, frozen=True, kw_only=True):
    """Per-channel and master components of one grade wheel."""

    red: Component = 0.0
    green: Component = 0.0
    blue: Component = 0.0
    master: Component = 0.0


class Primary(Effect, frozen=True, kw_only=True, tag=13):
    """Applies scene-linear gain in stops after an additive offset."""

    stage = Stage.PRIMARY
    title = "Primary Wheels"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    gain: Annotated[Wheel, msgspec.Meta(title="Gain", description="Channel gain in stops")] = Wheel()
    offset: Annotated[Wheel, msgspec.Meta(title="Offset", description="Scene-linear value added to each channel")] = Wheel()

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
        linear.exposure, linear.offset = (PyOpenColorIO.GradingRGBM(*msgspec.structs.astuple(wheel)) for wheel in (self.gain, self.offset))
        return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))


class Contrast(Effect, frozen=True, kw_only=True, tag=14):
    """Raises scene-linear values to a power about a grey pivot, keeping each sign."""

    stage = Stage.CONTRAST
    title = "Contrast"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    slope: Annotated[float, msgspec.Meta(ge=0.01, le=2.0, title="Contrast", description="Power about the pivot, equal to a slope in stops"), Presentation(step=Some(0.01), decimals=Some(3))] = 1.0
    pivot: Annotated[Stops, msgspec.Meta(title="Pivot", description="Grey the contrast turns about, in stops from scene-linear grey")] = 0.0

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
        linear.contrast, linear.pivot = PyOpenColorIO.GradingRGBM(1.0, 1.0, 1.0, self.slope), self.pivot
        return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))


class Saturation(Effect, frozen=True, kw_only=True, tag=15):
    """Scales chroma about working-space luminance with saturation-dependent vibrance."""

    stage = Stage.SATURATION
    title = "Saturation"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_SCENE_LINEAR
    scale: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=sys.float_info.max, title="Saturation", description="Chroma scale about the working space's luminance"),
        Presentation(soft=Some((0.0, 2.0)), step=Some(0.01), decimals=Some(3)),
    ] = 1.0
    vibrance: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, title="Vibrance", description="Saturation gain weighted by the pixel's saturation"), Presentation(step=Some(0.01), decimals=Some(3))] = 0.0

    @override
    def transform(self, config: PyOpenColorIO.Config) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        return Ok(Nothing) if self.vibrance != 0.0 else luma(config).map(lambda weights: Some(PyOpenColorIO.MatrixTransform.Sat(self.scale, weights.tolist())))


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ["Component", "Contrast", "Primary", "Saturation", "Wheel"]
