"""Desired graph reconciliation onto node trees, owned groups, and node content."""

from collections import deque
from collections.abc import Callable, Mapping
import functools
import itertools

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.data.ids.images import generated, loaded
from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.descriptors import assigned, Descriptor, differing, holds, struct_at, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Content, CurvePoints, End, FileItems, Generated, Graph, Link, Loaded, Nested, Node, NodeGroup
from blender.bpy.nodes.interface import addresses, Declared, Interface, interfaced, kept, MenuItems, Restated, revised, slot, Withdrawn
from blender.bpy.nodes.trees import created, grouped, joined, ordered, socket_at
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Ramp
from blender.rendering.edits import Owner, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type Structural = Configured | Withdrawn | Declared | Removed | Added
type Alteration = Altered | Disconnected
type TreeChange = Structural | Alteration | Connected | Reordered | Valued | Restated

# --- [MODELS] ---------------------------------------------------------------------------


class Configured(msgspec.Struct, frozen=True, kw_only=True):
    """Graph members tree lacks."""

    members: dict[Descriptor, Value]


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
    members: dict[Descriptor, Value] = msgspec.field(default_factory=dict)
    content: Option[Content] = Nothing
    parent: Option[Option[Key]] = Nothing


class Valued(msgspec.Struct, frozen=True, kw_only=True):
    """Socket values live node lacks, by address."""

    key: Key
    inputs: dict[str, Value] = msgspec.field(default_factory=dict)
    outputs: dict[str, Value] = msgspec.field(default_factory=dict)


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

    structure: tuple[Structural, ...] = ()
    alterations: tuple[Alteration, ...] = ()
    links: tuple[Connected | Reordered, ...] = ()
    values: tuple[Valued, ...] = ()
    restatements: tuple[Restated, ...] = ()


# --- [OPERATIONS] -----------------------------------------------------------------------


def changes(desired: Graph | NodeGroup, tree: bpy.types.NodeTree, identity: Identity) -> BlenderResult[Changes]:
    """Reads tree against desired graph, writing nothing.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to read.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows. `Error` of `absent` or `choice` for content member node class lacks, `register` for refused attachment name.
    """

    def referenced[T: bpy.types.ID](live: bpy.types.Node, identifier: str, kind: type[T]) -> Option[T]:
        return struct_at(live, identifier).bind(lambda found: Some(found) if isinstance(found, kind) else Nothing)

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
        for tree in referenced(live, "node_tree", bpy.types.NodeTree).filter(lambda tree: Mark.read(identity, tree) == Some(Mark(key=content.group.key))).to_list()
    }
    current = {pair: changes(group, pair[0], identity).map(lambda rows: rows == Changes()) for pair, group in pointed.items()}

    def holding(content: Content, live: bpy.types.Node) -> BlenderResult[bool]:
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
                    referenced(live, "image", bpy.types.Image)
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
                        and all(held.name == item.name and held.socket_type == item.kind and not differing(item.members, held) for held, item in zip(output.file_output_items, files.items))
                    )
                )

    def stale(
        sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: dict[str, Value], keyed: Mapping[str, str], interface: Option[dict[str, tuple[str, type[bpy.types.NodeSocket]]]]
    ) -> dict[str, Value]:
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

    def altered(key: Key, node: Node, parent: Option[Key]) -> BlenderResult[Option[Altered]]:
        def drawn(live: bpy.types.Node) -> BlenderResult[Altered]:
            framed = parent.bind(lambda frame: Option.of_optional(held.get(frame))).to_optional() == live.parent
            return (
                node.content
                .map(lambda wanted: holding(wanted, live).map(lambda now: Nothing if now else Some(wanted)))
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
            key: group.ownership.found(identity).map(lambda found: surviving(found, group.interface)).default_value({}) if group.single else {}
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
                key=key, inputs=stale(held[key].inputs, node.inputs, keyed[key], future := Option.of_optional(interfaces.get(key))), outputs=stale(held[key].outputs, node.outputs, keyed[key], future)
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
    """Writes `changes` rows phase by phase.

    Args:
        desired: Embedded tree graph, or group with graph and interface.
        tree: Owned tree to write.
        identity: Extension marking owned nodes.

    Returns:
        `Ok` of rows written. `Error` of `changes` faults, or every `edit`, `absent`, or `choice` fault from phase writes.
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
    """Marked nodes by key, first in tree order over later user copies."""
    return dict(reversed(tuple((mark.key, node) for node in tree.nodes for mark in Mark.read(identity, node).to_list())))


def applied(row: TreeChange, tree: bpy.types.NodeTree, identity: Identity, slot: str, nodes: Mapping[Key, bpy.types.Node]) -> BlenderResult[None]:
    """Writes one change row onto a tree.

    Args:
        row: Row to write.
        tree: Owned tree row names.
        identity: Extension marking owned nodes.
        slot: ID property name holding stored item keys.
        nodes: `owned` result before row phase.

    Returns:
        `Ok` once written. `Error` of `edit` for refused node class or write, `absent` for unresolved node, item, or socket, `choice` for refused enum identifier.
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
        lambda: Ok(bpy.data.node_groups.new(vacant(group.name, bpy.data.node_groups.keys()), group.graph.kind.__name__)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        of=of,
        filled=lambda tree: reconciled(group, tree, identity).map(lambda _: tree),
    )


def stored(content: Content, live: bpy.types.Node, identity: Identity) -> BlenderResult[None]:
    """Writes declared content onto a node.

    Args:
        content: Ramp, curve, image, group, menu, or File Output items to write.
        live: Node caller resource scope holds.
        identity: Extension owning referenced groups, images, and content marks.

    Returns:
        `Ok` once written. `Error` of snapshot, image, group, and RNA faults.
    """

    def ordered(
        items: bpy.types.NodeMenuSwitchItems | bpy.types.NodeCompositorFileOutputItems, names: tuple[str, ...], added: Callable[[str], object], removed: Callable[[str], object]
    ) -> tuple[bpy.types.NodeEnumItem | bpy.types.NodeCompositorFileOutputItem, ...]:
        for name in tuple(item.name for item in items if item.name not in names):
            removed(name)
        for name in [name for name in names if items.get(name) is None]:
            added(name)
        for index, name in enumerate(names):
            items.move(tuple(items).index(items[name]), index)
        return tuple(items)

    match content:
        case Ramp():
            return Snapshot(struct=live.bl_rna.identifier, tree=_tree(content)).apply(live).outcome  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        case CurvePoints():
            mapping: bpy.types.CurveMapping = live.mapping  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Snapshot(struct=live.bl_rna.identifier, tree=_tree(content)).apply(live).outcome.map(lambda _: mapping.reset_view()).map(lambda _: mapping.update())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
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
                .map(lambda switch: ordered(switch.enum_items, tuple(each.name for each in menu.items), switch.enum_items.new, lambda name: switch.enum_items.remove(switch.enum_items[name])))
                .bind(lambda held: accumulated(written(each, "description", item.description) for each, item in zip(held, menu.items)).map(lambda _: None))
            )
        case FileItems() as files:
            kinds = {item.name: item.kind for item in files.items}
            return (
                _narrowed(live, bpy.types.CompositorNodeOutputFile, "file_output_items")
                .map(
                    lambda output: ordered(
                        output.file_output_items,
                        tuple(kinds),
                        lambda name: output.file_output_items.new(kinds[name], name),
                        lambda name: output.file_output_items.remove(output.file_output_items[name]),
                    )
                )
                .bind(lambda held: accumulated(paired(written(each, "socket_type", item.kind), assigned(item.members, each)) for each, item in zip(held, files.items)).map(lambda _: None))
            )


def valued(live: bpy.types.Node, sockets: bpy.types.NodeInputs | bpy.types.NodeOutputs, values: Mapping[str, Value], slot: str) -> BlenderResult[None]:
    """Writes socket defaults by identifier or interface item key."""
    keyed = _keyed(live, slot)
    return accumulated(socket_at(sockets, keyed.get(address, address)).bind(lambda socket: written(socket, "default_value", value)) for address, value in values.items()).map(lambda _: None)


def _tree(content: Ramp | CurvePoints) -> Tree[Value]:
    """Ramp or curve point settings by node RNA path, collections owning items as `Snapshot.capture` reads them."""
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


def _framed(live: bpy.types.Node) -> BlenderResult[bpy.types.ImageUser | bpy.types.CompositorNodeImage]:
    """Frame window holder, compositor image node itself or shader image node `image_user`."""
    match live:
        case bpy.types.CompositorNodeImage():
            return Ok(live)
        case bpy.types.ShaderNodeTexImage() | bpy.types.ShaderNodeTexEnvironment():
            return Ok(live.image_user)
        case _:
            return Error((BlenderAbsentFault(owner=repr(live), key="image_user"),))


def _narrowed[N: bpy.types.Node](live: bpy.types.Node, kind: type[N], member: str) -> BlenderResult[N]:
    """Node as class declaring member, else `absent` fault naming member."""
    return Ok(live) if isinstance(live, kind) else Error((BlenderAbsentFault(owner=repr(live), key=member),))


def _keyed(node: bpy.types.Node, slot: str) -> dict[str, str]:
    """Socket identifiers by item key on group input, output, or group node, by item name on Menu Switch or File Output."""
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
