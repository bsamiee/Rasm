"""Native node trees: group and output node classes, holders, sockets, node and link creation, link order, and content fingerprints."""

import functools
import itertools
from typing import overload

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids import images
from blender.bpy.data.ids.store import embedded
from blender.bpy.data.rna.descriptors import Descriptor, IdName, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import content_hash, ContentHash

# --- [TYPES] ----------------------------------------------------------------------------

type GraphTree = bpy.types.CompositorNodeTree | bpy.types.GeometryNodeTree | bpy.types.ShaderNodeTree
GroupNode = bpy.types.CompositorNodeGroup | bpy.types.ShaderNodeGroup | bpy.types.GeometryNodeGroup
OutputNode = bpy.types.ShaderNodeOutputMaterial | bpy.types.ShaderNodeOutputWorld | bpy.types.ShaderNodeOutputLight | bpy.types.NodeGroupOutput
type Holder = bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light

# --- [OPERATIONS] -----------------------------------------------------------------------


def group_node(kind: type[GraphTree]) -> type[GroupNode]:
    """Returns the group node class a tree class nests its groups through."""
    groups: dict[type[GraphTree], type[GroupNode]] = {
        bpy.types.CompositorNodeTree: bpy.types.CompositorNodeGroup,
        bpy.types.ShaderNodeTree: bpy.types.ShaderNodeGroup,
        bpy.types.GeometryNodeTree: bpy.types.GeometryNodeGroup,
    }
    return groups[kind]


@overload
def tree_of(holder: bpy.types.Material | bpy.types.World | bpy.types.Light) -> BlenderResult[bpy.types.ShaderNodeTree]: ...
@overload
def tree_of(holder: bpy.types.ID) -> BlenderResult[bpy.types.NodeTree]: ...
def tree_of(holder: bpy.types.ID) -> BlenderResult[bpy.types.NodeTree]:
    """Returns a group itself, else the node tree its holder embeds, or the holder's absent tree."""
    return (Some(holder) if isinstance(holder, bpy.types.NodeTree) else embedded(holder)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(holder), key=bpy.types.NodeTree.__name__),))


def emptied[T: bpy.types.ID](holder: T) -> BlenderResult[T]:
    """Removes every node of the tree a holder embeds, returning the holder."""
    return tree_of(holder).map(lambda tree: tree.nodes.clear()).map(lambda _: holder)


def socket_at(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, identifier: str) -> BlenderResult[bpy.types.NodeSocket]:
    """Finds a socket by identifier alone, never by name.

    Args:
        sockets: Inputs or outputs of one node.
        identifier: Native socket identifier, stable where names repeat or translate.

    Returns:
        `Ok` of the socket; `Error` of `absent` for an identifier the node holds no socket under.
    """
    return Option.of_optional(next((socket for socket in sockets if socket.identifier == identifier), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(sockets), key=identifier),))


def created[N: bpy.types.Node](tree: bpy.types.NodeTree, kind: type[N]) -> BlenderResult[N]:
    """Adds one node of a native class to a tree by the name `bpy.types` publishes it under, since add-ons rebind class-level `bl_rna`, its refusal answering `edit`, and a node of the other class an alias maps to removed again and answering `choice`."""
    identifier = kind.__name__

    def narrowed(made: bpy.types.Node) -> BlenderResult[N]:
        match made:
            case kind():
                return Ok(made)
            case _:
                replaced = made.bl_idname
                tree.nodes.remove(made)
                return Error((BlenderChoiceFault(path=repr(tree.nodes), identifier=replaced, accepted=(identifier,)),))

    made: BlenderResult[bpy.types.Node] = catch(lambda: tree.nodes.new(identifier), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(tree.nodes), error=error),))
    return Result[bpy.types.Node, BlenderFaults].bind(made, narrowed)


def grouped(node: bpy.types.Node, group: bpy.types.NodeTree) -> BlenderResult[None]:
    """Points a group node at a group, a group containing the node's own tree answering `edit`, since Blender leaves that write empty with no raise."""
    host: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return (
        Error((BlenderEditFault(request=f"{node!r}.node_tree", error=ValueError(f"{group.name} contains {host.name}")),))
        if group.contains_tree(host)
        else written(node, "node_tree", group).map(lambda _: None)
    )


def joined(
    tree: bpy.types.NodeTree, source: bpy.types.NodeSocket, target: bpy.types.NodeSocket, *, verify_limits: bool = True, handle_dynamic_sockets: bool = False
) -> BlenderResult[bpy.types.NodeLink]:
    """Links an output socket to an input socket of the same tree.

    Args:
        tree: Tree holding both sockets.
        source: Output socket.
        target: Input socket, a virtual extend socket under `handle_dynamic_sockets`.
        verify_limits: Whether the link replaces links past the target's limit.
        handle_dynamic_sockets: Whether a virtual target grows the socket the link ends at.

    Returns:
        `Ok` of a valid link; `Error` of `edit` for a refused link or an invalid one the host created and the call removes, or of `absent` where the host answers no link.
    """

    def answered(link: bpy.types.NodeLink | None) -> BlenderResult[bpy.types.NodeLink]:
        match link:
            case bpy.types.NodeLink(is_valid=True):
                return Ok(link)
            case bpy.types.NodeLink():
                tree.links.remove(link)
                return Error((BlenderEditFault(request=repr(tree.links), error=ValueError(f"{source!r} cannot feed {target!r}")),))
            case None:
                return Error((BlenderAbsentFault(owner=repr(tree.links), key=repr((source, target))),))

    linked: BlenderResult[bpy.types.NodeLink | None] = catch(
        lambda: tree.links.new(source, target, verify_limits=verify_limits, handle_dynamic_sockets=handle_dynamic_sockets), exception=RuntimeError
    )().map_error(lambda error: (BlenderEditFault(request=repr(tree.links), error=error),))
    return Result[bpy.types.NodeLink | None, BlenderFaults].bind(linked, answered)


def ordered(socket: bpy.types.NodeSocket, desired: tuple[bpy.types.NodeLink, ...]) -> None:
    """Orders links of a multi-input socket top to bottom as desired, within the slots they occupy, through native sort-id swaps."""
    type Plan = tuple[tuple[bpy.types.NodeLink, ...], tuple[tuple[bpy.types.NodeLink, bpy.types.NodeLink], ...]]

    def planned(state: Plan, step: tuple[int, bpy.types.NodeLink]) -> Plan:
        (live, swaps), (index, wanted) = state, step
        held, position = live[index], live.index(wanted)
        return (live, swaps) if held == wanted else (tuple(wanted if at == index else held if at == position else link for at, link in enumerate(live)), (*swaps, (held, wanted)))

    start: Plan = (tuple(link for link in socket.links if link in desired), ())  # type: ignore[union-attr]  # ty: ignore[not-iterable]
    _, swaps = functools.reduce(planned, enumerate(desired), start)
    for held, wanted in swaps:
        held.swap_multi_input_sort_id(wanted)


def active_output(tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.NodeGroupOutput]:
    """Returns a group's active Group Output node."""
    return Option.of_optional(next((node for node in tree.nodes if isinstance(node, bpy.types.NodeGroupOutput) and node.is_active_output), None)).to_result_with(
        lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=bpy.types.NodeGroupOutput.__name__),)
    )


def fingerprinted(owner: bpy.types.NodeTree | bpy.types.Node) -> BlenderResult[Option[ContentHash]]:
    """Hashes observable native content of a tree or one node's upstream graph.

    Args:
        owner: Complete tree, or target node whose incoming connections select its upstream closure.

    Returns:
        The digest of persistent settings, socket defaults, ordered links, nested
        groups, and image content when fully observable. Unobservable image
        dependencies return `Nothing`; native capture faults remain errors.
    """
    tree: bpy.types.NodeTree = owner if isinstance(owner, bpy.types.NodeTree) else owner.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    pending = list(owner.nodes) if isinstance(owner, bpy.types.NodeTree) else [owner]
    seen: set[bpy.types.Node] = set()
    while pending:
        if (current := (pending.pop())) not in seen:
            seen.add(current)
            pending.extend(link.from_node for socket in current.inputs for link in socket.links)  # type: ignore[misc,union-attr]  # ty: ignore[invalid-argument-type,not-iterable]
    nodes = tuple(sorted(seen, key=lambda item: item.name))
    settings = tuple(Descriptor(root=type(tree), owner="", prop=prop) for prop in Snapshot.members(tree.bl_rna) if not isinstance(prop, bpy.types.CollectionProperty))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    def captured(struct: "bpy.types.bpy_struct[object]", selection: Option[tuple[Descriptor, ...]] = Nothing) -> BlenderResult[tuple[Snapshot, tuple[Option[ContentHash], ...]]]:
        def referenced(snapshot: Snapshot) -> BlenderResult[tuple[Snapshot, tuple[Option[ContentHash], ...]]]:
            blocks = dict.fromkeys(
                block
                for at, held in snapshot.tree.owners.items()
                for name, value in held.values.items()
                if isinstance(value, IdName)
                for block in (struct.path_resolve(f"{at}.{name}".removeprefix(".")),)
                if isinstance(block, bpy.types.NodeTree | bpy.types.Image)
            )
            return accumulated(fingerprinted(block) if isinstance(block, bpy.types.NodeTree) else images.fingerprinted(block) for block in blocks).map(lambda hashes: (snapshot, hashes))

        return Snapshot.capture(struct, selection).bind(referenced)

    def linked(link: bpy.types.NodeLink) -> BlenderResult[ContentHash]:
        endpoints = msgspec.msgpack.encode((link.from_node.name, link.from_socket.identifier, link.to_node.name, link.to_socket.identifier))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        members = tuple(Descriptor(root=type(link), owner="", prop=prop) for prop in link.bl_rna.properties if not prop.is_readonly)
        return Snapshot.capture(link, Some(members)).map(lambda snapshot: content_hash(bpy.types.NodeLink, endpoints, snapshot))

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

__all__ = ["GraphTree", "GroupNode", "Holder", "OutputNode", "active_output", "created", "emptied", "fingerprinted", "group_node", "grouped", "joined", "ordered", "socket_at", "tree_of"]
