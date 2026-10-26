"""Staged particle light with stored spectra."""

from collections.abc import Mapping

import bpy
from expression import Ok, Option, Some
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import stored
from blender.bpy.nodes.trees import created
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, precise, Shell, shuttered
from blender.imaging.effects.generators.noise import Gradient, spectrum
from blender.imaging.effects.generators.particles import Particles
from blender.imaging.faults import accumulated, ImagingResult, scoped
from blender.imaging.parameters import Ramp
from blender.imaging.sampling import Estimate, Precision

# --- [OPERATIONS] -----------------------------------------------------------------------


def prepared(
    identity: Identity,
    scene: bpy.types.Scene,
    record: Particles,
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    precision: Precision,
    frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[Particles, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Integrates particle light over captured window and scene shutter, spectrum ramps evaluated by host color bands of a scratch tree.

    Args:
        identity: Extension owning staged render.
        scene: Evaluated scene supplying reference frame and shutter.
        record: Evaluated particle controls with graph edits.
        config: Validated working color configuration.
        image: Associated entering RGBA with data and display windows.
        precision: Integration nets and numerical tolerances caller chose.
        frame: Camera geometry at consuming frame.
        _entry: Evaluated stage entry.

    Returns:
        Unchanged record and main image estimate, or graph and imaging faults.
    """
    spectra = ((record.coverage.enabled, record.coverage.value), (record.emission.enabled, record.emission.value.spectrum))
    ramps = tuple(dict.fromkeys(value.ramp for enabled, value in spectra if enabled and isinstance(value, Gradient)))
    timing = shuttered(scene)

    def integrated(bands: Mapping[Ramp, bpy.types.ColorRamp]) -> BlenderResult[tuple[Particles, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
        def gradient(ramp: Ramp, tones: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
            return Ok(np.asarray([bands[ramp].evaluate(float(tone)) for tone in tones], dtype=np.float64)[:, :3])

        return record.applied(
            image,
            frame.window(image.spec())[0],
            reference_frame=scene.frame_current_final,
            shutter=(timing.opening * timing.length, (timing.opening + 1.0) * timing.length),
            precision=precision,
            spectrum=lambda request, tones: spectrum(request, tones, config=config, gradient=gradient),
        ).map(lambda estimate: (record, {Shell.image: estimate}))

    def banded(tree: bpy.types.NodeTree, ramp: Ramp) -> BlenderResult[tuple[Ramp, bpy.types.ColorRamp]]:
        return created(tree, bpy.types.ShaderNodeValToRGB).bind(
            lambda live: (
                stored(ramp, live, identity).bind(lambda _: Option.of_optional(live.color_ramp).to_result((BlenderAbsentFault(owner=repr(live), key="color_ramp"),))).map(lambda band: (ramp, band))
            )
        )

    def scratch() -> BlenderResult[bpy.types.NodeTree]:
        return Option.of_optional(bpy.data.node_groups.new(record.title, "ShaderNodeTree")).to_result((BlenderAbsentFault(owner=repr(bpy.data.node_groups), key=record.title),))

    return (
        scoped(
            scratch(),
            lambda tree: accumulated(banded(tree, ramp) for ramp in ramps).map(dict).bind(integrated),
            lambda tree: BlenderEditFault.attempted(repr(bpy.data.node_groups), lambda: bpy.data.node_groups.remove(tree)),
        )
        if ramps
        else integrated({})
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

particles = CompositorEffect(record=Particles, graph=Shell.passed, alpha=Alpha.RESULT, requested=Some(precise), preparation=Some(prepared))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["particles", "prepared"]
