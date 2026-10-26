"""Tree and node classes, holders, sockets, node and link creation, link order, tree walks, and content fingerprints."""

from collections import deque
from collections.abc import Iterable, Mapping
import functools
import itertools
from types import MappingProxyType
from typing import Final, overload

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids import images
from blender.bpy.data.ids.store import embedded, IdName
from blender.bpy.data.rna.descriptors import Descriptor, member_path, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.records.faults import accumulated, DomainFault, paired
from blender.records.identity import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------

type GraphTree = bpy.types.CompositorNodeTree | bpy.types.GeometryNodeTree | bpy.types.ShaderNodeTree
GroupNode = bpy.types.CompositorNodeGroup | bpy.types.ShaderNodeGroup | bpy.types.GeometryNodeGroup
OutputNode = bpy.types.ShaderNodeOutputMaterial | bpy.types.ShaderNodeOutputWorld | bpy.types.ShaderNodeOutputLight | bpy.types.NodeGroupOutput
type Holder = bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light

# --- [CONSTANTS] ------------------------------------------------------------------------

GROUP_NODES: Final[Mapping[type[GraphTree], type[GroupNode]]] = MappingProxyType({
    bpy.types.CompositorNodeTree: bpy.types.CompositorNodeGroup,
    bpy.types.ShaderNodeTree: bpy.types.ShaderNodeGroup,
    bpy.types.GeometryNodeTree: bpy.types.GeometryNodeGroup,
})

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [EDITS]


@overload
def tree_of(holder: bpy.types.Material | bpy.types.World | bpy.types.Light) -> BlenderResult[bpy.types.ShaderNodeTree]: ...
@overload
def tree_of(holder: bpy.types.ID) -> BlenderResult[bpy.types.NodeTree]: ...
def tree_of(holder: bpy.types.ID) -> BlenderResult[bpy.types.NodeTree]:
    """Returns group itself or embedded tree, `absent` fault for holder without one."""
    return (Some(holder) if isinstance(holder, bpy.types.NodeTree) else embedded(holder)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(holder), key=bpy.types.NodeTree.__name__),))


def emptied[T: bpy.types.ID](holder: T) -> BlenderResult[T]:
    """Removes every node of holder tree, returning holder."""
    return tree_of(holder).map(lambda tree: tree.nodes.clear()).map(lambda _: holder)


def socket_at(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, identifier: str) -> BlenderResult[bpy.types.NodeSocket]:
    """Finds socket by identifier alone.

    Args:
        sockets: Inputs or outputs of one node.
        identifier: Socket identifier, stable where names repeat or translate.

    Returns:
        `Ok` of socket. `Error` of `absent` for identifier with no socket.
    """
    return Option.of_optional(next((socket for socket in sockets if socket.identifier == identifier), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(sockets), key=identifier),))


def created[N: bpy.types.Node](tree: bpy.types.NodeTree, kind: type[N]) -> BlenderResult[N]:
    """Adds node by `bpy.types` class name.

    Returns:
        `Ok` of node. `Error` of `edit` for refused class, `choice` for alias add-ons rebind to another class, its node removed.
    """
    identifier = kind.__name__

    def narrowed(made: bpy.types.Node) -> BlenderResult[N]:
        match made:
            case kind():
                return Ok(made)
            case _:
                replaced = made.bl_idname
                tree.nodes.remove(made)
                return Error((DomainFault(subject=repr(tree.nodes), value=replaced, accepted=identifier),))

    made: BlenderResult[bpy.types.Node] = catch(lambda: tree.nodes.new(identifier), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(tree.nodes), error=error),))
    return Result[bpy.types.Node, BlenderFaults].bind(made, narrowed)


def grouped(node: bpy.types.Node, group: bpy.types.NodeTree) -> BlenderResult[None]:
    """Points group node at group, `edit` fault for group containing node's tree."""
    host: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return (
        Error((BlenderEditFault(request=f"{node!r}.node_tree", error=ValueError(f"{group.name} contains {host.name}")),))
        if group.contains_tree(host)
        else written(node, "node_tree", group).map(lambda _: None)
    )


def joined(
    tree: bpy.types.NodeTree, source: bpy.types.NodeSocket, target: bpy.types.NodeSocket, *, verify_limits: bool = True, handle_dynamic_sockets: bool = False
) -> BlenderResult[bpy.types.NodeLink]:
    """Links output socket to input socket.

    Args:
        tree: Tree holding both sockets.
        source: Output socket.
        target: Input socket, virtual extend socket under `handle_dynamic_sockets`.
        verify_limits: Whether link replaces links past target limit.
        handle_dynamic_sockets: Whether virtual target grows socket link ends at.

    Returns:
        `Ok` of valid link. `Error` of `edit` for refused link or invalid link the call removes, `absent` when host returns none.
    """

    def answered(link: bpy.types.NodeLink | None) -> BlenderResult[bpy.types.NodeLink]:
        match link:
            case bpy.types.NodeLink(is_valid=True):
                return Ok(link)
            case bpy.types.NodeLink():
                tree.links.remove(link)
                return Error((BlenderEditFault(request=repr(tree.links), error=ValueError(f"Link from {source!r} to {target!r} is invalid")),))
            case None:
                return Error((BlenderAbsentFault(owner=repr(tree.links), key=repr((source, target))),))

    linked: BlenderResult[bpy.types.NodeLink | None] = catch(
        lambda: tree.links.new(source, target, verify_limits=verify_limits, handle_dynamic_sockets=handle_dynamic_sockets), exception=RuntimeError
    )().map_error(lambda error: (BlenderEditFault(request=repr(tree.links), error=error),))
    return Result[bpy.types.NodeLink | None, BlenderFaults].bind(linked, answered)


def ordered(socket: bpy.types.NodeSocket, desired: tuple[bpy.types.NodeLink, ...]) -> None:
    """Orders multi-input links top to bottom within their slots by sort ID swaps."""
    type Plan = tuple[tuple[bpy.types.NodeLink, ...], tuple[tuple[bpy.types.NodeLink, bpy.types.NodeLink], ...]]

    def planned(state: Plan, step: tuple[int, bpy.types.NodeLink]) -> Plan:
        (live, swaps), (index, wanted) = state, step
        held, position = live[index], live.index(wanted)
        return (live, swaps) if held == wanted else (tuple(wanted if at == index else held if at == position else link for at, link in enumerate(live)), (*swaps, (held, wanted)))

    start: Plan = (tuple(link for link in socket.links if link in desired), ())  # type: ignore[union-attr]  # ty: ignore[not-iterable]
    _, swaps = functools.reduce(planned, enumerate(desired), start)
    for held, wanted in swaps:
        held.swap_multi_input_sort_id(wanted)


# --- [WALKS]


def active_output(tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.NodeGroupOutput]:
    """Returns active Group Output node."""
    return Option.of_optional(next((node for node in tree.nodes if isinstance(node, bpy.types.NodeGroupOutput) and node.is_active_output), None)).to_result_with(
        lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=bpy.types.NodeGroupOutput.__name__),)
    )


def nested[T: bpy.types.NodeTree](roots: Iterable[T], *, muted: bool = True) -> tuple[T, ...]:
    """Returns roots and every group tree their group nodes reach, each once in first-reached breadth-first order.

    Args:
        roots: Trees to walk from.
        muted: Whether muted group nodes lead into their groups.
    """
    pending = deque(roots)
    reached: dict[T, None] = {}
    while pending:
        if (tree := pending.popleft()) not in reached:
            reached[tree] = None
            pending.extend(child for node in tree.nodes if isinstance(node, GroupNode) and (muted or not node.mute) for child in (node.node_tree,) if isinstance(child, type(tree)))
    return tuple(reached)


def upstream(node: bpy.types.Node) -> tuple[bpy.types.Node, ...]:
    """Returns node and every node its input links reach, sorted by name."""
    pending, seen = [node], set[bpy.types.Node]()
    while pending:
        if (current := pending.pop()) not in seen:
            seen.add(current)
            pending.extend(link.from_node for socket in current.inputs for link in socket.links)  # type: ignore[misc,union-attr]  # ty: ignore[invalid-argument-type,not-iterable]
    return tuple(sorted(seen, key=lambda item: item.name))


def fingerprinted(owner: bpy.types.NodeTree | bpy.types.Node) -> BlenderResult[Option[ContentHash]]:
    """Hashes observable content of tree or node upstream graph.

    Args:
        owner: Whole tree, or node whose `upstream` closure answers.

    Returns:
        `Ok` of digest over persistent settings, socket defaults, ordered links, nested groups, and image content.
        `Ok` of `Nothing` for unobservable image dependency. `Error` of capture faults.
    """
    tree: bpy.types.NodeTree = owner if isinstance(owner, bpy.types.NodeTree) else owner.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    nodes = tuple(sorted(owner.nodes, key=lambda item: item.name)) if isinstance(owner, bpy.types.NodeTree) else upstream(owner)
    settings = Descriptor.saved(tree, lambda prop: not isinstance(prop, bpy.types.CollectionProperty))

    def captured(struct: "bpy.types.bpy_struct[object]", selection: Option[tuple[Descriptor, ...]] = Nothing) -> BlenderResult[tuple[Snapshot, tuple[Option[ContentHash], ...]]]:
        def referenced(snapshot: Snapshot) -> BlenderResult[tuple[Snapshot, tuple[Option[ContentHash], ...]]]:
            blocks = dict.fromkeys(
                block
                for at, held in snapshot.tree.owners.items()
                for name, value in held.values.items()
                if isinstance(value, IdName)
                for block in (struct.path_resolve(member_path(at, name)),)
                if isinstance(block, bpy.types.NodeTree | bpy.types.Image)
            )
            return accumulated(fingerprinted(block) if isinstance(block, bpy.types.NodeTree) else images.fingerprinted(block) for block in blocks).map(lambda hashes: (snapshot, hashes))

        return Snapshot.capture(struct, selection).bind(referenced)

    def linked(link: bpy.types.NodeLink) -> BlenderResult[ContentHash]:
        endpoints = msgspec.msgpack.encode((link.from_node.name, link.from_socket.identifier, link.to_node.name, link.to_socket.identifier))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        return Snapshot.capture(link, Some(Descriptor.saved(link, lambda prop: not prop.is_readonly))).map(lambda snapshot: content_hash(bpy.types.NodeLink, endpoints, snapshot))

    return paired(
        accumulated(itertools.starmap(captured, ((tree, Some(settings)), *((item, Nothing) for item in nodes)))),
        accumulated(linked(link) for item in nodes for socket in item.inputs for link in socket.links),  # type: ignore[union-attr]  # ty: ignore[not-iterable]
    ).map(
        lambda parts: functools.reduce(
            lambda held, part: held.map2(lambda prior, current: content_hash(type(owner), prior, current), part),
            (key for _, hashes in parts[0] for key in hashes),
            Some(content_hash(type(owner), *(snapshot for snapshot, _ in parts[0]), *parts[1])),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "GROUP_NODES",
    "GraphTree",
    "GroupNode",
    "Holder",
    "OutputNode",
    "active_output",
    "created",
    "emptied",
    "fingerprinted",
    "grouped",
    "joined",
    "nested",
    "ordered",
    "socket_at",
    "tree_of",
    "upstream",
]
