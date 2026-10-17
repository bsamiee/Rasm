"""Affine black and white point adjustment in the compositor."""

import bpy
from expression import Nothing, Option, option, Some
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Done, Operator
from blender.bpy.data.rna.conditions import Job
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.nodes.graphs import End, ended, Fragment, math, mixed, node
from blender.bpy.render.effects.stack import CompositorEffect, Named, Shell
from blender.bpy.render.effects.viewers import ViewerRead
from blender.imaging.analysis.metering import Samples, samples_of
from blender.imaging.color.config import admitted
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.tone.levels import Levels
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.images import ImageFile

# --- [OPERATIONS] -----------------------------------------------------------------------


def leveled(shell: Shell[Levels]) -> BlenderResult[tuple[Fragment, End]]:
    """Subtracts the covered black point and applies the white span in stops."""
    source = shell.source
    black, white = (shell.socket(field) for field in Levels.__struct_fields__)
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


def measured(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
    """Writes covered frame extrema as levels, retaining the entry's blend, and names an undo step where the levels changed."""
    found: BlenderResult[Option[Levels]] = (
        admitted(PyOpenColorIO.GetCurrentConfig(), ())
        .bind(lambda config: samples_of(config, file.pixels))
        .map(Samples.extent)
        .map(option.bind(lambda extent: Levels.of(extent.low, extent.white)))
        .map_error(BlenderImagingFault.lifted)
    )
    return (
        paired(found, levels.read(identity, entry))
        .bind(lambda values: accumulated(levels.write(identity, scene, entry, Entry(effect=record, blend=held.blend)) for records, held in (values,) for record in records.to_list()))
        .map(lambda writes: Some(auto_levels.title) if any(writes) else Nothing)
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
