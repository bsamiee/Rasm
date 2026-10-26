"""Channel mixing or color-space conversion by union menu."""

import bpy
from expression import Ok

from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.spaces import space_item
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Shell
from blender.imaging.effects.grade.primaries import Conversion, Mixer, Primaries, Weights
from blender.imaging.faults import paired

# --- [OPERATIONS] -----------------------------------------------------------------------


def remapped(shell: Shell[Primaries]) -> BlenderResult[Expr]:
    """Returns both remap branches behind animatable union menu.

    Args:
        shell: Entry inputs, menu items, and retained conversion parameters.

    Returns:
        Remapped color, or accumulated space faults.
    """
    (remap,) = Primaries.__struct_fields__
    matrix = Expr(
        kind=bpy.types.FunctionNodeCombineMatrix,
        inputs=tuple(
            (f"Column {column} Row {row}", shell.input(remap, Mixer, output, source))
            for row, output in enumerate(Mixer.__struct_fields__, 1)
            for column, source in enumerate(Weights.__struct_fields__, 1)
        ),
        socket="Matrix",
    )
    mixer = Expr(kind=bpy.types.FunctionNodeTransformDirection, inputs=(("Direction", shell.color), ("Transform", matrix)), socket="Direction", shape="VECTOR")

    def converted(spaces: tuple[tuple[str, str], ...]) -> Expr:
        conversion = Expr(kind=bpy.types.CompositorNodeConvertColorSpace, inputs=(("Image", shell.color),), members=spaces, socket="Image", shape="RGBA")
        return shell.menu(remap, cases={Mixer.title: mixer, Conversion.title: conversion}, kind="RGBA")

    spaces: BlenderResult[tuple[tuple[str, str], ...]] = (
        shell
        .case(Conversion, remap)
        .map(lambda kept: paired(space_item(shell.config, kept.source), space_item(shell.config, kept.target)).map(lambda named: tuple(zip(("from_color_space", "to_color_space"), named))))
        .default_value(Ok(()))
    )
    return spaces.map(converted)


# --- [COMPOSITION] ----------------------------------------------------------------------

primaries = CompositorEffect(record=Primaries, graph=remapped, alpha=Alpha.STRAIGHT)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["primaries", "remapped"]
