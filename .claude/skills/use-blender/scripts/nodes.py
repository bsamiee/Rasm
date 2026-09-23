# ast-grep-ignore: no-stdlib-record
# mypy: disable-error-code="unreachable"
"""Digest one node tree as its interface, non-default nodes, and links by socket identifier, run inside Blender through `runpy.run_path`."""

from collections import ChainMap
from collections.abc import Iterable
from dataclasses import asdict, dataclass
from typing import cast, Literal

import bpy
from mathutils import Color, Euler, Matrix, Quaternion, Vector

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Digest | UnknownTree

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class Socket:
    """Interface socket with the values that differ from a fresh socket of its type."""

    identifier: str
    name: str
    in_out: str
    socket_type: str
    values: dict[str, object]


@dataclass(frozen=True, slots=True)
class Node:
    """Node with the settings, unlinked inputs, and outputs that differ from a fresh node of its type."""

    name: str
    bl_idname: str
    values: dict[str, object]
    inputs: dict[str, object]
    outputs: dict[str, object]


@dataclass(frozen=True, slots=True)
class Link:
    """Link from an output to an input, each named by node name and socket identifier."""

    from_node: str
    from_socket: str
    to_node: str
    to_socket: str
    is_muted: bool


@dataclass(frozen=True, slots=True)
class Digest:
    """Tree with the RNA type of its owner, interface sockets, nodes, and links."""

    tree: str
    owner: str
    interface: tuple[Socket, ...]
    nodes: tuple[Node, ...]
    links: tuple[Link, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
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


def record(owner: str, tree: bpy.types.NodeTree) -> Digest:
    """Interface, nodes, and links of a tree, each interface socket and node holding the values that differ from a fresh one made in a scratch tree of the same type."""
    layout = {p.identifier for p in bpy.types.Node.bl_rna.properties} - {"mute"}

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
            case Iterable():
                return [plain(v) for v in value]
            case _:
                raise TypeError(f"{type(value).__name__} is no RNA value type")

    def changed(item: bpy.types.bpy_struct, fresh: bpy.types.bpy_struct, skipped: set[str]) -> dict[str, object]:
        """Stored values of `item` outside `skipped` that differ from `fresh`."""
        keys = (p.identifier for p in item.bl_rna.properties if p.identifier not in skipped and stored(p))
        return {k: v for k in keys if (v := plain(getattr(item, k))) != plain(getattr(fresh, k))}

    def socket_values(items: Iterable[bpy.types.NodeSocket]) -> dict[str, object]:
        """Values of the sockets that hold one, by identifier."""
        return {s.identifier: plain(s.default_value) for s in items if hasattr(s, "default_value")}

    def node(item: bpy.types.Node, fresh: bpy.types.Node) -> Node:
        """Settings of `item` that differ from `fresh`, then its sockets against `fresh` holding the same IDs, so a group node's sockets compare with its group's defaults."""
        values = changed(item, fresh, layout)
        for key, value in ((k, getattr(item, k)) for k in values):
            if isinstance(value, bpy.types.ID):
                setattr(fresh, key, value)
        inputs, outputs = socket_values(s for s in item.inputs if s.enabled and not s.is_linked), socket_values(item.outputs)
        blank_inputs, blank_outputs = socket_values(fresh.inputs), socket_values(fresh.outputs)
        return Node(
            item.name, item.bl_idname, values, {k: v for k, v in inputs.items() if v != blank_inputs.get(k)}, {k: v for k, v in outputs.items() if k in blank_outputs and v != blank_outputs[k]}
        )

    def link(item: bpy.types.NodeLink) -> Link:
        """Link by node names and socket identifiers."""
        match item:
            case bpy.types.NodeLink(from_node=bpy.types.Node(name=a), from_socket=bpy.types.NodeSocket(identifier=s), to_node=bpy.types.Node(name=b), to_socket=bpy.types.NodeSocket(identifier=t)):
                return Link(a, s, b, t, item.is_muted)
            case _:
                raise TypeError(f"{tree.name} holds a link without both ends")

    scratch = bpy.data.node_groups.new("digest", cast('Literal["GeometryNodeTree", "CompositorNodeTree", "ShaderNodeTree", "TextureNodeTree"]', tree.bl_idname))
    declared, blank = cast("bpy.types.NodeTreeInterface", tree.interface), cast("bpy.types.NodeTreeInterface", scratch.interface)
    try:
        nodes = tuple(node(item, scratch.nodes.new(item.bl_idname)) for item in tree.nodes)
        interface = tuple(
            Socket(i.identifier, i.name, i.in_out, i.socket_type, changed(i, blank.new_socket(i.name, in_out=i.in_out, socket_type=i.socket_type), set()))
            for i in declared.items_tree
            if isinstance(i, bpy.types.NodeTreeInterfaceSocket)
        )
    finally:
        bpy.data.node_groups.remove(scratch)
    return Digest(tree.name, owner, interface, nodes, tuple(link(k) for k in tree.links))


def digest(name: str) -> Outcome:
    """Digest of the tree owned by `name`, values rounded, layout properties left out."""
    hit = trees().get(name)
    return UnknownTree(name) if hit is None else record(*hit)


def as_result(value: Outcome) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name under `kind`."""
    return {"kind": type(value).__name__, **asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Digest", "Link", "Node", "Outcome", "Socket", "UnknownTree", "as_result", "digest", "record", "trees"]
