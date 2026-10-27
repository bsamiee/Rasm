"""Node editor state and owned group insertion under rollback of links and selection."""

from collections.abc import Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import reduce
from types import MappingProxyType
import typing
from typing import Final, Self

import bpy
from expression import Error, Ok, Option
from expression.collections import Block
import msgspec

from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import made, valued
from blender.bpy.nodes.graphs import NodeGroup
from blender.bpy.nodes.interface import addresses, slot
from blender.bpy.nodes.placement import measured
from blender.bpy.nodes.trees import created, GraphTree, GROUP_NODES, grouped, joined, ordered
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.trees import Gaps

# --- [TYPES] ----------------------------------------------------------------------------

type Incoming = bpy.types.NodeLink | tuple[bpy.types.NodeSocket, bool]

# --- [CONSTANTS] ------------------------------------------------------------------------

_GAPS: Final = Gaps()

# --- [MODELS] ---------------------------------------------------------------------------


class Chosen(msgspec.Struct, frozen=True, kw_only=True):
    """Node editor cursor, supported tree, and selected active node."""

    cursor: tuple[float, float]
    tree: GraphTree
    active: Option[bpy.types.Node]

    @classmethod
    def of(cls, context: bpy.types.Context) -> Outcome[Self]:
        """Returns editor state, or fault for missing editor or unsupported tree."""
        accepted: tuple[type[GraphTree], ...] = typing.get_args(GraphTree.__value__)
        match context.space_data:
            case bpy.types.SpaceNodeEditor(cursor_location=cursor) as space:
                match space.edit_tree:
                    case tree if isinstance(tree, accepted):
                        return Ok(cls(cursor=(cursor.x, cursor.y), tree=tree, active=Option.of_optional(tree.nodes.active).filter(lambda node: node.select)))
                    case None:
                        return Error(Fault.of(BlenderReason.ABSENT, repr(space), member="edit_tree"))
                    case other:
                        return Error(Fault.of(RecordReason.OUTSIDE, "edit_tree", type(other).__name__, member=", ".join(kind.__name__ for kind in accepted)))
            case _:
                return Error(Fault.of(BlenderReason.ABSENT, repr(context), member="SpaceNodeEditor.WINDOW"))


# --- [SERVICES] -------------------------------------------------------------------------


class _Edit:
    __slots__ = ("hidden", "resources", "saved", "tree")

    def __init__(self, tree: GraphTree, resources: ExitStack) -> None:
        self.tree, self.resources = tree, resources
        self.saved: set[bpy.types.NodeSocket] = set()
        self.hidden: set[bpy.types.NodeSocket] = set()

    def added(self, kind: type[bpy.types.Node]) -> Outcome[bpy.types.Node]:
        def retained(node: bpy.types.Node) -> bpy.types.Node:
            self.resources.callback(self.tree.nodes.remove, node)
            return node

        return created(self.tree, kind).map(retained)

    def rewired(self, target: bpy.types.NodeSocket, incoming: tuple[Incoming, ...]) -> Outcome[tuple[bpy.types.NodeLink, ...]]:
        current = tuple(target.links)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        sockets: tuple[bpy.types.NodeSocket, ...] = tuple(
            dict.fromkeys((target, *(link.from_socket for link in current), *(each.from_socket if isinstance(each, bpy.types.NodeLink) else each[0] for each in incoming)))  # type: ignore[arg-type]
        )  # ty: ignore[invalid-assignment]
        for socket in sockets:
            if socket not in self.hidden:
                self.hidden.add(socket)
                self.resources.callback(setattr, socket, "hide", socket.hide)
        if target not in self.saved:
            self.saved.add(target)
            original: tuple[tuple[bpy.types.NodeSocket, bool, bool], ...] = tuple((link.from_socket, link.is_valid, link.is_muted) for link in current)  # type: ignore[misc]

            def restored() -> None:
                for removed in tuple(target.links):  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    self.tree.links.remove(removed)
                for link, valid, muted in (
                    (link, valid, muted) for source, valid, muted in reversed(original) for link in Option.of_optional(self.tree.links.new(source, target, verify_limits=False)).to_list()
                ):
                    link.is_valid, link.is_muted = valid, muted

            self.resources.callback(restored)
        retained = {each for each in incoming if isinstance(each, bpy.types.NodeLink)}
        for link in current:
            if link not in retained:
                self.tree.links.remove(link)

        def added(
            done: Outcome[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]], request: tuple[int, bpy.types.NodeSocket, bool]
        ) -> Outcome[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]]:
            index, source, muted = request

            def linked(held: tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]], link: bpy.types.NodeLink) -> tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]:
                return link.to_socket, {**held[1], index: link}  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

            return done.bind(
                lambda held: joined(self.tree, source, held[0], verify_limits=False, handle_dynamic_sockets=True).bind(lambda link: written(link, "is_muted", muted).map(lambda _: linked(held, link)))
            )

        def finished(held: tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]) -> Outcome[tuple[bpy.types.NodeLink, ...]]:
            desired = tuple(each if isinstance(each, bpy.types.NodeLink) else held[1][index] for index, each in enumerate(incoming))
            return ordered(held[0], desired).map(lambda _: desired)

        requests = tuple((index, *each) for index, each in enumerate(incoming) if not isinstance(each, bpy.types.NodeLink))
        start: Outcome[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]] = Ok((target, {}))
        return reduce(added, reversed(requests), start).bind(finished)

    def committed[T](self, value: T) -> T:
        self.resources.pop_all()
        return value

    def replaced(self, replacements: Mapping[bpy.types.NodeLink, tuple[Incoming, ...]]) -> Outcome[None]:
        targets: tuple[bpy.types.NodeSocket, ...] = tuple(dict.fromkeys(link.to_socket for link in replacements))  # type: ignore[misc]  # ty: ignore[invalid-assignment]
        return accumulated(self.rewired(target, tuple(replacement for link in target.links for replacement in replacements.get(link, (link,)))) for target in targets).map(lambda _: None)  # type: ignore[union-attr]  # ty: ignore[not-iterable]


# --- [OPERATIONS] -----------------------------------------------------------------------


def inserted(chosen: Chosen, group: NodeGroup, identity: Identity, *, gaps: Gaps = _GAPS, inputs: Mapping[Key, Value] = MappingProxyType({})) -> Outcome[bpy.types.Node]:
    """Splices group after active node's first enabled output, routing each consumer link through group."""

    def spliced(node: bpy.types.Node, tree: bpy.types.NodeTree) -> Outcome[bpy.types.Node]:
        sources = tuple(socket for active in chosen.active.to_list() for socket in tuple(each for each in active.outputs if each.enabled)[:1])
        at = chosen.active.map(lambda active: (active.location_absolute.x + measured(active).width + gaps.across, active.location_absolute.y)).default_value(chosen.cursor)

        def linked(source: bpy.types.NodeSocket) -> Outcome[None]:
            def socket(sockets: Iterable[bpy.types.NodeSocket]) -> Outcome[bpy.types.NodeSocket]:
                return (
                    Block
                    .of_seq(sockets)
                    .filter(lambda each: each.enabled and each.type == source.type)
                    .try_head()
                    .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(sockets), member=source.type))
                )

            links = tuple(link for link in source.links if link.is_valid)  # type: ignore[union-attr]  # ty: ignore[not-iterable]
            return paired(socket(node.inputs), socket(node.outputs)).bind(
                lambda pair: edit.rewired(pair[0], ((source, False),)).bind(lambda _: edit.replaced({link: ((pair[1], link.is_muted),) for link in links}))
            )

        def activated(_: object) -> bpy.types.Node:
            chosen.tree.nodes.active = node
            return node

        return (
            slot(identity)
            .bind(lambda name: valued(node.inputs, inputs, addresses(tree, name)))
            .bind(lambda _: accumulated(linked(source) for source in sources))
            .bind(lambda _: written(node, "location_absolute", at))
            .bind(lambda _: accumulated(written(each, "select", each == node) for each in chosen.tree.nodes))
            .map(activated)
        )

    with _editing(chosen) as edit:
        return made(group, identity).bind(lambda tree: edit.added(GROUP_NODES[type(chosen.tree)]).bind(lambda node: grouped(node, tree).bind(lambda _: spliced(node, tree)))).map(edit.committed)


@contextmanager
def _editing(chosen: Chosen) -> Generator[_Edit]:
    tree = chosen.tree
    selected, active = tuple((node, node.select) for node in tree.nodes), tree.nodes.active

    def restored() -> None:
        for node, state in selected:
            node.select = state
        tree.nodes.active = active

    with ExitStack() as resources:
        resources.callback(restored)
        yield _Edit(tree, resources)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Chosen", "inserted"]
