"""Node editor edits: owned group insertion into an edited tree under rollback of links and selection, node-space measurement, and deferred layered placement of owned nodes."""

from collections.abc import Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import reduce
from types import MappingProxyType
from typing import Final

import bpy
from expression import Option
from expression.collections import Block
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _Tier, Acquisitions
from rasm_bpy.data._ids.ownership import _retained, _tracked
from rasm_bpy.data._ids.store import Handle, holder
from rasm_bpy.data._rna.descriptors import Descriptor, Value, written
from rasm_bpy.nodes._addressing import InterfaceKeys, owned
from rasm_bpy.nodes._trees import _created, _endpoints, _group_set, _linked, _links_of, _native, _ordered, _tree_of
from rasm_bpy.nodes.changes import made, NodeGroup, valued
from rasm_bpy.ui._drawing.handlers import draw_handler
from rasm_records import accumulated, Fault, Gaps, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Incoming = bpy.types.NodeLink | tuple[bpy.types.NodeSocket, bool]

# --- [CONSTANTS] ------------------------------------------------------------------------

LOCATION: Final = Descriptor(root=bpy.types.Node, prop=bpy.types.Node.bl_rna.properties["location_absolute"])
_GAPS: Final = Gaps()

# --- [MODELS] ---------------------------------------------------------------------------


class _Arranger(msgspec.Struct, frozen=True, kw_only=True):
    """Registration row arranging owned nodes on each editor draw."""

    gaps: Gaps = Gaps()

    def moves(self, tree: bpy.types.NodeTree, identity: Identity) -> tuple[tuple[str, tuple[float, float]], ...]:
        """Returns changed owned node positions once every owned node draws."""
        marked = _placeable(tree, identity)
        sizes = {name: _measured(node) for name, node in marked.items()}
        slots = {socket.as_pointer(): slot for node in marked.values() for slot, socket in enumerate(node.outputs)}
        links = tuple(
            (source.name, slots[outlet.as_pointer()], target.name)
            for link in tree.links
            if link.is_valid
            for source, outlet, target, _ in (_endpoints(link),)
            if source != target and source.name in marked and target.name in marked
        )
        bands = {name: Option.of_optional(node.parent).map(lambda parent: parent.name) for name, node in marked.items()}

        def origin(root: str) -> tuple[float, float]:
            position = marked[root].location_absolute
            return position.x, position.y

        placed = self.gaps.arranged(sizes, links, bands, origin) if all(width > 0.0 and height > 0.0 for width, height in sizes.values()) else {}
        return tuple((name, at) for name, at in placed.items() if not LOCATION.equal(LOCATION.read(marked[name]), at))

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Registers the node editor draw hook queueing owned node positions."""

        def moved(held: Handle[bpy.types.ID], places: tuple[tuple[str, tuple[float, float]], ...]) -> Outcome[Option[str]]:
            desired = dict(places)
            return (
                held
                .resolve()
                .bind(_tree_of)
                .bind(lambda tree: accumulated(LOCATION.write(node, desired[name]) for name, node in _placeable(tree, identity).items() if name in desired))
                .map(lambda _: Nothing)
            )

        @contextmanager
        def drawn() -> Generator[Outcome[None]]:
            with identity.reports.deferred(moved) as later:

                def drew() -> None:
                    context = bpy.context
                    match context.space_data, context.window:
                        case bpy.types.SpaceNodeEditor(edit_tree=bpy.types.NodeTree(is_editable=True) as tree), bpy.types.Window() as window if not window.modal_operators:
                            if places := self.moves(tree, identity):
                                for found in holder(tree).to_list():
                                    later(Handle.of(found), places)

                with draw_handler(bpy.types.SpaceNodeEditor, drew, "WINDOW", "POST_VIEW") as added:
                    yield added

        return ((_Tier.DRAW_HANDLERS, drawn()),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _measured(node: bpy.types.Node, /) -> tuple[float, float]:
    """Returns drawn node width and height in node space, `Node.dimensions` over `PreferencesSystem.ui_scale`."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # ty: ignore[invalid-assignment]
    size, scale = node.dimensions, preferences.system.ui_scale
    return size.x / scale, size.y / scale


def _inserted(
    tree: bpy.types.NodeTree,
    after: Option[bpy.types.Node],
    cursor: tuple[float, float],
    group: NodeGroup,
    identity: Identity,
    *,
    gaps: Gaps = _GAPS,
    inputs: Mapping[Key, Value] = MappingProxyType({}),
) -> Outcome[bpy.types.Node]:
    """Splices group after a node's first enabled output, routing each consumer link through group, at the cursor when no node leads.

    Args:
        tree: Edited tree (a node editor's `edit_tree`).
        after: Node the group follows (the selected active node).
        cursor: Canvas position the group takes when no node leads.
        group: Owned group to make and place.
        identity: Extension owning the group.
        gaps: Spacing after the leading node.
        inputs: Group node input values by interface item key.

    Returns:
        Placed group node, selected and active. `Error` of make, link, or write faults, every edit rolled back.
    """
    selected, active = tuple((node, node.select) for node in tree.nodes), tree.nodes.active
    saved: set[bpy.types.NodeSocket] = set()
    hidden: set[bpy.types.NodeSocket] = set()
    resources = ExitStack()

    def reselected() -> None:
        for node, state in selected:
            node.select = state
        tree.nodes.active = active

    def linked_into(target: bpy.types.NodeSocket, incoming: tuple[Incoming, ...]) -> Outcome[tuple[bpy.types.NodeLink, ...]]:
        current = _links_of(target)
        for socket in dict.fromkeys((target, *(_endpoints(link)[1] for link in current), *(_endpoints(each)[1] if isinstance(each, bpy.types.NodeLink) else each[0] for each in incoming))):
            if socket not in hidden:
                hidden.add(socket)
                resources.callback(setattr, socket, "hide", socket.hide)
        if target not in saved:
            saved.add(target)
            original = tuple((_endpoints(link)[1], link.is_valid, link.is_muted) for link in current)

            def relinked() -> None:
                for removed in _links_of(target):
                    tree.links.remove(removed)
                for link, valid, muted in (
                    (link, valid, muted) for source, valid, muted in reversed(original) for link in Option.of_optional(tree.links.new(source, target, verify_limits=False)).to_list()
                ):
                    link.is_valid, link.is_muted = valid, muted

            resources.callback(relinked)
        kept = {each for each in incoming if isinstance(each, bpy.types.NodeLink)}
        for link in current:
            if link not in kept:
                tree.links.remove(link)

        def added(
            done: Outcome[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]], request: tuple[int, bpy.types.NodeSocket, bool]
        ) -> Outcome[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]]:
            index, source, muted = request
            return done.bind(
                lambda held: _linked(tree, source, held[0], verify_limits=False, handle_dynamic_sockets=True).bind(
                    lambda link: written(link, ("is_muted",), muted).map(lambda _: (_endpoints(link)[3], {**held[1], index: link}))
                )
            )

        def finished(held: tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]) -> Outcome[tuple[bpy.types.NodeLink, ...]]:
            desired = tuple(each if isinstance(each, bpy.types.NodeLink) else held[1][index] for index, each in enumerate(incoming))
            return _ordered(held[0], desired).map(lambda _: desired)

        requests = tuple((index, *each) for index, each in enumerate(incoming) if not isinstance(each, bpy.types.NodeLink))
        start: Outcome[tuple[bpy.types.NodeSocket, dict[int, bpy.types.NodeLink]]] = Ok((target, {}))
        return reduce(added, reversed(requests), start).bind(finished)

    def replaced(replacements: Mapping[bpy.types.NodeLink, tuple[Incoming, ...]]) -> Outcome[None]:
        targets = tuple(dict.fromkeys(_endpoints(link)[3] for link in replacements))
        return accumulated(linked_into(target, tuple(replacement for link in _links_of(target) for replacement in replacements.get(link, (link,)))) for target in targets).map(lambda _: None)

    def spliced(node: bpy.types.Node, owned: bpy.types.NodeTree) -> Outcome[bpy.types.Node]:
        sources = tuple(socket for active in after.to_list() for socket in tuple(each for each in active.outputs if each.enabled)[:1])
        at = after.map(lambda active: (active.location_absolute.x + active.width + gaps.across, active.location_absolute.y)).default_value(cursor)

        def linked(source: bpy.types.NodeSocket) -> Outcome[None]:
            def socket(sockets: Iterable[bpy.types.NodeSocket]) -> Outcome[bpy.types.NodeSocket]:
                return (
                    Block.of_seq(sockets).filter(lambda each: each.enabled and each.type == source.type).try_head().to_result_with(lambda: Fault.of(Reason.ABSENT, repr(sockets), member=source.type))
                )

            links = tuple(link for link in _links_of(source) if link.is_valid)
            return paired(socket(node.inputs), socket(node.outputs)).bind(
                lambda pair: linked_into(pair[0], ((source, False),)).bind(lambda _: replaced({link: ((pair[1], link.is_muted),) for link in links}))
            )

        def activated(_: object) -> bpy.types.Node:
            tree.nodes.active = node
            return node

        return (
            valued(node.inputs, inputs, InterfaceKeys.of(owned, identity).sockets)
            .bind(lambda _: accumulated(linked(source) for source in sources))
            .bind(lambda _: written(node, ("location_absolute",), at))
            .bind(lambda _: accumulated(written(each, ("select",), each == node) for each in tree.nodes))
            .map(activated)
        )

    with resources:
        resources.callback(reselected)
        return _retained(
            _native(type(tree), bpy.types.ShaderNodeGroup).bind(
                lambda kind: made(group, identity).bind(
                    lambda owned: _tracked(resources, tree.nodes.remove, _created(tree, kind)).bind(lambda node: _group_set(node, owned).bind(lambda _: spliced(node, owned)))
                )
            ),
            resources,
        )


def _placeable(tree: bpy.types.NodeTree, identity: Identity) -> dict[str, bpy.types.Node]:
    """Owned nodes a layout places, frames left to wrap their members, by node name."""
    return {node.name: node for node in owned(tree, identity).values() if not isinstance(node, bpy.types.NodeFrame)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
