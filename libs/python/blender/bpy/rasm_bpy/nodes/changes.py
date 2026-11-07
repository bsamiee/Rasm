"""Desired graph reconciliation onto node trees: owned groups, node change rows that write themselves, and write phases over them and the interface rows."""

from collections import deque
from collections.abc import Mapping
from functools import partial, reduce
from types import MappingProxyType
from typing import override

import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._ids.ownership import Mark, Ownership, Saved
from rasm_bpy.data._rna.descriptors import _differing, Value, written
from rasm_bpy.data._rna.paths import RnaPath
from rasm_bpy.data._rna.snapshots import assigned
from rasm_bpy.nodes._addressing import InterfaceKeys, OwnedNodes
from rasm_bpy.nodes._desired import Content, End, Graph, ItemContent, Link, Node
from rasm_bpy.nodes._interface_changes import _Declared, _interfaced, _Placed, _Restated, _Withdrawn
from rasm_bpy.nodes._interfaces import Interface
from rasm_bpy.nodes._trees import _created, _endpoints, _group_set, _linked, _links_of, _new_group, _ordered, _socket_at, group_tree
from rasm_records import accumulated, Fault, Faults, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Row = Configured | _Withdrawn | _Declared | Removed | Added | _Placed | Altered | Connected | Reordered | Valued | _Restated
type Changes = tuple[tuple[_Row, ...], ...]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [GROUPS]


class NodeGroup(ItemContent, frozen=True, kw_only=True):
    """Owned node group of a graph, interface, and saved state, and the content of each group node pointing at it as a hidden tree of its own."""

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

    @property
    @override
    def digest_form(self) -> object:
        """Group key and name."""
        return (self.key, self.name)

    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Group ownership row, then the rows of every group its nodes point at in nesting order, each row once."""
        return tuple(dict.fromkeys((self.ownership, *(row for group in self.graph.contents.values() if isinstance(group, NodeGroup) for row in group.acquisitions(identity)))))

    @override
    def holds(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[bool]:
        return (
            group_tree(live)
            .filter(lambda tree: Mark.read(identity, tree) == Some(Mark(key=self.key)))
            .map(lambda tree: changes(self, tree, identity).map(lambda rows: not any(rows)))
            .default_value(Ok(value=False))
        )

    @override
    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        return made(msgspec.structs.replace(self, name=f".{self.name}"), identity).bind(lambda tree: _group_set(live, tree))

    @override
    def sockets(self, live: bpy.types.Node, identity: Identity, /) -> Mapping[str, str]:
        return group_tree(live).map(lambda tree: InterfaceKeys.of(tree, identity).retained(self.interface)).default_value(dict[str, str]())


# --- [NODE_ROWS]


class Configured(msgspec.Struct, frozen=True, kw_only=True):
    """Graph members tree lacks."""

    members: Mapping[RnaPath, Value]

    def applied(self, keyed: OwnedNodes, _identity: Identity) -> Outcome[None]:
        """Writes the tree members."""
        return assigned(keyed.interface.tree, self.members)


class Removed(msgspec.Struct, frozen=True, kw_only=True):
    """Owned link or node graph lacks or declares as another class."""

    item: bpy.types.Node | bpy.types.NodeLink

    def applied(self, keyed: OwnedNodes, _identity: Identity) -> Outcome[None]:
        """Removes the link or node, the node with its links."""
        match self.item:
            case bpy.types.NodeLink() as link:
                answer = catch(keyed.interface.tree.links.remove, exception=RuntimeError)(link)
            case node:
                answer = catch(keyed.interface.tree.nodes.remove, exception=RuntimeError)(node)
        return answer.map_error(partial(Fault.of, Reason.REFUSED, repr(keyed.interface.tree)))


class Added(msgspec.Struct, frozen=True, kw_only=True):
    """Node to create for a key."""

    key: Key
    kind: type[bpy.types.Node]

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Creates the node and marks it with its key."""
        return _created(keyed.interface.tree, self.kind).bind(lambda made: Mark(key=self.key).write(identity, made))


class Altered(msgspec.Struct, frozen=True, kw_only=True):
    """Node parts live node lacks."""

    key: Key
    members: Mapping[RnaPath, Value] = MappingProxyType({})
    content: Option[Content] = Nothing
    parent: Option[Option[Key]] = Nothing

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Writes content, then members, then the parent frame."""

        def framed(live: bpy.types.Node, frame: Option[Key]) -> Outcome[None]:
            return frame.map(lambda key: keyed.node(key).map(Some)).default_value(Ok(Nothing)).bind(lambda found: written(live, ("parent",), found.to_optional())).map(lambda _: None)

        return keyed.node(self.key).bind(
            lambda live: (
                self.content
                .map(lambda content: content.write(live, identity))
                .default_value(Ok(None))
                .bind(lambda _: assigned(live, self.members))
                .bind(lambda _: self.parent.map(lambda frame: framed(live, frame)).default_value(Ok(None)))
            )
        )


class Connected(msgspec.Struct, frozen=True, kw_only=True):
    """Link tree lacks."""

    link: Link

    def applied(self, keyed: OwnedNodes, _identity: Identity) -> Outcome[None]:
        """Links the two ends, a target its node's members leave unavailable taking no link."""
        return paired(keyed.socket(self.link.source.node, self.link.source.socket, output=True), keyed.socket(self.link.target.node, self.link.target.socket, output=False)).bind(
            lambda pair: _linked(keyed.interface.tree, *pair, verify_limits=False).map(lambda _: None) if pair[1].enabled else Ok(None)
        )


class Reordered(msgspec.Struct, frozen=True, kw_only=True):
    """Desired link order at one input, top to bottom."""

    target: End
    sources: tuple[End, ...]

    def applied(self, keyed: OwnedNodes, identity: Identity) -> Outcome[None]:
        """Orders the owned links into the target by the sources' order."""

        def reordered(target: bpy.types.NodeSocket, sources: tuple[bpy.types.NodeSocket, ...]) -> Outcome[None]:
            incoming: dict[bpy.types.NodeSocket, deque[bpy.types.NodeLink]] = {}
            for link in _links_of(target):
                source, outlet, _, _ = _endpoints(link)
                if Mark.read(identity, source).is_some():
                    incoming.setdefault(outlet, deque()).append(link)
            return accumulated(
                Option.of_optional(incoming.get(source)).filter(bool).map(lambda links: links.popleft()).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(target), member=repr(source)))
                for source in sources
            ).bind(lambda desired: _ordered(target, desired))

        return paired(keyed.socket(self.target.node, self.target.socket, output=False), accumulated(keyed.socket(source.node, source.socket, output=True) for source in self.sources)).bind(
            lambda pair: reordered(*pair)
        )


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """Socket values live node lacks, by address."""

    key: Key
    inputs: Mapping[str | int, Value] = MappingProxyType({})
    outputs: Mapping[str | int, Value] = MappingProxyType({})

    def applied(self, keyed: OwnedNodes, _identity: Identity) -> Outcome[None]:
        """Writes socket defaults."""
        return (
            keyed.node(self.key).bind(lambda live: paired(valued(live.inputs, self.inputs, keyed.sockets[self.key]), valued(live.outputs, self.outputs, keyed.sockets[self.key]))).map(lambda _: None)
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def changes(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> Outcome[Changes]:
    """Reads tree against desired graph, writing nothing.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to read.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows by write phase: links to drop, structure, item retypes and places, node parts, links, link order, socket values, item parts, each phase empty
        for a current tree. `Error` of `absent` or `outside` for content member node class lacks, `unregistered` for refused attachment name.
    """
    graph, interface = (desired.graph, desired.interface) if isinstance(desired, NodeGroup) else (desired, Interface())
    return _read(graph, interface, OwnedNodes.of(tree, identity, graph.contents), identity)


def reconciled(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> Outcome[Changes]:
    """Writes `changes` rows phase by phase, owned nodes and socket addresses read again before each phase.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to write.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows written. `Error` of `changes` faults, or every `refused`, `absent`, or `outside` fault from phase writes.
    """
    contents = (desired.graph if isinstance(desired, NodeGroup) else desired).contents

    def wrote(phase: tuple[_Row, ...]) -> Outcome[None]:
        keyed = OwnedNodes.of(tree, identity, contents)
        return accumulated(row.applied(keyed, identity) for row in phase).map(lambda _: None)

    return changes(desired, tree, identity).bind(lambda rows: reduce(lambda done, phase: done.bind(lambda _: wrote(phase)), filter(None, rows), Result[None, Faults](ok=None)).map(lambda _: rows))


def made(group: NodeGroup, identity: Identity, *, of: Option[bpy.types.ID] = Nothing) -> Outcome[bpy.types.NodeTree]:
    """Reconciles reused or built group tree.

    Args:
        group: Owned group to make.
        identity: Extension marking group and its nodes.
        of: Data-block a single group stays with.

    Returns:
        `Ok` of group tree. `Error` of reconcile faults, removing any tree the call built.
    """
    return group.ownership.made(identity, partial(_new_group, group.graph.kind, group.name), of=of, filled=lambda tree: reconciled(group, tree, identity).map(lambda _: tree))


def valued(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str, Value] | Mapping[str | int, Value], keyed: Mapping[str, str]) -> Outcome[None]:
    """Writes socket defaults by identifier or enabled position, or by interface item key or item name through `keyed` socket identifiers."""
    return accumulated(_socket_at(sockets, address, keyed).bind(lambda socket: written(socket, ("default_value",), value)) for address, value in values.items()).map(lambda _: None)


def _read(graph: Graph, interface: Interface, keyed: OwnedNodes, identity: Identity) -> Outcome[Changes]:
    addressed, marked = keyed.interface, keyed.nodes
    tree = addressed.tree
    placed = graph.placed
    held = {key: node for key, node in marked.items() if key in placed and isinstance(node, placed[key][0].kind)}
    owners = {node: key for key, node in held.items()}
    sockets = {key: keyed.sockets[key] for key in held}

    def stale(
        sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str | int, Value], keyed: Mapping[str, str], interface: Option[Mapping[str, tuple[str, type[bpy.types.NodeSocket]]]]
    ) -> dict[str | int, Value]:
        return {
            address: value
            for address, value in values.items()
            if not any(
                not _differing(socket, {("default_value",): value})
                and interface.map(
                    lambda future: any(identifier == socket.identifier and address in {key, identifier} and isinstance(socket, kind) for key, (identifier, kind) in future.items())
                ).default_value(value=True)
                for socket in _socket_at(sockets, address, keyed).to_option().to_list()
            )
        }

    def altered(key: Key, node: Node, parent: Option[Key]) -> Outcome[Option[Altered]]:
        def compared(live: bpy.types.Node) -> Outcome[Altered]:
            framed = parent.map(lambda frame: frame in held and held[frame] == live.parent).default_with(lambda: live.parent is None)
            return (
                node.content
                .map(lambda wanted: wanted.holds(live, identity).map(lambda now: Nothing if now else Some(wanted)))
                .default_value(Ok(Nothing))
                .map(lambda content: Altered(key=key, members=_differing(live, node.paths), content=content, parent=Nothing if framed else Some(parent)))
            )

        fresh = Altered(key=key, members=node.paths, content=node.content, parent=parent.map(Some))
        return (compared(held[key]) if key in held else Ok(fresh)).map(lambda row: Some(row).filter(lambda row: row != Altered(key=key)))

    def linked(
        alterations: tuple[Altered, ...], rewritten: frozenset[Key], surviving: Mapping[Key, tuple[str, type[bpy.types.NodeSocket]]], positions: Mapping[Key, Mapping[bool, Mapping[int, str]]]
    ) -> tuple[tuple[Removed, ...], tuple[Connected, ...], tuple[Reordered, ...], dict[Key, Mapping[str, tuple[str, type[bpy.types.NodeSocket]]]]]:
        future_ids = {key: {output: frozenset(by_position.values()) for output, by_position in sides.items()} for key, sides in positions.items()}

        def addressed_end(endpoint: End, *, output: bool) -> End:
            match endpoint.socket:
                case int(position) if endpoint.node in positions:
                    identifier = positions[endpoint.node][output].get(position)
                    return msgspec.structs.replace(endpoint, socket=identifier) if identifier is not None else endpoint
                case int(position) if endpoint.node in held and endpoint.node not in rewritten:
                    live = held[endpoint.node]
                    return _socket_at(live.outputs if output else live.inputs, position).map(lambda socket: msgspec.structs.replace(endpoint, socket=socket.identifier)).default_value(endpoint)
                case str(address) if endpoint.node in sockets:
                    return msgspec.structs.replace(endpoint, socket=sockets[endpoint.node].get(address, address))
                case _:
                    return endpoint

        def unavailable(link: Link) -> bool:
            end = addressed_end(link.target, output=False)
            return end.node in held and end.node not in rewritten and _socket_at(held[end.node].inputs, end.socket).map(lambda socket: not socket.enabled).default_value(value=False)

        own = {key: surviving for key, live in held.items() if isinstance(live, bpy.types.NodeGroupInput | bpy.types.NodeGroupOutput)}
        pointed = {
            change.key: {
                item: (identifier, bpy.types.NodeSocket)
                for tree in group.ownership.reused(identity).to_list()
                for item, identifier in InterfaceKeys.of(tree, identity).retained(group.interface).items()
            }
            for change in alterations
            if change.key in held
            for group in change.content.to_list()
            if isinstance(group, NodeGroup)
        }
        interfaces: dict[Key, Mapping[str, tuple[str, type[bpy.types.NodeSocket]]]] = {**own, **pointed}
        keys = {node: {identifier: key for key, identifier in sockets[node].items()} for node in interfaces}

        def retained(end: End, *, output: bool) -> bool:
            if end.node in future_ids:
                return end.socket in future_ids[end.node][output]
            match end.socket:
                case str(socket) if end.node in interfaces:
                    return any(identifier == socket and keys[end.node].get(socket, socket) in {key, identifier} for key, (identifier, _) in interfaces[end.node].items())
                case _:
                    return end.node not in interfaces

        wanted = tuple(link for link in graph.links if not unavailable(link))
        ends = {socket: End(node=key, socket=socket.identifier) for key, live in held.items() for socket in (*live.inputs, *live.outputs)}
        joined_ends = {link: (ends[outlet], ends[inlet]) for link in tree.links for source, outlet, target, inlet in (_endpoints(link),) if source in owners and target in owners}
        existing = tuple(sorted(joined_ends, key=lambda link: link.multi_input_sort_id, reverse=True))
        available: dict[tuple[End, End], deque[bpy.types.NodeLink]] = {}
        present: dict[End, list[End]] = {}
        for link in existing:
            source, target = joined_ends[link]
            present.setdefault(target, []).append(source)
            if retained(source, output=True) and retained(target, output=False):
                available.setdefault((source, target), deque()).append(link)
        matched = tuple(
            (link, Option.of_optional(available.get((addressed_end(link.source, output=True), addressed_end(link.target, output=False)))).filter(bool).map(lambda candidates: candidates.popleft()))
            for link in wanted
        )
        kept_links = frozenset(found for _, held_link in matched for found in held_link.to_list())
        desired_links: dict[End, list[End]] = {}
        for declared in wanted:
            desired_links.setdefault(declared.target, []).append(declared.source)
        return (
            tuple(Removed(item=link) for link in existing if link not in kept_links),
            tuple(Connected(link=link) for link, found in reversed(matched) if found.is_none()),
            tuple(
                Reordered(target=target, sources=tuple(sources))
                for target, sources in desired_links.items()
                if len(sources) > 1 and tuple(present.get(addressed_end(target, output=False), ())) != tuple(addressed_end(source, output=True) for source in sources)
            ),
            interfaces,
        )

    def assembled(rows: tuple[Option[Altered], ...]) -> Changes:
        alterations = tuple(row for option in rows for row in option.to_list())
        positions = {
            row.key: content.positions(held[row.key], identity)
            for row in alterations
            if row.key in held
            for content in row.content.to_list()
            if isinstance(content, ItemContent) and not isinstance(content, NodeGroup)
        }
        rewritten = frozenset(row.key for row in alterations if row.members or row.content.is_some())
        entries = {key: entry for key, entry, _, _ in interface.walked()}
        surviving = {key: (address, entry[0].refined if isinstance(entry := entries[key], tuple) else bpy.types.NodeSocketBool) for key, address in addressed.retained(interface).items()}
        (withdrawn, declared, placements, restated), (disconnected, connected, reordered, interfaces) = _interfaced(addressed, interface), linked(alterations, rewritten, surviving, positions)
        values = tuple(
            Valued(
                key=key,
                inputs=stale(held[key].inputs, node.inputs, sockets[key], future := Option.of_optional(interfaces.get(key))),
                outputs=stale(held[key].outputs, node.outputs, sockets[key], future),
            )
            if key in held and key not in rewritten
            else Valued(key=key, inputs=node.inputs, outputs=node.outputs)
            for key, (node, _) in placed.items()
        )
        return (
            disconnected,
            (
                *(Configured(members=members) for members in (_differing(tree, graph.paths),) if members),
                *withdrawn,
                *declared,
                *(Removed(item=node) for node in marked.values() if node not in owners),
                *(Added(key=key, kind=node.kind) for key, (node, _) in placed.items() if key not in held),
            ),
            placements,
            alterations,
            connected,
            reordered,
            tuple(row for row in values if row.inputs or row.outputs),
            restated,
        )

    return accumulated(altered(key, node, parent) for key, (node, parent) in placed.items()).map(assembled)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Added", "Altered", "Changes", "Configured", "Connected", "NodeGroup", "Removed", "Reordered", "Valued", "changes", "made", "reconciled", "valued"]
