"""Group interface change rows that write themselves."""

from collections.abc import Mapping
from functools import partial
from types import MappingProxyType

import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._rna.descriptors import Value, written
from rasm_bpy.data._rna.paths import rna_definition, RnaPath
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.nodes._addressing import InterfaceItem, InterfaceKeys, OwnedNodes
from rasm_bpy.nodes._interfaces import DesiredItem, InterfacePanel, Socket
from rasm_bpy.nodes._trees import base_socket, interface_of
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired

# --- [MODELS] ---------------------------------------------------------------------------


class Withdrawn(msgspec.Struct, frozen=True, kw_only=True):
    """Stored item interface drops or redeclares."""

    key: Key

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Removes the item and its panel toggle, then its stored key."""
        live = InterfaceKeys.of(keyed.interface.tree, identity)

        def withdrawn(held: bpy.types.NodeTreeInterfaceItem) -> None:
            interface = interface_of(live.tree)
            for toggle in live.toggle(held).to_list():
                interface.remove(toggle)
            interface.remove(held)
            live.store({name: identifier for name, identifier in live.stored.items() if name != self.key})

        return live.item(self.key, bpy.types.NodeTreeInterfaceItem).map(withdrawn)


class Declared(msgspec.Struct, frozen=True, kw_only=True):
    """Interface item to create."""

    key: Key
    entry: DesiredItem

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Creates the socket, or the panel with its toggle, and stores its key."""
        live = InterfaceKeys.of(keyed.interface.tree, identity)

        def created() -> Outcome[InterfaceItem]:
            interface = interface_of(live.tree)
            refused = partial(Fault.of, Refusal.REFUSED, repr(interface))

            def toggled(panel: bpy.types.NodeTreeInterfacePanel, *, value: bool) -> Outcome[None]:
                return (
                    catch(interface.new_socket, exception=TypeError)(panel.name, in_out="INPUT", socket_type=rna_definition(bpy.types.NodeSocketBool).identifier, parent=panel)
                    .map_error(refused)
                    .bind(lambda socket: accumulated((written(socket, ("is_panel_toggle",), value=True), written(socket, ("default_value",), value))).map(lambda _: None))
                )

            match self.entry:
                case (Socket() as socket, side):
                    return catch(interface.new_socket, exception=TypeError)(
                        socket.title, description=socket.declared.description, in_out=side, socket_type=rna_definition(socket.base).identifier
                    ).map_error(refused)
                case InterfacePanel(description=description, closed=closed, toggle=toggle) as panel:
                    return (
                        catch(interface.new_panel, exception=TypeError)(panel.title, description=description, default_closed=closed)
                        .map_error(refused)
                        .bind(lambda panel: accumulated(toggled(panel, value=value) for value in toggle.to_list()).map(lambda _: panel))
                    )

        return created().map(lambda declared: live.store({**live.stored, self.key: declared.identifier}))


class Placed(msgspec.Struct, frozen=True, kw_only=True):
    """Socket type to write and place to move an item to, ahead of node writes reading the sockets they give."""

    key: Key
    kind: Option[type[bpy.types.NodeSocket]] = Nothing
    place: Option[tuple[Option[Key], int]] = Nothing

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Retypes the item, reading it again under its refined class, then moves it under its panel."""

        def moved(live: InterfaceKeys, parent: Option[Key], position: int) -> Outcome[None]:
            return paired(live.item(self.key, bpy.types.NodeTreeInterfaceItem), parent.map(lambda key: live.item(key, bpy.types.NodeTreeInterfacePanel).map(Some)).default_value(Ok(Nothing))).bind(
                lambda pair: catch(interface_of(live.tree).move_to_parent, exception=TypeError)(pair[0], pair[1].to_optional(), position).map_error(
                    partial(Fault.of, Refusal.REFUSED, repr(interface_of(live.tree)))
                )
            )

        retyped = self.kind.map(
            lambda kind: (
                InterfaceKeys
                .of(keyed.interface.tree, identity)
                .item(self.key, bpy.types.NodeTreeInterfaceItem)
                .bind(lambda held: written(held, ("socket_type",), rna_definition(base_socket(kind)).identifier))
            )
        )
        return retyped.default_value(Ok(None)).bind(lambda _: self.place.map(lambda place: moved(InterfaceKeys.of(keyed.interface.tree, identity), *place)).default_value(Ok(None)))


class Restated(msgspec.Struct, frozen=True, kw_only=True):
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Declared", "Placed", "Restated", "Withdrawn"]
