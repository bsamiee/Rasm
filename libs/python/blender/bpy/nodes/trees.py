"""Tree and node classes, holders, sockets, node, item, and link creation, link order, link ends and feeds, and tree walks."""

from collections.abc import Callable, Iterable, Iterator, Mapping
from functools import partial, reduce
from types import MappingProxyType
from typing import Final, overload, Protocol, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch

from blender.bpy.data.ids.store import embedded
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.paths import struct_at
from blender.records.faults import Fault, Faults, Outcome, Reason
from blender.records.trees import reached

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type GraphTree = bpy.types.CompositorNodeTree | bpy.types.GeometryNodeTree | bpy.types.ShaderNodeTree
type Holder = bpy.types.NodeTree | bpy.types.Material | bpy.types.World | bpy.types.Light


class Named(Protocol):
    """Item of a node item collection, named as the socket it adds."""

    name: str


class Items[I: Named](Protocol):
    """Node item collection taking moves by index, such as Menu Switch `enum_items` and File Output `file_output_items`."""

    def __iter__(self) -> Iterator[I]:
        """Items in socket order."""

    def find(self, key: str, /) -> int:
        """Index of the item named `key`, -1 when none."""

    def move(self, from_index: int, to_index: int, /) -> None:
        """Moves one item, an index past the range moving nothing."""


# --- [CONSTANTS] ------------------------------------------------------------------------

NATIVE: Final[Mapping[type[bpy.types.NodeTree], Mapping[type[bpy.types.Node], type[bpy.types.Node]]]] = MappingProxyType({
    bpy.types.ShaderNodeTree: MappingProxyType({}),
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
GROUP_NODES: Final[tuple[type[bpy.types.Node], ...]] = tuple(dict.fromkeys(table.get(bpy.types.ShaderNodeGroup, bpy.types.ShaderNodeGroup) for table in NATIVE.values()))
OUTPUT_NODES: Final[tuple[type[bpy.types.Node], ...]] = (bpy.types.ShaderNodeOutputMaterial, bpy.types.ShaderNodeOutputWorld, bpy.types.ShaderNodeOutputLight, bpy.types.NodeGroupOutput)
DATA_TYPES: Final[Mapping[type[bpy.types.NodeSocket], "rna_enums.NodeSocketDataTypeItems"]] = MappingProxyType({
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


def base_socket(kind: type[bpy.types.NodeSocket]) -> type[bpy.types.NodeSocket]:
    """Returns the base socket class of a refined one, `NodeSocketFloat` for `NodeSocketFloatFactor`, the class whose name the host's refined class names begin with."""
    return max((base for base in DATA_TYPES if kind.__name__.startswith(base.__name__)), key=lambda base: len(base.__name__), default=kind)


def native(tree: type[bpy.types.NodeTree], kind: type[bpy.types.Node]) -> Outcome[type[bpy.types.Node]]:
    """Returns the class a `tree` tree builds for a shader-tree node class, such as its own group node or color nodes, refusal fault for an unsupported tree."""
    return (
        Option
        .of_optional(NATIVE.get(tree))
        .map(lambda table: table.get(kind, kind))
        .to_result_with(lambda: Fault.of(Reason.OUTSIDE, "tree", tree.__name__, member=", ".join(each.__name__ for each in NATIVE)))
    )


def arranged[I: Named](items: Items[I], names: tuple[str, ...], added: Callable[[str], object], removed: Callable[[I], object]) -> tuple[I, ...]:
    """Removes items `names` lacks, adds the names `items` lacks, and moves each item to its position in `names`.

    Args:
        items: Item collection of one node.
        names: Item names in desired order.
        added: Adds the item of one name.
        removed: Removes one item.

    Returns:
        Items in desired order.
    """
    for stale in tuple(item.name for item in items if item.name not in names):
        removed(next(item for item in items if item.name == stale))
    for name in tuple(name for name in names if items.find(name) < 0):
        added(name)
    for index, name in enumerate(names):
        items.move(items.find(name), index)
    return tuple(items)


def links_of(socket: bpy.types.NodeSocket) -> tuple[bpy.types.NodeLink, ...]:
    """Returns the links of one socket, an input's links top first, each read one scan of the tree's links."""
    links: tuple[bpy.types.NodeLink, ...] = socket.links  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return links


# --- [EDITS]


@overload
def tree_of(holder: bpy.types.Material | bpy.types.World | bpy.types.Light) -> Outcome[bpy.types.ShaderNodeTree]: ...
@overload
def tree_of(holder: bpy.types.ID) -> Outcome[bpy.types.NodeTree]: ...
def tree_of(holder: bpy.types.ID) -> Outcome[bpy.types.NodeTree]:
    """Returns group itself or embedded tree, `absent` fault for holder without one."""
    return (Some(holder) if isinstance(holder, bpy.types.NodeTree) else embedded(holder)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(holder), member=bpy.types.NodeTree.__name__))


def emptied[T: bpy.types.ID](holder: T) -> Outcome[T]:
    """Removes every node of holder tree, returning holder."""
    return tree_of(holder).map(lambda tree: tree.nodes.clear()).map(lambda _: holder)


def socket_at(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, identifier: str | int, aliases: Mapping[str, str] = MappingProxyType({}), /) -> Outcome[bpy.types.NodeSocket]:
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
            found = Block.of_seq(sockets).filter(lambda socket: socket.identifier == aliases.get(identifier, identifier)).try_head()
    return found.to_result_with(lambda: Fault.of(Reason.ABSENT, repr(sockets), member=str(identifier)))


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
                return Error(Fault.of(Reason.OUTSIDE, repr(tree.nodes), replaced, member=identifier))

    return catch(tree.nodes.new, exception=RuntimeError)(identifier).map_error(partial(Fault.of, Reason.REFUSED, repr(tree.nodes))).bind(narrowed)


def grouped(node: bpy.types.Node, group: bpy.types.NodeTree) -> Outcome[None]:
    """Points group node at group, `edit` fault for group containing node's tree."""
    host: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return (
        Error(Fault.of(Reason.REFUSED, f"{node!r}.node_tree", ValueError(f"{group.name} contains {host.name}")))
        if group.contains_tree(host)
        else written(node, ("node_tree",), group).map(lambda _: None)
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
                return Error(Fault.of(Reason.REFUSED, repr(tree.links), ValueError(f"Link from {source!r} to {target!r} is invalid")))
            case None:
                return Error(Fault.of(Reason.ABSENT, repr(tree.links), member=repr((source, target))))

    linked: Outcome[bpy.types.NodeLink | None] = catch(tree.links.new, exception=RuntimeError)(source, target, verify_limits=verify_limits, handle_dynamic_sockets=handle_dynamic_sockets).map_error(
        partial(Fault.of, Reason.REFUSED, repr(tree.links))
    )
    return linked.bind(answered)


def ordered(socket: bpy.types.NodeSocket, desired: tuple[bpy.types.NodeLink, ...]) -> Outcome[None]:
    """Orders multi-input links top to bottom within their slots by sort ID swaps, `edit` fault for the first swap the host refuses."""
    type Plan = tuple[tuple[bpy.types.NodeLink, ...], tuple[tuple[bpy.types.NodeLink, bpy.types.NodeLink], ...]]

    def planned(state: Plan, step: tuple[int, bpy.types.NodeLink]) -> Plan:
        (live, swaps), (index, wanted) = state, step
        held, position = live[index], live.index(wanted)
        return (live, swaps) if held == wanted else (tuple(wanted if at == index else held if at == position else link for at, link in enumerate(live)), (*swaps, (held, wanted)))

    start: Plan = (tuple(link for link in links_of(socket) if link in desired), ())
    _, swaps = reduce(planned, enumerate(desired), start)
    return reduce(
        lambda done, pair: done.bind(lambda _: catch(pair[0].swap_multi_input_sort_id, exception=RuntimeError)(pair[1]).map_error(partial(Fault.of, Reason.REFUSED, repr(pair[0])))),
        swaps,
        Result[None, Faults](ok=None),
    )


# --- [WALKS]


def endpoints(link: bpy.types.NodeLink, /) -> tuple[bpy.types.Node, bpy.types.NodeSocket, bpy.types.Node, bpy.types.NodeSocket]:
    """Returns the nodes and sockets at both ends of a link, source node and socket first."""
    return link.from_node, link.from_socket, link.to_node, link.to_socket  # type: ignore[return-value]  # ty: ignore[invalid-return-type]


def group_tree(node: bpy.types.Node, /) -> Option[bpy.types.NodeTree]:
    """Returns the tree a group node points at, Nothing for any other node or an empty group node."""
    return struct_at(node, ("node_tree",), bpy.types.NodeTree) if isinstance(node, GROUP_NODES) else Nothing


def active_output(tree: bpy.types.NodeTree) -> Outcome[bpy.types.NodeGroupOutput]:
    """Returns active Group Output node."""
    return (
        Block
        .of_seq(tree.nodes)
        .choose(lambda node: Option.of_optional(try_downcast(bpy.types.NodeGroupOutput, node)))
        .filter(lambda node: node.is_active_output)
        .try_head()
        .to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=bpy.types.NodeGroupOutput.__name__))
    )


def nested[T: bpy.types.NodeTree](roots: Iterable[T], *, muted: bool = True) -> tuple[T, ...]:
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


def upstream(node: bpy.types.Node, /) -> tuple[bpy.types.Node, ...]:
    """Returns node and every node its input links reach, sorted by name."""
    tree: bpy.types.NodeTree = node.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    feeding: dict[bpy.types.Node, list[bpy.types.Node]] = {}
    for source, _, target, _ in map(endpoints, tree.links):
        feeding.setdefault(target, []).append(source)
    return tuple(sorted(reached((node,), lambda current: feeding.get(current, ())), key=lambda item: item.name))


def feeds(trees: Iterable[bpy.types.NodeTree], /) -> Mapping[bpy.types.NodeSocket, tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...]]:
    """Returns the source node and socket of each valid unmuted link into each input socket, one pass over the links of the trees given."""
    fed: dict[bpy.types.NodeSocket, tuple[tuple[bpy.types.Node, bpy.types.NodeSocket], ...]] = {}
    for source, outlet, _, inlet in (endpoints(link) for tree in trees for link in tree.links if link.is_valid and not link.is_muted):
        fed[inlet] = (*fed.get(inlet, ()), (source, outlet))
    return MappingProxyType(fed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DATA_TYPES",
    "GROUP_NODES",
    "NATIVE",
    "OUTPUT_NODES",
    "GraphTree",
    "Holder",
    "Items",
    "Named",
    "active_output",
    "arranged",
    "base_socket",
    "created",
    "emptied",
    "endpoints",
    "feeds",
    "group_tree",
    "grouped",
    "joined",
    "links_of",
    "native",
    "nested",
    "ordered",
    "socket_at",
    "tree_of",
    "upstream",
]
