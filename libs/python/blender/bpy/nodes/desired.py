"""Desired node tree state: node content, menu items, group interfaces, link ends, links, nodes, frames, graphs, and owned groups."""

from collections.abc import Iterable, Iterator, Mapping
from enum import Enum
import functools
import itertools
from types import MappingProxyType, MemberDescriptorType
from typing import Final, Literal, override, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
import msgspec
import msgspec.structs
import numpy as np

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.trees import arranged, base_socket, GraphTree
from blender.records.faults import accumulated, Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Members = Mapping[str | RnaPath, Value]
type DesiredItem = tuple[Socket, Literal["INPUT", "OUTPUT"]] | InterfacePanel

# --- [CONSTANTS] ------------------------------------------------------------------------

INPUTS: Final[Key] = "inputs"
OUTPUTS: Final[Key] = "outputs"
RESULT: Final[Key] = "result"

# --- [MODELS] ---------------------------------------------------------------------------

# --- [CONTENT]


class Content(msgspec.Struct, frozen=True, kw_only=True):
    """Node content a node's own members cannot state, such as ramp stops, curve points, images, groups, menu items, and File Output items, each case writing and reading itself."""

    @property
    def keyed(self) -> object:
        """Builtin form the lowering digest reads."""
        return msgspec.to_builtins(self, enc_hook=str)

    @property
    def nested(self) -> "tuple[NodeGroup, ...]":
        """Owned groups the content points at, at any depth."""
        return ()

    def holds(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[bool]:
        """Whether the live node holds this content.

        Raises:
            NotImplementedError: For a case declaring no read.
        """
        raise NotImplementedError(type(self).__name__)

    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        """Writes this content onto the live node.

        Raises:
            NotImplementedError: For a case declaring no write.
        """
        raise NotImplementedError(type(self).__name__)


class Addressing(Content, frozen=True, kw_only=True):
    """Node content whose items add sockets that links and values address by item key or item name."""

    def sockets(self, live: bpy.types.Node, identity: Identity, /) -> Mapping[str, str]:
        """Socket identifiers by the item key or item name the content addresses them through.

        Raises:
            NotImplementedError: For a case declaring no addressing.
        """
        raise NotImplementedError(type(self).__name__)


class MenuItem(msgspec.Struct, frozen=True, kw_only=True):
    """Menu Switch item."""

    name: str
    description: str = ""

    @classmethod
    def of(cls, member: Enum, /) -> Self:
        """Builds the item an enum member selects, named in words."""
        return cls(name=bpy.path.display_name(member.name.lower()))


class MenuItems(Addressing, frozen=True, kw_only=True):
    """Ordered Menu Switch items, the content a Menu Switch node holds."""

    items: tuple[MenuItem, ...]

    @classmethod
    def of(cls, source: bpy.types.EnumProperty | type[Enum], /) -> Self:
        """Builds items from the RNA enum the menu mirrors, named and described by its items, or from every member of an enum class."""
        match source:
            case bpy.types.EnumProperty(enum_items=items):
                return cls(items=tuple(MenuItem(name=item.name, description=item.description) for item in items))
            case _:
                return cls(items=tuple(map(MenuItem.of, source)))

    @override
    def holds(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[bool]:
        return _switch(live).map(lambda switch: tuple((each.name, each.description) for each in switch.enum_items) == tuple((each.name, each.description) for each in self.items))

    @override
    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        return _switch(live).bind(
            lambda switch: accumulated(
                written(each, ("description",), item.description)
                for each, item in zip(arranged(switch.enum_items, tuple(each.name for each in self.items), switch.enum_items.new, switch.enum_items.remove), self.items)
            ).map(lambda _: None)
        )

    @override
    def sockets(self, live: bpy.types.Node, identity: Identity, /) -> Mapping[str, str]:
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


# --- [GRAPHS]


class End(msgspec.Struct, frozen=True, kw_only=True):
    """Link end at node key and socket address: identifier, item key, item name, or position among the enabled sockets once node members are written."""

    node: Key
    socket: str | int = "Value"


class Link(msgspec.Struct, frozen=True, kw_only=True):
    """Declared link between two ends, equal links one link."""

    source: End
    target: End


class Node(msgspec.Struct, frozen=True, kw_only=True):
    """Desired node with members by path, an identifier naming a one-step path, and socket values by address."""

    kind: type[bpy.types.Node]
    members: Members = MappingProxyType({})
    inputs: Mapping[str | int, Value] = MappingProxyType({})
    outputs: Mapping[str | int, Value] = MappingProxyType({})
    content: Option[Content] = Nothing

    def __post_init__(self) -> None:
        """Holds members under their paths."""
        msgspec.structs.force_setattr(self, "members", member_paths(self.members))


class FrameNode(msgspec.Struct, frozen=True, kw_only=True):
    """Frame node members and framed nodes."""

    members: Members = MappingProxyType({})
    nodes: Mapping[Key, "Node | FrameNode"] = MappingProxyType({})

    def __post_init__(self) -> None:
        """Holds members under their paths."""
        msgspec.structs.force_setattr(self, "members", member_paths(self.members))


class Graph(msgspec.Struct, frozen=True, kw_only=True):
    """Desired contents of one node tree, or of a part of one that joins others."""

    kind: type[GraphTree]
    members: Members = MappingProxyType({})
    nodes: Mapping[Key, Node | FrameNode] = MappingProxyType({})
    links: tuple[Link, ...] = ()

    def __post_init__(self) -> None:
        """Holds members under their paths."""
        msgspec.structs.force_setattr(self, "members", member_paths(self.members))

    @property
    def placed(self) -> dict[Key, tuple[Node, Option[Key]]]:
        """Node and parent frame key per key in preorder, frames as `NodeFrame` nodes."""

        def walked(items: Mapping[Key, Node | FrameNode], parent: Option[Key]) -> Iterator[tuple[Key, tuple[Node, Option[Key]]]]:
            for key, item in items.items():
                match item:
                    case FrameNode(members=members, nodes=inner):
                        yield key, (Node(kind=bpy.types.NodeFrame, members=members), parent)
                        yield from walked(inner, Some(key))
                    case Node():
                        yield key, (item, parent)

        return dict(walked(self.nodes, Nothing))

    @property
    def contents(self) -> dict[Key, Content]:
        """Content each placed node holds, by key."""
        return {key: content for key, (node, _) in self.placed.items() for content in node.content.to_list()}

    @property
    def nested(self) -> "tuple[NodeGroup, ...]":
        """Groups nested at any depth, one per key."""
        return tuple({group.key: group for content in self.contents.values() for group in content.nested}.values())

    @classmethod
    def merged(cls, kind: type[GraphTree], parts: Iterable["Graph"]) -> Outcome["Graph"]:
        """Joins graph parts of one tree class, keeping one node per key and each link once.

        Args:
            kind: Tree class of the joined graph.
            parts: Parts to join, earlier keys first.

        Returns:
            `Ok` of joined graph. `Error` of `edit` per key holding different nodes.
        """
        held = tuple(parts)
        pairs = tuple((key, item) for part in held for key, item in part.nodes.items())
        first = {key: index for index, key in enumerate(dict.fromkeys(key for key, _ in pairs))}
        claims: dict[Key, tuple[Node | FrameNode, ...]] = {
            key: functools.reduce(lambda distinct, item: distinct if item in distinct else (*distinct, item), (item for _, item in run), ())
            for key, run in itertools.groupby(sorted(pairs, key=lambda pair: first[pair[0]]), key=lambda pair: pair[0])
        }
        return accumulated(
            Ok((key, first)) if not rest else Error(Fault.of(Reason.REFUSED, f"Graph.nodes[{key!r}]", ValueError(f"{len(rest) + 1} different nodes share key {key!r}")))
            for key, (first, *rest) in claims.items()
        ).map(
            lambda nodes: cls(
                kind=kind, members={path: value for part in held for path, value in part.members.items()}, nodes=dict(nodes), links=tuple(dict.fromkeys(link for part in held for link in part.links))
            )
        )

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns nested group ownership rows in nesting order."""
        return Ok(tuple(group.ownership for group in self.nested))


class NodeGroup(msgspec.Struct, frozen=True, kw_only=True):
    """Owned node group of a graph, interface, and saved state."""

    key: Key
    name: str
    graph: Graph
    interface: Interface = Interface()
    saved: Saved = Saved.USED
    single: bool = True

    @property
    def ownership(self) -> Ownership[bpy.types.NodeTree]:
        """Group tree ownership row."""
        return Ownership(kind=self.graph.kind, key=self.key, saved=self.saved, single=self.single)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns group ownership row, then every nested group's row."""
        return Ok((self.ownership, *(group.ownership for group in self.graph.nested)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def member_paths(members: Members) -> Mapping[RnaPath, Value]:
    """Returns members by RNA path, an identifier as its one-step path."""
    return MappingProxyType({(key,) if isinstance(key, str) else key: value for key, value in members.items()})


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
    return Option.of_optional(try_downcast(bpy.types.GeometryNodeMenuSwitch, live)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(live), member="enum_items"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "INPUTS",
    "OUTPUTS",
    "RESULT",
    "Addressing",
    "Content",
    "DesiredItem",
    "End",
    "FrameNode",
    "Graph",
    "Interface",
    "InterfacePanel",
    "Link",
    "Members",
    "MenuItem",
    "MenuItems",
    "Node",
    "NodeGroup",
    "Socket",
    "item_key",
    "member_paths",
    "under",
]
