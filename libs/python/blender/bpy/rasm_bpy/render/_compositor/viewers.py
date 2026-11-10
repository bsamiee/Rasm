"""Owned Viewer nodes of offered and pinned entries with the row of buttons viewing and clearing them, entering-frame reads delivered to a caller's consumer and the measure command form over them, and scoped pass-through captures of an entry's input."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import cached_property, partial, update_wrapper
from typing import Annotated, ClassVar, Final, Protocol, TYPE_CHECKING

import bpy
from expression import Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from rasm_bpy.commands import operators
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions, Resource
from rasm_bpy.data._ids.ownership import Mark, retained, tracked
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.conditions import Condition, Job
from rasm_bpy.data._rna.descriptors import written
from rasm_bpy.data._rna.paths import rna_definition, rooted, Struct
from rasm_bpy.nodes._addressing import OwnedNodes
from rasm_bpy.nodes._desired import End, INPUTS, OUTPUTS
from rasm_bpy.nodes._trees import created, endpoints, linked_sockets, links_of, socket_at
from rasm_bpy.nodes.changes import Removed
from rasm_bpy.render._color.spaces import entering_space
from rasm_bpy.render._compositor.colors import COLOR_INPUT, COLOR_RESULT
from rasm_bpy.render._compositor.effects import CompositorEffect, Named, VIEWER
from rasm_bpy.render._compositor.entries import EXECUTING
from rasm_bpy.render._compositor.inputs import grouped
from rasm_bpy.render._compositor.offered import OfferedEffects
from rasm_bpy.render._outputs.results import INTERMEDIATES
from rasm_bpy.render._sessions.staging import captured_input
from rasm_bpy.ui._layout.rows import operator_button, RowDraw
from rasm_bpy.ui._screens.areas import AreaQuery, EditorState
from rasm_imaging import called, captured_scene_source, converted_image, ImageFile, Region
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Reads = tuple[dict[Handle[bpy.types.Scene], _PendingRead], Callable[[Handle[bpy.types.Scene], _PendingRead, ImageFile], None]]


class _Measure(Protocol):
    """Read of an entry's entering frame writing what it measures into the entry, answering whether it wrote, its name titling the undo step."""

    __name__: str

    def __call__(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, file: ImageFile, /) -> Outcome[bool]:
        """Measures the frame into the entry."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class ViewTarget(Named):
    """Entry and group output to inspect."""

    node: Annotated[str, bpy.props.StringProperty(name="Node", description="Group node key holding the output", default=INPUTS)]
    socket: Annotated[str, bpy.props.StringProperty(name="Socket", description="Output socket key", default=COLOR_INPUT.key)]


class _PendingRead(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Captured input encoding, crop, and consumer for one requested prefix read."""

    name: str
    encoding: Option[PyOpenColorIO.Processor]
    region: Option[Region]
    consume: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, ImageFile], Outcome[Option[str]]]


# --- [SERVICES] -------------------------------------------------------------------------


class _ViewerRead(msgspec.Struct, frozen=True, kw_only=True, eq=False, dict=True):
    """Owned prefix captures delivered to one pending consumer per scene, forgotten at file load."""

    @cached_property
    def pending(self) -> Resource[_Reads]:
        """Pending reads and their scoped delivery queue."""
        return Resource(made=self.held, renewed=Some(lambda held: (dict[Handle[bpy.types.Scene], _PendingRead](), held[1])))

    @contextmanager
    def held(self, identity: Identity) -> Generator[_Reads]:
        """Keeps captured files deliverable while the declaration is acquired."""
        with identity.reports.deferred(partial(self.delivered, identity)) as deferred:
            yield dict[Handle[bpy.types.Scene], _PendingRead](), deferred

    def entering(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        entry: bpy.types.SceneCompositorEffect,
        consume: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, ImageFile], Outcome[Option[str]]],
        region: Option[Region] = Nothing,
    ) -> Outcome[None]:
        """Captures the entry input and its encoding through an isolated native prefix render, then queues delivery."""

        def queued(encoding: Option[PyOpenColorIO.Processor], file: ImageFile) -> None:
            held, deferred = self.pending[identity]
            handle = Handle.of(scene)
            pending = held[handle] = _PendingRead(name=entry.name, encoding=encoding, region=region, consume=consume)
            deferred(handle, pending, file)

        def prepared(config: PyOpenColorIO.Config, original: bpy.types.NodeTree, keyed: OwnedNodes, source: bpy.types.NodeSocket) -> Outcome[None]:
            return (
                entering_space(config, entry, source.identifier)
                .to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member="encoding colorspace"))
                .bind(lambda space: captured_scene_source(config, space).map(lambda encoding: (space, encoding)))
                .bind(lambda found: _connected(identity, entry, keyed, source).bind(lambda _: captured_input(identity, scene, entry, original, found[0])).map(partial(queued, found[1])))
            )

        return paired(
            called(PyOpenColorIO.GetCurrentConfig, entry.name),
            grouped(entry).bind(lambda tree: (keyed := OwnedNodes.of(tree, identity)).socket(INPUTS, COLOR_INPUT.key, output=True).map(lambda source: (tree, keyed, source))),
        ).bind(lambda pair: prepared(pair[0], *pair[1]))

    def delivered(self, identity: Identity, handle: Handle[bpy.types.Scene], pending: _PendingRead, file: ImageFile) -> Outcome[Option[str]]:
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
                        _cleared(identity, entry)
                        .bind(lambda _: called(PyOpenColorIO.GetCurrentConfig, pending.name))
                        .bind(lambda config: converted_image(config, bpy.data.colorspace.working_space, cropped, source=pending.encoding))
                        .bind(lambda normalized: pending.consume(identity, scene, entry, normalized))
                    )
                )
                .default_value(Ok(Nothing))
            )
        )

    def measuring(self, measure: _Measure, /, *, icon: Option["rna_enums.IconItems"] = Nothing, condition: Option[Condition] = Nothing) -> operators.Operator[Named]:
        """Builds the command measuring the named entry's entering frame, keyed, described, and undo-titled by the measure, available while no render runs and the condition holds."""
        running = Job(kind="RENDER")
        return operators.Operator(
            form=Named, run=update_wrapper(partial(self.measured, measure), measure), options=frozenset(), icon=icon, condition=Some(condition.map(lambda held: running & held).default_value(running))
        )

    def measured(self, measure: _Measure, call: operators.Call[Named]) -> Outcome[operators.Answer]:
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
            .map(lambda _: operators.Done())
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
        return (
            catch(lambda: scene.compositor_effects[name], exception=KeyError)()
            .to_option()
            .bind(lambda entry: Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).filter(lambda mark: mark.key in keys).map(lambda mark: (entry, mark)))
            .map(lambda pair: (pair[0], Option.of_optional(self.rows.keyed.get(pair[1].key))))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def view_output(self, call: operators.Call[ViewTarget]) -> Outcome[operators.Answer]:
        """Show effect group output in place of stack result, connecting the owned Viewer to the chosen output and activating the entry."""
        query = AreaQuery(space=bpy.types.SpaceNodeEditor, ui_type=Some(rna_definition(bpy.types.CompositorNodeTree).identifier))
        chosen = call.properties

        def viewed(entry: bpy.types.SceneCompositorEffect, keyed: OwnedNodes) -> Outcome[None]:
            entering = (
                Option
                .of_optional(keyed.nodes.get(OUTPUTS))
                .bind(lambda output: Option.of_optional(keyed.sockets[OUTPUTS].get(chosen.socket)).bind(lambda identifier: socket_at(output.inputs, identifier).to_option()))
                .bind(lambda socket: Block.of_seq(links_of(socket)).try_head().map(lambda link: endpoints(link)[1]))
                .to_result_with(partial(Fault.of, Reason.ABSENT, repr(keyed.interface.tree.links), member=chosen.socket))
                if chosen.node == OUTPUTS
                else keyed.socket(chosen.node, chosen.socket, output=True)
            )
            return entering.bind(partial(_connected, call.identity, entry, keyed))

        return paired(
            EditorState.of(bpy.types.SpaceNodeEditor, {("node_tree_sub_type",): "SCENE", ("show_backdrop",): True}),
            rooted(bpy.types.Scene, call.context).bind(lambda scene: self.entry(call.identity, scene, chosen.name)),
        ).bind(
            lambda pair: (
                grouped(pair[1][0])
                .map(lambda tree: OwnedNodes.of(tree, call.identity))
                .bind(partial(viewed, pair[1][0]))
                .bind(
                    lambda _: rooted(bpy.types.WindowManager, call.context).bind(
                        lambda manager: (
                            Block
                            .of_seq(query.located(manager.windows))
                            .try_head()
                            .map(lambda at: pair[0].applied(at).map(lambda _: operators.Done()))
                            .default_value(Ok(operators.Done(notes=(("INFO", "No compositor Node Editor open"),))))
                        )
                    )
                )
            )
        )

    def clear_viewer(self, call: operators.Call[Named]) -> Outcome[operators.Answer]:
        """Remove effect Viewer to show stack result."""
        return (
            rooted(bpy.types.Scene, call.context)
            .bind(lambda scene: self.entry(call.identity, scene, call.properties.name))
            .bind(lambda held: _cleared(call.identity, held[0]).map(lambda removed: operators.Done() if removed else operators.Unchanged(reason=f"{held[0].name} has no Viewer")))
        )

    @cached_property
    def viewing(self) -> operators.Operator[ViewTarget]:
        """Command showing an entry's group output."""
        return operators.Operator(form=ViewTarget, run=self.view_output, options=frozenset({"INTERNAL", "UNDO"}))

    @cached_property
    def clearing(self) -> operators.Operator[Named]:
        """Command removing an entry's owned Viewer."""
        return operators.Operator(form=Named, run=self.clear_viewer, options=frozenset({"INTERNAL", "UNDO"}))

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
            viewed = (operator_button(line, identity, operators.Placement(self.viewing, {"name": entry.name, "node": end.node, "socket": end.socket}), {"text": text}) for text, end in self.ends)
            identity.reports.faulted(accumulated((*viewed, operator_button(line, identity, operators.Placement(self.clearing, {"name": entry.name}), {"text": "Clear"}))))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _cleared(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
    """Removes owned Viewer, answering whether it existed."""

    def removed(tree: bpy.types.NodeTree) -> Outcome[bool]:
        keyed = OwnedNodes.of(tree, identity)
        return Option.of_optional(keyed.nodes.get(VIEWER)).map(lambda viewer: Removed(item=viewer).applied(keyed, identity).map(lambda _: True)).default_value(Ok(value=False))

    return grouped(entry).bind(removed)


def _connected(identity: Identity, entry: bpy.types.SceneCompositorEffect, keyed: OwnedNodes, source: bpy.types.NodeSocket) -> Outcome[None]:
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
                    .bind(lambda target: tracked(rollback, tree.links.remove, linked_sockets(tree, source, target, verify_limits=False)))
                    .bind(lambda link: accumulated(Removed(item=old).applied(keyed, identity) for old in stale if old != link))
                    .map(lambda _: viewer)
                )
            ),
            rollback,
        ).bind(lambda viewer: written(entry, ("is_active",), value=True).bind(lambda _: written(tree.nodes, ("active",), viewer)).map(lambda _: None))


# --- [COMPOSITION] ----------------------------------------------------------------------

READ: Final = _ViewerRead()

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["READ", "ViewTarget", "Viewers"]
