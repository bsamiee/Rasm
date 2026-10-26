"""Primary grades in OCIO's linear style: gain and offset wheels, contrast about gray, and saturation with vibrance about working luminance."""

from collections.abc import Mapping

import bpy
from expression import Error, Nothing, Ok, Option, Some
import numpy as np
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, math, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.colors import AXES
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell
from blender.imaging.color.processors import luma
from blender.records.faults import Fault, Outcome, RecordReason

from ...chain.stages import Effect, Stage, transform

# --- [MODELS] ---------------------------------------------------------------------------


class Primary(Effect, frozen=True, kw_only=True):
    """Applies scene-linear gain after an additive offset, each a color wheel with master."""

    stage = Stage.PRIMARY
    pointwise = True


class Contrast(Effect, frozen=True, kw_only=True):
    """Raises scene-linear values to a power about a gray pivot, keeping sign."""

    stage = Stage.CONTRAST
    pointwise = True


class Saturation(Effect, frozen=True, kw_only=True):
    """Scales chroma about working-space luminance with saturation-dependent vibrance."""

    stage = Stage.SATURATION
    pointwise = True


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [OCIO]


@transform.register
def _(_record: Primary, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns `GradingPrimaryTransform` exposure of the gain wheel and offset of the offset wheel."""
    match values:
        case {
            "gain_color": (float(gain_red), float(gain_green), float(gain_blue), _),
            "gain_master": float(stops),
            "offset_color": (float(red), float(green), float(blue), _),
            "offset_master": float(added),
        }:
            linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
            linear.exposure = PyOpenColorIO.GradingRGBM(float(np.log2(gain_red)), float(np.log2(gain_green)), float(np.log2(gain_blue)), stops)
            linear.offset = PyOpenColorIO.GradingRGBM(red - 1.0, green - 1.0, blue - 1.0, added)
            return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))
        case _:
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=Primary.__name__, cause=repr(values)),))


@transform.register
def _(_record: Contrast, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns `GradingPrimaryTransform` master contrast about the pivot."""
    match values:
        case {"slope": float(slope), "pivot": float(pivot)}:
            linear = PyOpenColorIO.GradingPrimary(PyOpenColorIO.GRADING_LIN)
            linear.contrast, linear.pivot = PyOpenColorIO.GradingRGBM(1.0, 1.0, 1.0, slope), pivot
            return Ok(Some(PyOpenColorIO.GradingPrimaryTransform(values=linear, style=PyOpenColorIO.GRADING_LIN)))
        case _:
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=Contrast.__name__, cause=repr(values)),))


@transform.register
def _(_record: Saturation, values: Mapping[Key, Value], config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns OCIO's saturation matrix over working luminance, Nothing under vibrance, which has no OCIO operation."""
    match values:
        case {"vibrance": float(vibrance)} if vibrance != 0.0:
            return Ok(Nothing)
        case {"scale": float(scale)}:
            return luma(config).map(lambda weights: Some(PyOpenColorIO.MatrixTransform.Sat(scale, weights.tolist())))
        case _:
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=Saturation.__name__, cause=repr(values)),))


# --- [GRAPHS]


def wheeled(shell: Shell[Primary]) -> Outcome[Expr]:
    """Binds the offset wheel to the primary form's offset and the gain wheel to its exposure in stops."""
    tint, stops = shell.input("gain_color"), shell.input("gain_master")
    return Ok(
        grades.primary(
            shell.color,
            offset=vector("ADD", vector("SUBTRACT", shell.input("offset_color"), (1.0, 1.0, 1.0)), shell.input("offset_master")),
            exposure=xyz(*(math("LOGARITHM", tint[axis], 2.0) + stops for axis in AXES)),
        )
    )


def contrasted(shell: Shell[Contrast]) -> Outcome[Expr]:
    """Binds the slope to the primary form's contrast about the pivot."""
    slope = shell.input("slope")
    return Ok(grades.primary(shell.color, contrast=xyz(slope, slope, slope), pivot=Some(shell.input("pivot"))))


def saturated(shell: Shell[Saturation]) -> Outcome[Expr]:
    """Scales chroma about configured luminance by the saturation, raised for muted pixels and lowered for vivid ones by the vibrance."""
    scale, vibrance = shell.input("scale"), shell.input("vibrance")
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

primary = CompositorEffect(
    record=Primary,
    graph=wheeled,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        panels=tuple(
            InterfacePanel(
                key=key,
                name=bpy.path.display_name(key),
                description=description,
                inputs=(
                    Socket(key=item_key(key, "color"), name="Color", kind=bpy.types.NodeSocketColor, description=color, members={"default_value": (1.0, 1.0, 1.0, 1.0)}),
                    Socket(key=item_key(key, "master"), name="Master", kind=bpy.types.NodeSocketFloat, description=master),
                ),
            )
            for key, description, color, master in (
                ("gain", "Channel multiplier color and master gain in stops", "Channel multiplier color, white neutral", "Gain of every channel in stops"),
                ("offset", "Channel color less white and master value added to each channel", "Channel color, white neutral", "Value added to every channel"),
            )
        )
    ),
)
contrast = CompositorEffect(
    record=Contrast,
    graph=contrasted,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        inputs=(
            Socket(key="slope", name="Slope", kind=bpy.types.NodeSocketFloat, description="Power about pivot, slope in stops", members={"min_value": 0.01, "max_value": 2.0, "default_value": 1.0}),
            Socket(key="pivot", name="Pivot", kind=bpy.types.NodeSocketFloat, description="Pivot in stops from scene-linear gray", members={"min_value": -32.0, "max_value": 32.0}),
        )
    ),
)
saturation = CompositorEffect(
    record=Saturation,
    graph=saturated,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        inputs=(
            Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketFloat, description="Chroma scale about luminance", members={"min_value": 0.0, "default_value": 1.0}),
            Socket(key="vibrance", name="Vibrance", kind=bpy.types.NodeSocketFloat, description="Saturation gain weighted by pixel saturation", members={"min_value": -1.0, "max_value": 1.0}),
        )
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Contrast", "Primary", "Saturation", "contrast", "primary", "saturation"]
