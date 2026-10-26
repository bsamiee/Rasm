"""Primary grades in OCIO's linear style: gain and offset wheels, contrast about gray, and saturation with vibrance about working luminance."""

from typing import Annotated, override

from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.colors import AXES
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell
from blender.imaging.color.processors import luma, Stops
from blender.imaging.faults import ImagingResult
from blender.records.fields import Finite, NonNegative, Positive, Presentation, Rgb

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Wheel(msgspec.Struct, frozen=True, kw_only=True):
    """Grade wheel color, white neutral, and master component."""

    color: Annotated[Rgb[Finite], msgspec.Meta(description="Channel color, white neutral")] = Rgb(red=1.0, green=1.0, blue=1.0)
    master: Annotated[Finite, Presentation(soft=Some((-1.0, 1.0)), step=Some(0.01), precision=Some(3))] = 0.0


class Gain(Wheel, frozen=True, kw_only=True):
    """Gain wheel of a positive channel multiplier color and master stops."""

    color: Annotated[Rgb[Positive], msgspec.Meta(description="Channel multiplier color, white neutral")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Primary(Effect, frozen=True, kw_only=True):
    """Applies scene-linear gain after an additive offset, each a color wheel with master."""

    stage = Stage.PRIMARY
    title = "Primary Wheels"
    pointwise = True
    gain: Annotated[Gain, msgspec.Meta(description="Channel multiplier color and master gain in stops")] = Gain()
    offset: Annotated[Wheel, msgspec.Meta(description="Channel color less white and master value added to each channel")] = Wheel()

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns `GradingPrimaryTransform` exposure of the gain wheel and offset of the offset wheel."""
        linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
        red, green, blue = (float(np.log2(channel)) for channel in msgspec.structs.astuple(self.gain.color))
        linear.exposure = PyOpenColorIO.GradingRGBM(red, green, blue, self.gain.master)
        red, green, blue = (channel - 1.0 for channel in msgspec.structs.astuple(self.offset.color))
        linear.offset = PyOpenColorIO.GradingRGBM(red, green, blue, self.offset.master)
        return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))


class Contrast(Effect, frozen=True, kw_only=True):
    """Raises scene-linear values to a power about a gray pivot, keeping sign."""

    stage = Stage.CONTRAST
    pointwise = True
    slope: Annotated[float, msgspec.Meta(ge=0.01, le=2.0, title="Contrast", description="Power about pivot, slope in stops"), Presentation(step=Some(0.01), precision=Some(3))] = 1.0
    pivot: Annotated[Stops, msgspec.Meta(description="Pivot in stops from scene-linear gray")] = 0.0

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns `GradingPrimaryTransform` master contrast about the pivot."""
        linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
        linear.contrast, linear.pivot = PyOpenColorIO.GradingRGBM(1.0, 1.0, 1.0, self.slope), self.pivot
        return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))


class Saturation(Effect, frozen=True, kw_only=True):
    """Scales chroma about working-space luminance with saturation-dependent vibrance."""

    stage = Stage.SATURATION
    pointwise = True
    scale: Annotated[NonNegative, msgspec.Meta(title="Saturation", description="Chroma scale about luminance"), Presentation(soft=Some((0.0, 2.0)), step=Some(0.01), precision=Some(3))] = 1.0
    vibrance: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0, description="Saturation gain weighted by pixel saturation"), Presentation(step=Some(0.01), precision=Some(3))] = 0.0

    @override
    def transform(self, config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's saturation matrix over working luminance, Nothing under vibrance, which has no OCIO operation."""
        return Ok(Nothing) if self.vibrance != 0.0 else luma(config).map(lambda weights: Some(PyOpenColorIO.MatrixTransform.Sat(self.scale, weights.tolist())))


# --- [OPERATIONS] -----------------------------------------------------------------------


def wheeled(shell: Shell[Primary]) -> BlenderResult[Expr]:
    """Binds the offset wheel to the primary form's offset and the gain wheel to its exposure in stops."""
    color, master = Wheel.__struct_fields__
    gain, offset = Primary.__struct_fields__
    tint, stops = shell.input(gain, color), shell.input(gain, master)
    return Ok(
        grades.primary(
            shell.color,
            offset=vector("ADD", vector("SUBTRACT", shell.input(offset, color), (1.0, 1.0, 1.0)), shell.input(offset, master)),
            exposure=xyz(*(math("LOGARITHM", tint[axis], 2.0) + stops for axis in AXES)),
        )
    )


def contrasted(shell: Shell[Contrast]) -> BlenderResult[Expr]:
    """Binds the slope to the primary form's contrast about the pivot."""
    slope, pivot = map(shell.input, Contrast.__struct_fields__)
    return Ok(grades.primary(shell.color, contrast=xyz(slope, slope, slope), pivot=Some(pivot)))


def saturated(shell: Shell[Saturation]) -> BlenderResult[Expr]:
    """Scales chroma about configured luminance by the saturation, raised for muted pixels and lowered for vivid ones by the vibrance."""
    scale, vibrance = map(shell.input, Saturation.__struct_fields__)
    color = shell.color
    red, green, blue = (color[axis] for axis in AXES)
    sigma = 1.0 - math("MAXIMUM", math("MINIMUM", math("MINIMUM", red, green), blue), 0.0) / math("MAXIMUM", math("MAXIMUM", red, green), blue)
    inverse = 1.0 - sigma
    factor = scale * math("MULTIPLY_ADD", vibrance, switched(vibrance > 0.0, sigma * sigma * 0.5, inverse * inverse), 1.0)

    def built(weights: tuple[float, ...]) -> Expr:
        luminance = vector("DOT_PRODUCT", color, weights)
        return vector("MULTIPLY_ADD", vector("SUBTRACT", color, luminance), factor, luminance)

    return luma(shell.config).map(lambda values: built(tuple(map(float, values))))


# --- [COMPOSITION] ----------------------------------------------------------------------

primary = CompositorEffect(record=Primary, graph=wheeled, alpha=Alpha.STRAIGHT)
contrast = CompositorEffect(record=Contrast, graph=contrasted, alpha=Alpha.STRAIGHT)
saturation = CompositorEffect(record=Saturation, graph=saturated, alpha=Alpha.STRAIGHT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Contrast", "Gain", "Primary", "Saturation", "Wheel", "contrast", "primary", "saturation"]
