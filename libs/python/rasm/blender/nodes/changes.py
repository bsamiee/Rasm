"""Reconciliation of desired graphs onto native trees: change rows, their phased writes, owned groups, and node content."""

from collections import deque
from collections.abc import Callable, Mapping
import functools
import itertools

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from rasm.blender.data.ids.images import generated, loaded
from rasm.blender.data.ids.ownership import earliest, Mark
from rasm.blender.data.rna.descriptors import assigned, Descriptor, differing, holds, struct_at, Value, written
from rasm.blender.data.rna.snapshots import Snapshot
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from rasm.blender.nodes.graphs import Content, CurvePoints, End, FileItems, Generated, Graph, Link, Loaded, Nested, Node, NodeGroup
from rasm.blender.nodes.interface import addresses, Declared, Interface, interfaced, kept, MenuItems, Restated, revised, slot, Withdrawn
from rasm.blender.nodes.trees import created, grouped, joined, ordered, socket_at
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Ramp
from rasm.rendering.edits import Owner, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type Structural = Configured | Withdrawn | Declared | Removed | Added
type Alteration = Altered | Disconnected
type TreeChange = Structural | Alteration | Connected | Reordered | Valued | Restated

# --- [MODELS] ---------------------------------------------------------------------------


class Configured(msgspec.Struct, frozen=True, kw_only=True):
    """Tree members differing from the graph."""

    members: dict[Descriptor, Value]


class Removed(msgspec.Struct, frozen=True, kw_only=True):
    """First holder of a mark whose key the desired graph lacks, named by its node name."""

    name: str


class Added(msgspec.Struct, frozen=True, kw_only=True):
    """Node to create for a key."""

    key: Key
    kind: type[bpy.types.Node]


class Altered(msgspec.Struct, frozen=True, kw_only=True):
    """Node's differing desired parts."""

    key: Key
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    content: Option[Content] = Nothing
    parent: Option[Option[Key]] = Nothing


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """Node's differing socket values by address."""

    key: Key
    inputs: dict[str, Value] = msgspec.field(default_factory=dict)
    outputs: dict[str, Value] = msgspec.field(default_factory=dict)


class Disconnected(msgspec.Struct, frozen=True, kw_only=True):
    """Owned link the graph lacks."""

    link: bpy.types.NodeLink


class Connected(msgspec.Struct, frozen=True, kw_only=True):
    """Desired link the tree lacks."""

    link: Link


class Reordered(msgspec.Struct, frozen=True, kw_only=True):
    """Desired top-to-bottom order of owned connections at one input."""

    target: End
    sources: tuple[End, ...]


class Changes(msgspec.Struct, frozen=True, kw_only=True):
    """Reconcile rows in apply phases, `Changes()` for a current tree."""

    structure: tuple[Structural, ...] = ()
    alterations: tuple[Alteration, ...] = ()
    links: tuple[Connected | Reordered, ...] = ()
    values: tuple[Valued, ...] = ()
    restatements: tuple[Restated, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


def changes(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> BlenderResult[Changes]:
    """Reads a tree against a desired graph, writing nothing.

    Args:
        desired: Graph of an embedded tree, or a group whose graph and interface the tree declares.
        tree: Owned tree to read.
        identity: Extension whose marks find owned nodes.

    Returns:
        `Ok` of the rows an apply writes, `Changes()` for a current tree; `Error` of `absent` or `choice` for a content member the node's class lacks, or of `register` for a refused stored-key name.
    """
    graph, interface = (desired.graph, desired.interface) if isinstance(desired, NodeGroup) else (desired, Interface())
    placed = graph.placed
    first = owned(tree, identity)
    held = {key: node for key, node in first.items() if key in placed and isinstance(node, placed[key][0].kind)}
    owners = {node: key for key, node in held.items()}
    pointed = {
        (tree, content.group.key): content.group
        for key, live in held.items()
        for content in placed[key][0].content.to_list()
        if isinstance(content, Nested)
        for tree in _pointed(live, "node_tree", bpy.types.NodeTree).filter(lambda tree: Mark.read(identity, tree) == Some(Mark(key=content.group.key))).to_list()
    }
    current = {pair: changes(group, pair[0], identity).map(lambda rows: rows == Changes()) for pair, group in pointed.items()}

    def altered(key: Key, node: Node, parent: Option[Key]) -> BlenderResult[Option[Altered]]:
        def drawn(live: bpy.types.Node) -> BlenderResult[Altered]:
            framed = parent.bind(lambda frame: Option.of_optional(held.get(frame))).to_optional() == live.parent
            return (
                node.content
                .map(lambda wanted: _current(wanted, live, identity, current).map(lambda now: Nothing if now else Some(wanted)))
                .default_value(Ok(Nothing))
                .map(lambda content: Altered(key=key, members=differing(node.members, live), content=content, parent=Nothing if framed else Some(parent)))
            )

        fresh = Altered(key=key, members=node.members, content=node.content, parent=parent.map(Some))
        return (drawn(held[key]) if key in held else Ok(fresh)).map(lambda row: Some(row).filter(lambda row: row != Altered(key=key)))

    def linked(
        name: str, keyed: Mapping[Key, dict[str, str]], rows: tuple[Option[Altered], ...]
    ) -> tuple[tuple[Disconnected, ...], tuple[Connected | Reordered, ...], dict[Key, dict[str, tuple[str, type[bpy.types.NodeSocket]]]]]:
        def addressed(endpoint: End) -> End:
            return msgspec.structs.replace(endpoint, socket=keyed[endpoint.node].get(endpoint.socket, endpoint.socket)) if endpoint.node in keyed else endpoint

        def surviving(group: bpy.types.NodeTree, face: Interface) -> dict[str, tuple[str, type[bpy.types.NodeSocket]]]:
            items, entries = kept(group, face, name), {key: entry for key, entry, _, _ in face.walked()}
            return {key: (address, entry[0].kind if isinstance(entry := entries[key], tuple) else bpy.types.NodeSocketBool) for key, address in addresses(group, name).items() if key in items}

        nested = {
            key: Option.of_optional(earliest(group.ownership.held(identity)).get(Mark(key=group.key))).map(lambda found: surviving(found, group.interface)).default_value({}) if group.single else {}
            for change in (change for option in rows for change in option.to_list())
            if change.key in held
            for content in change.content.to_list()
            if isinstance(content, Nested)
            for key, group in ((change.key, content.group),)
        }
        interfaces = {**{key: surviving(tree, interface) for key, live in held.items() if isinstance(live, (bpy.types.NodeGroupInput, bpy.types.NodeGroupOutput))}, **nested}

        def retained(end: End) -> bool:
            address = {identifier: key for key, identifier in keyed[end.node].items()}.get(end.socket, end.socket) if end.node in interfaces else end.socket
            return end.node not in interfaces or any(identifier == end.socket and address in {key, identifier} for key, (identifier, _) in interfaces[end.node].items())

        sockets = {socket: End(node=key, socket=socket.identifier) for key, live in held.items() for socket in (*live.inputs, *live.outputs)}
        endpoints = {
            link: (sockets[link.from_socket], sockets[link.to_socket])  # ty: ignore[invalid-argument-type]
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
        matched = tuple((link, Option.of_optional(available.get((addressed(link.source), addressed(link.target)))).filter(bool).map(lambda candidates: candidates.popleft())) for link in graph.links)
        kept_links = frozenset(found for _, held_link in matched for found in held_link.to_list())
        desired_links: dict[End, tuple[End, ...]] = {}
        for declared in graph.links:
            desired_links[declared.target] = (*desired_links.get(declared.target, ()), declared.source)
        return (
            tuple(Disconnected(link=link) for link in existing if link not in kept_links),
            (
                *(Connected(link=link) for link, found in reversed(matched) if found.is_none()),
                *(Reordered(target=target, sources=sources) for target, sources in desired_links.items() if len(sources) > 1 and present.get(addressed(target), ()) != tuple(map(addressed, sources))),
            ),
            interfaces,
        )

    def assembled(name: str, rows: tuple[Option[Altered], ...]) -> Changes:
        keyed = {key: _keyed(live, name) for key, live in held.items()}
        (withdrawn, declared, restated), (disconnected, connected, interfaces) = interfaced(tree, interface, name), linked(name, keyed, rows)
        values = tuple(
            Valued(
                key=key,
                inputs=_stale(held[key].inputs, node.inputs, keyed[key], future := Option.of_optional(interfaces.get(key))),
                outputs=_stale(held[key].outputs, node.outputs, keyed[key], future),
            )
            if key in held
            else Valued(key=key, inputs=node.inputs, outputs=node.outputs)
            for key, (node, _) in placed.items()
        )
        return Changes(
            structure=(
                *(Configured(members=members) for members in (differing(graph.members, tree),) if members),
                *withdrawn,
                *declared,
                *(Removed(name=node.name) for node in first.values() if node not in owners),
                *(Added(key=key, kind=node.kind) for key, (node, _) in placed.items() if key not in held),
            ),
            alterations=(*(row for option in rows for row in option.to_list()), *disconnected),
            links=connected,
            values=tuple(row for row in values if row.inputs or row.outputs),
            restatements=restated,
        )

    return slot(identity).bind(lambda name: accumulated(altered(key, node, parent) for key, (node, parent) in placed.items()).map(lambda rows: assembled(name, rows)))


def reconciled(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> BlenderResult[Changes]:
    """Writes the rows `changes` answers, phase by phase.

    Args:
        desired: Graph of an embedded tree, or a group whose graph and interface the tree declares.
        tree: Owned tree to write.
        identity: Extension whose marks find and mark owned nodes.

    Returns:
        `Ok` of the rows written; `Error` of the faults `changes` answers, or of every `edit`, `absent`, or `choice` fault a phase's writes answer.
    """

    def phased(name: str, rows: Changes) -> BlenderResult[Changes]:
        def wrote(phase: tuple[TreeChange, ...]) -> BlenderResult[None]:
            nodes = owned(tree, identity)
            return accumulated(applied(row, tree, identity, name, nodes) for row in phase).map(lambda _: None)

        phases: tuple[tuple[TreeChange, ...], ...] = (
            tuple(row for row in rows.alterations if isinstance(row, Disconnected)),
            rows.structure,
            tuple(Restated(key=row.key, kind=row.kind, place=row.place) for row in rows.restatements if row.kind.is_some() or row.place.is_some()),
            tuple(row for row in rows.alterations if isinstance(row, Altered)),
            tuple(row for row in rows.links if isinstance(row, Connected)),
            tuple(row for row in rows.links if isinstance(row, Reordered)),
            rows.values,
            tuple(msgspec.structs.replace(row, kind=Nothing, place=Nothing) for row in rows.restatements),
        )
        return functools.reduce(lambda done, phase: done.bind(lambda _: wrote(phase)), phases, Result[None, BlenderFaults].Ok(None)).map(lambda _: rows)

    return slot(identity).bind(lambda name: changes(desired, tree, identity).bind(lambda rows: phased(name, rows)))


def owned(tree: bpy.types.NodeTree, identity: Identity) -> dict[Key, bpy.types.Node]:
    """Marked nodes of a tree by key, the first in tree order holding a key, a later holder being a copy the user made and keeps."""
    return dict(reversed(tuple((mark.key, node) for node in tree.nodes for mark in Mark.read(identity, node).to_list())))


def applied(row: TreeChange, tree: bpy.types.NodeTree, identity: Identity, slot: str, nodes: Mapping[Key, bpy.types.Node]) -> BlenderResult[None]:
    """Writes one change row onto a tree.

    Args:
        row: Change row to write.
        tree: Owned tree the row names.
        identity: Extension whose marks find and mark owned nodes.
        slot: ID property name of the stored item keys.
        nodes: Marked nodes by key as `owned` reads them before the row's phase.

    Returns:
        `Ok` once written; `Error` of `edit` for a refused node type or write, of `absent` for an unresolved node, item, or socket, or of `choice` for a refused enum identifier.
    """

    def node(key: Key) -> BlenderResult[bpy.types.Node]:
        return Option.of_optional(nodes.get(key)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=key),))

    def output(end: End) -> BlenderResult[bpy.types.NodeSocket]:
        return node(end.node).bind(lambda live: socket_at(live.outputs, _keyed(live, slot).get(end.socket, end.socket)))

    def entering(end: End) -> BlenderResult[bpy.types.NodeSocket]:
        return node(end.node).bind(lambda live: socket_at(live.inputs, _keyed(live, slot).get(end.socket, end.socket)))

    def altered(change: Altered) -> BlenderResult[None]:
        def framed(live: bpy.types.Node, frame: Option[Key]) -> BlenderResult[None]:
            def parented(found: Option[bpy.types.Node]) -> None:
                live.parent = found.to_optional()

            return frame.map(lambda key: node(key).map(Some)).default_value(Ok(Nothing)).map(parented)

        return node(change.key).bind(
            lambda live: (
                change.content
                .map(lambda content: stored(content, live, identity))
                .default_value(Ok(None))
                .bind(lambda _: assigned(change.members, live))
                .bind(lambda _: change.parent.map(lambda frame: framed(live, frame)).default_value(Ok(None)))
            )
        )

    def reordered(target: bpy.types.NodeSocket, sources: tuple[bpy.types.NodeSocket, ...]) -> BlenderResult[None]:
        owned = tuple(link for link in target.links if Mark.read(identity, link.from_node).is_some())  # type: ignore[union-attr, arg-type]  # ty: ignore[not-iterable, invalid-argument-type]
        return accumulated(
            Option.of_optional(next(itertools.islice((link for link in owned if link.from_socket == source), sources[:index].count(source), None), None)).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(target), key=repr(source)),)
            )
            for index, source in enumerate(sources)
        ).map(lambda desired: ordered(target, desired))

    match row:
        case Configured(members=members):
            return assigned(members, tree)
        case Withdrawn() | Declared() | Restated():
            return revised(row, tree, slot)
        case Removed(name=name):
            return Option.of_optional(tree.nodes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(tree.nodes), key=name),)).map(tree.nodes.remove)
        case Added(key=key, kind=kind):
            return created(tree, kind).bind(lambda made: Mark(key=key).write(identity, made))
        case Altered():
            return altered(row)
        case Disconnected(link=link):
            tree.links.remove(link)
            return Ok(None)
        case Connected(link=link):
            return paired(output(link.source), entering(link.target)).bind(lambda pair: joined(tree, *pair, verify_limits=False)).map(lambda _: None)
        case Reordered(target=target, sources=sources):
            return paired(entering(target), accumulated(map(output, sources))).bind(lambda pair: reordered(*pair))
        case Valued(key=key, inputs=inputs, outputs=outputs):
            return node(key).bind(lambda live: paired(valued(live, live.inputs, inputs, slot), valued(live, live.outputs, outputs, slot))).map(lambda _: None)


def made(group: NodeGroup, identity: Identity, *, of: Option[bpy.types.ID] = Nothing) -> BlenderResult[bpy.types.NodeTree]:
    """Reuses or builds a group's tree and reconciles it.

    Args:
        group: Owned group to make.
        identity: Extension whose marks find and mark the group and its nodes.
        of: Data-block the group serves, so a single group of one scene stays that scene's.

    Returns:
        `Ok` of the group's tree; `Error` of the faults its reconcile answers.
    """
    return group.ownership.made(identity, lambda: Ok(bpy.data.node_groups.new(group.name, group.graph.kind.bl_rna.identifier)), of=of).bind(  # type: ignore[arg-type,attr-defined]  # ty: ignore[invalid-argument-type,unresolved-attribute]
        lambda tree: reconciled(group, tree, identity).map(lambda _: tree)
    )


def stored(content: Content, live: bpy.types.Node, identity: Identity) -> BlenderResult[None]:
    """Writes declared content onto an acquired native node.

    Args:
        content: Ramp, curves, image, group, menu, or file-output storage to write.
        live: Native node retained by its caller's resource scope.
        identity: Extension owning referenced groups, images, and content marks.

    Returns:
        `Ok` after writing; native snapshot, image, group, and RNA faults remain typed errors.
    """
    match content:
        case Ramp() | CurvePoints():
            return Snapshot(struct=live.bl_rna.identifier, tree=_tree(content)).apply(live).outcome  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case Generated(ownership=ownership, content=hashed, name=name, plane=plane, tag=tag):
            return generated(ownership, identity, name, hashed, plane, tag).bind(
                lambda image: written(live, "image", image.to_optional()).bind(
                    lambda _: (
                        Mark
                        .read(identity, live)
                        .map(lambda mark: msgspec.structs.replace(mark, content=Some(hashed) if image.is_none() else Nothing).write(identity, live))
                        .default_value(Error((BlenderAbsentFault(owner=repr(live), key=identity.manifest.id),)))
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
                .map(lambda switch: _ordered(switch.enum_items, tuple(each.name for each in menu.items), switch.enum_items.new, lambda name: switch.enum_items.remove(switch.enum_items[name])))
                .bind(lambda held: accumulated(written(each, "description", item.description) for each, item in zip(held, menu.items)).map(lambda _: None))
            )
        case FileItems() as files:
            kinds = {item.name: item.kind for item in files.items}
            return (
                _narrowed(live, bpy.types.CompositorNodeOutputFile, "file_output_items")
                .map(
                    lambda output: _ordered(
                        output.file_output_items,
                        tuple(kinds),
                        lambda name: output.file_output_items.new(kinds[name], name),
                        lambda name: output.file_output_items.remove(output.file_output_items[name]),
                    )
                )
                .bind(lambda held: accumulated(paired(written(each, "socket_type", item.kind), assigned(item.members, each)) for each, item in zip(held, files.items)).map(lambda _: None))
            )


def valued(live: bpy.types.Node, sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str, Value], slot: str) -> BlenderResult[None]:
    """Writes socket defaults by native identifier or declared interface key."""
    keyed = _keyed(live, slot)
    return accumulated(socket_at(sockets, keyed.get(address, address)).bind(lambda socket: written(socket, "default_value", value)) for address, value in values.items()).map(lambda _: None)


def _tree(content: Ramp | CurvePoints) -> Tree[Value]:
    """Lowers a ramp or curve points into the settings tree its node stores, keyed by RNA path from the node, each collection an owner of its items as `Snapshot.capture` reads it."""
    match content:
        case Ramp(stops=stops, interpolation=interpolation, mode=mode, hue_path=hue_path):
            elements = tuple(f"color_ramp.elements[{index}]" for index in range(len(stops)))
            return Tree(
                roots=("color_ramp",),
                owners={
                    "color_ramp": Owner(values={"interpolation": interpolation.name, "color_mode": mode.name, "hue_interpolation": hue_path.name}, children=("color_ramp.elements",)),
                    "color_ramp.elements": Owner(values={}, children=elements),
                    **{
                        key: Owner(values={"position": stop.position, "color": (stop.color.red, stop.color.green, stop.color.blue, 1.0)})
                        for key, stop in zip(elements, sorted(stops, key=lambda stop: stop.position))
                    },
                },
            )
        case CurvePoints(clip=clip, maps=maps, handle=handle, clipped=clipped):
            curves = {f"mapping.curves[{index}]": points for index, points in enumerate(maps)}
            keys = {curve: tuple(f"{curve}.points[{index}]" for index in range(len(points))) for curve, points in curves.items()}
            bounds = clip.map(lambda span: dict(zip(("clip_min_x", "clip_min_y", "clip_max_x", "clip_max_y"), span))).default_value({})
            return Tree(
                roots=("mapping",),
                owners={
                    "mapping": Owner(values={"use_clip": clipped, "extend": "EXTRAPOLATED", **bounds}, children=("mapping.curves",)),
                    "mapping.curves": Owner(values={}, children=tuple(curves)),
                    **{curve: Owner(values={}, children=(f"{curve}.points",)) for curve in curves},
                    **{f"{curve}.points": Owner(values={}, children=keys[curve]) for curve in curves},
                    **{key: Owner(values={"location": point, "handle_type": handle}) for curve, points in curves.items() for key, point in zip(keys[curve], points)},
                },
            )


def _current(content: Content, live: bpy.types.Node, identity: Identity, nested: Mapping[tuple[bpy.types.NodeTree, Key], BlenderResult[bool]]) -> BlenderResult[bool]:
    """Whether a node holds a content, one arm per content case, a nested group's currency read once per owned tree and group key."""
    match content:
        case Ramp() | CurvePoints():
            desired = _tree(content)
            return (
                accumulated(Descriptor.resolve(type(live), root, bpy.types.PointerProperty) for root in desired.roots)
                .bind(lambda roots: Snapshot.capture(live, Some(roots)))
                .map(
                    lambda held: all(
                        key in held.tree.owners
                        and held.tree.owners[key].children == owner.children
                        and all(holds(struct, name, value) for struct in struct_at(live, key).to_list() for name, value in owner.values.items())
                        for key, owner in desired.owners.items()
                    )
                )
            )
        case Generated(ownership=ownership, content=hashed):
            return Ok(
                _pointed(live, "image", bpy.types.Image)
                .map(lambda image: Mark.read(identity, image) == Some(Mark(key=ownership.key, content=Some(hashed))))
                .default_with(lambda: Mark.read(identity, live).map(lambda mark: mark.content == Some(hashed)).default_value(False))
            )
        case Loaded(path=path, tag=tag, frames=frames):
            return (
                frames
                .map(lambda want: _framed(live).map(lambda holder: all(holds(holder, name, value) for name, value in msgspec.structs.asdict(want).items())))
                .default_value(Ok(True))
                .map(
                    lambda framed: (
                        framed
                        and _pointed(live, "image", bpy.types.Image)
                        .map(lambda image: bpy.path.abspath(image.filepath, library=image.library) == path and (image.source == "SEQUENCE") == tag.sequence)
                        .default_value(False)
                    )
                )
            )
        case Nested(group=group):
            return _pointed(live, "node_tree", bpy.types.NodeTree).bind(lambda tree: Option.of_optional(nested.get((tree, group.key)))).default_value(Ok(False))
        case MenuItems() as menu:
            return _narrowed(live, bpy.types.GeometryNodeMenuSwitch, "enum_items").map(
                lambda switch: tuple((each.name, each.description) for each in switch.enum_items) == tuple((each.name, each.description) for each in menu.items)
            )
        case FileItems() as files:
            return _narrowed(live, bpy.types.CompositorNodeOutputFile, "file_output_items").map(
                lambda output: (
                    len(output.file_output_items) == len(files.items)
                    and all(held.name == item.name and held.socket_type == item.kind and not differing(item.members, held) for held, item in zip(output.file_output_items, files.items))
                )
            )


def _ordered(
    items: bpy.types.NodeMenuSwitchItems | bpy.types.NodeCompositorFileOutputItems, names: tuple[str, ...], made: Callable[[str], object], removed: Callable[[str], object]
) -> tuple[bpy.types.NodeEnumItem | bpy.types.NodeCompositorFileOutputItem, ...]:
    """Keeps a named item collection to the given names in order, removing the rest and adding the missing, so a held item keeps its name and stored values."""
    for name in tuple(item.name for item in items if item.name not in names):
        removed(name)
    for name in [name for name in names if items.get(name) is None]:
        made(name)
    for index, name in enumerate(names):
        items.move(tuple(items).index(items[name]), index)
    return tuple(items)


def _stale(
    sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: dict[str, Value], keyed: Mapping[str, str], interface: Option[dict[str, tuple[str, type[bpy.types.NodeSocket]]]]
) -> dict[str, Value]:
    """Socket values by address, resolved through the node's keyed identifiers, whose live socket is absent or holds another value."""
    return {
        address: value
        for address, value in values.items()
        if not any(
            holds(socket, "default_value", value)
            and interface.map(
                lambda future: any(identifier == socket.identifier and address in {key, identifier} and isinstance(socket, kind) for key, (identifier, kind) in future.items())
            ).default_value(True)
            for socket in socket_at(sockets, keyed.get(address, address)).to_option().to_list()
        )
    }


def _pointed[T: bpy.types.ID](live: bpy.types.Node, identifier: str, kind: type[T]) -> Option[T]:
    """Data-block a node's pointer member names, `Nothing` for an empty pointer or a member the node lacks."""
    return struct_at(live, identifier).bind(lambda found: Some(found) if isinstance(found, kind) else Nothing)


def _framed(live: bpy.types.Node) -> BlenderResult[bpy.types.ImageUser | bpy.types.CompositorNodeImage]:
    """Struct holding a node's frame window: a compositor image node itself, a shader image node's `image_user`."""
    match live:
        case bpy.types.CompositorNodeImage():
            return Ok(live)
        case bpy.types.ShaderNodeTexImage() | bpy.types.ShaderNodeTexEnvironment():
            return Ok(live.image_user)
        case _:
            return Error((BlenderAbsentFault(owner=repr(live), key="image_user"),))


def _narrowed[N: bpy.types.Node](live: bpy.types.Node, kind: type[N], member: str) -> BlenderResult[N]:
    """Node as the class holding a member, `absent` holding the member for any other class."""
    return Ok(live) if isinstance(live, kind) else Error((BlenderAbsentFault(owner=repr(live), key=member),))


def _keyed(node: bpy.types.Node, slot: str) -> dict[str, str]:
    """Address to socket identifier: interface item keys on a group boundary or group node, item names on a Menu Switch or File Output, empty for any other node."""
    match node:
        case bpy.types.NodeGroupInput() | bpy.types.NodeGroupOutput():
            return addresses(node.id_data, slot)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        case bpy.types.CompositorNodeGroup() | bpy.types.ShaderNodeGroup() | bpy.types.GeometryNodeGroup():
            return Option.of_optional(node.node_tree).map(lambda tree: addresses(tree, slot)).default_value({})
        case bpy.types.GeometryNodeMenuSwitch():
            fixed = {socket.identifier for socket in (*node.inputs, *node.outputs)}  # type: ignore[misc]  # ty: ignore[not-iterable]
            return {item.name: socket.identifier for item, socket in zip(node.enum_items, tuple(node.outputs)[1:]) if item.name not in fixed}  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        case bpy.types.CompositorNodeOutputFile():
            return {item.name: socket.identifier for item, socket in zip(node.file_output_items, node.inputs)}  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        case _:
            return {}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Added",
    "Alteration",
    "Altered",
    "Changes",
    "Configured",
    "Connected",
    "Disconnected",
    "Removed",
    "Reordered",
    "Structural",
    "TreeChange",
    "Valued",
    "applied",
    "changes",
    "made",
    "owned",
    "reconciled",
    "stored",
    "valued",
]
