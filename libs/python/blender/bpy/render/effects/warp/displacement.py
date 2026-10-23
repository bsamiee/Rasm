"""Staged displacement with filtered nonlinear drop refraction."""

from collections.abc import Mapping

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.nodes.graphs import End
from blender.bpy.render.effects.stack import CameraFrame, CompositorEffect, Coverage, entry_input, precise, Shell
from blender.bpy.render.effects.tone.exposure import exposure
from blender.bpy.render.effects.viewers import inspection
from blender.imaging.color.processors import applied, luma, processor
from blender.imaging.effects.generators.noise import Estimate, Precision
from blender.imaging.effects.warp.displacement import Displacement, MapReading
from blender.imaging.faults import ImagingResult, paired
from blender.imaging.files.images import ImageFile, read_file
from blender.imaging.parameters import Gated

# --- [OPERATIONS] -----------------------------------------------------------------------


def prepared(
    identity: Identity,
    scene: bpy.types.Scene,
    record: Displacement,
    config: PyOpenColorIO.Config,
    source: OpenImageIO.ImageBuf,
    precision: Precision,
    frame: CameraFrame,
    entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[Displacement, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Samples captured color under original camera coordinates and keyed exposure.

    Args:
        identity: Extension owning capture and effect inputs.
        scene: Evaluated scene supplying held motion clocks.
        record: Evaluated gated displacement controls.
        config: Color configuration for luminance and map files.
        source: Associated stage input with DATA and FULL windows.
        precision: Caller-selected pixel integration and replicate error policy.
        frame: Original camera pixel aspect and share of overscan.
        entry: Evaluated entry supplying keyed exposure gain of later entries.

    Returns:
        Unchanged controls and jointly estimated image, displacement, and drop coverage, or image, color, and input faults.
    """
    if not any(value.enabled for value in msgspec.structs.astuple(record) if isinstance(value, Gated)):
        return Ok((record, {}))
    spec = source.spec()
    bounds, extent = frame.window(spec)
    request = record.map.value

    def converted(file: ImageFile[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
        return (
            Ok(file.pixels)
            if request.reading in {MapReading.OFFSETS, MapReading.COORDINATES}
            else processor(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR)).map(
                lambda transform: applied(transform.getDefaultCPUProcessor(), file.pixels)
            )
        )

    mapping: ImagingResult[Option[npt.NDArray[np.float32]]] = (
        Option
        .of_optional(request.source)
        .map(lambda path: read_file(path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA) if request.reading in {MapReading.OFFSETS, MapReading.COORDINATES} else Nothing).bind(converted).map(Some))
        .default_value(Ok(Nothing))
        if record.map.enabled
        else Ok(Nothing)
    )

    def published(
        estimate: Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]],
    ) -> tuple[Displacement, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]:
        channels = spec.nchannels
        return record, {
            key: Estimate(value=plane, standard_error=estimate.standard_error[..., start:stop], precision=precision, replicates=estimate.replicates[..., start:stop])
            for key, plane, start, stop in (
                (Shell.image, estimate.value[0], 0, channels),
                ("displacement", estimate.value[1], channels, channels + 2),
                ("drops", estimate.value[2], channels + 2, channels + 3),
            )
        }

    return paired(
        paired(luma(config), mapping).map_error(BlenderImagingFault.lifted),
        entry_input(identity, entry, exposure.gain.key).map(lambda value: float(value.value)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    ).bind(
        lambda values: (
            record
            .applied(source, bounds, extent, reference_frame=scene.frame_current_final, luminance=values[0][0].astype(np.float64), gain=values[1], mapping=values[0][1], precision=precision)
            .map_error(BlenderImagingFault.lifted)
            .map(published)
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


displacement: CompositorEffect[Displacement] = CompositorEffect(
    record=Displacement,
    graph=Shell.passed,
    coverage=Coverage.RESULT,
    requested=Some(precise),
    preparation=Some(prepared),
    inputs=(exposure.gain,),
    animated=Some(exposure.keyed),
    registrables=(
        inspection(lambda: displacement, "displacement_field", "View Displacement", "Integrated displacement in displayed frame units", lambda: End(node="displacement", socket="Image")),
        inspection(lambda: displacement, "raindrop_coverage", "View Raindrops", "Integrated refracting drop and trail coverage", lambda: End(node="drops", socket="Image")),
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["displacement", "prepared"]
