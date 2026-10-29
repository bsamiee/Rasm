"""Desired node tree contents: the content protocol nodes hold, link ends, nodes, frames, graphs, and owned groups."""

from collections.abc import Iterable, Iterator, Mapping
import functools
import itertools
from types import MappingProxyType
from typing import Final, Protocol

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.interface import Interface
from blender.bpy.nodes.trees import GraphTree
from blender.records.faults import accumulated, Fault, Outcome

# --- [TYPES] ----------------------------------------------------------------------------

type Members = Mapping[str | RnaPath, Value]


class Content(Protocol):
    """Node content a node's own members cannot state, such as ramp stops, curve points, images, groups, menu items, and File Output items, writing and reading itself."""

    @property
    def keyed(self) -> object:
        """MessagePack-encodable form the lowering digest reads."""

    @property
    def nested(self) -> "tuple[NodeGroup, ...]":
        """Owned groups the content points at, at any depth."""

    def holds(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[bool]:
        """Whether the live node holds this content."""

    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        """Writes this content onto the live node."""

    def sockets(self, live: bpy.types.Node, identity: Identity, slot: str, /) -> Mapping[str, str]:
        """Socket identifiers by the item key or item name the content addresses them through."""


# --- [CONSTANTS] ------------------------------------------------------------------------

INPUTS: Final[Key] = "inputs"
OUTPUTS: Final[Key] = "outputs"
RESULT: Final[Key] = "result"

# --- [MODELS] ---------------------------------------------------------------------------


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
    def nested(self) -> "tuple[NodeGroup, ...]":
        """Groups nested at any depth, one per key."""
        return tuple({group.key: group for node, _ in self.placed.values() for content in node.content.to_list() for group in content.nested}.values())

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
            Ok((key, first)) if not rest else Error(Fault.of(BlenderReason.REFUSED, f"Graph.nodes[{key!r}]", ValueError(f"{len(rest) + 1} different nodes share key {key!r}")))
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["INPUTS", "OUTPUTS", "RESULT", "Content", "End", "FrameNode", "Graph", "Link", "Members", "Node", "NodeGroup", "member_paths"]
