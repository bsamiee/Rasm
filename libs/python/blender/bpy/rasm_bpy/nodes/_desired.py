"""Desired node tree state: node content, menu items, link ends, links, nodes, frames, and graphs."""

from collections.abc import Iterable, Iterator, Mapping
from enum import Enum
from functools import partial
import inspect
from itertools import chain, islice
from types import MappingProxyType, MemberDescriptorType
from typing import ClassVar, Final, get_args, override, Self, TypeAliasType

import bpy
from expression import Option, Result, Some, try_downcast
import msgspec
import msgspec.structs

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._rna.descriptors import _differing, Value, written
from rasm_bpy.data._rna.paths import RnaPath
from rasm_bpy.data._rna.properties import _titled
from rasm_bpy.nodes._trees import _matched_items, _ordered_items, GraphTree
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, Reason, words

# --- [TYPES] ----------------------------------------------------------------------------

type Members = Mapping[str | RnaPath, Value]
type Choice = Enum | type[msgspec.Struct]

# --- [CONSTANTS] ------------------------------------------------------------------------

INPUTS: Final[Key] = "inputs"
_OUTPUTS: Final[Key] = "outputs"
RESULT: Final[Key] = "result"

# --- [MODELS] ---------------------------------------------------------------------------

# --- [CONTENT]


class Content(msgspec.Struct, frozen=True, kw_only=True):
    """Node content a node's own members cannot state (ramp stops, curve points, images, groups, menu items, File Output items), each case writing and reading itself."""

    @property
    def digest_form(self) -> object:
        """Builtin form the lowering digest reads."""
        return msgspec.to_builtins(self, enc_hook=str)

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

    def positions(self, live: bpy.types.Node, identity: Identity, /) -> Mapping[bool, Mapping[int, str]]:
        """Native item identifiers the write retains, by enabled position per output side.

        Raises:
            NotImplementedError: For a case whose sockets follow a group interface in place of native items.
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
                return cls(name=_titled(choice), description=Option.of_optional(choice.__doc__).map(inspect.cleandoc).default_value(""))

    @staticmethod
    def member[C: Choice](choices: Iterable[C], name: str, /) -> Option[C]:
        """Returns the enum member or case class whose item `of` names `name`, the inverse of `of` over an enum class or a union alias's `get_args(alias.__value__)`."""
        return Option.of_optional(next((choice for choice in choices if MenuItem.of(choice).name == name), None))


class MenuItems(ItemContent, frozen=True, kw_only=True):
    """Ordered Menu Switch items, the content a Menu Switch node holds, an absent selection keeping a native one that names an item and taking the first item otherwise."""

    items: tuple[MenuItem, ...]
    selected: MenuItem | None = None
    _key: ClassVar[Key] = "menu_items"

    @staticmethod
    def _bound(live: bpy.types.GeometryNodeMenuSwitch, identity: Identity) -> tuple[Mapping[str, Mapping[str, str]], dict[str, bpy.types.NodeEnumItem], bpy.types.NodeInputs, bpy.types.NodeOutputs]:
        bindings: Mapping[str, Mapping[str, str]] = live.get(identity.identifier(MenuItems._key), {})  # ty: ignore[unsound-assignment]
        inputs: bpy.types.NodeInputs = live.inputs  # ty: ignore[invalid-assignment]
        outputs: bpy.types.NodeOutputs = live.outputs  # ty: ignore[invalid-assignment]
        return bindings, {socket.identifier: item for item, socket in zip(live.enum_items, islice(outputs, 1, None))}, inputs, outputs

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
        def matched(switch: bpy.types.GeometryNodeMenuSwitch) -> bool:
            bindings, inventory, inputs, _ = self._bound(switch, identity)
            selection = None if self.selected is None else next((native.name for native, wanted in zip(inventory.values(), self.items) if wanted == self.selected), self.selected.name)
            return (
                len(inventory) == len(self.items)
                and tuple(bindings[identifier]["request"] for identifier in inventory if identifier in bindings) == tuple(item.name for item in self.items)
                and all(native.name == bindings[identifier]["name"] and native.description == wanted.description for (identifier, native), wanted in zip(inventory.items(), self.items))
                and (selection is None or not _differing(inputs["Menu"], {("default_value",): selection}))
            )

        return _switch(live).map(matched)

    @override
    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        match _switch(live):
            case Result(tag="ok", ok=switch):
                pass
            case Result(error=errors):
                return Error(errors)
        bindings, inventory, inputs, outputs = self._bound(switch, identity)
        removed, matched = _matched_items(inventory, {identifier: row["request"] for identifier, row in bindings.items()}, (item.name for item in self.items))
        names = {identifier: item.name for identifier, item in inventory.items()}
        selected = next((index for index, item in enumerate(self.items) if item == self.selected), None)
        selection = None if self.selected is None else self.selected.name
        kept: dict[str, dict[str, str]] = {}
        desired: list[str] = []
        answers: list[Outcome[object]] = []
        refused = partial(Fault.of, Refusal.REFUSED, repr(switch))
        try:
            for index in reversed(removed):
                switch.enum_items.remove(switch.enum_items[index])
            for index, (wanted, retained) in enumerate(zip(self.items, matched)):
                item = switch.enum_items[names[retained]] if retained is not None else switch.enum_items.new(wanted.name)
                if retained is not None and item.name != bindings[retained]["name"]:
                    item.name = wanted.name
                identifier = retained if retained is not None else outputs[len(switch.enum_items)].identifier
                kept[identifier] = {"request": wanted.name, "name": item.name}
                selection = item.name if index == selected else selection
                desired.append(identifier)
                answers.append(written(item, ("description",), wanted.description))
            current = (*(identifier for identifier in inventory if identifier in kept), *(identifier for identifier, retained in zip(desired, matched) if retained is None))
            _ordered_items(current, desired, switch.enum_items.move)
            if selection is None and all(_differing(inputs["Menu"], {("default_value",): item.name}) for item in switch.enum_items):
                selection = next((item.name for item in switch.enum_items), None)
            if selection is not None:
                answers.append(written(inputs["Menu"], ("default_value",), selection))
        except (RuntimeError, TypeError, ValueError) as error:
            return accumulated((*answers, Error(refused(error)))).map(lambda _: None)
        try:
            switch[identity.identifier(self._key)] = kept
        except (KeyError, TypeError, ValueError) as error:
            answers.append(Error(refused(error)))
        return accumulated(answers).map(lambda _: None)

    @override
    def sockets(self, live: bpy.types.Node, identity: Identity, /) -> Mapping[str, str]:
        match live:
            case bpy.types.GeometryNodeMenuSwitch() as switch:
                bindings, inventory, inputs, outputs = self._bound(switch, identity)
            case _:
                return dict[str, str]()
        fixed = {socket.identifier for socket in chain(inputs, outputs)}
        aliases: dict[str, str] = {}
        for name, identifier in chain(
            ((bindings[identifier]["request"], identifier) for identifier in inventory if identifier in bindings), ((item.name, identifier) for identifier, item in inventory.items())
        ):
            if name not in fixed:
                aliases.setdefault(name, identifier)
        return aliases

    @override
    def positions(self, live: bpy.types.Node, identity: Identity, /) -> Mapping[bool, Mapping[int, str]]:
        match live:
            case bpy.types.GeometryNodeMenuSwitch() as switch:
                bindings, inventory, inputs, outputs = self._bound(switch, identity)
            case _:
                return {False: dict[int, str](), True: dict[int, str]()}
        _, matched = _matched_items(inventory, {identifier: row["request"] for identifier, row in bindings.items()}, (item.name for item in self.items))
        return {
            output: {
                index: identifier
                for index, identifier in enumerate((
                    sockets[0].identifier,
                    *matched,
                    *(socket.identifier for socket in islice(sockets, 1, None) if socket.identifier not in inventory and socket.enabled),
                ))
                if identifier is not None
            }
            for output, sockets in ((False, inputs), (True, outputs))
        }


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
        msgspec.structs.force_setattr(self, "members", _member_paths(self.members))

    @property
    def paths(self) -> Mapping[RnaPath, Value]:
        """Members by RNA path."""
        return _member_paths(self.members)


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
        placed: dict[Key, tuple[Node, Option[Key]]] = {}
        pending: list[tuple[Iterator[tuple[Key, Node | FrameNode]], Option[Key]]] = [(iter(self.nodes.items()), Nothing)]
        while pending:
            items, parent = pending[-1]
            match next(items, None):
                case (key, FrameNode(members=members, nodes=inner)):
                    placed[key] = (Node(kind=bpy.types.NodeFrame, members=members), parent)
                    pending.append((iter(inner.items()), Some(key)))
                case (key, Node() as node):
                    placed[key] = (node, parent)
                case None:
                    pending.pop()
        return placed

    @property
    def contents(self) -> dict[Key, Content]:
        """Content each placed node holds, by key."""
        return {key: content for key, (node, _) in self.placed.items() for content in node.content.to_list()}

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
        claims: dict[Key, list[Node | FrameNode]] = {}
        for key, item in ((key, item) for part in held for key, item in part.nodes.items()):
            distinct = claims.setdefault(key, [])
            if item not in distinct:
                distinct.append(item)
        return accumulated(
            Ok((key, first)) if not rest else Error(Fault.of(Refusal.REFUSED, f"Graph.nodes[{key!r}]", ValueError(f"{len(rest) + 1} different nodes share key {key!r}")))
            for key, (first, *rest) in claims.items()
        ).map(
            lambda nodes: cls(
                kind=kind, members={path: value for part in held for path, value in part.members.items()}, nodes=dict(nodes), links=tuple(dict.fromkeys(link for part in held for link in part.links))
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _member_paths(members: Members) -> Mapping[RnaPath, Value]:
    """Returns members by RNA path, an identifier as its one-step path."""
    return MappingProxyType({(key,) if isinstance(key, str) else key: value for key, value in members.items()})


def item_key(*parts: object) -> Key:
    """Joins key path into item key: field descriptors by name, case classes in snake case, other parts as text."""

    def part(value: object) -> str:
        match value:
            case MemberDescriptorType():
                return value.__name__
            case type():
                return "_".join(word.lower() for word in words(value.__name__))
            case _:
                return str(value)

    return "_".join(map(part, parts))


def _switch(live: bpy.types.Node) -> Outcome[bpy.types.GeometryNodeMenuSwitch]:
    """Node as Menu Switch, else `absent` fault naming its items."""
    return Option.of_optional(try_downcast(bpy.types.GeometryNodeMenuSwitch, live)).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(live), member="enum_items"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["INPUTS", "RESULT", "Choice", "Content", "End", "FrameNode", "Graph", "ItemContent", "Link", "Membered", "Members", "MenuItem", "MenuItems", "Node", "item_key"]
