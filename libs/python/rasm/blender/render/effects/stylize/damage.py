# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
"""Staged footprint-filtered film damage and native lamp flicker."""

from collections.abc import Mapping
from fractions import Fraction
from math import floor

import bpy
from expression import Ok, Some
import OpenImageIO
import PyOpenColorIO

from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderImagingFault, BlenderResult
from rasm.blender.nodes.graphs import Clock, End, ended, Fragment, math, node, origin
from rasm.blender.render.effects.stack import CameraFrame, CompositorEffect, Coverage, precise, Shell
from rasm.blender.render.effects.viewers import inspection
from rasm.blender.render.view_layers.passes import PassRequest
from rasm.imaging.effects.generators.noise import Estimate, Precision
from rasm.imaging.effects.stylize.damage import Damage, Flicker
from rasm.imaging.faults import paired
from rasm.imaging.parameters import Motion

# --- [OPERATIONS] ------------------------------------------------------------------------


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
    """Filters the captured film in original camera units and publishes its damage key.

    Args:
        _identity: Native stage job owner.
        scene: Evaluated scene supplying the film clock.
        record: Evaluated film marks, weave, and stationary gate.
        _config: Stage color configuration; film uses associated scene-linear RGBA.
        source: Top-first native DATA image with its retained FULL window.
        precision: Caller-selected integration resolution and error replicates.
        frame: Original camera pixel aspect and retained overscan scale.
        _entry: Evaluated native stage entry.

    Returns:
        The same record and jointly estimated image and scalar damage key,
        or the original typed imaging refusal at the native package boundary.
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

    return record.applied(source, bounds, extent, clock, precision=precision).map_error(lambda faults: (BlenderImagingFault(faults=faults),)).map(published)


def flickered(shell: Shell[Flicker]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds a spatially uniform lamp variation in scene-linear exposure stops.

    Args:
        shell: Entry color and native input endpoints of the Flicker record.

    Returns:
        Exposed scene-linear color with its graph, or accumulated native graph faults.
    """
    clock = Clock(key="clock", speed=shell.socket(Flicker.motion.__name__, Motion.speed.__name__), hold=shell.socket(Flicker.motion.__name__, Motion.hold.__name__))
    return paired(clock.value, origin("flicker_origin", shell.socket(Flicker.seed.__name__), 8)).bind(
        lambda inputs: Fragment.joined(
            node("flicker_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": inputs[1]}, {}),
            math("flicker_step", "MULTIPLY", inputs[0], shell.socket(Flicker.rate.__name__)),
            math("flicker_at", "ADD", End(node="flicker_step"), End(node="flicker_axes", socket="X")),
            node("flicker_wave", bpy.types.ShaderNodeTexNoise, {"W": End(node="flicker_at"), "Scale": 1.0}, {"noise_dimensions": "1D"}),
            math("flicker_signed", "MULTIPLY_ADD", End(node="flicker_wave", socket="Fac"), 2.0, -1.0),
            math("flicker_stops", "MULTIPLY", End(node="flicker_signed"), shell.socket(Flicker.amplitude.__name__)),
            node("flicker_exposed", bpy.types.CompositorNodeExposure, {"Image": shell.source, "Exposure": End(node="flicker_stops")}, {}),
        ).map(ended("flicker_exposed", "Image"))
    )


# --- [COMPOSITION] -----------------------------------------------------------------------


damage: CompositorEffect[Damage] = CompositorEffect(
    record=Damage,
    graph=lambda shell: Ok((Fragment(), shell.source)),
    coverage=Coverage.RESULT,
    requested=Some(precise),
    preparation=Some(prepared),
    registrables=(inspection(lambda: damage, "damage_key", "View Damage", "Shows the integrated film marks and stationary aperture", lambda: End(node="damage", socket="Image")),),
)
flicker = CompositorEffect(record=Flicker, graph=flickered, requested=Some(lambda _record, layer: Ok(PassRequest(layer=layer, precise=True))))


# --- [EXPORTS] ---------------------------------------------------------------------------

__all__ = ["damage", "flicker", "flickered", "prepared"]
