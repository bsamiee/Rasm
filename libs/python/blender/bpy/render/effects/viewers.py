"""Owned effect viewers and scoped delivery of their evaluated input planes."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from pathlib import Path
from typing import Annotated, ClassVar

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.data.ids.images import built, intermediates, read
from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.events.handlers import PathHandler, SceneHandler
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from blender.bpy.nodes.changes import Added, applied, Connected, owned, Removed
from blender.bpy.nodes.graphs import End, INPUTS, Link, OUTPUTS
from blender.bpy.nodes.interface import addresses, slot
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.color.exposure import Consume
from blender.bpy.render.effects.stack import EffectStack, Named, Placeable, Shell, StackEntry
from blender.bpy.settings.attachments import Stored
from blender.bpy.ui.screens.areas import AreaQuery
from blender.bpy.ui.screens.editors import EditorState
from blender.imaging.analysis.metering import Region
from blender.imaging.color.config import admitted
from blender.imaging.files.images import ImageFile

# --- [MODELS] ---------------------------------------------------------------------------


class ViewTarget(Named, frozen=True, kw_only=True):
    """Entry and the output inside its group to inspect."""

    node: Annotated[str, msgspec.Meta(min_length=1)] = INPUTS
    socket: Annotated[str, msgspec.Meta(min_length=1)] = Shell.image


class EntryRegion(Named, frozen=True, kw_only=True):
    """Entry and normalized region of its entering plane."""

    region: Region = Region()


class Unread(msgspec.Struct, frozen=True, kw_only=True):
    """Entry whose region contains no value its read can consume."""

    entry: str

    @property
    def message(self) -> str:
        """Names the missing covered input for the native poll message."""
        return f"Requires a covered region of the frame entering {self.entry} holding a value its effect reads"


class Arm(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Entry and optional crop waiting for one compositor completion."""

    name: str
    region: Option[Region]


class Armed(msgspec.Struct, kw_only=True):
    """Pending input read owned by each native scene handle."""

    entries: dict[Handle[bpy.types.Scene], Arm] = msgspec.field(default_factory=dict)
    captured: set[Arm] = msgspec.field(default_factory=set)


# --- [SERVICES] -------------------------------------------------------------------------


class ViewerRead(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """One consumer with deferred delivery and independently scoped pending reads."""

    consume: Consume
    armed: Armed = msgspec.field(default_factory=Armed)

    def entering(self, identity: Identity, entry: bpy.types.SceneCompositorEffect, region: Option[Region] = Nothing) -> BlenderResult[None]:
        """Shows the entry input and replaces this scene's pending read."""
        return shown(identity, entry, End(node=INPUTS, socket=Shell.image)).map(lambda _: self.armed.entries.update({Handle.of(entry.id_data): Arm(name=entry.name, region=region)}))  # type: ignore[arg-type, type-var]

    def delivered(self, identity: Identity, handle: Handle[bpy.types.Scene], arm: Arm, file: ImageFile[np.float32]) -> BlenderResult[Option[str]]:
        """Consumes captured pixels only while their original read remains demanded."""
        self.armed.captured.discard(arm)
        if self.armed.entries.get(handle) is not arm:
            return Ok(Nothing)
        del self.armed.entries[handle]
        cropped = arm.region.map(lambda region: msgspec.structs.replace(file, pixels=file.pixels[region.window(*file.pixels.shape[:2])])).default_value(file)
        return handle.resolve().bind(
            lambda scene: (
                Option
                .of_optional(scene.compositor_effects.get(arm.name))
                .map(lambda entry: cleared(identity, entry).bind(lambda _: self.consume(identity, scene, entry, cropped)))
                .default_value(Ok(Nothing))
            )
        )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Captures the exclusive completion buffer and defers consumption under one scope."""
        try:
            with identity.reports.deferred(lambda handle, arm, file: self.delivered(identity, handle, arm, file)) as deferred:

                def queued(handle: Handle[bpy.types.Scene], arm: Arm, file: ImageFile[np.float32]) -> None:
                    self.armed.captured.add(arm)
                    deferred(handle, arm, file)

                def completed(scene: bpy.types.Scene) -> BlenderResult[None]:
                    if scene.session_uid in intermediates:
                        return Ok(None)
                    handle = Handle.of(scene)
                    return (
                        Option
                        .of_optional(self.armed.entries.get(handle))
                        .filter(lambda arm: arm not in self.armed.captured)
                        .bind(
                            lambda arm: (
                                Option.of_optional(scene.compositor_effects.get(arm.name)).filter(lambda entry: entry.is_active).map(lambda _: plane().map(lambda file: queued(handle, arm, file)))
                            )
                        )
                        .default_value(Ok(None))
                    )

                def forgotten(_path: Option[Path]) -> BlenderResult[None]:
                    identity.reports.cancelled(deferred)
                    self.armed.entries.clear()
                    self.armed.captured.clear()
                    return Ok(None)

                yield Ok((SceneHandler(handlers=bpy.app.handlers.composite_post, body=completed), PathHandler(handlers=bpy.app.handlers.load_post, body=forgotten)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        finally:
            self.armed.entries.clear()
            self.armed.captured.clear()


class EffectViewer:
    """Inspection and input-reading commands for offered and pinned stack entries."""

    stack: EffectStack
    viewing: Operator[ViewTarget]
    clearing: Operator[Named]
    reading: Operator[EntryRegion]
    key: ClassVar[Key] = Shell.viewer
    entering: ClassVar[End] = End(node=INPUTS, socket=Shell.image)
    leaving: ClassVar[End] = End(node=OUTPUTS, socket=Shell.result)

    def entry(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[StackEntry]:
        """Finds a named offered or pinned entry without claiming a user's group."""
        pinned = frozenset(row.ownership.key for row in self.stack.pinned)
        return Option.of_optional(
            next(
                (
                    held
                    for held in self.stack.held(identity, scene)
                    if held.entry.name == name
                    and (held.row.is_some() or Option.of_optional(held.entry.node_group).bind(lambda group: Mark.read(identity, group)).filter(lambda mark: mark.key in pinned).is_some())
                ),
                None,
            )
        ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),))

    def __init__(self, *, stack: EffectStack) -> None:
        """Retains the original inspection, clear, and read command declarations."""
        self.stack = stack

        def view(call: Call[ViewTarget], inspected: EditorState) -> BlenderResult[Done]:
            query = AreaQuery(space="NODE_EDITOR", ui_type=Some(bpy.types.CompositorNodeTree.__name__))
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.entry(call.identity, scene, call.record.name))
                .bind(
                    lambda held: shown(
                        call.identity, held.entry, End(node=call.record.node, socket=call.record.socket), pending=call.record.node != INPUTS and held.row.filter(lambda row: row.staged).is_some()
                    )
                )
                .bind(
                    lambda _: call.root(bpy.types.WindowManager).bind(
                        lambda manager: (
                            Option
                            .of_optional(next(iter(query.located(manager.windows)), None))
                            .map(lambda at: inspected.applied(at.area).map(lambda _: Done()))
                            .default_value(Ok(Done(notes=(("INFO", "No compositor node editor shows the effect's group"),))))
                        )
                    )
                )
            )

        def take(call: Call[EntryRegion], row: Placeable, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[Done]:
            reading: BlenderResult[ViewerRead] = Option.of_optional(next((each for each in row.registrables if isinstance(each, ViewerRead)), None)).to_result_with(
                lambda: (BlenderAbsentFault(owner=row.kind, key=ViewerRead.__name__),)
            )
            return (
                Result[ViewerRead, BlenderFaults]
                .bind(reading, lambda reading: reading.entering(call.identity, entry, Some(call.record.region)))
                .map(lambda _: Done(notes=(("INFO", f"Reading the frame entering {entry.name}"),)))
            )

        def read(call: Call[EntryRegion]) -> BlenderResult[Done]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.stack.found(call.identity, scene, call.record.name))
                .bind(
                    lambda held: Result[Placeable, BlenderFaults].bind(
                        held.row.to_result_with(lambda: (BlenderAbsentFault(owner=repr(held.entry), key=held.entry.name),)), lambda row: take(call, row, held.entry)
                    )
                )
            )

        def clear(call: Call[Named]) -> BlenderResult[Outcome]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.entry(call.identity, scene, call.record.name))
                .bind(lambda held: cleared(call.identity, held.entry).map(lambda removed: Done() if removed else Unchanged(reason=f"{held.entry.name} shows no viewer")))
            )

        self.viewing = Operator(
            key="effect_view",
            title="View Output",
            description="Shows an output inside the effect in place of the stack's result",
            form=ViewTarget,
            run=lambda call: EditorState.of(bpy.types.SpaceNodeEditor, {"node_tree_sub_type": "SCENE", "show_backdrop": True}).bind(lambda inspected: view(call, inspected)),
            internal=True,
        )
        self.clearing = Operator(key="effect_view_clear", title="Clear Viewer", description="Removes the effect's Viewer, so the stack's result shows again", form=Named, run=clear, internal=True)
        self.reading = Operator(key="effect_read", title="Read Frame", description="Hands a region of the frame entering the effect to its own read", form=EntryRegion, run=read, internal=True)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the three original command declarations for central registration."""
        return Ok((self.viewing, self.clearing, self.reading))


# --- [OPERATIONS] -----------------------------------------------------------------------


def inspection(row: Callable[[], Placeable], key: Key, title: str, description: str, target: Callable[[], End]) -> Operator[Named]:
    """Declares a row-specific inspection without walking its parameters at import."""

    def run(call: Call[Named]) -> BlenderResult[Done]:
        offered = row()
        return (
            call
            .root(bpy.types.Scene)
            .bind(lambda scene: offered.named(call.identity, scene, call.record.name))
            .bind(lambda entry: shown(call.identity, entry, target(), pending=offered.staged))
            .map(lambda _: Done())
        )

    return Operator(key=key, title=title, description=description, form=Named, internal=True, run=run)


def shown(identity: Identity, entry: bpy.types.SceneCompositorEffect, target: End, *, pending: bool = False) -> BlenderResult[None]:
    """Connects a marked viewer to an owned endpoint and activates its entry."""

    def viewed(tree: bpy.types.NodeTree, name: str) -> BlenderResult[None]:
        def source(sockets: dict[Key, str]) -> BlenderResult[End]:
            nodes = owned(tree, identity)
            keys = {node: key for key, node in nodes.items()}
            return (
                Option
                .of_optional(nodes.get(OUTPUTS))
                .bind(lambda output: Option.of_optional(sockets.get(target.socket)).bind(lambda identifier: socket_at(output.inputs, identifier).to_option()))
                .bind(
                    lambda socket: Option.of_optional(
                        next((End(node=keys[link.from_node], socket=link.from_socket.identifier) for link in socket.links if link.from_node in keys), None)  # type: ignore[union-attr]  # ty: ignore[not-iterable, unresolved-attribute]
                    )
                )
                .to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.links), key=target.socket),))
            )

        def connected(end: End) -> BlenderResult[None]:
            for viewer in marked(identity, tree).to_list():
                for link in tuple(tree.links):
                    if link.to_node == viewer:
                        tree.links.remove(link)
            return applied(Connected(link=Link(source=end, target=End(node=Shell.viewer, socket="Image"))), tree, identity, name, owned(tree, identity))

        entering = source(addresses(tree, name)) if target.node == OUTPUTS else Ok(target)
        return (
            entering
            .bind(
                lambda end: (Ok(None) if marked(identity, tree).is_some() else applied(Added(key=Shell.viewer, kind=bpy.types.CompositorNodeViewer), tree, identity, name, owned(tree, identity))).bind(
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
    """Removes the owned viewer and its stored target while preserving unmarked native nodes, answering whether either existed."""

    def removed(tree: bpy.types.NodeTree, name: str, stored: bool) -> BlenderResult[bool]:
        return marked(identity, tree).map(lambda viewer: applied(Removed(name=viewer.name), tree, identity, name, owned(tree, identity)).map(lambda _: True)).default_value(Ok(stored))

    return grouped(identity, entry).bind(
        lambda pair: (
            identity.name(Name.ATTACHMENT, Shell.viewer).bind(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(End)).removed(pair[0])).bind(lambda stored: removed(*pair, stored))
        )
    )


def grouped(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[tuple[bpy.types.NodeTree, str]]:
    """Returns the native group and its declared interface storage slot."""
    tree: BlenderResult[bpy.types.NodeTree] = Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),))
    return Result[bpy.types.NodeTree, BlenderFaults].bind(tree, lambda tree: slot(identity).map(lambda name: (tree, name)))


def marked(identity: Identity, tree: bpy.types.NodeTree) -> Option[bpy.types.Node]:
    """Returns the native viewer carrying this extension's reserved mark."""
    return Option.of_optional(owned(tree, identity).get(Shell.viewer))


def plane() -> BlenderResult[ImageFile[np.float32]]:
    """Reads the native compositor viewer with the current admitted OCIO config."""
    config: BlenderResult[PyOpenColorIO.Config] = admitted(PyOpenColorIO.GetCurrentConfig(), ()).map_error(BlenderImagingFault.lifted)
    return Result[PyOpenColorIO.Config, BlenderFaults].bind(config, lambda config: built("COMPOSITING").bind(lambda image: read(image, config)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arm", "Armed", "EffectViewer", "EntryRegion", "Unread", "ViewTarget", "ViewerRead", "inspection"]
