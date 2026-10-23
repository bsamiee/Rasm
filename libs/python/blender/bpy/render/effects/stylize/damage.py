"""Staged footprint-filtered film damage and lamp flicker."""

from collections.abc import Mapping
from fractions import Fraction
from math import floor

import bpy
from expression import Ok, Some
import OpenImageIO
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.nodes.graphs import Clock, End, ended, Fragment, math, node, origin
from blender.bpy.render.effects.stack import CameraFrame, CompositorEffect, Coverage, precise, Shell
from blender.bpy.render.effects.viewers import inspection
from blender.bpy.render.view_layers.passes import PassRequest
from blender.imaging.effects.generators.noise import Estimate, Precision
from blender.imaging.effects.stylize.damage import Damage, Flicker
from blender.imaging.faults import paired
from blender.imaging.parameters import Motion

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
        Unchanged record with jointly estimated image and damage key, or lifted imaging faults.
    """
    bounds, extent = frame.window(source.spec())
    time = Fraction(scene.frame_current) + Fraction(scene.frame_subframe)
    clock = float(Fraction(record.motion.speed) * Fraction(record.motion.hold) * floor(time / Fraction(record.motion.hold)))

    def published(estimate: Estimate[tuple[OpenImageIO.ImageBuf, OpenImageIO.ImageBuf]]) -> tuple[Damage, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]:
        image, key = estimate.value
        return record, {
            Shell.image: Estimate(
                value=image, standard_error=estimate.standard_error[..., :4], precision=estimate.precision, replicates=estimate.replicates[..., :4], normalization=estimate.normalization
            ),
            "damage": Estimate(value=key, standard_error=estimate.standard_error[..., 4:], precision=estimate.precision, replicates=estimate.replicates[..., 4:], normalization=estimate.normalization),
        }

    return record.applied(source, bounds, extent, clock, precision=precision).map_error(BlenderImagingFault.lifted).map(published)


def flickered(shell: Shell[Flicker]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds uniform lamp variation in scene-linear stops.

    Args:
        shell: Entry color and Flicker input endpoints.

    Returns:
        Exposed scene-linear color with graph, or accumulated graph faults.
    """
    motion, seed, amplitude, rate = Flicker.__struct_fields__
    speed, hold = Motion.__struct_fields__
    clock = Clock(key="clock", speed=shell.socket(motion, speed), hold=shell.socket(motion, hold))
    return paired(clock.value, origin("flicker_origin", shell.socket(seed), 8)).bind(
        lambda inputs: Fragment.joined(
            node("flicker_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": inputs[1]}, {}),
            math("flicker_step", "MULTIPLY", inputs[0], shell.socket(rate)),
            math("flicker_at", "ADD", End(node="flicker_step"), End(node="flicker_axes", socket="X")),
            node("flicker_wave", bpy.types.ShaderNodeTexNoise, {"W": End(node="flicker_at"), "Scale": 1.0}, {"noise_dimensions": "1D"}),
            math("flicker_signed", "MULTIPLY_ADD", End(node="flicker_wave", socket="Fac"), 2.0, -1.0),
            math("flicker_stops", "MULTIPLY", End(node="flicker_signed"), shell.socket(amplitude)),
            node("flicker_exposed", bpy.types.CompositorNodeExposure, {"Image": shell.source, "Exposure": End(node="flicker_stops")}, {}),
        ).map(ended("flicker_exposed", "Image"))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


damage: CompositorEffect[Damage] = CompositorEffect(
    record=Damage,
    graph=Shell.passed,
    coverage=Coverage.RESULT,
    requested=Some(precise),
    preparation=Some(prepared),
    registrables=(inspection(lambda: damage, "damage_key", "View Damage", "Integrated film marks and stationary gate", lambda: End(node="damage", socket="Image")),),
)
flicker = CompositorEffect(record=Flicker, graph=flickered, requested=Some(lambda _record, layer: Ok(PassRequest(layer=layer, precise=True))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["damage", "flicker", "flickered", "prepared"]
