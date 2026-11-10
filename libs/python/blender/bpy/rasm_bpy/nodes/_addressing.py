"""Live group addressing: stored interface item keys, panel toggles, owned nodes and their socket identifiers, and group input values."""

from collections.abc import Iterator, Mapping
from functools import partial
from types import MappingProxyType
from typing import Literal, Self

import bpy
from expression import Option, Some, try_downcast
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._ids.ownership import Mark
from rasm_bpy.data._rna.descriptors import Descriptor, Value
from rasm_bpy.nodes._desired import Content, ItemContent
from rasm_bpy.nodes._interfaces import DesiredItem, Interface, InterfacePanel, Socket
from rasm_bpy.nodes._trees import interface_of, socket_at
from rasm_records import Fault, Nothing, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type InterfaceItem = bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel

# --- [SERVICES] -------------------------------------------------------------------------


class InterfaceKeys(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item keys of one group, under the ID property name the extension holds them in, read once against its live interface items."""

    tree: bpy.types.NodeTree
    slot: str
    stored: Mapping[Key, str | int]
    live: Mapping[str | int, InterfaceItem]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, identity: Identity, /) -> Self:
        """Reads item identifiers by stored key, list entries holding keys past the 63-byte ID property name limit, and live sockets and panels."""
        slot = identity.identifier("interface")
        return cls(
            tree=tree,
            slot=slot,
            stored={row["key"]: row["identifier"] for row in tree.get(slot, ())},
            live={item.identifier: item for item in interface_of(tree).items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel)},
        )

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
                return Option.of_optional(next((child for child in item.interface_items if isinstance(child, bpy.types.NodeTreeInterfaceSocket) and child.is_panel_toggle), None))
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

    def retained(self, interface: Interface, /) -> dict[Key, str]:
        """Live socket identifiers by stored key of the items `kept` matches against `interface`."""
        kept = self.kept(interface)
        return {key: identifier for key, identifier in self.sockets.items() if key in kept}

    def item[T: bpy.types.NodeTreeInterfaceItem](self, key: Key, kind: type[T], /) -> Outcome[T]:
        """Live item at stored key as `kind`, `absent` fault naming the key otherwise."""
        identifier = self.stored.get(key)
        return (
            (Option.of_optional(self.live.get(identifier)) if identifier is not None else Nothing)
            .bind(lambda each: Option.of_optional(try_downcast(kind, each)))
            .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(interface_of(self.tree)), member=key))
        )

    def store(self, items: Mapping[Key, str | int], /) -> None:
        """Writes item identifiers by key as the tree's stored list."""
        self.tree[self.slot] = [{"key": key, "identifier": identifier} for key, identifier in items.items()]


class OwnedNodes(msgspec.Struct, frozen=True, kw_only=True):
    """Owned nodes, their interface, and socket identifiers by item key or name, read once per write phase."""

    interface: InterfaceKeys
    nodes: Mapping[Key, bpy.types.Node]
    sockets: Mapping[Key, Mapping[str, str]]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, identity: Identity, contents: Mapping[Key, Content] = MappingProxyType({}), /) -> Self:
        """Reads `owned` nodes of tree and their socket identifiers, by item key on Group Input and Output, by the addressing of the content each node holds otherwise."""
        nodes = owned(tree, identity)
        interface = InterfaceKeys.of(tree, identity)
        own = interface.sockets

        def addressed(key: Key, node: bpy.types.Node) -> Mapping[str, str]:
            match node:
                case bpy.types.NodeGroupInput() | bpy.types.NodeGroupOutput():
                    return own
                case _:
                    return Option.of_optional(try_downcast(ItemContent, contents.get(key))).map(lambda content: content.sockets(node, identity)).default_value(dict[str, str]())

        return cls(interface=interface, nodes=nodes, sockets={key: addressed(key, node) for key, node in nodes.items()})

    def node(self, key: Key) -> Outcome[bpy.types.Node]:
        """Owned node at key, `absent` fault naming key when no node holds it."""
        return Option.of_optional(self.nodes.get(key)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(self.interface.tree.nodes), member=key))

    def socket(self, key: Key, address: str | int, *, output: bool) -> Outcome[bpy.types.NodeSocket]:
        """Live socket of the node at key, addressed by item key, item name, identifier, or enabled position."""
        return self.node(key).bind(lambda live: socket_at(live.outputs if output else live.inputs, address, self.sockets[key]))


# --- [OPERATIONS] -----------------------------------------------------------------------


def owned(tree: bpy.types.NodeTree, identity: Identity) -> dict[Key, bpy.types.Node]:
    """Marked nodes by key, first in tree order over later user copies."""
    marked: dict[Key, bpy.types.Node] = {}
    for node in tree.nodes:
        for mark in Mark.read(identity, node).to_list():
            marked.setdefault(mark.key, node)
    return marked


def grouped_value(identity: Identity, tree: bpy.types.NodeTree, node: bpy.types.Node, key: Key) -> Outcome[Value]:
    """Reads a group node input value by its group's interface item key."""
    return (
        Option
        .of_optional(InterfaceKeys.of(tree, identity).sockets.get(key))
        .to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree), member=key))
        .bind(lambda found: socket_at(node.inputs, found))
        .bind(lambda held: Descriptor.resolve(held, ("default_value",), bpy.types.Property).map(lambda descriptor: descriptor.read(held)))
    )


def interface_sockets(tree: bpy.types.NodeTree, side: Literal["INPUT", "OUTPUT"] = "INPUT") -> Iterator[bpy.types.NodeTreeInterfaceSocket]:
    """Reads live interface sockets of one native side in tree order, including panel toggles."""
    return (item for item in interface_of(tree).items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == side)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["InterfaceItem", "InterfaceKeys", "OwnedNodes", "grouped_value", "interface_sockets", "owned"]
