"""Desired group interfaces, curve axes, stored item keys, and live interface writes."""

from collections.abc import Iterator, Mapping
from functools import partial
from types import MemberDescriptorType
from typing import Literal, Self

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import msgspec.structs

from blender.bpy.data.rna.descriptors import assigned, differing, Value, written
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type SocketSide = Literal["INPUT", "OUTPUT"]
type InterfaceItem = bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel
type DesiredItem = tuple[Socket, SocketSide] | InterfacePanel

# --- [MODELS] ---------------------------------------------------------------------------


class MenuItem(msgspec.Struct, frozen=True, kw_only=True):
    """Menu Switch item."""

    name: str
    description: str = ""


class MenuItems(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered Menu Switch items."""

    items: tuple[MenuItem, ...]

    @classmethod
    def of(cls, prop: bpy.types.EnumProperty) -> Self:
        """Builds items from the RNA enum the menu mirrors, named and described by its items."""
        return cls(items=tuple(MenuItem(name=item.name, description=item.description) for item in prop.enum_items))


class Socket(msgspec.Struct, frozen=True, kw_only=True):
    """Interface socket by parameter key, group input menu items kept only while linked to a Menu Switch."""

    key: Key
    name: str
    kind: type[bpy.types.NodeSocket]
    description: str = ""
    members: Mapping[str, Value] = msgspec.field(default_factory=dict)
    items: Option[MenuItems] = Nothing


class Interface(msgspec.Struct, frozen=True, kw_only=True):
    """Group interface items in Blender order."""

    outputs: tuple[Socket, ...] = ()
    inputs: tuple[Socket, ...] = ()
    panels: tuple["InterfacePanel", ...] = ()

    def walked(self, parent: Option[Key] = Nothing) -> Iterator[tuple[Key, DesiredItem, Option[Key], int]]:
        """Walks interface items in preorder.

        Args:
            parent: Key of panel holding items, `Nothing` at root.

        Yields:
            Item key, socket with side or panel, parent key, and position after toggle socket a toggled panel holds first.
        """
        items: tuple[DesiredItem, ...] = (*((socket, "OUTPUT") for socket in self.outputs), *((socket, "INPUT") for socket in self.inputs), *self.panels)
        for position, item in enumerate(items, int(isinstance(self, InterfacePanel) and self.toggle.is_some())):
            match item:
                case InterfacePanel(key=key):
                    yield key, item, parent, position
                    yield from item.walked(Some(key))
                case (socket, _):
                    yield socket.key, item, parent, position


class InterfacePanel(Interface, frozen=True, kw_only=True):
    """Interface panel with optional toggle."""

    key: Key
    name: str
    description: str = ""
    closed: bool = False
    toggle: Option[bool] = Nothing


class Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item interface drops or redeclares."""

    key: Key


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create."""

    key: Key
    entry: DesiredItem


class Restated(msgspec.Struct, frozen=True, kw_only=True):
    """Item parts and place to write."""

    key: Key
    kind: Option[type[bpy.types.NodeSocket]] = Nothing
    name: Option[str] = Nothing
    description: Option[str] = Nothing
    closed: Option[bool] = Nothing
    toggle: Option[bool] = Nothing
    place: Option[tuple[Option[Key], int]] = Nothing
    members: Mapping[str, Value] = msgspec.field(default_factory=dict)


class CurveAxes(msgspec.Struct, frozen=True, kw_only=True):
    """Record curve placed in its node's own axes, the shown height `scale * y + slope * x + offset`."""

    scale: float = 1.0
    slope: float = 0.0
    offset: float = 0.0

    def shown(self, x: float, y: float) -> float:
        """Returns node height of record point."""
        return self.scale * y + self.slope * x + self.offset

    def held(self, x: float, y: float) -> float:
        """Returns record height of node point."""
        return (y - self.slope * x - self.offset) / self.scale


# --- [SERVICES] -------------------------------------------------------------------------


class Addressed(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item keys of one group read once against its live interface items."""

    stored: Mapping[Key, str | int]
    live: Mapping[str | int, InterfaceItem]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, slot: str) -> Self:
        """Reads item identifiers by stored key, list entries holding keys past 63-byte ID property name limit, and live sockets and panels."""
        live = {item.identifier: item for item in tree.interface.items_tree if isinstance(item, bpy.types.NodeTreeInterfaceSocket | bpy.types.NodeTreeInterfacePanel)}  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
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
        """Live items matching item key, kind, side, and toggle presence of `interface`."""
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


# --- [OPERATIONS] -----------------------------------------------------------------------


def item_key(*parts: Key | int | type[Case] | MemberDescriptorType) -> Key:
    """Joins key path into item key, record fields by their member descriptor's name and case classes as lowercase names."""

    def part(value: Key | int | type[Case] | MemberDescriptorType) -> str:
        match value:
            case MemberDescriptorType():
                return value.__name__
            case type():
                return value.__name__.lower()
            case str() | int():
                return str(value)

    return "_".join(map(part, parts))


def under[I: Interface](prefix: Key, face: I) -> I:
    """Returns interface with every key prefixed through `item_key`, for a group nested in another group's panel."""
    return msgspec.structs.replace(
        face,
        outputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key)) for socket in face.outputs),
        inputs=tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key)) for socket in face.inputs),
        panels=tuple(msgspec.structs.replace(under(prefix, panel), key=item_key(prefix, panel.key)) for panel in face.panels),
    )


def slot(identity: Identity) -> Outcome[str]:
    """Returns ID property name holding group item keys, or refused attachment name fault."""
    return identity.identifier("interface")


def addresses(tree: bpy.types.NodeTree, slot: str) -> dict[Key, str]:
    """Reads live socket identifiers by item key, panel keys mapping to toggle sockets."""
    return Addressed.of(tree, slot).sockets


def interfaced(tree: bpy.types.NodeTree, interface: Interface, slot: str) -> tuple[tuple[Withdrawn, ...], tuple[Declared, ...], tuple[Restated, ...]]:
    """Reads live interface against desired one, writing nothing.

    Args:
        tree: Group to read.
        interface: Interface group declares.
        slot: ID property name holding stored item keys.

    Returns:
        Items to withdraw last first, items to declare, and differing parts per item, siblings of each new or misplaced item placed again in preorder.
    """
    entries = tuple(interface.walked())
    addressed = Addressed.of(tree, slot)
    inverse = addressed.keys
    held = addressed.kept(interface)
    wanted = frozenset(key for key, *_ in entries)
    fresh = wanted - held.keys()
    placed = {key: (Option.of_optional(item.parent).bind(lambda panel: Option.of_optional(inverse.get(panel.identifier))), item.position) for key, item in held.items()}
    shifted = frozenset(parent for key, _, parent, position in entries if key in fresh or placed.get(key) != (parent, position) or any(frame in fresh for frame in parent.to_list()))

    def restated(key: Key, entry: DesiredItem, place: Option[tuple[Option[Key], int]]) -> Option[Restated]:
        def changed[T](now: T, wanted: T) -> Option[T]:
            return Nothing if now == wanted else Some(wanted)

        match entry, held.get(key):
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                kind = Nothing if item.socket_type == socket.kind.__name__ else Some(socket.kind)
                row = Restated(
                    key=key,
                    kind=kind,
                    name=changed(item.name, socket.name),
                    description=changed(item.description, socket.description),
                    place=place,
                    members=kind.map(lambda _: socket.members).default_with(lambda: differing(item, socket.members)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, panel.name),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: all(differing(toggle, {"default_value": value}) for toggle in _toggle(item).to_list())),
                    place=place,
                )
            case (Socket() as socket, _), _:
                row = Restated(key=key, place=place, members=socket.members)
            case _:
                row = Restated(key=key, place=place)
        return Some(row).filter(lambda differs: differs != Restated(key=key))

    return (
        tuple(Withdrawn(key=key) for identifier in reversed(tuple(addressed.live)) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh),
        tuple(Declared(key=key, entry=entry) for key, entry, _, _ in entries if key in fresh),
        tuple(row for key, entry, parent, position in entries for row in restated(key, entry, Some((parent, position)) if parent in shifted else Nothing).to_list()),
    )


def revised(row: Withdrawn | Declared | Restated, tree: bpy.types.NodeTree, slot: str) -> Outcome[None]:
    """Writes one interface row onto a group, reading live items again after each write that replaces them.

    Args:
        row: Interface row to write.
        tree: Group row names.
        slot: ID property name holding stored item keys.

    Returns:
        `Ok` once written. `Error` of `absent` for unresolved item, `edit` per refused socket type or write.
    """
    face: bpy.types.NodeTreeInterface = tree.interface  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def stored(items: dict[Key, str | int]) -> None:
        tree[slot] = [{"key": key, "identifier": identifier} for key, identifier in items.items()]

    def item[T: bpy.types.NodeTreeInterfaceItem](key: Key, kind: type[T]) -> Outcome[T]:
        return (
            Option
            .of_optional(Addressed.of(tree, slot).resolved.get(key))
            .bind(lambda each: Option.of_optional(try_downcast(kind, each)))
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(face), member=key))
        )

    def withdrawn(held: bpy.types.NodeTreeInterfaceItem, key: Key) -> None:
        for toggle in _toggle(held).to_list():
            face.remove(toggle)
        face.remove(held)
        stored({name: identifier for name, identifier in Addressed.of(tree, slot).stored.items() if name != key})

    refused = partial(Fault.of, BlenderReason.REFUSED, repr(face))

    def made(key: Key, entry: DesiredItem) -> Outcome[None]:
        def toggled(panel: bpy.types.NodeTreeInterfacePanel, *, value: bool) -> Outcome[None]:
            return (
                catch(lambda: face.new_socket(panel.name, in_out="INPUT", socket_type=bpy.types.NodeSocketBool.__name__), exception=TypeError)()  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                .bind(lambda socket: catch(face.move_to_parent, exception=TypeError)(socket, panel, 0).map(lambda _: socket))
                .map_error(refused)
                .bind(lambda socket: accumulated((written(socket, "is_panel_toggle", True), written(socket, "default_value", value))).map(lambda _: None))
            )

        def created() -> Outcome[InterfaceItem]:
            match entry:
                case tuple(pair):
                    socket, side = pair
                    return catch(lambda: face.new_socket(socket.name, description=socket.description, in_out=side, socket_type=socket.kind.__name__), exception=TypeError)().map_error(refused)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                case InterfacePanel(name=name, description=description, closed=closed, toggle=toggle):
                    return (
                        catch(lambda: face.new_panel(name, description=description, default_closed=closed), exception=TypeError)()
                        .map_error(refused)
                        .bind(lambda panel: accumulated(toggled(panel, value=value) for value in toggle.to_list()).map(lambda _: panel))
                    )

        return created().map(lambda declared: stored({**Addressed.of(tree, slot).stored, key: declared.identifier}))

    def restated(change: Restated) -> Outcome[None]:
        def retyped(held: bpy.types.NodeTreeInterfaceItem) -> Outcome[bpy.types.NodeTreeInterfaceItem]:
            return change.kind.map(lambda kind: written(held, "socket_type", kind.__name__).bind(lambda _: item(change.key, bpy.types.NodeTreeInterfaceItem))).default_value(Ok(held))

        def moved(held: bpy.types.NodeTreeInterfaceItem, parent: Option[Key], position: int) -> Outcome[None]:
            return (
                parent
                .map(lambda key: item(key, bpy.types.NodeTreeInterfacePanel).map(Some))
                .default_value(Ok(Nothing))
                .bind(lambda panel: catch(face.move_to_parent, exception=TypeError)(held, panel.to_optional(), position).map_error(refused))
            )

        def rest(held: bpy.types.NodeTreeInterfaceItem) -> Outcome[None]:
            return accumulated((
                *(written(held, "name", name) for name in change.name.to_list()),
                *(written(held, "description", text) for text in change.description.to_list()),
                *(written(held, "default_closed", closed) for closed in change.closed.to_list()),
                *(written(toggle, "default_value", value) for value in change.toggle.to_list() for toggle in _toggle(held).to_list()),
                *(moved(held, *place) for place in change.place.to_list()),
                assigned(held, change.members),
            )).map(lambda _: None)

        return item(change.key, bpy.types.NodeTreeInterfaceItem).bind(retyped).bind(rest)

    match row:
        case Withdrawn(key=key):
            return item(key, bpy.types.NodeTreeInterfaceItem).map(lambda held: withdrawn(held, key))
        case Declared(key=key, entry=entry):
            return made(key, entry)
        case Restated():
            return restated(row)


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

__all__ = [
    "Addressed",
    "CurveAxes",
    "Declared",
    "DesiredItem",
    "Interface",
    "InterfaceItem",
    "InterfacePanel",
    "MenuItem",
    "MenuItems",
    "Restated",
    "Socket",
    "SocketSide",
    "Withdrawn",
    "addresses",
    "interfaced",
    "item_key",
    "revised",
    "slot",
    "under",
]
