"""Exposure gain and a coverage-normalized bilateral local exposure graph."""

from collections.abc import Mapping
from itertools import batched
from math import log2
from typing import ClassVar

import bpy
from expression import Option, Result
import msgspec
import numpy as np

from rasm.blender.data.rna.animation import Animation
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from rasm.blender.nodes.graphs import compared, End, ended, Fragment, framed, FrameSpace, masked, math, mixed, nested, node, NodeGroup, Operand, Part, pixels, Socket
from rasm.blender.render.effects.compositing.masks import luminance
from rasm.blender.render.effects.stack import CompositorEffect, entries, keyed_product, Shell
from rasm.imaging.analysis.metering import Samples
from rasm.imaging.chain import Effect, GREY
from rasm.imaging.effects.tone.exposure import Exposure, LocalExposure
from rasm.imaging.faults import paired

# --- [MODELS] ---------------------------------------------------------------------------


class ExposureEffect(CompositorEffect[Exposure], frozen=True, kw_only=True):
    """Exposure entry supplying the scene's absolute gain port."""

    gain: ClassVar[Socket] = Socket(
        key="exposure_gain",
        name="Exposure Gain",
        kind=bpy.types.NodeSocketFloat,
        description="Product of later render-enabled exposure gains mixed by opacity",
        members={
            Descriptor(root=bpy.types.NodeTreeInterfaceSocket, owner="", prop=bpy.types.NodeTreeInterfaceSocket.bl_rna.properties["hide_in_modifier"]): True,
            Descriptor(root=bpy.types.NodeTreeInterfaceSocketFloat, owner="", prop=bpy.types.NodeTreeInterfaceSocketFloat.bl_rna.properties["default_value"]): 1.0,
        },
    )

    def keyed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, _record: Effect) -> BlenderResult[tuple[Animation, ...]]:
        """Keys the opacity-weighted gain of later exposure entries."""
        return keyed_product(identity, scene, entry, self.record, Exposure.stops.__name__, self.gain.key, np.exp2)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def stops(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[float]:
        """Returns the earliest exposure, or zero when the scene has no gain entry."""
        return self.current(identity, scene).map(lambda held: held.map(lambda record: record.stops).default_value(0.0))

    def written(self, identity: Identity, scene: bpy.types.Scene, stops: float) -> BlenderResult[None]:
        """Writes absolute stops while retaining the entry's blend and masks."""
        entry: BlenderResult[bpy.types.SceneCompositorEffect] = Option.of_optional(next(iter(entries(identity, scene, self.record)), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=self.record.__name__),)
        )
        return Result[bpy.types.SceneCompositorEffect, BlenderFaults].bind(
            entry, lambda entry: self.read(identity, entry).bind(lambda held: self.write(identity, scene, entry, msgspec.structs.replace(held, effect=Exposure(stops=stops))))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def exposed(shell: Shell[Exposure]) -> BlenderResult[tuple[Fragment, End]]:
    """Scales the entering RGB by two to the exposure stops, keeping alpha."""
    return node("gain", bpy.types.CompositorNodeExposure, {"Image": shell.source, "Exposure": shell.socket(Exposure.stops.__name__)}, {}).map(ended("gain", "Image"))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]


def localized(source: Operand, working: NodeGroup, frame: FrameSpace, highlights: Operand, shadows: Operand, clarity: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Builds local gain from straight luminance and common blurred coverage."""
    (low, high), count, grey = LocalExposure.grid_range, LocalExposure.grid_slices, log2(GREY)
    pairs = tuple(enumerate(batched(range(count), 2)))
    alpha, base = End(node="channels", socket="Alpha"), End(node="base", socket="Result_Float")

    def spread(key: Key, channels: Mapping[str, Operand], size: End) -> tuple[Part, ...]:
        return (
            node(f"{key}_pack", bpy.types.CompositorNodeCombineColor, channels, {"mode": "RGB"}),
            node(f"{key}_blur", bpy.types.CompositorNodeBlur, {"Image": End(node=f"{key}_pack", socket="Image"), "Size": size, "Type": "Fast Gaussian"}, {}),
            node(key, bpy.types.CompositorNodeSeparateColor, {"Image": End(node=f"{key}_blur", socket="Image")}, {"mode": "RGB"}),
        )

    return Fragment.joined(
        nested("luminance", working, {"image": source}, "luminance"),
        node("channels", bpy.types.CompositorNodeSeparateColor, {"Image": source}, {"mode": "RGB"}),
        pixels("grid_size", frame, (LocalExposure.grid_radius, LocalExposure.grid_radius, 0.0), "VECTOR"),
        pixels("frame_size", frame, (LocalExposure.blurred_radius, LocalExposure.blurred_radius, 0.0), "VECTOR"),
        *(
            part
            for j, (a, b) in pairs
            for part in spread(
                f"grid_{j}", {"Red": End(node=f"moment_{a}"), "Green": End(node=f"mass_{a}"), "Blue": End(node=f"moment_{b}"), "Alpha": End(node=f"mass_{b}")}, End(node="grid_size", socket="Vector")
            )
        ),
        *spread("frame", {"Red": End(node="weighted"), "Green": alpha}, End(node="frame_size", socket="Vector")),
        node("gain", bpy.types.CompositorNodeExposure, {"Image": source, "Exposure": End(node="exponent")}, {}),
        math("floor", "MAXIMUM", End(node="luminance", socket="luminance"), Samples.floor),
        math("log", "LOGARITHM", End(node="floor"), 2.0),
        math("weighted", "MULTIPLY", alpha, End(node="log")),
        math("position", "MULTIPLY_ADD", End(node="log"), 1.0 / (high - low), low / (low - high), clamp=True),
        *(
            part
            for k in range(count)
            for part in (
                math(f"offset_{k}", "MULTIPLY_ADD", End(node="position"), float(count - 1), float(-k)),
                math(f"distance_{k}", "ABSOLUTE", End(node=f"offset_{k}")),
                math(f"weight_{k}", "SUBTRACT", 1.0, End(node=f"distance_{k}"), clamp=True),
                math(f"mass_{k}", "MULTIPLY", alpha, End(node=f"weight_{k}")),
                math(f"moment_{k}", "MULTIPLY", End(node="weighted"), End(node=f"weight_{k}")),
            )
        ),
        *(
            math(f"{total}_{k}", "MULTIPLY_ADD", End(node=f"weight_{k}"), End(node=f"grid_{j}", socket=channel), End(node=f"{total}_{k - 1}") if k else 0.0)
            for j, pair in pairs
            for k, sides in zip(pair, (("Red", "Green"), ("Blue", "Alpha")))
            for total, channel in zip(("numerator", "denominator"), sides)
        ),
        math("blurred", "DIVIDE", End(node="frame", socket="Red"), End(node="frame", socket="Green")),
        math("sliced", "DIVIDE", End(node=f"numerator_{count - 1}"), End(node=f"denominator_{count - 1}")),
        compared("covered", "GREATER_THAN", End(node=f"denominator_{count - 1}"), 0.0),
        masked("bilateral", "FLOAT", End(node="covered", socket="Result"), End(node="blurred"), End(node="sliced")),
        mixed("base", "FLOAT", LocalExposure.blurred_share, End(node="bilateral"), End(node="blurred")),
        compared("above", "GREATER_THAN", base, grey),
        masked("slope", "FLOAT", End(node="above", socket="Result"), shadows, highlights),
        math("broad", "SUBTRACT", base, grey),
        math("slope_less", "SUBTRACT", End(node="slope"), 1.0),
        math("detail", "SUBTRACT", End(node="log"), base),
        math("clarity_less", "SUBTRACT", clarity, 1.0),
        math("detail_term", "MULTIPLY", End(node="detail"), End(node="clarity_less")),
        math("exponent", "MULTIPLY_ADD", End(node="broad"), End(node="slope_less"), End(node="detail_term")),
    ).map(ended("gain", "Image"))


# --- [COMPOSITION] ----------------------------------------------------------------------

exposure = ExposureEffect(record=Exposure, graph=exposed)
local_exposure = CompositorEffect(
    record=LocalExposure,
    straight=True,
    graph=lambda shell: paired(luminance(shell.config), framed("frame_space", shell.source, shell.aspect, shell.scale)).bind(
        lambda pair: localized(shell.source, *pair, *(shell.socket(field) for field in (LocalExposure.highlights.__name__, LocalExposure.shadows.__name__, LocalExposure.clarity.__name__)))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ExposureEffect", "exposed", "exposure", "local_exposure", "localized"]
