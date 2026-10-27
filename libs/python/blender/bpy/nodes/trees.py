"""Tree and node classes, holders, sockets, node and link creation, link order, and tree walks."""

from collections.abc import Iterable, Mapping
import functools
from functools import partial
from types import MappingProxyType
from typing import Final, overload

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch

from blender.bpy.data.ids.store import embedded
from blender.bpy.data.rna.descriptors import written
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome, RecordReason

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
def tree_of(holder: bpy.types.Material | bpy.types.World | bpy.types.Light) -> Outcome[bpy.types.ShaderNodeTree]: ...
@overload
def tree_of(holder: bpy.types.ID) -> Outcome[bpy.types.NodeTree]: ...
def tree_of(holder: bpy.types.ID) -> Outcome[bpy.types.NodeTree]:
    """Returns group itself or embedded tree, `absent` fault for holder without one."""
    return (Some(holder) if isinstance(holder, bpy.types.NodeTree) else embedded(holder)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(holder), member=bpy.types.NodeTree.__name__))


def emptied[T: bpy.types.ID](holder: T) -> Outcome[T]:
    """Removes every node of holder tree, returning holder."""
    return tree_of(holder).map(lambda tree: tree.nodes.clear()).map(lambda _: holder)


def socket_at(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, identifier: str | int) -> Outcome[bpy.types.NodeSocket]:
    """Finds socket by identifier, or by position among the sockets the node's mode members enable.

    Args:
        sockets: Inputs or outputs of one node.
        identifier: Socket identifier, stable where names repeat or translate, or position among enabled sockets.

    Returns:
        `Ok` of socket. `Error` of `absent` for an identifier or position with no socket.
    """
    match identifier:
        case int(position):
            found = Block.of_seq(sockets).filter(lambda socket: socket.enabled).skip(position).try_head()
        case str():
            found = Block.of_seq(sockets).filter(lambda socket: socket.identifier == identifier).try_head()
    return found.to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(sockets), member=str(identifier)))


def created[N: bpy.types.Node](tree: bpy.types.NodeTree, kind: type[N]) -> Outcome[N]:
    """Adds node by `bpy.types` class name.

    Returns:
        `Ok` of node. `Error` of `edit` for refused class, `choice` for alias add-ons rebind to another class, its node removed.
    """
    identifier = kind.__name__

    def narrowed(made: bpy.types.Node) -> Outcome[N]:
        match made:
            case kind():
                return Ok(made)
            case _:
                replaced = made.bl_idname
                tree.nodes.remove(made)
                return Error(Fault.of(RecordReason.OUTSIDE, repr(tree.nodes), replaced, member=identifier))

    return catch(tree.nodes.new, exception=RuntimeError)(identifier).map_error(functools.partial(Fault.of, BlenderReason.REFUSED, repr(tree.nodes))).bind(narrowed)


def grouped(node: bpy.types.Node, group: bpy.types.NodeTree) -> Outcome[None]:
    """Points group node at group, `edit` fault for group containing node's tree."""
    host: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return (
        Error(Fault.of(BlenderReason.REFUSED, f"{node!r}.node_tree", ValueError(f"{group.name} contains {host.name}")))
        if group.contains_tree(host)
        else written(node, "node_tree", group).map(lambda _: None)
    )


def joined(tree: bpy.types.NodeTree, source: bpy.types.NodeSocket, target: bpy.types.NodeSocket, *, verify_limits: bool = True, handle_dynamic_sockets: bool = False) -> Outcome[bpy.types.NodeLink]:
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

    def answered(link: bpy.types.NodeLink | None) -> Outcome[bpy.types.NodeLink]:
        match link:
            case bpy.types.NodeLink(is_valid=True):
                return Ok(link)
            case bpy.types.NodeLink():
                tree.links.remove(link)
                return Error(Fault.of(BlenderReason.REFUSED, repr(tree.links), ValueError(f"Link from {source!r} to {target!r} is invalid")))
            case None:
                return Error(Fault.of(BlenderReason.ABSENT, repr(tree.links), member=repr((source, target))))

    linked: Outcome[bpy.types.NodeLink | None] = catch(tree.links.new, exception=RuntimeError)(source, target, verify_limits=verify_limits, handle_dynamic_sockets=handle_dynamic_sockets).map_error(
        functools.partial(Fault.of, BlenderReason.REFUSED, repr(tree.links))
    )
    return linked.bind(answered)


def ordered(socket: bpy.types.NodeSocket, desired: tuple[bpy.types.NodeLink, ...]) -> Outcome[None]:
    """Orders multi-input links top to bottom within their slots by sort ID swaps, `edit` fault for the first swap the host refuses."""
    type Plan = tuple[tuple[bpy.types.NodeLink, ...], tuple[tuple[bpy.types.NodeLink, bpy.types.NodeLink], ...]]

    def planned(state: Plan, step: tuple[int, bpy.types.NodeLink]) -> Plan:
        (live, swaps), (index, wanted) = state, step
        held, position = live[index], live.index(wanted)
        return (live, swaps) if held == wanted else (tuple(wanted if at == index else held if at == position else link for at, link in enumerate(live)), (*swaps, (held, wanted)))

    start: Plan = (tuple(link for link in socket.links if link in desired), ())  # type: ignore[union-attr]  # ty: ignore[not-iterable]
    _, swaps = functools.reduce(planned, enumerate(desired), start)
    return functools.reduce(
        lambda done, pair: done.bind(lambda _: catch(pair[0].swap_multi_input_sort_id, exception=RuntimeError)(pair[1]).map_error(functools.partial(Fault.of, BlenderReason.REFUSED, repr(pair[0])))),
        swaps,
        Result[None, Faults](ok=None),
    )


# --- [WALKS]


def active_output(tree: bpy.types.NodeTree) -> Outcome[bpy.types.NodeGroupOutput]:
    """Returns active Group Output node."""
    return (
        Block
        .of_seq(tree.nodes)
        .choose(lambda node: Option.of_optional(try_downcast(bpy.types.NodeGroupOutput, node)))
        .filter(lambda node: node.is_active_output)
        .try_head()
        .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tree.nodes), member=bpy.types.NodeGroupOutput.__name__))
    )


def nested[T: bpy.types.NodeTree](roots: Iterable[T], *, muted: bool = True) -> tuple[T, ...]:
    """Returns roots and every group tree their group nodes reach, each once in first-reached breadth-first order.

    Args:
        roots: Trees to walk from.
        muted: Whether muted group nodes lead into their groups.
    """

    def step(frontier: tuple[tuple[T, ...], frozenset[T]]) -> Option[tuple[T, tuple[tuple[T, ...], frozenset[T]]]]:
        pending, seen = frontier
        match tuple(dict.fromkeys(tree for tree in pending if tree not in seen)):
            case (tree, *rest):
                children = tuple(child for node in tree.nodes if isinstance(node, GroupNode) and (muted or not node.mute) for child in (node.node_tree,) if isinstance(child, type(tree)))
                return Some((tree, ((*rest, *children), seen | {tree})))
            case _:
                return Nothing

    start: tuple[tuple[T, ...], frozenset[T]] = (tuple(roots), frozenset())
    return tuple(Block.unfold(step, start))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["GROUP_NODES", "GraphTree", "GroupNode", "Holder", "OutputNode", "active_output", "created", "emptied", "grouped", "joined", "nested", "ordered", "socket_at", "tree_of"]
