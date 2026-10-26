"""Staged displacement with filtered nonlinear drop refraction."""

from collections.abc import Mapping
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End
from blender.bpy.render.effects.records import entry_input
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, Inspected, precise, Shell
from blender.bpy.render.effects.tone.exposure import exposure
from blender.imaging.color.processors import converted, luma
from blender.imaging.effects.warp.displacement import Displacement, MapReading
from blender.imaging.faults import ImagingResult, paired
from blender.imaging.files.images import ImageFile, read_file
from blender.imaging.parameters import Gated
from blender.imaging.sampling import Estimate, Precision

# --- [CONSTANTS] ------------------------------------------------------------------------

DISPLACEMENT: Final[Key] = "displacement"
DROPS: Final[Key] = "drops"

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
    data = request.reading in {MapReading.OFFSETS, MapReading.COORDINATES}

    def linear(file: ImageFile[np.float32]) -> ImagingResult[npt.NDArray[np.float32]]:
        return Ok(file.pixels) if data else converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), file.pixels)

    mapping: ImagingResult[Option[npt.NDArray[np.float32]]] = (
        Option
        .of_optional(request.source)
        .filter(lambda _: record.map.enabled)
        .map(lambda path: read_file(path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA) if data else Nothing).bind(linear).map(Some))
        .default_value(Ok(Nothing))
    )

    def published(
        estimate: Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]],
    ) -> tuple[Displacement, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]:
        channels = spec.nchannels
        return record, {
            key: Estimate(value=plane, standard_error=estimate.standard_error[..., start:stop], precision=precision, replicates=estimate.replicates[..., start:stop])
            for key, plane, start, stop in (
                (Shell.image, estimate.value[0], 0, channels),
                (DISPLACEMENT, estimate.value[1], channels, channels + 2),
                (DROPS, estimate.value[2], channels + 2, channels + 3),
            )
        }

    return paired(
        luma(config),
        mapping,
        entry_input(identity, entry, exposure.gain.key).map(lambda value: float(value.value)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    ).bind(lambda values: record.applied(source, bounds, extent, reference_frame=scene.frame_current_final, luminance=values[0], gain=values[2], mapping=values[1], precision=precision).map(published))


# --- [COMPOSITION] ----------------------------------------------------------------------


displacement = CompositorEffect(
    record=Displacement,
    graph=Shell.passed,
    alpha=Alpha.RESULT,
    requested=Some(precise),
    preparation=Some(prepared),
    inputs=(exposure.gain,),
    animated=Some(exposure.keyed),
    inspected=(
        Inspected(key="displacement_field", end=End(node=DISPLACEMENT, socket="Image"), description="Integrated displacement in displayed frame units"),
        Inspected(key="raindrop_coverage", end=End(node=DROPS, socket="Image"), description="Integrated refracting drop and trail coverage"),
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["displacement", "prepared"]
