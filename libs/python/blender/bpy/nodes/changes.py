"""Desired graph reconciliation onto node trees, owned groups, and node content."""

from collections import deque
from collections.abc import Callable, Mapping
import functools
import itertools
from types import MappingProxyType
from typing import Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.images import generated, loaded
from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.rna.descriptors import assigned, definition, Descriptor, differing, Native, RnaPath, struct_at, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import Content, CurvePoints, End, FileItems, Generated, Graph, Link, Loaded, Nested, Node, NodeGroup, Points
from blender.bpy.nodes.interface import Addressed, addresses, Declared, Interface, interfaced, MenuItems, Restated, revised, slot, Withdrawn
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.nodes.trees import created, grouped, joined, ordered, socket_at
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.trees import Owner, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type _Structural = Configured | Withdrawn | Declared | Removed | Added
type _Alteration = Altered | Disconnected
type _TreeChange = _Structural | _Alteration | Connected | Reordered | Valued | Restated

# --- [MODELS] ---------------------------------------------------------------------------


class Configured(msgspec.Struct, frozen=True, kw_only=True):
    """Graph members tree lacks."""

    members: Mapping[Descriptor, Value]


class Removed(msgspec.Struct, frozen=True, kw_only=True):
    """Owned node graph lacks or declares as another class."""

    name: str


class Added(msgspec.Struct, frozen=True, kw_only=True):
    """Node to create for a key."""

    key: Key
    kind: type[bpy.types.Node]


class Altered(msgspec.Struct, frozen=True, kw_only=True):
    """Node parts live node lacks."""

    key: Key
    members: Mapping[Descriptor, Value] = MappingProxyType({})
    content: Option[Content] = Nothing
    parent: Option[Option[Key]] = Nothing


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """Socket values live node lacks, by address."""

    key: Key
    inputs: Mapping[str | int, Value] = MappingProxyType({})
    outputs: Mapping[str | int, Value] = MappingProxyType({})


class Disconnected(msgspec.Struct, frozen=True, kw_only=True):
    """Owned link graph lacks."""

    link: bpy.types.NodeLink


class Connected(msgspec.Struct, frozen=True, kw_only=True):
    """Link tree lacks."""

    link: Link


class Reordered(msgspec.Struct, frozen=True, kw_only=True):
    """Desired link order at one input, top to bottom."""

    target: End
    sources: tuple[End, ...]


class Changes(msgspec.Struct, frozen=True, kw_only=True):
    """Rows by write phase, `Changes()` for current tree."""

    structure: tuple[_Structural, ...] = ()
    alterations: tuple[_Alteration, ...] = ()
    links: tuple[Connected | Reordered, ...] = ()
    values: tuple[Valued, ...] = ()
    restatements: tuple[Restated, ...] = ()


class Keyed(msgspec.Struct, frozen=True, kw_only=True):
    """Owned nodes and their socket identifiers by item key or name, read once per write phase."""

    tree: bpy.types.NodeTree
    slot: str
    nodes: Mapping[Key, bpy.types.Node]
    sockets: Mapping[Key, Mapping[str, str]]

    @classmethod
    def of(cls, tree: bpy.types.NodeTree, identity: Identity, slot: str) -> Self:
        """Reads `owned` nodes of tree and their socket identifiers by item key on group input, output, or group node, by item name on Menu Switch or File Output."""
        nodes, own = owned(tree, identity), addresses(tree, slot)

        def addressed(node: bpy.types.Node) -> dict[str, str]:
            match node:
                case bpy.types.NodeGroupInput() | bpy.types.NodeGroupOutput():
                    return own
                case bpy.types.CompositorNodeGroup() | bpy.types.ShaderNodeGroup() | bpy.types.GeometryNodeGroup():
                    return Option.of_optional(node.node_tree).map(lambda group: addresses(group, slot)).default_value({})
                case bpy.types.GeometryNodeMenuSwitch():
                    fixed = {socket.identifier for socket in (*node.inputs, *node.outputs)}  # type: ignore[misc]  # ty: ignore[not-iterable]
                    return {item.name: socket.identifier for item, socket in zip(node.enum_items, tuple(node.outputs)[1:]) if item.name not in fixed}  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                case bpy.types.CompositorNodeOutputFile():
                    return {item.name: socket.identifier for item, socket in zip(node.file_output_items, node.inputs)}  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                case _:
                    return {}

        return cls(tree=tree, slot=slot, nodes=nodes, sockets={key: addressed(node) for key, node in nodes.items()})

    def node(self, key: Key) -> Outcome[bpy.types.Node]:
        """Owned node at key, `absent` fault naming key when no node holds it."""
        return Option.of_optional(self.nodes.get(key)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(self.tree.nodes), member=key),))

    def socket(self, end: End, *, output: bool) -> Outcome[bpy.types.NodeSocket]:
        """Live socket at end, addressed by item key, item name, identifier, or enabled position."""
        address = end.socket if isinstance(end.socket, int) else self.sockets[end.node].get(end.socket, end.socket)
        return self.node(end.node).bind(lambda live: socket_at(live.outputs if output else live.inputs, address))


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
    return slot(identity).bind(lambda name: _read(desired, Keyed.of(tree, identity, name), identity))


def reconciled(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> Outcome[Changes]:
    """Writes `changes` rows phase by phase.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to write.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows written. `Error` of `changes` faults, or every `edit`, `absent`, or `choice` fault from phase writes.
    """

    def phased(name: str, rows: Changes) -> Outcome[Changes]:
        def wrote(phase: tuple[_TreeChange, ...]) -> Outcome[None]:
            keyed = Keyed.of(tree, identity, name)
            return accumulated(applied(row, identity, keyed) for row in phase).map(lambda _: None)

        phases: tuple[tuple[_TreeChange, ...], ...] = (
            tuple(row for row in rows.alterations if isinstance(row, Disconnected)),
            rows.structure,
            tuple(Restated(key=row.key, kind=row.kind, place=row.place) for row in rows.restatements if row.kind.is_some() or row.place.is_some()),
            tuple(row for row in rows.alterations if isinstance(row, Altered)),
            tuple(row for row in rows.links if isinstance(row, Connected)),
            tuple(row for row in rows.links if isinstance(row, Reordered)),
            rows.values,
            tuple(msgspec.structs.replace(row, kind=Nothing, place=Nothing) for row in rows.restatements),
        )
        return functools.reduce(lambda done, phase: done.bind(lambda _: wrote(phase)), phases, Result[None, Faults](ok=None)).map(lambda _: rows)

    return slot(identity).bind(lambda name: changes(desired, tree, identity).bind(lambda rows: phased(name, rows)))


def owned(tree: bpy.types.NodeTree, identity: Identity) -> dict[Key, bpy.types.Node]:
    """Marked nodes by key, first in tree order over later user copies."""
    return dict(reversed(tuple((mark.key, node) for node in tree.nodes for mark in Mark.read(identity, node).to_list())))


def applied(row: _TreeChange, identity: Identity, keyed: Keyed) -> Outcome[None]:
    """Writes one change row onto a tree.

    Args:
        row: Row to write.
        identity: Extension marking owned nodes.
        keyed: Owned nodes and socket addresses of the tree before the row's phase.

    Returns:
        `Ok` once written. `Error` of `edit` for refused node class or write, `absent` for unresolved node, item, or socket, `choice` for refused enum identifier.
    """
    tree = keyed.tree

    def altered(change: Altered) -> Outcome[None]:
        return keyed.node(change.key).bind(
            lambda live: (
                change.content
                .map(lambda content: stored(content, live, identity))
                .default_value(Ok(None))
                .bind(lambda _: assigned(live, change.members))
                .bind(
                    lambda _: change.parent.map(
                        lambda frame: frame.map(lambda key: keyed.node(key).map(Some)).default_value(Ok(Nothing)).bind(lambda found: written(live, "parent", found.to_optional())).map(lambda _: None)
                    ).default_value(Ok(None))
                )
            )
        )

    def reordered(target: bpy.types.NodeSocket, sources: tuple[bpy.types.NodeSocket, ...]) -> Outcome[None]:
        marked = tuple(link for link in target.links if Mark.read(identity, link.from_node).is_some())  # type: ignore[union-attr, arg-type]  # ty: ignore[not-iterable, invalid-argument-type]
        return accumulated(
            Option.of_optional(next(itertools.islice((link for link in marked if link.from_socket == source), sources[:index].count(source), None), None)).to_result_with(
                lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(target), member=repr(source)),)
            )
            for index, source in enumerate(sources)
        ).bind(lambda desired: ordered(target, desired))

    match row:
        case Configured(members=members):
            return assigned(tree, members)
        case Withdrawn() | Declared() | Restated():
            return revised(row, tree, keyed.slot)
        case Removed(name=name):
            return (
                Option
                .of_optional(tree.nodes.get(name))
                .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(tree.nodes), member=name),))
                .bind(lambda node: catch(tree.nodes.remove, exception=RuntimeError)(node).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(tree.nodes), cause=error),)))
            )
        case Added(key=key, kind=kind):
            return created(tree, kind).bind(lambda made: Mark(key=key).write(identity, made))
        case Altered():
            return altered(row)
        case Disconnected(link=link):
            return catch(tree.links.remove, exception=RuntimeError)(link).map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(tree.links), cause=error),))
        case Connected(link=link):
            return paired(keyed.socket(link.source, output=True), keyed.socket(link.target, output=False)).bind(lambda pair: joined(tree, *pair, verify_limits=False)).map(lambda _: None)
        case Reordered(target=target, sources=sources):
            return paired(keyed.socket(target, output=False), accumulated(keyed.socket(source, output=True) for source in sources)).bind(lambda pair: reordered(*pair))
        case Valued(key=key, inputs=inputs, outputs=outputs):
            return keyed.node(key).bind(lambda live: paired(valued(live.inputs, inputs, keyed.sockets[key]), valued(live.outputs, outputs, keyed.sockets[key]))).map(lambda _: None)


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
        lambda: (
            catch(bpy.data.node_groups.new, exception=RuntimeError)(group.name, group.graph.kind.__name__)
            .map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(bpy.data.node_groups), cause=error),))
            .bind(lambda tree: Option.of_optional(tree).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(bpy.data.node_groups), member=group.name),)))
        ),
        of=of,
        filled=lambda tree: reconciled(group, tree, identity).map(lambda _: tree),
    )


def stored(content: Content, live: bpy.types.Node, identity: Identity) -> Outcome[None]:
    """Writes declared content onto a node.

    Args:
        content: Ramp, curve, image, group, menu, or File Output items to write.
        live: Node caller resource scope holds.
        identity: Extension owning referenced groups, images, and content marks.

    Returns:
        `Ok` once written. `Error` of snapshot, image, group, and RNA faults.
    """

    def arranged(
        items: bpy.types.NodeMenuSwitchItems | bpy.types.NodeCompositorFileOutputItems, names: tuple[str, ...], added: Callable[[str], object], removed: Callable[[str], object]
    ) -> tuple[bpy.types.NodeEnumItem | bpy.types.NodeCompositorFileOutputItem, ...]:
        for name in tuple(item.name for item in items if item.name not in names):
            removed(name)
        for name in [name for name in names if items.get(name) is None]:
            added(name)
        for index, name in enumerate(names):
            items.move(items.find(name), index)
        return tuple(items)

    def refreshed(mapping: Option[Native]) -> None:
        for curves in mapping.to_list():
            if isinstance(curves, bpy.types.CurveMapping):
                curves.reset_view()
                curves.update()

    match content:
        case Ramp() as ramp:
            return _ramp(live).bind(ramp.write)
        case CurvePoints():
            return Snapshot(struct=definition(live).identifier, tree=_tree(content)).apply(live).outcome.map(lambda _: refreshed(struct_at(live, ("mapping",))))
        case Generated(ownership=ownership, content=hashed, name=name, plane=plane, tag=tag):
            return generated(ownership, identity, name, hashed, plane, tag).bind(
                lambda image: written(live, "image", image.to_optional()).bind(
                    lambda _: (
                        Mark
                        .read(identity, live)
                        .map(lambda mark: msgspec.structs.replace(mark, content=Some(hashed) if image.is_none() else Nothing).write(identity, live))
                        .default_value(Error((Fault(reason=BlenderReason.ABSENT, subject=repr(live), member=identity.manifest.id),)))
                    )
                )
            )
        case Loaded(path=path, tag=tag, frames=frames):
            return loaded(path, tag).bind(lambda image: written(live, "image", image)).bind(lambda _: frames.map(lambda want: _framed(live).bind(want.applied)).default_value(Ok(None)))
        case Nested(group=group):
            return made(msgspec.structs.replace(group, name=f".{group.name}"), identity).bind(lambda tree: grouped(live, tree))
        case MenuItems() as menu:
            return (
                _narrowed(live, bpy.types.GeometryNodeMenuSwitch, "enum_items")
                .map(lambda switch: arranged(switch.enum_items, tuple(each.name for each in menu.items), switch.enum_items.new, lambda name: switch.enum_items.remove(switch.enum_items[name])))
                .bind(lambda held: accumulated(written(each, "description", item.description) for each, item in zip(held, menu.items)).map(lambda _: None))
            )
        case FileItems() as files:
            kinds = {item.name: item.kind for item in files.items}
            return (
                _narrowed(live, bpy.types.CompositorNodeOutputFile, "file_output_items")
                .map(
                    lambda output: arranged(
                        output.file_output_items,
                        tuple(kinds),
                        lambda name: output.file_output_items.new(kinds[name], name),
                        lambda name: output.file_output_items.remove(output.file_output_items[name]),
                    )
                )
                .bind(lambda held: accumulated(paired(written(each, "socket_type", item.kind), assigned(each, item.members)) for each, item in zip(held, files.items)).map(lambda _: None))
            )


def valued(sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str, Value] | Mapping[str | int, Value], keyed: Mapping[str, str]) -> Outcome[None]:
    """Writes socket defaults by identifier or enabled position, or by interface item key or item name through `keyed` socket identifiers."""
    return accumulated(
        socket_at(sockets, address if isinstance(address, int) else keyed.get(address, address)).bind(lambda socket: written(socket, "default_value", value)) for address, value in values.items()
    ).map(lambda _: None)


def _read(desired: Graph | NodeGroup, keyed: Keyed, identity: Identity) -> Outcome[Changes]:
    tree, name, first = keyed.tree, keyed.slot, keyed.nodes

    def referenced[T: bpy.types.ID](live: bpy.types.Node, identifier: str, kind: type[T]) -> Option[T]:
        return struct_at(live, identifier).bind(lambda found: Option.of_optional(try_downcast(kind, found)))

    graph, interface = (desired.graph, desired.interface) if isinstance(desired, NodeGroup) else (desired, Interface())
    placed = graph.placed
    held = {key: node for key, node in first.items() if key in placed and isinstance(node, placed[key][0].kind)}
    owners = {node: key for key, node in held.items()}
    pointed = {
        (tree, content.group.key): content.group
        for key, live in held.items()
        for content in placed[key][0].content.to_list()
        if isinstance(content, Nested)
        for tree in referenced(live, "node_tree", bpy.types.NodeTree).filter(lambda tree: Mark.read(identity, tree) == Some(Mark(key=content.group.key))).to_list()
    }
    current = {pair: changes(group, pair[0], identity).map(lambda rows: rows == Changes()) for pair, group in pointed.items()}

    def holding(content: Content, live: bpy.types.Node) -> Outcome[bool]:
        match content:
            case Ramp() as ramp:
                return _ramp(live).map(ramp.holds)
            case CurvePoints():
                desired = _tree(content)
                return (
                    accumulated(Descriptor.resolve(type(live), root, bpy.types.PointerProperty) for root in desired.roots)
                    .bind(lambda roots: Snapshot.capture(live, Some(roots)))
                    .map(
                        lambda held: all(
                            key in held.tree.owners and held.tree.owners[key].children == owner.children and all(not differing(struct, owner.values) for struct in struct_at(live, key).to_list())
                            for key, owner in desired.owners.items()
                        )
                    )
                )
            case Generated(ownership=ownership, content=hashed):
                return Ok(
                    referenced(live, "image", bpy.types.Image)
                    .map(lambda image: Mark.read(identity, image) == Some(Mark(key=ownership.key, content=Some(hashed))))
                    .default_with(lambda: Mark.read(identity, live).map(lambda mark: mark.content == Some(hashed)).default_value(False))
                )
            case Loaded(path=path, tag=tag, frames=frames):
                return (
                    frames
                    .map(lambda want: _framed(live).map(lambda holder: not differing(holder, msgspec.structs.asdict(want))))
                    .default_value(Ok(True))
                    .map(
                        lambda framed: (
                            framed
                            and referenced(live, "image", bpy.types.Image)
                            .map(lambda image: bpy.path.abspath(image.filepath, library=image.library) == path and (image.source == "SEQUENCE") == tag.sequence)
                            .default_value(False)
                        )
                    )
                )
            case Nested(group=group):
                return referenced(live, "node_tree", bpy.types.NodeTree).bind(lambda tree: Option.of_optional(current.get((tree, group.key)))).default_value(Ok(False))
            case MenuItems() as menu:
                return _narrowed(live, bpy.types.GeometryNodeMenuSwitch, "enum_items").map(
                    lambda switch: tuple((each.name, each.description) for each in switch.enum_items) == tuple((each.name, each.description) for each in menu.items)
                )
            case FileItems() as files:
                return _narrowed(live, bpy.types.CompositorNodeOutputFile, "file_output_items").map(
                    lambda output: (
                        len(output.file_output_items) == len(files.items)
                        and all(held.name == item.name and held.socket_type == item.kind and not differing(held, item.members) for held, item in zip(output.file_output_items, files.items))
                    )
                )

    def stale(
        sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str | int, Value], keyed: Mapping[str, str], interface: Option[dict[str, tuple[str, type[bpy.types.NodeSocket]]]]
    ) -> dict[str | int, Value]:
        return {
            address: value
            for address, value in values.items()
            if not any(
                not differing(socket, {"default_value": value})
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
                .map(lambda wanted: holding(wanted, live).map(lambda now: Nothing if now else Some(wanted)))
                .default_value(Ok(Nothing))
                .map(lambda content: Altered(key=key, members=differing(live, node.members), content=content, parent=Nothing if framed else Some(parent)))
            )

        fresh = Altered(key=key, members=node.members, content=node.content, parent=parent.map(Some))
        return (drawn(held[key]) if key in held else Ok(fresh)).map(lambda row: Some(row).filter(lambda row: row != Altered(key=key)))

    sockets = {key: keyed.sockets[key] for key in held}

    def linked(
        alterations: tuple[Altered, ...], rewritten: frozenset[Key]
    ) -> tuple[tuple[Disconnected, ...], tuple[Connected | Reordered, ...], dict[Key, dict[str, tuple[str, type[bpy.types.NodeSocket]]]]]:

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

        nested = {
            key: group.ownership.found(identity).map(lambda found: surviving(found, group.interface)).default_value({}) if group.single else {}
            for change in alterations
            if change.key in held
            for content in change.content.to_list()
            if isinstance(content, Nested)
            for key, group in ((change.key, content.group),)
        }
        interfaces = {**{key: surviving(tree, interface) for key, live in held.items() if isinstance(live, (bpy.types.NodeGroupInput, bpy.types.NodeGroupOutput))}, **nested}
        keys = {node: {identifier: key for key, identifier in sockets[node].items()} for node in interfaces}

        def retained(end: End) -> bool:
            match end.socket:
                case str(socket) if end.node in interfaces:
                    return any(identifier == socket and keys[end.node].get(socket, socket) in {key, identifier} for key, (identifier, _) in interfaces[end.node].items())
                case _:
                    return end.node not in interfaces

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
            for link in graph.links
        )
        kept_links = frozenset(found for _, held_link in matched for found in held_link.to_list())
        desired_links: dict[End, tuple[End, ...]] = {}
        for declared in graph.links:
            desired_links[declared.target] = (*desired_links.get(declared.target, ()), declared.source)
        return (
            tuple(Disconnected(link=link) for link in existing if link not in kept_links),
            (
                *(Connected(link=link) for link, found in reversed(matched) if found.is_none()),
                *(
                    Reordered(target=target, sources=sources)
                    for target, sources in desired_links.items()
                    if len(sources) > 1 and present.get(addressed(target, output=False), ()) != tuple(addressed(source, output=True) for source in sources)
                ),
            ),
            interfaces,
        )

    def assembled(rows: tuple[Option[Altered], ...]) -> Changes:
        alterations = tuple(row for option in rows for row in option.to_list())
        rewritten = frozenset(row.key for row in alterations if row.members)
        (withdrawn, declared, restated), (disconnected, connected, interfaces) = interfaced(tree, interface, name), linked(alterations, rewritten)
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
            structure=(
                *(Configured(members=members) for members in (differing(tree, graph.members),) if members),
                *withdrawn,
                *declared,
                *(Removed(name=node.name) for node in first.values() if node not in owners),
                *(Added(key=key, kind=node.kind) for key, (node, _) in placed.items() if key not in held),
            ),
            alterations=(*alterations, *disconnected),
            links=connected,
            values=tuple(row for row in values if row.inputs or row.outputs),
            restatements=restated,
        )

    return accumulated(altered(key, node, parent) for key, (node, parent) in placed.items()).map(assembled)


def _tree(content: CurvePoints) -> Tree[Value, RnaPath]:
    """Curve point settings by node RNA path, collections owning items as `Snapshot.capture` reads them."""
    curves: Mapping[RnaPath, Points] = {("mapping", "curves", index): points for index, points in enumerate(content.maps)}
    keys: Mapping[RnaPath, tuple[RnaPath, ...]] = {curve: tuple((*curve, "points", index) for index in range(len(points))) for curve, points in curves.items()}
    bounds = content.clip.map(lambda span: dict(zip(("clip_min_x", "clip_min_y", "clip_max_x", "clip_max_y"), span))).default_value({})
    return Tree(
        roots=(("mapping",),),
        rows=(
            (("mapping",), Owner(values={"use_clip": content.clipped, "extend": "EXTRAPOLATED", **bounds}, children=(("mapping", "curves"),))),
            (("mapping", "curves"), Owner(values={}, children=tuple(curves))),
            *((curve, Owner(values={}, children=((*curve, "points"),))) for curve in curves),
            *(((*curve, "points"), Owner(values={}, children=keys[curve])) for curve in curves),
            *((key, Owner[Value, RnaPath](values={"location": point, "handle_type": content.handle})) for curve, points in curves.items() for key, point in zip(keys[curve], points)),
        ),
    )


def _ramp(live: bpy.types.Node) -> Outcome[bpy.types.ColorRamp]:
    """Node's `color_ramp`, else `absent` fault naming it."""
    return (
        struct_at(live, ("color_ramp",))
        .bind(lambda held: Option.of_optional(try_downcast(bpy.types.ColorRamp, held)))
        .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(live), member="color_ramp"))
    )


def _framed(live: bpy.types.Node) -> Outcome[bpy.types.ImageUser | bpy.types.CompositorNodeImage]:
    """Frame window holder, compositor image node itself or shader image node `image_user`."""
    match live:
        case bpy.types.CompositorNodeImage():
            return Ok(live)
        case bpy.types.ShaderNodeTexImage() | bpy.types.ShaderNodeTexEnvironment():
            return Ok(live.image_user)
        case _:
            return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(live), member="image_user"),))


def _narrowed[N: bpy.types.Node](live: bpy.types.Node, kind: type[N], member: str) -> Outcome[N]:
    """Node as class declaring member, else `absent` fault naming member."""
    return Option.of_optional(try_downcast(kind, live)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(live), member=member),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Added", "Altered", "Changes", "Configured", "Connected", "Disconnected", "Keyed", "Removed", "Reordered", "Valued", "applied", "changes", "made", "owned", "reconciled", "stored", "valued"]
