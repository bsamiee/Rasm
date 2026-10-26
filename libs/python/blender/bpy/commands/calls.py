"""Operator calls, operator placements with property writers, and registered or drawn popups."""

from abc import abstractmethod
from collections.abc import Callable, Mapping
from contextlib import nullcontext
from functools import partial
from typing import override, Protocol, TYPE_CHECKING

import _bpy_types
import bpy
from expression import Error, Nothing, Ok, Option
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.faults import BlenderAbsentFault, BlenderOperatorFault, BlenderResult
from blender.imaging.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Popup = CalledPopup | DrawnMenu | DrawnPopover
type Operation = Callable[..., set[rna_enums.OperatorReturnItems]]  # type: ignore[explicit-any]
type Writer = Callable[[bpy.types.OperatorProperties], BlenderResult[None]]


class Placed(Protocol):
    """Operator placement with property writer."""

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns operator name and writer."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class Stock(msgspec.Struct, frozen=True, kw_only=True):
    """Placement of stock operator with RNA property values."""

    operator: Operation
    values: Mapping[str, Value] = msgspec.field(default_factory=dict)

    def invocation(self, _identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Resolves stock operator name and property writer."""
        identifier = self.operator.idname_py()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def member(kind: bpy.types.Struct, key: str) -> BlenderResult[Descriptor]:
            return (
                Option
                .of_optional(kind.properties.get(key))
                .map(lambda prop: Descriptor(root=bpy.types.OperatorProperties, owner="", prop=prop))
                .to_result_with(lambda: (BlenderAbsentFault(owner=identifier, key=key),))
            )

        try:
            kind = self.operator.get_rna_type()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        except KeyError:
            return Error((BlenderAbsentFault(owner="bpy.ops", key=identifier),))
        return accumulated(member(kind, key) for key in self.values).map(
            lambda members: (identifier, lambda properties: accumulated(descriptor.write(properties, self.values[descriptor.prop.identifier]) for descriptor in members).map(lambda _: None))
        )


class CalledPopup(msgspec.Struct, frozen=True, kw_only=True):
    """Registered menu, panel, or shelf the window manager opens through its call operator."""

    @abstractmethod
    def stock(self, identity: Identity, /) -> BlenderResult[Stock]:
        """Returns the window manager call operator with its properties."""
        ...

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns call operator name and property writer."""
        return self.stock(identity).bind(lambda stock: stock.invocation(identity))


class MenuPopup(CalledPopup, frozen=True, kw_only=True):
    """Registered menu opened as popup or pie."""

    menu: type[_bpy_types.Menu] | Key
    pie: bool = False

    @override
    def stock(self, identity: Identity, /) -> BlenderResult[Stock]:
        """Returns `wm.call_menu_pie` or `wm.call_menu` over the menu idname."""
        return idname(identity, Name.MENU, self.menu).map(lambda name: Stock(operator=bpy.ops.wm.call_menu_pie if self.pie else bpy.ops.wm.call_menu, values={"name": name}))


class PanelPopup(CalledPopup, frozen=True, kw_only=True):
    """Registered panel opened as popover."""

    panel: type[_bpy_types.Panel] | Key
    keep_open: bool = True
    auto_keymap: bool = False

    @override
    def stock(self, identity: Identity, /) -> BlenderResult[Stock]:
        """Returns `wm.call_panel` over the panel idname."""
        return idname(identity, Name.PANEL, self.panel).map(lambda name: Stock(operator=bpy.ops.wm.call_panel, values={"name": name, "keep_open": self.keep_open, "auto_keymap": self.auto_keymap}))


class ShelfPopup(CalledPopup, frozen=True, kw_only=True):
    """Registered asset shelf opened as popover."""

    shelf: type[_bpy_types.AssetShelf] | Key

    @override
    def stock(self, identity: Identity, /) -> BlenderResult[Stock]:
        """Returns `wm.call_asset_shelf_popover` over the shelf idname."""
        return idname(identity, Name.SHELF, self.shelf).map(lambda name: Stock(operator=bpy.ops.wm.call_asset_shelf_popover, values={"name": name}))


class DrawnMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Popup or pie a callback draws."""

    draw: Callable[[bpy.types.UILayout, bpy.types.Context], None]
    title: str = ""
    icon: "rna_enums.IconItems" = "NONE"
    pie: bool = False


class DrawnPopover(msgspec.Struct, frozen=True, kw_only=True):
    """Popover a callback draws."""

    draw: Callable[[bpy.types.UILayout, bpy.types.Context], None]
    units: int = 0
    anchored: bool = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def given[V](properties: Mapping[str, V | Option[V]]) -> dict[str, V]:
    """Keywords with Option values unwrapped and Nothing-valued entries left out."""
    return {key: value for key, held in properties.items() for value in (held.to_list() if isinstance(held, Option) else (held,))}


def idname(identity: Identity, form: Name, target: type[_bpy_types.Panel | _bpy_types.Menu | _bpy_types.AssetShelf] | Key) -> BlenderResult[str]:
    """Resolves an extension key under its host name form, or a stock class's RNA identifier."""
    return identity.name(form, target) if isinstance(target, str) else Ok(target.bl_rna.identifier)


def called(
    operator: Operation,
    properties: Mapping[str, object],
    /,
    *,
    context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT",
    undo: bool = False,
    override: Option["bpy.types.ContextTempOverride"] = Nothing,
) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
    """Calls operator, leaving Nothing-valued properties unset."""
    keywords = given(properties)
    identifier = operator.idname_py()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    request = f"bpy.ops.{identifier}({', '.join(f'{key}={value!r}' for key, value in keywords.items())})"
    try:
        with override.default_with(nullcontext):
            answer = frozenset(operator(context, undo, **keywords))
    except AttributeError:
        return Error((BlenderAbsentFault(owner="bpy.ops", key=identifier),))
    except RuntimeError as error:
        return Error((BlenderOperatorFault(request=request, error=error),))
    return Ok(answer) if not answer.isdisjoint(("FINISHED", "RUNNING_MODAL", "INTERFACE")) else Error((BlenderOperatorFault(request=request, error=answer),))


def opened(popup: Popup, context: bpy.types.Context, event: bpy.types.Event, identity: Identity) -> BlenderResult[None]:
    """Opens popup on invoking window."""

    def painted(draw: Callable[[bpy.types.UILayout, bpy.types.Context], None], holder: bpy.types.UIPopupMenu | bpy.types.UIPieMenu | bpy.types.UIPopover, held: bpy.types.Context) -> None:
        for layout in Option.of_optional(holder.layout).to_list():
            draw(layout, held)

    match popup, context.window_manager:
        case CalledPopup() as registered, _:
            return registered.stock(identity).bind(lambda stock: called(stock.operator, stock.values, context="INVOKE_DEFAULT")).map(lambda _: None)
        case DrawnMenu(draw=draw, title=title, icon=icon, pie=True), bpy.types.WindowManager() as manager:
            manager.popup_menu_pie(event, partial(painted, draw), title=title, icon=icon)
        case DrawnMenu(draw=draw, title=title, icon=icon), bpy.types.WindowManager() as manager:
            manager.popup_menu(partial(painted, draw), title=title, icon=icon)
        case DrawnPopover(draw=draw, units=units, anchored=anchored), bpy.types.WindowManager() as manager:
            manager.popover(partial(painted, draw), ui_units_x=units, from_active_button=anchored)
        case _:
            return Error((BlenderAbsentFault(owner=repr(context), key="window_manager"),))
    return Ok(None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CalledPopup", "DrawnMenu", "DrawnPopover", "MenuPopup", "Operation", "PanelPopup", "Placed", "Popup", "ShelfPopup", "Stock", "Writer", "called", "given", "idname", "opened"]
