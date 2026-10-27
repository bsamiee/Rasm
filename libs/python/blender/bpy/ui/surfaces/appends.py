"""Rows appended to stock panels, menus, headers, and context menus."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from functools import partial

import _bpy_types
from bl_ui import UI_MT_button_context_menu, UI_MT_list_item_context_menu
import bpy
from expression import Nothing, Option, Result
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.ui.layout.rows import acquired, painted, placed, Row, RowDraw, SurfaceDraw
from blender.records.faults import Faults, Outcome

# --- [TYPES] ----------------------------------------------------------------------------

type _Extended = bpy.types.Panel | bpy.types.Menu | bpy.types.Header
type _Destination = type[_bpy_types.Panel | _bpy_types.Menu | _bpy_types.Header] | ContextMenu

# --- [MODELS] ---------------------------------------------------------------------------


class ContextMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Context menu showing rows on buttons of one struct type alone."""

    menu: type[UI_MT_button_context_menu | UI_MT_list_item_context_menu]
    hovered: type[Struct]


class Append(msgspec.Struct, frozen=True, kw_only=True):
    """Rows drawn on stock panel, menu, header, or context menu."""

    target: _Destination
    rows: tuple[Row | RowDraw, ...]
    prepend: bool = False
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns row acquisitions and appended draw scope."""
        destination = self.target
        target = destination.menu if isinstance(destination, ContextMenu) else destination

        @contextmanager
        def appended() -> Generator[Outcome[None]]:
            drawn = placed(self.rows, identity).map(painted).map(lambda paint: partial(_shown, self, paint))
            with ExitStack() as scope:
                for draw in drawn.to_option().to_list():
                    (target.prepend if self.prepend else target.append)(draw)  # ty: ignore[invalid-argument-type]
                    scope.callback(target.remove, draw)  # ty: ignore[invalid-argument-type]
                yield drawn.map(lambda _: None)

        return Result[Acquisitions, Faults](ok=(*acquired(self.rows), (Band.APPENDS, appended())))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _shown(append: Append, paint: SurfaceDraw, surface: _Extended, context: bpy.types.Context) -> None:
    """Paints rows on the surface while the condition holds and a context menu's button shows its struct type."""
    destination, separator_after = append.target, append.prepend and not isinstance(append.target, ContextMenu)
    if not all(condition.holds(context) for condition in append.condition.to_list()):
        return
    if (
        isinstance(destination, ContextMenu)
        and catch(getattr, exception=AttributeError)(context, "button_pointer").to_option().filter(lambda pointer: isinstance(pointer, destination.hovered)).is_none()
    ):
        return
    for layout in Option.of_optional(surface.layout).to_list():
        match surface:
            case bpy.types.Panel():
                column = layout.column()
                column.use_property_split = True
                paint(column, context)
            case bpy.types.Menu():
                if not separator_after:
                    layout.separator()
                paint(layout, context)
                if separator_after:
                    layout.separator()
            case bpy.types.Header():
                paint(layout, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Append", "ContextMenu"]
