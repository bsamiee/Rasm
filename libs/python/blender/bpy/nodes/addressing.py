"""Live group addressing: stored interface item keys, panel toggles, owned nodes and their socket identifiers, and group input values."""

from collections.abc import Mapping
from functools import partial
from types import MappingProxyType
from typing import Self

import bpy
from expression import Nothing, Option, Some, try_downcast
from expression.collections import Block
import msgspec

from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.desired import Addressing, Content, DesiredItem, Interface, InterfacePanel, Socket
from blender.bpy.nodes.trees import socket_at
from blender.records.faults import Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type InterfaceItem = bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel

# --- [SERVICES] -------------------------------------------------------------------------


class Addressed(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item keys of one group, under the ID property name the extension holds them in, read once against its live interface items."""

    tree: bpy.types.NodeTree
    slot: str
    stored: Mapping[Key, str | int]
    live: Mapping[str | int, InterfaceItem]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, identity: Identity, /) -> Outcome[Self]:
        """Reads item identifiers by stored key, list entries holding keys past the 63-byte ID property name limit, and live sockets and panels."""
        live = {item.identifier: item for item in _face(tree).items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel)}
        return identity.identifier("interface").map(
            lambda slot: cls(
                tree=tree, slot=slot, stored={row["key"]: row["identifier"] for row in Option.of_optional(tree.get(slot)).filter(lambda value: isinstance(value, list)).default_value([])}, live=live
            )
        )

    @property
    def face(self) -> bpy.types.NodeTreeInterface:
        """The tree's interface."""
        return _face(self.tree)

    @property
    def resolved(self) -> dict[Key, InterfaceItem]:
        """Live items by stored key, removed items left out."""
        return {key: self.live[identifier] for key, identifier in self.stored.items() if identifier in self.live}

    @property
    def keys(self) -> dict[str | int, Key]:
        """Stored keys by item identifier."""
        return {identifier: key for key, identifier in self.stored.items()}

    @property
    def sockets(self) -> dict[Key, str]:
        """Live socket identifiers by stored key, panel keys mapping to their toggle sockets."""
        return {key: socket.identifier for key, item in self.resolved.items() for socket in (Some(item) if isinstance(item, bpy.types.NodeTreeInterfaceSocket) else self.toggle(item)).to_list()}

    @staticmethod
    def toggle(item: bpy.types.NodeTreeInterfaceItem) -> Option[bpy.types.NodeTreeInterfaceSocket]:
        """Panel toggle socket, `Nothing` for items without one."""
        match item:
            case bpy.types.NodeTreeInterfacePanel():
                return (
                    Block
                    .of_seq(item.interface_items)
                    .choose(lambda child: Option.of_optional(try_downcast(bpy.types.NodeTreeInterfaceSocket, child)))
                    .filter(lambda child: child.is_panel_toggle)
                    .try_head()
                )
            case _:
                return Nothing

    def kept(self, interface: Interface) -> dict[Key, InterfaceItem]:
        """Live items matching item key, side, and toggle presence of `interface`."""
        resolved = self.resolved

        def matched(entry: DesiredItem, item: InterfaceItem) -> bool:
            match entry, item:
                case (Socket(), side), bpy.types.NodeTreeInterfaceSocket():
                    return item.in_out == side
                case InterfacePanel(toggle=toggle), bpy.types.NodeTreeInterfacePanel():
                    return toggle.is_some() == self.toggle(item).is_some()
                case _:
                    return False

        return {key: item for key, entry, _, _ in interface.walked() for item in (resolved.get(key),) if item is not None and matched(entry, item)}

    def item[T: bpy.types.NodeTreeInterfaceItem](self, key: Key, kind: type[T], /) -> Outcome[T]:
        """Live item at stored key as `kind`, `absent` fault naming the key otherwise."""
        return Option.of_optional(self.resolved.get(key)).bind(lambda each: Option.of_optional(try_downcast(kind, each))).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(self.face), member=key))

    def store(self, items: Mapping[Key, str | int], /) -> None:
        """Writes item identifiers by key as the tree's stored list."""
        self.tree[self.slot] = [{"key": key, "identifier": identifier} for key, identifier in items.items()]


class Keyed(msgspec.Struct, frozen=True, kw_only=True):
    """Owned nodes and their socket identifiers by item key or name, read once per write phase."""

    tree: bpy.types.NodeTree
    nodes: Mapping[Key, bpy.types.Node]
    sockets: Mapping[Key, Mapping[str, str]]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, identity: Identity, contents: Mapping[Key, Content] = MappingProxyType({}), /) -> Outcome[Self]:
        """Reads `owned` nodes of tree and their socket identifiers, by item key on Group Input and Output, by the addressing of the content each node holds otherwise."""
        nodes = owned(tree, identity)

        def addressed(own: Mapping[str, str], key: Key, node: bpy.types.Node) -> Mapping[str, str]:
            match node:
                case bpy.types.NodeGroupInput() | bpy.types.NodeGroupOutput():
                    return own
                case _:
                    return Option.of_optional(try_downcast(Addressing, contents.get(key))).map(lambda content: content.sockets(node, identity)).default_value({})

        return Addressed.of(tree, identity).map(lambda live: cls(tree=tree, nodes=nodes, sockets={key: addressed(live.sockets, key, node) for key, node in nodes.items()}))

    def node(self, key: Key) -> Outcome[bpy.types.Node]:
        """Owned node at key, `absent` fault naming key when no node holds it."""
        return Option.of_optional(self.nodes.get(key)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(self.tree.nodes), member=key))

    def socket(self, key: Key, address: str | int, *, output: bool) -> Outcome[bpy.types.NodeSocket]:
        """Live socket of the node at key, addressed by item key, item name, identifier, or enabled position."""
        return self.node(key).bind(lambda live: socket_at(live.outputs if output else live.inputs, address, self.sockets[key]))


# --- [OPERATIONS] -----------------------------------------------------------------------


def owned(tree: bpy.types.NodeTree, identity: Identity) -> dict[Key, bpy.types.Node]:
    """Marked nodes by key, first in tree order over later user copies."""
    return dict(reversed(tuple((mark.key, node) for node in tree.nodes for mark in Mark.read(identity, node).to_list())))


def grouped_value(identity: Identity, tree: bpy.types.NodeTree, node: bpy.types.Node, key: Key) -> Outcome[Value]:
    """Reads a group node input value by its group's interface item key."""
    return (
        Addressed
        .of(tree, identity)
        .bind(lambda live: Option.of_optional(live.sockets.get(key)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree), member=key)))
        .bind(lambda found: socket_at(node.inputs, found))
        .bind(lambda held: Descriptor.resolve(held, ("default_value",), bpy.types.Property).map(lambda descriptor: descriptor.read(held)))
    )


def input_sockets(tree: bpy.types.NodeTree) -> dict[str, bpy.types.NodeTreeInterfaceSocket]:
    """Reads live input sockets of a tree's interface by identifier in tree order."""
    return {item.identifier: item for item in _face(tree).items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"}


def _face(tree: bpy.types.NodeTree) -> bpy.types.NodeTreeInterface:
    """The tree's interface, the one read of a member the stubs type optional."""
    face: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return face


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Addressed", "InterfaceItem", "Keyed", "grouped_value", "input_sockets", "owned"]
