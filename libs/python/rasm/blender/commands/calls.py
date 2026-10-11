"""Native operator calls and registered or drawn popups."""

from collections.abc import Callable, Mapping
from contextlib import nullcontext
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option
import msgspec

from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.faults import BlenderAbsentFault, BlenderOperatorFault, BlenderResult

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type CalledPopup = MenuPopup | PanelPopup | ShelfPopup
type Popup = CalledPopup | DrawnMenu | DrawnPopover
type Operation = Callable[..., set[rna_enums.OperatorReturnItems]]  # type: ignore[explicit-any]


# --- [MODELS] ---------------------------------------------------------------------------


class MenuPopup(msgspec.Struct, frozen=True, kw_only=True):
    """Registered menu opened as a popup or pie."""

    menu: type[bpy.types.Menu] | Key
    pie: bool = False


class PanelPopup(msgspec.Struct, frozen=True, kw_only=True):
    """Registered panel opened as a popover."""

    panel: type[bpy.types.Panel] | Key
    keep_open: bool = True
    auto_keymap: bool = False


class ShelfPopup(msgspec.Struct, frozen=True, kw_only=True):
    """Registered asset shelf opened as a popover."""

    shelf: type[bpy.types.AssetShelf] | Key


class DrawnMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Popup or pie populated by a layout callback."""

    draw: Callable[[bpy.types.UILayout, bpy.types.Context], None]
    title: str = ""
    icon: "rna_enums.IconItems" = "NONE"
    pie: bool = False


class DrawnPopover(msgspec.Struct, frozen=True, kw_only=True):
    """Popover populated by a layout callback."""

    draw: Callable[[bpy.types.UILayout, bpy.types.Context], None]
    units: int = 0
    anchored: bool = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def called(
    operator: Operation,
    properties: Mapping[str, object],
    /,
    *,
    context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT",
    undo: bool = False,
    override: Option["bpy.types.ContextTempOverride"] = Nothing,
) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
    """Returns the native outcome or absence/refusal, omitting Nothing-valued properties."""
    given = {key: value for key, held in properties.items() for value in (held.to_list() if isinstance(held, Option) else (held,))}
    try:
        with override.default_with(nullcontext):
            answer = frozenset(operator(context, undo, **given))
    except AttributeError:
        return Error((BlenderAbsentFault(owner="bpy.ops", key=repr(operator)),))
    except RuntimeError as error:
        return Error((BlenderOperatorFault(request=repr(operator), error=error),))
    return Ok(answer) if not answer.isdisjoint(("FINISHED", "RUNNING_MODAL", "INTERFACE")) else Error((BlenderOperatorFault(request=repr(operator), error=answer),))


def resolved(popup: CalledPopup, identity: Identity) -> BlenderResult[tuple[Operation, dict[str, str | bool]]]:
    """Returns a registered popup's native operator and properties, or its name refusal."""
    target: type[bpy.types.Menu | bpy.types.Panel | bpy.types.AssetShelf] | Key
    match popup:
        case MenuPopup(menu=menu, pie=pie):
            target, form, operator, values = menu, Name.MENU, bpy.ops.wm.call_menu_pie if pie else bpy.ops.wm.call_menu, {}
        case PanelPopup(panel=panel, keep_open=keep_open, auto_keymap=auto_keymap):
            target, form, operator, values = panel, Name.PANEL, bpy.ops.wm.call_panel, {"keep_open": keep_open, "auto_keymap": auto_keymap}
        case ShelfPopup(shelf=shelf):
            target, form, operator, values = shelf, Name.SHELF, bpy.ops.wm.call_asset_shelf_popover, {}
    named = identity.name(form, target) if isinstance(target, str) else Ok(target.bl_rna.identifier)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return named.map(lambda identifier: (operator, {"name": identifier, **values}))


def opened(popup: Popup, context: bpy.types.Context, event: bpy.types.Event, identity: Identity) -> BlenderResult[None]:
    """Returns success after opening a popup on its invoking window, or the native refusal."""
    manager = context.window_manager
    match popup:
        case MenuPopup() | PanelPopup() | ShelfPopup():
            return resolved(popup, identity).bind(lambda pair: called(*pair, context="INVOKE_DEFAULT")).map(lambda _: None)
        case DrawnMenu(draw=draw, title=title, icon=icon, pie=True):
            manager.popup_menu_pie(event, lambda holder, held: draw(holder.layout, held), title=title, icon=icon)  # type: ignore[union-attr, arg-type]  # ty: ignore[unresolved-attribute, invalid-argument-type]
        case DrawnMenu(draw=draw, title=title, icon=icon):
            manager.popup_menu(lambda holder, held: draw(holder.layout, held), title=title, icon=icon)  # type: ignore[union-attr, arg-type]  # ty: ignore[unresolved-attribute, invalid-argument-type]
        case DrawnPopover(draw=draw, units=units, anchored=anchored):
            manager.popover(lambda holder, held: draw(holder.layout, held), ui_units_x=units, from_active_button=anchored)  # type: ignore[union-attr, arg-type]  # ty: ignore[unresolved-attribute, invalid-argument-type]
    return Ok(None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CalledPopup", "DrawnMenu", "DrawnPopover", "MenuPopup", "Operation", "PanelPopup", "Popup", "ShelfPopup", "called", "opened", "resolved"]
