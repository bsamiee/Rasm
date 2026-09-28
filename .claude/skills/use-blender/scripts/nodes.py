# mypy: disable-error-code="unreachable, arg-type, attr-defined, union-attr"
# ty: ignore[invalid-argument-type, not-iterable, unresolved-attribute]
"""Digest one node tree as its interface, non-default nodes, and links by socket identifier through the stored-value walk of RNA structs, run inside Blender through `runpy.run_path`."""

from collections import ChainMap
from collections.abc import Iterable

import attrs
import bpy
from mathutils import Color, Euler, Matrix, Quaternion, Vector

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Digest | UnknownTree

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Socket:
    """Interface socket with the values that differ from a fresh socket of its type."""

    identifier: str
    name: str
    in_out: str
    socket_type: str
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
    """Name matching no node group, node tree owner, or scene compositor."""

    name: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def trees() -> dict[str, tuple[str, bpy.types.NodeTree]]:
    """Every node tree by owner name with the owner's RNA type, a node group first, then an ID owning a tree in `bpy.data` order, then a scene compositor."""
    owners = (
        getattr(bpy.data, p.identifier) for p in bpy.data.bl_rna.properties if isinstance(p, bpy.types.CollectionProperty) and p.fixed_type is not None and "node_tree" in p.fixed_type.properties
    )
    groups = {group.name: (type(group).__name__, group) for group in bpy.data.node_groups}
    compositors = {scene.name: (type(scene).__name__, tree) for scene in bpy.data.scenes if (tree := scene.compositing_node_group)}
    return dict(ChainMap(groups, *({owner.name: (type(owner).__name__, owner.node_tree) for owner in ids if owner.node_tree} for ids in owners), compositors))


def stored(p: bpy.types.Property) -> bool:
    """Value a struct stores: editable properties, ID pointers, and owned structs and collections, with back pointers and active-item references left out."""
    match p:
        case bpy.types.PointerProperty(fixed_type=bpy.types.ID()):
            return not p.is_readonly
        case bpy.types.PointerProperty(fixed_type=bpy.types.Node() | bpy.types.NodeTreeInterfaceItem() | bpy.types.Struct() | None):
            return False
        case bpy.types.PointerProperty():
            return p.is_readonly
        case bpy.types.CollectionProperty():
            return True
        case _:
            return not p.is_readonly


def plain(value: object) -> object:
    """JSON form of an RNA value, floats rounded, flag sets sorted, IDs by name, structs by stored value, arrays and collections by element."""
    match value:
        case float():
            return round(value, 5)
        case str() | int() | None:
            return value
        case set():
            return sorted(value)
        case bpy.types.ID():
            return value.name
        case bpy.types.bpy_struct():
            return {p.identifier: plain(getattr(value, p.identifier)) for p in value.bl_rna.properties if stored(p)}
        case Vector() | Color() | Euler() | Quaternion() | Matrix():
            return plain(value[:])
        case _:
            return [plain(v) for v in value]


def record(owner: str, tree: bpy.types.NodeTree) -> Digest:
    """Interface, nodes, and links of a tree, each interface socket and node holding the values that differ from a fresh one made in a new tree of the same type."""
    layout = frozenset(p.identifier for p in bpy.types.Node.bl_rna.properties) - {"mute"}

    def changed(item: bpy.types.bpy_struct, fresh: bpy.types.bpy_struct, skipped: frozenset[str]) -> dict[str, object]:
        """Stored values of `item` outside `skipped` that differ from `fresh`."""
        keys = (p.identifier for p in item.bl_rna.properties if p.identifier not in skipped and stored(p))
        return {k: v for k in keys if (v := plain(getattr(item, k))) != plain(getattr(fresh, k))}

    def socket_values(items: Iterable[bpy.types.NodeSocket]) -> dict[str, object]:
        """Values of the sockets whose RNA type declares `default_value`, by identifier."""
        return {s.identifier: plain(s.default_value) for s in items if "default_value" in s.bl_rna.properties}

    def node(item: bpy.types.Node, fresh: bpy.types.Node) -> Node:
        """Settings, enabled unlinked inputs, and outputs of `item` that differ from `fresh`, which takes `item`'s ID settings first to hold a group node's group defaults."""
        values = changed(item, fresh, layout)
        for key in values:
            if isinstance(value := getattr(item, key), bpy.types.ID):
                setattr(fresh, key, value)
        inputs, outputs = socket_values(s for s in item.inputs if s.enabled and not s.is_linked), socket_values(item.outputs)
        blank_inputs, blank_outputs = socket_values(fresh.inputs), socket_values(fresh.outputs)
        return Node(
            item.name, item.bl_idname, values, {k: v for k, v in inputs.items() if v != blank_inputs.get(k)}, {k: v for k, v in outputs.items() if k in blank_outputs and v != blank_outputs[k]}
        )

    baseline = bpy.data.node_groups.new("digest", tree.bl_idname)
    try:
        nodes = tuple(node(item, baseline.nodes.new(item.bl_idname)) for item in tree.nodes)
        interface = tuple(
            Socket(i.identifier, i.name, i.in_out, i.socket_type, changed(i, baseline.interface.new_socket(i.name, in_out=i.in_out, socket_type=i.socket_type), frozenset()))
            for i in tree.interface.items_tree
            if isinstance(i, bpy.types.NodeTreeInterfaceSocket)
        )
        links = tuple(Link(k.from_node.name, k.from_socket.identifier, k.to_node.name, k.to_socket.identifier, k.is_muted) for k in tree.links)
        return Digest(tree.name, owner, interface, nodes, links)
    finally:
        bpy.data.node_groups.remove(baseline)


def digest(name: str) -> Outcome:
    """Digest of the tree owned by `name`, values rounded, layout properties left out."""
    hit = trees().get(name)
    return UnknownTree(name) if hit is None else record(*hit)


def as_result(value: Outcome) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name under `kind`."""
    return {"kind": type(value).__name__, **attrs.asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Digest", "Link", "Node", "Outcome", "Socket", "UnknownTree", "as_result", "digest", "plain", "record", "stored", "trees"]
