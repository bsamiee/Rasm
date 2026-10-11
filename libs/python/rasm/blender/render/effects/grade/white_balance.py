"""Native white-point balance over scene-linear color."""

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import PyOpenColorIO

from rasm.blender.extension.identity import Identity
from rasm.blender.faults import BlenderFaults, BlenderImagingFault, BlenderResult, BlenderUnmetFault
from rasm.blender.nodes.graphs import End, ended, Fragment, node
from rasm.blender.render.effects.stack import CompositorEffect, Coverage, Shell
from rasm.blender.render.effects.viewers import Unread, ViewerRead
from rasm.imaging.analysis.metering import Samples
from rasm.imaging.color.config import admitted
from rasm.imaging.color.white import White
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.effects.grade.white_balance import WhiteBalance
from rasm.imaging.faults import paired
from rasm.imaging.files.images import ImageFile
from rasm.imaging.parameters import Rgb

# --- [OPERATIONS] -----------------------------------------------------------------------


def balanced(shell: Shell[WhiteBalance]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds native white-point balance from record-derived white sockets."""
    whites = {
        f"{side} {field.name.replace('_', ' ').title()}": shell.socket(key, field.name)
        for side, key in (("Input", WhiteBalance.source.__name__), ("Output", WhiteBalance.target.__name__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        for field in msgspec.structs.fields(White)
    }
    return node("balance", bpy.types.CompositorNodeColorBalance, {"Image": shell.source, "Fac": 1.0, "Type": "White Point", **whites}, {}).map(ended("balance", "Image"))


def consumed(identity: Identity, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
    """Sets the source white from the covered mean of the entering region."""
    config: BlenderResult[PyOpenColorIO.Config] = admitted(PyOpenColorIO.GetCurrentConfig(), ()).map_error(lambda faults: (BlenderImagingFault(faults=faults),))

    def solved(config: PyOpenColorIO.Config, held: Entry[WhiteBalance]) -> BlenderResult[Option[str]]:
        found: BlenderResult[Option[White]] = (
            Samples
            .of(config, file.pixels)
            .map(lambda samples: samples.color[samples.covered])
            .bind(
                lambda covered: (
                    White.of(config, Rgb(**dict(zip((field.name for field in msgspec.structs.fields(Rgb)), map(float, covered.mean(axis=0, dtype=np.float64)))))) if covered.size else Ok(Nothing)
                )
            )
            .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
        )
        selected: BlenderResult[White] = Result[Option[White], BlenderFaults].bind(found, lambda white: white.to_result_with(lambda: (BlenderUnmetFault(requirement=Unread(entry=entry.name)),)))
        return (
            Result[White, BlenderFaults]
            .bind(selected, lambda white: white_balance.write(identity, entry.id_data, entry, msgspec.structs.replace(held, effect=msgspec.structs.replace(held.effect, source=white))))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .map(lambda _: Some("Set Neutral"))
        )

    reading: BlenderResult[tuple[PyOpenColorIO.Config, Entry[WhiteBalance]]] = paired(config, white_balance.read(identity, entry))
    return Result[tuple[PyOpenColorIO.Config, Entry[WhiteBalance]], BlenderFaults].bind(reading, lambda pair: solved(*pair))


# --- [COMPOSITION] ----------------------------------------------------------------------

white_balance = CompositorEffect(record=WhiteBalance, graph=balanced, coverage=Coverage.RESULT, registrables=(ViewerRead(consume=consumed),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["balanced", "consumed", "white_balance"]
