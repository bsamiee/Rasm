"""Tree and node classes, holders, sockets, node, item, and link creation, link order, link ends and feeds, and tree walks."""

from collections import defaultdict, deque
from collections.abc import Callable, Hashable, Iterable, Mapping
from functools import partial, reduce
from types import MappingProxyType
from typing import Final, overload, TYPE_CHECKING

import bpy
from expression import Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch

from rasm_bpy.data._ids.store import _embedded
from rasm_bpy.data._rna.descriptors import written
from rasm_bpy.data._rna.paths import _definition, _struct_at
from rasm_records import Error, Fault, Faults, Nothing, Ok, Outcome, reached, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type GraphTree = bpy.types.CompositorNodeTree | bpy.types.GeometryNodeTree | bpy.types.ShaderNodeTree
type Holder = bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light

# --- [CONSTANTS] ------------------------------------------------------------------------

_NATIVE: Final[Mapping[type[bpy.types.NodeTree], Mapping[type[bpy.types.Node], type[bpy.types.Node]]]] = MappingProxyType({
    bpy.types.ShaderNodeTree: MappingProxyType(dict[type[bpy.types.Node], type[bpy.types.Node]]()),
    bpy.types.CompositorNodeTree: MappingProxyType({
        bpy.types.ShaderNodeSeparateColor: bpy.types.CompositorNodeSeparateColor,
        bpy.types.ShaderNodeCombineColor: bpy.types.CompositorNodeCombineColor,
        bpy.types.ShaderNodeGroup: bpy.types.CompositorNodeGroup,
    }),
    bpy.types.GeometryNodeTree: MappingProxyType({
        bpy.types.ShaderNodeSeparateColor: bpy.types.FunctionNodeSeparateColor,
        bpy.types.ShaderNodeCombineColor: bpy.types.FunctionNodeCombineColor,
        bpy.types.ShaderNodeGroup: bpy.types.GeometryNodeGroup,
    }),
})
_GROUP_NODES: Final[tuple[type[bpy.types.Node], ...]] = tuple(dict.fromkeys(table.get(bpy.types.ShaderNodeGroup, bpy.types.ShaderNodeGroup) for table in _NATIVE.values()))
OUTPUT_NODES: Final[tuple[type[bpy.types.Node], ...]] = (bpy.types.ShaderNodeOutputMaterial, bpy.types.ShaderNodeOutputWorld, bpy.types.ShaderNodeOutputLight, bpy.types.NodeGroupOutput)
_DATA_TYPES: Final[Mapping[type[bpy.types.NodeSocket], "rna_enums.NodeSocketDataTypeItems"]] = MappingProxyType({
    bpy.types.NodeSocketFloat: "FLOAT",
    bpy.types.NodeSocketInt: "INT",
    bpy.types.NodeSocketBool: "BOOLEAN",
    bpy.types.NodeSocketVector: "VECTOR",
    bpy.types.NodeSocketColor: "RGBA",
    bpy.types.NodeSocketRotation: "ROTATION",
    bpy.types.NodeSocketMatrix: "MATRIX",
    bpy.types.NodeSocketString: "STRING",
    bpy.types.NodeSocketMenu: "MENU",
    bpy.types.NodeSocketShader: "SHADER",
    bpy.types.NodeSocketObject: "OBJECT",
    bpy.types.NodeSocketImage: "IMAGE",
    bpy.types.NodeSocketGeometry: "GEOMETRY",
    bpy.types.NodeSocketCollection: "COLLECTION",
    bpy.types.NodeSocketTexture: "TEXTURE",
    bpy.types.NodeSocketMaterial: "MATERIAL",
    bpy.types.NodeSocketBundle: "BUNDLE",
    bpy.types.NodeSocketClosure: "CLOSURE",
    bpy.types.NodeSocketFont: "FONT",
    bpy.types.NodeSocketScene: "SCENE",
    bpy.types.NodeSocketText: "TEXT",
    bpy.types.NodeSocketMask: "MASK",
    bpy.types.NodeSocketSound: "SOUND",
    bpy.types.NodeSocketIntVector3D: "INT_VECTOR",
})

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CLASSES]


def _base_socket(kind: type[bpy.types.NodeSocket]) -> type[bpy.types.NodeSocket]:
    """Returns the native value carrier of a refined builtin socket, retaining basic and custom socket classes."""
    rna = _definition(kind)
    if kind in _DATA_TYPES or rna.base != _definition(bpy.types.NodeSocketStandard):
        return kind
    try:
        prop = rna.properties["default_value"]
    except KeyError:
        return kind
    match prop:
        case bpy.types.FloatProperty(is_array=True):
            return bpy.types.NodeSocketVector
        case bpy.types.FloatProperty():
            return bpy.types.NodeSocketFloat
        case bpy.types.IntProperty(is_array=True):
            return bpy.types.NodeSocketIntVector3D
        case bpy.types.IntProperty():
            return bpy.types.NodeSocketInt
        case bpy.types.StringProperty():
            return bpy.types.NodeSocketString
        case _:
            return kind


def _native(tree: type[bpy.types.NodeTree], kind: type[bpy.types.Node]) -> Outcome[type[bpy.types.Node]]:
    """Returns the class a `tree` tree builds for a shader-tree node class (its own group node, color nodes), refusal fault for an unsupported tree."""
    return (
        Option
        .of_optional(_NATIVE.get(tree))
        .map(lambda table: table.get(kind, kind))
        .to_result_with(lambda: Fault.of(Reason.OUTSIDE, "tree", tree.__name__, member=", ".join(each.__name__ for each in _NATIVE)))
    )


# --- [ITEMS]


def _matched_items[K: Hashable](live: Iterable[str], stored: Mapping[str, K], requests: Iterable[K]) -> tuple[tuple[int, ...], tuple[str | None, ...]]:
    """Matches original requests to live native identifiers in item order, returning removed indices and a retained identifier per request."""
    inventory = tuple(live)
    available: defaultdict[K, deque[str]] = defaultdict(deque)
    for identifier in inventory:
        if identifier in stored:
            available[stored[identifier]].append(identifier)
    matched: list[str | None] = []
    for request in requests:
        candidates = available[request]
        matched.append(candidates.popleft() if candidates else None)
    kept = frozenset(identifier for identifier in matched if identifier is not None)
    return tuple(index for index, identifier in enumerate(inventory) if identifier not in kept), tuple(matched)


def _ordered_items(live: Iterable[str], desired: Iterable[str], moved: Callable[[int, int], None]) -> None:
    """Moves native items into desired identifier order, retaining every item and its identifier."""
    current = list(live)
    for index, identifier in enumerate(desired):
        if (position := current.index(identifier)) != index:
            moved(position, index)
            current.insert(index, current.pop(position))


# --- [READS]


def _links_of(socket: bpy.types.NodeSocket) -> tuple[bpy.types.NodeLink, ...]:
    """Returns the links of one socket, an input's links top first, each read one scan of the tree's links."""
    links: tuple[bpy.types.NodeLink, ...] = socket.links  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return links


def _interface_of(tree: bpy.types.NodeTree, /) -> bpy.types.NodeTreeInterface:
    """Returns tree interface, a member the stubs type optional."""
    interface: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return interface


# --- [EDITS]


def _new_group[T: bpy.types.NodeTree](kind: type[T], name: str, /) -> Outcome[T]:
    """Creates empty node group of a tree class, `refused` fault for a class the host refuses."""

    def added() -> T:
        tree: T = bpy.data.node_groups.new(name, _definition(kind).identifier)  # type: ignore[arg-type, assignment]  # ty: ignore[invalid-argument-type, invalid-assignment]
        return tree

    return catch(added, exception=TypeError)().map_error(partial(Fault.of, Reason.REFUSED, repr(bpy.data.node_groups)))


@overload
def _tree_of(holder: bpy.types.Material | bpy.types.World | bpy.types.Light) -> Outcome[bpy.types.ShaderNodeTree]: ...
@overload
def _tree_of(holder: bpy.types.ID) -> Outcome[bpy.types.NodeTree]: ...
def _tree_of(holder: bpy.types.ID) -> Outcome[bpy.types.NodeTree]:
    """Returns group itself or embedded tree, `absent` fault for holder without one."""
    return (Some(holder) if isinstance(holder, bpy.types.NodeTree) else _embedded(holder)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(holder), member=bpy.types.NodeTree.__name__))


def _emptied[T: bpy.types.ID](holder: T) -> Outcome[T]:
    """Removes every node of holder tree, returning holder."""
    return _tree_of(holder).map(lambda tree: tree.nodes.clear()).map(lambda _: holder)


def _socket_at(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, identifier: str | int, aliases: Mapping[str, str] = MappingProxyType({}), /) -> Outcome[bpy.types.NodeSocket]:
    """Finds socket by identifier, or by position among the sockets the node's mode members enable.

    Args:
        sockets: Inputs or outputs of one node.
        identifier: Socket identifier, stable where names repeat or translate, an alias `aliases` maps to one, or position among enabled sockets.
        aliases: Socket identifiers by the interface item key or item name addressing them.

    Returns:
        `Ok` of socket. `Error` of `absent` for an identifier or position with no socket.
    """
    match identifier:
        case int(position):
            found = Block.of_seq(sockets).filter(lambda socket: socket.enabled).skip(position).try_head()
        case str():
            address = aliases.get(identifier, identifier)
            found = Option.of_optional(next((socket for socket in sockets if socket.identifier == address), None))
    return found.to_result_with(lambda: Fault.of(Reason.ABSENT, repr(sockets), member=str(identifier)))


def _created[N: bpy.types.Node](tree: bpy.types.NodeTree, kind: type[N]) -> Outcome[N]:
    """Adds node by its registered RNA identifier.

    Returns:
        `Ok` of node. `Error` of `refused` for refused class, `outside` for alias add-ons rebind to another class, its node removed.
    """
    identifier = _definition(kind).identifier

    def narrowed(made: bpy.types.Node) -> Outcome[N]:
        match made:
            case kind():
                return Ok(made)
            case _:
                replaced = made.bl_idname
                tree.nodes.remove(made)
                return Error(Fault.of(Reason.OUTSIDE, repr(tree.nodes), replaced, member=identifier))

    return catch(tree.nodes.new, exception=RuntimeError)(identifier).map_error(partial(Fault.of, Reason.REFUSED, repr(tree.nodes))).bind(narrowed)


def _group_set(node: bpy.types.Node, group: bpy.types.NodeTree) -> Outcome[None]:
    """Points group node at group, `refused` fault for group containing node's tree."""
    host: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return (
        Error(Fault.of(Reason.REFUSED, f"{node!r}.node_tree", ValueError(f"{group.name} contains {host.name}")))
        if group.contains_tree(host)
        else written(node, ("node_tree",), group).map(lambda _: None)
    )


def _linked(tree: bpy.types.NodeTree, source: bpy.types.NodeSocket, target: bpy.types.NodeSocket, *, verify_limits: bool = True, handle_dynamic_sockets: bool = False) -> Outcome[bpy.types.NodeLink]:
    """Links output socket to input socket.

    Args:
        tree: Tree holding both sockets.
        source: Output socket.
        target: Input socket, virtual extend socket under `handle_dynamic_sockets`.
        verify_limits: Whether link replaces links past target limit.
        handle_dynamic_sockets: Whether virtual target grows socket link ends at.

    Returns:
        `Ok` of valid link. `Error` of `refused` for refused link or invalid link the call removes, `absent` when host returns none.
    """

    def answered(link: bpy.types.NodeLink | None) -> Outcome[bpy.types.NodeLink]:
        match link:
            case bpy.types.NodeLink(is_valid=True):
                return Ok(link)
            case bpy.types.NodeLink():
                tree.links.remove(link)
                return Error(Fault.of(Reason.REFUSED, repr(tree.links), ValueError(f"Link from {source!r} to {target!r} is invalid")))
            case None:
                return Error(Fault.of(Reason.ABSENT, repr(tree.links), member=repr((source, target))))

    linked: Outcome[bpy.types.NodeLink | None] = catch(tree.links.new, exception=RuntimeError)(source, target, verify_limits=verify_limits, handle_dynamic_sockets=handle_dynamic_sockets).map_error(
        partial(Fault.of, Reason.REFUSED, repr(tree.links))
    )
    return linked.bind(answered)


def _ordered(socket: bpy.types.NodeSocket, desired: tuple[bpy.types.NodeLink, ...]) -> Outcome[None]:
    """Orders multi-input links top to bottom within their slots by sort ID swaps, `refused` fault for the first swap the host refuses."""
    selected = frozenset(desired)
    live = [link for link in _links_of(socket) if link in selected]
    positions = {link: index for index, link in enumerate(live)}
    swaps: list[tuple[bpy.types.NodeLink, bpy.types.NodeLink]] = []
    for index, wanted in enumerate(desired):
        held, position = live[index], positions[wanted]
        if held != wanted:
            live[index], live[position] = wanted, held
            positions[held], positions[wanted] = position, index
            swaps.append((held, wanted))
    return reduce(
        lambda done, pair: done.bind(lambda _: catch(pair[0].swap_multi_input_sort_id, exception=RuntimeError)(pair[1]).map_error(partial(Fault.of, Reason.REFUSED, repr(pair[0])))),
        swaps,
        Result[None, Faults](ok=None),
    )


# --- [WALKS]


def _endpoints(link: bpy.types.NodeLink, /) -> tuple[bpy.types.Node, bpy.types.NodeSocket, bpy.types.Node, bpy.types.NodeSocket]:
    """Returns the nodes and sockets at both ends of a link, source node and socket first."""
    return link.from_node, link.from_socket, link.to_node, link.to_socket  # type: ignore[return-value]  # ty: ignore[invalid-return-type]


def group_tree(node: bpy.types.Node, /) -> Option[bpy.types.NodeTree]:
    """Returns the tree a group node points at, Nothing for any other node or an empty group node."""
    return _struct_at(node, ("node_tree",), bpy.types.NodeTree) if isinstance(node, _GROUP_NODES) else Nothing


def _active_output(tree: bpy.types.NodeTree) -> Outcome[bpy.types.NodeGroupOutput]:
    """Returns active Group Output node."""
    return Option.of_optional(next((node for node in tree.nodes if isinstance(node, bpy.types.NodeGroupOutput) and node.is_active_output), None)).to_result_with(
        partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=bpy.types.NodeGroupOutput.__name__)
    )


def _nested[T: bpy.types.NodeTree](roots: Iterable[T], *, muted: bool = True) -> tuple[T, ...]:
    """Returns roots and every group tree their group nodes reach, each once at its first reach.

    Args:
        roots: Trees to walk from.
        muted: Whether muted group nodes lead into their groups.
    """
    return tuple(
        reached(
            tuple(roots),
            lambda tree: tuple(child for node in tree.nodes if muted or not node.mute for child in group_tree(node).bind(lambda held: Option.of_optional(try_downcast(type(tree), held))).to_list()),
        )
    )


def _upstream(node: bpy.types.Node, /) -> tuple[bpy.types.Node, ...]:
    """Returns node and every node its input links reach, sorted by name."""
    tree: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    feeding: dict[bpy.types.Node, list[bpy.types.Node]] = {}
    for source, _, target, _ in map(_endpoints, tree.links):
        feeding.setdefault(target, []).append(source)
    return tuple(sorted(reached((node,), lambda current: feeding.get(current, ())), key=lambda item: item.name))


def _feeds(trees: Iterable[bpy.types.NodeTree], /) -> Mapping[bpy.types.NodeSocket, tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...]]:
    """Returns the source node and socket of each valid unmuted link into each input socket, one pass over the links of the trees given."""
    fed: dict[bpy.types.NodeSocket, list[tuple[bpy.types.Node, bpy.types.NodeSocket]]] = {}
    for source, outlet, _, inlet in (_endpoints(link) for tree in trees for link in tree.links if link.is_valid and not link.is_muted):
        fed.setdefault(inlet, []).append((source, outlet))
    return MappingProxyType({inlet: tuple(sources) for inlet, sources in fed.items()})


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["OUTPUT_NODES", "GraphTree", "Holder", "group_tree"]
