"""Group interface reconciliation: interface change rows that write themselves, and the read of a live interface against a desired one."""

from collections.abc import Mapping
from functools import partial, reduce
from types import MappingProxyType

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import differing, Value, written
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.addressing import Addressed, InterfaceItem, Keyed
from blender.bpy.nodes.interfaces import DesiredItem, Interface, InterfacePanel, Socket
from blender.bpy.nodes.trees import base_socket
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item interface drops or redeclares."""

    key: Key

    def applied(self, keyed: Keyed, identity: Identity) -> Outcome[None]:
        """Removes the item and its panel toggle, then its stored key."""

        def withdrawn(live: Addressed, held: bpy.types.NodeTreeInterfaceItem) -> None:
            for toggle in live.toggle(held).to_list():
                live.face.remove(toggle)
            live.face.remove(held)
            live.store({name: identifier for name, identifier in live.stored.items() if name != self.key})

        return Addressed.of(keyed.tree, identity).bind(lambda live: live.item(self.key, bpy.types.NodeTreeInterfaceItem).map(partial(withdrawn, live)))


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create."""

    key: Key
    entry: DesiredItem

    def applied(self, keyed: Keyed, identity: Identity) -> Outcome[None]:
        """Creates the socket, or the panel with its toggle, and stores its key."""

        def created(live: Addressed) -> Outcome[InterfaceItem]:
            face, refused = live.face, partial(Fault.of, Reason.REFUSED, repr(live.face))

            def toggled(panel: bpy.types.NodeTreeInterfacePanel, *, value: bool) -> Outcome[None]:
                return (
                    catch(face.new_socket, exception=TypeError)(panel.name, in_out="INPUT", socket_type=bpy.types.NodeSocketBool.__name__, parent=panel)
                    .map_error(refused)
                    .bind(lambda socket: accumulated((written(socket, ("is_panel_toggle",), True), written(socket, ("default_value",), value))).map(lambda _: None))
                )

            match self.entry:
                case (Socket() as socket, side):
                    return catch(face.new_socket, exception=TypeError)(socket.title, description=socket.declared.description, in_out=side, socket_type=socket.base.__name__).map_error(refused)
                case InterfacePanel(description=description, closed=closed, toggle=toggle) as panel:
                    return (
                        catch(face.new_panel, exception=TypeError)(panel.title, description=description, default_closed=closed)
                        .map_error(refused)
                        .bind(lambda panel: accumulated(toggled(panel, value=value) for value in toggle.to_list()).map(lambda _: panel))
                    )

        return Addressed.of(keyed.tree, identity).bind(lambda live: created(live).map(lambda declared: live.store({**live.stored, self.key: declared.identifier})))


class Placed(msgspec.Struct, frozen=True, kw_only=True):
    """Socket type to write and place to move an item to, ahead of node writes reading the sockets they give."""

    key: Key
    kind: Option[type[bpy.types.NodeSocket]] = Nothing
    place: Option[tuple[Option[Key], int]] = Nothing

    def applied(self, keyed: Keyed, identity: Identity) -> Outcome[None]:
        """Retypes the item, reading it again under its refined class, then moves it under its panel."""

        def moved(live: Addressed, parent: Option[Key], position: int) -> Outcome[None]:
            return paired(live.item(self.key, bpy.types.NodeTreeInterfaceItem), parent.map(lambda key: live.item(key, bpy.types.NodeTreeInterfacePanel).map(Some)).default_value(Ok(Nothing))).bind(
                lambda pair: catch(live.face.move_to_parent, exception=TypeError)(pair[0], pair[1].to_optional(), position).map_error(partial(Fault.of, Reason.REFUSED, repr(live.face)))
            )

        retyped = self.kind.map(
            lambda kind: (
                Addressed
                .of(keyed.tree, identity)
                .bind(lambda live: live.item(self.key, bpy.types.NodeTreeInterfaceItem))
                .bind(lambda held: written(held, ("socket_type",), base_socket(kind).__name__))
            )
        )
        return retyped.default_value(Ok(Nothing)).bind(lambda _: self.place.map(lambda place: Addressed.of(keyed.tree, identity).bind(lambda live: moved(live, *place))).default_value(Ok(None)))


class Restated(msgspec.Struct, frozen=True, kw_only=True):
    """Item parts to write once sockets carry their values."""

    key: Key
    name: Option[str] = Nothing
    description: Option[str] = Nothing
    closed: Option[bool] = Nothing
    toggle: Option[bool] = Nothing
    members: Mapping[RnaPath, Value] = MappingProxyType({})

    def applied(self, keyed: Keyed, identity: Identity) -> Outcome[None]:
        """Writes name, description, panel state, toggle default, and socket members."""

        def rest(live: Addressed, held: bpy.types.NodeTreeInterfaceItem) -> Outcome[None]:
            return accumulated((
                *(written(held, ("name",), name) for name in self.name.to_list()),
                *(written(held, ("description",), text) for text in self.description.to_list()),
                *(written(held, ("default_closed",), closed) for closed in self.closed.to_list()),
                *(written(toggle, ("default_value",), value) for value in self.toggle.to_list() for toggle in live.toggle(held).to_list()),
                assigned(held, self.members),
            )).map(lambda _: None)

        return Addressed.of(keyed.tree, identity).bind(lambda live: live.item(self.key, bpy.types.NodeTreeInterfaceItem).bind(partial(rest, live)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def interfaced(addressed: Addressed, interface: Interface, /) -> tuple[tuple[Withdrawn, ...], tuple[Declared, ...], tuple[Placed, ...], tuple[Restated, ...]]:
    """Reads live interface against desired one, writing nothing.

    Args:
        addressed: Stored item keys and live items of the group.
        interface: Interface group declares.

    Returns:
        Items to withdraw last first, items to declare, retypes and places, siblings of each new or misplaced item placed again deepest first in preorder so every parent holds only its own items once its turn comes, and differing parts per item.
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
                return changed(item.socket_type, socket.base.__name__).map(lambda _: socket.refined)
            case _:
                return Nothing

    def restated(key: Key, entry: DesiredItem) -> Option[Restated]:
        match entry, held.get(key):
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, socket.title),
                    description=changed(item.description, socket.declared.description),
                    members=kind(key, entry).map(lambda _: socket.members).default_with(lambda: differing(item, socket.members)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, panel.title),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: all(differing(toggle, {("default_value",): value}) for toggle in addressed.toggle(item).to_list())),
                )
            case (Socket() as socket, _), _:
                row = Restated(key=key, members=socket.members)
            case _:
                row = Restated(key=key)
        return Some(row).filter(lambda differs: differs != Restated(key=key))

    depths = reduce(lambda held, entry: {**held, entry[0]: entry[2].map(lambda panel: held[panel] + 1).default_value(0)}, entries, dict[Key, int]())
    placed = sorted(
        (
            row
            for key, entry, parent, position in entries
            for row in Some(Placed(key=key, kind=kind(key, entry), place=Some((parent, position)) if parent in shifted else Nothing)).filter(lambda row: row != Placed(key=row.key)).to_list()
        ),
        key=lambda row: -depths[row.key],
    )
    return (
        tuple(Withdrawn(key=key) for identifier in reversed(tuple(addressed.live)) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh),
        tuple(Declared(key=key, entry=entry) for key, entry, _, _ in entries if key in fresh),
        tuple(placed),
        tuple(row for key, entry, _, _ in entries for row in restated(key, entry).to_list()),
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Declared", "Placed", "Restated", "Withdrawn", "interfaced"]
