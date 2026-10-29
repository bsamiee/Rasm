"""Owned group insertion into an edited tree under rollback of links and selection."""

from collections.abc import Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import reduce
from types import MappingProxyType
from typing import Final

import bpy
from expression import Ok, Option
from expression.collections import Block

from blender.bpy.data.ids.content import endpoints
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.addressing import Addressed, slot
from blender.bpy.nodes.changes import made, valued
from blender.bpy.nodes.desired import NodeGroup
from blender.bpy.nodes.placement import measured
from blender.bpy.nodes.trees import created, grouped, joined, links_of, native, ordered
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.trees import Gaps

# --- [TYPES] ----------------------------------------------------------------------------

type Incoming = bpy.types.NodeLink | tuple[bpy.types.NodeSocket, bool]

# --- [CONSTANTS] ------------------------------------------------------------------------

_GAPS: Final = Gaps()

# --- [SERVICES] -------------------------------------------------------------------------


class _Edit:
    __slots__ = ("hidden", "resources", "saved", "tree")

    def __init__(self, tree: bpy.types.NodeTree, resources: ExitStack) -> None:
        self.tree, self.resources = tree, resources
        self.saved: set[bpy.types.NodeSocket] = set()
        self.hidden: set[bpy.types.NodeSocket] = set()

    def added(self, kind: type[bpy.types.Node]) -> Outcome[bpy.types.Node]:
        def retained(node: bpy.types.Node) -> bpy.types.Node:
            self.resources.callback(self.tree.nodes.remove, node)
            return node

        return created(self.tree, kind).map(retained)

    def rewired(self, target: bpy.types.NodeSocket, incoming: tuple[Incoming, ...]) -> Outcome[tuple[bpy.types.NodeLink, ...]]:
        current = links_of(target)
        sockets: tuple[bpy.types.NodeSocket, ...] = tuple(
            dict.fromkeys((target, *(link.from_socket for link in current), *(each.from_socket if isinstance(each, bpy.types.NodeLink) else each[0] for each in incoming)))  # type: ignore[arg-type]
        )  # ty: ignore[invalid-assignment]
        for socket in sockets:
            if socket not in self.hidden:
                self.hidden.add(socket)
                self.resources.callback(setattr, socket, "hide", socket.hide)
        if target not in self.saved:
            self.saved.add(target)

            def outlet(link: bpy.types.NodeLink) -> bpy.types.NodeSocket:
                source: bpy.types.NodeSocket = link.from_socket  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                return source

            original = tuple((outlet(link), link.is_valid, link.is_muted) for link in current)

            def restored() -> None:
                for removed in links_of(target):
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
                lambda held: joined(self.tree, source, held[0], verify_limits=False, handle_dynamic_sockets=True).bind(
                    lambda link: written(link, ("is_muted",), muted).map(lambda _: linked(held, link))
                )
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
        def forwarded(target: bpy.types.NodeSocket) -> tuple[Incoming, ...]:
            return tuple(replacement for link in links_of(target) for replacement in replacements.get(link, (link,)))

        targets = tuple(dict.fromkeys(endpoints(link)[3] for link in replacements))
        return accumulated(self.rewired(target, forwarded(target)) for target in targets).map(lambda _: None)


# --- [OPERATIONS] -----------------------------------------------------------------------


def inserted(
    tree: bpy.types.NodeTree,
    after: Option[bpy.types.Node],
    cursor: tuple[float, float],
    group: NodeGroup,
    identity: Identity,
    *,
    gaps: Gaps = _GAPS,
    inputs: Mapping[Key, Value] = MappingProxyType({}),
) -> Outcome[bpy.types.Node]:
    """Splices group after a node's first enabled output, routing each consumer link through group, at the cursor when no node leads.

    Args:
        tree: Edited tree, such as a node editor's `edit_tree`.
        after: Node the group follows, such as the selected active node.
        cursor: Canvas position the group takes when no node leads.
        group: Owned group to make and place.
        identity: Extension owning the group.
        gaps: Spacing after the leading node.
        inputs: Group node input values by interface item key.

    Returns:
        Placed group node, selected and active. `Error` of make, link, or write faults, every edit rolled back.
    """

    def spliced(node: bpy.types.Node, owned: bpy.types.NodeTree) -> Outcome[bpy.types.Node]:
        sources = tuple(socket for active in after.to_list() for socket in tuple(each for each in active.outputs if each.enabled)[:1])
        at = after.map(lambda active: (active.location_absolute.x + measured(active).width + gaps.across, active.location_absolute.y)).default_value(cursor)

        def linked(source: bpy.types.NodeSocket) -> Outcome[None]:
            def socket(sockets: Iterable[bpy.types.NodeSocket]) -> Outcome[bpy.types.NodeSocket]:
                return (
                    Block
                    .of_seq(sockets)
                    .filter(lambda each: each.enabled and each.type == source.type)
                    .try_head()
                    .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(sockets), member=source.type))
                )

            links = tuple(link for link in links_of(source) if link.is_valid)
            return paired(socket(node.inputs), socket(node.outputs)).bind(
                lambda pair: edit.rewired(pair[0], ((source, False),)).bind(lambda _: edit.replaced({link: ((pair[1], link.is_muted),) for link in links}))
            )

        def activated(_: object) -> bpy.types.Node:
            tree.nodes.active = node
            return node

        return (
            slot(identity)
            .bind(lambda name: valued(node.inputs, inputs, Addressed.of(owned, name).sockets))
            .bind(lambda _: accumulated(linked(source) for source in sources))
            .bind(lambda _: written(node, ("location_absolute",), at))
            .bind(lambda _: accumulated(written(each, ("select",), each == node) for each in tree.nodes))
            .map(activated)
        )

    with _editing(tree) as edit:
        return (
            native(type(tree), bpy.types.ShaderNodeGroup)
            .bind(lambda kind: made(group, identity).bind(lambda owned: edit.added(kind).bind(lambda node: grouped(node, owned).bind(lambda _: spliced(node, owned)))))
            .map(edit.committed)
        )


@contextmanager
def _editing(tree: bpy.types.NodeTree) -> Generator[_Edit]:
    selected, active = tuple((node, node.select) for node in tree.nodes), tree.nodes.active

    def restored() -> None:
        for node, state in selected:
            node.select = state
        tree.nodes.active = active

    with ExitStack() as resources:
        resources.callback(restored)
        yield _Edit(tree, resources)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["inserted"]
