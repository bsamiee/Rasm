"""Native white-point balance over scene-linear color."""

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderFaults, BlenderImagingFault, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.graphs import End, ended, Fragment, node
from blender.bpy.render.effects.stack import CompositorEffect, Coverage, Shell
from blender.bpy.render.effects.viewers import Unread, ViewerRead
from blender.imaging.analysis.metering import samples_of
from blender.imaging.color.config import admitted
from blender.imaging.color.white import White
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.grade.white_balance import WhiteBalance
from blender.imaging.faults import paired
from blender.imaging.files.images import ImageFile
from blender.imaging.parameters import Rgb

# --- [OPERATIONS] -----------------------------------------------------------------------


def balanced(shell: Shell[WhiteBalance]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds native white-point balance from record-derived white sockets."""
    source, target = WhiteBalance.__struct_fields__
    whites = {f"{side} {field.replace('_', ' ').title()}": shell.socket(key, field) for side, key in (("Input", source), ("Output", target)) for field in White.__struct_fields__}
    return node("balance", bpy.types.CompositorNodeColorBalance, {"Image": shell.source, "Fac": 1.0, "Type": "White Point", **whites}, {}).map(ended("balance", "Image"))


def consumed(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
    """Sets the source white from the covered mean of the entering region, naming an undo step where the white changed."""
    config: BlenderResult[PyOpenColorIO.Config] = admitted(PyOpenColorIO.GetCurrentConfig(), ()).map_error(BlenderImagingFault.lifted)

    def solved(config: PyOpenColorIO.Config, held: Entry[WhiteBalance]) -> BlenderResult[Option[str]]:
        found: BlenderResult[Option[White]] = (
            samples_of(config, file.pixels)
            .map(lambda samples: samples.color[samples.covered])
            .bind(lambda covered: White.of(config, Rgb(**dict(zip(Rgb.__struct_fields__, map(float, covered.mean(axis=0, dtype=np.float64)))))) if covered.size else Ok(Nothing))
            .map_error(BlenderImagingFault.lifted)
        )
        selected: BlenderResult[White] = Result[Option[White], BlenderFaults].bind(found, lambda white: white.to_result_with(lambda: (BlenderUnmetFault(requirement=Unread(entry=entry.name)),)))
        return (
            Result[White, BlenderFaults]
            .bind(selected, lambda white: white_balance.write(identity, scene, entry, msgspec.structs.replace(held, effect=msgspec.structs.replace(held.effect, source=white))))
            .map(lambda changed: Some("Set Neutral") if changed else Nothing)
        )

    reading: BlenderResult[tuple[PyOpenColorIO.Config, Entry[WhiteBalance]]] = paired(config, white_balance.read(identity, entry))
    return Result[tuple[PyOpenColorIO.Config, Entry[WhiteBalance]], BlenderFaults].bind(reading, lambda pair: solved(*pair))


# --- [COMPOSITION] ----------------------------------------------------------------------

white_balance = CompositorEffect(record=WhiteBalance, graph=balanced, coverage=Coverage.RESULT, registrables=(ViewerRead(consume=consumed),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["balanced", "consumed", "white_balance"]
