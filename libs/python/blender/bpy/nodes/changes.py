"""Desired graph reconciliation onto node trees: change rows that write themselves, owned groups, and nested group content."""

from collections import deque
from collections.abc import Mapping
import functools
from types import MappingProxyType

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.content import endpoints
from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.rna.descriptors import differing, Value, written
from blender.bpy.data.rna.paths import RnaPath, struct_at
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.addressing import Addressed, Declared, interfaced, Keyed, Placed, Restated, slot, Withdrawn
from blender.bpy.nodes.desired import Content, End, Graph, Link, member_paths, Node, NodeGroup
from blender.bpy.nodes.interface import Interface
from blender.bpy.nodes.trees import created, GraphTree, grouped, joined, links_of, ordered, socket_at
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------

type _Structural = Configured | Withdrawn | Declared | Removed | Added
type _Row = _Structural | Disconnected | Placed | Altered | Connected | Reordered | Valued | Restated

# --- [MODELS] ---------------------------------------------------------------------------

# --- [CONTENT]


class Nested(msgspec.Struct, frozen=True, kw_only=True):
    """Owned group a group node points at, reconciled as a hidden tree of its own."""

    group: NodeGroup

    @property
    def keyed(self) -> object:
        """Group key and name the lowering digest reads, the parts the node's owned tree takes from the group."""
        return (self.group.key, self.group.name)

    @property
    def nested(self) -> tuple[NodeGroup, ...]:
        """The group, then every group it nests."""
        return (self.group, *self.group.graph.nested)

    def holds(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[bool]:
        """Whether the node points at the group's owned tree and that tree holds the group."""
        return (
            struct_at(live, ("node_tree",), bpy.types.NodeTree)
            .filter(lambda tree: Mark.read(identity, tree) == Some(Mark(key=self.group.key)))
            .map(lambda tree: changes(self.group, tree, identity).map(lambda rows: rows == Changes()))
            .default_value(Ok(False))
        )

    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        """Makes the group's hidden tree and points the node at it."""
        return made(msgspec.structs.replace(self.group, name=f".{self.group.name}"), identity).bind(lambda tree: grouped(live, tree))

    def sockets(self, _live: bpy.types.Node, identity: Identity, slot: str, /) -> Mapping[str, str]:
        """Socket identifiers by interface item key of the group's owned tree, items the group still declares alone."""
        return (
            self.group.ownership
            .reused(identity)
            .map(lambda tree: Addressed.of(tree, slot))
            .map(lambda live: {key: identifier for key, identifier in live.sockets.items() if key in live.kept(self.group.interface)})
            .default_value({})
        )


# --- [ROWS]


class Configured(msgspec.Struct, frozen=True, kw_only=True):
    """Graph members tree lacks."""

    members: Mapping[RnaPath, Value]

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Writes the tree members."""
        return assigned(keyed.tree, self.members)


class Removed(msgspec.Struct, frozen=True, kw_only=True):
    """Owned node graph lacks or declares as another class."""

    name: str

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Removes the node."""
        nodes = keyed.tree.nodes
        return (
            Option
            .of_optional(nodes.get(self.name))
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(nodes), member=self.name))
            .bind(lambda node: catch(nodes.remove, exception=RuntimeError)(node).map_error(functools.partial(Fault.of, BlenderReason.REFUSED, repr(nodes))))
        )


class Added(msgspec.Struct, frozen=True, kw_only=True):
    """Node to create for a key."""

    key: Key
    kind: type[bpy.types.Node]

    def applied(self, keyed: Keyed, identity: Identity) -> Outcome[None]:
        """Creates the node and marks it with its key."""
        return created(keyed.tree, self.kind).bind(lambda made: Mark(key=self.key).write(identity, made))


class Altered(msgspec.Struct, frozen=True, kw_only=True):
    """Node parts live node lacks."""

    key: Key
    members: Mapping[RnaPath, Value] = MappingProxyType({})
    content: Option[Content] = Nothing
    parent: Option[Option[Key]] = Nothing

    def applied(self, keyed: Keyed, identity: Identity) -> Outcome[None]:
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


class Disconnected(msgspec.Struct, frozen=True, kw_only=True):
    """Owned link graph lacks."""

    link: bpy.types.NodeLink

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Removes the link."""
        links = keyed.tree.links
        return catch(links.remove, exception=RuntimeError)(self.link).map_error(functools.partial(Fault.of, BlenderReason.REFUSED, repr(links)))


class Connected(msgspec.Struct, frozen=True, kw_only=True):
    """Link tree lacks."""

    link: Link

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Links the two ends, a target its node's members leave unavailable taking no link."""
        return paired(keyed.socket(self.link.source.node, self.link.source.socket, output=True), keyed.socket(self.link.target.node, self.link.target.socket, output=False)).bind(
            lambda pair: joined(keyed.tree, *pair, verify_limits=False).map(lambda _: None) if pair[1].enabled else Ok(None)
        )


class Reordered(msgspec.Struct, frozen=True, kw_only=True):
    """Desired link order at one input, top to bottom."""

    target: End
    sources: tuple[End, ...]

    def applied(self, keyed: Keyed, identity: Identity) -> Outcome[None]:
        """Orders the owned links into the target by the sources' order."""

        def reordered(target: bpy.types.NodeSocket, sources: tuple[bpy.types.NodeSocket, ...]) -> Outcome[None]:
            marked = tuple(link for link in links_of(target) if Mark.read(identity, endpoints(link)[0]).is_some())
            return accumulated(
                Block
                .of_seq(marked)
                .filter(lambda link: endpoints(link)[1] == source)
                .skip(sources[:index].count(source))
                .try_head()
                .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(target), member=repr(source)))
                for index, source in enumerate(sources)
            ).bind(lambda desired: ordered(target, desired))

        return paired(keyed.socket(self.target.node, self.target.socket, output=False), accumulated(keyed.socket(source.node, source.socket, output=True) for source in self.sources)).bind(
            lambda pair: reordered(*pair)
        )


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """Socket values live node lacks, by address."""

    key: Key
    inputs: Mapping[str | int, Value] = MappingProxyType({})
    outputs: Mapping[str | int, Value] = MappingProxyType({})

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Writes socket defaults."""
        return (
            keyed.node(self.key).bind(lambda live: paired(valued(live.inputs, self.inputs, keyed.sockets[self.key]), valued(live.outputs, self.outputs, keyed.sockets[self.key]))).map(lambda _: None)
        )


class Changes(msgspec.Struct, frozen=True, kw_only=True):
    """Rows by write phase in write order, `Changes()` for current tree."""

    disconnected: tuple[Disconnected, ...] = ()
    structure: tuple[_Structural, ...] = ()
    placed: tuple[Placed, ...] = ()
    altered: tuple[Altered, ...] = ()
    connected: tuple[Connected, ...] = ()
    reordered: tuple[Reordered, ...] = ()
    values: tuple[Valued, ...] = ()
    restated: tuple[Restated, ...] = ()

    @property
    def phases(self) -> tuple[tuple[_Row, ...], ...]:
        """Rows of each phase in write order: links to drop, structure, item retypes and places, node parts, links, link order, socket values, item parts."""
        return (self.disconnected, self.structure, self.placed, self.altered, self.connected, self.reordered, self.values, self.restated)


# --- [OPERATIONS] -----------------------------------------------------------------------


def changes(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> Outcome[Changes]:
    """Reads tree against desired graph, writing nothing.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to read.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows. `Error` of `absent` or `choice` for content member node class lacks, `register` for refused attachment name.
    """
    graph = desired.graph if isinstance(desired, NodeGroup) else desired
    return slot(identity).bind(lambda name: _read(desired, Keyed.of(tree, identity, name, _contents(graph)), identity))


def reconciled(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> Outcome[Changes]:
    """Writes `changes` rows phase by phase, owned nodes and socket addresses read again before each phase.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to write.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows written. `Error` of `changes` faults, or every `edit`, `absent`, or `choice` fault from phase writes.
    """
    contents = _contents(desired.graph if isinstance(desired, NodeGroup) else desired)

    def phased(name: str, rows: Changes) -> Outcome[Changes]:
        def wrote(phase: tuple[_Row, ...]) -> Outcome[None]:
            keyed = Keyed.of(tree, identity, name, contents)
            return accumulated(row.applied(keyed, identity) for row in phase).map(lambda _: None)

        return functools.reduce(lambda done, phase: done.bind(lambda _: wrote(phase)), rows.phases, Result[None, Faults](ok=None)).map(lambda _: rows)

    return slot(identity).bind(lambda name: changes(desired, tree, identity).bind(lambda rows: phased(name, rows)))


def made(group: NodeGroup, identity: Identity, *, of: Option[bpy.types.ID] = Nothing) -> Outcome[bpy.types.NodeTree]:
    """Reconciles reused or built group tree.

    Args:
        group: Owned group to make.
        identity: Extension marking group and its nodes.
        of: Data-block a single group stays with.

    Returns:
        `Ok` of group tree. `Error` of reconcile faults, removing any tree the call built.
    """
    return group.ownership.made(
        identity,
        lambda: _new_group(group.name, group.graph.kind).map_error(functools.partial(Fault.of, BlenderReason.REFUSED, repr(bpy.data.node_groups))),
        of=of,
        filled=lambda tree: reconciled(group, tree, identity).map(lambda _: tree),
    )


def valued(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str, Value] | Mapping[str | int, Value], keyed: Mapping[str, str]) -> Outcome[None]:
    """Writes socket defaults by identifier or enabled position, or by interface item key or item name through `keyed` socket identifiers."""
    return accumulated(
        socket_at(sockets, address if isinstance(address, int) else keyed.get(address, address)).bind(lambda socket: written(socket, ("default_value",), value)) for address, value in values.items()
    ).map(lambda _: None)


def _contents(graph: Graph) -> dict[Key, Content]:
    """Content each desired node holds, by key."""
    return {key: content for key, (node, _) in graph.placed.items() for content in node.content.to_list()}


def _read(desired: Graph | NodeGroup, keyed: Keyed, identity: Identity) -> Outcome[Changes]:
    tree, name, first = keyed.tree, keyed.slot, keyed.nodes
    graph, interface = (desired.graph, desired.interface) if isinstance(desired, NodeGroup) else (desired, Interface())
    placed = graph.placed
    held = {key: node for key, node in first.items() if key in placed and isinstance(node, placed[key][0].kind)}
    owners = {node: key for key, node in held.items()}
    sockets = {key: keyed.sockets[key] for key in held}

    def stale(
        sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str | int, Value], keyed: Mapping[str, str], interface: Option[Mapping[str, tuple[str, type[bpy.types.NodeSocket]]]]
    ) -> dict[str | int, Value]:
        return {
            address: value
            for address, value in values.items()
            if not any(
                not differing(socket, {("default_value",): value})
                and interface.map(
                    lambda future: any(identifier == socket.identifier and address in {key, identifier} and isinstance(socket, kind) for key, (identifier, kind) in future.items())
                ).default_value(True)
                for socket in socket_at(sockets, address if isinstance(address, int) else keyed.get(address, address)).to_option().to_list()
            )
        }

    def altered(key: Key, node: Node, parent: Option[Key]) -> Outcome[Option[Altered]]:
        def drawn(live: bpy.types.Node) -> Outcome[Altered]:
            framed = parent.bind(lambda frame: Option.of_optional(held.get(frame))).to_optional() == live.parent
            return (
                node.content
                .map(lambda wanted: wanted.holds(live, identity).map(lambda now: Nothing if now else Some(wanted)))
                .default_value(Ok(Nothing))
                .map(lambda content: Altered(key=key, members=differing(live, member_paths(node.members)), content=content, parent=Nothing if framed else Some(parent)))
            )

        fresh = Altered(key=key, members=member_paths(node.members), content=node.content, parent=parent.map(Some))
        return (drawn(held[key]) if key in held else Ok(fresh)).map(lambda row: Some(row).filter(lambda row: row != Altered(key=key)))

    def linked(
        alterations: tuple[Altered, ...], rewritten: frozenset[Key]
    ) -> tuple[tuple[Disconnected, ...], tuple[Connected, ...], tuple[Reordered, ...], dict[Key, Mapping[str, tuple[str, type[bpy.types.NodeSocket]]]]]:

        def addressed(endpoint: End, *, output: bool) -> End:
            match endpoint.socket:
                case int(position) if endpoint.node in held and endpoint.node not in rewritten:
                    live = held[endpoint.node]
                    return socket_at(live.outputs if output else live.inputs, position).map(lambda socket: msgspec.structs.replace(endpoint, socket=socket.identifier)).default_value(endpoint)
                case str(address) if endpoint.node in sockets:
                    return msgspec.structs.replace(endpoint, socket=sockets[endpoint.node].get(address, address))
                case _:
                    return endpoint

        def surviving(group: bpy.types.NodeTree, face: Interface) -> dict[str, tuple[str, type[bpy.types.NodeSocket]]]:
            live, entries = Addressed.of(group, name), {key: entry for key, entry, _, _ in face.walked()}
            items = live.kept(face)
            return {key: (address, entry[0].kind if isinstance(entry := entries[key], tuple) else bpy.types.NodeSocketBool) for key, address in live.sockets.items() if key in items}

        def unavailable(link: Link) -> bool:
            end = addressed(link.target, output=False)
            return end.node in held and end.node not in rewritten and socket_at(held[end.node].inputs, end.socket).map(lambda socket: not socket.enabled).default_value(False)

        own = {key: surviving(tree, interface) for key, live in held.items() if isinstance(live, bpy.types.NodeGroupInput | bpy.types.NodeGroupOutput)}
        pointed = {
            change.key: {item: (identifier, bpy.types.NodeSocket) for item, identifier in content.sockets(held[change.key], identity, name).items()}
            for change in alterations
            if change.key in held
            for content in change.content.to_list()
            if content.nested
        }
        interfaces: dict[Key, Mapping[str, tuple[str, type[bpy.types.NodeSocket]]]] = {**own, **pointed}
        keys = {node: {identifier: key for key, identifier in sockets[node].items()} for node in interfaces}

        def retained(end: End) -> bool:
            match end.socket:
                case str(socket) if end.node in interfaces:
                    return any(identifier == socket and keys[end.node].get(socket, socket) in {key, identifier} for key, (identifier, _) in interfaces[end.node].items())
                case _:
                    return end.node not in interfaces

        wanted = tuple(link for link in graph.links if not unavailable(link))
        ends = {socket: End(node=key, socket=socket.identifier) for key, live in held.items() for socket in (*live.inputs, *live.outputs)}
        endpoints = {
            link: (ends[link.from_socket], ends[link.to_socket])  # ty: ignore[invalid-argument-type]
            for link in tree.links
            if link.from_node in owners and link.to_node in owners
        }
        existing = tuple(sorted(endpoints, key=lambda link: link.multi_input_sort_id, reverse=True))
        available: dict[tuple[End, End], deque[bpy.types.NodeLink]] = {}
        present: dict[End, tuple[End, ...]] = {}
        for link in existing:
            source, target = endpoints[link]
            present[target] = (*present.get(target, ()), source)
            if retained(source) and retained(target):
                available.setdefault((source, target), deque()).append(link)
        matched = tuple(
            (link, Option.of_optional(available.get((addressed(link.source, output=True), addressed(link.target, output=False)))).filter(bool).map(lambda candidates: candidates.popleft()))
            for link in wanted
        )
        kept_links = frozenset(found for _, held_link in matched for found in held_link.to_list())
        desired_links: dict[End, tuple[End, ...]] = {}
        for declared in wanted:
            desired_links[declared.target] = (*desired_links.get(declared.target, ()), declared.source)
        return (
            tuple(Disconnected(link=link) for link in existing if link not in kept_links),
            tuple(Connected(link=link) for link, found in reversed(matched) if found.is_none()),
            tuple(
                Reordered(target=target, sources=sources)
                for target, sources in desired_links.items()
                if len(sources) > 1 and present.get(addressed(target, output=False), ()) != tuple(addressed(source, output=True) for source in sources)
            ),
            interfaces,
        )

    def assembled(rows: tuple[Option[Altered], ...]) -> Changes:
        alterations = tuple(row for option in rows for row in option.to_list())
        rewritten = frozenset(row.key for row in alterations if row.members)
        (withdrawn, declared, placements, restated), (disconnected, connected, reordered, interfaces) = interfaced(tree, interface, name), linked(alterations, rewritten)
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
        return Changes(
            disconnected=disconnected,
            structure=(
                *(Configured(members=members) for members in (differing(tree, member_paths(graph.members)),) if members),
                *withdrawn,
                *declared,
                *(Removed(name=node.name) for node in first.values() if node not in owners),
                *(Added(key=key, kind=node.kind) for key, (node, _) in placed.items() if key not in held),
            ),
            placed=placements,
            altered=alterations,
            connected=connected,
            reordered=reordered,
            values=tuple(row for row in values if row.inputs or row.outputs),
            restated=restated,
        )

    return accumulated(altered(key, node, parent) for key, (node, parent) in placed.items()).map(assembled)


@catch(exception=RuntimeError)
def _new_group(name: str, kind: type[GraphTree]) -> bpy.types.NodeTree:
    """New empty group of `kind`, the host answering a tree for every name and tree class it accepts."""
    tree: bpy.types.NodeTree = bpy.data.node_groups.new(name, kind.__name__)  # type: ignore[arg-type, assignment]  # ty: ignore[invalid-argument-type, invalid-assignment]
    return tree


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Added", "Altered", "Changes", "Configured", "Connected", "Disconnected", "Nested", "Removed", "Reordered", "Valued", "changes", "made", "reconciled", "valued"]
