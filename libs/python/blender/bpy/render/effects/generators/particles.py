"""Staged particle light with native stored spectra."""

from collections.abc import Mapping
from contextlib import ExitStack

import bpy
from expression import Ok, Some
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.nodes.changes import stored
from blender.bpy.nodes.graphs import Fragment
from blender.bpy.nodes.trees import created
from blender.bpy.render.effects.stack import CameraFrame, CompositorEffect, Coverage, precise, Shell, Shutter, shutter
from blender.imaging.effects.generators.noise import Estimate, Gradient, Precision, spectrum
from blender.imaging.effects.generators.particles import Particles
from blender.imaging.faults import accumulated, ImagingResult, paired
from blender.imaging.parameters import Ramp

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
    """Integrates particle light over the captured window and actual scene shutter.

    Args:
        identity: Extension owning the staged render.
        scene: Evaluated scene supplying the reference frame and shutter.
        record: Evaluated particle controls, including native graph edits.
        config: Admitted working color configuration.
        image: Associated entering RGBA with its native data and display windows.
        precision: Caller-selected integration nets and numerical tolerances.
        frame: Native camera geometry at the consuming frame.
        _entry: Evaluated native stage entry.

    Returns:
        The unchanged record and main image estimate, or native and imaging faults.
    """
    ramps = tuple(
        dict.fromkeys(
            value.ramp for enabled, value in ((record.coverage.enabled, record.coverage.value), (record.emission.enabled, record.emission.value.spectrum)) if enabled and isinstance(value, Gradient)
        )
    )
    with ExitStack() as scope:

        def ramped(tree: bpy.types.NodeTree) -> BlenderResult[dict[Ramp, bpy.types.Node]]:
            scope.callback(bpy.data.node_groups.remove, tree)
            return accumulated(created(tree, bpy.types.ShaderNodeValToRGB).bind(lambda live: stored(ramp, live, identity).map(lambda _: (ramp, live))) for ramp in ramps).map(dict)

        def integrated(native: dict[Ramp, bpy.types.Node], timing: Shutter) -> BlenderResult[tuple[Particles, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
            def gradient(ramp: Ramp, tones: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
                return Ok(np.asarray([native[ramp].color_ramp.evaluate(float(tone)) for tone in tones], dtype=np.float64)[:, :3])  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

            return (
                record
                .applied(
                    image,
                    frame.window(image.spec())[0],
                    reference_frame=scene.frame_current_final,
                    shutter=(timing.opening * timing.length, (timing.opening + 1.0) * timing.length),
                    precision=precision,
                    spectrum=lambda request, tones: spectrum(request, tones, config=config, gradient=gradient),
                )
                .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                .map(lambda estimate: (record, {Shell.image: estimate}))
            )

        native = ramped(bpy.data.node_groups.new(record.title, "ShaderNodeTree")) if ramps else Ok({})  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        return paired(native, shutter.of.some(identity, scene, record)).bind(lambda parts: integrated(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

particles = CompositorEffect(record=Particles, graph=lambda shell: Ok((Fragment(), shell.source)), coverage=Coverage.RESULT, requested=Some(precise), preparation=Some(prepared))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["particles", "prepared"]
