# mypy: disable-error-code="arg-type, attr-defined, union-attr, unreachable"
# ty: ignore[invalid-argument-type, unresolved-attribute]
"""Digest one node tree as its interface, non-default nodes, and links by socket identifier."""

from collections import ChainMap
from collections.abc import Iterable

import attrs
import bpy
from rna import plain, stored

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Socket:
    """Interface socket with its panel, `None` at the root, and the values that differ from a fresh socket of its type."""

    identifier: str
    name: str
    in_out: str
    socket_type: str
    panel: str | None
    values: dict[str, object]


@attrs.frozen
class Node:
    """Node with the settings, unlinked inputs, and outputs that differ from a fresh node of its type."""

    name: str
    bl_idname: str
    values: dict[str, object]
    inputs: dict[str, object]
    outputs: dict[str, object]


@attrs.frozen
class Link:
    """Link from an output to an input, each named by node name and socket identifier."""

    from_node: str
    from_socket: str
    to_node: str
    to_socket: str
    is_muted: bool


@attrs.frozen
class Digest:
    """Tree with the RNA type of its owner, interface sockets, nodes, and links."""

    tree: str
    owner: str
    interface: tuple[Socket, ...]
    nodes: tuple[Node, ...]
    links: tuple[Link, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class UnknownTree:
    """Name matching no node group or ID pointing at a node tree."""

    name: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def trees() -> dict[str, tuple[str, bpy.types.NodeTree]]:
    """Every node tree by owner name with the owner's RNA type, a node group before an ID pointing at a tree, IDs in `bpy.data` order."""
    owned = (
        {owner.name: (type(owner).__name__, tree) for owner in getattr(bpy.data, ids.identifier) if (tree := getattr(owner, pointer.identifier))}
        for ids in bpy.data.bl_rna.properties
        if isinstance(ids, bpy.types.CollectionProperty)
        for pointer in ids.fixed_type.properties
        if isinstance(pointer, bpy.types.PointerProperty) and isinstance(pointer.fixed_type, bpy.types.NodeTree) and not pointer.is_deprecated
    )
    return dict(ChainMap({group.name: (type(group).__name__, group) for group in bpy.data.node_groups}, *owned))


def record(owner: str, tree: bpy.types.NodeTree) -> Digest:
    """Interface, nodes, and links of a tree, each socket and node holding the values that differ from a fresh one in a new tree of its type."""
    layout = frozenset(p.identifier for p in bpy.types.Node.bl_rna.properties) - {"mute"}

    def changed[T: bpy.types.Node | bpy.types.NodeTreeInterfaceSocket](item: T, fresh: T, skipped: frozenset[str]) -> dict[str, object]:
        """Stored values of `item` outside `skipped` that differ from `fresh`."""
        keys = (p.identifier for p in item.bl_rna.properties if p.identifier not in skipped and stored(p))
        return {k: v for k in keys if (v := plain(getattr(item, k))) != plain(getattr(fresh, k))}

    def socket_values(items: Iterable[bpy.types.NodeSocket]) -> dict[str, object]:
        """Values by identifier of the sockets with a `default_value`."""
        return {s.identifier: plain(s.default_value) for s in items if "default_value" in s.bl_rna.properties}

    def node(item: bpy.types.Node, fresh: bpy.types.Node) -> Node:
        """Settings, enabled unlinked inputs, and outputs of `item` that differ from `fresh`, `fresh` first taking changed ID settings to hold a group node's group defaults."""
        values = changed(item, fresh, layout)
        for key in values:
            if isinstance(value := getattr(item, key), bpy.types.ID):
                setattr(fresh, key, value)
        inputs, blank_inputs = socket_values(s for s in item.inputs if s.enabled and not s.is_linked), socket_values(fresh.inputs)
        outputs, blank_outputs = socket_values(item.outputs), socket_values(fresh.outputs)
        return Node(
            item.name, item.bl_idname, values, {k: v for k, v in inputs.items() if v != blank_inputs.get(k)}, {k: v for k, v in outputs.items() if k in blank_outputs and v != blank_outputs[k]}
        )

    baseline = bpy.data.node_groups.new("digest", tree.bl_idname)
    try:
        nodes = tuple(node(item, baseline.nodes.new(item.bl_idname)) for item in tree.nodes)
        interface = tuple(
            Socket(
                item.identifier,
                item.name,
                item.in_out,
                item.socket_type,
                None if item.parent == tree.interface.root_panel else item.parent.name,
                changed(item, baseline.interface.new_socket(item.name, in_out=item.in_out, socket_type=item.socket_type), frozenset()),
            )
            for item in tree.interface.items_tree
            if isinstance(item, bpy.types.NodeTreeInterfaceSocket)
        )
        links = tuple(Link(link.from_node.name, link.from_socket.identifier, link.to_node.name, link.to_socket.identifier, link.is_muted) for link in tree.links)
        return Digest(tree.name, owner, interface, nodes, links)
    finally:
        bpy.data.node_groups.remove(baseline)


def digest(name: str) -> Digest | UnknownTree:
    """Digest of the tree owned by `name`, values rounded, layout and selection left out."""
    hit = trees().get(name)
    return UnknownTree(name) if hit is None else record(*hit)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Digest", "Link", "Node", "Socket", "UnknownTree", "digest", "record", "trees"]
