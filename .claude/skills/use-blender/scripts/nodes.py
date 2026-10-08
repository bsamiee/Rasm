# ty: ignore[invalid-argument-type, unresolved-attribute]
# mypy: disable-error-code="arg-type, attr-defined, union-attr, unreachable"
"""Digest of one node tree by socket identifier, and its layout through Node Arrange."""

from collections import ChainMap
from collections.abc import Iterable

import attrs
import bpy
from results import Fault, Faults, Resolved
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
    """Link from an output to an input, each named by node name and socket identifier, false `is_valid` on a type mismatch."""

    from_node: str
    from_socket: str
    to_node: str
    to_socket: str
    is_valid: bool
    is_muted: bool


@attrs.frozen
class Digest:
    """Tree with the RNA type of its owner, interface sockets, nodes, and links."""

    owner: str
    interface: tuple[Socket, ...]
    nodes: tuple[Node, ...]
    links: tuple[Link, ...]


@attrs.frozen
class Arranged:
    """Reroute nodes the layout added to the tree."""

    reroutes: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def trees() -> dict[str, tuple[str, bpy.types.NodeTree]]:
    """Every node tree of a registered type by owner name with the owner's RNA type, a node group before an ID pointing at a tree, IDs in `bpy.data` order."""
    owned = (
        {owner.name: (type(owner).__name__, tree) for owner in getattr(bpy.data, ids.identifier) if (tree := getattr(owner, pointer.identifier))}
        for ids in bpy.data.bl_rna.properties
        if isinstance(ids, bpy.types.CollectionProperty)
        for pointer in ids.fixed_type.properties
        if isinstance(pointer, bpy.types.PointerProperty) and isinstance(pointer.fixed_type, bpy.types.NodeTree) and not pointer.is_deprecated
    )
    chained = ChainMap({group.name: (type(group).__name__, group) for group in bpy.data.node_groups}, *owned)
    return {name: (kind, tree) for name, (kind, tree) in chained.items() if type(tree) is not bpy.types.NodeTree}


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
                changed(item, baseline.interface.new_socket(item.name, in_out=item.in_out, socket_type=item.socket_type), frozenset({"bl_socket_idname"})),
            )
            for item in tree.interface.items_tree
            if isinstance(item, bpy.types.NodeTreeInterfaceSocket)
        )
        links = tuple(Link(link.from_node.name, link.from_socket.identifier, link.to_node.name, link.to_socket.identifier, link.is_valid, link.is_muted) for link in tree.links)
        return Digest(owner, interface, nodes, links)
    finally:
        bpy.data.node_groups.remove(baseline)


def digest(name: str) -> Resolved[Digest]:
    """Return the digest of the tree owned by `name`, values rounded, layout and selection left out."""
    known = trees()
    return Faults.of(Fault(bpy.types.NodeTree, name, tuple(known))) if (hit := known.get(name)) is None else record(*hit)


def arrange(name: str) -> Resolved[Arranged]:
    """Return the reroutes Node Arrange added laying out the tree owned by `name` in a temporary node editor window drawn once for node sizes, node selection kept, refused in a process with no window."""
    manager, known = bpy.context.window_manager, trees()
    match known.get(name) or Fault(bpy.types.NodeTree, name, tuple(known)), Fault(bpy.types.Window, None) if bpy.app.background else manager.windows[0]:
        case (_, bpy.types.NodeTree() as tree), bpy.types.Window() as source:
            opened, held, selected = {w.as_pointer() for w in manager.windows}, set(tree.nodes.keys()), {n.name for n in tree.nodes if n.select}
            with bpy.context.temp_override(window=source):
                bpy.ops.wm.window_new()
            temporary = next(w for w in manager.windows if w.as_pointer() not in opened)
            area = temporary.screen.areas[0]
            area.ui_type = tree.bl_idname
            area.spaces[0].pin, area.spaces[0].node_tree = True, tree
            tree.nodes.foreach_set("select", [True] * len(tree.nodes))
            with bpy.context.temp_override(window=temporary, area=area, region=next(r for r in area.regions if r.type == "WINDOW")):
                try:
                    bpy.ops.wm.redraw_timer(type="DRAW", iterations=1)
                    bpy.ops.node.na_arrange_selected()
                finally:
                    bpy.ops.wm.window_close()
                    tree.nodes.foreach_set("select", [n.name in selected for n in tree.nodes])
            return Arranged(tuple(n.name for n in tree.nodes if n.name not in held))
        case failed:
            return Faults.of(*failed)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arranged", "Digest", "Link", "Node", "Socket", "arrange", "digest", "record", "trees"]
