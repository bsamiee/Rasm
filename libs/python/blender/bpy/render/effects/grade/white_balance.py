"""White-point balance over scene-linear color."""

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.faults import BlenderFaults, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Measured, Shell
from blender.bpy.render.effects.viewers import Unread
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.white import White
from blender.imaging.effects.grade.white_balance import WhiteBalance
from blender.imaging.files.images import ImageFile
from blender.imaging.parameters import heading, Rgb

# --- [OPERATIONS] -----------------------------------------------------------------------


def balanced(shell: Shell[WhiteBalance]) -> BlenderResult[Expr]:
    """Builds white-point balance from record white sockets."""
    whites = tuple((f"{side} {heading(field)}", shell.input(key, field)) for side, key in zip(("Input", "Output"), WhiteBalance.__struct_fields__) for field in White.__struct_fields__)
    return Ok(Expr(kind=bpy.types.CompositorNodeColorBalance, inputs=(("Image", shell.color), ("Fac", 1.0), ("Type", "White Point"), *whites), socket="Image", shape="RGBA"))


def measured(config: PyOpenColorIO.Config, record: WhiteBalance, file: ImageFile[np.float32]) -> BlenderResult[Option[WhiteBalance]]:
    """Sets source white to the covered mean of the entering frame."""
    return Result[Option[White], BlenderFaults].bind(
        Samples.of(config, file.pixels).bind(
            lambda samples: White.of(config, msgspec.convert(samples.color[samples.covered].mean(axis=0, dtype=np.float64).tolist(), Rgb)) if samples.covered.any() else Ok(Nothing)
        ),
        lambda white: white.map(lambda source: Some(msgspec.structs.replace(record, source=source))).to_result_with(lambda: (BlenderUnmetFault(requirement=Unread(entry=record.title)),)),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

white_balance = CompositorEffect(record=WhiteBalance, graph=balanced, alpha=Alpha.RESULT, measured=Some(Measured(revise=measured)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["balanced", "measured", "white_balance"]
