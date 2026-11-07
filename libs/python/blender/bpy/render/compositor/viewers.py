"""Owned Viewer nodes of offered and pinned entries with the row of buttons viewing and clearing them, entering-frame reads delivered to a caller's consumer and the measure command form over them, and scoped pass-through captures of an entry's input."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import cached_property, partial, update_wrapper
from typing import ClassVar, Final, Protocol, TYPE_CHECKING

import bpy
from expression import Option, Some, try_downcast
from expression.collections import Block
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Placement, Unchanged
from blender.bpy.data.ids.ownership import Mark, retained, tracked
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Condition, Job
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.paths import definition, rooted, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Resource
from blender.bpy.nodes.addressing import OwnedNodes
from blender.bpy.nodes.changes import Removed
from blender.bpy.nodes.desired import End, INPUTS, OUTPUTS
from blender.bpy.nodes.trees import created, endpoints, linked, links_of, socket_at
from blender.bpy.render.color.spaces import entering
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.effects import CompositorEffect, Named, VIEWER
from blender.bpy.render.compositor.entries import EXECUTING
from blender.bpy.render.compositor.inputs import grouped
from blender.bpy.render.compositor.offered import OfferedEffects
from blender.bpy.render.outputs.results import INTERMEDIATES
from blender.bpy.render.sessions.staging import captured_input
from blender.bpy.ui.layout.rows import operator_button, RowDraw
from blender.bpy.ui.screens.areas import AreaQuery, EditorState
from blender.imaging.color.processors import called, captured_scene_source, converted_image
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Region
from blender.records.faults import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Consume = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, ImageFile], Outcome[Option[str]]]
type _Reads = tuple[dict[Handle[bpy.types.Scene], PendingRead], Callable[[Handle[bpy.types.Scene], PendingRead, ImageFile], None]]


class Measure(Protocol):
    """Read of an entry's entering frame writing what it measures into the entry, answering whether it wrote, its name titling the undo step."""

    __name__: str

    def __call__(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile, /) -> Outcome[bool]:
        """Measures the frame into the entry."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class ViewTarget(Named):
    """Entry and group output to inspect."""

    node: bpy.props.StringProperty(name="Node", description="Group node key holding the output", default=INPUTS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    socket: bpy.props.StringProperty(name="Socket", description="Output socket key", default=COLOR_INPUT.key)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class PendingRead(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Captured input encoding, crop, and consumer for one requested prefix read."""

    name: str
    encoding: Option[PyOpenColorIO.Processor]
    region: Option[Region]
    consume: Consume


# --- [SERVICES] -------------------------------------------------------------------------


class ViewerRead(msgspec.Struct, frozen=True, kw_only=True, eq=False, dict=True):
    """Owned prefix captures delivered to one pending consumer per scene, forgotten at file load."""

    @cached_property
    def pending(self) -> Resource[_Reads]:
        """Pending reads and their scoped delivery queue."""
        return Resource(made=self.held, renewed=Some(lambda held: (dict[Handle[bpy.types.Scene], PendingRead](), held[1])))

    @contextmanager
    def held(self, identity: Identity) -> Generator[_Reads]:
        """Keeps captured files deliverable while the declaration is acquired."""
        with identity.reports.deferred(partial(self.delivered, identity)) as deferred:
            yield dict[Handle[bpy.types.Scene], PendingRead](), deferred

    def entering(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, consume: Consume, region: Option[Region] = Nothing) -> Outcome[None]:
        """Captures the entry input and its encoding through an isolated native prefix render, then queues delivery."""

        def queued(encoding: Option[PyOpenColorIO.Processor], file: ImageFile) -> None:
            held, deferred = self.pending[identity]
            handle = Handle.of(scene)
            pending = held[handle] = PendingRead(name=entry.name, encoding=encoding, region=region, consume=consume)
            deferred(handle, pending, file)

        def prepared(config: PyOpenColorIO.Config, keyed: OwnedNodes, source: bpy.types.NodeSocket) -> Outcome[None]:
            return (
                entering(config, entry, source.identifier)
                .to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member="encoding colorspace"))
                .bind(
                    lambda space: captured_scene_source(config, space).bind(
                        lambda encoding: _shown(identity, entry, keyed, source).bind(lambda _: captured_input(identity, scene, entry, space, source)).map(partial(queued, encoding))
                    )
                )
            )

        return paired(
            called(PyOpenColorIO.GetCurrentConfig, entry.name),
            grouped(entry).map(lambda tree: OwnedNodes.of(tree, identity)).bind(lambda keyed: keyed.socket(INPUTS, COLOR_INPUT.key, output=True).map(lambda source: (keyed, source))),
        ).bind(lambda pair: prepared(pair[0], *pair[1]))

    def delivered(self, identity: Identity, handle: Handle[bpy.types.Scene], pending: PendingRead, file: ImageFile) -> Outcome[Option[str]]:
        """Consumes captured pixels while the capture is the scene's pending read."""
        held, _ = self.pending[identity]
        if held.get(handle) is not pending:
            return Ok(Nothing)
        del held[handle]
        cropped = pending.region.map(lambda region: msgspec.structs.replace(file, pixels=file.pixels[region.window(*file.pixels.shape[:2])])).default_value(file)
        return handle.resolve().bind(
            lambda scene: (
                Option
                .of_optional(scene.compositor_effects.get(pending.name))
                .map(
                    lambda entry: (
                        cleared(identity, entry)
                        .bind(lambda _: called(PyOpenColorIO.GetCurrentConfig, pending.name))
                        .bind(lambda config: converted_image(config, bpy.data.colorspace.working_space, cropped, source=pending.encoding))
                        .bind(lambda normalized: pending.consume(identity, scene, entry, normalized))
                    )
                )
                .default_value(Ok(Nothing))
            )
        )

    def measuring(self, measure: Measure, /, *, icon: Option["rna_enums.IconItems"] = Nothing, condition: Option[Condition] = Nothing) -> Operator[Named]:
        """Builds the command measuring the named entry's entering frame, keyed, described, and undo-titled by the measure, available while no render runs and the condition holds."""
        running = Job(kind="RENDER")
        return Operator(
            form=Named, run=update_wrapper(partial(self.measured, measure), measure), options=frozenset(), icon=icon, condition=Some(condition.map(lambda held: running & held).default_value(running))
        )

    def measured(self, measure: Measure, call: Call[Named]) -> Outcome[Answer]:
        """Views the named entry's input, the completed frame read by the measure."""
        title = bpy.path.display_name(measure.__name__)

        def consumed(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile) -> Outcome[Option[str]]:
            return measure(identity, scene, entry, file).map(lambda wrote: Some(title) if wrote else Nothing)

        return (
            rooted(bpy.types.Scene, call.context)
            .bind(
                lambda scene: (
                    Option
                    .of_optional(scene.compositor_effects.get(call.properties.name))
                    .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=call.properties.name))
                    .bind(lambda entry: self.entering(call.identity, scene, entry, consumed))
                )
            )
            .map(lambda _: Done())
        )

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Acquires pending deliveries and native prefix isolation resources."""
        return self.pending, INTERMEDIATES, EXECUTING


class Viewers(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """View and clear commands over the owned Viewer of offered and pinned entries."""

    rows: OfferedEffects
    entering: ClassVar[End] = End(node=INPUTS, socket=COLOR_INPUT.key)
    leaving: ClassVar[End] = End(node=OUTPUTS, socket=COLOR_RESULT.key)
    ends: ClassVar[tuple[tuple[str, End], ...]] = (("Input", entering), ("Result", leaving))

    def entry(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[tuple[bpy.types.SceneCompositorEffect, Option[CompositorEffect]]]:
        """Finds named offered or pinned entry with its offered effect, skipping user groups."""
        keys = frozenset((*self.rows.keyed, *(row.ownership.key for row in self.rows.pinned)))
        found: bpy.types.SceneCompositorEffect | None = scene.compositor_effects.get(name)  # ty: ignore[unsound-assignment]
        return (
            Option
            .of_optional(found)
            .bind(lambda entry: Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).filter(lambda mark: mark.key in keys).map(lambda mark: (entry, mark)))
            .map(lambda pair: (pair[0], Option.of_optional(self.rows.keyed.get(pair[1].key))))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def view_output(self, call: Call[ViewTarget]) -> Outcome[Answer]:
        """Show effect group output in place of stack result."""
        query = AreaQuery(space=bpy.types.SpaceNodeEditor, ui_type=Some(definition(bpy.types.CompositorNodeTree).identifier))
        return paired(
            EditorState.of(bpy.types.SpaceNodeEditor, {("node_tree_sub_type",): "SCENE", ("show_backdrop",): True}),
            rooted(bpy.types.Scene, call.context).bind(lambda scene: self.entry(call.identity, scene, call.properties.name)),
        ).bind(
            lambda pair: shown(call.identity, pair[1][0], End(node=call.properties.node, socket=call.properties.socket)).bind(
                lambda _: rooted(bpy.types.WindowManager, call.context).bind(
                    lambda manager: (
                        Block
                        .of_seq(query.located(manager.windows))
                        .try_head()
                        .map(lambda at: pair[0].applied(at).map(lambda _: Done()))
                        .default_value(Ok(Done(notes=(("INFO", "No compositor Node Editor open"),))))
                    )
                )
            )
        )

    def clear_viewer(self, call: Call[Named]) -> Outcome[Answer]:
        """Remove effect Viewer to show stack result."""
        return (
            rooted(bpy.types.Scene, call.context)
            .bind(lambda scene: self.entry(call.identity, scene, call.properties.name))
            .bind(lambda held: cleared(call.identity, held[0]).map(lambda removed: Done() if removed else Unchanged(reason=f"{held[0].name} has no Viewer")))
        )

    @cached_property
    def viewing(self) -> Operator[ViewTarget]:
        """Command showing an entry's group output."""
        return Operator(form=ViewTarget, run=self.view_output, options=frozenset({"INTERNAL", "UNDO"}))

    @cached_property
    def clearing(self) -> Operator[Named]:
        """Command removing an entry's owned Viewer."""
        return Operator(form=Named, run=self.clear_viewer, options=frozenset({"INTERNAL", "UNDO"}))

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns the entering-frame read and the view and clear commands."""
        return READ, self.viewing, self.clearing

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the row of Input, Result, and Clear buttons over the drawing entry."""
        return Ok(partial(self.shown, identity))

    def shown(self, identity: Identity, layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the buttons viewing the entry's input and result and clearing its Viewer."""
        for entry in Option.of_optional(try_downcast(bpy.types.SceneCompositorEffect, holder)).to_list():
            line = layout.row(align=True)
            viewed = (operator_button(line, identity, Placement(self.viewing, {"name": entry.name, "node": end.node, "socket": end.socket}), {"text": text}) for text, end in self.ends)
            identity.reports.faulted(accumulated((*viewed, operator_button(line, identity, Placement(self.clearing, {"name": entry.name}), {"text": "Clear"}))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def shown(identity: Identity, entry: bpy.types.SceneCompositorEffect, target: End) -> Outcome[None]:
    """Connects owned Viewer to target end and activates entry."""

    def source(keyed: OwnedNodes, key: Key) -> Outcome[bpy.types.NodeSocket]:
        return (
            Option
            .of_optional(keyed.nodes.get(OUTPUTS))
            .bind(lambda output: Option.of_optional(keyed.sockets[OUTPUTS].get(key)).bind(lambda identifier: socket_at(output.inputs, identifier).to_option()))
            .bind(lambda socket: Block.of_seq(links_of(socket)).try_head().map(lambda link: endpoints(link)[1]))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(keyed.interface.tree.links), member=key))
        )

    def viewed(keyed: OwnedNodes) -> Outcome[None]:
        match target:
            case End(node=node, socket=str(key)) if node == OUTPUTS:
                entering = source(keyed, key)
            case _:
                entering = keyed.socket(target.node, target.socket, output=True)
        return entering.bind(partial(_shown, identity, entry, keyed))

    return grouped(entry).map(lambda tree: OwnedNodes.of(tree, identity)).bind(viewed)


def cleared(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
    """Removes owned Viewer, answering whether it existed."""

    def removed(tree: bpy.types.NodeTree) -> Outcome[bool]:
        keyed = OwnedNodes.of(tree, identity)
        return Option.of_optional(keyed.nodes.get(VIEWER)).map(lambda viewer: Removed(item=viewer).applied(keyed, identity).map(lambda _: True)).default_value(Ok(value=False))

    return grouped(entry).bind(removed)


def _shown(identity: Identity, entry: bpy.types.SceneCompositorEffect, keyed: OwnedNodes, source: bpy.types.NodeSocket) -> Outcome[None]:
    """Connects and activates the prepared native source, retaining the old Viewer until linking succeeds."""
    tree = keyed.interface.tree
    viewer = keyed.nodes.get(VIEWER)
    stale = tuple(link for link in tree.links if link.to_node == viewer) if viewer is not None else ()
    with ExitStack() as rollback:
        placed = (
            Ok(viewer)
            if viewer is not None
            else tracked(rollback, tree.nodes.remove, created(tree, bpy.types.CompositorNodeViewer)).bind(lambda node: Mark(key=VIEWER).write(identity, node).map(lambda _: node))
        )
        return retained(
            placed.bind(
                lambda viewer: (
                    socket_at(viewer.inputs, "Image")
                    .bind(lambda target: tracked(rollback, tree.links.remove, linked(tree, source, target, verify_limits=False)))
                    .bind(lambda link: accumulated(Removed(item=old).applied(keyed, identity) for old in stale if old != link))
                    .map(lambda _: viewer)
                )
            ),
            rollback,
        ).bind(lambda viewer: written(entry, ("is_active",), value=True).bind(lambda _: written(tree.nodes, ("active",), viewer)).map(lambda _: None))


# --- [COMPOSITION] ----------------------------------------------------------------------

READ: Final = ViewerRead()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["READ", "Consume", "Measure", "PendingRead", "ViewTarget", "ViewerRead", "Viewers", "cleared", "shown"]
