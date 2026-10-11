"""Scoped native panel, menu, header, and hovered-button draw additions."""

from collections.abc import Callable, Generator
from contextlib import contextmanager

from bl_ui import UI_MT_button_context_menu, UI_MT_list_item_context_menu
import bpy
from expression import Nothing, Option, Result
from expression.extra.result import catch
import msgspec

from rasm.blender.data.rna.conditions import Condition, holds
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.registration import Acquisitions, Band
from rasm.blender.faults import BlenderFaults, BlenderResult
from rasm.blender.ui.layout.rows import acquired, painted, placed, Row, RowDraw, SurfaceDraw

# --- [TYPES] ----------------------------------------------------------------------------

type Extended = bpy.types.Panel | bpy.types.Menu | bpy.types.Header
type Destination = type[Extended] | ContextMenu

# --- [MODELS] ---------------------------------------------------------------------------


class ContextMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Native context menu restricted to buttons bound to one struct type."""

    menu: type[UI_MT_button_context_menu | UI_MT_list_item_context_menu]
    hovered: "type[bpy.types.bpy_struct[object]]"


class Append(msgspec.Struct, frozen=True, kw_only=True):
    """Rows added to a native surface for the extension's registration scope."""

    target: Destination
    rows: tuple[Row | RowDraw, ...]
    prepend: bool = False
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains nested declarations and builds the scoped native draw manager."""
        destination = self.target
        target = destination.menu if isinstance(destination, ContextMenu) else destination
        separator_after = self.prepend and not isinstance(destination, ContextMenu)

        def drawn(paint: SurfaceDraw) -> Callable[[Extended, bpy.types.Context], None]:
            def shown(surface: Extended, context: bpy.types.Context) -> None:
                if not all(holds(condition, context) for condition in self.condition.to_list()):
                    return
                if (
                    isinstance(destination, ContextMenu)
                    and catch(lambda: context.path_resolve("button_pointer"), exception=ValueError)().to_option().filter(lambda pointer: isinstance(pointer, destination.hovered)).is_none()
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

            return shown

        @contextmanager
        def appended() -> Generator[BlenderResult[None]]:
            result = placed(self.rows, identity).map(painted).map(drawn)
            match result.tag:
                case "error":
                    yield Result[None, BlenderFaults].Error(result.error)
                case "ok":
                    draw = result.ok
                    (target.prepend if self.prepend else target.append)(draw)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    try:
                        yield Result[None, BlenderFaults].Ok(None)
                    finally:
                        target.remove(draw)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        return Result[Acquisitions, BlenderFaults](ok=(*acquired(self.rows), (Band.APPENDS, appended())))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Append", "ContextMenu", "Destination", "Extended"]
