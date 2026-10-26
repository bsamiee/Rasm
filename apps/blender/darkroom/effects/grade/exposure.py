"""Scene-linear exposure, the opacity-weighted gain of later exposure entries other effects read, and bilateral local exposure about middle gray."""

from typing import Annotated, ClassVar, Final, override

import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.rna.animation import Animation, Keys, sampled
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, mixed, NodeGroup, rgba, where, xyz
from blender.bpy.nodes.interface import item_key, Socket
from blender.bpy.render.compositor.blending import Blend
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.coordinates import pixels
from blender.bpy.render.compositor.effects import Alpha, Animates, CompositorEffect, Enabled, HIDDEN, Inputs, Shell
from blender.bpy.render.compositor.filters import bilateral
from blender.bpy.render.compositor.inputs import entry_input, path
from blender.bpy.render.compositor.masks import LUMINANCE, luminance
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import GREY, Stops
from blender.imaging.faults import ImagingResult
from blender.records.faults import accumulated, paired
from blender.records.fields import Unit

from ...chain.stages import Effect, Stage

# --- [MODELS] ---------------------------------------------------------------------------


class Exposure(Effect, frozen=True, kw_only=True):
    """Scales scene-linear light by a power of two, keeping alpha."""

    stage = Stage.EXPOSURE
    pointwise = True
    stops: Annotated[Stops, msgspec.Meta(title="Exposure", description="Gain in stops, each stop doubling light")] = 0.0

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns OCIO's unclamped RGB scale with an identity alpha channel."""
        return Ok(Some(PyOpenColorIO.MatrixTransform.Scale([2.0**self.stops] * 3 + [1.0])))


class LocalExposure(Effect, frozen=True, kw_only=True):
    """Compresses broad tones toward middle gray and scales detail about their base, the base a bilateral grid of log luminance mixed with a wide blur."""

    stage = Stage.LOCAL_EXPOSURE
    pointwise = False
    bounds: ClassVar[tuple[float, float]] = (-8.0, 4.0)
    pairs: ClassVar[int] = 16
    radius: ClassVar[float] = 0.1
    highlights: Annotated[Unit, msgspec.Meta(description="Base contrast above middle gray, 0 flat")] = 1.0
    shadows: Annotated[Unit, msgspec.Meta(description="Base contrast below middle gray, 0 flat")] = 1.0
    clarity: Annotated[float, msgspec.Meta(ge=1.0, le=2.2, description="Detail scale about base")] = 1.0
    smoothing: Annotated[Unit, msgspec.Meta(description="Share of the wide blur in the base")] = 0.6
    reach: Annotated[Unit, msgspec.Meta(description="Wide blur radius over greater frame side")] = 0.25


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GAIN]


def _products(identity: Identity, scene: bpy.types.Scene, held: tuple[bpy.types.SceneCompositorEffect, ...], frames: tuple[float, ...]) -> BlenderResult[npt.NDArray[np.float64]]:
    """Products over frames of the held Exposure entries' gains, each mixed by its opacity and render enable."""
    (stops,) = Exposure.__struct_fields__
    _, opacity, _ = Blend.__struct_fields__

    def samples(entry: bpy.types.SceneCompositorEffect) -> BlenderResult[tuple[tuple[float, ...], ...]]:
        return paired(path(identity, entry, stops), path(identity, entry, item_key(Shell.blend, opacity))).bind(
            lambda paths: accumulated(sampled(scene, address, frames) for address in (*paths, entry.path_from_id(Enabled.RENDER.descriptor.prop.identifier)))
        )

    def product(found: tuple[tuple[tuple[float, ...], ...], ...]) -> npt.NDArray[np.float64]:
        values, weights, enabled = np.asarray(found, dtype=np.float64).reshape(-1, 3, len(frames)).transpose(1, 0, 2)
        return np.prod(1.0 + weights * enabled * (np.exp2(values) - 1.0), axis=0)

    return accumulated(map(samples, held)).map(product)


def _later(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Effect) -> BlenderResult[tuple[Animation, ...]]:
    """Keys the entry's later gain input to the product of the Exposure entries after it over the scene frame range."""
    frames = tuple(map(float, range(scene.frame_start, scene.frame_end + 1)))
    ordered = tuple(scene.compositor_effects)
    following = ordered[ordered.index(entry) + 1 :]
    return paired(
        entry_input(identity, entry, LATER.key).map(lambda held: held.path_from_id()),
        _products(identity, scene, tuple(held for held in exposure.entries(identity, scene) if held in following), frames),
    ).map(lambda pair: (Animation(struct=pair[0], member="value", source=Keys(points=tuple(zip(frames, map(float, pair[1]))))),))


def gain(identity: Identity, scene: bpy.types.Scene) -> BlenderResult[float]:
    """Returns the stops of the Exposure entries' opacity-weighted render gains at the current frame, zero without an entry."""
    return _products(identity, scene, exposure.entries(identity, scene), (scene.frame_current_final,)).map(lambda product: float(np.log2(product[0])))


# --- [GRAPHS]


def exposed(shell: Shell[Exposure]) -> BlenderResult[Expr]:
    """Scales stage input RGB by two to the stops, alpha unchanged."""
    (stops,) = Exposure.__struct_fields__
    return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", shell.input(stops))), socket="Image", shape="RGBA"))


def localized(shell: Shell[LocalExposure]) -> BlenderResult[Expr]:
    """Scales the straight stage input by its local gain about the base, detail scaled by clarity, alpha unchanged."""
    highlights, shadows, clarity, smoothing, reach = map(shell.input, LocalExposure.__struct_fields__)
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

LATER: Final = Socket(
    key="later_gain",
    name="Later Gain",
    kind=bpy.types.NodeSocketFloat,
    description="Product of later render-enabled exposure gains mixed by opacity",
    members={HIDDEN: True, Descriptor.declared(bpy.types.NodeTreeInterfaceSocketFloat, "default_value", bpy.types.FloatProperty): 1.0},
)
later: Final = (Inputs(sockets=(LATER,)), Animates[Effect](keys=_later))
exposure = CompositorEffect(record=Exposure, graph=exposed)
local_exposure = CompositorEffect(record=LocalExposure, graph=localized, alpha=Alpha.STRAIGHT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LATER", "Exposure", "LocalExposure", "exposure", "gain", "later", "local_exposure"]
