"""Node-space measurement, graph arrangement, and deferred placement of owned nodes."""

from collections.abc import Callable, Generator, Hashable, Mapping
from contextlib import contextmanager
from functools import reduce
from graphlib import TopologicalSorter
from itertools import accumulate
from operator import sub
from typing import Annotated, Final, Self

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.nodes.trees import tree_of
from blender.bpy.ui.drawing.handlers import hooked
from blender.records.faults import accumulated, Outcome

# --- [CONSTANTS] ------------------------------------------------------------------------

LOCATION: Final = Descriptor.declared(bpy.types.Node, "location_absolute", bpy.types.FloatProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class NodeSize(msgspec.Struct, frozen=True, kw_only=True):
    """Drawn node size in node space, the drawn dimensions over the interface scale."""

    width: float
    height: float

    @classmethod
    def of(cls, node: bpy.types.Node) -> Self:
        """Returns drawn dimensions over `PreferencesSystem.ui_scale`."""
        size, scale = node.dimensions, bpy.context.preferences.system.ui_scale  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        return cls(width=size.x / scale, height=size.y / scale)


class NodeSpacing(msgspec.Struct, frozen=True, kw_only=True):
    """Node-space gaps between columns, rows, and frame bands."""

    across: Annotated[float, msgspec.Meta(ge=0.0, description="Node-space gap between columns")] = 80.0
    down: Annotated[float, msgspec.Meta(ge=0.0, description="Node-space gap between rows")] = 20.0
    band: Annotated[float, msgspec.Meta(ge=0.0, description="Node-space gap between frames")] = 60.0

    def arranged[K: Hashable](
        self, sizes: Mapping[K, NodeSize], links: tuple[tuple[K, int, K], ...], bands: Mapping[K, Option[K]], origin: Callable[[K], tuple[float, float]]
    ) -> dict[K, tuple[float, float]]:
        """Returns node positions by link depth, source socket order, and frame band."""
        if not sizes:
            return {}
        feeds = {key: tuple((source, slot) for source, slot, target in links if target == key) for key in sizes}
        order = tuple(TopologicalSorter({key: tuple(source for source, _ in fed) for key, fed in feeds.items()}).static_order())
        position = {key: index for index, key in enumerate(order)}
        column: dict[K, int] = reduce(lambda held, key: held | {key: max((held[source] + 1 for source, _ in feeds[key]), default=0)}, order, {})
        rank = {key: min(((position[source], slot) for source, slot in feeds[key]), default=(position[key], -1)) for key in order}
        banded, depths = tuple(dict.fromkeys(bands[key] for key in order)), range(max(column.values()) + 1)
        cells = {(band, depth): sorted((key for key in order if (bands[key], column[key]) == (band, depth)), key=rank.__getitem__) for band in banded for depth in depths}
        left, top = origin(order[0])
        lefts = tuple(accumulate((max(sizes[key].width for key in order if column[key] == depth) + self.across for depth in depths), initial=left))
        stacks = {cell: sum(sizes[key].height for key in keys) + self.down * (len(keys) - 1) if keys else 0.0 for cell, keys in cells.items()}
        tops = dict(zip(banded, accumulate((max(stacks[band, depth] for depth in depths) + self.band for band in banded), sub, initial=top)))
        return {key: (lefts[depth], y) for (band, depth), keys in cells.items() for key, y in zip(keys, accumulate((sizes[key].height + self.down for key in keys), sub, initial=tops[band]))}


class Arranger(msgspec.Struct, frozen=True, kw_only=True):
    """Registration row arranging owned nodes on each editor draw."""

    spacing: NodeSpacing = NodeSpacing()

    # --- [MEASUREMENT]

    def owned(self, tree: bpy.types.NodeTree, identity: Identity) -> tuple[tuple[str, tuple[float, float]], ...]:
        """Returns changed owned node positions once every owned node draws."""
        marked = {node.name: node for node in tree.nodes if not isinstance(node, bpy.types.NodeFrame) and Mark.read(identity, node).is_some()}
        sizes = {name: NodeSize.of(node) for name, node in marked.items()}
        slots = {socket.as_pointer(): slot for node in marked.values() for slot, socket in enumerate(node.outputs)}
        links = tuple(
            (link.from_node.name, slots[link.from_socket.as_pointer()], link.to_node.name)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            for link in tree.links
            if link.is_valid and link.from_node != link.to_node and link.from_node.name in marked and link.to_node.name in marked  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        )
        bands = {name: Option.of_optional(node.parent).map(lambda parent: parent.name) for name, node in marked.items()}

        def origin(root: str) -> tuple[float, float]:
            """Returns canvas position of first root."""
            position = marked[root].location_absolute
            return position.x, position.y

        placed = self.spacing.arranged(sizes, links, bands, origin) if all(size.width > 0.0 and size.height > 0.0 for size in sizes.values()) else {}
        return tuple((name, at) for name, at in placed.items() if not LOCATION.equal(LOCATION.read(marked[name]), at))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Registers the node editor draw hook queueing owned node positions."""

        def moved(held: Handle[bpy.types.ID], places: tuple[tuple[str, tuple[float, float]], ...]) -> Outcome[Option[str]]:
            """Writes owned node positions still in tree, with no undo step."""
            desired = dict(places)
            return (
                held
                .resolve()
                .bind(tree_of)
                .bind(
                    lambda tree: accumulated(
                        LOCATION.write(node, desired[node.name]) for node in tree.nodes if node.name in desired and not isinstance(node, bpy.types.NodeFrame) and Mark.read(identity, node).is_some()
                    )
                )
                .map(lambda _: Nothing)
            )

        @contextmanager
        def drawn() -> Generator[Outcome[None]]:
            """Retires pending writes after removing the draw hook."""
            with identity.reports.deferred(moved) as later:

                def drew() -> None:
                    """Queues changed owned node positions of active editable tree."""
                    context = bpy.context
                    match context.space_data, context.window:
                        case bpy.types.SpaceNodeEditor(edit_tree=bpy.types.NodeTree(is_editable=True) as tree), bpy.types.Window() as window if not window.modal_operators:
                            if places := self.owned(tree, identity):
                                for found in holder(tree).to_list():
                                    later(Handle.of(found), places)

                with hooked(bpy.types.SpaceNodeEditor, drew, "WINDOW", "POST_VIEW") as added:
                    yield added

        return Ok(((Band.DRAW_HANDLERS, drawn()),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arranger", "NodeSize", "NodeSpacing"]
