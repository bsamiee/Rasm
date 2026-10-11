"""Selected-tree editing, native link occurrences, and owned-group insertion."""

from collections import deque
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial, reduce
from itertools import accumulate, starmap
from statistics import fmean
from typing import Annotated, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.structs

from rasm.blender.commands.operators import Call, Confirm, Done, Empty, Operator, Properties
from rasm.blender.data.rna.conditions import AllOf, Condition, Count, Present, Setting
from rasm.blender.data.rna.descriptors import Descriptor, IdName, Value, written
from rasm.blender.data.rna.snapshots import Snapshot
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult
from rasm.blender.nodes.graphs import GraphTree, NodeGroup, valued
from rasm.blender.nodes.placement import NodeSize, NodeSpacing
from rasm.blender.settings.properties import Choices
from rasm.blender.ui.screens.areas import AreaQuery, Located
from rasm.blender.ui.screens.editors import EditorState
from rasm.blender.ui.screens.regions import RegionView
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Case, Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Matching(Choice):
    """Fact pairing an active output with a selected target."""

    TYPE = 1, "Socket Type"
    NAME = 2, "Node Name"
    OUTPUT = 3, "Output Name"


class Outputs(Choice):
    """Output connections taking a reroute."""

    ALL = 1, "All Outputs"
    LOOSE = 2, "Loose Outputs"
    LINKED = 3, "Linked Outputs"


class LabelSource(Choice):
    """Source of a selected node's copied label."""

    ACTIVE = 1, "Active Node"
    NODE = 2, "Linked Node"
    SOCKET = 3, "Linked Socket"


class SteppedMember(Choice):
    """Native enum member edited through its RNA item order."""

    BLEND_TYPE = 1, "Blend Type"
    OPERATION = 2, "Operation"


class StepDirection(Choice):
    """Direction through the native enum items."""

    NEXT = 1, "Next"
    PREVIOUS = 2, "Previous"


class FactorChange(Choice):
    """Assignment or addition to a mixing factor."""

    SET = 1, "Set"
    NUDGE = 2, "Nudge"


class Relation(Choice):
    """Direction of selection through frame relationships."""

    PARENTS = 1, "Parent Frames"
    CHILDREN = 2, "Frame Children"


type Merging = Automatic | MixBlend | MathOperation | MixShader | AddShader | Join | MeshBoolean | DepthCombine | Alpha
type Mode = MixBlend | MathOperation
type Terminal = bpy.types.ShaderNodeOutputMaterial | bpy.types.ShaderNodeOutputWorld | bpy.types.ShaderNodeOutputLight | bpy.types.NodeGroupOutput
type EditBody[T: msgspec.Struct] = Callable[[T, Chosen, NodeSpacing], BlenderResult[Done]]
type Incoming = bpy.types.NodeLink | tuple[bpy.types.NodeSocket, bool]
type GroupNode = bpy.types.CompositorNodeGroup | bpy.types.ShaderNodeGroup | bpy.types.GeometryNodeGroup

# --- [ROSTERS] --------------------------------------------------------------------------


def _rostered(kind: type[bpy.types.Node], member: str) -> Choices:
    return Choices(
        lambda _identity, _owner, _context: (
            (item.identifier, item.name, item.description) for prop in kind.bl_rna.properties if isinstance(prop, bpy.types.EnumProperty) and prop.identifier == member for item in prop.enum_items
        )
    )


blends = _rostered(bpy.types.ShaderNodeMix, "blend_type")
operations = _rostered(bpy.types.ShaderNodeMath, "operation")
booleans = _rostered(bpy.types.GeometryNodeMeshBoolean, "operation")

# --- [MODELS] ---------------------------------------------------------------------------


class Automatic(Case, frozen=True, kw_only=True, tag=1):
    """Merge family selected by the first output's native type."""

    title = "Automatic"


class MixBlend(Case, frozen=True, kw_only=True, tag=2):
    """Color and value sources joined by native blend nodes."""

    title = "Blend"
    blend: Annotated[str, msgspec.Meta(description="Blend type of the mix nodes"), blends] = "MIX"


class MathOperation(Case, frozen=True, kw_only=True, tag=3):
    """Scalar sources joined by native math nodes."""

    title = "Operation"
    operation: Annotated[str, msgspec.Meta(description="Math operation joining the sources"), operations] = "ADD"


class MixShader(Case, frozen=True, kw_only=True, tag=4):
    """Shader sources joined by Mix Shader nodes."""

    title = "Mix Shader"


class AddShader(Case, frozen=True, kw_only=True, tag=5):
    """Shader sources joined by Add Shader nodes."""

    title = "Add Shader"


class Join(Case, frozen=True, kw_only=True, tag=6):
    """Geometry sources joined through one multi-input node."""

    title = "Join"


class MeshBoolean(Case, frozen=True, kw_only=True, tag=7):
    """Geometry sources joined by the selected mesh boolean operation."""

    title = "Boolean"
    operation: Annotated[str, msgspec.Meta(description="Mesh boolean operation"), booleans] = "DIFFERENCE"


class DepthCombine(Case, frozen=True, kw_only=True, tag=8):
    """Images combined with their native depth outputs."""

    title = "Depth"


class Alpha(Case, frozen=True, kw_only=True, tag=9):
    """Images combined by Alpha Over nodes."""

    title = "Alpha Over"


class Merge(msgspec.Struct, frozen=True, kw_only=True):
    """Merge family selected by the operator."""

    merging: Annotated[Merging, msgspec.Meta(title="Merge", description="Family joining the sources")] = Automatic()


class LinkActive(msgspec.Struct, frozen=True, kw_only=True):
    """Matching and replacement policy for active-to-selected links."""

    matching: Annotated[Matching, msgspec.Meta(title="Match")] = Matching.TYPE
    replace: bool = False


class Reroute(msgspec.Struct, frozen=True, kw_only=True):
    """Outputs taking a newly inserted reroute."""

    outputs: Outputs = Outputs.ALL


class Relabel(msgspec.Struct, frozen=True, kw_only=True):
    """Text edits applied to every selected label."""

    prefix: str = ""
    suffix: str = ""
    find: str = ""
    replace: str = ""


class CopyLabel(msgspec.Struct, frozen=True, kw_only=True):
    """Source of the copied labels."""

    source: LabelSource = LabelSource.ACTIVE


class Batch(msgspec.Struct, frozen=True, kw_only=True):
    """Native blend or math member assigned across the selection."""

    mode: Mode = MixBlend()


class Cycle(msgspec.Struct, frozen=True, kw_only=True):
    """Native enum and direction of a repeated step."""

    member: SteppedMember = SteppedMember.BLEND_TYPE
    direction: StepDirection = StepDirection.NEXT


class Factor(msgspec.Struct, frozen=True, kw_only=True):
    """Assignment or nudge of the first enabled mixing factor."""

    change: FactorChange = FactorChange.NUDGE
    value: Annotated[float, msgspec.Meta(ge=-1.0, le=1.0)] = 0.1


class FrameFamily(msgspec.Struct, frozen=True, kw_only=True):
    """Direction of selection through frame ownership."""

    relation: Annotated[Relation, msgspec.Meta(title="Select")] = Relation.PARENTS


class Chosen(msgspec.Struct, frozen=True, kw_only=True):
    """Node editor, supported tree, selection, and available windows."""

    view: RegionView
    space: bpy.types.SpaceNodeEditor
    tree: GraphTree
    nodes: tuple[bpy.types.Node, ...]
    active: Option[bpy.types.Node]
    windows: tuple[bpy.types.Window, ...]

    @classmethod
    def of(cls, context: bpy.types.Context) -> BlenderResult[Self]:
        """Returns supported node editor state or an absent editor or unsupported tree."""
        match context.space_data, context.window, context.area, context.window_manager:
            case bpy.types.SpaceNodeEditor() as space, bpy.types.Window() as window, bpy.types.Area() as area, bpy.types.WindowManager() as manager:
                match space.edit_tree:
                    case bpy.types.CompositorNodeTree() | bpy.types.GeometryNodeTree() | bpy.types.ShaderNodeTree() as tree:
                        return RegionView.of(Located(window=window, area=area)).map(
                            lambda view: cls(
                                view=view,
                                space=space,
                                tree=tree,
                                nodes=tuple(node for node in tree.nodes if node.select),
                                active=Option.of_optional(tree.nodes.active).filter(lambda node: node.select),
                                windows=tuple(manager.windows),
                            )
                        )
                    case other:
                        return Error((
                            BlenderChoiceFault(
                                path="edit_tree",
                                identifier=type(other).__name__,
                                accepted=tuple(kind.bl_rna.identifier for kind in (bpy.types.CompositorNodeTree, bpy.types.GeometryNodeTree, bpy.types.ShaderNodeTree)),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                            ),
                        ))
            case _:
                return Error((BlenderAbsentFault(owner=repr(context), key="SpaceNodeEditor.WINDOW"),))


class Gates(msgspec.Struct, frozen=True, kw_only=True):
    """Resolved native requirements shared by all edit rows."""

    editable: Setting
    picked: Setting
    nodes: Descriptor[bpy.types.CollectionProperty]
    active: tuple[Present, Setting]
    kinds: Setting

    def needs(self, least: int, most: Option[int] = Nothing, *more: Condition) -> Condition:
        """Returns editability and a native selection count with further requirements."""
        return AllOf(conditions=(self.editable, Count(collection=self.nodes, least=least, most=most, where=Some(self.picked)), *more))

    @property
    def populated(self) -> Condition:
        """Editability and at least one node, regardless of selection."""
        return AllOf(conditions=(self.editable, Count(collection=self.nodes)))

    @classmethod
    def resolved(cls) -> BlenderResult[Self]:
        """Accumulates independent native property resolutions once before row construction."""
        editor = bpy.types.SpaceNodeEditor
        flags = accumulated(
            Descriptor.resolve(root, path, bpy.types.BoolProperty) for root, path in ((editor, "edit_tree.is_editable"), (bpy.types.Node, "select"), (editor, "edit_tree.nodes.active.select"))
        )
        nodes = Descriptor.resolve(editor, "edit_tree.nodes", bpy.types.CollectionProperty)
        pointers = paired(Descriptor.resolve(editor, "edit_tree.nodes.active", bpy.types.PointerProperty), Descriptor.resolve(editor, "tree_type", bpy.types.EnumProperty))
        return paired(paired(flags, nodes), pointers).map(
            lambda values: cls(
                editable=Setting(descriptor=values[0][0][0]),
                picked=Setting(descriptor=values[0][0][1]),
                nodes=values[0][1],
                active=(Present(descriptor=values[1][0]), Setting(descriptor=values[0][0][2])),
                kinds=Setting(descriptor=values[1][1], accepted=tuple(kind.bl_rna.identifier for kind in (bpy.types.CompositorNodeTree, bpy.types.GeometryNodeTree, bpy.types.ShaderNodeTree))),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            )
        )


class NodeEdits(msgspec.Struct, frozen=True, kw_only=True):
    """Original operator declarations shared by registration, menus, and keymaps."""

    merge: Operator[Merge]
    link_active: Operator[LinkActive]
    swap: Operator[Empty]
    link_output: Operator[Empty]
    reroute: Operator[Reroute]
    relabel: Operator[Relabel]
    unlabel: Operator[Empty]
    copy_label: Operator[CopyLabel]
    batch: Operator[Batch]
    cycle: Operator[Cycle]
    factor: Operator[Factor]
    reset: Operator[Empty]
    copy_settings: Operator[Empty]
    cleanup: Operator[Empty]
    dissolve: Operator[Empty]
    align: Operator[Empty]
    family: Operator[FrameFamily]
    show: Operator[Empty]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the original nested declarations for central scoped registration."""
        return Ok(msgspec.structs.astuple(self))

    @classmethod
    def of(cls, spacing: NodeSpacing = NodeSpacing()) -> BlenderResult[Self]:  # ruff: ignore[function-call-in-default-argument]
        """Resolves gates and constructs each placed declaration exactly once."""
        match Gates.resolved():
            case Result(tag="ok", ok=gates):
                pass
            case Result(error=faults):
                return Error(faults)

        def ran[T: msgspec.Struct](edit: EditBody[T]) -> Callable[[Call[T]], BlenderResult[Done]]:
            return lambda call: Chosen.of(call.context).bind(lambda chosen: edit(call.record, chosen, spacing))

        picked = Some(gates.needs(1))
        active = Some(gates.needs(2, Nothing, *gates.active))
        populated = Some(gates.populated)
        return Ok(
            cls(
                merge=Operator(key="merge", title="Merge Nodes", description="Joins selected outputs through a merge family", form=Merge, condition=picked, run=ran(_merged)),
                link_active=Operator(
                    key="link_active", title="Link Active to Selected", description="Links active outputs into the selection", form=LinkActive, condition=active, run=ran(_linked_active)
                ),
                swap=Operator(key="swap", title="Swap Links", description="Exchanges outgoing links or matching input sources", form=Empty, condition=Some(gates.needs(1, Some(2))), run=ran(_swapped)),
                link_output=Operator(
                    key="link_output",
                    title="Link to Output",
                    description="Links the active node into the active tree output",
                    form=Empty,
                    condition=Some(AllOf(conditions=(gates.editable, *gates.active, gates.kinds))),
                    run=ran(_linked_output),
                ),
                reroute=Operator(key="reroute", title="Add Reroutes", description="Inserts a reroute for each chosen output", form=Reroute, condition=picked, run=ran(_rerouted)),
                relabel=Operator(key="relabel", title="Modify Labels", description="Rewrites selected node labels", form=Relabel, condition=picked, run=ran(_relabeled), invoke=Some(Properties())),
                unlabel=Operator(key="unlabel", title="Clear Labels", description="Clears selected node labels", form=Empty, condition=picked, run=ran(_unlabeled)),
                copy_label=Operator(
                    key="copy_label", title="Copy Label", description="Copies labels from active or connected nodes and sockets", form=CopyLabel, condition=picked, run=ran(_label_copied)
                ),
                batch=Operator(key="batch", title="Batch Change", description="Sets a native blend type or operation", form=Batch, condition=picked, run=ran(_batched)),
                cycle=Operator(key="cycle", title="Step Mode", description="Steps through a native member's enum items", form=Cycle, condition=picked, run=ran(_cycled), undo="Step Mode"),
                factor=Operator(key="factor", title="Change Factor", description="Sets or nudges the first enabled factor", form=Factor, condition=picked, run=ran(_factored), undo="Change Factor"),
                reset=Operator(key="reset", title="Reset Nodes", description="Restores declaration defaults while retaining links and data-blocks", form=Empty, condition=picked, run=ran(_defaulted)),
                copy_settings=Operator(
                    key="copy_settings", title="Copy Settings", description="Copies active settings to selected nodes of its type", form=Empty, condition=active, run=ran(_settings_copied)
                ),
                cleanup=Operator(
                    key="cleanup",
                    title="Delete Unused Nodes",
                    description="Removes nodes reaching no output and empty frames",
                    form=Empty,
                    condition=populated,
                    run=ran(_cleaned),
                    invoke=Some(
                        Confirm(
                            message=Some(lambda context, _: Chosen.of(context).map(lambda chosen: "Delete {} unused nodes and {} empty frames".format(*map(len, _swept(chosen.tree))))), icon="WARNING"
                        )
                    ),
                ),
                dissolve=Operator(
                    key="dissolve",
                    title="Dissolve Muted Nodes",
                    description="Removes muted nodes while connecting their internal pass-throughs",
                    form=Empty,
                    condition=populated,
                    run=ran(_dissolved),
                    invoke=Some(
                        Confirm(message=Some(lambda context, _: Chosen.of(context).map(lambda chosen: f"Dissolve {sum(node.mute for node in chosen.tree.nodes)} muted nodes")), icon="WARNING")
                    ),
                ),
                align=Operator(key="align", title="Align Nodes", description="Arranges the selection along its wider spread", form=Empty, condition=Some(gates.needs(2)), run=ran(_aligned)),
                family=Operator(
                    key="family", title="Select Parent or Children", description="Selects parent frames or their direct children", form=FrameFamily, condition=picked, run=ran(_frames_selected)
                ),
                show=Operator(
                    key="show",
                    title="Show Node Image",
                    description="Shows the active image in the largest image editor",
                    form=Empty,
                    condition=Some(AllOf(conditions=gates.active)),
                    run=ran(_image_shown),
                    undo=False,
                ),
            )
        )


class _Joining(msgspec.Struct, frozen=True, kw_only=True):
    kind: type[bpy.types.Node]
    joins: frozenset[str]
    settings: dict[str, Value] = msgspec.field(default_factory=dict)
    companion: Option[str] = Nothing
    factor: Option[float] = Nothing

    def merged(
        self, edit: "_Edit", chosen: Chosen, spacing: NodeSpacing, sources: tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...], upstream: frozenset[str]
    ) -> BlenderResult[tuple[bpy.types.Node, ...]]:
        joined = tuple((node, socket) for node, socket in sources if socket.type in self.joins)
        first = joined[0][1]
        moved = joined[1][1] if len(joined) == 2 and not any(link.is_valid for link in first.links) else first  # type: ignore[union-attr]  # ty: ignore[not-iterable]
        replaced = frozenset(link for link in moved.links if link.is_valid and link.to_node.name not in upstream)  # type: ignore[union-attr]  # ty: ignore[not-iterable, unresolved-attribute]
        right = max(node.location_absolute.x + NodeSize.of(node, chosen.view).width for node, _ in joined) + spacing.across

        def made() -> BlenderResult[bpy.types.Node]:
            match edit.added(self.kind):
                case Result(tag="ok", ok=node):
                    return accumulated(written(node, key, value) for key, value in self.settings.items()).map(lambda _: node)
                case Result(error=faults):
                    return Error(faults)

        def linked(node: bpy.types.Node, feeds: tuple[bpy.types.NodeSocket, ...]) -> BlenderResult[bpy.types.Node]:
            entries = tuple(entry for entry in node.inputs if entry.enabled and entry.type in self.joins)
            singles = tuple(entry for entry in entries if not entry.is_multi_input and not isinstance(node, bpy.types.GeometryNodeStringJoin))
            many = tuple(entry for entry in entries if entry.is_multi_input)[:1]
            incoming: dict[bpy.types.NodeSocket, list[Incoming]] = {}
            for target, source in zip((*singles, *(many * len(feeds))), feeds):
                incoming.setdefault(target, []).append((source, False))
            extra = tuple(
                (target, output)
                for companion in self.companion.to_list()
                for target, feed in zip((entry for entry in node.inputs if entry.enabled and entry.type == companion), feeds)
                for output in tuple(output for output in feed.node.outputs if output.enabled and output.type == companion and output.name in {"Z", "Depth"})[:1]  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )
            for target, output in extra:
                incoming.setdefault(target, []).append((output, False))
            at = right, fmean(source.node.location_absolute.y for source in feeds)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            return accumulated((
                *(edit.rewired(target, tuple(values)) for target, values in incoming.items()),
                *(
                    written(entry, "default_value", factor)
                    for factor in self.factor.to_list()
                    for entry in tuple(each for each in node.inputs if each.enabled and isinstance(each, bpy.types.NodeSocketFloatFactor))[:1]
                ),
                written(node, "location_absolute", at),
            )).map(lambda _: node)

        def chained(feed: bpy.types.NodeSocket, nodes: tuple[bpy.types.Node, ...]) -> BlenderResult[tuple[bpy.types.Node, ...]]:
            return made().bind(lambda node: linked(node, (next(output for output in nodes[-1].outputs if output.enabled), feed)).map(lambda node: (*nodes, node)))

        def finished(nodes: tuple[bpy.types.Node, ...]) -> BlenderResult[tuple[bpy.types.Node, ...]]:
            output = next(socket for socket in nodes[-1].outputs if socket.enabled)
            return edit.replaced({link: ((output, link.is_muted),) for link in replaced}).map(lambda _: nodes)

        match made():
            case Result(tag="ok", ok=root):
                feeds = tuple(socket for _, socket in joined)
                binary = not any(entry.enabled and entry.type in self.joins and entry.is_multi_input for entry in root.inputs)
                chain: BlenderResult[tuple[bpy.types.Node, ...]] = linked(root, feeds[:2] if binary else feeds).map(lambda node: (node,))
                return reduce(lambda result, feed: result.bind(partial(chained, feed)), feeds[2:] if binary else (), chain).bind(finished)
            case Result(error=faults):
                return Error(faults)


# --- [SERVICES] -------------------------------------------------------------------------


class _Edit(msgspec.Struct, frozen=True, kw_only=True):
    tree: GraphTree
    resources: ExitStack
    saved: set[bpy.types.NodeSocket] = msgspec.field(default_factory=set)
    hidden: set[bpy.types.NodeSocket] = msgspec.field(default_factory=set)

    def added[N: bpy.types.Node](self, kind: type[N]) -> BlenderResult[N]:
        def created() -> N:
            node: N = self.tree.nodes.new(kind.bl_rna.identifier)  # type: ignore[assignment, attr-defined]  # ty: ignore[invalid-assignment, unresolved-attribute]
            self.resources.callback(self.tree.nodes.remove, node)
            return node

        return catch(created, exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(self.tree.nodes), error=error),))

    def rewired(self, target: bpy.types.NodeSocket, incoming: tuple[Incoming, ...]) -> BlenderResult[tuple[bpy.types.NodeLink, ...]]:
        current = tuple(target.links)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        sockets: tuple[bpy.types.NodeSocket, ...] = tuple(
            dict.fromkeys((
                target,
                *(link.from_socket for link in current),  # type: ignore[arg-type]
                *(each.from_socket for each in incoming if isinstance(each, bpy.types.NodeLink)),  # type: ignore[arg-type]
                *(source for each in incoming if not isinstance(each, bpy.types.NodeLink) for source, _ in (each,)),
            ))
        )  # ty: ignore[invalid-assignment]
        for socket in sockets:
            if socket not in self.hidden:
                self.hidden.add(socket)
                self.resources.callback(setattr, socket, "hide", socket.hide)
        if target not in self.saved:
            self.saved.add(target)
            original: tuple[tuple[bpy.types.NodeSocket, bool, bool], ...] = tuple((link.from_socket, link.is_valid, link.is_muted) for link in current)  # type: ignore[misc]
            self.resources.callback(_restored, self.tree, target, original, tuple((socket, socket.hide) for socket in sockets))
        retained = {each for each in incoming if isinstance(each, bpy.types.NodeLink)}
        for link in current:
            if link not in retained:
                self.tree.links.remove(link)
        created: dict[int, bpy.types.NodeLink] = {}

        def added(request: tuple[int, bpy.types.NodeSocket, bool], socket: bpy.types.NodeSocket) -> BlenderResult[bpy.types.NodeSocket]:
            index, source, muted = request
            try:
                link = self.tree.links.new(source, socket, verify_limits=False, handle_dynamic_sockets=True)
            except RuntimeError as error:
                return Error((BlenderEditFault(request=repr(self.tree.links), error=error),))
            match link:
                case bpy.types.NodeLink():
                    created[index] = link
                    return written(link, "is_muted", muted).map(lambda _: link.to_socket)  # type: ignore[arg-type, return-value]  # ty: ignore[invalid-return-type]
                case None:
                    return Error((BlenderAbsentFault(owner=repr(self.tree.links), key=repr((source, socket))),))

        def ordered(socket: bpy.types.NodeSocket) -> tuple[bpy.types.NodeLink, ...]:
            desired = tuple(each if isinstance(each, bpy.types.NodeLink) else created[index] for index, each in enumerate(incoming))
            live = list(socket.links)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            positions = {link: index for index, link in enumerate(live)}
            for index, wanted in enumerate(desired):
                position, held = positions[wanted], live[index]
                if held != wanted:
                    held.swap_multi_input_sort_id(wanted)
                    live[index], live[position] = wanted, held
                    positions[wanted], positions[held] = index, position
            return desired

        requests = tuple((index, *each) for index, each in enumerate(incoming) if not isinstance(each, bpy.types.NodeLink))
        return reduce(lambda result, request: result.bind(partial(added, request)), reversed(requests), Result[bpy.types.NodeSocket, BlenderFaults](ok=target)).map(ordered)

    def committed[T](self, value: T) -> T:
        self.resources.pop_all()
        return value

    def replaced(self, replacements: Mapping[bpy.types.NodeLink, tuple[Incoming, ...]]) -> BlenderResult[None]:
        targets: tuple[bpy.types.NodeSocket, ...] = tuple(dict.fromkeys(link.to_socket for link in replacements))  # type: ignore[misc]  # ty: ignore[invalid-assignment]
        return accumulated(self.rewired(target, tuple(replacement for link in target.links for replacement in replacements.get(link, (link,)))) for target in targets).map(lambda _: None)  # type: ignore[union-attr]  # ty: ignore[not-iterable]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _restored(tree: GraphTree, target: bpy.types.NodeSocket, sources: tuple[tuple[bpy.types.NodeSocket, bool, bool], ...], hidden: tuple[tuple[bpy.types.NodeSocket, bool], ...]) -> None:
    for removed in tuple(target.links):  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        tree.links.remove(removed)
    for source, valid, muted in reversed(sources):
        link: bpy.types.NodeLink = tree.links.new(source, target, verify_limits=False)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        link.is_valid, link.is_muted = valid, muted
    for socket, state in hidden:
        socket.hide = state


@contextmanager
def _editing(chosen: Chosen) -> Generator[_Edit]:
    tree = chosen.tree
    selected = tuple((node, node.select) for node in tree.nodes)
    active = Option.of_optional(tree.nodes.active)
    interface: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    selected_items = tuple((item, item.select) for item in interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel))
    active_item = Option.of_optional(interface.active)

    def restored() -> None:
        original = frozenset(item for item, _ in selected_items)
        for item in reversed(tuple(interface.items_tree)):
            if item not in original:
                interface.remove(item)
        interface.active = active_item.to_optional()
        for item, state in selected_items:
            item.select = state
        for node, state in selected:
            node.select = state
        tree.nodes.active = active.to_optional()

    with ExitStack() as resources:
        resources.callback(restored)
        yield _Edit(tree=tree, resources=resources)


def _selected(tree: bpy.types.NodeTree, nodes: tuple[bpy.types.Node, ...], active: Option[bpy.types.Node] = Nothing) -> BlenderResult[None]:
    def activated(_: object) -> None:
        for node in active.to_list():
            tree.nodes.active = node

    return accumulated(written(node, "select", node in nodes) for node in tree.nodes).map(activated)


def _closure(starts: Iterable[str], edges: Mapping[str, tuple[str, ...]]) -> frozenset[str]:
    held: set[str] = set()
    pending = deque(starts)
    while pending:
        if (key := pending.popleft()) not in held:
            held.add(key)
            pending.extend(edges.get(key, ()))
    return frozenset(held)


def inserted(chosen: Chosen, group: NodeGroup, identity: Identity, spacing: NodeSpacing, *, inputs: Mapping[Key, Value] = {}) -> BlenderResult[GroupNode]:
    """Splices a group after the active output, retaining each consumer occurrence."""
    kind: type[GroupNode]
    match chosen.tree:
        case bpy.types.CompositorNodeTree():
            kind = bpy.types.CompositorNodeGroup
        case bpy.types.ShaderNodeTree():
            kind = bpy.types.ShaderNodeGroup
        case bpy.types.GeometryNodeTree():
            kind = bpy.types.GeometryNodeGroup

    def spliced[N: bpy.types.Node](node: N) -> BlenderResult[N]:
        sources = tuple(socket for active in chosen.active.to_list() for socket in tuple(each for each in active.outputs if each.enabled)[:1])
        at = chosen.active.map(lambda active: (active.location_absolute.x + NodeSize.of(active, chosen.view).width + spacing.across, active.location_absolute.y)).default_with(
            lambda: chosen.view.to_view(chosen.view.region.width / 2.0, chosen.view.region.height / 2.0)
        )

        def joined(source: bpy.types.NodeSocket) -> BlenderResult[None]:
            def socket(sockets: Iterable[bpy.types.NodeSocket]) -> BlenderResult[bpy.types.NodeSocket]:
                return Option.of_optional(next((each for each in sockets if each.enabled and each.type == source.type), None)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=repr(sockets), key=source.type),)
                )

            sockets = paired(socket(node.inputs), socket(node.outputs))
            links = tuple(link for link in source.links if link.is_valid)  # type: ignore[union-attr]  # ty: ignore[not-iterable]
            return sockets.bind(lambda pair: edit.rewired(pair[0], ((source, False),)).bind(lambda _: edit.replaced({link: ((pair[1], link.is_muted),) for link in links})))

        return (
            identity
            .name(Name.ATTACHMENT, "interface")
            .bind(lambda slot: valued(node, node.inputs, inputs, slot))
            .bind(lambda _: accumulated(joined(source) for source in sources))
            .bind(lambda _: written(node, "location_absolute", at))
            .bind(lambda _: _selected(chosen.tree, (node,), Some(node)))
            .map(lambda _: node)
        )

    with _editing(chosen) as edit:
        return (
            paired(group.made(identity), Descriptor.resolve(kind, "node_tree", bpy.types.PointerProperty))
            .bind(lambda pair: edit.added(kind).bind(lambda node: pair[1].write(node, pair[0]).bind(lambda _: spliced(node))))
            .map(edit.committed)
        )


def _merged(record: Merge, chosen: Chosen, spacing: NodeSpacing) -> BlenderResult[Done]:
    sources = tuple((node, socket) for node in sorted(chosen.nodes, key=lambda node: (-node.location_absolute.y, node.name)) for socket in tuple(each for each in node.outputs if each.enabled)[:1])
    integers = frozenset(
        item.identifier for prop in bpy.types.FunctionNodeIntegerMath.bl_rna.properties if isinstance(prop, bpy.types.EnumProperty) and prop.identifier == "operation" for item in prop.enum_items
    )
    types = frozenset(socket.type for _, socket in sources)

    def joining(socket: bpy.types.NodeSocket) -> Option[_Joining]:
        match record.merging, socket.type:
            case Automatic(), "RGBA" | "VALUE" | "INT" if "RGBA" in types:
                return Some(_Joining(kind=bpy.types.ShaderNodeMix, joins=frozenset({"RGBA", "VALUE", "INT"}), settings={"data_type": "RGBA"}))
            case Automatic(), "VALUE" | "INT" if "VALUE" in types:
                return Some(_Joining(kind=bpy.types.ShaderNodeMath, joins=frozenset({"VALUE", "INT"})))
            case Automatic(), "VALUE" | "INT" | "VECTOR" | "BOOLEAN" | "STRING" | "SHADER" | "GEOMETRY" as native:
                return Some(
                    _Joining(
                        kind={
                            "VALUE": bpy.types.ShaderNodeMath,
                            "INT": bpy.types.FunctionNodeIntegerMath,
                            "VECTOR": bpy.types.ShaderNodeVectorMath,
                            "BOOLEAN": bpy.types.FunctionNodeBooleanMath,
                            "STRING": bpy.types.GeometryNodeStringJoin,
                            "SHADER": bpy.types.ShaderNodeMixShader,
                            "GEOMETRY": bpy.types.GeometryNodeJoinGeometry,
                        }[native],
                        joins=frozenset({native}),
                    )
                )
            case Automatic(), _:
                return Nothing
            case MixBlend(blend=blend), _:
                return Some(
                    _Joining(
                        kind=bpy.types.ShaderNodeMix, joins=frozenset({"RGBA", "VALUE", "INT"}), settings={"data_type": "RGBA", "blend_type": blend}, factor=Nothing if blend == "MIX" else Some(1.0)
                    )
                )
            case MathOperation(operation=operation), "INT" if operation in integers and types <= {"INT"}:
                return Some(_Joining(kind=bpy.types.FunctionNodeIntegerMath, joins=frozenset({"INT"}), settings={"operation": operation}))
            case MathOperation(operation=operation), _:
                return Some(_Joining(kind=bpy.types.ShaderNodeMath, joins=frozenset({"VALUE", "INT"}), settings={"operation": operation}))
            case MixShader(), _:
                return Some(_Joining(kind=bpy.types.ShaderNodeMixShader, joins=frozenset({"SHADER"})))
            case AddShader(), _:
                return Some(_Joining(kind=bpy.types.ShaderNodeAddShader, joins=frozenset({"SHADER"})))
            case Join(), _:
                return Some(_Joining(kind=bpy.types.GeometryNodeJoinGeometry, joins=frozenset({"GEOMETRY"})))
            case MeshBoolean(operation=operation), _:
                return Some(_Joining(kind=bpy.types.GeometryNodeMeshBoolean, joins=frozenset({"GEOMETRY"}), settings={"operation": operation}))
            case DepthCombine(), _:
                return Some(_Joining(kind=bpy.types.CompositorNodeZcombine, joins=frozenset({"RGBA"}), companion=Some("VALUE")))
            case Alpha(), _:
                return Some(_Joining(kind=bpy.types.CompositorNodeAlphaOver, joins=frozenset({"RGBA"})))

    if not (families := {family.kind: family for _, socket in sources for family in joining(socket).to_list() if socket.type in family.joins}):
        return Ok(Done(notes=(("INFO", "The selected nodes hold no output this merge joins"),)))

    upstream = _closure(
        (node.name for node in chosen.nodes),
        {node.name: tuple(link.from_node.name for link in chosen.tree.links if link.is_valid and link.to_node == node) for node in chosen.tree.nodes},  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    )
    joins = frozenset(native for family in families.values() for native in family.joins)
    skipped = sum(not any(socket.enabled and socket.type in joins for socket in node.outputs) for node in chosen.nodes)
    with _editing(chosen) as edit:
        return (
            accumulated(family.merged(edit, chosen, spacing, sources, upstream) for family in families.values())
            .bind(lambda groups: _selected(chosen.tree, tuple(node for group in groups for node in group), Some(groups[-1][-1]) if groups else Nothing))
            .map(lambda _: Done(notes=tuple(("INFO", f"{count} nodes hold no output this merge joins") for count in (skipped,) if count)))
            .map(edit.committed)
        )


def _linked_active(record: LinkActive, chosen: Chosen, _: NodeSpacing) -> BlenderResult[Done]:
    def pair(active: bpy.types.Node, target: bpy.types.Node) -> Option[tuple[bpy.types.NodeSocket, bpy.types.NodeSocket]]:
        outputs = tuple(output for output in active.outputs if output.enabled)
        inputs = tuple(entry for entry in target.inputs if entry.enabled and (record.replace or not entry.is_linked))
        match record.matching:
            case Matching.TYPE:
                pairs = ((output, entry) for entry in inputs for output in outputs if isinstance(target, bpy.types.NodeReroute) or entry.type == output.type)
            case Matching.NAME | Matching.OUTPUT:
                name = target.label or target.name
                pairs = (
                    (output, entry)
                    for output in outputs
                    for entry in inputs
                    if (name == (active.label or active.name) if record.matching is Matching.NAME else output.name == name) and (isinstance(target, bpy.types.NodeReroute) or entry.type == output.type)
                )
        return Option.of_optional(next(pairs, None))

    pairs = tuple(pair(active, target) for active in chosen.active.to_list() for target in chosen.nodes if target != active)
    with _editing(chosen) as edit:
        return (
            accumulated(edit.rewired(target, ((source, False),)) for found in pairs for source, target in found.to_list())
            .map(lambda _: Done(notes=tuple(("INFO", f"{count} nodes take no link") for count in (sum(found.is_none() for found in pairs),) if count)))
            .map(edit.committed)
        )


def _swapped(_: Empty, chosen: Chosen, __: NodeSpacing) -> BlenderResult[Done]:
    with _editing(chosen) as edit:
        match chosen.nodes:
            case (first, second):
                peers = {first: second, second: first}
                held = tuple(link for link in chosen.tree.links if link.is_valid and link.from_node in peers)
                replacements = {link: Option.of_optional(next((output for output in peers[link.from_node].outputs if output.identifier == link.from_socket.identifier), None)) for link in held}  # type: ignore[index, union-attr]  # ty: ignore[invalid-argument-type, unresolved-attribute]
                return (
                    edit
                    .replaced({link: output.map(lambda socket: ((socket, link.is_muted),)).default_value((link,)) for link, output in replacements.items()})
                    .map(
                        lambda _: Done(notes=tuple(("INFO", f"{count} links found no output of the same identifier") for count in (sum(value.is_none() for value in replacements.values()),) if count))
                    )
                    .map(edit.committed)
                )
            case (node,):
                entries = tuple(entry for entry in node.inputs if entry.enabled and entry.is_linked and not entry.is_multi_input)
                pair = next(((left, right) for index, left in enumerate(entries) for right in entries[index + 1 :] if left.type == right.type), tuple(entries) if len(entries) == 2 else None)
                match pair:
                    case (left, right):
                        feeds: tuple[tuple[bpy.types.NodeSocket, bool], tuple[bpy.types.NodeSocket, bool]] = (
                            (left.links[0].from_socket, left.links[0].is_muted),  # type: ignore[index]  # ty: ignore[not-subscriptable]
                            (right.links[0].from_socket, right.links[0].is_muted),  # type: ignore[index]  # ty: ignore[not-subscriptable]
                        )  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                        return paired(edit.rewired(left, (feeds[1],)), edit.rewired(right, (feeds[0],))).map(lambda _: Done()).map(edit.committed)
                    case _ if len(entries) == 1:
                        source = entries[0]
                        inputs = tuple(entry for entry in node.inputs if entry.enabled and not entry.is_multi_input)
                        index = inputs.index(source)
                        target = Option.of_optional(next((entry for entry in (*inputs[index + 1 :], *inputs[:index]) if not entry.is_linked), None))
                        return target.map(
                            lambda found: (
                                edit.rewired(found, ((source.links[0].from_socket, source.links[0].is_muted),)).bind(lambda _: edit.rewired(source, ())).map(lambda _: Done()).map(edit.committed)  # type: ignore[index, arg-type]  # ty: ignore[not-subscriptable, invalid-argument-type]
                            )
                        ).default_value(Ok(Done(notes=(("INFO", "No unlinked input takes the connection"),))))
                    case _:
                        return Ok(Done(notes=(("INFO", "No linked inputs can be swapped"),)))
            case _:
                return Ok(Done(notes=(("INFO", "Swap takes one or two selected nodes"),)))


def _linked_output(_: Empty, chosen: Chosen, spacing: NodeSpacing) -> BlenderResult[Done]:
    kind: type[Terminal]
    match chosen.space.id:
        case bpy.types.Material():
            kind = bpy.types.ShaderNodeOutputMaterial
        case bpy.types.World():
            kind = bpy.types.ShaderNodeOutputWorld
        case bpy.types.Light():
            kind = bpy.types.ShaderNodeOutputLight
        case bpy.types.NodeTree():
            kind = bpy.types.NodeGroupOutput
        case other:
            return Error((BlenderAbsentFault(owner=repr(other), key="id"),))

    def linked(target: bpy.types.Node) -> BlenderResult[Done]:
        entries = tuple(entry for entry in target.inputs if entry.enabled)
        outputs = tuple(
            output for active in chosen.active.to_list() for output in active.outputs if output.enabled and (not isinstance(chosen.tree, bpy.types.GeometryNodeTree) or output.type == "GEOMETRY")
        )
        output = Option.of_optional(next((output for output in outputs for entry in entries[:1] if output.type == entry.type), None)).or_else(Option.of_optional(next(iter(outputs), None)))
        pair = output.bind(
            lambda found: (
                Option
                .of_optional(next((entry for entry in entries if entry.name == found.name and entry.type == found.type), None))
                .or_else(Option.of_optional(next((entry for entry in entries if entry.type == found.type), None)))
                .or_else(Option.of_optional(next((entry for entry in entries if isinstance(entry, bpy.types.NodeSocketVirtual)), None)))
                .or_else(Option.of_optional(next(iter(entries), None)))
                .map(lambda entry: (found, entry))
            )
        )
        return accumulated(edit.rewired(target, ((source, False),)) for source, target in pair.to_list()).map(
            lambda _: Done(notes=() if pair.is_some() else (("INFO", "The active node holds no output the target takes"),))
        )

    def made() -> BlenderResult[Terminal]:
        return edit.added(kind).bind(
            lambda target: (
                accumulated(
                    written(target, "location_absolute", (active.location_absolute.x + NodeSize.of(active, chosen.view).width + spacing.across, active.location_absolute.y))
                    for active in chosen.active.to_list()
                )
                .bind(lambda _: _selected(chosen.tree, (target,), Some(target)))
                .map(lambda _: target)
            )
        )

    with _editing(chosen) as edit:
        return Option.of_optional(next((node for node in chosen.tree.nodes if isinstance(node, kind) and node.is_active_output), None)).map(Ok).default_with(made).bind(linked).map(edit.committed)


def _rerouted(record: Reroute, chosen: Chosen, spacing: NodeSpacing) -> BlenderResult[Done]:
    def outputs(node: bpy.types.Node) -> tuple[bpy.types.NodeSocket, ...]:
        shown = tuple(output for output in node.outputs if not output.is_unavailable and not isinstance(output, bpy.types.NodeSocketVirtual))
        match record.outputs:
            case Outputs.ALL:
                return shown
            case Outputs.LOOSE:
                return tuple(output for output in shown if not output.is_linked)
            case Outputs.LINKED:
                return tuple(output for output in shown if output.is_linked)

    def placed(node: bpy.types.Node, source: bpy.types.NodeSocket, index: int, count: int) -> BlenderResult[bpy.types.Node]:
        size = NodeSize.of(node, chosen.view)
        at = node.location_absolute.x + size.width + spacing.across, node.location_absolute.y - size.height * (index + 1) / (count + 1)
        links = tuple(link for link in source.links if link.is_valid)  # type: ignore[union-attr]  # ty: ignore[not-iterable]
        return edit.added(bpy.types.NodeReroute).bind(
            lambda reroute: (
                written(reroute, "location_absolute", at)
                .bind(lambda _: edit.rewired(reroute.inputs[0], ((source, False),)))
                .bind(lambda _: edit.replaced({link: ((reroute.outputs[0], link.is_muted),) for link in links}))
                .map(lambda _: reroute)
            )
        )

    steps = tuple((node, output, index, len(shown)) for node in chosen.nodes for shown in (outputs(node),) for index, output in enumerate(shown))
    with _editing(chosen) as edit:
        return (
            accumulated(starmap(placed, steps)).bind(lambda nodes: _selected(chosen.tree, nodes)).map(lambda _: Done()).map(edit.committed)
            if steps
            else Ok(Done(notes=(("INFO", "No selected node holds a chosen output"),)))
        )


def _relabeled(record: Relabel, chosen: Chosen, _: NodeSpacing) -> BlenderResult[Done]:
    return accumulated(written(node, "label", f"{record.prefix}{node.label.replace(record.find, record.replace) if record.find else node.label}{record.suffix}") for node in chosen.nodes).map(
        lambda _: Done()
    )


def _unlabeled(_: Empty, chosen: Chosen, __: NodeSpacing) -> BlenderResult[Done]:
    return accumulated(written(node, "label", "") for node in chosen.nodes).map(lambda _: Done())


def _label_copied(record: CopyLabel, chosen: Chosen, _: NodeSpacing) -> BlenderResult[Done]:
    def label(node: bpy.types.Node) -> Option[str]:
        fed: Option[bpy.types.NodeLink] = Option.of_optional(next((link for entry in node.inputs for link in entry.links if link.is_valid), None))  # type: ignore[union-attr]  # ty: ignore[not-iterable]
        match record.source:
            case LabelSource.ACTIVE:
                return chosen.active.map(lambda active: active.label)
            case LabelSource.NODE:
                return fed.map(lambda link: link.from_node.label or link.from_node.name)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            case LabelSource.SOCKET:
                return fed.map(lambda link: link.from_socket.name)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

    labels = tuple((node, label(node)) for node in chosen.nodes)
    return accumulated(written(node, "label", value) for node, found in labels for value in found.to_list()).map(
        lambda _: Done(notes=tuple(("INFO", f"{count} nodes found no label") for count in (sum(found.is_none() for _, found in labels),) if count))
    )


def _batched(record: Batch, chosen: Chosen, _: NodeSpacing) -> BlenderResult[Done]:
    match record.mode:
        case MixBlend(blend=value):
            member = SteppedMember.BLEND_TYPE
        case MathOperation(operation=value):
            member = SteppedMember.OPERATION
    nodes = tuple(node for node in chosen.nodes if member.name.lower() in node.bl_rna.properties)
    return accumulated(written(node, member.name.lower(), value) for node in nodes).map(
        lambda _: Done(notes=tuple(("INFO", f"{count} nodes hold no {member.label}") for count in (len(chosen.nodes) - len(nodes),) if count))
    )


def _cycled(record: Cycle, chosen: Chosen, _: NodeSpacing) -> BlenderResult[Done]:
    name, step = record.member.name.lower(), 1 if record.direction is StepDirection.NEXT else -1
    props = tuple((node, prop) for node in chosen.nodes for prop in node.bl_rna.properties if isinstance(prop, bpy.types.EnumProperty) and prop.identifier == name)
    return accumulated(
        written(node, name, items[(items.index(getattr(node, name)) + step) % len(items)]) for node, prop in props for items in (tuple(item.identifier for item in prop.enum_items),)
    ).map(lambda _: Done(notes=tuple(("INFO", f"{count} nodes hold no {record.member.label}") for count in (len(chosen.nodes) - len(props),) if count)))


def _factored(record: Factor, chosen: Chosen, _: NodeSpacing) -> BlenderResult[Done]:
    factors = tuple(socket for node in chosen.nodes for socket in tuple(each for each in node.inputs if each.enabled)[:1] if isinstance(socket, bpy.types.NodeSocketFloatFactor))
    return accumulated(written(socket, "default_value", record.value if record.change is FactorChange.SET else socket.default_value + record.value) for socket in factors).map(
        lambda _: Done(notes=tuple(("INFO", f"{count} nodes hold no factor") for count in (len(chosen.nodes) - len(factors),) if count))
    )


def _transferred(source: bpy.types.Node, target: bpy.types.Node, *, pointers: bool) -> BlenderResult[None]:
    own = tuple(
        Descriptor(root=type(source), owner="", prop=prop)
        for prop in source.bl_rna.properties
        if prop.identifier not in bpy.types.Node.bl_rna.properties and (pointers or not (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.ID)))
    )

    def copied(_: object) -> BlenderResult[None]:
        held = {socket.identifier: socket for socket in source.inputs}
        return accumulated(
            Descriptor(root=type(socket), owner="", prop=prop).write(socket, Descriptor(root=type(held[socket.identifier]), owner="", prop=prop).read(held[socket.identifier]))
            for socket in target.inputs
            if socket.enabled and not socket.is_linked and socket.identifier in held
            for prop in socket.bl_rna.properties
            if prop.identifier == "default_value"
        ).map(lambda _: None)

    return Snapshot.capture(source, Some(own)).bind(lambda snapshot: snapshot.apply(target).outcome).bind(copied)


def _defaulted(_: Empty, chosen: Chosen, __: NodeSpacing) -> BlenderResult[Done]:
    def reset(node: bpy.types.Node) -> BlenderResult[None]:
        pointers = tuple(
            Descriptor(root=type(node), owner="", prop=prop)
            for prop in node.bl_rna.properties
            if isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.ID) and not prop.is_readonly  # type: ignore[unreachable]
        )
        with _editing(chosen) as edit:
            return edit.added(type(node)).bind(
                lambda fresh: accumulated(descriptor.write(fresh, descriptor.read(node)) for descriptor in pointers).bind(lambda _: _transferred(fresh, node, pointers=False))
            )

    return accumulated(reset(node) for node in chosen.nodes).map(lambda _: Done())


def _settings_copied(_: Empty, chosen: Chosen, __: NodeSpacing) -> BlenderResult[Done]:
    targets = tuple((active, node) for active in chosen.active.to_list() for node in chosen.nodes if node != active and node.bl_idname == active.bl_idname)
    return accumulated(_transferred(active, node, pointers=True) for active, node in targets).map(
        lambda _: Done(notes=tuple(("INFO", f"{count} nodes are of another type") for count in (len(chosen.nodes) - 1 - len(targets),) if count))
    )


def _swept(tree: bpy.types.NodeTree) -> tuple[tuple[bpy.types.Node, ...], tuple[bpy.types.NodeFrame, ...]]:
    nodes = tuple(node for node in tree.nodes if not isinstance(node, bpy.types.NodeFrame))
    kept = _closure(
        (node.name for node in nodes if not node.outputs or isinstance(node, (bpy.types.NodeGroupInput, bpy.types.CompositorNodeLevels, bpy.types.GeometryNodeWarning))),
        {node.name: tuple(link.from_node.name for link in tree.links if link.is_valid and link.to_node == node) for node in nodes},  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    )

    def lineage(node: bpy.types.Node) -> tuple[str, ...]:
        return Option.of_optional(node.parent).map(lambda parent: (parent.name, *lineage(parent))).default_value(())

    frames = frozenset(name for node in nodes if node.name in kept for name in lineage(node))
    return tuple(node for node in nodes if node.name not in kept), tuple(node for node in tree.nodes if isinstance(node, bpy.types.NodeFrame) and node.name not in frames)


def _cleaned(_: Empty, chosen: Chosen, __: NodeSpacing) -> BlenderResult[Done]:
    nodes, frames = _swept(chosen.tree)
    for node in (*nodes, *frames):
        chosen.tree.nodes.remove(node)
    return Ok(Done(notes=(("INFO", f"Deleted {len(nodes)} unused nodes and {len(frames)} empty frames"),)))


def _dissolved(_: Empty, chosen: Chosen, __: NodeSpacing) -> BlenderResult[Done]:
    muted = tuple(node for node in chosen.tree.nodes if node.mute)

    def dissolved(node: bpy.types.Node) -> BlenderResult[None]:
        crossings: dict[bpy.types.NodeSocket, tuple[bpy.types.NodeSocket, ...]] = {
            passed.to_socket: tuple(link.from_socket for link in passed.from_socket.links if link.is_valid)  # type: ignore[misc, union-attr]  # ty: ignore[not-iterable, unresolved-attribute]
            for passed in node.internal_links
        }  # ty: ignore[invalid-assignment]
        with _editing(chosen) as edit:
            return (
                edit
                .replaced({link: tuple((source, link.is_muted) for source in sources) for output, sources in crossings.items() for link in output.links if link.is_valid})  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                .map(lambda _: edit.committed(None))
                .map(lambda _: chosen.tree.nodes.remove(node))
            )

    return reduce(lambda result, node: result.bind(lambda _: dissolved(node)), muted, Result[None, BlenderFaults](ok=None)).map(
        lambda _: Done(notes=(("INFO", f"Dissolved {len(muted)} muted nodes"),))
    )


def _aligned(_: Empty, chosen: Chosen, spacing: NodeSpacing) -> BlenderResult[Done]:
    sizes = {node: NodeSize.of(node, chosen.view) for node in chosen.nodes}
    centers = {node: (node.location_absolute.x + sizes[node].width / 2.0, node.location_absolute.y - sizes[node].height / 2.0) for node in chosen.nodes}
    xs, ys = zip(*centers.values())
    horizontal = max(xs) - min(xs) >= max(ys) - min(ys)
    sign, gap = (1.0, spacing.across) if horizontal else (-1.0, spacing.down)
    length = {node: size.width if horizontal else size.height for node, size in sizes.items()}
    along = {node: x if horizontal else y for node, (x, y) in centers.items()}
    ranked = sorted(centers, key=lambda node: (sign * along[node], node.name))
    laid = {node: sign * (start + length[node] / 2.0) for node, start in zip(ranked, accumulate((length[node] + gap for node in ranked), initial=0.0))}
    anchor = chosen.active.map(lambda node: along[node] - laid[node]).default_with(lambda: fmean(along.values()) - fmean(laid.values()))
    middle = fmean(ys if horizontal else xs)
    return accumulated(
        written(node, "location_absolute", ((laid[node] + anchor if horizontal else middle) - sizes[node].width / 2.0, (middle if horizontal else laid[node] + anchor) + sizes[node].height / 2.0))
        for node in chosen.nodes
    ).map(lambda _: Done())


def _frames_selected(record: FrameFamily, chosen: Chosen, _: NodeSpacing) -> BlenderResult[Done]:
    nodes: tuple[bpy.types.Node, ...]
    match record.relation:
        case Relation.PARENTS:
            nodes = tuple(dict.fromkeys(parent for node in chosen.nodes for parent in Option.of_optional(node.parent).to_list()))
        case Relation.CHILDREN:
            frames = frozenset(node for node in chosen.nodes if isinstance(node, bpy.types.NodeFrame))
            nodes = tuple(node for node in chosen.tree.nodes if node.parent in frames)
    return accumulated(written(node, "select", True) for node in nodes).map(lambda _: Done())


def _image_shown(_: Empty, chosen: Chosen, __: NodeSpacing) -> BlenderResult[Done]:
    def image(node: bpy.types.Node) -> Option[IdName]:
        member = (
            Option
            .of_optional(node.bl_rna.properties.get("image"))
            .map(lambda prop: Descriptor(root=type(node), owner="", prop=prop).read(node))
            .bind(lambda value: Some(value) if isinstance(value, IdName) else Nothing)
        )
        socket = Option.of_optional(next((entry.default_value for entry in node.inputs if isinstance(entry, bpy.types.NodeSocketImage) and entry.default_value), None)).map(IdName.of)
        match node:
            case bpy.types.CompositorNodeViewer():
                kind: Option[str] = Some("COMPOSITING")
            case bpy.types.CompositorNodeRLayers():
                kind = Some("RENDER_RESULT")
            case _:
                kind = Nothing
        result = kind.bind(lambda wanted: Option.of_optional(next((held for held in bpy.data.images if held.type == wanted), None))).map(IdName.of)
        return member.or_else(socket).or_else(result)

    match chosen.active.to_optional():
        case bpy.types.Node() as node:
            match image(node).to_optional():
                case IdName() as named:
                    return (
                        paired(EditorState.of(bpy.types.SpaceImageEditor, {"image": named}), AreaQuery(space="IMAGE_EDITOR").first(chosen.windows))
                        .bind(lambda pair: pair[0].applied(pair[1].area))
                        .map(lambda _: Done())
                    )
                case None:
                    return Error((BlenderAbsentFault(owner=repr(node), key="image"),))
        case None:
            return Error((BlenderAbsentFault(owner=repr(chosen.tree.nodes), key="active"),))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = [
    "AddShader",
    "Alpha",
    "Automatic",
    "Batch",
    "Chosen",
    "CopyLabel",
    "Cycle",
    "DepthCombine",
    "EditBody",
    "Factor",
    "FactorChange",
    "FrameFamily",
    "Gates",
    "GroupNode",
    "Join",
    "LabelSource",
    "LinkActive",
    "Matching",
    "MathOperation",
    "Merge",
    "Merging",
    "MeshBoolean",
    "MixBlend",
    "MixShader",
    "Mode",
    "NodeEdits",
    "Outputs",
    "Relabel",
    "Relation",
    "Reroute",
    "StepDirection",
    "SteppedMember",
    "Terminal",
    "blends",
    "booleans",
    "inserted",
    "operations",
]
