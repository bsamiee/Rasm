"""Affine black and white point adjustment in the compositor."""

import bpy
from expression import Nothing, Option, option, Some
import numpy as np
import PyOpenColorIO

from rasm.blender.commands.operators import Done, Operator
from rasm.blender.data.rna.conditions import Job
from rasm.blender.extension.identity import Identity
from rasm.blender.faults import BlenderImagingFault, BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, math, mixed, node
from rasm.blender.render.effects.stack import CompositorEffect, Named, Shell
from rasm.blender.render.effects.viewers import ViewerRead
from rasm.imaging.analysis.metering import Samples
from rasm.imaging.color.config import admitted
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.effects.tone.levels import Levels
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.files.images import ImageFile

# --- [OPERATIONS] -----------------------------------------------------------------------


def leveled(shell: Shell[Levels]) -> BlenderResult[tuple[Fragment, End]]:
    """Subtracts the covered black point and applies the white span in stops."""
    source, black, white = shell.source, shell.socket(Levels.black.__name__), shell.socket(Levels.white.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return (
        paired(
            node("coverage", bpy.types.CompositorNodeSeparateColor, {"Image": source}, {})
            .bind(lambda coverage: math("covered", "MULTIPLY", black, ended("coverage", "Alpha")(coverage)))
            .bind(lambda covered: mixed("offset", "RGBA", 1.0, source, covered, "SUBTRACT")),
            math("stops", "MULTIPLY", white, -1.0),
        )
        .bind(lambda ends: node("scale", bpy.types.CompositorNodeExposure, dict(zip(("Image", "Exposure"), ends)), {}))
        .map(ended("scale", "Image"))
    )


def measured(identity: Identity, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
    """Writes covered frame extrema as levels, retaining the entry's blend."""
    found: BlenderResult[Option[Levels]] = (
        admitted(PyOpenColorIO.GetCurrentConfig(), (Levels.space,))
        .bind(lambda config: Samples.of(config, file.pixels))
        .map(Samples.extent)
        .map(option.bind(lambda extent: Levels.of(extent.low, extent.white)))
        .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
    )
    return (
        paired(found, levels.read(identity, entry))
        .bind(lambda values: accumulated(levels.write(identity, entry.id_data, entry, Entry(effect=record, blend=held.blend)) for records, held in (values,) for record in records.to_list()))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        .map(lambda writes: Some(auto_levels.title) if writes else Nothing)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

reading = ViewerRead(consume=measured)
auto_levels = Operator(
    key="auto_levels",
    title="Auto Levels",
    description=f"Measures {Levels.title} from the entering frame",
    form=Named,
    undo=False,
    condition=Some(Job(kind="RENDER")),
    run=lambda call: (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: levels.named(call.identity, scene, call.record.name))
        .bind(lambda entry: reading.entering(call.identity, entry))
        .map(lambda _: Done(notes=(("INFO", "Measuring the frame entering levels"),)))
    ),
)
levels: CompositorEffect[Levels] = CompositorEffect(record=Levels, graph=leveled, registrables=(auto_levels, reading))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["auto_levels", "leveled", "levels", "measured", "reading"]
