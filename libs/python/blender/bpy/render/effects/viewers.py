"""Owned Viewer nodes, row inspections, and entering-frame reads delivered to row measures."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, nullcontext
from types import MappingProxyType
from typing import Annotated, ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.data.ids.images import read
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.events.handlers import SceneHandler
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import Added, applied, Connected, Disconnected, Keyed, owned, Removed
from blender.bpy.nodes.graphs import End, INPUTS, Link, OUTPUTS
from blender.bpy.nodes.interface import slot
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.rows import CompositorEffect, EffectName, Inspected, Marks, Measured, Named, revised, Shell
from blender.bpy.render.effects.stack import EffectStack
from blender.bpy.render.outputs.results import built, INTERMEDIATES
from blender.bpy.render.review.frames import Picked, picking
from blender.bpy.settings.attachments import Stored
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.analysis.metering import Region
from blender.imaging.chain import Effect
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.images import ImageFile
from blender.imaging.parameters import heading

# --- [TYPES] ----------------------------------------------------------------------------

type Consume = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, ImageFile[np.float32]], BlenderResult[Option[str]]]

# --- [MODELS] ---------------------------------------------------------------------------


class ViewTarget(Named, frozen=True, kw_only=True):
    """Entry and group output to inspect."""

    node: Annotated[str, msgspec.Meta(min_length=1)] = INPUTS
    socket: Annotated[str, msgspec.Meta(min_length=1)] = Shell.image


class EntryRegion(Named, frozen=True, kw_only=True):
    """Entry and normalized region of entering frame."""

    region: Region = Region()


class EntryPick(Picked, frozen=True, kw_only=True):
    """Pick naming the effect entry it reads."""

    name: EffectName


class Unread(msgspec.Struct, frozen=True, kw_only=True):
    """Entry with no readable value in region."""

    entry: str

    @property
    def message(self) -> str:
        """Poll message naming entry without readable region."""
        return f"Region of frame entering {self.entry} holds no value its effect reads"


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

    def entering(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, consume: Consume, region: Option[Region] = Nothing) -> BlenderResult[None]:
        """Views entry input, replacing the scene's pending read."""
        return shown(identity, entry, End(node=INPUTS, socket=Shell.image)).map(lambda _: self.pending.of(identity).update({Handle.of(scene): Arm(name=entry.name, region=region, consume=consume)}))

    def delivered(self, identity: Identity, handle: Handle[bpy.types.Scene], arm: Arm, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
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
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields the pending cell and the completion handler capturing the compositor result for deferred delivery."""
        with identity.reports.deferred(lambda handle, arm, file: self.delivered(identity, handle, arm, file)) as deferred:

            def completed(scene: bpy.types.Scene) -> BlenderResult[None]:
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
                    .map(lambda arm: config_of().bind(lambda config: built("COMPOSITING").bind(lambda image: read(image, config))).map(lambda file: queued(arm, file)))
                    .default_value(Ok(None))
                )

            yield Ok((self.pending, INTERMEDIATES, SceneHandler(handlers=bpy.app.handlers.composite_post, body=completed)))


class EffectViewer:
    """View, clear, read, and pick commands for offered and pinned entries, and each row's inspection and measure commands."""

    stack: EffectStack
    viewing: Operator[ViewTarget]
    clearing: Operator[Named]
    reading: Operator[EntryRegion]
    picking: Operator[EntryPick]
    commands: Mapping[Key, tuple[Operator[Named], ...]]
    key: ClassVar[Key] = Shell.viewer
    entering: ClassVar[End] = End(node=INPUTS, socket=Shell.image)
    leaving: ClassVar[End] = End(node=OUTPUTS, socket=Shell.result)

    def entry(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, Option[CompositorEffect[Effect]]]]:
        """Finds named offered or pinned entry with its offered row, skipping user groups."""
        pinned = frozenset(row.ownership.key for row in self.stack.pinned)
        return Option.of_optional(
            next(
                (
                    (entry, Option.of_optional(self.stack.keyed.get(mark.key)))
                    for entry, mark in Marks.of(identity, scene).held
                    if entry.name == name and (mark.key in self.stack.keyed or mark.key in pinned)
                ),
                None,
            )
        ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),))

    def __init__(self, *, stack: EffectStack) -> None:
        """Declares view, clear, read, and pick commands, and one command per row inspection and measure."""
        self.stack = stack

        def view(call: Call[ViewTarget], inspected: EditorState) -> BlenderResult[Done]:
            query = AreaQuery(space="NODE_EDITOR", ui_type=Some(bpy.types.CompositorNodeTree.__name__))
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.entry(call.identity, scene, call.record.name))
                .bind(
                    lambda held: shown(
                        call.identity, held[0], End(node=call.record.node, socket=call.record.socket), pending=call.record.node != INPUTS and held[1].filter(lambda row: row.staged).is_some()
                    )
                )
                .bind(
                    lambda _: call.root(bpy.types.WindowManager).bind(
                        lambda manager: (
                            Option
                            .of_optional(next(iter(query.located(manager.windows)), None))
                            .map(lambda at: inspected.applied(at.area).map(lambda _: Done()))
                            .default_value(Ok(Done(notes=(("INFO", "No compositor Node Editor open"),))))
                        )
                    )
                )
            )

        def taken[T: msgspec.Struct](call: Call[T], name: str, region: Region) -> BlenderResult[Outcome]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.stack.found(call.identity, scene, name).bind(lambda held: READ.entering(call.identity, scene, held.entry, measured(held.row), Some(region))))
                .map(lambda _: Done(notes=(("INFO", f"Reading frame entering {name}"),)))
            )

        def clear(call: Call[Named]) -> BlenderResult[Outcome]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.entry(call.identity, scene, call.record.name))
                .bind(lambda held: cleared(call.identity, held[0]).map(lambda removed: Done() if removed else Unchanged(reason=f"{held[0].name} has no Viewer")))
            )

        self.viewing = Operator(
            key="effect_view",
            title="View Output",
            description="Show effect group output in place of stack result",
            form=ViewTarget,
            run=lambda call: EditorState.of(bpy.types.SpaceNodeEditor, {"node_tree_sub_type": "SCENE", "show_backdrop": True}).bind(lambda inspected: view(call, inspected)),
            internal=True,
        )
        self.clearing = Operator(key="effect_view_clear", title="Clear Viewer", description="Remove effect Viewer to show stack result", form=Named, run=clear, internal=True)
        self.reading = Operator(
            key="effect_read", title="Read Frame", description="Read region of entering frame", form=EntryRegion, run=lambda call: taken(call, call.record.name, call.record.region), internal=True
        )
        self.picking = picking("pick_read", "Pick Read Region", "Hands picked entry input square to entry's read", "EYEDROPPER", EntryPick, lambda call, region: taken(call, call.record.name, region))
        self.commands = MappingProxyType({
            row.kind: (*(inspecting(row, held) for held in row.inspected), *(command for measure in row.measured.to_list() for command in measure.command.to_list())) for row in stack.rows
        })

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the entering-frame read, view, clear, read, and pick commands, then each row's inspection and measure commands."""
        return Ok((READ, self.viewing, self.clearing, self.reading, self.picking, *(command for commands in self.commands.values() for command in commands)))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [MEASURES]


def measured[R: Effect](row: CompositorEffect[R]) -> Consume:
    """Consumer revising an entry's record from its entering frame through the row's measure, its undo step named by the measure command."""
    title = row.measured.bind(lambda measure: measure.command).map(lambda command: command.title).default_value(f"Measure {row.record.title}")

    def consumed(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
        def changed(measure: Measured[R], config: PyOpenColorIO.Config) -> BlenderResult[bool]:
            return revised(row, identity, scene, entry, lambda record: measure.revise(config, record, file).map(lambda found: found.default_value(record)))

        return (
            paired(row.measured.to_result_with(lambda: (BlenderAbsentFault(owner=row.kind, key="measured"),)), config_of())
            .bind(lambda pair: changed(*pair))
            .map(lambda wrote: Some(title) if wrote else Nothing)
        )

    return consumed


def measuring[R: Effect](row: CompositorEffect[R], call: Call[Named]) -> BlenderResult[Outcome]:
    """Arms the stack read on the named entry of the row, delivering its next entering frame to the row's measure."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: row.named(call.identity, scene, call.record.name).bind(lambda entry: READ.entering(call.identity, scene, entry, measured(row))))
        .map(lambda _: Done(notes=(("INFO", f"Measuring {row.record.title} from stage input"),)))
    )


# --- [VIEWER]


def inspecting(row: CompositorEffect[Effect], held: Inspected) -> Operator[Named]:
    """Declares the Viewer command of one row inspection, titled by its key."""

    def run(call: Call[Named]) -> BlenderResult[Outcome]:
        return (
            call
            .root(bpy.types.Scene)
            .bind(lambda scene: row.named(call.identity, scene, call.record.name))
            .bind(lambda entry: shown(call.identity, entry, held.end, pending=row.staged))
            .map(lambda _: Done())
        )

    return Operator(key=held.key, title=f"View {heading(held.key)}", description=held.description, icon=Some("HIDE_OFF"), form=Named, internal=True, run=run)


def shown(identity: Identity, entry: bpy.types.SceneCompositorEffect, target: End, *, pending: bool = False) -> BlenderResult[None]:
    """Connects owned Viewer to target end and activates entry."""

    def viewed(tree: bpy.types.NodeTree, name: str) -> BlenderResult[None]:
        def source(keyed: Keyed) -> BlenderResult[End]:
            keys = {node: key for key, node in keyed.nodes.items()}
            return (
                Option
                .of_optional(keyed.nodes.get(OUTPUTS))
                .bind(lambda output: Option.of_optional(keyed.sockets[OUTPUTS].get(target.socket)).bind(lambda identifier: socket_at(output.inputs, identifier).to_option()))
                .bind(
                    lambda socket: Option.of_optional(
                        next((End(node=keys[link.from_node], socket=link.from_socket.identifier) for link in socket.links if link.from_node in keys), None)  # type: ignore[union-attr]  # ty: ignore[not-iterable, unresolved-attribute]
                    )
                )
                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.links), key=target.socket),))
            )

        def connected(end: End) -> BlenderResult[None]:
            stale = tuple(link for viewer in marked(identity, tree).to_list() for link in tree.links if link.to_node == viewer)
            return accumulated(applied(Disconnected(link=link), identity, Keyed.of(tree, identity, name)) for link in stale).bind(
                lambda _: applied(Connected(link=Link(source=end, target=End(node=Shell.viewer, socket="Image"))), identity, Keyed.of(tree, identity, name))
            )

        entering = source(Keyed.of(tree, identity, name)) if target.node == OUTPUTS else Ok(target)
        return (
            entering
            .bind(
                lambda end: (Ok(None) if marked(identity, tree).is_some() else applied(Added(key=Shell.viewer, kind=bpy.types.CompositorNodeViewer), identity, Keyed.of(tree, identity, name))).bind(
                    lambda _: connected(end)
                )
            )
            .bind(lambda _: written(entry, "is_active", True))
            .bind(lambda _: marked(identity, tree).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=Shell.viewer),)))
            .bind(lambda viewer: written(tree.nodes, "active", viewer).map(lambda _: None))
        )

    return grouped(identity, entry).bind(
        lambda pair: (
            identity
            .name(Name.ATTACHMENT, Shell.viewer)
            .bind(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(End)).write(pair[0], target))
            .bind(lambda _: written(entry, "is_active", True).map(lambda _: None) if pending else viewed(*pair))
        )
    )


def cleared(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bool]:
    """Removes owned Viewer and stored target, answering whether either existed."""

    def removed(tree: bpy.types.NodeTree, name: str, *, stored: bool) -> BlenderResult[bool]:
        return marked(identity, tree).map(lambda viewer: applied(Removed(name=viewer.name), identity, Keyed.of(tree, identity, name)).map(lambda _: True)).default_value(Ok(stored))

    return grouped(identity, entry).bind(
        lambda pair: (
            identity.name(Name.ATTACHMENT, Shell.viewer).bind(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(End)).removed(pair[0])).bind(lambda stored: removed(*pair, stored=stored))
        )
    )


def grouped(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[tuple[bpy.types.NodeTree, str]]:
    """Returns entry group and interface slot name."""
    tree: BlenderResult[bpy.types.NodeTree] = Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),))
    return Result[bpy.types.NodeTree, BlenderFaults].bind(tree, lambda tree: slot(identity).map(lambda name: (tree, name)))


def marked(identity: Identity, tree: bpy.types.NodeTree) -> Option[bpy.types.Node]:
    """Returns Viewer node that extension marks as owned."""
    return Option.of_optional(owned(tree, identity).get(Shell.viewer))


# --- [COMPOSITION] ----------------------------------------------------------------------

READ: Final = ViewerRead()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["READ", "Arm", "Consume", "EffectViewer", "EntryPick", "EntryRegion", "Unread", "ViewTarget", "ViewerRead", "cleared", "inspecting", "measured", "measuring", "shown"]
