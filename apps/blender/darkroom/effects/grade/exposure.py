"""Scene-linear exposure, the opacity-weighted gain of later exposure entries other effects read, and bilateral local exposure about middle gray."""

from collections.abc import Mapping
from functools import partial
from typing import ClassVar, Final

import bpy
from expression import Error, Ok, Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.rna.animation import Animation, Keys, sampled
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import id_path, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.desired import Interface, item_key, Socket
from blender.bpy.nodes.graphs import Expr, math, mixed, rgba, where, xyz
from blender.bpy.render.color.spaces import EXPOSURE
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, Enabled
from blender.bpy.render.compositor.filters import bilateral
from blender.bpy.render.compositor.inputs import entry_input, Shell
from blender.bpy.render.compositor.masks import BLEND, luminance, OPACITY
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import GREY
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

from ...chain.stages import Effect, Stage, transform

# --- [CONSTANTS] ------------------------------------------------------------------------

LATER: Final = Socket(key="later_gain", kind=bpy.types.NodeSocketFloat, description="Product of later render-enabled exposure gains mixed by opacity", hide_in_modifier=True, default_value=1.0)
_STOPS: Final = Socket(key="stops", kind=bpy.types.NodeSocketFloat, description="Gain in stops, each stop doubling light", min_value=EXPOSURE.prop.hard_min, max_value=EXPOSURE.prop.hard_max)
_HIGHLIGHTS: Final = Socket(key="highlights", kind=bpy.types.NodeSocketFloatFactor, description="Base contrast above middle gray, 0 flat", min_value=0.0, max_value=1.0, default_value=1.0)
_SHADOWS: Final = Socket(key="shadows", kind=bpy.types.NodeSocketFloatFactor, description="Base contrast below middle gray, 0 flat", min_value=0.0, max_value=1.0, default_value=1.0)
_CLARITY: Final = Socket(key="clarity", kind=bpy.types.NodeSocketFloat, description="Detail scale about base", min_value=1.0, max_value=2.2, default_value=1.0)
_SMOOTHING: Final = Socket(key="smoothing", kind=bpy.types.NodeSocketFloatFactor, description="Share of the wide blur in the base", min_value=0.0, max_value=1.0, default_value=0.6)
_REACH: Final = Socket(key="reach", kind=bpy.types.NodeSocketFloatFactor, description="Wide blur radius over greater frame side", min_value=0.0, max_value=1.0, default_value=0.25)

# --- [MODELS] ---------------------------------------------------------------------------


class Exposure(Effect, frozen=True, kw_only=True):
    """Scales scene-linear light by a power of two, keeping alpha."""

    stage = Stage.EXPOSURE
    pointwise = True


class LocalExposure(Effect, frozen=True, kw_only=True):
    """Compresses broad tones toward middle gray and scales detail about their base, the base a bilateral grid of log luminance mixed with a wide blur."""

    stage = Stage.LOCAL_EXPOSURE
    pointwise = False
    bounds: ClassVar[tuple[float, float]] = (-8.0, 4.0)
    pairs: ClassVar[int] = 16
    radius: ClassVar[float] = 0.1


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GAIN]


def _products(identity: Identity, held: tuple[bpy.types.SceneCompositorEffect, ...], frames: tuple[float, ...]) -> Outcome[npt.NDArray[np.float64]]:
    """Products over frames of the held Exposure entries' gains, each mixed by its opacity and render enable."""

    def samples(entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[tuple[float, ...], ...]]:
        channels: tuple[tuple[Outcome[Struct], str], ...] = (
            (entry_input(identity, entry, _STOPS.key), "value"),
            (entry_input(identity, entry, item_key(BLEND, OPACITY.key)), "value"),
            (Ok(entry), Enabled.RENDER.value),
        )
        return accumulated(held.bind(partial(_sampled, frames, member)) for held, member in channels)

    def product(found: tuple[tuple[tuple[float, ...], ...], ...]) -> npt.NDArray[np.float64]:
        values, weights, enabled = np.asarray(found, dtype=np.float64).reshape(-1, 3, len(frames)).transpose(1, 0, 2)
        return np.asarray(np.prod(1.0 + weights * enabled * (np.exp2(values) - 1.0), axis=0), dtype=np.float64)

    return accumulated(map(samples, held)).map(product)


def _sampled(frames: tuple[float, ...], member: str, owner: Struct) -> Outcome[tuple[float, ...]]:
    """Samples one member of an entry or its input at frames."""
    return sampled(owner, (member,), frames)


def later(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Effect) -> Outcome[tuple[Animation, ...]]:
    """Keys the entry's later gain input to the product of the Exposure entries after it over the scene frame range."""
    frames = tuple(map(float, range(scene.frame_start, scene.frame_end + 1)))
    ordered = tuple(scene.compositor_effects)
    following = ordered[ordered.index(entry) + 1 :]
    return paired(
        entry_input(identity, entry, LATER.key).bind(lambda held: id_path(held).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(held), member="value"))),
        _products(identity, tuple(held for held in exposure.entries(identity, scene) if held in following), frames),
    ).map(lambda pair: (Animation(path=(*pair[0], "value"), source=Keys(points=tuple(zip(frames, map(float, pair[1]))))),))


def gain(identity: Identity, scene: bpy.types.Scene) -> Outcome[float]:
    """Returns the stops of the Exposure entries' opacity-weighted render gains at the current frame, zero without an entry."""
    return _products(identity, exposure.entries(identity, scene), (scene.frame_current_final,)).map(lambda product: float(np.log2(product[0])))


@transform.register
def _(_record: Exposure, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns OCIO's unclamped RGB scale with an identity alpha channel."""
    match values:
        case {"stops": float(stops)}:
            return Ok(Some(PyOpenColorIO.MatrixTransform.Scale([2.0**stops] * 3 + [1.0])))
        case _:
            return Error(Fault.of(Reason.UNREADABLE, Exposure.__name__, repr(values)))


# --- [GRAPHS]


def _exposure(shell: Shell[Exposure]) -> Outcome[Expr]:
    """Scales stage input RGB by two to the stops, alpha unchanged."""
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", shell.input(_STOPS))), socket="Image", shape=bpy.types.NodeSocketColor))


exposure: Final = CompositorEffect(graph=_exposure, record=Exposure, interface=Interface(inputs=(_STOPS,)))


def _local_exposure(shell: Shell[LocalExposure]) -> Outcome[Expr]:
    """Scales the straight stage input by its local gain about the base, detail scaled by clarity, alpha unchanged."""
    highlights, shadows, clarity, smoothing, reach = map(shell.input, (_HIGHLIGHTS, _SHADOWS, _CLARITY, _SMOOTHING, _REACH))
    source, frame, grey = shell.color, shell.space, float(np.log2(GREY))
    alpha = source["Alpha"]

    def built(level: Expr) -> Expr:
        log = math("LOGARITHM", math("MAXIMUM", level, Samples.floor), 2.0)
        spread = Expr(
            kind=bpy.types.CompositorNodeBlur,
            inputs=(("Image", rgba(alpha * log, alpha, 0.0)), ("Size", frame.pixel_size(xyz(reach, reach))), ("Type", "Fast Gaussian")),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
        smooth = spread["Red"] / spread["Green"]
        base = mixed(smoothing, bilateral(log, log, alpha, frame, smooth, bounds=LocalExposure.bounds, pairs=LocalExposure.pairs, radius=LocalExposure.radius), smooth)
        exponent = math("MULTIPLY_ADD", base - grey, where(base > grey, highlights, shadows) - 1.0, (log - base) * (clarity - 1.0))
        return Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", source), ("Exposure", exponent)), socket="Image", shape=bpy.types.NodeSocketColor)

    return luminance(shell.config, source).map(built)


local_exposure: Final = CompositorEffect(
    graph=_local_exposure, record=LocalExposure, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(_HIGHLIGHTS, _SHADOWS, _CLARITY, _SMOOTHING, _REACH))
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LATER", "Exposure", "LocalExposure", "exposure", "gain", "later", "local_exposure"]
