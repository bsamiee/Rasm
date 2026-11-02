"""Desired node tree state: node content, menu items, link ends, links, nodes, frames, and graphs."""

from collections.abc import Iterable, Iterator, Mapping
from enum import Enum
import functools
import inspect
import itertools
import re
from types import MappingProxyType, MemberDescriptorType
from typing import Final, get_args, override, Self, TypeAliasType

import bpy
from expression import Error, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec
import msgspec.structs

from blender.bpy.data.ids.ownership import Ownership
from blender.bpy.data.rna.descriptors import Value, written
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.properties import titled
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.trees import arranged, GraphTree
from blender.records.faults import accumulated, Fault, Outcome, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Members = Mapping[str | RnaPath, Value]
type Choice = Enum | type[msgspec.Struct]

# --- [CONSTANTS] ------------------------------------------------------------------------

INPUTS: Final[Key] = "inputs"
OUTPUTS: Final[Key] = "outputs"
RESULT: Final[Key] = "result"

# --- [MODELS] ---------------------------------------------------------------------------

# --- [CONTENT]


class Content(msgspec.Struct, frozen=True, kw_only=True):
    """Node content a node's own members cannot state (ramp stops, curve points, images, groups, menu items, File Output items), each case writing and reading itself."""

    @property
    def digest_form(self) -> object:
        """Builtin form the lowering digest reads."""
        return msgspec.to_builtins(self, enc_hook=str)

    @property
    def groups(self) -> tuple[Ownership[bpy.types.NodeTree], ...]:
        """Ownership rows of the owned groups the content points at, at any depth."""
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


class ItemContent(Content, frozen=True, kw_only=True):
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
    def of(cls, choice: Choice, /) -> Self:
        """Builds the item an enum member or union case class selects, a member named by its name in words, a case by its class name in `titled` words and described by its own docstring."""
        match choice:
            case Enum(name=name):
                return cls(name=bpy.path.display_name(name.lower()))
            case _:
                return cls(name=titled(choice), description=Option.of_optional(choice.__doc__).map(inspect.cleandoc).default_value(""))

    @staticmethod
    def member[C: Choice](choices: Iterable[C], name: str, /) -> Option[C]:
        """Returns the enum member or case class whose item `of` names `name`, the inverse of `of` over an enum class or a union alias's `get_args(alias.__value__)`."""
        return Block.of_seq(choices).filter(lambda choice: MenuItem.of(choice).name == name).try_head()


class MenuItems(ItemContent, frozen=True, kw_only=True):
    """Ordered Menu Switch items, the content a Menu Switch node holds."""

    items: tuple[MenuItem, ...]

    @classmethod
    def of(cls, source: bpy.types.EnumProperty | Iterable[Choice] | TypeAliasType, /) -> Self:
        """Builds items from the RNA enum the menu matches, from enum members or case classes, or from a union alias's cases, each item named and described as `MenuItem.of` states it."""
        match source:
            case bpy.types.EnumProperty(enum_items=items):
                return cls(items=tuple(MenuItem(name=item.name, description=item.description) for item in items))
            case TypeAliasType():
                return cls.of(get_args(source.__value__))
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


# --- [GRAPHS]


class End(msgspec.Struct, frozen=True, kw_only=True):
    """Link end at node key and socket address: identifier, item key, item name, or position among the enabled sockets once node members are written."""

    node: Key
    socket: str | int = "Value"


class Link(msgspec.Struct, frozen=True, kw_only=True):
    """Declared link between two ends, equal links one link."""

    source: End
    target: End


class Membered(msgspec.Struct, frozen=True, kw_only=True):
    """Record holding RNA members under their paths, an identifier given as its one-step path."""

    members: Members = MappingProxyType({})

    def __post_init__(self) -> None:
        """Holds members under their paths."""
        msgspec.structs.force_setattr(self, "members", member_paths(self.members))

    @property
    def paths(self) -> Mapping[RnaPath, Value]:
        """Members by RNA path."""
        return member_paths(self.members)


class Node(Membered, frozen=True, kw_only=True):
    """Desired node with members by path and socket values by address."""

    kind: type[bpy.types.Node]
    inputs: Mapping[str | int, Value] = MappingProxyType({})
    outputs: Mapping[str | int, Value] = MappingProxyType({})
    content: Option[Content] = Nothing


class FrameNode(Membered, frozen=True, kw_only=True):
    """Frame node members and framed nodes."""

    nodes: Mapping[Key, "Node | FrameNode"] = MappingProxyType({})


class Graph(Membered, frozen=True, kw_only=True):
    """Desired contents of one node tree, or of a part of one that joins others."""

    kind: type[GraphTree]
    nodes: Mapping[Key, Node | FrameNode] = MappingProxyType({})
    links: tuple[Link, ...] = ()

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
    def groups(self) -> tuple[Ownership[bpy.types.NodeTree], ...]:
        """Ownership rows of the groups nested at any depth, one per key in nesting order."""
        return tuple({group.key: group for content in self.contents.values() for group in content.groups}.values())

    @classmethod
    def merged(cls, kind: type[GraphTree], parts: Iterable["Graph"]) -> Outcome["Graph"]:
        """Joins graph parts of one tree class, keeping one node per key and each link once.

        Args:
            kind: Tree class of the joined graph.
            parts: Parts to join, earlier keys first.

        Returns:
            `Ok` of joined graph. `Error` of `refused` per key holding different nodes.
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
        return Ok(self.groups)


# --- [OPERATIONS] -----------------------------------------------------------------------


def member_paths(members: Members) -> Mapping[RnaPath, Value]:
    """Returns members by RNA path, an identifier as its one-step path."""
    return MappingProxyType({(key,) if isinstance(key, str) else key: value for key, value in members.items()})


def item_key(*parts: object) -> Key:
    """Joins key path into item key: field descriptors by name, case classes in snake case, other parts as text."""

    def part(value: object) -> str:
        match value:
            case MemberDescriptorType():
                return value.__name__
            case type():
                return re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", value.__name__).lower()
            case _:
                return str(value)

    return "_".join(map(part, parts))


def _switch(live: bpy.types.Node) -> Outcome[bpy.types.GeometryNodeMenuSwitch]:
    """Node as Menu Switch, else `absent` fault naming its items."""
    return Option.of_optional(try_downcast(bpy.types.GeometryNodeMenuSwitch, live)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(live), member="enum_items"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["INPUTS", "OUTPUTS", "RESULT", "Choice", "Content", "End", "FrameNode", "Graph", "ItemContent", "Link", "Membered", "Members", "MenuItem", "MenuItems", "Node", "item_key", "member_paths"]
