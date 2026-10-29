"""Owned Viewer nodes of offered and pinned entries, entering-frame reads delivered to a caller's consumer, and scoped pass-through captures of an entry's input."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack, nullcontext
from functools import cached_property, partial
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.ids.images import read
from blender.bpy.data.ids.ownership import Mark, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes import functions
from blender.bpy.nodes.addressing import Keyed, owned, slot
from blender.bpy.nodes.changes import Added, Connected, Disconnected, made, Removed
from blender.bpy.nodes.desired import End, INPUTS, Link, OUTPUTS
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.nodes.interface import Interface
from blender.bpy.nodes.trees import links_of, socket_at
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.edits import bound
from blender.bpy.render.compositor.effects import CompositorEffect, Named, VIEWER
from blender.bpy.render.compositor.entries import EffectRows, relocated
from blender.bpy.render.compositor.inputs import grouped
from blender.bpy.render.outputs.results import built, INTERMEDIATES
from blender.bpy.ui.screens.areas import AreaQuery, EditorState
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Region
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Consume = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, ImageFile[np.float32]], Outcome[Option[str]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ENTERING: Final = End(node=INPUTS, socket=COLOR_INPUT.key)

# --- [MODELS] ---------------------------------------------------------------------------


class ViewTarget(Named):
    """Entry and group output to inspect."""

    node: bpy.props.StringProperty(name="Node", description="Group node key holding the output", default=INPUTS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    socket: bpy.props.StringProperty(name="Socket", description="Output socket key", default=COLOR_INPUT.key)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Arm(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Pending read of one entry: its crop, its consumer, and whether a completed frame was captured for it."""

    name: str
    region: Option[Region]
    consume: Consume
    captured: bool = False


# --- [SERVICES] -------------------------------------------------------------------------


class ViewerRead(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Entering-frame reads delivered after compositor completion, one pending read per scene, forgotten at file load."""

    pending: Resource[dict[Handle[bpy.types.Scene], Arm]] = msgspec.field(default_factory=lambda: Resource(made=lambda _identity: nullcontext({}), renewed=Some(lambda _held: {})))

    def entering(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, consume: Consume, region: Option[Region] = Nothing) -> Outcome[None]:
        """Views entry input, replacing the scene's pending read."""
        return shown(identity, entry, _ENTERING).map(lambda _: identity.held[self.pending].update({Handle.of(scene): Arm(name=entry.name, region=region, consume=consume)}))

    def delivered(self, identity: Identity, handle: Handle[bpy.types.Scene], arm: Arm, file: ImageFile[np.float32]) -> Outcome[Option[str]]:
        """Consumes captured pixels while the capture is the scene's pending read."""
        if (held := identity.held[self.pending]).get(handle) is not arm:
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
                if scene.session_uid in identity.held[INTERMEDIATES]:
                    return Ok(None)
                handle, held = Handle.of(scene), identity.held[self.pending]

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

            yield Ok((self.pending, INTERMEDIATES, Handler(handlers=bpy.app.handlers.composite_post, body=completed)))


class Viewers[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """View and clear commands over the owned Viewer of offered and pinned entries."""

    rows: EffectRows[R]
    entering: ClassVar[End] = _ENTERING
    leaving: ClassVar[End] = End(node=OUTPUTS, socket=COLOR_RESULT.key)

    def entry(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[tuple[bpy.types.SceneCompositorEffect, Option[CompositorEffect[R]]]]:
        """Finds named offered or pinned entry with its offered effect, skipping user groups."""
        keys = frozenset((*self.rows.keyed, *(row.ownership.key for row in self.rows.pinned)))
        return (
            Option
            .of_optional(scene.compositor_effects.get(name))
            .bind(lambda entry: Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).filter(lambda mark: mark.key in keys).map(lambda mark: (entry, mark)))
            .map(lambda pair: (pair[0], Option.of_optional(self.rows.keyed.get(pair[1].key))))
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    @cached_property
    def viewing(self) -> Operator[ViewTarget]:
        """Command showing an entry's group output."""
        return Operator(form=ViewTarget, run=bound(view_output, self), internal=True)

    @cached_property
    def clearing(self) -> Operator[Named]:
        """Command removing an entry's owned Viewer."""
        return Operator(form=Named, run=bound(clear_viewer, self), internal=True)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the entering-frame read and the view and clear commands."""
        return Ok((READ, self.viewing, self.clearing))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COMMANDS]


def view_output[R: Case](viewers: Viewers[R], call: Call[ViewTarget]) -> Outcome[Answer]:
    """Show effect group output in place of stack result."""
    query = AreaQuery(space=bpy.types.SpaceNodeEditor, ui_type=Some(bpy.types.CompositorNodeTree.__name__))
    return paired(
        EditorState.of(bpy.types.SpaceNodeEditor, {("node_tree_sub_type",): "SCENE", ("show_backdrop",): True}),
        rooted(bpy.types.Scene, call.context).bind(lambda scene: viewers.entry(call.identity, scene, call.properties.name)),
    ).bind(
        lambda pair: shown(call.identity, pair[1][0], End(node=call.properties.node, socket=call.properties.socket)).bind(
            lambda _: rooted(bpy.types.WindowManager, call.context).bind(
                lambda manager: (
                    Block
                    .of_seq(query.located(manager.windows))
                    .try_head()
                    .map(lambda at: pair[0].applied(at.area).map(lambda _: Done()))
                    .default_value(Ok(Done(notes=(("INFO", "No compositor Node Editor open"),))))
                )
            )
        )
    )


def clear_viewer[R: Case](viewers: Viewers[R], call: Call[Named]) -> Outcome[Answer]:
    """Remove effect Viewer to show stack result."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: viewers.entry(call.identity, scene, call.properties.name))
        .bind(lambda held: cleared(call.identity, held[0]).map(lambda removed: Done() if removed else Unchanged(reason=f"{held[0].name} has no Viewer")))
    )


# --- [VIEWER]


def shown(identity: Identity, entry: bpy.types.SceneCompositorEffect, target: End) -> Outcome[None]:
    """Connects owned Viewer to target end and activates entry."""

    def viewed(tree: bpy.types.NodeTree, name: str) -> Outcome[None]:
        def source(keyed: Keyed, key: Key) -> Outcome[End]:
            keys = {node: key for key, node in keyed.nodes.items()}

            def ended(link: bpy.types.NodeLink) -> Option[End]:
                match link.from_node, link.from_socket:
                    case bpy.types.Node() as node, bpy.types.NodeSocket(identifier=identifier) if node in keys:
                        return Some(End(node=keys[node], socket=identifier))
                    case _:
                        return Nothing

            def linked(socket: bpy.types.NodeSocket) -> Option[End]:
                links = links_of(socket)
                return Block.of_seq(links).choose(ended).try_head()

            return (
                Option
                .of_optional(keyed.nodes.get(OUTPUTS))
                .bind(lambda output: Option.of_optional(keyed.sockets[OUTPUTS].get(key)).bind(lambda identifier: socket_at(output.inputs, identifier).to_option()))
                .bind(linked)
                .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tree.links), member=key))
            )

        def connected(end: End) -> Outcome[None]:
            stale = tuple(link for viewer in _viewer(identity, tree).to_list() for link in tree.links if link.to_node == viewer)
            return accumulated(Disconnected(link=link).applied(Keyed.of(tree, identity, name), identity) for link in stale).bind(
                lambda _: Connected(link=Link(source=end, target=End(node=VIEWER, socket="Image"))).applied(Keyed.of(tree, identity, name), identity)
            )

        match target:
            case End(node=node, socket=str(key)) if node == OUTPUTS:
                entering = source(Keyed.of(tree, identity, name), key)
            case _:
                entering = Ok(target)
        return (
            entering
            .bind(
                lambda end: (Ok(None) if _viewer(identity, tree).is_some() else Added(key=VIEWER, kind=bpy.types.CompositorNodeViewer).applied(Keyed.of(tree, identity, name), identity)).bind(
                    lambda _: connected(end)
                )
            )
            .bind(lambda _: written(entry, ("is_active",), True))
            .bind(lambda _: _viewer(identity, tree).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tree.nodes), member=VIEWER)))
            .bind(lambda viewer: written(tree.nodes, ("active",), viewer).map(lambda _: None))
        )

    return _slotted(identity, entry).bind(lambda pair: viewed(*pair))


def cleared(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
    """Removes owned Viewer, answering whether it existed."""

    def removed(tree: bpy.types.NodeTree, name: str) -> Outcome[bool]:
        return _viewer(identity, tree).map(lambda viewer: Removed(name=viewer.name).applied(Keyed.of(tree, identity, name), identity).map(lambda _: True)).default_value(Ok(False))

    return _slotted(identity, entry).bind(lambda pair: removed(*pair))


def _slotted(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[bpy.types.NodeTree, str]]:
    """Returns entry group and interface slot name."""
    return Result[bpy.types.NodeTree, Faults].bind(grouped(entry), lambda tree: slot(identity).map(lambda name: (tree, name)))


def _viewer(identity: Identity, tree: bpy.types.NodeTree) -> Option[bpy.types.Node]:
    """Returns Viewer node that extension marks as owned."""
    return Option.of_optional(owned(tree, identity).get(VIEWER))


# --- [CAPTURE]


@contextmanager
def capturing(
    identity: Identity, scene: bpy.types.Scene, before: bpy.types.SceneCompositorEffect, output: tuple[Expr, ...], *, stop: bool = False
) -> Generator[Outcome[bpy.types.SceneCompositorEffect]]:
    """Places scoped pass-through capture entry before the scene's entry, holding the output roots beside its pass-through, the active entry kept active."""
    at = tuple(scene.compositor_effects).index(before)
    face = Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,))
    with ExitStack() as scope:
        for active in Option.of_optional(scene.compositor_effects.active).to_list():
            scope.callback(setattr, active, "is_active", True)

        def placed(tree: bpy.types.NodeTree) -> Outcome[bpy.types.SceneCompositorEffect]:
            scope.callback(bpy.data.node_groups.remove, tree)
            entry = scene.compositor_effects.new("Input Capture")
            scope.callback(scene.compositor_effects.remove, entry)
            return written(entry, ("node_group",), tree).bind(lambda _: relocated(scene, entry, at)).map(lambda _: entry)

        suffix = tuple(scene.compositor_effects)[at:] if stop else ()
        for entry in suffix:
            scope.callback(setattr, entry, "enable_for_render", entry.enable_for_render)
        yield (
            composites
            .check(bpy.context, Some(scene))
            .bind(lambda _: accumulated(written(entry, ("enable_for_render",), False) for entry in suffix))
            .bind(
                lambda _: functions.built(
                    "input_capture", "Input Capture", face, {COLOR_RESULT.key: group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor).keeping(*output)}, bpy.types.CompositorNodeTree
                )
            )
            .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .bind(placed)
        )


# --- [COMPOSITION] ----------------------------------------------------------------------

READ: Final = ViewerRead()
composites: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, ("render", "use_compositing"), bpy.types.BoolProperty))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["READ", "Arm", "Consume", "ViewTarget", "ViewerRead", "Viewers", "capturing", "clear_viewer", "cleared", "composites", "shown", "view_output"]
