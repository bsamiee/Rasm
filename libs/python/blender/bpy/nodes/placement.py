"""Node-space measurement, graph arrangement, and deferred placement of owned nodes."""

from collections.abc import Callable, Generator, Hashable, Mapping
from contextlib import contextmanager, ExitStack
from functools import reduce
from graphlib import TopologicalSorter
from itertools import accumulate
from operator import sub
from typing import Annotated, Self

import bpy
from expression import Nothing, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderRegisterFault, BlenderResult
from blender.bpy.nodes.trees import tree_of
from blender.bpy.ui.screens.areas import Located
from blender.bpy.ui.screens.regions import RegionView
from blender.imaging.faults import accumulated

# --- [MODELS] ---------------------------------------------------------------------------


class NodeSize(msgspec.Struct, frozen=True, kw_only=True):
    """Node's drawn width and height in node space."""

    width: float
    height: float

    @classmethod
    def of(cls, node: bpy.types.Node, view: RegionView) -> Self:
        """Returns drawn dimensions converted by the region's interface scale."""
        size, scale = node.dimensions, view.scale
        return cls(width=size.x / scale, height=size.y / scale)


class NodeSpacing(msgspec.Struct, frozen=True, kw_only=True):
    """Node-space gaps between columns, rows, and frame bands."""

    across: Annotated[float, msgspec.Meta(ge=0.0, description="Node-space gap between columns")] = 80.0
    down: Annotated[float, msgspec.Meta(ge=0.0, description="Node-space gap between rows")] = 20.0
    band: Annotated[float, msgspec.Meta(ge=0.0, description="Node-space gap between frames")] = 60.0

    def arranged[K: Hashable](
        self, sizes: Mapping[K, NodeSize], links: tuple[tuple[K, int, K], ...], bands: Mapping[K, Option[K]], origin: Callable[[K], tuple[float, float]]
    ) -> dict[K, tuple[float, float]]:
        """Returns DAG positions by link depth, source socket order, and frame band."""
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
    """Registration row arranging owned nodes whenever their editor draws."""

    spacing: NodeSpacing = NodeSpacing()

    # --- [MEASUREMENT]

    def owned(self, tree: bpy.types.NodeTree, identity: Identity, view: RegionView, location: Descriptor[bpy.types.FloatProperty]) -> tuple[tuple[str, tuple[float, float]], ...]:
        """Returns changed owned positions after every owned node has drawn dimensions."""
        marked = {node.name: node for node in tree.nodes if not isinstance(node, bpy.types.NodeFrame) and Mark.read(identity, node).is_some()}
        sizes = {name: NodeSize.of(node, view) for name, node in marked.items()}
        slots = {socket.as_pointer(): slot for node in marked.values() for slot, socket in enumerate(node.outputs)}
        links = tuple(
            (link.from_node.name, slots[link.from_socket.as_pointer()], link.to_node.name)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            for link in tree.links
            if link.is_valid and link.from_node != link.to_node and link.from_node.name in marked and link.to_node.name in marked  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        )
        bands = {name: Option.of_optional(node.parent).map(lambda parent: parent.name) for name, node in marked.items()}

        def origin(root: str) -> tuple[float, float]:
            """Returns the first root's native canvas position."""
            position = marked[root].location_absolute
            return position.x, position.y

        placed = self.spacing.arranged(sizes, links, bands, origin) if all(size.width > 0.0 and size.height > 0.0 for size in sizes.values()) else {}
        return tuple((name, at) for name, at in placed.items() if not location.equal(location.read(marked[name]), at))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Resolves canvas placement and acquires its scoped native draw handler."""

        @contextmanager
        def drawn(location: Descriptor[bpy.types.FloatProperty]) -> Generator[BlenderResult[None]]:
            """Retires pending writes before removing the handler that publishes them."""

            def moved(held: Handle[bpy.types.ID], places: tuple[tuple[str, tuple[float, float]], ...]) -> BlenderResult[Option[str]]:
                """Resolves current data and writes surviving owned nodes without an undo step."""
                desired = dict(places)
                return (
                    held
                    .resolve()
                    .bind(tree_of)
                    .bind(
                        lambda tree: accumulated(
                            location.write(node, desired[node.name])
                            for node in tree.nodes
                            if node.name in desired and not isinstance(node, bpy.types.NodeFrame) and Mark.read(identity, node).is_some()
                        )
                    )
                    .map(lambda _: Nothing)
                )

            with ExitStack() as resources, identity.reports.deferred(moved) as later:

                def drew() -> None:
                    """Measures the active editable tree and queues its changed owned positions."""
                    context = bpy.context
                    match context.space_data, context.window, context.area, context.region:
                        case bpy.types.SpaceNodeEditor(
                            edit_tree=bpy.types.NodeTree(is_editable=True) as tree
                        ), bpy.types.Window() as window, bpy.types.Area() as area, bpy.types.Region() as region if not window.modal_operators:
                            if places := self.owned(tree, identity, RegionView(located=Located(window=window, area=area), region=region), location):
                                for found in holder(tree).to_list():
                                    later(Handle.of(found), places)

                region, phase = "WINDOW", "POST_VIEW"
                added = catch(lambda: bpy.types.SpaceNodeEditor.draw_handler_add(drew, (), region, phase), exception=TypeError)().map_error(
                    lambda error: (BlenderRegisterFault(request=(bpy.types.SpaceNodeEditor.__name__, region, phase), error=error),)
                )
                for handle in added.to_option().to_list():
                    resources.callback(bpy.types.SpaceNodeEditor.draw_handler_remove, handle, region)
                yield added.map(lambda _: None)

        return Descriptor.resolve(bpy.types.Node, "location_absolute", bpy.types.FloatProperty).map(lambda location: ((Band.DRAW_HANDLERS, drawn(location)),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arranger", "NodeSize", "NodeSpacing"]
