"""Live group addressing: stored interface item keys, owned nodes and their socket identifiers, group input values, and interface rows that write themselves."""

from collections.abc import Mapping
from functools import partial
from types import MappingProxyType
from typing import Protocol, Self

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.rna.descriptors import Descriptor, differing, Value, written
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.interface import DesiredItem, Interface, InterfacePanel, Socket
from blender.bpy.nodes.trees import base_socket, socket_at
from blender.records.faults import accumulated, Fault, Outcome

# --- [TYPES] ----------------------------------------------------------------------------

type InterfaceItem = bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel


class Addressing(Protocol):
    """Node content addressing its node's sockets by item key or item name."""

    def sockets(self, live: bpy.types.Node, identity: Identity, slot: str, /) -> Mapping[str, str]:
        """Socket identifiers by item key or item name."""


# --- [SERVICES] -------------------------------------------------------------------------

# --- [ADDRESSING]


class Addressed(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item keys of one group read once against its live interface items."""

    stored: Mapping[Key, str | int]
    live: Mapping[str | int, InterfaceItem]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, slot: str) -> Self:
        """Reads item identifiers by stored key, list entries holding keys past 63-byte ID property name limit, and live sockets and panels."""
        live = {item.identifier: item for item in _face(tree).items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel)}
        return cls(stored={row["key"]: row["identifier"] for row in Option.of_optional(tree.get(slot)).filter(lambda value: isinstance(value, list)).default_value([])}, live=live)

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
        return {key: socket.identifier for key, item in self.resolved.items() for socket in (Some(item) if isinstance(item, bpy.types.NodeTreeInterfaceSocket) else _toggle(item)).to_list()}

    def kept(self, interface: Interface) -> dict[Key, InterfaceItem]:
        """Live items matching item key, side, and toggle presence of `interface`."""
        resolved = self.resolved

        def matched(entry: DesiredItem, item: InterfaceItem) -> bool:
            match entry, item:
                case (Socket(), side), bpy.types.NodeTreeInterfaceSocket():
                    return item.in_out == side
                case InterfacePanel(toggle=toggle), bpy.types.NodeTreeInterfacePanel():
                    return toggle.is_some() == _toggle(item).is_some()
                case _:
                    return False

        return {key: item for key, entry, _, _ in interface.walked() for item in (resolved.get(key),) if item is not None and matched(entry, item)}


class Keyed(msgspec.Struct, frozen=True, kw_only=True):
    """Owned nodes and their socket identifiers by item key or name, read once per write phase."""

    tree: bpy.types.NodeTree
    slot: str
    nodes: Mapping[Key, bpy.types.Node]
    sockets: Mapping[Key, Mapping[str, str]]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, identity: Identity, slot: str, contents: Mapping[Key, Addressing] = MappingProxyType({})) -> Self:
        """Reads `owned` nodes of tree and their socket identifiers, by item key on Group Input and Output, by the addressing of the content each node holds otherwise."""
        nodes, own = owned(tree, identity), Addressed.of(tree, slot).sockets

        def addressed(key: Key, node: bpy.types.Node) -> Mapping[str, str]:
            match node:
                case bpy.types.NodeGroupInput() | bpy.types.NodeGroupOutput():
                    return own
                case _:
                    return Option.of_optional(contents.get(key)).map(lambda content: content.sockets(node, identity, slot)).default_value({})

        return cls(tree=tree, slot=slot, nodes=nodes, sockets={key: addressed(key, node) for key, node in nodes.items()})

    def node(self, key: Key) -> Outcome[bpy.types.Node]:
        """Owned node at key, `absent` fault naming key when no node holds it."""
        return Option.of_optional(self.nodes.get(key)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(self.tree.nodes), member=key))

    def socket(self, key: Key, address: str | int, *, output: bool) -> Outcome[bpy.types.NodeSocket]:
        """Live socket of the node at key, addressed by item key, item name, identifier, or enabled position."""
        return self.node(key).bind(lambda live: socket_at(live.outputs if output else live.inputs, address if isinstance(address, int) else self.sockets[key].get(address, address)))


# --- [ROWS]


class Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item interface drops or redeclares."""

    key: Key

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Removes the item and its panel toggle, then its stored key."""
        face = _face(keyed.tree)

        def withdrawn(held: bpy.types.NodeTreeInterfaceItem) -> None:
            for toggle in _toggle(held).to_list():
                face.remove(toggle)
            face.remove(held)
            _stored(keyed, {name: identifier for name, identifier in Addressed.of(keyed.tree, keyed.slot).stored.items() if name != self.key})

        return _item(keyed, self.key, bpy.types.NodeTreeInterfaceItem).map(withdrawn)


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create."""

    key: Key
    entry: DesiredItem

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Creates the socket, or the panel with its toggle, and stores its key."""
        face = _face(keyed.tree)
        refused = partial(Fault.of, BlenderReason.REFUSED, repr(face))

        def toggled(panel: bpy.types.NodeTreeInterfacePanel, *, value: bool) -> Outcome[None]:
            return (
                catch(face.new_socket, exception=TypeError)(panel.name, in_out="INPUT", socket_type=bpy.types.NodeSocketBool.__name__, parent=panel)
                .map_error(refused)
                .bind(lambda socket: accumulated((written(socket, ("is_panel_toggle",), True), written(socket, ("default_value",), value))).map(lambda _: None))
            )

        def created() -> Outcome[InterfaceItem]:
            match self.entry:
                case tuple(pair):
                    socket, side = pair
                    return catch(face.new_socket, exception=TypeError)(socket.title, description=socket.description, in_out=side, socket_type=socket.base.__name__).map_error(refused)
                case InterfacePanel(name=name, description=description, closed=closed, toggle=toggle):
                    return (
                        catch(face.new_panel, exception=TypeError)(name, description=description, default_closed=closed)
                        .map_error(refused)
                        .bind(lambda panel: accumulated(toggled(panel, value=value) for value in toggle.to_list()).map(lambda _: panel))
                    )

        return created().map(lambda declared: _stored(keyed, {**Addressed.of(keyed.tree, keyed.slot).stored, self.key: declared.identifier}))


class Placed(msgspec.Struct, frozen=True, kw_only=True):
    """Socket type to write and place to move an item to, ahead of node writes reading the sockets they give."""

    key: Key
    kind: Option[type[bpy.types.NodeSocket]] = Nothing
    place: Option[tuple[Option[Key], int]] = Nothing

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Retypes the item, reading it again, then moves it under its panel."""
        face = _face(keyed.tree)

        def retyped(held: bpy.types.NodeTreeInterfaceItem) -> Outcome[bpy.types.NodeTreeInterfaceItem]:
            return self.kind.map(lambda kind: written(held, ("socket_type",), base_socket(kind).__name__).bind(lambda _: _item(keyed, self.key, bpy.types.NodeTreeInterfaceItem))).default_value(
                Ok(held)
            )

        def moved(held: bpy.types.NodeTreeInterfaceItem, parent: Option[Key], position: int) -> Outcome[None]:
            return (
                parent
                .map(lambda key: _item(keyed, key, bpy.types.NodeTreeInterfacePanel).map(Some))
                .default_value(Ok(Nothing))
                .bind(lambda panel: catch(face.move_to_parent, exception=TypeError)(held, panel.to_optional(), position).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(face))))
            )

        return _item(keyed, self.key, bpy.types.NodeTreeInterfaceItem).bind(retyped).bind(lambda held: self.place.map(lambda place: moved(held, *place)).default_value(Ok(None)))


class Restated(msgspec.Struct, frozen=True, kw_only=True):
    """Item parts to write once sockets carry their values."""

    key: Key
    name: Option[str] = Nothing
    description: Option[str] = Nothing
    closed: Option[bool] = Nothing
    toggle: Option[bool] = Nothing
    members: Mapping[RnaPath, Value] = MappingProxyType({})

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Writes name, description, panel state, toggle default, and socket members."""

        def rest(held: bpy.types.NodeTreeInterfaceItem) -> Outcome[None]:
            return accumulated((
                *(written(held, ("name",), name) for name in self.name.to_list()),
                *(written(held, ("description",), text) for text in self.description.to_list()),
                *(written(held, ("default_closed",), closed) for closed in self.closed.to_list()),
                *(written(toggle, ("default_value",), value) for value in self.toggle.to_list() for toggle in _toggle(held).to_list()),
                assigned(held, self.members),
            )).map(lambda _: None)

        return _item(keyed, self.key, bpy.types.NodeTreeInterfaceItem).bind(rest)


# --- [OPERATIONS] -----------------------------------------------------------------------


def slot(identity: Identity) -> Outcome[str]:
    """Returns ID property name holding group item keys, or refused attachment name fault."""
    return identity.identifier("interface")


def owned(tree: bpy.types.NodeTree, identity: Identity) -> dict[Key, bpy.types.Node]:
    """Marked nodes by key, first in tree order over later user copies."""
    return dict(reversed(tuple((mark.key, node) for node in tree.nodes for mark in Mark.read(identity, node).to_list())))


def grouped_value(identity: Identity, tree: bpy.types.NodeTree, node: bpy.types.Node, key: Key) -> Outcome[Value]:
    """Reads a group node input value by its group's interface item key."""
    identifier = slot(identity).bind(lambda name: Option.of_optional(Addressed.of(tree, name).sockets.get(key)).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(tree), member=key)))
    socket = identifier.bind(
        lambda found: Block.of_seq(node.inputs).filter(lambda held: held.identifier == found).try_head().to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(node), member=found))
    )
    return socket.bind(lambda held: Descriptor.resolve(held, ("default_value",), bpy.types.Property).map(lambda descriptor: descriptor.read(held)))


def input_sockets(tree: bpy.types.NodeTree) -> dict[str, bpy.types.NodeTreeInterfaceSocket]:
    """Reads live input sockets of a tree's interface by identifier in tree order."""
    return {item.identifier: item for item in _face(tree).items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket) and item.in_out == "INPUT"}


def interfaced(tree: bpy.types.NodeTree, interface: Interface, slot: str) -> tuple[tuple[Withdrawn, ...], tuple[Declared, ...], tuple[Placed, ...], tuple[Restated, ...]]:
    """Reads live interface against desired one, writing nothing.

    Args:
        tree: Group to read.
        interface: Interface group declares.
        slot: ID property name holding stored item keys.

    Returns:
        Items to withdraw last first, items to declare, retypes and places, siblings of each new or misplaced item placed again in preorder, and differing parts per item.
    """
    entries = tuple(interface.walked())
    addressed = Addressed.of(tree, slot)
    inverse = addressed.keys
    held = addressed.kept(interface)
    wanted = frozenset(key for key, *_ in entries)
    fresh = wanted - held.keys()
    located = {key: (Option.of_optional(item.parent).bind(lambda panel: Option.of_optional(inverse.get(panel.identifier))), item.position) for key, item in held.items()}
    shifted = frozenset(parent for key, _, parent, position in entries if key in fresh or located.get(key) != (parent, position) or any(frame in fresh for frame in parent.to_list()))

    def changed[T](now: T, wanted: T) -> Option[T]:
        return Nothing if now == wanted else Some(wanted)

    def kind(key: Key, entry: DesiredItem) -> Option[type[bpy.types.NodeSocket]]:
        match entry, held.get(key):
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                return changed(item.socket_type, socket.base.__name__).map(lambda _: socket.kind)
            case _:
                return Nothing

    def restated(key: Key, entry: DesiredItem) -> Option[Restated]:
        match entry, held.get(key):
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, socket.title),
                    description=changed(item.description, socket.description),
                    members=kind(key, entry).map(lambda _: socket.members).default_with(lambda: differing(item, socket.members)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, panel.name),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: all(differing(toggle, {("default_value",): value}) for toggle in _toggle(item).to_list())),
                )
            case (Socket() as socket, _), _:
                row = Restated(key=key, members=socket.members)
            case _:
                row = Restated(key=key)
        return Some(row).filter(lambda differs: differs != Restated(key=key))

    placed = tuple(
        row
        for key, entry, parent, position in entries
        for row in Some(Placed(key=key, kind=kind(key, entry), place=Some((parent, position)) if parent in shifted else Nothing)).filter(lambda row: row != Placed(key=row.key)).to_list()
    )
    return (
        tuple(Withdrawn(key=key) for identifier in reversed(tuple(addressed.live)) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh),
        tuple(Declared(key=key, entry=entry) for key, entry, _, _ in entries if key in fresh),
        placed,
        tuple(row for key, entry, _, _ in entries for row in restated(key, entry).to_list()),
    )


def _face(tree: bpy.types.NodeTree) -> bpy.types.NodeTreeInterface:
    """The tree's interface, the one read of a member the stubs type optional."""
    face: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return face


def _stored(keyed: Keyed, items: Mapping[Key, str | int]) -> None:
    keyed.tree[keyed.slot] = [{"key": key, "identifier": identifier} for key, identifier in items.items()]


def _item[T: bpy.types.NodeTreeInterfaceItem](keyed: Keyed, key: Key, kind: type[T]) -> Outcome[T]:
    return (
        Option
        .of_optional(Addressed.of(keyed.tree, keyed.slot).resolved.get(key))
        .bind(lambda each: Option.of_optional(try_downcast(kind, each)))
        .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(_face(keyed.tree)), member=key))
    )


def _toggle(item: bpy.types.NodeTreeInterfaceItem) -> Option[bpy.types.NodeTreeInterfaceSocket]:
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Addressed", "Addressing", "Declared", "InterfaceItem", "Keyed", "Placed", "Restated", "Withdrawn", "grouped_value", "input_sockets", "interfaced", "owned", "slot"]
