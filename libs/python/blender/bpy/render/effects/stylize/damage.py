"""Staged footprint-filtered film damage and lamp flicker."""

from collections.abc import Mapping

import bpy
from expression import Ok, Some
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math
from blender.bpy.render.effects.coordinates import clocked, origin
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, Inspected, precise, Shell
from blender.imaging.effects.stylize.damage import Damage, Flicker
from blender.imaging.parameters import Motion
from blender.imaging.sampling import Estimate, Precision

# --- [OPERATIONS] -----------------------------------------------------------------------


def prepared(
    _identity: Identity,
    scene: bpy.types.Scene,
    record: Damage,
    _config: PyOpenColorIO.Config,
    source: OpenImageIO.ImageBuf,
    precision: Precision,
    frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[Damage, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Filters captured film in original camera units and publishes damage key.

    Args:
        _identity: Stage job owner.
        scene: Evaluated scene supplying film clock.
        record: Evaluated film marks, weave, and stationary gate.
        _config: Stage color configuration, film reads associated scene-linear RGBA.
        source: Top-first DATA image with retained FULL window.
        precision: Caller-selected integration resolution and error replicates.
        frame: Original camera pixel aspect and retained overscan scale.
        _entry: Evaluated stage entry.

    Returns:
        Unchanged record with jointly estimated image and damage key, or imaging faults.
    """
    bounds, extent = frame.window(source.spec())

    def published(estimate: Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]) -> tuple[Damage, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]:
        return record, {
            key: Estimate(value=plane, standard_error=estimate.standard_error[..., part], precision=estimate.precision, replicates=estimate.replicates[..., part], normalization=estimate.normalization)
            for key, plane, part in zip((Shell.image, "damage"), estimate.value, (slice(4), slice(4, None)))
        }

    return record.applied(source, bounds, extent, record.motion.clock(scene.frame_current + scene.frame_subframe), precision=precision).map(published)


def flickered(shell: Shell[Flicker]) -> BlenderResult[Expr]:
    """Builds uniform lamp variation in scene-linear stops.

    Args:
        shell: Entry color and Flicker inputs.

    Returns:
        Exposed scene-linear color.
    """
    motion, seed, amplitude, rate = Flicker.__struct_fields__
    speed, hold = Motion.__struct_fields__
    wave = Expr(
        kind=bpy.types.ShaderNodeTexNoise,
        inputs=(("W", clocked(shell.input(motion, speed), shell.input(motion, hold)) * shell.input(rate) + origin(shell.input(seed), 8)["X"]), ("Scale", 1.0)),
        members=(("noise_dimensions", "1D"),),
        socket="Fac",
    )
    stops = math("MULTIPLY_ADD", wave, 2.0, -1.0) * shell.input(amplitude)
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", stops)), socket="Image", shape="RGBA"))


# --- [COMPOSITION] ----------------------------------------------------------------------


damage = CompositorEffect(
    record=Damage,
    graph=Shell.passed,
    alpha=Alpha.RESULT,
    requested=Some(precise),
    preparation=Some(prepared),
    inspected=(Inspected(key="damage_key", end=End(node="damage", socket="Image"), description="Integrated film marks and stationary gate"),),
)
flicker = CompositorEffect(record=Flicker, graph=flickered, requested=Some(precise))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["damage", "flicker", "flickered", "prepared"]
