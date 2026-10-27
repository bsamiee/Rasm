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
from blender.bpy.data.rna.descriptors import id_path, Struct, Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import Expr, math, mixed, NodeGroup, rgba, where, xyz
from blender.bpy.nodes.interface import Interface, item_key, Socket
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.coordinates import pixels
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Enabled, Shell
from blender.bpy.render.compositor.filters import bilateral
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.compositor.masks import LUMINANCE, luminance
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import GREY
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason

from ...chain.stages import Effect, Stage, transform

# --- [CONSTANTS] ------------------------------------------------------------------------

LATER: Final = Socket(
    key="later_gain",
    name="Later Gain",
    kind=bpy.types.NodeSocketFloat,
    description="Product of later render-enabled exposure gains mixed by opacity",
    members={"hide_in_modifier": True, "default_value": 1.0},
)

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
            (entry_input(identity, entry, "stops"), "value"),
            (entry_input(identity, entry, item_key(Shell.blend, "opacity")), "value"),
            (Ok(entry), Enabled.RENDER.descriptor.prop.identifier),
        )
        return accumulated(held.bind(partial(_sampled, frames, member)) for held, member in channels)

    def product(found: tuple[tuple[tuple[float, ...], ...], ...]) -> npt.NDArray[np.float64]:
        values, weights, enabled = np.asarray(found, dtype=np.float64).reshape(-1, 3, len(frames)).transpose(1, 0, 2)
        return np.asarray(np.prod(1.0 + weights * enabled * (np.exp2(values) - 1.0), axis=0), dtype=np.float64)

    return accumulated(map(samples, held)).map(product)


def _sampled(frames: tuple[float, ...], member: str, owner: Struct) -> Outcome[tuple[float, ...]]:
    """Samples one member of an entry or its input at frames."""
    return sampled(owner, member, frames)


def later(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Effect) -> Outcome[tuple[Animation, ...]]:
    """Keys the entry's later gain input to the product of the Exposure entries after it over the scene frame range."""
    frames = tuple(map(float, range(scene.frame_start, scene.frame_end + 1)))
    ordered = tuple(scene.compositor_effects)
    following = ordered[ordered.index(entry) + 1 :]
    return paired(
        entry_input(identity, entry, LATER.key).bind(lambda held: id_path(held).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(held), member="value"))),
        _products(identity, tuple(held for held in exposure.entries(identity, scene) if held in following), frames),
    ).map(lambda pair: (Animation(owner=pair[0], member="value", source=Keys(points=tuple(zip(frames, map(float, pair[1]))))),))


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
            return Error(Fault.of(RecordReason.UNREADABLE, Exposure.__name__, repr(values)))


# --- [GRAPHS]


def exposed(shell: Shell[Exposure]) -> Outcome[Expr]:
    """Scales stage input RGB by two to the stops, alpha unchanged."""
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", shell.input("stops"))), socket="Image", shape="RGBA"))


def localized(shell: Shell[LocalExposure]) -> Outcome[Expr]:
    """Scales the straight stage input by its local gain about the base, detail scaled by clarity, alpha unchanged."""
    highlights, shadows, clarity, smoothing, reach = map(shell.input, ("highlights", "shadows", "clarity", "smoothing", "reach"))
    source, frame, grey = shell.color, shell.space, float(np.log2(GREY))
    alpha = source["Alpha"]

    def built(working: NodeGroup) -> Expr:
        log = math("LOGARITHM", math("MAXIMUM", working.placed({COLOR_INPUT.key: source}, LUMINANCE.key), Samples.floor), 2.0)
        spread = Expr(
            kind=bpy.types.CompositorNodeBlur,
            inputs=(("Image", rgba(alpha * log, alpha, 0.0)), ("Size", pixels(frame, xyz(reach, reach), "VECTOR")), ("Type", "Fast Gaussian")),
            socket="Image",
            shape="RGBA",
        )
        smooth = spread["Red"] / spread["Green"]
        base = mixed(smoothing, bilateral(log, log, alpha, frame, smooth, bounds=LocalExposure.bounds, pairs=LocalExposure.pairs, radius=LocalExposure.radius), smooth)
        exponent = math("MULTIPLY_ADD", base - grey, where(base > grey, highlights, shadows) - 1.0, (log - base) * (clarity - 1.0))
        return Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", source), ("Exposure", exponent)), socket="Image", shape="RGBA")

    return luminance(shell.config).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

exposure = CompositorEffect(
    record=Exposure,
    graph=exposed,
    interface=Interface(
        inputs=(Socket(key="stops", name="Stops", kind=bpy.types.NodeSocketFloat, description="Gain in stops, each stop doubling light", members={"min_value": -32.0, "max_value": 32.0}),)
    ),
)
local_exposure = CompositorEffect(
    record=LocalExposure,
    graph=localized,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        inputs=tuple(
            Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketFloat, description=text, members={"subtype": subtype, **members})
            for key, text, subtype, members in (
                ("highlights", "Base contrast above middle gray, 0 flat", "FACTOR", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                ("shadows", "Base contrast below middle gray, 0 flat", "FACTOR", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                ("clarity", "Detail scale about base", "NONE", {"min_value": 1.0, "max_value": 2.2, "default_value": 1.0}),
                ("smoothing", "Share of the wide blur in the base", "FACTOR", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.6}),
                ("reach", "Wide blur radius over greater frame side", "FACTOR", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.25}),
            )
        )
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LATER", "Exposure", "LocalExposure", "exposure", "gain", "later", "local_exposure"]
