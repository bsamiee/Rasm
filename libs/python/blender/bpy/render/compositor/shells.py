"""The graph builder's context: the record, entry input color, render layer, preview frame, and the group sockets an effect graph reads by their declarations."""

from collections.abc import Callable, Mapping

import bpy
from expression import Nothing, Option
import msgspec
import PyOpenColorIO

from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.graphs import Expr, group_input, menu, Shape, Term
from blender.bpy.nodes.interface import InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.coordinates import framed, FrameSpace
from blender.bpy.render.compositor.inputs import FRAME_ASPECT, FRAME_SCALE
from blender.bpy.render.outputs.framing import Framing
from blender.records.faults import Outcome
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Graph[R: Case] = Callable[[Shell[R]], Outcome[Expr]]

# --- [MODELS] ---------------------------------------------------------------------------


class PreviewFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Stored pixel size of a preview plane and the render framing it stands for."""

    size: tuple[int, int]
    framing: Framing


class Shell[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Graph builder's record, entry input color, render layer, and group sockets by item key, toggled panels reading as their toggles."""

    record: R
    identity: Identity
    config: PyOpenColorIO.Config
    color: Expr
    render: Expr
    sockets: Mapping[Key, Socket]
    preview: Option[PreviewFrame] = Nothing

    def input(self, *path: Key | int | Socket | InterfacePanel) -> Expr:
        """Reads the group input of the declaring socket or toggled panel that ends a key path, a color or vector socket in its shape and every other socket as a value.

        Returns:
            Group input expression of the declaration.

        Raises:
            LookupError: For a path ending in no declaration the group holds, at graph build.
        """
        key = item_key(*(part.key if isinstance(part, Socket | InterfacePanel) else part for part in path))
        match path:
            case (*_, Socket() | InterfacePanel()) if key in self.sockets:
                return read(self.sockets[key], key)
            case _:
                raise LookupError(f"{key} names no input the group declares")

    @property
    def space(self) -> FrameSpace:
        """Camera frame of the entry input."""
        return framed(self.color, self.input(FRAME_ASPECT), self.input(FRAME_SCALE))

    def layer(self, socket: str) -> Expr:
        """Reads one Render Layers output of the effect's pass request layer."""
        return msgspec.structs.replace(self.render, socket=socket)

    def menu(self, *path: Key | int | Socket, cases: Mapping[MenuItem, Term], shape: Option[Shape] = Nothing) -> Expr:
        """Switches between case terms at the menu socket ending a key path, one case per item the socket declares in its order.

        Returns:
            Menu Switch expression over the cases in item order.

        Raises:
            LookupError: For a path ending in no menu socket the group holds, at graph build.
            ValueError: For cases holding other items than the socket declares, at graph build.
        """
        selector = self.input(*path)
        key = item_key(*(part.key if isinstance(part, Socket) else part for part in path))
        match self.sockets[key].items.default_value(None):
            case MenuItems(items=declared) if set(cases) == set(declared):
                return menu(selector, {item: cases[item] for item in declared}, shape=shape)
            case MenuItems(items=declared):
                raise ValueError(f"{key} cases {[item.name for item in cases]} differ from its items {[item.name for item in declared]}")
            case _:
                raise LookupError(f"{key} names no menu socket the group declares")


# --- [OPERATIONS] -----------------------------------------------------------------------


def read(socket: Socket, key: Key = "") -> Expr:
    """Reads the group input of a socket at its key, or at `key` for a socket nested under a prefix, a color or vector socket in its shape and every other socket as a value."""
    base = socket.base
    return group_input(key or socket.key, base if base in {bpy.types.NodeSocketColor, bpy.types.NodeSocketVector} else bpy.types.NodeSocketFloat)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Graph", "PreviewFrame", "Shell", "read"]
