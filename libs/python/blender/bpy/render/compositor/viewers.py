"""Owned Viewer nodes of offered and pinned entries with the row of buttons viewing and clearing them, entering-frame reads delivered to a caller's consumer and the measure command form over them, and scoped pass-through captures of an entry's input."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack, nullcontext
from functools import cached_property, partial, update_wrapper
from typing import ClassVar, Final, Protocol, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, NoUndo, Operator, Placement, Unchanged
from blender.bpy.data.ids.images import read
from blender.bpy.data.ids.ownership import Mark, Saved
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.conditions import Condition, Job, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import alerted
from blender.bpy.nodes import functions
from blender.bpy.nodes.addressing import owned, OwnedNodes
from blender.bpy.nodes.changes import Added, Connected, made, Removed
from blender.bpy.nodes.desired import End, INPUTS, Link, OUTPUTS
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.nodes.interfaces import Interface
from blender.bpy.nodes.trees import links_of, socket_at
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.effects import CompositorEffect, Named, VIEWER
from blender.bpy.render.compositor.entries import added_entry
from blender.bpy.render.compositor.inputs import grouped
from blender.bpy.render.compositor.offered import OfferedEffects
from blender.bpy.render.outputs.results import built, INTERMEDIATES
from blender.bpy.ui.layout.rows import operator_button, RowDraw
from blender.bpy.ui.screens.areas import AreaQuery, EditorState
from blender.imaging.files.images import ImageFile
from blender.imaging.geometry.frames import Region
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Consume = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, ImageFile], Outcome[Option[str]]]


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
    """Pending read of one entry: its crop, its consumer, and whether a completed frame was captured for it."""

    name: str
    region: Option[Region]
    consume: Consume
    captured: bool = False


# --- [SERVICES] -------------------------------------------------------------------------


class ViewerRead(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Entering-frame reads delivered after compositor completion, one pending read per scene, forgotten at file load."""

    pending: Resource[dict[Handle[bpy.types.Scene], PendingRead]] = msgspec.field(default_factory=lambda: Resource(made=lambda _identity: nullcontext({}), renewed=Some(lambda _held: {})))

    def entering(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, consume: Consume, region: Option[Region] = Nothing) -> Outcome[None]:
        """Views entry input, replacing the scene's pending read."""
        return shown(identity, entry, Viewers.entering).map(lambda _: identity.resources[self.pending].update({Handle.of(scene): PendingRead(name=entry.name, region=region, consume=consume)}))

    def delivered(self, identity: Identity, handle: Handle[bpy.types.Scene], pending: PendingRead, file: ImageFile) -> Outcome[Option[str]]:
        """Consumes captured pixels while the capture is the scene's pending read."""
        if (held := identity.resources[self.pending]).get(handle) is not pending:
            return Ok(Nothing)
        del held[handle]
        cropped = pending.region.map(lambda region: msgspec.structs.replace(file, pixels=file.pixels[region.window(*file.pixels.shape[:2])])).default_value(file)
        return handle.resolve().bind(
            lambda scene: (
                Option
                .of_optional(scene.compositor_effects.get(pending.name))
                .map(lambda entry: cleared(identity, entry).bind(lambda _: pending.consume(identity, scene, entry, cropped)))
                .default_value(Ok(Nothing))
            )
        )

    def measuring(self, measure: Measure, /, *, icon: Option["rna_enums.IconItems"] = Nothing, condition: Option[Condition] = Nothing) -> Operator[Named]:
        """Builds the command measuring the named entry's entering frame, keyed, described, and undo-titled by the measure, available while no render runs and the condition holds."""
        running = Job(kind="RENDER")
        return Operator(
            form=Named, run=update_wrapper(partial(self.measured, measure), measure), undo=NoUndo(), icon=icon, condition=Some(condition.map(lambda held: running & held).default_value(running))
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

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields the pending cell and the completion handler capturing the compositor result for deferred delivery."""
        with identity.reports.deferred(lambda handle, pending, file: self.delivered(identity, handle, pending, file)) as deferred:

            def completed(scene: bpy.types.Scene) -> Outcome[None]:
                if scene.session_uid in identity.resources[INTERMEDIATES]:
                    return Ok(None)
                handle, held = Handle.of(scene), identity.resources[self.pending]

                def queued(pending: PendingRead, file: ImageFile) -> None:
                    taken = held[handle] = msgspec.structs.replace(pending, captured=True)
                    deferred(handle, taken, file)

                return (
                    Option
                    .of_optional(held.get(handle))
                    .filter(lambda pending: not pending.captured and Option.of_optional(scene.compositor_effects.get(pending.name)).filter(lambda entry: entry.is_active).is_some())
                    .map(lambda pending: built("COMPOSITING").bind(lambda image: read(image, PyOpenColorIO.GetCurrentConfig())).map(lambda file: queued(pending, file)))
                    .default_value(Ok(None))
                )

            yield Ok((self.pending, INTERMEDIATES, Handler(handlers=bpy.app.handlers.composite_post, body=completed)))


class Viewers[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """View and clear commands over the owned Viewer of offered and pinned entries."""

    rows: OfferedEffects[R]
    entering: ClassVar[End] = End(node=INPUTS, socket=COLOR_INPUT.key)
    leaving: ClassVar[End] = End(node=OUTPUTS, socket=COLOR_RESULT.key)
    ends: ClassVar[tuple[tuple[str, End], ...]] = (("Input", entering), ("Result", leaving))

    def entry(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[tuple[bpy.types.SceneCompositorEffect, Option[CompositorEffect[R]]]]:
        """Finds named offered or pinned entry with its offered effect, skipping user groups."""
        keys = frozenset((*self.rows.keyed, *(row.ownership.key for row in self.rows.pinned)))
        return (
            Option
            .of_optional(scene.compositor_effects.get(name))
            .bind(lambda entry: Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).filter(lambda mark: mark.key in keys).map(lambda mark: (entry, mark)))
            .map(lambda pair: (pair[0], Option.of_optional(self.rows.keyed.get(pair[1].key))))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def view_output(self, call: Call[ViewTarget]) -> Outcome[Answer]:
        """Show effect group output in place of stack result."""
        query = AreaQuery(space=bpy.types.SpaceNodeEditor, ui_type=Some(bpy.types.CompositorNodeTree.__name__))
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
                        .map(lambda at: pair[0].applied(at.area).map(lambda _: Done()))
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
        return Operator(form=ViewTarget, run=self.view_output, options=frozenset({"INTERNAL"}))

    @cached_property
    def clearing(self) -> Operator[Named]:
        """Command removing an entry's owned Viewer."""
        return Operator(form=Named, run=self.clear_viewer, options=frozenset({"INTERNAL"}))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the entering-frame read and the view and clear commands."""
        return Ok((READ, self.viewing, self.clearing))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the row of Input, Result, and Clear buttons over the drawing entry."""
        return Ok(partial(self.shown, identity))

    def shown(self, identity: Identity, layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the buttons viewing the entry's input and result and clearing its Viewer."""
        for entry in Option.of_optional(try_downcast(bpy.types.SceneCompositorEffect, holder)).to_list():
            line = layout.row(align=True)
            viewed = (operator_button(line, identity, Placement(self.viewing, {"name": entry.name, "node": end.node, "socket": end.socket}), {"text": text}) for text, end in self.ends)
            alerted(line, accumulated((*viewed, operator_button(line, identity, Placement(self.clearing, {"name": entry.name}), {"text": "Clear"}))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [VIEWER]


def shown(identity: Identity, entry: bpy.types.SceneCompositorEffect, target: End) -> Outcome[None]:
    """Connects owned Viewer to target end and activates entry."""

    def source(keyed: OwnedNodes, key: Key) -> Outcome[End]:
        keys = {node: key for key, node in keyed.nodes.items()}

        def ended(link: bpy.types.NodeLink) -> Option[End]:
            match link.from_node, link.from_socket:
                case bpy.types.Node() as node, bpy.types.NodeSocket(identifier=identifier) if node in keys:
                    return Some(End(node=keys[node], socket=identifier))
                case _:
                    return Nothing

        return (
            Option
            .of_optional(keyed.nodes.get(OUTPUTS))
            .bind(lambda output: Option.of_optional(keyed.sockets[OUTPUTS].get(key)).bind(lambda identifier: socket_at(output.inputs, identifier).to_option()))
            .bind(lambda socket: Block.of_seq(links_of(socket)).choose(ended).try_head())
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(keyed.tree.links), member=key))
        )

    def placed(tree: bpy.types.NodeTree, keyed: OwnedNodes, end: End) -> Outcome[None]:
        stale = tuple(link for viewer in _viewer(identity, tree).to_list() for link in tree.links if link.to_node == viewer)
        adding = () if _viewer(identity, tree).is_some() else (Added(key=VIEWER, kind=bpy.types.CompositorNodeViewer),)
        return (
            accumulated(row.applied(keyed, identity) for row in (*(Removed(item=link) for link in stale), *adding))
            .bind(lambda _: OwnedNodes.of(tree, identity))
            .bind(lambda fresh: Connected(link=Link(source=end, target=End(node=VIEWER, socket="Image"))).applied(fresh, identity))
        )

    def viewed(tree: bpy.types.NodeTree, keyed: OwnedNodes) -> Outcome[None]:
        match target:
            case End(node=node, socket=str(key)) if node == OUTPUTS:
                entering = source(keyed, key)
            case _:
                entering = Ok(target)
        return (
            entering
            .bind(lambda end: placed(tree, keyed, end))
            .bind(lambda _: written(entry, ("is_active",), True))
            .bind(lambda _: _viewer(identity, tree).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=VIEWER)))
            .bind(lambda viewer: written(tree.nodes, ("active",), viewer).map(lambda _: None))
        )

    return grouped(entry).bind(lambda tree: OwnedNodes.of(tree, identity).bind(lambda keyed: viewed(tree, keyed)))


def cleared(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
    """Removes owned Viewer, answering whether it existed."""

    def removed(tree: bpy.types.NodeTree) -> Outcome[bool]:
        return _viewer(identity, tree).map(lambda viewer: OwnedNodes.of(tree, identity).bind(lambda keyed: Removed(item=viewer).applied(keyed, identity)).map(lambda _: True)).default_value(Ok(False))

    return grouped(entry).bind(removed)


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
    interface = Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,))
    with ExitStack() as scope:
        for active in Option.of_optional(scene.compositor_effects.active).to_list():
            scope.callback(setattr, active, "is_active", True)
        suffix = tuple(scene.compositor_effects)[at:] if stop else ()
        for entry in suffix:
            scope.callback(setattr, entry, "enable_for_render", entry.enable_for_render)

        def placed(tree: bpy.types.NodeTree) -> Outcome[bpy.types.SceneCompositorEffect]:
            scope.callback(bpy.data.node_groups.remove, tree)
            return added_entry(scope, scene, tree, at=Some(at))

        yield (
            composites
            .check(bpy.context, Some(scene))
            .bind(lambda _: accumulated(written(entry, ("enable_for_render",), False) for entry in suffix))
            .bind(lambda _: functions.built("input_capture", interface, {COLOR_RESULT.key: group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor).keeping(*output)}, bpy.types.CompositorNodeTree))
            .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .bind(placed)
        )


# --- [COMPOSITION] ----------------------------------------------------------------------

READ: Final = ViewerRead()
composites: Final = Setting(descriptor=Descriptor.declared(bpy.types.Scene, ("render", "use_compositing"), bpy.types.BoolProperty))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["READ", "Consume", "Measure", "PendingRead", "ViewTarget", "ViewerRead", "Viewers", "capturing", "cleared", "composites", "shown"]
