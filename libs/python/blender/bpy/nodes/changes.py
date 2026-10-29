"""Desired graph reconciliation onto node trees: interface and node change rows that write themselves, write phases, owned groups, and nested group content."""

from collections import deque
from collections.abc import Mapping
from functools import partial, reduce
from types import MappingProxyType
from typing import override

import bpy
from expression import Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.rna.descriptors import differing, Value, written
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.addressing import Addressed, InterfaceItem, Keyed
from blender.bpy.nodes.desired import Addressing, Content, DesiredItem, End, Graph, Interface, InterfacePanel, Link, member_paths, Node, NodeGroup, Socket
from blender.bpy.nodes.trees import base_socket, created, endpoints, group_tree, grouped, joined, links_of, ordered, socket_at
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason, typed

# --- [TYPES] ----------------------------------------------------------------------------

type _Row = Configured | Withdrawn | Declared | Removed | Added | Placed | Altered | Connected | Reordered | Valued | Restated
type Changes = tuple[tuple[_Row, ...], ...]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [CONTENT]


class Nested(Addressing, frozen=True, kw_only=True):
    """Owned group a group node points at, reconciled as a hidden tree of its own."""

    group: NodeGroup

    @property
    @override
    def keyed(self) -> object:
        """Group key and name the lowering digest reads, the parts the node's owned tree takes from the group."""
        return (self.group.key, self.group.name)

    @property
    @override
    def nested(self) -> tuple[NodeGroup, ...]:
        """The group, then every group it nests."""
        return (self.group, *self.group.graph.nested)

    @override
    def holds(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[bool]:
        return (
            group_tree(live)
            .filter(lambda tree: Mark.read(identity, tree) == Some(Mark(key=self.group.key)))
            .map(lambda tree: changes(self.group, tree, identity).map(lambda rows: not any(rows)))
            .default_value(Ok(False))
        )

    @override
    def write(self, live: bpy.types.Node, identity: Identity, /) -> Outcome[None]:
        return made(msgspec.structs.replace(self.group, name=f".{self.group.name}"), identity).bind(lambda tree: grouped(live, tree))

    @override
    def sockets(self, live: bpy.types.Node, identity: Identity, /) -> Mapping[str, str]:
        return (
            self.group.ownership
            .reused(identity)
            .bind(lambda tree: Addressed.of(tree, identity).to_option())
            .map(lambda held: (held.sockets, held.kept(self.group.interface)))
            .map(lambda pair: {key: identifier for key, identifier in pair[0].items() if key in pair[1]})
            .default_value({})
        )


# --- [INTERFACE_ROWS]


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
                    return catch(face.new_socket, exception=TypeError)(socket.title, description=socket.description, in_out=side, socket_type=socket.base.__name__).map_error(refused)
                case InterfacePanel(name=name, description=description, closed=closed, toggle=toggle):
                    return (
                        catch(face.new_panel, exception=TypeError)(name, description=description, default_closed=closed)
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


# --- [NODE_ROWS]


class Configured(msgspec.Struct, frozen=True, kw_only=True):
    """Graph members tree lacks."""

    members: Mapping[RnaPath, Value]

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Writes the tree members."""
        return assigned(keyed.tree, self.members)


class Removed(msgspec.Struct, frozen=True, kw_only=True):
    """Owned link or node graph lacks or declares as another class."""

    item: bpy.types.Node | bpy.types.NodeLink

    def applied(self, keyed: Keyed, _identity: Identity) -> Outcome[None]:
        """Removes the link or node, the node with its links."""
        match self.item:
            case bpy.types.NodeLink() as link:
                answer = catch(keyed.tree.links.remove, exception=RuntimeError)(link)
            case node:
                answer = catch(keyed.tree.nodes.remove, exception=RuntimeError)(node)
        return answer.map_error(partial(Fault.of, Reason.REFUSED, repr(keyed.tree)))


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
                .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(target), member=repr(source)))
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


# --- [OPERATIONS] -----------------------------------------------------------------------


def changes(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> Outcome[Changes]:
    """Reads tree against desired graph, writing nothing.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to read.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows by write phase: links to drop, structure, item retypes and places, node parts, links, link order, socket values, item parts, each phase empty
        for a current tree. `Error` of `absent` or `choice` for content member node class lacks, `register` for refused attachment name.
    """
    graph, interface = (desired.graph, desired.interface) if isinstance(desired, NodeGroup) else (desired, Interface())
    return paired(Keyed.of(tree, identity, graph.contents), Addressed.of(tree, identity)).bind(lambda pair: _read(graph, interface, *pair, identity))


def reconciled(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> Outcome[Changes]:
    """Writes `changes` rows phase by phase, owned nodes and socket addresses read again before each phase.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to write.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows written. `Error` of `changes` faults, or every `edit`, `absent`, or `choice` fault from phase writes.
    """
    contents = (desired.graph if isinstance(desired, NodeGroup) else desired).contents

    def wrote(phase: tuple[_Row, ...]) -> Outcome[None]:
        return Keyed.of(tree, identity, contents).bind(lambda keyed: accumulated(row.applied(keyed, identity) for row in phase)).map(lambda _: None)

    return changes(desired, tree, identity).bind(lambda rows: reduce(lambda done, phase: done.bind(lambda _: wrote(phase)), rows, Result[None, Faults](ok=None)).map(lambda _: rows))


def made(group: NodeGroup, identity: Identity, *, of: Option[bpy.types.ID] = Nothing) -> Outcome[bpy.types.NodeTree]:
    """Reconciles reused or built group tree.

    Args:
        group: Owned group to make.
        identity: Extension marking group and its nodes.
        of: Data-block a single group stays with.

    Returns:
        `Ok` of group tree. `Error` of reconcile faults, removing any tree the call built.
    """
    request = repr(bpy.data.node_groups)

    def built() -> Outcome[bpy.types.NodeTree]:
        tree = catch(bpy.data.node_groups.new, exception=TypeError)(group.name, group.graph.kind.__name__)
        return tree.map_error(partial(Fault.of, Reason.REFUSED, request)).bind(partial(typed, bpy.types.NodeTree, request))

    return group.ownership.made(identity, built, of=of, filled=lambda tree: reconciled(group, tree, identity).map(lambda _: tree))


def valued(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str, Value] | Mapping[str | int, Value], keyed: Mapping[str, str]) -> Outcome[None]:
    """Writes socket defaults by identifier or enabled position, or by interface item key or item name through `keyed` socket identifiers."""
    return accumulated(socket_at(sockets, address, keyed).bind(lambda socket: written(socket, ("default_value",), value)) for address, value in values.items()).map(lambda _: None)


def interfaced(addressed: Addressed, interface: Interface, /) -> tuple[tuple[Withdrawn, ...], tuple[Declared, ...], tuple[Placed, ...], tuple[Restated, ...]]:
    """Reads live interface against desired one, writing nothing.

    Args:
        addressed: Stored item keys and live items of the group.
        interface: Interface group declares.

    Returns:
        Items to withdraw last first, items to declare, retypes and places, siblings of each new or misplaced item placed again in preorder, and differing parts per item.
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
                return changed(item.socket_type, socket.base.__name__).map(lambda _: socket.kind)
            case _:
                return Nothing

    def restated(key: Key, entry: DesiredItem) -> Option[Restated]:
        match entry, held.get(key):
            case (Socket() as socket, _), bpy.types.NodeTreeInterfaceSocket() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, socket.title),
                    description=changed(item.description, socket.description),
                    members=kind(key, entry).map(lambda _: socket.members).default_with(lambda: differing(item, socket.members)),
                )
            case InterfacePanel() as panel, bpy.types.NodeTreeInterfacePanel() as item:
                row = Restated(
                    key=key,
                    name=changed(item.name, panel.name),
                    description=changed(item.description, panel.description),
                    closed=changed(item.default_closed, panel.closed),
                    toggle=panel.toggle.filter(lambda value: all(differing(toggle, {("default_value",): value}) for toggle in addressed.toggle(item).to_list())),
                )
            case (Socket() as socket, _), _:
                row = Restated(key=key, members=socket.members)
            case _:
                row = Restated(key=key)
        return Some(row).filter(lambda differs: differs != Restated(key=key))

    placed = tuple(
        row
        for key, entry, parent, position in entries
        for row in Some(Placed(key=key, kind=kind(key, entry), place=Some((parent, position)) if parent in shifted else Nothing)).filter(lambda row: row != Placed(key=row.key)).to_list()
    )
    return (
        tuple(Withdrawn(key=key) for identifier in reversed(tuple(addressed.live)) for key in Option.of_optional(inverse.get(identifier)).to_list() if key not in wanted or key in fresh),
        tuple(Declared(key=key, entry=entry) for key, entry, _, _ in entries if key in fresh),
        placed,
        tuple(row for key, entry, _, _ in entries for row in restated(key, entry).to_list()),
    )


def _read(graph: Graph, interface: Interface, keyed: Keyed, addressed: Addressed, identity: Identity) -> Outcome[Changes]:
    tree, first = keyed.tree, keyed.nodes
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
                for socket in socket_at(sockets, address, keyed).to_option().to_list()
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
        alterations: tuple[Altered, ...], rewritten: frozenset[Key], surviving: Mapping[Key, tuple[str, type[bpy.types.NodeSocket]]]
    ) -> tuple[tuple[Removed, ...], tuple[Connected, ...], tuple[Reordered, ...], dict[Key, Mapping[str, tuple[str, type[bpy.types.NodeSocket]]]]]:

        def addressed_end(endpoint: End, *, output: bool) -> End:
            match endpoint.socket:
                case int(position) if endpoint.node in held and endpoint.node not in rewritten:
                    live = held[endpoint.node]
                    return socket_at(live.outputs if output else live.inputs, position).map(lambda socket: msgspec.structs.replace(endpoint, socket=socket.identifier)).default_value(endpoint)
                case str(address) if endpoint.node in sockets:
                    return msgspec.structs.replace(endpoint, socket=sockets[endpoint.node].get(address, address))
                case _:
                    return endpoint

        def unavailable(link: Link) -> bool:
            end = addressed_end(link.target, output=False)
            return end.node in held and end.node not in rewritten and socket_at(held[end.node].inputs, end.socket).map(lambda socket: not socket.enabled).default_value(False)

        own = {key: surviving for key, live in held.items() if isinstance(live, bpy.types.NodeGroupInput | bpy.types.NodeGroupOutput)}
        pointed = {
            change.key: {item: (identifier, bpy.types.NodeSocket) for item, identifier in content.sockets(held[change.key], identity).items()}
            for change in alterations
            if change.key in held
            for content in change.content.bind(lambda each: Option.of_optional(try_downcast(Addressing, each))).to_list()
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
        joined_ends = {link: (ends[outlet], ends[inlet]) for link in tree.links for source, outlet, target, inlet in (endpoints(link),) if source in owners and target in owners}
        existing = tuple(sorted(joined_ends, key=lambda link: link.multi_input_sort_id, reverse=True))
        available: dict[tuple[End, End], deque[bpy.types.NodeLink]] = {}
        present: dict[End, tuple[End, ...]] = {}
        for link in existing:
            source, target = joined_ends[link]
            present[target] = (*present.get(target, ()), source)
            if retained(source) and retained(target):
                available.setdefault((source, target), deque()).append(link)
        matched = tuple(
            (link, Option.of_optional(available.get((addressed_end(link.source, output=True), addressed_end(link.target, output=False)))).filter(bool).map(lambda candidates: candidates.popleft()))
            for link in wanted
        )
        kept_links = frozenset(found for _, held_link in matched for found in held_link.to_list())
        desired_links: dict[End, tuple[End, ...]] = {}
        for declared in wanted:
            desired_links[declared.target] = (*desired_links.get(declared.target, ()), declared.source)
        return (
            tuple(Removed(item=link) for link in existing if link not in kept_links),
            tuple(Connected(link=link) for link, found in reversed(matched) if found.is_none()),
            tuple(
                Reordered(target=target, sources=sources)
                for target, sources in desired_links.items()
                if len(sources) > 1 and present.get(addressed_end(target, output=False), ()) != tuple(addressed_end(source, output=True) for source in sources)
            ),
            interfaces,
        )

    def assembled(rows: tuple[Option[Altered], ...]) -> Changes:
        alterations = tuple(row for option in rows for row in option.to_list())
        rewritten = frozenset(row.key for row in alterations if row.members)
        entries = {key: entry for key, entry, _, _ in interface.walked()}
        items = addressed.kept(interface)
        surviving = {key: (address, entry[0].kind if isinstance(entry := entries[key], tuple) else bpy.types.NodeSocketBool) for key, address in addressed.sockets.items() if key in items}
        (withdrawn, declared, placements, restated), (disconnected, connected, reordered, interfaces) = interfaced(addressed, interface), linked(alterations, rewritten, surviving)
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
                *(Configured(members=members) for members in (differing(tree, member_paths(graph.members)),) if members),
                *withdrawn,
                *declared,
                *(Removed(item=node) for node in first.values() if node not in owners),
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

__all__ = [
    "Added",
    "Altered",
    "Changes",
    "Configured",
    "Connected",
    "Declared",
    "Nested",
    "Placed",
    "Removed",
    "Reordered",
    "Restated",
    "Valued",
    "Withdrawn",
    "changes",
    "interfaced",
    "made",
    "reconciled",
    "valued",
]
