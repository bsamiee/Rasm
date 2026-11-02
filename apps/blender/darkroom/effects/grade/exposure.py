"""Scene-linear exposure: gain in stops with the opacity-weighted gain of later exposure entries other effects read, lamp flicker along a noise, black and white levels measured from the entering frame, and bilateral local exposure about middle gray."""

from itertools import starmap
from typing import Final

import bpy
from expression import Ok, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.rna.animation import Animation, Keys, sampled
from blender.bpy.data.rna.paths import id_path, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes.graphs import Expr, math, mixed, rgba, Term, where, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.nodes.textures import Noise
from blender.bpy.render.color.spaces import EXPOSURE
from blender.bpy.render.compositor import motion
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.entries import Enabled
from blender.bpy.render.compositor.filters import bilateral
from blender.bpy.render.compositor.inputs import captured, entered, entry_input, GraphInputs, value, valued
from blender.bpy.render.compositor.masks import BLEND, luminance, OPACITY
from blender.bpy.render.compositor.motion import CLOCK, Clock, clocked, origin, SEED
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import GRAY
from blender.imaging.files.images import ImageFile
from blender.imaging.sampling.tiles import stream
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [CONSTANTS] ------------------------------------------------------------------------

LATER: Final = Socket(key="later_gain", kind=bpy.types.NodeSocketFloat, description="Product of later render-enabled exposure gains mixed by opacity", hide_in_modifier=True, default_value=1.0)
STOPS: Final = Socket(key="stops", kind=bpy.types.NodeSocketFloat, description="Gain in stops, each stop doubling light", min_value=EXPOSURE.prop.hard_min, max_value=EXPOSURE.prop.hard_max)
_AMPLITUDE: Final = Socket(key="amplitude", kind=bpy.types.NodeSocketFloat, description="Largest signed variation in stops", min_value=-32.0, max_value=32.0, default_value=0.05)
_RATE: Final = Socket(key="rate", kind=bpy.types.NodeSocketFloat, description="Lamp evolution in noise cells per clock unit", min_value=0.0, default_value=2.5)
_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=24.0))
_BLACK: Final = Socket(key="black", kind=bpy.types.NodeSocketFloat, description="Scene-linear value mapping to 0")
_WHITE: Final = Socket(key="white", kind=bpy.types.NodeSocketFloat, description="Stops from black point to the value mapping to 1", min_value=EXPOSURE.prop.hard_min, max_value=EXPOSURE.prop.hard_max)
_HIGHLIGHTS: Final = Socket(key="highlights", kind=bpy.types.NodeSocketFloatFactor, description="Base contrast above middle gray, 0 flat", default_value=1.0)
_SHADOWS: Final = Socket(key="shadows", kind=bpy.types.NodeSocketFloatFactor, description="Base contrast below middle gray, 0 flat", default_value=1.0)
_CLARITY: Final = Socket(key="clarity", kind=bpy.types.NodeSocketFloat, description="Detail scale about base", min_value=1.0, max_value=2.2, default_value=1.0)
_SMOOTHING: Final = Socket(key="smoothing", kind=bpy.types.NodeSocketFloatFactor, description="Share of the wide blur in the base", default_value=0.6)
_REACH: Final = Socket(key="reach", kind=bpy.types.NodeSocketFloatFactor, description="Wide blur radius over greater frame side", default_value=0.25)

# --- [MODELS] ---------------------------------------------------------------------------


class Exposure(Case, frozen=True, kw_only=True):
    """Scales scene-linear light by a power of two, keeping alpha."""


class Flicker(Case, frozen=True, kw_only=True):
    """Uniform scene-linear lamp exposure variation in stops."""


class Levels(Case, frozen=True, kw_only=True):
    """Maps a scene-linear black point to 0 and a white point stops above it to 1."""


class LocalExposure(Case, frozen=True, kw_only=True):
    """Compresses broad tones toward middle gray and scales detail about their base, the base a bilateral grid of log luminance mixed with a wide blur."""


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GAIN]


def _products(identity: Identity, held: tuple[bpy.types.SceneCompositorEffect, ...], frames: tuple[float, ...], *, enabled: Enabled = Enabled.RENDER) -> Outcome[npt.NDArray[np.float64]]:
    """Products over frames of the held Exposure entries' gains, each mixed by its opacity and enable flag."""

    def samples(entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[tuple[float, ...], ...]]:
        channels: tuple[tuple[Outcome[Struct], str], ...] = ((entry_input(identity, entry, STOPS), "value"), (entry_input(identity, entry, BLEND, OPACITY), "value"), (Ok(entry), enabled.value))

        def each(owner: Outcome[Struct], member: str) -> Outcome[tuple[float, ...]]:
            return owner.bind(lambda found: sampled(found, (member,), frames))

        return accumulated(starmap(each, channels))

    def product(found: tuple[tuple[tuple[float, ...], ...], ...]) -> npt.NDArray[np.float64]:
        values, weights, flags = np.asarray(found, dtype=np.float64).reshape(-1, 3, len(frames)).transpose(1, 0, 2)
        return np.asarray(np.prod(1.0 + weights * flags * (np.exp2(values) - 1.0), axis=0), dtype=np.float64)

    return accumulated(map(samples, held)).map(product)


def later(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Case) -> Outcome[tuple[Animation, ...]]:
    """Keys the entry's later gain input to the product of the Exposure entries after it over the scene frame range."""
    frames = tuple(map(float, range(scene.frame_start, scene.frame_end + 1)))
    ordered = tuple(scene.compositor_effects)
    following = ordered[ordered.index(entry) + 1 :]
    return paired(
        entry_input(identity, entry, LATER).bind(lambda held: id_path(held).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(held), member="value"))),
        _products(identity, tuple(held for held in exposure.entries(identity, scene) if held in following), frames),
    ).map(lambda pair: (Animation(path=(*pair[0], "value"), source=Keys(points=tuple(zip(frames, map(float, pair[1]))))),))


def gain(identity: Identity, scene: bpy.types.Scene, *, enabled: Enabled = Enabled.RENDER) -> Outcome[float]:
    """Returns the stops of the Exposure entries' opacity-weighted gains under an enable flag at the current frame, zero without an entry."""
    return _products(identity, exposure.entries(identity, scene), (scene.frame_current_final,), enabled=enabled).map(lambda product: float(np.log2(product[0])))


def entry_stops(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[float]:
    """Returns an Exposure entry's own stops input."""
    return captured(identity, entry).map(valued).bind(lambda values: value(values, float, STOPS))


# --- [LEVELS]


def auto_levels(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[bool]:
    """Measure levels from the stage input."""
    return Samples.of(PyOpenColorIO.GetCurrentConfig(), file.pixels).bind(
        lambda samples: (
            samples
            .extent()
            .filter(lambda extent: extent.white > extent.low)
            .map(lambda extent: entered(identity, entry, {_BLACK.key: extent.low, _WHITE.key: float(np.log2(extent.white - extent.low))}))
            .default_value(Ok(False))
        )
    )


# --- [GRAPHS]


def exposed(color: Term, stops: Term) -> Expr:
    """Scales RGB by two to the stops through the Exposure node, alpha unchanged."""
    return Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", color), ("Exposure", stops)), socket="Image", socket_type=bpy.types.NodeSocketColor)


def _exposure(shell: GraphInputs[Exposure]) -> Outcome[Expr]:
    """Scales stage input RGB by two to the stops."""
    return Ok(exposed(shell.color, shell.input(STOPS)))


def _flicker(shell: GraphInputs[Flicker]) -> Outcome[Expr]:
    """Exposes the entry color by the signed noise wave scaled to the amplitude in stops."""
    phase = clocked(shell.input, _CLOCK) * shell.input(_RATE) + origin(shell.input(SEED), float(stream(Flicker)))["X"]
    wave = Noise(noise_dimensions="1D", scale=1.0).node((0.0, 0.0, 0.0), phase)
    return Ok(exposed(shell.color, math("MULTIPLY_ADD", wave, 2.0, -1.0) * shell.input(_AMPLITUDE)))


def _levels(shell: GraphInputs[Levels]) -> Outcome[Expr]:
    """Subtracts the coverage-weighted black point and lowers by the white span in stops."""
    color = shell.color
    return Ok(exposed(color - shell.input(_BLACK) * color["Alpha"], -shell.input(_WHITE)))


def _local_exposure(shell: GraphInputs[LocalExposure]) -> Outcome[Expr]:
    """Scales the straight stage input by its local gain about the base, detail scaled by clarity, alpha unchanged."""
    highlights, shadows, clarity, smoothing, reach = map(shell.input, (_HIGHLIGHTS, _SHADOWS, _CLARITY, _SMOOTHING, _REACH))
    source, frame, gray = shell.color, shell.space, float(np.log2(GRAY))
    alpha = source["Alpha"]

    def built(level: Expr) -> Expr:
        log = math("LOGARITHM", math("MAXIMUM", level, Samples.floor), 2.0)
        spread = Expr(
            kind=bpy.types.CompositorNodeBlur,
            inputs=(("Image", rgba(alpha * log, alpha, 0.0)), ("Size", frame.pixel_size(xyz(reach, reach))), ("Type", "Fast Gaussian")),
            socket="Image",
            socket_type=bpy.types.NodeSocketColor,
        )
        smooth = spread["Red"] / spread["Green"]
        base = mixed(smoothing, bilateral(log, log, alpha, frame, smooth, bounds=(-8.0, 4.0), pairs=16, radius=0.1), smooth)
        return exposed(source, math("MULTIPLY_ADD", base - gray, where(base > gray, highlights, shadows) - 1.0, (log - base) * (clarity - 1.0)))

    return luminance(shell.config, source).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

exposure: Final = CompositorEffect(_exposure, record=Exposure, interface=Interface(inputs=(STOPS,)), pointwise=True)
flicker: Final = CompositorEffect(_flicker, record=Flicker, interface=Interface(inputs=(SEED, _AMPLITUDE, _RATE), panels=(_CLOCK,)), pointwise=True, request=Some(precise), derives=(motion.timing,))
levels: Final = CompositorEffect(_levels, record=Levels, interface=Interface(inputs=(_BLACK, _WHITE)), pointwise=True, registers=(READ.measuring(auto_levels, icon=Some("SEQ_HISTOGRAM")),))
local_exposure: Final = CompositorEffect(_local_exposure, record=LocalExposure, mix=Blended(alpha=Alpha.STRAIGHT), interface=Interface(inputs=(_HIGHLIGHTS, _SHADOWS, _CLARITY, _SMOOTHING, _REACH)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LATER", "STOPS", "Exposure", "Flicker", "Levels", "LocalExposure", "entry_stops", "exposed", "exposure", "flicker", "gain", "later", "levels", "local_exposure"]
