"""Exposure gain and coverage-normalized bilateral local exposure graphs."""

import functools
from itertools import batched
from math import log2
from typing import ClassVar

import bpy
from expression import Ok
import numpy as np

from blender.bpy.data.rna.animation import Animation
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, NodeGroup, rgba, where
from blender.bpy.nodes.interface import Socket
from blender.bpy.render.color.spaces import COLOR_INPUT
from blender.bpy.render.effects.compositing.masks import LUMINANCE, luminance
from blender.bpy.render.effects.coordinates import pixels
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Shell
from blender.bpy.render.effects.stack import keyed_product
from blender.imaging.analysis.metering import Samples
from blender.imaging.chain import Effect, GREY
from blender.imaging.effects.tone.exposure import Exposure, LocalExposure

# --- [MODELS] ---------------------------------------------------------------------------


class ExposureEffect(CompositorEffect[Exposure], frozen=True, kw_only=True):
    """Exposure entry supplying scene absolute gain."""

    gain: ClassVar[Socket] = Socket(
        key="exposure_gain",
        name="Exposure Gain",
        kind=bpy.types.NodeSocketFloat,
        description="Product of later render-enabled exposure gains mixed by opacity",
        members={
            Descriptor.declared(bpy.types.NodeTreeInterfaceSocket, "hide_in_modifier", bpy.types.BoolProperty): True,
            Descriptor.declared(bpy.types.NodeTreeInterfaceSocketFloat, "default_value", bpy.types.FloatProperty): 1.0,
        },
    )

    def keyed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Effect) -> BlenderResult[tuple[Animation, ...]]:
        """Keys opacity-weighted gain of later exposure entries."""
        (stops,) = Exposure.__struct_fields__
        return keyed_product(identity, scene, entry, self.record, stops, self.gain.key, np.exp2)


# --- [OPERATIONS] -----------------------------------------------------------------------


def exposed(shell: Shell[Exposure]) -> BlenderResult[Expr]:
    """Scales stage input RGB by 2^stops, alpha unchanged."""
    (stops,) = Exposure.__struct_fields__
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", shell.input(stops))), socket="Image", shape="RGBA"))


def localized(shell: Shell[LocalExposure]) -> BlenderResult[Expr]:
    """Scales the straight stage input by local gain from a coverage-weighted bilateral grid over working luminance and a shared blurred base.

    Args:
        shell: Straight stage input, camera frame, working config, and base contrast and clarity inputs.

    Returns:
        Lowered graph of the source under its local exposure, alpha unchanged, or luminance group faults.
    """
    (low, high), count, grey = LocalExposure.grid_range, LocalExposure.grid_slices, log2(GREY)
    highlights, shadows, clarity = map(shell.input, LocalExposure.__struct_fields__)
    source, frame = shell.color, shell.space
    alpha = source["Alpha"]
    pairs = tuple(batched(range(count), 2))

    def blurred(channels: Expr, radius: float) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", channels), ("Size", pixels(frame, (radius, radius, 0.0), "VECTOR")), ("Type", "Fast Gaussian")), socket="Image", shape="RGBA")

    def built(working: NodeGroup) -> Expr:
        log = math("LOGARITHM", math("MAXIMUM", working.placed({COLOR_INPUT.key: source}, LUMINANCE.key), Samples.floor), 2.0)
        weighted = alpha * log
        position = math("MULTIPLY_ADD", log, 1.0 / (high - low), low / (low - high), clamp=True)
        weights = tuple(math("SUBTRACT", 1.0, abs(math("MULTIPLY_ADD", position, float(count - 1), float(-index))), clamp=True) for index in range(count))
        grids = tuple(blurred(rgba(weighted * weights[first], alpha * weights[first], weighted * weights[second], alpha * weights[second]), LocalExposure.grid_radius) for first, second in pairs)
        sides: tuple[tuple[Component, Component], ...] = (("Red", "Green"), ("Blue", "Alpha"))
        slices = tuple((weights[index], grid[moment], grid[mass]) for grid, pair in zip(grids, pairs) for index, (moment, mass) in zip(pair, sides))
        first, *rest = slices

        def totaled(side: int) -> Expr:
            return functools.reduce(lambda total, held: math("MULTIPLY_ADD", held[0], held[side], total), rest, math("MULTIPLY_ADD", first[0], first[side], 0.0))

        numerator, denominator = totaled(1), totaled(2)
        spread = blurred(rgba(weighted, alpha, 0.0), LocalExposure.blurred_radius)
        smooth = spread["Red"] / spread["Green"]
        base = mixed(LocalExposure.blurred_share, where(denominator > 0.0, numerator / denominator, smooth), smooth)
        exponent = math("MULTIPLY_ADD", base - grey, where(base > grey, highlights, shadows) - 1.0, (log - base) * (clarity - 1.0))
        return Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", source), ("Exposure", exponent)), socket="Image", shape="RGBA")

    return luminance(shell.config).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

exposure = ExposureEffect(record=Exposure, graph=exposed)
local_exposure = CompositorEffect(record=LocalExposure, alpha=Alpha.STRAIGHT, graph=localized)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ExposureEffect", "exposed", "exposure", "local_exposure", "localized"]
