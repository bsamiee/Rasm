"""Desired group interfaces and Menu Switch items: sockets, panels, item keys, and the menu content a Menu Switch node holds."""

from collections.abc import Iterator, Mapping
from types import MemberDescriptorType
from typing import ClassVar, Literal, override, Self

import bpy
from expression import Nothing, Option, Some, try_downcast
import msgspec
import msgspec.structs
import numpy as np

from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.trees import arranged, base_socket
from blender.records.faults import accumulated, Fault, Outcome

# --- [TYPES] ----------------------------------------------------------------------------

type SocketSide = Literal["INPUT", "OUTPUT"]
type DesiredItem = tuple[Socket, SocketSide] | InterfacePanel

# --- [MODELS] ---------------------------------------------------------------------------

# --- [ITEMS]


class MenuItem(msgspec.Struct, frozen=True, kw_only=True):
    """Menu Switch item."""

    name: str
    description: str = ""


class MenuItems(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered Menu Switch items, the content a Menu Switch node holds."""

    items: tuple[MenuItem, ...]

    nested: ClassVar[tuple[()]] = ()

    @classmethod
    def of(cls, prop: bpy.types.EnumProperty) -> Self:
        """Builds items from the RNA enum the menu mirrors, named and described by its items."""
        return cls(items=tuple(MenuItem(name=item.name, description=item.description) for item in prop.enum_items))

    @property
    def keyed(self) -> object:
        """Builtin form the lowering digest reads."""
        return msgspec.to_builtins(self)

    def holds(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[bool]:
        """Whether the Menu Switch holds these items in order with these descriptions."""
        return _switch(live).map(lambda switch: tuple((each.name, each.description) for each in switch.enum_items) == tuple((each.name, each.description) for each in self.items))

    def write(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[None]:
        """Arranges the Menu Switch items by name, then writes each description."""
        return _switch(live).bind(
            lambda switch: accumulated(
                written(each, ("description",), item.description)
                for each, item in zip(arranged(switch.enum_items, tuple(each.name for each in self.items), switch.enum_items.new, switch.enum_items.remove), self.items)
            ).map(lambda _: None)
        )

    @staticmethod
    def sockets(live: bpy.types.Node, _identity: Identity, _slot: str, /) -> Mapping[str, str]:
        """Item socket identifiers by item name, the outputs past `Output` in item order."""
        fixed = {socket.identifier for socket in (*live.inputs, *live.outputs)}
        return _switch(live).map(lambda switch: {item.name: socket.identifier for item, socket in zip(switch.enum_items, tuple(live.outputs)[1:]) if item.name not in fixed}).default_value({})


# --- [INTERFACE]


class Socket(msgspec.Struct, frozen=True, kw_only=True):
    """Interface socket by parameter key, its class a refined socket class whose default value states subtype, dimensions, and bounds, group input menu items kept only while linked to a Menu Switch."""

    key: Key
    kind: type[bpy.types.NodeSocket]
    name: str | None = None
    description: str = ""
    min_value: float | None = None
    max_value: float | None = None
    default_value: Value = None
    hide_in_modifier: bool | None = None
    items: Option[MenuItems] = Nothing

    @property
    def title(self) -> str:
        """Stated name, else the key's display name."""
        return bpy.path.display_name(self.key) if self.name is None else self.name

    @property
    def base(self) -> type[bpy.types.NodeSocket]:
        """Base socket class an interface item takes as its socket type."""
        return base_socket(self.kind)

    @property
    def members(self) -> dict[RnaPath, Value]:
        """Interface members by path in write order: subtype and dimensions of a refined class, then bounds, the class's own finite bounds where none is stated, then the default."""
        properties = self.kind.bl_rna.properties
        match properties["default_value"] if self.kind is not self.base and "default_value" in properties else None:
            case bpy.types.FloatProperty() | bpy.types.IntProperty() as prop:
                finite = abs(prop.hard_max) < float(np.finfo(np.float32).max)
                subtype = None if prop.subtype == "NONE" else prop.subtype
                dimensions = prop.array_length if self.base is bpy.types.NodeSocketVector else None
                low, high = (prop.hard_min, prop.hard_max) if finite else (None, None)
            case _:
                subtype, dimensions, low, high = None, None, None, None
        return {
            path: value
            for path, value in (
                (("subtype",), subtype),
                (("dimensions",), dimensions),
                (("min_value",), low if self.min_value is None else self.min_value),
                (("max_value",), high if self.max_value is None else self.max_value),
                (("default_value",), self.default_value),
                (("hide_in_modifier",), self.hide_in_modifier),
            )
            if value is not None
        }


class Interface(msgspec.Struct, frozen=True, kw_only=True):
    """Group interface items in Blender order."""

    outputs: tuple[Socket, ...] = ()
    inputs: tuple[Socket, ...] = ()
    panels: tuple["InterfacePanel", ...] = ()

    @property
    def first(self) -> int:
        """Position of the first item, after nothing at the root."""
        return 0

    def walked(self, parent: Option[Key] = Nothing) -> Iterator[tuple[Key, DesiredItem, Option[Key], int]]:
        """Walks interface items in preorder.

        Args:
            parent: Key of panel holding items, `Nothing` at root.

        Yields:
            Item key, socket with side or panel, parent key, and position after the toggle socket a toggled panel holds first.
        """
        items: tuple[DesiredItem, ...] = (*((socket, "OUTPUT") for socket in self.outputs), *((socket, "INPUT") for socket in self.inputs), *self.panels)
        for position, item in enumerate(items, self.first):
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

    @property
    @override
    def first(self) -> int:
        """Position of the first item, after the toggle a toggled panel holds."""
        return int(self.toggle.is_some())

    @property
    def item(self) -> MenuItem:
        """Menu item selecting the panel, under its name and description."""
        return MenuItem(name=self.name, description=self.description)


# --- [OPERATIONS] -----------------------------------------------------------------------


def item_key(*parts: object) -> Key:
    """Joins key path into item key, record fields by their member descriptor's name, case classes as lowercase names, and keys and indexes as their text, parts typed over `object` as checkers read a field on its struct class as the field's value."""

    def part(value: object) -> str:
        match value:
            case MemberDescriptorType():
                return value.__name__
            case type():
                return value.__name__.lower()
            case _:
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


def _switch(live: bpy.types.Node) -> Outcome[bpy.types.GeometryNodeMenuSwitch]:
    """Node as Menu Switch, else `absent` fault naming its items."""
    return Option.of_optional(try_downcast(bpy.types.GeometryNodeMenuSwitch, live)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(live), member="enum_items"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DesiredItem", "Interface", "InterfacePanel", "MenuItem", "MenuItems", "Socket", "SocketSide", "item_key", "under"]
