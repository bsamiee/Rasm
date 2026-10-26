"""Primaries remap before gamut compression: unclamped channel mixing or colorspace interpretation through OCIO."""

from typing import Annotated, override

import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, xyz
from blender.bpy.render.compositor.colors import space_item
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Shell
from blender.bpy.render.compositor.grades import matrixed
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.processors import ColorSpace, matrix
from blender.imaging.faults import ImagingResult
from blender.records.faults import paired
from blender.records.fields import Case, Finite, Presentation

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Weight = Annotated[Finite, Presentation(soft=Some((-2.0, 2.0)), precision=Some(3))]
type Weights = Annotated[tuple[Weight, Weight, Weight], msgspec.Meta(description="Red, green, and blue input weights summed by one output channel")]
type Remap = Mixer | Conversion

# --- [MODELS] ---------------------------------------------------------------------------


class Mixer(Case, frozen=True, kw_only=True):
    """Linear channel mixing with signed weights and unclamped output."""

    red: Annotated[Weights, msgspec.Meta(title="Red")] = (1.0, 0.0, 0.0)
    green: Annotated[Weights, msgspec.Meta(title="Green")] = (0.0, 1.0, 0.0)
    blue: Annotated[Weights, msgspec.Meta(title="Blue")] = (0.0, 0.0, 1.0)

    @property
    def transform(self) -> PyOpenColorIO.MatrixTransform:
        """Fresh matrix operation of the weight rows, alpha passed through."""
        return matrix(np.array(msgspec.structs.astuple(self), dtype=np.float64))


class Conversion(Case, frozen=True, kw_only=True):
    """Colorspace interpretation and conversion through the OCIO config."""

    source: Annotated[ColorSpace, GraphForm.COLOR_SPACE, msgspec.Meta(description="Colorspace values read as")] = PyOpenColorIO.ROLE_SCENE_LINEAR
    target: Annotated[ColorSpace, GraphForm.COLOR_SPACE, msgspec.Meta(description="Colorspace values convert to")] = PyOpenColorIO.ROLE_SCENE_LINEAR

    @property
    def transform(self) -> PyOpenColorIO.ColorSpaceTransform:
        """Fresh conversion from source to target."""
        return PyOpenColorIO.ColorSpaceTransform(src=self.source, dst=self.target)


class Primaries(Effect, frozen=True, kw_only=True):
    """Remaps scene-linear channels before gamut compression."""

    stage = Stage.PRIMARIES
    pointwise = True
    remap: Annotated[Remap, msgspec.Meta(description="Channel mixing or colorspace interpretation")] = Mixer()

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the remap's OCIO operation."""
        return Ok(Some(self.remap.transform))


# --- [OPERATIONS] -----------------------------------------------------------------------


def remapped(shell: Shell[Primaries]) -> BlenderResult[Expr]:
    """Switches at the remap menu between the matrix form of the mixer weights and a colorspace conversion of the held conversion's spaces."""
    (remap,) = Primaries.__struct_fields__
    red, green, blue = (shell.input(remap, Mixer, output) for output in Mixer.__struct_fields__)
    mixer = matrixed(shell.color, (red, green, blue), xyz())

    def converted(spaces: tuple[tuple[str, str], ...]) -> Expr:
        conversion = Expr(kind=bpy.types.CompositorNodeConvertColorSpace, inputs=(("Image", shell.color),), members=spaces, socket="Image", shape="RGBA")
        return shell.menu(remap, cases={Mixer.title: mixer, Conversion.title: conversion}, kind="RGBA")

    spaces: BlenderResult[tuple[tuple[str, str], ...]] = (
        shell
        .case(Conversion, remap)
        .map(lambda kept: paired(space_item(shell.config, kept.source), space_item(shell.config, kept.target)).map(lambda named: (("from_color_space", named[0]), ("to_color_space", named[1]))))
        .default_value(Ok(()))
    )
    return spaces.map(converted)


# --- [COMPOSITION] ----------------------------------------------------------------------

primaries = CompositorEffect(record=Primaries, graph=remapped, alpha=Alpha.STRAIGHT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Conversion", "Mixer", "Primaries", "primaries"]
