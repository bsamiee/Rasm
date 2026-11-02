"""Darkroom temporal denoise: the follow-up job after each render blending motion-warped neighboring frames by color similarity, its group built over the written color and pixel-motion sequences."""

from functools import reduce
import operator
from pathlib import Path
from typing import Final

import bpy
from expression import Some

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes import functions
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.content import ImageFrames, Loaded
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, switched, where, xyz
from blender.bpy.nodes.interfaces import Interface
from blender.bpy.render.compositor.colors import COLOR_RESULT, with_alpha
from blender.bpy.render.compositor.coordinates import PixelGrid, uv_mapped
from blender.bpy.render.outputs.results import COMBINED
from blender.bpy.render.sessions.followups import FollowedRender, FollowUp, FollowUpSettings, WrittenPass
from blender.records.faults import Outcome, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

VECTOR_PASS: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_vector",), bpy.types.BoolProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class TemporalDenoise(FollowUpSettings):
    """Scene setting of the job blending motion-warped neighboring frames by color similarity after each render."""

    hue: bpy.props.FloatProperty(name="Hue Threshold", description="Hue distance zeroing neighbor weight", min=0.0, default=0.1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    saturation: bpy.props.FloatProperty(name="Saturation Threshold", description="Chroma distance zeroing neighbor weight", min=0.0, default=0.05)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    value: bpy.props.FloatProperty(name="Value Threshold", description="Value distance zeroing neighbor weight", min=0.0, default=0.05)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    max_weight: bpy.props.FloatProperty(name="Max Weight", description="Largest share per neighbor frame", min=0.0, max=1 / 3, default=0.3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    tolerance: bpy.props.FloatProperty(name="Motion Tolerance", description="Allowed reverse-motion error in pixels", min=0.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------


def reconstructed(identity: Identity, scene: bpy.types.Scene, render: FollowedRender, settings: TemporalDenoise, base: Path) -> Outcome[None]:
    """Makes the temporal group of a job file's scene over the followed render's color and motion sequences, the stack's pinned head placing it.

    Each neighbor warps along its motion pass, keeps the share its reverse motion and HSV distance from the current frame allow, and mixes in by
    that share inside the rendered range.

    Returns:
        `Ok` once the group is made, absent faults for a pass the render wrote none of, or graph faults.
    """

    def grouped(written: WrittenPass, moving: WrittenPass) -> Outcome[None]:
        def loaded(sequence: WrittenPass, offset: int) -> Expr:
            frames = ImageFrames(frame_start=render.first, frame_duration=render.last - render.first + 1, frame_offset=render.first - 1 + offset)
            return Expr(
                kind=bpy.types.CompositorNodeImage,
                content=Some(Loaded(path=base / sequence.path, tag=Tag(sequence=True), frames=Some(frames))),
                socket=sequence.output,
                socket_type=bpy.types.NodeSocketColor,
            )

        def hsv(image: Expr, channel: Component) -> Expr:
            return Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", image),), members={"mode": "HSV"}, socket=channel)

        color, motion = loaded(written, 0), loaded(moving, 0)
        grid = PixelGrid.of(color)
        time = Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Frame")
        current_value = hsv(color, "Blue")
        current_chroma = hsv(color, "Green") * current_value

        def side(offset: int, toward: tuple[Component, Component], back: tuple[Component, Component], scale: Expr, inside: Expr) -> tuple[Expr, Expr]:
            uv = grid.at(xyz(*(motion[channel] for channel in toward)), scale)
            warped = uv_mapped(loaded(written, offset), uv)
            moved = uv_mapped(loaded(moving, offset), uv, interpolation="Nearest")
            agree = reduce(operator.mul, (1.0 - (abs(moved[reverse] - motion[forward]) > settings.tolerance) for reverse, forward in zip(back, toward)))
            spread = abs(hsv(color, "Red") - hsv(warped, "Red"))
            distances = (
                (math("MINIMUM", spread, 1.0 - spread), settings.hue),
                (abs(current_chroma - hsv(warped, "Green") * hsv(warped, "Blue")), settings.saturation),
                (abs(current_value - hsv(warped, "Blue")), settings.value),
            )
            alike = reduce(operator.mul, (math("DIVIDE", threshold - distance, threshold, clamp=True) for distance, threshold in distances))
            return switched(inside, 0.0, where(agree, alike, 0.0) * settings.max_weight), switched(inside, color, warped)

        previous_weight, previous = side(-1, ("Red", "Green"), ("Blue", "Alpha"), grid.inverse, time > float(render.first))
        next_weight, following = side(1, ("Blue", "Alpha"), ("Red", "Green"), -grid.inverse, time < float(render.last))
        total = previous_weight + next_weight
        restored = with_alpha(mixed(total, color, mixed(next_weight / total, previous, following)), hsv(color, "Alpha"))
        return (
            functions
            .built(OWNERSHIP.key, Interface(outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: restored}, bpy.types.CompositorNodeTree, name=Some(bpy.path.display_name(OWNERSHIP.key)))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .map(lambda _: None)
        )

    return paired(render.written(COMBINED), render.written(VECTOR_PASS.prop.name)).bind(lambda passes: grouped(*passes))


# --- [COMPOSITION] ----------------------------------------------------------------------

temporal_denoise: Final = FollowUp(
    key="temporal_denoise", form=TemporalDenoise, built=reconstructed, passes=(VECTOR_PASS,), held=Some(Snapshot.of(bpy.types.Scene, {("render", "use_motion_blur"): False}))
)
OWNERSHIP: Final = Ownership(kind=bpy.types.CompositorNodeTree, key=temporal_denoise.key, saved=Saved.USED)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["OWNERSHIP", "VECTOR_PASS", "TemporalDenoise", "reconstructed", "temporal_denoise"]
