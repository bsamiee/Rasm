"""Menus of ordered rows, submenus, or pie slots, preset menus, editor headers, and content appended to stock panels, menus, headers, and context menus, each refusing the sections whose layout panels the host refuses there."""

from collections.abc import Callable, Generator, Iterable
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import Enum
from functools import partial
from typing import TYPE_CHECKING

import _bpy_types
from bl_ui import UI_MT_button_context_menu, UI_MT_list_item_context_menu
import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Called, Command, idname, Opened, Operation, Placement
from blender.bpy.data.rna.conditions import Condition, polled
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.layout.members import admits, Member
from blender.bpy.ui.layout.presets import Captured, Listing
from blender.bpy.ui.layout.rows import acquired, layout_method, Line, Opens, OperatorButton, Property, realized, RowContent, RowDraw, surfaced, SurfaceDraw
from blender.bpy.ui.layout.sections import reached
from blender.bpy.ui.layout.values import Store
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Slot = OperatorButton | Opens | Property | RowDraw
type Hovered = type[Struct] | Member | Command | Operation
type _Extended = bpy.types.Panel | bpy.types.Menu | bpy.types.Header
type _Pointed = Callable[[bpy.types.Context], bool]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [MENUS]


class Dropdown(msgspec.Struct, frozen=True, kw_only=True):
    """Entries in declaration order, or in the order `order` reads from the context as entry indices, an index past the entries skipped."""

    entries: tuple[RowContent, ...]
    order: Option[Callable[[bpy.types.Context], Iterable[int]]] = Nothing
    search: bool = False


class Submenus[G: Enum](msgspec.Struct, frozen=True, kw_only=True):
    """Entries filed into one submenu per group in group declaration order, the native add-menu form."""

    entries: tuple[tuple[G, RowContent], ...]
    search: bool = False


class Pie(msgspec.Struct, frozen=True, kw_only=True):
    """Pie slots in Blender's radial fill order, an empty slot kept free."""

    west: Option[Slot] = Nothing
    east: Option[Slot] = Nothing
    south: Option[Slot] = Nothing
    north: Option[Slot] = Nothing
    north_west: Option[Slot] = Nothing
    north_east: Option[Slot] = Nothing
    south_west: Option[Slot] = Nothing
    south_east: Option[Slot] = Nothing


class PieRows(msgspec.Struct, frozen=True, kw_only=True):
    """Entries filling pie slots in Blender's radial fill order, an expanded enum property or an `operator_enum` button taking one slot per item."""

    entries: tuple[Slot, ...]


class Menu(msgspec.Struct, frozen=True, kw_only=True):
    """Extension menu with poll condition."""

    key: Key
    title: str
    description: str
    body: Dropdown | Submenus[Enum] | Pie | PieRows
    condition: Option[Condition] = Nothing

    # --- [PLACEMENT]

    @property
    def popup(self) -> Placement:
        """Placement buttons and keymaps open the menu through, as a pie for a pie body."""
        return Placement(Opened(self.key, Called.PIE if isinstance(self.body, Pie | PieRows) else Called.MENU))

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns submenus, row acquisitions, then the menu class."""
        match self.body:
            case Dropdown(entries=rows, order=order, search=search):
                submenus: tuple[Menu, ...] = ()
                drawing = order.map(lambda ordered: partial(_ordered_drawing, identity, rows, ordered)).default_value(partial(surfaced, rows, identity))
            case Submenus(entries=entries, search=search):
                present = frozenset(held for held, _ in entries)
                submenus = tuple(
                    Menu(
                        key=f"{self.key}_{group.name.lower()}",
                        title=bpy.path.display_name(group.name.lower()),
                        description=f"{bpy.path.display_name(group.name.lower())}: {self.description}",
                        body=Dropdown(entries=tuple(row for held, row in entries if held is group)),
                        condition=self.condition,
                    )
                    for kind in dict.fromkeys(type(held) for held, _ in entries)
                    for group in kind
                    if group in present
                )
                rows = tuple(Opens(target=Opened(group.key, Called.MENU)) for group in submenus)
                drawing = partial(surfaced, rows, identity)
            case Pie():
                slots, search, submenus = msgspec.structs.astuple(self.body), False, ()
                rows = tuple(row for slot in slots for row in slot.to_list())
                drawing = partial(_radial_drawing, identity, slots)
            case PieRows(entries=rows):
                search, submenus = False, ()
                drawing = partial(_radial_drawing, identity, tuple(map(Some, rows)))
        return Ok((*submenus, *acquired(rows), (Band.INTERFACE, _registered(identity, self.key, self.title, self.description, rows, drawing, search=search, condition=self.condition))))


class PresetMenu[T: msgspec.Struct = Snapshot](msgspec.Struct, frozen=True, kw_only=True):
    """Searchable preset listing with trailing rows."""

    source: Store[bpy.types.PropertyGroup] | Listing[T] | Captured
    entries: tuple[RowContent, ...] = ()

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns source acquisitions, then the preset menu."""

        @contextmanager
        def stored(store: Store[bpy.types.PropertyGroup]) -> Generator[Outcome[None]]:
            """Registers the preset menu of the store's listing for the scope, nothing while the store holds no presets."""
            with ExitStack() as stack:
                yield store.listing(identity).bind(lambda found: found.map(lambda listing: stack.enter_context(_preset_menu(identity, self.entries, listing))).default_value(Ok(None)))

        match self.source:
            case Store() as store:
                return Ok((store, *acquired(self.entries), (Band.INTERFACE, stored(store))))
            case Listing() as listing:
                return _listed(identity, self.entries, listing)
            case Captured() as captured:
                return captured.listing().bind(partial(_listed, identity, self.entries))


# --- [HEADERS]


class Header(msgspec.Struct, frozen=True, kw_only=True):
    """Extension header drawn after the stock headers of an editor's header region, with poll condition, `menus` drawn ahead of the rows inline or folded by `area.show_menus`."""

    key: Key
    space: "rna_enums.SpaceTypeItems"
    rows: tuple[RowContent, ...]
    menus: Option[Menu] = Nothing
    region: "rna_enums.RegionTypeItems" = "HEADER"
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the editor menus, row acquisitions, then the header class."""
        return Ok((*self.menus.to_list(), *acquired(self.rows), (Band.INTERFACE, _header(self, identity))))


# --- [EXTENSIONS]


class ContextMenu(msgspec.Struct, frozen=True, kw_only=True):
    """Button or list row context menu showing content only over the hovered button a struct type, a bound member, or an operator names."""

    menu: type[UI_MT_button_context_menu | UI_MT_list_item_context_menu]
    hovered: Hovered

    def pointing(self, identity: Identity) -> Outcome[_Pointed]:
        """Resolves the hovered button test once, a member's property and an operator's type definition read at realization."""
        match self.hovered:
            case Member() as member:
                return member.resolved().map(lambda held: partial(_on_property, held.descriptor.prop))
            case type() as kind:
                return Ok(partial(_on_struct, kind))
            case operator:
                return idname(identity, operator).bind(
                    lambda dotted: (
                        Result[tuple[str, str, str], Faults]
                        .Ok(dotted.partition("."))
                        .filter_with(lambda parts: parts[2] in dir(getattr(bpy.ops, parts[0])), lambda _: Fault.of(BlenderReason.ABSENT, "bpy.ops", member=dotted))
                        .map(lambda parts: partial(_on_operator, f"{parts[0].upper()}_OT_{parts[2]}"))
                    )
                )


class Append(msgspec.Struct, frozen=True, kw_only=True):
    """Layout content drawn after a stock panel, menu, header, or context menu, or before it when `prepend` is set, while the condition holds.

    Appended content inherits the layout the stock draw leaves, its property split included, prepended content draws ahead of it, and menu content keeps a separator from the stock entries.
    """

    target: type[_bpy_types.Panel | _bpy_types.Menu | _bpy_types.Header] | ContextMenu
    rows: tuple[RowContent, ...]
    prepend: bool = False
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns what the content holds, then the draw function held on the target for the scope."""
        return Ok((*acquired(self.rows), (Band.APPENDS, _extended(self, identity))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REGISTRATION]


@contextmanager
def _registered(
    identity: Identity, key: Key, title: str, description: str, rows: tuple[RowContent, ...], drawing: Callable[[], Outcome[SurfaceDraw]], *, search: bool, condition: Option[Condition] = Nothing
) -> Generator[Outcome[None]]:
    """Registers the menu class once its rows realize, refusing a section among them, a searchable menu opened from code or a key leading with its search entry."""

    def built(draw: SurfaceDraw, name: str, heading: Option[RowDraw]) -> type[bpy.types.Menu]:
        options = {"SEARCH_ON_KEY_PRESS"} if search else set()
        native = {"bl_idname": name, "bl_label": title, "bl_description": description, "bl_options": options, "draw": layout_method(partial(_headed, heading, draw)), **polled(condition)}
        return type(name, (bpy.types.Menu,), native)

    def searched(name: str) -> Outcome[Option[RowDraw]]:
        entry = OperatorButton(target=Placement(bpy.ops.wm.search_single_menu, {"menu_idname": name}), keywords={"text": "Search..."}, icon=Some("VIEWZOOM"))
        return realized((entry,), identity).map(Some) if search else Ok(Nothing)

    with ExitStack() as stack:
        yield (
            _unpaneled(key, rows)
            .bind(lambda _: drawing())
            .bind(lambda draw: identity.name(bpy.types.Menu, key).bind(lambda name: searched(name).map(lambda heading: built(draw, name, heading))))
            .bind(lambda native: entered(stack, (native,)))
        )


@contextmanager
def _header(header: Header, identity: Identity) -> Generator[Outcome[None]]:
    """Registers the header class for the scope once its menus name and its rows realize."""

    def built(name: str, menus: Option[str], paint: SurfaceDraw) -> type[bpy.types.Header]:
        drawn = menus.map(lambda menu: partial(_collapsed, menu, paint)).default_value(paint)
        return type(name, (bpy.types.Header,), {"bl_idname": name, "bl_space_type": header.space, "bl_region_type": header.region, "draw": layout_method(drawn), **polled(header.condition)})

    menus = header.menus.map(lambda menu: identity.name(bpy.types.Menu, menu.key).map(Some)).default_value(Ok(Nothing))
    with ExitStack() as stack:
        yield (
            _unpaneled(header.key, header.rows)
            .bind(lambda _: identity.name(bpy.types.Header, header.key))
            .bind(lambda name: menus.bind(lambda named: surfaced(header.rows, identity).map(partial(built, name, named))))
            .bind(lambda native: entered(stack, (native,)))
        )


@contextmanager
def _extended(append: Append, identity: Identity) -> Generator[Outcome[None]]:
    """Adds the draw function to the target for the scope once the content and hovered test realize."""
    target = append.target.menu if isinstance(append.target, ContextMenu) else append.target
    pointed: Outcome[Option[_Pointed]] = append.target.pointing(identity).map(Some) if isinstance(append.target, ContextMenu) else Ok(Nothing)
    drawn = pointed.bind(lambda test: surfaced(append.rows, identity).map(lambda paint: partial(_shown, append, paint, test)))
    with ExitStack() as stack:
        for draw in drawn.to_option().to_list():
            (target.prepend if append.prepend else target.append)(draw)  # ty: ignore[invalid-argument-type]
            stack.callback(target.remove, draw)  # ty: ignore[invalid-argument-type]
        yield drawn.map(lambda _: None)


def _listed[S: msgspec.Struct](identity: Identity, entries: tuple[RowContent, ...], listing: Listing[S]) -> Outcome[Acquisitions]:
    """Returns the listing and trailing rows, then the preset menu over them."""
    return Ok((*acquired((listing, *entries)), (Band.INTERFACE, _preset_menu(identity, entries, listing))))


def _preset_menu[S: msgspec.Struct](identity: Identity, entries: tuple[RowContent, ...], listing: Listing[S]) -> AbstractContextManager[Outcome[None]]:
    """Registers the searchable preset menu titled after the listing's preset."""
    name = bpy.path.display_name(listing.preset.key)
    owned = (listing, *entries)
    return _registered(identity, listing.key, f"{name} Presets", f"Apply, add, or import {name.lower()} presets", owned, partial(surfaced, owned, identity), search=True)


def _unpaneled(key: Key, rows: tuple[RowContent, ...]) -> Outcome[None]:
    """Refuses rows holding a section, whose layout panel the host refuses in menus, pies, and headers."""
    return (
        Ok(reached(rows))
        .filter_with(lambda found: not found, lambda found: Fault.of(RecordReason.OUTSIDE, key, ", ".join(section.key for section in found), member="rows without layout panels"))
        .map(lambda _: None)
    )


# --- [DRAWING]


def _headed(heading: Option[RowDraw], draw: SurfaceDraw, layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
    """Leads with the search entry where the opener sets the `EXEC_REGION_WIN` context of code and keymap calls, as stock add menus do, then draws the entries."""
    for entry in heading.to_list() if layout.operator_context == "EXEC_REGION_WIN" else ():
        entry(layout, context, context)
        layout.separator()
    draw(layout, context)


def _collapsed(menu: str, paint: SurfaceDraw, layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
    """Draws the editor menus through the host's collapsible header form, then the rows."""
    if isinstance(kind := getattr(bpy.types, menu, None), type) and issubclass(kind, bpy.types.Menu):
        kind.draw_collapsible(context, layout)
    paint(layout, context)


def _ordered_drawing(identity: Identity, rows: tuple[RowContent, ...], order: Callable[[bpy.types.Context], Iterable[int]]) -> Outcome[SurfaceDraw]:
    """Realizes each entry into a draw the context's order picks by index."""
    return accumulated(realized((row,), identity) for row in rows).map(lambda draws: partial(_picked, draws, order))


def _picked(draws: tuple[RowDraw, ...], order: Callable[[bpy.types.Context], Iterable[int]], layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
    """Draws the entries the context's order names, skipping an index past them."""
    for index in order(context):
        for draw in draws[index : index + 1] if index >= 0 else ():
            draw(layout, context, context)


def _radial_drawing(identity: Identity, slots: tuple[Option[Slot], ...]) -> Outcome[SurfaceDraw]:
    """Realizes each slot, a property slot holding a boolean or enum, into the pie draw, a button slot drawing as the pie's own item and a plain draw in a column taking one slot."""

    def slot(row: Slot) -> Outcome[tuple[RowDraw, bool]]:
        checked = row.target.resolved().bind(lambda held: held.accepts((bpy.types.BoolProperty, bpy.types.EnumProperty))).map(lambda _: None) if isinstance(row, Property) else Ok(None)
        return checked.bind(lambda _: realized((row,), identity)).map(lambda draw: (draw, isinstance(row, Line)))

    return accumulated(each.map(lambda row: slot(row).map(Some)).default_with(lambda: Ok(Nothing)) for each in slots).map(lambda draws: partial(_radial, draws))


def _radial(draws: tuple[Option[tuple[RowDraw, bool]], ...], layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
    """Draws each slot into the pie, a separator keeping an empty slot free."""
    pie = layout.menu_pie()
    for drawn in draws:
        drawn.map(lambda slot: slot[0](pie if slot[1] else pie.column(), context, context)).default_with(pie.separator)


def _shown(append: Append, paint: SurfaceDraw, pointed: Option[_Pointed], surface: _Extended, context: bpy.types.Context) -> None:
    """Paints the content while the condition holds and a context menu's hovered button passes its test."""
    if not (pointed.map(lambda test: test(context)).default_value(True) and admits(append.condition, context)):
        return
    layout: bpy.types.UILayout = surface.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    match surface, append.prepend:
        case bpy.types.Menu(), False:
            layout.separator()
            paint(layout, context)
        case bpy.types.Menu(), True:
            paint(layout, context)
            layout.separator()
        case _:
            paint(layout, context)


# --- [HOVERED]


def _on_struct(kind: type, context: bpy.types.Context) -> bool:
    """Whether the hovered button's data is of the struct type."""
    return catch(getattr, exception=AttributeError)(context, "button_pointer").to_option().filter(lambda pointer: isinstance(pointer, kind)).is_some()


def _on_property(prop: bpy.types.Property, context: bpy.types.Context) -> bool:
    """Whether the hovered button draws the property."""
    return catch(getattr, exception=AttributeError)(context, "button_prop").to_option().filter(lambda held: held == prop).is_some()


def _on_operator(name: str, context: bpy.types.Context) -> bool:
    """Whether the hovered button runs the operator its type definition names."""
    return catch(getattr, exception=AttributeError)(context, "button_operator").to_option().filter(lambda held: definition(held).identifier == name).is_some()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Append", "ContextMenu", "Dropdown", "Header", "Hovered", "Menu", "Pie", "PieRows", "PresetMenu", "Slot", "Submenus"]
