"""Display formation: the chain entry forming scene-linear color through a display view, the scene views it holds and restores, and the frames it displays."""

from itertools import starmap
from typing import Annotated, Final, override

import bpy
from expression import Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.data.rna.descriptors import assigned, written
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import owned
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.spaces import config_of, displayed, followed, NO_LOOK, viewed
from blender.bpy.render.compositor.effects import CompositorEffect, editable, Holds, Named, Registers, Replaced, Restoring, revised, Seeds, Shell
from blender.bpy.render.outputs.exports import formed
from blender.bpy.render.outputs.formats import owners
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import ImageFile
from blender.records.faults import accumulated, paired
from blender.records.fields import Gated

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

_HELD: Final = {
    "view_settings.view_transform": "Raw",
    "view_settings.look": NO_LOOK,
    "view_settings.exposure": 0.0,
    "view_settings.gamma": 1.0,
    "view_settings.use_white_balance": False,
    "view_settings.use_curve_mapping": False,
}

# --- [MODELS] ---------------------------------------------------------------------------


class Formation(Effect, frozen=True, kw_only=True):
    """Scene-linear color formed through a display view, its display, view, look, exposure, and gamma held by one Convert to Display node."""

    stage = Stage.FORMATION
    pointwise = True
    display_view: Annotated[DisplayView, msgspec.Meta(description="Viewing pipeline the entry forms through"), GraphForm.SETTING]

    @override
    def transform(self, config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the viewing pipeline as one group transform."""
        return self.display_view.transform(config).map(lambda processor: Some(processor.createGroupTransform()))


class DisplayFollowing(msgspec.Struct, frozen=True, kw_only=True):
    """Display device subscription writing the scene display into every formation entry forming for another."""

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Returns the scene display device subscription."""
        settings = bpy.types.ColorManagedDisplaySettings
        return Ok((Subscription(owner=settings, setting=settings.bl_rna.properties["display_device"], body=lambda: following(identity)),))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRAPH]


def formed_display(shell: Shell[Formation]) -> BlenderResult[Expr]:
    """Builds the Convert to Display node keyed `display`, the display written before the view settings it governs."""
    view = shell.record.display_view
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeConvertToDisplay,
            inputs=(("Image", shell.color),),
            members=(
                ("display_settings.display_device", view.display),
                ("view_settings.view_transform", view.view),
                ("view_settings.look", view.look.value if view.look.enabled else NO_LOOK),
                ("view_settings.exposure", view.exposure),
                ("view_settings.gamma", view.gamma),
            ),
            socket="Image",
            shape="RGBA",
        ).named("display")
    )


# --- [VIEWS]


def formed_view(identity: Identity, scene: bpy.types.Scene) -> BlenderResult[Option[DisplayView]]:
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


def displayed_frame(identity: Identity, scene: bpy.types.Scene, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
    """Returns display values of a written frame: formed already by a formation entry and tagged with its view's colorspace, else formed through the scene view."""

    def tagged(view: DisplayView) -> BlenderResult[ImageFile[np.float32]]:
        return paired(
            converted(config, PyOpenColorIO.ColorSpaceTransform(src=source, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), plane),
            view.colorspace(config).to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"{view.display}/{view.view}"),)),
        ).map(lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName()))

    return formed_view(identity, scene).bind(lambda held: held.map(tagged).default_with(lambda: formed(viewed(scene), plane, source, config)))


def looked(identity: Identity, scene: bpy.types.Scene, look: str) -> BlenderResult[bool]:
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
                lambda record: Ok(msgspec.structs.replace(record, display_view=msgspec.structs.replace(record.display_view, look=Gated(enabled=look != NO_LOOK, value=look)))),
            )
        )
        .default_with(lambda: written(scene, "view_settings.look", look).map(Option.is_some))
    )


def following(identity: Identity) -> BlenderResult[None]:
    """Writes the scene display device to every formation display node showing another, storing the view Blender matched as the entry's record."""

    def scene_followed(scene: bpy.types.Scene) -> BlenderResult[None]:
        held = {
            node: entry
            for entry in formation.entries(identity, scene)
            for group in Option.of_optional(entry.node_group).to_list()
            for node in Option.of_optional(owned(group, identity).get("display")).to_list()
            if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)
        }

        def stored(node: bpy.types.CompositorNodeConvertToDisplay, view: DisplayView) -> BlenderResult[bool]:
            return revised(formation, identity, scene, held[node], lambda record: Ok(msgspec.structs.replace(record, display_view=view)))

        return followed(scene, held).bind(lambda matched: accumulated(starmap(stored, matched))).map(lambda _: None)

    return accumulated(scene_followed(scene) for scene in bpy.data.scenes if editable(scene).is_ok()).map(lambda _: None)


# --- [HOLDS]


def overridden(_identity: Identity, scene: bpy.types.Scene) -> BlenderResult[None]:
    """Writes the held view settings to every output overriding color management."""
    return accumulated(assigned(owner, _HELD) for owner in owners(scene) if owner.color_management == "OVERRIDE").map(lambda _: None)


def restoring(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[Restoring]:
    """Reads the entry's view, written to the scene and overriding outputs once the entry and its holds are gone."""
    return formation.read(identity, entry).map(lambda record: lambda: displayed(scene, record.record.display_view))


# --- [COMMANDS]


def encoded(call: Call[Named]) -> BlenderResult[Outcome]:
    """Selects the first encoding-only view of the formation display without a look."""

    def chosen(scene: bpy.types.Scene, config: PyOpenColorIO.Config, held: Option[DisplayView]) -> BlenderResult[Outcome]:
        display = held.map(lambda view: view.display).default_with(lambda: viewed(scene).display)

        def selected(target: DisplayView) -> BlenderResult[Outcome]:
            return (
                Option
                .of_optional(next(iter(formation.entries(call.identity, scene)), None))
                .map(lambda entry: revised(formation, call.identity, scene, entry, lambda record: Ok(msgspec.structs.replace(record, display_view=target))))
                .default_with(lambda: assigned(scene, {"view_settings.view_transform": target.view, "view_settings.look": NO_LOOK}).map(lambda _: True))
                .map(lambda changed: Done() if changed else Unchanged(reason=f"Formation encodes through {target.view}"))
            )

        return (
            Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults]
            .bind(
                DisplayView.for_encoding(config, display),
                lambda found: found.map(lambda pair: pair[0]).to_result_with(lambda: (BlenderAbsentFault(owner=display, key=DisplayView.for_encoding.__name__),)),
            )
            .bind(selected)
        )

    return call.root(bpy.types.Scene).bind(lambda scene: paired(config_of(), formed_view(call.identity, scene)).bind(lambda pair: chosen(scene, *pair)))


# --- [COMPOSITION] ----------------------------------------------------------------------

encode = Operator(key="encode_formation", title="Use Display Encoding", description="Select encoding-only formation after tone curve", form=Named, run=encoded)
formation: CompositorEffect[Formation] = CompositorEffect(
    record=Formation,
    graph=formed_display,
    mix=Replaced(),
    parts=(Seeds(seed=lambda scene: Formation(display_view=viewed(scene))), Holds(scene=_HELD, held=overridden, restored=restoring), Registers(rows=(encode, DisplayFollowing()))),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DisplayFollowing", "Formation", "displayed_frame", "encode", "formation", "formed_view", "looked", "shown_space"]
