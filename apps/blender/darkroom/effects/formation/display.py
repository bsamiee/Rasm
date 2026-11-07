"""Display formation: the chain entry forming scene-linear color through a display view, the scene views it holds, and the frames it displays, and BT.2390 peak mapping of formed absolute PQ codes into a display's peak and black."""

from collections.abc import Callable
from functools import partial
from itertools import starmap
from typing import Final, override

import bpy
from expression import Option, Some, try_downcast
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Subscription
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interfaces import Interface, Socket
from blender.bpy.render.color.spaces import ColorManagement, followed, HdrWindow, NO_LOOK, viewed
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Mix, Named, Seeded
from blender.bpy.render.compositor.inputs import entered, GraphInputs, Holds
from blender.bpy.render.compositor.viewers import READ
from blender.imaging.analysis.metering import Samples
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.files.images import ImageFile
from blender.records.faults import accumulated, Fault, Ok, Outcome, paired, Reason

from ..grade.exposure import exposed

# --- [CONSTANTS] ------------------------------------------------------------------------

_DISPLAY: Final[Key] = "display"
_SOURCE_PEAK: Final = Socket(key="source_peak", kind=bpy.types.NodeSocketFloat, description="Brightest frame luminance", min_value=0.0, max_value=10000.0, default_value=10000.0)
_TARGET_PEAK: Final = Socket(key="target_peak", kind=bpy.types.NodeSocketFloat, description="Display peak luminance", min_value=0.0, max_value=10000.0, default_value=1000.0)
_DISPLAY_BLACK: Final = Socket(key="display_black", kind=bpy.types.NodeSocketFloatFactor, description="Share of fitted peak luminance, inverse of display contrast ratio")
_KNEE_OFFSET: Final = Socket(key="knee_offset", kind=bpy.types.NodeSocketFloat, description="Offset from fitted peak down to knee, 0.5 in BT.2390", min_value=0.5, max_value=2.0, default_value=0.5)

# --- [MODELS] ---------------------------------------------------------------------------


class Formation(CompositorRecord, frozen=True, kw_only=True):
    """Scene-linear color formed through a display view, its display, view, look, exposure, and gamma the members of one Convert to Display node."""

    display_view: DisplayView

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Builds the Convert to Display node keyed `display` between the view's exposure ahead and its display gamma after, the node holding the display, view, and look Blender lets it set."""
        view, viewing = self.display_view, ColorManagement.of(self.display_view)
        members = msgspec.structs.replace(viewing, view_settings=msgspec.structs.replace(viewing.view_settings, exposure=msgspec.UNSET, gamma=msgspec.UNSET)).members()
        entering = exposed(shell.color, view.exposure) if view.exposure else shell.color
        formed = Expr(kind=bpy.types.CompositorNodeConvertToDisplay, inputs=(("Image", entering),), members=members, socket="Image", socket_type=bpy.types.NodeSocketColor).keyed(_DISPLAY)
        return Ok(
            Expr(kind=bpy.types.CompositorNodeGamma, inputs=(("Image", formed), ("Gamma", view.inverse_gamma)), socket="Image", socket_type=bpy.types.NodeSocketColor) if view.gamma != 1.0 else formed
        )


class PeakMapping(CompositorRecord, frozen=True, kw_only=True):
    """Per-channel PQ peak compression with Hermite roll-off and black lift."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Maps formed PQ codes through the BT.2390 EETF over live peak, black, and knee inputs."""
        return Ok(grades.eetf(shell.color, source=shell.input(_SOURCE_PEAK), target=shell.input(_TARGET_PEAK), black=shell.input(_DISPLAY_BLACK), knee=shell.input(_KNEE_OFFSET)))


class DisplayFollowing(msgspec.Struct, frozen=True, kw_only=True):
    """Display device subscription writing the scene display into every formation entry forming for another."""

    @staticmethod
    def acquisitions(identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns the scene display device subscription, writing the scene display device to every formation display node showing another and storing the view Blender matched as the entry's record."""

        def scene_followed(scene: bpy.types.Scene) -> Outcome[None]:
            held = {node: entry for entry, node in displays(identity, scene)}

            def stored(node: bpy.types.CompositorNodeConvertToDisplay, view: DisplayView) -> Outcome[bool]:
                return formation.revised(
                    identity,
                    scene,
                    held[node],
                    lambda record: Ok(msgspec.structs.replace(record, display_view=msgspec.structs.replace(view, exposure=record.display_view.exposure, gamma=record.display_view.gamma))),
                    form=Formation,
                )

            return followed(scene, held).bind(lambda matched: accumulated(starmap(stored, matched))).map(lambda _: None)

        settings = bpy.types.ColorManagedDisplaySettings
        return Ok((Subscription(owner=settings, setting=settings.bl_rna.properties["display_device"], body=lambda: accumulated(map(scene_followed, bpy.data.scenes)).map(lambda _: None)),))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [VIEWS]


def _unformed(scene: bpy.types.Scene) -> Outcome[Snapshot]:
    """Scene view settings passing formed values through: the scene display's data view with no look, exposure, gamma, white balance, or curve."""
    display = viewed(scene).display
    return (
        DisplayView
        .data(PyOpenColorIO.GetCurrentConfig(), display)
        .to_result_with(partial(Fault.of, Reason.ABSENT, display, member="data view"))
        .map(
            lambda view: Snapshot.of(
                bpy.types.Scene,
                {
                    ("view_settings", "view_transform"): view.view,
                    ("view_settings", "look"): NO_LOOK,
                    ("view_settings", "exposure"): 0.0,
                    ("view_settings", "gamma"): 1.0,
                    ("view_settings", "use_white_balance"): False,
                    ("view_settings", "use_curve_mapping"): False,
                },
            )
        )
    )


def displays(identity: Identity, scene: bpy.types.Scene) -> tuple[tuple[bpy.types.SceneCompositorEffect, bpy.types.CompositorNodeConvertToDisplay], ...]:
    """Returns the scene's formation entries in stack order, each with the Convert to Display node its group owns."""
    return tuple(
        (entry, node)
        for entry in formation.entries(identity, scene)
        for group in Option.of_optional(entry.node_group).to_list()
        for node in Option.of_optional(owned(group, identity).get(_DISPLAY)).bind(lambda found: Option.of_optional(try_downcast(bpy.types.CompositorNodeConvertToDisplay, found))).to_list()
    )


def formed_view(identity: Identity, scene: bpy.types.Scene) -> Outcome[Option[DisplayView]]:
    """Returns the earliest formation entry's view, Nothing without one."""
    return formation.current(identity, scene, form=Formation).map(lambda found: found.map(lambda record: record.display_view))


def current_view(identity: Identity, scene: bpy.types.Scene) -> Outcome[DisplayView]:
    """Returns the earliest formation entry's view, else the scene's."""
    return formed_view(identity, scene).map(lambda found: found.default_with(lambda: viewed(scene)))


def encoding(identity: Identity, scene: bpy.types.Scene, config: PyOpenColorIO.Config) -> Outcome[tuple[DisplayView, PyOpenColorIO.Processor]]:
    """Returns the encoding view of the current view's display with its processor."""

    def found(view: DisplayView) -> Outcome[tuple[DisplayView, PyOpenColorIO.Processor]]:
        return DisplayView.for_encoding(config, view.display).bind(lambda held: held.to_result_with(partial(Fault.of, Reason.ABSENT, view.display, member="encoding view")))

    return current_view(identity, scene).bind(found)


def displayed_frame(identity: Identity, scene: bpy.types.Scene, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config) -> Outcome[ImageFile]:
    """Returns display values of a written frame: a formation entry's frame tagged with its view's colorspace, else formed through the scene view."""

    def tagged(view: DisplayView) -> Outcome[ImageFile]:
        return paired(
            converted(config, PyOpenColorIO.ColorSpaceTransform(src=source, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), plane),
            view.colorspace(config).to_result_with(partial(Fault.of, Reason.ABSENT, repr(config), member=f"{view.display}/{view.view}")),
        ).map(lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName()))

    return formed_view(identity, scene).bind(lambda held: held.map(tagged).default_with(lambda: viewed(scene).image(config, plane, source)))


def _reformed(identity: Identity, scene: bpy.types.Scene, change: Callable[[DisplayView], DisplayView], unformed: Callable[[], Outcome[bool]]) -> Outcome[bool]:
    """Writes a changed view into the earliest formation entry, else the scene view settings, answering whether either changed."""
    return (
        formation
        .earliest(identity, scene)
        .map(lambda entry: formation.revised(identity, scene, entry, lambda record: Ok(msgspec.structs.replace(record, display_view=change(record.display_view))), form=Formation))
        .default_with(unformed)
    )


def looked(identity: Identity, scene: bpy.types.Scene, look: str) -> Outcome[bool]:
    """Writes a look into the earliest formation entry, else into the scene view, answering whether either changed."""
    return _reformed(
        identity,
        scene,
        lambda view: msgspec.structs.replace(view, looks_override=look, looks_override_enabled=look != NO_LOOK),
        lambda: written(scene, ("view_settings", "look"), look).map(Option.is_some),
    )


# --- [COMMANDS]


def measure_peak(identity: Identity, _scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[bool]:
    """Measure source peak from stage input."""
    return (
        Samples
        .of(PyOpenColorIO.GetCurrentConfig(), file.pixels)
        .map(Samples.peak)
        .bind(lambda peak: peak.map(lambda value: entered(identity, entry, {_SOURCE_PEAK.key: value})).default_value(Ok(value=False)))
    )


def encode_formation(call: Call[Named]) -> Outcome[Answer]:
    """Select encoding-only formation after tone curve."""

    def selected(scene: bpy.types.Scene, target: DisplayView) -> Outcome[Answer]:
        return _reformed(
            call.identity, scene, lambda _view: target, lambda: assigned(scene, {("view_settings", "view_transform"): target.view, ("view_settings", "look"): NO_LOOK}).map(lambda _: True)
        ).map(lambda changed: Done() if changed else Unchanged(reason=f"Formation encodes through {target.view}"))

    return rooted(bpy.types.Scene, call.context).bind(lambda scene: encoding(call.identity, scene, PyOpenColorIO.GetCurrentConfig()).bind(lambda found: selected(scene, found[0])))


# --- [COMPOSITION] ----------------------------------------------------------------------

formation: Final = CompositorEffect(
    record=Formation,
    mix=Mix.REPLACE,
    pointwise=True,
    source=Seeded(seed=lambda scene: Formation(display_view=viewed(scene))),
    holds=Holds(scene=_unformed),
    registers=(Operator(form=Named, run=encode_formation), DisplayFollowing()),
)
peak_mapping: Final = CompositorEffect(
    record=PeakMapping,
    interface=Interface(inputs=(_SOURCE_PEAK, _TARGET_PEAK, _DISPLAY_BLACK, _KNEE_OFFSET)),
    pointwise=True,
    registers=(READ.measuring(measure_peak, condition=Some(HdrWindow(absolute=True))),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DisplayFollowing", "Formation", "PeakMapping", "current_view", "displayed_frame", "displays", "encoding", "formation", "formed_view", "looked", "peak_mapping"]
