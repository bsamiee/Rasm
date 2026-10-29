"""Node-space measurement and deferred layered placement of owned nodes."""

from collections.abc import Generator
from contextlib import contextmanager
from typing import Final

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.data.ids.content import endpoints
from blender.bpy.data.ids.store import Handle, holder
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.trees import tree_of
from blender.bpy.ui.drawing.handlers import hooked
from blender.records.faults import accumulated, Outcome
from blender.records.trees import Gaps, Size

# --- [CONSTANTS] ------------------------------------------------------------------------

LOCATION: Final = Descriptor.declared(bpy.types.Node, ("location_absolute",), bpy.types.FloatProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Arranger(msgspec.Struct, frozen=True, kw_only=True):
    """Registration row arranging owned nodes on each editor draw."""

    gaps: Gaps = Gaps()

    # --- [MEASUREMENT]

    def owned(self, tree: bpy.types.NodeTree, identity: Identity) -> tuple[tuple[str, tuple[float, float]], ...]:
        """Returns changed owned node positions once every owned node draws."""
        marked = _arranged(tree, identity)
        sizes = {name: measured(node) for name, node in marked.items()}
        slots = {socket.as_pointer(): slot for node in marked.values() for slot, socket in enumerate(node.outputs)}
        links = tuple(
            (source.name, slots[outlet.as_pointer()], target.name)
            for link in tree.links
            if link.is_valid
            for source, outlet, target, _ in (endpoints(link),)
            if source != target and source.name in marked and target.name in marked
        )
        bands = {name: Option.of_optional(node.parent).map(lambda parent: parent.name) for name, node in marked.items()}

        def origin(root: str) -> tuple[float, float]:
            """Returns canvas position of first root."""
            position = marked[root].location_absolute
            return position.x, position.y

        placed = self.gaps.arranged(sizes, links, bands, origin) if all(size.width > 0.0 and size.height > 0.0 for size in sizes.values()) else {}
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
                .bind(lambda tree: accumulated(LOCATION.write(node, desired[name]) for name, node in _arranged(tree, identity).items() if name in desired))
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


# --- [OPERATIONS] -----------------------------------------------------------------------


def measured(node: bpy.types.Node) -> Size:
    """Returns drawn node size in node space, `Node.dimensions` over `PreferencesSystem.ui_scale`."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    size, scale = node.dimensions, preferences.system.ui_scale
    return Size(width=size.x / scale, height=size.y / scale)


def _arranged(tree: bpy.types.NodeTree, identity: Identity) -> dict[str, bpy.types.Node]:
    """Owned nodes a layout places, frames left to wrap their members, by node name."""
    return {node.name: node for node in owned(tree, identity).values() if not isinstance(node, bpy.types.NodeFrame)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Arranger", "measured"]
