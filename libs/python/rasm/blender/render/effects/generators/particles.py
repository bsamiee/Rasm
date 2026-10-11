"""Staged particle light with native stored spectra."""

from collections.abc import Mapping
from contextlib import ExitStack

import bpy
from expression import Error, Ok, Some
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderEditFault, BlenderImagingFault, BlenderResult
from rasm.blender.nodes import graphs
from rasm.blender.nodes.graphs import Fragment
from rasm.blender.render.effects.stack import CameraFrame, CompositorEffect, Coverage, precise, Shell, shutter
from rasm.imaging.effects.generators.noise import Estimate, Gradient, Precision, spectrum
from rasm.imaging.effects.generators.particles import Particles
from rasm.imaging.faults import accumulated, ImagingResult, paired
from rasm.imaging.parameters import Ramp

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
        try:
            native: dict[Ramp, bpy.types.ShaderNodeValToRGB] = {}
            if ramps:
                tree: bpy.types.NodeTree = bpy.data.node_groups.new(record.title, bpy.types.ShaderNodeTree.bl_rna.identifier)  # type: ignore[assignment,attr-defined]  # ty: ignore[invalid-assignment,unresolved-attribute]
                scope.callback(bpy.data.node_groups.remove, tree)
                native = {ramp: tree.nodes.new(bpy.types.ShaderNodeValToRGB.bl_rna.identifier) for ramp in ramps}  # type: ignore[misc,attr-defined]  # ty: ignore[invalid-assignment,unresolved-attribute]
        except (MemoryError, RuntimeError) as error:
            return Error((BlenderEditFault(request=record.title, error=error),))

        def gradient(ramp: Ramp, tones: npt.NDArray[np.float64]) -> ImagingResult[npt.NDArray[np.float64]]:
            return Ok(np.asarray([native[ramp].color_ramp.evaluate(float(tone)) for tone in tones], dtype=np.float64)[:, :3])  # type: ignore[misc,union-attr]  # ty: ignore[unresolved-attribute]

        return paired(accumulated(graphs.stored(ramp, live, identity) for ramp, live in native.items()), shutter.of.some(identity, scene, record)).bind(
            lambda parts: (
                record
                .applied(
                    image,
                    frame.window(image.spec())[0],
                    reference_frame=scene.frame_current_final,
                    shutter=(parts[1].opening * parts[1].length, (parts[1].opening + 1.0) * parts[1].length),
                    precision=precision,
                    spectrum=lambda request, tones: spectrum(request, tones, config=config, gradient=gradient),
                )
                .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                .map(lambda estimate: (record, {Shell.image: estimate}))
            )
        )


# --- [COMPOSITION] ----------------------------------------------------------------------

particles = CompositorEffect(record=Particles, graph=lambda shell: Ok((Fragment(), shell.source)), coverage=Coverage.RESULT, requested=Some(precise), preparation=Some(prepared))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["particles", "prepared"]
