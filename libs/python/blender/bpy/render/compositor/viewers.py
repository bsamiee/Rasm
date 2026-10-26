"""Owned Viewer nodes of offered and pinned entries, and entering-frame reads delivered to a caller's consumer."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, nullcontext
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.ids.images import read
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.events.handlers import SceneHandler
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import Added, applied, Connected, Disconnected, Keyed, owned, Removed
from blender.bpy.nodes.graphs import End, INPUTS, Link, OUTPUTS
from blender.bpy.nodes.interface import slot
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.compositor.effects import CompositorEffect, Marks, Named, Shell
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.outputs.results import built, INTERMEDIATES
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Region
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Consume = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, ImageFile[np.float32]], Outcome[Option[str]]]

# --- [MODELS] ---------------------------------------------------------------------------


class ViewTarget(bpy.types.PropertyGroup):
    """Entry and group output to inspect."""

    name: bpy.props.StringProperty(name="Entry", description="Compositor entry name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    node: bpy.props.StringProperty(name="Node", description="Group node key holding the output", default=INPUTS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    socket: bpy.props.StringProperty(name="Socket", description="Output socket key", default=Shell.image)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Arm(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Pending read of one entry: its crop, its consumer, and whether a completed frame was captured for it."""

    name: str
    region: Option[Region]
    consume: Consume
    captured: bool = False


# --- [SERVICES] -------------------------------------------------------------------------


class ViewerRead(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Entering-frame reads delivered after compositor completion, one pending read per scene, forgotten at file load."""

    pending: Scoped[dict[Handle[bpy.types.Scene], Arm]] = msgspec.field(default_factory=lambda: Scoped(made=lambda _identity: nullcontext({}), renewed=Some(lambda _held: {})))

    def entering(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, consume: Consume, region: Option[Region] = Nothing) -> Outcome[None]:
        """Views entry input, replacing the scene's pending read."""
        return shown(identity, entry, End(node=INPUTS, socket=Shell.image)).map(lambda _: self.pending.of(identity).update({Handle.of(scene): Arm(name=entry.name, region=region, consume=consume)}))

    def delivered(self, identity: Identity, handle: Handle[bpy.types.Scene], arm: Arm, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
        """Consumes captured pixels while the capture is the scene's pending read."""
        if (held := self.pending.of(identity)).get(handle) is not arm:
            return Ok(Nothing)
        del held[handle]
        cropped = arm.region.map(lambda region: msgspec.structs.replace(file, pixels=file.pixels[region.window(*file.pixels.shape[:2])])).default_value(file)
        return handle.resolve().bind(
            lambda scene: (
                Option
                .of_optional(scene.compositor_effects.get(arm.name))
                .map(lambda entry: cleared(identity, entry).bind(lambda _: arm.consume(identity, scene, entry, cropped)))
                .default_value(Ok(Nothing))
            )
        )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields the pending cell and the completion handler capturing the compositor result for deferred delivery."""
        with identity.reports.deferred(lambda handle, arm, file: self.delivered(identity, handle, arm, file)) as deferred:

            def completed(scene: bpy.types.Scene) -> Outcome[None]:
                if scene.session_uid in INTERMEDIATES.of(identity):
                    return Ok(None)
                handle, held = Handle.of(scene), self.pending.of(identity)

                def queued(arm: Arm, file: ImageFile[np.float32]) -> None:
                    taken = held[handle] = msgspec.structs.replace(arm, captured=True)
                    deferred(handle, taken, file)

                return (
                    Option
                    .of_optional(held.get(handle))
                    .filter(lambda arm: not arm.captured and Option.of_optional(scene.compositor_effects.get(arm.name)).filter(lambda entry: entry.is_active).is_some())
                    .map(lambda arm: built("COMPOSITING").bind(lambda image: read(image, PyOpenColorIO.GetCurrentConfig())).map(lambda file: queued(arm, file)))
                    .default_value(Ok(None))
                )

            yield Ok((self.pending, INTERMEDIATES, SceneHandler(handlers=bpy.app.handlers.composite_post, body=completed)))


class Viewers[R: Case]:
    """View and clear commands over the owned Viewer of offered and pinned entries."""

    rows: EffectRows[R]
    viewing: Operator[ViewTarget]
    clearing: Operator[Named]
    key: ClassVar[Key] = Shell.viewer
    entering: ClassVar[End] = End(node=INPUTS, socket=Shell.image)
    leaving: ClassVar[End] = End(node=OUTPUTS, socket=Shell.result)

    def entry(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[tuple[bpy.types.SceneCompositorEffect, Option[CompositorEffect[R]]]]:
        """Finds named offered or pinned entry with its offered effect, skipping user groups."""
        pinned = frozenset(row.ownership.key for row in self.rows.pinned)
        return Option.of_optional(
            next(
                (
                    (entry, Option.of_optional(self.rows.keyed.get(mark.key)))
                    for entry, mark in Marks.of(identity, scene).held
                    if entry.name == name and (mark.key in self.rows.keyed or mark.key in pinned)
                ),
                None,
            )
        ).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene.compositor_effects), member=name),))

    def __init__(self, *, rows: EffectRows[R]) -> None:
        """Declares the view and clear commands."""
        self.rows = rows

        def view_output(call: Call[ViewTarget]) -> Outcome[Answer]:
            """Show effect group output in place of stack result."""
            query = AreaQuery(space="NODE_EDITOR", ui_type=Some(bpy.types.CompositorNodeTree.__name__))
            return paired(
                EditorState.of(bpy.types.SpaceNodeEditor, {"node_tree_sub_type": "SCENE", "show_backdrop": True}),
                call.root(bpy.types.Scene).bind(lambda scene: self.entry(call.identity, scene, call.properties.name)),
            ).bind(
                lambda pair: shown(call.identity, pair[1][0], End(node=call.properties.node, socket=call.properties.socket)).bind(
                    lambda _: call.root(bpy.types.WindowManager).bind(
                        lambda manager: (
                            Option
                            .of_optional(next(iter(query.located(manager.windows)), None))
                            .map(lambda at: pair[0].applied(at.area).map(lambda _: Done()))
                            .default_value(Ok(Done(notes=(("INFO", "No compositor Node Editor open"),))))
                        )
                    )
                )
            )

        def clear_viewer(call: Call[Named]) -> Outcome[Answer]:
            """Remove effect Viewer to show stack result."""
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.entry(call.identity, scene, call.properties.name))
                .bind(lambda held: cleared(call.identity, held[0]).map(lambda removed: Done() if removed else Unchanged(reason=f"{held[0].name} has no Viewer")))
            )

        self.viewing = Operator(form=ViewTarget, run=view_output, internal=True)
        self.clearing = Operator(form=Named, run=clear_viewer, internal=True)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the entering-frame read and the view and clear commands."""
        return Ok((READ, self.viewing, self.clearing))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [VIEWER]


def shown(identity: Identity, entry: bpy.types.SceneCompositorEffect, target: End) -> Outcome[None]:
    """Connects owned Viewer to target end and activates entry."""

    def viewed(tree: bpy.types.NodeTree, name: str) -> Outcome[None]:
        def source(keyed: Keyed, key: Key) -> Outcome[End]:
            keys = {node: key for key, node in keyed.nodes.items()}
            return (
                Option
                .of_optional(keyed.nodes.get(OUTPUTS))
                .bind(lambda output: Option.of_optional(keyed.sockets[OUTPUTS].get(key)).bind(lambda identifier: socket_at(output.inputs, identifier).to_option()))
                .bind(
                    lambda socket: Option.of_optional(
                        next((End(node=keys[link.from_node], socket=link.from_socket.identifier) for link in socket.links if link.from_node in keys), None)  # type: ignore[union-attr]  # ty: ignore[not-iterable, unresolved-attribute]
                    )
                )
                .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(tree.links), member=key),))
            )

        def connected(end: End) -> Outcome[None]:
            stale = tuple(link for viewer in marked(identity, tree).to_list() for link in tree.links if link.to_node == viewer)
            return accumulated(applied(Disconnected(link=link), identity, Keyed.of(tree, identity, name)) for link in stale).bind(
                lambda _: applied(Connected(link=Link(source=end, target=End(node=Shell.viewer, socket="Image"))), identity, Keyed.of(tree, identity, name))
            )

        match target:
            case End(node=node, socket=str(key)) if node == OUTPUTS:
                entering = source(Keyed.of(tree, identity, name), key)
            case _:
                entering = Ok(target)
        return (
            entering
            .bind(
                lambda end: (Ok(None) if marked(identity, tree).is_some() else applied(Added(key=Shell.viewer, kind=bpy.types.CompositorNodeViewer), identity, Keyed.of(tree, identity, name))).bind(
                    lambda _: connected(end)
                )
            )
            .bind(lambda _: written(entry, "is_active", True))
            .bind(lambda _: marked(identity, tree).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(tree.nodes), member=Shell.viewer),)))
            .bind(lambda viewer: written(tree.nodes, "active", viewer).map(lambda _: None))
        )

    return grouped(identity, entry).bind(lambda pair: viewed(*pair))


def cleared(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
    """Removes owned Viewer, answering whether it existed."""

    def removed(tree: bpy.types.NodeTree, name: str) -> Outcome[bool]:
        return marked(identity, tree).map(lambda viewer: applied(Removed(name=viewer.name), identity, Keyed.of(tree, identity, name)).map(lambda _: True)).default_value(Ok(False))

    return grouped(identity, entry).bind(lambda pair: removed(*pair))


def grouped(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[bpy.types.NodeTree, str]]:
    """Returns entry group and interface slot name."""
    tree: Outcome[bpy.types.NodeTree] = Option.of_optional(entry.node_group).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(entry), member="node_group"),))
    return Result[bpy.types.NodeTree, Faults].bind(tree, lambda tree: slot(identity).map(lambda name: (tree, name)))


def marked(identity: Identity, tree: bpy.types.NodeTree) -> Option[bpy.types.Node]:
    """Returns Viewer node that extension marks as owned."""
    return Option.of_optional(owned(tree, identity).get(Shell.viewer))


# --- [COMPOSITION] ----------------------------------------------------------------------

READ: Final = ViewerRead()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["READ", "Arm", "Consume", "ViewTarget", "ViewerRead", "Viewers", "cleared", "shown"]
