"""Node editor state and owned group insertion under rollback of links and selection."""

from collections.abc import Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import reduce
import typing
from typing import Self

import bpy
from expression import Error, Ok, Option
import msgspec

from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderResult
from blender.bpy.nodes.changes import made, valued
from blender.bpy.nodes.graphs import NodeGroup
from blender.bpy.nodes.interface import addresses, slot
from blender.bpy.nodes.placement import NodeSize, NodeSpacing
from blender.bpy.nodes.trees import created, GraphTree, GROUP_NODES, grouped, joined, ordered
from blender.bpy.ui.screens.areas import Located
from blender.bpy.ui.screens.regions import RegionView
from blender.imaging.faults import accumulated, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Incoming = bpy.types.NodeLink | tuple[bpy.types.NodeSocket, bool]

# --- [MODELS] ---------------------------------------------------------------------------


class Chosen(msgspec.Struct, frozen=True, kw_only=True):
    """Node editor region, supported tree, and selected active node."""

    view: RegionView
    tree: GraphTree
    active: Option[bpy.types.Node]

    @classmethod
    def of(cls, context: bpy.types.Context) -> BlenderResult[Self]:
        """Returns editor state, or fault for missing editor or unsupported tree."""
        accepted: tuple[type[GraphTree], ...] = typing.get_args(GraphTree.__value__)
        match context.space_data, context.window, context.area:
            case bpy.types.SpaceNodeEditor() as space, bpy.types.Window() as window, bpy.types.Area() as area:
                match space.edit_tree:
                    case tree if isinstance(tree, accepted):
                        return RegionView.of(Located(window=window, area=area)).map(
                            lambda view: cls(view=view, tree=tree, active=Option.of_optional(tree.nodes.active).filter(lambda node: node.select))
                        )
                    case None:
                        return Error((BlenderAbsentFault(owner=repr(space), key="edit_tree"),))
                    case other:
                        return Error((BlenderChoiceFault(path="edit_tree", identifier=type(other).__name__, accepted=tuple(kind.__name__ for kind in accepted)),))
            case _:
                return Error((BlenderAbsentFault(owner=repr(context), key="SpaceNodeEditor.WINDOW"),))


# --- [SERVICES] -------------------------------------------------------------------------


class _Edit(msgspec.Struct, frozen=True, kw_only=True):
    tree: GraphTree
    resources: ExitStack
    saved: set[bpy.types.NodeSocket] = msgspec.field(default_factory=set)
    hidden: set[bpy.types.NodeSocket] = msgspec.field(default_factory=set)

    def added(self, kind: type[bpy.types.Node]) -> BlenderResult[bpy.types.Node]:
        def retained(node: bpy.types.Node) -> bpy.types.Node:
            self.resources.callback(self.tree.nodes.remove, node)
            return node

        return created(self.tree, kind).map(retained)

    def rewired(self, target: bpy.types.NodeSocket, incoming: tuple[Incoming, ...]) -> BlenderResult[tuple[bpy.types.NodeLink, ...]]:
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
            done: BlenderResult[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]], request: tuple[int, bpy.types.NodeSocket, bool]
        ) -> BlenderResult[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]]:
            index, source, muted = request

            def linked(held: tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]], link: bpy.types.NodeLink) -> tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]:
                return link.to_socket, {**held[1], index: link}  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

            return done.bind(
                lambda held: joined(self.tree, source, held[0], verify_limits=False, handle_dynamic_sockets=True).bind(lambda link: written(link, "is_muted", muted).map(lambda _: linked(held, link)))
            )

        def finished(held: tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]) -> tuple[bpy.types.NodeLink, ...]:
            desired = tuple(each if isinstance(each, bpy.types.NodeLink) else held[1][index] for index, each in enumerate(incoming))
            ordered(held[0], desired)
            return desired

        requests = tuple((index, *each) for index, each in enumerate(incoming) if not isinstance(each, bpy.types.NodeLink))
        start: BlenderResult[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]] = Ok((target, {}))
        return reduce(added, reversed(requests), start).map(finished)

    def committed[T](self, value: T) -> T:
        self.resources.pop_all()
        return value

    def replaced(self, replacements: Mapping[bpy.types.NodeLink, tuple[Incoming, ...]]) -> BlenderResult[None]:
        targets: tuple[bpy.types.NodeSocket, ...] = tuple(dict.fromkeys(link.to_socket for link in replacements))  # type: ignore[misc]  # ty: ignore[invalid-assignment]
        return accumulated(self.rewired(target, tuple(replacement for link in target.links for replacement in replacements.get(link, (link,)))) for target in targets).map(lambda _: None)  # type: ignore[union-attr]  # ty: ignore[not-iterable]


# --- [OPERATIONS] -----------------------------------------------------------------------


def inserted(chosen: Chosen, group: NodeGroup, identity: Identity, spacing: NodeSpacing, *, inputs: Mapping[Key, Value] = {}) -> BlenderResult[bpy.types.Node]:
    """Splices group after active node's first enabled output, routing each consumer link through group."""

    def spliced(node: bpy.types.Node, tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.Node]:
        sources = tuple(socket for active in chosen.active.to_list() for socket in tuple(each for each in active.outputs if each.enabled)[:1])
        at = chosen.active.map(lambda active: (active.location_absolute.x + NodeSize.of(active, chosen.view).width + spacing.across, active.location_absolute.y)).default_with(
            lambda: chosen.view.to_view(chosen.view.region.width / 2.0, chosen.view.region.height / 2.0)
        )

        def linked(source: bpy.types.NodeSocket) -> BlenderResult[None]:
            def socket(sockets: Iterable[bpy.types.NodeSocket]) -> BlenderResult[bpy.types.NodeSocket]:
                return Option.of_optional(next((each for each in sockets if each.enabled and each.type == source.type), None)).to_result_with(
                    lambda: (BlenderAbsentFault(owner=repr(sockets), key=source.type),)
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
        yield _Edit(tree=tree, resources=resources)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Chosen", "inserted"]
