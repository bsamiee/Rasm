"""Group interface reconciliation: interface change rows that write themselves, and the read of a live interface against a desired one."""

from collections.abc import Mapping
from functools import partial
from types import MappingProxyType

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._rna.descriptors import _differing, Value, written
from rasm_bpy.data._rna.paths import _definition, RnaPath
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.nodes._addressing import InterfaceItem, InterfaceKeys, OwnedNodes
from rasm_bpy.nodes._interfaces import DesiredItem, Interface, InterfacePanel, Socket
from rasm_bpy.nodes._trees import _base_socket, _interface_of
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class _Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item interface drops or redeclares."""

    key: Key

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Removes the item and its panel toggle, then its stored key."""
        live = InterfaceKeys.of(keyed.interface.tree, identity)

        def withdrawn(held: bpy.types.NodeTreeInterfaceItem) -> None:
            interface = _interface_of(live.tree)
            for toggle in live.toggle(held).to_list():
                interface.remove(toggle)
            interface.remove(held)
            live.store({name: identifier for name, identifier in live.stored.items() if name != self.key})

        return live.item(self.key, bpy.types.NodeTreeInterfaceItem).map(withdrawn)


class _Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create."""

    key: Key
    entry: DesiredItem

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Creates the socket, or the panel with its toggle, and stores its key."""
        live = InterfaceKeys.of(keyed.interface.tree, identity)

        def created() -> Outcome[InterfaceItem]:
            interface = _interface_of(live.tree)
            refused = partial(Fault.of, Reason.REFUSED, repr(interface))

            def toggled(panel: bpy.types.NodeTreeInterfacePanel, *, value: bool) -> Outcome[None]:
                return (
                    catch(interface.new_socket, exception=TypeError)(panel.name, in_out="INPUT", socket_type=_definition(bpy.types.NodeSocketBool).identifier, parent=panel)
                    .map_error(refused)
                    .bind(lambda socket: accumulated((written(socket, ("is_panel_toggle",), value=True), written(socket, ("default_value",), value))).map(lambda _: None))
                )

            match self.entry:
                case (Socket() as socket, side):
                    return catch(interface.new_socket, exception=TypeError)(
                        socket.title, description=socket.declared.description, in_out=side, socket_type=_definition(socket.base).identifier
                    ).map_error(refused)
                case InterfacePanel(description=description, closed=closed, toggle=toggle) as panel:
                    return (
                        catch(interface.new_panel, exception=TypeError)(panel.title, description=description, default_closed=closed)
                        .map_error(refused)
                        .bind(lambda panel: accumulated(toggled(panel, value=value) for value in toggle.to_list()).map(lambda _: panel))
                    )

        return created().map(lambda declared: live.store({**live.stored, self.key: declared.identifier}))


class _Placed(msgspec.Struct, frozen=True, kw_only=True):
    """Socket type to write and place to move an item to, ahead of node writes reading the sockets they give."""

    key: Key
    kind: Option[type[bpy.types.NodeSocket]] = Nothing
    place: Option[tuple[Option[Key], int]] = Nothing

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Retypes the item, reading it again under its refined class, then moves it under its panel."""

        def moved(live: InterfaceKeys, parent: Option[Key], position: int) -> Outcome[None]:
            return paired(live.item(self.key, bpy.types.NodeTreeInterfaceItem), parent.map(lambda key: live.item(key, bpy.types.NodeTreeInterfacePanel).map(Some)).default_value(Ok(Nothing))).bind(
                lambda pair: catch(_interface_of(live.tree).move_to_parent, exception=TypeError)(pair[0], pair[1].to_optional(), position).map_error(
                    partial(Fault.of, Reason.REFUSED, repr(_interface_of(live.tree)))
                )
            )

        retyped = self.kind.map(
            lambda kind: (
                InterfaceKeys
                .of(keyed.interface.tree, identity)
                .item(self.key, bpy.types.NodeTreeInterfaceItem)
                .bind(lambda held: written(held, ("socket_type",), _definition(_base_socket(kind)).identifier))
            )
        )
        return retyped.default_value(Ok(None)).bind(lambda _: self.place.map(lambda place: moved(InterfaceKeys.of(keyed.interface.tree, identity), *place)).default_value(Ok(None)))


class _Restated(msgspec.Struct, frozen=True, kw_only=True):
    """Item parts to write once sockets hold their values."""

    key: Key
    name: Option[str] = Nothing
    description: Option[str] = Nothing
    closed: Option[bool] = Nothing
    toggle: Option[bool] = Nothing
    members: Mapping[RnaPath, Value] = MappingProxyType({})

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Writes name, description, panel state, toggle default, and socket members."""
        live = InterfaceKeys.of(keyed.interface.tree, identity)

        def rest(held: bpy.types.NodeTreeInterfaceItem) -> Outcome[None]:
            return accumulated((
                *(written(held, ("name",), name) for name in self.name.to_list()),
                *(written(held, ("description",), text) for text in self.description.to_list()),
                *(written(held, ("default_closed",), closed) for closed in self.closed.to_list()),
                *(written(toggle, ("default_value",), value) for value in self.toggle.to_list() for toggle in live.toggle(held).to_list()),
                assigned(held, self.members),
            )).map(lambda _: None)

        return live.item(self.key, bpy.types.NodeTreeInterfaceItem).bind(rest)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _interfaced(addressed: InterfaceKeys, interface: Interface, /) -> tuple[tuple[_Withdrawn, ...], tuple[_Declared, ...], tuple[_Placed, ...], tuple[_Restated, ...]]:
    """Reads live interface against desired one, writing nothing.

    Args:
        addressed: Stored item keys and live items of the group.
        interface: Interface group declares.

    Returns:
        Items to withdraw last first, items to declare, retypes and places deepest first, and differing parts per item.
    """
    entries = tuple(interface.walked())
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
                return changed(item.socket_type, _definition(socket.base).identifier).map(lambda _: socket.refined)
            case _:
                return Nothing

    def restated(key: Key, entry: DesiredItem) -> Option[_Restated]:
        match entry, held.get(key):
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                row = _Restated(
                    key=key,
                    name=changed(item.name, socket.title),
                    description=changed(item.description, socket.declared.description),
                    members=kind(key, entry).map(lambda _: socket.members).default_with(lambda: _differing(item, socket.members)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                row = _Restated(
                    key=key,
                    name=changed(item.name, panel.title),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: all(_differing(toggle, {("default_value",): value}) for toggle in addressed.toggle(item).to_list())),
                )
            case (Socket() as socket, _), _:
                row = _Restated(key=key, members=socket.members)
            case _:
                row = _Restated(key=key)
        return Some(row).filter(lambda differs: differs != _Restated(key=key))

    depths: dict[Key, int] = {}
    for key, _, parent, _ in entries:
        depths[key] = parent.map(lambda panel: depths[panel] + 1).default_value(0)
    placed = sorted(
        (
            row
            for key, entry, parent, position in entries
            for row in Some(_Placed(key=key, kind=kind(key, entry), place=Some((parent, position)) if parent in shifted else Nothing)).filter(lambda row: row != _Placed(key=row.key)).to_list()
        ),
        key=lambda row: -depths[row.key],
    )
    return (
        tuple(_Withdrawn(key=key) for identifier in reversed(tuple(addressed.live)) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh),
        tuple(_Declared(key=key, entry=entry) for key, entry, _, _ in entries if key in fresh),
        tuple(placed),
        tuple(row for key, entry, _, _ in entries for row in restated(key, entry).to_list()),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
