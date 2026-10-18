"""BT.2390 peak compression and black adaptation over formed PQ codes."""

import bpy
from expression import Nothing, Option, Some
import msgspec
import numpy as np

from blender.bpy.commands.operators import Done, Operator
from blender.bpy.data.rna.conditions import AllOf, HdrWindow, Job
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.nodes.graphs import End, ended, Fragment, math, node, vector
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.stack import CompositorEffect, Named, Shell, viewed
from blender.bpy.render.effects.viewers import ViewerRead
from blender.imaging.analysis import metering
from blender.imaging.color.processors import DisplayView
from blender.imaging.effects.tone.peak import PeakMapping
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.images import ImageFile

# --- [OPERATIONS] -----------------------------------------------------------------------


def peaked(shell: Shell[PeakMapping]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds the forward-only EETF, retaining black adaptation without compression."""
    source = shell.source
    source_peak, target_peak, black, knee_offset = (shell.socket(field) for field in PeakMapping.__struct_fields__)
    m1, m2, c1, c2, c3 = 2610.0 / 16384.0, 2523.0 / 32.0, 3424.0 / 4096.0, 2413.0 / 128.0, 2392.0 / 128.0
    zero, one = (0.0,) * 3, (1.0,) * 3
    return Fragment.joined(
        math("limit", "MINIMUM", target_peak, source_peak),
        math("black_level", "MULTIPLY", black, End(node="limit")),
        node("levels", bpy.types.ShaderNodeCombineXYZ, {"X": source_peak, "Y": End(node="limit"), "Z": End(node="black_level")}, {}),
        vector("luminance", "SCALE", End(node="levels", socket="Vector"), 1.0 / 10000.0),
        vector("powered", "POWER", End(node="luminance", socket="Vector"), (m1,) * 3),
        vector("numerator", "MULTIPLY_ADD", End(node="powered", socket="Vector"), (c2,) * 3, (c1,) * 3),
        vector("denominator", "MULTIPLY_ADD", End(node="powered", socket="Vector"), (c3,) * 3, one),
        vector("ratio", "DIVIDE", End(node="numerator", socket="Vector"), End(node="denominator", socket="Vector")),
        vector("encoded", "POWER", End(node="ratio", socket="Vector"), (m2,) * 3),
        node("codes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="encoded", socket="Vector")}, {}),
        math("peak", "DIVIDE", End(node="codes", socket="Y"), End(node="codes", socket="X")),
        math("floor", "DIVIDE", End(node="codes", socket="Z"), End(node="codes", socket="X")),
        math("drop", "SUBTRACT", End(node="peak"), 1.0),
        math("knee", "MULTIPLY_ADD", knee_offset, End(node="drop"), End(node="peak")),
        math("span", "SUBTRACT", 1.0, End(node="knee")),
        math("square_base", "MULTIPLY_ADD", End(node="peak"), 3.0, -2.0),
        math("square", "SUBTRACT", End(node="square_base"), End(node="knee")),
        math("cube_base", "MULTIPLY_ADD", End(node="peak"), -2.0, 1.0),
        math("cube", "ADD", End(node="cube_base"), End(node="knee")),
        math("inverse_floor", "DIVIDE", 1.0, End(node="floor")),
        math("exponent", "MINIMUM", End(node="inverse_floor"), 4.0),
        math("headroom", "SUBTRACT", 1.0, End(node="peak")),
        math("headroom_power", "POWER", End(node="headroom"), End(node="exponent")),
        math("floor_share", "DIVIDE", End(node="floor"), End(node="peak")),
        math("gain_base", "MULTIPLY_ADD", End(node="headroom_power"), End(node="floor_share"), 1.0),
        math("gain", "DIVIDE", 1.0, End(node="gain_base")),
        math("lift_gain", "MULTIPLY", End(node="gain"), End(node="floor")),
        math("lift_offset", "SUBTRACT", End(node="floor"), End(node="lift_gain")),
        vector("code", "DIVIDE", source, End(node="codes", socket="X")),
        vector("positive", "MAXIMUM", End(node="code", socket="Vector"), zero),
        vector("clipped", "MINIMUM", End(node="positive", socket="Vector"), one),
        vector("excess", "SUBTRACT", End(node="clipped", socket="Vector"), End(node="knee")),
        vector("over", "MAXIMUM", End(node="excess", socket="Vector"), zero),
        vector("position", "DIVIDE", End(node="over", socket="Vector"), End(node="span")),
        vector("cubic", "MULTIPLY_ADD", End(node="position", socket="Vector"), End(node="cube"), End(node="square")),
        vector("squared", "MULTIPLY", End(node="position", socket="Vector"), End(node="position", socket="Vector")),
        vector("rolled", "MULTIPLY_ADD", End(node="squared", socket="Vector"), End(node="cubic", socket="Vector"), End(node="clipped", socket="Vector")),
        vector("remainder", "SUBTRACT", one, End(node="rolled", socket="Vector")),
        vector("raised", "POWER", End(node="remainder", socket="Vector"), End(node="exponent")),
        vector("lift_sum", "MULTIPLY_ADD", End(node="raised", socket="Vector"), End(node="lift_gain"), End(node="lift_offset")),
        vector("lifted", "MULTIPLY_ADD", End(node="rolled", socket="Vector"), End(node="gain"), End(node="lift_sum", socket="Vector")),
        vector("floored", "MAXIMUM", End(node="lifted", socket="Vector"), End(node="floor")),
        vector("capped", "MINIMUM", End(node="floored", socket="Vector"), End(node="peak")),
        vector("scaled", "SCALE", End(node="capped", socket="Vector"), End(node="codes", socket="X")),
    ).map(ended("scaled", "Vector"))


def formed_view(context: bpy.types.Context) -> Option[DisplayView]:
    """Returns the enabled compositor formation or the scene's display view."""
    return Option.of_optional(context.scene).map(
        lambda scene: (
            Option
            .of_optional(
                next(
                    (
                        node
                        for entry in scene.compositor_effects
                        if entry.enable_for_preview
                        for group in Option.of_optional(entry.node_group).to_list()
                        for node in group.nodes
                        if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)
                    ),
                    None,
                )
            )
            .map(viewed)
            .default_with(lambda: viewed(scene))
        )
    )


def measured(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
    """Writes the observed PQ peak while preserving mapping parameters and blend, naming an undo step where the peak changed."""
    found = config_of().bind(lambda config: metering.samples_of(config, file.pixels).map(metering.peak).map_error(BlenderImagingFault.lifted))
    return (
        paired(found, peak_mapping.read(identity, entry))
        .bind(
            lambda values: accumulated(
                peak_mapping.write(identity, scene, entry, msgspec.structs.replace(held, effect=msgspec.structs.replace(held.effect, source_peak=value)))
                for peaks, held in (values,)
                for value in peaks.to_list()
            )
        )
        .map(lambda writes: Some(measure_peak.title) if any(writes) else Nothing)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

reading = ViewerRead(consume=measured)
measure_peak = Operator(
    key="measure_peak",
    title="Measure Source Peak",
    description="Measures the frame's peak luminance into the source peak",
    form=Named,
    undo=False,
    condition=Some(AllOf(conditions=(Job(kind="RENDER"), HdrWindow(formed=formed_view)))),
    run=lambda call: (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: peak_mapping.named(call.identity, scene, call.record.name))
        .bind(lambda entry: reading.entering(call.identity, entry))
        .map(lambda _: Done(notes=(("INFO", "Measuring the frame entering peak mapping"),)))
    ),
)
peak_mapping: CompositorEffect[PeakMapping] = CompositorEffect(record=PeakMapping, graph=peaked, registrables=(measure_peak, reading))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["formed_view", "measure_peak", "measured", "peak_mapping", "peaked", "reading"]
