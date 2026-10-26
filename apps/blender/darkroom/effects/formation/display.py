"""Display formation: the chain entry forming scene-linear color through a display view, the scene views it holds, and the frames it displays."""

from collections.abc import Mapping
from itertools import starmap
from typing import Annotated, Final

import bpy
from expression import Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.descriptors import assigned, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import owned
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.spaces import followed, NO_LOOK, viewed
from blender.bpy.render.compositor.effects import CompositorEffect, Holds, Named, Registers, Replaced, revised, Seeds, Shell
from blender.bpy.render.outputs.exports import formed
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.files.images import ImageFile
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired

from ...chain.stages import Effect, Stage, transform

# --- [MODELS] ---------------------------------------------------------------------------


class Formation(Effect, frozen=True, kw_only=True):
    """Scene-linear color formed through a display view, its display, view, look, exposure, and gamma the members of one Convert to Display node."""

    stage = Stage.FORMATION
    pointwise = True
    display_view: Annotated[DisplayView, msgspec.Meta(description="Viewing pipeline the entry forms through")]


class DisplayFollowing(msgspec.Struct, frozen=True, kw_only=True):
    """Display device subscription writing the scene display into every formation entry forming for another."""

    @staticmethod
    def acquisitions(identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns the scene display device subscription, writing the scene display device to every formation display node showing another and storing the view Blender matched as the entry's record."""

        def scene_followed(scene: bpy.types.Scene) -> Outcome[None]:
            held = {
                node: entry
                for entry in formation.entries(identity, scene)
                for group in Option.of_optional(entry.node_group).to_list()
                for node in Option.of_optional(owned(group, identity).get("display")).to_list()
                if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)
            }

            def stored(node: bpy.types.CompositorNodeConvertToDisplay, view: DisplayView) -> Outcome[bool]:
                return revised(formation, identity, scene, held[node], lambda record: Ok(msgspec.structs.replace(record, display_view=view)))

            return followed(scene, held).bind(lambda matched: accumulated(starmap(stored, matched))).map(lambda _: None)

        settings = bpy.types.ColorManagedDisplaySettings
        return Ok((Subscription(owner=settings, setting=settings.bl_rna.properties["display_device"], body=lambda: accumulated(map(scene_followed, bpy.data.scenes)).map(lambda _: None)),))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRAPH]


@transform.register
def _(record: Formation, _values: Mapping[Key, Value], config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns the viewing pipeline as one group transform."""
    return record.display_view.transform(config).map(lambda processor: Some(processor.createGroupTransform()))


def formed_display(shell: Shell[Formation]) -> Outcome[Expr]:
    """Builds the Convert to Display node keyed `display`, the display written before the view settings it governs."""
    view = shell.record.display_view
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeConvertToDisplay,
            inputs=(("Image", shell.color),),
            members=(
                ("display_settings.display_device", view.display),
                ("view_settings.view_transform", view.view),
                ("view_settings.look", view.looks_override if view.looks_override_enabled else NO_LOOK),
                ("view_settings.exposure", view.exposure),
                ("view_settings.gamma", view.gamma),
            ),
            socket="Image",
            shape="RGBA",
        ).named("display")
    )


# --- [VIEWS]


def formed_view(identity: Identity, scene: bpy.types.Scene) -> Outcome[Option[DisplayView]]:
    """Returns the earliest formation entry's view, Nothing without one."""
    return formation.current(identity, scene).map(lambda found: found.map(lambda record: record.display_view))


def shown_space(context: bpy.types.Context) -> Option[str]:
    """Returns the colorspace a preview-enabled Convert to Display node forms the frame into, else the scene view's."""
    return Option.of_optional(context.scene).bind(
        lambda scene: (
            next(
                (
                    viewed(node)
                    for entry in scene.compositor_effects
                    if entry.enable_for_preview
                    for group in Option.of_optional(entry.node_group).to_list()
                    for node in group.nodes
                    if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)
                ),
                viewed(scene),
            )
            .colorspace(PyOpenColorIO.GetCurrentConfig())
            .map(PyOpenColorIO.ColorSpace.getName)
        )
    )


def displayed_frame(identity: Identity, scene: bpy.types.Scene, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config) -> Outcome[ImageFile[np.float32]]:
    """Returns display values of a written frame: formed already by a formation entry and tagged with its view's colorspace, else formed through the scene view."""

    def tagged(view: DisplayView) -> Outcome[ImageFile[np.float32]]:
        return paired(
            converted(config, PyOpenColorIO.ColorSpaceTransform(src=source, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), plane),
            view.colorspace(config).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(config), member=f"{view.display}/{view.view}"),)),
        ).map(lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName()))

    return formed_view(identity, scene).bind(lambda held: held.map(tagged).default_with(lambda: formed(viewed(scene), plane, source, config)))


def looked(identity: Identity, scene: bpy.types.Scene, look: str) -> Outcome[bool]:
    """Writes a look into the earliest formation entry, else into the scene view, answering whether either changed."""
    return (
        Option
        .of_optional(next(iter(formation.entries(identity, scene)), None))
        .map(
            lambda entry: revised(
                formation,
                identity,
                scene,
                entry,
                lambda record: Ok(msgspec.structs.replace(record, display_view=msgspec.structs.replace(record.display_view, looks_override=look, looks_override_enabled=look != NO_LOOK))),
            )
        )
        .default_with(lambda: written(scene, "view_settings.look", look).map(Option.is_some))
    )


# --- [COMMANDS]


def encode_formation(call: Call[Named]) -> Outcome[Answer]:
    """Select encoding-only formation after tone curve."""

    def chosen(scene: bpy.types.Scene, config: PyOpenColorIO.Config, held: Option[DisplayView]) -> Outcome[Answer]:
        display = held.map(lambda view: view.display).default_with(lambda: viewed(scene).display)

        def selected(target: DisplayView) -> Outcome[Answer]:
            return (
                Option
                .of_optional(next(iter(formation.entries(call.identity, scene)), None))
                .map(lambda entry: revised(formation, call.identity, scene, entry, lambda record: Ok(msgspec.structs.replace(record, display_view=target))))
                .default_with(lambda: assigned(scene, {"view_settings.view_transform": target.view, "view_settings.look": NO_LOOK}).map(lambda _: True))
                .map(lambda changed: Done() if changed else Unchanged(reason=f"Formation encodes through {target.view}"))
            )

        return (
            Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], Faults]
            .bind(
                DisplayView.for_encoding(config, display),
                lambda found: found.map(lambda pair: pair[0]).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=display, member=DisplayView.for_encoding.__name__),)),
            )
            .bind(selected)
        )

    return call.root(bpy.types.Scene).bind(lambda scene: formed_view(call.identity, scene).bind(lambda held: chosen(scene, PyOpenColorIO.GetCurrentConfig(), held)))


# --- [COMPOSITION] ----------------------------------------------------------------------

encode: Final = Operator(form=Named, run=encode_formation)
formation: Final[CompositorEffect[Formation]] = CompositorEffect(
    record=Formation,
    graph=formed_display,
    mix=Replaced(),
    parts=(
        Seeds(seed=lambda scene: Formation(display_view=viewed(scene))),
        Holds(
            scene=Snapshot.of(
                bpy.types.Scene,
                {
                    "view_settings.view_transform": "Raw",
                    "view_settings.look": NO_LOOK,
                    "view_settings.exposure": 0.0,
                    "view_settings.gamma": 1.0,
                    "view_settings.use_white_balance": False,
                    "view_settings.use_curve_mapping": False,
                },
            )
        ),
        Registers(rows=(encode, DisplayFollowing())),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DisplayFollowing", "Formation", "displayed_frame", "encode", "formation", "formed_view", "looked", "shown_space"]
